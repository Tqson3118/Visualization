import { ref, reactive } from 'vue';
import { useAuthStore } from '../../features/auth/store/useAuthStore';
import { API_BASE_URL } from '@/services/apiConfig';

const BASE_URL = API_BASE_URL;

function getAuthHeaders() {
  const token = useAuthStore().getAccessToken();
  return {
    'Content-Type': 'application/json',
    ...(token ? { 'Authorization': `Bearer ${token}` } : {})
  };
}

export function useQuizBuilder() {
  const quizzesList = ref<any[]>([]);
  const loading = ref(false);
  const quizQuestions = reactive<Record<string, any[]>>({});
  const loadingQuizQuestions = ref<Record<string, boolean>>({});
  const expandedQuizId = ref<string | null>(null);
  const topics = ref<string[]>([]);

  async function loadQuizzes() {
    loading.value = true;
    try {
      const res = await fetch(`${BASE_URL}/api/v1/concepts/quiz/all`, { headers: getAuthHeaders() });
      if (res.ok) {
        quizzesList.value = await res.json();
        const ts: string[] = [...new Set(quizzesList.value.map((q: any) => q.topic).filter(Boolean))];
        topics.value = ts.sort();
      }
    } catch (err) {
      console.error('Failed to load quizzes:', err);
    } finally {
      loading.value = false;
    }
  }

  async function loadQuizQuestions(quizId: string) {
    loadingQuizQuestions.value[quizId] = true;
    try {
      const res = await fetch(`${BASE_URL}/api/v1/concepts/quiz/${quizId}`, { headers: getAuthHeaders() });
      if (res.ok) {
        const data = await res.json();
        quizQuestions[quizId] = data.questions ?? [];
      }
    } catch (err) {
      console.error('Failed to load questions:', err);
    } finally {
      loadingQuizQuestions.value[quizId] = false;
    }
  }

  async function createQuiz(data: any) {
    try {
      const res = await fetch(`${BASE_URL}/api/v1/concepts/quiz/manage`, {
        method: 'POST',
        headers: getAuthHeaders(),
        body: JSON.stringify(data)
      });
      if (res.ok) {
        await loadQuizzes();
      }
    } catch (err) {
      console.error('Failed to create quiz:', err);
    }
  }

  async function updateQuiz(id: string, data: any) {
    try {
      await fetch(`${BASE_URL}/api/v1/concepts/quiz/manage/${id}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(data)
      });
    } catch (err) {
      console.error('Failed to update quiz:', err);
    }
  }

  async function deleteQuiz(quizId: string) {
    try {
      const res = await fetch(`${BASE_URL}/api/v1/concepts/quiz/manage/${quizId}`, {
        method: 'DELETE',
        headers: getAuthHeaders()
      });
      if (res.ok) {
        quizzesList.value = quizzesList.value.filter((q: any) => q.id !== quizId);
      }
    } catch (err) {
      console.error('Failed to delete quiz:', err);
    }
  }

  async function addQuestionToQuiz(quizId: string, data: any) {
    try {
      // Lấy quiz hiện tại, thêm câu mới rồi PUT toàn bộ — không ghi đè mất câu cũ
      const detailRes = await fetch(`${BASE_URL}/api/v1/concepts/quiz/${quizId}`, { headers: getAuthHeaders() });
      if (!detailRes.ok) return;
      const quiz = await detailRes.json();
      const updated = {
        ...quiz,
        questions: [...(quiz.questions ?? []), data]
      };
      const res = await fetch(`${BASE_URL}/api/v1/concepts/quiz/manage/${quizId}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(updated)
      });
      if (res.ok) {
        await loadQuizQuestions(quizId);
      }
    } catch (err) {
      console.error('Failed to add question:', err);
    }
  }

  async function updateQuestion(quizId: string, questionId: string, data: any) {
    try {
      const detailRes = await fetch(`${BASE_URL}/api/v1/concepts/quiz/${quizId}`, { headers: getAuthHeaders() });
      if (!detailRes.ok) return;
      const quiz = await detailRes.json();
      const updated = {
        ...quiz,
        questions: (quiz.questions ?? []).map((q: any) =>
          String(q.id) === String(questionId) ? { ...q, ...data, id: q.id } : q
        )
      };
      const res = await fetch(`${BASE_URL}/api/v1/concepts/quiz/manage/${quizId}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(updated)
      });
      if (res.ok) {
        await loadQuizQuestions(quizId);
      }
    } catch (err) {
      console.error('Failed to update question:', err);
    }
  }

  async function deleteQuestion(quizId: string, questionId: string) {
    try {
      const detailRes = await fetch(`${BASE_URL}/api/v1/concepts/quiz/${quizId}`, { headers: getAuthHeaders() });
      if (!detailRes.ok) return;
      const quiz = await detailRes.json();
      const updated = {
        ...quiz,
        questions: (quiz.questions ?? []).filter((q: any) => String(q.id) !== String(questionId))
      };
      const res = await fetch(`${BASE_URL}/api/v1/concepts/quiz/manage/${quizId}`, {
        method: 'PUT',
        headers: getAuthHeaders(),
        body: JSON.stringify(updated)
      });
      if (res.ok) {
        await loadQuizQuestions(quizId);
      }
    } catch (err) {
      console.error('Failed to delete question:', err);
    }
  }

  return {
    quizzesList,
    loading,
    quizQuestions,
    loadingQuizQuestions,
    expandedQuizId,
    topics,
    loadQuizzes,
    loadQuizQuestions,
    createQuiz,
    updateQuiz,
    deleteQuiz,
    addQuestionToQuiz,
    updateQuestion,
    deleteQuestion
  };
}
