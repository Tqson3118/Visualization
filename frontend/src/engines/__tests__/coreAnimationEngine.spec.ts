// engines/__tests__/coreAnimationEngine.spec.ts
// Bê từ source/VisualizationDSA3/frontend/src/core/__tests__/CoreAnimationEngine.spec.ts (V3),
// chỉnh bỏ phần CompilerStepExecutor (đã tách sang stepExecutor.spec.ts) và thay `any` bằng
// kiểu rõ ràng.

import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

import { CoreAnimationEngine } from '../renderers/coreAnimationEngine';

let originalRAF: typeof requestAnimationFrame;
let originalCAF: typeof cancelAnimationFrame;

beforeEach(() => {
  originalRAF = globalThis.requestAnimationFrame;
  originalCAF = globalThis.cancelAnimationFrame;
  globalThis.requestAnimationFrame = vi.fn((cb: FrameRequestCallback) => setTimeout(cb, 16.67) as unknown as number);
  globalThis.cancelAnimationFrame = vi.fn((id: number) => clearTimeout(id));
  vi.useFakeTimers();
});

afterEach(() => {
  globalThis.requestAnimationFrame = originalRAF;
  globalThis.cancelAnimationFrame = originalCAF;
  vi.restoreAllMocks();
});

describe('CoreAnimationEngine', () => {
  test('Phép toán Lerp phải hoạt động chính xác trong giới hạn 0-1', () => {
    expect(CoreAnimationEngine.lerp(10, 20, 0.5)).toBe(15);
    expect(CoreAnimationEngine.lerp(10, 20, -1)).toBe(10);
    expect(CoreAnimationEngine.lerp(10, 20, 2)).toBe(20);
  });

  test('Phép toán lerpPoint phải hoạt động chính xác cho tọa độ 2D', () => {
    const p1 = { x: 0, y: 0 };
    const p2 = { x: 100, y: 200 };
    expect(CoreAnimationEngine.lerpPoint(p1, p2, 0.5)).toEqual({ x: 50, y: 100 });
    expect(CoreAnimationEngine.lerpPoint(p1, p2, -0.1)).toEqual({ x: 0, y: 0 });
    expect(CoreAnimationEngine.lerpPoint(p1, p2, 1.5)).toEqual({ x: 100, y: 200 });
  });

  test('Đăng ký, hủy đăng ký callback và chạy loop rAF', () => {
    const engine = new CoreAnimationEngine();
    const mockCallback = vi.fn();

    const captured: { cb: FrameRequestCallback | null } = { cb: null };
    const mockRaf = vi.spyOn(globalThis, 'requestAnimationFrame').mockImplementation((cb) => {
      captured.cb = cb;
      return 123;
    });

    engine.registerRender(mockCallback);
    expect(mockRaf).toHaveBeenCalled();
    expect(captured.cb).not.toBeNull();

    if (captured.cb) {
      captured.cb(performance.now() + 16.67);
    }
    expect(mockCallback).toHaveBeenCalled();

    engine.unregisterRender(mockCallback);
    engine.destroy();
  });

  test('Clamping DeltaTime không vượt quá 32ms ngay cả khi trễ lớn', () => {
    const engine = new CoreAnimationEngine();
    let recordedDelta = 0;
    const callback = (dt: number) => {
      recordedDelta = dt;
    };

    const captured: { cb: FrameRequestCallback | null } = { cb: null };
    vi.spyOn(globalThis, 'requestAnimationFrame').mockImplementation((cb) => {
      captured.cb = cb;
      return 456;
    });

    engine.registerRender(callback);

    if (captured.cb) {
      captured.cb(1000);
    }

    if (captured.cb) {
      captured.cb(2000);
    }

    expect(recordedDelta).toBe(32);

    engine.destroy();
  });

  test('Hủy đăng ký một callback chưa từng đăng ký', () => {
    const engine = new CoreAnimationEngine();
    const mockCallback = vi.fn();

    expect(() => engine.unregisterRender(mockCallback)).not.toThrow();

    const activeCb = vi.fn();
    engine.registerRender(activeCb);
    expect(() => engine.unregisterRender(mockCallback)).not.toThrow();

    engine.destroy();
  });

  test('Gọi destroy nhiều lần phải an toàn không gây lỗi', () => {
    const engine = new CoreAnimationEngine();
    engine.destroy();
    expect(() => engine.destroy()).not.toThrow();
  });
});
