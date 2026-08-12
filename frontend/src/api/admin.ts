import type { PagedResponse } from './types';

/** Endpoint theo API_REFERENCE §4.10 (admin) + §4.8 (users) */
export const ADMIN_ENDPOINTS = {
  stats: '/admin/stats',
  settings: '/settings',
  users: '/users',
  user: (id: number) => `/users/${id}`,
  userStatus: (id: number) => `/users/${id}/status`,
  userRole: (id: number) => `/users/${id}/role`,
  approveTeacher: (id: number) => `/users/${id}/approve-teacher`,
  resetPassword: (id: number) => `/users/${id}/reset-password`,
} as const;

// ── DTO (API_REFERENCE §4.8, §4.10) ──

export interface AdminUserDto {
  id: number;
  displayName: string;
  email: string;
  role: 'STUDENT' | 'TEACHER' | 'TEACHER_PENDING' | 'ADMIN';
  isActive: boolean;
  createdAt: string;
}

export interface AdminStatsDto {
  totalUsers: number;
  totalLessons: number;
  totalExercises: number;
  totalSimulations: number;
  activeUsersToday: number;
}

// ── Stub CRUD (body TODO) ──

export async function fetchStats(): Promise<AdminStatsDto> {
  // TODO: getData({ method: 'GET', url: ADMIN_ENDPOINTS.stats })
  return Promise.reject(new Error('TODO: adminApi.fetchStats chưa triển khai'));
}

export async function fetchUsers(params: { role?: string; status?: string; q?: string; page?: number } = {}): Promise<PagedResponse<AdminUserDto>> {
  // TODO: getData({ method: 'GET', url: ADMIN_ENDPOINTS.users, params })
  return Promise.reject(new Error('TODO: adminApi.fetchUsers chưa triển khai'));
}

export async function setUserStatus(id: number, payload: { isActive: boolean }): Promise<void> {
  // TODO: client.put(ADMIN_ENDPOINTS.userStatus(id), payload)
  return Promise.reject(new Error('TODO: adminApi.setUserStatus chưa triển khai'));
}

export async function setUserRole(id: number, payload: { role: 'STUDENT' | 'TEACHER' }): Promise<void> {
  // TODO: client.put(ADMIN_ENDPOINTS.userRole(id), payload)
  return Promise.reject(new Error('TODO: adminApi.setUserRole chưa triển khai'));
}

export async function approveTeacher(id: number, payload: { approve: boolean; reason?: string }): Promise<void> {
  // TODO: client.post(ADMIN_ENDPOINTS.approveTeacher(id), payload)
  return Promise.reject(new Error('TODO: adminApi.approveTeacher chưa triển khai'));
}
