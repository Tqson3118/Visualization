<template>
  <div class="tiptap-editor flex flex-col rounded-2xl border border-vdsa-border bg-vdsa-surface overflow-hidden transition-colors focus-within:border-vdsa-accent/60">
    <!-- ═══ WORD-STYLE TOOLBAR ═══ -->
    <div class="tiptap-toolbar flex flex-wrap items-center gap-1 p-2 bg-vdsa-bg border-b border-vdsa-border/80 text-xs">
      <!-- Undo / Redo -->
      <div class="flex items-center gap-0.5 pr-2 border-r border-vdsa-border/60">
        <button
          type="button"
          @click="editor?.chain().focus().undo().run()"
          :disabled="!editor?.can().undo()"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors disabled:opacity-30 disabled:cursor-not-allowed"
          title="Hoàn tác (Ctrl+Z)"
        >
          <Undo :size="15" />
        </button>
        <button
          type="button"
          @click="editor?.chain().focus().redo().run()"
          :disabled="!editor?.can().redo()"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors disabled:opacity-30 disabled:cursor-not-allowed"
          title="Làm lại (Ctrl+Y)"
        >
          <Redo :size="15" />
        </button>
      </div>

      <!-- Headings H1, H2, H3 -->
      <div class="flex items-center gap-0.5 px-2 border-r border-vdsa-border/60">
        <button
          type="button"
          @click="editor?.chain().focus().toggleHeading({ level: 1 }).run()"
          :class="{ 'bg-vdsa-accent/20 text-vdsa-accent-light font-bold': editor?.isActive('heading', { level: 1 }) }"
          class="px-2 py-1 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors font-bold"
          title="Tiêu đề 1 (H1)"
        >
          H1
        </button>
        <button
          type="button"
          @click="editor?.chain().focus().toggleHeading({ level: 2 }).run()"
          :class="{ 'bg-vdsa-accent/20 text-vdsa-accent-light font-bold': editor?.isActive('heading', { level: 2 }) }"
          class="px-2 py-1 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors font-bold"
          title="Tiêu đề 2 (H2)"
        >
          H2
        </button>
        <button
          type="button"
          @click="editor?.chain().focus().toggleHeading({ level: 3 }).run()"
          :class="{ 'bg-vdsa-accent/20 text-vdsa-accent-light font-bold': editor?.isActive('heading', { level: 3 }) }"
          class="px-2 py-1 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors font-bold"
          title="Tiêu đề 3 (H3)"
        >
          H3
        </button>
      </div>

      <!-- Text formatting: Bold, Italic, Strike, Code -->
      <div class="flex items-center gap-0.5 px-2 border-r border-vdsa-border/60">
        <button
          type="button"
          @click="editor?.chain().focus().toggleBold().run()"
          :class="{ 'bg-vdsa-accent/20 text-vdsa-accent-light': editor?.isActive('bold') }"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="In đậm (Ctrl+B)"
        >
          <Bold :size="15" />
        </button>
        <button
          type="button"
          @click="editor?.chain().focus().toggleItalic().run()"
          :class="{ 'bg-vdsa-accent/20 text-vdsa-accent-light': editor?.isActive('italic') }"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="In nghiêng (Ctrl+I)"
        >
          <Italic :size="15" />
        </button>
        <button
          type="button"
          @click="editor?.chain().focus().toggleStrike().run()"
          :class="{ 'bg-vdsa-accent/20 text-vdsa-accent-light': editor?.isActive('strike') }"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="Gạch ngang"
        >
          <Strikethrough :size="15" />
        </button>
        <button
          type="button"
          @click="editor?.chain().focus().toggleCode().run()"
          :class="{ 'bg-vdsa-accent/20 text-vdsa-accent-light': editor?.isActive('code') }"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="Inline Code"
        >
          <Code :size="15" />
        </button>
      </div>

      <!-- Lists & Quotes -->
      <div class="flex items-center gap-0.5 px-2 border-r border-vdsa-border/60">
        <button
          type="button"
          @click="editor?.chain().focus().toggleBulletList().run()"
          :class="{ 'bg-vdsa-accent/20 text-vdsa-accent-light': editor?.isActive('bulletList') }"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="Danh sách gạch đầu dòng"
        >
          <List :size="15" />
        </button>
        <button
          type="button"
          @click="editor?.chain().focus().toggleOrderedList().run()"
          :class="{ 'bg-vdsa-accent/20 text-vdsa-accent-light': editor?.isActive('orderedList') }"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="Danh sách đánh số"
        >
          <ListOrdered :size="15" />
        </button>
        <button
          type="button"
          @click="editor?.chain().focus().toggleBlockquote().run()"
          :class="{ 'bg-vdsa-accent/20 text-vdsa-accent-light': editor?.isActive('blockquote') }"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="Trích dẫn (Quote)"
        >
          <Quote :size="15" />
        </button>
        <button
          type="button"
          @click="editor?.chain().focus().toggleCodeBlock().run()"
          :class="{ 'bg-vdsa-accent/20 text-vdsa-accent-light': editor?.isActive('codeBlock') }"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="Khối mã lệnh (Code Block)"
        >
          <FileCode :size="15" />
        </button>
      </div>

      <!-- Media & Dividers -->
      <div class="flex items-center gap-0.5 px-2 border-r border-vdsa-border/60">
        <button
          type="button"
          @click="addImage"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="Chèn ảnh (URL)"
        >
          <ImageIcon :size="15" />
        </button>
        <button
          type="button"
          @click="editor?.chain().focus().setHorizontalRule().run()"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="Đường kẻ phân cách"
        >
          <Minus :size="15" />
        </button>
      </div>

      <!-- Mode switcher: WYSIWYG vs Raw Code -->
      <div class="ml-auto flex items-center gap-1">
        <button
          type="button"
          @click="showRawCode = !showRawCode"
          :class="showRawCode ? 'bg-vdsa-purple/20 text-vdsa-purple-light' : 'text-vdsa-muted hover:text-white'"
          class="px-2.5 py-1 rounded-lg text-xs font-semibold flex items-center gap-1.5 border border-vdsa-border/60 hover:bg-vdsa-hover transition-colors"
        >
          <FileText :size="13" />
          <span>{{ showRawCode ? 'Xem Trực quan' : 'Xem mã' }}</span>
        </button>
      </div>
    </div>

    <!-- ═══ EDITOR BODY ═══ -->
    <div class="tiptap-content flex-1 p-5 min-h-[300px] overflow-y-auto custom-scrollbar">
      <div v-show="!showRawCode" class="prose prose-invert max-w-none focus:outline-none min-h-[260px]">
        <EditorContent :editor="editor" />
      </div>
      <textarea
        v-show="showRawCode"
        :value="modelValue"
        @input="onRawInput"
        class="w-full h-full min-h-[260px] p-3 rounded-xl bg-vdsa-bg border border-vdsa-border font-mono text-xs text-white resize-none outline-none focus:border-vdsa-accent"
        placeholder="Nhập mã HTML hoặc Markdown..."
      ></textarea>
    </div>

    <!-- ═══ FOOTER STATS ═══ -->
    <div class="flex items-center justify-between px-4 py-2 bg-vdsa-bg/60 border-t border-vdsa-border/60 text-[11px] text-vdsa-muted">
      <div class="flex items-center gap-3">
        <span>Ký tự: <strong class="text-white">{{ charCount }}</strong></span>
        <span>Từ: <strong class="text-white">{{ wordCount }}</strong></span>
      </div>
      <span class="text-vdsa-muted/70">TipTap WYSIWYG · Auto-sync</span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onBeforeUnmount, computed } from 'vue';
import { useEditor, EditorContent } from '@tiptap/vue-3';
import StarterKit from '@tiptap/starter-kit';
import Image from '@tiptap/extension-image';
import Placeholder from '@tiptap/extension-placeholder';
import {
  Bold,
  Italic,
  Strikethrough,
  Code,
  List,
  ListOrdered,
  Quote,
  FileCode,
  Image as ImageIcon,
  Minus,
  Undo,
  Redo,
  FileText,
} from 'lucide-vue-next';

const props = withDefaults(
  defineProps<{
    modelValue: string;
    placeholder?: string;
    editable?: boolean;
  }>(),
  {
    modelValue: '',
    placeholder: 'Bắt đầu soạn thảo lý thuyết bài giảng trực quan...',
    editable: true,
  },
);

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void;
}>();

const showRawCode = ref(false);
let isInternalUpdate = false;

const editor = useEditor({
  content: props.modelValue,
  editable: props.editable,
  extensions: [
    StarterKit.configure({
      heading: {
        levels: [1, 2, 3],
      },
    }),
    Image.configure({
      inline: true,
      allowBase64: true,
    }),
    Placeholder.configure({
      placeholder: props.placeholder,
    }),
  ],
  editorProps: {
    attributes: {
      class: 'focus:outline-none min-h-[250px] leading-relaxed text-sm text-vdsa-secondary',
    },
  },
  onUpdate: ({ editor }) => {
    isInternalUpdate = true;
    const html = editor.getHTML();
    emit('update:modelValue', html);
    queueMicrotask(() => {
      isInternalUpdate = false;
    });
  },
});

// Đồng bộ khi modelValue thay đổi từ bên ngoài (e.g. nạp dữ liệu, AI format, template)
watch(
  () => props.modelValue,
  (newVal) => {
    if (isInternalUpdate) return;
    if (!editor.value) return;
    const isSame = editor.value.getHTML() === newVal;
    if (!isSame) {
      editor.value.commands.setContent(newVal, { emitUpdate: false });
    }
  },
);

watch(
  () => props.editable,
  (canEdit) => {
    editor.value?.setEditable(canEdit);
  },
);

function addImage(): void {
  const url = window.prompt('Nhập đường dẫn hình ảnh (URL):');
  if (url && editor.value) {
    editor.value.chain().focus().setImage({ src: url }).run();
  }
}

function onRawInput(event: Event): void {
  const value = (event.target as HTMLTextAreaElement).value;
  isInternalUpdate = true;
  emit('update:modelValue', value);
  if (editor.value) {
    editor.value.commands.setContent(value, { emitUpdate: false });
  }
  queueMicrotask(() => {
    isInternalUpdate = false;
  });
}

const charCount = computed(() => {
  if (!editor.value) return 0;
  return editor.value.getText().length;
});

const wordCount = computed(() => {
  if (!editor.value) return 0;
  const text = editor.value.getText().trim();
  return text ? text.split(/\s+/).length : 0;
});

defineExpose({
  editor,
  getText: () => editor.value?.getText() || '',
  getHTML: () => editor.value?.getHTML() || '',
});

onBeforeUnmount(() => {
  editor.value?.destroy();
});
</script>

<style>
/* TipTap Prose Custom Styling */
.tiptap-content .ProseMirror p.is-editor-empty:first-child::before {
  content: attr(data-placeholder);
  float: left;
  color: #64748b;
  pointer-events: none;
  height: 0;
}

.tiptap-content .ProseMirror h1 {
  font-size: 1.65rem;
  font-weight: 800;
  color: #ffffff;
  margin-top: 1.25rem;
  margin-bottom: 0.75rem;
}

.tiptap-content .ProseMirror h2 {
  font-size: 1.35rem;
  font-weight: 700;
  color: #f1f5f9;
  margin-top: 1rem;
  margin-bottom: 0.5rem;
}

.tiptap-content .ProseMirror h3 {
  font-size: 1.15rem;
  font-weight: 700;
  color: #e2e8f0;
  margin-top: 0.75rem;
  margin-bottom: 0.5rem;
}

.tiptap-content .ProseMirror ul {
  list-style-type: disc;
  padding-left: 1.5rem;
  margin-top: 0.5rem;
  margin-bottom: 0.5rem;
}

.tiptap-content .ProseMirror ol {
  list-style-type: decimal;
  padding-left: 1.5rem;
  margin-top: 0.5rem;
  margin-bottom: 0.5rem;
}

.tiptap-content .ProseMirror blockquote {
  border-left: 3px solid #a855f7;
  padding-left: 1rem;
  color: #cbd5e1;
  font-style: italic;
  margin-top: 0.75rem;
  margin-bottom: 0.75rem;
}

.tiptap-content .ProseMirror pre {
  background: #0f172a;
  border: 1px solid #334155;
  border-radius: 0.75rem;
  padding: 0.75rem 1rem;
  font-family: monospace;
  color: #38bdf8;
  margin-top: 0.75rem;
  margin-bottom: 0.75rem;
}

.tiptap-content .ProseMirror code {
  background: #1e293b;
  color: #f59e0b;
  padding: 0.15rem 0.35rem;
  border-radius: 0.25rem;
  font-family: monospace;
  font-size: 0.85em;
}

.tiptap-content .ProseMirror img {
  max-width: 100%;
  border-radius: 0.75rem;
  margin-top: 0.75rem;
  margin-bottom: 0.75rem;
}
</style>
