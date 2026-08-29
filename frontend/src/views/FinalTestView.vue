<script setup lang="ts">
// FinalTestView — Màn 30: kiểm tra cuối lộ trình (trộn quiz + dự đoán, pass ≥ 70%)
// Tái sử dụng QuizStage (20.3). Đề fallback sinh từ engines/catalog khi backend chưa có.
// View-quality (Phase 2 bổ sung): banner gradient aurora + blob + overlay hack → surface band
// level-2 + kicker mono `FINAL TEST · PASS ≥ 70%` (dữ liệu thật threshold — quyết định #1);
// rules strip bỏ shadow + hover-lift → level-1, giá trị số mono + weight 600 (bỏ 700);
// easing chuẩn; icon gradient → ô muted + lucide 20px tertiary; badge muted; bỏ 🏅/← i18n.
// GIỮ NGUYÊN logic fetch/build/submit.
import { computed, onMounted, ref } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';
import { ArrowLeft, Flag, Gauge, Repeat, Trophy } from 'lucide-vue-next';
import { Motion } from 'motion-v';

import * as exercisesApi from '@/api/exercises';
import type { ExerciseDto, QuestionDto } from '@/api/exercises';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import QuizStage from '@/components/ladder/QuizStage.vue';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/Button.vue';
import Card from '@/components/ui/Card.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import { CATALOG } from '@/engines/catalog';
import { allowLocalFallbacks } from '@/config/runtime';

const route = useRoute();
const router = useRouter();
const ui = useUiStore();

const topicId = computed(() => String(route.params.topicId ?? ''));

const exercise = ref<ExerciseDto | null>(null);
const loading = ref(true);
const passThreshold = computed(() => exercise.value?.passThreshold ?? exercise.value?.passingScore ?? 70);

onMounted(async () => {
  try {
    // Ưu tiên lấy đề từ backend (final-test của path)
    const fetched = await fetchLocalFinalTest();
    exercise.value = fetched;
  } catch {
    if (allowLocalFallbacks) {
      exercise.value = buildLocalFinalTest();
    }
  } finally {
    loading.value = false;
  }
});

async function fetchLocalFinalTest(): Promise<ExerciseDto> {
  // Lấy danh sách theo stage=1 (Quiz) → lấy chi tiết theo id (đề có kèm câu hỏi — GET /exercises/{id})
  const pathId = Number(topicId.value);
  const exercises = await exercisesApi.fetchExercises({ stage: 1 });
  if (exercises.length > 0) return exercisesApi.fetchExercise(exercises[0].id);
  void pathId;
  if (allowLocalFallbacks) return buildLocalFinalTest();
  throw new Error('No final test is available from the API');
}

function buildLocalFinalTest(): ExerciseDto {
  const keys = CATALOG.filter((c) => c.category === 'algorithm').slice(0, 5);
  const questions: QuestionDto[] = keys.map((meta, idx) => ({
    id: 1000 + idx,
    content: `Độ phức tạp trung bình của "${meta.title}" là bao nhiêu?`,
    type: 'SINGLE',
    options: [meta.complexity.average, meta.complexity.best, meta.complexity.worst, 'O(1)'],
    points: 2,
  }));
  return {
    id: 0,
    title: 'Kiểm tra cuối lộ trình — Đề mẫu',
    description: 'Đề tổng hợp từ danh mục mô phỏng (backend chưa khả dụng).',
    type: 'MCQ',
    lessonId: null,
    nodeId: null,
    stage: 1,
    durationMinutes: 20,
    maxScore: questions.length * 2,
    status: 'active',
    questions,
  };
}

function onPassed(scorePct: number): void {
  ui.showToast(
    scorePct >= passThreshold.value
      ? messages.finalTest.toastPassed
      : messages.finalTest.toastFailed(passThreshold.value, scorePct),
    scorePct >= passThreshold.value ? 'success' : 'warning',
  );
}
</script>

<template>
  <main class="final-test container">
    <!-- Chrome header — surface band level-2 + kicker mono (DESIGN.md §1/§6, không gradient) -->
    <Motion
      as="header"
      class="final-test__chrome"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.28, ease: [0.16, 1, 0.3, 1] }"
    >
      <nav class="final-test__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'path-topic', params: { topicId } }">
          {{ messages.finalTest.breadcrumbPath }}
        </RouterLink>
        <span aria-hidden="true">/</span>
        <span aria-current="page">{{ messages.finalTest.breadcrumbLabel }}</span>
      </nav>

      <p class="final-test__kicker">
        FINAL TEST · PASS ≥ {{ passThreshold }}%
      </p>

      <div class="final-test__hero">
        <span class="final-test__icon" aria-hidden="true">
          <Flag :size="20" />
        </span>
        <div class="final-test__hero-title-wrap">
          <h1 class="final-test__title">{{ messages.finalTest.title }}</h1>
          <p class="final-test__sub">{{ messages.finalTest.subtitle(passThreshold) }}</p>
        </div>
        <Badge variant="muted" class="final-test__badge">
          {{ messages.finalTest.badge }}
        </Badge>
      </div>
    </Motion>

    <!-- Rules strip: ngưỡng đạt / trọng số / làm lại — level-1, không shadow/hover-lift -->
    <div class="final-test__rules" :aria-label="messages.finalTest.rulesAria">
      <div class="final-test__rule">
        <span class="final-test__rule-icon" aria-hidden="true"><Gauge :size="16" /></span>
        <div class="final-test__rule-text">
          <p class="final-test__rule-label">{{ messages.finalTest.ruleThreshold }}</p>
          <p class="final-test__rule-value"><span class="font-mono">{{ messages.finalTest.ruleThresholdValue }}</span></p>
        </div>
      </div>
      <div class="final-test__rule">
        <span class="final-test__rule-icon" aria-hidden="true"><Trophy :size="16" /></span>
        <div class="final-test__rule-text">
          <p class="final-test__rule-label">{{ messages.finalTest.ruleWeight }}</p>
          <p class="final-test__rule-value">
            <span class="font-mono">20%</span> điểm lộ trình
          </p>
        </div>
      </div>
      <div class="final-test__rule">
        <span class="final-test__rule-icon" aria-hidden="true"><Repeat :size="16" /></span>
        <div class="final-test__rule-text">
          <p class="final-test__rule-label">{{ messages.finalTest.ruleRetry }}</p>
          <p class="final-test__rule-value">{{ messages.finalTest.ruleRetryValue }}</p>
        </div>
      </div>
    </div>

    <div v-if="loading" class="final-test__loading">
      <Card class="final-test__loading-card">
        <Skeleton height="28px" width="55%" />
        <Skeleton height="16px" :lines="3" />
      </Card>
    </div>

    <QuizStage
      v-else
      :exercise="exercise"
      @passed="onPassed"
      @finished="router.push({ name: 'path-topic', params: { topicId } })"
    />

    <div class="final-test__actions">
      <Button variant="ghost" @click="router.push({ name: 'path-topic', params: { topicId } })">
        <ArrowLeft :size="16" aria-hidden="true" />
        {{ messages.finalTest.backToMap }}
      </Button>
    </div>
  </main>
</template>

<style scoped>
.final-test {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Chrome header — surface band level-2 (§6): card-raised + border-subtle, KHÔNG shadow ── */
.final-test__chrome {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-lg) var(--space-xl);
}

.final-test__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}

.final-test__breadcrumb a { color: var(--color-primary); font-weight: 600; }

.final-test__kicker {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
  margin: 0;
}

.final-test__hero {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.final-test__icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-text-tertiary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.final-test__hero-title-wrap {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  flex: 1;
  min-width: 220px;
}

.final-test__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  line-height: 1.1;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
  margin: 0;
}

.final-test__sub {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  max-width: 64ch;
  margin: 0;
}

.final-test__badge { margin-left: auto; align-self: flex-start; }

/* ── Rules strip — level-1 (§6): surface + border, KHÔNG shadow, hover chỉ đổi border ── */
.final-test__rules {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: var(--space-sm);
}

.final-test__rule {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-sm) var(--space-md);
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.final-test__rule:hover {
  border-color: var(--color-border-strong);
}

.final-test__rule-icon {
  flex-shrink: 0;
  width: 32px;
  height: 32px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-text-tertiary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.final-test__rule-text { display: flex; flex-direction: column; gap: var(--space-xs); min-width: 0; }

.final-test__rule-label {
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  font-weight: 400;
  margin: 0;
}

.final-test__rule-value {
  font-size: var(--text-sm);
  font-weight: 600;
  margin: 0;
}

/* ── Loading ── */
.final-test__loading-card {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

/* ── Actions ── */
.final-test__actions {
  display: flex;
  justify-content: flex-end;
  padding-top: var(--space-md);
  border-top: 1px solid var(--color-border);
}

@media (max-width: 640px) {
  .final-test__chrome { padding: var(--space-md); }
  .final-test__badge { margin-left: 0; }
  .final-test__hero { align-items: flex-start; }
  .final-test__actions { justify-content: flex-start; }
}
</style>
