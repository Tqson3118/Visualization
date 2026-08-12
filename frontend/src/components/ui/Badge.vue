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
    case 'success':
      return cn(base, 'border-transparent bg-emerald-500/15 text-emerald-700 dark:text-emerald-400');
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
