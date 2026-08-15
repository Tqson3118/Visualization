<script setup lang="ts">
// ClassReportView — Màn 21: báo cáo lớp (hero KPI tối + KPI phụ + chart phân bố + lagging learners)
// View-quality Phase 1 (Nhóm D) + Task 2 (ui-redesign):
//  - Banner = surface band level-2 + sub mono; bảng chuẩn §4.6 + cột số mono + mobile card-stack.
//  - 14/08: align contract API với backend THẬT (ClassReportDto = totalMembers/
//    assignments[]/laggingLearners[] — trước đây view đọc completionPct/rows không tồn
//    tại → "undefined%"/"NaN"/bảng rỗng). Bảng chuyển sang thống kê từng bài gán +
//    block lagging learners (dữ liệu thật, block-token + index mono).
//  - Task 2: hero KPI (tỷ lệ hoàn thành + điểm TB) trên panel LUÔN tối canvas-ink;
//    donut ECharts phân bố nộp bài (Đúng hạn/Nộp trễ/Chưa nộp) nền tối + label/legend
//    tương phản cao; card lagging nổi bật: badge đỏ dịu destructive + nút nhắc nhở.
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { Check, Copy, Download, Printer } from 'lucide-vue-next';

// ECharts: đăng ký thêm PieChart (VChartLazy đã đăng ký CanvasRenderer/Legend/Tooltip
// toàn cục qua 'echarts/core' singleton — G-F2d pattern LeaderboardView/ProfileView).
import { use } from 'echarts/core';
import { PieChart } from 'echarts/charts';
use([PieChart]);

import * as classesApi from '@/api/classes';
import type { ClassReportAssignmentDto, ClassReportDto, LaggingLearnerDto } from '@/api/types';
import { useUiStore } from '@/stores/ui';
import { formatDate, formatNumber } from '@/utils/format';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import { Card } from '@/components/ui/card';
import PageHero from '@/components/ui/PageHero.vue';
import StatCard from '@/components/ui/StatCard.vue';
import VChartLazy from '@/components/ui/VChartLazy.vue';

const route = useRoute();
const router = useRouter();
const ui = useUiStore();

const classId = computed(() => Number(route.params.id));
const report = ref<ClassReportDto | null>(null);
const loading = ref(true);
const exporting = ref(false);

const pad = (n: number): string => String(n).padStart(2, '0');

/** Tổng bài nộp (đúng hạn + trễ) vs kỳ vọng (bài gán × thành viên) → % hoàn thành. */
const totals = computed(() => {
  const r = report.value;
  if (!r) return { submitted: 0, expected: 0, pct: 0 };
  const submitted = r.assignments.reduce((sum, a) => sum + a.onTime + a.late, 0);
  const expected = r.assignments.length * r.totalMembers;
  return { submitted, expected, pct: expected > 0 ? Math.round((submitted / expected) * 100) : 0 };
});

const avgScore = computed(() => {
  const r = report.value;
  if (!r || r.assignments.length === 0) return '—';
  const avg = r.assignments.reduce((sum, a) => sum + a.avgScore, 0) / r.assignments.length;
  return avg.toFixed(1);
});

/** KPI phụ (level-1 — KHÔNG icon tròn, không shadow; tối đa 1 hero-stat/màn).
    Điểm TB đã vào hero tối → đây chỉ giữ 3 chỉ số còn lại. */
const secondaryKpis = computed(() => {
  const r = report.value;
  if (!r) return [];
  return [
    { label: messages.classes.reportKpiMembers, value: formatNumber(r.totalMembers) },
    { label: messages.classes.reportKpiAssignments, value: formatNumber(r.assignments.length) },
    { label: messages.classes.reportKpiSubmissions, value: formatNumber(totals.value.submitted) },
  ];
});

/** Tổng phân bố nộp bài (đúng hạn / trễ / chưa nộp) — cộng dồn các bài gán (dữ liệu thật). */
const chartTotals = computed(() => {
  const r = report.value;
  if (!r) return { onTime: 0, late: 0, missing: 0, total: 0 };
  const onTime = r.assignments.reduce((sum, a) => sum + a.onTime, 0);
  const late = r.assignments.reduce((sum, a) => sum + a.late, 0);
  const missing = r.assignments.reduce((sum, a) => sum + a.notSubmitted, 0);
  return { onTime, late, missing, total: onTime + late + missing };
});

function cssVar(name: string, fallback: string): string {
  if (typeof document === 'undefined') return fallback;
  const value = getComputedStyle(document.documentElement).getPropertyValue(name).trim();
  return value || fallback;
}

/** Donut phân bố nộp bài — nền LUÔN tối (canvas-ink, vùng dữ liệu); màu semantic:
    resolved/warning/conflict; label + legend tương phản cao trên nền tối (#D9DDE8 —
    canvas text, precedent ClassReportView lagging-name). */
const distributionOption = computed(() => {
  // Phụ thuộc theme (ui.theme) → recompute option khi toggle sáng/tối
  void ui.theme;
  const ink = cssVar('--color-canvas-ink', '#0D1020');
  const canvasText = '#D9DDE8';
  const resolved = cssVar('--color-resolved', '#34D399');
  const warning = cssVar('--color-warning', '#D97706');
  const conflict = cssVar('--color-conflict', '#F87171');
  const t = chartTotals.value;

  return {
    tooltip: {
      trigger: 'item' as const,
      backgroundColor: ink,
      borderColor: 'rgba(255, 255, 255, 0.16)',
      textStyle: { color: canvasText, fontSize: 12 },
      formatter: (p: { name: string; value: number; percent: number }) =>
        `${p.name}<br/><b>${p.value}</b> (${p.percent}%)`,
    },
    legend: {
      bottom: 0,
      icon: 'circle' as const,
      itemWidth: 8,
      itemHeight: 8,
      itemGap: 20,
      textStyle: { color: canvasText, fontSize: 12 },
    },
    series: [
      {
        type: 'pie' as const,
        radius: ['52%', '72%'],
        center: ['50%', '42%'],
        avoidLabelOverlap: true,
        itemStyle: { borderRadius: 4, borderColor: ink, borderWidth: 2 },
        label: {
          show: true,
          position: 'outside' as const,
          color: canvasText,
          fontSize: 12,
          formatter: '{d}%',
        },
        labelLine: { lineStyle: { color: 'rgba(255, 255, 255, 0.28)' } },
        emphasis: { scaleSize: 4 },
        data: [
          { name: messages.classes.reportChartOnTime, value: t.onTime, itemStyle: { color: resolved } },
          { name: messages.classes.reportChartLate, value: t.late, itemStyle: { color: warning } },
          { name: messages.classes.reportChartMissing, value: t.missing, itemStyle: { color: conflict } },
        ],
      },
    ],
  };
});

/** Hành động "Nhắc nhở" (Task 2): sao chép lời nhắc chuẩn bị sẵn cho từng học viên
    chậm tiến độ — dùng clipboard (không cần API mới), teacher tự dán vào kênh liên hệ. */
async function copyReminder(learner: LaggingLearnerDto): Promise<void> {
  const message = messages.classes.reportLaggingRemindMsg(
    learner.displayName,
    report.value?.className ?? '',
    learner.missingCount,
  );
  // FIX REVIEW: clipboard có thể không khả dụng (HTTP non-secure / trình duyệt chặn) —
  // optional chain cũ → await undefined → toast "Đã sao chép" giả. Check tường minh.
  if (!navigator.clipboard) {
    ui.showToast(messages.classes.reportLaggingRemindFail, 'error');
    return;
  }
  try {
    await navigator.clipboard.writeText(message);
    ui.showToast(messages.classes.reportLaggingRemindDone, 'success');
  } catch {
    ui.showToast(messages.classes.reportLaggingRemindFail, 'error');
  }
}

function assignmentStatus(assign: ClassReportAssignmentDto): { label: string; variant: 'success' | 'warning' | 'muted' } {
  if (assign.late > 0) return { label: messages.classes.statusLate, variant: 'warning' };
  if (assign.notSubmitted === 0) return { label: messages.classes.statusCompleted, variant: 'success' };
  return { label: messages.classes.statusNotStarted, variant: 'muted' };
}

onMounted(load);

async function load(): Promise<void> {
  loading.value = true;
  try {
    report.value = await classesApi.fetchClassReport(classId.value);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.classes.reportLoadError, 'error');
  } finally {
    loading.value = false;
  }
}

async function exportCsv(): Promise<void> {
  exporting.value = true;
  try {
    const csv = await classesApi.exportClassReportCsv(classId.value);
    const blob = new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = `class-report-${classId.value}.csv`;
    link.click();
    URL.revokeObjectURL(url);
    ui.showToast(messages.classes.reportCsvDone, 'success');
  } catch {
    ui.showToast(messages.classes.reportCsvFail, 'error');
  } finally {
    exporting.value = false;
  }
}

function printReport(): void {
  window.print();
}
</script>

<template>
  <main class="class-report container">
    <nav class="class-report__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'classes' }">{{ messages.classes.detailBreadcrumb }}</RouterLink>
      <span aria-hidden="true">/</span>
      <RouterLink :to="{ name: 'class-detail', params: { id: String(classId) } }">
        {{ messages.classes.detailFallbackTitle }}
      </RouterLink>
      <span aria-hidden="true">/</span>
      <span>{{ messages.classes.reportTitle }}</span>
    </nav>

    <!-- Banner: PageHero shared (surface band level-2 compact — DESIGN §1/#1) -->
    <PageHero border="full" padding="lg">
      <template #title>{{ messages.classes.reportTitle }}</template>
      <template #description>
        <span class="class-report__sub">{{ report?.className ?? '…' }} · ID {{ pad(classId) }}</span>
      </template>
      <template #side>
        <div class="class-report__actions">
          <Button size="md" :loading="exporting" @click="exportCsv">
            <Download :size="14" aria-hidden="true" /> {{ messages.classes.reportExportCsv }}
          </Button>
          <Button size="md" variant="secondary" @click="printReport">
            <Printer :size="14" aria-hidden="true" /> {{ messages.classes.reportPrint }}
          </Button>
        </div>
      </template>
    </PageHero>

    <div v-if="loading" class="class-report__loading" aria-busy="true">
      <div class="class-report__kpis">
        <Skeleton v-for="i in 4" :key="i" height="120px" />
      </div>
      <div class="class-report__summary">
        <Skeleton height="72px" />
      </div>
      <Skeleton height="280px" />
    </div>

    <EmptyState
      v-else-if="!report"
      icon="database"
      :title="messages.classes.reportEmptyTitle"
      :description="messages.classes.reportEmptyDesc"
      :action-label="messages.classes.reportBackDetail"
      @action="router.push({ name: 'class-detail', params: { id: String(classId) } })"
    />

    <template v-else>
      <!-- Hero KPI (Task 2): 1 hero-stat/màn — panel LUÔN tối canvas-ink chứa tỷ lệ
           hoàn thành + điểm trung bình (block-token + index mono, DESIGN §1/#3/#5) -->
      <Card class="class-report__hero">
        <div class="class-report__hero-panel">
          <div class="class-report__hero-block">
            <p
              class="class-report__hero-value"
              :class="{ 'class-report__hero-value--done': totals.pct >= 100 }"
            >
              {{ totals.pct }}<span class="class-report__hero-unit">%</span>
            </p>
            <p class="class-report__hero-label">{{ messages.classes.reportHeroCompletion }}</p>
            <p class="class-report__hero-index">
              {{ pad(totals.submitted) }} / {{ pad(totals.expected) }} BÀI NỘP
            </p>
          </div>
          <span class="class-report__hero-divider" aria-hidden="true" />
          <div class="class-report__hero-block">
            <p class="class-report__hero-value">{{ avgScore }}</p>
            <p class="class-report__hero-label">{{ messages.classes.reportHeroAvgScore }}</p>
            <p class="class-report__hero-index">{{ messages.classes.reportHeroScale }}</p>
          </div>
        </div>
      </Card>

      <!-- KPI phụ: 3 stat level-1 (members / assignments / submissions) -->
      <div class="class-report__kpis">
        <StatCard
          v-for="kpi in secondaryKpis"
          :key="kpi.label"
          :label="kpi.label"
          :value="kpi.value"
        />
      </div>

      <!-- Phân bố nộp bài (Task 2): CHART CONTAINER nền LUÔN tối canvas-ink —
           donut ECharts (vue-echarts sẵn có) + legend/label tương phản cao.
           Grid 12: chart 8 cột + lagging 4 cột (DESIGN §5) — lagging luôn hiển thị. -->
      <div class="class-report__grid">
        <Card v-if="report.assignments.length > 0" class="class-report__chart-card">
          <div class="class-report__chart-head">
            <h2 class="class-report__chart-title">{{ messages.classes.reportChartTitle }}</h2>
            <span class="class-report__chart-total">{{ messages.classes.reportChartTotal(formatNumber(chartTotals.total)) }}</span>
          </div>
          <div class="class-report__chart">
            <VChartLazy :option="distributionOption" height="264px" />
          </div>
        </Card>

        <!-- Học viên chậm tiến độ (Task 2): card nổi bật — badge đỏ dịu destructive
             + nút hành động nhắc nhở rõ ràng (sao chép lời nhắc) -->
        <Card
          class="class-report__lagging"
          :class="{ 'class-report__lagging--full': report.assignments.length === 0 }"
        >
          <div class="class-report__lagging-head">
            <h2 class="class-report__lagging-title">{{ messages.classes.reportLaggingTitle }}</h2>
            <span v-if="report.laggingLearners.length > 0" class="class-report__lagging-count">
              {{ messages.classes.reportLaggingCount(report.laggingLearners.length) }}
            </span>
          </div>
          <div v-if="report.laggingLearners.length === 0" class="class-report__lagging-empty">
            <Check :size="14" class="text-resolved" aria-hidden="true" />
            <span class="class-report__lagging-empty-text">{{ messages.classes.reportLaggingEmpty }}</span>
          </div>
          <ul v-else class="class-report__lagging-list">
            <li v-for="(learner, i) in report.laggingLearners" :key="learner.userId" class="class-report__lagging-row">
              <span class="class-report__lagging-index" aria-hidden="true">#{{ pad(i + 1) }}</span>
              <span class="class-report__lagging-name">{{ learner.displayName }}</span>
              <span class="class-report__lagging-badge">{{ messages.classes.reportLaggingMissing(learner.missingCount) }}</span>
              <Button size="sm" variant="secondary" class="class-report__remind-btn" @click="copyReminder(learner)">
                <Copy :size="14" aria-hidden="true" /> {{ messages.classes.reportLaggingRemind }}
              </Button>
            </li>
          </ul>
        </Card>
      </div>

      <!-- Bảng bài gán (cột số mono — DESIGN §4.6) -->
      <template v-if="report.assignments.length > 0">
        <Card class="class-report__table">
          <div class="class-report__table-scroll">
            <table>
              <thead>
                <tr>
                  <th scope="col">#</th>
                  <th scope="col">{{ messages.classes.reportColContent }}</th>
                  <th scope="col">{{ messages.classes.reportColOnTime }}</th>
                  <th scope="col">{{ messages.classes.reportColLate }}</th>
                  <th scope="col">{{ messages.classes.reportColNotSubmitted }}</th>
                  <th scope="col">{{ messages.classes.reportColBest }}</th>
                  <th scope="col">{{ messages.classes.reportColStatus }}</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="(assign, i) in report.assignments"
                  :key="assign.assignmentId"
                  class="hover:bg-muted/50"
                >
                  <td class="class-report__num" data-label="#">#{{ pad(i + 1) }}</td>
                  <td :data-label="messages.classes.reportColContent">
                    <p class="class-report__assign-title">{{ assign.title }}</p>
                    <p class="class-report__assign-due">
                      {{ assign.dueAt ? messages.classes.detailDue(formatDate(assign.dueAt)) : messages.classes.detailDueNone }}
                    </p>
                  </td>
                  <td class="class-report__num" :data-label="messages.classes.reportColOnTime">{{ assign.onTime }}</td>
                  <td class="class-report__num" :data-label="messages.classes.reportColLate">{{ assign.late }}</td>
                  <td class="class-report__num" :data-label="messages.classes.reportColNotSubmitted">{{ assign.notSubmitted }}</td>
                  <td class="class-report__num" :data-label="messages.classes.reportColBest">{{ assign.avgScore.toFixed(1) }}</td>
                  <td :data-label="messages.classes.reportColStatus">
                    <Badge :variant="assignmentStatus(assign).variant">
                      {{ assignmentStatus(assign).label }}
                    </Badge>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </Card>
      </template>
      <EmptyState
        v-else
        icon="book"
        :title="messages.classes.reportEmptyTitle"
        :description="messages.classes.reportEmptyDesc"
        :action-label="messages.classes.reportBackDetail"
        @action="router.push({ name: 'class-detail', params: { id: String(classId) } })"
      />
    </template>
  </main>
</template>

<style scoped>
.class-report {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.class-report__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  color: var(--foreground-secondary);
  flex-wrap: wrap;
}

/* ── Banner: PageHero shared — sub mono trong slot #description ── */
.class-report__sub {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  color: var(--foreground-tertiary);
  margin: 0;
  display: block;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 60ch;
}

.class-report__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

/* ── Loading ── */
.class-report__loading { display: flex; flex-direction: column; gap: var(--space-md); }

/* ── KPI: 3 stat phụ level-1 (hero tối đã nằm ở class-report__hero) ── */
.class-report__kpis {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: var(--space-md);
}

/* ── Hero KPI (Task 2): card level-2 + panel LUÔN tối canvas-ink — block-token
   lớn + index mono (DESIGN §1/#3/#5; tối đa 1 hero-stat/màn) ── */
.class-report__hero {
  background: var(--card-raised);
  border-color: var(--border-subtle);
  padding: var(--space-md);
}

.class-report__hero-panel {
  display: flex;
  align-items: stretch;
  gap: var(--space-xl);
  background: var(--canvas-ink);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: var(--radius-lg);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: 0 8px 24px -6px rgba(15, 23, 42, 0.35);
  opacity: 0;
  transform: translateY(6px);
  animation: report-hero-enter 280ms cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

.class-report__hero-block {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
}

.class-report__hero-value {
  margin: 0;
  font-size: var(--text-4xl);
  font-weight: 600;
  letter-spacing: -0.03em;
  line-height: 1.1;
  color: var(--data-core);
  font-variant-numeric: tabular-nums;
}

.class-report__hero-value--done { color: var(--resolved); }

.class-report__hero-unit { font-size: var(--text-2xl); letter-spacing: -0.015em; }

.class-report__hero-label {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  color: var(--index-muted);
}

.class-report__hero-index {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--index-muted);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.class-report__hero-divider {
  width: 1px;
  background: rgba(255, 255, 255, 0.12);
  flex-shrink: 0;
}

@keyframes report-hero-enter {
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .class-report__hero-panel {
    animation: none;
    opacity: 1;
    transform: none;
  }
}

/* ── Grid 12: chart 7 cột + lagging 5 cột (~60/40) ── */
.class-report__grid {
  display: grid;
  grid-template-columns: repeat(12, minmax(0, 1fr));
  gap: var(--space-md);
  align-items: stretch;
}

.class-report__chart-card { grid-column: span 7; }

.class-report__lagging { grid-column: span 5; }

/* Không có bài gán (chưa có chart) → lagging full width.
   FIX REVIEW: thay :not(:has(...)) (Chrome<105/FF<121 không chạy → card lagging kẹt
   span 4 cột) bằng :class binding trên Card (class-report__lagging--full). */
.class-report__lagging--full {
  grid-column: 1 / -1;
}

/* ── Chart container: nền LUÔN tối canvas-ink (vùng dữ liệu — Task 2) ── */
.class-report__chart-card { display: flex; flex-direction: column; gap: var(--space-sm); }

.class-report__chart-head {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: var(--space-sm);
  flex-wrap: wrap;
}

.class-report__chart-title {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  letter-spacing: 0.08em;
  color: var(--foreground-tertiary);
}

.class-report__chart-total {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  letter-spacing: 0.08em;
  font-variant-numeric: tabular-nums;
}

.class-report__chart {
  background: var(--canvas-ink);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: var(--radius-lg);
  padding: var(--space-md) var(--space-md) var(--space-sm);
  box-shadow: 0 8px 24px -6px rgba(15, 23, 42, 0.35);
  min-width: 0;
}

/* ── Bảng bài gán (DESIGN §4.6) ── */
.class-report__table { padding: 0; }

.class-report__table-scroll { overflow-x: auto; border-radius: inherit; }

.class-report__table table { width: 100%; border-collapse: collapse; }

.class-report__table th {
  text-align: left;
  font-size: var(--text-sm);
  font-weight: 500;
  color: var(--foreground-tertiary);
  padding: 0 var(--space-md);
  border-bottom: 1px solid var(--border);
  background: var(--muted);
  white-space: nowrap;
  height: 40px;
}

.class-report__table td {
  padding: 12px var(--space-md);
  border-bottom: 1px solid var(--border);
  font-size: var(--text-sm);
  vertical-align: middle;
}

.class-report__table tbody tr:last-child td { border-bottom: none; }

.class-report__assign-title {
  font-weight: 500;
  color: var(--foreground);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 320px;
}

.class-report__assign-due {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  margin-top: var(--space-xs);
  white-space: nowrap;
}

.class-report__num {
  text-align: center;
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-variant-numeric: tabular-nums;
  white-space: nowrap;
}

/* ── Lagging learners (Task 2): card nổi bật — list block-token tối + index mono,
   badge đỏ dịu destructive + nút nhắc nhở rõ ràng ── */
.class-report__lagging { display: flex; flex-direction: column; gap: var(--space-sm); padding: var(--space-lg); min-width: 0; }

.class-report__lagging-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
  flex-wrap: wrap;
}

.class-report__lagging-title {
  font-size: var(--text-lg);
  font-weight: 600;
  letter-spacing: -0.015em;
  line-height: 1.25;
  margin: 0;
  color: var(--foreground);
}

.class-report__lagging-count {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  color: var(--destructive);
  white-space: nowrap;
}

.class-report__lagging-empty {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  align-self: flex-start;
  border: 1px solid rgba(52, 211, 153, 0.35);
  background: var(--canvas-ink);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
}

.class-report__lagging-empty-text {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--resolved);
}

.class-report__lagging-list {
  list-style: none;
  margin: 0;
  padding: var(--space-sm);
  display: flex;
  flex-direction: column;
  gap: 2px;
  background: var(--canvas-ink);
  border: 1px solid rgba(66, 85, 255, 0.3);
  border-radius: var(--radius-md);
}

.class-report__lagging-row {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-sm);
  border-radius: var(--radius-sm);
}

.class-report__lagging-row:hover { background: rgba(255, 255, 255, 0.05); }

.class-report__lagging-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--index-muted);
  min-width: 28px;
}

.class-report__lagging-name {
  flex: 1;
  min-width: 0;
  font-size: var(--text-sm);
  /* #d9dde8 = màu text canvas engine (canvasTheme.ts) — xem pm-decision-log-viewquality 14/08 */
  color: #d9dde8;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* Badge đỏ dịu (Task 2): số bài thiếu — destructive tint + border (DESIGN §2.2) */
.class-report__lagging-badge {
  display: inline-flex;
  align-items: center;
  min-height: 24px;
  padding: 2px 10px;
  border-radius: var(--radius-md);
  border: 1px solid color-mix(in srgb, var(--destructive) 35%, transparent);
  background: color-mix(in srgb, var(--destructive) 12%, transparent);
  color: var(--destructive);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  white-space: nowrap;
}

/* Nút nhắc nhở TRÊN NỀN TỐI: màu sáng tường minh (không phụ thuộc theme — text
   foreground light-mode tối sẽ không đọc được trên canvas-ink) */
.class-report__remind-btn {
  color: #d9dde8;
  border-color: rgba(255, 255, 255, 0.16);
  background: rgba(255, 255, 255, 0.06);
}

.class-report__remind-btn:hover {
  color: #ffffff;
  background: rgba(255, 255, 255, 0.12);
}

@media (max-width: 1023px) {
  .class-report__kpis { grid-template-columns: repeat(2, 1fr); }
  .class-report__grid { grid-template-columns: 1fr; }
  .class-report__chart-card,
  .class-report__lagging { grid-column: auto; }
}

@media (max-width: 640px) {
  .class-report__sub { white-space: normal; }
  .class-report__kpis { grid-template-columns: 1fr; }

  /* Hero: 2 block xếp dọc, divider chuyển thành line ngang (DESIGN §8 spacing) */
  .class-report__hero-panel {
    flex-direction: column;
    gap: var(--space-md);
    padding: var(--space-md);
  }

  .class-report__hero-divider {
    width: auto;
    height: 1px;
  }

  .class-report__hero-value { font-size: var(--text-3xl); }

  .class-report__lagging-row { flex-wrap: wrap; }
  .class-report__remind-btn { margin-left: auto; }

  /* Bảng → card-stack (DESIGN §8 — cấm scroll ngang bảng chính ở mobile) */
  .class-report__table-scroll { overflow-x: visible; }

  .class-report__table thead { display: none; }

  .class-report__table tbody tr {
    display: grid;
    grid-template-columns: auto 1fr;
    gap: var(--space-xs) var(--space-md);
    padding: var(--space-sm) var(--space-md);
    border-bottom: 1px solid var(--border);
  }

  .class-report__table tbody tr:last-child { border-bottom: none; }

  .class-report__table td {
    display: flex;
    flex-direction: column;
    gap: var(--space-xs);
    padding: 0;
    border-bottom: none;
    text-align: left;
  }

  .class-report__table td::before {
    content: attr(data-label);
    font-size: var(--text-xs);
    color: var(--foreground-tertiary);
  }

  .class-report__table td:first-child { grid-row: span 2; align-items: flex-start; padding-top: 2px; }
  .class-report__table td:first-child::before { content: none; }

  .class-report__assign-title { max-width: 100%; white-space: normal; }
}
</style>
