import { computed, onBeforeUnmount, ref, watch } from 'vue';

/**
 * useCountdown — đếm ngược phút:giây cho bài kiểm tra/bài tập (UI-PREMIUM Phase 1B).
 * - `durationMinutes`: số phút cố định hoặc getter (VD () => exercise?.durationMinutes ?? 0)
 *   để đồng bộ khi exercise nạp xong.
 * - `progress`: 0..100 = phần trăm thời gian CÒN LẠI (nuôi ProgressRing).
 * - `formatted`: chuỗi mono `mm:ss`.
 * - Chỉ mang tính hiển thị — KHÔNG tự nộp bài, không đổi logic submit.
 * - Không phụ thuộc animation (đồng hồ vẫn chạy kể cả prefers-reduced-motion).
 */
export function useCountdown(durationMinutes: number | (() => number)) {
  const getMinutes = typeof durationMinutes === 'function' ? durationMinutes : () => durationMinutes;

  const totalSeconds = computed(() => Math.max(0, Math.round(getMinutes() * 60)));
  const remaining = ref(0);
  const running = ref(false);
  let timer: ReturnType<typeof setInterval> | null = null;

  // Đồng bộ khi duration thay đổi (exercise nạp xong): chưa chạy thì reset về full,
  // đang chạy thì chỉ thu hẹp về mức tối đa mới.
  watch(
    totalSeconds,
    (total) => {
      if (!running.value) remaining.value = total;
      else remaining.value = Math.min(remaining.value, total);
    },
    { immediate: true },
  );

  function stop(): void {
    if (timer) clearInterval(timer);
    timer = null;
    running.value = false;
  }

  function start(): void {
    if (timer || remaining.value <= 0) return;
    running.value = true;
    timer = setInterval(() => {
      remaining.value = Math.max(0, remaining.value - 1);
      if (remaining.value <= 0) stop();
    }, 1000);
  }

  onBeforeUnmount(stop);

  const progress = computed(() =>
    totalSeconds.value === 0 ? 0 : Math.round((remaining.value / totalSeconds.value) * 100),
  );

  const formatted = computed(() => {
    const m = Math.floor(remaining.value / 60);
    const s = remaining.value % 60;
    return `${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`;
  });

  const isDone = computed(() => remaining.value <= 0);

  return { remaining, totalSeconds, progress, formatted, isDone, running, start, stop };
}
