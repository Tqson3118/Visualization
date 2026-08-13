<script setup lang="ts">
// HomeView — Màn 01: trang chủ công khai (SDD Màn 01).
// View-quality (nhóm A): bỏ header nội bộ (AppHeader toàn cục đã render brand+nav —
// trước đây 2 header chồng nhau), bỏ gradient aurora/blob (KILL-LIST) → hero = surface
// band level-2 + panel demo tối `canvas-ink` chạy mô phỏng THẬT từ engines (3 demo công
// khai catalog.ts: sort.bubble / search.binary / graph.bfs) render bằng DOM block
// (block-token + index mono, status data-core/resolved/conflict). Icon = lucide-vue-next.
import { computed, onMounted, onUnmounted, ref, shallowRef } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { ArrowRight, ArrowUpDown, Map, Network, Play, Search, Target } from 'lucide-vue-next';

import type { Component } from 'vue';
import type { InputConfig, Step } from '@/engines/core/types';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import { CATALOG } from '@/engines/catalog';
import { getSimulation } from '@/engines/registry';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import { buttonVariants } from '@/components/ui/button';

const router = useRouter();

/** Icon cho 3 demo công khai (FR-7.6: sort.bubble / search.binary / graph.bfs) */
const DEMO_ICONS: Record<string, Component> = {
  'sort.bubble': ArrowUpDown,
  'search.binary': Search,
  'graph.bfs': Network,
};

const FEATURE_ICONS = [Play, Map, Target];

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

/* ── Hero mini-sim: step THẬT từ engine (catalog.ts + registry) ── */
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

const activeMeta = computed(() => CATALOG.find((c) => c.key === activeKey.value));
const currentStep = computed(() => steps.value[stepIndex.value] ?? null);
const frameElements = computed(() => currentStep.value?.structure.elements ?? []);
const stepLabel = computed(() => messages.home.simStepOf(stepIndex.value + 1, steps.value.length));

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
}

function startPlayback(): void {
  stopPlayback();
  // Tôn trọng prefers-reduced-motion: không autoplay, giữ frame đầu tĩnh.
  if (prefersReducedMotion()) return;
  stepTimer.value = setInterval(() => {
    if (stepIndex.value < steps.value.length - 1) {
      stepIndex.value++;
    } else {
      // Hết dãy → dừng 1 nhịp rồi lặp nhẹ (ambient, không giật vòng lại ngay).
      stopPlayback();
      restartTimer.value = setTimeout(() => {
        stepIndex.value = 0;
        startPlayback();
      }, 1400);
    }
  }, 380);
}

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
  startPlayback();
}

onMounted(() => {
  loadDemo(activeKey.value);
  startPlayback();
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

        <!-- Panel demo tối — mini-sim: step thật từ engine, block thở theo bước -->
        <div class="home__bench" aria-label="Mô phỏng trực quan đang chạy — mở mô phỏng để tương tác từng bước">
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

          <p class="home__bench-explain">
            {{ currentStep?.explanation ?? ' ' }}
          </p>

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

    <!-- Số liệu (SDD Màn 01 — nguồn danh mục nội dung; stat phụ level-1, không shadow) -->
    <section class="home__stats container" aria-label="Thống kê">
      <div class="home__stat">
        <span class="home__stat-value">{{ stats.visuals }}+</span>
        <span class="home__stat-label">{{ messages.home.statsVisuals }}</span>
      </div>
      <div class="home__stat">
        <span class="home__stat-value">{{ stats.groups }}</span>
        <span class="home__stat-label">{{ messages.home.statsGroups }}</span>
      </div>
      <div class="home__stat">
        <span class="home__stat-value">{{ stats.levels }}</span>
        <span class="home__stat-label">{{ messages.home.statsLevels }}</span>
      </div>
      <p class="home__stats-note">{{ messages.home.statsNote }}</p>
    </section>

    <!-- 3 demo công khai (FR-7.6) — Card shadcn level-1, CTA qua Button -->
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
            <div class="home__demo-icon" aria-hidden="true">
              <component :is="demo.icon" :size="18" />
            </div>
            <CardTitle class="home__demo-title">{{ demo.title }}</CardTitle>
            <CardDescription class="home__demo-meta">
              {{ demo.dataStructure }} · Cấp độ {{ demo.level }}
            </CardDescription>
          </CardHeader>
          <CardContent class="home__demo-content">
            <dl class="home__demo-complexity">
              <dt>{{ messages.home.demoComplexity }}</dt>
              <dd>TB {{ demo.complexity.average }} · {{ demo.complexity.space }}</dd>
            </dl>
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

    <!-- Feature highlight (SDD Màn 01 — 3 tính năng chính) -->
    <section class="home__section container">
      <div class="home__grid home__grid--features">
        <Card
          v-for="(feature, idx) in [
            { title: messages.home.featureVisual.title, desc: messages.home.featureVisual.desc },
            { title: messages.home.featurePath.title, desc: messages.home.featurePath.desc },
            { title: messages.home.featurePractice.title, desc: messages.home.featurePractice.desc },
          ]"
          :key="feature.title"
        >
          <CardHeader>
            <div class="home__demo-icon" aria-hidden="true">
              <component :is="FEATURE_ICONS[idx]" :size="18" />
            </div>
            <CardTitle class="home__feature-title">{{ feature.title }}</CardTitle>
            <CardDescription>{{ feature.desc }}</CardDescription>
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
.home__hero {
  background: var(--color-card-raised);
  border-bottom: 1px solid var(--color-border-subtle);
}

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
    gap: var(--space-xl);
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

.home__kicker--center {
  justify-content: center;
}

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
  line-height: 1.1;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
}

.home__subtitle {
  margin: 0;
  max-width: 52ch;
  font-size: var(--text-md);
  color: var(--color-text-secondary);
}

.home__cta {
  display: flex;
  gap: var(--space-md);
  flex-wrap: wrap;
  margin-top: var(--space-sm);
}

/* ── Bench demo — vùng dữ liệu LUÔN tối (canvas-ink) bất kể theme ── */
.home__bench {
  background: var(--color-canvas-ink);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: var(--radius-lg);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  animation: bench-in 400ms cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes bench-in {
  from { opacity: 0; transform: translateY(12px); }
  to { opacity: 1; transform: translateY(0); }
}

.home__bench-head {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  font-family: var(--font-mono);
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
.home__bench-step {
  color: var(--color-index-muted);
}

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

.home__block-value {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 500;
  color: rgba(255, 255, 255, 0.92);
}

.home__block-index {
  font-family: var(--font-mono);
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

.home__bench-explain {
  margin: 0;
  min-height: 36px;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  line-height: 1.5;
  color: rgba(255, 255, 255, 0.6);
}

/* Selector demo — segmented trên nền tối (Button ghost + active rõ) */
.home__bench-tabs {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
}

.home__bench-tab {
  color: var(--color-index-muted);
}

.home__bench-tab:hover {
  color: rgba(255, 255, 255, 0.9);
  background: rgba(255, 255, 255, 0.08);
}

.home__bench-tab--active {
  background: rgba(255, 255, 255, 0.14);
  color: rgba(255, 255, 255, 0.95);
}

/* ── Stats — stat phụ level-1 (1 hero/màn = bench panel; không shadow, không gradient) ── */
.home__stats {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: var(--space-md);
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-lg);
}

.home__stat {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: var(--space-xs);
}

.home__stat-value {
  font-size: var(--text-2xl);
  font-weight: 600;
  letter-spacing: -0.015em;
  color: var(--color-foreground);
}

.home__stat-label {
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}

.home__stats-note {
  grid-column: 1 / -1;
  margin: 0;
  text-align: center;
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  padding-top: var(--space-sm);
  border-top: 1px solid var(--color-border-subtle);
}

/* ── Sections chung ── */
.home__section {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

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

.home__grid--features {
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
}

/* Card demo — level-1, hover chỉ đổi border (§4.2), không hover-lift/shadow */
.home__demo {
  display: flex;
  flex-direction: column;
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.home__demo:hover {
  border-color: var(--color-border-strong);
}

.home__demo-icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-text-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  margin-bottom: var(--space-sm);
}

.home__demo-title {
  font-size: var(--text-md);
  font-weight: 600;
  line-height: 1.3;
  letter-spacing: -0.015em;
}

.home__demo-meta {
  margin-top: 4px;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}

.home__demo-content {
  margin-top: auto;
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.home__demo-complexity {
  display: flex;
  justify-content: space-between;
  align-items: baseline;
  gap: var(--space-sm);
  font-size: var(--text-xs);
  border-top: 1px solid var(--color-border-subtle);
  padding-top: var(--space-sm);
  margin: 0;
}

.home__demo-complexity dt {
  font-weight: 500;
  color: var(--color-text-tertiary);
}

.home__demo-complexity dd {
  font-family: var(--font-mono);
  color: var(--color-foreground);
}

.home__feature-title {
  font-size: var(--text-md);
  font-weight: 600;
  letter-spacing: -0.015em;
}

@media (max-width: 640px) {
  .home__stats { grid-template-columns: 1fr; gap: var(--space-md); }
  .home__stats-note { grid-column: 1; }
  .home__title { font-size: var(--text-3xl); }
}
</style>
