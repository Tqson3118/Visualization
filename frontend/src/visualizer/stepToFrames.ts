// visualizer/stepToFrames.ts — Adapter: legacy engine Step[] (engines/core/types) → SharedVisualFrame.
import type { ElementStatus, Step, Structure } from '@/engines/core/types';
import type { SharedVisualFrame, VisualizerStatus } from './types';

export function statusFromElement(s: ElementStatus): VisualizerStatus {
  switch (s) {
    case 'active':
      return 'comparing';
    case 'swap':
      return 'swapping';
    case 'highlight':
      return 'visited';
    case 'done':
      return 'done';
    case 'error':
      return 'error';
    default:
      return 'idle';
  }
}

/** Trạng thái nổi bật của structure (ưu tiên error > swap > active > highlight > done > idle). */
export function dominantStatus(structure: Structure | null): ElementStatus {
  if (!structure || structure.elements.length === 0) return 'default';
  const has = (s: ElementStatus) => structure.elements.some((el) => el.status === s);
  if (has('error')) return 'error';
  if (has('swap')) return 'swap';
  if (has('active')) return 'active';
  if (has('highlight')) return 'highlight';
  if (structure.elements.every((el) => el.status === 'done' || el.status === 'muted')) return 'done';
  return 'default';
}

export function structureToFrame(structure: Structure, i: number, total: number, algorithmKey: string, base: Pick<Step, 'explanation' | 'pseudocodeLine' | 'highlights' | 'annotations' | 'variables'>): SharedVisualFrame {
  return {
    algorithmKey,
    stepIndex: i,
    totalSteps: total,
    description: base.explanation,
    highlights: base.highlights,
    data: structure,
    pseudocodeLine: base.pseudocodeLine,
    status: statusFromElement(dominantStatus(structure)),
    annotations: base.annotations,
    variables: base.variables,
  };
}

/** Legacy Step[] → SharedVisualFrame[] (không đổi engine/simulator cũ). */
export function legacyStepsToFrames(steps: Step[], algorithmKey: string): SharedVisualFrame[] {
  const total = steps.length;
  return steps.map((s, i) =>
    structureToFrame(s.structure, i, total, algorithmKey, {
      explanation: s.explanation,
      pseudocodeLine: s.pseudocodeLine,
      highlights: s.highlights,
      annotations: s.annotations,
      variables: s.variables,
    }),
  );
}
