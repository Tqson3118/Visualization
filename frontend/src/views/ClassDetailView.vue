<script setup lang="ts">
// ClassDetailView — Màn 20: 3 tab (Thành viên / Lộ trình đã gán / Cài đặt)
// View-quality Phase 1 (Nhóm D): banner = surface band level-2; mã mời =
// block-token tối canvas-ink (quyết định #4/#5); bảng chuẩn §4.6 + mobile
// card-stack; assignment có index mono; bỏ gradient/glassmorphism/hover-lift.
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  AlertTriangle,
  ArrowDown,
  ArrowRight,
  ArrowUp,
  BarChart3,
  BookOpen,
  CalendarClock,
  Check,
  CheckCircle2,
  ChevronLeft,
  ChevronRight,
  ClipboardCopy,
  Clock,
  Code2,
  Download,
  ExternalLink,
  KeyRound,
  Layers,
  Pencil,
  Play,
  Plus,
  Puzzle,
  Settings2,
  Sparkles,
  Timer,
  Trash2,
  UserPlus,
  Users,
  X,
  Zap,
} from 'lucide-vue-next';
import type { Component } from 'vue';

import { useClassStore } from '@/stores/classStore';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import * as classesApi from '@/api/classes';
import * as lessonsApi from '@/api/lessons';
import * as exercisesApi from '@/api/exercises';
import { courseApi, type CourseListDto } from '@/services/courseApi';
import { formatDate } from '@/utils/format';
import { messages } from '@/i18n/vi';
import type { ClassCurriculumItemDto } from '@/api/types';
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
import PathModuleList, { type PathModuleGroup, type PathModuleLesson } from '@/components/path/PathModuleList.vue';
import { updateClassDeadline } from '@/api/pathItems';

const route = useRoute();
const router = useRouter();
const classStore = useClassStore();
const auth = useAuthStore();
const ui = useUiStore();

const classId = computed(() => Number(route.params.id));
const tab = ref<'members' | 'curriculum' | 'settings'>('curriculum');
const loading = ref(true);

const confirmRemove = ref<number | null>(null);
const confirmDelete = ref(false);
const copied = ref(false);
const addEmail = ref('');
const addMemberOpen = ref(false);

// ── Learning Path / Curriculum (per-class) — lớp TRỎ vào lộ trình (D2) ──
// ── Main Door: Select/Change Active Learning Path ──
const selectPathModalOpen = ref(false);
const availablePaths = ref<CourseListDto[]>([]);
const loadingPaths = ref(false);
const selectedPathId = ref<number | null>(null);
const bindingPath = ref(false);

async function openSelectPathModal(): Promise<void> {
  selectPathModalOpen.value = true;
  selectedPathId.value = classStore.currentClass?.learningPathId ?? null;
  loadingPaths.value = true;
  try {
    availablePaths.value = await courseApi.getCourses();
  } catch {
    availablePaths.value = [];
  } finally {
    loadingPaths.value = false;
  }
}

async function handleSetLearningPath(pathId: number | null): Promise<void> {
  bindingPath.value = true;
  try {
    await classStore.setLearningPath(classId.value, pathId);
    ui.showToast(pathId ? 'Đã gán Lộ trình cho lớp thành công!' : 'Đã gỡ lộ trình khỏi lớp.', 'success');
    selectPathModalOpen.value = false;
    await classStore.fetchClass(classId.value);
    await classStore.fetchCurriculum(classId.value);
    if (isManager.value) void loadReport();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể gán lộ trình.', 'error');
  } finally {
    bindingPath.value = false;
  }
}

// ── Class Modules Grouping & Deadlines (reusing /path styling per D4) ──
const FOLDER_TYPES = new Set(['folder', 'Folder', 0]);

function isFolderItem(t: ClassCurriculumItemDto['itemType']): boolean {
  return FOLDER_TYPES.has(t as string | number);
}

function mapItemType(t: ClassCurriculumItemDto['itemType']): 'theory' | 'quiz' | 'codelab' {
  const s = String(t).toLowerCase();
  if (s === 'quiz' || s === '2') return 'quiz';
  if (s === 'lab' || s === 'codelab' || s === 'exercise' || s === '3') return 'codelab';
  return 'theory';
}

/** Cây MỚI: items là DFS của cây lộ trình — Folder = module tiêu đề, Page = bài học. */
const classModules = computed<PathModuleGroup[]>(() => {
  const items = classStore.curriculum?.items ?? [];
  if (items.length === 0) return [];

  const modules: PathModuleGroup[] = [];
  let current: PathModuleGroup | null = null;

  const pushLesson = (item: ClassCurriculumItemDto): void => {
    if (!current) {
      current = { title: item.topicName || 'Nội dung lộ trình', lessons: [] };
      modules.push(current);
    }
    current.lessons.push({
      id: item.pathItemId ?? item.assignmentId ?? item.lessonId ?? item.exerciseId ?? item.title,
      pathItemId: item.pathItemId ?? item.assignmentId,
      lessonId: item.lessonId ?? undefined,
      assignmentId: item.assignmentId,
      title: item.title,
      sandboxType: mapItemType(item.itemType),
      status: item.status === 'completed' ? 'Completed' : item.status,
      isCompleted: item.status === 'completed',
      locked: false,
      isLocked: false,
      dueAt: item.dueAt,
      allowLateSubmission: item.allowLateSubmission,
      bestScore: item.bestScore,
      xpReward: item.xpReward,
    });
  };

  const walk = (list: ClassCurriculumItemDto[]): void => {
    for (const item of list) {
      if (isFolderItem(item.itemType)) {
        current = { title: item.title || item.topicName || 'Chủ đề bài học', lessons: [] };
        modules.push(current);
        if (Array.isArray(item.children) && item.children.length) walk(item.children);
        continue;
      }
      pushLesson(item);
    }
  };

  walk(items);
  return modules.filter((m) => m.lessons.length > 0 || modules.length === 1);
});

/** Mục kế tiếp chưa hoàn thành (DFS) — cho nút "Tiếp tục học". */
const nextUpItem = computed<ClassCurriculumItemDto | null>(() => {
  const all: ClassCurriculumItemDto[] = [];
  const collect = (list: ClassCurriculumItemDto[]): void => {
    for (const item of list) {
      if (!isFolderItem(item.itemType)) all.push(item);
      if (Array.isArray(item.children) && item.children.length) collect(item.children);
    }
  };
  collect(classStore.curriculum?.items ?? []);
  return all.find((i) => i.status !== 'completed') ?? null;
});

const continueLabel = computed(() => {
  const items = classStore.curriculum?.items ?? [];
  const anyDone = items.some((i) => i.status === 'completed');
  return anyDone ? 'Tiếp tục học' : 'Bắt đầu học';
});

function continueLearning(): void {
  const item = nextUpItem.value;
  if (!item) return;
  if (item.lessonId) {
    void router.push({ name: 'lesson-study', params: { id: String(item.lessonId) } });
    return;
  }
  const pathId = classStore.currentClass?.learningPathId;
  const targetId = item.pathItemId ?? item.assignmentId;
  if (pathId && targetId) {
    void router.push({ path: '/learn/' + pathId + '/' + targetId });
  }
}

function handleSelectModuleLesson(lesson: PathModuleLesson): void {
  if (lesson.lessonId) {
    if (classStore.currentClass?.learningPathId) {
      void router.push({ path: `/learn/${classStore.currentClass.learningPathId}/${lesson.lessonId}` });
    } else {
      void router.push({ name: 'lesson-study', params: { id: String(lesson.lessonId) } });
    }
  } else if (lesson.pathItemId && classStore.currentClass?.learningPathId) {
    void router.push({ path: `/learn/${classStore.currentClass.learningPathId}/${lesson.pathItemId}` });
  }
}

// ── Deadline Editing (Per-lesson overlay) ──
const deadlineModalOpen = ref(false);
const editingLessonDeadline = ref<{
  lessonId?: number;
  pathItemId?: number;
  title: string;
  dueAt: string;
  allowLateSubmission: boolean;
} | null>(null);
const savingDeadline = ref(false);

function openDeadlineModal(item: ClassCurriculumItemDto | PathModuleLesson): void {
  const lessonId = (item as any).lessonId;
  const pathItemId = (item as any).pathItemId;
  if (!lessonId && !pathItemId) return;
  editingLessonDeadline.value = {
    lessonId: lessonId,
    pathItemId: pathItemId,
    title: item.title,
    dueAt: item.dueAt ? toLocalInput(item.dueAt) : '',
    allowLateSubmission: item.allowLateSubmission ?? true,
  };
  deadlineModalOpen.value = true;
}

async function handleSaveDeadline(): Promise<void> {
  if (!editingLessonDeadline.value) return;
  savingDeadline.value = true;
  try {
    const dueAtIso = editingLessonDeadline.value.dueAt ? new Date(editingLessonDeadline.value.dueAt).toISOString() : null;
    if (editingLessonDeadline.value.pathItemId) {
      await updateClassDeadline(classId.value, {
        pathItemId: editingLessonDeadline.value.pathItemId,
        dueAt: dueAtIso,
        allowLateSubmission: editingLessonDeadline.value.allowLateSubmission,
      });
    } else if (editingLessonDeadline.value.lessonId) {
      await classesApi.updateLessonDeadline(classId.value, editingLessonDeadline.value.lessonId, {
        dueAt: dueAtIso,
        allowLateSubmission: editingLessonDeadline.value.allowLateSubmission,
      });
    }
    ui.showToast('Đã cập nhật hạn nộp.', 'success');
    deadlineModalOpen.value = false;
    await classStore.fetchCurriculum(classId.value);
    if (isManager.value) void loadReport();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể lưu hạn nộp.', 'error');
  } finally {
    savingDeadline.value = false;
  }
}

// B4: assignmentStats/deadlineBadge/submissionPct removed — assignments tab bị bỏ
// Chỉ giữ laggingMap cho cột "Bài chưa nộp" trong bảng thành viên.
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
    // B4: Chỉ giữ laggingMap cho cột "Bài chưa nộp" trong bảng thành viên.
    const lagging: Record<number, number> = {};
    for (const l of report.laggingLearners) lagging[l.userId] = l.missingCount;
    laggingMap.value = lagging;
    reportLoaded.value = true;
  } catch {
    // Báo cáo là quyền giảng viên — lỗi không chặn chi tiết lớp.
  }
}

/** Số bài thiếu của 1 học viên (0 = không trong danh sách chậm tiến độ của report). */
const missingOf = (userId: number): number => laggingMap.value[userId] ?? 0;

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

// B4: Bỏ tab 'assignments' — lớp học chỉ học theo lộ trình GV tạo sẵn.
const detailTabs = computed<Array<{ key: 'members' | 'curriculum' | 'settings'; label: string }>>(() => {
  const tabs: Array<{ key: 'members' | 'curriculum' | 'settings'; label: string }> = [
    { key: 'members', label: messages.classes.detailTabMembers },
    { key: 'curriculum', label: messages.classes.curriculumTab },
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
    // Curriculum: nạp lộ trình học (học viên: status từ progress thật) — không chặn màn.
    void classStore.fetchCurriculum(classId.value).catch(() => undefined);
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
    if (isManager.value) void loadReport();
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
    await classStore.addMember(classId.value, addEmail.value.trim());
    ui.showToast(messages.classes.detailAddSuccess, 'success');
    addEmail.value = '';
    addMemberOpen.value = false;
    if (isManager.value) void loadReport();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.detailAddFailed, 'error');
  }
}

/** ISO → giá trị cho input datetime-local (múi giờ địa phương). */
function toLocalInput(iso: string): string {
  const d = new Date(iso);
  const pad = (n: number): string => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

// B4: openEdit/saveEdit/removeAssignment/openAssignment/isNavigable removed — assignments tab bị bỏ.

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
  void navigator.clipboard?.writeText(code)
    .then(() => {
      ui.showToast(messages.classes.detailCopied, 'success');
      copied.value = true;
      if (copyTimer) clearTimeout(copyTimer);
      copyTimer = setTimeout(() => {
        copied.value = false;
      }, 1600);
    })
    .catch(() => {
      // FIX B4 — clipboard từ chối/không khả dụng → báo lỗi, không hiện trạng thái copy giả.
      ui.showToast(messages.classes.detailCopyFailed, 'error');
    });
}

function cleanTitle(title?: string): string {
  if (!title) return '';
  return title.replace(/Mini-Quizz/gi, 'Mini-Quiz');
}

</script>

<template>
  <section class="class-detail container">
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
            <!-- FIX B1 — bỏ RouterLink bọc Button (button trong anchor = HTML không hợp lệ):
                 dùng @click router.push giữ nguyên visual Button. -->
            <Button
              v-if="isManager"
              size="md"
              variant="secondary"
              class="class-detail__hero-link"
              @click="router.push({ name: 'class-report', params: { id: String(classId) } })"
            >
              <BarChart3 :size="14" /> {{ messages.classes.detailReportBtn }} <ArrowRight :size="14" aria-hidden="true" />
            </Button>
          </div>
        </template>
      </PageHero>

      <!-- Tabs shadcn: Thành viên / Lộ trình đã gán / Cài đặt -->
      <Tabs :tabs="detailTabs" v-model="tab" @change="tab = $event as typeof tab" />

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

      <!-- Tab Lộ trình học (curriculum — feature port: teacher tạo path, student xem status) -->
      <section v-else-if="tab === 'curriculum'" class="class-detail__panel space-y-4">
        <!-- Banner Lộ trình Active của Lớp (Mô hình hợp nhất) -->
        <Card class="p-4 sm:p-5 bg-vdsa-surface border border-vdsa-border rounded-2xl space-y-4">
          <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3">
            <div class="space-y-1">
              <div class="flex items-center gap-2 flex-wrap">
                <span class="text-xs font-bold text-purple-400 uppercase tracking-wider flex items-center gap-1.5">
                  <Layers :size="15" /> Lộ trình học chính
                </span>
                <Badge
                  v-if="classStore.currentClass?.curriculumPublished"
                  variant="success"
                  class="text-[11px]"
                >
                  Đã xuất bản cho học viên
                </Badge>
                <Badge
                  v-else
                  variant="warning"
                  class="text-[11px]"
                >
                  Bản nháp (Học viên chưa thấy)
                </Badge>
              </div>
              <h2 class="text-lg sm:text-xl font-black text-white">
                {{ classStore.currentClass?.learningPathTitle || classStore.curriculum?.title || classStore.currentClass?.curriculumTitle || 'Chưa chọn lộ trình cho lớp' }}
              </h2>
              <p v-if="classStore.curriculum?.description" class="text-xs text-slate-400 line-clamp-2">
                {{ classStore.curriculum.description }}
              </p>
            </div>

            <!-- Học ngay mục kế tiếp (tiến độ theo cây — D9) -->
            <div class="flex items-center gap-2 flex-wrap shrink-0">
              <Button
                v-if="nextUpItem"
                size="sm"
                variant="primary"
                data-testid="continue-learning"
                class="gap-1.5 bg-purple-600 hover:bg-purple-500"
                @click="continueLearning"
              >
                <Play :size="14" aria-hidden="true" /> {{ continueLabel }}
              </Button>
              <template v-if="isManager">
                <Button size="sm" variant="secondary" class="gap-1.5" @click="openSelectPathModal">
                  <Layers :size="14" /> {{ classStore.currentClass?.learningPathId ? 'Đổi Lộ trình khác' : 'Chọn Lộ trình giảng dạy' }}
                </Button>
                <Button
                  v-if="classStore.currentClass?.learningPathId"
                  size="sm"
                  variant="secondary"
                  class="gap-1.5"
                  @click="router.push({ path: '/studio', query: { tab: 'curriculum', courseId: String(classStore.currentClass.learningPathId) } })"
                >
                  <ExternalLink :size="14" /> Soạn trong Studio
                </Button>
              </template>
            </div>
          </div>

          <!-- Overall Progress Bar -->
          <div class="pt-2 border-t border-vdsa-border/60 flex items-center gap-4">
            <div class="flex-1">
              <div class="flex items-center justify-between text-xs text-slate-300 font-semibold mb-1">
                <span>Tiến độ hoàn thành</span>
                <span class="font-mono text-emerald-400 font-bold">{{ classStore.curriculum?.progressPct ?? 0 }}%</span>
              </div>
              <ProgressBar :value="classStore.curriculum?.progressPct ?? 0" variant="success" />
            </div>
            <span class="text-xs text-slate-400 font-mono shrink-0">
              {{ classStore.curriculum?.items.length ?? 0 }} bài học
            </span>
          </div>
        </Card>

        <div v-if="classStore.curriculumLoading" class="space-y-3 py-4">
          <Skeleton v-for="i in 5" :key="i" height="60px" class="rounded-xl" />
        </div>

        <!-- Guided Empty State -->
        <div
          v-else-if="!classStore.curriculum?.items || classStore.curriculum.items.length === 0"
          class="p-8 sm:p-12 rounded-2xl border-2 border-dashed border-vdsa-border bg-card/40 text-center space-y-4 max-w-xl mx-auto my-4"
        >
          <div class="w-12 h-12 rounded-2xl bg-purple-500/15 text-purple-300 flex items-center justify-center mx-auto">
            <BookOpen :size="24" />
          </div>
          <div class="space-y-1">
            <h3 class="text-base font-bold text-white">Lớp học chưa có Lộ trình giảng dạy</h3>
            <p class="text-xs text-slate-400 max-w-md mx-auto leading-relaxed">
              Hãy chọn một <strong>Lộ trình học</strong> (Course) từ thư viện của bạn hoặc toàn sàn để gán cho lớp. Mọi học sinh trong lớp sẽ học theo lộ trình này.
            </p>
          </div>
          <div v-if="isManager" class="flex flex-col sm:flex-row items-center justify-center gap-2.5 pt-2">
            <Button variant="primary" size="md" class="w-full sm:w-auto gap-1.5 bg-purple-600 hover:bg-purple-500" @click="openSelectPathModal">
              <Layers :size="15" /> Chọn Lộ trình cho lớp ngay
            </Button>
            <Button variant="secondary" size="md" class="w-full sm:w-auto gap-1.5" @click="router.push('/studio?tab=curriculum')">
              <Plus :size="15" /> Tạo Lộ trình mới tại Studio
            </Button>
          </div>
        </div>

        <!-- Unified Module Tree List (using PathModuleList per Decision D4) -->
        <div v-else class="space-y-4">
          <PathModuleList
            :modules="classModules"
            :show-deadlines="true"
            :is-teacher="isManager"
            @select-lesson="handleSelectModuleLesson"
            @edit-deadline="openDeadlineModal"
          />
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

    <!-- Modal Gán / Đổi Lộ trình cho Lớp (Cửa chính) -->
    <Modal :open="selectPathModalOpen" title="Chọn Lộ trình giảng dạy cho Lớp học" width="650px" @close="selectPathModalOpen = false">
      <div class="space-y-4">
        <p class="text-xs sm:text-sm text-slate-300 leading-relaxed">
          Một lớp học chỉ gắn với <strong>1 Lộ trình active</strong> tại một thời điểm. Mọi thay đổi nội dung trên Lộ trình sẽ tự động cập nhật đến tất cả học sinh trong lớp.
        </p>

        <div v-if="loadingPaths" class="py-8 text-center text-sm text-slate-400">
          <div class="inline-block w-6 h-6 border-2 border-purple-500 border-t-transparent rounded-full animate-spin mb-2"></div>
          <p>Đang tải danh sách lộ trình khả dụng...</p>
        </div>

        <div v-else-if="availablePaths.length === 0" class="py-8 text-center text-sm text-slate-400 space-y-2">
          <p>Chưa có lộ trình nào trên hệ thống.</p>
          <Button size="sm" variant="secondary" @click="router.push('/studio?tab=curriculum')">
            <Plus :size="14" /> Tạo Lộ trình mới tại Studio
          </Button>
        </div>

        <div v-else data-testid="learning-path-select" class="space-y-2 max-h-72 overflow-y-auto pr-1">
          <label
            v-for="p in availablePaths"
            :key="p.id"
            class="flex items-start gap-3 p-3 rounded-xl border cursor-pointer transition-all"
            :class="
              selectedPathId === Number(p.id)
                ? 'bg-purple-950/40 border-purple-500 ring-1 ring-purple-500/50'
                : 'bg-vdsa-surface border-vdsa-border hover:border-slate-600'
            "
          >
            <input
              v-model="selectedPathId"
              type="radio"
              name="select-path"
              :value="Number(p.id)"
              class="mt-1 accent-purple-600 cursor-pointer"
            />
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 flex-wrap">
                <span class="font-bold text-white text-sm truncate">{{ p.title }}</span>
                <Badge :variant="p.status === 'active' ? 'success' : p.status === 'class' ? 'secondary' : 'muted'">
                  {{ p.status === 'active' ? 'Công khai' : p.status === 'class' ? 'Dành cho Lớp' : 'Bản nháp' }}
                </Badge>
              </div>
              <p class="text-xs text-slate-400 line-clamp-1 mt-0.5">{{ p.description }}</p>
              <div class="flex items-center gap-3 text-[11px] text-purple-300 font-semibold mt-1">
                <span>{{ p.totalLessons || p.lessons?.length || 0 }} bài học</span>
                <span>·</span>
                <span>{{ p.xpReward || 0 }} XP</span>
                <span v-if="p.authorName">· Tác giả: {{ p.authorName }}</span>
              </div>
            </div>
          </label>
        </div>
      </div>

      <template #footer>
        <div class="flex items-center justify-between w-full gap-2 flex-wrap">
          <div>
            <Button
              v-if="classStore.currentClass?.learningPathId"
              variant="danger"
              size="sm"
              :loading="bindingPath"
              @click="handleSetLearningPath(null)"
            >
              Gỡ lộ trình khỏi lớp
            </Button>
          </div>
          <div class="flex items-center gap-2">
            <Button variant="ghost" size="sm" @click="selectPathModalOpen = false">{{ messages.classes.cancel }}</Button>
            <Button
              variant="primary"
              size="sm"
              data-testid="learning-path-confirm"
              class="bg-purple-600 hover:bg-purple-500"
              :disabled="selectedPathId === null || bindingPath"
              :loading="bindingPath"
              @click="handleSetLearningPath(selectedPathId)"
            >
              Xác nhận gán cho lớp
            </Button>
          </div>
        </div>
      </template>
    </Modal>

    <!-- Modal Cập nhật Deadline cho bài học trong lớp -->
    <Modal :open="deadlineModalOpen" title="Cài đặt Hạn nộp & Deadline" width="460px" @close="deadlineModalOpen = false">
      <div v-if="editingLessonDeadline" class="space-y-4">
        <div>
          <span class="text-xs text-slate-400 block mb-1">Bài học:</span>
          <p class="text-sm font-bold text-white">{{ cleanTitle(editingLessonDeadline.title) }}</p>
        </div>

        <div>
          <label class="block text-xs font-bold text-slate-300 uppercase mb-1.5">Hạn nộp bài (Deadline)</label>
          <input
            v-model="editingLessonDeadline.dueAt"
            type="datetime-local"
            class="w-full bg-vdsa-surface border border-vdsa-border rounded-xl px-3 py-2 text-sm text-white focus:outline-none focus:border-purple-500"
          />
          <span class="text-[11px] text-slate-500 mt-1 block">Để trống nếu không muốn áp dụng hạn nộp.</span>
        </div>

        <div class="flex items-center gap-2.5 pt-2">
          <input
            id="allow-late-toggle"
            v-model="editingLessonDeadline.allowLateSubmission"
            type="checkbox"
            class="w-4 h-4 rounded text-purple-600 focus:ring-purple-500 accent-purple-600 cursor-pointer"
          />
          <label for="allow-late-toggle" class="text-xs font-semibold text-slate-300 cursor-pointer select-none">
            Cho phép nộp muộn sau deadline (sẽ gắn nhãn "Nộp muộn" trong báo cáo)
          </label>
        </div>
      </div>

      <template #footer>
        <Button variant="ghost" size="sm" @click="deadlineModalOpen = false">{{ messages.classes.cancel }}</Button>
        <Button variant="primary" size="sm" data-testid="deadline-save" class="bg-purple-600 hover:bg-purple-500" :loading="savingDeadline" @click="handleSaveDeadline">
          Lưu hạn nộp
        </Button>
      </template>
    </Modal>
  </section>
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

/* ── Lộ trình học (curriculum) ── */
.class-detail__curriculum-meta {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  max-width: 640px;
}

.class-detail__curriculum-meta-head {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-sm);
  flex-wrap: wrap;
}

.class-detail__curriculum-meta-title { font-size: var(--text-lg); font-weight: 600; letter-spacing: -0.015em; }
.class-detail__curriculum-meta-note { font-size: var(--text-sm); color: var(--foreground-secondary); }
.class-detail__curriculum-actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

.class-detail__curriculum-list { display: flex; flex-direction: column; gap: var(--space-sm); }

.class-detail__curriculum-row {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-sm) var(--space-md);
  min-width: 0;
  cursor: pointer;
}

.class-detail__curriculum-row:focus-visible { outline: 2px solid var(--ring); outline-offset: 2px; }

.class-detail__curriculum-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  min-width: 28px;
  flex-shrink: 0;
}

.class-detail__curriculum-row-main {
  flex: 1;
  min-width: 0;
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  flex-wrap: wrap;
}

.class-detail__curriculum-row-title {
  font-weight: 500;
  font-size: var(--text-sm);
  color: var(--foreground);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.class-detail__curriculum-row-actions {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  flex-shrink: 0;
}

.class-detail__curriculum-student-head { display: flex; flex-direction: column; gap: var(--space-xs); }

.class-detail__curriculum-student-title { font-size: var(--text-xl); font-weight: 600; letter-spacing: -0.02em; }
.class-detail__curriculum-student-desc { font-size: var(--text-sm); color: var(--foreground-secondary); }

.class-detail__curriculum-draft-msg {
  font-size: var(--text-xs);
  color: var(--warning);
  display: inline-flex;
  gap: var(--space-xs);
  align-items: center;
}

.class-detail__curriculum-progress {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  max-width: 420px;
}

.class-detail__curriculum-progress-label {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-secondary);
  letter-spacing: 0.08em;
  font-variant-numeric: tabular-nums;
}

.class-detail__curriculum-student-list { margin-top: var(--space-xs); }

</style>
