// engines/generators/linear/queue.ts — Queue Enqueue/Dequeue (SDD §4.7.10, §4.6.3)
import type { Element, InputConfig, InputSchema, SimulationGenerator, Structure } from '../../core/types';
import { buildGenerator, intField, Trace } from '../helpers';

const PSEUDOCODE = [
  'enqueue(x): if rear = capacity-1 and front = 0 then error "Hàng đợi đầy"',
  '            else rear++, q[rear] ← x',
  'dequeue():  if front > rear then error "Hàng đợi rỗng"',
  '            else x ← q[front], front++',
];

const SCHEMA: InputSchema = {
  kind: 'linear',
  fields: [
    { name: 'operations', type: 'string[]', label: 'Danh sách thao tác', default: ['Push 5', 'Push 3', 'Pop'], description: 'VD: Push 5 / Enqueue 5 (thêm), Pop / Dequeue (lấy ra)' },
    { name: 'capacity', type: 'int', label: 'Dung lượng', min: 1, max: 20, default: 8, description: 'Số ô tối đa của hàng đợi' },
  ],
};

interface QueueOp {
  kind: 'enqueue' | 'dequeue';
  value?: number;
}

function parseOps(raw: unknown): QueueOp[] {
  const list = Array.isArray(raw) ? raw : [];
  const ops: QueueOp[] = [];
  for (const item of list) {
    if (typeof item !== 'string') continue;
    const t = item.trim();
    const enq = /^(?:push|enqueue)\s+(-?\d+)$/i.exec(t);
    if (enq) {
      ops.push({ kind: 'enqueue', value: Number(enq[1]) });
    } else if (/^(?:pop|dequeue)$/i.test(t)) {
      ops.push({ kind: 'dequeue' });
    }
  }
  return ops;
}

function queueStructure(values: number[], capacity: number, statuses: Record<number, string>): Structure {
  const elements: Element[] = [];
  for (let i = 0; i < capacity; i++) {
    const filled = i < values.length;
    elements.push({
      id: `cell:${i}`,
      label: filled ? String(values[i]) : '—',
      status: (statuses[i] as Element['status']) ?? (filled ? 'default' : 'muted'),
      group: 'queue',
      meta: { empty: !filled },
    });
  }
  return { kind: 'queue', elements, links: [] };
}

export function createQueueEnqueueGenerator(): SimulationGenerator {
  return buildGenerator('queue.enqueue', SCHEMA, PSEUDOCODE, { validate: queueValidate, generate: runQueueOps });
}

export function createQueueDequeueGenerator(): SimulationGenerator {
  return buildGenerator('queue.dequeue', SCHEMA, PSEUDOCODE, { validate: queueValidate, generate: runQueueOps });
}

function queueValidate(input: InputConfig): { ok: boolean; errors: string[] } {
  const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
  const errors: string[] = [];
  const ops = parseOps(rec.operations);
  if (!Array.isArray(rec.operations) || rec.operations.length === 0) {
    errors.push('operations: cần ít nhất 1 thao tác (VD: "Push 5, Pop")');
  } else if (ops.length !== rec.operations.length) {
    errors.push('operations: mỗi thao tác phải có dạng "Push <số>" / "Enqueue <số>" hoặc "Pop" / "Dequeue"');
  } else if (ops.length > 30) {
    errors.push(`operations: tối đa 30 thao tác (hiện có ${ops.length})`);
  }
  ops.forEach((op, idx) => {
    if (op.kind === 'enqueue' && op.value !== undefined && (op.value < -999 || op.value > 999)) {
      errors.push(`operations[${idx}]: giá trị thêm phải trong khoảng -999..999 (hiện tại ${op.value})`);
    }
  });
  const capacity = intField(rec, 'capacity', 8);
  if (capacity < 1 || capacity > 20) errors.push(`capacity: phải trong khoảng 1–20 (hiện tại ${capacity})`);
  return { ok: errors.length === 0, errors };
}

function runQueueOps(input: InputConfig): ReturnType<SimulationGenerator['generate']> {
  const rec = input.data !== null && typeof input.data === 'object' ? (input.data as Record<string, unknown>) : {};
  const ops = parseOps(rec.operations);
  const capacity = intField(rec, 'capacity', 8);
  const trace = new Trace();
  const queue: number[] = [];
  const statuses: Record<number, string> = {};

  trace.vars.front = 0;
  trace.vars.rear = -1;
  trace.vars.capacity = capacity;
  trace.vars.queue = '';
  trace.push({
    line: 1,
    explanation: `Bắt đầu: hàng đợi rỗng, dung lượng ${capacity}, front=0, rear=-1.`,
    structure: queueStructure(queue, capacity, statuses),
    annotations: [`capacity=${capacity}, front=0, rear=-1`],
  });

  for (let opIdx = 0; opIdx < ops.length; opIdx++) {
    const op = ops[opIdx];
    trace.vars.op = `${op.kind}${op.value !== undefined ? ` ${op.value}` : ''}`;
    const front = 0;
    const rear = queue.length - 1;

    if (op.kind === 'enqueue') {
      const value = op.value ?? 0;
      if (rear + 1 >= capacity && front === 0) {
        trace.push({
          line: 1,
          explanation: `Hàng đợi đầy: rear=${rear} = capacity-1=${capacity - 1}, front=0 — không thể enqueue ${value}.`,
          structure: queueStructure(queue, capacity, statuses),
          annotations: ['LỖI: Hàng đợi đầy'],
        });
        continue;
      }
      trace.push({
        line: 1,
        explanation: `Kiểm tra rear=${rear} < capacity-1=${capacity - 1} → còn chỗ, enqueue ${value}.`,
        structure: queueStructure(queue, capacity, statuses),
        annotations: [`front=${front}, rear=${rear}`],
      });
      const newRear = rear + 1;
      queue.push(value);
      statuses[newRear] = 'swap';
      trace.vars.rear = newRear;
      trace.vars.queue = queue.join(',');
      trace.push({
        line: 2,
        explanation: `Ghi q[${newRear}] ← ${value}; rear: ${rear} → ${newRear}.`,
        structure: queueStructure(queue, capacity, statuses),
        annotations: [`front=${front}, rear=${newRear}, q[${newRear}]=${value}`],
      });
      statuses[newRear] = 'done';
    } else {
      if (queue.length === 0) {
        trace.push({
          line: 3,
          explanation: 'Hàng đợi rỗng: front=0 > rear=-1 — không thể dequeue.',
          structure: queueStructure(queue, capacity, statuses),
          annotations: ['LỖI: Hàng đợi rỗng'],
        });
        continue;
      }
      const value = queue[0];
      statuses[0] = 'swap';
      trace.push({
        line: 4,
        explanation: `Dequeue: x ← q[0] = ${value}, front++.`,
        structure: queueStructure(queue, capacity, statuses),
        annotations: [`x=${value}, front: 0 → 1`],
      });
      queue.shift();
      // Dịch trạng thái sang trái: ô cũ 0 bị lấy ra, các ô sau dịch lên.
      const next: Record<number, string> = {};
      for (let i = 0; i < queue.length; i++) next[i] = statuses[i + 1] ?? 'default';
      for (let i = queue.length; i < capacity; i++) next[i] = 'muted';
      trace.vars.front = 1;
      trace.vars.rear = queue.length - 1;
      trace.vars.queue = queue.join(',');
      trace.push({
        line: 4,
        explanation: `Đã lấy ${value} ra khỏi hàng đợi (phần tử còn lại dịch lên đầu).`,
        structure: queueStructure(queue, capacity, next),
        annotations: [`front=1, rear=${queue.length - 1}`],
      });
      for (const k of Object.keys(statuses)) delete statuses[Number(k)];
      Object.assign(statuses, next);
    }
  }

  trace.push({
    line: 4,
    explanation: `Kết thúc: hàng đợi [${queue.join(', ')}], front=0, rear=${queue.length - 1}.`,
    structure: queueStructure(queue, capacity, statuses),
  });
  return trace.steps;
}
