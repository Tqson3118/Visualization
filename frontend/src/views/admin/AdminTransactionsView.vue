<script setup lang="ts">
import { computed, onMounted, ref, reactive } from 'vue';
import {
  CreditCard,
  Crown,
  Sparkles,
  Plus,
  RefreshCw,
  Search,
  CheckCircle2,
  Clock,
  User,
  ShieldCheck,
  AlertCircle,
  Calendar,
  Zap,
} from 'lucide-vue-next';

import * as adminApi from '@/api/admin';
import type { AdminSubscriptionDto } from '@/api/admin';
import { useUiStore } from '@/stores/ui';
import StudioShell from '@/components/studio/StudioShell.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const ui = useUiStore();

const loading = ref(true);
const subscriptions = ref<AdminSubscriptionDto[]>([]);
const filterStatus = ref<'all' | 'active' | 'pending' | 'expired'>('all');
const searchQuery = ref('');

// Grant Modal
const grantModalOpen = ref(false);
const granting = ref(false);
const grantForm = reactive({
  email: '',
  planId: 'pro-monthly',
  durationDays: 30,
});

async function loadSubscriptions(): Promise<void> {
  loading.value = true;
  try {
    const data = await adminApi.fetchAdminSubscriptions();
    subscriptions.value = data || [];
  } catch (err) {
    ui.showToast('Không thể tải danh sách giao dịch & đăng ký Pro.', 'error');
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  void loadSubscriptions();
});

function isSubscriptionActive(sub: AdminSubscriptionDto): boolean {
  if (sub.status === 2) return false;
  if (sub.isActive) return true;
  if (!sub.expiresAt && sub.status === 0) return true;
  if (sub.expiresAt && new Date(sub.expiresAt) > new Date()) return true;
  return false;
}

function getSubBadgeVariant(sub: AdminSubscriptionDto): 'success' | 'warning' | 'muted' {
  if (sub.status === 2) return 'warning';
  if (isSubscriptionActive(sub)) return 'success';
  return 'muted';
}

function getSubBadgeLabel(sub: AdminSubscriptionDto): string {
  if (sub.status === 2) return '🚫 Hủy kích hoạt';
  if (isSubscriptionActive(sub)) {
    return sub.expiresAt ? '✨ Đang kích hoạt' : '✨ Vĩnh viễn';
  }
  return 'Đã hết hạn';
}

const filteredSubs = computed(() => {
  return subscriptions.value.filter((s) => {
    const q = searchQuery.value.trim().toLowerCase();
    const matchSearch =
      !q ||
      s.userEmail.toLowerCase().includes(q) ||
      s.userDisplayName.toLowerCase().includes(q) ||
      (s.orderRef && s.orderRef.toLowerCase().includes(q));

    const isPending = s.status === 2;
    const isActive = isSubscriptionActive(s);
    const isExpired = s.status === 1 || (!isPending && !isActive);

    const matchStatus =
      filterStatus.value === 'all' ||
      (filterStatus.value === 'active' && isActive) ||
      (filterStatus.value === 'pending' && isPending) ||
      (filterStatus.value === 'expired' && isExpired);

    return matchSearch && matchStatus;
  });
});

const activeCount = computed(() => subscriptions.value.filter((s) => isSubscriptionActive(s)).length);
const pendingCount = computed(() => subscriptions.value.filter((s) => s.status === 2).length);
const expiredCount = computed(() => subscriptions.value.filter((s) => s.status === 1 || (s.status !== 2 && !isSubscriptionActive(s))).length);

function openGrantModal(): void {
  grantForm.email = '';
  grantForm.planId = 'pro-monthly';
  grantForm.durationDays = 30;
  grantModalOpen.value = true;
}

async function handleGrantPro(): Promise<void> {
  if (!grantForm.email.trim()) {
    ui.showToast('Vui lòng nhập email người dùng.', 'warning');
    return;
  }
  granting.value = true;
  try {
    await adminApi.grantAdminPro({
      email: grantForm.email.trim(),
      planId: grantForm.planId,
      durationDays: Number(grantForm.durationDays),
    });
    ui.showToast(`Đã kích hoạt quyền Pro cho ${grantForm.email} thành công!`, 'success');
    grantModalOpen.value = false;
    await loadSubscriptions();
  } catch (err: any) {
    ui.showToast(err.response?.data?.message || err.message || 'Không thể cấp quyền Pro.', 'error');
  } finally {
    granting.value = false;
  }
}

async function handleRevokePro(sub: AdminSubscriptionDto): Promise<void> {
  const confirmed = window.confirm(`Bạn có chắc chắn muốn hủy kích hoạt gói Pro của ${sub.userEmail}?`);
  if (!confirmed) return;

  try {
    await adminApi.revokeAdminPro(sub.id);
    ui.showToast(`Đã thu hồi gói Pro của ${sub.userEmail} thành công!`, 'success');
    await loadSubscriptions();
  } catch (err: any) {
    ui.showToast(err.response?.data?.message || err.message || 'Không thể thu hồi gói Pro.', 'error');
  }
}

function planLabel(planId: string | null): string {
  if (!planId) return 'Gói Pro Chuẩn';
  if (planId.includes('yearly')) return 'Gói Pro Năm (799k)';
  if (planId.includes('grant') || planId.includes('manual')) return 'Admin Cấp Thủ Công';
  return 'Gói Pro Tháng (99k)';
}

function formatDate(dateStr: string | null, isActive?: boolean, status?: number): string {
  if (status === 2) return 'Hủy kích hoạt';
  if (!dateStr) return 'Vĩnh viễn';
  const iso = dateStr.includes('Z') || dateStr.includes('+') ? dateStr : dateStr + 'Z';
  const d = new Date(iso);
  return d.toLocaleString('vi-VN', {
    timeZone: 'Asia/Ho_Chi_Minh',
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}
</script>

<template>
  <StudioShell active-tab="transactions">
    <div class="space-y-6">
      <!-- Header -->
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 p-6 rounded-3xl bg-vdsa-surface border border-vdsa-border">
        <div>
          <h1 class="text-xl md:text-2xl font-black text-white flex items-center gap-2.5">
            <span class="p-2 rounded-2xl bg-amber-500/20 text-amber-400 border border-amber-500/30">
              <CreditCard :size="22" />
            </span>
            Giao dịch & Quản lý Gói Pro
          </h1>
          <p class="text-xs text-vdsa-muted mt-1">
            Theo dõi danh sách đăng ký Premium, đơn hàng VietQR và cấp quyền Pro thủ công
          </p>
        </div>

        <div class="flex items-center gap-2.5">
          <Button variant="ghost" size="sm" @click="loadSubscriptions" :loading="loading">
            <RefreshCw :size="14" /> Làm mới
          </Button>
          <Button variant="primary" size="sm" @click="openGrantModal">
            <Crown :size="14" class="text-amber-300" /> Kích hoạt Pro thủ công
          </Button>
        </div>
      </div>

      <!-- Stats Overview -->
      <div class="grid grid-cols-2 lg:grid-cols-4 gap-3 md:gap-4">
        <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-purple-500/15 text-purple-400 flex items-center justify-center font-bold">
            <CreditCard :size="18" />
          </div>
          <div>
            <span class="text-[11px] text-vdsa-muted uppercase font-bold tracking-wider">Tổng đơn đăng ký</span>
            <p class="text-lg font-black text-white">{{ subscriptions.length }}</p>
          </div>
        </div>

        <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-emerald-500/15 text-emerald-400 flex items-center justify-center font-bold">
            <CheckCircle2 :size="18" />
          </div>
          <div>
            <span class="text-[11px] text-vdsa-muted uppercase font-bold tracking-wider">Đang kích hoạt</span>
            <p class="text-lg font-black text-emerald-400">{{ activeCount }}</p>
          </div>
        </div>

        <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-rose-500/15 text-rose-400 flex items-center justify-center font-bold">
            <Clock :size="18" />
          </div>
          <div>
            <span class="text-[11px] text-vdsa-muted uppercase font-bold tracking-wider">Đã hết hạn</span>
            <p class="text-lg font-black text-rose-400">{{ expiredCount }}</p>
          </div>
        </div>

        <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border flex items-center gap-3">
          <div class="w-10 h-10 rounded-xl bg-amber-500/15 text-amber-400 flex items-center justify-center font-bold">
            <Crown :size="18" />
          </div>
          <div>
            <span class="text-[11px] text-vdsa-muted uppercase font-bold tracking-wider">Đặc quyền</span>
            <p class="text-xs font-bold text-slate-300">Không giới hạn Tim + AI</p>
          </div>
        </div>
      </div>

      <!-- Filters & Search -->
      <div class="flex flex-col sm:flex-row items-center justify-between gap-3">
        <div class="relative flex-1 w-full sm:max-w-xs">
          <Search :size="14" class="absolute left-3 top-1/2 -translate-y-1/2 text-slate-500" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm theo email, tên, mã đơn..."
            class="w-full pl-9 pr-3 py-2 text-xs bg-vdsa-surface border border-vdsa-border rounded-xl text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
          />
        </div>

        <div class="flex items-center gap-2 w-full sm:w-auto flex-wrap">
          <button
            type="button"
            class="px-3 py-1.5 rounded-lg text-xs font-bold transition-all cursor-pointer"
            :class="filterStatus === 'all' ? 'bg-purple-600 text-white' : 'bg-vdsa-surface text-slate-400 hover:text-white'"
            @click="filterStatus = 'all'"
          >
            Tất cả ({{ subscriptions.length }})
          </button>
          <button
            type="button"
            class="px-3 py-1.5 rounded-lg text-xs font-bold transition-all cursor-pointer"
            :class="filterStatus === 'active' ? 'bg-emerald-600 text-white' : 'bg-vdsa-surface text-slate-400 hover:text-white'"
            @click="filterStatus = 'active'"
          >
            Đang hoạt động ({{ activeCount }})
          </button>
          <button
            type="button"
            class="px-3 py-1.5 rounded-lg text-xs font-bold transition-all cursor-pointer"
            :class="filterStatus === 'pending' ? 'bg-amber-600 text-white' : 'bg-vdsa-surface text-slate-400 hover:text-white'"
            @click="filterStatus = 'pending'"
          >
            Hủy kích hoạt ({{ pendingCount }})
          </button>
          <button
            type="button"
            class="px-3 py-1.5 rounded-lg text-xs font-bold transition-all cursor-pointer"
            :class="filterStatus === 'expired' ? 'bg-rose-600 text-white' : 'bg-vdsa-surface text-slate-400 hover:text-white'"
            @click="filterStatus = 'expired'"
          >
            Đã hết hạn ({{ expiredCount }})
          </button>
        </div>
      </div>

      <!-- Table -->
      <div class="overflow-x-auto rounded-2xl border border-vdsa-border bg-vdsa-surface">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-vdsa-bg-secondary border-b border-vdsa-border text-[11px] uppercase font-bold text-vdsa-muted">
              <th class="p-3">Mã đơn / OrderRef</th>
              <th class="p-3">Người dùng</th>
              <th class="p-3">Gói dịch vụ</th>
              <th class="p-3">Ngày bắt đầu</th>
              <th class="p-3">Hạn sử dụng</th>
              <th class="p-3">Trạng thái</th>
              <th class="p-3 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-vdsa-border text-xs">
            <tr v-for="sub in filteredSubs" :key="sub.id" class="hover:bg-white/5 transition-colors">
              <td class="p-3 font-mono">
                <span class="text-amber-400 font-bold mr-1">#{{ sub.id }}</span>
                <span class="text-slate-300 font-medium">{{ sub.orderRef || 'MANUAL' }}</span>
              </td>
              <td class="p-3">
                <p class="font-bold text-white flex items-center gap-1.5">
                  <User :size="13" class="text-purple-400" />
                  {{ sub.userDisplayName }}
                </p>
                <p class="text-[11px] text-slate-400">{{ sub.userEmail }}</p>
              </td>
              <td class="p-3">
                <Badge variant="primary" class="font-medium text-[11px]">{{ planLabel(sub.planId) }}</Badge>
              </td>
              <td class="p-3 text-slate-300 font-mono text-[11px]">
                {{ formatDate(sub.startedAt, sub.isActive, sub.status) }}
              </td>
              <td class="p-3 font-mono text-[11px]" :class="sub.isActive ? 'text-emerald-400 font-semibold' : 'text-slate-500'">
                {{ formatDate(sub.expiresAt, sub.isActive, sub.status) }}
              </td>
              <td class="p-3">
                <Badge :variant="getSubBadgeVariant(sub)">
                  {{ getSubBadgeLabel(sub) }}
                </Badge>
              </td>
              <td class="p-3 text-right">
                <button
                  v-if="isSubscriptionActive(sub)"
                  type="button"
                  class="px-2.5 py-1 rounded-lg bg-rose-500/10 hover:bg-rose-500/20 text-rose-400 hover:text-rose-300 border border-rose-500/30 text-[11px] font-bold transition-colors cursor-pointer"
                  title="Thu hồi / Hủy quyền Pro thủ công"
                  @click="handleRevokePro(sub)"
                >
                  Hủy Pro
                </button>
                <span v-else class="text-slate-600 text-[11px]">—</span>
              </td>
            </tr>
            <tr v-if="filteredSubs.length === 0">
              <td colspan="7" class="p-8 text-center text-xs text-slate-500">
                Không tìm thấy đơn đăng ký Pro nào phù hợp.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Modal Cấp quyền Pro thủ công -->
    <Modal :open="grantModalOpen" title="Kích hoạt Quyền Pro Thủ công" :is-dirty="Boolean(grantForm.email)" @close="grantModalOpen = false">
      <form class="space-y-4 pt-1" novalidate @submit.prevent="handleGrantPro">
        <div>
          <label class="block text-[11px] font-bold text-slate-300 uppercase mb-1">Email người dùng *</label>
          <input
            v-model="grantForm.email"
            type="email"
            placeholder="nhập email sinh viên, ví dụ: student@demo.local"
            class="w-full px-3 py-2 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
            required
          />
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <div>
            <label class="block text-[11px] font-bold text-slate-300 uppercase mb-1">Gói cước *</label>
            <select
              v-model="grantForm.planId"
              class="w-full px-3 py-2 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white focus:outline-none focus:border-purple-500"
            >
              <option value="pro-monthly">Gói Pro Tháng (99k)</option>
              <option value="pro-yearly">Gói Pro Năm (799k)</option>
              <option value="pro-admin-grant">Admin Cấp Trực Tiếp</option>
            </select>
          </div>

          <div>
            <label class="block text-[11px] font-bold text-slate-300 uppercase mb-1">Thời hạn cấp (Số ngày) *</label>
            <select
              v-model="grantForm.durationDays"
              class="w-full px-3 py-2 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white focus:outline-none focus:border-purple-500"
            >
              <option :value="30">30 Ngày (1 Tháng)</option>
              <option :value="90">90 Ngày (3 Tháng)</option>
              <option :value="365">365 Ngày (1 Năm)</option>
              <option :value="3650">3650 Ngày (Trọn đời)</option>
            </select>
          </div>
        </div>

        <div class="flex items-center justify-end gap-2 pt-3 border-t border-[#2e2c44]">
          <Button variant="ghost" @click="grantModalOpen = false">Hủy</Button>
          <Button type="submit" :loading="granting">Xác nhận Kích hoạt Pro</Button>
        </div>
      </form>
    </Modal>
  </StudioShell>
</template>
