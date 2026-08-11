<template>
  <div class="lesson-visualizer flex flex-col w-full h-full gap-2 min-h-0 relative">
    <!-- Header: locked algorithm picker -->
    <div class="flex items-center justify-between gap-2 px-3 py-2 bg-bg-surface border border-border-default rounded-xl shrink-0">
      <div class="flex items-center gap-2 min-w-0">
        <span class="px-2 py-0.5 rounded text-[9px] font-black uppercase tracking-widest bg-accent/20 text-accent border border-border-accent shrink-0">
          Viz
        </span>
        <div class="flex items-center gap-1.5 min-w-0">
          <BaseIcon name="lock" class="w-3 h-3 text-text-muted shrink-0" />
          <span class="text-xs font-bold text-text-primary truncate">{{ algorithmLabel }}</span>
        </div>
        <span
          v-if="isDemo"
          class="px-1.5 py-0.5 rounded text-[9px] font-black uppercase tracking-widest bg-accent-warm/20 text-accent-warm border border-accent-warm/30 shrink-0"
          title="Mô phỏng cục bộ (demo) — backend không phản hồi"
        >
          Demo
        </span>
      </div>

      <div class="flex items-center gap-1.5 shrink-0">
        <button
          class="px-2.5 py-1 rounded-lg text-[10px] font-bold bg-accent/15 text-accent border border-accent/30 hover:bg-accent/30 transition-all flex items-center gap-1 cursor-pointer"
          @click="$emit('fullscreen')"
        >
          <svg class="w-3 h-3" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4" /></svg>
          <span class="hidden sm:inline">Mở toàn màn hình</span>
        </button>
      </div>
    </div>

    <!-- Body: DSA engine hoặc ConceptVisualizer -->
    <div class="flex-1 min-h-0 flex gap-2">
      <div class="flex-[65] lg:flex-[65] flex-none w-full lg:w-auto rounded-xl overflow-hidden border border-border-subtle shadow-lg relative min-h-0">
        <!-- DSA algorithm (có engine thật) -->
        <template v-if="isDsaAlgorithm">
          <AlgorithmVisualizer v-if="hasFrames" class="absolute inset-0 w-full h-full" />
          <div v-else-if="vizError" class="absolute inset-0 flex flex-col items-center justify-center gap-3 text-center px-8">
            <div class="w-8 h-8 rounded-full bg-accent-red/10 border border-accent-red/20 flex items-center justify-center">
              <svg class="w-4 h-4 text-accent-red" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.46 0L3.34 16.5c-.77-1.333.192 3 1.732 3z" /></svg>
            </div>
            <p class="text-sm text-text-muted">{{ vizError }}</p>
            <button class="px-4 py-1.5 rounded-lg bg-accent/15 text-accent border border-accent/30 hover:bg-accent/30 transition-all text-xs font-bold cursor-pointer" @click="executeViz">
              Thử lại
            </button>
          </div>
          <div v-else class="absolute inset-0 flex flex-col items-center justify-center gap-3 text-center px-8">
            <div class="w-8 h-8 border-2 border-accent border-t-transparent rounded-full animate-spin"></div>
            <p class="text-sm text-text-muted">Đang sinh mô phỏng {{ algorithmLabel }}...</p>
          </div>
        </template>

        <!-- OOP/SOLID concept -->
        <ConceptVisualizer
          v-else
          :algorithm="algorithm"
          :title="title"
          :description="description"
          :speed="speed"
          class="absolute inset-0 w-full h-full"
          @speed-change="onConceptSpeed"
        />
      </div>

      <!-- Pseudocode -->
      <div v-if="isDsaAlgorithm && pseudoCodeLines.length > 0" class="flex-[35] lg:flex-[35] flex-none w-full lg:w-auto min-h-0 hidden sm:flex">
        <PseudocodeViewer
          :pseudo-code="pseudoCodeLines"
          :active-line="activeLine"
          :description="pseudoDescription"
        />
      </div>
    </div>

    <!-- Mini VCR controls -->
    <div v-if="isDsaAlgorithm && hasFrames" class="shrink-0">
      <AnimationVcrControls
        :is-playing="animStore.isPlaying"
        :current-index="animStore.currentIndex"
        :total-steps="animStore.totalSteps"
        :playback-speed="animStore.playbackSpeed"
        @stop="animStore.stop()"
        @step-backward="animStore.stepBackward()"
        @step-forward="animStore.stepForward()"
        @toggle-play="animStore.togglePlay()"
        @scrub="animStore.scrubTo"
        @speed-change="onSpeedChange"
      />
    </div>

    <div v-else-if="!isDsaAlgorithm" class="shrink-0 text-center text-[10px] text-text-muted">
      Minh hoạ khái niệm — sử dụng bộ chọn tốc độ bên trên
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch, onMounted, onBeforeUnmount } from 'vue';
import BaseIcon from '@/shared/components/BaseIcon.vue';
import AlgorithmVisualizer from '@/features/dsa-modules/components/AlgorithmVisualizer.vue';
import PseudocodeViewer from '@/features/dsa-modules/components/PseudocodeViewer.vue';
import AnimationVcrControls from '@/features/animation-engine/components/AnimationVcrControls.vue';
import ConceptVisualizer from './ConceptVisualizer.vue';
import { useAlgorithmStore } from '@/features/dsa-modules/store/useAlgorithmStore';
import { useAnimationStore } from '@/features/animation-engine/store/useAnimationStore';
import { ALGORITHM_CATALOG } from '@/features/dsa-modules/services/algorithmCatalog';
import { executeDSAAlgorithm } from '@/features/dsa-modules/services/dsaApi';

export interface VisualizerConfig {
  algorithm: string;
  sampleInput?: string;
  speed?: number;
}

const props = defineProps<{
  config?: VisualizerConfig | null;
  moduleKey?: string;
  title?: string;
  description?: string;
}>();

const emit = defineEmits<{
  (e: 'fullscreen'): void;
  (e: 'completeStep'): void;
}>();

const algoStore = useAlgorithmStore();
const animStore = useAnimationStore();

const isExecuting = ref(false);
const hasFrames = ref(false);
const isDemo = ref(false);
const vizError = ref<string | null>(null);

const algorithm = computed(() => props.config?.algorithm || props.moduleKey || 'sorting');
const speed = computed(() => props.config?.speed ?? 1);
const sampleInput = computed(() => props.config?.sampleInput || defaultInput(algorithm.value));

const isDsaAlgorithm = computed(() => {
  const algo = algorithm.value.toLowerCase();
  if (CONCEPT_ALGORITHMS.has(algo)) return false;
  return ALGORITHM_CATALOG.some(a => a.id === algo) || DSA_ALIASES.has(algo);
});

const algorithmLabel = computed(() => {
  const algo = algorithm.value.toLowerCase();
  const catalog = ALGORITHM_CATALOG.find(a => a.id === algo);
  if (catalog) return catalog.name;
  const label = CONCEPT_LABELS[algo];
  return label || algo;
});

const algorithmId = computed(() => DSA_ALIASES.get(algorithm.value.toLowerCase()) ?? algorithm.value.toLowerCase());

const pseudoCodeLines = computed(() => animStore.pseudoCode);
const activeLine = computed(() => animStore.currentFrame?.activeLine);
const pseudoDescription = computed(() => {
  const algo = algorithmId.value;
  const meta = algoStore.metadata;
  if (meta) return meta.description;
  const catalog = ALGORITHM_CATALOG.find(a => a.id === algo);
  return catalog ? `${catalog.name} — ${catalog.category}` : '';
});

function defaultInput(algo: string): string {
  const key = algo.toLowerCase();
  if (key.includes('graph') || key.includes('bfs') || key.includes('dfs') || key.includes('dijkstra') || key.includes('kruskal') || key.includes('prim') || key.includes('bellman')) return '0-1,0-2,1-3,1-4,2-5,2-6';
  if (key.includes('bst') || key.includes('tree')) return '50, 30, 70, 20, 40, 60, 80';
  if (key.includes('stack') || key.includes('queue')) return '10, 20, 30, 40, 50';
  if (key.includes('binary-search') || key.includes('linear-search') || key.includes('sliding-window')) return '2, 5, 8, 12, 16, 23, 38, 56, 72, 91';
  return '5, 3, 8, 1, 9, 2, 7';
}

function parseInput(raw: string): number[] {
  const trimmed = raw.trim();
  if (!trimmed) return [5, 3, 8, 1, 9, 2, 7];
  if (/^[\d,\s-]+$/.test(trimmed) || /^\d+-\d+/.test(trimmed)) {
    // Tách mọi số nguyên trong token — giữ nguyên cạnh đồ thị "0-1,0-2" → [0,1,0,2]
    const nums: number[] = [];
    for (const token of trimmed.split(/[,\s]+/)) {
      if (!token) continue;
      const matches = token.match(/-?\d+/g);
      if (matches) {
        for (const m of matches) {
          const n = parseInt(m, 10);
          if (!Number.isNaN(n)) nums.push(n);
        }
      }
    }
    return nums;
  }
  return [5, 3, 8, 1, 9, 2, 7];
}

async function executeViz(): Promise<void> {
  if (isExecuting.value) return;
  isExecuting.value = true;
  hasFrames.value = false;
  isDemo.value = false;
  vizError.value = null;
  try {
    const algo = ALGORITHM_CATALOG.find(a => a.id === algorithmId.value);
    if (algo) algoStore.selectAlgorithm(algo, 'simulation');
    const data = parseInput(sampleInput.value);
    const result = await executeDSAAlgorithm(algorithmId.value, data);
    isDemo.value = result.demo === true;
    if (result.frames.length === 0) {
      vizError.value = 'Không có frame mô phỏng cho dữ liệu này.';
      return;
    }
    animStore.loadResult({
      algorithmId: result.algorithmId,
      pseudoCode: result.pseudoCode,
      frames: result.frames.map(f => ({
        stepId: f.stepId,
        activeLine: f.activeLine,
        explanation: f.explanation,
        dataState: f.dataState,
        highlights: {
          compare: f.highlights?.compare ?? [],
          swap: f.highlights?.swap ?? [],
          sorted: f.highlights?.sorted ?? [],
          dimmed: f.highlights?.dimmed ?? [],
          active: f.highlights?.active ?? [],
        },
      })),
    });
    animStore.setSpeed(speed.value);
    animStore.play();
    hasFrames.value = true;
  } catch (err) {
    console.error('LessonVisualizer execute failed:', err);
    vizError.value = 'Không thể sinh mô phỏng. Vui lòng thử lại.';
  } finally {
    isExecuting.value = false;
  }
}

function onSpeedChange(newSpeed: number): void {
  animStore.setSpeed(newSpeed);
}

function onConceptSpeed(newSpeed: number): void {
  // Concept visualizer tự xử lý tốc độ; chỉ giữ giá trị để đồng bộ UI
}

watch(() => [props.config?.algorithm, props.config?.sampleInput, props.moduleKey], () => {
  if (isDsaAlgorithm.value) {
    void executeViz();
  }
});

onMounted(() => {
  if (isDsaAlgorithm.value) {
    void executeViz();
  }
});

onBeforeUnmount(() => {
  animStore.stop();
});

const CONCEPT_ALGORITHMS = new Set([
  'encapsulation', 'inheritance', 'polymorphism', 'abstraction',
  'solid-srp', 'solid-ocp', 'solid-lsp', 'solid-isp', 'solid-dip',
  'srp', 'ocp', 'lsp', 'isp', 'dip', 'oop', 'solid', 'strategy',
]);

const DSA_ALIASES = new Map<string, string>([
  ['sorting', 'bubble-sort'],
  ['graph', 'bfs'],
  ['dsa', 'bubble-sort'],
  ['bubble', 'bubble-sort'],
  ['quick', 'quick-sort'],
  ['merge', 'merge-sort'],
  ['heap', 'heap-sort'],
  ['radix-sort', 'radix-sort'],
  ['counting-sort', 'counting-sort'],
  ['bucket-sort', 'bucket-sort'],
  ['linear-search', 'linear-search'],
  ['binary-search', 'binary-search'],
  ['sliding-window', 'sliding-window'],
  ['stack', 'stack'],
  ['queue', 'queue'],
  ['monotonic-stack', 'monotonic-stack'],
  ['bst', 'bst'],
  ['tree', 'bst'],
  ['binary-search-tree', 'bst'],
  ['bfs', 'bfs'],
  ['dfs', 'dfs'],
  ['dijkstra', 'dijkstra'],
  ['bellman-ford', 'bellman-ford'],
  ['bellmanford', 'bellman-ford'],
  ['bellman', 'bellman-ford'],
  ['kruskal', 'kruskal'],
  ['prim', 'prim'],
  ['tarjan', 'tarjan'],
  ['a-star', 'a-star'],
  ['astar', 'a-star'],
  ['a*', 'a-star'],
]);

const CONCEPT_LABELS: Record<string, string> = {
  encapsulation: 'Encapsulation (Đóng gói)',
  inheritance: 'Inheritance (Kế thừa)',
  polymorphism: 'Polymorphism (Đa hình)',
  abstraction: 'Abstraction (Trừu tượng)',
  'solid-srp': 'SOLID - Single Responsibility',
  'solid-ocp': 'SOLID - Open/Closed',
  'solid-lsp': 'SOLID - Liskov Substitution',
  'solid-isp': 'SOLID - Interface Segregation',
  'solid-dip': 'SOLID - Dependency Inversion',
  srp: 'SOLID - Single Responsibility',
  ocp: 'SOLID - Open/Closed',
  lsp: 'SOLID - Liskov Substitution',
  isp: 'SOLID - Interface Segregation',
  dip: 'SOLID - Dependency Inversion',
  oop: 'Lập trình hướng đối tượng (OOP)',
  solid: '5 Nguyên lý SOLID',
  strategy: 'Strategy Pattern',
};
</script>
