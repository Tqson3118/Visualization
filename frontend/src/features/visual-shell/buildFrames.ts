// features/visual-shell/buildFrames.ts — B3/D6b: sinh SortFrame[] từ catalog key (engine registry)
// Pipeline: catalog key → engine generator (Step[]) → LegacyStepAdapter → SortFrame[] (sandbox renderer).
import { getSimulation } from '@/engines/registry';
import '@/engines/catalog'; // side-effect: đăng ký 44 generator (GP-T4 cần sort.bubble)
import { defaultInput } from '@/engines/generators/helpers';
import { catalogKeyToSortAlgorithm } from './helpers/catalogKeyMap';
import { legacyStepsToSortFrames } from './adapters/legacyStepAdapter';
import type { Step } from '@/engines/core/types';
import type { SortFrame } from './types/sorting.types';

/**
 * Sinh frames hoặc Step[] từ 1 key catalog bất kỳ bằng engine generator THẬT.
 */
export function buildFramesFromCatalogKey(key: string, input?: any): SortFrame[] | Step[] | null {
  const generator = getSimulation(key);
  if (!generator) return null;
  const inVal = input ?? defaultInput(generator);
  const steps = generator.generate(inVal);
  const algorithm = catalogKeyToSortAlgorithm(key);
  if (algorithm) {
    return legacyStepsToSortFrames(steps, algorithm, { algorithmKey: key });
  }
  return steps as any;
}

/**
 * Sinh SortFrame[] từ 1 key catalog sort.* (VD 'sort.bubble') bằng engine generator THẬT.
 * Trả null nếu generator chưa đăng ký.
 */
export function buildSortFramesFromCatalogKey(key: string, input?: any): SortFrame[] | null {
  const frames = buildFramesFromCatalogKey(key, input);
  return Array.isArray(frames) ? (frames as SortFrame[]) : null;
}

/**
 * SortFrame[] từ steps engine ĐÃ generate (dùng trong SimulatorView khi steps đã sẵn
 * qua useSimulation — tránh generate lại). Key phải thuộc 6 sort.* (renderer sandbox).
 * Trả [] nếu key không có renderer sandbox.
 */
export function stepsToSortFrames(steps: Step[], key: string): SortFrame[] {
  const algorithm = catalogKeyToSortAlgorithm(key);
  if (!algorithm) return [];
  return legacyStepsToSortFrames(steps, algorithm, { algorithmKey: key });
}
