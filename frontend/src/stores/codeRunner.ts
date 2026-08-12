import { computed, ref } from 'vue';
import { defineStore } from 'pinia';

import * as codeRunnerApi from '@/api/codeRunner';
import type { CodeRunSummary, CodeSubmitResult } from '@/api/codeRunner';
import { getCatalogMeta } from '@/engines/catalog';
import { runCode } from '@/engines/core/stepExecutor';

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
  }

  /** Chạy code trong sandbox client (SDD §4.0.3 — runCode; giới hạn 50.000 event). */
  async function run(): Promise<void> {
    if (!editorCode.value.trim()) {
      runError.value = 'Hãy nhập code trước khi chạy.';
      return;
    }
    runState.value = 'running';
    runError.value = null;
    lastOutput.value = null;
    lastStats.value = null;

    try {
      const defaultArray = [5, 3, 8, 1, 9, 2, 7];
      const result = await Promise.resolve(
        runCode({ code: editorCode.value, entry: 'solve', bindings: [] }, defaultArray),
      );
      if (result.error) {
        runState.value = 'error';
        runError.value = `Lỗi dòng ${result.error.line}: ${result.error.message}`;
        return;
      }
      lastOutput.value = result.output;
      lastStats.value = result.stats;
      runState.value = 'passed';
      // Lưu vết lần chạy lên server (bỏ qua lỗi mạng — chạy client vẫn OK).
      // Contract CodeRunRequest = {exerciseId?, key, code, input? (string), status, durationMs, ...} — ADR-012 (SETUP_TODO §6.7)
      try {
        lastRun.value = await codeRunnerApi.saveCodeRun({
          key: key.value,
          code: editorCode.value,
          input: JSON.stringify(defaultArray),
          status: 'Success',
          durationMs: result.stats?.durationMs ?? 0,
          stats: result.stats
            ? { comparisons: result.stats.comparisons, swaps: result.stats.swaps }
            : undefined,
        });
      } catch {
        lastRun.value = null;
      }
    } catch (err) {
      runState.value = 'error';
      runError.value = err instanceof Error ? err.message : 'Lỗi không xác định khi chạy code.';
    }
  }

  async function submit(exerciseId: number): Promise<CodeSubmitResult> {
    runState.value = 'running';
    runError.value = null;
    try {
      const result = await codeRunnerApi.submitCode(exerciseId, editorCode.value);
      runState.value = 'passed';
      await fetchHistory(exerciseId);
      return result;
    } catch (err) {
      runState.value = 'error';
      runError.value = err instanceof Error ? err.message : 'Không thể nộp bài.';
      throw err;
    }
  }

  async function fetchHistory(exerciseId: number): Promise<void> {
    try {
      submissions.value = await codeRunnerApi.fetchMyCodeSubmissions(exerciseId);
    } catch {
      submissions.value = [];
    }
  }

  function restoreTemplate(): void {
    if (key.value) {
      void loadTemplate(key.value);
    }
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
  };
});
