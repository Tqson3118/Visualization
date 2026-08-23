// src/engines/renderers/pixi/PixiTreePainter.ts
// Specialized WebGL Painter for Tree algorithms (BST Insert/Delete/Search, AVL Rotation, Heap, etc.)
// Features:
// - Smooth Tree Rotation interpolation & layout transitions
// - Quadratic Bezier branch links with glowing highlights
// - Ghost afterimage on displaced subtrees
// - Node Glow Pulse halo on visited/active nodes
// - Auxiliary heap array strip support

import { Container, Graphics, Text, TextStyle } from 'pixi.js';
import type { Element, ElementStatus, Link, Structure } from '@/engines/core/types';
import { PIXI_STATUS_COLORS } from './PixiArrayPainter';
import type { ParticleManager } from './ParticleManager';

export interface TreePainterOptions {
  showIndex: boolean;
  showValues: boolean;
  zoom: number;
}

interface TreeNodePos {
  id: string;
  x: number;
  y: number;
  radius: number;
  status: ElementStatus;
  label: string;
}

interface NodeAnimState {
  x: number;
  y: number;
  targetX: number;
  targetY: number;
  ghostX: number;
  ghostY: number;
  ghostAlpha: number;
  pulsePhase: number;
  status: ElementStatus;
}

export class PixiTreePainter {
  public container: Container;
  private linkGraphics: Graphics;
  private nodeGraphics: Graphics;
  private glowGraphics: Graphics;
  private textContainer: Container;
  private textPool: Text[] = [];
  private textPoolIndex = 0;

  private animStates = new Map<string, NodeAnimState>();
  private globalPulse = 0;

  private nodeTextStyle: TextStyle;
  private linkTextStyle: TextStyle;
  private isDestroyed = false;

  constructor() {
    this.container = new Container();
    this.glowGraphics = new Graphics();
    this.linkGraphics = new Graphics();
    this.nodeGraphics = new Graphics();
    this.textContainer = new Container();

    this.container.addChild(this.glowGraphics);
    this.container.addChild(this.linkGraphics);
    this.container.addChild(this.nodeGraphics);
    this.container.addChild(this.textContainer);

    this.nodeTextStyle = new TextStyle({
      fontFamily: "'JetBrains Mono', 'Fira Code', ui-monospace, monospace",
      fontSize: 12,
      fontWeight: 'bold',
      fill: 0x090d16,
      align: 'center',
    });

    this.linkTextStyle = new TextStyle({
      fontFamily: "'JetBrains Mono', 'Fira Code', ui-monospace, monospace",
      fontSize: 10,
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
    this.globalPulse += 0.06 * timeScale;

    this.animStates.forEach((state) => {
      // Check for big movement (rotation / relocation) -> create ghost
      const dist = Math.hypot(state.targetX - state.x, state.targetY - state.y);
      if (dist > 15 && state.ghostAlpha <= 0.05) {
        state.ghostX = state.x;
        state.ghostY = state.y;
        state.ghostAlpha = 0.5;
      }

      state.x += (state.targetX - state.x) * 0.22 * timeScale;
      state.y += (state.targetY - state.y) * 0.22 * timeScale;
      state.ghostAlpha = Math.max(0, state.ghostAlpha - 0.03 * timeScale);
      state.pulsePhase += 0.05 * timeScale;
    });
  }

  /** Compute hierarchical tree node positions */
  private layoutTree(
    elements: Element[],
    links: Link[],
    width: number,
    height: number,
  ): Map<string, TreeNodePos> {
    const positions = new Map<string, TreeNodePos>();
    if (elements.length === 0) return positions;

    // Filter tree elements vs heap-array elements
    const treeElements = elements.filter((el) => el.group !== 'heap-array');
    if (treeElements.length === 0) return positions;

    const childrenMap = new Map<string, { left?: string; right?: string }>();
    const parents = new Set<string>();

    for (const link of links) {
      if (!childrenMap.has(link.from)) childrenMap.set(link.from, {});
      const c = childrenMap.get(link.from)!;
      if (link.label === 'L' || link.label === 'left' || !c.left) {
        c.left = link.to;
      } else {
        c.right = link.to;
      }
      parents.add(link.to);
    }

    const roots = treeElements.filter((el) => !parents.has(el.id));
    const rootId = roots.length > 0 ? roots[0].id : treeElements[0].id;

    // Assign depths & in-order X positions
    const depths = new Map<string, number>();
    const inOrderList: string[] = [];

    const traverse = (nodeId: string, depth: number) => {
      depths.set(nodeId, depth);
      const ch = childrenMap.get(nodeId);
      if (ch?.left) traverse(ch.left, depth + 1);
      inOrderList.push(nodeId);
      if (ch?.right) traverse(ch.right, depth + 1);
    };

    traverse(rootId, 0);

    // Fallback for unvisited nodes
    for (const el of treeElements) {
      if (!depths.has(el.id)) {
        depths.set(el.id, 0);
        inOrderList.push(el.id);
      }
    }

    const maxDepth = Math.max(1, ...Array.from(depths.values()));
    const stepX = (width * 0.85) / Math.max(1, inOrderList.length);
    const startX = (width - stepX * inOrderList.length) / 2;
    const radius = Math.min(24, Math.max(15, stepX * 0.38));
    const levelH = Math.min(85, Math.max(55, (height - 60) / (maxDepth + 1.2)));
    const totalTreeH = maxDepth * levelH + 2 * radius;
    const treeStartY = Math.max(radius + 15, (height - totalTreeH) / 2 + radius);

    inOrderList.forEach((id, inIdx) => {
      const depth = depths.get(id) ?? 0;
      const el = treeElements.find((e) => e.id === id);
      if (!el) return;

      const x = startX + (inIdx + 0.5) * stepX;
      const y = treeStartY + depth * levelH;

      positions.set(id, {
        id,
        x,
        y,
        radius,
        status: el.status,
        label: el.label,
      });
    });

    return positions;
  }

  public render(
    structure: Structure,
    opts: TreePainterOptions,
    width: number,
    height: number,
    particles?: ParticleManager,
  ): void {
    if (this.isDestroyed || !this.nodeGraphics || (this.nodeGraphics as any).destroyed) return;
    this.linkGraphics.clear();
    this.nodeGraphics.clear();
    this.glowGraphics.clear();
    this.resetTextPool();

    const positions = this.layoutTree(structure.elements, structure.links, width, height);

    // Update animations targets
    positions.forEach((pos, id) => {
      let state = this.animStates.get(id);
      if (!state) {
        state = {
          x: pos.x,
          y: pos.y,
          targetX: pos.x,
          targetY: pos.y,
          ghostX: pos.x,
          ghostY: pos.y,
          ghostAlpha: 0,
          pulsePhase: Math.random() * Math.PI,
          status: pos.status,
        };
        this.animStates.set(id, state);
      } else {
        state.targetX = pos.x;
        state.targetY = pos.y;
        state.status = pos.status;
      }
    });

    // 1. Draw Tree Branch Links with Quadratic Bezier curves
    for (const link of structure.links) {
      const fromPos = positions.get(link.from);
      const toPos = positions.get(link.to);
      const fromState = this.animStates.get(link.from);
      const toState = this.animStates.get(link.to);

      if (!fromState || !toState || !fromPos || !toPos) continue;

      const sx = fromState.x;
      const sy = fromState.y + fromPos.radius * 0.7;
      const ex = toState.x;
      const ey = toState.y - toPos.radius * 0.7;
      const midY = (sy + ey) / 2;

      const isLinkActive = link.status === 'active' || link.status === 'swap';
      const linkColor = isLinkActive ? 0xfbbf24 : 0x334155;
      const linkWidth = isLinkActive ? 3 : 2;

      // Draw curved branch
      this.linkGraphics
        .moveTo(sx, sy)
        .bezierCurveTo(sx, midY, ex, midY, ex, ey)
        .stroke({ width: linkWidth, color: linkColor, alpha: isLinkActive ? 0.9 : 0.6 });

      if (link.label) {
        const lx = (sx + ex) / 2 + (sx < ex ? 8 : -8);
        const ly = midY;
        this.acquireText(link.label, this.linkTextStyle, lx, ly);
      }
    }

    // 2. Draw Nodes, Glow Pulses, and Ghost Afterimages
    positions.forEach((pos, id) => {
      const state = this.animStates.get(id);
      if (!state) return;

      const color = PIXI_STATUS_COLORS[pos.status] ?? PIXI_STATUS_COLORS.default;
      const isPulse = pos.status === 'active' || pos.status === 'highlight' || pos.status === 'swap';

      // Ghost afterimage on rotation
      if (state.ghostAlpha > 0.05) {
        this.glowGraphics
          .circle(state.ghostX, state.ghostY, pos.radius)
          .stroke({ width: 1.5, color: 0x5eead4, alpha: state.ghostAlpha });
      }

      // Pulsing glow aura
      if (isPulse) {
        const pulseR = pos.radius + 6 + Math.sin(state.pulsePhase * 2) * 3;
        this.glowGraphics
          .circle(state.x, state.y, pulseR)
          .fill({ color, alpha: 0.25 })
          .circle(state.x, state.y, pulseR + 3)
          .stroke({ width: 1.5, color, alpha: 0.4 });

        if (particles && Math.random() < 0.2) {
          particles.spawnSparks(state.x, state.y, color, 4);
        }
      }

      // Node circle body
      this.nodeGraphics
        .circle(state.x, state.y, pos.radius)
        .fill({ color, alpha: pos.status === 'muted' ? 0.4 : 0.95 })
        .stroke({ width: 2, color: 0x090d16, alpha: 0.85 });

      // Bevel highlight
      if (pos.status !== 'muted') {
        this.nodeGraphics
          .circle(state.x - pos.radius * 0.25, state.y - pos.radius * 0.25, pos.radius * 0.4)
          .fill({ color: 0xffffff, alpha: 0.35 });
      }

      // Value label
      if (opts.showValues) {
        const textColor = pos.status === 'muted' ? 0x94a3b8 : 0x090d16;
        const style = new TextStyle({
          ...this.nodeTextStyle,
          fontSize: Math.min(13, Math.max(9, pos.radius * 0.75)),
          fill: textColor,
        });
        this.acquireText(pos.label, style, state.x, state.y);
      }
    });
  }

  public destroy(): void {
    this.isDestroyed = true;
    try {
      this.textContainer.destroy({ children: true });
      this.container.destroy({ children: true });
    } catch {}
  }
}
