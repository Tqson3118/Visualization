// engines/__tests__/renderers.spec.ts — Renderer canvas thật (SDD §8.3)
//
// jsdom không có canvas thật (getContext trả null) → mock 2D context với vi.fn()
// cho các method mà CanvasPainter dùng; test đảm bảo render không throw + vẽ đúng
// màu theo trạng thái (không hardcode màu).

import { describe, expect, test, vi } from 'vitest';

import type { Structure } from '../core/types';
import { ArrayRenderer, MUTED_CELL_ALPHA } from '../renderers/arrayRenderer';
import { CANVAS_COLORS, CANVAS_LAYOUT, hexToRgba } from '../renderers/canvasTheme';
import { CanvasPainter } from '../renderers/painter/canvasPainter';
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

  test('render m=11 bucket trên canvas hẹp 200px không throw, header roundRect width > 0', () => {
    const renderer = new HashTableRenderer();
    // m=11: colW = (200 - 2*margin)/11 ≈ 12.7 → colW - 16 < 0 (regression: IndexSizeError).
    const elements: Structure['elements'] = [];
    for (let b = 0; b < 11; b++) {
      elements.push({ id: `bucket:${b}`, label: `[${b}]`, status: 'default', group: `bucket:${b}` });
      elements.push({ id: `node:${b}`, label: String(b), status: 'default', group: `bucket:${b}`, meta: { chainPos: 0 } });
    }
    const structure: Structure = { kind: 'hashtable', elements, links: [] };
    const mock = createMockContext();
    const canvas = { getContext: vi.fn(() => mock.ctx), width: 200, height: 600 } as unknown as HTMLCanvasElement;
    renderer.mount(canvas);
    renderer.resize(200, 600);

    expect(() => renderer.render(structure, OPTS)).not.toThrow();
    // Mọi roundRect phải có width > 0 — trước fix, header có width âm (IndexSizeError).
    const calls = vi.mocked(mock.ctx.roundRect).mock.calls;
    expect(calls.length).toBeGreaterThan(0);
    for (const args of calls) {
      expect(args[2]).toBeGreaterThan(0);
    }
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

describe('CanvasPainter primitives (Task 6)', () => {
  /** Mount CanvasPainter vào canvas giả (pattern như mountRenderer). */
  function mountPainter(painter: CanvasPainter, mock: MockContext): void {
    const canvas = {
      getContext: vi.fn(() => mock.ctx),
      width: 800,
      height: 600,
    } as unknown as HTMLCanvasElement;
    painter.mount(canvas);
  }

  test('dashedRoundRect vẽ không throw, gọi setLineDash với dash rồi reset []', () => {
    const mock = createMockContext();
    const painter = new CanvasPainter();
    mountPainter(painter, mock);

    expect(() => painter.dashedRoundRect(10, 10, 80, 40, 6, CANVAS_COLORS.muted, 1.5)).not.toThrow();
    expect(mock.ctx.setLineDash).toHaveBeenCalledWith([6, 4]);
    expect(mock.ctx.setLineDash).toHaveBeenLastCalledWith([]); // reset dash sau khi vẽ
    expect(mock.ctx.stroke).toHaveBeenCalled(); // viền đứt được vẽ
  });

  test('arcGlow vẽ không throw, shadowBlur > 0 trong lúc vẽ và reset về 0 sau khi vẽ', () => {
    const mock = createMockContext();
    const painter = new CanvasPainter();
    mountPainter(painter, mock);
    let blurDuringFill = 0;
    vi.mocked(mock.ctx.fill).mockImplementation(() => {
      blurDuringFill = Math.max(blurDuringFill, Number(mock.ctx.shadowBlur) || 0);
    });

    expect(() => painter.arcGlow(100, 100, 20, CANVAS_COLORS.compare, 8)).not.toThrow();
    expect(blurDuringFill).toBeGreaterThan(0); // glow đang bật khi fill
    expect(mock.ctx.shadowBlur).toBe(0); // reset — không rò rỉ state canvas
    expect(mock.ctx.shadowColor).toBe('transparent');
  });

  test('fadeRect vẽ không throw, fillStyle là rgba với alpha đúng', () => {
    const mock = createMockContext();
    const painter = new CanvasPainter();
    mountPainter(painter, mock);

    expect(() => painter.fadeRect(10, 10, 100, 30, '#4255ff', 0.25)).not.toThrow();
    expect(mock.ctx.fillRect).toHaveBeenCalledWith(10, 10, 100, 30);
    expect(mock.fillStyles).toContain(hexToRgba('#4255ff', 0.25));
  });

  test('graphRenderer: đỉnh active được vẽ arcGlow (shadow set rồi reset, fill màu compare alpha 0.3)', () => {
    const renderer = new GraphRenderer();
    const structure: Structure = {
      kind: 'graph',
      elements: [
        { id: 'node:0', label: '0', status: 'active' },
        { id: 'node:1', label: '1', status: 'default' },
      ],
      links: [],
    };
    const mock = createMockContext();
    mountRenderer(renderer, mock);
    let maxBlur = 0;
    vi.mocked(mock.ctx.fill).mockImplementation(() => {
      maxBlur = Math.max(maxBlur, Number(mock.ctx.shadowBlur) || 0);
    });

    renderer.render(structure, OPTS);
    expect(maxBlur).toBeGreaterThan(0); // arcGlow đã chạy cho đỉnh active
    expect(mock.ctx.shadowBlur).toBe(0); // reset sau render
    expect(mock.fillStyles).toContain(hexToRgba(CANVAS_COLORS.compare, 0.3)); // glow dùng màu status
  });
});

describe('ArrayRenderer — wrap layout mảng dài (Task 7)', () => {
  // Chỉ vẽ index (không vẽ giá trị) → fillText chỉ chứa label index, kiểm tra chính xác.
  const OPTS_NO_VALUES: RenderOptions = { showIndex: true, showValues: false, zoom: 1, showLegend: false };

  function numericArray(n: number): Structure {
    const elements: Structure['elements'] = [];
    for (let i = 0; i < n; i++) elements.push({ id: `cell:${i}`, label: String(i), status: 'default' });
    return { kind: 'array', elements, links: [] };
  }

  /** Render với options tùy chỉnh (renderAll dùng OPTS cố định). */
  function renderAllWith(renderer: Renderer, structure: Structure, opts: RenderOptions): MockContext {
    const mock = createMockContext();
    mountRenderer(renderer, mock);
    renderer.render(structure, opts);
    return mock;
  }

  /** Số hàng thực tế = số đáy bar/ô khác nhau (y + h làm tròn). */
  function rowCount(mock: MockContext): number {
    const calls = vi.mocked(mock.ctx.roundRect).mock.calls;
    return new Set(calls.map((a) => Math.round((a[1] as number) + (a[3] as number)))).size;
  }

  test('bar mode n=60 (800×600): wrap ≥2 hàng, mọi bar width > 0, index toàn cục 0 và 59 xuất hiện', () => {
    const renderer = new ArrayRenderer();
    const mock = renderAllWith(renderer, numericArray(60), OPTS_NO_VALUES);
    const calls = vi.mocked(mock.ctx.roundRect).mock.calls;

    expect(calls.length).toBe(60); // mỗi phần tử vẽ đúng 1 bar
    expect(rowCount(mock)).toBeGreaterThanOrEqual(2); // ≥ 2 hàng
    for (const args of calls) {
      expect(args[2]).toBeGreaterThan(0); // bar width > 0
    }
    // showValues=false → fillText chỉ là index dưới đáy bar: 0 (hàng đầu) và 59 (hàng cuối).
    const texts = vi.mocked(mock.ctx.fillText).mock.calls.map((c) => String(c[0]));
    expect(texts).toContain('0');
    expect(texts).toContain('59');
  });

  test('bar mode n=5 (800×600): giữ hành vi cũ — 1 hàng, số bar vẽ bằng n (không wrap)', () => {
    const renderer = new ArrayRenderer();
    const mock = renderAll(renderer, numericArray(5));
    const calls = vi.mocked(mock.ctx.roundRect).mock.calls;

    expect(calls.length).toBe(5);
    expect(rowCount(mock)).toBe(1); // slotW = 148 ≥ 44 → 1 hàng duy nhất
  });

  test('bar mode n=100 (800×600): ≥3 hàng, không tràn khỏi margin', () => {
    const renderer = new ArrayRenderer();
    const mock = renderAll(renderer, numericArray(100));
    const calls = vi.mocked(mock.ctx.roundRect).mock.calls;

    expect(calls.length).toBe(100);
    expect(rowCount(mock)).toBeGreaterThanOrEqual(3);
    for (const args of calls) {
      expect(args[0] as number).toBeGreaterThanOrEqual(CANVAS_LAYOUT.margin); // x ≥ margin
      expect((args[0] as number) + (args[2] as number)).toBeLessThanOrEqual(800 - CANVAS_LAYOUT.margin); // x + w ≤ w - margin
    }
  });

  test('renderSquares n=40 (500×400): label chữ → wrap ≥2 hàng, không throw', () => {
    const renderer = new ArrayRenderer();
    const elements: Structure['elements'] = [];
    for (let i = 0; i < 40; i++) {
      elements.push({ id: `cell:${i}`, label: String.fromCharCode(97 + (i % 26)), status: 'default' });
    }
    const mock = createMockContext();
    const canvas = { getContext: vi.fn(() => mock.ctx), width: 500, height: 400 } as unknown as HTMLCanvasElement;
    renderer.mount(canvas);
    renderer.resize(500, 400);

    expect(() => renderer.render({ kind: 'array', elements, links: [] }, OPTS)).not.toThrow();
    const calls = vi.mocked(mock.ctx.roundRect).mock.calls;
    expect(calls.length).toBe(40); // mỗi ô vẽ đúng 1 lần
    expect(rowCount(mock)).toBeGreaterThanOrEqual(2); // ≥ 2 hàng
  });

  test('con trỏ trỏ đúng ô sau wrap (target cell:59 ở hàng cuối)', () => {
    const renderer = new ArrayRenderer();
    const structure = numericArray(60);
    structure.elements.push({ id: 'ptr', label: 'P', status: 'active', group: 'pointer', meta: { target: 'cell:59' } });
    const mock = renderAll(renderer, structure);

    // cell:59 là phần tử vẽ thứ 60 (các cell:0..59 đều không phải pointer) — lấy tâm bar từ roundRect.
    const barCalls = vi.mocked(mock.ctx.roundRect).mock.calls;
    const barCx = (barCalls[59][0] as number) + (barCalls[59][2] as number) / 2;
    // Trong bar mode, moveTo chỉ do mũi tên con trỏ gọi → lần gọi đầu tiên là mũi tên của pointer.
    const moveToCalls = vi.mocked(mock.ctx.moveTo).mock.calls;
    expect(moveToCalls.length).toBeGreaterThan(0);
    expect(moveToCalls[0][0] as number).toBeCloseTo(barCx, 5); // mũi tên thẳng đứng tại tâm ô đích
  });
});
