import { client, getData } from './client';
import type {
  ClassAssignmentDto,
  ClassCurriculumDto,
  ClassCurriculumReorderRequest,
  ClassCurriculumUpsertRequest,
  ClassDetailDto,
  ClassDto,
  ClassMemberDto,
  ClassReportDto,
} from './types';

/** Endpoint theo API_REFERENCE §4.11 (Classes — Module H) */
export const CLASS_ENDPOINTS = {
  list: '/classes',
  detail: (id: number) => `/classes/${id}`,
  update: (id: number) => `/classes/${id}`,
  remove: (id: number) => `/classes/${id}`,
  join: (id: number) => `/classes/${id}/join`,
  joinByCode: '/classes/join-by-code',
  members: (id: number) => `/classes/${id}/members`,
  member: (id: number, userId: number) => `/classes/${id}/members/${userId}`,
  assignments: (id: number) => `/classes/${id}/assignments`,
  assignment: (id: number, assignId: number) => `/classes/${id}/assignments/${assignId}`,
  report: (id: number) => `/classes/${id}/report`,
  reportExport: (id: number) => `/classes/${id}/report/export`,
  // ── Learning Path / Curriculum (per-class) ──
  curriculum: (id: number) => `/classes/${id}/curriculum`,
  curriculumReorder: (id: number) => `/classes/${id}/curriculum/reorder`,
} as const;

// ── CRUD (API_REFERENCE §4.11) ──

export async function fetchClasses(): Promise<ClassDto[]> {
  return getData<ClassDto[]>({ method: 'GET', url: CLASS_ENDPOINTS.list });
}

export async function createClass(payload: { name: string; description?: string }): Promise<ClassDto> {
  return getData<ClassDto>({ method: 'POST', url: CLASS_ENDPOINTS.list, data: payload });
}

export async function fetchClass(id: number): Promise<ClassDetailDto> {
  return getData<ClassDetailDto>({ method: 'GET', url: CLASS_ENDPOINTS.detail(id) });
}

export async function updateClass(id: number, payload: { name?: string; description?: string; ownerId?: number }): Promise<ClassDto> {
  return getData<ClassDto>({ method: 'PUT', url: CLASS_ENDPOINTS.update(id), data: payload });
}

export async function deleteClass(id: number): Promise<void> {
  await client.delete(CLASS_ENDPOINTS.remove(id));
}

export async function joinClass(id: number, inviteCode: string): Promise<ClassDetailDto> {
  return getData<ClassDetailDto>({ method: 'POST', url: CLASS_ENDPOINTS.join(id), data: { inviteCode } });
}

/** v2.15: tham gia lớp bằng mã mời — POST /classes/join-by-code (không cần classId). */
export async function joinByCode(inviteCode: string): Promise<ClassDetailDto> {
  return getData<ClassDetailDto>({ method: 'POST', url: CLASS_ENDPOINTS.joinByCode, data: { inviteCode } });
}

export async function fetchClassMembers(id: number): Promise<ClassMemberDto[]> {
  return getData<ClassMemberDto[]>({ method: 'GET', url: CLASS_ENDPOINTS.members(id) });
}

export async function addClassMember(id: number, email: string): Promise<ClassMemberDto> {
  return getData<ClassMemberDto>({ method: 'POST', url: CLASS_ENDPOINTS.members(id), data: { email } });
}

export async function removeClassMember(id: number, userId: number): Promise<void> {
  await client.delete(CLASS_ENDPOINTS.member(id, userId));
}

export async function fetchClassAssignments(id: number): Promise<ClassAssignmentDto[]> {
  return getData<ClassAssignmentDto[]>({ method: 'GET', url: CLASS_ENDPOINTS.assignments(id) });
}

export async function createClassAssignment(
  id: number,
  payload: { lessonId?: number | null; exerciseId?: number | null; dueAt?: string | null; allowLateSubmission?: boolean },
): Promise<ClassAssignmentDto> {
  return getData<ClassAssignmentDto>({ method: 'POST', url: CLASS_ENDPOINTS.assignments(id), data: payload });
}

export async function updateClassAssignment(
  id: number,
  assignId: number,
  payload: { dueAt?: string | null; allowLateSubmission?: boolean },
): Promise<void> {
  await client.put(CLASS_ENDPOINTS.assignment(id, assignId), payload);
}

export async function deleteClassAssignment(id: number, assignId: number): Promise<void> {
  await client.delete(CLASS_ENDPOINTS.assignment(id, assignId));
}

export async function fetchClassReport(id: number): Promise<ClassReportDto> {
  return getData<ClassReportDto>({ method: 'GET', url: CLASS_ENDPOINTS.report(id) });
}

/** Xuất CSV báo cáo lớp — trả về text CSV (UTF-8 BOM — mở được bằng Excel). */
export async function exportClassReportCsv(id: number): Promise<string> {
  const response = await getData<unknown>({ method: 'GET', url: CLASS_ENDPOINTS.reportExport(id) });
  return typeof response === 'string' ? response : '';
}
// ── Learning Path / Curriculum (per-class) ───────────────────

/** Lộ trình học của lớp (học viên: kèm status từ progress thật; manager: cùng schema). */
export async function fetchClassCurriculum(id: number): Promise<ClassCurriculumDto> {
  return getData<ClassCurriculumDto>({ method: 'GET', url: CLASS_ENDPOINTS.curriculum(id) });
}

/** Teacher/Admin: cập nhật meta lộ trình + publish/unpublish (draft ẩn với học viên). */
export async function updateClassCurriculum(
  id: number,
  payload: ClassCurriculumUpsertRequest,
): Promise<ClassDetailDto> {
  return getData<ClassDetailDto>({ method: 'PUT', url: CLASS_ENDPOINTS.curriculum(id), data: payload });
}

/** Teacher/Admin: sắp xếp lại thứ tự items trong lộ trình. */
export async function reorderClassCurriculum(
  id: number,
  payload: ClassCurriculumReorderRequest,
): Promise<void> {
  await client.put(CLASS_ENDPOINTS.curriculumReorder(id), payload);
}

/** Teacher/Admin: Nhập nhanh toàn bộ bài học từ một Lộ trình có sẵn vào lớp. */
export async function importCourseToClass(id: number, courseId: number): Promise<ClassDetailDto> {
  return getData<ClassDetailDto>({ method: 'POST', url: `/classes/${id}/import-course/${courseId}` });
}
