<script setup lang="ts">
// ClassDetailView — Màn 20: 3 tab (Thành viên / Lộ trình đã gán / Cài đặt)
// View-quality Phase 1 (Nhóm D): banner = surface band level-2; mã mời =
// block-token tối canvas-ink (quyết định #4/#5); bảng chuẩn §4.6 + mobile
// card-stack; assignment có index mono; bỏ gradient/glassmorphism/hover-lift.
import { computed, onMounted, onUnmounted, ref } from 'vue';
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

let copyTimer: ReturnType<typeof setTimeout> | undefined;

onUnmounted(() => {
  if (copyTimer) clearTimeout(copyTimer);
});

/** API detail KHÔNG trả `role` (ClassDetailDto chỉ có OwnerId) → tính từ owner. */
const isManager = computed(() => {
  const cls = classStore.currentClass;
  return cls?.ownerId === auth.user?.id || auth.role === 'ADMIN';
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

/** Số thứ tự 2 chữ số cho assignment (index mono — quyết định #4). */
const pad = (n: number): string => String(n).padStart(2, '0');

/** Avatar stack hero: tối đa 4 thành viên + overflow badge "+N" */
const avatarStack = computed(() => classStore.members.slice(0, 4));
const avatarOverflow = computed(() => Math.max(0, classStore.members.length - 4));

/** Countdown hạn nộp: ok (xanh) / warn (≤ 3 ngày hoặc < 24h) / over (quá hạn). */
function deadlineInfo(assign: ClassAssignmentDto): { label: string; tone: 'none' | 'ok' | 'warn' | 'over' } {
  if (!assign.dueAt) return { label: messages.classes.detailDueNone, tone: 'none' };
  const ms = new Date(assign.dueAt).getTime() - Date.now();
  if (Number.isNaN(ms)) return { label: messages.classes.detailDueNone, tone: 'none' };
  if (ms <= 0) return { label: 'Đã hết hạn', tone: 'over' };
  const hours = ms / 3_600_000;
  if (hours < 24) return { label: `Còn ${Math.max(1, Math.round(hours))} giờ`, tone: 'warn' };
  const days = Math.ceil(hours / 24);
  return { label: `Còn ${days} ngày`, tone: days <= 3 ? 'warn' : 'ok' };
}

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
    if (copyTimer) clearTimeout(copyTimer);
    copyTimer = setTimeout(() => {
      copied.value = false;
    }, 1600);
  });
}

function assignmentTitle(assign: ClassAssignmentDto): string {
  if (assign.lessonId !== null) return messages.classes.detailLesson(assign.lessonId);
  if (assign.exerciseId !== null) return messages.classes.detailExercise(assign.exerciseId);
  return messages.classes.detailGenericContent;
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
      <!-- Banner: surface band level-2 (DESIGN §1/#1 — không gradient) -->
      <header class="class-detail__hero">
        <div class="class-detail__hero-top">
          <div class="class-detail__hero-main">
            <h1 class="class-detail__hero-title">{{ classStore.currentClass.name }}</h1>
            <p class="class-detail__hero-desc">{{ classStore.currentClass.description || messages.classes.noDescription }}</p>
          </div>
          <div class="class-detail__hero-badges">
            <!-- Avatar stack + overflow badge (thành viên gần đây) -->
            <div v-if="avatarStack.length > 0" class="class-detail__avatar-stack" aria-label="Thành viên gần đây">
              <span
                v-for="(member, i) in avatarStack"
                :key="member.id"
                class="class-detail__avatar-stack-item"
                :style="{ '--i': i }"
                aria-hidden="true"
              >
                {{ initial(member.displayName) }}
              </span>
              <span v-if="avatarOverflow > 0" class="class-detail__avatar-stack-overflow" aria-hidden="true">
                +{{ avatarOverflow }}
              </span>
            </div>
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
          <!-- Mã mời = block-token tối (vùng dữ liệu LUÔN tối — quyết định #5) -->
          <span class="class-detail__code-panel">
            <span class="class-detail__code-label">
              <KeyRound :size="13" aria-hidden="true" />
              {{ messages.classes.inviteLabel }}
            </span>
            <code class="class-detail__code">{{ classStore.currentClass.inviteCode }}</code>
            <Button
              v-if="isManager"
              size="sm"
              variant="secondary"
              class="class-detail__copy-btn"
              :aria-label="messages.classes.detailCopy"
              @click="copyInvite"
            >
              <Check v-if="copied" :size="14" aria-hidden="true" />
              <ClipboardCopy v-else :size="14" aria-hidden="true" />
              {{ messages.classes.detailCopy }}
            </Button>
          </span>
          <RouterLink :to="{ name: 'class-report', params: { id: String(classId) } }" class="class-detail__hero-link">
            <Button v-if="isManager" size="md" variant="secondary">
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
          <Button size="md" @click="addMemberOpen = true">
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
                  <th scope="col">{{ messages.classes.detailColMember }}</th>
                  <th scope="col">{{ messages.classes.detailColRole }}</th>
                  <th scope="col">{{ messages.classes.detailColJoined }}</th>
                  <th v-if="isManager" scope="col">{{ messages.classes.detailColActions }}</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="member in classStore.members"
                  :key="member.id"
                  class="hover:bg-muted/50"
                >
                  <td :data-label="messages.classes.detailColMember">
                    <div class="class-detail__user">
                      <span class="class-detail__avatar" aria-hidden="true">{{ initial(member.displayName) }}</span>
                      <div class="class-detail__user-meta">
                        <p class="class-detail__name">{{ member.displayName }}</p>
                        <p class="class-detail__email">{{ member.email }}</p>
                      </div>
                    </div>
                  </td>
                  <td :data-label="messages.classes.detailColRole">
                    <Badge :variant="member.role === 'TEACHER' ? 'primary' : 'muted'">
                      {{ member.role === 'TEACHER' ? messages.classes.roleTeacher : messages.classes.roleStudent }}
                    </Badge>
                  </td>
                  <td class="class-detail__date" :data-label="messages.classes.detailColJoined">
                    {{ formatDate(member.joinedAt) }}
                  </td>
                  <td v-if="isManager" :data-label="messages.classes.detailColActions">
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
          <Button size="md" @click="assignOpen = true">
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
          <Card v-for="(assign, i) in classStore.assignments" :key="assign.id" class="class-detail__assign">
            <span class="class-detail__assign-index" aria-hidden="true">#{{ pad(i + 1) }}</span>
            <span class="class-detail__assign-icon" aria-hidden="true">
              <Puzzle v-if="assign.exerciseId !== null" :size="18" />
              <BookOpen v-else :size="18" />
            </span>
            <div class="class-detail__assign-info">
              <p class="class-detail__assign-title">{{ assignmentTitle(assign) }}</p>
              <p class="class-detail__assign-due">
                <CalendarClock :size="13" aria-hidden="true" />
                {{ assign.dueAt ? messages.classes.detailDue(formatDate(assign.dueAt)) : messages.classes.detailDueNone }}
                <!-- Deadline countdown — tone theo thời gian còn lại -->
                <span
                  v-if="deadlineInfo(assign).tone !== 'none'"
                  class="class-detail__assign-countdown"
                  :class="`class-detail__assign-countdown--${deadlineInfo(assign).tone}`"
                >
                  {{ deadlineInfo(assign).label }}
                </span>
              </p>
            </div>
            <!-- Status badge animated (open = nhịp thở nhẹ, ui-pulse-glow) -->
            <Badge
              :variant="assign.status === 'open' ? 'success' : 'danger'"
              :class="{ 'class-detail__assign-status--open': assign.status === 'open' }"
            >
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
              <p class="class-detail__settings-note">{{ messages.classes.detailSettingsNote }}</p>
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
        <Input
          id="assign-due"
          v-model="assignDue"
          type="datetime-local"
          :label="messages.classes.detailAssignDueLabel"
        />
        <p class="class-detail__modal-note">{{ messages.classes.detailAssignNote }}</p>
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
  color: var(--foreground-secondary);
  flex-wrap: wrap;
}

/* ── Banner: surface band level-2 (DESIGN §6) — không gradient, không shadow ── */
.class-detail__hero {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-xl);
  border-radius: var(--radius-lg);
  background: var(--card-raised);
  border: 1px solid var(--border-subtle);
}

.class-detail__hero-top {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.class-detail__hero-main { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: var(--space-xs); }

.class-detail__hero-title {
  font-size: var(--text-4xl);
  font-weight: 600;
  letter-spacing: -0.03em;
  line-height: 1.1;
  margin: 0;
  color: var(--foreground);
  overflow-wrap: anywhere;
}

.class-detail__hero-desc {
  color: var(--foreground-secondary);
  font-size: var(--text-sm);
  max-width: 70ch;
  margin: 0;
}

.class-detail__hero-badges { display: flex; gap: var(--space-sm); flex-wrap: wrap; align-items: center; }

.class-detail__hero-chip {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  border: 1px solid var(--border);
  background: var(--card);
  border-radius: var(--radius-md);
  padding: 0 var(--space-sm);
  color: var(--foreground-secondary);
  font-size: var(--text-xs);
  white-space: nowrap;
  min-height: 24px;
}

/* ── Avatar stack + overflow badge (member list — premium) ── */
.class-detail__avatar-stack {
  display: inline-flex;
  align-items: center;
  padding-left: var(--space-sm);
}

.class-detail__avatar-stack-item {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  border: 2px solid var(--card-raised);
  background: var(--muted);
  color: var(--foreground-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 500;
  font-size: var(--text-xs);
  margin-left: -8px;
  animation: avatar-stack-in 260ms var(--ease-out-expo) both;
  animation-delay: calc(var(--i) * 55ms + 80ms);
}

.class-detail__avatar-stack-item:first-child { margin-left: 0; }

.class-detail__avatar-stack-overflow {
  min-width: 32px;
  height: 32px;
  padding: 0 8px;
  border-radius: var(--radius-full);
  border: 2px solid var(--card-raised);
  background: color-mix(in srgb, var(--primary) 14%, var(--muted));
  color: var(--primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 600;
  margin-left: -8px;
  animation: avatar-stack-in 260ms var(--ease-out-expo) both;
  animation-delay: 320ms;
}

@keyframes avatar-stack-in {
  from { opacity: 0; transform: translateY(6px) scale(0.9); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}

@media (prefers-reduced-motion: reduce) {
  .class-detail__avatar-stack-item,
  .class-detail__avatar-stack-overflow { animation: none; }
}

.class-detail__hero-actions {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  align-items: center;
}

.class-detail__hero-link { text-decoration: none; }
.class-detail__hero-link:hover { text-decoration: none; }

/* ── Panel mã mời: block-token tối (khoảnh khắc đầu tư — enter settle) ── */
.class-detail__code-panel {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  background: var(--canvas-ink);
  border: 1px solid rgba(66, 85, 255, 0.3);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
  opacity: 0;
  transform: translateY(6px);
  animation: code-panel-enter 250ms cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

.class-detail__code-label {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--index-muted);
  white-space: nowrap;
}

.class-detail__code {
  font-family: var(--font-mono);
  font-size: var(--text-md);
  font-weight: 500;
  letter-spacing: 0.12em;
  color: var(--resolved);
}

.class-detail__copy-btn { flex-shrink: 0; }

@keyframes code-panel-enter {
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .class-detail__code-panel {
    animation: none;
    opacity: 1;
    transform: none;
  }
}

/* ── Panel ── */
.class-detail__panel { display: flex; flex-direction: column; gap: var(--space-md); }

.class-detail__toolbar { display: flex; justify-content: flex-end; }

/* ── Bảng thành viên (DESIGN §4.6) ── */
.class-detail__table { padding: 0; }

.class-detail__table-scroll { overflow-x: auto; border-radius: inherit; }

.class-detail__table table { width: 100%; border-collapse: collapse; }

.class-detail__table th {
  text-align: left;
  font-size: var(--text-sm);
  font-weight: 500;
  color: var(--foreground-tertiary);
  padding: 0 var(--space-md);
  border-bottom: 1px solid var(--border);
  background: var(--muted);
  white-space: nowrap;
  height: 40px;
}

.class-detail__table td {
  padding: 12px var(--space-md);
  border-bottom: 1px solid var(--border);
  font-size: var(--text-sm);
  vertical-align: middle;
}

.class-detail__table tbody tr:last-child td { border-bottom: none; }

.class-detail__user { display: flex; align-items: center; gap: var(--space-sm); min-width: 0; }

.class-detail__avatar {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--muted);
  color: var(--foreground-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 500;
  font-size: var(--text-sm);
  flex-shrink: 0;
}

.class-detail__user-meta { min-width: 0; }

.class-detail__name {
  font-weight: 500;
  color: var(--foreground);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 240px;
}

.class-detail__email {
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 240px;
}

.class-detail__date { white-space: nowrap; font-variant-numeric: tabular-nums; font-family: var(--font-mono); font-size: var(--text-xs); }

/* ── Lộ trình đã gán: index mono + card level-1 (quyết định #4) ── */
.class-detail__assignments { display: flex; flex-direction: column; gap: var(--space-sm); }

.class-detail__assign {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-md);
  min-width: 0;
  border-color: var(--border);
  transition: border-color 150ms;
}

.class-detail__assign:hover { border-color: var(--border-strong); }

.class-detail__assign-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  flex-shrink: 0;
  min-width: 28px;
}

.class-detail__assign-icon {
  width: 38px;
  height: 38px;
  border-radius: var(--radius-md);
  background: var(--muted);
  color: var(--foreground-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.class-detail__assign-info { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: var(--space-xs); }

.class-detail__assign-title {
  font-weight: 500;
  font-size: var(--text-sm);
  color: var(--foreground);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.class-detail__assign-due {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  white-space: nowrap;
  flex-wrap: wrap;
}

/* Deadline countdown — tone ok/warn/over (semantic, palette 6 màu) */
.class-detail__assign-countdown {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 600;
  border-radius: var(--radius-full);
  padding: 1px 8px;
  white-space: nowrap;
}

.class-detail__assign-countdown--ok {
  color: var(--success);
  background: color-mix(in srgb, var(--success) 12%, transparent);
}

.class-detail__assign-countdown--warn {
  color: var(--warning);
  background: color-mix(in srgb, var(--warning) 12%, transparent);
}

.class-detail__assign-countdown--over {
  color: var(--destructive);
  background: color-mix(in srgb, var(--destructive) 12%, transparent);
}

/* Status badge "Đang mở" — nhịp thở nhẹ (ui-pulse-glow global) */
.class-detail__assign-status--open {
  animation: ui-pulse-glow 2.2s var(--ease-in-out) infinite;
}

@media (prefers-reduced-motion: reduce) {
  .class-detail__assign-status--open { animation: none; }
}

/* ── Cài đặt (danger zone — semantic destructive) ── */
.class-detail__settings {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  max-width: 520px;
  border-color: color-mix(in srgb, var(--destructive) 40%, var(--border));
}

.class-detail__settings-head { display: flex; align-items: flex-start; gap: var(--space-sm); }

.class-detail__settings-icon {
  width: 38px;
  height: 38px;
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--destructive) 12%, transparent);
  color: var(--destructive);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.class-detail__settings-title { font-size: var(--text-lg); font-weight: 600; letter-spacing: -0.015em; color: var(--destructive); }
.class-detail__settings-note { font-size: var(--text-sm); color: var(--foreground-secondary); }

/* ── Modal ── */
.class-detail__modal-text { font-size: var(--text-sm); color: var(--foreground); overflow-wrap: anywhere; }

.class-detail__modal-actions { display: flex; justify-content: flex-end; gap: var(--space-sm); margin-top: var(--space-md); }

.class-detail__modal-note { font-size: var(--text-xs); color: var(--foreground-tertiary); margin-top: var(--space-sm); }

@media (max-width: 640px) {
  .class-detail__hero { padding: var(--space-lg); }

  /* Bảng → card-stack (DESIGN §8 — cấm scroll ngang bảng chính ở mobile) */
  .class-detail__table-scroll { overflow-x: visible; }

  .class-detail__table thead { display: none; }

  .class-detail__table tbody tr {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-xs) var(--space-md);
    padding: var(--space-sm) var(--space-md);
    border-bottom: 1px solid var(--border);
  }

  .class-detail__table tbody tr:last-child { border-bottom: none; }

  .class-detail__table td {
    display: flex;
    flex-direction: column;
    gap: var(--space-xs);
    padding: 0;
    border-bottom: none;
  }

  .class-detail__table td::before {
    content: attr(data-label);
    font-size: var(--text-xs);
    color: var(--foreground-tertiary);
  }

  .class-detail__table td:first-child { grid-column: 1 / -1; }
  .class-detail__table td:last-child { align-items: flex-start; }

  .class-detail__name,
  .class-detail__email { max-width: 100%; white-space: normal; }
}
</style>
