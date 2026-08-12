<script setup lang="ts">
// LabView — Màn 15: Interactive Lab (Bậc 2) tại /ladder/:nodeId/lab
// Canvas editable + chấm trạng thái cuối + giới hạn bước ×1.5 + nộp labAnswer
import { computed } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { getCatalogMeta } from '@/engines/catalog';
import LabStage from '@/components/ladder/LabStage.vue';
import Button from '@/components/ui/Button.vue';

const route = useRoute();
const router = useRouter();

const nodeId = computed(() => String(route.params.nodeId ?? ''));

const simKey = computed(() => {
  const keys = ['sort.bubble', 'search.binary', 'graph.bfs', 'tree.bst-insert', 'stack.push', 'queue.enqueue', 'sort.merge', 'sort.quick'];
  const key = keys[(Number(nodeId.value) - 1) % keys.length];
  return getCatalogMeta(key) ? key : 'sort.bubble';
});

const title = computed(() => getCatalogMeta(simKey.value)?.title ?? `Node ${nodeId.value}`);
</script>

<template>
  <main class="lab-view container">
    <nav class="lab-view__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'ladder', params: { nodeId } }">Ladder</RouterLink>
      <span aria-hidden="true">/</span>
      <span>Lab — {{ title }}</span>
    </nav>

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

.lab-view__actions {
  display: flex;
  justify-content: flex-start;
  padding-top: var(--space-md);
  border-top: 1px solid var(--color-border);
}
</style>
