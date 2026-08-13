<script setup lang="ts">
// Button — wrapper giữ API cũ (G-F1b): render bằng shadcn-vue buttonVariants.
// Variants cũ: primary/secondary/ghost/danger → shadcn default/outline/ghost/destructive.
// UI-PREMIUM 0B: press state (scale 0.97), focus-visible glow ring, loading spinner
// + opacity transition, disabled desaturation — KHÔNG đổi API (backward compatible).
import { computed } from 'vue';

import { messages } from '@/i18n/vi';
import { cn } from '@/lib/utils';
import { buttonVariants } from './button';

const props = withDefaults(
  defineProps<{
    variant?: 'primary' | 'secondary' | 'ghost' | 'danger';
    // 'icon'/'icon-sm'/'icon-lg' map thẳng buttonVariants (DESIGN.md §4.1) — bổ sung cho
    // nút icon (SimulatorView favorite/share), backward compatible với size cũ.
    size?: 'sm' | 'md' | 'lg' | 'icon' | 'icon-sm' | 'icon-lg';
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
  icon: 'icon',
  'icon-sm': 'icon-sm',
  'icon-lg': 'icon-lg',
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
  <button
    :class="classes"
    :type="type"
    :disabled="disabled || loading"
    :aria-busy="loading"
    class="ui-btn"
  >
    <span
      v-if="loading"
      class="h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent"
      aria-hidden="true"
    />
    <span v-else class="ui-btn-content inline-flex items-center gap-2">
      <slot>{{ messages.common.confirm }}</slot>
    </span>
  </button>
</template>

<style scoped>
/* UI-PREMIUM 0B — micro-feedback:
   press scale 60ms, focus glow ring, loading fade, disabled desaturate */
.ui-btn {
  position: relative;
  overflow: hidden;
  transition:
    transform 60ms var(--ease-out-quad),
    box-shadow var(--duration-fast) var(--ease-out-quad),
    opacity var(--duration-fast) var(--ease-out-quad),
    background-color var(--duration-fast) var(--ease-out-quad),
    border-color var(--duration-fast) var(--ease-out-quad),
    color var(--duration-fast) var(--ease-out-quad);
}

.ui-btn:active:not(:disabled) {
  transform: scale(0.97);
}

.ui-btn:focus-visible {
  box-shadow: var(--glow-primary);
  outline: 2px solid var(--color-ring);
  outline-offset: 2px;
}

.ui-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
  filter: saturate(0.6);
}

.ui-btn-content {
  transition: opacity var(--duration-fast) var(--ease-out-quad);
}

.ui-btn[aria-busy='true'] .ui-btn-content {
  opacity: 0;
}
</style>
