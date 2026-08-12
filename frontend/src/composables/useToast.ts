import { useUiStore, type ToastType } from '@/stores/ui';

/** useToast — SDD §3.6: mọi thông báo qua store ui */
export function useToast() {
  const ui = useUiStore();

  return {
    show(message: string, type: ToastType = 'info'): number {
      return ui.showToast(message, type);
    },
    success(message: string): number {
      return ui.showToast(message, 'success');
    },
    error(message: string): number {
      return ui.showToast(message, 'error');
    },
    warning(message: string): number {
      return ui.showToast(message, 'warning');
    },
    info(message: string): number {
      return ui.showToast(message, 'info');
    },
    dismiss(id: number): void {
      ui.dismissToast(id);
    },
  };
}
