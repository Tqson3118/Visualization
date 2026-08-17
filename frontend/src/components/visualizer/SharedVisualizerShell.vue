<script setup lang="ts">
// SharedVisualizerShell — shell visual chung (Workstream B/B1).
// Anatomy: HUD/header · stage (renderer adapter) · playback controls · progress/status ·
// trace drawer · pseudocode + explanation. Nhận nguồn dữ liệu là SharedVisualFrame[]
// từ các adapter (legacy Step[], DSL trace, sandbox frame).
import { computed, onBeforeUnmount, ref } from 'vue';
import { ChevronDown, ChevronUp, X } from 'lucide-vue-next';

import ControlBar from '@/components/simulator/ControlBar.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Button from '@/components/ui/Button.vue';
import type { Structure } from '@/engines/core/types';
import type { SimulationStatus } from '@/stores/simulation';
import type { SharedVisualFrame } from '@/visualizer/types';

import ArrayBarsRenderer from './ArrayBarsRenderer.vue';
import VisualizerTraceDrawer from './VisualizerTraceDrawer.vue';

const props = withDefaults(
  defineProps<{
    frames: SharedVisualFrame[];
    pseudocode?: string[];
    title?: string;
    subtitle?: string;
    defaultExpandedTrace?: boolean;
  }>(),
  {
    pseudocode: () => [],
    title: '',
    subtitle: '',
    defaultExpandedTrace: false,
  },
);

const emit = defineEmits<{ close: [] }>();

const index = ref(0);
const speed = ref(1);
const isPlaying = ref(false);
const showTrace = ref(props.defaultExpandedTrace);
let timer: ReturnType<typeof setInterval> | null = null;

const total = computed(() => props.frames.length);
const currentFrame = computed<SharedVisualFrame | null>(() => props.frames[index.value] ?? null);

const structure = computed<Structure | null>(() => {
  const f = currentFrame.value;
  if (!f) return null;
  const d = f.data as { kind?: string } | null;
  return d && typeof d === 'object' && typeof d.kind === 'string' ? (f.data as Structure) : null;
});
const isArray = computed(() => structure.value?.kind === 'array');
const progressPct = computed(() => (total.value > 0 ? (index.value / (total.value - 1)) * 100 : 0));
const stepLabel = computed(() => (total.value > 0 ? `${index.value + 1} / ${total.value}` : '0'));
const hasVariables = computed(() => {
  const v = currentFrame.value?.variables;
  return !!v && Object.keys(v).length > 0;
});

const playStatus = computed<SimulationStatus>(() => {
  if (isPlaying.value) return 'running';
  if (total.value > 0 && index.value >= total.value - 1) return 'finished';
  if (index.value > 0) return 'paused';
  return 'idle';
});

function clearTimer(): void {
  if (timer !== null) {
    clearInterval(timer);
    timer = null;
  }
}
function stop(): void {
  isPlaying.value = false;
  clearTimer();
}
function play(): void {
  if (total.value === 0) return;
  if (index.value >= total.value - 1) index.value = 0;
  isPlaying.value = true;
  clearTimer();
  timer = setInterval(() => {
    if (index.value >= total.value - 1) {
      stop();
      return;
    }
    index.value += 1;
  }, Math.max(75, 1200 / speed.value));
}
function pause(): void {
  stop();
}
function stepBack(): void {
  stop();
  if (index.value > 0) index.value -= 1;
}
function stepForward(): void {
  stop();
  if (index.value < total.value - 1) index.value += 1;
}
function reset(): void {
  stop();
  index.value = 0;
}
function setSpeed(value: number): void {
  speed.value = value;
}
function jumpTo(i: number): void {
  stop();
  index.value = i;
}
function toggleTrace(): void {
  showTrace.value = !showTrace.value;
}

onBeforeUnmount(() => clearTimer());
</script>

<template>
  <section class="visualizer-shell" data-testid="shared-visualizer-shell">
    <!-- HUD / header -->
    <header class="visualizer-shell__hud">
      <div class="visualizer-shell__titles">
        <p class="visualizer-shell__kicker">Visualizer · {{ title || 'Thuật toán' }}</p>
        <p v-if="subtitle" class="visualizer-shell__subtitle">{{ subtitle }}</p>
      </div>
      <div class="visualizer-shell__hud-actions">
        <span class="visualizer-shell__step" role="status" data-testid="viz-step-label">{{ stepLabel }}</span>
        <Button variant="ghost" size="sm" aria-label="Đóng visualizer" @click="emit('close')">
          <X :size="15" aria-hidden="true" />
          Đóng
        </Button>
      </div>
    </header>

    <!-- Stage + renderer adapter -->
    <div class="visualizer-shell__stage" data-testid="viz-stage">
      <ArrayBarsRenderer v-if="isArray && structure" :structure="structure" />
      <div v-else class="visualizer-shell__fallback" data-testid="viz-fallback">
        <p v-if="currentFrame" class="visualizer-shell__fallback-desc">{{ currentFrame.description }}</p>
        <p v-else class="visualizer-shell__fallback-desc">Bấm Chạy để xem các bước.</p>
      </div>
    </div>

    <!-- Playback controls (chung — ControlBar hiện có) -->
    <ControlBar
      :current-index="index"
      :total-frames="total"
      :status="playStatus"
      :speed="speed"
      @play="play"
      @pause="pause"
      @step-back="stepBack"
      @step-forward="stepForward"
      @reset="reset"
      @set-speed="setSpeed"
    />

    <!-- Progress / status -->
    <div class="visualizer-shell__progress">
      <ProgressBar :value="Math.round(progressPct)" show-label size="sm" />
    </div>

    <!-- Pseudocode + explanation -->
    <div class="visualizer-shell__panels">
      <div v-if="pseudocode.length > 0" class="visualizer-shell__pseudo" data-testid="viz-pseudo">
        <p class="visualizer-shell__panel-title">Mã giả</p>
        <ol class="pseudo-lines">
          <li
            v-for="(line, i) in pseudocode"
            :key="i"
            class="pseudo-line"
            :class="{ 'pseudo-line--active': currentFrame?.pseudocodeLine === i + 1 }"
          >
            <span class="pseudo-line__num">{{ i + 1 }}</span>
            <code>{{ line }}</code>
          </li>
        </ol>
      </div>
      <div class="visualizer-shell__explain" data-testid="viz-explain">
        <p class="visualizer-shell__panel-title">Giải thích</p>
        <p class="visualizer-shell__desc" data-testid="viz-description">
          {{ currentFrame?.description ?? '—' }}
        </p>
        <ul v-if="currentFrame?.annotations && currentFrame.annotations.length > 0" class="visualizer-shell__annotations">
          <li v-for="(a, i) in currentFrame.annotations" :key="i">{{ a }}</li>
        </ul>
        <dl v-if="hasVariables" class="visualizer-shell__vars">
          <div v-for="(v, k) in currentFrame!.variables" :key="k">
            <dt>{{ k }}</dt>
            <dd>{{ v }}</dd>
          </div>
        </dl>
      </div>
    </div>

    <!-- Trace drawer -->
    <div class="visualizer-shell__trace">
      <Button variant="ghost" size="sm" class="visualizer-shell__trace-toggle" @click="toggleTrace">
        {{ showTrace ? 'Ẩn bảng trace' : 'Xem bảng trace' }}
        <ChevronDown v-if="!showTrace" :size="14" aria-hidden="true" />
        <ChevronUp v-else :size="14" aria-hidden="true" />
      </Button>
      <VisualizerTraceDrawer
        v-if="showTrace"
        :frames="frames"
        :current-index="index"
        @select="jumpTo"
      />
    </div>
  </section>
</template>

<style scoped>
.visualizer-shell {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  padding: var(--space-md);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  background: var(--color-card);
}

/* — HUD — */
.visualizer-shell__hud {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-sm);
  flex-wrap: wrap;
}
.visualizer-shell__kicker {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  letter-spacing: 0.06em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
  margin: 0;
}
.visualizer-shell__subtitle {
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
  margin: 0;
}
.visualizer-shell__hud-actions {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}
.visualizer-shell__step {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 600;
  color: var(--color-on-primary);
  background: var(--color-primary);
  border-radius: var(--radius-full);
  padding: 3px 10px;
  font-variant-numeric: tabular-nums;
}

/* — Stage — */
.visualizer-shell__stage {
  min-height: 240px;
  border-radius: var(--radius-md);
  background: var(--color-canvas-ink);
  overflow: hidden;
  display: flex;
}
.visualizer-shell__stage > * { flex: 1; min-width: 0; }
.visualizer-shell__fallback {
  margin: auto;
  padding: var(--space-lg);
  text-align: center;
  color: var(--color-text-tertiary);
}
.visualizer-shell__fallback-desc { margin: 0; font-size: var(--text-sm); }

/* — Progress — */
.visualizer-shell__progress { padding: 0 2px; }

/* — Panels — */
.visualizer-shell__panels {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-md);
}
@media (max-width: 720px) {
  .visualizer-shell__panels { grid-template-columns: 1fr; }
}
.visualizer-shell__pseudo,
.visualizer-shell__explain {
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
  background: var(--color-muted);
}
.visualizer-shell__panel-title {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-tertiary);
  margin: 0 0 var(--space-xs);
}
.pseudo-lines {
  list-style: none;
  margin: 0;
  padding: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
}
.pseudo-line {
  display: flex;
  gap: var(--space-sm);
  padding: 2px 4px;
  border-radius: var(--radius-sm);
  color: var(--color-text-secondary);
}
.pseudo-line--active {
  background: var(--color-primary);
  color: var(--color-on-primary);
  font-weight: 600;
}
.pseudo-line__num {
  color: var(--color-text-quaternary);
  min-width: 18px;
  text-align: right;
  user-select: none;
}
.pseudo-line--active .pseudo-line__num { color: inherit; }
.visualizer-shell__desc {
  margin: 0;
  font-size: var(--text-sm);
  line-height: 1.6;
  color: var(--color-text-primary);
}
.visualizer-shell__annotations {
  margin: var(--space-xs) 0 0;
  padding-left: var(--space-md);
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
}
.visualizer-shell__vars {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
  margin: var(--space-sm) 0 0;
}
.visualizer-shell__vars div {
  display: inline-flex;
  gap: 4px;
  font-family: var(--font-mono);
  font-size: 11px;
  background: var(--color-surface);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-sm);
  padding: 2px 6px;
}
.visualizer-shell__vars dt { color: var(--color-text-tertiary); }
.visualizer-shell__vars dd { margin: 0; font-weight: 600; color: var(--color-primary); }

/* — Trace — */
.visualizer-shell__trace { display: flex; flex-direction: column; gap: var(--space-sm); }
.visualizer-shell__trace-toggle { align-self: flex-start; }

@media (prefers-reduced-motion: reduce) {
  .visualizer-shell__stage { transition: none; }
}
</style>
