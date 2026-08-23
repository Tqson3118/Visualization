import { getCurrentInstance, onMounted, onUnmounted, readonly, ref, type Ref } from 'vue';

/**
 * useScrollReveal — IntersectionObserver wrapper (UI-PREMIUM 0C).
 * Trả về `isVisible` reactive: true khi element vào viewport.
 * - once: chỉ reveal 1 lần (mặc định) — không lặp lại khi ra khỏi viewport.
 * - threshold/rootMargin cấu hình được.
 * - Tôn trọng prefers-reduced-motion: nếu bật → isVisible = true ngay (không cần animation).
 */
export function useScrollReveal(
  target: Ref<HTMLElement | null>,
  options: { threshold?: number; rootMargin?: string; once?: boolean } = {},
): { isVisible: Readonly<Ref<boolean>> } {
  const { threshold = 0.15, rootMargin = '0px 0px -40px 0px', once = true } = options;

  const isVisible = ref(false);

  let observer: IntersectionObserver | null = null;

  const reducedMotion =
    typeof window !== 'undefined' &&
    typeof window.matchMedia === 'function' &&
    window.matchMedia('(prefers-reduced-motion: reduce)').matches;

  function observe(): void {
    const el = target.value;
    if (!el || typeof IntersectionObserver === 'undefined') {
      isVisible.value = true;
      return;
    }

    observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (entry.isIntersecting) {
            isVisible.value = true;
            if (once) observer?.disconnect();
            return;
          }
        }
      },
      { threshold, rootMargin },
    );
    observer.observe(el);
  }

  if (getCurrentInstance()) {
    onMounted(() => {
      if (reducedMotion) {
        isVisible.value = true;
        return;
      }
      observe();
    });

    onUnmounted(() => {
      observer?.disconnect();
      observer = null;
    });
  }

  return { isVisible: readonly(isVisible) };
}
