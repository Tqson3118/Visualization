<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import {
  Activity,
  Users,
  BookOpen,
  DollarSign,
  ShoppingBag,
  RefreshCw,
  TrendingUp,
  CheckCircle2,
  Clock,
  Layers,
  Sparkles,
  ArrowUpRight,
  ShieldCheck,
  GraduationCap,
  Award,
} from 'lucide-vue-next';

import * as adminApi from '@/api/admin';
import type { AdminStatsDto } from '@/api/admin';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import StudioShell from '@/components/studio/StudioShell.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/Button.vue';
import VChartLazy from '@/components/ui/VChartLazy.vue';
import { formatNumber } from '@/utils/format';

const ui = useUiStore();
const stats = ref<AdminStatsDto | null>(null);
const loading = ref(true);
const loadError = ref(false);
const chartMetric = ref<'revenue' | 'orders'>('revenue');
const chartPeriod = ref<'7d' | '30d' | 'all'>('7d');

onMounted(load);

async function setPeriod(period: '7d' | '30d' | 'all'): Promise<void> {
  chartPeriod.value = period;
  await load();
}

async function load(): Promise<void> {
  loading.value = true;
  loadError.value = false;
  try {
    stats.value = await adminApi.fetchStats(chartPeriod.value);
  } catch {
    loadError.value = true;
  } finally {
    loading.value = false;
  }
}

function formatDate(iso: string): string {
  if (!iso) return '—';
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return iso;
  return d.toLocaleString('vi-VN', {
    day: '2-digit',
    month: '2-digit',
    year: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

function formatCurrency(val?: number): string {
  if (val === undefined || val === null) return '0 đ';
  return `${formatNumber(val)} đ`;
}

// ── Chart 1: Doanh thu & Đơn hàng 7 ngày (ECharts Bar / Line) ──
const revenueDays = computed(() => stats.value?.revenueByDay ?? []);
const chartLabels = computed(() =>
  revenueDays.value.map((d) => {
    const dt = new Date(d.date + 'T00:00:00');
    if (Number.isNaN(dt.getTime())) return d.date.slice(5);
    const day = dt.toLocaleDateString('vi-VN', { weekday: 'short' });
    const dateStr = dt.toLocaleDateString('vi-VN', { day: '2-digit', month: '2-digit' });
    return `${day} (${dateStr})`;
  }),
);

const revenueValues = computed(() => revenueDays.value.map((d) => d.revenue));
const orderValues = computed(() => revenueDays.value.map((d) => d.orders));
const totalRevenueRange = computed(() => revenueValues.value.reduce((a, b) => a + b, 0));
const totalOrdersRange = computed(() => orderValues.value.reduce((a, b) => a + b, 0));
const periodLabel = computed(() =>
  chartPeriod.value === '7d'
    ? '7 Ngày Gần Nhất'
    : chartPeriod.value === '30d'
      ? '30 Ngày Gần Nhất'
      : 'Tất Cả Thời Gian',
);

const revenueChartOption = computed(() => {
  const isRev = chartMetric.value === 'revenue';
  return {
    backgroundColor: 'transparent',
    tooltip: {
      trigger: 'axis',
      backgroundColor: '#0d1117',
      borderColor: '#30363d',
      textStyle: { color: '#f0f6fc', fontSize: 12 },
      formatter: (params: Array<{ name: string; value: number }>) => {
        const item = params[0];
        if (!item) return '';
        return isRev
          ? `${item.name}<br/><span style="color:#a855f7;font-weight:bold;">Doanh thu: ${formatCurrency(item.value)}</span>`
          : `${item.name}<br/><span style="color:#38bdf8;font-weight:bold;">Đơn hàng: ${formatNumber(item.value)} đơn</span>`;
      },
    },
    grid: { left: 16, right: 16, top: 32, bottom: 16, containLabel: true },
    xAxis: {
      type: 'category',
      data: chartLabels.value,
      axisLabel: { color: '#8b949e', fontSize: 11 },
      axisLine: { lineStyle: { color: '#30363d' } },
      axisTick: { show: false },
    },
    yAxis: {
      type: 'value',
      axisLabel: {
        color: '#8b949e',
        fontSize: 11,
        formatter: (v: number) => (isRev ? `${formatNumber(v / 1000)}k` : `${v}`),
      },
      splitLine: { lineStyle: { color: 'rgba(255, 255, 255, 0.06)' } },
    },
    series: [
      {
        type: 'bar',
        data: isRev ? revenueValues.value : orderValues.value,
        barWidth: 28,
        itemStyle: {
          borderRadius: [6, 6, 0, 0],
          color: isRev
            ? {
                type: 'linear',
                x: 0,
                y: 0,
                x2: 0,
                y2: 1,
                colorStops: [
                  { offset: 0, color: '#a855f7' },
                  { offset: 1, color: '#6366f1' },
                ],
              }
            : {
                type: 'linear',
                x: 0,
                y: 0,
                x2: 0,
                y2: 1,
                colorStops: [
                  { offset: 0, color: '#38bdf8' },
                  { offset: 1, color: '#0284c7' },
                ],
              },
        },
        label: {
          show: true,
          position: 'top',
          color: '#c9d1d9',
          fontSize: 10,
          formatter: (v: { value: number }) => (isRev ? (v.value > 0 ? `${formatNumber(v.value / 1000)}k` : '0') : `${v.value}`),
        },
      },
    ],
  };
});

// ── Chart 2: Phân bố vai trò người dùng (ECharts Donut) ──
const roleLabelMap: Record<string, string> = {
  STUDENT: 'Học viên (Student)',
  TEACHER: 'Giảng viên (Teacher)',
  TEACHER_PENDING: 'Chờ duyệt GV',
  ADMIN: 'Quản trị viên (Admin)',
};

const roleColors = ['#6366f1', '#10b981', '#f59e0b', '#ec4899'];

const roleChartOption = computed(() => {
  const list = stats.value?.roleDistribution ?? [];
  const chartData = list.map((item, idx) => ({
    name: roleLabelMap[item.role] || item.role,
    value: item.count,
    itemStyle: { color: roleColors[idx % roleColors.length] },
  }));

  const total = chartData.reduce((s, i) => s + i.value, 0);

  return {
    backgroundColor: 'transparent',
    tooltip: {
      trigger: 'item',
      backgroundColor: '#0d1117',
      borderColor: '#30363d',
      textStyle: { color: '#f0f6fc', fontSize: 12 },
      formatter: '{b}: <b>{c}</b> ({d}%)',
    },
    legend: {
      bottom: 0,
      left: 'center',
      textStyle: { color: '#8b949e', fontSize: 11 },
      icon: 'circle',
    },
    series: [
      {
        type: 'pie',
        radius: ['52%', '76%'],
        center: ['50%', '45%'],
        avoidLabelOverlap: false,
        itemStyle: {
          borderRadius: 8,
          borderColor: '#161b22',
          borderWidth: 3,
        },
        label: {
          show: false,
          position: 'center',
        },
        emphasis: {
          label: {
            show: true,
            fontSize: 14,
            fontWeight: 'bold',
            color: '#ffffff',
            formatter: '{b}\n{c} ({d}%)',
          },
        },
        labelLine: { show: false },
        data: chartData,
      },
    ],
  };
});
</script>

<template>
  <StudioShell active-tab="stats">
    <div class="space-y-6">
      <!-- Header Hero Banner -->
      <header class="admin-stats__hero">
        <div class="admin-stats__hero-inner">
          <div class="admin-stats__hero-main">
            <div class="admin-stats__hero-badges">
              <Badge variant="primary">
                <Activity :size="13" /> Quản Trị Hệ Thống
              </Badge>
            </div>
            <h1 class="admin-stats__title">Thống Kê &amp; Báo Cáo Tổng Quan</h1>
            <p class="admin-stats__sub">Theo dõi các chỉ số tăng trưởng người dùng, nội dung đào tạo và giao dịch hệ thống theo thời gian thực.</p>
          </div>

          <div class="flex items-center gap-3">
            <Button variant="secondary" size="sm" class="gap-1.5" :loading="loading" @click="load">
              <RefreshCw :size="14" /> Làm mới dữ liệu
            </Button>
          </div>
        </div>
      </header>

    <!-- Loading Skeletons -->
    <div v-if="loading" class="space-y-6" aria-busy="true">
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <Skeleton v-for="i in 4" :key="i" height="120px" class="rounded-2xl" />
      </div>
      <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <Skeleton class="lg:col-span-2 rounded-2xl" height="340px" />
        <Skeleton class="rounded-2xl" height="340px" />
      </div>
    </div>

    <!-- Error State -->
    <div v-else-if="loadError" class="p-8 text-center rounded-2xl bg-vdsa-surface border border-vdsa-border space-y-3" role="alert">
      <p class="text-sm text-vdsa-red">Không thể tải dữ liệu thống kê từ máy chủ.</p>
      <Button size="sm" variant="secondary" @click="load">
        <RefreshCw :size="14" /> Thử lại
      </Button>
    </div>

    <!-- Main Dashboard Content -->
    <template v-else-if="stats">
      <!-- ═══ 4 THẺ KPI CHÍNH ═══ -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <!-- 1. Người dùng -->
        <div class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border hover:border-vdsa-accent/50 transition-all flex flex-col justify-between group">
          <div class="flex items-start justify-between">
            <div>
              <span class="text-xs font-bold text-vdsa-secondary uppercase tracking-wider block">Tổng Người Dùng</span>
              <h2 class="text-3xl font-black text-white mt-1.5">{{ formatNumber(stats.totalUsers) }}</h2>
            </div>
            <div class="w-10 h-10 rounded-xl bg-indigo-500/15 border border-indigo-500/30 flex items-center justify-center text-indigo-400 group-hover:scale-110 transition-transform">
              <Users :size="20" />
            </div>
          </div>
          <div class="flex items-center gap-2 mt-4 pt-3 border-t border-vdsa-border/60 text-xs text-emerald-400 font-semibold">
            <span class="w-2 h-2 rounded-full bg-emerald-400 animate-pulse"></span>
            <span><strong>{{ formatNumber(stats.activeUsersToday) }}</strong> đang hoạt động hôm nay</span>
          </div>
        </div>

        <!-- 2. Nội dung học tập -->
        <div class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border hover:border-vdsa-accent/50 transition-all flex flex-col justify-between group">
          <div class="flex items-start justify-between">
            <div>
              <span class="text-xs font-bold text-vdsa-secondary uppercase tracking-wider block">Kho Bài Học &amp; CTDL</span>
              <h2 class="text-3xl font-black text-white mt-1.5">{{ formatNumber(stats.totalLessons) }}</h2>
            </div>
            <div class="w-10 h-10 rounded-xl bg-teal-500/15 border border-teal-500/30 flex items-center justify-center text-teal-400 group-hover:scale-110 transition-transform">
              <BookOpen :size="20" />
            </div>
          </div>
          <div class="flex items-center justify-between mt-4 pt-3 border-t border-vdsa-border/60 text-xs text-vdsa-secondary font-medium">
            <span>{{ formatNumber(stats.totalExercises) }} bài tập</span>
            <span>·</span>
            <span>{{ formatNumber(stats.totalSimulations) }} mô phỏng</span>
          </div>
        </div>

        <!-- 3. Doanh thu hệ thống -->
        <div class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border hover:border-vdsa-accent/50 transition-all flex flex-col justify-between group">
          <div class="flex items-start justify-between">
            <div>
              <span class="text-xs font-bold text-vdsa-secondary uppercase tracking-wider block">Tổng Doanh Thu</span>
              <h2 class="text-2xl font-black text-emerald-400 mt-1.5">{{ formatCurrency(stats.totalRevenue) }}</h2>
            </div>
            <div class="w-10 h-10 rounded-xl bg-emerald-500/15 border border-emerald-500/30 flex items-center justify-center text-emerald-400 group-hover:scale-110 transition-transform">
              <DollarSign :size="20" />
            </div>
          </div>
          <div class="flex items-center justify-between mt-4 pt-3 border-t border-vdsa-border/60 text-xs text-vdsa-secondary font-medium">
            <span>Theo biểu đồ: <strong class="text-white">{{ formatCurrency(totalRevenueRange) }}</strong></span>
          </div>
        </div>

        <!-- 4. Giao dịch / Đơn hàng -->
        <div class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border hover:border-vdsa-accent/50 transition-all flex flex-col justify-between group">
          <div class="flex items-start justify-between">
            <div>
              <span class="text-xs font-bold text-vdsa-secondary uppercase tracking-wider block">Tổng Giao Dịch</span>
              <h2 class="text-3xl font-black text-white mt-1.5">{{ formatNumber(stats.totalOrders ?? 0) }}</h2>
            </div>
            <div class="w-10 h-10 rounded-xl bg-amber-500/15 border border-amber-500/30 flex items-center justify-center text-amber-400 group-hover:scale-110 transition-transform">
              <ShoppingBag :size="20" />
            </div>
          </div>
          <div class="flex items-center justify-between mt-4 pt-3 border-t border-vdsa-border/60 text-xs text-vdsa-secondary font-medium">
            <span class="text-emerald-400">✓ {{ stats.completedOrders ?? 0 }} thành công</span>
            <span v-if="stats.pendingOrders" class="text-amber-400">⏳ {{ stats.pendingOrders }} chờ</span>
          </div>
        </div>
      </div>

      <!-- ═══ BIỂU ĐỒ DOANH THU & GIAO DỊCH ═══ -->
      <div class="w-full p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border flex flex-col justify-between">
        <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-3 mb-2">
          <div>
            <h3 class="text-base font-extrabold text-white flex items-center gap-2">
              <TrendingUp :size="18" class="text-vdsa-accent" />
              Biểu Đồ Doanh Thu &amp; Giao Dịch ({{ periodLabel }})
            </h3>
            <p class="text-xs text-vdsa-muted mt-0.5">
              Tổng {{ chartPeriod === '7d' ? '7 ngày' : chartPeriod === '30d' ? '30 ngày' : 'toàn thời gian' }}: {{ formatCurrency(totalRevenueRange) }} ({{ totalOrdersRange }} đơn)
            </p>
          </div>

          <!-- Controls: Period Selector & Metric Toggle -->
          <div class="flex items-center gap-2 flex-wrap self-start sm:self-auto">
            <!-- Period Selector -->
            <div class="flex bg-vdsa-bg p-1 rounded-lg border border-vdsa-border text-xs">
              <button
                type="button"
                class="px-2.5 py-1 rounded font-semibold transition-colors cursor-pointer"
                :class="chartPeriod === '7d' ? 'bg-vdsa-surface text-white border border-vdsa-border' : 'text-vdsa-muted hover:text-white'"
                @click="setPeriod('7d')"
              >
                7 Ngày
              </button>
              <button
                type="button"
                class="px-2.5 py-1 rounded font-semibold transition-colors cursor-pointer"
                :class="chartPeriod === '30d' ? 'bg-vdsa-surface text-white border border-vdsa-border' : 'text-vdsa-muted hover:text-white'"
                @click="setPeriod('30d')"
              >
                30 Ngày
              </button>
              <button
                type="button"
                class="px-2.5 py-1 rounded font-semibold transition-colors cursor-pointer"
                :class="chartPeriod === 'all' ? 'bg-vdsa-surface text-white border border-vdsa-border' : 'text-vdsa-muted hover:text-white'"
                @click="setPeriod('all')"
              >
                Tất cả
              </button>
            </div>

            <!-- Metric Toggle Pill -->
            <div class="flex bg-vdsa-bg p-1 rounded-lg border border-vdsa-border text-xs">
              <button
                type="button"
                class="px-3 py-1 rounded font-semibold transition-colors cursor-pointer"
                :class="chartMetric === 'revenue' ? 'bg-vdsa-accent text-white' : 'text-vdsa-muted hover:text-white'"
                @click="chartMetric = 'revenue'"
              >
                Doanh thu
              </button>
              <button
                type="button"
                class="px-3 py-1 rounded font-semibold transition-colors cursor-pointer"
                :class="chartMetric === 'orders' ? 'bg-vdsa-accent text-white' : 'text-vdsa-muted hover:text-white'"
                @click="chartMetric = 'orders'"
              >
                Đơn hàng
              </button>
            </div>
          </div>
        </div>

        <VChartLazy :option="revenueChartOption" height="320px" />
      </div>

      <!-- ═══ BẢNG GIAO DỊCH MỚI NHẤT (NẾU CÓ DỮ LIỆU) ═══ -->
      <div v-if="stats.recentOrders && stats.recentOrders.length > 0" class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border space-y-4">
        <div class="flex items-center justify-between">
          <h3 class="text-base font-extrabold text-white flex items-center gap-2">
            <ShoppingBag :size="18" class="text-vdsa-yellow" />
            Giao Dịch Gói Học Phí Mới Nhất
          </h3>
          <span class="text-xs text-vdsa-muted">{{ stats.recentOrders.length }} giao dịch gần nhất</span>
        </div>

        <div class="overflow-x-auto">
          <table class="w-full border-collapse text-left">
            <thead>
              <tr class="border-b border-vdsa-border text-xs font-bold text-vdsa-muted uppercase tracking-wider">
                <th class="py-3 px-4">Mã Đơn</th>
                <th class="py-3 px-4">Khách Hàng</th>
                <th class="py-3 px-4">Số Tiền</th>
                <th class="py-3 px-4">Trạng Thái</th>
                <th class="py-3 px-4">Thời Gian</th>
              </tr>
            </thead>
            <tbody class="divide-y divide-vdsa-border/60 text-xs">
              <tr v-for="order in stats.recentOrders" :key="order.id" class="hover:bg-vdsa-hover/50 transition-colors">
                <td class="py-3.5 px-4 font-mono font-bold text-vdsa-accent">{{ order.paymentCode || order.id }}</td>
                <td class="py-3.5 px-4">
                  <div class="font-bold text-white">{{ order.userDisplayName }}</div>
                  <div class="text-[11px] text-vdsa-muted">{{ order.email }}</div>
                </td>
                <td class="py-3.5 px-4 font-bold text-emerald-400">{{ formatCurrency(order.amount) }}</td>
                <td class="py-3.5 px-4">
                  <Badge :variant="order.status === 'COMPLETED' ? 'success' : order.status === 'PENDING' ? 'warning' : 'muted'">
                    {{ order.status === 'COMPLETED' ? 'Hoàn thành' : order.status === 'PENDING' ? 'Chờ thanh toán' : order.status }}
                  </Badge>
                </td>
                <td class="py-3.5 px-4 text-vdsa-muted">{{ formatDate(order.createdAt) }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </template>
    </div>
  </StudioShell>
</template>

<style scoped>
.admin-stats {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 1200px;
}

/* ── Header Banner ── */
.admin-stats__hero {
  border: 1px solid var(--border-subtle, rgba(255, 255, 255, 0.08));
  background: var(--card-raised, #161b22);
  border-radius: var(--radius-lg, 16px);
  padding: var(--space-xl, 24px);
}

.admin-stats__hero-inner {
  display: flex;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-lg, 20px);
  flex-wrap: wrap;
}

.admin-stats__hero-main {
  display: flex;
  flex-direction: column;
  gap: var(--space-xs, 6px);
}

.admin-stats__hero-badges {
  display: flex;
  align-items: center;
  gap: var(--space-xs, 6px);
}

.admin-stats__title {
  font-size: var(--text-2xl, 24px);
  font-weight: 800;
  color: var(--color-text-primary, #ffffff);
  line-height: 1.2;
}

.admin-stats__sub {
  font-size: var(--text-sm, 13px);
  color: var(--color-text-secondary, #8b949e);
}
</style>
