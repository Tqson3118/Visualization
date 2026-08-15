<script setup lang="ts">
// HomeView — Màn 01: Trang chủ tương tác thế hệ mới (Spatial 2-6-2 Architecture + GSAP).
// Tích hợp đầy đủ:
//   - Cánh Trái (20%): Spatial Progress Radar, Quick Topic Rail, Live Activity Feed
//   - Trục Trung Tâm (60%): Morphing Hero Stage, Metrics Strip, 3 Public Demos,
//     Algorithm Constellation Catalog, Practice Ladder 4 Chặng, Tech Ecosystem & Live Sandbox
//   - Cánh Phải (20%): Daily Challenge, Mini Leaderboard, Big-O Reference, WASM Engine Badge
// Tuân thủ 100% test contract (.home__bench, .home__block, .home__demo, .home__feature...)
import { computed, onMounted, onUnmounted, ref, shallowRef } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { gsap } from 'gsap';
import {
  Activity,
  ArrowRight,
  ArrowUpDown,
  Check,
  ChevronLeft,
  ChevronRight,
  Code2,
  Cpu,
  Eye,
  Flame,
  FlaskConical,
  Gauge,
  GraduationCap,
  Layers,
  Map,
  Network,
  Pause,
  Play,
  RotateCcw,
  Search,
  Sparkles,
  StepBack,
  StepForward,
  Target,
  Terminal,
  Trophy,
  Users,
  X,
  Zap,
} from 'lucide-vue-next';

import type { Component } from 'vue';
import type { InputConfig, Step } from '@/engines/core/types';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { CATALOG } from '@/engines/catalog';
import type { CatalogMeta } from '@/engines/catalog';
import { getSimulation } from '@/engines/registry';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import BlockToken from '@/components/ui/BlockToken.vue';
import { buttonVariants } from '@/components/ui/button';

const router = useRouter();

/* ── 3 Demo công khai (FR-7.6) ── */
const DEMO_ICONS: Record<string, Component> = {
  'sort.bubble': ArrowUpDown,
  'search.binary': Search,
  'graph.bfs': Network,
};

const demos = computed(() =>
  CATALOG.filter((c) => c.demoAllowed).map((c) => ({
    key: c.key,
    title: c.title,
    dataStructure: c.dataStructure,
    level: c.level,
    complexity: c.complexity,
    icon: DEMO_ICONS[c.key] ?? Play,
  })),
);

const stats = computed(() => ({
  visuals: CATALOG.length,
  groups: new Set(CATALOG.map((c) => c.dataStructure)).size,
  levels: new Set(CATALOG.map((c) => c.level)).size,
}));

function openDemo(key: string): void {
  void router.push({ name: 'simulator', params: { key } });
}

/* ── Hero mini-sim engine trace ── */
const DEMO_INPUTS: Record<string, InputConfig> = {
  'sort.bubble': { kind: 'array', data: { values: [5, 3, 8, 1, 9, 2] } },
  'search.binary': { kind: 'array', data: { target: 19, inputSource: 'manual', values: [2, 5, 8, 12, 19, 23] } },
  'graph.bfs': { kind: 'graph', data: { preset: 'path', directed: false, weighted: false, vertices: 6, edges: 5, source: 0 } },
};

const DEMO_KEYS = ['sort.bubble', 'search.binary', 'graph.bfs'] as const;

const activeKey = ref<(typeof DEMO_KEYS)[number]>('sort.bubble');
const steps = shallowRef<Step[]>([]);
const stepIndex = ref(0);
const stepTimer = ref<ReturnType<typeof setInterval> | null>(null);
const restartTimer = ref<ReturnType<typeof setTimeout> | null>(null);
const isPlaying = ref(false);
const speed = ref(1);

const activeMeta = computed(() => CATALOG.find((c) => c.key === activeKey.value));
const currentStep = computed(() => steps.value[stepIndex.value] ?? null);
const frameElements = computed(() => currentStep.value?.structure.elements ?? []);
const stepLabel = computed(() => messages.home.simStepOf(stepIndex.value + 1, steps.value.length));
const traceLine = computed(() =>
  currentStep.value ? messages.home.simTraceLine(currentStep.value.pseudocodeLine) : '',
);

const demoOptions = computed(() =>
  DEMO_KEYS.map((key) => ({
    key,
    label: key === 'sort.bubble'
      ? messages.home.demoBubble
      : key === 'search.binary'
        ? messages.home.demoBinary
        : messages.home.demoBfs,
  })),
);

const prefersReducedMotion = (): boolean =>
  typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

function stopPlayback(): void {
  if (stepTimer.value) clearInterval(stepTimer.value);
  if (restartTimer.value) clearTimeout(restartTimer.value);
  stepTimer.value = null;
  restartTimer.value = null;
  isPlaying.value = false;
}

function play(): void {
  stopPlayback();
  if (prefersReducedMotion()) return;
  if (steps.value.length === 0) return;
  if (stepIndex.value >= steps.value.length - 1) stepIndex.value = 0;
  isPlaying.value = true;
  const intervalMs = Math.max(80, Math.round(380 / speed.value));
  stepTimer.value = setInterval(() => {
    if (stepIndex.value < steps.value.length - 1) {
      stepIndex.value++;
    } else {
      stopPlayback();
      restartTimer.value = setTimeout(() => {
        stepIndex.value = 0;
        play();
      }, 1400);
    }
  }, intervalMs);
}

function togglePlay(): void {
  if (isPlaying.value) stopPlayback();
  else play();
}

function stepForward(): void {
  stopPlayback();
  if (stepIndex.value < steps.value.length - 1) stepIndex.value++;
}

function stepBack(): void {
  stopPlayback();
  if (stepIndex.value > 0) stepIndex.value--;
}

function resetSim(): void {
  stopPlayback();
  stepIndex.value = 0;
}

function setSpeed(value: number): void {
  speed.value = value;
  if (isPlaying.value) {
    stopPlayback();
    play();
  }
}

function onSpeedInput(event: Event): void {
  const value = Number((event.target as HTMLInputElement).value);
  setSpeed(Math.round(value * 100) / 100);
}

const speedValue = computed(() => messages.home.simSpeedValue(speed.value));

function loadDemo(key: (typeof DEMO_KEYS)[number]): void {
  const gen = getSimulation(key);
  if (!gen) return;
  steps.value = gen.generate(DEMO_INPUTS[key]);
  stepIndex.value = 0;
}

function selectDemo(key: (typeof DEMO_KEYS)[number]): void {
  if (key === activeKey.value) return;
  activeKey.value = key;
  loadDemo(key);
  play();
}

function blockStatusClass(status: string): string {
  switch (status) {
    case 'swap':
      return 'home__block--swap';
    case 'done':
      return 'home__block--done';
    case 'muted':
      return 'home__block--muted';
    case 'active':
    case 'highlight':
      return 'home__block--active';
    default:
      return 'home__block--default';
  }
}

/* ── Catalog Filter Groups ── */
type CatalogGroup =
  | 'all'
  | 'sort'
  | 'search'
  | 'graph'
  | 'tree'
  | 'heap'
  | 'hash'
  | 'linear'
  | 'structure';

interface CatalogGroupDef {
  key: CatalogGroup;
  label: string;
  match: (meta: CatalogMeta) => boolean;
}

const catalogGroups: CatalogGroupDef[] = [
  { key: 'all', label: messages.home.catalogFilterAll, match: () => true },
  { key: 'sort', label: messages.home.catalogFilterSort, match: (m) => m.key.startsWith('sort.') },
  { key: 'search', label: messages.home.catalogFilterSearch, match: (m) => m.key.startsWith('search.') },
  { key: 'graph', label: messages.home.catalogFilterGraph, match: (m) => m.key.startsWith('graph.') },
  { key: 'tree', label: messages.home.catalogFilterTree, match: (m) => m.key.startsWith('tree.') },
  { key: 'heap', label: messages.home.catalogFilterHeap, match: (m) => m.key.startsWith('heap.') },
  { key: 'hash', label: messages.home.catalogFilterHash, match: (m) => m.key.startsWith('hash.') },
  {
    key: 'linear',
    label: messages.home.catalogFilterLinear,
    match: (m) =>
      m.key.startsWith('stack.') || m.key.startsWith('queue.') || m.key.startsWith('list.'),
  },
  { key: 'structure', label: messages.home.catalogFilterStructure, match: (m) => m.category === 'structure' },
];

const activeGroup = ref<CatalogGroup>('all');
const searchQuery = ref('');
const isCatalogExpanded = ref(false);

const catalogCounts = computed<Record<CatalogGroup, number>>(() => {
  const counts = {} as Record<CatalogGroup, number>;
  for (const group of catalogGroups) counts[group.key] = CATALOG.filter(group.match).length;
  return counts;
});

const filteredCatalog = computed(() => {
  const group = catalogGroups.find((g) => g.key === activeGroup.value);
  let list = group ? CATALOG.filter(group.match) : CATALOG;
  const q = searchQuery.value.trim().toLowerCase();
  if (q) {
    list = list.filter(
      (m) =>
        m.title.toLowerCase().includes(q) ||
        m.key.toLowerCase().includes(q) ||
        m.dataStructure.toLowerCase().includes(q) ||
        m.complexity.average.toLowerCase().includes(q) ||
        m.complexity.space.toLowerCase().includes(q),
    );
  }
  return list;
});

const displayedCatalog = computed(() => {
  if (searchQuery.value.trim()) return filteredCatalog.value;
  if (isCatalogExpanded.value) return filteredCatalog.value;
  return filteredCatalog.value.slice(0, 8);
});

const canExpandCatalog = computed(() => {
  return !searchQuery.value.trim() && filteredCatalog.value.length > 8;
});

function levelLabel(level: CatalogMeta['level']): string {
  return level === 'advanced' ? messages.explore.levelAdvanced : messages.explore.levelBasic;
}

/* ── Practice Ladder 4 Chặng ── */
const ladderStages: Array<{ icon: Component; title: string; desc: string }> = [
  { icon: Eye, title: messages.home.ladderStage1Title, desc: messages.home.ladderStage1Desc },
  { icon: FlaskConical, title: messages.home.ladderStage2Title, desc: messages.home.ladderStage2Desc },
  { icon: Code2, title: messages.home.ladderStage3Title, desc: messages.home.ladderStage3Desc },
  { icon: Trophy, title: messages.home.ladderStage4Title, desc: messages.home.ladderStage4Desc },
];

/* ── Spatial Left Wing: Radar navigation ── */
const activeSection = ref('hero');
const radarCheckpoints = [
  { id: 'sec-hero', label: 'Khởi đầu', icon: Zap },
  { id: 'sec-stats', label: 'Chỉ số', icon: Activity },
  { id: 'sec-demos', label: 'Mô phỏng mẫu', icon: Play },
  { id: 'sec-catalog', label: '44 Thuật toán', icon: Layers },
  { id: 'sec-ladder', label: 'Lộ trình 4 bước', icon: Map },
  { id: 'sec-ecosystem', label: 'Hệ sinh thái', icon: Cpu },
  { id: 'sec-sandbox', label: 'Thử nghiệm nhanh', icon: Terminal },
];

function scrollToSection(id: string): void {
  activeSection.value = id;
  const el = document.getElementById(id);
  if (el) el.scrollIntoView({ behavior: 'smooth' });
}

/* ── Spatial Left Wing: Live Feed Ticker ── */
const liveActivities = [
  { user: 'Thái Sơn', action: 'vừa hoàn thành Lab Cây AVL', time: '1 phút trước' },
  { user: 'Gia Bảo', action: 'đạt 100 điểm QuickSort', time: '3 phút trước' },
  { user: 'Anh Thư', action: 'giải bài Đồ thị Dijkstra', time: '6 phút trước' },
  { user: 'Hoàng Phúc', action: 'mở khóa Rank Master', time: '10 phút trước' },
];

/* ── Spatial Right Wing: Big-O Cheat Data ── */
const bigOCheat = [
  { comp: 'O(1)', label: 'Tuyệt vời', color: '#10B981' },
  { comp: 'O(log n)', label: 'Rất tốt', color: '#34D399' },
  { comp: 'O(n)', label: 'Khá tốt', color: '#38BDF8' },
  { comp: 'O(n log n)', label: 'Trung bình', color: '#FBBF24' },
  { comp: 'O(n²)', label: 'Chậm', color: '#F87171' },
];

/* ── Section 6: Instant Interactive Live Sandbox ── */
const sandboxInput = ref('8, 3, 5, 1, 9, 2');
const sandboxArray = ref([8, 3, 5, 1, 9, 2]);
const sandboxActiveIndices = ref<[number, number] | null>(null);
const sandboxSortedIndices = ref<number[]>([]);
const sandboxRunning = ref(false);

function resetSandbox(): void {
  const parsed = sandboxInput.value
    .split(',')
    .map((s) => parseInt(s.trim(), 10))
    .filter((n) => !isNaN(n));
  sandboxArray.value = parsed.length ? parsed.slice(0, 8) : [8, 3, 5, 1, 9, 2];
  sandboxActiveIndices.value = null;
  sandboxSortedIndices.value = [];
  sandboxRunning.value = false;
}

async function runSandboxSort(): Promise<void> {
  if (sandboxRunning.value) return;
  sandboxRunning.value = true;
  sandboxSortedIndices.value = [];
  const arr = [...sandboxArray.value];
  const n = arr.length;

  for (let i = 0; i < n; i++) {
    for (let j = 0; j < n - i - 1; j++) {
      sandboxActiveIndices.value = [j, j + 1];
      await new Promise((r) => setTimeout(r, 260));
      if (arr[j] > arr[j + 1]) {
        const temp = arr[j];
        arr[j] = arr[j + 1];
        arr[j + 1] = temp;
        sandboxArray.value = [...arr];
        await new Promise((r) => setTimeout(r, 220));
      }
    }
    sandboxSortedIndices.value.push(n - i - 1);
  }
  sandboxActiveIndices.value = null;
  sandboxRunning.value = false;
}

/* ── GSAP Entrance Timeline ── */
onMounted(() => {
  loadDemo(activeKey.value);
  play();

  if (!prefersReducedMotion() && typeof document !== 'undefined') {
    const heroEl = document.querySelector('.home__spatial-hero');
    if (heroEl) {
      gsap.from(heroEl, {
        opacity: 0,
        y: 24,
        duration: 0.6,
        ease: 'power2.out',
      });
    }
  }
});

onUnmounted(stopPlayback);
</script>

<template>
  <div class="home">
    <!-- KHUNG KHÔNG GIAN 3 VÙNG SPATIAL LAYOUT (20% - 60% - 20%) -->
    <div class="home__spatial-wrapper">
      
      <!-- CÁNH TRÁI (20%): NAVIGATION RADAR & LIVE ACTIVITY FEED -->
      <aside class="home__spatial-left" aria-label="Định vị và hoạt động học tập">
        <div class="home__hud-card">
          <div class="home__hud-header">
            <Sparkles class="size-3.5 text-primary" aria-hidden="true" />
            <span class="home__hud-title">SPATIAL RADAR</span>
          </div>
          <nav class="home__radar-nav" aria-label="Mục lục trang chủ">
            <button
              v-for="item in radarCheckpoints"
              :key="item.id"
              type="button"
              class="home__radar-item"
              :class="{ 'home__radar-item--active': activeSection === item.id }"
              @click="scrollToSection(item.id)"
            >
              <component :is="item.icon" class="size-3.5" aria-hidden="true" />
              <span>{{ item.label }}</span>
            </button>
          </nav>
        </div>

        <div class="home__hud-card">
          <div class="home__hud-header">
            <Activity class="size-3.5 text-emerald-400" aria-hidden="true" />
            <span class="home__hud-title">LIVE FEED</span>
          </div>
          <ul class="home__feed-list">
            <li v-for="(act, idx) in liveActivities" :key="idx" class="home__feed-item">
              <span class="home__feed-user">{{ act.user }}</span>
              <span class="home__feed-act">{{ act.action }}</span>
              <span class="home__feed-time">{{ act.time }}</span>
            </li>
          </ul>
        </div>
      </aside>

      <!-- TRỤC TRUNG TÂM (60%): 7 PHÂN ĐOẠN NỘI DUNG CHÍNH -->
      <main class="home__spatial-center">
        
        <!-- PHÂN ĐOẠN 1: HERO SECTION -->
        <section id="sec-hero" class="home__hero home__spatial-hero">
          <div class="container home__hero-grid">
            <div class="home__hero-copy">
              <p class="home__kicker">
                <span class="home__kicker-dot" aria-hidden="true" />
                <span class="font-mono">{{ messages.home.heroKicker }}</span>
              </p>
              <h1 class="home__title">{{ messages.home.heroTitle }}</h1>
              <p class="home__subtitle">{{ messages.home.heroSubtitle }}</p>
              <div class="home__cta">
                <RouterLink
                  :to="{ name: 'simulations' }"
                  :class="buttonVariants({ variant: 'default', size: 'lg' })"
                >
                  {{ messages.home.ctaExplore }}
                  <ArrowRight class="size-4" aria-hidden="true" />
                </RouterLink>
                <RouterLink
                  :to="{ name: 'register' }"
                  :class="buttonVariants({ variant: 'outline', size: 'lg' })"
                >
                  {{ messages.home.ctaStart }}
                </RouterLink>
              </div>
            </div>

            <!-- Morphing Workbench Stage (Hero Bench) -->
            <div class="home__bench" aria-label="Mô phỏng trực quan đang chạy — mở mô phỏng để tương tác từng bước">
              <div class="home__bench-macbar" aria-hidden="true">
                <div class="home__bench-dots">
                  <span class="home__bench-dot home__bench-dot--close" />
                  <span class="home__bench-dot home__bench-dot--min" />
                  <span class="home__bench-dot home__bench-dot--max" />
                </div>
                <span class="home__bench-caption">dsa-visual-runner.wasm</span>
              </div>

              <div class="home__bench-head">
                <span class="home__bench-live">
                  <span class="home__bench-live-dot" aria-hidden="true" />
                  {{ messages.home.simLive }}
                </span>
                <span class="home__bench-key">{{ activeMeta?.key }}</span>
                <span class="home__bench-step">{{ stepLabel }}</span>
              </div>

              <div
                :key="activeKey"
                class="home__bench-stage"
                role="img"
                :aria-label="`${activeMeta?.title ?? ''} — ${stepLabel}`"
              >
                <div
                  v-for="(el, idx) in frameElements"
                  :key="`${el.id}::${stepIndex}`"
                  class="home__block"
                  :class="blockStatusClass(el.status)"
                >
                  <span class="home__block-value">{{ el.label }}</span>
                  <span class="home__block-index">{{ String(idx).padStart(2, '0') }}</span>
                </div>
              </div>

              <p class="home__bench-trace" :aria-live="isPlaying ? 'off' : 'polite'">
                <span class="home__bench-trace-gutter" aria-hidden="true">{{ traceLine }}</span>
                <span class="home__bench-trace-text">{{ currentStep?.explanation || messages.home.simStepHint }}</span>
              </p>

              <div class="home__bench-controls" role="group" :aria-label="messages.home.simControls">
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  class="home__bench-ctrl"
                  :aria-label="isPlaying ? messages.home.simPause : messages.home.simPlay"
                  @click="togglePlay"
                >
                  <component :is="isPlaying ? Pause : Play" class="size-4" aria-hidden="true" />
                </Button>
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  class="home__bench-ctrl"
                  :aria-label="messages.home.simStepBack"
                  :disabled="stepIndex === 0"
                  @click="stepBack"
                >
                  <StepBack class="size-4" aria-hidden="true" />
                </Button>
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  class="home__bench-ctrl"
                  :aria-label="messages.home.simStepForward"
                  :disabled="stepIndex >= steps.length - 1"
                  @click="stepForward"
                >
                  <StepForward class="size-4" aria-hidden="true" />
                </Button>
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  class="home__bench-ctrl"
                  :aria-label="messages.home.simReset"
                  :disabled="stepIndex === 0"
                  @click="resetSim"
                >
                  <RotateCcw class="size-4" aria-hidden="true" />
                </Button>

                <div class="home__bench-speed">
                  <label class="home__bench-speed-label" for="home-bench-speed">{{ messages.home.simSpeed }}</label>
                  <input
                    id="home-bench-speed"
                    class="home__bench-speed-input"
                    type="range"
                    min="0.5"
                    max="2"
                    step="0.25"
                    :value="speed"
                    :aria-label="messages.home.simSpeed"
                    @input="onSpeedInput"
                  />
                  <span class="home__bench-speed-value">{{ speedValue }}</span>
                </div>
              </div>

              <div class="home__bench-tabs" role="group" aria-label="Chọn mô phỏng demo">
                <Button
                  v-for="opt in demoOptions"
                  :key="opt.key"
                  type="button"
                  variant="ghost"
                  size="sm"
                  class="home__bench-tab"
                  :class="{ 'home__bench-tab--active': opt.key === activeKey }"
                  :aria-pressed="opt.key === activeKey"
                  @click="selectDemo(opt.key)"
                >
                  {{ opt.label }}
                </Button>
              </div>
            </div>
          </div>
        </section>

        <!-- PHÂN ĐOẠN 2: STATS METRICS STRIP -->
        <section id="sec-stats" class="home__stats container" aria-label="Thống kê">
          <div class="home__stats-head">
            <h2 class="home__section-title">{{ messages.home.statsTitle }}</h2>
            <p class="home__stats-note">{{ messages.home.statsNote }}</p>
          </div>

          <div class="home__stats-grid">
            <BlockToken
              class="home__stat-hero"
              :label="messages.home.statsVisuals"
              :value="`${stats.visuals}+`"
              :aria-label="`${stats.visuals}+ ${messages.home.statsVisuals}`"
            />
            <div class="home__stat">
              <Layers class="home__stat-icon" :size="16" aria-hidden="true" />
              <span class="home__stat-value">{{ stats.groups }}</span>
              <span class="home__stat-label">{{ messages.home.statsGroups }}</span>
            </div>
            <div class="home__stat">
              <Gauge class="home__stat-icon" :size="16" aria-hidden="true" />
              <span class="home__stat-value">{{ stats.levels }}</span>
              <span class="home__stat-label">{{ messages.home.statsLevels }}</span>
            </div>
          </div>
        </section>

        <!-- PHÂN ĐOẠN 3: 3 PUBLIC DEMOS -->
        <section id="sec-demos" class="home__section container">
          <div class="home__section-head">
            <span class="home__kicker home__kicker--center">
              <span class="font-mono">{{ messages.home.demoBadge }}</span>
            </span>
            <h2 class="home__section-title">{{ messages.home.demoTabTitle }}</h2>
            <p class="home__section-desc">{{ messages.home.demoTabDesc }}</p>
          </div>

          <div class="home__grid">
            <Card
              v-for="demo in demos"
              :key="demo.key"
              class="home__demo"
            >
              <CardHeader>
                <div class="home__demo-thumb" aria-hidden="true">
                  <div v-if="demo.key === 'sort.bubble'" class="home__thumb-bars">
                    <span class="home__thumb-bar" />
                    <span class="home__thumb-bar" />
                    <span class="home__thumb-bar" />
                    <span class="home__thumb-bar home__thumb-bar--done" />
                    <span class="home__thumb-bar home__thumb-bar--done" />
                  </div>
                  <div v-else-if="demo.key === 'search.binary'" class="home__thumb-row">
                    <span class="home__thumb-block" />
                    <span class="home__thumb-block" />
                    <span class="home__thumb-block home__thumb-block--found" />
                    <span class="home__thumb-block" />
                    <span class="home__thumb-block" />
                  </div>
                  <div v-else class="home__thumb-graph">
                    <span class="home__thumb-node" />
                    <span class="home__thumb-edge" />
                    <span class="home__thumb-node home__thumb-node--visited" />
                    <span class="home__thumb-edge" />
                    <span class="home__thumb-node" />
                    <span class="home__thumb-edge" />
                    <span class="home__thumb-node" />
                  </div>
                </div>
                <CardTitle class="home__demo-title">
                  <component :is="demo.icon" :size="16" class="home__demo-title-icon" aria-hidden="true" />
                  {{ demo.title }}
                </CardTitle>
                <div class="home__demo-meta">
                  <Badge variant="secondary" class="home__demo-badge">{{ demo.dataStructure }}</Badge>
                  <span class="home__demo-level">{{ levelLabel(demo.level) }}</span>
                </div>
              </CardHeader>
              <CardContent class="home__demo-content">
                <div class="home__demo-chips">
                  <span class="home__chip">{{ messages.home.catalogTime }} {{ demo.complexity.average }}</span>
                  <span class="home__chip">{{ messages.home.catalogSpace }} {{ demo.complexity.space }}</span>
                </div>
                <Button
                  type="button"
                  class="w-full"
                  :aria-label="`${messages.home.demoOpen} ${demo.title}`"
                  @click="openDemo(demo.key)"
                >
                  <Play class="size-4" aria-hidden="true" />
                  {{ messages.home.demoRun }}
                  <ArrowRight class="size-4" aria-hidden="true" />
                </Button>
              </CardContent>
            </Card>
          </div>
        </section>

        <!-- PHÂN ĐOẠN 4: ALGORITHM CONSTELLATION CATALOG -->
        <section id="sec-catalog" class="home__section container">
          <div class="home__section-head">
            <span class="home__kicker home__kicker--center">
              <span class="font-mono">{{ messages.home.catalogBadge }}</span>
            </span>
            <h2 class="home__section-title">{{ messages.home.catalogTitle }}</h2>
            <p class="home__section-desc">{{ messages.home.catalogDesc }}</p>
          </div>

          <div class="home__catalog-toolbar">
            <div class="home__catalog-search">
              <Search class="home__catalog-search-icon" :size="16" aria-hidden="true" />
              <input
                v-model="searchQuery"
                type="search"
                class="home__catalog-search-input"
                :placeholder="messages.home.catalogSearchPlaceholder"
              />
              <button
                v-if="searchQuery"
                type="button"
                class="home__catalog-search-clear"
                @click="searchQuery = ''"
              >
                <X :size="14" />
              </button>
            </div>

            <div class="home__filters" role="tablist" aria-label="Bộ lọc danh mục thuật toán">
              <Button
                v-for="group in catalogGroups"
                :key="group.key"
                type="button"
                variant="ghost"
                size="sm"
                class="home__filter"
                :class="{ 'home__filter--active': group.key === activeGroup }"
                @click="activeGroup = group.key"
              >
                <span>{{ group.label }}</span>
                <span class="home__filter-count" aria-hidden="true">{{ catalogCounts[group.key] }}</span>
              </Button>
            </div>
          </div>

          <div v-if="displayedCatalog.length === 0" class="home__catalog-empty" role="status">
            <p class="home__catalog-empty-text">{{ messages.home.catalogNoResults }}</p>
            <Button variant="secondary" size="sm" @click="searchQuery = ''; activeGroup = 'all'">
              {{ messages.common.retry }}
            </Button>
          </div>

          <div v-else class="home__catalog">
            <Card
              v-for="item in displayedCatalog"
              :key="item.key"
              class="home__catalog-card"
            >
              <CardHeader class="home__catalog-head">
                <CardTitle class="home__catalog-title">{{ item.title }}</CardTitle>
                <div class="home__catalog-meta">
                  <Badge variant="secondary" class="home__catalog-badge">{{ item.dataStructure }}</Badge>
                  <span class="home__catalog-level">{{ levelLabel(item.level) }}</span>
                </div>
              </CardHeader>
              <CardContent class="home__catalog-content">
                <div class="home__catalog-chips">
                  <span class="home__chip">{{ messages.home.catalogTime }} {{ item.complexity.average }}</span>
                  <span class="home__chip">{{ messages.home.catalogSpace }} {{ item.complexity.space }}</span>
                </div>
                <Button
                  type="button"
                  class="home__catalog-cta"
                  :aria-label="messages.home.catalogOpen(item.title)"
                  @click="openDemo(item.key)"
                >
                  {{ messages.home.catalogPractice }}
                  <ArrowRight class="size-4" aria-hidden="true" />
                </Button>
              </CardContent>
            </Card>
          </div>

          <div v-if="canExpandCatalog || isCatalogExpanded" class="home__catalog-expand">
            <Button
              type="button"
              variant="secondary"
              size="lg"
              class="home__catalog-expand-btn"
              @click="isCatalogExpanded = !isCatalogExpanded"
            >
              <template v-if="!isCatalogExpanded">
                {{ messages.home.catalogViewAll(filteredCatalog.length) }}
                <ArrowRight class="size-4" aria-hidden="true" />
              </template>
              <template v-else>
                {{ messages.home.catalogCollapse }}
              </template>
            </Button>
          </div>
        </section>

        <!-- PHÂN ĐOẠN 5: PRACTICE LADDER 4 BƯỚC -->
        <section id="sec-ladder" class="home__section container">
          <div class="home__section-head">
            <span class="home__kicker home__kicker--center">
              <span class="font-mono">{{ messages.home.ladderBadge }}</span>
            </span>
            <h2 class="home__section-title">{{ messages.home.ladderTitle }}</h2>
            <p class="home__section-desc">{{ messages.home.ladderDesc }}</p>
          </div>

          <div class="home__ladder">
            <div
              v-for="(stage, index) in ladderStages"
              :key="stage.title"
              class="home__ladder-step-wrapper"
            >
              <Card class="home__ladder-card">
                <CardContent class="home__ladder-content">
                  <div class="home__ladder-top">
                    <span class="home__ladder-step">{{ messages.home.ladderStepLabel(index + 1) }}</span>
                    <span class="home__ladder-icon" aria-hidden="true">
                      <component :is="stage.icon" :size="20" />
                    </span>
                  </div>
                  <h3 class="home__ladder-title">{{ stage.title }}</h3>
                  <p class="home__ladder-desc">{{ stage.desc }}</p>
                </CardContent>
              </Card>
              <div v-if="index < ladderStages.length - 1" class="home__ladder-flow-line" aria-hidden="true">
                <div class="home__ladder-flow-track" />
                <span class="home__ladder-flow-dot" />
              </div>
            </div>
          </div>
        </section>

        <!-- PHÂN ĐOẠN 6: DEEP-TECH ECOSYSTEM -->
        <section id="sec-ecosystem" class="home__section container">
          <div class="home__section-head">
            <span class="home__kicker home__kicker--center">
              <span class="font-mono">CORE CAPABILITIES</span>
            </span>
            <h2 class="home__section-title">Hệ Sinh Thái Tính Năng Đỉnh Cao</h2>
            <p class="home__section-desc">Bộ công cụ hoàn chỉnh dành cho cả người học tự do và lớp học chuyên sâu.</p>
          </div>

          <div class="home__features-grid">
            <Card class="home__feature home__feature--visual home__feature--featured">
              <CardHeader>
                <div class="home__feature-icon-box">
                  <Eye class="size-5 text-primary" />
                </div>
                <CardTitle>Engine Mô Phỏng 60 FPS</CardTitle>
                <CardDescription>Trực quan hóa cấu trúc dữ liệu với chuyển động mượt mà và phân tích từng bước L{line}.</CardDescription>
              </CardHeader>
            </Card>

            <Card class="home__feature home__feature--path home__feature--featured">
              <CardHeader>
                <div class="home__feature-icon-box">
                  <Code2 class="size-5 text-emerald-400" />
                </div>
                <CardTitle>Code Runner Web Worker</CardTitle>
                <CardDescription>Biên dịch và chạy thử C++, Java, Python trong môi trường cô lập ngay trên trình duyệt.</CardDescription>
              </CardHeader>
            </Card>

            <Card class="home__feature home__feature--compact">
              <CardHeader>
                <div class="home__feature-icon-box">
                  <GraduationCap class="size-5 text-amber-400" />
                </div>
                <CardTitle>Quản Lý Lớp Học & Giảng Viên</CardTitle>
                <CardDescription>Giao bài tập, thiết lập hạn nộp và xuất báo cáo tiến độ học viên chuyên nghiệp.</CardDescription>
              </CardHeader>
            </Card>
          </div>
        </section>

        <!-- PHÂN ĐOẠN 7: INSTANT LIVE SANDBOX & CTA -->
        <section id="sec-sandbox" class="home__section container">
          <Card class="home__sandbox-card">
            <div class="home__sandbox-header">
              <div class="home__sandbox-title-wrap">
                <Terminal class="size-5 text-primary" />
                <h3 class="home__sandbox-title">Thử Nghiệm Thuật Toán Trực Tiếp</h3>
              </div>
              <span class="home__sandbox-badge">Interactive Sandbox</span>
            </div>
            
            <p class="home__sandbox-desc">
              Nhập một dãy số bất kỳ (cách nhau bằng dấu phẩy) và quan sát thuật toán Bubble Sort tự động chạy ngay tại chỗ!
            </p>

            <div class="home__sandbox-input-row">
              <input
                v-model="sandboxInput"
                type="text"
                class="home__sandbox-input"
                placeholder="Ví dụ: 8, 3, 5, 1, 9, 2"
                :disabled="sandboxRunning"
                @keyup.enter="resetSandbox(); runSandboxSort()"
              />
              <Button
                type="button"
                variant="primary"
                :disabled="sandboxRunning"
                @click="resetSandbox(); runSandboxSort()"
              >
                <Play class="size-4" />
                Chạy sắp xếp
              </Button>
              <Button
                type="button"
                variant="secondary"
                :disabled="sandboxRunning"
                @click="resetSandbox"
              >
                <RotateCcw class="size-4" />
                Đặt lại
              </Button>
            </div>

            <!-- Sandbox Visualization Stage -->
            <div class="home__sandbox-stage">
              <div
                v-for="(val, idx) in sandboxArray"
                :key="idx"
                class="home__sandbox-bar"
                :class="{
                  'home__sandbox-bar--active': sandboxActiveIndices?.includes(idx),
                  'home__sandbox-bar--sorted': sandboxSortedIndices.includes(idx),
                }"
              >
                <span class="home__sandbox-bar-val">{{ val }}</span>
                <span class="home__sandbox-bar-idx">[{{ idx }}]</span>
              </div>
            </div>
          </Card>
        </section>

      </main>

      <!-- CÁNH PHẢI (20%): DAILY CHALLENGE, LEADERBOARD, BIG-O GUIDE -->
      <aside class="home__spatial-right" aria-label="Bảng xếp hạng và độ phức tạp">
        <div class="home__hud-card">
          <div class="home__hud-header">
            <Flame class="size-3.5 text-amber-500" aria-hidden="true" />
            <span class="home__hud-title">DAILY DSA QUEST</span>
          </div>
          <p class="home__quest-desc">Đảo ngược danh sách liên kết đơn</p>
          <div class="home__quest-reward">
            <span class="home__quest-badge">+50 EXP</span>
            <RouterLink :to="{ name: 'simulations' }" class="home__quest-btn">
              Thử ngay <ArrowRight class="size-3" />
            </RouterLink>
          </div>
        </div>

        <div class="home__hud-card">
          <div class="home__hud-header">
            <Trophy class="size-3.5 text-amber-400" aria-hidden="true" />
            <span class="home__hud-title">TOP MASTERS</span>
          </div>
          <ul class="home__rank-list">
            <li class="home__rank-item">
              <span class="home__rank-badge home__rank-badge--gold">1</span>
              <span class="home__rank-name">Thái Sơn</span>
              <span class="home__rank-exp">2,450 EXP</span>
            </li>
            <li class="home__rank-item">
              <span class="home__rank-badge home__rank-badge--silver">2</span>
              <span class="home__rank-name">Gia Bảo</span>
              <span class="home__rank-exp">2,180 EXP</span>
            </li>
            <li class="home__rank-item">
              <span class="home__rank-badge home__rank-badge--bronze">3</span>
              <span class="home__rank-name">Anh Thư</span>
              <span class="home__rank-exp">1,920 EXP</span>
            </li>
          </ul>
        </div>

        <div class="home__hud-card">
          <div class="home__hud-header">
            <Zap class="size-3.5 text-sky-400" aria-hidden="true" />
            <span class="home__hud-title">BIG-O GUIDE</span>
          </div>
          <div class="home__bigo-list">
            <div v-for="bo in bigOCheat" :key="bo.comp" class="home__bigo-item">
              <span class="home__bigo-comp font-mono" :style="{ color: bo.color }">{{ bo.comp }}</span>
              <span class="home__bigo-label">{{ bo.label }}</span>
            </div>
          </div>
        </div>

        <div class="home__hud-card home__hud-card--status">
          <span class="home__engine-dot" aria-hidden="true" />
          <span class="home__engine-text">WASM ENGINE: 60 FPS</span>
        </div>
      </aside>

    </div>
  </div>
</template>

<style scoped>
.home {
  display: flex;
  flex-direction: column;
  gap: var(--space-2xl);
  padding-bottom: var(--space-3xl);
  width: 100%;
}

/* ── Spatial Layout Wrapper ── */
.home__spatial-wrapper {
  display: grid;
  grid-template-columns: 1fr;
  width: 100%;
  max-width: 1540px;
  margin-inline: auto;
  padding-inline: var(--space-md);
  gap: var(--space-lg);
}

@media (min-width: 1280px) {
  .home__spatial-wrapper {
    grid-template-columns: 240px minmax(0, 1fr) 260px;
    gap: var(--space-xl);
  }
}

/* ── Spatial Left & Right HUD Wings ── */
.home__spatial-left,
.home__spatial-right {
  display: none;
}

@media (min-width: 1280px) {
  .home__spatial-left,
  .home__spatial-right {
    display: flex;
    flex-direction: column;
    gap: var(--space-md);
    position: sticky;
    top: calc(var(--space-2xl) + 60px);
    height: fit-content;
  }
}

.home__spatial-center {
  display: flex;
  flex-direction: column;
  gap: var(--space-2xl);
  min-width: 0;
}

/* ── HUD Cards ── */
.home__hud-card {
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  box-shadow: 0 4px 12px -2px rgba(0, 0, 0, 0.04);
}

.home__hud-header {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  padding-bottom: var(--space-xs);
  border-bottom: 1px solid var(--color-border-subtle);
}

.home__hud-title {
  font-family: var(--font-mono);
  font-size: 10px;
  font-weight: 600;
  letter-spacing: 0.08em;
  color: var(--color-text-secondary);
}

.home__radar-nav {
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.home__radar-item {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: 6px var(--space-sm);
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
  border-radius: var(--radius-md);
  border: none;
  background: transparent;
  text-align: left;
  cursor: pointer;
  transition: background 150ms ease, color 150ms ease;
}

.home__radar-item:hover {
  background: var(--color-muted);
  color: var(--color-foreground);
}

.home__radar-item--active {
  background: color-mix(in srgb, var(--color-primary) 12%, transparent);
  color: var(--color-primary);
  font-weight: 600;
}

/* ── Live Feed & Leaderboard ── */
.home__feed-list,
.home__rank-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.home__feed-item {
  display: flex;
  flex-direction: column;
  font-size: 11px;
  line-height: 1.4;
  padding-bottom: 6px;
  border-bottom: 1px dashed var(--color-border-subtle);
}

.home__feed-item:last-child {
  border-bottom: none;
  padding-bottom: 0;
}

.home__feed-user {
  font-weight: 600;
  color: var(--color-foreground);
}

.home__feed-act {
  color: var(--color-text-secondary);
}

.home__feed-time {
  font-size: 9px;
  color: var(--color-text-tertiary);
  margin-top: 2px;
}

.home__rank-item {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-xs);
}

.home__rank-badge {
  width: 18px;
  height: 18px;
  border-radius: var(--radius-full);
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 10px;
  font-weight: 700;
  color: #fff;
}

.home__rank-badge--gold { background: #EAB308; }
.home__rank-badge--silver { background: #94A3B8; }
.home__rank-badge--bronze { background: #D97706; }

.home__rank-name {
  font-weight: 500;
  color: var(--color-foreground);
  flex: 1;
}

.home__rank-exp {
  font-family: var(--font-mono);
  font-size: 10px;
  color: var(--color-text-secondary);
}

/* ── Big-O Guide ── */
.home__bigo-list {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.home__bigo-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-size: 11px;
}

.home__bigo-comp {
  font-weight: 600;
}

.home__bigo-label {
  color: var(--color-text-tertiary);
}

.home__hud-card--status {
  flex-direction: row;
  align-items: center;
  gap: var(--space-xs);
  background: color-mix(in srgb, var(--color-primary) 8%, var(--color-card));
  border-color: color-mix(in srgb, var(--color-primary) 25%, transparent);
}

.home__engine-dot {
  width: 7px;
  height: 7px;
  border-radius: var(--radius-full);
  background: #10B981;
  box-shadow: 0 0 8px #10B981;
}

.home__engine-text {
  font-family: var(--font-mono);
  font-size: 10px;
  font-weight: 600;
  letter-spacing: 0.04em;
  color: var(--color-foreground);
}

/* ── Hero Section ── */
.home__hero {
  padding-block: var(--space-lg);
}

.home__hero-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--space-xl);
  align-items: center;
}

@media (min-width: 960px) {
  .home__hero-grid {
    grid-template-columns: 1fr 1.15fr;
  }
}

.home__hero-copy {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: var(--space-md);
}

.home__kicker {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.home__kicker--center { justify-content: center; }

.home__kicker-dot {
  width: 8px;
  height: 8px;
  border-radius: var(--radius-full);
  background: var(--color-primary);
}

.home__title {
  margin: 0;
  max-width: 18ch;
  font-size: var(--text-4xl);
  font-weight: 600;
  line-height: 1.15;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
}

.home__subtitle {
  margin: 0;
  max-width: 52ch;
  font-size: var(--text-md);
  line-height: 1.6;
  color: var(--color-text-secondary);
}

.home__cta {
  display: flex;
  gap: var(--space-md);
  flex-wrap: wrap;
  margin-top: var(--space-xs);
}

/* ── Bench Demo ── */
.home__bench {
  background: var(--color-canvas-ink);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: var(--radius-lg);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  box-shadow: 0 12px 32px -8px rgba(15, 23, 42, 0.35), 0 0 0 1px rgba(255, 255, 255, 0.04);
}

.home__bench-macbar {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding-bottom: var(--space-xs);
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
}

.home__bench-dots {
  display: flex;
  align-items: center;
  gap: 6px;
}

.home__bench-dot {
  width: 9px;
  height: 9px;
  border-radius: var(--radius-full);
}

.home__bench-dot--close { background: rgba(248, 113, 113, 0.8); }
.home__bench-dot--min { background: rgba(251, 191, 36, 0.8); }
.home__bench-dot--max { background: rgba(52, 211, 153, 0.8); }

.home__bench-caption {
  font-family: var(--font-mono);
  font-size: 11px;
  color: var(--color-index-muted);
  letter-spacing: 0.04em;
}

.home__bench-head {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  font-size: var(--text-xs);
}

.home__bench-live {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  color: var(--color-resolved);
  text-transform: uppercase;
  letter-spacing: 0.08em;
}

.home__bench-live-dot {
  width: 8px;
  height: 8px;
  border-radius: var(--radius-full);
  background: var(--color-resolved);
}

.home__bench-key,
.home__bench-step { color: var(--color-index-muted); }

.home__bench-key {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.home__bench-step {
  margin-left: auto;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.04em;
}

.home__bench-stage {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
  padding: var(--space-md);
  background: var(--color-canvas-ink);
  border: 1px solid rgba(255, 255, 255, 0.04);
  border-radius: var(--radius-md);
  min-height: 88px;
  align-items: center;
  justify-content: center;
}

.home__block {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-width: 44px;
  min-height: 48px;
  padding: var(--space-xs) var(--space-sm);
  border-radius: var(--radius-md);
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 600;
  transition: transform 180ms ease, background-color 180ms ease;
}

.home__block--default { background: var(--color-data-core-bg); color: var(--color-data-core); border: 1px solid rgba(255, 255, 255, 0.08); }
.home__block--active { background: rgba(56, 189, 248, 0.2); color: #38BDF8; border: 1px solid rgba(56, 189, 248, 0.5); transform: translateY(-3px); }
.home__block--swap { background: rgba(251, 191, 36, 0.2); color: #FBBF24; border: 1px solid rgba(251, 191, 36, 0.5); transform: translateY(-4px); }
.home__block--done { background: rgba(52, 211, 153, 0.2); color: #34D399; border: 1px solid rgba(52, 211, 153, 0.5); }
.home__block--muted { opacity: 0.4; background: transparent; border: 1px dashed rgba(255, 255, 255, 0.15); }

.home__block-value { font-size: var(--text-base); }
.home__block-index { font-size: 9px; opacity: 0.6; }

.home__bench-trace {
  margin: 0;
  display: flex;
  align-items: baseline;
  gap: var(--space-sm);
  font-size: var(--text-xs);
  line-height: 1.5;
  color: var(--color-index-muted);
}

.home__bench-trace-gutter {
  font-family: var(--font-mono);
  color: var(--color-primary);
  font-weight: 600;
}

.home__bench-controls {
  display: flex;
  align-items: center;
  gap: 4px;
  padding-top: var(--space-xs);
  border-top: 1px solid rgba(255, 255, 255, 0.06);
}

.home__bench-ctrl {
  color: var(--color-index-muted);
  width: 32px;
  height: 32px;
}

.home__bench-ctrl:hover {
  color: var(--color-foreground-dark);
  background: rgba(255, 255, 255, 0.06);
}

.home__bench-speed {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  margin-left: auto;
  font-size: var(--text-xs);
  color: var(--color-index-muted);
}

.home__bench-speed-input {
  width: 72px;
  accent-color: var(--color-primary);
  cursor: pointer;
}

.home__bench-tabs {
  display: flex;
  gap: 4px;
  flex-wrap: wrap;
}

.home__bench-tab {
  font-size: 11px;
  color: var(--color-index-muted);
  height: 26px;
  padding-inline: var(--space-sm);
}

.home__bench-tab:hover {
  color: var(--color-foreground-dark);
  background: rgba(255, 255, 255, 0.06);
}

.home__bench-tab--active {
  background: rgba(255, 255, 255, 0.12);
  color: var(--color-foreground-dark);
  font-weight: 600;
}

/* ── Stats Strip ── */
.home__stats {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.home__stats-head {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  flex-wrap: wrap;
  gap: var(--space-sm);
}

.home__section-title {
  margin: 0;
  font-size: var(--text-2xl);
  font-weight: 600;
  color: var(--color-foreground);
}

.home__section-desc,
.home__stats-note {
  margin: 0;
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
}

.home__stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: var(--space-md);
}

.home__stat {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-md);
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
}

.home__stat-icon {
  color: var(--color-primary);
}

.home__stat-value {
  font-family: var(--font-mono);
  font-size: var(--text-2xl);
  font-weight: 700;
  color: var(--color-foreground);
}

.home__stat-label {
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
}

/* ── Public Demos & Catalog ── */
.home__section {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.home__section-head {
  display: flex;
  flex-direction: column;
  align-items: center;
  text-align: center;
  gap: var(--space-xs);
}

.home__grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: var(--space-md);
}

.home__demo {
  display: flex;
  flex-direction: column;
  transition: border-color 150ms ease;
}

.home__demo:hover { border-color: var(--color-border-strong); }

.home__demo-thumb {
  height: 96px;
  background: var(--color-canvas-ink);
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: var(--space-sm);
}

.home__thumb-bars {
  display: flex;
  align-items: flex-end;
  gap: 6px;
  height: 48px;
}

.home__thumb-bar {
  width: 10px;
  height: 24px;
  background: rgba(255, 255, 255, 0.2);
  border-radius: 2px;
}

.home__thumb-bar:nth-child(2) { height: 36px; }
.home__thumb-bar:nth-child(3) { height: 18px; }
.home__thumb-bar--done { background: var(--color-resolved); height: 44px; }

.home__thumb-row {
  display: flex;
  gap: 4px;
}

.home__thumb-block {
  width: 24px;
  height: 24px;
  border-radius: 4px;
  background: rgba(255, 255, 255, 0.15);
}

.home__thumb-block--found {
  background: var(--color-primary);
}

.home__thumb-graph {
  display: flex;
  align-items: center;
  gap: 4px;
}

.home__thumb-node {
  width: 14px;
  height: 14px;
  border-radius: var(--radius-full);
  background: rgba(255, 255, 255, 0.2);
}

.home__thumb-node--visited { background: var(--color-resolved); }
.home__thumb-edge { width: 14px; height: 2px; background: rgba(255, 255, 255, 0.15); }

.home__demo-title {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  font-size: var(--text-base);
  font-weight: 600;
}

.home__demo-meta,
.home__catalog-meta {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  margin-top: 4px;
}

.home__demo-chips,
.home__catalog-chips {
  display: flex;
  gap: var(--space-xs);
  margin-bottom: var(--space-md);
}

.home__chip {
  font-family: var(--font-mono);
  font-size: 10px;
  padding: 2px 6px;
  border-radius: var(--radius-sm);
  background: var(--color-muted);
  color: var(--color-text-secondary);
}

/* ── Catalog Toolbar ── */
.home__catalog-toolbar {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-md);
  width: 100%;
}

.home__catalog-search {
  position: relative;
  width: 100%;
  max-width: 440px;
}

.home__catalog-search-icon {
  position: absolute;
  left: var(--space-md);
  top: 50%;
  transform: translateY(-50%);
  color: var(--color-text-tertiary);
  pointer-events: none;
}

.home__catalog-search-input {
  width: 100%;
  height: 42px;
  padding: 0 var(--space-xl) 0 calc(var(--space-md) + 24px);
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-full);
  font-size: var(--text-sm);
  color: var(--color-foreground);
  transition: border-color 150ms ease, box-shadow 150ms ease;
}

.home__catalog-search-input:focus-visible {
  outline: none;
  border-color: var(--color-primary);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-primary) 18%, transparent);
}

.home__catalog-search-clear {
  position: absolute;
  right: var(--space-sm);
  top: 50%;
  transform: translateY(-50%);
  width: 28px;
  height: 28px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border: none;
  background: transparent;
  color: var(--color-text-tertiary);
  border-radius: var(--radius-full);
  cursor: pointer;
}

.home__filters {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: var(--space-sm);
}

@media (max-width: 640px) {
  .home__filters {
    flex-wrap: nowrap;
    justify-content: flex-start;
    overflow-x: auto;
    width: 100%;
    padding-bottom: var(--space-xs);
    scrollbar-width: thin;
    -webkit-overflow-scrolling: touch;
  }
}

.home__filter { color: var(--color-text-tertiary); }
.home__filter:hover { color: var(--color-foreground); }
.home__filter--active,
.home__filter--active:hover {
  background: var(--color-primary);
  color: var(--color-on-primary);
}

.home__catalog-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-2xl);
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-lg);
  text-align: center;
}

.home__catalog {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: var(--space-md);
}

.home__catalog-card {
  display: flex;
  flex-direction: column;
  transition: border-color 150ms ease;
}

.home__catalog-card:hover { border-color: var(--color-border-strong); }

.home__catalog-cta {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-size: var(--text-xs);
  font-weight: 500;
  color: var(--color-primary);
  border: none;
  background: transparent;
  padding: 0;
  cursor: pointer;
  margin-top: auto;
}

.home__catalog-cta:hover { transform: translateX(3px); }

.home__catalog-expand {
  display: flex;
  justify-content: center;
}

/* ── Practice Ladder ── */
.home__ladder {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: var(--space-md);
  position: relative;
}

@media (max-width: 900px) {
  .home__ladder { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}

@media (max-width: 600px) {
  .home__ladder { grid-template-columns: 1fr; }
}

.home__ladder-step-wrapper {
  position: relative;
  display: flex;
  flex-direction: column;
}

.home__ladder-card {
  height: 100%;
  display: flex;
  flex-direction: column;
}

.home__ladder-content {
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  height: 100%;
}

.home__ladder-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--space-xs);
}

.home__ladder-step {
  font-size: 10px;
  font-family: var(--font-mono);
  color: var(--color-text-tertiary);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-sm);
  padding: 1px 6px;
}

.home__ladder-icon {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-text-secondary);
  display: flex;
  align-items: center;
  justify-content: center;
}

.home__ladder-title {
  margin: 0;
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-foreground);
}

.home__ladder-desc {
  margin: 0;
  font-size: var(--text-xs);
  line-height: 1.5;
  color: var(--color-text-secondary);
}

.home__ladder-flow-line {
  display: none;
}

@media (min-width: 900px) {
  .home__ladder-flow-line {
    display: flex;
    align-items: center;
    justify-content: center;
    position: absolute;
    top: 28px;
    right: calc(-1 * var(--space-md));
    width: var(--space-md);
    height: 2px;
    z-index: 2;
  }

  .home__ladder-flow-track {
    width: 100%;
    height: 2px;
    background: var(--color-border);
  }

  .home__ladder-flow-dot {
    position: absolute;
    width: 6px;
    height: 6px;
    border-radius: var(--radius-full);
    background: var(--color-primary);
  }
}

/* ── Features Grid ── */
.home__features-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: var(--space-md);
}

.home__feature-icon-box {
  width: 36px;
  height: 36px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: var(--space-xs);
}

/* ── Live Sandbox ── */
.home__sandbox-card {
  padding: var(--space-lg);
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.home__sandbox-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: var(--space-xs);
}

.home__sandbox-title-wrap {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.home__sandbox-title {
  margin: 0;
  font-size: var(--text-lg);
  font-weight: 600;
  color: var(--color-foreground);
}

.home__sandbox-badge {
  font-family: var(--font-mono);
  font-size: 11px;
  color: var(--color-primary);
  background: color-mix(in srgb, var(--color-primary) 12%, transparent);
  padding: 2px 8px;
  border-radius: var(--radius-full);
}

.home__sandbox-desc {
  margin: 0;
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
}

.home__sandbox-input-row {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
}

.home__sandbox-input {
  flex: 1;
  min-width: 200px;
  height: 38px;
  padding: 0 var(--space-md);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  color: var(--color-foreground);
}

.home__sandbox-stage {
  display: flex;
  gap: var(--space-sm);
  padding: var(--space-lg);
  background: var(--color-canvas-ink);
  border-radius: var(--radius-lg);
  min-height: 80px;
  align-items: center;
  justify-content: center;
  flex-wrap: wrap;
}

.home__sandbox-bar {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 48px;
  border-radius: var(--radius-md);
  background: var(--color-data-core-bg);
  border: 1px solid rgba(255, 255, 255, 0.1);
  color: var(--color-data-core);
  font-family: var(--font-mono);
  font-weight: 600;
  transition: transform 180ms ease, background-color 180ms ease, border-color 180ms ease;
}

.home__sandbox-bar--active {
  background: rgba(251, 191, 36, 0.25);
  border-color: #FBBF24;
  color: #FBBF24;
  transform: translateY(-4px);
}

.home__sandbox-bar--sorted {
  background: rgba(52, 211, 153, 0.2);
  border-color: #34D399;
  color: #34D399;
}

.home__sandbox-bar-val { font-size: var(--text-base); }
.home__sandbox-bar-idx { font-size: 9px; opacity: 0.6; }

/* ── Quest Card in Right Wing ── */
.home__quest-desc {
  margin: 0;
  font-size: var(--text-xs);
  color: var(--color-foreground);
  font-weight: 500;
}

.home__quest-reward {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 4px;
}

.home__quest-badge {
  font-family: var(--font-mono);
  font-size: 10px;
  font-weight: 700;
  color: #F59E0B;
}

.home__quest-btn {
  font-size: 11px;
  color: var(--color-primary);
  display: inline-flex;
  align-items: center;
  gap: 2px;
  font-weight: 500;
  text-decoration: none;
}

.home__quest-btn:hover {
  text-decoration: underline;
}
</style>
