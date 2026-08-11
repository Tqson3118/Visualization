<template>
  <div class="lesson-step-viz flex flex-col h-full w-full bg-bg-secondary relative overflow-hidden">
    <!-- Fullscreen overlay -->
    <Teleport to="body">
      <div v-if="isFullscreen" class="fixed inset-0 z-[100] bg-bg-primary flex flex-col overflow-hidden">
        <div class="flex items-center justify-between px-4 py-3 border-b border-border-subtle bg-bg-secondary shrink-0">
          <span class="text-sm font-bold text-text-primary">{{ vizTitle || 'Trực Quan Hóa' }}</span>
          <button
            @click="isFullscreen = false"
            class="px-3.5 py-1.5 bg-accent/90 hover:bg-accent text-white rounded-xl text-xs font-bold transition-all shadow-lg cursor-pointer border border-accent/30 flex items-center gap-1.5"
          >
            <svg class="w-3.5 h-3.5" fill="none" stroke="currentColor" stroke-width="2" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" /></svg>
            <span>Đóng</span>
          </button>
        </div>
        <div class="flex-1 min-h-0 p-4">
          <LessonVisualizer
            :config="visualizerConfig"
            :module-key="moduleKey"
            :title="vizTitle"
            :description="vizDescription"
          />
        </div>
      </div>
    </Teleport>

    <!-- Scoped visualizer -->
    <div class="flex-1 min-h-0 relative w-full h-full">
      <LessonVisualizer
        v-if="hasViz"
        :config="visualizerConfig"
        :module-key="moduleKey"
        :title="vizTitle"
        :description="vizDescription"
        @fullscreen="isFullscreen = true"
      />
      <div v-else class="flex flex-col items-center justify-center h-full text-text-muted p-8 text-center">
        <div class="w-12 h-12 rounded-full bg-bg-surface flex items-center justify-center text-accent mb-3">
          <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
          </svg>
        </div>
        <p class="text-sm font-semibold text-text-secondary">Chưa có cấu hình mô phỏng trực quan</p>
        <p class="text-xs text-text-muted mt-1">Bài học này chưa gắn visualizer. Giảng viên có thể gắn cấu hình trực quan hóa ở bước soạn bài.</p>
      </div>
    </div>

    <!-- Thanh hoàn thành bước (không đè lên toolbar viz) -->
    <footer class="shrink-0 px-4 py-2.5 border-t border-border-subtle bg-bg-surface flex items-center justify-end gap-2">
      <p class="text-[11px] text-text-muted mr-auto">Đã xem xong phần mô phỏng?</p>
      <button
        @click="$emit('completeStep')"
        class="px-4 py-2 bg-accent hover:bg-accent text-white rounded-xl text-xs font-bold transition-all shadow-lg cursor-pointer border border-accent/30 flex items-center gap-1.5"
      >
        <BaseIcon name="check" class="w-3.5 h-3.5" />
        Hoàn thành bước
      </button>
    </footer>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import BaseIcon from '@/shared/components/BaseIcon.vue';
import LessonVisualizer, { type VisualizerConfig } from './LessonVisualizer.vue';

const props = defineProps<{
  vizTitle?: string;
  vizDescription?: string;
  moduleKey?: string;
  visualizerConfig?: VisualizerConfig | string | null;
}>();

defineEmits<{
  (e: 'completeStep'): void;
}>();

const isFullscreen = ref(false);

const parsedConfig = computed<VisualizerConfig | null>(() => {
  if (!props.visualizerConfig) return null;
  if (typeof props.visualizerConfig === 'string') {
    try {
      return JSON.parse(props.visualizerConfig) as VisualizerConfig;
    } catch {
      return null;
    }
  }
  return props.visualizerConfig;
});

const hasViz = computed(() => {
  if (parsedConfig.value?.algorithm) return true;
  return !!props.moduleKey;
});

const visualizerConfig = computed<VisualizerConfig | null>(() => {
  if (parsedConfig.value?.algorithm) return parsedConfig.value;
  return null;
});
</script>
