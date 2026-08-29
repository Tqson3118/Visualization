import { createPinia, setActivePinia } from 'pinia';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import * as authApi from '@/api/auth';
import type { LoginResponse, UserSummary } from '@/api/auth';
import { useAuthStore } from './auth';

// Mẫu test store theo SDD §3.7 (mock api module)
vi.mock('@/api/auth', () => ({
  login: vi.fn(),
  register: vi.fn(),
  logout: vi.fn(),
  refresh: vi.fn(),
  fetchMe: vi.fn(),
  updateProfile: vi.fn(),
  changePassword: vi.fn(),
  forgotPassword: vi.fn(),
  resetPassword: vi.fn(),
  getData: vi.fn(),
}));

const mockUser: UserSummary = {
  id: 1,
  displayName: 'Nguyễn Minh',
  email: 'minh@university.edu.vn',
  role: 'STUDENT',
  avatarUrl: null,
  createdAt: '2026-08-01T08:00:00Z',
  xp: 100,
  level: 2,
  twoFactorEnabled: false,
};

const mockLoginResponse: LoginResponse = {
  accessToken: 'abc',
  expiresIn: 3600,
  user: mockUser,
};

describe('auth store', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  it('login thành công ghi token và user', async () => {
    vi.mocked(authApi.login).mockResolvedValue(mockLoginResponse);
    const store = useAuthStore();
    await store.login('a@b.c', 'Pass@123');
    expect(store.accessToken).toBe('abc');
    expect(store.user).toEqual(mockUser);
    expect(store.status).toBe('authenticated');
    expect(store.isAuthenticated).toBe(true);
  });

  it('login thất bại giữ nguyên trạng thái', async () => {
    vi.mocked(authApi.login).mockRejectedValue({ status: 401 });
    const store = useAuthStore();
    await expect(store.login('a@b.c', 'sai')).rejects.toBeTruthy();
    expect(store.status).toBe('error');
    expect(store.accessToken).toBeNull();
    expect(store.isAuthenticated).toBe(false);
  });

  it('logout xóa token và trả về trạng thái idle, reset các store cá nhân', async () => {
    vi.mocked(authApi.login).mockResolvedValue(mockLoginResponse);
    vi.mocked(authApi.logout).mockResolvedValue();
    const { useGamificationStore } = await import('./gamification');
    const gamification = useGamificationStore();
    gamification.gems = 150;
    gamification.xp = 500;

    const store = useAuthStore();
    await store.login('a@b.c', 'Pass@123');
    await store.logout();
    expect(store.accessToken).toBeNull();
    expect(store.user).toBeNull();
    expect(store.status).toBe('idle');
    expect(gamification.gems).toBe(0);
    expect(gamification.xp).toBe(0);
  });

  it('refresh là singleton promise — gọi 2 lần chỉ 1 request API', async () => {
    vi.mocked(authApi.refresh).mockResolvedValue({ accessToken: 'new-token', expiresIn: 3600 });
    const store = useAuthStore();
    const [a, b] = await Promise.all([store.refresh(), store.refresh()]);
    expect(a).toBe('new-token');
    expect(b).toBe('new-token');
    expect(authApi.refresh).toHaveBeenCalledTimes(1);
  });

  it('refresh thất bại → token null, status error', async () => {
    vi.mocked(authApi.refresh).mockRejectedValue({ status: 401 });
    const store = useAuthStore();
    const result = await store.refresh();
    expect(result).toBeNull();
    expect(store.accessToken).toBeNull();
    expect(store.status).toBe('error');
  });
});
