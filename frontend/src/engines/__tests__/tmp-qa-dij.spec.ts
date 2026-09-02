import { describe, expect, it } from 'vitest';
import '../catalog';
import { getSimulation } from '../registry';

describe('A10-fix: dijkstra verified with real weights', () => {
  it('recompute reference dijkstra from actual links', () => {
    const gen = getSimulation('graph.dijkstra')!;
    const input = { kind: 'graph', data: { preset: 'custom', directed: true, weighted: true, vertices: 7, edges: 10, source: 0 } };
    const steps = gen.generate(input);
    const first = steps[0].structure;
    console.log('LINKS[0]:', JSON.stringify(first.links));
    console.log('EL-META[0]:', JSON.stringify(first.elements.slice(0, 3)));
    const links = first.links ?? [];
    const directed = (first.elements[0] as any)?.meta?.directed ?? false;
    const adj = new Map<number, Array<[number, number]>>();
    const get = (u: number) => { if (!adj.has(u)) adj.set(u, []); return adj.get(u)!; };
    for (const l of links) {
      const u = Number(String(l.from).replace(/[^0-9]/g, ''));
      const v = Number(String(l.to).replace(/[^0-9]/g, ''));
      const w = Number((l as any).weight ?? (l as any).meta?.weight ?? NaN);
      if (Number.isNaN(w)) { console.log('NO WEIGHT on', l.from, '->', l.to); }
      get(u).push([v, w]);
      if (!directed) get(v).push([u, w]);
    }
    const n = 7;
    const dist: number[] = new Array(n).fill(Infinity);
    dist[0] = 0;
    const visited = new Set<number>();
    for (;;) {
      let u = -1; let best = Infinity;
      for (let i = 0; i < n; i++) if (!visited.has(i) && dist[i] < best) { best = dist[i]; u = i; }
      if (u < 0) break;
      visited.add(u);
      for (const [v, w] of get(u)) if (dist[u] + w < dist[v]) dist[v] = dist[u] + w;
    }
    const last = steps[steps.length - 1].explanation;
    const m = /d = \[([^\]]+)\]/.exec(last);
    console.log('directed=' + directed);
    console.log('reference d=[', dist.map(d => (d === Infinity ? 'INF' : d)).join(', '), ']');
    console.log('generator d=[' + (m ? m[1] : '?') + ']');
    const genD = m ? m[1].split(',').map(s => Number(s.trim())) : [];
    const refD = dist.map(d => (d === Infinity ? -1 : d));
    const match = genD.length === refD.length && genD.every((v, i) => Math.abs(v - refD[i]) < 1e-9);
    console.log('MATCH: ' + match);
    expect(match).toBe(true);
  });
});
