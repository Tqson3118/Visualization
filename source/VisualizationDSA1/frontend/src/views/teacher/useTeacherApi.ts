import { useAuthStore } from '../../features/auth/store/useAuthStore';
import { API_BASE_URL } from '@/services/apiConfig';

const BASE_URL = API_BASE_URL;

export function useTeacherApi() {
  const authStore = useAuthStore();

  function getAuthHeaders(): Record<string, string> {
    const token = authStore.getAccessToken() || '';
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    };
  }

  function formatTopic(topic: string): string {
    const map: Record<string, string> = {
      'sorting': 'Sắp xếp',
      'graph': 'Đồ thị',
      'oop': 'Hướng đối tượng',
      'solid': 'Nguyên lý SOLID',
      'di': 'DI/IoC',
      'array': 'Mảng',
      'linked-list': 'Danh sách liên kết',
      'design-patterns': 'Mẫu thiết kế'
    };
    return map[topic] || topic;
  }

  function formatDifficulty(diff: string): string {
    const map: Record<string, string> = {
      'easy': 'Dễ',
      'medium': 'Trung bình',
      'hard': 'Khó'
    };
    return map[diff] || diff;
  }

  function formatDate(dateString: string): string {
    try {
      const d = new Date(dateString);
      return d.toLocaleDateString('vi-VN');
    } catch {
      return dateString;
    }
  }

  function formatAttemptDate(dateString: string): string {
    try {
      const d = new Date(dateString);
      return d.toLocaleDateString('vi-VN', {
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
      });
    } catch {
      return dateString;
    }
  }

  return {
    BASE_URL,
    getAuthHeaders,
    formatTopic,
    formatDifficulty,
    formatDate,
    formatAttemptDate
  };
}
