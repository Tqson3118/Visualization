/**
 * useLenis — smooth scroll toàn cục (Phase 2a G — POLISH).
 *
 * - Singleton: 1 instance Lenis cho cả app (autoRaf → tự chạy raf loop).
 * - `anchors: true`  → hỗ trợ anchor link (#section) tự cuộn mượt.
 * - `allowNestedScroll: true` → không phá scroll bên trong container con
 *   (canvas simulator, code runner…) — Lenis 1.3.26.
 * - `respectReducedMotion: true` (mặc định của Lenis) → nếu user bật
 *   prefers-reduced-motion, lerp = 1 (cuộn bám input 1:1) + scrollTo tức thì.
 * - `autoToggle: true` → tự stop khi trang không tràn (không cần smooth).
 *
 * Dùng: gọi 1 lần trong App.vue setup (app đời). Component khác chỉ cần
 * `scrollTo()`/`scrollToTop()` — instance được chia sẻ qua module singleton.
 */
import type { LenisOptions, ScrollToOptions } from 'lenis';
import Lenis from 'lenis';
import { onScopeDispose } from 'vue';

let instance: Lenis | null = null;

export interface UseLenisReturn {
  /** Instance Lenis (null khi SSR / chưa tạo). */
  lenis: Lenis | null;
  /** Cuộn tới target (số px | '#anchor' | HTMLElement). */
  scrollTo: (target: number | string | HTMLElement, options?: ScrollToOptions) => void;
  /** Cuộn về đầu trang. immediate = nhảy ngay (mặc định smooth). */
  scrollToTop: (immediate?: boolean) => void;
  /** Tạm dừng / chạy lại smooth scroll. */
  stop: () => void;
  start: () => void;
  /** Huỷ instance (chỉ dùng khi app unmount). */
  destroy: () => void;
}

/** Lấy instance Lenis toàn cục (cho code ngoài composable). */
export function getLenis(): Lenis | null {
  return instance;
}

/**
 * Tạo (nếu chưa có) và trả về instance Lenis dùng chung.
 * options chỉ áp dụng lần tạo đầu tiên (singleton).
 */
export function useLenis(options?: LenisOptions): UseLenisReturn {
  if (typeof window !== 'undefined' && !instance) {
    instance = new Lenis({
      autoRaf: true,
      anchors: true,
      allowNestedScroll: true,
      autoToggle: true,
      stopInertiaOnNavigate: true,
      respectReducedMotion: true,
      ...options,
    });
  }

  onScopeDispose(() => {
    if (instance) {
      instance.destroy();
      instance = null;
    }
  });

  return {
    lenis: instance,
    scrollTo(target, opts) {
      instance?.scrollTo(target, opts);
    },
    scrollToTop(immediate = false) {
      instance?.scrollTo(0, { immediate });
    },
    stop() {
      instance?.stop();
    },
    start() {
      instance?.start();
    },
    destroy() {
      if (instance) {
        instance.destroy();
        instance = null;
      }
    },
  };
}

export default useLenis;
