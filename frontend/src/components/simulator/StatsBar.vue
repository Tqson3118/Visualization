<script setup lang="ts">
// StatsBar — bộ đếm thống kê mô phỏng (FR-3.9: comparisons/swaps/writes + steps)
import { computed } from 'vue';

import { messages } from '@/i18n/vi';

const props = withDefaults(
  defineProps<{
    comparisons?: number;
    swaps?: number;
    writes?: number;
    step?: number;
    totalSteps?: number;
  }>(),
  {
    comparisons: 0,
    swaps: 0,
    writes: 0,
    step: 0,
    totalSteps: 0,
  },
);

const stepLabel = computed(() =>
  messages.simulator.stepOf
    .replace('{current}', String(props.totalSteps === 0 ? 0 : props.step + 1))
    .replace('{total}', String(props.totalSteps)),
);
</script>

<template>
  <div class="stats" aria-label="Thống kê">
    <span class="stats__item">{{ stepLabel }}</span>
    <span class="stats__item">So sánh: {{ comparisons }}</span>
    <span class="stats__item">Hoán đổi: {{ swaps }}</span>
    <span class="stats__item">Ghi: {{ writes }}</span>
  </div>
</template>

<style scoped>
.stats {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
  font-size: var(--text-xs);
  color: var(--color-text-muted);
}

.stats__item {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  padding: 3px 10px;
  border-radius: var(--radius-full);
  white-space: nowrap;
}
</style>
