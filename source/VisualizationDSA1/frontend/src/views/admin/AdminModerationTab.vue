<template>
  <section class="tab-section fade-in">
    <!-- Reports -->
    <div class="card card--moderation bg-bg-secondary/40 border border-border-subtle rounded-3xl p-6 mb-6">
      <div class="card-header-row flex justify-between items-center mb-6">
        <h3 class="card-heading flex items-center gap-2 m-0 text-text-primary text-base font-black">
          <BaseIcon name="warning" style="width:18px;height:18px;color:#fbbf24" />
          Báo cáo nội dung (Content Reports)
        </h3>
        <button class="btn-create-user flex items-center gap-1 bg-bg-hover border border-border-subtle px-3 py-1.5 rounded-xl text-xs text-white hover:bg-bg-hover transition-all font-bold cursor-pointer" @click="loadReports">
          Làm mới ↻
        </button>
      </div>

      <div v-if="loadingReports" class="loading-state py-12 text-center text-text-muted text-xs">
        <div class="spinner inline-block w-6 h-6 border-2 border-accent/20 border-t-indigo-400 rounded-full animate-spin mr-2"></div>
        Đang tải báo cáo...
      </div>
      <div v-else-if="reports.length === 0" class="empty-state py-12 text-center text-text-muted text-xs">
        Không có báo cáo chờ xử lý.
      </div>
      <div v-else class="table-responsive overflow-x-auto">
        <table class="data-table w-full text-left border-collapse">
          <thead>
            <tr class="text-text-muted text-xs border-b border-border-subtle">
              <th class="pb-3">Thời gian</th><th class="pb-3">Người báo cáo</th>
              <th class="pb-3">Lý do</th><th class="pb-3">Chi tiết</th><th class="pb-3">Trạng thái</th><th class="pb-3">Thao tác</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="r in reports" :key="r.id" class="border-b border-border-subtle text-xs hover:bg-bg-hover transition-colors">
              <td class="py-3 font-mono text-text-muted whitespace-nowrap">{{ formatDate(r.createdAt) }}</td>
              <td class="py-3 font-bold text-white">{{ r.reporterName || '—' }}</td>
              <td class="py-3">
                <div class="flex flex-col gap-1">
                  <span class="px-2 py-0.5 rounded text-[10px] font-bold uppercase tracking-wider whitespace-nowrap w-fit" :class="reasonClass(r.reason)">{{ r.reason }}</span>
                  <span v-if="r.nodeName" class="text-text-secondary font-medium truncate max-w-[180px]" :title="r.nodeName">{{ r.nodeName }}</span>
                  <span v-if="r.isNodeHidden" class="px-1.5 py-0.5 rounded text-[9px] font-bold uppercase tracking-wider bg-accent-red/10 text-accent-red border border-accent-red/20 w-fit">Đã ẩn</span>
                </div>
              </td>
              <td class="py-3 text-text-secondary max-w-[220px] truncate">{{ r.detail || '—' }}</td>
              <td class="py-3">
                <span class="px-2 py-0.5 rounded text-[10px] font-bold uppercase tracking-wider whitespace-nowrap" :class="statusClass(r.status)">{{ r.status }}</span>
              </td>
              <td class="py-3">
                <div class="flex gap-1.5">
                  <router-link :to="`/lessons/${r.nodeId}`" class="px-2 py-1 rounded-lg text-[10px] font-bold bg-accent-cyan/10 text-accent-cyan border border-accent-cyan/20 hover:bg-accent-cyan/20 whitespace-nowrap">Xem nội dung</router-link>
                  <button class="px-2 py-1 rounded-lg text-[10px] font-bold bg-accent-green/10 text-accent-green border border-accent-green/20 hover:bg-accent-green/20 cursor-pointer" @click="resolveReport(r.id, 'remove')">{{ r.isNodeHidden ? 'Đã ẩn' : 'Xóa nội dung' }}</button>
                  <button class="px-2 py-1 rounded-lg text-[10px] font-bold bg-accent/10 text-accent border border-accent/20 hover:bg-accent/20 cursor-pointer" @click="resolveReport(r.id, 'warn_teacher')">Cảnh cáo</button>
                  <button class="px-2 py-1 rounded-lg text-[10px] font-bold bg-bg-hover text-text-muted border border-border-subtle hover:bg-bg-hover/60 cursor-pointer" @click="resolveReport(r.id, 'dismiss')">Bỏ qua</button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Blacklist -->
    <div class="card card--blacklist bg-bg-secondary/40 border border-border-subtle rounded-3xl p-6">
      <div class="card-header-row flex justify-between items-center mb-6">
        <h3 class="card-heading flex items-center gap-2 m-0 text-text-primary text-base font-black">
          <BaseIcon name="shield" style="width:18px;height:18px;color:#34d399" />
          Danh sách từ khóa bị chặn (Blacklist)
        </h3>
        <div class="flex items-center gap-2">
          <input v-model="newKeyword" type="text" placeholder="Nhập từ khóa..."
            class="bg-bg-hover border border-border-subtle px-3 py-1.5 rounded-xl text-xs text-text-primary placeholder:text-text-muted focus:outline-none focus:border-border-accent"
            @keyup.enter="addKeyword" />
          <select v-model="newCategory" class="bg-bg-hover border border-border-subtle px-2 py-1.5 rounded-xl text-xs text-text-primary focus:outline-none">
            <option value="general">general</option>
            <option value="offensive">offensive</option>
            <option value="spam">spam</option>
          </select>
          <button class="bg-accent text-white px-3 py-1.5 rounded-xl text-xs font-bold hover:bg-accent-light cursor-pointer" @click="addKeyword">Thêm</button>
        </div>
      </div>

      <div v-if="loadingBlacklist" class="loading-state py-8 text-center text-text-muted text-xs">Đang tải...</div>
      <div v-else-if="blacklist.length === 0" class="empty-state py-8 text-center text-text-muted text-xs">Chưa có từ khóa nào.</div>
      <div v-else class="flex flex-wrap gap-2">
        <div v-for="b in blacklist" :key="b.id" class="flex items-center gap-2 px-3 py-1.5 rounded-xl bg-bg-hover border border-border-subtle text-xs">
          <span class="font-mono text-text-primary">{{ b.keyword }}</span>
          <span class="px-1.5 py-0.5 rounded text-[9px] font-bold uppercase tracking-wider bg-bg-surface text-text-muted">{{ b.category }}</span>
          <button class="text-text-muted hover:text-accent-red cursor-pointer" title="Xóa" @click="removeKeyword(b.id)">
            <BaseIcon name="x" class="w-3.5 h-3.5" />
          </button>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useToastStore } from '@/composables/useToast';
import { useAdminApi } from './useAdminApi';

const { BASE_URL, getAuthHeaders } = useAdminApi();
const toastStore = useToastStore();

interface ReportItem { id: string; nodeId: string; nodeName?: string | null; isNodeHidden?: boolean; reason: string; detail: string | null; status: string; reporterName: string | null; createdAt: string; }
interface BlacklistItem { id: string; keyword: string; category: string; createdAt: string; }

const reports = ref<ReportItem[]>([]);
const blacklist = ref<BlacklistItem[]>([]);
const loadingReports = ref(false);
const loadingBlacklist = ref(false);
const newKeyword = ref('');
const newCategory = ref('general');

async function loadReports() {
  loadingReports.value = true;
  try {
    const res = await fetch(`${BASE_URL}/api/v1/concepts/admin/reports`, { headers: getAuthHeaders() });
    if (res.ok) { const data = await res.json(); reports.value = Array.isArray(data) ? data : (data.reports ?? []); }
  } catch (err) { console.error('Failed to load reports:', err); }
  finally { loadingReports.value = false; }
}

async function resolveReport(id: string, action: string) {
  try {
    const res = await fetch(`${BASE_URL}/api/v1/concepts/admin/reports/${id}/resolve`, {
      method: 'PATCH', headers: getAuthHeaders(), body: JSON.stringify({ action })
    });
    if (res.ok) {
      toastStore.success('Đã xử lý báo cáo.');
      await loadReports();
    } else {
      const err = await res.json().catch(() => ({}));
      toastStore.error(err.message || 'Không thể xử lý báo cáo.');
    }
  } catch { toastStore.error('Lỗi kết nối khi xử lý báo cáo.'); }
}

async function loadBlacklist() {
  loadingBlacklist.value = true;
  try {
    const res = await fetch(`${BASE_URL}/api/v1/concepts/admin/blacklist`, { headers: getAuthHeaders() });
    if (res.ok) { const data = await res.json(); blacklist.value = Array.isArray(data) ? data : (data.items ?? []); }
  } catch (err) { console.error('Failed to load blacklist:', err); }
  finally { loadingBlacklist.value = false; }
}

async function addKeyword() {
  const keyword = newKeyword.value.trim();
  if (!keyword) return;
  try {
    const res = await fetch(`${BASE_URL}/api/v1/concepts/admin/blacklist`, {
      method: 'POST', headers: getAuthHeaders(), body: JSON.stringify({ keyword, category: newCategory.value })
    });
    if (res.ok) {
      toastStore.success('Đã thêm từ khóa.');
      newKeyword.value = '';
      await loadBlacklist();
    } else {
      const err = await res.json().catch(() => ({}));
      toastStore.error(err.message || 'Không thể thêm từ khóa.');
    }
  } catch { toastStore.error('Lỗi kết nối khi thêm từ khóa.'); }
}

async function removeKeyword(id: string) {
  try {
    const res = await fetch(`${BASE_URL}/api/v1/concepts/admin/blacklist/${id}`, {
      method: 'DELETE', headers: getAuthHeaders()
    });
    if (res.ok) { toastStore.success('Đã xóa từ khóa.'); await loadBlacklist(); }
    else toastStore.error('Không thể xóa từ khóa.');
  } catch { toastStore.error('Lỗi kết nối khi xóa từ khóa.'); }
}

function formatDate(dateStr: string) {
  try { return new Date(dateStr).toLocaleString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' }); }
  catch { return dateStr; }
}

function reasonClass(reason: string) {
  switch (reason) {
    case 'spam': return 'bg-accent-yellow/10 text-accent-yellow border border-accent-yellow/20';
    case 'offensive': return 'bg-accent-red/10 text-accent-red border border-accent-red/20';
    case 'wrong_info': return 'bg-accent-cyan/10 text-accent-cyan border border-accent-cyan/20';
    default: return 'bg-slate-500/10 text-text-muted border border-slate-500/20';
  }
}

function statusClass(status: string) {
  if (status === 'Pending') return 'bg-accent-warm/10 text-accent-warm border border-accent-warm/20';
  if (status === 'Resolved') return 'bg-accent-green/10 text-accent-green border border-accent-green/20';
  return 'bg-slate-500/10 text-text-muted border border-slate-500/20';
}

onMounted(() => {
  loadReports();
  loadBlacklist();
});
</script>
