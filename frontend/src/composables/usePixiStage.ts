// src/composables/usePixiStage.ts
// Lifecycle manager for PixiJS (WebGL) Application stage
// Supports automatic resizing, gsap.ticker / app.ticker 60fps synchronization,
// container resolution adaptation, theme backgrounds, and graceful cleanup.

import { shallowRef, ref, onUnmounted } from 'vue';
import { Application, Container } from 'pixi.js';
import gsap from 'gsap';

export interface PixiStageOptions {
  backgroundAlpha?: number;
  backgroundColor?: number | string;
  antialias?: boolean;
  useGsapTicker?: boolean;
  onTick?: (deltaMs: number) => void;
  onResize?: (width: number, height: number) => void;
}

export function usePixiStage() {
  const app = shallowRef<Application | null>(null);
  const stage = shallowRef<Container | null>(null);
  const isReady = ref(false);
  const stageWidth = ref(800);
  const stageHeight = ref(420);
  const dpr = ref(1);

  let resizeObserver: ResizeObserver | null = null;
  let boundContainer: HTMLElement | null = null;
  let gsapTickerFn: (() => void) | null = null;
  let customTickCallback: ((deltaMs: number) => void) | null = null;
  let customResizeCallback: ((width: number, height: number) => void) | null = null;

  async function mount(
    container: HTMLElement,
    existingCanvas?: HTMLCanvasElement | null,
    options: PixiStageOptions = {},
  ): Promise<Application | null> {
    if (typeof window === 'undefined') return null;

    // Dispose any previous instance
    dispose();

    boundContainer = container;
    customTickCallback = options.onTick ?? null;
    customResizeCallback = options.onResize ?? null;

    dpr.value = Math.min(2, window.devicePixelRatio || 1);
    const rect = container.getBoundingClientRect();
    const w = Math.max(200, Math.round(rect.width || 800));
    const h = Math.max(200, Math.round(rect.height || 420));
    stageWidth.value = w;
    stageHeight.value = h;

    // If running in an environment without canvas/WebGL context support (e.g. jsdom in tests)
    const testCanvas = existingCanvas || (typeof document !== 'undefined' ? document.createElement('canvas') : null);
    if (!testCanvas || typeof testCanvas.getContext !== 'function') {
      return null;
    }
    const testCtx = testCanvas.getContext('webgl2') || testCanvas.getContext('webgl') || testCanvas.getContext('2d');
    if (!testCtx) {
      return null;
    }

    const pixiApp = new Application();

    try {
      await pixiApp.init({
        canvas: existingCanvas ?? undefined,
        width: w,
        height: h,
        resolution: dpr.value,
        autoDensity: true,
        backgroundAlpha: options.backgroundAlpha ?? 1,
        backgroundColor: options.backgroundColor ?? 0x090d16,
        antialias: options.antialias ?? true,
        powerPreference: 'high-performance',
      });

      // Force canvas styles to 100% so inline styles from Pixi v8 do not shrink the element
      const targetCanvas = existingCanvas || pixiApp.canvas;
      if (targetCanvas) {
        targetCanvas.style.width = '100%';
        targetCanvas.style.height = '100%';
        targetCanvas.style.display = 'block';
        if (!existingCanvas) {
          container.appendChild(targetCanvas);
        }
      }

      app.value = pixiApp;
      stage.value = pixiApp.stage;
      isReady.value = true;

      // Setup 60fps ticker loop with gsap ticker integration
      if (options.useGsapTicker !== false) {
        gsapTickerFn = () => {
          if (!isReady.value || !app.value || !app.value.renderer || (app.value.renderer as any).destroyed) return;
          const delta = pixiApp.ticker.deltaMS;
          if (customTickCallback) {
            customTickCallback(delta);
          }
          // Pixi auto-renders via its own ticker or manual render
        };
        gsap.ticker.add(gsapTickerFn);
      } else {
        pixiApp.ticker.add((ticker) => {
          if (!isReady.value || !app.value || !app.value.renderer || (app.value.renderer as any).destroyed) return;
          if (customTickCallback) {
            customTickCallback(ticker.deltaMS);
          }
        });
      }

      // Container ResizeObserver
      resizeObserver = new ResizeObserver((entries) => {
        if (!isReady.value || !app.value) return;
        for (const entry of entries) {
          const cw = Math.round(entry.contentRect.width);
          const ch = Math.round(entry.contentRect.height);
          if (cw > 0 && ch > 0) {
            resize(cw, ch);
          }
        }
      });
      resizeObserver.observe(container);

      return pixiApp;
    } catch (err) {
      console.warn('[usePixiStage] WebGL init fallback:', err);
      return null;
    }
  }

  function resize(w: number, h: number): void {
    if (!app.value || !isReady.value || !app.value.renderer || (app.value.renderer as any).destroyed) return;

    stageWidth.value = w;
    stageHeight.value = h;
    dpr.value = Math.min(2, window.devicePixelRatio || 1);

    try {
      app.value.renderer.resize(w, h);
      if (app.value.canvas) {
        app.value.canvas.style.width = '100%';
        app.value.canvas.style.height = '100%';
        app.value.canvas.style.display = 'block';
      }
      if (customResizeCallback) {
        customResizeCallback(w, h);
      }
    } catch (e) {
      console.error('[usePixiStage] resize error:', e);
    }
  }

  function dispose(): void {
    isReady.value = false;

    if (resizeObserver) {
      resizeObserver.disconnect();
      resizeObserver = null;
    }

    if (gsapTickerFn) {
      gsap.ticker.remove(gsapTickerFn);
      gsapTickerFn = null;
    }

    if (app.value) {
      try {
        const pApp = app.value;
        // B1 — guard chống destroy 2 lần: nếu renderer đã destroyed thì chỉ null refs
        if ((pApp.renderer as any)?.destroyed) {
          app.value = null;
          stage.value = null;
          return;
        }
        pApp.ticker.stop();
        // v8: destroy(removeView?, options?) — removeView=true gỡ canvas khỏi DOM.
        // texture default=false (painter tự destroy texture trước) — không dùng texture:true.
        pApp.destroy(true, { children: true });
      } catch {}
      app.value = null;
      stage.value = null;
    }

    boundContainer = null;
  }

  onUnmounted(() => {
    dispose();
  });

  return {
    app,
    stage,
    isReady,
    stageWidth,
    stageHeight,
    dpr,
    mount,
    resize,
    dispose,
  };
}
