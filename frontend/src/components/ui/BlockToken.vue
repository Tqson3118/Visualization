<script setup lang="ts">
// BlockToken — block-token chung (Data Bench, DESIGN-IDENTITY §1.5 + DESIGN.md §6):
// panel tối canvas-ink (LUÔN tối bất kể theme) + block màu + label/index mono —
// "dữ liệu luôn được đánh số". Dùng cho: XP hero, streak, gems, rank, bộ đếm.
// Tones: default (data-core) · resolved (hoàn thành/streak) · warning (rank top).
// UI-PREMIUM 0B: thêm prop `glow` (box-shadow theo status), `pulse` (nhấp nháy khi
// comparing — signature "block thở theo bước"), size `sm/md/lg` (32/44/56 px).
import { computed } from 'vue';

const props = withDefaults(
  defineProps<{
    label?: string;
    value: string | number;
    index?: string;
    tone?: 'default' | 'resolved' | 'warning';
    /** size="sm" — bản compact cho stat phụ/cột rank; "lg" — hero stat */
    size?: 'sm' | 'md' | 'lg';
    /** glow theo trạng thái thuật toán (data-core/resolved/conflict) */
    glow?: boolean;
    /** nhấp nháy chậm khi đang compare */
    pulse?: boolean;
  }>(),
  {
    label: '',
    index: '',
    tone: 'default',
    size: 'md',
    glow: false,
    pulse: false,
  },
);

const blockClass = computed(() => {
  const base = 'rounded-sm bg-data-core';
  if (props.tone === 'resolved') return base.replace('bg-data-core', 'bg-resolved');
  if (props.tone === 'warning') return base.replace('bg-data-core', 'bg-warning');
  return base;
});

const valueClass = computed(() => {
  const base =
    props.size === 'sm'
      ? 'font-mono text-sm font-medium'
      : props.size === 'lg'
        ? 'text-3xl font-semibold tracking-[-0.025em]'
        : 'text-2xl font-semibold tracking-[-0.015em]';
  if (props.tone === 'resolved') return `${base} text-resolved`;
  if (props.tone === 'warning') return `${base} text-warning`;
  return `${base} text-index-muted`;
});

const sizeClass = computed(() => {
  if (props.size === 'sm') return 'min-w-16 px-3 py-2';
  if (props.size === 'lg') return 'min-w-36 px-5 py-4';
  return 'min-w-28 px-4 py-3';
});
</script>

<template>
  <div
    class="ui-blocktoken flex flex-col gap-1 rounded-lg border border-data-core/20 bg-canvas-ink"
    :class="[
      sizeClass,
      pulse ? 'ui-blocktoken--pulse' : '',
      glow ? 'ui-blocktoken--glow' : '',
      glow && tone === 'resolved' ? 'ui-blocktoken--glow-resolved' : '',
      glow && tone === 'warning' ? 'ui-blocktoken--glow-warning' : '',
    ]"
  >
    <span v-if="label || index" class="flex items-center gap-1.5">
      <span aria-hidden="true" class="h-2 w-2 rounded-sm" :class="blockClass" />
      <span class="font-mono text-xs text-index-muted">
        {{ label }}<template v-if="label && index"> · </template>{{ index }}
      </span>
    </span>
    <span class="font-variant-numeric tabular-nums" :class="valueClass">{{ value }}</span>
  </div>
</template>

<style scoped>
.ui-blocktoken {
  transition:
    box-shadow var(--duration-normal) var(--ease-out-expo),
    transform var(--duration-normal) var(--ease-out-expo);
}

/* Glow theo trạng thái — nguồn palette 6 màu (DESIGN-IDENTITY §1.2) */
.ui-blocktoken--glow {
  box-shadow: var(--glow-data-core);
}

.ui-blocktoken--glow-resolved {
  box-shadow: var(--glow-resolved);
}

.ui-blocktoken--glow-warning {
  box-shadow: var(--glow-conflict);
}

/* Pulse — "block đang compare", nhấp nháy chậm (signature §1.5) */
.ui-blocktoken--pulse {
  animation: ui-blocktoken-pulse 1.6s var(--ease-in-out) infinite;
}

@keyframes ui-blocktoken-pulse {
  0%, 100% { box-shadow: 0 0 0 color-mix(in srgb, var(--color-data-core) 0%, transparent); }
  50% { box-shadow: 0 0 16px color-mix(in srgb, var(--color-data-core) 35%, transparent); }
}

@media (prefers-reduced-motion: reduce) {
  .ui-blocktoken--pulse {
    animation: none;
  }
}
</style>
