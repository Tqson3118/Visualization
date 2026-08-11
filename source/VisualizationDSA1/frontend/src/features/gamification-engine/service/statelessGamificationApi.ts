





import { API_BASE_URL } from '@/services/apiConfig';

const BASE_URL = API_BASE_URL;

export interface StatelessUserProfile {
  userId: string;
  username: string;
  totalXp: number;
  currentLevel: number;
  levelName: string;
  streakDays: number;
  earnedBadges: StatelessBadge[];
  recentActivity: StatelessXpEvent[];
}

export interface StatelessBadge {
  id: string;
  name: string;
  description: string;
  icon: string;
  color: string;
  earnedAt: string;
}

export interface StatelessXpEvent {
  type: string;
  amount: number;
  description: string;
  timestamp: string;
}

export interface StatelessLeaderboardEntry {
  rank: number;
  username: string;
  totalXp: number;
  level: number;
  levelName: string;
  badgeCount: number;
  streakDays: number;
}

export const statelessGamificationApi = {
  
  async getProfile(): Promise<StatelessUserProfile> {
    const res = await fetch(`${BASE_URL}/api/v1/concepts/gamification/profile`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },

  
  async awardXp(amount: number, reason: string): Promise<StatelessUserProfile> {
    const res = await fetch(`${BASE_URL}/api/v1/concepts/gamification/award-xp`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ amount, reason }),
    });
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },

  
  async getBadges(): Promise<StatelessBadge[]> {
    const res = await fetch(`${BASE_URL}/api/v1/concepts/gamification/badges`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },

  
  async getLeaderboard(limit: number = 10): Promise<StatelessLeaderboardEntry[]> {
    const res = await fetch(`${BASE_URL}/api/v1/concepts/gamification/leaderboard?limit=${limit}`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },

  
  async getConfig(): Promise<Record<string, unknown>> {
    const res = await fetch(`${BASE_URL}/api/v1/concepts/gamification/config`);
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return res.json();
  },
};
