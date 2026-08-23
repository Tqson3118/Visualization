// views/__tests__/CodeToVisualView.spec.ts — F3 view orchestration tests
// Mount view với test Pinia; stub ControlBar/CanvasArea/PseudocodePanel (tránh Pixi/canvas).
import { mount } from '@vue/test-utils';
import { createPinia, setActivePinia } from 'pinia';
import { beforeEach, describe, expect, it } from 'vitest';

import CodeToVisualView from '../CodeToVisualView.vue';
import { useSimulationStore } from '@/stores/simulation';

const StubControlBar = {
  name: 'ControlBar',
  props: ['currentIndex', 'totalFrames', 'status', 'speed'],
  template: '<div data-testid="control-bar" />',
};
const StubCanvasArea = {
  name: 'CanvasArea',
  props: ['structure', 'simKey'],
  template: '<div data-testid="canvas-area" />',
};
const StubPseudocode = {
  name: 'PseudocodePanel',
  props: ['pseudocode', 'activeLine', 'variables', 'collapsed'],
  template: '<div data-testid="pseudocode" />',
};

function mountView() {
  const pinia = createPinia();
  setActivePinia(pinia);
  const wrapper = mount(CodeToVisualView, {
    global: {
      plugins: [pinia],
      stubs: {
        RouterLink: { template: '<a><slot /></a>' },
        ControlBar: StubControlBar,
        CanvasArea: StubCanvasArea,
        PseudocodePanel: StubPseudocode,
        Button: { props: ['disabled', 'variant', 'size'], template: '<button :disabled="disabled"><slot /></button>' },
      },
    },
  });
  return { wrapper, store: useSimulationStore() };
}

describe('CodeToVisualView', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
  });

  it('chưa chạy -> empty-state + console empty hiển thị', () => {
    const { wrapper } = mountView();
    expect(wrapper.find('[data-testid="empty-state"]').exists()).toBe(true);
    expect(wrapper.text()).toContain('Nhấn "Chạy" để phân tích mã DSL từng bước.');
  });

  it('input array không hợp lệ -> thông báo lỗi + RUN disabled', async () => {
    const { wrapper } = mountView();
    await wrapper.find('[data-testid="array-input"]').setValue('5');
    expect(wrapper.text()).toContain('Mảng không hợp lệ');
    const runBtn = wrapper.findAll('button').find((b) => b.text().includes('Chạy'));
    expect(runBtn?.attributes('disabled')).toBeDefined();
  });

  it('chạy DSL hợp lệ -> sinh steps, empty-state biến mất, console có log success', async () => {
    const { wrapper, store } = mountView();
    await wrapper.find('[data-testid="dsl-editor"]').setValue('array.push(5)\narray.swap(0, 1)');
    const runBtn = wrapper.findAll('button').find((b) => b.text().includes('Chạy'));
    await runBtn?.trigger('click');
    await wrapper.vm.$nextTick();
    await new Promise((resolve) => setTimeout(resolve, 0));

    expect(store.steps.length).toBeGreaterThan(0);
    expect(wrapper.find('[data-testid="empty-state"]').exists()).toBe(false);
    expect(wrapper.find('[data-testid="control-bar"]').exists()).toBe(true);
    expect(wrapper.find('[data-testid="canvas-area"]').exists()).toBe(true);
    expect(wrapper.text()).toContain('Hoàn tất');
  });

  it('DSL lỗi cú pháp -> data-testid dsl-error hiển thị đúng line + console error', async () => {
    const { wrapper, store } = mountView();
    await wrapper.find('[data-testid="dsl-editor"]').setValue('array.push(1)\nfoo.bar()');
    const runBtn = wrapper.findAll('button').find((b) => b.text().includes('Chạy'));
    await runBtn?.trigger('click');
    await wrapper.vm.$nextTick();
    await new Promise((resolve) => setTimeout(resolve, 0));

    const err = wrapper.find('[data-testid="dsl-error"]');
    expect(err.exists()).toBe(true);
    expect(err.text()).toContain('2');
    expect(store.steps.length).toBe(0);
  });

  it('nút Xóa -> clear editor + steps + console', async () => {
    const { wrapper, store } = mountView();
    await wrapper.find('[data-testid="dsl-editor"]').setValue('array.push(1)');
    const runBtn = wrapper.findAll('button').find((b) => b.text().includes('Chạy'));
    await runBtn?.trigger('click');
    await new Promise((resolve) => setTimeout(resolve, 0));
    expect(store.steps.length).toBeGreaterThan(0);

    const clearBtn = wrapper.findAll('button').find((b) => b.text().includes('Xóa'));
    await clearBtn?.trigger('click');
    await wrapper.vm.$nextTick();
    expect(store.steps.length).toBe(0);
    expect(wrapper.find('[data-testid="empty-state"]').exists()).toBe(true);
  });
});
