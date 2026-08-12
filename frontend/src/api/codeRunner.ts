import { client, getData } from './client';

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

/** Payload POST /code-runs theo API_REFERENCE §4.13 / ADR-012 — key bắt buộc string, input là string (SETUP_TODO §6.7). */
export interface SaveCodeRunPayload {
  exerciseId?: number | null;
  key: string;
  code: string;
  input?: string | null;
  status?: 'Success' | 'Error' | 'Timeout' | string;
  durationMs?: number;
  stats?: { comparisons?: number | null; swaps?: number | null; steps?: number | null };
  output?: string | null;
  error?: string | null;
  trace?: unknown[];
}

// ── CRUD (API_REFERENCE §4.13) ──

export async function saveCodeRun(payload: SaveCodeRunPayload): Promise<CodeRunSummary> {
  return getData<CodeRunSummary>({ method: 'POST', url: CODE_RUNNER_ENDPOINTS.codeRuns, data: payload });
}

export async function fetchCodeRun(id: number): Promise<CodeRunSummary> {
  return getData<CodeRunSummary>({ method: 'GET', url: CODE_RUNNER_ENDPOINTS.codeRun(id) });
}

export async function fetchCodeRunTrace(id: number): Promise<unknown[]> {
  return getData<unknown[]>({ method: 'GET', url: CODE_RUNNER_ENDPOINTS.codeRunTrace(id) });
}

export async function submitCode(exerciseId: number, code: string): Promise<CodeSubmitResult> {
  return getData<CodeSubmitResult>({
    method: 'POST',
    url: CODE_RUNNER_ENDPOINTS.codeSubmit(exerciseId),
    data: { code },
  });
}

export async function fetchMyCodeSubmissions(exerciseId: number): Promise<CodeRunSummary[]> {
  return getData<CodeRunSummary[]>({ method: 'GET', url: CODE_RUNNER_ENDPOINTS.myCodeSubmissions(exerciseId) });
}

export { client };
