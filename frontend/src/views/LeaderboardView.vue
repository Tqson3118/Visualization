<script setup lang="ts">
// LeaderboardView — Màn 24: 3 tab (Tuần/Level/Lớp) + vị trí của mình + phân trang.
// G-F2d: hero gradient Aurora + trophy, Tabs shadcn, bar chart top 10 (VChartLazy),
// reorder hoạt hình khi đổi tab (TransitionGroup FLIP — tương đương motion-v
// AutoAnimate; global.css đã cắt transition khi prefers-reduced-motion),
// top 3 highlight gradient, dòng "Bạn" ghim cuối bảng, phân trang Button.
import { computed, onMounted } from 'vue';
import { ChevronLeft, ChevronRight, Flame, Trophy } from 'lucide-vue-next';

import type { LeaderboardEntryDto } from '@/api/gamification';
import { useLeaderboardStore } from '@/stores/leaderboard';
import { useGamificationStore } from '@/stores/gamification';
import { useClassStore } from '@/stores/classStore';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import Tabs from '@/components/ui/Tabs.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import VChartLazy from '@/components/ui/VChartLazy.vue';

const board = useLeaderboardStore();
const gamification = useGamificationStore();
const classStore = useClassStore();
const auth = useAuthStore();
const ui = useUiStore();

const tabs: Array<{ key: 'week' | 'level' | 'class'; label: string }> = [
  { key: 'week', label: 'Tuần' },
  { key: 'level', label: 'Level' },
  { key: 'class', label: 'Lớp' },
];

onMounted(() => {
  void board.fetchBoard('week');
  if (gamification.gems === 0 && gamification.hearts === 0) void gamification.fetchAll();
});

async function switchTab(key: string): Promise<void> {
  if (key === 'class') {
    // G-F3E-NEW-2: tab Lớp phải gửi classId — lấy lớp hiện tại; chưa có → EmptyState, không gọi API.
    const classId = await resolveClassId();
    if (classId === null) {
      board.setNoClass();
      return;
    }
    void board.fetchBoard('class', classId);
    return;
  }
  if (key === 'week' || key === 'level') void board.fetchBoard(key);
}

/** Lớp dùng cho tab Lớp: currentClass → lớp đầu tiên user đang tham gia → tải danh sách lớp nếu chưa có. */
async function resolveClassId(): Promise<number | null> {
  if (classStore.currentClass) return classStore.currentClass.id;
  if (classStore.classes.length > 0) return classStore.classes[0].id;
  try {
    await classStore.fetchClasses();
  } catch {
    return null;
  }
  return classStore.classes[0]?.id ?? null;
}

function goToPage(next: number): void {
  if (next < 1 || next > board.totalPages) return;
  // G-F3E2: tab Lớp phân trang phải giữ classId (lastClassId lưu ở store) — nếu không backend
  // 400 "Thiếu classId cho tab lớp" khi lớp >20 thành viên (P2 review g-f3c F2). week/level không cần.
  const classId = board.tab === 'class' ? (board.lastClassId ?? undefined) : undefined;
  void board.fetchBoard(undefined, classId, next);
}

const medal = computed(() => (rank: number) => (rank === 1 ? '🥇' : rank === 2 ? '🥈' : rank === 3 ? '🥉' : ''));

const myUserId = computed(() => auth.user?.id ?? null);
const isMe = (row: LeaderboardEntryDto): boolean => row.userId === myUserId.value;

const valueLabel = computed(() =>
  board.tab === 'level' ? 'XP' : board.tab === 'class' ? 'điểm' : 'XP tuần',
);

const initial = (name: string): string => name.charAt(0).toUpperCase();

const chartRows = computed(() => board.rows.slice(0, 10));

/** Đọc CSS variable thành màu cụ thể (ECharts canvas không hiểu var()). */
function cssVar(name: string, fallback: string): string {
  if (typeof document === 'undefined') return fallback;
  const val = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  return val || fallback;
}

const truncateName = (name: string): string => (name.length > 16 ? `${name.slice(0, 16)}…` : name);

/** Bar chart top 10 — màu rank: vàng/bạc/đồng cho top 3, teal cho còn lại. */
const boardChartOption = computed(() => {
  // Phụ thuộc theme (ui.theme) → recompute option khi toggle sáng/tối
  void ui.theme;
  const textColor = cssVar('--color-text-muted', '#5E7A77');
  const axisColor = cssVar('--color-border', '#cbd5e1');
  const foreground = cssVar('--color-foreground', '#134e4a');
  const cardColor = cssVar('--color-card', '#ffffff');
  const primary = cssVar('--color-primary', '#0d9488');

  const names = chartRows.value.map((row) => truncateName(row.displayName));
  const values = chartRows.value.map((row) => row.value);
  // Màu theo rank: vàng/bạc/đồng cho top 3, teal cho còn lại.
  // KHÔNG để top-level `color` (array đổi độ dài → vue-echarts bỏ vào replaceMerge
  // → echarts 6 lỗi "'color' is not valid component main type" → đặt trong data item.
  const items = chartRows.value.map((row, i) => ({
    value: values[i],
    itemStyle: {
      color: row.rank === 1 ? '#f59e0b' : row.rank === 2 ? '#94a3b8' : row.rank === 3 ? '#d97706' : primary,
      borderRadius: [0, 7, 7, 0],
    },
  }));

  return {
    tooltip: {
      trigger: 'axis' as const,
      backgroundColor: cardColor,
      borderColor: axisColor,
      textStyle: { color: foreground, fontSize: 12 },
      formatter: (params: Array<{ name: string; value: number }>) => {
        const item = params[0];
        return item ? `${item.name}<br/>${valueLabel.value}: <b>${item.value.toLocaleString('vi-VN')}</b>` : '';
      },
    },
    grid: { left: 8, right: 32, top: 8, bottom: 8, containLabel: true },
    xAxis: {
      type: 'value' as const,
      axisLabel: { color: textColor, fontSize: 11 },
      splitLine: { lineStyle: { color: axisColor } },
    },
    yAxis: {
      type: 'category' as const,
      inverse: true,
      data: names,
      axisLabel: { color: textColor, fontSize: 11, width: 120, overflow: 'truncate' },
      axisLine: { lineStyle: { color: axisColor } },
    },
    series: [
      {
        type: 'bar' as const,
        data: items,
        barWidth: 14,
        label: {
          show: true,
          position: 'right' as const,
          color: textColor,
          fontSize: 11,
          formatter: (p: { value: number }) => p.value.toLocaleString('vi-VN'),
        },
      },
    ],
  };
});

// ── E2E selector giữ ổn định (nếu cần): tab = role=tab, danh sách = .board__row ──
</script>

<template>
  <main class="leaderboard container">
    <!-- Hero gradient Aurora (palette 1 — gamification) -->
    <header class="leaderboard__hero">
      <div class="leaderboard__hero-body">
        <span class="leaderboard__hero-icon" aria-hidden="true"><Trophy :size="24" /></span>
        <div class="leaderboard__hero-title-wrap">
          <h1 class="leaderboard__title">Bảng xếp hạng</h1>
          <p class="leaderboard__sub">
            Tuần reset thứ Hai 00:00 (UTC+7) · chứng minh kỹ năng của bạn trên toàn trường
          </p>
        </div>
        <Badge variant="primary" class="leaderboard__hero-badge">🏆 Top learners</Badge>
      </div>
    </header>

    <!-- Tabs shadcn: Tuần / Level / Lớp (giữ logic fetchBoard + phân trang) -->
    <Tabs :tabs="tabs" :model-value="board.tab" @change="switchTab" />

    <!-- Loading lần đầu: chưa có dữ liệu → Skeleton. Đổi tab: giữ bảng cũ để reorder. -->
    <div v-if="board.loading && board.rows.length === 0" class="leaderboard__loading" aria-busy="true">
      <Skeleton v-for="i in 8" :key="i" height="44px" />
    </div>

    <EmptyState
      v-else-if="board.noClass"
      icon="user"
      title="Bạn chưa tham gia lớp học nào"
      description="Nhập mã mời từ giảng viên tại trang Lớp học để xem bảng xếp hạng của lớp."
    />

    <EmptyState
      v-else-if="board.rows.length === 0 && board.error === null"
      icon="target"
      title="Chưa có dữ liệu xếp hạng"
      description="Tuần mới bắt đầu — hãy là người đầu tiên trên bảng!"
    />

    <EmptyState
      v-else-if="board.rows.length === 0"
      icon="alert-circle"
      title="Không tải được bảng xếp hạng"
      :description="board.error ?? ''"
    />

    <section v-else class="leaderboard__board card" :class="{ 'leaderboard__board--busy': board.loading }">
      <!-- Bar chart top 10 (vue-echarts lazy — G-F2d) -->
      <div v-if="chartRows.length > 1" class="leaderboard__chart">
        <h2 class="leaderboard__chart-title">Tổng quan Top {{ chartRows.length }}</h2>
        <VChartLazy :option="boardChartOption" height="248px" />
      </div>

      <div class="leaderboard__table-wrap">
        <!-- TransitionGroup: FLIP move khi đổi tab (key ổn định = userId) -->
        <TransitionGroup name="board-row" tag="ol" class="leaderboard__list">
          <li
            v-for="row in board.rows"
            :key="row.userId"
            class="board-row"
            :class="{
              'board-row--top-1': row.rank === 1,
              'board-row--top-2': row.rank === 2,
              'board-row--top-3': row.rank === 3,
              'board-row--me': isMe(row),
            }"
          >
            <span class="board-row__rank">
              <span v-if="medal(row.rank)" class="board-row__medal" aria-hidden="true">{{ medal(row.rank) }}</span>
              <span class="board-row__rank-num" :class="{ 'board-row__rank-num--top': row.rank <= 3 }">{{ row.rank }}</span>
            </span>

            <span class="board-row__avatar">
              <img v-if="row.avatarUrl" :src="row.avatarUrl" alt="" class="board-row__avatar-img" />
              <span v-else class="board-row__avatar-fallback">{{ initial(row.displayName) }}</span>
            </span>

            <span class="board-row__user">
              <span class="board-row__name">{{ row.displayName }}</span>
              <span v-if="isMe(row)" class="board-row__me-badge">Bạn</span>
            </span>

            <span class="board-row__meta">
              <Badge v-if="board.tab === 'level' && row.level !== undefined" variant="muted" class="board-row__level">
                Lv {{ row.level }}
              </Badge>
              <span v-if="row.streak" class="board-row__streak"><Flame :size="12" /> {{ row.streak }}</span>
            </span>

            <span class="board-row__value">
              {{ row.value.toLocaleString('vi-VN') }}
              <span class="board-row__value-label">{{ valueLabel }}</span>
            </span>
          </li>
        </TransitionGroup>

        <!-- Dòng ghim vị trí của tôi (badge Bạn) -->
        <div
          v-if="board.myRank"
          class="board-row board-row--pinned"
          role="status"
          :aria-label="`Vị trí của bạn: hạng ${board.myRank.rank}`"
        >
          <span class="board-row__pinned-label">Vị trí của bạn</span>
          <span class="board-row__rank">
            <span class="board-row__rank-num">{{ board.myRank.rank }}</span>
          </span>
          <span class="board-row__avatar">
            <span class="board-row__avatar-fallback">{{ initial(board.myRank.displayName) }}</span>
          </span>
          <span class="board-row__user">
            <span class="board-row__name">{{ board.myRank.displayName }}</span>
            <span class="board-row__me-badge">Bạn</span>
          </span>
          <span v-if="board.myRank.streak" class="board-row__meta board-row__meta--pinned">
            <span class="board-row__streak"><Flame :size="12" /> {{ board.myRank.streak }}</span>
          </span>
          <span class="board-row__value">
            {{ board.myRank.value.toLocaleString('vi-VN') }}
            <span class="board-row__value-label">{{ valueLabel }}</span>
          </span>
        </div>
      </div>

      <!-- Phân trang -->
      <div v-if="board.totalPages > 1" class="leaderboard__pager">
        <Button
          variant="secondary"
          size="sm"
          :disabled="board.page <= 1 || board.loading"
          aria-label="Trang trước"
          @click="goToPage(board.page - 1)"
        >
          <ChevronLeft :size="14" /> Trước
        </Button>
        <span class="leaderboard__pager-info" aria-live="polite">
          Trang {{ board.page }} / {{ board.totalPages }}
        </span>
        <Button
          variant="secondary"
          size="sm"
          :disabled="board.page >= board.totalPages || board.loading"
          aria-label="Trang sau"
          @click="goToPage(board.page + 1)"
        >
          Sau <ChevronRight :size="14" />
        </Button>
      </div>
    </section>
  </main>
</template>

<style scoped>
.leaderboard {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 860px;
}

/* ── Hero gradient Aurora (palette 1 — teal → cyan → violet) ── */
.leaderboard__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 32%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-aurora);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
}

.leaderboard__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: color-mix(in srgb, var(--color-background) 58%, transparent);
}

.leaderboard__hero::before {
  content: '';
  position: absolute;
  width: 260px;
  height: 260px;
  border-radius: 50%;
  top: -120px;
  right: -60px;
  z-index: -1;
  background: color-mix(in srgb, var(--color-secondary) 30%, transparent);
  filter: blur(64px);
}

.leaderboard__hero-body {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.leaderboard__hero-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg);
  background-image: var(--gradient-aurora);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-md);
}

.leaderboard__hero-title-wrap { display: flex; flex-direction: column; gap: 4px; }

.leaderboard__title {
  font-size: var(--text-2xl);
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.leaderboard__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

.leaderboard__hero-badge { margin-left: auto; }

.leaderboard__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

.leaderboard__board { display: flex; flex-direction: column; gap: var(--space-md); padding: var(--space-lg); }

/* Mờ nhẹ + chặn click khi đang đổi tab (giữ bảng cũ → reorder animation vẫn thấy) */
.leaderboard__board--busy { opacity: 0.55; pointer-events: none; }

.leaderboard__chart {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-surface);
  padding: var(--space-md);
}

.leaderboard__chart-title {
  font-size: var(--text-sm);
  font-weight: 700;
  color: var(--color-text-muted);
  margin-bottom: var(--space-xs);
}

.leaderboard__table-wrap {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  min-width: 0;
}

.leaderboard__list { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; }

/* ── Reorder FLIP (tương đương AutoAnimate) ── */
.board-row-move { transition: transform 0.45s cubic-bezier(0.22, 1, 0.36, 1); }
.board-row-enter-active { transition: opacity 0.3s ease, transform 0.3s ease; }
.board-row-enter-from { opacity: 0; transform: translateY(-8px); }
.board-row-leave-active { transition: opacity 0.2s ease; }
.board-row-leave-to { opacity: 0; }

.board-row {
  position: relative;
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: 0.75rem var(--space-md);
  font-size: var(--text-sm);
  border-bottom: 1px solid var(--color-border);
  border-left: 3px solid transparent;
  transition: background-color 180ms ease;
}

/* Top 3 — highlight gradient (overlay mờ giữ text đọc được) */
.board-row--top-1::before,
.board-row--top-2::before,
.board-row--top-3::before {
  content: '';
  position: absolute;
  inset: 0;
  pointer-events: none;
  z-index: 0;
}

.board-row--top-1 { border-left-color: var(--color-warning); }
.board-row--top-1::before { background-image: var(--gradient-sunset); opacity: 0.18; }

.board-row--top-2 { border-left-color: color-mix(in srgb, var(--color-secondary) 60%, var(--color-border)); }
.board-row--top-2::before { background-image: var(--gradient-aurora); opacity: 0.1; }

.board-row--top-3 { border-left-color: color-mix(in srgb, var(--color-accent) 60%, var(--color-border)); }
.board-row--top-3::before { background-image: var(--gradient-mint); opacity: 0.12; }

.board-row > * { position: relative; z-index: 1; }

.board-row--me { background: color-mix(in srgb, var(--color-primary) 9%, var(--color-surface)); }

.board-row__rank { display: flex; align-items: center; gap: 4px; width: 76px; flex-shrink: 0; font-family: var(--font-mono); }
.board-row__medal { font-size: var(--text-base); }
.board-row__rank-num { font-weight: 800; }
.board-row__rank-num--top { color: var(--color-warning); }

.board-row__avatar { display: inline-flex; flex-shrink: 0; }
.board-row__avatar-fallback {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  background: var(--color-muted);
  color: var(--color-text-muted);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: var(--text-xs);
}
.board-row__avatar-img { width: 34px; height: 34px; border-radius: 50%; object-fit: cover; }

.board-row__user { display: flex; align-items: center; gap: var(--space-sm); min-width: 0; flex: 1; }
.board-row__name { font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

.board-row__me-badge {
  font-size: 10px;
  font-weight: 700;
  background-image: var(--gradient-aurora);
  color: var(--color-on-primary);
  padding: 1px 8px;
  border-radius: var(--radius-full);
  flex-shrink: 0;
}

.board-row__meta { display: flex; align-items: center; gap: var(--space-xs); width: 96px; flex-shrink: 0; }
.board-row__streak { display: inline-flex; align-items: center; gap: 2px; font-size: var(--text-xs); font-weight: 600; color: var(--color-warning); white-space: nowrap; }

.board-row__value { font-weight: 700; text-align: right; white-space: nowrap; margin-left: auto; }
.board-row__value-label { font-weight: 400; color: var(--color-text-muted); font-size: var(--text-xs); }

/* Dòng ghim "Bạn" cuối bảng */
.board-row--pinned {
  margin-top: var(--space-xs);
  border: 1px solid color-mix(in srgb, var(--color-primary) 35%, var(--color-border));
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-primary) 6%, var(--color-surface));
  box-shadow: var(--shadow-sm);
}
.board-row__pinned-label {
  position: absolute;
  top: -9px;
  left: var(--space-md);
  padding: 0 6px;
  font-size: 10px;
  font-weight: 700;
  letter-spacing: 0.02em;
  color: var(--color-primary);
  background: var(--color-card);
  border-radius: var(--radius-sm);
  z-index: 2;
}
.board-row__meta--pinned { justify-content: flex-end; }

.leaderboard__pager {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-md);
  padding-top: var(--space-xs);
  border-top: 1px solid var(--color-border);
}

.leaderboard__pager-info { font-size: var(--text-sm); color: var(--color-text-muted); font-variant-numeric: tabular-nums; }

@media (max-width: 640px) {
  .leaderboard__chart { display: none; } /* bar chart chỉ hiển thị ≥ md — tránh chật/overflow */
  .board-row__meta { width: auto; }
  .board-row__value-label { display: none; }
  .board-row__pinned-label { display: none; }
}
</style>
