<script setup lang="ts">
import { computed, onMounted, onUnmounted, ref, shallowRef } from 'vue';
import type { InputConfig, Step } from '@/engines/core/types';
import { CATALOG } from '@/engines/catalog';
import { getSimulation } from '@/engines/registry';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';

const DEMO_KEYS = ['sort.bubble', 'search.binary', 'graph.bfs'] as const;

const DEMO_INPUTS: Record<string, InputConfig> = {
  'sort.bubble': { kind: 'array', data: { values: [5, 3, 8, 1, 9, 2] } },
  'search.binary': { kind: 'array', data: { target: 19, inputSource: 'manual', values: [2, 5, 8, 12, 19, 23] } },
  'graph.bfs': { kind: 'graph', data: { preset: 'path', directed: false, weighted: false, vertices: 6, edges: 5, source: 0 } },
};

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
    label:
      key === 'sort.bubble'
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
  if (prefersReducedMotion()) return;
  stepTimer.value = setInterval(() => {
    if (stepIndex.value < steps.value.length - 1) {
      stepIndex.value++;
    } else {
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

onUnmounted(() => {
  stopPlayback();
});

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
  <div class="home__bench" aria-label="Mô phỏng trực quan đang chạy — mở mô phỏng để tương tác từng bước">
    <div class="home__bench-bar">
      <span class="home__bench-dot home__bench-dot--red" />
      <span class="home__bench-dot home__bench-dot--yellow" />
      <span class="home__bench-dot home__bench-dot--green" />
      <span class="home__bench-title">{{ messages.home.benchTitle }}</span>
      <span class="home__bench-live">
        <span class="home__bench-live-dot" aria-hidden="true" />
        {{ messages.home.simLive }}
      </span>
    </div>

    <div class="home__bench-head">
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
      {{ currentStep?.explanation ?? messages.home.benchReady }}
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
</template>

<style scoped>
.home__bench {
  position: relative;
  background: rgba(10, 9, 16, 0.82);
  -webkit-backdrop-filter: blur(18px) saturate(1.4);
  backdrop-filter: blur(18px) saturate(1.4);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: var(--radius-xl, 16px);
  padding: 0;
  display: flex;
  flex-direction: column;
  overflow: hidden;
  box-shadow: 0 24px 60px rgba(0, 0, 0, 0.45), inset 0 1px 0 rgba(255, 255, 255, 0.04);
  animation: bench-in 600ms cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes bench-in {
  from { opacity: 0; transform: translateY(18px) scale(0.98); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

.home__bench-bar {
  display: flex;
  align-items: center;
  gap: 7px;
  padding: 10px 14px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
  background: rgba(255, 255, 255, 0.02);
}

.home__bench-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
}

.home__bench-dot--red { background: #b85c5c; }
.home__bench-dot--yellow { background: #c9a227; }
.home__bench-dot--green { background: #a855f7; }

.home__bench-title {
  margin-left: 6px;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: rgba(255, 255, 255, 0.38);
}

.home__bench-live {
  margin-left: auto;
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-family: var(--font-mono);
  font-size: 10px;
  letter-spacing: 0.1em;
  color: #c084fc;
}

.home__bench-live-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: #c084fc;
  box-shadow: 0 0 8px #a855f7;
  animation: bench-live 2s cubic-bezier(0.16, 1, 0.3, 1) infinite;
}

@keyframes bench-live {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.2; }
}

.home__bench-head {
  display: flex;
  align-items: center;
  gap: var(--space-md, 16px);
  padding: 14px 18px 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
}

.home__bench-key {
  color: #c084fc;
}

.home__bench-key::before {
  content: '>';
  margin-right: 6px;
  color: rgba(255, 255, 255, 0.38);
}

.home__bench-step {
  margin-left: auto;
  color: rgba(255, 255, 255, 0.38);
  white-space: nowrap;
}

.home__bench-stage {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  gap: var(--space-sm, 8px);
  min-height: 88px;
  padding: 14px 18px 6px;
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
  height: 58px;
  border-radius: var(--radius-md, 8px);
  border: 1px solid rgba(255, 255, 255, 0.05);
}

.home__block-value {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 600;
  color: rgba(255, 255, 255, 0.92);
}

.home__block-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: rgba(255, 255, 255, 0.35);
}

.home__block--default { background: #181225; border-color: rgba(168, 85, 247, 0.35); }
.home__block--active { background: #7c3aed; box-shadow: 0 0 14px rgba(168, 85, 247, 0.4); }
.home__block--swap { background: #b85c5c; animation: bench-pop 240ms cubic-bezier(0.16, 1, 0.3, 1) both; }
.home__block--done { background: #a855f7; box-shadow: 0 0 10px rgba(168, 85, 247, 0.35); }
.home__block--muted { background: #181225; opacity: 0.4; }

@keyframes bench-pop {
  0% { transform: scale(1); }
  50% { transform: scale(1.12); }
  100% { transform: scale(1); }
}

.home__bench-explain {
  margin: 0;
  min-height: 38px;
  padding: 6px 18px 4px;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  line-height: 1.55;
  color: rgba(255, 255, 255, 0.55);
}

.home__bench-tabs {
  display: flex;
  gap: var(--space-sm, 8px);
  flex-wrap: wrap;
  padding: 8px 14px 14px;
  border-top: 1px solid rgba(255, 255, 255, 0.05);
  margin-top: 2px;
}

.home__bench-tab {
  color: rgba(255, 255, 255, 0.38);
  border-radius: var(--radius-md, 8px);
}

.home__bench-tab:hover {
  color: rgba(255, 255, 255, 0.9);
  background: rgba(255, 255, 255, 0.07);
}

.home__bench-tab--active {
  background: rgba(168, 85, 247, 0.12);
  color: #c084fc;
  box-shadow: inset 0 0 0 1px rgba(168, 85, 247, 0.35);
}
</style>
