// engines/generators/graph/bfs.ts — Đồ thị BFS (SDD §4.7.15, §4.6.5)
import type { ElementStatus, InputConfig, InputSchema, SimulationGenerator } from '../../core/types';
import { adjacency, buildGenerator, buildGraphEdges, graphStructure, parseGraphParams, Trace, validateGraphParams } from '../helpers';

const PSEUDOCODE = [
  'procedure BFS(G, s)',
  '  visited ← {}; parent ← {}; queue ← [s]',
  '  visited[s] ← true',
  '  while queue ≠ rỗng do',
  '    u ← dequeue',
  '    for v kề u do',
  '      if visited[v] = false then',
  '        visited[v] ← true; parent[v] ← u',
  '        enqueue(v)',
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
    { name: 'weighted', type: 'bool', label: 'Có trọng số', default: false, description: 'Các cạnh có trọng số hay không' },
    { name: 'vertices', type: 'int', label: 'Số đỉnh', min: 2, max: 50, default: 6, description: 'Số đỉnh của đồ thị' },
    { name: 'edges', type: 'int', label: 'Số cạnh', min: 1, max: 200, default: 8, description: 'Số cạnh khi dùng preset custom' },
    { name: 'source', type: 'int', label: 'Đỉnh nguồn', min: 0, max: 49, default: 0, description: 'Đỉnh bắt đầu duyệt' },
  ],
};

export function createBfsGenerator(): SimulationGenerator {
  return buildGenerator('graph.bfs', SCHEMA, PSEUDOCODE, {
    validate(input: InputConfig) {
      const errors = validateGraphParams(input.data);
      return { ok: errors.length === 0, errors };
    },

    generate(input: InputConfig) {
      const g = parseGraphParams(input.data);
      const edges = buildGraphEdges(g);
      const adj = adjacency(g, edges);
      const trace = new Trace();
      const statuses = { nodes: {} as Record<number, ElementStatus>, edges: {} as Record<string, ElementStatus> };
      const order: Record<number, number> = {};
      let orderCounter = 1;

      const edgeKey = (u: number, v: number): string => (g.directed ? `${u}-${v}` : `${Math.min(u, v)}-${Math.max(u, v)}`);

      trace.vars.source = g.source;
      trace.vars.queue = String(g.source);
      trace.vars.parent = '';
      trace.push({
        line: 1,
        explanation: `Bắt đầu: đồ thị ${g.vertices} đỉnh ${edges.length} cạnh, BFS từ đỉnh ${g.source}.`,
        structure: graphStructure(g, edges, statuses),
        annotations: [`source=${g.source}, V=${g.vertices}, E=${edges.length}`],
      });

      const queue: number[] = [g.source];
      const visited = new Set<number>([g.source]);
      const parent: Record<number, number> = {};
      order[g.source] = orderCounter++;
      statuses.nodes[g.source] = 'highlight';
      trace.push({
        line: 3,
        explanation: `Đưa ${g.source} vào hàng đợi; visited[${g.source}] = true (order ${order[g.source]}).`,
        structure: graphStructure(g, edges, statuses),
        annotations: [`queue=[${queue.join(',')}], order: ${order[g.source]}`],
      });

      while (queue.length > 0) {
        const u = queue.shift() as number;
        trace.vars.u = u;
        statuses.nodes[u] = 'done';
        trace.push({
          line: 5,
          explanation: `Dequeue u = ${u} → thăm đỉnh ${u}.`,
          structure: graphStructure(g, edges, statuses),
          annotations: [`queue=[${queue.join(',')}]`],
        });

        for (const [v] of adj[u]) {
          statuses.edges[edgeKey(u, v)] = 'active';
          statuses.nodes[v] = statuses.nodes[v] ?? 'active';
          trace.push({
            line: 6,
            explanation: `Xét đỉnh kề v = ${v} của u = ${u}.`,
            structure: graphStructure(g, edges, statuses),
            annotations: [`cạnh (${u}, ${v})`],
          });
          if (!visited.has(v)) {
            visited.add(v);
            parent[v] = u;
            order[v] = orderCounter++;
            statuses.edges[edgeKey(u, v)] = 'done';
            statuses.nodes[v] = 'done';
            queue.push(v);
            trace.vars.queue = queue.join(',');
            trace.vars.parent = Object.keys(parent).map((k) => `${k}←${parent[Number(k)]}`).join(',');
            trace.push({
              line: 8,
              explanation: `visited[${v}] = false → đánh dấu thăm (order ${order[v]}), parent[${v}] = ${u}, enqueue(${v}).`,
              structure: graphStructure(g, edges, statuses),
              annotations: [`order ${order[v]}: thăm ${v}`, `parent[${v}]=${u}`, `queue=[${queue.join(',')}]`],
            });
          } else {
            statuses.edges[edgeKey(u, v)] = 'default';
            trace.push({
              line: 7,
              explanation: `Đỉnh ${v} đã thăm rồi → bỏ qua.`,
              structure: graphStructure(g, edges, statuses),
              annotations: [`visited[${v}]=true`],
            });
          }
        }
      }

      trace.push({
        line: 10,
        explanation: `Kết thúc: BFS hoàn tất, thứ tự thăm [${Object.keys(order).sort((a, b) => order[Number(a)] - order[Number(b)]).join(', ')}].`,
        structure: graphStructure(g, edges, statuses),
        annotations: [`thứ tự thăm: ${Object.keys(order).sort((a, b) => order[Number(a)] - order[Number(b)]).join(' → ')}`],
      });
      return trace.steps;
    },
  });
}
