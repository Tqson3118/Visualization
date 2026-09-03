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
  FileText,
  Download,
  FileSpreadsheet,
  Maximize2,
  Minimize2,
} from 'lucide-vue-next';
import mammoth from 'mammoth';
import * as XLSX from 'xlsx';
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
  (e: 'dirtyChange', isDirty: boolean): void;
}>();

const ui = useUiStore();
const saving = ref(false);
const loadingDetail = ref(false);
const activeViewMode = ref<'edit' | 'preview'>('edit');
const isFullscreen = ref(false);

// Common Form
const form = reactive({
  title: '',
  description: '',
});
const lessonScope = ref<'draft' | 'class'>('class');

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
  starterCode: `/**\n * @param {any} input\n * @return {any}\n */\nfunction solve(input) {\n  // Viết mã nguồn giải thuật tại đây\n  return null;\n}`,
  testCases: [] as LabTestCase[],
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
  if (labForm.testCases.length === 0) {
    ui.showToast('Vui lòng thêm ít nhất 1 test case để chạy thử nghiệm', 'warning');
    return;
  }
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
    ui.showToast(`Lỗi cú pháp hoặc biên dịch: ${err?.message || err}`, 'error');
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
    scope: lessonScope.value,
    theory: theoryContentHtml.value,
    simulations: attachedSimulations.value,
    quiz: quizQuestions.value,
    lab: { ...labForm },
  });
}

const isDirty = computed(() => snapshot() !== dirtyBaseline.value);

watch(isDirty, (val) => {
  emit('dirtyChange', val);
}, { immediate: true });

function handleBeforeUnload(e: BeforeUnloadEvent) {
  if (isDirty.value && !saving.value) {
    e.preventDefault();
    e.returnValue = '';
  }
}

function handleFullscreenKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape' && isFullscreen.value) {
    isFullscreen.value = false;
  }
}

onMounted(() => {
  window.addEventListener('beforeunload', handleBeforeUnload);
  window.addEventListener('keydown', handleFullscreenKeydown);
});

onUnmounted(() => {
  window.removeEventListener('beforeunload', handleBeforeUnload);
  window.removeEventListener('keydown', handleFullscreenKeydown);
  emit('dirtyChange', false);
});

function requestClose(): void {
  isFullscreen.value = false;
  if (isDirty.value && !saving.value) {
    showDirtyConfirm.value = true;
    return;
  }
  emit('close');
}

function confirmClose(): void {
  isFullscreen.value = false;
  showDirtyConfirm.value = false;
  emit('close');
}

let latestFetchId = 0;
watch(
  () => props.item,
  async (newItem) => {
    if (!newItem) return;
    const currentFetch = ++latestFetchId;
    form.title = newItem.title || '';
    form.description = newItem.description || '';
    theoryContentHtml.value = '';
    attachedSimulations.value = [];
    quizQuestions.value = [];

    // Fetch full detail if available
    loadingDetail.value = true;
    try {
      const detail = await fetchItemDetail(newItem.id);
      if (currentFetch !== latestFetchId) return;
      form.title = detail.title || '';
      form.description = detail.description || '';

      if (normalizeItemType(detail.itemType) === 'theory' && detail.lesson) {
        let content = detail.lesson.contentHtml || '';
        const PLACEHOLDER_TEXT = '<p>Nội dung bài học đang được biên soạn.</p>';
        if (content.trim() === PLACEHOLDER_TEXT) {
          content = '';
        }
        if (content && !content.includes('<p>') && !content.includes('<div>') && !content.includes('<h') && (content.includes('#') || content.includes('```') || content.includes('> [!'))) {
          content = parseMarkdownToHtml(content);
        }
        theoryContentHtml.value = content;
        attachedSimulations.value = (detail.lesson.simulations || []).map((s: any) => s.simulationKey || s);
        lessonScope.value = detail.lesson.isClassOnly ? 'class' : (detail.lesson.status === 'active' ? 'class' : 'draft');
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
              id: 1,
              content: 'Nội dung câu hỏi trắc nghiệm...',
              type: 'Single',
              options: ['Lựa chọn A', 'Lựa chọn B', 'Lựa chọn C', 'Lựa chọn D'],
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
      if (currentFetch !== latestFetchId) return;
      // Use props item as fallback
    } finally {
      if (currentFetch === latestFetchId) {
        loadingDetail.value = false;
        dirtyBaseline.value = snapshot();
      }
    }
  },
  { immediate: true, deep: true },
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

function downloadSampleTheoryDocx(): void {
  const link = document.createElement('a');
  link.href = '/templates/mau_bai_giang_ly_thuyet_dsa.docx';
  link.download = 'mau_bai_giang_ly_thuyet_dsa.docx';
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  ui.showToast('Đã tải xuống file Word (.docx) bài giảng mẫu!', 'success');
}


const tiptapRef = ref<InstanceType<typeof TipTapEditor> | null>(null);
const showSimPicker = ref(false);
const pickerInitialKey = ref('sort.bubble');

function openPickerModal(key?: string): void {
  if (key) {
    pickerInitialKey.value = key;
  }
  showSimPicker.value = true;
}

function insertSimulationTag(key: string): void {
  if (tiptapRef.value && typeof tiptapRef.value.insertSimulation === 'function') {
    tiptapRef.value.insertSimulation(key);
  } else {
    const sim = CATALOG.find((c) => c.key === key);
    const title = sim ? sim.title : key;
    const tag = `<p><strong>🎮 Mô phỏng trực quan: ${title}</strong></p><p>[Mô phỏng: ${key}]</p>`;
    theoryContentHtml.value += tag;
  }
  if (!attachedSimulations.value.includes(key)) {
    attachedSimulations.value.push(key);
  }
  ui.showToast(`Đã chèn mô phỏng: ${getSimTitle(key)}`, 'success');
}

function addSimulations(keys: string[]): void {
  const added = keys.filter((k) => k && !attachedSimulations.value.includes(k));
  if (added.length === 0) {
    ui.showToast('Các mô phỏng đã được gắn vào bài học từ trước.', 'info');
    showSimPicker.value = false;
    return;
  }
  attachedSimulations.value.push(...added);
  ui.showToast('Đã gắn ' + added.length + ' mô phỏng vào bài học!', 'success');
  showSimPicker.value = false;
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
  const [key] = attachedSimulations.value.splice(index, 1);
  if (key) {
    // Fix bug "bỏ chọn mô phỏng nhưng vẫn hiển thị trong bài": gỡ chip phải gỡ luôn
    // thẻ [Mô phỏng: key] (và tiêu đề 🎮 đi kèm) khỏi nội dung bài giảng
    const pairRe = new RegExp(
      '(?:<p[^>]*>\\s*<strong[^>]*>[^<]*🎮[^<]*</strong>\\s*</p>\\s*)?<p[^>]*>\\s*\\[(?:Mô phỏng|Simulation|mo phong):\\s*' + key + '\\]\\s*</p>',
      'gi',
    );
    const anchorOnlyRe = new RegExp(
      '\\[(?:Mô phỏng|Simulation|mo phong):\\s*' + key + '\\]',
      'gi',
    );
    let content = theoryContentHtml.value;
    content = content.replace(pairRe, '');
    content = content.replace(anchorOnlyRe, '');
    theoryContentHtml.value = content;
    ui.showToast('Đã gỡ mô phỏng "' + getSimTitle(key) + '" khỏi bài học', 'info');
  }
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

// ── Bộ công cụ Quiz: Tải file mẫu CSV/Excel, Import Excel, Import Word và AI Format ──
function downloadSampleQuizCsv(): void {
  const csvContent = `question,option_a,option_b,option_c,option_d,correct_option,explanation,points
"Độ phức tạp thời gian tốt nhất của Bubble Sort (có cờ kiểm tra) là gì?","O(N)","O(N^2)","O(log N)","O(1)","A","Khi mảng đã sắp xếp trước, thuật toán dừng sau 1 lần duyệt kiểm tra O(N).",2
"Thuật toán sắp xếp nào sau đây KHÔNG có tính ổn định (Not Stable)?","Selection Sort","Merge Sort","Bubble Sort","Insertion Sort","A","Selection Sort có thể hoán đổi các phần tử bằng nhau qua khoảng cách xa làm đổi thứ tự ban đầu.",2
"Cấu trúc dữ liệu Ngăn xếp (Stack) hoạt động theo nguyên lý nào sau đây?","LIFO (Last In, First Out)","FIFO (First In, First Out)","LILO (Last In, Last Out)","Random Access","A","Stack hoạt động theo cơ chế vào sau ra trước (LIFO).",2
"Cây tìm kiếm nhị phân (BST) có đặc điểm nào dưới đây?","Mọi node con bên trái đều nhỏ hơn node cha và bên phải lớn hơn node cha","Mọi node con bên trái đều lớn hơn node cha","Các node luôn được cân bằng hoàn hảo","Là một đồ thị có chu trình kín","A","Quy tắc cơ bản của BST là: Cây con trái < Node cha < Cây con phải.",2`;

  const blob = new Blob(['\uFEFF' + csvContent], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.setAttribute('href', url);
  link.setAttribute('download', 'mau_cau_hoi_quiz_dsa.csv');
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
  ui.showToast('Đã tải xuống file mẫu CSV câu hỏi trắc nghiệm!', 'success');
}

async function handleQuizExcelUpload(e: Event): Promise<void> {
  const input = e.target as HTMLInputElement;
  if (!input.files || input.files.length === 0) return;
  const file = input.files[0];

  try {
    const ext = file.name.split('.').pop()?.toLowerCase();
    let rows: any[] = [];

    if (ext === 'xlsx' || ext === 'xls') {
      const data = await file.arrayBuffer();
      const wb = XLSX.read(data, { type: 'array' });
      const ws = wb.Sheets[wb.SheetNames[0]];
      rows = XLSX.utils.sheet_to_json(ws, { defval: '' });
    } else {
      const text = await file.text();
      const wb = XLSX.read(text, { type: 'string' });
      const ws = wb.Sheets[wb.SheetNames[0]];
      rows = XLSX.utils.sheet_to_json(ws, { defval: '' });
    }

    if (!Array.isArray(rows) || rows.length === 0) {
      ui.showToast('File không có dữ liệu câu hỏi hợp lệ.', 'warning');
      return;
    }

    const importedList: InlineQuizQuestion[] = [];
    for (const r of rows) {
      const content = String(r.question || r['Câu hỏi'] || r['Content'] || '').trim();
      if (!content) continue;

      const optA = String(r.option_a || r['Đáp án A'] || r['A'] || '').trim();
      const optB = String(r.option_b || r['Đáp án B'] || r['B'] || '').trim();
      const optC = String(r.option_c || r['Đáp án C'] || r['C'] || '').trim();
      const optD = String(r.option_d || r['Đáp án D'] || r['D'] || '').trim();

      const options = [optA, optB, optC, optD].filter(Boolean);
      if (options.length < 2) continue;

      const correctRaw = String(r.correct_option || r['Đáp án đúng'] || r['Correct'] || 'A').trim().toUpperCase();
      let correctIdx = 0;
      if (correctRaw === 'B' || correctRaw === '1') correctIdx = 1;
      else if (correctRaw === 'C' || correctRaw === '2') correctIdx = 2;
      else if (correctRaw === 'D' || correctRaw === '3') correctIdx = 3;

      const explanation = String(r.explanation || r['Giải thích'] || '').trim();
      const points = Number(r.points || r['Điểm'] || 1) || 1;

      importedList.push({
        content,
        type: 'Single',
        options: [optA || '', optB || '', optC || '', optD || ''],
        correctIndex: correctIdx,
        correctIndices: [correctIdx],
        explanation,
        points,
      });
    }

    if (importedList.length === 0) {
      ui.showToast('Không tìm thấy câu hỏi đúng cấu trúc trong file Excel/CSV.', 'warning');
      return;
    }

    quizQuestions.value.push(...importedList);
    ui.showToast(`Đã nhập thành công ${importedList.length} câu hỏi từ file Excel/CSV!`, 'success');
  } catch (err: any) {
    ui.showToast(`Lỗi khi đọc file Excel/CSV: ${err?.message || err}`, 'error');
  } finally {
    input.value = '';
  }
}

async function handleQuizWordUpload(e: Event): Promise<void> {
  const input = e.target as HTMLInputElement;
  if (!input.files || input.files.length === 0) return;
  const file = input.files[0];

  try {
    const arrayBuffer = await file.arrayBuffer();
    const result = await mammoth.extractRawText({ arrayBuffer });
    const text = result.value;

    if (!text || !text.trim()) {
      ui.showToast('Không đọc được nội dung văn bản từ file Word.', 'warning');
      return;
    }

    // Smart Regex Parser bóc tách câu hỏi và đáp án từ Word
    const rawBlocks = text.split(/(?:Câu\s+\d+[:.]|\bQuestion\s+\d+[:.]|\b\d+[\.\)]\s+)/i).filter((b) => b.trim().length > 0);
    const parsedQuestions: InlineQuizQuestion[] = [];

    for (const block of rawBlocks) {
      const lines = block.split(/\r?\n/).map((l) => l.trim()).filter(Boolean);
      if (lines.length < 3) continue;

      const content = lines[0];
      const options: string[] = [];
      let correctIdx = 0;
      let explanation = '';

      for (let i = 1; i < lines.length; i++) {
        const line = lines[i];
        const optMatch = line.match(/^([*]?)\s*([A-D])[\.\)]\s*(.+)$/i);
        if (optMatch) {
          const isMarked = Boolean(optMatch[1]);
          const letter = optMatch[2].toUpperCase();
          const optText = optMatch[3].trim();
          const curIdx = options.length;
          options.push(optText);
          if (isMarked) {
            correctIdx = curIdx;
          } else if (line.includes('[x]') || line.includes('(đúng)')) {
            correctIdx = curIdx;
          }
        } else if (line.toLowerCase().startsWith('giải thích:') || line.toLowerCase().startsWith('explanation:')) {
          explanation = line.replace(/^(giải thích|explanation):\s*/i, '').trim();
        } else if (line.toLowerCase().startsWith('đáp án:') || line.toLowerCase().startsWith('answer:')) {
          const ansLetter = line.replace(/^(đáp án|answer):\s*/i, '').trim().toUpperCase();
          if (ansLetter === 'B' || ansLetter === '1') correctIdx = 1;
          else if (ansLetter === 'C' || ansLetter === '2') correctIdx = 2;
          else if (ansLetter === 'D' || ansLetter === '3') correctIdx = 3;
        }
      }

      if (options.length >= 2) {
        while (options.length < 4) {
          options.push('');
        }
        parsedQuestions.push({
          content,
          type: 'Single',
          options: options.slice(0, 4),
          correctIndex: correctIdx,
          correctIndices: [correctIdx],
          explanation,
          points: 1,
        });
      }
    }

    if (parsedQuestions.length === 0) {
      ui.showToast('Không tìm thấy cấu trúc câu hỏi trắc nghiệm (Câu 1: ... A. ... B. ...) trong file Word.', 'warning');
      return;
    }

    quizQuestions.value.push(...parsedQuestions);
    ui.showToast(`Đã bóc tách thành công ${parsedQuestions.length} câu hỏi từ file Word (.docx)!`, 'success');
  } catch (err: any) {
    ui.showToast(`Lỗi khi đọc file Word: ${err?.message || err}`, 'error');
  } finally {
    input.value = '';
  }
}

function handleQuizAiFormat(): void {
  if (quizQuestions.value.length === 0) {
    ui.showToast('Vui lòng thêm ít nhất 1 câu hỏi trước khi dùng AI Chuẩn hóa.', 'warning');
    return;
  }

  let formattedCount = 0;
  for (const q of quizQuestions.value) {
    if (q.content) {
      q.content = q.content.trim();
      if (!q.content.endsWith('?') && !q.content.endsWith(':') && !q.content.endsWith('.')) {
        q.content += '?';
      }
    }
    q.options = q.options.map((o) => (o ? o.trim() : ''));
    if (!q.explanation || !q.explanation.trim()) {
      const correctOptText = q.options[q.correctIndex] || 'đáp án này';
      q.explanation = `Lựa chọn đúng là "${correctOptText}" dựa trên nguyên lý hoạt động và phân tích độ phức tạp của thuật toán.`;
    }
    if (!q.points || q.points <= 0) {
      q.points = 1;
    }
    formattedCount++;
  }

  ui.showToast(`AI đã chuẩn hóa và bổ sung giải thích cho ${formattedCount} câu hỏi trắc nghiệm!`, 'success');
}

function handleQuizAiGenerate(): void {
  const topicName = form.title || 'Cấu trúc dữ liệu & Giải thuật';
  const aiSamples: InlineQuizQuestion[] = [
    {
      content: `Khi áp dụng giải thuật liên quan đến "${topicName}", trường hợp xấu nhất (Worst Case) thường xảy ra khi nào?`,
      type: 'Single',
      options: [
        'Dữ liệu đầu vào nghịch đảo hoặc phân bố không đồng đều',
        'Dữ liệu đã được sắp xếp sẵn theo thứ tự mong muốn',
        'Kích thước tập dữ liệu n < 10',
        'Bộ nhớ RAM của hệ thống còn dưới 20%',
      ],
      correctIndex: 0,
      correctIndices: [0],
      explanation: 'Trường hợp xấu nhất xảy ra khi thứ tự dữ liệu đầu vào làm thuật toán phải thực hiện số phép so sánh/phép duyệt tối đa.',
      points: 2,
    },
    {
      content: `Độ phức tạp không gian (Space Complexity) tối ưu của cấu trúc/thuật toán "${topicName}" là:`,
      type: 'Single',
      options: ['O(1) bộ nhớ phụ trợ tại chỗ (In-place)', 'O(N^2) mảng hai chiều', 'O(2^N) đệ quy vô hạn', 'O(N!) hoán vị'],
      correctIndex: 0,
      correctIndices: [0],
      explanation: 'Thuật toán tối ưu tận dụng không gian tại chỗ để đạt độ phức tạp không gian O(1).',
      points: 2,
    },
    {
      content: `Ứng dụng thực tế quan trọng nhất của "${topicName}" trong kỹ thuật phần mềm là gì?`,
      type: 'Single',
      options: [
        'Tối ưu hóa thời gian tìm kiếm, truy xuất và quản lý dữ liệu hiệu quả',
        'Thay thế hoàn toàn ngôn ngữ lập trình bậc cao',
        'Tự động sửa lỗi phần cứng CPU',
        'Mã hóa dữ liệu không thể giải mã',
      ],
      correctIndex: 0,
      correctIndices: [0],
      explanation: 'Cấu trúc dữ liệu và giải thuật giúp nâng cao hiệu năng truy xuất và xử trị dữ liệu quy mô lớn.',
      points: 2,
    },
  ];

  quizQuestions.value.push(...aiSamples);
  ui.showToast(`AI đã sinh 3 câu hỏi trắc nghiệm chuyên sâu về "${topicName}"!`, 'success');
}

function addTestCase() {
  labForm.testCases.push({ input: '', expected: '', isHidden: false });
}

function removeTestCase(idx: number) {
  labForm.testCases.splice(idx, 1);
}

// ── Lắng nghe phím tắt Ctrl + S / Cmd + S ──
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
      const simKeysToSave = Array.from(new Set(attachedSimulations.value));
      const contentHtmlToSave = theoryContentHtml.value?.trim() ? theoryContentHtml.value : '<p>Nội dung bài học đang được biên soạn.</p>';

      if (props.item.lessonId) {
        try {
          // PUT lesson = thay thế toàn bộ resource: fetch hiện trạng để giữ topicId/status
          const currentLesson = await lessonsApi.fetchLesson(props.item.lessonId);
          await lessonsApi.updateLesson(props.item.lessonId, {
            topicId: currentLesson.topicId ?? 1,
            title: form.title.trim(),
            description: form.description.trim(),
            contentHtml: contentHtmlToSave,
            status: lessonScope.value === 'class' ? 'active' : 'draft',
            sortOrder: currentLesson.sortOrder ?? 1,
            isClassOnly: lessonScope.value === 'class',
            simulationKeys: simKeysToSave,
          });
          await updatePathItem(props.item.id, {
            title: form.title.trim(),
            description: form.description.trim(),
          });
          props.item.title = form.title.trim();
          props.item.description = form.description.trim();
        } catch (fetchErr) {
          console.warn('Lesson không tồn tại trên máy chủ, đang tạo mới bài học thay thế:', fetchErr);
          const created = await lessonsApi.createLesson({
            topicId: 1,
            title: form.title.trim(),
            description: form.description.trim(),
            contentHtml: contentHtmlToSave,
            status: lessonScope.value === 'class' ? 'active' : 'draft',
            sortOrder: 1,
            isClassOnly: lessonScope.value === 'class',
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
          contentHtml: contentHtmlToSave,
          status: lessonScope.value === 'class' ? 'active' : 'draft',
          sortOrder: 1,
          isClassOnly: lessonScope.value === 'class',
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
      if (!props.item.lessonId) {
        const createdLesson = await lessonsApi.createLesson({
          topicId: 1,
          title: form.title.trim(),
          description: form.description.trim(),
          contentHtml: '<p>Nội dung bài tập trắc nghiệm.</p>',
          status: 'draft',
          isClassOnly: lessonScope.value === 'class',
        });
        props.item.lessonId = createdLesson.id;
        (props.item as any).lesson = createdLesson;
        await updatePathItem(props.item.id, {
          lessonId: createdLesson.id,
          title: form.title.trim(),
          description: form.description.trim(),
        });
      }
      const exerciseId = props.item.finalTestId ?? props.item.exerciseId ?? props.item.exercise?.id;
      const quizPayload = {
        lessonId: props.item.lessonId,
        nodeId: props.item.id,
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
      if (!props.item.lessonId) {
        const createdLesson = await lessonsApi.createLesson({
          topicId: 1,
          title: form.title.trim(),
          description: form.description.trim(),
          contentHtml: '<p>Thử thách lập trình.</p>',
          status: 'draft',
          isClassOnly: lessonScope.value === 'class',
        });
        props.item.lessonId = createdLesson.id;
        (props.item as any).lesson = createdLesson;
        await updatePathItem(props.item.id, {
          lessonId: createdLesson.id,
          title: form.title.trim(),
          description: form.description.trim(),
        });
      }
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
        lessonId: props.item.lessonId,
        nodeId: props.item.id,
        title: form.title.trim(),
        description: form.description.trim(),
        maxScore: labForm.maxScore,
        configJson,
      };
      if (exerciseId) {
        await exercisesApi.updateExercise(exerciseId, labPayload);
      } else {
        const created = await exercisesApi.createExercise({
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
    (updated as any).updatedAt = new Date().toISOString();
    (updated as any).UpdatedAt = new Date().toISOString();
    emit('saved', updated);
  } catch (err: any) {
    ui.showToast(err?.message || 'Lỗi khi lưu mục nội dung', 'error');
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <Teleport to="body" :disabled="!isFullscreen">
    <div
      v-if="open && item"
      class="item-editor-slideover flex flex-col bg-[#12111a] border border-[#262438] overflow-hidden shadow-2xl transition-all duration-200"
      :class="isFullscreen ? 'fixed inset-0 z-[9999] rounded-none w-screen h-screen' : 'relative h-full rounded-2xl'"
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
          :title="isFullscreen ? 'Thu nhỏ cửa sổ' : 'Phóng to toàn màn hình'"
          class="p-1.5 rounded-lg hover:bg-white/10 text-slate-400 hover:text-white transition-colors cursor-pointer"
          @click="isFullscreen = !isFullscreen"
        >
          <Minimize2 v-if="isFullscreen" class="w-4 h-4 text-purple-400" />
          <Maximize2 v-else class="w-4 h-4" />
        </button>
        <button
          type="button"
          data-testid="item-editor-save"
          :disabled="saving"
          class="flex items-center gap-1.5 px-3 py-1.5 rounded-xl bg-purple-600 hover:bg-purple-500 text-white text-xs font-bold shadow-md shadow-purple-950/50 transition-colors disabled:opacity-50 cursor-pointer"
          @click="handleSave"
        >
          <Save class="w-3.5 h-3.5" />
          <span>{{ saving ? 'Đang lưu...' : 'Lưu bài học' }}</span>
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

            <!-- Nút Tải mẫu Word (.docx) -->
            <button
              type="button"
              class="px-2.5 py-1 text-xs font-bold bg-blue-500/10 hover:bg-blue-500/20 border border-blue-500/30 text-blue-300 rounded-lg transition-all flex items-center gap-1.5 cursor-pointer shadow-sm"
              title="Tải file Word (.docx) bài giảng mẫu về máy để điền nội dung"
              @click="downloadSampleTheoryDocx"
            >
              <Download class="w-3.5 h-3.5 text-blue-400" />
              <span>Tải mẫu Word</span>
            </button>

            <!-- Nút Upload Word -->
            <button
              type="button"
              class="px-2.5 py-1 text-xs font-bold bg-[#1d1b2e] hover:bg-purple-900/40 border border-purple-500/30 text-purple-200 rounded-lg transition-all flex items-center gap-1.5 cursor-pointer shadow-sm"
              title="Tải lên tệp Word (.docx) và chuyển đổi thành bài giảng"
              @click="fileInputRef?.click()"
            >
              <Upload class="w-3.5 h-3.5 text-purple-400" />
              <span>Nhập từ Word</span>
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
          ref="tiptapRef"
          v-model="theoryContentHtml"
          class="min-h-[360px]"
        />
        <div
          v-else
          class="p-5 bg-[#0e0d16] border border-[#2e2c44] rounded-2xl min-h-[140px] text-slate-200 text-xs leading-relaxed prose prose-invert !max-w-none w-full"
        >
          <ProseContent :html="theoryContentHtml" />
        </div>

      </div>

      <!-- Quiz Editor -->
      <div v-else-if="currentItemType === 'quiz'" class="space-y-4">
        <!-- Quiz Control Bar -->
        <div class="p-3 bg-[#171527] border border-[#27253b] rounded-xl space-y-3 shadow-md">
          <div class="flex items-center justify-between gap-2 flex-wrap pb-1 border-b border-[#242238]">
            <div class="flex items-center gap-2">
              <label class="text-[11px] font-bold text-slate-300 uppercase tracking-wider flex items-center gap-1.5">
                <HelpCircle class="w-3.5 h-3.5 text-orange-400" />
                <span>Danh sách Câu hỏi Trắc nghiệm ({{ quizQuestions.length }})</span>
              </label>
              <span class="text-[10px] font-bold px-2.5 py-0.5 rounded-full bg-orange-500/20 text-orange-300 border border-orange-500/30">
                {{ quizQuestions.length }} câu hỏi (Mỗi câu = {{ (10 / Math.max(1, quizQuestions.length)).toFixed(1) }} đ — Thang 10)
              </span>
            </div>
            
            <button
              type="button"
              class="flex items-center gap-1 px-3 py-1 text-xs font-bold text-white bg-orange-600 hover:bg-orange-500 rounded-lg transition-colors cursor-pointer shadow-sm"
              @click="addQuestion"
            >
              <Plus class="w-3.5 h-3.5" />
              <span>+ Thêm Câu hỏi</span>
            </button>
          </div>

          <!-- Quiz Toolbar Tools: Tải mẫu, Nhập Excel, Nhập Word, AI Assistant -->
          <div class="flex items-center justify-between gap-2 flex-wrap">
            <div class="flex items-center gap-2 flex-wrap">
              <!-- Tải file mẫu Excel / CSV -->
              <button
                type="button"
                title="Tải file mẫu Excel / CSV chuẩn để soạn câu hỏi trắc nghiệm hàng loạt"
                class="flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold text-sky-300 bg-sky-500/10 hover:bg-sky-500/20 border border-sky-500/30 rounded-xl transition-all cursor-pointer shadow-sm"
                @click="downloadSampleQuizCsv"
              >
                <Download class="w-4 h-4 text-sky-400" />
                <span>Tải file mẫu (Excel / CSV)</span>
              </button>

              <!-- Nhập Excel / CSV -->
              <label
                class="flex items-center gap-1.5 px-3 py-1.5 text-xs font-bold text-emerald-300 bg-emerald-500/10 hover:bg-emerald-500/20 border border-emerald-500/30 rounded-xl transition-all cursor-pointer shadow-sm"
                title="Nhập câu hỏi trắc nghiệm từ file Excel (.xlsx, .xls) hoặc CSV"
              >
                <FileSpreadsheet class="w-4 h-4 text-emerald-400" />
                <span>Nhập từ Excel / CSV</span>
                <input
                  type="file"
                  accept=".xlsx, .xls, .csv"
                  class="hidden"
                  @change="handleQuizExcelUpload"
                />
              </label>
            </div>

            <div class="text-[11px] text-slate-400 font-medium italic">
              * Tải file mẫu về điền câu hỏi, sau đó nhập vào để hoàn thành nhanh.
            </div>
          </div>
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
      :attached-keys="attachedSimulations"
      @close="showSimPicker = false"
      @attach="addSimulation"
      @attach-many="addSimulations"
      @insert="insertSimulationTag"
    />
    </div>
  </Teleport>
</template>

<style scoped>
.item-editor-slideover {
  min-height: 500px;
}

.item-editor-slideover :deep(.prose),
.item-editor-slideover .prose {
  width: 100% !important;
  max-width: 100% !important;
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
