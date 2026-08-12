<script setup lang="ts">
// CodeRunnerView — Màn 16: Code Runner (Module I) tại /code/:key
// Editor (textarea — Monaco sẽ bật khi cài gói monaco-editor) + chạy sandbox +
// canvas 2 chiều từ generator thật + panel test ẩn sau nộp + lịch sử nộp.
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import { useCodeRunnerStore } from '@/stores/codeRunner';
import { useSimulationStore } from '@/stores/simulation';
import { useUiStore } from '@/stores/ui';
import { getCatalogMeta } from '@/engines/catalog';
import CanvasArea from '@/components/simulator/CanvasArea.vue';
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

const key = computed(() => String(route.params.key ?? ''));
const loading = ref(true);
const error = ref('');

const meta = computed(() => getCatalogMeta(key.value));

const historyOpen = ref(false);

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
    <header class="code-runner__header">
      <div>
        <nav class="code-runner__breadcrumb" aria-label="Breadcrumb">
          <RouterLink :to="{ name: 'simulations' }">Khám phá</RouterLink>
          <span aria-hidden="true">/</span>
          <span>{{ meta?.title ?? key }}</span>
        </nav>
        <h1 class="code-runner__title">💻 {{ meta?.title ?? key }} — Code Challenge</h1>
        <p class="code-runner__sub text-muted">
          Nộp bài: output đúng là đạt. Dùng hàm có sẵn (VD sort()) → vẫn đạt nhưng KHÔNG xem được mô phỏng bước.
        </p>
      </div>
      <Button variant="ghost" size="sm" @click="toggleHistory">
        {{ historyOpen ? 'Ẩn lịch sử' : 'Lịch sử nộp' }}
      </Button>
    </header>

    <div v-if="loading" class="code-runner__loading">
      <Skeleton height="280px" />
    </div>

    <EmptyState
      v-else-if="!meta"
      icon="package"
      title="Mô phỏng không tồn tại"
      :description="`Key '${key}' không có trong danh mục 44 mô phỏng.`"
      action-label="Về danh mục"
      @action="router.push({ name: 'simulations' })"
    />

    <template v-else>
      <div class="code-runner__layout">
        <!-- Trái: editor -->
        <div class="code-runner__editor">
          <textarea
            v-model="codeStore.editorCode"
            class="code-runner__textarea"
            spellcheck="false"
            :aria-label="`Trình soạn mã ${key}`"
          />
          <p class="code-runner__note">
            * Monaco editor sẽ được bật khi cài gói <code>monaco-editor</code> (SDD Màn 16 — @monaco-editor/loader đã có).
          </p>
          <div class="code-runner__actions">
            <Button size="sm" :loading="codeStore.isRunning" @click="onRun">▶ Chạy (Ctrl+Enter)</Button>
            <Button variant="ghost" size="sm" @click="codeStore.restoreTemplate()">Khôi phục code mẫu</Button>
          </div>
          <p v-if="codeStore.runError" class="code-runner__status code-runner__status--error" role="alert">
            {{ codeStore.runError }}
          </p>
          <p v-else-if="codeStore.lastStats" class="code-runner__status code-runner__status--ok" role="status">
            Thành công · {{ codeStore.lastStats.durationMs }}ms
          </p>
        </div>

        <!-- Phải: canvas + stats -->
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
            <span class="code-runner__step-info text-muted">
              Bước {{ simStore.currentIndex + 1 }}/{{ simStore.steps.length }}
            </span>
          </div>
        </div>
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

.code-runner__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  margin-bottom: 4px;
}

.code-runner__header {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  align-items: flex-start;
  flex-wrap: wrap;
}

.code-runner__title { font-size: var(--text-xl); }
.code-runner__sub { font-size: var(--text-xs); margin-top: 4px; max-width: 48rem; }

.code-runner__layout {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: var(--space-md);
  align-items: start;
}

.code-runner__editor { display: flex; flex-direction: column; gap: var(--space-sm); }

.code-runner__textarea {
  width: 100%;
  min-height: 300px;
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: #0f172a;
  color: #e2e8f0;
  padding: var(--space-md);
  resize: vertical;
  line-height: 1.5;
}

.code-runner__textarea:focus { outline: 2px solid var(--color-primary); }

.code-runner__note { font-size: var(--text-xs); color: var(--color-text-muted); }
.code-runner__note code { font-family: var(--font-mono); }

.code-runner__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

.code-runner__status { font-size: var(--text-sm); }
.code-runner__status--ok { color: var(--color-success); }
.code-runner__status--error { color: var(--color-destructive); }

.code-runner__visual { display: flex; flex-direction: column; gap: var(--space-sm); }

.code-runner__sim-controls {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.code-runner__step-info { font-size: var(--text-xs); }

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
