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

    // Wrap layout: danh sách dài (chain.length > threshold) → chia nhiều hàng, giữ
    // kích thước nút gốc (NODE_W × NODE_H). Danh sách ngắn → giữ hành vi cũ: co giãn
    // 1 hàng (giảm kích thước nút + khoảng cách cho vừa canvas).
    const threshold = Math.floor((w - 2 * CANVAS_LAYOUT.margin) / (NODE_W + GAP));
    const wrap = chain.length > 0 && chain.length > threshold;
    const maxPerRow = Math.max(1, threshold);
    const rows = wrap ? Math.ceil(chain.length / maxPerRow) : 1;

    const totalW = chain.length * NODE_W + Math.max(0, chain.length - 1) * GAP;
    const scale = Math.min(1, (w - 2 * CANVAS_LAYOUT.margin) / Math.max(1, totalW));
    const nodeW = wrap ? NODE_W : Math.max(44, Math.round(NODE_W * scale));
    const gap = wrap ? GAP : Math.max(16, Math.round(GAP * scale));

    const totalRowW = wrap
      ? maxPerRow * nodeW + (maxPerRow - 1) * gap + GAP + NULL_NODE
      : chain.length * nodeW + Math.max(0, chain.length - 1) * gap + GAP + NULL_NODE;
    const startX = Math.max(CANVAS_LAYOUT.margin, (w - totalRowW) / 2);

    // Căn giữa theo chiều dọc khối (1 hoặc nhiều hàng); hàng r đặt tại rowY(r).
    const blockH = rows * NODE_H + (rows - 1) * CANVAS_LAYOUT.rowGap;
    const y = Math.max(24, (h - blockH) / 2);
    const rowY = (r: number): number => y + r * (NODE_H + CANVAS_LAYOUT.rowGap);
    // Cột của node cuối trong hàng cuối (hàng cuối có thể không đầy).
    const lastCol = chain.length - 1 - (rows - 1) * maxPerRow;

    // Vẽ chuỗi nút + mũi tên next. Cùng hàng: mũi tên thẳng; cuối hàng → đầu hàng
    // kế: mũi tên cong xuống (curve). wrap=false → maxPerRow chỉ là ngưỡng tham chiếu,
    // mọi node nằm hàng 0 và luôn dùng arrow (giữ nguyên hành vi cũ).
    chain.forEach((el, i) => {
      const col = wrap ? i % maxPerRow : i;
      const r = wrap ? Math.floor(i / maxPerRow) : 0;
      const nx = startX + col * (nodeW + gap);
      const ny = rowY(r);
      this.drawNode(el, nx, ny, nodeW, options);
      if (i < chain.length - 1) {
        const color = this.edgeColorFor(el);
        if (wrap && col === maxPerRow - 1) {
          // Nút cuối hàng r → nút đầu hàng r+1: cong nhẹ xuống dưới.
          const xEnd = startX + (maxPerRow - 1) * (nodeW + gap) + nodeW + gap - 2;
          this.painter.curve(xEnd, ny + NODE_H / 2, startX + 2, rowY(r + 1) + NODE_H / 2, color, 2, 12, true);
        } else {
          this.painter.arrow(nx + nodeW + 2, ny + NODE_H / 2, nx + nodeW + gap - 2, ny + NODE_H / 2, color, 2, 7);
        }
      }
    });

    // Ô null cuối chuỗi (∅): sau node cuối thật của hàng cuối.
    if (chain.length > 0) {
      const lastNodeX = startX + lastCol * (nodeW + gap);
      const nullX = lastNodeX + nodeW + gap;
      const midY = rowY(rows - 1) + NODE_H / 2;
      this.painter.arrow(lastNodeX + nodeW + 2, midY, nullX - 2, midY, CANVAS_COLORS.muted, 2, 7);
      this.drawNullNode(nullX, rowY(rows - 1) + (NODE_H - NULL_NODE) / 2);
    } else {
      // Danh sách rỗng: chỉ vẽ ô null giữa màn hình.
      this.drawNullNode(w / 2 - NULL_NODE / 2, h / 2 - NULL_NODE / 2);
    }

    // Nhãn head / tail phía trên nút đầu (hàng 0) / nút cuối (hàng cuối).
    if (chain.length > 0 && options.showValues) {
      if (chain.length === 1) {
        this.painter.label('head / tail', startX + nodeW / 2, rowY(0) - 12, CANVAS_COLORS.compare, 10);
      } else {
        this.painter.label('head', startX + nodeW / 2, rowY(0) - 12, CANVAS_COLORS.compare, 10);
        this.painter.label('tail', startX + lastCol * (nodeW + gap) + nodeW / 2, rowY(rows - 1) - 12, CANVAS_COLORS.muted, 10);
      }
    }

    // Nút nổi (chưa nối vào chuỗi — thao tác insert): vẽ bên dưới hàng cuối, viền đứt nét.
    const floating = structure.elements.filter((el) => !seen.has(el.id) && el.group !== 'pointer');
    if (floating.length > 0) {
      const fy = rowY(rows - 1) + NODE_H + FLOAT_GAP;
      floating.forEach((el, i) => {
        const fx = startX + i * (nodeW + gap);
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
