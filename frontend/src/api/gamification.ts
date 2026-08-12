import { client, getData } from './client';
import { useAuthStore } from '@/stores/auth';
import type { PagedResponse } from './types';

/** Endpoint theo API_REFERENCE §4.14 (gamification, premium, leaderboard) */
export const GAMIFICATION_ENDPOINTS = {
  hearts: '/me/hearts',
  enterNode: (pathId: number, nodeId: number) => `/learning-path/${pathId}/nodes/${nodeId}/enter`,
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
  id: number;
  itemId: number;
  name: string;
  slot: string | null;
  isEquipped: boolean;
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

export async function enterNode(pathId: number, nodeId: number): Promise<{ session: unknown; heartsLeft: number }> {
  return getData<{ session: unknown; heartsLeft: number }>({
    method: 'POST',
    url: GAMIFICATION_ENDPOINTS.enterNode(pathId, nodeId),
  });
}

export async function fetchLearningPath(id: number): Promise<LearningPathDto> {
  return getData<LearningPathDto>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.learningPath(id) });
}

export async function fetchFinalTest(id: number): Promise<unknown> {
  return getData<unknown>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.finalTest(id) });
}

export async function fetchQuests(): Promise<QuestDto[]> {
  return getData<QuestDto[]>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.quests });
}

export async function claimQuest(id: number): Promise<{ gems: number; xp: number }> {
  return getData<{ gems: number; xp: number }>({ method: 'POST', url: GAMIFICATION_ENDPOINTS.claimQuest(id) });
}

export async function fetchStreak(): Promise<StreakDto> {
  return getData<StreakDto>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.streak });
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

export async function equipItem(itemId: number, slot: string): Promise<InventoryItemDto[]> {
  return getData<InventoryItemDto[]>({ method: 'PUT', url: GAMIFICATION_ENDPOINTS.equip, data: { itemId, slot } });
}

export async function fetchPremiumStatus(): Promise<PremiumStatusDto> {
  return getData<PremiumStatusDto>({ method: 'GET', url: GAMIFICATION_ENDPOINTS.premiumStatus });
}

export async function upgradePremium(planId: string | number): Promise<{ orderId: string; plan: string }> {
  return getData<{ orderId: string; plan: string }>({
    method: 'POST',
    url: GAMIFICATION_ENDPOINTS.premiumUpgrade,
    data: { planId },
  });
}

export async function mockPayPremium(orderId: string): Promise<PremiumStatusDto> {
  return getData<PremiumStatusDto>({ method: 'POST', url: GAMIFICATION_ENDPOINTS.premiumMockPay, data: { orderId } });
}
