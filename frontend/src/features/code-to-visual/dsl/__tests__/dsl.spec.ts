// features/code-to-visual/dsl/__tests__/dsl.spec.ts
// Test patterns (addendum mục 12): assert trace event lineNumbers + types +
// invalid DSL -> error + errorLine; engine/DSL tests (thuần, không mount).
import { describe, expect, it } from 'vitest';

import { parseDsl } from '../parser';
import { runTrace } from '../trace';
import { eventsToSteps } from '../toSimSteps';

describe('parseDsl', () => {
  it('parse các lệnh hợp lệ array/stack/queue với line number đúng', () => {
    const { ops } = parseDsl(
      'array.push(5)\n' +
      'array.push(3)\n' +
      'array.swap(0, 1)\n' +
      'array.set(1, 9)\n' +
      'array.pop()\n' +
      'stack.push(7)\n' +
      'stack.peek()\n' +
      'stack.pop()\n' +
      'queue.enqueue(4)\n' +
      'queue.front()\n' +
      'queue.dequeue()',
    );
    expect(ops).toHaveLength(11);
    expect(ops[0]).toMatchObject({ target: 'array', op: 'push', value: 5, line: 1 });
    expect(ops[2]).toMatchObject({ target: 'array', op: 'swap', i: 0, j: 1, line: 3 });
    expect(ops[3]).toMatchObject({ target: 'array', op: 'set', index: 1, value: 9, line: 4 });
    expect(ops[5]).toMatchObject({ target: 'stack', op: 'push', value: 7, line: 6 });
    expect(ops[8]).toMatchObject({ target: 'queue', op: 'enqueue', value: 4, line: 9 });
  });

  it('bỏ qua dòng trống và comment, giữ nguyên lines cho activeLine', () => {
    const { ops, lines } = parseDsl(
      '// demo\n' +
      '\n' +
      '# nhận xét\n' +
      'array.push(1)\n' +
      '-- dòng cuối comment',
    );
    expect(ops).toHaveLength(1);
    expect(ops[0]!.line).toBe(4);
    expect(lines).toHaveLength(5);
  });

  it('từ chối lệnh lạ kèm line number', () => {
    expect(() => parseDsl('array.push(1)\nfoo.bar()')).toThrowError(
      expect.objectContaining({ line: 2 }),
    );
  });

  it('từ chối cú pháp sai (thiếu dấu ngoặc đóng) kèm line number', () => {
    expect(() => parseDsl('array.push(1\narray.push(2)')).toThrowError(
      expect.objectContaining({ line: 1 }),
    );
  });

  it('hỗ trợ số âm trong push/set', () => {
    const { ops } = parseDsl('array.push(-3)\nstack.push(-7)\narray.set(0, -1)');
    expect(ops[0]).toMatchObject({ value: -3 });
    expect(ops[1]).toMatchObject({ value: -7 });
    expect(ops[2]).toMatchObject({ value: -1 });
  });
});

describe('runTrace', () => {
  const emptyInitial = { array: [] as number[], stack: [] as number[], queue: [] as number[] };

  it('sinh event khởi tạo + 1 event mỗi lệnh với line/operation/state đúng', () => {
    const { ops } = parseDsl('array.push(5)\narray.push(3)\narray.swap(0, 1)');
    const { events, error } = runTrace(ops, { array: [1, 2], stack: [], queue: [] });
    expect(error).toBeNull();
    expect(events).toHaveLength(4); // create + 3 ops
    expect(events[0]).toMatchObject({ step: 0, line: 0, operation: 'create', structure: 'array' });
    expect(events[1]).toMatchObject({ line: 1, operation: 'push', state: [1, 2, 5], highlightedIndices: [2] });
    expect(events[2]).toMatchObject({ line: 2, operation: 'push', state: [1, 2, 5, 3], highlightedIndices: [3] });
    expect(events[3]).toMatchObject({ line: 3, operation: 'swap', state: [2, 1, 5, 3], highlightedIndices: [0, 1] });
  });

  it('stack push/pop/peek đúng LIFO', () => {
    const { ops } = parseDsl('stack.push(4)\nstack.push(9)\nstack.pop()\nstack.peek()');
    const { events, error } = runTrace(ops, emptyInitial);
    expect(error).toBeNull();
    const push1 = events[1];
    const push2 = events[2];
    const pop = events[3];
    const peek = events[4];
    expect(push1).toMatchObject({ structure: 'stack', state: [4], highlightedIndices: [0], line: 1 });
    expect(push2).toMatchObject({ state: [4, 9], highlightedIndices: [1], line: 2 });
    expect(pop).toMatchObject({ state: [4], line: 3 });
    expect(peek).toMatchObject({ state: [4], highlightedIndices: [0], line: 4 });
  });

  it('queue enqueue/dequeue/front đúng FIFO', () => {
    const { ops } = parseDsl('queue.enqueue(1)\nqueue.enqueue(2)\nqueue.front()\nqueue.dequeue()');
    const { events, error } = runTrace(ops, emptyInitial);
    expect(error).toBeNull();
    expect(events[1]).toMatchObject({ structure: 'queue', state: [1], highlightedIndices: [0], line: 1 });
    expect(events[2]).toMatchObject({ state: [1, 2], highlightedIndices: [1], line: 2 });
    expect(events[3]).toMatchObject({ operation: 'front', state: [1, 2], highlightedIndices: [0], line: 3 });
    expect(events[4]).toMatchObject({ operation: 'dequeue', state: [2], line: 4 });
  });

  it('array.set cập nhật đúng vị trí', () => {
    const { ops } = parseDsl('array.set(1, 8)');
    const { events, error } = runTrace(ops, { array: [1, 2, 3], stack: [], queue: [] });
    expect(error).toBeNull();
    expect(events[1]).toMatchObject({ operation: 'set', state: [1, 8, 3], highlightedIndices: [1], line: 1 });
  });

  it('runtime error: pop khi rỗng -> DslError có line, không throw', () => {
    const { ops } = parseDsl('stack.pop()');
    const { events, error, logs } = runTrace(ops, emptyInitial);
    expect(error).not.toBeNull();
    expect(error!.line).toBe(1);
    expect(logs.some((l) => l.type === 'error')).toBe(true);
    expect(events.length).toBeGreaterThan(0);
  });

  it('runtime error: set ngoài phạm vi -> DslError có line', () => {
    const { ops } = parseDsl('array.set(99, 1)');
    const { error } = runTrace(ops, { array: [1], stack: [], queue: [] });
    expect(error).not.toBeNull();
    expect(error!.line).toBe(1);
  });
});

describe('eventsToSteps', () => {
  it('mỗi event -> 1 Step với structure/pseudocodeLine/highlights', () => {
    const { ops } = parseDsl('array.push(5)\narray.swap(0, 1)');
    const { events, error } = runTrace(ops, { array: [1, 2], stack: [], queue: [] });
    expect(error).toBeNull();
    const steps = eventsToSteps(events);
    expect(steps).toHaveLength(3);
    expect(steps[0]!.structure.kind).toBe('array');
    expect(steps[0]!.pseudocodeLine).toBe(0);
    expect(steps[1]!.pseudocodeLine).toBe(1);
    expect(steps[1]!.highlights).toEqual(['cell:2']);
    expect(steps[2]!.highlights).toEqual(['cell:0', 'cell:1']);
    expect(steps[2]!.structure.elements[0]!.status).toBe('swap');
    expect(steps[2]!.structure.elements[1]!.status).toBe('swap');
  });

  it('stack/queue render structure đúng kind với cell muted cho ô trống', () => {
    const { ops } = parseDsl('stack.push(3)');
    const { events } = runTrace(ops, { array: [], stack: [], queue: [] });
    const steps = eventsToSteps(events);
    const last = steps[steps.length - 1]!;
    expect(last.structure.kind).toBe('stack');
    const filled = last.structure.elements.filter((e) => e.meta?.empty !== true);
    expect(filled.map((e) => e.label)).toEqual(['3']);
    // ô trống sau đỉnh muted
    const emptyCells = last.structure.elements.filter((e) => e.meta?.empty === true);
    expect(emptyCells.length).toBeGreaterThan(0);
    expect(emptyCells.every((e) => e.status === 'muted')).toBe(true);
  });

  it('stats tích lũy writes/swaps', () => {
    const { ops } = parseDsl('array.push(1)\narray.swap(0, 1)\narray.set(1, 9)');
    const { events, error } = runTrace(ops, { array: [1, 2], stack: [], queue: [] });
    expect(error).toBeNull();
    const steps = eventsToSteps(events);
    const last = steps[steps.length - 1]!;
    expect(last.stats.writes).toBe(2); // push + set; swap đếm vào swaps, không phải writes
    expect(last.stats.swaps).toBe(1);
  });
});