// @vitest-environment jsdom
import { describe, it, expect, beforeEach } from 'vitest';
import { createPinia, setActivePinia } from 'pinia';
import { mount } from '@vue/test-utils';

import { useLessonStore } from '../store/useLessonStore';
import LessonStepTheory from '../../../views/lesson/components/LessonStepTheory.vue';

describe('Lesson flow — simulationKey + "Chạy thử thuật toán" (B3)', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
  });

  describe('useLessonStore.simulationKey', () => {
    it('đọc simulationKey từ sandboxConfig json', () => {
      const store = useLessonStore();
      store.lessonMeta = {
        courseId: '1',
        courseTitle: 'T',
        quizId: null,
        exerciseId: null,
        sandboxType: 'dsa',
        sandboxConfig: '{"simulationKey":"sort.bubble"}',
        orderIndex: 1,
      };
      expect(store.simulationKey).toBe('sort.bubble');
    });

    it('fallback sang demo cũ khi không có simulationKey', () => {
      const store = useLessonStore();
      store.lessonMeta = {
        courseId: '1',
        courseTitle: 'T',
        quizId: null,
        exerciseId: null,
        sandboxType: 'dsa',
        sandboxConfig: '{"demo":"sort.bubble"}',
        orderIndex: 1,
      };
      expect(store.simulationKey).toBe('sort.bubble');
    });

    it('null khi sandboxConfig rỗng/hỏng', () => {
      const store = useLessonStore();
      store.lessonMeta = {
        courseId: '1',
        courseTitle: 'T',
        quizId: null,
        exerciseId: null,
        sandboxType: 'dsa',
        sandboxConfig: '',
        orderIndex: 1,
      };
      expect(store.simulationKey).toBeNull();
    });
  });

  describe('LessonStepTheory', () => {
    const mountTheory = (simulationKey?: string | null) =>
      mount(LessonStepTheory, {
        props: {
          title: 'Bubble Sort',
          content: '# Lý thuyết',
          simulationKey,
        },
        global: {
          plugins: [createPinia()],
          stubs: {
            BaseIcon: { template: '<span />' },
            SharedVisualizerShell: {
              name: 'SharedVisualizerShell',
              props: ['frames', 'algorithmKey'],
              template: '<div data-testid="shell-stub"><button data-testid="shell-close">Đóng</button></div>',
            },
          },
        },
      });

    it('không hiện nút khi không có simulationKey', () => {
      const wrapper = mountTheory(null);
      expect(wrapper.find('[data-testid="run-simulation-btn"]').exists()).toBe(false);
    });

    it('hiện nút "Chạy thử thuật toán" khi có simulationKey', () => {
      const wrapper = mountTheory('sort.bubble');
      const btn = wrapper.find('[data-testid="run-simulation-btn"]');
      expect(btn.exists()).toBe(true);
      expect(btn.text()).toContain('Chạy thử thuật toán');
    });

    it('bấm nút → mở embedded visualizer; đóng → về lý thuyết (không mất content)', async () => {
      const wrapper = mountTheory('sort.bubble');
      expect(wrapper.find('[data-testid="embedded-visualizer"]').exists()).toBe(false);

      await wrapper.find('[data-testid="run-simulation-btn"]').trigger('click');
      expect(wrapper.find('[data-testid="embedded-visualizer"]').exists()).toBe(true);

      // Mô phỏng đóng shell (bấm nút đóng) — quay về Lý thuyết, content giữ nguyên
      await wrapper.find('[data-testid="shell-close"]').trigger('click');
      await wrapper.vm.$nextTick();
      expect(wrapper.find('[data-testid="embedded-visualizer"]').exists()).toBe(false);
      // Content Lý thuyết (render markdown h1) vẫn còn sau khi đóng shell
      expect(wrapper.text()).toContain('Lý thuyết');
      expect(wrapper.find('[data-testid="run-simulation-btn"]').exists()).toBe(true);
    });
  });
});
