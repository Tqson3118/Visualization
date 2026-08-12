/** Endpoint theo API_REFERENCE §4.14 (gamification, premium, leaderboard) */
export const GAMIFICATION_ENDPOINTS = {
  hearts: '/me/hearts',
  enterNode: (pathId: number, nodeId: number) => `/learning-path/${pathId}/nodes/${nodeId}/enter`,
  quests: '/me/quests',
  claimQuest: (id: number) => `/me/quests/${id}/claim`,
  streak: '/me/streak',
  leaderboard: '/leaderboard',
  shopItems: '/shop/items',
  buy: '/shop/buy',
  inventory: '/me/inventory',
  equip: '/me/inventory/equip',
  premiumStatus: '/premium/status',
  premiumUpgrade: '/premium/upgrade',
  premiumMockPay: '/premium/mock-pay',
} as const;

// ── DTO (API_REFERENCE §3.12-3.13) ──

export interface HeartsStatusDto {
  hearts: number;
  heartsMax: number;
  lastHeartAt: string | null;
  nextHeartAt: string | null;
}

export interface QuestDto {
  id: number;
  title: string;
  description: string;
  target: number;
  current: number;
  rewardGems: number;
  rewardXp: number;
  claimed: boolean;
}

export interface ShopItemDto {
  id: number;
  name: string;
  description: string;
  priceGems: number;
  slot: string | null;
}

export interface PremiumStatusDto {
  isPremium: boolean;
  plan: string | null;
  expiresAt: string | null;
}

// ── Stub CRUD (body TODO) ──

export async function fetchHearts(): Promise<HeartsStatusDto> {
  // TODO: getData({ method: 'GET', url: GAMIFICATION_ENDPOINTS.hearts })
  return Promise.reject(new Error('TODO: gamificationApi.fetchHearts chưa triển khai'));
}

export async function enterNode(pathId: number, nodeId: number): Promise<{ session: unknown; heartsLeft: number }> {
  // TODO: client.post(GAMIFICATION_ENDPOINTS.enterNode(pathId, nodeId))
  return Promise.reject(new Error('TODO: gamificationApi.enterNode chưa triển khai'));
}

export async function fetchQuests(): Promise<QuestDto[]> {
  // TODO: getData({ method: 'GET', url: GAMIFICATION_ENDPOINTS.quests })
  return Promise.reject(new Error('TODO: gamificationApi.fetchQuests chưa triển khai'));
}

export async function claimQuest(id: number): Promise<{ gems: number; xp: number }> {
  // TODO: client.post(GAMIFICATION_ENDPOINTS.claimQuest(id))
  return Promise.reject(new Error('TODO: gamificationApi.claimQuest chưa triển khai'));
}

export async function fetchLeaderboard(params: { tab?: 'week' | 'level' | 'class'; classId?: number; page?: number } = {}): Promise<unknown> {
  // TODO: getData({ method: 'GET', url: GAMIFICATION_ENDPOINTS.leaderboard, params })
  return Promise.reject(new Error('TODO: gamificationApi.fetchLeaderboard chưa triển khai'));
}

export async function fetchShopItems(): Promise<ShopItemDto[]> {
  // TODO: getData({ method: 'GET', url: GAMIFICATION_ENDPOINTS.shopItems })
  return Promise.reject(new Error('TODO: gamificationApi.fetchShopItems chưa triển khai'));
}

export async function buyItem(itemId: number): Promise<unknown> {
  // TODO: client.post(GAMIFICATION_ENDPOINTS.buy, { itemId })
  return Promise.reject(new Error('TODO: gamificationApi.buyItem chưa triển khai'));
}

export async function fetchPremiumStatus(): Promise<PremiumStatusDto> {
  // TODO: getData({ method: 'GET', url: GAMIFICATION_ENDPOINTS.premiumStatus })
  return Promise.reject(new Error('TODO: gamificationApi.fetchPremiumStatus chưa triển khai'));
}
