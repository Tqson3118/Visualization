// engines/__tests__/compileWorker.spec.ts
// Test client wrapper compileWorker.ts (ADR-012): runMeasureInWorker đo trong Web Worker
// không chặn UI + tương thích ngược compileInWorker. jsdom không có Worker thật
// → mock global Worker bằng FakeWorker (trả kết quả theo kịch bản từng test).

import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { compileInWorker, disposeCompileWorker, runMeasureInWorker } from '../worker/compileWorker';

interface FakeRequest {
  requestId: number;
  kind?: string;
}

/**
 * Fake Worker cho test: nhận postMessage → gọi `respond` (nếu có) rồi trả kết quả
 * bất đồng bộ qua onmessage (giống worker thật). Nếu `respond` trả undefined
 * → không trả lời (dùng cho test timeout/kill-switch).
 */
class FakeWorker {
  static instances: FakeWorker[] = [];
  static respond: ((request: unknown) => unknown) | null = null;

  onmessage: ((event: MessageEvent<unknown>) => void) | null = null;
  onerror: ((event: ErrorEvent) => void) | null = null;
  terminated = false;
  posted: unknown[] = [];

  constructor(_url: string | URL, _options?: WorkerOptions) {
    FakeWorker.instances.push(this);
  }

  postMessage(message: unknown, _transfer?: Transferable[] | StructuredSerializeOptions): void {
    this.posted.push(message);
    const respond = FakeWorker.respond;
    if (!respond) return;
    const response = respond(message);
    if (response !== undefined) {
      queueMicrotask(() => {
        this.onmessage?.call(this, { data: response } as MessageEvent);
      });
    }
  }

  terminate(): void {
    this.terminated = true;
  }
}

function stubWorker(): void {
  (globalThis as { Worker?: unknown }).Worker = FakeWorker;
}

/** Fake phản hồi measure hợp lệ — kiểm tra request có đúng `kind: 'measure'`. */
function respondWithMeasure(
  measure: { durationMs: number; comparisons: number; swaps: number; writes: number } | null,
): (request: unknown) => unknown {
  return (request: unknown) => {
    const req = request as FakeRequest;
    expect(req.kind).toBe('measure');
    return { requestId: req.requestId, ok: true, measure };
  };
}

describe('runMeasureInWorker (ADR-012 — đo trong Web Worker, không chặn UI)', () => {
  beforeEach(() => {
    FakeWorker.instances = [];
    FakeWorker.respond = null;
    stubWorker();
  });

  afterEach(() => {
    disposeCompileWorker(); // reset singleton worker của module giữa các test
  });

  it('worker trả ok kèm số đo → resolve đúng { durationMs, comparisons, swaps, writes }', async () => {
    FakeWorker.respond = respondWithMeasure({ durationMs: 12, comparisons: 5, swaps: 2, writes: 3 });

    const result = await runMeasureInWorker('compare(arr[0], arr[1]);\nswap(arr[0], arr[1]);', [10, 20]);

    expect(result).toEqual({ durationMs: 12, comparisons: 5, swaps: 2, writes: 3 });
  });

  it('worker trả measure null (MeasureTimeoutError trong worker) → resolve null (N/A)', async () => {
    FakeWorker.respond = respondWithMeasure(null);

    await expect(runMeasureInWorker('while (true) { i++; }', [1, 2, 3])).resolves.toBeNull();
  });

  it('code lỗi (worker trả ok:false) → resolve null (không reject)', async () => {
    FakeWorker.respond = (request: unknown) => {
      const req = request as FakeRequest;
      return { requestId: req.requestId, ok: false, error: 'Lỗi cú pháp' };
    };

    await expect(runMeasureInWorker('let a = ;', [1, 2])).resolves.toBeNull();
  });

  it('worker không trả lời → kill-switch terminate + resolve null (timeout)', async () => {
    vi.useFakeTimers();
    try {
      const promise = runMeasureInWorker('while (true) { i++; }', [1, 2, 3], 50);
      const assertion = expect(promise).resolves.toBeNull();

      await vi.advanceTimersByTimeAsync(60);

      await assertion;
      expect(FakeWorker.instances[0]?.terminated).toBe(true); // kill switch đã bật
      expect(FakeWorker.instances[0]?.posted.length).toBe(1);
    } finally {
      vi.useRealTimers();
    }
  });

  it('worker onerror (sự cố runtime) → resolve null (không reject)', async () => {
    // Worker không respond → tự bắn onerror thay vì chờ timeout
    FakeWorker.respond = null;
    const promise = runMeasureInWorker('code', [1], 5000);
    const worker = FakeWorker.instances[FakeWorker.instances.length - 1];
    worker?.onerror?.call(worker, { message: 'Worker crashed' } as ErrorEvent);

    await expect(promise).resolves.toBeNull();
  });
});

describe('compileInWorker (giao thức compile cũ — tương thích ngược)', () => {
  beforeEach(() => {
    FakeWorker.instances = [];
    FakeWorker.respond = null;
    stubWorker();
  });

  afterEach(() => {
    disposeCompileWorker();
  });

  it('vẫn biên dịch bình thường khi worker trả frames (kind không bắt buộc)', async () => {
    FakeWorker.respond = (request: unknown) => {
      const req = request as FakeRequest;
      expect(req.kind).toBeUndefined(); // request compile cũ không có kind
      return { requestId: req.requestId, ok: true, frames: [] };
    };

    await expect(compileInWorker('const a = 1;', [1, 2])).resolves.toEqual([]);
  });

  it('lỗi compile → vẫn reject (hành vi cũ giữ nguyên)', async () => {
    FakeWorker.respond = (request: unknown) => {
      const req = request as FakeRequest;
      return { requestId: req.requestId, ok: false, error: 'Hết thời gian biên dịch' };
    };

    await expect(compileInWorker('code lỗi', [1])).rejects.toThrow(/Hết thời gian biên dịch/);
  });
});
