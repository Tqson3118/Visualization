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

    // Nhãn hàm băm (SDD §8.3): hiển thị theo meta (mặc định k mod m hoặc phép nhân)
    let formula = 'h(k) = k mod m';
    for (const el of structure.elements) {
      if (el.meta?.formula) {
        formula = String(el.meta.formula);
        break;
      }
      if (el.meta?.hashMode === 'multiplication') {
        formula = 'h(k) = ⌊m · (k · A mod 1)⌋';
        break;
      }
    }
    this.painter.text(formula, CANVAS_LAYOUT.margin + 8, 20, { size: 12, color: CANVAS_COLORS.muted, align: 'left' });

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

      // Header bucket. Clamp hw ≥ 12: với nhiều bucket (vd m=11) + canvas hẹp,
      // colW - 16 có thể âm → roundRect(width âm) ném IndexSizeError (SDD §8.3).
      if (rec.header) {
        const hw = Math.max(12, Math.min(70, colW - 16));
        const hx = colX + (colW - hw) / 2;
        const headerFill = this.painter.statusColorWithAlpha(rec.header.status, 0.22);
        const headerStroke = this.painter.statusColor(rec.header.status);
        this.painter.roundRect(hx, HEADER_Y, hw, HEADER_H, CANVAS_LAYOUT.borderRadius, headerFill, headerStroke);
        this.painter.text(rec.header.label, hx + hw / 2, HEADER_Y + HEADER_H / 2, { size: 12, weight: 'bold' });
      }

      // Chuỗi nối kết dọc trong bucket (chaining)
      const nodes = rec.nodes
        .slice()
        .sort((a, b) => (typeof a.meta?.chainPos === 'number' ? a.meta.chainPos : (CanvasPainter.indexFromId(a.id) ?? 0))
          - (typeof b.meta?.chainPos === 'number' ? b.meta.chainPos : (CanvasPainter.indexFromId(b.id) ?? 0)));
      if (nodes.length === 0) {
        this.painter.text('∅', centerX, HEADER_Y + HEADER_H + 20, { size: 14, color: CANVAS_COLORS.muted });
        return;
      }

      const nodeW = Math.max(24, Math.min(56, colW - 12));
      const nodeH = 26;
      const chainVGap = 16;
      const x = colX + (colW - nodeW) / 2;

      // Mũi tên từ header xuống nút đầu tiên
      this.painter.arrow(centerX, HEADER_Y + HEADER_H + 2, centerX, HEADER_Y + HEADER_H + chainVGap - 2, hexToRgba(CANVAS_COLORS.text, 0.4), 2, 5);

      let curY = HEADER_Y + HEADER_H + chainVGap;
      nodes.forEach((el, i) => {
        const muted = el.status === 'muted';
        const fill = muted ? this.painter.statusColorWithAlpha('muted', 0.5) : this.painter.statusColor(el.status);
        const stroke = hexToRgba(this.painter.statusColor(el.status), 0.8);
        this.painter.roundRect(x, curY, nodeW, nodeH, CANVAS_LAYOUT.borderRadius, fill, stroke);
        if (options.showValues) {
          this.painter.text(el.label, x + nodeW / 2, curY + nodeH / 2, { size: 11, weight: 'bold', color: muted ? CANVAS_COLORS.muted : CANVAS_COLORS.text });
        }
        // Mũi tên xuống nút kế tiếp trong chuỗi
        if (i < nodes.length - 1) {
          this.painter.arrow(centerX, curY + nodeH + 2, centerX, curY + nodeH + chainVGap - 2, hexToRgba(CANVAS_COLORS.text, 0.4), 2, 5);
        }
        curY += nodeH + chainVGap;
      });
    });
  }
}
