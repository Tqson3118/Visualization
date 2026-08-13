<script setup lang="ts">
// ClassReportView — Màn 21: báo cáo lớp (KPI + bảng sinh viên + xuất CSV)
// H-C: hero gradient Sunset compact + KPI Card shadcn (chuẩn AdminStats) + summary
// ProgressBar + table chuẩn AdminUsers. GIỮ nguyên logic API/store.
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import {
  Award,
  BarChart3,
  ClipboardCheck,
  Download,
  Gauge,
  Printer,
  Users,
} from 'lucide-vue-next';

import * as classesApi from '@/api/classes';
import type { ClassReportDto } from '@/api/types';
import { useUiStore } from '@/stores/ui';
import { formatNumber } from '@/utils/format';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import { Card, CardContent, CardDescription, CardHeader } from '@/components/ui/card';

const route = useRoute();
const router = useRouter();
const ui = useUiStore();

const classId = computed(() => Number(route.params.id));
const report = ref<ClassReportDto | null>(null);
const loading = ref(true);
const exporting = ref(false);

const initial = (name: string): string => (name.trim() ? name.trim().charAt(0).toUpperCase() : '?');

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

const statusBadge: Record<string, { label: string; variant: 'success' | 'warning' | 'muted' }> = {
  completed: { label: messages.classes.statusCompleted, variant: 'success' },
  late: { label: messages.classes.statusLate, variant: 'warning' },
  not_started: { label: messages.classes.statusNotStarted, variant: 'muted' },
};

const KPIS = [
  { label: messages.classes.reportKpiMembers, value: () => (report.value ? formatNumber(report.value.totalMembers) : '—'), icon: Users, tint: 'class-report__kpi-icon--aurora' },
  { label: messages.classes.reportKpiCompletion, value: () => (report.value ? `${report.value.completionPct}%` : '—'), icon: Gauge, tint: 'class-report__kpi-icon--sunset' },
  { label: messages.classes.reportKpiAvgScore, value: () => (report.value?.avgScore ?? '—'), icon: Award, tint: 'class-report__kpi-icon--mint' },
  { label: messages.classes.reportKpiSubmissions, value: () => (report.value ? formatNumber(report.value.submissions) : '—'), icon: ClipboardCheck, tint: 'class-report__kpi-icon--aurora' },
];

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

    <!-- Hero gradient Sunset compact -->
    <header class="class-report__hero">
      <div class="class-report__hero-top">
        <div class="class-report__hero-main">
          <span class="class-report__hero-icon" aria-hidden="true"><BarChart3 :size="22" /></span>
          <div class="class-report__hero-text">
            <h1 class="class-report__title">{{ messages.classes.reportTitle }}</h1>
            <p class="class-report__sub">
              {{ report?.className ?? '...' }} · ID {{ classId }}
            </p>
          </div>
        </div>
        <div class="class-report__actions">
          <Button size="sm" :loading="exporting" @click="exportCsv">
            <Download :size="14" aria-hidden="true" /> {{ messages.classes.reportExportCsv }}
          </Button>
          <Button size="sm" variant="secondary" @click="printReport">
            <Printer :size="14" aria-hidden="true" /> {{ messages.classes.reportPrint }}
          </Button>
        </div>
      </div>
    </header>

    <div v-if="loading" class="class-report__loading" aria-busy="true">
      <div class="class-report__kpis">
        <Skeleton v-for="i in 4" :key="i" height="108px" />
      </div>
      <div class="class-report__summary">
        <Skeleton height="72px" />
      </div>
      <Skeleton height="280px" />
    </div>

    <EmptyState
      v-else-if="!report"
      icon="chart"
      :title="messages.classes.reportEmptyTitle"
      :description="messages.classes.reportEmptyDesc"
      :action-label="messages.classes.reportBackDetail"
      @action="router.push({ name: 'class-detail', params: { id: String(classId) } })"
    />

    <template v-else>
      <!-- 4 KPI — Card shadcn + hover-lift -->
      <div class="class-report__kpis">
        <Card v-for="kpi in KPIS" :key="kpi.label" class="class-report__kpi hover-lift">
          <CardHeader class="class-report__kpi-head">
            <span class="class-report__kpi-icon" :class="kpi.tint" aria-hidden="true">
              <component :is="kpi.icon" :size="18" />
            </span>
            <CardDescription class="class-report__kpi-label">{{ kpi.label }}</CardDescription>
          </CardHeader>
          <CardContent class="class-report__kpi-body">
            <p class="class-report__kpi-value">{{ kpi.value() }}</p>
          </CardContent>
        </Card>
      </div>

      <!-- Tỷ lệ hoàn thành -->
      <Card class="class-report__summary">
        <p class="class-report__summary-text text-muted">{{ messages.classes.reportSummary }}</p>
        <ProgressBar
          :value="report.completionPct"
          show-label
          :variant="report.completionPct >= 100 ? 'success' : 'default'"
        />
      </Card>

      <!-- Bảng sinh viên -->
      <Card class="class-report__table">
        <div class="class-report__table-scroll">
          <table>
            <thead>
              <tr>
                <th>{{ messages.classes.reportColStudent }}</th>
                <th>{{ messages.classes.reportColViewed }}</th>
                <th>{{ messages.classes.reportColSims }}</th>
                <th>{{ messages.classes.reportColExercises }}</th>
                <th>{{ messages.classes.reportColBest }}</th>
                <th>{{ messages.classes.reportColStatus }}</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in report.rows" :key="row.studentId">
                <td>
                  <div class="class-report__user">
                    <span class="class-report__avatar" aria-hidden="true">{{ initial(row.displayName) }}</span>
                    <div class="class-report__user-meta">
                      <p class="class-report__name">{{ row.displayName }}</p>
                      <p class="class-report__email text-muted">{{ row.email }}</p>
                    </div>
                  </div>
                </td>
                <td class="class-report__center">{{ row.viewed ? '✓' : '—' }}</td>
                <td class="class-report__center">{{ row.simulationsRun }}</td>
                <td class="class-report__center">{{ row.exercisesCompleted }}</td>
                <td class="class-report__center">{{ row.bestScore ?? '—' }}</td>
                <td>
                  <Badge :variant="statusBadge[row.status]?.variant ?? 'muted'">
                    {{ statusBadge[row.status]?.label ?? row.status }}
                  </Badge>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </Card>
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
  color: var(--color-text-muted);
  flex-wrap: wrap;
}

/* ── Hero gradient Sunset compact (GP-T9b dark overlay) ── */
.class-report__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border-radius: var(--radius-xl);
  background-image: var(--gradient-sunset);
  color: #fff;
  box-shadow: var(--shadow-lg);
  padding: var(--space-lg) var(--space-xl);
}

.class-report__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: linear-gradient(120deg, rgba(255, 255, 255, 0.16), transparent 55%);
}

.dark .class-report__hero::after {
  background: rgba(4, 47, 46, 0.62);
}

.class-report__hero-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.class-report__hero-main { display: flex; align-items: center; gap: var(--space-md); min-width: 0; }

.class-report__hero-icon {
  width: 46px;
  height: 46px;
  border-radius: var(--radius-lg);
  background: rgba(255, 255, 255, 0.16);
  border: 1px solid rgba(255, 255, 255, 0.3);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  color: #fff;
}

.class-report__hero-text { display: flex; flex-direction: column; gap: 2px; min-width: 0; }

.class-report__title {
  font-size: var(--text-2xl);
  margin: 0;
  text-shadow: 0 2px 10px rgba(0, 0, 0, 0.16);
}

.class-report__sub {
  color: rgba(255, 255, 255, 0.92);
  font-size: var(--text-sm);
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 60ch;
}

.class-report__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

/* ── Loading ── */
.class-report__loading { display: flex; flex-direction: column; gap: var(--space-md); }

/* ── KPI ── */
.class-report__kpis { display: grid; grid-template-columns: repeat(auto-fit, minmax(170px, 1fr)); gap: var(--space-md); }

.class-report__kpi { display: flex; flex-direction: column; }

.class-report__kpi-head { display: flex; flex-direction: row; align-items: center; gap: var(--space-sm); padding: var(--space-md) var(--space-md) 0; }

.class-report__kpi-icon {
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

.class-report__kpi-icon--aurora { background-image: var(--gradient-aurora); }
.class-report__kpi-icon--sunset { background-image: var(--gradient-sunset); }
.class-report__kpi-icon--mint { background-image: var(--gradient-mint); }

.class-report__kpi-label { font-size: var(--text-xs); font-weight: 600; }

.class-report__kpi-body { padding: var(--space-xs) var(--space-md) var(--space-md); }

.class-report__kpi-value {
  font-size: var(--text-2xl);
  font-weight: 800;
  color: var(--color-foreground);
  font-variant-numeric: tabular-nums;
  line-height: 1.2;
}

/* ── Summary ── */
.class-report__summary { display: flex; flex-direction: column; gap: var(--space-sm); padding: var(--space-md) var(--space-lg); }

.class-report__summary-text { font-size: var(--text-sm); }

/* ── Bảng sinh viên ── */
.class-report__table { padding: 0; }

.class-report__table-scroll { overflow-x: auto; border-radius: inherit; }

.class-report__table table { width: 100%; border-collapse: collapse; min-width: 640px; }

.class-report__table th {
  text-align: left;
  font-size: var(--text-xs);
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
  padding: var(--space-sm) var(--space-md);
  border-bottom: 1px solid var(--color-border);
  background: var(--color-muted);
  white-space: nowrap;
}

.class-report__table td {
  padding: var(--space-sm) var(--space-md);
  border-bottom: 1px solid var(--color-border);
  font-size: var(--text-sm);
  vertical-align: middle;
}

.class-report__table tbody tr { transition: background-color 150ms ease; }

.class-report__table tbody tr:hover { background: color-mix(in srgb, var(--color-primary) 5%, transparent); }

.class-report__table tbody tr:last-child td { border-bottom: none; }

.class-report__user { display: flex; align-items: center; gap: var(--space-sm); min-width: 0; }

.class-report__avatar {
  width: 34px;
  height: 34px;
  border-radius: 50%;
  background-image: var(--gradient-mint);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  font-size: var(--text-xs);
  flex-shrink: 0;
}

.class-report__user-meta { min-width: 0; }

.class-report__name { font-weight: 700; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 240px; }
.class-report__email { font-size: var(--text-xs); white-space: nowrap; overflow: hidden; text-overflow: ellipsis; max-width: 240px; }

.class-report__center { text-align: center; font-variant-numeric: tabular-nums; white-space: nowrap; }

@media (max-width: 640px) {
  .class-report__hero { padding: var(--space-lg); }
  .class-report__sub { white-space: normal; }
}
</style>
