// engines/generators/graph/dijkstra.ts — Dijkstra đường đi ngắn nhất (SDD §4.7.15, §4.6.5)
import type { ElementStatus, InputConfig, InputSchema, SimulationGenerator } from '../../core/types';
import { adjacency, buildGenerator, buildGraphEdges, graphStructure, parseGraphParams, Trace, validateGraphParams } from '../helpers';

const PSEUDOCODE = [
  'procedure Dijkstra(G, s)',
  '  d[s] ← 0; với mọi v ≠ s: d[v] ← ∞',
  '  PQ ← {(0, s)}',
  '  while PQ ≠ rỗng do',
  '    u ← extract-min',
  '    for cạnh (u, v, w) do',
  '      if d[u] + w < d[v] then',
  '        d[v] ← d[u] + w; parent[v] ← u; decrease-key',
  '  end procedure',
];

const SCHEMA: InputSchema = {
  kind: 'graph',
  fields: [
    { name: 'preset', type: 'select', label: 'Mẫu đồ thị', options: [
      { label: 'Đường đi (path)', value: 'path' },
      { label: 'Chu trình (cycle)', value: 'cycle' },
      { label: 'Đầy đủ (complete)', value: 'complete' },
      { label: 'Hai phía (bipartite)', value: 'bipartite' },
      { label: 'Lưới (grid)', value: 'grid' },
      { label: 'Tự sinh (custom)', value: 'custom' },
    ], default: 'custom', description: 'Mẫu đồ thị' },
    { name: 'directed', type: 'bool', label: 'Có hướng', default: true, description: 'Đồ thị có hướng hay vô hướng' },
    { name: 'weighted', type: 'bool', label: 'Có trọng số', default: true, description: 'Các cạnh có trọng số hay không' },
    { name: 'vertices', type: 'int', label: 'Số đỉnh', min: 2, max: 50, default: 6, description: 'Số đỉnh của đồ thị' },
    { name: 'edges', type: 'int', label: 'Số cạnh', min: 1, max: 200, default: 8, description: 'Số cạnh khi dùng preset custom' },
    { name: 'source', type: 'int', label: 'Đỉnh nguồn', min: 0, max: 49, default: 0, description: 'Đỉnh bắt đầu' },
    { name: 'target', type: 'int', label: 'Đỉnh đích (tùy chọn)', min: 0, max: 49, default: null, description: 'null = tính đến mọi đỉnh' },
  ],
};

export function createDijkstraGenerator(): SimulationGenerator {
  return buildGenerator('graph.dijkstra', SCHEMA, PSEUDOCODE, {
    validate(input: InputConfig) {
      return { ok: true, errors: validateGraphParams(input.data) };
    },

    generate(input: InputConfig) {
      const g = parseGraphParams(input.data);
      const edges = buildGraphEdges(g);
      const adj = adjacency(g, edges);
      const trace = new Trace();
      const statuses = { nodes: {} as Record<number, ElementStatus>, edges: {} as Record<string, ElementStatus> };

      const edgeKey = (u: number, v: number): string => (g.directed ? `${u}-${v}` : `${Math.min(u, v)}-${Math.max(u, v)}`);
      const dist: Array<number | null> = Array.from({ length: g.vertices }, () => null);
      const parent: Record<number, number> = {};

      trace.vars.source = g.source;
      trace.vars.target = g.target;
      trace.vars.d = '';
      trace.vars.parent = '';
      trace.push({
        line: 1,
        explanation: `Bắt đầu: đồ thị ${g.vertices} đỉnh ${edges.length} cạnh (có trọng số), Dijkstra từ đỉnh ${g.source}.`,
        structure: graphStructure(g, edges, statuses, dist),
        annotations: [`source=${g.source}`],
      });

      for (let v = 0; v < g.vertices; v++) dist[v] = v === g.source ? 0 : null;
      trace.vars.d = dist.map((d) => (d === null ? '∞' : String(d))).join(',');
      trace.push({
        line: 2,
        explanation: `Khởi tạo: d[${g.source}] = 0, các đỉnh còn lại d[v] = ∞.`,
        structure: graphStructure(g, edges, statuses, dist),
        annotations: [`d = [${dist.map((d) => (d === null ? '∞' : d)).join(', ')}]`],
      });
      trace.push({
        line: 3,
        explanation: `PQ chứa phần tử khởi tạo (0, ${g.source}).`,
        structure: graphStructure(g, edges, statuses, dist),
        annotations: [`PQ = {(0, ${g.source})}`],
      });

      const unvisited = new Set<number>(Array.from({ length: g.vertices }, (_, i) => i));
      let guard = 0;

      while (unvisited.size > 0 && guard < g.vertices * g.vertices + 4) {
        guard++;
        // extract-min: đỉnh chưa thăm có d nhỏ nhất (bỏ qua ∞ nếu còn đỉnh hữu hạn)
        let u = -1;
        let best: number | null = null;
        for (const v of unvisited) {
          const dv = dist[v];
          if (dv === null) continue;
          if (best === null || dv < best) { best = dv; u = v; }
        }
        if (u === -1) break;
        unvisited.delete(u);
        statuses.nodes[u] = 'done';
        trace.push({
          line: 5,
          explanation: `extract-min: chọn u = ${u} có d[${u}] = ${dist[u]} nhỏ nhất trong các đỉnh chưa thăm.`,
          structure: graphStructure(g, edges, statuses, dist),
          annotations: [`d[${u}]=${dist[u]} → đã chốt`],
        });

        for (const [v, w] of adj[u]) {
          if (!unvisited.has(v)) continue;
          statuses.edges[edgeKey(u, v)] = 'active';
          trace.push({
            line: 6,
            explanation: `Xét cạnh (${u}, ${v}) với trọng số w = ${w}.`,
            structure: graphStructure(g, edges, statuses, dist),
            annotations: [`d[${u}] + w = ${dist[u]} + ${w} = ${(dist[u] ?? 0) + w}`],
          });

          const relax = dist[u] as number + w;
          const oldD = dist[v];
          trace.stats.comparisons++;
          trace.push({
            line: 7,
            explanation: `So sánh d[${u}] + w = ${relax} và d[${v}] = ${oldD === null ? '∞' : oldD}.`,
            structure: graphStructure(g, edges, statuses, dist),
            annotations: [`${relax} < ${oldD === null ? '∞' : oldD}?`],
          });
          if (oldD === null || relax < oldD) {
            dist[v] = relax;
            parent[v] = u;
            trace.stats.writes++;
            statuses.nodes[v] = 'swap';
            trace.vars.d = dist.map((d) => (d === null ? '∞' : String(d))).join(',');
            trace.vars.parent = Object.keys(parent).map((k) => `${k}←${parent[Number(k)]}`).join(',');
            trace.push({
              line: 8,
              explanation: `${relax} < ${oldD === null ? '∞' : oldD} → đúng, cập nhật d[${v}] = ${relax}, parent[${v}] = ${u}.`,
              structure: graphStructure(g, edges, statuses, dist),
              annotations: [`d[${v}]=${relax}, parent[${v}]=${u}`],
            });
            statuses.nodes[v] = 'default';
          } else {
            trace.push({
              line: 7,
              explanation: `${relax} < ${oldD} → sai, giữ nguyên d[${v}] = ${oldD}.`,
              structure: graphStructure(g, edges, statuses, dist),
            });
          }
          statuses.edges[edgeKey(u, v)] = 'default';
        }
      }

      // Cây đường đi ngắn nhất: tô các cạnh parent thành done
      for (const v of Object.keys(parent)) {
        const child = Number(v);
        const p = parent[child];
        statuses.edges[edgeKey(p, child)] = 'done';
      }
      trace.push({
        line: 9,
        explanation: `Kết thúc: đường đi ngắn nhất từ ${g.source}: d = [${dist.map((d) => (d === null ? '∞' : d)).join(', ')}].`,
        structure: graphStructure(g, edges, statuses, dist),
        annotations: [`d = [${dist.map((d) => (d === null ? '∞' : d)).join(', ')}]`],
      });
      return trace.steps;
    },
  });
}
