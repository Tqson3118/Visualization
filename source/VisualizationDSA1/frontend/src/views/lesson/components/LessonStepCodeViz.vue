<template>
  <div class="lesson-step-codeviz flex flex-col w-full">
    <!-- Header -->
    <div class="border-b border-border-subtle pb-4 mb-4">
      <div class="flex items-center gap-2 text-xs font-semibold text-accent uppercase tracking-wider mb-1">
        <svg class="w-4 h-4 text-accent" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M10 20l4-16m4 4l4 4-4 4M6 16l-4-4 4-4" />
        </svg>
        <span>Bước {{ stepNumber }} / {{ totalSteps }}</span>
        <span>•</span>
        <span>Code-to-Visualization</span>
      </div>
      <h2 class="text-xl font-extrabold text-text-primary tracking-tight">Gõ code của bạn → bấm Play xem từng dòng chạy</h2>
      <p class="text-xs text-text-muted mt-1">
        Viết JavaScript xử lý mảng, nhấn <span class="text-accent font-semibold">RUN</span> để quan sát từng thao tác so sánh &amp; gán trên mảng.
      </p>
      <span class="inline-flex items-center gap-1.5 mt-3 px-2.5 py-1 rounded-full text-[10px] font-bold bg-accent/15 text-accent border border-accent/30">
        <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M7 8l-4 4 4 4m10-8l4 4-4 4M14 4l-4 16" />
        </svg>
        JavaScript · thuật toán mảng
      </span>
    </div>

    <!-- CodeWorkspace (đã hỗ trợ responsive 1 cột trên mobile).
         Lazy-mount: chỉ dựng khi người học cuộn tới bước này — tránh xung đột
         animStore dùng chung với LessonVisualizer của bước Viz phía trên. -->
    <div ref="workspaceSectionRef" class="codeviz-workspace">
      <CodeWorkspace
        v-if="workspaceVisible"
        :initial-code="initialCode"
        :initial-array="initialArray"
        preserve-animation
      />
      <div v-else class="flex items-center justify-center h-full text-text-muted text-xs">
        Cuộn xuống để tải Code-to-Viz…
      </div>
    </div>

    <!-- Footer: bước tuỳ chọn -->
    <footer class="mt-4 pt-4 border-t border-border-subtle flex flex-wrap items-center justify-between gap-3">
      <p class="text-[11px] text-text-muted">
        Bước này <span class="text-text-secondary font-semibold">không bắt buộc</span> — thử gõ code của riêng bạn rồi xem nó chạy!
      </p>
      <div class="flex items-center gap-2">
        <button
          @click="$emit('completeStep')"
          class="px-4 py-2 text-xs font-bold rounded-xl border border-border-default text-text-secondary hover:text-text-primary hover:bg-bg-hover transition-all cursor-pointer flex items-center gap-1.5"
        >
          <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M9.663 17h4.673M12 3v1m6.364 1.636l-.707.707M21 12h-1M4 12H3m3.343-5.657l-.707-.707m2.828 9.9a5 5 0 117.072 0l-.548.547A3.374 3.374 0 0014 18.469V19a2 2 0 01-2 2h-0a2 2 0 01-2-2v-.531c0-.895-.356-1.754-.988-2.386l-.548-.547z" /></svg>
          Bỏ qua → Quiz
        </button>
        <button
          @click="$emit('completeStep')"
          class="px-4 py-2 bg-accent hover:bg-accent text-white rounded-xl text-xs font-bold transition-all shadow-lg cursor-pointer border border-accent/30 flex items-center gap-1.5"
        >
          <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="3" viewBox="0 0 24 24"><polyline points="20 6 9 17 4 12" /></svg>
          Hoàn thành bước
        </button>
      </div>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';
import { CodeWorkspace } from '@/features/code-to-visualization';

withDefaults(defineProps<{
  initialCode?: string;
  initialArray?: number[];
  stepNumber?: number;
  totalSteps?: number;
}>(), {
  initialCode: undefined,
  initialArray: undefined,
  stepNumber: 3,
  totalSteps: 4,
});

defineEmits<{
  (e: 'completeStep'): void;
}>();

const workspaceSectionRef = ref<HTMLElement | null>(null);
const workspaceVisible = ref(false);
let intersectionObserver: IntersectionObserver | null = null;

onMounted(() => {
  if (typeof IntersectionObserver === 'undefined') {
    workspaceVisible.value = true;
    return;
  }
  intersectionObserver = new IntersectionObserver(
    (entries) => {
      if (entries.some(entry => entry.isIntersecting)) {
        workspaceVisible.value = true;
        intersectionObserver?.disconnect();
        intersectionObserver = null;
      }
    },
    { root: null, threshold: 0.01 },
  );
  if (workspaceSectionRef.value) {
    intersectionObserver.observe(workspaceSectionRef.value);
  }
});

onUnmounted(() => {
  intersectionObserver?.disconnect();
  intersectionObserver = null;
});
</script>

<style scoped>
.codeviz-workspace {
  width: 100%;
  height: min(70vh, 620px);
  min-height: 420px;
  overflow: hidden;
  border-radius: 16px;
}

@media (max-width: 640px) {
  .codeviz-workspace {
    min-height: 380px;
  }
}
</style>
