import { computed, ref } from 'vue';
import { defineStore } from 'pinia';

import * as gamificationApi from '@/api/gamification';
import type {
  AchievementDto,
  GamificationSummaryDto,
  InventoryItemDto,
  PremiumStatusDto,
  QuestDto,
  StreakDto,
} from '@/api/gamification';

/** Store gamification theo SDD §3.2 — Module J (ADR-011) — triển khai thật với API. */
export const useGamificationStore = defineStore('gamification', () => {
  const hearts = ref(0);
  const heartsMax = ref(5);
  const lastHeartAt = ref<string | null>(null);
  const gems = ref(0);
  const streakDays = ref(0);
  const freezeAvailable = ref(0);
  const xp = ref(0);
  const level = ref(1);
  /** Tổng hợp level/XP từ GET /me/gamification — nguồn số liệu THẬT cho các card gamification. */
  const summary = ref<GamificationSummaryDto | null>(null);
  const quests = ref<QuestDto[]>([]);
  const inventory = ref<InventoryItemDto[]>([]);
  const achievements = ref<AchievementDto[]>([]);
  const premium = ref<PremiumStatusDto | null>(null);
  const loading = ref(false);

  /** XP đã tích lũy trong level hiện tại (từ summary — 0 khi chưa tải). */
  const xpIntoLevel = computed(() => summary.value?.xpIntoLevel ?? 0);
  /** XP cần để lên level tiếp theo (từ summary). */
  const xpForNextLevel = computed(() => summary.value?.xpForNextLevel ?? 100);
  /** Phần trăm tiến tới level kế (0-100). */
  const levelProgressPct = computed(() => summary.value?.levelProgressPct ?? 0);
  const heartsPercent = computed(() =>
    heartsMax.value === 0 ? 0 : Math.round((hearts.value / heartsMax.value) * 100),
  );
  const questDone = computed(() => quests.value.filter((quest) => quest.claimed).length);
  const isPremium = computed(() => premium.value?.isPremium ?? false);

  async function fetchHearts(): Promise<void> {
    try {
      const status = await gamificationApi.fetchHearts();
      hearts.value = status.hearts;
      heartsMax.value = status.heartsMax;
      lastHeartAt.value = status.lastHeartAt;
      if (typeof status.gems === 'number') {
        gems.value = status.gems;
      }
    } catch {
      // API failure is not a local/demo data source; keep the unavailable state.
    }
  }

  /** Nạp level/XP từ server (nguồn thật — không cộng XP ở FE). */
  async function fetchSummary(): Promise<void> {
    try {
      summary.value = await gamificationApi.fetchGamificationSummary();
      xp.value = summary.value.xp;
      level.value = summary.value.level;
      if (typeof summary.value.gems === 'number') {
        gems.value = summary.value.gems;
      }
    } catch {
      summary.value = null;
    }
  }

  async function enterNode(pathId: number, nodeId: number): Promise<{ session: unknown; heartsLeft: number }> {
    const result = await gamificationApi.enterNode(pathId, nodeId);
    hearts.value = result.heartsLeft;
    return result;
  }

  async function fetchQuests(): Promise<void> {
    try {
      quests.value = await gamificationApi.fetchQuests();
    } catch {
      // API lỗi → danh sách rỗng, view hiện empty state
    }
  }

  async function claimQuest(id: number): Promise<void> {
    const reward = await gamificationApi.claimQuest(id);
    const quest = quests.value.find((q) => q.id === id);
    if (quest) quest.claimed = true;
    gems.value += reward.gems;
    xp.value += reward.xp;
  }

  async function fetchInventory(): Promise<void> {
    try {
      inventory.value = await gamificationApi.fetchInventory();
    } catch {
      inventory.value = [];
    }
  }

  async function buyItem(id: number): Promise<void> {
    const result = await gamificationApi.buyItem(id);
    gems.value = result.gemsLeft;
    await fetchInventory();
  }

  async function equipItem(id: number, isEquipped: boolean): Promise<void> {
    // Bug contract: API trả 200 body rỗng → luôn refetch inventory để UI khớp server.
    await gamificationApi.equipItem(id, isEquipped);
    await fetchInventory();
  }

  async function fetchAchievements(): Promise<void> {
    try {
      achievements.value = await gamificationApi.fetchAchievements();
    } catch {
      achievements.value = [];
    }
  }

  async function fetchStreak(): Promise<void> {
    try {
      const streak: StreakDto = await gamificationApi.fetchStreak();
      streakDays.value = streak.streakDays ?? 0;
      freezeAvailable.value = streak.freezeAvailable ?? 0;
    } catch {
      // bỏ qua — streak không bắt buộc
    }
  }

  async function fetchPremium(): Promise<void> {
    try {
      premium.value = await gamificationApi.fetchPremiumStatus();
    } catch {
      premium.value = null;
    }
  }

  /** Nạp toàn bộ gamification một lượt (header/profile/quests) */
  async function fetchAll(): Promise<void> {
    loading.value = true;
    try {
      await Promise.allSettled([fetchHearts(), fetchSummary(), fetchStreak(), fetchPremium()]);
    } finally {
      loading.value = false;
    }
  }

  function reset(): void {
    hearts.value = 0;
    heartsMax.value = 5;
    lastHeartAt.value = null;
    gems.value = 0;
    streakDays.value = 0;
    freezeAvailable.value = 0;
    xp.value = 0;
    level.value = 1;
    summary.value = null;
    quests.value = [];
    inventory.value = [];
    achievements.value = [];
    premium.value = null;
    loading.value = false;
  }

  return {
    hearts,
    heartsMax,
    lastHeartAt,
    gems,
    streakDays,
    freezeAvailable,
    xp,
    level,
    summary,
    xpIntoLevel,
    xpForNextLevel,
    levelProgressPct,
    quests,
    inventory,
    achievements,
    premium,
    loading,
    heartsPercent,
    questDone,
    isPremium,
    fetchAll,
    fetchHearts,
    fetchSummary,
    enterNode,
    fetchQuests,
    claimQuest,
    fetchInventory,
    buyItem,
    equipItem,
    fetchAchievements,
    fetchStreak,
    fetchPremium,
    reset,
  };
});
