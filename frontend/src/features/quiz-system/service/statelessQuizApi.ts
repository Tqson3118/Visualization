import { getLessonAuthToken } from '../../lesson/services/lessonApi';

const BASE_URL = '/api/v1';

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
    const res = await fetch(`${BASE_URL}/concepts/quiz/all`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },

  async getTopics(): Promise<string[]> {
    const res = await fetch(`${BASE_URL}/concepts/quiz/topics`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },

  async getQuizById(quizId: string): Promise<StatelessQuizDetail> {
    const token = getLessonAuthToken();
    const headers: Record<string, string> = {};
    if (token) headers['Authorization'] = `Bearer ${token}`;
    const res = await fetch(`${BASE_URL}/concepts/quiz/${encodeURIComponent(quizId)}`, { headers });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },

  async getQuizzesByTopic(topic: string): Promise<StatelessQuizDetail[]> {
    const res = await fetch(`${BASE_URL}/concepts/quiz/topic/${encodeURIComponent(topic)}`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },

  async submitAttempt(quizId: string, answers: number[], token?: string | null, skipXp: boolean = false): Promise<StatelessAttemptResult> {
    const headers: Record<string, string> = { 'Content-Type': 'application/json' };
    const effectiveToken = token || getLessonAuthToken();
    if (effectiveToken) {
      headers['Authorization'] = `Bearer ${effectiveToken}`;
    }
    const url = new URL(`${window.location.origin}${BASE_URL}/concepts/quiz/submit`);
    if (skipXp) {
      url.searchParams.append('skipXp', 'true');
    }
    const res = await fetch(url.toString(), {
      method: 'POST',
      headers,
      body: JSON.stringify({ quizId, answers }),
    });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },
};
