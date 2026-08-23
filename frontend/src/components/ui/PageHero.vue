<script setup lang="ts">
// PageHero — banner surface band level-2 (DESIGN.md §6): KHÔNG gradient, KHÔNG shadow.
// Trích xuất từ khối hero lặp ở 8 views (Admin ×5, Classes, ClassDetail, ClassReport):
//   background: var(--card-raised) + border-bottom/border + radius-lg + padding xl/lg.
// Main: [badges][h1 title][description][actions] · Side: cột phải (mono strip, actions…)
// · Bottom: hàng dưới full width (VD ClassDetailView).
// border="bottom" → border-bottom: 1px solid var(--border-subtle) (5 Admin + Classes);
// border="full"   → border: 1px solid var(--border-subtle) (ClassDetail/ClassReport).
import Badge from '@/components/ui/Badge.vue';

withDefaults(
  defineProps<{
    title?: string;
    description?: string;
    /** Badge variant="primary" hiển thị trên title (thêm nhiều badge qua slot #badges) */
    badge?: string;
    /** padding="lg" → var(--space-lg) var(--space-xl) (ClassReportView) */
    padding?: 'xl' | 'lg';
    /** border="full" → border 4 cạnh (ClassDetail/ClassReport) */
    border?: 'bottom' | 'full';
  }>(),
  {
    title: '',
    description: '',
    badge: '',
    padding: 'xl',
    border: 'bottom',
  },
);
</script>

<template>
  <header
    class="page-hero"
    :class="[
      padding === 'lg' ? 'page-hero--padding-lg' : '',
      border === 'full' ? 'page-hero--border-full' : '',
    ]"
  >
    <div class="page-hero__inner">
      <div class="page-hero__main">
        <div v-if="badge || $slots.badges" class="page-hero__badges">
          <Badge v-if="badge" variant="primary">{{ badge }}</Badge>
          <slot name="badges" />
        </div>
        <h1 v-if="title || $slots.title" class="page-hero__title">
          <slot name="title">{{ title }}</slot>
        </h1>
        <p v-if="description || $slots.description" class="page-hero__desc">
          <slot name="description">{{ description }}</slot>
        </p>
        <div v-if="$slots.actions" class="page-hero__actions">
          <slot name="actions" />
        </div>
      </div>
      <div v-if="$slots.side" class="page-hero__side">
        <slot name="side" />
      </div>
    </div>
    <div v-if="$slots.bottom" class="page-hero__bottom">
      <slot name="bottom" />
    </div>
  </header>
</template>

<style scoped>
/* ── Banner: surface band level-2 (DESIGN §6) — không gradient, không shadow ── */
.page-hero {
  background: var(--card-raised);
  border-bottom: 1px solid var(--border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-xl);
}

.page-hero--border-full {
  border: 1px solid var(--border-subtle);
}

.page-hero--padding-lg {
  padding: var(--space-lg) var(--space-xl);
}

.page-hero__inner {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-lg);
  flex-wrap: wrap;
}

.page-hero__main {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  min-width: 0;
  flex: 1 1 320px;
}

.page-hero__badges {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
}

.page-hero__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  letter-spacing: -0.03em;
  line-height: 1.1;
  margin: 0;
  color: var(--foreground);
  overflow-wrap: anywhere;
}

.page-hero__desc {
  color: var(--foreground-secondary);
  font-size: var(--text-sm);
  max-width: 60ch;
  margin: 0;
}

.page-hero__actions {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  align-items: center;
  margin-top: var(--space-sm);
}

.page-hero__side {
  flex: 0 1 260px;
}

.page-hero__bottom {
  margin-top: var(--space-md);
}

@media (max-width: 640px) {
  .page-hero {
    padding: var(--space-lg);
  }

  .page-hero__side {
    flex-basis: 100%;
  }
}
</style>
