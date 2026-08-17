<script setup lang="ts">
// ArrayBarsRenderer — renderer adapter "Array/Sorting" cho SharedVisualizerShell.
// Render trạng thái Structure (kind='array') thành cột bar thuần DOM/CSS — nhẹ, responsive,
// không cần canvas/WebGL => hoạt động tốt trong test và viewport nhỏ.
import { computed } from 'vue';

import type { ElementStatus, Structure } from '@/engines/core/types';

const props = withDefaults(
  defineProps<{
    structure: Structure | null;
    showIndex?: boolean;
  }>(),
  { structure: null, showIndex: true },
);

interface Bar {
  id: string;
  index: number;
  value: number;
  label: string;
  status: ElementStatus;
}

const bars = computed<Bar[]>(() => {
  if (!props.structure || props.structure.kind !== 'array') return [];
  return props.structure.elements.map((el, i) => {
    const n = Number.parseFloat(el.label);
    return {
      id: el.id,
      index: i,
      value: Number.isFinite(n) ? n : 0,
      label: el.label,
      status: el.status ?? 'default',
    };
  });
});

const maxValue = computed(() => {
  let m = 0;
  for (const b of bars.value) m = Math.max(m, Math.abs(b.value));
  return m || 1;
});

function barHeight(value: number): string {
  const pct = Math.max(6, Math.round((Math.abs(value) / maxValue.value) * 100));
  return `${pct}%`;
}
</script>

<template>
  <div class="array-bars" data-testid="array-bars-renderer" :class="{ 'array-bars--indexed': showIndex }">
    <div v-if="bars.length === 0" class="array-bars__empty">Chưa có dữ liệu mảng.</div>
    <template v-else>
      <div class="array-bars__row" role="img" :aria-label="'Mảng ' + bars.map(b => b.label).join(', ')">
        <div
          v-for="bar in bars"
          :key="bar.id"
          class="array-bars__cell"
          :class="`bar--${bar.status}`"
          :style="{ height: barHeight(bar.value) }"
          :title="`a[${bar.index}] = ${bar.label}`"
          data-testid="array-bar"
        >
          <span class="array-bars__value">{{ bar.label }}</span>
        </div>
      </div>
      <div v-if="showIndex" class="array-bars__indexes" aria-hidden="true">
        <span v-for="bar in bars" :key="bar.id + '_idx'" class="array-bars__index">{{ bar.index }}</span>
      </div>
    </template>
  </div>
</template>

<style scoped>
.array-bars {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  width: 100%;
  height: 100%;
  min-height: 0;
  justify-content: flex-end;
}

.array-bars__empty {
  margin: auto;
  color: var(--color-text-tertiary);
  font-size: var(--text-sm);
}

.array-bars__row {
  flex: 1;
  min-height: 180px;
  display: flex;
  align-items: flex-end;
  gap: clamp(2px, 1vw, 8px);
  padding-inline: var(--space-sm);
  overflow-x: auto;
}

.array-bars__cell {
  flex: 1 1 0;
  min-width: 18px;
  max-width: 64px;
  border-radius: var(--radius-sm) var(--radius-sm) 0 0;
  display: flex;
  align-items: flex-end;
  justify-content: center;
  transition: background-color 150ms ease, height 120ms ease;
}

.array-bars__value {
  font-family: var(--font-mono);
  font-size: 11px;
  font-weight: 600;
  color: rgba(255, 255, 255, 0.92);
  padding-bottom: 4px;
  white-space: nowrap;
}

.array-bars__indexes {
  display: flex;
  gap: clamp(2px, 1vw, 8px);
  padding-inline: var(--space-sm);
  border-top: 1px solid rgba(255, 255, 255, 0.08);
}

.array-bars__index {
  flex: 1 1 0;
  text-align: center;
  font-family: var(--font-mono);
  font-size: 10px;
  color: var(--color-index-muted);
  padding-top: 4px;
}

/* — Trạng thái (tokens canvas — DESIGN-IDENTITY §1.2) — */
.bar--default { background: color-mix(in srgb, var(--color-data-core) 55%, transparent); }
.bar--active  { background: var(--color-primary); }
.bar--swap    { background: var(--color-conflict); }
.bar--done    { background: var(--color-resolved); }
.bar--highlight { background: var(--color-accent); }
.bar--error   { background: var(--color-destructive); }
.bar--muted   { background: var(--color-index-muted); opacity: 0.45; }
</style>
