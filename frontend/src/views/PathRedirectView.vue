<script setup lang="ts">
// PathRedirectView — /path: danh sách lộ trình (5 topic) — Màn 13 selector
// Tải topics từ API; fallback cục bộ theo SEED_COURSES + engines/catalog khi API lỗi.
// P1-B3: banner surface band level-2 + kicker mono (bỏ 🎯), card → RouterLink
// (Enter/Space/focus native — bỏ role="button" tự chế), bỏ .card/.card--interactive
// (shadow-md + hover lift — vi phạm §6) → card level-1 hover border-color,
// index vòng tròn primary/800 → kicker mono "TOPIC 01/05", EmptyState icon book
// (icon "map" không tồn tại trong SVG_PATHS → fallback x-circle), stagger enter.
import { computed, onMounted, ref } from 'vue';

import { Route } from 'lucide-vue-next';

import * as gamificationApi from '@/api/gamification';
import type { LearningPathSummaryDto } from '@/api/gamification';
import { useProgressStore } from '@/stores/progress';
import { allowLocalFallbacks } from '@/config/runtime';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const progressStore = useProgressStore();

const paths = ref<LearningPathSummaryDto[]>([]);
const loading = ref(true);
const apiFailed = ref(false);

const LOCAL_TOPICS: Array<{ id: number; name: string; description: string }> = [
  { id: 1, name: 'Sắp xếp & Tìm kiếm', description: 'Bubble, Selection, Insertion, Merge, Quick, Heap + tìm kiếm tuyến tính/nhị phân' },
  { id: 2, name: 'CTDL tuyến tính', description: 'Ngăn xếp, hàng đợi, danh sách liên kết' },
  { id: 3, name: 'Cây', description: 'BST, AVL, duyệt cây, heap' },
  { id: 4, name: 'Bảng băm', description: 'Chèn, tìm kiếm, xóa — chuỗi nối kết' },
  { id: 5, name: 'Đồ thị', description: 'BFS, DFS, Dijkstra' },
];

onMounted(async () => {
  try {
    paths.value = await gamificationApi.fetchLearningPaths();
  } catch {
    apiFailed.value = true;
    paths.value = [];
  } finally {
    loading.value = false;
  }
  try {
    await progressStore.fetchOverview();
  } catch {
    /* tiến độ không bắt buộc */
  }
});

/** Chỉ hiện path ACTIVE (5 path cũ đã ẩn sau khi import Grokking — backend lọc IsActive). */
const displayTopics = computed(() => {
  if (paths.value.length > 0) {
    return paths.value.map((p) => ({ id: p.id, name: p.title, description: p.description, progressPct: p.progressPct }));
  }
  return allowLocalFallbacks
    ? LOCAL_TOPICS.map((t) => ({ id: t.id, name: t.name, description: t.description, progressPct: 0 }))
    : [];
});

function topicProgress(topicId: number): number {
  const topic = progressStore.overview?.topics.find((t) => t.id === topicId);
  return topic?.progressPct ?? displayTopics.value.find((t) => t.id === topicId)?.progressPct ?? 0;
}
</script>

<template>
  <main class="path-redirect container">
    <!-- Banner surface band level-2 + kicker mono (DESIGN.md §1, không gradient/emoji) -->
    <header class="path-redirect__chrome">
      <p class="path-redirect__kicker">Learning Path · Chọn chủ đề</p>
      <h1 class="path-redirect__title">
        <Route :size="28" aria-hidden="true" class="path-redirect__title-icon" />
        Lộ trình học
      </h1>
      <p class="path-redirect__sub">
        Chọn lộ trình theo chủ đề — hoàn thành node để mở khóa node kế tiếp.
      </p>
    </header>

    <div v-if="loading" class="path-redirect__loading" aria-hidden="true">
      <Skeleton v-for="i in 5" :key="i" height="150px" />
    </div>

    <EmptyState
      v-else-if="displayTopics.length === 0"
      icon="book"
      title="Chưa có lộ trình"
      description="Lộ trình đang được biên soạn — quay lại sau để bắt đầu học nhé."
    />

    <div v-else class="path-redirect__grid">
      <RouterLink
        v-for="(topic, idx) in displayTopics"
        :key="topic.id"
        :to="{ name: 'path-topic', params: { topicId: String(topic.id) } }"
        class="path-redirect__card"
        :style="{ '--card-i': Math.min(idx, 8) }"
      >
        <span class="path-redirect__card-index" aria-hidden="true">
          TOPIC {{ String(idx + 1).padStart(2, '0') }}/{{ String(displayTopics.length).padStart(2, '0') }}
        </span>
        <h2 class="path-redirect__card-title">{{ topic.name }}</h2>
        <p class="path-redirect__card-desc">{{ topic.description }}</p>
        <ProgressBar
          :value="topicProgress(topic.id)"
          show-label
          :variant="topicProgress(topic.id) >= 100 ? 'success' : 'default'"
        />
        <p v-if="apiFailed" class="path-redirect__note">
          * Đang kết nối máy chủ để cập nhật tiến độ lộ trình học tập.
        </p>
      </RouterLink>
    </div>
  </main>
</template>

<style scoped>
.path-redirect {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Banner surface band level-2 (DESIGN.md §1 + §6) — không gradient, không shadow ── */
.path-redirect__chrome {
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-lg) var(--space-xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.path-redirect__kicker {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
}

.path-redirect__title {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-3xl);
  font-weight: 600;
  letter-spacing: -0.02em;
  line-height: 1.15;
  margin: 0;
}

.path-redirect__title-icon { color: var(--color-text-tertiary); flex-shrink: 0; }

.path-redirect__sub { font-size: var(--text-sm); color: var(--color-text-secondary); margin: 0; }

.path-redirect__loading { display: flex; flex-direction: column; gap: var(--space-md); }

.path-redirect__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: var(--space-md);
}

/* ── Card topic = link level-1 (bỏ .card shadow-md + hover lift — §6) ── */
.path-redirect__card {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-lg);
  text-decoration: none;
  color: inherit;
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
  /* Khoảnh khắc đầu tư: stagger enter (max 8 × 40ms) — easing chuẩn §7 */
  animation: card-enter 240ms cubic-bezier(0.16, 1, 0.3, 1) both;
  animation-delay: calc(var(--card-i, 0) * 40ms);
}

.path-redirect__card:hover { border-color: var(--color-border-strong); }

@keyframes card-enter {
  from { opacity: 0; transform: translateY(8px); }
  to { opacity: 1; transform: translateY(0); }
}

@media (prefers-reduced-motion: reduce) {
  .path-redirect__card { animation: none; }
}

/* Index mono (signature "index dưới block" — DESIGN-IDENTITY §1.5) */
.path-redirect__card-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
}

.path-redirect__card-title {
  font-size: var(--text-lg);
  font-weight: 600;
  letter-spacing: -0.015em;
  color: var(--color-text-primary);
  margin: 0;
}

.path-redirect__card-desc { font-size: var(--text-sm); color: var(--color-text-secondary); flex: 1; margin: 0; }

.path-redirect__note { font-size: var(--text-xs); color: var(--color-text-tertiary); margin: 0; }
</style>
