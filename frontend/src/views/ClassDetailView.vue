<script setup lang="ts">
// ClassDetailView — Màn 20: 3 tab (Thành viên / Lộ trình đã gán / Cài đặt)
// View-quality Phase 1 (Nhóm D): banner = surface band level-2; mã mời =
// block-token tối canvas-ink (quyết định #4/#5); bảng chuẩn §4.6 + mobile
// card-stack; assignment có index mono; bỏ gradient/glassmorphism/hover-lift.
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  AlertTriangle,
  ArrowRight,
  BookOpen,
  CalendarClock,
  Check,
  CheckCircle2,
  ChevronLeft,
  ChevronRight,
  ClipboardCopy,
  Clock,
  KeyRound,
  Pencil,
  Puzzle,
  Timer,
  Trash2,
  UserPlus,
  Users,
} from 'lucide-vue-next';
import type { Component } from 'vue';

import { useClassStore } from '@/stores/classStore';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import * as classesApi from '@/api/classes';
import * as lessonsApi from '@/api/lessons';
import * as exercisesApi from '@/api/exercises';
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
import Select, { type SelectOption } from '@/components/ui/Select.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import PageHero from '@/components/ui/PageHero.vue';

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
const assignType = ref<string>('lesson');
const assignItem = ref('');
const assignDue = ref('');
const assignLate = ref(true);
const assignSubmitting = ref(false);
const assignLoading = ref(false);
const lessonOptions = ref<SelectOption[]>([]);
const exerciseOptions = ref<SelectOption[]>([]);

const editAssign = ref<ClassAssignmentDto | null>(null);
const editDue = ref('');
const editLate = ref(true);
const editSaving = ref(false);

const confirmAssignDelete = ref<number | null>(null);

// ── Task 2: thống kê nộp bài thật từ GET /classes/{id}/report (chỉ manager, không
// chặn màn nếu báo cáo lỗi) — dùng cho thanh tiến độ + badge hạn nộp của bài gán
// và cột "Bài chưa nộp" của bảng học viên. API KHÔNG có progress/avgScore từng
// thành viên → cột tiến độ hiển thị số bài thiếu (LaggingLearnerDto — dữ liệu thật).
const assignmentStats = ref<Record<number, { onTime: number; late: number; notSubmitted: number }>>({});
const reportTotalMembers = ref(0);
const laggingMap = ref<Record<number, number>>({});
const reportLoaded = ref(false);

/** Bảng học viên phân trang client-side (dữ liệu thật từ classStore.members). */
const MEMBER_PAGE_SIZE = 8;
const memberPage = ref(1);
const memberPageCount = computed(() => Math.max(1, Math.ceil(classStore.members.length / MEMBER_PAGE_SIZE)));
const pagedMembers = computed(() => {
  const start = (memberPage.value - 1) * MEMBER_PAGE_SIZE;
  return classStore.members.slice(start, start + MEMBER_PAGE_SIZE);
});
const memberPageRange = computed(() => {
  const total = classStore.members.length;
  if (total === 0) return { from: 0, to: 0 };
  const from = (memberPage.value - 1) * MEMBER_PAGE_SIZE + 1;
  return { from, to: Math.min(memberPage.value * MEMBER_PAGE_SIZE, total) };
});

function goMemberPage(page: number): void {
  memberPage.value = Math.min(Math.max(1, page), memberPageCount.value);
}

// Thành viên thay đổi (thêm/gỡ) → về trang đầu.
watch(
  () => classStore.members.length,
  () => {
    memberPage.value = 1;
  },
);

async function loadReport(): Promise<void> {
  try {
    const report = await classesApi.fetchClassReport(classId.value);
    reportTotalMembers.value = report.totalMembers;
    const stats: Record<number, { onTime: number; late: number; notSubmitted: number }> = {};
    for (const a of report.assignments) {
      stats[a.assignmentId] = { onTime: a.onTime, late: a.late, notSubmitted: a.notSubmitted };
    }
    assignmentStats.value = stats;
    const lagging: Record<number, number> = {};
    for (const l of report.laggingLearners) lagging[l.userId] = l.missingCount;
    laggingMap.value = lagging;
    reportLoaded.value = true;
  } catch {
    // Báo cáo là quyền giảng viên — lỗi không chặn chi tiết lớp (giữ hành vi cũ).
  }
}

/** % nộp bài của 1 bài gán (submitted/totalMembers — dữ liệu thật từ report). */
function submissionPct(assign: ClassAssignmentDto): number {
  const stats = assignmentStats.value[assign.id];
  if (!stats || reportTotalMembers.value === 0) return 0;
  return Math.round(((stats.onTime + stats.late) / reportTotalMembers.value) * 100);
}

/** Badge hạn nộp semantic (Task 2): Đang mở (muted) / Còn thiếu (destructive) /
    Nộp trễ (warning) / Đúng hạn (success) — từ dueAt + thống kê report thật. */
function deadlineBadge(
  assign: ClassAssignmentDto,
): { label: string; tone: 'success' | 'warning' | 'destructive' | 'muted' } | null {
  const stats = assignmentStats.value[assign.id];
  const due = assign.dueAt ? new Date(assign.dueAt).getTime() : null;
  const isOpen = due === null || due > Date.now();
  if (isOpen) return { label: messages.classes.detailStatusOpen, tone: 'muted' };
  if (!stats) return null;
  if (stats.notSubmitted > 0) return { label: messages.classes.detailStatusMissing(stats.notSubmitted), tone: 'destructive' };
  if (stats.late > 0) return { label: messages.classes.detailStatusLate(stats.late), tone: 'warning' };
  return { label: messages.classes.detailStatusOnTime, tone: 'success' };
}

/** FIX R1: icon lucide nhỏ cho badge hạn nộp theo tone (visual + semantic, không đổi logic). */
function deadlineIcon(tone: 'success' | 'warning' | 'destructive' | 'muted'): Component {
  switch (tone) {
    case 'success':
      return CheckCircle2;
    case 'warning':
      return Timer;
    case 'destructive':
      return AlertTriangle;
    default:
      return Clock;
  }
}

/** Số bài thiếu của 1 học viên (0 = không trong danh sách chậm tiến độ của report). */
const missingOf = (userId: number): number => laggingMap.value[userId] ?? 0;

/** Thống kê nộp bài của 1 bài gán (null khi report chưa tải / không có quyền). */
const assignStatsOf = (assign: ClassAssignmentDto) => assignmentStats.value[assign.id] ?? null;

const assignOptions = computed<SelectOption[]>(() => (assignType.value === 'lesson' ? lessonOptions.value : exerciseOptions.value));

/** Đổi loại nội dung → reset lựa chọn item. */
watch(assignType, () => {
  assignItem.value = '';
});

let copyTimer: ReturnType<typeof setTimeout> | undefined;

onUnmounted(() => {
  if (copyTimer) clearTimeout(copyTimer);
});

/** API detail KHÔNG trả `role` (ClassDetailDto chỉ có OwnerId) → tính từ owner. */
const isManager = computed(() => {
  const cls = classStore.currentClass;
  return cls?.ownerId === auth.user?.id || auth.role === 'ADMIN';
});

/** Vai trò thành viên: backend ClassMemberDto không trả role → so với OwnerId của lớp. */
const isMemberTeacher = (member: { userId: number }): boolean => member.userId === classStore.currentClass?.ownerId;

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

onMounted(load);

async function load(): Promise<void> {
  loading.value = true;
  try {
    await classStore.fetchClass(classId.value);
    // Task 2: manager → nạp kèm báo cáo để vẽ tiến độ nộp bài + badge hạn nộp (không chặn).
    if (isManager.value) void loadReport();
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

/** Mở modal gán: tải danh sách bài học + bài tập để chọn (GET /lessons, GET /exercises). */
async function openAssign(): Promise<void> {
  assignOpen.value = true;
  assignLoading.value = true;
  try {
    const [lessonsPaged, exercises] = await Promise.all([
      lessonsApi.fetchLessons({ page: 1, pageSize: 100 }),
      exercisesApi.fetchExercises({}),
    ]);
    lessonOptions.value = (lessonsPaged.items ?? []).map((l) => ({ label: l.title, value: l.id }));
    exerciseOptions.value = exercises.map((e) => ({ label: e.title, value: e.id }));
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.detailAssignFailed, 'error');
    lessonOptions.value = [];
    exerciseOptions.value = [];
  } finally {
    assignLoading.value = false;
  }
}

async function createAssignment(): Promise<void> {
  const itemId = Number(assignItem.value);
  if (!assignItem.value || !Number.isFinite(itemId)) {
    ui.showToast(messages.classes.detailAssignItemRequired, 'warning');
    return;
  }
  assignSubmitting.value = true;
  try {
    await classStore.assignContent({
      classId: classId.value,
      lessonId: assignType.value === 'lesson' ? itemId : null,
      exerciseId: assignType.value === 'exercise' ? itemId : null,
      dueAt: assignDue.value ? new Date(assignDue.value).toISOString() : null,
      allowLateSubmission: assignLate.value,
    });
    ui.showToast(messages.classes.detailAssigned, 'success');
    assignOpen.value = false;
    assignItem.value = '';
    assignDue.value = '';
    assignLate.value = true;
    await classStore.reloadAssignments(classId.value);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.detailAssignFailed, 'error');
  } finally {
    assignSubmitting.value = false;
  }
}

/** ISO → giá trị cho input datetime-local (múi giờ địa phương). */
function toLocalInput(iso: string): string {
  const d = new Date(iso);
  const pad = (n: number): string => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

function openEdit(assign: ClassAssignmentDto): void {
  editAssign.value = assign;
  editDue.value = assign.dueAt ? toLocalInput(assign.dueAt) : '';
  editLate.value = assign.allowLateSubmission;
}

async function saveEdit(): Promise<void> {
  if (!editAssign.value) return;
  editSaving.value = true;
  try {
    await classStore.updateAssignment(classId.value, editAssign.value.id, {
      dueAt: editDue.value ? new Date(editDue.value).toISOString() : null,
      allowLateSubmission: editLate.value,
    });
    ui.showToast(messages.classes.detailEditSaved, 'success');
    editAssign.value = null;
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.detailEditFailed, 'error');
  } finally {
    editSaving.value = false;
  }
}

async function removeAssignment(assignId: number): Promise<void> {
  try {
    await classStore.removeAssignment(classId.value, assignId);
    ui.showToast(messages.classes.detailDeleteAssignSuccess, 'success');
    confirmAssignDelete.value = null;
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.detailDeleteAssignFailed, 'error');
  }
}

/** Mở nội dung bài gán: bài tập → /exercise/:id?classAssignmentId=..., bài học → route lesson. */
function openAssignment(assign: ClassAssignmentDto): void {
  if (assign.exerciseId !== null) {
    void router.push({
      name: 'exercise',
      params: { id: String(assign.exerciseId) },
      query: { classAssignmentId: String(assign.id) },
    });
  } else if (assign.lessonId !== null) {
    void router.push({ name: 'lesson', params: { lessonId: String(assign.lessonId) } });
  }
}

function isNavigable(assign: ClassAssignmentDto): boolean {
  return assign.exerciseId !== null || assign.lessonId !== null;
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
  if (assign.title) return assign.title;
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
      <!-- Banner: PageHero shared (surface band level-2 — DESIGN §1/#1, không gradient) -->
      <PageHero border="full" padding="xl">
        <template #title>{{ classStore.currentClass.name }}</template>
        <template #description>{{ classStore.currentClass.description || messages.classes.noDescription }}</template>
        <template #side>
          <div class="class-detail__hero-badges">
            <Badge variant="primary">
              {{ isManager ? messages.classes.roleManager : messages.classes.roleMember }}
            </Badge>
            <span class="class-detail__hero-chip">
              <Users :size="13" aria-hidden="true" />
              {{ messages.classes.members(classStore.members.length) }}
            </span>
          </div>
        </template>
        <template #bottom>
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
        </template>
      </PageHero>

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
                  <th v-if="isManager && reportLoaded" scope="col">{{ messages.classes.detailColMissing }}</th>
                  <th scope="col">{{ messages.classes.detailColJoined }}</th>
                  <th v-if="isManager" scope="col">{{ messages.classes.detailColActions }}</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="member in pagedMembers"
                  :key="member.userId"
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
                    <Badge :variant="isMemberTeacher(member) ? 'primary' : 'muted'">
                      {{ isMemberTeacher(member) ? messages.classes.roleTeacher : messages.classes.roleStudent }}
                    </Badge>
                  </td>
                  <td v-if="isManager && reportLoaded" :data-label="messages.classes.detailColMissing">
                    <span class="class-detail__missing" :class="{ 'class-detail__missing--ok': missingOf(member.userId) === 0 }">
                      <template v-if="missingOf(member.userId) > 0">
                        <span class="class-detail__missing-badge">{{ messages.classes.reportLaggingMissing(missingOf(member.userId)) }}</span>
                      </template>
                      <template v-else>
                        <Check :size="14" aria-hidden="true" />
                        <span class="class-detail__missing-text">{{ messages.classes.detailMissingOk }}</span>
                      </template>
                    </span>
                  </td>
                  <td class="class-detail__date" :data-label="messages.classes.detailColJoined">
                    {{ formatDate(member.joinedAt) }}
                  </td>
                  <td v-if="isManager" :data-label="messages.classes.detailColActions">
                    <Button size="sm" variant="danger" @click="confirmRemove = member.userId">
                      {{ messages.classes.detailRemove }}
                    </Button>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
          <!-- Phân trang (Task 2): counter mono + prev/next icon button (DESIGN §4.1) -->
          <div class="class-detail__pager">
            <span class="class-detail__pager-info">
              {{ messages.classes.detailPageInfo(memberPageRange.from, memberPageRange.to, classStore.members.length) }}
            </span>
            <div class="class-detail__pager-buttons">
              <Button
                size="icon"
                variant="secondary"
                :disabled="memberPage <= 1"
                :aria-label="messages.classes.detailPagePrev"
                @click="goMemberPage(memberPage - 1)"
              >
                <ChevronLeft :size="16" aria-hidden="true" />
              </Button>
              <Button
                size="icon"
                variant="secondary"
                :disabled="memberPage >= memberPageCount"
                :aria-label="messages.classes.detailPageNext"
                @click="goMemberPage(memberPage + 1)"
              >
                <ChevronRight :size="16" aria-hidden="true" />
              </Button>
            </div>
          </div>
        </Card>
      </section>

      <!-- Tab Bài tập -->
      <section v-else-if="tab === 'assignments'" class="class-detail__panel">
        <div v-if="isManager" class="class-detail__toolbar">
          <Button size="md" @click="openAssign">
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
          <Card
            v-for="(assign, i) in classStore.assignments"
            :key="assign.id"
            class="class-detail__assign"
            :class="{ 'class-detail__assign--link': isNavigable(assign) }"
            :role="isNavigable(assign) ? 'button' : undefined"
            :tabindex="isNavigable(assign) ? 0 : undefined"
            :aria-label="isNavigable(assign) ? assignmentTitle(assign) : undefined"
            @click="isNavigable(assign) && openAssignment(assign)"
            @keydown.enter="isNavigable(assign) && openAssignment(assign)"
            @keydown.space.prevent="isNavigable(assign) && openAssignment(assign)"
          >
            <span class="class-detail__assign-index" aria-hidden="true">#{{ pad(i + 1) }}</span>
            <span class="class-detail__assign-icon" aria-hidden="true">
              <Puzzle v-if="assign.exerciseId !== null" :size="18" />
              <BookOpen v-else :size="18" />
            </span>
            <div class="class-detail__assign-main">
              <div class="class-detail__assign-info">
                <p class="class-detail__assign-title">{{ assignmentTitle(assign) }}</p>
                <p class="class-detail__assign-due">
                  <CalendarClock :size="13" aria-hidden="true" />
                  {{ assign.dueAt ? messages.classes.detailDue(formatDate(assign.dueAt)) : messages.classes.detailDueNone }}
                </p>
              </div>
              <!-- Task 2: tiến độ nộp bài (dữ liệu thật từ report) + badge hạn nộp semantic -->
              <div v-if="assignStatsOf(assign)" class="class-detail__assign-progress">
                <div class="class-detail__assign-progress-row">
                  <span class="class-detail__assign-progress-count">
                    {{
                      messages.classes.detailSubmittedCount(
                        assignStatsOf(assign)!.onTime + assignStatsOf(assign)!.late,
                        reportTotalMembers,
                      )
                    }}
                  </span>
                  <span
                    v-if="deadlineBadge(assign) !== null"
                    class="class-detail__deadline"
                    :class="`class-detail__deadline--${deadlineBadge(assign)!.tone}`"
                  >
                    <component :is="deadlineIcon(deadlineBadge(assign)!.tone)" :size="12" aria-hidden="true" />
                    {{ deadlineBadge(assign)!.label }}
                  </span>
                </div>
                <ProgressBar :value="submissionPct(assign)" variant="success" size="sm" />
              </div>
            </div>
            <div class="class-detail__assign-actions">
              <Badge v-if="assign.allowLateSubmission" variant="muted">
                {{ messages.classes.detailLateBadge }}
              </Badge>
              <Button v-if="isNavigable(assign)" size="sm" variant="secondary" @click.stop="openAssignment(assign)">
                {{ messages.classes.detailDoBtn }} <ArrowRight :size="14" aria-hidden="true" />
              </Button>
              <template v-if="isManager">
                <Button size="sm" variant="ghost" @click.stop="openEdit(assign)">
                  <Pencil :size="14" aria-hidden="true" /> {{ messages.classes.detailEditDueBtn }}
                </Button>
                <Button size="sm" variant="danger" @click.stop="confirmAssignDelete = assign.id">
                  <Trash2 :size="14" aria-hidden="true" /> {{ messages.classes.detailDeleteAssignBtn }}
                </Button>
              </template>
            </div>
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
      <form class="class-detail__assign-form" novalidate @submit.prevent="createAssignment">
        <Select
          v-model="assignType"
          :label="messages.classes.detailAssignTypeLabel"
          :options="[
            { label: messages.classes.detailAssignTypeLesson, value: 'lesson' },
            { label: messages.classes.detailAssignTypeExercise, value: 'exercise' },
          ]"
        />
        <Select
          v-model="assignItem"
          :label="messages.classes.detailAssignItemLabel"
          :options="assignOptions"
          :placeholder="messages.classes.detailAssignItemPlaceholder"
          :disabled="assignLoading"
        />
        <p class="class-detail__modal-note">{{ messages.classes.detailAssignNote }}</p>
        <Input
          id="assign-due"
          v-model="assignDue"
          type="datetime-local"
          :label="messages.classes.detailAssignDueLabel"
        />
        <label class="class-detail__late">
          <input v-model="assignLate" type="checkbox" />
          {{ messages.classes.detailAssignLateLabel }}
        </label>
        <div class="class-detail__modal-actions">
          <Button variant="ghost" @click="assignOpen = false">{{ messages.classes.cancel }}</Button>
          <Button type="submit" :loading="assignSubmitting">{{ messages.classes.detailAssignSubmit }}</Button>
        </div>
      </form>
    </Modal>

    <!-- Modal sửa hạn nộp bài gán -->
    <Modal :open="editAssign !== null" :title="messages.classes.detailEditDueTitle" @close="editAssign = null">
      <form class="class-detail__assign-form" novalidate @submit.prevent="saveEdit">
        <Input
          id="edit-due"
          v-model="editDue"
          type="datetime-local"
          :label="messages.classes.detailAssignDueLabel"
        />
        <label class="class-detail__late">
          <input v-model="editLate" type="checkbox" />
          {{ messages.classes.detailAssignLateLabel }}
        </label>
        <div class="class-detail__modal-actions">
          <Button variant="ghost" @click="editAssign = null">{{ messages.classes.cancel }}</Button>
          <Button type="submit" :loading="editSaving">{{ messages.classes.detailEditDueSubmit }}</Button>
        </div>
      </form>
    </Modal>

    <!-- Modal xác nhận xóa bài gán -->
    <Modal :open="confirmAssignDelete !== null" :title="messages.classes.detailDeleteAssignTitle" @close="confirmAssignDelete = null">
      <p class="class-detail__modal-text">{{ messages.classes.detailDeleteAssignConfirm }}</p>
      <template #footer>
        <Button variant="ghost" @click="confirmAssignDelete = null">{{ messages.classes.cancel }}</Button>
        <Button variant="danger" @click="removeAssignment(confirmAssignDelete ?? 0)">
          <Trash2 :size="14" aria-hidden="true" /> {{ messages.classes.detailDeleteAssignBtn }}
        </Button>
      </template>
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

/* ── Banner: PageHero shared — chỉ giữ override max-width desc (cũ 70ch vs 60ch) ── */
:deep(.page-hero__desc) {
  max-width: 70ch;
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

/* FIX R1: min-width bảng → tablet/desktop cuộn ngang TRONG card thay vì cột chật;
   mobile (≤640) bỏ min-width vì chuyển card-stack (media bên dưới). */
.class-detail__table table { width: 100%; min-width: 680px; border-collapse: collapse; }

.class-detail__table th {
  text-align: left;
  font-size: var(--text-sm);
  font-weight: 500;
  color: var(--foreground-secondary);
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
  color: var(--foreground-secondary);
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

.class-detail__assign--link { cursor: pointer; }
.class-detail__assign--link:focus-visible { outline: 2px solid var(--ring); outline-offset: 2px; }

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

.class-detail__assign-main {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.class-detail__assign-info { min-width: 0; display: flex; flex-direction: column; gap: var(--space-xs); }

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
  color: var(--foreground-secondary);
  white-space: nowrap;
}

/* ── Task 2: tiến độ nộp bài của bài gán (dữ liệu report thật) ── */
.class-detail__assign-progress {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  max-width: 420px;
}

.class-detail__assign-progress-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
  flex-wrap: wrap;
}

.class-detail__assign-progress-count {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-secondary);
  letter-spacing: 0.08em;
  white-space: nowrap;
}

/* Badge hạn nộp: outline mono + semantic (Task 2 — DESIGN §4.3) + icon lucide (FIX R1) */
.class-detail__deadline {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  min-height: 24px;
  padding: 2px 10px;
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  white-space: nowrap;
}

.class-detail__deadline--success {
  border-color: color-mix(in srgb, var(--success) 45%, transparent);
  background: color-mix(in srgb, var(--success) 10%, transparent);
  color: var(--success);
}

.class-detail__deadline--warning {
  border-color: color-mix(in srgb, var(--warning) 45%, transparent);
  background: color-mix(in srgb, var(--warning) 10%, transparent);
  color: var(--warning);
}

.class-detail__deadline--destructive {
  border-color: color-mix(in srgb, var(--destructive) 45%, transparent);
  background: color-mix(in srgb, var(--destructive) 10%, transparent);
  color: var(--destructive);
}

.class-detail__deadline--muted {
  color: var(--foreground-secondary);
}

/* FIX R1: progress bar nộp bài — animated width (transform transition, easing chủ đích) */
.class-detail__assign-progress :deep([data-value]) {
  transition: transform 300ms cubic-bezier(0.16, 1, 0.3, 1);
}

/* ── Task 2: cột "Bài chưa nộp" (dữ liệu thật từ report.laggingLearners) ── */
.class-detail__missing {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  white-space: nowrap;
}

.class-detail__missing--ok { color: var(--resolved); }

.class-detail__missing-badge {
  display: inline-flex;
  align-items: center;
  min-height: 24px;
  padding: 2px 10px;
  border-radius: var(--radius-md);
  border: 1px solid color-mix(in srgb, var(--destructive) 35%, transparent);
  background: color-mix(in srgb, var(--destructive) 10%, transparent);
  color: var(--destructive);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
}

.class-detail__missing-text {
  font-size: var(--text-xs);
  color: var(--foreground-secondary);
}

/* ── Task 2: phân trang bảng học viên ── */
.class-detail__pager {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  border-top: 1px solid var(--border);
  flex-wrap: wrap;
}

.class-detail__pager-info {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-secondary);
  letter-spacing: 0.08em;
  font-variant-numeric: tabular-nums;
}

.class-detail__pager-buttons {
  display: flex;
  gap: var(--space-sm);
}

.class-detail__assign-actions {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  flex-wrap: wrap;
  flex-shrink: 0;
}

/* ── Form gán / sửa hạn ── */
.class-detail__assign-form { display: flex; flex-direction: column; gap: var(--space-sm); }

.class-detail__late {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-size: var(--text-sm);
  color: var(--foreground);
  cursor: pointer;
  user-select: none;
}

.class-detail__late input { accent-color: var(--ring); width: 16px; height: 16px; }

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

.class-detail__modal-note { font-size: var(--text-xs); color: var(--foreground-secondary); margin-top: var(--space-sm); }

/* FIX R1: tab active rõ hơn — thêm weight 600 cho tab đang chọn (bên dưới border primary) */
.class-detail :deep([role="tab"][data-state="active"]) {
  font-weight: 600;
}

@media (max-width: 640px) {
  /* Bảng → card-stack (DESIGN §8 — cấm scroll ngang bảng chính ở mobile) */
  .class-detail__table-scroll { overflow-x: visible; }

  .class-detail__table table { min-width: 0; }

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
    color: var(--foreground-secondary);
  }

  .class-detail__table td:first-child { grid-column: 1 / -1; }
  .class-detail__table td:last-child { align-items: flex-start; }

  .class-detail__name,
  .class-detail__email { max-width: 100%; white-space: normal; }
}
</style>
