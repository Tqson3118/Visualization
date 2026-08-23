<script setup lang="ts">
// BenchmarkPanel — Màn 17: đo thật bằng runMeasure (engines) + bảng + biểu đồ SVG + kết luận
// + lưu qua POST /benchmarks/run (API_REFERENCE §4.14). KHÔNG tốn tim (SDD 20.4).
// P1-B3: chips raw → Button.vue (aria-pressed), bỏ ⚖/▶ → lucide Scale/Play,
// bảng+chart+conclusion = vùng dữ liệu LUÔN tối canvas-ink (block-token + index mono),
// palette ECharts đọc CSS var canvas palette (không hex rời), mobile bảng → card-stack,
// empty state ✂ → hourglass + copy §9. Giữ nguyên logic/worker/contract BE/exportCsv.
import { computed, ref } from 'vue';

import VChart from 'vue-echarts';
import { use } from 'echarts/core';
import { CanvasRenderer } from 'echarts/renderers';
import { LineChart } from 'echarts/charts';
import { GridComponent, LegendComponent, TooltipComponent } from 'echarts/components';
import { LegacyGridContainLabel } from 'echarts/features';

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
import { Play, Scale } from 'lucide-vue-next';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Skeleton from '@/components/ui/Skeleton.vue';

// Đăng ký module ECharts dùng riêng (tree-shaking) — G-F2c: chart line DurationMs theo n
// LegacyGridContainLabel: echarts 6 thay grid.containLabel (deprecated) — giữ label không bị cắt
use([CanvasRenderer, LineChart, GridComponent, LegendComponent, TooltipComponent, LegacyGridContainLabel]);

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
// P1-B3: chart = vùng dữ liệu LUÔN tối (canvas-ink) — màu đọc từ CSS var canvas palette
// (data-core/resolved/conflict/warning/info — DESIGN-IDENTITY §1.2), không hex rời.
// Fallback hex chỉ khi var() không đọc được (SSR) — pattern cssVar() của CodeRunnerView.

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

// Token canvas palette (DESIGN-IDENTITY §1.2) — fallback hex chỉ phòng SSR
const LINE_PALETTE = ['--color-data-core', '--color-resolved', '--color-conflict', '--color-warning', '--color-info'];
const LINE_PALETTE_FALLBACK = ['#4255FF', '#34D399', '#F87171', '#D97706', '#0891B2'];

function paletteColor(idx: number): string {
  const token = LINE_PALETTE[idx % LINE_PALETTE.length];
  const fallback = LINE_PALETTE_FALLBACK[idx % LINE_PALETTE_FALLBACK.length];
  return cssVar(token, fallback);
}

const chartOption = computed(() => {
  // Vùng dữ liệu LUÔN tối (quyết định xuyên-nhóm #5) — màu trục/legend theo index-muted
  void ui.theme;
  const textColor = cssVar('--color-index-muted', '#6B7385');
  const axisColor = cssVar('--color-index-muted', '#6B7385');
  const tooltipBg = cssVar('--color-canvas-ink', '#0D1020');

  const series = selectedKeys.value.map((key, idx) => ({
    name: BENCHMARK_ALGORITHMS[key].title,
    type: 'line' as const,
    symbol: 'circle',
    symbolSize: 7,
    lineStyle: { width: 2.5 },
    itemStyle: { color: paletteColor(idx) },
    data: rows.value.map((row) =>
      row.measures[key] ? [String(row.size), (row.measures[key] as BenchmarkMeasure).durationMs] : null,
    ),
    connectNulls: false,
  }));

  return {
    color: LINE_PALETTE.map((token, idx) => paletteColor(idx)),
    animation: !prefersReducedMotion(),
    animationDuration: 400,
    tooltip: {
      trigger: 'axis' as const,
      backgroundColor: tooltipBg,
      borderColor: axisColor,
      textStyle: { color: textColor, fontSize: 12 },
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
      <h2 class="benchmark__title">
        <Scale :size="20" aria-hidden="true" class="benchmark__title-icon" />
        Benchmark Lab
      </h2>
      <Badge variant="success">Không tốn tim</Badge>
    </header>

    <div class="benchmark__controls">
      <div class="benchmark__algo-select">
        <p class="benchmark__label">Chọn giải thuật (2–5, cùng cấu trúc dữ liệu):</p>
        <div class="benchmark__chips">
          <Button
            v-for="key in availableKeys"
            :key="key"
            :variant="selectedKeys.includes(key) ? 'primary' : 'secondary'"
            size="sm"
            :aria-pressed="selectedKeys.includes(key)"
            @click="toggleKey(key)"
          >
            {{ BENCHMARK_ALGORITHMS[key].title }}
          </Button>
        </div>
      </div>

      <div class="benchmark__runbar">
        <label class="benchmark__mode">
          Dữ liệu:
          <select v-model="dataMode" name="data-mode" class="benchmark__select">
            <option value="random">Ngẫu nhiên</option>
            <option value="worst">Xấu nhất</option>
            <option value="best">Tốt nhất</option>
          </select>
        </label>
        <Button size="lg" :loading="running" :disabled="selectedKeys.length < 2" @click="run">
          <Play :size="16" aria-hidden="true" />
          Chạy benchmark
        </Button>
        <Button variant="ghost" :disabled="rows.length === 0" @click="exportCsv">Xuất CSV</Button>
      </div>
      <p v-if="progress && running" class="benchmark__progress" role="status">{{ progress }}</p>
      <p v-if="saved" class="benchmark__saved">Đã lưu kết quả benchmark.</p>
    </div>

    <EmptyState
      v-if="rows.length === 0 && !running"
      icon="hourglass"
      title="Chưa có số liệu đo"
      description="Chọn 2+ giải thuật phía trên rồi bấm Chạy benchmark — mỗi độ đo tối đa 5 giây, quá hạn ghi N/A."
    />

    <div v-else class="benchmark__results">
      <div class="benchmark__table-wrap">
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
              <td class="benchmark__n" data-label="n">{{ row.size }}</td>
              <template v-for="key in selectedKeys" :key="key">
                <td class="benchmark__cell" :data-label="`${BENCHMARK_ALGORITHMS[key].title} (ms)`">
                  <span v-if="row.measures[key]" class="benchmark__block">
                    {{ row.measures[key]?.durationMs }}ms
                  </span>
                  <span v-else class="benchmark__na">N/A</span>
                </td>
                <td class="benchmark__cell-sub" :data-label="`${BENCHMARK_ALGORITHMS[key].title} (so sánh)`">
                  {{ row.measures[key]?.comparisons ?? '—' }}
                </td>
              </template>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="benchmark__chart">
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

      <div v-if="conclusion" class="benchmark__conclusion">
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
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-lg);
  font-weight: 600;
  letter-spacing: -0.015em;
  color: var(--color-text-primary);
  margin: 0;
}

.benchmark__title-icon { color: var(--color-text-tertiary); flex-shrink: 0; }

/* Controls — elevation level-1 (bỏ class .card có shadow-md — §6) */
.benchmark__controls {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-lg);
}

.benchmark__label { font-size: var(--text-sm); font-weight: 600; color: var(--color-text-secondary); margin-bottom: var(--space-sm); }

.benchmark__chips { display: flex; flex-wrap: wrap; gap: var(--space-sm); }

.benchmark__runbar { display: flex; gap: var(--space-sm); align-items: center; flex-wrap: wrap; }

.benchmark__mode { font-size: var(--text-sm); display: inline-flex; align-items: center; gap: var(--space-sm); color: var(--color-text-secondary); }

/* Select chuẩn §4.4: h-40px, padding ngang 16px (>=8px text-viền) */
.benchmark__select {
  height: 40px;
  padding: 0 var(--space-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  font-size: var(--text-sm);
  color: var(--color-text-primary);
  cursor: pointer;
}

.benchmark__progress { font-size: var(--text-sm); font-family: var(--font-mono); color: var(--color-text-secondary); }
.benchmark__saved { font-size: var(--text-xs); color: var(--color-text-tertiary); }

/* Khoảnh khắc đầu tư: results region enter (250ms, easing chuẩn §7) */
.benchmark__results {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  animation: benchmark-enter 250ms cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes benchmark-enter {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}

@media (prefers-reduced-motion: reduce) {
  .benchmark__results { animation: none; }
}

/* ── Bảng = vùng dữ liệu LUÔN tối (canvas-ink) — block-token + index mono ── */
.benchmark__table-wrap {
  overflow-x: auto;
  background: var(--color-canvas-ink);
  border: 1px solid color-mix(in srgb, var(--color-index-muted) 45%, transparent);
  border-radius: var(--radius-md);
}

.benchmark__table { width: 100%; border-collapse: collapse; min-width: 520px; }

.benchmark__table th {
  padding: var(--space-sm) var(--space-md);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  text-align: left;
  color: var(--color-index-muted);
  border-bottom: 1px solid color-mix(in srgb, var(--color-index-muted) 30%, transparent);
  white-space: nowrap;
}

.benchmark__th-sub { color: color-mix(in srgb, var(--color-index-muted) 70%, transparent); font-weight: 400; }

.benchmark__table td {
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-sm);
  border-bottom: 1px solid color-mix(in srgb, var(--color-index-muted) 22%, transparent);
  color: color-mix(in srgb, white 85%, var(--color-index-muted));
}

/* n = index mono (signature "index dưới block") — thắng .benchmark__table td (specificity) */
.benchmark__table td.benchmark__n { font-family: var(--font-mono); font-size: var(--text-xs); color: var(--color-index-muted); }

/* duration = block-token (data-core, ngôn ngữ canvas) */
.benchmark__block {
  display: inline-flex;
  align-items: center;
  padding: var(--space-xs) var(--space-sm);
  border: 1px solid color-mix(in srgb, var(--color-data-core) 45%, transparent);
  border-radius: var(--radius-sm);
  background: color-mix(in srgb, var(--color-data-core) 14%, transparent);
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  color: color-mix(in srgb, white 90%, var(--color-index-muted));
  white-space: nowrap;
}

.benchmark__na { font-family: var(--font-mono); font-size: var(--text-xs); color: var(--color-index-muted); }

.benchmark__cell-sub { font-family: var(--font-mono); font-size: var(--text-xs); color: var(--color-index-muted); }

/* ── Chart + kết luận = vùng dữ liệu tối (quyết định #5) ── */
.benchmark__chart {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  background: var(--color-canvas-ink);
  border: 1px solid color-mix(in srgb, var(--color-index-muted) 45%, transparent);
  border-radius: var(--radius-lg);
  padding: var(--space-md);
}

.benchmark__chart-title {
  font-size: var(--text-md);
  font-weight: 600;
  letter-spacing: -0.015em;
  color: color-mix(in srgb, white 85%, var(--color-index-muted));
  margin: 0;
}

.benchmark__echarts { width: 100%; height: 320px; border-radius: var(--radius-md); }

.benchmark__chart-note { font-size: var(--text-xs); color: var(--color-index-muted); margin: 0; }

.benchmark__conclusion {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  background: var(--color-canvas-ink);
  border: 1px solid color-mix(in srgb, var(--color-index-muted) 45%, transparent);
  border-radius: var(--radius-lg);
  padding: var(--space-md);
}

.benchmark__conclusion-text {
  font-size: var(--text-sm);
  line-height: 1.7;
  color: color-mix(in srgb, white 85%, var(--color-index-muted));
  margin: 0;
}

.benchmark__skeleton { display: flex; flex-direction: column; gap: var(--space-md); }

/* ── Mobile ≤640px: bảng → card-stack (standard §8 — cấm scroll ngang bảng chính) ── */
@media (max-width: 640px) {
  .benchmark__table-wrap { overflow: visible; background: transparent; border: none; }

  .benchmark__table { min-width: 0; }

  .benchmark__table thead { display: none; }

  .benchmark__table, .benchmark__table tbody, .benchmark__table tr, .benchmark__table td { display: block; width: 100%; }

  .benchmark__table tbody { display: flex; flex-direction: column; gap: var(--space-sm); }

  .benchmark__table tr {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: var(--space-sm);
    background: var(--color-canvas-ink);
    border: 1px solid color-mix(in srgb, var(--color-index-muted) 45%, transparent);
    border-radius: var(--radius-md);
    padding: var(--space-sm) var(--space-md);
  }

  .benchmark__table td { border-bottom: none; padding: 0; }

  .benchmark__table td::before {
    content: attr(data-label);
    display: block;
    font-family: var(--font-mono);
    font-size: var(--text-xs);
    color: var(--color-index-muted);
    margin-bottom: var(--space-xs);
  }

  .benchmark__n { grid-column: 1 / -1; }
}
</style>
