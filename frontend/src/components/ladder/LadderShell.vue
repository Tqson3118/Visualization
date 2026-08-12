<script setup lang="ts">
// LadderShell — shell 3 bậc của 1 node (Màn 14 — FR-4.11)
// Stepper + QuizStage/LabStage/CodeStage tách component; lưu bậc đã pass (localStorage fallback).
import { computed, ref, watch } from 'vue';

import type { ExerciseDto } from '@/api/exercises';
import { useUiStore } from '@/stores/ui';
import LadderStepper from './LadderStepper.vue';
import QuizStage from './QuizStage.vue';
import LabStage from './LabStage.vue';
import CodeStage from './CodeStage.vue';
import { getCatalogMeta } from '@/engines/catalog';
import Card from '@/components/ui/Card.vue';

const props = withDefaults(
  defineProps<{
    nodeId: number | string;
    /** Bài tập MCQ Bậc 1 (nếu có) */
    quizExercise?: ExerciseDto | null;
    quizLoading?: boolean;
    /** Key mô phỏng cho Bậc 2 Lab */
    simulationKey?: string | null;
    /** Exercise code cho Bậc 3 */
    codeExerciseId?: number | null;
  }>(),
  {
    quizExercise: null,
    quizLoading: false,
    simulationKey: null,
    codeExerciseId: null,
  },
);

const emit = defineEmits<{
  passed: [stage: number];
}>();

const ui = useUiStore();

type StageKey = 'quiz' | 'lab' | 'code';
const STAGES: Array<{ key: StageKey; label: string }> = [
  { key: 'quiz', label: 'Quiz' },
  { key: 'lab', label: 'Lab' },
  { key: 'code', label: 'Code' },
];

const activeKey = ref<StageKey>('quiz');
const passedStages = ref<Set<StageKey>>(new Set());

const storageKey = computed(() => `dsa-ladder-${props.nodeId}`);

function loadProgress(): void {
  try {
    const raw = localStorage.getItem(storageKey.value);
    if (raw) {
      const parsed = JSON.parse(raw) as StageKey[];
      passedStages.value = new Set(parsed);
    }
  } catch {
    passedStages.value = new Set();
  }
}

function saveProgress(): void {
  try {
    localStorage.setItem(storageKey.value, JSON.stringify([...passedStages.value]));
  } catch {
    // localStorage không khả dụng (private mode) — bỏ qua
  }
}

watch(storageKey, loadProgress, { immediate: true });

const stageStatus = computed<Array<{ key: StageKey; label: string; status: 'locked' | 'active' | 'passed' }>>(() =>
  STAGES.map((stage, idx) => {
    if (passedStages.value.has(stage.key)) return { ...stage, status: 'passed' };
    const prev = STAGES[idx - 1];
    if (idx === 0 || (prev && passedStages.value.has(prev.key))) return { ...stage, status: 'active' };
    return { ...stage, status: 'locked' };
  }),
);

function selectStage(key: string): void {
  const stage = stageStatus.value.find((s) => s.key === key);
  if (stage && stage.status !== 'locked') activeKey.value = key as StageKey;
}

function onStagePassed(stage: StageKey): void {
  passedStages.value.add(stage);
  saveProgress();
  const next = STAGES.find((s) => !passedStages.value.has(s.key));
  if (next) activeKey.value = next.key;
  emit('passed', STAGES.findIndex((s) => s.key === stage) + 1);
}

const simMeta = computed(() => (props.simulationKey ? getCatalogMeta(props.simulationKey) : null));
</script>

<template>
  <section class="ladder-shell">
    <LadderStepper :stages="stageStatus" :active-key="activeKey" @select="selectStage" />

    <Card class="ladder-shell__body-card">
      <QuizStage
        v-if="activeKey === 'quiz'"
        :exercise="quizExercise"
        :loading="quizLoading"
        @passed="onStagePassed('quiz')"
        @finished="onStagePassed('quiz')"
      />

      <div v-else-if="activeKey === 'lab'" class="ladder-shell__lab-wrap">
        <p v-if="!simulationKey" class="ladder-shell__note">
          Bậc 2 không khả dụng (node chưa gắn mô phỏng). Vẫn có thể học Bậc 1/3.
        </p>
        <LabStage
          v-else
          :title="`Interactive Lab — ${simMeta?.title ?? simulationKey}`"
          @passed="onStagePassed('lab')"
        />
      </div>

      <div v-else-if="activeKey === 'code'">
        <CodeStage
          :sim-key="simulationKey ?? 'sort.bubble'"
          :exercise-id="codeExerciseId"
          @passed="onStagePassed('code')"
        />
      </div>
    </Card>
  </section>
</template>

<style scoped>
.ladder-shell { display: flex; flex-direction: column; gap: var(--space-lg); }

/* Card bọc stage (shadcn Card — hover-lift nhẹ) */
.ladder-shell__body-card {
  transition: box-shadow 180ms ease, transform 180ms ease;
}

.ladder-shell__body-card:hover {
  box-shadow: var(--shadow-md);
  transform: translateY(-1px);
}

.ladder-shell__lab-wrap { min-height: 200px; }

.ladder-shell__note {
  color: var(--color-text-muted);
  font-size: var(--text-sm);
  background: var(--color-muted);
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-md);
}
</style>
