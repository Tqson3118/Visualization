<template>
  <div class="lesson-study-view flex flex-col h-full w-full overflow-auto bg-bg-secondary font-sans">
    <!-- Stepper header -->
    <header class="px-4 md:px-6 py-3 border-b border-border-subtle bg-bg-secondary backdrop-blur-md flex items-center justify-between shrink-0 shadow-lg z-20 gap-3">
      <div class="flex items-center gap-2 md:gap-3 min-w-0">
        <router-link :to="courseId ? `/courses/${courseId}` : '/courses'" class="text-xs font-semibold text-text-muted hover:text-text-primary transition-colors flex items-center gap-1 shrink-0">
          <span>←</span> <span class="hidden sm:inline">Quay lại</span>
        </router-link>
        <span class="text-text-disabled hidden sm:inline">|</span>
        <h2 class="text-sm font-extrabold text-text-primary line-clamp-1" v-if="lesson">
          {{ lesson.title }}
        </h2>
      </div>

      <!-- Steps: mobile chỉ số, desktop đầy đủ -->
      <div class="flex items-center gap-1 md:gap-2 shrink-0">
        <button
          v-for="step in steps"
          :key="step.number"
          @click="scrollToStep(step.number)"
          class="flex items-center gap-1.5 px-2 md:px-3 py-1.5 rounded-xl text-xs font-bold transition-all cursor-pointer"
          :class="[
            activeStep === step.number
              ? 'bg-accent text-white shadow-md shadow-accent/30'
              : step.isComplete
                ? 'bg-accent-green/15 text-accent-green border border-accent-green/25'
                : step.isLocked
                  ? 'bg-bg-secondary text-text-disabled border border-border-subtle opacity-60 cursor-not-allowed'
                  : 'bg-bg-secondary text-text-muted hover:text-text-primary border border-border-subtle'
          ]"
        >
          <span
            class="w-4 h-4 rounded-full flex items-center justify-center text-[10px] shrink-0"
            :class="step.isComplete ? 'bg-accent-green text-white' : activeStep === step.number ? 'bg-bg-hover text-white' : 'bg-bg-surface text-text-muted'"
          >
            <svg v-if="step.isComplete" class="w-2.5 h-2.5" fill="none" stroke="currentColor" stroke-width="3" viewBox="0 0 24 24"><polyline points="20 6 9 17 4 12" /></svg>
            <BaseIcon v-else-if="step.isLocked" name="lock" class="w-2.5 h-2.5" />
            <template v-else>{{ step.number }}</template>
          </span>
          <span class="hidden lg:inline">{{ step.label }}</span>
        </button>
      </div>

      <div class="flex items-center gap-2 font-mono text-xs shrink-0">
        <span class="px-2.5 py-1 rounded-lg bg-accent-yellow/50 text-accent-yellow border border-accent-yellow/30 font-bold flex items-center gap-1.5">
          <svg class="w-3.5 h-3.5 text-accent-yellow" fill="currentColor" viewBox="0 0 24 24">
            <path d="M13 2L3 14h9l-1 8 10-12h-9l1-8z" />
          </svg>
          <span class="hidden sm:inline">+{{ lesson?.xpReward ?? 50 }} XP</span>
        </span>
        <button
          class="px-2 py-1 rounded-lg text-[10px] font-bold bg-accent-red/10 text-accent-red border border-accent-red/20 hover:bg-accent-red/20 transition-colors cursor-pointer"
          title="Báo cáo nội dung vi phạm"
          @click="openReportModal"
        >
          Báo cáo
        </button>
      </div>
    </header>

    <!-- Loading -->
    <div v-if="loading" class="flex-1 flex flex-col items-center justify-center gap-4 py-20">
      <div class="w-10 h-10 border-4 border-accent border-t-transparent rounded-full animate-spin"></div>
      <p class="text-text-muted text-sm">Đang tải bài học...</p>
    </div>

    <!-- Error -->
    <div v-else-if="error" class="flex-1 flex flex-col items-center justify-center gap-4 py-20 px-6 text-center">
      <div class="w-14 h-14 rounded-full bg-accent-red/10 border border-accent-red/20 flex items-center justify-center">
        <BaseIcon name="warning" class="w-6 h-6 text-accent-red" />
      </div>
      <h3 class="text-lg font-bold text-text-primary">Không thể tải bài học</h3>
      <p class="text-text-muted text-sm max-w-md">{{ error }}</p>
      <button class="btn-primary mt-2" @click="loadLesson">Thử lại</button>
    </div>

    <!-- 1 trang cuộn: Theory → Viz → Quiz → CodeLab → LeetCode -->
    <main v-else ref="mainScrollEl" class="flex-1 min-h-0 overflow-y-auto bg-bg-primary" @scroll.passive="handleScroll">
      <div class="max-w-5xl mx-auto px-4 md:px-6 py-6 space-y-6">
        <section v-for="step in steps" :key="step.number" :id="`step-${step.number}`" class="lesson-section scroll-mt-24">
          <div class="flex items-center gap-2 mb-4">
            <span class="w-6 h-6 rounded-full flex items-center justify-center text-[11px] font-bold shrink-0"
              :class="step.isComplete ? 'bg-accent-green text-white' : activeStep === step.number ? 'bg-accent text-white' : 'bg-bg-hover text-text-muted'">
              {{ step.number }}
            </span>
            <h3 class="text-sm font-bold uppercase tracking-wider text-text-secondary">{{ step.label }}</h3>
          </div>

          <LessonStepTheory
            v-if="step.kind === 'theory'"
            :title="lesson?.title || 'Lý Thuyết Thuật Toán'"
            :content="lesson?.contentMd || 'Đọc tài liệu lý thuyết nền tảng trước khi xem mô phỏng trực quan hóa.'"
            :step-number="step.number"
            :total-steps="steps.length"
            @completeStep="handleStepComplete(step.number)"
          />

          <LessonStepViz
            v-else-if="step.kind === 'viz'"
            :viz-title="lesson?.title"
            :viz-description="lesson?.contentMd"
            :module-key="vizModuleKey"
            :visualizer-config="visualizerConfig"
            @completeStep="handleStepComplete(step.number)"
          />

          <LessonStepCodeViz
            v-else-if="step.kind === 'codeviz'"
            :initial-code="codeVizTemplate.code"
            :initial-array="codeVizTemplate.array"
            :step-number="step.number"
            :total-steps="steps.length"
            @completeStep="handleStepComplete(step.number)"
          />

          <LessonStepQuiz
            v-else-if="step.kind === 'quiz'"
            :quiz-id="lesson?.quizId"
            :step-number="step.number"
            :total-steps="steps.length"
            @completeStep="handleStepComplete(step.number)"
          />

          <LessonStepCodeLab
            v-else-if="step.kind === 'codelab'"
            :problem-title="'Thực hành: ' + (lesson?.codelab?.title || lesson?.title)"
            :codelab="lesson?.codelab"
            :step-number="step.number"
            :total-steps="steps.length"
            @completeStep="handleStepComplete(step.number)"
          />

          <LessonStepLeetCode
            v-else-if="step.kind === 'leetcode'"
            :leet-code-id="lesson?.leetCodeId"
            @completeLesson="completeLesson"
          />
        </section>
      </div>
    </main>

    <!-- Footer: nút Hoàn thành -->
    <footer v-if="!loading && !error" class="border-t border-border-subtle bg-bg-secondary px-4 md:px-6 py-3 shrink-0 flex items-center justify-between gap-3">
      <p class="text-[11px] text-text-muted hidden sm:block">
        <template v-if="allStepsComplete">Đã hoàn thành tất cả các bước. Nhấn Hoàn thành để ghi nhận!</template>
        <template v-else>Hoàn thành các bước phía trên để mở khóa nút Hoàn thành.</template>
      </p>
      <button
        class="btn-primary px-6 py-2.5 rounded-xl text-sm font-bold flex items-center gap-2 ml-auto"
        :disabled="!allStepsComplete"
        @click="completeLesson"
      >
        <BaseIcon name="check-circle" class="w-4 h-4" />
        Hoàn thành bài học
      </button>
    </footer>

    <LessonCompletionModal
      :show="showCompletionModal"
      :xpReward="lesson?.xpReward ?? 50"
      @close="goToNextLesson"
    />

    <!-- E — Báo cáo nội dung vi phạm -->
    <div v-if="showReportModal" class="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50" @click.self="closeReportModal">
      <div class="glass-panel rounded-2xl p-6 max-w-md w-full border border-border-default">
        <div class="flex items-center justify-between mb-4">
          <h3 class="text-lg font-bold text-text-primary flex items-center gap-2">
            <BaseIcon name="warning" class="w-5 h-5 text-accent-red" />
            Báo cáo nội dung
          </h3>
          <button class="w-8 h-8 flex items-center justify-center rounded-full bg-bg-hover text-text-secondary hover:text-text-primary transition-colors cursor-pointer" @click="closeReportModal" aria-label="Đóng">
            <BaseIcon name="close" class="w-4 h-4" />
          </button>
        </div>
        <p class="text-xs text-text-secondary mb-4">Nội dung bị báo cáo sẽ được Admin rà soát và có thể bị ẩn nếu vi phạm.</p>
        <div class="space-y-4">
          <div>
            <label class="block text-xs font-bold text-text-muted uppercase tracking-wider mb-1.5">Lý do</label>
            <select v-model="reportReason" class="w-full bg-bg-hover border border-border-default rounded-lg px-3 py-2 text-sm text-text-primary focus:outline-none focus:border-border-accent">
              <option value="offensive">Nội dung xúc phạm / độc hại</option>
              <option value="wrong_info">Thông tin sai lệch</option>
              <option value="spam">Spam / quảng cáo</option>
              <option value="other">Khác</option>
            </select>
          </div>
          <div>
            <label class="block text-xs font-bold text-text-muted uppercase tracking-wider mb-1.5">Chi tiết (tùy chọn)</label>
            <textarea v-model="reportDetail" rows="3" maxlength="1000" placeholder="Mô tả vấn đề bạn gặp phải..."
              class="w-full bg-bg-hover border border-border-default rounded-lg px-3 py-2 text-sm text-text-primary placeholder:text-text-muted focus:outline-none focus:border-border-accent resize-none"></textarea>
          </div>
          <div class="flex gap-3 pt-1">
            <button class="flex-1 py-2.5 rounded-xl bg-accent-red text-white font-bold text-sm hover:bg-accent-red/90 transition-all cursor-pointer" :disabled="reporting" @click="submitReport">
              {{ reporting ? 'Đang gửi...' : 'Gửi báo cáo' }}
            </button>
            <button class="px-4 py-2.5 rounded-xl bg-bg-hover text-text-secondary font-bold text-sm hover:text-text-primary transition-all cursor-pointer" @click="closeReportModal">Hủy</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, nextTick } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import LessonStepTheory from './components/LessonStepTheory.vue';
import LessonStepViz from './components/LessonStepViz.vue';
import LessonStepCodeViz from './components/LessonStepCodeViz.vue';
import LessonStepQuiz from './components/LessonStepQuiz.vue';
import LessonStepCodeLab from './components/LessonStepCodeLab.vue';
import LessonStepLeetCode from './components/LessonStepLeetCode.vue';
import LessonCompletionModal from './LessonCompletionModal.vue';
import BaseIcon from '@/shared/components/BaseIcon.vue';
import { api } from '@/services/apiClient';
import { useToastStore } from '@/composables/useToast';
import { teacherStudioService, type CustomNodeDto } from '@/services/TeacherStudioService';
import { codelabApi } from '@/features/codelabs/api/codelabApi';
import { useLiveCompilerStore } from '@/features/code-to-visualization/store/useLiveCompilerStore';
import { useAnimationStore } from '@/features/animation-engine/store/useAnimationStore';
import { DEFAULT_SOURCE_CODE, DEFAULT_INPUT_ARRAY } from '@/features/code-to-visualization/store/liveCompilerDefaults';
import { normalizeArrayAlgorithmKey, getCodeVizTemplate } from '@/features/code-to-visualization/store/codeVizTemplates';

const toastStore = useToastStore();

interface LessonStep {
  number: number;
  label: string;
  kind: 'theory' | 'viz' | 'codeviz' | 'quiz' | 'codelab' | 'leetcode';
  isComplete: boolean;
  isLocked: boolean;
}

const route = useRoute();
const router = useRouter();

const lessonId = route.params.id as string;
const courseId = computed(() => (route.query.courseId as string) || (lesson.value?.courseId as string) || '');

const loading = ref(true);
const error = ref('');
const activeStep = ref(1);
const showCompletionModal = ref(false);
const mainScrollEl = ref<HTMLElement | null>(null);

// E — Báo cáo nội dung
const showReportModal = ref(false);
const reportReason = ref('offensive');
const reportDetail = ref('');
const reporting = ref(false);

function openReportModal(): void {
  reportReason.value = 'offensive';
  reportDetail.value = '';
  showReportModal.value = true;
}

function closeReportModal(): void {
  showReportModal.value = false;
}

async function submitReport(): Promise<void> {
  if (reporting.value) return;
  reporting.value = true;
  try {
    await api.post(`/concepts/nodes/${lessonId}/report`, {
      reason: reportReason.value,
      detail: reportDetail.value.trim() || undefined,
    });
    toastStore.success('Cảm ơn bạn! Báo cáo đã được gửi đến Admin.');
    closeReportModal();
  } catch (err) {
    toastStore.error('Không thể gửi báo cáo. Vui lòng thử lại.');
  } finally {
    reporting.value = false;
  }
}

const lesson = ref<any>(null);
const steps = ref<LessonStep[]>([]);

const allStepsComplete = computed(() =>
  steps.value.length > 0 && steps.value.every(s => s.isComplete)
);

interface VizConfigPayload {
  algorithm: string;
  sampleInput?: string;
  speed?: number;
}

const visualizerConfig = computed<VizConfigPayload | null>(() => {
  if (!lesson.value) return null;
  const sandboxConfig = lesson.value.sandboxConfig;
  let configObj: Record<string, unknown> = {};
  if (typeof sandboxConfig === 'string' && sandboxConfig) {
    try {
      configObj = JSON.parse(sandboxConfig) as Record<string, unknown>;
    } catch {
      configObj = {};
    }
  } else if (sandboxConfig && typeof sandboxConfig === 'object') {
    configObj = sandboxConfig as Record<string, unknown>;
  }

  // Ưu tiên config đã gắn sẵn từ seed/teacher (G4.1.6) — lesson.visualizerConfig (roadmap node)
  const explicitViz = lesson.value.visualizerConfig;
  if (explicitViz && typeof explicitViz === 'string') {
    try {
      const parsed = JSON.parse(explicitViz) as VizConfigPayload;
      if (parsed.algorithm) return parsed;
    } catch {
      // fallthrough
    }
  }
  if (explicitViz && typeof explicitViz === 'object') {
    const parsed = explicitViz as VizConfigPayload;
    if (parsed.algorithm) return parsed;
  }

  // Ưu tiên config gắn trong sandboxConfig.visualizerConfig
  if (configObj.visualizerConfig && typeof configObj.visualizerConfig === 'string') {
    try {
      const parsed = JSON.parse(configObj.visualizerConfig) as VizConfigPayload;
      if (parsed.algorithm) return parsed;
    } catch {
      // fallthrough
    }
  }
  if (configObj.visualizerConfig && typeof configObj.visualizerConfig === 'object') {
    const parsed = configObj.visualizerConfig as VizConfigPayload;
    if (parsed.algorithm) return parsed;
  }

  const sandboxType = (lesson.value.sandboxType || '').toLowerCase();

  // Có thuật toán khai báo trực tiếp trong sandboxConfig
  if (typeof configObj.algorithm === 'string' && configObj.algorithm.trim()) {
    return {
      algorithm: configObj.algorithm.trim(),
      sampleInput: Array.isArray(configObj.array) ? (configObj.array as number[]).join(',') : undefined,
      speed: 1,
    };
  }

  // Map sandboxType → thuật toán (G2.3.11/G2.4.6): KHÔNG mặc định sai về bubble-sort
  const mapped = SANDBOX_ALGO_MAP[sandboxType];
  if (mapped) {
    const arr = Array.isArray(configObj.array) ? (configObj.array as number[]) : undefined;
    return {
      algorithm: mapped,
      sampleInput: arr && arr.length > 0 ? arr.join(',') : undefined,
      speed: 1,
    };
  }

  // Không có cấu hình → empty state (không bịa thuật toán)
  return null;
});

// Lesson chưa có visualizerConfig → hiện empty state thay vì mặc định sai
const vizModuleKey = computed(() => (visualizerConfig.value ? lesson.value?.sandboxType || 'sorting' : ''));

// ── Code-to-Viz (B1/B2): chỉ bật cho bài thuật toán MẢNG ──
const codeVizAlgorithmKey = computed<string | null>(() => {
  const vizAlgo = visualizerConfig.value?.algorithm;
  if (vizAlgo) {
    const key = normalizeArrayAlgorithmKey(vizAlgo);
    if (key) return key;
  }
  const sandboxType = String(lesson.value?.sandboxType || '').toLowerCase();
  const mapped = SANDBOX_ALGO_MAP[sandboxType];
  if (mapped) {
    const key = normalizeArrayAlgorithmKey(mapped);
    if (key) return key;
  }
  return normalizeArrayAlgorithmKey(sandboxType);
});

const isArrayAlgorithmLesson = computed(() => codeVizAlgorithmKey.value !== null);

const codeVizTemplate = computed(() =>
  getCodeVizTemplate(codeVizAlgorithmKey.value, visualizerConfig.value?.sampleInput ?? null),
);

const SANDBOX_ALGO_MAP: Record<string, string> = {
  'bubble-sort': 'bubble-sort',
  'quick-sort': 'quick-sort',
  'merge-sort': 'merge-sort',
  'heap-sort': 'heap-sort',
  'radix-sort': 'radix-sort',
  'counting-sort': 'counting-sort',
  'bucket-sort': 'bucket-sort',
  'linear-search': 'linear-search',
  'binary-search': 'binary-search',
  'sliding-window': 'sliding-window',
  stack: 'stack',
  queue: 'queue',
  'monotonic-stack': 'monotonic-stack',
  bst: 'bst',
  bfs: 'bfs',
  dfs: 'dfs',
  dijkstra: 'dijkstra',
  'bellman-ford': 'bellman-ford',
  kruskal: 'kruskal',
  prim: 'prim',
  tarjan: 'tarjan',
  'a-star': 'a-star',
  tree: 'bst',
  oop: 'encapsulation',
  solid: 'solid-srp',
  patterns: 'strategy',
  di: 'dip',
};

const currentStepLabel = computed(() => steps.value.find(s => s.number === activeStep.value)?.label || '');

function handleStepComplete(stepNumber: number): void {
  const step = steps.value.find(s => s.number === stepNumber);
  if (step) {
    step.isComplete = true;
    step.isLocked = false;
  }
  const next = steps.value.find(s => s.number === stepNumber + 1);
  if (next) {
    next.isLocked = false;
    activeStep.value = next.number;
    scrollToStep(next.number);
  }
}

function scrollToStep(stepNumber: number): void {
  const step = steps.value.find(s => s.number === stepNumber);
  if (!step) return;
  if (step.isLocked && !step.isComplete) return;
  activeStep.value = stepNumber;
  const el = document.getElementById(`step-${stepNumber}`);
  if (el) el.scrollIntoView({ behavior: 'smooth', block: 'start' });
}

async function completeLesson() {
  try {
    await api.post(`/concepts/lessons/${lessonId}/complete`);
    showCompletionModal.value = true;
  } catch (e: any) {
    const status = e?.status as number | undefined;
    if (status === 403) {
      toastStore.error('Chưa hoàn thành đủ các bước để kết thúc bài học. Vui lòng hoàn thành Theory → Quiz → Codelab trước.');
    } else {
      toastStore.error(e?.detail || e?.title || 'Không thể hoàn thành bài học. Vui lòng thử lại.');
    }
  }
}

async function loadLesson() {
  loading.value = true;
  error.value = '';
  try {
    let response: Record<string, unknown> & {
      id?: string;
      title?: string;
      contentMd?: string;
      sandboxType?: string;
      sandboxConfig?: unknown;
      visualizerConfig?: unknown;
      quizId?: string | null;
      codelab?: unknown;
      leetCodeId?: string | null;
      xpReward?: number;
      status?: string;
      lastScrollPercent?: number;
      courseId?: string;
    };
    try {
      response = await api.get(`/concepts/lessons/${lessonId}`) as typeof response;
    } catch {
      // Fallback: roadmap node (CustomNode) — không có Lesson record trong hệ legacy
      response = await loadRoadmapNodeLesson(lessonId);
    }
    lesson.value = response;

    let stepNum = 1;
    const dynamicSteps: LessonStep[] = [];
    const add = (label: string, kind: LessonStep['kind']) => {
      dynamicSteps.push({
        number: stepNum++,
        label,
        kind,
        isComplete: stepNum === 2,
        isLocked: stepNum !== 2,
      });
    };
    add('Lý Thuyết', 'theory');
    add('Trực Quan Hóa', 'viz');

    // Bước "Code-to-Viz" — chỉ hiện cho thuật toán MẢNG (Sorting/Searching/Sliding Window)
    if (isArrayAlgorithmLesson.value) {
      add('Code-to-Viz', 'codeviz');
    }

    if (lesson.value.quizId) add('Quiz', 'quiz');
    if (lesson.value.codelab) add('Code Lab', 'codelab');
    if (lesson.value.leetCodeId) add('LeetCode', 'leetcode');

    // Step 1 là bước đầu tiên: không khóa
    dynamicSteps[0] = { ...dynamicSteps[0], isLocked: false, isComplete: false };
    steps.value = dynamicSteps;

    // G3.1.2 — Resume: nếu lesson đã mở từ trước (status != NotStarted), scroll tới vị trí cũ
    if (response.status && response.status !== 'NotStarted') {
      await nextTick();
      const scrollPercent = response.lastScrollPercent ?? 0;
      if (mainScrollEl.value && scrollPercent > 0) {
        mainScrollEl.value.scrollTop = mainScrollEl.value.scrollHeight * (scrollPercent / 100);
      }
    }
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Không thể tải bài học. Vui lòng thử lại.';
  } finally {
    loading.value = false;
  }
}

// ── Roadmap node fallback (P0.1 — luồng học duy nhất đi qua LessonStudy) ──
function inferSandboxType(node: CustomNodeDto): string {
  let content: Record<string, unknown> | null = null;
  try {
    if (node.contentJson && node.contentJson.trim()) {
      content = JSON.parse(node.contentJson) as Record<string, unknown>;
    }
  } catch {
    content = null;
  }

  const rawViz = node.visualizerConfig;
  let algo = '';
  if (typeof rawViz === 'string') {
    try {
      algo = String((JSON.parse(rawViz) as { algorithm?: string }).algorithm || '');
    } catch {
      algo = '';
    }
  }
  const haystack = `${algo} ${node.name} ${node.description}`.toLowerCase();
  if (/(oop|encapsul|inheritan|polymorph|abstraction)/.test(haystack)) return 'oop';
  if (/(solid|srp|ocp|lsp|isp|dip)/.test(haystack)) return 'solid';
  if (/(strategy|pattern|singleton|factory|observer)/.test(haystack)) return 'patterns';
  if (/(dependency inversion|ioc|inversion of control|\bdi\b)/.test(haystack)) return 'di';
  if (/(bellman|kruskal|prim|tarjan|a-star|\bdijkstra\b|\bbfs\b|\bdfs\b|graph|đồ thị)/.test(haystack)) return 'graph';
  if (/(bst|binary search tree|tree|segment tree|cây)/.test(haystack)) return 'bst';
  if (/(stack|queue|monotonic)/.test(haystack)) return 'stack';
  if (/(binary search|tìm kiếm nhị phân)/.test(haystack)) return 'binary-search';
  if (content && Array.isArray(content.graph)) return 'graph';
  if (algo) return 'sorting';
  return 'sorting';
}

async function loadRoadmapNodeLesson(nodeId: string): Promise<typeof lesson.value> {
  const roadmaps = await teacherStudioService.getPublishedRoadmaps();
  for (const roadmap of roadmaps) {
    const node = roadmap.nodes.find(n => n.id === nodeId);
    if (node) {
      let codelab: unknown = null;
      if (node.labId) {
        try {
          codelab = await codelabApi.getCodelab(node.labId);
        } catch {
          codelab = null;
        }
      }
      let sandboxConfig = node.contentJson || '[]';
      if (node.visualizerConfig) {
        sandboxConfig = JSON.stringify({ visualizerConfig: node.visualizerConfig });
      }
      return {
        id: node.id,
        courseId: node.roadmapId,
        courseTitle: roadmap.name,
        title: node.name,
        contentMd: node.description || 'Nội dung bài học đang được chuẩn bị.',
        sandboxType: inferSandboxType(node),
        sandboxConfig,
        visualizerConfig: node.visualizerConfig ?? null,
        quizId: node.quizId ?? null,
        codelab,
        leetCodeId: node.leetCodeId ?? null,
        xpReward: node.difficulty === 'Easy' ? 30 : node.difficulty === 'Hard' ? 80 : 50,
        // CustomNode.IsComplete chỉ là flag "node đủ Quiz+Lab+LeetCode" (teacher configure), KHÔNG phải tiến độ user.
        // Luồng roadmap chưa có UserLessonProgress riêng → không đánh dấu 'Completed' nhầm ngay khi mới mở bài.
        status: 'NotStarted',
        lastScrollPercent: 0,
      };
    }
  }
  throw new Error('Không tìm thấy bài học trong lộ trình.');
}

onMounted(() => {
  loadLesson();
});

let progressTimer: ReturnType<typeof setTimeout> | null = null;

function handleScroll(): void {
  if (progressTimer) return;
  progressTimer = setTimeout(async () => {
    progressTimer = null;
    const mainEl = mainScrollEl.value;
    if (!mainEl) return;
    const scrollPercent = Math.min(100, Math.round((mainEl.scrollTop / Math.max(1, mainEl.scrollHeight - mainEl.clientHeight)) * 100));
    try {
      await api.post(`/concepts/lessons/${lessonId}/progress`, {
        lastActiveFrameIndex: 0,
        lastScrollPercent: scrollPercent,
      });
    } catch (e) {
      console.warn('Failed to save lesson progress:', e);
    }
  }, 800);
}

onUnmounted(() => {
  if (progressTimer) clearTimeout(progressTimer);
  // Reset sandbox Code-to-Viz khi rời bài — tránh code/input bài trước lưu sang bài sau.
  const compilerStore = useLiveCompilerStore();
  compilerStore.cancelExecution();
  compilerStore.setSourceCode(DEFAULT_SOURCE_CODE);
  compilerStore.setInputArray([...DEFAULT_INPUT_ARRAY]);
  compilerStore.clearLogs();
  useAnimationStore().clear();
});

function goToNextLesson(): void {
  showCompletionModal.value = false;
  if (courseId) {
    router.push('/courses/' + courseId);
  } else {
    router.push('/courses');
  }
}
</script>

<style scoped>
.fade-slide-enter-active,
.fade-slide-leave-active {
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

.lesson-section {
  scroll-margin-top: 5rem;
}
</style>
