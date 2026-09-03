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
          @click="handleToggleHeading(1)"
          :class="{ 'bg-vdsa-accent/20 text-vdsa-accent-light font-bold': editor?.isActive('heading', { level: 1 }) }"
          class="px-2 py-1 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors font-bold"
          title="Tiêu đề 1 (H1)"
        >
          H1
        </button>
        <button
          type="button"
          @click="handleToggleHeading(2)"
          :class="{ 'bg-vdsa-accent/20 text-vdsa-accent-light font-bold': editor?.isActive('heading', { level: 2 }) }"
          class="px-2 py-1 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors font-bold"
          title="Tiêu đề 2 (H2)"
        >
          H2
        </button>
        <button
          type="button"
          @click="handleToggleHeading(3)"
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
          @click="handleToggleBulletList"
          :class="{ 'bg-vdsa-accent/20 text-vdsa-accent-light': editor?.isActive('bulletList') }"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="Danh sách gạch đầu dòng"
        >
          <List :size="15" />
        </button>
        <button
          type="button"
          @click="handleToggleOrderedList"
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
          @click="insertTable"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="Chèn bảng (3x3)"
        >
          <TableIcon :size="15" />
        </button>
        <button
          type="button"
          @click="openImageModal"
          class="p-1.5 rounded-lg text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="Chèn ảnh (Tải file hoặc URL)"
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

      <!-- Table Controls (appears when cursor is inside a table) -->
      <div v-if="editor?.isActive('table')" class="flex items-center gap-0.5 px-2 border-r border-vdsa-border/60 bg-vdsa-surface/50 rounded-lg py-0.5">
        <button
          type="button"
          @click="editor?.chain().focus().addColumnAfter().run()"
          class="px-1.5 py-1 rounded text-[11px] text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="Thêm cột bên phải"
        >
          +Cột
        </button>
        <button
          type="button"
          @click="editor?.chain().focus().deleteColumn().run()"
          class="px-1.5 py-1 rounded text-[11px] text-rose-400 hover:text-rose-300 hover:bg-rose-500/10 transition-colors"
          title="Xóa cột"
        >
          -Cột
        </button>
        <button
          type="button"
          @click="editor?.chain().focus().addRowAfter().run()"
          class="px-1.5 py-1 rounded text-[11px] text-vdsa-muted hover:text-white hover:bg-vdsa-hover transition-colors"
          title="Thêm dòng bên dưới"
        >
          +Dòng
        </button>
        <button
          type="button"
          @click="editor?.chain().focus().deleteRow().run()"
          class="px-1.5 py-1 rounded text-[11px] text-rose-400 hover:text-rose-300 hover:bg-rose-500/10 transition-colors"
          title="Xóa dòng"
        >
          -Dòng
        </button>
        <button
          type="button"
          @click="editor?.chain().focus().deleteTable().run()"
          class="px-1.5 py-1 rounded text-[11px] text-rose-400 hover:text-rose-300 hover:bg-rose-500/10 transition-colors font-bold"
          title="Xóa toàn bộ bảng"
        >
          Xóa bảng
        </button>
      </div>
    </div>

    <!-- ═══ EDITOR BODY ═══ -->
    <div class="tiptap-content flex-1 p-5 min-h-[300px] overflow-y-auto custom-scrollbar">
      <div class="prose prose-invert max-w-none focus:outline-none min-h-[260px]">
        <EditorContent :editor="editor" />
      </div>
    </div>

    <!-- ═══ FOOTER STATS ═══ -->
    <div class="flex items-center justify-between px-4 py-2 bg-vdsa-bg/60 border-t border-vdsa-border/60 text-[11px] text-vdsa-muted">
      <div class="flex items-center gap-3">
        <span>Ký tự: <strong class="text-white">{{ charCount }}</strong></span>
        <span>Từ: <strong class="text-white">{{ wordCount }}</strong></span>
      </div>
      <span class="text-vdsa-muted/70">TipTap WYSIWYG · Auto-sync</span>
    </div>
    <!-- ═══ MODAL CHÈN ẢNH (Teleport to body) ═══ -->
    <Teleport to="body">
      <div v-if="showImageModal" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/75 backdrop-blur-sm">
        <div class="bg-[#181724] border border-[#2b293d] rounded-2xl p-6 max-w-md w-full shadow-2xl space-y-4">
          <div class="flex items-center justify-between border-b border-[#2b293d] pb-3">
            <h3 class="text-sm font-extrabold text-white flex items-center gap-2">
              <ImageIcon :size="16" class="text-purple-400" />
              <span>Chèn hình ảnh</span>
            </h3>
            <button @click="showImageModal = false" class="p-1 rounded-lg text-slate-400 hover:text-white hover:bg-white/10 transition-colors">
              <CloseIcon :size="16" />
            </button>
          </div>

          <!-- Tabs: Tải từ máy / Nhập URL -->
          <div class="flex rounded-xl bg-black/30 p-1 border border-white/5">
            <button
              type="button"
              @click="imageTab = 'file'"
              class="flex-1 py-1.5 rounded-lg text-xs font-bold transition-all flex items-center justify-center gap-1.5 cursor-pointer"
              :class="imageTab === 'file' ? 'bg-purple-600 text-white shadow-md' : 'text-slate-400 hover:text-white'"
            >
              <Upload :size="13" /> Tải từ máy tính
            </button>
            <button
              type="button"
              @click="imageTab = 'url'"
              class="flex-1 py-1.5 rounded-lg text-xs font-bold transition-all flex items-center justify-center gap-1.5 cursor-pointer"
              :class="imageTab === 'url' ? 'bg-purple-600 text-white shadow-md' : 'text-slate-400 hover:text-white'"
            >
              <LinkIcon :size="13" /> Nhập link URL
            </button>
          </div>

          <!-- Tab Content: File Upload -->
          <div v-if="imageTab === 'file'" class="space-y-3">
            <div
              @click="imageFileInput?.click()"
              class="border-2 border-dashed border-[#3d3a54] hover:border-purple-500 rounded-xl p-6 text-center cursor-pointer hover:bg-purple-500/5 transition-all space-y-2"
            >
              <Upload :size="28" class="mx-auto text-purple-400 opacity-80" />
              <p class="text-xs font-bold text-white">Bấm để chọn file ảnh</p>
              <p class="text-[11px] text-slate-400">Hỗ trợ PNG, JPG, GIF, WebP (Tối đa 3MB)</p>
              <input ref="imageFileInput" type="file" accept="image/*" class="hidden" @change="handleFileSelect" />
            </div>
          </div>

          <!-- Tab Content: URL Input -->
          <div v-else class="space-y-3">
            <label class="block text-xs font-bold text-slate-300">Đường dẫn hình ảnh (URL)</label>
            <input
              v-model="imageUrlInput"
              type="url"
              placeholder="https://example.com/image.png"
              @keydown.enter.prevent="insertUrlImage"
              class="w-full px-3.5 py-2.5 rounded-xl bg-black/40 border border-[#2b293d] text-xs text-white placeholder:text-slate-500 focus:outline-none focus:border-purple-500 transition-colors"
            />
            <div class="flex justify-end gap-2 pt-2">
              <button
                type="button"
                @click="showImageModal = false"
                class="px-4 py-2 rounded-xl text-xs font-bold text-slate-400 hover:text-white hover:bg-white/5 transition-colors cursor-pointer"
              >
                Hủy
              </button>
              <button
                type="button"
                @click="insertUrlImage"
                class="px-4 py-2 rounded-xl text-xs font-bold bg-purple-600 hover:bg-purple-500 text-white shadow-md transition-colors cursor-pointer"
              >
                Chèn ảnh
              </button>
            </div>
          </div>

          <p v-if="imageError" class="text-xs text-rose-400 font-semibold">{{ imageError }}</p>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onBeforeUnmount, computed } from 'vue';
import { useEditor, EditorContent } from '@tiptap/vue-3';
import StarterKit from '@tiptap/starter-kit';
import Image from '@tiptap/extension-image';
import Placeholder from '@tiptap/extension-placeholder';
import { Table, TableRow, TableCell, TableHeader } from '@tiptap/extension-table';
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
  Table as TableIcon,
  Minus,
  Undo,
  Redo,
  FileText,
  Upload,
  Link as LinkIcon,
  X as CloseIcon,
} from 'lucide-vue-next';
import { CATALOG } from '@/engines/catalog';

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

const showImageModal = ref(false);
const imageTab = ref<'file' | 'url'>('file');
const imageUrlInput = ref('');
const imageFileInput = ref<HTMLInputElement | null>(null);
const imageError = ref('');
let isInternalUpdate = false;

function handleToggleHeading(level: 1 | 2 | 3) {
  if (!editor.value) return;
  const ed = editor.value.chain().focus();
  if (editor.value.isActive('bulletList')) {
    ed.toggleBulletList();
  }
  if (editor.value.isActive('orderedList')) {
    ed.toggleOrderedList();
  }
  ed.toggleHeading({ level }).run();
}

function handleToggleBulletList() {
  if (!editor.value) return;
  const ed = editor.value.chain().focus();
  if (editor.value.isActive('orderedList')) {
    ed.toggleOrderedList();
  }
  if (editor.value.isActive('heading')) {
    ed.setParagraph();
  }
  ed.toggleBulletList().run();
}

function handleToggleOrderedList() {
  if (!editor.value) return;
  const ed = editor.value.chain().focus();
  if (editor.value.isActive('bulletList')) {
    ed.toggleBulletList();
  }
  if (editor.value.isActive('heading')) {
    ed.setParagraph();
  }
  ed.toggleOrderedList().run();
}

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
    Table.configure({
      resizable: true,
    }),
    TableRow,
    TableHeader,
    TableCell,
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

function insertSimulation(simKey: string): void {
  const sim = CATALOG.find((c) => c.key === simKey);
  const title = sim ? sim.title : simKey;
  const content = `<p><strong>🎮 Mô phỏng trực quan: ${title}</strong></p><p>[Mô phỏng: ${simKey}]</p>`;
  editor.value?.chain().focus().insertContent(content).run();
}

function insertTable(): void {
  if (editor.value) {
    editor.value.chain().focus().insertTable({ rows: 3, cols: 3, withHeaderRow: true }).run();
  }
}

function openImageModal(): void {
  imageTab.value = 'file';
  imageUrlInput.value = '';
  imageError.value = '';
  showImageModal.value = true;
}

function addImage(): void {
  openImageModal();
}

function handleFileSelect(e: Event): void {
  imageError.value = '';
  const file = (e.target as HTMLInputElement).files?.[0];
  if (!file) return;
  if (file.size > 3 * 1024 * 1024) {
    imageError.value = 'Kích thước ảnh tối đa là 3MB.';
    return;
  }
  const reader = new FileReader();
  reader.onload = () => {
    const base64 = reader.result as string;
    if (base64 && editor.value) {
      editor.value.chain().focus().setImage({ src: base64 }).run();
      showImageModal.value = false;
    }
  };
  reader.readAsDataURL(file);
}

function insertUrlImage(): void {
  imageError.value = '';
  const url = imageUrlInput.value.trim();
  if (!url) {
    imageError.value = 'Vui lòng nhập đường dẫn hình ảnh.';
    return;
  }
  if (editor.value) {
    editor.value.chain().focus().setImage({ src: url }).run();
    showImageModal.value = false;
  }
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
  insertTable,
  insertSimulation,
});

onBeforeUnmount(() => {
  editor.value?.destroy();
});
</script>

<style>
/* TipTap Prose Custom Styling */
.tiptap-content,
.tiptap-content .prose,
.tiptap-content .ProseMirror {
  width: 100% !important;
  max-width: 100% !important;
}

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

/* TipTap Tables Custom Styling */
.tiptap-content .ProseMirror table {
  border-collapse: collapse;
  margin: 1.25rem 0;
  table-layout: fixed;
  width: 100%;
  overflow: hidden;
  border-radius: 0.75rem;
  border: 1px solid rgba(255, 255, 255, 0.12);
}

.tiptap-content .ProseMirror table td,
.tiptap-content .ProseMirror table th {
  min-width: 1em;
  padding: 0.625rem 0.75rem;
  vertical-align: top;
  box-sizing: border-box;
  position: relative;
  border: 1px solid rgba(255, 255, 255, 0.12);
  font-size: 0.85rem;
}

.tiptap-content .ProseMirror table th {
  font-weight: bold;
  text-align: left;
  background-color: rgba(30, 27, 46, 0.9);
  color: #c4b5fd;
}

.tiptap-content .ProseMirror table td {
  color: #cbd5e1;
  background-color: rgba(13, 17, 23, 0.5);
}

.tiptap-content .ProseMirror table tr:hover td {
  background-color: rgba(168, 85, 247, 0.08);
}

.tiptap-content .ProseMirror table .selectedCell:after {
  z-index: 2;
  position: absolute;
  content: "";
  left: 0; right: 0; top: 0; bottom: 0;
  background: rgba(168, 85, 247, 0.25);
  pointer-events: none;
}

.tiptap-content .ProseMirror table .column-resize-handle {
  position: absolute;
  right: -2px;
  top: 0;
  bottom: 0;
  width: 4px;
  background-color: #a855f7;
  pointer-events: none;
}

.tiptap-content .tableWrapper {
  overflow-x: auto;
  margin: 1rem 0;
}

.tiptap-content .resize-cursor {
  cursor: ew-resize;
  cursor: col-resize;
}

select option,
select optgroup {
  background-color: #171527 !important;
  color: #f1f5f9 !important;
}

select optgroup {
  color: #c084fc !important;
  font-weight: 700;
}
</style>
