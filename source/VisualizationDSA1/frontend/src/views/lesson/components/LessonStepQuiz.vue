<template>
  <div class="lesson-step-quiz flex flex-col h-full overflow-y-auto p-6 text-text-primary font-sans max-w-3xl mx-auto w-full">
    
    <div class="border-b border-border-subtle pb-4 mb-6 text-center">
      <div class="flex items-center justify-center gap-1.5 text-xs font-semibold text-accent uppercase tracking-wider">
        <svg class="w-4 h-4 text-accent" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 01-2 2h-0a2 2 0 01-2-2v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
        </svg>
        <span>Bước {{ stepNumber }} / {{ totalSteps }}</span>
      </div>
      <h2 class="text-xl font-extrabold text-white mt-1">{{ quizTitle }}</h2>
      <p class="text-xs text-text-muted mt-1">Hoàn thành bài Quiz củng cố kiến thức để mở khóa phần Code Lab.</p>
    </div>

    
    <div v-if="loading" class="flex flex-col items-center justify-center flex-1">
      <div class="spinner"></div>
      <span>Đang tải bài Quiz...</span>
    </div>

    <div v-else-if="quizError" class="flex flex-col items-center justify-center flex-1 text-center py-12">
      <div class="w-16 h-16 rounded-2xl bg-accent-red/20 border border-accent-red/30 flex items-center justify-center text-accent-red mb-4">
        <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.46 0L3.34 16.5c-.77-1.333.192 3 1.732 3z" />
          <path stroke-linecap="round" stroke-linejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 18v2m0 0v2m0-2h2v2H8v-2M9 12l2 2 4-4m5.618-4.008A5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 01-2 2h-0a2 2 0 01-2-2v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
        </svg>
      </div>
      <h3 class="text-base font-bold text-white mt-3">Không thể tải bài Quiz</h3>
      <p class="text-xs text-text-muted mt-1 max-w-md">{{ quizError }}</p>
      <button class="mt-6 px-6 py-3 bg-accent hover:bg-accent text-white rounded-xl text-xs font-bold transition-all shadow-lg cursor-pointer" @click="loadQuiz">
        Thử lại
      </button>
    </div>

    <div v-else-if="questions.length > 0" class="flex flex-col gap-6 flex-1">
      <div
        v-for="(q, qIdx) in questions"
        :key="q.id"
        class="bg-bg-secondary border border-border-subtle rounded-2xl p-5 shadow-lg"
      >
        <div class="flex items-start gap-3 mb-4">
          <span class="w-6 h-6 rounded-full bg-accent/30 border border-accent/50 text-accent font-bold text-xs flex items-center justify-center shrink-0">
            {{ qIdx + 1 }}
          </span>
          <p class="text-sm font-bold text-text-primary leading-relaxed">{{ q.text }}</p>
        </div>

        <div class="grid grid-cols-1 gap-2.5 ml-9">
          <button
            v-for="(opt, oIdx) in q.options"
            :key="oIdx"
            @click="userAnswers[q.id] = oIdx"
            class="px-4 py-3 rounded-xl border text-left text-xs font-semibold transition-all cursor-pointer flex items-center justify-between"
            :class="userAnswers[q.id] === oIdx
              ? 'bg-accent text-white border-accent shadow-md'
              : 'bg-bg-secondary text-text-secondary border-border-subtle hover:border-border-default hover:bg-bg-surface'"
          >
            <span>{{ opt }}</span>
            <span v-if="userAnswers[q.id] === oIdx" class="text-sm"><BaseIcon name="check" class="w-3.5 h-3.5 text-accent" /></span>
          </button>
        </div>
      </div>

      
      <div class="mt-4 p-5 rounded-2xl bg-bg-secondary border border-border-subtle flex items-center justify-between">
        <div>
          <span class="text-xs font-semibold text-text-muted">Đã chọn {{ answeredCount }} / {{ questions.length }} câu hỏi</span>
        </div>
        <button
          @click="submitQuiz"
          :disabled="answeredCount < questions.length || submitting"
          class="px-6 py-3 bg-accent hover:bg-accent disabled:opacity-40 disabled:cursor-not-allowed text-white rounded-xl text-xs font-bold transition-all shadow-lg cursor-pointer"
        >
          {{ isSubmitted ? 'Đã Đạt! Mở Khóa Code Lab' : 'Nộp Bài Quiz' }} <BaseIcon v-if="isSubmitted" name="arrow-right" class="w-3.5 h-3.5 inline-block ml-0.5 align-text-bottom" />
        </button>
      </div>
    </div>

    <div v-else class="flex flex-col items-center justify-center flex-1 text-center py-12">
      <div class="w-16 h-16 rounded-2xl bg-accent/20 border border-accent/30 flex items-center justify-center text-accent mb-4">
        <svg class="w-8 h-8" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 01-2 2h-0a2 2 0 01-2-2v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" />
        </svg>
      </div>
      <h3 class="text-base font-bold text-white">Kiểm Tra Trắc Nghiệm</h3>
      <p class="text-xs text-text-muted mt-1 max-w-md">Bấm bên dưới để xác nhận bạn đã nắm vững kiến thức và tiến tới phần Code Lab.</p>
      <button
        @click="$emit('completeStep')"
        class="mt-6 px-6 py-3 bg-accent hover:bg-accent text-white rounded-xl text-xs font-bold transition-all shadow-lg cursor-pointer"
      >
        Mở Khóa Code Lab <BaseIcon name="arrow-right" class="w-3.5 h-3.5 inline-block ml-0.5 align-text-bottom" />
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useToastStore } from '@/composables/useToast';
import { ref, computed, onMounted, watch } from 'vue';
import { statelessQuizApi, type StatelessQuizDetail, type StatelessQuestion } from '@/features/quiz-system/service/statelessQuizApi';
import { useAuthStore } from '@/features/auth/store/useAuthStore';
import BaseIcon from '@/shared/components/BaseIcon.vue';

const props = withDefaults(defineProps<{
  quizId?: string;
  stepNumber?: number;
  totalSteps?: number;
}>(), {
  quizId: undefined,
  stepNumber: 3,
  totalSteps: 4,
});

const emit = defineEmits<{
  (e: 'completeStep'): void;
}>();

const authStore = useAuthStore();
const toastStore = useToastStore();

const questions = ref<StatelessQuestion[]>([]);
const quizTitle = ref('Kiểm Tra Nhận Thức Nhanh');
const loading = ref(false);
const quizError = ref<string | null>(null);

async function loadQuiz() {
  if (!props.quizId) {
    if (import.meta.env.DEV) {
      // DEV-only fallback: không bao giờ chạy ở production (quizId phải có từ lesson)
      questions.value = [
        {
          id: 'q1',
          text: 'Độ phức tạp thời gian trung bình của thuật toán Bubble Sort là bao nhiêu?',
          options: ['O(1)', 'O(N log N)', 'O(N²)', 'O(N³)'],
          correctIndex: 2,
          explanation: 'Bubble Sort có độ phức tạp trung bình O(N²) do hai vòng lặp lồng nhau.'
        },
        {
          id: 'q2',
          text: 'Bubble Sort có phải là một thuật toán sắp xếp ổn định (Stable Sort) không?',
          options: ['Có, giữ nguyên thứ tự tương đối của các phần tử bằng nhau', 'Không, thay đổi thứ tự phần tử bằng nhau'],
          correctIndex: 0,
          explanation: 'Bubble Sort là thuật toán ổn định vì nó chỉ hoán đổi khi phần tử trước lớn hơn phần tử sau.'
        }
      ];
      quizTitle.value = 'Kiểm Tra Nhận Thức Nhanh';
    } else {
      quizError.value = 'Bài học này chưa có bài kiểm tra (quizId bị thiếu).';
      questions.value = [];
    }
    return;
  }

  loading.value = true;
  quizError.value = null;
  try {
    const quiz = await statelessQuizApi.getQuizById(props.quizId!);
    questions.value = quiz.questions.map((q, idx) => ({
      id: q.id,
      text: q.text,
      options: q.options,
      correctIndex: q.correctIndex,
      explanation: q.explanation
    }));
    quizTitle.value = quiz.title;
  } catch (err: any) {
    quizError.value = err.message || 'Không thể tải bài Quiz';
  } finally {
    loading.value = false;
  }
}

const userAnswers = ref<Record<string, number>>({});
const isSubmitted = ref(false);
const submitting = ref(false);

const answeredCount = computed(() => Object.keys(userAnswers.value).length);

async function submitQuiz() {
  if (answeredCount.value < questions.value.length || submitting.value) return;
  
  submitting.value = true;
  try {
    const answers = questions.value.map(q => userAnswers.value[q.id] ?? -1);
    const token = useAuthStore().getAccessToken();
    
    // We need the quiz ID - get it from props.quizId
    const quizId = props.quizId!;
    const answersArray = answers.map(a => a === -1 ? 0 : a);
    const result = await statelessQuizApi.submitAttempt(
      quizId,
      answersArray,
      useAuthStore().getAccessToken()
    );
    
    // Check if passed (70% threshold)
    if (result.passed) {
      isSubmitted.value = true;
      // Emit completeStep after a short delay to show success
      setTimeout(() => {
        emit('completeStep');
      }, 1500);
    } else {
      toastStore.info(`Bạn chưa đạt điểm đủ. Điểm: ${result.score}/${result.maxScore}. Cần ${Math.ceil(result.maxScore * 0.7)} để pass.`);
    }
  } catch (err: any) {
    toastStore.error(err.message || 'Nộp bài thất bại');
  } finally {
    submitting.value = false;
  }
}

onMounted(() => {
  loadQuiz();
});

watch(() => props.quizId, () => {
  userAnswers.value = {};
  isSubmitted.value = false;
  loadQuiz();
});
</script>