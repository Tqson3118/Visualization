import { ref } from 'vue';
import { defineStore } from 'pinia';

import * as gamificationApi from '@/api/gamification';
import type { LeaderboardEntryDto } from '@/api/gamification';

/** Store leaderboard theo SDD §3.2 — triển khai thật với API /leaderboard. */
export const useLeaderboardStore = defineStore('leaderboard', () => {
  const tab = ref<'week' | 'level' | 'class'>('week');
  const rows = ref<LeaderboardEntryDto[]>([]);
  const myRank = ref<LeaderboardEntryDto | null>(null);
  const loading = ref(false);
  const error = ref<string | null>(null);

  async function fetchBoard(nextTab?: 'week' | 'level' | 'class', classId?: number): Promise<void> {
    if (nextTab) tab.value = nextTab;
    loading.value = true;
    error.value = null;
    try {
      const board = await gamificationApi.fetchLeaderboard({ tab: tab.value, classId });
      rows.value = board.rows;
      myRank.value = board.myRank;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Không tải được bảng xếp hạng';
      rows.value = [];
      myRank.value = null;
    } finally {
      loading.value = false;
    }
  }

  return { tab, rows, myRank, loading, error, fetchBoard };
});
