import { onUnmounted, ref, watch, type MaybeRef } from 'vue';

/**
 * useInterval — SDD §3.6: setInterval tự dọn khi unmount (đếm ngược bài tập, Màn 28).
 * Đổi ms → tự restart; trả về start/stop/active.
 */
export function useInterval(fn: () => void, ms: MaybeRef<number>) {
  const delay = typeof ms === 'number' ? ref(ms) : ms;
  const active = ref(false);
  let timer: ReturnType<typeof setInterval> | null = null;

  function clear(): void {
    if (timer !== null) {
      clearInterval(timer);
      timer = null;
    }
    active.value = false;
  }

  function start(): void {
    clear();
    timer = setInterval(fn, delay.value);
    active.value = true;
  }

  function stop(): void {
    clear();
  }

  watch(delay, () => {
    if (active.value) start();
  });

  onUnmounted(clear);

  return { start, stop, active };
}
