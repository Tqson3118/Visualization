// features/visual-shell/helpers/catalogKeyMap.ts — Map SortAlgorithm ↔ catalog key sort.*
import type { SortAlgorithm } from '../types/sorting.types';

/** Map thuật toán sandbox → key catalog sort.* (chỉ những thuật toán có key thật). */
export const SORT_ALGORITHM_CATALOG_KEYS: Partial<Record<SortAlgorithm, string>> = {
  bubble: 'sort.bubble',
  selection: 'sort.selection',
  insertion: 'sort.insertion',
  quick: 'sort.quick',
  merge: 'sort.merge',
  heap: 'sort.heap',
};

/** Thuật toán sandbox ứng với 1 key catalog sort.* (null nếu key không thuộc 6 thuật toán trên). */
export function catalogKeyToSortAlgorithm(key: string): SortAlgorithm | null {
  for (const [algo, catalogKey] of Object.entries(SORT_ALGORITHM_CATALOG_KEYS)) {
    if (catalogKey === key) return algo as SortAlgorithm;
  }
  return null;
}

/** Key catalog sort.* ứng với 1 thuật toán sandbox (null nếu thuật toán chưa có key). */
export function sortAlgorithmToCatalogKey(algo: SortAlgorithm): string | null {
  return SORT_ALGORITHM_CATALOG_KEYS[algo] ?? null;
}
