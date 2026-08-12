import { ref } from 'vue';
import { defineStore } from 'pinia';

/** Store leaderboard theo SDD §3.2 */
export interface LeaderboardRow {
  rank: number;
  userId: number;
  displayName: string;
  avatarUrl: string | null;
  value: number; // xp (week/level) hoặc điểm trung bình (class)
}

export const useLeaderboardStore = defineStore('leaderboard', () => {
  const tab = ref<'week' | 'level' | 'class'>('week');
  const rows = ref<LeaderboardRow[]>([]);
  const myRank = ref<LeaderboardRow | null>(null);

  async function fetchBoard(nextTab?: 'week' | 'level' | 'class'): Promise<void> {
    // TODO: gọi gamificationApi.fetchLeaderboard({ tab }) — API_REFERENCE §4.14
    if (nextTab) tab.value = nextTab;
    return Promise.reject(new Error('TODO: leaderboardStore.fetchBoard chưa triển khai'));
  }

  return { tab, rows, myRank, fetchBoard };
});
