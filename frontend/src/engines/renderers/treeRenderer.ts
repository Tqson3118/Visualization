// engines/renderers/treeRenderer.ts — Renderer cây + heap (SDD §8.3)
//
// Quy ước vẽ:
// - Tọa độ đệ quy theo links cha → con: duyệt in-order để gán cột (không chồng lấp),
//   độ sâu quyết định hàng; nút tròn 40px, cạnh cong (quadratic).
// - Heap: vẽ thêm mảng tương ứng phía dưới khi có element nhóm 'heap-array';
//   bubble up/down hiển thị bằng mũi tên ở đầu cạnh (link status active/swap/highlight).

import type { Element, Link, Structure } from '@/engines/core/types';
import { CANVAS_COLORS, CANVAS_LAYOUT, hexToRgba } from './canvasTheme';
import type { Renderer, RenderOptions } from './interface';
import { CanvasPainter } from './painter/canvasPainter';

const NODE_R = 20;        // SDD §8.3: nút tròn 40px
const MIN_LEVEL_H = 48;
const MAX_LEVEL_H = 110;
const HEAP_ARRAY_RESERVE = 84;   // khoảng trống phía dưới cho mảng heap

export class TreeRenderer implements Renderer {
  supportedKinds: string[] = ['tree', 'heap'];

  private painter = new CanvasPainter();

  mount(canvas: HTMLCanvasElement): void {
    this.painter.mount(canvas);
  }

  resize(width: number, height: number): void {
    this.painter.resize(width, height);
  }

  dispose(): void {
    // Renderer không giữ tài nguyên cần giải phóng.
  }

  render(structure: Structure, options: RenderOptions): void {
    this.painter.setZoom(options.zoom);
    if (this.painter.width < 10 || this.painter.height < 10) return;
    this.painter.beginFrame();

    const w = this.painter.logicalWidth;
    const h = this.painter.logicalHeight;
    const hasHeapArray = structure.elements.some((el) => el.group === 'heap-array');
    const bottomReserve = hasHeapArray ? HEAP_ARRAY_RESERVE : 0;

    // Cây con: links cha → con.
    const childrenOf = new Map<string, Link[]>();
    const parents = new Set<string>();
    for (const link of structure.links) {
      const list = childrenOf.get(link.from) ?? [];
      list.push(link);
      childrenOf.set(link.from, list);
      parents.add(link.to);
    }
    const byId = new Map<string, Element>();
    for (const el of structure.elements) byId.set(el.id, el);
    const root = structure.elements.find((el) => !parents.has(el.id)) ?? structure.elements[0];
    if (!root) return;

    // Gán cột theo in-order (trái → gốc → phải) và độ sâu.
    const orderOf = new Map<string, number>();
    const depthOf = new Map<string, number>();
    let counter = 0;
    const assign = (id: string, depth: number): void => {
      const kids = childrenOf.get(id) ?? [];
      const left = kids.find((l) => l.label === 'L' || l.label === 'left') ?? kids[0];
      const right = kids.find((l) => l.label === 'R' || l.label === 'right') ?? kids[1];
      if (left) assign(left.to, depth + 1);
      orderOf.set(id, counter++);
      depthOf.set(id, depth);
      if (right) assign(right.to, depth + 1);
    };
    assign(root.id, 0);

    const nodeCount = Math.max(1, orderOf.size);
    const maxDepth = Math.max(1, ...Array.from(depthOf.values()).map((d) => d + 1));
    const slotW = (w - 2 * CANVAS_LAYOUT.margin) / nodeCount;
    const levelH = Math.max(MIN_LEVEL_H, Math.min(MAX_LEVEL_H, (h - CANVAS_LAYOUT.paddingTop - bottomReserve - CANVAS_LAYOUT.margin) / maxDepth));
    const posOf = new Map<string, { x: number; y: number }>();
    for (const el of structure.elements) {
      const order = orderOf.get(el.id);
      const depth = depthOf.get(el.id);
      if (order === undefined || depth === undefined) continue;
      posOf.set(el.id, {
        x: CANVAS_LAYOUT.margin + order * slotW + slotW / 2,
        y: CANVAS_LAYOUT.paddingTop + depth * levelH + NODE_R,
      });
    }

    // Vẽ cạnh trước (nằm dưới nút).
    for (const link of structure.links) {
      const from = posOf.get(link.from);
      const to = posOf.get(link.to);
      if (!from || !to) continue;
      const color = this.painter.edgeColor(link.status);
      const bubble = link.status === 'active' || link.status === 'swap' || link.status === 'highlight';
      this.painter.curve(from.x, from.y + NODE_R - 4, to.x, to.y - NODE_R + 4, color, 2, 16, bubble);
      if (link.label === 'L' || link.label === 'R') {
        this.painter.label(link.label, (from.x + to.x) / 2 + 8, (from.y + to.y) / 2 - 6, CANVAS_COLORS.muted, 9);
      }
    }

    // Vẽ nút.
    for (const el of structure.elements) {
      const pos = posOf.get(el.id);
      if (!pos) continue;
      this.drawNode(el, pos.x, pos.y, options);
    }

    // Mảng heap tương ứng phía dưới.
    if (hasHeapArray && structure.kind === 'heap') {
      this.drawHeapArray(structure, options);
    }
  }

  private drawNode(el: Element, x: number, y: number, options: RenderOptions): void {
    const muted = el.status === 'muted';
    const fill = muted ? this.painter.statusColorWithAlpha('muted', 0.5) : this.painter.statusColor(el.status);
    const stroke = hexToRgba(this.painter.statusColor(el.status), 0.8);
    // Glow cho nút đang xét — vẽ trước hình tròn để nằm dưới.
    if (el.status === 'active' || el.status === 'highlight') {
      this.painter.arcGlow(x, y, NODE_R, this.painter.statusColor(el.status), 8);
    }
    this.painter.circle(x, y, NODE_R, fill, stroke);
    if (options.showValues) {
      this.painter.text(el.label, x, y, {
        size: 12,
        weight: 'bold',
        color: muted ? CANVAS_COLORS.muted : CANVAS_COLORS.text,
      });
    }
  }

  /** Mảng heap (nhóm 'heap-array') — dãy ô nhỏ phía dưới cây. */
  private drawHeapArray(structure: Structure, options: RenderOptions): void {
    const w = this.painter.logicalWidth;
    const h = this.painter.logicalHeight;
    const cells = structure.elements
      .filter((el) => el.group === 'heap-array')
      .sort((a, b) => (typeof a.meta?.heapIndex === 'number' ? a.meta.heapIndex : (CanvasPainter.indexFromId(a.id) ?? 0))
        - (typeof b.meta?.heapIndex === 'number' ? b.meta.heapIndex : (CanvasPainter.indexFromId(b.id) ?? 0)));
    if (cells.length === 0) return;

    const cellW = Math.max(26, Math.min(44, (w - 2 * CANVAS_LAYOUT.margin) / cells.length));
    const y = h - CANVAS_LAYOUT.margin - 36;
    cells.forEach((el, i) => {
      const x = CANVAS_LAYOUT.margin + i * cellW;
      const muted = el.status === 'muted';
      const fill = muted ? this.painter.statusColorWithAlpha('muted', 0.5) : this.painter.statusColor(el.status);
      const stroke = hexToRgba(this.painter.statusColor(el.status), 0.7);
      this.painter.roundRect(x, y, cellW, 30, 4, fill, stroke);
      if (options.showValues) {
        this.painter.text(el.label, x + cellW / 2, y + 15, { size: 10, color: muted ? CANVAS_COLORS.muted : CANVAS_COLORS.text });
      }
      if (options.showIndex) {
        const idx = typeof el.meta?.heapIndex === 'number' ? el.meta.heapIndex : CanvasPainter.indexFromId(el.id);
        if (idx !== null) this.painter.label(String(idx), x + cellW / 2, y + 42, CANVAS_COLORS.muted, 8);
      }
    });
    this.painter.label('heap-array', CANVAS_LAYOUT.margin, y - 8, CANVAS_COLORS.muted, 10);
  }
}
