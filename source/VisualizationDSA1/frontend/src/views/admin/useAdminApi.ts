import { ref } from 'vue';
import { useAuthStore } from '../../features/auth/store/useAuthStore';
import { API_BASE_URL } from '@/services/apiConfig';

const BASE_URL = API_BASE_URL;

interface AuditLog {
  time: string;
  type: 'INFO' | 'WARN' | 'ERROR';
  message: string;
}


// Audit log hiện tại là dữ liệu DEMO (chưa nối endpoint thật) — hiển thị kèm nhãn Demo
const auditLogs = ref<AuditLog[]>([
  { time: '—', type: 'INFO', message: '[Demo] Nhật ký hệ thống là dữ liệu mẫu, chưa nối API thật.' },
]);

export function useAdminApi() {
  const authStore = useAuthStore();

  function getAuthHeaders(): Record<string, string> {
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${authStore.getAccessToken()}`
    };
  }

  function pushLog(type: 'INFO' | 'WARN' | 'ERROR', message: string): void {
    const d = new Date();
    const time = d.toTimeString().split(' ')[0];
    auditLogs.value.unshift({ time, type, message });
    if (auditLogs.value.length > 15) auditLogs.value.pop();
  }

  function getDifficultyLabel(diff: string): string {
    if (diff === 'easy') return 'Dễ';
    if (diff === 'hard') return 'Khó';
    return 'Trung bình';
  }

  return {
    BASE_URL,
    authStore,
    auditLogs,
    getAuthHeaders,
    pushLog,
    getDifficultyLabel
  };
}
