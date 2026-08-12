<script setup lang="ts">
// LeaderboardView — Màn 24: 3 tab (Tuần/Level/Lớp) + vị trí của mình + phân trang
import { computed, onMounted, ref } from 'vue';

import { useLeaderboardStore } from '@/stores/leaderboard';
import { useGamificationStore } from '@/stores/gamification';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const board = useLeaderboardStore();
const gamification = useGamificationStore();

const tabs = [
  { key: 'week', label: 'Tuần' },
  { key: 'level', label: 'Level' },
  { key: 'class', label: 'Lớp' },
] as const;

const page = ref(1);

onMounted(() => {
  void board.fetchBoard('week');
  if (gamification.gems === 0 && gamification.hearts === 0) void gamification.fetchAll();
});

async function switchTab(key: 'week' | 'level' | 'class'): Promise<void> {
  page.value = 1;
  await board.fetchBoard(key);
}

const medal = computed(() => (rank: number) => (rank === 1 ? '🥇' : rank === 2 ? '🥈' : rank === 3 ? '🥉' : ''));
</script>

<template>
  <main class="leaderboard container">
    <header class="leaderboard__header">
      <h1 class="leaderboard__title">🏆 Bảng xếp hạng</h1>
      <p class="text-muted leaderboard__sub">Tab Tuần reset thứ Hai 00:00 (UTC+7)</p>
    </header>

    <div class="leaderboard__tabs">
      <button
        v-for="tab in tabs"
        :key="tab.key"
        type="button"
        class="leaderboard__tab"
        :class="{ 'leaderboard__tab--active': board.tab === tab.key }"
        @click="switchTab(tab.key)"
      >
        {{ tab.label }}
      </button>
    </div>

    <div v-if="board.loading" class="leaderboard__loading">
      <Skeleton v-for="i in 8" :key="i" height="40px" />
    </div>

    <EmptyState
      v-else-if="board.rows.length === 0"
      icon="trophy"
      title="Chưa có dữ liệu xếp hạng"
      description="Tuần mới bắt đầu — hãy là người đầu tiên trên bảng!"
    />

    <div v-else class="leaderboard__table card">
      <table class="leaderboard__rows">
        <tbody>
          <tr
            v-for="row in board.rows"
            :key="row.userId"
            class="leaderboard__row"
            :class="{ 'leaderboard__row--me': row.userId === 0 }"
          >
            <td class="leaderboard__rank">
              <span class="leaderboard__medal">{{ medal(row.rank) }}</span>
              {{ row.rank }}
            </td>
            <td class="leaderboard__user">
              <span class="leaderboard__avatar">{{ row.displayName.charAt(0).toUpperCase() }}</span>
              {{ row.displayName }}
              <span v-if="row.userId === 0" class="leaderboard__me-badge">Bạn</span>
            </td>
            <td class="leaderboard__value">
              {{ row.value.toLocaleString('vi-VN') }}
              <span class="leaderboard__value-label">
                {{ board.tab === 'level' ? 'XP' : board.tab === 'class' ? 'điểm' : 'XP tuần' }}
              </span>
            </td>
            <td v-if="row.streak" class="leaderboard__streak">🔥 {{ row.streak }}</td>
          </tr>
        </tbody>
      </table>

      <!-- Dòng ghim vị trí của tôi -->
      <div v-if="board.myRank" class="leaderboard__pinned">
        <span class="leaderboard__rank">{{ board.myRank.rank }}</span>
        <span class="leaderboard__user">{{ board.myRank.displayName }} <em>(bạn)</em></span>
        <span class="leaderboard__value">{{ board.myRank.value.toLocaleString('vi-VN') }}</span>
      </div>
    </div>
  </main>
</template>

<style scoped>
.leaderboard {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 820px;
}

.leaderboard__title { font-size: var(--text-2xl); }
.leaderboard__sub { font-size: var(--text-sm); margin-top: 4px; }

.leaderboard__tabs {
  display: flex;
  gap: var(--space-xs);
  border-bottom: 2px solid var(--color-border);
}

.leaderboard__tab {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  padding: var(--space-sm) var(--space-lg);
  font-weight: 700;
  color: var(--color-text-muted);
  cursor: pointer;
  margin-bottom: -2px;
}

.leaderboard__tab--active { color: var(--color-primary); border-bottom-color: var(--color-primary); }

.leaderboard__rows { width: 100%; border-collapse: collapse; }

.leaderboard__row td { padding: var(--space-sm) var(--space-md); border-bottom: 1px solid var(--color-border); font-size: var(--text-sm); }

.leaderboard__row--me { background: color-mix(in srgb, var(--color-primary) 8%, transparent); }

.leaderboard__rank { font-weight: 800; width: 72px; font-family: var(--font-mono); }
.leaderboard__medal { margin-right: 4px; }

.leaderboard__user { display: flex; align-items: center; gap: var(--space-sm); font-weight: 600; }
.leaderboard__avatar {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  background: var(--color-muted);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: var(--text-xs);
  flex-shrink: 0;
}

.leaderboard__me-badge {
  font-size: 10px;
  background: var(--color-primary);
  color: var(--color-on-primary);
  padding: 1px 8px;
  border-radius: var(--radius-full);
}

.leaderboard__value { font-weight: 700; text-align: right; white-space: nowrap; }
.leaderboard__value-label { font-weight: 400; color: var(--color-text-muted); font-size: var(--text-xs); }

.leaderboard__streak { width: 80px; text-align: center; }

.leaderboard__pinned {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-sm) var(--space-md);
  background: var(--color-surface-hover);
  font-size: var(--text-sm);
  font-weight: 600;
  border-top: 2px solid var(--color-border);
}

.leaderboard__pinned em { font-style: normal; color: var(--color-primary); }
</style>
