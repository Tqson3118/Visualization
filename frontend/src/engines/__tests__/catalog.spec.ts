// engines/__tests__/catalog.spec.ts — ĐỒNG BỘ danh mục với shared/simulation-catalog.json
// (SDD §4.5: CI so sánh 2 danh sách key → khác → fail build; test này là cổng CI tương đương).

import { existsSync, readFileSync } from 'node:fs';
import { resolve } from 'node:path';

import { describe, expect, it } from 'vitest';

import { CATALOG, stubSimulation } from '../catalog';

/**
 * Leo ngược từ cwd tìm shared/simulation-catalog.json (không phụ thuộc nơi vitest khởi chạy —
 * import.meta.url bị Vitest 4 inline thành data-URL nên không dùng được).
 */
function findSharedCatalog(startDir: string): string {
  let dir = startDir;
  for (let i = 0; i < 6; i++) {
    const candidate = resolve(dir, 'shared/simulation-catalog.json');
    if (existsSync(candidate)) return candidate;
    dir = resolve(dir, '..');
  }
  throw new Error('Không tìm thấy shared/simulation-catalog.json');
}

const sharedPath = findSharedCatalog(process.cwd());
// File shared có UTF-8 BOM — strip trước khi parse.
const SHARED = JSON.parse(readFileSync(sharedPath, 'utf-8').replace(/^\uFEFF/, '')) as Array<{
  key: string;
  title: string;
  dataStructure: string;
  category: string;
  level: string;
  complexity: { best: string; average: string; worst: string; space: string };
}>;

describe('engines/catalog — khớp shared/simulation-catalog.json', () => {
  it('danh sách key KHỚP 100% (cùng tập hợp, không thừa/thiếu)', () => {
    const catalogKeys = CATALOG.map((m) => m.key).sort();
    const sharedKeys = SHARED.map((m) => m.key).sort();
    expect(catalogKeys).toEqual(sharedKeys);
  });

  it('số lượng đúng 44 mô phỏng', () => {
    expect(CATALOG.length).toBe(44);
  });

  it('key theo định dạng {nhóm}.{tên} và duy nhất', () => {
    const keys = CATALOG.map((m) => m.key);
    expect(new Set(keys).size).toBe(keys.length);
    for (const key of keys) {
      expect(key).toMatch(/^[a-z]+\.[a-z-]+$/);
    }
  });

  it('metadata (title/category/level/complexity) khớp file shared', () => {
    for (const meta of CATALOG) {
      const shared = SHARED.find((s) => s.key === meta.key);
      expect(shared).toBeDefined();
      expect(meta.title).toBe(shared?.title);
      expect(meta.dataStructure).toBe(shared?.dataStructure);
      expect(meta.category).toBe(shared?.category);
      expect(meta.level).toBe(shared?.level);
      expect(meta.complexity).toEqual(shared?.complexity);
    }
  });

  it('stub generator đủ type SimulationGenerator, generate() chưa implement', () => {
    const gen = stubSimulation(CATALOG[0]);
    expect(gen.key).toBe(CATALOG[0].key);
    expect(gen.validate({ kind: 'array', data: [1] })).toEqual({ ok: true, errors: [] });
    expect(() => gen.generate({ kind: 'array', data: [1] })).toThrow(/implement in task tiếp/);
  });
});
