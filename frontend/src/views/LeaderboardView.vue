<script setup lang="ts">
// LeaderboardView — Màn 24: 3 tab (Tuần/Level/Lớp) + vị trí của mình + phân trang.
// View-quality C (DESIGN.md §1/§6): hero = surface band level-2 (không gradient/blob),
// rank = block-token tối canvas-ink + index mono header, chart top-10 = vùng dữ liệu LUÔN tối,
// reorder TransitionGroup easing chuẩn enter/exit, EmptyState chung + copy §9 + nút retry/CTA.
import { computed, onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';
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
import { messages } from '@/i18n/vi';

const board = useLeaderboardStore();
const gamification = useGamificationStore();
const classStore = useClassStore();
const auth = useAuthStore();
const ui = useUiStore();
const router = useRouter();

const tabs: Array<{ key: 'week' | 'level' | 'class'; label: string }> = [
  { key: 'week', label: 'Tuần' },
  { key: 'level', label: 'Level' },
  { key: 'class', label: 'Lớp' },
];

const selectedClassId = ref<number | null>(null);

onMounted(async () => {
  void board.fetchBoard('week');
  if (gamification.gems === 0 && gamification.hearts === 0) void gamification.fetchAll();
  if (auth.isAuthenticated) {
    try {
      await classStore.fetchClasses();
      if (classStore.classes.length > 0) {
        selectedClassId.value = classStore.classes[0].id;
      }
    } catch {
      // Bỏ qua lỗi tải lớp nếu chưa có
    }
  }
});

async function switchTab(key: string): Promise<void> {
  if (key === 'class') {
    const classId = await resolveClassId();
    if (classId === null) {
      board.setNoClass();
      return;
    }
    selectedClassId.value = classId;
    void board.fetchBoard('class', classId);
    return;
  }
  if (key === 'week' || key === 'level') void board.fetchBoard(key);
}

function onClassChange(): void {
  if (selectedClassId.value) {
    void board.fetchBoard('class', selectedClassId.value);
  }
}

/** Lớp dùng cho tab Lớp: currentClass → lớp đầu tiên user đang tham gia → tải danh sách lớp nếu chưa có. */
async function resolveClassId(): Promise<number | null> {
  if (selectedClassId.value) return selectedClassId.value;
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

function retryBoard(): void {
  const classId = board.tab === 'class' ? (board.lastClassId ?? undefined) : undefined;
  void board.fetchBoard(undefined, classId, 1);
}

function goClasses(): void {
  void router.push({ name: 'classes' });
}

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

/** Bar chart top 10 — nền LUÔN tối (canvas-ink, vùng dữ liệu); màu rank: warning/index-muted/accent, còn lại data-core. */
const boardChartOption = computed(() => {
  // Phụ thuộc theme (ui.theme) → recompute option khi toggle sáng/tối
  void ui.theme;
  const indexMuted = cssVar('--color-index-muted', '#6B7385');
  const dataCore = cssVar('--color-data-core', '#4255FF');
  const ink = cssVar('--color-canvas-ink', '#0D1020');
  const warning = cssVar('--color-warning', '#D97706');
  const accent = cssVar('--color-accent', '#D97706');

  const names = chartRows.value.map((row) => truncateName(row.displayName));
  const values = chartRows.value.map((row) => row.value);
  // Màu theo rank: warning/index-muted/accent cho top 3, data-core cho còn lại.
  // KHÔNG để top-level `color` (array đổi độ dài → vue-echarts bỏ vào replaceMerge
  // → echarts 6 lỗi "'color' is not valid component main type" → đặt trong data item).
  const items = chartRows.value.map((row, i) => ({
    value: values[i],
    itemStyle: {
      color: row.rank === 1 ? warning : row.rank === 2 ? indexMuted : row.rank === 3 ? accent : dataCore,
      borderRadius: [0, 8, 8, 0],
    },
  }));

  return {
    tooltip: {
      trigger: 'axis' as const,
      backgroundColor: ink,
      borderColor: indexMuted,
      textStyle: { color: indexMuted, fontSize: 12 },
      formatter: (params: Array<{ name: string; value: number }>) => {
        const item = params[0];
        return item ? `${item.name}<br/>${valueLabel.value}: <b>${item.value.toLocaleString('vi-VN')}</b>` : '';
      },
    },
    grid: { left: 8, right: 32, top: 8, bottom: 8, containLabel: true },
    xAxis: {
      type: 'value' as const,
      axisLabel: { color: indexMuted, fontSize: 11 },
      splitLine: { lineStyle: { color: indexMuted, opacity: 0.3 } },
    },
    yAxis: {
      type: 'category' as const,
      inverse: true,
      data: names,
      axisLabel: { color: indexMuted, fontSize: 11, width: 120, overflow: 'truncate' },
      axisLine: { lineStyle: { color: indexMuted } },
    },
    series: [
      {
        type: 'bar' as const,
        data: items,
        barWidth: 14,
        label: {
          show: true,
          position: 'right' as const,
          color: indexMuted,
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
    <!-- Hero — surface band level-2 + strip mono dữ liệu (không gradient, không blob) -->
    <header class="leaderboard__hero">
      <div class="leaderboard__hero-body">
        <span class="leaderboard__hero-icon" aria-hidden="true"><Trophy :size="20" /></span>
        <div class="leaderboard__hero-title-wrap">
          <h1 class="leaderboard__title">Bảng xếp hạng</h1>
          <p class="leaderboard__sub">
            Tuần reset thứ Hai 00:00 (UTC+7) · chứng minh kỹ năng của bạn trên toàn trường
          </p>
        </div>
        <span class="leaderboard__hero-strip" aria-hidden="true">
          <span class="leaderboard__strip-block" />
          TOP {{ board.rows.length > 0 ? board.rows.length : '—' }} · {{ valueLabel }}
        </span>
      </div>
    </header>

    <!-- Tabs shadcn: Tuần / Level / Lớp (giữ logic fetchBoard + phân trang) -->
    <Tabs :tabs="tabs" :model-value="board.tab" @change="switchTab" />

    <!-- Class Selector Dropdown for Tab 'class' -->
    <div
      v-if="board.tab === 'class' && classStore.classes.length > 0"
      class="flex items-center gap-3 p-3 rounded-xl bg-vdsa-surface border border-vdsa-border -mt-2 mb-2"
    >
      <span class="text-xs font-bold text-vdsa-secondary uppercase">Chọn Lớp học:</span>
      <select
        v-model="selectedClassId"
        class="bg-vdsa-bg border border-vdsa-border rounded-lg px-3 py-1.5 text-xs text-white focus:outline-none focus:border-accent cursor-pointer"
        @change="onClassChange"
      >
        <option v-for="c in classStore.classes" :key="c.id" :value="c.id">
          {{ c.name }} ({{ c.inviteCode }})
        </option>
      </select>
    </div>

    <!-- Loading lần đầu: chưa có dữ liệu → Skeleton. Đổi tab: giữ bảng cũ để reorder. -->
    <div v-if="board.loading && board.rows.length === 0" class="leaderboard__loading" aria-busy="true">
      <Skeleton v-for="i in 8" :key="i" height="44px" />
    </div>

    <EmptyState
      v-else-if="board.noClass"
      icon="user"
      title="Bạn chưa tham gia lớp học nào"
      description="Nhập mã mời từ giảng viên tại trang Lớp học để xem bảng xếp hạng của lớp."
      :action-label="messages.leaderboard.goClasses"
      @action="goClasses"
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
      :action-label="messages.leaderboard.retry"
      @action="retryBoard"
    />

    <section v-else class="leaderboard__board card" :class="{ 'leaderboard__board--busy': board.loading }">
      <!-- Bar chart top 10 (vue-echarts lazy) — vùng dữ liệu LUÔN tối canvas-ink -->
      <div v-if="chartRows.length > 1" class="leaderboard__chart">
        <h2 class="leaderboard__chart-title">TỔNG QUAN TOP {{ chartRows.length }}</h2>
        <VChartLazy :option="boardChartOption" height="248px" />
      </div>

      <div class="leaderboard__table-wrap">
        <!-- Index mono header (signature "dữ liệu luôn được đánh số") -->
        <div class="leaderboard__list-head" aria-hidden="true">
          <span>RANK</span>
          <span class="leaderboard__list-head-user">USER</span>
          <span>VALUE</span>
        </div>

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
              <span class="board-row__rank-chip" :class="{ 'board-row__rank-chip--top': row.rank <= 3 }">
                {{ row.rank }}
              </span>
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
            <span class="board-row__rank-chip">{{ board.myRank.rank }}</span>
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

/* Card dùng class global .card (global.css có shadow-md) — §6 cấm shadow card → override */
.leaderboard .card {
  box-shadow: none;
}

/* ── Hero — surface band level-2 (DESIGN.md §6) ── */
.leaderboard__hero {
  display: flex;
  flex-direction: column;
  gap: var(--space-md);
  padding: var(--space-lg) var(--space-xl);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  background: var(--color-card-raised);
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
  background: var(--color-muted);
  color: var(--color-text-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.leaderboard__hero-title-wrap { display: flex; flex-direction: column; gap: var(--space-xs); }

.leaderboard__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
  margin: 0;
}

.leaderboard__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

/* Strip mono dữ liệu — block-token nhỏ bên phải hero */
.leaderboard__hero-strip {
  margin-left: auto;
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  background: var(--color-canvas-ink);
  border: 1px solid color-mix(in srgb, var(--color-data-core) 25%, transparent);
  border-radius: var(--radius-md);
  padding: var(--space-xs) var(--space-sm);
  white-space: nowrap;
}

.leaderboard__strip-block {
  width: 8px;
  height: 8px;
  border-radius: var(--radius-sm);
  background: var(--color-data-core);
}

.leaderboard__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

.leaderboard__board { display: flex; flex-direction: column; gap: var(--space-md); padding: var(--space-lg); }

/* Mờ nhẹ + chặn click khi đang đổi tab (giữ bảng cũ → reorder animation vẫn thấy) */
.leaderboard__board--busy { opacity: 0.55; pointer-events: none; }

/* Chart = vùng dữ liệu → LUÔN tối canvas-ink bất kể theme */
.leaderboard__chart {
  border: 1px solid color-mix(in srgb, var(--color-data-core) 20%, transparent);
  border-radius: var(--radius-lg);
  background: var(--color-canvas-ink);
  padding: var(--space-md);
}

.leaderboard__chart-title {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-text-tertiary);
  margin-bottom: var(--space-xs);
}

.leaderboard__table-wrap {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  min-width: 0;
}

/* Index mono header (signature "dữ liệu luôn được đánh số") */
.leaderboard__list-head {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-xs) var(--space-md);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}

.leaderboard__list-head > span:first-child { width: 44px; flex-shrink: 0; }
.leaderboard__list-head-user { flex: 1; }
.leaderboard__list-head > span:last-child { margin-left: auto; }

.leaderboard__list { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; }

/* ── Reorder FLIP — easing chuẩn DESIGN.md §7 (enter 0.16,1,0.3,1 / exit 0.7,0,0.84,0) ── */
.board-row-move { transition: transform 300ms cubic-bezier(0.16, 1, 0.3, 1); }
.board-row-enter-active { transition: opacity 250ms cubic-bezier(0.16, 1, 0.3, 1), transform 250ms cubic-bezier(0.16, 1, 0.3, 1); }
.board-row-enter-from { opacity: 0; transform: translateY(-8px); }
.board-row-leave-active { transition: opacity 200ms cubic-bezier(0.7, 0, 0.84, 0); }
.board-row-leave-to { opacity: 0; }

.board-row {
  position: relative;
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-sm);
  border-bottom: 1px solid var(--color-border);
  border-left: 3px solid transparent;
  transition: background-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

/* Top 3 — phân cấp bằng border màu (không gradient overlay, không shadow) */
.board-row--top-1 { border-left-color: var(--color-warning); }
.board-row--top-2 { border-left-color: color-mix(in srgb, var(--color-secondary) 60%, var(--color-border)); }
.board-row--top-3 { border-left-color: color-mix(in srgb, var(--color-primary) 60%, var(--color-border)); }

.board-row--me { background: color-mix(in srgb, var(--color-primary) 9%, var(--color-surface)); }

/* Rank = block-token tối (vùng dữ liệu) + index mono */
.board-row__rank { display: flex; align-items: center; width: 44px; flex-shrink: 0; }

.board-row__rank-chip {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 32px;
  border: 1px solid color-mix(in srgb, var(--color-data-core) 25%, transparent);
  border-radius: var(--radius-md);
  background: var(--color-canvas-ink);
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-index-muted);
  font-variant-numeric: tabular-nums;
}

.board-row__rank-chip--top {
  color: var(--color-warning);
  border-color: color-mix(in srgb, var(--color-warning) 40%, transparent);
}

.board-row__avatar { display: inline-flex; flex-shrink: 0; }
.board-row__avatar-fallback {
  width: 34px;
  height: 34px;
  border-radius: var(--radius-full);
  background: var(--color-muted);
  color: var(--color-text-muted);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 600;
  font-size: var(--text-xs);
}
.board-row__avatar-img { width: 34px; height: 34px; border-radius: var(--radius-full); object-fit: cover; }

.board-row__user { display: flex; align-items: center; gap: var(--space-sm); min-width: 0; flex: 1; }
.board-row__name { font-weight: 600; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

.board-row__me-badge {
  font-size: var(--text-xs);
  font-weight: 500;
  background: var(--color-primary);
  color: var(--color-on-primary);
  padding: var(--space-xs) var(--space-sm);
  border-radius: var(--radius-full);
  flex-shrink: 0;
}

.board-row__meta { display: flex; align-items: center; gap: var(--space-xs); width: 96px; flex-shrink: 0; }
.board-row__streak { display: inline-flex; align-items: center; gap: var(--space-xs); font-size: var(--text-xs); font-weight: 600; color: var(--color-warning); white-space: nowrap; }

.board-row__value { font-family: var(--font-mono); font-weight: 600; text-align: right; white-space: nowrap; margin-left: auto; font-variant-numeric: tabular-nums; }
.board-row__value-label { font-weight: 400; color: var(--color-text-muted); font-size: var(--text-xs); }

/* Dòng ghim "Bạn" cuối bảng — không shadow (chỉ border + tint) */
.board-row--pinned {
  margin-top: var(--space-xs);
  border: 1px solid color-mix(in srgb, var(--color-primary) 35%, var(--color-border));
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-primary) 6%, var(--color-surface));
}
.board-row__pinned-label {
  position: absolute;
  top: -9px;
  left: var(--space-md);
  padding: 0 6px;
  font-size: var(--text-xs);
  font-weight: 600;
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

.leaderboard__pager-info { font-family: var(--font-mono); font-size: var(--text-sm); color: var(--color-text-muted); font-variant-numeric: tabular-nums; }

@media (max-width: 640px) {
  .leaderboard__chart { display: none; } /* bar chart chỉ hiển thị ≥ md — tránh chật/overflow */
  .board-row__meta { width: auto; }
  .board-row__value-label { display: none; }
  .board-row__pinned-label { display: none; }
}
</style>
