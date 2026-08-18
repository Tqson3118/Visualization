<template>
  <div class="shared-visualizer-shell relative flex flex-col w-full h-full min-h-0 overflow-hidden font-sans bg-bg-primary border border-border-default rounded-xl">
    <!-- Header / HUD -->
    <div class="relative z-10 flex items-center justify-between gap-2 px-3.5 pt-3 pb-2 shrink-0 flex-wrap">
      <SortingHudOverlay :step-description="stepDescription" />
      <div class="flex items-center gap-2 shrink-0">
        <span class="text-[10px] font-bold uppercase tracking-wider text-text-muted font-mono border border-border-default rounded-md px-2 py-1 bg-bg-surface">
          {{ algoLabel }}
        </span>
        <button
          v-if="embedded"
          type="button"
          class="flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-[11px] font-bold text-text-secondary hover:text-accent hover:bg-bg-hover border border-border-default cursor-pointer transition-all"
          :aria-label="closeLabel || 'Đóng mô phỏng'"
          data-testid="shared-shell-close"
          @click="$emit('close')"
        >
          <BaseIcon name="close" class="w-3.5 h-3.5" />
          <span>{{ closeLabel || 'Đóng' }}</span>
        </button>
      </div>
    </div>

    <!-- Renderer (slot mặc định = sorting dispatcher) -->
    <div class="relative z-10 flex-1 min-h-0 px-3 pb-2 overflow-hidden">
      <slot name="renderer" :frame="currentFrame">
        <SortingVisualizerDispatcher :frame="currentFrame" class="w-full h-full" />
      </slot>
    </div>

    <!-- Drawer trace (props-driven) -->
    <div class="absolute bottom-2 right-2 z-30">
      <button
        type="button"
        class="flex items-center gap-2 px-3 py-1.5 rounded-lg text-xs font-semibold transition-all duration-200 shadow-lg cursor-pointer bg-bg-surface hover:bg-bg-hover text-text-secondary border border-border-default hover:border-border-strong"
        :aria-expanded="traceOpen"
        :aria-label="traceOpen ? 'Thu gọn bảng trạng thái' : 'Mở bảng trạng thái'"
        @click="traceOpen = !traceOpen"
      >
        <BaseIcon name="file-text" class="w-4 h-4 text-accent" />
        <span>{{ traceOpen ? 'Thu gọn' : codeBtnLabel }}</span>
      </button>

      <Transition name="shell-drawer">
        <div
          v-if="traceOpen"
          class="absolute bottom-10 right-0 z-40 w-80 max-w-[calc(100vw_-_1.5rem)] max-h-[min(520px,calc(100vh_-_6rem))] p-2 rounded-lg bg-bg-surface border border-border-default shadow-2xl flex flex-col gap-2 overflow-hidden"
        >
          <div class="flex items-center justify-between border-b border-border-subtle pb-1.5 flex-shrink-0">
            <span class="text-xs font-semibold text-text-primary flex items-center gap-1.5">
              <BaseIcon name="multi-view" class="w-3.5 h-3.5 text-accent" />
              Bảng trạng thái
            </span>
            <span class="text-[10px] font-mono text-text-muted">{{ currentStepLabel }}</span>
          </div>
          <div class="flex-1 min-h-0 flex flex-col gap-1.5 px-1 py-1 overflow-auto">
            <p class="text-[11px] text-text-primary leading-normal">{{ stepDescription }}</p>
            <SortingTraceTable
              :frames="frames"
              :current-index="vcrStore.currentFrameIndex"
              class="flex-1 min-h-0"
              @jump="vcrStore.jumpToFrame"
            />
          </div>
        </div>
      </Transition>
    </div>

    <!-- Progress + Controls -->
    <SortingProgressBar :progress-percent="progressPercent" class="relative z-20" />
    <div class="relative z-20 shrink-0 px-3 pb-3 pt-1 flex justify-center">
      <div class="w-full max-w-2xl">
        <VcrDockBar />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { useVcrStore } from '../../vcr-player';
import VcrDockBar from '../../vcr-player/components/VcrDockBar.vue';
import SortingHudOverlay from '../../algorithm-sandbox/components/SortingHudOverlay.vue';
import SortingVisualizerDispatcher from '../../algorithm-sandbox/components/SortingVisualizerDispatcher.vue';
import SortingProgressBar from '../../algorithm-sandbox/components/SortingProgressBar.vue';
import SortingTraceTable from '../../algorithm-sandbox/components/SortingTraceTable.vue';
import BaseIcon from '@/shared/components/BaseIcon.vue';
import type { SortFrame } from '../../algorithm-sandbox/types/sorting.types';

const props = defineProps<{
  frames: SortFrame[];
  algorithmKey?: string;
  embedded?: boolean;
  closeLabel?: string;
}>();

defineEmits<{
  (e: 'close'): void;
}>();

const vcrStore = useVcrStore();
const traceOpen = ref(false);

const ALGO_LABELS: Record<string, string> = {
  bubble: 'Bubble Sort',
  selection: 'Selection Sort',
  insertion: 'Insertion Sort',
  quick: 'Quick Sort',
  merge: 'Merge Sort',
  heap: 'Heap Sort',
  radix: 'Radix Sort',
  counting: 'Counting Sort',
  bucket: 'Bucket Sort',
};

const algoLabel = computed(() => {
  const fromFrame = props.frames[0]?.algorithm;
  const label = fromFrame ? ALGO_LABELS[fromFrame] : undefined;
  if (label) return label;
  const key = props.algorithmKey ?? '';
  const short = key.replace('sort.', '');
  return ALGO_LABELS[short] ?? (key || 'Mô phỏng');
});

const currentFrame = computed<SortFrame | null>(() => {
  const frames = props.frames;
  if (frames.length === 0) return null;
  const idx = vcrStore.currentFrameIndex;
  if (idx < 0 || idx >= frames.length) return null;
  return frames[idx] ?? null;
});

const stepDescription = computed(() => currentFrame.value?.description ?? 'Nhấn Phát để chạy mô phỏng từng bước.');

const progressPercent = computed(() => {
  const frames = props.frames;
  if (frames.length <= 1) return 0;
  const ratio = vcrStore.currentFrameIndex / (frames.length - 1);
  return Math.min(100, Math.max(0, ratio * 100));
});

const currentStepLabel = computed(() => {
  const total = props.frames.length;
  if (total <= 0) return '';
  return String(Math.min(vcrStore.currentFrameIndex + 1, total)) + '/' + String(total);
});

const codeBtnLabel = computed(() => {
  const total = props.frames.length;
  if (total <= 0) return 'Bảng trạng thái';
  return algoLabel.value + ' (' + String(Math.min(vcrStore.currentFrameIndex + 1, total)) + '/' + String(total) + ')';
});

watch(
  () => props.frames,
  (frames) => {
    vcrStore.playbackFrames = frames;
    vcrStore.customCompileFn = null;
    vcrStore.reset();
  },
  { immediate: true, deep: true },
);

onBeforeUnmount(() => {
  // Dừng timer VCR khi rời màn (không tắt customCompileFn — feature khác tự quản lý).
  vcrStore.pause();
});
</script>

<style scoped>
.shell-drawer-enter-active,
.shell-drawer-leave-active {
  transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1);
}
.shell-drawer-enter-from,
.shell-drawer-leave-to {
  opacity: 0;
  transform: translateY(8px) scale(0.96);
}
</style>
