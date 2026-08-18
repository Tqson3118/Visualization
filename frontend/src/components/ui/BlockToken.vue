<script setup lang="ts">
// BlockToken — block-token chung (Data Bench, DESIGN-IDENTITY §1.5 + DESIGN.md §6):
// panel tối canvas-ink (LUÔN tối bất kể theme) + block màu + label/index mono —
// "dữ liệu luôn được đánh số". Dùng cho: XP hero, streak, gems, rank, bộ đếm.
// Tones: default (data-core) · resolved (hoàn thành/streak) · warning (rank top).
import { computed } from 'vue';

const props = withDefaults(
  defineProps<{
    label?: string;
    value: string | number;
    index?: string;
    tone?: 'default' | 'resolved' | 'warning';
    /** size="sm" — bản compact cho stat phụ/cột rank */
    size?: 'sm' | 'md';
  }>(),
  {
    label: '',
    index: '',
    tone: 'default',
    size: 'md',
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
      ? 'font-mono text-sm font-semibold'
      : 'text-2xl font-bold tracking-[-0.015em]';
  if (props.tone === 'resolved') return `${base} text-emerald-400`;
  if (props.tone === 'warning') return `${base} text-amber-400`;
  return `${base} text-slate-100`;
});
</script>

<template>
  <div
    class="flex flex-col gap-1 rounded-lg border border-slate-700/50 bg-canvas-ink px-4 py-3"
    :class="size === 'sm' ? 'min-w-16' : 'min-w-28'"
  >
    <span v-if="label || index" class="flex items-center gap-1.5">
      <span aria-hidden="true" class="h-2 w-2 rounded-sm" :class="blockClass" />
      <span class="font-mono text-xs text-slate-400">
        {{ label }}<template v-if="label && index"> · </template>{{ index }}
      </span>
    </span>
    <span class="font-variant-numeric tabular-nums" :class="valueClass">{{ value }}</span>
  </div>
</template>
