// visualizer/dslToFrames.spec.ts — Adapter: Code-to-Visual DSL trace → SharedVisualFrame[].
import { describe, expect, it } from 'vitest';

import type { TraceEvent } from '@/features/code-to-visual/dsl/types';
import { dslTraceToFrames } from './dslToFrames';
import type { Structure } from '@/engines/core/types';

describe('dslTraceToFrames', () => {
  it('chuyển TraceEvent[] qua eventsToSteps → frames', () => {
    const events: TraceEvent[] = [
      { step: 0, line: 1, structure: 'array', operation: 'create', state: [3, 1, 2], explanation: 'Tạo mảng [3,1,2]' },
      { step: 1, line: 2, structure: 'array', operation: 'swap', state: [1, 3, 2], highlightedIndices: [0, 1], explanation: 'Hoán đổi a[0] và a[1]' },
    ];
    const frames = dslTraceToFrames(events, 'code-to-visual');
    expect(frames).toHaveLength(2);
    expect(frames[0].status).toBe('idle');
    expect(frames[1].status).toBe('swapping');
    expect(frames[1].description).toContain('Hoán đổi');
    expect(frames[1].pseudocodeLine).toBe(2);
  });

  it('mảng rỗng → frames rỗng', () => {
    expect(dslTraceToFrames([], 'code-to-visual')).toHaveLength(0);
  });

  it('stack structure không làm vỡ contract (fallback renderer an toàn)', () => {
    const events: TraceEvent[] = [
      { step: 0, line: 1, structure: 'stack', operation: 'push', state: [5], highlightedIndices: [0], explanation: 'Đẩy 5 vào stack' },
    ];
    const frames = dslTraceToFrames(events, 'code-to-visual');
    const data = frames[0].data as Structure;
    expect(data.kind).toBe('stack');
    expect(frames[0].status).toBe('comparing');
  });
});
