import { getData } from './client';

/** Endpoint theo API_REFERENCE §4.7 */
export const PROGRESS_ENDPOINTS = {
  overview: '/progress/me',
  lesson: (lessonId: number) => `/progress/me/lessons/${lessonId}`,
  report: '/progress/report',
  reportExport: '/progress/report/export',
} as const;

// ── DTO (API_REFERENCE §3.9) ──

export interface ProgressOverviewDto {
  lessonsViewed: number;
  lessonsTotal: number;
  exercisesCompleted: number;
  exercisesTotal: number;
  avgScore: number | null;
  topics: Array<{
    id: number;
    name: string;
    progressPct: number;
    lessons: Array<{
      id: number;
      title: string;
      viewed: boolean;
      bestScore: number | null;
      completed: boolean;
    }>;
  }>;
}

export interface TeacherReportDto {
  lessonId: number;
  lessonTitle?: string;
  totalLearners?: number;
  learnersViewed?: number;
  completionPct?: number;
  avgScore?: number | null;
  rows: Array<{
    studentId: number;
    displayName: string;
    email: string;
    viewed: boolean;
    attempts: number;
    bestScore: number | null;
    avgScore: number | null;
  }>;
}

// ── CRUD (API_REFERENCE §4.7) ──

export async function fetchOverview(): Promise<ProgressOverviewDto> {
  return getData<ProgressOverviewDto>({ method: 'GET', url: PROGRESS_ENDPOINTS.overview });
}

export async function fetchLessonProgress(lessonId: number): Promise<{ viewed: boolean; bestScore: number | null; completed: boolean }> {
  return getData<{ viewed: boolean; bestScore: number | null; completed: boolean }>({
    method: 'GET',
    url: PROGRESS_ENDPOINTS.lesson(lessonId),
  });
}

export async function fetchReport(params: { lessonId: number }): Promise<TeacherReportDto> {
  return getData<TeacherReportDto>({ method: 'GET', url: PROGRESS_ENDPOINTS.report, params });
}

/** Xuất CSV báo cáo giảng viên — trả về text CSV (UTF-8 BOM). */
export async function fetchReportCsv(params: { lessonId: number }): Promise<string> {
  const response = await getData<unknown>({ method: 'GET', url: PROGRESS_ENDPOINTS.reportExport, params });
  return typeof response === 'string' ? response : '';
}
