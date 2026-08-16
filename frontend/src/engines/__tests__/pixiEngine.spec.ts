// src/engines/__tests__/pixiEngine.spec.ts
import { describe, expect, it, beforeEach, vi } from 'vitest';
import type { Structure } from '../core/types';
import {
  ParticleManager,
  PixiArrayPainter,
  PixiGraphPainter,
  PixiLinearPainter,
  PixiTreePainter,
} from '../renderers/pixi';

describe('ParticleManager', () => {
  beforeEach(() => {
    window.matchMedia = vi.fn().mockReturnValue({
      matches: false,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    });
  });

  it('spawns and updates radial spark bursts without throwing', () => {
    const pm = new ParticleManager();
    pm.spawnSparks(100, 100, 0xf59e0b, 16);
    // Pool phải có particle active thật (graphics luôn là con duy nhất của container,
    // nên count active = field phản ánh đúng số particle đã được spawn)
    const active = (pm as unknown as { pool: { active: boolean }[] }).pool.filter((p) => p.active).length;
    expect(active).toBeGreaterThan(0);
    expect(() => pm.update(16.67)).not.toThrow();
    expect(() => pm.render()).not.toThrow();
    pm.clear();
    pm.destroy();
    // destroy idempotent: gọi lần 2 không throw (B1/B2)
    expect(() => pm.destroy()).not.toThrow();
  });

  it('spawns laser bullet with trail progression and completion callback', () => {
    const pm = new ParticleManager();
    let completed = false;
    pm.spawnLaserBullet(0, 0, 100, 100, 0x38bdf8, 0.5, () => {
      completed = true;
    });

    // Advance frames to complete bullet travel
    pm.update(30);
    pm.update(30);
    pm.update(30);
    expect(() => pm.render()).not.toThrow();
    expect(completed).toBe(true);
    pm.destroy();
  });

  it('spawns confetti celebration and dissolve dust', () => {
    const pm = new ParticleManager();
    expect(() => pm.spawnConfetti(800, 420, 20)).not.toThrow();
    expect(() => pm.spawnDissolveDust(200, 200, 50, 30, 0xf87171, 10)).not.toThrow();
    pm.update(16.67);
    pm.render();
    pm.destroy();
  });

  it('respects prefers-reduced-motion: does not spawn particles when active', () => {
    window.matchMedia = vi.fn().mockReturnValue({
      matches: true,
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
    });

    const pm = new ParticleManager();
    pm.spawnSparks(100, 100, 0xf59e0b, 16);
    pm.update(16.67);
    pm.render();
    // Reduced-motion: không particle nào active (children luôn có 1 graphics nên
    // đếm qua pool = field thật thể hiện không spawn)
    const active = (pm as unknown as { pool: { active: boolean }[] }).pool.filter((p) => p.active).length;
    expect(active).toBe(0);
    pm.destroy();
    expect(() => pm.destroy()).not.toThrow();
  });
});

describe('PixiArrayPainter', () => {
  it('renders array elements with bar mode and handles updates', () => {
    const painter = new PixiArrayPainter();
    const pm = new ParticleManager();
    const structure: Structure = {
      kind: 'array',
      elements: [
        { id: 'cell:0', label: '10', status: 'default' },
        { id: 'cell:1', label: '30', status: 'active' },
        { id: 'cell:2', label: '20', status: 'swap' },
        { id: 'cell:3', label: '50', status: 'highlight' },
        { id: 'cell:4', label: '40', status: 'done' },
      ],
      links: [],
    };

    expect(() =>
      painter.render(structure, { showIndex: true, showValues: true, zoom: 1 }, 800, 420, pm),
    ).not.toThrow();
    // Sau render, container đã có children (graphics/aura/textContainer + text đã vẽ)
    expect(painter.container.children.length).toBeGreaterThan(0);

    expect(() => painter.update(16.67, pm)).not.toThrow();
    painter.destroy();
    // destroy idempotent: gọi lần 2 không throw (B1/B2)
    expect(() => painter.destroy()).not.toThrow();
    pm.destroy();
  });
});

describe('PixiTreePainter', () => {
  it('renders BST tree structure with branches and node glow pulses', () => {
    const painter = new PixiTreePainter();
    const structure: Structure = {
      kind: 'tree',
      elements: [
        { id: 'node:50', label: '50', status: 'default' },
        { id: 'node:30', label: '30', status: 'active' },
        { id: 'node:70', label: '70', status: 'done' },
      ],
      links: [
        { from: 'node:50', to: 'node:30', label: 'L' },
        { from: 'node:50', to: 'node:70', label: 'R' },
      ],
    };

    expect(() =>
      painter.render(structure, { showIndex: true, showValues: true, zoom: 1 }, 800, 420),
    ).not.toThrow();
    expect(() => painter.update(16.67)).not.toThrow();
    expect(painter.container.children.length).toBeGreaterThan(0);
    painter.destroy();
    expect(() => painter.destroy()).not.toThrow();
  });
});

describe('PixiGraphPainter', () => {
  it('renders graph vertices with laser bullet triggers and ripples', () => {
    const painter = new PixiGraphPainter();
    const pm = new ParticleManager();
    const structure: Structure = {
      kind: 'graph',
      elements: [
        { id: 'node:0', label: 'A', status: 'done' },
        { id: 'node:1', label: 'B', status: 'active' },
        { id: 'node:2', label: 'C', status: 'default' },
      ],
      links: [
        { from: 'node:0', to: 'node:1', label: 'w=4', status: 'done' },
        { from: 'node:1', to: 'node:2', label: 'w=2', status: 'active' },
      ],
    };

    expect(() =>
      painter.render(structure, { showIndex: true, showValues: true, zoom: 1 }, 800, 420, pm),
    ).not.toThrow();
    expect(() => painter.update(16.67)).not.toThrow();
    expect(painter.container.children.length).toBeGreaterThan(0);
    painter.destroy();
    expect(() => painter.destroy()).not.toThrow();
    pm.destroy();
  });
});

describe('PixiLinearPainter', () => {
  it('renders stack, queue, and linked list with spring bounce and dissolve effects', () => {
    const painter = new PixiLinearPainter();
    const pm = new ParticleManager();

    const stackStructure: Structure = {
      kind: 'stack',
      elements: [
        { id: 'cell:0', label: '5', status: 'done' },
        { id: 'cell:1', label: '8', status: 'active' },
      ],
      links: [],
    };

    expect(() =>
      painter.render(stackStructure, { showIndex: true, showValues: true, zoom: 1 }, 800, 420, pm),
    ).not.toThrow();
    expect(() => painter.update(16.67)).not.toThrow();

    const listStructure: Structure = {
      kind: 'linkedlist',
      elements: [
        { id: 'node:0', label: '10', status: 'default' },
        { id: 'node:1', label: '20', status: 'done' },
      ],
      links: [{ from: 'node:0', to: 'node:1', label: 'next' }],
    };

    expect(() =>
      painter.render(listStructure, { showIndex: true, showValues: true, zoom: 1 }, 800, 420, pm),
    ).not.toThrow();
    expect(painter.container.children.length).toBeGreaterThan(0);

    painter.destroy();
    expect(() => painter.destroy()).not.toThrow();
    pm.destroy();
  });
});
