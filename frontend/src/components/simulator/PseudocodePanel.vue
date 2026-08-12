<script setup lang="ts">
// PseudocodePanel — panel mã giả (SDD §8.5 — trái 3/12)
// Hiển thị pseudocode[] + highlight dòng active (pseudocodeLine) + chip biến + breakpoint
import { computed } from 'vue';

import { messages } from '@/i18n/vi';

const props = withDefaults(
  defineProps<{
    pseudocode: string[];
    activeLine?: number;
    variables?: Record<string, unknown>;
    collapsed?: boolean;
  }>(),
  {
    activeLine: 0,
    variables: () => ({}),
    collapsed: false,
  },
);

const emit = defineEmits<{
  'update:collapsed': [value: boolean];
  'toggle-breakpoint': [line: number];
}>();

const variableChips = computed(() =>
  Object.entries(props.variables)
    .filter(([key, value]) => key !== 'array' && value !== undefined && value !== null)
    .slice(0, 8)
    .map(([key, value]) => ({ key, value: String(value) })),
);
</script>

<template>
  <section class="pseudo" :class="{ 'pseudo--collapsed': collapsed }" aria-label="Mã giả">
    <header class="pseudo__header">
      <h3 class="pseudo__title">{{ messages.simulator.controlsTitle }}</h3>
      <button
        type="button"
        class="pseudo__collapse"
        :aria-label="collapsed ? 'Mở mã giả' : 'Thu gọn mã giả'"
        @click="emit('update:collapsed', !collapsed)"
      >
        {{ collapsed ? '▸' : '▾' }}
      </button>
    </header>

    <ol v-if="!collapsed" class="pseudo__list">
      <li
        v-for="(line, idx) in pseudocode"
        :key="idx"
        class="pseudo__line"
        :class="{ 'pseudo__line--active': activeLine === idx + 1 }"
        :data-line="idx + 1"
        @click="emit('toggle-breakpoint', idx + 1)"
      >
        <span class="pseudo__no">{{ idx + 1 }}</span>
        <code class="pseudo__code">{{ line }}</code>
        <span v-if="activeLine === idx + 1" class="pseudo__arrow" aria-hidden="true">▶</span>
      </li>
    </ol>

    <div v-if="!collapsed && variableChips.length > 0" class="pseudo__vars">
      <span
        v-for="chip in variableChips"
        :key="chip.key"
        class="pseudo__var"
        :title="`${chip.key} = ${chip.value}`"
      >
        {{ chip.key }}={{ chip.value }}
      </span>
    </div>
  </section>
</template>

<style scoped>
.pseudo {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-surface);
  overflow: hidden;
  min-height: 200px;
}

.pseudo__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-sm) var(--space-md);
  border-bottom: 1px solid var(--color-border);
}

.pseudo__title { font-size: var(--text-sm); }

.pseudo__collapse {
  background: none;
  border: none;
  cursor: pointer;
  color: var(--color-text-muted);
  font-size: var(--text-sm);
}

.pseudo__list {
  list-style: none;
  overflow-y: auto;
  max-height: 420px;
  padding: var(--space-xs);
  display: flex;
  flex-direction: column;
  gap: 1px;
}

.pseudo__line {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: 3px 8px;
  border-radius: var(--radius-sm);
  font-size: var(--text-xs);
  font-family: var(--font-mono);
  position: relative;
  cursor: pointer;
}

.pseudo__line--active {
  background: color-mix(in srgb, var(--color-warning) 18%, transparent);
}

.pseudo__no {
  color: var(--color-text-disabled);
  min-width: 1.5rem;
  text-align: right;
  user-select: none;
}

.pseudo__code { color: var(--color-foreground); white-space: pre-wrap; }

.pseudo__arrow { color: var(--color-warning); font-size: 10px; }

.pseudo__vars {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  padding: var(--space-sm) var(--space-md);
  border-top: 1px solid var(--color-border);
}

.pseudo__var {
  font-size: var(--text-xs);
  font-family: var(--font-mono);
  background: var(--color-muted);
  padding: 2px 8px;
  border-radius: var(--radius-sm);
  color: var(--color-primary);
}
</style>
