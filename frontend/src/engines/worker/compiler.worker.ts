// engines/worker/compiler.worker.ts — Web Worker chạy interpreter (không chặn UI)
//
// NGUỒN BÊ: source/VisualizationDSA3/frontend/src/core/compiler.worker.ts (V3 — bê NGUYÊN VẸN,
// chỉ sửa import path sang engines/core/stepExecutor).

import { CompilerStepExecutor, type CompileOptions, type PlaybackFrame } from '../core/stepExecutor';

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

const workerScope = self as unknown as {
  onmessage: ((event: MessageEvent<CompileWorkerRequest>) => void) | null;
  postMessage(message: CompileWorkerResponse): void;
};

workerScope.onmessage = (event: MessageEvent<CompileWorkerRequest>) => {
  const { requestId, sourceCode, initialArray, options } = event.data;
  try {
    const frames = CompilerStepExecutor.compileAlgorithm(sourceCode, initialArray, options);
    workerScope.postMessage({ requestId, ok: true, frames });
  } catch (err: unknown) {
    const message = err instanceof Error ? err.message : String(err);
    workerScope.postMessage({ requestId, ok: false, error: message });
  }
};
