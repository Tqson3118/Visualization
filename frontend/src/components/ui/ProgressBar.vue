<script setup lang="ts">
// ProgressBar — component UI chung: thanh tiến độ (design tokens — SDD §8.1)
import { computed } from 'vue';

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
</script>

<template>
  <div class="ui-progress" :class="`ui-progress--${size}`">
    <div class="ui-progress__track" role="progressbar" :aria-valuenow="clamped" aria-valuemin="0" aria-valuemax="100">
      <div
        class="ui-progress__fill"
        :class="`ui-progress__fill--${variant}`"
        :style="{ width: `${clamped}%` }"
      />
    </div>
    <span v-if="showLabel" class="ui-progress__label">{{ clamped }}%</span>
  </div>
</template>

<style scoped>
.ui-progress {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  width: 100%;
}

.ui-progress__track {
  flex: 1;
  background: var(--color-muted);
  border-radius: var(--radius-full);
  overflow: hidden;
  height: 8px;
}

.ui-progress--sm .ui-progress__track { height: 6px; }

.ui-progress__fill {
  height: 100%;
  border-radius: var(--radius-full);
  background: var(--color-primary);
  transition: width 300ms ease;
}

.ui-progress__fill--success { background: var(--color-success); }
.ui-progress__fill--warning { background: var(--color-warning); }
.ui-progress__fill--danger { background: var(--color-destructive); }

.ui-progress__label {
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  min-width: 2.5rem;
  text-align: right;
}
</style>
