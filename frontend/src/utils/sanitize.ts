import DOMPurify from 'dompurify';

/**
 * Sanitize HTML an toàn chống XSS (Stored & Reflected).
 * Cho phép các thẻ HTML và SVG thông dụng, tự động loại bỏ thẻ <script>,
 * các event handler (onerror, onload, onclick...) và link javascript:.
 */
export function sanitizeHtml(dirty: string): string {
  if (!dirty) return '';
  return DOMPurify.sanitize(dirty, {
    USE_PROFILES: { html: true, svg: true },
    ADD_ATTR: ['target', 'rel'],
  });
}

export default sanitizeHtml;
