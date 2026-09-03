<template>
  <div class="lesson-step-quiz relative flex flex-col h-full overflow-hidden font-sans bg-vdsa-bg">
    <!-- Animated background accents -->
    <div class="absolute inset-0 pointer-events-none overflow-hidden">
      <div class="absolute -top-[20%] -left-[10%] w-[50%] h-[50%] rounded-full bg-purple-600/20 blur-[120px] mix-blend-screen animate-pulse"></div>
      <div class="absolute top-[30%] -right-[10%] w-[40%] h-[40%] rounded-full bg-indigo-600/15 blur-[100px] mix-blend-screen"></div>
    </div>

    <!-- Top Floating Progress Bar & Nav -->
    <header class="relative z-10 w-full px-4 pt-4 pb-2 shrink-0">
      <div class="max-w-3xl mx-auto flex flex-col gap-3">
        <!-- Badge -->
        <div class="flex items-center justify-between">
          <div class="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-purple-500/15 border border-purple-500/30 text-xs font-bold text-purple-300">
            <BaseIcon name="puzzle" class="w-3.5 h-3.5 text-purple-400" />
            <span>Thử Thách Trắc Nghiệm</span>
          </div>
          <span class="text-xs font-mono font-bold text-slate-300">
            Đã chọn: <strong class="text-purple-300">{{ answeredCount }}</strong>/{{ questions.length }} câu
          </span>
        </div>

        <!-- Progress Bar -->
        <div class="w-full bg-white/10 h-2.5 rounded-full overflow-hidden backdrop-blur-md p-0.5 border border-white/10">
          <div
            class="h-full rounded-full transition-all duration-500 ease-out shadow-lg"
            :class="answeredCount === questions.length ? 'bg-gradient-to-r from-vdsa-accent-green to-vdsa-green' : 'bg-gradient-to-r from-purple-500 to-indigo-500'"
            :style="{ width: `${(answeredCount / questions.length) * 100}%` }"
          ></div>
        </div>

        <!-- Question Number Navigator -->
        <div class="flex items-center gap-2 overflow-x-auto py-1 scrollbar-none">
          <button
            v-for="(q, idx) in questions"
            :key="q.id"
            @click="goToQuestion(idx)"
            class="group relative w-8 h-8 rounded-xl text-xs font-bold transition-all duration-300 flex items-center justify-center cursor-pointer overflow-hidden backdrop-blur-md"
            :class="[
              idx === currentIndex ? 'ring-2 ring-purple-400 ring-offset-2 ring-offset-vdsa-bg text-white bg-purple-600 shadow-md shadow-purple-600/40' : '',
              idx !== currentIndex && hasUserAnswered(q.id) && !isSubmitted ? 'bg-purple-600/20 text-purple-300 border border-purple-500/40 hover:bg-purple-600/30' : '',
              idx !== currentIndex && hasUserAnswered(q.id) && isSubmitted && isQuestionCorrect(q) ? 'bg-vdsa-green/20 border-vdsa-green/50 text-vdsa-green' : '',
              idx !== currentIndex && hasUserAnswered(q.id) && isSubmitted && !isQuestionCorrect(q) ? 'bg-vdsa-red/20 border-vdsa-red/50 text-vdsa-red' : '',
              idx !== currentIndex && !hasUserAnswered(q.id) ? 'bg-white/5 border border-white/10 text-vdsa-muted hover:bg-white/10 hover:text-white' : '',
              !isSubmitted && unansweredIndices.includes(idx) && showUnansweredWarning ? 'ring-2 ring-amber-400 ring-offset-1 ring-offset-vdsa-bg animate-pulse' : '',
            ]"
          >
            <span v-if="isSubmitted && isQuestionCorrect(q)"><BaseIcon name="check" class="w-4 h-4" /></span>
            <span v-else-if="isSubmitted && hasUserAnswered(q.id) && !isQuestionCorrect(q)"><BaseIcon name="close" class="w-4 h-4" /></span>
            <span v-else>{{ idx + 1 }}</span>
          </button>
        </div>

        <!-- Previous Submission Info Banner -->
        <div v-if="props.initialSubmission && isSubmitted" class="mt-2 flex items-center justify-between px-4 py-2 rounded-2xl bg-white/5 border border-white/10 text-xs backdrop-blur-md">
          <div class="flex items-center gap-2">
            <span class="inline-flex items-center gap-1 font-bold" :class="quizPassed ? 'text-vdsa-green' : 'text-amber-400'">
              <BaseIcon :name="quizPassed ? 'check' : 'warning'" class="w-4 h-4" />
              {{ quizPassed ? 'Lần làm trước: ĐÃ ĐẠT' : 'Lần làm trước: CHƯA ĐẠT (Cần ≥ 70%)' }}
            </span>
            <span class="text-white/80 font-mono">
              ({{ quizScore }}/{{ questions.length }} câu đúng)
            </span>
          </div>
          <button
            @click="resetQuiz"
            class="px-3 py-1 rounded-xl bg-purple-600/20 hover:bg-purple-600/40 text-purple-300 text-xs font-bold transition-colors cursor-pointer border border-purple-500/30"
          >
            Làm lại bài
          </button>
        </div>
      </div>
    </header>

    <!-- Main Content Area -->
    <main class="relative z-10 flex-1 overflow-y-auto px-4 py-4 w-full scroll-smooth">
      <div v-if="questions.length > 0 && currentQuestion" class="max-w-3xl mx-auto flex flex-col gap-6 min-h-full pb-20">

        <!-- Question Card -->
        <Transition name="fade-slide" mode="out-in">
          <div :key="currentQuestion.id" class="w-full flex flex-col gap-6">

            <!-- The Question -->
            <div class="relative bg-white/5 backdrop-blur-2xl border border-white/10 rounded-3xl p-8 shadow-[0_8px_32px_rgba(0,0,0,0.3)]">
              <div class="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-vdsa-accent to-purple-500 opacity-50 rounded-t-3xl"></div>
              <div class="flex items-center justify-between gap-4 mb-3">
                <span v-if="isQuestionMulti(currentQuestion)" class="px-3 py-1 rounded-full bg-indigo-500/20 text-indigo-300 border border-indigo-500/30 text-xs font-bold flex items-center gap-1.5">
                  <BaseIcon name="check" class="w-3.5 h-3.5" />
                  Nhiều đáp án (Chọn tất cả đáp án đúng)
                </span>
                <span v-else class="px-3 py-1 rounded-full bg-white/5 text-vdsa-muted border border-white/10 text-xs font-semibold">
                  1 đáp án duy nhất
                </span>
              </div>
              <h3 class="text-xl sm:text-2xl font-bold text-white leading-relaxed">
                {{ currentQuestion.questionText }}
              </h3>
            </div>

            <!-- Unanswered Warning -->
            <Transition name="fade">
              <div v-if="showUnansweredWarning && unansweredIndices.includes(currentIndex) && !isSubmitted"
                   class="flex items-center gap-3 text-amber-400 bg-amber-400/10 border border-amber-400/20 backdrop-blur-md rounded-2xl px-5 py-3 shadow-lg shadow-amber-400/5">
                <BaseIcon name="warning" class="w-5 h-5 shrink-0" />
                <span class="text-sm font-semibold">Bạn chưa chọn đáp án cho câu hỏi này!</span>
              </div>
            </Transition>

            <!-- Options Grid -->
            <div class="grid grid-cols-1 gap-3">
              <button
                v-for="(opt, oIdx) in currentQuestion.options"
                :key="oIdx"
                @click="!isSubmitted && selectAnswer(currentQuestion.id, oIdx)"
                class="group relative w-full text-left p-5 rounded-2xl border transition-all duration-300 flex items-center gap-4 overflow-hidden"
                :class="[
                  isOptionSelected(currentQuestion.id, oIdx) && !isSubmitted ? 'bg-purple-600/20 border-purple-500 ring-2 ring-purple-500/80 shadow-[0_0_25px_rgba(168,85,247,0.3)] text-white font-semibold' : '',
                  !isSubmitted && !isOptionSelected(currentQuestion.id, oIdx) ? 'bg-white/5 border-white/10 text-slate-200 hover:bg-white/10 hover:border-purple-500/40 hover:text-white cursor-pointer hover:-translate-y-0.5' : '',
                  isSubmitted && isOptionCorrect(currentQuestion, oIdx) ? 'bg-vdsa-green/20 border-vdsa-green/50 ring-1 ring-vdsa-accent-green shadow-[0_0_20px_rgba(16,185,129,0.2)] text-white' : '',
                  isSubmitted && isOptionSelected(currentQuestion.id, oIdx) && !isOptionCorrect(currentQuestion, oIdx) ? 'bg-vdsa-red/20 border-vdsa-red/50 ring-1 ring-vdsa-accent-red text-white' : '',
                  isSubmitted && !isOptionCorrect(currentQuestion, oIdx) && !isOptionSelected(currentQuestion.id, oIdx) ? 'bg-black/20 border-transparent text-vdsa-muted opacity-40 cursor-default' : '',
                ]"
                :disabled="isSubmitted"
              >
                <!-- Option Letter or Checkbox Bubble -->
                <div
                  class="w-10 h-10 rounded-xl flex items-center justify-center shrink-0 text-sm font-bold transition-all duration-300"
                  :class="[
                    isOptionSelected(currentQuestion.id, oIdx) && !isSubmitted ? 'bg-purple-600 text-white shadow-lg shadow-purple-600/50' : '',
                    !isSubmitted && !isOptionSelected(currentQuestion.id, oIdx) ? 'bg-black/30 text-vdsa-muted group-hover:bg-purple-900/30 group-hover:text-purple-300' : '',
                    isSubmitted && isOptionCorrect(currentQuestion, oIdx) ? 'bg-vdsa-green text-white shadow-lg shadow-vdsa-accent-green/40' : '',
                    isSubmitted && isOptionSelected(currentQuestion.id, oIdx) && !isOptionCorrect(currentQuestion, oIdx) ? 'bg-vdsa-red text-white' : '',
                    isSubmitted && !isOptionCorrect(currentQuestion, oIdx) && !isOptionSelected(currentQuestion.id, oIdx) ? 'bg-black/40 text-vdsa-disabled' : '',
                  ]"
                >
                  <span v-if="isSubmitted && isOptionCorrect(currentQuestion, oIdx)"><BaseIcon name="check" class="w-5 h-5" /></span>
                  <span v-else-if="isSubmitted && isOptionSelected(currentQuestion.id, oIdx) && !isOptionCorrect(currentQuestion, oIdx)"><BaseIcon name="close" class="w-5 h-5" /></span>
                  <span v-else>{{ String.fromCharCode(65 + oIdx) }}</span>
                </div>

                <span class="text-sm sm:text-base font-medium flex-1">{{ opt }}</span>

                <!-- Multi-choice checkbox indicator when not submitted -->
                <div v-if="isQuestionMulti(currentQuestion) && !isSubmitted" class="w-5 h-5 rounded-md border flex items-center justify-center transition-all"
                     :class="isOptionSelected(currentQuestion.id, oIdx) ? 'bg-purple-600 border-purple-400 text-white' : 'border-white/20 bg-black/20'">
                  <BaseIcon v-if="isOptionSelected(currentQuestion.id, oIdx)" name="check" class="w-3.5 h-3.5" />
                </div>
              </button>
            </div>

            <!-- Explanation Box -->
            <Transition name="fade-slide-up">
              <div v-if="isSubmitted"
                   class="mt-2 p-6 rounded-3xl border backdrop-blur-xl shadow-2xl relative overflow-hidden"
                   :class="isQuestionCorrect(currentQuestion) ? 'border-vdsa-green/30 bg-vdsa-green/10' : 'border-vdsa-red/30 bg-vdsa-red/10'">
                <div class="absolute top-0 left-0 w-1 h-full" :class="isQuestionCorrect(currentQuestion) ? 'bg-vdsa-green' : 'bg-vdsa-red'"></div>
                <div class="flex items-center gap-3 mb-3">
                  <div class="p-2 rounded-full" :class="isQuestionCorrect(currentQuestion) ? 'bg-vdsa-green/20 text-vdsa-green' : 'bg-vdsa-red/20 text-vdsa-red'">
                    <BaseIcon :name="isQuestionCorrect(currentQuestion) ? 'check' : 'close'" class="w-5 h-5" />
                  </div>
                  <h4 class="text-lg font-bold" :class="isQuestionCorrect(currentQuestion) ? 'text-vdsa-green' : 'text-vdsa-red'">
                    {{ isQuestionCorrect(currentQuestion) ? 'Chính xác!' : 'Chưa chính xác!' }}
                  </h4>
                </div>
                <div class="text-sm text-white/90 leading-relaxed pl-12">
                  <span v-if="currentQuestion.explanation">{{ currentQuestion.explanation }}</span>
                  <span v-else-if="getCorrectAnswerNames(currentQuestion)">
                    Đáp án đúng: <strong class="text-white">{{ getCorrectAnswerNames(currentQuestion) }}</strong>
                  </span>
                </div>
              </div>
            </Transition>
          </div>
        </Transition>
      </div>

      <!-- Empty State -->
      <div v-else class="flex flex-col items-center justify-center h-full text-center py-20">
        <div class="relative w-24 h-24 mb-6">
          <div class="absolute inset-0 bg-accent/20 rounded-full blur-xl animate-pulse"></div>
          <div class="relative w-full h-full rounded-2xl bg-white/5 border border-white/10 flex items-center justify-center backdrop-blur-sm text-accent shadow-2xl">
            <BaseIcon name="puzzle" class="w-10 h-10" />
          </div>
        </div>
        <h3 class="text-2xl font-black text-white">Chưa có câu hỏi</h3>
        <p class="text-sm text-vdsa-muted mt-2 max-w-sm">Nội dung trắc nghiệm đang được cập nhật. Vui lòng quay lại sau.</p>
      </div>
    </main>

    <!-- Bottom Action Bar -->
    <div v-if="questions.length > 0" class="relative z-20 border-t border-white/5 bg-vdsa-bg-secondary/80 backdrop-blur-2xl px-6 py-4 flex items-center justify-between gap-4 shrink-0 shadow-[0_-10px_40px_rgba(0,0,0,0.3)]">

      <!-- Result Summary -->
      <div v-if="isSubmitted" class="flex-1">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 rounded-full flex items-center justify-center shadow-lg" :class="quizPassed ? 'bg-vdsa-green text-black' : 'bg-vdsa-red text-white'">
            <span class="font-black text-sm">{{ Math.round((quizScore || 0) / questions.length * 100) }}%</span>
          </div>
          <div>
            <h4 class="text-sm font-bold text-white leading-tight">
              {{ quizPassed ? 'Chúc mừng! Bạn đã qua bài.' : 'Chưa đạt yêu cầu (cần ≥ 70%).' }}
            </h4>
            <p class="text-xs font-medium" :class="quizPassed ? 'text-vdsa-green' : 'text-vdsa-red'">
              Đúng {{ quizScore }} / {{ questions.length }} câu
            </p>
          </div>
        </div>
      </div>

      <!-- Placeholder if not submitted -->
      <div v-else class="flex-1 hidden sm:block">
        <span class="text-xs font-semibold text-vdsa-muted">
          Chọn đáp án và bấm <strong class="text-white">Nộp Bài</strong> khi hoàn thành.
        </span>
      </div>

      <!-- Actions -->
      <div class="flex items-center gap-3 w-full sm:w-auto justify-between sm:justify-end">
        <div class="flex items-center gap-2">
          <button
            @click="showHistoryModal = true"
            class="px-3.5 py-2.5 rounded-2xl flex items-center gap-1.5 text-xs font-bold transition-all duration-300 bg-white/10 hover:bg-white/20 text-white cursor-pointer border border-white/15 shadow-lg hover:-translate-y-0.5"
            title="Lịch sử làm bài"
          >
            <BaseIcon name="clock" class="w-4 h-4 text-vdsa-purple-light" />
            <span class="hidden sm:inline">Lịch sử</span>
          </button>

          <button
            @click="prevQuestion"
            :disabled="currentIndex === 0"
            class="px-3.5 py-2.5 rounded-2xl flex items-center gap-1.5 text-xs font-bold transition-all duration-300"
            :class="currentIndex === 0 ? 'bg-white/5 text-white/30 cursor-not-allowed border border-white/5' : 'bg-white/10 hover:bg-white/20 text-white cursor-pointer border border-white/15 shadow-lg hover:-translate-y-0.5'"
            title="Câu trước"
          >
            <BaseIcon name="arrow-left" class="w-4 h-4" />
            <span class="hidden sm:inline">Câu trước</span>
          </button>

          <button
            v-if="currentIndex < questions.length - 1"
            @click="nextQuestion"
            class="px-4 py-2.5 rounded-2xl flex items-center gap-1.5 text-xs font-bold transition-all duration-300 bg-purple-600 hover:bg-purple-500 text-white border border-purple-500/40 cursor-pointer shadow-lg shadow-purple-600/30 hover:-translate-y-0.5"
            title="Câu tiếp theo"
          >
            <span>Câu tiếp theo</span>
            <BaseIcon name="arrow-right" class="w-4 h-4" />
          </button>
        </div>

        <div class="w-px h-8 bg-white/10 mx-1 hidden sm:block"></div>

        <button
          v-if="isSubmitted"
          @click="resetQuiz"
          class="px-6 py-2.5 rounded-2xl bg-white/10 hover:bg-white/20 text-white text-sm font-bold transition-all duration-300 cursor-pointer shadow-lg hover:-translate-y-0.5 whitespace-nowrap shrink-0"
        >
          Làm lại
        </button>

        <button
          v-if="!isSubmitted"
          @click="submitQuiz"
          :disabled="answeredCount !== questions.length"
          class="px-8 py-2.5 rounded-2xl text-sm font-extrabold transition-all duration-300 shadow-lg flex items-center gap-2 group whitespace-nowrap shrink-0"
          :class="answeredCount === questions.length ? 'bg-purple-600 hover:bg-purple-500 text-white shadow-[0_0_25px_rgba(168,85,247,0.5)] hover:-translate-y-1 cursor-pointer' : 'bg-white/5 text-vdsa-disabled opacity-60 cursor-not-allowed'"
        >
          <span>Nộp Bài</span>
          <BaseIcon name="check" class="w-4 h-4 transition-transform" :class="answeredCount === questions.length ? 'group-hover:scale-125' : ''" />
        </button>

        <button
          v-if="isSubmitted && quizPassed"
          @click="completeStep"
          class="px-6 py-2.5 rounded-2xl bg-gradient-to-r from-vdsa-accent-green to-vdsa-green hover:from-vdsa-green hover:to-vdsa-green text-black text-sm font-bold transition-all duration-300 cursor-pointer shadow-[0_0_20px_rgba(16,185,129,0.4)] flex items-center gap-2 hover:-translate-y-1 whitespace-nowrap shrink-0"
        >
          Tiếp Tục <BaseIcon name="arrow-right" class="w-4 h-4" />
        </button>
      </div>
    </div>

    <!-- Modal Lịch sử làm bài Quiz -->
    <QuizHistoryModal
      :is-open="showHistoryModal"
      :exercise-id="props.exerciseId"
      :questions="props.questions"
      :initial-submission="props.initialSubmission"
      @close="showHistoryModal = false"
      @restore="handleRestoreAnswers"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import type { QuizQuestion } from '../../../features/lesson/types/lesson.types';
import BaseIcon from '../../../shared/components/BaseIcon.vue';
import QuizHistoryModal from './QuizHistoryModal.vue';

const props = withDefaults(defineProps<{
  questions?: QuizQuestion[];
  initialSubmission?: {
    score: number;
    maxScore: number;
    passed: boolean;
    answersJson?: string | null;
    resultJson?: string | null;
    submittedAt?: string | null;
  } | null;
  exerciseId?: string | number | null;
}>(), {
  questions: () => [],
  initialSubmission: null,
  exerciseId: null,
});

const emit = defineEmits<{
  (e: 'submit', answers: Record<string, number | number[]>): void;
  (e: 'completeStep'): void;
}>();

const showHistoryModal = ref(false);

function handleRestoreAnswers(restoredAnswers: Record<string, number[]>): void {
  userAnswers.value = { ...restoredAnswers };
  isSubmitted.value = true;
  saveDraftAnswers();
}

const PASS_THRESHOLD = 0.7;

// Lưu danh sách index các đáp án đã chọn cho từng câu hỏi
const userAnswers = ref<Record<string, number[]>>({});
const isSubmitted = ref(false);
const currentIndex = ref(0);
const showUnansweredWarning = ref(false);

const currentQuestion = computed(() => props.questions[currentIndex.value] ?? null);

function isQuestionMulti(q: QuizQuestion | null | undefined): boolean {
  if (!q) return false;
  const t = (q.type || '').toUpperCase();
  return t === 'MULTIPLE' || t === 'MULTI' || (q.correctIndices?.length ?? 0) > 1;
}

function hasUserAnswered(questionId: string | number): boolean {
  const ans = userAnswers.value[String(questionId)];
  return ans !== undefined && ans.length > 0;
}

function isOptionSelected(questionId: string | number, optionIdx: number): boolean {
  const ans = userAnswers.value[String(questionId)];
  return ans ? ans.includes(optionIdx) : false;
}

function isOptionCorrect(q: QuizQuestion, optionIdx: number): boolean {
  if (q.correctIndices && q.correctIndices.length > 0) {
    return q.correctIndices.includes(optionIdx);
  }
  return q.correctIndex === optionIdx;
}

function isQuestionCorrect(q: QuizQuestion): boolean {
  const selected = userAnswers.value[String(q.id)] ?? [];
  const correct = q.correctIndices && q.correctIndices.length > 0 ? q.correctIndices : (q.correctIndex !== undefined ? [q.correctIndex] : []);
  if (selected.length === 0 || correct.length === 0) return false;
  return selected.length === correct.length && selected.every(idx => correct.includes(idx));
}

function getCorrectAnswerNames(q: QuizQuestion): string {
  const correct = q.correctIndices && q.correctIndices.length > 0 ? q.correctIndices : (q.correctIndex !== undefined ? [q.correctIndex] : []);
  return correct.map(idx => q.options[idx]).filter(Boolean).join(', ');
}

const answeredCount = computed(() => {
  return props.questions.filter(q => hasUserAnswered(q.id)).length;
});

const unansweredIndices = computed(() => {
  return props.questions
    .map((q, idx) => (!hasUserAnswered(q.id) ? idx : -1))
    .filter(idx => idx !== -1);
});

const quizScore = computed(() => {
  let score = 0;
  for (const q of props.questions) {
    if (isQuestionCorrect(q)) score++;
  }
  return score;
});

const quizPassed = computed(() => {
  if (props.questions.length === 0) return false;
  return quizScore.value / props.questions.length >= PASS_THRESHOLD;
});

const quizStorageKey = computed(() => {
  if (!props.questions || props.questions.length === 0) return '';
  const ids = props.questions.map(q => q.id).join('_');
  return `vdsa_quiz_draft_${ids}`;
});

function loadDraftAnswers(): void {
  if (!quizStorageKey.value) return;
  try {
    const raw = sessionStorage.getItem(quizStorageKey.value);
    if (raw) {
      userAnswers.value = JSON.parse(raw);
    }
  } catch {}
}

function saveDraftAnswers(): void {
  if (!quizStorageKey.value) return;
  try {
    sessionStorage.setItem(quizStorageKey.value, JSON.stringify(userAnswers.value));
  } catch {}
}

function clearDraftAnswers(): void {
  if (!quizStorageKey.value) return;
  try {
    sessionStorage.removeItem(quizStorageKey.value);
  } catch {}
}

function restoreFromInitialSubmission(): boolean {
  if (!props.initialSubmission || !props.initialSubmission.answersJson) return false;
  try {
    const raw = props.initialSubmission.answersJson;
    const parsed = typeof raw === 'string' ? JSON.parse(raw) : raw;
    if (Array.isArray(parsed)) {
      const mapping: Record<string, number[]> = {};
      props.questions.forEach((q, idx) => {
        if (parsed.length > idx) {
          const item = parsed[idx];
          if (Array.isArray(item)) {
            mapping[String(q.id)] = item;
          } else if (typeof item === 'number' && item >= 0) {
            mapping[String(q.id)] = [item];
          }
        }
      });
      userAnswers.value = mapping;
      isSubmitted.value = true;
      return true;
    } else if (typeof parsed === 'object' && parsed !== null) {
      const mapping: Record<string, number[]> = {};
      for (const [k, v] of Object.entries(parsed)) {
        if (Array.isArray(v)) {
          mapping[k] = v as number[];
        } else if (typeof v === 'number' && (v as number) >= 0) {
          mapping[k] = [v as number];
        }
      }
      userAnswers.value = mapping;
      isSubmitted.value = true;
      return true;
    }
  } catch (e) {
    console.warn('Error restoring initial quiz submission', e);
  }
  return false;
}

// Khôi phục đáp án từ kết quả đã nộp hoặc bản nháp khi câu hỏi thay đổi / khởi tạo
watch([() => props.questions, () => props.initialSubmission], () => {
  if (props.initialSubmission && props.initialSubmission.answersJson) {
    restoreFromInitialSubmission();
  } else if (!isSubmitted.value) {
    loadDraftAnswers();
  }
}, { immediate: true });

function selectAnswer(questionId: string | number, optionIdx: number): void {
  const key = String(questionId);
  const isMulti = isQuestionMulti(currentQuestion.value);
  const currentList = userAnswers.value[key] ? [...userAnswers.value[key]] : [];

  if (isMulti) {
    const foundIdx = currentList.indexOf(optionIdx);
    if (foundIdx >= 0) {
      currentList.splice(foundIdx, 1);
    } else {
      currentList.push(optionIdx);
    }
    if (currentList.length === 0) {
      const next = { ...userAnswers.value };
      delete next[key];
      userAnswers.value = next;
    } else {
      userAnswers.value = { ...userAnswers.value, [key]: currentList.sort((a, b) => a - b) };
    }
  } else {
    userAnswers.value = { ...userAnswers.value, [key]: [optionIdx] };
  }

  saveDraftAnswers();

  // Clear warning for this question
  if (showUnansweredWarning.value && unansweredIndices.value.length === 0) {
    showUnansweredWarning.value = false;
  }
}

function goToQuestion(idx: number): void {
  if (idx >= 0 && idx < props.questions.length) {
    currentIndex.value = idx;
  }
}

function prevQuestion(): void {
  if (currentIndex.value > 0) currentIndex.value--;
}

function nextQuestion(): void {
  if (currentIndex.value < props.questions.length - 1) currentIndex.value++;
}

function submitQuiz(): void {
  if (isSubmitted.value) return;

  const unanswered = unansweredIndices.value;
  if (unanswered.length > 0 && !showUnansweredWarning.value) {
    showUnansweredWarning.value = true;
    if (!unanswered.includes(currentIndex.value)) {
      currentIndex.value = unanswered[0];
    }
    return;
  }

  doSubmit();
}

function doSubmit(): void {
  showUnansweredWarning.value = false;
  isSubmitted.value = true;
  clearDraftAnswers();

  // Chuẩn hóa payload: Single choice gửi number, Multi choice gửi number[]
  const payload: Record<string, number | number[]> = {};
  for (const q of props.questions) {
    const selected = userAnswers.value[String(q.id)] ?? [];
    if (isQuestionMulti(q)) {
      payload[String(q.id)] = selected;
    } else {
      payload[String(q.id)] = selected.length > 0 ? selected[0] : -1;
    }
  }

  emit('submit', payload);
}

function resetQuiz(): void {
  isSubmitted.value = false;
  showUnansweredWarning.value = false;
  userAnswers.value = {};
  currentIndex.value = 0;
  clearDraftAnswers();
}

function completeStep(): void {
  if (!quizPassed.value || !isSubmitted.value) return;
  emit('completeStep');
}
</script>

<style scoped>
.fade-slide-enter-active, .fade-slide-leave-active {
  transition: all 0.4s cubic-bezier(0.4, 0, 0.2, 1);
}
.fade-slide-enter-from {
  opacity: 0;
  transform: translateX(20px);
}
.fade-slide-leave-to {
  opacity: 0;
  transform: translateX(-20px);
}

.fade-slide-up-enter-active, .fade-slide-up-leave-active {
  transition: all 0.5s cubic-bezier(0.34, 1.56, 0.64, 1);
}
.fade-slide-up-enter-from, .fade-slide-up-leave-to {
  opacity: 0;
  transform: translateY(20px);
}

.fade-enter-active, .fade-leave-active {
  transition: opacity 0.3s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
}
</style>
