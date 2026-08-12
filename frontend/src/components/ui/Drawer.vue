<script setup lang="ts">
// Drawer — component UI chung: panel trượt từ cạnh phải (ghi chú, bộ lọc…)
import { watch } from 'vue';

import { messages } from '@/i18n/vi';
import BaseIcon from './BaseIcon.vue';

const props = withDefaults(
  defineProps<{
    open: boolean;
    title?: string;
    width?: string;
  }>(),
  {
    title: '',
    width: '420px',
  },
);

const emit = defineEmits<{
  close: [];
}>();

watch(
  () => props.open,
  (open) => {
    document.body.style.overflow = open ? 'hidden' : '';
  },
);
</script>

<template>
  <Teleport to="body">
    <Transition name="ui-drawer">
      <div v-if="open" class="ui-drawer" @click.self="emit('close')">
        <aside class="ui-drawer__panel card" role="dialog" aria-modal="true" :aria-label="title || 'Drawer'" :style="{ maxWidth: width }">
          <header class="ui-drawer__header">
            <h2 class="ui-drawer__title">{{ title }}</h2>
            <button type="button" class="ui-drawer__close" :aria-label="messages.common.close" @click="emit('close')">
              <BaseIcon name="x" :size="18" />
            </button>
          </header>
          <div class="ui-drawer__body">
            <slot />
          </div>
        </aside>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.ui-drawer {
  position: fixed;
  inset: 0;
  z-index: var(--z-modal);
  background: rgba(0, 0, 0, 0.4);
  display: flex;
  justify-content: flex-end;
}

.ui-drawer__panel {
  width: 100%;
  height: 100%;
  border-radius: 0;
  border: none;
  border-left: 1px solid var(--color-border);
  display: flex;
  flex-direction: column;
  box-shadow: var(--shadow-xl);
}

.ui-drawer__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-md) var(--space-lg);
  border-bottom: 1px solid var(--color-border);
}

.ui-drawer__title { font-size: var(--text-md); }

.ui-drawer__close {
  background: none;
  border: none;
  color: var(--color-text-muted);
  cursor: pointer;
  padding: var(--space-xs);
  border-radius: var(--radius-sm);
}
.ui-drawer__close:hover { background: var(--color-surface-hover); }

.ui-drawer__body {
  flex: 1;
  overflow-y: auto;
  padding: var(--space-lg);
}

.ui-drawer-enter-active,
.ui-drawer-leave-active { transition: opacity 200ms ease; }
.ui-drawer-enter-from,
.ui-drawer-leave-to { opacity: 0; }
.ui-drawer-enter-active .ui-drawer__panel { transition: transform 250ms ease; }
.ui-drawer-enter-from .ui-drawer__panel,
.ui-drawer-leave-to .ui-drawer__panel { transform: translateX(100%); }
</style>
