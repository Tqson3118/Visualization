<script setup lang="ts">
// ProseContent — hiển thị nội dung lý thuyết với typography chuẩn (DESIGN.md §2.2 + tokens.css).
// Hợp nhất 2 API: `contentHtml` (dev — LessonDetail/AdminContentView/NodeHub/Simulator/simOverview)
// và `content` (alias — auto-detect HTML thật vs plain text).
// An toàn: plain text / format='text' → escape HTML entities trước khi wrap (chống XSS);
// format='html' (caller chỉ định) → render nguyên văn (chỉ dùng content đã sanitize backend).
import { computed } from 'vue';

const props = withDefaults(
  defineProps<{
    /** HTML thô từ backend/biên soạn — tin cậy khi format='html' (đã sanitize Ganss.Xss). */
    contentHtml?: string;
    /** Alias của contentHtml — auto-detect (có <tag> thường & không có tag nguy hiểm → HTML). */
    content?: string;
    /** Ép kiểu render; bỏ trống → auto-detect. */
    format?: 'html' | 'text';
  }>(),
  { contentHtml: '', content: '', format: undefined },
);

const HAS_HTML_TAG = /<[a-z][^>]*>/i;
/** Tag nguy hiểm — auto-detect gặp → coi là text (escape), chống XSS khi nguồn là plain text chứa mã độc. */
const DANGEROUS_TAG = /<(script|style|iframe|object|embed|form|link|meta)\b/i;

/** Escape HTML entities — bắt buộc trước khi wrap plain text (chống XSS). */
function escapeHtml(value: string): string {
  return value
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#39;');
}

/** Plain text (đã escape) → các <p>; `\n\n+` = đoạn mới, `\n` đơn = <br>. */
function textToParagraphs(escaped: string): string {
  return escaped
    .split(/\n{2,}/)
    .map((block) => block.trim())
    .filter(Boolean)
    .map((block) => `<p>${block.replaceAll('\n', '<br>')}</p>`)
    .join('');
}

const source = computed<string>(() => props.contentHtml || props.content || '');

const rendered = computed<string>(() => {
  const raw = source.value;
  if (!raw) return '';
  // format='html' (caller chỉ định) → tin cậy, render nguyên văn.
  // Auto-detect: có tag HTML thường và KHÔNG có tag nguy hiểm → render nguyên văn.
  const renderRaw =
    props.format === 'html' ||
    (props.format == null && HAS_HTML_TAG.test(raw) && !DANGEROUS_TAG.test(raw));
  return renderRaw ? raw : textToParagraphs(escapeHtml(raw));
});
</script>

<template>
  <div class="prose" v-html="rendered" />
</template>

<style scoped>
.prose {
  font-size: var(--text-base);
  line-height: 1.75;
  color: var(--color-text-primary);
  min-width: 0;
  overflow-wrap: break-word;
  word-break: break-word;
}

.prose > :first-child { margin-top: 0; }
.prose > :last-child { margin-bottom: 0; }

/* ── Headings ── */
.prose :deep(h1) { font-size: var(--text-xl); line-height: 1.3; margin-block: var(--space-lg) var(--space-md); }
.prose :deep(h2) { font-size: var(--text-lg); line-height: 1.3; margin-block: var(--space-lg) var(--space-sm); }
.prose :deep(h3) { font-size: var(--text-md); line-height: 1.4; margin-block: var(--space-md) var(--space-sm); }
.prose :deep(h4) { font-size: var(--text-base); line-height: 1.4; margin-block: var(--space-md) var(--space-sm); }

/* ── Đoạn văn — ngắt dòng thoáng ── */
.prose :deep(p) { margin-block: 0 var(--space-md); }

.prose :deep(a) {
  color: var(--color-primary);
  text-decoration: underline;
  text-underline-offset: 2px;
}

.prose :deep(strong) { font-weight: 600; }
</style>
