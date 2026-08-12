// engines/__tests__/renderers.spec.ts — Renderer canvas thật (SDD §8.3)
//
// jsdom không có canvas thật (getContext trả null) → mock 2D context với vi.fn()
// cho các method mà CanvasPainter dùng; test đảm bảo render không throw + vẽ đúng
// màu theo trạng thái (không hardcode màu).

import { describe, expect, test, vi } from 'vitest';

import type { Structure } from '../core/types';
import { ArrayRenderer, MUTED_CELL_ALPHA } from '../renderers/arrayRenderer';
import { CANVAS_COLORS, hexToRgba } from '../renderers/canvasTheme';
import { GraphRenderer } from '../renderers/graphRenderer';
import { HashTableRenderer } from '../renderers/hashTableRenderer';
import type { Renderer, RenderOptions } from '../renderers/interface';
import { ListRenderer } from '../renderers/listRenderer';
import { ALL_RENDERERS, getRendererForKind } from '../renderers/rendererRegistry';
import { StackQueueRenderer } from '../renderers/stackQueueRenderer';
import { TreeRenderer } from '../renderers/treeRenderer';

const OPTS: RenderOptions = { showIndex: true, showValues: true, zoom: 1, showLegend: false };

interface MockContext {
  ctx: CanvasRenderingContext2D;
  fillStyles: string[];
  strokeStyles: string[];
}

/** Tạo 2D context giả — ghi lại mọi màu gán vào fillStyle/strokeStyle. */
function createMockContext(): MockContext {
  let fillStyle = '';
  let strokeStyle = '';
  const fillStyles: string[] = [];
  const strokeStyles: string[] = [];
  const ctx: Record<string, unknown> = {
    setTransform: vi.fn(),
    clearRect: vi.fn(),
    fillRect: vi.fn(),
    beginPath: vi.fn(),
    roundRect: vi.fn(),
    rect: vi.fn(),
    arc: vi.fn(),
    fill: vi.fn(),
    stroke: vi.fn(),
    fillText: vi.fn(),
    moveTo: vi.fn(),
    lineTo: vi.fn(),
    quadraticCurveTo: vi.fn(),
    closePath: vi.fn(),
    setLineDash: vi.fn(),
    save: vi.fn(),
    restore: vi.fn(),
    scale: vi.fn(),
    lineWidth: 1,
    font: '',
    textAlign: 'center',
    textBaseline: 'middle',
  };
  Object.defineProperty(ctx, 'fillStyle', {
    get: () => fillStyle,
    set: (v: string) => {
      fillStyle = v;
      fillStyles.push(v);
    },
  });
  Object.defineProperty(ctx, 'strokeStyle', {
    get: () => strokeStyle,
    set: (v: string) => {
      strokeStyle = v;
      strokeStyles.push(v);
    },
  });
  return { ctx: ctx as unknown as CanvasRenderingContext2D, fillStyles, strokeStyles };
}

/** Mount renderer vào canvas giả rồi resize 800×600. */
function mountRenderer(renderer: Renderer, mock: MockContext): void {
  const canvas = {
    getContext: vi.fn(() => mock.ctx),
    width: 800,
    height: 600,
  } as unknown as HTMLCanvasElement;
  renderer.mount(canvas);
  renderer.resize(800, 600);
}

function renderAll(renderer: Renderer, structure: Structure): MockContext {
  const mock = createMockContext();
  mountRenderer(renderer, mock);
  renderer.render(structure, OPTS);
  return mock;
}

describe('ArrayRenderer', () => {
  test('render mảng 5 phần tử không throw, vẽ ô (fillRect/fillText) và tôn trọng muted', () => {
    const renderer = new ArrayRenderer();
    const structure: Structure = {
      kind: 'array',
      elements: [
        { id: 'cell:0', label: '5', status: 'default' },
        { id: 'cell:1', label: '3', status: 'active' },
        { id: 'cell:2', label: '8', status: 'swap' },
        { id: 'cell:3', label: '1', status: 'muted' },
        { id: 'cell:4', label: '9', status: 'done' },
      ],
      links: [],
    };
    const mock = renderAll(renderer, structure);

    expect(mock.ctx.fillRect).toHaveBeenCalled(); // nền + ô
    expect(mock.ctx.fillText).toHaveBeenCalled(); // giá trị + index
    // Ô muted phải tô màu muted (pha alpha làm mờ) — không hardcode màu.
    expect(mock.fillStyles).toContain(hexToRgba(CANVAS_COLORS.muted, MUTED_CELL_ALPHA));
  });

  test('render mảng rỗng không throw', () => {
    const renderer = new ArrayRenderer();
    const mock = renderAll(renderer, { kind: 'array', elements: [], links: [] });
    expect(mock.ctx.fillRect).toHaveBeenCalled(); // chỉ tô nền
  });
});

describe('ListRenderer', () => {
  test('render linkedlist có links không throw, vẽ mũi tên (lineTo) + ô null (setLineDash)', () => {
    const renderer = new ListRenderer();
    const structure: Structure = {
      kind: 'linkedlist',
      elements: [
        { id: 'node:0', label: '7', status: 'default', group: 'linkedlist', meta: { index: 0 } },
        { id: 'node:1', label: '3', status: 'active', group: 'linkedlist', meta: { index: 1 } },
        { id: 'node:2', label: '9', status: 'done', group: 'linkedlist', meta: { index: 2 } },
      ],
      links: [
        { from: 'node:0', to: 'node:1', label: 'next' },
        { from: 'node:1', to: 'node:2', label: 'next' },
      ],
    };
    const mock = renderAll(renderer, structure);

    expect(mock.ctx.lineTo).toHaveBeenCalled(); // mũi tên next
    expect(mock.ctx.setLineDash).toHaveBeenCalled(); // ô null ∅ viền đứt
  });

  test('nút nổi (chưa nối, thao tác insert) được vẽ viền đứt, không throw', () => {
    const renderer = new ListRenderer();
    const structure: Structure = {
      kind: 'linkedlist',
      elements: [
        { id: 'node:0', label: '7', status: 'default', group: 'linkedlist', meta: { index: 0 } },
        { id: 'node:1', label: '3', status: 'highlight', group: 'linkedlist', meta: { index: 1 } },
      ],
      links: [],
    };
    const mock = renderAll(renderer, structure);
    expect(mock.ctx.setLineDash).toHaveBeenCalled();
  });
});

describe('TreeRenderer', () => {
  test('render BST 3 nút (2 links) không throw, vẽ nút tròn (arc) + cạnh cong', () => {
    const renderer = new TreeRenderer();
    const structure: Structure = {
      kind: 'tree',
      elements: [
        { id: 'node:50', label: '50', status: 'default', group: 'tree' },
        { id: 'node:30', label: '30', status: 'active', group: 'tree' },
        { id: 'node:70', label: '70', status: 'done', group: 'tree' },
      ],
      links: [
        { from: 'node:50', to: 'node:30', label: 'L' },
        { from: 'node:50', to: 'node:70', label: 'R' },
      ],
    };
    const mock = renderAll(renderer, structure);
    expect(mock.ctx.arc).toHaveBeenCalled();
    expect(mock.ctx.quadraticCurveTo).toHaveBeenCalled();
  });

  test('render heap kèm mảng heap-array bên dưới không throw', () => {
    const renderer = new TreeRenderer();
    const structure: Structure = {
      kind: 'heap',
      elements: [
        { id: 'cell:0', label: '9', status: 'swap', group: 'heap-array', meta: { heapIndex: 0 } },
        { id: 'cell:1', label: '5', status: 'default', group: 'heap-array', meta: { heapIndex: 1 } },
        { id: 'cell:2', label: '7', status: 'muted', group: 'heap-array', meta: { heapIndex: 2 } },
      ],
      links: [
        { from: 'cell:0', to: 'cell:1', status: 'default' },
        { from: 'cell:0', to: 'cell:2', status: 'default' },
      ],
    };
    const mock = renderAll(renderer, structure);
    expect(mock.ctx.arc).toHaveBeenCalled();
    expect(mock.ctx.fillText).toHaveBeenCalled();
  });
});

describe('GraphRenderer', () => {
  test('render đồ thị 4 đỉnh + cạnh có hướng không throw, vẽ đỉnh (arc) + mũi tên', () => {
    const renderer = new GraphRenderer();
    const structure: Structure = {
      kind: 'graph',
      elements: [
        { id: 'node:0', label: '0', status: 'done', meta: { d: 0 } },
        { id: 'node:1', label: '1', status: 'active' },
        { id: 'node:2', label: '2', status: 'default' },
        { id: 'node:3', label: '3', status: 'muted' },
      ],
      links: [
        { from: 'node:0', to: 'node:1', label: 'w=4', status: 'done' },
        { from: 'node:1', to: 'node:2', label: 'w=2' },
        { from: 'node:0', to: 'node:3', label: 'w=7' },
      ],
    };
    const mock = renderAll(renderer, structure);
    expect(mock.ctx.arc).toHaveBeenCalled();
    expect(mock.ctx.lineTo).toHaveBeenCalled();
    expect(mock.ctx.fillText).toHaveBeenCalled();
  });
});

describe('StackQueueRenderer', () => {
  const stack: Structure = {
    kind: 'stack',
    elements: [
      { id: 'cell:0', label: '5', status: 'done', group: 'stack' },
      { id: 'cell:1', label: '3', status: 'swap', group: 'stack' },
      { id: 'cell:2', label: '—', status: 'muted', group: 'stack', meta: { empty: true } },
      { id: 'cell:3', label: '—', status: 'muted', group: 'stack', meta: { empty: true } },
    ],
    links: [],
  };
  const queue: Structure = {
    kind: 'queue',
    elements: [
      { id: 'cell:0', label: '4', status: 'swap', group: 'queue' },
      { id: 'cell:1', label: '9', status: 'done', group: 'queue' },
      { id: 'cell:2', label: '—', status: 'muted', group: 'queue', meta: { empty: true } },
    ],
    links: [],
  };

  test('render stack dãy dọc không throw', () => {
    const renderer = new StackQueueRenderer();
    const mock = renderAll(renderer, stack);
    expect(mock.ctx.roundRect).toHaveBeenCalled();
    expect(mock.ctx.fillText).toHaveBeenCalled();
  });

  test('render queue dãy ngang không throw', () => {
    const renderer = new StackQueueRenderer();
    const mock = renderAll(renderer, queue);
    expect(mock.ctx.roundRect).toHaveBeenCalled();
  });
});

describe('HashTableRenderer', () => {
  test('render 2 bucket không throw, vẽ header + chuỗi nối kết', () => {
    const renderer = new HashTableRenderer();
    const structure: Structure = {
      kind: 'hashtable',
      elements: [
        { id: 'bucket:0', label: '[0]', status: 'default', group: 'bucket:0' },
        { id: 'node:12', label: '12', status: 'done', group: 'bucket:0', meta: { chainPos: 0 } },
        { id: 'node:25', label: '25', status: 'swap', group: 'bucket:0', meta: { chainPos: 1 } },
        { id: 'bucket:1', label: '[1]', status: 'active', group: 'bucket:1' },
        { id: 'node:13', label: '13', status: 'default', group: 'bucket:1', meta: { chainPos: 0 } },
      ],
      links: [{ from: 'node:12', to: 'node:25', label: 'next' }],
    };
    const mock = renderAll(renderer, structure);
    expect(mock.ctx.roundRect).toHaveBeenCalled();
    expect(mock.ctx.fillText).toHaveBeenCalled();
    expect(mock.ctx.lineTo).toHaveBeenCalled(); // mũi tên nối chuỗi
  });
});

describe('rendererRegistry', () => {
  test('đăng ký renderer thật cho mọi kind trong SDD §8.3', () => {
    const kinds = ['array', 'stack', 'queue', 'linkedlist', 'tree', 'heap', 'hashtable', 'graph'];
    for (const kind of kinds) {
      expect(getRendererForKind(kind), `kind ${kind} phải có renderer`).not.toBeNull();
    }
  });

  test('getRendererForKind("array") trả đúng instance ArrayRenderer', () => {
    expect(getRendererForKind('array')).toBeInstanceOf(ArrayRenderer);
  });

  test('kind lạ → null (component dùng fallback vẽ cũ)', () => {
    expect(getRendererForKind('unknown-kind')).toBeNull();
  });

  test('ALL_RENDERERS có đủ 6 renderer và đăng ký đúng instance', () => {
    expect(ALL_RENDERERS).toHaveLength(6);
    expect(ALL_RENDERERS.some((r) => r instanceof StackQueueRenderer)).toBe(true);
    expect(ALL_RENDERERS.some((r) => r instanceof TreeRenderer)).toBe(true);
    expect(ALL_RENDERERS.some((r) => r instanceof ListRenderer)).toBe(true);
    expect(ALL_RENDERERS.some((r) => r instanceof HashTableRenderer)).toBe(true);
    expect(ALL_RENDERERS.some((r) => r instanceof GraphRenderer)).toBe(true);
    // Instance đã đăng ký phải đúng instance trong registry.
    expect(getRendererForKind('linkedlist')).toBe(ALL_RENDERERS.find((r) => r instanceof ListRenderer) ?? null);
  });
});
