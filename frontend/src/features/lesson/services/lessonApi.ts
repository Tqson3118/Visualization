import { client } from '@/api/client';
import { useAuthStore } from '@/stores/auth';

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

export async function fetchLessonDetail(lessonId: string, courseId?: string | number | null): Promise<LessonDetailResponse> {
  const q = courseId ? `?courseId=${encodeURIComponent(String(courseId))}` : '';
  const res = await client.get<LessonDetailResponse>(`/concepts/lessons/${encodeURIComponent(lessonId)}${q}`);
  return res.data;
}

export async function fetchLessonProgress(lessonId: string) {
  const res = await client.get(`/concepts/auth/progress/${encodeURIComponent(lessonId)}`);
  return res.data;
}

export async function saveLessonProgress(payload: LessonProgressPayload) {
  await client.post(`/concepts/auth/progress/${encodeURIComponent(payload.lessonId)}`, {
    hasWatchedVisualizer: payload.hasWatchedVisualizer,
    quizScore: payload.quizScore,
    bestScore: payload.bestScore,
    codelabCompleted: payload.codelabCompleted,
    xpAwarded: payload.xpAwarded,
    completed: payload.completed,
  });
  return true;
}

export async function awardXp(amount: number, reason = 'Hoàn thành nhiệm vụ bài học') {
  const res = await client.post('/concepts/auth/award-xp', { amount, reason });
  return res.data;
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
  const res = await client.post<CodelabSubmitResult>(`/exercises/${encodeURIComponent(exerciseId)}/code-submit`, {
    code,
    taskId,
    score: 0,
    passed: 0,
    total: 0,
    clientRequestId: typeof crypto !== 'undefined' && 'randomUUID' in crypto
      ? crypto.randomUUID()
      : `${Date.now()}-${Math.random().toString(36).slice(2)}`,
  });
  return res.data;
}
