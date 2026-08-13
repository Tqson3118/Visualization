// engines/renderers/listRenderer.ts — Renderer danh sách liên kết (SDD §8.3)
//
// Quy ước vẽ:
// - Nút chữ nhật 80×40 nối bằng mũi tên → theo links (chuỗi từ head).
// - Ô null cuối (∅) — viền đứt nét.
// - Nhãn head/tail phía trên nút đầu / nút cuối.
// - Nút chưa nối (thao tác insert, chưa có link) = nút nổi: viền đứt nét + mờ, vẽ dưới chuỗi.

import type { Element, Structure } from '@/engines/core/types';
import { CANVAS_COLORS, CANVAS_LAYOUT, hexToRgba } from './canvasTheme';
import type { Renderer, RenderOptions } from './interface';
import { CanvasPainter } from './painter/canvasPainter';

const NODE_W = 80;   // SDD §8.3: nút chữ nhật 80×40
const NODE_H = 40;
const GAP = 36;      // khoảng trống cho mũi tên next
const NULL_NODE = 40;
const FLOAT_GAP = 34;

export class ListRenderer implements Renderer {
  supportedKinds: string[] = ['linkedlist'];

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

    // Chuỗi chính: xuất phát từ head (element không bị link nào trỏ tới), đi theo links.
    const byId = new Map<string, Element>();
    for (const el of structure.elements) byId.set(el.id, el);
    const nextOf = new Map<string, string>();
    const hasParent = new Set<string>();
    for (const link of structure.links) {
      nextOf.set(link.from, link.to);
      hasParent.add(link.to);
    }
    const chain: Element[] = [];
    const seen = new Set<string>();
    let cur: Element | undefined = structure.elements.find((el) => !hasParent.has(el.id));
    while (cur && !seen.has(cur.id)) {
      seen.add(cur.id);
      chain.push(cur);
      const nxt = nextOf.get(cur.id);
      cur = nxt ? byId.get(nxt) : undefined;
    }

    const y = Math.max(24, (h - NODE_H) / 2);

    // Co giãn khi danh sách dài: giảm kích thước nút + khoảng cách mũi tên cho vừa canvas.
    const totalW = chain.length * NODE_W + Math.max(0, chain.length - 1) * GAP;
    const scale = Math.min(1, (w - 2 * CANVAS_LAYOUT.margin) / Math.max(1, totalW));
    const nodeW = Math.max(44, Math.round(NODE_W * scale));
    const gap = Math.max(16, Math.round(GAP * scale));

    let x = CANVAS_LAYOUT.margin;

    // Vẽ chuỗi nút + mũi tên next.
    chain.forEach((el, i) => {
      this.drawNode(el, x, y, nodeW, options);
      if (i < chain.length - 1) {
        const color = this.edgeColorFor(el);
        this.painter.arrow(x + nodeW + 2, y + NODE_H / 2, x + nodeW + gap - 2, y + NODE_H / 2, color, 2, 7);
      }
      x += nodeW + gap;
    });

    // Ô null cuối chuỗi (∅).
    if (chain.length > 0) {
      this.drawNullNode(x + 4, y + (NODE_H - NULL_NODE) / 2);
    } else {
      // Danh sách rỗng: chỉ vẽ ô null giữa màn hình.
      this.drawNullNode(w / 2 - NULL_NODE / 2, h / 2 - NULL_NODE / 2);
    }

    // Nhãn head / tail phía trên nút đầu / nút cuối.
    if (chain.length > 0 && options.showValues) {
      this.painter.label('head', CANVAS_LAYOUT.margin + nodeW / 2, y - 12, CANVAS_COLORS.compare, 10);
      const lastX = CANVAS_LAYOUT.margin + (chain.length - 1) * (nodeW + gap);
      this.painter.label('tail', lastX + nodeW / 2, y - 12, CANVAS_COLORS.muted, 10);
    }

    // Nút nổi (chưa nối vào chuỗi — thao tác insert): vẽ bên dưới, viền đứt nét.
    const floating = structure.elements.filter((el) => !seen.has(el.id) && el.group !== 'pointer');
    if (floating.length > 0) {
      const fy = y + NODE_H + FLOAT_GAP;
      floating.forEach((el, i) => {
        const fx = CANVAS_LAYOUT.margin + i * (nodeW + gap);
        this.drawFloatingNode(el, fx, fy, nodeW, options);
      });
    }
  }

  private drawNode(el: Element, x: number, y: number, nodeW: number, options: RenderOptions): void {
    const cx = x + nodeW / 2;
    const cy = y + NODE_H / 2;
    const muted = el.status === 'muted';
    const fill = muted ? this.painter.statusColorWithAlpha('muted', 0.5) : this.painter.statusColor(el.status);
    const stroke = hexToRgba(this.painter.statusColor(el.status), 0.8);
    this.painter.roundRect(x, y, nodeW, NODE_H, CANVAS_LAYOUT.borderRadius, fill, stroke);

    if (options.showValues) {
      this.painter.text(el.label, cx, cy, {
        size: nodeW >= 60 ? 13 : 11,
        weight: 'bold',
        color: muted ? CANVAS_COLORS.muted : CANVAS_COLORS.text,
      });
    }
    if (options.showIndex) {
      const idx = typeof el.meta?.index === 'number' ? el.meta.index : CanvasPainter.indexFromId(el.id);
      if (idx !== null) {
        this.painter.label(String(idx), cx, y + NODE_H + 13, CANVAS_COLORS.muted, 9);
      }
    }
  }

  /** Nút nổi (chưa nối) — viền đứt nét, mờ. */
  private drawFloatingNode(el: Element, x: number, y: number, nodeW: number, options: RenderOptions): void {
    const cx = x + nodeW / 2;
    const cy = y + NODE_H / 2;
    const fill = this.painter.statusColorWithAlpha(el.status, 0.3);
    this.painter.roundRect(x, y, nodeW, NODE_H, CANVAS_LAYOUT.borderRadius, fill, undefined);
    this.painter.dashedRoundRect(x, y, nodeW, NODE_H, 6, CANVAS_COLORS.muted, 1.5);
    if (options.showValues) {
      this.painter.text(el.label, cx, cy, { size: nodeW >= 60 ? 13 : 11, weight: 'bold', color: CANVAS_COLORS.text });
    }
  }

  /** Ô null cuối (∅) — viền đứt nét. */
  private drawNullNode(x: number, y: number): void {
    const color = hexToRgba(CANVAS_COLORS.muted, 0.7);
    this.painter.roundRect(x, y, NULL_NODE, NULL_NODE, CANVAS_LAYOUT.borderRadius, undefined, undefined);
    this.painter.dashedRoundRect(x, y, NULL_NODE, NULL_NODE, 4, color, 1.5);
    this.painter.text('∅', x + NULL_NODE / 2, y + NULL_NODE / 2, { size: 16, color: CANVAS_COLORS.muted });
  }

  private edgeColorFor(el: Element): string {
    return hexToRgba(this.painter.statusColor(el.status), 0.6);
  }
}
