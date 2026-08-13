<script setup lang="ts">
// LabView — Màn 15: Interactive Lab (Bậc 2) tại /ladder/:nodeId/lab
// Canvas editable + chấm trạng thái cuối + giới hạn bước ×1.5 + nộp labAnswer
// P1-B2: banner surface band level-2 (không gradient), info card hover border-color,
// icon lucide, breadcrumb mono, ArrowLeft. LabStage (component con) xử lý canvas tối.
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { ArrowLeft, BookOpen, Lightbulb, Target } from 'lucide-vue-next';

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
    <!-- Banner — surface band level-2 + kicker mono (DESIGN.md §1, không gradient) -->
    <header class="lab-view__banner">
      <nav class="lab-view__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'ladder', params: { nodeId } }">Ladder</RouterLink>
        <span aria-hidden="true">/</span>
        <span>Lab — {{ title }}</span>
      </nav>
      <p class="lab-view__kicker">INTERACTIVE LAB · NODE {{ nodeId }}</p>
      <h1 class="lab-view__title">Thao tác dữ liệu thật</h1>
      <p class="lab-view__sub">
        Hoán đổi các ô liền kề tới khi dãy đúng trạng thái cuối — số bước giới hạn
        {{ 'chuẩn × 1.5' }} như trong đề bài.
      </p>
    </header>

    <!-- Thẻ mô tả / mục tiêu / hướng dẫn (SDD Màn 15) -->
    <div class="lab-view__info-grid">
      <Card
        v-for="info in INFO_CARDS"
        :key="info.title"
        class="lab-view__info-card"
      >
        <CardHeader class="lab-view__info-header">
          <span class="lab-view__info-icon" aria-hidden="true">
            <component :is="info.icon" :size="16" />
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
        <ArrowLeft :size="16" aria-hidden="true" />
        Về Ladder
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

/* Banner — surface band level-2 + luminance stacking (DESIGN.md §1 + §6) */
.lab-view__banner {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-lg) var(--space-xl);
}

.lab-view__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}

.lab-view__breadcrumb a { color: var(--color-primary); font-weight: 600; }

.lab-view__kicker {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-primary);
  font-weight: 500;
}

.lab-view__title {
  font-size: var(--text-3xl);
  font-weight: 600;
  letter-spacing: -0.025em;
  color: var(--color-text-primary);
  line-height: 1.15;
}

.lab-view__sub {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  max-width: 60ch;
}

.lab-view__info-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: var(--space-md);
}

.lab-view__info-card {
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.lab-view__info-card:hover {
  border-color: var(--color-border-strong);
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
  background: var(--color-muted);
  color: var(--color-text-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.lab-view__info-title { font-size: var(--text-lg); font-weight: 600; letter-spacing: -0.015em; }

.lab-view__info-text { font-size: var(--text-sm); color: var(--color-text-secondary); line-height: 1.6; }

.lab-view__actions {
  display: flex;
  justify-content: flex-start;
  padding-top: var(--space-md);
  border-top: 1px solid var(--color-border);
}

@media (prefers-reduced-motion: reduce) {
  .lab-view__info-card { transition: none; }
}
</style>
