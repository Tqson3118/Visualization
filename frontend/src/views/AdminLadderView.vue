<script setup lang="ts">
// AdminLadderView — Màn N-6: soạn node (gắn quiz/lab/code) — dạng cơ bản:
// danh sách node + chọn exercise gắn vào. Soạn quiz nâng cao (ngân hàng câu hỏi/CSV) ở GĐ sau.
import { computed, onMounted, ref } from 'vue';

import * as exercisesApi from '@/api/exercises';
import type { ExerciseDto } from '@/api/exercises';
import { useUiStore } from '@/stores/ui';
import AdminNav from '@/components/admin/AdminNav.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const ui = useUiStore();

const exercises = ref<ExerciseDto[]>([]);
const loading = ref(true);

const NODES = Array.from({ length: 8 }, (_, i) => ({
  id: i + 1,
  title: `Node ${i + 1}`,
  stage: ((i % 3) + 1) as 1 | 2 | 3,
}));

const selectedNode = ref<number | null>(null);
const selectedExercise = ref<number | null>(null);

onMounted(async () => {
  try {
    exercises.value = await exercisesApi.fetchExercises({});
  } catch {
    ui.showToast('Không thể tải danh sách bài tập (backend chưa khả dụng).', 'error');
    exercises.value = [];
  } finally {
    loading.value = false;
  }
});

const stageLabel: Record<number, string> = { 1: 'Quiz (Bậc 1)', 2: 'Lab (Bậc 2)', 3: 'Code (Bậc 3)' };

const nodeExercises = computed(() => {
  const map = new Map<number, ExerciseDto | null>();
  for (const node of NODES) {
    const ex = exercises.value.find((e) => e.nodeId === node.id);
    map.set(node.id, ex ?? null);
  }
  return map;
});

async function attach(): Promise<void> {
  if (selectedNode.value === null || selectedExercise.value === null) return;
  try {
    // Backend: cập nhật nodeId của exercise (PUT /exercises/{id})
    await exercisesApi.updateExercise(selectedExercise.value, { nodeId: selectedNode.value });
    ui.showToast(`Đã gắn exercise #${selectedExercise.value} vào Node ${selectedNode.value}.`, 'success');
    // Reload để map cập nhật
    exercises.value = await exercisesApi.fetchExercises({});
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Gắn thất bại.', 'error');
  }
}
</script>

<template>
  <main class="admin-ladder container">
    <h1 class="admin-ladder__title">🪜 Soạn Ladder (node)</h1>

    <AdminNav active="ladder" />

    <p class="admin-ladder__note text-muted">
      Dạng cơ bản: danh sách node + chọn bài tập gắn. Soạn quiz nâng cao (ngân hàng câu hỏi,
      import CSV, test ẩn code) sẽ triển khai ở giai đoạn sau.
    </p>

    <div v-if="loading" class="admin-ladder__loading">
      <Skeleton v-for="i in 6" :key="i" height="44px" />
    </div>

    <div class="admin-ladder__grid">
      <div class="admin-ladder__nodes card">
        <h2 class="admin-ladder__subtitle">Danh sách node</h2>
        <button
          v-for="node in NODES"
          :key="node.id"
          type="button"
          class="admin-ladder__node"
          :class="{ 'admin-ladder__node--selected': selectedNode === node.id }"
          @click="selectedNode = node.id"
        >
          <span class="admin-ladder__node-id">{{ node.id }}</span>
          {{ stageLabel[node.stage] }}
          <Badge v-if="nodeExercises.get(node.id)" variant="success">đã gắn</Badge>
          <Badge v-else variant="muted">trống</Badge>
        </button>
      </div>

      <div class="admin-ladder__attach card">
        <h2 class="admin-ladder__subtitle">Gắn bài tập vào node</h2>

        <EmptyState
          v-if="exercises.length === 0"
          icon="puzzle"
          title="Chưa có bài tập"
          description="Tạo bài tập ở backend/admin API (POST /exercises) trước."
        />

        <template v-else>
          <label class="label" for="node-select">Node</label>
          <select id="node-select" v-model="selectedNode" class="input">
            <option :value="null" disabled>Chọn node...</option>
            <option v-for="node in NODES" :key="node.id" :value="node.id">
              Node {{ node.id }} — {{ stageLabel[node.stage] }}
            </option>
          </select>

          <label class="label mt-md" for="exercise-select">Bài tập</label>
          <select id="exercise-select" v-model="selectedExercise" class="input">
            <option :value="null" disabled>Chọn bài tập...</option>
            <option v-for="ex in exercises" :key="ex.id" :value="ex.id">
              #{{ ex.id }} — {{ ex.title }} ({{ ex.type }}, stage {{ ex.stage }})
            </option>
          </select>

          <div class="admin-ladder__actions">
            <Button :disabled="selectedNode === null || selectedExercise === null" @click="attach">
              Gắn exercise vào node
            </Button>
          </div>
        </template>
      </div>
    </div>
  </main>
</template>

<style scoped>
.admin-ladder {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  max-width: 960px;
}

.admin-ladder__title { font-size: var(--text-2xl); }
.admin-ladder__note { font-size: var(--text-sm); }

.admin-ladder__grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-md); }

.admin-ladder__subtitle { font-size: var(--text-md); margin-bottom: var(--space-sm); }

.admin-ladder__nodes { display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-ladder__node {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  font-size: var(--text-sm);
  font-weight: 600;
  cursor: pointer;
  text-align: left;
}

.admin-ladder__node--selected { border-color: var(--color-primary); background: var(--color-surface-hover); }

.admin-ladder__node-id {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: var(--color-primary);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: var(--text-xs);
  flex-shrink: 0;
}

.admin-ladder__attach { display: flex; flex-direction: column; gap: var(--space-sm); align-self: start; }

.admin-ladder__actions { margin-top: var(--space-md); display: flex; justify-content: flex-end; }

@media (max-width: 800px) {
  .admin-ladder__grid { grid-template-columns: 1fr; }
}
</style>
