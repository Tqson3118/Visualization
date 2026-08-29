<template>
  <div class="lesson-step-theory flex flex-col h-full overflow-y-auto p-6 text-vdsa-text font-sans leading-relaxed">

    <div class="border-b border-vdsa-border pb-4 mb-6">
      <div class="flex items-center gap-2 text-xs font-semibold text-vdsa-purple-light uppercase tracking-wider mb-1">
        <svg class="w-4 h-4 text-vdsa-purple-light" fill="none" stroke="currentColor" viewBox="0 0 24 24" stroke-width="2">
          <path stroke-linecap="round" stroke-linejoin="round" d="M12 6.253v13m0-13C10.832 5.477 9.246 5 7.5 5S4.168 5.477 3 6.253v13C4.168 18.477 5.754 18 7.5 18s3.332.477 4.5 1.253m0-13C13.168 5.477 14.754 5 16.5 5c1.747 0 3.332.477 4.5 1.253v13C19.832 18.477 18.247 18 16.5 18c-1.746 0-3.332.477-4.5 1.253" />
        </svg>
        <span>Kiến Thức Nền Tảng</span>
      </div>
      <h1 class="text-2xl font-black text-white tracking-tight">{{ title }}</h1>
    </div>

    <!-- Danh sách mô phỏng tương tác đính kèm -->
    <div v-if="allSimulationKeys.length > 0" class="mb-6 p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border">
      <div class="flex items-center justify-between mb-3">
        <h3 class="text-xs font-extrabold text-white uppercase tracking-wider flex items-center gap-2">
          <Sparkles class="w-4 h-4 text-vdsa-purple" />
          Mô phỏng trực quan đính kèm ({{ allSimulationKeys.length }})
        </h3>
        <span class="text-[11px] text-vdsa-muted">Thực hành tương tác từng bước</span>
      </div>
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
        <div
          v-for="simKey in allSimulationKeys"
          :key="simKey"
          class="p-3 rounded-xl bg-vdsa-bg-secondary border border-vdsa-border flex items-center justify-between gap-3 hover:border-vdsa-accent transition-all"
        >
          <div class="flex items-center gap-2.5 min-w-0">
            <span class="p-2 rounded-lg bg-vdsa-accent/20 text-vdsa-purple-light shrink-0">
              <Play class="w-4 h-4" />
            </span>
            <div class="min-w-0">
              <h4 class="text-xs font-bold text-white truncate">{{ getSimulationTitle(simKey) }}</h4>
              <p class="text-[10px] font-mono text-vdsa-muted truncate">{{ simKey }}</p>
            </div>
          </div>
          <div class="flex items-center gap-1.5 shrink-0">
            <button
              type="button"
              data-testid="run-simulation-btn"
              class="px-3 py-1.5 rounded-lg bg-vdsa-accent hover:bg-vdsa-accent-dark text-white text-[11px] font-bold transition-colors cursor-pointer flex items-center gap-1"
              @click="openEmbeddedVisualizer(simKey)"
            >
              <Play class="w-3 h-3" />
              <span>Chạy thử thuật toán</span>
            </button>
            <router-link
              :to="`/simulator/${simKey}`"
              target="_blank"
              class="p-1.5 rounded-lg bg-vdsa-surface hover:bg-vdsa-hover text-vdsa-muted hover:text-white transition-colors"
              title="Mở toàn màn hình"
            >
              <ExternalLink class="w-3.5 h-3.5" />
            </router-link>
          </div>
        </div>
      </div>
    </div>

    <!-- Embedded shared visualizer (B3) — đóng về Lý thuyết KHÔNG mất progress -->
    <div v-if="visualizerOpen" class="relative mb-6 h-[480px] rounded-xl overflow-hidden border border-vdsa-border" data-testid="embedded-visualizer">
      <SharedVisualizerShell
        :frames="frames"
        :algorithm-key="activeSimKey ?? undefined"
        embedded
        close-label="Về lý thuyết"
        @close="visualizerOpen = false"
      />
    </div>

    <div class="prose prose-invert max-w-none text-sm space-y-4">
      <div v-html="formattedContent"></div>
    </div>

    <div class="mt-8 pt-6 border-t border-vdsa-border flex items-center justify-between">
      <span class="text-xs text-vdsa-muted">
        {{ isCompleted ? 'Bạn đã hoàn thành bài học này.' : 'Đọc hết nội dung để hoàn thành bài học.' }}
      </span>
      <div
        v-if="isCompleted"
        class="px-5 py-2.5 bg-vdsa-green/15 text-vdsa-green border border-vdsa-green/30 rounded-xl text-xs font-bold flex items-center gap-2 select-none"
      >
        <BaseIcon name="check-circle" class="w-4 h-4 text-vdsa-green" />
        <span>Đã hoàn thành</span>
      </div>
      <button
        v-else
        @click="$emit('completeStep')"
        class="px-5 py-2.5 bg-vdsa-accent hover:bg-vdsa-accent-dark text-white rounded-xl text-xs font-bold transition-all shadow-lg shadow-vdsa-accent/30 flex items-center gap-2 cursor-pointer"
      >
        <span>Hoàn thành bài học</span>
        <BaseIcon name="check" class="w-4 h-4" />
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import { Sparkles, Play, ExternalLink } from 'lucide-vue-next';
import BaseIcon from '../../../shared/components/BaseIcon.vue';
import { parseMarkdownToHtml } from '@/utils/markdownParser';
import SharedVisualizerShell from '../../../features/visual-shell/components/SharedVisualizerShell.vue';
import { buildFramesFromCatalogKey } from '../../../features/visual-shell/buildFrames';
import type { SortFrame } from '../../../features/algorithm-sandbox/types/sorting.types';
import { CATALOG } from '@/engines/catalog';

const props = defineProps<{
  title: string;
  content: string;
  isCompleted?: boolean;
  /** Catalog key mô phỏng (VD 'sort.bubble') */
  simulationKey?: string | null;
  /** Mảng các mô phỏng đính kèm */
  simulationKeys?: string[];
}>();

defineEmits<{
  (e: 'completeStep'): void;
}>();

const visualizerOpen = ref(false);
const activeSimKey = ref<string | null>(props.simulationKey || null);

const allSimulationKeys = computed<string[]>(() => {
  const list = [...(props.simulationKeys || [])];
  if (props.simulationKey && !list.includes(props.simulationKey)) {
    list.unshift(props.simulationKey);
  }
  return list;
});

function getSimulationTitle(key: string): string {
  const item = CATALOG.find((c) => c.key === key);
  return item ? item.title : key;
}

function openEmbeddedVisualizer(key: string): void {
  activeSimKey.value = key;
  visualizerOpen.value = true;
}

/** Frame từ engine generator: key → Step[] → LegacyStepAdapter → SortFrame[]. */
const frames = computed<SortFrame[]>(() => {
  if (!activeSimKey.value) return [];
  const res = buildFramesFromCatalogKey(activeSimKey.value);
  return Array.isArray(res) ? (res as SortFrame[]) : [];
});

/**
 * Render Markdown sang HTML với Syntax Highlighting và dark theme
 */
const formattedContent = computed(() => {
  if (!props.content) return '<p class="text-vdsa-muted italic">Không có nội dung lý thuyết.</p>';
  return parseMarkdownToHtml(props.content);
});
</script>
