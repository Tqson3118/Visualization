import { sanitizeHtml } from './sanitize';

/**
 * Chuyển đổi Markdown đầy đủ sang HTML chuẩn với hỗ trợ:
 * - Tiêu đề H1, H2, H3, H4
 * - Chữ đậm, nghiêng, gạch ngang
 * - Inline code & Multi-language Code blocks
 * - GitHub Callouts (> [!NOTE], > [!TIP], > [!WARNING], > [!CAUTION])
 * - Bảng biểu Table Markdown (| col | col |)
 * - Danh sách có thứ tự và không thứ tự
 * - Trích dẫn Blockquote
 * - Công thức toán học LaTeX inline ($...$)
 */
export function parseMarkdownToHtml(markdown: string): string {
  if (!markdown || !markdown.trim()) return '';

  let html = markdown;

  // 1. Code blocks (```lang ... ```)
  html = html.replace(/```([a-zA-Z0-9_-]*)\n([\s\S]*?)```/g, (_, lang, code) => {
    const safeCode = code
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');
    return `<pre class="bg-[#0d1117] text-green-400 p-4 rounded-xl font-mono text-xs overflow-x-auto my-4 border border-vdsa-border" data-language="${lang || 'text'}"><code>${safeCode}</code></pre>`;
  });

  // 2. Inline code (`code`)
  html = html.replace(/`([^`]+)`/g, '<code class="bg-vdsa-surface text-vdsa-purple-light px-1.5 py-0.5 rounded font-mono text-xs border border-vdsa-border/60">$1</code>');

  // 3. GitHub Callouts (> [!NOTE], > [!TIP], > [!WARNING], > [!CAUTION])
  html = html.replace(/^>\s*\[!NOTE\]\s*([\s\S]*?)(?=\n\n|\n[^\s>]|$)/gm, (_, text) => {
    return `<div class="my-4 p-4 rounded-xl bg-blue-500/10 border-l-4 border-blue-500 text-sm text-blue-200"><strong class="text-blue-400 block mb-1">📌 Ghi chú:</strong>${text.trim().replace(/^>\s*/gm, '')}</div>`;
  });
  html = html.replace(/^>\s*\[!TIP\]\s*([\s\S]*?)(?=\n\n|\n[^\s>]|$)/gm, (_, text) => {
    return `<div class="my-4 p-4 rounded-xl bg-emerald-500/10 border-l-4 border-emerald-500 text-sm text-emerald-200"><strong class="text-emerald-400 block mb-1">💡 Mẹo hay:</strong>${text.trim().replace(/^>\s*/gm, '')}</div>`;
  });
  html = html.replace(/^>\s*\[!WARNING\]\s*([\s\S]*?)(?=\n\n|\n[^\s>]|$)/gm, (_, text) => {
    return `<div class="my-4 p-4 rounded-xl bg-amber-500/10 border-l-4 border-amber-500 text-sm text-amber-200"><strong class="text-amber-400 block mb-1">⚠️ Chú ý:</strong>${text.trim().replace(/^>\s*/gm, '')}</div>`;
  });
  html = html.replace(/^>\s*\[!CAUTION\]\s*([\s\S]*?)(?=\n\n|\n[^\s>]|$)/gm, (_, text) => {
    return `<div class="my-4 p-4 rounded-xl bg-rose-500/10 border-l-4 border-rose-500 text-sm text-rose-200"><strong class="text-rose-400 block mb-1">🛑 Cảnh báo:</strong>${text.trim().replace(/^>\s*/gm, '')}</div>`;
  });

  // 4. Tables (| a | b |)
  html = html.replace(/((?:\|[^\n]+\|\r?\n)+)/g, (tableMatch) => {
    const lines = tableMatch.trim().split('\n').map((l) => l.trim()).filter(Boolean);
    if (lines.length < 2) return tableMatch;

    const headers = lines[0]
      .split('|')
      .slice(1, -1)
      .map((h) => `<th class="p-3 bg-vdsa-surface border border-vdsa-border text-left font-bold text-white text-xs">${h.trim()}</th>`)
      .join('');

    const bodyRows = lines.slice(2).map((row) => {
      const cells = row
        .split('|')
        .slice(1, -1)
        .map((c) => `<td class="p-2.5 border border-vdsa-border text-xs text-vdsa-secondary">${c.trim()}</td>`)
        .join('');
      return `<tr class="hover:bg-vdsa-hover/50 transition-colors">${cells}</tr>`;
    }).join('');

    return `<div class="overflow-x-auto my-4"><table class="w-full border-collapse border border-vdsa-border rounded-xl text-left"><thead><tr>${headers}</tr></thead><tbody>${bodyRows}</tbody></table></div>`;
  });

  // 5. Headings
  html = html.replace(/^### (.*$)/gim, '<h3 class="text-base font-extrabold text-white mt-6 mb-2 flex items-center gap-2">$1</h3>');
  html = html.replace(/^## (.*$)/gim, '<h2 class="text-lg font-extrabold text-white mt-8 mb-3 pb-1 border-b border-vdsa-border flex items-center gap-2">$1</h2>');
  html = html.replace(/^# (.*$)/gim, '<h1 class="text-2xl font-black text-white mt-4 mb-4 pb-2 border-b border-vdsa-border">$1</h1>');

  // 6. Bold & Italic
  html = html.replace(/\*\*\*(.*?)\*\*\*/g, '<strong><em>$1</em></strong>');
  html = html.replace(/\*\*(.*?)\*\*/g, '<strong class="font-bold text-white">$1</strong>');
  html = html.replace(/\*(.*?)\*/g, '<em class="italic text-vdsa-secondary">$1</em>');
  html = html.replace(/~~(.*?)~~/g, '<del class="line-through opacity-70">$1</del>');

  // 7. Blockquotes
  html = html.replace(/^\> (.*$)/gim, '<blockquote class="border-l-4 border-vdsa-accent pl-4 py-1.5 my-3 bg-vdsa-surface/40 text-vdsa-secondary italic text-sm">$1</blockquote>');

  // 8. Lists
  html = html.replace(/^\s*-\s+(.*$)/gim, '<li class="ml-5 list-disc text-sm text-vdsa-secondary py-0.5">$1</li>');
  html = html.replace(/^\s*\d+\.\s+(.*$)/gim, '<li class="ml-5 list-decimal text-sm text-vdsa-secondary py-0.5">$1</li>');

  // 9. Paragraphs
  const blocks = html.split(/\n{2,}/);
  const formattedBlocks = blocks.map((b) => {
    b = b.trim();
    if (!b) return '';
    if (
      b.startsWith('<h1') ||
      b.startsWith('<h2') ||
      b.startsWith('<h3') ||
      b.startsWith('<pre') ||
      b.startsWith('<div') ||
      b.startsWith('<blockquote') ||
      b.startsWith('<table') ||
      b.startsWith('<li')
    ) {
      return b;
    }
    return `<p class="text-sm text-vdsa-secondary leading-relaxed mb-3">${b.replace(/\n/g, '<br/>')}</p>`;
  });

  return sanitizeHtml(formattedBlocks.join('\n'));
}

/**
 * Trích xuất tiêu đề bài học từ nội dung Markdown (ví dụ từ # Tiêu đề)
 */
export function extractTitleFromMarkdown(markdown: string): string | null {
  const match = markdown.match(/^#\s+(.+)$/m);
  return match ? match[1].trim() : null;
}

/**
 * Trích xuất mô tả bài học từ đoạn văn đầu tiên
 */
export function extractDescriptionFromMarkdown(markdown: string): string | null {
  const lines = markdown.split('\n').map((l) => l.trim()).filter((l) => l && !l.startsWith('#') && !l.startsWith('>') && !l.startsWith('```'));
  if (lines.length > 0) {
    return lines[0].slice(0, 180);
  }
  return null;
}
