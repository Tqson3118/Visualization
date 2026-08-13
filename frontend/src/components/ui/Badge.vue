<script setup lang="ts">
// Badge — wrapper giữ API cũ (G-F1b): render bằng shadcn-vue badgeVariants.
// Variants cũ: default/primary/success/warning/danger/muted (+ secondary cho call site ExerciseView).
import { computed } from 'vue';

import { cn } from '@/lib/utils';
import { badgeVariants } from './badge';

const props = withDefaults(
  defineProps<{
    variant?: 'default' | 'primary' | 'success' | 'warning' | 'danger' | 'muted' | 'secondary';
  }>(),
  {
    variant: 'default',
  },
);

const classes = computed(() => {
  const base = 'inline-flex items-center gap-1 whitespace-nowrap';
  switch (props.variant) {
    case 'primary':
      return cn(base, badgeVariants({ variant: 'default' }));
    case 'danger':
      return cn(base, badgeVariants({ variant: 'destructive' }));
    // H-E2: chip opaque (không phụ thuộc nền gradient chrome) — emerald-100/800 light ≈ 6.8:1,
    // emerald-950/300 dark ≈ 9.9:1 (text-xs cần ≥ 4.5:1). Trước: tint /15 mờ → 3.05:1 light / 2.87:1 dark.
    case 'success':
      return cn(
        base,
        'border-emerald-200 bg-emerald-100 text-emerald-800 dark:border-emerald-900 dark:bg-emerald-950 dark:text-emerald-300',
      );
    case 'warning':
      return cn(base, 'border-transparent bg-amber-500/15 text-amber-700 dark:text-amber-400');
    case 'muted':
      return cn(base, 'border-transparent bg-muted text-muted-foreground');
    case 'secondary':
      return cn(base, badgeVariants({ variant: 'secondary' }));
    case 'default':
    default:
      return cn(base, badgeVariants({ variant: 'secondary' }));
  }
});
</script>

<template>
  <span :class="classes"><slot /></span>
</template>
