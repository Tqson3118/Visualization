<script setup lang="ts">
// LessonView — Màn 04: chi tiết bài học (SDD Màn 04)
// Phase 1 view-quality: banner = surface band level-2 (bỏ gradient sunset + text-shadow),
// icon lucide (ArrowLeft thay "←"), nút "Học tiếp" = hành động thật (mở mô phỏng đầu tiên
// của bài / chuyển tab Lý thuyết), weight 700 → 600, hover card chỉ đổi border (§6).
import { computed, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArrowLeft, CheckCircle2, Play, Puzzle } from 'lucide-vue-next';

import { useLessonStore } from '@/stores/lesson';
import { useUiStore } from '@/stores/ui';
import { getCatalogMeta } from '@/engines/catalog';
import LessonDetail from '@/components/lesson/LessonDetail.vue';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/Button.vue';
import Card from '@/components/ui/Card.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import Tabs, { type TabItem } from '@/components/ui/Tabs.vue';
import EmbeddedVisualizer from '@/components/visualizer/EmbeddedVisualizer.vue';

const route = useRoute();
const router = useRouter();
const lessonStore = useLessonStore();
const ui = useUiStore();

const lessonId = computed(() => String(route.params.lessonId ?? ''));
const error = ref('');
const marking = ref(false);
const activeTab = ref('content');
/** Trạng thái mở visualizer inline trong tab Lý thuyết ("Chạy thử thuật toán"). */
const runOpen = ref(false);

const lesson = computed(() => lessonStore.currentLesson);
const viewed = computed(() => lesson.value?.progress?.viewed ?? false);
const theoryMeta = computed(() => {
  const key = lesson.value?.simulations?.[0]?.simulationKey;
  return key ? getCatalogMeta(key) : undefined;
});
const theorySimKey = computed(() => lesson.value?.simulations?.[0]?.simulationKey ?? '');

const TABS: TabItem[] = [
  { key: 'content', label: 'Nội dung' },
  { key: 'theory', label: 'Lý thuyết' },
  { key: 'quiz', label: 'Quiz' },
];

onMounted(async () => {
  try {
    await lessonStore.fetchLesson(Number(lessonId.value));
  } catch {
    error.value = 'Bài học không tồn tại hoặc đã bị ẩn.';
  }
});

function openSimulation(key: string): void {
  void router.push({ name: 'simulator', params: { key } });
}

function openExercise(id: number): void {
  void router.push({ name: 'exercise', params: { id: String(id) } });
}

/** "Học tiếp": sang tab Lý thuyết và mở visualizer inline ngay trong lesson (single surface — B3). */
function onContinue(): void {
  activeTab.value = 'theory';
  if (theorySimKey.value) {
    runOpen.value = true;
  }
}

async function onMarkViewed(): Promise<void> {
  marking.value = true;
  try {
    await lessonStore.markViewed(Number(lessonId.value));
    ui.showToast('Đã đánh dấu bài học!', 'success');
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể đánh dấu.', 'error');
  } finally {
    marking.value = false;
  }
}
</script>

<template>
  <main class="lesson-view container">
    <nav class="lesson-view__breadcrumb" aria-label="Breadcrumb">
      <RouterLink :to="{ name: 'path' }">Lộ trình</RouterLink>
      <span aria-hidden="true">/</span>
      <span>{{ lesson?.title ?? 'Bài học' }}</span>
    </nav>

    <EmptyState
      v-if="error"
      icon="alert-circle"
      title="Bài học không tồn tại"
      :description="error"
      action-label="Về lộ trình"
      @action="router.push({ name: 'path' })"
    />

    <template v-else>
      <!-- Hero bài học — surface band level-2 (DESIGN.md §1), không gradient -->
      <header class="lesson-view__hero">
        <div class="lesson-view__hero-badges">
          <Badge variant="primary">Bài học</Badge>
          <Badge v-if="viewed" variant="success">Đã học</Badge>
        </div>
        <h1 class="lesson-view__hero-title">{{ lesson?.title ?? 'Bài học' }}</h1>
        <p class="lesson-view__hero-desc">{{ lesson?.description }}</p>
        <div class="lesson-view__hero-actions">
          <Button :loading="marking" :disabled="viewed" @click="onMarkViewed">
            <CheckCircle2 :size="16" aria-hidden="true" />
            {{ viewed ? 'Đã đánh dấu' : 'Đánh dấu đã học' }}
          </Button>
          <Button v-if="!viewed" variant="secondary" @click="onContinue">
            <Play :size="16" aria-hidden="true" />
            Học tiếp
          </Button>
          <Button variant="ghost" @click="router.push({ name: 'path' })">
            <ArrowLeft :size="16" aria-hidden="true" />
            Về lộ trình
          </Button>
        </div>
      </header>

      <!-- Tabs: Nội dung / Lý thuyết / Quiz (Tabs shadcn) -->
      <Tabs v-model="activeTab" :tabs="TABS" class="lesson-view__tabs">
        <!-- Nội dung: toàn bộ bài học (rich content + mô phỏng + bài tập + ghi chú + đánh giá) -->
        <section v-if="activeTab === 'content'" class="lesson-view__panel">
          <LessonDetail
            :lesson-id="lessonId"
            hide-header
            @open-simulation="openSimulation"
            @open-exercise="openExercise"
          />
        </section>

        <!-- Lý thuyết: bản đọc thuần + tóm tắt độ phức tạp -->
        <section v-else-if="activeTab === 'theory'" class="lesson-view__panel">
          <Card v-if="theoryMeta" class="lesson-view__theory-card">
            <dl class="lesson-view__theory-meta">
              <div>
                <dt>Độ phức tạp TB</dt>
                <dd>{{ theoryMeta.complexity.average }}</dd>
              </div>
              <div>
                <dt>Không gian</dt>
                <dd>{{ theoryMeta.complexity.space }}</dd>
              </div>
              <div>
                <dt>Cấp độ</dt>
                <dd>{{ theoryMeta.level }}</dd>
              </div>
            </dl>
          </Card>

          <!-- Chạy thử thuật toán: visual tương tác inline (KHÔNG thêm tab Visualizer riêng — B3).
               Chỉ hiện khi bài có simulationKey. -->
          <div v-if="theorySimKey" class="lesson-view__try">
            <Button
              v-if="!runOpen"
              variant="secondary"
              data-testid="run-algo-btn"
              @click="runOpen = true"
            >
              <Play :size="16" aria-hidden="true" />
              Chạy thử thuật toán
            </Button>
            <EmbeddedVisualizer
              v-else
              :simulation-key="theorySimKey"
              @close="runOpen = false"
            />
          </div>

          <article
            class="lesson-view__theory"
            v-html="lesson?.contentHtml || '<p>Bài học đang được biên soạn.</p>'"
          />
        </section>

        <!-- Quiz: bài tập trắc nghiệm liên quan -->
        <section v-else class="lesson-view__panel">
          <div v-if="lesson?.exercises && lesson.exercises.length > 0" class="lesson-view__quiz-list">
            <Card
              v-for="ex in lesson.exercises"
              :key="ex.id"
              class="lesson-view__quiz"
            >
              <div class="lesson-view__quiz-icon" aria-hidden="true">
                <Puzzle :size="18" />
              </div>
              <div class="lesson-view__quiz-info">
                <p class="lesson-view__quiz-title">{{ ex.title }}</p>
                <Badge variant="muted">{{ ex.type }}</Badge>
              </div>
              <Button size="sm" @click="openExercise(ex.id)">
                <Play :size="14" aria-hidden="true" />
                Làm bài
              </Button>
            </Card>
          </div>
          <EmptyState
            v-else
            icon="puzzle"
            title="Chưa có bài tập quiz"
            description="Bài tập trắc nghiệm của bài học này đang được biên soạn — quay lại sau nhé."
          />
        </section>
      </Tabs>
    </template>
  </main>
</template>

<style scoped>
.lesson-view {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

.lesson-view__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.04em;
  color: var(--color-text-muted);
}

.lesson-view__breadcrumb a { color: var(--color-primary); font-weight: 600; text-decoration: none; }

/* ── Hero surface band level-2 (DESIGN.md §1 + §6) ── */
.lesson-view__hero {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  padding: var(--space-xl);
  border-radius: var(--radius-lg);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
}

.lesson-view__hero-badges {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
}

.lesson-view__hero-title {
  font-size: var(--text-3xl);
  font-weight: 600;
  letter-spacing: -0.02em;
  margin: 0;
}

.lesson-view__hero-desc {
  color: var(--color-text-muted);
  font-size: var(--text-sm);
  margin: 0;
}

.lesson-view__hero-actions {
  display: flex;
  gap: var(--space-sm);
  flex-wrap: wrap;
  align-items: center;
  margin-top: var(--space-sm);
}

/* ── Tabs ── */
.lesson-view__tabs { margin-top: var(--space-sm); }

.lesson-view__panel { padding-top: var(--space-sm); }

/* ── Lý thuyết ── */
.lesson-view__theory-card {
  margin-bottom: var(--space-md);
}

/* Khu "Chạy thử thuật toán" — visual tương tác inline trong Lý thuyết (B3) */
.lesson-view__try {
  margin-bottom: var(--space-md);
}

.lesson-view__theory-meta {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(120px, 1fr));
  gap: var(--space-md);
  margin: 0;
}

.lesson-view__theory-meta dt {
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  font-weight: 500;
}

.lesson-view__theory-meta dd {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 600;
  margin-top: var(--space-xs);
}

.lesson-view__theory {
  font-size: var(--text-base);
  line-height: 1.75;
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-lg);
}

.lesson-view__theory :deep(h2) { font-size: var(--text-lg); font-weight: 600; margin-block: var(--space-md) var(--space-sm); }
.lesson-view__theory :deep(h3) { font-size: var(--text-md); font-weight: 600; margin-block: var(--space-md) var(--space-sm); }
.lesson-view__theory :deep(p) { margin-bottom: var(--space-sm); }
.lesson-view__theory :deep(pre) { background: var(--color-muted); padding: var(--space-md); border-radius: var(--radius-md); overflow-x: auto; }
.lesson-view__theory :deep(code) { font-family: var(--font-mono); font-size: var(--text-sm); }
.lesson-view__theory :deep(table) { border-collapse: collapse; width: 100%; margin-block: var(--space-md); }
.lesson-view__theory :deep(th), .lesson-view__theory :deep(td) { border: 1px solid var(--color-border); padding: var(--space-sm) var(--space-sm); text-align: left; }

/* ── Quiz ── */
.lesson-view__quiz-list {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.lesson-view__quiz {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-md);
  transition: border-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.lesson-view__quiz:hover { border-color: var(--color-border-strong); }

.lesson-view__quiz-icon {
  flex-shrink: 0;
  width: 38px;
  height: 38px;
  border-radius: var(--radius-md);
  background: var(--color-muted);
  color: var(--color-text-secondary);
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.lesson-view__quiz-info { flex: 1; min-width: 0; display: flex; flex-direction: column; gap: var(--space-xs); }
.lesson-view__quiz-title { font-weight: 600; font-size: var(--text-sm); margin: 0; }

@media (prefers-reduced-motion: reduce) {
  .lesson-view__quiz { transition: none; }
}
</style>
