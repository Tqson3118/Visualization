// engines/__tests__/catalog.spec.ts — ĐỒNG BỘ danh mục với shared/simulation-catalog.json
// (SDD §4.5: CI so sánh 2 danh sách key → khác → fail build; test này là cổng CI tương đương).
//
// Test generator thật (SDD §4.8/§4.9A): mọi key có generator đăng ký + generate hợp lệ,
// GOLDEN TEST bubble [3,1,2] (21 bước, c/s/w tích lũy, trạng thái phần tử), và các mốc
// nhanh của nhóm tìm kiếm / stack / đồ thị.

import { existsSync, readFileSync } from 'node:fs';
import { resolve } from 'node:path';

import { describe, expect, it } from 'vitest';

import { CATALOG } from '../catalog';
import type { InputConfig, InputSchema, SimulationGenerator, Step } from '../core/types';
import { getSimulation, listSimulations } from '../registry';

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

/** Dựng InputConfig mặc định từ schema (dùng default của từng field). */
function defaultInput(gen: SimulationGenerator): InputConfig {
  const data: Record<string, unknown> = {};
  for (const f of gen.inputSchema.fields) data[f.name] = f.default;
  return { kind: gen.inputSchema.kind, data };
}

/** Kiểm tra cấu trúc chuẩn của một bước (SDD §4.2/§4.3). */
function expectWellFormedStep(step: Step): void {
  expect(step.index).toBeGreaterThanOrEqual(0);
  expect(step.version).toBe(1);
  expect(step.explanation.length).toBeGreaterThan(0);
  expect(step.pseudocodeLine).toBeGreaterThanOrEqual(1);
  expect(Array.isArray(step.highlights)).toBe(true);
  expect(Array.isArray(step.annotations)).toBe(true);
  expect(step.stats).toMatchObject({ comparisons: expect.any(Number), swaps: expect.any(Number), writes: expect.any(Number) });
  expect(step.structure.kind.length).toBeGreaterThan(0);
  expect(Array.isArray(step.structure.elements)).toBe(true);
  expect(Array.isArray(step.structure.links)).toBe(true);
  for (const el of step.structure.elements) {
    expect(el.id.length).toBeGreaterThan(0);
    expect(el.status).toBeTruthy();
  }
}

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

  it('mọi key trong CATALOG có generator THẬT: schema, pseudocode, generate, validate', () => {
    for (const meta of CATALOG) {
      const gen = getSimulation(meta.key);
      expect(gen, `generator cho ${meta.key} phải được đăng ký`).toBeDefined();
      expect(gen!.key).toBe(meta.key);
      expect(gen!.inputSchema.fields.length, `${meta.key}: inputSchema.fields > 0`).toBeGreaterThan(0);
      expect(gen!.pseudocode.length, `${meta.key}: pseudocode ≥ 3 dòng`).toBeGreaterThanOrEqual(3);

      const input = defaultInput(gen!);
      const validation = gen!.validate(input);
      expect(validation.ok, `${meta.key}: validate mặc định hợp lệ — ${validation.errors.join('; ')}`).toBe(true);

      const steps = gen!.generate(input);
      expect(steps.length, `${meta.key}: generate với input mặc định cho ≥ 2 bước`).toBeGreaterThanOrEqual(2);
      // Bước 0 = khởi tạo (SDD §4.3): line 1, stats 0/0/0.
      expect(steps[0].pseudocodeLine, `${meta.key}: bước 0 pseudocodeLine = 1`).toBe(1);
      expect(steps[0].stats).toEqual({ comparisons: 0, swaps: 0, writes: 0 });
      expect(steps[0].explanation).toContain('Bắt đầu');
      // Bước cuối = hoàn tất.
      expect(steps[steps.length - 1].explanation).toContain('Kết thúc');
      // Stats tích lũy không giảm (SDD §4.1).
      for (let i = 1; i < steps.length; i++) {
        expect(steps[i].stats.comparisons).toBeGreaterThanOrEqual(steps[i - 1].stats.comparisons);
        expect(steps[i].stats.swaps).toBeGreaterThanOrEqual(steps[i - 1].stats.swaps);
        expect(steps[i].stats.writes).toBeGreaterThanOrEqual(steps[i - 1].stats.writes);
        expect(steps[i].index).toBe(i);
        expectWellFormedStep(steps[i]);
      }
      expectWellFormedStep(steps[0]);
    }
  });

  it('registry đăng ký đủ 44 generator (không thiếu, không thừa)', () => {
    const list = listSimulations();
    expect(list.length).toBe(44);
    const keys = list.map((g) => g.key).sort();
    expect(keys).toEqual(CATALOG.map((m) => m.key).sort());
  });

  it('GOLDEN TEST bubble [3,1,2]: 21 bước, c=4, s=2, bước 2 active, bước 3 swap, cuối toàn done', () => {
    const gen = getSimulation('sort.bubble')!;
    const steps = gen.generate({ kind: 'array', data: { values: [3, 1, 2] } });

    // Trace chuẩn SDD §4.9A: 21 bước, comparisons cuối = n(n-1)/2 = 4.
    expect(steps.length).toBe(21);
    const last = steps[steps.length - 1];
    expect(last.stats.comparisons).toBe(4);
    // LƯU Ý: bảng §4.9A ghi swaps=3 nhưng bản thân nó mâu thuẫn (pass 1 dùng giá trị cũ a[1]=3
    // sau khi pass 0 đã xong [1,2,3]); trace trung thực theo SDD §4.0.2 cho swaps=2.
    expect(last.stats.swaps).toBe(2);
    expect(last.explanation).toContain('Kết thúc');

    // Mốc bước 2: so sánh a[0]=3 > a[1]=1 → cell:0 + cell:1 active.
    expect(steps[2].structure.elements[0].status).toBe('active');
    expect(steps[2].structure.elements[1].status).toBe('active');
    // Mốc bước 3: hoán đổi → cell:0 + cell:1 swap.
    expect(steps[3].structure.elements[0].status).toBe('swap');
    expect(steps[3].structure.elements[1].status).toBe('swap');
    // Kết thúc: toàn bộ done.
    for (const el of last.structure.elements) {
      expect(el.status).toBe('done');
    }
    // Giá trị cuối đã sắp xếp.
    expect(last.structure.elements.map((e) => e.label)).toEqual(['1', '2', '3']);
  });

  it('binary search: tìm thấy / không thấy / tự sắp xếp khi dữ liệu chưa sắp xếp', () => {
    const gen = getSimulation('search.binary')!;
    const found = gen.generate({ kind: 'array', data: { values: [2, 5, 8, 12, 19, 23], target: 12 } });
    expect(found[found.length - 1].explanation).toMatch(/tìm thấy target=12/);
    expect(found[found.length - 1].stats.comparisons).toBeGreaterThanOrEqual(2);

    const missed = gen.generate({ kind: 'array', data: { values: [2, 5, 8, 12, 19, 23], target: 7 } });
    expect(missed[missed.length - 1].explanation).toMatch(/không tìm thấy target=7/);
    // Không thấy → toàn mảng muted (SDD §4.6.2).
    expect(missed[missed.length - 1].structure.elements.every((e) => e.status === 'muted')).toBe(true);

    // Dữ liệu chưa sắp xếp → TỰ SẮP XẾP kèm banner, không lỗi (SDD §4.14).
    const autoSorted = gen.generate({ kind: 'array', data: { values: [23, 2, 8, 12, 5, 19], target: 5 } });
    expect(autoSorted.some((s) => s.annotations.some((a) => a.includes('tự sắp xếp')))).toBe(true);
    expect(autoSorted[autoSorted.length - 1].explanation).toMatch(/tìm thấy target=5/);
  });

  it('stack.push: tràn ngăn xếp → có bước error', () => {
    const gen = getSimulation('stack.push')!;
    const steps = gen.generate({ kind: 'linear', data: { operations: ['Push 5', 'Push 3'], capacity: 1 } });
    expect(steps.some((s) => s.structure.elements.some((e) => e.status === 'error'))).toBe(true);
    // Giải thích lỗi cụ thể.
    expect(steps.some((s) => /tràn|đầy/i.test(s.explanation))).toBe(true);
  });

  it('graph.bfs: thứ tự thăm đúng trên path 0→1→2 (order 0,1,2)', () => {
    const gen = getSimulation('graph.bfs')!;
    const steps = gen.generate({
      kind: 'graph',
      data: { preset: 'path', vertices: 3, edges: 2, source: 0, directed: false, weighted: false },
    });
    const firstDone: string[] = [];
    for (const s of steps) {
      for (const e of s.structure.elements) {
        if (e.id.startsWith('node:') && e.status === 'done' && !firstDone.includes(e.label)) {
          firstDone.push(e.label);
        }
      }
    }
    expect(firstDone).toEqual(['0', '1', '2']);
  });
});
