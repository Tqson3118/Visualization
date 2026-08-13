<script setup lang="ts">
// CodeRunnerView — Màn 16: Code Runner (Module I) tại /code/:key
// Editor (textarea — Monaco sẽ bật khi cài gói monaco-editor) + chạy sandbox +
// canvas 2 chiều từ generator thật + panel test ẩn sau nộp + lịch sử nộp.
// P1-B2: chrome surface band level-2 (bỏ gradient/shadow), editor LUÔN tối
// (canvas-ink — quyết định #5), bỏ ghi chú dev Monaco → caption phím tắt mono,
// lucide icons thay ký tự ◀▶▶, EmptyState component chung.
// GIỮ textarea aria-label + text "Thành công · Xms" (e2e).
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Code2, History, Play, StepBack, StepForward, Terminal } from 'lucide-vue-next';

import { useCodeRunnerStore } from '@/stores/codeRunner';
import { useSimulationStore } from '@/stores/simulation';
import { useUiStore } from '@/stores/ui';
import { getCatalogMeta } from '@/engines/catalog';
import type { TraceEvent } from '@/engines/core/stepExecutor';
import { useCodeTracePlayback } from '@/composables/useCodeTracePlayback';
import CanvasArea from '@/components/simulator/CanvasArea.vue';
import ControlBar from '@/components/simulator/ControlBar.vue';
import StatsBar from '@/components/simulator/StatsBar.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';
import EmptyState from '@/components/ui/EmptyState.vue';

const route = useRoute();
const router = useRouter();
const codeStore = useCodeRunnerStore();
const simStore = useSimulationStore();
const ui = useUiStore();

// Trace playback (useCodeTracePlayback) — refs lồng trong object thường KHÔNG tự unwrap ở
// template (chỉ top-level binding unwrap), nên destructure refs ra top-level để template
// dùng trực tiếp (playbackIndex/playbackStructure/...); method vẫn gọi qua `playback`.
const playback = useCodeTracePlayback();
const {
  currentIndex: playbackIndex,
  totalFrames: playbackFrames,
  currentStructure: playbackStructure,
  currentLine: playbackLine,
} = playback;

/** Trace lần chạy gần nhất (sandbox) — null khi không có → fallback generator preview. */
const traceRef = ref<TraceEvent[] | null>(null);
const traceMode = computed(() => !!traceRef.value && traceRef.value.length > 0);

/** Trạng thái playback cho ControlBar — 'running'/'idle' hợp lệ với SimulationStatus. */
const playbackStatus = computed(() => (playback.isPlaying.value ? 'running' : 'idle'));

/** Speed multiplier cho ControlBar — ngược với ms: mult = 1000 / durationPerStep (250ms = 4x). */
const playbackSpeed = computed(() => Math.round(1000 / playback.durationPerStep.value));

/** Biến frame trace hiện tại — tối đa 10 mục cho vars panel. */
const playbackVarsList = computed<{ key: string; value: unknown }[]>(() =>
  Object.entries(playback.currentVars.value)
    .slice(0, 10)
    .map(([key, value]) => ({ key, value })),
);

function formatVarValue(value: unknown): string {
  if (value === null) return 'null';
  if (value === undefined) return 'undefined';
  if (typeof value === 'object') return JSON.stringify(value);
  return String(value);
}

const key = computed(() => String(route.params.key ?? ''));
const loading = ref(true);
const error = ref('');

const meta = computed(() => getCatalogMeta(key.value));

const historyOpen = ref(false);

/** Số dòng hiển thị ở gutter (đồng bộ với nội dung textarea) */
const gutterLines = computed(() =>
  Array.from({ length: codeStore.editorCode.split('\n').length }, (_, i) => i + 1),
);

const gutterRef = ref<HTMLDivElement | null>(null);

function onEditorScroll(event: Event): void {
  const el = event.target as HTMLTextAreaElement;
  if (gutterRef.value) gutterRef.value.scrollTop = el.scrollTop;
}

onMounted(async () => {
  try {
    await codeStore.loadTemplate(key.value);
    // Nạp steps mẫu từ generator thật để hiển thị canvas 2 chiều
    await simStore.loadSim(key.value).catch(() => {
      /* không bắt buộc */
    });
  } finally {
    loading.value = false;
  }
});

onBeforeUnmount(() => {
  playback.dispose();
  simStore.stopPlayback();
});

async function onRun(): Promise<void> {
  const result = await codeStore.run();
  if (codeStore.runState === 'passed' && result && Array.isArray(result.trace) && result.trace.length > 0) {
    traceRef.value = result.trace;
    playback.init(result.trace);
    playback.play();
  } else {
    // Trace rỗng/error/timeout → giữ hành vi cũ: generator preview + stats + 2 nút step
    traceRef.value = null;
  }
  if (codeStore.runState === 'passed') {
    ui.showToast('Chạy thành công!', 'success');
  }
}

/** Bỏ trace → quay về preview generator mẫu (stats giữ nguyên, playback reset về đầu). */
function showSamplePreview(): void {
  traceRef.value = null;
  playback.reset();
}

function onPlaybackPlay(): void {
  playback.play();
}

function onPlaybackPause(): void {
  playback.pause();
}

function onPlaybackStepBack(): void {
  playback.stepBack();
}

function onPlaybackStepForward(): void {
  playback.stepForward();
}

function onPlaybackReset(): void {
  playback.reset();
}

function onPlaybackSpeed(multiplier: number): void {
  playback.setSpeed(1000 / multiplier);
}

async function toggleHistory(): Promise<void> {
  historyOpen.value = !historyOpen.value;
  if (historyOpen.value) {
    // Không có exerciseId → lấy danh sách nộp chung (nếu có exercise gắn với key)
    await codeStore.fetchHistory(0).catch(() => undefined);
  }
}
</script>

<template>
  <main class="code-runner container">
    <!-- Chrome header — surface band level-2 + kicker mono (DESIGN.md §1, không gradient) -->
    <header class="code-runner__chrome">
      <div class="code-runner__header">
        <div>
          <nav class="code-runner__breadcrumb" aria-label="Breadcrumb">
            <RouterLink :to="{ name: 'simulations' }">Khám phá</RouterLink>
            <span aria-hidden="true">/</span>
            <span>{{ meta?.title ?? key }}</span>
          </nav>
          <p class="code-runner__kicker">CODE CHALLENGE · {{ key }}</p>
          <h1 class="code-runner__title">{{ meta?.title ?? key }}</h1>
          <p class="code-runner__sub">
            Nộp bài: output đúng là đạt. Dùng hàm có sẵn (VD sort()) → vẫn đạt nhưng KHÔNG xem được mô phỏng bước.
          </p>
        </div>
        <Button variant="ghost" size="sm" @click="toggleHistory">
          <History :size="16" aria-hidden="true" />
          {{ historyOpen ? 'Ẩn lịch sử' : 'Lịch sử nộp' }}
        </Button>
      </div>
    </header>

    <div v-if="loading" class="code-runner__loading">
      <Skeleton height="280px" />
    </div>

    <EmptyState
      v-else-if="!meta"
      icon="database"
      title="Không tìm thấy bài thử thách"
      :description="`Key '${key}' chưa có trong danh mục mô phỏng — kiểm tra lại đường dẫn hoặc quay về danh mục.`"
      action-label="Về danh mục"
      @action="router.push({ name: 'simulations' })"
    />

    <template v-else>
      <div class="code-runner__layout">
        <!-- Trái: editor -->
        <section class="code-runner__panel code-runner__editor-panel" aria-label="Trình soạn mã">
          <header class="code-runner__panel-header">
            <span class="code-runner__panel-title">
              <Code2 :size="16" aria-hidden="true" />
              Editor
            </span>
            <Badge variant="muted">{{ key }}</Badge>
          </header>

          <div class="code-runner__editor-wrap">
            <div ref="gutterRef" class="code-runner__gutter" aria-hidden="true">
              <span
                v-for="line in gutterLines"
                :key="line"
                class="code-runner__gutter-line"
                :class="{ 'code-runner__gutter-line--active': traceMode && line === playbackLine }"
              >
                {{ line }}
              </span>
            </div>
            <textarea
              v-model="codeStore.editorCode"
              class="code-runner__textarea"
              spellcheck="false"
              :aria-label="`Trình soạn mã ${key}`"
              @scroll="onEditorScroll"
            />
          </div>

          <p class="code-runner__note">
            <kbd>Ctrl+Enter</kbd> chạy code · <kbd>Ctrl+Z</kbd> hoàn tác · <kbd>Ctrl+Shift+Z</kbd> làm lại
          </p>
          <div class="code-runner__actions">
            <Button size="sm" :loading="codeStore.isRunning" @click="onRun">
              <Play :size="16" aria-hidden="true" />
              Chạy (Ctrl+Enter)
            </Button>
            <Button variant="ghost" size="sm" @click="codeStore.restoreTemplate()">Khôi phục code mẫu</Button>
          </div>
        </section>

        <!-- Phải: output + canvas -->
        <section class="code-runner__panel code-runner__output-panel" aria-label="Kết quả chạy">
          <header class="code-runner__panel-header">
            <span class="code-runner__panel-title">
              <Terminal :size="16" aria-hidden="true" />
              Output
            </span>
            <Badge
              :variant="codeStore.runError ? 'danger' : codeStore.lastStats ? 'success' : 'muted'"
            >
              {{ codeStore.runError ? 'Lỗi' : codeStore.lastStats ? 'Thành công' : 'Sẵn sàng' }}
            </Badge>
          </header>

          <div class="code-runner__status-box">
            <div v-if="codeStore.runError" class="code-runner__status code-runner__status--error" role="alert">
              {{ codeStore.runError }}
            </div>
            <div
              v-else-if="codeStore.lastStats"
              class="code-runner__status code-runner__status--ok"
              role="status"
            >
              Thành công · {{ codeStore.lastStats.durationMs }}ms ·
              {{ codeStore.lastStats.comparisons }} so sánh ·
              {{ codeStore.lastStats.swaps }} hoán đổi
            </div>
            <div v-else class="code-runner__status code-runner__status--idle" role="status">
              <Terminal :size="16" aria-hidden="true" />
              Chưa có kết quả — bấm <strong>Chạy</strong> để thực thi code trong sandbox.
            </div>
          </div>

          <pre v-if="codeStore.lastOutput !== null" class="code-runner__output-box">
{{ JSON.stringify(codeStore.lastOutput, null, 2) }}
          </pre>

          <div class="code-runner__divider" role="presentation" />

          <div class="code-runner__visual">
            <CanvasArea
              :structure="traceMode ? playbackStructure : simStore.currentStep?.structure ?? null"
              :empty-text="'Canvas 2 chiều — đồng bộ theo trace code của bạn'"
            />
            <StatsBar
              :comparisons="codeStore.lastStats?.comparisons ?? 0"
              :swaps="codeStore.lastStats?.swaps ?? 0"
              :writes="codeStore.lastStats?.writes ?? 0"
              :step="traceMode ? playbackIndex : simStore.currentIndex"
              :total-steps="traceMode ? playbackFrames : simStore.steps.length"
            />
            <div
              v-if="traceMode && playbackVarsList.length > 0"
              class="code-runner__vars"
              aria-label="Biến hiện tại"
            >
              <div v-for="entry in playbackVarsList" :key="entry.key" class="code-runner__vars-row">
                <span class="code-runner__vars-key">{{ entry.key }}</span>
                <span class="code-runner__vars-eq" aria-hidden="true">=</span>
                <span class="code-runner__vars-value">{{ formatVarValue(entry.value) }}</span>
              </div>
            </div>
            <div class="code-runner__sim-controls">
              <template v-if="traceMode">
                <ControlBar
                  :current-index="playbackIndex"
                  :total-frames="playbackFrames"
                  :status="playbackStatus"
                  :speed="playbackSpeed"
                  @play="onPlaybackPlay"
                  @pause="onPlaybackPause"
                  @step-back="onPlaybackStepBack"
                  @step-forward="onPlaybackStepForward"
                  @reset="onPlaybackReset"
                  @set-speed="onPlaybackSpeed"
                />
                <Button variant="ghost" size="sm" @click="showSamplePreview">Xem lại mẫu</Button>
              </template>
              <template v-else>
                <Button
                  variant="ghost"
                  size="icon"
                  aria-label="Bước lùi"
                  :disabled="simStore.isFirst"
                  @click="simStore.stepBack()"
                >
                  <StepBack :size="16" aria-hidden="true" />
                </Button>
                <Button
                  variant="ghost"
                  size="icon"
                  aria-label="Bước tới"
                  :disabled="simStore.isLast"
                  @click="simStore.stepForward()"
                >
                  <StepForward :size="16" aria-hidden="true" />
                </Button>
                <span class="code-runner__step-info">
                  Bước {{ simStore.currentIndex + 1 }}/{{ simStore.steps.length }}
                </span>
              </template>
            </div>
          </div>
        </section>
      </div>

      <!-- Lịch sử nộp -->
      <section v-if="historyOpen" class="code-runner__history">
        <h2 class="code-runner__history-title">Lịch sử nộp</h2>
        <p v-if="codeStore.submissions.length === 0" class="code-runner__history-empty">
          Chưa có bài nộp — nộp từ Bậc 3 (Ladder) sẽ hiển thị ở đây.
        </p>
        <ul v-else class="code-runner__history-list">
          <li v-for="sub in codeStore.submissions" :key="sub.id">
            <Badge :variant="sub.status === 'passed' ? 'success' : 'danger'">{{ sub.status }}</Badge>
            <span class="code-runner__history-date">{{ sub.createdAt }}</span>
          </li>
        </ul>
      </section>
    </template>
  </main>
</template>

<style scoped>
.code-runner {
  padding-block: var(--space-lg) var(--space-2xl);
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
}

/* ── Chrome header — surface band level-2 (DESIGN.md §1 + §6, không gradient/shadow) ── */
.code-runner__chrome {
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  padding: var(--space-lg) var(--space-xl);
}

.code-runner__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
  margin-bottom: var(--space-xs);
}

.code-runner__breadcrumb a { color: var(--color-primary); font-weight: 600; }

.code-runner__header {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  align-items: flex-start;
  flex-wrap: wrap;
}

.code-runner__kicker {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--color-primary);
  font-weight: 500;
}

.code-runner__title {
  font-size: var(--text-3xl);
  font-weight: 600;
  letter-spacing: -0.025em;
  color: var(--color-text-primary);
  line-height: 1.15;
  margin-top: var(--space-xs);
}

.code-runner__sub { font-size: var(--text-sm); margin-top: var(--space-xs); max-width: 48rem; color: var(--color-text-secondary); }

.code-runner__loading { display: flex; flex-direction: column; gap: var(--space-md); }

.code-runner__layout {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-md);
  align-items: start;
}

/* ── Panel chung — elevation level-1, KHÔNG shadow (§6) ── */
.code-runner__panel {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-surface);
  padding: var(--space-md);
}

.code-runner__panel-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
  padding-bottom: var(--space-sm);
  border-bottom: 1px solid var(--color-border);
}

.code-runner__panel-title {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-text-primary);
}

/* ── Editor: vùng dữ liệu LUÔN tối (canvas-ink) bất kể theme (quyết định #5) ── */
.code-runner__editor-wrap {
  display: flex;
  border: 1px solid color-mix(in srgb, var(--color-index-muted) 45%, transparent);
  border-radius: var(--radius-md);
  overflow: hidden;
  background: var(--color-canvas-ink);
}

.code-runner__gutter {
  width: 48px;
  flex-shrink: 0;
  overflow: hidden;
  /* padding-left giữ chỗ cho border-left của dòng active (không xê dịch text khi đổi dòng) */
  padding: var(--space-md) var(--space-sm) var(--space-md) var(--space-xs);
  background: color-mix(in srgb, var(--color-canvas-ink) 60%, var(--color-index-muted) 8%);
  border-right: 1px solid color-mix(in srgb, var(--color-index-muted) 35%, transparent);
  text-align: right;
  user-select: none;
}

.code-runner__gutter-line {
  display: block;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  line-height: 1.6;
  color: var(--color-index-muted);
}

.code-runner__gutter-line--active {
  color: var(--color-primary);
  font-weight: 700;
  background: color-mix(in srgb, var(--color-primary) 12%, transparent);
  border-left: 2px solid var(--color-primary);
}

.code-runner__textarea {
  width: 100%;
  min-height: 320px;
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  line-height: 1.6;
  border: none;
  outline: none;
  background: transparent;
  color: color-mix(in srgb, white 85%, var(--color-index-muted));
  padding: var(--space-md);
  resize: vertical;
  tab-size: 2;
  caret-color: var(--color-primary);
}

.code-runner__textarea:focus { outline: none; }

.code-runner__textarea::selection { background: color-mix(in srgb, var(--color-primary) 32%, transparent); }

.code-runner__note { font-size: var(--text-xs); color: var(--color-text-tertiary); }
.code-runner__note kbd {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  padding: 1px var(--space-xs);
  background: var(--color-muted);
}

.code-runner__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

/* ── Output panel ── */
.code-runner__status-box {
  min-height: 48px;
  display: flex;
  align-items: center;
}

.code-runner__status {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
  width: 100%;
}

.code-runner__status--ok {
  color: var(--color-success);
  background: color-mix(in srgb, var(--color-success) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-success) 30%, transparent);
}

.code-runner__status--error {
  color: var(--color-destructive);
  background: color-mix(in srgb, var(--color-destructive) 9%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-destructive) 30%, transparent);
}

.code-runner__status--idle {
  color: var(--color-text-tertiary);
  background: var(--color-muted);
  border: 1px dashed var(--color-border);
}

/* Output = dữ liệu → nền tối canvas-ink (quyết định #5) */
.code-runner__output-box {
  background: var(--color-canvas-ink);
  border: 1px solid color-mix(in srgb, var(--color-index-muted) 45%, transparent);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: color-mix(in srgb, white 85%, var(--color-index-muted));
  overflow-x: auto;
  min-height: 56px;
  margin: 0;
  white-space: pre-wrap;
}

.code-runner__divider {
  height: 1px;
  background: var(--color-border);
}

.code-runner__visual { display: flex; flex-direction: column; gap: var(--space-sm); }

/* ── Vars panel (trace playback) — nền dữ liệu tối canvas-ink (quyết định #5) ── */
.code-runner__vars {
  display: flex;
  flex-direction: column;
  gap: 2px;
  max-height: 96px;
  overflow-y: auto;
  background: var(--color-canvas-ink);
  border: 1px solid color-mix(in srgb, var(--color-index-muted) 45%, transparent);
  border-radius: var(--radius-md);
  padding: var(--space-xs) var(--space-sm);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: color-mix(in srgb, white 85%, var(--color-index-muted));
}

.code-runner__vars-row {
  display: flex;
  align-items: baseline;
  gap: var(--space-xs);
  line-height: 1.6;
}

.code-runner__vars-key {
  color: var(--color-primary);
  font-weight: 600;
}

.code-runner__vars-eq { color: var(--color-index-muted); }

.code-runner__vars-value { word-break: break-all; }

.code-runner__sim-controls {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.code-runner__step-info { font-size: var(--text-xs); color: var(--color-text-tertiary); font-family: var(--font-mono); }

.code-runner__history {
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-surface);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.code-runner__history-title { font-size: var(--text-md); font-weight: 600; letter-spacing: -0.015em; }

.code-runner__history-empty { font-size: var(--text-sm); color: var(--color-text-tertiary); }

.code-runner__history-list {
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.code-runner__history-list li {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  font-size: var(--text-sm);
}

.code-runner__history-date { font-family: var(--font-mono); font-size: var(--text-xs); color: var(--color-text-tertiary); }

@media (max-width: 1000px) {
  .code-runner__layout { grid-template-columns: 1fr; }
}
</style>
