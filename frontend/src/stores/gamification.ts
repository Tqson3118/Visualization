import { computed, ref } from 'vue';
import { defineStore } from 'pinia';

import * as gamificationApi from '@/api/gamification';
import type { InventoryItemDto, PremiumStatusDto, QuestDto, StreakDto } from '@/api/gamification';

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
  const quests = ref<QuestDto[]>([]);
  const inventory = ref<InventoryItemDto[]>([]);
  const premium = ref<PremiumStatusDto | null>(null);
  const loading = ref(false);

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
    } catch {
      // API lỗi → giữ giá trị cục bộ (fallback 5 tim)
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

  async function equipItem(id: number, slot: string): Promise<void> {
    inventory.value = await gamificationApi.equipItem(id, slot);
  }

  async function fetchStreak(): Promise<void> {
    try {
      const streak: StreakDto = await gamificationApi.fetchStreak();
      streakDays.value = streak.streakDays;
      freezeAvailable.value = streak.freezeAvailable;
    } catch {
      // bỏ qua — streak không bắt buộc
    }
  }

  async function fetchPremium(): Promise<void> {
    try {
      premium.value = await gamificationApi.fetchPremiumStatus();
    } catch {
      premium.value = { isPremium: false, plan: null, expiresAt: null };
    }
  }

  /** Nạp toàn bộ gamification một lượt (header/profile/quests) */
  async function fetchAll(): Promise<void> {
    loading.value = true;
    try {
      await Promise.allSettled([fetchHearts(), fetchStreak(), fetchPremium()]);
    } finally {
      loading.value = false;
    }
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
    quests,
    inventory,
    premium,
    loading,
    heartsPercent,
    questDone,
    isPremium,
    fetchAll,
    fetchHearts,
    enterNode,
    fetchQuests,
    claimQuest,
    fetchInventory,
    buyItem,
    equipItem,
    fetchStreak,
    fetchPremium,
  };
});
