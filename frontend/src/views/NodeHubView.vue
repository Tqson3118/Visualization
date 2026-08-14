<script setup lang="ts">
// NodeHubView — Màn 31: 3 tab (Lý thuyết / Luyện tập / Cheatsheet) — mỗi tab 1 component tách
// View-quality (Phase 2 bổ sung): banner gradient sunset + blob + overlay hack → surface band
// level-2 + kicker mono `NODE 04 · SORT.BUBBLE` (dữ liệu thật route/simKey — quyết định #1);
// easing chuẩn enter/exit; icon gradient + shadow → ô muted + lucide 20px tertiary; CTA sm→md;
// H1 48px/600/-0.03em; badge muted; bỏ 📖/▶ trong i18n. GIỮ NGUYÊN logic.
import { computed, onMounted, ref } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';
import { ArrowLeft, BookOpen, GraduationCap, Play } from 'lucide-vue-next';
import { Motion } from 'motion-v';

import { useLessonStore } from '@/stores/lesson';
import { useUiStore } from '@/stores/ui';
import * as exercisesApi from '@/api/exercises';
import type { ExerciseDto } from '@/api/exercises';
import { CATALOG, getCatalogMeta } from '@/engines/catalog';
import { TOPIC_NODE_LESSONS } from '@/data/nodeHubData';
import { buildSimOverviewHtml, escapeHtml } from '@/utils/simOverview';
import { messages } from '@/i18n/vi';
import LessonDetail from '@/components/lesson/LessonDetail.vue';
import LadderShell from '@/components/ladder/LadderShell.vue';
import CheatSheetTable from '@/components/lesson/CheatSheetTable.vue';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/Button.vue';
import Card from '@/components/ui/Card.vue';
import Tabs, { type TabItem } from '@/components/ui/Tabs.vue';
import ProseContent from '@/components/ui/ProseContent.vue';

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

/** Fallback lý thuyết: overview từ catalog meta + hướng dẫn (render qua ProseContent). */
const fallbackHtml = computed(() => {
  const overview = buildSimOverviewHtml(getCatalogMeta(simKey.value));
  const guide = `<p>${escapeHtml(messages.nodeHub.fallbackText)}</p>`;
  return overview ? `${overview}\n${guide}` : guide;
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
    <!-- Chrome header — surface band level-2 + kicker mono (DESIGN.md §1/§6, không gradient) -->
    <Motion
      as="header"
      class="node-hub__chrome"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.28, ease: [0.16, 1, 0.3, 1] }"
    >
      <nav class="node-hub__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'path-topic', params: { topicId } }">
          {{ messages.nodeHub.breadcrumbPath }}
        </RouterLink>
        <span aria-hidden="true">/</span>
        <span aria-current="page">{{ nodeTitle }}</span>
      </nav>

      <p class="node-hub__kicker">
        NODE {{ String(nodeId).padStart(2, '0') }} · <span class="font-mono">{{ simKey.toUpperCase() }}</span>
      </p>

      <div class="node-hub__hero">
        <span class="node-hub__icon" aria-hidden="true">
          <GraduationCap :size="20" />
        </span>
        <div class="node-hub__hero-title-wrap">
          <h1 class="node-hub__title">{{ nodeTitle }}</h1>
          <p class="node-hub__sub">{{ messages.nodeHub.subtitle(nodeId) }}</p>
        </div>
        <Badge variant="muted" class="node-hub__badge">
          {{ messages.nodeHub.badgeNode(nodeId) }}
        </Badge>
      </div>

      <div class="node-hub__hero-actions">
        <Button @click="openSimulation(simKey)">
          <Play :size="16" aria-hidden="true" />
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
            <ProseContent :content-html="fallbackHtml" />
            <Button @click="openSimulation(simKey)">
              <Play :size="16" aria-hidden="true" />
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
        <ArrowLeft :size="16" aria-hidden="true" />
        {{ messages.nodeHub.backToMap }}
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

/* ── Chrome header — surface band level-2 (§6): card-raised + border-subtle, KHÔNG shadow ── */
.node-hub__chrome {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-lg) var(--space-xl);
}

.node-hub__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}

.node-hub__breadcrumb a { color: var(--color-primary); font-weight: 600; }

.node-hub__kicker {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
  margin: 0;
}

.node-hub__hero {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.node-hub__icon {
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

.node-hub__hero-title-wrap {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  flex: 1;
  min-width: 220px;
}

.node-hub__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  line-height: 1.1;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
  margin: 0;
}

.node-hub__sub {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
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

/* ── Panel transition (easing chuẩn enter/exit — DESIGN.md §7) ── */
.hub-panel-enter-active {
  transition: opacity 200ms cubic-bezier(0.16, 1, 0.3, 1), transform 200ms cubic-bezier(0.16, 1, 0.3, 1);
}

.hub-panel-leave-active {
  transition: opacity 150ms cubic-bezier(0.7, 0, 0.84, 0), transform 150ms cubic-bezier(0.7, 0, 0.84, 0);
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
  background: var(--color-muted);
  color: var(--color-text-tertiary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.node-hub__fallback-title { font-size: var(--text-xl); font-weight: 600; letter-spacing: -0.015em; margin: 0; }

/* ── Actions (ngoài chrome — nền trang) ── */
.node-hub__actions {
  display: flex;
  justify-content: flex-end;
  padding-top: var(--space-md);
  border-top: 1px solid var(--color-border);
}

@media (max-width: 640px) {
  .node-hub__chrome { padding: var(--space-md); }
  .node-hub__badge { margin-left: 0; }
  .node-hub__hero { align-items: flex-start; }
  .node-hub__actions { justify-content: flex-start; }
}
</style>
