<script setup lang="ts">
// Button — component UI chung (design tokens — SDD §8.1)
// Variants: primary / secondary / ghost / danger; hỗ trợ trạng thái loading.
import { computed } from 'vue';

import { messages } from '@/i18n/vi';

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

const classes = computed(() => [
  'ui-btn',
  `ui-btn--${props.variant}`,
  `ui-btn--${props.size}`,
  { 'ui-btn--block': props.block, 'ui-btn--loading': props.loading },
]);
</script>

<template>
  <button :class="classes" :type="type" :disabled="disabled || loading" :aria-busy="loading">
    <span v-if="loading" class="ui-btn__spinner" aria-hidden="true" />
    <slot>{{ messages.common.confirm }}</slot>
  </button>
</template>

<style scoped>
.ui-btn {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-sm);
  border: none;
  border-radius: var(--radius-md);
  font-weight: 600;
  font-family: inherit;
  cursor: pointer;
  transition: var(--transition-fast);
  text-decoration: none;
  white-space: nowrap;
}

.ui-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.ui-btn--sm { padding: 0.4rem 0.8rem; font-size: var(--text-sm); }
.ui-btn--md { padding: 0.6rem 1.25rem; font-size: var(--text-base); }
.ui-btn--lg { padding: 0.8rem 1.75rem; font-size: var(--text-md); }

.ui-btn--primary { background: var(--color-primary); color: var(--color-on-primary); }
.ui-btn--primary:hover:not(:disabled) { opacity: 0.9; transform: translateY(-1px); }

.ui-btn--secondary {
  background: transparent;
  color: var(--color-primary);
  border: 2px solid var(--color-primary);
}
.ui-btn--secondary:hover:not(:disabled) { background: var(--color-surface-hover); }

.ui-btn--ghost {
  background: transparent;
  color: var(--color-foreground);
  border: 1px solid var(--color-border);
}
.ui-btn--ghost:hover:not(:disabled) { background: var(--color-surface-hover); }

.ui-btn--danger { background: var(--color-destructive); color: var(--color-on-primary); }
.ui-btn--danger:hover:not(:disabled) { opacity: 0.9; }

.ui-btn--block { width: 100%; }

.ui-btn__spinner {
  width: 1em;
  height: 1em;
  border: 2px solid currentColor;
  border-top-color: transparent;
  border-radius: 50%;
  animation: ui-btn-spin 0.7s linear infinite;
}

@keyframes ui-btn-spin {
  to { transform: rotate(360deg); }
}
</style>
