<script setup lang="ts">
// ProgressBar — wrapper giữ API cũ (G-F1b): value/showLabel/variant/size.
// Render bằng shadcn-vue Progress; màu variant qua selector [&_[data-value]] (reka-ui indicator).
import { computed } from 'vue';

import { cn } from '@/lib/utils';
import Progress from './progress/Progress.vue';

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

const clamped = computed(() => Math.max(0, Math.min(100, props.value)));

const trackClass = computed(() => {
  const height = props.size === 'sm' ? 'h-1.5' : 'h-2';
  switch (props.variant) {
    case 'success':
      return cn(height, '[&_[data-value]]:bg-emerald-500');
    case 'warning':
      return cn(height, '[&_[data-value]]:bg-amber-500');
    case 'danger':
      return cn(height, '[&_[data-value]]:bg-destructive');
    default:
      return height;
  }
});
</script>

<template>
  <div class="flex w-full items-center gap-2">
    <Progress :model-value="clamped" class="flex-1" :class="trackClass" />
    <span v-if="showLabel" class="min-w-10 text-right text-xs text-muted-foreground">
      {{ clamped }}%
    </span>
  </div>
</template>
