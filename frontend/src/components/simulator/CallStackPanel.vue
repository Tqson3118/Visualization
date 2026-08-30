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
  <section class="callstack" aria-label="Call stack">
    <header class="callstack__header flex items-center justify-between">
      <h4 class="callstack__title text-xs font-bold text-white">Ngăn xếp đệ quy ({{ depth }})</h4>
      <span v-if="frames.length === 0" class="text-[10px] text-vdsa-muted font-mono">Không hoạt động</span>
    </header>
    <ol v-if="frames.length > 0" class="callstack__list space-y-1 p-2">
      <li
        v-for="(frame, idx) in frames"
        :key="idx"
        class="callstack__frame p-1.5 rounded bg-vdsa-surface text-xs flex justify-between"
        :class="{ 'callstack__frame--top': idx === frames.length - 1 }"
      >
        <span class="callstack__name font-mono text-purple-300">{{ frame.name }}</span>
        <span class="callstack__depth text-slate-400 text-[11px]">depth {{ frame.depth }}</span>
      </li>
    </ol>
    <div v-else class="p-3 text-center text-xs text-vdsa-muted italic leading-relaxed">
      Thuật toán này chạy vòng lặp tuần tự (Iterative), không sử dụng ngăn xếp đệ quy.
    </div>
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
