import { client, getData } from './client';
import { useAuthStore } from '@/stores/auth';
import type { PagedResponse } from './types';

/** Endpoint theo API_REFERENCE §4.14 (gamification, premium, leaderboard) */
export const GAMIFICATION_ENDPOINTS = {
  hearts: '/me/hearts',
  gamificationSummary: '/me/gamification',
  enterNode: (pathId: number, nodeId: number) => `/learning-path/${pathId}/nodes/${nodeId}/enter`,
  learningPaths: `/learning-paths`,
  learningPath: (id: number) => `/learning-path/${id}`,
  finalTest: (id: number) => `/learning-path/${id}/final-test`,
  quests: '/me/quests',
  claimQuest: (id: number) => `/me/quests/${id}/claim`,
  streak: '/me/streak',
  leaderboard: '/leaderboard',
  shopItems: '/shop/items',
  buy: '/shop/buy',
  inventory: '/me/inventory',
  equip: '/me/inventory/equip',
  achievements: '/achievements',
  premiumStatus: '/premium/status',
  premiumUpgrade: '/premium/upgrade',
  premiumMockPay: '/premium/mock-pay',
} as const;

// ── DTO (API_REFERENCE §3.12-3.13) ──

/** Tổng hợp level/XP — GET /me/gamification (feature port gamification UI). */
export interface GamificationSummaryDto {
  xp: number;
  level: number;
  xpIntoLevel: number;
  xpForNextLevel: number;
  levelProgressPct: number;
}

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

/** DTO thật từ backend GET /me/quests — trả progress + reward:{gems,xp} (KHÔNG có current/rewardGems/rewardXp). */
interface RawQuestDto {
  id: number;
  questId: number;
  title: string;
  type: number;
  progress: number;
  target: number;
  claimed: boolean;
  reward: { gems: number; xp: number };
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

/** POST /premium/upgrade — GP-T7: contentRef = mã CK DSV{userId}T{months} hiển thị trên QR MB Bank. */
export interface PremiumUpgradeResultDto {
  orderId: number;
  planId: string;
  expiresAt: string;
  contentRef: string;
}

export interface LeaderboardEntryDto {
  rank: number;
  userId: number;
  displayName: string;
  avatarUrl: string | null;
  value: number;
  streak?: number;
  level?: number;
}

export interface LeaderboardDto {
  rows: LeaderboardEntryDto[];
  myRank: LeaderboardEntryDto | null;
  page: number;
  total: number;
  totalPages: number;
}

export interface InventoryItemDto {
  id: number;              // rowId của dòng UserInventory
  itemId: number;
  itemKey: string;
  name: string;
  quantity: number;
  type: number;            // 0=consumable / 1=avatar / 2=frame (fallback — ưu tiên itemKey prefix)
  isEquipped: boolean;
  expiresAt: string | null;
}

export interface AchievementDto {
  id: number;
  code: string;
  name: string;
  description: string | null;
  iconUrl: string | null;
  earnedAt: string | null; // null = chưa đạt
}

export interface StreakDto {
  streakDays: number;
  freezeAvailable: number;
}

export interface LearningPathNodeDto {
  id: number;
  title: string;
  description: string;
  sortOrder: number;
  status: 'locked' | 'active' | 'passed';
  stars: number;
  bestScore: number | null;
  lessonId: number | null;
  simulationKey: string | null;
  exerciseId: number | null;
  requiredStages: { quiz: boolean; lab: boolean; code: boolean };
}

export interface LearningPathSummaryDto {
  id: number;
  title: string;
  description: string;
  topicId: number | null;
  sortOrder: number;
  progressPct: number;
  nodeCount: number;
}

export interface LearningPathDto {
  id: number;
  name: string;
  description: string;
  progressPct: number;
  nodes: LearningPathNodeDto[];
  finalTestUnlocked: boolean;
}

// ── CRUD (API_REFERENCE §4.14) ──

export async function fetchHearts(): Promise<HeartsStatusDto> {
  return getData<HeartsStatusDto>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.hearts });
}

export async function fetchGamificationSummary(): Promise<GamificationSummaryDto> {
  return getData<GamificationSummaryDto>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.gamificationSummary });
}

export async function enterNode(pathId: number, nodeId: number): Promise<{ session: unknown; heartsLeft: number }> {
  return getData<{ session: unknown; heartsLeft: number }>({
    method: 'POST',
    url: GAMIFICATION_ENDPOINTS.enterNode(pathId, nodeId),
  });
}

export async function fetchLearningPaths(): Promise<LearningPathSummaryDto[]> {
  return getData<LearningPathSummaryDto[]>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.learningPaths });
}

export async function fetchLearningPath(id: number): Promise<LearningPathDto> {
  return getData<LearningPathDto>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.learningPath(id) });
}

export async function fetchFinalTest(id: number): Promise<unknown> {
  return getData<unknown>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.finalTest(id) });
}

export async function fetchQuests(): Promise<QuestDto[]> {
  // Map DTO backend → view: progress→current, reward.{gems,xp}→rewardGems/rewardXp
  // (fix NaN% — backend KHÔNG trả current/rewardGems/rewardXp/description)
  const raw = await getData<RawQuestDto[]>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.quests });
  return raw.map((q) => ({
    id: q.id,
    title: q.title,
    description: '',
    target: q.target,
    current: q.progress,
    rewardGems: q.reward?.gems ?? 0,
    rewardXp: q.reward?.xp ?? 0,
    claimed: q.claimed,
  }));
}

export async function claimQuest(id: number): Promise<{ gems: number; xp: number }> {
  const raw = await getData<{ claimed: boolean; reward: { gems: number; xp: number }; gemsTotal: number }>({
    method: 'POST',
    url: GAMIFICATION_ENDPOINTS.claimQuest(id),
  });
  return { gems: raw.reward?.gems ?? 0, xp: raw.reward?.xp ?? 0 };
}

export async function fetchStreak(): Promise<StreakDto> {
  // BE trả { streakDays, streakFreeze } (StreakDto: StreakFreeze) — map sang freezeAvailable
  // để UI không bao giờ hiển thị "undefined đông cứng" khi field thiếu.
  const raw = await getData<{ streakDays: number; streakFreeze?: number }>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.streak });
  return { streakDays: raw.streakDays ?? 0, freezeAvailable: raw.streakFreeze ?? 0 };
}

export async function fetchLeaderboard(params: { tab?: 'week' | 'level' | 'class'; classId?: number; page?: number } = {}): Promise<LeaderboardDto> {
  // BE trả PagedResponse<LeaderboardEntryDto> { items, page, pageSize, total, totalPages } — KHÔNG có rows/myRank
  // (SETUP_TODO §8.1, F3-NEW-1). Map items → rows; myRank = dòng của user hiện tại (nếu có) hoặc null.
  const paged = await getData<PagedResponse<LeaderboardEntryDto>>({
    method: 'GET',
    url: GAMIFICATION_ENDPOINTS.leaderboard,
    params,
  });
  const rows = Array.isArray(paged.items) ? paged.items : [];
  let currentUserId: number | null = null;
  try {
    currentUserId = useAuthStore().user?.id ?? null;
  } catch {
    currentUserId = null; // chưa có pinia active (test/edge) — không đánh dấu dòng "Bạn"
  }
  const myRank = currentUserId === null ? null : (rows.find((item) => item.userId === currentUserId) ?? null);
  return { rows, myRank, page: paged.page, total: paged.total, totalPages: paged.totalPages };
}

export async function fetchShopItems(): Promise<ShopItemDto[]> {
  return getData<ShopItemDto[]>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.shopItems });
}

export async function buyItem(itemId: number): Promise<{ gemsLeft: number }> {
  return getData<{ gemsLeft: number }>({ method: 'POST', url: GAMIFICATION_ENDPOINTS.buy, data: { itemId } });
}

export async function fetchInventory(): Promise<InventoryItemDto[]> {
  return getData<InventoryItemDto[]>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.inventory });
}

export async function equipItem(itemId: number, isEquipped: boolean): Promise<void> {
  // Bug contract: PUT /me/inventory/equip trả 200 OK body RỖNG → caller phải fetch lại inventory.
  await client.put(GAMIFICATION_ENDPOINTS.equip, { itemId, isEquipped });
}

export async function fetchAchievements(): Promise<AchievementDto[]> {
  return getData<AchievementDto[]>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.achievements });
}

export async function fetchPremiumStatus(): Promise<PremiumStatusDto> {
  return getData<PremiumStatusDto>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.premiumStatus });
}

export async function upgradePremium(planId: string | number): Promise<PremiumUpgradeResultDto> {
  return getData<PremiumUpgradeResultDto>({
    method: 'POST',
    url: GAMIFICATION_ENDPOINTS.premiumUpgrade,
    data: { planId },
  });
}

export async function mockPayPremium(orderId: number): Promise<PremiumStatusDto> {
  return getData<PremiumStatusDto>({ method: 'POST', url: GAMIFICATION_ENDPOINTS.premiumMockPay, data: { orderId } });
}
