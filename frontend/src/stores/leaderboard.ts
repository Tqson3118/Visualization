import { ref } from 'vue';
import { defineStore } from 'pinia';

import * as gamificationApi from '@/api/gamification';
import type { LeaderboardEntryDto } from '@/api/gamification';

/** Store leaderboard theo SDD §3.2 — triển khai thật với API /leaderboard. */
export const useLeaderboardStore = defineStore('leaderboard', () => {
  const tab = ref<'week' | 'level' | 'class'>('week');
  const rows = ref<LeaderboardEntryDto[]>([]);
  const myRank = ref<LeaderboardEntryDto | null>(null);
  // G-F2d: phân trang thật (BE trả totalPages từ PagedResponse) — additive, giữ nguyên rows/myRank.
  const page = ref(1);
  const totalPages = ref(1);
  const loading = ref(false);
  const error = ref<string | null>(null);
  // G-F3E-NEW-2: tab Lớp khi user chưa tham gia lớp nào → EmptyState, KHÔNG gọi API (tránh 400).
  const noClass = ref(false);

  async function fetchBoard(nextTab?: 'week' | 'level' | 'class', classId?: number, nextPage?: number): Promise<void> {
    // Đổi tab → về trang 1 (hành vi cũ giữ nguyên); nextPage chỉ dùng khi bấm phân trang.
    if (nextTab) {
      tab.value = nextTab;
      page.value = 1;
    }
    if (nextPage !== undefined) page.value = nextPage;
    loading.value = true;
    error.value = null;
    noClass.value = false;
    try {
      const board = await gamificationApi.fetchLeaderboard({ tab: tab.value, classId, page: page.value });
      rows.value = board.rows;
      myRank.value = board.myRank;
      totalPages.value = board.totalPages > 0 ? board.totalPages : 1;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Không tải được bảng xếp hạng';
      rows.value = [];
      myRank.value = null;
    } finally {
      loading.value = false;
    }
  }

  /** G-F3E-NEW-2: tab Lớp nhưng user chưa tham gia lớp → EmptyState thay vì gọi API (không 400). */
  function setNoClass(): void {
    tab.value = 'class';
    page.value = 1;
    rows.value = [];
    myRank.value = null;
    totalPages.value = 1;
    loading.value = false;
    error.value = null;
    noClass.value = true;
  }

  return { tab, rows, myRank, page, totalPages, loading, error, noClass, fetchBoard, setNoClass };
});
