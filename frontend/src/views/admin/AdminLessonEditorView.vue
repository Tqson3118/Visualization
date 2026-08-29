<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, reactive, ref, watch } from 'vue';
import { useRoute, useRouter, onBeforeRouteLeave } from 'vue-router';
import {
  ArrowLeft,
  Check,
  Eye,
  Layers,
  PenTool,
  Puzzle,
  Save,
  Settings,
  Sparkles,
} from 'lucide-vue-next';

import * as lessonsApi from '@/api/lessons';
import type { Topic, LessonStatusValue } from '@/api/lessons';
import * as simulationsApi from '@/api/simulations';
import type { SimulationMetaDto } from '@/api/simulations';
import * as classesApi from '@/api/classes';
import { courseApi } from '@/services/courseApi';
import { useUiStore } from '@/stores/ui';
import { useAuthStore } from '@/stores/auth';
import { parseMarkdownToHtml } from '@/utils/markdownParser';
import { LESSON_TEMPLATES } from '@/data/lessonTemplates';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import Modal from '@/components/ui/Modal.vue';

// Sub-tabs
import TheoryTab from './editor-tabs/TheoryTab.vue';
import SimulationTab from './editor-tabs/SimulationTab.vue';
import QuizTab from './editor-tabs/QuizTab.vue';
import SettingsTab from './editor-tabs/SettingsTab.vue';

const route = useRoute();
const router = useRouter();
const ui = useUiStore();
const auth = useAuthStore();

// ── Route & State ──
const isEdit = computed(() => Boolean(route.params.id));
const lessonId = computed(() => (route.params.id ? Number(route.params.id) : null));

const activeTab = ref<'theory' | 'simulations' | 'quiz' | 'settings'>('theory');

const loading = ref(true);
const saving = ref(false);
const isDirty = ref(false);
const initialSnapshot = ref<string>('');
const lastSavedDraftTime = ref<string | null>(null);
const hasRestorableDraft = ref(false);
const previewModalOpen = ref(false);

const theoryTabRef = ref<InstanceType<typeof TheoryTab> | null>(null);

// Topics & Simulations Data
const topics = ref<Topic[]>([]);
const allSimulations = ref<SimulationMetaDto[]>([]);

// Form State
const form = reactive({
  title: '',
  description: '',
  topicId: 1,
  sortOrder: 1,
  status: 'active' as LessonStatusValue,
  isClassOnly: false,
  selectedClassId: null as number | null,
  markdown: '',
  selectedSimulations: [] as string[],
});

// Auto-save storage key
const draftStorageKey = computed(() => `dsa_lesson_draft_${isEdit.value ? lessonId.value : 'new'}`);

// ── Initial Data Load ──
onMounted(async () => {
  loading.value = true;
  try {
    const [topicList, simPage] = await Promise.all([
      lessonsApi.fetchTopics(),
      simulationsApi.fetchSimulations(),
    ]);
    topics.value = topicList;
    allSimulations.value = simPage.items || [];

    if (route.query.topicId) {
      form.topicId = Number(route.query.topicId);
    } else if (topics.value.length > 0) {
      form.topicId = topics.value[0].id;
    }

    if (isEdit.value && lessonId.value) {
      try {
        const lesson = await lessonsApi.fetchLesson(lessonId.value);
        if (auth.role !== 'ADMIN' && lesson.createdBy && lesson.createdBy !== auth.user?.id) {
          ui.showToast('Bạn không có quyền chỉnh sửa bài học này.', 'error');
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
        form.markdown = lesson.contentHtml || '';
      } catch (err: any) {
        if (err?.response?.status === 403 || err?.status === 403 || err?.message?.includes('403') || err?.message?.includes('quyền')) {
          ui.showToast('Bạn không có quyền chỉnh sửa bài học này.', 'error');
        } else {
          ui.showToast('Không thể tải thông tin bài học hoặc bạn không có quyền truy cập.', 'error');
        }
        void router.replace('/studio');
        return;
      }
    } else {
      if (!form.markdown) {
        form.markdown = LESSON_TEMPLATES[0].content;
        form.title = 'Bài học mới: Giải thuật & Cấu trúc dữ liệu';
        form.description = 'Nắm vững nguyên lý hoạt động, phân tích độ phức tạp và thực hành trực quan.';
      }
    }

    checkForDraft();
  } catch {
    ui.showToast('Không thể tải dữ liệu bài học.', 'error');
  } finally {
    loading.value = false;
    initialSnapshot.value = JSON.stringify(form);
    setTimeout(() => {
      isDirty.value = false;
    }, 200);
  }
});

// Auto-save debounced
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
      // ignore
    }
  }, 1000);
}

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

function checkForDraft(): void {
  try {
    const raw = localStorage.getItem(draftStorageKey.value);
    if (!raw) return;
    const draft = JSON.parse(raw);
    if (draft && draft.savedAt && draft.markdown && draft.markdown !== form.markdown) {
      hasRestorableDraft.value = true;
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

onBeforeRouteLeave((_to, _from, next) => {
  if (isNavigatingAwayAfterSave.value) {
    next();
    return;
  }
  const isActuallyDirty = initialSnapshot.value && JSON.stringify(form) !== initialSnapshot.value;
  if (isActuallyDirty) {
    if (window.confirm('Bạn có thay đổi chưa lưu trên bài học. Bạn có chắc chắn muốn rời đi?')) {
      next();
    } else {
      next(false);
    }
  } else {
    next();
  }
});

// Handle simulation anchor insertion into theory tab
function onInsertSimulationAnchor(simKey: string): void {
  activeTab.value = 'theory';
  theoryTabRef.value?.insertSimulationAnchor(simKey);
  ui.showToast(`Đã chèn thẻ mô phỏng "${simKey}" vào nội dung bài giảng!`, 'info');
}

// ── S4: Xem trước bài giảng chính xác ──
function handlePreview(): void {
  if (isEdit.value && lessonId.value) {
    // S4: mở đúng bài đang soạn theo route :id
    window.open(`/lessons/${lessonId.value}`, '_blank');
  } else {
    // Chưa lưu ID -> mở modal xem trước Live Preview
    previewModalOpen.value = true;
  }
}

// ── LE5: Lưu & Xuất bản ──
async function handleSave(): Promise<void> {
  if (form.title.trim().length < 3) {
    ui.showToast('Vui lòng nhập tiêu đề bài học (tối thiểu 3 ký tự).', 'warning');
    activeTab.value = 'theory';
    return;
  }
  if (!form.markdown.trim()) {
    ui.showToast('Vui lòng soạn thảo nội dung bài học trước khi lưu.', 'warning');
    activeTab.value = 'theory';
    return;
  }

  saving.value = true;
  try {
    const htmlContent = parseMarkdownToHtml(form.markdown);

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

    let targetLessonId = lessonId.value;

    if (isEdit.value && lessonId.value) {
      await lessonsApi.updateLesson(lessonId.value, payload);
    } else {
      const created = await lessonsApi.createLesson(payload);
      targetLessonId = created?.id || null;

      // Link to course if requested
      if (route.query.courseId && targetLessonId) {
        try {
          await courseApi.addCourseNode(Number(route.query.courseId), {
            title: created.title,
            lessonId: targetLessonId,
          });
        } catch {
          // ignore
        }
      }
    }

    // LE2: Gán trực tiếp vào lớp học nếu chọn classId
    if (form.isClassOnly && form.selectedClassId && targetLessonId) {
      try {
        await classesApi.createClassAssignment(form.selectedClassId, {
          lessonId: targetLessonId,
        });
        ui.showToast('Đã tự động gán bài giảng vào lớp học được chọn!', 'info');
      } catch {
        // ignore
      }
    }

    localStorage.removeItem(draftStorageKey.value);
    isDirty.value = false;
    isNavigatingAwayAfterSave.value = true;

    // LE5: Toast thành công & điều hướng
    if (saveStatus === 'pendingreview') {
      ui.showToast('Đã lưu bài học! Nội dung công khai đang chờ Quản trị viên duyệt.', 'info');
    } else {
      ui.showToast('Đã xuất bản bài học thành công!', 'success');
    }

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
  <div class="admin-lesson-studio flex flex-col h-[calc(100vh-64px)] bg-[#0b0f19] text-white overflow-hidden">
    <!-- Top Header -->
    <header class="h-14 bg-vdsa-surface border-b border-vdsa-border px-4 flex items-center justify-between shrink-0 gap-4">
      <div class="flex items-center gap-3">
        <Button variant="ghost" size="sm" class="gap-1.5 text-xs text-slate-300 hover:text-white" @click="goBack">
          <ArrowLeft :size="15" /> Studio
        </Button>
        <span class="w-px h-5 bg-vdsa-border" />
        <div class="flex items-center gap-2">
          <span class="text-xs font-extrabold text-white">
            {{ isEdit ? `Chỉnh sửa bài học #${lessonId}` : 'Soạn thảo bài học mới' }}
          </span>
          <span v-if="lastSavedDraftTime" class="text-[11px] text-slate-400 flex items-center gap-1">
            <Check :size="12" class="text-emerald-400" /> Nháp: {{ lastSavedDraftTime }}
          </span>
        </div>
      </div>

      <!-- Center Tabs Switcher -->
      <nav class="hidden md:flex items-center bg-vdsa-bg-secondary border border-vdsa-border rounded-xl p-1 gap-1">
        <button
          type="button"
          class="px-3 py-1.5 rounded-lg text-xs font-bold transition-colors flex items-center gap-1.5 cursor-pointer"
          :class="activeTab === 'theory' ? 'bg-vdsa-accent text-white shadow' : 'text-slate-400 hover:text-white'"
          @click="activeTab = 'theory'"
        >
          <PenTool :size="13" /> 1. Lý thuyết & Nội dung
        </button>
        <button
          type="button"
          class="px-3 py-1.5 rounded-lg text-xs font-bold transition-colors flex items-center gap-1.5 cursor-pointer"
          :class="activeTab === 'simulations' ? 'bg-vdsa-accent text-white shadow' : 'text-slate-400 hover:text-white'"
          @click="activeTab = 'simulations'"
        >
          <Layers :size="13" /> 2. Mô phỏng ({{ form.selectedSimulations.length }})
        </button>
        <button
          type="button"
          class="px-3 py-1.5 rounded-lg text-xs font-bold transition-colors flex items-center gap-1.5 cursor-pointer"
          :class="activeTab === 'quiz' ? 'bg-vdsa-accent text-white shadow' : 'text-slate-400 hover:text-white'"
          @click="activeTab = 'quiz'"
        >
          <Puzzle :size="13" /> 3. Câu hỏi Quiz
        </button>
        <button
          type="button"
          class="px-3 py-1.5 rounded-lg text-xs font-bold transition-colors flex items-center gap-1.5 cursor-pointer"
          :class="activeTab === 'settings' ? 'bg-vdsa-accent text-white shadow' : 'text-slate-400 hover:text-white'"
          @click="activeTab = 'settings'"
        >
          <Settings :size="13" /> 4. Cấu hình
        </button>
      </nav>

      <!-- Right Actions: Preview & Save -->
      <div class="flex items-center gap-2">
        <Button variant="secondary" size="sm" class="gap-1.5 text-xs" @click="handlePreview">
          <Eye :size="14" /> Xem trước bài
        </Button>
        <Button variant="primary" size="sm" class="gap-1.5 text-xs font-bold" :loading="saving" @click="handleSave">
          <Save :size="14" /> {{ isEdit ? 'Lưu cập nhật' : 'Xuất bản bài học' }}
        </Button>
      </div>
    </header>

    <!-- Draft restore alert -->
    <div v-if="hasRestorableDraft" class="bg-amber-500/10 border-b border-amber-500/30 px-4 py-2 flex items-center justify-between shrink-0">
      <div class="flex items-center gap-2 text-xs text-amber-200">
        <Sparkles :size="14" class="text-amber-400" />
        <span>Phát hiện bản nháp tự lưu gần nhất của bạn. Khôi phục nội dung đang gõ dở?</span>
      </div>
      <div class="flex items-center gap-2">
        <Button size="sm" variant="primary" class="text-xs py-1 h-7" @click="restoreDraft">Khôi phục ngay</Button>
        <Button size="sm" variant="ghost" class="text-xs py-1 h-7 text-slate-400" @click="discardDraft">Bỏ qua</Button>
      </div>
    </div>

    <!-- Title Bar -->
    <div class="px-6 py-3 bg-[#0d1117] border-b border-vdsa-border shrink-0 flex items-center gap-3">
      <input
        v-model="form.title"
        type="text"
        placeholder="Nhập tiêu đề bài học (VD: Thuật toán Quick Sort & Phân tích Big-O)..."
        class="flex-1 bg-transparent border-none outline-none text-base sm:text-lg font-black text-white placeholder-slate-500"
      />
      <Badge v-if="form.isClassOnly" variant="success" class="text-[11px] shrink-0 font-bold">
        Nội bộ Lớp học
      </Badge>
    </div>

    <!-- Mobile Sub-nav -->
    <div class="md:hidden flex items-center justify-around bg-vdsa-surface border-b border-vdsa-border py-2 px-1 text-xs shrink-0">
      <button
        type="button"
        class="px-2.5 py-1 rounded-lg font-bold"
        :class="activeTab === 'theory' ? 'bg-vdsa-accent text-white' : 'text-slate-400'"
        @click="activeTab = 'theory'"
      >
        Lý thuyết
      </button>
      <button
        type="button"
        class="px-2.5 py-1 rounded-lg font-bold"
        :class="activeTab === 'simulations' ? 'bg-vdsa-accent text-white' : 'text-slate-400'"
        @click="activeTab = 'simulations'"
      >
        Mô phỏng ({{ form.selectedSimulations.length }})
      </button>
      <button
        type="button"
        class="px-2.5 py-1 rounded-lg font-bold"
        :class="activeTab === 'quiz' ? 'bg-vdsa-accent text-white' : 'text-slate-400'"
        @click="activeTab = 'quiz'"
      >
        Quiz
      </button>
      <button
        type="button"
        class="px-2.5 py-1 rounded-lg font-bold"
        :class="activeTab === 'settings' ? 'bg-vdsa-accent text-white' : 'text-slate-400'"
        @click="activeTab = 'settings'"
      >
        Cấu hình
      </button>
    </div>

    <!-- Main Workspace Tab Content (LE1: overflow-hidden wrapper, internal overflow-y-auto) -->
    <main class="flex-1 overflow-hidden">
      <div v-if="loading" class="p-8 space-y-4">
        <Skeleton height="50px" />
        <Skeleton height="350px" />
      </div>

      <template v-else>
        <!-- Tab 1: Theory Content -->
        <TheoryTab
          v-show="activeTab === 'theory'"
          ref="theoryTabRef"
          v-model="form.markdown"
          @template-applied="
            (tpl) => {
              if (tpl.title) form.title = tpl.title;
              if (tpl.description) form.description = tpl.description;
            }
          "
        />

        <!-- Tab 2: Simulations -->
        <SimulationTab
          v-show="activeTab === 'simulations'"
          v-model="form.selectedSimulations"
          :simulations="allSimulations"
          @insert-anchor="onInsertSimulationAnchor"
        />

        <!-- Tab 3: Quiz -->
        <QuizTab
          v-show="activeTab === 'quiz'"
          :lesson-id="lessonId"
          :lesson-title="form.title"
        />

        <!-- Tab 4: Settings -->
        <SettingsTab
          v-show="activeTab === 'settings'"
          v-model:topic-id="form.topicId"
          v-model:description="form.description"
          v-model:sort-order="form.sortOrder"
          v-model:status="form.status"
          v-model:is-class-only="form.isClassOnly"
          v-model:selected-class-id="form.selectedClassId"
          :topics="topics"
        />
      </template>
    </main>

    <!-- Modal Xem Trước Live Preview -->
    <Modal :open="previewModalOpen" :title="`Xem trước: ${form.title}`" size="lg" @close="previewModalOpen = false">
      <div class="space-y-6 max-h-[75vh] overflow-y-auto p-4 bg-[#090d16] rounded-xl text-slate-200">
        <div>
          <h1 class="text-2xl font-black text-white mb-2">{{ form.title }}</h1>
          <p v-if="form.description" class="text-xs text-slate-400 italic">{{ form.description }}</p>
        </div>

        <div v-if="form.selectedSimulations.length > 0" class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border">
          <span class="text-xs font-bold text-white block mb-2">⚡ Mô phỏng đính kèm ({{ form.selectedSimulations.length }}):</span>
          <div class="flex flex-wrap gap-2">
            <span v-for="k in form.selectedSimulations" :key="k" class="px-2.5 py-1 rounded bg-purple-500/20 text-purple-300 text-xs font-mono">
              {{ k }}
            </span>
          </div>
        </div>

        <!-- Rendered Theory -->
        <div class="prose-preview border-t border-slate-800 pt-4" v-html="parseMarkdownToHtml(form.markdown)" />
      </div>
    </Modal>
  </div>
</template>
