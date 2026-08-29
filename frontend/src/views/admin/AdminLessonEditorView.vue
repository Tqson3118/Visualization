<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue';
import { useRoute, useRouter, onBeforeRouteLeave } from 'vue-router';
import {
  ArrowLeft,
  BookOpen,
  Check,
  Code2,
  Download,
  Eye,
  FileCode,
  FileText,
  HelpCircle,
  Layers,
  Columns2,
  Maximize2,
  PenTool,
  Plus,
  RefreshCw,
  Save,
  Search,
  Settings,
  Sparkles,
  Table,
  Trash2,
  Upload,
  X,
  Zap,
} from 'lucide-vue-next';

import * as lessonsApi from '@/api/lessons';
import type { Topic, LessonDto, LessonStatusValue } from '@/api/lessons';
import * as simulationsApi from '@/api/simulations';
import type { SimulationMetaDto } from '@/api/simulations';
import { useUiStore } from '@/stores/ui';
import { useAuthStore } from '@/stores/auth';
import { parseMarkdownToHtml } from '@/utils/markdownParser';
import { LESSON_TEMPLATES, type LessonTemplate } from '@/data/lessonTemplates';
import { formatLessonWithAi, getAiUsageRemaining } from '@/services/aiFormatService';
import { courseApi } from '@/services/courseApi';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import Input from '@/components/ui/Input.vue';
import TipTapEditor from '@/components/ui/TipTapEditor.vue';

const route = useRoute();
const router = useRouter();
const ui = useUiStore();
const auth = useAuthStore();

// ── Trạng thái trang & chế độ ──
const isEdit = computed(() => Boolean(route.params.id));
const lessonId = computed(() => (route.params.id ? Number(route.params.id) : null));

const loading = ref(true);
const saving = ref(false);
const isDirty = ref(false);
const initialSnapshot = ref<string>('');
const lastSavedDraftTime = ref<string | null>(null);
const hasRestorableDraft = ref(false);
const aiFormatting = ref(false);
const aiRemaining = ref(getAiUsageRemaining(auth.user?.email || 'default'));

async function handleAiFormat(): Promise<void> {
  if (!form.markdown.trim()) {
    ui.showToast('Vui lòng nhập nội dung bài giảng trước khi dùng AI format.', 'warning');
    return;
  }
  aiFormatting.value = true;
  try {
    const formatted = await formatLessonWithAi(form.markdown, auth.user?.email || 'default');
    form.markdown = formatted;
    aiRemaining.value = getAiUsageRemaining(auth.user?.email || 'default');
    ui.showToast('Đã định dạng bài giảng thành công bằng AI (DeepSeek)!', 'success');
  } catch (err: any) {
    ui.showToast(err.message || 'Lỗi khi định dạng AI.', 'error');
  } finally {
    aiFormatting.value = false;
  }
}

// View Mode: 'split' | 'editor' | 'preview'
const viewMode = ref<'split' | 'editor' | 'preview'>('split');
const editorType = ref<'wysiwyg' | 'markdown'>('wysiwyg');

// Dữ liệu Topics & Simulations
const topics = ref<Topic[]>([]);
const allSimulations = ref<SimulationMetaDto[]>([]);
const simSearch = ref('');
const simCategoryFilter = ref<string>('all');

// Form dữ liệu bài học
const form = reactive({
  title: '',
  description: '',
  topicId: 1,
  sortOrder: 1,
  status: 'active' as LessonStatusValue,
  isClassOnly: false,
  markdown: '',
  selectedSimulations: [] as string[],
});

const renderedPreviewHtml = computed(() => parseMarkdownToHtml(form.markdown));

// Auto-save storage key
const draftStorageKey = computed(() => `dsa_lesson_draft_${isEdit.value ? lessonId.value : 'new'}`);

// ── Tải dữ liệu ban đầu ──
onMounted(async () => {
  loading.value = true;
  try {
    const [topicList, simPage] = await Promise.all([
      lessonsApi.fetchTopics(),
      simulationsApi.fetchSimulations(),
    ]);
    topics.value = topicList;
    allSimulations.value = simPage.items;

    if (route.query.topicId) {
      form.topicId = Number(route.query.topicId);
    } else if (topics.value.length > 0) {
      form.topicId = topics.value[0].id;
    }

    if (isEdit.value && lessonId.value) {
      try {
        const lesson = await lessonsApi.fetchLesson(lessonId.value);
        if (auth.role !== 'ADMIN' && lesson.createdBy && lesson.createdBy !== auth.user?.id) {
          ui.showToast('Bạn không có quyền chỉnh sửa bài học của giảng viên khác.', 'error');
          void router.replace('/studio');
          return;
        }
        form.title = lesson.title;
        form.description = lesson.description || '';
        form.topicId = lesson.topicId;
        form.sortOrder = lesson.sortOrder;
        form.status = lesson.status;
        form.isClassOnly = lesson.isClassOnly || false;
        form.selectedSimulations = (lesson.simulations || []).map((s) => s.simulationKey);

        // Trích xuất markdown nếu có hoặc hiển thị nội dung
        form.markdown = lesson.contentHtml || '';
      } catch (err: any) {
        ui.showToast('Không thể tải thông tin bài học hoặc bạn không có quyền truy cập.', 'error');
        void router.replace('/studio');
        return;
      }
    } else {
      // Mặc định nạp mẫu giải thuật chuẩn cho bài mới
      if (!form.markdown) {
        form.markdown = LESSON_TEMPLATES[0].content;
        form.title = 'Bài học mới: Giải thuật & Cấu trúc dữ liệu';
        form.description = 'Nắm vững nguyên lý hoạt động, phân tích độ phức tạp và thực hành trực quan.';
      }
    }

    // Kiểm tra bản nháp lưu tạm
    checkForDraft();
  } catch (err) {
    ui.showToast('Không thể tải thông tin bài học.', 'error');
  } finally {
    loading.value = false;
    initialSnapshot.value = JSON.stringify(form);
    setTimeout(() => {
      isDirty.value = false;
    }, 200);
  }
});

// Theo dõi thay đổi để kích hoạt Auto-save & Dirty check
watch(
  () => [form.title, form.description, form.topicId, form.markdown, form.selectedSimulations],
  () => {
    if (!loading.value) {
      isDirty.value = true;
      saveDraftDebounced();
    }
  },
  { deep: true },
);

// ── Auto-save vào LocalStorage ──
let saveTimer: ReturnType<typeof setTimeout> | null = null;
function saveDraftDebounced(): void {
  if (saveTimer) clearTimeout(saveTimer);
  saveTimer = setTimeout(() => {
    try {
      const payload = {
        title: form.title,
        description: form.description,
        topicId: form.topicId,
        sortOrder: form.sortOrder,
        status: form.status,
        markdown: form.markdown,
        selectedSimulations: form.selectedSimulations,
        savedAt: new Date().toISOString(),
      };
      localStorage.setItem(draftStorageKey.value, JSON.stringify(payload));
      const d = new Date();
      lastSavedDraftTime.value = `${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}:${d.getSeconds().toString().padStart(2, '0')}`;
    } catch {
      // Bỏ qua nếu quota đầy
    }
  }, 1000);
}

function checkForDraft(): void {
  try {
    const raw = localStorage.getItem(draftStorageKey.value);
    if (!raw) return;
    const draft = JSON.parse(raw);
    if (draft && draft.savedAt) {
      // Nếu có nháp khác với nội dung hiện tại
      if (draft.markdown && draft.markdown !== form.markdown) {
        hasRestorableDraft.value = true;
      }
    }
  } catch {
    hasRestorableDraft.value = false;
  }
}

function restoreDraft(): void {
  try {
    const raw = localStorage.getItem(draftStorageKey.value);
    if (!raw) return;
    const draft = JSON.parse(raw);
    if (draft) {
      if (draft.title) form.title = draft.title;
      if (draft.description) form.description = draft.description;
      if (draft.topicId) form.topicId = draft.topicId;
      if (draft.markdown) form.markdown = draft.markdown;
      if (draft.selectedSimulations) form.selectedSimulations = draft.selectedSimulations;
      hasRestorableDraft.value = false;
      ui.showToast('Đã khôi phục bản nháp tự lưu!', 'success');
    }
  } catch {
    ui.showToast('Không thể khôi phục bản nháp.', 'error');
  }
}

function discardDraft(): void {
  localStorage.removeItem(draftStorageKey.value);
  hasRestorableDraft.value = false;
}

const isNavigatingAwayAfterSave = ref(false);

// ── Cảnh báo rời trang khi có thay đổi chưa lưu ──
onBeforeRouteLeave((_to, _from, next) => {
  if (isNavigatingAwayAfterSave.value) {
    next();
    return;
  }
  const isActuallyDirty = initialSnapshot.value && JSON.stringify(form) !== initialSnapshot.value;
  if (isActuallyDirty) {
    const answer = window.confirm('Bạn có thay đổi chưa lưu trên bài học. Bạn có chắc chắn muốn rời đi?');
    if (answer) {
      next();
    } else {
      next(false);
    }
  } else {
    next();
  }
});

// ── Thao tác Toolbar Markdown ──
const textareaRef = ref<HTMLTextAreaElement | null>(null);

function insertFormatting(prefix: string, suffix: string = '', defaultPlaceholder: string = ''): void {
  const el = textareaRef.value;
  if (!el) {
    form.markdown += `${prefix}${defaultPlaceholder}${suffix}`;
    return;
  }

  const start = el.selectionStart;
  const end = el.selectionEnd;
  const text = form.markdown;
  const selected = text.substring(start, end) || defaultPlaceholder;
  const replacement = `${prefix}${selected}${suffix}`;

  form.markdown = text.substring(0, start) + replacement + text.substring(end);

  setTimeout(() => {
    el.focus();
    el.setSelectionRange(start + prefix.length, start + prefix.length + selected.length);
  }, 0);
}

function applyTemplate(tpl: LessonTemplate): void {
  if (form.markdown && form.markdown.trim().length > 50) {
    if (!confirm('Nội dung hiện tại sẽ được thay thế bằng mẫu mới. Bạn có chắc chắn không?')) {
      return;
    }
  }
  form.markdown = tpl.content;
  ui.showToast(`Đã áp dụng mẫu: "${tpl.name}"`, 'success');
}

// ── Import file Markdown (.md) ──
function triggerFileInput(): void {
  const input = document.createElement('input');
  input.type = 'file';
  input.accept = '.md,.txt,.markdown';
  input.onchange = (e) => {
    const file = (e.target as HTMLInputElement).files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (event) => {
      const content = event.target?.result as string;
      if (content) {
        form.markdown = content;

        // Tự động tìm Tiêu đề H1 (# ...)
        const h1Match = content.match(/^#\s+(.+)$/m);
        if (h1Match && h1Match[1]) {
          form.title = h1Match[1].trim();
        }

        // Tự động tìm mô tả tóm tắt
        const pMatch = content.match(/^(?!#|>|```|\||-|\d+\.)([A-ZÀ-Ỹa-zà-ỹ0-9\s,.\-—–()]{20,200})$/m);
        if (pMatch && pMatch[1]) {
          form.description = pMatch[1].trim();
        }

        ui.showToast(`Đã nhập thành công file "${file.name}"!`, 'success');
      }
    };
    reader.readAsText(file);
  };
  input.click();
}

// ── Bộ lọc danh sách 40+ mô phỏng tương tác ──
const simulationCategories = [
  { key: 'all', label: 'Tất cả (40+)' },
  { key: 'sort', label: 'Sắp xếp (Sorting)' },
  { key: 'search', label: 'Tìm kiếm (Search)' },
  { key: 'linear', label: 'CTDL Tuyến tính' },
  { key: 'tree', label: 'Cây & BST' },
  { key: 'graph', label: 'Đồ thị (Graph)' },
  { key: 'hash', label: 'Bảng băm (Hash)' },
];

const filteredSimulations = computed(() => {
  let list = allSimulations.value;

  // Lọc theo danh mục
  if (simCategoryFilter.value === 'sort') {
    list = list.filter((s) => s.key.startsWith('sort.') || s.tags?.some((t) => t.includes('sort')));
  } else if (simCategoryFilter.value === 'search') {
    list = list.filter((s) => s.key.startsWith('search.') || s.tags?.some((t) => t.includes('search')));
  } else if (simCategoryFilter.value === 'linear') {
    list = list.filter((s) => s.key.startsWith('list.') || s.key.startsWith('stack.') || s.key.startsWith('queue.'));
  } else if (simCategoryFilter.value === 'tree') {
    list = list.filter((s) => s.key.startsWith('tree.') || s.key.startsWith('bst.'));
  } else if (simCategoryFilter.value === 'graph') {
    list = list.filter((s) => s.key.startsWith('graph.'));
  } else if (simCategoryFilter.value === 'hash') {
    list = list.filter((s) => s.key.startsWith('hash.') || s.key.startsWith('set.'));
  }

  // Tìm kiếm theo từ khóa
  if (simSearch.value.trim()) {
    const q = simSearch.value.trim().toLowerCase();
    list = list.filter(
      (s) =>
        s.title.toLowerCase().includes(q) ||
        s.key.toLowerCase().includes(q) ||
        s.dataStructure?.toLowerCase().includes(q) ||
        s.tags?.some((t) => t.toLowerCase().includes(q)),
    );
  }

  return list;
});

function toggleSimulation(key: string): void {
  const index = form.selectedSimulations.indexOf(key);
  if (index >= 0) {
    form.selectedSimulations.splice(index, 1);
  } else {
    form.selectedSimulations.push(key);
  }
}

function removeSimulation(key: string): void {
  const index = form.selectedSimulations.indexOf(key);
  if (index >= 0) {
    form.selectedSimulations.splice(index, 1);
  }
}

// ── Lưu / Xuất bản bài học ──
async function handleSave(): Promise<void> {
  if (form.title.trim().length < 3) {
    ui.showToast('Vui lòng nhập tiêu đề bài học (tối thiểu 3 ký tự).', 'warning');
    return;
  }
  if (!form.markdown.trim()) {
    ui.showToast('Vui lòng soạn thảo nội dung bài học trước khi lưu.', 'warning');
    return;
  }

  saving.value = true;
  try {
    const htmlContent = parseMarkdownToHtml(form.markdown);

    // Quyền Teacher: nếu chọn nội bộ lớp -> active ngay; nếu chọn công khai toàn hệ thống và đang active -> chuyển pendingreview
    let saveStatus = form.status;
    if (auth.role === 'TEACHER') {
      if (form.isClassOnly) {
        saveStatus = 'active';
      } else if (form.status === 'active') {
        saveStatus = 'pendingreview';
      }
    }

    const payload: lessonsApi.LessonUpsertRequest = {
      topicId: Number(form.topicId),
      title: form.title.trim(),
      description: form.description.trim() || undefined,
      contentHtml: htmlContent,
      status: saveStatus,
      isClassOnly: form.isClassOnly,
      sortOrder: Number(form.sortOrder) || 1,
      simulationKeys: [...form.selectedSimulations],
    };

    if (isEdit.value && lessonId.value) {
      await lessonsApi.updateLesson(lessonId.value, payload);
      if (saveStatus === 'pendingreview') {
        ui.showToast('Đã lưu bài giảng! Yêu cầu xuất bản công khai đang chờ Quản trị viên duyệt.', 'info');
      } else {
        ui.showToast('Đã cập nhật bài học thành công!', 'success');
      }
    } else {
      const created = await lessonsApi.createLesson(payload);
      if (route.query.courseId && created?.id) {
        try {
          await courseApi.addCourseNode(Number(route.query.courseId), {
            title: created.title,
            lessonId: created.id,
          });
        } catch {
          // Bỏ qua nếu đã gắn
        }
      }
      if (saveStatus === 'pendingreview') {
        ui.showToast('Đã tạo bài học mới! Bài học công khai đang chờ Quản trị viên phê duyệt.', 'info');
      } else if (form.isClassOnly) {
        ui.showToast('Đã tạo bài giảng lớp học và kích hoạt ngay cho sinh viên!', 'success');
      } else {
        ui.showToast('Đã tạo bài học mới và gắn vào lộ trình thành công!', 'success');
      }
    }

    // Xóa nháp sau khi lưu thành công
    localStorage.removeItem(draftStorageKey.value);
    isDirty.value = false;
    isNavigatingAwayAfterSave.value = true;

    // Quay lại trang quản trị nội dung với lộ trình tương ứng
    if (route.query.courseId) {
      await router.push(`/studio?courseId=${route.query.courseId}`);
    } else {
      await router.push('/studio');
    }
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Lưu bài học thất bại.', 'error');
  } finally {
    saving.value = false;
  }
}

function goBack(): void {
  if (route.query.courseId) {
    void router.push(`/studio?courseId=${route.query.courseId}`);
  } else {
    void router.push('/studio');
  }
}
</script>

<template>
  <div class="admin-lesson-studio">
    <!-- ═══ TOP NAVBAR: ACTIONS & STATUS ═══ -->
    <header class="studio-header">
      <div class="studio-header__left">
        <Button variant="ghost" size="sm" class="gap-1.5" @click="goBack">
          <ArrowLeft :size="16" /> Quay lại danh sách
        </Button>
        <span class="studio-header__divider" />
        <div class="studio-header__title-badge">
          <span class="studio-header__mode-tag">
            {{ isEdit ? `Chỉnh sửa bài học #${lessonId}` : 'Tạo bài học mới' }}
          </span>
          <span v-if="lastSavedDraftTime" class="studio-header__draft-status">
            <Check :size="12" class="text-emerald-400" /> Tự lưu nháp: {{ lastSavedDraftTime }}
          </span>
        </div>
      </div>

      <div class="studio-header__right">
        <!-- Nút import markdown -->
        <Button variant="secondary" size="sm" class="gap-1.5" @click="triggerFileInput">
          <Upload :size="14" /> Nhập file .md
        </Button>

        <!-- Dropdown Mẫu bài giảng -->
        <div class="studio-dropdown">
          <Button variant="secondary" size="sm" class="gap-1.5">
            <Sparkles :size="14" /> Mẫu bài giảng ▾
          </Button>
          <div class="studio-dropdown__menu">
            <button
              v-for="tpl in LESSON_TEMPLATES"
              :key="tpl.id"
              type="button"
              class="studio-dropdown__item"
              @click="applyTemplate(tpl)"
            >
              <span class="font-bold text-white text-xs block">{{ tpl.name }}</span>
              <span class="text-[11px] text-vdsa-muted block mt-0.5">{{ tpl.description }}</span>
            </button>
          </div>
        </div>

        <!-- View mode switcher -->
        <div class="studio-viewmode">
          <button
            type="button"
            class="studio-viewmode__btn"
            :class="{ 'studio-viewmode__btn--active': viewMode === 'split' }"
            title="Xem song song Editor & Preview"
            @click="viewMode = 'split'"
          >
            <Columns2 :size="14" /> Song song
          </button>
          <button
            type="button"
            class="studio-viewmode__btn"
            :class="{ 'studio-viewmode__btn--active': viewMode === 'editor' }"
            title="Chỉ mở khung soạn thảo"
            @click="viewMode = 'editor'"
          >
            <PenTool :size="14" /> Soạn thảo
          </button>
          <button
            type="button"
            class="studio-viewmode__btn"
            :class="{ 'studio-viewmode__btn--active': viewMode === 'preview' }"
            title="Chỉ mở khung xem trước"
            @click="viewMode = 'preview'"
          >
            <Eye :size="14" /> Xem trước
          </button>
        </div>

        <!-- Primary Save Button -->
        <Button variant="primary" size="sm" class="gap-1.5" :loading="saving" @click="handleSave">
          <Save :size="15" /> {{ isEdit ? 'Lưu cập nhật' : 'Xuất bản bài học' }}
        </Button>
      </div>
    </header>

    <!-- Alert khôi phục nháp nếu có -->
    <div v-if="hasRestorableDraft" class="studio-draft-alert">
      <div class="flex items-center gap-2">
        <Sparkles :size="16" class="text-amber-400" />
        <span class="text-xs text-amber-200">
          Phát hiện bản nháp tự lưu gần nhất của bạn. Bạn có muốn khôi phục nội dung đang gõ dở không?
        </span>
      </div>
      <div class="flex items-center gap-2">
        <Button size="sm" variant="primary" @click="restoreDraft">Khôi phục ngay</Button>
        <Button size="sm" variant="ghost" @click="discardDraft">Bỏ qua</Button>
      </div>
    </div>

    <!-- ═══ MAIN WORKSPACE (2 COLUMNS) ═══ -->
    <div v-if="loading" class="studio-loading">
      <Skeleton height="60px" />
      <div class="grid grid-cols-3 gap-4 mt-4">
        <Skeleton height="500px" class="col-span-2" />
        <Skeleton height="500px" />
      </div>
    </div>

    <div v-else class="studio-workspace">
      <!-- CỘT TRÁI (72%): TRÌNH SOẠN THẢO VÀ LIVE PREVIEW -->
      <main class="studio-main">
        <!-- Input Tiêu đề bài học -->
        <div class="studio-title-box">
          <input
            v-model="form.title"
            class="studio-title-input"
            type="text"
            placeholder="Nhập tiêu đề bài học (VD: Thuật toán Quick Sort & Phân tích Big-O)..."
          />
        </div>

        <!-- Mode Switcher: WYSIWYG Word-like TipTap vs Raw Markdown -->
        <div class="flex items-center justify-between pb-3">
          <div class="flex items-center gap-2">
            <button
              type="button"
              @click="editorType = 'wysiwyg'"
              :class="editorType === 'wysiwyg' ? 'bg-vdsa-accent text-white shadow-md' : 'bg-vdsa-surface text-vdsa-secondary hover:text-white'"
              class="px-3.5 py-1.5 rounded-xl text-xs font-bold transition-colors flex items-center gap-1.5 border border-vdsa-border"
            >
              <PenTool :size="14" /> Soạn thảo trực quan kiểu Word (TipTap)
            </button>
            <button
              type="button"
              @click="editorType = 'markdown'"
              :class="editorType === 'markdown' ? 'bg-vdsa-accent text-white shadow-md' : 'bg-vdsa-surface text-vdsa-secondary hover:text-white'"
              class="px-3.5 py-1.5 rounded-xl text-xs font-bold transition-colors flex items-center gap-1.5 border border-vdsa-border"
            >
              <FileCode :size="14" /> Soạn thảo Markdown (Split View)
            </button>
          </div>

          <div v-if="editorType === 'wysiwyg'" class="text-xs text-vdsa-muted flex items-center gap-2">
            <button
              type="button"
              :disabled="aiFormatting || aiRemaining <= 0"
              class="studio-tool-btn bg-purple-600/30 text-purple-300 border-purple-500/50 hover:bg-purple-600/50 flex items-center gap-1.5 font-bold px-3 py-1.5 rounded-xl"
              :title="aiRemaining <= 0 ? 'Đã hết lượt dùng AI miễn phí (5/5)' : 'Tự động chuẩn hóa & làm đẹp bài giảng bằng DeepSeek AI'"
              @click="handleAiFormat"
            >
              <Sparkles :size="13" class="text-purple-400" :class="{ 'animate-spin': aiFormatting }" />
              {{ aiFormatting ? 'AI đang format...' : `✨ Format AI (${aiRemaining}/5)` }}
            </button>
          </div>
        </div>

        <!-- Mode 1: TipTap WYSIWYG Word Editor -->
        <div v-if="editorType === 'wysiwyg'" class="mb-4">
          <TipTapEditor v-model="form.markdown" placeholder="Bắt đầu soạn thảo lý thuyết bài giảng trực quan kiểu Word..." />
        </div>

        <!-- Mode 2: Markdown Formatting Toolbar & Panes -->
        <div v-else class="space-y-4">
          <div class="studio-toolbar">
            <div class="studio-toolbar__group">
              <button type="button" class="studio-tool-btn" title="Tiêu đề H1" @click="insertFormatting('# ', '', 'Tiêu đề 1')">H1</button>
              <button type="button" class="studio-tool-btn" title="Tiêu đề H2" @click="insertFormatting('## ', '', 'Tiêu đề 2')">H2</button>
              <button type="button" class="studio-tool-btn" title="Tiêu đề H3" @click="insertFormatting('### ', '', 'Tiêu đề 3')">H3</button>
            </div>

            <div class="studio-toolbar__group">
              <button type="button" class="studio-tool-btn" title="In đậm" @click="insertFormatting('**', '**', 'văn bản đậm')"><b>B</b></button>
              <button type="button" class="studio-tool-btn" title="In nghiêng" @click="insertFormatting('*', '*', 'văn bản nghiêng')"><i>I</i></button>
              <button type="button" class="studio-tool-btn" title="Inline code" @click="insertFormatting('`', '`', 'code')">&lt;/&gt;</button>
            </div>

            <div class="studio-toolbar__group">
              <button type="button" class="studio-tool-btn studio-tool-btn--lang" title="Khối Code C++" @click="insertFormatting('```cpp\n// Viết mã C++ ở đây\nvoid example() {\n    \n}\n', '```')">C++</button>
              <button type="button" class="studio-tool-btn studio-tool-btn--lang" title="Khối Code Python" @click="insertFormatting('```python\n# Viết mã Python ở đây\ndef example():\n    pass\n', '```')">Python</button>
              <button type="button" class="studio-tool-btn" title="Bảng biểu Markdown" @click="insertFormatting('\n| Cột 1 | Cột 2 | Cột 3 |\n| :--- | :--- | :--- |\n| Giá trị A | Giá trị B | Giá trị C |\n', '')">
                <Table :size="13" /> Bảng
              </button>
            </div>

            <div class="studio-toolbar__group">
              <button type="button" class="studio-tool-btn studio-tool-btn--note" title="Hộp Ghi chú (Note)" @click="insertFormatting('> [!NOTE]\n> ', '', 'Nội dung ghi chú quan trọng...')">📌 Note</button>
              <button type="button" class="studio-tool-btn studio-tool-btn--tip" title="Hộp Mẹo hay (Tip)" @click="insertFormatting('> [!TIP]\n> ', '', 'Mẹo hay giúp giải nhanh...')">💡 Tip</button>
              <button type="button" class="studio-tool-btn studio-tool-btn--warn" title="Hộp Chú ý (Warning)" @click="insertFormatting('> [!WARNING]\n> ', '', 'Lưu ý các trường hợp biên...')">⚠️ Warn</button>
            </div>

            <div class="studio-toolbar__group ml-auto">
              <button
                type="button"
                :disabled="aiFormatting || aiRemaining <= 0"
                class="studio-tool-btn bg-purple-600/30 text-purple-300 border-purple-500/50 hover:bg-purple-600/50 flex items-center gap-1.5 font-bold"
                :title="aiRemaining <= 0 ? 'Đã hết lượt dùng AI miễn phí (5/5)' : 'Tự động chuẩn hóa & làm đẹp bài giảng bằng DeepSeek AI'"
                @click="handleAiFormat"
              >
                <Sparkles :size="13" class="text-purple-400" :class="{ 'animate-spin': aiFormatting }" />
                {{ aiFormatting ? 'AI đang format...' : `✨ Format AI (${aiRemaining}/5)` }}
              </button>
            </div>
          </div>

        <!-- Khung soạn thảo & Preview (Tùy theo viewMode) -->
        <div class="studio-editor-panes" :class="`studio-editor-panes--${viewMode}`">
          <!-- Pane 1: Editor Textarea -->
          <div v-show="viewMode === 'split' || viewMode === 'editor'" class="studio-pane studio-pane--editor">
            <div class="studio-pane__header">
              <span class="studio-pane__title flex items-center gap-1.5">
                <FileCode :size="13" class="text-vdsa-purple" /> Mã nguồn Markdown
              </span>
              <span class="studio-pane__count">{{ form.markdown.length }} ký tự</span>
            </div>
            <textarea
              ref="textareaRef"
              v-model="form.markdown"
              class="studio-textarea"
              placeholder="Bắt đầu viết bài giảng giải thuật bằng Markdown tại đây..."
              spellcheck="false"
            />
          </div>

          <!-- Pane 2: Live HTML Preview -->
          <div v-show="viewMode === 'split' || viewMode === 'preview'" class="studio-pane studio-pane--preview">
            <div class="studio-pane__header">
              <span class="studio-pane__title flex items-center gap-1.5">
                <Eye :size="13" class="text-emerald-400" /> Kết quả hiển thị bài giảng (Live Preview)
              </span>
              <span class="text-[10px] text-vdsa-muted">Tự động đồng bộ</span>
            </div>
            <div class="studio-preview-content prose-vdsa" v-html="renderedPreviewHtml" />
          </div>
        </div>
      </div>
      </main>

      <!-- CỘT PHẢI (28%): CÀI ĐẶT BÀI HỌC & GẮN MÔ PHỎNG TƯƠNG TÁC -->
      <aside class="studio-sidebar">
        <!-- Khối 1: Thuộc tính bài học -->
        <section class="studio-card">
          <h3 class="studio-card__title flex items-center gap-1.5">
            <Settings :size="14" class="text-vdsa-purple" /> Cấu hình bài giảng
          </h3>

          <div class="space-y-3.5 mt-3">
            <div>
              <label class="studio-label">Chủ đề khóa học (Topic)</label>
              <select v-model.number="form.topicId" class="studio-select">
                <option v-for="t in topics" :key="t.id" :value="t.id">
                  {{ t.name }}
                </option>
              </select>
            </div>

            <div>
              <label class="studio-label">Mô tả tóm tắt (1-2 câu ngắn)</label>
              <textarea
                v-model="form.description"
                rows="2"
                class="studio-input"
                placeholder="Tóm tắt nội dung trọng tâm của bài giảng..."
              />
            </div>

            <div class="grid grid-cols-2 gap-3">
              <div>
                <label class="studio-label">Thứ tự hiển thị</label>
                <input v-model.number="form.sortOrder" type="number" min="1" class="studio-input" />
              </div>

              <div>
                <label class="studio-label">Trạng thái</label>
                <select v-model="form.status" class="studio-select">
                  <option value="active">Kích hoạt (Active)</option>
                  <option value="draft">Bản nháp (Draft)</option>
                  <option value="hidden">Tạm ẩn (Hidden)</option>
                </select>
              </div>
            </div>

            <div>
              <label class="studio-label">Phạm vi phát hành</label>
              <div class="space-y-2 mt-1">
                <label class="flex items-start gap-2.5 p-2.5 rounded-xl border border-vdsa-border bg-white/5 cursor-pointer hover:border-vdsa-accent/50 transition-colors" :class="{ 'border-vdsa-accent bg-vdsa-accent/10': !form.isClassOnly }">
                  <input type="radio" :value="false" v-model="form.isClassOnly" class="mt-0.5" />
                  <div>
                    <span class="text-xs font-bold text-white block">Công khai toàn hệ thống</span>
                    <span class="text-[11px] text-vdsa-muted block">Gửi Quản trị viên duyệt trước khi phát hành cho mọi học viên.</span>
                  </div>
                </label>
                <label class="flex items-start gap-2.5 p-2.5 rounded-xl border border-vdsa-border bg-white/5 cursor-pointer hover:border-emerald-500/50 transition-colors" :class="{ 'border-emerald-500 bg-emerald-500/10': form.isClassOnly }">
                  <input type="radio" :value="true" v-model="form.isClassOnly" class="mt-0.5" />
                  <div>
                    <span class="text-xs font-bold text-emerald-400 block">Nội bộ lớp học của tôi</span>
                    <span class="text-[11px] text-vdsa-muted block">Kích hoạt ngay — dùng để gán trực tiếp cho học viên trong lớp.</span>
                  </div>
                </label>
              </div>
            </div>
          </div>
        </section>

        <!-- Khối 2: Gắn Mô phỏng thuật toán tương tác (40+ thuật toán) -->
        <section class="studio-card studio-card--simulations">
          <div class="flex items-center justify-between">
            <h3 class="studio-card__title flex items-center gap-1.5">
              <Layers :size="14" class="text-vdsa-accent" /> Mô phỏng tương tác
            </h3>
            <span class="text-xs font-bold text-vdsa-purple">
              Đã gắn: {{ form.selectedSimulations.length }}
            </span>
          </div>

          <p class="text-[11px] text-vdsa-muted mt-1">
            Gắn các animation mô phỏng trực quan giúp học viên vừa đọc lý thuyết vừa thực hành từng bước.
          </p>

          <!-- Danh sách Chip đã chọn -->
          <div v-if="form.selectedSimulations.length > 0" class="studio-selected-chips">
            <span v-for="simKey in form.selectedSimulations" :key="simKey" class="studio-chip">
              <code>{{ simKey }}</code>
              <button type="button" class="studio-chip__remove" @click="removeSimulation(simKey)">
                <X :size="11" />
              </button>
            </span>
          </div>

          <!-- Thanh tìm kiếm mô phỏng -->
          <div class="studio-sim-search">
            <Search :size="13" class="text-vdsa-muted" />
            <input
              v-model="simSearch"
              type="text"
              placeholder="Tìm thuật toán (quick, tree, sort...)"
              class="studio-sim-search-input"
            />
            <button v-if="simSearch" type="button" class="text-vdsa-muted hover:text-white" @click="simSearch = ''">
              <X :size="12" />
            </button>
          </div>

          <!-- Filter categories -->
          <div class="studio-sim-categories">
            <button
              v-for="cat in simulationCategories"
              :key="cat.key"
              type="button"
              class="studio-sim-cat-btn"
              :class="{ 'studio-sim-cat-btn--active': simCategoryFilter === cat.key }"
              @click="simCategoryFilter = cat.key"
            >
              {{ cat.label }}
            </button>
          </div>

          <!-- Danh sách mô phỏng cuộn mượt -->
          <div class="studio-sim-list">
            <div
              v-for="sim in filteredSimulations"
              :key="sim.key"
              class="studio-sim-item"
              :class="{ 'studio-sim-item--active': form.selectedSimulations.includes(sim.key) }"
              @click="toggleSimulation(sim.key)"
            >
              <div class="flex items-center gap-2">
                <input
                  type="checkbox"
                  :checked="form.selectedSimulations.includes(sim.key)"
                  class="rounded text-vdsa-purple"
                  @click.stop="toggleSimulation(sim.key)"
                />
                <div>
                  <h4 class="text-xs font-bold text-white">{{ sim.title }}</h4>
                  <span class="text-[10px] text-vdsa-muted font-mono">{{ sim.key }}</span>
                </div>
              </div>
              <Badge variant="muted" class="text-[9px]">{{ sim.category }}</Badge>
            </div>

            <p v-if="filteredSimulations.length === 0" class="text-xs text-vdsa-muted text-center py-6">
              Không tìm thấy thuật toán nào khớp từ khóa.
            </p>
          </div>
        </section>
      </aside>
    </div>
  </div>
</template>

<style scoped>
.admin-lesson-studio {
  display: flex;
  flex-direction: column;
  height: calc(100vh - 64px);
  background: var(--color-background, #0b0f19);
  overflow: hidden;
}

/* ── TOP HEADER ── */
.studio-header {
  height: 56px;
  border-bottom: 1px solid var(--color-border, #21262d);
  background: var(--color-surface, #161b22);
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-inline: var(--space-md);
  flex-shrink: 0;
  gap: var(--space-md);
}

.studio-header__left {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.studio-header__divider {
  width: 1px;
  height: 20px;
  background: var(--color-border, #30363d);
}

.studio-header__title-badge {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.studio-header__mode-tag {
  font-size: 13px;
  font-weight: 700;
  color: var(--color-text-primary, #ffffff);
}

.studio-header__draft-status {
  font-size: 11px;
  color: #8b949e;
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

.studio-header__right {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

/* Dropdown Menu */
.studio-dropdown {
  position: relative;
}

.studio-dropdown__menu {
  display: none;
  position: absolute;
  top: 100%;
  right: 0;
  margin-top: 4px;
  width: 280px;
  background: #161b22;
  border: 1px solid #30363d;
  border-radius: 10px;
  box-shadow: 0 10px 30px rgba(0, 0, 0, 0.5);
  padding: 6px;
  z-index: 50;
}

.studio-dropdown:hover .studio-dropdown__menu {
  display: block;
}

.studio-dropdown__item {
  width: 100%;
  text-align: left;
  padding: 8px 10px;
  border-radius: 6px;
  background: transparent;
  border: none;
  cursor: pointer;
  transition: background 150ms ease;
}

.studio-dropdown__item:hover {
  background: rgba(99, 102, 241, 0.15);
}

/* View Mode Switcher */
.studio-viewmode {
  display: inline-flex;
  background: #0d1117;
  border: 1px solid #30363d;
  border-radius: 8px;
  padding: 2px;
}

.studio-viewmode__btn {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 4px 10px;
  border-radius: 6px;
  border: none;
  background: transparent;
  color: #8b949e;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
  transition: all 150ms ease;
}

.studio-viewmode__btn--active {
  background: #21262d;
  color: #ffffff;
}

/* Alert khôi phục nháp */
.studio-draft-alert {
  background: rgba(245, 158, 11, 0.1);
  border-bottom: 1px solid rgba(245, 158, 11, 0.3);
  padding: 8px var(--space-md);
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-shrink: 0;
}

/* ── WORKSPACE GRID ── */
.studio-workspace {
  flex: 1;
  display: grid;
  grid-template-columns: 1fr 340px;
  overflow: hidden;
}

@media (max-width: 1024px) {
  .studio-workspace {
    grid-template-columns: 1fr;
    overflow-y: auto;
  }
}

.studio-main {
  display: flex;
  flex-direction: column;
  border-right: 1px solid var(--color-border, #21262d);
  overflow: hidden;
}

.studio-title-box {
  padding: 12px 16px;
  border-bottom: 1px solid #21262d;
  background: #0d1117;
}

.studio-title-input {
  width: 100%;
  font-size: 18px;
  font-weight: 800;
  color: #ffffff;
  background: transparent;
  border: none;
  outline: none;
}

.studio-title-input::placeholder {
  color: #484f58;
}

/* ── TOOLBAR ── */
.studio-toolbar {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 16px;
  background: #161b22;
  border-bottom: 1px solid #21262d;
  flex-wrap: wrap;
}

.studio-toolbar__group {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding-right: 8px;
  border-right: 1px solid #30363d;
}

.studio-tool-btn {
  padding: 4px 8px;
  background: #21262d;
  border: 1px solid #30363d;
  border-radius: 6px;
  color: #c9d1d9;
  font-size: 11px;
  font-weight: 700;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  transition: all 150ms ease;
}

.studio-tool-btn:hover {
  background: #30363d;
  color: #ffffff;
}

.studio-tool-btn--lang { color: #58a6ff; }
.studio-tool-btn--note { color: #60a5fa; }
.studio-tool-btn--tip { color: #34d399; }
.studio-tool-btn--warn { color: #fbbf24; }

/* ── DUAL EDITOR PANES ── */
.studio-editor-panes {
  flex: 1;
  display: grid;
  overflow: hidden;
}

.studio-editor-panes--split {
  grid-template-columns: 1fr 1fr;
}
.studio-editor-panes--editor {
  grid-template-columns: 1fr;
}
.studio-editor-panes--preview {
  grid-template-columns: 1fr;
}

.studio-pane {
  display: flex;
  flex-direction: column;
  overflow: hidden;
}

.studio-pane--editor {
  border-right: 1px solid #21262d;
}

.studio-pane__header {
  height: 32px;
  background: #161b22;
  border-bottom: 1px solid #21262d;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding-inline: 12px;
  font-size: 11px;
  font-weight: 700;
  color: #8b949e;
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.studio-textarea {
  flex: 1;
  width: 100%;
  background: #0d1117;
  color: #e6edf3;
  font-family: var(--font-mono, 'Fira Code', monospace);
  font-size: 13px;
  line-height: 1.6;
  padding: 16px;
  border: none;
  outline: none;
  resize: none;
  overflow-y: auto;
}

.studio-pane--preview {
  background: #0d1117;
}

.studio-preview-content {
  flex: 1;
  padding: 20px;
  overflow-y: auto;
  color: #c9d1d9;
}

/* ── RIGHT SIDEBAR ── */
.studio-sidebar {
  background: #161b22;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 16px;
  overflow-y: auto;
}

.studio-card {
  background: #0d1117;
  border: 1px solid #30363d;
  border-radius: 12px;
  padding: 14px;
}

.studio-card__title {
  font-size: 12px;
  font-weight: 800;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: #ffffff;
}

.studio-label {
  display: block;
  font-size: 11px;
  font-weight: 700;
  color: #8b949e;
  margin-bottom: 4px;
  text-transform: uppercase;
}

.studio-select,
.studio-input {
  width: 100%;
  background: #161b22;
  border: 1px solid #30363d;
  border-radius: 8px;
  padding: 8px 10px;
  color: #ffffff;
  font-size: 12px;
  outline: none;
}

.studio-select:focus,
.studio-input:focus {
  border-color: #6366f1;
}

/* Simulation Hub */
.studio-selected-chips {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-top: 10px;
}

.studio-chip {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  background: rgba(99, 102, 241, 0.15);
  border: 1px solid rgba(99, 102, 241, 0.3);
  color: #c4b5fd;
  font-size: 11px;
  padding: 2px 6px;
  border-radius: 6px;
}

.studio-chip__remove {
  background: transparent;
  border: none;
  color: #8b949e;
  cursor: pointer;
  display: flex;
  align-items: center;
}

.studio-chip__remove:hover { color: #ef4444; }

.studio-sim-search {
  display: flex;
  align-items: center;
  gap: 6px;
  background: #161b22;
  border: 1px solid #30363d;
  border-radius: 8px;
  padding: 6px 10px;
  margin-top: 10px;
}

.studio-sim-search-input {
  flex: 1;
  background: transparent;
  border: none;
  outline: none;
  font-size: 11px;
  color: #ffffff;
}

.studio-sim-categories {
  display: flex;
  gap: 4px;
  overflow-x: auto;
  padding-block: 8px;
}

.studio-sim-cat-btn {
  white-space: nowrap;
  padding: 3px 8px;
  border-radius: 4px;
  background: #21262d;
  border: none;
  color: #8b949e;
  font-size: 10px;
  font-weight: 700;
  cursor: pointer;
}

.studio-sim-cat-btn--active {
  background: #6366f1;
  color: #ffffff;
}

.studio-sim-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: 280px;
  overflow-y: auto;
  padding-right: 4px;
}

.studio-sim-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 10px;
  background: #161b22;
  border: 1px solid #21262d;
  border-radius: 8px;
  cursor: pointer;
  transition: all 120ms ease;
}

.studio-sim-item:hover {
  border-color: #6366f1;
}

.studio-sim-item--active {
  border-color: #6366f1;
  background: rgba(99, 102, 241, 0.1);
}
</style>
