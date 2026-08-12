import { computed, ref } from 'vue';
import { defineStore } from 'pinia';

import * as authApi from '@/api/auth';
import type { LoginRequest, RegisterRequest, UserSummary } from '@/api/auth';

export type AuthStatus = 'idle' | 'loading' | 'authenticated' | 'error';

/**
 * Store auth theo SDD §3.2.
 * Token chỉ trong memory Pinia (ADR-004) — F5 → refresh qua cookie HttpOnly khôi phục phiên.
 * refresh() là singleton promise (SDD §3.4) — nhiều request 401 cùng lúc chỉ gọi 1 lần.
 */
export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserSummary | null>(null);
  const accessToken = ref<string | null>(null);
  const status = ref<AuthStatus>('idle');

  let refreshPromise: Promise<string | null> | null = null;

  const isAuthenticated = computed(
    () => status.value === 'authenticated' && accessToken.value !== null,
  );
  const role = computed(() => user.value?.role ?? null);

  async function login(email: string, password: string): Promise<void> {
    const payload: LoginRequest = { email, password };
    status.value = 'loading';
    try {
      const response = await authApi.login(payload);
      accessToken.value = response.accessToken;
      user.value = response.user;
      status.value = 'authenticated';
    } catch (error) {
      status.value = 'error';
      throw error;
    }
  }

  async function register(payload: RegisterRequest): Promise<void> {
    status.value = 'loading';
    try {
      const response = await authApi.register(payload);
      accessToken.value = response.accessToken;
      user.value = response.user;
      status.value = 'authenticated';
    } catch (error) {
      status.value = 'error';
      throw error;
    }
  }

  async function logout(): Promise<void> {
    try {
      await authApi.logout();
    } catch {
      // logout luôn reset trạng thái cục bộ dù API lỗi
    } finally {
      accessToken.value = null;
      user.value = null;
      status.value = 'idle';
    }
  }

  /** Singleton promise: chỉ 1 lần gọi /auth/refresh cho mọi request 401 đồng thời */
  async function refresh(): Promise<string | null> {
    if (refreshPromise) return refreshPromise;
    refreshPromise = authApi
      .refresh()
      .then((response) => {
        accessToken.value = response.accessToken;
        status.value = 'authenticated';
        return response.accessToken;
      })
      .catch(() => {
        accessToken.value = null;
        user.value = null;
        status.value = 'error';
        return null;
      })
      .finally(() => {
        refreshPromise = null;
      });
    return refreshPromise;
  }

  async function fetchMe(): Promise<void> {
    const me = await authApi.fetchMe();
    user.value = me;
    status.value = 'authenticated';
  }

  return {
    user,
    accessToken,
    status,
    isAuthenticated,
    role,
    login,
    register,
    logout,
    refresh,
    fetchMe,
  };
});
