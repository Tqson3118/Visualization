// views/stackqueue/stackQueueEngine.ts — Sinh các bước hoạt ảnh cho 4 cấu trúc:
// Stack (LIFO) · Queue (FIFO) · Circular Queue (tròn) · Deque (2 đầu).
// Thuần logic (không phụ thuộc Vue) → test được. Ngữ nghĩa chuẩn theo
// engines/generators/linear/{stack,queue}.ts (mô phỏng).

export type DsMode = 'stack' | 'queue' | 'circular' | 'deque' | 'minmax';
export type OpKind = 'push' | 'pop' | 'peek' | 'pushFront' | 'popFront';
export interface DsOp {
  kind: OpKind;
  value?: number;
}

export type DsCellState = 'idle' | 'filled' | 'active' | 'inserted' | 'removed' | 'highlight';
export interface DsCell {
  val: number | null;
  state: DsCellState;
}

export interface DsStep {
  stepIndex: number;
  /** cells[i] = ô thứ i (stack/queue/deque: 0 = đáy/đầu). Ô trống → val = null, state = 'idle'. */
  cells: DsCell[];
  /** Con trỏ: pointers[cellIndex] = mảng nhãn (TOP/FRONT/REAR). */
  pointers: Record<number, string[]>;
  /** Nhãn thao tác đang thực hiện (VD: "Push 5", "Pop", "Enqueue 7"). */
  label: string;
  log: string;
  /** false = bước lỗi (tràn / rỗng...). */
  ok: boolean;
  isFinal: boolean;
  /** index của thao tác sinh ra bước này (init=-1, final=-1). */
  opIndex: number;
  /** Các biến trạng thái hiển thị (top/front/rear/size/capacity/wrap...). */
  indices: Record<string, string | number>;
  /** Hàng ô phụ (song song) — dùng cho Min/Max Stack (MIN row, MAX row). */
  extraRows?: Array<{ label: string; cells: DsCell[] }>;
}

function emptyCells(capacity: number): DsCell[] {
  return Array.from({ length: capacity }, () => ({ val: null, state: 'idle' as DsCellState }));
}

function fillCells(capacity: number, values: (number | null)[], focus?: { idx: number; state: DsCellState }): DsCell[] {
  const next: DsCell[] = [];
  for (let i = 0; i < capacity; i++) {
    const val = values[i] ?? null;
    next.push({ val, state: val === null ? 'idle' : 'filled' });
  }
  if (focus && focus.idx >= 0 && focus.idx < next.length) {
    next[focus.idx] = { val: next[focus.idx].val, state: focus.state };
  }
  return next;
}

function addPointer(ptr: Record<number, string[]>, idx: number, label: string): void {
  if (idx < 0) return;
  ptr[idx] = [...(ptr[idx] ?? []), label];
}

// ─────────────────────────────── Stack (LIFO) ───────────────────────────────
export function generateStackSteps(ops: DsOp[], capacity: number): DsStep[] {
  const steps: DsStep[] = [];
  const stack: number[] = [];
  let step = 0;

  const push = (label: string, log: string, cells: DsCell[], pointers: Record<number, string[]>, ok: boolean, opIndex: number, isFinal = false): void => {
    const top = stack.length - 1;
    steps.push({
      stepIndex: step++, cells, pointers, label, log, ok, isFinal, opIndex,
      indices: { top, size: stack.length, capacity },
    });
  };

  push('Khởi tạo', `Bắt đầu: ngăn xếp rỗng, dung lượng ${capacity} (LIFO — vào sau ra trước).`, emptyCells(capacity), {}, true, -1);

  for (let opIdx = 0; opIdx < ops.length; opIdx++) {
    const op = ops[opIdx];
    const top = stack.length - 1;
    const label = `${op.kind === 'push' ? 'Push' : op.kind === 'pop' ? 'Pop' : 'Peek'}${op.value !== undefined ? ` ${op.value}` : ''}`;

    if (op.kind === 'push') {
      const v = op.value ?? 0;
      if (stack.length >= capacity) {
        const ptr: Record<number, string[]> = {};
        addPointer(ptr, capacity - 1, 'TOP');
        push(label, `LỖI: tràn ngăn xếp — top=${top} = capacity-1=${capacity - 1}, không thể push ${v}.`, fillCells(capacity, stack, { idx: capacity - 1, state: 'removed' }), ptr, false, opIdx);
        continue;
      }
      push(label, `Kiểm tra top=${top} < capacity-1 → còn chỗ, chuẩn bị ghi s[${stack.length}] ← ${v}.`, fillCells(capacity, stack, { idx: stack.length, state: 'active' }), { [stack.length]: ['TOP'] }, true, opIdx);
      stack.push(v);
      const ptr: Record<number, string[]> = {};
      addPointer(ptr, stack.length - 1, 'TOP');
      push(label, `Ghi s[${stack.length - 1}] ← ${v} — top: ${top} → ${stack.length - 1}.`, fillCells(capacity, stack, { idx: stack.length - 1, state: 'inserted' }), ptr, true, opIdx);
    } else if (op.kind === 'pop') {
      if (stack.length === 0) {
        push(label, 'LỖI: ngăn xếp rỗng (top = -1), không thể pop.', emptyCells(capacity), {}, false, opIdx);
        continue;
      }
      const v = stack[top];
      push(label, `Pop: x ← s[${top}] = ${v} — đang lấy phần tử đỉnh ra.`, fillCells(capacity, stack, { idx: top, state: 'removed' }), { [top]: ['TOP'] }, true, opIdx);
      stack.pop();
      const ptr: Record<number, string[]> = {};
      if (stack.length > 0) addPointer(ptr, stack.length - 1, 'TOP');
      push(label, `Đã lấy ${v} ra — ô s[${top}] trống, top → ${stack.length - 1}.`, fillCells(capacity, stack), ptr, true, opIdx);
    } else {
      if (stack.length === 0) {
        push(label, 'LỖI: ngăn xếp rỗng (top = -1), không thể peek.', emptyCells(capacity), {}, false, opIdx);
        continue;
      }
      const v = stack[top];
      push(label, `Peek: trả về s[${top}] = ${v} (không xóa phần tử).`, fillCells(capacity, stack, { idx: top, state: 'highlight' }), { [top]: ['TOP'] }, true, opIdx);
    }
  }

  const ptr: Record<number, string[]> = {};
  if (stack.length > 0) addPointer(ptr, stack.length - 1, 'TOP');
  push('Kết thúc', `Kết thúc: ngăn xếp [${stack.join(', ')}] — top=${stack.length - 1}.`, fillCells(capacity, stack), ptr, true, -1, true);
  return steps;
}

// ─────────────────────────────── Queue (FIFO) ───────────────────────────────
export function generateQueueSteps(ops: DsOp[], capacity: number): DsStep[] {
  const steps: DsStep[] = [];
  const queue: number[] = [];
  let step = 0;

  const push = (label: string, log: string, cells: DsCell[], pointers: Record<number, string[]>, ok: boolean, opIndex: number, isFinal = false): void => {
    steps.push({
      stepIndex: step++, cells, pointers, label, log, ok, isFinal, opIndex,
      indices: { front: 0, rear: queue.length - 1, size: queue.length, capacity },
    });
  };

  push('Khởi tạo', `Bắt đầu: hàng đợi rỗng, dung lượng ${capacity} (FIFO — vào trước ra trước).`, emptyCells(capacity), { [0]: ['FRONT'] }, true, -1);

  for (let opIdx = 0; opIdx < ops.length; opIdx++) {
    const op = ops[opIdx];
    if (op.kind === 'peek') continue;
    const rear = queue.length - 1;
    const label = `${op.kind === 'push' ? 'Enqueue' : 'Dequeue'}${op.value !== undefined ? ` ${op.value}` : ''}`;

    if (op.kind === 'push') {
      const v = op.value ?? 0;
      if (queue.length >= capacity) {
        const ptr: Record<number, string[]> = { [0]: ['FRONT'] };
        addPointer(ptr, capacity - 1, 'REAR');
        push(label, `LỖI: hàng đợi đầy — rear=${rear} = capacity-1=${capacity - 1}, không thể enqueue ${v}.`, fillCells(capacity, queue, { idx: capacity - 1, state: 'removed' }), ptr, false, opIdx);
        continue;
      }
      const ptr1: Record<number, string[]> = { [0]: ['FRONT'] };
      addPointer(ptr1, queue.length, 'REAR');
      push(label, `Kiểm tra rear=${rear} < capacity-1 → còn chỗ, chuẩn bị ghi q[${queue.length}] ← ${v}.`, fillCells(capacity, queue, { idx: queue.length, state: 'active' }), ptr1, true, opIdx);
      queue.push(v);
      const ptr2: Record<number, string[]> = { [0]: ['FRONT'] };
      addPointer(ptr2, queue.length - 1, 'REAR');
      push(label, `Ghi q[${queue.length - 1}] ← ${v} — rear: ${rear} → ${queue.length - 1}.`, fillCells(capacity, queue, { idx: queue.length - 1, state: 'inserted' }), ptr2, true, opIdx);
    } else {
      if (queue.length === 0) {
        push(label, 'LỖI: hàng đợi rỗng (front=0 > rear=-1), không thể dequeue.', emptyCells(capacity), { [0]: ['FRONT'] }, false, opIdx);
        continue;
      }
      const v = queue[0];
      push(label, `Dequeue: x ← q[0] = ${v} — đang lấy phần tử đầu hàng.`, fillCells(capacity, queue, { idx: 0, state: 'removed' }), { [0]: ['FRONT'], [rear]: ['REAR'] }, true, opIdx);
      queue.shift();
      const ptr: Record<number, string[]> = { [0]: ['FRONT'] };
      if (queue.length > 0) addPointer(ptr, queue.length - 1, 'REAR');
      push(label, `Đã lấy ${v} ra — các phần tử còn lại DỒN LÊN đầu (tốn O(n)), rear → ${queue.length - 1}.`, fillCells(capacity, queue), ptr, true, opIdx);
    }
  }

  const ptr: Record<number, string[]> = { [0]: ['FRONT'] };
  if (queue.length > 0) addPointer(ptr, queue.length - 1, 'REAR');
  push('Kết thúc', `Kết thúc: hàng đợi [${queue.join(', ')}] — front=0, rear=${queue.length - 1}.`, fillCells(capacity, queue), ptr, true, -1, true);
  return steps;
}

// ─────────────────────── Circular Queue (hàng đợi tròn) ─────────────────────
export function generateCircularQueueSteps(ops: DsOp[], capacity: number): DsStep[] {
  const steps: DsStep[] = [];
  const slots: (number | null)[] = Array(capacity).fill(null);
  let front = 0;
  let size = 0;
  let step = 0;

  const push = (label: string, log: string, cells: DsCell[], pointers: Record<number, string[]>, ok: boolean, opIndex: number, isFinal = false): void => {
    const rear = size > 0 ? (front + size - 1) % capacity : -1;
    const wrapped = size > 0 && front + size > capacity;
    steps.push({
      stepIndex: step++, cells, pointers, label, log, ok, isFinal, opIndex,
      indices: { front, rear, size, capacity, ...(wrapped ? { wrap: 'đã quay vòng' } : {}) },
    });
  };

  push('Khởi tạo', `Bắt đầu: hàng đợi TRÒN rỗng, dung lượng ${capacity} — front=0, rear=-1. Dequeue không dồn lên, front/rear quay vòng tái sử dụng ô trống.`, emptyCells(capacity), { [0]: ['FRONT'] }, true, -1);

  for (let opIdx = 0; opIdx < ops.length; opIdx++) {
    const op = ops[opIdx];
    if (op.kind === 'peek') continue;
    const label = `${op.kind === 'push' ? 'Enqueue' : 'Dequeue'}${op.value !== undefined ? ` ${op.value}` : ''}`;

    if (op.kind === 'push') {
      const v = op.value ?? 0;
      if (size === capacity) {
        const ptr: Record<number, string[]> = { [front]: ['FRONT'] };
        addPointer(ptr, (front + size - 1) % capacity, 'REAR');
        push(label, `LỖI: hàng đợi tròn đầy (size=${size} = capacity), không thể enqueue ${v}.`, fillCells(capacity, slots, { idx: (front + size - 1) % capacity, state: 'removed' }), ptr, false, opIdx);
        continue;
      }
      const writeIdx = (front + size) % capacity;
      const willWrap = writeIdx < front;
      const ptr1: Record<number, string[]> = { [front]: ['FRONT'] };
      addPointer(ptr1, writeIdx, 'REAR');
      push(label, `Kiểm tra size=${size} < capacity → còn ${capacity - size} chỗ, sẽ ghi vào ô q[${writeIdx}]${willWrap ? ' (QUAY VÒNG qua cuối mảng!)' : ''}.`, fillCells(capacity, slots, { idx: writeIdx, state: 'active' }), ptr1, true, opIdx);
      slots[writeIdx] = v;
      size++;
      const newRear = (front + size - 1) % capacity;
      const ptr2: Record<number, string[]> = { [front]: ['FRONT'] };
      addPointer(ptr2, newRear, 'REAR');
      push(label, `Ghi q[${writeIdx}] ← ${v} — rear: ${newRear === 0 ? '—' : newRear - 1} → ${newRear}${willWrap ? ' (tái sử dụng ô trống phía trước!)' : ''}.`, fillCells(capacity, slots, { idx: writeIdx, state: 'inserted' }), ptr2, true, opIdx);
    } else {
      if (size === 0) {
        push(label, 'LỖI: hàng đợi tròn rỗng (size=0), không thể dequeue.', emptyCells(capacity), { [front]: ['FRONT'] }, false, opIdx);
        continue;
      }
      const readIdx = front;
      const v = slots[readIdx];
      const ptr1: Record<number, string[]> = { [front]: ['FRONT'] };
      addPointer(ptr1, (front + size - 1) % capacity, 'REAR');
      push(label, `Dequeue: x ← q[${readIdx}] = ${v} — đang lấy phần tử ở front.`, fillCells(capacity, slots, { idx: readIdx, state: 'removed' }), ptr1, true, opIdx);
      slots[readIdx] = null;
      front = (front + 1) % capacity;
      size--;
      const ptr2: Record<number, string[]> = { [front]: ['FRONT'] };
      if (size > 0) addPointer(ptr2, (front + size - 1) % capacity, 'REAR');
      push(label, `Đã lấy ${v} ra — front quay tới ${front} (ô [${readIdx}] TRỐNG được tái sử dụng, không dồn lên → O(1)).`, fillCells(capacity, slots), ptr2, true, opIdx);
    }
  }

  const ptr: Record<number, string[]> = { [front]: ['FRONT'] };
  if (size > 0) addPointer(ptr, (front + size - 1) % capacity, 'REAR');
  const vals: (number | null)[] = [...slots];
  push('Kết thúc', `Kết thúc: hàng đợi tròn front=${front}, size=${size} — nội dung [${vals.map((x) => x ?? '—').join(', ')}].`, fillCells(capacity, vals), ptr, true, -1, true);
  return steps;
}

// ─────────────────────────────── Deque (2 đầu) ──────────────────────────────
export function generateDequeSteps(ops: DsOp[], capacity: number): DsStep[] {
  const steps: DsStep[] = [];
  const deque: number[] = [];
  let step = 0;

  const push = (label: string, log: string, cells: DsCell[], pointers: Record<number, string[]>, ok: boolean, opIndex: number, isFinal = false): void => {
    steps.push({
      stepIndex: step++, cells, pointers, label, log, ok, isFinal, opIndex,
      indices: { front: 0, rear: deque.length - 1, size: deque.length, capacity },
    });
  };

  push('Khởi tạo', `Bắt đầu: DEQUE rỗng, dung lượng ${capacity} — thêm/bớt được ở CẢ HAI ĐẦU (front & rear).`, emptyCells(capacity), {}, true, -1);

  const opLabel = (op: DsOp): string => {
    if (op.kind === 'push') return `Push sau${op.value !== undefined ? ` ${op.value}` : ''}`;
    if (op.kind === 'pushFront') return `Push trước${op.value !== undefined ? ` ${op.value}` : ''}`;
    if (op.kind === 'pop') return 'Pop sau';
    return 'Pop trước';
  };

  for (let opIdx = 0; opIdx < ops.length; opIdx++) {
    const op = ops[opIdx];
    if (op.kind === 'peek') continue;
    const label = opLabel(op);
    const rear = deque.length - 1;

    if (op.kind === 'push' || op.kind === 'pushFront') {
      const v = op.value ?? 0;
      const isFront = op.kind === 'pushFront';
      if (deque.length >= capacity) {
        const ptr: Record<number, string[]> = {};
        addPointer(ptr, 0, 'FRONT');
        addPointer(ptr, rear, 'REAR');
        push(label, `LỖI: deque đầy (size=${deque.length} = capacity), không thể thêm ${v}.`, fillCells(capacity, deque, { idx: isFront ? 0 : rear, state: 'removed' }), ptr, false, opIdx);
        continue;
      }
      if (isFront) {
        const prePtr: Record<number, string[]> = {};
        if (deque.length > 0) { addPointer(prePtr, 0, 'FRONT'); addPointer(prePtr, deque.length - 1, 'REAR'); }
        push(label, `Chèn ${v} vào TRƯỚC — các phần tử hiện có sẽ dồn sang phải một ô.`, fillCells(capacity, deque, { idx: 0, state: 'active' }), prePtr, true, opIdx);
        deque.unshift(v);
        const ptr: Record<number, string[]> = { [0]: ['FRONT'] };
        addPointer(ptr, deque.length - 1, 'REAR');
        push(label, `Đã chèn ${v} vào đầu — front=${0}, các phần tử cũ dồn phải.`, fillCells(capacity, deque, { idx: 0, state: 'inserted' }), ptr, true, opIdx);
      } else {
        const prePtr: Record<number, string[]> = {};
        if (deque.length > 0) { addPointer(prePtr, 0, 'FRONT'); addPointer(prePtr, deque.length - 1, 'REAR'); }
        push(label, `Chèn ${v} vào SAU (cuối) — chuẩn bị ghi vào ô [${deque.length}].`, fillCells(capacity, deque, { idx: deque.length, state: 'active' }), prePtr, true, opIdx);
        deque.push(v);
        const ptr: Record<number, string[]> = { [0]: ['FRONT'] };
        addPointer(ptr, deque.length - 1, 'REAR');
        push(label, `Đã chèn ${v} vào cuối — rear: ${rear} → ${deque.length - 1}.`, fillCells(capacity, deque, { idx: deque.length - 1, state: 'inserted' }), ptr, true, opIdx);
      }
    } else {
      const isFront = op.kind === 'popFront';
      if (deque.length === 0) {
        push(label, 'LỖI: deque rỗng, không thể lấy phần tử.', emptyCells(capacity), {}, false, opIdx);
        continue;
      }
      const idx = isFront ? 0 : deque.length - 1;
      const v = deque[idx];
      const ptr: Record<number, string[]> = {};
      addPointer(ptr, 0, 'FRONT');
      addPointer(ptr, deque.length - 1, 'REAR');
      push(label, `${isFront ? 'Pop trước' : 'Pop sau'}: x ← ${v} — đang lấy phần tử ${isFront ? 'đầu' : 'cuối'}.`, fillCells(capacity, deque, { idx, state: 'removed' }), ptr, true, opIdx);
      if (isFront) deque.shift(); else deque.pop();
      const ptr2: Record<number, string[]> = {};
      if (deque.length > 0) { addPointer(ptr2, 0, 'FRONT'); addPointer(ptr2, deque.length - 1, 'REAR'); }
      const doneLog = isFront
        ? `Đã lấy ${v} ra — các phần tử còn lại dồn lên đầu, rear → ${deque.length - 1}.`
        : `Đã lấy ${v} ra — rear lùi về ${deque.length - 1}.`;
      push(label, doneLog, fillCells(capacity, deque), ptr2, true, opIdx);
    }
  }

  const ptr: Record<number, string[]> = {};
  if (deque.length > 0) { addPointer(ptr, 0, 'FRONT'); addPointer(ptr, deque.length - 1, 'REAR'); }
  push('Kết thúc', `Kết thúc: deque [${deque.join(', ')}] — front=0, rear=${deque.length - 1}.`, fillCells(capacity, deque), ptr, true, -1, true);
  return steps;
}

// ──────────────────────── Min/Max Stack (getMin/getMax O(1)) ────────────────
// Hai ngăn xếp song song: s chứa dữ liệu; minStack/maxStack lưu giá trị
// nhỏ/lớn nhất "đến thời điểm hiện tại" — pop đồng bộ cả 3 → getMin/getMax O(1).
export function generateMinMaxStackSteps(ops: DsOp[], capacity: number): DsStep[] {
  const steps: DsStep[] = [];
  const stack: number[] = [];
  const minStack: number[] = [];
  const maxStack: number[] = [];
  let step = 0;

  const rowsFor = (s: number[], mn: number[], mx: number[]): Array<{ label: string; cells: DsCell[] }> => [
    { label: 'MIN', cells: fillCells(capacity, mn) },
    { label: 'MAX', cells: fillCells(capacity, mx) },
  ];

  const push = (
    label: string, log: string, cells: DsCell[], pointers: Record<number, string[]>,
    ok: boolean, opIndex: number, isFinal = false, focus?: { idx: number; state: DsCellState },
  ): void => {
    const top = stack.length - 1;
    const curMin = minStack.length > 0 ? minStack[minStack.length - 1] : null;
    const curMax = maxStack.length > 0 ? maxStack[maxStack.length - 1] : null;
    const rows = rowsFor(stack, minStack, maxStack);
    if (focus) {
      rows.forEach((r) => {
        if (focus.idx >= 0 && focus.idx < r.cells.length) {
          r.cells[focus.idx] = { val: r.cells[focus.idx].val, state: focus.state };
        }
      });
    }
    steps.push({
      stepIndex: step++, cells, pointers, label, log, ok, isFinal, opIndex, extraRows: rows,
      indices: { top, size: stack.length, capacity, min: curMin ?? '—', max: curMax ?? '—' },
    });
  };

  push('Khởi tạo', `Bắt đầu: Min/Max Stack rỗng, dung lượng ${capacity} — song song duy trì min & max để getMin/getMax O(1).`, emptyCells(capacity), {}, true, -1);

  for (let opIdx = 0; opIdx < ops.length; opIdx++) {
    const op = ops[opIdx];
    const top = stack.length - 1;
    const label = `${op.kind === 'push' ? 'Push' : op.kind === 'pop' ? 'Pop' : 'Peek'}${op.value !== undefined ? ` ${op.value}` : ''}`;

    if (op.kind === 'push') {
      const v = op.value ?? 0;
      if (stack.length >= capacity) {
        const ptr: Record<number, string[]> = {};
        addPointer(ptr, capacity - 1, 'TOP');
        push(label, `LỖI: tràn ngăn xếp — top=${top} = capacity-1=${capacity - 1}, không thể push ${v}.`, fillCells(capacity, stack, { idx: capacity - 1, state: 'removed' }), ptr, false, opIdx, false, { idx: capacity - 1, state: 'removed' });
        continue;
      }
      push(label, `Kiểm tra top=${top} < capacity-1 → còn chỗ; min hiện tại = ${minStack.length ? minStack[minStack.length - 1] : '—'}, max = ${maxStack.length ? maxStack[maxStack.length - 1] : '—'}.`, fillCells(capacity, stack, { idx: stack.length, state: 'active' }), { [stack.length]: ['TOP'] }, true, opIdx, false, { idx: stack.length, state: 'active' });
      const newMin = minStack.length > 0 ? Math.min(minStack[minStack.length - 1], v) : v;
      const newMax = maxStack.length > 0 ? Math.max(maxStack[maxStack.length - 1], v) : v;
      stack.push(v);
      minStack.push(newMin);
      maxStack.push(newMax);
      const ptr: Record<number, string[]> = {};
      addPointer(ptr, stack.length - 1, 'TOP');
      push(label, `Ghi s[${stack.length - 1}] ← ${v} — min mới = ${newMin}, max mới = ${newMax} (đồng bộ 3 ngăn xếp).`, fillCells(capacity, stack, { idx: stack.length - 1, state: 'inserted' }), ptr, true, opIdx, false, { idx: stack.length - 1, state: 'inserted' });
    } else if (op.kind === 'pop') {
      if (stack.length === 0) {
        push(label, 'LỖI: ngăn xếp rỗng (top = -1), không thể pop.', emptyCells(capacity), {}, false, opIdx);
        continue;
      }
      const v = stack[top];
      push(label, `Pop: x ← s[${top}] = ${v} — đang lấy phần tử đỉnh (pop đồng bộ cả min/max).`, fillCells(capacity, stack, { idx: top, state: 'removed' }), { [top]: ['TOP'] }, true, opIdx, false, { idx: top, state: 'removed' });
      stack.pop();
      minStack.pop();
      maxStack.pop();
      const ptr: Record<number, string[]> = {};
      if (stack.length > 0) addPointer(ptr, stack.length - 1, 'TOP');
      push(label, `Đã lấy ${v} ra — min mới = ${minStack.length ? minStack[minStack.length - 1] : '—'}, max mới = ${maxStack.length ? maxStack[maxStack.length - 1] : '—'}.`, fillCells(capacity, stack), ptr, true, opIdx);
    } else {
      if (stack.length === 0) {
        push(label, 'LỖI: ngăn xếp rỗng (top = -1), không thể peek.', emptyCells(capacity), {}, false, opIdx);
        continue;
      }
      const v = stack[top];
      push(label, `Peek: s[${top}] = ${v} — getMin() = ${minStack[minStack.length - 1]}, getMax() = ${maxStack[maxStack.length - 1]} (cả 2 đều O(1)).`, fillCells(capacity, stack, { idx: top, state: 'highlight' }), { [top]: ['TOP'] }, true, opIdx, false, { idx: top, state: 'highlight' });
    }
  }

  const ptr: Record<number, string[]> = {};
  if (stack.length > 0) addPointer(ptr, stack.length - 1, 'TOP');
  push('Kết thúc', `Kết thúc: min = ${minStack.length ? minStack[minStack.length - 1] : '—'}, max = ${maxStack.length ? maxStack[maxStack.length - 1] : '—'} — ngăn xếp [${stack.join(', ')}].`, fillCells(capacity, stack), ptr, true, -1, true);
  return steps;
}

export function generateDsSteps(mode: DsMode, ops: DsOp[], capacity: number): DsStep[] {
  switch (mode) {
    case 'stack': return generateStackSteps(ops, capacity);
    case 'minmax': return generateMinMaxStackSteps(ops, capacity);
    case 'queue': return generateQueueSteps(ops, capacity);
    case 'circular': return generateCircularQueueSteps(ops, capacity);
    case 'deque': return generateDequeSteps(ops, capacity);
  }
}

/** Sinh chuỗi thao tác ngẫu nhiên hợp lệ cho chế độ hiện tại. */
export function generateRandomOps(mode: DsMode, capacity: number): DsOp[] {
  const ops: DsOp[] = [];
  const count = 6 + Math.floor(Math.random() * 4);
  let size = 0;

  const rand = (): number => 1 + Math.floor(Math.random() * 99);

  for (let i = 0; i < count; i++) {
    const choices: DsOp[] = [];
    if (mode === 'stack' || mode === 'minmax') {
      if (size < capacity) choices.push({ kind: 'push', value: rand() });
      if (size > 0) { choices.push({ kind: 'pop' }); choices.push({ kind: 'peek' }); }
    } else if (mode === 'queue' || mode === 'circular') {
      if (size < capacity) choices.push({ kind: 'push', value: rand() });
      if (size > 0) choices.push({ kind: 'pop' });
    } else {
      if (size < capacity) { choices.push({ kind: 'push', value: rand() }); choices.push({ kind: 'pushFront', value: rand() }); }
      if (size > 0) { choices.push({ kind: 'pop' }); choices.push({ kind: 'popFront' }); }
    }
    if (choices.length === 0) break;
    const chosen = choices[Math.floor(Math.random() * choices.length)];
    ops.push(chosen);
    if (chosen.kind === 'push' || chosen.kind === 'pushFront') size++;
    else if (chosen.kind === 'pop' || chosen.kind === 'popFront') size--;
  }
  return ops;
}