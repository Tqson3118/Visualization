<script setup lang="ts">
// StatCard — KPI card 2 cấp (DESIGN.md §6: tối đa 1 hero/màn):
//   level="hero"   → Card level-2 (card-raised/border-subtle) chứa panel tối
//                    canvas-ink + value data-core + label mono index-muted
//                    (ClassReportView hero-stat; #panel slot cho nội dung
//                    thêm vào panel tối, VD block row của AdminStatsView).
//   level="default"→ Card level-1 (card/border): icon nhỏ + label tertiary,
//                    value text-2xl 600 tabular-nums + diff mono.
// loading=true → Skeleton thay toàn bộ card.
import { type Component } from 'vue';

import { Card } from '@/components/ui/card';
import Skeleton from '@/components/ui/Skeleton.vue';

  withDefaults(
  defineProps<{
    label: string;
    value: string | number;
    /** Index mono trong panel hero (mặc định fallback = label) */
    index?: string;
    /** Delta/đơn vị mono dưới value (chỉ level default) */
    diff?: string;
    /** Icon lucide nhỏ cạnh label (hero: hiển thị head khi có icon) */
    icon?: Component | null;
    loading?: boolean;
    level?: 'hero' | 'default';
  }>(),
  {
    index: '',
    diff: '',
    icon: null,
    loading: false,
    level: 'default',
  },
);
</script>

<template>
  <Skeleton v-if="loading" v-bind="$attrs" height="108px" aria-hidden="true" />

  <!-- v-bind="$attrs": cho phép view truyền class grid (VD admin-stats__kpi-hero) qua fallthrough -->
  <Card
    v-else
    v-bind="$attrs"
    class="stat-card"
    :class="level === 'hero' ? 'stat-card--hero' : 'stat-card--default'"
  >
    <!-- Hero: block-token tối — signature Data Bench -->
    <template v-if="level === 'hero'">
      <div v-if="icon" class="stat-card__head">
        <span class="stat-card__icon" aria-hidden="true">
          <component :is="icon" :size="18" />
        </span>
        <span class="stat-card__label">{{ label }}</span>
      </div>
      <div class="stat-card__panel">
        <slot name="panel" />
        <p class="stat-card__value">{{ value }}</p>
        <p v-if="index || label" class="stat-card__index">{{ index || label }}</p>
      </div>
    </template>

    <!-- Default: level-1, không icon tròn, không shadow (DESIGN §6) -->
    <template v-else>
      <div v-if="icon || label" class="stat-card__head">
        <span v-if="icon" class="stat-card__icon" aria-hidden="true">
          <component :is="icon" :size="18" />
        </span>
        <span class="stat-card__label">{{ label }}</span>
      </div>
      <div class="stat-card__body">
        <p class="stat-card__value">{{ value }}</p>
        <p v-if="diff" class="stat-card__diff">{{ diff }}</p>
      </div>
    </template>
  </Card>
</template>

<style scoped>
.stat-card {
  display: flex;
  flex-direction: column;
}

/* ── Hero: Card level-2 (DESIGN §6) ── */
.stat-card--hero {
  background: var(--card-raised);
  border-color: var(--border-subtle);
  padding: var(--space-md);
}

/* Head hero (icon + label) — giống head default: flex row, gap sm, padding md md 0 */
.stat-card--hero .stat-card__head {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-md) var(--space-md) 0;
}

/* Block-token tối — signature Data Bench (enter) */
.stat-card__panel {
  background: var(--canvas-ink);
  border: 1px solid rgba(66, 85, 255, 0.25);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  flex: 1;
  justify-content: center;
  opacity: 0;
  transform: translateY(6px);
  animation: stat-card-enter 280ms cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

.stat-card__value {
  margin: 0;
  font-size: var(--text-2xl);
  font-weight: 600;
  letter-spacing: -0.015em;
  line-height: 1.2;
  color: var(--data-core);
  font-variant-numeric: tabular-nums;
}

.stat-card__label {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--index-muted);
  letter-spacing: 0.08em;
}

/* Index mono trong panel hero — giống .stat-card__label cũ */
.stat-card__index {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--index-muted);
  letter-spacing: 0.08em;
}

@keyframes stat-card-enter {
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .stat-card__panel {
    animation: none;
    opacity: 1;
    transform: none;
  }
}

/* ── Default: Card level-1 ── */
.stat-card--default .stat-card__head {
  display: flex;
  flex-direction: row;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-md) var(--space-md) 0;
}

/* Icon chung 2 level (hero head giống head default) */
.stat-card__icon {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-md);
  background: var(--muted);
  color: var(--foreground-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-card--default .stat-card__label {
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
}

.stat-card--default .stat-card__body {
  padding: var(--space-xs) var(--space-md) var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.stat-card--default .stat-card__value {
  color: var(--foreground);
}

.stat-card--default .stat-card__diff {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
}
</style>
