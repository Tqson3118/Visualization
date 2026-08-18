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
  bugReports: '/admin/bug-reports',
  bugReport: (id: number) => `/admin/bug-reports/${id}`,
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
  avatarUrl?: string | null;
  /** Task L — thông tin đăng ký giảng viên (chỉ có với TEACHER_PENDING) */
  department?: string;
  staffCode?: string;
  teacherBio?: string;
  /** Block 2.3 — hồ sơ giảng viên mở rộng (học vị + link hồ sơ) */
  academicDegree?: string;
  profileLink?: string;
  /** Block 2.3 — thống kê học tập (đầy đủ với GET /users/{id}) */
  xp?: number;
  level?: number;
  streakDays?: number;
  gems?: number;
  hearts?: number;
  lessonsCompletedCount?: number;
  exercisesPassedCount?: number;
  joinedClassesCount?: number;
}

export interface AdminStatsDto {
  totalUsers: number;
  totalLessons: number;
  totalExercises: number;
  totalSimulations: number;
  activeUsersToday: number;
  /** §1c — thống kê giao dịch thật từ Order (GET /admin/stats mở rộng). */
  totalOrders?: number;
  totalRevenue?: number;
  pendingOrders?: number;
  completedOrders?: number;
  cancelledOrders?: number;
  revenueByDay?: Array<{ date: string; revenue: number; orders: number }>;
  recentOrders?: Array<{
    id: string;
    userDisplayName: string;
    email: string;
    amount: number;
    status: string;
    paymentCode: string;
    createdAt: string;
    completedAt: string | null;
  }>;
}

/** Báo cáo lỗi / vi phạm — GET /admin/bug-reports (v2.15: adminNote) */
export interface BugReportDto {
  id: number;
  userId: number | null;
  description: string;
  context: string | null;
  status: 'NEW' | 'PROCESSING' | 'RESOLVED' | 'CLOSED';
  adminNote: string | null;
  createdAt: string;
  resolvedAt: string | null;
}

// ── CRUD (API_REFERENCE §4.8, §4.10) ──

export async function fetchStats(): Promise<AdminStatsDto> {
  return getData<AdminStatsDto>({ method: 'GET', url: ADMIN_ENDPOINTS.stats });
}

/** Danh sách báo cáo lỗi/vi phạm (v2.15) — GET /admin/bug-reports */
export async function fetchBugReports(): Promise<BugReportDto[]> {
  return getData<BugReportDto[]>({ method: 'GET', url: ADMIN_ENDPOINTS.bugReports });
}

/** Cập nhật trạng thái + phản hồi AdminNote (v2.15) — PUT /admin/bug-reports/{id} */
export async function updateBugReport(
  id: number,
  payload: { status: BugReportDto['status']; adminNote?: string },
): Promise<BugReportDto> {
  return getData<BugReportDto>({ method: 'PUT', url: ADMIN_ENDPOINTS.bugReport(id), data: payload });
}

export async function fetchUsers(params: { role?: string; status?: string; q?: string; page?: number } = {}): Promise<PagedResponse<AdminUserDto>> {
  return getData<PagedResponse<AdminUserDto>>({ method: 'GET', url: ADMIN_ENDPOINTS.users, params });
}

/** Block 2.3 — chi tiết user đầy đủ (stats học tập + hồ sơ GV) cho drawer admin */
export async function fetchUser(id: number): Promise<AdminUserDto> {
  return getData<AdminUserDto>({ method: 'GET', url: ADMIN_ENDPOINTS.user(id) });
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
