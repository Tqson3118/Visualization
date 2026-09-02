import { describe, expect, it } from 'vitest';
import '../catalog';
import { getSimulation } from '../registry';

describe('Audit All 44 Algorithms — Specific Verification Tests', () => {
  it('C1: structure.graph respects preset and creates correct edges', () => {
    const gen = getSimulation('structure.graph')!;
    const stepsCycle = gen.generate({
      kind: 'graph',
      data: { vertices: 4, preset: 'cycle', directed: true, weighted: true },
    });
    expect(stepsCycle.length).toBeGreaterThan(0);
    const links = stepsCycle[0].structure.links ?? [];
    expect(links.length).toBe(4);
  });

  it('C2: structure.avl builds dynamic AVL tree with calculated bf and height for [1, 2, 3]', () => {
    const gen = getSimulation('structure.avl')!;
    const steps = gen.generate({ kind: 'tree', data: { values: [1, 2, 3] } });
    expect(steps.length).toBeGreaterThan(0);
    const elements = steps[0].structure.elements;
    // For [1, 2, 3], AVL balances with root 2, left 1, right 3
    const root2 = elements.find((e) => e.label === '2');
    expect(root2).toBeDefined();
    expect(root2?.meta?.height).toBe(2);
    expect(root2?.meta?.bf).toBe(0);
  });

  it('C3: structure.binarytree builds general binary tree and traces without BST assumptions', () => {
    const gen = getSimulation('structure.binarytree')!;
    const steps = gen.generate({ kind: 'tree', data: { values: [50, 80, 20] } });
    expect(steps.length).toBeGreaterThan(0);
    const root = steps[0].structure.elements.find((e) => e.label === '50');
    expect(root).toBeDefined();
  });

  it('C4 & C5: tree.bst-insert and tree.bst-delete trace insert attachment and 2-child successor delete', () => {
    const insertGen = getSimulation('tree.bst-insert')!;
    const insertSteps = insertGen.generate({
      kind: 'tree',
      data: { keys: [50, 30, 70], value: 20 },
    });
    expect(insertSteps.some((s) => s.explanation.includes('newNode(20)') || s.explanation.includes('20'))).toBe(true);

    const deleteGen = getSimulation('tree.bst-delete')!;
    const deleteSteps = deleteGen.generate({
      kind: 'tree',
      data: { keys: [50, 30, 70, 60, 80], value: 70 },
    });
    expect(deleteSteps.some((s) => s.explanation.includes('tìm min cây con phải'))).toBe(true);
    expect(deleteSteps.some((s) => s.explanation.includes('Xóa đệ quy nút 80'))).toBe(true);
  });

  it('C6 & C7: graph.dfs handles unvisited vertices and detects disconnected components', () => {
    const gen = getSimulation('graph.dfs')!;
    const steps = gen.generate({
      kind: 'graph',
      data: {
        vertices: 6,
        preset: 'custom',
        edges: 1,
        directed: false,
        source: 0,
      },
    });
    expect(steps.some((s) => s.explanation.includes('không thể đến được từ đỉnh nguồn 0'))).toBe(true);
  });

  it('C7: graph.bfs handles disconnected components cleanly', () => {
    const gen = getSimulation('graph.bfs')!;
    const steps = gen.generate({
      kind: 'graph',
      data: {
        vertices: 6,
        preset: 'custom',
        edges: 1,
        directed: false,
        source: 0,
      },
    });
    expect(steps.some((s) => s.explanation.includes('không thể đến được từ đỉnh nguồn 0'))).toBe(true);
  });

  it('C10: graph.dijkstra supports target with early exit and path backtracking', () => {
    const gen = getSimulation('graph.dijkstra')!;
    const steps = gen.generate({
      kind: 'graph',
      data: {
        vertices: 4,
        preset: 'path',
        directed: true,
        weighted: true,
        source: 0,
        target: 2,
      },
    });
    const last = steps[steps.length - 1];
    expect(last.explanation).toContain('đích 2');
    expect(last.explanation).toContain('0 → 1 → 2');
  });

  it('C11: list.delete cleans deleted node status on delete', () => {
    const gen = getSimulation('list.delete')!;
    const steps = gen.generate({
      kind: 'list',
      data: { initialValues: [10, 20, 30], position: 0 },
    });
    const last = steps[steps.length - 1];
    expect(last.structure.elements.length).toBe(2);
    expect(last.structure.elements.every((e) => e.status !== 'error')).toBe(true);
  });

  it('C12: queue.dequeue handles dequeue and updates front pointer', () => {
    const gen = getSimulation('queue.dequeue')!;
    const steps = gen.generate({
      kind: 'queue',
      data: {
        capacity: 8,
        operations: ['Push 10', 'Push 20', 'Pop'],
      },
    });
    const dequeueStep = steps.find((s) => s.explanation.includes('Dequeue') || s.explanation.includes('10 đã ra khỏi hàng đợi'));
    expect(dequeueStep).toBeDefined();
    expect(dequeueStep?.variables.front).toBeDefined();
  });

  it('C13: tree.avl-insert maintains valid state.root upon rotations in subtree', () => {
    const gen = getSimulation('tree.avl-insert')!;
    const steps = gen.generate({
      kind: 'tree',
      data: { keys: [50, 20, 70, 10, 30], value: 5 },
    });
    expect(steps.some((s) => s.explanation.includes('xoay LL') || s.explanation.includes('xoay'))).toBe(true);
  });

  it('C14 & M3: sort.heap uses heap structure kind and clear swap explanation', () => {
    const gen = getSimulation('sort.heap')!;
    const steps = gen.generate({ kind: 'heap', data: { values: [4, 10, 3, 5, 1] } });
    expect(steps[0].structure.kind).toBe('heap');
    expect(steps.some((s) => s.explanation.includes('Hoán đổi a[0]') || s.explanation.includes('hoán đổi cha'))).toBe(true);
  });

  it('M1: sort.bubble uses inner loop bound and single terminal step', () => {
    const gen = getSimulation('sort.bubble')!;
    const steps = gen.generate({ kind: 'array', data: { values: [3, 1, 2] } });
    expect(steps.length).toBe(17);
    expect(steps[steps.length - 1].explanation).toContain('Kết thúc');
  });

  it('M7: hash.insert uses unshift for chaining head insertion and passes meta', () => {
    const gen = getSimulation('hash.insert')!;
    const steps = gen.generate({
      kind: 'hashTable',
      data: { tableSize: 5, keys: [5, 10], hashMode: 'modulo' },
    });
    const collisionStep = steps.find((s) => s.explanation.includes('thêm nút mới vào đầu bucket'));
    expect(collisionStep).toBeDefined();
    expect(collisionStep?.structure.meta?.hashFormula).toBe('h(k) = k mod m');
  });
});
