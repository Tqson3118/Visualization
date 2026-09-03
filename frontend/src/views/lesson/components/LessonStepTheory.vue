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

    <!-- Render nội dung lý thuyết & Nhúng trực tiếp các bộ mô phỏng inline -->
    <div class="space-y-6">
      <template v-for="(seg, idx) in contentSegments" :key="idx">
        <div v-if="seg.type === 'html'" class="prose prose-invert max-w-none text-sm space-y-4" v-html="seg.html"></div>
        <InlineSimulationPlayer v-else-if="seg.type === 'simulation' && seg.simKey" :sim-key="seg.simKey" />
      </template>

      <!-- Tự động hiển thị các mô phỏng đính kèm vào luồng bài học nếu chưa có thẻ anchor trong text -->
      <div v-if="trailingSimulations.length > 0" class="mt-8 pt-6 border-t border-vdsa-border/60 space-y-6">
        <div class="flex items-center gap-2">
          <Sparkles class="w-4 h-4 text-purple-400" />
          <h3 class="text-xs font-extrabold text-white uppercase tracking-wider">
            Mô phỏng Trực quan Tương tác ({{ trailingSimulations.length }})
          </h3>
        </div>
        <InlineSimulationPlayer
          v-for="simKey in trailingSimulations"
          :key="simKey"
          :sim-key="simKey"
        />
      </div>
    </div>

    <div class="mt-8 pt-6 border-t border-vdsa-border flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
      <span class="text-xs text-vdsa-muted">
        <template v-if="isCompleted">
          Bạn đã hoàn thành bài học này.
        </template>
        <template v-else>
          Đã sẵn sàng! Nhấn hoàn thành để nhận XP và sang bài tiếp theo.
        </template>
      </span>

      <div v-if="isCompleted" class="flex items-center gap-2 shrink-0">
        <div class="px-4 py-2 bg-vdsa-green/15 text-vdsa-green border border-vdsa-green/30 rounded-xl text-xs font-bold flex items-center gap-2 select-none">
          <BaseIcon name="check-circle" class="w-4 h-4 text-vdsa-green" />
          <span>Đã hoàn thành</span>
        </div>
        <button
          @click="$emit('completeStep')"
          class="px-3 py-2 rounded-xl text-xs font-medium text-vdsa-muted hover:text-white hover:bg-vdsa-hover border border-vdsa-border transition-colors cursor-pointer"
          title="Bấm để đồng bộ lại tiến độ hoàn thành lên máy chủ nếu bài sau chưa mở khóa"
        >
          <span>Đồng bộ lại</span>
        </button>
      </div>

      <button
        v-else
        @click="$emit('completeStep')"
        class="px-5 py-2.5 rounded-xl text-xs font-bold transition-all flex items-center gap-2 shrink-0 select-none cursor-pointer bg-vdsa-accent hover:bg-vdsa-accent-dark text-white shadow-lg shadow-vdsa-accent/30 ring-1 ring-vdsa-accent-light/50 hover:scale-[1.02]"
      >
        <span>Hoàn thành bài học</span>
        <BaseIcon name="check" class="w-4 h-4" />
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { Sparkles } from 'lucide-vue-next';
import BaseIcon from '../../../shared/components/BaseIcon.vue';
import { parseMarkdownToHtml } from '@/utils/markdownParser';
import InlineSimulationPlayer from '@/components/simulator/InlineSimulationPlayer.vue';

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

const inlineSimulationKeys = computed<string[]>(() => {
  if (!props.content) return [];
  const matches = props.content.matchAll(/\[(?:Mô phỏng|Simulation|mo phong):\s*([a-zA-Z0-9._-]+)\]/gi);
  const keys: string[] = [];
  for (const m of matches) {
    if (m[1] && !keys.includes(m[1])) keys.push(m[1]);
  }
  return keys;
});

const allSimulationKeys = computed<string[]>(() => {
  const list = [...(props.simulationKeys || [])];
  if (props.simulationKey && !list.includes(props.simulationKey)) {
    list.unshift(props.simulationKey);
  }
  for (const k of inlineSimulationKeys.value) {
    if (!list.includes(k)) list.push(k);
  }
  return list;
});

const trailingSimulations = computed<string[]>(() => {
  const explicit = inlineSimulationKeys.value;
  return allSimulationKeys.value.filter((k) => !explicit.includes(k));
});

interface ContentSegment {
  type: 'html' | 'simulation';
  html?: string;
  simKey?: string;
}

function formatTheorySegment(text: string): string {
  if (!text || !text.trim()) return '';
  const hasMarkdownPatterns = /(^|\n)#{1,6}\s|(\*\*|__)[^\n]+(\*\*|__)|```[a-zA-Z0-9_-]*\n|> \[!|\$[^$]+\$|\|[^\n]+\|/m.test(text);
  if (!hasMarkdownPatterns && /<\/?(p|h[1-6]|div|ul|ol|li|table|pre|code|strong|em|blockquote|span)[^>]*>/i.test(text)) {
    return text;
  }
  return parseMarkdownToHtml(text);
}

/**
 * Phân tách nội dung thành các phân đoạn HTML và các widget mô phỏng tương tác inline
 */
const contentSegments = computed<ContentSegment[]>(() => {
  if (!props.content) {
    return [{ type: 'html', html: '<p class="text-vdsa-muted italic">Không có nội dung lý thuyết.</p>' }];
  }

  const raw = props.content.trim();
  const segments: ContentSegment[] = [];
  const regex = /(?:<p[^>]*>\s*)?\[(?:Mô phỏng|Simulation|mo phong):\s*([a-zA-Z0-9._-]+)\](?:\s*<\/p>)?/gi;

  let lastIndex = 0;
  let match: RegExpExecArray | null;

  while ((match = regex.exec(raw)) !== null) {
    const textBefore = raw.substring(lastIndex, match.index);
    if (textBefore.trim()) {
      segments.push({
        type: 'html',
        html: formatTheorySegment(textBefore),
      });
    }

    segments.push({
      type: 'simulation',
      simKey: match[1],
    });

    lastIndex = regex.lastIndex;
  }

  const remaining = raw.substring(lastIndex);
  if (remaining.trim()) {
    segments.push({
      type: 'html',
      html: formatTheorySegment(remaining),
    });
  }

  return segments.length > 0 ? segments : [{ type: 'html', html: '<p class="text-vdsa-muted italic">Không có nội dung lý thuyết.</p>' }];
});
</script>
