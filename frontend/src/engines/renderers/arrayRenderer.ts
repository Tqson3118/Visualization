// engines/renderers/arrayRenderer.ts — Renderer mảng (SDD §8.3)
//
// Quy tắc vẽ:
// - Dãy ô vuông ngang, mặc định 60×60 (thu nhỏ khi mảng dài).
// - Index dưới ô, giá trị bên trong ô.
// - Con trỏ = mũi tên + nhãn trên ô: element group='pointer' (meta.target = id ô đích)
//   hoặc element có meta.pointer=true (vẽ mũi tên trên chính ô đó).
// - Vùng bỏ qua (status 'muted') làm mờ.
// - BAR MODE (13/08 — khắc phục "canvas đen loãng", nguồn V3 renderArrayBar): khi mọi label là số,
//   vẽ bar cao tỉ lệ giá trị từ đáy canvas (gradient + glow) — tận dụng chiều cao, không còn 1 hàng
//   ô nhỏ lơ lửng giữa khoảng đen; giá trị nằm trên bar, index dưới đáy bar. Label chữ → giữ ô vuông.
// - WRAP (14/08 — Task 7): mảng dài → chia nhiều hàng (bar: slotW < 44; ô: cellSize < 36), hàng cách
//   nhau CANVAS_LAYOUT.rowGap; index toàn cục 0..n-1; positions theo row/col → con trỏ vẫn trỏ đúng ô.

import type { Element, Structure } from '@/engines/core/types';
import { CANVAS_COLORS, CANVAS_LAYOUT, hexToRgba } from './canvasTheme';
import type { Renderer, RenderOptions } from './interface';
import { CanvasPainter } from './painter/canvasPainter';

/** Alpha làm mờ vùng bỏ qua (status 'muted') — SDD §8.3. */
export const MUTED_CELL_ALPHA = 0.45;

const CELL_SIZE = 60;       // SDD §8.3: ô vuông 60×60
const MIN_CELL_SIZE = 28;   // mảng dài (>100 phần tử) → thu nhỏ ô
const POINTER_ARROW_LEN = 26;

// Bar mode (V3 renderArrayBar): chiều cao bar tối thiểu/placeholder + vùng index.
const BAR_MIN_H = 26;
const BAR_PLACEHOLDER_H = 14;
const BAR_INDEX_SPACE = 22;

function isNumericLabel(label: string): boolean {
  if (label === '—' || label === '−' || label === '') return true;
  return Number.isFinite(Number(label));
}

function parseNumericLabel(label: string): number | null {
  if (label === '—' || label === '−' || label === '') return null;
  const v = Number(label);
  return Number.isFinite(v) ? v : null;
}

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

    // Sắp xếp theo số thứ tự thật trong id (cell:0, cell:1...) để vẽ đúng thứ tự mảng.
    const cells = elements
      .slice()
      .sort((a, b) => (CanvasPainter.indexFromId(a.id) ?? 0) - (CanvasPainter.indexFromId(b.id) ?? 0));

    // Mọi label là số (hoặc placeholder —/—) → bar mode; label chữ hoặc có con trỏ/displayMode squares → ô vuông (fallback).
    const isSquareMode =
      structure.meta?.displayMode === 'squares' ||
      structure.meta?.displayMode === 'cells' ||
      elements.some((el) => el.group === 'pointer' || el.meta?.pointer === true);

    if (!isSquareMode && cells.every((el) => el.group === 'pointer' || isNumericLabel(el.label))) {
      this.renderBars(cells, elements, w, h, options);
    } else {
      this.renderSquares(cells, elements, w, h, options);
    }
  }

  /**
   * Ô vuông (fallback khi label không phải số, hoặc khi giải thuật tìm kiếm / có con trỏ).
   * Mảng dài (ô hẹp < 36px) → wrap nhiều hàng, hàng cách nhau CANVAS_LAYOUT.rowGap.
   */
  private renderSquares(cells: Element[], elements: Element[], w: number, h: number, options: RenderOptions): void {
    const dataCells = cells.filter((c) => c.group !== 'pointer');
    const n = dataCells.length;
    if (n === 0) return;
    const rawCell = (w - 2 * CANVAS_LAYOUT.margin) / Math.max(1, n);

    // Vị trí tâm + đỉnh của từng ô (dùng cho con trỏ).
    const positions = new Map<string, { cx: number; top: number }>();

    if (rawCell < 36) {
      // Wrap: maxPerRow = max(10, floor((w-2*margin)/36)); cellSize tính lại theo hàng đông nhất.
      const maxPerRow = Math.max(10, Math.floor((w - 2 * CANVAS_LAYOUT.margin) / 36));
      const rows = Math.ceil(n / maxPerRow);
      const cellSize = Math.max(
        MIN_CELL_SIZE,
        Math.min(CELL_SIZE, (w - 2 * CANVAS_LAYOUT.margin) / Math.min(n, maxPerRow)),
      );
      const rowHeight = cellSize + CANVAS_LAYOUT.rowGap + 36;
      const y0 = Math.max(48, (h - (rows * cellSize + (rows - 1) * (CANVAS_LAYOUT.rowGap + 36))) / 2);

      dataCells.forEach((el, i) => {
        const row = Math.floor(i / maxPerRow);
        const col = i % maxPerRow;
        const x = CANVAS_LAYOUT.margin + col * cellSize;
        const cx = x + cellSize / 2;
        const y = y0 + row * rowHeight;
        positions.set(el.id, { cx, top: y });
        this.drawCell(el, x, y, cellSize, options);
      });
    } else {
      // Mảng ngắn: 1 hàng (căn giữa đẹp mắt).
      const cellSize = Math.max(MIN_CELL_SIZE, Math.min(CELL_SIZE, rawCell));
      const totalWidth = n * cellSize;
      const x0 = Math.max(CANVAS_LAYOUT.margin, (w - totalWidth) / 2);
      const y0 = Math.max(48, (h - cellSize) / 2);

      dataCells.forEach((el, i) => {
        const x = x0 + i * cellSize;
        const cx = x + cellSize / 2;
        positions.set(el.id, { cx, top: y0 });
        this.drawCell(el, x, y0, cellSize, options);
      });
    }

    this.drawPointers(elements, positions, options);
  }

  /**
   * Bar mode (13/08 — canvas không còn "loãng", nguồn V3 renderArrayBar):
   * bar cao tỉ lệ giá trị, đáy chung, giá trị trên bar, index dưới đáy.
   * 14/08 (Task 7) — wrap nhiều hàng khi mảng dài (slotW < 44): maxPerRow = max(10, floor((w-2*margin)/44)),
   * usable chia đều theo số hàng, index toàn cục 0..n-1, positions theo row/col → con trỏ vẫn đúng ô.
   * Mảng ngắn (slotW ≥ 44) giữ nguyên công thức 1 hàng cũ.
   */
  private renderBars(cells: Element[], elements: Element[], w: number, h: number, options: RenderOptions): void {
    const values = new Map<string, number>();
    let maxVal = 0;
    let minVal = 0;
    for (const el of cells) {
      if (el.group === 'pointer') continue;
      const v = parseNumericLabel(el.label);
      if (v !== null) {
        values.set(el.id, v);
        if (v > maxVal) maxVal = v;
        if (v < minVal) minVal = v;
      }
    }
    const valRange = Math.max(1, maxVal - minVal);

    const n = cells.length;
    const top = CANVAS_LAYOUT.paddingTop;
    const baseY = h - 8 - (options.showIndex ? BAR_INDEX_SPACE : 8);
    const slotW = (w - 2 * CANVAS_LAYOUT.margin) / Math.max(1, n);

    const positions = new Map<string, { cx: number; top: number }>();

    if (slotW < 44) {
      // Wrap: mảng dài → nhiều hàng; tỉ lệ bar vẫn theo maxVal TOÀN mảng (không tính lại theo hàng).
      const maxPerRow = Math.max(10, Math.floor((w - 2 * CANVAS_LAYOUT.margin) / 44));
      const rows = Math.ceil(n / maxPerRow);
      const usablePerRow = Math.max(30, (baseY - top - rows * CANVAS_LAYOUT.rowGap) / rows);
      const rowBaseY: number[] = [];
      for (let r = 0; r < rows; r++) {
        rowBaseY.push(top + usablePerRow * (r + 1) + CANVAS_LAYOUT.rowGap * r);
      }

      cells.forEach((el, i) => {
        const row = Math.floor(i / maxPerRow);
        const col = i % maxPerRow;
        const rowSlotW = (w - 2 * CANVAS_LAYOUT.margin) / Math.min(n - row * maxPerRow, maxPerRow);
        const barW = Math.max(14, Math.min(88, rowSlotW - 6));
        const cx = CANVAS_LAYOUT.margin + col * rowSlotW + rowSlotW / 2;
        const x = cx - barW / 2;
        const v = values.get(el.id);
        const barH = v === undefined
          ? BAR_PLACEHOLDER_H
          : Math.max(BAR_MIN_H, Math.min(usablePerRow, ((v - minVal) / valRange) * (usablePerRow - BAR_MIN_H) + BAR_MIN_H));
        const y = rowBaseY[row] - barH;
        positions.set(el.id, { cx, top: y });
        // Element nhóm 'pointer' không phải ô dữ liệu → vẽ ở bước con trỏ.
        if (el.group !== 'pointer') this.drawBar(el, x, y, barW, barH, options);
      });
    } else {
      // Mảng ngắn: 1 hàng.
      const barW = Math.max(14, Math.min(88, slotW - 6));
      const usable = Math.max(30, baseY - top - 8);

      cells.forEach((el, i) => {
        const cx = CANVAS_LAYOUT.margin + i * slotW + slotW / 2;
        const x = cx - barW / 2;
        const v = values.get(el.id);
        const barH = v === undefined
          ? BAR_PLACEHOLDER_H
          : Math.max(BAR_MIN_H, Math.min(usable, ((v - minVal) / valRange) * (usable - BAR_MIN_H) + BAR_MIN_H));
        const y = baseY - barH;
        positions.set(el.id, { cx, top: y });
        // Element nhóm 'pointer' không phải ô dữ liệu → vẽ ở bước con trỏ.
        if (el.group !== 'pointer') this.drawBar(el, x, y, barW, barH, options);
      });
    }

    this.drawPointers(elements, positions, options);
  }

  private drawBar(el: Element, x: number, y: number, w: number, barH: number, options: RenderOptions): void {
    const muted = el.status === 'muted';
    const color = muted
      ? this.painter.statusColorWithAlpha(el.status, MUTED_CELL_ALPHA)
      : this.painter.statusColor(el.status);
    const glow = !muted && (el.status === 'active' || el.status === 'highlight' || el.status === 'swap');
    this.painter.gradientBar(x, y, w, barH, CANVAS_LAYOUT.borderRadius, color, glow);

    // Giá trị: trong bar khi đủ cao, ngược lại đặt phía trên bar.
    if (options.showValues && el.label !== '—' && el.label !== '−') {
      const style = {
        size: 13,
        weight: 'bold' as const,
        color: muted ? CANVAS_COLORS.muted : CANVAS_COLORS.text,
      };
      if (barH >= 30) {
        this.painter.text(el.label, x + w / 2, y + 14, style);
      } else {
        this.painter.text(el.label, x + w / 2, y - 10, style);
      }
    }
    // Index dưới đáy bar.
    if (options.showIndex) {
      const idx = CanvasPainter.indexFromId(el.id);
      if (idx !== null) {
        this.painter.label(String(idx), x + w / 2, y + barH + 16);
      }
    }
  }

  private drawPointers(
    elements: Element[],
    positions: Map<string, { cx: number; top: number }>,
    options: RenderOptions,
  ): void {
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
    const labelText = el.label;
    if (labelText) {
      this.painter.label(labelText, pos.cx, startY - 8, CANVAS_COLORS.compare, 12);
    }
  }
}
