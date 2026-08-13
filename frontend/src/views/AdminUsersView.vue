<script setup lang="ts">
// AdminUsersView — Màn 10: quản lý người dùng + tab "Chờ duyệt Teacher" (Màn 29)
// H-B: hero gradient Aurora soft + Tabs shadcn (badge chờ duyệt) + filter bar card
// + table hover/avatar + Badge trạng thái + modal duyệt giữ nguyên logic.
import { computed, onMounted, ref } from 'vue';
import { Check, Lock, LockOpen, Search, UserCheck, Users, X } from 'lucide-vue-next';

import * as adminApi from '@/api/admin';
import type { AdminUserDto } from '@/api/admin';
import { useUiStore } from '@/stores/ui';
import { formatDate } from '@/utils/format';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';
import Tabs from '@/components/ui/Tabs.vue';
import AdminNav from '@/components/admin/AdminNav.vue';

const ui = useUiStore();

const tab = ref<'all' | 'pending'>('all');
const users = ref<AdminUserDto[]>([]);
const loading = ref(true);
const search = ref('');
const roleFilter = ref('');
const statusFilter = ref('');

// Modal duyệt/từ chối teacher
const reviewTarget = ref<AdminUserDto | null>(null);
const reviewAction = ref<'approve' | 'reject'>('approve');
const rejectReason = ref('');

// Task L — hiển thị thông tin đăng ký GV trong modal (chỉ khi có giá trị)
const hasReviewInfo = computed(() => {
  const u = reviewTarget.value;
  return Boolean(u && (u.department || u.staffCode || u.teacherBio));
});

onMounted(load);

async function load(): Promise<void> {
  loading.value = true;
  try {
    const page = await adminApi.fetchUsers({ role: tab.value === 'pending' ? 'TEACHER_PENDING' : undefined, q: search.value || undefined, page: 1 });
    users.value = page.items;
  } catch {
    ui.showToast('Không thể tải danh sách người dùng.', 'error');
    users.value = [];
  } finally {
    loading.value = false;
  }
}

function switchTab(next: string): void {
  tab.value = next as 'all' | 'pending';
  void load();
}

const pendingCount = computed(() => users.value.filter((u) => u.role === 'TEACHER_PENDING').length);

const userTabs = computed(() => [
  { key: 'all', label: messages.admin.users.tabAll },
  { key: 'pending', label: messages.admin.users.tabPending, badge: pendingCount.value > 0 ? pendingCount.value : undefined },
]);

const filtered = computed(() => {
  let list = users.value;
  if (roleFilter.value) list = list.filter((u) => u.role === roleFilter.value);
  if (statusFilter.value === 'active') list = list.filter((u) => u.isActive);
  if (statusFilter.value === 'locked') list = list.filter((u) => !u.isActive);
  return list;
});

const roleLabel: Record<string, string> = {
  STUDENT: messages.admin.users.roleStudent,
  TEACHER: messages.admin.users.roleTeacher,
  TEACHER_PENDING: messages.admin.users.rolePending,
  ADMIN: messages.admin.users.roleAdmin,
};

const initial = (name: string): string => (name.trim() ? name.trim().charAt(0).toUpperCase() : '?');

async function toggleLock(user: AdminUserDto): Promise<void> {
  try {
    await adminApi.setUserStatus(user.id, { isActive: !user.isActive });
    user.isActive = !user.isActive;
    ui.showToast(user.isActive ? 'Đã mở khóa tài khoản.' : 'Đã khóa tài khoản.', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Thao tác thất bại.', 'error');
  }
}

function openReview(user: AdminUserDto, action: 'approve' | 'reject'): void {
  reviewTarget.value = user;
  reviewAction.value = action;
  rejectReason.value = '';
}

async function submitReview(): Promise<void> {
  if (!reviewTarget.value) return;
  try {
    await adminApi.approveTeacher(reviewTarget.value.id, {
      approve: reviewAction.value === 'approve',
      reason: reviewAction.value === 'reject' ? rejectReason.value : undefined,
    });
    ui.showToast(reviewAction.value === 'approve' ? 'Đã duyệt giảng viên!' : 'Đã từ chối — tài khoản về vai học viên.', 'success');
    reviewTarget.value = null;
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Thao tác thất bại.', 'error');
  }
}
</script>

<template>
  <main class="admin-users container">
    <!-- Hero gradient Aurora soft (palette 1 — gamification/admin) -->
    <header class="admin-users__hero">
      <div class="admin-users__hero-body">
        <span class="admin-users__hero-icon" aria-hidden="true"><Users :size="24" /></span>
        <div class="admin-users__hero-title-wrap">
          <h1 class="admin-users__title">{{ messages.admin.users.title }}</h1>
          <p class="admin-users__sub">{{ messages.admin.users.subtitle }}</p>
        </div>
        <Badge variant="primary" class="admin-users__hero-badge">{{ messages.admin.badge }}</Badge>
      </div>
    </header>

    <AdminNav active="users" />

    <!-- Tabs shadcn: Tất cả / Chờ duyệt Teacher (badge = số chờ duyệt) -->
    <Tabs :tabs="userTabs" :model-value="tab" @change="switchTab" />

    <div class="admin-users__filters card">
      <div class="admin-users__search-box">
        <Search :size="15" class="admin-users__search-icon" aria-hidden="true" />
        <input
          v-model="search"
          class="input admin-users__search"
          type="search"
          :placeholder="messages.admin.users.searchPlaceholder"
          :aria-label="messages.admin.users.searchLabel"
          @keyup.enter="load"
        />
      </div>
      <select v-model="roleFilter" class="input admin-users__select" :aria-label="messages.admin.users.roleFilterLabel">
        <option value="">{{ messages.admin.users.roleAll }}</option>
        <option value="STUDENT">{{ messages.admin.users.roleStudent }}</option>
        <option value="TEACHER">{{ messages.admin.users.roleTeacher }}</option>
        <option value="TEACHER_PENDING">{{ messages.admin.users.rolePending }}</option>
        <option value="ADMIN">{{ messages.admin.users.roleAdmin }}</option>
      </select>
      <select v-model="statusFilter" class="input admin-users__select" :aria-label="messages.admin.users.statusFilterLabel">
        <option value="">{{ messages.admin.users.statusAll }}</option>
        <option value="active">{{ messages.admin.users.statusActive }}</option>
        <option value="locked">{{ messages.admin.users.statusLocked }}</option>
      </select>
      <Button size="sm" variant="secondary" class="admin-users__search-btn" @click="load">
        <Search :size="14" /> {{ messages.admin.users.search }}
      </Button>
    </div>

    <div v-if="loading" class="admin-users__loading" aria-busy="true">
      <Skeleton v-for="i in 6" :key="i" height="56px" />
    </div>

    <EmptyState
      v-else-if="filtered.length === 0"
      icon="user"
      :title="tab === 'pending' ? messages.admin.users.emptyPending : messages.admin.users.emptyTitle"
    />

    <div v-else class="admin-users__table card">
      <div class="admin-users__table-scroll">
        <table>
          <thead>
            <tr>
              <th>{{ messages.admin.users.colUser }}</th>
              <th>{{ messages.admin.users.colRole }}</th>
              <th>{{ messages.admin.users.colStatus }}</th>
              <th>{{ messages.admin.users.colCreated }}</th>
              <th class="admin-users__actions-col">{{ messages.admin.users.colActions }}</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="user in filtered" :key="user.id">
              <td>
                <div class="admin-users__user">
                  <span class="admin-users__avatar" aria-hidden="true">{{ initial(user.displayName) }}</span>
                  <div class="admin-users__user-meta">
                    <p class="admin-users__name">{{ user.displayName }}</p>
                    <p class="admin-users__email text-muted">{{ user.email }}</p>
                  </div>
                </div>
              </td>
              <td>
                <Badge :variant="user.role === 'TEACHER_PENDING' ? 'warning' : 'primary'">{{ roleLabel[user.role] }}</Badge>
              </td>
              <td>
                <Badge :variant="user.isActive ? 'success' : 'danger'">
                  {{ user.isActive ? messages.admin.users.active : messages.admin.users.locked }}
                </Badge>
              </td>
              <td class="admin-users__date text-muted">{{ formatDate(user.createdAt) }}</td>
              <td>
                <div class="admin-users__actions">
                  <template v-if="user.role === 'TEACHER_PENDING'">
                    <Button size="sm" variant="secondary" @click="openReview(user, 'approve')">
                      <Check :size="14" /> {{ messages.admin.users.approve }}
                    </Button>
                    <Button size="sm" variant="danger" @click="openReview(user, 'reject')">
                      <X :size="14" /> {{ messages.admin.users.reject }}
                    </Button>
                  </template>
                  <template v-else>
                    <Button size="sm" variant="ghost" :aria-label="`${user.isActive ? messages.admin.users.lock : messages.admin.users.unlock} ${user.displayName}`" @click="toggleLock(user)">
                      <LockOpen v-if="!user.isActive" :size="14" />
                      <Lock v-else :size="14" />
                      {{ user.isActive ? messages.admin.users.lock : messages.admin.users.unlock }}
                    </Button>
                  </template>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <Modal
      :open="reviewTarget !== null"
      :title="reviewAction === 'approve' ? messages.admin.users.approveTitle : messages.admin.users.rejectTitle"
      @close="reviewTarget = null"
    >
      <div class="admin-users__review">
        <span class="admin-users__review-avatar" aria-hidden="true">
          <UserCheck :size="18" />
        </span>
        <div class="admin-users__review-text">
          <p class="admin-users__review-name">{{ reviewTarget?.displayName }}</p>
          <p class="text-muted">{{ reviewTarget?.email }}</p>
        </div>
      </div>
      <div v-if="hasReviewInfo" class="admin-users__review-info">
        <p class="admin-users__review-info-title">{{ messages.admin.users.reviewTeacherInfo }}</p>
        <div v-if="reviewTarget?.department" class="admin-users__review-info-row">
          <span class="admin-users__review-info-label">{{ messages.admin.users.department }}</span>
          <span class="admin-users__review-info-value">{{ reviewTarget.department }}</span>
        </div>
        <div v-if="reviewTarget?.staffCode" class="admin-users__review-info-row">
          <span class="admin-users__review-info-label">{{ messages.admin.users.staffCode }}</span>
          <span class="admin-users__review-info-value">{{ reviewTarget.staffCode }}</span>
        </div>
        <div v-if="reviewTarget?.teacherBio" class="admin-users__review-info-row">
          <span class="admin-users__review-info-label">{{ messages.admin.users.teacherBio }}</span>
          <span class="admin-users__review-info-value">{{ reviewTarget.teacherBio }}</span>
        </div>
      </div>
      <Input
        v-if="reviewAction === 'reject'"
        v-model="rejectReason"
        :label="messages.admin.users.rejectReasonLabel"
        :placeholder="messages.admin.users.rejectReasonPlaceholder"
      />
      <template #footer>
        <Button variant="ghost" @click="reviewTarget = null">{{ messages.admin.users.cancel }}</Button>
        <Button
          :variant="reviewAction === 'approve' ? 'primary' : 'danger'"
          :disabled="reviewAction === 'reject' && rejectReason.trim() === ''"
          @click="submitReview"
        >
          {{ reviewAction === 'approve' ? messages.admin.users.confirmApprove : messages.admin.users.confirmReject }}
        </Button>
      </template>
    </Modal>
  </main>
</template>

<style scoped>
.admin-users {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Hero gradient Aurora soft (cùng pattern LeaderboardView) ── */
.admin-users__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 32%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-aurora);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
}

.admin-users__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: color-mix(in srgb, var(--color-background) 58%, transparent);
}

.admin-users__hero::before {
  content: '';
  position: absolute;
  width: 260px;
  height: 260px;
  border-radius: 50%;
  top: -120px;
  right: -60px;
  z-index: -1;
  background: color-mix(in srgb, var(--color-secondary) 30%, transparent);
  filter: blur(64px);
}

.admin-users__hero-body { display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.admin-users__hero-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg);
  background-image: var(--gradient-aurora);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-md);
}

.admin-users__hero-title-wrap { display: flex; flex-direction: column; gap: 4px; }

.admin-users__title {
  font-size: var(--text-2xl);
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.admin-users__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

.admin-users__hero-badge { margin-left: auto; }

/* ── Filter bar ── */
.admin-users__filters { display: flex; gap: var(--space-sm); flex-wrap: wrap; align-items: center; padding: var(--space-md); }

.admin-users__search-box { position: relative; flex: 1 1 220px; min-width: 200px; }

.admin-users__search-icon {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--color-text-muted);
  pointer-events: none;
}

.admin-users__search { padding-left: 34px; }

.admin-users__select { width: auto; }

/* ── Loading ── */
.admin-users__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

/* ── Table ── */
.admin-users__table { padding: 0; }

.admin-users__table-scroll { overflow-x: auto; border-radius: inherit; }

.admin-users__table table { width: 100%; border-collapse: collapse; min-width: 720px; }

.admin-users__table th {
  text-align: left;
  font-size: var(--text-xs);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
  padding: var(--space-sm) var(--space-md);
  border-bottom: 1px solid var(--color-border);
  background: var(--color-muted);
  white-space: nowrap;
}

.admin-users__table td {
  padding: var(--space-sm) var(--space-md);
  border-bottom: 1px solid var(--color-border);
  font-size: var(--text-sm);
  vertical-align: middle;
}

.admin-users__table tbody tr { transition: background-color 150ms ease; }

.admin-users__table tbody tr:hover { background: color-mix(in srgb, var(--color-primary) 5%, transparent); }

.admin-users__table tbody tr:last-child td { border-bottom: none; }

.admin-users__user { display: flex; align-items: center; gap: var(--space-sm); min-width: 0; }

.admin-users__avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background-image: var(--gradient-aurora);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: var(--text-sm);
  flex-shrink: 0;
}

.admin-users__user-meta { min-width: 0; }

.admin-users__name { font-weight: 700; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 260px; }
.admin-users__email { font-size: var(--text-xs); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 260px; }

.admin-users__date { white-space: nowrap; font-variant-numeric: tabular-nums; }

.admin-users__actions { display: flex; gap: var(--space-xs); flex-wrap: wrap; }

/* ── Modal duyệt ── */
.admin-users__review { display: flex; align-items: center; gap: var(--space-sm); margin-bottom: var(--space-md); }

.admin-users__review-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background-image: var(--gradient-aurora);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.admin-users__review-text { display: flex; flex-direction: column; min-width: 0; }
.admin-users__review-name { font-weight: 700; font-size: var(--text-sm); }

/* ── Thông tin đăng ký GV trong modal duyệt (task L) ── */
.admin-users__review-info {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  margin-bottom: var(--space-md);
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-muted);
}

.admin-users__review-info-title {
  font-size: var(--text-xs);
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
}

.admin-users__review-info-row {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
}

.admin-users__review-info-label {
  flex-shrink: 0;
  min-width: 7rem;
  color: var(--color-text-muted);
}

.admin-users__review-info-value {
  font-weight: 600;
  white-space: pre-wrap;
  word-break: break-word;
}

@media (max-width: 640px) {
  .admin-users__hero-badge { margin-left: 0; }
}
</style>
