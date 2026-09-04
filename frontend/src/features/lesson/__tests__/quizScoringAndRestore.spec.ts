// @vitest-environment jsdom
import { describe, it, expect } from 'vitest';
import { applySubmissionResultsToQuestions } from '../store/useLessonStore';
import type { QuizQuestion } from '../types/lesson.types';

describe('Quiz scoring and restore verification', () => {
  it('correctly applies PascalCase resultJson from .NET backend', () => {
    const questions: QuizQuestion[] = [
      {
        id: '510',
        questionText: 'Thuật toán BFS dùng cấu trúc dữ liệu nào?',
        type: 'SINGLE',
        options: ['Queue', 'Stack', 'Tree', 'Hash'],
        correctIndex: undefined,
        correctIndices: [],
        explanation: '',
      },
      {
        id: '511',
        questionText: 'BFS dùng để tìm bài toán nào?',
        type: 'SINGLE',
        options: ['Đường đi ngắn nhất', 'Cây khung', 'Topo', 'Euler'],
        correctIndex: undefined,
        correctIndices: [],
        explanation: '',
      },
    ];

    const resultJson = JSON.stringify([
      {
        QuestionId: '510',
        IsCorrect: true,
        CorrectIndex: 0,
        CorrectIndices: [0],
        Explanation: 'BFS dùng Queue FIFO.',
      },
      {
        QuestionId: '511',
        IsCorrect: true,
        CorrectIndex: 0,
        CorrectIndices: [0],
        Explanation: 'BFS tìm đường đi ngắn nhất không trọng số.',
      },
    ]);

    applySubmissionResultsToQuestions(questions, resultJson);

    expect(questions[0].correctIndex).toBe(0);
    expect(questions[0].correctIndices).toEqual([0]);
    expect(questions[0].explanation).toBe('BFS dùng Queue FIFO.');

    expect(questions[1].correctIndex).toBe(0);
    expect(questions[1].correctIndices).toEqual([0]);
    expect(questions[1].explanation).toBe('BFS tìm đường đi ngắn nhất không trọng số.');
  });

  it('correctly applies camelCase resultJson', () => {
    const questions: QuizQuestion[] = [
      {
        id: '101',
        questionText: 'Câu hỏi test',
        type: 'MULTIPLE',
        options: ['A', 'B', 'C', 'D'],
        correctIndex: undefined,
        correctIndices: [],
        explanation: '',
      },
    ];

    const resultJson = [
      {
        questionId: '101',
        isCorrect: true,
        correctIndex: 1,
        correctIndices: [1, 2],
        explanation: 'Chọn B và C.',
      },
    ];

    applySubmissionResultsToQuestions(questions, resultJson);

    expect(questions[0].correctIndex).toBe(1);
    expect(questions[0].correctIndices).toEqual([1, 2]);
    expect(questions[0].explanation).toBe('Chọn B và C.');
  });

  it('correctly applies legacy seed resultJson with CorrectAnswer and fallback by index', () => {
    const questions: QuizQuestion[] = [
      {
        id: '463',
        questionText: 'Hàm băm là gì?',
        type: 'SINGLE',
        options: ['Ánh xạ khóa thành chỉ số', 'Sắp xếp mảng', 'Tìm kiếm nhị phân', 'Đảo ngược'],
        correctIndex: undefined,
        correctIndices: [],
        explanation: '',
      },
    ];

    const resultJson = [
      {
        QuestionId: 463,
        Correct: true,
        CorrectAnswer: [0],
        Explanation: 'Hàm băm ánh xạ khóa tùy ý thành chỉ số mảng.',
      },
    ];

    applySubmissionResultsToQuestions(questions, resultJson);

    expect(questions[0].correctIndex).toBe(0);
    expect(questions[0].correctIndices).toEqual([0]);
    expect(questions[0].explanation).toBe('Hàm băm ánh xạ khóa tùy ý thành chỉ số mảng.');
  });
});
