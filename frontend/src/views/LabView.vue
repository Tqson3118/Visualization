<script setup lang="ts">
// LabView — Màn 15: Interactive Lab (Bậc 2) tại /ladder/:nodeId/lab
// Canvas editable + chấm trạng thái cuối + giới hạn bước ×1.5 + nộp labAnswer
// G-F2c: hero gradient mint nhẹ + 3 thẻ (Mô tả bài / Mục tiêu / Hướng dẫn) Card shadcn + icon lucide.
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { BookOpen, Lightbulb, Target } from 'lucide-vue-next';

import { getCatalogMeta } from '@/engines/catalog';
import LabStage from '@/components/ladder/LabStage.vue';
import Button from '@/components/ui/Button.vue';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

const route = useRoute();
const router = useRouter();

const nodeId = computed(() => String(route.params.nodeId ?? ''));

const simKey = computed(() => {
  const keys = ['sort.bubble', 'search.binary', 'graph.bfs', 'tree.bst-insert', 'stack.push', 'queue.enqueue', 'sort.merge', 'sort.quick'];
  const key = keys[(Number(nodeId.value) - 1) % keys.length];
  return getCatalogMeta(key) ? key : 'sort.bubble';
});

const title = computed(() => getCatalogMeta(simKey.value)?.title ?? `Node ${nodeId.value}`);

const INFO_CARDS = [
  {
    icon: BookOpen,
    title: 'Mô tả bài',
    text: 'Thao tác trực tiếp trên cấu trúc dữ liệu để hiểu cách thuật toán hoạt động từ bên trong — không cần viết code.',
  },
  {
    icon: Target,
    title: 'Mục tiêu',
    text: 'Đưa dãy về đúng trạng thái cuối theo chuẩn (sắp xếp tăng dần). Số bước dùng không vượt quá chuẩn × 1.5.',
  },
  {
    icon: Lightbulb,
    title: 'Hướng dẫn',
    text: 'Chọn ô thứ nhất rồi ô liền kề để hoán đổi. Hoàn tác / Làm lại không tính bước. Nộp bài để chấm trạng thái cuối.',
  },
];
</script>

<template>
  <main class="lab-view container">
    <nav class="lab-view__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'ladder', params: { nodeId } }">Ladder</RouterLink>
      <span aria-hidden="true">/</span>
      <span>Lab — {{ title }}</span>
    </nav>

    <!-- Thẻ mô tả / mục tiêu / hướng dẫn (SDD Màn 15) -->
    <div class="lab-view__info-grid">
      <Card
        v-for="info in INFO_CARDS"
        :key="info.title"
        class="lab-view__info-card"
      >
        <CardHeader class="lab-view__info-header">
          <span class="lab-view__info-icon" aria-hidden="true">
            <component :is="info.icon" :size="18" />
          </span>
          <CardTitle class="lab-view__info-title">{{ info.title }}</CardTitle>
        </CardHeader>
        <CardContent>
          <p class="lab-view__info-text">{{ info.text }}</p>
        </CardContent>
      </Card>
    </div>

    <LabStage
      :title="`Interactive Lab — ${title}`"
      :initial-array="[5, 3, 8, 1, 9, 2]"
      :standard-steps="8"
      @passed="router.push({ name: 'ladder', params: { nodeId } })"
      @view-theory="router.push({ name: 'node-hub', params: { topicId: '1', nodeId } })"
    />

    <div class="lab-view__actions">
      <Button variant="ghost" @click="router.push({ name: 'ladder', params: { nodeId } })">
        ← Về Ladder
      </Button>
    </div>
  </main>
</template>

<style scoped>
.lab-view {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.lab-view__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.lab-view__breadcrumb a { color: var(--color-primary); font-weight: 600; }

.lab-view__info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: var(--space-md);
}

.lab-view__info-card { transition: box-shadow 180ms ease, transform 180ms ease; }

.lab-view__info-card:hover {
  box-shadow: var(--shadow-md);
  transform: translateY(-2px);
}

.lab-view__info-header {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding-bottom: var(--space-sm);
}

.lab-view__info-icon {
  width: 34px;
  height: 34px;
  border-radius: var(--radius-md);
  background-image: var(--gradient-mint);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-sm);
}

.lab-view__info-title { font-size: var(--text-md); }

.lab-view__info-text { font-size: var(--text-sm); color: var(--color-text-muted); line-height: 1.6; }

.lab-view__actions {
  display: flex;
  justify-content: flex-start;
  padding-top: var(--space-md);
  border-top: 1px solid var(--color-border);
}
</style>
