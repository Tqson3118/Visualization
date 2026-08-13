<template>
  <div class="control-bar">
    <div class="control-bar__row">
      <div class="control-bar__nav">
        <Button
          variant="ghost"
          size="sm"
          :disabled="currentIndex <= 0"
          :aria-label="messages.simulator.stepBack"
          @click="emit('step-back')"
        >
          <SkipBack :size="15" aria-hidden="true" />
          <span>{{ messages.simulator.stepBack }}</span>
        </Button>
        <Button
          variant="ghost"
          size="sm"
          :disabled="currentIndex >= totalFrames - 1"
          :aria-label="messages.simulator.stepForward"
          @click="emit('step-forward')"
        >
          <span>{{ messages.simulator.stepForward }}</span>
          <SkipForward :size="15" aria-hidden="true" />
        </Button>
        <Button
          v-if="status !== 'running'"
          variant="primary"
          size="sm"
          :aria-label="messages.simulator.play"
          class="control-bar__play"
          @click="emit('play')"
        >
          <Play :size="15" aria-hidden="true" />
          <span>{{ messages.simulator.play }}</span>
        </Button>
        <Button
          v-else
          variant="primary"
          size="sm"
          :aria-label="messages.simulator.pause"
          class="control-bar__play"
          @click="emit('pause')"
        >
          <Pause :size="15" aria-hidden="true" />
          <span>{{ messages.simulator.pause }}</span>
        </Button>
        <Button
          variant="ghost"
          size="sm"
          :disabled="currentIndex <= 0"
          :aria-label="messages.simulator.reset"
          @click="emit('reset')"
        >
          <RotateCcw :size="15" aria-hidden="true" />
          <span>{{ messages.simulator.reset }}</span>
        </Button>
      </div>

      <span class="control-bar__indicator" role="status">{{ stepIndicator }}</span>

      <label class="control-bar__speed">
        <Gauge :size="15" aria-hidden="true" />
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

import { Gauge, Pause, Play, RotateCcw, SkipBack, SkipForward } from 'lucide-vue-next';

import { messages } from '@/i18n/vi';
import type { SimulationStatus } from '@/stores/simulation';
import Button from '@/components/ui/Button.vue';

// Bê từ source/VisualizationDSA1/frontend/src/components/VcrControls.vue (V1).
// ĐIỀU CHỈNH:
//   - Props/emit theo simulationStore (SDD §3.2): play/pause/step-back/step-forward/reset/set-speed
//     (bỏ "Exit VCR → Sandbox" — feature solid-sandbox đã cắt).
//   - Chuỗi cứng → i18n/vi.ts; màu → design tokens (card/btn).
//   - Thêm chọn tốc độ 0.25x–4x (SDD §3.5: interval = 1200/speed ms).
//   - G-F2c: nút dùng shadcn Button + icon lucide; GIỮ nguyên accessible name
//     (Bước tới/Bước lùi/Chạy/Tạm dừng/Đặt lại) + .control-bar__indicator — e2e phụ thuộc.

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
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-card);
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
  align-items: center;
}

.control-bar__indicator {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-on-primary);
  padding: var(--space-xs) var(--space-sm);
  background: var(--color-primary);
  border-radius: var(--radius-full);
  white-space: nowrap;
}

.control-bar__speed {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
  margin-left: auto;
}

.control-bar__speed select {
  padding: var(--space-xs) var(--space-sm);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-foreground);
  cursor: pointer;
  font-weight: 500;
}
</style>
