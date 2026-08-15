<script setup lang="ts">
// AdminUsersView — Màn 10: quản lý người dùng + tab "Chờ duyệt Teacher" (Màn 29)
// View-quality 14/08 (Nhóm D): banner = surface band level-2 + mono strip
// block-token (số chờ duyệt — dữ liệu thật); bảng chuẩn §4.6 + mobile card-stack;
// error state + retry; icon/avatar neutral; actions gap ≥8px.
import { computed, onMounted, ref } from 'vue';
import {
  Check,
  ExternalLink,
  GraduationCap,
  KeyRound,
  Lock,
  LockOpen,
  RefreshCw,
  Search,
  UserCheck,
  UserCog,
  Users,
  X,
} from 'lucide-vue-next';

import * as adminApi from '@/api/admin';
import type { AdminRole, AdminUserDto } from '@/api/admin';
import { useUiStore } from '@/stores/ui';
import { formatDate } from '@/utils/format';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Modal from '@/components/ui/Modal.vue';
import Drawer from '@/components/ui/Drawer.vue';
import Input from '@/components/ui/Input.vue';
import Select, { type SelectOption } from '@/components/ui/Select.vue';
import Tabs from '@/components/ui/Tabs.vue';
import PageHero from '@/components/ui/PageHero.vue';
import DetailSection from '@/components/ui/DetailSection.vue';
import AdminHeroStrip from '@/components/admin/AdminHeroStrip.vue';
import AdminNav from '@/components/admin/AdminNav.vue';

const ui = useUiStore();

const tab = ref<'all' | 'pending'>('all');
const users = ref<AdminUserDto[]>([]);
const loading = ref(true);
const loadError = ref(false);
const search = ref('');
// Lọc client-side trên trang hiện tại. Sentinel 'ALL' = không lọc — reka-ui
// SelectItem CẤM value '' (ném Error lúc render), nên dùng sentinel thay option trống.
const ROLE_ALL = 'ALL';
const STATUS_ALL = 'ALL';
const roleFilter = ref(ROLE_ALL);
const statusFilter = ref(STATUS_ALL);

const roleOptions: SelectOption[] = [
  { label: messages.admin.users.roleAll, value: ROLE_ALL },
  { label: messages.admin.users.roleStudent, value: 'STUDENT' },
  { label: messages.admin.users.roleTeacher, value: 'TEACHER' },
  { label: messages.admin.users.rolePending, value: 'TEACHER_PENDING' },
  { label: messages.admin.users.roleAdmin, value: 'ADMIN' },
];

const statusOptions: SelectOption[] = [
  { label: messages.admin.users.statusAll, value: STATUS_ALL },
  { label: messages.admin.users.statusActive, value: 'active' },
  { label: messages.admin.users.statusLocked, value: 'locked' },
];

// Modal duyệt/từ chối teacher
const reviewTarget = ref<AdminUserDto | null>(null);
const reviewAction = ref<'approve' | 'reject'>('approve');
const rejectReason = ref('');
const rejectError = ref('');

// Block 2.3 — drawer chi tiết user (stats thật qua GET /users/{id})
const drawerUser = ref<AdminUserDto | null>(null);
const drawerDetail = ref<AdminUserDto | null>(null);
const drawerLoading = ref(false);
const drawerError = ref(false);

// Block 2.3 — hiển thị thông tin đăng ký GV trong modal (chỉ khi có giá trị)
const hasReviewInfo = computed(() => {
  const u = reviewTarget.value;
  return Boolean(u && (u.department || u.staffCode || u.academicDegree || u.profileLink || u.teacherBio));
});

onMounted(load);

async function load(): Promise<void> {
  loading.value = true;
  loadError.value = false;
  try {
    const page = await adminApi.fetchUsers({ role: tab.value === 'pending' ? 'TEACHER_PENDING' : undefined, q: search.value || undefined, page: 1 });
    users.value = page.items;
  } catch {
    loadError.value = true;
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
  if (roleFilter.value !== ROLE_ALL) list = list.filter((u) => u.role === roleFilter.value);
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

/** 2 chữ cái đầu: chữ cái đầu của 2 từ, hoặc 2 ký tự đầu khi tên 1 từ (Task 3a). */
const initials = (name: string): string => {
  const parts = name.trim().split(/\s+/).filter(Boolean);
  if (parts.length >= 2) return (parts[0].charAt(0) + parts[1].charAt(0)).toUpperCase();
  return name.trim().slice(0, 2).toUpperCase() || '?';
};

// Vai trò → badge/avatar: 1 ngôn ngữ màu xuyên 2 màn (donut AdminStats dùng
// data-core/resolved/index-muted; badge dùng variant shadcn tương phản cả 2 theme).
const roleBadgeVariant: Record<AdminRole, 'primary' | 'secondary' | 'warning' | 'muted'> = {
  STUDENT: 'muted',
  TEACHER: 'primary',
  TEACHER_PENDING: 'warning',
  ADMIN: 'secondary',
};

const avatarRoleClass = (role: AdminRole): string =>
  role === 'TEACHER'
    ? 'admin-users__role-avatar--teacher'
    : role === 'TEACHER_PENDING'
      ? 'admin-users__role-avatar--pending'
      : role === 'ADMIN'
        ? 'admin-users__role-avatar--admin'
        : '';

async function toggleLock(user: AdminUserDto): Promise<void> {
  try {
    await adminApi.setUserStatus(user.id, { isActive: !user.isActive });
    user.isActive = !user.isActive;
    const row = users.value.find((u) => u.id === user.id);
    if (row) row.isActive = user.isActive;
    ui.showToast(user.isActive ? messages.admin.users.toastUnlocked : messages.admin.users.toastLocked, 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.admin.users.toastActionFailed, 'error');
  }
}

function openReview(user: AdminUserDto, action: 'approve' | 'reject'): void {
  reviewTarget.value = user;
  reviewAction.value = action;
  rejectReason.value = '';
  rejectError.value = '';
}

async function submitReview(): Promise<void> {
  if (!reviewTarget.value) return;
  if (reviewAction.value === 'reject' && !rejectReason.value.trim()) {
    rejectError.value = messages.admin.users.rejectReasonRequired;
    return;
  }
  try {
    await adminApi.approveTeacher(reviewTarget.value.id, {
      approve: reviewAction.value === 'approve',
      reason: reviewAction.value === 'reject' ? rejectReason.value.trim() : undefined,
    });
    ui.showToast(reviewAction.value === 'approve' ? messages.admin.users.toastApproved : messages.admin.users.toastRejected, 'success');
    reviewTarget.value = null;
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.admin.users.toastActionFailed, 'error');
  }
}

// ── Block 2.3 - Drawer chi tiết user ──

function openDrawer(user: AdminUserDto): void {
  drawerUser.value = user;
  drawerDetail.value = null;
  drawerError.value = false;
  drawerLoading.value = true;
  void loadDrawerDetail(user.id);
}

function closeDrawer(): void {
  drawerUser.value = null;
  drawerDetail.value = null;
  drawerLoading.value = false;
  drawerError.value = false;
}

async function loadDrawerDetail(id: number): Promise<void> {
  drawerLoading.value = true;
  drawerError.value = false;
  try {
    drawerDetail.value = await adminApi.fetchUser(id);
  } catch {
    drawerError.value = true;
    drawerDetail.value = null;
  } finally {
    drawerLoading.value = false;
  }
}

/** Đổi vai trò STUDENT ↔ TEACHER (không áp dụng cho TEACHER_PENDING/ADMIN). */
async function changeRole(user: AdminUserDto): Promise<void> {
  const next = user.role === 'TEACHER' ? 'STUDENT' : 'TEACHER';
  try {
    await adminApi.setUserRole(user.id, { role: next });
    user.role = next;
    const row = users.value.find((u) => u.id === user.id);
    if (row) row.role = next;
    ui.showToast(messages.admin.users.roleChanged(next === 'STUDENT' ? messages.admin.users.roleStudent : messages.admin.users.roleTeacher), 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Thao tác thất bại.', 'error');
  }
}

async function resetPassword(user: AdminUserDto): Promise<void> {
  try {
    await adminApi.resetUserPassword(user.id);
    ui.showToast(messages.admin.users.passwordReset, 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Thao tác thất bại.', 'error');
  }
}

// ── Task 3a — xác nhận hành động tài khoản (drawer): khóa/mở khóa + đặt lại mật khẩu ──
type ConfirmAction = 'lock' | 'unlock' | 'resetPassword';

const confirmAction = ref<{ user: AdminUserDto; action: ConfirmAction } | null>(null);

function askConfirm(user: AdminUserDto, action: ConfirmAction): void {
  confirmAction.value = { user, action };
}

const confirmTitle = computed(() => {
  const c = confirmAction.value;
  if (!c) return '';
  if (c.action === 'lock') return messages.admin.users.lockConfirmTitle;
  if (c.action === 'unlock') return messages.admin.users.unlockConfirmTitle;
  return messages.admin.users.resetConfirmTitle;
});

const confirmMessage = computed(() => {
  const c = confirmAction.value;
  if (!c) return '';
  const name = c.user.displayName || c.user.email;
  if (c.action === 'lock') return messages.admin.users.lockConfirmMessage(name);
  if (c.action === 'unlock') return messages.admin.users.unlockConfirmMessage(name);
  return messages.admin.users.resetConfirmMessage(name);
});

const confirmButtonLabel = computed(() => {
  const c = confirmAction.value;
  if (!c) return '';
  if (c.action === 'lock') return messages.admin.users.confirmLock;
  if (c.action === 'unlock') return messages.admin.users.confirmUnlock;
  return messages.admin.users.confirmReset;
});

const confirmVariant = computed(() =>
  confirmAction.value?.action === 'lock' ? 'danger' : 'primary',
);

async function runConfirm(): Promise<void> {
  const c = confirmAction.value;
  if (!c) return;
  confirmAction.value = null;
  // Giữ NGUYÊN API hiện có — chỉ thêm bước xác nhận trực quan.
  if (c.action === 'resetPassword') await resetPassword(c.user);
  else await toggleLock(c.user);
}
</script>

<template>
  <main class="admin-users container">
    <!-- Banner: surface band level-2 (PageHero — DESIGN §1/#1: KHÔNG gradient, KHÔNG shadow) -->
    <PageHero
      :badge="messages.admin.badge"
      :title="messages.admin.users.title"
      :description="messages.admin.users.subtitle"
    >
      <!-- Mono strip: block-token dữ liệu thật (số chờ duyệt) + index mono (quyết định #4) -->
      <template #side>
        <AdminHeroStrip :count="pendingCount" :label="messages.admin.users.stripLabel(pendingCount)" />
      </template>
    </PageHero>

    <AdminNav active="users" />

    <!-- Tabs shadcn: Tất cả / Chờ duyệt Teacher (badge = số chờ duyệt) -->
    <Tabs :tabs="userTabs" :model-value="tab" @change="switchTab" />

    <div class="admin-users__filters">
      <div class="admin-users__search-box">
        <Search :size="16" class="admin-users__search-icon" aria-hidden="true" />
        <input
          v-model="search"
          class="admin-users__search"
          type="search"
          :placeholder="messages.admin.users.searchPlaceholder"
          :aria-label="messages.admin.users.searchLabel"
          @keyup.enter="load"
        />
      </div>
      <!-- Select shadcn (reka-ui) — nhất quán ClassDetailView; label ẩn sr-only qua :deep -->
      <div class="admin-users__select-group">
        <Select
          v-model="roleFilter"
          :label="messages.admin.users.roleFilterLabel"
          :options="roleOptions"
          :placeholder="messages.admin.users.roleAll"
        />
      </div>
      <div class="admin-users__select-group">
        <Select
          v-model="statusFilter"
          :label="messages.admin.users.statusFilterLabel"
          :options="statusOptions"
          :placeholder="messages.admin.users.statusAll"
        />
      </div>
      <Button variant="secondary" @click="load">
        <Search :size="16" /> {{ messages.admin.users.search }}
      </Button>
    </div>

    <div v-if="loading" class="admin-users__loading" aria-busy="true">
      <Skeleton v-for="i in 6" :key="i" height="56px" />
    </div>

    <div v-else-if="loadError" class="admin-users__error" role="alert">
      <p class="admin-users__error-text">{{ messages.admin.users.loadError }}</p>
      <Button size="sm" variant="secondary" @click="load">
        <RefreshCw :size="14" /> {{ messages.admin.users.retry }}
      </Button>
    </div>

    <EmptyState
      v-else-if="filtered.length === 0"
      icon="user"
      :title="tab === 'pending' ? messages.admin.users.emptyPending : messages.admin.users.emptyTitle"
      :description="tab === 'pending' ? messages.admin.users.emptyPendingDesc : messages.admin.users.emptyTitleDesc"
    />

    <div v-else class="admin-users__table">
      <div class="admin-users__table-scroll">
        <table>
          <thead>
            <tr>
              <th scope="col">{{ messages.admin.users.colUser }}</th>
              <th scope="col">{{ messages.admin.users.colRole }}</th>
              <th scope="col">{{ messages.admin.users.colStatus }}</th>
              <th scope="col">{{ messages.admin.users.colCreated }}</th>
              <th scope="col" class="admin-users__actions-col">{{ messages.admin.users.colActions }}</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="user in filtered"
              :key="user.id"
              class="admin-users__row"
              tabindex="0"
              role="button"
              :aria-label="`${messages.admin.users.detailsHint}: ${user.displayName || user.email}`"
              @click="openDrawer(user)"
              @keydown.enter="openDrawer(user)"
              @keydown.space.prevent="openDrawer(user)"
            >
              <td :data-label="messages.admin.users.colUser">
                <div class="admin-users__user">
                  <span
                    class="admin-users__avatar"
                    :class="avatarRoleClass(user.role)"
                    aria-hidden="true"
                  >{{ initials(user.displayName) }}</span>
                  <div class="admin-users__user-meta">
                    <p class="admin-users__name">{{ user.displayName }}</p>
                    <p class="admin-users__email">{{ user.email }}</p>
                  </div>
                </div>
              </td>
              <td :data-label="messages.admin.users.colRole">
                <Badge :variant="roleBadgeVariant[user.role]" class="rounded-md!">{{ roleLabel[user.role] }}</Badge>
              </td>
              <td :data-label="messages.admin.users.colStatus">
                <Badge :variant="user.isActive ? 'success' : 'danger'" class="rounded-md!">
                  {{ user.isActive ? messages.admin.users.active : messages.admin.users.locked }}
                </Badge>
              </td>
              <td :data-label="messages.admin.users.colCreated" class="admin-users__date">{{ formatDate(user.createdAt) }}</td>
              <td :data-label="messages.admin.users.colActions">
                <div class="admin-users__actions" @click.stop>
                  <template v-if="user.role === 'TEACHER_PENDING'">
                    <Button size="sm" variant="secondary" @click="openReview(user, 'approve')">
                      <Check :size="16" /> {{ messages.admin.users.approve }}
                    </Button>
                    <Button size="sm" variant="danger" @click="openReview(user, 'reject')">
                      <X :size="16" /> {{ messages.admin.users.reject }}
                    </Button>
                  </template>
                  <template v-else>
                    <Button size="sm" variant="ghost" :aria-label="`${user.isActive ? messages.admin.users.lock : messages.admin.users.unlock} ${user.displayName || user.email}`" @click="toggleLock(user)">
                      <LockOpen v-if="!user.isActive" :size="16" />
                      <Lock v-else :size="16" />
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
          <p class="admin-users__review-email">{{ reviewTarget?.email }}</p>
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
        <div v-if="reviewTarget?.academicDegree" class="admin-users__review-info-row">
          <span class="admin-users__review-info-label">{{ messages.admin.users.academicDegree }}</span>
          <span class="admin-users__review-info-value">{{ reviewTarget.academicDegree }}</span>
        </div>
        <div v-if="reviewTarget?.profileLink" class="admin-users__review-info-row">
          <span class="admin-users__review-info-label">{{ messages.admin.users.profileLink }}</span>
          <a
            class="admin-users__review-info-link"
            :href="reviewTarget.profileLink"
            target="_blank"
            rel="noopener noreferrer"
          >
            {{ reviewTarget.profileLink }}
            <ExternalLink :size="12" aria-hidden="true" />
          </a>
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
        :error="rejectError"
        @update:model-value="rejectError = ''"
      />
      <template #footer>
        <Button variant="ghost" @click="reviewTarget = null">{{ messages.admin.users.cancel }}</Button>
        <Button
          :variant="reviewAction === 'approve' ? 'primary' : 'danger'"
          @click="submitReview"
        >
          {{ reviewAction === 'approve' ? messages.admin.users.confirmApprove : messages.admin.users.confirmReject }}
        </Button>
      </template>
    </Modal>

    <!-- Task 3a — xác nhận khóa/mở khóa + đặt lại mật khẩu (giữ nguyên API hiện có) -->
    <Modal :open="confirmAction !== null" :title="confirmTitle" @close="confirmAction = null">
      <p class="admin-users__confirm-message">{{ confirmMessage }}</p>
      <template #footer>
        <Button variant="ghost" @click="confirmAction = null">{{ messages.admin.users.cancel }}</Button>
        <Button :variant="confirmVariant" @click="runConfirm">{{ confirmButtonLabel }}</Button>
      </template>
    </Modal>

    <!-- Block 2.3 — Drawer chi tiết user: stats thật qua GET /users/{id} -->
    <Drawer
      :open="drawerUser !== null"
      :title="messages.admin.users.drawerTitle"
      :description="drawerUser ? messages.admin.users.detailDescription(drawerUser.displayName) : ''"
      :width="'440px'"
      @close="closeDrawer"
    >
      <div v-if="drawerLoading" class="admin-users__drawer-loading" aria-busy="true">
        <Skeleton v-for="i in 4" :key="i" height="48px" />
      </div>

      <div v-else-if="drawerError" class="admin-users__drawer-error" role="alert">
        <p class="admin-users__drawer-error-text">{{ messages.admin.users.drawerLoadError }}</p>
        <Button size="sm" variant="secondary" @click="drawerUser && loadDrawerDetail(drawerUser.id)">
          <RefreshCw :size="14" /> {{ messages.admin.users.retry }}
        </Button>
      </div>

      <div v-else-if="drawerDetail" class="admin-users__drawer">
        <div class="admin-users__drawer-head">
          <span
            class="admin-users__drawer-avatar"
            :class="avatarRoleClass(drawerDetail.role)"
            aria-hidden="true"
          >{{ initials(drawerDetail.displayName) }}</span>
          <div class="admin-users__drawer-head-meta">
            <p class="admin-users__drawer-name">{{ drawerDetail.displayName }}</p>
            <p class="admin-users__drawer-email">{{ drawerDetail.email }}</p>
            <div class="admin-users__drawer-badges">
              <Badge :variant="roleBadgeVariant[drawerDetail.role]" class="rounded-md!">
                {{ roleLabel[drawerDetail.role] }}
              </Badge>
              <Badge :variant="drawerDetail.isActive ? 'success' : 'danger'" class="rounded-md!">
                {{ drawerDetail.isActive ? messages.admin.users.active : messages.admin.users.locked }}
              </Badge>
            </div>
          </div>
        </div>

        <DetailSection :title="messages.admin.users.sectionProfile">
          <div class="admin-users__drawer-info">
            <div v-if="drawerDetail.department" class="admin-users__drawer-row">
              <span class="admin-users__drawer-label">{{ messages.admin.users.department }}</span>
              <span class="admin-users__drawer-value">{{ drawerDetail.department }}</span>
            </div>
            <div v-if="drawerDetail.staffCode" class="admin-users__drawer-row">
              <span class="admin-users__drawer-label">{{ messages.admin.users.staffCode }}</span>
              <span class="admin-users__drawer-value">{{ drawerDetail.staffCode }}</span>
            </div>
            <div v-if="drawerDetail.academicDegree" class="admin-users__drawer-row">
              <span class="admin-users__drawer-label">{{ messages.admin.users.academicDegree }}</span>
              <span class="admin-users__drawer-value">{{ drawerDetail.academicDegree }}</span>
            </div>
            <div v-if="drawerDetail.profileLink" class="admin-users__drawer-row">
              <span class="admin-users__drawer-label">{{ messages.admin.users.profileLink }}</span>
              <a
                class="admin-users__drawer-link"
                :href="drawerDetail.profileLink"
                target="_blank"
                rel="noopener noreferrer"
              >
                {{ drawerDetail.profileLink }}
                <ExternalLink :size="12" aria-hidden="true" />
              </a>
            </div>
            <div class="admin-users__drawer-row">
              <span class="admin-users__drawer-label">{{ messages.admin.users.colCreated }}</span>
              <span class="admin-users__drawer-value admin-users__drawer-value--mono">{{ formatDate(drawerDetail.createdAt) }}</span>
            </div>
          </div>
        </DetailSection>

        <DetailSection :title="messages.admin.users.sectionLearning" :icon="GraduationCap">
          <div class="admin-users__drawer-stats">
            <div class="admin-users__drawer-stat">
              <span class="admin-users__drawer-stat-value">{{ drawerDetail.level ?? 0 }}</span>
              <span class="admin-users__drawer-stat-label">{{ messages.admin.users.statLevel }}</span>
            </div>
            <div class="admin-users__drawer-stat">
              <span class="admin-users__drawer-stat-value">{{ drawerDetail.xp ?? 0 }}</span>
              <span class="admin-users__drawer-stat-label">{{ messages.admin.users.statXp }}</span>
            </div>
            <div class="admin-users__drawer-stat">
              <span class="admin-users__drawer-stat-value">{{ drawerDetail.streakDays ?? 0 }}</span>
              <span class="admin-users__drawer-stat-label">{{ messages.admin.users.statStreak }}</span>
            </div>
            <div class="admin-users__drawer-stat">
              <span class="admin-users__drawer-stat-value">{{ drawerDetail.gems ?? 0 }}</span>
              <span class="admin-users__drawer-stat-label">{{ messages.admin.users.statGems }}</span>
            </div>
            <div class="admin-users__drawer-stat">
              <span class="admin-users__drawer-stat-value">{{ drawerDetail.hearts ?? 0 }}</span>
              <span class="admin-users__drawer-stat-label">{{ messages.admin.users.statHearts }}</span>
            </div>
          </div>
        </DetailSection>

        <DetailSection :title="messages.admin.users.sectionActivity">
          <div class="admin-users__drawer-stats">
            <div class="admin-users__drawer-stat">
              <span class="admin-users__drawer-stat-value">{{ drawerDetail.lessonsCompletedCount ?? 0 }}</span>
              <span class="admin-users__drawer-stat-label">{{ messages.admin.users.statLessons }}</span>
            </div>
            <div class="admin-users__drawer-stat">
              <span class="admin-users__drawer-stat-value">{{ drawerDetail.exercisesPassedCount ?? 0 }}</span>
              <span class="admin-users__drawer-stat-label">{{ messages.admin.users.statExercises }}</span>
            </div>
            <div class="admin-users__drawer-stat">
              <span class="admin-users__drawer-stat-value">{{ drawerDetail.joinedClassesCount ?? 0 }}</span>
              <span class="admin-users__drawer-stat-label">{{ messages.admin.users.statClasses }}</span>
            </div>
          </div>
        </DetailSection>

        <DetailSection :title="messages.admin.users.colActions">
          <div class="admin-users__drawer-actions">
            <Button
              block
              :variant="drawerDetail.isActive ? 'secondary' : 'primary'"
              @click="askConfirm(drawerDetail, drawerDetail.isActive ? 'lock' : 'unlock')"
            >
              <LockOpen v-if="!drawerDetail.isActive" :size="16" />
              <Lock v-else :size="16" />
              {{ drawerDetail.isActive ? messages.admin.users.lock : messages.admin.users.unlock }}
            </Button>
            <Button
              v-if="drawerDetail.role === 'TEACHER' || drawerDetail.role === 'STUDENT'"
              block
              variant="secondary"
              @click="changeRole(drawerDetail)"
            >
              <UserCog :size="16" /> {{ messages.admin.users.changeRole }}
            </Button>
            <Button block variant="secondary" @click="askConfirm(drawerDetail, 'resetPassword')">
              <KeyRound :size="16" /> {{ messages.admin.users.resetPassword }}
            </Button>
          </div>
        </DetailSection>
      </div>
    </Drawer>
  </main>
</template>

<style scoped>
.admin-users {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Filter bar ── */
.admin-users__filters {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  align-items: center;
  padding: var(--space-md);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
}

.admin-users__search-box { position: relative; flex: 1 1 220px; min-width: 200px; }

.admin-users__search-icon {
  position: absolute;
  left: var(--space-sm);
  top: 50%;
  transform: translateY(-50%);
  color: var(--foreground-quaternary);
  pointer-events: none;
}

.admin-users__search {
  height: 40px;
  width: 100%;
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  background: var(--card);
  color: var(--foreground);
  font-size: var(--text-sm);
  padding: 0 var(--space-md) 0 var(--space-xl);
  transition: border-color 150ms;
}

.admin-users__search::placeholder { color: var(--foreground-quaternary); }

.admin-users__search:focus-visible {
  outline: 2px solid var(--ring);
  outline-offset: 0;
  border-color: var(--primary);
}

/* Select shadcn trong toolbar — label sr-only (a11y), trigger h-10 chuẩn §4.4 */
.admin-users__select-group { flex: 1 1 180px; min-width: 170px; max-width: 250px; }

.admin-users__select-group :deep(label) {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

/* ── Loading / Error ── */
.admin-users__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-users__error {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
  flex-wrap: wrap;
  padding: var(--space-md);
  border: 1px solid color-mix(in srgb, var(--destructive) 35%, transparent);
  background: color-mix(in srgb, var(--destructive) 8%, transparent);
  border-radius: var(--radius-md);
}

.admin-users__error-text { margin: 0; font-size: var(--text-sm); color: var(--destructive); }

/* ── Table (DESIGN §4.6) ── */
.admin-users__table {
  padding: 0;
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.admin-users__table-scroll { overflow-x: auto; border-radius: inherit; }

.admin-users__table table { width: 100%; border-collapse: collapse; }

.admin-users__table th {
  text-align: left;
  font-size: var(--text-sm);
  font-weight: 500;
  color: var(--foreground-tertiary);
  padding: 0 var(--space-md);
  height: 40px;
  border-bottom: 1px solid var(--border);
  background: var(--muted);
  white-space: nowrap;
}

.admin-users__table td {
  padding: 12px var(--space-md);
  border-bottom: 1px solid var(--border);
  font-size: var(--text-sm);
  vertical-align: middle;
}

.admin-users__table tbody tr { transition: background-color 150ms; }

.admin-users__table tbody tr:hover { background: color-mix(in srgb, var(--muted) 50%, transparent); }

.admin-users__table tbody tr:last-child td { border-bottom: none; }

.admin-users__user { display: flex; align-items: center; gap: var(--space-sm); min-width: 0; }

.admin-users__avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--muted);
  color: var(--foreground-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: var(--text-sm);
  flex-shrink: 0;
}

/* Avatar theo vai trò (Task 3a) — tint + text foreground, tương phản cả 2 theme */
.admin-users__role-avatar--teacher {
  background: color-mix(in srgb, var(--primary) 14%, transparent);
  color: var(--foreground);
}

.admin-users__role-avatar--pending {
  background: color-mix(in srgb, var(--warning) 16%, transparent);
  color: var(--foreground);
}

.admin-users__role-avatar--admin {
  background: var(--muted);
  border: 1px solid var(--border-strong);
  color: var(--foreground);
}

.admin-users__user-meta { min-width: 0; }

.admin-users__name { font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 260px; }
.admin-users__email { font-size: var(--text-xs); color: var(--foreground-tertiary); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 260px; }

.admin-users__date {
  white-space: nowrap;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  font-variant-numeric: tabular-nums;
}

.admin-users__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

/* ── Modal duyệt ── */
.admin-users__review { display: flex; align-items: center; gap: var(--space-sm); margin-bottom: var(--space-md); }

.admin-users__review-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: var(--muted);
  color: var(--foreground-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.admin-users__review-text { display: flex; flex-direction: column; min-width: 0; }
.admin-users__review-name { font-weight: 600; font-size: var(--text-sm); }
.admin-users__review-email { font-size: var(--text-xs); color: var(--foreground-tertiary); }

/* ── Thông tin đăng ký GV trong modal duyệt (task L) ── */
.admin-users__review-info {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  margin-bottom: var(--space-md);
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  background: var(--muted);
}

.admin-users__review-info-title {
  font-size: var(--text-xs);
  font-weight: 600;
  color: var(--foreground-tertiary);
}

.admin-users__review-info-row {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
}

.admin-users__review-info-label {
  flex-shrink: 0;
  min-width: 7rem;
  color: var(--foreground-tertiary);
}

.admin-users__review-info-value {
  font-weight: 600;
  white-space: pre-wrap;
  word-break: break-word;
}

/* ── Row clickable → drawer chi tiết (Block 2.3) ── */
.admin-users__row { cursor: pointer; }

.admin-users__row:focus-visible {
  outline: 2px solid var(--primary);
  outline-offset: -2px;
}

.admin-users__review-info-link {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-weight: 600;
  color: var(--primary);
  word-break: break-all;
}

.admin-users__review-info-link:hover { text-decoration: underline; }

/* ── Drawer chi tiết user (Block 2.3) ── */
.admin-users__drawer { display: flex; flex-direction: column; gap: var(--space-lg); }

.admin-users__drawer-loading { display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-users__drawer-error {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: var(--space-sm);
  padding: var(--space-md);
  border: 1px solid color-mix(in srgb, var(--destructive) 35%, transparent);
  background: color-mix(in srgb, var(--destructive) 8%, transparent);
  border-radius: var(--radius-md);
}

.admin-users__drawer-error-text { margin: 0; font-size: var(--text-sm); color: var(--destructive); }

.admin-users__drawer-head {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding-bottom: var(--space-md);
  border-bottom: 1px solid var(--border);
}

.admin-users__drawer-avatar {
  width: 44px;
  height: 44px;
  border-radius: 50%;
  background: var(--muted);
  color: var(--foreground-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: var(--text-base);
  flex-shrink: 0;
}

.admin-users__drawer-head-meta { display: flex; flex-direction: column; gap: 2px; min-width: 0; }
.admin-users__drawer-name { font-weight: 600; margin: 0; word-break: break-word; }
.admin-users__drawer-email { font-size: var(--text-xs); color: var(--foreground-tertiary); margin: 0; word-break: break-all; }

.admin-users__drawer-badges { display: flex; gap: var(--space-xs); flex-wrap: wrap; margin-top: var(--space-xs); }

.admin-users__drawer-info {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  background: var(--muted);
}

.admin-users__drawer-row {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
}

.admin-users__drawer-label {
  flex-shrink: 0;
  min-width: 7rem;
  color: var(--foreground-tertiary);
}

.admin-users__drawer-value {
  font-weight: 600;
  white-space: pre-wrap;
  word-break: break-word;
}

.admin-users__drawer-value--mono {
  font-family: var(--font-mono);
  font-variant-numeric: tabular-nums;
}

.admin-users__drawer-link {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-weight: 600;
  color: var(--primary);
  word-break: break-all;
}

.admin-users__drawer-link:hover { text-decoration: underline; }

.admin-users__drawer-stats {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: var(--space-sm);
}

.admin-users__drawer-stat {
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  background: var(--card);
}

.admin-users__drawer-stat-value {
  font-family: var(--font-mono);
  font-size: var(--text-lg);
  font-weight: 600;
  font-variant-numeric: tabular-nums;
  color: var(--foreground);
}

.admin-users__drawer-stat-label {
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
}

.admin-users__drawer-actions { display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-users__confirm-message {
  margin: 0;
  font-size: var(--text-sm);
  line-height: 1.6;
  color: var(--foreground-secondary);
}

@media (max-width: 640px) {
  .admin-users__select-group { flex: 1 1 100%; max-width: none; }

  /* Bảng → card-stack (DESIGN §8 — cấm scroll ngang bảng chính ở mobile) */
  .admin-users__table-scroll { overflow-x: visible; }

  .admin-users__table thead { display: none; }

  .admin-users__table tbody tr {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-xs) var(--space-md);
    padding: var(--space-sm) var(--space-md);
    border-bottom: 1px solid var(--border);
  }

  .admin-users__table tbody tr:last-child { border-bottom: none; }

  .admin-users__table td {
    display: flex;
    flex-direction: column;
    gap: var(--space-xs);
    padding: 0;
    border-bottom: none;
  }

  .admin-users__table td::before {
    content: attr(data-label);
    font-size: var(--text-xs);
    color: var(--foreground-tertiary);
  }

  .admin-users__table td:first-child { grid-column: 1 / -1; }
  .admin-users__table td:last-child { align-items: flex-start; }

  .admin-users__name,
  .admin-users__email { max-width: 100%; white-space: normal; }
}
</style>
