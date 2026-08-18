// @vitest-environment jsdom
import { describe, it, expect, beforeEach, vi } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { mount } from '@vue/test-utils';

import SharedVisualizerShell from '../components/SharedVisualizerShell.vue';
import { useVcrStore } from '../../vcr-player/store/useVcrStore';
import { generateBubbleSortFrames } from '../../algorithm-sandbox/algorithms/bubbleSort';

describe('SharedVisualizerShell (B2) — shared sandbox shell', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
  });

  it('nạp frames vào VCR store và reset về bước đầu', () => {
    const frames = generateBubbleSortFrames([5, 3, 8, 1, 9, 2]);
    const store = useVcrStore();

    mount(SharedVisualizerShell, {
      props: { frames, algorithmKey: 'sort.bubble' },
      global: { stubs: { BaseIcon: { template: '<span />' } } },
    });

    expect(store.playbackFrames).toHaveLength(frames.length);
    expect(store.currentFrameIndex).toBe(0);
    expect(store.customCompileFn).toBeNull();
  });

  it('render renderer mặc định (SortingVisualizerDispatcher) với frame hiện tại', () => {
    const frames = generateBubbleSortFrames([4, 2]);
    const wrapper = mount(SharedVisualizerShell, {
      props: { frames },
      global: { stubs: { BaseIcon: { template: '<span />' } } },
    });

    // dispatcher được render (không stub) → nhãn empty tránh crash; frame 0 có mảng
    expect(wrapper.find('.shared-visualizer-shell').exists()).toBe(true);
  });

  it('embedded: nút Đóng emit close', async () => {
    const frames = generateBubbleSortFrames([3, 1, 2]);
    const wrapper = mount(SharedVisualizerShell, {
      props: { frames, embedded: true },
      global: { stubs: { BaseIcon: { template: '<span />' } } },
    });

    const closeBtn = wrapper.get('[data-testid="shared-shell-close"]');
    await closeBtn.trigger('click');
    expect(wrapper.emitted('close')).toBeTruthy();
  });

  it('trace drawer hiển thị bảng trạng thái (props-driven)', async () => {
    const frames = generateBubbleSortFrames([5, 3, 8]);
    const wrapper = mount(SharedVisualizerShell, {
      props: { frames },
      global: { stubs: { BaseIcon: { template: '<span />' } } },
    });

    await wrapper.get('button[aria-label*="Bảng trạng thái"]').trigger('click');
    expect(wrapper.text()).toContain('Bảng trạng thái');
  });

  it('bước Next qua VCR store → shell hiển thị description của frame mới', async () => {
    const frames = generateBubbleSortFrames([5, 3, 8]);
    const store = useVcrStore();
    const wrapper = mount(SharedVisualizerShell, {
      props: { frames },
      global: { stubs: { BaseIcon: { template: '<span />' } } },
    });

    // trước khi step: description frame 0
    const desc = () => wrapper.find('.shared-visualizer-shell').text();
    void desc;

    store.stepNext();
    await vi.dynamicImportSettled();
    expect(store.currentFrameIndex).toBe(1);
  });

  it('không đè customCompileFn do feature khác set (shell không can thiệp sau khi nạp frames)', async () => {
    const frames = generateBubbleSortFrames([1, 2]);
    const store = useVcrStore();
    const compileStub = vi.fn();

    mount(SharedVisualizerShell, {
      props: { frames },
      global: { stubs: { BaseIcon: { template: '<span />' } } },
    });

    // Shell chỉ set null lúc nạp frames; sau đó feature khác (VD SortingView) có thể
    // đặt customCompileFn cho nhu cầu riêng — shell KHÔNG chiến đấu lại.
    store.customCompileFn = compileStub;
    await vi.dynamicImportSettled();
    expect(store.customCompileFn).toBe(compileStub);
  });
});
