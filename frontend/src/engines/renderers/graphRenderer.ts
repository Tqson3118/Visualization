// engines/renderers/graphRenderer.ts — Renderer đồ thị (SDD §8.3)
//
// Quy ước vẽ:
// - Đỉnh tròn bố trí vòng tròn quanh tâm; cạnh vô hướng = đoạn thẳng, có hướng = mũi tên.
// - Trọng số = nhãn ở giữa cạnh (link.label, VD 'w=4').
// - Tô theo thứ tự duyệt (BFS/DFS): done = đã thăm (màu sorted), active = đang xét (compare).
// - Dijkstra: vẽ d[] dưới đỉnh khi el.meta.d là số hoặc el.label dạng 'd[x]=...'.

import type { Element, Structure } from '@/engines/core/types';
import { CANVAS_COLORS, hexToRgba } from './canvasTheme';
import type { Renderer, RenderOptions } from './interface';
import { CanvasPainter } from './painter/canvasPainter';

const NODE_R = 20;   // SDD §8.3: đỉnh tròn
const LABEL_OFFSET = 14;

export class GraphRenderer implements Renderer {
  supportedKinds: string[] = ['graph'];

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
    const n = structure.elements.length;
    if (n === 0) return;

    const radius = Math.max(50, Math.min(w, h) / 2 - 54);
    const cx = w / 2;
    const cy = h / 2;

    // Vị trí đỉnh theo vòng tròn, thứ tự theo id (node:0, node:1...).
    const vertices = structure.elements.slice().sort((a, b) => (CanvasPainter.indexFromId(a.id) ?? 0) - (CanvasPainter.indexFromId(b.id) ?? 0));
    const posOf = new Map<string, { x: number; y: number }>();
    vertices.forEach((el, i) => {
      const angle = (i / Math.max(1, n)) * Math.PI * 2 - Math.PI / 2;
      posOf.set(el.id, { x: cx + radius * Math.cos(angle), y: cy + radius * Math.sin(angle) });
    });

    const directed = this.isDirected(structure);

    // Vẽ cạnh trước (nằm dưới đỉnh).
    for (const link of structure.links) {
      const from = posOf.get(link.from);
      const to = posOf.get(link.to);
      if (!from || !to) continue;
      const color = this.painter.edgeColor(link.status);
      const sx = from.x + (to.x - from.x) * 0.12;
      const sy = from.y + (to.y - from.y) * 0.12;
      const ex = to.x - (to.x - from.x) * 0.12;
      const ey = to.y - (to.y - from.y) * 0.12;
      if (directed) {
        this.painter.arrow(sx, sy, ex, ey, color, 2, 8);
      } else {
        this.painter.line(sx, sy, ex, ey, color, 2);
      }
      if (link.label) {
        const mx = (from.x + to.x) / 2;
        const my = (from.y + to.y) / 2 - 6;
        this.painter.label(link.label, mx, my, CANVAS_COLORS.muted, 10);
      }
    }

    // Vẽ đỉnh.
    for (const el of vertices) {
      const pos = posOf.get(el.id);
      if (!pos) continue;
      this.drawVertex(el, pos.x, pos.y, options);
    }
  }

  private drawVertex(el: Element, x: number, y: number, options: RenderOptions): void {
    const muted = el.status === 'muted';
    const fill = muted ? this.painter.statusColorWithAlpha('muted', 0.5) : this.painter.statusColor(el.status);
    const stroke = hexToRgba(this.painter.statusColor(el.status), 0.8);
    this.painter.circle(x, y, NODE_R, fill, stroke);

    if (options.showValues) {
      // Nhãn chính (số đỉnh) — bỏ qua nhãn dạng d[] (vẽ riêng phía dưới).
      if (!/^d\[/.test(el.label)) {
        this.painter.text(el.label, x, y, { size: 12, weight: 'bold', color: muted ? CANVAS_COLORS.muted : CANVAS_COLORS.text });
      }
    }

    // Dijkstra d[]: meta.d là số, hoặc label dạng 'd[x]=...'.
    const dist = el.meta?.d;
    if (typeof dist === 'number') {
      this.painter.label(`d=${dist}`, x, y + NODE_R + LABEL_OFFSET, CANVAS_COLORS.compare, 10);
    } else if (/^d\[/.test(el.label)) {
      this.painter.label(el.label, x, y + NODE_R + LABEL_OFFSET, CANVAS_COLORS.compare, 10);
    }
  }

  /** Có hướng hay không: ưu tiên meta.directed của generator; heuristic cặp ngược → có hướng. */
  private isDirected(structure: Structure): boolean {
    for (const el of structure.elements) {
      if (typeof el.meta?.directed === 'boolean') return el.meta.directed;
    }
    const pairs = new Set<string>();
    for (const link of structure.links) pairs.add(`${link.from}|${link.to}`);
    for (const link of structure.links) {
      if (pairs.has(`${link.to}|${link.from}`)) return true;
    }
    // Mặc định vẽ mũi tên (thể hiện hướng duyệt BFS/DFS).
    return true;
  }
}
