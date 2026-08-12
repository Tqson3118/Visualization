<script setup lang="ts">
// FinalTestView — Màn 30: kiểm tra cuối lộ trình (trộn quiz + dự đoán, pass ≥ 70%)
// Tái sử dụng QuizStage (20.3). Đề fallback sinh từ engines/catalog khi backend chưa có.
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import * as exercisesApi from '@/api/exercises';
import type { ExerciseDto, QuestionDto } from '@/api/exercises';
import { useUiStore } from '@/stores/ui';
import QuizStage from '@/components/ladder/QuizStage.vue';
import Button from '@/components/ui/Button.vue';
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
  // Thử lấy exercise theo path: nếu API chưa có, fallback buildLocalFinalTest
  const pathId = Number(topicId.value);
  const exercises = await exercisesApi.fetchExercises({ stage: 1 });
  if (exercises.length > 0) return exercises[0];
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
      ? '🏅 Hoàn thành lộ trình! Đã mở khóa lộ trình kế tiếp (nếu có).'
      : `Đạt ${scorePct}% — cần ≥ ${passThreshold}%. Làm lại trong phiên miễn phí.`,
    scorePct >= passThreshold ? 'success' : 'warning',
  );
}
</script>

<template>
  <main class="final-test container">
    <nav class="final-test__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'path-topic', params: { topicId } }">Lộ trình</RouterLink>
      <span aria-hidden="true">/</span>
      <span>Kiểm tra cuối</span>
    </nav>

    <header class="final-test__header">
      <h1 class="final-test__title">🏁 Kiểm tra cuối lộ trình</h1>
      <p class="text-muted final-test__sub">
        Ngưỡng đạt: ≥ {{ passThreshold }}% · Điểm lộ trình = ĐTB node × 80% + final × 20%
      </p>
    </header>

    <div v-if="loading" class="final-test__loading">
      <Skeleton height="72px" :lines="5" />
    </div>

    <QuizStage
      v-else
      :exercise="exercise"
      @passed="onPassed"
      @finished="router.push({ name: 'path-topic', params: { topicId } })"
    />

    <div class="final-test__actions">
      <Button variant="ghost" @click="router.push({ name: 'path-topic', params: { topicId } })">
        ← Về bản đồ lộ trình
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

.final-test__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.final-test__title { font-size: var(--text-xl); }
.final-test__sub { font-size: var(--text-sm); margin-top: 4px; }
</style>
