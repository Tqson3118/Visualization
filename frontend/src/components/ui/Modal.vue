<script setup lang="ts">
// Modal — component UI chung: overlay + dialog + Esc đóng (design tokens — SDD §8.1)
import { onMounted, onUnmounted, watch } from 'vue';

import { messages } from '@/i18n/vi';
import BaseIcon from './BaseIcon.vue';

const props = withDefaults(
  defineProps<{
    open: boolean;
    title?: string;
    closable?: boolean;
    width?: string;
  }>(),
  {
    title: '',
    closable: true,
    width: '560px',
  },
);

const emit = defineEmits<{
  close: [];
}>();

function onKeydown(event: KeyboardEvent): void {
  if (event.key === 'Escape' && props.open && props.closable) {
    emit('close');
  }
}

onMounted(() => window.addEventListener('keydown', onKeydown));
onUnmounted(() => window.removeEventListener('keydown', onKeydown));

watch(
  () => props.open,
  (open) => {
    document.body.style.overflow = open ? 'hidden' : '';
  },
);

function onOverlayClick(): void {
  if (props.closable) emit('close');
}
</script>

<template>
  <Teleport to="body">
    <Transition name="ui-modal">
      <div v-if="open" class="ui-modal" role="presentation" @click.self="onOverlayClick">
        <div
          class="ui-modal__dialog card"
          role="dialog"
          aria-modal="true"
          :aria-label="title || 'Dialog'"
          :style="{ maxWidth: width }"
        >
          <header v-if="title || closable" class="ui-modal__header">
            <h2 class="ui-modal__title">{{ title }}</h2>
            <button
              v-if="closable"
              type="button"
              class="ui-modal__close"
              :aria-label="messages.common.close"
              @click="emit('close')"
            >
              <BaseIcon name="x" :size="18" />
            </button>
          </header>
          <div class="ui-modal__body">
            <slot />
          </div>
          <footer v-if="$slots.footer" class="ui-modal__footer">
            <slot name="footer" />
          </footer>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.ui-modal {
  position: fixed;
  inset: 0;
  z-index: var(--z-modal);
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-md);
}

.ui-modal__dialog {
  width: 100%;
  max-height: 90vh;
  overflow-y: auto;
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  box-shadow: var(--shadow-xl);
}

.ui-modal__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-md);
}

.ui-modal__title { font-size: var(--text-lg); }

.ui-modal__close {
  background: none;
  border: none;
  color: var(--color-text-muted);
  cursor: pointer;
  padding: var(--space-xs);
  border-radius: var(--radius-sm);
}
.ui-modal__close:hover { background: var(--color-surface-hover); color: var(--color-foreground); }

.ui-modal__footer {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-sm);
  padding-top: var(--space-sm);
  border-top: 1px solid var(--color-border);
}

.ui-modal-enter-active,
.ui-modal-leave-active { transition: opacity 200ms ease; }
.ui-modal-enter-from,
.ui-modal-leave-to { opacity: 0; }
.ui-modal-enter-active .ui-modal__dialog { transition: transform 200ms ease; }
.ui-modal-enter-from .ui-modal__dialog { transform: translateY(12px) scale(0.98); }
</style>
