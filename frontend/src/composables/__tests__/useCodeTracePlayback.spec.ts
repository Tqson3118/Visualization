// composables/__tests__/useCodeTracePlayback.spec.ts — TraceEvent[] → Structure frame + playback
//
// Dùng vi.useFakeTimers để điều khiển interval chính xác. Kiểm tra:
// - SAMPLING: trace 5000 event → ≤ maxFrames+1 frame, frame cuối = trạng thái cuối THẬT ('done' + sorted).
// - play/stepForward/stepBack/jumpTo với bounds (clamp cả 2 đầu, play ở cuối → reset về 0).
// - pause/dispose không rò timer (vi.getTimerCount() === 0); setSpeed restart interval với ms mới.
// - KHÔNG hardcode: dùng runCode THẬT (stepExecutor) — bubble vs selection sort → trace khác nhau.
// - Status mapping theo kind (compare→highlight, swap→swap, assign→active, cuối→done), id lạ (node:/queue:) bỏ qua.
// - currentLine/currentVars map đúng theo currentIndex (kể cả sau sampling).
// - Fallback khi trace không có vars.array: mảng tuần tự từ initialArray, swap theo highlight.

import { afterEach, beforeEach, describe, expect, test, vi } from 'vitest';

import { runCode, type CodeSimulation, type TraceEvent } from '@/engines/core/stepExecutor';
import type { Element } from '@/engines/core/types';
import { useCodeTracePlayback } from '../useCodeTracePlayback';

const DEFAULT_ARRAY = [5, 3, 8, 1, 9, 2, 7];
const SORTED = [1, 2, 3, 5, 7, 8, 9];

const BUBBLE_SORT_CODE = `for (let i = 0; i < arr.length - 1; i++) {
  for (let j = 0; j < arr.length - 1 - i; j++) {
    compare(arr[j], arr[j + 1]);
    if (arr[j] > arr[j + 1]) {
      swap(arr[j], arr[j + 1]);
    }
  }
}`;

const SELECTION_SORT_CODE = `for (let i = 0; i < arr.length - 1; i++) {
  let min = i;
  for (let j = i + 1; j < arr.length; j++) {
    compare(arr[min], arr[j]);
    if (arr[j] < arr[min]) {
      min = j;
    }
  }
  if (min !== i) {
    swap(arr[i], arr[min]);
  }
}`;

function sim(code: string): CodeSimulation {
  return { code, entry: 'main', bindings: [{ variable: 'arr', structure: 'array' }] };
}

function makeEvent(
  line: number,
  vars: Record<string, unknown>,
  highlight: string[],
  kind: TraceEvent['kind'],
): TraceEvent {
  return { line, vars, highlight, kind, explanation: `dòng ${line}` };
}

/** Trace giả dài `length`: mảng đang giảm dần, event cuối = mảng đã sort. */
function makeLongTrace(length: number): TraceEvent[] {
  const trace: TraceEvent[] = [];
  for (let i = 0; i < length - 1; i++) {
    trace.push(makeEvent(1, { array: [9, 7, 5, 3, 1], i }, ['cell:0', 'cell:1'], 'compare'));
  }
  trace.push(makeEvent(2, { array: [1, 3, 5, 7, 9] }, [], 'return'));
  return trace;
}

function labelsOf(structure: { elements: Element[] }): string[] {
  return structure.elements.map((el) => el.label);
}

function statusesOf(structure: { elements: Element[] }): string[] {
  return structure.elements.map((el) => el.status);
}

beforeEach(() => {
  vi.useFakeTimers();
});

afterEach(() => {
  vi.useRealTimers();
});

describe('useCodeTracePlayback', () => {
  test('SAMPLING: trace 5000 event → ≤ 3001 frame, frame cuối = trạng thái cuối THẬT (sorted + all done)', () => {
    const trace = makeLongTrace(5000);
    const playback = useCodeTracePlayback();
    playback.init(trace);

    expect(playback.totalFrames.value).toBeGreaterThan(0);
    expect(playback.totalFrames.value).toBeLessThanOrEqual(3001); // KHÔNG đẩy 5000 frame vào UI
    expect(playback.frames.value.length).toBe(playback.totalFrames.value);

    const first = playback.frames.value[0];
    expect(labelsOf(first)).toEqual(['9', '7', '5', '3', '1']); // mảng ban đầu

    const last = playback.frames.value[playback.totalFrames.value - 1];
    expect(labelsOf(last)).toEqual(['1', '3', '5', '7', '9']); // sorted — trạng thái cuối thật
    expect(statusesOf(last)).toEqual(['done', 'done', 'done', 'done', 'done']);
  });

  test('play/stepForward/stepBack/jumpTo: advance theo durationPerStep, clamp bounds 2 đầu', () => {
    const trace = Array.from({ length: 10 }, (_, i) =>
      makeEvent(i + 1, { array: DEFAULT_ARRAY, i }, [], 'loop'),
    );
    const playback = useCodeTracePlayback();
    playback.init(trace);
    expect(playback.currentIndex.value).toBe(0);

    playback.play();
    expect(playback.isPlaying.value).toBe(true);
    vi.advanceTimersByTime(250); // 1 bước
    expect(playback.currentIndex.value).toBe(1);
    vi.advanceTimersByTime(500); // 2 bước nữa
    expect(playback.currentIndex.value).toBe(3);

    playback.stepBack();
    expect(playback.currentIndex.value).toBe(2);

    // Bounds: jumpTo quá xa / âm
    playback.jumpTo(99999);
    expect(playback.currentIndex.value).toBe(playback.totalFrames.value - 1);
    playback.stepForward(); // ở cuối → giữ nguyên
    expect(playback.currentIndex.value).toBe(playback.totalFrames.value - 1);

    playback.jumpTo(-5);
    expect(playback.currentIndex.value).toBe(0);
    playback.stepBack(); // ở 0 → giữ 0
    expect(playback.currentIndex.value).toBe(0);
  });

  test('play() khi đang ở frame cuối → reset về 0 trước khi chạy', () => {
    const trace = makeLongTrace(20);
    const playback = useCodeTracePlayback();
    playback.init(trace);
    playback.jumpTo(playback.totalFrames.value - 1);

    playback.play();
    expect(playback.currentIndex.value).toBe(0);
    expect(playback.isPlaying.value).toBe(true);
  });

  test('play → chạm cuối → tự pause; stepForward ở cuối không vượt', () => {
    const trace = makeLongTrace(6); // 6 frame, không sampling
    const playback = useCodeTracePlayback();
    playback.init(trace);

    playback.play();
    vi.advanceTimersByTime(250 * 10); // vượt xa cuối
    expect(playback.currentIndex.value).toBe(playback.totalFrames.value - 1);
    expect(playback.isPlaying.value).toBe(false); // đã tự pause

    playback.stepForward();
    expect(playback.currentIndex.value).toBe(playback.totalFrames.value - 1); // không vượt
  });

  test('pause()/dispose() không rò timer (vi.getTimerCount() === 0)', () => {
    const playback = useCodeTracePlayback();
    playback.init(makeLongTrace(10));

    playback.play();
    expect(vi.getTimerCount()).toBe(1);
    playback.pause();
    expect(vi.getTimerCount()).toBe(0);
    expect(playback.isPlaying.value).toBe(false);

    playback.play();
    expect(vi.getTimerCount()).toBe(1);
    playback.dispose();
    expect(vi.getTimerCount()).toBe(0);
    expect(playback.isPlaying.value).toBe(false);
    expect(playback.currentIndex.value).toBe(0); // reset index để tái dùng
  });

  test('setSpeed: restart interval với ms mới khi đang play', () => {
    const trace = Array.from({ length: 12 }, (_, i) =>
      makeEvent(i + 1, { array: DEFAULT_ARRAY, i }, [], 'loop'),
    );
    const playback = useCodeTracePlayback();
    playback.init(trace);

    playback.play(); // durationPerStep mặc định 250
    vi.advanceTimersByTime(500); // 2 bước
    expect(playback.currentIndex.value).toBe(2);

    playback.setSpeed(100);
    vi.advanceTimersByTime(300); // 3 bước với 100ms
    expect(playback.currentIndex.value).toBe(5);
    expect(playback.durationPerStep.value).toBe(100);
    expect(playback.isPlaying.value).toBe(true); // vẫn chưa tới cuối (12 frame)
  });

  test('KHÔNG hardcode: runCode THẬT bubble vs selection sort (cùng input) → trace/frames khác nhau', () => {
    const bubble = runCode(sim(BUBBLE_SORT_CODE), DEFAULT_ARRAY);
    const selection = runCode(sim(SELECTION_SORT_CODE), DEFAULT_ARRAY);

    expect(bubble.error).toBeUndefined();
    expect(selection.error).toBeUndefined();
    expect(bubble.trace.length).toBeGreaterThan(0);
    expect(selection.trace.length).toBeGreaterThan(0);
    expect(bubble.trace.length).not.toBe(selection.trace.length); // 2 thuật toán → trace khác nhau

    // Cả 2 kết thúc với mảng đã sort thật
    const lastBubble = bubble.trace[bubble.trace.length - 1].vars.array as number[];
    const lastSelection = selection.trace[selection.trace.length - 1].vars.array as number[];
    expect(lastBubble).toEqual(SORTED);
    expect(lastSelection).toEqual(SORTED);

    const pbBubble = useCodeTracePlayback();
    pbBubble.init(bubble.trace, DEFAULT_ARRAY);
    const pbSelection = useCodeTracePlayback();
    pbSelection.init(selection.trace, DEFAULT_ARRAY);

    expect(pbBubble.totalFrames.value).not.toBe(pbSelection.totalFrames.value);
    const finalBubble = pbBubble.frames.value[pbBubble.totalFrames.value - 1];
    expect(labelsOf(finalBubble)).toEqual(SORTED.map(String));
    expect(statusesOf(finalBubble)).toEqual(SORTED.map(() => 'done'));
  });

  test('status mapping theo kind: compare→highlight, swap→swap, assign→active, id lạ bỏ qua, cuối→done', () => {
    const trace: TraceEvent[] = [
      makeEvent(10, { array: [5, 3] }, ['cell:0', 'cell:1'], 'compare'),
      makeEvent(11, { array: [5, 3] }, ['cell:0', 'cell:1'], 'swap'),
      makeEvent(12, { array: [3, 5] }, ['node:9', 'queue:x'], 'assign'), // id lạ → bỏ qua, giữ default
      makeEvent(13, { array: [3, 5] }, ['cell:1'], 'return'),
    ];
    const playback = useCodeTracePlayback();
    playback.init(trace);

    const [f0, f1, f2, f3] = playback.frames.value;
    expect(statusesOf(f0)).toEqual(['highlight', 'highlight']); // compare
    expect(statusesOf(f1)).toEqual(['swap', 'swap']); // swap
    expect(statusesOf(f2)).toEqual(['default', 'default']); // node:/queue: không áp cho cell
    expect(statusesOf(f3)).toEqual(['done', 'done']); // event cuối → tất cả done
    expect(labelsOf(f3)).toEqual(['3', '5']);
  });

  test('currentLine/currentVars theo currentIndex (kể cả sau sampling map về trace gốc)', () => {
    const trace = makeLongTrace(5000); // sampling bật (step = ceil(5000/3000) = 2)
    const playback = useCodeTracePlayback();
    playback.init(trace);

    expect(playback.currentLine.value).toBe(trace[0].line);
    expect((playback.currentVars.value.array as number[])).toEqual([9, 7, 5, 3, 1]);

    playback.jumpTo(playback.totalFrames.value - 1); // frame cuối = event trace[4999]
    expect(playback.currentLine.value).toBe(trace[4999].line);
    expect((playback.currentVars.value.array as number[])).toEqual([1, 3, 5, 7, 9]);

    playback.jumpTo(1); // frame thứ 2 = trace[step] = trace[2]
    expect(playback.currentLine.value).toBe(trace[2].line);
  });

  test('fallback khi trace KHÔNG có vars.array: mảng tuần tự từ initialArray, swap theo highlight', () => {
    const trace: TraceEvent[] = [
      makeEvent(1, {}, ['cell:0', 'cell:1'], 'compare'), // không đổi
      makeEvent(2, {}, ['cell:0', 'cell:1'], 'swap'), // 5↔3
      makeEvent(3, {}, ['cell:2', 'cell:3'], 'swap'), // 8↔1
      makeEvent(4, {}, [], 'return'),
    ];
    const playback = useCodeTracePlayback();
    playback.init(trace, [5, 3, 8, 1]);

    expect(labelsOf(playback.frames.value[0])).toEqual(['5', '3', '8', '1']);
    expect(labelsOf(playback.frames.value[1])).toEqual(['3', '5', '8', '1']);
    expect(labelsOf(playback.frames.value[2])).toEqual(['3', '5', '1', '8']);
    const last = playback.frames.value[3];
    expect(labelsOf(last)).toEqual(['3', '5', '1', '8']);
    expect(statusesOf(last)).toEqual(['done', 'done', 'done', 'done']);
  });
});
