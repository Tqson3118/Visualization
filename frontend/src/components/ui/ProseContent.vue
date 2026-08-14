<script setup lang="ts">
// ProseContent — hiển thị HTML nội dung lý thuyết với typography ngắt đoạn chuẩn
// (DESIGN.md §2.2 + tokens.css). Dùng cho lesson.contentHtml và mô tả lý thuyết dài:
// heading, đoạn văn, danh sách, blockquote/callout, code/pre (nền tối nhẹ, chữ mono),
// bảng. Text dài được phép ngắt dòng (overflow-wrap) — không dính 1 dòng.
defineProps<{
  /** HTML thô từ backend/biên soạn — KHÔNG render user input */
  contentHtml: string;
}>();
</script>

<template>
  <div class="prose" v-html="contentHtml" />
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

.prose :deep(hr) {
  border: none;
  border-top: 1px solid var(--color-border);
  margin-block: var(--space-lg);
}

/* ── Danh sách ── */
.prose :deep(ul),
.prose :deep(ol) {
  margin-block: 0 var(--space-md);
  padding-left: 1.5em;
}

.prose :deep(li) { margin-block: var(--space-xs); }
.prose :deep(li > ul),
.prose :deep(li > ol) { margin-block: var(--space-xs); }

/* ── Blockquote / callout (>[!NOTE]/TIP/WARNING nếu có) ── */
.prose :deep(blockquote) {
  margin-block: 0 var(--space-md);
  padding: var(--space-sm) var(--space-md);
  border-left: 3px solid var(--color-primary);
  border-radius: 0 var(--radius-md) var(--radius-md) 0;
  background: var(--color-surface-hover);
  color: var(--color-text-secondary);
}

.prose :deep(blockquote p:last-child) { margin-bottom: 0; }

/* ── Code inline ── */
.prose :deep(code) {
  font-family: var(--font-mono);
  font-size: 0.875em;
  padding: 2px 6px;
  border-radius: var(--radius-sm);
  background: color-mix(in srgb, var(--color-muted) 70%, transparent);
  overflow-wrap: break-word;
}

/* ── Pre — nền tối nhẹ (motif block-token §6), chữ mono, ngắt dòng thay vì dính 1 dòng ── */
.prose :deep(pre) {
  margin-block: 0 var(--space-md);
  padding: var(--space-md);
  border-radius: var(--radius-md);
  background: var(--color-canvas-ink);
  color: rgba(255, 255, 255, 0.92);
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  line-height: 1.6;
  white-space: pre-wrap;
  overflow-wrap: anywhere;
  overflow-x: auto;
}

.prose :deep(pre code) {
  padding: 0;
  background: transparent;
  color: inherit;
  font-size: inherit;
}

/* ── Bảng ── */
.prose :deep(table) {
  display: block;
  width: 100%;
  max-width: 100%;
  overflow-x: auto;
  border-collapse: collapse;
  margin-block: 0 var(--space-md);
  font-size: var(--text-sm);
}

.prose :deep(th),
.prose :deep(td) {
  border: 1px solid var(--color-border);
  padding: 6px 10px;
  text-align: left;
}

.prose :deep(th) {
  font-weight: 600;
  background: var(--color-muted);
}
</style>
