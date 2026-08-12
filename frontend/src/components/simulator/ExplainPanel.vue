<template>
  <Transition name="explain-fade" mode="out-in">
    <div v-if="explanation" :key="frameKey" class="explain-panel">
      <span v-if="kindLabel" class="explain-panel__kind">{{ kindLabel }}</span>
      <span class="explain-panel__text" v-html="parseEmojiToSvg(explanation)"></span>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { computed } from 'vue';

import { messages } from '@/i18n/vi';
import { parseEmojiToSvg } from '@/utils/emojiParser';

// Bê từ source/VisualizationDSA1/frontend/src/components/VcrExplanationBanner.vue (V1).
// ĐIỀU CHỈNH: prop actionType → kind (TraceKind — SDD §4.0.3), nhãn kind → i18n,
// màu → design tokens. `kind` tùy chọn: khi không có (bước Step — SDD §4.2) chỉ hiện text.

const props = defineProps<{
  kind?: string;
  explanation: string;
  frameKey: number;
}>();

const kindLabel = computed(() => {
  if (!props.kind) return '';
  const kinds = messages.simulator.kinds;
  return kinds[props.kind as keyof typeof kinds] ?? '';
});
</script>

<style scoped>
.explain-panel {
  background: var(--color-surface-hover);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-md) var(--space-lg);
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  min-height: 48px;
}

.explain-panel__kind {
  font-size: var(--text-xs);
  font-weight: 700;
  color: var(--color-primary);
  background: color-mix(in srgb, var(--color-primary) 12%, transparent);
  padding: 3px 8px;
  border-radius: var(--radius-sm);
  text-transform: uppercase;
  letter-spacing: 0.5px;
  white-space: nowrap;
}

.explain-panel__text {
  font-size: var(--text-sm);
  color: var(--color-foreground);
}

.explain-fade-enter-active,
.explain-fade-leave-active {
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.explain-fade-enter-from {
  opacity: 0;
  transform: translateY(4px);
}

.explain-fade-leave-to {
  opacity: 0;
}
</style>
