import { ref, watch } from 'vue';
import { defineStore } from 'pinia';

import { toast } from '@/lib/toast';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface ModalState {
  open: boolean;
  kind: string | null;
  payload: unknown;
  resolve: ((value: boolean) => void) | null;
}

const THEME_KEY = 'dsa.theme';

function readStoredTheme(): 'light' | 'dark' {
  try {
    const stored = localStorage.getItem(THEME_KEY);
    if (stored === 'light' || stored === 'dark') return stored;
  } catch {
    /* localStorage unavailable — fallback dark */
  }
  return 'dark';
}

/** Store ui theo SDD §3.2 — modal, sidebar, theme. Toast: G-F1b delegate sang vue-sonner. */
export const useUiStore = defineStore('ui', () => {
  const modalState = ref<ModalState>({ open: false, kind: null, payload: null, resolve: null });
  const sidebarOpen = ref(false);
  const theme = ref<'light' | 'dark'>(readStoredTheme());

  /** Áp class .dark + color-scheme lên <html> mỗi khi theme đổi (dark mặc định — ghi đè light cũ). */
  watch(
    theme,
    (value) => {
      document.documentElement.classList.toggle('dark', value === 'dark');
      document.documentElement.style.colorScheme = value;
      try {
        localStorage.setItem(THEME_KEY, value);
      } catch {
        /* ignore */
      }
    },
    { immediate: true },
  );

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
