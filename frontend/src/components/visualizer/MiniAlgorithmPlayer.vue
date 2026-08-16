<script setup lang="ts">
// MiniAlgorithmPlayer.vue — Reusable mini algorithm visualizer player
// Uses DataStructureStage (PixiJS WebGL + GSAP + Micro-SFX)
// Ideal for HomeView hero section and SimulationsView catalog previews.
//
// B4 — DEAD CODE (giữ nguyên): hiện chưa view nào import component này.
// Không sửa/xoá vì có thể tái dùng sau; mọi thay đổi phải cân nhắc trước khi kích hoạt.

import { computed, onMounted, onUnmounted, ref, shallowRef } from 'vue';
import { Play, Pause, RotateCcw } from 'lucide-vue-next';
import type { InputConfig, Step } from '@/engines/core/types';
import { CATALOG } from '@/engines/catalog';
import { getSimulation } from '@/engines/registry';
import { messages } from '@/i18n/vi';
import DataStructureStage from './DataStructureStage.vue';
import Button from '@/components/ui/Button.vue';

const props = withDefaults(
  defineProps<{
    initialKey?: string;
    keys?: string[];
    autoplay?: boolean;
    stepInterval?: number;
    height?: number | string;
    showTabs?: boolean;
    showControls?: boolean;
  }>(),
  {
    initialKey: 'sort.bubble',
    keys: () => ['sort.bubble', 'search.binary', 'graph.bfs'],
    autoplay: true,
    stepInterval: 480,
    height: 320,
    showTabs: true,
    showControls: true,
  },
);

const DEMO_INPUTS: Record<string, InputConfig> = {
  'sort.bubble': { kind: 'array', data: { values: [5, 3, 8, 1, 9, 2] } },
  'search.binary': { kind: 'array', data: { target: 19, inputSource: 'manual', values: [2, 5, 8, 12, 19, 23] } },
  'graph.bfs': { kind: 'graph', data: { preset: 'path', directed: false, weighted: false, vertices: 6, edges: 5, source: 0 } },
  'tree.bst-insert': { kind: 'tree', data: { values: [50, 30, 70, 20, 40, 60, 80] } },
  'sort.quick': { kind: 'array', data: { values: [6, 2, 8, 4, 9, 1] } },
};

const activeKey = ref<string>(props.initialKey);
const steps = shallowRef<Step[]>([]);
const stepIndex = ref(0);
const isPlaying = ref(props.autoplay);
const stepTimer = ref<ReturnType<typeof setInterval> | null>(null);
const restartTimer = ref<ReturnType<typeof setTimeout> | null>(null);

const activeMeta = computed(() => CATALOG.find((c) => c.key === activeKey.value));
const currentStep = computed(() => steps.value[stepIndex.value] ?? null);
const currentStructure = computed(() => currentStep.value?.structure ?? null);
const stepLabel = computed(() => messages.home.simStepOf(stepIndex.value + 1, steps.value.length));

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
  if (prefersReducedMotion()) {
    isPlaying.value = false;
    return;
  }
  isPlaying.value = true;
  stepTimer.value = setInterval(() => {
    if (stepIndex.value < steps.value.length - 1) {
      stepIndex.value++;
    } else {
      // Pause at end, then loop back
      stopPlayback();
      restartTimer.value = setTimeout(() => {
        stepIndex.value = 0;
        if (isPlaying.value) {
          startPlayback();
        }
      }, 1600);
    }
  }, props.stepInterval);
}

function togglePlay(): void {
  if (isPlaying.value) {
    isPlaying.value = false;
    stopPlayback();
  } else {
    isPlaying.value = true;
    startPlayback();
  }
}

function resetSimulation(): void {
  stopPlayback();
  stepIndex.value = 0;
  if (isPlaying.value) {
    startPlayback();
  }
}

function loadDemo(key: string): void {
  const gen = getSimulation(key);
  if (!gen) return;
  const input = DEMO_INPUTS[key] ?? { kind: 'array', data: { values: [5, 2, 8, 4, 7] } };
  steps.value = gen.generate(input);
  stepIndex.value = 0;
}

function selectDemo(key: string): void {
  if (key === activeKey.value) return;
  activeKey.value = key;
  loadDemo(key);
  if (isPlaying.value) {
    startPlayback();
  }
}

function getTabLabel(key: string): string {
  if (key === 'sort.bubble') return messages.home.demoBubble;
  if (key === 'search.binary') return messages.home.demoBinary;
  if (key === 'graph.bfs') return messages.home.demoBfs;
  const meta = CATALOG.find((c) => c.key === key);
  return meta?.title ?? key;
}

onMounted(() => {
  loadDemo(activeKey.value);
  if (props.autoplay && !prefersReducedMotion()) {
    startPlayback();
  }
});

onUnmounted(stopPlayback);
</script>

<template>
  <div class="mini-player" aria-label="Bộ phát trực quan mini WebGL">
    <!-- Header bar -->
    <div class="mini-player__head">
      <span class="mini-player__live">
        <span class="mini-player__live-dot" aria-hidden="true" />
        {{ messages.home.simLive }}
      </span>
      <span class="mini-player__key">{{ activeMeta?.key }}</span>
      <span class="mini-player__step">{{ stepLabel }}</span>

      <!-- Mini play/pause controls -->
      <div v-if="showControls" class="mini-player__controls">
        <Button
          type="button"
          variant="ghost"
          size="icon"
          class="mini-player__ctrl-btn"
          :aria-label="isPlaying ? 'Tạm dừng' : 'Phát tiếp'"
          @click="togglePlay"
        >
          <component :is="isPlaying ? Pause : Play" :size="13" aria-hidden="true" />
        </Button>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          class="mini-player__ctrl-btn"
          aria-label="Chạy lại từ đầu"
          @click="resetSimulation"
        >
          <RotateCcw :size="13" aria-hidden="true" />
        </Button>
      </div>
    </div>

    <!-- Data Structure Stage WebGL -->
    <div class="mini-player__stage-wrap">
      <DataStructureStage
        :structure="currentStructure"
        :show-index="true"
        :show-values="true"
        :enable-sound="true"
        :height="height"
        :sim-key="activeKey"
        :empty-text="activeMeta?.title ?? 'Đang tải thuật toán...'"
      />
    </div>

    <!-- Explanation footer -->
    <p class="mini-player__explain">
      {{ currentStep?.explanation || 'Đang thực thi giải thuật...' }}
    </p>

    <!-- Selector tabs -->
    <div v-if="showTabs && keys.length > 1" class="mini-player__tabs" role="group" aria-label="Chọn mô phỏng demo">
      <Button
        v-for="k in keys"
        :key="k"
        type="button"
        variant="ghost"
        size="sm"
        class="mini-player__tab"
        :class="{ 'mini-player__tab--active': k === activeKey }"
        :aria-pressed="k === activeKey"
        @click="selectDemo(k)"
      >
        {{ getTabLabel(k) }}
      </Button>
    </div>
  </div>
</template>

<style scoped>
.mini-player {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm, 10px);
  padding: var(--space-md, 16px);
  background: var(--color-surface, #0f172a);
  border: 1px solid var(--color-border, #1e293b);
  border-radius: var(--radius-xl, 12px);
  box-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.3);
}

.mini-player__head {
  display: flex;
  align-items: center;
  gap: var(--space-md, 12px);
  font-size: var(--text-xs, 12px);
  color: var(--color-text-muted, #94a3b8);
  flex-wrap: wrap;
}

.mini-player__live {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-weight: 600;
  color: #34d399;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  font-size: 11px;
}

.mini-player__live-dot {
  width: 7px;
  height: 7px;
  border-radius: 999px;
  background: #34d399;
  box-shadow: 0 0 8px #34d399;
  animation: pulse-dot 1.8s ease-in-out infinite;
}

@keyframes pulse-dot {
  0%, 100% { transform: scale(1); opacity: 1; }
  50% { transform: scale(1.3); opacity: 0.6; }
}

.mini-player__key {
  font-family: 'JetBrains Mono', monospace;
  color: #5eead4;
  background: rgba(94, 234, 212, 0.1);
  padding: 2px 6px;
  border-radius: var(--radius-sm, 4px);
}

.mini-player__step {
  margin-left: auto;
  font-family: 'JetBrains Mono', monospace;
}

.mini-player__controls {
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

.mini-player__ctrl-btn {
  width: 24px;
  height: 24px;
  padding: 0;
  color: var(--color-text-muted, #94a3b8);
}

.mini-player__ctrl-btn:hover {
  color: var(--color-foreground, #f8fafc);
  background: var(--color-muted, #1e293b);
}

.mini-player__stage-wrap {
  width: 100%;
}

.mini-player__explain {
  font-size: var(--text-xs, 12px);
  color: var(--color-text-muted, #94a3b8);
  min-height: 20px;
  line-height: 1.4;
  margin: 0;
  background: rgba(15, 23, 42, 0.5);
  padding: 6px 10px;
  border-radius: var(--radius-sm, 4px);
  border-left: 2px solid #5eead4;
}

.mini-player__tabs {
  display: flex;
  gap: 6px;
  flex-wrap: wrap;
}

.mini-player__tab {
  font-size: 11px;
  height: 28px;
  padding: 0 10px;
  color: var(--color-text-muted, #94a3b8);
}

.mini-player__tab--active {
  background: var(--color-muted, #1e293b);
  color: #5eead4;
  font-weight: 600;
}
</style>
