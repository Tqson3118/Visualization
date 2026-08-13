<script setup lang="ts">
// NodeHubView — Màn 31: 3 tab (Lý thuyết / Luyện tập / Cheatsheet) — mỗi tab 1 component tách
// H-E3: chrome hero gradient Sunset (palette học tập, đồng bộ LessonView/LadderView) + Tabs shadcn
// (LessonView pattern) + micro-interaction (Motion hero, Transition panel) + i18n.
// GIỮ NGUYÊN logic: lessonId/simKey/exercise ladder/openSimulation/openExercise.
import { computed, onMounted, ref } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';
import { BookOpen, GraduationCap, Play } from 'lucide-vue-next';
import { Motion } from 'motion-v';

import { useLessonStore } from '@/stores/lesson';
import { useUiStore } from '@/stores/ui';
import * as exercisesApi from '@/api/exercises';
import type { ExerciseDto } from '@/api/exercises';
import { CATALOG, getCatalogMeta } from '@/engines/catalog';
import { TOPIC_NODE_LESSONS } from '@/data/nodeHubData';
import { messages } from '@/i18n/vi';
import LessonDetail from '@/components/lesson/LessonDetail.vue';
import LadderShell from '@/components/ladder/LadderShell.vue';
import CheatSheetTable from '@/components/lesson/CheatSheetTable.vue';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/Button.vue';
import Card from '@/components/ui/Card.vue';
import Tabs, { type TabItem } from '@/components/ui/Tabs.vue';

const route = useRoute();
const router = useRouter();
const lessonStore = useLessonStore();
const ui = useUiStore();

const topicId = computed(() => String(route.params.topicId ?? ''));
const nodeId = computed(() => Number(route.params.nodeId ?? 0));

const tab = ref<'theory' | 'practice' | 'cheatsheet'>('theory');

const TABS: TabItem[] = [
  { key: 'theory', label: messages.nodeHub.tabTheory },
  { key: 'practice', label: messages.nodeHub.tabPractice },
  { key: 'cheatsheet', label: messages.nodeHub.tabCheatsheet },
];

/** Node → bài học lý thuyết (map cục bộ topic×node) — fallback khi backend chưa gắn lesson */
const lessonId = computed(() => {
  const map = TOPIC_NODE_LESSONS[Number(topicId.value)]?.[nodeId.value];
  return map ?? null;
});

const simKey = computed(() => {
  const keysByTopic: Record<number, string[]> = {
    1: ['sort.bubble', 'sort.selection', 'sort.insertion', 'sort.merge', 'sort.quick', 'sort.heap', 'search.linear', 'search.binary'],
    2: ['structure.array', 'stack.push', 'stack.pop', 'queue.enqueue', 'queue.dequeue', 'list.insert', 'list.delete', 'structure.linkedlist'],
    3: ['structure.binarytree', 'tree.bst-insert', 'tree.bst-search', 'tree.bst-inorder', 'tree.avl-insert', 'heap.insert', 'heap.heapify'],
    4: ['structure.hashtable', 'hash.insert', 'hash.search', 'hash.delete'],
    5: ['structure.graph', 'graph.bfs', 'graph.dfs', 'graph.dijkstra'],
  };
  const keys = keysByTopic[Number(topicId.value)] ?? [];
  const key = keys[nodeId.value - 1];
  return key && getCatalogMeta(key) ? key : 'sort.bubble';
});

const nodeTitle = computed(() => {
  const meta = CATALOG.find((c) => c.key === simKey.value);
  return meta?.title ?? `Node ${nodeId.value}`;
});

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
  if (lessonId.value !== null) {
    try {
      await lessonStore.fetchLesson(lessonId.value);
    } catch {
      ui.showToast('Không thể tải lý thuyết — hiển thị nội dung mẫu.', 'warning');
    }
  }
  await loadLadderExercises();
});

function openSimulation(key: string): void {
  void router.push({ name: 'simulator', params: { key } });
}

/** Nút "Làm bài" (LessonDetail.vue:154) — bug P1 #3: trước đây emit không ai lắng nghe */
function openExercise(id: number): void {
  void router.push({ name: 'exercise', params: { id: String(id) } });
}
</script>

<template>
  <main class="node-hub container">
    <nav class="node-hub__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'path-topic', params: { topicId } }">
        {{ messages.nodeHub.breadcrumbPath }}
      </RouterLink>
      <span aria-hidden="true">/</span>
      <span>{{ nodeTitle }}</span>
    </nav>

    <!-- Hero gradient Sunset (palette học tập, đồng bộ LessonView — chữ trắng AA) -->
    <Motion
      class="node-hub__hero"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.32, ease: 'easeOut' }"
    >
      <div class="node-hub__hero-body">
        <span class="node-hub__icon" aria-hidden="true">
          <GraduationCap :size="22" />
        </span>
        <div class="node-hub__hero-title-wrap">
          <h1 class="node-hub__title">{{ nodeTitle }}</h1>
          <p class="node-hub__sub">{{ messages.nodeHub.subtitle(nodeId) }}</p>
        </div>
        <Badge variant="primary" class="node-hub__badge">
          {{ messages.nodeHub.badgeNode(nodeId) }}
        </Badge>
      </div>

      <div class="node-hub__hero-actions">
        <Button size="sm" @click="openSimulation(simKey)">
          <Play :size="14" aria-hidden="true" />
          {{ messages.nodeHub.openSimulation }}
        </Button>
      </div>
    </Motion>

    <!-- Tabs: Lý thuyết / Luyện tập / Cheatsheet (Tabs shadcn — LessonView pattern) -->
    <Tabs v-model="tab" :tabs="TABS" class="node-hub__tabs">
      <Transition name="hub-panel" mode="out-in">
        <section v-if="tab === 'theory'" key="theory" class="node-hub__panel">
          <LessonDetail
            v-if="lessonId !== null"
            :lesson-id="lessonId"
            @open-simulation="openSimulation"
            @open-exercise="openExercise"
          />
          <Card v-else class="node-hub__fallback">
            <div class="node-hub__fallback-head">
              <span class="node-hub__fallback-icon" aria-hidden="true">
                <BookOpen :size="18" />
              </span>
              <h2 class="node-hub__fallback-title">{{ messages.nodeHub.fallbackTitle(nodeTitle) }}</h2>
            </div>
            <p class="node-hub__fallback-text">{{ messages.nodeHub.fallbackText }}</p>
            <Button size="sm" @click="openSimulation(simKey)">
              <Play :size="14" aria-hidden="true" />
              {{ messages.nodeHub.fallbackCta(simKey) }}
            </Button>
          </Card>
        </section>

        <section v-else-if="tab === 'practice'" key="practice" class="node-hub__panel">
          <LadderShell
            :node-id="nodeId"
            :quiz-exercise="quizExercise"
            :quiz-loading="quizLoading"
            :simulation-key="simKey"
            :code-exercise-id="codeExerciseId"
          />
        </section>

        <section v-else key="cheatsheet" class="node-hub__panel">
          <CheatSheetTable @open-simulation="openSimulation" />
        </section>
      </Transition>
    </Tabs>

    <div class="node-hub__actions">
      <Button variant="ghost" @click="router.push({ name: 'path-topic', params: { topicId } })">
        ← {{ messages.nodeHub.backToMap }}
      </Button>
    </div>
  </main>
</template>

<style scoped>
.node-hub {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Breadcrumb (ngoài hero — nền trang, link primary đủ AA) ── */
.node-hub__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.node-hub__breadcrumb a { color: var(--color-primary); font-weight: 600; }

/* ── Hero gradient — Sunset (palette 2 — amber → rose, chữ trắng AA) ── */
.node-hub__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border-radius: var(--radius-xl);
  background-image: var(--gradient-sunset);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-lg);
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
}

/* Điểm sáng trang trí (decorative) */
.node-hub__hero::before {
  content: '';
  position: absolute;
  width: 240px;
  height: 240px;
  border-radius: 50%;
  top: -110px;
  right: -50px;
  z-index: -1;
  background: color-mix(in srgb, var(--color-warning) 22%, transparent);
  filter: blur(56px);
}

/* GP-T9b (#12): dark mode gradient Sunset sáng (0.75-0.88) làm chữ trắng khó đọc
   → phủ lớp tối (0.72) để chữ trắng ≥ 4.5:1. */
.dark .node-hub__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: rgba(4, 47, 46, 0.72);
}

.node-hub__hero-body {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.node-hub__icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-lg);
  background-image: var(--gradient-sunset);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-md);
}

.node-hub__hero-title-wrap {
  display: flex;
  flex-direction: column;
  gap: 2px;
  flex: 1;
  min-width: 220px;
}

.node-hub__title {
  font-size: clamp(var(--text-2xl), 4vw, var(--text-3xl));
  color: #fff;
  margin: 0;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.16);
}

.node-hub__sub {
  font-size: var(--text-sm);
  color: rgba(255, 255, 255, 0.92);
  max-width: 64ch;
  margin: 0;
}

.node-hub__badge { margin-left: auto; align-self: flex-start; }

.node-hub__hero-actions {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  align-items: center;
  padding-top: var(--space-xs);
}

/* ── Tabs ── */
.node-hub__tabs { margin-top: var(--space-xs); }

.node-hub__panel { padding-top: var(--space-xs); }

/* ── Panel transition (micro-interaction nhẹ khi đổi tab) ── */
.hub-panel-enter-active,
.hub-panel-leave-active {
  transition: opacity 180ms ease, transform 180ms ease;
}

.hub-panel-enter-from {
  opacity: 0;
  transform: translateY(6px);
}

.hub-panel-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}

/* ── Fallback lý thuyết (node chưa gắn lesson) ── */
.node-hub__fallback {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  align-items: flex-start;
}

.node-hub__fallback-head {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.node-hub__fallback-icon {
  flex-shrink: 0;
  width: 38px;
  height: 38px;
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-primary) 12%, transparent);
  color: var(--color-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.node-hub__fallback-title { font-size: var(--text-md); }

.node-hub__fallback-text {
  font-size: var(--text-sm);
  color: var(--color-text-muted);
  max-width: 72ch;
}

/* ── Actions (ngoài hero — nền trang) ── */
.node-hub__actions {
  display: flex;
  justify-content: flex-end;
  padding-top: var(--space-md);
  border-top: 1px solid var(--color-border);
}

@media (max-width: 640px) {
  .node-hub__hero { padding: var(--space-md); }
  .node-hub__badge { margin-left: 0; }
  .node-hub__actions { justify-content: flex-start; }
}
</style>
