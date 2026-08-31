<script setup lang="ts">
import { computed, onMounted, onUnmounted, reactive, ref, watch } from 'vue';
import { useRoute, useRouter, onBeforeRouteLeave } from 'vue-router';
import {
  ArrowLeft,
  BookOpen,
  Check,
  CheckCircle2,
  ChevronDown,
  Code,
  Eye,
  ExternalLink,
  HelpCircle,
  Layers,
  PenTool,
  Plus,
  Puzzle,
  Save,
  Search,
  Settings,
  Sparkles,
  Trash2,
  X,
  Zap,
} from 'lucide-vue-next';

import * as lessonsApi from '@/api/lessons';
import type { Topic, LessonStatusValue } from '@/api/lessons';
import * as simulationsApi from '@/api/simulations';
import type { SimulationMetaDto } from '@/api/simulations';
import * as exercisesApi from '@/api/exercises';
import * as classesApi from '@/api/classes';
import type { ClassDto } from '@/api/types';
import { courseApi } from '@/services/courseApi';
import { useUiStore } from '@/stores/ui';
import { useAuthStore } from '@/stores/auth';
import { parseMarkdownToHtml } from '@/utils/markdownParser';
import { LESSON_TEMPLATES } from '@/data/lessonTemplates';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import Modal from '@/components/ui/Modal.vue';

// Sub-tabs / Sections
import TheoryTab from './editor-tabs/TheoryTab.vue';
import QuizTab, { type InlineQuestionItem } from './editor-tabs/QuizTab.vue';
import CodeLabTab, { type CodeLabFormState } from './editor-tabs/CodeLabTab.vue';

const route = useRoute();
const router = useRouter();
const ui = useUiStore();
const auth = useAuthStore();

// ── Route & State ──
const isEdit = computed(() => Boolean(route.params.id));
const lessonId = computed(() => (route.params.id ? Number(route.params.id) : null));

const loading = ref(true);
const saving = ref(false);
const isDirty = ref(false);
const initialSnapshot = ref<string>('');
const lastSavedDraftTime = ref<string | null>(null);
const hasRestorableDraft = ref(false);
const previewModalOpen = ref(false);
const previewTab = ref<'theory' | 'quiz' | 'codelab'>('theory');
const previewAnswers = reactive<Record<number, number>>({});
const simPickerModalOpen = ref(false);

const theoryTabRef = ref<InstanceType<typeof TheoryTab> | null>(null);


// Topics, Simulations & Classes Data
const topics = ref<Topic[]>([]);
const allSimulations = ref<SimulationMetaDto[]>([]);
const classes = ref<ClassDto[]>([]);
const simSearchQuery = ref('');

// Existing Exercise IDs (if editing existing lesson)
const existingQuizExerciseId = ref<number | null>(null);
const existingCodeExerciseId = ref<number | null>(null);

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
  quizQuestions: [] as InlineQuestionItem[],
  codeLab: {
    enabled: false,
    exerciseId: null,
    title: '',
    description: '',
    difficulty: 'Easy' as const,
    entryFunction: 'solve',
    durationMinutes: 20,
    maxScore: 100,
    starterCode: `/**
 * @param {any} input
 * @return {any}
 */
function solve(input) {
  // Viết mã nguồn giải thuật của bạn ở đây
  return input;
}`,
    solutionCode: `function solve(input) {
  // Code giải mẫu của Giảng viên
  return input;
}`,
    testCases: [
      { input: '[1, 2, 3]', expected: '[1, 2, 3]', isHidden: false },
      { input: '[5, 4, 3, 2, 1]', expected: '[5, 4, 3, 2, 1]', isHidden: false },
      { input: '[]', expected: '[]', isHidden: true },
    ],
  } as CodeLabFormState,
});

// Auto-save storage key
const draftStorageKey = computed(() => `dsa_lesson_draft_${isEdit.value ? lessonId.value : 'new'}`);

// ── Quality Score & Checklist ──
const qualityChecklist = computed(() => {
  const hasTitle = form.title.trim().length >= 3;
  const hasTheory = form.markdown.trim().length >= 50;
  const hasSim = form.selectedSimulations.length > 0;
  const hasQuiz = form.quizQuestions.some((q) => q.content.trim().length > 0);
  const hasCodeLab = form.codeLab.enabled && form.codeLab.testCases.length > 0;

  let score = 0;
  if (hasTitle) score += 25;
  if (hasTheory) score += 35;
  if (hasSim) score += 15;
  if (hasQuiz) score += 15;
  if (hasCodeLab) score += 10;

  return {
    score: Math.min(100, score),
    hasTitle,
    hasTheory,
    hasSim,
    hasQuiz,
    hasCodeLab,
    isReady: hasTitle && hasTheory,
  };
});

// Filtered simulations for sidebar picker
const filteredSimulations = computed(() => {
  if (!simSearchQuery.value.trim()) return allSimulations.value;
  const q = simSearchQuery.value.trim().toLowerCase();
  return allSimulations.value.filter(
    (s) =>
      s.title.toLowerCase().includes(q) ||
      s.key.toLowerCase().includes(q) ||
      s.dataStructure?.toLowerCase().includes(q) ||
      s.tags?.some((t) => t.toLowerCase().includes(q)),
  );
});

// ── Initial Data Load ──
onMounted(async () => {
  loading.value = true;
  try {
    const [topicList, simPage, classList] = await Promise.all([
      lessonsApi.fetchTopics(),
      simulationsApi.fetchSimulations(),
      classesApi.fetchClasses().catch(() => []),
    ]);
    topics.value = topicList;
    allSimulations.value = simPage.items || [];
    classes.value = classList || [];

    if (route.query.topicId) {
      form.topicId = Number(route.query.topicId);
    } else if (topics.value.length > 0) {
      form.topicId = topics.value[0].id;
    }

    if (isEdit.value && lessonId.value) {
      try {
        const [lesson, attachedExercises] = await Promise.all([
          lessonsApi.fetchLesson(lessonId.value),
          exercisesApi.fetchExercises({ lessonId: lessonId.value }).catch(() => []),
        ]);

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

        // Parse attached MCQ and CodeLab exercises
        const mcqEx = attachedExercises.find((e) => e.type?.toUpperCase() === 'MCQ');
        if (mcqEx) {
          existingQuizExerciseId.value = mcqEx.id;
          try {
            const mcqDetail = await exercisesApi.fetchExercise(mcqEx.id);
            if (Array.isArray(mcqDetail.questions) && mcqDetail.questions.length > 0) {
              form.quizQuestions = mcqDetail.questions.map((q: any) => {
                let correctIdx = 0;
                if (q.answerJson) {
                  try {
                    const arr = JSON.parse(q.answerJson);
                    if (Array.isArray(arr) && arr.length > 0) correctIdx = Number(arr[0]) || 0;
                  } catch {
                    // ignore
                  }
                }
                const opts = Array.isArray(q.options) && q.options.length > 0 ? q.options.map(String) : ['', '', '', ''];
                while (opts.length < 4) opts.push('');
                return {
                  id: q.id,
                  content: q.content || '',
                  options: opts,
                  correctIndex: correctIdx >= 0 && correctIdx < opts.length ? correctIdx : 0,
                  explanation: q.explanation || '',
                  points: q.points || 2,
                };
              });
            }
          } catch {
            // ignore
          }
        }

        const codeEx = attachedExercises.find((e) => e.type?.toUpperCase() === 'CODE' || e.stage === 3);
        if (codeEx) {
          existingCodeExerciseId.value = codeEx.id;
          form.codeLab.enabled = true;
          form.codeLab.exerciseId = codeEx.id;
          form.codeLab.title = codeEx.title;
          form.codeLab.description = codeEx.description || '';
          form.codeLab.durationMinutes = codeEx.durationMinutes || 20;
          form.codeLab.maxScore = codeEx.maxScore || 100;

          try {
            const codeDetail = await exercisesApi.fetchExercise(codeEx.id);
            if ((codeDetail as any).configJson) {
              const parsed = JSON.parse((codeDetail as any).configJson);
              if (parsed.starterCode) form.codeLab.starterCode = parsed.starterCode;
              if (parsed.solutionCode) form.codeLab.solutionCode = parsed.solutionCode;
              if (parsed.entryFunction) form.codeLab.entryFunction = parsed.entryFunction;
              if (parsed.difficulty) form.codeLab.difficulty = parsed.difficulty;
              if (Array.isArray(parsed.testCases) && parsed.testCases.length > 0) {
                form.codeLab.testCases = parsed.testCases.map((tc: any) => ({
                  input: tc.input || '',
                  expected: tc.expected || '',
                  isHidden: Boolean(tc.isHidden),
                }));
              }
            }
          } catch {
            // ignore
          }
        }
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
        form.title = 'Thuật toán & Cấu trúc Dữ liệu';
        form.description = 'Nắm vững nguyên lý hoạt động, phân tích độ phức tạp thời gian và thực hành trực quan.';
        form.markdown = parseMarkdownToHtml(LESSON_TEMPLATES[0].content);
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

  window.addEventListener('beforeunload', handleBeforeUnload);
  window.addEventListener('keydown', onKeydown);
});

onUnmounted(() => {
  window.removeEventListener('beforeunload', handleBeforeUnload);
  window.removeEventListener('keydown', onKeydown);
  if (saveTimer) clearTimeout(saveTimer);
});

function handleBeforeUnload(e: BeforeUnloadEvent): void {
  const isActuallyDirty = initialSnapshot.value && JSON.stringify(form) !== initialSnapshot.value;
  if (isActuallyDirty && !isNavigatingAwayAfterSave.value) {
    e.preventDefault();
    e.returnValue = '';
  }
}

function onKeydown(e: KeyboardEvent): void {
  if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's') {
    e.preventDefault();
    if (!saving.value) {
      void handleSave();
    }
  }
}

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
        quizQuestions: form.quizQuestions,
        codeLab: form.codeLab,
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
  () => [form.title, form.description, form.topicId, form.markdown, form.selectedSimulations, form.quizQuestions, form.codeLab],
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
      if (Array.isArray(draft.quizQuestions)) form.quizQuestions = draft.quizQuestions;
      if (draft.codeLab) form.codeLab = draft.codeLab;
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

// Simulation Quick Toggle
function toggleSimulation(key: string): void {
  const idx = form.selectedSimulations.indexOf(key);
  if (idx >= 0) {
    form.selectedSimulations.splice(idx, 1);
    ui.showToast(`Đã gỡ mô phỏng "${key}"`, 'info');
  } else {
    form.selectedSimulations.push(key);
    ui.showToast(`Đã gắn mô phỏng "${key}" vào bài học!`, 'success');
  }
}

function removeSimulation(key: string): void {
  form.selectedSimulations = form.selectedSimulations.filter((k) => k !== key);
}

// ── Smooth Scroll to Section ──
function scrollToSection(sectionId: string): void {
  const el = document.getElementById(sectionId);
  if (el) {
    el.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }
}

// ── Xem trước bài giảng ──
function handlePreview(): void {
  previewTab.value = 'theory';
  previewModalOpen.value = true;
}

// ── Lưu & Xuất bản 4-in-1 Transactional Save ──
async function handleSave(): Promise<void> {
  if (form.title.trim().length < 3) {
    ui.showToast('Vui lòng nhập tiêu đề bài học (tối thiểu 3 ký tự).', 'warning');
    scrollToSection('section-header');
    return;
  }
  if (!form.markdown.trim()) {
    ui.showToast('Vui lòng soạn thảo nội dung bài học trước khi lưu.', 'warning');
    scrollToSection('section-theory');
    return;
  }

  // Validation CodeLab
  if (form.codeLab.enabled) {
    if (!form.codeLab.testCases || form.codeLab.testCases.length === 0) {
      ui.showToast('Phần Code Lab đang bật nhưng chưa có Test case nào. Vui lòng thêm ít nhất 1 Test case hoặc tắt Code Lab.', 'warning');
      scrollToSection('section-codelab');
      return;
    }
    if (!form.codeLab.starterCode?.trim()) {
      ui.showToast('Vui lòng nhập Starter Code cho bài tập Code Lab.', 'warning');
      scrollToSection('section-codelab');
      return;
    }
  }

  // Validation Quiz
  const activeQuiz = form.quizQuestions.filter((q) => q.content.trim().length > 0);
  for (let i = 0; i < activeQuiz.length; i++) {
    const validOpts = activeQuiz[i].options.filter((o) => o.trim().length > 0);
    if (validOpts.length < 2) {
      ui.showToast(`Câu hỏi Quiz #${i + 1} cần có ít nhất 2 lựa chọn đáp án.`, 'warning');
      scrollToSection('section-quiz');
      return;
    }
  }


  saving.value = true;
  let hasSubPartError = false;
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

    // 1. Lưu Lesson
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
        } catch (courseErr: any) {
          hasSubPartError = true;
          console.error('Lỗi khi gắn bài vào lộ trình:', courseErr);
          ui.showToast(`Lỗi khi gắn bài vào lộ trình: ${courseErr?.message || 'Không xác định'}`, 'error');
        }
      }
    }

    // 2. Lưu / Đồng bộ Quiz questions (MCQ)
    if (targetLessonId) {
      const validQuestions = form.quizQuestions
        .filter((q) => q.content.trim().length > 0)
        .map((q, idx) => ({
          content: q.content.trim(),
          type: 'Single' as const,
          options: q.options.map((o) => o.trim()).filter(Boolean),
          answerJson: JSON.stringify([q.correctIndex]),
          explanation: q.explanation.trim() || undefined,
          points: q.points || 2,
          sortOrder: idx + 1,
        }));

      if (validQuestions.length > 0) {
        try {
          if (!existingQuizExerciseId.value) {
            const existingList = await exercisesApi.fetchExercises({ lessonId: targetLessonId });
            const foundMcq = existingList.find((e) => e.type === 'MCQ');
            if (foundMcq) existingQuizExerciseId.value = foundMcq.id;
          }

          if (existingQuizExerciseId.value) {
            await exercisesApi.updateExercise(existingQuizExerciseId.value, {
              title: `Quiz: ${form.title.trim()}`,
              description: 'Trắc nghiệm củng cố kiến thức bài học.',
              type: 'Mcq',
              durationMinutes: 10,
              maxScore: validQuestions.reduce((sum, q) => sum + (q.points || 2), 0),
              status: 'Active',
              questions: validQuestions,
            });
          } else {
            const createdQuiz = await exercisesApi.createExercise({
              lessonId: targetLessonId,
              title: `Quiz: ${form.title.trim()}`,
              description: 'Trắc nghiệm củng cố kiến thức bài học.',
              type: 'Mcq',
              durationMinutes: 10,
              maxScore: validQuestions.reduce((sum, q) => sum + (q.points || 2), 0),
              status: 'Active',
              questions: validQuestions,
            });
            if (createdQuiz?.id) existingQuizExerciseId.value = createdQuiz.id;
          }
        } catch (quizErr: any) {
          hasSubPartError = true;
          console.error('Lỗi khi lưu Quiz:', quizErr);
          ui.showToast(`Lỗi khi lưu phần Trắc nghiệm: ${quizErr?.message || 'Không xác định'}`, 'error');
        }
      } else if (existingQuizExerciseId.value) {
        // Vô hiệu hóa (deactivate về Draft) khi không có câu hỏi để giữ nguyên data khi bật lại
        try {
          await exercisesApi.updateExercise(existingQuizExerciseId.value, { status: 'Draft' });
        } catch (deactQuizErr) {
          console.warn('Không thể vô hiệu hóa bài quiz cũ:', deactQuizErr);
        }
      }
    }

    // 3. Lưu / Đồng bộ Code Lab (Code Exercise)
    if (targetLessonId) {
      if (form.codeLab.enabled) {
        const configJson = JSON.stringify({
          starterCode: form.codeLab.starterCode,
          solutionCode: form.codeLab.solutionCode,
          entryFunction: form.codeLab.entryFunction,
          difficulty: form.codeLab.difficulty,
          testCases: form.codeLab.testCases,
        });

        try {
          if (!existingCodeExerciseId.value) {
            const existingList = await exercisesApi.fetchExercises({ lessonId: targetLessonId });
            const foundCode = existingList.find((e) => e.type === 'CODE');
            if (foundCode) existingCodeExerciseId.value = foundCode.id;
          }

          if (existingCodeExerciseId.value) {
            await exercisesApi.updateExercise(existingCodeExerciseId.value, {
              title: form.codeLab.title.trim() || `Thực hành: ${form.title.trim()}`,
              description: form.codeLab.description.trim() || undefined,
              type: 'Code',
              stage: 3,
              configJson,
              durationMinutes: form.codeLab.durationMinutes || 20,
              maxScore: form.codeLab.maxScore || 100,
              status: 'Active',
            });
          } else {
            const createdCode = await exercisesApi.createExercise({
              lessonId: targetLessonId,
              title: form.codeLab.title.trim() || `Thực hành: ${form.title.trim()}`,
              description: form.codeLab.description.trim() || undefined,
              type: 'Code',
              stage: 3,
              configJson,
              durationMinutes: form.codeLab.durationMinutes || 20,
              maxScore: form.codeLab.maxScore || 100,
              status: 'Active',
            });
            if (createdCode?.id) existingCodeExerciseId.value = createdCode.id;
          }
        } catch (codeErr: any) {
          hasSubPartError = true;
          console.error('Lỗi khi lưu Code Lab:', codeErr);
          ui.showToast(`Lỗi khi lưu phần Code Lab: ${codeErr?.message || 'Không xác định'}`, 'error');
        }
      } else if (existingCodeExerciseId.value) {
        // Vô hiệu hóa (deactivate về Draft) khi tắt toggle để bảo tồn dữ liệu
        try {
          await exercisesApi.updateExercise(existingCodeExerciseId.value, { status: 'Draft' });
        } catch (deactCodeErr) {
          console.warn('Không thể vô hiệu hóa Code Lab cũ:', deactCodeErr);
        }
      }
    }

    // 4. Gán vào lớp học nếu chọn classId
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

    if (hasSubPartError) {
      ui.showToast('Đã lưu lý thuyết bài học, nhưng có lỗi ở phần Trắc nghiệm/Code Lab. Vui lòng kiểm tra lại trước khi thoát!', 'warning');
      return;
    }

    localStorage.removeItem(draftStorageKey.value);
    isDirty.value = false;
    isNavigatingAwayAfterSave.value = true;

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
  <div class="admin-lesson-studio flex flex-col h-[calc(100vh-var(--app-header-h,68px))] bg-[#090d16] text-white overflow-hidden">
    <!-- ══════════════════════════════════════════════════════════════════
         TOP HEADER BAR (CLEAN, MINIMAL, MODERN)
         ══════════════════════════════════════════════════════════════════ -->
    <header class="h-14 bg-vdsa-surface border-b border-vdsa-border px-4 sm:px-6 flex items-center justify-between shrink-0 gap-4 z-20">
      <!-- Left: Back & Breadcrumb -->
      <div class="flex items-center gap-3 min-w-0">
        <Button variant="ghost" size="sm" class="gap-1.5 text-xs text-slate-300 hover:text-white shrink-0" @click="goBack">
          <ArrowLeft :size="15" /> Studio
        </Button>
        <span class="w-px h-5 bg-vdsa-border shrink-0" />
        <div class="flex items-center gap-2 truncate">
          <span class="text-xs font-bold text-slate-200 truncate">
            {{ isEdit ? `Chỉnh sửa bài học #${lessonId}` : 'Soạn bài học mới' }}
          </span>
          <span v-if="lastSavedDraftTime" class="text-[11px] text-slate-400 flex items-center gap-1 shrink-0">
            <Check :size="12" class="text-emerald-400" /> Nháp: {{ lastSavedDraftTime }}
          </span>
        </div>
      </div>

      <!-- Center Jump Links -->
      <nav class="hidden md:flex items-center bg-slate-900/90 border border-slate-800 rounded-xl p-1 gap-1">
        <button
          type="button"
          class="px-2.5 py-1 rounded-lg text-xs font-semibold text-slate-300 hover:text-white hover:bg-slate-800 transition-colors flex items-center gap-1.5"
          @click="scrollToSection('section-theory')"
        >
          <PenTool :size="12" class="text-purple-400" /> Lý thuyết
        </button>
        <button
          type="button"
          class="px-2.5 py-1 rounded-lg text-xs font-semibold text-slate-300 hover:text-white hover:bg-slate-800 transition-colors flex items-center gap-1.5"
          @click="scrollToSection('section-simulations')"
        >
          <Layers :size="12" class="text-sky-400" /> Mô phỏng ({{ form.selectedSimulations.length }})
        </button>
        <button
          type="button"
          class="px-2.5 py-1 rounded-lg text-xs font-semibold text-slate-300 hover:text-white hover:bg-slate-800 transition-colors flex items-center gap-1.5"
          @click="scrollToSection('section-quiz')"
        >
          <Puzzle :size="12" class="text-amber-400" /> Quiz ({{ form.quizQuestions.length }})
        </button>
        <button
          type="button"
          class="px-2.5 py-1 rounded-lg text-xs font-semibold transition-colors flex items-center gap-1.5"
          :class="form.codeLab.enabled ? 'text-emerald-400 font-bold hover:bg-slate-800' : 'text-slate-400 hover:text-white hover:bg-slate-800'"
          @click="scrollToSection('section-codelab')"
        >
          <Code :size="12" class="text-emerald-400" /> Code Lab {{ form.codeLab.enabled ? '✓' : '' }}
        </button>
      </nav>

      <!-- Right Actions: Preview & Save -->
      <div class="flex items-center gap-2 shrink-0">
        <Button variant="secondary" size="sm" class="gap-1.5 text-xs" @click="handlePreview">
          <Eye :size="14" /> Xem trước
        </Button>
        <Button
          variant="primary"
          size="sm"
          class="gap-1.5 text-xs font-extrabold bg-purple-600 hover:bg-purple-500 shadow-md shadow-purple-900/30"
          :loading="saving"
          @click="handleSave"
        >
          <Save :size="14" /> {{ isEdit ? 'Lưu cập nhật' : 'Xuất bản bài học' }}
        </Button>
      </div>
    </header>

    <!-- Draft restore alert -->
    <div v-if="hasRestorableDraft" class="bg-amber-500/10 border-b border-amber-500/30 px-6 py-2 flex items-center justify-between shrink-0">
      <div class="flex items-center gap-2 text-xs text-amber-200">
        <Sparkles :size="14" class="text-amber-400" />
        <span>Phát hiện bản nháp tự lưu gần nhất của bạn. Khôi phục nội dung đang gõ dở?</span>
      </div>
      <div class="flex items-center gap-2">
        <Button size="sm" variant="primary" class="text-xs py-1 h-7" @click="restoreDraft">Khôi phục ngay</Button>
        <Button size="sm" variant="ghost" class="text-xs py-1 h-7 text-slate-400" @click="discardDraft">Bỏ qua</Button>
      </div>
    </div>

    <!-- ══════════════════════════════════════════════════════════════════
         MAIN 2-COLUMN STUDIO WORKSPACE (CANVAS + SIDEBAR INSPECTOR)
         ══════════════════════════════════════════════════════════════════ -->
    <div class="flex-1 flex overflow-hidden">
      <!-- ──────────────────────────────────────────────────────────────
           LEFT / CENTER: MAIN DOCUMENT CANVAS (70% - NOTION STYLE)
           ────────────────────────────────────────────────────────────── -->
      <main class="flex-1 overflow-y-auto p-4 sm:p-8 space-y-10 custom-scrollbar">
        <div v-if="loading" class="max-w-4xl mx-auto space-y-6">
          <Skeleton height="60px" />
          <Skeleton height="400px" />
        </div>

        <div v-else class="max-w-4xl mx-auto space-y-10">
          <!-- ── NOTION-STYLE HERO TITLE & DESCRIPTION ── -->
          <div id="section-header" class="space-y-3 pb-6 border-b border-slate-800/80">
            <input
              v-model="form.title"
              type="text"
              placeholder="Nhập tiêu đề bài học (VD: Thuật toán Quick Sort & Phân tích Big-O)..."
              class="w-full bg-transparent border-none outline-none text-2xl sm:text-4xl font-black text-white placeholder-slate-600 tracking-tight leading-tight"
            />
            <textarea
              v-model="form.description"
              rows="2"
              placeholder="Mô tả tóm tắt mục tiêu bài học, kiến thức đạt được và thời lượng dự kiến..."
              class="w-full bg-transparent border-none outline-none text-sm text-slate-400 placeholder-slate-600 resize-none leading-relaxed italic"
            ></textarea>

            <!-- Metadata tags -->
            <div class="flex items-center gap-2 flex-wrap pt-2">
              <Badge variant="secondary" class="text-xs font-mono">
                Chương: {{ topics.find(t => t.id === Number(form.topicId))?.name || 'Mặc định' }}
              </Badge>
              <Badge v-if="form.isClassOnly" variant="success" class="text-xs font-bold">
                Nội bộ lớp học
              </Badge>
              <Badge v-else variant="primary" class="text-xs">
                Công khai toàn hệ thống
              </Badge>
              <Badge v-if="form.selectedSimulations.length > 0" variant="secondary" class="text-xs text-sky-300">
                ⚡ {{ form.selectedSimulations.length }} Mô phỏng
              </Badge>
              <Badge v-if="form.quizQuestions.length > 0" variant="secondary" class="text-xs text-amber-300">
                ❓ {{ form.quizQuestions.length }} Câu hỏi Quiz
              </Badge>
              <Badge v-if="form.codeLab.enabled" variant="secondary" class="text-xs text-emerald-300">
                💻 Code Lab ({{ form.codeLab.testCases.length }} testcases)
              </Badge>
            </div>
          </div>

          <!-- ── SECTION 1: LÝ THUYẾT & BÀI GIẢNG ── -->
          <section id="section-theory" class="space-y-4">
            <div class="flex items-center justify-between border-b border-slate-800 pb-2">
              <h2 class="text-base font-extrabold text-white flex items-center gap-2">
                <PenTool :size="17" class="text-purple-400" />
                1. Nội dung Lý thuyết & Bài giảng
              </h2>
              <span class="text-xs text-slate-400">Hỗ trợ soạn thảo trực quan & AI Format</span>
            </div>

            <TheoryTab
              ref="theoryTabRef"
              v-model="form.markdown"
              @template-applied="
                (tpl) => {
                  if (tpl.title) form.title = tpl.title;
                  if (tpl.description) form.description = tpl.description;
                }
              "
            />
          </section>

          <!-- ── SECTION 2: MÔ PHỎNG TRỰC QUAN ĐÍNH KÈM ── -->
          <section id="section-simulations" class="space-y-4">
            <div class="flex items-center justify-between border-b border-slate-800 pb-2">
              <div>
                <h2 class="text-base font-extrabold text-white flex items-center gap-2">
                  <Layers :size="17" class="text-sky-400" />
                  2. Mô phỏng Thuật toán Trực quan ({{ form.selectedSimulations.length }})
                </h2>
                <p class="text-xs text-slate-400 mt-0.5">
                  Học viên sẽ được tương tác trực tiếp với mô phỏng chạy từng bước của các thuật toán đính kèm.
                </p>
              </div>

              <Button size="sm" variant="secondary" class="text-xs gap-1.5" @click="simPickerModalOpen = true">
                <Plus :size="13" /> Gắn thêm Mô phỏng
              </Button>
            </div>

            <!-- Attached Simulations List -->
            <div v-if="form.selectedSimulations.length === 0" class="p-6 rounded-2xl bg-vdsa-surface/50 border border-dashed border-slate-800 text-center space-y-2">
              <Layers :size="28" class="mx-auto text-slate-600" />
              <p class="text-xs font-semibold text-slate-300">Chưa gắn mô phỏng trực quan nào vào bài học</p>
              <p class="text-[11px] text-slate-500">
                Hãy chọn nhanh các thuật toán có sẵn trong thư viện 44+ thuật toán ở cột bên phải hoặc bấm nút bên dưới.
              </p>
              <Button size="sm" variant="secondary" class="text-xs mt-2" @click="simPickerModalOpen = true">
                <Plus :size="13" /> Mở thư viện thuật toán
              </Button>
            </div>

            <div v-else class="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <div
                v-for="simKey in form.selectedSimulations"
                :key="simKey"
                class="p-3.5 rounded-xl bg-vdsa-surface border border-sky-500/30 flex items-center justify-between gap-3 shadow-md"
              >
                <div class="flex items-center gap-3 min-w-0">
                  <div class="w-9 h-9 rounded-lg bg-sky-500/20 text-sky-400 flex items-center justify-center shrink-0">
                    <Zap :size="18" />
                  </div>
                  <div class="min-w-0">
                    <span class="text-xs font-bold text-white block truncate">
                      {{ allSimulations.find(s => s.key === simKey)?.title || simKey }}
                    </span>
                    <span class="text-[11px] font-mono text-sky-300/80">{{ simKey }}</span>
                  </div>
                </div>

                <div class="flex items-center gap-1 shrink-0">
                  <a
                    :href="`/simulator/${simKey}`"
                    target="_blank"
                    class="p-1.5 rounded-lg text-slate-400 hover:text-white hover:bg-slate-800 transition-colors"
                    title="Mở chạy thử mô phỏng"
                  >
                    <ExternalLink :size="14" />
                  </a>
                  <button
                    type="button"
                    class="p-1.5 rounded-lg text-slate-400 hover:text-rose-400 hover:bg-rose-500/10 transition-colors"
                    title="Gỡ mô phỏng này"
                    @click="removeSimulation(simKey)"
                  >
                    <Trash2 :size="14" />
                  </button>
                </div>
              </div>
            </div>
          </section>

          <!-- ── SECTION 3: MINI-QUIZ KIỂM TRA KIẾN THỨC ── -->
          <section id="section-quiz" class="space-y-4">
            <div class="flex items-center justify-between border-b border-slate-800 pb-2">
              <div>
                <h2 class="text-base font-extrabold text-white flex items-center gap-2">
                  <Puzzle :size="17" class="text-amber-400" />
                  3. Câu hỏi Trắc nghiệm Củng cố (Mini-Quiz)
                </h2>
                <p class="text-xs text-slate-400 mt-0.5">
                  Kiểm tra mức độ hiểu bài của học viên ngay sau phần lý thuyết. Tự động lưu kèm bài học.
                </p>
              </div>
            </div>

            <QuizTab
              v-model="form.quizQuestions"
              :lesson-id="lessonId"
              :lesson-title="form.title"
            />
          </section>

          <!-- ── SECTION 4: BÀI TẬP LẬP TRÌNH CODE LAB ── -->
          <section id="section-codelab" class="space-y-4">
            <div class="flex items-center justify-between border-b border-slate-800 pb-2">
              <div>
                <h2 class="text-base font-extrabold text-white flex items-center gap-2">
                  <Code :size="17" class="text-emerald-400" />
                  4. Bài tập Lập trình Thực hành (Code Lab)
                </h2>
                <p class="text-xs text-slate-400 mt-0.5">
                  Cài đặt thuật toán, chạy thử nghiệm trên Solution Playground và tự động chấm điểm testcases.
                </p>
              </div>
            </div>

            <CodeLabTab
              v-model="form.codeLab"
              :lesson-title="form.title"
            />
          </section>
        </div>
      </main>

      <!-- ──────────────────────────────────────────────────────────────
           RIGHT SIDEBAR: SETTINGS & QUICK PICKER (30% - INSPECTOR)
           ────────────────────────────────────────────────────────────── -->
      <aside class="w-80 bg-[#0c101c] border-l border-vdsa-border p-5 space-y-6 overflow-y-auto shrink-0 hidden xl:block custom-scrollbar">
        <!-- Quality Score Card -->
        <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border space-y-3">
          <div class="flex items-center justify-between">
            <span class="text-xs font-extrabold text-white uppercase tracking-wider">Tiến độ hoàn thiện</span>
            <Badge :variant="qualityChecklist.isReady ? 'success' : 'warning'" class="text-[11px] font-bold">
              {{ qualityChecklist.score }}% Đạt
            </Badge>
          </div>

          <div class="w-full bg-slate-800 rounded-full h-2 overflow-hidden">
            <div
              class="h-full rounded-full transition-all duration-300"
              :class="qualityChecklist.score >= 80 ? 'bg-emerald-500' : 'bg-amber-500'"
              :style="{ width: `${qualityChecklist.score}%` }"
            ></div>
          </div>

          <div class="space-y-1.5 text-xs text-slate-300">
            <div class="flex items-center gap-2">
              <CheckCircle2 v-if="qualityChecklist.hasTitle" :size="14" class="text-emerald-400 shrink-0" />
              <div v-else class="w-3.5 h-3.5 rounded-full border border-slate-600 shrink-0" />
              <span :class="{ 'text-slate-400 line-through': qualityChecklist.hasTitle }">Tiêu đề bài học</span>
            </div>
            <div class="flex items-center gap-2">
              <CheckCircle2 v-if="qualityChecklist.hasTheory" :size="14" class="text-emerald-400 shrink-0" />
              <div v-else class="w-3.5 h-3.5 rounded-full border border-slate-600 shrink-0" />
              <span :class="{ 'text-slate-400 line-through': qualityChecklist.hasTheory }">Nội dung lý thuyết (≥ 50 từ)</span>
            </div>
            <div class="flex items-center gap-2">
              <CheckCircle2 v-if="qualityChecklist.hasSim" :size="14" class="text-emerald-400 shrink-0" />
              <div v-else class="w-3.5 h-3.5 rounded-full border border-slate-600 shrink-0" />
              <span :class="{ 'text-slate-400 line-through': qualityChecklist.hasSim }">Gắn mô phỏng thuật toán</span>
            </div>
            <div class="flex items-center gap-2">
              <CheckCircle2 v-if="qualityChecklist.hasQuiz" :size="14" class="text-emerald-400 shrink-0" />
              <div v-else class="w-3.5 h-3.5 rounded-full border border-slate-600 shrink-0" />
              <span :class="{ 'text-slate-400 line-through': qualityChecklist.hasQuiz }">Câu hỏi trắc nghiệm Quiz</span>
            </div>
            <div class="flex items-center gap-2">
              <CheckCircle2 v-if="qualityChecklist.hasCodeLab" :size="14" class="text-emerald-400 shrink-0" />
              <div v-else class="w-3.5 h-3.5 rounded-full border border-slate-600 shrink-0" />
              <span :class="{ 'text-slate-400 line-through': qualityChecklist.hasCodeLab }">Bài tập Code Lab thực hành</span>
            </div>
          </div>
        </div>

        <!-- ── Settings Form ── -->
        <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border space-y-4">
          <h3 class="text-xs font-extrabold text-white uppercase tracking-wider flex items-center gap-1.5">
            <Settings :size="14" class="text-purple-400" /> Cấu hình & Phân quyền
          </h3>

          <div>
            <label class="block text-xs font-bold text-slate-300 mb-1">Chương / Chủ đề <span class="text-rose-400">*</span></label>
            <select
              v-model="form.topicId"
              class="w-full h-9 rounded-lg border border-slate-700 bg-slate-900 px-3 text-xs text-white focus:outline-none focus:ring-1 focus:ring-purple-500"
            >
              <option v-for="t in topics" :key="t.id" :value="t.id">
                {{ t.name }}
              </option>
            </select>
          </div>

          <div>
            <label class="block text-xs font-bold text-slate-300 mb-1">Thứ tự hiển thị (Sort Order)</label>
            <input
              v-model.number="form.sortOrder"
              type="number"
              min="1"
              max="999"
              class="w-full h-9 rounded-lg border border-slate-700 bg-slate-900 px-3 text-xs text-white focus:outline-none focus:ring-1 focus:ring-purple-500"
            />
          </div>

          <!-- Scope: Public vs Class Only -->
          <div class="space-y-2 pt-2 border-t border-slate-800">
            <label class="block text-xs font-bold text-slate-300">Phạm vi phát hành</label>
            <div class="space-y-1.5">
              <label class="flex items-center gap-2 p-2 rounded-lg border border-slate-800 hover:border-slate-700 cursor-pointer text-xs" :class="{ 'bg-purple-950/20 border-purple-500/40': !form.isClassOnly }">
                <input
                  v-model="form.isClassOnly"
                  type="radio"
                  :value="false"
                  class="text-purple-600 focus:ring-0"
                />
                <span class="text-white font-medium">Toàn hệ thống (Công khai)</span>
              </label>

              <label class="flex items-center gap-2 p-2 rounded-lg border border-slate-800 hover:border-slate-700 cursor-pointer text-xs" :class="{ 'bg-purple-950/20 border-purple-500/40': form.isClassOnly }">
                <input
                  v-model="form.isClassOnly"
                  type="radio"
                  :value="true"
                  class="text-purple-600 focus:ring-0"
                />
                <span class="text-white font-medium">Nội bộ Lớp học</span>
              </label>
            </div>
          </div>

          <!-- Select Class if Class Only -->
          <div v-if="form.isClassOnly" class="space-y-1.5 pt-1">
            <label class="block text-xs font-bold text-slate-300">Chọn lớp nhận bài học</label>
            <select
              v-model="form.selectedClassId"
              class="w-full h-9 rounded-lg border border-emerald-500/40 bg-slate-900 px-3 text-xs text-white focus:outline-none"
            >
              <option :value="null">-- Chọn lớp học --</option>
              <option v-for="c in classes" :key="c.id" :value="c.id">
                🎓 {{ c.name }}
              </option>
            </select>
          </div>
        </div>

        <!-- ── Quick Algorithm Simulator Picker ── -->
        <div class="p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border space-y-3">
          <div class="flex items-center justify-between">
            <h3 class="text-xs font-extrabold text-sky-400 uppercase tracking-wider flex items-center gap-1.5">
              <Zap :size="14" class="text-sky-400" /> Thư viện Mô phỏng
            </h3>
            <span class="text-[11px] text-slate-400 font-mono">{{ form.selectedSimulations.length }} đã gắn</span>
          </div>

          <div class="relative">
            <Search :size="13" class="absolute left-2.5 top-2.5 text-slate-500" />
            <input
              v-model="simSearchQuery"
              type="text"
              placeholder="Tìm thuật toán (Bubble, BST...)"
              class="w-full h-8 pl-8 pr-2.5 rounded-lg bg-slate-900 border border-slate-800 text-xs text-white placeholder-slate-500 focus:outline-none focus:border-sky-500"
            />
          </div>

          <div class="space-y-1 max-h-60 overflow-y-auto pr-1 custom-scrollbar text-xs">
            <button
              v-for="sim in filteredSimulations.slice(0, 20)"
              :key="sim.key"
              type="button"
              class="w-full p-2 rounded-lg text-left flex items-center justify-between gap-2 transition-colors"
              :class="form.selectedSimulations.includes(sim.key) ? 'bg-sky-500/20 border border-sky-500/40 text-sky-200' : 'bg-slate-900/60 hover:bg-slate-800 text-slate-300'"
              @click="toggleSimulation(sim.key)"
            >
              <div class="truncate">
                <span class="font-bold block truncate">{{ sim.title }}</span>
                <span class="text-[10px] text-slate-400 font-mono">{{ sim.key }}</span>
              </div>
              <Check v-if="form.selectedSimulations.includes(sim.key)" :size="14" class="text-sky-400 shrink-0" />
              <Plus v-else :size="13" class="text-slate-500 shrink-0" />
            </button>
          </div>
        </div>
      </aside>
    </div>

    <!-- ═══ MODAL THƯ VIỆN MÔ PHỎNG CHI TIẾT ═══ -->
    <Modal :open="simPickerModalOpen" title="Kho Mô phỏng Thuật toán Trực quan" size="lg" @close="simPickerModalOpen = false">
      <div class="space-y-4 max-h-[70vh] overflow-y-auto p-4 bg-[#090d16] rounded-xl text-slate-200">
        <div class="relative">
          <Search :size="15" class="absolute left-3 top-3 text-slate-500" />
          <input
            v-model="simSearchQuery"
            type="text"
            placeholder="Tìm kiếm theo tên thuật toán, cấu trúc dữ liệu hoặc tag..."
            class="w-full h-10 pl-10 pr-3 rounded-xl bg-slate-900 border border-slate-800 text-xs text-white focus:outline-none focus:border-sky-500"
          />
        </div>

        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
          <div
            v-for="sim in filteredSimulations"
            :key="sim.key"
            class="p-3 rounded-xl border flex items-center justify-between gap-3 cursor-pointer transition-all"
            :class="form.selectedSimulations.includes(sim.key) ? 'bg-sky-950/40 border-sky-500/60 ring-1 ring-sky-500/30' : 'bg-slate-900/60 border-slate-800 hover:border-slate-700'"
            @click="toggleSimulation(sim.key)"
          >
            <div class="min-w-0">
              <span class="text-xs font-bold text-white block truncate">{{ sim.title }}</span>
              <span class="text-[11px] font-mono text-sky-400">{{ sim.key }}</span>
              <p class="text-[11px] text-slate-400 line-clamp-1 mt-0.5">{{ sim.dataStructure || sim.category }}</p>
            </div>
            <Button
              size="sm"
              :variant="form.selectedSimulations.includes(sim.key) ? 'primary' : 'secondary'"
              class="h-7 text-xs shrink-0"
            >
              {{ form.selectedSimulations.includes(sim.key) ? 'Đã gắn ✓' : '+ Gắn' }}
            </Button>
          </div>
        </div>

        <div class="flex justify-end pt-2 border-t border-slate-800">
          <Button variant="primary" size="sm" @click="simPickerModalOpen = false">Hoàn tất</Button>
        </div>
      </div>
    </Modal>

    <!-- ═══ MODAL XEM TRƯỚC LIVE PREVIEW TƯƠNG TÁC ═══ -->
    <Modal :open="previewModalOpen" :title="`Xem trước: ${form.title || 'Bài học'}`" size="lg" @close="previewModalOpen = false">
      <div class="space-y-4 max-h-[75vh] overflow-y-auto p-4 bg-[#090d16] rounded-xl text-slate-200 custom-scrollbar">
        <!-- Interactive Tab Switcher -->
        <div class="flex items-center gap-1.5 p-1 bg-slate-900 border border-slate-800 rounded-xl">
          <button
            type="button"
            class="flex-1 py-1.5 px-3 rounded-lg text-xs font-bold transition-all flex items-center justify-center gap-1.5"
            :class="previewTab === 'theory' ? 'bg-purple-600 text-white shadow-md' : 'text-slate-400 hover:text-white'"
            @click="previewTab = 'theory'"
          >
            <PenTool :size="13" /> 1. Lý thuyết ({{ form.selectedSimulations.length }} mô phỏng)
          </button>

          <button
            type="button"
            class="flex-1 py-1.5 px-3 rounded-lg text-xs font-bold transition-all flex items-center justify-center gap-1.5"
            :class="previewTab === 'quiz' ? 'bg-amber-600 text-white shadow-md' : 'text-slate-400 hover:text-white'"
            @click="previewTab = 'quiz'"
          >
            <Puzzle :size="13" /> 2. Mini-Quiz ({{ form.quizQuestions.filter(q => q.content.trim().length > 0).length }})
          </button>

          <button
            v-if="form.codeLab.enabled"
            type="button"
            class="flex-1 py-1.5 px-3 rounded-lg text-xs font-bold transition-all flex items-center justify-center gap-1.5"
            :class="previewTab === 'codelab' ? 'bg-emerald-600 text-white shadow-md' : 'text-slate-400 hover:text-white'"
            @click="previewTab = 'codelab'"
          >
            <Code :size="13" /> 3. Code Lab ({{ form.codeLab.testCases.length }})
          </button>
        </div>

        <!-- ── TAB 1: LÝ THUYẾT & MÔ PHỎNG ── -->
        <div v-show="previewTab === 'theory'" class="space-y-4">
          <div class="border-b border-slate-800 pb-3">
            <h1 class="text-xl sm:text-2xl font-black text-white mb-1.5">{{ form.title }}</h1>
            <p v-if="form.description" class="text-xs text-slate-400 italic">{{ form.description }}</p>
          </div>

          <div v-if="form.selectedSimulations.length > 0" class="p-3.5 rounded-xl bg-vdsa-surface border border-sky-500/30 space-y-2">
            <span class="text-xs font-bold text-sky-300 block flex items-center gap-1.5">
              <Zap :size="14" class="text-sky-400" /> Mô phỏng thuật toán trực quan gắn kèm:
            </span>
            <div class="flex flex-wrap gap-2">
              <a
                v-for="k in form.selectedSimulations"
                :key="k"
                :href="`/simulator/${k}`"
                target="_blank"
                class="inline-flex items-center gap-1 px-2.5 py-1 rounded bg-sky-500/20 hover:bg-sky-500/30 text-sky-200 text-xs font-mono border border-sky-500/40 transition-colors"
                title="Bấm để mở trình chạy mô phỏng"
              >
                <span>{{ allSimulations.find(s => s.key === k)?.title || k }}</span>
                <ExternalLink :size="11" />
              </a>
            </div>
          </div>

          <div class="prose-preview pt-2" v-html="parseMarkdownToHtml(form.markdown)" />
        </div>

        <!-- ── TAB 2: MINI-QUIZ TƯƠNG TÁC ── -->
        <div v-show="previewTab === 'quiz'" class="space-y-4">
          <div v-if="form.quizQuestions.filter(q => q.content.trim().length > 0).length === 0" class="p-8 text-center text-slate-400 text-xs">
            Bài học này chưa có câu hỏi trắc nghiệm nào.
          </div>

          <div v-else class="space-y-4">
            <p class="text-xs text-slate-400">
              Chế độ thử nghiệm: Bạn có thể chọn đáp án để kiểm tra logic hiển thị và giải thích trước khi học viên làm bài.
            </p>

            <div
              v-for="(q, qIdx) in form.quizQuestions.filter(q => q.content.trim().length > 0)"
              :key="qIdx"
              class="p-4 rounded-xl bg-vdsa-surface border border-slate-800 space-y-3"
            >
              <div class="flex items-start gap-2.5">
                <span class="w-6 h-6 rounded-lg bg-amber-500/20 text-amber-300 font-bold text-xs flex items-center justify-center shrink-0">
                  {{ qIdx + 1 }}
                </span>
                <span class="text-xs font-bold text-white">{{ q.content }}</span>
              </div>

              <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
                <button
                  v-for="(opt, oIdx) in q.options.filter(o => o.trim().length > 0)"
                  :key="oIdx"
                  type="button"
                  class="p-2.5 rounded-lg border text-left text-xs font-medium transition-all flex items-center justify-between"
                  :class="
                    previewAnswers[qIdx] === oIdx
                      ? (oIdx === q.correctIndex
                          ? 'bg-emerald-950/40 border-emerald-500 text-emerald-200'
                          : 'bg-rose-950/40 border-rose-500 text-rose-200')
                      : 'bg-slate-900/70 border-slate-800 text-slate-300 hover:border-slate-700'
                  "
                  @click="previewAnswers[qIdx] = oIdx"
                >
                  <span>{{ String.fromCharCode(65 + oIdx) }}. {{ opt }}</span>
                  <span v-if="previewAnswers[qIdx] === oIdx">
                    {{ oIdx === q.correctIndex ? '✓ Đúng' : '✗ Sai' }}
                  </span>
                </button>
              </div>

              <div v-if="previewAnswers[qIdx] !== undefined && q.explanation" class="p-2.5 rounded-lg bg-slate-900/80 border border-slate-800 text-xs text-slate-300">
                <strong class="text-amber-300">Giải thích:</strong> {{ q.explanation }}
              </div>
            </div>
          </div>
        </div>

        <!-- ── TAB 3: CODE LAB PREVIEW ── -->
        <div v-show="previewTab === 'codelab'" class="space-y-4">
          <div v-if="!form.codeLab.enabled" class="p-8 text-center text-slate-400 text-xs">
            Bài học này không đính kèm bài tập Code Lab.
          </div>

          <div v-else class="space-y-4">
            <div class="p-4 rounded-xl bg-vdsa-surface border border-emerald-500/30 space-y-2">
              <div class="flex items-center justify-between flex-wrap gap-2">
                <h3 class="text-sm font-bold text-white">{{ form.codeLab.title }}</h3>
                <div class="flex items-center gap-2">
                  <Badge variant="secondary" class="text-[10px] font-mono">Hàm: {{ form.codeLab.entryFunction }}()</Badge>
                  <Badge variant="primary" class="text-[10px]">{{ form.codeLab.difficulty }}</Badge>
                  <Badge variant="secondary" class="text-[10px]">{{ form.codeLab.maxScore }} điểm</Badge>
                </div>
              </div>
              <p v-if="form.codeLab.description" class="text-xs text-slate-300 leading-relaxed">{{ form.codeLab.description }}</p>
            </div>

            <div class="space-y-1.5">
              <span class="text-xs font-bold text-slate-300 uppercase tracking-wider block">Starter Code:</span>
              <pre class="p-3.5 rounded-xl bg-[#060911] border border-slate-800 text-xs font-mono text-emerald-300 overflow-x-auto"><code>{{ form.codeLab.starterCode }}</code></pre>
            </div>

            <div class="space-y-2">
              <span class="text-xs font-bold text-slate-300 uppercase tracking-wider block">Test Cases ({{ form.codeLab.testCases.length }}):</span>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
                <div
                  v-for="(tc, tcIdx) in form.codeLab.testCases"
                  :key="tcIdx"
                  class="p-2.5 rounded-lg bg-slate-900 border border-slate-800 text-xs font-mono space-y-1"
                >
                  <div class="flex items-center justify-between text-slate-400 text-[11px]">
                    <span>#Testcase {{ tcIdx + 1 }}</span>
                    <span v-if="tc.isHidden" class="text-amber-400">Ẩn</span>
                    <span v-else class="text-slate-500">Công khai</span>
                  </div>
                  <div class="text-slate-300 truncate">Input: {{ tc.input }}</div>
                  <div class="text-emerald-400 truncate">Expected: {{ tc.expected }}</div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <div class="flex justify-end pt-3 border-t border-slate-800">
          <Button variant="secondary" size="sm" @click="previewModalOpen = false">Đóng xem trước</Button>
        </div>
      </div>
    </Modal>
  </div>
</template>

