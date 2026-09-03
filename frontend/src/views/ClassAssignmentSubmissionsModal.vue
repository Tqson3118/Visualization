<template>
  <Teleport to="body">
    <div
      v-if="isOpen"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm transition-opacity"
      @click.self="$emit('close')"
    >
      <div class="bg-vdsa-surface border border-vdsa-border rounded-2xl max-w-4xl w-full max-h-[90vh] flex flex-col shadow-2xl overflow-hidden animate-fade-in-up text-white text-[13px]">
        <!-- Header -->
        <div class="flex items-center justify-between px-6 py-4 border-b border-vdsa-border bg-vdsa-bg-secondary shrink-0">
          <div class="flex items-center gap-3">
            <button
              v-if="currentView !== 'students'"
              @click="backToPreviousView"
              class="p-1 rounded-lg hover:bg-white/10 text-vdsa-muted hover:text-white transition-colors mr-1"
              title="Quay lại"
            >
              <ArrowLeft :size="16" />
            </button>
            <div
              class="w-9 h-9 rounded-xl flex items-center justify-center shrink-0"
              :class="isCodeLab ? 'bg-amber-500/20 text-amber-400' : 'bg-indigo-500/20 text-indigo-400'"
            >
              <Code v-if="isCodeLab" :size="18" />
              <HelpCircle v-else :size="18" />
            </div>
            <div>
              <div class="flex items-center gap-2">
                <h3 class="text-base font-bold text-white">
                  {{ headerTitle }}
                </h3>
                <span
                  class="px-2 py-0.5 rounded text-[10px] font-bold uppercase tracking-wider"
                  :class="isCodeLab ? 'bg-amber-500/20 text-amber-400 border border-amber-500/30' : 'bg-indigo-500/20 text-indigo-400 border border-indigo-500/30'"
                >
                  {{ isCodeLab ? 'Code Lab' : 'Quiz' }}
                </span>
              </div>
              <p class="text-xs text-vdsa-muted mt-0.5">
                {{ headerSubtitle }}
              </p>
            </div>
          </div>

          <button
            @click="$emit('close')"
            class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-white/10 transition-colors"
          >
            <X :size="18" />
          </button>
        </div>

        <!-- Body -->
        <div class="flex-1 overflow-y-auto p-6 custom-scrollbar">
          <!-- Loading state -->
          <div v-if="loading" class="py-16 flex flex-col items-center justify-center gap-3 text-vdsa-muted">
            <Loader2 :size="24" class="animate-spin text-vdsa-accent" />
            <p class="text-xs">Đang tải danh sách bài nộp và học viên...</p>
          </div>

          <!-- Error or No Exercise Id -->
          <div v-else-if="!assignment?.exerciseId && studentSummaries.length === 0" class="py-12 text-center text-vdsa-muted">
            <FileQuestion :size="40" class="mx-auto mb-3 opacity-30" />
            <p class="text-sm font-semibold text-white">Bài gán này chưa có bài nộp nào</p>
            <p class="text-xs mt-1">Chưa có dữ liệu bài nộp được ghi nhận trong hệ thống.</p>
          </div>

          <!-- VIEW 3: CHI TIẾT 1 LẦN NỘP (Xem code cụ thể hoặc đáp án cụ thể) -->
          <div v-else-if="currentView === 'detail' && selectedAttempt" class="space-y-6">
            <!-- Metadata Bar -->
            <div class="flex flex-wrap items-center justify-between gap-3 bg-white/5 border border-white/10 p-4 rounded-xl">
              <div>
                <span class="text-xs text-vdsa-muted">Học viên:</span>
                <p class="text-sm font-bold text-white">{{ selectedStudent?.displayName }}</p>
                <p class="text-xs text-vdsa-muted font-mono mt-0.5">Nộp lúc: {{ formatDate(selectedAttempt.submittedAt) }}</p>
              </div>

              <div class="flex items-center gap-3">
                <template v-if="isCodeLab">
                  <div class="text-right">
                    <span class="text-xs text-vdsa-muted">Kết quả test</span>
                    <p class="text-sm font-bold" :class="selectedAttempt.passedTests === selectedAttempt.totalTests && selectedAttempt.totalTests > 0 ? 'text-emerald-400' : 'text-amber-400'">
                      {{ selectedAttempt.passedTests }}/{{ selectedAttempt.totalTests }} tests passed
                    </p>
                  </div>
                </template>
                <template v-else>
                  <div class="text-right">
                    <span class="text-xs text-vdsa-muted">Điểm số</span>
                    <p class="text-sm font-bold" :class="isQuizAttemptPassed(selectedAttempt) ? 'text-emerald-400' : 'text-rose-400'">
                      {{ selectedAttempt.score }}/{{ quizQuestions.length || selectedAttempt.maxScore || 10 }} điểm
                    </p>
                  </div>
                </template>
              </div>
            </div>

            <!-- CODE LAB DETAIL: Code cụ thể -->
            <div v-if="isCodeLab" class="space-y-3">
              <div class="flex items-center justify-between">
                <h4 class="text-xs font-bold uppercase tracking-wider text-vdsa-muted flex items-center gap-1.5">
                  <Code :size="14" class="text-amber-400" />
                  Mã nguồn học viên đã nộp (Lần {{ selectedAttemptNumber }})
                </h4>
                <button
                  @click="copyCode(selectedAttempt.code || '')"
                  class="px-2.5 py-1 rounded bg-white/10 hover:bg-white/20 text-xs text-white font-semibold transition-colors flex items-center gap-1"
                >
                  <Check v-if="isCodeCopied" :size="12" class="text-emerald-400" />
                  <Copy v-else :size="12" />
                  <span>{{ isCodeCopied ? 'Đã sao chép' : 'Sao chép code' }}</span>
                </button>
              </div>

              <div class="rounded-xl border border-vdsa-border overflow-hidden bg-[#0d1117] font-mono text-xs">
                <div class="flex items-center justify-between px-4 py-2 border-b border-vdsa-border bg-white/[0.03] text-[11px] text-vdsa-muted">
                  <span>JavaScript / Solution Code</span>
                  <span>{{ (selectedAttempt.code || '').split('\n').length }} dòng</span>
                </div>
                <pre class="p-4 overflow-x-auto text-[#c9d1d9] leading-relaxed select-text">{{ selectedAttempt.code || '// Học viên nộp bài rỗng' }}</pre>
              </div>
            </div>

            <!-- QUIZ DETAIL: Đáp án cụ thể từng câu -->
            <div v-else class="space-y-4">
              <h4 class="text-xs font-bold uppercase tracking-wider text-vdsa-muted flex items-center gap-1.5">
                <HelpCircle :size="14" class="text-indigo-400" />
                Chi tiết câu hỏi và đáp án học sinh đã chọn (Lần {{ selectedAttemptNumber }})
              </h4>

              <div class="space-y-4">
                <div
                  v-for="(q, qIdx) in quizQuestions"
                  :key="q.id || qIdx"
                  class="bg-vdsa-bg-secondary border border-vdsa-border rounded-xl p-4 space-y-3"
                >
                  <div class="flex items-start justify-between gap-2">
                    <div class="flex items-start gap-2">
                      <span class="px-2 py-0.5 rounded bg-white/10 text-xs font-bold text-indigo-300 shrink-0">
                        Câu {{ qIdx + 1 }}
                      </span>
                      <h5 class="font-semibold text-white leading-relaxed">
                        {{ q.content || q.question }}
                      </h5>
                    </div>
                    <span
                      class="px-2 py-0.5 rounded text-[11px] font-bold shrink-0"
                      :class="isQuestionCorrectInAttempt(q, selectedAttempt) ? 'bg-emerald-500/20 text-emerald-400' : 'bg-rose-500/20 text-rose-400'"
                    >
                      {{ isQuestionCorrectInAttempt(q, selectedAttempt) ? 'Đúng' : 'Sai' }}
                    </span>
                  </div>

                  <div class="space-y-2 pt-1">
                    <div
                      v-for="(opt, oIdx) in getQuestionOptions(q)"
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
                            <Check :size="13" /> Học sinh chọn (Đúng)
                          </span>
                          <span v-else class="text-rose-400 flex items-center gap-1">
                            <X :size="13" /> Học sinh chọn (Sai)
                          </span>
                        </template>
                        <template v-else-if="isOptionCorrect(q, oIdx)">
                          <span class="text-emerald-400/80 italic">Đáp án đúng</span>
                        </template>
                      </div>
                    </div>
                  </div>

                  <div v-if="q.explanation" class="p-3 rounded-lg bg-white/5 text-xs text-vdsa-muted">
                    <strong class="text-amber-400">Giải thích:</strong> {{ q.explanation }}
                  </div>
                </div>
              </div>
            </div>
          </div>

          <!-- VIEW 2: DANH SÁCH CÁC LẦN NỘP CỦA HỌC VIÊN ĐƯỢC CHỌN (Lần 1, Lần 2, Lần 3...) -->
          <div v-else-if="currentView === 'attempts' && selectedStudent" class="space-y-4">
            <div class="flex items-center justify-between bg-white/5 border border-white/10 p-3.5 rounded-xl">
              <div>
                <h4 class="font-bold text-sm text-white">{{ selectedStudent.displayName }}</h4>
                <p class="text-xs text-vdsa-muted">{{ selectedStudent.email || 'Học viên lớp' }}</p>
              </div>
              <span class="px-2.5 py-1 rounded-full text-xs font-semibold bg-vdsa-accent/20 text-vdsa-accent">
                {{ selectedStudent.attempts.length }} lần nộp bài
              </span>
            </div>

            <div v-if="selectedStudent.attempts.length === 0" class="py-12 text-center text-vdsa-muted">
              <FileQuestion :size="36" class="mx-auto mb-2 opacity-30" />
              <p class="text-xs">Học viên này chưa nộp bài lần nào.</p>
            </div>

            <div v-else class="space-y-3">
              <div
                v-for="(att, aIdx) in selectedStudent.attempts"
                :key="att.id || aIdx"
                class="p-4 rounded-xl bg-vdsa-bg-secondary border border-vdsa-border hover:border-vdsa-border-strong transition-all flex items-center justify-between gap-4"
              >
                <div class="flex items-center gap-3.5">
                  <div
                    class="w-10 h-10 rounded-xl flex items-center justify-center font-mono font-bold text-sm shrink-0 border"
                    :class="isCodeLab
                      ? (att.passedTests === att.totalTests && att.totalTests > 0 ? 'bg-emerald-500/20 text-emerald-400 border-emerald-500/30' : 'bg-amber-500/20 text-amber-400 border-amber-500/30')
                      : (isQuizAttemptPassed(att) ? 'bg-emerald-500/20 text-emerald-400 border-emerald-500/30' : 'bg-rose-500/20 text-rose-400 border-rose-500/30')"
                  >
                    #{{ selectedStudent.attempts.length - aIdx }}
                  </div>

                  <div>
                    <h5 class="font-bold text-white text-sm flex items-center gap-2">
                      Lần {{ selectedStudent.attempts.length - aIdx }}
                      <span v-if="aIdx === 0" class="text-[10px] uppercase font-semibold px-1.5 py-0.5 bg-vdsa-accent/20 text-vdsa-accent rounded">
                        Mới nhất
                      </span>
                    </h5>
                    <p class="text-xs text-vdsa-muted mt-0.5">
                      {{ formatDate(att.submittedAt) }}
                    </p>
                  </div>
                </div>

                <div class="flex items-center gap-4">
                  <!-- Info badge -->
                  <div class="text-right">
                    <template v-if="isCodeLab">
                      <div class="font-bold text-sm text-white">
                        {{ att.passedTests }}/{{ att.totalTests }} Tests
                      </div>
                      <span class="text-[11px] text-vdsa-muted">Điểm: {{ att.score }}</span>
                    </template>
                    <template v-else>
                      <div class="font-bold text-sm" :class="isQuizAttemptPassed(att) ? 'text-emerald-400' : 'text-rose-400'">
                        {{ att.score }} câu đúng
                      </div>
                      <span class="text-[11px] text-vdsa-muted">{{ isQuizAttemptPassed(att) ? 'Đạt' : 'Chưa đạt' }}</span>
                    </template>
                  </div>

                  <button
                    @click="viewAttemptDetail(att, selectedStudent.attempts.length - aIdx)"
                    class="px-3 py-1.5 bg-white/10 hover:bg-white/20 text-white rounded-lg text-xs font-semibold transition-colors flex items-center gap-1.5"
                  >
                    <Eye :size="13" />
                    <span>Xem chi tiết</span>
                  </button>
                </div>
              </div>
            </div>
          </div>

          <!-- VIEW 1: DANH SÁCH HỌC VIÊN TRONG LỚP -->
          <div v-else class="space-y-4">
            <!-- Search & Stats Filter -->
            <div class="flex items-center justify-between gap-4">
              <div class="relative flex-1 max-w-sm">
                <Search :size="14" class="absolute left-3 top-1/2 -translate-y-1/2 text-vdsa-muted" />
                <input
                  v-model="searchKeyword"
                  type="text"
                  placeholder="Tìm học viên theo tên hoặc email..."
                  class="w-full pl-9 pr-3 py-1.5 bg-white/5 border border-vdsa-border rounded-lg text-xs text-white placeholder:text-vdsa-muted focus:outline-none focus:border-vdsa-accent"
                />
              </div>

              <div class="flex items-center gap-2 text-xs text-vdsa-muted">
                <span>Đã nộp: <strong class="text-emerald-400">{{ submittedCount }}</strong> / {{ studentSummaries.length }}</span>
              </div>
            </div>

            <!-- Student List Table / Cards -->
            <div class="border border-vdsa-border rounded-xl overflow-hidden bg-vdsa-bg-secondary">
              <table class="w-full text-left text-xs">
                <thead class="bg-vdsa-surface border-b border-vdsa-border text-vdsa-muted uppercase font-semibold text-[11px]">
                  <tr>
                    <th class="px-4 py-3">Học viên</th>
                    <th class="px-4 py-3">Trạng thái</th>
                    <th class="px-4 py-3 text-center">Số lần nộp</th>
                    <th class="px-4 py-3 text-center">{{ isCodeLab ? 'Test pass cao nhất' : 'Điểm cao nhất' }}</th>
                    <th class="px-4 py-3 text-right">Lần nộp gần nhất</th>
                    <th class="px-4 py-3 text-center">Thao tác</th>
                  </tr>
                </thead>
                <tbody class="divide-y divide-vdsa-border">
                  <tr
                    v-for="student in filteredStudents"
                    :key="student.userId"
                    class="hover:bg-white/[0.02] transition-colors"
                  >
                    <!-- Student info -->
                    <td class="px-4 py-3">
                      <div class="flex items-center gap-2.5">
                        <div class="w-7 h-7 rounded-full bg-vdsa-accent/20 text-vdsa-accent flex items-center justify-center font-bold text-xs uppercase shrink-0">
                          {{ (student.displayName || 'U')[0] }}
                        </div>
                        <div class="min-w-0">
                          <p class="font-semibold text-white truncate">{{ student.displayName }}</p>
                          <p class="text-[11px] text-vdsa-muted truncate">{{ student.email }}</p>
                        </div>
                      </div>
                    </td>

                    <!-- Status -->
                    <td class="px-4 py-3">
                      <span
                        class="px-2 py-0.5 rounded text-[11px] font-semibold"
                        :class="student.attempts.length > 0 ? 'bg-emerald-500/20 text-emerald-400' : 'bg-white/5 text-vdsa-muted'"
                      >
                        {{ student.attempts.length > 0 ? 'Đã nộp bài' : 'Chưa nộp' }}
                      </span>
                    </td>

                    <!-- Attempts Count -->
                    <td class="px-4 py-3 text-center font-mono font-bold">
                      <span v-if="student.attempts.length > 0" class="text-white">
                        {{ student.attempts.length }} lần
                      </span>
                      <span v-else class="text-vdsa-muted">—</span>
                    </td>

                    <!-- Best Result -->
                    <td class="px-4 py-3 text-center font-mono font-bold">
                      <template v-if="student.attempts.length > 0">
                        <span v-if="isCodeLab" class="text-amber-400">
                          {{ student.bestTests }}
                        </span>
                        <span v-else class="text-emerald-400">
                          {{ student.bestScore }} điểm
                        </span>
                      </template>
                      <span v-else class="text-vdsa-muted">—</span>
                    </td>

                    <!-- Last submitted -->
                    <td class="px-4 py-3 text-right text-[11px] text-vdsa-muted">
                      {{ student.attempts.length > 0 ? formatDate(student.attempts[0].submittedAt) : '—' }}
                    </td>

                    <!-- Action button -->
                    <td class="px-4 py-3 text-center">
                      <button
                        v-if="student.attempts.length > 0"
                        @click="selectStudent(student)"
                        class="px-2.5 py-1 rounded bg-white/10 hover:bg-white/20 text-white font-semibold transition-colors inline-flex items-center gap-1 text-[11px]"
                      >
                        <span>Xem lịch sử</span>
                        <ChevronRight :size="12" />
                      </button>
                      <span v-else class="text-vdsa-muted text-[11px] italic">Chưa nộp</span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <!-- Footer -->
        <div class="px-6 py-3.5 border-t border-vdsa-border bg-vdsa-bg-secondary flex justify-between items-center text-xs text-vdsa-muted">
          <span>{{ currentView === 'detail' ? 'Bấm "Quay lại" để xem các lần làm khác của học viên.' : currentView === 'attempts' ? 'Bấm "Quay lại" để chọn học viên khác.' : 'Click "Xem lịch sử" để xem chi tiết code hoặc bài quiz của học viên.' }}</span>
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
import {
  X,
  ArrowLeft,
  Search,
  Code,
  HelpCircle,
  Check,
  Copy,
  Eye,
  ChevronRight,
  Loader2,
  FileQuestion,
} from 'lucide-vue-next';

import { fetchClassMembers } from '@/api/classes';
import {
  fetchExercise,
  fetchExerciseSubmissions,
  fetchExerciseCodeSubmissions,
  type SubmissionSummaryDto,
  type CodeSubmissionSummaryDto,
  type ExerciseDto,
} from '@/api/exercises';
import type { ClassReportAssignmentDto } from '@/api/types';

const props = withDefaults(defineProps<{
  isOpen: boolean;
  classId: number;
  assignment: ClassReportAssignmentDto | null;
}>(), {
  isOpen: false,
  assignment: null,
});

defineEmits<{
  (e: 'close'): void;
}>();

const loading = ref(false);
const searchKeyword = ref('');
const currentView = ref<'students' | 'attempts' | 'detail'>('students');

interface StudentSummary {
  userId: number;
  displayName: string;
  email: string;
  attempts: any[];
  bestScore: number;
  bestTests: string;
}

const studentSummaries = ref<StudentSummary[]>([]);
const selectedStudent = ref<StudentSummary | null>(null);
const selectedAttempt = ref<any | null>(null);
const selectedAttemptNumber = ref<number>(1);
const quizQuestions = ref<any[]>([]);
const isCodeCopied = ref(false);

const isCodeLab = computed(() => {
  if (!props.assignment) return false;
  return props.assignment.itemType === 'code';
});

const headerTitle = computed(() => {
  if (currentView.value === 'detail') {
    return `${selectedStudent.value?.displayName} - Lần ${selectedAttemptNumber.value}`;
  }
  if (currentView.value === 'attempts') {
    return `Lịch sử nộp: ${selectedStudent.value?.displayName}`;
  }
  return props.assignment?.title || 'Chi tiết bài nộp';
});

const headerSubtitle = computed(() => {
  if (currentView.value === 'detail') {
    return isCodeLab.value ? 'Chi tiết mã nguồn đã nộp' : 'Chi tiết kết quả bài trắc nghiệm';
  }
  if (currentView.value === 'attempts') {
    return `Tổng cộng ${selectedStudent.value?.attempts.length || 0} lần nộp bài`;
  }
  return `Bài gán ID #${props.assignment?.assignmentId} · Lớp ID #${props.classId}`;
});

const submittedCount = computed(() => {
  return studentSummaries.value.filter(s => s.attempts.length > 0).length;
});

const filteredStudents = computed(() => {
  const kw = searchKeyword.value.trim().toLowerCase();
  if (!kw) return studentSummaries.value;
  return studentSummaries.value.filter(s =>
    (s.displayName || '').toLowerCase().includes(kw) ||
    (s.email || '').toLowerCase().includes(kw)
  );
});

function backToPreviousView(): void {
  if (currentView.value === 'detail') {
    currentView.value = 'attempts';
    selectedAttempt.value = null;
  } else if (currentView.value === 'attempts') {
    currentView.value = 'students';
    selectedStudent.value = null;
  }
}

function formatDate(isoDate?: string | null): string {
  if (!isoDate) return '—';
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

function isQuizAttemptPassed(att: any): boolean {
  const total = quizQuestions.value.length || att.maxScore || 10;
  return total > 0 && att.score / total >= 0.7;
}

watch(() => props.isOpen, async (open) => {
  if (open) {
    currentView.value = 'students';
    selectedStudent.value = null;
    selectedAttempt.value = null;
    searchKeyword.value = '';
    await loadData();
  }
});

async function loadData(): Promise<void> {
  loading.value = true;
  studentSummaries.value = [];
  quizQuestions.value = [];

  try {
    // 1. Tải danh sách học viên trong lớp
    const members = await fetchClassMembers(props.classId);
    const studentsOnly = members.filter(m => ((m as any).role || '').toUpperCase() !== 'TEACHER');

    const exId = props.assignment?.exerciseId;
    let submissions: any[] = [];

    if (exId && exId > 0) {
      if (isCodeLab.value) {
        submissions = await fetchExerciseCodeSubmissions(exId, { pageSize: 200 });
      } else {
        submissions = await fetchExerciseSubmissions(exId, { pageSize: 200 });
        try {
          const exDetail = await fetchExercise(exId);
          quizQuestions.value = (exDetail.questions || []).map((q: any) => {
            let options = q.options;
            if (typeof options === 'string') {
              try { options = JSON.parse(options); } catch { options = []; }
            }
            return {
              id: q.id,
              content: q.content,
              options: options || [],
              answerJson: q.answerJson,
              explanation: q.explanation,
            };
          });
        } catch (e) {
          console.warn('Could not fetch quiz questions detail', e);
        }
      }
    }

    // Map bài nộp cho từng học viên
    studentSummaries.value = studentsOnly.map(member => {
      const userSubs = submissions
        .filter(s => s.userId === member.userId)
        .sort((a, b) => new Date(b.submittedAt).getTime() - new Date(a.submittedAt).getTime());

      let bestScore = 0;
      let bestTests = '0/0';

      if (userSubs.length > 0) {
        bestScore = Math.max(...userSubs.map(s => s.score || 0));
        if (isCodeLab.value) {
          const maxPassed = Math.max(...userSubs.map(s => s.passedTests || 0));
          const total = userSubs[0].totalTests || 0;
          bestTests = `${maxPassed}/${total}`;
        }
      }

      return {
        userId: member.userId,
        displayName: member.displayName || (member as any).name || (member as any).fullName || 'Học viên',
        email: member.email || '',
        attempts: userSubs,
        bestScore,
        bestTests,
      };
    });
  } catch (err) {
    console.error('Lỗi khi tải dữ liệu bài nộp lớp:', err);
  } finally {
    loading.value = false;
  }
}

function selectStudent(student: StudentSummary): void {
  selectedStudent.value = student;
  currentView.value = 'attempts';
}

function viewAttemptDetail(attempt: any, numberLabel: number): void {
  selectedAttempt.value = attempt;
  selectedAttemptNumber.value = numberLabel;
  currentView.value = 'detail';
}

async function copyCode(code: string): Promise<void> {
  try {
    await navigator.clipboard.writeText(code);
    isCodeCopied.value = true;
    setTimeout(() => {
      isCodeCopied.value = false;
    }, 2000);
  } catch {
    // fallback
  }
}

// ── Helpers cho Quiz Attempt ──

function getQuestionOptions(q: any): string[] {
  if (Array.isArray(q.options)) return q.options;
  if (typeof q.optionsJson === 'string') {
    try { return JSON.parse(q.optionsJson); } catch { return []; }
  }
  return [];
}

function parseAttemptAnswers(attempt: any): Record<string, number[]> {
  if (!attempt || !attempt.answersJson) return {};
  try {
    const raw = attempt.answersJson;
    const parsed = typeof raw === 'string' ? JSON.parse(raw) : raw;
    const mapping: Record<string, number[]> = {};

    if (Array.isArray(parsed)) {
      quizQuestions.value.forEach((q, idx) => {
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
  } catch {}
  return {};
}

function isOptionSelectedInAttempt(q: any, optionIdx: number, attempt: any): boolean {
  const ansMap = parseAttemptAnswers(attempt);
  const selected = ansMap[String(q.id)];
  return selected ? selected.includes(optionIdx) : false;
}

function isOptionCorrect(q: any, optionIdx: number): boolean {
  let correctIndices: number[] = [];
  if (q.answerJson) {
    try {
      const parsed = typeof q.answerJson === 'string' ? JSON.parse(q.answerJson) : q.answerJson;
      if (Array.isArray(parsed)) correctIndices = parsed;
      else if (typeof parsed === 'number') correctIndices = [parsed];
    } catch {}
  }
  return correctIndices.includes(optionIdx);
}

function isQuestionCorrectInAttempt(q: any, attempt: any): boolean {
  const ansMap = parseAttemptAnswers(attempt);
  const selected = ansMap[String(q.id)] ?? [];
  let correctIndices: number[] = [];
  if (q.answerJson) {
    try {
      const parsed = typeof q.answerJson === 'string' ? JSON.parse(q.answerJson) : q.answerJson;
      if (Array.isArray(parsed)) correctIndices = parsed;
      else if (typeof parsed === 'number') correctIndices = [parsed];
    } catch {}
  }
  if (selected.length === 0 || correctIndices.length === 0) return false;
  return selected.length === correctIndices.length && selected.every(idx => correctIndices.includes(idx));
}

function getOptionClass(q: any, optionIdx: number, attempt: any): string {
  const isSelected = isOptionSelectedInAttempt(q, optionIdx, attempt);
  const isCorrect = isOptionCorrect(q, optionIdx);

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
