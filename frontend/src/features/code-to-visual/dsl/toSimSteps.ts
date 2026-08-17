// features/code-to-visual/dsl/toSimSteps.ts — TraceEvent[] -> engine Step[]
// Bắc cầu sang simulation store/canvas: tái sử dụng Structure/Step type + arrayStructure.
// Frame shape mượn reference: {stepId, activeLine, explanation, dataState, highlights, variables}.
import type { Element, ElementStatus, Step, Structure } from '@/engines/core/types';
import { arrayStructure } from '@/engines/generators/helpers';
import type { TraceEvent } from './types';

const STATUS_AXIS: ElementStatus = 'active';
const STATUS_SWAP: ElementStatus = 'swap';
const STATUS_PEEK: ElementStatus = 'highlight';

/** Enum status theo thao tác — trung thực với channel highlight (compare/swap/assign). */
function statusFor(event: TraceEvent, idx: number): ElementStatus {
  const highlighted = event.highlightedIndices ?? [];
  if (!highlighted.includes(idx)) {
    // Các ô còn lại: muted nếu vượt kích thước thực của structure hiển thị (stack/queue).
    return 'default';
  }
  if (event.operation === 'swap') return STATUS_SWAP;
  if (event.operation === 'push' || event.operation === 'enqueue' || event.operation === 'set') return STATUS_AXIS;
  return STATUS_PEEK;
}

function linearStructure(kind: 'stack' | 'queue', values: number[], capacity: number, event: TraceEvent): Structure {
  const elements: Element[] = [];
  for (let i = 0; i < capacity; i++) {
    const filled = i < values.length;
    elements.push({
      id: 'cell:' + i,
      label: filled ? String(values[i]) : '—',
      status: filled ? statusFor(event, i) : 'muted',
      group: kind,
      meta: { empty: !filled },
    });
  }
  return { kind, elements, links: [] };
}

function capacityFor(values: number[]): number {
  return Math.max(6, values.length + 2);
}

/** TraceEvent[] -> Step[] — mỗi event 1 step; pseudocodeLine = event.line (carry-through). */
export function eventsToSteps(events: TraceEvent[]): Step[] {
  const steps: Step[] = [];
  const stats = { comparisons: 0, swaps: 0, writes: 0 };

  for (const event of events) {
    let structure: Structure;
    if (event.structure === 'array') {
      const statusMap: Record<number, ElementStatus> = {};
      for (const idx of event.highlightedIndices ?? []) statusMap[idx] = statusFor(event, idx);
      structure = arrayStructure(event.state, statusMap);
    } else {
      const kind = event.structure; // 'stack' | 'queue'
      structure = linearStructure(kind, event.state, capacityFor(event.state), event);
    }

    if (event.operation === 'swap') stats.swaps += 1;
    if (event.operation === 'push' || event.operation === 'pop' || event.operation === 'set'
      || event.operation === 'enqueue' || event.operation === 'dequeue') {
      stats.writes += 1;
    }

    // Variables hiển thị trong PseudocodePanel
    const variables: Record<string, string | number | boolean | null> = {
      structure: event.structure,
      size: event.state.length,
      operation: event.operation,
    };
    if (event.state.length > 0) {
      variables.topOrFront = event.structure === 'stack' ? event.state[event.state.length - 1] : event.state[0];
    }
    if ((event.highlightedIndices ?? []).length > 0) {
      variables.actives = (event.highlightedIndices ?? []).join(',');
    }

    steps.push({
      index: steps.length,
      structure,
      explanation: event.explanation,
      pseudocodeLine: event.line,
      highlights: (event.highlightedIndices ?? []).map((idx) => 'cell:' + idx),
      annotations: [event.structure + '.' + event.operation],
      variables,
      stats: { ...stats },
      version: 1,
    });
  }

  return steps;
}
