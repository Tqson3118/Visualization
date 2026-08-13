<script setup lang="ts">
// AdminLadderView — Màn N-6: soạn node (gắn quiz/lab/code) — dạng cơ bản:
// danh sách node + chọn exercise gắn vào. H-B: hero Aurora soft + info banner
// + node list có gradient id/selected state + attach panel giữ id select cũ.
import { computed, onMounted, ref } from 'vue';
import { Check, Info, Link2, ListOrdered } from 'lucide-vue-next';

import * as exercisesApi from '@/api/exercises';
import type { ExerciseSummaryDto } from '@/api/exercises';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import AdminNav from '@/components/admin/AdminNav.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const ui = useUiStore();

const exercises = ref<ExerciseSummaryDto[]>([]);
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
  const map = new Map<number, ExerciseSummaryDto | null>();
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
    <!-- Hero gradient Aurora soft -->
    <header class="admin-ladder__hero">
      <div class="admin-ladder__hero-body">
        <span class="admin-ladder__hero-icon" aria-hidden="true"><ListOrdered :size="24" /></span>
        <div class="admin-ladder__hero-title-wrap">
          <h1 class="admin-ladder__title">{{ messages.admin.ladder.title }}</h1>
          <p class="admin-ladder__sub">{{ messages.admin.ladder.subtitle }}</p>
        </div>
        <Badge variant="primary" class="admin-ladder__hero-badge">{{ messages.admin.badge }}</Badge>
      </div>
    </header>

    <AdminNav active="ladder" />

    <div class="admin-ladder__note card">
      <Info :size="16" class="admin-ladder__note-icon" aria-hidden="true" />
      <p class="admin-ladder__note-text">{{ messages.admin.ladder.note }}</p>
    </div>

    <div v-if="loading" class="admin-ladder__loading" aria-busy="true">
      <Skeleton v-for="i in 6" :key="i" height="56px" />
    </div>

    <div v-else class="admin-ladder__grid">
      <!-- Danh sách node -->
      <div class="admin-ladder__nodes card">
        <h2 class="admin-ladder__subtitle">{{ messages.admin.ladder.nodeList }}</h2>
        <ul class="admin-ladder__node-list">
          <li v-for="node in NODES" :key="node.id">
            <button
              type="button"
              class="admin-ladder__node hover-lift"
              :class="{ 'admin-ladder__node--selected': selectedNode === node.id }"
              :aria-pressed="selectedNode === node.id"
              @click="selectedNode = node.id"
            >
              <span class="admin-ladder__node-id" aria-hidden="true">{{ node.id }}</span>
              <span class="admin-ladder__node-stage">{{ stageLabel[node.stage] }}</span>
              <Badge v-if="nodeExercises.get(node.id)" variant="success" class="admin-ladder__node-badge">
                <Check :size="11" /> {{ messages.admin.ladder.attached }}
              </Badge>
              <Badge v-else variant="muted" class="admin-ladder__node-badge">
                {{ messages.admin.ladder.empty }}
              </Badge>
            </button>
          </li>
        </ul>
      </div>

      <!-- Gắn bài tập -->
      <div class="admin-ladder__attach card">
        <h2 class="admin-ladder__subtitle">
          <Link2 :size="15" class="admin-ladder__subtitle-icon" aria-hidden="true" />
          {{ messages.admin.ladder.attachTitle }}
        </h2>

        <EmptyState
          v-if="exercises.length === 0"
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
              <Link2 :size="14" /> {{ messages.admin.ladder.attachBtn }}
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

/* ── Hero gradient Aurora soft ── */
.admin-ladder__hero {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 32%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-aurora);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
}

.admin-ladder__hero::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: color-mix(in srgb, var(--color-background) 58%, transparent);
}

.admin-ladder__hero::before {
  content: '';
  position: absolute;
  width: 260px;
  height: 260px;
  border-radius: 50%;
  top: -120px;
  right: -60px;
  z-index: -1;
  background: color-mix(in srgb, var(--color-secondary) 30%, transparent);
  filter: blur(64px);
}

.admin-ladder__hero-body { display: flex; align-items: center; gap: var(--space-md); flex-wrap: wrap; }

.admin-ladder__hero-icon {
  width: 48px;
  height: 48px;
  border-radius: var(--radius-lg);
  background-image: var(--gradient-aurora);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: var(--shadow-md);
}

.admin-ladder__hero-title-wrap { display: flex; flex-direction: column; gap: 4px; }

.admin-ladder__title {
  font-size: var(--text-2xl);
  background-image: var(--gradient-aurora);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.admin-ladder__sub { font-size: var(--text-sm); color: var(--color-text-muted); max-width: 60ch; }

.admin-ladder__hero-badge { margin-left: auto; }

/* ── Info banner ── */
.admin-ladder__note { display: flex; align-items: flex-start; gap: var(--space-sm); padding: var(--space-md); }

.admin-ladder__note-icon { flex-shrink: 0; margin-top: 2px; color: var(--color-info); }

.admin-ladder__note-text { font-size: var(--text-sm); color: var(--color-text-muted); }

.admin-ladder__loading { display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-ladder__grid { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-md); align-items: start; }

.admin-ladder__subtitle { display: flex; align-items: center; gap: var(--space-sm); font-size: var(--text-md); margin-bottom: var(--space-md); }

.admin-ladder__subtitle-icon { color: var(--color-primary); }

/* ── Node list ── */
.admin-ladder__node-list { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: var(--space-sm); }

.admin-ladder__node {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  width: 100%;
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  font-size: var(--text-sm);
  font-weight: 600;
  cursor: pointer;
  text-align: left;
  color: var(--color-foreground);
}

.admin-ladder__node--selected {
  border-color: var(--color-primary);
  background: color-mix(in srgb, var(--color-primary) 7%, var(--color-surface));
  box-shadow: 0 0 0 2px color-mix(in srgb, var(--color-primary) 18%, transparent);
}

.admin-ladder__node-id {
  width: 26px;
  height: 26px;
  border-radius: 50%;
  background-image: var(--gradient-aurora);
  color: var(--color-on-primary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: var(--text-xs);
  font-weight: 800;
  flex-shrink: 0;
}

.admin-ladder__node-stage { min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

.admin-ladder__node-badge { margin-left: auto; flex-shrink: 0; }

/* ── Attach panel ── */
.admin-ladder__attach { display: flex; flex-direction: column; gap: var(--space-md); }

.admin-ladder__field { display: flex; flex-direction: column; gap: var(--space-xs); }

.admin-ladder__actions { display: flex; justify-content: flex-end; }

@media (max-width: 800px) {
  .admin-ladder__grid { grid-template-columns: 1fr; }
}

@media (max-width: 640px) {
  .admin-ladder__hero-badge { margin-left: 0; }
}
</style>
