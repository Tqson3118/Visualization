<template>
  <Teleport to="body">
    <div class="toast-container" aria-live="polite" aria-atomic="true">
      <TransitionGroup name="toast-slide">
        <div
          v-for="toast in ui.toasts"
          :key="toast.id"
          class="toast-item"
          :class="`toast-item--${toast.type}`"
          role="alert"
          @click="ui.dismissToast(toast.id)"
        >
          <div class="toast-icon">
            <BaseIcon :name="iconFor(toast.type)" :size="14" />
          </div>
          <div class="toast-body">
            <div class="toast-message">{{ toast.message }}</div>
          </div>
          <button
            class="toast-close"
            :aria-label="messages.common.close"
            @click.stop="ui.dismissToast(toast.id)"
          >×</button>
          <div class="toast-progress" style="animation-duration: 4s" />
        </div>
      </TransitionGroup>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { messages } from '@/i18n/vi';
import { useUiStore, type ToastType } from '@/stores/ui';
import BaseIcon from './BaseIcon.vue';

// Bê từ source/VisualizationDSA1/frontend/src/components/ToastContainer.vue (V1).
// ĐIỀU CHỈNH: dùng uiStore mới (toasts: {id,type,message} + dismissToast) thay vì
// toastStore cũ (có title/duration); icon chữ đơn giản × (không phụ thuộc BaseIcon cũ);
// CSS chuyển sang design tokens light (--color-success/warning/...).
const ui = useUiStore();

function iconFor(type: ToastType): string {
  switch (type) {
    case 'success': return 'check';
    case 'error': return 'x-mark';
    case 'warning': return 'warning';
    default: return 'info';
  }
}
</script>

<style scoped>
@import "./ToastContainer.css";
</style>
