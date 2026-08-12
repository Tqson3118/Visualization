<script setup lang="ts">
import { RouterView } from 'vue-router';

import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';

const ui = useUiStore();
</script>

<template>
  <div class="app-shell">
    <RouterView />

    <!-- Toast container (qua uiStore) -->
    <div class="toast-container" aria-live="polite" aria-atomic="false">
      <TransitionGroup name="toast">
        <div
          v-for="toast in ui.toasts"
          :key="toast.id"
          class="toast"
          :class="`toast--${toast.type}`"
          role="status"
        >
          <span class="toast__message">{{ toast.message }}</span>
          <button
            type="button"
            class="toast__close"
            :aria-label="messages.common.close"
            @click="ui.dismissToast(toast.id)"
          >
            &times;
          </button>
        </div>
      </TransitionGroup>
    </div>
  </div>
</template>

<style scoped>
.app-shell {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.toast-container {
  position: fixed;
  top: var(--space-md);
  right: var(--space-md);
  z-index: var(--z-toast);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  max-width: min(24rem, calc(100vw - 2 * var(--space-md)));
}

.toast {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  box-shadow: var(--shadow-lg);
  color: var(--color-foreground);
  font-size: var(--text-sm);
}

.toast--success { border-color: var(--color-secondary); }
.toast--error { border-color: var(--color-destructive); }
.toast--warning { border-color: var(--color-accent); }

.toast__close {
  background: none;
  border: none;
  color: inherit;
  font-size: 1.25rem;
  line-height: 1;
  cursor: pointer;
  padding: 0 var(--space-xs);
}

.toast-enter-active,
.toast-leave-active {
  transition: all 200ms ease;
}

.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateX(0.5rem);
}
</style>
