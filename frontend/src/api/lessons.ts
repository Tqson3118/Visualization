import type { PagedResponse } from './types';

/** Endpoint theo API_REFERENCE §4.3 (topics) + §4.4 (lessons) */
export const LESSON_ENDPOINTS = {
  topics: '/topics',
  topic: (id: number) => `/topics/${id}`,
  lessons: '/lessons',
  lesson: (id: number) => `/lessons/${id}`,
  lessonProgress: (id: number) => `/lessons/${id}/progress`,
  markViewed: (id: number) => `/lessons/${id}/mark-viewed`,
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
}

// ── Stub CRUD (body TODO) ──

export async function fetchTopics(): Promise<Topic[]> {
  // TODO: getData({ method: 'GET', url: LESSON_ENDPOINTS.topics })
  return Promise.reject(new Error('TODO: lessonsApi.fetchTopics chưa triển khai'));
}

export async function fetchTopic(id: number): Promise<Topic> {
  // TODO: getData({ method: 'GET', url: LESSON_ENDPOINTS.topic(id) })
  return Promise.reject(new Error('TODO: lessonsApi.fetchTopic chưa triển khai'));
}

export async function fetchLessons(params: { topicId?: number; status?: string; q?: string; page?: number } = {}): Promise<PagedResponse<LessonSummary>> {
  // TODO: getData({ method: 'GET', url: LESSON_ENDPOINTS.lessons, params })
  return Promise.reject(new Error('TODO: lessonsApi.fetchLessons chưa triển khai'));
}

export async function fetchLesson(id: number): Promise<LessonDto> {
  // TODO: getData({ method: 'GET', url: LESSON_ENDPOINTS.lesson(id) })
  return Promise.reject(new Error('TODO: lessonsApi.fetchLesson chưa triển khai'));
}

export async function fetchLessonProgress(id: number): Promise<LessonProgressDto> {
  // TODO: getData({ method: 'GET', url: LESSON_ENDPOINTS.lessonProgress(id) })
  return Promise.reject(new Error('TODO: lessonsApi.fetchLessonProgress chưa triển khai'));
}

export async function markViewed(id: number): Promise<void> {
  // TODO: client.post(LESSON_ENDPOINTS.markViewed(id))
  return Promise.reject(new Error('TODO: lessonsApi.markViewed chưa triển khai'));
}
