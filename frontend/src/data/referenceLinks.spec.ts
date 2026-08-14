// data/referenceLinks.spec.ts — REFERENCE_LINKS phủ 100% key catalog (44 key).
// Thiếu/thừa key hoặc URL hỏng → fail (giữ đồng bộ với engines/catalog.ts).
// Cấu trúc merged (PR #23 + #22): Record<string, { wikipedia?, geeksforgeeks?, note? }>.

import { describe, expect, it } from 'vitest';

import { CATALOG } from '@/engines/catalog';
import { REFERENCE_LINKS } from './referenceLinks';

describe('data/referenceLinks — phủ 44 key catalog', () => {
  it('mọi key trong CATALOG đều có entry (không thiếu key)', () => {
    expect(Object.keys(REFERENCE_LINKS).length).toBe(CATALOG.length);
    for (const meta of CATALOG) {
      expect(REFERENCE_LINKS[meta.key], `thiếu REFERENCE_LINKS['${meta.key}']`).toBeDefined();
    }
  });

  it('không có key lạ ngoài CATALOG', () => {
    const catalogKeys = new Set(CATALOG.map((m) => m.key));
    for (const key of Object.keys(REFERENCE_LINKS)) {
      expect(catalogKeys.has(key), `key lạ không thuộc CATALOG: ${key}`).toBe(true);
    }
  });

  it('mỗi key có ≥ 1 link (wikipedia/geeksforgeeks), url hợp lệ (https)', () => {
    for (const [key, ref] of Object.entries(REFERENCE_LINKS)) {
      const links = [ref.wikipedia, ref.geeksforgeeks].filter((u): u is string => Boolean(u));
      expect(links.length, `${key}: tối thiểu 1 link`).toBeGreaterThanOrEqual(1);
      for (const url of links) {
        let parsed: URL;
        try {
          parsed = new URL(url);
        } catch {
          throw new Error(`${key}: URL không hợp lệ — ${url}`);
        }
        expect(parsed.protocol, `${key}: ${url} phải là https`).toBe('https:');
        expect(parsed.hostname.length, `${key}: ${url} thiếu hostname`).toBeGreaterThan(0);
      }
    }
  });
});
