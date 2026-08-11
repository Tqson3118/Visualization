import { defineStore } from 'pinia';
import { ref } from 'vue';

export interface ConfirmOptions {
  title: string;
  message: string;
  details?: string;
  confirmText?: string;
  cancelText?: string;
  variant?: 'danger' | 'primary' | 'warning';
  icon?: string;
}

export const useConfirmDialogStore = defineStore('confirmDialog', () => {
  const visible = ref(false);
  const options = ref<ConfirmOptions | null>(null);

  let pendingResolve: ((value: boolean) => void) | null = null;

  function confirmDialog(opts: ConfirmOptions): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      options.value = opts;
      pendingResolve = resolve;
      visible.value = true;
    });
  }

  function resolveConfirm(value: boolean): void {
    visible.value = false;
    options.value = null;
    if (pendingResolve) {
      const resolve = pendingResolve;
      pendingResolve = null;
      resolve(value);
    }
  }

  return {
    visible,
    options,
    confirmDialog,
    resolveConfirm,
  };
});
