<script setup lang="ts">
// Drawer — wrapper giữ API cũ (G-F1b): open/title/width + @close + slot default.
// Render bằng vaul-vue Drawer (shadcn drawer root) dạng panel trượt từ phải (direction="right").
// A11y: title qua DrawerTitle (reka DialogTitle — hết warning "must have a DialogTitle"),
// description tùy chọn qua DrawerDescription sr-only (aria-describedby — hết warning description).
import { computed } from 'vue';
import { messages } from '@/i18n/vi';
import { DrawerClose, DrawerContent, DrawerOverlay, DrawerPortal } from 'vaul-vue';

import { X } from 'lucide-vue-next';
import { Drawer as DrawerRoot, DrawerDescription, DrawerTitle } from './drawer';

const props = withDefaults(
  defineProps<{
    open?: boolean;
    modelValue?: boolean;
    title?: string;
    description?: string;
    width?: string;
  }>(),
  {
    open: undefined,
    modelValue: undefined,
    title: '',
    description: '',
    width: '420px',
  },
);

const emit = defineEmits<{
  close: [];
  'update:open': [value: boolean];
  'update:modelValue': [value: boolean];
}>();

const isOpen = computed(() => props.open ?? props.modelValue ?? false);

function onOpenChange(next: boolean): void {
  emit('update:open', next);
  emit('update:modelValue', next);
  if (!next && isOpen.value) emit('close');
}

function handleClose(): void {
  emit('close');
  emit('update:open', false);
  emit('update:modelValue', false);
}
</script>

<template>
  <DrawerRoot
    :open="isOpen"
    direction="right"
    :should-scale-background="false"
    @update:open="onOpenChange"
  >
    <DrawerPortal>
      <DrawerOverlay class="fixed inset-0 z-50 bg-black/40" @click="handleClose" />
      <DrawerContent
        class="fixed inset-y-0 right-0 z-50 flex h-full w-full flex-col border-l bg-background shadow-xl outline-none"
        :style="{ maxWidth: width ? `min(${width}, 95vw)` : 'min(420px, 95vw)' }"
      >
        <header class="flex items-center justify-between gap-4 border-b px-6 py-4">
          <DrawerTitle v-if="title" class="text-base font-semibold">{{ title }}</DrawerTitle>
          <DrawerTitle v-else class="sr-only">{{ messages.common.close }}</DrawerTitle>
          <button
            type="button"
            class="rounded-lg p-1.5 text-muted-foreground transition-colors hover:bg-accent hover:text-foreground cursor-pointer flex items-center justify-center"
            :aria-label="messages.common.close"
            @click="handleClose"
          >
            <X :size="18" />
          </button>
        </header>
        <DrawerDescription v-if="description" class="sr-only">{{ description }}</DrawerDescription>
        <div class="flex-1 overflow-y-auto p-6">
          <slot />
        </div>
      </DrawerContent>
    </DrawerPortal>
  </DrawerRoot>
</template>
