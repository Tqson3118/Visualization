<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue';
import {
  AlertCircle,
  Check,
  CheckCircle2,
  Copy,
  Download,
  FileSpreadsheet,
  HelpCircle,
  Info,
  Plus,
  Puzzle,
  Sparkles,
  Trash2,
  Upload,
  X,
} from 'lucide-vue-next';
import * as XLSX from 'xlsx';

import * as exercisesApi from '@/api/exercises';
import type { ExerciseDto, ExerciseSummaryDto } from '@/api/exercises';
import { useUiStore } from '@/stores/ui';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/Button.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';

export interface InlineQuestionItem {
  id?: number;
  content: string;
  options: string[];
  correctIndex: number;
  explanation: string;
  points: number;
}

const props = defineProps<{
  lessonId: number | null;
  lessonTitle: string;
  modelValue?: InlineQuestionItem[];
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', questions: InlineQuestionItem[]): void;
}>();

const ui = useUiStore();

// Local list of questions
const questions = ref<InlineQuestionItem[]>(
  props.modelValue && props.modelValue.length > 0
    ? JSON.parse(JSON.stringify(props.modelValue))
    : [
        {
          content: 'Độ phức tạp thời gian tốt nhất của thuật toán Bubble Sort (có cờ kiểm tra) là bao nhiêu?',
          options: ['O(N)', 'O(N^2)', 'O(log N)', 'O(1)'],
          correctIndex: 0,
          explanation: 'Khi mảng đã được sắp xếp trước, thuật toán dừng sau 1 lần duyệt kiểm tra O(N).',
          points: 2,
        },
      ],
);

// Sync with modelValue
watch(
  () => props.modelValue,
  (newVal) => {
    if (newVal && JSON.stringify(newVal) !== JSON.stringify(questions.value)) {
      questions.value = JSON.parse(JSON.stringify(newVal));
    }
  },
  { deep: true },
);

// Emit update when local questions change
watch(
  questions,
  () => {
    emit('update:modelValue', JSON.parse(JSON.stringify(questions.value)));
  },
  { deep: true },
);

function isQuestionComplete(q: InlineQuestionItem): boolean {
  const hasContent = q.content.trim().length > 0;
  const validOpts = q.options.filter((o) => o.trim().length > 0);
  return hasContent && validOpts.length >= 2;
}

function addQuestion(): void {
  questions.value.push({
    content: '',
    options: ['', '', '', ''],
    correctIndex: 0,
    explanation: '',
    points: 2,
  });
}

function duplicateQuestion(idx: number): void {
  const target = questions.value[idx];
  if (!target) return;
  questions.value.splice(idx + 1, 0, {
    content: `${target.content} (Bản sao)`,
    options: [...target.options],
    correctIndex: target.correctIndex,
    explanation: target.explanation,
    points: target.points,
  });
  ui.showToast('Đã nhân bản câu hỏi', 'success');
}

function removeQuestion(idx: number): void {
  if (questions.value.length > 1) {
    questions.value.splice(idx, 1);
  }
}


// ── Preset Templates ──
function addSampleQuestions(): void {
  const samples: InlineQuestionItem[] = [
    {
      content: 'Cấu trúc dữ liệu Ngăn xếp (Stack) hoạt động theo nguyên lý nào sau đây?',
      options: ['LIFO (Last In, First Out)', 'FIFO (First In, First Out)', 'LILO (Last In, Last Out)', 'Random Access'],
      correctIndex: 0,
      explanation: 'Stack hoạt động theo cơ chế vào sau ra trước (LIFO).',
      points: 2,
    },
    {
      content: 'Thuật toán sắp xếp nào sau đây KHÔNG có tính ổn định (Not Stable)?',
      options: ['Selection Sort', 'Merge Sort', 'Bubble Sort', 'Insertion Sort'],
      correctIndex: 0,
      explanation: 'Selection Sort có thể hoán đổi các phần tử có cùng giá trị qua khoảng cách xa làm đổi thứ tự ban đầu.',
      points: 2,
    },
    {
      content: 'Cây tìm kiếm nhị phân (BST) có đặc điểm nào dưới đây?',
      options: [
        'Mọi node con bên trái đều nhỏ hơn node cha và bên phải lớn hơn node cha',
        'Mọi node con bên trái đều lớn hơn node cha',
        'Các node luôn được cân bằng hoàn hảo',
        'Là một đồ thị có chu trình kín',
      ],
      correctIndex: 0,
      explanation: 'Quy tắc cơ bản của BST là: Cây con trái < Node cha < Cây con phải.',
      points: 2,
    },
  ];

  questions.value.push(...samples);
  ui.showToast('Đã thêm 3 câu hỏi trắc nghiệm DSA mẫu!', 'success');
}

// ── Import CSV / Excel ──
const fileInputRef = ref<HTMLInputElement | null>(null);
const importModalOpen = ref(false);
const parsedPreviewQuestions = ref<InlineQuestionItem[]>([]);

function triggerFileInput(): void {
  fileInputRef.value?.click();
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
  ui.showToast('Đã tải xuống file mẫu CSV!', 'success');
}

async function onFileSelected(event: Event): Promise<void> {
  const file = (event.target as HTMLInputElement).files?.[0];
  if (!file) return;

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

    const importedList: InlineQuestionItem[] = [];
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

      importedList.push({
        content,
        options: [optA || '', optB || '', optC || '', optD || ''],
        correctIndex: correctIdx,
        explanation,
        points: 2,
      });
    }

    if (importedList.length === 0) {
      ui.showToast('Không tìm thấy câu hỏi đúng cấu trúc trong file.', 'warning');
      return;
    }

    parsedPreviewQuestions.value = importedList;
    importModalOpen.value = true;
  } catch (err: any) {
    ui.showToast(`Lỗi đọc file: ${err?.message || err}`, 'error');
  } finally {
    if (fileInputRef.value) fileInputRef.value.value = '';
  }
}

function confirmImport(): void {
  questions.value.push(...parsedPreviewQuestions.value);
  ui.showToast(`Đã nạp thành công ${parsedPreviewQuestions.value.length} câu hỏi vào bài học!`, 'success');
  importModalOpen.value = false;
  parsedPreviewQuestions.value = [];
}
</script>

<template>
  <div class="quiz-tab flex flex-col h-full p-6 overflow-y-auto max-w-5xl mx-auto space-y-6">
    <!-- Header Bar -->
    <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4 p-5 rounded-2xl bg-gradient-to-r from-purple-950/40 via-vdsa-surface to-purple-950/20 border border-purple-500/30 shadow-lg">
      <div class="flex items-center gap-3.5">
        <div class="w-11 h-11 rounded-xl bg-purple-500/20 text-purple-400 border border-purple-500/30 flex items-center justify-center shrink-0">
          <Puzzle :size="22" />
        </div>
        <div>
          <div class="flex items-center gap-2">
            <h2 class="text-base font-black text-white">Câu hỏi Trắc nghiệm (Mini-Quiz)</h2>
            <Badge variant="primary" class="text-[11px] font-bold">{{ questions.length }} câu</Badge>
          </div>
          <p class="text-xs text-slate-300 mt-0.5">
            Củng cố kiến thức cho học viên ngay sau khi đọc lý thuyết bài học. Tự động lưu kèm bài học.
          </p>
        </div>
      </div>

      <div class="flex items-center gap-2 flex-wrap">
        <input ref="fileInputRef" type="file" accept=".csv, .xlsx, .xls" class="hidden" @change="onFileSelected" />
        <Button size="sm" variant="ghost" class="text-xs" @click="downloadSampleCsv">
          <Download :size="13" /> File mẫu
        </Button>
        <Button size="sm" variant="secondary" class="text-xs gap-1.5" @click="triggerFileInput">
          <Upload :size="13" /> Nhập Excel / CSV
        </Button>
        <Button size="sm" variant="primary" class="text-xs gap-1.5" @click="addQuestion">
          <Plus :size="13" /> Thêm câu hỏi
        </Button>
      </div>
    </div>

    <!-- Quick Templates Bar -->
    <div class="p-3.5 rounded-xl bg-vdsa-surface border border-vdsa-border flex items-center justify-between gap-2 flex-wrap">
      <span class="text-xs font-bold text-slate-300 flex items-center gap-1.5">
        <Sparkles :size="14" class="text-amber-400" /> Điền nhanh câu hỏi mẫu:
      </span>
      <Button size="sm" variant="secondary" class="text-xs py-1 h-7 text-amber-300 border-amber-500/30 hover:bg-amber-500/10" @click="addSampleQuestions">
        <Sparkles :size="13" /> + Thêm 3 câu hỏi mẫu DSA
      </Button>
    </div>

    <!-- Questions Form List -->
    <div class="space-y-4">
      <article
        v-for="(q, idx) in questions"
        :key="idx"
        class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border space-y-4 shadow-md relative group hover:border-purple-500/40 transition-colors"
      >
        <!-- Header Question -->
        <div class="flex items-center justify-between gap-3">
          <div class="flex items-center gap-2.5">
            <span class="w-7 h-7 rounded-lg bg-purple-500/20 text-purple-300 font-mono font-bold text-xs flex items-center justify-center">
              {{ idx + 1 }}
            </span>
            <span class="text-xs font-bold text-slate-200">Câu hỏi #{{ idx + 1 }}</span>
            <span
              v-if="isQuestionComplete(q)"
              class="text-[10px] font-semibold text-emerald-400 bg-emerald-500/15 border border-emerald-500/30 px-2 py-0.5 rounded-full"
            >
              Hoàn thiện ✓
            </span>
            <span
              v-else
              class="text-[10px] font-semibold text-amber-400 bg-amber-500/15 border border-amber-500/30 px-2 py-0.5 rounded-full flex items-center gap-1"
            >
              <AlertCircle :size="11" /> Chưa hoàn thiện
            </span>
          </div>

          <div class="flex items-center gap-2">
            <div class="flex items-center gap-1.5 text-xs text-slate-400">
              <span>Điểm:</span>
              <input
                v-model.number="q.points"
                type="number"
                min="1"
                max="100"
                class="w-14 h-7 rounded bg-slate-900 border border-slate-700 px-2 text-center text-xs text-white focus:outline-none focus:border-purple-500"
              />
            </div>

            <button
              type="button"
              class="text-slate-400 hover:text-sky-300 p-1.5 rounded-lg hover:bg-sky-500/10 transition-colors"
              title="Nhân bản câu hỏi này"
              @click="duplicateQuestion(idx)"
            >
              <Copy :size="14" />
            </button>

            <button
              v-if="questions.length > 1"
              type="button"
              class="text-slate-500 hover:text-rose-400 p-1.5 rounded-lg hover:bg-rose-500/10 transition-colors"
              title="Xóa câu hỏi này"
              @click="removeQuestion(idx)"
            >
              <Trash2 :size="15" />
            </button>
          </div>
        </div>

        <!-- Question Content -->
        <div>
          <label class="block text-xs font-bold text-slate-300 mb-1">Nội dung câu hỏi <span class="text-rose-400">*</span></label>
          <textarea
            v-model="q.content"
            rows="2"
            class="w-full rounded-xl border border-slate-700 bg-slate-900 p-2.5 text-xs text-white focus:outline-none focus:ring-1 focus:ring-purple-500 font-sans leading-relaxed"
            placeholder="Nhập nội dung câu hỏi trắc nghiệm..."
          ></textarea>
        </div>

        <!-- 4 Options Grid -->
        <div class="space-y-2">
          <label class="block text-xs font-bold text-slate-300">
            Các lựa chọn đáp án & Chọn đáp án đúng:
          </label>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-2.5">
            <div
              v-for="(opt, optIdx) in q.options"
              :key="optIdx"
              class="flex items-center gap-2 p-2 rounded-xl border transition-all"
              :class="q.correctIndex === optIdx ? 'bg-emerald-950/20 border-emerald-500/60 ring-1 ring-emerald-500/30' : 'bg-slate-900/60 border-slate-800'"
            >
              <button
                type="button"
                class="w-7 h-7 rounded-lg text-xs font-bold flex items-center justify-center shrink-0 transition-colors cursor-pointer"
                :class="q.correctIndex === optIdx ? 'bg-emerald-600 text-white shadow' : 'bg-slate-800 text-slate-400 hover:text-white'"
                :title="q.correctIndex === optIdx ? 'Đáp án đúng' : 'Bấm để chọn làm đáp án đúng'"
                @click="q.correctIndex = optIdx"
              >
                {{ String.fromCharCode(65 + optIdx) }}
              </button>

              <input
                v-model="q.options[optIdx]"
                type="text"
                :placeholder="`Nhập nội dung lựa chọn ${String.fromCharCode(65 + optIdx)}...`"
                class="flex-1 bg-transparent border-none outline-none text-xs text-slate-100 placeholder-slate-500"
              />

              <Check v-if="q.correctIndex === optIdx" :size="14" class="text-emerald-400 shrink-0" />
            </div>
          </div>
        </div>

        <!-- Explanation -->
        <div>
          <label class="block text-[11px] font-bold text-slate-400 mb-1 flex items-center gap-1">
            <Info :size="12" class="text-purple-400" /> Giải thích chi tiết (Hiển thị cho học viên sau khi nộp bài)
          </label>
          <input
            v-model="q.explanation"
            type="text"
            placeholder="Giải thích vì sao đáp án trên là chính xác..."
            class="w-full h-8 rounded-lg border border-slate-800 bg-slate-950 px-3 text-xs text-slate-300 focus:outline-none focus:border-purple-500"
          />
        </div>
      </article>

      <Button variant="secondary" size="md" class="w-full py-3 border-dashed border-slate-700 hover:border-purple-500 text-slate-300 hover:text-white" @click="addQuestion">
        <Plus :size="16" /> + Thêm câu hỏi tiếp theo
      </Button>
    </div>

    <!-- Modal Preview Import CSV -->
    <Modal :open="importModalOpen" title="Xem trước câu hỏi nhập từ file" size="lg" @close="importModalOpen = false">
      <div class="space-y-4 max-h-[70vh] overflow-y-auto p-4 bg-[#090d16] rounded-xl text-slate-200">
        <p class="text-xs text-slate-400">
          Tìm thấy <strong class="text-white">{{ parsedPreviewQuestions.length }}</strong> câu hỏi hợp lệ trong file. Xác nhận nạp vào bài học?
        </p>

        <div class="space-y-2">
          <div
            v-for="(pq, pIdx) in parsedPreviewQuestions"
            :key="pIdx"
            class="p-3 rounded-xl bg-slate-900 border border-slate-800 text-xs space-y-1.5"
          >
            <div class="font-bold text-white">#{{ pIdx + 1 }}. {{ pq.content }}</div>
            <div class="grid grid-cols-2 gap-2 text-slate-300">
              <div v-for="(o, oIdx) in pq.options" :key="oIdx" :class="{ 'text-emerald-400 font-bold': pq.correctIndex === oIdx }">
                {{ String.fromCharCode(65 + oIdx) }}. {{ o }}
              </div>
            </div>
          </div>
        </div>

        <div class="flex justify-end gap-2 pt-2 border-t border-slate-800">
          <Button variant="ghost" size="sm" @click="importModalOpen = false">Hủy bỏ</Button>
          <Button variant="primary" size="sm" class="bg-emerald-600 hover:bg-emerald-500" @click="confirmImport">
            Nạp {{ parsedPreviewQuestions.length }} câu hỏi vào bài
          </Button>
        </div>
      </div>
    </Modal>
  </div>
</template>
