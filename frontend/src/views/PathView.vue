<script setup lang="ts">
// PathView — Màn 13: bản đồ node kiểu graph node-edge (/path/:topicId)
// Phase 1 view-quality: banner surface band level-2 (bỏ gradient aurora + emoji),
// map = VueFlow node-edge THẬT (lazy defineAsyncComponent — entry bundle không đổi),
// node = block-token (canvas-ink + index mono + trạng thái resolved/conflict) — DESIGN-IDENTITY §1.5.
// Dữ liệu: API /learning-path/{id}; fallback cục bộ (catalog + localStorage) khi backend chưa có.
import { computed, defineAsyncComponent, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { Heart, Route } from 'lucide-vue-next';

import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import * as gamificationApi from '@/api/gamification';
import type { LearningPathNodeDto } from '@/api/gamification';
import { CATALOG } from '@/engines/catalog';
import { allowLocalFallbacks } from '@/config/runtime';
import Button from '@/components/ui/Button.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

// Lazy-load graph (vue-flow) — CSS của @vue-flow/core nằm trong chunk PathGraph, không phình entry.
const PathGraph = defineAsyncComponent(() => import('@/components/path/PathGraph.vue'));

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
    if (!allowLocalFallbacks) {
      nodes.value = [];
      return;
    }
    nodes.value = allowLocalFallbacks ? applyLocalProgress(localNodes()) : [];
  } finally {
    loading.value = false;
  }
}

const progressPct = computed(() => {
  if (nodes.value.length === 0) return 0;
  return Math.round((nodes.value.filter((n) => n.status === 'passed').length / nodes.value.length) * 100);
});

const passedCount = computed(() => nodes.value.filter((n) => n.status === 'passed').length);

const finalTestUnlocked = computed(() => nodes.value.length > 0 && nodes.value.every((n) => n.status === 'passed'));

watch(topicId, () => void load(), { immediate: true });

onMounted(() => {
  if (gamification.heartsMax === 5 && gamification.hearts === 0) {
    void gamification.fetchHearts();
  }
});

async function startNode(node: LearningPathNodeDto): Promise<void> {
  if (node.status === 'locked' || enteringId.value !== null) return;
  enteringId.value = node.id;
  try {
    // Trừ tim (atomic server) — lỗi 403 HEARTS_EMPTY → modal hết tim (Màn 28)
    await gamification.enterNode(topicId.value, node.id);
    popoverNode.value = null;
    void router.push({ name: 'node-hub', params: { topicId: String(topicId.value), nodeId: String(node.id) } });
  } catch {
    // Fallback: không có backend → mở thẳng Node Hub (demo cục bộ không trừ tim)
    if (apiFailed.value && allowLocalFallbacks) {
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

function openFinalTest(): void {
  if (!finalTestUnlocked.value) return;
  void router.push({ name: 'final-test', params: { topicId: String(topicId.value) } });
}
</script>

<template>
  <main class="path-view container">
    <header class="path-view__hero">
      <div class="path-view__hero-main">
        <p class="path-view__hero-kicker">Learning Path · <span class="font-mono">TOPIC {{ String(topicId).padStart(2, '0') }}</span></p>
        <h1 class="path-view__title">
          <Route :size="28" aria-hidden="true" class="path-view__title-icon" />
          {{ topicMeta.name }}
        </h1>
        <p class="path-view__sub">{{ topicMeta.description }}</p>
      </div>
      <div class="path-view__progress">
        <p class="path-view__progress-label">Tiến độ tổng</p>
        <ProgressBar :value="progressPct" show-label :variant="progressPct >= 100 ? 'success' : 'default'" />
        <p class="path-view__progress-count">
          <span class="path-view__progress-num">{{ String(passedCount).padStart(2, '0') }}/{{ String(nodes.length).padStart(2, '0') }}</span>
          node đã qua
        </p>
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
      @action="router.push({ name: 'courses' })"
    />

    <template v-else>
      <!-- Bản đồ node dạng graph node-edge (VueFlow lazy) -->
      <PathGraph
        :nodes="nodes"
        :entering-id="enteringId"
        :final-test-unlocked="finalTestUnlocked"
        @select-node="popoverNode = $event"
        @select-final="openFinalTest"
      />

      <p v-if="apiFailed" class="path-view__note">
        * Backend chưa khả dụng — hiển thị lộ trình mẫu, tiến độ lưu trên thiết bị này.
      </p>

      <!-- Popover node -->
      <Teleport to="body">
        <Transition name="pop-fade">
          <div v-if="popoverNode" class="path-view__popover">
            <h3 class="path-view__popover-title">{{ popoverNode.title }}</h3>
            <p class="path-view__popover-desc">{{ popoverNode.description }}</p>
            <p class="path-view__popover-cost">
              <Heart :size="14" aria-hidden="true" />
              {{ popoverNode.status === 'passed' ? 'Miễn phí (đã qua)' : '1 tim — trừ khi vào node' }}
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

/* ── Banner surface band level-2 (DESIGN.md §1 + §6) — không gradient, không shadow ── */
.path-view__hero {
  width: 100%;
  display: flex;
  justify-content: space-between;
  gap: var(--space-lg);
  align-items: flex-start;
  flex-wrap: wrap;
  padding: var(--space-xl);
  border-radius: var(--radius-lg);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
}

.path-view__hero-main { display: flex; flex-direction: column; gap: var(--space-xs); }

.path-view__hero-kicker {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
}

.path-view__title {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-3xl);
  font-weight: 600;
  letter-spacing: -0.02em;
  margin: 0;
}

.path-view__title-icon { color: var(--color-text-tertiary); flex-shrink: 0; }

.path-view__sub {
  font-size: var(--text-sm);
  color: var(--color-text-muted);
  margin: 0;
}

.path-view__progress {
  width: min(300px, 100%);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  padding: var(--space-md);
  border-radius: var(--radius-lg);
  background: var(--color-card);
  border: 1px solid var(--color-border);
}

.path-view__progress-label {
  font-size: var(--text-xs);
  font-weight: 500;
  color: var(--color-text-tertiary);
}

.path-view__progress-count {
  display: flex;
  align-items: baseline;
  gap: var(--space-xs);
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  margin: 0;
}

.path-view__progress-num {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-foreground);
}

.path-view__note {
  font-size: var(--text-xs);
  text-align: center;
  color: var(--color-text-muted);
  margin: 0;
}

.path-view__loading { display: flex; flex-direction: column; gap: var(--space-md); }

/* ── Popover node (bottom sheet nhẹ — dropdown exception, shadow hợp lệ §6) ── */
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
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-lg);
  box-shadow: var(--shadow-xl);
}

.path-view__popover-title {
  font-size: var(--text-md);
  font-weight: 600;
  margin: 0;
}

.path-view__popover-desc {
  font-size: var(--text-sm);
  color: var(--color-text-muted);
  margin: 0;
}

.path-view__popover-cost {
  display: flex;
  align-items: center;
  gap: var(--space-xs);
  font-size: var(--text-xs);
  color: var(--color-warning);
  font-weight: 600;
  margin: 0;
}

.path-view__popover-actions { display: flex; justify-content: flex-end; gap: var(--space-sm); }

/* Easing chuẩn DESIGN.md §7: enter cubic-bezier(0.16,1,0.3,1) / exit (0.7,0,0.84,0), 200-250ms */
.pop-fade-enter-active {
  transition: opacity 200ms cubic-bezier(0.16, 1, 0.3, 1), transform 200ms cubic-bezier(0.16, 1, 0.3, 1);
}

.pop-fade-leave-active {
  transition: opacity 150ms cubic-bezier(0.7, 0, 0.84, 0), transform 150ms cubic-bezier(0.7, 0, 0.84, 0);
}

.pop-fade-enter-from, .pop-fade-leave-to { opacity: 0; transform: translate(-50%, 8px); }

@media (prefers-reduced-motion: reduce) {
  .pop-fade-enter-active, .pop-fade-leave-active { transition: none; }
}
</style>
