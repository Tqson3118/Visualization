import { client, getData } from './client';
import type { ClassAssignmentDto, ClassDto, ClassMemberDto, ClassReportDto, PagedResponse } from './types';

/** Endpoint theo API_REFERENCE §4.11 (Classes — Module H) */
export const CLASS_ENDPOINTS = {
  list: '/classes',
  detail: (id: number) => `/classes/${id}`,
  update: (id: number) => `/classes/${id}`,
  remove: (id: number) => `/classes/${id}`,
  join: (id: number) => `/classes/${id}/join`,
  members: (id: number) => `/classes/${id}/members`,
  member: (id: number, userId: number) => `/classes/${id}/members/${userId}`,
  assignments: (id: number) => `/classes/${id}/assignments`,
  assignment: (id: number, assignId: number) => `/classes/${id}/assignments/${assignId}`,
  report: (id: number) => `/classes/${id}/report`,
  reportExport: (id: number) => `/classes/${id}/report/export`,
} as const;

// ── CRUD (API_REFERENCE §4.11) ──

export async function fetchClasses(): Promise<ClassDto[]> {
  return getData<ClassDto[]>({ method: 'GET', url: CLASS_ENDPOINTS.list });
}

export async function createClass(payload: { name: string; description?: string }): Promise<ClassDto> {
  return getData<ClassDto>({ method: 'POST', url: CLASS_ENDPOINTS.list, data: payload });
}

export async function fetchClass(id: number): Promise<ClassDto & { members?: ClassMemberDto[]; assignments?: ClassAssignmentDto[] }> {
  return getData<ClassDto & { members?: ClassMemberDto[]; assignments?: ClassAssignmentDto[] }>({
    method: 'GET',
    url: CLASS_ENDPOINTS.detail(id),
  });
}

export async function updateClass(id: number, payload: { name?: string; description?: string; ownerId?: number }): Promise<ClassDto> {
  return getData<ClassDto>({ method: 'PUT', url: CLASS_ENDPOINTS.update(id), data: payload });
}

export async function deleteClass(id: number): Promise<void> {
  await client.delete(CLASS_ENDPOINTS.remove(id));
}

export async function joinClass(id: number, inviteCode: string): Promise<ClassDto> {
  return getData<ClassDto>({ method: 'POST', url: CLASS_ENDPOINTS.join(id), data: { inviteCode } });
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
  payload: { lessonId?: number | null; exerciseId?: number | null; dueAt?: string | null },
): Promise<ClassAssignmentDto> {
  return getData<ClassAssignmentDto>({ method: 'POST', url: CLASS_ENDPOINTS.assignments(id), data: payload });
}

export async function updateClassAssignment(
  id: number,
  assignId: number,
  payload: { dueAt?: string | null; status?: 'open' | 'closed' },
): Promise<ClassAssignmentDto> {
  return getData<ClassAssignmentDto>({ method: 'PUT', url: CLASS_ENDPOINTS.assignment(id, assignId), data: payload });
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
