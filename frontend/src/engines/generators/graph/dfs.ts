// engines/generators/graph/dfs.ts — Đồ thị DFS (dùng stack, không đệ quy — SDD §4.7.15, §4.6.5)
import type { ElementStatus, InputConfig, InputSchema, SimulationGenerator } from '../../core/types';
import { adjacency, buildGenerator, buildGraphEdges, graphStructure, parseGraphParams, Trace, validateGraphParams } from '../helpers';

const PSEUDOCODE = [
  'procedure DFS(G, s)',
  '  visited ← {}; parent ← {}; stack ← [s]',
  '  while stack ≠ rỗng do',
  '    u ← pop',
  '    if visited[u] = false then',
  '      visited[u] ← true',
  '      for v kề u (ngược) do',
  '        if visited[v] = false then parent[v] ← u; push(v)',
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

export function createDfsGenerator(): SimulationGenerator {
  return buildGenerator('graph.dfs', SCHEMA, PSEUDOCODE, {
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
      trace.vars.stack = String(g.source);
      trace.vars.parent = '';
      trace.push({
        line: 1,
        explanation: `Bắt đầu: đồ thị ${g.vertices} đỉnh ${edges.length} cạnh, DFS từ đỉnh ${g.source}.`,
        structure: graphStructure(g, edges, statuses),
        annotations: [`source=${g.source}, V=${g.vertices}, E=${edges.length}`],
      });

      const stack: number[] = [g.source];
      const visited = new Set<number>();
      const parent: Record<number, number> = {};

      while (stack.length > 0) {
        const u = stack.pop() as number;
        trace.vars.u = u;
        trace.vars.stack = stack.join(',');
        trace.push({
          line: 4,
          explanation: `Pop u = ${u} từ ngăn xếp.`,
          structure: graphStructure(g, edges, statuses),
          annotations: [`stack=[${stack.join(',')}]`],
        });
        if (!visited.has(u)) {
          visited.add(u);
          order[u] = orderCounter++;
          statuses.nodes[u] = 'done';
          trace.push({
            line: 6,
            explanation: `visited[${u}] = false → đánh dấu thăm (order ${order[u]}).`,
            structure: graphStructure(g, edges, statuses),
            annotations: [`order ${order[u]}: thăm ${u}`],
          });

          // Push ngược để pop theo thứ tự tăng dần (tương đương DFS đệ quy).
          for (let i = adj[u].length - 1; i >= 0; i--) {
            const [v] = adj[u][i];
            if (!visited.has(v)) {
              parent[v] = u;
              statuses.edges[edgeKey(u, v)] = 'active';
              statuses.nodes[v] = 'active';
              stack.push(v);
              trace.vars.stack = stack.join(',');
              trace.vars.parent = Object.keys(parent).map((k) => `${k}←${parent[Number(k)]}`).join(',');
              trace.push({
                line: 8,
                explanation: `Đỉnh kề ${v} chưa thăm → parent[${v}] = ${u}, push(${v}) vào ngăn xếp.`,
                structure: graphStructure(g, edges, statuses),
                annotations: [`parent[${v}]=${u}`, `stack=[${stack.join(',')}]`],
              });
              statuses.edges[edgeKey(u, v)] = 'default';
              statuses.nodes[v] = 'default';
            }
          }
        } else {
          trace.push({
            line: 5,
            explanation: `Đỉnh ${u} đã được thăm trước đó → bỏ qua.`,
            structure: graphStructure(g, edges, statuses),
            annotations: [`visited[${u}]=true`],
          });
        }
      }

      const unreachable: number[] = [];
      for (let i = 0; i < g.vertices; i++) {
        if (!visited.has(i)) {
          unreachable.push(i);
          statuses.nodes[i] = 'muted';
        }
      }
      if (unreachable.length > 0) {
        trace.push({
          line: 9,
          explanation: `Các đỉnh [${unreachable.join(', ')}] không thể đến được từ đỉnh nguồn ${g.source} (thuộc thành phần liên thông khác).`,
          structure: graphStructure(g, edges, statuses),
          annotations: [`unreachable: ${unreachable.join(', ')}`],
        });
      }

      trace.push({
        line: 9,
        explanation: `Kết thúc: DFS hoàn tất, thứ tự thăm [${Object.keys(order).sort((a, b) => order[Number(a)] - order[Number(b)]).join(', ')}].`,
        structure: graphStructure(g, edges, statuses),
        annotations: [`thứ tự thăm: ${Object.keys(order).sort((a, b) => order[Number(a)] - order[Number(b)]).join(' → ')}`],
      });
      return trace.steps;
    },
  });
}
