import axios, { AxiosError, type AxiosRequestConfig, type InternalAxiosRequestConfig } from 'axios';

import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';

/**
 * Axios client theo SDD §3.4:
 * - baseURL = VITE_API_BASE_URL (mặc định /api/v1)
 * - Request: gắn Authorization: Bearer <accessToken>
 * - Response: 401 → refresh 1 lần (cờ _retry + singleton) → thất bại → logout → /login?redirect=
 *             400/422 → parse error.message + field (ApiError)
 *             429 → toast + thông báo Retry-After
 *             5xx → toast "Đã có lỗi xảy ra, vui lòng thử lại"
 */

/** Lỗi chuẩn theo API_REFERENCE §2.1: { error: { code, message, field, details } } */
export interface ApiErrorBody {
  code: string;
  message: string;
  field?: string | null;
  details?: unknown[];
}

export class ApiError extends Error {
  readonly code: string;
  readonly field: string | null;
  readonly status: number;
  readonly retryAfterSeconds?: number;

  constructor(status: number, body: Partial<ApiErrorBody>, retryAfterSeconds?: number) {
    super(body.message ?? messages.toast.serverError);
    this.name = 'ApiError';
    this.status = status;
    this.code = body.code ?? 'UNKNOWN';
    this.field = body.field ?? null;
    this.retryAfterSeconds = retryAfterSeconds;
  }
}

const BASE_URL: string = import.meta.env.VITE_API_BASE_URL ?? '/api/v1';

export const client = axios.create({
  baseURL: BASE_URL,
  timeout: 15000,
  headers: { 'Content-Type': 'application/json' },
});

/** Đánh dấu request đã thử refresh 1 lần (SDD §3.4) */
declare module 'axios' {
  export interface InternalAxiosRequestConfig {
    _retry?: boolean;
  }
}

client.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const auth = useAuthStore();
  if (auth.accessToken) {
    config.headers.Authorization = `Bearer ${auth.accessToken}`;
  }
  return config;
});

/** Ném ApiError chuẩn từ lỗi axios (hoặc lỗi mạng) */
function toApiError(error: unknown): ApiError {
  if (error instanceof ApiError) return error;

  if (axios.isAxiosError(error)) {
    const err = error as AxiosError<{ error?: Partial<ApiErrorBody> }>;
    const status = err.response?.status ?? 0;
    const body = err.response?.data?.error ?? {};
    if (status === 429) {
      const retryAfter = Number(err.response?.headers?.['retry-after'] ?? NaN);
      return new ApiError(status, body, Number.isFinite(retryAfter) ? retryAfter : undefined);
    }
    return new ApiError(status, body);
  }

  return new ApiError(0, { code: 'NETWORK_ERROR', message: messages.toast.networkError });
}

/**
 * Response interceptor theo SDD §3.4:
 * (1) 401 → refresh 1 lần → retry; thất bại → logout + redirect /login?redirect=...
 * (2) 400/422 → ném ApiError có message + field
 * (3) 429 → toast "vui lòng thử lại sau N giây" (Retry-After)
 * (4) 5xx → toast lỗi chung
 */
client.interceptors.response.use(
  (response) => response,
  async (error: unknown) => {
    if (!axios.isAxiosError(error)) {
      return Promise.reject(toApiError(error));
    }

    const original = error.config as (InternalAxiosRequestConfig & { _retry?: boolean }) | undefined;
    const status = error.response?.status ?? 0;
    const auth = useAuthStore();
    const ui = useUiStore();

    // (1) 401 — refresh 1 lần
    if (status === 401 && original && !original._retry && !original.url?.includes('/auth/')) {
      original._retry = true;
      const newToken = await auth.refresh();
      if (newToken) {
        original.headers.Authorization = `Bearer ${newToken}`;
        return client(original as AxiosRequestConfig);
      }
      // Refresh thất bại → logout + redirect về /login
      await auth.logout();
      const redirect = encodeURIComponent(window.location.pathname + window.location.search);
      window.location.assign(`/login?redirect=${redirect}`);
      return Promise.reject(toApiError(error));
    }

    const apiError = toApiError(error);

    // (3) 429 — toast + Retry-After
    if (status === 429) {
      const seconds = apiError.retryAfterSeconds;
      ui.showToast(
        seconds !== undefined
          ? messages.toast.rateLimited(seconds)
          : messages.toast.rateLimitedUnknown,
        'warning',
      );
    } else if (status >= 500) {
      // (4) 5xx — toast chung
      ui.showToast(messages.toast.serverError, 'error');
    }
    // (2) 400/422 — không toast, giao cho caller xử lý theo ApiError.field

    return Promise.reject(apiError);
  },
);

/** Helper: unwrap response.data với kiểu mong đợi */
export async function getData<T>(config: AxiosRequestConfig): Promise<T> {
  const response = await client.request<T>(config);
  return response.data;
}
