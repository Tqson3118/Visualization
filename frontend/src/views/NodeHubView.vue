<script setup lang="ts">
// NodeHubView — Màn 31: 3 tab (Lý thuyết / Luyện tập / Cheatsheet) — mỗi tab 1 component tách
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { useLessonStore } from '@/stores/lesson';
import { useUiStore } from '@/stores/ui';
import { CATALOG, getCatalogMeta } from '@/engines/catalog';
import { TOPIC_NODE_LESSONS } from '@/data/nodeHubData';
import LessonDetail from '@/components/lesson/LessonDetail.vue';
import LadderShell from '@/components/ladder/LadderShell.vue';
import CheatSheetTable from '@/components/lesson/CheatSheetTable.vue';
import Button from '@/components/ui/Button.vue';

const route = useRoute();
const router = useRouter();
const lessonStore = useLessonStore();
const ui = useUiStore();

const topicId = computed(() => String(route.params.topicId ?? ''));
const nodeId = computed(() => Number(route.params.nodeId ?? 0));

const tab = ref<'theory' | 'practice' | 'cheatsheet'>('theory');

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

onMounted(async () => {
  if (lessonId.value !== null) {
    try {
      await lessonStore.fetchLesson(lessonId.value);
    } catch {
      ui.showToast('Không thể tải lý thuyết — hiển thị nội dung mẫu.', 'warning');
    }
  }
});

function openSimulation(key: string): void {
  void router.push({ name: 'simulator', params: { key } });
}
</script>

<template>
  <main class="node-hub container">
    <nav class="node-hub__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'path-topic', params: { topicId } }">Lộ trình</RouterLink>
      <span aria-hidden="true">/</span>
      <span>{{ nodeTitle }}</span>
    </nav>

    <header class="node-hub__header">
      <div>
        <h1 class="node-hub__title">{{ nodeTitle }}</h1>
        <p class="text-muted node-hub__sub">Node {{ nodeId }} — điểm vào duy nhất cho luồng học của node này.</p>
      </div>
      <Button variant="ghost" size="sm" @click="router.push({ name: 'path-topic', params: { topicId } })">
        ← Về bản đồ
      </Button>
    </header>

    <div class="node-hub__tabs">
      <button
        type="button"
        class="node-hub__tab"
        :class="{ 'node-hub__tab--active': tab === 'theory' }"
        @click="tab = 'theory'"
      >
        Lý thuyết
      </button>
      <button
        type="button"
        class="node-hub__tab"
        :class="{ 'node-hub__tab--active': tab === 'practice' }"
        @click="tab = 'practice'"
      >
        Luyện tập (Ladder)
      </button>
      <button
        type="button"
        class="node-hub__tab"
        :class="{ 'node-hub__tab--active': tab === 'cheatsheet' }"
        @click="tab = 'cheatsheet'"
      >
        Cheatsheet
      </button>
    </div>

    <section v-if="tab === 'theory'" class="node-hub__panel">
      <LessonDetail
        v-if="lessonId !== null"
        :lesson-id="lessonId"
        @open-simulation="openSimulation"
      />
      <div v-else class="node-hub__fallback card">
        <h2 class="node-hub__fallback-title">📖 Lý thuyết — {{ nodeTitle }}</h2>
        <p class="node-hub__fallback-text">
          Node này chưa được gắn bài học lý thuyết trên backend. Hãy mở mô phỏng
          <button type="button" class="node-hub__link" @click="openSimulation(simKey)">
            {{ simKey }}
          </button>
          để xem thuật toán chạy từng bước, hoặc xem tab Cheatsheet để tra độ phức tạp.
        </p>
        <Button size="sm" variant="secondary" @click="openSimulation(simKey)">
          ▶ Mở mô phỏng {{ simKey }}
        </Button>
      </div>
    </section>

    <section v-else-if="tab === 'practice'" class="node-hub__panel">
      <LadderShell
        :node-id="nodeId"
        :quiz-exercise="null"
        :simulation-key="simKey"
        :code-exercise-id="null"
      />
    </section>

    <section v-else class="node-hub__panel">
      <CheatSheetTable @open-simulation="openSimulation" />
    </section>
  </main>
</template>

<style scoped>
.node-hub {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.node-hub__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.node-hub__header {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  align-items: flex-start;
  flex-wrap: wrap;
}

.node-hub__title { font-size: var(--text-xl); }
.node-hub__sub { font-size: var(--text-sm); margin-top: 4px; }

.node-hub__tabs {
  display: flex;
  gap: var(--space-xs);
  border-bottom: 2px solid var(--color-border);
  overflow-x: auto;
}

.node-hub__tab {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  padding: var(--space-sm) var(--space-md);
  font-weight: 700;
  font-size: var(--text-sm);
  color: var(--color-text-muted);
  cursor: pointer;
  white-space: nowrap;
  margin-bottom: -2px;
}

.node-hub__tab--active { color: var(--color-primary); border-bottom-color: var(--color-primary); }

.node-hub__fallback { display: flex; flex-direction: column; gap: var(--space-md); align-items: flex-start; }

.node-hub__fallback-text { font-size: var(--text-sm); }

.node-hub__link {
  background: none;
  border: none;
  color: var(--color-primary);
  font-weight: 700;
  cursor: pointer;
  font-family: var(--font-mono);
  font-size: inherit;
  text-decoration: underline;
}
</style>
