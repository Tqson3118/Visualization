import { onUnmounted, ref } from 'vue';

/**
 * useDebounce — SDD §3.6: trễ hành động (tìm kiếm FR-2.5).
 * Trả về hàm gọi debounced + cancel/flush; tự dọn khi unmount.
 */
export function useDebounce<Args extends unknown[], R>(
  fn: (...args: Args) => R,
  delay = 300,
) {
  const timer = ref<ReturnType<typeof setTimeout> | null>(null);

  function call(...args: Args): void {
    cancel();
    timer.value = setTimeout(() => {
      timer.value = null;
      fn(...args);
    }, delay);
  }

  function cancel(): void {
    if (timer.value !== null) {
      clearTimeout(timer.value);
      timer.value = null;
    }
  }

  /** Chạy ngay với args mới nhất (nếu có) — không dùng cho debounce thuần */
  function flush(...args: Args): R {
    cancel();
    return fn(...args);
  }

  onUnmounted(cancel);

  return { call, cancel, flush };
}
