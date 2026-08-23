// features/algorithm-sandbox/helpers/__tests__/catalogKeyMap.spec.ts — D6
import { describe, it, expect } from 'vitest';
import { CATALOG } from '@/engines/catalog';
import {
  SORT_ALGORITHM_CATALOG_KEYS,
  catalogKeyToSortAlgorithm,
  sortAlgorithmToCatalogKey,
} from '../catalogKeyMap';

describe('D6 — map sandbox SortAlgorithm ↔ catalog key sort.*', () => {
  it('mọi key map đều tồn tại trong CATALOG 44-key', () => {
    const catalogKeys = new Set(CATALOG.map((c) => c.key));
    for (const key of Object.values(SORT_ALGORITHM_CATALOG_KEYS)) {
      expect(catalogKeys.has(key), 'thiếu key catalog: ' + key).toBe(true);
    }
  });

  it('map 6 thuật toán có key thật (bubble/selection/insertion/quick/merge/heap)', () => {
    expect(sortAlgorithmToCatalogKey('bubble')).toBe('sort.bubble');
    expect(sortAlgorithmToCatalogKey('selection')).toBe('sort.selection');
    expect(sortAlgorithmToCatalogKey('insertion')).toBe('sort.insertion');
    expect(sortAlgorithmToCatalogKey('quick')).toBe('sort.quick');
    expect(sortAlgorithmToCatalogKey('merge')).toBe('sort.merge');
    expect(sortAlgorithmToCatalogKey('heap')).toBe('sort.heap');
  });

  it('radix/counting/bucket CHƯA có catalog key (backlog D6 — không tự thêm vì CI 44 key)', () => {
    expect(sortAlgorithmToCatalogKey('radix')).toBeNull();
    expect(sortAlgorithmToCatalogKey('counting')).toBeNull();
    expect(sortAlgorithmToCatalogKey('bucket')).toBeNull();
  });

  it('catalogKeyToSortAlgorithm đảo ngược đúng', () => {
    expect(catalogKeyToSortAlgorithm('sort.bubble')).toBe('bubble');
    expect(catalogKeyToSortAlgorithm('sort.quick')).toBe('quick');
    expect(catalogKeyToSortAlgorithm('search.linear')).toBeNull();
    expect(catalogKeyToSortAlgorithm('structure.array')).toBeNull();
  });
});
