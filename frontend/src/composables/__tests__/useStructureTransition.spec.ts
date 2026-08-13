// composables/__tests__/useStructureTransition.spec.ts — hiệu ứng transition push/pop stack/queue
//
// Dùng engine GIẢ (capture callback) — không cần rAF; test gọi callback(ms) thủ công để
// tiến animation theo thời gian giả lập. Kiểm tra: frame đầu/mid/cuối, prefers-reduced-motion,
// cancel(), không mutate structure gốc.

import { afterEach, describe, expect, test, vi } from 'vitest';

import type { Element, Structure } from '@/engines/core/types';
import { CoreAnimationEngine } from '@/engines/renderers/coreAnimationEngine';
import { useStructureTransition } from '../useStructureTransition';

/** Engine giả: capture registerRender/unregisterRender để test gọi callback thủ công. */
function createFakeEngine(): {
  engine: CoreAnimationEngine;
  callbacks: Array<(dt: number) => void>;
  registered: Array<(dt: number) => void>;
  unregistered: Array<(dt: number) => void>;
} {
  const callbacks: Array<(dt: number) => void> = [];
  const registered: Array<(dt: number) => void> = [];
  const unregistered: Array<(dt: number) => void> = [];
  const engine = {
    registerRender: (cb: (dt: number) => void) => {
      callbacks.push(cb);
      registered.push(cb);
    },
    unregisterRender: (cb: (dt: number) => void) => {
      const i = callbacks.indexOf(cb);
      if (i >= 0) callbacks.splice(i, 1);
      unregistered.push(cb);
    },
  } as unknown as CoreAnimationEngine;
  return { engine, callbacks, registered, unregistered };
}

function stackWith(ids: number[]): Structure {
  const elements: Element[] = ids.map((i) => ({ id: `cell:${i}`, label: String(i), status: 'default', group: 'stack' }));
  return { kind: 'stack', elements, links: [] };
}

function queueWith(ids: number[]): Structure {
  const elements: Element[] = ids.map((i) => ({ id: `cell:${i}`, label: String(i), status: 'default', group: 'queue' }));
  return { kind: 'queue', elements, links: [] };
}

const originalMatchMedia = typeof window !== 'undefined' ? window.matchMedia : undefined;

afterEach(() => {
  Object.defineProperty(window, 'matchMedia', { configurable: true, value: originalMatchMedia });
  vi.restoreAllMocks();
});

describe('useStructureTransition', () => {
  test('push stack: frame đầu/mid có animY ≠ 0, frame cuối = structure THẬT, ≥2 frame trung gian', () => {
    const { engine, callbacks } = createFakeEngine();
    const renderFrame = vi.fn();
    const transition = useStructureTransition({ engine, durationMs: 200 });
    const prev = stackWith([0, 1, 2]);
    const next = stackWith([0, 1, 2, 3]);

    transition.update(prev, next, renderFrame);

    expect(callbacks.length).toBe(1); // đã đăng ký engine
    expect(transition.isAnimating()).toBe(true);

    // Frame đầu (vẽ đồng bộ): cell:3 (added) đang trượt từ trên xuống → animY ≠ 0
    const first = renderFrame.mock.calls[0][0] as Structure;
    const added = first.elements.find((el) => el.id === 'cell:3');
    expect(added).toBeDefined();
    expect(added?.meta?.animY).not.toBe(0);
    expect(added?.meta?.animAlpha).toBeUndefined(); // added chỉ trượt, không fade

    // Frame giữa: vẫn chưa về 0 (ease chưa kết thúc)
    callbacks[0](100);
    const mid = renderFrame.mock.calls[1][0] as Structure;
    expect(mid.elements.find((el) => el.id === 'cell:3')?.meta?.animY).not.toBe(0);

    // Frame cuối: structure THẬT, không meta anim
    callbacks[0](100);
    const last = renderFrame.mock.calls[2][0] as Structure;
    expect(last).toBe(next);
    expect(last.elements.find((el) => el.id === 'cell:3')?.meta?.animY).toBeUndefined();
    expect(transition.isAnimating()).toBe(false);
    expect(renderFrame.mock.calls.length).toBeGreaterThanOrEqual(3); // ≥2 frame trung gian
  });

  test('pop queue: frame đầu có cell:0 (removed) với animAlpha < 1 đang fade + common cell trượt trái; frame cuối không còn cell:0', () => {
    const { engine, callbacks } = createFakeEngine();
    const renderFrame = vi.fn();
    const transition = useStructureTransition({ engine, durationMs: 200 });
    const prev = queueWith([0, 1, 2, 3]);
    const next = queueWith([1, 2, 3]);

    transition.update(prev, next, renderFrame);

    const first = renderFrame.mock.calls[0][0] as Structure;
    const removed = first.elements.find((el) => el.id === 'cell:0');
    expect(removed).toBeDefined(); // removed vẫn nằm trong copy để renderer vẽ bay ra
    expect(Number(removed?.meta?.animAlpha)).toBeLessThan(1); // đang fade
    expect(Number(removed?.meta?.animAlpha)).toBeGreaterThan(0);
    expect(Number(removed?.meta?.animX)).not.toBe(0); // bay ra trái
    // Common cell:1 lệch trái 1 slot so với prev
    expect(Number(first.elements.find((el) => el.id === 'cell:1')?.meta?.animX)).not.toBe(0);

    callbacks[0](200);
    const last = renderFrame.mock.calls[1][0] as Structure;
    expect(last).toBe(next);
    expect(last.elements.find((el) => el.id === 'cell:0')).toBeUndefined();
  });

  test('prefers-reduced-motion: vẽ thẳng NGAY, không đăng ký engine', () => {
    Object.defineProperty(window, 'matchMedia', {
      configurable: true,
      value: vi.fn(() => ({ matches: true })),
    });
    const { engine, registered } = createFakeEngine();
    const renderFrame = vi.fn();
    const transition = useStructureTransition({ engine, durationMs: 200 });
    const prev = stackWith([0, 1, 2]);
    const next = stackWith([0, 1, 2, 3]);

    transition.update(prev, next, renderFrame);

    expect(renderFrame).toHaveBeenCalledTimes(1); // 0 frame trung gian
    expect(renderFrame.mock.calls[0][0]).toBe(next);
    expect(registered.length).toBe(0);
    expect(transition.isAnimating()).toBe(false);
  });

  test('cancel(): unregister callback + callback cũ không vẽ thêm frame', () => {
    const { engine, callbacks, unregistered } = createFakeEngine();
    const renderFrame = vi.fn();
    const transition = useStructureTransition({ engine, durationMs: 200 });
    transition.update(stackWith([0, 1, 2]), stackWith([0, 1, 2, 3]), renderFrame);
    const cb = callbacks[0];
    const drawnBefore = renderFrame.mock.calls.length;
    expect(drawnBefore).toBeGreaterThan(0);

    transition.cancel();
    expect(unregistered).toContain(cb);
    expect(transition.isAnimating()).toBe(false);

    cb(100); // callback cũ bị bỏ qua sau cancel
    expect(renderFrame.mock.calls.length).toBe(drawnBefore);
  });

  test('prev null → vẽ ngay, không animate', () => {
    const { engine, registered } = createFakeEngine();
    const renderFrame = vi.fn();
    const transition = useStructureTransition({ engine });
    const next = stackWith([0, 1, 2]);

    transition.update(null, next, renderFrame);

    expect(renderFrame).toHaveBeenCalledTimes(1);
    expect(renderFrame.mock.calls[0][0]).toBe(next);
    expect(registered.length).toBe(0);
  });

  test('đổi kind (stack → array) → vẽ ngay, không animate', () => {
    const { engine, registered } = createFakeEngine();
    const renderFrame = vi.fn();
    const transition = useStructureTransition({ engine });
    const array: Structure = {
      kind: 'array',
      elements: [{ id: 'cell:0', label: '5', status: 'default' }],
      links: [],
    };

    transition.update(stackWith([0, 1]), array, renderFrame);

    expect(renderFrame).toHaveBeenCalledTimes(1);
    expect(renderFrame.mock.calls[0][0]).toBe(array);
    expect(registered.length).toBe(0);
  });

  test('cùng tập element (không added/removed) → vẽ ngay, không animate', () => {
    const { engine, registered } = createFakeEngine();
    const renderFrame = vi.fn();
    const transition = useStructureTransition({ engine });
    const same = stackWith([0, 1, 2]);

    transition.update(same, same, renderFrame);

    expect(renderFrame).toHaveBeenCalledTimes(1);
    expect(renderFrame.mock.calls[0][0]).toBe(same);
    expect(registered.length).toBe(0);
  });

  test('KHÔNG mutate structure gốc sau cả quá trình animation', () => {
    const { engine, callbacks } = createFakeEngine();
    const renderFrame = vi.fn();
    const transition = useStructureTransition({ engine, durationMs: 200 });
    const prev = queueWith([0, 1, 2, 3]);
    const next = queueWith([1, 2, 3]);
    const prevJson = JSON.stringify(prev);
    const nextJson = JSON.stringify(next);

    transition.update(prev, next, renderFrame);
    callbacks[0](100);
    callbacks[0](100);

    expect(JSON.stringify(prev)).toBe(prevJson);
    expect(JSON.stringify(next)).toBe(nextJson);
  });
});
