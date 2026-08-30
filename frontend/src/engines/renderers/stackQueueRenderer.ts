// engines/renderers/stackQueueRenderer.ts — Renderer stack + queue (SDD §8.3)
//
// Quy ước vẽ:
// - Stack: dãy dọc, đỉnh trên cùng (cell:0 ở đáy, cell cuối ở đỉnh); phần tử pop
//   (status 'swap') bay lên (dịch lên + giảm alpha).
// - Queue: dãy ngang, front trái / rear phải; phần tử dequeue (status 'swap')
//   bay ra trái (dịch trái + giảm alpha).
// - Ô trống (label '—') vẽ mờ.

import type { Element, Structure } from '@/engines/core/types';
import { CANVAS_COLORS, CANVAS_LAYOUT, hexToRgba } from './canvasTheme';
import type { Renderer, RenderOptions } from './interface';
import { CanvasPainter } from './painter/canvasPainter';

const CELL_SIZE = 56;        // kích thước ô cơ bản
const MIN_CELL_SIZE = 28;
const FLY_OFFSET = 18;       // quãng "bay" khi pop/dequeue (SDD: ≤200ms — vẽ tĩnh, không animate)
const FLY_ALPHA = 0.75;

export class StackQueueRenderer implements Renderer {
  supportedKinds: string[] = ['stack', 'queue'];

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

    if (structure.kind === 'stack') {
      this.renderStack(structure, options);
    } else {
      this.renderQueue(structure, options);
    }
  }

  // ── Stack: dãy dọc, đỉnh trên cùng ────────────────────────────────────────

  private renderStack(structure: Structure, options: RenderOptions): void {
    const w = this.painter.logicalWidth;
    const h = this.painter.logicalHeight;
    const n = Math.max(1, structure.elements.length);
    const cellSize = Math.max(MIN_CELL_SIZE, Math.min(CELL_SIZE, (h - 2 * CANVAS_LAYOUT.margin) / n));
    const cx = w / 2;
    const totalHeight = n * cellSize;
    const bottom = Math.min(h - CANVAS_LAYOUT.margin, (h + totalHeight) / 2);
    const cells = sortedElements(structure);

    let topCellY: number | null = null;
    cells.forEach((el, i) => {
      let cy = bottom - (i + 1) * cellSize + cellSize / 2;
      const flying = el.status === 'swap' && !hasAnimOverride(el);
      if (flying) cy -= FLY_OFFSET; // pop: bay lên
      this.drawCell(el, cx - cellSize / 2, cy - cellSize / 2, cellSize, cellSize, options, flying ? FLY_ALPHA : undefined);
      if (topCellY === null && !isEmptyCell(el)) topCellY = cy;
    });

    // Nhãn "đỉnh" bên phải ô trên cùng (top) kèm mũi tên chỉ rõ ràng.
    if (topCellY !== null && options.showValues) {
      this.painter.label('← top', cx + cellSize / 2 + 24, topCellY, CANVAS_COLORS.compare, 11);
    }
  }

  // ── Queue: dãy ngang, front trái / rear phải ──────────────────────────────

  private renderQueue(structure: Structure, options: RenderOptions): void {
    const w = this.painter.logicalWidth;
    const h = this.painter.logicalHeight;
    const n = Math.max(1, structure.elements.length);
    const cellSize = Math.max(MIN_CELL_SIZE, Math.min(CELL_SIZE, (w - 2 * CANVAS_LAYOUT.margin) / n));
    const cy = h / 2;
    const totalWidth = n * cellSize;
    const startX = Math.max(CANVAS_LAYOUT.margin, (w - totalWidth) / 2);
    const cells = sortedElements(structure);

    // Front = ô có dữ liệu đầu tiên, rear = ô có dữ liệu cuối cùng.
    const filled: Element[] = cells.filter((el) => !isEmptyCell(el));
    const frontEl = filled[0];
    const rearEl = filled[filled.length - 1];

    cells.forEach((el, i) => {
      let cx = startX + i * cellSize + cellSize / 2;
      const flying = el.status === 'swap' && !hasAnimOverride(el);
      if (flying) cx -= FLY_OFFSET; // dequeue: bay ra trái
      this.drawCell(el, cx - cellSize / 2, cy - cellSize / 2, cellSize, cellSize, options, flying ? FLY_ALPHA : undefined, 'queue');
      
      if (options.showValues) {
        if (frontEl && rearEl && frontEl.id === rearEl.id && el.id === frontEl.id) {
          this.painter.label('front / rear', cx, cy + cellSize / 2 + 14, CANVAS_COLORS.compare, 11);
        } else {
          if (frontEl && el.id === frontEl.id) {
            this.painter.label('front', cx, cy + cellSize / 2 + 14, CANVAS_COLORS.compare, 11);
          }
          if (rearEl && el.id === rearEl.id) {
            this.painter.label('rear', cx, cy + cellSize / 2 + 14, CANVAS_COLORS.muted, 11);
          }
        }
      }
    });
  }

  private drawCell(
    el: Element,
    x: number,
    y: number,
    w: number,
    h: number,
    options: RenderOptions,
    alpha?: number,
    kind: 'stack' | 'queue' = 'stack',
  ): void {
    // Override meta từ useStructureTransition (Task 3): animX/animY dịch tương đối
    // (element trượt/bay trong lúc push/pop), animAlpha thay thế alpha (fade khi pop/dequeue).
    const anim = el.meta;
    const animX = typeof anim?.animX === 'number' ? anim.animX : 0;
    const animY = typeof anim?.animY === 'number' ? anim.animY : 0;
    const alphaOverride = typeof anim?.animAlpha === 'number' ? anim.animAlpha : undefined;
    x += animX;
    y += animY;
    const cx = x + w / 2;
    const cy = y + h / 2;
    const empty = isEmptyCell(el);
    const effAlpha = alphaOverride ?? alpha;
    const fill = effAlpha !== undefined
      ? this.painter.statusColorWithAlpha(el.status, effAlpha)
      : empty
        ? this.painter.statusColorWithAlpha('muted', 0.35)
        : this.painter.statusColor(el.status);
    const stroke = hexToRgba(empty ? CANVAS_COLORS.muted : this.painter.statusColor(el.status), 0.5);

    this.painter.roundRect(x, y, w, h, CANVAS_LAYOUT.borderRadius, fill, stroke);

    if (options.showValues) {
      this.painter.text(el.label, cx, cy, {
        size: w >= 40 ? 13 : 10,
        weight: 'bold',
        color: empty ? CANVAS_COLORS.muted : CANVAS_COLORS.text,
      });
    }
    // Index: bên trái cho stack (tránh đè 'top' bên phải), hoặc dưới đáy cho queue.
    if (options.showIndex) {
      const idx = CanvasPainter.indexFromId(el.id);
      if (idx !== null) {
        if (kind === 'stack') {
          this.painter.label(String(idx), x - 10, cy, CANVAS_COLORS.muted, 10);
        } else {
          this.painter.label(String(idx), cx, y + h + 12, CANVAS_COLORS.muted, 10);
        }
      }
    }
  }
}

function isEmptyCell(el: Element): boolean {
  return el.label === '—' || el.meta?.empty === true;
}

/** Element đang bị transition điều khiển (useStructureTransition) → KHÔNG áp dụng FLY_OFFSET tĩnh. */
function hasAnimOverride(el: Element): boolean {
  const anim = el.meta;
  return typeof anim?.animX === 'number' || typeof anim?.animY === 'number' || typeof anim?.animAlpha === 'number';
}

function sortedElements(structure: Structure): Element[] {
  return structure.elements.slice().sort((a, b) => (CanvasPainter.indexFromId(a.id) ?? 0) - (CanvasPainter.indexFromId(b.id) ?? 0));
}
