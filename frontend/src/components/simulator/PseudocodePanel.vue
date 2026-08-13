<script setup lang="ts">
// PseudocodePanel — panel mã giả (SDD §8.5 — trái 3/12)
// Hiển thị pseudocode[] + highlight dòng active (pseudocodeLine) + chip biến + breakpoint
// G-F2c: style code panel (gutter số dòng + syntax highlight nhẹ + active line mint).
// UI-PREMIUM 1B: highlight dòng active trượt MƯỢT (overlay theo offsetTop/offsetHeight
// của dòng thật, transition ease-out-expo — không snap).
import { computed, nextTick, onMounted, ref, watch } from 'vue';

import { ChevronDown, ChevronRight } from 'lucide-vue-next';

import { messages } from '@/i18n/vi';

const props = withDefaults(
  defineProps<{
    pseudocode: string[];
    activeLine?: number;
    variables?: Record<string, unknown>;
    collapsed?: boolean;
    breakpoints?: Set<number>;
  }>(),
  {
    activeLine: 0,
    variables: () => ({}),
    collapsed: false,
    breakpoints: () => new Set<number>(),
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

/** Syntax highlight nhẹ — escape HTML rồi tô token (keyword/số/chuỗi/chú thích). */
function escapeHtml(raw: string): string {
  return raw
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;');
}

const KEYWORDS =
  '\\b(if|else|then|for|while|do|repeat|until|return|break|function|end|begin|procedure)\\b';

function highlightLine(line: string): string {
  const escaped = escapeHtml(line);
  return escaped
    .replace(/(\/\/.*$|#.*$|--.*$)/g, '<span class="pseudo__tok--comment">$1</span>')
    .replace(/"([^"]*)"/g, '<span class="pseudo__tok--string">"$1"</span>')
    .replace(/\b(\d+(?:\.\d+)?)\b/g, '<span class="pseudo__tok--num">$1</span>')
    .replace(new RegExp(KEYWORDS, 'g'), '<span class="pseudo__tok--kw">$1</span>');
}

function highlight(line: string): string {
  return highlightLine(line);
}

/* ── Overlay highlight trượt — vị trí bám theo dòng active thật (offsetTop/offsetHeight) ── */
const listRef = ref<HTMLElement | null>(null);
const activeTop = ref(0);
const activeHeight = ref(24);

function syncHighlight(): void {
  if (!listRef.value) return;
  const line = listRef.value.querySelector<HTMLElement>(`[data-line="${props.activeLine}"]`);
  if (!line) return;
  activeTop.value = line.offsetTop;
  activeHeight.value = line.offsetHeight;
}

watch(
  () => props.activeLine,
  () => void nextTick(syncHighlight),
  { immediate: true },
);

watch(
  () => props.pseudocode.length,
  () => void nextTick(syncHighlight),
);

onMounted(() => void nextTick(syncHighlight));
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
        <component :is="collapsed ? ChevronRight : ChevronDown" :size="16" aria-hidden="true" />
      </button>
    </header>

    <ol v-if="!collapsed" ref="listRef" class="pseudo__list">
      <li
        v-for="(line, idx) in pseudocode"
        :key="idx"
        class="pseudo__line"
        :class="{
          'pseudo__line--active': activeLine === idx + 1,
          'pseudo__line--bp': breakpoints.has(idx + 1),
        }"
        :data-line="idx + 1"
        :data-bp="breakpoints.has(idx + 1) ? '1' : '0'"
        @click="emit('toggle-breakpoint', idx + 1)"
      >
        <button
          type="button"
          class="pseudo__bp"
          :class="{ 'pseudo__bp--on': breakpoints.has(idx + 1) }"
          :aria-label="`Bật/tắt breakpoint dòng ${idx + 1}`"
          :aria-pressed="breakpoints.has(idx + 1)"
          :data-bp-line="idx + 1"
          @click.stop="emit('toggle-breakpoint', idx + 1)"
        >
          <span class="pseudo__bp-dot" aria-hidden="true" />
        </button>
        <span class="pseudo__no">{{ idx + 1 }}</span>
        <code class="pseudo__code" v-html="highlight(line)" />
        <span v-if="activeLine === idx + 1" class="pseudo__arrow" aria-hidden="true">▶</span>
      </li>
      <span
        v-if="activeLine > 0 && activeLine <= pseudocode.length"
        class="pseudo__highlight"
        :style="{ top: `${activeTop}px`, height: `${activeHeight}px` }"
        aria-hidden="true"
      />
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
  box-shadow: var(--shadow-sm);
}

.pseudo__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-sm) var(--space-md);
  border-bottom: 1px solid var(--color-border);
  background: color-mix(in srgb, var(--color-background) 60%, var(--color-muted));
}

.pseudo__title {
  font-size: var(--text-sm);
  font-weight: 700;
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.pseudo__title::before {
  content: '';
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background-image: var(--gradient-mint);
}

.pseudo__collapse {
  background: none;
  border: none;
  cursor: pointer;
  color: var(--color-text-muted);
  font-size: var(--text-sm);
  display: inline-flex;
  align-items: center;
  padding: 2px;
  border-radius: var(--radius-sm);
  transition: var(--transition-fast);
}

.pseudo__collapse:hover { color: var(--color-primary); background: var(--color-surface-hover); }

.pseudo__list {
  list-style: none;
  overflow-y: auto;
  max-height: 420px;
  padding: var(--space-xs);
  display: flex;
  flex-direction: column;
  gap: 2px;
  position: relative;
}

/* Overlay highlight — trượt mượt theo dòng active (UI-PREMIUM 1B) */
.pseudo__highlight {
  position: absolute;
  left: 0;
  right: 0;
  z-index: 0;
  border-radius: var(--radius-sm);
  background: color-mix(in srgb, var(--color-primary) 12%, transparent);
  box-shadow: inset 3px 0 0 var(--color-primary);
  pointer-events: none;
  transition:
    top var(--duration-normal) var(--ease-out-expo),
    height var(--duration-normal) var(--ease-out-expo);
}

.pseudo__line {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: 4px 10px;
  border-radius: var(--radius-sm);
  font-size: var(--text-xs);
  font-family: var(--font-mono);
  position: relative;
  z-index: 1;
  cursor: pointer;
  transition: background 120ms ease;
}

.pseudo__line:hover { background: var(--color-surface-hover); }

.pseudo__line--active { background: transparent; box-shadow: none; }

@media (prefers-reduced-motion: reduce) {
  .pseudo__highlight { transition: none; }
}

/* ── Breakpoint (GP-T4): chấm tròn toggle — đỏ khi bật ── */
.pseudo__bp {
  flex: none;
  width: 18px;
  height: 18px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  background: none;
  border: none;
  padding: 0;
  border-radius: var(--radius-sm);
  cursor: pointer;
  color: var(--color-text-disabled);
  transition: var(--transition-fast);
}

.pseudo__bp:hover { background: var(--color-surface-hover); color: var(--color-text-muted); }

.pseudo__bp-dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  border: 2px solid currentColor;
  transition: var(--transition-fast);
}

.pseudo__bp--on { color: var(--color-destructive); }
.pseudo__bp--on .pseudo__bp-dot { background: var(--color-destructive); border-color: var(--color-destructive); }

.pseudo__line--bp { box-shadow: inset 3px 0 0 var(--color-destructive); }

.pseudo__no {
  color: var(--color-text-disabled);
  min-width: 1.5rem;
  text-align: right;
  user-select: none;
  font-size: 10px;
}

.pseudo__code { color: var(--color-foreground); white-space: pre-wrap; }

.pseudo__line--active .pseudo__code { font-weight: 600; }

.pseudo__arrow { color: var(--color-primary); font-size: 10px; }

/* ── Syntax highlight nhẹ ── */
.pseudo__tok--kw { color: var(--color-primary); font-weight: 700; }
.pseudo__tok--num { color: var(--color-warning); }
.pseudo__tok--string { color: var(--color-success); }
.pseudo__tok--comment { color: var(--color-text-disabled); font-style: italic; }

.pseudo__vars {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  padding: var(--space-sm) var(--space-md);
  border-top: 1px solid var(--color-border);
  background: color-mix(in srgb, var(--color-background) 60%, var(--color-muted));
}

.pseudo__var {
  font-size: var(--text-xs);
  font-family: var(--font-mono);
  background: color-mix(in srgb, var(--color-primary) 10%, var(--color-surface));
  padding: 2px 8px;
  border-radius: var(--radius-sm);
  color: var(--color-primary);
  border: 1px solid color-mix(in srgb, var(--color-primary) 24%, transparent);
}
</style>
