<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue';
import { MoreHorizontal } from 'lucide-vue-next';
import type { Component } from 'vue';

export interface ActionDropdownItem {
  id: string;
  label: string;
  icon?: Component;
  danger?: boolean;
  disabled?: boolean;
  divider?: boolean;
  onClick?: () => void;
}

const props = withDefaults(
  defineProps<{
    items: ActionDropdownItem[];
    align?: 'left' | 'right';
    buttonLabel?: string;
    size?: 'sm' | 'md';
  }>(),
  {
    align: 'right',
    buttonLabel: 'Tùy chọn thao tác',
    size: 'sm',
  },
);

const emit = defineEmits<{
  select: [id: string];
}>();

const isOpen = ref(false);
const dropdownRef = ref<HTMLElement | null>(null);

function toggle(e: MouseEvent): void {
  e.stopPropagation();
  isOpen.value = !isOpen.value;
}

function handleSelect(item: ActionDropdownItem, e: MouseEvent): void {
  e.stopPropagation();
  if (item.disabled) return;
  isOpen.value = false;
  if (item.onClick) {
    item.onClick();
  }
  emit('select', item.id);
}

function onDocumentClick(e: MouseEvent): void {
  if (dropdownRef.value && !dropdownRef.value.contains(e.target as Node)) {
    isOpen.value = false;
  }
}

onMounted(() => {
  document.addEventListener('click', onDocumentClick);
});

onUnmounted(() => {
  document.removeEventListener('click', onDocumentClick);
});
</script>

<template>
  <div ref="dropdownRef" class="action-dropdown">
    <button
      type="button"
      class="action-dropdown__trigger"
      :class="[`action-dropdown__trigger--${size}`, { 'action-dropdown__trigger--active': isOpen }]"
      :aria-label="buttonLabel"
      :aria-expanded="isOpen"
      @click="toggle"
    >
      <slot name="trigger">
        <MoreHorizontal :size="size === 'sm' ? 16 : 18" />
      </slot>
    </button>

    <Transition name="action-dropdown-pop">
      <div
        v-if="isOpen"
        class="action-dropdown__menu"
        :class="[`action-dropdown__menu--${align}`]"
        role="menu"
      >
        <template v-for="(item, idx) in items" :key="item.id || idx">
          <div v-if="item.divider" class="action-dropdown__divider" />
          <button
            v-else
            type="button"
            class="action-dropdown__item"
            :class="{
              'action-dropdown__item--danger': item.danger,
              'action-dropdown__item--disabled': item.disabled,
            }"
            :disabled="item.disabled"
            role="menuitem"
            @click="handleSelect(item, $event)"
          >
            <component :is="item.icon" v-if="item.icon" :size="15" class="action-dropdown__item-icon" />
            <span class="action-dropdown__item-label">{{ item.label }}</span>
          </button>
        </template>
      </div>
    </Transition>
  </div>
</template>

<style scoped>
.action-dropdown {
  position: relative;
  display: inline-flex;
}

.action-dropdown__trigger {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  border-radius: var(--radius-md, 8px);
  border: 1px solid var(--border, rgba(255, 255, 255, 0.1));
  background: var(--surface, #161b22);
  color: var(--foreground-secondary, #8b949e);
  cursor: pointer;
  transition: all 150ms ease;
}

.action-dropdown__trigger--sm {
  width: 32px;
  height: 32px;
}

.action-dropdown__trigger--md {
  width: 38px;
  height: 38px;
}

.action-dropdown__trigger:hover,
.action-dropdown__trigger--active {
  background: var(--muted, #21262d);
  color: var(--foreground, #ffffff);
  border-color: var(--primary, #a855f7);
}

.action-dropdown__menu {
  position: absolute;
  top: calc(100% + 4px);
  z-index: var(--z-popover, 50);
  min-width: 160px;
  padding: 4px;
  background: rgba(22, 27, 34, 0.96);
  -webkit-backdrop-filter: blur(16px);
  backdrop-filter: blur(16px);
  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: var(--radius-lg, 10px);
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.45);
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.action-dropdown__menu--right {
  right: 0;
}

.action-dropdown__menu--left {
  left: 0;
}

.action-dropdown__item {
  display: flex;
  align-items: center;
  gap: 8px;
  width: 100%;
  padding: 7px 10px;
  border: none;
  background: transparent;
  color: rgba(255, 255, 255, 0.85);
  font-size: 13px;
  font-weight: 500;
  border-radius: var(--radius-md, 6px);
  text-align: left;
  cursor: pointer;
  transition: all 120ms ease;
  white-space: nowrap;
}

.action-dropdown__item:hover:not(:disabled) {
  background: rgba(168, 85, 247, 0.14);
  color: #c084fc;
}

.action-dropdown__item--danger {
  color: #ef4444;
}

.action-dropdown__item--danger:hover:not(:disabled) {
  background: rgba(239, 68, 68, 0.14) !important;
  color: #f87171 !important;
}

.action-dropdown__item--disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.action-dropdown__item-icon {
  flex-shrink: 0;
}

.action-dropdown__divider {
  height: 1px;
  background: rgba(255, 255, 255, 0.08);
  margin: 3px 0;
}

.action-dropdown-pop-enter-active,
.action-dropdown-pop-leave-active {
  transition: opacity 120ms ease, transform 120ms ease;
}

.action-dropdown-pop-enter-from,
.action-dropdown-pop-leave-to {
  opacity: 0;
  transform: translateY(-4px) scale(0.96);
}
</style>
