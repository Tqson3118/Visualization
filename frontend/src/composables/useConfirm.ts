import { useUiStore } from '@/stores/ui';

export interface ConfirmOptions {
  title?: string;
  message: string;
  confirmLabel?: string;
  cancelLabel?: string;
  variant?: 'primary' | 'danger' | 'warning';
}

/**
 * useConfirm — SDD §3.6: modal xác nhận dạng promise (mọi thao tác xóa).
 * Kết quả resolve qua uiStore.closeModal(true/false).
 */
export function useConfirm() {
  const ui = useUiStore();

  function confirm(options: ConfirmOptions): Promise<boolean> {
    return new Promise<boolean>((resolve) => {
      ui.openModal('confirm', options, resolve);
    });
  }

  return { confirm };
}
