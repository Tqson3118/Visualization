// src/engines/renderers/pixi/ParticleManager.ts
// High-performance Particle Pooling System for WebGL Canvas
// Supports: Spark burst (comparisons/swaps), Motion trail, Laser bullet (graph edges),
// Confetti celebration (completion), Dissolve dust (pop/delete).
// Respects prefers-reduced-motion.

import { Container, Graphics } from 'pixi.js';

export interface Particle {
  active: boolean;
  x: number;
  y: number;
  vx: number;
  vy: number;
  alpha: number;
  decay: number;
  scale: number;
  scaleDecay: number;
  color: number;
  rotation: number;
  vRot: number;
  gravity: number;
  shape: 'circle' | 'square' | 'spark' | 'confetti';
  size: number;
  life: number;
  maxLife: number;
}

export interface LaserBullet {
  active: boolean;
  startX: number;
  startY: number;
  endX: number;
  endY: number;
  progress: number; // 0 -> 1
  speed: number;
  color: number;
  radius: number;
  trailLength: number;
  onComplete?: () => void;
}

const MAX_PARTICLES = 300;
const MAX_BULLETS = 20;

export class ParticleManager {
  public container: Container;
  private graphics: Graphics;
  private pool: Particle[] = [];
  private bullets: LaserBullet[] = [];
  private reducedMotion = false;
  private isDestroyed = false;

  constructor() {
    this.container = new Container();
    this.graphics = new Graphics();
    this.container.addChild(this.graphics);

    this.checkReducedMotion();
    this.initPool();
  }

  public checkReducedMotion(): boolean {
    if (typeof window !== 'undefined' && window.matchMedia) {
      this.reducedMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    }
    return this.reducedMotion;
  }

  private initPool(): void {
    for (let i = 0; i < MAX_PARTICLES; i++) {
      this.pool.push({
        active: false,
        x: 0,
        y: 0,
        vx: 0,
        vy: 0,
        alpha: 1,
        decay: 0.02,
        scale: 1,
        scaleDecay: 0.01,
        color: 0x5eead4,
        rotation: 0,
        vRot: 0,
        gravity: 0.1,
        shape: 'spark',
        size: 4,
        life: 0,
        maxLife: 60,
      });
    }

    for (let i = 0; i < MAX_BULLETS; i++) {
      this.bullets.push({
        active: false,
        startX: 0,
        startY: 0,
        endX: 0,
        endY: 0,
        progress: 0,
        speed: 0.05,
        color: 0x38bdf8,
        radius: 4,
        trailLength: 15,
      });
    }
  }

  private acquireParticle(): Particle | null {
    if (this.reducedMotion) return null;
    for (let i = 0; i < this.pool.length; i++) {
      if (!this.pool[i].active) {
        this.pool[i].active = true;
        return this.pool[i];
      }
    }
    return null;
  }

  /** Radial spark explosion (Compare, Swap, Collision) */
  public spawnSparks(x: number, y: number, color = 0xf59e0b, count = 16): void {
    if (this.reducedMotion) return;
    const actualCount = Math.min(count, 32);
    for (let i = 0; i < actualCount; i++) {
      const p = this.acquireParticle();
      if (!p) break;

      const angle = (Math.PI * 2 * i) / actualCount + (Math.random() - 0.5) * 0.5;
      const speed = 2 + Math.random() * 4.5;

      p.x = x;
      p.y = y;
      p.vx = Math.cos(angle) * speed;
      p.vy = Math.sin(angle) * speed;
      p.alpha = 1;
      p.decay = 0.025 + Math.random() * 0.025;
      p.scale = 1;
      p.scaleDecay = 0.02;
      p.color = color;
      p.rotation = Math.random() * Math.PI;
      p.vRot = (Math.random() - 0.5) * 0.2;
      p.gravity = 0.08;
      p.shape = 'spark';
      p.size = 3 + Math.random() * 2.5;
      p.life = 0;
      p.maxLife = 40;
    }
  }

  /** Soft motion trail behind moving elements */
  public spawnTrail(x: number, y: number, color = 0x5eead4, size = 5): void {
    if (this.reducedMotion) return;
    const p = this.acquireParticle();
    if (!p) return;

    p.x = x + (Math.random() - 0.5) * 4;
    p.y = y + (Math.random() - 0.5) * 4;
    p.vx = (Math.random() - 0.5) * 0.5;
    p.vy = (Math.random() - 0.5) * 0.5;
    p.alpha = 0.6;
    p.decay = 0.04;
    p.scale = 1;
    p.scaleDecay = 0.04;
    p.color = color;
    p.rotation = 0;
    p.vRot = 0;
    p.gravity = 0;
    p.shape = 'circle';
    p.size = size;
    p.life = 0;
    p.maxLife = 25;
  }

  /** Laser Bullet energy projectile running along graph edges */
  public spawnLaserBullet(
    startX: number,
    startY: number,
    endX: number,
    endY: number,
    color = 0x38bdf8,
    speed = 0.04,
    onComplete?: () => void,
  ): void {
    if (this.reducedMotion) {
      if (onComplete) onComplete();
      return;
    }
    for (let i = 0; i < this.bullets.length; i++) {
      if (!this.bullets[i].active) {
        const b = this.bullets[i];
        b.active = true;
        b.startX = startX;
        b.startY = startY;
        b.endX = endX;
        b.endY = endY;
        b.progress = 0;
        b.speed = speed;
        b.color = color;
        b.radius = 4.5;
        b.trailLength = 20;
        b.onComplete = onComplete;
        return;
      }
    }
    // Fallback if full
    if (onComplete) onComplete();
  }

  /** Confetti celebration fireworks when algorithm finishes */
  public spawnConfetti(stageWidth: number, stageHeight: number, count = 45): void {
    if (this.reducedMotion) return;
    const CONFETTI_COLORS = [0x5eead4, 0xfbbf24, 0xf87171, 0x34d399, 0xa78bfa, 0x38bdf8];

    for (let i = 0; i < count; i++) {
      const p = this.acquireParticle();
      if (!p) break;

      const fromLeft = Math.random() < 0.5;
      p.x = fromLeft ? stageWidth * 0.15 : stageWidth * 0.85;
      p.y = stageHeight * 0.75;
      const angle = fromLeft
        ? -Math.PI * 0.35 + (Math.random() - 0.5) * 0.4
        : -Math.PI * 0.65 + (Math.random() - 0.5) * 0.4;
      const speed = 7 + Math.random() * 8;

      p.vx = Math.cos(angle) * speed;
      p.vy = Math.sin(angle) * speed;
      p.alpha = 1;
      p.decay = 0.012;
      p.scale = 1;
      p.scaleDecay = 0.003;
      p.color = CONFETTI_COLORS[Math.floor(Math.random() * CONFETTI_COLORS.length)];
      p.rotation = Math.random() * Math.PI * 2;
      p.vRot = (Math.random() - 0.5) * 0.25;
      p.gravity = 0.22;
      p.shape = 'confetti';
      p.size = 5 + Math.random() * 4;
      p.life = 0;
      p.maxLife = 90;
    }
  }

  /** Dissolve dust when an element is popped / deleted */
  public spawnDissolveDust(x: number, y: number, w: number, h: number, color = 0xf87171, count = 18): void {
    if (this.reducedMotion) return;
    for (let i = 0; i < count; i++) {
      const p = this.acquireParticle();
      if (!p) break;

      p.x = x - w / 2 + Math.random() * w;
      p.y = y - h / 2 + Math.random() * h;
      p.vx = (Math.random() - 0.5) * 2;
      p.vy = -1 - Math.random() * 2.5;
      p.alpha = 0.9;
      p.decay = 0.035;
      p.scale = 1;
      p.scaleDecay = 0.03;
      p.color = color;
      p.rotation = Math.random() * Math.PI;
      p.vRot = (Math.random() - 0.5) * 0.1;
      p.gravity = -0.04; // floats upward gently
      p.shape = 'square';
      p.size = 2.5 + Math.random() * 3;
      p.life = 0;
      p.maxLife = 35;
    }
  }

  /** Update all particles and laser bullets in the frame */
  public update(deltaMs = 16.67): void {
    const timeScale = Math.min(2, Math.max(0.5, deltaMs / 16.67));

    // Update Particles
    for (let i = 0; i < this.pool.length; i++) {
      const p = this.pool[i];
      if (!p.active) continue;

      p.life += timeScale;
      p.x += p.vx * timeScale;
      p.y += p.vy * timeScale;
      p.vy += p.gravity * timeScale;
      p.rotation += p.vRot * timeScale;
      p.alpha -= p.decay * timeScale;
      p.scale = Math.max(0, p.scale - p.scaleDecay * timeScale);

      if (p.alpha <= 0 || p.scale <= 0 || p.life >= p.maxLife) {
        p.active = false;
      }
    }

    // Update Laser Bullets
    for (let i = 0; i < this.bullets.length; i++) {
      const b = this.bullets[i];
      if (!b.active) continue;

      b.progress += b.speed * timeScale;
      if (b.progress >= 1) {
        b.active = false;
        b.progress = 1;
        if (b.onComplete) {
          b.onComplete();
        }
      }
    }
  }

  /** Render all particles and bullets to single Graphics */
  public render(): void {
    this.graphics.clear();

    // Render Particles
    for (let i = 0; i < this.pool.length; i++) {
      const p = this.pool[i];
      if (!p.active || p.alpha <= 0) continue;

      const size = p.size * p.scale;
      if (size <= 0.5) continue;

      if (p.shape === 'spark') {
        this.graphics
          .circle(p.x, p.y, size)
          .fill({ color: p.color, alpha: p.alpha * 0.9 })
          .circle(p.x, p.y, size * 0.5)
          .fill({ color: 0xffffff, alpha: p.alpha });
      } else if (p.shape === 'circle') {
        this.graphics
          .circle(p.x, p.y, size)
          .fill({ color: p.color, alpha: p.alpha });
      } else if (p.shape === 'confetti') {
        const half = size;
        this.graphics
          .poly([
            p.x + Math.cos(p.rotation) * half,
            p.y + Math.sin(p.rotation) * half,
            p.x + Math.cos(p.rotation + 1.5) * half,
            p.y + Math.sin(p.rotation + 1.5) * half,
            p.x + Math.cos(p.rotation + 3.14) * half,
            p.y + Math.sin(p.rotation + 3.14) * half,
            p.x + Math.cos(p.rotation + 4.64) * half,
            p.y + Math.sin(p.rotation + 4.64) * half,
          ])
          .fill({ color: p.color, alpha: p.alpha });
      } else {
        // square
        this.graphics
          .rect(p.x - size / 2, p.y - size / 2, size, size)
          .fill({ color: p.color, alpha: p.alpha });
      }
    }

    // Render Laser Bullets
    for (let i = 0; i < this.bullets.length; i++) {
      const b = this.bullets[i];
      if (!b.active) continue;

      const curX = b.startX + (b.endX - b.startX) * b.progress;
      const curY = b.startY + (b.endY - b.startY) * b.progress;

      // Glow beam trail
      const trailProgress = Math.max(0, b.progress - 0.25);
      const trailStartX = b.startX + (b.endX - b.startX) * trailProgress;
      const trailStartY = b.startY + (b.endY - b.startY) * trailProgress;

      this.graphics
        .moveTo(trailStartX, trailStartY)
        .lineTo(curX, curY)
        .stroke({ width: b.radius * 1.5, color: b.color, alpha: 0.7 });

      // Core head
      this.graphics
        .circle(curX, curY, b.radius)
        .fill({ color: 0xffffff, alpha: 1 })
        .circle(curX, curY, b.radius * 1.8)
        .fill({ color: b.color, alpha: 0.5 });
    }
  }

  public clear(): void {
    for (let i = 0; i < this.pool.length; i++) {
      this.pool[i].active = false;
    }
    for (let i = 0; i < this.bullets.length; i++) {
      this.bullets[i].active = false;
    }
    this.graphics.clear();
  }

  public destroy(): void {
    // Idempotent: guard chống destroy 2 lần (B1/B2) — sau destroy đầu, graphics/container
    // đã bị phá hủy nên gọi lại sẽ ném lỗi.
    if (this.isDestroyed) return;
    this.isDestroyed = true;
    try {
      this.clear();
      this.container.destroy({ children: true });
    } catch {}
  }
}
