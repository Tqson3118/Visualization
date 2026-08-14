<script setup lang="ts">
// AdminContentView — Màn 09: quản lý lessons/topics CRUD cơ bản + gắn mô phỏng
// View-quality 14/08 (Nhóm D): banner surface band + mono strip block-token
// (số bài học/chủ đề — dữ liệu thật); bảng §4.6 + mobile card-stack; SỬA BUG
// cột "Ngày tạo" hiển thị formatDate(new Date()) (LessonSummary không có
// createdAt) → cột index mono #01; topic card bỏ gradient/hover-lift;
// error state + retry.
// v2.15: Smart Markdown editor (2 tab Soạn thảo/Xem trước + toolbar định dạng),
// checkbox xuất bản (Class Only ↔ Public → isClassOnly + status), multi-select
// simulationKeys, luồng kiểm duyệt (Chờ duyệt / Từ chối kèm lý do).
import { computed, nextTick, onMounted, reactive, ref } from 'vue';
import { useRouter } from 'vue-router';
import { ArrowRight, Eye, Layers, Network, Pencil, Plus, RefreshCw, Trash2 } from 'lucide-vue-next';

import * as lessonsApi from '@/api/lessons';
import type { LessonSummary, LessonUpsertRequest, Topic } from '@/api/lessons';
import { getData } from '@/api/client';
import { useUiStore } from '@/stores/ui';
import { useAuthStore } from '@/stores/auth';
import { messages } from '@/i18n/vi';
import ProseContent from '@/components/ui/ProseContent.vue';
import {
  Card,
  CardDescription,
  CardHeader,
  CardTitle,
} from '@/components/ui/card';
import AdminNav from '@/components/admin/AdminNav.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Modal from '@/components/ui/Modal.vue';
import Input from '@/components/ui/Input.vue';
import Tabs from '@/components/ui/Tabs.vue';
import PageHero from '@/components/ui/PageHero.vue';
import AdminHeroStrip from '@/components/admin/AdminHeroStrip.vue';
import { CATALOG } from '@/engines/catalog';

// ── Kiểu local theo backend v2.15 (lessons.ts chưa theo kịp — không sửa file khác) ──

type LessonStatusValue = 'draft' | 'pendingreview' | 'active' | 'hidden';

interface LessonRow extends Omit<LessonSummary, 'status'> {
  status: LessonStatusValue;
  isClassOnly: boolean;
  publishedAt: string | null;
}

interface LessonDetailRow extends LessonRow {
  contentHtml: string;
  rejectionReason: string | null;
  simulations: Array<{ simulationKey: string; title: string }>;
}

interface LessonSavePayload {
  topicId: number;
  title: string;
  description?: string;
  contentHtml: string;
  status: LessonStatusValue;
  isClassOnly: boolean;
  sortOrder?: number;
  simulationKeys: string[];
}

const ui = useUiStore();
const router = useRouter();
const auth = useAuthStore();

const tab = ref<'lessons' | 'topics'>('lessons');
const lessons = ref<LessonRow[]>([]);
const topics = ref<Topic[]>([]);
const loading = ref(true);
const loadError = ref(false);

// Form bài học
const formOpen = ref(false);
const editingId = ref<number | null>(null);
const saving = ref(false);
const form = reactive({
  title: '',
  description: '',
  topicId: 1,
  contentHtml: '',
  isClassOnly: false,
  simulationKeys: [] as string[],
  sortOrder: 1,
});

/** Lý do từ chối của bài đang sửa (backend chỉ trả khi tải chi tiết). */
const formRejectionReason = ref('');

/** Map lý do từ chối theo bài — hiển thị nhãn "Bị từ chối" trong danh sách. */
const rejectedReasons = reactive<Record<number, string>>({});

// Editor Markdown
const editorTab = ref<'write' | 'preview'>('write');
const textareaRef = ref<HTMLTextAreaElement | null>(null);

// Form topic
const topicFormOpen = ref(false);
const topicForm = reactive({ name: '', description: '', sortOrder: 0 });

// Preview bài học: reactive theo nội dung đang gõ trong modal form, hoặc nội dung đã lưu (từ list)
const previewForm = ref(false);
const previewLesson = ref<{ title: string; contentHtml: string } | null>(null);
const previewOpen = computed(() => previewForm.value || previewLesson.value !== null);
const previewTitle = computed(() => {
  if (previewForm.value) return form.title.trim() || 'Xem trước nội dung';
  return previewLesson.value?.title.trim() || 'Xem trước nội dung';
});
const previewContent = computed(() => (previewForm.value ? form.contentHtml : previewLesson.value?.contentHtml ?? ''));

/** Xem trước nội dung đang gõ trong form (reactive — cập nhật theo textarea). */
function openFormPreview(): void {
  previewForm.value = true;
  previewLesson.value = null;
}

/** Xem trước nội dung đã lưu của bài học từ danh sách (lấy qua GET /lessons/:id). */
function openLessonPreview(lesson: LessonSummary): void {
  previewForm.value = false;
  previewLesson.value = null;
  lessonsApi
    .fetchLesson(lesson.id)
    .then((detail) => {
      previewLesson.value = { title: detail.title, contentHtml: detail.contentHtml ?? '' };
    })
    .catch(() => ui.showToast('Không tải được nội dung bài học.', 'error'));
}

function closePreview(): void {
  previewForm.value = false;
  previewLesson.value = null;
}

onMounted(load);

async function load(): Promise<void> {
  loading.value = true;
  loadError.value = false;
  try {
    const [lessonPage, topicTree] = await Promise.all([
      lessonsApi.fetchLessons({}),
      lessonsApi.fetchTopics().catch(() => [] as Topic[]),
    ]);
    lessons.value = lessonPage.items.map(toRow);
    topics.value = topicTree;
    if (topicTree.length > 0 && form.topicId === 1 && !topicTree.some((t) => t.id === 1)) {
      form.topicId = topicTree[0].id;
    }
  } catch {
    loadError.value = true;
  } finally {
    loading.value = false;
  }
}

/** Backend v2.15 trả thêm isClassOnly/publishedAt — kiểu lessons.ts chưa khai báo. */
function toRow(item: LessonSummary): LessonRow {
  const extra = item as { isClassOnly?: boolean; publishedAt?: string | null };
  return {
    id: item.id,
    title: item.title,
    description: item.description,
    topicId: item.topicId,
    sortOrder: item.sortOrder,
    status: item.status as LessonStatusValue,
    isClassOnly: extra.isClassOnly ?? false,
    publishedAt: extra.publishedAt ?? null,
    simulationCount: item.simulationCount,
    exerciseCount: item.exerciseCount,
    progress: item.progress,
  };
}

async function fetchLessonDetail(id: number): Promise<LessonDetailRow> {
  const dto = await lessonsApi.fetchLesson(id);
  const extra = dto as { rejectionReason?: string | null };
  return {
    ...toRow(dto),
    contentHtml: dto.contentHtml,
    rejectionReason: extra.rejectionReason ?? null,
    simulations: dto.simulations ?? [],
  };
}

const topicName = computed(() => (id: number) => topics.value.find((t) => t.id === id)?.name ?? `#${id}`);

/** Số bài học mỗi chủ đề (tính từ danh sách lessons đã tải — presentation only). */
const topicLessonCount = computed(() => {
  const map = new Map<number, number>();
  for (const lesson of lessons.value) {
    map.set(lesson.topicId, (map.get(lesson.topicId) ?? 0) + 1);
  }
  return map;
});

const pad = (n: number): string => String(n).padStart(2, '0');

const contentTabs = computed(() => [
  { key: 'lessons', label: messages.admin.content.tabLessons, badge: lessons.value.length > 0 ? lessons.value.length : undefined },
  { key: 'topics', label: messages.admin.content.tabTopics, badge: topics.value.length > 0 ? topics.value.length : undefined },
]);

const isAdmin = computed(() => auth.role === 'ADMIN');

/** Trạng thái xuất bản theo checkbox: Class Only → active, Public → pendingreview. */
const publishStatus = computed<'active' | 'pendingreview'>(() => (form.isClassOnly ? 'active' : 'pendingreview'));

const statusLabel: Record<string, string> = {
  draft: messages.admin.content.statusDraft,
  pendingreview: 'Chờ duyệt',
  active: messages.admin.content.statusActive,
  hidden: messages.admin.content.statusHidden,
};

const statusVariant: Record<LessonStatusValue, 'success' | 'warning' | 'muted'> = {
  active: 'success',
  pendingreview: 'warning',
  draft: 'muted',
  hidden: 'muted',
};

function openCreate(): void {
  editingId.value = null;
  formRejectionReason.value = '';
  Object.assign(form, {
    title: '',
    description: '',
    topicId: topics.value[0]?.id ?? 1,
    contentHtml: '',
    isClassOnly: false,
    simulationKeys: [],
    sortOrder: lessons.value.length + 1,
  });
  editorTab.value = 'write';
  formOpen.value = true;
}

async function openEdit(lesson: LessonRow): Promise<void> {
  editingId.value = lesson.id;
  try {
    const detail = await fetchLessonDetail(lesson.id);
    formRejectionReason.value = detail.status === 'draft' ? detail.rejectionReason ?? '' : '';
    if (detail.rejectionReason) rejectedReasons[lesson.id] = detail.rejectionReason;
    Object.assign(form, {
      title: detail.title,
      description: detail.description,
      topicId: detail.topicId,
      contentHtml: detail.contentHtml,
      isClassOnly: detail.isClassOnly,
      simulationKeys: detail.simulations.map((sim) => sim.simulationKey),
      sortOrder: detail.sortOrder,
    });
    editorTab.value = 'write';
    formOpen.value = true;
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể tải bài học.', 'error');
  }
}

async function saveLesson(): Promise<void> {
  if (form.title.trim().length < 3) {
    ui.showToast('Tiêu đề phải từ 3 ký tự.', 'warning');
    return;
  }
  saving.value = true;
  try {
    // Kiểu lessons.ts chưa theo kịp backend v2.15 (status pendingreview, simulationKeys) — cast biên API.
    const payload = {
      topicId: form.topicId,
      title: form.title.trim(),
      description: form.description,
      contentHtml: form.contentHtml || '<p>Đang biên soạn...</p>',
      status: publishStatus.value,
      isClassOnly: form.isClassOnly,
      sortOrder: form.sortOrder,
      simulationKeys: [...form.simulationKeys],
    } as unknown as LessonUpsertRequest;
    if (editingId.value === null) {
      await lessonsApi.createLesson(payload);
    } else {
      await lessonsApi.updateLesson(editingId.value, payload);
    }
    ui.showToast('Đã lưu bài học.', 'success');
    formOpen.value = false;
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Lưu thất bại.', 'error');
  } finally {
    saving.value = false;
  }
}

async function deleteLesson(lesson: LessonRow): Promise<void> {
  if (!window.confirm(`Xóa bài học "${lesson.title}"? (xóa mềm — ẩn khỏi người học)`)) return;
  try {
    await lessonsApi.deleteLesson(lesson.id);
    ui.showToast('Đã xóa bài học.', 'success');
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Xóa thất bại.', 'error');
  }
}

// ── Kiểm duyệt Admin (backend v2.15: POST /lessons/{id}/review) ──

async function reviewLesson(id: number, payload: { approve: boolean; reason?: string }): Promise<{ rejectionReason?: string | null }> {
  return getData<{ rejectionReason?: string | null }>({ method: 'POST', url: `/lessons/${id}/review`, data: payload });
}

async function approveLesson(lesson: LessonRow): Promise<void> {
  if (!window.confirm(`Duyệt bài học "${lesson.title}"? Bài sẽ được xuất bản công khai.`)) return;
  try {
    await reviewLesson(lesson.id, { approve: true });
    ui.showToast('Đã duyệt bài học.', 'success');
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Duyệt thất bại.', 'error');
  }
}

async function rejectLesson(lesson: LessonRow): Promise<void> {
  const reason = window.prompt(`Lý do từ chối bài học "${lesson.title}" (bắt buộc):`);
  if (reason === null) return;
  if (!reason.trim()) {
    ui.showToast('Phải nhập lý do từ chối.', 'warning');
    return;
  }
  try {
    const result = await reviewLesson(lesson.id, { approve: false, reason: reason.trim() });
    if (result.rejectionReason) rejectedReasons[lesson.id] = result.rejectionReason;
    ui.showToast('Đã từ chối bài học.', 'success');
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Từ chối thất bại.', 'error');
  }
}

// ── Toolbar Markdown: chèn vào vị trí con trỏ textarea (v2.15) ──

type ToolbarAction =
  | { kind: 'wrap'; label: string; title: string; before: string; after: string; placeholder: string }
  | { kind: 'line'; label: string; title: string; prefix: string }
  | { kind: 'snippet'; label: string; title: string; text: string };

const TABLE_SNIPPET = [
  '| Thuật toán | Độ phức tạp | Ghi chú |',
  '| ---------- | ----------- | ------- |',
  '| Ví dụ | O(n) | Mô tả |',
].join('\n');

const toolbarActions: ToolbarAction[] = [
  { kind: 'line', label: 'H2', title: 'Tiêu đề cấp 2 (## )', prefix: '## ' },
  { kind: 'line', label: 'H3', title: 'Tiêu đề cấp 3 (### )', prefix: '### ' },
  { kind: 'wrap', label: 'B', title: 'Đậm (**chữ**)', before: '**', after: '**', placeholder: 'chữ đậm' },
  { kind: 'wrap', label: 'I', title: 'Nghiêng (*chữ*)', before: '*', after: '*', placeholder: 'chữ nghiêng' },
  { kind: 'wrap', label: 'Code', title: 'Code inline (`mã`)', before: '`', after: '`', placeholder: 'mã' },
  { kind: 'wrap', label: 'Block', title: 'Code block (```)', before: '```\n', after: '\n```', placeholder: '// code' },
  { kind: 'line', label: '• List', title: 'Danh sách gạch đầu dòng (- )', prefix: '- ' },
  { kind: 'line', label: '1. List', title: 'Danh sách đánh số (1. )', prefix: '1. ' },
  { kind: 'line', label: '❝', title: 'Trích dẫn (> )', prefix: '> ' },
  { kind: 'snippet', label: 'Bảng', title: 'Chèn bảng Markdown', text: TABLE_SNIPPET },
  { kind: 'line', label: 'NOTE', title: 'Callout ghi chú (> [!NOTE])', prefix: '> [!NOTE] ' },
  { kind: 'line', label: 'TIP', title: 'Callout mẹo (> [!TIP])', prefix: '> [!TIP] ' },
  { kind: 'line', label: 'WARN', title: 'Callout cảnh báo (> [!WARNING])', prefix: '> [!WARNING] ' },
];

function runToolbar(action: ToolbarAction): void {
  const el = textareaRef.value;
  if (!el) return;
  if (action.kind === 'wrap') {
    wrapSelection(el, action.before, action.after, action.placeholder);
  } else if (action.kind === 'line') {
    prefixLines(el, action.prefix);
  } else {
    insertSnippet(el, action.text);
  }
}

/** Bọc vùng chọn (đậm/nghiêng/code...): chưa chọn thì chèn placeholder để gõ đè. */
function wrapSelection(el: HTMLTextAreaElement, before: string, after: string, placeholder: string): void {
  const { selectionStart: start, selectionEnd: end } = el;
  const core = form.contentHtml.slice(start, end) || placeholder;
  form.contentHtml = form.contentHtml.slice(0, start) + before + core + after + form.contentHtml.slice(end);
  void nextTick(() => {
    el.focus();
    el.setSelectionRange(start + before.length, start + before.length + core.length);
  });
}

/** Chèn prefix vào đầu dòng (heading/list/quote/callout); đa dòng → prefix từng dòng. */
function prefixLines(el: HTMLTextAreaElement, prefix: string): void {
  const { selectionStart: start, selectionEnd: end } = el;
  const value = form.contentHtml;
  const lineStart = value.lastIndexOf('\n', start - 1) + 1;
  const blockEnd = value.indexOf('\n', end);
  const block = value.slice(lineStart, blockEnd === -1 ? value.length : blockEnd);
  const callout = prefix.startsWith('> [!');
  const lines = block.split('\n').map((line, idx) => {
    if (!line || line.startsWith(prefix.trim())) return line;
    return callout && idx > 0 ? `> ${line}` : prefix + line;
  });
  form.contentHtml = value.slice(0, lineStart) + lines.join('\n') + value.slice(blockEnd === -1 ? value.length : blockEnd);
  void nextTick(() => {
    el.focus();
    el.setSelectionRange(start + prefix.length, end + prefix.length);
  });
}

/** Chèn khối mẫu (bảng) và chọn ô đầu tiên để gõ nhanh. */
function insertSnippet(el: HTMLTextAreaElement, text: string): void {
  const { selectionStart: start, selectionEnd: end } = el;
  const inserted = `\n\n${text}\n`;
  form.contentHtml = form.contentHtml.slice(0, start) + inserted + form.contentHtml.slice(end);
  void nextTick(() => {
    el.focus();
    const marker = 'Ví dụ';
    const markerIdx = inserted.indexOf(marker);
    const pos = markerIdx === -1 ? start + inserted.length : start + markerIdx;
    el.setSelectionRange(pos, pos + (markerIdx === -1 ? 0 : marker.length));
  });
}

// ── Render Markdown nhẹ cho tab Xem trước (v2.15) ─────────────
// Chỉ convert khi nội dung là Markdown; nội dung đã là HTML (bài cũ) truyền nguyên vẹn
// (backend đã sanitize khi lưu).

function escapeHtml(text: string): string {
  return text.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;');
}

function inlineMd(text: string): string {
  return text
    .replace(/`([^`\n]+)`/g, (_match, code: string) => `<code>${escapeHtml(code)}</code>`)
    .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
    .replace(/(^|[^*\w])\*([^*\n]+)\*(?!\*)/g, '$1<em>$2</em>');
}

function splitTableRow(line: string): string[] {
  const body = line.trim();
  const inner = body.startsWith('|') ? body.slice(1) : body;
  const cells = inner.endsWith('|') ? inner.slice(0, -1) : inner;
  return cells.split('|').map((cell) => cell.trim());
}

function renderMarkdown(src: string): string {
  if (/<[a-z][^>]*>/i.test(src)) return src;
  const lines = src.replace(/\r\n?/g, '\n').split('\n');
  const out: string[] = [];
  let paragraph: string[] | null = null;
  const flushParagraph = (): void => {
    if (paragraph) {
      out.push(`<p>${inlineMd(paragraph.join(' '))}</p>`);
      paragraph = null;
    }
  };

  let i = 0;
  while (i < lines.length) {
    const line = lines[i];

    // Code block (```)
    if (line.trimStart().startsWith('```')) {
      flushParagraph();
      const buf: string[] = [];
      i += 1;
      while (i < lines.length && !lines[i].trimStart().startsWith('```')) {
        buf.push(lines[i]);
        i += 1;
      }
      i += 1;
      out.push(`<pre><code>${escapeHtml(buf.join('\n'))}</code></pre>`);
      continue;
    }

    // Heading
    const heading = /^(#{1,6})\s+(.*)$/.exec(line);
    if (heading) {
      flushParagraph();
      const level = heading[1].length;
      out.push(`<h${level}>${inlineMd(heading[2])}</h${level}>`);
      i += 1;
      continue;
    }

    // Bảng (dòng kế tiếp là dòng phân cách)
    if (line.includes('|') && i + 1 < lines.length && /^\s*\|?[\s:|-]+\|?\s*$/.test(lines[i + 1]) && lines[i + 1].includes('-')) {
      flushParagraph();
      const head = splitTableRow(line);
      i += 2;
      const rows: string[][] = [];
      while (i < lines.length && lines[i].includes('|')) {
        rows.push(splitTableRow(lines[i]));
        i += 1;
      }
      out.push(
        '<table><thead><tr>' +
          head.map((cell) => `<th>${inlineMd(cell)}</th>`).join('') +
          '</tr></thead><tbody>' +
          rows.map((row) => `<tr>${row.map((cell) => `<td>${inlineMd(cell)}</td>`).join('')}</tr>`).join('') +
          '</tbody></table>',
      );
      continue;
    }

    // Blockquote / callout (> [!NOTE|TIP|WARNING])
    if (line.startsWith('>')) {
      flushParagraph();
      const buf: string[] = [];
      while (i < lines.length && lines[i].startsWith('>')) {
        buf.push(lines[i].replace(/^>\s?/, ''));
        i += 1;
      }
      const callout = /^\[!(NOTE|TIP|WARNING)\]\s*(.*)$/i.exec(buf[0] ?? '');
      if (callout) {
        const type = callout[1].toLowerCase();
        const content = [callout[2], ...buf.slice(1)].filter((part) => part.length > 0).join(' ');
        out.push(`<blockquote data-callout="${type}"><p>${inlineMd(content)}</p></blockquote>`);
      } else {
        out.push(`<blockquote><p>${inlineMd(buf.filter((part) => part.length > 0).join(' '))}</p></blockquote>`);
      }
      continue;
    }

    // Danh sách gạch đầu dòng / đánh số
    const unordered = /^[-*]\s+(.*)$/.exec(line);
    const ordered = /^\d+\.\s+(.*)$/.exec(line);
    if (unordered || ordered) {
      flushParagraph();
      const pattern = ordered ? /^\d+\.\s+(.*)$/ : /^[-*]\s+(.*)$/;
      const tag = ordered ? 'ol' : 'ul';
      const items: string[] = [];
      while (i < lines.length) {
        const match = pattern.exec(lines[i]);
        if (!match) break;
        items.push(inlineMd(match[1]));
        i += 1;
      }
      out.push(`<${tag}>${items.map((item) => `<li>${item}</li>`).join('')}</${tag}>`);
      continue;
    }

    // Dòng trống → đóng đoạn
    if (line.trim() === '') {
      flushParagraph();
      i += 1;
      continue;
    }

    paragraph = paragraph ?? [];
    paragraph.push(line);
    i += 1;
  }
  flushParagraph();
  return out.join('\n');
}

const previewHtml = computed(() => renderMarkdown(form.contentHtml));

async function saveTopic(): Promise<void> {
  try {
    await lessonsApi.createTopic(topicForm);
    ui.showToast('Đã tạo chủ đề.', 'success');
    topicFormOpen.value = false;
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Tạo chủ đề thất bại.', 'error');
  }
}
</script>

<template>
  <main class="admin-content container">
    <!-- Banner: surface band level-2 (PageHero — DESIGN §1/#1: KHÔNG gradient, KHÔNG shadow) -->
    <PageHero
      :badge="messages.admin.badge"
      :title="messages.admin.content.title"
      :description="messages.admin.content.subtitle"
    >
      <!-- Mono strip: block-token dữ liệu thật (bài học/chủ đề) + index mono -->
      <template #side>
        <AdminHeroStrip
          :count="Math.min(Math.max(lessons.length, topics.length), 5)"
          :label="messages.admin.content.stripLabel(lessons.length, topics.length)"
        />
      </template>
    </PageHero>

    <AdminNav active="content" />

    <Tabs :tabs="contentTabs" :model-value="tab" @change="(key: string) => (tab = key as 'lessons' | 'topics')" />

    <div v-if="loading" class="admin-content__loading" aria-busy="true">
      <Skeleton v-for="i in 5" :key="i" height="56px" />
    </div>

    <div v-else-if="loadError" class="admin-content__error" role="alert">
      <p class="admin-content__error-text">Không thể tải nội dung (backend chưa khả dụng).</p>
      <Button size="sm" variant="secondary" @click="load">
        <RefreshCw :size="14" /> {{ messages.admin.content.retry }}
      </Button>
    </div>

    <!-- Danh sách bài học -->
    <template v-else-if="tab === 'lessons'">
      <div class="admin-content__toolbar">
        <Button size="md" @click="openCreate"><Plus :size="16" /> {{ messages.admin.content.addLesson }}</Button>
        <Button size="sm" variant="ghost" @click="router.push({ name: 'admin-ladder' })">
          {{ messages.admin.content.ladderHint }} <ArrowRight :size="16" />
        </Button>
      </div>

      <EmptyState
        v-if="lessons.length === 0"
        icon="book"
        :title="messages.admin.content.emptyLessons"
        :description="messages.admin.content.emptyLessonsDesc"
        :action-label="messages.admin.content.addLesson"
        @action="openCreate"
      />

      <div v-else class="admin-content__table">
        <div class="admin-content__table-scroll">
          <table>
            <thead>
              <tr>
                <th scope="col">{{ messages.admin.content.colIndex }}</th>
                <th scope="col">{{ messages.admin.content.colTitle }}</th>
                <th scope="col">{{ messages.admin.content.colTopic }}</th>
                <th scope="col">{{ messages.admin.content.colStatus }}</th>
                <th scope="col">{{ messages.admin.content.colSim }}</th>
                <th scope="col" class="admin-content__actions-col">{{ messages.admin.content.colActions }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="(lesson, idx) in lessons" :key="lesson.id">
                <td :data-label="messages.admin.content.colIndex" class="admin-content__idx">{{ pad(idx + 1) }}</td>
                <td :data-label="messages.admin.content.colTitle" class="admin-content__title-cell">
                  <p class="admin-content__title-text">{{ lesson.title }}</p>
                  <p v-if="lesson.description" class="admin-content__title-desc">{{ lesson.description }}</p>
                  <p v-if="lesson.status === 'draft' && rejectedReasons[lesson.id]" class="admin-content__rejected">
                    Bị từ chối: {{ rejectedReasons[lesson.id] }}
                  </p>
                </td>
                <td :data-label="messages.admin.content.colTopic"><Badge variant="secondary">{{ topicName(lesson.topicId) }}</Badge></td>
                <td :data-label="messages.admin.content.colStatus">
                  <div class="admin-content__status-cell">
                    <Badge :variant="statusVariant[lesson.status]">{{ statusLabel[lesson.status] ?? lesson.status }}</Badge>
                    <Badge v-if="lesson.isClassOnly" variant="secondary">Lớp học riêng</Badge>
                  </div>
                </td>
                <td :data-label="messages.admin.content.colSim">
                  <span class="admin-content__sim-count" :class="{ 'admin-content__sim-count--zero': lesson.simulationCount === 0 }">
                    <Network :size="13" /> {{ lesson.simulationCount }}
                  </span>
                </td>
                <td :data-label="messages.admin.content.colActions">
                  <div class="admin-content__actions">
                    <template v-if="isAdmin && lesson.status === 'pendingreview'">
                      <Button size="sm" variant="secondary" @click="approveLesson(lesson)">Duyệt</Button>
                      <Button size="sm" variant="danger" @click="rejectLesson(lesson)">Từ chối</Button>
                    </template>
                    <Button size="sm" variant="ghost" @click="openLessonPreview(lesson)">
                      <Eye :size="16" /> Xem trước
                    </Button>
                    <Button size="sm" variant="ghost" @click="openEdit(lesson)">
                      <Pencil :size="16" /> {{ messages.admin.content.edit }}
                    </Button>
                    <Button size="sm" variant="danger" @click="deleteLesson(lesson)">
                      <Trash2 :size="16" /> {{ messages.admin.content.delete }}
                    </Button>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>

    <!-- Danh sách chủ đề -->
    <template v-else>
      <div class="admin-content__toolbar">
        <Button size="md" @click="topicFormOpen = true"><Plus :size="16" /> {{ messages.admin.content.addTopic }}</Button>
      </div>

      <EmptyState
        v-if="topics.length === 0"
        icon="book"
        :title="messages.admin.content.emptyTopics"
        :description="messages.admin.content.emptyTopicsDesc"
        :action-label="messages.admin.content.addTopic"
        @action="topicFormOpen = true"
      />

      <div v-else class="admin-content__topics">
        <Card v-for="topic in topics" :key="topic.id" class="admin-content__topic">
          <CardHeader class="admin-content__topic-head">
            <span class="admin-content__topic-icon" aria-hidden="true"><Layers :size="16" /></span>
            <div class="admin-content__topic-meta">
              <CardTitle class="admin-content__topic-name">{{ topic.name }}</CardTitle>
              <CardDescription class="admin-content__topic-desc">
                {{ topic.description || '—' }}
              </CardDescription>
            </div>
            <Badge variant="secondary" class="admin-content__topic-count">
              {{ topicLessonCount.get(topic.id) ?? 0 }} {{ messages.admin.content.lessonsCount }}
            </Badge>
          </CardHeader>
        </Card>
      </div>
    </template>

    <!-- Modal bài học -->
    <Modal :open="formOpen" :title="editingId === null ? messages.admin.content.createLessonTitle : messages.admin.content.editLessonTitle" @close="formOpen = false">
      <form class="admin-content__form" novalidate @submit.prevent="saveLesson">
        <p v-if="formRejectionReason" class="admin-content__reject-banner" role="alert">
          Bài học đã bị từ chối: {{ formRejectionReason }} — sửa nội dung và lưu để gửi lại duyệt.
        </p>
        <Input v-model="form.title" :label="messages.admin.content.lessonTitle" required />
        <Input v-model="form.description" :label="messages.admin.content.lessonDesc" />
        <div class="admin-content__row">
          <label class="label" for="lesson-topic">{{ messages.admin.content.lessonTopic }}</label>
          <select id="lesson-topic" v-model="form.topicId" class="input">
            <option v-for="topic in topics" :key="topic.id" :value="topic.id">{{ topic.name }}</option>
          </select>
        </div>
        <!-- Xuất bản: Class Only → active; Public → pendingreview (backend v2.15) -->
        <div class="admin-content__row">
          <label class="admin-content__publish" for="lesson-class-only">
            <input id="lesson-class-only" v-model="form.isClassOnly" type="checkbox" class="admin-content__checkbox" />
            <span class="admin-content__publish-text">
              <span class="admin-content__publish-label">
                {{ form.isClassOnly ? 'Chỉ dùng trong Lớp học riêng (Class Only)' : 'Gửi yêu cầu xuất bản toàn hệ thống (Public)' }}
              </span>
              <span class="admin-content__publish-hint">
                {{ form.isClassOnly ? 'Sẽ lưu trạng thái Kích hoạt — chỉ hiển thị trong lớp học riêng.' : 'Sẽ lưu trạng thái Chờ duyệt — Admin kiểm duyệt trước khi công khai.' }}
              </span>
            </span>
          </label>
        </div>
        <!-- Mô phỏng multi-select → simulationKeys -->
        <div class="admin-content__row">
          <span class="label">{{ messages.admin.content.simAttach }}</span>
          <div class="admin-content__sims" role="group" aria-label="Danh sách mô phỏng">
            <label v-for="sim in CATALOG" :key="sim.key" class="admin-content__sim-option">
              <input v-model="form.simulationKeys" type="checkbox" :value="sim.key" />
              <span class="admin-content__sim-title">{{ sim.title }}</span>
              <code class="admin-content__sim-key">{{ sim.key }}</code>
            </label>
          </div>
          <p class="admin-content__sim-hint">
            {{ form.simulationKeys.length > 0 ? `Đã chọn ${form.simulationKeys.length} mô phỏng.` : 'Chưa chọn mô phỏng nào.' }}
          </p>
        </div>
        <!-- Editor Markdown: Soạn thảo ↔ Xem trước -->
        <div class="admin-content__row">
          <span class="label">{{ messages.admin.content.contentHtml }}</span>
          <div class="admin-content__editor">
            <div class="admin-content__editor-tabs" role="tablist" aria-label="Chế độ soạn thảo nội dung">
              <button
                type="button"
                role="tab"
                :aria-selected="editorTab === 'write'"
                class="admin-content__editor-tab"
                :class="{ 'admin-content__editor-tab--active': editorTab === 'write' }"
                @click="editorTab = 'write'"
              >
                Soạn thảo
              </button>
              <button
                type="button"
                role="tab"
                :aria-selected="editorTab === 'preview'"
                class="admin-content__editor-tab"
                :class="{ 'admin-content__editor-tab--active': editorTab === 'preview' }"
                @click="editorTab = 'preview'"
              >
                Xem trước
              </button>
            </div>
            <div v-if="editorTab === 'write'">
              <div class="admin-content__md-toolbar" role="toolbar" aria-label="Thanh định dạng Markdown">
                <button
                  v-for="action in toolbarActions"
                  :key="action.label"
                  type="button"
                  class="admin-content__md-toolbar-btn"
                  :title="action.title"
                  @click="runToolbar(action)"
                >
                  {{ action.label }}
                </button>
              </div>
              <textarea
                ref="textareaRef"
                id="lesson-html"
                v-model="form.contentHtml"
                class="admin-content__html"
                rows="14"
                :placeholder="messages.admin.content.htmlPlaceholder"
              />
            </div>
            <div v-else class="admin-content__preview">
              <p v-if="!form.contentHtml.trim()" class="admin-content__preview-empty">
                Chưa có nội dung — gõ Markdown ở tab Soạn thảo.
              </p>
              <ProseContent v-else :content-html="previewHtml" />
            </div>
          </div>
        </div>
        <div class="admin-content__actions">
          <Button variant="ghost" @click="formOpen = false">{{ messages.admin.content.cancel }}</Button>
          <Button type="submit" :loading="saving">{{ messages.admin.content.save }}</Button>
        </div>
      </form>
    </Modal>

    <!-- Modal chủ đề -->
    <Modal :open="topicFormOpen" :title="messages.admin.content.createTopicTitle" @close="topicFormOpen = false">
      <form class="admin-content__form" novalidate @submit.prevent="saveTopic">
        <Input v-model="topicForm.name" :label="messages.admin.content.topicName" required />
        <Input v-model="topicForm.description" :label="messages.admin.content.topicDesc" />
        <div class="admin-content__actions">
          <Button variant="ghost" @click="topicFormOpen = false">{{ messages.admin.content.cancel }}</Button>
          <Button type="submit">{{ messages.admin.content.create }}</Button>
        </div>
      </form>
    </Modal>

    <!-- Modal xem trước bài học — render HTML như học viên thấy (ProseContent tự escape plain text) -->
    <Modal :open="previewOpen" :title="previewTitle" width="720px" @close="closePreview">
      <div class="admin-content__preview">
        <ProseContent v-if="previewContent.trim()" :content="previewContent" />
        <p v-else class="admin-content__preview-empty">Chưa có nội dung để xem trước.</p>
      </div>
      <template #footer>
        <Button variant="ghost" @click="closePreview">Đóng</Button>
      </template>
    </Modal>
  </main>
</template>

<style scoped>
.admin-content {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Loading / Error ── */
.admin-content__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-content__error {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
  flex-wrap: wrap;
  padding: var(--space-md);
  border: 1px solid color-mix(in srgb, var(--destructive) 35%, transparent);
  background: color-mix(in srgb, var(--destructive) 8%, transparent);
  border-radius: var(--radius-md);
}

.admin-content__error-text { margin: 0; font-size: var(--text-sm); color: var(--destructive); }

.admin-content__toolbar { display: flex; gap: var(--space-sm); justify-content: flex-end; flex-wrap: wrap; }

/* ── Table bài học (DESIGN §4.6) ── */
.admin-content__table {
  padding: 0;
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  overflow: hidden;
}

.admin-content__table-scroll { overflow-x: auto; border-radius: inherit; }

.admin-content__table table { width: 100%; border-collapse: collapse; }

.admin-content__table th {
  text-align: left;
  font-size: var(--text-sm);
  font-weight: 500;
  color: var(--foreground-tertiary);
  padding: 0 var(--space-md);
  height: 40px;
  border-bottom: 1px solid var(--border);
  background: var(--muted);
  white-space: nowrap;
}

.admin-content__table td {
  padding: 12px var(--space-md);
  border-bottom: 1px solid var(--border);
  font-size: var(--text-sm);
  vertical-align: middle;
}

.admin-content__table tbody tr { transition: background-color 150ms; }

.admin-content__table tbody tr:hover { background: color-mix(in srgb, var(--muted) 50%, transparent); }

.admin-content__table tbody tr:last-child td { border-bottom: none; }

.admin-content__idx {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  white-space: nowrap;
}

.admin-content__title-cell { min-width: 0; }

.admin-content__title-text { font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 280px; }

.admin-content__title-desc { font-size: var(--text-xs); color: var(--foreground-tertiary); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 280px; }

.admin-content__status-cell { display: inline-flex; flex-wrap: wrap; align-items: center; gap: var(--space-xs); }

.admin-content__rejected { margin: 2px 0 0; font-size: var(--text-xs); color: var(--destructive); }

.admin-content__sim-count {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  font-variant-numeric: tabular-nums;
}

.admin-content__sim-count--zero { color: var(--foreground-tertiary); }

.admin-content__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

/* ── Topic grid ── */
.admin-content__topics { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: var(--space-md); }

.admin-content__topic { min-width: 0; border-color: var(--border); transition: border-color 150ms; }

.admin-content__topic:hover { border-color: var(--border-strong); }

.admin-content__topic-head { display: flex; flex-direction: row; align-items: flex-start; gap: var(--space-sm); }

.admin-content__topic-icon {
  width: 36px;
  height: 36px;
  border-radius: var(--radius-md);
  background: var(--muted);
  color: var(--foreground-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.admin-content__topic-meta { min-width: 0; flex: 1; }

.admin-content__topic-name {
  font-size: var(--text-lg);
  font-weight: 600;
  letter-spacing: -0.015em;
  line-height: 1.25;
}

.admin-content__topic-desc { font-size: var(--text-sm); display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden; }

.admin-content__topic-count { flex-shrink: 0; }

/* ── Modal form ── */
.admin-content__form { display: flex; flex-direction: column; gap: var(--space-md); }

.admin-content__row { display: flex; flex-direction: column; gap: var(--space-xs); }

.admin-content__row-head { display: flex; align-items: center; justify-content: space-between; gap: var(--space-sm); }

/* ── Modal xem trước bài học ── */
.admin-content__preview { padding: var(--space-xs) 0; }

.admin-content__preview-empty {
  margin: 0;
  padding: var(--space-xl) 0;
  text-align: center;
  font-size: var(--text-sm);
  color: var(--foreground-tertiary);
}

/* Select/textarea chưa có wrapper shadcn — giữ .input nhưng token + easing chuẩn */
.admin-content__row .input,
.admin-content__html {
  background: var(--card);
  border-color: var(--border);
  color: var(--foreground);
  font-size: var(--text-sm);
  transition: border-color 150ms;
}

/* ── Editor Markdown 2 tab (v2.15) ── */
.admin-content__editor {
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  overflow: hidden;
  background: var(--card);
}

.admin-content__editor-tabs { display: flex; border-bottom: 1px solid var(--border); background: var(--muted); }

.admin-content__editor-tab {
  flex: 1;
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-sm);
  color: var(--foreground-secondary);
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  cursor: pointer;
  transition: color 150ms, border-color 150ms;
}

.admin-content__editor-tab--active {
  color: var(--foreground);
  font-weight: 500;
  background: var(--card);
  border-bottom-color: var(--data-core);
}

.admin-content__md-toolbar {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-xs);
  padding: var(--space-sm);
  border-bottom: 1px solid var(--border);
}

.admin-content__md-toolbar-btn {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  line-height: 1;
  padding: 6px 8px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  background: var(--card);
  color: var(--foreground-secondary);
  cursor: pointer;
  transition: border-color 150ms, color 150ms;
}

.admin-content__md-toolbar-btn:hover { border-color: var(--border-strong); color: var(--foreground); }

.admin-content__editor .admin-content__html {
  width: 100%;
  min-height: 280px;
  padding: var(--space-md);
  border: none;
  border-radius: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  resize: vertical;
}

.admin-content__preview {
  padding: var(--space-md);
  min-height: 280px;
  max-height: 420px;
  overflow-y: auto;
}

.admin-content__preview-empty { margin: 0; font-size: var(--text-sm); color: var(--foreground-tertiary); }

/* Callout trong preview — nhãn + màu viền theo loại */
.admin-content__preview :deep(blockquote[data-callout])::before {
  display: block;
  font-size: var(--text-xs);
  font-weight: 600;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  margin-bottom: var(--space-xs);
}

.admin-content__preview :deep(blockquote[data-callout='note']) { border-left-color: var(--color-primary); }
.admin-content__preview :deep(blockquote[data-callout='note'])::before { content: 'Ghi chú'; color: var(--color-primary); }
.admin-content__preview :deep(blockquote[data-callout='tip']) { border-left-color: var(--color-success); }
.admin-content__preview :deep(blockquote[data-callout='tip'])::before { content: 'Mẹo'; color: var(--color-success); }
.admin-content__preview :deep(blockquote[data-callout='warning']) { border-left-color: var(--color-warning); }
.admin-content__preview :deep(blockquote[data-callout='warning'])::before { content: 'Cảnh báo'; color: var(--color-warning); }

/* ── Xuất bản + mô phỏng (v2.15) ── */
.admin-content__publish { display: flex; align-items: flex-start; gap: var(--space-sm); cursor: pointer; }

.admin-content__publish .admin-content__checkbox { margin-top: 3px; accent-color: var(--data-core); }

.admin-content__publish-text { display: flex; flex-direction: column; gap: 2px; min-width: 0; }

.admin-content__publish-label { font-size: var(--text-sm); font-weight: 500; color: var(--foreground); }

.admin-content__publish-hint { font-size: var(--text-xs); color: var(--foreground-tertiary); }

.admin-content__reject-banner {
  padding: var(--space-sm) var(--space-md);
  border: 1px solid color-mix(in srgb, var(--destructive) 35%, transparent);
  background: color-mix(in srgb, var(--destructive) 8%, transparent);
  border-radius: var(--radius-md);
  font-size: var(--text-sm);
  color: var(--destructive);
}

.admin-content__sims {
  display: flex;
  flex-direction: column;
  gap: 2px;
  max-height: 200px;
  overflow-y: auto;
  padding: var(--space-xs) var(--space-sm);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
}

.admin-content__sim-option {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: 4px 6px;
  border-radius: var(--radius-sm);
  font-size: var(--text-sm);
  cursor: pointer;
}

.admin-content__sim-option:hover { background: var(--muted); }

.admin-content__sim-option input { accent-color: var(--data-core); }

.admin-content__sim-title { flex: 1; min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

.admin-content__sim-key { font-family: var(--font-mono); font-size: var(--text-xs); color: var(--foreground-tertiary); }

.admin-content__sim-hint { margin: 0; font-size: var(--text-xs); color: var(--foreground-tertiary); }

.admin-content__actions { display: flex; justify-content: flex-end; gap: var(--space-sm); }

@media (max-width: 640px) {
  /* Bảng → card-stack (DESIGN §8 — cấm scroll ngang bảng chính ở mobile) */
  .admin-content__table-scroll { overflow-x: visible; }

  .admin-content__table thead { display: none; }

  .admin-content__table tbody tr {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: var(--space-xs) var(--space-md);
    padding: var(--space-sm) var(--space-md);
    border-bottom: 1px solid var(--border);
  }

  .admin-content__table tbody tr:last-child { border-bottom: none; }

  .admin-content__table td {
    display: flex;
    flex-direction: column;
    gap: var(--space-xs);
    padding: 0;
    border-bottom: none;
  }

  .admin-content__table td::before {
    content: attr(data-label);
    font-size: var(--text-xs);
    color: var(--foreground-tertiary);
  }

  .admin-content__table td:first-child { grid-column: 1 / -1; }
  .admin-content__table td:last-child { align-items: flex-start; }

  .admin-content__title-text,
  .admin-content__title-desc { max-width: 100%; white-space: normal; }
}
</style>
