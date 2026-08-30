import { ref } from 'vue';
import { useUiStore } from '@/stores/ui';

export function useRejectModal() {
  const ui = useUiStore();
  const isOpen = ref(false);
  const targetTitle = ref('');
  const reason = ref('');
  const loading = ref(false);
  const targetData = ref<any>(null);

  let confirmCallback: ((reason: string, data?: any) => Promise<void> | void) | null = null;

  function open(
    title: string,
    onConfirm: (reason: string, data?: any) => Promise<void> | void,
    data?: any,
    initialReason = '',
  ): void {
    targetTitle.value = title;
    reason.value = initialReason;
    targetData.value = data;
    confirmCallback = onConfirm;
    isOpen.value = true;
    loading.value = false;
  }

  function close(): void {
    isOpen.value = false;
    reason.value = '';
    targetTitle.value = '';
    targetData.value = null;
    confirmCallback = null;
    loading.value = false;
  }

  async function submit(): Promise<void> {
    const trimmed = reason.value.trim();
    if (!trimmed) {
      ui.showToast('Vui lòng nhập lý do từ chối để tác giả có thể sửa đổi.', 'warning');
      return;
    }
    if (!confirmCallback) {
      close();
      return;
    }

    loading.value = true;
    try {
      await confirmCallback(trimmed, targetData.value);
      close();
    } catch {
      // Error handling is expected within callback or ui.showToast
    } finally {
      loading.value = false;
    }
  }

  return {
    isOpen,
    targetTitle,
    targetData,
    reason,
    loading,
    open,
    close,
    submit,
  };
}
