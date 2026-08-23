import { describe, it, expect } from 'vitest';
import { createBubbleGenerator } from '@/engines/generators/sort/bubble';
import { defaultInput } from '@/engines/generators/helpers';
import type { Step, Structure, Element, ElementStatus } from '@/engines/core/types';
import {
  legacyStepsToSortFrames,
  legacyStepsToSharedFrames,
  legacyStepsToSharedArrayFrames,
  parseElementValue,
  deriveVisualStatus,
} from '../adapters/legacyStepAdapter';
import type { SortFrame } from '../../algorithm-sandbox/types/sorting.types';

function arrayStep(opts: {
  index: number;
  values: number[];
  statuses?: Record<number, ElementStatus>;
  explanation?: string;
  pseudocodeLine?: number;
}): Step {
  const elements: Element[] = opts.values.map((v, i) => ({
    id: 'cell:' + i,
    label: String(v),
    status: opts.statuses?.[i] ?? 'default',
  }));
  const structure: Structure = { kind: 'array', elements, links: [] };
  return {
    index: opts.index,
    structure,
    explanation: opts.explanation ?? 'bước ' + opts.index,
    pseudocodeLine: opts.pseudocodeLine ?? 1,
    highlights: [],
    annotations: [],
    variables: {},
    stats: { comparisons: 0, swaps: 0, writes: 0 },
    version: 1,
  };
}

describe('LegacyStepAdapter — parseElementValue', () => {
  it('parse label số đơn', () => {
    expect(parseElementValue('7')).toBe(7);
    expect(parseElementValue('  -4')).toBe(-4);
  });
  it('parse số cuối trong label phức tạp', () => {
    expect(parseElementValue('d[2]=9')).toBe(9);
  });
  it('trả null khi không có số', () => {
    expect(parseElementValue('null')).toBeNull();
    expect(parseElementValue('')).toBeNull();
  });
});

describe('LegacyStepAdapter — Step[] → SortFrame[]', () => {
  it('chuyển structure.elements → arrayState + indices theo status', () => {
    const steps: Step[] = [
      arrayStep({
        index: 0,
        values: [5, 3, 8],
        statuses: { 0: 'active', 1: 'active' },
        explanation: 'so sánh 5 và 3',
      }),
      arrayStep({
        index: 1,
        values: [3, 5, 8],
        statuses: { 0: 'swap', 1: 'swap' },
        explanation: 'hoán vị 0↔1',
      }),
      arrayStep({
        index: 2,
        values: [3, 5, 8],
        statuses: { 2: 'done' },
        explanation: 'phần tử cuối yên vị',
      }),
    ];

    const frames = legacyStepsToSortFrames(steps, 'bubble');
    expect(frames).toHaveLength(3);

    expect(frames[0].arrayState).toEqual([5, 3, 8]);
    expect(frames[0].comparingIndices).toEqual([0, 1]);
    expect(frames[0].swappedIndices).toBeNull();
    expect(frames[0].sortedIndices).toEqual([]);
    expect(frames[0].description).toBe('so sánh 5 và 3');
    expect(frames[0].algorithm).toBe('bubble');

    expect(frames[1].arrayState).toEqual([3, 5, 8]);
    expect(frames[1].swappedIndices).toEqual([0, 1]);
    expect(frames[1].comparingIndices).toBeNull();

    expect(frames[2].sortedIndices).toEqual([2]);
  });

  it('enrich id (arrayStateWithIds) cho renderer sandbox', () => {
    const frames = legacyStepsToSortFrames([arrayStep({ index: 0, values: [1, 2] })], 'bubble');
    expect(frames[0].arrayStateWithIds).toBeDefined();
    expect(frames[0].arrayStateWithIds).toEqual([
      { id: 0, value: 1 },
      { id: 1, value: 2 },
    ]);
  });

  it('sử dụng Step thật từ engine bubble generator', () => {
    const generator = createBubbleGenerator();
    const steps = generator.generate(defaultInput(generator));
    expect(steps.length).toBeGreaterThan(0);

    const frames = legacyStepsToSortFrames(steps, 'bubble');
    expect(frames).toHaveLength(steps.length);
    expect(frames[0].arrayState.length).toBeGreaterThan(0);
    // Bước cuối (khi generator này hoàn tất) phải có sortedIndices full
    const sortedFromSteps = steps
      .map((s, idx) => ({ idx, local: s.structure.elements }))
      .filter(({ local }) => local.every((el) => el.status === 'done'))
      .map(({ idx }) => idx);
    for (const idx of sortedFromSteps) {
      const f = frames[idx] as SortFrame;
      expect(f.sortedIndices.length).toBe(frames[0].arrayState.length);
    }
    // Mảng frame không mutated schema
    expect(frames[0].stepIndex).toBe(steps[0].index);
  });
});

describe('LegacyStepAdapter — Step[] → SharedVisualFrame[]', () => {
  it('map đủ contract B2', () => {
    const steps = [arrayStep({ index: 0, values: [9, 4], statuses: { 0: 'active' }, pseudocodeLine: 5 })];
    const frames = legacyStepsToSharedFrames(steps, 'sort.bubble');
    expect(frames).toHaveLength(1);
    const f = frames[0];
    expect(f.algorithmKey).toBe('sort.bubble');
    expect(f.stepIndex).toBe(0);
    expect(f.totalSteps).toBe(1);
    expect(f.description).toBe('bước 0');
    expect(f.highlights).toEqual(['cell:0']);
    expect(f.pseudocodeLine).toBe(5);
    expect(f.status).toBe('comparing');
    expect((f.data as Structure).kind).toBe('array');
  });

  it('deriveVisualStatus: swap → swapping, all done → done', () => {
    const swapStep = arrayStep({ index: 0, values: [1], statuses: { 0: 'swap' } });
    expect(deriveVisualStatus(swapStep)).toBe('swapping');

    const doneStep = arrayStep({ index: 0, values: [1], statuses: { 0: 'done' } });
    expect(deriveVisualStatus(doneStep)).toBe('done');
  });

  it('legacyStepsToSharedArrayFrames nén data thành mảng số', () => {
    const steps = [arrayStep({ index: 0, values: [4, 7, 1] })];
    const frames = legacyStepsToSharedArrayFrames(steps, 'sort.bubble');
    expect(frames[0].data).toEqual([4, 7, 1]);
  });
});
