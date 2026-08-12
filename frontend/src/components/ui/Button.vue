<script setup lang="ts">
// Button — wrapper giữ API cũ (G-F1b): render bằng shadcn-vue buttonVariants.
// Variants cũ: primary/secondary/ghost/danger → shadcn default/outline/ghost/destructive.
import { computed } from 'vue';

import { messages } from '@/i18n/vi';
import { cn } from '@/lib/utils';
import { buttonVariants } from './button';

const props = withDefaults(
  defineProps<{
    variant?: 'primary' | 'secondary' | 'ghost' | 'danger';
    size?: 'sm' | 'md' | 'lg';
    loading?: boolean;
    disabled?: boolean;
    type?: 'button' | 'submit';
    block?: boolean;
  }>(),
  {
    variant: 'primary',
    size: 'md',
    loading: false,
    disabled: false,
    type: 'button',
    block: false,
  },
);

const variantMap: Record<string, NonNullable<Parameters<typeof buttonVariants>[0]>['variant']> = {
  primary: 'default',
  secondary: 'outline',
  ghost: 'ghost',
  danger: 'destructive',
};

const sizeMap: Record<string, NonNullable<Parameters<typeof buttonVariants>[0]>['size']> = {
  sm: 'sm',
  md: 'default',
  lg: 'lg',
};

const classes = computed(() =>
  cn(
    buttonVariants({ variant: variantMap[props.variant], size: sizeMap[props.size] }),
    'cursor-pointer select-none',
    { 'w-full': props.block },
  ),
);
</script>

<template>
  <button :class="classes" :type="type" :disabled="disabled || loading" :aria-busy="loading">
    <span
      v-if="loading"
      class="h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent"
      aria-hidden="true"
    />
    <slot>{{ messages.common.confirm }}</slot>
  </button>
</template>
