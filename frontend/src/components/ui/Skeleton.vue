<script setup lang="ts">
// Skeleton — wrapper giữ API cũ (G-F1b): height/width/lines/circle.
// UI-PREMIUM 0B: gradient shimmer animation (thay animate-pulse tĩnh),
// preset shapes: circle/text/card/chart (prop `shape`).
import { computed } from 'vue';

import { cn } from '@/lib/utils';
import Skeleton from './skeleton/Skeleton.vue';

const props = withDefaults(
  defineProps<{
    /** Chiều cao (px hoặc chuỗi CSS) */
    height?: string;
    width?: string;
    /** Số dòng skeleton (mặc định 1) */
    lines?: number;
    circle?: boolean;
    /** preset shape — tạo khối skeleton theo hình dáng quen thuộc */
    shape?: 'text' | 'circle' | 'card' | 'chart' | 'block';
  }>(),
  {
    height: '16px',
    width: '100%',
    lines: 1,
    circle: false,
    shape: 'text',
  },
);

const isCircle = computed(() => props.circle || props.shape === 'circle');

const shapeClass = computed(() => {
  switch (props.shape) {
    case 'card':
      return 'rounded-lg';
    case 'chart':
      return 'rounded-md';
    case 'block':
      return 'rounded-sm';
    default:
      return 'rounded-full';
  }
});
</script>

<template>
  <div class="flex flex-col gap-2" :style="{ width }" aria-hidden="true">
    <template v-if="!isCircle && lines > 1">
      <div
        v-for="i in lines"
        :key="i"
        class="ui-skeleton"
        :class="shapeClass"
        :style="{ height, width: i === lines ? '60%' : '100%' }"
      />
    </template>
    <div
      v-else
      class="ui-skeleton"
      :class="cn(shapeClass, isCircle && 'rounded-full')"
      :style="{ height, width }"
    />
  </div>
</template>

<style scoped>
/* Shimmer gradient — nền xám → highlight di chuyển (UI-PREMIUM 0B) */
.ui-skeleton {
  position: relative;
  overflow: hidden;
  background: color-mix(in srgb, var(--color-muted) 70%, transparent);
}

.ui-skeleton::after {
  content: '';
  position: absolute;
  inset: 0;
  transform: translateX(-100%);
  background: linear-gradient(
    90deg,
    transparent,
    color-mix(in srgb, var(--color-surface) 55%, transparent),
    transparent
  );
  animation: ui-shimmer 1.6s var(--ease-in-out) infinite;
}

@media (prefers-reduced-motion: reduce) {
  .ui-skeleton::after {
    animation: none;
  }
}
</style>
