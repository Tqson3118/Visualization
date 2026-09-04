<template>
  <Teleport to="body">
    <div
      v-if="isOpen"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/75 backdrop-blur-sm transition-opacity"
      @click.self="$emit('close')"
    >
      <div class="bg-vdsa-surface border border-vdsa-border rounded-2xl max-w-2xl w-full max-h-[85vh] flex flex-col shadow-2xl overflow-hidden animate-fade-in-up text-white text-[13px]">
        <!-- Modal Header -->
        <div class="flex items-center justify-between px-6 py-4 border-b border-vdsa-border bg-vdsa-bg-secondary shrink-0">
          <div class="flex items-center gap-2.5">
            <button
              v-if="selectedAttempt"
              @click="selectedAttempt = null"
              class="p-1 rounded-lg hover:bg-white/10 text-vdsa-muted hover:text-white transition-colors mr-1"
              title="Quay lại danh sách"
            >
              <BaseIcon name="arrow-left" class="w-4 h-4" />
            </button>
            <BaseIcon name="clock" class="w-5 h-5 text-vdsa-purple-light" />
            <div>
              <h3 class="text-base font-bold text-white">
                {{ selectedAttempt ? `Chi tiết bài làm - Lần ${selectedAttemptNumber}` : 'Lịch sử làm bài Quiz' }}
              </h3>
              <p class="text-xs text-vdsa-muted">
                {{ selectedAttempt ? `Điểm: ${selectedAttempt.score}/${questions.length} (${Math.round((selectedAttempt.score / (questions.length || 1)) * 100)}%)` : `${historyList.length} lần nộp bài được ghi nhận` }}
              </p>
            </div>
          </div>
          <button
            @click="$emit('close')"
            class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-white/10 transition-colors"
          >
            <BaseIcon name="close" class="w-4 h-4" />
          </button>
        </div>

        <!-- Modal Body -->
        <div class="flex-1 overflow-y-auto p-6 custom-scrollbar">
          <!-- Loading State -->
          <div v-if="loading" class="py-12 flex flex-col items-center justify-center gap-3 text-vdsa-muted">
            <BaseIcon name="spinner" class="w-6 h-6 animate-spin text-vdsa-accent" />
            <p class="text-xs">Đang tải lịch sử làm bài...</p>
          </div>

          <!-- DETAIL VIEW: Xem chi tiết 1 lần nộp -->
          <div v-else-if="selectedAttempt" class="space-y-6">
            <div class="flex items-center justify-between bg-white/5 border border-white/10 p-3.5 rounded-xl">
              <div>
                <span class="text-xs text-vdsa-muted">Thời gian nộp:</span>
                <p class="text-xs font-semibold text-white mt-0.5">{{ formatDate(selectedAttempt.submittedAt) }}</p>
              </div>
              <div class="flex items-center gap-2">
                <span
                  class="px-3 py-1 rounded-full text-xs font-bold"
                  :class="isAttemptPassed(selectedAttempt) ? 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/30' : 'bg-rose-500/20 text-rose-400 border border-rose-500/30'"
                >
                  {{ isAttemptPassed(selectedAttempt) ? 'Đạt (≥ 70%)' : 'Chưa đạt' }}
                </span>
                <button
                  @click="onRestore(selectedAttempt)"
                  class="px-3 py-1 bg-vdsa-accent hover:bg-vdsa-accent-dark text-white rounded-lg text-xs font-semibold transition-colors flex items-center gap-1"
                  title="Khôi phục các câu trả lời của lần này vào bài tập"
                >
                  <BaseIcon name="refresh" class="w-3.5 h-3.5" />
                  Khôi phục đáp án
                </button>
              </div>
            </div>

            <!-- Question Review List -->
            <div class="space-y-5">
              <div
                v-for="(q, qIdx) in questions"
                :key="q.id ?? qIdx"
                class="bg-vdsa-surface border border-vdsa-border rounded-xl p-4.5 space-y-3"
              >
                <!-- Question Title -->
                <div class="flex items-start justify-between gap-3">
                  <div class="flex items-start gap-2">
                    <span class="px-2 py-0.5 rounded bg-white/10 text-xs font-bold text-vdsa-purple-light shrink-0">
                      Câu {{ qIdx + 1 }}
                    </span>
                    <h4 class="font-semibold text-white leading-relaxed text-[13.5px]">
                      {{ q.questionText || (q as any).content || (q as any).question }}
                    </h4>
                  </div>
                  <span
                    class="shrink-0 px-2 py-0.5 rounded text-[11px] font-bold"
                    :class="isQuestionCorrectInAttempt(q, selectedAttempt) ? 'bg-emerald-500/20 text-emerald-400' : 'bg-rose-500/20 text-rose-400'"
                  >
                    {{ isQuestionCorrectInAttempt(q, selectedAttempt) ? 'Đúng' : 'Sai' }}
                  </span>
                </div>

                <!-- Options -->
                <div class="space-y-2 pt-1">
                  <div
                    v-for="(opt, oIdx) in q.options"
                    :key="oIdx"
                    class="p-2.5 rounded-lg text-xs flex items-center justify-between border transition-colors"
                    :class="getOptionClass(q, oIdx, selectedAttempt)"
                  >
                    <div class="flex items-center gap-2">
                      <span class="font-bold text-white/50 w-5">{{ String.fromCharCode(65 + oIdx) }}.</span>
                      <span :class="isOptionSelectedInAttempt(q, oIdx, selectedAttempt) ? 'font-bold text-white' : 'text-vdsa-secondary'">
                        {{ opt }}
                      </span>
                    </div>

                    <div class="flex items-center gap-1.5 shrink-0 text-[11px] font-semibold">
                      <template v-if="isOptionSelectedInAttempt(q, oIdx, selectedAttempt)">
                        <span v-if="isOptionCorrect(q, oIdx)" class="text-emerald-400 flex items-center gap-1">
                          <BaseIcon name="check" class="w-3.5 h-3.5" /> Bạn chọn (Đúng)
                        </span>
                        <span v-else class="text-rose-400 flex items-center gap-1">
                          <BaseIcon name="close" class="w-3.5 h-3.5" /> Bạn chọn (Sai)
                        </span>
                      </template>
                      <template v-else-if="isOptionCorrect(q, oIdx)">
                        <span class="text-emerald-400/80 italic">Đáp án đúng</span>
                      </template>
                    </div>
                  </div>
                </div>

                <!-- Explanation -->
                <div
                  v-if="attemptExplanation(q, selectedAttempt)"
                  class="mt-2 p-3 rounded-lg bg-white/5 border border-white/10 text-xs text-vdsa-muted leading-relaxed"
                >
                  <div class="flex items-center gap-1.5 text-vdsa-yellow font-bold text-[11px] mb-1">
                    <BaseIcon name="info" class="w-3.5 h-3.5" />
                    <span>Giải thích</span>
                  </div>
                  <p class="text-vdsa-secondary">{{ attemptExplanation(q, selectedAttempt) }}</p>
                </div>
              </div>
            </div>
          </div>

          <!-- LIST VIEW: Danh sách các lần làm -->
          <div v-else>
            <div v-if="historyList.length === 0" class="py-12 text-center text-vdsa-muted">
              <BaseIcon name="document" class="w-10 h-10 mx-auto mb-3 opacity-30" />
              <p class="text-sm font-semibold text-white">Chưa có lịch sử làm bài</p>
              <p class="text-xs mt-1">Hoàn thành và bấm "Nộp Bài" để lưu lại kết quả của bạn.</p>
            </div>

            <div v-else class="space-y-3">
              <div
                v-for="(item, idx) in historyList"
                :key="item.id || idx"
                class="p-4 rounded-xl bg-vdsa-bg-secondary border border-vdsa-border hover:border-vdsa-border-strong transition-all flex items-center justify-between gap-4 group"
              >
                <div class="flex items-center gap-3.5 min-w-0">
                  <div
                    class="w-10 h-10 rounded-xl flex items-center justify-center font-mono font-bold text-sm shrink-0 shadow-inner"
                    :class="isAttemptPassed(item) ? 'bg-emerald-500/20 text-emerald-400 border border-emerald-500/30' : 'bg-rose-500/20 text-rose-400 border border-rose-500/30'"
                  >
                    #{{ historyList.length - idx }}
                  </div>
                  <div class="min-w-0">
                    <div class="flex items-center gap-2">
                      <h4 class="font-bold text-white text-[13.5px]">
                        Lần {{ historyList.length - idx }}
                        <span v-if="idx === 0" class="ml-1.5 text-[10px] uppercase tracking-wider font-semibold px-1.5 py-0.5 bg-vdsa-accent/20 text-vdsa-accent rounded">Mới nhất</span>
                      </h4>
                    </div>
                    <p class="text-xs text-vdsa-muted mt-0.5">
                      {{ formatDate(item.submittedAt) }}
                    </p>
                  </div>
                </div>

                <div class="flex items-center gap-3 shrink-0">
                  <div class="text-right">
                    <div class="font-bold text-sm" :class="isAttemptPassed(item) ? 'text-emerald-400' : 'text-rose-400'">
                      {{ item.score }} / {{ questions.length }} câu
                    </div>
                    <span class="text-[11px] text-vdsa-muted">
                      {{ Math.round((item.score / (questions.length || 1)) * 100) }}%
                    </span>
                  </div>

                  <button
                    @click="viewAttemptDetail(item, historyList.length - idx)"
                    class="px-3.5 py-1.5 bg-white/10 hover:bg-white/20 text-white rounded-lg text-xs font-semibold transition-colors flex items-center gap-1.5 group-hover:border-vdsa-border"
                  >
                    <span>Xem chi tiết</span>
                    <BaseIcon name="arrow-right" class="w-3.5 h-3.5" />
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Modal Footer -->
        <div class="px-6 py-3.5 border-t border-vdsa-border bg-vdsa-bg-secondary flex justify-between items-center text-xs text-vdsa-muted">
          <span>{{ selectedAttempt ? 'Bấm "Quay lại" để xem các lần nộp khác.' : 'Hệ thống tự động lưu trữ mọi lần nộp bài của bạn.' }}</span>
          <button
            @click="$emit('close')"
            class="px-4 py-1.5 rounded-lg bg-white/10 hover:bg-white/20 text-white font-semibold transition-colors"
          >
            Đóng
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import BaseIcon from '../../../shared/components/BaseIcon.vue';
import { fetchMySubmissions, type SubmissionSummaryDto } from '@/api/exercises';
import type { QuizQuestion } from '../../../features/lesson/types/lesson.types';

const props = withDefaults(defineProps<{
  isOpen: boolean;
  exerciseId?: string | number | null;
  questions?: QuizQuestion[];
  initialSubmission?: {
    score: number;
    maxScore: number;
    passed: boolean;
    answersJson?: string | null;
    resultJson?: string | null;
    submittedAt?: string | null;
  } | null;
}>(), {
  isOpen: false,
  exerciseId: null,
  questions: () => [],
  initialSubmission: null,
});

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'restore', answers: Record<string, number[]>, attempt?: SubmissionSummaryDto): void;
}>();

const loading = ref(false);
const historyList = ref<SubmissionSummaryDto[]>([]);
const selectedAttempt = ref<SubmissionSummaryDto | null>(null);
const selectedAttemptNumber = ref<number>(1);

function formatDate(isoDate?: string | null): string {
  if (!isoDate) return 'Vừa xong';
  try {
    const d = new Date(isoDate);
    return d.toLocaleString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  } catch {
    return isoDate;
  }
}

function isAttemptPassed(item: SubmissionSummaryDto): boolean {
  const total = props.questions.length || 1;
  return item.score / total >= 0.7;
}

watch(() => props.isOpen, async (open) => {
  if (open) {
    selectedAttempt.value = null;
    await loadHistory();
  }
});

async function loadHistory(): Promise<void> {
  loading.value = true;
  historyList.value = [];
  try {
    const exId = Number(props.exerciseId);
    if (!isNaN(exId) && exId > 0) {
      const items = await fetchMySubmissions(exId, { pageSize: 50 });
      historyList.value = items;
    }
  } catch (err) {
    console.warn('Could not fetch quiz submissions:', err);
  } finally {
    // Nếu API rỗng hoặc lỗi nhưng có initialSubmission thì hiển thị nó
    if (historyList.value.length === 0 && props.initialSubmission) {
      historyList.value = [{
        id: 1,
        userId: 0,
        score: props.initialSubmission.score,
        submittedAt: props.initialSubmission.submittedAt || new Date().toISOString(),
        answersJson: props.initialSubmission.answersJson,
        resultJson: props.initialSubmission.resultJson,
      }];
    }
    loading.value = false;
  }
}

function viewAttemptDetail(attempt: SubmissionSummaryDto, numberLabel: number): void {
  selectedAttempt.value = attempt;
  selectedAttemptNumber.value = numberLabel;
}

function parseAttemptAnswers(attempt: SubmissionSummaryDto | null): Record<string, number[]> {
  if (!attempt || !attempt.answersJson) return {};
  try {
    const raw = attempt.answersJson;
    const parsed = typeof raw === 'string' ? JSON.parse(raw) : raw;
    const mapping: Record<string, number[]> = {};

    if (Array.isArray(parsed)) {
      props.questions.forEach((q, idx) => {
        if (parsed.length > idx) {
          const item = parsed[idx];
          if (Array.isArray(item)) mapping[String(q.id)] = item;
          else if (typeof item === 'number' && item >= 0) mapping[String(q.id)] = [item];
        }
      });
      return mapping;
    } else if (typeof parsed === 'object' && parsed !== null) {
      for (const [k, v] of Object.entries(parsed)) {
        if (Array.isArray(v)) mapping[k] = v as number[];
        else if (typeof v === 'number' && (v as number) >= 0) mapping[k] = [v as number];
      }
      return mapping;
    }
  } catch (err) {
    console.warn('Error parsing attempt answers', err);
  }
  return {};
}

function isOptionSelectedInAttempt(q: QuizQuestion, optionIdx: number, attempt: SubmissionSummaryDto | null): boolean {
  const ansMap = parseAttemptAnswers(attempt);
  const selected = ansMap[String(q.id)];
  return selected ? selected.includes(optionIdx) : false;
}

function attemptCorrectIndices(q: QuizQuestion, attempt: SubmissionSummaryDto | null): number[] {
  if (!attempt?.resultJson) return [];
  try {
    const results = JSON.parse(attempt.resultJson);
    if (!Array.isArray(results)) return [];
    const result = results.find((r: any) => String(r.questionId ?? r.QuestionId) === String(q.id));
    if (!result) return [];
    const indices = result.correctIndices ?? result.CorrectIndices;
    const index = result.correctIndex ?? result.CorrectIndex;
    return Array.isArray(indices) ? indices.map(Number) : (typeof index === 'number' ? [index] : []);
  } catch {
    return [];
  }
}

function isOptionCorrect(q: QuizQuestion, optionIdx: number, attempt: SubmissionSummaryDto | null = selectedAttempt.value): boolean {
  const resultCorrect = attemptCorrectIndices(q, attempt);
  if (resultCorrect.length > 0) return resultCorrect.includes(optionIdx);
  if (q.correctIndices && q.correctIndices.length > 0) return q.correctIndices.includes(optionIdx);
  return q.correctIndex === optionIdx;
}

function isQuestionCorrectInAttempt(q: QuizQuestion, attempt: SubmissionSummaryDto | null): boolean {
  const ansMap = parseAttemptAnswers(attempt);
  const selected = ansMap[String(q.id)] ?? [];
  const resultCorrect = attemptCorrectIndices(q, attempt);
  const correct = resultCorrect.length > 0 ? resultCorrect : (q.correctIndices && q.correctIndices.length > 0 ? q.correctIndices : (q.correctIndex !== undefined ? [q.correctIndex] : []));
  if (selected.length === 0 || correct.length === 0) return false;
  return selected.length === correct.length && selected.every(idx => correct.includes(idx));
}

function getOptionClass(q: QuizQuestion, optionIdx: number, attempt: SubmissionSummaryDto | null): string {
  const isSelected = isOptionSelectedInAttempt(q, optionIdx, attempt);
  const isCorrect = isOptionCorrect(q, optionIdx, attempt);

  if (isSelected && isCorrect) {
    return 'bg-emerald-500/15 border-emerald-500/60 text-emerald-300';
  }
  if (isSelected && !isCorrect) {
    return 'bg-rose-500/15 border-rose-500/60 text-rose-300';
  }
  if (!isSelected && isCorrect) {
    return 'bg-emerald-500/5 border-emerald-500/30 text-emerald-400/90';
  }
  return 'bg-white/5 border-white/5 text-vdsa-muted';
}

function attemptExplanation(q: QuizQuestion, attempt: SubmissionSummaryDto | null): string {
  if (!attempt?.resultJson) return q.explanation || '';
  try {
    const results = JSON.parse(attempt.resultJson);
    if (!Array.isArray(results)) return q.explanation || '';
    const result = results.find((r: any) => String(r.questionId ?? r.QuestionId ?? '') === String(q.id));
    return result?.explanation ?? result?.Explanation ?? q.explanation ?? '';
  } catch {
    return q.explanation || '';
  }
}

function onRestore(attempt: SubmissionSummaryDto): void {
  const ansMap = parseAttemptAnswers(attempt);
  emit('restore', ansMap, attempt);
  emit('close');
}
</script>

<style scoped>
.custom-scrollbar::-webkit-scrollbar {
  width: 6px;
  height: 6px;
}
.custom-scrollbar::-webkit-scrollbar-track {
  background: transparent;
}
.custom-scrollbar::-webkit-scrollbar-thumb {
  background: rgba(255, 255, 255, 0.15);
  border-radius: 3px;
}
.custom-scrollbar::-webkit-scrollbar-thumb:hover {
  background: rgba(255, 255, 255, 0.25);
}
</style>
