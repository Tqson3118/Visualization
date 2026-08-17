import { describe, expect, it } from 'vitest';

import { buildReorderItems } from '@/utils/curriculumOrder';
import type { ClassAssignmentDto } from '@/api/types';

const list = (ids: number[]): ClassAssignmentDto[] =>
  ids.map((id) => ({
    id,
    lessonId: null,
    exerciseId: null,
    title: null,
    dueAt: null,
    allowLateSubmission: true,
    sortOrder: 0,
    createdAt: '',
  }));

describe('utils/curriculumOrder', () => {
  it('move item down (1 -> 2) produces the swapped order with sequential sortOrder', () => {
    const items = buildReorderItems(list([10, 20, 30]), 1, 2);
    expect(items.map((i) => i.assignmentId)).toEqual([10, 30, 20]);
    expect(items.map((i) => i.sortOrder)).toEqual([0, 1, 2]);
  });

  it('move item up (2 -> 0) places it first', () => {
    const items = buildReorderItems(list([10, 20, 30]), 2, 0);
    expect(items.map((i) => i.assignmentId)).toEqual([30, 10, 20]);
    expect(items.map((i) => i.sortOrder)).toEqual([0, 1, 2]);
  });

  it('first item move up is a no-op (stays in place)', () => {
    const items = buildReorderItems(list([10, 20, 30]), 0, -1);
    expect(items.map((i) => i.assignmentId)).toEqual([10, 20, 30]);
  });

  it('last item move down is a no-op', () => {
    const items = buildReorderItems(list([10, 20, 30]), 2, 3);
    expect(items.map((i) => i.assignmentId)).toEqual([10, 20, 30]);
  });

  it('fromIndex === toIndex keeps the same order', () => {
    const items = buildReorderItems(list([10, 20, 30]), 1, 1);
    expect(items.map((i) => i.assignmentId)).toEqual([10, 20, 30]);
  });
});
