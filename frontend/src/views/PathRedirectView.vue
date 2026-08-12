<script setup lang="ts">
// PathRedirectView — /path: danh sách lộ trình (5 topic) — Màn 13 selector
// Tải topics từ API; fallback cục bộ theo SEED_COURSES + engines/catalog khi API lỗi.
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import * as lessonsApi from '@/api/lessons';
import type { Topic } from '@/api/lessons';
import { SEED_COURSES } from '@/data/courses';
import { useProgressStore } from '@/stores/progress';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const router = useRouter();
const progressStore = useProgressStore();

const topics = ref<Topic[]>([]);
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
    topics.value = await lessonsApi.fetchTopics();
  } catch {
    apiFailed.value = true;
    topics.value = [];
  } finally {
    loading.value = false;
  }
  try {
    await progressStore.fetchOverview();
  } catch {
    /* tiến độ không bắt buộc */
  }
});

const displayTopics = computed(() => {
  if (topics.value.length > 0) {
    return topics.value.map((t) => ({ id: t.id, name: t.name, description: t.description }));
  }
  return LOCAL_TOPICS;
});

function topicProgress(topicId: number): number {
  const topic = progressStore.overview?.topics.find((t) => t.id === topicId);
  return topic?.progressPct ?? 0;
}

function openTopic(id: number): void {
  void router.push({ name: 'path-topic', params: { topicId: String(id) } });
}
</script>

<template>
  <main class="path-redirect container">
    <header class="path-redirect__header">
      <h1 class="path-redirect__title">🎯 Lộ trình học</h1>
      <p class="text-muted path-redirect__sub">
        Chọn lộ trình theo chủ đề — hoàn thành node để mở khóa node kế tiếp.
      </p>
    </header>

    <div v-if="loading" class="path-redirect__loading">
      <Skeleton v-for="i in 5" :key="i" height="88px" />
    </div>

    <EmptyState
      v-else-if="displayTopics.length === 0"
      icon="map"
      title="Chưa có lộ trình"
      description="Lộ trình đang được biên soạn — quay lại sau nhé."
    />

    <div v-else class="path-redirect__grid">
      <article
        v-for="(topic, idx) in displayTopics"
        :key="topic.id"
        class="path-redirect__card card card--interactive"
        role="button"
        tabindex="0"
        @click="openTopic(topic.id)"
        @keydown.enter="openTopic(topic.id)"
      >
        <div class="path-redirect__card-head">
          <span class="path-redirect__card-index">{{ idx + 1 }}</span>
          <h2 class="path-redirect__card-title">{{ topic.name }}</h2>
        </div>
        <p class="path-redirect__card-desc text-muted">{{ topic.description }}</p>
        <ProgressBar
          :value="topicProgress(topic.id)"
          show-label
          :variant="topicProgress(topic.id) >= 100 ? 'success' : 'default'"
        />
        <p v-if="apiFailed" class="path-redirect__note text-muted">
          * Hiển thị dữ liệu mẫu cục bộ (backend chưa khả dụng).
        </p>
      </article>
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

.path-redirect__title { font-size: var(--text-2xl); }
.path-redirect__sub { font-size: var(--text-sm); margin-top: 4px; }

.path-redirect__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: var(--space-md);
}

.path-redirect__card {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  cursor: pointer;
}

.path-redirect__card-head { display: flex; align-items: center; gap: var(--space-sm); }

.path-redirect__card-index {
  width: 32px;
  height: 32px;
  border-radius: 50%;
  background: var(--color-primary);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: var(--text-sm);
  flex-shrink: 0;
}

.path-redirect__card-title { font-size: var(--text-md); }

.path-redirect__card-desc { font-size: var(--text-sm); flex: 1; }

.path-redirect__note { font-size: var(--text-xs); }
</style>
