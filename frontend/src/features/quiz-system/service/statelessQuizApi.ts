import { client } from '@/api/client';

export interface StatelessQuizSummary {
  id: string;
  title: string;
  topic: string;
  difficulty: string;
  xpReward: number;
  questionCount: number;
}

export interface StatelessQuestion {
  id: string;
  text: string;
  options: string[];
  correctIndex: number;
  explanation: string;
}

export interface StatelessQuizDetail {
  id: string;
  title: string;
  topic: string;
  difficulty: string;
  xpReward: number;
  questions: StatelessQuestion[];
}

export interface StatelessAttemptResult {
  score: number;
  maxScore: number;
  passed: boolean;
  xpAwarded: number;
  questionResults: Array<{
    questionId: string;
    isCorrect: boolean;
    correctIndex: number;
    explanation: string;
  }>;
}

export const statelessQuizApi = {
  async getAllQuizzes(): Promise<StatelessQuizSummary[]> {
    const res = await client.get<StatelessQuizSummary[]>('/concepts/quiz/all');
    return res.data;
  },

  async getTopics(): Promise<string[]> {
    const res = await client.get<string[]>('/concepts/quiz/topics');
    return res.data;
  },

  async getQuizById(quizId: string): Promise<StatelessQuizDetail> {
    const res = await client.get<StatelessQuizDetail>(`/concepts/quiz/${encodeURIComponent(quizId)}`);
    return res.data;
  },

  async getQuizzesByTopic(topic: string): Promise<StatelessQuizDetail[]> {
    const res = await client.get<StatelessQuizDetail[]>(`/concepts/quiz/topic/${encodeURIComponent(topic)}`);
    return res.data;
  },

  async submitAttempt(quizId: string, answers: number[], token?: string | null, skipXp: boolean = false): Promise<StatelessAttemptResult> {
    const params = skipXp ? { skipXp: 'true' } : undefined;
    const res = await client.post<StatelessAttemptResult>(
      '/concepts/quiz/submit',
      { quizId, answers },
      { params },
    );
    return res.data;
  },
};
