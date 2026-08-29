<script setup lang="ts">
import { computed } from 'vue';
import { cn } from '@/lib/utils';

const props = withDefaults(
  defineProps<{
    /** Giá trị 0..100 */
    value: number;
    showLabel?: boolean;
    variant?: 'default' | 'success' | 'warning' | 'danger';
    size?: 'sm' | 'md';
  }>(),
  {
    showLabel: false,
    variant: 'default',
    size: 'md',
  },
);

const clamped = computed(() => Math.max(0, Math.min(100, Number.isFinite(props.value) ? props.value : 0)));

const heightClass = computed(() => (props.size === 'sm' ? 'h-1.5' : 'h-2'));

const barColorClass = computed(() => {
  switch (props.variant) {
    case 'success':
      return 'bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.4)]';
    case 'warning':
      return 'bg-amber-500 shadow-[0_0_8px_rgba(245,158,11,0.4)]';
    case 'danger':
      return 'bg-rose-500 shadow-[0_0_8px_rgba(244,63,94,0.4)]';
    default:
      return 'bg-purple-500 shadow-[0_0_8px_rgba(168,85,247,0.4)]';
  }
});
</script>

<template>
  <div class="flex w-full items-center gap-2.5">
    <div
      class="flex-1 w-full overflow-hidden rounded-full bg-slate-800/80 border border-slate-700/50"
      :class="heightClass"
      role="progressbar"
      :aria-valuenow="clamped"
      aria-valuemin="0"
      aria-valuemax="100"
    >
      <div
        class="h-full rounded-full transition-all duration-300 ease-out"
        :class="barColorClass"
        :style="{ width: `${clamped}%` }"
      />
    </div>
    <span v-if="showLabel" class="min-w-9 text-right text-xs font-mono font-medium text-slate-400">
      {{ clamped }}%
    </span>
  </div>
</template>

