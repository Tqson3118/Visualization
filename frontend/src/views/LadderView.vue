<script setup lang="ts">
// LadderView — Màn 14: Practice Ladder shell (stepper 3 bậc + stages tách component)
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { useLessonStore } from '@/stores/lesson';
import * as exercisesApi from '@/api/exercises';
import type { ExerciseDto } from '@/api/exercises';
import { getCatalogMeta } from '@/engines/catalog';
import LadderShell from '@/components/ladder/LadderShell.vue';
import Button from '@/components/ui/Button.vue';
import Skeleton from '@/components/ui/Skeleton.vue';

const route = useRoute();
const router = useRouter();
const lessonStore = useLessonStore();

const nodeId = computed(() => String(route.params.nodeId ?? ''));
const topicId = ref<number>(1);

const simKey = computed(() => {
  // Suy key mô phỏng từ nodeId (fallback cục bộ)
  const keys = ['sort.bubble', 'search.binary', 'graph.bfs', 'tree.bst-insert', 'stack.push', 'queue.enqueue', 'sort.merge', 'sort.quick'];
  const key = keys[(Number(nodeId.value) - 1) % keys.length];
  return getCatalogMeta(key) ? key : 'sort.bubble';
});

const nodeTitle = computed(() => getCatalogMeta(simKey.value)?.title ?? `Node ${nodeId.value}`);

/** Exercise Ladder theo node: quiz (stage 1) + code (stage 3) — GET /exercises?nodeId&stage (SETUP_TODO §6.6) */
const quizExercise = ref<ExerciseDto | null>(null);
const quizLoading = ref(true);
const codeExerciseId = ref<number | null>(null);

async function loadLadderExercises(): Promise<void> {
  const node = Number(nodeId.value);
  try {
    const [quizList, codeList] = await Promise.all([
      exercisesApi.fetchExercises({ nodeId: node, stage: 1 }),
      exercisesApi.fetchExercises({ nodeId: node, stage: 3 }),
    ]);
    if (quizList.length > 0) {
      quizExercise.value = await exercisesApi.fetchExercise(quizList[0].id);
    }
    codeExerciseId.value = codeList[0]?.id ?? null;
  } catch {
    // API lỗi → giữ null; LadderShell hiện EmptyState thay vì crash
    quizExercise.value = null;
    codeExerciseId.value = null;
  } finally {
    quizLoading.value = false;
  }
}

onMounted(async () => {
  try {
    await lessonStore.fetchTopics();
    const first = lessonStore.topics[0];
    if (first) topicId.value = first.id;
  } catch {
    topicId.value = 1;
  }
  await loadLadderExercises();
});

function onPassed(stage: number): void {
  // stage 1,2,3 — LadderShell tự lưu; chuyển Lab khi pass Quiz nếu cần
  void stage;
}
</script>

<template>
  <main class="ladder container">
    <nav class="ladder__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'path-topic', params: { topicId: String(topicId) } }">Lộ trình</RouterLink>
      <span aria-hidden="true">/</span>
      <span>{{ nodeTitle }}</span>
    </nav>

    <header class="ladder__header">
      <h1 class="ladder__title">🪜 Practice Ladder — {{ nodeTitle }}</h1>
      <p class="text-muted ladder__sub">
        Quiz (20%) → Lab (30%) → Code (50%) · giữ MAX mỗi bậc · session 30 phút
      </p>
    </header>

    <div v-if="quizLoading" class="ladder__loading">
      <Skeleton height="96px" :lines="3" />
    </div>

    <LadderShell
      v-else
      :node-id="nodeId"
      :quiz-exercise="quizExercise"
      :quiz-loading="quizLoading"
      :simulation-key="simKey"
      :code-exercise-id="codeExerciseId"
      @passed="onPassed"
    />

    <div class="ladder__actions">
      <Button variant="ghost" @click="router.push({ name: 'path-topic', params: { topicId: String(topicId) } })">
        ← Thoát (giữ bậc đã pass)
      </Button>
      <Button variant="secondary" @click="router.push({ name: 'lab', params: { nodeId } })">
        Mở Lab trực tiếp →
      </Button>
    </div>
  </main>
</template>

<style scoped>
.ladder {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.ladder__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.ladder__title { font-size: var(--text-xl); }
.ladder__sub { font-size: var(--text-sm); margin-top: 4px; }

.ladder__loading { display: flex; flex-direction: column; gap: var(--space-md); }

.ladder__actions {
  display: flex;
  justify-content: space-between;
  gap: var(--space-sm);
  padding-top: var(--space-md);
  border-top: 1px solid var(--color-border);
}
</style>
