<script setup lang="ts">
// AdminStatsView — Màn 11: 5 KPI + bar chart ECharts (VChartLazy, 7 ngày)
// + donut SVG tự vẽ (phân bố vai trò).
// View-quality 14/08 (Nhóm D): banner surface band + mono strip block-token;
// 1 hero-stat (block-token tối) + 4 KPI level-1; 2 vùng biểu đồ LUÔN tối
// canvas-ink, palette đọc CSS var (data-core/index-muted/canvas-ink) + text
// #d9dde8 (engine canvasTheme.ts — decision log 14/08); error state + retry.
import { computed, onMounted, ref } from 'vue';
import { Activity, Info, RefreshCw, Users } from 'lucide-vue-next';

import * as adminApi from '@/api/admin';
import type { AdminStatsDto } from '@/api/admin';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import PageHero from '@/components/ui/PageHero.vue';
import AdminHeroStrip from '@/components/admin/AdminHeroStrip.vue';
import StatCard from '@/components/ui/StatCard.vue';
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

// ── 5 KPI (AdminStatsDto: totalUsers/totalLessons/totalExercises/totalSimulations/activeUsersToday) ──
// §6: tối đa 1 hero-stat/màn → totalUsers = hero (block-token tối); còn lại level-1.
const KPIS: Array<{ key: keyof AdminStatsDto; label: string }> = [
  { key: 'totalLessons', label: messages.admin.stats.totalLessons },
  { key: 'totalExercises', label: messages.admin.stats.totalExercises },
  { key: 'totalSimulations', label: messages.admin.stats.totalSimulations },
  { key: 'activeUsersToday', label: messages.admin.stats.activeToday },
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
  // Vùng dữ liệu LUÔN tối (quyết định #5) — palette đọc canvas vars, không theo theme.
  void ui.theme;
  const ink = cssVar('--canvas-ink', '#0D1020');
  const block = cssVar('--data-core', '#4255FF');
  const text = '#d9dde8'; // text engine canvasTheme.ts (decision log 14/08) — nhãn nền tối ≥ #D9DDE8
  const axis = 'rgba(66, 85, 255, 0.25)';
  const gridLine = 'rgba(217, 221, 232, 0.12)';

  return {
    backgroundColor: 'transparent',
    tooltip: {
      trigger: 'axis' as const,
      backgroundColor: ink,
      borderColor: 'rgba(66, 85, 255, 0.45)',
      textStyle: { color: text, fontSize: 12 },
      formatter: (params: Array<{ name: string; value: number }>) => {
        const item = params[0];
        return item ? `${item.name}: <b>${item.value}</b> ${messages.admin.stats.weekUnit}` : '';
      },
    },
    grid: { left: 8, right: 8, top: 32, bottom: 8, containLabel: true },
    xAxis: {
      type: 'category' as const,
      data: WEEK.map((d) => d.day),
      axisLabel: { color: text, fontSize: 12 },
      axisLine: { lineStyle: { color: axis } },
      axisTick: { show: false },
    },
    yAxis: {
      type: 'value' as const,
      axisLabel: { color: text, fontSize: 12 },
      splitLine: { lineStyle: { color: gridLine } },
    },
    series: [
      {
        type: 'bar' as const,
        data: WEEK.map((d) => d.value),
        barWidth: 22,
        itemStyle: { color: block, borderRadius: [4, 4, 0, 0] },
        label: { show: true, position: 'top' as const, color: text, fontSize: 11 },
      },
    ],
  };
});

// ── Donut phân bố vai trò (SVG tự vẽ — dữ liệu minh họa) ──
// 3 màu ngôn ngữ dữ liệu (palette 6 màu — decision log 14/08): data-core/resolved/index-muted.
const ROLES = [
  { label: 'Student', value: 70 },
  { label: 'Teacher', value: 20 },
  { label: 'Admin', value: 10 },
];

const DONUT_R = 60;
const DONUT_C = 80;
const DONUT_COLORS = ['var(--data-core)', 'var(--resolved)', 'var(--index-muted)'];

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
    <!-- Banner: surface band level-2 (PageHero — DESIGN §1/#1: KHÔNG gradient, KHÔNG shadow) -->
    <PageHero :title="messages.admin.stats.title" :description="messages.admin.stats.subtitle">
      <template #badges>
        <Badge variant="primary">
          <Activity :size="12" /> {{ messages.admin.badge }}
        </Badge>
      </template>
      <!-- Mono strip: block-token (5 chỉ số) + index mono (quyết định #4) -->
      <template #side>
        <AdminHeroStrip :count="5" :label="messages.admin.stats.stripLabel(5)" />
      </template>
    </PageHero>

    <AdminNav active="stats" />

    <div v-if="loading" class="admin-stats__loading" aria-busy="true">
      <div class="admin-stats__kpis">
        <Skeleton v-for="i in 5" :key="i" height="108px" />
      </div>
      <div class="admin-stats__charts">
        <Skeleton v-for="i in 2" :key="i" height="280px" />
      </div>
    </div>

    <div v-else-if="loadError" class="admin-stats__error" role="alert">
      <p class="admin-stats__error-text">{{ messages.admin.stats.loadError }}</p>
      <Button size="md" variant="secondary" @click="load">
        <RefreshCw :size="14" /> {{ messages.admin.stats.retry }}
      </Button>
    </div>

    <template v-else-if="stats">
      <!-- 1 hero-stat (block-token tối) + 4 KPI level-1 — §6 -->
      <div class="admin-stats__kpis">
        <StatCard
          level="hero"
          :icon="Users"
          :label="messages.admin.stats.totalUsers"
          :value="kpiValue('totalUsers')"
          index="USERS · 01"
          class="admin-stats__kpi-hero"
        >
          <template #panel>
            <div class="admin-stats__hero-block-row">
              <span class="admin-stats__hero-block" />
              <span class="admin-stats__hero-block admin-stats__hero-block--ghost" />
              <span class="admin-stats__hero-block" />
            </div>
          </template>
        </StatCard>

        <StatCard
          v-for="kpi in KPIS"
          :key="kpi.key"
          :label="kpi.label"
          :value="kpiValue(kpi.key)"
        />
      </div>

      <div class="admin-stats__charts">
        <!-- Hoạt động 7 ngày (ECharts bar — VChartLazy) — vùng dữ liệu LUÔN tối -->
        <div class="admin-stats__chart">
          <div class="admin-stats__chart-head">
            <h2 class="admin-stats__chart-title">{{ messages.admin.stats.weekTitle }}</h2>
            <span class="admin-stats__chart-tag">{{ messages.admin.stats.weekTag }}</span>
          </div>
          <VChartLazy :option="weekOption" height="260px" />
        </div>

        <!-- Phân bố vai trò (donut SVG) — vùng dữ liệu LUÔN tối -->
        <div class="admin-stats__chart">
          <h2 class="admin-stats__chart-title">{{ messages.admin.stats.roleTitle }}</h2>
          <svg :viewBox="`0 0 ${DONUT_C * 2} ${DONUT_C * 2}`" class="admin-stats__donut" role="img" :aria-label="messages.admin.stats.roleAria">
            <path
              v-for="(seg, idx) in donutSegments()"
              :key="seg.d"
              :d="seg.d"
              :fill="seg.color"
            >
              <!-- Tooltip native khi hover segment: tên vai trò + tỷ lệ (nền tối, text #d9dde8) -->
              <title>{{ messages.admin.stats.roleTooltip(ROLES[idx].label, ROLES[idx].value) }}</title>
            </path>
            <text x="80" y="72" font-size="16" font-weight="600" text-anchor="middle" fill="#d9dde8">{{ ROLES[0].value }}%</text>
            <text x="80" y="92" font-size="12" text-anchor="middle" fill="#d9dde8">{{ ROLES[0].label }}</text>
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
      <div class="admin-stats__total">
        <p class="admin-stats__note">
          <Info :size="14" class="admin-stats__note-icon" aria-hidden="true" />
          <span>
            {{ messages.admin.stats.noteSimsPrefix }} <strong>{{ formatNumber(44) }}</strong> {{ messages.admin.stats.noteSimsSuffix }}
          </span>
        </p>
        <p class="admin-stats__note admin-stats__note--muted">
          <span>{{ messages.admin.stats.noteMock }}</span>
        </p>
        <!-- FIX R1: microcopy ngắn — nguồn số liệu + cách làm mới (i18n) -->
        <p class="admin-stats__note admin-stats__note--muted">
          <RefreshCw :size="14" class="admin-stats__note-icon admin-stats__note-icon--muted" aria-hidden="true" />
          <span>{{ messages.admin.stats.noteRefresh }}</span>
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

/* ── KPI: 1 hero-stat + 4 level-1 (StatCard — §6) ── */
.admin-stats__kpis {
  display: grid;
  grid-template-columns: repeat(6, minmax(0, 1fr));
  gap: var(--space-md);
}

/* Hero-stat chiếm 2 cột grid (StatCard level="hero" nhận qua fallthrough) */
.admin-stats__kpi-hero { grid-column: span 2; }

/* Block row trong panel tối (slot #panel của StatCard hero) */
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
  color: #d9dde8;
  white-space: nowrap;
}

.admin-stats__donut { width: 100%; height: auto; max-height: 220px; }

.admin-stats__donut path { transition: opacity 150ms; }

.admin-stats__donut path:hover { opacity: 0.82; }

.admin-stats__legend { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: var(--space-xs); font-size: var(--text-sm); }

.admin-stats__legend li { display: flex; align-items: center; gap: var(--space-sm); }

.admin-stats__legend-dot { display: inline-block; width: 10px; height: 10px; border-radius: 50%; flex-shrink: 0; }

.admin-stats__legend-label { color: #d9dde8; }

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

/* FIX R1: text phụ nâng 1 tầng (tertiary 3.4:1 → secondary ≥ 4.5:1 trên nền light) */
.admin-stats__note--muted { color: var(--foreground-secondary); font-size: var(--text-xs); }

.admin-stats__note-icon { flex-shrink: 0; margin-top: 2px; color: var(--info); }

.admin-stats__note-icon--muted { color: var(--foreground-secondary); }

/* FIX R1: tablet (≤900) giữ chart 2 cột cho gọn luồng thao tác; chỉ xếp 1 cột ở mobile */
@media (max-width: 900px) {
  .admin-stats__kpis { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}

@media (max-width: 640px) {
  .admin-stats__charts { grid-template-columns: 1fr; }
  .admin-stats__kpis { grid-template-columns: 1fr; }
  .admin-stats__kpi-hero { grid-column: span 1; }
}
</style>
