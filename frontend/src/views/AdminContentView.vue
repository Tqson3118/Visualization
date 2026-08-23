<script setup lang="ts">
// AdminContentView (Unified Teacher & Admin Studio)
// 3 Tab thống nhất: 1. Lộ trình & Cây bài giảng | 2. Ngân hàng Bài tập & Quiz | 3. Ý kiến học viên
import { computed, onMounted, reactive, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  AlertCircle,
  ArrowRight,
  BookOpen,
  Check,
  CheckCircle2,
  ChevronDown,
  ChevronRight,
  Clock,
  Code,
  Download,
  ExternalLink,
  Eye,
  FileText,
  Filter,
  FlaskConical,
  FolderPlus,
  HelpCircle,
  Inbox,
  Layers,
  ListFilter,
  MessageSquare,
  Network,
  Pencil,
  Plus,
  RefreshCw,
  Search,
  Send,
  Sparkles,
  Trash2,
  Upload,
  User,
} from 'lucide-vue-next';

import * as lessonsApi from '@/api/lessons';
import type { LessonSummary, Topic } from '@/api/lessons';
import * as exercisesApi from '@/api/exercises';
import type { ExerciseSummaryDto } from '@/api/exercises';
import { courseApi, type CourseListDto, type CourseDetailDto, type CourseFeedbackDto } from '@/services/courseApi';
import CourseBuilderModal from '@/components/admin/CourseBuilderModal.vue';
import ExerciseBuilderModal from '@/components/admin/ExerciseBuilderModal.vue';
import { getData } from '@/api/client';
import { useUiStore } from '@/stores/ui';
import { useAuthStore } from '@/stores/auth';
import { formatDate } from '@/utils/format';
import ProseContent from '@/components/ui/ProseContent.vue';
import AdminNav from '@/components/admin/AdminNav.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';

type LessonStatusValue = 'draft' | 'pendingreview' | 'active' | 'hidden';

interface LessonRow extends Omit<LessonSummary, 'status'> {
  status: LessonStatusValue;
  isClassOnly: boolean;
  publishedAt: string | null;
  createdBy: number;
}

const ui = useUiStore();
const router = useRouter();
const route = useRoute();
const auth = useAuthStore();

// ── Tab chính Studio ──
type StudioTab = 'curriculum' | 'exercises' | 'feedback';
const activeTab = ref<StudioTab>('curriculum');

// Đồng bộ tab từ URL Query
watch(
  () => route.query.tab,
  (queryTab) => {
    if (queryTab === 'exercises' || queryTab === 'feedback' || queryTab === 'curriculum') {
      activeTab.value = queryTab as StudioTab;
    }
  },
  { immediate: true },
);

function switchTab(newTab: StudioTab): void {
  activeTab.value = newTab;
  const currentQueryTab = route.query.tab;
  if (currentQueryTab !== newTab) {
    const query: Record<string, any> = { ...route.query, tab: newTab };
    if (selectedCourseId.value && selectedCourseId.value !== 'all') {
      query.courseId = String(selectedCourseId.value);
    }
    void router.replace({ query }).catch(() => {});
  }
  if (newTab === 'feedback') {
    void loadFeedback();
  }
}

// ── Dữ liệu cốt lõi ──
const lessons = ref<LessonRow[]>([]);
const topics = ref<Topic[]>([]);
const courses = ref<CourseListDto[]>([]);
const exercises = ref<ExerciseSummaryDto[]>([]);
const feedbackItems = ref<CourseFeedbackDto[]>([]);
const loading = ref(true);
const loadError = ref(false);

// ── TAB 1: LỘ TRÌNH & CÂY BÀI GIẢNG ──
const selectedCourseId = ref<string | number | 'all'>(
  typeof route.query.courseId === 'string' ? route.query.courseId : 'all',
);
const courseDetailMap = reactive<Record<string, CourseDetailDto>>({});
const loadingCourseDetail = ref(false);

// Đồng bộ courseId từ URL Query khi URL thay đổi từ ngoài
watch(
  () => route.query.courseId,
  (queryCourseId) => {
    if (queryCourseId && queryCourseId !== selectedCourseId.value) {
      selectedCourseId.value = queryCourseId as string;
    }
  },
);

const viewMode = ref<'curriculum' | 'flat'>('curriculum');
const lessonSearchQuery = ref('');
const openTopicIds = reactive<Record<number, boolean>>({});

function toggleTopic(topicId: number): void {
  openTopicIds[topicId] = !isTopicOpen(topicId);
}

function isTopicOpen(topicId: number): boolean {
  return openTopicIds[topicId] !== false;
}

// Lộ trình hiện tại
const currentCourse = computed<CourseListDto | null>(() => {
  if (selectedCourseId.value === 'all') return null;
  return courses.value.find((c) => String(c.id) === String(selectedCourseId.value)) ?? null;
});

// Danh sách bài học của Lộ trình đang chọn
const currentCourseDetail = computed<CourseDetailDto | null>(() => {
  if (selectedCourseId.value === 'all') return null;
  return courseDetailMap[String(selectedCourseId.value)] ?? null;
});

// Lọc bài học thuộc Lộ trình đang chọn
const displayLessons = computed<LessonRow[]>(() => {
  let list = lessons.value;
  if (selectedCourseId.value !== 'all' && currentCourseDetail.value?.lessons) {
    const validLessonIds = new Set(
      (currentCourseDetail.value.lessons || []).map((l) => String(l.id)),
    );
    const validTitles = new Set(
      (currentCourseDetail.value.lessons || []).map((l) => (l.title || '').trim().toLowerCase()),
    );
    list = list.filter((l) => validLessonIds.has(String(l.id)) || (l.title && validTitles.has(l.title.trim().toLowerCase())));
  }
  if (lessonSearchQuery.value.trim()) {
    const q = lessonSearchQuery.value.trim().toLowerCase();
    list = list.filter(
      (l) => (l.title || '').toLowerCase().includes(q) || (l.description && l.description.toLowerCase().includes(q)),
    );
  }
  return list;
});

// Lọc các Chương / Chủ đề có chứa bài học của Lộ trình đang chọn
const displayTopics = computed<Topic[]>(() => {
  if (selectedCourseId.value === 'all') return topics.value;
  const currentTopicIds = new Set(displayLessons.value.map((l) => l.topicId));
  const matchedTopics = topics.value.filter((t) => currentTopicIds.has(t.id));
  return matchedTopics.length > 0 ? matchedTopics : topics.value;
});

function getLessonsByTopic(topicId: number): LessonRow[] {
  return displayLessons.value.filter((l) => l.topicId === topicId);
}

// Theo dõi khi đổi Lộ trình để tải chi tiết lộ trình đó và cập nhật query URL
watch(
  selectedCourseId,
  async (newId) => {
    if (!newId) return;

    const currentQueryId = route.query.courseId;
    const targetQueryId = newId !== 'all' ? String(newId) : undefined;
    if (currentQueryId !== targetQueryId) {
      const query: Record<string, any> = { ...route.query };
      if (targetQueryId) {
        query.courseId = targetQueryId;
      } else {
        delete query.courseId;
      }
      void router.replace({ query }).catch(() => {});
    }

    if (newId !== 'all') {
      const key = String(newId);
      if (!courseDetailMap[key]) {
        loadingCourseDetail.value = true;
        try {
          const detail = await courseApi.getCourseById(newId);
          courseDetailMap[key] = detail;
        } catch {
          // ignore
        } finally {
          loadingCourseDetail.value = false;
        }
      }
    }
  },
  { immediate: true },
);

// ── TAB 2: NGÂN HÀNG BÀI TẬP & QUIZ / CODE LAB ──
const exerciseFilter = ref<'all' | 'quiz' | 'code'>('all');
const exerciseSearchQuery = ref('');

const filteredExercises = computed(() => {
  let list = exercises.value;
  if (selectedCourseId.value !== 'all' && currentCourseDetail.value?.lessons) {
    const courseLessonIds = new Set(
      (currentCourseDetail.value.lessons || []).map((l) => Number(l.id)),
    );
    list = list.filter((e) => e.lessonId !== null && courseLessonIds.has(e.lessonId));
  }
  if (exerciseFilter.value === 'quiz') {
    list = list.filter((e) => e.type === 'MCQ' || e.stage === 1);
  } else if (exerciseFilter.value === 'code') {
    list = list.filter((e) => e.type === 'CODE' || e.stage === 3);
  }
  if (exerciseSearchQuery.value.trim()) {
    const q = exerciseSearchQuery.value.trim().toLowerCase();
    list = list.filter(
      (e) => (e.title || '').toLowerCase().includes(q) || (e.description && e.description.toLowerCase().includes(q)),
    );
  }
  return list;
});

const lessonMap = computed(() => {
  const map = new Map<number, string>();
  for (const l of lessons.value) {
    map.set(l.id, l.title);
  }
  return map;
});

// ── TAB 3: Ý KIẾN & ĐÁNH GIÁ HỌC VIÊN ──
const feedbackStatusFilter = ref<string>('');
const feedbackSearchQuery = ref('');
const replyTexts = ref<Record<number, string>>({});
const replySaving = ref<Record<number, boolean>>({});

async function loadFeedback(): Promise<void> {
  try {
    const cId = selectedCourseId.value !== 'all' ? Number(selectedCourseId.value) : undefined;
    feedbackItems.value = await courseApi.getTeacherFeedback({
      courseId: cId,
      status: feedbackStatusFilter.value || undefined,
    });
  } catch {
    // ignore
  }
}

const filteredFeedbacks = computed(() => {
  let list = feedbackItems.value;
  if (selectedCourseId.value !== 'all') {
    list = list.filter((i) => i.courseId === Number(selectedCourseId.value));
  }
  if (feedbackStatusFilter.value) {
    list = list.filter((i) => i.status === feedbackStatusFilter.value);
  }
  if (feedbackSearchQuery.value.trim()) {
    const q = feedbackSearchQuery.value.toLowerCase().trim();
    list = list.filter(
      (i) => i.content.toLowerCase().includes(q) || (i.userName && i.userName.toLowerCase().includes(q)),
    );
  }
  return list;
});

// ── Modals State ──
const courseModalOpen = ref(false);
const editingCourseId = ref<string | number | null>(null);

const topicFormOpen = ref(false);
const editingTopicId = ref<number | null>(null);
const topicForm = reactive({ name: '', description: '', sortOrder: 0 });

const exerciseModalOpen = ref(false);
const editingExerciseId = ref<number | null>(null);
const builderDefaultLessonId = ref<number | null>(null);
const builderDefaultStage = ref<number | null>(null);
const builderDefaultTab = ref<'quiz' | 'code' | 'import-csv' | null>(null);

const previewOpen = ref(false);
const previewTitle = ref('');
const previewContent = ref('');

const isAdmin = computed(() => auth.role === 'ADMIN');

onMounted(load);

async function load(): Promise<void> {
  loading.value = true;
  loadError.value = false;
  try {
    const [lessonPage, topicTree, courseList, exerciseList, feedbackList] = await Promise.all([
      lessonsApi.fetchLessons({ page: 1, pageSize: 200 }),
      lessonsApi.fetchTopics().catch(() => [] as Topic[]),
      courseApi.getCourses().catch(() => [] as CourseListDto[]),
      exercisesApi.fetchExercises({}).catch(() => [] as ExerciseSummaryDto[]),
      courseApi.getTeacherFeedback().catch(() => [] as CourseFeedbackDto[]),
    ]);
    lessons.value = lessonPage.items.map(toRow);
    topics.value = topicTree;
    courses.value = courseList;
    exercises.value = exerciseList;
    feedbackItems.value = feedbackList;

    if (route.query.courseId) {
      selectedCourseId.value = route.query.courseId as string;
    } else if (courseList.length > 0 && selectedCourseId.value === 'all') {
      selectedCourseId.value = courseList[0].id;
    }

    if (selectedCourseId.value && selectedCourseId.value !== 'all') {
      const key = String(selectedCourseId.value);
      if (!courseDetailMap[key]) {
        try {
          courseDetailMap[key] = await courseApi.getCourseById(selectedCourseId.value);
        } catch {
          // ignore
        }
      }
    }
  } catch {
    loadError.value = true;
  } finally {
    loading.value = false;
  }
}

function toRow(item: LessonSummary): LessonRow {
  const extra = item as { isClassOnly?: boolean; publishedAt?: string | null; createdBy?: number };
  return {
    id: item.id,
    title: item.title,
    description: item.description,
    topicId: item.topicId,
    sortOrder: item.sortOrder,
    status: item.status as LessonStatusValue,
    isClassOnly: extra.isClassOnly ?? false,
    publishedAt: extra.publishedAt ?? null,
    createdBy: extra.createdBy ?? 0,
    simulationCount: item.simulationCount,
    exerciseCount: item.exerciseCount,
    progress: item.progress,
  };
}

const topicName = computed(() => (id: number) => topics.value.find((t) => t.id === id)?.name ?? `#${id}`);

// ── Thao tác Lộ trình ──
function openCreateCourse(): void {
  editingCourseId.value = null;
  courseModalOpen.value = true;
}

function openEditCourse(course?: CourseListDto | null): void {
  if (!course) return;
  editingCourseId.value = course.id;
  courseModalOpen.value = true;
}

async function deleteCourse(course?: CourseListDto | null): Promise<void> {
  if (!course) return;
  if (!confirm(`Bạn có chắc muốn xóa/ẩn lộ trình "${course.title}"?`)) return;
  try {
    await courseApi.deleteCourse(course.id);
    ui.showToast('Đã xóa lộ trình thành công.', 'success');
    selectedCourseId.value = 'all';
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể xóa lộ trình.', 'error');
  }
}

function onCourseSaved(course: CourseDetailDto): void {
  selectedCourseId.value = course.id;
  courseDetailMap[String(course.id)] = course;
  void load();
}

// ── Thao tác Topic / Chương ──
function openCreateTopic(): void {
  editingTopicId.value = null;
  topicForm.name = '';
  topicForm.description = '';
  topicForm.sortOrder = (topics.value.length + 1) * 10;
  topicFormOpen.value = true;
}

function openEditTopic(topic: Topic): void {
  editingTopicId.value = topic.id;
  topicForm.name = topic.name;
  topicForm.description = topic.description || '';
  topicForm.sortOrder = topic.sortOrder;
  topicFormOpen.value = true;
}

async function saveTopic(): Promise<void> {
  if (!topicForm.name.trim()) {
    ui.showToast('Vui lòng nhập tên chương / chủ đề.', 'warning');
    return;
  }
  try {
    if (editingTopicId.value === null) {
      await lessonsApi.createTopic(topicForm);
      ui.showToast('Đã tạo chương mới thành công.', 'success');
    } else {
      await lessonsApi.updateTopic(editingTopicId.value, topicForm);
      ui.showToast('Đã cập nhật thông tin chương.', 'success');
    }
    topicFormOpen.value = false;
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Lỗi khi lưu chương.', 'error');
  }
}

async function deleteTopic(topic: Topic): Promise<void> {
  const count = displayLessons.value.filter((l) => l.topicId === topic.id).length;
  if (count > 0) {
    ui.showToast(`Không thể xóa chương "${topic.name}" vì vẫn còn ${count} bài học bên trong.`, 'warning');
    return;
  }
  if (!confirm(`Bạn có chắc muốn xóa chương "${topic.name}"?`)) return;
  try {
    await lessonsApi.deleteTopic(topic.id);
    ui.showToast('Đã xóa chương thành công.', 'success');
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Xóa chương thất bại.', 'error');
  }
}

// ── Thao tác Lesson ──
function openCreateLesson(topicId?: number): void {
  const query: Record<string, string> = {};
  if (topicId) query.topicId = String(topicId);
  if (selectedCourseId.value && selectedCourseId.value !== 'all') {
    query.courseId = String(selectedCourseId.value);
  }
  void router.push({ path: '/studio/lessons/new', query });
}

function canManageLesson(lesson: LessonRow | LessonSummary): boolean {
  if (auth.role === 'ADMIN') return true;
  return lesson.createdBy === auth.user?.id;
}

function openEditLesson(lesson: LessonRow): void {
  if (!canManageLesson(lesson)) {
    ui.showToast('Bạn chỉ có thể chỉnh sửa bài học do chính mình tạo. Hãy dùng "Xem trước" để tham khảo nội dung.', 'warning');
    return;
  }
  void router.push(`/studio/lessons/${lesson.id}/edit`);
}

async function deleteLesson(lesson: LessonRow): Promise<void> {
  if (!canManageLesson(lesson)) {
    ui.showToast('Bạn không có quyền xóa bài học của người khác.', 'error');
    return;
  }
  if (!confirm(`Xóa bài học "${lesson.title}"?`)) return;
  try {
    await lessonsApi.deleteLesson(lesson.id);
    ui.showToast('Đã xóa bài học thành công.', 'success');
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Xóa thất bại.', 'error');
  }
}

function openLessonPreview(lesson: LessonSummary): void {
  previewTitle.value = lesson.title;
  previewContent.value = 'Đang tải nội dung...';
  previewOpen.value = true;
  lessonsApi
    .fetchLesson(lesson.id)
    .then((detail) => {
      previewContent.value = detail.contentHtml ?? '<p>Chưa có nội dung bài học.</p>';
    })
    .catch(() => {
      ui.showToast('Không tải được nội dung bài học.', 'error');
      previewOpen.value = false;
    });
}

// ── Thao tác Exercise / Quiz ──
function openCreateQuizForLesson(lessonId?: number): void {
  editingExerciseId.value = null;
  builderDefaultLessonId.value = lessonId ?? null;
  builderDefaultStage.value = 1;
  builderDefaultTab.value = 'quiz';
  exerciseModalOpen.value = true;
}

function openCreateCodeForLesson(lessonId?: number): void {
  editingExerciseId.value = null;
  builderDefaultLessonId.value = lessonId ?? null;
  builderDefaultStage.value = 3;
  builderDefaultTab.value = 'code';
  exerciseModalOpen.value = true;
}

function openImportCsv(lessonId?: number): void {
  editingExerciseId.value = null;
  builderDefaultLessonId.value = lessonId ?? null;
  builderDefaultStage.value = 1;
  builderDefaultTab.value = 'import-csv';
  exerciseModalOpen.value = true;
}

function openEditExercise(ex: ExerciseSummaryDto): void {
  editingExerciseId.value = ex.id;
  builderDefaultLessonId.value = ex.lessonId;
  builderDefaultStage.value = ex.stage || (ex.type === 'CODE' ? 3 : 1);
  builderDefaultTab.value = ex.type === 'CODE' ? 'code' : 'quiz';
  exerciseModalOpen.value = true;
}

async function deleteExercise(ex: ExerciseSummaryDto): Promise<void> {
  if (!confirm(`Bạn có chắc muốn xóa bài tập "${ex.title}"?`)) return;
  try {
    await exercisesApi.deleteExercise(ex.id);
    ui.showToast('Đã xóa bài tập thành công.', 'success');
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Xóa bài tập thất bại.', 'error');
  }
}

function downloadSampleCsv(): void {
  const csvContent = `question,option_a,option_b,option_c,option_d,correct_option,explanation
"Độ phức tạp thời gian tốt nhất của Bubble Sort là gì?","O(N)","O(N^2)","O(log N)","O(1)","A","Khi mảng đã sắp xếp và có cờ swapped, Bubble Sort dừng sau 1 lượt O(N)."
"Thuật toán sắp xếp nào sau đây KHÔNG có tính ổn định (Not Stable)?","Selection Sort","Merge Sort","Bubble Sort","Insertion Sort","A","Selection Sort có thể hoán đổi các phần tử bằng nhau qua khoảng cách xa."
"Ngăn xếp (Stack) hoạt động theo nguyên lý nào?","LIFO","FIFO","LILO","FILO","A","Stack hoạt động theo cơ chế Last-In-First-Out."`;

  const blob = new Blob(['\uFEFF' + csvContent], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.setAttribute('href', url);
  link.setAttribute('download', 'mau_cau_hoi_quiz_dsa.csv');
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
  ui.showToast('Đã tải xuống file mẫu CSV (mau_cau_hoi_quiz_dsa.csv)!', 'success');
}

// ── Thao tác Feedback ──
async function sendFeedbackReply(item: CourseFeedbackDto): Promise<void> {
  const text = (replyTexts.value[item.id] ?? '').trim();
  if (!text) {
    ui.showToast('Vui lòng nhập nội dung trả lời.', 'warning');
    return;
  }
  replySaving.value[item.id] = true;
  try {
    await courseApi.replyCourseFeedback(item.id, {
      replyText: text,
      status: 'Resolved',
    });
    item.replyText = text;
    item.status = 'Resolved';
    replyTexts.value[item.id] = '';
    ui.showToast('Đã gửi phản hồi tới học viên thành công!', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Gửi phản hồi thất bại.', 'error');
  } finally {
    replySaving.value[item.id] = false;
  }
}

// ── Thao tác Admin Duyệt bài ──
async function approveLesson(lesson: LessonRow): Promise<void> {
  if (!confirm(`Duyệt bài học "${lesson.title}"?`)) return;
  try {
    await getData({ method: 'POST', url: `/lessons/${lesson.id}/review`, data: { approve: true } });
    ui.showToast('Đã duyệt bài học thành công.', 'success');
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Duyệt thất bại.', 'error');
  }
}

async function rejectLesson(lesson: LessonRow): Promise<void> {
  const reason = prompt(`Lý do từ chối bài học "${lesson.title}":`);
  if (!reason || !reason.trim()) return;
  try {
    await getData({ method: 'POST', url: `/lessons/${lesson.id}/review`, data: { approve: false, reason: reason.trim() } });
    ui.showToast('Đã từ chối bài học.', 'success');
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Từ chối thất bại.', 'error');
  }
}

const statusLabel: Record<string, string> = {
  draft: 'Bản nháp',
  pendingreview: 'Chờ duyệt',
  active: 'Đã duyệt',
  hidden: 'Ẩn',
};

const statusVariant: Record<LessonStatusValue, 'success' | 'warning' | 'muted'> = {
  active: 'success',
  pendingreview: 'warning',
  draft: 'muted',
  hidden: 'muted',
};

const pad = (n: number): string => String(n).padStart(2, '0');
</script>

<template>
  <main class="studio-view container">
    <!-- ═══ HEADER STUDIO BANNER ═══ -->
    <header class="studio-hero">
      <div class="studio-hero__info">
        <p class="studio-hero__kicker">
          <Sparkles :size="14" class="inline mr-1 text-primary-400" />
          {{ isAdmin ? 'ADMIN CONSOLE & STUDIO' : 'TEACHER STUDIO' }}
        </p>
        <h1 class="studio-hero__title">Studio Giảng viên & Nội dung</h1>
        <p class="studio-hero__desc">
          Biên soạn Lộ trình học, tổ chức cây Chương - Bài giảng, tạo Ngân hàng Bài tập & Quiz và tương tác với Học viên.
        </p>
      </div>

      <div class="studio-hero__stats">
        <div class="stat-pill">
          <span class="stat-pill__num">{{ courses.length }}</span>
          <span class="stat-pill__label">Lộ trình</span>
        </div>
        <div class="stat-pill">
          <span class="stat-pill__num">{{ lessons.length }}</span>
          <span class="stat-pill__label">Bài học</span>
        </div>
        <div class="stat-pill">
          <span class="stat-pill__num">{{ exercises.length }}</span>
          <span class="stat-pill__label">Bài tập</span>
        </div>
      </div>
    </header>

    <!-- Thanh điều hướng Admin / Teacher -->
    <AdminNav active="content" />

    <!-- ═══ THANH CHUYỂN 3 TAB CHÍNH CỦA STUDIO ═══ -->
    <nav class="studio-main-tabs" aria-label="Các khu vực của Studio">
      <button
        type="button"
        class="studio-main-tab"
        :class="{ 'studio-main-tab--active': activeTab === 'curriculum' }"
        @click="switchTab('curriculum')"
      >
        <Layers :size="16" />
        <span>1. Lộ trình & Cây bài giảng</span>
        <Badge variant="secondary" class="text-xs">{{ displayLessons.length }}</Badge>
      </button>

      <button
        type="button"
        class="studio-main-tab"
        :class="{ 'studio-main-tab--active': activeTab === 'exercises' }"
        @click="switchTab('exercises')"
      >
        <HelpCircle :size="16" />
        <span>2. Ngân hàng Bài tập & Quiz</span>
        <Badge variant="secondary" class="text-xs">{{ exercises.length }}</Badge>
      </button>

      <button
        type="button"
        class="studio-main-tab"
        :class="{ 'studio-main-tab--active': activeTab === 'feedback' }"
        @click="switchTab('feedback')"
      >
        <MessageSquare :size="16" />
        <span>3. Ý kiến & Đánh giá Học viên</span>
        <Badge variant="secondary" class="text-xs">{{ feedbackItems.length }}</Badge>
      </button>
    </nav>

    <div v-if="loading" class="studio-loading" aria-busy="true">
      <Skeleton v-for="i in 4" :key="i" height="70px" />
    </div>

    <div v-else-if="loadError" class="studio-error" role="alert">
      <p>Không thể tải dữ liệu Studio (máy chủ backend chưa phản hồi).</p>
      <Button size="sm" variant="secondary" @click="load">
        <RefreshCw :size="14" /> Thử lại
      </Button>
    </div>

    <div v-else class="studio-workspace">
      <!-- ══════════════════════════════════════════════════════════════════
           TAB 1: LỘ TRÌNH & CÂY BÀI GIẢNG (CURRICULUM STUDIO)
           ══════════════════════════════════════════════════════════════════ -->
      <section v-if="activeTab === 'curriculum'" class="space-y-4">
        <!-- 1. Thanh chọn Lộ trình học (Course Selector Bar) -->
        <div class="course-bar">
          <div class="course-bar__selector-group">
            <label class="course-bar__label">Lộ trình đang chọn:</label>
            <div class="flex items-center gap-2 flex-wrap">
              <select v-model="selectedCourseId" class="course-bar__select">
                <option value="all">🌐 Tất cả Lộ trình & Bài học (Tổng hợp)</option>
                <option v-for="course in courses" :key="course.id" :value="course.id">
                  📖 {{ course.title }} ({{ course.category || 'DSA' }})
                </option>
              </select>

              <Button size="sm" variant="secondary" @click="openCreateCourse">
                <Plus :size="14" /> Tạo Lộ trình mới
              </Button>
            </div>
          </div>

          <div v-if="currentCourse" class="course-bar__actions">
            <Button size="sm" variant="ghost" @click="openEditCourse(currentCourse)">
              <Pencil :size="14" /> Sửa thông tin
            </Button>
            <Button size="sm" variant="ghost" @click="router.push(`/path/${currentCourse.id}`)">
              <ExternalLink :size="14" /> Xem trang học viên
            </Button>
            <Button size="sm" variant="danger" @click="deleteCourse(currentCourse)">
              <Trash2 :size="14" />
            </Button>
          </div>
        </div>

        <!-- 2. Toolbar & Switcher -->
        <div class="studio-toolbar">
          <div class="studio-toolbar__left">
            <div class="view-switch">
              <button
                type="button"
                class="view-switch__btn"
                :class="{ 'view-switch__btn--active': viewMode === 'curriculum' }"
                @click="viewMode = 'curriculum'"
              >
                <Layers :size="15" /> Cây Lộ trình phân cấp
              </button>
              <button
                type="button"
                class="view-switch__btn"
                :class="{ 'view-switch__btn--active': viewMode === 'flat' }"
                @click="viewMode = 'flat'"
              >
                <ListFilter :size="15" /> Danh sách phẳng ({{ displayLessons.length }})
              </button>
            </div>
          </div>

          <div class="studio-toolbar__right">
            <div class="search-box">
              <Search :size="14" class="search-box__icon" />
              <input
                v-model="lessonSearchQuery"
                type="text"
                class="search-box__input"
                placeholder="Tìm bài học theo tên..."
              />
            </div>

            <Button size="md" @click="openCreateLesson()">
              <Plus :size="16" /> Soạn bài học mới
            </Button>
          </div>
        </div>

        <!-- 3. Cây Lộ trình phân cấp -->
        <div v-if="viewMode === 'curriculum'" class="curriculum-tree">
          <EmptyState
            v-if="displayTopics.length === 0 || displayLessons.length === 0"
            icon="book"
            title="Lộ trình này chưa có bài học nào"
            description="Hãy bắt đầu tạo chương và soạn bài học đầu tiên cho lộ trình này."
            action-label="+ Thêm Chương đầu tiên"
            @action="openCreateTopic"
          />

          <div v-else class="curriculum-tree__list">
            <article
              v-for="(topic, tIdx) in displayTopics"
              :key="topic.id"
              class="chapter-card"
            >
              <!-- Header Chương -->
              <header class="chapter-card__header" @click="toggleTopic(topic.id)">
                <div class="chapter-card__header-left">
                  <button type="button" class="chapter-card__toggle-btn">
                    <ChevronDown v-if="isTopicOpen(topic.id)" :size="18" />
                    <ChevronRight v-else :size="18" />
                  </button>

                  <div class="chapter-card__badge-index">Chương {{ pad(tIdx + 1) }}</div>

                  <div class="chapter-card__titles">
                    <h2 class="chapter-card__name">{{ topic.name }}</h2>
                    <p v-if="topic.description" class="chapter-card__desc">{{ topic.description }}</p>
                  </div>
                </div>

                <div class="chapter-card__header-right" @click.stop>
                  <Badge variant="secondary" class="chapter-card__count">
                    {{ getLessonsByTopic(topic.id).length }} bài học
                  </Badge>

                  <Button size="sm" variant="secondary" @click="openCreateLesson(topic.id)">
                    <Plus :size="14" /> Thêm bài vào chương
                  </Button>

                  <Button size="sm" variant="ghost" title="Sửa tên chương" @click="openEditTopic(topic)">
                    <Pencil :size="14" />
                  </Button>

                  <Button size="sm" variant="ghost" title="Xóa chương" @click="deleteTopic(topic)">
                    <Trash2 :size="14" class="text-rose-400" />
                  </Button>
                </div>
              </header>

              <!-- Body Chương -->
              <div v-show="isTopicOpen(topic.id)" class="chapter-card__body">
                <div v-if="getLessonsByTopic(topic.id).length === 0" class="chapter-card__empty">
                  <p>Chương này chưa có bài học nào.</p>
                  <Button size="sm" variant="secondary" @click="openCreateLesson(topic.id)">
                    <Plus :size="14" /> Soạn bài học đầu tiên
                  </Button>
                </div>

                <div v-else class="chapter-card__lessons">
                  <div
                    v-for="(lesson, lIdx) in getLessonsByTopic(topic.id)"
                    :key="lesson.id"
                    class="lesson-row"
                  >
                    <div class="lesson-row__main">
                      <span class="lesson-row__index">#{{ tIdx + 1 }}.{{ lIdx + 1 }}</span>
                      <div class="lesson-row__text">
                        <div class="flex items-center gap-2 flex-wrap">
                          <span class="lesson-row__title">{{ lesson.title }}</span>
                          <Badge :variant="statusVariant[lesson.status]" class="text-[11px] py-0">
                            {{ statusLabel[lesson.status] || lesson.status }}
                          </Badge>
                          <Badge v-if="lesson.isClassOnly" variant="secondary" class="text-[11px] py-0">
                            Lớp riêng
                          </Badge>
                        </div>
                        <p v-if="lesson.description" class="lesson-row__desc">{{ lesson.description }}</p>
                      </div>
                    </div>

                    <!-- Badges đính kèm -->
                    <div class="lesson-row__tags">
                      <span
                        class="lesson-row__tag"
                        :class="{ 'lesson-row__tag--active': lesson.simulationCount > 0 }"
                      >
                        <Network :size="13" />
                        {{ lesson.simulationCount > 0 ? `${lesson.simulationCount} mô phỏng` : 'Chưa gắn sim' }}
                      </span>

                      <span
                        class="lesson-row__tag"
                        :class="{ 'lesson-row__tag--active': lesson.exerciseCount > 0 }"
                      >
                        <CheckCircle2 :size="13" />
                        {{ lesson.exerciseCount > 0 ? `${lesson.exerciseCount} bài tập` : 'Chưa có quiz' }}
                      </span>
                    </div>

                    <!-- Actions -->
                    <div class="lesson-row__actions">
                      <template v-if="isAdmin && lesson.status === 'pendingreview'">
                        <Button size="sm" variant="secondary" @click="approveLesson(lesson)">Duyệt</Button>
                        <Button size="sm" variant="danger" @click="rejectLesson(lesson)">Từ chối</Button>
                      </template>

                      <Button size="sm" variant="ghost" @click="openLessonPreview(lesson)">
                        <Eye :size="15" /> Xem trước
                      </Button>

                      <Button v-if="canManageLesson(lesson)" size="sm" variant="ghost" @click="openEditLesson(lesson)">
                        <Pencil :size="15" /> Soạn / Sửa
                      </Button>

                      <Button v-if="canManageLesson(lesson)" size="sm" variant="ghost" @click="openCreateQuizForLesson(lesson.id)">
                        <Plus :size="15" /> Thêm Quiz / Lab
                      </Button>

                      <Button v-if="canManageLesson(lesson)" size="sm" variant="danger" @click="deleteLesson(lesson)">
                        <Trash2 :size="15" />
                      </Button>
                    </div>
                  </div>
                </div>
              </div>
            </article>

            <!-- Nút Thêm Chương Mới -->
            <button type="button" class="add-chapter-btn" @click="openCreateTopic">
              <FolderPlus :size="20" />
              <span>+ Thêm Chương / Chủ đề mới vào Lộ trình</span>
            </button>
          </div>
        </div>

        <!-- 4. Danh sách phẳng -->
        <div v-else class="flat-table-wrap">
          <div class="flat-table">
            <table>
              <thead>
                <tr>
                  <th scope="col">#</th>
                  <th scope="col">Tiêu đề bài học</th>
                  <th scope="col">Chương / Chủ đề</th>
                  <th scope="col">Trạng thái</th>
                  <th scope="col">Mô phỏng</th>
                  <th scope="col">Quiz / Lab</th>
                  <th scope="col" class="text-right">Hành động</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(lesson, idx) in displayLessons" :key="lesson.id">
                  <td class="font-mono text-slate-400">{{ pad(idx + 1) }}</td>
                  <td>
                    <div class="font-semibold text-slate-100">{{ lesson.title }}</div>
                    <div v-if="lesson.description" class="text-xs text-slate-400 truncate max-w-md">
                      {{ lesson.description }}
                    </div>
                  </td>
                  <td>
                    <Badge variant="secondary">{{ topicName(lesson.topicId) }}</Badge>
                  </td>
                  <td>
                    <Badge :variant="statusVariant[lesson.status]">
                      {{ statusLabel[lesson.status] || lesson.status }}
                    </Badge>
                  </td>
                  <td>
                    <span class="inline-flex items-center gap-1 text-xs text-slate-300">
                      <Network :size="13" /> {{ lesson.simulationCount }}
                    </span>
                  </td>
                  <td>
                    <span class="inline-flex items-center gap-1 text-xs text-slate-300">
                      <CheckCircle2 :size="13" /> {{ lesson.exerciseCount }}
                    </span>
                  </td>
                  <td class="text-right">
                    <div class="flex items-center justify-end gap-1.5">
                      <Button size="sm" variant="ghost" @click="openLessonPreview(lesson)"><Eye :size="15" /></Button>
                      <Button v-if="canManageLesson(lesson)" size="sm" variant="ghost" @click="openEditLesson(lesson)"><Pencil :size="15" /></Button>
                      <Button v-if="canManageLesson(lesson)" size="sm" variant="ghost" @click="openCreateQuizForLesson(lesson.id)"><Plus :size="15" /></Button>
                      <Button v-if="canManageLesson(lesson)" size="sm" variant="danger" @click="deleteLesson(lesson)"><Trash2 :size="15" /></Button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </section>

      <!-- ══════════════════════════════════════════════════════════════════
           TAB 2: NGÂN HÀNG BÀI TẬP & QUIZ / CODE LAB (EXERCISES BANK)
           ══════════════════════════════════════════════════════════════════ -->
      <section v-else-if="activeTab === 'exercises'" class="space-y-4">
        <!-- Course Selector Bar trong Tab 2 -->
        <div class="course-bar">
          <div class="course-bar__selector-group">
            <label class="course-bar__label">Lộ trình học:</label>
            <select v-model="selectedCourseId" class="course-bar__select">
              <option value="all">🌐 Tất cả Lộ trình & Bài tập (Tổng hợp)</option>
              <option v-for="course in courses" :key="course.id" :value="course.id">
                📖 {{ course.title }} ({{ course.category || 'DSA' }})
              </option>
            </select>
          </div>
          <div v-if="currentCourse" class="text-xs text-slate-400">
            Đang lọc bài tập của: <strong class="text-slate-200">{{ currentCourse.title }}</strong>
          </div>
        </div>

        <!-- Toolbar Bài tập -->
        <div class="flex items-center justify-between gap-4 flex-wrap">
          <div class="flex items-center gap-2">
            <div class="view-switch">
              <button
                type="button"
                class="view-switch__btn"
                :class="{ 'view-switch__btn--active': exerciseFilter === 'all' }"
                @click="exerciseFilter = 'all'"
              >
                Tất cả ({{ filteredExercises.length }})
              </button>
              <button
                type="button"
                class="view-switch__btn"
                :class="{ 'view-switch__btn--active': exerciseFilter === 'quiz' }"
                @click="exerciseFilter = 'quiz'"
              >
                <HelpCircle :size="14" /> Trắc nghiệm MCQ
              </button>
              <button
                type="button"
                class="view-switch__btn"
                :class="{ 'view-switch__btn--active': exerciseFilter === 'code' }"
                @click="exerciseFilter = 'code'"
              >
                <Code :size="14" /> Code Lab
              </button>
            </div>

            <div class="search-box">
              <Search :size="14" class="search-box__icon" />
              <input
                v-model="exerciseSearchQuery"
                type="text"
                class="search-box__input"
                placeholder="Tìm bài tập theo tiêu đề..."
              />
            </div>
          </div>

          <div class="flex items-center gap-2">
            <Button size="sm" variant="ghost" @click="downloadSampleCsv">
              <Download :size="14" /> File mẫu CSV
            </Button>
            <Button size="sm" variant="secondary" @click="openImportCsv()">
              <Upload :size="14" /> Nhập từ CSV
            </Button>
            <Button size="md" @click="openCreateQuizForLesson()">
              <Plus :size="16" /> Tạo bài tập mới
            </Button>
          </div>
        </div>

        <EmptyState
          v-if="filteredExercises.length === 0"
          icon="book"
          title="Chưa có bài tập nào"
          description="Soạn câu hỏi trắc nghiệm hoặc bài code chấm testcase tự động cho học viên."
          action-label="+ Tạo bài tập mới"
          @action="openCreateQuizForLesson()"
        />

        <!-- Danh sách bài tập -->
        <div v-else class="flat-table-wrap">
          <div class="flat-table">
            <table>
              <thead>
                <tr>
                  <th scope="col">#</th>
                  <th scope="col">Tiêu đề bài tập</th>
                  <th scope="col">Loại</th>
                  <th scope="col">Gắn vào bài học</th>
                  <th scope="col">Số câu / Testcase</th>
                  <th scope="col">Thời lượng</th>
                  <th scope="col" class="text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody>
                <tr v-for="(ex, idx) in filteredExercises" :key="ex.id">
                  <td class="font-mono text-slate-400">{{ pad(idx + 1) }}</td>
                  <td>
                    <div class="font-semibold text-slate-100">{{ ex.title }}</div>
                    <div v-if="ex.description" class="text-xs text-slate-400 truncate max-w-md">
                      {{ ex.description }}
                    </div>
                  </td>
                  <td>
                    <Badge :variant="ex.type === 'CODE' ? 'primary' : 'secondary'">
                      {{ ex.type === 'CODE' ? '💻 Code Lab' : '❓ Trắc nghiệm MCQ' }}
                    </Badge>
                  </td>
                  <td>
                    <span v-if="ex.lessonId" class="text-xs text-slate-300">
                      📖 {{ lessonMap.get(ex.lessonId) || `#${ex.lessonId}` }}
                    </span>
                    <span v-else class="text-xs text-slate-500 italic">Chưa gắn</span>
                  </td>
                  <td class="font-mono text-xs text-slate-300">
                    {{ ex.type === 'CODE' ? '1 bài thực hành' : 'Trắc nghiệm' }}
                  </td>
                  <td class="font-mono text-xs text-slate-400">
                    {{ ex.durationMinutes }} phút ({{ ex.maxScore }} điểm)
                  </td>
                  <td class="text-right">
                    <div class="flex items-center justify-end gap-1.5">
                      <Button size="sm" variant="ghost" @click="openEditExercise(ex)">
                        <Pencil :size="15" /> Sửa
                      </Button>
                      <Button size="sm" variant="danger" @click="deleteExercise(ex)">
                        <Trash2 :size="15" />
                      </Button>
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </section>

      <!-- ══════════════════════════════════════════════════════════════════
           TAB 3: Ý KIẾN & ĐÁNH GIÁ HỌC VIÊN (FEEDBACK HUB)
           ══════════════════════════════════════════════════════════════════ -->
      <section v-else-if="activeTab === 'feedback'" class="space-y-4">
        <!-- Course Selector Bar trong Tab 3 -->
        <div class="course-bar">
          <div class="course-bar__selector-group">
            <label class="course-bar__label">Lộ trình học:</label>
            <select v-model="selectedCourseId" class="course-bar__select">
              <option value="all">🌐 Tất cả Lộ trình (Tổng hợp)</option>
              <option v-for="course in courses" :key="course.id" :value="course.id">
                📖 {{ course.title }}
              </option>
            </select>
          </div>
          <div class="flex items-center gap-2">
            <select v-model="feedbackStatusFilter" class="course-bar__select text-xs py-1.5 w-auto">
              <option value="">Tất cả Trạng thái</option>
              <option value="New">Mới</option>
              <option value="Read">Đã đọc</option>
              <option value="Resolved">Đã xử lý</option>
            </select>
            <Button size="sm" variant="secondary" @click="loadFeedback">
              <RefreshCw :size="14" /> Làm mới
            </Button>
          </div>
        </div>

        <div class="flex items-center justify-between gap-4 flex-wrap">
          <div class="search-box">
            <Search :size="14" class="search-box__icon" />
            <input
              v-model="feedbackSearchQuery"
              type="text"
              class="search-box__input"
              placeholder="Tìm nội dung góp ý..."
            />
          </div>
        </div>

        <EmptyState
          v-if="filteredFeedbacks.length === 0"
          icon="user"
          title="Chưa có ý kiến nào từ học viên"
          description="Khi học viên gửi góp ý, báo lỗi hoặc yêu cầu trong khóa học, chúng sẽ hiển thị ở đây."
        />

        <div v-else class="space-y-3">
          <article
            v-for="item in filteredFeedbacks"
            :key="item.id"
            class="p-4 rounded-xl border border-slate-700/70 bg-slate-900/60 space-y-3"
          >
            <div class="flex items-center justify-between gap-2 flex-wrap">
              <div class="flex items-center gap-2">
                <span class="w-8 h-8 rounded-full bg-primary-600/30 text-primary-300 flex items-center justify-center font-bold text-xs">
                  {{ item.userName ? item.userName.charAt(0).toUpperCase() : 'U' }}
                </span>
                <div>
                  <div class="text-sm font-semibold text-slate-100">{{ item.userName }}</div>
                  <div class="text-xs text-slate-400">Khóa học: {{ item.courseTitle }} • {{ formatDate(item.createdAt) }}</div>
                </div>
              </div>

              <div class="flex items-center gap-1.5">
                <Badge :variant="item.type === 'Bug' ? 'danger' : 'secondary'">{{ item.type }}</Badge>
                <Badge :variant="item.status === 'Resolved' ? 'success' : 'warning'">{{ item.status }}</Badge>
              </div>
            </div>

            <p class="text-sm text-slate-200 bg-slate-950/40 p-3 rounded-lg border border-slate-800">
              {{ item.content }}
            </p>

            <!-- Trả lời feedback -->
            <div v-if="item.replyText" class="p-3 bg-primary-950/30 border border-primary-800/40 rounded-lg text-xs text-primary-200">
              <strong>Phản hồi của giảng viên ({{ item.repliedByName || 'Bạn' }}):</strong>
              <p class="mt-1">{{ item.replyText }}</p>
            </div>

            <div v-else class="flex items-center gap-2 pt-1">
              <input
                v-model="replyTexts[item.id]"
                type="text"
                placeholder="Nhập phản hồi trực tiếp cho học viên..."
                class="flex-1 rounded-md border border-slate-700 bg-slate-950 px-3 py-1.5 text-xs text-slate-100 focus:outline-none focus:ring-1 focus:ring-primary-500"
              />
              <Button
                size="sm"
                variant="primary"
                :disabled="replySaving[item.id]"
                @click="sendFeedbackReply(item)"
              >
                <Send :size="13" /> Gửi phản hồi
              </Button>
            </div>
          </article>
        </div>
      </section>
    </div>

    <!-- ═══ MODAL TẠO/SỬA CHƯƠNG (TOPIC) ═══ -->
    <Modal
      :open="topicFormOpen"
      :title="editingTopicId ? 'Chỉnh sửa Chương / Chủ đề' : 'Thêm Chương / Chủ đề mới'"
      @close="topicFormOpen = false"
    >
      <form class="space-y-4 py-2" @submit.prevent="saveTopic">
        <div>
          <label class="block text-sm font-medium mb-1">Tên chương / chủ đề <span class="text-rose-400">*</span></label>
          <Input v-model="topicForm.name" placeholder="Ví dụ: Chương 1 - Thuật toán Sắp xếp cơ bản" required />
        </div>

        <div>
          <label class="block text-sm font-medium mb-1">Mô tả tóm tắt</label>
          <textarea
            v-model="topicForm.description"
            rows="3"
            class="w-full rounded-md border border-slate-700 bg-slate-900 p-2.5 text-sm text-slate-100 focus:outline-none focus:ring-1 focus:ring-primary-500"
            placeholder="Mô tả mục tiêu và nội dung chính của chương học này..."
          />
        </div>

        <div class="flex justify-end gap-2 pt-2">
          <Button type="button" variant="ghost" @click="topicFormOpen = false">Hủy</Button>
          <Button type="submit" variant="primary">Lưu chương</Button>
        </div>
      </form>
    </Modal>

    <!-- ═══ MODAL TẠO/SỬA LỘ TRÌNH (COURSE BUILDER) ═══ -->
    <CourseBuilderModal
      :open="courseModalOpen"
      :course-id="editingCourseId"
      :lessons="lessons"
      @close="courseModalOpen = false"
      @saved="onCourseSaved"
    />

    <!-- ═══ MODAL THÊM QUIZ / BÀI TẬP (EXERCISE BUILDER) ═══ -->
    <ExerciseBuilderModal
      :open="exerciseModalOpen"
      :exercise-id="editingExerciseId"
      :default-lesson-id="builderDefaultLessonId"
      :default-stage="builderDefaultStage"
      :default-tab="builderDefaultTab"
      :lessons="lessons"
      @close="exerciseModalOpen = false"
      @saved="() => { exerciseModalOpen = false; load(); }"
    />

    <!-- ═══ MODAL XEM TRƯỚC BÀI HỌC (PREVIEW) ═══ -->
    <Modal :open="previewOpen" size="lg" @close="previewOpen = false">
      <template #header>
        <div class="flex items-center gap-2">
          <BookOpen :size="18" class="text-primary-400" />
          <h2 class="text-lg font-bold">{{ previewTitle }}</h2>
        </div>
      </template>

      <div class="max-h-[65vh] overflow-y-auto pr-1">
        <ProseContent :html="previewContent" />
      </div>

      <template #footer>
        <Button variant="secondary" @click="previewOpen = false">Đóng</Button>
      </template>
    </Modal>
  </main>
</template>

<style scoped>
.studio-view {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  padding-block: var(--space-lg) var(--space-2xl);
}

/* ── Hero Banner ── */
.studio-hero {
  display: flex;
  justify-content: space-between;
  align-items: flex-end;
  gap: var(--space-lg);
  padding: var(--space-xl);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-xl);
  box-shadow: 0 4px 20px -8px rgba(0, 0, 0, 0.4);
}

.studio-hero__kicker {
  margin: 0 0 var(--space-xs);
  color: var(--primary);
  font: 600 var(--text-xs)/1.2 var(--font-mono);
  letter-spacing: 0.12em;
}

.studio-hero__title {
  margin: 0;
  font-size: var(--text-3xl);
  font-weight: 800;
  letter-spacing: -0.02em;
}

.studio-hero__desc {
  margin: var(--space-xs) 0 0;
  color: var(--foreground-secondary);
  max-width: 65ch;
  font-size: var(--text-sm);
}

.studio-hero__stats {
  display: flex;
  gap: var(--space-md);
}

.stat-pill {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-width: 80px;
  padding: var(--space-sm) var(--space-md);
  background: var(--muted);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
}

.stat-pill__num {
  font-family: var(--font-mono);
  font-size: var(--text-xl);
  font-weight: 800;
  color: var(--primary);
}

.stat-pill__label {
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

/* ── Main Studio Tabs ── */
.studio-main-tabs {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  padding: 4px;
  background: var(--muted);
  border: 1px solid var(--border);
  border-radius: var(--radius-xl);
  overflow-x: auto;
}

.studio-main-tab {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  padding: 10px 18px;
  border-radius: var(--radius-lg);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--foreground-secondary);
  background: none;
  border: none;
  cursor: pointer;
  white-space: nowrap;
  transition: all 150ms ease;
}

.studio-main-tab:hover {
  color: var(--foreground);
  background: color-mix(in srgb, var(--card) 50%, transparent);
}

.studio-main-tab--active {
  color: var(--primary);
  background: var(--card);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.25);
}

/* ── Course Selector Bar ── */
.course-bar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-md) var(--space-lg);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
}

.course-bar__selector-group {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.course-bar__label {
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--foreground);
  white-space: nowrap;
}

.course-bar__select {
  min-width: 280px;
  max-width: 420px;
  padding: 8px 12px;
  border-radius: var(--radius-md);
  background: var(--muted);
  border: 1px solid var(--border);
  color: var(--foreground);
  font-size: var(--text-sm);
  font-weight: 500;
  outline: none;
  cursor: pointer;
}

.course-bar__actions {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
}

/* ── Toolbar & Switcher ── */
.studio-toolbar {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.studio-toolbar__left,
.studio-toolbar__right {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  flex-wrap: wrap;
}

.view-switch {
  display: inline-flex;
  padding: 3px;
  background: var(--muted);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
}

.view-switch__btn {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  padding: 6px 14px;
  border-radius: var(--radius-md);
  font-size: var(--text-xs);
  font-weight: 600;
  color: var(--foreground-secondary);
  background: none;
  border: none;
  cursor: pointer;
  transition: all 150ms ease;
}

.view-switch__btn--active {
  background: var(--card);
  color: var(--foreground);
  box-shadow: 0 1px 4px rgba(0, 0, 0, 0.3);
}

.search-box {
  position: relative;
  display: flex;
  align-items: center;
}

.search-box__icon {
  position: absolute;
  left: 10px;
  color: var(--foreground-tertiary);
}

.search-box__input {
  width: 240px;
  padding: 7px 12px 7px 32px;
  border-radius: var(--radius-md);
  background: var(--card);
  border: 1px solid var(--border);
  color: var(--foreground);
  font-size: var(--text-xs);
  outline: none;
  transition: border-color 150ms;
}

.search-box__input:focus {
  border-color: var(--primary);
}

/* ── Curriculum Hierarchy Tree ── */
.curriculum-tree__list {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

.chapter-card {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-xl);
  overflow: hidden;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.2);
  transition: border-color 150ms;
}

.chapter-card:hover {
  border-color: var(--border-strong);
}

.chapter-card__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: var(--space-md) var(--space-lg);
  background: color-mix(in srgb, var(--card) 90%, var(--muted));
  border-bottom: 1px solid var(--border);
  cursor: pointer;
  user-select: none;
  gap: var(--space-md);
}

.chapter-card__header-left {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  flex: 1;
  min-width: 0;
}

.chapter-card__toggle-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 28px;
  height: 28px;
  background: none;
  border: none;
  color: var(--foreground-secondary);
  cursor: pointer;
}

.chapter-card__badge-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 700;
  padding: 3px 8px;
  border-radius: var(--radius-sm);
  background: var(--primary);
  color: var(--primary-foreground);
  white-space: nowrap;
}

.chapter-card__titles {
  min-width: 0;
}

.chapter-card__name {
  margin: 0;
  font-size: var(--text-base);
  font-weight: 700;
  color: var(--foreground);
}

.chapter-card__desc {
  margin: 2px 0 0;
  font-size: var(--text-xs);
  color: var(--foreground-secondary);
}

.chapter-card__header-right {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
}

.chapter-card__body {
  padding: var(--space-xs) 0;
}

.chapter-card__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--space-xs);
  padding: var(--space-xl) var(--space-md);
  color: var(--foreground-tertiary);
  font-size: var(--text-sm);
}

/* ── Lesson Row inside Chapter ── */
.chapter-card__lessons {
  display: flex;
  flex-direction: column;
}

.lesson-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-md);
  padding: var(--space-sm) var(--space-lg);
  border-bottom: 1px solid var(--border-subtle);
  transition: background 150ms;
}

.lesson-row:last-child {
  border-bottom: none;
}

.lesson-row:hover {
  background: var(--muted);
}

.lesson-row__main {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex: 1;
  min-width: 0;
}

.lesson-row__index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 700;
  color: var(--foreground-tertiary);
  min-width: 48px;
}

.lesson-row__text {
  min-width: 0;
}

.lesson-row__title {
  font-weight: 600;
  font-size: var(--text-sm);
  color: var(--foreground);
}

.lesson-row__desc {
  margin: 2px 0 0;
  font-size: var(--text-xs);
  color: var(--foreground-secondary);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  max-width: 500px;
}

.lesson-row__tags {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
}

.lesson-row__tag {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 2px 8px;
  border-radius: var(--radius-sm);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  background: var(--muted);
  border: 1px solid var(--border);
}

.lesson-row__tag--active {
  color: var(--primary);
  border-color: color-mix(in srgb, var(--primary) 30%, transparent);
  background: color-mix(in srgb, var(--primary) 10%, transparent);
}

.lesson-row__actions {
  display: flex;
  align-items: center;
  gap: 6px;
}

/* ── Nút Thêm Chương Dưới Cùng ── */
.add-chapter-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-sm);
  width: 100%;
  padding: var(--space-lg);
  border: 2px dashed var(--border);
  border-radius: var(--radius-xl);
  background: var(--card);
  color: var(--foreground-secondary);
  font-size: var(--text-sm);
  font-weight: 600;
  cursor: pointer;
  transition: all 150ms ease;
}

.add-chapter-btn:hover {
  border-color: var(--primary);
  color: var(--primary);
  background: color-mix(in srgb, var(--primary) 5%, var(--card));
}

/* ── Flat Table Mode ── */
.flat-table-wrap {
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-xl);
  overflow: hidden;
}

.flat-table table {
  width: 100%;
  border-collapse: collapse;
}

.flat-table th,
.flat-table td {
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-sm);
  border-bottom: 1px solid var(--border);
}

.flat-table th {
  background: var(--muted);
  font-weight: 600;
  color: var(--foreground-secondary);
  text-align: left;
}

@media (max-width: 768px) {
  .studio-hero {
    flex-direction: column;
    align-items: flex-start;
  }
  .course-bar {
    flex-direction: column;
    align-items: flex-start;
  }
  .chapter-card__header {
    flex-direction: column;
    align-items: flex-start;
  }
  .lesson-row {
    flex-direction: column;
    align-items: flex-start;
  }
}
</style>

