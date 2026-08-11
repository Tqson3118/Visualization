import { describe, it, expect } from 'vitest';
import {
  ARRAY_ALGORITHM_KEYS,
  normalizeArrayAlgorithmKey,
  isArrayAlgorithmKey,
  parseNumberArray,
  getCodeVizTemplate,
} from '../store/codeVizTemplates';
import { compileAndInstrument } from '../engine/ASTInstrumentationEngine';

interface HarnessFrame {
  type: string;
  indices: number[];
  arrayState: number[];
}

// Mô phỏng y hệt Web Worker Sandbox (WorkerLifecycleCoordinator.buildWorkerScript)
function runInstrumented(code: string, initialArray: number[]): { frames: HarnessFrame[]; arrayCopy: number[] } {
  const frames: HarnessFrame[] = [];
  const traceCompare = (arr: number[], i: number, j: number, op: string): boolean => {
    frames.push({ type: 'COMPARE', indices: [i, j], arrayState: arr.slice() });
    if (op === '>') return arr[i] > arr[j];
    if (op === '<') return arr[i] < arr[j];
    if (op === '>=') return arr[i] >= arr[j];
    if (op === '<=') return arr[i] <= arr[j];
    return arr[i] > arr[j];
  };
  const traceAssign = (arr: number[], i: number, val: number): number => {
    arr[i] = val;
    frames.push({ type: 'SWAP', indices: [i], arrayState: arr.slice() });
    return val;
  };
  const arrayCopy = initialArray.slice();
  // eslint-disable-next-line no-new-func
  const fn = new Function('arr', 'traceCompare', 'traceAssign', code);
  fn(arrayCopy, traceCompare, traceAssign);
  return { frames, arrayCopy };
}

function isSortedAsc(arr: number[]): boolean {
  for (let i = 1; i < arr.length; i++) {
    if (arr[i] < arr[i - 1]) return false;
  }
  return true;
}

const SORT_KEYS = [
  'bubble-sort',
  'selection-sort',
  'insertion-sort',
  'quick-sort',
  'merge-sort',
  'heap-sort',
  'radix-sort',
  'counting-sort',
  'bucket-sort',
];

describe('normalizeArrayAlgorithmKey', () => {
  it('accepts canonical array-algorithm keys', () => {
    for (const key of ARRAY_ALGORITHM_KEYS) {
      expect(normalizeArrayAlgorithmKey(key)).toBe(key);
    }
  });

  it('maps common aliases to canonical keys', () => {
    expect(normalizeArrayAlgorithmKey('bubble')).toBe('bubble-sort');
    expect(normalizeArrayAlgorithmKey('BUBBLE_SORT')).toBe('bubble-sort');
    expect(normalizeArrayAlgorithmKey('quick')).toBe('quick-sort');
    expect(normalizeArrayAlgorithmKey('merge')).toBe('merge-sort');
    expect(normalizeArrayAlgorithmKey('heap')).toBe('heap-sort');
    expect(normalizeArrayAlgorithmKey('binary search')).toBe('binary-search');
    expect(normalizeArrayAlgorithmKey('linear_search')).toBe('linear-search');
    expect(normalizeArrayAlgorithmKey('sliding window')).toBe('sliding-window');
    expect(normalizeArrayAlgorithmKey('sorting')).toBe('bubble-sort');
  });

  it('rejects non-array topics (graph/bst/oop/solid/...)', () => {
    const excluded = [
      'graph', 'bfs', 'dfs', 'dijkstra', 'bellman-ford', 'kruskal', 'prim', 'tarjan', 'a-star',
      'bst', 'tree', 'binary-search-tree', 'stack', 'queue', 'monotonic-stack',
      'oop', 'encapsulation', 'solid', 'solid-srp', 'strategy', 'patterns', 'di', 'dip',
      '', undefined, null,
    ];
    for (const raw of excluded) {
      expect(normalizeArrayAlgorithmKey(raw)).toBeNull();
    }
  });
});

describe('isArrayAlgorithmKey', () => {
  it('returns true only for array algorithms', () => {
    expect(isArrayAlgorithmKey('bubble-sort')).toBe(true);
    expect(isArrayAlgorithmKey('quick')).toBe(true);
    expect(isArrayAlgorithmKey('binary-search')).toBe(true);
    expect(isArrayAlgorithmKey('sliding-window')).toBe(true);
    expect(isArrayAlgorithmKey('bfs')).toBe(false);
    expect(isArrayAlgorithmKey('bst')).toBe(false);
    expect(isArrayAlgorithmKey('oop')).toBe(false);
    expect(isArrayAlgorithmKey('solid')).toBe(false);
  });
});

describe('parseNumberArray', () => {
  it('parses comma separated numbers', () => {
    expect(parseNumberArray('5, 3, 8, 1')).toEqual([5, 3, 8, 1]);
  });

  it('returns null for invalid or too-short input', () => {
    expect(parseNumberArray('abc')).toBeNull();
    expect(parseNumberArray('5')).toBeNull();
    expect(parseNumberArray('')).toBeNull();
    expect(parseNumberArray(undefined)).toBeNull();
  });
});

describe('getCodeVizTemplate', () => {
  it('provides code + array for every allowed key', () => {
    for (const key of ARRAY_ALGORITHM_KEYS) {
      const tpl = getCodeVizTemplate(key);
      expect(tpl.code.trim().length).toBeGreaterThan(10);
      expect(tpl.array.length).toBeGreaterThanOrEqual(2);
    }
  });

  it('prefers a valid sampleInput over the default array', () => {
    const tpl = getCodeVizTemplate('bubble-sort', '9, 7, 5, 3');
    expect(tpl.array).toEqual([9, 7, 5, 3]);
  });

  it('falls back to DEFAULT_SOURCE_CODE for unknown keys', () => {
    const tpl = getCodeVizTemplate('graph');
    expect(tpl.code).toContain('function bubbleSort');
    expect(tpl.code).toContain('arr[j] > arr[j + 1]');
  });
});

describe('templates: AST instrument + sandbox runtime (end-to-end)', () => {
  it('every sort template compiles, runs, sorts the array and emits animation frames', () => {
    for (const key of SORT_KEYS) {
      const tpl = getCodeVizTemplate(key);
      const compileResult = compileAndInstrument(tpl.code);
      expect(compileResult.success, `${key} should compile`).toBe(true);
      const { frames, arrayCopy } = runInstrumented(compileResult.instrumentedCode as string, tpl.array);
      expect(isSortedAsc(arrayCopy), `${key} should sort ascending`).toBe(true);
      expect(frames.length, `${key} should produce animation frames`).toBeGreaterThan(0);
    }
  });

  it('comparison sorts emit COMPARE frames (so compare/swap animation works)', () => {
    const comparisonKeys = ['bubble-sort', 'selection-sort', 'insertion-sort', 'quick-sort', 'merge-sort', 'heap-sort'];
    for (const key of comparisonKeys) {
      const tpl = getCodeVizTemplate(key);
      const compileResult = compileAndInstrument(tpl.code);
      const { frames } = runInstrumented(compileResult.instrumentedCode as string, tpl.array);
      expect(frames.some(f => f.type === 'COMPARE'), `${key} should emit COMPARE frames`).toBe(true);
      expect(frames.some(f => f.type === 'SWAP'), `${key} should emit SWAP frames`).toBe(true);
    }
  });

  it('search/sliding-window templates compile and run without throwing', () => {
    const runKeys = ['linear-search', 'binary-search', 'sliding-window'];
    for (const key of runKeys) {
      const tpl = getCodeVizTemplate(key);
      const compileResult = compileAndInstrument(tpl.code);
      expect(compileResult.success, `${key} should compile`).toBe(true);
      expect(() => runInstrumented(compileResult.instrumentedCode as string, tpl.array)).not.toThrow();
    }
  });
});
