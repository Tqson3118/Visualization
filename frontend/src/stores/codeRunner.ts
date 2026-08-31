import { computed, ref } from 'vue';
import { defineStore } from 'pinia';

import * as codeRunnerApi from '@/api/codeRunner';
import type { CodeRunSummary, CodeSubmitResult } from '@/api/codeRunner';
import { getCatalogMeta } from '@/engines/catalog';
import { runCode } from '@/engines/core/stepExecutor';
import type { RunResult } from '@/engines/core/stepExecutor';
import { useSimulationStore } from '@/stores/simulation';

/** Store codeRunner theo SDD §3.2 — Module I (Code Runner, ADR-012: sandbox Web Worker client) */
export type RunState = 'idle' | 'running' | 'passed' | 'failed' | 'error';

/** Code mẫu nạp sẵn theo key (chạy được trong sandbox — dùng compare/swap/array). */
const TEMPLATES: Record<string, string> = {
  'sort.bubble': `function bubbleSort(a) {
  const n = a.length;
  for (let i = 0; i < n - 1; i++) {
    let swapped = false;
    for (let j = 0; j < n - i - 1; j++) {
      compare(j, j + 1);
      if (a[j] > a[j + 1]) {
        swap(j, j + 1);
        swapped = true;
      }
    }
    if (!swapped) break;
  }
}
bubbleSort(array);`,
  'search.binary': `function binarySearch(a, target) {
  let lo = 0, hi = a.length - 1;
  while (lo <= hi) {
    const mid = Math.floor((lo + hi) / 2);
    compare(mid, 0);
    if (a[mid] === target) return mid;
    if (a[mid] < target) lo = mid + 1;
    else hi = mid - 1;
  }
  return -1;
}
binarySearch(array, 42);`,
  'graph.bfs': `// Duyệt BFS — đồ thị dạng danh sách kề
const adj = [[1, 2], [0, 3], [0, 4], [1], [2]];
const visited = new Array(adj.length).fill(false);
const queue = [0];
visited[0] = true;
while (queue.length > 0) {
  const u = queue.shift();
  visit(u);
  for (const v of adj[u]) {
    if (!visited[v]) {
      visited[v] = true;
      enqueue(v);
      queue.push(v);
    }
  }
}`,
};

export const useCodeRunnerStore = defineStore('codeRunner', () => {
  const editorCode = ref('');
  const runState = ref<RunState>('idle');
  const lastRun = ref<CodeRunSummary | null>(null);
  const submissions = ref<CodeRunSummary[]>([]);
  const lastOutput = ref<unknown>(null);
  const lastStats = ref<{ comparisons: number; swaps: number; writes: number; durationMs: number } | null>(null);
  const runError = ref<string | null>(null);

  const isRunning = computed(() => runState.value === 'running');
  const key = ref('');

  async function loadTemplate(simKey: string): Promise<void> {
    key.value = simKey;
    const meta = getCatalogMeta(simKey);
    if (!meta) {
      runError.value = `Mô phỏng '${simKey}' không tồn tại trong danh mục.`;
      editorCode.value = '';
      return;
    }
    editorCode.value = TEMPLATES[simKey] ?? `// Chưa có code mẫu cho '${simKey}'\n// Code chạy trong sandbox: array (mảng dữ liệu), compare(i,j), swap(i,j)\n`;
    runState.value = 'idle';
    runError.value = null;

    // Nạp lịch sử trước đó từ localStorage nếu có
    try {
      const localKey = `coderun_history_${simKey}`;
      const saved = JSON.parse(localStorage.getItem(localKey) ?? '[]') as CodeRunSummary[];
      if (Array.isArray(saved)) {
        submissions.value = saved;
      }
    } catch {
      submissions.value = [];
    }
  }

  /** Chạy code trong sandbox client (SDD §4.0.3 — runCode; giới hạn 50.000 event). */
  async function run(customArray?: number[]): Promise<RunResult | null> {
    if (!editorCode.value.trim()) {
      runError.value = 'Hãy nhập code trước khi chạy.';
      return null;
    }
    runState.value = 'running';
    runError.value = null;
    lastOutput.value = null;
    lastStats.value = null;

    try {
      let targetArray = customArray;
      if (!targetArray || targetArray.length === 0) {
        const sim = useSimulationStore();
        const cfgArray = sim.inputConfig?.data && (sim.inputConfig.data as Record<string, unknown>).array;
        if (Array.isArray(cfgArray) && cfgArray.length > 0 && typeof cfgArray[0] === 'number') {
          targetArray = cfgArray as number[];
        } else {
          targetArray = [5, 3, 8, 1, 9, 2, 7];
        }
      }

      const result = await Promise.resolve(
        runCode({ code: editorCode.value, entry: 'solve', bindings: [] }, targetArray),
      );
      if (result.error) {
        runState.value = 'error';
        runError.value = `Lỗi dòng ${result.error.line}: ${result.error.message}`;

        const errSummary: CodeRunSummary = {
          id: Date.now(),
          exerciseId: null,
          key: key.value,
          code: editorCode.value,
          status: 'Error',
          durationMs: 0,
          output: null,
          error: `Dòng ${result.error.line}: ${result.error.message}`,
          passed: 0,
          total: 1,
          createdAt: new Date().toISOString(),
        };
        lastRun.value = errSummary;
        submissions.value = [errSummary, ...submissions.value.filter(s => s.id !== errSummary.id)];
        saveHistoryToLocal();
        return null;
      }
      lastOutput.value = result.output;
      lastStats.value = result.stats;
      runState.value = 'passed';

      // Lưu vết lần chạy lên server và đồng bộ lịch sử
      let savedSummary: CodeRunSummary;
      try {
        const saved = await codeRunnerApi.saveCodeRun({
          key: key.value,
          code: editorCode.value,
          input: JSON.stringify(targetArray),
          status: 'Success',
          durationMs: result.stats?.durationMs ?? 0,
          output: result.output !== undefined ? JSON.stringify(result.output) : null,
          stats: result.stats
            ? { comparisons: result.stats.comparisons, swaps: result.stats.swaps }
            : undefined,
        });
        savedSummary = {
          id: saved.id || Date.now(),
          exerciseId: saved.exerciseId ?? null,
          key: saved.key || key.value,
          code: editorCode.value,
          status: saved.status || 'Success',
          durationMs: saved.durationMs ?? (result.stats?.durationMs ?? 0),
          output: saved.output ?? (result.output !== undefined ? JSON.stringify(result.output) : null),
          passed: 1,
          total: 1,
          createdAt: saved.createdAt || new Date().toISOString(),
        };
      } catch {
        savedSummary = {
          id: Date.now(),
          exerciseId: null,
          key: key.value,
          code: editorCode.value,
          status: 'Success',
          durationMs: result.stats?.durationMs ?? 0,
          output: result.output !== undefined ? JSON.stringify(result.output) : null,
          passed: 1,
          total: 1,
          createdAt: new Date().toISOString(),
        };
      }
      lastRun.value = savedSummary;
      submissions.value = [savedSummary, ...submissions.value.filter(s => s.id !== savedSummary.id)];
      saveHistoryToLocal();

      return result;
    } catch (err) {
      runState.value = 'error';
      runError.value = err instanceof Error ? err.message : 'Lỗi không xác định khi chạy code.';
      return null;
    }
  }

  function saveHistoryToLocal(): void {
    if (!key.value) return;
    try {
      const localKey = `coderun_history_${key.value}`;
      localStorage.setItem(localKey, JSON.stringify(submissions.value.slice(0, 30)));
    } catch {}
  }

  async function submit(exerciseId: number, classAssignmentId?: number | null): Promise<CodeSubmitResult> {
    runState.value = 'running';
    runError.value = null;
    try {
      const result = await codeRunnerApi.submitCode(exerciseId, editorCode.value, classAssignmentId);
      runState.value = 'passed';
      await fetchHistory(exerciseId);
      return result;
    } catch (err) {
      runState.value = 'error';
      runError.value = err instanceof Error ? err.message : 'Không thể nộp bài.';
      throw err;
    }
  }

  async function fetchHistory(exerciseId?: number | null): Promise<void> {
    if (exerciseId && exerciseId > 0) {
      try {
        const remote = await codeRunnerApi.fetchMyCodeSubmissions(exerciseId);
        if (Array.isArray(remote) && remote.length > 0) {
          const map = new Map<number, CodeRunSummary>();
          for (const s of submissions.value) map.set(s.id, s);
          for (const r of remote) map.set(r.id, r);
          submissions.value = Array.from(map.values()).sort(
            (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
          );
          saveHistoryToLocal();
          return;
        }
      } catch {
        // Giữ nguyên submissions hiện tại
      }
    }

    // Nạp từ localStorage
    if (key.value) {
      try {
        const localKey = `coderun_history_${key.value}`;
        const saved = JSON.parse(localStorage.getItem(localKey) ?? '[]') as CodeRunSummary[];
        if (Array.isArray(saved) && saved.length > 0) {
          const map = new Map<number, CodeRunSummary>();
          for (const s of submissions.value) map.set(s.id, s);
          for (const s of saved) map.set(s.id, s);
          submissions.value = Array.from(map.values()).sort(
            (a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime(),
          );
        }
      } catch {}
    }
  }

  function restoreTemplate(): void {
    if (key.value) {
      void loadTemplate(key.value);
    }
  }

  function reset(): void {
    editorCode.value = '';
    runState.value = 'idle';
    lastRun.value = null;
    submissions.value = [];
    lastOutput.value = null;
    lastStats.value = null;
    runError.value = null;
    key.value = '';
  }

  return {
    editorCode,
    runState,
    lastRun,
    submissions,
    lastOutput,
    lastStats,
    runError,
    isRunning,
    loadTemplate,
    run,
    submit,
    fetchHistory,
    restoreTemplate,
    reset,
  };
});
