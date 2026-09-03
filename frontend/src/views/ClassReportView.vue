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
import { Check, Download, Printer, Eye, Folder } from 'lucide-vue-next';

import * as classesApi from '@/api/classes';
import * as XLSX from 'xlsx';
import type { ClassMemberDto, ClassReportAssignmentDto, ClassReportDto } from '@/api/types';
import { useUiStore } from '@/stores/ui';
import { formatDate, formatNumber } from '@/utils/format';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import ProgressBar from '@/components/ui/ProgressBar.vue';
import Card from '@/components/ui/Card.vue';
import ClassAssignmentSubmissionsModal from './ClassAssignmentSubmissionsModal.vue';

const route = useRoute();
const router = useRouter();
const ui = useUiStore();

const classId = computed(() => Number(route.params.id));
const report = ref<ClassReportDto | null>(null);
const loading = ref(true);
const exporting = ref(false);
const isSubmissionsModalOpen = ref(false);
const selectedAssignment = ref<ClassReportAssignmentDto | null>(null);

function openAssignmentSubmissions(assign: ClassReportAssignmentDto): void {
  const type = getItemType(assign);
  if (type !== 'quiz' && type !== 'code') return;
  selectedAssignment.value = assign;
  isSubmissionsModalOpen.value = true;
}

const pad = (n: number): string => String(n).padStart(2, '0');

/** Tổng bài nộp (đúng hạn + trễ) vs kỳ vọng (bài gán × thành viên) → % hoàn thành. */
const totals = computed(() => {
  const r = report.value;
  if (!r || r.totalMembers === 0) return { submitted: 0, expected: 0, pct: 0 };
  const submitted = r.assignments.reduce((sum, a) => sum + a.onTime + a.late, 0);
  const expected = r.assignments.length * r.totalMembers;
  return { submitted, expected, pct: expected > 0 ? Math.round((submitted / expected) * 100) : 0 };
});

function getItemType(assign: ClassReportAssignmentDto): 'theory' | 'quiz' | 'code' {
  if (assign.itemType === 'code' || assign.itemType === 'quiz' || assign.itemType === 'theory') {
    return assign.itemType;
  }
  const title = (assign.title || '').toLowerCase();
  if (title.includes('code') || title.includes('lab')) return 'code';
  if (title.includes('quiz') || title.includes('trắc nghiệm') || title.includes('test') || title.includes('kiểm tra')) return 'quiz';
  return 'theory';
}

function getItemTypeMeta(assign: ClassReportAssignmentDto): { label: string; variant: 'primary' | 'muted' | 'warning' } {
  const type = getItemType(assign);
  if (type === 'code') return { label: 'Code Lab', variant: 'warning' };
  if (type === 'quiz') return { label: 'Quiz', variant: 'primary' };
  return { label: 'Lý thuyết', variant: 'muted' };
}

const quizAvgScore = computed(() => {
  const r = report.value;
  if (!r) return '—';
  const quizAssignments = r.assignments.filter((a) => getItemType(a) === 'quiz' && a.avgScore > 0);
  if (quizAssignments.length === 0) return '—';
  const avg = quizAssignments.reduce((sum, a) => sum + a.avgScore, 0) / quizAssignments.length;
  return `${avg.toFixed(1)}/10`;
});

/** KPI phụ (level-1 — KHÔNG icon tròn, không shadow; tối đa 1 hero-stat/màn). */
const secondaryKpis = computed(() => {
  const r = report.value;
  if (!r) return [];
  return [
    { label: messages.classes.reportKpiAssignments, value: formatNumber(r.assignments.length) },
    { label: 'Điểm TB Quiz', value: quizAvgScore.value },
    { label: messages.classes.reportKpiSubmissions, value: formatNumber(totals.value.submitted) },
  ];
});

function assignmentStatus(assign: ClassReportAssignmentDto): { label: string; variant: 'success' | 'warning' | 'muted' } {
  const total = report.value?.totalMembers ?? 0;
  if (total === 0) return { label: 'Chưa có học viên', variant: 'muted' };
  const done = assign.onTime + assign.late;
  if (done >= total && total > 0) return { label: 'Đã hoàn thành', variant: 'success' };
  if (assign.late > 0) return { label: 'Có nộp muộn', variant: 'warning' };
  return { label: 'Đang diễn ra', variant: 'muted' };
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

async function exportXlsx(): Promise<void> {
  exporting.value = true;
  try {
    if (!report.value) {
      ui.showToast('Chưa có dữ liệu báo cáo để xuất.', 'warning');
      return;
    }
    const r = report.value;

    // Lấy thêm danh sách thành viên lớp để bổ sung thông tin chi tiết học viên
    let members: ClassMemberDto[] = [];
    try {
      members = await classesApi.fetchClassMembers(classId.value);
    } catch {
      // Bỏ qua nếu không tải được danh sách thành viên (fallback an toàn)
    }

    const memberEmailByName = new Map<string, string>();
    for (const m of members) {
      if (m.displayName) {
        memberEmailByName.set(m.displayName.trim().toLowerCase(), m.email);
      }
    }

    const laggingMap = new Map<number, number>();
    for (const l of r.laggingLearners) {
      laggingMap.set(l.userId, l.missingCount);
    }

    // Sheet 1 (Mở file ra thấy ngay): Báo cáo tổng hợp — Gồm thông tin lớp ở trên và bảng bài gán chi tiết ở dưới
    const asgHeader = [
      '#',
      'Chương / Module',
      'Tên bài gán',
      'Phân loại',
      'Hạn nộp',
      'Trạng thái bài',
      'Tổng học viên',
      'Đúng hạn',
      'Nộp muộn',
      'Chưa hoàn thành',
      'Tỷ lệ hoàn thành',
      'Kết quả / Đánh giá',
      'Điểm TB'
    ];

    const asgRows = r.assignments.map((a, i) => {
      const st = assignmentStatus(a);
      const typeMeta = getItemTypeMeta(a);
      const total = r.totalMembers;
      const done = a.onTime + a.late;
      const pct = total > 0 ? `${Math.round((done / total) * 100)}%` : '0%';

      let evalText = '—';
      if (typeMeta.label === 'Quiz' || typeMeta.label === 'Code Lab') {
        evalText = a.avgScore > 0 ? `${a.avgScore.toFixed(1)}/10` : 'Chưa có điểm';
      } else {
        evalText = total > 0 ? `Đã học: ${done}/${total}` : 'Chưa có học viên';
      }

      return [
        i + 1,
        a.moduleName || '—',
        a.title,
        typeMeta.label,
        a.dueAt ? new Date(a.dueAt).toLocaleDateString('vi-VN') : 'Không giới hạn',
        st.label,
        total,
        a.onTime,
        a.late,
        a.notSubmitted,
        pct,
        evalText,
        a.avgScore > 0 ? Number(a.avgScore.toFixed(1)) : 0
      ];
    });

    const mainSheetRows = [
      ['BÁO CÁO TIẾN ĐỘ LỚP HỌC: ' + (r.className || '—').toUpperCase()],
      ['Tên lớp học:', r.className || '—', '', 'Mã lớp (ID):', String(classId.value)],
      ['Tổng số học viên:', r.totalMembers, '', 'Tổng số bài gán:', r.assignments.length],
      ['Lượt đã hoàn thành:', `${totals.value.submitted} / ${totals.value.expected} (${totals.value.pct}%)`, '', 'Điểm TB Quiz:', quizAvgScore.value],
      ['Học viên chậm tiến độ:', r.laggingLearners.length, '', 'Thời gian xuất:', new Date().toLocaleString('vi-VN')],
      [],
      ['DANH SÁCH BÀI GÁN VÀ KẾT QUẢ TIẾN ĐỘ CHI TIẾT'],
      asgHeader,
      ...asgRows
    ];

    // Sheet 2: Danh sách học viên chậm tiến độ
    const lagHeader = ['#', 'Mã lớp', 'Tên lớp', 'Tên học viên', 'Email liên hệ', 'Số bài quá hạn chưa nộp', 'Tình trạng'];
    const lagRows = r.laggingLearners.length > 0
      ? r.laggingLearners.map((l, i) => {
          const email = memberEmailByName.get((l.displayName || '').trim().toLowerCase()) || '—';
          return [
            i + 1,
            String(classId.value),
            r.className || '—',
            l.displayName,
            email,
            l.missingCount,
            'Chậm tiến độ (Cần nhắc nhở)'
          ];
        })
      : [
          [
            1,
            String(classId.value),
            r.className || '—',
            r.totalMembers === 0 ? 'Chưa có học viên tham gia lớp' : 'Tất cả học viên đều hoàn thành đúng hạn',
            '—',
            0,
            'Đúng tiến độ'
          ]
        ];

    // Sheet 3: Danh sách học viên trong lớp & Tình trạng tiến độ
    const memHeader = ['#', 'Mã lớp', 'Tên lớp', 'Mã học viên (ID)', 'Họ và tên', 'Email', 'Ngày tham gia', 'Trạng thái tiến độ'];
    const memRows = members.map((m, i) => {
      const missing = laggingMap.get(m.userId) ?? 0;
      const statusStr = missing > 0 ? `Chậm tiến độ (Thiếu ${missing} bài quá hạn)` : 'Đúng hạn / Đang theo kịp';
      return [
        i + 1,
        String(classId.value),
        r.className || '—',
        m.userId,
        m.displayName,
        m.email,
        m.joinedAt ? new Date(m.joinedAt).toLocaleDateString('vi-VN') : '—',
        statusStr
      ];
    });

    const wb = XLSX.utils.book_new();

    // 1. Sheet đầu tiên luôn là Báo cáo lớp (mở file ra là thấy ngay đầy đủ số liệu và danh sách bài học)
    const ws1 = XLSX.utils.aoa_to_sheet(mainSheetRows);
    ws1['!cols'] = [
      { wch: 6 },  // #
      { wch: 24 }, // Chương / Module
      { wch: 34 }, // Tên bài gán
      { wch: 14 }, // Phân loại
      { wch: 16 }, // Hạn nộp
      { wch: 18 }, // Trạng thái bài
      { wch: 14 }, // Tổng học viên
      { wch: 12 }, // Đúng hạn
      { wch: 12 }, // Nộp muộn
      { wch: 16 }, // Chưa hoàn thành
      { wch: 18 }, // Tỷ lệ hoàn thành
      { wch: 22 }, // Kết quả / Đánh giá
      { wch: 12 }, // Điểm TB
    ];
    XLSX.utils.book_append_sheet(wb, ws1, 'Báo cáo lớp');

    // 2. Sheet thứ 2: Chậm tiến độ
    const ws2 = XLSX.utils.aoa_to_sheet([
      ['DANH SÁCH HỌC VIÊN CHẬM TIẾN ĐỘ'],
      ['Lớp:', r.className || '—', 'Mã lớp (ID):', String(classId.value)],
      [],
      lagHeader,
      ...lagRows
    ]);
    ws2['!cols'] = [
      { wch: 5 },  // #
      { wch: 10 }, // Mã lớp
      { wch: 20 }, // Tên lớp
      { wch: 26 }, // Tên học viên
      { wch: 28 }, // Email
      { wch: 24 }, // Số bài quá hạn
      { wch: 28 }, // Tình trạng
    ];
    XLSX.utils.book_append_sheet(wb, ws2, 'Chậm tiến độ');

    // 3. Sheet thứ 3: Danh sách toàn bộ học viên
    if (memRows.length > 0) {
      const ws3 = XLSX.utils.aoa_to_sheet([
        ['DANH SÁCH TOÀN BỘ HỌC VIÊN TRONG LỚP'],
        ['Lớp:', r.className || '—', 'Mã lớp (ID):', String(classId.value)],
        [],
        memHeader,
        ...memRows
      ]);
      ws3['!cols'] = [
        { wch: 5 },  // #
        { wch: 10 }, // Mã lớp
        { wch: 20 }, // Tên lớp
        { wch: 16 }, // Mã học viên
        { wch: 26 }, // Họ và tên
        { wch: 28 }, // Email
        { wch: 16 }, // Ngày tham gia
        { wch: 32 }, // Trạng thái tiến độ
      ];
      XLSX.utils.book_append_sheet(wb, ws3, 'Học viên');
    }

    XLSX.writeFile(wb, `bao-cao-lop-${classId.value}.xlsx`);
    ui.showToast('Đã xuất báo cáo Excel thành công!', 'success');
  } catch (err) {
    ui.showToast('Xuất Excel thất bại.', 'error');
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

    <!-- Banner: surface band level-2 compact (DESIGN §1/#1 — không gradient) -->
    <header class="class-report__hero">
      <div class="class-report__hero-top">
        <div class="class-report__hero-main">
          <h1 class="class-report__title">{{ messages.classes.reportTitle }}</h1>
          <p class="class-report__sub">
            {{ report?.className ?? '…' }} · ID {{ pad(classId) }}
          </p>
        </div>
        <div class="class-report__actions">
          <Button size="md" :loading="exporting" @click="exportXlsx">
            <Download :size="14" aria-hidden="true" /> Xuất Excel
          </Button>
          <Button size="md" variant="secondary" @click="printReport">
            <Printer :size="14" aria-hidden="true" /> {{ messages.classes.reportPrint }}
          </Button>
        </div>
      </div>
    </header>

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
      <!-- KPI: 1 hero-stat (block-token tối) + 3 stat level-1 (quyết định #3) -->
      <div class="class-report__kpis">
        <Card :padded="false" class="class-report__kpi class-report__kpi--hero">
          <div class="class-report__hero-stat" aria-hidden="true">
            <p class="class-report__hero-stat-value">{{ formatNumber(report.totalMembers) }}</p>
            <p class="class-report__hero-stat-index">{{ messages.classes.reportKpiMembers }}</p>
          </div>
        </Card>
        <Card v-for="kpi in secondaryKpis" :key="kpi.label" :padded="false" class="class-report__kpi">
          <div class="flex flex-col gap-y-1.5 p-6 class-report__kpi-head">
            <p class="text-sm text-muted-foreground class-report__kpi-label">{{ kpi.label }}</p>
          </div>
          <div class="p-6 pt-0 class-report__kpi-body">
            <p class="class-report__kpi-value">{{ kpi.value }}</p>
          </div>
        </Card>
      </div>

      <!-- Tỷ lệ hoàn thành -->
      <Card :padded="false" class="class-report__summary">
        <p class="class-report__summary-text">{{ messages.classes.reportSummary }}</p>
        <ProgressBar
          :value="totals.pct"
          show-label
          :variant="totals.pct >= 100 ? 'success' : 'default'"
        />
        <p class="class-report__summary-mono">
          {{ pad(totals.submitted) }} / {{ pad(totals.expected) }} NỘI DUNG CẦN HOÀN THÀNH
        </p>
      </Card>

      <!-- Bảng bài gán (cột số mono — DESIGN §4.6) -->
      <template v-if="report.assignments.length > 0">
        <Card :padded="false" class="class-report__table">
          <div class="class-report__table-scroll">
            <table>
              <thead>
                <tr>
                  <th scope="col">#</th>
                  <th scope="col">{{ messages.classes.reportColContent }}</th>
                  <th scope="col">Phân loại</th>
                  <th scope="col">{{ messages.classes.reportColOnTime }}</th>
                  <th scope="col">{{ messages.classes.reportColLate }}</th>
                  <th scope="col">Chưa hoàn thành</th>
                  <th scope="col">Kết quả / Đánh giá</th>
                  <th scope="col">{{ messages.classes.reportColStatus }}</th>
                  <th scope="col" class="text-center">Chi tiết</th>
                </tr>
              </thead>
              <tbody>
                <tr
                  v-for="(assign, i) in report.assignments"
                  :key="assign.assignmentId"
                  class="transition-colors"
                  :class="getItemType(assign) === 'quiz' || getItemType(assign) === 'code' ? 'hover:bg-muted/70 cursor-pointer' : 'hover:bg-muted/40'"
                  @click="openAssignmentSubmissions(assign)"
                >
                  <td class="class-report__num" data-label="#">#{{ pad(i + 1) }}</td>
                  <td :data-label="messages.classes.reportColContent">
                    <div v-if="assign.moduleName" class="flex items-center gap-1.5 mb-1">
                      <span class="inline-flex items-center gap-1 text-[10px] font-bold text-amber-300 bg-amber-500/10 border border-amber-500/20 px-1.5 py-0.5 rounded">
                        <Folder :size="10" />
                        {{ assign.moduleName }}
                      </span>
                    </div>
                    <p class="class-report__assign-title">{{ assign.title }}</p>
                    <p class="class-report__assign-due">
                      {{ assign.dueAt ? messages.classes.detailDue(formatDate(assign.dueAt)) : messages.classes.detailDueNone }}
                    </p>
                  </td>
                  <td data-label="Phân loại">
                    <Badge :variant="getItemTypeMeta(assign).variant" class="text-[11px] uppercase tracking-wider font-semibold">
                      {{ getItemTypeMeta(assign).label }}
                    </Badge>
                  </td>
                  <td class="class-report__num" :data-label="messages.classes.reportColOnTime">{{ assign.onTime }}</td>
                  <td class="class-report__num" :data-label="messages.classes.reportColLate">{{ assign.late }}</td>
                  <td class="class-report__num" data-label="Chưa hoàn thành">{{ assign.notSubmitted }}</td>
                  <td class="class-report__num text-left" data-label="Kết quả / Đánh giá">
                    <template v-if="getItemType(assign) === 'theory'">
                      <span class="inline-flex items-center gap-1.5 text-xs font-mono text-emerald-400 font-semibold">
                        ✓ Đã học: {{ assign.onTime + assign.late }}/{{ report.totalMembers }}
                      </span>
                    </template>
                    <template v-else-if="getItemType(assign) === 'quiz'">
                      <span class="inline-flex items-center gap-1.5 text-xs font-mono text-cyan-400 font-semibold">
                        {{ assign.avgScore > 0 ? `Điểm TB: ${assign.avgScore.toFixed(1)}/10` : 'Chưa có điểm' }}
                      </span>
                    </template>
                    <template v-else>
                      <span class="inline-flex items-center gap-1.5 text-xs font-mono text-purple-300 font-semibold">
                        Đã nộp: {{ assign.onTime + assign.late }}/{{ report.totalMembers }}
                        <span class="text-[10px] text-muted-foreground font-sans ml-1">(Pass Tests)</span>
                      </span>
                    </template>
                  </td>
                  <td :data-label="messages.classes.reportColStatus">
                    <Badge :variant="assignmentStatus(assign).variant">
                      {{ assignmentStatus(assign).label }}
                    </Badge>
                  </td>
                  <td data-label="Chi tiết" class="text-center" @click.stop>
                    <Button
                      v-if="getItemType(assign) === 'quiz' || getItemType(assign) === 'code'"
                      size="sm"
                      variant="secondary"
                      class="text-xs h-7 px-2.5 gap-1.5 hover:bg-accent hover:text-white"
                      @click="openAssignmentSubmissions(assign)"
                    >
                      <Eye :size="13" />
                      <span>Xem bài nộp</span>
                    </Button>
                    <span v-else class="text-muted-foreground text-xs">—</span>
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
      <Card :padded="false" class="class-report__lagging">
        <h2 class="class-report__lagging-title">{{ messages.classes.reportLaggingTitle }}</h2>
        <div v-if="report.totalMembers === 0" class="class-report__lagging-empty">
          <span class="class-report__lagging-empty-text text-muted-foreground">Chưa có học viên tham gia lớp học</span>
        </div>
        <div v-else-if="report.laggingLearners.length === 0" class="class-report__lagging-empty">
          <Check :size="14" class="text-resolved" aria-hidden="true" />
          <span class="class-report__lagging-empty-text">Không có học viên chậm tiến độ</span>
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

    <!-- Modal chi tiết bài nộp Quiz & Code Lab của lớp -->
    <ClassAssignmentSubmissionsModal
      :is-open="isSubmissionsModalOpen"
      :class-id="classId"
      :assignment="selectedAssignment"
      @close="isSubmissionsModalOpen = false"
    />
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

/* ── Banner: surface band level-2 compact (DESIGN §6) ── */
.class-report__hero {
  border-radius: var(--radius-lg);
  background: var(--card-raised);
  border: 1px solid var(--border-subtle);
  padding: var(--space-lg) var(--space-xl);
}

.class-report__hero-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.class-report__hero-main { display: flex; flex-direction: column; gap: var(--space-xs); min-width: 0; }

.class-report__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  letter-spacing: -0.03em;
  line-height: 1.1;
  margin: 0;
  color: var(--foreground);
}

.class-report__sub {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  color: var(--foreground-tertiary);
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 60ch;
}

.class-report__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

/* ── Loading ── */
.class-report__loading { display: flex; flex-direction: column; gap: var(--space-md); }

/* ── KPI: 1 hero-stat + 3 stat phụ ── */
.class-report__kpis {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: var(--space-md);
}

.class-report__kpi { display: flex; flex-direction: column; }

/* Hero-stat: level-2 + block-token tối (khoảnh khắc đầu tư — enter) */
.class-report__kpi--hero {
  background: var(--card-raised);
  border-color: var(--border-subtle);
  justify-content: center;
  padding: var(--space-md);
}

.class-report__hero-stat {
  background: var(--canvas-ink);
  border: 1px solid rgba(66, 85, 255, 0.3);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  flex: 1;
  justify-content: center;
  opacity: 0;
  transform: translateY(6px);
  animation: report-hero-enter 280ms cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

.class-report__hero-stat-value {
  font-size: var(--text-2xl);
  font-weight: 600;
  letter-spacing: -0.015em;
  line-height: 1.2;
  color: var(--data-core);
  margin: 0;
  font-variant-numeric: tabular-nums;
}

.class-report__hero-stat-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--index-muted);
  margin: 0;
  letter-spacing: 0.08em;
}

@keyframes report-hero-enter {
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .class-report__hero-stat {
    animation: none;
    opacity: 1;
    transform: none;
  }
}

/* Stat phụ: level-1, không icon tròn, không shadow (DESIGN §6 + prompt #8) */
.class-report__kpi-head { display: flex; flex-direction: row; align-items: center; padding: var(--space-md) var(--space-md) 0; }

.class-report__kpi-label { font-size: var(--text-xs); color: var(--foreground-tertiary); }

.class-report__kpi-body { padding: var(--space-xs) var(--space-md) var(--space-md); }

.class-report__kpi-value {
  font-size: var(--text-2xl);
  font-weight: 600;
  letter-spacing: -0.015em;
  line-height: 1.2;
  color: var(--foreground);
  font-variant-numeric: tabular-nums;
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
  .class-report__hero { padding: var(--space-lg); }
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

@media print {
  .class-report__breadcrumb,
  .class-report__actions {
    display: none !important;
  }

  .class-report {
    padding: 0 !important;
    gap: 16px !important;
    background: #ffffff !important;
    color: #000000 !important;
  }

  .class-report__hero,
  .class-report__kpi,
  .class-report__summary,
  .class-report__table,
  .class-report__lagging,
  .class-report__hero-stat {
    background: #ffffff !important;
    color: #000000 !important;
    border: 1px solid #d1d5db !important;
    box-shadow: none !important;
    break-inside: avoid;
    page-break-inside: avoid;
  }

  .class-report__title,
  .class-report__kpi-value,
  .class-report__hero-stat-value,
  .class-report__assign-title,
  .class-report__lagging-title,
  .class-report__lagging-name {
    color: #000000 !important;
  }

  .class-report__sub,
  .class-report__kpi-label,
  .class-report__hero-stat-index,
  .class-report__summary-text,
  .class-report__summary-mono,
  .class-report__assign-due,
  .class-report__lagging-index {
    color: #4b5563 !important;
  }

  .class-report__table table {
    min-width: 100% !important;
    width: 100% !important;
    border-collapse: collapse !important;
  }

  .class-report__table th {
    background: #f3f4f6 !important;
    color: #111827 !important;
    border-bottom: 2px solid #9ca3af !important;
  }

  .class-report__table td {
    border-bottom: 1px solid #e5e7eb !important;
    color: #111827 !important;
  }

  .class-report__lagging-list {
    background: #ffffff !important;
    border: 1px solid #d1d5db !important;
  }

  .class-report__lagging-row {
    border-bottom: 1px solid #f3f4f6 !important;
  }

  .class-report__lagging-row:hover {
    background: transparent !important;
  }
}
</style>
