// src/engines/renderers/pixi/PixiGraphPainter.ts
// Specialized WebGL Painter for Graph algorithms (BFS, DFS, Dijkstra, Prim, Kruskal, Topological Sort)
// Features:
// - Laser bullet energy beam traveling along graph edges
// - Ripple visited bounce effect on visited nodes
// - Path found neon glow on final path
// - Directed arrowheads & weight badges
// - Clean circular layout with ample spacing and crisp typography

import { Container, Graphics, Text, TextStyle } from 'pixi.js';
import type { Element, ElementStatus, Link, Structure } from '@/engines/core/types';
import { PIXI_STATUS_COLORS } from './PixiArrayPainter';
import type { ParticleManager } from './ParticleManager';

export interface GraphPainterOptions {
  showIndex: boolean;
  showValues: boolean;
  zoom: number;
}

interface GraphNodePos {
  id: string;
  x: number;
  y: number;
  radius: number;
  status: ElementStatus;
  label: string;
}

interface NodeRipple {
  x: number;
  y: number;
  radius: number;
  maxRadius: number;
  alpha: number;
  color: number;
}

export class PixiGraphPainter {
  public container: Container;
  private linkGraphics: Graphics;
  private pathGlowGraphics: Graphics;
  private nodeGraphics: Graphics;
  private rippleGraphics: Graphics;
  private textContainer: Container;
  private textPool: Text[] = [];
  private textPoolIndex = 0;

  private ripples: NodeRipple[] = [];
  private visitedSet = new Set<string>();
  private activeEdgeKeys = new Set<string>();

  private nodeTextStyle: TextStyle;
  private edgeTextStyle: TextStyle;
  private isDestroyed = false;

  constructor() {
    this.container = new Container();
    this.pathGlowGraphics = new Graphics();
    this.linkGraphics = new Graphics();
    this.rippleGraphics = new Graphics();
    this.nodeGraphics = new Graphics();
    this.textContainer = new Container();

    this.container.addChild(this.pathGlowGraphics);
    this.container.addChild(this.linkGraphics);
    this.container.addChild(this.rippleGraphics);
    this.container.addChild(this.nodeGraphics);
    this.container.addChild(this.textContainer);

    this.nodeTextStyle = new TextStyle({
      fontFamily: "'JetBrains Mono', 'Fira Code', ui-monospace, monospace",
      fontSize: 13,
      fontWeight: 'bold',
      fill: 0x090d16,
      align: 'center',
    });

    this.edgeTextStyle = new TextStyle({
      fontFamily: "'JetBrains Mono', 'Fira Code', ui-monospace, monospace",
      fontSize: 11,
      fontWeight: '600',
      fill: 0x38bdf8,
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

    // Update ripples
    for (let i = this.ripples.length - 1; i >= 0; i--) {
      const r = this.ripples[i];
      r.radius += 1.2 * timeScale;
      r.alpha -= 0.03 * timeScale;
      if (r.alpha <= 0 || r.radius >= r.maxRadius) {
        this.ripples.splice(i, 1);
      }
    }
  }

  private layoutGraph(elements: Element[], width: number, height: number): Map<string, GraphNodePos> {
    const positions = new Map<string, GraphNodePos>();
    if (elements.length === 0) return positions;

    // Check if elements have meta x/y
    const hasMetaCoords = elements.every(
      (el) => el.meta && typeof el.meta.x === 'number' && typeof el.meta.y === 'number',
    );

    if (hasMetaCoords) {
      const minX = Math.min(...elements.map((el) => el.meta!.x as number));
      const maxX = Math.max(...elements.map((el) => el.meta!.x as number));
      const minY = Math.min(...elements.map((el) => el.meta!.y as number));
      const maxY = Math.max(...elements.map((el) => el.meta!.y as number));

      const margin = 56;
      const scaleX = maxX > minX ? (width - margin * 2) / (maxX - minX) : 1;
      const scaleY = maxY > minY ? (height - margin * 2) / (maxY - minY) : 1;

      elements.forEach((el) => {
        const x = margin + ((el.meta!.x as number) - minX) * scaleX;
        const y = margin + ((el.meta!.y as number) - minY) * scaleY;
        positions.set(el.id, {
          id: el.id,
          x,
          y,
          radius: 22,
          status: el.status,
          label: el.label,
        });
      });
      return positions;
    }

    // Wide landscape elliptical layout filling canvas gracefully
    const rx = Math.min(width * 0.42, Math.max(140, width / 2 - 38));
    const ry = Math.min(height * 0.40, Math.max(110, height / 2 - 36));
    const cx = width / 2;
    const cy = height / 2;
    const n = elements.length;

    elements.forEach((el, i) => {
      const angle = (i / Math.max(1, n)) * Math.PI * 2 - Math.PI / 2;
      positions.set(el.id, {
        id: el.id,
        x: cx + rx * Math.cos(angle),
        y: cy + ry * Math.sin(angle),
        radius: 26,
        status: el.status,
        label: el.label,
      });
    });

    return positions;
  }

  public render(
    structure: Structure,
    opts: GraphPainterOptions,
    width: number,
    height: number,
    particles?: ParticleManager,
  ): void {
    if (this.isDestroyed || !this.nodeGraphics || (this.nodeGraphics as any).destroyed) return;
    this.linkGraphics.clear();
    this.pathGlowGraphics.clear();
    this.rippleGraphics.clear();
    this.nodeGraphics.clear();
    this.resetTextPool();

    const positions = this.layoutGraph(structure.elements, width, height);

    // 1. Draw Edges and trigger Laser Bullet projectiles
    for (const link of structure.links) {
      const from = positions.get(link.from);
      const to = positions.get(link.to);
      if (!from || !to) continue;

      const edgeKey = `${link.from}->${link.to}`;
      const isPath = link.status === 'done';
      const isActive = link.status === 'active' || link.status === 'swap';

      // Laser bullet spawn on newly activated edge
      if (isActive && !this.activeEdgeKeys.has(edgeKey)) {
        this.activeEdgeKeys.add(edgeKey);
        if (particles) {
          particles.spawnLaserBullet(from.x, from.y, to.x, to.y, 0x38bdf8, 0.06);
        }
      } else if (!isActive) {
        this.activeEdgeKeys.delete(edgeKey);
      }

      // Calculate edge endpoints on node perimeters (so lines never cross inside circles)
      const dx = to.x - from.x;
      const dy = to.y - from.y;
      const dist = Math.hypot(dx, dy) || 1;
      const ux = dx / dist;
      const uy = dy / dist;

      const startX = from.x + ux * from.radius;
      const startY = from.y + uy * from.radius;
      const endX = to.x - ux * (to.radius + 2);
      const endY = to.y - uy * (to.radius + 2);

      // Path found neon glow
      if (isPath) {
        this.pathGlowGraphics
          .moveTo(startX, startY)
          .lineTo(endX, endY)
          .stroke({ width: 8, color: 0x34d399, alpha: 0.35 });
      }

      // Edge line — crisp and high-contrast
      const strokeColor = isPath ? 0x10b981 : isActive ? 0x38bdf8 : 0x64748b;
      const strokeWidth = isPath ? 3.5 : isActive ? 3 : 2;

      this.linkGraphics
        .moveTo(startX, startY)
        .lineTo(endX, endY)
        .stroke({ width: strokeWidth, color: strokeColor, alpha: isActive || isPath ? 0.95 : 0.75 });

      // Edge weight badge with offset and distributed t-parameter so chords never collide
      if (opts.showValues && link.label) {
        const nx = -uy;
        const ny = ux;

        // Distribute placement ratio between 0.25 and 0.75 cycling through distinct positions
        const edgeIdx = structure.links.indexOf(link);
        const t = 0.25 + ((edgeIdx * 3 + 1) % 5) * 0.12; // 0.37, 0.73, 0.61, 0.49, 0.25
        const mx = startX + (endX - startX) * t + nx * 14;
        const my = startY + (endY - startY) * t + ny * 14;

        this.linkGraphics
          .roundRect(mx - 15, my - 10, 30, 20, 5)
          .fill({ color: 0x0f172a, alpha: 0.95 })
          .stroke({ width: 1.5, color: strokeColor, alpha: 0.9 });

        this.acquireText(link.label, this.edgeTextStyle, mx, my);
      }
    }

    // 2. Draw Ripples
    for (const r of this.ripples) {
      this.rippleGraphics
        .circle(r.x, r.y, r.radius)
        .stroke({ width: 2, color: r.color, alpha: r.alpha });
    }

    // 3. Draw Vertices and visited triggers
    positions.forEach((pos, id) => {
      const color = PIXI_STATUS_COLORS[pos.status] ?? PIXI_STATUS_COLORS.default;
      const isVisited = pos.status === 'done' || pos.status === 'active';

      // Check if newly visited -> trigger ripple
      if (isVisited && !this.visitedSet.has(id)) {
        this.visitedSet.add(id);
        this.ripples.push({
          x: pos.x,
          y: pos.y,
          radius: pos.radius,
          maxRadius: pos.radius + 28,
          alpha: 0.8,
          color,
        });
        if (particles) {
          particles.spawnSparks(pos.x, pos.y, color, 8);
        }
      }

      // Glow halo for active / done nodes
      if (pos.status === 'active' || pos.status === 'highlight') {
        this.nodeGraphics
          .circle(pos.x, pos.y, pos.radius + 6)
          .fill({ color, alpha: 0.25 })
          .circle(pos.x, pos.y, pos.radius + 8)
          .stroke({ width: 1.5, color, alpha: 0.5 });
      }

      // Vertex circle body
      this.nodeGraphics
        .circle(pos.x, pos.y, pos.radius)
        .fill({ color, alpha: pos.status === 'muted' ? 0.4 : 0.95 })
        .stroke({ width: 2, color: 0x090d16, alpha: 0.95 });

      // Highlight bevel shine
      if (pos.status !== 'muted') {
        this.nodeGraphics
          .circle(pos.x - pos.radius * 0.25, pos.y - pos.radius * 0.25, pos.radius * 0.4)
          .fill({ color: 0xffffff, alpha: 0.35 });
      }

      // Vertex label
      if (opts.showValues) {
        const textColor = pos.status === 'muted' ? 0x94a3b8 : 0x090d16;
        const style = new TextStyle({
          ...this.nodeTextStyle,
          fontSize: Math.min(13, pos.radius * 0.75),
          fill: textColor,
        });
        this.acquireText(pos.label, style, pos.x, pos.y);
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
