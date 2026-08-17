// features/code-to-visual/dsl/trace.ts — Interpreter đồng bộ (in-process)
// ParsedOp[] -> TraceEvent[] (snapshot sau mỗi thao tác + line carry-through).
// Lỗi runtime (pop rỗng, set ngoài phạm vi...) -> DslError {line, message} + vẫn trả
// events đã sinh tới điểm lỗi để UI hiển thị.
import type { ConsoleLogEntry, DslError, ParsedOp, TraceEvent } from './types';

export interface TraceResult {
  events: TraceEvent[];
  error: DslError | null;
  logs: ConsoleLogEntry[];
}

export interface InitialState {
  array: number[];
  stack: number[];
  queue: number[];
}

const clone = (arr: number[]): number[] => arr.slice();

function now(): string {
  return new Date().toLocaleTimeString('vi-VN', { hour12: false });
}

export function runTrace(ops: ParsedOp[], initial: InitialState): TraceResult {
  const state: InitialState = {
    array: clone(initial.array),
    stack: clone(initial.stack),
    queue: clone(initial.queue),
  };
  const logs: ConsoleLogEntry[] = [];
  const events: TraceEvent[] = [];
  let error: DslError | null = null;

  // Bước khởi tạo (line 0 — đứng trước mọi lệnh)
  events.push({
    step: 0,
    line: 0,
    structure: 'array',
    operation: 'create',
    state: clone(state.array),
    explanation: 'Khởi tạo cấu trúc dữ liệu: array = [' + state.array.join(', ') + '], stack rỗng, queue rỗng.',
  });
  logs.push({ text: 'Khởi tạo array=[' + state.array.join(', ') + '], stack=[], queue=[]', type: 'info', timestamp: now() });

  for (const op of ops) {
    const target = op.target;
    const arr = state[target];

    if (op.op === 'push') {
      if (target === 'array') {
        state.array = state.array.concat([op.value ?? 0]);
        const nextIdx = state.array.length - 1;
        events.push({
          step: events.length,
          line: op.line,
          structure: 'array',
          operation: 'push',
          state: clone(state.array),
          highlightedIndices: [nextIdx],
          explanation: 'array.push(' + (op.value ?? 0) + ') → thêm vào cuối, arr[' + nextIdx + '] = ' + (op.value ?? 0) + '.',
        });
        logs.push({ text: 'array.push(' + (op.value ?? 0) + ') → [' + state.array.join(', ') + ']', type: 'info', timestamp: now() });
        continue;
      }
      if (target === 'stack') {
        state.stack = state.stack.concat([op.value ?? 0]);
        const topIdx = state.stack.length - 1;
        events.push({
          step: events.length,
          line: op.line,
          structure: 'stack',
          operation: 'push',
          state: clone(state.stack),
          highlightedIndices: [topIdx],
          explanation: 'stack.push(' + (op.value ?? 0) + ') → đẩy lên đỉnh, top = ' + topIdx + '.',
        });
        logs.push({ text: 'stack.push(' + (op.value ?? 0) + ') → top=' + (state.stack[state.stack.length - 1] ?? 0), type: 'info', timestamp: now() });
        continue;
      }
      continue;
    }

    if (op.op === 'enqueue') {
      state.queue = state.queue.concat([op.value ?? 0]);
      const rearIdx = state.queue.length - 1;
      events.push({
        step: events.length,
        line: op.line,
        structure: 'queue',
        operation: 'enqueue',
        state: clone(state.queue),
        highlightedIndices: [rearIdx],
        explanation: 'queue.enqueue(' + (op.value ?? 0) + ') → thêm vào cuối hàng đợi, rear = ' + rearIdx + '.',
      });
      logs.push({ text: 'queue.enqueue(' + (op.value ?? 0) + ') → rear=' + rearIdx, type: 'info', timestamp: now() });
      continue;
    }

    if (op.op === 'set') {
      const index = op.index ?? 0;
      if (index < 0 || index >= arr.length) {
        error = { line: op.line, message: 'array.set: chỉ số ' + index + ' ngoài phạm vi [0..' + (arr.length - 1) + '].' };
        logs.push({ text: 'LỖI: ' + error.message, type: 'error', timestamp: now() });
        break;
      }
      const prev = arr[index];
      arr[index] = op.value ?? 0;
      events.push({
        step: events.length,
        line: op.line,
        structure: 'array',
        operation: 'set',
        state: clone(state.array),
        highlightedIndices: [index],
        explanation: 'array.set(' + index + ', ' + (op.value ?? 0) + ') → arr[' + index + ']: ' + prev + ' → ' + (op.value ?? 0) + '.',
      });
      logs.push({ text: 'array.set(' + index + ', ' + (op.value ?? 0) + ')', type: 'info', timestamp: now() });
      continue;
    }

    if (op.op === 'swap') {
      const i = op.i ?? 0;
      const j = op.j ?? 0;
      if (i < 0 || i >= arr.length || j < 0 || j >= arr.length) {
        error = { line: op.line, message: 'array.swap: chỉ số ' + i + ', ' + j + ' ngoài phạm vi [0..' + (arr.length - 1) + '].' };
        logs.push({ text: 'LỖI: ' + error.message, type: 'error', timestamp: now() });
        break;
      }
      const tmp = arr[i];
      arr[i] = arr[j];
      arr[j] = tmp;
      events.push({
        step: events.length,
        line: op.line,
        structure: 'array',
        operation: 'swap',
        state: clone(state.array),
        highlightedIndices: [i, j],
        explanation: 'array.swap(' + i + ', ' + j + ') → hoán đổi ' + tmp + ' ↔ ' + arr[i] + '.',
      });
      logs.push({ text: 'array.swap(' + i + ', ' + j + ')', type: 'info', timestamp: now() });
      continue;
    }

    if (op.op === 'pop' || op.op === 'peek' || op.op === 'dequeue' || op.op === 'front') {
      if (arr.length === 0) {
        const name = target === 'array' ? 'array.pop' : target + '.' + (op.op === 'dequeue' || op.op === 'front' ? (op.op === 'dequeue' ? 'dequeue' : 'front') : op.op);
        error = { line: op.line, message: name + ': cấu trúc đang rỗng — không thể ' + (op.op === 'peek' ? 'peek' : op.op === 'front' ? 'front' : 'lấy phần tử') + '.' };
        logs.push({ text: 'LỖI: ' + error.message, type: 'error', timestamp: now() });
        break;
      }
      if (op.op === 'pop' || op.op === 'dequeue') {
        const removed = target === 'array' || target === 'stack' ? arr[arr.length - 1] : arr[0];
        const removedIdx = target === 'queue' ? 0 : arr.length - 1;
        if (target === 'queue') state.queue = state.queue.slice(1);
        else if (target === 'stack') state.stack = state.stack.slice(0, -1);
        else state.array = state.array.slice(0, -1);
        const operation = target === 'array' ? 'pop' : target === 'stack' ? 'pop' : 'dequeue';
        events.push({
          step: events.length,
          line: op.line,
          structure: target,
          operation,
          state: clone(state[target]),
          highlightedIndices: [],
          explanation: operation + '() → lấy ' + removed + ' ra khỏi cấu trúc (ô ' + removedIdx + ').',
        });
        logs.push({ text: (target === 'queue' ? 'queue.dequeue' : operation) + '() → lấy ' + removed, type: 'info', timestamp: now() });
        continue;
      }
      // peek / front
      const shown = target === 'queue' ? arr[0] : arr[arr.length - 1];
      const shownIdx = target === 'queue' ? 0 : arr.length - 1;
      const operation = target === 'queue' ? 'front' : 'peek';
      events.push({
        step: events.length,
        line: op.line,
        structure: target,
        operation,
        state: clone(arr),
        highlightedIndices: [shownIdx],
        explanation: operation + '() → xem ' + shown + ' (không thay đổi cấu trúc).',
      });
      logs.push({ text: (target === 'queue' ? 'queue.front' : 'stack.peek') + '() → ' + shown, type: 'info', timestamp: now() });
      continue;
    }
  }

  if (!error && events.length > 1) {
    logs.push({ text: 'Hoàn tất: ' + (events.length - 1) + ' bước phân tích.', type: 'success', timestamp: now() });
  }
  return { events, error, logs };
}