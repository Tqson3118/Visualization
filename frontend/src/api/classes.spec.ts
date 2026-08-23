import { beforeEach, describe, expect, it, vi } from 'vitest';

import * as classesApi from '@/api/classes';
import type { ClassDto } from '@/api/types';

// Smoke test api/classes (mock client — SDD §3.4): kiểm tra đúng endpoint + payload.
vi.mock('@/api/client', () => ({
  client: {
    get: vi.fn(),
    post: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
  getData: vi.fn(),
}));

import { client, getData } from '@/api/client';

const mockClass: ClassDto = {
  id: 7,
  name: 'Lớp DSA 01',
  description: null,
  inviteCode: 'ABC123',
  ownerId: 1,
  memberCount: 3,
  createdAt: '2026-08-01T08:00:00Z',
  role: 'OWNER',
};

describe('api/classes', () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it('fetchClasses gọi GET /classes', async () => {
    vi.mocked(getData).mockResolvedValue([mockClass]);
    const result = await classesApi.fetchClasses();
    expect(getData).toHaveBeenCalledWith({ method: 'GET', url: '/classes' });
    expect(result).toEqual([mockClass]);
  });

  it('createClass gọi POST /classes với payload tên lớp', async () => {
    vi.mocked(getData).mockResolvedValue(mockClass);
    const result = await classesApi.createClass({ name: 'Lớp DSA 01', description: 'Nhóm 2' });
    expect(getData).toHaveBeenCalledWith({
      method: 'POST',
      url: '/classes',
      data: { name: 'Lớp DSA 01', description: 'Nhóm 2' },
    });
    expect(result.inviteCode).toBe('ABC123');
  });

  it('joinClass gọi POST /classes/{id}/join với mã 6 ký tự', async () => {
    vi.mocked(getData).mockResolvedValue(mockClass);
    await classesApi.joinClass(7, 'ABC123');
    expect(getData).toHaveBeenCalledWith({
      method: 'POST',
      url: '/classes/7/join',
      data: { inviteCode: 'ABC123' },
    });
  });

  it('fetchClassReport gọi GET /classes/{id}/report', async () => {
    vi.mocked(getData).mockResolvedValue({
      classId: 7,
      className: 'Lớp DSA 01',
      totalMembers: 3,
      assignments: [
        { assignmentId: 1, title: 'Bài học #1', dueAt: '2026-09-01T00:00:00Z', onTime: 2, late: 0, notSubmitted: 1, avgScore: 8.5 },
      ],
      laggingLearners: [{ userId: 9, displayName: 'Sinh viên A', missingCount: 2 }],
    });
    const report = await classesApi.fetchClassReport(7);
    expect(getData).toHaveBeenCalledWith({ method: 'GET', url: '/classes/7/report' });
    expect(report.totalMembers).toBe(3);
    expect(report.assignments[0].onTime).toBe(2);
    expect(report.laggingLearners[0].missingCount).toBe(2);
  });

  it('deleteClass gọi DELETE /classes/{id} qua client', async () => {
    vi.mocked(client.delete).mockResolvedValue({});
    await classesApi.deleteClass(7);
    expect(client.delete).toHaveBeenCalledWith('/classes/7');
  });

  it('createClassAssignment gọi POST /classes/{id}/assignments với dueAt', async () => {
    vi.mocked(getData).mockResolvedValue({ id: 1, lessonId: null, exerciseId: 5, dueAt: '2026-09-01T00:00:00Z', status: 'open' });
    await classesApi.createClassAssignment(7, { exerciseId: 5, dueAt: '2026-09-01T00:00:00Z' });
    expect(getData).toHaveBeenCalledWith({
      method: 'POST',
      url: '/classes/7/assignments',
      data: { exerciseId: 5, dueAt: '2026-09-01T00:00:00Z' },
    });
  });

  it('joinByCode gọi POST /classes/join-by-code với mã mời', async () => {
    vi.mocked(getData).mockResolvedValue({ ...mockClass, members: [], assignments: [] });
    const joined = await classesApi.joinByCode('ABC123');
    expect(getData).toHaveBeenCalledWith({
      method: 'POST',
      url: '/classes/join-by-code',
      data: { inviteCode: 'ABC123' },
    });
    expect(joined.id).toBe(7);
  });

  it('updateClassAssignment gọi PUT /classes/{id}/assignments/{assignId} với dueAt + allowLateSubmission', async () => {
    vi.mocked(client.put).mockResolvedValue({});
    await classesApi.updateClassAssignment(7, 3, { dueAt: '2026-10-01T00:00:00Z', allowLateSubmission: false });
    expect(client.put).toHaveBeenCalledWith('/classes/7/assignments/3', {
      dueAt: '2026-10-01T00:00:00Z',
      allowLateSubmission: false,
    });
  });

  it('deleteClassAssignment gọi DELETE /classes/{id}/assignments/{assignId} qua client', async () => {
    vi.mocked(client.delete).mockResolvedValue({});
    await classesApi.deleteClassAssignment(7, 3);
    expect(client.delete).toHaveBeenCalledWith('/classes/7/assignments/3');
  });

  // ── Learning Path / Curriculum (per-class) — feature port ──

  it('fetchClassCurriculum gọi GET /classes/{id}/curriculum', async () => {
    const curriculum = {
      classId: 7,
      title: 'Graph cơ bản',
      description: null,
      published: true,
      progressPct: 40,
      items: [
        { assignmentId: 1, lessonId: 1, exerciseId: null, title: 'Array', itemType: 'lesson', sortOrder: 0, dueAt: null, status: 'completed', bestScore: null },
        { assignmentId: 2, lessonId: 2, exerciseId: null, title: 'Stack', itemType: 'lesson', sortOrder: 1, dueAt: null, status: 'in_progress', bestScore: null },
      ],
    };
    vi.mocked(getData).mockResolvedValue(curriculum);
    const result = await classesApi.fetchClassCurriculum(7);
    expect(getData).toHaveBeenCalledWith({ method: 'GET', url: '/classes/7/curriculum' });
    expect(result.items[1].status).toBe('in_progress');
  });

  it('updateClassCurriculum gọi PUT /classes/{id}/curriculum (publish → học viên thấy)', async () => {
    vi.mocked(getData).mockResolvedValue({ ...mockClass, curriculumPublished: true });
    await classesApi.updateClassCurriculum(7, { title: 'Graph cơ bản', published: true });
    expect(getData).toHaveBeenCalledWith({
      method: 'PUT',
      url: '/classes/7/curriculum',
      data: { title: 'Graph cơ bản', published: true },
    });
  });

  it('updateClassCurriculum gọi PUT /classes/{id}/curriculum (save draft → published=false)', async () => {
    vi.mocked(getData).mockResolvedValue({ ...mockClass, curriculumPublished: false });
    await classesApi.updateClassCurriculum(7, { published: false });
    expect(getData).toHaveBeenCalledWith({
      method: 'PUT',
      url: '/classes/7/curriculum',
      data: { published: false },
    });
  });

  it('reorderClassCurriculum gọi PUT /classes/{id}/curriculum/reorder với danh sách thứ tự', async () => {
    vi.mocked(client.put).mockResolvedValue({});
    await classesApi.reorderClassCurriculum(7, {
      items: [
        { assignmentId: 3, sortOrder: 0 },
        { assignmentId: 1, sortOrder: 1 },
        { assignmentId: 2, sortOrder: 2 },
      ],
    });
    expect(client.put).toHaveBeenCalledWith('/classes/7/curriculum/reorder', {
      items: [
        { assignmentId: 3, sortOrder: 0 },
        { assignmentId: 1, sortOrder: 1 },
        { assignmentId: 2, sortOrder: 2 },
      ],
    });
  });
});
