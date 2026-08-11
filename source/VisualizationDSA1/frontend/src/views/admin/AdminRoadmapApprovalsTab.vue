<template>
  <div class="admin-roadmap-approvals admin-tab-content">
    <div class="header-section">
      <h3>Duyệt Lộ trình Học tập (Roadmaps)</h3>
      <p>Danh sách các lộ trình khóa học do Giảng viên yêu cầu xuất bản ra hệ thống.</p>
    </div>

    <div v-if="loading" class="flex justify-center py-8">
      <div class="spinner"></div>
    </div>

    <div v-else class="table-container">
      <table class="admin-table">
        <thead>
          <tr>
            <th>Tên Lộ trình</th>
            <th>Giảng viên</th>
            <th>Thể loại</th>
            <th>Ngày yêu cầu</th>
            <th>Trạng thái</th>
            <th class="text-right">Hành động</th>
          </tr>
        </thead>
        <tbody>
          <template v-for="rm in roadmaps" :key="rm.id">
            <tr>
              <td>
                <span class="font-bold text-text-primary">{{ rm.name }}</span>
              </td>
            <td>{{ rm.teacherName || 'N/A' }}</td>
            <td>
              <span class="px-2 py-1 bg-bg-hover rounded text-xs text-text-secondary">{{ rm.tags || 'N/A' }}</span>
            </td>
            <td>{{ new Date(rm.createdAt).toLocaleDateString() }}</td>
            <td>
              <span class="px-2 py-1 rounded text-[10px] font-bold" 
                    :class="rm.status === 'Pending' ? 'bg-accent-warm/20 text-accent-warm' : 'bg-bg-hover text-text-secondary'">
                {{ rm.status }}
              </span>
            </td>
            <td class="text-right">
              <div class="flex gap-2 justify-end">
                <button 
                  v-if="rm.status === 'Pending'"
                  @click="approveRoadmap(rm.id)" 
                  class="px-3 py-1 bg-accent-green hover:bg-accent-green text-text-primary rounded text-xs font-bold transition-colors"
                  :disabled="approvingId === rm.id"
                >
                  {{ approvingId === rm.id ? 'Đang duyệt...' : 'Xuất bản' }}
                </button>
                <button 
                  v-if="rm.status === 'Pending'"
                  @click="startReject(rm.id)" 
                  class="px-3 py-1 bg-accent-red hover:bg-accent-red text-text-primary rounded text-xs font-bold transition-colors"
                  :disabled="rejectingId === rm.id"
                >
                  {{ rejectingId === rm.id ? 'Đang từ chối...' : 'Từ chối' }}
                </button>
              </div>
            </td>
            </tr>
            <tr v-if="rejectingRoadmapId === rm.id">
              <td colspan="6" class="p-0">
                <div class="flex items-center gap-2 px-4 py-3 bg-accent-red/10 border-t border-accent-red/20">
                  <input
                    v-model="rejectReason"
                    type="text"
                    class="flex-1 bg-bg-primary border border-border-subtle rounded-lg px-3 py-2 text-xs text-text-primary focus:outline-none focus:border-accent"
                    placeholder="Nhập lý do từ chối..."
                    @keyup.enter="confirmReject(rm.id)"
                  />
                  <button @click="confirmReject(rm.id)" class="px-3 py-2 bg-accent-red hover:bg-accent-red text-text-primary rounded-lg text-xs font-bold transition-colors" :disabled="rejectingId === rm.id">
                    Xác nhận
                  </button>
                  <button @click="cancelReject" class="px-3 py-2 bg-bg-hover text-text-secondary rounded-lg text-xs font-bold transition-colors">
                    Hủy
                  </button>
                </div>
              </td>
            </tr>
          </template>
          <tr v-if="roadmaps.length === 0">
            <td colspan="6" class="text-center py-8 text-text-muted">Không có lộ trình nào chờ duyệt.</td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { teacherStudioService } from '@/services/TeacherStudioService';
import { useToastStore } from '@/composables/useToast';

const roadmaps = ref<CustomRoadmapDto[]>([]);
const loading = ref(false);
const approvingId = ref<string | null>(null);
const rejectingId = ref<string | null>(null);
const toastStore = useToastStore();

interface CustomRoadmapDto {
  id: string;
  teacherId: string;
  name: string;
  description: string;
  tags: string;
  thumbnailUrl?: string;
  visibility: string;
  status: string;
  adminRejectReason?: string;
  createdAt: string;
  teacherName?: string;
  nodes: any[];
}

const loadPendingRoadmaps = async () => {
  loading.value = true;
  try {
    const res = await teacherStudioService.getPendingRoadmaps();
    roadmaps.value = res;
  } catch (err) {
    toastStore.error('Lỗi khi tải danh sách lộ trình chờ duyệt');
  } finally {
    loading.value = false;
  }
};

const approveRoadmap = async (id: string) => {
  approvingId.value = id;
  try {
    await teacherStudioService.approveRoadmap(id);
    toastStore.success('Đã duyệt và xuất bản Lộ trình.');
    await loadPendingRoadmaps();
  } catch (err: any) {
    toastStore.error(err.response?.data?.message || 'Lỗi khi duyệt lộ trình');
  } finally {
    approvingId.value = null;
  }
};

const rejectingRoadmapId = ref<string | null>(null);
const rejectReason = ref('');

function startReject(id: string): void {
  rejectingRoadmapId.value = id;
  rejectReason.value = '';
}

function cancelReject(): void {
  rejectingRoadmapId.value = null;
  rejectReason.value = '';
}

async function confirmReject(id: string): Promise<void> {
  const reason = rejectReason.value.trim();
  if (!reason) {
    toastStore.error('Vui lòng nhập lý do từ chối.');
    return;
  }
  rejectingId.value = id;
  try {
    await teacherStudioService.rejectRoadmap(id, reason);
    toastStore.success('Đã từ chối lộ trình.');
    rejectingRoadmapId.value = null;
    rejectReason.value = '';
    await loadPendingRoadmaps();
  } catch (err: any) {
    toastStore.error(err.response?.data?.message || 'Lỗi khi từ chối lộ trình');
  } finally {
    rejectingId.value = null;
  }
};

onMounted(() => {
  loadPendingRoadmaps();
});
</script>

<style scoped>
.admin-tab-content { padding: 1.5rem; }
.header-section { margin-bottom: 1.5rem; }
.header-section h3 { font-size: 1.25rem; font-weight: bold; color: white; margin-bottom: 0.25rem; }
.header-section p { font-size: 0.875rem; color: #94a3b8; }
.admin-table { width: 100%; border-collapse: collapse; font-size: 0.875rem; }
.admin-table th { text-align: left; padding: 0.75rem 1rem; border-bottom: 1px solid rgba(255,255,255,0.1); color: #94a3b8; font-weight: 600; text-transform: uppercase; font-size: 0.75rem; }
.admin-table td { padding: 1rem; border-bottom: 1px solid rgba(255,255,255,0.05); color: #e2e8f0; }
.admin-table tr:hover td { background: rgba(255,255,255,0.02); }
.spinner {
  width: 32px;
  height: 32px;
  border: 3px solid rgba(255,255,255,0.1);
  border-top-color: var(--color-accent-primary);
  border-radius: 50%;
  animation: spin 1s linear infinite;
}
@keyframes spin {
  to { transform: rotate(360deg); }
}
</style>
