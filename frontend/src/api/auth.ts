import { client, getData } from './client';

/** Endpoint theo API_REFERENCE §4.1 + §4.12 (2FA) + B0 (OTP đăng ký) */
export const AUTH_ENDPOINTS = {
  register: '/auth/register',
  'register/otp': '/auth/register/otp',
  'register/otp/verify': '/auth/register/otp/verify',
  login: '/auth/login',
  refresh: '/auth/refresh',
  logout: '/auth/logout',
  me: '/auth/me',
  changePassword: '/auth/me/password',
  forgotPassword: '/auth/forgot-password',
  resetPassword: '/auth/reset-password',
  '2fa': '/auth/2fa',
  '2fa/send': '/auth/2fa/send',
  '2fa/verify': '/auth/2fa/verify',
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
  /** B1 — /auth/me trả về trạng thái 2FA để ProfileView render toggle */
  twoFactorEnabled: boolean;
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
  /** B0 — token nhận từ POST /auth/register/otp/verify (bắt buộc theo backend) */
  otpToken?: string;
  /** Task L — thông tin giảng viên (gửi khi isTeacher=true); department tùy chọn (A2) */
  department?: string;
  staffCode?: string;
  academicDegree?: string;
  profileLink?: string;
  teacherBio?: string;
}

export interface LoginResponse {
  accessToken: string;
  expiresIn: number;
  user: UserSummary;
  requiresTwoFactor?: boolean;
  twoFactorToken?: string;
  message?: string;
}

export interface Login2FaRequest {
  twoFactorToken: string;
  code: string;
}

export interface ResendLogin2FaRequest {
  twoFactorToken: string;
}

export interface RefreshResponse {
  accessToken: string;
  expiresIn: number;
}

// ── OTP xác thực email khi đăng ký (B0 — API_REFERENCE §4.1) ──

export interface SendRegisterOtpResponse {
  message: string;
  expiresInSeconds: number;
}

export interface VerifyRegisterOtpResponse {
  otpToken: string;
  expiresInSeconds: number;
  message: string;
}

// ── 2FA email (B1 — API_REFERENCE §4.12) ──

export interface Send2FaResponse {
  message: string;
  expiresInSeconds: number;
}

export interface Toggle2FaResponse {
  enabled: boolean;
  message: string;
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

// ── OTP xác thực email khi đăng ký (B0) ──

/** Bước 1/3 — gửi mã OTP 6 chữ số (5 phút, dùng 1 lần) về email chưa đăng ký. */
export async function sendRegisterOtp(email: string): Promise<SendRegisterOtpResponse> {
  return getData<SendRegisterOtpResponse>({
    method: 'POST',
    url: AUTH_ENDPOINTS['register/otp'],
    data: { email },
  });
}

/** Bước 2/3 — xác nhận mã OTP → nhận otpToken (10 phút) dùng cho POST /auth/register. */
export async function verifyRegisterOtp(email: string, code: string): Promise<VerifyRegisterOtpResponse> {
  return getData<VerifyRegisterOtpResponse>({
    method: 'POST',
    url: AUTH_ENDPOINTS['register/otp/verify'],
    data: { email, code },
  });
}

// ── 2FA email (B1 — GP-T2/FR-1.11) ──

/** Gửi mã OTP 2FA về email (cần accessToken) — 5 phút, dùng 1 lần. */
export async function send2FaCode(): Promise<Send2FaResponse> {
  return getData<Send2FaResponse>({ method: 'POST', url: AUTH_ENDPOINTS['2fa/send'] });
}

/** Xác nhận mã OTP 2FA → bật 2FA (enabled=true). */
export async function verify2FaCode(code: string): Promise<Toggle2FaResponse> {
  return getData<Toggle2FaResponse>({
    method: 'POST',
    url: AUTH_ENDPOINTS['2fa/verify'],
    data: { code },
  });
}

/**
 * Bật/tắt 2FA trực tiếp. Tắt (enabled=false) OK; bật qua endpoint này sẽ 400 OTP_REQUIRED
 * — phải đi qua send2FaCode + verify2FaCode.
 */
export async function toggle2Fa(enabled: boolean): Promise<Toggle2FaResponse> {
  return getData<Toggle2FaResponse>({
    method: 'PUT',
    url: AUTH_ENDPOINTS['2fa'],
    data: { enabled },
  });
}

/** Xác thực mã 2FA sau bước đăng nhập mật khẩu */
export async function login2Fa(payload: Login2FaRequest): Promise<LoginResponse> {
  return getData<LoginResponse>({
    method: 'POST',
    url: '/auth/login/2fa',
    data: payload,
  });
}

/** Gửi lại mã OTP 2FA cho phiên đăng nhập */
export async function resendLogin2Fa(payload: ResendLogin2FaRequest): Promise<Send2FaResponse> {
  return getData<Send2FaResponse>({
    method: 'POST',
    url: '/auth/login/2fa/resend',
    data: payload,
  });
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
