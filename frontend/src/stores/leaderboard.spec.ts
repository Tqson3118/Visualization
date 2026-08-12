import { createPinia, setActivePinia } from 'pinia';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import * as gamificationApi from '@/api/gamification';
import type { LeaderboardDto, LeaderboardEntryDto } from '@/api/gamification';
import { useLeaderboardStore } from './leaderboard';

// Mẫu test store theo SDD §3.7 (mock api module) — G-F3E-NEW-1/NEW-2:
// rows có value → render không crash; tab Lớp truyền classId / EmptyState khi chưa có lớp.
vi.mock('@/api/gamification', () => ({
  fetchLeaderboard: vi.fn(),
}));

const rows: LeaderboardEntryDto[] = [
  { rank: 1, userId: 11, displayName: 'Nguyễn Minh Anh', avatarUrl: null, value: 2450, streak: 12, level: 24 },
  { rank: 2, userId: 12, displayName: 'Trần Quốc Bảo', avatarUrl: null, value: 2100, streak: 9, level: 21 },
];

function mockBoard(overrides: Partial<LeaderboardDto> = {}): LeaderboardDto {
  return { rows, myRank: null, page: 1, total: 2, totalPages: 1, ...overrides };
}

describe('leaderboard store', () => {
  beforeEach(() => {
    setActivePinia(createPinia());
    vi.clearAllMocks();
  });

  it('fetchBoard map items → rows giữ nguyên value (FE đọc row.value không crash) + đúng tab', async () => {
    vi.mocked(gamificationApi.fetchLeaderboard).mockResolvedValue(mockBoard());
    const store = useLeaderboardStore();
    await store.fetchBoard('level');
    expect(gamificationApi.fetchLeaderboard).toHaveBeenCalledWith({ tab: 'level', classId: undefined, page: 1 });
    expect(store.tab).toBe('level');
    expect(store.rows).toHaveLength(2);
    expect(store.rows[0].value).toBe(2450);
    expect(store.rows[1].value).toBe(2100);
    expect(store.loading).toBe(false);
    expect(store.noClass).toBe(false);
    expect(store.error).toBeNull();
  });

  it('fetchBoard tab class truyền classId vào API', async () => {
    vi.mocked(gamificationApi.fetchLeaderboard).mockResolvedValue(mockBoard());
    const store = useLeaderboardStore();
    await store.fetchBoard('class', 7);
    expect(gamificationApi.fetchLeaderboard).toHaveBeenCalledWith({ tab: 'class', classId: 7, page: 1 });
    expect(store.tab).toBe('class');
    expect(store.noClass).toBe(false);
  });

  it('setNoClass bật EmptyState "chưa tham gia lớp" và không gọi API', () => {
    const store = useLeaderboardStore();
    store.setNoClass();
    expect(store.tab).toBe('class');
    expect(store.noClass).toBe(true);
    expect(store.rows).toHaveLength(0);
    expect(store.myRank).toBeNull();
    expect(store.error).toBeNull();
    expect(store.loading).toBe(false);
    expect(gamificationApi.fetchLeaderboard).not.toHaveBeenCalled();
  });

  it('fetchBoard lỗi → error set, rows rỗng, noClass false', async () => {
    vi.mocked(gamificationApi.fetchLeaderboard).mockRejectedValue(new Error('Thiếu classId cho tab lớp'));
    const store = useLeaderboardStore();
    await store.fetchBoard('class', 7);
    expect(store.error).toContain('Thiếu classId');
    expect(store.rows).toHaveLength(0);
    expect(store.noClass).toBe(false);
  });
});
