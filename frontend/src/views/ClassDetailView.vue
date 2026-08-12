<script setup lang="ts">
// ClassDetailView — Màn 20: 3 tab (Thành viên / Lộ trình đã gán / Cài đặt)
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { useClassStore } from '@/stores/classStore';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import { formatDate } from '@/utils/format';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';

const route = useRoute();
const router = useRouter();
const classStore = useClassStore();
const auth = useAuthStore();
const ui = useUiStore();

const classId = computed(() => Number(route.params.id));
const tab = ref<'members' | 'assignments' | 'settings'>('members');
const loading = ref(true);

const confirmRemove = ref<number | null>(null);
const addEmail = ref('');
const addMemberOpen = ref(false);
const assignOpen = ref(false);
const assignDue = ref('');

const isManager = computed(() => {
  const cls = classStore.currentClass;
  return cls?.role === 'OWNER' || cls?.role === 'TEACHER' || auth.role === 'ADMIN';
});

onMounted(load);

async function load(): Promise<void> {
  loading.value = true;
  try {
    await classStore.fetchClass(classId.value);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể tải chi tiết lớp.', 'error');
    void router.push({ name: 'classes' });
  } finally {
    loading.value = false;
  }
}

async function removeMember(userId: number): Promise<void> {
  try {
    await classStore.removeMember(classId.value, userId);
    ui.showToast('Đã gỡ thành viên.', 'success');
    confirmRemove.value = null;
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Gỡ thành viên thất bại.', 'error');
  }
}

async function addMember(): Promise<void> {
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(addEmail.value)) {
    ui.showToast('Email không hợp lệ.', 'warning');
    return;
  }
  try {
    const { addClassMember } = await import('@/api/classes');
    await addClassMember(classId.value, addEmail.value.trim());
    ui.showToast('Đã thêm thành viên (nếu email tồn tại trong hệ thống).', 'success');
    addEmail.value = '';
    addMemberOpen.value = false;
    await classStore.reloadMembers(classId.value);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Thêm thành viên thất bại.', 'error');
  }
}

async function createAssignment(): Promise<void> {
  try {
    await classStore.assignContent({
      classId: classId.value,
      exerciseId: null,
      lessonId: null,
      dueAt: assignDue.value ? new Date(assignDue.value).toISOString() : null,
    });
    ui.showToast('Đã gán nội dung cho lớp.', 'success');
    assignOpen.value = false;
    await classStore.reloadAssignments(classId.value);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Gán nội dung thất bại.', 'error');
  }
}

async function deleteClass(): Promise<void> {
  const name = classStore.currentClass?.name ?? '';
  if (!window.confirm(`Xóa lớp "${name}"? Hành động này không thể hoàn tác.`)) return;
  try {
    await classStore.removeClass(classId.value);
    ui.showToast('Đã xóa lớp.', 'success');
    void router.push({ name: 'classes' });
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Xóa lớp thất bại.', 'error');
  }
}

function copyInvite(): void {
  const code = classStore.currentClass?.inviteCode;
  if (!code) return;
  void navigator.clipboard?.writeText(code).then(() => {
    ui.showToast('Đã sao chép mã mời!', 'success');
  });
}
</script>

<template>
  <main class="class-detail container">
    <nav class="class-detail__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'classes' }">Lớp học</RouterLink>
      <span aria-hidden="true">/</span>
      <span>{{ classStore.currentClass?.name ?? 'Chi tiết lớp' }}</span>
    </nav>

    <div v-if="loading" class="class-detail__loading">
      <Skeleton v-for="i in 5" :key="i" height="44px" />
    </div>

    <template v-else-if="classStore.currentClass">
      <header class="class-detail__header card">
        <div>
          <h1 class="class-detail__title">{{ classStore.currentClass.name }}</h1>
          <p class="text-muted class-detail__desc">{{ classStore.currentClass.description || 'Chưa có mô tả' }}</p>
        </div>
        <div class="class-detail__meta">
          <span class="class-detail__invite">
            Mã mời: <code>{{ classStore.currentClass.inviteCode }}</code>
            <button
              v-if="isManager"
              type="button"
              class="class-detail__copy"
              @click="copyInvite"
            >
              Copy
            </button>
          </span>
          <span class="text-muted">{{ classStore.members.length }} thành viên</span>
          <RouterLink :to="{ name: 'class-report', params: { id: String(classId) } }">
            <Button v-if="isManager" size="sm" variant="secondary">Báo cáo lớp →</Button>
          </RouterLink>
        </div>
      </header>

      <div class="class-detail__tabs">
        <button type="button" class="class-detail__tab" :class="{ 'class-detail__tab--active': tab === 'members' }" @click="tab = 'members'">
          Thành viên
        </button>
        <button type="button" class="class-detail__tab" :class="{ 'class-detail__tab--active': tab === 'assignments' }" @click="tab = 'assignments'">
          Lộ trình đã gán
        </button>
        <button v-if="isManager" type="button" class="class-detail__tab" :class="{ 'class-detail__tab--active': tab === 'settings' }" @click="tab = 'settings'">
          Cài đặt
        </button>
      </div>

      <!-- Tab Thành viên -->
      <section v-if="tab === 'members'" class="class-detail__panel">
        <div v-if="isManager" class="class-detail__toolbar">
          <Button size="sm" @click="addMemberOpen = true">+ Thêm thành viên</Button>
        </div>
        <EmptyState
          v-if="classStore.members.length === 0"
          icon="user"
          title="Chưa có thành viên"
          description="Mời học viên bằng mã lớp hoặc thêm bằng email."
        />
        <div v-else class="class-detail__table card">
          <table>
            <thead>
              <tr>
                <th>Thành viên</th>
                <th>Vai trò</th>
                <th>Ngày tham gia</th>
                <th v-if="isManager">Thao tác</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="member in classStore.members" :key="member.id">
                <td>
                  <p class="class-detail__name">{{ member.displayName }}</p>
                  <p class="text-muted class-detail__email">{{ member.email }}</p>
                </td>
                <td><Badge :variant="member.role === 'TEACHER' ? 'primary' : 'muted'">{{ member.role === 'TEACHER' ? 'Giảng viên' : 'Học viên' }}</Badge></td>
                <td class="text-muted">{{ formatDate(member.joinedAt) }}</td>
                <td v-if="isManager">
                  <Button size="sm" variant="danger" @click="confirmRemove = member.id">Gỡ</Button>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- Tab Lộ trình đã gán -->
      <section v-else-if="tab === 'assignments'" class="class-detail__panel">
        <div v-if="isManager" class="class-detail__toolbar">
          <Button size="sm" @click="assignOpen = true">+ Gán nội dung</Button>
        </div>
        <EmptyState
          v-if="classStore.assignments.length === 0"
          icon="book"
          title="Chưa gán nội dung"
          description="Gán bài học/bài tập kèm hạn nộp cho lớp."
        />
        <div v-else class="class-detail__assignments">
          <div v-for="assign in classStore.assignments" :key="assign.id" class="class-detail__assign card">
            <div>
              <p class="class-detail__assign-title">
                {{ assign.lessonId ? `Bài học #${assign.lessonId}` : assign.exerciseId ? `Bài tập #${assign.exerciseId}` : 'Nội dung chung' }}
              </p>
              <p class="text-muted class-detail__assign-due">
                Hạn: {{ assign.dueAt ? formatDate(assign.dueAt) : 'Không giới hạn' }}
              </p>
            </div>
            <Badge :variant="assign.status === 'open' ? 'success' : 'danger'">{{ assign.status === 'open' ? 'Đang mở' : 'Đã đóng' }}</Badge>
          </div>
        </div>
      </section>

      <!-- Tab Cài đặt -->
      <section v-else class="class-detail__panel">
        <div class="class-detail__settings card">
          <h2 class="class-detail__settings-title">Nguy hiểm</h2>
          <p class="text-muted class-detail__settings-note">
            Xóa lớp sẽ ẩn lớp khỏi mọi thành viên (xóa mềm).
          </p>
          <Button variant="danger" @click="deleteClass">Xóa lớp này</Button>
        </div>
      </section>
    </template>

    <!-- Modal gỡ thành viên -->
    <Modal :open="confirmRemove !== null" title="Gỡ thành viên" @close="confirmRemove = null">
      <p class="class-detail__modal-text">Bạn chắc chắn muốn gỡ thành viên này khỏi lớp?</p>
      <template #footer>
        <Button variant="ghost" @click="confirmRemove = null">Hủy</Button>
        <Button variant="danger" @click="removeMember(confirmRemove ?? 0)">Gỡ</Button>
      </template>
    </Modal>

    <!-- Modal thêm thành viên -->
    <Modal :open="addMemberOpen" title="Thêm thành viên" @close="addMemberOpen = false">
      <form novalidate @submit.prevent="addMember">
        <Input v-model="addEmail" label="Email học viên" type="email" placeholder="sinhvien@truong.edu.vn" required />
        <div class="class-detail__modal-actions">
          <Button variant="ghost" @click="addMemberOpen = false">Hủy</Button>
          <Button type="submit">Thêm</Button>
        </div>
      </form>
    </Modal>

    <!-- Modal gán nội dung -->
    <Modal :open="assignOpen" title="Gán nội dung cho lớp" @close="assignOpen = false">
      <form novalidate @submit.prevent="createAssignment">
        <label class="label">Hạn nộp</label>
        <input v-model="assignDue" class="input" type="datetime-local" />
        <p class="text-muted class-detail__modal-note">
          * Gán theo exercise/lesson chi tiết sẽ bổ sung ở giai đoạn sau (backlog — cần chọn từ danh sách bài học).
        </p>
        <div class="class-detail__modal-actions">
          <Button variant="ghost" @click="assignOpen = false">Hủy</Button>
          <Button type="submit">Gán</Button>
        </div>
      </form>
    </Modal>
  </main>
</template>

<style scoped>
.class-detail {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.class-detail__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.class-detail__header {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.class-detail__title { font-size: var(--text-xl); }
.class-detail__desc { font-size: var(--text-sm); margin-top: 4px; }

.class-detail__meta { display: flex; flex-direction: column; align-items: flex-end; gap: var(--space-sm); }

.class-detail__invite code { font-family: var(--font-mono); font-weight: 700; color: var(--color-primary); }

.class-detail__copy {
  margin-left: 4px;
  background: none;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  font-size: var(--text-xs);
  padding: 2px 8px;
  cursor: pointer;
  color: var(--color-primary);
}

.class-detail__tabs { display: flex; gap: var(--space-xs); border-bottom: 2px solid var(--color-border); }

.class-detail__tab {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  padding: var(--space-sm) var(--space-md);
  font-weight: 700;
  color: var(--color-text-muted);
  cursor: pointer;
  margin-bottom: -2px;
}

.class-detail__tab--active { color: var(--color-primary); border-bottom-color: var(--color-primary); }

.class-detail__toolbar { display: flex; justify-content: flex-end; }

.class-detail__table { padding: 0; overflow-x: auto; }

.class-detail__table table { width: 100%; border-collapse: collapse; min-width: 560px; }

.class-detail__table th {
  text-align: left;
  font-size: var(--text-xs);
  text-transform: uppercase;
  color: var(--color-text-muted);
  padding: var(--space-sm) var(--space-md);
  border-bottom: 2px solid var(--color-border);
  background: var(--color-muted);
}

.class-detail__table td { padding: var(--space-sm) var(--space-md); border-bottom: 1px solid var(--color-border); font-size: var(--text-sm); }

.class-detail__name { font-weight: 700; }
.class-detail__email { font-size: var(--text-xs); }

.class-detail__assignments { display: flex; flex-direction: column; gap: var(--space-sm); }

.class-detail__assign {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-md);
}

.class-detail__assign-title { font-weight: 700; }
.class-detail__assign-due { font-size: var(--text-xs); }

.class-detail__settings { display: flex; flex-direction: column; gap: var(--space-md); max-width: 420px; border-color: var(--color-destructive); }

.class-detail__settings-title { font-size: var(--text-md); color: var(--color-destructive); }
.class-detail__settings-note { font-size: var(--text-sm); }

.class-detail__modal-text { font-size: var(--text-sm); }

.class-detail__modal-actions { display: flex; justify-content: flex-end; gap: var(--space-sm); margin-top: var(--space-md); }

.class-detail__modal-note { font-size: var(--text-xs); margin-top: 4px; }
</style>
