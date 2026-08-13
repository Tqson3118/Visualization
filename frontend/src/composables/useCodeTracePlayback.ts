// composables/useCodeTracePlayback.ts — biến TraceEvent[] (runCode, SDD §4.0.3) thành dãy
// Structure frame (SDD §4.2) + playback API (play/pause/step/jump/speed) + SAMPLING.
//
// - Mỗi TraceEvent → 1 Structure frame kind='array' (element id 'cell:<i>', label = giá trị).
// - Giá trị ưu tiên trace[i].vars.array (number[]) — do stepExecutor luôn đính kèm snapshot
//   mảng. Fallback khi KHÔNG có vars.array (code không dùng mảng): duy trì mảng tuần tự từ
//   initialArray (mặc định [5,3,8,1,9,2,7]), swap event đổi 2 vị trí theo highlight 'cell:a'/'cell:b',
//   các kind khác giữ nguyên (trạng thái hiển thị qua status).
// - Status mapping: kind='swap' → 'swap'; 'compare' → 'highlight'; 'assign'/'declare' → 'active';
//   'loop'/'call'/'return' → highlight ids (nếu có) 'highlight'; còn lại 'default'. Event CUỐI
//   (trace[length-1]) → tất cả 'done' — frame cuối LUÔN đúng trạng thái cuối thật.
// - SAMPLING: nếu trace.length > maxFrames (mặc định 3000) → step = ceil(length/maxFrames),
//   lấy trace[0], trace[step], trace[2*step], ... LUÔN kèm event cuối → KHÔNG đẩy 50.000 frame
//   vào UI (giới hạn ≤ maxFrames + 1 frame).
// - Playback dùng setInterval (structure đổi rời rạc — KHÔNG rAF). play() ở frame cuối → reset
//   về 0 trước. setSpeed(ms) restart interval nếu đang play. dispose() dọn timer (gọi từ onUnmounted).

import { computed, ref, type ComputedRef, type Ref } from 'vue';

import type { Element, ElementStatus, Structure } from '@/engines/core/types';
import type { TraceEvent, TraceKind } from '@/engines/core/stepExecutor';

const DEFAULT_MAX_FRAMES = 3000;
const DEFAULT_DURATION_MS = 250;
const DEFAULT_ARRAY: number[] = [5, 3, 8, 1, 9, 2, 7];

export interface CodeTracePlaybackOptions {
  /** Giới hạn số frame tối đa sau sampling — mặc định 3000. */
  maxFrames?: number;
}

/** Trích 2 vị trí từ highlight ids dạng 'cell:a' / 'cell:b' — fallback -1 khi không khớp. */
function cellPairFromHighlight(highlight: string[]): [number, number] {
  const idx: number[] = [];
  for (const id of highlight) {
    const m = /^cell:(\d+)$/.exec(id);
    if (m) idx.push(Number(m[1]));
    if (idx.length === 2) break;
  }
  return [idx[0] ?? -1, idx[1] ?? -1];
}

function isNumberArray(value: unknown): value is number[] {
  return Array.isArray(value) && value.every((v) => typeof v === 'number');
}

/** Status mỗi element theo kind của event (không áp cho event cuối — event cuối toàn 'done'). */
function statusForKind(kind: TraceKind): ElementStatus {
  switch (kind) {
    case 'swap':
      return 'swap';
    case 'compare':
      return 'highlight';
    case 'assign':
    case 'declare':
      return 'active';
    default:
      // 'loop' | 'call' | 'return' — highlight ids (nếu có) set 'highlight'
      return 'highlight';
  }
}

/** Chọn chỉ số trace gốc cho từng frame: sampling đều + LUÔN kèm event cuối. */
function sampleTraceIndices(traceLength: number, maxFrames: number): number[] {
  if (traceLength === 0) return [];
  if (traceLength <= maxFrames) {
    return Array.from({ length: traceLength }, (_, i) => i);
  }
  const step = Math.ceil(traceLength / maxFrames);
  const indices: number[] = [];
  for (let i = 0; i < traceLength - 1; i += step) {
    indices.push(i);
  }
  indices.push(traceLength - 1); // frame cuối = trạng thái cuối THẬT
  return indices;
}

/** Dựng 1 Structure frame kind='array' từ mảng giá trị + status theo event. */
function buildArrayFrame(values: number[], highlight: string[], kind: TraceKind, isFinal: boolean): Structure {
  const elements: Element[] = values.map((value, i) => {
    const id = `cell:${i}`;
    let status: ElementStatus = isFinal ? 'done' : 'default';
    if (!isFinal && highlight.includes(id)) {
      status = statusForKind(kind);
    }
    return { id, label: String(value), status };
  });
  return { kind: 'array', elements, links: [] };
}

export function useCodeTracePlayback(options: CodeTracePlaybackOptions = {}): {
  frames: ComputedRef<Structure[]>;
  currentIndex: Ref<number>;
  currentStructure: ComputedRef<Structure | null>;
  totalFrames: ComputedRef<number>;
  isPlaying: Ref<boolean>;
  durationPerStep: Ref<number>;
  currentLine: ComputedRef<number>;
  currentVars: ComputedRef<Record<string, unknown>>;
  init(trace: TraceEvent[], initialArray?: number[]): void;
  play(): void;
  pause(): void;
  stepForward(): void;
  stepBack(): void;
  jumpTo(index: number): void;
  reset(): void;
  setSpeed(ms: number): void;
  dispose(): void;
} {
  const maxFrames = options.maxFrames ?? DEFAULT_MAX_FRAMES;

  const traceRef = ref<TraceEvent[]>([]);
  /** Chỉ số trace GỐC tương ứng từng frame (sau sampling) — currentLine/currentVars dùng để map. */
  const frameIndices = ref<number[]>([]);
  const frameList = ref<Structure[]>([]);
  const currentIndex = ref(0);
  const isPlaying = ref(false);
  const durationPerStep = ref(DEFAULT_DURATION_MS);

  let intervalId: ReturnType<typeof setInterval> | null = null;

  const frames = computed<Structure[]>(() => frameList.value);
  const totalFrames = computed<number>(() => frameIndices.value.length);

  const currentStructure = computed<Structure | null>(() => {
    const i = currentIndex.value;
    return i >= 0 && i < frameList.value.length ? frameList.value[i] : null;
  });

  const currentLine = computed<number>(() => {
    const ti = frameIndices.value[currentIndex.value];
    return ti !== undefined && ti >= 0 && ti < traceRef.value.length ? traceRef.value[ti].line : 0;
  });

  const currentVars = computed<Record<string, unknown>>(() => {
    const ti = frameIndices.value[currentIndex.value];
    return ti !== undefined && ti >= 0 && ti < traceRef.value.length ? traceRef.value[ti].vars : {};
  });

  function clearTimer(): void {
    if (intervalId !== null) {
      clearInterval(intervalId);
      intervalId = null;
    }
  }

  /**
   * Nạp trace mới → dãy Structure frame (có sampling). Dừng playback cũ, đưa về index 0.
   * `initialArray` chỉ dùng khi trace không có vars.array (fallback — code không dùng mảng).
   */
  function init(trace: TraceEvent[], initialArray: number[] = DEFAULT_ARRAY): void {
    pause();
    traceRef.value = trace;

    // Duyệt tuần tự để duy trì mảng fallback (chỉ swap đổi giá trị — assign không có giá trị mới).
    // Khi event có vars.array (number[]) → ưu tiên dùng chính nó.
    const valuesByIndex: number[][] = [];
    let current = isNumberArray(trace[0]?.vars.array)
      ? [...(trace[0].vars.array as number[])]
      : [...initialArray];
    for (let i = 0; i < trace.length; i++) {
      const varsArray = trace[i].vars.array;
      if (isNumberArray(varsArray)) {
        current = [...varsArray];
      } else if (trace[i].kind === 'swap') {
        const [a, b] = cellPairFromHighlight(trace[i].highlight);
        if (a >= 0 && b >= 0 && a < current.length && b < current.length) {
          const tmp = current[a];
          current[a] = current[b];
          current[b] = tmp;
        }
      }
      valuesByIndex.push([...current]);
    }

    const indices = sampleTraceIndices(trace.length, maxFrames);
    frameIndices.value = indices;
    frameList.value = indices.map((ti) =>
      buildArrayFrame(valuesByIndex[ti], trace[ti].highlight, trace[ti].kind, ti === trace.length - 1),
    );
    currentIndex.value = 0;
  }

  function play(): void {
    if (frameIndices.value.length === 0 || isPlaying.value) return;
    if (currentIndex.value >= totalFrames.value - 1) {
      currentIndex.value = 0; // đang ở frame cuối → quay về đầu trước khi chạy
    }
    isPlaying.value = true;
    intervalId = setInterval(() => stepForward(), durationPerStep.value);
  }

  function pause(): void {
    clearTimer();
    isPlaying.value = false;
  }

  function stepForward(): void {
    if (frameIndices.value.length === 0) return;
    currentIndex.value = Math.min(currentIndex.value + 1, totalFrames.value - 1);
    if (currentIndex.value >= totalFrames.value - 1 && isPlaying.value) {
      pause(); // chạm cuối khi đang play → dừng
    }
  }

  function stepBack(): void {
    currentIndex.value = Math.max(currentIndex.value - 1, 0);
  }

  function jumpTo(index: number): void {
    if (frameIndices.value.length === 0) return;
    currentIndex.value = Math.max(0, Math.min(index, totalFrames.value - 1));
  }

  function reset(): void {
    currentIndex.value = 0;
    pause();
  }

  function setSpeed(ms: number): void {
    durationPerStep.value = Math.max(1, ms);
    if (isPlaying.value) {
      clearTimer(); // restart interval với nhịp mới
      intervalId = setInterval(() => stepForward(), durationPerStep.value);
    }
  }

  function dispose(): void {
    clearTimer();
    isPlaying.value = false;
    currentIndex.value = 0; // reset index để tái dùng (init lại được)
  }

  return {
    frames,
    currentIndex,
    currentStructure,
    totalFrames,
    isPlaying,
    durationPerStep,
    currentLine,
    currentVars,
    init,
    play,
    pause,
    stepForward,
    stepBack,
    jumpTo,
    reset,
    setSpeed,
    dispose,
  };
}
