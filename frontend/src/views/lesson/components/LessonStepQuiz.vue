<template>
  <div class="lesson-step-quiz relative flex flex-col h-full overflow-hidden font-sans bg-vdsa-bg">
    <!-- Animated background accents -->
    <div class="absolute inset-0 z-0 pointer-events-none overflow-hidden">
      <div class="absolute -top-[20%] -left-[10%] w-[50%] h-[50%] rounded-full bg-accent/20 blur-[120px] mix-blend-screen animate-pulse"></div>
      <div class="absolute top-[60%] -right-[10%] w-[60%] h-[60%] rounded-full bg-indigo-600/10 blur-[150px] mix-blend-screen" style="animation: pulse 8s infinite alternate;"></div>
    </div>

    <!-- Header & Progress -->
    <header class="relative z-10 px-8 pt-8 pb-4 shrink-0 flex flex-col items-center">
      <div class="inline-flex items-center justify-center gap-2 px-4 py-1.5 rounded-full bg-white/5 border border-white/10 backdrop-blur-md text-xs font-bold text-accent uppercase tracking-widest mb-4 shadow-xl">
        <BaseIcon name="puzzle" class="w-4 h-4 text-accent" />
        <span>Kiểm Tra Trắc Nghiệm</span>
      </div>

      <div class="w-full max-w-3xl flex flex-col gap-3">
        <div class="flex justify-between items-end">
          <h2 class="text-2xl font-black text-white tracking-tight">
            Câu {{ currentIndex + 1 }} <span class="text-vdsa-muted font-medium text-lg">/ {{ questions.length }}</span>
          </h2>
          <span class="text-xs font-semibold" :class="answeredCount === questions.length ? 'text-vdsa-green' : 'text-vdsa-yellow'">
            Đã hoàn thành {{ Math.round((answeredCount / questions.length) * 100) || 0 }}%
          </span>
        </div>
        <!-- Sleek Progress Bar -->
        <div class="w-full h-2 bg-white/5 rounded-full overflow-hidden border border-white/5">
          <div
            class="h-full rounded-full transition-all duration-700 ease-out"
            :class="answeredCount === questions.length ? 'bg-gradient-to-r from-vdsa-accent-green to-vdsa-green' : 'bg-gradient-to-r from-vdsa-accent to-indigo-400'"
            :style="{ width: `${(answeredCount / questions.length) * 100}%` }"
          ></div>
        </div>

        <!-- Question Navigator Dots -->
        <div v-if="questions.length > 0" class="mt-4 flex gap-2 flex-wrap justify-center sm:justify-start">
          <button
            v-for="(q, idx) in questions"
            :key="q.id"
            @click="goToQuestion(idx)"
            class="group relative w-8 h-8 rounded-xl text-xs font-bold transition-all duration-300 flex items-center justify-center cursor-pointer overflow-hidden backdrop-blur-md"
            :class="[
              idx === currentIndex ? 'ring-2 ring-vdsa-accent ring-offset-2 ring-offset-vdsa-bg text-white bg-accent/80' : '',
              idx !== currentIndex && userAnswers[q.id] !== undefined && !isSubmitted ? 'bg-accent/20 text-accent border border-accent/30 hover:bg-vdsa-accent/30' : '',
              idx !== currentIndex && userAnswers[q.id] !== undefined && isSubmitted && userAnswers[q.id] === q.correctIndex ? 'bg-vdsa-green/20 border-vdsa-green/50 text-vdsa-green' : '',
              idx !== currentIndex && userAnswers[q.id] !== undefined && isSubmitted && userAnswers[q.id] !== q.correctIndex ? 'bg-vdsa-red/20 border-vdsa-red/50 text-vdsa-red' : '',
              idx !== currentIndex && userAnswers[q.id] === undefined ? 'bg-white/5 border border-white/10 text-vdsa-muted hover:bg-white/10 hover:text-white' : '',
              !isSubmitted && unansweredIndices.includes(idx) && showUnansweredWarning ? 'ring-2 ring-amber-400 ring-offset-1 ring-offset-vdsa-bg animate-pulse' : '',
            ]"
          >
            <span v-if="isSubmitted && userAnswers[q.id] === q.correctIndex"><BaseIcon name="check" class="w-4 h-4" /></span>
            <span v-else-if="isSubmitted && userAnswers[q.id] !== undefined && userAnswers[q.id] !== q.correctIndex"><BaseIcon name="close" class="w-4 h-4" /></span>
            <span v-else>{{ idx + 1 }}</span>
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
                  userAnswers[currentQuestion.id] === oIdx && !isSubmitted ? 'bg-accent/20 border-accent/50 ring-1 ring-vdsa-accent shadow-[0_0_20px_rgba(99,102,241,0.2)] text-white' : '',
                  !isSubmitted && userAnswers[currentQuestion.id] !== oIdx ? 'bg-white/5 border-white/10 text-vdsa-secondary hover:bg-white/10 hover:border-white/20 hover:text-white cursor-pointer hover:-translate-y-0.5' : '',
                  isSubmitted && oIdx === currentQuestion.correctIndex ? 'bg-vdsa-green/20 border-vdsa-green/50 ring-1 ring-vdsa-accent-green shadow-[0_0_20px_rgba(16,185,129,0.2)] text-white' : '',
                  isSubmitted && userAnswers[currentQuestion.id] === oIdx && oIdx !== currentQuestion.correctIndex ? 'bg-vdsa-red/20 border-vdsa-red/50 ring-1 ring-vdsa-accent-red text-white' : '',
                  isSubmitted && oIdx !== currentQuestion.correctIndex && userAnswers[currentQuestion.id] !== oIdx ? 'bg-black/20 border-transparent text-vdsa-muted opacity-40 cursor-default' : '',
                ]"
                :disabled="isSubmitted"
              >
                <!-- Option Letter Bubble -->
                <div
                  class="w-10 h-10 rounded-xl flex items-center justify-center shrink-0 text-sm font-bold transition-all duration-300"
                  :class="[
                    userAnswers[currentQuestion.id] === oIdx && !isSubmitted ? 'bg-accent text-white shadow-lg shadow-vdsa-accent/40' : '',
                    !isSubmitted && userAnswers[currentQuestion.id] !== oIdx ? 'bg-black/30 text-vdsa-muted group-hover:bg-white/10 group-hover:text-white' : '',
                    isSubmitted && oIdx === currentQuestion.correctIndex ? 'bg-vdsa-green text-white shadow-lg shadow-vdsa-accent-green/40' : '',
                    isSubmitted && userAnswers[currentQuestion.id] === oIdx && oIdx !== currentQuestion.correctIndex ? 'bg-vdsa-red text-white' : '',
                    isSubmitted && oIdx !== currentQuestion.correctIndex && userAnswers[currentQuestion.id] !== oIdx ? 'bg-black/40 text-vdsa-disabled' : '',
                  ]"
                >
                  <span v-if="isSubmitted && oIdx === currentQuestion.correctIndex"><BaseIcon name="check" class="w-5 h-5" /></span>
                  <span v-else-if="isSubmitted && userAnswers[currentQuestion.id] === oIdx && oIdx !== currentQuestion.correctIndex"><BaseIcon name="close" class="w-5 h-5" /></span>
                  <span v-else>{{ String.fromCharCode(65 + oIdx) }}</span>
                </div>

                <span class="text-sm sm:text-base font-medium flex-1">{{ opt }}</span>
              </button>
            </div>

            <!-- Explanation Box -->
            <Transition name="fade-slide-up">
              <div v-if="isSubmitted"
                   class="mt-2 p-6 rounded-3xl border backdrop-blur-xl shadow-2xl relative overflow-hidden"
                   :class="userAnswers[currentQuestion.id] === currentQuestion.correctIndex ? 'border-vdsa-green/30 bg-vdsa-green/10' : 'border-vdsa-red/30 bg-vdsa-red/10'">
                <div class="absolute top-0 left-0 w-1 h-full" :class="userAnswers[currentQuestion.id] === currentQuestion.correctIndex ? 'bg-vdsa-green' : 'bg-vdsa-red'"></div>
                <div class="flex items-center gap-3 mb-3">
                  <div class="p-2 rounded-full" :class="userAnswers[currentQuestion.id] === currentQuestion.correctIndex ? 'bg-vdsa-green/20 text-vdsa-green' : 'bg-vdsa-red/20 text-vdsa-red'">
                    <BaseIcon :name="userAnswers[currentQuestion.id] === currentQuestion.correctIndex ? 'check' : 'close'" class="w-5 h-5" />
                  </div>
                  <h4 class="text-lg font-bold" :class="userAnswers[currentQuestion.id] === currentQuestion.correctIndex ? 'text-vdsa-green' : 'text-vdsa-red'">
                    {{ userAnswers[currentQuestion.id] === currentQuestion.correctIndex ? 'Chính xác!' : 'Chưa chính xác!' }}
                  </h4>
                </div>
                <div class="text-sm text-white/90 leading-relaxed pl-12">
                  <span v-if="currentQuestion.explanation">{{ currentQuestion.explanation }}</span>
                  <span v-else>Đáp án đúng là: <strong class="text-white">{{ currentQuestion.options[currentQuestion.correctIndex] }}</strong></span>
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
            @click="prevQuestion"
            :disabled="currentIndex === 0"
            class="w-11 h-11 rounded-2xl flex items-center justify-center transition-all duration-300"
            :class="currentIndex === 0 ? 'bg-white/5 text-vdsa-disabled cursor-not-allowed' : 'bg-white/10 hover:bg-white/20 text-white cursor-pointer shadow-lg hover:-translate-y-0.5'"
          >
            <BaseIcon name="arrow-left" class="w-5 h-5" />
          </button>

          <button
            @click="nextQuestion"
            :disabled="currentIndex === questions.length - 1"
            class="w-11 h-11 rounded-2xl flex items-center justify-center transition-all duration-300"
            :class="currentIndex === questions.length - 1 ? 'bg-white/5 text-vdsa-disabled cursor-not-allowed' : 'bg-accent/20 hover:bg-vdsa-accent/40 text-accent border border-accent/20 cursor-pointer shadow-lg hover:-translate-y-0.5'"
          >
            <BaseIcon name="arrow-right" class="w-5 h-5" />
          </button>
        </div>

        <div class="w-px h-8 bg-white/10 mx-1 hidden sm:block"></div>

        <button
          v-if="isSubmitted"
          @click="resetQuiz"
          class="px-6 py-2.5 rounded-2xl bg-white/10 hover:bg-white/20 text-white text-sm font-bold transition-all duration-300 cursor-pointer shadow-lg hover:-translate-y-0.5"
        >
          Làm lại
        </button>

        <button
          v-if="!isSubmitted"
          @click="submitQuiz"
          :disabled="answeredCount !== questions.length"
          class="px-8 py-2.5 rounded-2xl text-sm font-bold transition-all duration-300 shadow-lg flex items-center gap-2 group"
          :class="answeredCount === questions.length ? 'bg-accent hover:bg-vdsa-accent-light text-white shadow-[0_0_20px_rgba(99,102,241,0.4)] hover:-translate-y-1 cursor-pointer' : 'bg-white/5 text-vdsa-disabled opacity-60 cursor-not-allowed'"
        >
          <span>Nộp Bài</span>
          <BaseIcon name="check" class="w-4 h-4 transition-transform" :class="answeredCount === questions.length ? 'group-hover:scale-125' : ''" />
        </button>

        <button
          v-if="isSubmitted && quizPassed"
          @click="completeStep"
          class="px-6 py-2.5 rounded-2xl bg-gradient-to-r from-vdsa-accent-green to-vdsa-green hover:from-vdsa-green hover:to-vdsa-green text-black text-sm font-bold transition-all duration-300 cursor-pointer shadow-[0_0_20px_rgba(16,185,129,0.4)] flex items-center gap-2 hover:-translate-y-1"
        >
          Tiếp Tục <BaseIcon name="arrow-right" class="w-4 h-4" />
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue';
import type { QuizQuestion } from '../../../features/lesson/types/lesson.types';
import BaseIcon from '../../../shared/components/BaseIcon.vue';

const props = withDefaults(defineProps<{
  questions?: QuizQuestion[];
}>(), {
  questions: () => [],
});

const emit = defineEmits<{
  (e: 'submit', answers: Record<string, number>): void;
  (e: 'completeStep'): void;
}>();

const PASS_THRESHOLD = 0.7;

const userAnswers = ref<Record<string, number>>({});
const isSubmitted = ref(false);
const currentIndex = ref(0);
const showUnansweredWarning = ref(false);

const currentQuestion = computed(() => props.questions[currentIndex.value] ?? null);
const answeredCount = computed(() => Object.keys(userAnswers.value).length);

const unansweredIndices = computed(() => {
  return props.questions
    .map((q, idx) => (userAnswers.value[q.id] === undefined ? idx : -1))
    .filter(idx => idx !== -1);
});

const quizScore = computed(() => {
  let score = 0;
  for (const q of props.questions) {
    if (userAnswers.value[q.id] === q.correctIndex) score++;
  }
  return score;
});

const quizPassed = computed(() => {
  if (props.questions.length === 0) return false;
  return quizScore.value / props.questions.length >= PASS_THRESHOLD;
});

function selectAnswer(questionId: string, optionIdx: number): void {
  userAnswers.value = { ...userAnswers.value, [questionId]: optionIdx };
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
    // First click: show warning and navigate to first unanswered
    showUnansweredWarning.value = true;
    if (!unanswered.includes(currentIndex.value)) {
      currentIndex.value = unanswered[0];
    }
    return;
  }

  // All answered OR second click after warning → submit
  doSubmit();
}

function forceSubmit(): void {
  doSubmit();
}

function doSubmit(): void {
  showUnansweredWarning.value = false;
  isSubmitted.value = true;
  emit('submit', { ...userAnswers.value });
}

function resetQuiz(): void {
  isSubmitted.value = false;
  showUnansweredWarning.value = false;
  userAnswers.value = {};
  currentIndex.value = 0;
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
