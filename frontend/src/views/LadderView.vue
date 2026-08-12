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
import Badge from '@/components/ui/Badge.vue';
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
    <!-- Hero gradient Sunset (G-F2a palette 2) -->
    <header class="ladder__hero">
      <nav class="ladder__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'path-topic', params: { topicId: String(topicId) } }">Lộ trình</RouterLink>
        <span aria-hidden="true">/</span>
        <span>{{ nodeTitle }}</span>
      </nav>

      <div class="ladder__hero-body">
        <div class="ladder__hero-title-wrap">
          <h1 class="ladder__title">🪜 Practice Ladder</h1>
          <p class="ladder__sub">
            {{ nodeTitle }} · Quiz (20%) → Lab (30%) → Code (50%) · giữ MAX mỗi bậc · session 30 phút
          </p>
        </div>
        <Badge variant="primary" class="ladder__hero-badge">Đang học · Node {{ nodeId }}</Badge>
      </div>
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

/* ── Hero gradient Sunset (palette 2 — amber → rose) ── */
.ladder__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-warning) 32%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-sunset);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.ladder__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: color-mix(in srgb, var(--color-background) 60%, transparent);
}

.ladder__hero::before {
  content: '';
  position: absolute;
  width: 240px;
  height: 240px;
  border-radius: 50%;
  top: -110px;
  right: -50px;
  z-index: -1;
  background: color-mix(in srgb, var(--color-warning) 26%, transparent);
  filter: blur(56px);
}

.ladder__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.ladder__breadcrumb a { color: var(--color-primary); font-weight: 600; }

.ladder__hero-body {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.ladder__hero-title-wrap { display: flex; flex-direction: column; gap: 6px; }

.ladder__title {
  font-size: var(--text-3xl);
  background-image: var(--gradient-sunset);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.ladder__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

.ladder__hero-badge { align-self: flex-start; }

.ladder__loading { display: flex; flex-direction: column; gap: var(--space-md); }

.ladder__actions {
  display: flex;
  justify-content: space-between;
  gap: var(--space-sm);
  padding-top: var(--space-md);
  border-top: 1px solid var(--color-border);
}
</style>
