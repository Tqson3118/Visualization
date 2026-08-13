<script setup lang="ts">
// ExerciseView — Màn 06: bài tập trắc nghiệm (Bậc 1 / kiểm tra) tại /exercise/:id
// Tái sử dụng QuizStage + nút chuyển chế độ luyện tập (FR-4.6) + liên kết lý thuyết.
// Phase 1 view-quality: toolbar = surface band level-2 (thay class .card có shadow),
// kicker mono (bỏ 700 + tracking dương), toast không emoji, nút toggle có aria-pressed.
import { onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import * as exercisesApi from '@/api/exercises';
import type { ExerciseDto } from '@/api/exercises';
import QuizStage from '@/components/ladder/QuizStage.vue';
import Button from '@/components/ui/Button.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import { fireConfetti } from '@/composables/useConfetti';
import { useUiStore } from '@/stores/ui';

const route = useRoute();
const router = useRouter();
const ui = useUiStore();

const exercise = ref<ExerciseDto | null>(null);
const loading = ref(true);
const error = ref('');
const practiceMode = ref(false);

onMounted(async () => {
  const id = Number(route.params.id);
  try {
    exercise.value = await exercisesApi.fetchExercise(id);
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Bài tập không tồn tại hoặc đã bị ẩn.';
  } finally {
    loading.value = false;
  }
});

function onPassed(): void {
  // G-F2b: confetti khi pass (QuizStage cũng toast kết quả lúc nộp)
  fireConfetti('success');
  ui.showToast('Hoàn thành bài tập!', 'success');
}

function onFinished(): void {
  router.push({ name: 'path' });
}
</script>

<template>
  <main class="exercise container">
    <nav class="exercise__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'path' }">Lộ trình</RouterLink>
      <span aria-hidden="true">/</span>
      <span>Bài tập</span>
    </nav>

    <div v-if="loading" class="exercise__loading">
      <Skeleton height="64px" :lines="4" />
    </div>

    <EmptyState
      v-else-if="error"
      icon="puzzle"
      title="Bài tập không tồn tại"
      :description="error"
      action-label="Về lộ trình"
      @action="router.push({ name: 'path' })"
    />

    <template v-else>
      <header class="exercise__toolbar">
        <div class="exercise__toolbar-info">
          <p class="exercise__toolbar-kicker">Bài tập trắc nghiệm</p>
          <h1 class="exercise__title">{{ exercise?.title ?? 'Bài tập' }}</h1>
          <p v-if="exercise?.description" class="exercise__toolbar-desc">
            {{ exercise.description }}
          </p>
        </div>
        <Button
          size="sm"
          :variant="practiceMode ? 'primary' : 'secondary'"
          :aria-pressed="practiceMode"
          @click="practiceMode = !practiceMode"
        >
          {{ practiceMode ? 'Làm bài chính thức' : 'Luyện tập (không chấm điểm)' }}
        </Button>
      </header>

      <QuizStage
        :exercise="exercise"
        :practice-mode="practiceMode"
        @passed="onPassed"
        @finished="onFinished"
      />
    </template>
  </main>
</template>

<style scoped>
.exercise {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.exercise__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.04em;
  color: var(--color-text-muted);
}

.exercise__breadcrumb a { color: var(--color-primary); font-weight: 600; text-decoration: none; }

/* Toolbar — surface band level-2 (DESIGN.md §1), không shadow */
.exercise__toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-md);
  flex-wrap: wrap;
  padding: var(--space-lg);
  border-radius: var(--radius-lg);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
}

.exercise__toolbar-info { display: flex; flex-direction: column; gap: var(--space-xs); }

.exercise__toolbar-kicker {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
}

.exercise__title {
  font-size: var(--text-2xl);
  font-weight: 600;
  letter-spacing: -0.015em;
  margin: 0;
}

.exercise__toolbar-desc {
  font-size: var(--text-sm);
  color: var(--color-text-muted);
  margin: 0;
}

.exercise__loading { display: flex; flex-direction: column; gap: var(--space-md); }
</style>
