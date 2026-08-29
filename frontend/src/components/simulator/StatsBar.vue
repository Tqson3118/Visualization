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
    <span class="stats__title">Trạng thái:</span>
    <div class="stats__items">
      <span class="stats__item stats__item--step">
        <ListOrdered :size="12" aria-hidden="true" />
        {{ stepLabel }}
      </span>
      <span class="stats__item">
        <ArrowRightLeft :size="12" aria-hidden="true" />
        So sánh: {{ comparisons }}
      </span>
      <span class="stats__item">
        <Repeat :size="12" aria-hidden="true" />
        Hoán đổi: {{ swaps }}
      </span>
      <span class="stats__item">
        <PenLine :size="12" aria-hidden="true" />
        Ghi: {{ writes }}
      </span>
    </div>
  </div>
</template>

<style scoped>
.stats {
  position: relative;
  z-index: 2;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  min-height: 32px;
  padding: 4px 10px;
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  flex-wrap: wrap;
  max-width: 100%;
  overflow-x: auto;
  scrollbar-width: none;
}

.stats__title {
  color: var(--color-text-secondary);
  font-size: 11px;
  font-weight: 700;
  white-space: nowrap;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  flex-shrink: 0;
}

.stats__items {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: nowrap;
  overflow-x: auto;
  scrollbar-width: none;
}

.stats__item {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  background: var(--color-card);
  border: 1px solid var(--color-border);
  padding: 2px 8px;
  border-radius: var(--radius-full);
  white-space: nowrap;
  font-family: var(--font-mono);
  font-size: 11px;
  font-weight: 600;
  color: var(--color-foreground);
  flex-shrink: 0;
}

.stats__item svg { color: var(--color-primary); }

.stats__item--step {
  background: var(--color-primary);
  border-color: transparent;
  color: var(--color-on-primary);
}

.stats__item--step svg { color: currentColor; }

@media (max-width: 640px) {
  .stats {
    gap: 6px;
  }
  .stats__items {
    flex-wrap: wrap;
    gap: 4px;
  }
}
</style>
