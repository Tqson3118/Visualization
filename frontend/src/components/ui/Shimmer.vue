<script setup lang="ts">
// Shimmer — overlay shimmer cho card/image loading (UI-PREMIUM 0B).
// Dùng khi cần giữ khung thật (ảnh/card) nhưng phủ hiệu ứng shimmer đang tải.
// Tôn trọng prefers-reduced-motion: hiển thị nền tĩnh.
import { computed } from 'vue';

const props = withDefaults(
  defineProps<{
    /** Phủ toàn bộ phần tử cha (absolute inset-0) */
    absolute?: boolean;
    /** Bán kính bo (khớp phần tử cha) */
    radius?: string;
    /** Dừng shimmer (vd: đã load xong) */
    done?: boolean;
    /** Chân dung người dùng — text fallback trong khung */
    ariaLabel?: string;
  }>(),
  {
    absolute: false,
    radius: 'var(--radius-md)',
    done: false,
    ariaLabel: 'Đang tải',
  },
);

const rootClass = computed(() => [
  'ui-shimmer',
  props.absolute ? 'ui-shimmer--absolute' : '',
  props.done ? 'ui-shimmer--done' : '',
]);
</script>

<template>
  <div
    :class="rootClass"
    :style="{ borderRadius: radius }"
    role="status"
    :aria-label="ariaLabel"
  >
    <slot />
  </div>
</template>

<style scoped>
.ui-shimmer {
  position: relative;
  overflow: hidden;
  background: color-mix(in srgb, var(--color-muted) 70%, transparent);
}

.ui-shimmer--absolute {
  position: absolute;
  inset: 0;
  z-index: 1;
}

.ui-shimmer::after {
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

.ui-shimmer--done::after {
  animation: none;
  opacity: 0;
}

@media (prefers-reduced-motion: reduce) {
  .ui-shimmer::after {
    animation: none;
  }
}
</style>
