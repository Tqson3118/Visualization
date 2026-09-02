import { client } from '@/api/client';

export interface ExplainStepParams {
  algorithmTitle: string;
  stepIndex: number;
  explanation?: string;
  variables?: string;
  pseudocodeLine?: string;
  userQuestion: string;
}

export async function askAiStepExplanation(params: ExplainStepParams): Promise<string> {
  if (!params.userQuestion || !params.userQuestion.trim()) {
    throw new Error('Câu hỏi không được để trống.');
  }

  const response = await client.post<{ reply: string }>(
    '/ai/explain-step',
    params,
    { timeout: 35000 },
  );

  return response.data?.reply || 'AI chưa thể đưa ra câu trả lời vào lúc này.';
}
