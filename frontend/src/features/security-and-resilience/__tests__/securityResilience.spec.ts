import { describe, it, expect } from 'vitest';
import { mount } from '@vue/test-utils';
import { sanitizeHtml } from '@/utils/sanitize';
import ProseContent from '@/components/ui/ProseContent.vue';
import { renderMarkdown } from '@/shared/utils/markdown';
import { createBubbleGenerator } from '@/engines/generators/sort/bubble';
import { createQuickGenerator } from '@/engines/generators/sort/quick';
import { createMergeGenerator } from '@/engines/generators/sort/merge';
import { createHeapSortGenerator } from '@/engines/generators/sort/heap';
import { createInsertionGenerator } from '@/engines/generators/sort/insertion';
import { createSelectionGenerator } from '@/engines/generators/sort/selection';
import { createBinaryGenerator } from '@/engines/generators/search/binary';
import { createLinearGenerator } from '@/engines/generators/search/linear';
import { ArrayRenderer } from '@/engines/renderers/arrayRenderer';

describe('Security: XSS & Injection Protection', () => {
  it('sanitizeHtml strips script tags and payload <script>alert(1)</script>', () => {
    const malicious = '<script>alert(1)</script><p>Nội dung hợp lệ</p>';
    const cleaned = sanitizeHtml(malicious);
    expect(cleaned).not.toContain('<script>');
    expect(cleaned).not.toContain('alert(1)');
    expect(cleaned).toContain('<p>Nội dung hợp lệ</p>');
  });

  it('sanitizeHtml strips inline onerror and on* handlers from images and svgs', () => {
    const payload = '"><img src=x onerror=alert(1)>';
    const cleaned = sanitizeHtml(payload);
    expect(cleaned).not.toContain('onerror');
    expect(cleaned).not.toContain('alert(1)');
  });

  it('sanitizeHtml strips dangerous svg onload vectors', () => {
    const payload = '<svg onload="alert(1)"><circle cx="10" cy="10" r="5"/></svg>';
    const cleaned = sanitizeHtml(payload);
    expect(cleaned).not.toContain('onload');
    expect(cleaned).not.toContain('alert(1)');
  });

  it('sanitizeHtml strips javascript: links', () => {
    const payload = '<a href="javascript:alert(1)">Bấm vào đây</a>';
    const cleaned = sanitizeHtml(payload);
    expect(cleaned).not.toContain('javascript:');
  });

  it('ProseContent neutralizes stored XSS payloads', () => {
    const wrapper = mount(ProseContent, {
      props: {
        format: 'html',
        contentHtml: '<h1>Học Thuật Toán</h1><img src="x" onerror="alert(1)"><script>alert(2)</script>',
      },
    });
    const el = wrapper.element as HTMLElement;
    expect(el.querySelector('script')).toBeNull();
    const img = el.querySelector('img');
    if (img) {
      expect(img.getAttribute('onerror')).toBeNull();
    }
  });

  it('renderMarkdown escapes HTML tags and prevents executable img onerror elements', () => {
    const mdWithXss = '# Tiêu đề\n\n<img src=x onerror=alert(1)>\n\n**In đậm**';
    const rendered = renderMarkdown(mdWithXss);
    expect(rendered).toContain('&lt;img src=x onerror=alert(1)&gt;');
    expect(rendered).toContain('<strong>In đậm</strong>');
    
    // Mount to verify no active img with onerror exists in DOM
    const wrapper = mount({
      template: `<div v-html="rendered"></div>`,
      data() { return { rendered }; }
    });
    expect(wrapper.find('img').exists()).toBe(false);
  });
});

describe('UI Edge Cases & Resilience: Visualizer Engines', () => {
  const edgeCaseArrays = [
    { label: 'Mảng rỗng []', data: [] },
    { label: 'Mảng 1 phần tử [5]', data: [5] },
    { label: 'Mảng toàn trùng lặp [4, 4, 4, 4]', data: [4, 4, 4, 4] },
    { label: 'Mảng đã sắp xếp ngược [9, 8, 7, 6]', data: [9, 8, 7, 6] },
    { label: 'Mảng lớn 50 phần tử', data: Array.from({ length: 50 }, (_, i) => 50 - i) },
  ];

  const generators = [
    { name: 'Bubble Sort', gen: createBubbleGenerator() },
    { name: 'Quick Sort', gen: createQuickGenerator() },
    { name: 'Merge Sort', gen: createMergeGenerator() },
    { name: 'Heap Sort', gen: createHeapSortGenerator() },
    { name: 'Insertion Sort', gen: createInsertionGenerator() },
    { name: 'Selection Sort', gen: createSelectionGenerator() },
    { name: 'Linear Search', gen: createLinearGenerator() },
    { name: 'Binary Search', gen: createBinaryGenerator() },
  ];

  for (const { label, data } of edgeCaseArrays) {
    for (const { name, gen } of generators) {
      it(`${name} xử lý an toàn không crash với ${label}`, () => {
        const steps = gen.generate({
          kind: 'array',
          data: { values: data, target: 5 },
        });
        expect(Array.isArray(steps)).toBe(true);
        expect(steps.length).toBeGreaterThan(0);
        for (const step of steps) {
          expect(step.structure).toBeDefined();
          expect(step.explanation).toBeDefined();
          expect(step.explanation).not.toContain('NaN');
          expect(step.explanation).not.toContain('undefined');
        }
      });
    }
  }

  it('ArrayRenderer handles empty and large arrays on canvas without crashing or NaN bounds', () => {
    const renderer = new ArrayRenderer();
    const canvas = document.createElement('canvas');
    canvas.width = 390;
    canvas.height = 400;
    renderer.mount(canvas);
    renderer.resize(390, 400);

    // Empty structure
    expect(() => {
      renderer.render({ kind: 'array', elements: [], links: [] }, { showIndex: true, showValues: true, zoom: 1, showLegend: false });
    }).not.toThrow();

    // 1 element
    expect(() => {
      renderer.render(
        { kind: 'array', elements: [{ id: 'cell:0', label: '5', status: 'default' }], links: [] },
        { showIndex: true, showValues: true, zoom: 1, showLegend: false },
      );
    }).not.toThrow();

    // 50 elements on mobile width 390px
    const largeElements = Array.from({ length: 50 }, (_, i) => ({
      id: `cell:${i}`,
      label: String(i * 2),
      status: 'default' as const,
    }));

    expect(() => {
      renderer.render({ kind: 'array', elements: largeElements, links: [] }, { showIndex: true, showValues: true, zoom: 1, showLegend: false });
    }).not.toThrow();
  });
});
