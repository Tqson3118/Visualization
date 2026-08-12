// engines/renderers/coreAnimationEngine.ts — vòng lặp hoạt ảnh (rAF) cho CanvasPainter (SDD §4.4)
//
// NGUỒN BÊ: source/VisualizationDSA3/frontend/src/core/CoreAnimationEngine.ts (V3 — bê NGUYÊN VẸN).
// Chọn V3 thay vì V1: V3 clamp deltaTime ≤ 32ms (tránh nhảy khung khi tab bị trễ)
// và cô lập lỗi từng render callback (V1 không có). API giữ nguyên.
// Quy ước: tối đa 1 render/frame; CanvasPainter đăng ký callback qua registerRender.

export interface Point2D {
  x: number;
  y: number;
}

export class CoreAnimationEngine {
  private animationFrameId: number | null = null;
  private isRunning = false;
  private renderCallbacks: Array<(deltaTime: number) => void> = [];
  private lastTimestamp = 0;

  /** Đăng ký callback vẽ — tự khởi động loop nếu chưa chạy. */
  public registerRender(callback: (deltaTime: number) => void): void {
    this.renderCallbacks.push(callback);
    if (!this.isRunning) {
      this.startLoop();
    }
  }

  /** Hủy callback — dừng loop khi không còn callback nào (tiết kiệm CPU). */
  public unregisterRender(callback: (deltaTime: number) => void): void {
    this.renderCallbacks = this.renderCallbacks.filter((cb) => cb !== callback);
    if (this.renderCallbacks.length === 0 && this.isRunning) {
      this.stopLoop();
    }
  }

  private startLoop(): void {
    this.isRunning = true;
    this.lastTimestamp = performance.now();
    this.loop();
  }

  private stopLoop(): void {
    this.isRunning = false;
    if (this.animationFrameId !== null) {
      cancelAnimationFrame(this.animationFrameId);
      this.animationFrameId = null;
    }
  }

  private loop = (timestamp: number = performance.now()): void => {
    if (!this.isRunning) return;

    const deltaTime = timestamp - this.lastTimestamp;
    this.lastTimestamp = timestamp;

    // Clamp delta: khi tab bị treo lâu (đổi tab, debug), không nhảy hoạt ảnh vụt.
    const clampedDelta = Math.min(deltaTime, 32);

    this.renderCallbacks.forEach((cb) => {
      try {
        cb(clampedDelta);
      } catch (err) {
        console.error('Error in render callback:', err);
      }
    });

    this.animationFrameId = requestAnimationFrame(this.loop);
  };

  /** Nội suy tuyến tính có clamp [0,1] — dùng cho mọi chuyển động Lerp. */
  public static lerp(start: number, end: number, t: number): number {
    // Clamp t về [0,1] để lerp không tràn khỏi đoạn.
    const clampedT = Math.max(0, Math.min(1, t));
    return start + (end - start) * clampedT;
  }

  public static lerpPoint(start: Point2D, end: Point2D, t: number): Point2D {
    return {
      x: this.lerp(start.x, end.x, t),
      y: this.lerp(start.y, end.y, t)
    };
  }

  /** Dừng loop và xóa toàn bộ callback (gọi khi component unmount). */
  public destroy(): void {
    this.stopLoop();
    this.renderCallbacks = [];
  }
}
