import { describe, expect, it } from 'vitest';
import { mount } from '@vue/test-utils';

import ProseContent from './ProseContent.vue';

function html(content: string, format?: 'html' | 'markdown' | 'text') {
  const wrapper = mount(ProseContent, { props: { content, format } });
  return (wrapper.element as HTMLElement).innerHTML;
}

describe('ProseContent', () => {
  it('render nguyên vẹn khi format="html"', () => {
    const out = html('<p><strong>Đậm</strong> và <em>nghiêng</em></p><ul><li>a</li></ul>', 'html');
    expect(out).toBe('<p><strong>Đậm</strong> và <em>nghiêng</em></p><ul><li>a</li></ul>');
  });

  it('auto-detect: content chứa thẻ HTML → render nguyên, không wrap thêm', () => {
    const out = html('<p>Đoạn 1</p>\n<p>Đoạn 2</p>');
    expect(out).toBe('<p>Đoạn 1</p>\n<p>Đoạn 2</p>');
  });

  it('plain text 1 đoạn → wrap vào <p>', () => {
    const out = html('Giải thuật sắp xếp nổi bọt: so sánh từng cặp liền kề.');
    expect(out).toBe('<p>Giải thuật sắp xếp nổi bọt: so sánh từng cặp liền kề.</p>');
  });

  it('plain text nhiều đoạn (tách bởi dòng trống) → nhiều <p>', () => {
    const out = html('Đoạn một.\n\nĐoạn hai.\n\n\nĐoạn ba.');
    expect(out).toBe('<p>Đoạn một.</p><p>Đoạn hai.</p><p>Đoạn ba.</p>');
  });

  it('newline đơn trong đoạn → <br>', () => {
    const out = html('Dòng 1\nDòng 2');
    expect(out).toBe('<p>Dòng 1<br>Dòng 2</p>');
  });

  it('plain text chứa <script> → escape, không tạo element (XSS-safe)', () => {
    const wrapper = mount(ProseContent, { props: { content: 'Xem <script>alert(1)</script> ngay' } });
    const el = wrapper.element as HTMLElement;
    // Không có element <script> thật trong DOM (chỉ là text đã escape → jsdom decode về text)
    expect(el.querySelector('script')).toBeNull();
    expect(el.querySelectorAll('*')).toHaveLength(1); // chỉ <p>
    expect(el.textContent).toBe('Xem <script>alert(1)</script> ngay');
  });

  it('plain text chứa entity & và dấu nháy → hiển thị literal, không thành element/attribute', () => {
    const wrapper = mount(ProseContent, { props: { content: 'A & B < "x" > \'y\'' } });
    const el = wrapper.element as HTMLElement;
    expect(el.querySelector('p')?.textContent).toBe('A & B < "x" > \'y\'');
    expect(el.querySelectorAll('*')).toHaveLength(1);
  });

  it('format="text" ép text kể cả khi content có thẻ HTML (không render nguyên)', () => {
    const out = html('<p>Không phải HTML</p>', 'text');
    expect(out).not.toContain('<p>Không phải HTML</p>');
    expect(out).toContain('&lt;p&gt;');
  });

  it('format="markdown" xử lý như text (wrap + escape, không parse)', () => {
    const out = html('# Tiêu đề\n\nĐoạn văn.', 'markdown');
    expect(out).toBe('<p># Tiêu đề</p><p>Đoạn văn.</p>');
  });

  it('content rỗng → không render gì', () => {
    expect(html('')).toBe('');
  });

  it('text "a < b" không bị nhận nhầm là HTML (không có tag hợp lệ)', () => {
    const out = html('So sánh a < b và c > d');
    expect(out).toBe('<p>So sánh a &lt; b và c &gt; d</p>');
  });
});
