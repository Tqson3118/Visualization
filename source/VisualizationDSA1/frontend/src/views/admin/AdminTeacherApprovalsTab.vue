<template>
  <div class="admin-teacher-approvals admin-tab-content">
    <div class="header-section">
      <h3>Duyệt đơn Giảng viên</h3>
      <p>Danh sách các đơn đăng ký trở thành giảng viên chờ duyệt.</p>
    </div>

    <div class="mb-4 flex gap-2">
      <button
        v-for="f in ['Pending', 'Approved', 'Rejected']"
        :key="f"
        @click="statusFilter = f; loadApplications()"
        class="px-3 py-1.5 rounded-lg text-xs font-bold border transition-colors"
        :class="statusFilter === f ? 'bg-accent text-white border-accent' : 'bg-bg-surface text-text-secondary border-border-default hover:bg-bg-hover'"
      >
        {{ f }}
      </button>
    </div>

    <div class="table-container">
      <table class="admin-table">
        <thead>
          <tr>
            <th>Tài khoản</th>
            <th>Email</th>
            <th>Lý do / Hồ sơ</th>
            <th>Ngày nộp đơn</th>
            <th>Trạng thái</th>
            <th class="text-right">Hành động</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="app in applications" :key="app.id">
            <td>
              <div class="flex items-center gap-2">
                <span class="font-bold text-text-primary">{{ app.username || app.schoolName }}</span>
              </div>
            </td>
            <td>{{ app.email || '—' }}</td>
            <td class="max-w-xs truncate text-xs">{{ app.reason || app.bio }}</td>
            <td>{{ formatDate(app.createdAt || app.appliedAt) }}</td>
            <td>
              <span class="px-2 py-1 rounded text-[10px] font-bold"
                    :class="app.status === 'Pending' ? 'bg-accent-warm/20 text-accent-warm' : app.status === 'Approved' ? 'bg-accent-green/20 text-accent-green' : 'bg-accent-red/20 text-accent-red'">
                {{ app.status }}
              </span>
              <p v-if="app.status === 'Rejected' && app.rejectReason" class="text-[10px] text-accent-red mt-1">Lý do: {{ app.rejectReason }}</p>
            </td>
            <td class="text-right">
              <div class="flex gap-2 justify-end" v-if="app.status === 'Pending'">
                <button @click="approveApp(app.id)" class="px-3 py-1 bg-accent-green hover:bg-accent-green text-text-primary rounded text-xs font-bold transition-colors">
                  Duyệt
                </button>
                <button @click="openReject(app.id)" class="px-3 py-1 bg-accent-red hover:bg-accent-red text-text-primary rounded text-xs font-bold transition-colors">
                  Từ chối
                </button>
              </div>
            </td>
          </tr>
          <tr v-if="applications.length === 0 && !loading">
            <td colspan="6" class="text-center py-8 text-text-muted">Không có đơn nào.</td>
          </tr>
        </tbody>
      </table>
    </div>

    <!-- Reject modal -->
    <div v-if="showRejectModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm" @click.self="showRejectModal = false">
      <div class="glass-panel rounded-xl max-w-md w-full shadow-2xl overflow-hidden">
        <div class="px-6 py-4 border-b border-border-default flex justify-between items-center">
          <h3 class="text-lg font-bold text-text-primary">Từ chối đơn đăng ký</h3>
          <button class="text-text-secondary hover:text-text-primary" @click="showRejectModal = false">
            <BaseIcon name="x" class="w-5 h-5" />
          </button>
        </div>
        <div class="p-6">
          <label class="block text-sm font-medium text-text-secondary mb-2">Lý do từ chối</label>
          <textarea v-model="rejectReason" rows="3" class="w-full bg-bg-secondary border border-border-default rounded-lg px-4 py-2 text-text-primary" placeholder="Nhập lý do..."></textarea>
          <div class="flex justify-end space-x-3 mt-4">
            <button class="btn btn-secondary" @click="showRejectModal = false">Hủy</button>
            <button class="btn btn-primary" :disabled="!rejectReason.trim()" @click="confirmReject">Từ chối</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useToastStore } from '@/composables/useToast';
import { ref, onMounted } from 'vue';
import { useAuthStore } from '@/features/auth/store/useAuthStore';
import BaseIcon from '@/shared/components/BaseIcon.vue';
import { API_BASE_URL } from '@/services/apiConfig';

interface TeacherApplication {
  id: string;
  username?: string;
  email?: string;
  schoolName?: string;
  reason?: string;
  bio?: string;
  createdAt?: string;
  appliedAt?: string;
  status: string;
  rejectReason?: string;
}

const toastStore = useToastStore();
const authStore = useAuthStore();
const BASE_URL = API_BASE_URL;

const applications = ref<TeacherApplication[]>([]);
const loading = ref(false);
const statusFilter = ref('Pending');

const showRejectModal = ref(false);
const rejectTargetId = ref<string | null>(null);
const rejectReason = ref('');

function authHeaders(): Record<string, string> {
  return {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${authStore.getAccessToken()}`,
  };
}

function formatDate(dateStr?: string): string {
  if (!dateStr) return '—';
  return new Date(dateStr).toLocaleDateString('vi-VN');
}

async function loadApplications(): Promise<void> {
  loading.value = true;
  try {
    const res = await fetch(`${BASE_URL}/api/v1/admin/teacher-applications?status=${statusFilter.value}`, { headers: authHeaders() });
    if (res.ok) {
      applications.value = await res.json();
    }
  } catch (err) {
    toastStore.error('Không thể tải danh sách đơn.');
  } finally {
    loading.value = false;
  }
}

async function approveApp(id: string): Promise<void> {
  try {
    const res = await fetch(`${BASE_URL}/api/v1/admin/teacher-applications/${id}/approve`, {
      method: 'PATCH',
      headers: authHeaders(),
    });
    if (res.ok) {
      toastStore.success('Đã duyệt đơn đăng ký.');
      await loadApplications();
    } else {
      const err = await res.json();
      toastStore.error(err.message ?? 'Lỗi khi duyệt.');
    }
  } catch (err) {
    toastStore.error('Không thể kết nối máy chủ.');
  }
}

function openReject(id: string): void {
  rejectTargetId.value = id;
  rejectReason.value = '';
  showRejectModal.value = true;
}

async function confirmReject(): Promise<void> {
  if (!rejectTargetId.value) return;
  try {
    const res = await fetch(`${BASE_URL}/api/v1/admin/teacher-applications/${rejectTargetId.value}/reject`, {
      method: 'PATCH',
      headers: authHeaders(),
      body: JSON.stringify({ reason: rejectReason.value }),
    });
    if (res.ok) {
      toastStore.success('Đã từ chối đơn đăng ký.');
      showRejectModal.value = false;
      await loadApplications();
    } else {
      const err = await res.json();
      toastStore.error(err.message ?? 'Lỗi khi từ chối.');
    }
  } catch (err) {
    toastStore.error('Không thể kết nối máy chủ.');
  }
}

onMounted(() => {
  loadApplications();
});
</script>

<style scoped>
.admin-tab-content { padding: 1.5rem; }
.header-section { margin-bottom: 1.5rem; }
.header-section h3 { font-size: 1.25rem; font-weight: bold; color: var(--color-text-primary); margin-bottom: 0.25rem; }
.header-section p { font-size: 0.875rem; color: var(--color-text-secondary); }
.admin-table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
.admin-table th { text-align: left; padding: 0.75rem 1rem; border-bottom: 1px solid var(--color-border-default); color: var(--color-text-secondary); font-weight: 600; text-transform: uppercase; font-size: 0.75rem; }
.admin-table td { padding: 1rem; border-bottom: 1px solid var(--color-border-subtle); color: var(--color-text-primary); }
.admin-table tr:hover td { background: var(--color-bg-hover); }
</style>
