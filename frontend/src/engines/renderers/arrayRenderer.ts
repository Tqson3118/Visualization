// engines/renderers/arrayRenderer.ts — Renderer mảng (SDD §8.3)
//
// Quy ước vẽ:
// - Dãy ô vuông ngang, mặc định 60×60 (thu nhỏ khi mảng dài).
// - Index dưới ô, giá trị bên trong ô.
// - Con trỏ = mũi tên + nhãn trên ô: element group='pointer' (meta.target = id ô đích)
//   hoặc element có meta.pointer=true (vẽ mũi tên trên chính ô đó).
// - Vùng bỏ qua (status 'muted') làm mờ.

import type { Element, Structure } from '@/engines/core/types';
import { CANVAS_COLORS, CANVAS_LAYOUT, hexToRgba } from './canvasTheme';
import type { Renderer, RenderOptions } from './interface';
import { CanvasPainter } from './painter/canvasPainter';

/** Alpha làm mờ vùng bỏ qua (status 'muted') — SDD §8.3. */
export const MUTED_CELL_ALPHA = 0.45;

const CELL_SIZE = 60;       // SDD §8.3: ô vuông 60×60
const MIN_CELL_SIZE = 28;   // mảng dài (≤100 phần tử) → thu nhỏ ô
const POINTER_ARROW_LEN = 26;

export class ArrayRenderer implements Renderer {
  supportedKinds: string[] = ['array'];

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
    const elements = structure.elements;
    const n = elements.length;
    if (n === 0) return;

    const cellSize = Math.max(MIN_CELL_SIZE, Math.min(CELL_SIZE, (w - 2 * CANVAS_LAYOUT.margin) / n));
    const y0 = Math.max(16, (h - cellSize) / 2);

    // Sắp xếp theo số thứ tự trong id (cell:0, cell:1...) để vẽ đúng trật tự mảng.
    const cells = elements.slice().sort((a, b) => (CanvasPainter.indexFromId(a.id) ?? 0) - (CanvasPainter.indexFromId(b.id) ?? 0));

    // Vị trí tâm + đỉnh của từng ô (dùng cho con trỏ).
    const positions = new Map<string, { cx: number; top: number }>();
    cells.forEach((el, i) => {
      const x = CANVAS_LAYOUT.margin + i * cellSize;
      const cx = x + cellSize / 2;
      positions.set(el.id, { cx, top: y0 });
      // Element nhóm 'pointer' không phải ô dữ liệu → vẽ ở bước con trỏ.
      if (el.group !== 'pointer') this.drawCell(el, x, y0, cellSize, options);
    });

    // Con trỏ: mũi tên + nhãn phía trên ô đích (L/R/M/P...).
    for (const el of elements) {
      if (el.group === 'pointer' || el.meta?.pointer === true) {
        this.drawPointer(el, positions, options);
      }
    }
  }

  private drawCell(el: Element, x: number, y: number, cellSize: number, options: RenderOptions): void {
    const cx = x + cellSize / 2;
    const cy = y + cellSize / 2;
    const muted = el.status === 'muted';
    const fill = muted ? this.painter.statusColorWithAlpha(el.status, MUTED_CELL_ALPHA) : this.painter.statusColor(el.status);
    const stroke = muted ? hexToRgba(CANVAS_COLORS.muted, 0.5) : hexToRgba(this.painter.statusColor(el.status), 0.7);

    this.painter.roundRect(x, y, cellSize, cellSize, CANVAS_LAYOUT.borderRadius, fill, stroke);

    // Giá trị bên trong ô.
    if (options.showValues && el.label !== '—') {
      this.painter.text(el.label, cx, cy, {
        size: cellSize >= 40 ? 14 : 11,
        weight: 'bold',
        color: muted ? CANVAS_COLORS.muted : CANVAS_COLORS.text,
      });
    }
    // Index dưới ô.
    if (options.showIndex) {
      const idx = CanvasPainter.indexFromId(el.id);
      if (idx !== null) {
        this.painter.label(String(idx), cx, y + cellSize + 14);
      }
    }
  }

  private drawPointer(el: Element, positions: Map<string, { cx: number; top: number }>, options: RenderOptions): void {
    let pos: { cx: number; top: number } | undefined;
    if (el.group === 'pointer') {
      // Element con trỏ riêng: meta.target = id ô đích, hoặc meta.index = số thứ tự ô đích.
      const target = typeof el.meta?.target === 'string' ? el.meta.target : undefined;
      if (target) {
        pos = positions.get(target);
      } else {
        const targetIndex = typeof el.meta?.index === 'number' ? el.meta.index : CanvasPainter.indexFromId(el.id);
        if (targetIndex !== null) {
          for (const [id, p] of positions) {
            if (CanvasPainter.indexFromId(id) === targetIndex) {
              pos = p;
              break;
            }
          }
        }
      }
    } else {
      // meta.pointer=true — con trỏ gắn trên chính ô dữ liệu.
      pos = positions.get(el.id);
    }
    if (!pos) return;

    const arrowColor = CANVAS_COLORS.compare;
    const startY = pos.top - POINTER_ARROW_LEN - 8;
    const endY = pos.top - 4;
    this.painter.arrow(pos.cx, startY, pos.cx, endY, arrowColor, 2, 8);
    const labelText = options.showValues ? el.label : '';
    if (labelText) {
      this.painter.label(labelText, pos.cx, startY - 8, CANVAS_COLORS.compare, 12);
    }
  }
}
