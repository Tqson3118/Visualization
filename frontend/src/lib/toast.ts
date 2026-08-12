import { toast as sonner } from 'vue-sonner';

/** Toast type giữ nguyên API cũ (useToast/uiStore.showToast). */
export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface ToastHelper {
  show(message: string, type?: ToastType): string | number;
  success(message: string): string | number;
  error(message: string): string | number;
  warning(message: string): string | number;
  info(message: string): string | number;
  dismiss(id?: string | number): string | number | undefined;
}

/**
 * toast — helper G-F1b: thay cơ chế toast tự xây bằng vue-sonner.
 * Giữ API cũ (show/success/error/warning/info/dismiss) để call site cũ không vỡ.
 */
export const toast: ToastHelper = {
  show(message, type = 'info') {
    switch (type) {
      case 'success':
        return sonner.success(message);
      case 'error':
        return sonner.error(message);
      case 'warning':
        return sonner.warning(message);
      default:
        return sonner.info(message);
    }
  },
  success: (message) => sonner.success(message),
  error: (message) => sonner.error(message),
  warning: (message) => sonner.warning(message),
  info: (message) => sonner.info(message),
  dismiss: (id) => (id === undefined ? sonner.dismiss() : sonner.dismiss(id)),
};
