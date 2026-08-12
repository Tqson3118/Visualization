<script setup lang="ts">
// ClassReportView — Màn 21: báo cáo lớp (KPI + bảng sinh viên + xuất CSV)
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import * as classesApi from '@/api/classes';
import type { ClassReportDto } from '@/api/types';
import { useUiStore } from '@/stores/ui';
import { formatNumber } from '@/utils/format';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';

const route = useRoute();
const router = useRouter();
const ui = useUiStore();

const classId = computed(() => Number(route.params.id));
const report = ref<ClassReportDto | null>(null);
const loading = ref(true);
const exporting = ref(false);

onMounted(load);

async function load(): Promise<void> {
  loading.value = true;
  try {
    report.value = await classesApi.fetchClassReport(classId.value);
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể tải báo cáo lớp.', 'error');
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
    ui.showToast('Đã xuất CSV (UTF-8 BOM — mở được bằng Excel).', 'success');
  } catch {
    ui.showToast('Không thể xuất CSV.', 'error');
  } finally {
    exporting.value = false;
  }
}

const statusBadge: Record<string, { label: string; variant: 'success' | 'warning' | 'muted' }> = {
  completed: { label: 'Hoàn thành', variant: 'success' },
  late: { label: 'Chậm trễ', variant: 'warning' },
  not_started: { label: 'Chưa bắt đầu', variant: 'muted' },
};

function printReport(): void {
  window.print();
}
</script>

<template>
  <main class="class-report container">
    <nav class="class-report__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'classes' }">Lớp học</RouterLink>
      <span aria-hidden="true">/</span>
      <RouterLink :to="{ name: 'class-detail', params: { id: String(classId) } }">Chi tiết lớp</RouterLink>
      <span aria-hidden="true">/</span>
      <span>Báo cáo</span>
    </nav>

    <header class="class-report__header">
      <h1 class="class-report__title">📋 BÁO CÁO LỚP: {{ report?.className ?? '...' }} (ID {{ classId }})</h1>
      <div class="class-report__actions">
        <Button size="sm" variant="secondary" :loading="exporting" @click="exportCsv">Xuất CSV</Button>
        <Button size="sm" variant="ghost" @click="printReport">In</Button>
      </div>
    </header>

    <div v-if="loading" class="class-report__loading">
      <Skeleton v-for="i in 6" :key="i" height="48px" />
    </div>

    <EmptyState
      v-else-if="!report"
      icon="chart"
      title="Không có dữ liệu báo cáo"
      description="Lớp chưa có dữ liệu học tập — hãy gán lộ trình cho lớp trước."
      action-label="Về chi tiết lớp"
      @action="router.push({ name: 'class-detail', params: { id: String(classId) } })"
    />

    <template v-else>
      <div class="class-report__kpis">
        <div class="class-report__kpi card">
          <p class="class-report__kpi-value">{{ formatNumber(report.totalMembers) }}</p>
          <p class="class-report__kpi-label text-muted">Thành viên</p>
        </div>
        <div class="class-report__kpi card">
          <p class="class-report__kpi-value">{{ report.completionPct }}%</p>
          <p class="class-report__kpi-label text-muted">Hoàn thành</p>
        </div>
        <div class="class-report__kpi card">
          <p class="class-report__kpi-value">{{ report.avgScore ?? '—' }}</p>
          <p class="class-report__kpi-label text-muted">Điểm TB</p>
        </div>
        <div class="class-report__kpi card">
          <p class="class-report__kpi-value">{{ formatNumber(report.submissions) }}</p>
          <p class="class-report__kpi-label text-muted">Bài nộp</p>
        </div>
      </div>

      <div class="class-report__table card">
        <table>
          <thead>
            <tr>
              <th>Sinh viên</th>
              <th>Đã xem</th>
              <th>Mô phỏng</th>
              <th>Bài tập</th>
              <th>Điểm cao nhất</th>
              <th>Trạng thái</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in report.rows" :key="row.studentId">
              <td>
                <p class="class-report__name">{{ row.displayName }}</p>
                <p class="text-muted class-report__email">{{ row.email }}</p>
              </td>
              <td>{{ row.viewed ? '✓' : '—' }}</td>
              <td>{{ row.simulationsRun }}</td>
              <td>{{ row.exercisesCompleted }}</td>
              <td>{{ row.bestScore ?? '—' }}</td>
              <td>
                <Badge :variant="statusBadge[row.status]?.variant ?? 'muted'">
                  {{ statusBadge[row.status]?.label ?? row.status }}
                </Badge>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="class-report__summary card">
        <p class="text-muted">
          Tỷ lệ hoàn thành nội dung bắt buộc:
          <ProgressBar :value="report.completionPct" show-label style="margin-top: 8px" />
        </p>
      </div>
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

.class-report__header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.class-report__title { font-size: var(--text-lg); }
.class-report__actions { display: flex; gap: var(--space-sm); }

.class-report__kpis {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
  gap: var(--space-md);
}

.class-report__kpi { display: flex; flex-direction: column; gap: 4px; align-items: flex-start; }

.class-report__kpi-value { font-size: var(--text-2xl); font-weight: 800; color: var(--color-primary); }
.class-report__kpi-label { font-size: var(--text-sm); }

.class-report__table { padding: 0; overflow-x: auto; }

.class-report__table table { width: 100%; border-collapse: collapse; min-width: 640px; }

.class-report__table th {
  text-align: left;
  font-size: var(--text-xs);
  text-transform: uppercase;
  color: var(--color-text-muted);
  padding: var(--space-sm) var(--space-md);
  border-bottom: 2px solid var(--color-border);
  background: var(--color-muted);
}

.class-report__table td { padding: var(--space-sm) var(--space-md); border-bottom: 1px solid var(--color-border); font-size: var(--text-sm); }

.class-report__name { font-weight: 700; }
.class-report__email { font-size: var(--text-xs); }
</style>
