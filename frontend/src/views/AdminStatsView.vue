<script setup lang="ts">
// AdminStatsView — Màn 11: 7 KPI (1 Hero + 6 Level-1) + bar chart ECharts (VChartLazy, 7 ngày)
// + donut SVG tự vẽ (phân bố vai trò).
// View-quality 14/08 (Nhóm D): banner surface band + mono strip block-token;
// 1 hero-stat (block-token tối) + 6 KPI level-1; 2 vùng biểu đồ LUÔN tối
// canvas-ink, palette đọc CSS var (data-core/index-muted/canvas-ink) + text
// #d9dde8 (engine canvasTheme.ts — decision log 14/08); error state + retry.
import { computed, onMounted, ref } from 'vue';
import { Activity, Info, RefreshCw, Users } from 'lucide-vue-next';

import * as adminApi from '@/api/admin';
import type { AdminStatsDto } from '@/api/admin';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
} from '@/components/ui/card';
import AdminNav from '@/components/admin/AdminNav.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/Button.vue';
import VChartLazy from '@/components/ui/VChartLazy.vue';
import { formatNumber } from '@/utils/format';

const ui = useUiStore();
const stats = ref<AdminStatsDto | null>(null);
const loading = ref(true);
const loadError = ref(false);

onMounted(load);

async function load(): Promise<void> {
  loading.value = true;
  loadError.value = false;
  try {
    stats.value = await adminApi.fetchStats();
  } catch {
    loadError.value = true;
  } finally {
    loading.value = false;
  }
}

/** Đọc CSS variable thành màu cụ thể (SVG/echarts không phụ thuộc var() runtime). */
function cssVar(name: string, fallback: string): string {
  if (typeof document === 'undefined') return fallback;
  const val = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  return val || fallback;
}

// ── 7 KPI (1 Hero: totalUsers + 6 Level-1: totalLessons, totalExercises, totalSimulations, activeUsersToday, totalOrders, totalRevenue) ──
const KPIS: Array<{ key: keyof AdminStatsDto; label: string }> = [
  { key: 'totalLessons', label: messages.admin.stats.totalLessons },
  { key: 'totalExercises', label: messages.admin.stats.totalExercises },
  { key: 'totalSimulations', label: messages.admin.stats.totalSimulations },
  { key: 'activeUsersToday', label: messages.admin.stats.activeToday },
  { key: 'totalOrders', label: 'Giao dịch' },
  { key: 'totalRevenue', label: 'Doanh thu' },
];

/** Strip banner: block-token dữ liệu thật — 7 chỉ số đang theo dõi + index mono. */
const stripBlocks = [true, true, true, true, true, true, true];

// ── Bar chart 7 ngày (dữ liệu THẬT từ backend revenueByDay — §1c) ──
const revenueDays = computed(() => stats.value?.revenueByDay ?? []);
const chartLabels = computed(() => revenueDays.value.map((d) => dayLabel(d.date)));
const chartValues = computed(() => revenueDays.value.map((d) => d.revenue));
const revenueTotal = computed(() => revenueDays.value.reduce((sum, d) => sum + (d.revenue ?? 0), 0));
const orderTotal = computed(() => revenueDays.value.reduce((sum, d) => sum + (d.orders ?? 0), 0));

/** yyyy-MM-dd → nhãn ngày ngắn (T2…CN) */
function dayLabel(date: string): string {
  const dt = new Date(date + 'T00:00:00');
  if (Number.isNaN(dt.getTime())) return date.slice(5);
  return dt.toLocaleDateString('vi-VN', { weekday: 'short' });
}

const weekOption = computed(() => {
  // Vùng dữ liệu LUÔN tối (quyết định #5) — palette đọc canvas vars, không theo theme.
  void ui.theme;
  const ink = cssVar('--canvas-ink', '#0D1020');
  const block = cssVar('--data-core', '#4255FF');
  const muted = cssVar('--index-muted', '#6B7385');
  const text = '#d9dde8'; // text engine canvasTheme.ts (decision log 14/08)
  const axis = 'rgba(66, 85, 255, 0.25)';

  return {
    backgroundColor: 'transparent',
    tooltip: {
      trigger: 'axis' as const,
      backgroundColor: ink,
      borderColor: axis,
      textStyle: { color: text, fontSize: 12 },
      formatter: (params: Array<{ name: string; value: number }>) => {
        const item = params[0];
        return item ? `${item.name}: <b>${formatNumber(item.value)}</b> đ` : '';
      },
    },
    grid: { left: 8, right: 8, top: 28, bottom: 8, containLabel: true },
    xAxis: {
      type: 'category' as const,
      data: chartLabels.value,
      axisLabel: { color: muted, fontSize: 11 },
      axisLine: { lineStyle: { color: axis } },
      axisTick: { show: false },
    },
    yAxis: {
      type: 'value' as const,
      axisLabel: { color: muted, fontSize: 11 },
      splitLine: { lineStyle: { color: axis } },
    },
    series: [
      {
        type: 'bar' as const,
        data: chartValues.value,
        barWidth: 22,
        itemStyle: { color: block, borderRadius: [4, 4, 0, 0] },
        label: { show: true, position: 'top' as const, color: muted, fontSize: 10, formatter: (v: { value: number }) => formatNumber(v.value) },
      },
    ],
  };
});

// ── Donut phân bố vai trò (SVG tự vẽ — dữ liệu thật từ /admin/stats) ──
// 3 màu ngôn ngữ dữ liệu (palette 6 màu — decision log 14/08): data-core/resolved/index-muted.
const ROLES = computed(() => (stats.value?.roleDistribution ?? []).map((item) => ({
  label: item.role,
  value: item.count,
})));

const DONUT_R = 60;
const DONUT_C = 80;
const DONUT_COLORS = ['var(--data-core)', 'var(--resolved)', 'var(--index-muted)'];

function donutSegments(): Array<{ color: string; d: string }> {
  const total = ROLES.value.reduce((sum, r) => sum + r.value, 0);
  if (total <= 0) return [];
  let angle = -90;
  return ROLES.value.map((role, idx) => {
    const start = angle;
    const sweep = (role.value / total) * 360;
    angle += sweep;
    const a0 = (start * Math.PI) / 180;
    const a1 = ((start + sweep) * Math.PI) / 180;
    const x0 = DONUT_C + DONUT_R * Math.cos(a0);
    const y0 = DONUT_C + DONUT_R * Math.sin(a0);
    const x1 = DONUT_C + DONUT_R * Math.cos(a1);
    const y1 = DONUT_C + DONUT_R * Math.sin(a1);
    const large = sweep > 180 ? 1 : 0;
    return {
      color: DONUT_COLORS[idx % DONUT_COLORS.length],
      d: `M ${DONUT_C} ${DONUT_C} L ${x0} ${y0} A ${DONUT_R} ${DONUT_R} 0 ${large} 1 ${x1} ${y1} Z`,
    };
  });
}

function rolePercentage(value: number): string {
  const total = ROLES.value.reduce((sum, role) => sum + role.value, 0);
  return total > 0 ? String(Math.round((value / total) * 100)) : '0';
}

const kpiValue = (key: keyof AdminStatsDto): string => {
  const value = stats.value?.[key];
  return typeof value === 'number' ? formatNumber(value) : '—';
};
</script>

<template>
  <main class="admin-stats container">
    <!-- Banner: surface band level-2 (DESIGN §1/#1 — KHÔNG gradient, KHÔNG shadow) -->
    <header class="admin-stats__hero">
      <div class="admin-stats__hero-inner">
        <div class="admin-stats__hero-main">
          <div class="admin-stats__hero-badges">
            <Badge variant="primary">
              <Activity :size="12" /> {{ messages.admin.badge }}
            </Badge>
          </div>
          <h1 class="admin-stats__title">{{ messages.admin.stats.title }}</h1>
          <p class="admin-stats__sub">{{ messages.admin.stats.subtitle }}</p>
        </div>

        <!-- Mono strip: block-token (5 chỉ số) + index mono (quyết định #4) -->
        <div class="admin-stats__hero-strip" aria-hidden="true">
          <div class="admin-stats__strip-panel">
            <div class="admin-stats__strip-blocks">
              <span
                v-for="(filled, i) in stripBlocks"
                :key="i"
                class="admin-stats__strip-block"
                :class="{ 'admin-stats__strip-block--empty': !filled }"
                :style="{ '--i': i }"
              />
            </div>
            <div class="admin-stats__strip-index">
              <span v-for="(_, i) in stripBlocks" :key="i">{{ String(i).padStart(2, '0') }}</span>
            </div>
          </div>
          <p class="admin-stats__strip-caption">{{ messages.admin.stats.stripLabel(KPIS.length + 1) }}</p>
        </div>
      </div>
    </header>

    <AdminNav active="stats" />

    <div v-if="loading" class="admin-stats__loading" aria-busy="true">
      <div class="admin-stats__kpis">
        <Skeleton v-for="i in (KPIS.length + 1)" :key="i" height="108px" />
      </div>
      <div class="admin-stats__charts">
        <Skeleton v-for="i in 2" :key="i" height="280px" />
      </div>
    </div>

    <div v-else-if="loadError" class="admin-stats__error" role="alert">
      <p class="admin-stats__error-text">{{ messages.admin.stats.loadError }}</p>
      <Button size="sm" variant="secondary" @click="load">
        <RefreshCw :size="14" /> {{ messages.admin.stats.retry }}
      </Button>
    </div>

    <template v-else-if="stats">
      <!-- 1 hero-stat (block-token tối) + 6 KPI level-1 -->
      <div class="admin-stats__kpis">
        <Card class="admin-stats__hero">
          <CardHeader class="admin-stats__hero-head">
            <span class="admin-stats__hero-icon" aria-hidden="true"><Users :size="18" /></span>
            <CardDescription class="admin-stats__kpi-label">{{ messages.admin.stats.totalUsers }}</CardDescription>
          </CardHeader>
          <CardContent class="admin-stats__hero-body">
            <div class="admin-stats__hero-panel">
              <div class="admin-stats__hero-block-row">
                <span class="admin-stats__hero-block" />
                <span class="admin-stats__hero-block admin-stats__hero-block--ghost" />
                <span class="admin-stats__hero-block" />
              </div>
              <p class="admin-stats__hero-value">{{ kpiValue('totalUsers') }}</p>
              <p class="admin-stats__hero-index">USERS · 01</p>
            </div>
          </CardContent>
        </Card>

        <Card v-for="kpi in KPIS" :key="kpi.key" class="admin-stats__kpi">
          <CardHeader class="admin-stats__kpi-head">
            <CardDescription class="admin-stats__kpi-label">{{ kpi.label }}</CardDescription>
          </CardHeader>
          <CardContent class="admin-stats__kpi-body">
            <p class="admin-stats__kpi-value">{{ kpiValue(kpi.key) }}</p>
          </CardContent>
        </Card>
      </div>

      <div class="admin-stats__charts">
        <!-- Hoạt động 7 ngày (ECharts bar — VChartLazy) — vùng dữ liệu LUÔN tối -->
        <div class="admin-stats__chart">
          <div class="admin-stats__chart-head">
            <h2 class="admin-stats__chart-title">{{ messages.admin.stats.weekTitle }}</h2>
            <span class="admin-stats__chart-tag">7 ngày</span>
          </div>
          <VChartLazy :option="weekOption" height="260px" />
        </div>

        <!-- Phân bố vai trò (donut SVG) — vùng dữ liệu LUÔN tối -->
        <div class="admin-stats__chart">
          <h2 class="admin-stats__chart-title">{{ messages.admin.stats.roleTitle }}</h2>
          <svg :viewBox="`0 0 ${DONUT_C * 2} ${DONUT_C * 2}`" class="admin-stats__donut" role="img" :aria-label="messages.admin.stats.roleAria">
            <path v-for="seg in donutSegments()" :key="seg.d" :d="seg.d" :fill="seg.color" />
            <text x="80" y="72" font-size="16" font-weight="600" text-anchor="middle" fill="#d9dde8">{{ rolePercentage(ROLES[0]?.value ?? 0) }}%</text>
            <text x="80" y="92" font-size="12" text-anchor="middle" fill="var(--index-muted)">{{ ROLES[0]?.label ?? '—' }}</text>
          </svg>
          <ul class="admin-stats__legend">
            <li v-for="(role, idx) in ROLES" :key="role.label">
              <span class="admin-stats__legend-dot" :style="{ background: DONUT_COLORS[idx % DONUT_COLORS.length] }" />
              <span class="admin-stats__legend-label">{{ role.label }}</span>
              <span class="admin-stats__legend-value">{{ rolePercentage(role.value) }}%</span>
            </li>
          </ul>
        </div>
      </div>

      <!-- Ghi chú dữ liệu -->
      <div class="admin-stats__total">
        <p class="admin-stats__note">
          <Info :size="14" class="admin-stats__note-icon" aria-hidden="true" />
          <span>
            {{ messages.admin.stats.noteSimsPrefix }} <strong>{{ formatNumber(stats?.totalSimulations ?? 25) }}</strong> {{ messages.admin.stats.noteSimsSuffix }}
          </span>
        </p>
        <p class="admin-stats__note admin-stats__note--muted">
          <span>Doanh thu 7 ngày gần nhất: <strong>{{ formatNumber(revenueTotal) }}</strong> đ · {{ orderTotal }} đơn</span>
        </p>
      </div>
    </template>
  </main>
</template>

<style scoped>
.admin-stats {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Banner: surface band level-2 (DESIGN §6) — không gradient, không shadow ── */
.admin-stats__hero {
  border-bottom: 1px solid var(--border-subtle);
  background: var(--card-raised);
  border-radius: var(--radius-lg);
  padding: var(--space-xl);
}

.admin-stats__hero-inner {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-lg);
  flex-wrap: wrap;
}

.admin-stats__hero-main {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  min-width: 0;
  flex: 1 1 320px;
}

.admin-stats__hero-badges { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

.admin-stats__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  letter-spacing: -0.03em;
  line-height: 1.1;
  margin: 0;
  color: var(--foreground);
}

.admin-stats__sub {
  color: var(--foreground-secondary);
  font-size: var(--text-sm);
  max-width: 60ch;
  margin: 0;
}

/* ── Mono strip: block-token (khoảnh khắc đầu tư duy nhất) ── */
.admin-stats__hero-strip { flex: 0 1 260px; display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-stats__strip-panel {
  background: var(--canvas-ink);
  border: 1px solid rgba(66, 85, 255, 0.25);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.admin-stats__strip-blocks {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: var(--space-sm);
}

.admin-stats__strip-block {
  height: 28px;
  border-radius: var(--radius-sm);
  background: var(--data-core);
  opacity: 0;
  transform: translateY(6px);
  animation: admin-strip-enter 280ms cubic-bezier(0.16, 1, 0.3, 1) forwards;
  animation-delay: calc(var(--i) * 45ms + 60ms);
}

.admin-stats__strip-block--empty {
  background: transparent;
  border: 1px dashed var(--data-core);
  opacity: 1;
  transform: none;
  animation: none;
}

.admin-stats__strip-index {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: var(--space-sm);
}

.admin-stats__strip-index span {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--index-muted);
  text-align: center;
}

.admin-stats__strip-caption {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  letter-spacing: 0.08em;
  text-align: right;
}

@keyframes admin-strip-enter {
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .admin-stats__strip-block {
    animation: none;
    opacity: 1;
    transform: none;
  }
}

/* ── Loading / Error ── */
.admin-stats__loading { display: flex; flex-direction: column; gap: var(--space-md); }

.admin-stats__error {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
  flex-wrap: wrap;
  padding: var(--space-md);
  border: 1px solid color-mix(in srgb, var(--destructive) 35%, transparent);
  background: color-mix(in srgb, var(--destructive) 8%, transparent);
  border-radius: var(--radius-md);
}

.admin-stats__error-text { margin: 0; font-size: var(--text-sm); color: var(--destructive); }

/* ── KPI: 1 hero-stat + 4 level-1 (§6) ── */
.admin-stats__kpis {
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  gap: var(--space-md);
}

.admin-stats__hero { grid-column: span 2; display: flex; flex-direction: column; border-color: var(--border-subtle); background: var(--card-raised); }

.admin-stats__hero-head { display: flex; flex-direction: row; align-items: center; gap: var(--space-sm); padding: var(--space-md) var(--space-md) 0; }

.admin-stats__hero-icon {
  width: 32px;
  height: 32px;
  border-radius: var(--radius-md);
  background: var(--muted);
  color: var(--foreground-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.admin-stats__hero-body { padding: var(--space-sm) var(--space-md) var(--space-md); }

/* Block-token tối — signature Data Bench (quyết định #4/#5) */
.admin-stats__hero-panel {
  background: var(--canvas-ink);
  border: 1px solid rgba(66, 85, 255, 0.25);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.admin-stats__hero-block-row { display: flex; gap: var(--space-xs); }

.admin-stats__hero-block {
  width: 24px;
  height: 16px;
  border-radius: var(--radius-sm);
  background: var(--data-core);
}

.admin-stats__hero-block--ghost {
  background: transparent;
  border: 1px dashed var(--data-core);
}

.admin-stats__hero-value {
  margin: 0;
  font-size: var(--text-2xl);
  font-weight: 600;
  letter-spacing: -0.015em;
  line-height: 1.2;
  color: #d9dde8;
  font-variant-numeric: tabular-nums;
}

.admin-stats__hero-index {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--index-muted);
}

.admin-stats__kpi { display: flex; flex-direction: column; }

.admin-stats__kpi-head { display: flex; flex-direction: row; align-items: center; gap: var(--space-sm); padding: var(--space-md) var(--space-md) 0; }

.admin-stats__kpi-label { font-size: var(--text-xs); font-weight: 500; }

.admin-stats__kpi-body { padding: var(--space-xs) var(--space-md) var(--space-md); }

.admin-stats__kpi-value {
  font-size: var(--text-2xl);
  font-weight: 600;
  color: var(--foreground);
  font-variant-numeric: tabular-nums;
  line-height: 1.2;
}

/* ── Charts: vùng dữ liệu LUÔN tối (quyết định #5) ── */
.admin-stats__charts { display: grid; grid-template-columns: 7fr 5fr; gap: var(--space-md); }

.admin-stats__chart {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  min-width: 0;
  background: var(--canvas-ink);
  border: 1px solid rgba(66, 85, 255, 0.25);
  border-radius: var(--radius-lg);
  padding: var(--space-md);
}

.admin-stats__chart-head { display: flex; align-items: baseline; justify-content: space-between; gap: var(--space-sm); }

.admin-stats__chart-title {
  margin: 0;
  font-size: var(--text-base);
  font-weight: 600;
  letter-spacing: -0.015em;
  color: #d9dde8;
}

.admin-stats__chart-tag {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--index-muted);
  white-space: nowrap;
}

.admin-stats__donut { width: 100%; height: auto; max-height: 220px; }

.admin-stats__legend { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: var(--space-xs); font-size: var(--text-sm); }

.admin-stats__legend li { display: flex; align-items: center; gap: var(--space-sm); }

.admin-stats__legend-dot { display: inline-block; width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0; }

.admin-stats__legend-label { color: var(--index-muted); }

.admin-stats__legend-value {
  margin-left: auto;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  color: #d9dde8;
  font-variant-numeric: tabular-nums;
}

/* ── Ghi chú ── */
.admin-stats__total {
  font-size: var(--text-sm);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  padding: var(--space-md);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
}

.admin-stats__note { display: flex; align-items: flex-start; gap: var(--space-sm); color: var(--foreground); margin: 0; }

.admin-stats__note--muted { color: var(--foreground-tertiary); font-size: var(--text-xs); }

.admin-stats__note-icon { flex-shrink: 0; margin-top: 2px; color: var(--info); }

@media (max-width: 900px) {
  .admin-stats__charts { grid-template-columns: 1fr; }
  .admin-stats__kpis { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .admin-stats__hero { grid-column: span 2; }
}

@media (max-width: 640px) {
  .admin-stats__hero { padding: var(--space-lg); }
  .admin-stats__hero-strip { flex-basis: 100%; }
  .admin-stats__strip-caption { text-align: left; }
  .admin-stats__kpis { grid-template-columns: 1fr; }
  .admin-stats__hero { grid-column: span 1; }
}
</style>
