/**
 * AI Formatter Service — Hỗ trợ giáo viên định dạng và làm đẹp bài giảng tự động
 * Gọi qua Backend Proxy endpoint /api/v1/ai/format-theory (bảo mật tuyệt đối, không lộ API key).
 * Giới hạn tối đa 5 lượt gọi / tài khoản (lưu trữ theo user / session).
 */
import { client } from '@/api/client';

const MAX_AI_USES = 50;
const STORAGE_KEY_PREFIX = 'vdsa_ai_format_uses_';

export function getAiUsageRemaining(userEmailOrId: string = 'default'): number {
  try {
    const key = `${STORAGE_KEY_PREFIX}${userEmailOrId}`;
    const used = parseInt(localStorage.getItem(key) || '0', 10);
    return Math.max(0, MAX_AI_USES - used);
  } catch {
    return MAX_AI_USES;
  }
}

export function resetAiUsage(userEmailOrId: string = 'default'): void {
  try {
    const key = `${STORAGE_KEY_PREFIX}${userEmailOrId}`;
    localStorage.removeItem(key);
  } catch {
    // Ignore error in restricted storage
  }
}

export function incrementAiUsage(userEmailOrId: string = 'default'): number {
  try {
    const key = `${STORAGE_KEY_PREFIX}${userEmailOrId}`;
    const used = parseInt(localStorage.getItem(key) || '0', 10) + 1;
    localStorage.setItem(key, String(used));
    return Math.max(0, MAX_AI_USES - used);
  } catch {
    return 0;
  }
}

export async function formatLessonWithAi(
  rawContent: string,
  userEmailOrId: string = 'default',
): Promise<string> {
  const remaining = getAiUsageRemaining(userEmailOrId);
  if (remaining <= 0) {
    throw new Error(`Bạn đã sử dụng hết ${MAX_AI_USES} lượt định dạng AI miễn phí.`);
  }

  if (!rawContent || !rawContent.trim()) {
    throw new Error('Nội dung bài học đang trống, vui lòng nhập văn bản trước khi format.');
  }

  const response = await client.post<{ formatted: string }>(
    '/ai/format-theory',
    { rawContent },
    { timeout: 65000 },
  );

  const formatted = response.data?.formatted || '';
  incrementAiUsage(userEmailOrId);
  return formatted;
}
