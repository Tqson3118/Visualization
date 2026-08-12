<script setup lang="ts">
// LessonView — Màn 04: chi tiết bài học (rich-text + mô phỏng/bài tập + ghi chú + đánh giá)
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { useLessonStore } from '@/stores/lesson';
import { useUiStore } from '@/stores/ui';
import LessonDetail from '@/components/lesson/LessonDetail.vue';
import Button from '@/components/ui/Button.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const route = useRoute();
const router = useRouter();
const lessonStore = useLessonStore();
const ui = useUiStore();

const lessonId = computed(() => String(route.params.lessonId ?? ''));
const error = ref('');

onMounted(async () => {
  try {
    await lessonStore.fetchLesson(Number(lessonId.value));
  } catch {
    error.value = 'Bài học không tồn tại hoặc đã bị ẩn.';
  }
});

function openSimulation(key: string): void {
  void router.push({ name: 'simulator', params: { key } });
}

function openExercise(id: number): void {
  void router.push({ name: 'exercise', params: { id: String(id) } });
}
</script>

<template>
  <main class="lesson-view container">
    <nav class="lesson-view__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'path' }">Lộ trình</RouterLink>
      <span aria-hidden="true">/</span>
      <span>{{ lessonStore.currentLesson?.title ?? 'Bài học' }}</span>
    </nav>

    <EmptyState
      v-if="error"
      icon="alert-circle"
      title="Bài học không tồn tại"
      :description="error"
      action-label="Về lộ trình"
      @action="router.push({ name: 'path' })"
    />

    <LessonDetail
      v-else
      :lesson-id="lessonId"
      @open-simulation="openSimulation"
      @open-exercise="openExercise"
    />

    <div class="lesson-view__actions">
      <Button variant="ghost" @click="router.push({ name: 'path' })">← Về lộ trình</Button>
      <Button variant="secondary" @click="ui.showToast('Bước sau: mở mô phỏng liên quan từ thẻ bên trên.', 'info')">
        Học tiếp
      </Button>
    </div>
  </main>
</template>

<style scoped>
.lesson-view {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.lesson-view__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.lesson-view__actions {
  display: flex;
  justify-content: space-between;
  gap: var(--space-sm);
  padding-top: var(--space-md);
  border-top: 1px solid var(--color-border);
}
</style>
