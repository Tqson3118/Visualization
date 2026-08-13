<script setup lang="ts">
// ProgressRing — vòng tròn SVG tiến trình (UI-PREMIUM 0B):
// completion %, level progress. Animate stroke-dashoffset khi mount
// (IntersectionObserver, once). Tôn trọng prefers-reduced-motion.
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';

const props = withDefaults(
  defineProps<{
    /** Tiến trình 0..100 */
    progress: number;
    /** Đường kính px */
    size?: number;
    /** Độ dày stroke px */
    strokeWidth?: number;
    /** Màu stroke — token class hoặc hex */
    color?: string;
    /** Hiển thị % ở giữa (mono, chuẩn data) */
    showLabel?: boolean;
    label?: string;
    /** Chạy ngay khi mount, không chờ viewport */
    immediate?: boolean;
  }>(),
  {
    size: 64,
    strokeWidth: 5,
    color: 'var(--color-data-core)',
    showLabel: false,
    label: '',
    immediate: false,
  },
);

const el = ref<SVGSVGElement | null>(null);
const animated = ref(0);
let observer: IntersectionObserver | null = null;

const reducedMotion =
  typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

const radius = computed(() => (props.size - props.strokeWidth) / 2);
const circumference = computed(() => 2 * Math.PI * radius.value);

const dashOffset = computed(() => {
  const p = Math.min(Math.max(animated.value, 0), 100) / 100;
  return circumference.value * (1 - p);
});

function animateTo(target: number): void {
  if (reducedMotion) {
    animated.value = target;
    return;
  }
  const start = animated.value;
  const duration = 700;
  const t0 = performance.now();
  const step = (now: number): void => {
    const p = Math.min((now - t0) / duration, 1);
    const eased = p === 1 ? 1 : 1 - Math.pow(2, -10 * p);
    animated.value = start + (target - start) * eased;
    if (p < 1) requestAnimationFrame(step);
  };
  requestAnimationFrame(step);
}

onMounted(() => {
  if (props.immediate || !el.value || typeof IntersectionObserver === 'undefined') {
    animateTo(props.progress);
    return;
  }
  observer = new IntersectionObserver(
    (entries) => {
      if (entries.some((e) => e.isIntersecting)) {
        animateTo(props.progress);
        observer?.disconnect();
      }
    },
    { threshold: 0.3 },
  );
  observer.observe(el.value);
});

onUnmounted(() => observer?.disconnect());

watch(
  () => props.progress,
  (v) => {
    if (el.value) animateTo(v);
  },
);
</script>

<template>
  <div class="ui-progress-ring" :style="{ width: `${size}px`, height: `${size}px` }">
    <svg
      ref="el"
      :width="size"
      :height="size"
      :viewBox="`0 0 ${size} ${size}`"
      role="progressbar"
      :aria-valuenow="Math.round(progress)"
      aria-valuemin="0"
      aria-valuemax="100"
      aria-label="Tiến trình"
    >
      <circle
        :cx="size / 2"
        :cy="size / 2"
        :r="radius"
        fill="none"
        :stroke-width="strokeWidth"
        class="ui-progress-ring__track"
      />
      <circle
        :cx="size / 2"
        :cy="size / 2"
        :r="radius"
        fill="none"
        :stroke="color"
        :stroke-width="strokeWidth"
        stroke-linecap="round"
        :stroke-dasharray="circumference"
        :stroke-dashoffset="dashOffset"
        transform="rotate(-90 50 50)"
        class="ui-progress-ring__bar"
      />
    </svg>
    <div v-if="showLabel" class="ui-progress-ring__label" aria-hidden="true">
      <span class="ui-progress-ring__value">{{ Math.round(animated) }}%</span>
      <span v-if="label" class="ui-progress-ring__caption">{{ label }}</span>
    </div>
  </div>
</template>

<style scoped>
.ui-progress-ring {
  position: relative;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.ui-progress-ring__track {
  stroke: color-mix(in srgb, var(--color-data-core) 15%, transparent);
}

.ui-progress-ring__bar {
  transition: stroke-dashoffset 0.02s linear;
}

.ui-progress-ring__label {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 1px;
  pointer-events: none;
}

.ui-progress-ring__value {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 500;
  color: var(--color-foreground);
}

.ui-progress-ring__caption {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}
</style>
