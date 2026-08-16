import { ref } from 'vue';
import { defineStore } from 'pinia';

import { toast } from '@/lib/toast';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface ModalState {
  open: boolean;
  kind: string | null;
  payload: unknown;
  resolve: ((value: boolean) => void) | null;
}

/** Store ui theo SDD §3.2 — modal, sidebar, theme. Toast: G-F1b delegate sang vue-sonner. */
export const useUiStore = defineStore('ui', () => {
  const modalState = ref<ModalState>({ open: false, kind: null, payload: null, resolve: null });
  const sidebarOpen = ref(false);

  // FIX A1 — check giao diện khởi tạo bền vững (USER_GUIDE §3.15):
  // ưu tiên localStorage 'dsa_theme' nếu hợp lệ; không → prefers-color-scheme;
  // toggleTheme() ghi localStorage để giữ lựa chọn qua các phiên.
  const THEME_STORAGE_KEY = 'dsa_theme';

  function resolveInitialTheme(): 'light' | 'dark' {
    if (typeof window !== 'undefined') {
      try {
        const stored = window.localStorage.getItem(THEME_STORAGE_KEY);
        if (stored === 'light' || stored === 'dark') return stored;
      } catch {
        /* localStorage không khả dụng — fallback xuống prefers-color-scheme */
      }
      if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        return 'dark';
      }
    }
    return 'light';
  }

  const theme = ref<'light' | 'dark'>(resolveInitialTheme());

  /** showToast — giữ API cũ (ui.showToast(msg, type)) nhưng render bằng vue-sonner. */
  function showToast(message: string, type: ToastType = 'info', _durationMs = 4000): number {
    const id = toast.show(message, type);
    return typeof id === 'number' ? id : 0;
  }

  function dismissToast(id: number): void {
    toast.dismiss(id);
  }

  function openModal(kind: string, payload: unknown = null, resolve: ((value: boolean) => void) | null = null): void {
    modalState.value = { open: true, kind, payload, resolve };
  }

  function closeModal(result = false): void {
    const resolve = modalState.value.resolve;
    modalState.value = { open: false, kind: null, payload: null, resolve: null };
    if (resolve) resolve(result);
  }

  function toggleTheme(): void {
    theme.value = theme.value === 'light' ? 'dark' : 'light';
    if (typeof window !== 'undefined') {
      try {
        window.localStorage.setItem(THEME_STORAGE_KEY, theme.value);
      } catch {
        /* bỏ qua nếu localStorage không khả dụng */
      }
    }
  }

  return {
    modalState,
    sidebarOpen,
    theme,
    showToast,
    dismissToast,
    openModal,
    closeModal,
    toggleTheme,
  };
});
