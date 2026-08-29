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
  KeyRound,
  Pencil,
  Puzzle,
  Save,
  Send,
  Timer,
  Trash2,
  UserPlus,
  Users,
  Download,
  X,
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
import type { ClassAssignmentDto, ClassCurriculumItemDto } from '@/api/types';
import { buildReorderItems } from '@/utils/curriculumOrder';
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
const tab = ref<'members' | 'curriculum' | 'settings'>('members');
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

// B4: editAssign/confirmAssignDelete removed — assignments tab bị bỏ

// ── Learning Path / Curriculum (per-class) ──
const curriculumTitle = ref('');
const curriculumDesc = ref('');
const curriculumBusy = ref(false);
const curriculumRemoveId = ref<number | null>(null);

// Modal Nhập từ Lộ trình có sẵn
const importCourseModalOpen = ref(false);
const availableCourses = ref<CourseListDto[]>([]);
const loadingCourses = ref(false);
const selectedCourseIdToImport = ref<number | null>(null);
const importingCourse = ref(false);

async function openImportCourseModal(): Promise<void> {
  importCourseModalOpen.value = true;
  selectedCourseIdToImport.value = null;
  loadingCourses.value = true;
  try {
    availableCourses.value = await courseApi.getCourses();
  } catch {
    availableCourses.value = [];
  } finally {
    loadingCourses.value = false;
  }
}

async function handleImportCourse(): Promise<void> {
  if (!selectedCourseIdToImport.value) return;
  importingCourse.value = true;
  try {
    await classesApi.importCourseToClass(classId.value, selectedCourseIdToImport.value);
    ui.showToast('Đã nạp toàn bộ bài học từ Lộ trình vào Lớp thành công!', 'success');
    importCourseModalOpen.value = false;
    await classStore.fetchClass(classId.value);
    await classStore.fetchCurriculum(classId.value).catch(() => undefined);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể nạp lộ trình.', 'error');
  } finally {
    importingCourse.value = false;
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

// B4: Bỏ tab 'assignments' — lớp học chỉ học theo lộ trình GV tạo sẵn.
const detailTabs = computed<Array<{ key: 'members' | 'curriculum' | 'settings'; label: string }>>(() => {
  const tabs: Array<{ key: 'members' | 'curriculum' | 'settings'; label: string }> = [
    { key: 'members', label: messages.classes.detailTabMembers },
    { key: 'curriculum', label: messages.classes.curriculumTab },
  ];
  if (isManager.value) tabs.push({ key: 'settings', label: messages.classes.detailTabSettings });
  return tabs;
});

// Đồng bộ title/desc lộ trình khi currentClass tải xong / thay đổi.
watch(
  () => classStore.currentClass,
  (cls) => {
    curriculumTitle.value = cls?.curriculumTitle ?? '';
    curriculumDesc.value = cls?.curriculumDescription ?? '';
  },
  { immediate: true },
);

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
    if (isManager.value) void loadReport();
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

function assignmentTitle(assign: ClassAssignmentDto): string {
  if (assign.title) return cleanTitle(assign.title);
  if (assign.lessonId !== null) return cleanTitle(messages.classes.detailLesson(assign.lessonId));
  if (assign.exerciseId !== null) return cleanTitle(messages.classes.detailExercise(assign.exerciseId));
  return messages.classes.detailGenericContent;
}

// ── Learning Path / Curriculum (per-class) ──────────────────

/** Teacher: lưu tên/mô tả lộ trình (không đổi trạng thái publish). */
async function saveCurriculumMeta(): Promise<void> {
  curriculumBusy.value = true;
  try {
    await classStore.updateCurriculumMeta(classId.value, {
      title: curriculumTitle.value.trim() || null,
      description: curriculumDesc.value.trim() || null,
    });
    ui.showToast(messages.classes.curriculumMetaSaved, 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.curriculumMetaSaveFailed, 'error');
  } finally {
    curriculumBusy.value = false;
  }
}

/** Teacher: lưu nháp (unpublish) → học viên tạm ẩn lộ trình. */
async function saveDraft(): Promise<void> {
  curriculumBusy.value = true;
  try {
    await classStore.updateCurriculumMeta(classId.value, {
      title: curriculumTitle.value.trim() || null,
      description: curriculumDesc.value.trim() || null,
      published: false,
    });
    ui.showToast(messages.classes.curriculumDraftToast, 'success');
    void classStore.fetchCurriculum(classId.value).catch(() => undefined);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.curriculumFailed, 'error');
  } finally {
    curriculumBusy.value = false;
  }
}

/** Teacher: xuất bản lộ trình → học viên thấy. */
async function publishCurriculum(): Promise<void> {
  curriculumBusy.value = true;
  try {
    await classStore.updateCurriculumMeta(classId.value, {
      title: curriculumTitle.value.trim() || null,
      description: curriculumDesc.value.trim() || null,
      published: true,
    });
    ui.showToast(messages.classes.curriculumPublishedToast, 'success');
    void classStore.fetchCurriculum(classId.value).catch(() => undefined);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.curriculumFailed, 'error');
  } finally {
    curriculumBusy.value = false;
  }
}

/** Teacher: đổi vị trí 1 item (lên/xuống) → gửi reorder → reload. */
async function moveCurriculumItem(assign: ClassAssignmentDto, dir: -1 | 1): Promise<void> {
  if (curriculumBusy.value) return;
  const list = [...classStore.assignments];
  const idx = list.findIndex((a) => a.id === assign.id);
  const to = idx + dir;
  if (idx < 0 || to < 0 || to >= list.length) return;
  const items = buildReorderItems(list, idx, to);
  curriculumBusy.value = true;
  try {
    await classStore.reorderCurriculum(classId.value, items);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.curriculumReorderFailed, 'error');
  } finally {
    curriculumBusy.value = false;
  }
}

/** Teacher: xóa item khỏi lộ trình (modal xác nhận). */
async function removeCurriculumItem(): Promise<void> {
  if (curriculumRemoveId.value === null) return;
  try {
    await classStore.removeAssignment(classId.value, curriculumRemoveId.value);
    curriculumRemoveId.value = null;
    ui.showToast(messages.classes.curriculumRemoved, 'success');
    void classStore.fetchCurriculum(classId.value).catch(() => undefined);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.curriculumFailed, 'error');
  }
}

/** Student: mở nội dung của item trong lộ trình. */
function openCurriculumItem(item: ClassCurriculumItemDto): void {
  if (item.exerciseId !== null) {
    void router.push({
      name: 'exercise',
      params: { id: String(item.exerciseId) },
      query: { classAssignmentId: String(item.assignmentId) },
    });
  } else if (item.lessonId !== null) {
    void router.push({ name: 'lesson', params: { lessonId: String(item.lessonId) } });
  }
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

      <!-- B4: Tab Bài tập đã bị bỏ — lớp học chỉ dùng Tab Lộ trình -->

      <!-- Tab Lộ trình học (curriculum — feature port: teacher tạo path, student xem status) -->
      <section v-else-if="tab === 'curriculum'" class="class-detail__panel">
        <!-- Teacher: meta + publish/draft -->
        <Card v-if="isManager" class="class-detail__curriculum-meta">
          <div class="class-detail__curriculum-meta-head">
            <div class="class-detail__curriculum-meta-title-wrap">
              <h2 class="class-detail__curriculum-meta-title">{{ messages.classes.curriculumTitleLabel }}</h2>
              <p class="class-detail__curriculum-meta-note">{{ messages.classes.curriculumPublishHint }}</p>
            </div>
            <Badge :variant="classStore.currentClass?.curriculumPublished ? 'success' : 'warning'">
              {{
                classStore.currentClass?.curriculumPublished
                  ? messages.classes.curriculumPublishedBadge
                  : messages.classes.curriculumDraft
              }}
            </Badge>
          </div>
          <Input
            v-model="curriculumTitle"
            :placeholder="messages.classes.curriculumTitlePlaceholder"
            :aria-label="messages.classes.curriculumTitleLabel"
          />
          <Input
            v-model="curriculumDesc"
            :placeholder="messages.classes.curriculumDescPlaceholder"
            :aria-label="messages.classes.curriculumDescLabel"
          />
          <div class="class-detail__curriculum-actions">
            <Button size="sm" variant="secondary" :loading="curriculumBusy" @click="saveCurriculumMeta">
              <Save :size="14" aria-hidden="true" /> {{ messages.classes.curriculumSaveMeta }}
            </Button>
            <Button size="sm" variant="secondary" :loading="curriculumBusy" @click="saveDraft">
              {{ messages.classes.curriculumSaveDraft }}
            </Button>
            <Button size="sm" :loading="curriculumBusy" @click="publishCurriculum">
              <Send :size="14" aria-hidden="true" /> {{ messages.classes.curriculumPublish }}
            </Button>
          </div>
        </Card>

        <!-- Teacher: builder (thêm + sắp xếp + xóa) -->
        <template v-if="isManager">
          <div class="class-detail__toolbar flex items-center justify-between">
            <div class="flex items-center gap-2">
              <Button size="md" @click="openAssign">
                <BookOpen :size="14" aria-hidden="true" /> {{ messages.classes.curriculumAddBtn }}
              </Button>
              <Button size="md" variant="secondary" @click="openImportCourseModal">
                <Download :size="14" aria-hidden="true" /> Nhập từ Lộ trình có sẵn (Course)
              </Button>
            </div>
          </div>
          <EmptyState
            v-if="classStore.assignments.length === 0"
            icon="book"
            :title="messages.classes.curriculumEmpty"
            :description="messages.classes.curriculumEmptyDesc"
          />
          <div v-else class="class-detail__curriculum-list">
            <Card
              v-for="(assign, i) in classStore.assignments"
              :key="assign.id"
              class="class-detail__curriculum-row"
            >
              <span class="class-detail__curriculum-index" aria-hidden="true">#{{ pad(i + 1) }}</span>
              <span class="class-detail__assign-icon" aria-hidden="true">
                <Puzzle v-if="assign.exerciseId !== null" :size="18" />
                <BookOpen v-else :size="18" />
              </span>
              <div class="class-detail__curriculum-row-main">
                <p class="class-detail__curriculum-row-title">{{ assignmentTitle(assign) }}</p>
                <Badge variant="muted">
                  {{
                    assign.exerciseId !== null
                      ? messages.classes.curriculumItemExercise
                      : messages.classes.curriculumItemLesson
                  }}
                </Badge>
              </div>
              <div class="class-detail__curriculum-row-actions">
                <Button
                  size="icon"
                  variant="secondary"
                  :disabled="i === 0 || curriculumBusy"
                  :aria-label="messages.classes.curriculumMoveUp"
                  @click="moveCurriculumItem(assign, -1)"
                >
                  <ArrowUp :size="15" aria-hidden="true" />
                </Button>
                <Button
                  size="icon"
                  variant="secondary"
                  :disabled="i === classStore.assignments.length - 1 || curriculumBusy"
                  :aria-label="messages.classes.curriculumMoveDown"
                  @click="moveCurriculumItem(assign, 1)"
                >
                  <ArrowDown :size="15" aria-hidden="true" />
                </Button>
                <Button
                  size="sm"
                  variant="danger"
                  :aria-label="messages.classes.curriculumRemove"
                  @click="curriculumRemoveId = assign.id"
                >
                  <Trash2 :size="14" aria-hidden="true" />
                </Button>
              </div>
            </Card>
          </div>
        </template>

        <!-- Student: xem path + trạng thái tiến độ thật -->
        <template v-else>
          <div v-if="classStore.curriculumLoading" class="class-detail__loading" aria-busy="true">
            <Skeleton v-for="i in 4" :key="i" height="52px" />
          </div>
          <template v-else-if="classStore.curriculum">
            <div class="class-detail__curriculum-student-head">
              <h2 class="class-detail__curriculum-student-title">
                {{ classStore.curriculum.title || classStore.currentClass?.name }}
              </h2>
              <p v-if="classStore.curriculum.description" class="class-detail__curriculum-student-desc">
                {{ classStore.curriculum.description }}
              </p>
              <p v-if="!classStore.curriculum.published" class="class-detail__curriculum-draft-msg">
                {{ messages.classes.curriculumStudentDraftMsg }}
              </p>
            </div>
            <div class="class-detail__curriculum-progress">
              <span class="class-detail__curriculum-progress-label">
                {{ messages.classes.curriculumStudentProgress(classStore.curriculum.progressPct) }}
              </span>
              <ProgressBar :value="classStore.curriculum.progressPct" variant="success" />
            </div>
            <EmptyState
              v-if="classStore.curriculum.items.length === 0"
              icon="book"
              :title="messages.classes.curriculumStudentEmptyTitle"
              :description="messages.classes.curriculumStudentEmptyDesc"
            />
            <div v-else class="class-detail__curriculum-list class-detail__curriculum-student-list">
              <Card
                v-for="(item, i) in classStore.curriculum.items"
                :key="item.assignmentId"
                class="class-detail__curriculum-row"
                role="button"
                tabindex="0"
                :aria-label="item.title"
                @click="openCurriculumItem(item)"
                @keydown.enter="openCurriculumItem(item)"
                @keydown.space.prevent="openCurriculumItem(item)"
              >
                <span class="class-detail__curriculum-index" aria-hidden="true">#{{ pad(i + 1) }}</span>
                <span class="class-detail__assign-icon" aria-hidden="true">
                  <Puzzle v-if="item.exerciseId !== null" :size="18" />
                  <BookOpen v-else :size="18" />
                </span>
                <div class="class-detail__curriculum-row-main">
                  <p class="class-detail__curriculum-row-title">{{ cleanTitle(item.title) }}</p>
                  <Badge
                    :variant="
                      item.status === 'completed'
                        ? 'success'
                        : item.status === 'in_progress'
                          ? 'warning'
                          : 'muted'
                    "
                  >
                    {{
                      item.status === 'completed'
                        ? messages.classes.curriculumStatusCompleted
                        : item.status === 'in_progress'
                          ? messages.classes.curriculumStatusInProgress
                          : messages.classes.curriculumStatusNotStarted
                    }}
                  </Badge>
                </div>
                <Badge v-if="item.status === 'completed'" variant="success">
                  <Check :size="13" aria-hidden="true" />
                </Badge>
              </Card>
            </div>
          </template>
        </template>
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
        <div class="space-y-1">
          <div class="flex items-center justify-between">
            <label for="assign-due" class="text-sm font-medium text-foreground">{{ messages.classes.detailAssignDueLabel }}</label>
            <button
              v-if="assignDue"
              type="button"
              class="text-xs text-red-400 hover:text-red-300 hover:underline cursor-pointer flex items-center gap-1"
              @click="assignDue = ''"
            >
              <X :size="12" /> Xóa hạn nộp
            </button>
          </div>
          <Input
            id="assign-due"
            v-model="assignDue"
            type="datetime-local"
          />
        </div>
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

    <!-- B4: Modal sửa hạn nộp và modal xác nhận xóa bài gán đã bị bỏ -->

    <!-- Modal xác nhận xóa item khỏi lộ trình -->
    <Modal
      :open="curriculumRemoveId !== null"
      :title="messages.classes.curriculumRemove"
      @close="curriculumRemoveId = null"
    >
      <p class="class-detail__modal-text">{{ messages.classes.curriculumRemoveConfirm }}</p>
      <template #footer>
        <Button variant="ghost" @click="curriculumRemoveId = null">{{ messages.classes.cancel }}</Button>
        <Button variant="danger" @click="removeCurriculumItem">
          <Trash2 :size="14" aria-hidden="true" /> {{ messages.classes.curriculumRemove }}
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

    <!-- Modal Nhập từ Lộ trình có sẵn -->
    <Modal :open="importCourseModalOpen" title="Nhập Lộ trình học vào Lớp học phần" @close="importCourseModalOpen = false">
      <div class="space-y-4">
        <p class="text-sm text-vdsa-secondary">
          Chọn một Lộ trình học (Course) để tự động nạp toàn bộ danh sách bài học và bài tập của khóa vào lộ trình lớp này.
        </p>
        <div v-if="loadingCourses" class="py-6 text-center text-sm text-vdsa-muted">
          Đang tải danh sách lộ trình...
        </div>
        <div v-else-if="availableCourses.length === 0" class="py-6 text-center text-sm text-vdsa-muted">
          Chưa có lộ trình nào trên hệ thống.
        </div>
        <div v-else class="space-y-2 max-h-60 overflow-y-auto custom-scrollbar">
          <label
            v-for="c in availableCourses"
            :key="c.id"
            class="flex items-start gap-3 p-3 rounded-xl border border-vdsa-border bg-vdsa-surface hover:bg-vdsa-hover cursor-pointer transition-colors"
            :class="{ 'ring-2 ring-vdsa-accent border-vdsa-accent': selectedCourseIdToImport === Number(c.id) }"
          >
            <input
              v-model="selectedCourseIdToImport"
              type="radio"
              name="import-course"
              :value="Number(c.id)"
              class="mt-1 accent-purple-600 cursor-pointer"
            />
            <div class="flex-1">
              <div class="flex items-center gap-2">
                <span class="font-bold text-white text-sm">{{ c.title }}</span>
                <Badge variant="secondary">{{ c.category || 'DSA' }}</Badge>
              </div>
              <p class="text-xs text-vdsa-muted line-clamp-1 mt-0.5">{{ c.description }}</p>
              <span class="text-xs text-vdsa-purple-light font-semibold mt-1 inline-block">
                {{ c.totalLessons || c.lessons?.length || 0 }} bài học · {{ c.xpReward || 0 }} XP
              </span>
            </div>
          </label>
        </div>
      </div>
      <template #footer>
        <Button variant="ghost" @click="importCourseModalOpen = false">{{ messages.classes.cancel }}</Button>
        <Button
          variant="primary"
          :disabled="selectedCourseIdToImport === null || importingCourse"
          @click="handleImportCourse"
        >
          <span v-if="importingCourse" class="inline-block w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin mr-2"></span>
          Xác nhận nạp vào Lớp
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
