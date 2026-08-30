// engines/generators/heap/heapOps.ts — Heap: insert/extract/heapify (SDD §4.7.14, §4.6.4)
import type { ElementStatus, InputConfig, InputSchema, SimulationGenerator } from '../../core/types';
import type { StatusMap } from '../helpers';
import { buildGenerator, heapStructure, intField, intArrayField, strField, Trace } from '../helpers';

const PSEUDOCODE = [
  'procedure heapInsert(a, size, x)',
  '  a[size] ← x; size++',
  '  bubbleUp(size-1)',
  'procedure bubbleUp(i)',
  '  while i > 0 và a[parent(i)] < a[i] do',
  '    swap a[parent(i)], a[i]',
  '    i ← parent(i)',
  'procedure extractMax(a, size)',
  '  max ← a[0]',
  '  a[0] ← a[size-1]; size--',
  '  siftDown(0)',
  '  return max',
  'procedure heapify(a, n)',
  '  for i ← n/2-1 downto 0 do',
  '    siftDown(i)',
  'procedure siftDown(a, i)',
  '  while 2i+1 ≤ size-1 do',
  '    child ← max(a[2i+1], a[2i+2])',
  '    if a[i] < a[child] then swap; i ← child',
  '    else break',
  '  end procedures',
];

const SCHEMA: InputSchema = {
  kind: 'heap',
  fields: [
    { name: 'keys', type: 'int[]', label: 'Dãy khóa', min: -999, max: 999, default: [10, 7, 9, 4, 6, 8], description: 'Các khóa khởi tạo đống (1–31 khóa)' },
    { name: 'operation', type: 'select', label: 'Thao tác', options: [
      { label: 'Chèn (bubble up)', value: 'insert' },
      { label: 'Trích xuất max (sift down)', value: 'extract' },
      { label: 'Heapify', value: 'heapify' },
    ], default: 'heapify', description: 'Thao tác trên đống nhị phân max-heap' },
    { name: 'value', type: 'int', label: 'Giá trị', min: -999, max: 999, default: 15, description: 'Khóa chèn khi thao tác = insert' },
  ],
};

const OPERATIONS = ['insert', 'extract', 'heapify'];

export function createHeapInsertGenerator(): SimulationGenerator {
  return buildGenerator('heap.insert', SCHEMA, PSEUDOCODE, { validate: heapValidate, generate: (i) => runHeap(i, 'insert') });
}

export function createHeapExtractGenerator(): SimulationGenerator {
  return buildGenerator('heap.extract', SCHEMA, PSEUDOCODE, { validate: heapValidate, generate: (i) => runHeap(i, 'extract') });
}

export function createHeapHeapifyGenerator(): SimulationGenerator {
  return buildGenerator('heap.heapify', SCHEMA, PSEUDOCODE, { validate: heapValidate, generate: (i) => runHeap(i, 'heapify') });
}

function heapValidate(input: InputConfig): { ok: boolean; errors: string[] } {
  const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
  const errors: string[] = [];
  const keys = intArrayField(rec, 'keys', [10, 7, 9, 4, 6, 8]);
  if (keys.length < 1 || keys.length > 31) errors.push(`keys: phải có 1–31 khóa (hiện có ${keys.length})`);
  keys.forEach((k, i) => {
    if (k < -999 || k > 999) errors.push(`keys[${i}]=${k} phải trong khoảng -999..999`);
  });
  const op = strField(rec, 'operation', 'heapify');
  if (!OPERATIONS.includes(op)) errors.push(`operation: phải là một trong ${OPERATIONS.join(', ')} (hiện tại '${op}')`);
  const value = intField(rec, 'value', 15);
  if (value < -999 || value > 999) errors.push(`value: phải trong khoảng -999..999 (hiện tại ${value})`);
  return { ok: errors.length === 0, errors };
}

/** Kiểm tra max-heap hợp lệ từ vị trí i xuống dưới (sử dụng cho heapify/extract ban đầu). */
function isHeap(a: number[], size: number): boolean {
  for (let i = 0; i < size; i++) {
    const l = 2 * i + 1;
    const r = 2 * i + 2;
    if (l < size && a[i] < a[l]) return false;
    if (r < size && a[i] < a[r]) return false;
  }
  return true;
}

function runHeap(input: InputConfig, _preferred: 'insert' | 'extract' | 'heapify'): ReturnType<SimulationGenerator['generate']> {
  const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
  const keys = intArrayField(rec, 'keys', [10, 7, 9, 4, 6, 8]);
  const rawOp = typeof rec.operation === 'string' ? rec.operation : undefined;
  const op = rawOp && rawOp !== 'heapify' ? rawOp : (_preferred ?? rawOp ?? 'heapify');
  const value = intField(rec, 'value', 15);
  const trace = new Trace();
  const statuses: StatusMap = {};
  const a = keys.slice();
  let size = a.length;

  trace.vars.size = size;
  trace.vars.i = null;
  trace.vars.value = op === 'insert' ? value : null;
  trace.push({
    line: 1,
    explanation: `Bắt đầu: đống [${a.join(', ')}] (size=${size}), thao tác ${op}.`,
    structure: heapStructure(a, statuses, size),
    annotations: [`size=${size}`],
  });

  const siftDown = (root: number, end: number, lineBase: number): void => {
    let r = root;
    while (2 * r + 1 <= end) {
      trace.vars.i = r;
      trace.push({
        line: lineBase + 1,
        explanation: `siftDown(${r}): nút ${r} còn con trong phạm vi (2·${r}+1=${2 * r + 1} ≤ ${end}).`,
        structure: heapStructure(a, { ...statuses, [r]: 'active' }, size),
        annotations: [`root=${r}, end=${end}`],
      });
      const l = 2 * r + 1;
      const rr = 2 * r + 2;
      let child = l;
      if (rr <= end) {
        trace.stats.comparisons++;
        trace.push({
          line: lineBase + 2,
          explanation: `So sánh hai con a[${l}]=${a[l]} và a[${rr}]=${a[rr]}.`,
          structure: heapStructure(a, { ...statuses, [l]: 'active', [rr]: 'active', [r]: 'highlight' }, size),
          annotations: [`a[${l}]=${a[l]} vs a[${rr}]=${a[rr]}`],
        });
        if (a[rr] > a[l]) child = rr;
        trace.push({
          line: lineBase + 2,
          explanation: `Con lớn hơn là a[${child}]=${a[child]} → child=${child}.`,
          structure: heapStructure(a, { ...statuses, [child]: 'highlight' }, size),
          annotations: [`child=${child}`],
        });
      } else {
        trace.push({
          line: lineBase + 2,
          explanation: `Chỉ có con trái a[${l}]=${a[l]} → child=${l}.`,
          structure: heapStructure(a, { ...statuses, [l]: 'active', [r]: 'highlight' }, size),
          annotations: [`child=${l}`],
        });
      }

      trace.stats.comparisons++;
      trace.push({
        line: lineBase + 3,
        explanation: `So sánh a[${r}]=${a[r]} và a[${child}]=${a[child]}.`,
        structure: heapStructure(a, { ...statuses, [r]: 'active', [child]: 'active' }, size),
        annotations: [`a[${r}]=${a[r]} < a[${child}]=${a[child]}?`],
      });
      if (a[r] < a[child]) {
        const x = a[r];
        a[r] = a[child];
        a[child] = x;
        trace.stats.swaps++;
        trace.push({
          line: lineBase + 3,
          explanation: `${x} < ${a[r]} → đúng, hoán đổi a[${r}] và a[${child}], i ← ${child}.`,
          structure: heapStructure(a, { ...statuses, [r]: 'swap', [child]: 'swap' }, size),
          annotations: ['hoán đổi cha-con (sift down)'],
        });
        r = child;
      } else {
        trace.push({
          line: lineBase + 4,
          explanation: `a[${r}]=${a[r]} < a[${child}]=${a[child]} → sai, break.`,
          structure: heapStructure(a, { ...statuses, [r]: 'active', [child]: 'active' }, size),
        });
        break;
      }
    }
  };

  if (op === 'insert') {
    if (size >= 31) {
      trace.push({
        line: 2,
        explanation: `LỖI: đống đã đầy (size=${size} ≥ 31), không thể chèn ${value}.`,
        structure: heapStructure(a, statuses, size),
        annotations: ['LỖI: đống đầy'],
      });
    } else {
      a[size] = value;
      statuses[size] = 'highlight';
      trace.push({
        line: 2,
        explanation: `Ghi a[${size}] ← ${value}; size: ${size} → ${size + 1}.`,
        structure: heapStructure(a, statuses, size + 1),
        annotations: [`a[${size}]=${value}, size=${size + 1}`],
      });
      size++;
      trace.vars.size = size;
      let i = size - 1;
      trace.vars.i = i;
      trace.push({
        line: 3,
        explanation: `bubbleUp(${i}): đẩy nút mới ${value} lên đúng vị trí.`,
        structure: heapStructure(a, statuses, size),
        annotations: [`i=${i}`],
      });
      while (i > 0) {
        const parent = Math.floor((i - 1) / 2);
        trace.stats.comparisons++;
        trace.push({
          line: 5,
          explanation: `So sánh cha a[${parent}]=${a[parent]} và con a[${i}]=${a[i]}.`,
          structure: heapStructure(a, { ...statuses, [parent]: 'active', [i]: 'active' }, size),
          annotations: [`a[${parent}]=${a[parent]} < a[${i}]=${a[i]}?`],
        });
        if (a[parent] < a[i]) {
          const x = a[parent];
          a[parent] = a[i];
          a[i] = x;
          trace.stats.swaps++;
          trace.push({
            line: 6,
            explanation: `${x} < ${a[parent]} → đúng, hoán đổi a[${parent}] và a[${i}], i ← ${parent}.`,
            structure: heapStructure(a, { ...statuses, [parent]: 'swap', [i]: 'swap' }, size),
            annotations: ['hoán đổi cha-con (bubble up)'],
          });
          i = parent;
          trace.vars.i = i;
        } else {
          trace.push({
            line: 7,
            explanation: `a[${parent}]=${a[parent]} < a[${i}]=${a[i]} → sai, dừng bubble up.`,
            structure: heapStructure(a, { ...statuses, [parent]: 'active', [i]: 'active' }, size),
          });
          break;
        }
      }
      for (let k = 0; k < size; k++) statuses[k] = 'done';
      trace.push({
        line: 7,
        explanation: `Kết thúc: đã chèn ${value} → đống [${a.join(', ')}] (size=${size}).`,
        structure: heapStructure(a, statuses, size),
        annotations: ['max-heap hợp lệ'],
      });
    }
    return trace.steps;
  }

  if (op === 'extract') {
    if (size === 0) {
      trace.push({
        line: 8,
        explanation: 'LỖI: đống rỗng, không thể trích xuất max.',
        structure: heapStructure(a, statuses, size),
        annotations: ['LỖI: đống rỗng'],
      });
      trace.push({ line: 12, explanation: 'Kết thúc: không có phần tử để trích xuất.', structure: heapStructure(a, statuses, size) });
      return trace.steps;
    }
    const max = a[0];
    trace.vars.max = max;
    statuses[0] = 'error';
    trace.push({
      line: 8,
      explanation: `max ← a[0] = ${max}.`,
      structure: heapStructure(a, statuses, size),
      annotations: [`max=${max}`],
    });
    a[0] = a[size - 1];
    statuses[0] = 'swap';
    statuses[size - 1] = 'muted';
    trace.stats.writes++;
    trace.push({
      line: 9,
      explanation: `Đưa phần tử cuối lên đầu: a[0] ← a[${size - 1}] = ${a[0]}; size: ${size} → ${size - 1}.`,
      structure: heapStructure(a, statuses, size),
      annotations: [`a[0]=${a[0]}, size=${size - 1}`],
    });
    delete statuses[size - 1];
    size--;
    trace.vars.size = size;
    trace.vars.max = null;
    trace.push({
      line: 10,
      explanation: `siftDown(0): khôi phục max-heap sau khi lấy ${max} ra.`,
      structure: heapStructure(a, statuses, size),
      annotations: [`max=${max} đã lấy ra`],
    });
    siftDown(0, size - 1, 16);
    for (let k = 0; k < size; k++) statuses[k] = 'done';
    trace.push({
      line: 11,
      explanation: `Kết thúc: trích xuất max=${max} → đống còn lại [${a.slice(0, size).join(', ')}] (size=${size}).`,
      structure: heapStructure(a, statuses, size),
      annotations: [`max=${max}`],
    });
    return trace.steps;
  }

  // heapify
  trace.push({
    line: 13,
    explanation: `Mảng [${a.join(', ')}] (size=${size}) → bắt đầu heapify từ nút nội bộ cuối cùng ${Math.floor(size / 2) - 1} về gốc 0.`,
    structure: heapStructure(a, statuses, size),
    annotations: [`bắt đầu heapify, size=${size}`],
  });
  for (let i = Math.floor(size / 2) - 1; i >= 0; i--) {
    trace.vars.i = i;
    trace.push({
      line: 14,
      explanation: `Heapify nút i=${i} (a[${i}]=${a[i]}).`,
      structure: heapStructure(a, { ...statuses, [i]: 'active' }, size),
      annotations: [`i=${i}`],
    });
    siftDown(i, size - 1, 16);
  }
  for (let k = 0; k < size; k++) statuses[k] = 'done';
  trace.push({
    line: 15,
    explanation: `Kết thúc: heapify xong → max-heap [${a.join(', ')}].`,
    structure: heapStructure(a, statuses, size),
    annotations: ['max-heap hợp lệ'],
  });
  return trace.steps;
}
