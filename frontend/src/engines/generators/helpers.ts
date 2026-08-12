// engines/generators/helpers.ts — tiện ích dùng chung cho mọi generator (SDD §4.16)
//
// - buildGenerator: tạo SimulationGenerator từ metadata CATALOG (đọc bằng key) —
//   tránh lặp title/category/level/complexity trong từng file generator.
// - Trace: bộ tích lũy bước + thống kê (comparisons/swaps/writes) theo SDD §4.3.
// - Hàm dựng Structure (array/tree/graph/heap), RNG seed cố định (SDD §4.8: seed=42, xorshift)
//   và bộ parse/validate input dùng chung (SDD §4.14).

import type {
  Element,
  ElementStatus,
  InputConfig,
  InputSchema,
  Link,
  SimulationGenerator,
  Step,
  Structure,
} from '../core/types';
import { getCatalogMeta } from '../catalog';

// ── buildGenerator: lấy metadata từ CATALOG bằng key ─────────────────────────

export interface GeneratorImpl {
  generate(input: InputConfig): Step[];
  validate(input: InputConfig): { ok: boolean; errors: string[] };
}

export function buildGenerator(
  key: string,
  inputSchema: InputSchema,
  pseudocode: string[],
  impl: GeneratorImpl,
): SimulationGenerator {
  const meta = getCatalogMeta(key);
  if (!meta) {
    throw new Error(`catalog: thiếu metadata cho key '${key}'`);
  }
  return {
    key: meta.key,
    title: meta.title,
    category: meta.category,
    dataStructure: meta.dataStructure,
    level: meta.level,
    complexity: meta.complexity,
    inputSchema,
    pseudocode,
    generate: impl.generate,
    validate: impl.validate,
  };
}

// ── Trace: tích lũy bước + thống kê ─────────────────────────────────────────

export interface StatsAcc {
  comparisons: number;
  swaps: number;
  writes: number;
}

export interface PushOpts {
  line: number;
  explanation: string;
  structure: Structure;
  highlights?: string[];
  annotations?: string[];
  vars?: Record<string, string | number | boolean | null>;
}

export class Trace {
  readonly steps: Step[] = [];
  readonly stats: StatsAcc = { comparisons: 0, swaps: 0, writes: 0 };
  readonly vars: Record<string, string | number | boolean | null> = {};

  push(opts: PushOpts): void {
    this.steps.push({
      index: this.steps.length,
      structure: opts.structure,
      explanation: opts.explanation,
      pseudocodeLine: opts.line,
      highlights: opts.highlights ?? [],
      annotations: opts.annotations ?? [],
      variables: opts.vars ?? { ...this.vars },
      stats: {
        comparisons: this.stats.comparisons,
        swaps: this.stats.swaps,
        writes: this.stats.writes,
      },
      version: 1,
    });
  }
}

// ── Structure mảng ──────────────────────────────────────────────────────────

export type StatusMap = Record<number, ElementStatus>;

export function arrayStructure(values: number[], statuses: StatusMap = {}): Structure {
  return {
    kind: 'array',
    elements: values.map((v, i) => ({
      id: `cell:${i}`,
      label: String(v),
      status: statuses[i] ?? 'default',
    })),
    links: [],
  };
}

// ── RNG xorshift32 seed cố định (SDD §4.8 N6: seed=42) ──────────────────────

export function createRng(seed: number): () => number {
  let s = seed >>> 0 || 1;
  return () => {
    s ^= s << 13;
    s >>>= 0;
    s ^= s >> 17;
    s ^= s << 5;
    s >>>= 0;
    return s / 0xffffffff;
  };
}

// ── Parse + validate input mảng (SDD §4.14 — Mảng chung + Mảng ngẫu nhiên) ──

export interface ArrayParams {
  values: number[];
  size: number;
  minValue: number;
  maxValue: number;
  allowDuplicates: boolean;
  preset: string;
}

const PRESETS = ['random', 'sorted-asc', 'sorted-desc', 'nearly-sorted', 'all-equal', 'custom'];

export function asRecord(data: unknown): Record<string, unknown> {
  return data !== null && typeof data === 'object' && !Array.isArray(data)
    ? (data as Record<string, unknown>)
    : {};
}

export function intField(data: Record<string, unknown>, name: string, dflt: number): number {
  const v = data[name];
  return typeof v === 'number' && Number.isInteger(v) ? v : dflt;
}

export function intArrayField(data: Record<string, unknown>, name: string, dflt: number[]): number[] {
  const v = data[name];
  if (Array.isArray(v) && v.length > 0 && v.every((x) => typeof x === 'number' && Number.isInteger(x))) {
    return (v as number[]).slice();
  }
  return dflt.slice();
}

export function boolField(data: Record<string, unknown>, name: string, dflt: boolean): boolean {
  const v = data[name];
  return typeof v === 'boolean' ? v : dflt;
}

export function strField(data: Record<string, unknown>, name: string, dflt: string): string {
  const v = data[name];
  return typeof v === 'string' ? v : dflt;
}

/** Sinh mảng theo các field random (SDD §4.14) — deterministic nhờ seed cố định. */
function generateRandomValues(p: ArrayParams): number[] {
  const rng = createRng(42);
  if (p.preset === 'all-equal') {
    return Array.from({ length: p.size }, () => p.minValue);
  }
  const span = p.maxValue - p.minValue;
  let out: number[] = [];
  if (p.allowDuplicates) {
    out = Array.from({ length: p.size }, () => p.minValue + Math.floor(rng() * (span + 1)));
  } else {
    const pool = Array.from({ length: span + 1 }, (_, k) => p.minValue + k);
    for (let i = pool.length - 1; i > 0; i--) {
      const j = Math.floor(rng() * (i + 1));
      [pool[i], pool[j]] = [pool[j], pool[i]];
    }
    out = pool.slice(0, p.size);
  }
  if (p.preset === 'sorted-asc') return out.sort((x, y) => x - y);
  if (p.preset === 'sorted-desc') return out.sort((x, y) => y - x);
  if (p.preset === 'nearly-sorted') {
    const sorted = out.sort((x, y) => x - y);
    const swaps = Math.max(1, Math.floor(sorted.length / 10));
    for (let k = 0; k < swaps; k++) {
      const i = k * 2;
      if (i + 1 < sorted.length) [sorted[i], sorted[i + 1]] = [sorted[i + 1], sorted[i]];
    }
    return sorted;
  }
  return out;
}

export function parseArrayParams(data: unknown): ArrayParams {
  const rec = asRecord(data);
  const values = intArrayField(rec, 'values', []);
  const p: ArrayParams = {
    values,
    size: intField(rec, 'size', 15),
    minValue: intField(rec, 'minValue', 0),
    maxValue: intField(rec, 'maxValue', 99),
    allowDuplicates: boolField(rec, 'allowDuplicates', true),
    preset: strField(rec, 'preset', 'random'),
  };
  if (values.length > 0) {
    p.preset = 'custom';
  } else {
    p.values = generateRandomValues(p);
  }
  return p;
}

export function validateArrayParams(data: unknown): string[] {
  const rec = asRecord(data);
  const errors: string[] = [];
  const values = rec.values;
  if (Array.isArray(values) && values.length > 0) {
    if (values.length < 2 || values.length > 100) {
      errors.push(`values: mảng phải có 2–100 phần tử (hiện có ${values.length})`);
    }
    values.forEach((v, i) => {
      if (typeof v !== 'number' || !Number.isInteger(v) || v < -999 || v > 999) {
        errors.push(`values[${i}]=${String(v)} phải là số nguyên trong khoảng -999..999`);
      }
    });
    return errors;
  }
  const size = intField(rec, 'size', 15);
  const minValue = intField(rec, 'minValue', 0);
  const maxValue = intField(rec, 'maxValue', 99);
  if (size < 2 || size > 100) errors.push(`size: phải trong khoảng 2–100 (hiện tại ${size})`);
  if (minValue < -999 || minValue > 999) errors.push(`minValue: phải trong khoảng -999..999 (hiện tại ${minValue})`);
  if (maxValue < -999 || maxValue > 999) errors.push(`maxValue: phải trong khoảng -999..999 (hiện tại ${maxValue})`);
  if (minValue > maxValue) errors.push(`minValue (${minValue}) phải ≤ maxValue (${maxValue})`);
  if (!boolField(rec, 'allowDuplicates', true) && maxValue - minValue + 1 < size) {
    errors.push(`size=${size} vượt số giá trị phân biệt khả dụng ${maxValue - minValue + 1} khi allowDuplicates=false`);
  }
  const preset = strField(rec, 'preset', 'random');
  if (!PRESETS.includes(preset)) {
    errors.push(`preset: phải là một trong ${PRESETS.join(', ')} (hiện tại '${preset}')`);
  }
  return errors;
}

// ── Input mặc định từ schema (dùng trong test) ──────────────────────────────

export function defaultInput(gen: SimulationGenerator): InputConfig {
  const data: Record<string, unknown> = {};
  for (const f of gen.inputSchema.fields) data[f.name] = f.default;
  return { kind: gen.inputSchema.kind, data };
}

// ── Structure cây ────────────────────────────────────────────────────────────

export interface TreeNodeView {
  id: string;
  label: string;
  status: ElementStatus;
  meta?: Record<string, unknown>;
}

export function treeStructure(nodes: TreeNodeView[], links: Link[]): Structure {
  return { kind: 'tree', elements: nodes, links };
}

/** Link cha → con cho mảng nút (childId = null → không có con). */
export function treeLinks(childrenOf: Record<string, [string | null, string | null]>): Link[] {
  const links: Link[] = [];
  for (const parent of Object.keys(childrenOf)) {
    const [left, right] = childrenOf[parent];
    if (left) links.push({ from: parent, to: left, label: 'L' });
    if (right) links.push({ from: parent, to: right, label: 'R' });
  }
  return links;
}

// ── Structure heap (mảng + cạnh cha-con) ────────────────────────────────────

export function heapStructure(values: number[], statuses: StatusMap = {}, activeSize?: number): Structure {
  const size = activeSize ?? values.length;
  const elements: Element[] = values.slice(0, size).map((v, i) => ({
    id: `cell:${i}`,
    label: String(v),
    status: statuses[i] ?? 'default',
    group: 'heap-array',
    meta: { heapIndex: i },
  }));
  const links: Link[] = [];
  for (let i = 0; i < size; i++) {
    const l = 2 * i + 1;
    const r = 2 * i + 2;
    if (l < size) links.push({ from: `cell:${i}`, to: `cell:${l}`, status: 'default' });
    if (r < size) links.push({ from: `cell:${i}`, to: `cell:${r}`, status: 'default' });
  }
  return { kind: 'heap', elements, links };
}

// ── Đồ thị (SDD §4.14 — preset path/cycle/complete/bipartite/grid/custom) ───

export interface GraphParams {
  preset: string;
  directed: boolean;
  weighted: boolean;
  vertices: number;
  edges: number;
  source: number;
  target: number | null;
}

export const GRAPH_PRESETS = ['path', 'cycle', 'complete', 'bipartite', 'grid', 'custom'];

export function parseGraphParams(data: unknown): GraphParams {
  const rec = asRecord(data);
  const g: GraphParams = {
    preset: strField(rec, 'preset', 'custom'),
    directed: boolField(rec, 'directed', true),
    weighted: boolField(rec, 'weighted', true),
    vertices: intField(rec, 'vertices', 6),
    edges: intField(rec, 'edges', 8),
    source: intField(rec, 'source', 0),
    target: rec.target === null || rec.target === undefined ? null : intField(rec, 'target', -1),
  };
  return g;
}

export function validateGraphParams(data: unknown): string[] {
  const rec = asRecord(data);
  const errors: string[] = [];
  const g = parseGraphParams(data);
  if (!GRAPH_PRESETS.includes(g.preset)) {
    errors.push(`preset: phải là một trong ${GRAPH_PRESETS.join(', ')} (hiện tại '${g.preset}')`);
  }
  if (g.vertices < 2 || g.vertices > 50) errors.push(`vertices: phải trong khoảng 2–50 (hiện tại ${g.vertices})`);
  if (g.edges < 1 || g.edges > 200) errors.push(`edges: phải trong khoảng 1–200 (hiện tại ${g.edges})`);
  const maxUndirected = (g.vertices * (g.vertices - 1)) / 2;
  const maxEdges = g.directed ? g.vertices * (g.vertices - 1) : maxUndirected;
  if (g.edges > maxEdges) {
    errors.push(`edges=${g.edges} vượt số cạnh tối đa ${maxEdges} của đồ thị ${g.vertices} đỉnh (${g.directed ? 'có hướng' : 'vô hướng'})`);
  }
  if (g.source < 0 || g.source >= g.vertices) {
    errors.push(`source: phải trong khoảng 0..${g.vertices - 1} (hiện tại ${g.source})`);
  }
  if (g.target !== null && (g.target < 0 || g.target >= g.vertices)) {
    errors.push(`target: phải trong khoảng 0..${g.vertices - 1} hoặc null (hiện tại ${g.target})`);
  }
  if (g.target !== null && g.target === g.source) {
    errors.push('target phải khác source');
  }
  if (g.preset === 'grid') {
    const factors = gridFactors(g.vertices);
    if (factors === null) {
      errors.push(`preset=grid cần vertices phân tích được thành r×c (hiện tại ${g.vertices} — dùng preset khác hoặc đổi vertices)`);
    }
  }
  return errors;
}

/** Tìm cặp (r, c) với r·c = n, r gần sqrt(n) nhất; null nếu n nguyên tố (không phân tích được). */
function gridFactors(n: number): [number, number] | null {
  let r = Math.floor(Math.sqrt(n));
  while (r >= 2) {
    if (n % r === 0) return [r, n / r];
    r--;
  }
  return null;
}

export type Edge = [number, number, number]; // [u, v, w]

/** Sinh danh sách cạnh theo preset — deterministic (seed=42). */
export function buildGraphEdges(g: GraphParams): Edge[] {
  const n = g.vertices;
  const rng = createRng(42);
  const w = (): number => (g.weighted ? 1 + Math.floor(rng() * 9) : 1);
  const pairs: Array<[number, number]> = [];

  const addUndirected = (u: number, v: number): void => {
    if (u === v) return;
    pairs.push(u < v ? [u, v] : [v, u]);
  };

  switch (g.preset) {
    case 'path':
      for (let i = 0; i < n - 1; i++) addUndirected(i, i + 1);
      break;
    case 'cycle':
      for (let i = 0; i < n - 1; i++) addUndirected(i, i + 1);
      addUndirected(0, n - 1);
      break;
    case 'complete':
      for (let i = 0; i < n; i++) for (let j = i + 1; j < n; j++) addUndirected(i, j);
      break;
    case 'bipartite': {
      const half = Math.ceil(n / 2);
      for (let i = 0; i < half; i++) for (let j = half; j < n; j++) addUndirected(i, j);
      break;
    }
    case 'grid': {
      const f = gridFactors(n);
      if (f) {
        const [rows, cols] = f;
        const at = (r: number, c: number): number => r * cols + c;
        for (let r = 0; r < rows; r++) {
          for (let c = 0; c < cols; c++) {
            if (c + 1 < cols) addUndirected(at(r, c), at(r, c + 1));
            if (r + 1 < rows) addUndirected(at(r, c), at(r + 1, c));
          }
        }
      }
      break;
    }
    case 'custom':
    default: {
      const seen = new Set<string>();
      let guard = 0;
      while (pairs.length < g.edges && guard < g.edges * 50 + 100) {
        guard++;
        let u = Math.floor(rng() * n);
        let v = Math.floor(rng() * n);
        if (u === v) continue;
        if (!g.directed && u > v) {
          const tmp = u;
          u = v;
          v = tmp;
        }
        const key = `${u}-${v}`;
        if (seen.has(key)) continue;
        seen.add(key);
        pairs.push([u, v]);
      }
      break;
    }
  }

  // Nếu vô hướng: sinh cạnh 2 chiều cho thuật toán nhưng hiển thị 1 Link duy nhất.
  return pairs.map(([u, v]) => [u, v, w()]);
}

export function adjacency(g: GraphParams, edges: Edge[]): Array<Array<[number, number]>> {
  const adj: Array<Array<[number, number]>> = Array.from({ length: g.vertices }, () => []);
  for (const [u, v, w] of edges) {
    adj[u].push([v, w]);
    if (!g.directed) adj[v].push([u, w]);
  }
  for (const list of adj) list.sort((a, b) => a[0] - b[0]);
  return adj;
}

export interface GraphStatusMap {
  nodes: Record<number, ElementStatus>;
  edges: Record<string, ElementStatus>;
}

export function graphStructure(
  g: GraphParams,
  edges: Edge[],
  statuses: GraphStatusMap,
  dist?: Array<number | null>,
): Structure {
  const elements: Element[] = [];
  for (let i = 0; i < g.vertices; i++) {
    const el: Element = {
      id: `node:${i}`,
      label: String(i),
      status: statuses.nodes[i] ?? 'default',
      meta: dist ? { d: dist[i] } : { d: null },
    };
    elements.push(el);
  }
  const links: Link[] = edges.map(([u, v, w]) => {
    const from = g.directed ? u : Math.min(u, v);
    const to = g.directed ? v : Math.max(u, v);
    const key = `${from}-${to}`;
    return {
      from: `node:${from}`,
      to: `node:${to}`,
      label: g.weighted ? `w=${w}` : undefined,
      status: statuses.edges[key] ?? 'default',
    };
  });
  return { kind: 'graph', elements, links };
}

// ── Bảng băm ────────────────────────────────────────────────────────────────

export function hashIndex(key: number, tableSize: number, mode: string): number {
  if (mode === 'multiplication') {
    const frac = (key * 0.6180339887) % 1;
    return Math.floor(tableSize * Math.abs(frac));
  }
  return ((key % tableSize) + tableSize) % tableSize;
}

export function hashStructure(
  tableSize: number,
  buckets: number[][],
  statuses: Record<string, ElementStatus>,
  bucketStatuses: Record<number, ElementStatus> = {},
): Structure {
  const elements: Element[] = [];
  for (let b = 0; b < tableSize; b++) {
    elements.push({
      id: `bucket:${b}`,
      label: `[${b}]`,
      status: bucketStatuses[b] ?? 'default',
      group: `bucket:${b}`,
    });
    buckets[b].forEach((key, pos) => {
      elements.push({
        id: `node:${key}`,
        label: String(key),
        status: statuses[`node:${key}`] ?? 'default',
        group: `bucket:${b}`,
        meta: { chainPos: pos },
      });
    });
  }
  const links: Link[] = [];
  for (let b = 0; b < tableSize; b++) {
    for (let pos = 0; pos + 1 < buckets[b].length; pos++) {
      links.push({ from: `node:${buckets[b][pos]}`, to: `node:${buckets[b][pos + 1]}`, label: 'next' });
    }
  }
  return { kind: 'hashtable', elements, links };
}
