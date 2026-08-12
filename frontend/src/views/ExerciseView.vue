<script setup lang="ts">
// ExerciseView — Màn 06: bài tập trắc nghiệm (Bậc 1 / kiểm tra) tại /exercise/:id
// Tái sử dụng QuizStage + nút chuyển chế độ luyện tập (FR-4.6) + liên kết lý thuyết.
import { onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import * as exercisesApi from '@/api/exercises';
import type { ExerciseDto } from '@/api/exercises';
import QuizStage from '@/components/ladder/QuizStage.vue';
import Button from '@/components/ui/Button.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
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
  ui.showToast('🎉 Hoàn thành bài tập!', 'success');
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
      <div class="exercise__toolbar">
        <h1 class="exercise__title">{{ exercise?.title ?? 'Bài tập' }}</h1>
        <Button
          size="sm"
          :variant="practiceMode ? 'primary' : 'secondary'"
          @click="practiceMode = !practiceMode"
        >
          {{ practiceMode ? 'Làm bài chính thức' : 'Luyện tập (không chấm điểm)' }}
        </Button>
      </div>

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
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.exercise__toolbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.exercise__title { font-size: var(--text-xl); }

.exercise__loading { display: flex; flex-direction: column; gap: var(--space-md); }
</style>
