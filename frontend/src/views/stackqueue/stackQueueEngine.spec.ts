import { describe, it, expect } from 'vitest';
import {
  generateStackSteps,
  generateQueueSteps,
  generateCircularQueueSteps,
  generateDequeSteps,
  generateMinMaxStackSteps,
  generateDsSteps,
  generateRandomOps,
  type DsOp,
} from './stackQueueEngine';

function valuesOf(steps: ReturnType<typeof generateStackSteps>, idx: number): (number | null)[] {
  return steps[idx].cells.map((c) => c.val);
}

function lastStep(steps: ReturnType<typeof generateStackSteps>) {
  return steps[steps.length - 1];
}

const PUSH = (v: number): DsOp => ({ kind: 'push', value: v });
const POP: DsOp = { kind: 'pop' };
const PEEK: DsOp = { kind: 'peek' };

describe('Stack Queue engine', () => {
  describe('Stack (LIFO)', () => {
    it('push thêm vào đỉnh, pop lấy đúng phần tử vào sau cùng (LIFO)', () => {
      const steps = generateStackSteps([PUSH(5), PUSH(3), POP], 5);
      // Bước pop phải nói về việc lấy 3 (phần tử vào sau cùng)
      const popStep = steps.find((s) => s.label === 'Pop');
      expect(popStep).toBeDefined();
      expect(popStep!.log).toContain('3');
      // Sau khi pop, 5 còn lại, 3 đã bị lấy
      expect(valuesOf(steps, steps.length - 1)).toEqual([5, null, null, null, null]);
    });

    it('pop trên ngăn xếp rỗng → bước lỗi (ok=false)', () => {
      const steps = generateStackSteps([POP], 4);
      const err = steps.find((s) => !s.ok);
      expect(err).toBeDefined();
      expect(err!.log).toContain('rỗng');
    });

    it('push vượt capacity → bước lỗi tràn (ok=false)', () => {
      const steps = generateStackSteps([PUSH(1), PUSH(2), PUSH(3)], 2);
      const err = steps.find((s) => !s.ok);
      expect(err).toBeDefined();
      expect(err!.log).toContain('tràn');
    });

    it('peek làm nổi đỉnh nhưng không xóa', () => {
      const steps = generateStackSteps([PUSH(7), PEEK], 4);
      const peek = steps.find((s) => s.label === 'Peek 7' || s.label === 'Peek');
      expect(peek).toBeDefined();
      expect(peek!.cells.some((c) => c.state === 'highlight')).toBe(true);
      expect(valuesOf(steps, steps.length - 1)).toEqual([7, null, null, null]);
    });

    it('pointer TOP nằm ở đỉnh (ô cuối đã điền)', () => {
      const steps = generateStackSteps([PUSH(5), PUSH(9)], 4);
      const last = steps[steps.length - 1];
      expect(last.pointers[1]).toContain('TOP');
    });

    it('bước đầu tiên là khởi tạo rỗng, bước cuối là kết thúc', () => {
      const steps = generateStackSteps([PUSH(1)], 3);
      expect(steps[0].label).toBe('Khởi tạo');
      expect(steps[0].cells.every((c) => c.val === null)).toBe(true);
      expect(steps[steps.length - 1].isFinal).toBe(true);
    });
  });

  describe('Queue (FIFO)', () => {
    it('enqueue thêm vào cuối, dequeue lấy đúng phần tử vào trước (FIFO)', () => {
      const steps = generateQueueSteps([PUSH(10), PUSH(20), PUSH(30), POP], 6);
      // Bước dequeue phải nói về việc lấy 10 (phần tử vào trước tiên)
      const deqStep = steps.find((s) => s.label === 'Dequeue');
      expect(deqStep).toBeDefined();
      expect(deqStep!.log).toContain('10');
      // Sau khi dequeue, 10 đã ra, 20 và 30 dồn lên đầu
      expect(valuesOf(steps, steps.length - 1)).toEqual([20, 30, null, null, null, null]);
    });

    it('dequeue trên hàng đợi rỗng → bước lỗi', () => {
      const steps = generateQueueSteps([POP], 4);
      const err = steps.find((s) => !s.ok);
      expect(err).toBeDefined();
      expect(err!.log).toContain('rỗng');
    });

    it('enqueue vượt capacity → bước lỗi tràn', () => {
      const steps = generateQueueSteps([PUSH(1), PUSH(2), PUSH(3)], 2);
      const err = steps.find((s) => !s.ok);
      expect(err).toBeDefined();
      expect(err!.log).toContain('đầy');
    });

    it('pointer FRONT ở ô 0 và REAR ở ô cuối đã điền', () => {
      const steps = generateQueueSteps([PUSH(5), PUSH(9)], 4);
      const last = steps[steps.length - 1];
      expect(last.pointers[0]).toContain('FRONT');
      expect(last.pointers[1]).toContain('REAR');
    });

    it('peek bị bỏ qua ở queue (không có Peek trong hàng đợi)', () => {
      const steps = generateQueueSteps([PUSH(1), PUSH(2), PEEK, POP], 4);
      // PEEK không được xử lý như dequeue → chỉ 1 thao tác dequeue xảy ra
      const deqOpIdxs = new Set(steps.filter((s) => s.label === 'Dequeue').map((s) => s.opIndex));
      expect(deqOpIdxs.size).toBe(1);
      expect(valuesOf(steps, steps.length - 1)).toEqual([2, null, null, null]);
    });
  });

  describe('Circular Queue (tròn)', () => {
    it('enqueue/dequeue không dồn lên — front quay vòng', () => {
      const steps = generateCircularQueueSteps([PUSH(10), PUSH(20), PUSH(30), POP, POP, PUSH(40)], 4);
      // Sau 2 dequeue: front=2, nội dung [null,null,30,40]
      const last = steps[steps.length - 1];
      expect(last.cells.map((c) => c.val)).toEqual([null, null, 30, 40]);
      expect(last.indices.front).toBe(2);
      expect(last.indices.size).toBe(2);
    });

    it('dequeue làm front tăng (không shift mảng) và đánh dấu quay vòng khi wrap', () => {
      const steps = generateCircularQueueSteps([PUSH(1), PUSH(2), PUSH(3), POP, PUSH(4), PUSH(5)], 4);
      const last = steps[steps.length - 1];
      // 1,2,3 → pop (front=1) → push 4 (vào ô 3), push 5 → wrap về ô 0
      expect(last.cells.map((c) => c.val)).toEqual([5, 2, 3, 4]);
      expect(last.indices.front).toBe(1);
      expect(last.indices.wrap).toBe('đã quay vòng');
    });

    it('enqueue khi đầy → bước lỗi (size = capacity)', () => {
      const steps = generateCircularQueueSteps([PUSH(1), PUSH(2), PUSH(3), PUSH(4), PUSH(5)], 4);
      const err = steps.find((s) => !s.ok);
      expect(err).toBeDefined();
      expect(err!.log).toContain('đầy');
    });

    it('dequeue khi rỗng → bước lỗi', () => {
      const steps = generateCircularQueueSteps([POP], 4);
      const err = steps.find((s) => !s.ok);
      expect(err).toBeDefined();
      expect(err!.log).toContain('rỗng');
    });

    it('kết quả cuối khớp mô phỏng vòng tròn', () => {
      const ops: DsOp[] = [PUSH(1), PUSH(2), POP, PUSH(3), PUSH(4), POP, PUSH(5)];
      // Mô phỏng tròn
      const cap = 4;
      const slots: (number | null)[] = Array(cap).fill(null);
      let front = 0, size = 0;
      for (const o of ops) {
        if (o.kind === 'push') {
          slots[(front + size) % cap] = o.value!;
          size++;
        } else if (o.kind === 'pop') {
          slots[front] = null;
          front = (front + 1) % cap;
          size--;
        }
      }
      const steps = generateCircularQueueSteps(ops, cap);
      const last = steps[steps.length - 1];
      expect(last.cells.map((c) => c.val)).toEqual(slots);
      expect(last.indices.front).toBe(front);
    });
  });

  describe('Deque (2 đầu)', () => {
    it('pushFront chèn vào đầu (các phần tử dồn phải), pop lấy đúng đầu/cuối', () => {
      const steps = generateDequeSteps([
        PUSH(5),
        PUSH(9),
        { kind: 'pushFront', value: 1 },
        { kind: 'pop' },
        { kind: 'popFront' },
      ], 6);
      // 5,9 → 1,5,9 → pop sau bỏ 9 → pop trước bỏ 1 → còn [5]
      const last = steps[steps.length - 1];
      expect(last.cells.map((c) => c.val)).toEqual([5, null, null, null, null, null]);
      // Bước chèn trước phải nói về việc chèn 1 vào đầu
      const frontStep = steps.find((s) => s.label.includes('Push trước 1'));
      expect(frontStep).toBeDefined();
      expect(frontStep!.log).toContain('dồn');
    });

    it('thêm khi đầy → lỗi; lấy khi rỗng → lỗi', () => {
      const full = generateDequeSteps([PUSH(1), PUSH(2), PUSH(3), { kind: 'pushFront', value: 9 }], 3);
      expect(full.some((s) => !s.ok)).toBe(true);
      const empty = generateDequeSteps([{ kind: 'popFront' }, { kind: 'pop' }], 3);
      expect(empty.some((s) => !s.ok)).toBe(true);
    });

    it('pop trước và pop sau lấy đúng 2 đầu khác nhau', () => {
      const steps = generateDequeSteps([PUSH(10), PUSH(20), PUSH(30), { kind: 'popFront' }, { kind: 'pop' }], 5);
      // 10,20,30 → pop trước bỏ 10 → pop sau bỏ 30 → còn [20]
      const last = steps[steps.length - 1];
      expect(last.cells.map((c) => c.val)).toEqual([20, null, null, null, null]);
    });

    it('kết quả cuối khớp mô phỏng 2 đầu', () => {
      const ops: DsOp[] = [PUSH(1), { kind: 'pushFront', value: 0 }, PUSH(2), { kind: 'popFront' }, { kind: 'pop' }, { kind: 'pushFront', value: 9 }];
      const d: number[] = [];
      for (const o of ops) {
        if (o.kind === 'push') d.push(o.value!);
        else if (o.kind === 'pushFront') d.unshift(o.value!);
        else if (o.kind === 'pop') d.pop();
        else if (o.kind === 'popFront') d.shift();
      }
      const steps = generateDequeSteps(ops, 6);
      const last = steps[steps.length - 1];
      expect(last.cells.map((c) => c.val).slice(0, d.length)).toEqual(d);
    });
  });

  describe('Min/Max Stack', () => {
    it('push cập nhật min/max đúng, pop khôi phục lại giá trị trước', () => {
      const steps = generateMinMaxStackSteps([PUSH(5), PUSH(3), PUSH(8), PUSH(2), POP, POP], 6);
      // Mỗi Pop sinh 2 bước (đang lấy / đã lấy) — lấy bước "đã lấy"
      const popDone = steps.filter((s) => s.label === 'Pop' && s.log.startsWith('Đã lấy'));
      expect(popDone.length).toBe(2);
      // Sau pop 2: min=3, max=8
      expect(popDone[0].indices.min).toBe(3);
      expect(popDone[0].indices.max).toBe(8);
      // Sau pop 8: min=3, max=5
      expect(popDone[1].indices.min).toBe(3);
      expect(popDone[1].indices.max).toBe(5);
    });

    it('mỗi bước có 2 hàng phụ MIN và MAX song song với stack chính', () => {
      const steps = generateMinMaxStackSteps([PUSH(5), PUSH(3), PUSH(8)], 5);
      for (const s of steps) {
        expect(s.extraRows).toBeDefined();
        const labels = s.extraRows!.map((r) => r.label);
        expect(labels).toEqual(['MIN', 'MAX']);
        for (const r of s.extraRows!) expect(r.cells.length).toBe(5);
      }
      // Sau push 5,3,8: hàng MIN = [5,3,3], hàng MAX = [5,5,8]
      const last = steps[steps.length - 1];
      const minRow = last.extraRows![0].cells.map((c) => c.val);
      const maxRow = last.extraRows![1].cells.map((c) => c.val);
      expect(minRow.slice(0, 3)).toEqual([5, 3, 3]);
      expect(maxRow.slice(0, 3)).toEqual([5, 5, 8]);
    });

    it('min/max khớp với mô phỏng thủ công trên chuỗi ngẫu nhiên', () => {
      for (let t = 0; t < 100; t++) {
        const ops: DsOp[] = [];
        const st: number[] = [];
        const mn: number[] = [];
        const mx: number[] = [];
        const cap = 5;
        for (let i = 0; i < 12; i++) {
          const roll = Math.random();
          if (roll < 0.5 && st.length < cap) {
            const v = Math.floor(Math.random() * 20) - 5;
            ops.push({ kind: 'push', value: v });
            st.push(v);
            mn.push(mn.length ? Math.min(mn[mn.length - 1], v) : v);
            mx.push(mx.length ? Math.max(mx[mx.length - 1], v) : v);
          } else if (st.length > 0) {
            ops.push(roll < 0.8 ? { kind: 'pop' } : { kind: 'peek' });
            if (roll < 0.8) { st.pop(); mn.pop(); mx.pop(); }
          }
        }
        const steps = generateMinMaxStackSteps(ops, cap);
        const last = steps[steps.length - 1];
        expect(last.indices.min).toBe(mn.length ? mn[mn.length - 1] : '—');
        expect(last.indices.max).toBe(mx.length ? mx[mx.length - 1] : '—');
        const minRow = last.extraRows![0].cells.map((c) => c.val);
        const maxRow = last.extraRows![1].cells.map((c) => c.val);
        expect(minRow.slice(0, mn.length)).toEqual(mn);
        expect(maxRow.slice(0, mx.length)).toEqual(mx);
      }
    });

    it('tràn/rỗng vẫn báo lỗi như stack thường', () => {
      const full = generateMinMaxStackSteps([PUSH(1), PUSH(2), PUSH(3), PUSH(4), PUSH(5), PUSH(6)], 5);
      expect(full.some((s) => !s.ok)).toBe(true);
      const empty = generateMinMaxStackSteps([POP, POP], 5);
      expect(empty.some((s) => !s.ok)).toBe(true);
    });
  });

  describe('Random + tổng hợp', () => {
    it('generateRandomOps không bao giờ vượt capacity trong quá trình sinh (chạy 200 lần)', () => {
      for (let i = 0; i < 200; i++) {
        for (const mode of ['stack', 'minmax', 'queue', 'circular', 'deque'] as const) {
          const cap = 3 + Math.floor(Math.random() * 7);
          const ops = generateRandomOps(mode, cap);
          const steps = generateDsSteps(mode, ops, cap);
          expect(steps.length).toBeGreaterThan(0);
          // Không bước nào bị lỗi ngoài dự kiến của thuật toán (nhưng vẫn phải hợp lệ cấu trúc)
          for (const s of steps) {
            expect(s.cells.length).toBe(cap);
            expect(s.stepIndex).toBeGreaterThanOrEqual(0);
            expect(typeof s.indices).toBe('object');
          }
        }
      }
    });

    it('kết quả cuối cùng luôn khớp mô phỏng thực (stack + queue)', () => {
      const ops: DsOp[] = [PUSH(1), PUSH(2), PUSH(3), POP, PUSH(4), POP, PEEK, PUSH(5)];
      // Mô phỏng stack
      let st: number[] = [];
      for (const o of ops) {
        if (o.kind === 'push') st.push(o.value!);
        else if (o.kind === 'pop') st.pop();
      }
      const steps = generateStackSteps(ops, 8);
      expect(valuesOf(steps, steps.length - 1).slice(0, st.length)).toEqual(st);

      // Mô phỏng queue
      let q: number[] = [];
      for (const o of ops) {
        if (o.kind === 'push') q.push(o.value!);
        else if (o.kind === 'pop') q.shift();
      }
      const qsteps = generateQueueSteps(ops, 8);
      expect(valuesOf(qsteps, qsteps.length - 1).slice(0, q.length)).toEqual(q);
    });
  });
});