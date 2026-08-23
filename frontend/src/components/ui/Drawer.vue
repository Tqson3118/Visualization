<script setup lang="ts">
// Drawer — wrapper giữ API cũ (G-F1b): open/title/width + @close + slot default.
// Render bằng vaul-vue Drawer (shadcn drawer root) dạng panel trượt từ phải (direction="right").
// A11y: title qua DrawerTitle (reka DialogTitle — hết warning "must have a DialogTitle"),
// description tùy chọn qua DrawerDescription sr-only (aria-describedby — hết warning description).
import { messages } from '@/i18n/vi';
import { DrawerContent, DrawerOverlay, DrawerPortal } from 'vaul-vue';

import { Drawer as DrawerRoot, DrawerDescription, DrawerTitle } from './drawer';
import BaseIcon from './BaseIcon.vue';

const props = withDefaults(
  defineProps<{
    open: boolean;
    title?: string;
    description?: string;
    width?: string;
  }>(),
  {
    title: '',
    description: '',
    width: '420px',
  },
);

const emit = defineEmits<{
  close: [];
}>();

function onOpenChange(next: boolean): void {
  if (!next && props.open) emit('close');
}
</script>

<template>
  <DrawerRoot
    :open="open"
    direction="right"
    :should-scale-background="false"
    @update:open="onOpenChange"
  >
    <DrawerPortal>
      <DrawerOverlay class="fixed inset-0 z-50 bg-black/40" />
      <DrawerContent
        class="fixed inset-y-0 right-0 z-50 flex h-full w-full flex-col border-l bg-background shadow-xl outline-none"
        :style="{ maxWidth: width }"
      >
        <header class="flex items-center justify-between gap-4 border-b px-6 py-4">
          <DrawerTitle v-if="title" class="text-base font-semibold">{{ title }}</DrawerTitle>
          <DrawerTitle v-else class="sr-only">{{ messages.common.close }}</DrawerTitle>
          <button
            type="button"
            class="rounded-sm p-1 text-muted-foreground transition-colors hover:bg-accent hover:text-foreground"
            :aria-label="messages.common.close"
            @click="emit('close')"
          >
            <BaseIcon name="x" :size="18" />
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
