/**
 * Verify dữ liệu seed (frontend/src/data/*):
 * 1. Mọi simulation key trong LESSONS phải tồn tại trong shared/simulation-catalog.json.
 * 2. Mọi lesson ref trong SEED_COURSES phải có entry trong LESSONS.
 * 3. shop_items.json parse hợp lệ, id unique, đủ số item theo SDD §7.5 (≥ 8).
 */
import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';
import { describe, expect, it } from 'vitest';

import { SEED_COURSES } from './courses';
import { LESSONS } from './lessons';

interface CatalogItem {
  key: string;
  title: string;
  dataStructure: string;
}

interface ShopItemSeed {
  id: string;
  type: string;
  name: string;
  description: string;
  price: number;
  imageUrl: string;
  cssClass?: string;
}

/** shared/simulation-catalog.json — nằm ngoài frontend/src; thử nhiều cwd (frontend/, repo root). */
const CATALOG_CANDIDATES = [
  resolve(process.cwd(), '../shared/simulation-catalog.json'), // cwd = frontend/
  resolve(process.cwd(), 'shared/simulation-catalog.json'), // cwd = repo root
];

const SHOP_ITEMS_CANDIDATES = [
  resolve(process.cwd(), 'src/data/shop_items.json'), // cwd = frontend/
  resolve(process.cwd(), 'frontend/src/data/shop_items.json'), // cwd = repo root
];

function firstExisting(candidates: string[]): string {
  for (const candidate of candidates) {
    try {
      readFileSync(candidate);
      return candidate;
    } catch {
      // thử ứng viên tiếp theo
    }
  }
  throw new Error(`Không tìm thấy file trong các đường dẫn: ${candidates.join(', ')}`);
}

const CATALOG_PATH = firstExisting(CATALOG_CANDIDATES);
const SHOP_ITEMS_PATH = firstExisting(SHOP_ITEMS_CANDIDATES);

function readJsonFile(path: string): unknown {
  // Strip UTF-8 BOM (file sẵn có như shared/simulation-catalog.json)
  return JSON.parse(readFileSync(path, 'utf8').replace(/^\uFEFF/, ''));
}

function readCatalog(): CatalogItem[] {
  return readJsonFile(CATALOG_PATH) as CatalogItem[];
}

function readShopItems(): ShopItemSeed[] {
  return readJsonFile(SHOP_ITEMS_PATH) as ShopItemSeed[];
}

describe('seed data: simulation keys', () => {
  it('mọi simulation key trong LESSONS đều tồn tại trong simulation-catalog', () => {
    const catalogKeys = new Set(readCatalog().map((item) => item.key));
    const missing: Array<{ lessonId: string; key: string }> = [];

    for (const lesson of Object.values(LESSONS)) {
      for (const key of lesson.simulations) {
        if (!catalogKeys.has(key)) {
          missing.push({ lessonId: lesson.id, key });
        }
      }
    }

    // Bài không có simulation (VD: sliding-window) phải có mảng simulations rỗng trong lessons.ts
    expect(missing, `simulation key thiếu trong catalog: ${JSON.stringify(missing)}`).toEqual([]);
  });

  it('mọi lesson trong LESSONS đều có id khớp key của record', () => {
    for (const [key, lesson] of Object.entries(LESSONS)) {
      expect(lesson.id).toBe(key);
    }
  });
});

describe('seed data: courses ↔ lessons', () => {
  it('mọi lesson ref trong SEED_COURSES đều có entry trong LESSONS', () => {
    const missingLessonIds: string[] = [];
    const courseLessonIds = new Set<string>();

    for (const course of SEED_COURSES) {
      for (const ref of course.lessons) {
        courseLessonIds.add(ref.id);
        if (LESSONS[ref.id] === undefined) {
          missingLessonIds.push(`${course.id}:${ref.id}`);
        }
      }
    }

    expect(missingLessonIds, `lesson id thiếu trong LESSONS: ${JSON.stringify(missingLessonIds)}`).toEqual([]);
  });

  it('mọi lesson trong LESSONS đều thuộc một course trong SEED_COURSES', () => {
    const courseIds = new Set(SEED_COURSES.map((course) => course.id));
    const orphans = Object.values(LESSONS)
      .filter((lesson) => !courseIds.has(lesson.courseId))
      .map((lesson) => lesson.id);

    expect(orphans, `lesson không thuộc course nào: ${JSON.stringify(orphans)}`).toEqual([]);
  });

  it('mọi lesson của một course có sortOrder khớp order trong course', () => {
    for (const course of SEED_COURSES) {
      for (const ref of course.lessons) {
        expect(LESSONS[ref.id].sortOrder, `${course.id}:${ref.id}`).toBe(ref.order);
      }
    }
  });
});

describe('seed data: shop items', () => {
  it('shop_items.json parse hợp lệ, id unique, đủ ≥ 8 item (SDD §7.5)', () => {
    const items = readShopItems();
    const ids = new Set(items.map((item) => item.id));

    expect(items.length).toBeGreaterThanOrEqual(8);
    expect(ids.size).toBe(items.length);
  });

  it('mọi shop item có price dương và type hợp lệ', () => {
    for (const item of readShopItems()) {
      expect(item.price).toBeGreaterThan(0);
      expect(['avatar', 'frame']).toContain(item.type);
    }
  });
});
