// engines/renderers/painter/canvasPainter.ts — CanvasPainter: helper vẽ cơ bản cho mọi renderer
// (SDD §4.4, §8.3)
//
// - Màu sắc KHÔNG hardcode: đọc từ CANVAS_COLORS (canvasTheme.ts — nguồn là CSS variables).
// - Tọa độ vẽ theo CSS-pixel; transform DPR × zoom do beginFrame() xử lý một lần.
// - Nguyên thủy vẽ: roundRect / circle / text / line / dashedLine / arrow / curve / label.

import type { ElementStatus } from '@/engines/core/types';
import { CANVAS_COLORS, hexToRgba } from '../canvasTheme';

export interface TextStyle {
  size?: number;
  color?: string;
  weight?: 'normal' | 'bold';
  align?: CanvasTextAlign;
  baseline?: CanvasTextBaseline;
}

export class CanvasPainter {
  private canvas: HTMLCanvasElement | null = null;
  private ctx: CanvasRenderingContext2D | null = null;
  private viewWidth = 0;    // kích thước vùng vẽ (CSS px)
  private viewHeight = 0;   // kích thước vùng vẽ (CSS px)
  private zoom = 1;

  mount(canvas: HTMLCanvasElement): void {
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d');
  }

  resize(width: number, height: number): void {
    this.viewWidth = Math.max(1, width);
    this.viewHeight = Math.max(1, height);
  }

  setZoom(zoom: number): void {
    // Clamp zoom về [0.5, 2] (khớp dải zoom của CanvasArea).
    this.zoom = Math.max(0.5, Math.min(2, zoom));
  }

  get width(): number {
    return this.viewWidth;
  }

  get height(): number {
    return this.viewHeight;
  }

  /** Kích thước vùng vẽ theo tọa độ logic (đã chia zoom). */
  get logicalWidth(): number {
    return this.viewWidth / this.zoom;
  }

  get logicalHeight(): number {
    return this.viewHeight / this.zoom;
  }

  /** Màu theo trạng thái phần tử (SDD §8.3) — map trực tiếp từ CANVAS_COLORS, không hardcode. */
  statusColor(status: ElementStatus): string {
    switch (status) {
      case 'active':
      case 'highlight':
        return CANVAS_COLORS.compare;
      case 'swap':
        return CANVAS_COLORS.swap;
      case 'done':
        return CANVAS_COLORS.sorted;
      case 'error':
        return CANVAS_COLORS.swap; // lỗi dùng tông đỏ
      case 'muted':
        return CANVAS_COLORS.muted;
      default:
        return CANVAS_COLORS.default;
    }
  }

  /** Màu theo trạng thái kèm alpha — dùng cho hiệu ứng mờ / làm nổi. */
  statusColorWithAlpha(status: ElementStatus, alpha: number): string {
    return hexToRgba(this.statusColor(status), alpha);
  }

  /** Màu cạnh/link: trạng thái default hoặc không có → đường mờ, không tô đậm. */
  edgeColor(status?: ElementStatus): string {
    if (!status || status === 'default') return hexToRgba(CANVAS_COLORS.text, 0.35);
    return this.statusColor(status);
  }

  /** Bắt đầu khung vẽ: reset transform DPR × zoom, xóa + tô nền (--canvas-bg). */
  beginFrame(): void {
    const ctx = this.ctx;
    if (!ctx) return;
    const dpr = typeof window !== 'undefined' && window.devicePixelRatio > 0 ? window.devicePixelRatio : 1;
    ctx.setTransform(dpr * this.zoom, 0, 0, dpr * this.zoom, 0, 0);
    ctx.clearRect(0, 0, this.logicalWidth, this.logicalHeight);
    ctx.fillStyle = CANVAS_COLORS.bgDark;
    ctx.fillRect(0, 0, this.logicalWidth, this.logicalHeight);
  }

  /** Hình chữ nhật tròn góc; fallback rect khi trình duyệt không có roundRect. */
  roundRect(
    x: number,
    y: number,
    w: number,
    h: number,
    radius: number,
    fill?: string,
    stroke?: string,
    lineWidth = 1.5,
  ): void {
    const ctx = this.ctx;
    if (!ctx) return;
    const r = Math.max(0, Math.min(radius, w / 2, h / 2));
    ctx.beginPath();
    if (typeof ctx.roundRect === 'function') {
      ctx.roundRect(x, y, w, h, r);
    } else {
      ctx.rect(x, y, w, h);
    }
    if (fill) {
      ctx.fillStyle = fill;
      ctx.fill();
    }
    if (stroke) {
      ctx.strokeStyle = stroke;
      ctx.lineWidth = lineWidth;
      ctx.stroke();
    }
  }

  /**
   * Bar dữ liệu kiểu cũ V3 (renderArrayBar — nguồn source/VisualizationDSA3):
   * gradient dọc (đậm → nhạt) + glow tùy chọn cho trạng thái nổi bật.
   * Fallback màu phẳng khi ctx không có createLinearGradient (môi trường test/jsdom).
   */
  gradientBar(
    x: number,
    y: number,
    w: number,
    h: number,
    radius: number,
    fill: string,
    glow = false,
  ): void {
    const ctx = this.ctx;
    if (!ctx || w <= 0 || h <= 0) return;
    const r = Math.max(0, Math.min(radius, w / 2, h / 2));
    ctx.beginPath();
    if (typeof ctx.roundRect === 'function') {
      ctx.roundRect(x, y, w, h, r);
    } else {
      ctx.rect(x, y, w, h);
    }
    if (glow) {
      ctx.shadowBlur = 16;
      ctx.shadowColor = fill;
    }
    if (typeof ctx.createLinearGradient === 'function') {
      const grad = ctx.createLinearGradient(x, y, x, y + Math.max(1, h));
      grad.addColorStop(0, hexToRgba(fill, 0.9));
      grad.addColorStop(1, hexToRgba(fill, 0.18));
      ctx.fillStyle = grad;
    } else {
      ctx.fillStyle = fill;
    }
    ctx.fill();
    ctx.shadowBlur = 0;
    ctx.shadowColor = 'transparent';
  }

  /** Hình tròn (đỉnh / nút cây). */
  circle(x: number, y: number, r: number, fill?: string, stroke?: string, lineWidth = 2): void {    const ctx = this.ctx;
    if (!ctx) return;
    ctx.beginPath();
    ctx.arc(x, y, r, 0, Math.PI * 2);
    if (fill) {
      ctx.fillStyle = fill;
      ctx.fill();
    }
    if (stroke) {
      ctx.strokeStyle = stroke;
      ctx.lineWidth = lineWidth;
      ctx.stroke();
    }
  }

  /** Vẽ text — căn giữa mặc định; màu mặc định theo theme (--color-text-primary). */
  text(text: string, x: number, y: number, style: TextStyle = {}): void {    const ctx = this.ctx;
    if (!ctx) return;
    ctx.font = `${style.weight ?? 'normal'} ${style.size ?? 13}px sans-serif`;
    ctx.fillStyle = style.color ?? CANVAS_COLORS.text;
    ctx.textAlign = style.align ?? 'center';
    ctx.textBaseline = style.baseline ?? 'middle';
    ctx.fillText(text, x, y);
  }

  /** Nhãn phụ — text nhỏ, màu mờ (--color-text-muted). */
  label(text: string, x: number, y: number, color: string = CANVAS_COLORS.muted, size = 11): void {
    this.text(text, x, y, { size, color });
  }

  /** Đoạn thẳng. */
  line(x1: number, y1: number, x2: number, y2: number, color: string, width = 2): void {
    const ctx = this.ctx;
    if (!ctx) return;
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.strokeStyle = color;
    ctx.lineWidth = width;
    ctx.stroke();
  }

  /** Đoạn thẳng đứt nét — dùng cho nút nổi / ô null. */
  dashedLine(x1: number, y1: number, x2: number, y2: number, color: string, width = 2, dash: number[] = [6, 4]): void {
    const ctx = this.ctx;
    if (!ctx) return;
    ctx.setLineDash(dash);
    this.line(x1, y1, x2, y2, color, width);
    ctx.setLineDash([]);
  }

  /** Mũi tên: đoạn thẳng + đầu mũi tên tam giác tại (x2, y2). */
  arrow(x1: number, y1: number, x2: number, y2: number, color: string, width = 2, headSize = 9): void {
    const ctx = this.ctx;
    if (!ctx) return;
    const angle = Math.atan2(y2 - y1, x2 - x1);
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.lineTo(x2, y2);
    ctx.strokeStyle = color;
    ctx.lineWidth = width;
    ctx.stroke();
    // Đầu mũi tên: tam giác quay theo hướng đoạn thẳng.
    ctx.beginPath();
    ctx.moveTo(x2, y2);
    ctx.lineTo(x2 - headSize * Math.cos(angle - Math.PI / 6), y2 - headSize * Math.sin(angle - Math.PI / 6));
    ctx.lineTo(x2 - headSize * Math.cos(angle + Math.PI / 6), y2 - headSize * Math.sin(angle + Math.PI / 6));
    ctx.closePath();
    ctx.fillStyle = color;
    ctx.fill();
  }

  /** Cạnh cong (quadratic Bézier) — dùng cho cây: cha → con; head=true vẽ đầu mũi tên (bubble up/down). */
  curve(x1: number, y1: number, x2: number, y2: number, color: string, width = 2, bend = 14, head = false): void {
    const ctx = this.ctx;
    if (!ctx) return;
    const mx = (x1 + x2) / 2;
    const my = (y1 + y2) / 2 + bend;
    ctx.beginPath();
    ctx.moveTo(x1, y1);
    ctx.quadraticCurveTo(mx, my, x2, y2);
    ctx.strokeStyle = color;
    ctx.lineWidth = width;
    ctx.stroke();
    if (head) {
      // Mũi tên tại điểm cuối (phía con) — hướng theo đoạn cha→con.
      const angle = Math.atan2(y2 - y1, x2 - x1);
      const size = 7;
      ctx.beginPath();
      ctx.moveTo(x2, y2);
      ctx.lineTo(x2 - size * Math.cos(angle - Math.PI / 6), y2 - size * Math.sin(angle - Math.PI / 6));
      ctx.lineTo(x2 - size * Math.cos(angle + Math.PI / 6), y2 - size * Math.sin(angle + Math.PI / 6));
      ctx.closePath();
      ctx.fillStyle = color;
      ctx.fill();
    }
  }

  /** Lấy số thứ tự từ id dạng 'cell:3' / 'node:12' → 3/12; null nếu không khớp. */
  static indexFromId(id: string): number | null {
    const m = /(?:^|:)(\d+)$/.exec(id);
    return m ? Number(m[1]) : null;
  }
}
