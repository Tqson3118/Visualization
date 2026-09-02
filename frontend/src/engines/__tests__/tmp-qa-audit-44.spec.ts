import { describe, expect, it } from 'vitest';
import '../catalog';
import { getSimulation } from '../registry';
import { CATALOG } from '../catalog';
import type { Step, SimulationGenerator } from '../core/types';

function defaultData(gen: SimulationGenerator): Record<string, unknown> {
  const d: Record<string, unknown> = {};
  for (const f of gen.inputSchema.fields) d[f.name] = f.default;
  return d;
}
function nums(elements: { label: string }[]): number[] {
  return elements.map((e) => Number(e.label)).filter((v) => !Number.isNaN(v));
}
function isSortedAsc(a: number[]): boolean {
  for (let i = 1; i < a.length; i++) if (a[i - 1] > a[i]) return false;
  return true;
}
function sameMultiset(a: number[], b: number[]): boolean {
  if (a.length !== b.length) return false;
  const s = [...a].sort((x, y) => x - y).join(',');
  const t = [...b].sort((x, y) => x - y).join(',');
  return s === t;
}
function isMaxHeap(a: number[]): boolean {
  for (let i = 0; i < a.length; i++) {
    const l = 2 * i + 1, r = 2 * i + 2;
    if (l < a.length && a[i] < a[l]) return false;
    if (r < a.length && a[i] < a[r]) return false;
  }
  return true;
}
function lastArr(step: Step): number[] {
  return nums(step.structure.elements);
}
function firstArr(step: Step): number[] {
  return nums(step.structure.elements);
}

describe('QA AUDIT: 44 generators - structural invariants', () => {
  it('A1: every key produces valid steps (structure, explanation, pseudocodeLine, stats monotonic)', () => {
    const problems: string[] = [];
    for (const meta of CATALOG) {
      const gen = getSimulation(meta.key);
      if (!gen) { problems.push(`${meta.key}: registry missing`); continue; }
      const input = { kind: gen.inputSchema.kind, data: defaultData(gen) };
      if (!gen.validate(input).ok) { problems.push(`${meta.key}: default input invalid: ${gen.validate(input).errors.join('; ')}`); continue; }
      const steps = gen.generate(input);
      if (steps.length < 3) { problems.push(`${meta.key}: only ${steps.length} steps`); continue; }
      let pc = 0, sw = 0, wr = 0;
      steps.forEach((s, i) => {
        if (!s.explanation || s.explanation.trim().length < 4) problems.push(`${meta.key}#step${i + 1}: explanation empty`);
        if (s.pseudocodeLine < 1 || s.pseudocodeLine > gen.pseudocode.length) problems.push(`${meta.key}#step${i + 1}: line ${s.pseudocodeLine} out of 1..${gen.pseudocode.length}`);
        if (s.stats.comparisons < pc || s.stats.swaps < sw || s.stats.writes < wr) problems.push(`${meta.key}#step${i + 1}: stats not monotonic`);
        pc = s.stats.comparisons; sw = s.stats.swaps; wr = s.stats.writes;
      });
    }
    console.log(problems.length ? 'PROBLEMS:\n' + problems.join('\n') : 'A1: ALL 44 KEYS PASS structural invariants');
    expect(problems).toEqual([]);
  });

  it('A2: sort.* final arrays sorted ascending + multiset preserved', () => {
    const problems: string[] = [];
    for (const key of ['sort.bubble', 'sort.selection', 'sort.insertion', 'sort.merge', 'sort.quick', 'sort.heap']) {
      const gen = getSimulation(key)!;
      const steps = gen.generate({ kind: 'array', data: defaultData(gen) });
      const a0 = firstArr(steps[0]);
      const aN = lastArr(steps[steps.length - 1]);
      if (!isSortedAsc(aN)) problems.push(`${key}: final NOT sorted: [${aN.join(',')}]`);
      if (!sameMultiset(a0, aN)) problems.push(`${key}: multiset changed: [${a0}] -> [${aN}]`);
      console.log(`${key}: n=${a0.length} steps=${steps.length} cmp=${steps[steps.length - 1].stats.comparisons} swap=${steps[steps.length - 1].stats.swaps} final=[${aN.join(',')}]`);
    }
    expect(problems).toEqual([]);
  });

  it('A3: heap.* final arrays satisfy max-heap property', () => {
    const problems: string[] = [];
    for (const key of ['heap.insert', 'heap.extract', 'heap.heapify', 'sort.heap']) {
      const gen = getSimulation(key)!;
      const steps = gen.generate({ kind: 'heap', data: defaultData(gen) });
      const aN = lastArr(steps[steps.length - 1]);
      if (!isMaxHeap(aN)) problems.push(`${key}: final array NOT max-heap: [${aN.join(',')}]`);
      else console.log(`${key}: final=[${aN.join(',')}] is max-heap OK`);
    }
    expect(problems).toEqual([]);
  });

  it('A4: bst traversals visit all nodes; inorder sorted; others permutation of keys', () => {
    const keys = [50, 30, 70, 20, 40, 60, 80];
    const problems: string[] = [];
    const expectMap: Record<string, number[]> = {
      'tree.bst-preorder': [50, 30, 20, 40, 70, 60, 80],
      'tree.bst-inorder': [20, 30, 40, 50, 60, 70, 80],
      'tree.bst-postorder': [20, 40, 30, 60, 80, 70, 50],
      'tree.bst-levelorder': [50, 30, 70, 20, 40, 60, 80],
    };
    for (const [key, expected] of Object.entries(expectMap)) {
      const gen = getSimulation(key)!;
      const steps = gen.generate({ kind: 'tree', data: { keys, value: 25 } });
      // thứ tự thăm nằm trong explanation "Thăm nút X" / annotation order
      const visited: number[] = [];
      for (const s of steps) {
        const m = /thăm nút (\d+)/.exec(s.explanation) || /visit (\d+)/i.exec(s.explanation);
        if (m) visited.push(Number(m[1]));
      }
      if (visited.length !== keys.length) { problems.push(`${key}: visited ${visited.length}/7 nodes (${visited.join(',')})`); continue; }
      if (visited.join(',') !== expected.join(',')) problems.push(`${key}: order [${visited}] != expected [${expected}]`);
      else console.log(`${key}: [${visited.join(',')}] OK`);
    }
    expect(problems).toEqual([]);
  });

  it('A5: avl-insert final tree satisfies BST + AVL (|bf|<=1) for several inputs incl. rotation cases', () => {
    const problems: string[] = [];
    const cases: Array<{ keys: number[]; value: number }> = [
      { keys: [50, 30, 70, 20, 40], value: 10 },   // LL
      { keys: [20, 10, 30, 35], value: 40 },        // RR
      { keys: [50, 30, 70, 20, 40, 60, 80], value: 25 }, // no rotation
      { keys: [30, 10, 50, 40], value: 45 },        // RL-ish
    ];
    for (const c of cases) {
      const gen = getSimulation('tree.avl-insert')!;
      const steps = gen.generate({ kind: 'tree', data: { keys: c.keys, value: c.value } });
      const last = steps[steps.length - 1];
      const els = last.structure.elements;
      const links = last.structure.links ?? [];
      const byId = new Map(els.map((e) => [e.id, e]));
      const children = new Map<string, string[]>();
      const hasParent = new Set<string>();
      for (const l of links) { (children.get(l.from) ?? children.set(l.from, []).get(l.from)!).push(l.to); hasParent.add(l.to); }
      const root = els.find((e) => !hasParent.has(e.id));
      if (!root) { problems.push(`avl ${JSON.stringify(c)}: no root`); continue; }
      // BST + AVL check
      let ok = true;
      const reasons: string[] = [];
      const walk = (id: string | undefined, lo: number, hi: number): { h: number } => {
        if (!id) return { h: 0 };
        const e = byId.get(id)!;
        const v = Number(e.label);
        if (v <= lo || v >= hi) { ok = false; reasons.push(`BST violated at ${v} in (${lo},${hi})`); }
        const kids = children.get(id) ?? [];
        const lh = walk(kids[0], lo, v).h;
        const rh = walk(kids[1], v, hi).h;
        if (Math.abs(lh - rh) > 1) { ok = false; reasons.push(`AVL bf=${lh - rh} at node ${v}`); }
        return { h: 1 + Math.max(lh, rh) };
      };
      walk(root.id, -Infinity, Infinity);
      if (!ok) problems.push(`avl insert ${c.value} into [${c.keys}]: ${reasons.join('; ')}`);
      else console.log(`avl [${c.keys}] +${c.value}: BST+AVL OK (steps=${steps.length})`);
    }
    expect(problems).toEqual([]);
  });

  it('A6: hash.* respects key % tableSize chaining', () => {
    const problems: string[] = [];
    for (const key of ['hash.insert', 'hash.search', 'hash.delete']) {
      const gen = getSimulation(key)!;
      const steps = gen.generate({ kind: 'hash', data: defaultData(gen) });
      const last = steps[steps.length - 1];
      const els = last.structure.elements;
      const links = last.structure.links ?? [];
      // bucket element ids vs node ids
      const bucketEls = els.filter((e) => (e.meta as any)?.bucket !== undefined || /^bucket/.test(e.id));
      const nodeEls = els.filter((e) => !bucketEls.includes(e));
      const bucketOf = new Map<string, string>();
      for (const l of links) bucketOf.set(l.to, l.from);
      for (const n of nodeEls) {
        const v = Number(n.label);
        if (Number.isNaN(v)) continue;
        const b = bucketOf.get(n.id);
        if (b === undefined) continue;
        const bv = Number(b.replace(/[^0-9]/g, ''));
        if (!Number.isNaN(bv) && v % 11 !== bv % 11) problems.push(`${key}: key ${v} sits in bucket ${bv} (expected ${v % 11})`);
      }
      console.log(`${key}: steps=${steps.length}, nodes=${nodeEls.length}, buckets=${bucketEls.length} checked`);
    }
    expect(problems).toEqual([]);
  });

  it('A7: search.linear/binary find correct index; search miss reported', () => {
    const lin = getSimulation('search.linear')!;
    const linSteps = lin.generate({ kind: 'array', data: { target: 8, values: [5, 3, 8, 1, 9, 2] } });
    const linLast = linSteps[linSteps.length - 1].explanation;
    expect(linLast).toContain('2');
    const bin = getSimulation('search.binary')!;
    const binSteps = bin.generate({ kind: 'array', data: { target: 19, values: [2, 5, 8, 12, 19, 23] } });
    const binLast = binSteps[binSteps.length - 1].explanation;
    expect(binLast).toContain('4');
    // miss case
    const miss = bin.generate({ kind: 'array', data: { target: 100, values: [2, 5, 8, 12, 19, 23] } });
    expect(miss[miss.length - 1].explanation).toMatch(/không|100/);
    console.log('search.linear found idx2 OK; search.binary found idx4 OK; binary miss handled:', miss[miss.length - 1].explanation.slice(0, 80));
  });
});
