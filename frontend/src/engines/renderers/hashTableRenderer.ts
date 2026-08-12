// engines/renderers/hashTableRenderer.ts — Renderer bảng băm chuỗi nối kết (SDD §8.3)
//
// Quy ước vẽ:
// - Lưới bucket (cột dọc): mỗi bucket 1 cột, header `[i]` trên cùng (element id bucket:i).
// - Trong mỗi bucket: chuỗi nối kết NGANG các nút (element nhóm 'bucket:i'),
//   nối bằng mũi tên → (theo links hoặc thứ tự chainPos).
// - Nhãn hàm băm `h(k) = k mod m` ở góc trên-trái.

import type { Element, Structure } from '@/engines/core/types';
import { CANVAS_COLORS, CANVAS_LAYOUT, hexToRgba } from './canvasTheme';
import type { Renderer, RenderOptions } from './interface';
import { CanvasPainter } from './painter/canvasPainter';

const HEADER_H = 30;
const HEADER_Y = 46;
const CHAIN_GAP = 20;    // khoảng mũi tên giữa 2 nút trong chuỗi

export class HashTableRenderer implements Renderer {
  supportedKinds: string[] = ['hashtable'];

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

    // Nhãn hàm băm (SDD §8.3): h(k) = k mod m.
    this.painter.label('h(k) = k mod m', CANVAS_LAYOUT.margin, 20, CANVAS_COLORS.muted, 12);

    // Gom element theo bucket: header (id bucket:i) + các nút trong chuỗi.
    const buckets = new Map<number, { header: Element | null; nodes: Element[] }>();
    for (const el of structure.elements) {
      const m = /^bucket:(\d+)$/.exec(el.group ?? '');
      if (!m) continue;
      const b = Number(m[1]);
      let rec = buckets.get(b);
      if (!rec) {
        rec = { header: null, nodes: [] };
        buckets.set(b, rec);
      }
      if (el.id === `bucket:${b}`) {
        rec.header = el;
      } else {
        rec.nodes.push(el);
      }
    }
    const bucketIds = Array.from(buckets.keys()).sort((a, b) => a - b);
    if (bucketIds.length === 0) return;

    const colW = (w - 2 * CANVAS_LAYOUT.margin) / bucketIds.length;

    // Link từ → đến (chuỗi trong bucket) — dùng để vẽ mũi tên chính xác.
    const linkTargets = new Map<string, Element>();
    const byId = new Map<string, Element>();
    for (const el of structure.elements) byId.set(el.id, el);
    for (const link of structure.links) {
      const target = byId.get(link.to);
      if (target) linkTargets.set(link.from, target);
    }

    bucketIds.forEach((b, col) => {
      const rec = buckets.get(b);
      if (!rec) return;
      const colX = CANVAS_LAYOUT.margin + col * colW;
      const centerX = colX + colW / 2;

      // Header bucket.
      if (rec.header) {
        const hx = colX + (colW - Math.min(70, colW - 16)) / 2;
        const hw = Math.min(70, colW - 16);
        const headerFill = this.painter.statusColorWithAlpha(rec.header.status, 0.22);
        const headerStroke = this.painter.statusColor(rec.header.status);
        this.painter.roundRect(hx, HEADER_Y, hw, HEADER_H, CANVAS_LAYOUT.borderRadius, headerFill, headerStroke);
        this.painter.text(rec.header.label, hx + hw / 2, HEADER_Y + HEADER_H / 2, { size: 12, weight: 'bold' });
      }

      // Chuỗi nối kết ngang trong bucket.
      const nodes = rec.nodes
        .slice()
        .sort((a, b) => (typeof a.meta?.chainPos === 'number' ? a.meta.chainPos : (CanvasPainter.indexFromId(a.id) ?? 0))
          - (typeof b.meta?.chainPos === 'number' ? b.meta.chainPos : (CanvasPainter.indexFromId(b.id) ?? 0)));
      if (nodes.length === 0) {
        this.painter.text('∅', centerX, HEADER_Y + HEADER_H + 26, { size: 14, color: CANVAS_COLORS.muted });
        return;
      }

      const nodeW = Math.max(28, Math.min(52, colW - CHAIN_GAP - 12));
      const nodeH = 30;
      const totalW = nodes.length * nodeW + (nodes.length - 1) * CHAIN_GAP;
      const y = HEADER_Y + HEADER_H + 20;
      let x = centerX - totalW / 2;
      nodes.forEach((el, i) => {
        const muted = el.status === 'muted';
        const fill = muted ? this.painter.statusColorWithAlpha('muted', 0.5) : this.painter.statusColor(el.status);
        const stroke = hexToRgba(this.painter.statusColor(el.status), 0.8);
        this.painter.roundRect(x, y, nodeW, nodeH, CANVAS_LAYOUT.borderRadius, fill, stroke);
        if (options.showValues) {
          this.painter.text(el.label, x + nodeW / 2, y + nodeH / 2, { size: 11, weight: 'bold', color: muted ? CANVAS_COLORS.muted : CANVAS_COLORS.text });
        }
        // Mũi tên sang nút kế tiếp (theo links nếu có).
        const next = linkTargets.get(el.id);
        if (next && nodes.some((nn) => nn.id === next.id)) {
          this.painter.arrow(x + nodeW + 2, y + nodeH / 2, x + nodeW + CHAIN_GAP - 2, y + nodeH / 2, hexToRgba(CANVAS_COLORS.text, 0.4), 2, 6);
        }
        x += nodeW + CHAIN_GAP;
      });
    });
  }
}
