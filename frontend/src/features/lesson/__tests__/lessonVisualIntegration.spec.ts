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

  // UX mới (inline visualizer): LessonStepTheory KHÔNG còn nút "Chạy thử thuật toán" +
  // SharedVisualizerShell — mô phỏng tự động nhúng inline qua InlineSimulationPlayer.
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
            InlineSimulationPlayer: {
              name: 'InlineSimulationPlayer',
              props: ['simKey'],
              template: '<div data-testid="inline-sim-player">{{ simKey }}</div>',
            },
          },
        },
      });

    it('không nhúng mô phỏng khi không có simulationKey', () => {
      const wrapper = mountTheory(null);
      expect(wrapper.find('[data-testid="inline-sim-player"]').exists()).toBe(false);
    });

    it('tự nhúng InlineSimulationPlayer khi có simulationKey (không cần bấm nút)', () => {
      const wrapper = mountTheory('sort.bubble');
      const player = wrapper.find('[data-testid="inline-sim-player"]');
      expect(player.exists()).toBe(true);
      expect(player.text()).toContain('sort.bubble');
    });

    it('mô phỏng inline hiển thị cùng nội dung lý thuyết — content không mất', () => {
      const wrapper = mountTheory('sort.bubble');
      // Player hiển thị ngay không cần tương tác
      expect(wrapper.find('[data-testid="inline-sim-player"]').exists()).toBe(true);
      // Content Lý thuyết (render markdown h1) hiển thị đồng thời
      expect(wrapper.text()).toContain('Lý thuyết');
      expect(wrapper.text()).toContain('Mô phỏng Trực quan');
    });
  });
});
