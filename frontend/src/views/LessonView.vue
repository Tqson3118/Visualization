<script setup lang="ts">
// LessonView — Màn 04: chi tiết bài học (SDD Màn 04)
// Phase 1 view-quality: banner = surface band level-2 (bỏ gradient sunset + text-shadow),
// icon lucide (ArrowLeft thay "←"), nút "Học tiếp" = hành động thật (mở mô phỏng đầu tiên
// của bài / chuyển tab Lý thuyết), weight 700 → 600, hover card chỉ đổi border (§6).
import { computed, h, nextTick, onBeforeUnmount, onMounted, ref, render, watch, type Component } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { ArrowLeft, Check, CheckCircle2, Copy, Play, Puzzle } from 'lucide-vue-next';

import { useLessonStore } from '@/stores/lesson';
import { useUiStore } from '@/stores/ui';
import { getCatalogMeta } from '@/engines/catalog';
import LessonDetail from '@/components/lesson/LessonDetail.vue';
import Badge from '@/components/ui/Badge.vue';
import Button from '@/components/ui/Button.vue';
import Card from '@/components/ui/Card.vue';
import EmptyState from '@/components/ui/EmptyState.vue';
import RevealSection from '@/components/ui/RevealSection.vue';
import Tabs, { type TabItem } from '@/components/ui/Tabs.vue';

const route = useRoute();
const router = useRouter();
const lessonStore = useLessonStore();
const ui = useUiStore();

const lessonId = computed(() => String(route.params.lessonId ?? ''));
const error = ref('');
const marking = ref(false);
const activeTab = ref('content');

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

/** "Học tiếp": bước thao tác thật — mô phỏng đầu tiên của bài, nếu chưa có thì sang Lý thuyết. */
function onContinue(): void {
  if (theorySimKey.value) {
    openSimulation(theorySimKey.value);
  } else {
    activeTab.value = 'theory';
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

/* ── UI-PREMIUM 1B: stepper dots (Nội dung → Lý thuyết → Quiz) ── */
function tabIndex(key: string): number {
  return Math.max(0, TABS.findIndex((t) => t.key === key));
}

/* ── UI-PREMIUM 1B: copy button cho code block (v-html — gắn qua DOM) ── */
const contentPanelRef = ref<HTMLElement | null>(null);
const theoryPanelRef = ref<HTMLElement | null>(null);
let copyObserver: MutationObserver | null = null;

function mountIcon(host: HTMLElement, icon: Component, size: number): void {
  host.replaceChildren();
  const wrapper = document.createElement('span');
  render(h(icon, { size, 'aria-hidden': true }), wrapper);
  const svg = wrapper.firstElementChild;
  if (svg) host.appendChild(svg);
  render(null, wrapper);
}

async function copyCode(pre: HTMLElement, btn: HTMLButtonElement): Promise<void> {
  const code = pre.querySelector('code')?.textContent ?? pre.textContent ?? '';
  try {
    await navigator.clipboard.writeText(code);
  } catch {
    const ta = document.createElement('textarea');
    ta.value = code;
    ta.style.position = 'fixed';
    ta.style.opacity = '0';
    document.body.appendChild(ta);
    ta.select();
    document.execCommand('copy');
    ta.remove();
  }
  const iconHost = btn.querySelector<HTMLElement>('.lesson-view__copy-icon');
  const label = btn.querySelector<HTMLElement>('.lesson-view__copy-label');
  if (iconHost) mountIcon(iconHost, Check, 14);
  btn.classList.add('lesson-view__copy--done');
  if (label) label.textContent = 'Đã chép';
  window.setTimeout(() => {
    if (iconHost) mountIcon(iconHost, Copy, 14);
    btn.classList.remove('lesson-view__copy--done');
    if (label) label.textContent = 'Sao chép';
  }, 1600);
}

function attachCopyButtons(root: HTMLElement): void {
  root.querySelectorAll('pre').forEach((pre) => {
    if (pre.dataset.copyReady) return;
    pre.dataset.copyReady = '1';
    pre.classList.add('lesson-view__codeblock');
    const btn = document.createElement('button');
    btn.type = 'button';
    btn.className = 'lesson-view__copy';
    btn.setAttribute('aria-label', 'Sao chép code');
    btn.addEventListener('click', () => void copyCode(pre, btn));
    const iconHost = document.createElement('span');
    iconHost.className = 'lesson-view__copy-icon';
    mountIcon(iconHost, Copy, 14);
    btn.appendChild(iconHost);
    const label = document.createElement('span');
    label.className = 'lesson-view__copy-label';
    label.textContent = 'Sao chép';
    btn.appendChild(label);
    pre.appendChild(btn);
  });
}

function setupCopy(): void {
  copyObserver?.disconnect();
  copyObserver = null;
  const targets = [contentPanelRef.value, theoryPanelRef.value].filter(
    (t): t is HTMLElement => t !== null,
  );
  if (targets.length === 0) return;
  for (const t of targets) {
    attachCopyButtons(t);
    if (!copyObserver) {
      copyObserver = new MutationObserver(() => {
        for (const tt of targets) attachCopyButtons(tt);
      });
    }
    copyObserver.observe(t, { childList: true, subtree: true });
  }
}

onMounted(() => {
  void nextTick(setupCopy);
});

watch(activeTab, () => void nextTick(setupCopy));
watch(
  () => lesson.value?.contentHtml,
  () => void nextTick(setupCopy),
);

onBeforeUnmount(() => {
  copyObserver?.disconnect();
  copyObserver = null;
});
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
      <RevealSection preset="fadeDown">
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
      </RevealSection>

      <!-- Stepper dots — Nội dung → Lý thuyết → Quiz (UI-PREMIUM 1B) -->
      <RevealSection preset="fadeUp" :delay="60">
        <nav class="lesson-view__stepper" aria-label="Tiến trình bài học">
          <button
            v-for="tab in TABS"
            :key="tab.key"
            type="button"
            class="lesson-view__step"
            :class="{
              'lesson-view__step--active': activeTab === tab.key,
              'lesson-view__step--done': tabIndex(tab.key) < tabIndex(activeTab),
            }"
            :aria-current="activeTab === tab.key ? 'step' : undefined"
            @click="activeTab = tab.key"
          >
            <span class="lesson-view__step-dot" aria-hidden="true" />
            <span class="lesson-view__step-label">{{ tab.label }}</span>
          </button>
        </nav>
      </RevealSection>

      <!-- Tabs: Nội dung / Lý thuyết / Quiz (Tabs shadcn) -->
      <Tabs v-model="activeTab" :tabs="TABS" class="lesson-view__tabs">
        <!-- Nội dung: toàn bộ bài học (rich content + mô phỏng + bài tập + ghi chú + đánh giá) -->
        <section v-if="activeTab === 'content'" ref="contentPanelRef" class="lesson-view__panel">
          <RevealSection :delay="40">
            <LessonDetail
              :lesson-id="lessonId"
              hide-header
              @open-simulation="openSimulation"
              @open-exercise="openExercise"
            />
          </RevealSection>
        </section>

        <!-- Lý thuyết: bản đọc thuần + tóm tắt độ phức tạp -->
        <section v-else-if="activeTab === 'theory'" ref="theoryPanelRef" class="lesson-view__panel">
          <RevealSection :delay="40">
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
          </RevealSection>
          <RevealSection :delay="120">
            <article
              class="lesson-view__theory"
              v-html="lesson?.contentHtml || '<p>Bài học đang được biên soạn.</p>'"
            />
          </RevealSection>
        </section>

        <!-- Quiz: bài tập trắc nghiệm liên quan -->
        <section v-else class="lesson-view__panel">
          <div v-if="lesson?.exercises && lesson.exercises.length > 0" class="lesson-view__quiz-list">
            <RevealSection v-for="(ex, idx) in lesson.exercises" :key="ex.id" :delay="idx * 80">
              <Card
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
            </RevealSection>
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

/* ── Stepper dots (UI-PREMIUM 1B): chấm tiến trình animate fill ── */
.lesson-view__stepper {
  display: flex;
  align-items: center;
  gap: var(--space-lg);
  flex-wrap: wrap;
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-card);
}

.lesson-view__step {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  background: none;
  border: none;
  padding: var(--space-xs);
  border-radius: var(--radius-sm);
  cursor: pointer;
  color: var(--color-text-tertiary);
  font-size: var(--text-sm);
  font-weight: 500;
  transition: color var(--duration-fast) var(--ease-out-quad);
}

.lesson-view__step:hover { color: var(--color-text-primary); }

.lesson-view__step-dot {
  width: 12px;
  height: 12px;
  border-radius: 50%;
  border: 2px solid var(--color-border-strong);
  background: var(--color-surface);
  position: relative;
  transition:
    border-color var(--duration-fast) var(--ease-out-quad),
    background-color var(--duration-fast) var(--ease-out-quad);
}

.lesson-view__step--active {
  color: var(--color-primary);
  font-weight: 600;
}

.lesson-view__step--active .lesson-view__step-dot {
  border-color: var(--color-primary);
  background: var(--color-primary);
  box-shadow: 0 0 0 4px color-mix(in srgb, var(--color-primary) 16%, transparent);
  animation: lesson-step-fill 280ms var(--ease-out-expo);
}

.lesson-view__step--done {
  color: var(--color-success);
}

.lesson-view__step--done .lesson-view__step-dot {
  border-color: var(--color-success);
  background: var(--color-success);
}

@keyframes lesson-step-fill {
  0% { transform: scale(0.4); opacity: 0.3; }
  100% { transform: scale(1); opacity: 1; }
}

.lesson-view__panel { padding-top: var(--space-sm); }

/* ── Code block — copy button hover glow (UI-PREMIUM 1B)
   Buttons được chèn qua DOM (v-html) nên cần :deep() để scoped style áp dụng
   (compiled: [data-v-parent] .lesson-view__copy — khớp mọi descendant). ── */
.lesson-view__theory :deep(.lesson-view__codeblock),
.lesson-detail :deep(.lesson-view__codeblock) {
  position: relative;
  padding-right: 2.5rem;
}

:deep(.lesson-view__copy) {
  position: absolute;
  top: var(--space-sm);
  right: var(--space-sm);
  z-index: 2;
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  background: color-mix(in srgb, var(--color-surface) 92%, transparent);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-xs) var(--space-sm);
  font-size: var(--text-xs);
  font-weight: 500;
  color: var(--color-text-secondary);
  cursor: pointer;
  transition:
    color var(--duration-fast) var(--ease-out-quad),
    border-color var(--duration-fast) var(--ease-out-quad),
    box-shadow var(--duration-fast) var(--ease-out-quad);
}

:deep(.lesson-view__copy:hover) {
  color: var(--color-primary);
  border-color: var(--color-primary);
  box-shadow: var(--glow-primary);
}

:deep(.lesson-view__copy--done) {
  color: var(--color-success);
  border-color: var(--color-success);
  box-shadow: var(--glow-resolved);
}

:deep(.lesson-view__copy-icon) {
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

@media (prefers-reduced-motion: reduce) {
  .lesson-view__step-dot,
  :deep(.lesson-view__copy) {
    transition: none;
  }
  .lesson-view__step--active .lesson-view__step-dot {
    animation: none;
  }
}


/* ── Lý thuyết ── */
.lesson-view__theory-card {
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
