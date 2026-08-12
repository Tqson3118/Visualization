<script setup lang="ts">
// BenchmarkPanel — Màn 17: đo thật bằng runMeasure (engines) + bảng + biểu đồ SVG + kết luận
// + lưu qua POST /benchmarks/run (API_REFERENCE §4.14). MIỄN PHÍ tim (20.4).
import { computed, ref } from 'vue';

import { runBenchmark } from '@/api/benchmark';
import {
  BENCHMARK_ALGORITHMS,
  bestArray,
  randomArray,
  sizesForComplexity,
  worstArray,
  type BenchmarkMeasure,
} from '@/engines/benchmark/codeTemplates';
import { runMeasure } from '@/engines/core/stepExecutor';
import { useUiStore } from '@/stores/ui';
import Button from '@/components/ui/Button.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Skeleton from '@/components/ui/Skeleton.vue';

const props = defineProps<{
  /** Keys mặc định (từ route /benchmark/:k1/:k2) */
  defaultKeys?: string[];
}>();

const ui = useUiStore();

const selectedKeys = ref<string[]>(
  (props.defaultKeys ?? []).filter((key) => key in BENCHMARK_ALGORITHMS).slice(0, 5),
);
if (selectedKeys.value.length === 0) selectedKeys.value = ['sort.bubble', 'sort.merge'];

const dataMode = ref<'random' | 'worst' | 'best'>('random');
const running = ref(false);
const rows = ref<Array<{ size: number; measures: Record<string, BenchmarkMeasure | null> }>>([]);
const progress = ref('');
const error = ref('');
const saved = ref(false);

const availableKeys = computed(() => Object.keys(BENCHMARK_ALGORITHMS).sort());

const sizes = computed(() => {
  const set = new Set<number>();
  for (const key of selectedKeys.value) {
    for (const size of sizesForComplexity(BENCHMARK_ALGORITHMS[key].complexityClass)) {
      set.add(size);
    }
  }
  return [...set].sort((a, b) => a - b);
});

const maxDuration = computed(() => {
  let max = 0;
  for (const row of rows.value) {
    for (const key of selectedKeys.value) {
      const measure = row.measures[key];
      if (measure) max = Math.max(max, measure.durationMs);
    }
  }
  return Math.max(1, max);
});

async function toggleKey(key: string): Promise<void> {
  if (selectedKeys.value.includes(key)) {
    if (selectedKeys.value.length > 2) {
      selectedKeys.value = selectedKeys.value.filter((k) => k !== key);
    } else {
      ui.showToast('Cần ít nhất 2 giải thuật để so sánh.', 'warning');
    }
  } else {
    if (selectedKeys.value.length >= 5) {
      ui.showToast('Tối đa 5 giải thuật/lần chạy.', 'warning');
      return;
    }
    selectedKeys.value.push(key);
  }
}

function inputFor(mode: typeof dataMode.value, size: number): number[] {
  if (mode === 'worst') return worstArray(size);
  if (mode === 'best') return bestArray(size);
  return randomArray(size);
}

async function run(): Promise<void> {
  if (selectedKeys.value.length < 2) {
    ui.showToast('Chọn ít nhất 2 giải thuật để so sánh.', 'warning');
    return;
  }
  running.value = true;
  error.value = '';
  saved.value = false;
  rows.value = [];
  try {
    for (const size of sizes.value) {
      progress.value = `Đang đo n=${size}...`;
      const measures: Record<string, BenchmarkMeasure | null> = {};
      for (const key of selectedKeys.value) {
        const def = BENCHMARK_ALGORITHMS[key];
        const input = inputFor(dataMode.value, size);
        // runMeasure: timeout 5s/độ đo → null → hiển thị N/A (SDD Màn 17)
        const result = runMeasure(def.code, input);
        measures[key] = result
          ? {
              durationMs: result.durationMs,
              comparisons: result.comparisons,
              swaps: result.swaps,
              writes: result.writes,
            }
          : null;
        await new Promise((resolve) => setTimeout(resolve, 0)); // nhường UI
      }
      rows.value = [...rows.value, { size, measures }];
    }
    progress.value = 'Hoàn tất.';
    // Lưu kết quả (bỏ qua lỗi — đo client vẫn OK)
    try {
      await runBenchmark({ keys: selectedKeys.value, sizes: sizes.value });
      saved.value = true;
    } catch {
      saved.value = false;
    }
  } finally {
    running.value = false;
  }
}

// ── Kết luận tự sinh (template — 19.9 tầng 4) ──
const conclusion = computed(() => {
  if (rows.value.length === 0) return '';
  const last = rows.value[rows.value.length - 1];
  const parts: string[] = [];
  const best = selectedKeys.value.reduce<(string | null)>((acc, key) => {
    const measure = last.measures[key];
    if (!measure) return acc;
    if (acc === null) return key;
    const bestMeasure = last.measures[acc];
    if (!bestMeasure || measure.durationMs < bestMeasure.durationMs) return key;
    return acc;
  }, null);
  if (best) {
    parts.push(`Tại n=${last.size}, ${BENCHMARK_ALGORITHMS[best].title} nhanh nhất.`);
  }
  for (const key of selectedKeys.value) {
    const measure = last.measures[key];
    if (!measure) continue;
    const def = BENCHMARK_ALGORITHMS[key];
    parts.push(
      `${def.title}: ${measure.durationMs}ms, ${measure.comparisons} phép so sánh (kỳ vọng ${def.complexityClass}).`,
    );
  }
  if (parts.length === 0) parts.push('Một số độ đo vượt 5 giây — hiển thị N/A.');
  return parts.join(' ');
});

// ── Biểu đồ SVG overlay lý thuyết ──
const CHART_W = 640;
const CHART_H = 280;
const CHART_PAD = 40;

interface ChartLine {
  key: string;
  color: string;
  points: Array<{ x: number; y: number }>;
}

interface ChartData {
  lines: ChartLine[];
  xLabels: Array<{ size: number; x: number }>;
}

const chartPoints = computed<ChartData>(() => {
  if (rows.value.length === 0) return { lines: [], xLabels: [] };
  const keys = selectedKeys.value;
  const xMax = sizes.value[sizes.value.length - 1];
  const x = (size: number): number => CHART_PAD + ((size - sizes.value[0]) / Math.max(1, xMax - sizes.value[0])) * (CHART_W - CHART_PAD * 2);
  const y = (ms: number): number => CHART_H - CHART_PAD - (ms / maxDuration.value) * (CHART_H - CHART_PAD * 2);
  const lines: ChartLine[] = keys.map((key) => ({
    key,
    color: key.includes('merge') || key.includes('quick') || key.includes('heap')
      ? 'var(--color-primary)'
      : 'var(--color-destructive)',
    points: rows.value
      .filter((row) => row.measures[key])
      .map((row) => ({ x: x(row.size), y: y((row.measures[key] as BenchmarkMeasure).durationMs) })),
  }));
  return { lines, xLabels: sizes.value.map((size) => ({ size, x: x(size) })) };
});

function exportCsv(): void {
  const header = ['n', ...selectedKeys.value.map((key) => `${key} (ms)`), ...selectedKeys.value.map((key) => `${key} (so sánh)`)];
  const lines = rows.value.map((row) =>
    [row.size, ...selectedKeys.value.map((key) => row.measures[key]?.durationMs ?? 'N/A'), ...selectedKeys.value.map((key) => row.measures[key]?.comparisons ?? 'N/A')].join(','),
  );
  const csv = '\uFEFF' + [header.join(','), ...lines].join('\n');
  const blob = new Blob([csv], { type: 'text/csv;charset=utf-8' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.href = url;
  link.download = 'benchmark.csv';
  link.click();
  URL.revokeObjectURL(url);
}
</script>

<template>
  <section class="benchmark">
    <header class="benchmark__header">
      <h2 class="benchmark__title">⚖ Benchmark Lab</h2>
      <span class="benchmark__free">Miễn phí tim (20.4)</span>
    </header>

    <div class="benchmark__controls card">
      <div class="benchmark__algo-select">
        <p class="benchmark__label">Chọn giải thuật (2-5, cùng cấu trúc dữ liệu):</p>
        <div class="benchmark__chips">
          <button
            v-for="key in availableKeys"
            :key="key"
            type="button"
            class="benchmark__chip"
            :class="{ 'benchmark__chip--on': selectedKeys.includes(key) }"
            @click="toggleKey(key)"
          >
            {{ BENCHMARK_ALGORITHMS[key].title }}
          </button>
        </div>
      </div>

      <div class="benchmark__runbar">
        <label class="benchmark__mode">
          Dữ liệu:
          <select v-model="dataMode">
            <option value="random">Ngẫu nhiên</option>
            <option value="worst">Xấu nhất</option>
            <option value="best">Tốt nhất</option>
          </select>
        </label>
        <Button :loading="running" :disabled="selectedKeys.length < 2" @click="run">
          ▶ Chạy benchmark
        </Button>
        <Button variant="ghost" :disabled="rows.length === 0" @click="exportCsv">Xuất CSV</Button>
      </div>
      <p v-if="progress && running" class="benchmark__progress" role="status">{{ progress }}</p>
      <p v-if="saved" class="benchmark__saved text-muted">Đã lưu kết quả lên server.</p>
    </div>

    <EmptyState
      v-if="rows.length === 0 && !running"
      icon="scissors"
      title="Chưa có số liệu"
      description="Chọn 2+ giải thuật rồi bấm 'Chạy benchmark' — mỗi độ đo tối đa 5 giây, quá hạn ghi N/A."
    />

    <div v-else class="benchmark__results">
      <div class="benchmark__table-wrap card">
        <table class="benchmark__table">
          <thead>
            <tr>
              <th>n</th>
              <template v-for="key in selectedKeys" :key="key">
                <th>{{ BENCHMARK_ALGORITHMS[key].title }}</th>
                <th class="benchmark__th-sub">so sánh</th>
              </template>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in rows" :key="row.size">
              <td class="benchmark__n">{{ row.size }}</td>
              <template v-for="key in selectedKeys" :key="key">
                <td class="benchmark__cell">
                  <template v-if="row.measures[key]">
                    {{ row.measures[key]?.durationMs }}ms
                  </template>
                  <template v-else>N/A</template>
                </td>
                <td class="benchmark__cell-sub">{{ row.measures[key]?.comparisons ?? '—' }}</td>
              </template>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="benchmark__chart card">
        <h3 class="benchmark__chart-title">Thời gian thực tế (ms) theo n</h3>
        <svg :viewBox="`0 0 ${CHART_W} ${CHART_H}`" class="benchmark__svg" role="img" aria-label="Biểu đồ benchmark">
          <line v-for="i in 4" :key="i" x1="40" :y1="CHART_H - 40 - ((CHART_H - 80) / 4) * i" x2="600" :y2="CHART_H - 40 - ((CHART_H - 80) / 4) * i" stroke="#e2e8f0" stroke-width="1" />
          <template v-for="line in chartPoints.lines" :key="line.key">
            <polyline
              :points="line.points.map((p) => `${p.x},${p.y}`).join(' ')"
              :fill="'none'"
              :stroke="line.color"
              stroke-width="2.5"
            />
            <circle
              v-for="p in line.points"
              :key="`${line.key}-${p.x}`"
              :cx="p.x"
              :cy="p.y"
              r="3.5"
              :fill="line.color"
            />
          </template>
          <text v-for="label in chartPoints.xLabels" :key="label.size" :x="label.x" :y="CHART_H - 18" font-size="11" fill="#5E7A77" text-anchor="middle">
            {{ label.size }}
          </text>
          <text x="20" :y="20" font-size="12" fill="#5E7A77">ms</text>
        </svg>
        <div class="benchmark__legend">
          <span v-for="line in chartPoints.lines" :key="line.key" class="benchmark__legend-item">
            <span class="benchmark__legend-dot" :style="{ background: line.color }" />
            {{ BENCHMARK_ALGORITHMS[line.key].title }}
          </span>
        </div>
      </div>

      <div v-if="conclusion" class="benchmark__conclusion card">
        <h3 class="benchmark__chart-title">Kết luận</h3>
        <p class="benchmark__conclusion-text">{{ conclusion }}</p>
      </div>
    </div>

    <div v-if="running" class="benchmark__skeleton">
      <Skeleton height="48px" :lines="3" />
    </div>
  </section>
</template>

<style scoped>
.benchmark { display: flex; flex-direction: column; gap: var(--space-lg); }

.benchmark__header { display: flex; align-items: center; justify-content: space-between; gap: var(--space-md); flex-wrap: wrap; }
.benchmark__title { font-size: var(--text-xl); }
.benchmark__free { font-size: var(--text-xs); color: var(--color-success); font-weight: 700; }

.benchmark__controls { display: flex; flex-direction: column; gap: var(--space-md); }

.benchmark__label { font-size: var(--text-sm); font-weight: 600; margin-bottom: var(--space-sm); }

.benchmark__chips { display: flex; flex-wrap: wrap; gap: var(--space-xs); }

.benchmark__chip {
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  padding: 4px 12px;
  border-radius: var(--radius-full);
  font-size: var(--text-xs);
  font-weight: 600;
  cursor: pointer;
  color: var(--color-text-muted);
}

.benchmark__chip--on { background: var(--color-primary); color: var(--color-on-primary); border-color: var(--color-primary); }

.benchmark__runbar { display: flex; gap: var(--space-sm); align-items: center; flex-wrap: wrap; }

.benchmark__mode { font-size: var(--text-sm); display: inline-flex; align-items: center; gap: 6px; }
.benchmark__mode select { padding: 4px 8px; border: 1px solid var(--color-border); border-radius: var(--radius-md); }

.benchmark__progress { font-size: var(--text-sm); color: var(--color-primary); }
.benchmark__saved { font-size: var(--text-xs); }

.benchmark__results { display: flex; flex-direction: column; gap: var(--space-lg); }

.benchmark__table-wrap { overflow-x: auto; }

.benchmark__table { width: 100%; border-collapse: collapse; min-width: 520px; }

.benchmark__table th {
  padding: var(--space-sm);
  font-size: var(--text-xs);
  text-align: left;
  border-bottom: 2px solid var(--color-border);
  white-space: nowrap;
}

.benchmark__th-sub { color: var(--color-text-muted); font-weight: 400; }

.benchmark__table td { padding: var(--space-sm); font-size: var(--text-sm); border-bottom: 1px solid var(--color-border); }

.benchmark__n { font-weight: 800; font-family: var(--font-mono); }
.benchmark__cell { font-family: var(--font-mono); }
.benchmark__cell-sub { font-family: var(--font-mono); color: var(--color-text-muted); font-size: var(--text-xs); }

.benchmark__chart { display: flex; flex-direction: column; gap: var(--space-sm); }

.benchmark__chart-title { font-size: var(--text-md); }

.benchmark__svg { width: 100%; height: auto; background: var(--color-surface); border: 1px solid var(--color-border); border-radius: var(--radius-md); }

.benchmark__legend { display: flex; flex-wrap: wrap; gap: var(--space-md); font-size: var(--text-xs); color: var(--color-text-muted); }

.benchmark__legend-item { display: inline-flex; align-items: center; gap: 6px; }
.benchmark__legend-dot { width: 10px; height: 10px; border-radius: 50%; }

.benchmark__conclusion { border-color: var(--color-secondary); }
.benchmark__conclusion-text { font-size: var(--text-sm); line-height: 1.7; }
</style>
