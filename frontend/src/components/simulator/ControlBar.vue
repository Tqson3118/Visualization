<template>
  <div class="control-bar">
    <div class="control-bar__row">
      <!-- Cụm 1: Nút điều khiển Playback nhỏ gọn, icon-only với nút Play/Pause cố định độ rộng -->
      <div class="control-bar__nav">
        <!-- Đặt lại (Reset) -->
        <Button
          variant="ghost"
          size="sm"
          :disabled="currentIndex <= 0"
          :aria-label="messages.simulator.reset"
          title="Về bước đầu (Home)"
          class="control-bar__icon-btn"
          @click="emit('reset')"
        >
          <RotateCcw :size="15" aria-hidden="true" />
          <span class="sr-only">{{ messages.simulator.reset }}</span>
        </Button>

        <!-- Bước lùi (Step Back) -->
        <Button
          variant="ghost"
          size="sm"
          :disabled="currentIndex <= 0"
          :aria-label="messages.simulator.stepBack"
          title="Bước lùi 1 bước (←)"
          class="control-bar__icon-btn"
          @click="emit('step-back')"
        >
          <SkipBack :size="15" aria-hidden="true" />
          <span class="sr-only">{{ messages.simulator.stepBack }}</span>
        </Button>

        <!-- NÚT CHÍNH: Phát / Tạm dừng (Play / Pause) cố định chiều rộng để không bị giật layout -->
        <Button
          v-if="status !== 'running'"
          variant="primary"
          size="sm"
          :aria-label="messages.simulator.play"
          title="Chạy mô phỏng (Space)"
          class="control-bar__play"
          @click="emit('play')"
        >
          <Play :size="14" class="fill-current shrink-0" aria-hidden="true" />
          <span class="control-bar__btn-text">{{ messages.simulator.play }}</span>
        </Button>
        <Button
          v-else
          variant="primary"
          size="sm"
          :aria-label="messages.simulator.pause"
          title="Tạm dừng mô phỏng (Space)"
          class="control-bar__play control-bar__play--active"
          @click="emit('pause')"
        >
          <Pause :size="14" class="fill-current shrink-0" aria-hidden="true" />
          <span class="control-bar__btn-text">{{ messages.simulator.pause }}</span>
        </Button>

        <!-- Bước tới (Step Forward) -->
        <Button
          variant="ghost"
          size="sm"
          :disabled="currentIndex >= totalFrames - 1"
          :aria-label="messages.simulator.stepForward"
          title="Bước tới 1 bước (→)"
          class="control-bar__icon-btn"
          @click="emit('step-forward')"
        >
          <SkipForward :size="15" aria-hidden="true" />
          <span class="sr-only">{{ messages.simulator.stepForward }}</span>
        </Button>
      </div>

      <!-- Cụm 2: Thanh trượt tiến độ (Scrubber Timeline Slider) & Bộ đếm số bước -->
      <div class="control-bar__timeline">
        <input
          type="range"
          min="0"
          :max="Math.max(0, totalFrames - 1)"
          :value="currentIndex"
          :disabled="totalFrames <= 1"
          class="control-bar__slider"
          aria-label="Thanh trượt bước chạy"
          title="Kéo để tua nhanh bước chạy"
          @input="onSliderChange"
        />
        <span class="control-bar__indicator" role="status">{{ stepIndicator }}</span>
      </div>

      <!-- Cụm 3: Điều khiển tốc độ (Speed) -->
      <div class="control-bar__speed" title="Tốc độ phát mô phỏng ([ / ])">
        <Gauge :size="14" class="text-vdsa-purple shrink-0" aria-hidden="true" />
        <select :value="speed" :aria-label="messages.simulator.speed" class="control-bar__speed-select" @change="onSpeedChange">
          <option v-for="s in SPEED_OPTIONS" :key="s" :value="s">{{ s }}x</option>
        </select>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

import { Gauge, Pause, Play, RotateCcw, SkipBack, SkipForward } from 'lucide-vue-next';

import { messages } from '@/i18n/vi';
import type { SimulationStatus } from '@/stores/simulation';
import Button from '@/components/ui/Button.vue';

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
  'jump-to': [index: number];
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

function onSliderChange(event: Event): void {
  const target = event.target as HTMLInputElement;
  const val = Number(target.value);
  if (!Number.isNaN(val)) {
    emit('jump-to', val);
  }
}
</script>

<style scoped>
.control-bar {
  /* Simulator Play/Step buttons — giữ Emerald "go" color,
     không dùng Purple brand (UX convention: xanh lá = play/go) */
  --color-primary: #10b981;
  padding: 8px 12px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-card);
  box-shadow: 0 4px 16px rgba(0, 0, 0, 0.25);
}

.control-bar__row {
  display: flex;
  align-items: center;
  gap: 12px;
  flex-wrap: nowrap;
  width: 100%;
}

.control-bar__nav {
  display: flex;
  gap: 4px;
  align-items: center;
  shrink: 0;
}

.control-bar__icon-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  padding: 0;
  border-radius: var(--radius-md);
  color: var(--color-text-secondary, #94a3b8);
  transition: all 150ms ease;
}

.control-bar__icon-btn:hover:not(:disabled) {
  color: #fff;
  background: rgba(255, 255, 255, 0.08);
}

.control-bar__play {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  min-width: 88px;
  height: 32px;
  padding: 0 10px;
  background: var(--color-primary, #10b981);
  color: #fff;
  border-radius: var(--radius-md);
  font-size: 12px;
  font-weight: 700;
  box-shadow: 0 2px 8px color-mix(in srgb, var(--color-primary, #10b981) 35%, transparent);
  transition: all 150ms cubic-bezier(0.16, 1, 0.3, 1);
  shrink: 0;
}

.control-bar__play:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 12px color-mix(in srgb, var(--color-primary, #10b981) 50%, transparent);
}

.control-bar__play--active {
  background: #f59e0b;
  color: #000;
  box-shadow: 0 2px 8px rgba(245, 158, 11, 0.4);
}

.control-bar__btn-text {
  font-size: 12px;
  font-weight: 700;
  white-space: nowrap;
}

/* Timeline slider & Step Indicator */
.control-bar__timeline {
  flex: 1;
  min-width: 60px;
  display: flex;
  align-items: center;
  gap: 10px;
}

.control-bar__slider {
  flex: 1;
  min-width: 40px;
  height: 5px;
  -webkit-appearance: none;
  appearance: none;
  background: rgba(255, 255, 255, 0.12);
  border-radius: 9999px;
  outline: none;
  cursor: pointer;
  transition: background 150ms ease;
}

.control-bar__slider:hover {
  background: rgba(255, 255, 255, 0.22);
}

.control-bar__slider::-webkit-slider-thumb {
  -webkit-appearance: none;
  appearance: none;
  width: 13px;
  height: 13px;
  border-radius: 50%;
  background: var(--color-primary, #10b981);
  border: 2px solid #fff;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.5);
  cursor: pointer;
  transition: transform 120ms ease;
}

.control-bar__slider::-webkit-slider-thumb:hover {
  transform: scale(1.3);
}

.control-bar__slider::-moz-range-thumb {
  width: 13px;
  height: 13px;
  border-radius: 50%;
  background: var(--color-primary, #10b981);
  border: 2px solid #fff;
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.5);
  cursor: pointer;
}

.control-bar__indicator {
  font-family: var(--font-mono, monospace);
  font-size: 11px;
  font-weight: 600;
  color: var(--color-text-secondary, #94a3b8);
  white-space: nowrap;
  letter-spacing: 0.02em;
  shrink: 0;
}

/* Speed selector */
.control-bar__speed {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 11px;
  color: var(--color-text-muted, #64748b);
  shrink: 0;
}

.control-bar__speed-select {
  padding: 3px 6px;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-surface, #1e293b);
  color: #f1f5f9;
  font-size: 11px;
  cursor: pointer;
  font-weight: 600;
  transition: border-color 150ms ease;
}

.control-bar__speed-select:focus {
  border-color: var(--color-primary, #10b981);
  outline: none;
}

.sr-only {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border-width: 0;
}
</style>
