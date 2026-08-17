// components/visualizer/SharedVisualizerShell.spec.ts — component tests cho shared shell.
import { mount, type DOMWrapper } from '@vue/test-utils';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import type { ElementStatus, Step } from '@/engines/core/types';
import { legacyStepsToFrames } from '@/visualizer/stepToFrames';

import SharedVisualizerShell from './SharedVisualizerShell.vue';

function makeSteps(): Step[] {
  const mk = (index: number, statuses: ElementStatus[], explanation: string, line: number): Step => ({
    index,
    structure: {
      kind: 'array',
      elements: statuses.map((s, i) => ({ id: `c:${i}`, label: String([5, 3, 1][i]), status: s })),
      links: [],
    },
    explanation,
    pseudocodeLine: line,
    highlights: [],
    annotations: [explanation],
    variables: { j: index },
    stats: { comparisons: index, swaps: 0, writes: 0 },
    version: 1,
  });
  return [
    mk(0, ['default', 'default', 'default'], 'Bắt đầu mảng [5,3,1]', 1),
    mk(1, ['active', 'active', 'default'], 'So sánh a[0] và a[1]', 4),
    mk(2, ['done', 'done', 'done'], 'Kết thúc — mảng đã sắp xếp', 10),
  ];
}

const PSEUDO = ['procedure bubbleSort(a)', '  for i', '  for j', '    if a[j] > a[j+1]'];

function findBtn(wrapper: { findAll: (arg: string) => Array<DOMWrapper<Element>> }, label: string) {
  return wrapper.findAll('button').find((b) => b.text().includes(label));
}

function mountShell() {
  const frames = legacyStepsToFrames(makeSteps(), 'sort.bubble');
  return mount(SharedVisualizerShell, {
    props: { frames, pseudocode: PSEUDO, title: 'Bubble Sort', subtitle: 'Mảng · O(n²)' },
  });
}

describe('SharedVisualizerShell', () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });
  afterEach(() => {
    vi.useRealTimers();
  });

  it('render đúng bước đầu + stage array + pseudocode + explanation', () => {
    const wrapper = mountShell();
    expect(wrapper.find('[data-testid="shared-visualizer-shell"]').exists()).toBe(true);
    expect(wrapper.find('[data-testid="viz-step-label"]').text()).toBe('1 / 3');
    expect(wrapper.find('[data-testid="array-bars-renderer"]').exists()).toBe(true);
    expect(wrapper.find('[data-testid="viz-pseudo"]').exists()).toBe(true);
    expect(wrapper.find('[data-testid="viz-description"]').text()).toContain('Bắt đầu');
  });

  it('step-forward / step-back / reset cập nhật bước', async () => {
    const wrapper = mountShell();
    await findBtn(wrapper, 'Bước tới')?.trigger('click');
    expect(wrapper.find('[data-testid="viz-step-label"]').text()).toBe('2 / 3');
    expect(wrapper.find('[data-testid="viz-description"]').text()).toContain('So sánh');
    await findBtn(wrapper, 'Bước tới')?.trigger('click');
    expect(wrapper.find('[data-testid="viz-step-label"]').text()).toBe('3 / 3');
    await findBtn(wrapper, 'Đặt lại')?.trigger('click');
    expect(wrapper.find('[data-testid="viz-step-label"]').text()).toBe('1 / 3');
  });

  it('play dùng timer tăng bước rồi tự dừng ở cuối', async () => {
    const wrapper = mountShell();
    await findBtn(wrapper, 'Chạy')?.trigger('click');
    await wrapper.vm.$nextTick();
    // Play đã chạy → nút đổi thành Tạm dừng
    expect(findBtn(wrapper, 'Tạm dừng')).toBeDefined();
    await vi.advanceTimersByTimeAsync(1300);
    expect(wrapper.find('[data-testid="viz-step-label"]').text()).toBe('2 / 3');
    await vi.advanceTimersByTimeAsync(1400);
    expect(wrapper.find('[data-testid="viz-step-label"]').text()).toBe('3 / 3');
    // Tick dừng (t=3600ms) → tự dừng, nút trở về "Chạy"
    await vi.advanceTimersByTimeAsync(3000);
    expect(wrapper.find('[data-testid="viz-step-label"]').text()).toBe('3 / 3');
    expect(findBtn(wrapper, 'Tạm dừng')).toBeUndefined();
    await vi.advanceTimersByTimeAsync(3000);
    expect(wrapper.find('[data-testid="viz-step-label"]').text()).toBe('3 / 3');
  });

  it('trace drawer liệt kê các bước và nhảy tới bước được chọn', async () => {
    const wrapper = mountShell();
    expect(wrapper.find('[data-testid="trace-drawer"]').exists()).toBe(false);
    await findBtn(wrapper, 'Xem bảng trace')?.trigger('click');
    expect(wrapper.find('[data-testid="trace-drawer"]').exists()).toBe(true);
    const row = wrapper.findAll('[data-testid="trace-drawer"] button').find((b) =>
      (b.text() as string).includes('So sánh'),
    );
    await row?.trigger('click');
    expect(wrapper.find('[data-testid="viz-step-label"]').text()).toBe('2 / 3');
  });

  it('emit close khi bấm Đóng', async () => {
    const wrapper = mountShell();
    await findBtn(wrapper, 'Đóng')?.trigger('click');
    expect((wrapper.emitted('close') ?? []).length).toBe(1);
  });

  it('frame không phải array dùng fallback renderer — không vỡ shell', () => {
    const steps = makeSteps();
    steps[0] = {
      ...steps[0],
      structure: {
        kind: 'graph',
        elements: [
          { id: 'n:0', label: 'A', status: 'active' },
          { id: 'n:1', label: 'B', status: 'default' },
        ],
        links: [{ from: 'n:0', to: 'n:1', status: 'active' }],
      },
    };
    const frames = legacyStepsToFrames(steps, 'graph.dfs');
    const shell = mount(SharedVisualizerShell, { props: { frames, pseudocode: [] } });
    expect(shell.find('[data-testid="viz-fallback"]').exists()).toBe(true);
    expect(shell.find('[data-testid="array-bars-renderer"]').exists()).toBe(false);
  });
});
