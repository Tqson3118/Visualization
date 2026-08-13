// data/referenceLinks.spec.ts — REFERENCE_LINKS phủ 100% key catalog (44 key).
// Thiếu/thừa key hoặc link URL hỏng → fail (giữ đồng bộ với engines/catalog.ts).

import { describe, expect, it } from 'vitest';

import { CATALOG } from '@/engines/catalog';
import { REFERENCE_LINKS } from './referenceLinks';

describe('data/referenceLinks — phủ 44 key catalog', () => {
  it('mọi key trong CATALOG đều có danh sách link (không thiếu key)', () => {
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

  it('mỗi key có ≥ 2 link, label không rỗng, url hợp lệ (https)', () => {
    for (const [key, links] of Object.entries(REFERENCE_LINKS)) {
      expect(links.length, `${key}: tối thiểu 2 link`).toBeGreaterThanOrEqual(2);
      for (const link of links) {
        expect(link.label.trim().length, `${key}: label không rỗng`).toBeGreaterThan(0);
        let url: URL;
        try {
          url = new URL(link.url);
        } catch {
          throw new Error(`${key}: URL không hợp lệ — ${link.url}`);
        }
        expect(url.protocol, `${key}: ${link.url} phải là https`).toBe('https:');
        expect(url.hostname.length, `${key}: ${link.url} thiếu hostname`).toBeGreaterThan(0);
      }
    }
  });
});
