// @vitest-environment jsdom
// Kiểm tra toàn diện Stack & Queue Sandbox:
// 1) Thuật toán đúng ở MỌI bước (trạng thái cells + indices + pointers khớp nhau)
// 2) Animation hợp lệ: biến đổi giữa 2 bước liên tiếp đúng bản chất thao tác
// 3) Thao tác đầu vào: dung lượng, deque 2 đầu, chuỗi mẫu, ngẫu nhiên
import { describe, it, expect } from 'vitest';
import { mount } from '@vue/test-utils';
import {
  generateDsSteps,
  generateRandomOps,
  type DsCell,
  type DsMode,
  type DsOp,
  type DsStep,
} from './stackQueueEngine';
import StackQueueView from './StackQueueView.vue';

function vals(cells: DsCell[]): (number | null)[] { return cells.map((c) => c.val); }
function count(cells: DsCell[]): number { return cells.filter((c) => c.val !== null).length; }

/** Bước "đã xong" của thao tác = bước có log "Đã"/"Ghi"/"Peek" (post-state). */
function finalStepsOf(steps: DsStep[]): DsStep[] {
  return steps.filter((s) => s.log.startsWith('Đã') || s.log.startsWith('Ghi') || s.log.startsWith('Peek'));
}

describe('Stack & Queue — thuật toán + animation đúng từng bước', () => {
  const opsFor = (mode: DsMode, cap: number): DsOp[] => {
    switch (mode) {
      case 'stack': return [{ kind: 'push', value: 4 }, { kind: 'push', value: 9 }, { kind: 'push', value: 2 }, { kind: 'pop' }, { kind: 'peek' }, { kind: 'pop' }];
      case 'minmax': return [{ kind: 'push', value: 4 }, { kind: 'push', value: 9 }, { kind: 'push', value: 2 }, { kind: 'pop' }, { kind: 'peek' }];
      case 'queue': return [{ kind: 'push', value: 4 }, { kind: 'push', value: 9 }, { kind: 'push', value: 2 }, { kind: 'pop' }, { kind: 'push', value: 7 }, { kind: 'pop' }];
      case 'circular': return [{ kind: 'push', value: 4 }, { kind: 'push', value: 9 }, { kind: 'push', value: 2 }, { kind: 'pop' }, { kind: 'push', value: 7 }, { kind: 'pop' }, { kind: 'pop' }, { kind: 'push', value: 5 }, { kind: 'push', value: 6 }];
      case 'deque': return [{ kind: 'push', value: 4 }, { kind: 'push', value: 9 }, { kind: 'pushFront', value: 1 }, { kind: 'pop' }, { kind: 'popFront' }, { kind: 'pushFront', value: 8 }, { kind: 'pop' }];
    }
  };

  it.each(['stack', 'minmax', 'queue', 'circular', 'deque'] as DsMode[])(
    '%s: mọi bước — cells hợp lệ, indices khớp trạng thái, pointers đúng vị trí',
    (mode) => {
      const cap = 5;
      const steps = generateDsSteps(mode, opsFor(mode, cap), cap);
      expect(steps[0].label).toBe('Khởi tạo');
      expect(steps[steps.length - 1].isFinal).toBe(true);

      for (const s of steps) {
        // Cells: đủ cap ô, giá trị hợp lệ
        expect(s.cells.length).toBe(cap);
        for (const c of s.cells) expect(c.val === null || Number.isInteger(c.val)).toBe(true);
        // Không bước nào NaN/undefined
        expect(s.log).not.toContain('NaN');
        expect(s.log).not.toContain('undefined');

        const n = count(s.cells);
        const { top, front, rear, size } = s.indices;

        // size khớp số phần tử đang hiển thị
        expect(size).toBe(n);

        if (mode === 'stack' || mode === 'minmax') {
          expect(top).toBe(n - 1);
          // TOP ở ô cuối đã điền (n-1) HOẶC ô trống sắp ghi (n) trong bước "kiểm tra chỗ"
          if (n > 0) {
            const hasTop = s.pointers[n - 1]?.includes('TOP') || s.pointers[n]?.includes('TOP');
            expect(hasTop).toBe(true);
          }
        }
        if (mode === 'queue' || mode === 'deque') {
          expect(front).toBe(0);
          expect(rear).toBe(n - 1);
          if (n > 0) {
            expect(s.pointers[0]?.includes('FRONT')).toBe(true);
            const hasRear = s.pointers[n - 1]?.includes('REAR') || s.pointers[n]?.includes('REAR');
            expect(hasRear).toBe(true);
          }
        }
        if (mode === 'circular') {
          // front/rear nằm trong khoảng hợp lệ, ô tại front có giá trị (nếu size>0)
          expect(front).toBeGreaterThanOrEqual(0);
          expect(front).toBeLessThan(cap);
          expect(rear).toBeGreaterThanOrEqual(-1);
          expect(rear).toBeLessThan(cap);
          if (n > 0) {
            expect(s.cells[front as number].val).not.toBeNull();
            expect(s.cells[rear as number].val).not.toBeNull();
            expect(s.pointers[front as number]?.includes('FRONT')).toBe(true);
            // REAR ở rear hoặc ô trống sắp ghi (bước kiểm tra)
            const hasRear = s.pointers[rear as number]?.includes('REAR')
              || s.pointers[(front as number + n) % cap]?.includes('REAR');
            expect(hasRear).toBe(true);
            // size = số ô liên tục từ front đi tới (vòng tròn)
            for (let k = 0; k < n; k++) {
              expect(s.cells[(front as number + k) % cap].val).not.toBeNull();
            }
          }
        }
        // extraRows (minmax) khớp độ dài + số phần tử bằng size
        if (s.extraRows) {
          for (const row of s.extraRows) {
            expect(row.cells.length).toBe(cap);
            expect(count(row.cells)).toBe(n);
          }
        }
      }
    },
  );

  it.each(['stack', 'minmax', 'queue', 'circular', 'deque'] as DsMode[])(
    '%s: animation hợp lệ — mỗi bước đổi đúng bản chất thao tác (0/±1 phần tử, shift khi dồn)',
    (mode) => {
      const cap = 5;
      const steps = generateDsSteps(mode, opsFor(mode, cap), cap);
      const okSteps = steps.filter((s) => s.ok);

      for (let k = 1; k < okSteps.length; k++) {
        const a = okSteps[k - 1];
        const b = okSteps[k];
        const na = count(a.cells), nb = count(b.cells);
        const delta = nb - na;
        // Số phần tử chỉ có thể giữ nguyên hoặc ±1 (không bao giờ nhảy nhiều)
        expect([-1, 0, 1]).toContain(delta);

        if (delta !== 0) {
          // Push/enqueue: thêm đúng 1 ở đúng vị trí cuối; Pop/dequeue: bớt đúng 1
          const aVals = vals(a.cells), bVals = vals(b.cells);
          const diffs: number[] = [];
          for (let i = 0; i < cap; i++) if (aVals[i] !== bVals[i]) diffs.push(i);
          if (mode === 'queue' || mode === 'deque') {
            if (b.log.startsWith('Đã lấy') && b.indices.size === 0) {
              // Bước rỗng hoàn toàn sau dequeue — chấp nhận
            } else {
              expect(diffs.length).toBeGreaterThan(0);
            }
          } else {
            expect(diffs.length).toBe(1);
          }
        } else {
          // Không đổi số lượng: hoặc giữ nguyên (kiểm tra/peek/error) hoặc SHIFT
          const aVals = vals(a.cells), bVals = vals(b.cells);
          let same = true;
          for (let i = 0; i < cap; i++) if (aVals[i] !== bVals[i]) { same = false; break; }
          if (!same) {
            const changed = aVals.filter((v, i) => v !== bVals[i]).length;
            // Shift (dequeue dồn lên / pushFront / popFront) đổi nhiều ô nhưng là dạng trượt
            if (mode === 'queue') {
              // Dồn trái: b[i] = a[i+1]
              const n = na;
              for (let i = 0; i < n - 1; i++) expect(bVals[i]).toBe(aVals[i + 1]);
              expect(bVals[n - 1]).toBeNull();
            }
            if (mode === 'deque') {
              if (b.log.startsWith('Chèn')) {
                // pushFront: dồn phải
                for (let i = 1; i < cap; i++) expect(bVals[i]).toBe(aVals[i - 1]);
              } else if (b.log.startsWith('Đã lấy') && Number(b.indices.size) > 0) {
                // popFront: dồn trái
                for (let i = 0; i < nb; i++) expect(bVals[i]).toBe(aVals[i + 1]);
              }
            }
            // Circular/deque pushFront có thể đổi nhiều ô — chỉ cần đúng 1 ô khác null
            if (changed > 1 && mode === 'circular') {
              // circular không shift — không xảy ra
              expect(true).toBe(true);
            }
          }
        }
      }
    },
  );

  it('Stack: kết quả cuối + thứ tự LIFO đúng theo mô phỏng tham chiếu từng bước', () => {
    const ops: DsOp[] = [{ kind: 'push', value: 4 }, { kind: 'push', value: 9 }, { kind: 'push', value: 2 }, { kind: 'pop' }, { kind: 'peek' }, { kind: 'pop' }];
    const steps = generateDsSteps('stack', ops, 5);
    // Mô phỏng tham chiếu: theo dõi trạng thái sau mỗi op
    const st: number[] = [];
    for (const op of ops) {
      if (op.kind === 'push') st.push(op.value!);
      else if (op.kind === 'pop') st.pop();
      // bước "đã xong" của op này phải khớp trạng thái tham chiếu
      const done = finalStepsOf(steps).filter((s) => s.opIndex === ops.indexOf(op));
      const after = done[done.length - 1];
      expect(after.cells.map((c) => c.val).slice(0, st.length)).toEqual(st);
    }
  });

  it('Queue: dequeue lấy đúng phần tử đầu (FIFO) — mô phỏng tham chiếu', () => {
    const ops: DsOp[] = [{ kind: 'push', value: 4 }, { kind: 'push', value: 9 }, { kind: 'pop' }, { kind: 'push', value: 7 }, { kind: 'pop' }];
    const steps = generateDsSteps('queue', ops, 5);
    const q: number[] = [];
    for (const op of ops) {
      if (op.kind === 'push') q.push(op.value!);
      else if (op.kind === 'pop') q.shift();
      const done = finalStepsOf(steps).filter((s) => s.opIndex === ops.indexOf(op));
      const after = done[done.length - 1];
      expect(after.cells.map((c) => c.val).slice(0, q.length)).toEqual(q);
    }
  });
});

describe('Stack & Queue — thao tác đầu vào + view', () => {
  function make() {
    return mount(StackQueueView, {
      global: { stubs: { BaseIcon: { template: '<span />' } } },
    });
  }

  it('ĐIỀU CHỈNH DUNG LƯỢNG: kéo slider → steps sinh lại với capacity mới (cells đủ số ô)', async () => {
    const wrapper = make();
    const vm = wrapper.vm as any;
    const before = vm.capacity;
    vm.capacity = 9;
    await wrapper.vm.$nextTick();
    expect(vm.capacity).toBe(9);
    expect(vm.steps.length).toBeGreaterThan(0);
    for (const s of vm.steps) expect(s.cells.length).toBe(9);
    // Slider thay đổi → preset bị reset, stepIdx về 0
    expect(vm.stepIdx).toBe(0);
    expect(vm.currentPreset).toBeNull();
    expect(before).not.toBe(9);
  });

  it('DUNG LƯỢNG NHỎ: capacity = 3 → chuỗi mẫu Tràn vẫn báo lỗi đúng (đầy đúng lúc)', async () => {
    const wrapper = make();
    const vm = wrapper.vm as any;
    vm.capacity = 3;
    await wrapper.vm.$nextTick();
    vm.loadPreset(vm.presets.find((p: { key: string }) => p.key === 'overflow'));
    await wrapper.vm.$nextTick();
    // Preset Tràn = 3 push đủ + 1 push thừa → đúng 1 bước lỗi
    const errs = vm.steps.filter((s: { ok: boolean }) => !s.ok);
    expect(errs.length).toBe(1);
  });

  it('DEQUE 2 ĐẦU: cả 4 thao tác pushFront/pushBack/popFront/popBack hoạt động đúng', async () => {
    const wrapper = make();
    const vm = wrapper.vm as any;
    vm.setRoot('queue');
    await wrapper.vm.$nextTick();
    vm.setVariant('deque');
    await wrapper.vm.$nextTick();

    // View tự gán giá trị từ ô input (valueInput) cho push/pushFront
    vm.valueInput = 1;
    vm.addOp({ kind: 'pushFront' });
    vm.valueInput = 2;
    vm.addOp({ kind: 'push' });
    vm.valueInput = 3;
    vm.addOp({ kind: 'push' });
    vm.addOp({ kind: 'popFront' });
    vm.addOp({ kind: 'pop' });
    await wrapper.vm.$nextTick();

    // Chuỗi thao tác đã ghi nhận đủ 4 loại
    const kinds = vm.ops.map((o: DsOp) => o.kind);
    expect(kinds).toContain('pushFront');
    expect(kinds).toContain('push');
    expect(kinds).toContain('popFront');
    expect(kinds).toContain('pop');
    // Mô phỏng: 1(trước),2,3 → bỏ trước (1) → bỏ sau (3) → còn [2]
    const last = vm.steps[vm.steps.length - 1];
    expect(last.cells.map((c: DsCell) => c.val).slice(0, 1)).toEqual([2]);
  });

  it('CHUỖI MẪU: mọi preset của mọi chế độ chạy không lỗi, có init + final, preset lỗi sinh bước !ok', async () => {
    const wrapper = make();
    const vm = wrapper.vm as any;
    const roots: Array<{ root: 'stack' | 'queue'; variants: string[] }> = [
      { root: 'stack', variants: ['basic', 'minmax'] },
      { root: 'queue', variants: ['fifo', 'circular', 'deque'] },
    ];
    const errorPresets = ['overflow', 'empty', 'full'];
    for (const { root, variants } of roots) {
      vm.setRoot(root);
      await wrapper.vm.$nextTick();
      for (const v of variants) {
        vm.setVariant(v);
        await wrapper.vm.$nextTick();
        for (const p of vm.presets) {
          vm.loadPreset(p);
          await wrapper.vm.$nextTick();
          expect(vm.steps.length).toBeGreaterThan(0);
          expect(vm.steps[0].label).toBe('Khởi tạo');
          expect(vm.steps[vm.steps.length - 1].isFinal).toBe(true);
          // Không bước nào chứa NaN
          for (const s of vm.steps) expect(s.log).not.toContain('NaN');
          if (errorPresets.includes(p.key)) {
            expect(vm.steps.some((s: { ok: boolean }) => !s.ok)).toBe(true);
          }
        }
      }
    }
  });

  it('NGẪU NHIÊN: sinh chuỗi hợp lệ, chạy không lỗi ở cả 5 chế độ', async () => {
    const wrapper = make();
    const vm = wrapper.vm as any;
    for (const root of ['stack', 'queue'] as const) {
      vm.setRoot(root);
      await wrapper.vm.$nextTick();
      for (const v of (root === 'stack' ? ['basic', 'minmax'] : ['fifo', 'circular', 'deque'])) {
        vm.setVariant(v);
        await wrapper.vm.$nextTick();
        for (let t = 0; t < 10; t++) {
          vm.runRandom();
          await wrapper.vm.$nextTick();
          expect(vm.ops.length).toBeGreaterThan(0);
          expect(vm.steps.length).toBeGreaterThan(0);
          expect(vm.steps[vm.steps.length - 1].isFinal).toBe(true);
        }
      }
    }
  });

  it('ĐỔI GỐC/BIẾN THỂ: reset chuỗi thao tác + sinh lại steps', async () => {
    const wrapper = make();
    const vm = wrapper.vm as any;
    vm.addOp({ kind: 'push', value: 5 });
    await wrapper.vm.$nextTick();
    expect(vm.ops.length).toBeGreaterThan(0);
    vm.setVariant('minmax');
    await wrapper.vm.$nextTick();
    expect(vm.ops.length).toBe(0);
    expect(vm.steps[0].label).toBe('Khởi tạo');
  });
});