import { computed, ref } from 'vue';
import { defineStore } from 'pinia';

import type { PremiumStatusDto, QuestDto } from '@/api/gamification';

/** Store gamification theo SDD §3.2 — Module J (ADR-011) */
export const useGamificationStore = defineStore('gamification', () => {
  const hearts = ref(0);
  const heartsMax = ref(5);
  const lastHeartAt = ref<string | null>(null);
  const gems = ref(0);
  const streakDays = ref(0);
  const xp = ref(0);
  const level = ref(1);
  const quests = ref<QuestDto[]>([]);
  const inventory = ref<unknown[]>([]);
  const premium = ref<PremiumStatusDto | null>(null);

  const heartsPercent = computed(() =>
    heartsMax.value === 0 ? 0 : Math.round((hearts.value / heartsMax.value) * 100),
  );
  const questDone = computed(() => quests.value.filter((quest) => quest.claimed).length);
  const isPremium = computed(() => premium.value?.isPremium ?? false);

  async function fetchHearts(): Promise<void> {
    // TODO: gán hearts/heartsMax/lastHeartAt từ gamificationApi.fetchHearts()
    return Promise.reject(new Error('TODO: gamificationStore.fetchHearts chưa triển khai'));
  }

  async function enterNode(nodeId: number): Promise<void> {
    // TODO: gọi gamificationApi.enterNode(pathId, nodeId) — pathId lấy từ route/lessonStore
    void nodeId;
    return Promise.reject(new Error('TODO: gamificationStore.enterNode chưa triển khai'));
  }

  async function fetchQuests(): Promise<void> {
    // TODO
    return Promise.reject(new Error('TODO: gamificationStore.fetchQuests chưa triển khai'));
  }

  async function claimQuest(id: number): Promise<void> {
    // TODO
    void id;
    return Promise.reject(new Error('TODO: gamificationStore.claimQuest chưa triển khai'));
  }

  async function fetchInventory(): Promise<void> {
    // TODO
    return Promise.reject(new Error('TODO: gamificationStore.fetchInventory chưa triển khai'));
  }

  async function buyItem(id: number): Promise<void> {
    // TODO
    void id;
    return Promise.reject(new Error('TODO: gamificationStore.buyItem chưa triển khai'));
  }

  async function equipItem(id: number): Promise<void> {
    // TODO
    void id;
    return Promise.reject(new Error('TODO: gamificationStore.equipItem chưa triển khai'));
  }

  async function fetchPremium(): Promise<void> {
    // TODO: premium.value = await gamificationApi.fetchPremiumStatus()
    return Promise.reject(new Error('TODO: gamificationStore.fetchPremium chưa triển khai'));
  }

  return {
    hearts,
    heartsMax,
    lastHeartAt,
    gems,
    streakDays,
    xp,
    level,
    quests,
    inventory,
    premium,
    heartsPercent,
    questDone,
    isPremium,
    fetchHearts,
    enterNode,
    fetchQuests,
    claimQuest,
    fetchInventory,
    buyItem,
    equipItem,
    fetchPremium,
  };
});
