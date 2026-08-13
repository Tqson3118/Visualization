<script setup lang="ts">
// LadderView — Màn 14: Practice Ladder shell (stepper 3 bậc + stages tách component)
// View-quality (Phase 2 bổ sung): 🪜 glyph vỡ (r2-fixed-07) → lucide ListOrdered; banner
// gradient sunset + blob + shadow → surface band level-2 + kicker mono + strip block-token
// tối (trọng số 3 bậc — quyết định xuyên-nhóm #1/#4); H1 48px/600/-0.03em; badge muted;
// ←/→ ký tự → lucide ArrowLeft/ArrowRight; Motion enter 280ms cubic-bezier(0.16,1,0.3,1).
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArrowLeft, ArrowRight, ListOrdered } from 'lucide-vue-next';
import { Motion } from 'motion-v';

import { useLessonStore } from '@/stores/lesson';
import * as exercisesApi from '@/api/exercises';
import type { ExerciseDto } from '@/api/exercises';
import { getCatalogMeta } from '@/engines/catalog';
import { messages } from '@/i18n/vi';
import LadderShell from '@/components/ladder/LadderShell.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';

const route = useRoute();
const router = useRouter();
const lessonStore = useLessonStore();

const nodeId = computed(() => String(route.params.nodeId ?? ''));
const topicId = ref<number>(1);

const simKey = computed(() => {
  // Suy key mô phỏng từ nodeId (fallback cục bộ)
  const keys = ['sort.bubble', 'search.binary', 'graph.bfs', 'tree.bst-insert', 'stack.push', 'queue.enqueue', 'sort.merge', 'sort.quick'];
  const key = keys[(Number(nodeId.value) - 1) % keys.length];
  return getCatalogMeta(key) ? key : 'sort.bubble';
});

const nodeTitle = computed(() => getCatalogMeta(simKey.value)?.title ?? `Node ${nodeId.value}`);

/** Strip trọng số 3 bậc — dữ liệu thật (decorative, aria-hidden). */
const STAGE_WEIGHTS = [
  { stage: 'Quiz', weight: '20%', index: '01' },
  { stage: 'Lab', weight: '30%', index: '02' },
  { stage: 'Code', weight: '50%', index: '03' },
] as const;

/** Exercise Ladder theo node: quiz (stage 1) + code (stage 3) — GET /exercises?nodeId&stage (SETUP_TODO §6.6) */
const quizExercise = ref<ExerciseDto | null>(null);
const quizLoading = ref(true);
const codeExerciseId = ref<number | null>(null);

async function loadLadderExercises(): Promise<void> {
  const node = Number(nodeId.value);
  try {
    const [quizList, codeList] = await Promise.all([
      exercisesApi.fetchExercises({ nodeId: node, stage: 1 }),
      exercisesApi.fetchExercises({ nodeId: node, stage: 3 }),
    ]);
    if (quizList.length > 0) {
      quizExercise.value = await exercisesApi.fetchExercise(quizList[0].id);
    }
    codeExerciseId.value = codeList[0]?.id ?? null;
  } catch {
    // API lỗi → giữ null; LadderShell hiện EmptyState thay vì crash
    quizExercise.value = null;
    codeExerciseId.value = null;
  } finally {
    quizLoading.value = false;
  }
}

onMounted(async () => {
  try {
    await lessonStore.fetchTopics();
    const first = lessonStore.topics[0];
    if (first) topicId.value = first.id;
  } catch {
    topicId.value = 1;
  }
  await loadLadderExercises();
});

function onPassed(stage: number): void {
  // stage 1,2,3 — LadderShell tự lưu; chuyển Lab khi pass Quiz nếu cần
  void stage;
}
</script>

<template>
  <main class="ladder container">
    <!-- Chrome header — surface band level-2 + kicker mono + strip block-token (DESIGN.md §1/§6) -->
    <Motion
      as="header"
      class="ladder__chrome"
      :initial="{ opacity: 0, y: 12 }"
      :animate="{ opacity: 1, y: 0 }"
      :transition="{ duration: 0.28, ease: [0.16, 1, 0.3, 1] }"
    >
      <nav class="ladder__breadcrumb" aria-label="Breadcrumb">
        <RouterLink :to="{ name: 'path-topic', params: { topicId: String(topicId) } }">
          {{ messages.practiceLadder.breadcrumbPath }}
        </RouterLink>
        <span aria-hidden="true">/</span>
        <span aria-current="page">{{ nodeTitle }}</span>
      </nav>

      <p class="ladder__kicker">{{ messages.practiceLadder.kicker(Number(nodeId) || 0) }}</p>

      <div class="ladder__hero">
        <span class="ladder__icon" aria-hidden="true">
          <ListOrdered :size="20" />
        </span>
        <div class="ladder__hero-title-wrap">
          <h1 class="ladder__title">{{ messages.practiceLadder.title }}</h1>
          <p class="ladder__sub">{{ messages.practiceLadder.sub(nodeTitle) }}</p>
        </div>
        <Badge variant="muted" class="ladder__hero-badge">
          {{ messages.practiceLadder.badge(Number(nodeId) || 0) }}
        </Badge>
      </div>

      <!-- Strip block-token tối — trọng số 3 bậc + index mono (dữ liệu chỉ số → quyết định #4) -->
      <div class="ladder__strip" aria-hidden="true">
        <p class="ladder__strip-label">{{ messages.practiceLadder.stripLabel }}</p>
        <div class="ladder__strip-blocks">
          <div v-for="item in STAGE_WEIGHTS" :key="item.index" class="ladder__strip-block">
            <span class="ladder__strip-value">{{ messages.practiceLadder.stripBlock(item.stage, item.weight) }}</span>
            <span class="ladder__strip-index">{{ item.index }}</span>
          </div>
        </div>
      </div>
    </Motion>

    <div v-if="quizLoading" class="ladder__loading">
      <Skeleton height="96px" :lines="3" />
    </div>

    <LadderShell
      v-else
      :node-id="nodeId"
      :quiz-exercise="quizExercise"
      :quiz-loading="quizLoading"
      :simulation-key="simKey"
      :code-exercise-id="codeExerciseId"
      @passed="onPassed"
    />

    <div class="ladder__actions">
      <Button variant="ghost" @click="router.push({ name: 'path-topic', params: { topicId: String(topicId) } })">
        <ArrowLeft :size="16" aria-hidden="true" />
        {{ messages.practiceLadder.exit }}
      </Button>
      <Button variant="secondary" @click="router.push({ name: 'lab', params: { nodeId } })">
        {{ messages.practiceLadder.openLab }}
        <ArrowRight :size="16" aria-hidden="true" />
      </Button>
    </div>
  </main>
</template>

<style scoped>
.ladder {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Chrome header — surface band level-2 (§6): card-raised + border-subtle, KHÔNG shadow ── */
.ladder__chrome {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-lg) var(--space-xl);
}

.ladder__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}

.ladder__breadcrumb a { color: var(--color-primary); font-weight: 600; }

.ladder__kicker {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
  margin: 0;
}

.ladder__hero {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.ladder__icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-text-tertiary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.ladder__hero-title-wrap { display: flex; flex-direction: column; gap: var(--space-xs); flex: 1; min-width: 220px; }

.ladder__title {
  font-size: var(--text-4xl);
  font-weight: 600;
  line-height: 1.1;
  letter-spacing: -0.03em;
  color: var(--color-foreground);
  margin: 0;
}

.ladder__sub { font-size: var(--text-sm); color: var(--color-text-secondary); max-width: 64ch; margin: 0; }

.ladder__hero-badge { margin-left: auto; align-self: flex-start; }

/* ── Strip block-token tối — chip canvas-ink + index mono (hero motif duy nhất/màn) ── */
.ladder__strip {
  display: flex;
  flex-wrap: wrap;
  align-items: flex-end;
  justify-content: space-between;
  gap: var(--space-md);
  border-top: 1px solid var(--color-border-subtle);
  padding-top: var(--space-md);
}

.ladder__strip-label {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
  margin: 0;
}

.ladder__strip-blocks {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
}

.ladder__strip-block {
  display: inline-flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-xs);
  min-width: 64px;
  padding: var(--space-xs) var(--space-sm);
  border-radius: var(--radius-md);
  background: var(--color-canvas-ink);
}

.ladder__strip-value {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  color: rgba(255, 255, 255, 0.92);
  line-height: 1.4;
  white-space: nowrap;
}

.ladder__strip-index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-index-muted);
  line-height: 1.4;
}

.ladder__loading { display: flex; flex-direction: column; gap: var(--space-md); }

.ladder__actions {
  display: flex;
  justify-content: space-between;
  gap: var(--space-sm);
  padding-top: var(--space-md);
  border-top: 1px solid var(--color-border);
}

@media (max-width: 640px) {
  .ladder__chrome { padding: var(--space-md); }
  .ladder__hero-badge { margin-left: 0; }
  .ladder__hero { align-items: flex-start; }
  .ladder__strip { align-items: flex-start; }
  .ladder__actions { flex-direction: column; align-items: stretch; }
}
</style>
