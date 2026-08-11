import { api } from '../../../services/apiClient';

export interface CodelabDto {
  id: string;
  title: string;
  description: string;
  initialCode: string;
  difficulty: number;
  xpReward: number;
  maxRuntimeMs: number;
  maxMemoryBytes: number;
  allowedLanguages: string;
  constraints?: string;
  examples?: Array<{ input: string; expectedOutput: string }>;
  hints?: Array<{ content: string; xpCost?: number }>;
  expectedOutput?: string;
  testcases?: CodelabTestCaseDto[];
  templates?: CodelabTemplateDto[];
}

export interface SubmitCodeRequest {
  code: string;
  language: string;
}

export interface SubmitCodeResult {
  submissionId?: string;
  passed: boolean;
  status: string;
  errorMessage: string;
  runtimeMs: number;
  memoryBytes: number;
  testCaseResultsJson?: string;
}

export interface CodelabTestCaseDto {
  id: string;
  codelabId: string;
  input: string;
  expectedOutput: string;
  isHidden: boolean;
  orderIndex: number;
}

export interface CodelabTemplateDto {
  id: string;
  codelabId: string;
  language: string;
  starterCode: string;
  orderIndex: number;
}

export interface CodelabHintDto {
  id: string;
  codelabId: string;
  content: string;
  isTiered: boolean;
  xpCost: number;
  orderIndex: number;
}

export interface CreateCodelabRequest {
  title: string;
  description: string;
  difficulty: number;
  xpReward: number;
  maxRuntimeMs: number;
  maxMemoryBytes: number;
  allowedLanguages: string;
  initialCode: string;
  constraints?: string;
  examples?: Array<{ input: string; expectedOutput: string }>;
  hints?: Array<{ content: string; isTiered: boolean; xpCost: number }>;
}

export interface UpdateCodelabRequest extends Partial<CreateCodelabRequest> {
  id: string;
}

export interface CreateTestCaseRequest {
  input: string;
  expectedOutput: string;
  isHidden: boolean;
  orderIndex: number;
}

export interface UpdateTestCaseRequest extends Partial<CreateTestCaseRequest> {
  id: string;
}

export interface CreateTemplateRequest {
  language: string;
  starterCode: string;
  orderIndex: number;
}

export interface UpdateTemplateRequest extends Partial<CreateTemplateRequest> {
  id: string;
}

export interface CreateHintRequest {
  content: string;
  isTiered: boolean;
  xpCost: number;
  orderIndex: number;
}

export interface UpdateHintRequest extends Partial<CreateHintRequest> {
  id: string;
}

export const codelabApi = {
  async getCodelabs(params?: { search?: string; difficulty?: string; language?: string }): Promise<CodelabDto[]> {
    const searchParams = new URLSearchParams();
    if (params?.search) searchParams.append('search', params.search);
    if (params?.difficulty) searchParams.append('difficulty', params.difficulty);
    if (params?.language) searchParams.append('language', params.language);
    const query = searchParams.toString() ? `?${searchParams.toString()}` : '';
    return await api.get<CodelabDto[]>(`/codelabs${query}`);
  },

  async getCodelab(id: string): Promise<CodelabDto> {
    return await api.get<CodelabDto>(`/codelabs/${id}`);
  },

  async createCodelab(data: CreateCodelabRequest): Promise<{ codelabId: string; message: string }> {
    return await api.post<{ codelabId: string; message: string }>('/codelabs', data);
  },

  async updateCodelab(id: string, data: UpdateCodelabRequest): Promise<{ message: string }> {
    return await api.put<{ message: string }>(`/codelabs/${id}`, data);
  },

  async deleteCodelab(id: string): Promise<{ message: string }> {
    return await api.delete<{ message: string }>(`/codelabs/${id}`);
  },

  async submitCodelab(id: string, payload: SubmitCodeRequest): Promise<SubmitCodeResult> {
    return await api.post<SubmitCodeResult>(`/codelabs/${id}/submit`, payload);
  },

  async runCodelab(id: string, payload: SubmitCodeRequest): Promise<SubmitCodeResult> {
    return await api.post<SubmitCodeResult>(`/codelabs/${id}/run`, payload);
  },

  async revealHint(id: string, hintIndex: number): Promise<void> {
    await api.post(`/codelabs/${id}/hints/${hintIndex}/reveal`);
  },

  // Test Cases
  async getTestCases(codelabId: string): Promise<CodelabTestCaseDto[]> {
    return await api.get<CodelabTestCaseDto[]>(`/codelabs/${codelabId}/testcases`);
  },

  async createTestCase(codelabId: string, data: CreateTestCaseRequest): Promise<{ testCaseId: string; message: string }> {
    return await api.post<{ testCaseId: string; message: string }>(`/codelabs/${codelabId}/testcases`, data);
  },

  async updateTestCase(codelabId: string, testCaseId: string, data: UpdateTestCaseRequest): Promise<{ message: string }> {
    return await api.put<{ message: string }>(`/codelabs/${codelabId}/testcases/${testCaseId}`, data);
  },

  async deleteTestCase(codelabId: string, testCaseId: string): Promise<{ message: string }> {
    return await api.delete<{ message: string }>(`/codelabs/${codelabId}/testcases/${testCaseId}`);
  },

  // Templates
  async getTemplates(codelabId: string): Promise<CodelabTemplateDto[]> {
    return await api.get<CodelabTemplateDto[]>(`/codelabs/${codelabId}/templates`);
  },

  async createTemplate(codelabId: string, data: CreateTemplateRequest): Promise<{ templateId: string; message: string }> {
    return await api.post<{ templateId: string; message: string }>(`/codelabs/${codelabId}/templates`, data);
  },

  async updateTemplate(codelabId: string, templateId: string, data: UpdateTemplateRequest): Promise<{ message: string }> {
    return await api.put<{ message: string }>(`/codelabs/${codelabId}/templates/${templateId}`, data);
  },

  async deleteTemplate(codelabId: string, templateId: string): Promise<{ message: string }> {
    return await api.delete<{ message: string }>(`/codelabs/${codelabId}/templates/${templateId}`);
  },

  // Hints
  async getHints(codelabId: string): Promise<CodelabHintDto[]> {
    return await api.get<CodelabHintDto[]>(`/codelabs/${codelabId}/hints`);
  },

  async createHint(codelabId: string, data: CreateHintRequest): Promise<{ hintId: string; message: string }> {
    return await api.post<{ hintId: string; message: string }>(`/codelabs/${codelabId}/hints`, data);
  },

  async updateHint(codelabId: string, hintId: string, data: UpdateHintRequest): Promise<{ message: string }> {
    return await api.put<{ message: string }>(`/codelabs/${codelabId}/hints/${hintId}`, data);
  },

  async deleteHint(codelabId: string, hintId: string): Promise<{ message: string }> {
    return await api.delete<{ message: string }>(`/codelabs/${codelabId}/hints/${hintId}`);
  },
};
