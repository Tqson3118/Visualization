import { createPinia, setActivePinia } from 'pinia';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import '@/engines/catalog'; // side-effect: đăng ký 44 generator vào registry (GP-T4 cần sort.bubble)
import { useSimulationStore } from './simulation';

// GP-T4 — breakpoint theo dòng template (pseudocode):
// toggleBreakpoint + auto-pause khi bước chạy có pseudocodeLine ∈ breakpoints.
// Dùng generator THẬT sort.bubble (engine KHÔNG mock — chạy client-side).
describe('simulation store — breakpoints (GP-T4)', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('toggleBreakpoint thêm/xóa dòng trong tập breakpoints', () => {
    const store = useSimulationStore();
    store.toggleBreakpoint(5);
    expect(store.breakpoints.has(5)).toBe(true);
    expect(store.breakpoints.size).toBe(1);
    // toggle lần 2 → xóa
    store.toggleBreakpoint(5);
    expect(store.breakpoints.has(5)).toBe(false);
    expect(store.breakpoints.size).toBe(0);
  });

  it('play loop tự dừng tại bước có pseudocodeLine = breakpoint (dòng 5)', async () => {
    vi.useFakeTimers();
    const store = useSimulationStore();
    await store.loadSim('sort.bubble');

    // Smoke tương ứng: đặt breakpoint dòng 5 → play → dừng tại bước đầu có dòng 5
    const firstHit = store.steps.findIndex((s) => s.pseudocodeLine === 5);
    expect(firstHit).toBeGreaterThan(0);

    store.toggleBreakpoint(5);
    store.play();
    vi.advanceTimersByTime(5_000);

    expect(store.status).toBe('paused');
    expect(store.breakpointHit).toBe(5);
    expect(store.currentIndex).toBe(firstHit);
    expect(store.currentStep?.pseudocodeLine).toBe(5);
    // Timer đã dừng — không tiến thêm dù tiếp tục trôi thời gian
    vi.advanceTimersByTime(5_000);
    expect(store.currentIndex).toBe(firstHit);
    expect(store.status).toBe('paused');
  });

  it('stepForward cũng dừng khi bước tới có breakpoint', async () => {
    const store = useSimulationStore();
    await store.loadSim('sort.bubble');

    const line = store.steps[1].pseudocodeLine;
    store.toggleBreakpoint(line);
    store.stepForward();

    expect(store.status).toBe('paused');
    expect(store.breakpointHit).toBe(line);
    expect(store.currentIndex).toBe(1);
  });

  it('reset xóa breakpointHit nhưng giữ tập breakpoints', async () => {
    const store = useSimulationStore();
    await store.loadSim('sort.bubble');

    const line = store.steps[1].pseudocodeLine;
    store.toggleBreakpoint(line);
    store.stepForward();
    expect(store.breakpointHit).toBe(line);

    store.reset();
    expect(store.breakpointHit).toBeNull();
    expect(store.breakpoints.has(line)).toBe(true);
  });
});
