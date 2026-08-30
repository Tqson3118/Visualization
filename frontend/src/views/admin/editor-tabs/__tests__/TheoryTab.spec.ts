import { beforeEach, describe, expect, it, vi } from 'vitest';
import { mount } from '@vue/test-utils';
import { createPinia, setActivePinia } from 'pinia';
import TheoryTab from '../TheoryTab.vue';
import TipTapEditor from '@/components/ui/TipTapEditor.vue';
import { LESSON_TEMPLATES } from '@/data/lessonTemplates';
import * as aiFormatService from '@/services/aiFormatService';
import { parseMarkdownToHtml } from '@/utils/markdownParser';

vi.mock('@/services/aiFormatService', () => ({
  formatLessonWithAi: vi.fn(),
  getAiUsageRemaining: vi.fn(() => 5),
  incrementAiUsage: vi.fn(() => 4),
}));

describe('TheoryTab — Word-like WYSIWYG & Boundary Markdown-to-HTML', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  it('1. applyTemplate() chuyển đổi template Markdown sang HTML trước khi emit update:modelValue', async () => {
    const wrapper = mount(TheoryTab, {
      props: {
        modelValue: '',
      },
    });

    const template = LESSON_TEMPLATES[0];
    await wrapper.vm.applyTemplate(template);

    const emittedUpdates = wrapper.emitted('update:modelValue');
    expect(emittedUpdates).toBeTruthy();
    const emittedHtml = emittedUpdates![0][0] as string;

    // Phải là HTML (có thẻ heading, callout, table) — KHÔNG phải markdown thô
    expect(emittedHtml).toContain('<h1');
    expect(emittedHtml).toContain('<h2');
    expect(emittedHtml).not.toContain('> [!NOTE]');
    expect(emittedHtml).not.toContain('| :--- |');

    const emittedTpl = wrapper.emitted('templateApplied');
    expect(emittedTpl).toBeTruthy();
    expect(emittedTpl![0][0]).toEqual({
      title: template.name,
      description: template.description,
    });
  });

  it('2. triggerFileInput (.md import) tách metadata trên Markdown rồi chuyển đổi sang HTML', async () => {
    const wrapper = mount(TheoryTab, {
      props: {
        modelValue: '',
      },
    });

    const mdContent = `# Thuật toán Dijkstra
Thuật toán tìm đường đi ngắn nhất từ một đỉnh nguồn đến tất cả các đỉnh còn lại trong đồ thị có trọng số không âm.

> [!NOTE]
> Khóa học Cấu trúc dữ liệu nâng cao

| Đỉnh | Khoảng cách |
| :--- | :--- |
| A | 0 |
| B | 4 |
`;

    // Giả lập FileReader
    const mockFile = new File([mdContent], 'dijkstra.md', { type: 'text/markdown' });
    let fileChangeCallback: ((e: any) => void) | null = null;

    const originalCreateElement = document.createElement.bind(document);
    vi.spyOn(document, 'createElement').mockImplementation((tagName: string) => {
      const el = originalCreateElement(tagName);
      if (tagName === 'input') {
        Object.defineProperty(el, 'click', {
          value: () => {
            if (fileChangeCallback) {
              fileChangeCallback({ target: { files: [mockFile] } });
            }
          },
        });
        Object.defineProperty(el, 'onchange', {
          set: (fn) => {
            fileChangeCallback = fn;
          },
          get: () => fileChangeCallback,
        });
      }
      return el;
    });

    // Mock FileReader behavior
    class MockFileReader {
      onload: ((e: any) => void) | null = null;
      readAsText(_file: File) {
        setTimeout(() => {
          if (this.onload) {
            this.onload({ target: { result: mdContent } });
          }
        }, 0);
      }
    }
    vi.stubGlobal('FileReader', MockFileReader);

    wrapper.vm.triggerFileInput();

    await new Promise((resolve) => setTimeout(resolve, 50));

    const emittedUpdates = wrapper.emitted('update:modelValue');
    expect(emittedUpdates).toBeTruthy();
    const emittedHtml = emittedUpdates![0][0] as string;

    expect(emittedHtml).toContain('<h1');
    expect(emittedHtml).toContain('Dijkstra');
    expect(emittedHtml).not.toContain('> [!NOTE]');
    expect(emittedHtml).not.toContain('| :--- |');

    const emittedTpl = wrapper.emitted('templateApplied');
    expect(emittedTpl).toBeTruthy();
    expect(emittedTpl![0][0]).toMatchObject({
      title: 'Thuật toán Dijkstra',
    });
  });

  it('3. handleAiFormat() chuyển đổi kết quả Markdown từ AI sang HTML', async () => {
    const aiOutputMarkdown = `## Phân tích độ phức tạp
> [!TIP]
> Sử dụng Min-Heap để tối ưu thời gian.

| Cấu trúc | Time | Space |
| :--- | :--- | :--- |
| Mảng thường | O(V^2) | O(V) |
`;
    vi.mocked(aiFormatService.formatLessonWithAi).mockResolvedValueOnce(aiOutputMarkdown);

    const wrapper = mount(TheoryTab, {
      props: {
        modelValue: '<p>Nội dung ban đầu</p>',
      },
    });

    await wrapper.vm.handleAiFormat();

    const emittedUpdates = wrapper.emitted('update:modelValue');
    expect(emittedUpdates).toBeTruthy();
    const emittedHtml = emittedUpdates![0][0] as string;

    expect(emittedHtml).toContain('<h2');
    expect(emittedHtml).toContain('Phân tích độ phức tạp');
    expect(emittedHtml).toContain('Mẹo hay');
    expect(emittedHtml).not.toContain('> [!TIP]');
    expect(emittedHtml).not.toContain('| :--- |');
  });

  it('4. Chế độ nâng cao toggle giữa WYSIWYG mặc định và Markdown Split view', async () => {
    const wrapper = mount(TheoryTab, {
      props: {
        modelValue: '<p>Nội dung thử nghiệm</p>',
      },
    });

    // Mặc định là WYSIWYG
    expect(wrapper.vm.editorType).toBe('wysiwyg');
    expect(wrapper.findComponent(TipTapEditor).exists()).toBe(true);

    // Bấm nút "Chế độ nâng cao"
    const advButton = wrapper.findAll('button').find((b) => b.text().includes('Chế độ nâng cao'));
    expect(advButton).toBeDefined();
    await advButton!.trigger('click');

    expect(wrapper.vm.editorType).toBe('markdown');
    expect(wrapper.find('textarea[placeholder="Viết nội dung bài học bằng Markdown..."]').exists()).toBe(true);

    // Bấm lần nữa quay lại WYSIWYG
    await advButton!.trigger('click');
    expect(wrapper.vm.editorType).toBe('wysiwyg');
    expect(wrapper.find('textarea[placeholder="Viết nội dung bài học bằng Markdown..."]').exists()).toBe(false);
  });

  it('5. HTML pass-through & Idempotency: Nội dung HTML sẵn không bị double-convert lồng thẻ', () => {
    const existingHtml = '<h2 class="text-lg font-extrabold text-white">Thuật toán Bubble Sort</h2><p class="text-sm text-vdsa-secondary">Sắp xếp các phần tử bằng cách đổi chỗ liên tiếp.</p>';
    const parsed = parseMarkdownToHtml(existingHtml);

    // Không bị lồng <p><h2...
    expect(parsed).not.toContain('<p><h2');
    expect(parsed).toContain('Thuật toán Bubble Sort');
  });

  it('6. Simulation Anchor qua HTML: [Mô phỏng: key] được chèn và chuyển đổi thành card trực quan', () => {
    const htmlWithAnchor = '<p>Dưới đây là mô phỏng:</p><p>[Mô phỏng: sort.bubble]</p>';
    const parsed = parseMarkdownToHtml(htmlWithAnchor);

    expect(parsed).toContain('⚡');
    expect(parsed).toContain('Mô phỏng thuật toán trực quan:');
    expect(parsed).toContain('sort.bubble');
    expect(parsed).toContain('href="/simulator/sort.bubble"');
    expect(parsed).toContain('Mở mô phỏng ↗');
  });
});
