// engines/renderers/graphRenderer.ts — Renderer đồ thị (SDD §8.3)
//
// Quy ước vẽ:
// - Bố trí: mọi đỉnh có meta.x/meta.y (số) → normalize tọa độ vào vùng vẽ (có lề);
//   ngược lại → vòng tròn quanh tâm. Cạnh vô hướng = đoạn thẳng, có hướng = mũi tên.
// - Trọng số = nhãn ở giữa cạnh (link.label, VD 'w=4').
// - Tô theo thứ tự duyệt (BFS/DFS): done = đã thăm (màu sorted), active = đang xét (compare).
// - Dijkstra: vẽ d[] dưới đỉnh khi el.meta.d là số hoặc el.label dạng 'd[x]=...'.
// - Tên đỉnh (el.label) vẽ thêm bên dưới circle khi showValues (khác el.id, không phải d[]).

import type { Element, Structure } from '@/engines/core/types';
import { CANVAS_COLORS, CANVAS_LAYOUT, hexToRgba } from './canvasTheme';
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

    // Meta tọa độ (element.meta.x/y): nếu MỌI đỉnh (trừ nhóm pointer) đều có tọa độ số
    // thì bố trí theo tọa độ (normalize vào vùng vẽ), ngược lại giữ vòng tròn như cũ.
    const positioned = vertices.filter((el) => el.group !== 'pointer');
    const metaXY = (el: Element): { x: number; y: number } | null => {
      const x = el.meta?.x;
      const y = el.meta?.y;
      return typeof x === 'number' && typeof y === 'number' ? { x, y } : null;
    };
    const useMeta = positioned.length > 0 && positioned.every((el) => metaXY(el) !== null);

    if (useMeta) {
      const coords = positioned.map((el) => metaXY(el)!);
      const minX = Math.min(...coords.map((c) => c.x));
      const maxX = Math.max(...coords.map((c) => c.x));
      const minY = Math.min(...coords.map((c) => c.y));
      const maxY = Math.max(...coords.map((c) => c.y));
      const spanX = maxX - minX;
      const spanY = maxY - minY;
      const spreadX = spanX > 1e-6;
      const spreadY = spanY > 1e-6;
      if (spreadX && spreadY) {
        // Normalize cả 2 trục vào vùng vẽ (có lề) — giữ tỉ lệ tương đối của meta.
        coords.forEach((c, i) => {
          posOf.set(positioned[i].id, {
            x: CANVAS_LAYOUT.margin + ((c.x - minX) / spanX) * (w - 2 * CANVAS_LAYOUT.margin),
            y: CANVAS_LAYOUT.paddingTop + ((c.y - minY) / spanY) * (h - CANVAS_LAYOUT.margin - CANVAS_LAYOUT.paddingTop),
          });
        });
      } else if (spreadX) {
        // (b) Chỉ x phân biệt → dàn ngang, y giữa vùng vẽ.
        coords.forEach((c, i) => {
          posOf.set(positioned[i].id, {
            x: CANVAS_LAYOUT.margin + ((c.x - minX) / spanX) * (w - 2 * CANVAS_LAYOUT.margin),
            y: h / 2,
          });
        });
      } else {
        // (a)/(c) Không có biên độ x (hoặc x,y đều trùng) → vòng tròn như cũ.
        positioned.forEach((el, i) => {
          const angle = (i / Math.max(1, positioned.length)) * Math.PI * 2 - Math.PI / 2;
          posOf.set(el.id, { x: cx + radius * Math.cos(angle), y: cy + radius * Math.sin(angle) });
        });
      }
      // Đỉnh ngoài nhóm meta (vd pointer) vẫn giữ vòng tròn cũ — không mất đỉnh.
      vertices.forEach((el, i) => {
        if (!posOf.has(el.id)) {
          const angle = (i / Math.max(1, n)) * Math.PI * 2 - Math.PI / 2;
          posOf.set(el.id, { x: cx + radius * Math.cos(angle), y: cy + radius * Math.sin(angle) });
        }
      });
    } else {
      // Layout vòng tròn cũ — KHÔNG đổi.
      vertices.forEach((el, i) => {
        const angle = (i / Math.max(1, n)) * Math.PI * 2 - Math.PI / 2;
        posOf.set(el.id, { x: cx + radius * Math.cos(angle), y: cy + radius * Math.sin(angle) });
      });
    }

    const nodeR = Math.max(10, Math.min(NODE_R, Math.floor((2 * Math.PI * radius) / (Math.max(1, vertices.length) * 2.5))));
    const directed = this.isDirected(structure);

    // Vẽ cạnh trước (nằm dưới đỉnh).
    for (const link of structure.links) {
      const from = posOf.get(link.from);
      const to = posOf.get(link.to);
      if (!from || !to) continue;
      const color = this.painter.edgeColor(link.status);
      const dx = to.x - from.x;
      const dy = to.y - from.y;
      const dist = Math.hypot(dx, dy);
      const ratio = dist > 0 ? Math.min(0.45, (nodeR + 3) / dist) : 0.12;
      const sx = from.x + dx * ratio;
      const sy = from.y + dy * ratio;
      const ex = to.x - dx * ratio;
      const ey = to.y - dy * ratio;
      if (directed) {
        this.painter.arrow(sx, sy, ex, ey, color, 2, Math.max(5, Math.min(8, nodeR * 0.45)));
      } else {
        this.painter.line(sx, sy, ex, ey, color, 2);
      }
      if (link.label) {
        const mx = (from.x + to.x) / 2;
        const my = (from.y + to.y) / 2 - 6;
        this.painter.label(link.label, mx, my, CANVAS_COLORS.muted, Math.max(8, Math.min(10, nodeR * 0.6)));
      }
    }

    // Vẽ đỉnh.
    for (const el of vertices) {
      const pos = posOf.get(el.id);
      if (!pos) continue;
      this.drawVertex(el, pos.x, pos.y, nodeR, options);
    }
  }

  private drawVertex(el: Element, x: number, y: number, r: number, options: RenderOptions): void {
    const muted = el.status === 'muted';
    const fill = muted ? this.painter.statusColorWithAlpha('muted', 0.5) : this.painter.statusColor(el.status);
    const stroke = hexToRgba(this.painter.statusColor(el.status), 0.8);
    // Glow cho nút đang xét.
    if (el.status === 'active' || el.status === 'highlight') {
      this.painter.arcGlow(x, y, r, this.painter.statusColor(el.status), Math.min(8, r * 0.4));
    }
    this.painter.circle(x, y, r, fill, stroke);
    let extraYOffset = 0;
    if (options.showValues) {
      const fontSize = Math.max(9, Math.min(12, Math.round(r * 0.85)));
      // Nhãn chính (số đỉnh) — bỏ qua nhãn dạng d[] (vẽ riêng phía dưới).
      if (!/^d\[/.test(el.label)) {
        this.painter.text(el.label, x, y, {
          size: fontSize,
          weight: 'bold',
          color: muted ? CANVAS_COLORS.muted : CANVAS_COLORS.text,
        });
      }
      // Tên đỉnh bên dưới circle — bỏ qua khi trùng id hoặc nhãn dạng d[].
      if (el.label !== el.id && !/^d\[/.test(el.label)) {
        this.painter.label(el.label, x, y + r + LABEL_OFFSET, CANVAS_COLORS.muted, 10);
        extraYOffset = 12;
      }
    }

    // Dijkstra d[]: meta.d là số, hoặc label dạng 'd[x]=...'.
    const dist = el.meta?.d;
    if (typeof dist === 'number') {
      this.painter.label(`d=${dist}`, x, y + r + LABEL_OFFSET + extraYOffset, CANVAS_COLORS.compare, 10);
    } else if (/^d\[/.test(el.label)) {
      this.painter.label(el.label, x, y + r + LABEL_OFFSET + extraYOffset, CANVAS_COLORS.compare, 10);
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
    return false;
  }
}
