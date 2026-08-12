<script setup lang="ts">
// Modal — wrapper giữ API cũ (G-F1b): open/title/closable/width + @close + slot default/footer.
// Render bằng shadcn-vue Dialog (DialogScrollContent để giữ scroll khi nội dung dài).
import { messages } from '@/i18n/vi';
import { cn } from '@/lib/utils';
import { Dialog, DialogFooter, DialogHeader, DialogScrollContent, DialogTitle } from './dialog';

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

function onOpenChange(next: boolean): void {
  if (!next && props.open) emit('close');
}

// Khi closable=false: chặn đóng bằng overlay/Esc + ẩn nút X (giữ hành vi Modal cũ).
function onEscapeKeyDown(event: Event): void {
  if (!props.closable) event.preventDefault();
}
function onPointerDownOutside(event: Event): void {
  if (!props.closable) event.preventDefault();
}
</script>

<template>
  <Dialog :open="open" @update:open="onOpenChange">
    <DialogScrollContent
      :class="cn('w-full', !closable && '[&>button]:hidden')"
      :style="{ maxWidth: width }"
      @escape-key-down="onEscapeKeyDown"
      @pointer-down-outside="onPointerDownOutside"
    >
      <DialogHeader>
        <DialogTitle v-if="title">{{ title }}</DialogTitle>
        <DialogTitle v-else class="sr-only">{{ messages.common.close }}</DialogTitle>
      </DialogHeader>
      <div>
        <slot />
      </div>
      <DialogFooter v-if="$slots.footer" class="border-t pt-4">
        <slot name="footer" />
      </DialogFooter>
    </DialogScrollContent>
  </Dialog>
</template>
