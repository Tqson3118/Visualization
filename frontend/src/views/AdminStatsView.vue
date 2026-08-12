<script setup lang="ts">
// AdminStatsView — Màn 11: 4 KPI + biểu đồ SVG tự vẽ (không Chart.js — chưa có gói)
import { onMounted, ref } from 'vue';

import * as adminApi from '@/api/admin';
import type { AdminStatsDto } from '@/api/admin';
import { useUiStore } from '@/stores/ui';
import AdminNav from '@/components/admin/AdminNav.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
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
    ui.showToast('Không thể tải thống kê.', 'error');
  } finally {
    loading.value = false;
  }
}

const KPIS = [
  { key: 'totalUsers', label: 'Tổng người dùng', icon: '👥' },
  { key: 'totalLessons', label: 'Bài học', icon: '📚' },
  { key: 'totalExercises', label: 'Bài tập', icon: '🧩' },
  { key: 'activeUsersToday', label: 'Hoạt động hôm nay', icon: '⚡' },
] as const;

// Biểu đồ phân bố vai trò (SVG donut)
const ROLES = [
  { label: 'Student', value: 70 },
  { label: 'Teacher', value: 20 },
  { label: 'Admin', value: 10 },
];

const DONUT_R = 60;
const DONUT_C = 80;
const DONUT_COLORS = ['var(--color-primary)', 'var(--color-accent)', 'var(--color-info)'];

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

// Biểu đồ cột 7 ngày (dữ liệu minh họa — backend chỉ trả KPI tức thời)
const WEEK = [
  { day: 'T2', value: 42 },
  { day: 'T3', value: 55 },
  { day: 'T4', value: 38 },
  { day: 'T5', value: 61 },
  { day: 'T6', value: 73 },
  { day: 'T7', value: 50 },
  { day: 'CN', value: 29 },
];
const BAR_MAX = Math.max(...WEEK.map((d) => d.value));
</script>

<template>
  <main class="admin-stats container">
    <h1 class="admin-stats__title">📊 Thống kê hệ thống</h1>

    <AdminNav active="stats" />

    <div v-if="loading" class="admin-stats__loading">
      <Skeleton v-for="i in 6" :key="i" height="72px" />
    </div>

    <template v-else-if="stats">
      <div class="admin-stats__kpis">
        <div v-for="kpi in KPIS" :key="kpi.key" class="admin-stats__kpi card">
          <span class="admin-stats__kpi-icon" aria-hidden="true">{{ kpi.icon }}</span>
          <p class="admin-stats__kpi-value">{{ formatNumber(stats[kpi.key]) }}</p>
          <p class="admin-stats__kpi-label text-muted">{{ kpi.label }}</p>
        </div>
      </div>

      <div class="admin-stats__charts">
        <div class="admin-stats__chart card">
          <h2 class="admin-stats__chart-title">Hoạt động 7 ngày qua</h2>
          <svg :viewBox="'0 0 320 200'" class="admin-stats__bars" role="img" aria-label="Biểu đồ hoạt động 7 ngày">
            <g v-for="(day, idx) in WEEK" :key="day.day">
              <rect
                :x="30 + idx * 42"
                :y="170 - (day.value / BAR_MAX) * 130"
                :width="26"
                :height="(day.value / BAR_MAX) * 130"
                rx="4"
                fill="var(--color-primary)"
                opacity="0.85"
              />
              <text :x="30 + idx * 42 + 13" y="188" font-size="10" fill="#5E7A77" text-anchor="middle">{{ day.day }}</text>
              <text :x="30 + idx * 42 + 13" :y="162 - (day.value / BAR_MAX) * 130" font-size="9" fill="#5E7A77" text-anchor="middle">{{ day.value }}</text>
            </g>
          </svg>
        </div>

        <div class="admin-stats__chart card">
          <h2 class="admin-stats__chart-title">Phân bố vai trò</h2>
          <svg :viewBox="`0 0 ${DONUT_C * 2} ${DONUT_C * 2}`" class="admin-stats__donut" role="img" aria-label="Biểu đồ phân bố vai trò">
            <path v-for="seg in donutSegments()" :key="seg.d" :d="seg.d" :fill="seg.color" />
            <text x="80" y="76" font-size="14" font-weight="700" text-anchor="middle" fill="#134E4A">{{ ROLES[0].value }}%</text>
            <text x="80" y="92" font-size="9" text-anchor="middle" fill="#5E7A77">Student</text>
          </svg>
          <ul class="admin-stats__legend">
            <li v-for="(role, idx) in ROLES" :key="role.label">
              <span class="admin-stats__legend-dot" :style="{ background: DONUT_COLORS[idx] }" />
              {{ role.label }} — {{ role.value }}%
            </li>
          </ul>
        </div>
      </div>

      <div class="admin-stats__total card">
        <p class="text-muted">Tổng mô phỏng trong danh mục: <strong>{{ formatNumber(44) }}</strong> (engines/catalog)</p>
        <p class="text-muted">Ghi chú: biểu đồ 7 ngày + vai trò là dữ liệu minh họa — backend hiện chỉ trả KPI tức thời.</p>
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

.admin-stats__title { font-size: var(--text-2xl); }

.admin-stats__kpis { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: var(--space-md); }

.admin-stats__kpi { display: flex; flex-direction: column; gap: 4px; align-items: flex-start; }

.admin-stats__kpi-icon { font-size: 1.5rem; }

.admin-stats__kpi-value { font-size: var(--text-2xl); font-weight: 800; color: var(--color-primary); }

.admin-stats__kpi-label { font-size: var(--text-sm); }

.admin-stats__charts { display: grid; grid-template-columns: 7fr 5fr; gap: var(--space-md); }

.admin-stats__chart { display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-stats__chart-title { font-size: var(--text-md); }

.admin-stats__bars, .admin-stats__donut { width: 100%; height: auto; }

.admin-stats__legend { list-style: none; display: flex; gap: var(--space-md); flex-wrap: wrap; font-size: var(--text-sm); }

.admin-stats__legend-dot { display: inline-block; width: 10px; height: 10px; border-radius: 50%; margin-right: 4px; }

.admin-stats__total { font-size: var(--text-sm); display: flex; flex-direction: column; gap: 4px; }

@media (max-width: 900px) {
  .admin-stats__charts { grid-template-columns: 1fr; }
}
</style>
