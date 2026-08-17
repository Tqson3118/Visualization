// views/__tests__/lessonViewVisualizer.spec.ts — LessonView × EmbeddedVisualizer (Workstream B/B3).
// - "Học tiếp" / "Chạy thử thuật toán" chỉ xuất hiện khi bài có simulationKey.
// - Visual tương tác mở INLINE trong tab Lý thuyết, KHÔNG có tab Visualizer riêng.
// - Đóng visualizer về lại lý thuyết (không mất trạng thái lesson).
import { flushPromises, mount, RouterLinkStub, type DOMWrapper } from '@vue/test-utils';
import { createPinia, setActivePinia } from 'pinia';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { createMemoryHistory, createRouter } from 'vue-router';

import '@/engines/catalog';
import LessonView from '@/views/LessonView.vue';
import type { LessonDto } from '@/api/lessons';

const baseLesson: LessonDto = {
  id: 7,
  title: 'Bubble Sort',
  description: 'Sắp xếp nổi bọt',
  topicId: 1,
  sortOrder: 1,
  status: 'active',
  simulationCount: 1,
  exerciseCount: 0,
  progress: { viewed: false, bestScore: null, completed: false },
  contentHtml: '<p>Lý thuyết Bubble Sort.</p>',
  simulations: [{ simulationKey: 'sort.bubble', title: 'Bubble Sort' }],
  exercises: [],
};

let mockLesson: LessonDto = { ...baseLesson };

vi.mock('@/api/lessons', () => ({
  fetchLesson: vi.fn(async () => mockLesson),
  fetchTopics: vi.fn(async () => []),
  fetchLessons: vi.fn(async () => ({ items: [], total: 0, page: 1, pageSize: 10 })),
  fetchLessonProgress: vi.fn(async () => ({ viewed: false, bestScore: null, completed: false })),
  markViewed: vi.fn(async () => undefined),
}));

async function mountLesson() {
  const pinia = createPinia();
  setActivePinia(pinia);
  const router = createRouter({
    history: createMemoryHistory(),
    routes: [{ path: '/lessons/:lessonId', name: 'lesson', component: LessonView }],
  });
  await router.push('/lessons/7');
  await router.isReady();
  const wrapper = mount(LessonView, {
    global: {
      plugins: [pinia, router],
      stubs: { RouterLink: RouterLinkStub },
    },
  });
  await flushPromises();
  return wrapper;
}

function findBtn(wrapper: { findAll: (arg: string) => Array<DOMWrapper<Element>> }, label: string) {
  return wrapper.findAll('button').find((b) => b.text().includes(label));
}

describe('LessonView × EmbeddedVisualizer', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    mockLesson = { ...baseLesson };
  });

  it('bài có simulationKey → bấm "Học tiếp" mở visualizer inline, không rời lesson', async () => {
    const wrapper = await mountLesson();
    expect(wrapper.find('[data-testid="embedded-visualizer"]').exists()).toBe(false);
    await findBtn(wrapper, 'Học tiếp')?.trigger('click');
    await flushPromises();
    expect(wrapper.find('[data-testid="embedded-visualizer"]').exists()).toBe(true);
    expect(wrapper.find('[data-testid="array-bars-renderer"]').exists()).toBe(true);
    expect((wrapper.find('[data-testid="viz-step-label"]').text() as string)).toMatch(/^1 \/ \d+$/);
  });

  it('nút "Chạy thử thuật toán" hiện trong lý thuyết; đóng → quay lại lý thuyết (giữ lesson)', async () => {
    const wrapper = await mountLesson();
    await findBtn(wrapper, 'Học tiếp')?.trigger('click');
    await flushPromises();
    await findBtn(wrapper, 'Đóng')?.trigger('click');
    await wrapper.vm.$nextTick();
    // Đóng visualizer → nút "Chạy thử thuật toán" hiện lại (lý thuyết vẫn active)
    expect(wrapper.find('[data-testid="embedded-visualizer"]').exists()).toBe(false);
    expect(wrapper.find('[data-testid="run-algo-btn"]').exists()).toBe(true);
    // Bấm lại → mở lại
    await findBtn(wrapper, 'Chạy thử thuật toán')?.trigger('click');
    await flushPromises();
    expect(wrapper.find('[data-testid="embedded-visualizer"]').exists()).toBe(true);
  });

  it('bài KHÔNG có simulationKey → không có khu visual tương tác', async () => {
    const noSim: LessonDto = { ...baseLesson, simulations: [], simulationCount: 0 };
    mockLesson = noSim;
    const wrapper = await mountLesson();
    await findBtn(wrapper, 'Học tiếp')?.trigger('click');
    await flushPromises();
    expect(wrapper.find('[data-testid="embedded-visualizer"]').exists()).toBe(false);
    expect(wrapper.find('[data-testid="run-algo-btn"]').exists()).toBe(false);
  });
});
