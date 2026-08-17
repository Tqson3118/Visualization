// Temp smoke test — verify redesigned homepage renders + functionality intact.
import { flushPromises, mount } from '@vue/test-utils';
import { nextTick } from 'vue';
import { beforeEach, describe, expect, it, vi } from 'vitest';

const { pushMock } = vi.hoisted(() => ({ pushMock: vi.fn() }));

vi.mock('vue-router', () => ({
  useRouter: () => ({ push: pushMock }),
  RouterLink: { template: '<a><slot /></a>' },
}));

import HomeView from './HomeView.vue';

describe('HomeView — redesigned homepage smoke', () => {
  beforeEach(() => {
    pushMock.mockReset();
    // jsdom thiếu matchMedia → stub (matches: false = không reduced motion)
    window.matchMedia = vi.fn().mockReturnValue({
      matches: false,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    });
  });

  it('mount không lỗi; hero bench chạy demo mặc định sort.bubble (6 block)', async () => {
    const wrapper = mount(HomeView);
    await nextTick();
    expect(wrapper.find('.home__bench').exists()).toBe(true);
    expect(wrapper.find('.home__bench-stage .home__block').exists()).toBe(true);
    expect(wrapper.findAll('.home__bench-stage .home__block').length).toBe(6);
    expect(wrapper.find('.home__bench-tabs').exists()).toBe(true);
    wrapper.unmount();
  });

  it('hero hierarchy: h1 + subtitle muted + 2 CTA', () => {
    const wrapper = mount(HomeView);
    expect(wrapper.find('h1.home__title').exists()).toBe(true);
    expect(wrapper.find('p.home__subtitle').exists()).toBe(true);
    expect(wrapper.findAll('.home__hero-copy .home__cta a').length).toBe(2);
    // CTA band cuối trang cũng có 2 nút
    expect(wrapper.findAll('.home__cta-band .home__cta a').length).toBe(2);
    wrapper.unmount();
  });

  it('stats: BlockToken hero-stat + 3 stat cards (icon Lucide + stat mono 100%)', () => {
    const wrapper = mount(HomeView);
    expect(wrapper.find('.home__stat-hero').exists()).toBe(true);
    const statCards = wrapper.findAll('.home__stat');
    expect(statCards.length).toBe(3);
    expect(wrapper.findAll('.home__stat svg').length).toBe(2);
    expect(wrapper.find('.home__stat-icon--mono').exists()).toBe(true);
    wrapper.unmount();
  });

  it('demo: 3 card, mỗi type có thumbnail khác nhau (bars / row / graph)', () => {
    const wrapper = mount(HomeView);
    const cards = wrapper.findAll('.home__demo');
    expect(cards.length).toBe(3);
    expect(wrapper.find('.home__thumb-bars').exists()).toBe(true);
    expect(wrapper.find('.home__thumb-row').exists()).toBe(true);
    expect(wrapper.find('.home__thumb-graph').exists()).toBe(true);
    // icon demo nằm inline cạnh title (trong .home__demo-body — card tùy chỉnh, không phải shadcn Card)
    expect(wrapper.find('.home__demo-title svg').exists()).toBe(true);
    expect(wrapper.find('.home__demo-run').exists()).toBe(true);
    wrapper.unmount();
  });

  it('features: 2 featured (visual span 8 + path) + 1 compact', () => {
    const wrapper = mount(HomeView);
    expect(wrapper.findAll('.home__feature').length).toBe(3);
    expect(wrapper.find('.home__feature--visual').exists()).toBe(true);
    expect(wrapper.find('.home__feature--path').exists()).toBe(true);
    expect(wrapper.find('.home__feature--compact').exists()).toBe(true);
    expect(wrapper.findAll('.home__feature--featured').length).toBe(2);
    wrapper.unmount();
  });

  it('nút "Chạy thử" vẫn mở simulator qua router.push', async () => {
    const wrapper = mount(HomeView);
    const runButton = wrapper.findAll('.home__demo button')[0];
    await runButton.trigger('click');
    expect(pushMock).toHaveBeenCalledWith({ name: 'simulator', params: { key: 'sort.bubble' } });
    wrapper.unmount();
  });

  it('chuyển demo trong bench vẫn load bước mới (graph.bfs)', async () => {
    const wrapper = mount(HomeView);
    const bfsTab = wrapper.findAll('.home__bench-tab').find((b) => b.text().includes('BFS'));
    expect(bfsTab).toBeTruthy();
    await bfsTab!.trigger('click');
    await flushPromises();
    expect(wrapper.find('.home__bench-key').text()).toBe('graph.bfs');
    expect(wrapper.findAll('.home__bench-stage .home__block').length).toBeGreaterThan(0);
    wrapper.unmount();
  });
});
