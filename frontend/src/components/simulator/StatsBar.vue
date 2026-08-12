<script setup lang="ts">
// StatsBar — bộ đếm thống kê mô phỏng (FR-3.9: comparisons/swaps/writes + steps)
// G-F2c: chip icon lucide + nhãn rõ ràng; giữ aria-label "Thống kê" (không ai phụ thuộc ngoài UI).
import { computed } from 'vue';

import { ArrowRightLeft, ListOrdered, PenLine, Repeat } from 'lucide-vue-next';

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
    <span class="stats__item stats__item--step">
      <ListOrdered :size="13" aria-hidden="true" />
      {{ stepLabel }}
    </span>
    <span class="stats__item">
      <ArrowRightLeft :size="13" aria-hidden="true" />
      So sánh: {{ comparisons }}
    </span>
    <span class="stats__item">
      <Repeat :size="13" aria-hidden="true" />
      Hoán đổi: {{ swaps }}
    </span>
    <span class="stats__item">
      <PenLine :size="13" aria-hidden="true" />
      Ghi: {{ writes }}
    </span>
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
  display: inline-flex;
  align-items: center;
  gap: 5px;
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  padding: 3px 10px;
  border-radius: var(--radius-full);
  white-space: nowrap;
  font-weight: 600;
  color: var(--color-foreground);
}

.stats__item svg { color: var(--color-primary); }

.stats__item--step {
  background-image: var(--gradient-mint);
  border-color: transparent;
  color: var(--color-on-primary);
}

.stats__item--step svg { color: var(--color-on-primary); }
</style>
