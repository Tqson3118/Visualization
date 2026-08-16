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
  padding: 8px 14px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-card);
  min-height: 52px;
  display: flex;
  align-items: center;
}

.control-bar__row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-md);
  width: 100%;
  flex-wrap: nowrap;
}

.control-bar__nav {
  display: flex;
  gap: 6px;
  align-items: center;
  flex-shrink: 0;
}

.control-bar__indicator {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 600;
  color: var(--color-on-primary);
  padding: 4px 10px;
  background: var(--color-primary);
  border-radius: var(--radius-full);
  white-space: nowrap;
  font-variant-numeric: tabular-nums;
  min-width: 80px;
  text-align: center;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.control-bar__speed {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  flex-shrink: 0;
}

.control-bar__speed select {
  padding: 4px 8px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-foreground);
  cursor: pointer;
  font-weight: 500;
  font-family: var(--font-mono);
  font-size: 11px;
}

@media (max-width: 640px) {
  .control-bar {
    padding: 8px 10px;
    width: 100%;
    max-width: 100%;
  }
  .control-bar__row {
    flex-wrap: wrap;
    justify-content: space-between;
    gap: 8px;
    width: 100%;
  }
  .control-bar__nav {
    width: 100%;
    display: flex;
    justify-content: space-between;
    gap: 4px;
  }
  .control-bar__nav :deep(button) {
    padding-inline: 6px;
    font-size: 11px;
    height: 32px;
    flex: 1;
  }
  .control-bar__nav :deep(button span) {
    display: inline;
  }
  .control-bar__indicator {
    min-width: auto;
    font-size: 11px;
    padding: 3px 8px;
  }
  .control-bar__speed {
    font-size: 11px;
  }
}
</style>
