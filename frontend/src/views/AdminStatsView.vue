<script setup lang="ts">
// AdminStatsView — Màn 11: 5 KPI + bar chart ECharts (VChartLazy, 7 ngày)
// + donut SVG tự vẽ (phân bố vai trò). H-B: hero Aurora soft, Card shadcn
// KPI + hover-lift, chart màu theo CSS var (đổi theme runtime), giữ dữ liệu cũ.
import { computed, onMounted, ref } from 'vue';
import type { Component } from 'vue';
import { Activity, BarChart3, BookOpen, Cpu, Info, Puzzle, Users, Zap } from 'lucide-vue-next';

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
import VChartLazy from '@/components/ui/VChartLazy.vue';
import { formatNumber } from '@/utils/format';

const ui = useUiStore();
const stats = ref<AdminStatsDto | null>(null);
const loading = ref(true);

onMounted(load);

async function load(): Promise<void> {
  loading.value = true;
  try {
    stats.value = await adminApi.fetchStats();
  } catch {
    ui.showToast(messages.admin.stats.loadError, 'error');
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

// ── 5 KPI (AdminStatsDto: totalUsers/totalLessons/totalExercises/totalSimulations/activeUsersToday) ──
const KPIS: Array<{ key: keyof AdminStatsDto; label: string; icon: Component; tint: 'aurora' | 'mint' | 'sunset' }> = [
  { key: 'totalUsers', label: messages.admin.stats.totalUsers, icon: Users, tint: 'aurora' },
  { key: 'totalLessons', label: messages.admin.stats.totalLessons, icon: BookOpen, tint: 'mint' },
  { key: 'totalExercises', label: messages.admin.stats.totalExercises, icon: Puzzle, tint: 'sunset' },
  { key: 'totalSimulations', label: messages.admin.stats.totalSimulations, icon: Cpu, tint: 'aurora' },
  { key: 'activeUsersToday', label: messages.admin.stats.activeToday, icon: Zap, tint: 'sunset' },
];

// ── Bar chart 7 ngày (dữ liệu minh họa — backend chỉ trả KPI tức thời) ──
const WEEK = [
  { day: 'T2', value: 42 },
  { day: 'T3', value: 55 },
  { day: 'T4', value: 38 },
  { day: 'T5', value: 61 },
  { day: 'T6', value: 73 },
  { day: 'T7', value: 50 },
  { day: 'CN', value: 29 },
];

const weekOption = computed(() => {
  // Phụ thuộc theme (ui.theme) → recompute option khi toggle sáng/tối
  void ui.theme;
  const textColor = cssVar('--color-text-muted', '#5E7A77');
  const axisColor = cssVar('--color-border', '#cbd5e1');
  const foreground = cssVar('--color-foreground', '#134e4a');
  const cardColor = cssVar('--color-card', '#ffffff');
  const primary = cssVar('--color-primary', '#0d9488');

  return {
    tooltip: {
      trigger: 'axis' as const,
      backgroundColor: cardColor,
      borderColor: axisColor,
      textStyle: { color: foreground, fontSize: 12 },
      formatter: (params: Array<{ name: string; value: number }>) => {
        const item = params[0];
        return item ? `${item.name}: <b>${item.value}</b> lượt` : '';
      },
    },
    grid: { left: 8, right: 8, top: 28, bottom: 8, containLabel: true },
    xAxis: {
      type: 'category' as const,
      data: WEEK.map((d) => d.day),
      axisLabel: { color: textColor, fontSize: 11 },
      axisLine: { lineStyle: { color: axisColor } },
      axisTick: { show: false },
    },
    yAxis: {
      type: 'value' as const,
      axisLabel: { color: textColor, fontSize: 11 },
      splitLine: { lineStyle: { color: axisColor } },
    },
    series: [
      {
        type: 'bar' as const,
        data: WEEK.map((d) => d.value),
        barWidth: 22,
        itemStyle: { color: primary, borderRadius: [6, 6, 0, 0] },
        label: { show: true, position: 'top' as const, color: textColor, fontSize: 10 },
      },
    ],
  };
});

// ── Donut phân bố vai trò (SVG tự vẽ — dữ liệu minh họa) ──
const ROLES = [
  { label: 'Student', value: 70 },
  { label: 'Teacher', value: 20 },
  { label: 'Admin', value: 10 },
];

const DONUT_R = 60;
const DONUT_C = 80;
const DONUT_COLORS = ['var(--color-primary)', 'var(--color-secondary)', 'var(--color-info)'];

function donutSegments(): Array<{ color: string; d: string }> {
  const total = ROLES.reduce((sum, r) => sum + r.value, 0);
  let angle = -90;
  return ROLES.map((role, idx) => {
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

const kpiValue = (key: keyof AdminStatsDto): string => (stats.value ? formatNumber(stats.value[key]) : '—');
</script>

<template>
  <main class="admin-stats container">
    <!-- Hero gradient Aurora soft -->
    <header class="admin-stats__hero">
      <div class="admin-stats__hero-body">
        <span class="admin-stats__hero-icon" aria-hidden="true"><BarChart3 :size="24" /></span>
        <div class="admin-stats__hero-title-wrap">
          <h1 class="admin-stats__title">{{ messages.admin.stats.title }}</h1>
          <p class="admin-stats__sub">{{ messages.admin.stats.subtitle }}</p>
        </div>
        <Badge variant="primary" class="admin-stats__hero-badge">
          <Activity :size="12" /> {{ messages.admin.badge }}
        </Badge>
      </div>
    </header>

    <AdminNav active="stats" />

    <div v-if="loading" class="admin-stats__loading" aria-busy="true">
      <div class="admin-stats__kpis">
        <Skeleton v-for="i in 5" :key="i" height="108px" />
      </div>
      <div class="admin-stats__charts">
        <Skeleton v-for="i in 2" :key="i" height="280px" />
      </div>
    </div>

    <template v-else-if="stats">
      <!-- 5 KPI — Card shadcn + hover-lift -->
      <div class="admin-stats__kpis">
        <Card v-for="kpi in KPIS" :key="kpi.key" class="admin-stats__kpi hover-lift">
          <CardHeader class="admin-stats__kpi-head">
            <span class="admin-stats__kpi-icon" :class="`admin-stats__kpi-icon--${kpi.tint}`" aria-hidden="true">
              <component :is="kpi.icon" :size="18" />
            </span>
            <CardDescription class="admin-stats__kpi-label">{{ kpi.label }}</CardDescription>
          </CardHeader>
          <CardContent class="admin-stats__kpi-body">
            <p class="admin-stats__kpi-value">{{ kpiValue(kpi.key) }}</p>
          </CardContent>
        </Card>
      </div>

      <div class="admin-stats__charts">
        <!-- Hoạt động 7 ngày (ECharts bar — VChartLazy) -->
        <div class="admin-stats__chart card">
          <div class="admin-stats__chart-head">
            <h2 class="admin-stats__chart-title">{{ messages.admin.stats.weekTitle }}</h2>
            <span class="admin-stats__chart-tag text-muted">7 ngày</span>
          </div>
          <VChartLazy :option="weekOption" height="260px" />
        </div>

        <!-- Phân bố vai trò (donut SVG theme-aware) -->
        <div class="admin-stats__chart card">
          <h2 class="admin-stats__chart-title">{{ messages.admin.stats.roleTitle }}</h2>
          <svg :viewBox="`0 0 ${DONUT_C * 2} ${DONUT_C * 2}`" class="admin-stats__donut" role="img" :aria-label="messages.admin.stats.roleAria">
            <path v-for="seg in donutSegments()" :key="seg.d" :d="seg.d" :fill="seg.color" />
            <text x="80" y="74" font-size="16" font-weight="800" text-anchor="middle" fill="var(--color-foreground)">{{ ROLES[0].value }}%</text>
            <text x="80" y="92" font-size="10" text-anchor="middle" fill="var(--color-text-muted)">{{ ROLES[0].label }}</text>
          </svg>
          <ul class="admin-stats__legend">
            <li v-for="(role, idx) in ROLES" :key="role.label">
              <span class="admin-stats__legend-dot" :style="{ background: DONUT_COLORS[idx] }" />
              <span class="admin-stats__legend-label">{{ role.label }}</span>
              <span class="admin-stats__legend-value">{{ role.value }}%</span>
            </li>
          </ul>
        </div>
      </div>

      <!-- Ghi chú dữ liệu -->
      <div class="admin-stats__total card">
        <p class="admin-stats__note">
          <Info :size="14" class="admin-stats__note-icon" aria-hidden="true" />
          <span>
            {{ messages.admin.stats.noteSimsPrefix }} <strong>{{ formatNumber(44) }}</strong> {{ messages.admin.stats.noteSimsSuffix }}
          </span>
        </p>
        <p class="admin-stats__note admin-stats__note--muted">
          <span>{{ messages.admin.stats.noteMock }}</span>
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

/* ── Hero gradient Aurora soft ── */
.admin-stats__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 32%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-aurora);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
}

.admin-stats__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: color-mix(in srgb, var(--color-background) 58%, transparent);
}

.admin-stats__hero::before {
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

.admin-stats__hero-body { display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.admin-stats__hero-icon {
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

.admin-stats__hero-title-wrap { display: flex; flex-direction: column; gap: 4px; }

.admin-stats__title {
  font-size: var(--text-2xl);
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.admin-stats__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

.admin-stats__hero-badge { margin-left: auto; }

.admin-stats__loading { display: flex; flex-direction: column; gap: var(--space-md); }

/* ── KPI ── */
.admin-stats__kpis { display: grid; grid-template-columns: repeat(auto-fit, minmax(170px, 1fr)); gap: var(--space-md); }

.admin-stats__kpi { display: flex; flex-direction: column; }

.admin-stats__kpi-head { display: flex; flex-direction: row; align-items: center; gap: var(--space-sm); padding: var(--space-md) var(--space-md) 0; }

.admin-stats__kpi-icon {
  width: 36px;
  height: 36px;
  border-radius: var(--radius-md);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  color: #fff;
  box-shadow: var(--shadow-sm);
}

.admin-stats__kpi-icon--aurora { background-image: var(--gradient-aurora); }
.admin-stats__kpi-icon--mint { background-image: var(--gradient-mint); }
.admin-stats__kpi-icon--sunset { background-image: var(--gradient-sunset); }

.admin-stats__kpi-label { font-size: var(--text-xs); font-weight: 600; }

.admin-stats__kpi-body { padding: var(--space-xs) var(--space-md) var(--space-md); }

.admin-stats__kpi-value {
  font-size: var(--text-2xl);
  font-weight: 800;
  color: var(--color-foreground);
  font-variant-numeric: tabular-nums;
  line-height: 1.2;
}

/* ── Charts ── */
.admin-stats__charts { display: grid; grid-template-columns: 7fr 5fr; gap: var(--space-md); }

.admin-stats__chart { display: flex; flex-direction: column; gap: var(--space-sm); min-width: 0; }

.admin-stats__chart-head { display: flex; align-items: baseline; justify-content: space-between; gap: var(--space-sm); }

.admin-stats__chart-title { font-size: var(--text-md); }

.admin-stats__chart-tag {
  font-size: var(--text-xs);
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  white-space: nowrap;
}

.admin-stats__donut { width: 100%; height: auto; max-height: 220px; }

.admin-stats__legend { list-style: none; display: flex; flex-direction: column; gap: var(--space-xs); font-size: var(--text-sm); }

.admin-stats__legend li { display: flex; align-items: center; gap: var(--space-sm); }

.admin-stats__legend-dot { display: inline-block; width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0; }

.admin-stats__legend-label { color: var(--color-text-muted); }

.admin-stats__legend-value { margin-left: auto; font-weight: 700; font-variant-numeric: tabular-nums; }

/* ── Ghi chú ── */
.admin-stats__total { font-size: var(--text-sm); display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-stats__note { display: flex; align-items: flex-start; gap: var(--space-sm); color: var(--color-foreground); }

.admin-stats__note--muted { color: var(--color-text-muted); font-size: var(--text-xs); }

.admin-stats__note-icon { flex-shrink: 0; margin-top: 2px; color: var(--color-primary); }

@media (max-width: 900px) {
  .admin-stats__charts { grid-template-columns: 1fr; }
}

@media (max-width: 640px) {
  .admin-stats__hero-badge { margin-left: 0; }
  .admin-stats__kpis { grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); }
}
</style>
