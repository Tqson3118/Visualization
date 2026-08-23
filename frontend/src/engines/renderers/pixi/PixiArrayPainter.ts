// src/engines/renderers/pixi/PixiArrayPainter.ts
// Specialized WebGL Painter for Array algorithms (QuickSort, BubbleSort, BinarySearch, etc.)
// Features:
// - Parabolic Arc Swap trajectory with squash/stretch anticipation
// - Rotating Pivot Aura halo
// - Compare collision sparks
// - Sorted Bloom Wave on completion
// - Adaptive Bar & Square rendering modes with multi-row wrapping

import { Container, Graphics, Text, TextStyle } from 'pixi.js';
import type { Element, ElementStatus, Structure } from '@/engines/core/types';
import type { ParticleManager } from './ParticleManager';

export interface ArrayPainterOptions {
  showIndex: boolean;
  showValues: boolean;
  zoom: number;
  showLegend?: boolean;
}

export const PIXI_STATUS_COLORS: Record<ElementStatus, number> = {
  default: 0x38bdf8,
  active: 0x8b5cf6,
  highlight: 0xf59e0b,
  swap: 0xf43f5e,
  done: 0x10b981,
  error: 0xef4444,
  muted: 0x334155,
};

interface ElementAnimState {
  x: number;
  y: number;
  targetX: number;
  targetY: number;
  scaleX: number;
  scaleY: number;
  arcProgress: number; // 0 -> 1 for swap
  arcHeight: number;
  status: ElementStatus;
}

export class PixiArrayPainter {
  public container: Container;
  private graphics: Graphics;
  private auraGraphics: Graphics;
  private textContainer: Container;
  private textPool: Text[] = [];
  private textPoolIndex = 0;

  private animStates = new Map<string, ElementAnimState>();
  private pivotRotation = 0;
  private bloomWaveTime = 0;
  private isDoneState = false;

  private textStyle: TextStyle;
  private indexStyle: TextStyle;
  private isDestroyed = false;

  constructor() {
    this.container = new Container();
    this.auraGraphics = new Graphics();
    this.graphics = new Graphics();
    this.textContainer = new Container();

    this.container.addChild(this.auraGraphics);
    this.container.addChild(this.graphics);
    this.container.addChild(this.textContainer);

    this.textStyle = new TextStyle({
      fontFamily: "'JetBrains Mono', 'Fira Code', ui-monospace, monospace",
      fontSize: 13,
      fontWeight: 'bold',
      fill: 0x090d16,
      align: 'center',
    });

    this.indexStyle = new TextStyle({
      fontFamily: "'JetBrains Mono', 'Fira Code', ui-monospace, monospace",
      fontSize: 11,
      fontWeight: '500',
      fill: 0x94a3b8,
      align: 'center',
    });
  }

  private acquireText(content: string, style: TextStyle, x: number, y: number, alpha = 1): Text {
    let t: Text;
    if (this.textPoolIndex < this.textPool.length) {
      t = this.textPool[this.textPoolIndex];
      t.text = content;
      t.style = style;
      t.visible = true;
    } else {
      t = new Text({ text: content, style });
      this.textPool.push(t);
      this.textContainer.addChild(t);
    }
    t.anchor.set(0.5, 0.5);
    t.position.set(x, y);
    t.alpha = alpha;
    this.textPoolIndex++;
    return t;
  }

  private resetTextPool(): void {
    for (let i = this.textPoolIndex; i < this.textPool.length; i++) {
      this.textPool[i].visible = false;
    }
    this.textPoolIndex = 0;
  }

  public update(deltaMs: number, particles?: ParticleManager): void {
    const timeScale = Math.min(2, Math.max(0.5, deltaMs / 16.67));
    this.pivotRotation += 0.05 * timeScale;
    this.bloomWaveTime += 0.03 * timeScale;

    // Smooth lerp positions and arc swaps
    this.animStates.forEach((state) => {
      // Linear lerp position
      state.x += (state.targetX - state.x) * 0.25 * timeScale;
      state.y += (state.targetY - state.y) * 0.25 * timeScale;

      // Arc progress
      if (state.arcProgress < 1) {
        state.arcProgress = Math.min(1, state.arcProgress + 0.08 * timeScale);
        // Squash and stretch anticipation
        if (state.arcProgress < 0.3) {
          state.scaleX = 1.15;
          state.scaleY = 0.85;
        } else if (state.arcProgress < 0.7) {
          state.scaleX = 0.9;
          state.scaleY = 1.15;
          if (particles && Math.random() < 0.4) {
            particles.spawnTrail(state.x, state.y, PIXI_STATUS_COLORS[state.status]);
          }
        } else {
          state.scaleX += (1 - state.scaleX) * 0.2;
          state.scaleY += (1 - state.scaleY) * 0.2;
        }
      } else {
        state.scaleX += (1 - state.scaleX) * 0.2;
        state.scaleY += (1 - state.scaleY) * 0.2;
      }
    });
  }

  public render(
    structure: Structure,
    opts: ArrayPainterOptions,
    width: number,
    height: number,
    particles?: ParticleManager,
  ): void {
    if (this.isDestroyed || !this.graphics || (this.graphics as any).destroyed) return;
    this.graphics.clear();
    this.auraGraphics.clear();
    this.resetTextPool();

    const elements = structure.elements;
    if (elements.length === 0) return;

    const n = elements.length;
    const isAllDone = elements.every((el) => el.status === 'done');
    if (isAllDone && !this.isDoneState) {
      this.isDoneState = true;
      if (particles) {
        particles.spawnConfetti(width, height, 40);
      }
    } else if (!isAllDone) {
      this.isDoneState = false;
    }

    const targetRatio = 0.85;
    const availableW = Math.max(160, width * targetRatio);
    const availableH = Math.max(140, height - (opts.showIndex ? 60 : 36));
    const isBarMode = elements.every((el) => !isNaN(Number(el.label)));

    // Find max value for bar scaling with generous vertical dynamic range (75-80% fill ratio, min 50px)
    const maxVal = Math.max(1, ...elements.map((el) => Number(el.label) || 1));
    const maxCellW = n <= 6 ? 96 : n <= 10 ? 72 : n <= 16 ? 48 : 36;
    const idealGap = Math.min(16, Math.max(4, availableW / (n * 7)));
    const cellW = Math.min(maxCellW, Math.max(18, (availableW - (n - 1) * idealGap) / n));
    const gap = Math.min(16, Math.max(4, (availableW - n * cellW) / Math.max(1, n - 1)));
    const totalW = n * cellW + (n - 1) * gap;
    const startX = (width - totalW) / 2;

    const minBarH = Math.max(50, availableH * 0.25);
    const maxBarH = Math.max(minBarH + 20, availableH * 0.78);
    const centerY = height / 2;
    // Anchor baseline so the entire visualizer is vertically centered in the viewport
    const baselineY = isBarMode
      ? centerY + maxBarH / 2 - (opts.showIndex ? 6 : 0)
      : centerY + 28;

    let comparedPair: { x: number; y: number }[] = [];

    elements.forEach((el, i) => {
      const targetX = startX + i * (cellW + gap) + cellW / 2;
      const val = Number(el.label) || 0;
      const barH = isBarMode
        ? minBarH + ((val / maxVal) * (maxBarH - minBarH))
        : Math.min(64, Math.max(44, cellW));
      const targetY = isBarMode ? baselineY - barH / 2 : centerY - (opts.showIndex ? 10 : 0);

      let state = this.animStates.get(el.id);
      if (!state) {
        state = {
          x: targetX,
          y: targetY,
          targetX,
          targetY,
          scaleX: 1,
          scaleY: 1,
          arcProgress: 1,
          arcHeight: 40,
          status: el.status,
        };
        this.animStates.set(el.id, state);
      } else {
        if (Math.abs(state.targetX - targetX) > 2) {
          // If this is a resize or first layout rather than a step swap, immediately align x
          if (state.status !== 'swap' && el.status !== 'swap') {
            state.x = targetX;
            state.y = targetY;
          }
          state.targetX = targetX;
          state.targetY = targetY;
          state.arcProgress = 0;
          state.arcHeight = Math.min(80, Math.abs(state.x - targetX) * 0.4);
        }
        state.status = el.status;
      }

      // Parabolic Arc Y calculation
      let currentY = state.y;
      if (state.arcProgress < 1) {
        const p = state.arcProgress;
        const arcOffset = 4 * state.arcHeight * p * (1 - p);
        currentY -= arcOffset;
      }

      const color = PIXI_STATUS_COLORS[el.status] ?? PIXI_STATUS_COLORS.default;
      const isPivot = el.status === 'highlight' || (el.group && el.group.includes('pivot'));

      // 1. Pivot Aura rotating effect
      if (isPivot) {
        const auraRadius = Math.max(cellW, barH) / 2 + 10;
        this.auraGraphics
          .circle(state.x, currentY, auraRadius)
          .stroke({ width: 2, color: 0xf59e0b, alpha: 0.6 })
          .circle(state.x, currentY, auraRadius + Math.sin(this.pivotRotation) * 4)
          .stroke({ width: 1.5, color: 0xfbbf24, alpha: 0.4 });
      }

      // 2. Track compared elements for sparks
      if (el.status === 'active' || el.status === 'swap') {
        comparedPair.push({ x: state.x, y: currentY });
      }

      // 3. Bloom Wave calculation for done state
      let bloomBoost = 0;
      if (el.status === 'done') {
        const wavePhase = (this.bloomWaveTime * 2 - i * 0.4) % (Math.PI * 2);
        bloomBoost = Math.max(0, Math.sin(wavePhase)) * 0.3;
      }

      // 4. Draw Bar or Square
      const renderW = (cellW - 2) * state.scaleX;
      const renderH = barH * state.scaleY;
      const rx = state.x - renderW / 2;
      const ry = currentY - renderH / 2;

      // Glow shadow / base
      if (el.status === 'active' || el.status === 'swap' || isPivot || bloomBoost > 0) {
        this.graphics
          .roundRect(rx - 3, ry - 3, renderW + 6, renderH + 6, 8)
          .fill({ color, alpha: 0.25 + bloomBoost });
      }

      // Main element body
      const fillAlpha = el.status === 'muted' ? 0.4 : 0.95;
      this.graphics
        .roundRect(rx, ry, renderW, renderH, Math.min(6, cellW / 4))
        .fill({ color, alpha: fillAlpha })
        .stroke({ width: 1.5, color: 0x090d16, alpha: 0.8 });

      // Highlight bevel shine
      if (el.status !== 'muted') {
        this.graphics
          .roundRect(rx + 2, ry + 2, Math.max(4, renderW - 4), Math.min(6, renderH / 3), 3)
          .fill({ color: 0xffffff, alpha: 0.35 });
      }

      // 5. Value Text
      if (opts.showValues) {
        const textColor = el.status === 'muted' ? 0x94a3b8 : 0x090d16;
        const style = new TextStyle({
          ...this.textStyle,
          fontSize: Math.min(14, Math.max(10, cellW / 2.2)),
          fill: textColor,
        });
        const textY = isBarMode ? currentY : currentY;
        this.acquireText(el.label, style, state.x, textY);
      }

      // 6. Index Label
      if (opts.showIndex) {
        const idx = el.id.replace(/^cell:/, '');
        if (/^\d+$/.test(idx)) {
          this.acquireText(idx, this.indexStyle, state.x, baselineY + 16);
        }
      }
    });

    // Spawn sparks at midpoint if 2 compared items are active
    if (comparedPair.length === 2 && particles && Math.random() < 0.35) {
      const midX = (comparedPair[0].x + comparedPair[1].x) / 2;
      const midY = (comparedPair[0].y + comparedPair[1].y) / 2;
      particles.spawnSparks(midX, midY, 0xf59e0b, 8);
    }
  }

  public destroy(): void {
    this.isDestroyed = true;
    try {
      this.textContainer.destroy({ children: true });
      this.container.destroy({ children: true });
    } catch {}
  }
}
