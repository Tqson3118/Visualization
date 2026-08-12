import { client, getData } from './client';
import type { PagedResponse } from './types';

/** Endpoint theo API_REFERENCE §4.3 (topics) + §4.4 (lessons) */
export const LESSON_ENDPOINTS = {
  topics: '/topics',
  topic: (id: number) => `/topics/${id}`,
  lessons: '/lessons',
  lesson: (id: number) => `/lessons/${id}`,
  lessonProgress: (id: number) => `/lessons/${id}/progress`,
  markViewed: (id: number) => `/lessons/${id}/mark-viewed`,
  feedback: (id: number) => `/lessons/${id}/feedback`,
  attachSimulation: (id: number) => `/lessons/${id}/simulations`,
  detachSimulation: (id: number, simKey: string) => `/lessons/${id}/simulations/${simKey}`,
} as const;

// ── DTO (API_REFERENCE §3.4) ──

export interface Topic {
  id: number;
  parentId: number | null;
  name: string;
  description: string;
  sortOrder: number;
  children: Topic[];
}

export interface LessonProgressDto {
  viewed: boolean;
  bestScore: number | null;
  completed: boolean;
}

export interface LessonSummary {
  id: number;
  title: string;
  description: string;
  topicId: number;
  sortOrder: number;
  status: 'draft' | 'active' | 'hidden';
  simulationCount: number;
  exerciseCount: number;
  progress: LessonProgressDto | null;
}

export interface LessonDto extends LessonSummary {
  contentHtml: string;
  /** Tham chiếu mô phỏng gắn vào bài (API_REFERENCE §3.4 — SimulationRef[]) */
  simulations?: Array<{ simulationKey: string; title: string; defaultInput?: unknown }>;
  /** Tham chiếu bài tập gắn vào bài (ExerciseRef[]) */
  exercises?: Array<{ id: number; title: string; type: string }>;
}

export interface LessonUpsertRequest {
  topicId: number;
  title: string;
  description?: string;
  contentHtml: string;
  status: 'draft' | 'active' | 'hidden';
  sortOrder?: number;
  simulations?: Array<{ simulationKey: string; title?: string; defaultInput?: unknown }>;
}

// ── CRUD (API_REFERENCE §4.3/§4.4) ──

export async function fetchTopics(): Promise<Topic[]> {
  return getData<Topic[]>({ method: 'GET', url: LESSON_ENDPOINTS.topics });
}

export async function fetchTopic(id: number): Promise<Topic> {
  return getData<Topic>({ method: 'GET', url: LESSON_ENDPOINTS.topic(id) });
}

export async function fetchLessons(params: { topicId?: number; status?: string; q?: string; page?: number } = {}): Promise<PagedResponse<LessonSummary>> {
  return getData<PagedResponse<LessonSummary>>({ method: 'GET', url: LESSON_ENDPOINTS.lessons, params });
}

export async function fetchLesson(id: number): Promise<LessonDto> {
  return getData<LessonDto>({ method: 'GET', url: LESSON_ENDPOINTS.lesson(id), params: { includeContent: true } });
}

export async function fetchLessonProgress(id: number): Promise<LessonProgressDto> {
  return getData<LessonProgressDto>({ method: 'GET', url: LESSON_ENDPOINTS.lessonProgress(id) });
}

export async function markViewed(id: number): Promise<void> {
  await client.post(LESSON_ENDPOINTS.markViewed(id));
}

export interface LessonFeedbackRequest {
  /** 1-5 sao */
  rating: number;
  /** Nhận xét tùy chọn, tối đa 1000 ký tự */
  comment?: string;
}

export interface LessonFeedbackResult {
  lessonId: number;
  rating: number;
}

/** Gửi/chỉnh đánh giá bài học (FR-7.4) — upsert, 1 lần/người; 403 nếu chưa "Đánh dấu đã học" */
export async function submitLessonFeedback(id: number, payload: LessonFeedbackRequest): Promise<LessonFeedbackResult> {
  return getData<LessonFeedbackResult>({ method: 'POST', url: LESSON_ENDPOINTS.feedback(id), data: payload });
}

/** Admin/Teacher: tạo bài học (API_REFERENCE §4.4) */
export async function createLesson(payload: LessonUpsertRequest): Promise<LessonDto> {
  return getData<LessonDto>({ method: 'POST', url: LESSON_ENDPOINTS.lessons, data: payload });
}

export async function updateLesson(id: number, payload: LessonUpsertRequest): Promise<LessonDto> {
  return getData<LessonDto>({ method: 'PUT', url: LESSON_ENDPOINTS.lesson(id), data: payload });
}

export async function deleteLesson(id: number): Promise<void> {
  await client.delete(LESSON_ENDPOINTS.lesson(id));
}

export async function attachSimulation(id: number, payload: { simulationKey: string; title?: string; defaultInput?: unknown }): Promise<void> {
  await client.post(LESSON_ENDPOINTS.attachSimulation(id), payload);
}

export async function detachSimulation(id: number, simKey: string): Promise<void> {
  await client.delete(LESSON_ENDPOINTS.detachSimulation(id, simKey));
}

// ── Topics admin (API_REFERENCE §4.3) ──

export async function createTopic(payload: { parentId?: number | null; name: string; description?: string; sortOrder?: number }): Promise<Topic> {
  return getData<Topic>({ method: 'POST', url: LESSON_ENDPOINTS.topics, data: payload });
}

export async function updateTopic(id: number, payload: { name: string; description?: string; sortOrder?: number }): Promise<Topic> {
  return getData<Topic>({ method: 'PUT', url: LESSON_ENDPOINTS.topic(id), data: payload });
}

export async function deleteTopic(id: number): Promise<void> {
  await client.delete(LESSON_ENDPOINTS.topic(id));
}
