// features/visual-shell/buildFrames.ts — B3/D6b: sinh SortFrame[] từ catalog key (engine registry)
// Pipeline: catalog key → engine generator (Step[]) → LegacyStepAdapter → SortFrame[] (sandbox renderer).
import { getSimulation } from '@/engines/registry';
import '@/engines/catalog'; // side-effect: đăng ký 44 generator (GP-T4 cần sort.bubble)
import { defaultInput } from '@/engines/generators/helpers';
import { catalogKeyToSortAlgorithm } from '../algorithm-sandbox/helpers/catalogKeyMap';
import { legacyStepsToSortFrames } from './adapters/legacyStepAdapter';
import type { SortFrame } from '../algorithm-sandbox/types/sorting.types';

/**
 * Sinh SortFrame[] từ 1 key catalog sort.* (VD 'sort.bubble') bằng engine generator THẬT.
 * Trả null nếu:
 *  - key không thuộc 6 thuật toán có renderer sandbox (radix/counting/bucket — backlog D6),
 *  - hoặc generator chưa đăng ký.
 */
export function buildSortFramesFromCatalogKey(key: string): SortFrame[] | null {
  const algorithm = catalogKeyToSortAlgorithm(key);
  if (!algorithm) return null;
  const generator = getSimulation(key);
  if (!generator) return null;
  const steps = generator.generate(defaultInput(generator));
  return legacyStepsToSortFrames(steps, algorithm, { algorithmKey: key });
}
