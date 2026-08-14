import { mount } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';

import type { Step } from '@/engines/core/types';
import ManualPracticePanel from './ManualPracticePanel.vue';

// ── ManualPracticePanel — chế độ Tự thực hành (FR-3.12) ──
// UX fix: empty state khi chưa chạy sim (steps rỗng) + auto step-forward sau khi "Kiểm tra".

function makeStep(index: number, explanation: string): Step {
  return {
    index,
    structure: { kind: 'array', elements: [], links: [] },
    explanation,
    pseudocodeLine: 1,
    highlights: [],
    annotations: [],
    variables: {},
    stats: { comparisons: 0, swaps: 0, writes: 0 },
    version: 1,
  };
}

const EMPTY_HINT = 'Chọn nút ▶ trên thanh điều khiển để chạy mô phỏng';

function mountPanel(steps: Step[], currentIndex: number) {
  return mount(ManualPracticePanel, {
    props: { steps, currentIndex },
  });
}

describe('ManualPracticePanel — empty state (chưa chạy simulation)', () => {
  it('steps rỗng → hiện hint hướng dẫn, KHÔNG render options/actions', () => {
    const wrapper = mountPanel([], 0);
    expect(wrapper.text()).toContain(EMPTY_HINT);
    expect(wrapper.find('[role="radiogroup"]').exists()).toBe(false);
    expect(wrapper.text()).not.toContain('Bỏ qua bước');
    expect(wrapper.text()).not.toContain('Kiểm tra');
  });

  it('steps rỗng → không gắn class active accent', () => {
    const wrapper = mountPanel([], 0);
    expect(wrapper.find('section.practice').classes()).not.toContain('practice--active');
  });
});

describe('ManualPracticePanel — panel hoạt động', () => {
  it('có steps → hiện prompt + options + actions, gắn class active accent', () => {
    const wrapper = mountPanel([makeStep(0, 'Bắt đầu'), makeStep(1, 'Hoán đổi hai phần tử')], 0);
    expect(wrapper.text()).toContain('Bước kế tiếp của thuật toán là gì?');
    expect(wrapper.find('[role="radiogroup"]').exists()).toBe(true);
    expect(wrapper.find('section.practice').classes()).toContain('practice--active');
    expect(wrapper.find('button:not([disabled])').exists()).toBe(true);
  });

  it('Kiểm tra ĐÚNG → emit skip (tự chuyển bước) + tăng điểm đúng', async () => {
    const wrapper = mountPanel([makeStep(0, 'Bắt đầu'), makeStep(1, 'Hoán đổi hai phần tử')], 0);
    await wrapper.find('input[value="swap"]').setValue(true);
    await wrapper.findAll('button').find((b) => b.text() === 'Kiểm tra')!.trigger('click');

    expect(wrapper.emitted('skip')).toHaveLength(1);
    expect(wrapper.text()).toContain('Đúng 1 · Sai 0');
    expect(wrapper.text()).toContain('✓ Chính xác!');
  });

  it('Kiểm tra SAI → emit skip (tự chuyển bước) + tăng điểm sai', async () => {
    const wrapper = mountPanel([makeStep(0, 'Bắt đầu'), makeStep(1, 'Hoán đổi hai phần tử')], 0);
    await wrapper.find('input[value="move"]').setValue(true);
    await wrapper.findAll('button').find((b) => b.text() === 'Kiểm tra')!.trigger('click');

    expect(wrapper.emitted('skip')).toHaveLength(1);
    expect(wrapper.text()).toContain('Đúng 0 · Sai 1');
    expect(wrapper.text()).toContain('✗ Chưa đúng');
  });

  it('Bỏ qua bước → vẫn emit skip', async () => {
    const wrapper = mountPanel([makeStep(0, 'Bắt đầu'), makeStep(1, 'Hoán đổi hai phần tử')], 0);
    await wrapper.findAll('button').find((b) => b.text() === 'Bỏ qua bước')!.trigger('click');
    expect(wrapper.emitted('skip')).toHaveLength(1);
  });

  it('Kết thúc → emit done với kết quả đúng/sai', async () => {
    const wrapper = mountPanel([makeStep(0, 'Bắt đầu'), makeStep(1, 'Hoán đổi hai phần tử')], 0);
    await wrapper.find('input[value="swap"]').setValue(true);
    await wrapper.findAll('button').find((b) => b.text() === 'Kiểm tra')!.trigger('click');
    await wrapper.findAll('button').find((b) => b.text() === 'Kết thúc')!.trigger('click');

    expect(wrapper.emitted('done')).toHaveLength(1);
    expect(wrapper.emitted('done')![0][0]).toEqual({ correct: 1, wrong: 0 });
  });
});
