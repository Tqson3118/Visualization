<script setup lang="ts">
// Tooltip — component UI chung: chú giải khi hover (design tokens — SDD §8.1)
import { ref } from 'vue';

withDefaults(
  defineProps<{
    text?: string;
    position?: 'top' | 'bottom' | 'left' | 'right';
  }>(),
  {
    text: '',
    position: 'top',
  },
);

const visible = ref(false);
</script>

<template>
  <span
    class="ui-tooltip"
    @mouseenter="visible = true"
    @mouseleave="visible = false"
    @focusin="visible = true"
    @focusout="visible = false"
  >
    <slot />
    <span
      v-if="visible && text"
      class="ui-tooltip__bubble"
      :class="`ui-tooltip__bubble--${position}`"
      role="tooltip"
    >
      {{ text }}
    </span>
  </span>
</template>

<style scoped>
.ui-tooltip {
  position: relative;
  display: inline-flex;
}

.ui-tooltip__bubble {
  position: absolute;
  z-index: var(--z-tooltip);
  background: var(--color-foreground);
  color: var(--color-surface);
  font-size: var(--text-xs);
  padding: 4px 10px;
  border-radius: var(--radius-sm);
  white-space: nowrap;
  pointer-events: none;
  box-shadow: var(--shadow-md);
}

.ui-tooltip__bubble--top { bottom: calc(100% + 6px); left: 50%; transform: translateX(-50%); }
.ui-tooltip__bubble--bottom { top: calc(100% + 6px); left: 50%; transform: translateX(-50%); }
.ui-tooltip__bubble--left { right: calc(100% + 6px); top: 50%; transform: translateY(-50%); }
.ui-tooltip__bubble--right { left: calc(100% + 6px); top: 50%; transform: translateY(-50%); }
</style>
