<script setup lang="ts">
import { ref, reactive, watch, computed } from 'vue';
import { Plus, Trash2, HelpCircle, Code, FlaskConical, Upload, Check, FileText, Download } from 'lucide-vue-next';
import { createExercise, updateExercise, importExerciseCsv, type ExerciseSummaryDto, type ExerciseUpsertPayload, type QuestionUpsertDto } from '@/api/exercises';
import type { LessonSummary } from '@/api/lessons';
import { useUiStore } from '@/stores/ui';
import Modal from '@/components/ui/Modal.vue';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';
import Badge from '@/components/ui/Badge.vue';

const props = defineProps<{
  open: boolean;
  exerciseId?: number | null;
  defaultNodeId?: number | null;
  defaultStage?: number | null;
  defaultTab?: 'quiz' | 'code' | 'import-csv' | null;
  lessons: Array<{ id: number; title: string }>;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'saved'): void;
}>();

const ui = useUiStore();

const activeTab = ref<'quiz' | 'code' | 'import-csv'>('quiz');
const saving = ref(false);
const importing = ref(false);
const csvFile = ref<File | null>(null);

function downloadSampleCsv(): void {
  const csvContent = `question,option_a,option_b,option_c,option_d,correct_option,explanation
"Độ phức tạp thời gian tốt nhất của Bubble Sort là gì?","O(N)","O(N^2)","O(log N)","O(1)","A","Khi mảng đã sắp xếp và có cờ kiểm tra hoán đổi swapped, Bubble Sort dừng sau 1 lượt duyệt O(N)."
"Thuật toán sắp xếp nào sau đây KHÔNG có tính ổn định (Not Stable)?","Selection Sort","Merge Sort","Bubble Sort","Insertion Sort","A","Selection Sort có thể hoán đổi các phần tử bằng nhau qua khoảng cách xa làm thay đổi thứ tự ban đầu."
"Ngăn xếp (Stack) hoạt động theo nguyên lý nào?","LIFO","FIFO","LILO","FILO","A","Stack hoạt động theo cơ chế Last-In-First-Out (Vào sau ra trước)."`;

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

const form = reactive({
  title: '',
  description: '',
  lessonId: 0,
  nodeId: null as number | null,
  stage: 1 as number,
  durationMinutes: 15,
  maxScore: 100,
  status: 'Active' as 'Active' | 'Draft',
  // Code config
  starterCode: 'function solve(input) {\n  // Viết giải thuật của bạn ở đây\n  return input;\n}',
  testCases: [
    { input: '[1, 2, 3]', expected: '[3, 2, 1]', isHidden: false },
    { input: '[4, 5, 6]', expected: '[6, 5, 4]', isHidden: true },
  ],
  // Quiz questions
  questions: [
    {
      content: 'Độ phức tạp thời gian trung bình của thuật toán QuickSort là bao nhiêu?',
      options: ['O(N log N)', 'O(N^2)', 'O(N)', 'O(1)'],
      correctIndex: 0,
      explanation: 'QuickSort có độ phức tạp trung bình là O(N log N) khi chọn pivot ngẫu nhiên tốt.',
      points: 1,
    },
  ],
});

watch(
  () => props.open,
  (isOpen, oldOpen) => {
    if (!isOpen || isOpen === oldOpen) return;
    if (props.defaultNodeId) form.nodeId = props.defaultNodeId;
    if (props.defaultTab) {
      activeTab.value = props.defaultTab;
    } else if (props.defaultStage) {
      form.stage = props.defaultStage;
      activeTab.value = props.defaultStage === 3 ? 'code' : 'quiz';
    } else {
      activeTab.value = 'quiz';
    }
    if (props.lessons.length > 0 && !form.lessonId) {
      form.lessonId = props.lessons[0].id;
    }
  },
  { immediate: true },
);

// ── Question Operations ──
function addQuestion(): void {
  form.questions.push({
    content: '',
    options: ['', '', '', ''],
    correctIndex: 0,
    explanation: '',
    points: 1,
  });
}

function removeQuestion(index: number): void {
  if (form.questions.length <= 1) {
    ui.showToast('Bài Quiz phải có ít nhất 1 câu hỏi.', 'warning');
    return;
  }
  form.questions.splice(index, 1);
}

// ── Testcase Operations ──
function addTestCase(): void {
  form.testCases.push({
    input: '',
    expected: '',
    isHidden: false,
  });
}

function removeTestCase(index: number): void {
  if (form.testCases.length <= 1) {
    ui.showToast('Bài Code phải có ít nhất 1 testcase.', 'warning');
    return;
  }
  form.testCases.splice(index, 1);
}

function onFileSelected(event: Event): void {
  const target = event.target as HTMLInputElement;
  if (target.files && target.files.length > 0) {
    csvFile.value = target.files[0];
  }
}

// ── Save Exercise ──
async function handleSave(): Promise<void> {
  if (form.title.trim().length < 3) {
    ui.showToast('Tiêu đề bài tập phải từ 3 ký tự trở lên.', 'warning');
    return;
  }

  if (!form.lessonId) {
    ui.showToast('Vui lòng chọn Bài học liên kết.', 'warning');
    return;
  }

  saving.value = true;
  try {
    const isCode = activeTab.value === 'code';
    const exerciseType = isCode ? 'Code' : 'Mcq';
    const stage = isCode ? 3 : 1;

    let payloadQuestions: QuestionUpsertDto[] | undefined;
    let configJson: string | undefined;

    if (!isCode) {
      // Validate questions
      for (let i = 0; i < form.questions.length; i++) {
        const q = form.questions[i];
        if (!q.content.trim()) {
          ui.showToast(`Câu hỏi số ${i + 1} chưa có nội dung.`, 'warning');
          saving.value = false;
          return;
        }
        if (q.options.some((opt) => !opt.trim())) {
          ui.showToast(`Câu hỏi số ${i + 1} có đáp án trống.`, 'warning');
          saving.value = false;
          return;
        }
      }

      payloadQuestions = form.questions.map((q, idx) => ({
        content: q.content.trim(),
        type: 'Single',
        options: q.options.map((opt) => opt.trim()),
        answerJson: JSON.stringify([q.correctIndex]),
        explanation: q.explanation.trim() || undefined,
        points: q.points || 1,
        sortOrder: idx + 1,
      }));
    } else {
      // Validate testcases
      for (let i = 0; i < form.testCases.length; i++) {
        const tc = form.testCases[i];
        if (!tc.input.trim() || !tc.expected.trim()) {
          ui.showToast(`Testcase số ${i + 1} chưa điền đủ Input/Expected.`, 'warning');
          saving.value = false;
          return;
        }
      }

      configJson = JSON.stringify({
        starterCode: form.starterCode,
        testCases: form.testCases,
      });
    }

    const payload: ExerciseUpsertPayload = {
      title: form.title.trim(),
      description: form.description.trim() || undefined,
      lessonId: form.lessonId,
      nodeId: form.nodeId,
      stage,
      type: exerciseType,
      durationMinutes: form.durationMinutes > 0 ? form.durationMinutes : null,
      maxScore: form.maxScore,
      status: form.status,
      configJson,
      questions: payloadQuestions,
    };

    if (props.exerciseId) {
      await updateExercise(props.exerciseId, payload);
      ui.showToast('Đã cập nhật bài tập thành công!', 'success');
    } else {
      await createExercise(payload);
      ui.showToast('Đã tạo bài tập mới thành công!', 'success');
    }

    emit('saved');
    emit('close');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể lưu bài tập.', 'error');
  } finally {
    saving.value = false;
  }
}

// ── Import CSV ──
async function handleImportCsv(): Promise<void> {
  if (!csvFile.value) {
    ui.showToast('Vui lòng chọn file CSV để tải lên.', 'warning');
    return;
  }
  if (!form.lessonId) {
    ui.showToast('Vui lòng chọn Bài học liên kết.', 'warning');
    return;
  }

  importing.value = true;
  try {
    const result = await importExerciseCsv(form.lessonId, csvFile.value);
    ui.showToast(result.message || `Đã nhập thành công ${result.createdCount} câu hỏi từ CSV!`, 'success');
    emit('saved');
    emit('close');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Nhập CSV thất bại.', 'error');
  } finally {
    importing.value = false;
  }
}
</script>

<template>
  <Modal
    :open="open"
    :title="exerciseId ? 'Chỉnh sửa Bài tập' : 'Tạo Bài tập & Quiz mới cho Node'"
    class="max-w-4xl"
    @close="emit('close')"
  >
    <div class="space-y-6">
      <!-- Exercise Type Navigation -->
      <div class="flex border-b border-vdsa-border gap-2 pb-2">
        <button
          type="button"
          class="px-4 py-2 text-sm font-bold rounded-lg transition-colors flex items-center gap-2"
          :class="activeTab === 'quiz' ? 'bg-vdsa-accent text-white shadow-md' : 'text-vdsa-muted hover:text-white hover:bg-vdsa-hover'"
          @click="activeTab = 'quiz'"
        >
          <HelpCircle :size="16" /> 1. Quiz Trắc Nghiệm (Stage 1)
        </button>
        <button
          type="button"
          class="px-4 py-2 text-sm font-bold rounded-lg transition-colors flex items-center gap-2"
          :class="activeTab === 'code' ? 'bg-vdsa-accent text-white shadow-md' : 'text-vdsa-muted hover:text-white hover:bg-vdsa-hover'"
          @click="activeTab = 'code'"
        >
          <Code :size="16" /> 2. Bài tập Code / Testcase (Stage 3)
        </button>
        <button
          type="button"
          class="px-4 py-2 text-sm font-bold rounded-lg transition-colors flex items-center gap-2"
          :class="activeTab === 'import-csv' ? 'bg-vdsa-accent text-white shadow-md' : 'text-vdsa-muted hover:text-white hover:bg-vdsa-hover'"
          @click="activeTab = 'import-csv'"
        >
          <Upload :size="16" /> 3. Nhập từ file CSV
        </button>
      </div>

      <!-- FORM FIELDS FOR QUIZ & CODE -->
      <template v-if="activeTab !== 'import-csv'">
        <!-- General Info -->
        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
          <Input v-model="form.title" label="Tiêu đề bài tập" placeholder="Ví dụ: Kiểm tra trắc nghiệm Mảng & Danh sách liên kết" required />

          <div>
            <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Gắn vào Node Ladder</label>
            <select
              v-model="form.nodeId"
              class="w-full bg-vdsa-surface border border-vdsa-border rounded-xl px-3 py-2.5 text-sm text-white focus:outline-none focus:border-accent"
            >
              <option :value="null">-- Không gán vào Node (Bài tập độc lập) --</option>
              <option v-for="i in 8" :key="i" :value="i">
                Node {{ i }} (Bậc {{ ((i % 3) + 1) }})
              </option>
            </select>
          </div>
        </div>

        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Bài học liên kết</label>
            <select
              v-model="form.lessonId"
              class="w-full bg-vdsa-surface border border-vdsa-border rounded-xl px-3 py-2.5 text-sm text-white focus:outline-none focus:border-accent"
            >
              <option v-for="l in lessons" :key="l.id" :value="l.id">
                #{{ l.id }} - {{ l.title }}
              </option>
            </select>
          </div>

          <div>
            <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Thời gian làm bài (Phút)</label>
            <input
              v-model.number="form.durationMinutes"
              type="number"
              min="0"
              placeholder="0 = Không giới hạn"
              class="w-full bg-vdsa-surface border border-vdsa-border rounded-xl px-3 py-2.5 text-sm text-white focus:outline-none focus:border-accent"
            />
          </div>

          <div>
            <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Điểm tối đa</label>
            <input
              v-model.number="form.maxScore"
              type="number"
              min="10"
              class="w-full bg-vdsa-surface border border-vdsa-border rounded-xl px-3 py-2.5 text-sm text-white focus:outline-none focus:border-accent"
            />
          </div>
        </div>

        <div>
          <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Mô tả / Hướng dẫn làm bài</label>
          <textarea
            v-model="form.description"
            rows="2"
            class="w-full bg-vdsa-surface border border-vdsa-border rounded-xl p-3 text-sm text-white focus:outline-none focus:border-accent resize-y placeholder:text-vdsa-disabled"
            placeholder="Hướng dẫn sinh viên các yêu cầu cần hoàn thành..."
          ></textarea>
        </div>

        <!-- 1. QUIZ BUILDER (QUESTIONS LIST) -->
        <div v-if="activeTab === 'quiz'" class="space-y-4 pt-2">
          <div class="flex items-center justify-between">
            <h4 class="text-sm font-bold text-white uppercase tracking-wider flex items-center gap-2">
              <HelpCircle :size="16" class="text-vdsa-yellow" /> Danh sách câu hỏi trắc nghiệm ({{ form.questions.length }} câu)
            </h4>
            <Button size="sm" type="button" @click="addQuestion"><Plus :size="14" /> Thêm câu hỏi</Button>
          </div>

          <div class="space-y-4 max-h-[380px] overflow-y-auto pr-1 custom-scrollbar">
            <div
              v-for="(q, qIdx) in form.questions"
              :key="qIdx"
              class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border space-y-3 relative group"
            >
              <div class="flex items-center justify-between">
                <span class="text-xs font-extrabold text-vdsa-accent flex items-center gap-1.5 uppercase">
                  Câu hỏi {{ qIdx + 1 }}
                </span>
                <button
                  type="button"
                  class="text-vdsa-muted hover:text-vdsa-red transition-colors p-1"
                  title="Xóa câu hỏi này"
                  @click="removeQuestion(qIdx)"
                >
                  <Trash2 :size="15" />
                </button>
              </div>

              <div>
                <input
                  v-model="q.content"
                  type="text"
                  placeholder="Nhập nội dung câu hỏi..."
                  class="w-full bg-vdsa-bg border border-vdsa-border rounded-lg p-2.5 text-sm text-white focus:outline-none focus:border-accent"
                />
              </div>

              <!-- Options A, B, C, D -->
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-2.5 pt-1">
                <div
                  v-for="(opt, optIdx) in q.options"
                  :key="optIdx"
                  class="flex items-center gap-2 p-2 rounded-lg border transition-colors"
                  :class="q.correctIndex === optIdx ? 'bg-vdsa-green/10 border-vdsa-green/60' : 'bg-vdsa-bg border-vdsa-border'"
                >
                  <input
                    :id="`q-${qIdx}-opt-${optIdx}`"
                    type="radio"
                    :name="`question-${qIdx}`"
                    :checked="q.correctIndex === optIdx"
                    class="accent-green-500 cursor-pointer"
                    @change="q.correctIndex = optIdx"
                  />
                  <label :for="`q-${qIdx}-opt-${optIdx}`" class="text-xs font-bold text-vdsa-secondary w-5">
                    {{ String.fromCharCode(65 + optIdx) }}.
                  </label>
                  <input
                    v-model="q.options[optIdx]"
                    type="text"
                    :placeholder="`Đáp án ${String.fromCharCode(65 + optIdx)}`"
                    class="flex-1 bg-transparent border-none text-xs text-white focus:outline-none"
                  />
                </div>
              </div>

              <!-- Explanation -->
              <div>
                <input
                  v-model="q.explanation"
                  type="text"
                  placeholder="Giải thích tại sao đáp án này đúng (tùy chọn)..."
                  class="w-full bg-vdsa-bg/60 border border-vdsa-border rounded-lg px-3 py-1.5 text-xs text-vdsa-secondary focus:outline-none focus:border-accent"
                />
              </div>
            </div>
          </div>
        </div>

        <!-- 2. CODE & TESTCASES BUILDER -->
        <div v-if="activeTab === 'code'" class="space-y-4 pt-2">
          <!-- Starter Code -->
          <div>
            <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Mã nguồn khởi tạo (Starter Code IDE)</label>
            <textarea
              v-model="form.starterCode"
              rows="5"
              class="w-full bg-[#0d1117] font-mono border border-vdsa-border rounded-xl p-3 text-xs text-green-400 focus:outline-none focus:border-accent resize-y"
            ></textarea>
          </div>

          <!-- Test Cases -->
          <div class="space-y-3">
            <div class="flex items-center justify-between">
              <h4 class="text-sm font-bold text-white uppercase tracking-wider flex items-center gap-2">
                <Code :size="16" class="text-vdsa-purple-light" /> Bộ Test Cases chấm điểm ({{ form.testCases.length }} testcases)
              </h4>
              <Button size="sm" type="button" @click="addTestCase"><Plus :size="14" /> Thêm Test Case</Button>
            </div>

            <div class="space-y-2.5 max-h-[280px] overflow-y-auto pr-1 custom-scrollbar">
              <div
                v-for="(tc, tcIdx) in form.testCases"
                :key="tcIdx"
                class="p-3 rounded-xl bg-vdsa-surface border border-vdsa-border flex flex-col sm:flex-row items-center gap-3 relative"
              >
                <span class="text-xs font-bold text-vdsa-muted shrink-0">#{{ tcIdx + 1 }}</span>
                <div class="flex-1 w-full">
                  <label class="block text-[10px] font-bold text-vdsa-muted uppercase">Input</label>
                  <input
                    v-model="tc.input"
                    type="text"
                    placeholder="Ví dụ: [1, 2, 3]"
                    class="w-full bg-vdsa-bg border border-vdsa-border rounded-lg px-2.5 py-1.5 text-xs font-mono text-white focus:outline-none focus:border-accent"
                  />
                </div>
                <div class="flex-1 w-full">
                  <label class="block text-[10px] font-bold text-vdsa-muted uppercase">Expected Output</label>
                  <input
                    v-model="tc.expected"
                    type="text"
                    placeholder="Ví dụ: [3, 2, 1]"
                    class="w-full bg-vdsa-bg border border-vdsa-border rounded-lg px-2.5 py-1.5 text-xs font-mono text-white focus:outline-none focus:border-accent"
                  />
                </div>
                <div class="flex items-center gap-2 pt-3 sm:pt-0 shrink-0">
                  <label class="flex items-center gap-1.5 text-xs text-vdsa-secondary cursor-pointer select-none">
                    <input v-model="tc.isHidden" type="checkbox" class="accent-purple-600 rounded cursor-pointer" />
                    Ẩn testcase
                  </label>
                  <button
                    type="button"
                    class="text-vdsa-muted hover:text-vdsa-red transition-colors p-1"
                    title="Xóa testcase này"
                    @click="removeTestCase(tcIdx)"
                  >
                    <Trash2 :size="14" />
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </template>

      <!-- 3. IMPORT CSV TAB -->
      <div v-else class="space-y-4 py-4">
        <div class="p-6 rounded-2xl bg-vdsa-surface border border-vdsa-border text-center space-y-4">
          <div class="w-12 h-12 rounded-full bg-vdsa-accent/20 text-vdsa-accent flex items-center justify-center mx-auto">
            <FileText :size="24" />
          </div>
          <div>
            <h4 class="text-base font-bold text-white">Tải lên file CSV câu hỏi trắc nghiệm</h4>
            <p class="text-xs text-vdsa-muted max-w-md mx-auto mt-1">
              Định dạng CSV chuẩn: <code class="text-vdsa-yellow">Question,OptionA,OptionB,OptionC,OptionD,CorrectAnswer,Explanation</code>
            </p>
          </div>

          <div class="max-w-md mx-auto">
            <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5 text-left">Chọn Bài học liên kết</label>
            <select
              v-model="form.lessonId"
              class="w-full bg-vdsa-bg border border-vdsa-border rounded-xl px-3 py-2.5 text-sm text-white focus:outline-none focus:border-accent"
            >
              <option v-for="l in lessons" :key="l.id" :value="l.id">
                #{{ l.id }} - {{ l.title }}
              </option>
            </select>
          </div>

          <div class="max-w-md mx-auto border-2 border-dashed border-vdsa-border hover:border-vdsa-accent rounded-xl p-6 transition-colors">
            <input
              type="file"
              accept=".csv"
              class="block w-full text-xs text-vdsa-muted file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-xs file:font-semibold file:bg-vdsa-accent file:text-white hover:file:bg-vdsa-accent-dark cursor-pointer"
              @change="onFileSelected"
            />
            <p v-if="csvFile" class="text-xs text-vdsa-green font-bold mt-2">
              Đã chọn: {{ csvFile.name }} ({{ Math.round(csvFile.size / 1024) }} KB)
            </p>
          </div>

          <div class="flex items-center justify-center gap-3">
            <Button
              variant="secondary"
              size="md"
              type="button"
              class="gap-1.5 border-vdsa-border hover:bg-vdsa-hover"
              @click="downloadSampleCsv"
            >
              <Download :size="16" /> 📥 Tải file mẫu CSV
            </Button>
            <Button
              variant="primary"
              size="md"
              :disabled="!csvFile || importing"
              @click="handleImportCsv"
            >
              <span v-if="importing" class="inline-block w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin mr-2"></span>
              <Upload :size="16" /> Bắt đầu Nhập từ CSV
            </Button>
          </div>
        </div>
      </div>

      <!-- Action Buttons -->
      <div v-if="activeTab !== 'import-csv'" class="flex items-center justify-end gap-3 pt-4 border-t border-vdsa-border">
        <Button variant="ghost" size="md" type="button" @click="emit('close')">Hủy</Button>
        <Button variant="primary" size="md" type="button" :disabled="saving" @click="handleSave">
          <span v-if="saving" class="inline-block w-4 h-4 border-2 border-white/30 border-t-white rounded-full animate-spin mr-2"></span>
          {{ exerciseId ? 'Lưu thay đổi' : 'Tạo Bài tập' }}
        </Button>
      </div>
    </div>
  </Modal>
</template>
