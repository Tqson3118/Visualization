// src/components/visualizer/__tests__/DataStructureStage.spec.ts
import { describe, expect, it, vi, beforeEach } from 'vitest';
import { mount } from '@vue/test-utils';
import DataStructureStage from '../DataStructureStage.vue';
import type { Structure } from '@/engines/core/types';

describe('DataStructureStage.vue', () => {
  beforeEach(() => {
    window.matchMedia = vi.fn().mockReturnValue({
      matches: false,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    });
  });

  const mockStructure: Structure = {
    kind: 'array',
    elements: [
      { id: 'cell:0', label: '10', status: 'default' },
      { id: 'cell:1', label: '20', status: 'active' },
    ],
    links: [],
  };

  it('renders macOS terminal 3 dots and monospace title', () => {
    const wrapper = mount(DataStructureStage, {
      props: {
        structure: mockStructure,
        simKey: 'sort.bubble',
        complexity: 'O(N²)',
      },
    });

    // Verify 3 macOS dots
    expect(wrapper.find('.data-stage__dot--red').exists()).toBe(true);
    expect(wrapper.find('.data-stage__dot--yellow').exists()).toBe(true);
    expect(wrapper.find('.data-stage__dot--green').exists()).toBe(true);

    // Verify filename & complexity
    expect(wrapper.find('.data-stage__file-badge').text()).toContain('sort_bubble.ts • ARRAY_STAGE');
    expect(wrapper.find('.data-stage__complexity-chip').text()).toBe('O(N²)');
  });

  it('toggles index and values with modern pill switches', async () => {
    const wrapper = mount(DataStructureStage, {
      props: {
        structure: mockStructure,
        showIndex: true,
        showValues: true,
      },
    });

    const pills = wrapper.findAll('.data-stage__pill');
    expect(pills.length).toBe(2);

    // Click Show Index pill
    await pills[0].trigger('click');
    expect(wrapper.emitted('update:show-index')).toBeTruthy();
    expect(wrapper.emitted('update:show-index')![0]).toEqual([false]);

    // Click Show Values pill
    await pills[1].trigger('click');
    expect(wrapper.emitted('update:show-values')).toBeTruthy();
    expect(wrapper.emitted('update:show-values')![0]).toEqual([false]);
  });

  it('handles zoom selection change', async () => {
    const wrapper = mount(DataStructureStage, {
      props: {
        structure: mockStructure,
        zoom: 1,
      },
    });

    const select = wrapper.find('.data-stage__zoom-select');
    await select.setValue('1.5');

    expect(wrapper.emitted('update:zoom')).toBeTruthy();
    expect(wrapper.emitted('update:zoom')![0]).toEqual([1.5]);
  });

  it('mounts with structure and unmounts without throwing (B1/B2 teardown)', async () => {
    const wrapper = mount(DataStructureStage, {
      props: {
        structure: mockStructure,
      },
    });
    // initPixi chạy bất đồng bộ trên onMounted; yield một chút rồi unmount
    await new Promise((resolve) => setTimeout(resolve, 20));
    expect(() => wrapper.unmount()).not.toThrow();
  });

  it('mounts twice (mount → unmount → mount) without throwing', async () => {
    const w1 = mount(DataStructureStage, {
      props: {
        structure: mockStructure,
      },
    });
    await new Promise((resolve) => setTimeout(resolve, 20));
    expect(() => w1.unmount()).not.toThrow();

    const w2 = mount(DataStructureStage, {
      props: {
        structure: mockStructure,
      },
    });
    await new Promise((resolve) => setTimeout(resolve, 20));
    expect(() => w2.unmount()).not.toThrow();
  });
});
