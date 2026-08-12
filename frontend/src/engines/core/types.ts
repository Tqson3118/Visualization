// engines/core/types.ts — sao chép NGUYÊN VĂN interface từ SDD §4.2

export type ElementStatus = 'default' | 'active' | 'highlight' | 'swap' | 'done' | 'error' | 'muted';

export interface Element {
  id: string;            // duy nhất trong Structure, VD: 'cell:2', 'node:5', 'edge:2-3'
  label: string;         // giá trị hiển thị chính, VD: '7', 'null', 'd[2]=9'
  status: ElementStatus;
  group?: string;        // nhóm renderer bố trí, VD: 'heap-array', 'tree', 'bucket:3'
  meta?: Record<string, unknown>;
}

export interface Link {
  from: string;
  to: string;
  label?: string;        // VD: trọng số 'w=4'
  status?: ElementStatus;
}

export interface Structure {
  kind: string;          // 'array' | 'linkedlist' | 'stack' | 'queue' | 'tree' | 'heap' | 'hashtable' | 'graph'
  elements: Element[];
  links: Link[];
}

export interface Step {
  index: number;
  structure: Structure;
  explanation: string;         // tiếng Việt, 1-4 câu
  pseudocodeLine: number;      // dòng mã giả 1-based
  highlights: string[];
  annotations: string[];       // VD: ['i=2, j=3', 'so sánh a[2]=7 > a[3]=4 → hoán đổi']
  variables: Record<string, string | number | boolean | null>;
  stats: { comparisons: number; swaps: number; writes: number };
  version: 1;
}

export interface InputConfig { kind: string; data: unknown; }

export interface InputSchema {
  kind: string;
  fields: Array<{
    name: string;
    type: 'int' | 'int[]' | 'string[]' | 'select' | 'bool';
    label: string;
    min?: number; max?: number;
    options?: Array<{ label: string; value: unknown }>;
    default: unknown;
    description: string;
  }>;
}

export interface SimulationGenerator {
  key: string;                 // VD: 'sort.bubble'
  title: string;               // tiếng Việt
  category: 'structure' | 'algorithm';
  dataStructure: string;
  level: 'basic' | 'advanced';
  complexity: { best: string; average: string; worst: string; space: string };
  inputSchema: InputSchema;
  pseudocode: string[];        // mỗi phần tử = 1 dòng mã giả
  generate(input: InputConfig): Step[];
  validate(input: InputConfig): { ok: boolean; errors: string[] };
}
