// visualizer/dslToFrames.ts — Adapter: Code-to-Visual DSL trace → SharedVisualFrame[].
// Boundary rõ: DSL (features/code-to-visual/dsl) sinh TraceEvent → eventsToSteps → Step[] → frames.
import { eventsToSteps } from '@/features/code-to-visual/dsl/toSimSteps';
import type { TraceEvent } from '@/features/code-to-visual/dsl/types';
import { legacyStepsToFrames } from './stepToFrames';
import type { SharedVisualFrame } from './types';

export function dslTraceToFrames(events: TraceEvent[], algorithmKey = 'code-to-visual'): SharedVisualFrame[] {
  const steps = eventsToSteps(events);
  return legacyStepsToFrames(steps, algorithmKey);
}
