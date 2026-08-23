import { client, getData } from './client';
import type { PagedResponse } from './types';

/** Endpoint theo API_REFERENCE §4.6 */
export const EXERCISE_ENDPOINTS = {
  list: '/exercises',
  detail: (id: number) => `/exercises/${id}`,
  submit: (id: number) => `/exercises/${id}/submit`,
  practice: (id: number) => `/exercises/${id}/practice`,
  mySubmissions: (id: number) => `/exercises/${id}/submissions/me`,
  codeSubmit: (id: number) => `/exercises/${id}/code-submit`,
  create: '/exercises',
  update: (id: number) => `/exercises/${id}`,
  remove: (id: number) => `/exercises/${id}`,
} as const;

// ── DTO (API_REFERENCE §3.7-3.8) ──

export interface QuestionDto {
  id: number;
  content: string;
  type: 'SINGLE' | 'MULTIPLE' | 'FILL' | 'MATCH';
  options: string[];
  points: number;
}

export interface ExerciseDto {
  id: number;
  title: string;
  description?: string;
  type: 'MCQ' | 'SIMULATION_LAB' | 'CODE';
  lessonId: number | null;
  nodeId: number | null;
  stage: number;
  durationMinutes: number;
  maxScore: number;
  status: 'draft' | 'active' | 'hidden';
  questions: QuestionDto[];
}

/** Dòng trong GET /exercises (PagedResponse<ExerciseSummaryDto> — không kèm questions; lấy chi tiết qua GET /exercises/{id}) */
export interface ExerciseSummaryDto {
  id: number;
  title: string;
  description?: string;
  type: 'MCQ' | 'SIMULATION_LAB' | 'CODE';
  lessonId: number | null;
  nodeId: number | null;
  stage: number | null;
  durationMinutes: number;
  maxScore: number;
  status: 'draft' | 'active' | 'hidden';
  /** Số user đã pass exercise này (field mới — optional để tương thích khi backend chưa deploy) */
  completedByUserCount?: number;
}

export interface SubmitRequest {
  answers: Array<{ questionId: number; selected: number[] }>;
  classAssignmentId?: number | null;
}

export interface SubmitResultDto {
  score: number;
  maxScore: number;
  results: Array<{
    questionId: number;
    correct: boolean;
    correctAnswer: number[];
    explanation: string;
  }>;
  submissionId: number;
  submittedAt: string;
}

/** Dòng trong GET /exercises/{id}/submissions/me (PagedResponse<SubmissionSummaryDto> — lịch sử nộp bài). */
export interface SubmissionSummaryDto {
  id: number;
  userId: number;
  userDisplayName?: string | null;
  score: number;
  durationSeconds?: number | null;
  classAssignmentId?: number | null;
  submittedAt: string;
}

// ── CRUD (API_REFERENCE §4.6) ──

export async function fetchExercises(params: { lessonId?: number; nodeId?: number; stage?: number } = {}): Promise<ExerciseSummaryDto[]> {
  // BE trả PagedResponse<ExerciseSummaryDto> { items, ... } (API_REFERENCE §3.11) — unwrap items (SETUP_TODO §6.6)
  const paged = await getData<PagedResponse<ExerciseSummaryDto>>({ method: 'GET', url: EXERCISE_ENDPOINTS.list, params });
  return Array.isArray(paged.items) ? paged.items : [];
}

export async function fetchExercise(id: number): Promise<ExerciseDto> {
  return getData<ExerciseDto>({ method: 'GET', url: EXERCISE_ENDPOINTS.detail(id) });
}

export async function submitExercise(id: number, payload: SubmitRequest): Promise<SubmitResultDto> {
  return getData<SubmitResultDto>({ method: 'POST', url: EXERCISE_ENDPOINTS.submit(id), data: payload });
}

export async function practiceExercise(id: number, answers: Array<{ questionId: number; selected: number[] }>): Promise<SubmitResultDto> {
  return getData<SubmitResultDto>({ method: 'POST', url: EXERCISE_ENDPOINTS.practice(id), data: { answers } });
}

/** Lịch sử nộp của tôi — BE trả PagedResponse<SubmissionSummaryDto> (API_REFERENCE §3.11) → unwrap items. */
export async function fetchMySubmissions(id: number, params: { page?: number; pageSize?: number } = {}): Promise<SubmissionSummaryDto[]> {
  const paged = await getData<PagedResponse<SubmissionSummaryDto>>({
    method: 'GET',
    url: EXERCISE_ENDPOINTS.mySubmissions(id),
    params,
  });
  return Array.isArray(paged.items) ? paged.items : [];
}

export interface QuestionUpsertDto {
  content: string;
  type?: 'Single' | 'Multiple' | 'Boolean' | 'Lab';
  options: string[];
  answerJson: string;
  explanation?: string;
  points?: number;
  sortOrder?: number;
}

export interface ExerciseUpsertPayload {
  lessonId: number;
  nodeId?: number | null;
  stage?: number | null;
  title: string;
  description?: string;
  type: 'Mcq' | 'Predict' | 'Code';
  durationMinutes?: number | null;
  maxScore?: number;
  status?: 'Draft' | 'Active' | 'Hidden';
  configJson?: string;
  questions?: QuestionUpsertDto[];
}

export interface ImportCsvResultDto {
  createdCount: number;
  message: string;
}

/** Admin/Teacher: CRUD bài tập (API_REFERENCE §4.6) */
export async function createExercise(payload: ExerciseUpsertPayload): Promise<ExerciseDto> {
  return getData<ExerciseDto>({ method: 'POST', url: EXERCISE_ENDPOINTS.create, data: payload });
}

export async function updateExercise(id: number, payload: Partial<ExerciseUpsertPayload>): Promise<ExerciseDto> {
  return getData<ExerciseDto>({ method: 'PUT', url: EXERCISE_ENDPOINTS.update(id), data: payload });
}

export async function deleteExercise(id: number): Promise<void> {
  await client.delete(EXERCISE_ENDPOINTS.remove(id));
}

/** Admin/Teacher: Nhập danh sách câu hỏi trắc nghiệm từ file CSV */
export async function importExerciseCsv(lessonId: number, file: File): Promise<ImportCsvResultDto> {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('lessonId', String(lessonId));
  return getData<ImportCsvResultDto>({
    method: 'POST',
    url: '/exercises/import-csv',
    data: formData,
    headers: { 'Content-Type': 'multipart/form-data' },
  });
}
