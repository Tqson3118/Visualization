<template>
  <div class="control-bar card">
    <div class="control-bar__row">
      <div class="control-bar__nav">
        <button
          class="control-bar__btn"
          :disabled="currentIndex <= 0"
          :aria-label="messages.simulator.stepBack"
          @click="emit('step-back')"
        >
          <BaseIcon name="step-backward" :size="14" />
          {{ messages.simulator.stepBack }}
        </button>
        <button
          class="control-bar__btn"
          :disabled="currentIndex >= totalFrames - 1"
          :aria-label="messages.simulator.stepForward"
          @click="emit('step-forward')"
        >
          {{ messages.simulator.stepForward }}
          <BaseIcon name="step-forward" :size="14" />
        </button>
        <button
          v-if="status !== 'running'"
          class="control-bar__btn control-bar__btn--play"
          @click="emit('play')"
        >
          <BaseIcon name="play" :size="14" />
          {{ messages.simulator.play }}
        </button>
        <button
          v-else
          class="control-bar__btn control-bar__btn--play"
          @click="emit('pause')"
        >
          <BaseIcon name="pause" :size="14" />
          {{ messages.simulator.pause }}
        </button>
        <button
          class="control-bar__btn"
          :disabled="currentIndex <= 0"
          :aria-label="messages.simulator.reset"
          @click="emit('reset')"
        >
          <BaseIcon name="refresh" :size="14" />
          {{ messages.simulator.reset }}
        </button>
      </div>

      <span class="control-bar__indicator">{{ stepIndicator }}</span>

      <label class="control-bar__speed">
        <span>{{ messages.simulator.speed }}</span>
        <select :value="speed" :aria-label="messages.simulator.speed" @change="onSpeedChange">
          <option v-for="s in SPEED_OPTIONS" :key="s" :value="s">{{ s }}x</option>
        </select>
      </label>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

import { messages } from '@/i18n/vi';
import type { SimulationStatus } from '@/stores/simulation';
import BaseIcon from '@/components/ui/BaseIcon.vue';

// Bê từ source/VisualizationDSA1/frontend/src/components/VcrControls.vue (V1).
// ĐIỀU CHỈNH:
//   - Props/emit theo simulationStore (SDD §3.2): play/pause/step-back/step-forward/reset/set-speed
//     (bỏ "Exit VCR → Sandbox" — feature solid-sandbox đã cắt).
//   - Chuỗi cứng → i18n/vi.ts; màu → design tokens (card/btn).
//   - Thêm chọn tốc độ 0.25x–4x (SDD §3.5: interval = 1200/speed ms).

const props = defineProps<{
  currentIndex: number;
  totalFrames: number;
  status: SimulationStatus;
  speed: number;
}>();

const emit = defineEmits<{
  play: [];
  pause: [];
  'step-back': [];
  'step-forward': [];
  reset: [];
  'set-speed': [value: number];
}>();

const SPEED_OPTIONS = [0.25, 0.5, 1, 2, 4];

const stepIndicator = computed(() =>
  messages.simulator.stepOf
    .replace('{current}', String(Math.min(props.currentIndex + 1, props.totalFrames)))
    .replace('{total}', String(props.totalFrames)),
);

function onSpeedChange(event: Event): void {
  const value = Number((event.target as HTMLSelectElement).value);
  if (Number.isFinite(value)) {
    emit('set-speed', value);
  }
}
</script>

<style scoped>
.control-bar {
  padding: var(--space-md) var(--space-lg);
}

.control-bar__row {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.control-bar__nav {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
}

.control-bar__btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 14px;
  border-radius: var(--radius-md);
  font-size: var(--text-sm);
  font-weight: 600;
  cursor: pointer;
  transition: var(--transition-fast);
  border: 1px solid var(--color-border);
  background: var(--color-surface);
  color: var(--color-foreground);
}

.control-bar__btn:hover:not(:disabled) {
  background: var(--color-surface-hover);
}

.control-bar__btn:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.control-bar__btn--play {
  color: var(--color-on-primary);
  background: var(--color-primary);
  border-color: var(--color-primary);
}

.control-bar__btn--play:hover:not(:disabled) {
  opacity: 0.9;
}

.control-bar__indicator {
  font-size: var(--text-sm);
  color: var(--color-primary);
  font-weight: 600;
  padding: 4px 10px;
  background: var(--color-surface-hover);
  border-radius: var(--radius-md);
  white-space: nowrap;
}

.control-bar__speed {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
  margin-left: auto;
}

.control-bar__speed select {
  padding: 4px 8px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-foreground);
  cursor: pointer;
}
</style>
