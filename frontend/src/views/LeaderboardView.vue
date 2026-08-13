<script setup lang="ts">
// LeaderboardView — Màn 24: 3 tab (Tuần/Level/Lớp) + vị trí của mình + phân trang.
// View-quality C (DESIGN.md §1/§6): hero = surface band level-2 (không gradient/blob),
// rank = block-token tối canvas-ink + index mono header, chart top-10 = vùng dữ liệu LUÔN tối,
// reorder TransitionGroup easing chuẩn enter/exit, EmptyState chung + copy §9 + nút retry/CTA.
import { computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { ChevronLeft, ChevronRight, Crown, Flame, Trophy } from 'lucide-vue-next';

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
import BlockToken from '@/components/ui/BlockToken.vue';
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

/** Top 3 podium — chỉ ở trang 1; lọc rank ≤ 3 trong dữ liệu hiện tại (đã sort theo rank). */
const podiumRows = computed(() => {
  if (board.page !== 1) return [];
  return board.rows.filter((row) => row.rank >= 1 && row.rank <= 3).slice(0, 3);
});

/** index mono 2 chữ số cho podium/rank chip (signature "dữ liệu luôn được đánh số") */
const pad = (n: number): string => String(n).padStart(2, '0');

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
      <!-- Tab switch: content slide ngang (out-in, easing chuẩn §7) — key = tab -->
      <Transition name="lb-panel" mode="out-in">
        <div :key="board.tab" class="leaderboard__panel">
          <!-- Podium top 3 — 3 card nổi bật (gold/silver/bronze glow border + avatar + rank badge) -->
          <div v-if="podiumRows.length > 0" class="leaderboard__podium" role="list" aria-label="Ba vị trí dẫn đầu">
            <article
              v-for="entry in podiumRows"
              :key="entry.userId"
              role="listitem"
              class="podium-card"
              :class="`podium-card--${entry.rank}`"
            >
              <span class="podium-card__rank" :class="`podium-card__rank--${entry.rank}`" aria-hidden="true">
                <Crown v-if="entry.rank === 1" :size="15" />
                <template v-else>{{ pad(entry.rank) }}</template>
              </span>
              <span class="podium-card__avatar" :class="{ 'podium-card__avatar--crown': entry.rank === 1 }">
                <img v-if="entry.avatarUrl" :src="entry.avatarUrl" alt="" />
                <span v-else>{{ initial(entry.displayName) }}</span>
              </span>
              <h3 class="podium-card__name">{{ truncateName(entry.displayName) }}</h3>
              <p class="podium-card__value">
                {{ entry.value.toLocaleString('vi-VN') }}
                <span class="podium-card__unit">{{ valueLabel }}</span>
              </p>
            </article>
          </div>

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

            <!-- Sticky bar "Vị trí của tôi" — glow accent, dính đáy viewport khi cuộn -->
            <div
              v-if="board.myRank"
              class="board-row board-row--pinned"
              role="status"
              :aria-label="`Vị trí của bạn: hạng ${board.myRank.rank}`"
            >
              <span class="board-row__pinned-label">Vị trí của bạn</span>
              <span class="board-row__rank board-row__rank--pinned">
                <BlockToken size="sm" tone="warning" glow :value="board.myRank.rank" index="HẠNG" />
              </span>
              <span class="board-row__avatar">
                <img
                  v-if="board.myRank.avatarUrl"
                  :src="board.myRank.avatarUrl"
                  alt=""
                  class="board-row__avatar-img"
                />
                <span v-else class="board-row__avatar-fallback">{{ initial(board.myRank.displayName) }}</span>
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
        </div>
      </Transition>
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

/* ── Tab switch: slide ngang (DESIGN.md §7 — enter expo-out / exit expo-in) ── */
.lb-panel-enter-active {
  transition: opacity 260ms cubic-bezier(0.16, 1, 0.3, 1), transform 260ms cubic-bezier(0.16, 1, 0.3, 1);
}

.lb-panel-leave-active {
  transition: opacity 180ms cubic-bezier(0.7, 0, 0.84, 0), transform 180ms cubic-bezier(0.7, 0, 0.84, 0);
}

.lb-panel-enter-from { opacity: 0; transform: translateX(28px); }
.lb-panel-leave-to { opacity: 0; transform: translateX(-28px); }

/* ── Podium top 3 — gold/silver/bronze từ palette 6 màu (KHÔNG màu mới) ── */
.leaderboard__podium {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: var(--space-sm);
  align-items: end;
}

.podium-card {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
  text-align: center;
  padding: var(--space-md) var(--space-sm) var(--space-sm);
  border-radius: var(--radius-lg);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  min-width: 0;
  transition:
    transform 150ms var(--ease-out-expo),
    border-color 150ms var(--ease-out-expo),
    box-shadow 150ms var(--ease-out-expo);
}

/* Thứ tự podium: 2 · 1 · 3 (vàng ở giữa, nổi cao hơn) */
.podium-card--2 { order: 1; }
.podium-card--1 { order: 2; padding-bottom: var(--space-lg); }
.podium-card--3 { order: 3; }

/* Viền glow theo hạng — nguồn palette 6 màu (gold=warning, silver=index-muted, bronze=mix warning+data-core) */
.podium-card--1 {
  border-color: color-mix(in srgb, var(--color-warning) 50%, var(--color-border));
  box-shadow:
    0 0 0 3px color-mix(in srgb, var(--color-warning) 22%, transparent),
    0 0 18px color-mix(in srgb, var(--color-warning) 26%, transparent);
}

.podium-card--2 {
  border-color: color-mix(in srgb, var(--color-index-muted) 50%, var(--color-border));
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-index-muted) 18%, transparent);
}

.podium-card--3 {
  border-color: color-mix(in srgb, var(--color-primary) 45%, var(--color-border));
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-primary) 16%, transparent);
}

.podium-card:hover {
  transform: translateY(-2px);
  border-color: var(--color-border-strong);
}

.podium-card__rank {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 26px;
  height: 26px;
  padding: 0 6px;
  border-radius: var(--radius-full);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 600;
  background: var(--color-muted);
  color: var(--color-text-secondary);
}

.podium-card__rank--1 {
  background: color-mix(in srgb, var(--color-warning) 14%, var(--color-muted));
  color: var(--color-warning);
}

.podium-card__rank--2 {
  background: color-mix(in srgb, var(--color-index-muted) 18%, var(--color-muted));
  color: var(--color-index-muted);
}

.podium-card__rank--3 {
  background: color-mix(in srgb, var(--color-primary) 12%, var(--color-muted));
  color: var(--color-primary);
}

.podium-card__avatar {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-full);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  font-weight: 600;
  font-size: var(--text-sm);
  background: var(--color-muted);
  color: var(--color-text-secondary);
  border: 2px solid var(--color-border);
  flex-shrink: 0;
}

.podium-card__avatar--crown {
  width: 56px;
  height: 56px;
  border-color: color-mix(in srgb, var(--color-warning) 55%, transparent);
  box-shadow: 0 0 14px color-mix(in srgb, var(--color-warning) 40%, transparent);
}

.podium-card__avatar img { width: 100%; height: 100%; object-fit: cover; }

.podium-card__name {
  font-size: var(--text-sm);
  font-weight: 600;
  letter-spacing: -0.01em;
  margin: 0;
  max-width: 100%;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.podium-card__value {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-foreground);
  font-variant-numeric: tabular-nums;
  margin: 0;
  white-space: nowrap;
}

.podium-card__unit {
  font-size: var(--text-xs);
  font-weight: 400;
  color: var(--color-text-tertiary);
}

/* Podium enter — stagger nhẹ (chỉ transform + opacity; backwards để hover transform vẫn chạy) */
.podium-card {
  animation: podium-enter 320ms var(--ease-out-expo) backwards;
}

.podium-card:nth-child(1) { animation-delay: 40ms; }
.podium-card:nth-child(2) { animation-delay: 120ms; }
.podium-card:nth-child(3) { animation-delay: 200ms; }

@keyframes podium-enter {
  from { opacity: 0; transform: translateY(10px); }
  to { opacity: 1; transform: translateY(0); }
}

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

/* Hover highlight row — tint nhẹ primary (interactive accent, không shadow) */
.board-row:not(.board-row--pinned):hover {
  background-color: color-mix(in srgb, var(--color-primary) 6%, var(--color-surface));
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
  transition: box-shadow 150ms var(--ease-out-expo);
}

.board-row__rank-chip--top {
  color: var(--color-warning);
  border-color: color-mix(in srgb, var(--color-warning) 40%, transparent);
}

/* Glow nhẹ cho rank top 1–3 (glow theo trạng thái — palette 6 màu) */
.board-row--top-1 .board-row__rank-chip {
  box-shadow: 0 0 12px color-mix(in srgb, var(--color-warning) 35%, transparent);
}

.board-row--top-2 .board-row__rank-chip {
  box-shadow: 0 0 10px color-mix(in srgb, var(--color-index-muted) 30%, transparent);
}

.board-row--top-3 .board-row__rank-chip {
  box-shadow: 0 0 10px color-mix(in srgb, var(--color-primary) 30%, transparent);
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

/* Dòng ghim "Bạn" — STICKY bar đáy viewport + glow accent (interactive, DESIGN §7) */
.board-row--pinned {
  position: sticky;
  bottom: var(--space-sm);
  z-index: 6;
  margin-top: var(--space-xs);
  border: 1px solid color-mix(in srgb, var(--color-primary) 40%, var(--color-border));
  border-radius: var(--radius-md);
  background: color-mix(in srgb, var(--color-primary) 6%, var(--color-surface));
  box-shadow: var(--glow-primary);
}

.board-row__rank--pinned {
  width: auto;
  flex-shrink: 0;
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
  .board-row__rank--pinned .ui-blocktoken { min-width: 0; padding-inline: 8px; }
}

@media (prefers-reduced-motion: reduce) {
  .lb-panel-enter-active,
  .lb-panel-leave-active,
  .board-row-move,
  .board-row-enter-active,
  .board-row-leave-active {
    transition: none;
  }
  .podium-card { animation: none; }
  .board-row { transition: none; }
}
</style>
