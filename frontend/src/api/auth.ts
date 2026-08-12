import { getData } from './client';

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

// ── Stub CRUD (body TODO — triển khai ở task kết nối backend) ──

export async function login(_payload: LoginRequest): Promise<LoginResponse> {
  // TODO: client.post(AUTH_ENDPOINTS.login, payload)
  return Promise.reject(new Error('TODO: authApi.login chưa triển khai'));
}

export async function register(_payload: RegisterRequest): Promise<LoginResponse> {
  // TODO: client.post(AUTH_ENDPOINTS.register, payload)
  return Promise.reject(new Error('TODO: authApi.register chưa triển khai'));
}

export async function logout(): Promise<void> {
  // TODO: client.post(AUTH_ENDPOINTS.logout)
  return Promise.reject(new Error('TODO: authApi.logout chưa triển khai'));
}

export async function refresh(): Promise<RefreshResponse> {
  // TODO: client.post(AUTH_ENDPOINTS.refresh) — cookie HttpOnly tự gửi (ADR-004)
  return Promise.reject(new Error('TODO: authApi.refresh chưa triển khai'));
}

export async function fetchMe(): Promise<UserSummary> {
  // TODO: getData(AUTH_ENDPOINTS.me)
  return Promise.reject(new Error('TODO: authApi.fetchMe chưa triển khai'));
}

export async function updateProfile(payload: { displayName?: string; avatarUrl?: string | null }): Promise<UserSummary> {
  // TODO: client.put(AUTH_ENDPOINTS.me, payload)
  return Promise.reject(new Error('TODO: authApi.updateProfile chưa triển khai'));
}

export async function changePassword(payload: { currentPassword: string; newPassword: string }): Promise<void> {
  // TODO: client.put(AUTH_ENDPOINTS.changePassword, payload)
  return Promise.reject(new Error('TODO: authApi.changePassword chưa triển khai'));
}

export async function forgotPassword(email: string): Promise<void> {
  // TODO: client.post(AUTH_ENDPOINTS.forgotPassword, { email })
  return Promise.reject(new Error('TODO: authApi.forgotPassword chưa triển khai'));
}

export async function resetPassword(payload: { token: string; newPassword: string }): Promise<void> {
  // TODO: client.post(AUTH_ENDPOINTS.resetPassword, payload)
  return Promise.reject(new Error('TODO: authApi.resetPassword chưa triển khai'));
}

/** Re-export để store/auth dùng khi triển khai (giữ chỗ helper gọi axios trực tiếp) */
export { getData };
