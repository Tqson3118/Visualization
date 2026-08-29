<script setup lang="ts">
import { computed, ref } from 'vue';
import {
  Columns2,
  Eye,
  FileCode,
  PenTool,
  Sparkles,
  Table,
  Upload,
} from 'lucide-vue-next';

import { LESSON_TEMPLATES, type LessonTemplate } from '@/data/lessonTemplates';
import { formatLessonWithAi, getAiUsageRemaining } from '@/services/aiFormatService';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import { parseMarkdownToHtml } from '@/utils/markdownParser';
import Button from '@/components/ui/Button.vue';
import TipTapEditor from '@/components/ui/TipTapEditor.vue';

const props = defineProps<{
  modelValue: string;
}>();

const emit = defineEmits<{
  (e: 'update:modelValue', val: string): void;
  (e: 'templateApplied', tpl: { title?: string; description?: string }): void;
}>();

const auth = useAuthStore();
const ui = useUiStore();

const editorType = ref<'wysiwyg' | 'markdown'>('wysiwyg');
const viewMode = ref<'split' | 'editor' | 'preview'>('split');
const textareaRef = ref<HTMLTextAreaElement | null>(null);

const aiFormatting = ref(false);
const aiRemaining = ref(getAiUsageRemaining(auth.user?.email || 'default'));

const renderedPreviewHtml = computed(() => parseMarkdownToHtml(props.modelValue));

function updateContent(val: string): void {
  emit('update:modelValue', val);
}

function insertFormatting(prefix: string, suffix: string = '', defaultPlaceholder: string = ''): void {
  const el = textareaRef.value;
  if (!el) {
    updateContent(`${props.modelValue}${prefix}${defaultPlaceholder}${suffix}`);
    return;
  }

  const start = el.selectionStart;
  const end = el.selectionEnd;
  const text = props.modelValue;
  const selected = text.substring(start, end) || defaultPlaceholder;
  const replacement = `${prefix}${selected}${suffix}`;

  updateContent(text.substring(0, start) + replacement + text.substring(end));

  setTimeout(() => {
    el.focus();
    el.setSelectionRange(start + prefix.length, start + prefix.length + selected.length);
  }, 0);
}

function insertSimulationAnchor(simKey: string): void {
  const anchor = `\n\n[Mô phỏng: ${simKey}]\n\n`;
  const el = textareaRef.value;
  if (!el) {
    updateContent(`${props.modelValue}${anchor}`);
    return;
  }
  const start = el.selectionStart;
  const text = props.modelValue;
  updateContent(text.substring(0, start) + anchor + text.substring(start));
  setTimeout(() => {
    el.focus();
    el.setSelectionRange(start + anchor.length, start + anchor.length);
  }, 0);
}

function applyTemplate(tpl: LessonTemplate): void {
  if (props.modelValue && props.modelValue.trim().length > 50) {
    if (!confirm('Nội dung hiện tại sẽ được thay thế bằng mẫu mới. Bạn có chắc chắn không?')) {
      return;
    }
  }
  updateContent(tpl.content);
  emit('templateApplied', { title: tpl.name, description: tpl.description });
  ui.showToast(`Đã áp dụng mẫu: "${tpl.name}"`, 'success');
}

function triggerFileInput(): void {
  const input = document.createElement('input');
  input.type = 'file';
  input.accept = '.md,.txt,.markdown';
  input.onchange = (e) => {
    const file = (e.target as HTMLInputElement).files?.[0];
    if (!file) return;

    const reader = new FileReader();
    reader.onload = (event) => {
      const content = event.target?.result as string;
      if (content) {
        updateContent(content);
        const h1Match = content.match(/^#\s+(.+)$/m);
        const title = h1Match && h1Match[1] ? h1Match[1].trim() : undefined;
        const pMatch = content.match(/^(?!#|>|```|\||-|\d+\.)([A-ZÀ-Ỹa-zà-ỹ0-9\s,.\-—–()]{20,200})$/m);
        const description = pMatch && pMatch[1] ? pMatch[1].trim() : undefined;
        emit('templateApplied', { title, description });
        ui.showToast(`Đã nhập thành công file "${file.name}"!`, 'success');
      }
    };
    reader.readAsText(file);
  };
  input.click();
}

async function handleAiFormat(): Promise<void> {
  if (!props.modelValue.trim()) {
    ui.showToast('Vui lòng nhập nội dung bài giảng trước khi dùng AI format.', 'warning');
    return;
  }
  aiFormatting.value = true;
  try {
    const formatted = await formatLessonWithAi(props.modelValue, auth.user?.email || 'default');
    updateContent(formatted);
    aiRemaining.value = getAiUsageRemaining(auth.user?.email || 'default');
    ui.showToast('Đã định dạng bài giảng thành công bằng AI (DeepSeek)!', 'success');
  } catch (err: any) {
    ui.showToast(err.message || 'Lỗi khi định dạng AI.', 'error');
  } finally {
    aiFormatting.value = false;
  }
}

defineExpose({
  insertFormatting,
  insertSimulationAnchor,
});
</script>

<template>
  <div class="theory-tab flex flex-col h-full overflow-hidden">
    <!-- Top Action Bar for Theory -->
    <div class="flex items-center justify-between px-4 py-2.5 bg-vdsa-surface border-b border-vdsa-border flex-wrap gap-2 shrink-0">
      <!-- Mode Switcher -->
      <div class="flex items-center gap-2">
        <button
          type="button"
          @click="editorType = 'wysiwyg'"
          :class="editorType === 'wysiwyg' ? 'bg-vdsa-accent text-white shadow-md' : 'bg-vdsa-bg-secondary text-vdsa-secondary hover:text-white'"
          class="px-3 py-1.5 rounded-xl text-xs font-bold transition-colors flex items-center gap-1.5 border border-vdsa-border cursor-pointer"
        >
          <PenTool :size="13" /> Trực quan (TipTap)
        </button>
        <button
          type="button"
          @click="editorType = 'markdown'"
          :class="editorType === 'markdown' ? 'bg-vdsa-accent text-white shadow-md' : 'bg-vdsa-bg-secondary text-vdsa-secondary hover:text-white'"
          class="px-3 py-1.5 rounded-xl text-xs font-bold transition-colors flex items-center gap-1.5 border border-vdsa-border cursor-pointer"
        >
          <FileCode :size="13" /> Markdown (Split View)
        </button>
      </div>

      <div class="flex items-center gap-2">
        <!-- Import Markdown -->
        <Button variant="secondary" size="sm" class="gap-1.5 text-xs" @click="triggerFileInput">
          <Upload :size="13" /> Nhập file .md
        </Button>

        <!-- Templates Dropdown -->
        <div class="relative group">
          <Button variant="secondary" size="sm" class="gap-1.5 text-xs">
            <Sparkles :size="13" /> Mẫu bài giảng ▾
          </Button>
          <div class="hidden group-hover:block absolute right-0 top-full mt-1 w-64 bg-vdsa-surface border border-vdsa-border rounded-xl shadow-2xl p-1.5 z-50">
            <button
              v-for="tpl in LESSON_TEMPLATES"
              :key="tpl.id"
              type="button"
              class="w-full text-left p-2.5 rounded-lg hover:bg-vdsa-accent/20 transition-colors cursor-pointer"
              @click="applyTemplate(tpl)"
            >
              <span class="font-bold text-white text-xs block">{{ tpl.name }}</span>
              <span class="text-[11px] text-vdsa-muted block mt-0.5">{{ tpl.description }}</span>
            </button>
          </div>
        </div>

        <!-- View Mode Switcher for Markdown -->
        <div v-if="editorType === 'markdown'" class="inline-flex bg-vdsa-bg-secondary border border-vdsa-border rounded-lg p-0.5">
          <button
            type="button"
            class="px-2.5 py-1 rounded text-xs font-medium transition-colors flex items-center gap-1 cursor-pointer"
            :class="viewMode === 'split' ? 'bg-vdsa-surface text-white' : 'text-vdsa-muted hover:text-white'"
            @click="viewMode = 'split'"
            title="Song song"
          >
            <Columns2 :size="13" />
          </button>
          <button
            type="button"
            class="px-2.5 py-1 rounded text-xs font-medium transition-colors flex items-center gap-1 cursor-pointer"
            :class="viewMode === 'editor' ? 'bg-vdsa-surface text-white' : 'text-vdsa-muted hover:text-white'"
            @click="viewMode = 'editor'"
            title="Chỉ soạn thảo"
          >
            <PenTool :size="13" />
          </button>
          <button
            type="button"
            class="px-2.5 py-1 rounded text-xs font-medium transition-colors flex items-center gap-1 cursor-pointer"
            :class="viewMode === 'preview' ? 'bg-vdsa-surface text-white' : 'text-vdsa-muted hover:text-white'"
            @click="viewMode = 'preview'"
            title="Chỉ xem trước"
          >
            <Eye :size="13" />
          </button>
        </div>

        <!-- AI Format Button -->
        <button
          type="button"
          :disabled="aiFormatting || aiRemaining <= 0"
          class="bg-purple-600/30 text-purple-300 border border-purple-500/50 hover:bg-purple-600/50 px-3 py-1 rounded-xl text-xs font-bold flex items-center gap-1.5 transition-colors cursor-pointer disabled:opacity-50"
          :title="aiRemaining <= 0 ? 'Đã hết lượt dùng AI miễn phí' : 'Chuẩn hóa bằng DeepSeek AI'"
          @click="handleAiFormat"
        >
          <Sparkles :size="12" class="text-purple-400" :class="{ 'animate-spin': aiFormatting }" />
          {{ aiFormatting ? 'Đang format...' : `✨ Format AI (${aiRemaining}/5)` }}
        </button>
      </div>
    </div>

    <!-- Mode 1: TipTap WYSIWYG -->
    <div v-if="editorType === 'wysiwyg'" class="flex-1 overflow-y-auto p-4 bg-vdsa-bg">
      <TipTapEditor :model-value="modelValue" @update:model-value="updateContent" placeholder="Bắt đầu soạn thảo lý thuyết bài giảng trực quan..." />
    </div>

    <!-- Mode 2: Markdown Split / Panes -->
    <div v-else class="flex-1 flex flex-col overflow-hidden">
      <!-- Toolbar -->
      <div class="flex items-center gap-2 px-4 py-2 bg-vdsa-surface/90 border-b border-vdsa-border overflow-x-auto shrink-0 flex-wrap">
        <div class="flex items-center gap-1 pr-2 border-r border-vdsa-border">
          <button type="button" class="px-2 py-1 rounded bg-vdsa-bg-secondary text-vdsa-secondary hover:text-white text-xs font-bold cursor-pointer" @click="insertFormatting('# ', '', 'Tiêu đề 1')">H1</button>
          <button type="button" class="px-2 py-1 rounded bg-vdsa-bg-secondary text-vdsa-secondary hover:text-white text-xs font-bold cursor-pointer" @click="insertFormatting('## ', '', 'Tiêu đề 2')">H2</button>
          <button type="button" class="px-2 py-1 rounded bg-vdsa-bg-secondary text-vdsa-secondary hover:text-white text-xs font-bold cursor-pointer" @click="insertFormatting('### ', '', 'Tiêu đề 3')">H3</button>
        </div>

        <div class="flex items-center gap-1 pr-2 border-r border-vdsa-border">
          <button type="button" class="px-2 py-1 rounded bg-vdsa-bg-secondary text-vdsa-secondary hover:text-white text-xs font-bold cursor-pointer" @click="insertFormatting('**', '**', 'văn bản đậm')"><b>B</b></button>
          <button type="button" class="px-2 py-1 rounded bg-vdsa-bg-secondary text-vdsa-secondary hover:text-white text-xs font-bold cursor-pointer" @click="insertFormatting('*', '*', 'văn bản nghiêng')"><i>I</i></button>
          <button type="button" class="px-2 py-1 rounded bg-vdsa-bg-secondary text-vdsa-secondary hover:text-white text-xs font-bold cursor-pointer" @click="insertFormatting('`', '`', 'code')">&lt;/&gt;</button>
        </div>

        <div class="flex items-center gap-1 pr-2 border-r border-vdsa-border">
          <button type="button" class="px-2 py-1 rounded bg-vdsa-bg-secondary text-sky-400 hover:text-white text-xs font-bold cursor-pointer" @click="insertFormatting('```cpp\n// Viết mã C++\nvoid solution() {\n    \n}\n', '```')">C++</button>
          <button type="button" class="px-2 py-1 rounded bg-vdsa-bg-secondary text-emerald-400 hover:text-white text-xs font-bold cursor-pointer" @click="insertFormatting('```python\n# Viết mã Python\ndef solution():\n    pass\n', '```')">Python</button>
          <button type="button" class="px-2 py-1 rounded bg-vdsa-bg-secondary text-vdsa-secondary hover:text-white text-xs font-bold flex items-center gap-1 cursor-pointer" @click="insertFormatting('\n| Cột 1 | Cột 2 | Cột 3 |\n| :--- | :--- | :--- |\n| A | B | C |\n', '')">
            <Table :size="12" /> Bảng
          </button>
        </div>

        <div class="flex items-center gap-1">
          <button type="button" class="px-2 py-1 rounded bg-blue-500/10 text-blue-300 hover:bg-blue-500/20 text-xs font-semibold cursor-pointer" @click="insertFormatting('> [!NOTE]\n> ', '', 'Ghi chú...')">📌 Note</button>
          <button type="button" class="px-2 py-1 rounded bg-emerald-500/10 text-emerald-300 hover:bg-emerald-500/20 text-xs font-semibold cursor-pointer" @click="insertFormatting('> [!TIP]\n> ', '', 'Mẹo hay...')">💡 Tip</button>
          <button type="button" class="px-2 py-1 rounded bg-amber-500/10 text-amber-300 hover:bg-amber-500/20 text-xs font-semibold cursor-pointer" @click="insertFormatting('> [!WARNING]\n> ', '', 'Chú ý...')">⚠️ Warn</button>
        </div>
      </div>

      <!-- Editor Panes (LE1: 2-way overflow scroll) -->
      <div class="flex-1 grid overflow-hidden" :class="viewMode === 'split' ? 'grid-cols-2' : 'grid-cols-1'">
        <!-- Editor Pane -->
        <div v-show="viewMode === 'split' || viewMode === 'editor'" class="flex flex-col h-full border-r border-vdsa-border overflow-hidden">
          <div class="px-4 py-1.5 bg-vdsa-surface border-b border-vdsa-border flex items-center justify-between text-[11px] text-vdsa-muted font-bold">
            <span class="flex items-center gap-1.5 text-vdsa-purple-light"><FileCode :size="12" /> Markdown Source</span>
            <span>{{ modelValue.length }} ký tự</span>
          </div>
          <textarea
            ref="textareaRef"
            :value="modelValue"
            @input="updateContent(($event.target as HTMLTextAreaElement).value)"
            class="flex-1 w-full bg-[#0d1117] text-[#e6edf3] font-mono text-sm leading-relaxed p-4 border-none outline-none resize-none overflow-y-auto"
            placeholder="Viết nội dung bài học bằng Markdown..."
            spellcheck="false"
          />
        </div>

        <!-- Preview Pane -->
        <div v-show="viewMode === 'split' || viewMode === 'preview'" class="flex flex-col h-full bg-[#0d1117] overflow-hidden">
          <div class="px-4 py-1.5 bg-vdsa-surface border-b border-vdsa-border flex items-center justify-between text-[11px] text-vdsa-muted font-bold">
            <span class="flex items-center gap-1.5 text-emerald-400"><Eye :size="12" /> Live Preview</span>
            <span class="text-[10px]">Tự động đồng bộ</span>
          </div>
          <div class="flex-1 p-6 overflow-y-auto prose-vdsa" v-html="renderedPreviewHtml" />
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.prose-vdsa :deep(h1) {
  font-size: 1.5rem;
  font-weight: 900;
  color: #fff;
  margin-bottom: 0.75rem;
}
.prose-vdsa :deep(h2) {
  font-size: 1.25rem;
  font-weight: 800;
  color: #fff;
  margin-top: 1.5rem;
  margin-bottom: 0.5rem;
}
.prose-vdsa :deep(h3) {
  font-size: 1rem;
  font-weight: 700;
  color: #c4b5fd;
  margin-top: 1rem;
  margin-bottom: 0.5rem;
}
.prose-vdsa :deep(p) {
  font-size: 0.875rem;
  line-height: 1.65;
  color: #cbd5e1;
  margin-bottom: 0.75rem;
}
</style>
