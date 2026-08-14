<script setup lang="ts">
// RevealSection — wrapper reveal khi scroll vào viewport (UI-PREMIUM 0B/0C):
// IntersectionObserver → Motion fade+translateY, prop `delay` cho stagger.
// Tôn trọng prefers-reduced-motion (useScrollReveal set visible ngay lập tức).
// Chỉ transform + opacity, easing easeOut-expo — chuẩn DESIGN.md §7.10.
import { computed, ref } from 'vue';
import { Motion } from 'motion-v';

import { useScrollReveal } from '@/composables/useScrollReveal';

const props = withDefaults(
  defineProps<{
    /** delay ms cho stagger (MOTION.stagger) */
    delay?: number;
    /** preset motion */
    preset?: 'fadeUp' | 'fadeDown' | 'scaleIn' | 'slideRight';
    /** chạy ngay khi mount, không chờ scroll */
    immediate?: boolean;
  }>(),
  {
    delay: 0,
    preset: 'fadeUp',
    immediate: false,
  },
);

const root = ref<HTMLElement | null>(null);
const { isVisible } = useScrollReveal(root, { threshold: 0.12 });

const visible = computed(() => props.immediate || isVisible.value);
</script>

<template>
  <!-- div thật để observe (Motion là component — ref không trả Element) -->
  <div ref="root">
    <Motion
      :initial="
        preset === 'fadeDown'
          ? { opacity: 0, y: -12 }
          : preset === 'scaleIn'
            ? { opacity: 0, scale: 0.95 }
            : preset === 'slideRight'
              ? { opacity: 0, x: -16 }
              : { opacity: 0, y: 20 }
      "
      :animate="
        visible
          ? preset === 'fadeDown'
            ? { opacity: 1, y: 0 }
            : preset === 'scaleIn'
              ? { opacity: 1, scale: 1 }
              : preset === 'slideRight'
                ? { opacity: 1, x: 0 }
                : { opacity: 1, y: 0 }
          : preset === 'fadeDown'
            ? { opacity: 0, y: -12 }
            : preset === 'scaleIn'
              ? { opacity: 0, scale: 0.95 }
              : preset === 'slideRight'
                ? { opacity: 0, x: -16 }
                : { opacity: 0, y: 20 }
      "
      :transition="{ duration: 0.5, ease: [0.16, 1, 0.3, 1], delay }"
    >
      <slot />
    </Motion>
  </div>
</template>
