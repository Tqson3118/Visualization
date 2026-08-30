import { beforeEach, describe, expect, it, vi } from 'vitest';
import { mount } from '@vue/test-utils';
import { createPinia, setActivePinia } from 'pinia';
import CodeLabTab, { type CodeLabFormState } from '../CodeLabTab.vue';
import QuizTab, { type InlineQuestionItem } from '../QuizTab.vue';

describe('Teacher Studio — Unified Quiz & CodeLab Authoring Spec', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  describe('CodeLabTab Component', () => {
    it('1. Render trạng thái tắt/bật CodeLab chính xác', async () => {
      const state: CodeLabFormState = {
        enabled: false,
        title: 'Bài tập: Test Two Sum',
        description: 'Mô tả bài toán',
        difficulty: 'Easy',
        entryFunction: 'twoSum',
        durationMinutes: 20,
        maxScore: 100,
        starterCode: 'function twoSum() {}',
        solutionCode: 'function twoSum() { return [0, 1]; }',
        testCases: [{ input: '[[2, 7], 9]', expected: '[0, 1]', isHidden: false }],
      };

      const wrapper = mount(CodeLabTab, {
        props: {
          modelValue: state,
          lessonTitle: 'Two Sum Topic',
        },
      });

      expect(wrapper.text()).toContain('Bài học này chưa đính kèm bài tập Code Lab');

      // Click button bật
      const enableBtn = wrapper.find('button');
      await enableBtn.trigger('click');

      const emitted = wrapper.emitted('update:modelValue');
      expect(emitted).toBeTruthy();
      expect(emitted![0][0]).toMatchObject({
        enabled: true,
      });
    });

    it('2. Áp dụng template DSA (Bubble Sort) cập nhật đầy đủ starter code và testcases', async () => {
      const state: CodeLabFormState = {
        enabled: true,
        title: '',
        description: '',
        difficulty: 'Easy',
        entryFunction: 'solve',
        durationMinutes: 20,
        maxScore: 100,
        starterCode: '',
        solutionCode: '',
        testCases: [],
      };

      const wrapper = mount(CodeLabTab, {
        props: {
          modelValue: state,
          lessonTitle: 'Sorting Topic',
        },
      });

      const sortBtn = wrapper.findAll('button').find((b) => b.text().includes('Bubble Sort'));
      expect(sortBtn).toBeDefined();
      await sortBtn!.trigger('click');

      const emitted = wrapper.emitted('update:modelValue');
      expect(emitted).toBeTruthy();
      const lastEmitted = emitted![emitted!.length - 1][0] as CodeLabFormState;
      expect(lastEmitted.entryFunction).toBe('bubbleSort');
      expect(lastEmitted.title).toContain('Bubble Sort');
      expect(lastEmitted.testCases.length).toBeGreaterThanOrEqual(3);
    });
  });

  describe('QuizTab Component', () => {
    it('3. Thêm câu hỏi trắc nghiệm inline cập nhật modelValue reactive', async () => {
      const initialQuestions: InlineQuestionItem[] = [
        {
          content: 'Cấu trúc dữ liệu nào hoạt động theo cơ chế LIFO?',
          options: ['Stack', 'Queue', 'Array', 'Tree'],
          correctIndex: 0,
          explanation: 'Stack hoạt động theo Last In First Out.',
          points: 2,
        },
      ];

      const wrapper = mount(QuizTab, {
        props: {
          lessonId: null,
          lessonTitle: 'Bài giảng Stack',
          modelValue: initialQuestions,
        },
      });

      const textareas = wrapper.findAll('textarea');
      expect(textareas.length).toBeGreaterThan(0);
      expect((textareas[0].element as HTMLTextAreaElement).value).toBe('Cấu trúc dữ liệu nào hoạt động theo cơ chế LIFO?');

      const addBtn = wrapper.findAll('button').find((b) => b.text().includes('Thêm câu hỏi'));
      expect(addBtn).toBeDefined();
      await addBtn!.trigger('click');

      const emitted = wrapper.emitted('update:modelValue');
      expect(emitted).toBeTruthy();
      const lastQuestions = emitted![emitted!.length - 1][0] as InlineQuestionItem[];
      expect(lastQuestions.length).toBe(2);
    });

    it('4. Điền nhanh bộ 3 câu hỏi mẫu DSA thành công', async () => {
      const wrapper = mount(QuizTab, {
        props: {
          lessonId: null,
          lessonTitle: 'DSA Cơ bản',
          modelValue: [],
        },
      });

      const templateBtn = wrapper.findAll('button').find((b) => b.text().includes('+ Thêm 3 câu hỏi mẫu DSA'));
      expect(templateBtn).toBeDefined();
      await templateBtn!.trigger('click');

      const emitted = wrapper.emitted('update:modelValue');
      expect(emitted).toBeTruthy();
      const lastQuestions = emitted![emitted!.length - 1][0] as InlineQuestionItem[];
      expect(lastQuestions.length).toBeGreaterThanOrEqual(3);
    });
  });
});
