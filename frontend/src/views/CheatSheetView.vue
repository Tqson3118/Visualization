<script setup lang="ts">
// CheatSheetView — Màn 18: bảng Big-O tương tác + deep-link mô phỏng (trừ tim như bình thường)
import { useRouter } from 'vue-router';

import CheatSheetTable from '@/components/lesson/CheatSheetTable.vue';

const router = useRouter();

function openSimulation(key: string): void {
  void router.push({ name: 'simulator', params: { key } });
}
</script>

<template>
  <main class="cheatsheet-view container">
    <header class="cheatsheet-view__header">
      <h1 class="cheatsheet-view__title">📐 CheatSheet — Độ phức tạp Big-O</h1>
      <p class="text-muted cheatsheet-view__sub">
        Bảng tra cứu nhanh · "▶ Xem mô phỏng" mở simulator (trừ 1 tim theo 20.4 — 3 demo công khai miễn phí).
      </p>
    </header>

    <CheatSheetTable @open-simulation="openSimulation" />
  </main>
</template>

<style scoped>
.cheatsheet-view {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.cheatsheet-view__title { font-size: var(--text-2xl); }
.cheatsheet-view__sub { font-size: var(--text-sm); margin-top: 4px; }
</style>
