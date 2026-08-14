<script setup lang="ts">
// ClassReportView — Màn 21: báo cáo lớp (1 hero-stat + KPI phụ + bảng bài gán + lagging learners)
// View-quality Phase 1 (Nhóm D):
//  - Banner = surface band level-2 + sub mono; hero-stat duy nhất = block-token tối
//    (quyết định #3/#4/#5); bảng chuẩn §4.6 + cột số mono + mobile card-stack.
//  - 14/08: align contract API với backend THẬT (ClassReportDto = totalMembers/
//    assignments[]/laggingLearners[] — trước đây view đọc completionPct/rows không tồn
//    tại → "undefined%"/"NaN"/bảng rỗng). Bảng chuyển sang thống kê từng bài gán +
//    block lagging learners (dữ liệu thật, block-token + index mono).
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { Check, Download, Printer } from 'lucide-vue-next';

import * as classesApi from '@/api/classes';
import type { ClassReportAssignmentDto, ClassReportDto } from '@/api/types';
import { useUiStore } from '@/stores/ui';
import { formatDate, formatNumber } from '@/utils/format';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import { Card } from '@/components/ui/card';
import PageHero from '@/components/ui/PageHero.vue';
import StatCard from '@/components/ui/StatCard.vue';

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

/** KPI phụ (level-1 — KHÔNG icon tròn, không shadow; tối đa 1 hero-stat/màn). */
const secondaryKpis = computed(() => {
  const r = report.value;
  if (!r) return [];
  return [
    { label: messages.classes.reportKpiAssignments, value: formatNumber(r.assignments.length) },
    { label: messages.classes.reportKpiAvgScore, value: avgScore.value },
    { label: messages.classes.reportKpiSubmissions, value: formatNumber(totals.value.submitted) },
  ];
});

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
      <!-- KPI: 1 hero-stat (StatCard hero — block-token tối) + 3 stat level-1 (quyết định #3) -->
      <div class="class-report__kpis">
        <StatCard
          level="hero"
          :label="messages.classes.reportKpiMembers"
          :value="formatNumber(report.totalMembers)"
        />
        <StatCard
          v-for="kpi in secondaryKpis"
          :key="kpi.label"
          :label="kpi.label"
          :value="kpi.value"
        />
      </div>

      <!-- Tỷ lệ hoàn thành -->
      <Card class="class-report__summary">
        <p class="class-report__summary-text">{{ messages.classes.reportSummary }}</p>
        <ProgressBar
          :value="totals.pct"
          show-label
          :variant="totals.pct >= 100 ? 'success' : 'default'"
        />
        <p class="class-report__summary-mono">
          {{ pad(totals.submitted) }} / {{ pad(totals.expected) }} BÀI NỘP
        </p>
      </Card>

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

      <!-- Học viên chậm tiến độ: block-token tối + index mono (quyết định #4/#5) -->
      <Card class="class-report__lagging">
        <h2 class="class-report__lagging-title">{{ messages.classes.reportLaggingTitle }}</h2>
        <div v-if="report.laggingLearners.length === 0" class="class-report__lagging-empty">
          <Check :size="14" class="text-resolved" aria-hidden="true" />
          <span class="class-report__lagging-empty-text">{{ messages.classes.reportLaggingEmpty }}</span>
        </div>
        <ul v-else class="class-report__lagging-list">
          <li v-for="(learner, i) in report.laggingLearners" :key="learner.userId" class="class-report__lagging-row">
            <span class="class-report__lagging-index" aria-hidden="true">#{{ pad(i + 1) }}</span>
            <span class="class-report__lagging-name">{{ learner.displayName }}</span>
            <span class="class-report__lagging-missing">{{ messages.classes.reportLaggingMissing(learner.missingCount) }}</span>
          </li>
        </ul>
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

/* ── KPI: 1 hero-stat (StatCard) + 3 stat phụ ── */
.class-report__kpis {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--space-md);
}

/* ── Summary ── */
.class-report__summary { display: flex; flex-direction: column; gap: var(--space-sm); padding: var(--space-md) var(--space-lg); }

.class-report__summary-text { font-size: var(--text-sm); color: var(--foreground-secondary); }

.class-report__summary-mono {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  letter-spacing: 0.08em;
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

/* ── Lagging learners: block-token tối + index mono ── */
.class-report__lagging { display: flex; flex-direction: column; gap: var(--space-sm); padding: var(--space-lg); }

.class-report__lagging-title {
  font-size: var(--text-lg);
  font-weight: 600;
  letter-spacing: -0.015em;
  line-height: 1.25;
  margin: 0;
  color: var(--foreground);
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
  padding: var(--space-sm) var(--space-sm);
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

.class-report__lagging-missing {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--conflict);
  white-space: nowrap;
}

@media (max-width: 1023px) {
  .class-report__kpis { grid-template-columns: repeat(2, 1fr); }
}

@media (max-width: 640px) {
  .class-report__sub { white-space: normal; }
  .class-report__kpis { grid-template-columns: 1fr; }

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
