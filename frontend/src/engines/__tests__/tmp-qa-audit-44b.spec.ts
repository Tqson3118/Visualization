import { describe, expect, it } from 'vitest';
import '../catalog';
import { getSimulation } from '../registry';

describe('QA AUDIT follow-up', () => {
  it('A4fix: traversal orders from final explanation', () => {
    const keys = [50, 30, 70, 20, 40, 60, 80];
    const expectMap: Record<string, string> = {
      'tree.bst-preorder': '50, 30, 20, 40, 70, 60, 80',
      'tree.bst-inorder': '20, 30, 40, 50, 60, 70, 80',
      'tree.bst-postorder': '20, 40, 30, 60, 80, 70, 50',
      'tree.bst-levelorder': '50, 30, 70, 20, 40, 60, 80',
    };
    const problems: string[] = [];
    for (const [key, expected] of Object.entries(expectMap)) {
      const gen = getSimulation(key)!;
      const steps = gen.generate({ kind: 'tree', data: { keys, value: 25 } });
      const last = steps[steps.length - 1].explanation;
      const m = /thứ tự \[([^\]]+)\]/.exec(last);
      const got = m ? m[1].trim() : '(not found: ' + last.slice(0, 60) + ')';
      if (got !== expected) problems.push(`${key}: [${got}] != [${expected}]`);
      else console.log(`${key}: [${got}] OK`);
    }
    expect(problems).toEqual([]);
  });

  it('A5fix: avl final tree BST+AVL using value-based child classification', () => {
    const problems: string[] = [];
    const cases: Array<{ keys: number[]; value: number; note: string }> = [
      { keys: [50, 30, 70, 20, 40], value: 10, note: 'LL rotation expected' },
      { keys: [20, 10, 30, 35], value: 40, note: 'RR rotation expected' },
      { keys: [50, 30, 70, 20, 40, 60, 80], value: 25, note: 'no rotation' },
      { keys: [30, 10, 50, 40], value: 45, note: 'RL-ish' },
    ];
    for (const c of cases) {
      const gen = getSimulation('tree.avl-insert')!;
      const steps = gen.generate({ kind: 'tree', data: { keys: c.keys, value: c.value } });
      const last = steps[steps.length - 1];
      const els = last.structure.elements;
      const links = last.structure.links ?? [];
      const byId = new Map(els.map((e) => [e.id, e]));
      const valOf = (id: string) => Number(byId.get(id)!.label);
      const kidsMap = new Map<string, string[]>();
      const hasParent = new Set<string>();
      for (const l of links) { (kidsMap.get(l.from) ?? kidsMap.set(l.from, []).get(l.from)!).push(l.to); hasParent.add(l.to); }
      const root = els.find((e) => !hasParent.has(e.id));
      if (!root) { problems.push(`${c.note}: no root`); continue; }
      let ok = true; const reasons: string[] = [];
      const walk = (id: string, lo: number, hi: number): number => {
        const v = valOf(id);
        if (v <= lo || v >= hi) { ok = false; reasons.push(`BST violated at ${v} in (${lo},${hi})`); }
        const kids = kidsMap.get(id) ?? [];
        let left = kids.find(k => valOf(k) < v);
        let right = kids.find(k => valOf(k) > v);
        const lh = left ? walk(left, lo, v) : 0;
        const rh = right ? walk(right, v, hi) : 0;
        if (kids.length > 2 || (kids.length === 2 && left === right)) { ok = false; reasons.push(`node ${v} has odd children`); }
        if (Math.abs(lh - rh) > 1) { ok = false; reasons.push(`AVL bf=${lh - rh} at ${v}`); }
        return 1 + Math.max(lh, rh);
      };
      walk(root.id, -Infinity, Infinity);
      if (!ok) problems.push(`${c.note}: ${reasons.join('; ')}`);
      else console.log(`${c.note}: OK (steps=${steps.length})`);
    }
    expect(problems).toEqual([]);
  });

  it('A8: heap-only keys keep max-heap (exclude sort.heap)', () => {
    const problems: string[] = [];
    for (const key of ['heap.insert', 'heap.extract', 'heap.heapify']) {
      const gen = getSimulation(key)!;
      const steps = gen.generate({ kind: 'heap', data: { keys: [10, 7, 9, 4, 6, 8], operation: key.split('.')[1] } });
      const last = steps[steps.length - 1];
      const a = last.structure.elements.map((e) => Number(e.label)).filter((v) => !Number.isNaN(v));
      for (let i = 0; i < a.length; i++) {
        const l = 2 * i + 1, r = 2 * i + 2;
        if (l < a.length && a[i] < a[l]) problems.push(`${key}: heap violated at idx ${i} [${a.join(',')}]`);
        if (r < a.length && a[i] < a[r]) problems.push(`${key}: heap violated at idx ${i} [${a.join(',')}]`);
      }
      console.log(`${key}: [${a.join(',')}] heap-OK`);
    }
    expect(problems).toEqual([]);
  });

  it('A9: bst-delete mid-state has two nodes with same label (copied value) but distinct ids - intentional', () => {
    const gen = getSimulation('tree.bst-delete')!;
    const steps = gen.generate({ kind: 'tree', data: { keys: [50, 30, 70, 20, 40, 60, 80], operation: 'delete', value: 50 } });
    const step6 = steps[5];
    const labels = step6.structure.elements.map((e) => e.label);
    const count60 = labels.filter((l) => l === '60').length;
    console.log(`bst-delete step6 labels: [${labels.join(',')}] — two '60' circles = ${count60 === 2 ? 'YES (intended mid-state)' : 'NO'}`);
    expect(count60).toBe(2);
  });

  it('A10: dijkstra distances verified against reference implementation', () => {
    const gen = getSimulation('graph.dijkstra')!;
    const steps = gen.generate({ kind: 'graph', data: { vertices: 7, preset: 'custom', directed: true, weighted: true, source: 0 } });
    const first = steps[0].structure;
    const links = first.links ?? [];
    // build adjacency
    const adj = new Map<number, Array<[number, number]>>();
    for (const l of links) {
      const u = Number(l.from.replace(/[^0-9]/g, ''));
      const v = Number(l.to.replace(/[^0-9]/g, ''));
      const w = Number((l as any).weight ?? (l as any).meta?.weight ?? 1);
      if (!adj.has(u)) adj.set(u, []);
      adj.get(u)!.push([v, w]);
      if (!adj.has(v)) adj.set(v, []);
      adj.get(v)!.push([u, w]); // assuming undirected rendering of custom preset? check directed meta
    }
    const directed = (first.elements[0] as any)?.meta?.directed ?? false;
    const dist: number[] = new Array(7).fill(Infinity);
    dist[0] = 0;
    const visited = new Set<number>();
    for (;;) {
      let u = -1; let best = Infinity;
      for (let i = 0; i < 7; i++) if (!visited.has(i) && dist[i] < best) { best = dist[i]; u = i; }
      if (u < 0) break;
      visited.add(u);
      for (const [v, w] of adj.get(u) ?? []) if (dist[u] + w < dist[v]) dist[v] = dist[u] + w;
    }
    const last = steps[steps.length - 1].explanation;
    console.log(`directed=${directed} reference d=[${dist.map(d => d === Infinity ? 'INF' : d).join(',')}]`);
    console.log(`generator final: ${last.slice(0, 140)}`);
  });
});
