// engines/worker/compileWorker.ts — client wrapper cho compiler.worker.ts (timeout + kill switch)
//
// NGUỒN BÊ: source/VisualizationDSA3/frontend/src/core/compileWorker.ts (V3 — bê NGUYÊN VẸN,
// chỉ sửa import path sang engines/core/stepExecutor).

import type { CompileOptions, PlaybackFrame } from '../core/stepExecutor';

interface CompileWorkerRequest {
  requestId: number;
  sourceCode: string;
  initialArray: number[];
  options?: CompileOptions;
}

interface CompileWorkerResponse {
  requestId: number;
  ok: boolean;
  frames?: PlaybackFrame[];
  error?: string;
}

/** Kết quả chế độ ĐO không trace (SDD §4.0.3 v2.5 — Benchmark Lab FR-3.20). */
export interface MeasureResult {
  durationMs: number;
  comparisons: number;
  swaps: number;
  writes: number;
}

interface MeasureWorkerResponse {
  requestId: number;
  ok: boolean;
  /** Kết quả đo hợp lệ; null = vượt timeout (MeasureTimeoutError) → UI hiện N/A. */
  measure?: MeasureResult | null;
  error?: string;
}

/** Thời gian tối đa cho một lần biên dịch trong worker (ms). */
export const COMPILE_TIMEOUT_MS = 15000;

/** Thời gian tối đa cho một lần đo trong worker (ms) — theo SDD §4.0.3: 5 giây/độ đo. */
export const MEASURE_TIMEOUT_MS = 5000;

let worker: Worker | null = null;
let requestCounter = 0;

function getWorker(): Worker {
  if (worker) return worker;
  worker = new Worker(new URL('./compiler.worker.ts', import.meta.url), { type: 'module' });
  return worker;
}

/**
 * Biên dịch code thuật toán trong Web Worker (không chặn UI).
 * Nếu vượt quá timeout, worker bị terminate và promise reject với lỗi rõ ràng
 * — đây là "kill switch" cuối cùng chống code treo vô hạn.
 */
export function compileInWorker(
  sourceCode: string,
  initialArray: number[],
  options?: CompileOptions,
  timeoutMs: number = COMPILE_TIMEOUT_MS,
): Promise<PlaybackFrame[]> {
  return new Promise<PlaybackFrame[]>((resolve, reject) => {
    const requestId = ++requestCounter;
    const target = getWorker();

    const timer = setTimeout(() => {
      target.terminate();
      worker = null;
      reject(new Error(`Hết thời gian biên dịch (${timeoutMs / 1000}s). Code quá nặng hoặc có vòng lặp vô hạn!`));
    }, timeoutMs);

    target.onmessage = (event: MessageEvent<CompileWorkerResponse>) => {
      if (event.data.requestId !== requestId) return;
      clearTimeout(timer);
      if (event.data.ok && event.data.frames) {
        resolve(event.data.frames);
      } else {
        reject(new Error(event.data.error ?? 'Lỗi không xác định khi biên dịch.'));
      }
    };

    target.onerror = (event: ErrorEvent) => {
      clearTimeout(timer);
      reject(new Error(event.message || 'Lỗi không xác định khi biên dịch.'));
    };

    target.postMessage({ requestId, sourceCode, initialArray, options });
  });
}

/**
 * Chế độ ĐO không trace trong Web Worker (ADR-012 — SDD §4.0.3 v2.5):
 * chạy code thật trong worker (KHÔNG sinh TraceEvent[]), đo durationMs/comparisons/swaps/writes.
 * KHÔNG bao giờ reject:
 *  - vượt timeout → kill-switch terminate worker + resolve `null` (UI hiện N/A);
 *  - MeasureTimeoutError bên trong worker (worker trả `measure: null`) → resolve `null`;
 *  - code lỗi / worker lỗi → resolve `null` (không có số đo hợp lệ).
 */
export function runMeasureInWorker(
  code: string,
  input: unknown,
  timeoutMs: number = MEASURE_TIMEOUT_MS,
): Promise<MeasureResult | null> {
  return new Promise<MeasureResult | null>((resolve) => {
    const requestId = ++requestCounter;
    const target = getWorker();

    const timer = setTimeout(() => {
      // Kill switch cuối cùng: terminate worker để chống code treo vô hạn ở phía worker.
      target.terminate();
      worker = null;
      resolve(null);
    }, timeoutMs);

    target.onmessage = (event: MessageEvent<MeasureWorkerResponse>) => {
      if (event.data.requestId !== requestId) return;
      clearTimeout(timer);
      if (event.data.ok) {
        resolve(event.data.measure ?? null);
      } else {
        // Code lỗi → không có số đo hợp lệ → null (N/A).
        resolve(null);
      }
    };

    target.onerror = (event: ErrorEvent) => {
      clearTimeout(timer);
      resolve(null);
    };

    target.postMessage({ requestId, kind: 'measure', code, input, timeoutMs });
  });
}

/** Hủy worker đang chạy (gọi khi component unmount để giải phóng tài nguyên). */
export function disposeCompileWorker(): void {
  worker?.terminate();
  worker = null;
}
