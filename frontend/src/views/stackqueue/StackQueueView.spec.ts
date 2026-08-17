// @vitest-environment jsdom
import { describe, it, expect } from 'vitest';
import { mount } from '@vue/test-utils';
import StackQueueView from './StackQueueView.vue';

describe('StackQueueView mount', () => {
  it('render top bar + canvas + VCR không lỗi', () => {
    const wrapper = mount(StackQueueView, {
      global: { stubs: { BaseIcon: { template: '<span />' } } },
    });
    expect(wrapper.text()).toContain('Stack & Queue Sandbox');
    expect(wrapper.text()).toContain('Stack');
    expect(wrapper.text()).toContain('Queue');
    expect(wrapper.findAll('.sq-cell').length).toBeGreaterThan(0);
    expect(wrapper.findAll('.sq-vcr').length).toBe(1);
  });

  it('bấm Push sinh thao tác + xuất hiện giá trị trong các bước', async () => {
    const wrapper = mount(StackQueueView, {
      global: { stubs: { BaseIcon: { template: '<span />' } } },
    });
    const vm = wrapper.vm as any;
    vm.addOp({ kind: 'push', value: 7 });
    await wrapper.vm.$nextTick();
    expect(vm.steps.length).toBeGreaterThan(0);
    expect(vm.steps.some((s: { cells: Array<{ val: number | null }> }) => s.cells.some((c) => c.val === 7))).toBe(true);
  });

  it('Stack gốc có 2 biến thể (Cơ bản/Min-Max) — chọn Min/Max hiện 2 hàng MIN/MAX', async () => {
    const wrapper = mount(StackQueueView, {
      global: { stubs: { BaseIcon: { template: '<span />' } } },
    });
    const vm = wrapper.vm as any;
    // Stack root mặc định: 2 pills biến thể trên thanh canvas (như Sorting Sandbox)
    const labels = wrapper.findAll('.sq-variant-pill').map((p) => p.text());
    expect(labels).toContain('Cơ bản');
    expect(labels).toContain('Min/Max');
    expect(vm.effectiveMode).toBe('stack');
    // Chọn biến thể Min/Max
    vm.setVariant('minmax');
    await wrapper.vm.$nextTick();
    expect(vm.effectiveMode).toBe('minmax');
    // Canvas phải có hàng phụ MIN/MAX
    expect(wrapper.findAll('.sq-row-extra').length).toBe(2);
    expect(wrapper.text()).toContain('MIN');
    expect(wrapper.text()).toContain('MAX');
  });

  it('Queue gốc có 3 biến thể con (FIFO/Tròn/Deque) — chọn Deque hiện nút 2 đầu', async () => {
    const wrapper = mount(StackQueueView, {
      global: { stubs: { BaseIcon: { template: '<span />' } } },
    });
    const vm = wrapper.vm as any;
    vm.setRoot('queue');
    await wrapper.vm.$nextTick();
    // 3 pills biến thể trên thanh canvas
    const pills = wrapper.findAll('.sq-variant-pill');
    const labels = pills.map((p) => p.text());
    expect(labels).toContain('FIFO');
    expect(labels).toContain('Tròn');
    expect(labels).toContain('Deque');
    // Chưa có nút Thêm trước (deque chưa chọn)
    expect(wrapper.text()).not.toContain('Thêm trước');
    // Chọn biến thể Deque → có nút 2 đầu
    vm.setVariant('deque');
    await wrapper.vm.$nextTick();
    expect(wrapper.text()).toContain('Thêm trước');
    expect(wrapper.text()).toContain('Bỏ trước');
    expect(vm.effectiveMode).toBe('deque');
  });

  it('preset Tràn sinh bước lỗi khi vượt capacity', async () => {
    const wrapper = mount(StackQueueView, {
      global: { stubs: { BaseIcon: { template: '<span />' } } },
    });
    const vm = wrapper.vm as any;
    vm.loadPreset(vm.presets.find((p: { key: string }) => p.key === 'overflow'));
    await wrapper.vm.$nextTick();
    expect(vm.steps.some((s: { ok: boolean }) => !s.ok)).toBe(true);
  });
});