import type { Component } from 'vue';
import { toast as sonner } from 'vue-sonner';

/** Toast type giữ nguyên API cũ (useToast/uiStore.showToast). */
export type ToastType = 'success' | 'error' | 'warning' | 'info';

/** Options cho toast.promise — mô tả các trạng thái của promise. */
export interface PromiseToastOptions<T> {
  loading?: string;
  success?: string | ((data: T) => string);
  error?: string | ((error: unknown) => string);
  finally?: () => void;
}

/** Return type của vue-sonner toast.promise (có unwrap để chờ kết quả). */
export type PromiseToastReturn = ReturnType<typeof sonner.promise>;

export interface ToastHelper {
  show(message: string, type?: ToastType): string | number;
  success(message: string): string | number;
  error(message: string): string | number;
  warning(message: string): string | number;
  info(message: string): string | number;
  /** G-F2a: toast loading (có spinner) — dùng cho thao tác nền. */
  loading(message: string): string | number;
  /** G-F2a: toast theo promise — tự loading → success/error. */
  promise<T>(
    promise: Promise<T> | (() => Promise<T>),
    options?: PromiseToastOptions<T>,
  ): PromiseToastReturn;
  /** G-F2a: toast custom (render component tuỳ ý — VD progress bar). */
  custom(component: Component, props?: Record<string, unknown>): string | number;
  dismiss(id?: string | number): string | number | undefined;
}

/**
 * toast — helper G-F1b (mở rộng G-F2a): thay cơ chế toast tự xây bằng vue-sonner.
 * Giữ API cũ (show/success/error/warning/info/dismiss) để call site cũ không vỡ;
 * bổ sung loading/promise/custom cho luồng async + progress.
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
  loading: (message) => sonner.loading(message),
  promise<T>(promise: Promise<T> | (() => Promise<T>), options?: PromiseToastOptions<T>) {
    return sonner.promise(promise, {
      loading: options?.loading ?? 'Đang xử lý…',
      success: options?.success,
      error: options?.error,
      finally: options?.finally,
    });
  },
  custom: (component, props) => sonner.custom(component, props),
  dismiss: (id) => (id === undefined ? sonner.dismiss() : sonner.dismiss(id)),
};
