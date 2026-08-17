import { describe, expect, it } from 'vitest';

import { messages } from '@/i18n/vi';

// PR30 regression: vi.ts phải chứa đủ key dev (F1/F2/F3) + key purple mới — không được xóa key cũ.
const mustHave = {
  dev: [
    'classes.curriculumTab',
    'classes.detailStatusOpen',
    'classes.detailStatusMissing',
    'common.toggleTheme',
    'common.toLightTheme',
    'auth.tooManyAttempts',
    'gamification.streakUnit',
    'gamification.questsTitle',
    'home.heroTitlePrefix',
    'home.dashXpTitle',
    'home.catalogTitle',
    'codeToVisual.title',
    'shop.title',
  ],
  pr30: [
    'shop.mascotGreeting',
    'shop.signTitle',
    'shop.ticker',
    'home.heroTitle',
    'home.heroBadge',
    'home.featureVisual.title',
    'home.featurePath.title',
    'home.featurePractice.title',
    'home.ctaGoCourses',
    'admin.users.detailTitle',
  ],
};

function has(key: string): boolean {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  let node: any = messages;
  for (const part of key.split('.')) {
    if (node == null || typeof node !== 'object' || !(part in node)) return false;
    node = node[part];
  }
  return node !== undefined;
}

describe('i18n vi.ts — PR30 key preservation', () => {
  for (const key of mustHave.dev) {
    it(`dev key tồn tại: ${key}`, () => expect(has(key), key).toBe(true));
  }
  for (const key of mustHave.pr30) {
    it(`PR30 key tồn tại: ${key}`, () => expect(has(key), key).toBe(true));
  }
});
