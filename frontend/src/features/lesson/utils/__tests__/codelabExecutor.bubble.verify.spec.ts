import { describe, expect, it } from 'vitest';
import { executeCodelab } from '../codelabExecutor';
import type { TestCase } from '../../types/lesson.types';

/** VERIFY tạm thời (xóa sau): chấm bubble sort input→output phía client (Web Worker executor). */
const testCases: TestCase[] = [
  { input: '[[3,1,2]]', expectedOutput: '[1,2,3]' },
  { input: '[[5,4,3,2,1]]', expectedOutput: '[1, 2, 3, 4, 5]' },
  { input: '[[1]]', expectedOutput: '[1]' },
  { input: '[[2,2,9,1]]', expectedOutput: '[1,2,2,9]', isHidden: true },
];

const correctCode = `
function bubbleSort(a) {
  const n = a.length;
  for (let i = 0; i < n - 1; i++) {
    for (let j = 0; j < n - i - 1; j++) {
      if (a[j] > a[j + 1]) {
        const t = a[j]; a[j] = a[j + 1]; a[j + 1] = t;
      }
    }
  }
  return a;
}
`;

describe('VERIFY bubble sort — client executor', () => {
  it('code ĐÚNG → mọi case pass (expected có space vẫn khớp)', () => {
    const res = executeCodelab(correctCode, testCases, 'bubbleSort');
    expect(res.ok).toBe(true);
    expect(res.results).toHaveLength(4);
    expect(res.results.every(r => r.passed)).toBe(true);
  });

  it('starter cũ return input → phải FAIL (kể cả case đã trùng thứ tự)', () => {
    const res = executeCodelab('function bubbleSort(input) {\n  return input;\n}', testCases, 'bubbleSort');
    const failed = res.results.filter(r => !r.passed);
    expect(failed.length).toBeGreaterThan(0);
  });

  it('sắp xếp ngược (DESC) → FAIL', () => {
    const desc = correctCode.replace('a[j] > a[j + 1]', 'a[j] < a[j + 1]');
    const res = executeCodelab(desc, testCases, 'bubbleSort');
    expect(res.results.some(r => !r.passed)).toBe(true);
  });

  it('code rỗng → mọi case fail kèm error', () => {
    const res = executeCodelab('   \n  ', testCases, 'bubbleSort');
    expect(res.results.every(r => !r.passed)).toBe(true);
    expect(res.results.every(r => !!r.error)).toBe(true);
  });
});
