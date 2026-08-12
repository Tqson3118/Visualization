<script setup lang="ts">
// BenchmarkPanel — Màn 17: đo thật bằng runMeasure (engines) + bảng + biểu đồ SVG + kết luận
// + lưu qua POST /benchmarks/run (API_REFERENCE §4.14). MIỄN PHÍ tim (20.4).
import { computed, ref } from 'vue';

import VChart from 'vue-echarts';
import { use } from 'echarts/core';
import { CanvasRenderer } from 'echarts/renderers';
import { LineChart } from 'echarts/charts';
import { GridComponent, LegendComponent, TooltipComponent } from 'echarts/components';

import { runBenchmark } from '@/api/benchmark';
import {
  BENCHMARK_ALGORITHMS,
  bestArray,
  randomArray,
  sizesForComplexity,
  worstArray,
  type BenchmarkMeasure,
} from '@/engines/benchmark/codeTemplates';
import { runMeasureInWorker } from '@/engines/worker/compileWorker';
import { useUiStore } from '@/stores/ui';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Skeleton from '@/components/ui/Skeleton.vue';

// Đăng ký module ECharts dùng riêng (tree-shaking) — G-F2c: chart line DurationMs theo n
use([CanvasRenderer, LineChart, GridComponent, LegendComponent, TooltipComponent]);

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

/**
 * Map kết quả đo (rows: size × key) → results theo contract BE:
 * [{ key, measurements: [{ n, durationMs, comparisons, swaps }] }] — ADR-012 (SETUP_TODO §6.8).
 * Độ đo bị timeout/null → gửi 0 (endpoint chỉ lưu vết; UI vẫn hiển thị N/A từ rows).
 */
function buildResults(): Array<{ key: string; measurements: Array<{ n: number; durationMs: number; comparisons: number; swaps: number }> }> {
  return selectedKeys.value.map((key) => ({
    key,
    measurements: rows.value.map((row) => ({
      n: row.size,
      durationMs: row.measures[key]?.durationMs ?? 0,
      comparisons: row.measures[key]?.comparisons ?? 0,
      swaps: row.measures[key]?.swaps ?? 0,
    })),
  }));
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
        // runMeasureInWorker: đo trong Web Worker (ADR-012) — không chặn UI;
        // timeout 5s/độ đo → null → hiển thị N/A (SDD Màn 17)
        try {
          const result = await runMeasureInWorker(def.code, input);
          measures[key] = result
            ? {
                durationMs: result.durationMs,
                comparisons: result.comparisons,
                swaps: result.swaps,
                writes: result.writes,
              }
            : null;
        } catch {
          measures[key] = null; // phòng hờ: lỗi bất ngờ → N/A
        }
        await new Promise((resolve) => setTimeout(resolve, 0)); // nhường UI
      }
      rows.value = [...rows.value, { size, measures }];
    }
    progress.value = 'Hoàn tất.';
    // Lưu kết quả (bỏ qua lỗi — đo client vẫn OK). Gửi kèm results đo được — BE bắt buộc (SETUP_TODO §6.8)
    try {
      await runBenchmark({
        keys: selectedKeys.value,
        sizes: sizes.value,
        results: buildResults(),
      });
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

// ── Biểu đồ ECharts (vue-echarts 8.1 + echarts 6.1 — G-F2c) ──
// Line chart: DurationMs theo n, overlay nhiều thuật toán; giữ bảng số liệu + kết luận.
// Màu đọc từ CSS variables lúc dựng option → đổi theme không cần reload.

const LINE_PALETTE = ['#14b8a6', '#8b5cf6', '#f59e0b', '#f43f5e', '#06b6d4'];

/** Đọc CSS variable thành màu cụ thể (ECharts canvas không hiểu var()). */
function cssVar(name: string, fallback: string): string {
  if (typeof document === 'undefined') return fallback;
  const val = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  return val || fallback;
}

function prefersReducedMotion(): boolean {
  return (
    typeof window !== 'undefined' &&
    typeof window.matchMedia === 'function' &&
    window.matchMedia('(prefers-reduced-motion: reduce)').matches
  );
}

const chartOption = computed(() => {
  // Phụ thuộc theme (ui.theme) → recompute option khi toggle sáng/tối
  void ui.theme;
  const textColor = cssVar('--color-text-muted', '#5E7A77');
  const axisColor = cssVar('--color-border', '#cbd5e1');
  const cardColor = cssVar('--color-card', '#ffffff');
  const foreground = cssVar('--color-foreground', '#134e4a');

  const series = selectedKeys.value.map((key, idx) => ({
    name: BENCHMARK_ALGORITHMS[key].title,
    type: 'line' as const,
    symbol: 'circle',
    symbolSize: 7,
    lineStyle: { width: 2.5 },
    itemStyle: { color: LINE_PALETTE[idx % LINE_PALETTE.length] },
    data: rows.value.map((row) =>
      row.measures[key] ? [String(row.size), (row.measures[key] as BenchmarkMeasure).durationMs] : null,
    ),
    connectNulls: false,
  }));

  return {
    color: LINE_PALETTE,
    animation: !prefersReducedMotion(),
    animationDuration: 400,
    tooltip: {
      trigger: 'axis' as const,
      backgroundColor: cardColor,
      borderColor: axisColor,
      textStyle: { color: foreground, fontSize: 12 },
    },
    legend: {
      bottom: 0,
      icon: 'circle',
      itemWidth: 10,
      itemHeight: 10,
      textStyle: { color: textColor, fontSize: 12 },
    },
    grid: { left: 8, right: 16, top: 28, bottom: 44, containLabel: true },
    xAxis: {
      type: 'category' as const,
      name: 'n',
      nameLocation: 'middle' as const,
      nameGap: 30,
      nameTextStyle: { color: textColor, fontWeight: 600 },
      data: sizes.value.map(String),
      axisLine: { lineStyle: { color: axisColor } },
      axisLabel: { color: textColor },
    },
    yAxis: {
      type: 'value' as const,
      name: 'ms',
      nameTextStyle: { color: textColor, fontWeight: 600 },
      splitLine: { lineStyle: { color: axisColor } },
      axisLabel: { color: textColor },
    },
    series,
  };
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
      <Badge variant="success">Miễn phí tim (20.4)</Badge>
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
        <Button size="lg" :loading="running" :disabled="selectedKeys.length < 2" @click="run">
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
        <h3 class="benchmark__chart-title">Thời gian thực tế (ms) theo n — overlay nhiều thuật toán</h3>
        <VChart
          v-if="rows.length > 0"
          :option="chartOption"
          autoresize
          class="benchmark__echarts"
          role="img"
          aria-label="Biểu đồ benchmark thời gian thực tế theo n"
        />
        <p class="benchmark__chart-note">Hover để xem giá trị · dữ liệu N/A (quá 5s) hiển thị dạng khoảng trống</p>
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
.benchmark__title {
  font-size: var(--text-2xl);
  background-image: var(--gradient-mint);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.benchmark__controls { display: flex; flex-direction: column; gap: var(--space-md); }

.benchmark__label { font-size: var(--text-sm); font-weight: 600; margin-bottom: var(--space-sm); }

.benchmark__chips { display: flex; flex-wrap: wrap; gap: var(--space-sm); }

.benchmark__chip {
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  padding: 4px 12px;
  border-radius: var(--radius-full);
  font-size: var(--text-xs);
  font-weight: 600;
  cursor: pointer;
  color: var(--color-text-muted);
  transition: var(--transition-fast);
}

.benchmark__chip:hover { border-color: var(--color-primary); color: var(--color-primary); transform: translateY(-1px); }

.benchmark__chip--on {
  background-image: var(--gradient-mint);
  color: var(--color-on-primary);
  border-color: transparent;
  box-shadow: var(--shadow-sm);
}

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

.benchmark__echarts {
  width: 100%;
  height: 320px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-sm);
}

.benchmark__chart-note { font-size: var(--text-xs); color: var(--color-text-muted); }

.benchmark__conclusion { border-color: var(--color-secondary); }
.benchmark__conclusion-text { font-size: var(--text-sm); line-height: 1.7; }
</style>
