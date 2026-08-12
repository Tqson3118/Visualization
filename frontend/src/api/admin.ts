import { client, getData } from './client';
import type { PagedResponse, SystemSettingsDto } from './types';

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

export type AdminRole = 'STUDENT' | 'TEACHER' | 'TEACHER_PENDING' | 'ADMIN';

export interface AdminUserDto {
  id: number;
  displayName: string;
  email: string;
  role: AdminRole;
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

// ── CRUD (API_REFERENCE §4.8, §4.10) ──

export async function fetchStats(): Promise<AdminStatsDto> {
  return getData<AdminStatsDto>({ method: 'GET', url: ADMIN_ENDPOINTS.stats });
}

export async function fetchUsers(params: { role?: string; status?: string; q?: string; page?: number } = {}): Promise<PagedResponse<AdminUserDto>> {
  return getData<PagedResponse<AdminUserDto>>({ method: 'GET', url: ADMIN_ENDPOINTS.users, params });
}

export async function setUserStatus(id: number, payload: { isActive: boolean }): Promise<void> {
  await client.put(ADMIN_ENDPOINTS.userStatus(id), payload);
}

export async function setUserRole(id: number, payload: { role: 'STUDENT' | 'TEACHER' }): Promise<void> {
  await client.put(ADMIN_ENDPOINTS.userRole(id), payload);
}

export async function approveTeacher(id: number, payload: { approve: boolean; reason?: string }): Promise<void> {
  await client.post(ADMIN_ENDPOINTS.approveTeacher(id), payload);
}

export async function resetUserPassword(id: number): Promise<void> {
  await client.post(ADMIN_ENDPOINTS.resetPassword(id));
}

export async function deleteUser(id: number): Promise<void> {
  await client.delete(ADMIN_ENDPOINTS.user(id));
}

/** Cấu hình hệ thống (API_REFERENCE §4.10 — GET/PUT /settings) */
export async function fetchSettings(): Promise<SystemSettingsDto> {
  return getData<SystemSettingsDto>({ method: 'GET', url: ADMIN_ENDPOINTS.settings });
}

export async function updateSettings(payload: Partial<SystemSettingsDto>): Promise<SystemSettingsDto> {
  return getData<SystemSettingsDto>({ method: 'PUT', url: ADMIN_ENDPOINTS.settings, data: payload });
}
