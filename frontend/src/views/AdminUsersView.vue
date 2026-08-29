<script setup lang="ts">
import { computed, onMounted, reactive, ref } from 'vue';
import {
  Check,
  CheckCircle2,
  ExternalLink,
  GraduationCap,
  KeyRound,
  Lock,
  LockOpen,
  RefreshCw,
  Search,
  UserCheck,
  UserPlus,
  Users,
  X,
  Pencil,
  Ban,
  Shield,
  ShieldCheck,
  AlertTriangle,
  Mail,
  Building,
  User,
} from 'lucide-vue-next';

import * as adminApi from '@/api/admin';
import type { AdminUserDto, AdminRole } from '@/api/admin';
import { useUiStore } from '@/stores/ui';
import { useAuthStore } from '@/stores/auth';
import { formatDate } from '@/utils/format';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Modal from '@/components/ui/Modal.vue';
import Drawer from '@/components/ui/Drawer.vue';
import Input from '@/components/ui/Input.vue';
import Tabs from '@/components/ui/Tabs.vue';
import AdminNav from '@/components/admin/AdminNav.vue';

const ui = useUiStore();
const auth = useAuthStore();

const tab = ref<'all' | 'pending'>('all');
const users = ref<AdminUserDto[]>([]);
const loading = ref(true);
const loadError = ref(false);
const search = ref('');
const roleFilter = ref('');
const statusFilter = ref('');

// ── QUY TẮC BẢO VỆ ADMIN NGANG ROLE (Business & Security Rule) ──
function isTargetAdmin(user?: AdminUserDto | null): boolean {
  return user?.role === 'ADMIN';
}

function isSelf(user?: AdminUserDto | null): boolean {
  if (!user || !auth.user) return false;
  return user.id === auth.user.id || user.email === auth.user.email;
}

/** Admin không được sửa role, khóa hoặc xóa tài khoản Admin khác hoặc tài khoản của chính mình */
function canManageUser(user?: AdminUserDto | null): boolean {
  if (!user) return false;
  if (isSelf(user)) return false;
  if (isTargetAdmin(user)) return false;
  return true;
}

// ── Modal Tạo Người Dùng Mới ──
const createModalOpen = ref(false);
const creating = ref(false);
const createForm = reactive({
  displayName: '',
  email: '',
  password: '',
  role: 'STUDENT' as AdminRole,
  department: '',
  staffCode: '',
});

function openCreateModal(): void {
  Object.assign(createForm, {
    displayName: '',
    email: '',
    password: '',
    role: 'STUDENT',
    department: '',
    staffCode: '',
  });
  createModalOpen.value = true;
}

async function handleCreateUser(): Promise<void> {
  if (createForm.displayName.trim().length < 2) {
    ui.showToast('Họ tên phải từ 2 ký tự trở lên.', 'warning');
    return;
  }
  if (!createForm.email.includes('@')) {
    ui.showToast('Email không hợp lệ.', 'warning');
    return;
  }
  if (createForm.password.length < 6) {
    ui.showToast('Mật khẩu tối thiểu 6 ký tự.', 'warning');
    return;
  }
  if (createForm.role === 'TEACHER' && !createForm.staffCode.trim()) {
    ui.showToast('Vui lòng nhập Mã giảng viên', 'warning');
    return;
  }

  creating.value = true;
  try {
    await adminApi.createUser({
      displayName: createForm.displayName.trim(),
      email: createForm.email.trim(),
      password: createForm.password,
      role: createForm.role,
      department: createForm.role === 'TEACHER' ? createForm.department.trim() : undefined,
      staffCode: createForm.role === 'TEACHER' ? createForm.staffCode.trim() : undefined,
    });
    ui.showToast('Đã tạo người dùng mới thành công!', 'success');
    createModalOpen.value = false;
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Tạo người dùng thất bại.', 'error');
  } finally {
    creating.value = false;
  }
}

// ── Modal Chỉnh Sửa Người Dùng ──
const editModalOpen = ref(false);
const editing = ref(false);
const editingTarget = ref<AdminUserDto | null>(null);
const editForm = reactive({
  displayName: '',
  role: 'STUDENT' as AdminRole,
  isActive: true,
  department: '',
  staffCode: '',
  academicDegree: '',
  profileLink: '',
  teacherBio: '',
});

function openEditModal(user: AdminUserDto): void {
  editingTarget.value = user;
  Object.assign(editForm, {
    displayName: user.displayName,
    role: user.role,
    isActive: user.isActive,
    department: user.department || '',
    staffCode: user.staffCode || '',
    academicDegree: user.academicDegree || '',
    profileLink: user.profileLink || '',
    teacherBio: user.teacherBio || '',
  });
  editModalOpen.value = true;
}

async function handleUpdateUser(): Promise<void> {
  if (!editingTarget.value) return;
  if (editingTarget.value.role === 'ADMIN' && editForm.role !== 'ADMIN') {
    ui.showToast('Không thể hạ cấp vai trò Quản trị viên cùng cấp.', 'warning');
    return;
  }

  editing.value = true;
  try {
    await adminApi.updateUser(editingTarget.value.id, {
      displayName: editForm.displayName.trim(),
      role: isTargetAdmin(editingTarget.value) ? undefined : editForm.role,
      isActive: isTargetAdmin(editingTarget.value) ? undefined : editForm.isActive,
      department: editForm.department.trim() || undefined,
      staffCode: editForm.staffCode.trim() || undefined,
      academicDegree: editForm.academicDegree.trim() || undefined,
      profileLink: editForm.profileLink.trim() || undefined,
      teacherBio: editForm.teacherBio.trim() || undefined,
    });
    ui.showToast('Đã cập nhật thông tin người dùng!', 'success');
    editModalOpen.value = false;
    void load();
    if (drawerUser.value?.id === editingTarget.value.id) {
      void loadDrawerDetail(editingTarget.value.id);
    }
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Cập nhật thất bại.', 'error');
  } finally {
    editing.value = false;
  }
}

// ── Modal Đặt Lại Mật Khẩu ──
const resetModalOpen = ref(false);
const resetting = ref(false);
const resetTarget = ref<AdminUserDto | null>(null);
const newPassword = ref('');

function openResetModal(user: AdminUserDto): void {
  if (isTargetAdmin(user) && !isSelf(user)) {
    ui.showToast('Không thể đặt lại mật khẩu của Quản trị viên khác.', 'warning');
    return;
  }
  resetTarget.value = user;
  newPassword.value = '';
  resetModalOpen.value = true;
}

async function handleResetPassword(): Promise<void> {
  if (!resetTarget.value) return;
  if (newPassword.value.length < 6) {
    ui.showToast('Mật khẩu mới tối thiểu 6 ký tự.', 'warning');
    return;
  }

  resetting.value = true;
  try {
    await adminApi.resetUserPassword(resetTarget.value.id, newPassword.value);
    ui.showToast(`Đã đặt lại mật khẩu mới cho ${resetTarget.value.displayName}!`, 'success');
    resetModalOpen.value = false;
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Đặt lại mật khẩu thất bại.', 'error');
  } finally {
    resetting.value = false;
  }
}

// ── Modal Trạng Thái Tài Khoản (Vô hiệu hóa / Kích hoạt lại — A4) ──
const statusModalOpen = ref(false);
const statusBusy = ref(false);
const statusTarget = ref<AdminUserDto | null>(null);
const statusNextActive = ref<boolean>(false);

function openStatusModal(user: AdminUserDto, nextActive: boolean): void {
  if (!canManageUser(user)) {
    ui.showToast('Không thể thay đổi trạng thái tài khoản Quản trị viên cùng cấp.', 'warning');
    return;
  }
  statusTarget.value = user;
  statusNextActive.value = nextActive;
  statusModalOpen.value = true;
}

async function handleConfirmStatus(): Promise<void> {
  if (!statusTarget.value) return;
  statusBusy.value = true;
  const targetId = statusTarget.value.id;
  const nextActive = statusNextActive.value;
  try {
    await adminApi.setUserStatus(targetId, { isActive: nextActive });
    ui.showToast(
      nextActive ? 'Đã kích hoạt lại tài khoản thành công!' : 'Đã vô hiệu hóa tài khoản người dùng.',
      'success',
    );
    statusModalOpen.value = false;
    statusTarget.value = null;
    await load();
    if (drawerUser.value?.id === targetId) {
      await loadDrawerDetail(targetId);
    }
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Thay đổi trạng thái tài khoản thất bại.', 'error');
  } finally {
    statusBusy.value = false;
  }
}

// ── Modal duyệt/từ chối teacher ──
const reviewTarget = ref<AdminUserDto | null>(null);
const reviewAction = ref<'approve' | 'reject'>('approve');
const rejectReason = ref('');
const rejectError = ref('');

// ── Drawer chi tiết user ──
const drawerUser = ref<AdminUserDto | null>(null);
const drawerDetail = ref<AdminUserDto | null>(null);
const drawerLoading = ref(false);
const drawerError = ref(false);

const hasReviewInfo = computed(() => {
  const u = reviewTarget.value;
  return Boolean(u && (u.department || u.staffCode || u.academicDegree || u.profileLink || u.teacherBio));
});

onMounted(load);

async function load(): Promise<void> {
  loading.value = true;
  loadError.value = false;
  try {
    const page = await adminApi.fetchUsers({
      role: tab.value === 'pending' ? 'TEACHER_PENDING' : undefined,
      q: search.value || undefined,
      page: 1,
    });
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

const stripBlocks = computed<boolean[]>(() => {
  const count = Math.min(pendingCount.value, 5);
  const size = Math.max(count, 1);
  return Array.from({ length: size }, (_, i) => i < count);
});

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
    const targetId = reviewTarget.value.id;
    await adminApi.approveTeacher(targetId, {
      approve: reviewAction.value === 'approve',
      reason: reviewAction.value === 'reject' ? rejectReason.value.trim() : undefined,
    });
    ui.showToast(reviewAction.value === 'approve' ? messages.admin.users.toastApproved : messages.admin.users.toastRejected, 'success');
    reviewTarget.value = null;
    await load();
    if (drawerUser.value?.id === targetId) {
      await loadDrawerDetail(targetId);
    }
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.admin.users.toastActionFailed, 'error');
  }
}

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
</script>

<template>
  <main class="admin-users container">
    <!-- Banner: surface band level-2 -->
    <header class="admin-users__hero">
      <div class="admin-users__hero-inner">
        <div class="admin-users__hero-main">
          <div class="admin-users__hero-badges">
            <Badge variant="primary">{{ messages.admin.badge }}</Badge>
          </div>
          <h1 class="admin-users__title">{{ messages.admin.users.title }}</h1>
          <p class="admin-users__sub">{{ messages.admin.users.subtitle }}</p>
        </div>

        <div class="flex items-center gap-3">
          <Button variant="primary" size="sm" class="gap-1.5" @click="openCreateModal">
            <UserPlus :size="15" /> + Thêm Người Dùng
          </Button>
        </div>
      </div>
    </header>

    <AdminNav active="users" />

    <!-- Tabs: Tất cả / Chờ duyệt Teacher -->
    <Tabs :tabs="userTabs" :model-value="tab" @change="switchTab" />

    <!-- Filter & Search Toolbar -->
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
      <select v-model="roleFilter" class="admin-users__select" :aria-label="messages.admin.users.roleFilterLabel">
        <option value="">{{ messages.admin.users.roleAll }}</option>
        <option value="STUDENT">{{ messages.admin.users.roleStudent }}</option>
        <option value="TEACHER">{{ messages.admin.users.roleTeacher }}</option>
        <option value="TEACHER_PENDING">{{ messages.admin.users.rolePending }}</option>
        <option value="ADMIN">{{ messages.admin.users.roleAdmin }}</option>
      </select>
      <select v-model="statusFilter" class="admin-users__select" :aria-label="messages.admin.users.statusFilterLabel">
        <option value="">{{ messages.admin.users.statusAll }}</option>
        <option value="active">{{ messages.admin.users.statusActive }}</option>
        <option value="locked">{{ messages.admin.users.statusLocked }}</option>
      </select>
      <Button size="sm" variant="secondary" class="admin-users__search-btn" @click="load">
        <Search :size="16" /> {{ messages.admin.users.search }}
      </Button>
    </div>

    <!-- Loading State -->
    <div v-if="loading" class="admin-users__loading" aria-busy="true">
      <Skeleton v-for="i in 6" :key="i" height="56px" />
    </div>

    <!-- Error State -->
    <div v-else-if="loadError" class="admin-users__error" role="alert">
      <p class="admin-users__error-text">{{ messages.admin.users.loadError }}</p>
      <Button size="sm" variant="secondary" @click="load">
        <RefreshCw :size="14" /> {{ messages.admin.users.retry }}
      </Button>
    </div>

    <!-- Empty State -->
    <EmptyState
      v-else-if="filtered.length === 0"
      icon="user"
      :title="tab === 'pending' ? messages.admin.users.emptyPending : messages.admin.users.emptyTitle"
      :description="tab === 'pending' ? messages.admin.users.emptyPendingDesc : messages.admin.users.emptyTitleDesc"
    />

    <!-- User Data Table -->
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
              <!-- User Info -->
              <td :data-label="messages.admin.users.colUser">
                <div class="admin-users__user">
                  <span class="admin-users__avatar" aria-hidden="true">{{ initial(user.displayName) }}</span>
                  <div class="admin-users__user-meta">
                    <p class="admin-users__name flex items-center gap-1.5">
                      {{ user.displayName }}
                      <span v-if="isSelf(user)" class="text-[10px] bg-vdsa-accent/20 text-vdsa-accent px-1.5 py-0.2 rounded font-bold">Bạn</span>
                    </p>
                    <p class="admin-users__email">{{ user.email }}</p>
                  </div>
                </div>
              </td>

              <!-- Role -->
              <td :data-label="messages.admin.users.colRole">
                <Badge :variant="user.role === 'ADMIN' ? 'success' : user.role === 'TEACHER_PENDING' ? 'warning' : 'primary'">
                  {{ roleLabel[user.role] }}
                </Badge>
              </td>

              <!-- Status -->
              <td :data-label="messages.admin.users.colStatus">
                <Badge :variant="user.isActive ? 'success' : 'danger'">
                  {{ user.isActive ? messages.admin.users.active : messages.admin.users.locked }}
                </Badge>
              </td>

              <!-- Date -->
              <td :data-label="messages.admin.users.colCreated" class="admin-users__date">{{ formatDate(user.createdAt) }}</td>

              <!-- Actions -->
              <td :data-label="messages.admin.users.colActions">
                <div class="admin-users__actions" @click.stop>
                  <!-- Case 1: Teacher Pending Review -->
                  <template v-if="user.role === 'TEACHER_PENDING'">
                    <Button size="sm" variant="secondary" @click="openReview(user, 'approve')">
                      <Check :size="15" /> {{ messages.admin.users.approve }}
                    </Button>
                    <Button size="sm" variant="danger" @click="openReview(user, 'reject')">
                      <X :size="15" /> {{ messages.admin.users.reject }}
                    </Button>
                  </template>

                  <!-- Case 2: Peer Admin Protection (Ngang Role Admin) -->
                  <template v-else-if="isTargetAdmin(user)">
                    <span class="text-xs text-vdsa-muted italic flex items-center gap-1 px-2 py-1 bg-vdsa-surface rounded-lg border border-vdsa-border/60">
                      <ShieldCheck :size="13" class="text-vdsa-accent" /> Quản trị viên
                    </span>
                  </template>

                  <!-- Case 3: Standard User Actions (Student / Teacher) -->
                  <template v-else>
                    <Button size="sm" variant="secondary" title="Chỉnh sửa thông tin" @click="openEditModal(user)">
                      <Pencil :size="14" />
                    </Button>
                    <Button size="sm" variant="secondary" title="Đặt lại mật khẩu" @click="openResetModal(user)">
                      <KeyRound :size="14" />
                    </Button>
                    <Button
                      v-if="user.isActive"
                      size="sm"
                      variant="danger"
                      title="Vô hiệu hóa tài khoản"
                      @click="openStatusModal(user, false)"
                    >
                      <Ban :size="14" />
                    </Button>
                    <Button
                      v-else
                      size="sm"
                      variant="primary"
                      title="Kích hoạt lại tài khoản"
                      @click="openStatusModal(user, true)"
                    >
                      <CheckCircle2 :size="14" />
                    </Button>
                  </template>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- ═══ MODAL 1: TẠO NGƯỜI DÙNG MỚI ═══ -->
    <Modal :open="createModalOpen" title="Tạo tài khoản người dùng mới" @close="createModalOpen = false">
      <form class="space-y-4" @submit.prevent="handleCreateUser">
        <Input v-model="createForm.displayName" label="Họ và tên" placeholder="Nguyễn Văn A" required />
        <Input v-model="createForm.email" type="email" label="Địa chỉ Email" placeholder="user@example.com" required />
        <Input v-model="createForm.password" type="password" label="Mật khẩu ban đầu" placeholder="Tối thiểu 6 ký tự" required />

        <div>
          <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Vai trò (Role)</label>
          <select
            v-model="createForm.role"
            class="w-full bg-vdsa-surface border border-vdsa-border rounded-xl px-3 py-2.5 text-sm text-white focus:outline-none focus:border-accent"
          >
            <option value="STUDENT">Học viên (Student)</option>
            <option value="TEACHER">Giảng viên (Teacher)</option>
            <option value="ADMIN">Quản trị viên (Admin)</option>
          </select>
        </div>

        <div v-if="createForm.role === 'TEACHER'" class="grid grid-cols-2 gap-3 p-3.5 rounded-xl bg-vdsa-surface border border-vdsa-border">
          <Input v-model="createForm.department" label="Khoa / Bộ môn" placeholder="Khoa CNTT" />
          <Input v-model="createForm.staffCode" label="Mã Giảng viên *" placeholder="GV00123" required />
        </div>

        <div class="flex items-center justify-end gap-3 pt-4 border-t border-vdsa-border">
          <Button variant="ghost" type="button" @click="createModalOpen = false">Hủy</Button>
          <Button variant="primary" type="submit" :loading="creating">Tạo tài khoản</Button>
        </div>
      </form>
    </Modal>

    <!-- ═══ MODAL 2: CHỈNH SỬA NGƯỜI DÙNG ═══ -->
    <Modal :open="editModalOpen" title="Chỉnh sửa thông tin người dùng" @close="editModalOpen = false">
      <form class="space-y-4" @submit.prevent="handleUpdateUser">
        <Input v-model="editForm.displayName" label="Họ và tên" required />

        <div v-if="!isTargetAdmin(editingTarget)" class="grid grid-cols-2 gap-3">
          <div>
            <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Vai trò</label>
            <select
              v-model="editForm.role"
              class="w-full bg-vdsa-surface border border-vdsa-border rounded-xl px-3 py-2.5 text-sm text-white focus:outline-none focus:border-accent"
            >
              <option value="STUDENT">Học viên (Student)</option>
              <option value="TEACHER">Giảng viên (Teacher)</option>
              <option value="TEACHER_PENDING">Chờ duyệt GV</option>
            </select>
          </div>

          <div>
            <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Trạng thái</label>
            <select
              v-model="editForm.isActive"
              class="w-full bg-vdsa-surface border border-vdsa-border rounded-xl px-3 py-2.5 text-sm text-white focus:outline-none focus:border-accent"
            >
              <option :value="true">Hoạt động (Active)</option>
              <option :value="false">Bị khóa (Locked)</option>
            </select>
          </div>
        </div>

        <div class="grid grid-cols-2 gap-3">
          <Input v-model="editForm.department" label="Khoa / Đơn vị" placeholder="Khoa CNTT" />
          <Input v-model="editForm.staffCode" label="Mã Cán bộ / MSSV" placeholder="SE12345" />
        </div>

        <Input v-model="editForm.academicDegree" label="Học vị / Học hàm" placeholder="Tiến sĩ / Thạc sĩ..." />
        <Input v-model="editForm.profileLink" label="Link hồ sơ / Website" placeholder="https://..." />
        <Input v-model="editForm.teacherBio" label="Giới thiệu bản thân" placeholder="Thông tin tóm tắt..." />

        <div class="flex items-center justify-end gap-3 pt-4 border-t border-vdsa-border">
          <Button variant="ghost" type="button" @click="editModalOpen = false">Hủy</Button>
          <Button variant="primary" type="submit" :loading="editing">Lưu thay đổi</Button>
        </div>
      </form>
    </Modal>

    <!-- ═══ MODAL 3: ĐẶT LẠI MẬT KHẨU ═══ -->
    <Modal :open="resetModalOpen" title="Đặt lại mật khẩu người dùng" @close="resetModalOpen = false">
      <form class="space-y-4" @submit.prevent="handleResetPassword">
        <p class="text-xs text-vdsa-muted">
          Đặt lại mật khẩu mới cho tài khoản <strong>{{ resetTarget?.displayName }}</strong> ({{ resetTarget?.email }}).
        </p>

        <Input v-model="newPassword" type="password" label="Mật khẩu mới" placeholder="Nhập mật khẩu mới (tối thiểu 6 ký tự)" required />

        <div class="flex items-center justify-end gap-3 pt-4 border-t border-vdsa-border">
          <Button variant="ghost" type="button" @click="resetModalOpen = false">Hủy</Button>
          <Button variant="primary" type="submit" :loading="resetting">Đổi mật khẩu</Button>
        </div>
      </form>
    </Modal>

    <!-- ═══ MODAL 4: PHÊ DUYỆT GIẢNG VIÊN ═══ -->
    <Modal
      :open="reviewTarget !== null"
      :title="reviewAction === 'approve' ? messages.admin.users.approveTitle : messages.admin.users.rejectTitle"
      @close="reviewTarget = null"
    >
      <div class="space-y-4">
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
        <div class="flex items-center justify-end gap-3 pt-4 border-t border-vdsa-border">
          <Button variant="ghost" @click="reviewTarget = null">{{ messages.admin.users.cancel }}</Button>
          <Button
            :variant="reviewAction === 'approve' ? 'primary' : 'danger'"
            @click="submitReview"
          >
            {{ reviewAction === 'approve' ? messages.admin.users.confirmApprove : messages.admin.users.confirmReject }}
          </Button>
        </div>
      </div>
    </Modal>

    <!-- ═══ MODAL 5: VÔ HIỆU HÓA / KÍCH HOẠT LẠI TÀI KHOẢN (A4) ═══ -->
    <Modal
      :open="statusModalOpen"
      :title="statusNextActive ? 'Kích hoạt lại tài khoản' : 'Vô hiệu hóa tài khoản'"
      @close="statusModalOpen = false"
    >
      <div class="space-y-4">
        <div
          class="p-4 rounded-xl"
          :class="statusNextActive ? 'bg-indigo-500/10 border border-indigo-500/30' : 'bg-rose-500/10 border border-rose-500/30'"
        >
          <p class="text-sm font-medium text-white">
            <template v-if="!statusNextActive">
              Vô hiệu hóa tài khoản <strong>"{{ statusTarget?.displayName }}"</strong> ({{ statusTarget?.email }})?
              <br /><br />
              <span class="text-xs text-rose-300">
                Người dùng sẽ không thể đăng nhập vào hệ thống cho đến khi được quản trị viên kích hoạt lại.
              </span>
            </template>
            <template v-else>
              Kích hoạt lại tài khoản <strong>"{{ statusTarget?.displayName }}"</strong> ({{ statusTarget?.email }})?
              <br /><br />
              <span class="text-xs text-indigo-300">
                Người dùng sẽ có thể đăng nhập bình thường vào hệ thống.
              </span>
            </template>
          </p>
        </div>

        <div class="flex items-center justify-end gap-3 pt-4 border-t border-vdsa-border">
          <Button variant="ghost" type="button" :disabled="statusBusy" @click="statusModalOpen = false">
            Hủy
          </Button>
          <Button
            :variant="statusNextActive ? 'primary' : 'danger'"
            type="button"
            :loading="statusBusy"
            @click="handleConfirmStatus"
          >
            {{ statusNextActive ? 'Kích hoạt lại' : 'Vô hiệu hóa tài khoản' }}
          </Button>
        </div>
      </div>
    </Modal>

    <!-- ═══ DRAWER CHI TIẾT USER (STATS & HOẠT ĐỘNG) ═══ -->
    <Drawer :open="drawerUser !== null" :title="messages.admin.users.drawerTitle" :width="'440px'" @close="closeDrawer">
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
          <span class="admin-users__drawer-avatar" aria-hidden="true">{{ initial(drawerDetail.displayName) }}</span>
          <div class="admin-users__drawer-head-meta">
            <p class="admin-users__drawer-name">{{ drawerDetail.displayName }}</p>
            <p class="admin-users__drawer-email">{{ drawerDetail.email }}</p>
            <div class="admin-users__drawer-badges">
              <Badge :variant="drawerDetail.role === 'ADMIN' ? 'success' : drawerDetail.role === 'TEACHER_PENDING' ? 'warning' : 'primary'">
                {{ roleLabel[drawerDetail.role] }}
              </Badge>
              <Badge :variant="drawerDetail.isActive ? 'success' : 'danger'">
                {{ drawerDetail.isActive ? messages.admin.users.active : messages.admin.users.locked }}
              </Badge>
            </div>
          </div>
        </div>

        <section class="admin-users__drawer-section">
          <h3 class="admin-users__drawer-section-title">{{ messages.admin.users.sectionProfile }}</h3>
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
        </section>

        <section class="admin-users__drawer-section">
          <h3 class="admin-users__drawer-section-title">
            <GraduationCap :size="14" aria-hidden="true" />
            {{ messages.admin.users.sectionLearning }}
          </h3>
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
        </section>

        <section class="admin-users__drawer-section">
          <h3 class="admin-users__drawer-section-title">{{ messages.admin.users.sectionActivity }}</h3>
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
        </section>

        <!-- Drawer Quick Actions -->
        <section v-if="canManageUser(drawerDetail)" class="admin-users__drawer-section">
          <h3 class="admin-users__drawer-section-title">{{ messages.admin.users.colActions }}</h3>
          <div class="admin-users__drawer-actions">
            <Button size="sm" variant="secondary" @click="openEditModal(drawerDetail)">
              <Pencil :size="14" /> Sửa thông tin
            </Button>
            <Button size="sm" variant="secondary" @click="openResetModal(drawerDetail)">
              <KeyRound :size="14" /> Đổi mật khẩu
            </Button>
            <Button
              v-if="drawerDetail.isActive"
              size="sm"
              variant="danger"
              class="gap-1.5"
              @click="openStatusModal(drawerDetail, false)"
            >
              <Ban :size="14" /> Vô hiệu hóa tài khoản
            </Button>
            <Button
              v-else
              size="sm"
              variant="primary"
              class="gap-1.5"
              @click="openStatusModal(drawerDetail, true)"
            >
              <CheckCircle2 :size="14" /> Kích hoạt lại tài khoản
            </Button>
          </div>
        </section>
        <section v-else class="admin-users__drawer-section">
          <div class="p-3 rounded-xl bg-vdsa-surface border border-vdsa-border text-xs text-vdsa-muted italic flex items-center gap-2">
            <ShieldCheck :size="16" class="text-vdsa-accent" />
            <span>Tài khoản Quản trị viên cùng cấp — Không thể thay đổi trạng thái hoặc xóa.</span>
          </div>
        </section>
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
  max-width: 1200px;
}

/* ── Banner: surface band level-2 ── */
.admin-users__hero {
  border: 1px solid var(--border-subtle, rgba(255, 255, 255, 0.08));
  background: var(--card-raised, #161b22);
  border-radius: var(--radius-lg, 16px);
  padding: var(--space-xl, 24px);
}

.admin-users__hero-inner {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-lg, 20px);
  flex-wrap: wrap;
}

.admin-users__hero-main {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs, 6px);
}

.admin-users__hero-badges {
  display: flex;
  align-items: center;
  gap: var(--space-xs, 6px);
}

.admin-users__title {
  font-size: var(--text-2xl, 24px);
  font-weight: 800;
  color: var(--color-text-primary, #ffffff);
  line-height: 1.2;
}

.admin-users__sub {
  font-size: var(--text-sm, 13px);
  color: var(--color-text-secondary, #8b949e);
}

/* ── Bộ lọc & Tìm kiếm ── */
.admin-users__filters {
  display: flex;
  align-items: center;
  gap: var(--space-sm, 8px);
  flex-wrap: wrap;
}

.admin-users__search-box {
  position: relative;
  flex: 1;
  min-width: 220px;
}

.admin-users__search-icon {
  position: absolute;
  left: 12px;
  top: 50%;
  transform: translateY(-50%);
  color: var(--color-text-tertiary, #8b949e);
  pointer-events: none;
}

.admin-users__search {
  width: 100%;
  height: 38px;
  padding-left: 36px;
  padding-right: 12px;
  background: var(--color-surface, #161b22);
  border: 1px solid var(--color-border, #30363d);
  border-radius: var(--radius-md, 8px);
  color: var(--color-text-primary, #ffffff);
  font-size: var(--text-sm, 13px);
}

.admin-users__select {
  height: 38px;
  padding: 0 12px;
  background: var(--color-surface, #161b22);
  border: 1px solid var(--color-border, #30363d);
  border-radius: var(--radius-md, 8px);
  color: var(--color-text-primary, #ffffff);
  font-size: var(--text-sm, 13px);
}

/* ── Bảng dữ liệu ── */
.admin-users__table {
  border: 1px solid var(--color-border, #30363d);
  border-radius: var(--radius-lg, 12px);
  overflow: hidden;
  background: var(--color-card, #0d1117);
}

.admin-users__table-scroll {
  overflow-x: auto;
  -webkit-overflow-scrolling: touch;
}

table {
  width: 100%;
  min-width: 750px;
  border-collapse: collapse;
  text-align: left;
}

th {
  height: 44px;
  padding: 0 16px;
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-tertiary, #8b949e);
  background: var(--color-muted, #161b22);
  border-bottom: 1px solid var(--color-border, #30363d);
}

td {
  padding: 12px 16px;
  font-size: 13px;
  border-bottom: 1px solid var(--color-border, #30363d);
}

.admin-users__row:hover td {
  background: var(--color-hover, rgba(255, 255, 255, 0.02));
}

.admin-users__user {
  display: flex;
  align-items: center;
  gap: 12px;
}

.admin-users__avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--color-accent, #6366f1);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 14px;
  flex-shrink: 0;
}

.admin-users__name {
  font-weight: 600;
  color: var(--color-text-primary, #ffffff);
}

.admin-users__email {
  font-size: 11px;
  color: var(--color-text-tertiary, #8b949e);
}

.admin-users__date {
  font-family: var(--font-mono, monospace);
  font-size: 12px;
  color: var(--color-text-secondary, #8b949e);
}

.admin-users__actions {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-wrap: wrap;
}

/* ── Modal Review & Drawer ── */
.admin-users__review {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px;
  background: var(--color-surface, #161b22);
  border-radius: 8px;
  margin-bottom: 12px;
}

.admin-users__review-avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--color-muted, #21262d);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-primary, #a855f7);
}

.admin-users__review-name { font-weight: 600; color: #ffffff; }
.admin-users__review-email { font-size: 12px; color: #8b949e; }

.admin-users__review-info {
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding: 12px;
  background: var(--color-surface, #161b22);
  border-radius: 8px;
  margin-bottom: 12px;
  font-size: 13px;
}

.admin-users__review-info-row {
  display: flex;
  justify-content: space-between;
  gap: 12px;
}

.admin-users__review-info-label { color: #8b949e; font-weight: 500; }
.admin-users__review-info-value { color: #ffffff; font-weight: 600; text-align: right; }

.admin-users__drawer {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.admin-users__drawer-head {
  display: flex;
  align-items: center;
  gap: 16px;
  padding-bottom: 16px;
  border-bottom: 1px solid var(--color-border, #30363d);
}

.admin-users__drawer-avatar {
  width: 52px;
  height: 52px;
  border-radius: 50%;
  background: var(--color-accent, #6366f1);
  color: #ffffff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 20px;
  font-weight: 800;
}

.admin-users__drawer-name { font-size: 16px; font-weight: 700; color: #ffffff; }
.admin-users__drawer-email { font-size: 13px; color: #8b949e; margin-bottom: 6px; }
.admin-users__drawer-badges { display: flex; gap: 6px; }

.admin-users__drawer-section-title {
  font-size: 12px;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #8b949e;
  margin-bottom: 10px;
  display: flex;
  align-items: center;
  gap: 6px;
}

.admin-users__drawer-info {
  display: flex;
  flex-direction: column;
  gap: 8px;
  background: var(--color-surface, #161b22);
  border: 1px solid var(--color-border, #30363d);
  border-radius: 10px;
  padding: 12px;
}

.admin-users__drawer-row {
  display: flex;
  justify-content: space-between;
  font-size: 13px;
}

.admin-users__drawer-label { color: #8b949e; }
.admin-users__drawer-value { color: #ffffff; font-weight: 600; }
.admin-users__drawer-value--mono { font-family: monospace; }

.admin-users__drawer-stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 8px;
}

.admin-users__drawer-stat {
  background: var(--color-surface, #161b22);
  border: 1px solid var(--color-border, #30363d);
  border-radius: 10px;
  padding: 10px;
  text-align: center;
}

.admin-users__drawer-stat-value {
  display: block;
  font-size: 18px;
  font-weight: 800;
  color: #ffffff;
}

.admin-users__drawer-stat-label {
  font-size: 11px;
  color: #8b949e;
  font-weight: 500;
}

.admin-users__drawer-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

/* ── Responsive Mobile & Tablet ── */
@media (max-width: 768px) {
  .admin-users {
    padding-inline: var(--space-sm, 12px);
    gap: var(--space-md, 16px);
  }

  .admin-users__hero {
    padding: var(--space-md, 16px);
  }

  .admin-users__hero-inner {
    flex-direction: column;
    align-items: stretch;
  }

  .admin-users__filters {
    flex-direction: column;
    align-items: stretch;
  }

  .admin-users__search-box {
    min-width: 100%;
    width: 100%;
  }

  .admin-users__select {
    width: 100%;
  }

  .admin-users__drawer-stats {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 480px) {
  .admin-users__drawer-stats {
    grid-template-columns: 1fr;
  }
}
</style>
