<script setup lang="ts">
import { ref, reactive, watch, computed, nextTick } from 'vue';
import {
  BookOpen,
  FileText,
  Upload,
  Sparkles,
  Columns2,
  Eye,
  Edit3,
  Layers,
  Check,
  HelpCircle,
  Code,
  Table,
  List,
  Quote,
  AlertCircle,
  Plus,
} from 'lucide-vue-next';

import type { Topic } from '@/api/lessons';
import { CATALOG } from '@/engines/catalog';
import { useUiStore } from '@/stores/ui';
import { useAuthStore } from '@/stores/auth';
import { parseMarkdownToHtml, extractTitleFromMarkdown, extractDescriptionFromMarkdown } from '@/utils/markdownParser';
import { LESSON_TEMPLATES, type LessonTemplate } from '@/data/lessonTemplates';
import { formatLessonWithAi, getAiUsageRemaining } from '@/services/aiFormatService';
import Modal from '@/components/ui/Modal.vue';
import Button from '@/components/ui/Button.vue';
import Input from '@/components/ui/Input.vue';
import Badge from '@/components/ui/Badge.vue';
import ProseContent from '@/components/ui/ProseContent.vue';
import TipTapEditor from '@/components/ui/TipTapEditor.vue';

export interface LessonFormPayload {
  title: string;
  description: string;
  topicId: number;
  contentHtml: string;
  isClassOnly: boolean;
  simulationKeys: string[];
  sortOrder: number;
}

const props = defineProps<{
  open: boolean;
  editingId?: number | null;
  initialData?: Partial<LessonFormPayload> | null;
  rejectionReason?: string | null;
  topics: Topic[];
  defaultSortOrder?: number;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'save', payload: LessonFormPayload): void;
}>();

const ui = useUiStore();
const auth = useAuthStore();

const editorMode = ref<'wysiwyg' | 'markdown'>('wysiwyg');
const viewMode = ref<'split' | 'write' | 'preview'>('split');
const textareaRef = ref<HTMLTextAreaElement | null>(null);
const fileInputRef = ref<HTMLInputElement | null>(null);
const aiFormatting = ref(false);
const aiRemaining = ref(getAiUsageRemaining(auth.user?.email || 'default'));

async function handleAiFormat(): Promise<void> {
  if (!form.markdownContent.trim()) {
    ui.showToast('Vui lòng nhập nội dung bài giảng trước khi dùng AI format.', 'warning');
    return;
  }
  aiFormatting.value = true;
  try {
    const formatted = await formatLessonWithAi(form.markdownContent, auth.user?.email || 'default');
    form.markdownContent = formatted;
    aiRemaining.value = getAiUsageRemaining(auth.user?.email || 'default');
    ui.showToast('Đã định dạng bài giảng thành công bằng AI (DeepSeek)!', 'success');
  } catch (err: any) {
    ui.showToast(err.message || 'Lỗi khi định dạng AI.', 'error');
  } finally {
    aiFormatting.value = false;
  }
}

const form = reactive({
  title: '',
  description: '',
  topicId: 1,
  markdownContent: '',
  isClassOnly: false,
  simulationKeys: [] as string[],
  sortOrder: 1,
});

watch(
  () => props.open,
  (isOpen, oldOpen) => {
    if (!isOpen || isOpen === oldOpen) return;
    if (props.initialData) {
      form.title = props.initialData.title || '';
      form.description = props.initialData.description || '';
      form.topicId = props.initialData.topicId || (props.topics[0]?.id ?? 1);
      form.markdownContent = props.initialData.contentHtml || '';
      form.isClassOnly = props.initialData.isClassOnly ?? false;
      form.simulationKeys = [...(props.initialData.simulationKeys || [])];
      form.sortOrder = props.initialData.sortOrder || 1;
    } else {
      // Default new lesson with template 1
      form.title = '';
      form.description = '';
      form.topicId = props.topics[0]?.id ?? 1;
      form.markdownContent = LESSON_TEMPLATES[0].content;
      form.isClassOnly = false;
      form.simulationKeys = [];
      form.sortOrder = props.defaultSortOrder || 1;
    }
  },
  { immediate: true },
);

const livePreviewHtml = computed(() => {
  return parseMarkdownToHtml(form.markdownContent);
});

// ── Template Insertion ──
function applyTemplate(tpl: LessonTemplate): void {
  if (form.markdownContent.trim() && !confirm('Thay thế nội dung hiện tại bằng mẫu bài giảng này?')) {
    return;
  }
  form.markdownContent = tpl.content;
  const extracted = extractTitleFromMarkdown(tpl.content);
  if (extracted && !form.title.trim()) {
    form.title = extracted;
  }
  ui.showToast(`Đã áp dụng mẫu: ${tpl.name}`, 'success');
}

// ── Import File Markdown / Text / PDF ──
function triggerFileUpload(): void {
  fileInputRef.value?.click();
}

function handleFileUpload(event: Event): void {
  const target = event.target as HTMLInputElement;
  if (!target.files || target.files.length === 0) return;

  const file = target.files[0];
  const fileName = file.name.toLowerCase();

  const reader = new FileReader();
  reader.onload = (e) => {
    const text = (e.target?.result as string) || '';
    form.markdownContent = text;

    // Tự động đoán Tiêu đề và Mô tả từ file
    const extTitle = extractTitleFromMarkdown(text);
    if (extTitle) {
      form.title = extTitle;
    } else {
      form.title = file.name.replace(/\.[^/.]+$/, '');
    }

    const extDesc = extractDescriptionFromMarkdown(text);
    if (extDesc && !form.description.trim()) {
      form.description = extDesc;
    }

    ui.showToast(`Đã nạp thành công nội dung từ file "${file.name}"!`, 'success');
    target.value = '';
  };

  reader.onerror = () => {
    ui.showToast('Đọc file thất bại.', 'error');
  };

  reader.readAsText(file, 'UTF-8');
}

// ── Toolbar Formatting ──
type ToolbarAction =
  | { kind: 'wrap'; label: string; title: string; before: string; after: string; placeholder: string }
  | { kind: 'line'; label: string; title: string; prefix: string }
  | { kind: 'snippet'; label: string; title: string; text: string };

const TOOLBAR_ACTIONS: ToolbarAction[] = [
  { kind: 'line', label: 'H1', title: 'Tiêu đề lớn (# )', prefix: '# ' },
  { kind: 'line', label: 'H2', title: 'Tiêu đề mục (## )', prefix: '## ' },
  { kind: 'line', label: 'H3', title: 'Tiêu đề nhỏ (### )', prefix: '### ' },
  { kind: 'wrap', label: 'B', title: 'Chữ đậm (**text**)', before: '**', after: '**', placeholder: 'chữ đậm' },
  { kind: 'wrap', label: 'I', title: 'Chữ nghiêng (*text*)', before: '*', after: '*', placeholder: 'chữ nghiêng' },
  { kind: 'wrap', label: 'Code', title: 'Mã inline (`code`)', before: '`', after: '`', placeholder: 'mã' },
  { kind: 'snippet', label: 'C++', title: 'Code block C++', text: '```cpp\n#include <iostream>\nusing namespace std;\n\nint main() {\n    // code\n    return 0;\n}\n```\n' },
  { kind: 'snippet', label: 'Python', title: 'Code block Python', text: '```python\ndef solve(arr):\n    # Viết giải thuật\n    return arr\n```\n' },
  { kind: 'snippet', label: 'Bảng', title: 'Chèn bảng so sánh Markdown', text: '\n| Cột 1 | Cột 2 | Cột 3 |\n| :--- | :--- | :--- |\n| Giá trị 1 | Giá trị 2 | Giá trị 3 |\n' },
  { kind: 'line', label: '• List', title: 'Danh sách gạch đầu dòng (- )', prefix: '- ' },
  { kind: 'line', label: '1. List', title: 'Danh sách đánh số (1. )', prefix: '1. ' },
  { kind: 'line', label: 'NOTE', title: 'Callout ghi chú (> [!NOTE])', prefix: '> [!NOTE]\n> ' },
  { kind: 'line', label: 'TIP', title: 'Callout mẹo (> [!TIP])', prefix: '> [!TIP]\n> ' },
  { kind: 'line', label: 'WARN', title: 'Callout cảnh báo (> [!WARNING])', prefix: '> [!WARNING]\n> ' },
];

function runToolbar(action: ToolbarAction): void {
  const el = textareaRef.value;
  if (!el) return;

  if (action.kind === 'wrap') {
    const { selectionStart: start, selectionEnd: end } = el;
    const core = form.markdownContent.slice(start, end) || action.placeholder;
    form.markdownContent = form.markdownContent.slice(0, start) + action.before + core + action.after + form.markdownContent.slice(end);
    void nextTick(() => {
      el.focus();
      el.setSelectionRange(start + action.before.length, start + action.before.length + core.length);
    });
  } else if (action.kind === 'line') {
    const { selectionStart: start, selectionEnd: end } = el;
    const value = form.markdownContent;
    const lineStart = value.lastIndexOf('\n', start - 1) + 1;
    form.markdownContent = value.slice(0, lineStart) + action.prefix + value.slice(lineStart);
    void nextTick(() => {
      el.focus();
      el.setSelectionRange(start + action.prefix.length, end + action.prefix.length);
    });
  } else {
    const { selectionStart: start, selectionEnd: end } = el;
    form.markdownContent = form.markdownContent.slice(0, start) + action.text + form.markdownContent.slice(end);
    void nextTick(() => {
      el.focus();
      el.setSelectionRange(start + action.text.length, start + action.text.length);
    });
  }
}

// ── Submit ──
function handleSubmit(): void {
  if (form.title.trim().length < 3) {
    ui.showToast('Tiêu đề bài học phải từ 3 ký tự trở lên.', 'warning');
    return;
  }
  if (!form.markdownContent.trim()) {
    ui.showToast('Nội dung bài học không được để trống.', 'warning');
    return;
  }

  // Tự động convert Markdown sang HTML chuẩn
  const finalHtml = parseMarkdownToHtml(form.markdownContent);

  emit('save', {
    title: form.title.trim(),
    description: form.description.trim(),
    topicId: form.topicId,
    contentHtml: finalHtml,
    isClassOnly: form.isClassOnly,
    simulationKeys: [...form.simulationKeys],
    sortOrder: form.sortOrder,
  });
}
</script>

<template>
  <Modal
    :open="open"
    :title="editingId ? 'Chỉnh sửa bài học &amp; Nội dung' : 'Tạo bài học mới (Soạn thảo Markdown)'"
    class="max-w-6xl w-[95vw]"
    @close="emit('close')"
  >
    <form class="space-y-5" novalidate @submit.prevent="handleSubmit">
      <!-- Rejection Banner (nếu có) -->
      <div v-if="rejectionReason" class="p-3.5 rounded-xl bg-vdsa-red/10 border border-vdsa-red/40 text-xs text-vdsa-red flex items-center gap-2">
        <AlertCircle :size="16" />
        <span><strong>Bài học bị từ chối duyệt:</strong> {{ rejectionReason }} — hãy chỉnh sửa nội dung và lưu lại để gửi duyệt.</span>
      </div>

      <!-- Top Tools: Templates & File Import -->
      <div class="p-3 rounded-xl bg-vdsa-surface border border-vdsa-border flex flex-wrap items-center justify-between gap-3">
        <!-- Templates Dropdown -->
        <div class="flex items-center gap-2 flex-wrap">
          <span class="text-xs font-bold text-vdsa-secondary uppercase flex items-center gap-1.5">
            <Sparkles :size="14" class="text-vdsa-yellow" /> Mẫu bài giảng:
          </span>
          <div class="flex items-center gap-1.5 flex-wrap">
            <Button
              v-for="tpl in LESSON_TEMPLATES"
              :key="tpl.id"
              size="sm"
              variant="secondary"
              type="button"
              :title="tpl.description"
              @click="applyTemplate(tpl)"
            >
              {{ tpl.name.split('.')[1] || tpl.name }}
            </Button>
          </div>
        </div>

        <!-- Import Markdown / Text File & AI Format -->
        <div class="flex items-center gap-2">
          <input
            ref="fileInputRef"
            type="file"
            accept=".md,.txt,.markdown"
            class="hidden"
            @change="handleFileUpload"
          />
          <Button size="sm" variant="secondary" type="button" class="gap-1.5" @click="triggerFileUpload">
            <Upload :size="14" /> 📥 Nhập file Markdown (.md)
          </Button>
          <Button
            size="sm"
            variant="secondary"
            type="button"
            :disabled="aiFormatting || aiRemaining <= 0"
            class="gap-1.5 bg-purple-600/20 text-purple-300 border-purple-500/40 hover:bg-purple-600/30"
            @click="handleAiFormat"
          >
            <Sparkles :size="14" class="text-purple-400" :class="{ 'animate-spin': aiFormatting }" />
            {{ aiFormatting ? 'AI đang định dạng...' : `✨ Format AI (còn ${aiRemaining}/5)` }}
          </Button>
        </div>
      </div>

      <!-- General Meta -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div class="md:col-span-2">
          <Input v-model="form.title" label="Tiêu đề bài học" placeholder="Ví dụ: Cài đặt và Tối ưu hóa Thuật toán Quick Sort" required />
        </div>

        <div>
          <label class="block text-xs font-bold text-vdsa-secondary uppercase mb-1.5">Thuộc Chủ đề (Topic)</label>
          <select
            v-model="form.topicId"
            class="w-full bg-vdsa-surface border border-vdsa-border rounded-xl px-3 py-2.5 text-sm text-white focus:outline-none focus:border-accent"
          >
            <option v-for="t in topics" :key="t.id" :value="t.id">{{ t.name }}</option>
          </select>
        </div>
      </div>

      <Input v-model="form.description" label="Mô tả ngắn bài học" placeholder="Tóm tắt nội dung bài học trong 1-2 câu..." />

      <!-- Settings Row: Class-only & Simulations -->
      <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
        <!-- Class-only checkbox -->
        <label class="flex items-start gap-3 p-3.5 rounded-xl bg-vdsa-surface border border-vdsa-border cursor-pointer select-none">
          <input v-model="form.isClassOnly" type="checkbox" class="mt-1 accent-purple-600 rounded cursor-pointer" />
          <div>
            <span class="text-xs font-bold text-white block">
              {{ form.isClassOnly ? 'Chỉ dùng trong Lớp học riêng (Class Only)' : 'Xuất bản công khai toàn hệ thống (Public)' }}
            </span>
            <span class="text-[11px] text-vdsa-muted block mt-0.5">
              {{ form.isClassOnly ? 'Bài học chỉ hiển thị cho sinh viên thuộc các lớp được phân phối.' : 'Sẽ gửi yêu cầu duyệt tới Admin để công khai trên toàn hệ thống.' }}
            </span>
          </div>
        </label>

        <!-- Simulation selector -->
        <div class="p-3.5 rounded-xl bg-vdsa-surface border border-vdsa-border space-y-2">
          <div class="flex items-center justify-between">
            <span class="text-xs font-bold text-white uppercase">Gắn mô phỏng tương tác ({{ form.simulationKeys.length }})</span>
            <span class="text-[11px] text-vdsa-muted">Chọn từ thư viện</span>
          </div>
          <div class="max-h-20 overflow-y-auto custom-scrollbar flex flex-wrap gap-1.5">
            <label
              v-for="sim in CATALOG"
              :key="sim.key"
              class="flex items-center gap-1.5 px-2 py-1 rounded-lg border text-[11px] cursor-pointer transition-colors"
              :class="form.simulationKeys.includes(sim.key) ? 'bg-vdsa-accent/20 border-vdsa-accent text-white font-bold' : 'bg-vdsa-bg border-vdsa-border text-vdsa-muted hover:text-white'"
            >
              <input v-model="form.simulationKeys" type="checkbox" :value="sim.key" class="hidden" />
              <span>{{ sim.title }}</span>
            </label>
          </div>
        </div>
      </div>

      <!-- ═══ EDITOR STUDIO (WYSIWYG TIPTAP / MARKDOWN) ═══ -->
      <div class="space-y-2 pt-2">
        <!-- Editor Mode Switcher: WYSIWYG vs Markdown -->
        <div class="flex items-center justify-between flex-wrap gap-2 border-b border-vdsa-border pb-2">
          <div class="flex items-center gap-1.5">
            <button
              type="button"
              @click="editorMode = 'wysiwyg'"
              :class="editorMode === 'wysiwyg' ? 'bg-vdsa-accent text-white shadow' : 'bg-vdsa-surface text-vdsa-muted hover:text-white'"
              class="px-3 py-1.5 rounded-lg text-xs font-bold transition-colors flex items-center gap-1.5 border border-vdsa-border"
            >
              <Edit3 :size="13" /> Trực quan kiểu Word (TipTap)
            </button>
            <button
              type="button"
              @click="editorMode = 'markdown'"
              :class="editorMode === 'markdown' ? 'bg-vdsa-accent text-white shadow' : 'bg-vdsa-surface text-vdsa-muted hover:text-white'"
              class="px-3 py-1.5 rounded-lg text-xs font-bold transition-colors flex items-center gap-1.5 border border-vdsa-border"
            >
              <Code :size="13" /> Mã nguồn Markdown
            </button>
          </div>

          <div v-show="editorMode === 'markdown'" class="flex items-center gap-1 flex-wrap">
            <button
              v-for="act in TOOLBAR_ACTIONS"
              :key="act.label"
              type="button"
              class="px-2 py-1 rounded bg-vdsa-surface hover:bg-vdsa-hover border border-vdsa-border text-xs font-mono font-bold text-vdsa-secondary hover:text-white transition-colors"
              :title="act.title"
              @click="runToolbar(act)"
            >
              {{ act.label }}
            </button>
          </div>

          <div v-show="editorMode === 'markdown'" class="flex bg-vdsa-bg p-1 rounded-lg border border-vdsa-border text-xs">
            <button
              type="button"
              class="px-2.5 py-1 rounded font-semibold transition-colors flex items-center gap-1"
              :class="viewMode === 'write' ? 'bg-vdsa-accent text-white' : 'text-vdsa-muted hover:text-white'"
              @click="viewMode = 'write'"
            >
              <Edit3 :size="12" /> Soạn thảo
            </button>
            <button
              type="button"
              class="px-2.5 py-1 rounded font-semibold transition-colors flex items-center gap-1"
              :class="viewMode === 'split' ? 'bg-vdsa-accent text-white' : 'text-vdsa-muted hover:text-white'"
              @click="viewMode = 'split'"
            >
              <Columns2 :size="12" /> Live Split View
            </button>
            <button
              type="button"
              class="px-2.5 py-1 rounded font-semibold transition-colors flex items-center gap-1"
              :class="viewMode === 'preview' ? 'bg-vdsa-accent text-white' : 'text-vdsa-muted hover:text-white'"
              @click="viewMode = 'preview'"
            >
              <Eye :size="12" /> Xem trước
            </button>
          </div>
        </div>

        <!-- Mode 1: TipTap WYSIWYG Editor -->
        <div v-if="editorMode === 'wysiwyg'" class="w-full">
          <TipTapEditor v-model="form.markdownContent" placeholder="Bắt đầu soạn thảo lý thuyết bài giảng trực quan kiểu Word..." />
        </div>

        <!-- Mode 2: Markdown Studio Panes -->
        <div
          v-else
          class="grid gap-3"
          :class="{
            'grid-cols-1 md:grid-cols-2': viewMode === 'split',
            'grid-cols-1': viewMode !== 'split',
          }"
        >
          <!-- Left: Markdown Textarea -->
          <div v-show="viewMode === 'split' || viewMode === 'write'" class="flex flex-col">
            <textarea
              ref="textareaRef"
              v-model="form.markdownContent"
              rows="18"
              class="w-full h-[450px] bg-[#0d1117] font-mono border border-vdsa-border rounded-xl p-4 text-xs text-green-300 leading-relaxed focus:outline-none focus:border-accent resize-none custom-scrollbar"
              placeholder="Nhập nội dung bài học bằng định dạng Markdown (# Tiêu đề, ## Mục, ```cpp code...)"
            ></textarea>
          </div>

          <!-- Right: Live Preview -->
          <div
            v-show="viewMode === 'split' || viewMode === 'preview'"
            class="h-[450px] overflow-y-auto p-5 rounded-xl bg-vdsa-surface border border-vdsa-border custom-scrollbar"
          >
            <div class="text-[11px] font-bold text-vdsa-muted uppercase tracking-wider mb-3 pb-2 border-b border-vdsa-border flex items-center justify-between">
              <span>Trực quan hóa nội dung (Live Preview)</span>
              <Badge variant="secondary" size="sm">Render tức thì</Badge>
            </div>
            <ProseContent :content-html="livePreviewHtml" />
          </div>
        </div>
      </div>

      <!-- Action Buttons -->
      <div class="flex items-center justify-end gap-3 pt-3 border-t border-vdsa-border">
        <Button variant="ghost" size="md" type="button" @click="emit('close')">Hủy</Button>
        <Button variant="primary" size="md" type="submit">
          <Check :size="16" /> {{ editingId ? 'Lưu thay đổi bài học' : 'Tạo bài học' }}
        </Button>
      </div>
    </form>
  </Modal>
</template>
