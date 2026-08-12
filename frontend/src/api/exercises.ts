/** Endpoint theo API_REFERENCE §4.6 */
export const EXERCISE_ENDPOINTS = {
  list: '/exercises',
  detail: (id: number) => `/exercises/${id}`,
  submit: (id: number) => `/exercises/${id}/submit`,
  practice: (id: number) => `/exercises/${id}/practice`,
  mySubmissions: (id: number) => `/exercises/${id}/submissions/me`,
  codeSubmit: (id: number) => `/exercises/${id}/code-submit`,
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
  type: 'MCQ' | 'SIMULATION_LAB' | 'CODE';
  lessonId: number | null;
  nodeId: number | null;
  stage: number;
  durationMinutes: number;
  maxScore: number;
  status: 'draft' | 'active' | 'hidden';
  questions: QuestionDto[];
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

// ── Stub CRUD (body TODO) ──

export async function fetchExercises(params: { lessonId?: number; nodeId?: number; stage?: number } = {}): Promise<ExerciseDto[]> {
  // TODO: getData({ method: 'GET', url: EXERCISE_ENDPOINTS.list, params })
  return Promise.reject(new Error('TODO: exercisesApi.fetchExercises chưa triển khai'));
}

export async function fetchExercise(id: number): Promise<ExerciseDto> {
  // TODO: getData({ method: 'GET', url: EXERCISE_ENDPOINTS.detail(id) })
  return Promise.reject(new Error('TODO: exercisesApi.fetchExercise chưa triển khai'));
}

export async function submitExercise(id: number, payload: SubmitRequest): Promise<SubmitResultDto> {
  // TODO: getData({ method: 'POST', url: EXERCISE_ENDPOINTS.submit(id), data: payload })
  return Promise.reject(new Error('TODO: exercisesApi.submitExercise chưa triển khai'));
}

export async function practiceExercise(id: number, answers: Array<{ questionId: number; selected: number[] }>): Promise<SubmitResultDto> {
  // TODO: getData({ method: 'POST', url: EXERCISE_ENDPOINTS.practice(id), data: { answers } })
  return Promise.reject(new Error('TODO: exercisesApi.practiceExercise chưa triển khai'));
}

export async function fetchMySubmissions(id: number): Promise<SubmitResultDto[]> {
  // TODO: getData({ method: 'GET', url: EXERCISE_ENDPOINTS.mySubmissions(id) })
  return Promise.reject(new Error('TODO: exercisesApi.fetchMySubmissions chưa triển khai'));
}
