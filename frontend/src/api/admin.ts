import { client, getData } from './client';
import type { PagedResponse, SystemSettingsDto, GamificationSettingsDto } from './types';

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

export interface AdminRoleDistributionDto {
  role: string;
  count: number;
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
  roleDistribution?: AdminRoleDistributionDto[];
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

export async function fetchStats(period?: string): Promise<AdminStatsDto> {
  return getData<AdminStatsDto>({ method: 'GET', url: ADMIN_ENDPOINTS.stats, params: period ? { period } : undefined });
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

export interface AdminCreateUserPayload {
  displayName: string;
  email: string;
  password: string;
  role: AdminRole;
  department?: string;
  staffCode?: string;
}

export interface AdminUpdateUserPayload {
  displayName: string;
  role?: AdminRole;
  isActive?: boolean;
  department?: string;
  staffCode?: string;
  academicDegree?: string;
  profileLink?: string;
  teacherBio?: string;
}

export async function createUser(payload: AdminCreateUserPayload): Promise<AdminUserDto> {
  return getData<AdminUserDto>({ method: 'POST', url: ADMIN_ENDPOINTS.users, data: payload });
}

export async function updateUser(id: number, payload: AdminUpdateUserPayload): Promise<AdminUserDto> {
  return getData<AdminUserDto>({ method: 'PUT', url: ADMIN_ENDPOINTS.user(id), data: payload });
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

export async function setUserRole(id: number, payload: { role: 'STUDENT' | 'TEACHER' | 'ADMIN' }): Promise<void> {
  await client.put(ADMIN_ENDPOINTS.userRole(id), payload);
}

export async function approveTeacher(id: number, payload: { approve: boolean; reason?: string }): Promise<void> {
  await client.post(ADMIN_ENDPOINTS.approveTeacher(id), payload);
}

export async function resetUserPassword(id: number, newPassword?: string): Promise<void> {
  await client.post(ADMIN_ENDPOINTS.resetPassword(id), newPassword ? { newPassword } : {});
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

// ── ADMIN SHOP & GAMIFICATION (V2) ──

export interface AdminShopItemDto {
  id: number;
  itemKey: string;
  name: string;
  priceGems: number;
  type: number; // 0=Consumable, 1=Avatar, 2=Frame, 3=Theme
  maxStack: number;
  durationHours: number | null;
  ownersCount: number;
  imageUrl?: string | null;
}

export interface AdminGemTransactionDto {
  id: number;
  userId: number;
  userEmail: string;
  userDisplayName: string;
  type: number;
  amount: number;
  refType: string | null;
  refId: string | null;
  createdAt: string;
}

export async function fetchAdminShopItems(): Promise<AdminShopItemDto[]> {
  return getData<AdminShopItemDto[]>({ method: 'GET', url: '/admin/shop/items' });
}

export async function uploadShopAsset(payload: { image: string; name?: string }): Promise<{ url: string }> {
  return getData<{ url: string }>({ method: 'POST', url: '/admin/shop/upload-asset', data: payload });
}

export async function saveCustomShopAsset(itemKey: string, imageUrl: string): Promise<void> {
  await client.post('/admin/shop/custom-assets', { itemKey, imageUrl });
}

export async function fetchAdminCustomShopAssets(): Promise<Record<string, string>> {
  return getData<Record<string, string>>({ method: 'GET', url: '/admin/shop/custom-assets' });
}

export async function createAdminShopItem(payload: {
  itemKey: string;
  name: string;
  priceGems: number;
  type: number;
  maxStack?: number;
  durationHours?: number | null;
}): Promise<AdminShopItemDto> {
  return getData<AdminShopItemDto>({ method: 'POST', url: '/admin/shop/items', data: payload });
}

export async function updateAdminShopItem(
  id: number,
  payload: {
    name: string;
    priceGems: number;
    type: number;
    maxStack?: number;
    durationHours?: number | null;
  },
): Promise<AdminShopItemDto> {
  return getData<AdminShopItemDto>({ method: 'PUT', url: `/admin/shop/items/${id}`, data: payload });
}

export async function deleteAdminShopItem(id: number): Promise<{ message: string }> {
  return getData<{ message: string }>({ method: 'DELETE', url: `/admin/shop/items/${id}` });
}

export async function fetchAdminGemTransactions(limit = 50): Promise<AdminGemTransactionDto[]> {
  return getData<AdminGemTransactionDto[]>({ method: 'GET', url: '/admin/shop/gem-transactions', params: { limit } });
}

// ── ADMIN SUBSCRIPTIONS & TRANSACTIONS (V2) ──

export interface AdminSubscriptionDto {
  id: number;
  userId: number;
  userEmail: string;
  userDisplayName: string;
  avatarUrl: string | null;
  planId: string | null;
  startedAt: string;
  expiresAt: string | null;
  status: number;
  isActive: boolean;
  orderRef: string | null;
  createdAt: string;
}

export async function fetchAdminSubscriptions(status?: 'active' | 'expired' | 'pending' | string): Promise<AdminSubscriptionDto[]> {
  return getData<AdminSubscriptionDto[]>({ method: 'GET', url: '/admin/subscriptions', params: status ? { status } : {} });
}

export async function grantAdminPro(payload: {
  email?: string;
  userId?: number;
  planId?: string;
  durationDays: number;
}): Promise<AdminSubscriptionDto> {
  return getData<AdminSubscriptionDto>({ method: 'POST', url: '/admin/subscriptions/grant', data: payload });
}

export async function revokeAdminPro(id: number): Promise<AdminSubscriptionDto> {
  return getData<AdminSubscriptionDto>({ method: 'POST', url: `/admin/subscriptions/${id}/revoke` });
}

// ── ADMIN GAMIFICATION SETTINGS (0 DB MIGRATION) ──

export async function fetchGamificationSettings(): Promise<GamificationSettingsDto> {
  return getData<GamificationSettingsDto>({ method: 'GET', url: '/admin/gamification/settings' });
}

export async function updateGamificationSettings(data: GamificationSettingsDto): Promise<GamificationSettingsDto> {
  return getData<GamificationSettingsDto>({ method: 'PUT', url: '/admin/gamification/settings', data });
}

export async function resetGamificationSettings(): Promise<GamificationSettingsDto> {
  return getData<GamificationSettingsDto>({ method: 'POST', url: '/admin/gamification/settings/reset' });
}


