<script setup lang="ts">
import { ref, reactive, watch, computed, onMounted, onUnmounted } from 'vue';
import {
  X,
  Save,
  BookOpen,
  HelpCircle,
  Code,
  Folder,
  Plus,
  Trash2,
  Check,
  Eye,
  Edit3,
  Sparkles,
  AlertCircle,
  CheckCircle2,
  ArrowRight,
  Copy,
  ChevronUp,
  ChevronDown,
  Play,
  Clock,
  Terminal,
  CheckCircle,
  XCircle,
  Upload,
  Wand2,
  FileText,
} from 'lucide-vue-next';
import mammoth from 'mammoth';
import InlineSimulationPlayer from '@/components/simulator/InlineSimulationPlayer.vue';
import {
  type PathItemDto,
  type PathItemType,
  normalizeItemType,
  updatePathItem,
  fetchItemDetail,
} from '@/api/pathItems';
import * as lessonsApi from '@/api/lessons';
import * as exercisesApi from '@/api/exercises';
import { useUiStore } from '@/stores/ui';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';
import Badge from '@/components/ui/Badge.vue';
import ProseContent from '@/components/ui/ProseContent.vue';
import TipTapEditor from '@/components/ui/TipTapEditor.vue';
import { parseMarkdownToHtml } from '@/utils/markdownParser';
import { LESSON_TEMPLATES, type LessonTemplate } from '@/data/lessonTemplates';
import { CATALOG } from '@/engines/catalog';
import SimulationPickerModal from './SimulationPickerModal.vue';

const props = defineProps<{
  open: boolean;
  item: PathItemDto | null;
  pathId: number;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'saved', item: PathItemDto): void;
  (e: 'addChild', type: PathItemType, parentId: number): void;
  (e: 'selectItem', item: PathItemDto): void;
}>();

const ui = useUiStore();
const saving = ref(false);
const loadingDetail = ref(false);
const activeViewMode = ref<'edit' | 'preview'>('edit');

// Common Form
const form = reactive({
  title: '',
  description: '',
});

// Theory Specific
const theoryContentHtml = ref('');
const attachedSimulations = ref<string[]>([]);

// Quiz Specific
interface InlineQuizQuestion {
  id?: number;
  content: string;
  type: 'Single' | 'Multiple';
  options: string[];
  correctIndex: number;
  correctIndices: number[];
  explanation: string;
  points: number;
}
const quizQuestions = ref<InlineQuizQuestion[]>([]);

// Lab Specific
interface LabTestCase {
  input: string;
  expected: string;
  isHidden: boolean;
}
const labForm = reactive({
  difficulty: 'Easy',
  entryFunction: 'solve',
  durationMinutes: 20,
  maxScore: 100,
  starterCode: `/**\n * @param {any} input\n * @return {any}\n */\nfunction solve(input) {\n  // Viết mã nguồn giải thuật tại đây\n  return Array.isArray(input) ? [...input].reverse() : input;\n}`,
  testCases: [
    { input: '[1, 2, 3]', expected: '[3, 2, 1]', isHidden: false },
    { input: '[5, 4, 3, 2, 1]', expected: '[1, 2, 3, 4, 5]', isHidden: true },
  ] as LabTestCase[],
});

const currentItemType = computed<PathItemType>(() => {
  return props.item ? normalizeItemType(props.item.itemType) : 'theory';
});

const theoryWordCount = computed(() => {
  const text = theoryContentHtml.value.replace(/<[^>]*>/g, ' ').trim();
  return text ? text.split(/\s+/).length : 0;
});

const theoryReadingTimeMinutes = computed(() => {
  return Math.max(1, Math.ceil(theoryWordCount.value / 180));
});

const totalQuizPoints = computed(() => {
  return quizQuestions.value.reduce((sum, q) => sum + (Number(q.points) || 0), 0);
});

// Lab Testing Sandbox in Studio
interface TestRunResult {
  index: number;
  input: string;
  expected: string;
  actual: string;
  passed: boolean;
  error?: string;
}
const testingLab = ref(false);
const testRunResults = ref<TestRunResult[] | null>(null);

function runLabTests() {
  testingLab.value = true;
  testRunResults.value = [];
  try {
    const fnName = labForm.entryFunction.trim() || 'solve';
    const runnerCode = `
      ${labForm.starterCode}
      if (typeof ${fnName} !== 'function') {
        throw new Error('Không tìm thấy hàm "' + "${fnName}" + '" trong mã nguồn khởi đầu');
      }
      return ${fnName};
    `;
    const userFn = new Function(runnerCode)();

    const results: TestRunResult[] = [];
    for (let i = 0; i < labForm.testCases.length; i++) {
      const tc = labForm.testCases[i];
      let parsedInput: any;
      try {
        parsedInput = JSON.parse(tc.input);
      } catch {
        parsedInput = tc.input;
      }

      try {
        const actualVal = userFn(parsedInput);
        const actualStr = JSON.stringify(actualVal);
        let expectedParsed: any;
        try {
          expectedParsed = JSON.parse(tc.expected);
        } catch {
          expectedParsed = tc.expected;
        }
        const expectedStr = JSON.stringify(expectedParsed);
        const passed = actualStr === expectedStr || String(actualVal).trim() === String(tc.expected).trim();
        results.push({
          index: i + 1,
          input: tc.input,
          expected: tc.expected,
          actual: actualStr !== undefined ? actualStr : String(actualVal),
          passed,
        });
      } catch (err: any) {
        results.push({
          index: i + 1,
          input: tc.input,
          expected: tc.expected,
          actual: 'Lỗi',
          passed: false,
          error: err?.message || String(err),
        });
      }
    }
    testRunResults.value = results;
    const passCount = results.filter((r) => r.passed).length;
    if (passCount === results.length) {
      ui.showToast(`Chạy thử hoàn tất: Đạt ${passCount}/${results.length} test cases!`, 'success');
    } else {
      ui.showToast(`Chạy thử hoàn tất: Đạt ${passCount}/${results.length} test cases.`, 'info');
    }
  } catch (err: any) {
    ui.showToast(`Lỗi khi thực thi mã nguồn: ${err?.message || err}`, 'error');
  } finally {
    testingLab.value = false;
  }
}

// ── Dirty state guard: cảnh báo thoát khi còn thay đổi chưa lưu (plan §5.1) ──
const dirtyBaseline = ref('{}');
const showDirtyConfirm = ref(false);

function snapshot(): string {
  return JSON.stringify({
    title: form.title,
    description: form.description,
    theory: theoryContentHtml.value,
    simulations: attachedSimulations.value,
    quiz: quizQuestions.value,
    lab: { ...labForm },
  });
}

const isDirty = computed(() => snapshot() !== dirtyBaseline.value);

function requestClose(): void {
  if (isDirty.value && !saving.value) {
    showDirtyConfirm.value = true;
    return;
  }
  emit('close');
}

function confirmClose(): void {
  showDirtyConfirm.value = false;
  emit('close');
}

watch(
  () => props.item,
  async (newItem) => {
    if (!newItem) return;
    form.title = newItem.title || '';
    form.description = newItem.description || '';
    theoryContentHtml.value = '';
    attachedSimulations.value = [];
    quizQuestions.value = [];

    // Fetch full detail if available
    loadingDetail.value = true;
    try {
      const detail = await fetchItemDetail(newItem.id);
      form.title = detail.title || '';
      form.description = detail.description || '';

      if (normalizeItemType(detail.itemType) === 'theory' && detail.lesson) {
        let content = detail.lesson.contentHtml || '';
        if (content && !content.includes('<p>') && !content.includes('<div>') && !content.includes('<h') && (content.includes('#') || content.includes('```') || content.includes('> [!'))) {
          content = parseMarkdownToHtml(content);
        }
        theoryContentHtml.value = content;
        attachedSimulations.value = (detail.lesson.simulations || []).map((s: any) => s.simulationKey || s);
      } else if (normalizeItemType(detail.itemType) === 'quiz' && detail.exercise) {
        if (detail.exercise.questions && detail.exercise.questions.length > 0) {
          quizQuestions.value = detail.exercise.questions.map((q: any) => {
            const isMulti = q.type === 'Multi' || q.type === 'Multiple' || q.type === 1;
            let rawAnswers: number[] = [];
            if (Array.isArray(q.answer)) {
              rawAnswers = q.answer;
            } else if (q.answerJson) {
              try {
                const parsed = JSON.parse(q.answerJson);
                if (Array.isArray(parsed)) rawAnswers = parsed;
              } catch {
                // ignore
              }
            }
            const correctIndices = rawAnswers.length > 0 ? rawAnswers : [q.correctIndex ?? 0];
            const correctIndex = correctIndices[0] ?? 0;
            return {
              id: q.id,
              content: q.content,
              type: isMulti ? 'Multiple' : 'Single',
              options: q.options || ['A', 'B', 'C', 'D'],
              correctIndex,
              correctIndices,
              explanation: q.explanation || '',
              points: q.points || 1,
            };
          });
        } else {
          quizQuestions.value = [
            {
              content: 'Câu hỏi trắc nghiệm số 1?',
              type: 'Single',
              options: ['Đáp án A', 'Đáp án B', 'Đáp án C', 'Đáp án D'],
              correctIndex: 0,
              correctIndices: [0],
              explanation: 'Giải thích chi tiết đáp án đúng.',
              points: 1,
            },
          ];
        }
      } else if (normalizeItemType(detail.itemType) === 'lab' && detail.exercise) {
        if (detail.exercise.configJson) {
          try {
            const parsed = JSON.parse(detail.exercise.configJson);
            labForm.starterCode = parsed.starterCode || labForm.starterCode;
            labForm.entryFunction = parsed.entryFunction || 'solve';
            if (Array.isArray(parsed.testCases)) {
              labForm.testCases = parsed.testCases;
            }
          } catch {
            // fallback
          }
        }
      }
    } catch {
      // Use props item as fallback
    } finally {
      loadingDetail.value = false;
      dirtyBaseline.value = snapshot();
    }
  },
  { immediate: true },
);

function applyTemplate(tpl: LessonTemplate): void {
  if (theoryContentHtml.value && theoryContentHtml.value.trim().length > 50) {
    if (!confirm('Nội dung hiện tại sẽ được thay thế bằng mẫu mới. Bạn có chắc chắn không?')) {
      return;
    }
  }
  theoryContentHtml.value = parseMarkdownToHtml(tpl.content);
  ui.showToast(`Đã áp dụng mẫu: "${tpl.name}"`, 'success');
}

const previewSimKey = ref<string | null>(null);
const fileInputRef = ref<HTMLInputElement | null>(null);

function toggleInlinePreviewSim(key: string): void {
  if (previewSimKey.value === key) {
    previewSimKey.value = null;
  } else {
    previewSimKey.value = key;
  }
}

async function handleWordUpload(e: Event): Promise<void> {
  const input = e.target as HTMLInputElement;
  if (!input.files || input.files.length === 0) return;
  const file = input.files[0];
  try {
    const arrayBuffer = await file.arrayBuffer();
    const result = await mammoth.convertToHtml({ arrayBuffer });
    const html = result.value;
    if (html) {
      if (theoryContentHtml.value && theoryContentHtml.value.trim().length > 30) {
        theoryContentHtml.value += `\n` + html;
      } else {
        theoryContentHtml.value = html;
      }
      ui.showToast(`Đã tải lên và đọc nội dung từ file "${file.name}" thành công!`, 'success');
    } else {
      ui.showToast('Không đọc được nội dung từ file đã chọn.', 'warning');
    }
  } catch (err: any) {
    ui.showToast(`Lỗi khi đọc file: ${err?.message || err}`, 'error');
  } finally {
    input.value = '';
  }
}

function handleAiFormat(): void {
  if (!theoryContentHtml.value || !theoryContentHtml.value.trim()) {
    ui.showToast('Vui lòng nhập hoặc tải nội dung bài giảng trước khi AI format', 'warning');
    return;
  }
  let content = theoryContentHtml.value;
  if (!content.includes('class="p-4 rounded-xl bg-blue-500/10') && !content.includes('> [!NOTE]')) {
    content = `<div class="my-4 p-4 rounded-xl bg-blue-500/10 border-l-4 border-blue-500 text-xs text-blue-200 not-prose"><strong class="text-blue-400 block mb-1">📌 Mục tiêu bài học:</strong>Nắm vững khái niệm cốt lõi, cơ chế hoạt động và phân tích độ phức tạp thời gian/không gian của thuật toán.</div>\n` + content;
  }
  if (!content.includes('class="p-4 rounded-xl bg-emerald-500/10') && !content.includes('> [!TIP]')) {
    content += `\n<div class="my-4 p-4 rounded-xl bg-emerald-500/10 border-l-4 border-emerald-500 text-xs text-emerald-200 not-prose"><strong class="text-emerald-400 block mb-1">💡 Mẹo tối ưu & Ghi nhớ:</strong>Luôn chú ý điều kiện biên, kích thước tập dữ liệu và xử lý trường hợp đặc biệt để đạt hiệu năng tối ưu.</div>`;
  }
  theoryContentHtml.value = content;
  ui.showToast('AI đã chuẩn hóa cấu trúc đề mục và khối kiến thức thành công!', 'success');
}

const showSimPicker = ref(false);
const pickerInitialKey = ref('sort.bubble');

function openPickerModal(key?: string): void {
  if (key) {
    pickerInitialKey.value = key;
  }
  showSimPicker.value = true;
}

function insertSimulationTag(key: string): void {
  const sim = CATALOG.find((c) => c.key === key);
  const title = sim ? sim.title : key;
  const tag = `<p><strong>🎮 Mô phỏng trực quan: ${title}</strong></p><p>[Mô phỏng: ${key}]</p>`;
  theoryContentHtml.value += tag;
  if (!attachedSimulations.value.includes(key)) {
    attachedSimulations.value.push(key);
  }
  ui.showToast(`Đã chèn mô phỏng: ${getSimTitle(key)}`, 'success');
}

function addSimulation(key: string): void {
  if (key && !attachedSimulations.value.includes(key)) {
    attachedSimulations.value.push(key);
    const sim = CATALOG.find((c) => c.key === key);
    const title = sim ? sim.title : key;
    const anchor = `[Mô phỏng: ${key}]`;
    if (!theoryContentHtml.value.includes(anchor)) {
      theoryContentHtml.value += `<p><strong>🎮 Mô phỏng trực quan: ${title}</strong></p><p>[Mô phỏng: ${key}]</p>`;
    }
    ui.showToast(`Đã nhúng mô phỏng: ${getSimTitle(key)}`, 'success');
  }
}

function removeSimulation(index: number): void {
  attachedSimulations.value.splice(index, 1);
}

function getSimTitle(key: string): string {
  const item = CATALOG.find((c) => c.key === key);
  return item ? `${item.title} (${item.key})` : key;
}

function isOptionCorrect(q: InlineQuizQuestion, optIdx: number): boolean {
  if (q.type === 'Multiple') {
    return Array.isArray(q.correctIndices) && q.correctIndices.includes(optIdx);
  }
  return q.correctIndex === optIdx;
}

function setSingleCorrect(q: InlineQuizQuestion, optIdx: number) {
  q.correctIndex = optIdx;
  q.correctIndices = [optIdx];
}

function toggleMultiCorrect(q: InlineQuizQuestion, optIdx: number) {
  if (!Array.isArray(q.correctIndices)) {
    q.correctIndices = [q.correctIndex ?? 0];
  }
  const idx = q.correctIndices.indexOf(optIdx);
  if (idx >= 0) {
    if (q.correctIndices.length > 1) {
      q.correctIndices.splice(idx, 1);
    } else {
      ui.showToast('Phải có ít nhất 1 đáp án đúng', 'warning');
    }
  } else {
    q.correctIndices.push(optIdx);
    q.correctIndices.sort((a, b) => a - b);
  }
  q.correctIndex = q.correctIndices[0] ?? 0;
}

function addQuestion() {
  quizQuestions.value.push({
    content: `Câu hỏi ${quizQuestions.value.length + 1}?`,
    type: 'Single',
    options: ['Lựa chọn A', 'Lựa chọn B', 'Lựa chọn C', 'Lựa chọn D'],
    correctIndex: 0,
    correctIndices: [0],
    explanation: '',
    points: 1,
  });
}

function removeQuestion(index: number) {
  quizQuestions.value.splice(index, 1);
}

function duplicateQuestion(index: number) {
  const target = quizQuestions.value[index];
  if (!target) return;
  quizQuestions.value.splice(index + 1, 0, {
    content: `${target.content} (Bản sao)`,
    type: target.type ?? 'Single',
    options: [...target.options],
    correctIndex: target.correctIndex,
    correctIndices: [...(target.correctIndices ?? [target.correctIndex ?? 0])],
    explanation: target.explanation,
    points: target.points,
  });
  ui.showToast('Đã nhân bản câu hỏi', 'success');
}

function moveQuestionUp(index: number) {
  if (index <= 0) return;
  const temp = quizQuestions.value[index];
  quizQuestions.value[index] = quizQuestions.value[index - 1];
  quizQuestions.value[index - 1] = temp;
}

function moveQuestionDown(index: number) {
  if (index >= quizQuestions.value.length - 1) return;
  const temp = quizQuestions.value[index];
  quizQuestions.value[index] = quizQuestions.value[index + 1];
  quizQuestions.value[index + 1] = temp;
}

function addOption(qIdx: number) {
  if (quizQuestions.value[qIdx].options.length < 6) {
    quizQuestions.value[qIdx].options.push(`Lựa chọn ${quizQuestions.value[qIdx].options.length + 1}`);
  }
}

function removeOption(qIdx: number, optIdx: number) {
  const q = quizQuestions.value[qIdx];
  if (q.options.length > 2) {
    q.options.splice(optIdx, 1);
    if (q.type === 'Multiple' && Array.isArray(q.correctIndices)) {
      q.correctIndices = q.correctIndices
        .filter((i) => i !== optIdx)
        .map((i) => (i > optIdx ? i - 1 : i));
      if (q.correctIndices.length === 0) {
        q.correctIndices = [0];
      }
      q.correctIndex = q.correctIndices[0] ?? 0;
    } else {
      if (q.correctIndex >= q.options.length) {
        q.correctIndex = 0;
      }
      q.correctIndices = [q.correctIndex];
    }
  }
}

function addTestCase() {
  labForm.testCases.push({ input: '', expected: '', isHidden: false });
}

function removeTestCase(idx: number) {
  labForm.testCases.splice(idx, 1);
}

// ── Lắng nghe phím tắt Ctrl + S / Cmd + S & Chống mất dữ liệu ──
function handleBeforeUnload(e: BeforeUnloadEvent): void {
  if (isDirty.value && !saving.value) {
    e.preventDefault();
    e.returnValue = '';
  }
}

function onGlobalKeydown(e: KeyboardEvent) {
  if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 's') {
    e.preventDefault();
    if (!saving.value) {
      void handleSave();
    }
  }
}

onMounted(() => {
  window.addEventListener('keydown', onGlobalKeydown);
  window.addEventListener('beforeunload', handleBeforeUnload);
});

onUnmounted(() => {
  window.removeEventListener('keydown', onGlobalKeydown);
  window.removeEventListener('beforeunload', handleBeforeUnload);
});


async function handleSave() {
  if (!props.item) return;
  if (!form.title.trim()) {
    ui.showToast('Tiêu đề không được để trống', 'warning');
    return;
  }

  saving.value = true;
  try {
    // 1. Update Path Item Node (Title, Description)
    const updated = await updatePathItem(props.item.id, {
      title: form.title.trim(),
      description: form.description.trim(),
    });

    // 2. Update Type-specific payload
    const itemType = normalizeItemType(props.item.itemType);

    if (itemType === 'theory') {
      const inlineSimKeys: string[] = [];
      const matches = theoryContentHtml.value.matchAll(/\[(?:Mô phỏng|Simulation|mo phong):\s*([a-zA-Z0-9._-]+)\]/gi);
      for (const m of matches) {
        if (m[1] && !inlineSimKeys.includes(m[1])) {
          inlineSimKeys.push(m[1]);
        }
      }
      const simKeysToSave = Array.from(new Set([...attachedSimulations.value, ...inlineSimKeys]));

      if (props.item.lessonId) {
        try {
          // PUT lesson = thay thế toàn bộ resource: fetch hiện trạng để giữ topicId/status
          const currentLesson = await lessonsApi.fetchLesson(props.item.lessonId);
          await lessonsApi.updateLesson(props.item.lessonId, {
            topicId: currentLesson.topicId ?? 1,
            title: form.title.trim(),
            description: form.description.trim(),
            contentHtml: theoryContentHtml.value,
            status: currentLesson.status ?? 'active',
            sortOrder: currentLesson.sortOrder ?? 1,
            isClassOnly: currentLesson.isClassOnly ?? false,
            simulationKeys: simKeysToSave,
          });
        } catch (fetchErr) {
          console.warn('Lesson không tồn tại trên máy chủ, đang tạo mới bài học thay thế:', fetchErr);
          const created = await lessonsApi.createLesson({
            topicId: 1,
            title: form.title.trim(),
            description: form.description.trim(),
            contentHtml: theoryContentHtml.value,
            status: 'active',
            sortOrder: 1,
            isClassOnly: false,
            simulationKeys: simKeysToSave,
          });
          props.item.lessonId = created.id;
          await updatePathItem(props.item.id, {
            lessonId: created.id,
            title: form.title.trim(),
            description: form.description.trim(),
          });
        }
      } else {
        // Tạo lesson mới nếu node chưa có lessonId và liên kết ngay lập tức
        const created = await lessonsApi.createLesson({
          topicId: 1,
          title: form.title.trim(),
          description: form.description.trim(),
          contentHtml: theoryContentHtml.value,
          status: 'active',
          sortOrder: 1,
          isClassOnly: false,
          simulationKeys: simKeysToSave,
        });
        props.item.lessonId = created.id;
        await updatePathItem(props.item.id, {
          lessonId: created.id,
          title: form.title.trim(),
          description: form.description.trim(),
        });
      }
    } else if (itemType === 'quiz') {
      if (quizQuestions.value.length === 0) {
        ui.showToast('Vui lòng thêm ít nhất 1 câu hỏi trắc nghiệm', 'warning');
        saving.value = false;
        return;
      }
      const exerciseId = props.item.finalTestId ?? props.item.exerciseId ?? props.item.exercise?.id;
      const quizPayload = {
        title: form.title.trim(),
        description: form.description.trim(),
        maxScore: quizQuestions.value.reduce((sum, q) => sum + (Number(q.points) || 0), 0),
        questions: quizQuestions.value.map((q, idx) => ({
          content: q.content,
          type: q.type === 'Multiple' ? ('Multiple' as const) : ('Single' as const),
          options: q.options,
          answerJson: JSON.stringify(q.type === 'Multiple' ? (q.correctIndices?.length ? q.correctIndices : [q.correctIndex || 0]) : [q.correctIndex || 0]),
          explanation: q.explanation,
          points: Number(q.points) || 1,
          sortOrder: idx + 1,
        })),
      };
      if (exerciseId) {
        await exercisesApi.updateExercise(exerciseId, quizPayload);
      } else {
        const created = await exercisesApi.createExercise({
          lessonId: props.item.lessonId ?? 0,
          nodeId: props.item.id,
          type: 'Mcq',
          ...quizPayload,
        });
        props.item.finalTestId = created.id;
        (props.item as any).exerciseId = created.id;
        await updatePathItem(props.item.id, {
          finalTestId: created.id,
          title: form.title.trim(),
          description: form.description.trim(),
        });
      }
    } else if (itemType === 'lab') {
      const exerciseId = props.item.labExerciseId ?? props.item.exerciseId ?? props.item.exercise?.id;
      const configJson = JSON.stringify({
        entryFunction: labForm.entryFunction,
        starterCode: labForm.starterCode,
        testCases: labForm.testCases.map(tc => ({
          input: tc.input,
          expected: (tc as any).expected ?? (tc as any).expectedOutput ?? '',
          expectedOutput: (tc as any).expectedOutput ?? (tc as any).expected ?? '',
          isHidden: !!tc.isHidden,
        })),
      });
      const labPayload = {
        title: form.title.trim(),
        description: form.description.trim(),
        maxScore: labForm.maxScore,
        configJson,
      };
      if (exerciseId) {
        await exercisesApi.updateExercise(exerciseId, labPayload);
      } else {
        const created = await exercisesApi.createExercise({
          lessonId: props.item.lessonId ?? 0,
          nodeId: props.item.id,
          type: 'Code',
          stage: 3,
          ...labPayload,
        });
        props.item.labExerciseId = created.id;
        (props.item as any).exerciseId = created.id;
        await updatePathItem(props.item.id, {
          labExerciseId: created.id,
          title: form.title.trim(),
          description: form.description.trim(),
        });
      }
    }

    ui.showToast('Đã lưu nội dung thành công', 'success');
    dirtyBaseline.value = snapshot();
    showDirtyConfirm.value = false;
    emit('saved', updated);
  } catch (err: any) {
    ui.showToast(err?.message || 'Lỗi khi lưu mục nội dung', 'error');
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <div
    v-if="open && item"
    class="item-editor-slideover relative flex flex-col h-full bg-[#12111a] border border-[#262438] rounded-2xl overflow-hidden shadow-2xl"
    data-testid="item-editor-slideover"
  >
    <!-- Header -->
    <div class="p-3.5 bg-[#171622] border-b border-[#262438] flex items-center justify-between gap-3 shrink-0">
      <div class="flex items-center gap-2.5 min-w-0">
        <div class="p-1.5 rounded-lg bg-purple-500/20 text-purple-300 border border-purple-500/30 shrink-0">
          <Folder v-if="currentItemType === 'folder'" class="w-4 h-4 text-amber-400" />
          <BookOpen v-else-if="currentItemType === 'theory'" class="w-4 h-4 text-sky-400" />
          <HelpCircle v-else-if="currentItemType === 'quiz'" class="w-4 h-4 text-orange-400" />
          <Code v-else-if="currentItemType === 'lab'" class="w-4 h-4 text-emerald-400" />
        </div>
        <div class="min-w-0">
          <h3 class="text-xs font-black uppercase tracking-wider text-slate-200 truncate">
            Chỉnh sửa: {{ currentItemType === 'folder' ? 'Chương (Module)' : currentItemType === 'theory' ? 'Lý thuyết' : currentItemType === 'quiz' ? 'Quiz Trắc nghiệm' : 'Codelab Thử thách' }}
          </h3>
          <p class="text-[10px] text-slate-400 truncate">Node #{{ item.id }}</p>
        </div>
      </div>

      <div class="flex items-center gap-2">
        <button
          type="button"
          data-testid="item-editor-save"
          :disabled="saving"
          class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-purple-600 hover:bg-purple-500 text-white text-xs font-bold shadow-md shadow-purple-950/50 transition-colors disabled:opacity-50 cursor-pointer"
          @click="handleSave"
        >
          <Save class="w-3.5 h-3.5" />
          <span>{{ saving ? 'Đang lưu...' : 'Lưu' }}</span>
        </button>
        <button
          type="button"
          data-testid="item-editor-close"
          aria-label="Đóng bảng soạn thảo"
          class="p-1.5 rounded-lg hover:bg-white/10 text-slate-400 hover:text-white transition-colors cursor-pointer"
          @click="requestClose"
        >
          <X class="w-4 h-4" />
        </button>
      </div>
    </div>

    <!-- Scrollable Body -->
    <div class="flex-1 p-4 overflow-y-auto space-y-4">
      <!-- Common Metadata Section -->
      <div class="space-y-3 p-3.5 bg-[#171624] border border-[#27253b] rounded-xl">
        <div>
          <label class="block text-[11px] font-bold text-slate-300 uppercase tracking-wider mb-1">
            Tiêu đề mục <span class="text-rose-400">*</span>
          </label>
          <input
            v-model="form.title"
            type="text"
            placeholder="Nhập tiêu đề..."
            class="w-full px-3 py-2 text-xs font-medium bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
          />
        </div>
        <div>
          <label class="block text-[11px] font-bold text-slate-300 uppercase tracking-wider mb-1">
            Mô tả tóm tắt
          </label>
          <textarea
            v-model="form.description"
            rows="2"
            placeholder="Mô tả mục tiêu hoặc nội dung cốt lõi của chương / bài học..."
            class="w-full px-3 py-2 text-xs font-medium bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
          />
        </div>
      </div>

      <!-- Folder Specific: Quick Actions & Child Summary -->
      <div v-if="currentItemType === 'folder'" class="space-y-4">
        <div class="p-4 bg-gradient-to-br from-[#1b192e] to-[#141322] border border-purple-500/25 rounded-xl space-y-3 shadow-md">
          <div class="flex items-center gap-2">
            <Folder class="w-4 h-4 text-amber-400" />
            <h4 class="text-xs font-black uppercase tracking-wider text-slate-200">
              Bước tiếp theo — Thêm bài học vào chương này
            </h4>
          </div>
          <p class="text-xs text-slate-400 leading-relaxed">
            Bạn có thể nhanh chóng thêm các bài học lý thuyết, trắc nghiệm hoặc bài thực hành lập trình trực tiếp vào bên trong chương này:
          </p>
          
          <div class="grid grid-cols-1 sm:grid-cols-3 gap-2.5 pt-1">
            <button
              type="button"
              class="flex items-center justify-center gap-2 p-2.5 rounded-xl bg-sky-500/10 hover:bg-sky-500/20 border border-sky-500/30 text-sky-300 text-xs font-bold transition-all cursor-pointer shadow-sm hover:scale-[1.02]"
              @click="$emit('addChild', 'theory', item.id)"
            >
              <BookOpen class="w-4 h-4" />
              <span>+ Bài lý thuyết</span>
            </button>

            <button
              type="button"
              class="flex items-center justify-center gap-2 p-2.5 rounded-xl bg-orange-500/10 hover:bg-orange-500/20 border border-orange-500/30 text-orange-300 text-xs font-bold transition-all cursor-pointer shadow-sm hover:scale-[1.02]"
              @click="$emit('addChild', 'quiz', item.id)"
            >
              <HelpCircle class="w-4 h-4" />
              <span>+ Quiz trắc nghiệm</span>
            </button>

            <button
              type="button"
              class="flex items-center justify-center gap-2 p-2.5 rounded-xl bg-emerald-500/10 hover:bg-emerald-500/20 border border-emerald-500/30 text-emerald-300 text-xs font-bold transition-all cursor-pointer shadow-sm hover:scale-[1.02]"
              @click="$emit('addChild', 'lab', item.id)"
            >
              <Code class="w-4 h-4" />
              <span>+ Codelab thử thách</span>
            </button>
          </div>
        </div>

        <!-- Danh sách bài học hiện có trong chương -->
        <div class="p-3.5 bg-[#171624] border border-[#27253b] rounded-xl space-y-2.5">
          <div class="flex items-center justify-between">
            <span class="text-[11px] font-bold text-slate-300 uppercase tracking-wider">
              Nội dung trong chương ({{ item.children?.length ?? 0 }} bài học)
            </span>
          </div>
          <div v-if="item.children?.length" class="space-y-1.5 pt-1">
            <div
              v-for="(child, idx) in item.children"
              :key="child.id"
              class="flex items-center justify-between px-3 py-2 bg-[#0e0d16] border border-[#262438] rounded-lg text-xs hover:border-purple-500/30 transition-colors"
            >
              <div class="flex items-center gap-2 min-w-0">
                <span class="text-[10px] font-mono text-slate-500">{{ idx + 1 }}.</span>
                <BookOpen v-if="normalizeItemType(child.itemType) === 'theory'" class="w-3.5 h-3.5 text-sky-400 shrink-0" />
                <HelpCircle v-else-if="normalizeItemType(child.itemType) === 'quiz'" class="w-3.5 h-3.5 text-orange-400 shrink-0" />
                <Code v-else-if="normalizeItemType(child.itemType) === 'lab'" class="w-3.5 h-3.5 text-emerald-400 shrink-0" />
                <Folder v-else class="w-3.5 h-3.5 text-amber-400 shrink-0" />
                <span class="font-bold text-slate-200 truncate">{{ child.title || 'Mục chưa đặt tên' }}</span>
              </div>
              <button
                type="button"
                class="text-[11px] font-bold text-purple-400 hover:text-purple-300 px-2 py-0.5 rounded hover:bg-purple-500/10 transition-colors cursor-pointer shrink-0 flex items-center gap-1"
                @click="$emit('selectItem', child)"
              >
                <span>Chỉnh sửa</span>
                <ArrowRight class="w-3 h-3" />
              </button>
            </div>
          </div>
          <p v-else class="text-xs text-slate-500 italic py-2">
            Chương này chưa có bài học nào. Hãy bấm một trong các nút phía trên để thêm bài học đầu tiên.
          </p>
        </div>
      </div>

      <!-- Theory Editor (WYSIWYG Word-Style TipTap) -->
      <div v-else-if="currentItemType === 'theory'" class="space-y-3">
        <div class="flex items-center justify-between gap-2 flex-wrap pb-1">
          <div class="flex items-center gap-2">
            <label class="text-[11px] font-bold text-slate-300 uppercase tracking-wider flex items-center gap-1.5">
              <BookOpen class="w-3.5 h-3.5 text-sky-400" />
              <span>Nội dung Lý thuyết</span>
            </label>
            <span class="text-[10px] font-bold px-2 py-0.5 rounded-full bg-sky-500/20 text-sky-300 border border-sky-500/30 flex items-center gap-1">
              <Clock class="w-3 h-3" />
              <span>~{{ theoryReadingTimeMinutes }} phút đọc</span>
            </span>
          </div>
          <div class="flex items-center gap-2 flex-wrap">
            <!-- Hidden Word Upload Input -->
            <input
              ref="fileInputRef"
              type="file"
              accept=".docx,.doc,.txt"
              class="hidden"
              @change="handleWordUpload"
            />

            <!-- Nút Upload Word -->
            <button
              type="button"
              class="px-2.5 py-1 text-xs font-bold bg-[#1d1b2e] hover:bg-purple-900/40 border border-purple-500/30 text-purple-200 rounded-lg transition-all flex items-center gap-1.5 cursor-pointer shadow-sm"
              title="Tải lên tệp Word (.docx) và chuyển đổi thành bài giảng"
              @click="fileInputRef?.click()"
            >
              <Upload class="w-3.5 h-3.5 text-purple-400" />
              <span>Tải Word (.docx)</span>
            </button>

            <!-- Nút AI Format -->
            <button
              type="button"
              class="px-2.5 py-1 text-xs font-bold bg-amber-500/10 hover:bg-amber-500/20 border border-amber-500/30 text-amber-300 rounded-lg transition-all flex items-center gap-1.5 cursor-pointer shadow-sm"
              title="AI tự động phân tích và chuẩn hóa cấu trúc bài giảng"
              @click="handleAiFormat"
            >
              <Wand2 class="w-3.5 h-3.5 text-amber-400" />
              <span>AI Chuẩn hóa</span>
            </button>

            <!-- Dropdown Mẫu bài giảng nhanh -->
            <select
              aria-label="Chọn mẫu bài giảng nhanh"
              class="px-2.5 py-1 text-xs bg-[#171624] border border-[#27253b] rounded-lg text-slate-300 hover:text-white outline-none cursor-pointer"
              @change="(e) => {
                const target = e.target as HTMLSelectElement;
                const tpl = LESSON_TEMPLATES.find(t => t.id === target.value);
                if (tpl) applyTemplate(tpl);
                target.value = '';
              }"
            >
              <option value="" disabled selected>📑 Mẫu bài giảng...</option>
              <option v-for="tpl in LESSON_TEMPLATES" :key="tpl.id" :value="tpl.id">
                {{ tpl.name }}
              </option>
            </select>

            <!-- Nút mở modal xem trước & chèn mô phỏng -->
            <button
              type="button"
              class="px-2.5 py-1 text-xs font-bold bg-[#231e38] hover:bg-purple-950/80 border border-purple-500/40 text-purple-200 rounded-lg transition-all flex items-center gap-1.5 cursor-pointer shadow-sm"
              title="Xem trước 44 mô phỏng và chèn vào nội dung bài giảng"
              @click="openPickerModal()"
            >
              <Sparkles class="w-3.5 h-3.5 text-purple-400" />
              <span>🎮 Chèn Mô phỏng</span>
            </button>

            <!-- Toggle Soạn thảo / Xem trước -->
            <div class="flex items-center gap-1 bg-[#171624] p-1 rounded-lg border border-[#27253b]">
              <button
                type="button"
                class="px-2.5 py-0.5 text-[10px] font-bold rounded transition-colors cursor-pointer"
                :class="activeViewMode === 'edit' ? 'bg-purple-600 text-white' : 'text-slate-400 hover:text-white'"
                @click="activeViewMode = 'edit'"
              >
                Soạn thảo
              </button>
              <button
                type="button"
                class="px-2.5 py-0.5 text-[10px] font-bold rounded transition-colors cursor-pointer"
                :class="activeViewMode === 'preview' ? 'bg-purple-600 text-white' : 'text-slate-400 hover:text-white'"
                @click="activeViewMode = 'preview'"
              >
                Xem trước
              </button>
            </div>
          </div>
        </div>

        <TipTapEditor
          v-if="activeViewMode === 'edit'"
          v-model="theoryContentHtml"
          class="min-h-[360px]"
        />
        <div
          v-else
          class="p-5 bg-[#0e0d16] border border-[#2e2c44] rounded-2xl min-h-[360px] text-slate-200 text-xs leading-relaxed prose prose-invert max-w-none"
        >
          <ProseContent :html="theoryContentHtml" />
        </div>

        <!-- Attached Visualizer Simulations Section -->
        <div class="p-3.5 bg-[#171624] border border-[#27253b] rounded-xl space-y-2.5">
          <div class="flex items-center justify-between gap-2 flex-wrap">
            <label class="text-[11px] font-bold text-slate-300 uppercase tracking-wider flex items-center gap-1.5">
              <Sparkles class="w-3.5 h-3.5 text-purple-400" />
              <span>Mô phỏng Trực quan đính kèm ({{ attachedSimulations.length }})</span>
            </label>
            
            <div class="flex items-center gap-2">
              <button
                type="button"
                data-testid="btn-browse-simulations"
                class="px-2.5 py-1 text-xs font-bold bg-purple-600/30 hover:bg-purple-600 border border-purple-500/40 text-purple-200 rounded-lg transition-all cursor-pointer flex items-center gap-1.5 shadow-sm"
                @click="openPickerModal()"
              >
                <Eye class="w-3.5 h-3.5" />
                <span>Xem &amp; Chọn Mô phỏng</span>
              </button>

              <select
                data-testid="add-attached-simulation"
                class="px-2.5 py-1 text-xs font-bold bg-[#231e38] text-purple-200 border border-purple-500/40 rounded-lg hover:bg-purple-500/25 transition-colors cursor-pointer outline-none max-w-[170px] truncate shadow-sm"
                @change="(e) => {
                  const target = e.target as HTMLSelectElement;
                  if (target.value) {
                    addSimulation(target.value);
                    target.value = '';
                  }
                }"
              >
                <option value="" disabled selected>+ Chọn nhanh...</option>
                <optgroup label="📊 Sắp xếp (Sorting)">
                  <option value="sort.bubble">Sắp xếp nổi bọt (Bubble Sort)</option>
                  <option value="sort.selection">Sắp xếp chọn (Selection Sort)</option>
                  <option value="sort.insertion">Sắp xếp chèn (Insertion Sort)</option>
                  <option value="sort.merge">Sắp xếp trộn (Merge Sort)</option>
                  <option value="sort.quick">Sắp xếp nhanh (Quick Sort)</option>
                  <option value="sort.heap">Sắp xếp vun đống (Heap Sort)</option>
                </optgroup>
                <optgroup label="🔍 Tìm kiếm (Searching)">
                  <option value="search.linear">Tìm kiếm tuyến tính</option>
                  <option value="search.binary">Tìm kiếm nhị phân (Binary Search)</option>
                </optgroup>
                <optgroup label="🥞 Ngăn xếp & Hàng đợi">
                  <option value="structure.stack">Ngăn xếp (Stack)</option>
                  <option value="stack.push">Stack — Push</option>
                  <option value="stack.pop">Stack — Pop</option>
                  <option value="structure.queue">Hàng đợi (Queue)</option>
                  <option value="queue.enqueue">Queue — Enqueue</option>
                  <option value="queue.dequeue">Queue — Dequeue</option>
                </optgroup>
                <optgroup label="🔗 Danh sách liên kết">
                  <option value="structure.linkedlist">Danh sách liên kết đơn</option>
                  <option value="list.insert">Linked List — Chèn</option>
                  <option value="list.delete">Linked List — Xóa</option>
                  <option value="list.search">Linked List — Tìm kiếm</option>
                </optgroup>
                <optgroup label="🌳 Cây & BST & AVL">
                  <option value="structure.bst">Cây BST</option>
                  <option value="tree.bst-insert">BST — Chèn</option>
                  <option value="tree.bst-delete">BST — Xóa</option>
                  <option value="tree.bst-search">BST — Tìm kiếm</option>
                  <option value="tree.bst-inorder">BST — Duyệt Inorder</option>
                  <option value="tree.avl-insert">Cây AVL — Chèn & Xoay</option>
                </optgroup>
                <optgroup label="🏔️ Đống (Heap)">
                  <option value="structure.heap">Đống nhị phân (Heap)</option>
                  <option value="heap.insert">Heap — Chèn</option>
                  <option value="heap.extract">Heap — Trích xuất Max</option>
                </optgroup>
                <optgroup label="🔑 Bảng băm">
                  <option value="structure.hashtable">Bảng băm (Hash Table)</option>
                  <option value="hash.insert">Hash Table — Chèn</option>
                  <option value="hash.search">Hash Table — Tìm kiếm</option>
                </optgroup>
                <optgroup label="🕸️ Đồ thị (Graph)">
                  <option value="structure.graph">Đồ thị (Graph)</option>
                  <option value="graph.bfs">Đồ thị — Duyệt BFS</option>
                  <option value="graph.dfs">Đồ thị — Duyệt DFS</option>
                  <option value="graph.dijkstra">Đồ thị — Dijkstra ngắn nhất</option>
                </optgroup>
              </select>
            </div>
          </div>

          <div v-if="attachedSimulations.length > 0" class="space-y-3 pt-1">
            <div class="flex flex-wrap gap-2">
              <div
                v-for="(simKey, sIdx) in attachedSimulations"
                :key="sIdx"
                class="px-2.5 py-1.5 rounded-lg bg-purple-500/15 border border-purple-500/30 text-purple-200 text-xs font-semibold flex items-center gap-2"
              >
                <Sparkles class="w-3.5 h-3.5 text-purple-400" />
                <span>{{ getSimTitle(simKey) }}</span>
                <button
                  type="button"
                  title="Chạy thử mô phỏng trực tiếp ngay trong Studio"
                  class="px-2 py-0.5 rounded text-[10px] font-bold cursor-pointer transition-colors flex items-center gap-1"
                  :class="previewSimKey === simKey ? 'bg-purple-600 text-white' : 'bg-purple-600/30 hover:bg-purple-600 text-purple-200'"
                  @click="toggleInlinePreviewSim(simKey)"
                >
                  <Play class="w-3 h-3" />
                  <span>{{ previewSimKey === simKey ? 'Đóng xem thử' : 'Chạy thử tại đây' }}</span>
                </button>
                <button
                  type="button"
                  title="Gỡ mô phỏng này"
                  class="text-slate-400 hover:text-rose-400 p-0.5 rounded cursor-pointer transition-colors"
                  @click="removeSimulation(sIdx)"
                >
                  <X class="w-3.5 h-3.5" />
                </button>
              </div>
            </div>

            <!-- Inline Interactive Player directly in Studio -->
            <div v-if="previewSimKey" class="p-3 bg-[#0d0c18] border border-purple-500/40 rounded-xl space-y-2">
              <div class="flex items-center justify-between">
                <span class="text-xs font-bold text-purple-300 flex items-center gap-1.5">
                  <Play class="w-3.5 h-3.5 text-purple-400" />
                  <span>Trực quan hóa thuật toán: {{ getSimTitle(previewSimKey) }}</span>
                </span>
                <button
                  type="button"
                  class="text-xs text-slate-400 hover:text-white px-2 py-0.5 rounded hover:bg-white/10"
                  @click="previewSimKey = null"
                >
                  ✕ Đóng
                </button>
              </div>
              <InlineSimulationPlayer :sim-key="previewSimKey" />
            </div>
          </div>
          <p v-else class="text-[11px] text-slate-500 italic">
            Chưa có mô phỏng nào được đính kèm. Bấm "Xem &amp; Chọn Mô phỏng" để duyệt và chạy thử 44 giải thuật trực quan.
          </p>
        </div>
      </div>

      <!-- Quiz Editor -->
      <div v-else-if="currentItemType === 'quiz'" class="space-y-4">
        <div class="flex items-center justify-between gap-2 flex-wrap pb-1">
          <div class="flex items-center gap-2">
            <label class="text-[11px] font-bold text-slate-300 uppercase tracking-wider flex items-center gap-1.5">
              <HelpCircle class="w-3.5 h-3.5 text-orange-400" />
              <span>Danh sách Câu hỏi Trắc nghiệm ({{ quizQuestions.length }})</span>
            </label>
            <span class="text-[10px] font-black px-2 py-0.5 rounded-full bg-orange-500/20 text-orange-300 border border-orange-500/30">
              Tổng điểm: {{ totalQuizPoints }}
            </span>
          </div>
          <button
            type="button"
            class="flex items-center gap-1 px-2.5 py-1 text-xs font-bold text-orange-300 bg-orange-500/10 hover:bg-orange-500/20 border border-orange-500/30 rounded-lg transition-colors cursor-pointer"
            @click="addQuestion"
          >
            <Plus class="w-3.5 h-3.5" />
            <span>+ Câu hỏi</span>
          </button>
        </div>

        <div v-for="(q, qIdx) in quizQuestions" :key="qIdx" class="p-3.5 bg-[#171624] border border-[#27253b] rounded-xl space-y-3 shadow-sm">
          <div class="flex items-center justify-between gap-2">
            <div class="flex items-center gap-2">
              <span class="px-2 py-0.5 rounded bg-orange-500/20 text-orange-300 text-xs font-black">
                Câu {{ qIdx + 1 }}
              </span>
              <select
                v-model="q.type"
                class="px-2 py-0.5 text-[11px] font-bold bg-[#0e0d16] border border-[#2e2c44] rounded text-slate-300 focus:outline-none focus:border-purple-500"
                @change="if (q.type === 'Single') q.correctIndices = [q.correctIndex || 0]; else if (!q.correctIndices?.length) q.correctIndices = [q.correctIndex || 0]"
              >
                <option value="Single">1 Đáp án</option>
                <option value="Multiple">Nhiều đáp án</option>
              </select>
            </div>
            <div class="flex items-center gap-2">
              <span class="text-[10px] font-bold text-slate-400">Điểm:</span>
              <input
                v-model.number="q.points"
                type="number"
                min="1"
                max="10"
                class="w-12 px-1.5 py-0.5 text-xs font-bold bg-[#0e0d16] border border-[#2e2c44] rounded text-white text-center"
              />
              <div class="flex items-center gap-1">
                <button
                  v-if="qIdx > 0"
                  type="button"
                  title="Di chuyển lên"
                  class="p-1 text-slate-400 hover:text-white rounded hover:bg-white/10 cursor-pointer transition-colors"
                  @click="moveQuestionUp(qIdx)"
                >
                  <ChevronUp class="w-3.5 h-3.5" />
                </button>
                <button
                  v-if="qIdx < quizQuestions.length - 1"
                  type="button"
                  title="Di chuyển xuống"
                  class="p-1 text-slate-400 hover:text-white rounded hover:bg-white/10 cursor-pointer transition-colors"
                  @click="moveQuestionDown(qIdx)"
                >
                  <ChevronDown class="w-3.5 h-3.5" />
                </button>
                <button
                  type="button"
                  title="Nhân bản câu hỏi này"
                  class="p-1 text-sky-400 hover:bg-sky-500/20 rounded cursor-pointer transition-colors"
                  @click="duplicateQuestion(qIdx)"
                >
                  <Copy class="w-3.5 h-3.5" />
                </button>
                <button
                  type="button"
                  title="Xóa câu hỏi này"
                  class="p-1 text-rose-400 hover:bg-rose-500/20 rounded cursor-pointer transition-colors"
                  @click="removeQuestion(qIdx)"
                >
                  <Trash2 class="w-3.5 h-3.5" />
                </button>
              </div>
            </div>
          </div>

          <div>
            <textarea
              v-model="q.content"
              rows="2"
              placeholder="Nhập nội dung câu hỏi..."
              class="w-full px-3 py-1.5 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500 font-medium"
            />
          </div>

          <!-- Options -->
          <div class="space-y-2">
            <span class="text-[10px] uppercase font-bold text-slate-400 tracking-wider">
              Các lựa chọn ({{ q.type === 'Multiple' ? 'Tích chọn các đáp án đúng' : 'Chọn 1 đáp án đúng' }}):
            </span>
            <div
              v-for="(opt, optIdx) in q.options"
              :key="optIdx"
              class="flex items-center gap-2 p-1.5 rounded-lg border transition-all"
              :class="isOptionCorrect(q, optIdx) ? 'bg-emerald-500/15 border-emerald-500/50 shadow-sm' : 'bg-[#0e0d16] border-[#2e2c44]'"
            >
              <input
                v-if="q.type === 'Single'"
                type="radio"
                :name="`quiz-correct-${qIdx}`"
                :checked="q.correctIndex === optIdx"
                class="accent-emerald-500 w-4 h-4 cursor-pointer ml-1"
                @change="setSingleCorrect(q, optIdx)"
              />
              <input
                v-else
                type="checkbox"
                :checked="isOptionCorrect(q, optIdx)"
                class="accent-emerald-500 w-4 h-4 cursor-pointer ml-1 rounded"
                @change="toggleMultiCorrect(q, optIdx)"
              />
              <input
                v-model="q.options[optIdx]"
                type="text"
                class="flex-1 px-2.5 py-1 text-xs bg-transparent border-0 rounded text-white placeholder:text-slate-600 focus:outline-none"
                :class="isOptionCorrect(q, optIdx) ? 'font-bold text-emerald-200' : ''"
              />
              <span
                v-if="isOptionCorrect(q, optIdx)"
                class="text-[9px] font-black text-emerald-400 px-1.5 py-0.5 rounded bg-emerald-500/20 border border-emerald-500/30 uppercase tracking-wider shrink-0"
              >
                Đáp án đúng
              </span>
              <button
                v-if="q.options.length > 2"
                type="button"
                class="p-1 text-slate-500 hover:text-rose-400 cursor-pointer transition-colors shrink-0"
                @click="removeOption(qIdx, optIdx)"
              >
                <X class="w-3 h-3" />
              </button>
            </div>
            <button
              v-if="q.options.length < 6"
              type="button"
              class="text-[11px] font-bold text-purple-400 hover:text-purple-300 mt-1 cursor-pointer transition-colors inline-flex items-center gap-1"
              @click="addOption(qIdx)"
            >
              <Plus class="w-3 h-3" />
              <span>Thêm lựa chọn</span>
            </button>
          </div>

          <!-- Explanation -->
          <div>
            <span class="text-[10px] uppercase font-bold text-slate-400 tracking-wider">Giải thích chi tiết sau khi nộp:</span>
            <input
              v-model="q.explanation"
              type="text"
              placeholder="Giải thích tại sao đáp án này đúng..."
              class="w-full mt-1 px-2.5 py-1 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
            />
          </div>
        </div>

        <!-- Sticky Bottom + Câu hỏi Button -->
        <div class="pt-2">
          <button
            type="button"
            class="w-full py-2.5 rounded-xl border border-dashed border-orange-500/40 hover:border-orange-500 bg-orange-500/5 hover:bg-orange-500/10 text-orange-300 text-xs font-black transition-all flex items-center justify-center gap-2 cursor-pointer shadow-sm"
            @click="addQuestion"
          >
            <Plus class="w-4 h-4" />
            <span>+ Thêm câu hỏi trắc nghiệm</span>
          </button>
        </div>
      </div>

      <!-- Lab Editor -->
      <div v-else-if="currentItemType === 'lab'" class="space-y-4">
        <div class="flex items-center justify-between">
          <label class="text-[11px] font-bold text-slate-300 uppercase tracking-wider flex items-center gap-1.5">
            <Code class="w-3.5 h-3.5 text-emerald-400" />
            <span>Cấu hình Codelab</span>
          </label>
        </div>

        <!-- 1. Starter Code First (prominent) -->
        <div>
          <div class="flex items-center justify-between mb-1.5">
            <label class="text-[11px] font-bold text-slate-300 uppercase tracking-wider">
              Mã nguồn khởi đầu (Starter Code)
            </label>
            <span class="text-[10px] font-semibold text-emerald-400 bg-emerald-500/15 border border-emerald-500/30 px-2 py-0.5 rounded-full font-mono">JavaScript (ES6)</span>
          </div>
          <textarea
            v-model="labForm.starterCode"
            rows="12"
            class="w-full px-3.5 py-2.5 font-mono text-xs leading-relaxed bg-[#0e0d16] border border-[#2e2c44] rounded-xl text-emerald-300 focus:outline-none focus:border-purple-500 shadow-inner min-h-[260px]"
          />
        </div>

        <!-- 2. Config Details -->
        <div class="grid grid-cols-2 gap-3 p-3.5 bg-[#171624] border border-[#27253b] rounded-xl">
          <div>
            <label class="block text-[10px] font-bold text-slate-400 uppercase mb-1">Tên hàm thực thi (Entry Function)</label>
            <input
              v-model="labForm.entryFunction"
              type="text"
              class="w-full px-2.5 py-1.5 text-xs font-mono bg-[#0e0d16] border border-[#2e2c44] rounded-md text-emerald-400 focus:outline-none focus:border-purple-500 font-bold"
            />
          </div>
          <div>
            <label class="block text-[10px] font-bold text-slate-400 uppercase mb-1">Điểm tối đa</label>
            <input
              v-model.number="labForm.maxScore"
              type="number"
              class="w-full px-2.5 py-1.5 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-md text-white focus:outline-none focus:border-purple-500 font-bold"
            />
          </div>
        </div>

        <!-- 3. Test Cases -->
        <div class="space-y-2.5">
          <div class="flex items-center justify-between">
            <span class="text-[11px] font-bold text-slate-300 uppercase tracking-wider">Bộ Test Cases ({{ labForm.testCases.length }})</span>
            <button
              type="button"
              class="flex items-center gap-1 px-2.5 py-1 text-xs font-bold text-emerald-300 bg-emerald-500/10 hover:bg-emerald-500/20 border border-emerald-500/30 rounded-lg cursor-pointer transition-colors"
              @click="addTestCase"
            >
              <Plus class="w-3.5 h-3.5" />
              <span>+ Test Case</span>
            </button>
          </div>

          <div
            v-for="(tc, tcIdx) in labForm.testCases"
            :key="tcIdx"
            class="p-3 bg-[#171624] border border-[#27253b] rounded-xl space-y-2 shadow-sm"
          >
            <div class="flex items-center justify-between">
              <span class="text-xs font-black text-emerald-400">Test Case {{ tcIdx + 1 }}</span>
              <div class="flex items-center gap-2">
                <label class="flex items-center gap-1.5 text-[10px] font-bold text-slate-400 cursor-pointer">
                  <input v-model="tc.isHidden" type="checkbox" class="accent-purple-500 w-3.5 h-3.5" />
                  <span>Ẩn (Chấm điểm bí mật)</span>
                </label>
                <button
                  type="button"
                  title="Xóa test case này"
                  class="text-rose-400 hover:text-rose-300 p-1 rounded hover:bg-rose-500/10 cursor-pointer transition-colors"
                  @click="removeTestCase(tcIdx)"
                >
                  <Trash2 class="w-3.5 h-3.5" />
                </button>
              </div>
            </div>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-2">
              <div>
                <span class="text-[9px] uppercase font-bold text-slate-500">Đầu vào (Input JSON)</span>
                <input
                  v-model="tc.input"
                  type="text"
                  placeholder="ví dụ: [1, 2, 3]"
                  class="w-full px-2.5 py-1.5 text-xs font-mono bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
                />
              </div>
              <div>
                <span class="text-[9px] uppercase font-bold text-slate-500">Đầu ra kỳ vọng (Expected Output)</span>
                <input
                  v-model="tc.expected"
                  type="text"
                  placeholder="ví dụ: [3, 2, 1]"
                  class="w-full px-2.5 py-1.5 text-xs font-mono bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
                />
              </div>
            </div>
          </div>

          <!-- 4. Sandbox Test Runner for Teachers -->
          <div class="mt-4 p-3.5 bg-gradient-to-br from-[#181629] to-[#12111d] border border-emerald-500/30 rounded-xl space-y-3 shadow-md">
            <div class="flex items-center justify-between gap-2 flex-wrap">
              <div class="flex items-center gap-2">
                <Terminal class="w-4 h-4 text-emerald-400" />
                <span class="text-xs font-black uppercase tracking-wider text-slate-200">
                  Kiểm tra &amp; Chạy thử Test Cases
                </span>
              </div>
              <button
                type="button"
                :disabled="testingLab"
                class="px-3 py-1.5 rounded-lg bg-emerald-600 hover:bg-emerald-500 text-white text-xs font-bold transition-all cursor-pointer flex items-center gap-1.5 shadow-md shadow-emerald-950/50 disabled:opacity-50"
                @click="runLabTests"
              >
                <Play class="w-3.5 h-3.5" />
                <span>{{ testingLab ? 'Đang chạy thử...' : 'Chạy thử nghiệm' }}</span>
              </button>
            </div>

            <p class="text-[11px] text-slate-400 leading-relaxed">
              Giảng viên có thể chạy thử trực tiếp mã nguồn khởi đầu với bộ Test Cases bên trên để kiểm tra kết quả ngay lập tức.
            </p>

            <!-- Kết quả chạy thử -->
            <div v-if="testRunResults" class="space-y-2 pt-1">
              <div
                v-for="res in testRunResults"
                :key="res.index"
                class="p-2.5 rounded-lg border text-xs space-y-1 font-mono transition-colors"
                :class="res.passed ? 'bg-emerald-500/10 border-emerald-500/40 text-emerald-200' : 'bg-rose-500/10 border-rose-500/40 text-rose-200'"
              >
                <div class="flex items-center justify-between font-bold">
                  <span class="flex items-center gap-1.5">
                    <CheckCircle v-if="res.passed" class="w-3.5 h-3.5 text-emerald-400" />
                    <XCircle v-else class="w-3.5 h-3.5 text-rose-400" />
                    Test Case #{{ res.index }}
                  </span>
                  <span class="text-[10px] px-1.5 py-0.5 rounded font-black uppercase" :class="res.passed ? 'bg-emerald-500/20 text-emerald-300' : 'bg-rose-500/20 text-rose-300'">
                    {{ res.passed ? 'PASS' : 'FAIL' }}
                  </span>
                </div>
                <div class="text-[11px] space-y-0.5 text-slate-300">
                  <div><span class="text-slate-500">Input:</span> {{ res.input }}</div>
                  <div><span class="text-slate-500">Expected:</span> {{ res.expected }}</div>
                  <div>
                    <span class="text-slate-500">Output:</span>
                    <span :class="res.passed ? 'text-emerald-300 font-bold' : 'text-rose-300 font-bold'"> {{ res.actual }}</span>
                    <span v-if="res.error" class="text-rose-400 ml-1">({{ res.error }})</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Sticky Save Bar -->
    <div class="sticky bottom-0 bg-[#171622]/95 backdrop-blur-md border-t border-[#262438] px-4 py-3 flex items-center justify-between gap-3 z-30 shadow-lg shrink-0">
      <div class="flex items-center gap-2 text-xs">
        <span
          v-if="isDirty"
          class="flex items-center gap-1.5 font-bold text-amber-400"
        >
          <span class="w-2 h-2 rounded-full bg-amber-400 animate-pulse" />
          Chưa lưu thay đổi
        </span>
        <span
          v-else
          class="flex items-center gap-1.5 font-semibold text-emerald-400"
        >
          <CheckCircle2 class="w-3.5 h-3.5 text-emerald-400" />
          Đã lưu
        </span>

        <span v-if="currentItemType === 'theory'" class="text-slate-500 hidden sm:inline text-[11px]">
          · {{ theoryWordCount }} từ
        </span>
        <span v-else-if="currentItemType === 'quiz'" class="text-slate-500 hidden sm:inline text-[11px]">
          · {{ quizQuestions.length }} câu · {{ totalQuizPoints }} điểm
        </span>
        <span v-else-if="currentItemType === 'lab'" class="text-slate-500 hidden sm:inline text-[11px]">
          · {{ labForm.testCases.length }} test cases · {{ labForm.maxScore }} điểm
        </span>
      </div>

      <div class="flex items-center gap-2 shrink-0">
        <button
          type="button"
          class="px-3 py-1.5 rounded-xl bg-white/5 hover:bg-white/10 text-slate-300 hover:text-white text-xs font-bold transition-colors cursor-pointer"
          @click="requestClose"
        >
          Đóng
        </button>
        <button
          type="button"
          data-testid="item-editor-save-bottom"
          :disabled="saving"
          class="flex items-center gap-1.5 px-4 py-1.5 rounded-xl bg-purple-600 hover:bg-purple-500 text-white text-xs font-black shadow-md shadow-purple-950/50 transition-colors disabled:opacity-50 cursor-pointer"
          @click="handleSave"
        >
          <Save class="w-3.5 h-3.5" />
          <span>{{ saving ? 'Đang lưu...' : 'Lưu ngay' }}</span>
        </button>
      </div>
    </div>

    <!-- Xác nhận thoát khi còn thay đổi chưa lưu -->
    <div
      v-if="showDirtyConfirm"
      class="absolute inset-0 z-50 bg-black/70 backdrop-blur-sm flex items-center justify-center p-4"
      role="alertdialog"
      aria-modal="true"
      aria-label="Thay đổi chưa được lưu"
    >
      <div class="w-full max-w-sm bg-[#1a1928] border border-amber-500/40 rounded-2xl p-4 space-y-3 shadow-2xl">
        <div class="flex items-center gap-2">
          <AlertCircle class="w-5 h-5 text-amber-400" />
          <h4 class="text-sm font-black text-white">Thay đổi chưa được lưu</h4>
        </div>
        <p class="text-xs text-slate-300 leading-relaxed">
          Bạn đang có thay đổi chưa lưu. Nếu thoát bây giờ, các thay đổi này sẽ bị mất.
        </p>
        <div class="flex items-center justify-end gap-2 pt-1">
          <button
            type="button"
            class="px-3 py-1.5 rounded-lg bg-white/5 hover:bg-white/10 text-slate-200 text-xs font-bold cursor-pointer"
            data-testid="dirty-confirm-reject"
            @click="showDirtyConfirm = false"
          >
            Tiếp tục chỉnh sửa
          </button>
          <button
            type="button"
            data-testid="dirty-confirm-accept"
            class="px-3 py-1.5 rounded-lg bg-rose-600 hover:bg-rose-500 text-white text-xs font-bold cursor-pointer"
            @click="confirmClose"
          >
            Bỏ thay đổi &amp; thoát
          </button>
        </div>
      </div>
    </div>

    <!-- Modal Xem trước và Chọn Mô phỏng cho Giảng viên -->
    <SimulationPickerModal
      :is-open="showSimPicker"
      :initial-key="pickerInitialKey"
      @close="showSimPicker = false"
      @attach="addSimulation"
      @insert="insertSimulationTag"
    />
  </div>
</template>

<style scoped>
.item-editor-slideover {
  min-height: 500px;
}

select option,
select optgroup {
  background-color: #171527 !important;
  color: #f1f5f9 !important;
}

select optgroup {
  color: #c084fc !important;
  font-weight: 700;
}
</style>
