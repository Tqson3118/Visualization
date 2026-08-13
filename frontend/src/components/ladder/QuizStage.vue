<script setup lang="ts">
// QuizStage — Bậc 1: bài trắc nghiệm (Màn 06 — layout 8/12 câu hỏi + 4/12 mini-map)
// Pass ≥ 60% → emit('passed', scorePct). Hỗ trợ chế độ luyện tập (practice).
// G-F2b: ProgressBar tiến độ câu hỏi + option dạng button selectable + toast kết quả
// + confetti 'success' khi pass. KHÔNG đổi logic submit (pre-check đủ câu — G-BF2).
// UI-PREMIUM 1B: thẻ câu hỏi enter scaleIn (motion-v, re-key theo câu) + error shake (ui-shake).
import { computed, reactive, ref, watch } from 'vue';
import { Motion } from 'motion-v';

import * as exercisesApi from '@/api/exercises';
import type { ExerciseDto, QuestionDto, SubmitResultDto } from '@/api/exercises';
import { messages } from '@/i18n/vi';
import { useUiStore } from '@/stores/ui';
import { fireConfetti } from '@/composables/useConfetti';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Skeleton from '@/components/ui/Skeleton.vue';

const reducedMotion =
  typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

const props = withDefaults(
  defineProps<{
    exercise: ExerciseDto | null;
    loading?: boolean;
    /** Chế độ luyện tập — không chấm điểm, xem kết quả ngay (FR-4.6) */
    practiceMode?: boolean;
  }>(),
  {
    exercise: null,
    loading: false,
    practiceMode: false,
  },
);

const emit = defineEmits<{
  passed: [scorePct: number];
  finished: [];
  'view-theory': [];
}>();

const ui = useUiStore();

const currentQuestion = ref(0);
const answers = reactive<Record<number, number[]>>({});
const result = ref<SubmitResultDto | null>(null);
const submitting = ref(false);
const submitError = ref('');
const practiceChecked = ref(false);

const questions = computed<QuestionDto[]>(() => props.exercise?.questions ?? []);
const answeredCount = computed(() => Object.keys(answers).length);
const progressPct = computed(() => (questions.value.length === 0 ? 0 : Math.round((answeredCount.value / questions.value.length) * 100)));

function isAnswered(index: number): boolean {
  return Array.isArray(answers[index]) && (answers[index] as number[]).length > 0;
}

function toggleOption(questionIndex: number, optionIndex: number): void {
  const question = questions.value[questionIndex];
  if (!question) return;
  const current = answers[questionIndex] ?? [];
  if (question.type === 'MULTIPLE') {
    answers[questionIndex] = current.includes(optionIndex)
      ? current.filter((i) => i !== optionIndex)
      : [...current, optionIndex];
  } else {
    answers[questionIndex] = [optionIndex];
  }
}

async function onSubmit(): Promise<void> {
  if (!props.exercise) return;
  // Bug P2 #5: kiểm tra đã trả lời ĐỦ câu trước khi nộp — liệt kê câu thiếu thay vì để server trả 400
  // QUESTION_ANSWER_MISMATCH (SETUP_TODO §6.5).
  const unanswered = questions.value.map((_, idx) => idx).filter((idx) => !isAnswered(idx));
  if (unanswered.length > 0) {
    submitError.value =
      `Bạn còn ${unanswered.length}/${questions.value.length} câu chưa trả lời: ` +
      unanswered.map((i) => i + 1).join(', ') +
      '. Hãy trả lời đủ trước khi nộp bài.';
    currentQuestion.value = unanswered[0]; // nhảy tới câu thiếu đầu tiên
    return;
  }
  submitting.value = true;
  submitError.value = '';
  try {
    const payload = Object.entries(answers).map(([qIndex, selected]) => ({
      questionId: questions.value[Number(qIndex)]?.id ?? Number(qIndex),
      selected,
    }));
    if (props.practiceMode) {
      // Luyện tập: chấm ngay không lưu submission — dùng practice endpoint
      result.value = await exercisesApi.practiceExercise(props.exercise.id, payload);
      practiceChecked.value = true;
    } else {
      result.value = await exercisesApi.submitExercise(props.exercise.id, { answers: payload });
    }
    showResultToast();
  } catch (err) {
    submitError.value = err instanceof Error ? err.message : 'Không thể nộp bài, vui lòng thử lại.';
  } finally {
    submitting.value = false;
  }
}

const scorePct = computed(() => {
  if (!result.value || result.value.maxScore === 0) return 0;
  return Math.round((result.value.score / result.value.maxScore) * 100);
});

const passed = computed(() => scorePct.value >= 60);

// G-F2b: khi kết quả hiện ra và ĐẠT → confetti 'success' (mỗi lần submit lại cũng bắn lại)
watch([result, passed], ([res, isPassed]) => {
  if (res && isPassed) fireConfetti('success');
});

/** G-F2b: toast kết quả ngay khi nộp xong (score + pass/fail). */
function showResultToast(): void {
  const pct = scorePct.value;
  if (pct >= 60) {
    ui.showToast(`🎉 Đạt yêu cầu — ${pct}%`, 'success');
  } else {
    ui.showToast(`Kết quả: ${pct}% — chưa đạt, thử lại nhé`, 'warning');
  }
}

function scoreColor(): string {
  if (scorePct.value >= 80) return 'var(--color-success)';
  if (scorePct.value >= 40) return 'var(--color-warning)';
  return 'var(--color-destructive)';
}

function retry(): void {
  result.value = null;
  practiceChecked.value = false;
  currentQuestion.value = 0;
  for (const key of Object.keys(answers)) delete answers[Number(key)];
}

function finish(): void {
  if (props.practiceMode) {
    emit('finished');
    return;
  }
  if (passed.value) emit('passed', scorePct.value);
  else emit('finished');
}
</script>

<template>
  <section class="quiz-stage">
    <div v-if="loading" class="quiz-stage__loading">
      <Skeleton height="80px" :lines="3" />
    </div>

    <EmptyState
      v-else-if="!exercise || questions.length === 0"
      icon="puzzle"
      :title="messages.common.comingSoon"
      description="Bài tập chưa có câu hỏi — hãy quay lại sau."
    />

    <div v-else-if="result" class="quiz-stage__result">
      <div
        class="quiz-stage__score-circle"
        :class="{ 'quiz-stage__score-circle--fail': !passed }"
        :style="{ '--score-color': scoreColor() }"
      >
        <span class="quiz-stage__score-value">{{ scorePct }}%</span>
      </div>
      <h3 class="quiz-stage__result-title">
        {{ practiceMode ? 'Kết quả luyện tập' : passed ? '🎉 Chúc mừng qua Bậc 1!' : 'Chưa đạt — làm lại trong phiên (miễn phí)' }}
      </h3>
      <p class="quiz-stage__result-meta">
        Điểm {{ result.score }}/{{ result.maxScore }} · Đúng {{ result.results.filter((r) => r.correct).length }}/{{
          result.results.length
        }}
      </p>

      <ul class="quiz-stage__explain">
        <li v-for="(item, idx) in result.results" :key="item.questionId" class="quiz-stage__explain-item">
          <Badge :variant="item.correct ? 'success' : 'danger'">
            {{ item.correct ? 'Đúng' : 'Sai' }}
          </Badge>
          <p class="quiz-stage__explain-text">{{ item.explanation }}</p>
          <p v-if="!item.correct" class="quiz-stage__explain-answer">
            Đáp án đúng: {{ (item.correctAnswer ?? []).map((i) => questions[idx]?.options[i] ?? `#${i + 1}`).join(', ') }}
          </p>
        </li>
      </ul>

      <div class="quiz-stage__actions">
        <Button variant="secondary" @click="retry">{{ messages.common.retry }}</Button>
        <Button @click="finish">{{ passed || practiceMode ? 'Tiếp tục' : 'Quay lại' }}</Button>
      </div>
    </div>

    <template v-else>
      <div class="quiz-stage__body">
        <div class="quiz-stage__question-area">
          <Motion
            as="div"
            class="quiz-stage__question-card"
            :key="currentQuestion"
            :initial="reducedMotion ? undefined : { opacity: 0, scale: 0.95 }"
            :animate="{ opacity: 1, scale: 1 }"
            :transition="{ duration: 0.35, ease: [0.16, 1, 0.3, 1] }"
          >
            <header class="quiz-stage__header">
              <h2 class="quiz-stage__title">{{ exercise.title }}</h2>
              <Badge variant="primary">Câu {{ currentQuestion + 1 }}/{{ questions.length }}</Badge>
            </header>

            <ProgressBar
              :value="progressPct"
              show-label
              size="sm"
              class="quiz-stage__progress"
              :variant="progressPct === 100 ? 'success' : 'default'"
            />

            <p class="quiz-stage__question">{{ questions[currentQuestion].content }}</p>

            <div class="quiz-stage__options">
              <label
                v-for="(option, optionIndex) in questions[currentQuestion].options"
                :key="optionIndex"
                class="quiz-stage__option"
                :class="{ 'quiz-stage__option--selected': (answers[currentQuestion] ?? []).includes(optionIndex) }"
              >
                <input
                  :type="questions[currentQuestion].type === 'MULTIPLE' ? 'checkbox' : 'radio'"
                  class="visually-hidden"
                  :checked="(answers[currentQuestion] ?? []).includes(optionIndex)"
                  @change="toggleOption(currentQuestion, optionIndex)"
                />
                <span class="quiz-stage__option-mark" aria-hidden="true">
                  {{ (answers[currentQuestion] ?? []).includes(optionIndex) ? '✓' : '' }}
                </span>
                {{ option }}
              </label>
            </div>
          </Motion>

          <p
            v-if="submitError"
            :key="submitError"
            class="quiz-stage__error quiz-stage__error--shake"
            role="alert"
          >
            {{ submitError }}
          </p>

          <div class="quiz-stage__nav">
            <Button variant="ghost" :disabled="currentQuestion === 0" @click="currentQuestion -= 1">
              Câu trước
            </Button>
            <Button
              v-if="currentQuestion < questions.length - 1"
              variant="secondary"
              @click="currentQuestion += 1"
            >
              Câu tiếp
            </Button>
            <Button :loading="submitting" @click="onSubmit">Nộp bài</Button>
          </div>
        </div>

        <aside class="quiz-stage__minimap">
          <h3 class="quiz-stage__minimap-title">Tiến độ</h3>
          <p class="quiz-stage__minimap-count">Đã trả lời {{ answeredCount }}/{{ questions.length }}</p>
          <div class="quiz-stage__minimap-grid">
            <button
              v-for="(q, idx) in questions"
              :key="q.id"
              type="button"
              class="quiz-stage__minimap-cell"
              :class="{
                'quiz-stage__minimap-cell--answered': isAnswered(idx),
                'quiz-stage__minimap-cell--current': idx === currentQuestion,
              }"
              :aria-label="`Nhảy tới câu ${idx + 1}`"
              @click="currentQuestion = idx"
            >
              {{ idx + 1 }}
            </button>
          </div>
        </aside>
      </div>
    </template>
  </section>
</template>

<style scoped>
.quiz-stage__body {
  display: grid;
  grid-template-columns: 3fr 1fr;
  gap: var(--space-lg);
}

/* Thẻ câu hỏi — nền cho enter scaleIn (UI-PREMIUM 1B) */
.quiz-stage__question-card {
  display: flex;
  flex-direction: column;
}

/* Error shake — sai thiếu câu / lỗi nộp (keyframes ui-shake trong global.css) */
.quiz-stage__error--shake {
  animation: ui-shake 400ms cubic-bezier(0.36, 0, 0.66, -0.56);
}

.quiz-stage__score-circle--fail {
  animation: ui-shake 500ms cubic-bezier(0.36, 0, 0.66, -0.56);
}

@media (prefers-reduced-motion: reduce) {
  .quiz-stage__error--shake,
  .quiz-stage__score-circle--fail {
    animation: none;
  }
}

.quiz-stage__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-md);
  margin-bottom: var(--space-md);
}

.quiz-stage__progress { margin-bottom: var(--space-md); }

.quiz-stage__title { font-size: var(--text-lg); }

.quiz-stage__question { font-size: var(--text-md); margin-bottom: var(--space-md); }

.quiz-stage__options { display: flex; flex-direction: column; gap: var(--space-sm); }

/* Option dạng nút selectable (G-F2b): hover nâng nhẹ, active = viền primary + check.
   GP-T9b (#17): padding ≥ 12px theo gợi ý review. */
.quiz-stage__option {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: 0.75rem var(--space-md);
  border: 2px solid var(--color-border);
  border-radius: var(--radius-md);
  cursor: pointer;
  font-size: var(--text-sm);
  font-weight: 600;
  background: var(--color-surface);
  transition: border-color 150ms ease, background 150ms ease, transform 150ms ease, box-shadow 150ms ease;
}

.quiz-stage__option:hover {
  border-color: var(--color-primary);
  background: var(--color-surface-hover);
  transform: translateY(-1px);
}

.quiz-stage__option--selected {
  border-color: var(--color-primary);
  background: color-mix(in srgb, var(--color-primary) 10%, transparent);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-primary) 16%, transparent);
}

.quiz-stage__option-mark {
  width: 20px;
  height: 20px;
  border-radius: 50%;
  border: 1px solid var(--color-border);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: var(--text-xs);
  color: var(--color-on-primary);
  background: transparent;
  transition: background 150ms ease, border-color 150ms ease;
}

.quiz-stage__option--selected .quiz-stage__option-mark { background: var(--color-primary); border-color: var(--color-primary); }

.quiz-stage__error { color: var(--color-destructive); font-size: var(--text-sm); margin-top: var(--space-sm); }

.quiz-stage__nav {
  display: flex;
  gap: var(--space-sm);
  margin-top: var(--space-lg);
  flex-wrap: wrap;
}

.quiz-stage__minimap {
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-md);
  align-self: start;
  position: sticky;
  top: 80px;
}

.quiz-stage__minimap-title { font-size: var(--text-sm); margin-bottom: var(--space-xs); }
.quiz-stage__minimap-count { font-size: var(--text-xs); color: var(--color-text-muted); margin-bottom: var(--space-sm); }

.quiz-stage__minimap-grid { display: grid; grid-template-columns: repeat(5, 1fr); gap: 6px; }

.quiz-stage__minimap-cell {
  aspect-ratio: 1;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  background: var(--color-surface);
  color: var(--color-text-muted);
  font-size: var(--text-xs);
  cursor: pointer;
}

.quiz-stage__minimap-cell--answered { background: color-mix(in srgb, var(--color-success) 18%, transparent); color: var(--color-success); }
.quiz-stage__minimap-cell--current { border-color: var(--color-primary); color: var(--color-primary); font-weight: 700; }

.quiz-stage__result { display: flex; flex-direction: column; align-items: center; gap: var(--space-md); text-align: center; padding: var(--space-lg) 0; }

.quiz-stage__score-circle {
  width: 120px;
  height: 120px;
  border-radius: 50%;
  border: 8px solid var(--score-color, var(--color-primary));
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: var(--text-2xl);
  font-weight: 800;
  color: var(--score-color, var(--color-primary));
}

.quiz-stage__result-title { font-size: var(--text-lg); }
.quiz-stage__result-meta { color: var(--color-text-muted); font-size: var(--text-sm); }

.quiz-stage__explain { list-style: none; display: flex; flex-direction: column; gap: var(--space-md); width: 100%; max-width: 42rem; text-align: left; }

.quiz-stage__explain-item { display: flex; flex-direction: column; gap: 4px; border: 1px solid var(--color-border); border-radius: var(--radius-md); padding: var(--space-sm) var(--space-md); }

.quiz-stage__explain-text { font-size: var(--text-sm); }
.quiz-stage__explain-answer { font-size: var(--text-xs); color: var(--color-success); }

.quiz-stage__actions { display: flex; gap: var(--space-sm); }

@media (max-width: 900px) {
  .quiz-stage__body { grid-template-columns: 1fr; }
  .quiz-stage__minimap { position: static; }
}
</style>
