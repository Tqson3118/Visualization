// src/engines/renderers/pixi/PixiLinearPainter.ts
// Specialized WebGL Painter for Linear Data Structures (Stack, Queue, Linked List, Deque)
// Features:
// - Spring push overshoot bounce animation
// - Pop dissolve dust particle burst
// - Idle breathing pulse on Top/Front elements
// - Singly & Doubly linked list arrows with null pointer indicators

import { Container, Graphics, Text, TextStyle } from 'pixi.js';
import type { Element, ElementStatus, Link, Structure } from '@/engines/core/types';
import { PIXI_STATUS_COLORS } from './PixiArrayPainter';
import type { ParticleManager } from './ParticleManager';

export interface LinearPainterOptions {
  showIndex: boolean;
  showValues: boolean;
  zoom: number;
}

interface LinearItemAnim {
  x: number;
  y: number;
  targetX: number;
  targetY: number;
  scaleX: number;
  scaleY: number;
  vy: number;
  status: ElementStatus;
}

export class PixiLinearPainter {
  public container: Container;
  private linkGraphics: Graphics;
  private itemGraphics: Graphics;
  private glowGraphics: Graphics;
  private textContainer: Container;
  private textPool: Text[] = [];
  private textPoolIndex = 0;

  private animStates = new Map<string, LinearItemAnim>();
  private prevElementIds = new Set<string>();
  private idleTime = 0;

  private itemTextStyle: TextStyle;
  private labelTextStyle: TextStyle;
  private isDestroyed = false;

  constructor() {
    this.container = new Container();
    this.glowGraphics = new Graphics();
    this.linkGraphics = new Graphics();
    this.itemGraphics = new Graphics();
    this.textContainer = new Container();

    this.container.addChild(this.glowGraphics);
    this.container.addChild(this.linkGraphics);
    this.container.addChild(this.itemGraphics);
    this.container.addChild(this.textContainer);

    this.itemTextStyle = new TextStyle({
      fontFamily: "'JetBrains Mono', 'Fira Code', ui-monospace, monospace",
      fontSize: 13,
      fontWeight: 'bold',
      fill: 0x090d16,
      align: 'center',
    });

    this.labelTextStyle = new TextStyle({
      fontFamily: "'JetBrains Mono', 'Fira Code', ui-monospace, monospace",
      fontSize: 11,
      fontWeight: '600',
      fill: 0x5eead4,
      align: 'center',
    });
  }

  private acquireText(content: string, style: TextStyle, x: number, y: number): Text {
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
    this.textPoolIndex++;
    return t;
  }

  private resetTextPool(): void {
    for (let i = this.textPoolIndex; i < this.textPool.length; i++) {
      this.textPool[i].visible = false;
    }
    this.textPoolIndex = 0;
  }

  public update(deltaMs: number): void {
    const timeScale = Math.min(2, Math.max(0.5, deltaMs / 16.67));
    this.idleTime += 0.05 * timeScale;

    this.animStates.forEach((state) => {
      // Spring physics toward target
      const dx = state.targetX - state.x;
      const dy = state.targetY - state.y;
      state.x += dx * 0.22 * timeScale;
      state.y += dy * 0.22 * timeScale;

      // Settle scale
      state.scaleX += (1 - state.scaleX) * 0.2 * timeScale;
      state.scaleY += (1 - state.scaleY) * 0.2 * timeScale;
    });
  }

  public render(
    structure: Structure,
    opts: LinearPainterOptions,
    width: number,
    height: number,
    particles?: ParticleManager,
  ): void {
    if (this.isDestroyed || !this.itemGraphics || (this.itemGraphics as any).destroyed) return;
    this.linkGraphics.clear();
    this.itemGraphics.clear();
    this.glowGraphics.clear();
    this.resetTextPool();

    const isStack = structure.kind === 'stack';
    const isQueue = structure.kind === 'queue';
    const isLinkedList = structure.kind === 'linkedlist';
    const elements = structure.elements;

    // Detect popped elements -> trigger dissolve dust
    const currentIds = new Set(elements.map((el) => el.id));
    this.prevElementIds.forEach((prevId) => {
      if (!currentIds.has(prevId) && particles) {
        const lastState = this.animStates.get(prevId);
        if (lastState) {
          particles.spawnDissolveDust(lastState.x, lastState.y, 60, 36, 0xf87171, 16);
          this.animStates.delete(prevId);
        }
      }
    });
    this.prevElementIds = currentIds;

    const n = elements.length;
    const cellW = isStack ? Math.min(240, width - 80) : Math.min(88, Math.max(56, (width * 0.85) / Math.max(1, n)));
    const cellH = isStack ? 44 : 52;
    const totalStackH = Math.max(1, n) * (cellH + 8);
    const stackCenterY = height / 2;
    const stackBaseY = Math.min(height - 24, stackCenterY + totalStackH / 2);

    elements.forEach((el, i) => {
      let targetX = 0;
      let targetY = 0;

      if (isStack) {
        // Stack: vertical centered bottom-up
        targetX = width / 2;
        targetY = stackBaseY - (i + 0.5) * (cellH + 8);
      } else {
        // Queue / Linked List: horizontal row centered
        const gap = isLinkedList ? 32 : 8;
        const totalW = n * cellW + (n - 1) * gap;
        const startX = Math.max(20, (width - totalW) / 2);
        targetX = startX + i * (cellW + gap) + cellW / 2;
        targetY = height / 2;
      }

      let state = this.animStates.get(el.id);
      if (!state) {
        // New item pushed -> Spring overshoot initial state
        const spawnY = isStack ? targetY - 60 : targetY - 40;
        state = {
          x: targetX,
          y: spawnY,
          targetX,
          targetY,
          scaleX: 1.25,
          scaleY: 0.75,
          vy: 4,
          status: el.status,
        };
        this.animStates.set(el.id, state);
        if (particles) {
          particles.spawnSparks(targetX, targetY, 0x5eead4, 6);
        }
      } else {
        if (Math.abs(state.targetX - targetX) > 2) {
          if (state.status !== 'swap') {
            state.x = targetX;
            state.y = targetY;
          }
          state.targetX = targetX;
          state.targetY = targetY;
        }
        state.status = el.status;
      }

      const color = PIXI_STATUS_COLORS[el.status] ?? PIXI_STATUS_COLORS.default;
      const isTopOrFront = isStack ? i === n - 1 : i === 0;

      // 1. Idle Breathing pulse on TOP / FRONT element
      if (isTopOrFront && elements.length > 0) {
        const pulse = Math.sin(this.idleTime * 2) * 3;
        this.glowGraphics
          .roundRect(
            state.x - (cellW * state.scaleX) / 2 - 4 - pulse / 2,
            state.y - (cellH * state.scaleY) / 2 - 4 - pulse / 2,
            cellW * state.scaleX + 8 + pulse,
            cellH * state.scaleY + 8 + pulse,
            8,
          )
          .stroke({ width: 1.5, color: 0x5eead4, alpha: 0.4 + Math.sin(this.idleTime * 2) * 0.2 });

        // Indicator label TOP / FRONT
        const indLabel = isStack ? '▲ TOP' : '◄ FRONT';
        const indX = isStack ? state.x + cellW / 2 + 28 : state.x;
        const indY = isStack ? state.y : state.y - cellH / 2 - 14;
        this.acquireText(indLabel, this.labelTextStyle, indX, indY);
      }

      // 2. Draw element box
      const rw = cellW * state.scaleX;
      const rh = cellH * state.scaleY;
      const rx = state.x - rw / 2;
      const ry = state.y - rh / 2;

      this.itemGraphics
        .roundRect(rx, ry, rw, rh, 6)
        .fill({ color, alpha: el.status === 'muted' ? 0.35 : 0.92 })
        .stroke({ width: 1.5, color: 0x090d16, alpha: 0.85 });

      // Shine bevel
      if (el.status !== 'muted') {
        this.itemGraphics
          .roundRect(rx + 2, ry + 2, rw - 4, rh * 0.35, 3)
          .fill({ color: 0xffffff, alpha: 0.3 });
      }

      // 3. Draw Linked List pointer arrows
      if (isLinkedList && i < n - 1) {
        const arrowStartX = rx + rw;
        const arrowEndX = arrowStartX + 28;
        const arrowY = state.y;

        this.linkGraphics
          .moveTo(arrowStartX, arrowY)
          .lineTo(arrowEndX, arrowY)
          .stroke({ width: 2, color: 0x5eead4, alpha: 0.7 })
          .moveTo(arrowEndX - 5, arrowY - 4)
          .lineTo(arrowEndX, arrowY)
          .lineTo(arrowEndX - 5, arrowY + 4)
          .stroke({ width: 2, color: 0x5eead4, alpha: 0.7 });
      }

      // Null indicator at end of list
      if (isLinkedList && i === n - 1) {
        const nullX = rx + rw + 16;
        this.acquireText('∅', this.labelTextStyle, nullX, state.y);
      }

      // 4. Value text
      if (opts.showValues) {
        const textColor = el.status === 'muted' ? 0x94a3b8 : 0x090d16;
        const style = new TextStyle({
          ...this.itemTextStyle,
          fill: textColor,
        });
        this.acquireText(el.label, style, state.x, state.y);
      }
    });

    // Draw Stack bottom container base
    if (isStack) {
      const baseW = cellW + 24;
      const baseY = stackBaseY;
      this.linkGraphics
        .moveTo(width / 2 - baseW / 2, baseY - 12)
        .lineTo(width / 2 - baseW / 2, baseY)
        .lineTo(width / 2 + baseW / 2, baseY)
        .lineTo(width / 2 + baseW / 2, baseY - 12)
        .stroke({ width: 3, color: 0x334155, alpha: 0.8 });
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
