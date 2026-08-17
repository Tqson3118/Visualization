// components/visualizer/EmbeddedVisualizer.spec.ts — nạp generator thật sort.bubble → shell.
import { flushPromises, mount, type DOMWrapper } from '@vue/test-utils';
import { describe, expect, it } from 'vitest';

import '@/engines/catalog';

import EmbeddedVisualizer from './EmbeddedVisualizer.vue';

function findBtn(wrapper: { findAll: (arg: string) => Array<DOMWrapper<Element>> }, label: string) {
  return wrapper.findAll('button').find((b) => b.text().includes(label));
}

describe('EmbeddedVisualizer', () => {
  it('nạp sort.bubble → render shell + bar + mã giả + step label', async () => {
    const wrapper = mount(EmbeddedVisualizer, { props: { simulationKey: 'sort.bubble' } });
    await flushPromises();
    expect(wrapper.find('[data-testid="shared-visualizer-shell"]').exists()).toBe(true);
    expect(wrapper.find('[data-testid="array-bars-renderer"]').exists()).toBe(true);
    const label = wrapper.find('[data-testid="viz-step-label"]').text();
    expect(label).toMatch(/^1 \/ \d+$/);
    expect(wrapper.find('[data-testid="viz-pseudo"]').exists()).toBe(true);
  });

  it('play + step-forward hoạt động trong shell nhúng', async () => {
    const wrapper = mount(EmbeddedVisualizer, { props: { simulationKey: 'sort.bubble' } });
    await flushPromises();
    const before = wrapper.find('[data-testid="viz-step-label"]').text();
    await findBtn(wrapper, 'Bước tới')?.trigger('click');
    const after = wrapper.find('[data-testid="viz-step-label"]').text();
    expect(after).not.toBe(before);
    expect(after).toMatch(/^2 \/ \d+$/);
  });

  it('emit close khi bấm Đóng', async () => {
    const wrapper = mount(EmbeddedVisualizer, { props: { simulationKey: 'sort.bubble' } });
    await flushPromises();
    await findBtn(wrapper, 'Đóng')?.trigger('click');
    expect((wrapper.emitted('close') ?? []).length).toBe(1);
  });

  it('simulationKey không tồn tại → hiện lỗi, không render shell', async () => {
    const wrapper = mount(EmbeddedVisualizer, { props: { simulationKey: 'nope.missing' } });
    await flushPromises();
    expect(wrapper.find('[data-testid="embedded-error"]').exists()).toBe(true);
    expect(wrapper.find('[data-testid="shared-visualizer-shell"]').exists()).toBe(false);
  });
});
