import { client, getData } from './client';

/** Endpoint theo API_REFERENCE §4.1 */
export const AUTH_ENDPOINTS = {
  register: '/auth/register',
  login: '/auth/login',
  refresh: '/auth/refresh',
  logout: '/auth/logout',
  me: '/auth/me',
  changePassword: '/auth/me/password',
  forgotPassword: '/auth/forgot-password',
  resetPassword: '/auth/reset-password',
} as const;

// ── DTO (API_REFERENCE §3.1-3.3) ──

export type UserRole = 'STUDENT' | 'TEACHER' | 'TEACHER_PENDING' | 'ADMIN';

export interface UserSummary {
  id: number;
  displayName: string;
  email: string;
  role: UserRole;
  avatarUrl: string | null;
  createdAt: string;
  xp: number;
  level: number;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  displayName: string;
  email: string;
  password: string;
  isTeacher: boolean;
  /** Task L — thông tin giảng viên (gửi khi isTeacher=true) */
  department?: string;
  staffCode?: string;
  teacherBio?: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresIn: number;
  user: UserSummary;
}

export interface RefreshResponse {
  accessToken: string;
  expiresIn: number;
}

// ── CRUD (API_REFERENCE §4.1 — triển khai thật, lỗi qua ApiError) ──

export async function login(payload: LoginRequest): Promise<LoginResponse> {
  return getData<LoginResponse>({ method: 'POST', url: AUTH_ENDPOINTS.login, data: payload });
}

export async function register(payload: RegisterRequest): Promise<LoginResponse> {
  return getData<LoginResponse>({ method: 'POST', url: AUTH_ENDPOINTS.register, data: payload });
}

export async function logout(): Promise<void> {
  await client.post(AUTH_ENDPOINTS.logout);
}

export async function refresh(): Promise<RefreshResponse> {
  // Cookie HttpOnly tự gửi (ADR-004)
  return getData<RefreshResponse>({ method: 'POST', url: AUTH_ENDPOINTS.refresh });
}

export async function fetchMe(): Promise<UserSummary> {
  return getData<UserSummary>({ method: 'GET', url: AUTH_ENDPOINTS.me });
}

export async function updateProfile(payload: { displayName?: string; avatarUrl?: string | null }): Promise<UserSummary> {
  return getData<UserSummary>({ method: 'PUT', url: AUTH_ENDPOINTS.me, data: payload });
}

export async function changePassword(payload: { currentPassword: string; newPassword: string }): Promise<void> {
  await client.put(AUTH_ENDPOINTS.changePassword, payload);
}

export async function forgotPassword(email: string): Promise<void> {
  await client.post(AUTH_ENDPOINTS.forgotPassword, { email });
}

export async function resetPassword(payload: { token: string; newPassword: string }): Promise<void> {
  await client.post(AUTH_ENDPOINTS.resetPassword, payload);
}

/** Re-export để store/auth dùng khi triển khai */
export { getData };
