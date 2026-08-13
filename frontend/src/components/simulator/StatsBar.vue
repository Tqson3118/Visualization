<script setup lang="ts">
// StatsBar — bộ đếm thống kê mô phỏng (FR-3.9: comparisons/swaps/writes + steps)
// G-F2c: chip icon lucide + nhãn rõ ràng; giữ aria-label "Thống kê" (không ai phụ thuộc ngoài UI).
import { computed } from 'vue';

import { ArrowRightLeft, ListOrdered, PenLine, Repeat } from 'lucide-vue-next';

import { messages } from '@/i18n/vi';
import AnimatedNumber from '@/components/ui/AnimatedNumber.vue';

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
      <span class="stats__icon" :key="`cmp-${comparisons}`" aria-hidden="true">
        <ArrowRightLeft :size="13" />
      </span>
      So sánh: <AnimatedNumber :value="comparisons" :duration="350" immediate />
    </span>
    <span class="stats__item">
      <span class="stats__icon" :key="`swp-${swaps}`" aria-hidden="true">
        <Repeat :size="13" />
      </span>
      Hoán đổi: <AnimatedNumber :value="swaps" :duration="350" immediate />
    </span>
    <span class="stats__item">
      <span class="stats__icon" :key="`wrt-${writes}`" aria-hidden="true">
        <PenLine :size="13" />
      </span>
      Ghi: <AnimatedNumber :value="writes" :duration="350" immediate />
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
  gap: var(--space-xs);
  background: var(--color-card);
  border: 1px solid var(--color-border);
  padding: var(--space-xs) var(--space-sm);
  border-radius: var(--radius-full);
  white-space: nowrap;
  font-family: var(--font-mono);
  font-weight: 500;
  color: var(--color-foreground);
}

.stats__item svg { color: var(--color-primary); }

/* Icon micro-animation — nhảy nhẹ mỗi khi giá trị bộ đếm đổi (re-key) */
.stats__icon {
  display: inline-flex;
  animation: stats-pop 280ms var(--ease-out-expo);
}

@keyframes stats-pop {
  0% { transform: scale(0.55); opacity: 0.4; }
  100% { transform: scale(1); opacity: 1; }
}

.stats__item--step {
  background: var(--color-primary);
  border-color: transparent;
  color: var(--color-on-primary);
  font-weight: 600;
}

.stats__item--step svg { color: var(--color-on-primary); }

@media (prefers-reduced-motion: reduce) {
  .stats__icon { animation: none; }
}
</style>
