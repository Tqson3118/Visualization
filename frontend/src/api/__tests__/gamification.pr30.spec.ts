import { beforeEach, describe, expect, it, vi } from 'vitest';

import * as gamificationApi from '@/api/gamification';

// PR30 regression: streak/shop response mapping phải ổn định (không "undefined đông cứng", không NaN giá).
vi.mock('@/api/client', () => ({
  client: { get: vi.fn(), post: vi.fn(), put: vi.fn(), delete: vi.fn() },
  getData: vi.fn(),
}));

import { getData } from '@/api/client';

describe('api/gamification — PR30 regression', () => {
  beforeEach(() => vi.clearAllMocks());

  it('fetchStreak maps backend streakFreeze -> freezeAvailable (không undefined)', async () => {
    vi.mocked(getData).mockResolvedValue({ streakDays: 5, streakFreeze: 2 });
    const r = await gamificationApi.fetchStreak();
    expect(r).toEqual({ streakDays: 5, freezeAvailable: 2 });
  });

  it('fetchStreak defaults khi backend thiếu field (không "undefined đông cứng")', async () => {
    vi.mocked(getData).mockResolvedValue({ streakDays: 3 });
    const r = await gamificationApi.fetchStreak();
    expect(r.freezeAvailable).toBe(0);
    expect(r.streakDays).toBe(3);
  });

  it('fetchShopItems giữ fallback priceGems khi backend trả price/cost', async () => {
    vi.mocked(getData).mockResolvedValue([
      { id: 1, name: 'Vật phẩm', description: '', slot: null, price: 120 },
      { id: 2, name: 'Vật phẩm B', description: '', slot: null, priceGems: 60 },
    ]);
    const [a, b] = await gamificationApi.fetchShopItems();
    expect(a.priceGems).toBe(120);
    expect(b.priceGems).toBe(60);
  });

  it('fetchLearningPaths calls /learning-paths (F1 selector)', async () => {
    vi.mocked(getData).mockResolvedValue([]);
    await gamificationApi.fetchLearningPaths();
    expect(vi.mocked(getData)).toHaveBeenCalledWith({ method: 'GET', url: '/learning-paths' });
  });
});
