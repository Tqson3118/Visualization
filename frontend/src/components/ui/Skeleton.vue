<script setup lang="ts">
// Skeleton — component UI chung: placeholder loading (design tokens — SDD §8.1)
withDefaults(
  defineProps<{
    /** Chiều cao (px hoặc chuỗi CSS) */
    height?: string;
    width?: string;
    /** Số dòng skeleton (mặc định 1) */
    lines?: number;
    circle?: boolean;
  }>(),
  {
    height: '16px',
    width: '100%',
    lines: 1,
    circle: false,
  },
);
</script>

<template>
  <div class="ui-skeleton" :class="{ 'ui-skeleton--circle': circle }" :style="{ height, width }" aria-hidden="true">
    <template v-if="!circle && lines > 1">
      <div v-for="i in lines" :key="i" class="ui-skeleton__line" :style="{ width: i === lines ? '60%' : '100%' }" />
    </template>
  </div>
</template>

<style scoped>
.ui-skeleton {
  display: flex;
  flex-direction: column;
  gap: 8px;
  border-radius: var(--radius-sm);
  background: linear-gradient(90deg, var(--color-muted) 25%, var(--color-surface-hover) 50%, var(--color-muted) 75%);
  background-size: 200% 100%;
  animation: ui-skeleton-shimmer 1.4s ease infinite;
}

.ui-skeleton--circle { border-radius: 50%; }

.ui-skeleton__line {
  height: inherit;
  border-radius: var(--radius-sm);
  background: var(--color-muted);
}

@keyframes ui-skeleton-shimmer {
  to { background-position: -200% 0; }
}
</style>
