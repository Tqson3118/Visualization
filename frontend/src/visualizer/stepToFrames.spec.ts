// visualizer/stepToFrames.spec.ts — Adapter: legacy engine Step[] → SharedVisualFrame.
import { describe, expect, it } from 'vitest';

import '@/engines/catalog'; // đăng ký 44 generator — dùng sort.bubble thật
import type { ElementStatus, Step, Structure } from '@/engines/core/types';
import { getSimulation } from '@/engines/registry';
import { defaultInputFromSchema } from './helpers';
import { dominantStatus, legacyStepsToFrames, statusFromElement } from './stepToFrames';

const KEY = 'sort.bubble';

function stepsOfBubble(values: number[]): Step[] {
  const gen = getSimulation(KEY);
  if (!gen) throw new Error('missing sort.bubble');
  return gen.generate({ kind: 'array', data: { values, size: values.length, preset: 'custom' } });
}

describe('statusFromElement / dominantStatus', () => {
  it('map ElementStatus → VisualizerStatus', () => {
    expect(statusFromElement('active')).toBe('comparing');
    expect(statusFromElement('swap')).toBe('swapping');
    expect(statusFromElement('highlight')).toBe('visited');
    expect(statusFromElement('done')).toBe('done');
    expect(statusFromElement('error')).toBe('error');
    expect(statusFromElement('default')).toBe('idle');
  });

  it('ưu tiên error > swap > active > highlight > done > idle', () => {
    const st = (statuses: ElementStatus[]): Structure => ({
      kind: 'array',
      elements: statuses.map((s, i) => ({ id: `c:${i}`, label: String(i), status: s })),
      links: [],
    });
    expect(dominantStatus(st(['swap', 'active']))).toBe('swap');
    expect(dominantStatus(st(['active', 'done']))).toBe('active');
    expect(dominantStatus(st(['highlight', 'done']))).toBe('highlight');
    expect(dominantStatus(st(['done', 'done']))).toBe('done');
    expect(dominantStatus(st(['default', 'default']))).toBe('default');
    expect(dominantStatus(null)).toBe('default');
  });
});

describe('legacyStepsToFrames', () => {
  it('giữ nguyên số bước, stepIndex và totalSteps', () => {
    const steps = stepsOfBubble([3, 1, 2]);
    const frames = legacyStepsToFrames(steps, KEY);
    expect(frames).toHaveLength(steps.length);
    expect(frames[0].stepIndex).toBe(0);
    expect(frames[0].totalSteps).toBe(steps.length);
    expect(frames[frames.length - 1].stepIndex).toBe(steps.length - 1);
    expect(frames[0].algorithmKey).toBe(KEY);
  });

  it('mang description + pseudocodeLine + data structure qua frame', () => {
    const steps = stepsOfBubble([3, 1, 2]);
    const frames = legacyStepsToFrames(steps, KEY);
    expect(frames[0].description).toBe(steps[0].explanation);
    expect(frames[0].pseudocodeLine).toBe(steps[0].pseudocodeLine);
    const data = frames[0].data as Structure;
    expect(data.kind).toBe('array');
  });

  it('frame swap → status swapping; kết thúc → done', () => {
    const steps = stepsOfBubble([3, 1, 2]);
    const frames = legacyStepsToFrames(steps, KEY);
    const swapIdx = steps.findIndex((s) => s.structure.elements.some((e) => e.status === 'swap'));
    expect(swapIdx).toBeGreaterThan(0);
    expect(frames[swapIdx].status).toBe('swapping');
    const last = frames[frames.length - 1];
    expect(last.status).toBe('done');
    const data = last.data as Structure;
    expect(data.elements.every((e) => e.status === 'done')).toBe(true);
  });

  it('frame rỗng → mảng rỗng', () => {
    expect(legacyStepsToFrames([], KEY)).toHaveLength(0);
  });
});

describe('defaultInputFromSchema', () => {
  it('sinh InputConfig từ default của schema', () => {
    const gen = getSimulation(KEY);
    if (!gen) throw new Error('missing sort.bubble');
    const input = defaultInputFromSchema(gen);
    expect(input.kind).toBe('array');
    const data = input.data as Record<string, unknown>;
    expect(Array.isArray(data.values)).toBe(true);
    expect(typeof data.size).toBe('number');
  });
});
