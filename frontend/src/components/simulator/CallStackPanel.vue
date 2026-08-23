<script setup lang="ts">
// CallStackPanel — ngăn xếp đệ quy từ trace variables (FR-3.14, tối đa 15 frame)
import { computed } from 'vue';

import { messages } from '@/i18n/vi';

const props = defineProps<{
  variables?: Record<string, unknown>;
}>();

const frames = computed(() => {
  const raw = props.variables?.callStack;
  if (!Array.isArray(raw)) return [];
  return raw.slice(0, 15).map((frame) => {
    if (typeof frame === 'object' && frame !== null) {
      const f = frame as Record<string, unknown>;
      return { name: String(f.functionName ?? '?'), depth: Number(f.depth ?? 0) };
    }
    return { name: String(frame), depth: 0 };
  });
});

const depth = computed(() => Number(props.variables?.recursionDepth ?? frames.value.length));
</script>

<template>
  <section v-if="frames.length > 0" class="callstack" aria-label="Call stack">
    <header class="callstack__header">
      <h4 class="callstack__title">Call stack ({{ depth }})</h4>
    </header>
    <ol class="callstack__list">
      <li
        v-for="(frame, idx) in frames"
        :key="idx"
        class="callstack__frame"
        :class="{ 'callstack__frame--top': idx === frames.length - 1 }"
      >
        <span class="callstack__name">{{ frame.name }}</span>
        <span class="callstack__depth">depth {{ frame.depth }}</span>
      </li>
    </ol>
    <p v-if="frames.length >= 15" class="callstack__note">
      {{ messages.simulator.stepOf.replace('{current}', '15').replace('{total}', '15') }}+ frame — hiển thị tối đa 15
    </p>
  </section>
</template>

<style scoped>
.callstack {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-surface);
  overflow: hidden;
}

.callstack__header {
  padding: var(--space-sm) var(--space-md);
  border-bottom: 1px solid var(--color-border);
}

.callstack__title { font-size: var(--text-sm); color: var(--color-primary); }

.callstack__list {
  list-style: none;
  display: flex;
  flex-direction: column;
  padding: var(--space-xs);
  max-height: 220px;
  overflow-y: auto;
}

.callstack__frame {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-sm);
  padding: 5px 10px;
  font-size: var(--text-xs);
  font-family: var(--font-mono);
  border-radius: var(--radius-sm);
}

.callstack__frame--top { background: color-mix(in srgb, var(--color-primary) 12%, transparent); }

.callstack__depth { color: var(--color-text-muted); }

.callstack__note { padding: 0 var(--space-md) var(--space-sm); font-size: var(--text-xs); color: var(--color-text-muted); }
</style>
