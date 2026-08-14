// utils/simOverview.ts — HTML giới thiệu thuật toán/CTDL từ catalog meta (render qua ProseContent).
// Dùng chung cho SimulatorView (panel Giới thiệu) và NodeHubView (fallback lý thuyết).
import type { CatalogMeta } from '@/engines/catalog';

const HTML_ESCAPES: Record<string, string> = {
  '&': '&amp;',
  '<': '&lt;',
  '>': '&gt;',
  '"': '&quot;',
  "'": '&#39;',
};

/** Escape HTML cho text từ dữ liệu (title/tags/...) trước khi nhúng vào contentHtml. */
export function escapeHtml(value: string): string {
  return value.replace(/[&<>"']/g, (ch) => HTML_ESCAPES[ch] ?? ch);
}

/** HTML ngắn mô tả một mô phỏng: nhóm, độ phức tạp, thẻ — trả '' khi không có meta. */
export function buildSimOverviewHtml(meta: CatalogMeta | undefined): string {
  if (!meta) return '';
  const level = meta.level === 'basic' ? 'cơ bản' : 'nâng cao';
  const kind = meta.category === 'algorithm' ? 'thuật toán' : 'cấu trúc dữ liệu';
  const tags = meta.tags.map((t) => `<code>${escapeHtml(t)}</code>`).join(' ');
  return [
    `<p><strong>${escapeHtml(meta.title)}</strong> — ${kind} thuộc nhóm <strong>${escapeHtml(meta.dataStructure)}</strong> (mức ${level}).</p>`,
    `<p>Độ phức tạp: trung bình <code>${escapeHtml(meta.complexity.average)}</code> · không gian <code>${escapeHtml(meta.complexity.space)}</code>.</p>`,
    `<p>Thẻ liên quan: ${tags}</p>`,
  ].join('\n');
}
