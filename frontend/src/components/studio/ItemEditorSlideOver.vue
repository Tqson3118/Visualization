<script setup lang="ts">
import { ref, reactive, watch, computed } from 'vue';
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
  ArrowUp,
  ArrowDown,
} from 'lucide-vue-next';
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

const props = defineProps<{
  open: boolean;
  item: PathItemDto | null;
  pathId: number;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'saved', item: PathItemDto): void;
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

// Quiz Specific
interface InlineQuizQuestion {
  id?: number;
  content: string;
  options: string[];
  correctIndex: number;
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
  starterCode: `/**\n * @param {any} input\n * @return {any}\n */\nfunction solve(input) {\n  // Viết mã nguồn giải thuật tại đây\n  return input;\n}`,
  testCases: [
    { input: '[1, 2, 3]', expected: '[3, 2, 1]', isHidden: false },
    { input: '[5, 4, 3, 2, 1]', expected: '[1, 2, 3, 4, 5]', isHidden: true },
  ] as LabTestCase[],
});

const currentItemType = computed<PathItemType>(() => {
  return props.item ? normalizeItemType(props.item.itemType) : 'theory';
});

// ── Dirty state guard: cảnh báo thoát khi còn thay đổi chưa lưu (plan §5.1) ──
const dirtyBaseline = ref('{}');
const showDirtyConfirm = ref(false);

function snapshot(): string {
  return JSON.stringify({
    title: form.title,
    description: form.description,
    theory: theoryContentHtml.value,
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
    quizQuestions.value = [];

    // Fetch full detail if available
    loadingDetail.value = true;
    try {
      const detail = await fetchItemDetail(newItem.id);
      form.title = detail.title || '';
      form.description = detail.description || '';

      if (normalizeItemType(detail.itemType) === 'theory' && detail.lesson) {
        theoryContentHtml.value = detail.lesson.contentHtml || '';
      } else if (normalizeItemType(detail.itemType) === 'quiz' && detail.exercise) {
        if (detail.exercise.questions && detail.exercise.questions.length > 0) {
          quizQuestions.value = detail.exercise.questions.map((q: any) => ({
            id: q.id,
            content: q.content,
            options: q.options || ['A', 'B', 'C', 'D'],
            correctIndex: q.answer?.[0] ?? 0,
            explanation: q.explanation || '',
            points: q.points || 1,
          }));
        } else {
          quizQuestions.value = [
            {
              content: 'Câu hỏi trắc nghiệm số 1?',
              options: ['Đáp án A', 'Đáp án B', 'Đáp án C', 'Đáp án D'],
              correctIndex: 0,
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

function addQuestion() {
  quizQuestions.value.push({
    content: `Câu hỏi ${quizQuestions.value.length + 1}?`,
    options: ['Lựa chọn A', 'Lựa chọn B', 'Lựa chọn C', 'Lựa chọn D'],
    correctIndex: 0,
    explanation: '',
    points: 1,
  });
}

function removeQuestion(index: number) {
  quizQuestions.value.splice(index, 1);
}

function addOption(qIdx: number) {
  if (quizQuestions.value[qIdx].options.length < 6) {
    quizQuestions.value[qIdx].options.push(`Lựa chọn ${quizQuestions.value[qIdx].options.length + 1}`);
  }
}

function removeOption(qIdx: number, optIdx: number) {
  if (quizQuestions.value[qIdx].options.length > 2) {
    quizQuestions.value[qIdx].options.splice(optIdx, 1);
    if (quizQuestions.value[qIdx].correctIndex >= quizQuestions.value[qIdx].options.length) {
      quizQuestions.value[qIdx].correctIndex = 0;
    }
  }
}

function addTestCase() {
  labForm.testCases.push({ input: '', expected: '', isHidden: false });
}

function removeTestCase(idx: number) {
  labForm.testCases.splice(idx, 1);
}

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

    if (itemType === 'theory' && props.item.lessonId) {
      // PUT lesson = thay thế toàn bộ resource: fetch hiện trạng để giữ topicId/status
      const currentLesson = await lessonsApi.fetchLesson(props.item.lessonId);
      await lessonsApi.updateLesson(props.item.lessonId, {
        topicId: currentLesson.topicId,
        title: form.title.trim(),
        description: form.description.trim(),
        contentHtml: theoryContentHtml.value,
        status: currentLesson.status,
        sortOrder: currentLesson.sortOrder,
        isClassOnly: currentLesson.isClassOnly,
      });
    } else if (itemType === 'quiz') {
      const exerciseId = props.item.finalTestId || props.item.exerciseId;
      if (exerciseId) {
        await exercisesApi.updateExercise(exerciseId, {
          title: form.title.trim(),
          description: form.description.trim(),
          maxScore: quizQuestions.value.reduce((sum, q) => sum + q.points, 0),
          questions: quizQuestions.value.map((q, idx) => ({
            content: q.content,
            type: 'Single' as const,
            options: q.options,
            answerJson: JSON.stringify([q.correctIndex]),
            explanation: q.explanation,
            points: q.points,
            sortOrder: idx + 1,
          })),
        });
      }
    } else if (itemType === 'lab') {
      const exerciseId = props.item.labExerciseId || props.item.exerciseId;
      if (exerciseId) {
        const configJson = JSON.stringify({
          entryFunction: labForm.entryFunction,
          starterCode: labForm.starterCode,
          testCases: labForm.testCases,
        });
        await exercisesApi.updateExercise(exerciseId, {
          title: form.title.trim(),
          description: form.description.trim(),
          maxScore: labForm.maxScore,
          configJson,
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
    <div class="p-3.5 bg-[#171622] border-b border-[#262438] flex items-center justify-between gap-3">
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

    <!-- Body -->
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
            placeholder="Mô tả mục tiêu hoặc nội dung cốt lõi..."
            class="w-full px-3 py-2 text-xs font-medium bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
          />
        </div>
      </div>

      <!-- Theory Editor -->
      <div v-if="currentItemType === 'theory'" class="space-y-3">
        <div class="flex items-center justify-between">
          <label class="text-[11px] font-bold text-slate-300 uppercase tracking-wider flex items-center gap-1.5">
            <BookOpen class="w-3.5 h-3.5 text-sky-400" />
            <span>Nội dung Lý thuyết (Markdown / HTML)</span>
          </label>
          <div class="flex items-center gap-1 bg-[#171624] p-1 rounded-lg border border-[#27253b]">
            <button
              type="button"
              class="px-2 py-0.5 text-[10px] font-bold rounded transition-colors"
              :class="activeViewMode === 'edit' ? 'bg-purple-600 text-white' : 'text-slate-400 hover:text-white'"
              @click="activeViewMode = 'edit'"
            >
              Soạn thảo
            </button>
            <button
              type="button"
              class="px-2 py-0.5 text-[10px] font-bold rounded transition-colors"
              :class="activeViewMode === 'preview' ? 'bg-purple-600 text-white' : 'text-slate-400 hover:text-white'"
              @click="activeViewMode = 'preview'"
            >
              Xem trước
            </button>
          </div>
        </div>

        <textarea
          v-if="activeViewMode === 'edit'"
          v-model="theoryContentHtml"
          rows="14"
          placeholder="Viết nội dung bài giảng tại đây (hỗ trợ HTML và Markdown)..."
          class="w-full px-3.5 py-2.5 font-mono text-xs leading-relaxed bg-[#0e0d16] border border-[#2e2c44] rounded-xl text-slate-200 placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
        />
        <div
          v-else
          class="p-4 bg-[#0e0d16] border border-[#2e2c44] rounded-xl min-h-[300px] text-slate-200 text-xs leading-relaxed prose prose-invert max-w-none"
        >
          <ProseContent :html="theoryContentHtml" />
        </div>
      </div>

      <!-- Quiz Editor -->
      <div v-else-if="currentItemType === 'quiz'" class="space-y-4">
        <div class="flex items-center justify-between">
          <label class="text-[11px] font-bold text-slate-300 uppercase tracking-wider flex items-center gap-1.5">
            <HelpCircle class="w-3.5 h-3.5 text-orange-400" />
            <span>Danh sách Câu hỏi Trắc nghiệm ({{ quizQuestions.length }})</span>
          </label>
          <button
            type="button"
            class="flex items-center gap-1 px-2.5 py-1 text-xs font-bold text-orange-300 bg-orange-500/10 hover:bg-orange-500/20 border border-orange-500/30 rounded-lg transition-colors cursor-pointer"
            @click="addQuestion"
          >
            <Plus class="w-3.5 h-3.5" />
            <span>+ Câu hỏi</span>
          </button>
        </div>

        <div v-for="(q, qIdx) in quizQuestions" :key="qIdx" class="p-3.5 bg-[#171624] border border-[#27253b] rounded-xl space-y-3">
          <div class="flex items-center justify-between gap-2">
            <span class="text-xs font-black text-orange-400">Câu {{ qIdx + 1 }}</span>
            <div class="flex items-center gap-2">
              <span class="text-[10px] text-slate-400">Điểm:</span>
              <input
                v-model.number="q.points"
                type="number"
                min="1"
                max="10"
                class="w-12 px-1.5 py-0.5 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded text-white text-center"
              />
              <button
                type="button"
                title="Xóa câu hỏi này"
                class="p-1 text-rose-400 hover:bg-rose-500/20 rounded cursor-pointer"
                @click="removeQuestion(qIdx)"
              >
                <Trash2 class="w-3.5 h-3.5" />
              </button>
            </div>
          </div>

          <textarea
            v-model="q.content"
            rows="2"
            placeholder="Nội dung câu hỏi..."
            class="w-full px-3 py-1.5 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-lg text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
          />

          <!-- Options -->
          <div class="space-y-1.5">
            <span class="text-[10px] uppercase font-bold text-slate-400">Các lựa chọn (Tích chọn đáp án đúng):</span>
            <div
              v-for="(opt, optIdx) in q.options"
              :key="optIdx"
              class="flex items-center gap-2"
            >
              <input
                type="radio"
                :name="`quiz-correct-${qIdx}`"
                :checked="q.correctIndex === optIdx"
                class="accent-orange-500 w-3.5 h-3.5 cursor-pointer"
                @change="q.correctIndex = optIdx"
              />
              <input
                v-model="q.options[optIdx]"
                type="text"
                class="flex-1 px-2.5 py-1 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-md text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
              />
              <button
                v-if="q.options.length > 2"
                type="button"
                class="p-1 text-slate-500 hover:text-rose-400 cursor-pointer"
                @click="removeOption(qIdx, optIdx)"
              >
                <X class="w-3 h-3" />
              </button>
            </div>
            <button
              v-if="q.options.length < 6"
              type="button"
              class="text-[11px] font-bold text-purple-400 hover:text-purple-300 mt-1 cursor-pointer"
              @click="addOption(qIdx)"
            >
              + Thêm lựa chọn
            </button>
          </div>

          <!-- Explanation -->
          <div>
            <span class="text-[10px] uppercase font-bold text-slate-400">Giải thích chi tiết sau khi nộp:</span>
            <input
              v-model="q.explanation"
              type="text"
              placeholder="Giải thích tại sao đáp án này đúng..."
              class="w-full mt-1 px-2.5 py-1 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-md text-white placeholder:text-slate-600 focus:outline-none focus:border-purple-500"
            />
          </div>
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

        <div class="grid grid-cols-2 gap-3 p-3.5 bg-[#171624] border border-[#27253b] rounded-xl">
          <div>
            <label class="block text-[10px] font-bold text-slate-400 uppercase mb-1">Tên hàm thực thi (Entry Function)</label>
            <input
              v-model="labForm.entryFunction"
              type="text"
              class="w-full px-2.5 py-1.5 text-xs font-mono bg-[#0e0d16] border border-[#2e2c44] rounded-md text-emerald-400"
            />
          </div>
          <div>
            <label class="block text-[10px] font-bold text-slate-400 uppercase mb-1">Điểm tối đa</label>
            <input
              v-model.number="labForm.maxScore"
              type="number"
              class="w-full px-2.5 py-1.5 text-xs bg-[#0e0d16] border border-[#2e2c44] rounded-md text-white"
            />
          </div>
        </div>

        <div>
          <label class="block text-[11px] font-bold text-slate-300 uppercase tracking-wider mb-1.5">
            Mã nguồn khởi đầu (Starter Code)
          </label>
          <textarea
            v-model="labForm.starterCode"
            rows="6"
            class="w-full px-3 py-2 font-mono text-xs leading-relaxed bg-[#0e0d16] border border-[#2e2c44] rounded-xl text-emerald-300 focus:outline-none focus:border-purple-500"
          />
        </div>

        <!-- Test Cases -->
        <div class="space-y-2">
          <div class="flex items-center justify-between">
            <span class="text-[11px] font-bold text-slate-300 uppercase tracking-wider">Bộ Test Cases ({{ labForm.testCases.length }})</span>
            <button
              type="button"
              class="flex items-center gap-1 px-2 py-0.5 text-xs font-bold text-emerald-300 bg-emerald-500/10 hover:bg-emerald-500/20 border border-emerald-500/30 rounded cursor-pointer"
              @click="addTestCase"
            >
              <Plus class="w-3 h-3" />
              <span>+ Test Case</span>
            </button>
          </div>

          <div
            v-for="(tc, tcIdx) in labForm.testCases"
            :key="tcIdx"
            class="p-2.5 bg-[#171624] border border-[#27253b] rounded-xl space-y-2"
          >
            <div class="flex items-center justify-between">
              <span class="text-[11px] font-bold text-emerald-400">Test Case {{ tcIdx + 1 }}</span>
              <div class="flex items-center gap-2">
                <label class="flex items-center gap-1 text-[10px] text-slate-400 cursor-pointer">
                  <input v-model="tc.isHidden" type="checkbox" class="accent-purple-500" />
                  <span>Ẩn (Chấm điểm bí mật)</span>
                </label>
                <button
                  type="button"
                  class="text-rose-400 hover:text-rose-300 p-0.5 cursor-pointer"
                  @click="removeTestCase(tcIdx)"
                >
                  <Trash2 class="w-3 h-3" />
                </button>
              </div>
            </div>
            <div class="grid grid-cols-2 gap-2">
              <div>
                <span class="text-[9px] uppercase font-bold text-slate-500">Đầu vào (Input JSON)</span>
                <input
                  v-model="tc.input"
                  type="text"
                  placeholder="ví dụ: [1, 2, 3]"
                  class="w-full px-2 py-1 text-xs font-mono bg-[#0e0d16] border border-[#2e2c44] rounded text-white"
                />
              </div>
              <div>
                <span class="text-[9px] uppercase font-bold text-slate-500">Đầu ra kỳ vọng (Expected Output)</span>
                <input
                  v-model="tc.expected"
                  type="text"
                  placeholder="ví dụ: [3, 2, 1]"
                  class="w-full px-2 py-1 text-xs font-mono bg-[#0e0d16] border border-[#2e2c44] rounded text-white"
                />
              </div>
            </div>
          </div>
        </div>
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
  </div>
</template>

<style scoped>
.item-editor-slideover {
  min-height: 500px;
}
</style>
