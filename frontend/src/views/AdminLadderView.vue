<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { Check, Code, FileText, HelpCircle, Info, Link2, ListOrdered, Plus, RefreshCw, Trash2, Unlink, Upload, Pencil } from 'lucide-vue-next';

import * as exercisesApi from '@/api/exercises';
import type { ExerciseSummaryDto } from '@/api/exercises';
import * as lessonsApi from '@/api/lessons';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import AdminNav from '@/components/admin/AdminNav.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import ExerciseBuilderModal from '@/components/admin/ExerciseBuilderModal.vue';

const ui = useUiStore();

const exercises = ref<ExerciseSummaryDto[]>([]);
const lessonsList = ref<Array<{ id: number; title: string }>>([]);
const loading = ref(true);
const loadError = ref(false);

const exerciseTab = ref<'all' | 'quiz' | 'code'>('all');

// Modals state
const exerciseModalOpen = ref(false);
const editingExerciseId = ref<number | null>(null);
const builderDefaultNodeId = ref<number | null>(null);
const builderDefaultStage = ref<number | null>(null);
const builderDefaultTab = ref<'quiz' | 'code' | 'import-csv' | null>(null);

const NODES = Array.from({ length: 8 }, (_, i) => ({
  id: i + 1,
  title: `Node ${i + 1}`,
  stage: ((i % 3) + 1) as 1 | 2 | 3,
}));

const selectedNode = ref<number | null>(1);
const selectedExercise = ref<number | null>(null);

async function load(): Promise<void> {
  loading.value = true;
  loadError.value = false;
  try {
    const [exs, lessonPage] = await Promise.all([
      exercisesApi.fetchExercises({}),
      lessonsApi.fetchLessons({}).catch(() => ({ items: [] })),
    ]);
    exercises.value = exs;
    lessonsList.value = (lessonPage.items || []).map((l) => ({ id: l.id, title: l.title }));
  } catch {
    loadError.value = true;
    exercises.value = [];
  } finally {
    loading.value = false;
  }
}

onMounted(load);

const stageLabel: Record<number, string> = {
  1: messages.admin.ladder.stage[1],
  2: messages.admin.ladder.stage[2],
  3: messages.admin.ladder.stage[3],
};

const filteredExercises = computed(() => {
  if (exerciseTab.value === 'quiz') {
    return exercises.value.filter((e) => e.type === 'MCQ' || e.stage === 1);
  }
  if (exerciseTab.value === 'code') {
    return exercises.value.filter((e) => e.type === 'CODE' || e.stage === 3);
  }
  return exercises.value;
});

const nodeExercises = computed(() => {
  const map = new Map<number, ExerciseSummaryDto[]>();
  for (const node of NODES) map.set(node.id, []);
  for (const ex of exercises.value) {
    if (ex.nodeId !== null && map.has(ex.nodeId)) {
      map.get(ex.nodeId)!.push(ex);
    }
  }
  return map;
});

// Số exercise đã gắn vào từng node
const nodeExerciseCounts = computed(() => {
  const counts = new Map<number, number>();
  for (const node of NODES) counts.set(node.id, 0);
  for (const ex of exercises.value) {
    if (ex.nodeId !== null) counts.set(ex.nodeId, (counts.get(ex.nodeId) ?? 0) + 1);
  }
  return counts;
});

// Số user đã qua từng node
const nodePassedUserCounts = computed(() => {
  const counts = new Map<number, number>();
  for (const node of NODES) counts.set(node.id, 0);
  for (const ex of exercises.value) {
    if (ex.nodeId !== null) counts.set(ex.nodeId, (counts.get(ex.nodeId) ?? 0) + (ex.completedByUserCount ?? 0));
  }
  return counts;
});

function openCreateQuiz(nodeId?: number): void {
  editingExerciseId.value = null;
  builderDefaultNodeId.value = nodeId ?? (selectedNode.value || 1);
  builderDefaultStage.value = 1;
  builderDefaultTab.value = 'quiz';
  exerciseModalOpen.value = true;
}

function openCreateCode(nodeId?: number): void {
  editingExerciseId.value = null;
  builderDefaultNodeId.value = nodeId ?? (selectedNode.value || 1);
  builderDefaultStage.value = 3;
  builderDefaultTab.value = 'code';
  exerciseModalOpen.value = true;
}

function openImportCsv(nodeId?: number): void {
  editingExerciseId.value = null;
  builderDefaultNodeId.value = nodeId ?? (selectedNode.value || 1);
  builderDefaultStage.value = 1;
  builderDefaultTab.value = 'import-csv';
  exerciseModalOpen.value = true;
}

function openEditExercise(ex: ExerciseSummaryDto): void {
  editingExerciseId.value = ex.id;
  builderDefaultNodeId.value = ex.nodeId;
  builderDefaultStage.value = ex.stage || (ex.type === 'CODE' ? 3 : 1);
  builderDefaultTab.value = ex.type === 'CODE' ? 'code' : 'quiz';
  exerciseModalOpen.value = true;
}

async function attach(): Promise<void> {
  if (selectedNode.value === null || selectedExercise.value === null) return;
  try {
    await exercisesApi.updateExercise(selectedExercise.value, { nodeId: selectedNode.value });
    ui.showToast(messages.admin.ladder.attachToast(selectedExercise.value, selectedNode.value), 'success');
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.admin.ladder.attachFailed, 'error');
  }
}

async function detach(ex: ExerciseSummaryDto): Promise<void> {
  try {
    await exercisesApi.updateExercise(ex.id, { nodeId: null });
    ui.showToast(`Đã gỡ bài tập "${ex.title}" khỏi Node!`, 'success');
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể gỡ bài tập.', 'error');
  }
}

async function deleteExerciseItem(ex: ExerciseSummaryDto): Promise<void> {
  if (!confirm(`Bạn có chắc muốn xóa bài tập "${ex.title}"?`)) return;
  try {
    await exercisesApi.deleteExercise(ex.id);
    ui.showToast('Đã xóa bài tập thành công!', 'success');
    void load();
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể xóa bài tập.', 'error');
  }
}
</script>

<template>
  <main class="admin-ladder container">
    <!-- Banner -->
    <header class="admin-ladder__hero">
      <div class="admin-ladder__hero-inner">
        <div class="admin-ladder__hero-main">
          <div class="admin-ladder__hero-badges">
            <Badge variant="primary">
              <ListOrdered :size="12" /> {{ messages.admin.badge }}
            </Badge>
          </div>
          <h1 class="admin-ladder__title">Quản lý Node Ladder & Soạn Bài tập</h1>
          <p class="admin-ladder__sub">Tạo bài Quiz trắc nghiệm, bài tập Code IDE và gán vào các bậc của từng Node trong lộ trình.</p>
        </div>
      </div>
    </header>

    <AdminNav active="ladder" />

    <!-- Action Toolbar for Teacher -->
    <div class="flex flex-wrap items-center justify-between gap-3 p-4 rounded-2xl bg-vdsa-surface border border-vdsa-border">
      <div class="flex items-center gap-2">
        <Button variant="primary" size="md" @click="openCreateQuiz()">
          <HelpCircle :size="16" /> + Tạo Quiz Trắc Nghiệm (Stage 1)
        </Button>
        <Button variant="secondary" size="md" @click="openCreateCode()">
          <Code :size="16" /> + Tạo Bài Tập Code (Stage 3)
        </Button>
        <Button variant="ghost" size="md" @click="openImportCsv()">
          <Upload :size="16" /> 📥 Nhập Quiz từ CSV
        </Button>
      </div>

      <div class="text-xs text-vdsa-muted font-semibold">
        Tổng số: <span class="text-white font-bold">{{ exercises.length }}</span> bài tập trong hệ thống
      </div>
    </div>

    <div v-if="loading" class="admin-ladder__loading" aria-busy="true">
      <Skeleton v-for="i in 6" :key="i" height="56px" />
    </div>

    <div v-else class="grid grid-cols-1 lg:grid-cols-12 gap-6">
      <!-- Cột trái: 8 Nodes Ladder (4 cột trên 12) -->
      <div class="lg:col-span-5 space-y-4">
        <div class="flex items-center justify-between">
          <h2 class="text-sm font-extrabold text-white uppercase tracking-wider">Bản đồ 8 Node Ladder</h2>
          <span class="text-xs text-vdsa-muted">Chọn node để xem bài đã gán</span>
        </div>

        <ul class="space-y-2">
          <li v-for="node in NODES" :key="node.id">
            <button
              type="button"
              class="w-full text-left p-3.5 rounded-xl border transition-all flex items-center justify-between gap-3"
              :class="selectedNode === node.id ? 'bg-vdsa-accent/15 border-vdsa-accent shadow-md ring-1 ring-vdsa-accent/40' : 'bg-vdsa-surface border-vdsa-border hover:bg-vdsa-hover'"
              @click="selectedNode = node.id"
            >
              <div class="flex items-center gap-3">
                <span class="w-7 h-7 rounded-lg bg-vdsa-bg-secondary text-white font-mono font-bold text-xs flex items-center justify-center border border-vdsa-border">
                  #0{{ node.id }}
                </span>
                <div>
                  <div class="text-sm font-bold text-white">Node {{ node.id }}</div>
                  <div class="text-xs text-vdsa-muted">{{ stageLabel[node.stage] }}</div>
                </div>
              </div>

              <div class="flex items-center gap-2">
                <Badge :variant="(nodeExerciseCounts.get(node.id) ?? 0) > 0 ? 'success' : 'secondary'">
                  {{ (nodeExerciseCounts.get(node.id) ?? 0) }} bài tập
                </Badge>
              </div>
            </button>
          </li>
        </ul>
      </div>

      <!-- Cột phải: Chi tiết Node đang chọn & Danh sách bài tập (7 cột trên 12) -->
      <div class="lg:col-span-7 space-y-6">
        <!-- Panel bài tập của Node đang chọn -->
        <div v-if="selectedNode !== null" class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border space-y-4">
          <div class="flex items-center justify-between border-b border-vdsa-border pb-3">
            <div>
              <h3 class="text-base font-extrabold text-white flex items-center gap-2">
                <ListOrdered :size="18" class="text-vdsa-accent" />
                Nội dung của Node {{ selectedNode }} ({{ stageLabel[((selectedNode - 1) % 3) + 1] }})
              </h3>
              <p class="text-xs text-vdsa-muted mt-0.5">Các bài tập học viên sẽ làm khi vượt qua node này</p>
            </div>
            <div class="flex items-center gap-2 flex-wrap">
              <Button size="sm" variant="secondary" @click="openCreateQuiz(selectedNode)">
                <Plus :size="14" /> Thêm Quiz
              </Button>
              <Button size="sm" variant="secondary" @click="openCreateCode(selectedNode)">
                <Plus :size="14" /> Thêm Code
              </Button>
              <Button size="sm" variant="secondary" @click="openImportCsv(selectedNode)">
                <Upload :size="14" /> 📥 Nhập / Mẫu CSV
              </Button>
            </div>
          </div>

          <!-- Danh sách bài tập đã gán vào Node -->
          <div v-if="(nodeExercises.get(selectedNode) ?? []).length === 0" class="py-6 text-center text-xs text-vdsa-muted border border-dashed border-vdsa-border rounded-xl">
            Node {{ selectedNode }} chưa có bài tập nào. Hãy bấm nút tạo bài hoặc gán bài tập từ danh sách bên dưới!
          </div>

          <div v-else class="space-y-2">
            <div
              v-for="ex in nodeExercises.get(selectedNode)"
              :key="ex.id"
              class="p-3 rounded-xl bg-vdsa-bg-secondary border border-vdsa-border flex items-center justify-between gap-3"
            >
              <div class="flex items-center gap-2.5">
                <span class="p-2 rounded-lg bg-vdsa-surface text-vdsa-purple-light border border-vdsa-border">
                  <Code v-if="ex.type === 'CODE'" :size="16" />
                  <HelpCircle v-else :size="16" />
                </span>
                <div>
                  <h4 class="text-xs font-bold text-white">{{ ex.title }}</h4>
                  <div class="text-[11px] text-vdsa-muted flex items-center gap-2 mt-0.5">
                    <span>{{ ex.type }}</span>
                    <span>·</span>
                    <span>Tối đa {{ ex.maxScore }} điểm</span>
                    <span>·</span>
                    <span>{{ ex.durationMinutes > 0 ? `${ex.durationMinutes} phút` : 'Không giới hạn' }}</span>
                  </div>
                </div>
              </div>

              <div class="flex items-center gap-1.5">
                <Button size="sm" variant="ghost" title="Chỉnh sửa" @click="openEditExercise(ex)">
                  <Pencil :size="14" />
                </Button>
                <Button size="sm" variant="ghost" title="Gỡ khỏi Node này" @click="detach(ex)">
                  <Unlink :size="14" /> Gỡ
                </Button>
              </div>
            </div>
          </div>

          <!-- Quick Attach Row -->
          <div class="pt-3 border-t border-vdsa-border flex flex-col sm:flex-row items-center gap-2.5">
            <div class="flex-1 w-full">
              <select
                v-model="selectedExercise"
                class="w-full bg-vdsa-bg border border-vdsa-border rounded-lg px-3 py-2 text-xs text-white focus:outline-none focus:border-accent"
              >
                <option :value="null">-- Chọn bài tập có sẵn để gán vào Node {{ selectedNode }} --</option>
                <option v-for="ex in exercises" :key="ex.id" :value="ex.id">
                  #{{ ex.id }} - {{ ex.title }} ({{ ex.type }}) {{ ex.nodeId ? `[Đang ở Node ${ex.nodeId}]` : '[Chưa gán]' }}
                </option>
              </select>
            </div>
            <Button size="sm" variant="primary" :disabled="selectedExercise === null" @click="attach">
              <Link2 :size="14" /> Gán vào Node {{ selectedNode }}
            </Button>
          </div>
        </div>

        <!-- Bảng Tất Cả Bài Tập Trong Hệ Thống -->
        <div class="p-5 rounded-2xl bg-vdsa-surface border border-vdsa-border space-y-4">
          <div class="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3 border-b border-vdsa-border pb-3">
            <h3 class="text-sm font-extrabold text-white uppercase tracking-wider">
              Tất cả bài tập trong hệ thống ({{ filteredExercises.length }})
            </h3>

            <!-- Sub Filter -->
            <div class="flex bg-vdsa-bg-secondary p-1 rounded-lg border border-vdsa-border text-xs">
              <button
                type="button"
                class="px-3 py-1 rounded font-semibold transition-colors"
                :class="exerciseTab === 'all' ? 'bg-vdsa-accent text-white' : 'text-vdsa-muted hover:text-white'"
                @click="exerciseTab = 'all'"
              >
                Tất cả
              </button>
              <button
                type="button"
                class="px-3 py-1 rounded font-semibold transition-colors"
                :class="exerciseTab === 'quiz' ? 'bg-vdsa-accent text-white' : 'text-vdsa-muted hover:text-white'"
                @click="exerciseTab = 'quiz'"
              >
                Quiz
              </button>
              <button
                type="button"
                class="px-3 py-1 rounded font-semibold transition-colors"
                :class="exerciseTab === 'code' ? 'bg-vdsa-accent text-white' : 'text-vdsa-muted hover:text-white'"
                @click="exerciseTab = 'code'"
              >
                Code Lab
              </button>
            </div>
          </div>

          <div class="space-y-2.5 max-h-[420px] overflow-y-auto pr-1 custom-scrollbar">
            <div
              v-for="ex in filteredExercises"
              :key="ex.id"
              class="p-3.5 rounded-xl bg-vdsa-bg border border-vdsa-border hover:border-vdsa-accent/40 transition-colors flex flex-col sm:flex-row sm:items-center justify-between gap-3"
            >
              <div class="flex items-start gap-3">
                <span class="p-2 rounded-lg bg-vdsa-surface text-vdsa-purple-light border border-vdsa-border shrink-0 mt-0.5">
                  <Code v-if="ex.type === 'CODE'" :size="16" />
                  <HelpCircle v-else :size="16" />
                </span>
                <div>
                  <h4 class="text-xs font-bold text-white">{{ ex.title }}</h4>
                  <div class="text-[11px] text-vdsa-muted flex items-center gap-2 mt-1">
                    <Badge variant="secondary" size="sm">{{ ex.type }}</Badge>
                    <Badge v-if="ex.nodeId" variant="primary" size="sm">Gán ở Node {{ ex.nodeId }}</Badge>
                    <Badge v-else variant="muted" size="sm">Chưa gán Node</Badge>
                    <span>·</span>
                    <span>{{ ex.maxScore }} điểm</span>
                  </div>
                </div>
              </div>

              <div class="flex items-center gap-2 self-end sm:self-center">
                <Button size="sm" variant="ghost" @click="openEditExercise(ex)">
                  <Pencil :size="13" /> Sửa
                </Button>
                <Button size="sm" variant="danger" @click="deleteExerciseItem(ex)">
                  <Trash2 :size="13" /> Xóa
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Modal Soạn Bài Tập (Quiz / Code / CSV) -->
    <ExerciseBuilderModal
      :open="exerciseModalOpen"
      :exercise-id="editingExerciseId"
      :default-node-id="builderDefaultNodeId"
      :default-stage="builderDefaultStage"
      :default-tab="builderDefaultTab"
      :lessons="lessonsList"
      @close="exerciseModalOpen = false"
      @saved="load"
    />
  </main>
</template>

<style scoped>
.admin-ladder {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 1200px;
}

.admin-ladder__hero {
  border-bottom: 1px solid var(--border-subtle);
  background: var(--card-raised);
  border-radius: var(--radius-lg);
  padding: var(--space-xl);
}

.admin-ladder__hero-inner {
  display: flex;
  align-items: flex-end;
  gap: var(--space-lg);
  flex-wrap: wrap;
}

.admin-ladder__hero-main {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.admin-ladder__hero-badges { display: flex; gap: var(--space-sm); }
.admin-ladder__title { font-size: var(--text-4xl); font-weight: 600; margin: 0; color: var(--foreground); }
.admin-ladder__sub { color: var(--foreground-secondary); font-size: var(--text-sm); margin: 0; }
</style>
