import { useAuthStore } from '@/stores/auth';

const API_BASE = '/api/v1';

export function getLessonAuthToken(): string | null {
  try {
    const fromStore = useAuthStore().accessToken;
    if (fromStore) return fromStore;
  } catch {
    // Pinia chưa active (test edge)
  }
  return localStorage.getItem('token');
}

export interface LessonProgressPayload {
  lessonId: string;
  hasWatchedVisualizer: boolean;
  quizScore: number | null;
  bestScore: number;
  codelabCompleted: boolean;
  xpAwarded: number;
  /** Đã bấm "Hoàn thành bài học" (bài lý thuyết / node không có quiz) → backend đánh node pass. */
  completed: boolean;
}

export interface LessonDetailResponse {
  id: string;
  courseId: string;
  courseTitle: string;
  title: string;
  contentMd: string;
  sandboxType: string;
  sandboxConfig: string;
  quizId: string | null;
  exerciseId: string | null;
  xpReward: number;
  orderIndex: number;
  status: string;
  lastActiveFrameIndex: number;
  lastScrollPercent: number;
}

export async function fetchLessonDetail(lessonId: string): Promise<LessonDetailResponse> {
  const token = getLessonAuthToken();
  const headers: Record<string, string> = {};
  if (token) headers['Authorization'] = `Bearer ${token}`;

  const res = await fetch(`${API_BASE}/concepts/lessons/${encodeURIComponent(lessonId)}`, { headers });
  if (!res.ok) {
    let message = `Không tải được bài học (HTTP ${res.status})`;
    try {
      const body = await res.json() as { message?: string };
      if (body?.message) message = body.message;
    } catch {
      // body không phải JSON → giữ message mặc định
    }
    throw new Error(message);
  }
  return res.json() as Promise<LessonDetailResponse>;
}

export async function fetchLessonProgress(lessonId: string) {
  const token = getLessonAuthToken();
  if (!token) return null;

  const res = await fetch(`${API_BASE}/concepts/auth/progress/${lessonId}`, {
    headers: { 'Authorization': `Bearer ${token}` },
  });
  if (!res.ok) throw new Error(`Failed to fetch progress: ${res.status}`);
  return res.json();
}

export async function saveLessonProgress(payload: LessonProgressPayload) {
  const token = getLessonAuthToken();
  if (!token) return null;

  const res = await fetch(`${API_BASE}/concepts/auth/progress/${payload.lessonId}`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    body: JSON.stringify({
      hasWatchedVisualizer: payload.hasWatchedVisualizer,
      quizScore: payload.quizScore,
      bestScore: payload.bestScore,
      codelabCompleted: payload.codelabCompleted,
      xpAwarded: payload.xpAwarded,
      completed: payload.completed,
    }),
  });
  if (!res.ok) throw new Error(`Failed to save progress: ${res.status}`);
  return true;
}

export async function awardXp(amount: number, reason = 'Hoàn thành nhiệm vụ bài học') {
  const token = getLessonAuthToken();
  if (!token) return null;

  const res = await fetch(`${API_BASE}/concepts/auth/award-xp`, {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`,
    },
    body: JSON.stringify({ amount, reason }),
  });
  if (!res.ok) throw new Error(`Failed to award XP: ${res.status}`);
  return res.json();
}

export interface CodelabSubmitResult {
  score: number;
  passed: number;
  total: number;
  error: string | null;
  submissionId: number;
  results?: Array<{ testId: string; passed: boolean; error?: string | null }>;
}

/**
 * Nộp bài code cho MÁY CHỦ chấm (Jint sandbox — nghiệp vụ 15/08): bài ASM/kiểm tra cuối
 * chỉ PASS khi code chạy ĐÚNG trên server; điểm/Passed/Total client khai bị bỏ qua.
 * @param taskId id của task con (entryFunction được dùng làm fallback khi id không khớp).
 */
export async function submitCodelab(exerciseId: string, code: string, taskId: string): Promise<CodelabSubmitResult> {
  const token = getLessonAuthToken();
  const headers: Record<string, string> = { 'Content-Type': 'application/json' };
  if (token) headers['Authorization'] = `Bearer ${token}`;

  const res = await fetch(`${API_BASE}/exercises/${encodeURIComponent(exerciseId)}/code-submit`, {
    method: 'POST',
    headers,
    body: JSON.stringify({
      code,
      taskId,
      score: 0,
      passed: 0,
      total: 0,
      clientRequestId: typeof crypto !== 'undefined' && 'randomUUID' in crypto
        ? crypto.randomUUID()
        : `${Date.now()}-${Math.random().toString(36).slice(2)}`,
    }),
  });
  if (!res.ok) {
    let message = `Máy chủ chấm bài thất bại (HTTP ${res.status})`;
    try {
      const body = await res.json() as { message?: string; error?: { message?: string } };
      if (body?.error?.message) message = body.error.message;
      else if (body?.message) message = body.message;
    } catch {
      // giữ message mặc định
    }
    throw new Error(message);
  }
  return res.json() as Promise<CodelabSubmitResult>;
}
