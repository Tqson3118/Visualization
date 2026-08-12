<script setup lang="ts">
// CodeRunnerView — Màn 16: Code Runner (Module I) tại /code/:key
// Editor (textarea — Monaco sẽ bật khi cài gói monaco-editor) + chạy sandbox +
// canvas 2 chiều từ generator thật + panel test ẩn sau nộp + lịch sử nộp.
// G-F2c: layout 2 cột (editor trái / output phải) + toolbar shadcn + số dòng +
// JetBrains Mono + EmptyState/error style. GIỮ textarea aria-label + text "Thành công · Xms" (e2e).
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { Code2, History, Terminal } from 'lucide-vue-next';

import { useCodeRunnerStore } from '@/stores/codeRunner';
import { useSimulationStore } from '@/stores/simulation';
import { useUiStore } from '@/stores/ui';
import { getCatalogMeta } from '@/engines/catalog';
import CanvasArea from '@/components/simulator/CanvasArea.vue';
import StatsBar from '@/components/simulator/StatsBar.vue';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';

const route = useRoute();
const router = useRouter();
const codeStore = useCodeRunnerStore();
const simStore = useSimulationStore();
const ui = useUiStore();

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
  simStore.stopPlayback();
});

async function onRun(): Promise<void> {
  await codeStore.run();
  if (codeStore.runState === 'passed') {
    ui.showToast('Chạy thành công!', 'success');
  }
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
    <!-- Chrome header — Cyber Mint nhẹ -->
    <header class="code-runner__chrome">
      <div class="code-runner__header">
        <div>
          <nav class="code-runner__breadcrumb" aria-label="Breadcrumb">
            <RouterLink :to="{ name: 'simulations' }">Khám phá</RouterLink>
            <span aria-hidden="true">/</span>
            <span>{{ meta?.title ?? key }}</span>
          </nav>
          <h1 class="code-runner__title">💻 {{ meta?.title ?? key }} — Code Challenge</h1>
          <p class="code-runner__sub">
            Nộp bài: output đúng là đạt. Dùng hàm có sẵn (VD sort()) → vẫn đạt nhưng KHÔNG xem được mô phỏng bước.
          </p>
        </div>
        <Button variant="ghost" size="sm" @click="toggleHistory">
          <History :size="15" aria-hidden="true" />
          {{ historyOpen ? 'Ẩn lịch sử' : 'Lịch sử nộp' }}
        </Button>
      </div>
    </header>

    <div v-if="loading" class="code-runner__loading">
      <Skeleton height="280px" />
    </div>

    <div v-else-if="!meta" class="code-runner__empty card">
      <p class="code-runner__empty-title">Mô phỏng không tồn tại</p>
      <p class="text-muted">Key '{{ key }}' không có trong danh mục 44 mô phỏng.</p>
      <Button variant="secondary" size="sm" @click="router.push({ name: 'simulations' })">
        Về danh mục
      </Button>
    </div>

    <template v-else>
      <div class="code-runner__layout">
        <!-- Trái: editor -->
        <section class="code-runner__panel code-runner__editor-panel" aria-label="Trình soạn mã">
          <header class="code-runner__panel-header">
            <span class="code-runner__panel-title">
              <Code2 :size="15" aria-hidden="true" />
              Editor
            </span>
            <Badge variant="muted">{{ key }}</Badge>
          </header>

          <div class="code-runner__editor-wrap">
            <div ref="gutterRef" class="code-runner__gutter" aria-hidden="true">
              <span v-for="line in gutterLines" :key="line" class="code-runner__gutter-line">
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
            * Monaco editor sẽ được bật khi cài gói <code>monaco-editor</code> (SDD Màn 16 — @monaco-editor/loader đã có).
          </p>
          <div class="code-runner__actions">
            <Button size="sm" :loading="codeStore.isRunning" @click="onRun">▶ Chạy (Ctrl+Enter)</Button>
            <Button variant="ghost" size="sm" @click="codeStore.restoreTemplate()">Khôi phục code mẫu</Button>
          </div>
        </section>

        <!-- Phải: output + canvas -->
        <section class="code-runner__panel code-runner__output-panel" aria-label="Kết quả chạy">
          <header class="code-runner__panel-header">
            <span class="code-runner__panel-title">
              <Terminal :size="15" aria-hidden="true" />
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
              :structure="simStore.currentStep?.structure ?? null"
              :empty-text="'Canvas 2 chiều — đồng bộ theo trace code của bạn'"
            />
            <StatsBar
              :comparisons="codeStore.lastStats?.comparisons ?? 0"
              :swaps="codeStore.lastStats?.swaps ?? 0"
              :writes="codeStore.lastStats?.writes ?? 0"
              :step="simStore.currentIndex"
              :total-steps="simStore.steps.length"
            />
            <div class="code-runner__sim-controls">
              <Button variant="ghost" size="sm" :disabled="simStore.isFirst" @click="simStore.stepBack()">◀</Button>
              <Button variant="ghost" size="sm" :disabled="simStore.isLast" @click="simStore.stepForward()">▶</Button>
              <span class="code-runner__step-info">
                Bước {{ simStore.currentIndex + 1 }}/{{ simStore.steps.length }}
              </span>
            </div>
          </div>
        </section>
      </div>

      <!-- Lịch sử nộp -->
      <section v-if="historyOpen" class="code-runner__history card">
        <h2 class="code-runner__history-title">Lịch sử nộp</h2>
        <p v-if="codeStore.submissions.length === 0" class="text-muted">
          Chưa có bài nộp — nộp từ Bậc 3 (Ladder) sẽ hiển thị ở đây.
        </p>
        <ul v-else class="code-runner__history-list">
          <li v-for="sub in codeStore.submissions" :key="sub.id">
            <Badge :variant="sub.status === 'passed' ? 'success' : 'danger'">{{ sub.status }}</Badge>
            <span class="text-muted">{{ sub.createdAt }}</span>
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

/* ── Chrome header — Cyber Mint nhẹ ── */
.code-runner__chrome {
  position: relative;
  isolation: isolate;
  overflow: hidden;
  border: 1px solid color-mix(in srgb, var(--color-primary) 28%, var(--color-border));
  border-radius: var(--radius-xl);
  background-image: var(--gradient-mint);
  padding: var(--space-lg) var(--space-xl);
  box-shadow: var(--shadow-md);
}

.code-runner__chrome::after {
  content: '';
  position: absolute;
  inset: 0;
  z-index: -1;
  background: color-mix(in srgb, var(--color-background) 62%, transparent);
}

.code-runner__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  margin-bottom: 4px;
}

.code-runner__breadcrumb a { color: var(--color-primary); font-weight: 600; }

.code-runner__header {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  align-items: flex-start;
  flex-wrap: wrap;
}

.code-runner__title {
  font-size: var(--text-2xl);
  background-image: var(--gradient-mint);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.code-runner__sub { font-size: var(--text-xs); margin-top: 4px; max-width: 48rem; color: var(--color-text-muted); }

.code-runner__loading { display: flex; flex-direction: column; gap: var(--space-md); }

.code-runner__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-md);
  padding: var(--space-2xl);
  text-align: center;
}

.code-runner__empty-title { font-size: var(--text-lg); font-weight: 700; }

.code-runner__layout {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-md);
  align-items: start;
}

/* ── Panel chung ── */
.code-runner__panel {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-surface);
  padding: var(--space-md);
  box-shadow: var(--shadow-sm);
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
  gap: 6px;
  font-size: var(--text-sm);
  font-weight: 700;
}

.code-runner__panel-title svg { color: var(--color-primary); }

/* ── Editor: gutter số dòng + textarea ── */
.code-runner__editor-wrap {
  display: flex;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  overflow: hidden;
  background: color-mix(in srgb, #0f172a 92%, transparent);
}

.code-runner__gutter {
  width: 42px;
  flex-shrink: 0;
  overflow: hidden;
  padding: 12px 8px 12px 0;
  background: color-mix(in srgb, #1e293b 85%, transparent);
  border-right: 1px solid color-mix(in srgb, #ffffff 8%, transparent);
  text-align: right;
  user-select: none;
}

.code-runner__gutter-line {
  display: block;
  font-family: var(--font-mono);
  font-size: 12px;
  line-height: 1.6;
  color: #64748b;
}

.code-runner__textarea {
  width: 100%;
  min-height: 320px;
  font-family: var(--font-mono);
  font-size: 13px;
  line-height: 1.6;
  border: none;
  outline: none;
  background: transparent;
  color: #e2e8f0;
  padding: 12px 14px;
  resize: vertical;
  tab-size: 2;
  caret-color: var(--color-primary);
}

.code-runner__textarea:focus { outline: none; }

.code-runner__textarea::selection { background: color-mix(in srgb, var(--color-primary) 32%, transparent); }

.code-runner__note { font-size: var(--text-xs); color: var(--color-text-muted); }
.code-runner__note code { font-family: var(--font-mono); }

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
  gap: 6px;
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
  color: var(--color-text-muted);
  background: var(--color-muted);
  border: 1px dashed var(--color-border);
}

.code-runner__output-box {
  background: var(--color-muted);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
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

.code-runner__sim-controls {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.code-runner__step-info { font-size: var(--text-xs); color: var(--color-text-muted); font-family: var(--font-mono); }

.code-runner__history-title { font-size: var(--text-md); margin-bottom: var(--space-sm); }

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

@media (max-width: 1000px) {
  .code-runner__layout { grid-template-columns: 1fr; }
}
</style>
