// features/visual-shell/adapters/legacyStepAdapter.ts
// LegacyStepAdapter — B2: chuyển engine Step[] sang contract chung.
//   Step[] → SortFrame[]        (renderer sandbox: SortingVisualizerDispatcher)
//   Step[] → SharedVisualFrame[] (contract hợp nhất cho mọi renderer)
// Mapping: Step.structure.elements → arrayState/indices (element.status → trạng thái).
import type { ElementStatus, Step } from '@/engines/core/types';
import type { SortAlgorithm, SortFrame } from '../types/sorting.types';
import { enrichFramesWithIds } from '../helpers/sortingIdEnricher';
import type { SharedVisualFrame, SharedVisualStatus } from '../types/sharedVisualFrame';

/** Parse giá trị số từ label của element ('7', có thể là 'd[2]=9' mang giá trị cuối). */
export function parseElementValue(label: string): number | null {
  const trimmed = label.trim();
  if (!trimmed) return null;
  // Lấy SỐ CUỐI trong label (VD 'd[2]=9' → 9, 'cell:3' → 3) — không nhầm với index.
  const matches = trimmed.match(/-?\d+(?:\.\d+)?/g);
  if (!matches || matches.length === 0) return null;
  const n = Number(matches[matches.length - 1]);
  return Number.isFinite(n) ? n : null;
}

export interface LegacyStepAdapterOptions {
  /** Key catalog đích (VD 'sort.bubble'); mặc định tự suy 'sort.<algorithm>'. */
  algorithmKey?: string;
  /** Ghép id (arrayStateWithIds) cho renderer sandbox ngay trong adapter. Default true. */
  enrichIds?: boolean;
}

/** Vị trí 2-index đầu tiên từ danh sách index thoả điều kiện (null nếu < 2). */
function pairFrom(indices: number[]): [number, number] | null {
  if (indices.length < 2) return null;
  return [indices[0], indices[1]];
}

/** Lọc variables thích hợp hiển thị (bỏ null/boolean). */
function displayVariables(step: Step): Record<string, string | number> {
  const vars: Record<string, string | number> = {};
  for (const [k, v] of Object.entries(step.variables ?? {})) {
    if (v === null || typeof v === 'boolean') continue;
    vars[k] = v;
  }
  return vars;
}

/**
 * Step[] → SortFrame[] (renderer sandbox).
 * - arrayState       = label của element theo đúng index
 * - comparingIndices = element status 'active'/'highlight' (2 đầu tiên)
 * - swappedIndices   = element status 'swap' (2 đầu tiên)
 * - sortedIndices    = element status 'done'
 */
export function legacyStepsToSortFrames(
  steps: Step[],
  algorithm: SortAlgorithm,
  options: LegacyStepAdapterOptions = {},
): SortFrame[] {
  const frames: SortFrame[] = steps.map((step) => {
    const elements = step.structure?.elements ?? [];
    const arrayState: number[] = [];
    const comparing: number[] = [];
    const swapping: number[] = [];
    const sorted: number[] = [];

    elements.forEach((el, idx) => {
      const val = parseElementValue(el.label);
      arrayState[idx] = val ?? 0;
      const status: ElementStatus = el.status;
      if (status === 'active' || status === 'highlight') comparing.push(idx);
      else if (status === 'swap') swapping.push(idx);
      else if (status === 'done') sorted.push(idx);
    });

    return {
      stepIndex: step.index,
      arrayState,
      comparingIndices: pairFrom(comparing),
      pivotIndex: null,
      swappedIndices: pairFrom(swapping),
      sortedIndices: sorted,
      description: step.explanation || '',
      algorithm,
      variables: displayVariables(step),
    };
  });

  if (options.enrichIds !== false) enrichFramesWithIds(frames);
  return frames;
}

/** Suy trạng thái hợp nhất (B2 status) từ Step. */
export function deriveVisualStatus(step: Step): SharedVisualStatus {
  const elements = step.structure?.elements ?? [];
  if (elements.some((el) => el.status === 'swap')) return 'swapping';
  if (elements.some((el) => el.status === 'active' || el.status === 'highlight')) return 'comparing';
  if (elements.length > 0 && elements.every((el) => el.status === 'done')) return 'done';
  return 'running';
}

/** Step[] → SharedVisualFrame[] (contract hợp nhất B2). */
export function legacyStepsToSharedFrames(
  steps: Step[],
  algorithmKey: string,
): SharedVisualFrame[] {
  const total = steps.length;
  return steps.map((step, idx) => {
    const elements = step.structure?.elements ?? [];
    const highlights = elements
      .filter((el) => el.status !== 'default' && el.status !== 'muted')
      .map((el) => el.id);
    return {
      algorithmKey,
      stepIndex: idx,
      totalSteps: total,
      description: step.explanation || '',
      highlights,
      data: step.structure ?? null,
      pseudocodeLine: step.pseudocodeLine,
      status: deriveVisualStatus(step),
    };
  });
}

/** Tiện ích: Step[] → SharedVisualFrame[] với dữ liệu được nén thành mảng số. */
export function legacyStepsToSharedArrayFrames(
  steps: Step[],
  algorithmKey: string,
): SharedVisualFrame[] {
  return legacyStepsToSharedFrames(steps, algorithmKey).map((frame, i) => {
    const step = steps[i];
    const elements = step.structure?.elements ?? [];
    const arrayState: number[] = elements.map((el) => parseElementValue(el.label) ?? 0);
    return { ...frame, data: arrayState };
  });
}
