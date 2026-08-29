<script setup lang="ts">
// CodeStage — Bậc 3: Code Challenge (Màn 16 — FR-9.1..9.3)
// Editor (textarea — Monaco sẽ bật khi cài gói monaco-editor) + chạy sandbox client +
// nộp (test ẩn hiện tên + kết quả sau nộp). Pass ≥ 70% → emit('passed').
import { computed, onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';

import { useCodeRunnerStore } from '@/stores/codeRunner';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';
import Skeleton from '@/components/ui/Skeleton.vue';

const props = withDefaults(
  defineProps<{
    simKey: string;
    exerciseId?: number | null;
    loading?: boolean;
  }>(),
  {
    exerciseId: null,
    loading: false,
  },
);

const emit = defineEmits<{
  passed: [];
  'view-theory': [];
}>();

const route = useRoute();
const ui = useUiStore();
const codeStore = useCodeRunnerStore();

/** Bài tập mở từ lớp học: /exercise/:id?classAssignmentId=... → nộp kèm để chấm theo bài gán. */
const classAssignmentId = computed<number | null>(() => {
  const raw = Number(route.query.classAssignmentId);
  return Number.isFinite(raw) && raw > 0 ? raw : null;
});

const submitResult = ref<{ passed: number; total: number; results: Array<{ testId: string; passed: boolean; message: string }> } | null>(null);
const submitting = ref(false);

onMounted(() => {
  if (codeStore.editorCode === '') {
    void codeStore.loadTemplate(props.simKey).catch(() => {
      /* loadTemplate không reject */
    });
  }
});

const isPassed = computed(() => {
  if (!submitResult.value || submitResult.value.total === 0) return false;
  return submitResult.value.passed / submitResult.value.total >= 0.7;
});

async function onSubmit(): Promise<void> {
  if (submitting.value) return;
  if (!props.exerciseId) {
    ui.showToast('Bài tập này chưa được gắn exercise — chỉ chạy thử.', 'warning');
    return;
  }
  submitting.value = true;
  try {
    const result = await codeStore.submit(props.exerciseId, classAssignmentId.value);
    submitResult.value = result;
    if (result.total > 0 && result.passed / result.total >= 0.7) {
      ui.showToast('🎉 Hoàn thành node! Điểm = Quiz 20% + Lab 30% + Code 50%.', 'success');
      emit('passed');
    }
  } catch (err) {
    ui.showToast(err instanceof Error ? err.message : 'Không thể nộp bài.', 'error');
  } finally {
    submitting.value = false;
  }
}
</script>

<template>
  <section class="code-stage">
    <header class="code-stage__header">
      <h2 class="code-stage__title">Code Challenge — {{ simKey }}</h2>
      <Badge variant="muted">Nộp bài: output đúng là đạt (không giới hạn cách viết)</Badge>
    </header>

    <div v-if="loading" class="code-stage__loading">
      <Skeleton height="240px" />
    </div>

    <div v-else class="code-stage__layout">
      <div class="code-stage__editor">
        <textarea
          v-model="codeStore.editorCode"
          class="code-stage__textarea"
          spellcheck="false"
          :aria-label="`Trình soạn mã ${simKey}`"
        />
        <p class="code-stage__note">
          * Trình soạn Monaco sẽ được bật khi cài gói <code>monaco-editor</code> (SDD Màn 16).
        </p>
        <div class="code-stage__actions">
          <Button size="sm" :loading="codeStore.isRunning" @click="codeStore.run()">▶ Chạy</Button>
          <Button variant="ghost" size="sm" @click="codeStore.restoreTemplate()">Khôi phục code mẫu</Button>
          <Button variant="secondary" size="sm" :loading="submitting" :disabled="!exerciseId" @click="onSubmit">
            Nộp bài
          </Button>
        </div>

        <!-- Thanh progress màu xám khi đang chạy code (F1) -->
        <div v-if="codeStore.isRunning" class="w-full h-1 bg-gray-700 rounded overflow-hidden mt-2">
          <div class="h-full bg-gray-400 animate-pulse w-full"></div>
        </div>

        <p v-if="codeStore.runError" class="code-stage__status code-stage__status--error" role="alert">
          {{ codeStore.runError }}
        </p>
        <p v-else-if="codeStore.lastStats" class="code-stage__status code-stage__status--ok" role="status">
          Thành công · {{ codeStore.lastStats.durationMs }}ms · so sánh {{ codeStore.lastStats.comparisons }} ·
          hoán đổi {{ codeStore.lastStats.swaps }}
        </p>
      </div>

      <div class="code-stage__output">
        <h3 class="code-stage__output-title">Kết quả chạy</h3>
        <pre v-if="codeStore.lastOutput !== null" class="code-stage__output-box">
{{ JSON.stringify(codeStore.lastOutput, null, 2) }}
        </pre>
        <p v-else class="code-stage__output-empty text-muted">Chưa có kết quả — bấm "Chạy".</p>

        <template v-if="submitResult">
          <h3 class="code-stage__output-title">Kết quả nộp (test ẩn)</h3>
          <ul class="code-stage__tests">
            <li
              v-for="test in submitResult.results"
              :key="test.testId"
              class="code-stage__test"
            >
              <Badge :variant="test.passed ? 'success' : 'danger'">
                {{ test.passed ? '✔' : '✘' }}
              </Badge>
              <span>{{ test.testId }}</span>
            </li>
          </ul>
          <p class="code-stage__score" :class="isPassed ? 'code-stage__score--pass' : 'code-stage__score--fail'">
            Đạt {{ submitResult.passed }}/{{ submitResult.total }} test (cần ≥ 70%)
          </p>
        </template>
      </div>
    </div>

    <Button v-if="exerciseId" variant="secondary" size="sm" @click="emit('view-theory')">
      Xem lại lý thuyết
    </Button>
  </section>
</template>

<style scoped>
.code-stage { display: flex; flex-direction: column; gap: var(--space-md); }

.code-stage__header { display: flex; align-items: center; justify-content: space-between; gap: var(--space-md); flex-wrap: wrap; }

.code-stage__title { font-size: var(--text-lg); }

.code-stage__layout { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-md); }

.code-stage__textarea {
  width: 100%;
  min-height: 260px;
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

.code-stage__textarea:focus { outline: 2px solid var(--color-primary); }

.code-stage__note { font-size: var(--text-xs); color: var(--color-text-muted); margin-top: 4px; }
.code-stage__note code { font-family: var(--font-mono); }

.code-stage__actions { display: flex; gap: var(--space-sm); margin-top: var(--space-sm); flex-wrap: wrap; }

.code-stage__status { font-size: var(--text-sm); margin-top: var(--space-sm); }
.code-stage__status--ok { color: var(--color-success); }
.code-stage__status--error { color: var(--color-destructive); }

.code-stage__output-title { font-size: var(--text-sm); margin-bottom: var(--space-sm); }

.code-stage__output-box {
  background: var(--color-muted);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
  font-size: var(--text-xs);
  overflow-x: auto;
  min-height: 60px;
}

.code-stage__output-empty { font-size: var(--text-sm); }

.code-stage__tests { list-style: none; display: flex; flex-direction: column; gap: 6px; margin-top: var(--space-md); }

.code-stage__test { display: flex; align-items: center; gap: var(--space-sm); font-size: var(--text-sm); font-family: var(--font-mono); }

.code-stage__score { margin-top: var(--space-sm); font-weight: 700; }
.code-stage__score--pass { color: var(--color-success); }
.code-stage__score--fail { color: var(--color-warning); }

@media (max-width: 1000px) {
  .code-stage__layout { grid-template-columns: 1fr; }
}
</style>
