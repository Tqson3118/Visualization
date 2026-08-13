<script setup lang="ts">
// CheatSheetView — Màn 18: bảng Big-O tương tác + deep-link mô phỏng (trừ tim như bình thường)
// H-E2: chrome hero Cyber Mint + breadcrumb (đồng bộ SimulationsView/BenchmarkView).
import { RouterLink, useRouter } from 'vue-router';
import { Table2 } from 'lucide-vue-next';

import CheatSheetTable from '@/components/lesson/CheatSheetTable.vue';
import Badge from '@/components/ui/Badge.vue';
import { CATALOG } from '@/engines/catalog';
import { messages } from '@/i18n/vi';

const router = useRouter();

function openSimulation(key: string): void {
  void router.push({ name: 'simulator', params: { key } });
}
</script>

<template>
  <main class="cheatsheet-view container">
    <!-- Chrome header — Cyber Mint (palette 3) -->
    <header class="cheatsheet-view__chrome">
      <nav class="cheatsheet-view__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'simulations' }">{{ messages.cheatsheet.breadcrumbParent }}</RouterLink>
        <span aria-hidden="true">/</span>
        <span>CheatSheet</span>
      </nav>
      <div class="cheatsheet-view__hero">
        <span class="cheatsheet-view__icon" aria-hidden="true">
          <Table2 :size="22" />
        </span>
        <div>
          <h1 class="cheatsheet-view__title">{{ messages.cheatsheet.title }}</h1>
          <p class="cheatsheet-view__sub">{{ messages.cheatsheet.sub }}</p>
        </div>
        <Badge variant="primary" class="cheatsheet-view__badge">
          {{ messages.cheatsheet.badge(CATALOG.length) }}
        </Badge>
      </div>
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

/* Chrome header — Cyber Mint (đồng bộ SimulationsView/BenchmarkView) */
.cheatsheet-view__chrome {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 28%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-mint);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.cheatsheet-view__chrome::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  /* H-E2: 68% (thay 62% của BenchmarkView) để text-muted trên chrome ≥ 4.5:1 cả 2 theme */
  background: color-mix(in srgb, var(--color-background) 68%, transparent);
}

.cheatsheet-view__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-muted);
}

.cheatsheet-view__breadcrumb a { color: var(--color-primary); font-weight: 600; }

.cheatsheet-view__hero {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.cheatsheet-view__icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-lg);
  background-image: var(--gradient-mint);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-md);
}

.cheatsheet-view__title {
  font-size: clamp(var(--text-2xl), 4vw, var(--text-3xl));
  background-image: var(--gradient-mint);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.cheatsheet-view__sub {
  font-size: var(--text-sm);
  /* H-E2: foreground 92% — text-muted chỉ 3.47:1 trên chrome light (fail AA) */
  color: color-mix(in srgb, var(--color-foreground) 92%, transparent);
  max-width: 64ch;
  margin-top: 2px;
}

.cheatsheet-view__badge { margin-left: auto; }

@media (max-width: 640px) {
  .cheatsheet-view__chrome { padding: var(--space-md); }
  .cheatsheet-view__badge { margin-left: 0; }
}
</style>
