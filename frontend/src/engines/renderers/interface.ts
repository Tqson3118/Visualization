// engines/renderers/interface.ts — Hợp đồng Renderer theo SDD §4.4 (sao chép nguyên văn từ SDD)
// Renderer KHÔNG chứa logic giải thuật; chỉ đọc `Structure` và vẽ lên canvas.
// Renderer tạo class vẽ riêng CanvasPainter (xem coreAnimationEngine.ts — vòng lặp rAF).

import type { Structure } from '@/engines/core/types';

export interface RenderOptions {
  showIndex: boolean;
  showValues: boolean;
  zoom: number;                      // 0.5 - 2
  showLegend: boolean;
}

export interface Renderer {
  supportedKinds: string[];          // VD: ['array']
  mount(canvas: HTMLCanvasElement): void;
  render(structure: Structure, options: RenderOptions): void;
  resize(width: number, height: number): void;
  dispose(): void;
}
