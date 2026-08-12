<script setup lang="ts">
// AdminUsersView — Màn 10: quản lý người dùng + tab "Chờ duyệt Teacher" (Màn 29)
import { computed, onMounted, ref } from 'vue';

import * as adminApi from '@/api/admin';
import type { AdminUserDto } from '@/api/admin';
import { useUiStore } from '@/stores/ui';
import { formatDate } from '@/utils/format';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';
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

function switchTab(next: 'all' | 'pending'): void {
  tab.value = next;
  void load();
}

const pendingCount = computed(() => users.value.filter((u) => u.role === 'TEACHER_PENDING').length);

const filtered = computed(() => {
  let list = users.value;
  if (roleFilter.value) list = list.filter((u) => u.role === roleFilter.value);
  if (statusFilter.value === 'active') list = list.filter((u) => u.isActive);
  if (statusFilter.value === 'locked') list = list.filter((u) => !u.isActive);
  return list;
});

const roleLabel: Record<string, string> = {
  STUDENT: 'Học viên',
  TEACHER: 'Giảng viên',
  TEACHER_PENDING: 'Chờ duyệt GV',
  ADMIN: 'Quản trị',
};

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
    <h1 class="admin-users__title">👥 Quản lý người dùng</h1>

    <AdminNav active="users" />

    <div class="admin-users__tabs">
      <button type="button" class="admin-users__tab" :class="{ 'admin-users__tab--active': tab === 'all' }" @click="switchTab('all')">
        Tất cả người dùng
      </button>
      <button type="button" class="admin-users__tab" :class="{ 'admin-users__tab--active': tab === 'pending' }" @click="switchTab('pending')">
        Chờ duyệt Teacher <Badge v-if="pendingCount > 0" variant="warning">{{ pendingCount }}</Badge>
      </button>
    </div>

    <div class="admin-users__filters">
      <input v-model="search" class="input admin-users__search" type="search" placeholder="Tìm theo tên/email..." aria-label="Tìm người dùng" @keyup.enter="load" />
      <select v-model="roleFilter" class="input admin-users__select" aria-label="Lọc vai trò">
        <option value="">Vai trò: Tất cả</option>
        <option value="STUDENT">Học viên</option>
        <option value="TEACHER">Giảng viên</option>
        <option value="TEACHER_PENDING">Chờ duyệt</option>
        <option value="ADMIN">Quản trị</option>
      </select>
      <select v-model="statusFilter" class="input admin-users__select" aria-label="Lọc trạng thái">
        <option value="">Trạng thái: Tất cả</option>
        <option value="active">Hoạt động</option>
        <option value="locked">Đã khóa</option>
      </select>
      <Button size="sm" variant="ghost" @click="load">Tìm</Button>
    </div>

    <div v-if="loading" class="admin-users__loading">
      <Skeleton v-for="i in 6" :key="i" height="44px" />
    </div>

    <EmptyState
      v-else-if="filtered.length === 0"
      icon="user"
      :title="tab === 'pending' ? 'Không có tài khoản chờ duyệt' : 'Không có người dùng phù hợp'"
    />

    <div v-else class="admin-users__table card">
      <table>
        <thead>
          <tr>
            <th>Người dùng</th>
            <th>Vai trò</th>
            <th>Trạng thái</th>
            <th>Ngày tạo</th>
            <th class="admin-users__actions-col">Thao tác</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="user in filtered" :key="user.id">
            <td>
              <p class="admin-users__name">{{ user.displayName }}</p>
              <p class="admin-users__email text-muted">{{ user.email }}</p>
            </td>
            <td><Badge :variant="user.role === 'TEACHER_PENDING' ? 'warning' : 'default'">{{ roleLabel[user.role] }}</Badge></td>
            <td>
              <Badge :variant="user.isActive ? 'success' : 'danger'">
                {{ user.isActive ? 'Hoạt động' : 'Đã khóa' }}
              </Badge>
            </td>
            <td class="text-muted">{{ formatDate(user.createdAt) }}</td>
            <td>
              <div class="admin-users__actions">
                <template v-if="user.role === 'TEACHER_PENDING'">
                  <Button size="sm" variant="secondary" @click="openReview(user, 'approve')">Duyệt</Button>
                  <Button size="sm" variant="danger" @click="openReview(user, 'reject')">Từ chối</Button>
                </template>
                <template v-else>
                  <Button size="sm" variant="ghost" @click="toggleLock(user)">
                    {{ user.isActive ? 'Khóa' : 'Mở khóa' }}
                  </Button>
                </template>
              </div>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <Modal
      :open="reviewTarget !== null"
      :title="reviewAction === 'approve' ? 'Duyệt giảng viên' : 'Từ chối giảng viên'"
      @close="reviewTarget = null"
    >
      <p class="admin-users__review-text">
        {{ reviewTarget?.displayName }} ({{ reviewTarget?.email }})
      </p>
      <Input
        v-if="reviewAction === 'reject'"
        v-model="rejectReason"
        label="Lý do từ chối (bắt buộc)"
        placeholder="VD: Thiếu bằng cấp/chứng chỉ..."
      />
      <template #footer>
        <Button variant="ghost" @click="reviewTarget = null">Hủy</Button>
        <Button
          :variant="reviewAction === 'approve' ? 'primary' : 'danger'"
          :disabled="reviewAction === 'reject' && rejectReason.trim() === ''"
          @click="submitReview"
        >
          {{ reviewAction === 'approve' ? 'Xác nhận duyệt' : 'Xác nhận từ chối' }}
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

.admin-users__title { font-size: var(--text-2xl); }

.admin-users__tabs { display: flex; gap: var(--space-xs); border-bottom: 2px solid var(--color-border); }

.admin-users__tab {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  padding: var(--space-sm) var(--space-md);
  font-weight: 700;
  color: var(--color-text-muted);
  cursor: pointer;
  margin-bottom: -2px;
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
}

.admin-users__tab--active { color: var(--color-primary); border-bottom-color: var(--color-primary); }

.admin-users__filters { display: flex; gap: var(--space-sm); flex-wrap: wrap; align-items: center; }

.admin-users__search { max-width: 260px; }
.admin-users__select { width: auto; }

.admin-users__table { overflow-x: auto; padding: 0; }

.admin-users__table table { width: 100%; border-collapse: collapse; min-width: 640px; }

.admin-users__table th {
  text-align: left;
  font-size: var(--text-xs);
  text-transform: uppercase;
  color: var(--color-text-muted);
  padding: var(--space-sm) var(--space-md);
  border-bottom: 2px solid var(--color-border);
  background: var(--color-muted);
}

.admin-users__table td { padding: var(--space-sm) var(--space-md); border-bottom: 1px solid var(--color-border); font-size: var(--text-sm); }

.admin-users__name { font-weight: 700; }
.admin-users__email { font-size: var(--text-xs); }

.admin-users__actions { display: flex; gap: var(--space-xs); flex-wrap: wrap; }

.admin-users__review-text { font-size: var(--text-sm); margin-bottom: var(--space-md); }
</style>
