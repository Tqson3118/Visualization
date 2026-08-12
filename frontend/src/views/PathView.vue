<script setup lang="ts">
// PathView — Màn 13: bản đồ node Duolingo-style (/path/:topicId)
// Dữ liệu: API /learning-path/{id}; fallback cục bộ (catalog + localStorage) khi backend chưa có.
import { computed, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import * as gamificationApi from '@/api/gamification';
import type { LearningPathNodeDto } from '@/api/gamification';
import { CATALOG } from '@/engines/catalog';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/Button.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const route = useRoute();
const router = useRouter();
const gamification = useGamificationStore();
const ui = useUiStore();

const topicId = computed(() => Number(route.params.topicId ?? 0));
const nodes = ref<LearningPathNodeDto[]>([]);
const loading = ref(true);
const apiFailed = ref(false);
const enteringId = ref<number | null>(null);
const popoverNode = ref<LearningPathNodeDto | null>(null);

const LOCAL_KEY = 'dsa-path-progress';

// ── Dữ liệu cục bộ theo topic (fallback) ──
const TOPIC_META: Record<number, { name: string; description: string }> = {
  1: { name: 'Sắp xếp & Tìm kiếm', description: 'Các thuật toán sắp xếp và tìm kiếm trên mảng' },
  2: { name: 'CTDL tuyến tính', description: 'Ngăn xếp, hàng đợi, danh sách liên kết' },
  3: { name: 'Cây', description: 'Cây nhị phân, BST, AVL, heap' },
  4: { name: 'Bảng băm', description: 'Bảng băm — chuỗi nối kết' },
  5: { name: 'Đồ thị', description: 'BFS, DFS, Dijkstra' },
};

const TOPIC_KEYS: Record<number, string[]> = {
  1: ['sort.bubble', 'sort.selection', 'sort.insertion', 'sort.merge', 'sort.quick', 'sort.heap', 'search.linear', 'search.binary'],
  2: ['structure.array', 'stack.push', 'stack.pop', 'queue.enqueue', 'queue.dequeue', 'list.insert', 'list.delete', 'structure.linkedlist'],
  3: ['structure.binarytree', 'tree.bst-insert', 'tree.bst-search', 'tree.bst-inorder', 'tree.avl-insert', 'heap.insert', 'heap.heapify'],
  4: ['structure.hashtable', 'hash.insert', 'hash.search', 'hash.delete'],
  5: ['structure.graph', 'graph.bfs', 'graph.dfs', 'graph.dijkstra'],
};

const topicMeta = computed(() => TOPIC_META[topicId.value] ?? { name: 'Lộ trình', description: '' });

function localNodes(): LearningPathNodeDto[] {
  const keys = TOPIC_KEYS[topicId.value] ?? [];
  return keys.map((key, idx) => {
    const meta = CATALOG.find((c) => c.key === key);
    return {
      id: idx + 1,
      title: meta?.title ?? key,
      description: meta ? `Độ phức tạp: ${meta.complexity.average} · Không gian: ${meta.complexity.space}` : '',
      sortOrder: idx + 1,
      status: 'active',
      stars: 0,
      bestScore: null,
      lessonId: null,
      simulationKey: key,
      exerciseId: null,
      requiredStages: { quiz: true, lab: true, code: false },
    };
  });
}

/** Trạng thái node: passed nếu trong localStorage (fallback) hoặc API trả passed */
function applyLocalProgress(list: LearningPathNodeDto[]): LearningPathNodeDto[] {
  let passedIds: number[] = [];
  try {
    passedIds = JSON.parse(localStorage.getItem(LOCAL_KEY) ?? '[]') as number[];
  } catch {
    passedIds = [];
  }
  let activeAssigned = false;
  return list.map((node, idx) => {
    if (node.status === 'passed' || passedIds.includes(node.id)) return { ...node, status: 'passed' };
    if (node.status === 'active') return node;
    if (!activeAssigned && (idx === 0 || list[idx - 1].status === 'passed' || passedIds.includes(list[idx - 1].id))) {
      activeAssigned = true;
      return { ...node, status: 'active' };
    }
    return node;
  });
}

async function load(): Promise<void> {
  loading.value = true;
  try {
    const path = await gamificationApi.fetchLearningPath(topicId.value);
    nodes.value = applyLocalProgress(path.nodes);
  } catch {
    apiFailed.value = true;
    nodes.value = applyLocalProgress(localNodes());
  } finally {
    loading.value = false;
  }
}

const progressPct = computed(() => {
  if (nodes.value.length === 0) return 0;
  return Math.round((nodes.value.filter((n) => n.status === 'passed').length / nodes.value.length) * 100);
});

const finalTestUnlocked = computed(() => nodes.value.length > 0 && nodes.value.every((n) => n.status === 'passed'));

watch(topicId, () => void load(), { immediate: true });

onMounted(() => {
  if (gamification.heartsMax === 5 && gamification.hearts === 0) {
    void gamification.fetchHearts();
  }
});

async function startNode(node: LearningPathNodeDto): Promise<void> {
  if (node.status === 'locked') return;
  enteringId.value = node.id;
  try {
    // Trừ tim (atomic server) — lỗi 403 HEARTS_EMPTY → modal hết tim (Màn 28)
    await gamification.enterNode(topicId.value, node.id);
    popoverNode.value = null;
    void router.push({ name: 'node-hub', params: { topicId: String(topicId.value), nodeId: String(node.id) } });
  } catch {
    // Fallback: không có backend → mở thẳng Node Hub (demo cục bộ không trừ tim)
    if (apiFailed.value) {
      markLocalPassed(node.id);
      void router.push({ name: 'node-hub', params: { topicId: String(topicId.value), nodeId: String(node.id) } });
    } else {
      ui.showToast('Bạn đã hết tim. Hãy chờ hồi hoặc nâng cấp Premium.', 'warning');
    }
  } finally {
    enteringId.value = null;
  }
}

function markLocalPassed(nodeId: number): void {
  try {
    const list = JSON.parse(localStorage.getItem(LOCAL_KEY) ?? '[]') as number[];
    if (!list.includes(nodeId)) {
      localStorage.setItem(LOCAL_KEY, JSON.stringify([...list, nodeId]));
      nodes.value = nodes.value.map((n) => (n.id === nodeId ? { ...n, status: 'passed' } : n));
    }
  } catch {
    /* bỏ qua */
  }
}

const starLabel = computed(() => (count: number) => (count > 0 ? '⭐'.repeat(Math.min(3, count)) : ''));
</script>

<template>
  <main class="path-view container">
    <header class="path-view__header">
      <div class="path-view__hero">
        <div class="path-view__hero-main">
          <p class="path-view__hero-kicker">Learning Path</p>
          <h1 class="path-view__title">🎯 {{ topicMeta.name }}</h1>
          <p class="text-muted path-view__sub">{{ topicMeta.description }}</p>
        </div>
        <div class="path-view__progress">
          <p class="path-view__progress-label">Tiến độ tổng</p>
          <ProgressBar :value="progressPct" show-label :variant="progressPct >= 100 ? 'success' : 'default'" />
          <p class="path-view__progress-count text-muted">
            {{ nodes.filter((n) => n.status === 'passed').length }}/{{ nodes.length }} node đã qua
          </p>
        </div>
      </div>
    </header>

    <div v-if="loading" class="path-view__loading">
      <Skeleton v-for="i in 6" :key="i" height="64px" />
    </div>

    <EmptyState
      v-else-if="nodes.length === 0"
      icon="map"
      title="Lộ trình chưa có node"
      description="Quay lại sau khi nội dung được biên soạn."
      action-label="Về danh sách lộ trình"
      @action="router.push({ name: 'path' })"
    />

    <template v-else>
      <!-- Bản đồ node dạng đường mòn -->
      <div class="path-view__map">
        <template v-for="(node, idx) in nodes" :key="node.id">
          <div class="path-view__connector" :class="{ 'path-view__connector--done': node.status === 'passed' }" />
          <button
            type="button"
            class="path-view__node"
            :class="{
              'path-view__node--active': node.status === 'active',
              'path-view__node--passed': node.status === 'passed',
              'path-view__node--locked': node.status === 'locked',
              'hover-lift': node.status !== 'locked',
            }"
            :disabled="node.status === 'locked'"
            :aria-label="node.title"
            @click="popoverNode = node"
          >
            <span class="path-view__node-icon" aria-hidden="true">
              {{ node.status === 'locked' ? '🔒' : node.status === 'passed' ? starLabel(node.stars) || '⭐' : '▶' }}
            </span>
            <span class="path-view__node-label">{{ idx + 1 }}. {{ node.title }}</span>
            <Badge
              class="path-view__node-badge"
              :variant="node.status === 'passed' ? 'success' : node.status === 'active' ? 'primary' : 'muted'"
            >
              {{ node.status === 'passed' ? 'Đã qua' : node.status === 'active' ? 'Đang học' : 'Khóa' }}
            </Badge>
          </button>
        </template>

        <!-- Final test -->
        <div class="path-view__connector" :class="{ 'path-view__connector--done': finalTestUnlocked }" />
        <button
          type="button"
          class="path-view__node path-view__node--final"
          :class="{ 'path-view__node--locked': !finalTestUnlocked, 'hover-lift': finalTestUnlocked }"
          :disabled="!finalTestUnlocked"
          :title="finalTestUnlocked ? 'Mở kiểm tra cuối lộ trình' : 'Hoàn thành toàn bộ node để mở'"
          @click="router.push({ name: 'final-test', params: { topicId: String(topicId) } })"
        >
          <span class="path-view__node-icon" aria-hidden="true">🏁</span>
          <span class="path-view__node-label">Kiểm tra cuối lộ trình</span>
          <Badge :variant="finalTestUnlocked ? 'success' : 'muted'" class="path-view__node-badge">
            {{ finalTestUnlocked ? 'Mở được' : 'Khóa' }}
          </Badge>
        </button>
      </div>

      <p v-if="apiFailed" class="path-view__note text-muted">
        * Backend chưa khả dụng — hiển thị lộ trình mẫu, tiến độ lưu trên thiết bị này.
      </p>

      <!-- Popover node -->
      <Teleport to="body">
        <Transition name="pop-fade">
          <div v-if="popoverNode" class="path-view__popover card">
            <h3 class="path-view__popover-title">{{ popoverNode.title }}</h3>
            <p class="path-view__popover-desc text-muted">{{ popoverNode.description }}</p>
            <p class="path-view__popover-cost">
              {{ popoverNode.status === 'passed' ? 'Miễn phí (đã pass)' : '❤ 1 — trừ 1 tim khi vào node' }}
            </p>
            <div class="path-view__popover-actions">
              <Button
                variant="ghost"
                size="sm"
                @click="popoverNode = null"
              >
                Hủy
              </Button>
              <Button
                v-if="popoverNode.status !== 'locked'"
                size="sm"
                :loading="enteringId === popoverNode.id"
                @click="startNode(popoverNode)"
              >
                Bắt đầu
              </Button>
            </div>
          </div>
        </Transition>
      </Teleport>
    </template>
  </main>
</template>

<style scoped>
.path-view {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.path-view__header { display: flex; }

/* ── Hero gradient nhẹ (G-F2b) ── */
.path-view__hero {
  position: relative;
  overflow: hidden;
  isolation: isolate;
  width: 100%;
  display: flex;
  justify-content: space-between;
  gap: var(--space-lg);
  align-items: flex-start;
  flex-wrap: wrap;
  padding: var(--space-xl);
  border-radius: var(--radius-xl);
  background-color: var(--aurora-soft);
  border: 1px solid var(--color-border);
  box-shadow: var(--shadow-md);
}

.path-view__hero::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  height: 4px;
  z-index: -1;
  background-image: var(--gradient-aurora);
}

.path-view__hero-main { display: flex; flex-direction: column; gap: 4px; }

.path-view__hero-kicker {
  font-size: var(--text-xs);
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: var(--color-primary);
}

.path-view__title { font-size: var(--text-2xl); }
.path-view__sub { font-size: var(--text-sm); }

.path-view__progress {
  width: min(300px, 100%);
  display: flex;
  flex-direction: column;
  gap: 6px;
  padding: var(--space-md);
  border-radius: var(--radius-lg);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
}

.path-view__progress-label {
  font-size: var(--text-xs);
  font-weight: 700;
  color: var(--color-text-muted);
  text-transform: uppercase;
  letter-spacing: 0.06em;
}

.path-view__progress-count { font-size: var(--text-xs); }

.path-view__map {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
  padding-block: var(--space-md);
}

.path-view__connector {
  width: 3px;
  height: 28px;
  background: var(--color-border);
}

.path-view__connector--done { background: var(--color-success); }

.path-view__node {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-lg);
  border-radius: var(--radius-full);
  border: 2px solid var(--color-border);
  background: var(--color-surface);
  cursor: pointer;
  max-width: 100%;
}

.path-view__node--active {
  border-color: var(--color-primary);
  background: color-mix(in srgb, var(--color-primary) 8%, transparent);
  box-shadow: var(--shadow-md);
}

.path-view__node--passed { border-color: var(--color-success); }

.path-view__node--locked { opacity: 0.55; cursor: not-allowed; }

.path-view__node-icon { font-size: var(--text-md); flex-shrink: 0; }

.path-view__node-label { font-weight: 700; font-size: var(--text-sm); }

.path-view__node-badge { flex-shrink: 0; }

.path-view__node--final { border-style: dashed; }

.path-view__note { font-size: var(--text-xs); text-align: center; }

.path-view__loading { display: flex; flex-direction: column; gap: var(--space-md); }

.path-view__popover {
  position: fixed;
  left: 50%;
  bottom: 10vh;
  transform: translateX(-50%);
  z-index: var(--z-raised);
  width: min(360px, calc(100vw - 32px));
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  box-shadow: var(--shadow-xl);
}

.path-view__popover-title { font-size: var(--text-md); }
.path-view__popover-desc { font-size: var(--text-sm); }
.path-view__popover-cost { font-size: var(--text-xs); color: var(--color-warning); font-weight: 700; }

.path-view__popover-actions { display: flex; justify-content: flex-end; gap: var(--space-sm); }

.pop-fade-enter-active, .pop-fade-leave-active { transition: opacity 200ms ease, transform 200ms ease; }
.pop-fade-enter-from, .pop-fade-leave-to { opacity: 0; transform: translate(-50%, 8px); }
</style>
