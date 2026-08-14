<script setup lang="ts">
// DetailSection — section drawer/sidebar (DESIGN.md §4): title uppercase xs 600
// tracking 0.06em + nội dung flex column. Trích xuất từ khối section lặp ở
// AdminUsersView drawer + ClassDetailView sidebar.
import { type Component } from 'vue';

withDefaults(
  defineProps<{
    title?: string;
    /** Icon lucide nhỏ cạnh title */
    icon?: Component | null;
  }>(),
  {
    title: '',
    icon: null,
  },
);
</script>

<template>
  <section class="detail-section">
    <h3 v-if="title || icon" class="detail-section__title">
      <component v-if="icon" :is="icon" :size="14" aria-hidden="true" />
      {{ title }}
    </h3>
    <div class="detail-section__content">
      <slot />
    </div>
  </section>
</template>

<style scoped>
.detail-section {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.detail-section__title {
  display: flex;
  align-items: center;
  gap: 6px;
  margin: 0;
  font-size: var(--text-xs);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--foreground-tertiary);
}

.detail-section__title :deep(svg) {
  color: var(--foreground-tertiary);
}

.detail-section__content {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}
</style>
