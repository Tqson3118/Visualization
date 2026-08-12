import { ref } from 'vue';
import { defineStore } from 'pinia';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

export interface Toast {
  id: number;
  type: ToastType;
  message: string;
}

export interface ModalState {
  open: boolean;
  kind: string | null;
  payload: unknown;
  resolve: ((value: boolean) => void) | null;
}

/** Store ui theo SDD §3.2 — toast, modal, sidebar, theme */
export const useUiStore = defineStore('ui', () => {
  const toasts = ref<Toast[]>([]);
  const modalState = ref<ModalState>({ open: false, kind: null, payload: null, resolve: null });
  const sidebarOpen = ref(false);
  const theme = ref<'light' | 'dark'>('light');

  let toastSeq = 0;

  function showToast(message: string, type: ToastType = 'info', durationMs = 4000): number {
    const id = ++toastSeq;
    toasts.value.push({ id, type, message });
    if (durationMs > 0) {
      window.setTimeout(() => dismissToast(id), durationMs);
    }
    return id;
  }

  function dismissToast(id: number): void {
    toasts.value = toasts.value.filter((toast) => toast.id !== id);
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
    toasts,
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
