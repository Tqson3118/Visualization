<script setup lang="ts">
// HomeView — Màn 01: trang chủ công khai (SDD Màn 01).
// Task 1 (UI redesign, worktree ui-redesign): nâng cấp mỹ quan + micro-interaction —
//   1) Hero Data Bench: bộ điều khiển tương tác (Play/Pause · Step Back/Forward · Reset ·
//      Speed slider) trên trace THẬT từ engine (catalog.ts + registry), callout bước dạng
//      code-line L{line} từ pseudocodeLine của Step — KHÔNG đổi logic sim.
//   2) Algorithm Catalog Grid: filter tabs có count mono (key prefix) + card dense với
//      badge CTDL + chip Big-O mono + CTA "Thực hành ngay" dịch phải khi hover.
//   3) Practice Ladder Showcase: 4 chặng (Trực quan → Thí nghiệm → Code → Đánh giá)
//      index mono STEP 01..04 + icon lucide.
// Giữ 100% selector/API/Pinia/router; icon = lucide-vue-next; vùng dữ liệu LUÔN tối
// (canvas-ink) bất kể theme; không gradient trang trí, không shadow thẻ (DESIGN.md §6).
import { computed, onMounted, onUnmounted, ref, shallowRef } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import {
  ArrowRight,
  ArrowUpDown,
  Code2,
  Eye,
  FlaskConical,
  Gauge,
  Layers,
  Map,
  Network,
  Pause,
  Play,
  RotateCcw,
  Search,
  StepBack,
  StepForward,
  Target,
  Trophy,
  X,
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

/** Icon cho 3 demo công khai (FR-7.6: sort.bubble / search.binary / graph.bfs) */
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

/** Số liệu tĩnh từ danh mục nội dung (44 mô phỏng — SDD §19.6A) */
const stats = computed(() => ({
  visuals: CATALOG.length,
  groups: new Set(CATALOG.map((c) => c.dataStructure)).size,
  levels: new Set(CATALOG.map((c) => c.level)).size,
}));

function openDemo(key: string): void {
  void router.push({ name: 'simulator', params: { key } });
}

/* ── Hero mini-sim: step THẬT từ engine (catalog.ts + registry) ──
   Task 1: thêm điều khiển tương tác — play/pause/step/back/reset/speed.
   KHÔNG đổi generator/input/step — chỉ điều phối timer + stepIndex (UI layer). */
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
/** Tốc độ phát 0.5×–2× (0.25 bước) — map 380ms / speed */
const speed = ref(1);

const activeMeta = computed(() => CATALOG.find((c) => c.key === activeKey.value));
const currentStep = computed(() => steps.value[stepIndex.value] ?? null);
const frameElements = computed(() => currentStep.value?.structure.elements ?? []);
const stepLabel = computed(() => messages.home.simStepOf(stepIndex.value + 1, steps.value.length));

/** Trace line dạng code: L{line} — dòng mã giả thật của bước (Step.pseudocodeLine). */
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
  // FIX REVIEW (MAJOR): khôi phục guard cũ của startPlayback() — dừng mọi interval/
  // timeout đang chạy trước khi phát. selectDemo()/setSpeed() gọi play() khi timer cũ
  // còn sống → nếu thiếu guard: interval thứ 2 + timer cũ leak (tốc độ nhân đôi, vòng
  // tự duy trì sống sót cả sau unmount).
  stopPlayback();
  // Tôn trọng prefers-reduced-motion: không autoplay, giữ frame đầu tĩnh.
  if (prefersReducedMotion()) return;
  if (steps.value.length === 0) return;
  // Hết dãy khi bấm Play → quay đầu (ambient loop).
  if (stepIndex.value >= steps.value.length - 1) stepIndex.value = 0;
  isPlaying.value = true;
  const intervalMs = Math.max(80, Math.round(380 / speed.value));
  stepTimer.value = setInterval(() => {
    if (stepIndex.value < steps.value.length - 1) {
      stepIndex.value++;
    } else {
      // Hết dãy → dừng 1 nhịp rồi lặp nhẹ (không giật vòng lại ngay).
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

onMounted(() => {
  loadDemo(activeKey.value);
  play();
});

onUnmounted(stopPlayback);

/** Block status (engine ElementStatus) → class màu block-token (DESIGN §2.1). */
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

/* ── Algorithm Catalog Grid — filter theo nhóm (key prefix) + count mono ── */
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
  if (searchQuery.value.trim()) {
    return filteredCatalog.value;
  }
  if (isCatalogExpanded.value) {
    return filteredCatalog.value;
  }
  return filteredCatalog.value.slice(0, 8);
});

const canExpandCatalog = computed(() => {
  return !searchQuery.value.trim() && filteredCatalog.value.length > 8;
});

function levelLabel(level: CatalogMeta['level']): string {
  return level === 'advanced' ? messages.explore.levelAdvanced : messages.explore.levelBasic;
}

/* ── Practice Ladder Showcase — 4 chặng: Trực quan → Thí nghiệm → Code → Đánh giá ── */
const ladderStages: Array<{ icon: Component; title: string; desc: string }> = [
  { icon: Eye, title: messages.home.ladderStage1Title, desc: messages.home.ladderStage1Desc },
  { icon: FlaskConical, title: messages.home.ladderStage2Title, desc: messages.home.ladderStage2Desc },
  { icon: Code2, title: messages.home.ladderStage3Title, desc: messages.home.ladderStage3Desc },
  { icon: Trophy, title: messages.home.ladderStage4Title, desc: messages.home.ladderStage4Desc },
];
</script>

<template>
  <main class="home">
    <!-- Hero — surface band level-2 (§6), KHÔNG gradient/blob; panel demo tối chạy engine thật -->
    <section class="home__hero">
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

        <!-- Panel demo tối — mini-sim: step thật từ engine, block thở theo bước,
             bộ điều khiển tương tác (Task 1): play/pause · step back/forward · reset · speed -->
        <div class="home__bench" aria-label="Mô phỏng trực quan đang chạy — mở mô phỏng để tương tác từng bước">
          <!-- Top terminal / workbench mockup bar -->
          <div class="home__bench-macbar" aria-hidden="true">
            <div class="home__bench-dots">
              <span class="home__bench-dot home__bench-dot--close" />
              <span class="home__bench-dot home__bench-dot--min" />
              <span class="home__bench-dot home__bench-dot--max" />
            </div>
            <span class="home__bench-caption">dsa-visual-runner.exe</span>
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

          <!-- Callout bước — dạng trace/code line: gutter L{line} + explanation (Step thật) -->
          <p class="home__bench-trace" :aria-live="isPlaying ? 'off' : 'polite'">
            <span class="home__bench-trace-gutter" aria-hidden="true">{{ traceLine }}</span>
            <span class="home__bench-trace-text">{{ currentStep?.explanation || messages.home.simStepHint }}</span>
          </p>

          <!-- Bộ điều khiển mini-sim (Task 1) — icon-only + speed slider -->
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

    <!-- Số liệu (SDD Màn 01 — nguồn danh mục nội dung; 1 hero-stat level-2 = BlockToken,
         stat phụ level-1 có icon Lucide, không icon tròn đổi màu — DESIGN §6) -->
    <section class="home__stats container" aria-label="Thống kê">
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

    <!-- 3 demo công khai (FR-7.6) — Card shadcn level-1, badge CTDL + chip Big-O mono + CTA -->
    <section class="home__section container">
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
            <!-- Thumbnail tối — mini illustration khác nhau theo type (sort bars /
                 search blocks / graph chain), dùng token engine trên canvas-ink -->
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

    <!-- Algorithm Catalog Grid (Task 1) — Live Search + Filter Tabs + Compact 8 Featured Cards:
         title Geist 600 · badge CTDL · chip Big-O mono · CTA dịch phải khi hover -->
    <section class="home__section container">
      <div class="home__section-head">
        <span class="home__kicker home__kicker--center">
          <span class="font-mono">{{ messages.home.catalogBadge }}</span>
        </span>
        <h2 class="home__section-title">{{ messages.home.catalogTitle }}</h2>
        <p class="home__section-desc">{{ messages.home.catalogDesc }}</p>
      </div>

      <div class="home__catalog-toolbar">
        <!-- Live Search Input -->
        <div class="home__catalog-search">
          <Search class="home__catalog-search-icon" :size="16" aria-hidden="true" />
          <input
            v-model="searchQuery"
            type="search"
            class="home__catalog-search-input"
            :placeholder="messages.home.catalogSearchPlaceholder"
            :aria-label="messages.home.catalogSearchPlaceholder"
          />
          <button
            v-if="searchQuery"
            type="button"
            class="home__catalog-search-clear"
            aria-label="Xóa tìm kiếm"
            @click="searchQuery = ''"
          >
            <X :size="14" aria-hidden="true" />
          </button>
        </div>

        <!-- Filter tabs -->
        <div class="home__filters" role="group" :aria-label="messages.home.catalogTitle">
          <Button
            v-for="group in catalogGroups"
            :key="group.key"
            type="button"
            variant="ghost"
            size="sm"
            class="home__filter"
            :class="{ 'home__filter--active': group.key === activeGroup }"
            :aria-pressed="group.key === activeGroup"
            @click="activeGroup = group.key"
          >
            {{ group.label }}
            <span class="home__filter-count" aria-hidden="true">{{ catalogCounts[group.key] }}</span>
          </Button>
        </div>
      </div>

      <!-- Empty state when search has no results -->
      <div v-if="displayedCatalog.length === 0" class="home__catalog-empty" role="status">
        <p class="home__catalog-empty-text">{{ messages.home.catalogNoResults }}</p>
        <Button variant="secondary" size="sm" @click="searchQuery = ''; activeGroup = 'all'">
          {{ messages.common.retry }}
        </Button>
      </div>

      <!-- Catalog Cards Grid (max 8 by default) -->
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

      <!-- Expand / Collapse Button -->
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

    <!-- Practice Ladder Showcase (Task 1) — 4 chặng, index mono STEP 01..04, icon lucide + Step Flow Connectors -->
    <section class="home__section container">
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
          <!-- Step flow connector line to next step -->
          <div v-if="index < ladderStages.length - 1" class="home__ladder-flow-line" aria-hidden="true">
            <div class="home__ladder-flow-track" />
            <span class="home__ladder-flow-dot" />
          </div>
        </div>
      </div>
    </section>

    <!-- Feature highlight (SDD Màn 01 — 3 tính năng chính; Visualizer + Learning Path
         nổi bậc level-2 (DESIGN §6), Practice là dải gọn level-1) -->
    <section class="home__section container">
      <div class="home__grid home__grid--features">
        <Card class="home__feature home__feature--featured home__feature--visual">
          <CardHeader>
            <div class="home__feature-icon" aria-hidden="true">
              <Play :size="24" />
            </div>
            <CardTitle class="home__feature-title">{{ messages.home.featureVisual.title }}</CardTitle>
            <CardDescription>{{ messages.home.featureVisual.desc }}</CardDescription>
          </CardHeader>
        </Card>

        <Card class="home__feature home__feature--featured home__feature--path">
          <CardHeader>
            <div class="home__feature-icon" aria-hidden="true">
              <Map :size="24" />
            </div>
            <CardTitle class="home__feature-title">{{ messages.home.featurePath.title }}</CardTitle>
            <CardDescription>{{ messages.home.featurePath.desc }}</CardDescription>
          </CardHeader>
        </Card>

        <Card class="home__feature home__feature--compact">
          <CardHeader class="home__feature-header-row">
            <div class="home__feature-icon" aria-hidden="true">
              <Target :size="20" />
            </div>
            <div>
              <CardTitle class="home__feature-title">{{ messages.home.featurePractice.title }}</CardTitle>
              <CardDescription>{{ messages.home.featurePractice.desc }}</CardDescription>
            </div>
          </CardHeader>
        </Card>
      </div>
    </section>
  </main>
</template>

<style scoped>
.home {
  display: flex;
  flex-direction: column;
  gap: var(--space-2xl);
  padding-bottom: var(--space-3xl);
}

/* ── Hero band — surface level-2, luminance stacking (KHÔNG gradient, KHÔNG blob) ── */
.home__hero { background: var(--color-card-raised); border-bottom: 1px solid var(--color-border-subtle); }

.home__hero-grid {
  display: grid;
  grid-template-columns: 1fr;
  gap: var(--space-xl);
  padding-block: var(--space-2xl) var(--space-xl);
}

@media (min-width: 900px) {
  .home__hero-grid {
    grid-template-columns: repeat(12, minmax(0, 1fr));
    align-items: center;
    padding-block: var(--space-3xl);
  }

  .home__hero-copy { grid-column: span 7; }
  .home__bench { grid-column: span 5; }
}

.home__hero-copy {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: var(--space-md);
}

/* Kicker mono — label ngắn, không phải heading */
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
  /* DESIGN §3: H1 = 600 — CẤM 700 ở heading (giữ 600 dù bản nháp đề xuất 700) */
  font-weight: 600;
  line-height: 1.1;
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
  margin-top: var(--space-sm);
}

/* ── Bench demo — vùng dữ liệu LUÔN tối (canvas-ink) bất kể theme;
   viền tinh tế chuyển tiếp mềm mại (Task 1) ── */
.home__bench {
  background: var(--color-canvas-ink);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: var(--radius-lg);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  box-shadow: 0 12px 32px -8px rgba(15, 23, 42, 0.35), 0 0 0 1px rgba(255, 255, 255, 0.04);
  animation: bench-in 400ms cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes bench-in {
  from { opacity: 0; transform: translateY(12px); }
  to { opacity: 1; transform: translateY(0); }
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
  animation: bench-live 2s cubic-bezier(0.16, 1, 0.3, 1) infinite;
}

@keyframes bench-live {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.25; }
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
  white-space: nowrap;
}

/* Stage — block thở theo bước (block-token + index mono) */
.home__bench-stage {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  gap: var(--space-sm);
  min-height: 72px;
  animation: stage-in 250ms cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes stage-in {
  from { opacity: 0; transform: translateY(6px); }
  to { opacity: 1; transform: translateY(0); }
}

.home__block {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  width: 44px;
  height: 56px;
  border-radius: var(--radius-md);
}

/* Mono dùng chung (bench + demo + chips) — chỉ gom font-family, không đổi giá trị khác */
.home__bench-head,
.home__block-value,
.home__block-index,
.home__bench-trace,
.home__bench-speed-value,
.home__filter-count,
.home__demo-meta,
.home__demo-level,
.home__catalog-level,
.home__chip { font-family: var(--font-mono); }

.home__block-value {
  font-size: var(--text-sm);
  font-weight: 500;
  color: rgba(255, 255, 255, 0.92);
}

.home__block-index {
  font-size: var(--text-xs);
  color: var(--color-index-muted);
}

/* Trạng thái thuật toán (engine ElementStatus → ngôn ngữ dữ liệu §2.1) */
.home__block--default { background: var(--color-data-core); }
.home__block--active { background: var(--color-data-core); box-shadow: inset 0 0 0 2px rgba(255, 255, 255, 0.35); }
.home__block--swap { background: var(--color-conflict); animation: bench-pop 240ms cubic-bezier(0.16, 1, 0.3, 1) both; }
.home__block--done { background: var(--color-resolved); }
.home__block--muted { background: var(--color-data-core); opacity: 0.4; }

@keyframes bench-pop {
  0% { transform: scale(1); }
  50% { transform: scale(1.1); }
  100% { transform: scale(1); }
}

/* Trace callout (Task 1) — code line: gutter L{line} data-core + explanation */
.home__bench-trace {
  margin: 0;
  display: flex;
  gap: var(--space-sm);
  align-items: baseline;
  min-height: 40px;
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.06);
}

.home__bench-trace-gutter {
  flex-shrink: 0;
  font-size: var(--text-xs);
  font-weight: 500;
  color: var(--color-data-core);
}

.home__bench-trace-text {
  font-size: var(--text-xs);
  line-height: 1.5;
  color: rgba(255, 255, 255, 0.68);
}

/* Bộ điều khiển mini-sim (Task 1) — icon-only ghost trên nền tối + speed slider */
.home__bench-controls {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: var(--space-xs);
}

.home__bench-ctrl { color: var(--color-index-muted); }

.home__bench-ctrl:hover {
  color: rgba(255, 255, 255, 0.92);
  background: rgba(255, 255, 255, 0.08);
}

.home__bench-ctrl:disabled { opacity: 0.35; }

.home__bench-speed {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  margin-left: auto;
}

.home__bench-speed-label { font-size: var(--text-xs); color: var(--color-index-muted); }

.home__bench-speed-input {
  width: 88px;
  accent-color: var(--color-data-core);
  cursor: pointer;
}

.home__bench-speed-value {
  min-width: 2.5em;
  text-align: right;
  font-size: var(--text-xs);
  color: rgba(255, 255, 255, 0.85);
  font-variant-numeric: tabular-nums;
}

/* Selector demo — segmented trên nền tối (Button ghost + active rõ) */
.home__bench-tabs { display: flex; gap: var(--space-sm); flex-wrap: wrap; }
.home__bench-tab { color: var(--color-index-muted); }

.home__bench-tab:hover {
  color: rgba(255, 255, 255, 0.9);
  background: rgba(255, 255, 255, 0.08);
}

.home__bench-tab--active {
  background: rgba(255, 255, 255, 0.14);
  color: rgba(255, 255, 255, 0.95);
}

/* ── Stats — band level-2 (DESIGN §6): 1 hero-stat (BlockToken) + stat phụ level-1
   có icon Lucide trung tính; không shadow, không icon tròn đổi màu ── */
.home__stats {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-xl);
}

.home__stats-head {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
  text-align: center;
}

.home__stats-note { margin: 0; font-size: var(--text-xs); color: var(--color-text-tertiary); }

.home__stats-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: var(--space-md);
}

.home__stat-hero { grid-column: span 2; width: 100%; }

.home__stat {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: var(--space-xs);
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-lg);
}

.home__stat-icon { color: var(--color-text-tertiary); }

.home__stat-value {
  font-size: var(--text-2xl);
  font-weight: 600;
  letter-spacing: -0.015em;
  color: var(--color-foreground);
}

.home__stat-label { font-size: var(--text-xs); color: var(--color-text-tertiary); }

/* ── Sections chung ── */
.home__section { display: flex; flex-direction: column; gap: var(--space-lg); }

.home__section-head {
  text-align: center;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-sm);
}

.home__section-title {
  margin: 0;
  font-size: var(--text-3xl);
  font-weight: 600;
  letter-spacing: -0.025em;
  color: var(--color-foreground);
}

.home__section-desc {
  margin: 0;
  max-width: 56ch;
  color: var(--color-text-secondary);
  font-size: var(--text-sm);
}

.home__grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
  gap: var(--space-lg);
}

.home__grid--features { grid-template-columns: repeat(12, minmax(0, 1fr)); }

/* Feature cards — featured level-2 (DESIGN §6), compact là dải level-1 gọn hơn */
.home__feature--featured {
  background: var(--color-card-raised);
  border-color: var(--color-border-subtle);
}

.home__feature--visual { grid-column: span 8; }
.home__feature--path { grid-column: span 4; }
.home__feature--compact { grid-column: 1 / -1; }

.home__feature-header-row { display: flex; align-items: center; gap: var(--space-lg); }

.home__feature-header-row .home__feature-icon { margin-bottom: 0; flex-shrink: 0; }

.home__feature-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-text-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  margin-bottom: var(--space-sm);
}

/* Card demo — level-1, hover chỉ đổi border (§4.2), không hover-lift/shadow */
.home__demo { display: flex; flex-direction: column; transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1); }
.home__demo:hover { border-color: var(--color-border-strong); }

/* Thumbnail tối — mini illustration theo type trên canvas-ink (LUÔN tối bất kể theme) */
.home__demo-thumb {
  height: 88px;
  border-radius: var(--radius-md);
  background: var(--color-canvas-ink);
  border: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: var(--space-md);
}

/* Sort — dãy bar tiến trình sắp xếp (data-core → resolved) */
.home__thumb-bars { display: flex; align-items: flex-end; gap: 4px; height: 48px; }

.home__thumb-bar { width: 14px; border-radius: var(--radius-sm) var(--radius-sm) 2px 2px; background: var(--color-data-core); }

.home__thumb-bar:nth-child(1) { height: 40%; }
.home__thumb-bar:nth-child(2) { height: 65%; }
.home__thumb-bar:nth-child(3) { height: 50%; }
.home__thumb-bar:nth-child(4) { height: 80%; }
.home__thumb-bar:nth-child(5) { height: 100%; }

/* Search — block với phần tử tìm thấy ở giữa (resolved + ring) */
.home__thumb-row { display: flex; align-items: center; gap: 4px; }

.home__thumb-block { width: 18px; height: 24px; border-radius: var(--radius-sm); background: var(--color-data-core); }

.home__thumb-block--found {
  background: var(--color-resolved);
  box-shadow: inset 0 0 0 2px rgba(255, 255, 255, 0.35);
}

/* Graph — chuỗi node-cạnh, node đã duyệt (resolved) */
.home__thumb-graph { display: flex; align-items: center; }

.home__thumb-node { width: 14px; height: 14px; border-radius: var(--radius-full); background: var(--color-data-core); }

/* Phần tử tô resolved — gộp selector cùng body (bar sort + node graph) */
.home__thumb-bar--done,
.home__thumb-node--visited { background: var(--color-resolved); }

.home__thumb-edge { width: 18px; height: 2px; border-radius: 1px; background: var(--color-index-muted); }

.home__demo-title {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-md);
  font-weight: 600;
  line-height: 1.3;
  letter-spacing: -0.015em;
}

.home__demo-title-icon { color: var(--color-text-tertiary); flex-shrink: 0; }

/* Meta row: badge CTDL + level mono (Task 1 — thay chuỗi "Mảng · Cấp độ 1") */
.home__demo-meta {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: var(--space-sm);
  font-size: var(--text-xs);
}

.home__demo-badge { font-weight: 500; }

.home__demo-level { color: var(--color-text-tertiary); text-transform: uppercase; letter-spacing: 0.08em; }

.home__demo-content {
  margin-top: auto;
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

/* Chip Big-O mono (DESIGN §4.3) — dùng chung demo + catalog */
.home__demo-chips,
.home__catalog-chips {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
}

.home__chip {
  display: inline-flex;
  align-items: center;
  min-height: 24px;
  padding: 2px var(--space-sm);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
  background: var(--color-card);
  font-variant-numeric: tabular-nums;
}

  /* ── Algorithm Catalog Grid (Task 1) ── */
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

.home__catalog-search-clear:hover {
  color: var(--color-foreground);
  background: var(--color-muted);
}

.home__filters {
  display: flex;
  flex-wrap: wrap;
  justify-content: center;
  gap: var(--space-sm);
}

/* FIX R1: mobile — filter tabs cuộn ngang trong 1 hàng (không tràn/không wrap nát) */
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

.home__filter-count { font-size: var(--text-xs); opacity: 0.75; font-variant-numeric: tabular-nums; }

.home__catalog-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--space-md);
  padding: var(--space-2xl) var(--space-lg);
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-card-raised);
  text-align: center;
}

.home__catalog-empty-text {
  margin: 0;
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
}

.home__catalog {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: var(--space-md);
}

/* Card dense level-1 — hover chỉ đổi border-strong (§4.2), không shadow */
.home__catalog-card {
  display: flex;
  flex-direction: column;
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.home__catalog-card:hover { border-color: var(--color-border-strong); }

.home__catalog-head { padding: var(--space-md); gap: var(--space-sm); }

.home__catalog-title {
  font-size: var(--text-md);
  font-weight: 600;
  line-height: 1.35;
  letter-spacing: -0.015em;
}

.home__catalog-meta {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: var(--space-sm);
  font-size: var(--text-xs);
}

.home__catalog-badge { font-weight: 500; }

.home__catalog-level { color: var(--color-text-tertiary); text-transform: uppercase; letter-spacing: 0.08em; }

.home__catalog-content {
  margin-top: auto;
  padding: var(--space-sm) var(--space-md) var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

/* CTA "Thực hành ngay" — dịch phải 4px khi hover (Task 1) */
.home__catalog-cta {
  transition: transform 180ms cubic-bezier(0.16, 1, 0.3, 1), background-color 150ms ease, color 150ms ease;
}

.home__catalog-cta:hover { transform: translateX(4px); }

.home__catalog-expand {
  display: flex;
  justify-content: center;
  padding-top: var(--space-md);
}

.home__catalog-expand-btn {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  font-weight: 500;
}

/* ── Practice Ladder Showcase (Task 1) — 4 chặng + connector flow ── */
.home__ladder {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: var(--space-md);
  position: relative;
}

@media (max-width: 900px) {
  .home__ladder { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}

.home__ladder-step-wrapper {
  position: relative;
  display: flex;
  flex-direction: column;
}

.home__ladder-card {
  display: flex;
  flex-direction: column;
  height: 100%;
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.home__ladder-card:hover { border-color: var(--color-border-strong); }

.home__ladder-content {
  padding: var(--space-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  height: 100%;
}

.home__ladder-top {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: var(--space-xs);
}

.home__ladder-step {
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
  padding: 2px var(--space-sm);
  min-height: 24px;
  display: inline-flex;
  align-items: center;
  letter-spacing: 0.04em;
}

.home__ladder-icon {
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-text-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.home__ladder-title {
  margin: 0;
  font-size: var(--text-md);
  font-weight: 600;
  letter-spacing: -0.015em;
  color: var(--color-foreground);
}

.home__ladder-desc {
  margin: 0;
  font-size: var(--text-sm);
  line-height: 1.55;
  color: var(--color-text-secondary);
}

/* Connector line nối giữa các bước */
.home__ladder-flow-line {
  display: none;
}

@media (min-width: 900px) {
  .home__ladder-flow-line {
    display: flex;
    align-items: center;
    justify-content: center;
    position: absolute;
    top: 36px;
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

@media (max-width: 900px) {
  .home__feature--visual,
  .home__feature--path { grid-column: 1 / -1; }

  .home__feature-header-row {
    flex-direction: column;
    align-items: flex-start;
  }

  .home__feature-header-row .home__feature-icon { margin-bottom: var(--space-sm); }
}

@media (max-width: 640px) {
  /* §8: mobile giảm 1 bậc spacing — section gap 32px */
  .home { gap: var(--space-xl); padding-bottom: var(--space-2xl); }
  .home__stats-grid { grid-template-columns: 1fr; }
  .home__stat-hero { grid-column: auto; }
  .home__title { font-size: var(--text-3xl); }

  /* Bench: speed slider gọn lại trên màn nhỏ */
  .home__bench-speed { margin-left: 0; }
  .home__bench-speed-input { width: 64px; }
  .home__bench-step { display: none; }

  .home__ladder { grid-template-columns: 1fr; }

  /* FIX R1: CTA "Thực hành ngay" rõ hơn ở mobile — full width + hit target 44px (§8) */
  .home__catalog-cta { width: 100%; min-height: 44px; }
}
</style>
