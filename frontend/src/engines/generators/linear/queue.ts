// engines/generators/linear/queue.ts — Queue Enqueue/Dequeue (SDD §4.7.10, §4.6.3)
import type { Element, InputConfig, InputSchema, SimulationGenerator, Structure } from '../../core/types';
import { buildGenerator, intField, Trace } from '../helpers';

const PSEUDOCODE = [
  'enqueue(x): if rear = capacity-1 and front = 0 then error "Hàng đợi đầy"',
  '            else rear++, q[rear] ← x',
  'dequeue():  if front > rear then error "Hàng đợi rỗng"',
  '            else x ← q[front], front++',
];

const ENQUEUE_SCHEMA: InputSchema = {
  kind: 'linear',
  fields: [
    { name: 'operations', type: 'string[]', label: 'Danh sách thao tác', default: ['Enqueue 10', 'Enqueue 20', 'Enqueue 30', 'Enqueue 40', 'Enqueue 50'], description: 'VD: Enqueue 10 (thêm vào cuối), Dequeue (lấy ra từ đầu)' },
    { name: 'capacity', type: 'int', label: 'Dung lượng (Capacity)', min: 1, max: 20, default: 8, description: 'Số ô tối đa của hàng đợi' },
  ],
};

const DEQUEUE_SCHEMA: InputSchema = {
  kind: 'linear',
  fields: [
    { name: 'operations', type: 'string[]', label: 'Danh sách thao tác', default: ['Enqueue 15', 'Enqueue 28', 'Enqueue 42', 'Dequeue', 'Dequeue'], description: 'VD: Enqueue 15, Dequeue' },
    { name: 'capacity', type: 'int', label: 'Dung lượng (Capacity)', min: 1, max: 20, default: 8, description: 'Số ô tối đa của hàng đợi' },
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

function queueStructure(values: (number | null)[], capacity: number, statuses: Record<number, string>): Structure {
  const elements: Element[] = [];
  for (let i = 0; i < capacity; i++) {
    const val = values[i];
    const filled = val !== null;
    elements.push({
      id: `cell:${i}`,
      label: filled ? String(val) : '—',
      status: (statuses[i] as Element['status']) ?? (filled ? 'done' : 'muted'),
      group: 'queue',
      meta: { empty: !filled },
    });
  }
  return { kind: 'queue', elements, links: [] };
}

export function createQueueEnqueueGenerator(): SimulationGenerator {
  return buildGenerator('queue.enqueue', ENQUEUE_SCHEMA, PSEUDOCODE, { validate: queueValidate, generate: runQueueOps });
}

export function createQueueDequeueGenerator(): SimulationGenerator {
  return buildGenerator('queue.dequeue', DEQUEUE_SCHEMA, PSEUDOCODE, { validate: queueValidate, generate: runQueueOps });
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
  
  const queue: (number | null)[] = Array(capacity).fill(null);
  const statuses: Record<number, string> = {};

  let front = 0;
  let rear = -1;
  let size = 0;

  trace.vars.front = 0;
  trace.vars.rear = -1;
  trace.vars.capacity = capacity;
  trace.vars.size = 0;
  trace.vars.queue = '';
  trace.push({
    line: 1,
    explanation: `Bắt đầu: Hàng đợi rỗng (Dung lượng: ${capacity}). Con trỏ front=0, rear=-1 (chưa có phần tử nào).`,
    structure: queueStructure(queue, capacity, statuses),
    annotations: [`Dung lượng: ${capacity}, Số phần tử: 0`],
  });

  for (let opIdx = 0; opIdx < ops.length; opIdx++) {
    const op = ops[opIdx];
    trace.vars.op = `${op.kind === 'enqueue' ? 'Enqueue' : 'Dequeue'}${op.value !== undefined ? ` ${op.value}` : ''}`;

    if (op.kind === 'enqueue') {
      const value = op.value ?? 0;
      if (rear >= capacity - 1) {
        trace.push({
          line: 1,
          explanation: `Hàng đợi đầy (hoặc rear chạm đáy): rear=${rear} đã đạt dung lượng tối đa (${capacity-1}) — Không thể thêm ${value}. (Mô hình mảng tuyến tính cứng).`,
          structure: queueStructure(queue, capacity, statuses),
          annotations: ['CẢNH BÁO: Hàng đợi đầy (Overflow)'],
        });
        continue;
      }
      trace.push({
        line: 1,
        explanation: `Kiểm tra dung lượng: rear=${rear} < capacity-1=${capacity - 1} → Hàng đợi còn chỗ, tiến hành Enqueue(${value}).`,
        structure: queueStructure(queue, capacity, statuses),
        annotations: [`front=${front}, rear=${rear}, size=${size}`],
      });
      rear++;
      queue[rear] = value;
      size++;
      statuses[rear] = 'swap';
      trace.vars.rear = rear;
      trace.vars.size = size;
      trace.vars.queue = queue.filter(v => v !== null).join(', ');
      trace.push({
        line: 2,
        explanation: `Enqueue: Tăng rear lên ${rear} và lưu q[${rear}] = ${value}. Phần tử ${value} đứng ở cuối hàng đợi (sau cùng).`,
        structure: queueStructure(queue, capacity, statuses),
        annotations: [`front=${front}, rear=${rear}, q[${rear}]=${value}`],
      });
      statuses[rear] = 'done';
    } else {
      if (front > rear || size === 0) {
        trace.push({
          line: 3,
          explanation: `Hàng đợi rỗng: front (${front}) > rear (${rear}) (không có phần tử) — Không thể Dequeue (Underflow).`,
          structure: queueStructure(queue, capacity, statuses),
          annotations: ['CẢNH BÁO: Hàng đợi rỗng (Underflow)'],
        });
        continue;
      }
      const value = queue[front];
      statuses[front] = 'swap';
      trace.push({
        line: 4,
        explanation: `Dequeue: Lấy phần tử ${value} tại đầu hàng đợi (front=${front}) ra xử lý theo nguyên tắc FIFO.`,
        structure: queueStructure(queue, capacity, statuses),
        annotations: [`Giá trị lấy ra: x = ${value}`],
      });
      
      queue[front] = null;
      statuses[front] = 'muted';
      front++;
      size--;
      
      trace.vars.front = front;
      trace.vars.size = size;
      trace.vars.queue = queue.filter(v => v !== null).join(', ');
      trace.push({
        line: 4,
        explanation: `Đã hoàn tất Dequeue: Phần tử ${value} đã ra khỏi hàng đợi. Tăng con trỏ front lên ${front}. Hiện còn ${size} phần tử.`,
        structure: queueStructure(queue, capacity, statuses),
        annotations: [`front=${front}, rear=${rear}, size=${size}`],
      });
    }
  }

  trace.push({
    line: 4,
    explanation: `Kết thúc chuỗi thao tác: Hàng đợi có ${size} phần tử. front=${front}, rear=${rear}.`,
    structure: queueStructure(queue, capacity, statuses),
  });
  return trace.steps;
}

