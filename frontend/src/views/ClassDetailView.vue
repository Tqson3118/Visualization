<script setup lang="ts">
// ClassDetailView — Màn 20: 3 tab (Thành viên / Lộ trình đã gán / Cài đặt)
// H-C: hero gradient Sunset + chips mã mời (copy feedback) + Tabs shadcn + table chuẩn
// AdminUsers + xác nhận xóa bằng Modal (thay window.confirm). GIỮ nguyên logic store/API.
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  AlertTriangle,
  ArrowRight,
  BookOpen,
  CalendarClock,
  Check,
  ClipboardCopy,
  KeyRound,
  Puzzle,
  Trash2,
  UserPlus,
  Users,
} from 'lucide-vue-next';

import { useClassStore } from '@/stores/classStore';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import { formatDate } from '@/utils/format';
import { messages } from '@/i18n/vi';
import type { ClassAssignmentDto } from '@/api/types';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';
import Card from '@/components/ui/Card.vue';
import Tabs from '@/components/ui/Tabs.vue';

const route = useRoute();
const router = useRouter();
const classStore = useClassStore();
const auth = useAuthStore();
const ui = useUiStore();

const classId = computed(() => Number(route.params.id));
const tab = ref<'members' | 'assignments' | 'settings'>('members');
const loading = ref(true);

const confirmRemove = ref<number | null>(null);
const confirmDelete = ref(false);
const copied = ref(false);
const addEmail = ref('');
const addMemberOpen = ref(false);
const assignOpen = ref(false);
const assignDue = ref('');

const isManager = computed(() => {
  const cls = classStore.currentClass;
  return cls?.role === 'OWNER' || cls?.role === 'TEACHER' || auth.role === 'ADMIN';
});

const detailTabs = computed<Array<{ key: 'members' | 'assignments' | 'settings'; label: string }>>(() => {
  const tabs: Array<{ key: 'members' | 'assignments' | 'settings'; label: string }> = [
    { key: 'members', label: messages.classes.detailTabMembers },
    { key: 'assignments', label: messages.classes.detailTabAssignments },
  ];
  if (isManager.value) tabs.push({ key: 'settings', label: messages.classes.detailTabSettings });
  return tabs;
});

const initial = (name: string): string => (name.trim() ? name.trim().charAt(0).toUpperCase() : '?');

onMounted(load);

async function load(): Promise<void> {
  loading.value = true;
  try {
    await classStore.fetchClass(classId.value);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.detailLoadError, 'error');
    void router.push({ name: 'classes' });
  } finally {
    loading.value = false;
  }
}

async function removeMember(userId: number): Promise<void> {
  try {
    await classStore.removeMember(classId.value, userId);
    ui.showToast(messages.classes.detailRemoved, 'success');
    confirmRemove.value = null;
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.detailRemoveFailed, 'error');
  }
}

async function addMember(): Promise<void> {
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(addEmail.value)) {
    ui.showToast(messages.classes.detailAddInvalidEmail, 'warning');
    return;
  }
  try {
    const { addClassMember } = await import('@/api/classes');
    await addClassMember(classId.value, addEmail.value.trim());
    ui.showToast(messages.classes.detailAddSuccess, 'success');
    addEmail.value = '';
    addMemberOpen.value = false;
    await classStore.reloadMembers(classId.value);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.detailAddFailed, 'error');
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
    ui.showToast(messages.classes.detailAssigned, 'success');
    assignOpen.value = false;
    assignDue.value = '';
    await classStore.reloadAssignments(classId.value);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.detailAssignFailed, 'error');
  }
}

async function confirmDeleteClass(): Promise<void> {
  try {
    await classStore.removeClass(classId.value);
    ui.showToast(messages.classes.detailDeleted, 'success');
    confirmDelete.value = false;
    void router.push({ name: 'classes' });
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.detailDeleteFailed, 'error');
  }
}

function copyInvite(): void {
  const code = classStore.currentClass?.inviteCode;
  if (!code) return;
  void navigator.clipboard?.writeText(code).then(() => {
    ui.showToast(messages.classes.detailCopied, 'success');
    copied.value = true;
    window.setTimeout(() => {
      copied.value = false;
    }, 1600);
  });
}

function assignmentTitle(assign: ClassAssignmentDto): string {
  if (assign.lessonId !== null) return messages.classes.detailLesson(assign.lessonId);
  if (assign.exerciseId !== null) return messages.classes.detailExercise(assign.exerciseId);
  return messages.classes.detailGenericContent;
}

function assignmentTint(assign: ClassAssignmentDto): string {
  if (assign.lessonId !== null) return 'class-detail__assign-icon--mint';
  if (assign.exerciseId !== null) return 'class-detail__assign-icon--sunset';
  return 'class-detail__assign-icon--aurora';
}
</script>

<template>
  <main class="class-detail container">
    <nav class="class-detail__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'classes' }">{{ messages.classes.detailBreadcrumb }}</RouterLink>
      <span aria-hidden="true">/</span>
      <span>{{ classStore.currentClass?.name ?? messages.classes.detailFallbackTitle }}</span>
    </nav>

    <div v-if="loading" class="class-detail__loading" aria-busy="true">
      <Skeleton v-for="i in 5" :key="i" height="44px" />
    </div>

    <template v-else-if="classStore.currentClass">
      <!-- Hero gradient Sunset + mã mời (copy chip) -->
      <header class="class-detail__hero">
        <div class="class-detail__hero-top">
          <div class="class-detail__hero-main">
            <h1 class="class-detail__hero-title">{{ classStore.currentClass.name }}</h1>
            <p class="class-detail__hero-desc">{{ classStore.currentClass.description || messages.classes.noDescription }}</p>
          </div>
          <div class="class-detail__hero-badges">
            <Badge variant="primary">
              {{ isManager ? messages.classes.roleManager : messages.classes.roleMember }}
            </Badge>
            <span class="class-detail__hero-chip">
              <Users :size="13" aria-hidden="true" />
              {{ messages.classes.members(classStore.members.length) }}
            </span>
          </div>
        </div>

        <div class="class-detail__hero-actions">
          <span class="class-detail__hero-chip class-detail__hero-chip--code">
            <KeyRound :size="15" aria-hidden="true" />
            <span>{{ messages.classes.inviteLabel }}:</span>
            <code>{{ classStore.currentClass.inviteCode }}</code>
            <button
              v-if="isManager"
              type="button"
              class="class-detail__copy-btn"
              :aria-label="messages.classes.detailCopy"
              @click="copyInvite"
            >
              <Check v-if="copied" :size="12" aria-hidden="true" />
              <ClipboardCopy v-else :size="12" aria-hidden="true" />
              {{ copied ? '✓' : messages.classes.detailCopy }}
            </button>
          </span>
          <RouterLink :to="{ name: 'class-report', params: { id: String(classId) } }" class="class-detail__hero-link">
            <Button v-if="isManager" size="sm" variant="secondary">
              {{ messages.classes.detailReportBtn }} <ArrowRight :size="14" aria-hidden="true" />
            </Button>
          </RouterLink>
        </div>
      </header>

      <!-- Tabs shadcn: Thành viên / Lộ trình đã gán / Cài đặt -->
      <Tabs :tabs="detailTabs" :model-value="tab" @change="tab = $event as typeof tab" />

      <!-- Tab Thành viên -->
      <section v-if="tab === 'members'" class="class-detail__panel">
        <div v-if="isManager" class="class-detail__toolbar">
          <Button size="sm" @click="addMemberOpen = true">
            <UserPlus :size="14" aria-hidden="true" /> {{ messages.classes.detailAddMember }}
          </Button>
        </div>
        <EmptyState
          v-if="classStore.members.length === 0"
          icon="user"
          :title="messages.classes.detailEmptyMembers"
          :description="messages.classes.detailEmptyMembersDesc"
        />
        <Card v-else class="class-detail__table">
          <div class="class-detail__table-scroll">
            <table>
              <thead>
                <tr>
                  <th>{{ messages.classes.detailColMember }}</th>
                  <th>{{ messages.classes.detailColRole }}</th>
                  <th>{{ messages.classes.detailColJoined }}</th>
                  <th v-if="isManager">{{ messages.classes.detailColActions }}</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="member in classStore.members" :key="member.id">
                  <td>
                    <div class="class-detail__user">
                      <span class="class-detail__avatar" aria-hidden="true">{{ initial(member.displayName) }}</span>
                      <div class="class-detail__user-meta">
                        <p class="class-detail__name">{{ member.displayName }}</p>
                        <p class="class-detail__email text-muted">{{ member.email }}</p>
                      </div>
                    </div>
                  </td>
                  <td>
                    <Badge :variant="member.role === 'TEACHER' ? 'primary' : 'muted'">
                      {{ member.role === 'TEACHER' ? messages.classes.roleTeacher : messages.classes.roleStudent }}
                    </Badge>
                  </td>
                  <td class="class-detail__date text-muted">{{ formatDate(member.joinedAt) }}</td>
                  <td v-if="isManager">
                    <Button size="sm" variant="danger" @click="confirmRemove = member.id">
                      {{ messages.classes.detailRemove }}
                    </Button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </Card>
      </section>

      <!-- Tab Lộ trình đã gán -->
      <section v-else-if="tab === 'assignments'" class="class-detail__panel">
        <div v-if="isManager" class="class-detail__toolbar">
          <Button size="sm" @click="assignOpen = true">
            <BookOpen :size="14" aria-hidden="true" /> {{ messages.classes.detailAssignBtn }}
          </Button>
        </div>
        <EmptyState
          v-if="classStore.assignments.length === 0"
          icon="book"
          :title="messages.classes.detailEmptyAssign"
          :description="messages.classes.detailEmptyAssignDesc"
        />
        <div v-else class="class-detail__assignments">
          <Card v-for="assign in classStore.assignments" :key="assign.id" class="class-detail__assign hover-lift">
            <span class="class-detail__assign-icon" :class="assignmentTint(assign)" aria-hidden="true">
              <Puzzle v-if="assign.exerciseId !== null" :size="18" />
              <BookOpen v-else :size="18" />
            </span>
            <div class="class-detail__assign-info">
              <p class="class-detail__assign-title">{{ assignmentTitle(assign) }}</p>
              <p class="class-detail__assign-due text-muted">
                <CalendarClock :size="13" aria-hidden="true" />
                {{ assign.dueAt ? messages.classes.detailDue(formatDate(assign.dueAt)) : messages.classes.detailDueNone }}
              </p>
            </div>
            <Badge :variant="assign.status === 'open' ? 'success' : 'danger'">
              {{ assign.status === 'open' ? messages.classes.detailOpen : messages.classes.detailClosed }}
            </Badge>
          </Card>
        </div>
      </section>

      <!-- Tab Cài đặt -->
      <section v-else class="class-detail__panel">
        <Card class="class-detail__settings">
          <div class="class-detail__settings-head">
            <span class="class-detail__settings-icon" aria-hidden="true"><AlertTriangle :size="18" /></span>
            <div>
              <h2 class="class-detail__settings-title">{{ messages.classes.detailSettingsDanger }}</h2>
              <p class="class-detail__settings-note text-muted">{{ messages.classes.detailSettingsNote }}</p>
            </div>
          </div>
          <Button variant="danger" @click="confirmDelete = true">
            <Trash2 :size="14" aria-hidden="true" /> {{ messages.classes.detailDeleteBtn }}
          </Button>
        </Card>
      </section>
    </template>

    <!-- Modal gỡ thành viên -->
    <Modal :open="confirmRemove !== null" :title="messages.classes.detailRemoveTitle" @close="confirmRemove = null">
      <p class="class-detail__modal-text">{{ messages.classes.detailRemoveConfirm }}</p>
      <template #footer>
        <Button variant="ghost" @click="confirmRemove = null">{{ messages.classes.cancel }}</Button>
        <Button variant="danger" @click="removeMember(confirmRemove ?? 0)">{{ messages.classes.detailRemove }}</Button>
      </template>
    </Modal>

    <!-- Modal thêm thành viên -->
    <Modal :open="addMemberOpen" :title="messages.classes.detailAddTitle" @close="addMemberOpen = false">
      <form novalidate @submit.prevent="addMember">
        <Input
          v-model="addEmail"
          :label="messages.classes.detailAddEmailLabel"
          type="email"
          :placeholder="messages.classes.detailAddEmailPlaceholder"
          required
        />
        <div class="class-detail__modal-actions">
          <Button variant="ghost" @click="addMemberOpen = false">{{ messages.classes.cancel }}</Button>
          <Button type="submit">{{ messages.classes.detailAddSubmit }}</Button>
        </div>
      </form>
    </Modal>

    <!-- Modal gán nội dung -->
    <Modal :open="assignOpen" :title="messages.classes.detailAssignTitle" @close="assignOpen = false">
      <form novalidate @submit.prevent="createAssignment">
        <label class="label" for="assign-due">{{ messages.classes.detailAssignDueLabel }}</label>
        <input id="assign-due" v-model="assignDue" class="input" type="datetime-local" />
        <p class="class-detail__modal-note text-muted">{{ messages.classes.detailAssignNote }}</p>
        <div class="class-detail__modal-actions">
          <Button variant="ghost" @click="assignOpen = false">{{ messages.classes.cancel }}</Button>
          <Button type="submit">{{ messages.classes.detailAssignSubmit }}</Button>
        </div>
      </form>
    </Modal>

    <!-- Modal xác nhận xóa lớp -->
    <Modal :open="confirmDelete" :title="messages.classes.detailDeleteTitle" @close="confirmDelete = false">
      <p class="class-detail__modal-text">
        {{ messages.classes.detailDeleteConfirm(classStore.currentClass?.name ?? '') }}
      </p>
      <template #footer>
        <Button variant="ghost" @click="confirmDelete = false">{{ messages.classes.cancel }}</Button>
        <Button variant="danger" @click="confirmDeleteClass">
          <Trash2 :size="14" aria-hidden="true" /> {{ messages.classes.detailDeleteBtn }}
        </Button>
      </template>
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
  flex-wrap: wrap;
}

/* ── Hero gradient Sunset (cùng pattern LessonView — GP-T9b dark overlay) ── */
.class-detail__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-xl);
  border-radius: var(--radius-xl);
  background-image: var(--gradient-sunset);
  color: #fff;
  box-shadow: var(--shadow-lg);
}

.class-detail__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: linear-gradient(120deg, rgba(255, 255, 255, 0.16), transparent 55%);
}

.dark .class-detail__hero::after {
  background: rgba(4, 47, 46, 0.62);
}

.class-detail__hero-top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.class-detail__hero-main { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 4px; }

.class-detail__hero-title {
  font-size: var(--text-2xl);
  margin: 0;
  overflow-wrap: anywhere;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.16);
}

.class-detail__hero-desc {
  color: rgba(255, 255, 255, 0.92);
  font-size: var(--text-sm);
  max-width: 70ch;
  margin: 0;
}

.class-detail__hero-badges { display: flex; gap: var(--space-sm); flex-wrap: wrap; align-items: center; }

.class-detail__hero-actions {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  align-items: center;
}

.class-detail__hero-link { text-decoration: none; }
.class-detail__hero-link:hover { text-decoration: none; }

/* ── Chip trong hero (trắng trong suốt — đọc được trên gradient) ── */
.class-detail__hero-chip {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  border: 1px solid rgba(255, 255, 255, 0.35);
  background: rgba(255, 255, 255, 0.14);
  border-radius: var(--radius-md);
  padding: 4px 10px;
  color: #fff;
  font-size: var(--text-xs);
  white-space: nowrap;
  backdrop-filter: blur(4px);
}

.class-detail__hero-chip--code {
  border-style: dashed;
  border-color: rgba(255, 255, 255, 0.55);
  font-size: var(--text-sm);
}

.class-detail__hero-chip--code code {
  font-family: var(--font-mono);
  font-weight: 700;
  font-size: var(--text-md);
  letter-spacing: 0.12em;
}

.class-detail__copy-btn {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  background: rgba(255, 255, 255, 0.18);
  border: none;
  border-radius: var(--radius-sm);
  color: #fff;
  font-size: var(--text-xs);
  font-weight: 600;
  padding: 3px 8px;
  cursor: pointer;
  transition: background-color 150ms ease;
}

.class-detail__copy-btn:hover { background: rgba(255, 255, 255, 0.32); }

/* ── Panel ── */
.class-detail__panel { display: flex; flex-direction: column; gap: var(--space-md); }

.class-detail__toolbar { display: flex; justify-content: flex-end; }

/* ── Bảng thành viên ── */
.class-detail__table { padding: 0; }

.class-detail__table-scroll { overflow-x: auto; border-radius: inherit; }

.class-detail__table table { width: 100%; border-collapse: collapse; min-width: 600px; }

.class-detail__table th {
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

.class-detail__table td {
  padding: var(--space-sm) var(--space-md);
  border-bottom: 1px solid var(--color-border);
  font-size: var(--text-sm);
  vertical-align: middle;
}

.class-detail__table tbody tr { transition: background-color 150ms ease; }

.class-detail__table tbody tr:hover { background: color-mix(in srgb, var(--color-primary) 5%, transparent); }

.class-detail__table tbody tr:last-child td { border-bottom: none; }

.class-detail__user { display: flex; align-items: center; gap: var(--space-sm); min-width: 0; }

.class-detail__avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background-image: var(--gradient-mint);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: var(--text-sm);
  flex-shrink: 0;
}

.class-detail__user-meta { min-width: 0; }

.class-detail__name { font-weight: 700; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 240px; }
.class-detail__email { font-size: var(--text-xs); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 240px; }

.class-detail__date { white-space: nowrap; font-variant-numeric: tabular-nums; }

/* ── Lộ trình đã gán ── */
.class-detail__assignments { display: flex; flex-direction: column; gap: var(--space-sm); }

.class-detail__assign {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-md);
  min-width: 0;
}

.class-detail__assign-icon {
  width: 38px;
  height: 38px;
  border-radius: var(--radius-md);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  color: #fff;
  box-shadow: var(--shadow-sm);
}

.class-detail__assign-icon--mint { background-image: var(--gradient-mint); }
.class-detail__assign-icon--sunset { background-image: var(--gradient-sunset); }
.class-detail__assign-icon--aurora { background-image: var(--gradient-aurora); }

.class-detail__assign-info { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: 2px; }

.class-detail__assign-title {
  font-weight: 700;
  font-size: var(--text-sm);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.class-detail__assign-due {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: var(--text-xs);
  white-space: nowrap;
}

/* ── Cài đặt (danger zone) ── */
.class-detail__settings {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  max-width: 520px;
  border-color: color-mix(in srgb, var(--color-destructive) 45%, var(--color-border));
}

.class-detail__settings-head { display: flex; align-items: flex-start; gap: var(--space-sm); }

.class-detail__settings-icon {
  width: 38px;
  height: 38px;
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-destructive) 12%, transparent);
  color: var(--color-destructive);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.class-detail__settings-title { font-size: var(--text-md); color: var(--color-destructive); }
.class-detail__settings-note { font-size: var(--text-sm); }

/* ── Modal ── */
.class-detail__modal-text { font-size: var(--text-sm); overflow-wrap: anywhere; }

.class-detail__modal-actions { display: flex; justify-content: flex-end; gap: var(--space-sm); margin-top: var(--space-md); }

.class-detail__modal-note { font-size: var(--text-xs); margin-top: 6px; }

@media (max-width: 640px) {
  .class-detail__hero { padding: var(--space-lg); }
}
</style>
