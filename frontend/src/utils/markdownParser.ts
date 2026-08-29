import { sanitizeHtml } from './sanitize';

/**
 * Highlight syntax cho code fence
 */
export function highlightCode(rawCode: string): string {
  const escaped = rawCode
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');

  // Token regex for single-pass replacement to avoid replacing inside already replaced spans
  // 1. Comments: // ... or /* ... */ or # ...
  // 2. Strings: &quot;...&quot; or &#39;...&#39; or `...`
  // 3. Keywords: function, let, const, var, if, else, for, while, return, etc.
  // 4. Built-in objects / functions: console, log, Math, Array, etc.
  // 5. Numbers: \b\d+(\.\d+)?\b
  const tokenRegex = /(\/\/[^\n]*|\/\*[\s\S]*?\*\/)|(&quot;(?:\\.|[^&])*?&quot;|&#39;(?:\\.|[^&])*?&#39;|`(?:\\[\s\S]|[^`])*?`)|(\b(?:function|return|let|const|var|if|else|for|while|do|switch|case|break|continue|new|this|class|extends|super|import|export|from|default|try|catch|finally|throw|async|await|typeof|instanceof|void|delete|in|of|def|elif|lambda|pass|raise|except|with|as|int|float|str|bool|char|double|long|short|public|private|protected|static|final|abstract|interface|struct|namespace|using)\b)|(\b(?:console|log|warn|error|print|println|System|Math|Array|Object|String|Number|Boolean|Set|Map|Promise|document|window|undefined|null|true|false|None|True|False)\b)|(\b\d+(?:\.\d+)?\b)/g;

  return escaped.replace(tokenRegex, (match, comment, str, keyword, builtin, number) => {
    if (comment) {
      return `<span class="text-slate-400 italic">${comment}</span>`;
    }
    if (str) {
      return `<span class="text-emerald-300 font-medium">${str}</span>`;
    }
    if (keyword) {
      return `<span class="text-purple-400 font-bold">${keyword}</span>`;
    }
    if (builtin) {
      return `<span class="text-sky-300 font-medium">${builtin}</span>`;
    }
    if (number) {
      return `<span class="text-amber-300 font-mono">${number}</span>`;
    }
    return match;
  });
}

/**
 * Chuyển đổi Markdown đầy đủ sang HTML chuẩn với hỗ trợ:
 * - Tiêu đề H1, H2, H3, H4
 * - Chữ đậm, nghiêng, gạch ngang
 * - Inline code & Multi-language Code blocks kèm syntax highlighting
 * - GitHub Callouts (> [!NOTE], > [!TIP], > [!WARNING], > [!CAUTION])
 * - Bảng biểu Table Markdown (| col | col |)
 * - Danh sách có thứ tự và không thứ tự
 * - Trích dẫn Blockquote
 * - Công thức toán học LaTeX inline ($...$)
 */
export function parseMarkdownToHtml(markdown: string): string {
  if (!markdown || !markdown.trim()) return '';

  const codeBlocks: string[] = [];

  // Step 1: Extract code blocks (```lang ... ```) and replace with placeholder
  let html = markdown.replace(/```([a-zA-Z0-9_-]*)\n([\s\S]*?)```/g, (_, lang, code) => {
    const placeholder = `%%%CODE_BLOCK_${codeBlocks.length}%%%`;
    const langLabel = (lang || 'code').toUpperCase();
    const highlighted = highlightCode(code.trim());
    const renderedBlock = `
<div class="my-4 rounded-xl bg-[#09090e] border border-vdsa-border overflow-hidden shadow-xl not-prose">
  <div class="flex items-center justify-between px-4 py-2 bg-vdsa-surface/90 border-b border-vdsa-border/60">
    <div class="flex items-center gap-2">
      <span class="w-2.5 h-2.5 rounded-full bg-rose-500/80 inline-block"></span>
      <span class="w-2.5 h-2.5 rounded-full bg-amber-500/80 inline-block"></span>
      <span class="w-2.5 h-2.5 rounded-full bg-emerald-500/80 inline-block"></span>
      <span class="text-[10px] font-mono font-bold uppercase tracking-wider text-vdsa-muted ml-2">${langLabel}</span>
    </div>
  </div>
  <pre class="p-4 overflow-x-auto text-[13px] font-mono leading-relaxed text-slate-100 select-text m-0"><code class="block font-mono text-slate-100">${highlighted}</code></pre>
</div>`.trim();
    codeBlocks.push(renderedBlock);
    return placeholder;
  });

  // Step 2: GitHub Callouts (> [!NOTE], > [!TIP], > [!WARNING], > [!CAUTION])
  html = html.replace(/^>\s*\[!NOTE\]\s*([\s\S]*?)(?=\n\n|\n[^\s>]|$)/gm, (_, text) => {
    return `<div class="my-4 p-4 rounded-xl bg-blue-500/10 border-l-4 border-blue-500 text-sm text-blue-200 not-prose"><strong class="text-blue-400 block mb-1">📌 Ghi chú:</strong>${text.trim().replace(/^>\s*/gm, '')}</div>`;
  });
  html = html.replace(/^>\s*\[!TIP\]\s*([\s\S]*?)(?=\n\n|\n[^\s>]|$)/gm, (_, text) => {
    return `<div class="my-4 p-4 rounded-xl bg-emerald-500/10 border-l-4 border-emerald-500 text-sm text-emerald-200 not-prose"><strong class="text-emerald-400 block mb-1">💡 Mẹo hay:</strong>${text.trim().replace(/^>\s*/gm, '')}</div>`;
  });
  html = html.replace(/^>\s*\[!WARNING\]\s*([\s\S]*?)(?=\n\n|\n[^\s>]|$)/gm, (_, text) => {
    return `<div class="my-4 p-4 rounded-xl bg-amber-500/10 border-l-4 border-amber-500 text-sm text-amber-200 not-prose"><strong class="text-amber-400 block mb-1">⚠️ Chú ý:</strong>${text.trim().replace(/^>\s*/gm, '')}</div>`;
  });
  html = html.replace(/^>\s*\[!CAUTION\]\s*([\s\S]*?)(?=\n\n|\n[^\s>]|$)/gm, (_, text) => {
    return `<div class="my-4 p-4 rounded-xl bg-rose-500/10 border-l-4 border-rose-500 text-sm text-rose-200 not-prose"><strong class="text-rose-400 block mb-1">🛑 Cảnh báo:</strong>${text.trim().replace(/^>\s*/gm, '')}</div>`;
  });

  // Step 3: Tables (| a | b |)
  html = html.replace(/((?:\|[^\n]+\|\r?\n)+)/g, (tableMatch) => {
    const lines = tableMatch.trim().split('\n').map((l) => l.trim()).filter(Boolean);
    if (lines.length < 2) return tableMatch;

    const headers = lines[0]
      .split('|')
      .slice(1, -1)
      .map((h) => `<th class="p-3 bg-vdsa-surface border border-vdsa-border text-left font-bold text-vdsa-purple-light text-xs">${h.trim()}</th>`)
      .join('');

    const bodyRows = lines.slice(2).map((row) => {
      const cells = row
        .split('|')
        .slice(1, -1)
        .map((c) => `<td class="p-2.5 border border-vdsa-border text-xs text-vdsa-secondary">${c.trim()}</td>`)
        .join('');
      return `<tr class="hover:bg-vdsa-hover/50 transition-colors">${cells}</tr>`;
    }).join('');

    return `<div class="overflow-x-auto my-4 not-prose"><table class="w-full border-collapse border border-vdsa-border rounded-xl text-left"><thead><tr>${headers}</tr></thead><tbody>${bodyRows}</tbody></table></div>`;
  });

  // Step 4: Headings
  html = html.replace(/^### (.*$)/gim, '<h3 class="text-base font-bold text-vdsa-purple-light mt-6 mb-2 flex items-center gap-2">$1</h3>');
  html = html.replace(/^## (.*$)/gim, '<h2 class="text-lg font-extrabold text-white mt-8 mb-3 pb-1 border-b border-vdsa-border/60 flex items-center gap-2">$1</h2>');
  html = html.replace(/^# (.*$)/gim, '<h1 class="text-2xl font-black text-white mt-4 mb-4 pb-2 border-b border-vdsa-border/60">$1</h1>');

  // Step 5: Inline code
  html = html.replace(/`([^`]+)`/g, '<code class="bg-[#1a1826] text-vdsa-purple-light px-1.5 py-0.5 rounded font-mono text-xs border border-vdsa-border/60 font-semibold">$1</code>');

  // Step 6: Bold, Italic, Strikethrough
  html = html.replace(/\*\*\*(.*?)\*\*\*/g, '<strong><em>$1</em></strong>');
  html = html.replace(/\*\*(.*?)\*\*/g, '<strong class="font-bold text-white">$1</strong>');
  html = html.replace(/\*(.*?)\*/g, '<em class="italic text-vdsa-secondary">$1</em>');
  html = html.replace(/~~(.*?)~~/g, '<del class="line-through opacity-70">$1</del>');

  // Step 7: Blockquotes
  html = html.replace(/^\> (.*$)/gim, '<blockquote class="border-l-4 border-vdsa-accent pl-4 py-1.5 my-3 bg-vdsa-surface/40 text-vdsa-secondary italic text-sm">$1</blockquote>');

  // Step 8: Lists
  html = html.replace(/^\s*-\s+(.*$)/gim, '<li class="ml-5 list-disc text-sm text-vdsa-secondary py-0.5">$1</li>');
  html = html.replace(/^\s*\d+\.\s+(.*$)/gim, '<li class="ml-5 list-decimal text-sm text-vdsa-secondary py-0.5">$1</li>');

  // Step 9: Paragraphs
  const blocks = html.split(/\n{2,}/);
  const formattedBlocks = blocks.map((b) => {
    b = b.trim();
    if (!b) return '';
    if (
      b.startsWith('<h1') ||
      b.startsWith('<h2') ||
      b.startsWith('<h3') ||
      b.startsWith('<h4') ||
      b.startsWith('<p') ||
      b.startsWith('<!--') ||
      b.startsWith('<pre') ||
      b.startsWith('<div') ||
      b.startsWith('<blockquote') ||
      b.startsWith('<table') ||
      b.startsWith('<ul') ||
      b.startsWith('<ol') ||
      b.startsWith('<li') ||
      b.startsWith('%%%CODE_BLOCK_')
    ) {
      return b;
    }
    return `<p class="text-sm text-vdsa-secondary leading-relaxed mb-3">${b.replace(/\n/g, '<br/>')}</p>`;
  });

  let finalHtml = formattedBlocks.join('\n');

  // Step 10: Re-insert code blocks
  codeBlocks.forEach((block, idx) => {
    finalHtml = finalHtml.replace(`%%%CODE_BLOCK_${idx}%%%`, block);
  });

  return sanitizeHtml(finalHtml);
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
