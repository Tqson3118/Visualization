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
    vi.mocked(getData).mockResolvedValue({ classId: 7, className: 'Lớp DSA 01', totalMembers: 3, completionPct: 60, avgScore: 7.5, submissions: 9, rows: [] });
    const report = await classesApi.fetchClassReport(7);
    expect(getData).toHaveBeenCalledWith({ method: 'GET', url: '/classes/7/report' });
    expect(report.completionPct).toBe(60);
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
});
