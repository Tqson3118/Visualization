// engines/worker/compiler.worker.ts — Web Worker chạy interpreter (không chặn UI)
//
// NGUỒN BÊ: source/VisualizationDSA3/frontend/src/core/compiler.worker.ts (V3 — bê NGUYÊN VẸN,
// chỉ sửa import path sang engines/core/stepExecutor).
//
// MỞ RỘNG (task E1a — ADR-012): thêm request `kind: 'measure'` cho chế độ ĐO không trace
// (SDD §4.0.3 v2.5 — Benchmark Lab FR-3.20): worker chạy compileAlgorithm với options.measure
// và trả về số đo thực tế. Giao thức compile cũ (không có `kind`) giữ NGUYÊN.

import {
  CompilerStepExecutor,
  MeasureTimeoutError,
  type CompileOptions,
  type MeasureCounters,
  type PlaybackFrame,
} from '../core/stepExecutor';
import type { MeasureResult } from './compileWorker';

/** Request biên dịch cũ (giữ nguyên để tương thích ngược — không bắt buộc gửi `kind`). */
interface CompileWorkerRequest {
  requestId: number;
  kind?: 'compile';
  sourceCode: string;
  initialArray: number[];
  options?: CompileOptions;
}

/** Request đo mới (kind: 'measure' — SDD §4.0.3 v2.5). */
interface MeasureWorkerRequest {
  requestId: number;
  kind: 'measure';
  code: string;
  input: unknown;
  timeoutMs: number;
}

type WorkerRequest = CompileWorkerRequest | MeasureWorkerRequest;

interface CompileWorkerResponse {
  requestId: number;
  ok: boolean;
  frames?: PlaybackFrame[];
  error?: string;
}

interface MeasureWorkerResponse {
  requestId: number;
  ok: boolean;
  /** Kết quả đo hợp lệ; null = vượt timeout (MeasureTimeoutError) → UI hiện N/A. */
  measure?: MeasureResult | null;
  error?: string;
}

type WorkerResponse = CompileWorkerResponse | MeasureWorkerResponse;

const workerScope = self as unknown as {
  onmessage: ((event: MessageEvent<WorkerRequest>) => void) | null;
  postMessage(message: WorkerResponse): void;
};

/** Chuyển input (thường là number[]) về number[] — giống toNumberArray trong stepExecutor. */
function toNumberArray(input: unknown): number[] {
  if (Array.isArray(input)) {
    return input.filter((v): v is number => typeof v === 'number');
  }
  return [];
}

/** Xử lý request đo: chạy code thật không sinh trace, đếm thao tác + đo thời gian thật. */
function handleMeasure(request: MeasureWorkerRequest): void {
  const { requestId, code, input, timeoutMs } = request;
  try {
    const counters: MeasureCounters = { comparisons: 0, swaps: 0, writes: 0 };
    const start = performance.now();
    CompilerStepExecutor.compileAlgorithm(code, toNumberArray(input), {
      fallbackToRegex: false,
      measure: { timeoutMs, counters },
    });
    const measure: MeasureResult = {
      durationMs: Math.round(performance.now() - start),
      comparisons: counters.comparisons,
      swaps: counters.swaps,
      writes: counters.writes,
    };
    workerScope.postMessage({ requestId, ok: true, measure });
  } catch (err: unknown) {
    // MeasureTimeoutError → kết quả hợp lệ dạng null (không phải lỗi) → client resolve null (N/A)
    if (err instanceof MeasureTimeoutError) {
      workerScope.postMessage({ requestId, ok: true, measure: null });
      return;
    }
    const message = err instanceof Error ? err.message : String(err);
    workerScope.postMessage({ requestId, ok: false, error: message });
  }
}

workerScope.onmessage = (event: MessageEvent<WorkerRequest>) => {
  const { requestId } = event.data;
  if (event.data.kind === 'measure') {
    handleMeasure(event.data);
    return;
  }
  const { sourceCode, initialArray, options } = event.data;
  try {
    const frames = CompilerStepExecutor.compileAlgorithm(sourceCode, initialArray, options);
    workerScope.postMessage({ requestId, ok: true, frames });
  } catch (err: unknown) {
    const message = err instanceof Error ? err.message : String(err);
    workerScope.postMessage({ requestId, ok: false, error: message });
  }
};
