<script setup lang="ts">
// AdminLadderView — Màn N-6: soạn node (gắn quiz/lab/code) — dạng cơ bản:
// danh sách node + chọn exercise gắn vào.
// View-quality 14/08 (Nhóm D): banner surface band level-2; node row qua
// Button.vue (grep `<button` raw = 0); node-id = block-token tối index mono
// (dữ liệu tuần tự — quyết định #4); bỏ hover-lift/gradient; error state + retry.
import { computed, onMounted, ref } from 'vue';
import { Check, Info, Link2, ListOrdered, RefreshCw } from 'lucide-vue-next';

import * as exercisesApi from '@/api/exercises';
import type { ExerciseSummaryDto } from '@/api/exercises';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import AdminNav from '@/components/admin/AdminNav.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import PageHero from '@/components/ui/PageHero.vue';

const ui = useUiStore();

const exercises = ref<ExerciseSummaryDto[]>([]);
const loading = ref(true);
const loadError = ref(false);

const NODES = Array.from({ length: 8 }, (_, i) => ({
  id: i + 1,
  title: `Node ${i + 1}`,
  stage: ((i % 3) + 1) as 1 | 2 | 3,
}));

const selectedNode = ref<number | null>(null);
const selectedExercise = ref<number | null>(null);

async function load(): Promise<void> {
  loading.value = true;
  loadError.value = false;
  try {
    exercises.value = await exercisesApi.fetchExercises({});
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

const nodeExercises = computed(() => {
  const map = new Map<number, ExerciseSummaryDto | null>();
  for (const node of NODES) {
    const ex = exercises.value.find((e) => e.nodeId === node.id);
    map.set(node.id, ex ?? null);
  }
  return map;
});

// Số exercise đã gắn vào từng node (đếm từ list exercises đã fetch — không gọi API mới).
const nodeExerciseCounts = computed(() => {
  const counts = new Map<number, number>();
  for (const node of NODES) counts.set(node.id, 0);
  for (const ex of exercises.value) {
    if (ex.nodeId !== null) counts.set(ex.nodeId, (counts.get(ex.nodeId) ?? 0) + 1);
  }
  return counts;
});

// Số user đã qua từng node (tổng completedByUserCount của exercise gắn node — field
// optional, backend deploy song song; chưa có field thì badge hiển thị 0).
const nodePassedUserCounts = computed(() => {
  const counts = new Map<number, number>();
  for (const node of NODES) counts.set(node.id, 0);
  for (const ex of exercises.value) {
    if (ex.nodeId !== null) counts.set(ex.nodeId, (counts.get(ex.nodeId) ?? 0) + (ex.completedByUserCount ?? 0));
  }
  return counts;
});

async function attach(): Promise<void> {
  if (selectedNode.value === null || selectedExercise.value === null) return;
  try {
    // Backend: cập nhật nodeId của exercise (PUT /exercises/{id})
    await exercisesApi.updateExercise(selectedExercise.value, { nodeId: selectedNode.value });
    ui.showToast(messages.admin.ladder.attachToast(selectedExercise.value, selectedNode.value), 'success');
    // Reload để map cập nhật
    exercises.value = await exercisesApi.fetchExercises({});
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : messages.admin.ladder.attachFailed, 'error');
  }
}
</script>

<template>
  <main class="admin-ladder container">
    <!-- Banner: surface band level-2 (PageHero — DESIGN §1/#1: KHÔNG gradient, KHÔNG shadow) -->
    <PageHero :title="messages.admin.ladder.title" :description="messages.admin.ladder.subtitle">
      <template #badges>
        <Badge variant="primary">
          <ListOrdered :size="12" /> {{ messages.admin.badge }}
        </Badge>
      </template>
    </PageHero>

    <AdminNav active="ladder" />

    <div class="admin-ladder__note">
      <Info :size="16" class="admin-ladder__note-icon" aria-hidden="true" />
      <p class="admin-ladder__note-text">{{ messages.admin.ladder.note }}</p>
    </div>

    <div v-if="loading" class="admin-ladder__loading" aria-busy="true">
      <Skeleton v-for="i in 6" :key="i" height="56px" />
    </div>

    <div v-else class="admin-ladder__grid">
      <!-- Danh sách node -->
      <div class="admin-ladder__nodes">
        <h2 class="admin-ladder__subtitle">{{ messages.admin.ladder.nodeList }}</h2>
        <ul class="admin-ladder__node-list">
          <li v-for="node in NODES" :key="node.id">
            <Button
              variant="secondary"
              class="admin-ladder__node"
              :class="{ 'admin-ladder__node--selected': selectedNode === node.id }"
              :aria-pressed="selectedNode === node.id"
              @click="selectedNode = node.id"
            >
              <span class="admin-ladder__node-id" aria-hidden="true">{{ node.id }}</span>
              <span class="admin-ladder__node-stage">{{ stageLabel[node.stage] }}</span>
              <Badge variant="secondary" class="admin-ladder__node-count">
                {{ messages.admin.ladder.exercisesCount(nodeExerciseCounts.get(node.id) ?? 0) }}
              </Badge>
              <Badge variant="secondary" class="admin-ladder__node-passed">
                {{ messages.admin.ladder.passedUsersCount(nodePassedUserCounts.get(node.id) ?? 0) }}
              </Badge>
              <Badge v-if="nodeExercises.get(node.id)" variant="success" class="admin-ladder__node-badge">
                <Check :size="12" /> {{ messages.admin.ladder.attached }}
              </Badge>
              <Badge v-else variant="muted" class="admin-ladder__node-badge">
                {{ messages.admin.ladder.empty }}
              </Badge>
            </Button>
          </li>
        </ul>
      </div>

      <!-- Gắn bài tập -->
      <div class="admin-ladder__attach">
        <h2 class="admin-ladder__subtitle">
          <Link2 :size="16" class="admin-ladder__subtitle-icon" aria-hidden="true" />
          {{ messages.admin.ladder.attachTitle }}
        </h2>

        <div v-if="loadError" class="admin-ladder__error" role="alert">
          <p class="admin-ladder__error-text">{{ messages.admin.ladder.loadErrorText }}</p>
          <Button size="sm" variant="secondary" @click="load">
            <RefreshCw :size="14" /> {{ messages.admin.ladder.retry }}
          </Button>
        </div>

        <EmptyState
          v-else-if="exercises.length === 0"
          icon="puzzle"
          :title="messages.admin.ladder.emptyTitle"
          :description="messages.admin.ladder.emptyDesc"
        />

        <template v-else>
          <div class="admin-ladder__field">
            <label class="label" for="node-select">{{ messages.admin.ladder.nodeLabel }}</label>
            <select id="node-select" v-model="selectedNode" class="input">
              <option :value="null" disabled>{{ messages.admin.ladder.nodePlaceholder }}</option>
              <option v-for="node in NODES" :key="node.id" :value="node.id">
                Node {{ node.id }} — {{ stageLabel[node.stage] }}
              </option>
            </select>
          </div>

          <div class="admin-ladder__field">
            <label class="label" for="exercise-select">{{ messages.admin.ladder.exerciseLabel }}</label>
            <select id="exercise-select" v-model="selectedExercise" class="input">
              <option :value="null" disabled>{{ messages.admin.ladder.exercisePlaceholder }}</option>
              <option v-for="ex in exercises" :key="ex.id" :value="ex.id">
                #{{ ex.id }} — {{ ex.title }} ({{ ex.type }}, stage {{ ex.stage }})
              </option>
            </select>
          </div>

          <div class="admin-ladder__actions">
            <Button :disabled="selectedNode === null || selectedExercise === null" @click="attach">
              <Link2 :size="16" /> {{ messages.admin.ladder.attachBtn }}
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
  max-width: 1000px;
}

/* ── Info banner ── */
.admin-ladder__note {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  padding: var(--space-md);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
}

.admin-ladder__note-icon { flex-shrink: 0; margin-top: 2px; color: var(--info); }

.admin-ladder__note-text { margin: 0; font-size: var(--text-sm); color: var(--foreground-secondary); }

.admin-ladder__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-ladder__grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-md); align-items: start; }

/* ── Panels ── */
.admin-ladder__nodes,
.admin-ladder__attach {
  padding: var(--space-md);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
}

.admin-ladder__attach { display: flex; flex-direction: column; gap: var(--space-md); }

.admin-ladder__subtitle {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  margin: 0 0 var(--space-md);
  font-size: var(--text-lg);
  font-weight: 600;
  letter-spacing: -0.015em;
  line-height: 1.25;
}

.admin-ladder__subtitle-icon { color: var(--foreground-secondary); }

/* ── Error ── */
.admin-ladder__error {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
  flex-wrap: wrap;
  padding: var(--space-sm) var(--space-md);
  border: 1px solid color-mix(in srgb, var(--destructive) 35%, transparent);
  background: color-mix(in srgb, var(--destructive) 8%, transparent);
  border-radius: var(--radius-md);
}

.admin-ladder__error-text { margin: 0; font-size: var(--text-sm); color: var(--destructive); }

/* ── Node list ── */
.admin-ladder__node-list { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: var(--space-sm); }

/* Node row = Button.vue (outline md) — chỉ override layout, không đè padding */
.admin-ladder__node {
  width: 100%;
  justify-content: flex-start;
  border-color: var(--border);
  color: var(--foreground);
  transition: border-color 150ms, background-color 150ms;
}

.admin-ladder__node:hover { border-color: var(--border-strong); }

.admin-ladder__node--selected {
  border-color: var(--primary);
  background: color-mix(in srgb, var(--primary) 7%, var(--card));
  box-shadow: 0 0 0 1px var(--primary);
}

.admin-ladder__node--selected:hover { border-color: var(--primary); }

/* Node-id = block-token tối + index mono (dữ liệu tuần tự — quyết định #4) */
.admin-ladder__node-id {
  min-width: 28px;
  height: 28px;
  padding: 0 var(--space-xs);
  border-radius: var(--radius-sm);
  border: 1px solid rgba(66, 85, 255, 0.3);
  background: var(--canvas-ink);
  color: var(--data-core);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  flex-shrink: 0;
}

.admin-ladder__node-stage { min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

/* Count badges nằm cạnh stage; badge trạng thái (đã gắn/trống) giữ margin-left:auto đẩy phải */
.admin-ladder__node-count,
.admin-ladder__node-passed { flex-shrink: 0; }

.admin-ladder__node-badge { margin-left: auto; flex-shrink: 0; }

/* ── Attach panel ── */
.admin-ladder__field { display: flex; flex-direction: column; gap: var(--space-xs); }

/* Select chưa có wrapper shadcn — giữ .input nhưng token + easing chuẩn */
.admin-ladder__field .input {
  background: var(--card);
  border-color: var(--border);
  color: var(--foreground);
  font-size: var(--text-sm);
  transition: border-color 150ms;
}

.admin-ladder__actions { display: flex; justify-content: flex-end; }

@media (max-width: 800px) {
  .admin-ladder__grid { grid-template-columns: 1fr; }
}
</style>
