import { createPinia, setActivePinia } from 'pinia';
import { beforeEach, describe, expect, it, vi } from 'vitest';

vi.mock('@/api/gamification', () => ({
  claimQuest: vi.fn(),
  fetchHearts: vi.fn(),
  fetchGamificationSummary: vi.fn(),
  fetchQuests: vi.fn(),
  fetchInventory: vi.fn(),
  buyItem: vi.fn(),
  equipItem: vi.fn(),
  fetchAchievements: vi.fn(),
  fetchStreak: vi.fn(),
  fetchPremiumStatus: vi.fn(),
  spendHeart: vi.fn(),
  enterNode: vi.fn(),
}));

import * as gamificationApi from '@/api/gamification';
import { useGamificationStore } from '../gamification';

describe('useGamificationStore — Adversarial & Concurrency Tests', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  describe('claimQuest concurrency & double click protection', () => {
    it('không gọi API 2 lần hoặc cộng đôi gems/xp khi bấm nhận thưởng 2 lần đồng thời', async () => {
      const store = useGamificationStore();
      store.quests = [
        {
          id: 101,
          title: 'Đăng nhập lần đầu',
          description: 'Mô tả',
          rewardGems: 50,
          rewardXp: 100,
          current: 1,
          target: 1,
          claimed: false,
        },
      ];

      (gamificationApi.claimQuest as any).mockResolvedValue({
        success: true,
        rewardGems: 50,
        rewardXp: 100,
      });

      // Gọi 2 lần đồng thời
      const promise1 = store.claimQuest(101);
      const promise2 = store.claimQuest(101);

      await Promise.all([promise1, promise2]);

      // Chỉ được gọi API đúng 1 lần
      expect(gamificationApi.claimQuest).toHaveBeenCalledTimes(1);
    });

    it('không crash khi claim quest ID không tồn tại', async () => {
      const store = useGamificationStore();
      store.quests = [
        {
          id: 101,
          title: 'Đăng nhập lần đầu',
          description: 'Mô tả',
          rewardGems: 50,
          rewardXp: 100,
          current: 0,
          target: 1,
          claimed: false,
        },
      ];

      await store.claimQuest(102);
      expect(gamificationApi.claimQuest).not.toHaveBeenCalled();
    });
  });
});
