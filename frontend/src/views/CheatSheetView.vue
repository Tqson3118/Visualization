<script setup lang="ts">
// CheatSheetView — Màn 18: bảng Big-O tương tác + deep-link mô phỏng (trừ tim như bình thường).
// View-quality (nhóm A): chrome gradient mint + shadow → surface band level-2 (card-raised +
// border-subtle, KHÔNG shadow — §1/§6); thêm strip Big-O block-token + index mono trong banner
// (dữ liệu chỉ số → quyết định xuyên-nhóm 4); H1 48px/600/-0.03em; icon lucide muted square;
// badge count → muted (accent CHỈ interactive); giữ i18n + selector breadcrumb cũ.
import { RouterLink, useRouter } from 'vue-router';
import { Motion } from 'motion-v';
import { Table2 } from 'lucide-vue-next';

import CheatSheetTable from '@/components/lesson/CheatSheetTable.vue';
import Badge from '@/components/ui/Badge.vue';
import { CATALOG } from '@/engines/catalog';
import { messages } from '@/i18n/vi';

const router = useRouter();

function openSimulation(key: string): void {
  void router.push({ name: 'simulator', params: { key } });
}

/** Strip Big-O — chip block-token tối + index mono (decorative, aria-hidden). */
const BIG_O_STRIP = [
  { value: 'O(1)', index: '00' },
  { value: 'O(log n)', index: '01' },
  { value: 'O(n)', index: '02' },
  { value: 'O(n log n)', index: '03' },
  { value: 'O(n²)', index: '04' },
] as const;
</script>

<template>
  <main class="cheatsheet-view container">
    <!-- Chrome header — surface band level-2 (bỏ gradient mint + shadow, §1/§6) -->
    <Motion
      class="cheatsheet-view__chrome"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.28, ease: [0.16, 1, 0.3, 1] }"
    >
      <nav class="cheatsheet-view__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'simulations' }">{{ messages.cheatsheet.breadcrumbParent }}</RouterLink>
        <span aria-hidden="true">/</span>
        <span aria-current="page">{{ messages.cheatsheet.title }}</span>
      </nav>
      <div class="cheatsheet-view__hero">
        <span class="cheatsheet-view__icon" aria-hidden="true">
          <Table2 :size="20" />
        </span>
        <div>
          <h1 class="cheatsheet-view__title">{{ messages.cheatsheet.title }}</h1>
          <p class="cheatsheet-view__sub">{{ messages.cheatsheet.sub }}</p>
        </div>
        <Badge variant="muted" class="cheatsheet-view__badge">
          {{ messages.cheatsheet.badge(CATALOG.length) }}
        </Badge>
      </div>

      <!-- Strip Big-O block-token + index mono — dấu vân tay Data Bench (decorative) -->
      <div class="cheatsheet-view__strip" aria-hidden="true">
        <p class="cheatsheet-view__strip-label">{{ messages.cheatsheet.stripLabel(CATALOG.length) }}</p>
        <div class="cheatsheet-view__strip-blocks">
          <div v-for="chip in BIG_O_STRIP" :key="chip.index" class="cheatsheet-view__strip-block">
            <span class="cheatsheet-view__strip-value">{{ chip.value }}</span>
            <span class="cheatsheet-view__strip-index">{{ chip.index }}</span>
          </div>
        </div>
      </div>
    </Motion>

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

/* ── Chrome header — surface band level-2 (§6): card-raised + border-subtle, KHÔNG shadow ── */
.cheatsheet-view__chrome {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-lg) var(--space-xl);
}

.cheatsheet-view__breadcrumb {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
}

.cheatsheet-view__breadcrumb a {
  color: var(--color-primary);
  font-weight: 600;
  padding-block: var(--space-xs);
}

.cheatsheet-view__hero {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.cheatsheet-view__icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.cheatsheet-view__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  line-height: 1.1;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
  margin: 0;
}

.cheatsheet-view__sub {
  font-size: var(--text-sm);
  color: var(--color-text-secondary);
  max-width: 64ch;
  margin-top: var(--space-xs);
}

.cheatsheet-view__badge { margin-left: auto; }

/* ── Strip Big-O: chip block-token tối + index mono (hero motif duy nhất/màn) ── */
.cheatsheet-view__strip {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-md);
  border-top: 1px solid var(--color-border-subtle);
  padding-top: var(--space-md);
}

.cheatsheet-view__strip-label {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
  margin: 0;
}

.cheatsheet-view__strip-blocks {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
}

.cheatsheet-view__strip-block {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
  min-width: 44px;
  padding: var(--space-xs) var(--space-sm);
  border-radius: var(--radius-md);
  background: var(--color-canvas-ink);
}

.cheatsheet-view__strip-value {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  color: rgba(255, 255, 255, 0.92);
  line-height: 1.4;
  white-space: nowrap;
}

.cheatsheet-view__strip-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-index-muted);
  line-height: 1.4;
}

@media (max-width: 640px) {
  .cheatsheet-view__chrome { padding: var(--space-md); }
  .cheatsheet-view__badge { margin-left: 0; }
  .cheatsheet-view__hero { align-items: flex-start; }
  .cheatsheet-view__strip { align-items: flex-start; }
}
</style>
