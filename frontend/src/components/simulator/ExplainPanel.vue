<template>
  <Transition name="explain-fade" mode="out-in">
    <div v-if="explanation" :key="frameKey" class="explain-panel">
      <div class="flex-1 flex items-center gap-2 flex-wrap">
        <span v-if="kindLabel" class="explain-panel__kind">{{ kindLabel }}</span>
        <span class="explain-panel__text" v-html="parseEmojiToSvg(explanation)"></span>
      </div>
      <button
        type="button"
        class="explain-panel__ai-btn"
        title="Hỏi AI phân tích bước này"
        @click="emit('ask-ai')"
      >
        <Sparkles :size="13" class="text-purple-300 shrink-0" />
        <span>Hỏi AI</span>
      </button>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { Sparkles } from 'lucide-vue-next';

import { messages } from '@/i18n/vi';
import { parseEmojiToSvg } from '@/utils/emojiParser';

const props = defineProps<{
  kind?: string;
  explanation: string;
  frameKey: number;
}>();

const emit = defineEmits<{
  'ask-ai': [];
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
  /* Giữ dòng mới khi explanation nhiều dòng (không phá parseEmojiToSvg — chỉ CSS) */
  white-space: pre-line;
}

.explain-panel__ai-btn {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  background: rgba(168, 85, 247, 0.15);
  border: 1px solid rgba(168, 85, 247, 0.35);
  border-radius: var(--radius-sm);
  padding: 4px 8px;
  font-size: 11px;
  font-weight: 600;
  color: #c084fc;
  cursor: pointer;
  white-space: nowrap;
  transition: all 150ms ease;
  margin-left: auto;
}

.explain-panel__ai-btn:hover {
  background: rgba(168, 85, 247, 0.28);
  border-color: #a855f7;
  color: #ffffff;
  box-shadow: 0 0 8px rgba(168, 85, 247, 0.3);
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
