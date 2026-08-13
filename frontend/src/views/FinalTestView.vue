<script setup lang="ts">
// FinalTestView — Màn 30: kiểm tra cuối lộ trình (trộn quiz + dự đoán, pass ≥ 70%)
// Tái sử dụng QuizStage (20.3). Đề fallback sinh từ engines/catalog khi backend chưa có.
// H-E3: chrome hero gradient Aurora (mốc hoàn thành lộ trình) + rules strip + skeleton card
// + micro-interaction nhẹ (Motion hero) + i18n. GIỮ NGUYÊN logic fetch/build/submit.
import { computed, onMounted, ref } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';
import { Flag, Gauge, Repeat, Trophy } from 'lucide-vue-next';
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

const route = useRoute();
const router = useRouter();
const ui = useUiStore();

const topicId = computed(() => String(route.params.topicId ?? ''));

const exercise = ref<ExerciseDto | null>(null);
const loading = ref(true);
const passThreshold = 70;

onMounted(async () => {
  try {
    // Ưu tiên lấy đề từ backend (final-test của path)
    const fetched = await fetchLocalFinalTest();
    exercise.value = fetched;
  } catch {
    // Fallback: đề mẫu tổng hợp từ catalog
    exercise.value = buildLocalFinalTest();
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
  return buildLocalFinalTest();
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
    scorePct >= passThreshold
      ? messages.finalTest.toastPassed
      : messages.finalTest.toastFailed(passThreshold, scorePct),
    scorePct >= passThreshold ? 'success' : 'warning',
  );
}
</script>

<template>
  <main class="final-test container">
    <nav class="final-test__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'path-topic', params: { topicId } }">
        {{ messages.finalTest.breadcrumbPath }}
      </RouterLink>
      <span aria-hidden="true">/</span>
      <span>{{ messages.finalTest.breadcrumbLabel }}</span>
    </nav>

    <!-- Hero gradient Aurora (mốc hoàn thành lộ trình — chữ trắng AA, đồng bộ LessonView) -->
    <Motion
      class="final-test__hero"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.32, ease: 'easeOut' }"
    >
      <div class="final-test__hero-body">
        <span class="final-test__icon" aria-hidden="true">
          <Flag :size="22" />
        </span>
        <div class="final-test__hero-title-wrap">
          <h1 class="final-test__title">{{ messages.finalTest.title }}</h1>
          <p class="final-test__sub">{{ messages.finalTest.subtitle(passThreshold) }}</p>
        </div>
        <Badge variant="primary" class="final-test__badge">
          {{ messages.finalTest.badge }}
        </Badge>
      </div>
    </Motion>

    <!-- Rules strip: ngưỡng đạt / trọng số / làm lại -->
    <div class="final-test__rules" :aria-label="messages.finalTest.rulesAria">
      <div class="final-test__rule">
        <span class="final-test__rule-icon" aria-hidden="true"><Gauge :size="16" /></span>
        <div class="final-test__rule-text">
          <p class="final-test__rule-label">{{ messages.finalTest.ruleThreshold }}</p>
          <p class="final-test__rule-value">{{ messages.finalTest.ruleThresholdValue }}</p>
        </div>
      </div>
      <div class="final-test__rule">
        <span class="final-test__rule-icon" aria-hidden="true"><Trophy :size="16" /></span>
        <div class="final-test__rule-text">
          <p class="final-test__rule-label">{{ messages.finalTest.ruleWeight }}</p>
          <p class="final-test__rule-value">{{ messages.finalTest.ruleWeightValue }}</p>
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

/* ── Breadcrumb (ngoài hero — nền trang, link primary đủ AA) ── */
.final-test__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.final-test__breadcrumb a { color: var(--color-primary); font-weight: 600; }

/* ── Hero gradient — Aurora (palette 1 — teal → cyan → violet, chữ trắng AA) ── */
.final-test__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border-radius: var(--radius-xl);
  background-image: var(--gradient-aurora);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

/* Điểm sáng trang trí (decorative) */
.final-test__hero::before {
  content: '';
  position: absolute;
  width: 240px;
  height: 240px;
  border-radius: 50%;
  top: -110px;
  right: -50px;
  z-index: -1;
  background: color-mix(in srgb, var(--color-info) 22%, transparent);
  filter: blur(56px);
}

/* GP-T9b (#12): dark mode gradient Aurora sáng (0.72-0.86) làm chữ trắng khó đọc
   → phủ lớp tối (0.72) để chữ trắng ≥ 4.5:1 (stop violet tối nhất cũng ≥ 4.6:1). */
.dark .final-test__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: rgba(4, 47, 46, 0.72);
}

.final-test__hero-body {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.final-test__icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-lg);
  background-image: var(--gradient-aurora);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-md);
}

.final-test__hero-title-wrap {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
  min-width: 220px;
}

.final-test__title {
  font-size: clamp(var(--text-2xl), 4vw, var(--text-3xl));
  color: #fff;
  margin: 0;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.16);
}

.final-test__sub {
  font-size: var(--text-sm);
  color: rgba(255, 255, 255, 0.92);
  max-width: 64ch;
  margin: 0;
}

.final-test__badge { margin-left: auto; align-self: flex-start; }

/* ── Rules strip ── */
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
  box-shadow: var(--shadow-sm);
  transition: transform 180ms ease, box-shadow 180ms ease, border-color 180ms ease;
}

.final-test__rule:hover {
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
  border-color: color-mix(in srgb, var(--color-primary) 45%, var(--color-border));
}

.final-test__rule-icon {
  flex-shrink: 0;
  width: 32px;
  height: 32px;
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-primary) 12%, transparent);
  color: var(--color-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.final-test__rule-text { display: flex; flex-direction: column; gap: 0; min-width: 0; }

.final-test__rule-label {
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  font-weight: 600;
}

.final-test__rule-value {
  font-size: var(--text-sm);
  font-weight: 700;
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
  .final-test__hero { padding: var(--space-md); }
  .final-test__badge { margin-left: 0; }
  .final-test__actions { justify-content: flex-start; }
}
</style>
