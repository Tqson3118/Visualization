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

// ── Stub CRUD (body TODO) ──

export async function fetchOverview(): Promise<ProgressOverviewDto> {
  // TODO: getData({ method: 'GET', url: PROGRESS_ENDPOINTS.overview })
  return Promise.reject(new Error('TODO: progressApi.fetchOverview chưa triển khai'));
}

export async function fetchLessonProgress(lessonId: number): Promise<{ viewed: boolean; bestScore: number | null; completed: boolean }> {
  // TODO: getData({ method: 'GET', url: PROGRESS_ENDPOINTS.lesson(lessonId) })
  return Promise.reject(new Error('TODO: progressApi.fetchLessonProgress chưa triển khai'));
}

export async function fetchReport(params: { lessonId: number }): Promise<TeacherReportDto> {
  // TODO: getData({ method: 'GET', url: PROGRESS_ENDPOINTS.report, params })
  return Promise.reject(new Error('TODO: progressApi.fetchReport chưa triển khai'));
}
