<script setup lang="ts">
import { onMounted, reactive, ref, computed } from 'vue';
import {
  AlertTriangle,
  Bug,
  CheckCircle2,
  Filter,
  Inbox,
  RefreshCw,
  Search,
  Send,
  ShieldAlert,
  Sparkles,
  MessageSquare,
  Sliders,
  Save,
  Lock,
  Globe,
  Cpu,
} from 'lucide-vue-next';

import { courseApi, type CourseFeedbackDto } from '@/services/courseApi';
import * as adminApi from '@/api/admin';
import type { SystemSettingsDto } from '@/api/types';
import { useUiStore } from '@/stores/ui';
import AdminNav from '@/components/admin/AdminNav.vue';
import Button from '@/components/ui/Button.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import Badge from '@/components/ui/Badge.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const ui = useUiStore();

// ── Tab chính ──
type SettingTab = 'system' | 'reports';
const activeTab = ref<SettingTab>('system');

// ── TAB 1: CẤU HÌNH HỆ THỐNG ──
const settingsLoading = ref(true);
const settingsSaving = ref(false);
const settingsForm = reactive<SystemSettingsDto>({
  siteName: 'DSA-Visual',
  allowedDomains: [],
  passwordPolicy: {
    minLength: 8,
    requireUppercase: true,
    requireDigit: true,
    requireSpecial: true,
  },
  uploadMaxMb: 10,
  sandboxSeconds: 5,
  sandboxMemoryMb: 128,
});
const allowedDomainsInput = ref('');

async function loadSettings(): Promise<void> {
  settingsLoading.value = true;
  try {
    const data = await adminApi.fetchSettings();
    if (data) {
      settingsForm.siteName = data.siteName ?? 'DSA-Visual';
      settingsForm.allowedDomains = Array.isArray(data.allowedDomains) ? data.allowedDomains : [];
      allowedDomainsInput.value = settingsForm.allowedDomains.join(', ');
      if (data.passwordPolicy) {
        settingsForm.passwordPolicy = {
          minLength: data.passwordPolicy.minLength ?? 8,
          requireUppercase: Boolean(data.passwordPolicy.requireUppercase),
          requireDigit: Boolean(data.passwordPolicy.requireDigit),
          requireSpecial: Boolean(data.passwordPolicy.requireSpecial),
        };
      }
      settingsForm.uploadMaxMb = data.uploadMaxMb ?? 10;
      settingsForm.sandboxSeconds = data.sandboxSeconds ?? 5;
      settingsForm.sandboxMemoryMb = data.sandboxMemoryMb ?? 128;
    }
  } catch (err) {
    ui.showToast('Không thể tải cấu hình hệ thống.', 'error');
  } finally {
    settingsLoading.value = false;
  }
}

async function handleSaveSettings(): Promise<void> {
  settingsSaving.value = true;
  try {
    const domains = allowedDomainsInput.value
      .split(',')
      .map((d) => d.trim())
      .filter(Boolean);
    settingsForm.allowedDomains = domains;

    const updated = await adminApi.updateSettings(settingsForm);
    if (updated) {
      settingsForm.siteName = updated.siteName;
      settingsForm.allowedDomains = updated.allowedDomains;
      allowedDomainsInput.value = updated.allowedDomains.join(', ');
      settingsForm.passwordPolicy = updated.passwordPolicy;
      settingsForm.uploadMaxMb = updated.uploadMaxMb;
      settingsForm.sandboxSeconds = updated.sandboxSeconds;
      settingsForm.sandboxMemoryMb = updated.sandboxMemoryMb;
    }
    ui.showToast('Đã lưu cấu hình hệ thống thành công!', 'success');
  } catch (err: any) {
    ui.showToast(err?.message || 'Lưu cấu hình thất bại.', 'error');
  } finally {
    settingsSaving.value = false;
  }
}

// ── TAB 2: BÁO CÁO LỖI & PHẢN HỒI ──
const reports = ref<CourseFeedbackDto[]>([]);
const reportsLoading = ref(false);
const reportsError = ref('');
const statusFilter = ref<string>('');
const typeFilter = ref<string>('');
const searchQuery = ref<string>('');

const replyTexts = reactive<Record<number, string>>({});
const replyStatuses = reactive<Record<number, string>>({});
const savingReportId = ref<number | null>(null);

const STATUS_OPTIONS = [
  { value: '', label: 'Tất cả trạng thái' },
  { value: 'New', label: 'Chưa xử lý' },
  { value: 'Read', label: 'Đang xử lý' },
  { value: 'Resolved', label: 'Đã giải quyết' },
];

const TYPE_OPTIONS = [
  { value: '', label: 'Tất cả loại' },
  { value: 'Bug', label: 'Báo lỗi hệ thống' },
  { value: 'Suggestion', label: 'Đóng góp ý kiến' },
  { value: 'Request', label: 'Yêu cầu hỗ trợ' },
];

const pendingCount = computed(() => reports.value.filter((r) => r.status === 'New').length);
const processingCount = computed(() => reports.value.filter((r) => r.status === 'Read').length);
const resolvedCount = computed(() => reports.value.filter((r) => r.status === 'Resolved').length);

const filteredReports = computed(() => {
  return reports.value.filter((r) => {
    if (statusFilter.value && r.status !== statusFilter.value) return false;
    if (typeFilter.value && r.type !== typeFilter.value) return false;
    if (searchQuery.value.trim()) {
      const q = searchQuery.value.toLowerCase().trim();
      const matchContent = r.content?.toLowerCase().includes(q);
      const matchUser = r.userName?.toLowerCase().includes(q);
      const matchCourse = r.courseTitle?.toLowerCase().includes(q);
      if (!matchContent && !matchUser && !matchCourse) return false;
    }
    return true;
  });
});

async function loadReports(): Promise<void> {
  reportsLoading.value = true;
  reportsError.value = '';
  try {
    reports.value = await courseApi.getTeacherFeedback();
  } catch (err) {
    reportsError.value = 'Không thể tải danh sách báo cáo lỗi & vi phạm từ máy chủ.';
  } finally {
    reportsLoading.value = false;
  }
}

async function handleReply(report: CourseFeedbackDto): Promise<void> {
  const text = replyTexts[report.id]?.trim();
  const nextStatus = replyStatuses[report.id] || (text ? 'Resolved' : report.status);

  savingReportId.value = report.id;
  try {
    const updated = await courseApi.replyCourseFeedback(report.id, {
      replyText: text || undefined,
      status: nextStatus,
    });
    const idx = reports.value.findIndex((r) => r.id === report.id);
    if (idx !== -1) {
      reports.value[idx] = updated;
    }
    replyTexts[report.id] = '';
    ui.showToast('Đã cập nhật trạng thái báo cáo & gửi phản hồi thành công!', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Cập nhật thất bại.', 'error');
  } finally {
    savingReportId.value = null;
  }
}

function formatDate(iso: string | null): string {
  if (!iso) return '—';
  try {
    return new Date(iso).toLocaleString('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    });
  } catch {
    return iso;
  }
}

function getTypeBadgeVariant(type: string): 'danger' | 'warning' | 'primary' | 'muted' {
  switch (type) {
    case 'Bug':
      return 'danger';
    case 'Suggestion':
      return 'primary';
    case 'Request':
      return 'warning';
    default:
      return 'muted';
  }
}

function getTypeLabel(type: string): string {
  switch (type) {
    case 'Bug':
      return 'Báo lỗi';
    case 'Suggestion':
      return 'Góp ý';
    case 'Request':
      return 'Yêu cầu';
    default:
      return type;
  }
}

function getStatusBadgeVariant(status: string): 'danger' | 'warning' | 'success' | 'muted' {
  switch (status) {
    case 'New':
      return 'danger';
    case 'Read':
      return 'warning';
    case 'Resolved':
      return 'success';
    default:
      return 'muted';
  }
}

function getStatusLabel(status: string): string {
  switch (status) {
    case 'New':
      return 'Chưa xử lý';
    case 'Read':
      return 'Đang xử lý';
    case 'Resolved':
      return 'Đã giải quyết';
    default:
      return status;
  }
}

onMounted(() => {
  void loadSettings();
  void loadReports();
});
</script>

<template>
  <div class="admin-settings-page container">
    <AdminNav active="settings" class="mb-6" />

    <!-- Hero Header -->
    <header class="page-hero">
      <div class="page-hero__body">
        <div class="page-hero__icon">
          <Sliders :size="24" />
        </div>
        <div>
          <h1 class="page-hero__title">Cấu hình & Quản trị Hệ thống</h1>
          <p class="page-hero__sub">
            Quản lý tham số vận hành, chính sách bảo mật, sandbox và xử lý phản hồi từ người dùng
          </p>
        </div>
      </div>
    </header>

    <!-- Navigation Tabs -->
    <div class="tabs-nav mb-6">
      <button
        type="button"
        class="tab-btn"
        :class="{ 'tab-btn--active': activeTab === 'system' }"
        @click="activeTab = 'system'"
      >
        <Sliders :size="16" />
        <span>Cấu hình Hệ thống</span>
      </button>
      <button
        type="button"
        class="tab-btn"
        :class="{ 'tab-btn--active': activeTab === 'reports' }"
        @click="activeTab = 'reports'"
      >
        <MessageSquare :size="16" />
        <span>Báo cáo & Ý kiến</span>
        <Badge v-if="pendingCount > 0" variant="danger" size="sm" class="ml-1">
          {{ pendingCount }}
        </Badge>
      </button>
    </div>

    <!-- TAB 1: CẤU HÌNH HỆ THỐNG -->
    <section v-if="activeTab === 'system'" class="settings-section space-y-6">
      <div v-if="settingsLoading" class="space-y-4">
        <Skeleton v-for="i in 4" :key="i" height="80px" />
      </div>

      <form v-else @submit.prevent="handleSaveSettings" class="space-y-6">
        <!-- 1. Thông tin chung -->
        <div class="settings-card card">
          <div class="settings-card__header">
            <Globe :size="20" class="text-primary-400" />
            <div>
              <h2 class="settings-card__title">Thông tin chung</h2>
              <p class="settings-card__desc">Tên website và quy định tên miền email được phép đăng ký</p>
            </div>
          </div>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 mt-4">
            <div>
              <label class="form-label" for="site-name">Tên hệ thống (Site Name)</label>
              <input
                id="site-name"
                v-model="settingsForm.siteName"
                type="text"
                class="form-input"
                placeholder="VD: DSA-Visual"
                required
              />
            </div>
            <div>
              <label class="form-label" for="allowed-domains">Domain email cho phép (phân tách dấu phẩy)</label>
              <input
                id="allowed-domains"
                v-model="allowedDomainsInput"
                type="text"
                class="form-input"
                placeholder="VD: fpt.edu.vn, university.edu.vn (để trống = tất cả)"
              />
              <p class="text-xs text-slate-400 mt-1">Để trống nếu cho phép đăng ký với mọi email.</p>
            </div>
          </div>
        </div>

        <!-- 2. Chính sách Mật khẩu -->
        <div class="settings-card card">
          <div class="settings-card__header">
            <Lock :size="20" class="text-emerald-400" />
            <div>
              <h2 class="settings-card__title">Chính sách Mật khẩu & Bảo mật</h2>
              <p class="settings-card__desc">Quy định độ phức tạp của mật khẩu khi người dùng đăng ký hoặc đổi mật khẩu</p>
            </div>
          </div>
          <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mt-4">
            <div>
              <label class="form-label" for="min-pwd-length">Độ dài tối thiểu (ký tự)</label>
              <input
                id="min-pwd-length"
                v-model.number="settingsForm.passwordPolicy.minLength"
                type="number"
                min="6"
                max="32"
                class="form-input"
                required
              />
            </div>
            <div class="flex items-center gap-2 pt-6">
              <input
                id="pwd-upper"
                v-model="settingsForm.passwordPolicy.requireUppercase"
                type="checkbox"
                class="form-checkbox"
              />
              <label for="pwd-upper" class="text-sm font-medium text-slate-200 cursor-pointer">Bắt buộc chữ hoa (A-Z)</label>
            </div>
            <div class="flex items-center gap-2 pt-6">
              <input
                id="pwd-digit"
                v-model="settingsForm.passwordPolicy.requireDigit"
                type="checkbox"
                class="form-checkbox"
              />
              <label for="pwd-digit" class="text-sm font-medium text-slate-200 cursor-pointer">Bắt buộc chữ số (0-9)</label>
            </div>
            <div class="flex items-center gap-2 pt-6">
              <input
                id="pwd-special"
                v-model="settingsForm.passwordPolicy.requireSpecial"
                type="checkbox"
                class="form-checkbox"
              />
              <label for="pwd-special" class="text-sm font-medium text-slate-200 cursor-pointer">Bắt buộc ký tự đặc biệt</label>
            </div>
          </div>
        </div>

        <!-- 3. Giới hạn Sandbox & Lưu trữ -->
        <div class="settings-card card">
          <div class="settings-card__header">
            <Cpu :size="20" class="text-amber-400" />
            <div>
              <h2 class="settings-card__title">Giới hạn Sandbox & Lưu trữ</h2>
              <p class="settings-card__desc">Cấu hình tài nguyên thực thi code client-side và giới hạn file tải lên</p>
            </div>
          </div>
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-4 mt-4">
            <div>
              <label class="form-label" for="sandbox-seconds">Thời gian timeout Sandbox (giây)</label>
              <input
                id="sandbox-seconds"
                v-model.number="settingsForm.sandboxSeconds"
                type="number"
                min="1"
                max="30"
                class="form-input"
                required
              />
            </div>
            <div>
              <label class="form-label" for="sandbox-mem">Bộ nhớ RAM tối đa Sandbox (MB)</label>
              <input
                id="sandbox-mem"
                v-model.number="settingsForm.sandboxMemoryMb"
                type="number"
                min="16"
                max="1024"
                class="form-input"
                required
              />
            </div>
            <div>
              <label class="form-label" for="upload-max">Dung lượng upload tối đa (MB)</label>
              <input
                id="upload-max"
                v-model.number="settingsForm.uploadMaxMb"
                type="number"
                min="1"
                max="100"
                class="form-input"
                required
              />
            </div>
          </div>
        </div>

        <!-- Action bar -->
        <div class="flex items-center justify-end gap-3 pt-2">
          <Button type="button" variant="secondary" @click="loadSettings">
            <RefreshCw :size="16" /> Đặt lại
          </Button>
          <Button type="submit" variant="primary" :loading="settingsSaving">
            <Save :size="16" /> Lưu cấu hình hệ thống
          </Button>
        </div>
      </form>
    </section>

    <!-- TAB 2: BÁO CÁO LỖI & Ý KIẾN HỌC VIÊN -->
    <section v-else-if="activeTab === 'reports'" class="space-y-6">
      <!-- Stats Overview -->
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div class="stat-box card">
          <span class="stat-box__label">Chưa xử lý</span>
          <span class="stat-box__value text-rose-400 font-mono">{{ pendingCount }}</span>
        </div>
        <div class="stat-box card">
          <span class="stat-box__label">Đang xử lý</span>
          <span class="stat-box__value text-amber-400 font-mono">{{ processingCount }}</span>
        </div>
        <div class="stat-box card">
          <span class="stat-box__label">Đã giải quyết</span>
          <span class="stat-box__value text-emerald-400 font-mono">{{ resolvedCount }}</span>
        </div>
      </div>

      <!-- Filters & Search -->
      <div class="filter-bar card">
        <div class="filter-bar__search">
          <Search :size="16" class="text-slate-400" />
          <input
            v-model="searchQuery"
            type="text"
            placeholder="Tìm theo nội dung, học viên, bài học..."
            class="filter-bar__input"
          />
        </div>
        <div class="filter-bar__controls">
          <select v-model="statusFilter" class="form-select">
            <option v-for="opt in STATUS_OPTIONS" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
          <select v-model="typeFilter" class="form-select">
            <option v-for="opt in TYPE_OPTIONS" :key="opt.value" :value="opt.value">
              {{ opt.label }}
            </option>
          </select>
          <Button variant="ghost" size="sm" @click="loadReports">
            <RefreshCw :size="15" />
          </Button>
        </div>
      </div>

      <!-- Reports List -->
      <div v-if="reportsLoading" class="space-y-4">
        <Skeleton v-for="i in 3" :key="i" height="120px" />
      </div>

      <EmptyState
        v-else-if="reportsError"
        icon="alert-circle"
        title="Lỗi tải dữ liệu"
        :description="reportsError"
        action-label="Thử lại"
        @action="loadReports"
      />

      <EmptyState
        v-else-if="filteredReports.length === 0"
        icon="inbox"
        title="Không có báo cáo nào"
        description="Hiện tại không có báo cáo lỗi hoặc đóng góp ý kiến nào khớp với bộ lọc."
      />

      <div v-else class="space-y-4">
        <article
          v-for="item in filteredReports"
          :key="item.id"
          class="report-card card"
          :class="`report-card--${item.status.toLowerCase()}`"
        >
          <div class="report-card__top">
            <div class="flex items-center gap-2">
              <Badge :variant="getTypeBadgeVariant(item.type)">
                {{ getTypeLabel(item.type) }}
              </Badge>
              <Badge :variant="getStatusBadgeVariant(item.status)">
                {{ getStatusLabel(item.status) }}
              </Badge>
              <span class="text-xs text-slate-400 font-mono">{{ formatDate(item.createdAt) }}</span>
            </div>
            <div class="text-xs text-slate-400">
              Học viên: <strong class="text-slate-200">{{ item.userName }}</strong>
              <span v-if="item.courseTitle"> · Khóa: <em class="text-slate-300">{{ item.courseTitle }}</em></span>
            </div>
          </div>

          <div class="report-card__content">
            <p class="text-slate-200 whitespace-pre-wrap">{{ item.content }}</p>
          </div>

          <div v-if="item.replyText" class="report-card__reply">
            <div class="text-xs text-primary-400 font-semibold mb-1">Phản hồi từ quản trị viên:</div>
            <p class="text-slate-300 text-sm whitespace-pre-wrap">{{ item.replyText }}</p>
          </div>

          <!-- Reply form -->
          <div class="report-card__actions pt-3 border-t border-slate-700/50 flex flex-col sm:flex-row gap-2">
            <input
              v-model="replyTexts[item.id]"
              type="text"
              class="form-input flex-1 text-sm"
              :placeholder="item.replyText ? 'Cập nhật nội dung phản hồi...' : 'Nhập phản hồi gửi học viên...'"
            />
            <select v-model="replyStatuses[item.id]" class="form-select text-sm w-auto">
              <option value="New">Chưa xử lý</option>
              <option value="Read">Đang xử lý</option>
              <option value="Resolved">Đã giải quyết</option>
            </select>
            <Button
              size="sm"
              variant="primary"
              :loading="savingReportId === item.id"
              @click="handleReply(item)"
            >
              <Send :size="14" /> Gửi
            </Button>
          </div>
        </article>
      </div>
    </section>
  </div>
</template>

<style scoped>
.admin-settings-page {
  padding-block: var(--space-lg) var(--space-2xl);
}

.page-hero {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: var(--space-lg);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  margin-bottom: var(--space-lg);
}

.page-hero__body {
  display: flex;
  align-items: center;
  gap: var(--space-md);
}

.page-hero__icon {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: var(--radius-md);
  background: rgba(99, 102, 241, 0.15);
  color: var(--primary-400);
}

.page-hero__title {
  font-size: var(--text-xl);
  font-weight: 700;
  color: var(--text-base);
  margin: 0;
}

.page-hero__sub {
  font-size: var(--text-sm);
  color: var(--text-muted);
  margin: 0;
}

.tabs-nav {
  display: flex;
  gap: var(--space-xs);
  border-bottom: 1px solid var(--border);
  padding-bottom: 2px;
}

.tab-btn {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--text-muted);
  background: transparent;
  border: none;
  border-bottom: 2px solid transparent;
  cursor: pointer;
  transition: all 0.15s ease;
}

.tab-btn:hover {
  color: var(--text-base);
}

.tab-btn--active {
  color: var(--primary-400);
  border-bottom-color: var(--primary-400);
}

.settings-card {
  padding: var(--space-lg);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
}

.settings-card__header {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  margin-bottom: var(--space-md);
}

.settings-card__title {
  font-size: var(--text-base);
  font-weight: 600;
  color: var(--text-base);
  margin: 0;
}

.settings-card__desc {
  font-size: var(--text-xs);
  color: var(--text-muted);
  margin: 0;
}

.form-label {
  display: block;
  font-size: var(--text-xs);
  font-weight: 500;
  color: var(--text-muted);
  margin-bottom: var(--space-xs);
}

.form-input {
  width: 100%;
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-sm);
  color: var(--text-base);
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  outline: none;
  transition: border-color 0.15s ease;
}

.form-input:focus {
  border-color: var(--primary-400);
}

.form-checkbox {
  width: 18px;
  height: 18px;
  accent-color: var(--primary-500);
  cursor: pointer;
}

.stat-box {
  padding: var(--space-md);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-box__label {
  font-size: var(--text-xs);
  color: var(--text-muted);
}

.stat-box__value {
  font-size: var(--text-2xl);
  font-weight: 700;
}

.filter-bar {
  padding: var(--space-md);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-md);
  align-items: center;
  justify-content: space-between;
}

.filter-bar__search {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  flex: 1;
  min-width: 240px;
}

.filter-bar__input {
  width: 100%;
  background: transparent;
  border: none;
  outline: none;
  font-size: var(--text-sm);
  color: var(--text-base);
}

.filter-bar__controls {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.form-select {
  padding: 6px 10px;
  font-size: var(--text-xs);
  color: var(--text-base);
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  outline: none;
}

.report-card {
  padding: var(--space-md);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.report-card__top {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-xs);
}

.report-card__reply {
  padding: var(--space-sm) var(--space-md);
  background: rgba(99, 102, 241, 0.08);
  border-left: 3px solid var(--primary-400);
  border-radius: var(--radius-sm);
}
</style>
