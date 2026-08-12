<script setup lang="ts">
// Badge — component UI chung: nhãn trạng thái (design tokens — SDD §8.1)
import { computed } from 'vue';

const props = withDefaults(
  defineProps<{
    variant?: 'default' | 'primary' | 'success' | 'warning' | 'danger' | 'muted';
  }>(),
  {
    variant: 'default',
  },
);

const classes = computed(() => `ui-badge ui-badge--${props.variant}`);
</script>

<template>
  <span :class="classes"><slot /></span>
</template>

<style scoped>
.ui-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: var(--text-xs);
  font-weight: 700;
  padding: 2px 10px;
  border-radius: var(--radius-full);
  letter-spacing: 0.3px;
  white-space: nowrap;
}

.ui-badge--default { background: var(--color-muted); color: var(--color-foreground); }
.ui-badge--primary { background: color-mix(in srgb, var(--color-primary) 14%, transparent); color: var(--color-primary); }
.ui-badge--success { background: color-mix(in srgb, var(--color-success) 14%, transparent); color: var(--color-success); }
.ui-badge--warning { background: color-mix(in srgb, var(--color-warning) 16%, transparent); color: var(--color-warning); }
.ui-badge--danger { background: color-mix(in srgb, var(--color-destructive) 14%, transparent); color: var(--color-destructive); }
.ui-badge--muted { background: var(--color-muted); color: var(--color-text-muted); }
</style>
