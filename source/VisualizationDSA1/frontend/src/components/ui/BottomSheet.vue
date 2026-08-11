<template>
  <Transition name="bottom-sheet">
    <div v-if="modelValue" class="bottom-sheet-overlay" @click.self="handleOverlayClick">
      <div 
        class="bottom-sheet" 
        :class="{ 'bottom-sheet--full': fullscreen }"
        @touchstart.passive="handleTouchStart"
        @touchmove.passive="handleTouchMove"
        @touchend="handleTouchEnd"
      >
        <!-- Drag Handle -->
        <div class="bottom-sheet__handle" @click="handleHandleClick" aria-label="Kéo để đóng">
          <div class="handle-bar" />
        </div>
        
        <!-- Header -->
        <header v-if="title || showClose" class="bottom-sheet__header">
          <h2 v-if="title" class="bottom-sheet__title">{{ title }}</h2>
          <button 
            v-if="showClose" 
            class="bottom-sheet__close" 
            @click="$emit('update:modelValue', false)"
            aria-label="Đóng"
          >
            <BaseIcon name="x" class="w-5 h-5" />
          </button>
        </header>
        
        <!-- Content -->
        <div class="bottom-sheet__content">
          <slot />
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import BaseIcon from '../shared/components/BaseIcon.vue';

interface Props {
  modelValue: boolean;
  title?: string;
  fullscreen?: boolean;
  showClose?: boolean;
  closeOnOverlayClick?: boolean;
  closeOnHandleClick?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  fullscreen: false,
  showClose: true,
  closeOnOverlayClick: true,
  closeOnHandleClick: true,
});

const emit = defineEmits<{
  'update:modelValue': [value: boolean];
  close: [];
}>();

const dragOffset = ref(0);
const startY = ref(0);
const isDragging = ref(false);
const sheetRef = ref<HTMLElement | null>(null);

const threshold = computed(() => window.innerHeight * 0.3);

function handleOverlayClick() {
  if (props.closeOnOverlayClick) {
    close();
  }
}

function handleHandleClick() {
  if (props.closeOnHandleClick && !props.fullscreen) {
    close();
  }
}

function handleTouchStart(e: TouchEvent) {
  if (props.fullscreen) return;
  startY.value = e.touches[0].clientY;
  isDragging.value = true;
}

function handleTouchMove(e: TouchEvent) {
  if (!isDragging.value || props.fullscreen) return;
  
  const currentY = e.touches[0].clientY;
  const delta = currentY - startY.value;
  
  // Only allow dragging down
  if (delta > 0) {
    dragOffset.value = delta;
    e.preventDefault();
  }
}

function handleTouchEnd() {
  if (!isDragging.value || props.fullscreen) return;
  
  isDragging.value = false;
  
  if (dragOffset.value > threshold.value) {
    close();
  } else {
    // Snap back
    dragOffset.value = 0;
  }
}

function close() {
  emit('update:modelValue', false);
  emit('close');
}

defineExpose({
  close,
});
</script>

<style scoped>
.bottom-sheet-overlay {
  position: fixed;
  inset: 0;
  z-index: var(--z-modal, 1000);
  display: flex;
  align-items: flex-end;
  justify-content: center;
  background: rgba(0, 0, 0, 0.5);
  backdrop-filter: blur(4px);
  -webkit-backdrop-filter: blur(4px);
  animation: fadeIn 0.2s ease-out;
}

.bottom-sheet {
  width: 100%;
  max-width: 100%;
  background: var(--color-bg-surface);
  border-radius: 24px 24px 0 0;
  border: 1px solid var(--color-border-default);
  border-bottom: none;
  box-shadow: 0 -25px 50px -12px rgba(0, 0, 0, 0.5);
  display: flex;
  flex-direction: column;
  max-height: 90vh;
  overflow: hidden;
  transform: translateY(var(--drag-offset, 0));
  transition: transform 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.bottom-sheet--full {
  max-height: 100vh;
  border-radius: 0;
  height: 100%;
}

.bottom-sheet__handle {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 12px;
  cursor: grab;
  -webkit-user-select: none;
  user-select: none;
}

.handle-bar {
  width: 40px;
  height: 5px;
  background: var(--color-border-default);
  border-radius: 3px;
  transition: background 0.2s ease;
}

.bottom-sheet__handle:hover .handle-bar,
.bottom-sheet__handle:active .handle-bar {
  background: var(--color-text-muted);
}

.bottom-sheet__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 20px 16px;
  border-bottom: 1px solid var(--color-border-subtle);
  flex-shrink: 0;
}

.bottom-sheet__title {
  font-size: 1.125rem;
  font-weight: 600;
  color: var(--color-text-heading);
  margin: 0;
  font-family: var(--font-display);
}

.bottom-sheet__close {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: var(--color-bg-surface);
  border: 1px solid var(--color-border-default);
  color: var(--color-text-secondary);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s ease;
  flex-shrink: 0;
  min-width: 44px;
  min-height: 44px;
}

.bottom-sheet__close:hover {
  background: var(--color-accent-primary);
  border-color: var(--color-accent-primary);
  color: white;
}

.bottom-sheet__content {
  flex: 1;
  overflow-y: auto;
  padding: 20px;
  -webkit-overflow-scrolling: touch;
}

@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}

/* Transition */
.bottom-sheet-enter-active,
.bottom-sheet-leave-active {
  transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
}

.bottom-sheet-enter-from {
  opacity: 0;
}

.bottom-sheet-enter-from .bottom-sheet {
  transform: translateY(100%);
}

.bottom-sheet-leave-to {
  opacity: 0;
}

.bottom-sheet-leave-to .bottom-sheet {
  transform: translateY(100%);
}

/* Safe area support */
@supports (padding-bottom: env(safe-area-inset-bottom)) {
  .bottom-sheet__content {
    padding-bottom: calc(20px + env(safe-area-inset-bottom));
  }
}

/* Reduced motion */
@media (prefers-reduced-motion: reduce) {
  .bottom-sheet-overlay,
  .bottom-sheet,
  .bottom-sheet-enter-active,
  .bottom-sheet-leave-active {
    animation: none;
    transition: none;
  }
}

/* Landscape mobile optimization */
@media (max-height: 500px) and (orientation: landscape) {
  .bottom-sheet {
    max-height: 85vh;
    border-radius: 16px 16px 0 0;
  }
  
  .bottom-sheet--full {
    max-height: 100vh;
  }
}
</style>