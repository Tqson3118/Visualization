// composables/useStructureTransition.ts — hiệu ứng "thở" khi push/pop stack/queue (Task 3)
//
// - Dùng CoreAnimationEngine (SDD §4.4): đăng ký callback vẽ trong lúc transition; mỗi frame
//   tạo 1 Structure COPY với meta anim (animX/animY/animAlpha) để StackQueueRenderer vẽ
//   element trượt/bay/fade. KHÔNG bao giờ mutate structure/element gốc.
// - Layout tĩnh (cellSize/cx/cy) copy NGUYÊN CÔNG THỨC từ stackQueueRenderer.ts — giữ đồng bộ
//   khi sửa renderer (mọi hằng số chú thích "khớp stackQueueRenderer").
// - Prefers-reduced-motion / kind khác / không delta → vẽ thẳng ngay, không animate.

import type { Element, Structure } from '@/engines/core/types';
import { CANVAS_LAYOUT } from '@/engines/renderers/canvasTheme';
import { CoreAnimationEngine } from '@/engines/renderers/coreAnimationEngine';

const DEFAULT_DURATION_MS = 200;
const CELL_SIZE = 56; // khớp stackQueueRenderer (CELL_SIZE)
const MIN_CELL_SIZE = 28; // khớp stackQueueRenderer (MIN_CELL_SIZE)
const FLY_EXTRA = 12; // quãng nhô khi bay: stack -(cellSize/2 + 12); queue -(cellSize + 12)
const FIRST_FRAME_MS = 1000 / 60; // ≈ 1 khung rAF 60fps — frame đầu vẽ ở thời điểm này

export interface StructureTransitionOptions {
  /** Injectable để test; mặc định tạo engine mới. */
  engine?: CoreAnimationEngine;
  /** Thời lượng transition (ms) — mặc định 200. */
  durationMs?: number;
  /**
   * Kích thước vùng vẽ LOGIC (CSS px, đã chia zoom) — khớp layout renderer
   * (renderer vẽ theo logicalWidth/Height). Mặc định 800×600.
   */
  viewport?: { width: number; height: number } | (() => { width: number; height: number });
}

export interface StructureTransition {
  update(prev: Structure | null, next: Structure, renderFrame: (s: Structure) => void): void;
  cancel(): void;
  isAnimating(): boolean;
}

interface Layout {
  cellSize: number;
  x(i: number): number; // tâm ô theo index đã SORT (tăng dần indexFromId)
  y(i: number): number;
}

interface Offset {
  dx: number;
  dy: number;
}

/** Layout tĩnh stack/queue — ĐỒNG BỘ stackQueueRenderer.renderStack/renderQueue (SDD §8.3). */
function layoutFor(structure: Structure, w: number, h: number): Layout {
  const n = Math.max(1, structure.elements.length);
  if (structure.kind === 'queue') {
    const cellSize = Math.max(MIN_CELL_SIZE, Math.min(CELL_SIZE, (w - 2 * CANVAS_LAYOUT.margin) / n));
    const cy = h / 2;
    return {
      cellSize,
      x: (i: number) => CANVAS_LAYOUT.margin + i * cellSize + cellSize / 2,
      y: () => cy,
    };
  }
  // stack: dãy dọc, đáy cố định, đỉnh trên cùng (cell cuối = đỉnh)
  const cellSize = Math.max(MIN_CELL_SIZE, Math.min(CELL_SIZE, (h - 2 * CANVAS_LAYOUT.margin) / n));
  const cx = w / 2;
  const bottom = h - CANVAS_LAYOUT.margin;
  return {
    cellSize,
    x: () => cx,
    y: (i: number) => bottom - (i + 1) * cellSize + cellSize / 2,
  };
}

/** Số thứ tự từ id 'cell:3' — khớp CanvasPainter.indexFromId (fallback +∞ khi không khớp). */
function indexOf(id: string): number {
  const m = /(?:^|:)(\d+)$/.exec(id);
  return m ? Number(m[1]) : Number.MAX_SAFE_INTEGER;
}

/** Danh sách id theo thứ tự layout (sort indexFromId tăng dần — như renderer). */
function sortedIds(elements: Element[]): string[] {
  return elements
    .slice()
    .sort((a, b) => indexOf(a.id) - indexOf(b.id))
    .map((el) => el.id);
}

function easeOutCubic(t: number): number {
  return 1 - (1 - t) ** 3;
}

function prefersReducedMotion(): boolean {
  if (typeof window === 'undefined' || typeof window.matchMedia !== 'function') return false;
  return window.matchMedia('(prefers-reduced-motion: reduce)').matches;
}

export function useStructureTransition(options: StructureTransitionOptions = {}): StructureTransition {
  const engine = options.engine ?? new CoreAnimationEngine();
  const durationMs = options.durationMs ?? DEFAULT_DURATION_MS;
  let callback: ((dt: number) => void) | null = null;
  let animating = false;

  function cancel(): void {
    if (callback) engine.unregisterRender(callback);
    callback = null;
    animating = false;
  }

  function isAnimating(): boolean {
    return animating;
  }

  function update(prev: Structure | null, next: Structure, renderFrame: (s: Structure) => void): void {
    cancel(); // update liên tiếp (push nhanh) → hủy animation cũ trước khi bắt đầu mới

    // Các trường hợp vẽ thẳng ngay (không animate).
    if (!prev) {
      renderFrame(next);
      return;
    }
    if (next.kind !== 'stack' && next.kind !== 'queue') {
      renderFrame(next);
      return;
    }
    if (prev.kind !== next.kind) {
      renderFrame(next);
      return;
    }
    if (prefersReducedMotion()) {
      renderFrame(next);
      return;
    }
    if (durationMs <= 0) {
      renderFrame(next);
      return;
    }

    // Delta detect theo sorted ids (indexFromId).
    const prevIds = sortedIds(prev.elements);
    const nextIds = sortedIds(next.elements);
    const prevSet = new Set(prevIds);
    const nextSet = new Set(nextIds);
    const added = nextIds.filter((id) => !prevSet.has(id));
    const removed = prevIds.filter((id) => !nextSet.has(id));

    if (added.length === 0 && removed.length === 0) {
      renderFrame(next); // cùng cấu trúc — không cần animate
      return;
    }

    const vp = typeof options.viewport === 'function' ? options.viewport() : options.viewport;
    const w = vp?.width ?? 800;
    const h = vp?.height ?? 600;

    const lPrev = layoutFor(prev, w, h);
    const lNext = layoutFor(next, w, h);
    const prevIndex = new Map(prevIds.map((id, i) => [id, i]));
    const nextIndex = new Map(nextIds.map((id, i) => [id, i]));

    // common: lerp từ vị trí cũ (prev) → vị trí mới (next); added: trượt từ ngoài khung vào
    // vị trí tĩnh next. meta { animX: dx*(1-t'), animY: dy*(1-t') } → tại t'=0 element ở vị trí
    // cũ/ngoài khung, tại t'=1 về đúng vị trí tĩnh next.
    const commonOffsets = new Map<string, Offset>();
    const addedOffsets = new Map<string, Offset>();
    for (const id of nextIds) {
      const pi = prevIndex.get(id);
      if (pi !== undefined) {
        const ni = nextIndex.get(id) ?? 0;
        const dx = lNext.x(ni) - lPrev.x(pi);
        const dy = lNext.y(ni) - lPrev.y(pi);
        if (dx !== 0 || dy !== 0) commonOffsets.set(id, { dx, dy });
      } else if (next.kind === 'queue') {
        // push queue: trượt từ phải vào (rear)
        addedOffsets.set(id, { dx: lNext.cellSize + FLY_EXTRA, dy: 0 });
      } else {
        // push stack: trượt từ trên xuống (đỉnh)
        addedOffsets.set(id, { dx: 0, dy: -(lNext.cellSize + FLY_EXTRA) });
      }
    }

    // removed (pop/dequeue): KHÔNG có trong next → nối vào copy (renderer vẽ nó ở slot của copy)
    // với offset = vị trí tĩnh prev + quãng bay, kèm fade alpha 1 → 0 (animAlpha = 1 - t').
    const prevById = new Map(prev.elements.map((el) => [el.id, el]));
    const removedEls = removed
      .map((id) => prevById.get(id))
      .filter((el): el is Element => el !== undefined);
    const removedAnim: Array<{ el: Element; dx: number; dy: number }> = [];
    if (removedEls.length > 0) {
      const copyList = [...next.elements, ...removedEls]
        .slice()
        .sort((a, b) => indexOf(a.id) - indexOf(b.id));
      const copyLayout = layoutFor({ ...next, elements: copyList }, w, h);
      const copyIndex = new Map(copyList.map((el, i) => [el.id, i]));
      for (const el of removedEls) {
        const pi = prevIndex.get(el.id) ?? 0;
        const ci = copyIndex.get(el.id) ?? 0;
        const fly =
          next.kind === 'queue'
            ? { dx: -(copyLayout.cellSize + FLY_EXTRA), dy: 0 } // dequeue: bay ra trái
            : { dx: 0, dy: -(copyLayout.cellSize / 2 + FLY_EXTRA) }; // pop: bay lên
        removedAnim.push({
          el,
          dx: lPrev.x(pi) + fly.dx - copyLayout.x(ci),
          dy: lPrev.y(pi) + fly.dy - copyLayout.y(ci),
        });
      }
    }

    // Animation state: t 0 → 1 theo deltaTime từ engine callback; easeOutCubic.
    let elapsedMs = FIRST_FRAME_MS; // bắt đầu sau ~1 khung rAF (frame đầu đã fade, alpha < 1)
    let framesDrawn = 0;

    const buildFrame = (ease: number): Structure => {
      const inv = 1 - ease;
      const elements: Element[] = next.elements.map((el) => {
        const anim: { animX?: number; animY?: number } = {};
        const c = commonOffsets.get(el.id) ?? addedOffsets.get(el.id);
        if (c) {
          if (c.dx !== 0) anim.animX = c.dx * inv;
          if (c.dy !== 0) anim.animY = c.dy * inv;
        }
        return anim.animX !== undefined || anim.animY !== undefined
          ? { ...el, meta: { ...el.meta, ...anim } } // copy nông + meta mới — không đụng gốc
          : el;
      });
      for (const r of removedAnim) {
        elements.push({
          ...r.el,
          meta: { ...r.el.meta, animX: r.dx * inv, animY: r.dy * inv, animAlpha: inv },
        });
      }
      return { kind: next.kind, elements, links: next.links };
    };

    const drawFrame = (ease: number): void => {
      framesDrawn++;
      renderFrame(buildFrame(ease));
    };

    const frameCb = (dt: number): void => {
      if (callback !== frameCb) return; // đã cancel/đè — bỏ qua frame cũ
      elapsedMs += dt;
      if (elapsedMs >= durationMs) {
        // Kết thúc: hủy đăng ký trước (dù renderFrame lỗi cũng không loop) rồi vẽ
        // frame cuối = structure THẬT — y hệt renderer tĩnh, không meta anim.
        cancel();
        renderFrame(next);
        return;
      }
      drawFrame(easeOutCubic(elapsedMs / durationMs));
    };

    animating = true;
    callback = frameCb;
    engine.registerRender(frameCb);
    // Engine thật gọi callback đồng bộ (dt=0) trong registerRender → frame đầu đã vẽ.
    // Engine giả (test) không gọi → tự vẽ frame đầu ở thời điểm ≈ khung rAF đầu tiên.
    if (framesDrawn === 0) drawFrame(easeOutCubic(Math.min(1, elapsedMs / durationMs)));
  }

  return { update, cancel, isAnimating };
}
