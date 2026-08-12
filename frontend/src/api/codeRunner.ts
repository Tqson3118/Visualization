/** Endpoint theo API_REFERENCE §4.13 (Module I — Code Runner, ADR-012) */
export const CODE_RUNNER_ENDPOINTS = {
  codeRuns: '/code-runs',
  codeRun: (id: number) => `/code-runs/${id}`,
  codeRunTrace: (id: number) => `/code-runs/${id}/trace`,
  codeSubmit: (exerciseId: number) => `/exercises/${exerciseId}/code-submit`,
  codeSubmissions: (exerciseId: number) => `/exercises/${exerciseId}/code-submissions`,
  myCodeSubmissions: (exerciseId: number) => `/exercises/${exerciseId}/code-submissions/me`,
} as const;

// ── DTO ──

export interface CodeRunSummary {
  id: number;
  exerciseId: number | null;
  status: 'pending' | 'running' | 'passed' | 'failed' | 'error';
  passed: number | null;
  total: number | null;
  createdAt: string;
}

export interface CodeSubmitResult {
  score: number;
  passed: number;
  total: number;
  results: Array<{ testId: string; passed: boolean; message: string }>;
}

// ── Stub CRUD (body TODO) ──

export async function saveCodeRun(payload: { exerciseId?: number | null; code: string; input?: unknown }): Promise<CodeRunSummary> {
  // TODO: getData({ method: 'POST', url: CODE_RUNNER_ENDPOINTS.codeRuns, data: payload })
  return Promise.reject(new Error('TODO: codeRunnerApi.saveCodeRun chưa triển khai'));
}

export async function fetchCodeRun(id: number): Promise<CodeRunSummary> {
  // TODO: getData({ method: 'GET', url: CODE_RUNNER_ENDPOINTS.codeRun(id) })
  return Promise.reject(new Error('TODO: codeRunnerApi.fetchCodeRun chưa triển khai'));
}

export async function fetchCodeRunTrace(id: number): Promise<unknown[]> {
  // TODO: getData({ method: 'GET', url: CODE_RUNNER_ENDPOINTS.codeRunTrace(id) })
  return Promise.reject(new Error('TODO: codeRunnerApi.fetchCodeRunTrace chưa triển khai'));
}

export async function submitCode(exerciseId: number, code: string): Promise<CodeSubmitResult> {
  // TODO: getData({ method: 'POST', url: CODE_RUNNER_ENDPOINTS.codeSubmit(exerciseId), data: { code } })
  return Promise.reject(new Error('TODO: codeRunnerApi.submitCode chưa triển khai'));
}

export async function fetchMyCodeSubmissions(exerciseId: number): Promise<CodeRunSummary[]> {
  // TODO: getData({ method: 'GET', url: CODE_RUNNER_ENDPOINTS.myCodeSubmissions(exerciseId) })
  return Promise.reject(new Error('TODO: codeRunnerApi.fetchMyCodeSubmissions chưa triển khai'));
}
