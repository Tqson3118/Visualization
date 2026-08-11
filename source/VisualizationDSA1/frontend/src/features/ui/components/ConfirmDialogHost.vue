<template>
  <ConfirmModal
    :show="store.visible"
    :title="store.options?.title ?? 'Xác nhận'"
    :message="store.options?.message ?? ''"
    :details="store.options?.details"
    :confirm-text="store.options?.confirmText"
    :cancel-text="store.options?.cancelText"
    :variant="store.options?.variant"
    :icon="store.options?.icon"
    @update:show="(v: boolean) => { if (!v) store.resolveConfirm(false) }"
    @confirm="store.resolveConfirm(true)"
  />
</template>

<script setup lang="ts">
import ConfirmModal from '@/components/ui/ConfirmModal.vue';
import { useConfirmDialogStore } from '@/features/ui/store/useConfirmDialogStore';

const store = useConfirmDialogStore();
</script>
