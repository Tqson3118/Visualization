// engines/generators/linear/stack.ts — Stack Push/Pop/Peek (SDD §4.7.9, §4.6.3)
import type { Element, InputConfig, InputSchema, SimulationGenerator, Structure } from '../../core/types';
import { buildGenerator, intField, Trace } from '../helpers';

const PSEUDOCODE = [
  'push(x):  if top = capacity-1 then error "Tràn ngăn xếp"',
  '          else top++, s[top] ← x',
  'pop():    if top = -1 then error "Ngăn xếp rỗng"',
  '          else x ← s[top], top--',
  'peek():   if top = -1 then error "Ngăn xếp rỗng"',
  '          else return s[top]',
];

const SCHEMA: InputSchema = {
  kind: 'linear',
  fields: [
    { name: 'operations', type: 'string[]', label: 'Danh sách thao tác', default: ['Push 5', 'Push 3', 'Pop'], description: 'VD: Push 5, Push 3, Pop, Peek (phân cách dấu phẩy)' },
    { name: 'capacity', type: 'int', label: 'Dung lượng', min: 1, max: 20, default: 8, description: 'Số ô tối đa của ngăn xếp' },
  ],
};

interface StackOp {
  kind: 'push' | 'pop' | 'peek';
  value?: number;
}

function parseOps(raw: unknown): StackOp[] {
  const list = Array.isArray(raw) ? raw : [];
  const ops: StackOp[] = [];
  for (const item of list) {
    if (typeof item !== 'string') continue;
    const t = item.trim();
    const push = /^push\s+(-?\d+)$/i.exec(t);
    if (push) {
      ops.push({ kind: 'push', value: Number(push[1]) });
    } else if (/^pop$/i.test(t)) {
      ops.push({ kind: 'pop' });
    } else if (/^peek$/i.test(t)) {
      ops.push({ kind: 'peek' });
    }
  }
  return ops;
}

function stackStructure(values: number[], capacity: number, statuses: Record<number, string>): Structure {
  const elements: Element[] = [];
  for (let i = 0; i < capacity; i++) {
    const filled = i < values.length;
    elements.push({
      id: `cell:${i}`,
      label: filled ? String(values[i]) : '—',
      status: (statuses[i] as Element['status']) ?? (filled ? 'default' : 'muted'),
      group: 'stack',
      meta: { empty: !filled },
    });
  }
  return { kind: 'stack', elements, links: [] };
}

export function createStackPushGenerator(): SimulationGenerator {
  return buildGenerator(
    'stack.push',
    SCHEMA,
    PSEUDOCODE,
    { validate: stackValidate, generate: (input) => runStackOps(input, 'push') },
  );
}

export function createStackPopGenerator(): SimulationGenerator {
  return buildGenerator(
    'stack.pop',
    SCHEMA,
    PSEUDOCODE,
    { validate: stackValidate, generate: (input) => runStackOps(input, 'pop') },
  );
}

export function createStackPeekGenerator(): SimulationGenerator {
  return buildGenerator(
    'stack.peek',
    SCHEMA,
    PSEUDOCODE,
    { validate: stackValidate, generate: (input) => runStackOps(input, 'peek') },
  );
}

function stackValidate(input: InputConfig): { ok: boolean; errors: string[] } {
  const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
  const errors: string[] = [];
  const ops = parseOps(rec.operations);
  if (!Array.isArray(rec.operations) || rec.operations.length === 0) {
    errors.push('operations: cần ít nhất 1 thao tác (VD: "Push 5, Pop")');
  } else if (ops.length !== rec.operations.length) {
    errors.push('operations: mỗi thao tác phải có dạng "Push <số>", "Pop" hoặc "Peek"');
  } else if (ops.length > 30) {
    errors.push(`operations: tối đa 30 thao tác (hiện có ${ops.length})`);
  }
  ops.forEach((op, idx) => {
    if (op.kind === 'push' && op.value !== undefined && (op.value < -999 || op.value > 999)) {
      errors.push(`operations[${idx}]: giá trị Push phải trong khoảng -999..999 (hiện tại ${op.value})`);
    }
  });
  const capacity = intField(rec, 'capacity', 8);
  if (capacity < 1 || capacity > 20) errors.push(`capacity: phải trong khoảng 1–20 (hiện tại ${capacity})`);
  return { ok: errors.length === 0, errors };
}

function runStackOps(input: InputConfig, _preferred: 'push' | 'pop' | 'peek'): ReturnType<SimulationGenerator['generate']> {
  const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
  const ops = parseOps(rec.operations);
  const capacity = intField(rec, 'capacity', 8);
  const trace = new Trace();
  const stack: number[] = [];
  const statuses: Record<number, string> = {};

  trace.vars.top = -1;
  trace.vars.capacity = capacity;
  trace.vars.stack = '';
  trace.push({
    line: 1,
    explanation: `Bắt đầu: ngăn xếp rỗng, dung lượng ${capacity}, top = -1.`,
    structure: stackStructure(stack, capacity, statuses),
    annotations: [`capacity=${capacity}, top=-1`],
  });

  for (let opIdx = 0; opIdx < ops.length; opIdx++) {
    const op = ops[opIdx];
    trace.vars.op = `${op.kind}${op.value !== undefined ? ` ${op.value}` : ''}`;
    const top = stack.length - 1;

    if (op.kind === 'push') {
      const value = op.value ?? 0;
      if (top + 1 >= capacity) {
        statuses[top] = 'error';
        trace.push({
          line: 1,
          explanation: `Tràn ngăn xếp: top=${top} = capacity-1=${capacity - 1}, không thể push ${value}.`,
          structure: stackStructure(stack, capacity, statuses),
          annotations: ['LỖI: Tràn ngăn xếp'],
        });
        continue;
      }
      trace.push({
        line: 1,
        explanation: `Kiểm tra top=${top} < capacity-1=${capacity - 1} → không tràn, thực hiện push ${value}.`,
        structure: stackStructure(stack, capacity, { ...statuses, [top]: 'active' }),
        annotations: [`top=${top}`],
      });
      const newTop = top + 1;
      stack.push(value);
      statuses[newTop] = 'swap';
      trace.vars.top = newTop;
      trace.vars.stack = stack.join(',');
      trace.push({
        line: 2,
        explanation: `Ghi s[${newTop}] ← ${value}; top: ${top} → ${newTop}.`,
        structure: stackStructure(stack, capacity, statuses),
        annotations: [`top=${newTop}, s[${newTop}]=${value}`],
      });
      statuses[newTop] = 'done';
    } else if (op.kind === 'pop') {
      if (top < 0) {
        trace.push({
          line: 3,
          explanation: 'Ngăn xếp rỗng: top = -1, không thể pop.',
          structure: stackStructure(stack, capacity, statuses),
          annotations: ['LỖI: Ngăn xếp rỗng'],
        });
        continue;
      }
      statuses[top] = 'swap';
      const value = stack[top];
      trace.push({
        line: 4,
        explanation: `Pop: x ← s[${top}] = ${value}, top--.`,
        structure: stackStructure(stack, capacity, statuses),
        annotations: [`x=${value}, top: ${top} → ${top - 1}`],
      });
      stack.pop();
      statuses[top] = 'muted';
      trace.vars.top = top - 1;
      trace.vars.stack = stack.join(',');
      trace.push({
        line: 4,
        explanation: `Đã lấy ${value} ra khỏi ngăn xếp (ô s[${top}] trở lại trống).`,
        structure: stackStructure(stack, capacity, statuses),
        annotations: [`top=${top - 1}`],
      });
    } else {
      if (top < 0) {
        trace.push({
          line: 5,
          explanation: 'Ngăn xếp rỗng: top = -1, không thể peek.',
          structure: stackStructure(stack, capacity, statuses),
          annotations: ['LỖI: Ngăn xếp rỗng'],
        });
        continue;
      }
      statuses[top] = 'highlight';
      trace.push({
        line: 6,
        explanation: `Peek: trả về s[${top}] = ${stack[top]} (không xóa).`,
        structure: stackStructure(stack, capacity, statuses),
        annotations: [`s[${top}]=${stack[top]}`],
      });
      statuses[top] = 'default';
    }
  }

  trace.push({
    line: 6,
    explanation: `Kết thúc: ngăn xếp [${stack.join(', ')}], top=${stack.length - 1}.`,
    structure: stackStructure(stack, capacity, statuses),
  });
  return trace.steps;
}
