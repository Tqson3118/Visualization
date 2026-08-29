<script setup lang="ts">
import { onMounted, ref, watch } from 'vue';
import {
  Check,
  FileSpreadsheet,
  HelpCircle,
  Plus,
  Puzzle,
  Sparkles,
  Trash2,
  Upload,
  X,
} from 'lucide-vue-next';

import * as exercisesApi from '@/api/exercises';
import type { ExerciseDto, ExerciseSummaryDto } from '@/api/exercises';
import { useUiStore } from '@/stores/ui';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/Button.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';

const props = defineProps<{
  lessonId: number | null;
  lessonTitle: string;
}>();

const ui = useUiStore();

const loading = ref(false);
const exercises = ref<ExerciseSummaryDto[]>([]);
const createModalOpen = ref(false);
const creating = ref(false);

const csvInputRef = ref<HTMLInputElement | null>(null);
const importingCsv = ref(false);

// Form tạo quiz mới
const quizForm = ref({
  title: '',
  description: '',
  durationMinutes: 10,
  maxScore: 10,
  questions: [
    {
      content: '',
      options: ['', '', '', ''],
      correctIndex: 0,
      explanation: '',
      points: 2,
    },
  ],
});

async function loadExercises(): Promise<void> {
  if (!props.lessonId) {
    exercises.value = [];
    return;
  }
  loading.value = true;
  try {
    exercises.value = await exercisesApi.fetchExercises({ lessonId: props.lessonId });
  } catch {
    exercises.value = [];
  } finally {
    loading.value = false;
  }
}

watch(() => props.lessonId, loadExercises, { immediate: true });

function openCreateModal(): void {
  quizForm.value = {
    title: `Quiz: ${props.lessonTitle || 'Kiểm tra kiến thức'}`,
    description: 'Trắc nghiệm củng cố lý thuyết và phân tích thuật toán.',
    durationMinutes: 10,
    maxScore: 10,
    questions: [
      {
        content: '',
        options: ['', '', '', ''],
        correctIndex: 0,
        explanation: '',
        points: 2,
      },
    ],
  };
  createModalOpen.value = true;
}

function addQuestion(): void {
  quizForm.value.questions.push({
    content: '',
    options: ['', '', '', ''],
    correctIndex: 0,
    explanation: '',
    points: 2,
  });
}

function removeQuestion(idx: number): void {
  if (quizForm.value.questions.length > 1) {
    quizForm.value.questions.splice(idx, 1);
  }
}

async function handleCreateQuiz(): Promise<void> {
  if (!props.lessonId) {
    ui.showToast('Vui lòng lưu bài học trước khi tạo Quiz.', 'warning');
    return;
  }
  if (!quizForm.value.title.trim()) {
    ui.showToast('Vui lòng nhập tiêu đề Quiz.', 'warning');
    return;
  }

  // Validate questions
  for (let i = 0; i < quizForm.value.questions.length; i++) {
    const q = quizForm.value.questions[i];
    if (!q.content.trim()) {
      ui.showToast(`Vui lòng nhập nội dung cho câu hỏi #${i + 1}.`, 'warning');
      return;
    }
    const filledOptions = q.options.filter((o) => o.trim().length > 0);
    if (filledOptions.length < 2) {
      ui.showToast(`Câu hỏi #${i + 1} phải có ít nhất 2 đáp án lựa chọn.`, 'warning');
      return;
    }
  }

  creating.value = true;
  try {
    const formattedQuestions = quizForm.value.questions.map((q, idx) => ({
      content: q.content.trim(),
      type: 'Single' as const,
      options: q.options.map((o) => o.trim()).filter(Boolean),
      answerJson: JSON.stringify([q.correctIndex]),
      explanation: q.explanation.trim() || undefined,
      points: q.points || 2,
      sortOrder: idx + 1,
    }));

    await exercisesApi.createExercise({
      lessonId: props.lessonId,
      title: quizForm.value.title.trim(),
      description: quizForm.value.description.trim() || undefined,
      type: 'Mcq',
      durationMinutes: quizForm.value.durationMinutes,
      maxScore: quizForm.value.maxScore,
      status: 'Active',
      questions: formattedQuestions,
    });

    ui.showToast('Đã tạo Quiz trắc nghiệm gắn vào bài học thành công!', 'success');
    createModalOpen.value = false;
    await loadExercises();
  } catch (err: any) {
    ui.showToast(err.message || 'Tạo Quiz thất bại.', 'error');
  } finally {
    creating.value = false;
  }
}

async function handleDeleteExercise(id: number): Promise<void> {
  if (!confirm('Bạn có chắc chắn muốn xóa Quiz này không?')) return;
  try {
    await exercisesApi.deleteExercise(id);
    ui.showToast('Đã xóa Quiz thành công.', 'success');
    await loadExercises();
  } catch (err: any) {
    ui.showToast(err.message || 'Xóa Quiz thất bại.', 'error');
  }
}

function triggerCsvUpload(): void {
  csvInputRef.value?.click();
}

async function onCsvFileSelected(event: Event): Promise<void> {
  const file = (event.target as HTMLInputElement).files?.[0];
  if (!file || !props.lessonId) return;

  importingCsv.value = true;
  try {
    const res = await exercisesApi.importExerciseCsv(props.lessonId, file);
    ui.showToast(`Đã nhập thành công ${res.createdCount || 'các'} câu hỏi từ CSV!`, 'success');
    await loadExercises();
  } catch (err: any) {
    ui.showToast(err.message || 'Không thể nhập file CSV.', 'error');
  } finally {
    importingCsv.value = false;
    if (csvInputRef.value) csvInputRef.value.value = '';
  }
}
</script>

<template>
  <div class="quiz-tab flex flex-col h-full p-6 overflow-y-auto max-w-5xl mx-auto space-y-6">
    <!-- Header -->
    <div class="flex items-center justify-between border-b border-vdsa-border pb-4">
      <div>
        <h2 class="text-lg font-black text-white flex items-center gap-2">
          <Puzzle class="text-vdsa-purple" :size="20" />
          Bài tập & Câu hỏi trắc nghiệm (Quiz)
        </h2>
        <p class="text-xs text-vdsa-muted mt-1">
          Gắn các bộ câu hỏi trắc nghiệm giúp học viên kiểm tra kiến thức ngay sau khi đọc lý thuyết bài học.
        </p>
      </div>

      <div class="flex items-center gap-2" v-if="lessonId">
        <input ref="csvInputRef" type="file" accept=".csv" class="hidden" @change="onCsvFileSelected" />
        <Button variant="secondary" size="sm" class="gap-1.5 text-xs" :loading="importingCsv" @click="triggerCsvUpload">
          <Upload :size="13" /> Nhập từ CSV
        </Button>
        <Button variant="primary" size="sm" class="gap-1.5 text-xs" @click="openCreateModal">
          <Plus :size="13" /> Tạo bộ Quiz mới
        </Button>
      </div>
    </div>

    <!-- Alert if lesson not saved yet -->
    <div v-if="!lessonId" class="p-6 rounded-2xl bg-purple-950/20 border border-purple-500/30 text-center space-y-3">
      <div class="w-12 h-12 rounded-full bg-purple-500/20 text-purple-300 flex items-center justify-center mx-auto text-lg font-bold">
        ❓
      </div>
      <h3 class="text-sm font-bold text-white">Bài học chưa được lưu</h3>
      <p class="text-xs text-slate-400 max-w-md mx-auto">
        Vui lòng lưu hoặc xuất bản bài học trước để hệ thống cấp mã định danh ID, sau đó bạn có thể gắn câu hỏi trắc nghiệm và bài tập trực tiếp cho bài học này.
      </p>
    </div>

    <!-- Exercises list -->
    <div v-else class="space-y-4">
      <div v-if="loading" class="text-center py-8 text-xs text-slate-400">
        Đang tải danh sách bài tập...
      </div>

      <div v-else-if="exercises.length === 0" class="p-8 rounded-2xl bg-vdsa-surface border border-vdsa-border text-center space-y-3">
        <HelpCircle class="w-10 h-10 text-slate-500 mx-auto" />
        <h3 class="text-sm font-bold text-white">Chưa có bài tập hay câu hỏi Quiz nào</h3>
        <p class="text-xs text-slate-400">
          Bài học này hiện chưa gắn bài kiểm tra. Bạn có thể bấm nút bên dưới để tạo bài trắc nghiệm mới.
        </p>
        <Button variant="primary" size="sm" class="mt-2" @click="openCreateModal">
          <Plus :size="13" /> Tạo câu hỏi Quiz đầu tiên
        </Button>
      </div>

      <div v-else class="grid grid-cols-1 gap-3">
        <div
          v-for="ex in exercises"
          :key="ex.id"
          class="p-4 rounded-xl bg-vdsa-surface border border-vdsa-border flex items-center justify-between gap-4 hover:border-slate-600 transition-colors"
        >
          <div class="flex items-center gap-3">
            <span class="w-9 h-9 rounded-lg bg-emerald-500/20 text-emerald-300 flex items-center justify-center font-bold text-sm">
              <Check :size="16" />
            </span>
            <div>
              <h4 class="text-sm font-bold text-white">{{ ex.title }}</h4>
              <p class="text-xs text-slate-400 mt-0.5">{{ ex.description || 'Bài tập trắc nghiệm' }}</p>
              <div class="flex items-center gap-3 mt-1.5 text-[11px] text-slate-400">
                <span>⏱ {{ ex.durationMinutes }} phút</span>
                <span>⭐ Tối đa: {{ ex.maxScore }} điểm</span>
                <span class="font-mono text-purple-300">#{{ ex.id }}</span>
              </div>
            </div>
          </div>

          <div class="flex items-center gap-2">
            <a
              :href="`/exercise/${ex.id}`"
              target="_blank"
              class="px-3 py-1.5 rounded-lg bg-vdsa-bg-secondary hover:bg-slate-700 text-white text-xs font-medium transition-colors"
            >
              Làm thử ↗
            </a>
            <Button size="icon" variant="danger" class="w-8 h-8" @click="handleDeleteExercise(ex.id)">
              <Trash2 :size="14" />
            </Button>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Tạo Quiz mới -->
    <Modal :open="createModalOpen" title="Tạo bộ câu hỏi Quiz cho bài học" @close="createModalOpen = false">
      <form class="space-y-4 max-h-[70vh] overflow-y-auto pr-1" @submit.prevent="handleCreateQuiz">
        <Input v-model="quizForm.title" label="Tiêu đề Quiz" placeholder="VD: Trắc nghiệm Quick Sort..." required />
        <Input v-model="quizForm.description" label="Mô tả tóm tắt" placeholder="Mô tả yêu cầu hoặc kiến thức..." />

        <div class="grid grid-cols-2 gap-3">
          <Input v-model.number="quizForm.durationMinutes" label="Thời gian làm bài (phút)" type="number" min="1" />
          <Input v-model.number="quizForm.maxScore" label="Điểm tối đa" type="number" min="1" />
        </div>

        <!-- Questions List -->
        <div class="border-t border-vdsa-border pt-4 space-y-4">
          <div class="flex items-center justify-between">
            <h4 class="text-xs font-bold uppercase tracking-wider text-white">
              Danh sách câu hỏi ({{ quizForm.questions.length }})
            </h4>
            <Button type="button" size="sm" variant="secondary" class="text-xs" @click="addQuestion">
              <Plus :size="12" /> Thêm câu hỏi
            </Button>
          </div>

          <div
            v-for="(q, qIdx) in quizForm.questions"
            :key="qIdx"
            class="p-4 rounded-xl bg-vdsa-bg-secondary border border-vdsa-border space-y-3"
          >
            <div class="flex items-center justify-between">
              <span class="text-xs font-bold text-vdsa-purple-light">Câu hỏi #{{ qIdx + 1 }}</span>
              <button
                v-if="quizForm.questions.length > 1"
                type="button"
                class="text-xs text-red-400 hover:underline cursor-pointer"
                @click="removeQuestion(qIdx)"
              >
                Xóa câu này
              </button>
            </div>

            <Input v-model="q.content" label="Nội dung câu hỏi" placeholder="VD: Độ phức tạp trung bình của Quick Sort là gì?" required />

            <!-- Options -->
            <div class="space-y-2">
              <label class="text-[11px] font-bold text-slate-300 block">Các lựa chọn đáp án (chọn radio để đánh dấu đáp án đúng):</label>
              <div v-for="(_, optIdx) in q.options" :key="optIdx" class="flex items-center gap-2">
                <input
                  type="radio"
                  :name="`correct_${qIdx}`"
                  :checked="q.correctIndex === optIdx"
                  @change="q.correctIndex = optIdx"
                  class="text-emerald-500 focus:ring-0 cursor-pointer"
                  :title="`Chọn đáp án ${optIdx + 1} là đáp án đúng`"
                />
                <input
                  v-model="q.options[optIdx]"
                  type="text"
                  :placeholder="`Đáp án ${String.fromCharCode(65 + optIdx)}...`"
                  class="flex-1 px-3 py-1.5 rounded-lg bg-vdsa-surface border border-vdsa-border text-xs text-white outline-none focus:border-vdsa-accent"
                />
              </div>
            </div>

            <Input v-model="q.explanation" label="Giải thích chi tiết (hiện sau khi nộp)" placeholder="Giải thích vì sao đáp án này đúng..." />
          </div>
        </div>

        <div class="flex items-center justify-end gap-2 pt-4 border-t border-vdsa-border">
          <Button variant="ghost" type="button" @click="createModalOpen = false">Hủy</Button>
          <Button variant="primary" type="submit" :loading="creating">Lưu & Gắn vào bài học</Button>
        </div>
      </form>
    </Modal>
  </div>
</template>
