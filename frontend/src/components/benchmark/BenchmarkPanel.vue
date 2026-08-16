<script setup lang="ts">
// BenchmarkPanel â€” PhÃ²ng ThÃ­ Nghiá»‡m Äo Hiá»‡u NÄƒng Äa Cáº¥u TrÃºc Dá»¯ Liá»‡u & Giáº£i Thuáº­t (Multi-Domain Benchmark Studio)
// Äo lÆ°á»ng thá»i gian thá»±c (ms), sá»‘ phÃ©p so sÃ¡nh, hoÃ¡n Ä‘á»•i vÃ  thao tÃ¡c bá»™ nhá»› Ä‘á»™c láº­p trÃªn Web Worker Ä‘a luá»“ng.
// Há»— trá»£ 5 ChuyÃªn Ä‘á»: Sáº¯p xáº¿p (11), TÃ¬m kiáº¿m (6), Tra cá»©u CTDL (5), Äá»“ thá»‹ (4), Chiáº¿n lÆ°á»£c Äá»‘ng (2).
import { computed, ref, watch } from 'vue';

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
  duplicatesArray,
  nearlySortedArray,
  randomArray,
  sizesForDomain,
  worstArray,
  type BenchmarkDomain,
  type BenchmarkMeasure,
} from '@/engines/benchmark/codeTemplates';
import { runMeasureInWorker } from '@/engines/worker/compileWorker';
import { useUiStore } from '@/stores/ui';
import {
  ArrowUpDown,
  Check,
  Cpu,
  Download,
  Flame,
  Info,
  Layers,
  Play,
  Scale,
  Search,
  Share2,
  Sparkles,
  Trophy,
  Zap,
} from 'lucide-vue-next';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';

// ÄÄƒng kÃ½ module ECharts tree-shaking
use([CanvasRenderer, LineChart, GridComponent, LegendComponent, TooltipComponent, LegacyGridContainLabel]);

const props = defineProps<{
  /** Keys máº·c Ä‘á»‹nh (tá»« route /benchmark/:k1/:k2) */
  defaultKeys?: string[];
}>();


/** Đọc CSS variable thành màu cụ thể (ECharts canvas không hiểu var()). */
function cssVar(name: string, fallback: string): string {
  if (typeof document === 'undefined') return fallback;
  const val = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  return val || fallback;
}

const ui = useUiStore();

// â”€â”€ Domain Management â”€â”€
const DOMAIN_TABS: Array<{ id: BenchmarkDomain; label: string; icon: any; count: number }> = [
  { id: 'sort', label: 'Sáº¯p xáº¿p Máº£ng', icon: ArrowUpDown, count: 11 },
  { id: 'search', label: 'TÃ¬m kiáº¿m Máº£ng', icon: Search, count: 6 },
  { id: 'lookup', label: 'Tra cá»©u CTDL', icon: Layers, count: 5 },
  { id: 'graph', label: 'Giáº£i thuáº­t Äá»“ thá»‹', icon: Share2, count: 4 },
  { id: 'heap_strategy', label: 'Chiáº¿n lÆ°á»£c Äá»‘ng', icon: Flame, count: 2 },
];

const activeDomain = ref<BenchmarkDomain>('sort');

// Tá»± Ä‘á»™ng suy luáº­n domain tá»« props.defaultKeys náº¿u cÃ³
if (props.defaultKeys && props.defaultKeys.length > 0) {
  const firstKey = props.defaultKeys[0];
  if (firstKey && BENCHMARK_ALGORITHMS[firstKey]) {
    activeDomain.value = BENCHMARK_ALGORITHMS[firstKey].domain;
  }
}

const selectedKeys = ref<string[]>([]);
const dataMode = ref<'random' | 'best' | 'worst' | 'nearly_sorted' | 'duplicates'>('random');
const running = ref(false);
const rows = ref<Array<{ size: number; measures: Record<string, BenchmarkMeasure | null> }>>([]);
const progress = ref('');
const error = ref('');
const saved = ref(false);

function initDomainKeys(domain: BenchmarkDomain): void {
  if (domain === 'sort') {
    selectedKeys.value = ['sort.quick_hoare', 'sort.quick', 'sort.merge', 'sort.heap'];
  } else if (domain === 'search') {
    selectedKeys.value = ['search.binary', 'search.ternary', 'search.jump', 'search.exponential'];
  } else if (domain === 'lookup') {
    selectedKeys.value = ['lookup.hashtable', 'lookup.avl', 'lookup.bst', 'lookup.array'];
  } else if (domain === 'graph') {
    selectedKeys.value = ['graph.bfs', 'graph.dfs'];
  } else {
    selectedKeys.value = ['heap.floyd', 'heap.sequential'];
  }
  rows.value = [];
  error.value = '';
}

// Khá»Ÿi táº¡o ban Ä‘áº§u
initDomainKeys(activeDomain.value);

function switchDomain(domain: BenchmarkDomain): void {
  if (activeDomain.value === domain) return;
  activeDomain.value = domain;
  initDomainKeys(domain);
}

const displayedKeys = computed(() => {
  return Object.keys(BENCHMARK_ALGORITHMS).filter(
    (k) => BENCHMARK_ALGORITHMS[k].domain === activeDomain.value,
  );
});

const sizes = computed(() => {
  const set = new Set<number>();
  for (const key of selectedKeys.value) {
    const def = BENCHMARK_ALGORITHMS[key];
    if (def) {
      for (const size of sizesForDomain(def.domain, def.complexityClass)) {
        set.add(size);
      }
    }
  }
  return [...set].sort((a, b) => a - b);
});

async function toggleKey(key: string): Promise<void> {
  const def = BENCHMARK_ALGORITHMS[key];
  if (!def) return;

  if (selectedKeys.value.includes(key)) {
    if (selectedKeys.value.length > 2) {
      selectedKeys.value = selectedKeys.value.filter((k) => k !== key);
      rows.value = [];
    } else {
      ui.showToast('Cáº§n chá»n Ã­t nháº¥t 2 thuáº­t toÃ¡n Ä‘á»ƒ so sÃ¡nh.', 'warning');
    }
  } else {
    if (selectedKeys.value.length >= 5) {
      ui.showToast('Tá»‘i Ä‘a 5 giáº£i thuáº­t cho má»—i láº§n benchmark.', 'warning');
      return;
    }
    // Náº¿u thuáº­t toÃ¡n thuá»™c domain khÃ¡c, tá»± chuyá»ƒn domain
    if (def.domain !== activeDomain.value) {
      activeDomain.value = def.domain;
      selectedKeys.value = [key];
      rows.value = [];
      ui.showToast(`ÄÃ£ chuyá»ƒn sang chuyÃªn Ä‘á» ${def.domain.toUpperCase()}.`, 'info');
      return;
    }

    selectedKeys.value.push(key);
    rows.value = [];
  }
}

// â”€â”€ Dynamic Domain Presets â”€â”€
const currentPresets = computed(() => {
  if (activeDomain.value === 'sort') {
    return [
      {
        id: 'optimal_sort',
        name: 'Sáº¯p xáº¿p Tá»‘i Æ°u',
        sub: 'Quick (Hoare) Â· Lomuto Â· Merge Â· Heap',
        badge: 'O(N log N)',
        keys: ['sort.quick_hoare', 'sort.quick', 'sort.merge', 'sort.heap'],
      },
      {
        id: 'basic_sort',
        name: 'Sáº¯p xáº¿p CÆ¡ báº£n',
        sub: 'Bubble Â· Selection Â· Insertion Â· Cocktail',
        badge: 'O(NÂ²)',
        keys: ['sort.bubble', 'sort.selection', 'sort.insertion', 'sort.cocktail'],
      },
      {
        id: 'non_comp_sort',
        name: 'Phi So SÃ¡nh Tuyáº¿n TÃ­nh',
        sub: 'Counting Â· Radix LSD vs Quick Sort',
        badge: 'O(N+K)',
        keys: ['sort.counting', 'sort.radix_lsd', 'sort.quick_hoare'],
      },
      {
        id: 'nearly_sorted',
        name: 'Máº£ng Gáº§n NhÆ° ÄÃ£ Sáº¯p',
        sub: 'Insertion vs Shell vs Quick vs Bubble',
        badge: 'O(N) vs O(NÂ²)',
        keys: ['sort.insertion', 'sort.shell', 'sort.quick_hoare', 'sort.bubble'],
      },
    ];
  }
  if (activeDomain.value === 'search') {
    return [
      {
        id: 'search_div_conquer',
        name: 'Chia Äá»ƒ Trá»‹ & Nháº£y BÆ°á»›c',
        sub: 'Binary vs Ternary vs Jump vs Exponential',
        badge: 'O(log N)',
        keys: ['search.binary', 'search.ternary', 'search.jump', 'search.exponential'],
      },
      {
        id: 'search_interp',
        name: 'Ná»™i Suy vs Tuyáº¿n TÃ­nh',
        sub: 'Interpolation vs Binary vs Linear',
        badge: 'O(log log N) vs O(N)',
        keys: ['search.interpolation', 'search.binary', 'search.linear'],
      },
    ];
  }
  if (activeDomain.value === 'lookup') {
    return [
      {
        id: 'lookup_all',
        name: 'So SÃ¡nh Tra Cá»©u ToÃ n Diá»‡n',
        sub: 'Hash Table vs AVL vs BST vs Array Scan',
        badge: 'O(1) -> O(N)',
        keys: ['lookup.hashtable', 'lookup.avl', 'lookup.bst', 'lookup.array'],
      },
      {
        id: 'lookup_tree_vs_hash',
        name: 'Báº£ng BÄƒm vs CÃ¢y CÃ¢n Báº±ng',
        sub: 'Hash Table (Chaining) vs AVL Tree vs Array',
        badge: 'O(1) vs O(log N)',
        keys: ['lookup.hashtable', 'lookup.avl', 'lookup.array'],
      },
    ];
  }
  if (activeDomain.value === 'graph') {
    return [
      {
        id: 'graph_traversal',
        name: 'Duyá»‡t Äá»“ Thá»‹ (Traversal)',
        sub: 'BFS (HÃ ng Ä‘á»£i) vs DFS (NgÄƒn xáº¿p/Äá»‡ quy)',
        badge: 'O(V+E)',
        keys: ['graph.bfs', 'graph.dfs'],
      },
      {
        id: 'graph_shortest_path',
        name: 'TÃ¬m ÄÆ°á»ng Äi Ngáº¯n Nháº¥t',
        sub: 'Dijkstra (Min-Heap) vs Dijkstra (Matrix)',
        badge: 'O((V+E)log V) vs O(VÂ²)',
        keys: ['graph.dijkstra_heap', 'graph.dijkstra_matrix'],
      },
    ];
  }
  return [
    {
      id: 'heap_build',
      name: 'XÃ¢y Äá»‘ng (Heap Construction)',
      sub: 'Floyd Heapify (Bottom-up) vs N láº§n ChÃ¨n',
      badge: 'O(N) vs O(N log N)',
      keys: ['heap.floyd', 'heap.sequential'],
    },
  ];
});

function applyPreset(keys: string[]): void {
  selectedKeys.value = [...keys];
  rows.value = [];
  error.value = '';
}

function inputFor(mode: typeof dataMode.value, size: number): number[] {
  if (mode === 'worst') return worstArray(size);
  if (mode === 'best') return bestArray(size);
  if (mode === 'nearly_sorted') return nearlySortedArray(size);
  if (mode === 'duplicates') return duplicatesArray(size);
  return randomArray(size);
}

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
    ui.showToast('Chá»n Ã­t nháº¥t 2 thuáº­t toÃ¡n Ä‘á»ƒ so sÃ¡nh.', 'warning');
    return;
  }
  running.value = true;
  error.value = '';
  saved.value = false;
  rows.value = [];
  try {
    for (const size of sizes.value) {
      progress.value = `Äang Ä‘o máº«u n = ${size}...`;
      const measures: Record<string, BenchmarkMeasure | null> = {};
      for (const key of selectedKeys.value) {
        const def = BENCHMARK_ALGORITHMS[key];
        if (!def) continue;
        const input = inputFor(dataMode.value, size);
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
          measures[key] = null;
        }
        await new Promise((resolve) => setTimeout(resolve, 0));
      }
      rows.value = [...rows.value, { size, measures }];
    }
    progress.value = 'HoÃ n táº¥t Ä‘o Ä‘áº¡c.';
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

function exportCsv(): void {
  if (rows.value.length === 0) return;
  const header = ['n', ...selectedKeys.value.map((k) => `${BENCHMARK_ALGORITHMS[k]?.title ?? k} (ms)`)].join(',');
  const lines = rows.value.map((r) => {
    const vals = selectedKeys.value.map((k) => {
      const m = r.measures[k];
      return m ? String(m.durationMs) : 'N/A';
    });
    return [String(r.size), ...vals].join(',');
  });
  const blob = new Blob([[header, ...lines].join('\n')], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `benchmark-${activeDomain.value}-${Date.now()}.csv`;
  a.click();
  URL.revokeObjectURL(url);
}

// â”€â”€ Káº¿t luáº­n & PhÃ¢n tÃ­ch thÃ´ng minh â”€â”€
const analysis = computed(() => {
  if (rows.value.length === 0) return null;
  // TÃ¬m hÃ ng cÃ³ kÃ­ch thÆ°á»›c máº«u lá»›n nháº¥t mÃ  cÃ³ Ã­t nháº¥t 1 thuáº­t toÃ¡n Ä‘o thÃ nh cÃ´ng
  const validRows = [...rows.value].reverse();
  let targetRow = validRows.find((r) => selectedKeys.value.some((k) => r.measures[k] !== null)) ?? rows.value[rows.value.length - 1];

  const sortedBySpeed = [...selectedKeys.value]
    .filter((k) => targetRow.measures[k] !== null)
    .sort((a, b) => (targetRow.measures[a]?.durationMs ?? Infinity) - (targetRow.measures[b]?.durationMs ?? Infinity));

  if (sortedBySpeed.length === 0) return null;

  const winnerKey = sortedBySpeed[0];
  const winner = winnerKey ? BENCHMARK_ALGORITHMS[winnerKey] : null;
  const winnerMeasure = winnerKey ? targetRow.measures[winnerKey] : null;

  const slowestKey = sortedBySpeed[sortedBySpeed.length - 1];
  const slowest = slowestKey && slowestKey !== winnerKey ? BENCHMARK_ALGORITHMS[slowestKey] : null;
  const slowestMeasure = slowestKey ? targetRow.measures[slowestKey] : null;

  let speedRatio = 1;
  if (winnerMeasure && slowestMeasure && winnerMeasure.durationMs > 0) {
    speedRatio = Math.max(1, Math.round((slowestMeasure.durationMs / winnerMeasure.durationMs) * 10) / 10);
  }

  return {
    winner,
    winnerMeasure,
    slowest,
    slowestMeasure,
    speedRatio,
    maxN: targetRow.size,
  };
});

function prefersReducedMotion(): boolean {
  return (
    typeof window !== 'undefined' &&
    typeof window.matchMedia === 'function' &&
    window.matchMedia('(prefers-reduced-motion: reduce)').matches
  );
}

const LINE_COLORS = ['#38bdf8', '#34d399', '#f43f5e', '#fbbf24', '#a855f7'];

function paletteColor(idx: number): string {
  return LINE_COLORS[idx % LINE_COLORS.length];
}

const chartOption = computed(() => {
  void ui.theme;
  const textColor = cssVar('--color-index-muted', '#6B7385');
  const gridLineColor = cssVar('--color-border-subtle', 'rgba(255, 255, 255, 0.07)');
  const tooltipBg = cssVar('--color-canvas-ink', '#0F172A');

  const series = selectedKeys.value.map((key, idx) => ({
    name: BENCHMARK_ALGORITHMS[key]?.title ?? key,
    type: 'line' as const,
    smooth: true,
    symbol: 'circle',
    symbolSize: 8,
    lineStyle: { width: 3, color: paletteColor(idx) },
    itemStyle: { color: paletteColor(idx) },
    data: rows.value.map((row) =>
      row.measures[key] ? [String(row.size), (row.measures[key] as BenchmarkMeasure).durationMs] : null,
    ),
    connectNulls: false,
  }));

  return {
    backgroundColor: 'transparent',
    color: LINE_COLORS,
    animation: !prefersReducedMotion(),
    animationDuration: 400,
    tooltip: {
      trigger: 'axis' as const,
      backgroundColor: tooltipBg,
      borderColor: 'rgba(255, 255, 255, 0.15)',
      textStyle: { color: '#f8fafc', fontSize: 12 },
    },
    legend: {
      top: 6,
      icon: 'roundRect',
      itemWidth: 14,
      itemHeight: 6,
      textStyle: { color: '#cbd5e1', fontSize: 12 },
    },
    grid: { left: 45, right: 24, top: 48, bottom: 36, containLabel: true },
    xAxis: {
      type: 'category' as const,
      name: activeDomain.value === 'graph' ? 'V (Ä‘á»‰nh)' : 'n (kÃ­ch thÆ°á»›c)',
      nameLocation: 'middle' as const,
      nameGap: 24,
      nameTextStyle: { color: textColor, fontSize: 11 },
      axisLine: { lineStyle: { color: gridLineColor } },
      axisTick: { lineStyle: { color: gridLineColor } },
      axisLabel: { color: textColor, fontSize: 11 },
      data: rows.value.map((r) => String(r.size)),
    },
    yAxis: {
      type: 'value' as const,
      name: 'Thá»i gian (ms)',
      nameTextStyle: { color: textColor, fontSize: 11 },
      axisLine: { show: false },
      axisTick: { show: false },
      axisLabel: { color: textColor, fontSize: 11 },
      splitLine: { lineStyle: { color: gridLineColor } },
      min: 0,
    },
    series,
  };
});

function complexityColorClass(cls: string): string {
  if (cls.includes('nÂ²') || cls.includes('VÂ²')) return 'badge--n2';
  if (cls.includes('log') || cls.includes('n^1.3')) return 'badge--nlogn';
  if (cls.includes('O(1)')) return 'badge--o1';
  if (cls.includes('n') || cls.includes('V+E')) return 'badge--n';
  return 'badge--log';
}
</script>

<template>
  <div class="benchmark-studio">
    <!-- â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• -->
    <!-- TOP DOMAIN SELECTOR BAR                                              -->
    <!-- â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• -->
    <div class="benchmark-domain-bar">
      <button
        v-for="tab in DOMAIN_TABS"
        :key="tab.id"
        type="button"
        class="domain-tab-btn"
        :class="{ 'domain-tab-btn--active': activeDomain === tab.id }"
        @click="switchDomain(tab.id)"
      >
        <component :is="tab.icon" :size="15" />
        <span>{{ tab.label }}</span>
        <span class="domain-tab-count">({{ tab.count }})</span>
      </button>
    </div>

    <!-- â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• -->
    <!-- HEADER BAR                                                           -->
    <!-- â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• -->
    <header class="benchmark-studio__header">
      <div class="benchmark-studio__header-left">
        <div class="benchmark-studio__icon-box">
          <Scale :size="20" />
        </div>
        <div>
          <h2 class="benchmark-studio__title">
            PhÃ²ng ThÃ­ Nghiá»‡m Hiá»‡u NÄƒng (Benchmark Studio)
          </h2>
          <p class="benchmark-studio__subtitle">
            Äo lÆ°á»ng thá»i gian thá»±c (ms), sá»‘ phÃ©p so sÃ¡nh vÃ  thao tÃ¡c bá»™ nhá»› Ä‘á»™c láº­p trÃªn Web Worker Ä‘a luá»“ng.
          </p>
        </div>
      </div>

      <div class="benchmark-studio__badges">
        <span class="benchmark-studio__badge">
          <Cpu :size="13" /> Web Worker Sandbox
        </span>
        <span class="benchmark-studio__badge benchmark-studio__badge--free">
          <Zap :size="13" /> KhÃ´ng tá»‘n tim
        </span>
      </div>
    </header>

    <!-- â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• -->
    <!-- MAIN CONTENT BODY                                                    -->
    <!-- â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â•â• -->
    <div class="benchmark-studio__content">
      <!-- â”€â”€ Táº¦NG 1: Cáº¤U HÃŒNH & CHá»ŒN THUáº¬T TOÃN (CONFIG DECK) â”€â”€ -->
      <section class="config-deck">
        <!-- 1.1: Quick Presets -->
        <div class="config-deck__presets">
          <div class="config-deck__presets-header">
            <span class="config-deck__presets-label">
              <Sparkles :size="13" class="text-amber-400" />
              Ká»ŠCH Báº¢N SO SÃNH Gá»¢I Ã:
            </span>
          </div>

          <div class="preset-cards">
            <button
              v-for="preset in currentPresets"
              :key="preset.id"
              type="button"
              class="preset-card"
              @click="applyPreset(preset.keys)"
            >
              <div class="preset-card__icon">
                <Zap :size="16" class="text-amber-400" />
              </div>
              <div class="preset-card__info">
                <span class="preset-card__name">{{ preset.name }}</span>
                <span class="preset-card__sub">
                  {{ preset.sub }} <code class="mono-badge">{{ preset.badge }}</code>
                </span>
              </div>
            </button>
          </div>
        </div>

        <!-- 1.2: Algorithm Selection Grid -->
        <div class="config-deck__matrix">
          <div class="config-deck__matrix-header">
            <div class="config-deck__matrix-title">
              Chá»n thuáº­t toÃ¡n cáº§n Ä‘o (chá»n tá»« 2 Ä‘áº¿n 5 giáº£i thuáº­t):
              <span class="config-deck__counter">({{ selectedKeys.length }}/5 Ä‘Ã£ chá»n)</span>
            </div>
          </div>

          <div class="algo-grid">
            <button
              v-for="key in displayedKeys"
              :key="key"
              type="button"
              class="algo-pill"
              :class="{ 'algo-pill--selected': selectedKeys.includes(key) }
"
              @click="toggleKey(key)"
            >
              <div class="algo-pill__left">
                <span class="algo-pill__check">
                  <Check v-if="selectedKeys.includes(key)" :size="13" />
                </span>
                <span class="algo-pill__name">{{ BENCHMARK_ALGORITHMS[key]?.title }}</span>
              </div>
              <span
                class="algo-pill__badge"
                :class="complexityColorClass(BENCHMARK_ALGORITHMS[key]?.complexityClass ?? '')"
              >
                {{ BENCHMARK_ALGORITHMS[key]?.complexityClass }}
              </span>
            </button>
          </div>
        </div>

        <!-- 1.3: Action Ribbon (Data Distribution + Run Button) -->
        <div class="config-deck__ribbon">
          <div class="config-deck__dist-wrap">
            <label class="config-deck__dist-label" for="benchmark-data-mode">
              Ká»‹ch báº£n dá»¯ liá»‡u:
            </label>
            <select id="benchmark-data-mode" v-model="dataMode" class="config-deck__select">
              <option value="random">ðŸŽ² Máº£ng ngáº«u nhiÃªn (Random)</option>
              <option value="best">ðŸ“ˆ Tá»‘t nháº¥t / TÄƒng dáº§n (Best Case)</option>
              <option value="worst">ðŸ“‰ Xáº¥u nháº¥t / Giáº£m dáº§n (Worst Case)</option>
              <option value="nearly_sorted">ðŸ”„ Gáº§n nhÆ° Ä‘Ã£ sáº¯p (Nearly Sorted 95%)</option>
              <option value="duplicates">ðŸ‘¯ Nhiá»u pháº§n tá»­ trÃ¹ng (Duplicates)</option>
            </select>
          </div>

          <div class="config-deck__actions">
            <Button
              variant="primary"
              size="sm"
              class="run-btn"
              :disabled="running || selectedKeys.length < 2"
              @click="run"
            >
              <Play :size="14" class="fill-current mr-1.5" />
              {{ running ? 'Äang Ä‘o lÆ°á»ng...' : 'Báº¯t Ä‘áº§u Ä‘o hiá»‡u nÄƒng' }}
            </Button>

            <Button
              v-if="rows.length > 0"
              variant="secondary"
              size="sm"
              @click="exportCsv"
            >
              <Download :size="14" class="mr-1.5" /> Xuáº¥t CSV
            </Button>
          </div>

          <div v-if="running" class="config-deck__progress-bar">
            <div class="config-deck__progress-spinner" />
            <span class="config-deck__progress-text">{{ progress }}</span>
          </div>
        </div>
      </section>

      <!-- â”€â”€ Táº¦NG 2: BÃO CÃO & TRá»°C QUAN HÃ“A Sá» LIá»†U (RESULTS & ANALYTICS) â”€â”€ -->

      <!-- 2A: Preview State (Before running) -->
      <div v-if="rows.length === 0 && !running" class="benchmark-preview">
        <div class="benchmark-preview__header">
          <div class="benchmark-preview__icon">
            <Info :size="18" class="text-primary" />
          </div>
          <div>
            <h4 class="benchmark-preview__title">Báº£ng Äá»‘i Chiáº¿u Äá»™ Phá»©c Táº¡p LÃ½ Thuyáº¿t</h4>
            <p class="benchmark-preview__desc">
              CÃ¡c thuáº­t toÃ¡n Ä‘Ã£ chá»n sáº½ Ä‘Æ°á»£c biÃªn dá»‹ch vÃ  Ä‘o Ä‘áº¡c qua Web Worker vá»›i cÃ¡c kÃ­ch thÆ°á»›c máº«u:
              <strong class="text-foreground">{{ activeDomain === 'graph' ? 'V' : 'n' }} = {{ sizes.join(', ') }}</strong>.
            </p>
          </div>
        </div>

        <div class="benchmark-preview__table-wrap">
          <table class="benchmark-preview__table">
            <thead>
              <tr>
                <th>Thuáº­t toÃ¡n</th>
                <th>Ká»³ vá»ng Thá»i gian</th>
                <th>Bá»™ nhá»› phá»¥ (Space)</th>
                <th>TrÆ°á»ng há»£p tá»‘t nháº¥t</th>
                <th>TrÆ°á»ng há»£p xáº¥u nháº¥t</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="key in selectedKeys" :key="key">
                <td class="font-medium text-foreground">
                  {{ BENCHMARK_ALGORITHMS[key]?.title }}
                </td>
                <td>
                  <span class="algo-pill__badge" :class="complexityColorClass(BENCHMARK_ALGORITHMS[key]?.complexityClass ?? '')">
                    {{ BENCHMARK_ALGORITHMS[key]?.complexityClass }}
                  </span>
                </td>
                <td class="font-mono text-xs text-muted-foreground">
                  {{ BENCHMARK_ALGORITHMS[key]?.spaceComplexity }}
                </td>
                <td class="font-mono text-xs text-emerald-400">
                  {{ BENCHMARK_ALGORITHMS[key]?.bestCase }}
                </td>
                <td class="font-mono text-xs text-rose-400">
                  {{ BENCHMARK_ALGORITHMS[key]?.worstCase }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- 2B: Results Dashboard (After running) -->
      <div v-else-if="rows.length > 0" class="benchmark-results">
        <!-- 2.1: Winner Hero Banner (Analysis Summary) -->
        <div v-if="analysis && analysis.winner" class="analysis-hero">
          <div class="analysis-hero__left">
            <div class="analysis-hero__trophy">
              <Trophy :size="24" class="text-amber-400" />
            </div>
            <div>
              <span class="analysis-hero__kicker">
                Káº¾T QUáº¢ Tá»”NG QUAN Táº I {{ activeDomain === 'graph' ? 'V' : 'N' }} = {{ analysis.maxN }}
              </span>
              <h3 class="analysis-hero__winner-title">
                ðŸ† <strong class="text-emerald-400">{{ analysis.winner.title }}</strong> nhanh nháº¥t!
              </h3>
              <p class="analysis-hero__desc">
                HoÃ n thÃ nh chá»‰ trong <span class="text-emerald-400 font-mono font-bold">{{ analysis.winnerMeasure?.durationMs }}ms</span>
                vá»›i <span class="text-foreground font-mono">{{ analysis.winnerMeasure?.comparisons }}</span> phÃ©p so sÃ¡nh
                <template v-if="analysis.slowest && analysis.speedRatio > 1">
                  (nhanh hÆ¡n <span class="text-amber-400 font-bold font-mono">{{ analysis.speedRatio }}x</span> so vá»›i {{ analysis.slowest.title }}).
                </template>
              </p>
            </div>
          </div>

          <div class="analysis-hero__badge">
            <span class="text-xs text-muted-foreground">Dá»¯ liá»‡u thá»­ nghiá»‡m</span>
            <span class="text-sm font-semibold capitalize text-foreground">{{ dataMode }}</span>
          </div>
        </div>

        <!-- 2.2: 2-Column Analytics Grid (Chart + Metric Table) -->
        <div class="analytics-grid">
          <!-- Column 1: ECharts Line Graph -->
          <div class="analytics-card">
            <div class="analytics-card__header">
              <h3 class="analytics-card__title">Äá»“ Thá»‹ TÄƒng TrÆ°á»Ÿng Thá»i Gian (ms theo {{ activeDomain === 'graph' ? 'V' : 'n' }})</h3>
              <span class="analytics-card__sub">Overlay so sÃ¡nh trá»±c quan</span>
            </div>
            <div class="analytics-card__chart-wrap">
              <VChart
                :option="chartOption"
                autoresize
                class="analytics-chart"
                role="img"
                aria-label="Biá»ƒu Ä‘á»“ benchmark thá»i gian thá»±c táº¿ theo n"
              />
            </div>
          </div>

          <!-- Column 2: Data Matrix Table -->
          <div class="analytics-card">
            <div class="analytics-card__header">
              <h3 class="analytics-card__title">Báº£ng Sá»‘ Liá»‡u Chi Tiáº¿t</h3>
              <span class="analytics-card__sub">Thá»i gian ms & Sá»‘ phÃ©p so sÃ¡nh</span>
            </div>
            <div class="table-container">
              <table class="data-table">
                <thead>
                  <tr>
                    <th class="data-table__th-n">Máº«u</th>
                    <th v-for="key in selectedKeys" :key="key">
                      {{ BENCHMARK_ALGORITHMS[key]?.title }}
                    </th>
                  </tr>
                </thead>
                <tbody>
                  <tr v-for="row in rows" :key="row.size">
                    <td class="data-table__n font-mono font-bold">
                      {{ activeDomain === 'graph' ? 'V' : 'n' }}={{ row.size }}
                    </td>
                    <td v-for="key in selectedKeys" :key="key" class="data-table__cell-td">
                      <div v-if="row.measures[key]" class="data-table__cell">
                        <span class="data-table__time font-mono">
                          {{ row.measures[key]?.durationMs }}ms
                        </span>
                        <span class="data-table__ops font-mono">
                          {{ row.measures[key]?.comparisons }} cmp
                        </span>
                      </div>
                      <span v-else class="text-muted-foreground text-xs font-mono">N/A</span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.benchmark-studio {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

/* â”€â”€ Top Domain Selector Bar â”€â”€ */
.benchmark-domain-bar {
  display: flex;
  gap: 6px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: 4px;
  overflow-x: auto;
}

.domain-tab-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 14px;
  border-radius: var(--radius-md);
  border: 1px solid transparent;
  background: transparent;
  color: var(--color-text-secondary);
  font-size: var(--text-xs);
  font-weight: 600;
  cursor: pointer;
  white-space: nowrap;
  transition: all var(--duration-fast) var(--ease-out);
}

.domain-tab-btn:hover {
  background: var(--color-surface-hover);
  color: var(--color-foreground);
}

.domain-tab-btn--active {
  background: var(--color-primary-subtle);
  color: var(--color-primary);
  border-color: rgba(56, 189, 248, 0.3);
}

.domain-tab-count {
  font-size: 11px;
  opacity: 0.7;
}

/* â”€â”€ Header â”€â”€ */
.benchmark-studio__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-md) var(--space-lg);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  gap: var(--space-md);
  flex-wrap: wrap;
}

.benchmark-studio__header-left {
  display: flex;
  align-items: center;
  gap: var(--space-md);
}

.benchmark-studio__icon-box {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: var(--color-primary-subtle);
  color: var(--color-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.benchmark-studio__title {
  font-size: var(--text-xl);
  font-weight: 700;
  letter-spacing: -0.02em;
  color: var(--color-foreground);
  margin: 0;
}

.benchmark-studio__subtitle {
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
  margin: 2px 0 0;
}

.benchmark-studio__badges {
  display: flex;
  gap: var(--space-xs);
}

.benchmark-studio__badge {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  font-size: 11px;
  font-weight: 600;
  padding: 4px 10px;
  border-radius: var(--radius-full);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  color: var(--color-text-secondary);
}

.benchmark-studio__badge--free {
  background: rgba(16, 185, 129, 0.1);
  color: #10b981;
  border-color: rgba(16, 185, 129, 0.25);
}

/* â”€â”€ Content Body â”€â”€ */
.benchmark-studio__content {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* â”€â”€ Config Deck (Táº§ng Cáº¥u hÃ¬nh) â”€â”€ */
.config-deck {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-lg);
}

/* 1.1: Presets */
.config-deck__presets {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.config-deck__presets-header {
  display: flex;
  align-items: center;
}

.config-deck__presets-label {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 11px;
  font-weight: 700;
  letter-spacing: 0.04em;
  color: var(--color-text-secondary);
}

.preset-cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: var(--space-sm);
}

.preset-card {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--color-border);
  background: var(--color-card-raised);
  border-radius: var(--radius-md);
  cursor: pointer;
  text-align: left;
  transition: all var(--duration-fast) var(--ease-out);
}

.preset-card:hover {
  border-color: var(--color-primary);
  background: var(--color-primary-subtle);
  transform: translateY(-1px);
}

.preset-card__icon {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-sm);
  background: rgba(251, 191, 36, 0.12);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.preset-card__info {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.preset-card__name {
  font-size: var(--text-xs);
  font-weight: 600;
  color: var(--color-foreground);
}

.preset-card__sub {
  font-size: 10px;
  color: var(--color-text-secondary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.mono-badge {
  font-family: var(--font-mono);
  font-size: 10px;
  background: rgba(255, 255, 255, 0.06);
  padding: 1px 4px;
  border-radius: var(--radius-xs);
  color: var(--color-primary);
}

/* 1.2: Matrix */
.config-deck__matrix {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  padding-top: var(--space-xs);
}

.config-deck__matrix-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: var(--space-sm);
}

.config-deck__matrix-title {
  font-size: var(--text-xs);
  font-weight: 600;
  color: var(--color-text-secondary);
}

.config-deck__counter {
  color: var(--color-primary);
  font-weight: 700;
}

.algo-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: var(--space-xs);
}

.algo-pill {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 6px 10px;
  border: 1px solid var(--color-border);
  background: var(--color-card-raised);
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all var(--duration-fast) var(--ease-out);
}

.algo-pill:hover {
  border-color: var(--color-primary);
}

.algo-pill--selected {
  background: var(--color-primary-subtle);
  border-color: var(--color-primary);
}

.algo-pill__left {
  display: flex;
  align-items: center;
  gap: 8px;
  min-width: 0;
}

.algo-pill__check {
  width: 16px;
  height: 16px;
  border-radius: 4px;
  border: 1.5px solid var(--color-border);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-primary);
  background: var(--color-surface);
  flex-shrink: 0;
}

.algo-pill--selected .algo-pill__check {
  border-color: var(--color-primary);
  background: var(--color-primary);
  color: #0b0f19;
}

.algo-pill__name {
  font-size: 11px;
  font-weight: 600;
  color: var(--color-foreground);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.algo-pill__badge {
  font-family: var(--font-mono);
  font-size: 10px;
  font-weight: 600;
  padding: 2px 6px;
  border-radius: var(--radius-sm);
  border: 1px solid transparent;
  flex-shrink: 0;
}

.badge--n2 {
  background: rgba(244, 63, 94, 0.1);
  color: #f43f5e;
  border-color: rgba(244, 63, 94, 0.25);
}

.badge--nlogn {
  background: rgba(56, 189, 248, 0.1);
  color: #38bdf8;
  border-color: rgba(56, 189, 248, 0.25);
}

.badge--n {
  background: rgba(168, 85, 247, 0.1);
  color: #a855f7;
  border-color: rgba(168, 85, 247, 0.25);
}

.badge--log {
  background: rgba(52, 211, 153, 0.1);
  color: #34d399;
  border-color: rgba(52, 211, 153, 0.25);
}

.badge--o1 {
  background: rgba(251, 191, 36, 0.1);
  color: #fbbf24;
  border-color: rgba(251, 191, 36, 0.25);
}

/* 1.3: Ribbon */
.config-deck__ribbon {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-md);
  padding-top: var(--space-md);
  border-top: 1px solid var(--color-border-subtle);
  flex-wrap: wrap;
}

.config-deck__dist-wrap {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.config-deck__dist-label {
  font-size: var(--text-xs);
  font-weight: 600;
  color: var(--color-text-secondary);
}

.config-deck__select {
  height: 38px;
  padding: 0 var(--space-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-card-raised);
  color: var(--color-foreground);
  font-size: var(--text-xs);
  font-weight: 500;
  cursor: pointer;
}

.config-deck__actions {
  display: flex;
  gap: var(--space-sm);
  align-items: center;
}

.run-btn {
  font-weight: 600;
}

.config-deck__progress-bar {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  background: var(--color-primary-subtle);
  border: 1px solid var(--color-primary);
  border-radius: var(--radius-md);
}

.config-deck__progress-spinner {
  width: 16px;
  height: 16px;
  border: 2px solid var(--color-primary);
  border-top-color: transparent;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

.config-deck__progress-text {
  font-size: var(--text-xs);
  font-family: var(--font-mono);
  color: var(--color-primary);
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

/* â”€â”€ Táº¦NG 2: PREVIEW STATE (Báº¢NG Äá»I CHIáº¾U LÃ THUYáº¾T) â”€â”€ */
.benchmark-preview {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.benchmark-preview__header {
  display: flex;
  align-items: flex-start;
  gap: var(--space-md);
}

.benchmark-preview__icon {
  width: 36px;
  height: 36px;
  border-radius: var(--radius-md);
  background: var(--color-primary-subtle);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.benchmark-preview__title {
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-foreground);
  margin: 0;
}

.benchmark-preview__desc {
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
  margin: 2px 0 0;
}

.benchmark-preview__table-wrap {
  overflow-x: auto;
  border-radius: var(--radius-md);
  border: 1px solid var(--color-border-subtle);
}

.benchmark-preview__table {
  width: 100%;
  border-collapse: collapse;
  font-size: var(--text-xs);
}

.benchmark-preview__table th,
.benchmark-preview__table td {
  padding: 10px 14px;
  text-align: left;
  border-bottom: 1px solid var(--color-border-subtle);
}

.benchmark-preview__table th {
  background: var(--color-card-raised);
  color: var(--color-text-secondary);
  font-weight: 600;
  font-size: 11px;
}

/* â”€â”€ Táº¦NG 2: LIVE RESULTS DASHBOARD â”€â”€ */
.benchmark-results {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

/* 2.1: Winner Hero */
.analysis-hero {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-md) var(--space-lg);
  background: linear-gradient(135deg, rgba(16, 185, 129, 0.12), rgba(6, 78, 59, 0.25));
  border: 1px solid rgba(16, 185, 129, 0.35);
  border-radius: var(--radius-lg);
  gap: var(--space-md);
  flex-wrap: wrap;
}

.analysis-hero__left {
  display: flex;
  align-items: center;
  gap: var(--space-md);
}

.analysis-hero__trophy {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: rgba(251, 191, 36, 0.15);
  border: 1px solid rgba(251, 191, 36, 0.3);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.analysis-hero__kicker {
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.05em;
  color: #34d399;
  display: block;
}

.analysis-hero__winner-title {
  font-size: var(--text-base);
  font-weight: 700;
  color: #f8fafc;
  margin: 2px 0;
}

.analysis-hero__desc {
  font-size: var(--text-xs);
  color: #94a3b8;
  margin: 0;
}

.analysis-hero__badge {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  background: rgba(0, 0, 0, 0.25);
  padding: 6px 12px;
  border-radius: var(--radius-md);
  border: 1px solid rgba(255, 255, 255, 0.08);
}

/* 2.2: 2-Column Analytics Grid */
.analytics-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-md);
}

.analytics-card {
  background: #0b0f19;
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: var(--radius-lg);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  box-shadow: var(--shadow-sm);
}

.analytics-card__header {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
}

.analytics-card__title {
  font-size: var(--text-sm);
  font-weight: 600;
  color: #f8fafc;
  margin: 0;
}

.analytics-card__sub {
  font-size: 11px;
  color: #64748b;
}

.analytics-card__chart-wrap {
  height: 320px;
  width: 100%;
}

.analytics-chart {
  width: 100%;
  height: 100%;
}

/* Metric Table */
.table-container {
  overflow-x: auto;
  border-radius: var(--radius-md);
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  font-size: var(--text-xs);
  table-layout: fixed;
}

.data-table th,
.data-table td {
  padding: 8px 10px;
  text-align: left;
  vertical-align: middle;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.data-table__th-n {
  width: 75px;
}

.data-table th {
  color: #94a3b8;
  font-size: 11px;
  font-weight: 600;
  background: rgba(255, 255, 255, 0.03);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.data-table__n {
  color: #38bdf8;
  font-size: 11px;
}

.data-table__cell {
  display: flex;
  flex-direction: column;
  gap: 1px;
}

.data-table__time {
  font-weight: 600;
  color: #f8fafc;
  font-size: 12px;
}

.data-table__ops {
  color: #64748b;
  font-size: 10px;
}

@media (max-width: 640px) {
  .analytics-grid {
    grid-template-columns: 1fr;
  }
}
</style>
