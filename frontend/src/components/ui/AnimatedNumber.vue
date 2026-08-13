<script setup lang="ts">
// AnimatedNumber — số đếm lên (count-up) cho stats/XP/gems (UI-PREMIUM 0B).
// Dùng requestAnimationFrame + easing easeOutExpo. Chỉ chạy khi element
// vào viewport (IntersectionObserver, once) hoặc khi `start` = true.
// Tôn trọng prefers-reduced-motion: hiển thị thẳng giá trị cuối.
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';

const props = withDefaults(
  defineProps<{
    /** Giá trị đích */
    value: number;
    /** Số chữ số thập phân hiển thị (mặc định 0) */
    decimals?: number;
    /** prefix/suffix ví dụ "+", "%", " ngày" */
    prefix?: string;
    suffix?: string;
    /** Duration ms (mặc định 800) */
    duration?: number;
    /** Bắt đầu chạy ngay khi mount (không chờ viewport) */
    immediate?: boolean;
  }>(),
  {
    decimals: 0,
    prefix: '',
    suffix: '',
    duration: 800,
    immediate: false,
  },
);

const el = ref<HTMLElement | null>(null);
const display = ref(0);
const started = ref(false);
let raf = 0;
let observer: IntersectionObserver | null = null;

const reducedMotion =
  typeof window !== 'undefined' && window.matchMedia('(prefers-reduced-motion: reduce)').matches;

function animate(): void {
  const from = 0;
  const to = props.value;
  const start = performance.now();

  const tick = (now: number): void => {
    const progress = Math.min((now - start) / props.duration, 1);
    // easeOutExpo — chuẩn motion token
    const eased = progress === 1 ? 1 : 1 - Math.pow(2, -10 * progress);
    display.value = from + (to - from) * eased;
    if (progress < 1) {
      raf = requestAnimationFrame(tick);
    } else {
      display.value = to;
    }
  };

  raf = requestAnimationFrame(tick);
}

function start(): void {
  if (started.value) return;
  started.value = true;
  animate();
}

onMounted(() => {
  if (reducedMotion) {
    display.value = props.value;
    return;
  }
  if (props.immediate) {
    start();
    return;
  }
  if (!el.value || typeof IntersectionObserver === 'undefined') {
    start();
    return;
  }
  observer = new IntersectionObserver(
    (entries) => {
      if (entries.some((e) => e.isIntersecting)) {
        start();
        observer?.disconnect();
      }
    },
    { threshold: 0.3 },
  );
  observer.observe(el.value);
});

onUnmounted(() => {
  cancelAnimationFrame(raf);
  observer?.disconnect();
});

watch(
  () => props.value,
  () => {
    if (started.value && !reducedMotion) animate();
  },
);

const formatted = computed(() =>
  display.value.toLocaleString('vi-VN', {
    minimumFractionDigits: props.decimals,
    maximumFractionDigits: props.decimals,
  }),
);
</script>

<template>
  <span ref="el" class="ui-animated-number font-variant-numeric tabular-nums">
    {{ prefix }}<span aria-hidden="true">{{ formatted }}</span>{{ suffix }}
  </span>
</template>
