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
  // G-F3E2: classId lần gần nhất của tab Lớp — phân trang (goToPage) tái dùng để không gửi
  // classId: undefined → backend 400 "Thiếu classId cho tab lớp" (P2 review g-f3c F2).
  const lastClassId = ref<number | null>(null);

  async function fetchBoard(nextTab?: 'week' | 'level' | 'class', classId?: number, nextPage?: number): Promise<void> {
    // Đổi tab → về trang 1 (hành vi cũ giữ nguyên); nextPage chỉ dùng khi bấm phân trang.
    if (nextTab) {
      tab.value = nextTab;
      page.value = 1;
    }
    if (nextPage !== undefined) page.value = nextPage;
    // G-F3E2: lưu classId mỗi khi được truyền (tab Lớp tải lần đầu / chuyển tab) → phân trang tái dùng.
    if (classId !== undefined) lastClassId.value = classId;
    loading.value = true;
    error.value = null;
    noClass.value = false;
    try {
      // G-F3E2: tab=class mà không truyền classId (phân trang) → dùng lastClassId, tránh 400 "Thiếu classId".
      // lastClassId có thể null (chưa từng resolve) → chuyển thành undefined để API không nhận null.
      const effectiveClassId = tab.value === 'class' ? (classId ?? lastClassId.value ?? undefined) : undefined;
      const board = await gamificationApi.fetchLeaderboard({ tab: tab.value, classId: effectiveClassId, page: page.value });
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
    // G-F3E2: không có lớp → không còn classId hợp lệ để phân trang.
    lastClassId.value = null;
  }

  return { tab, rows, myRank, page, totalPages, loading, error, noClass, lastClassId, fetchBoard, setNoClass };
});
