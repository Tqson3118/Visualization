<script setup lang="ts">
// ManualPracticePanel — chế độ Tự thực hành (FR-3.12)
// Hộp chọn thao tác kế tiếp; so sánh với trace thật (bước kế) → đúng/sai
// UX: hướng dẫn khi chưa chạy mô phỏng · tự cuộn vào view khi panel xuất hiện ·
// tự sang bước kế sau khi trả lời (~400ms để feedback hiện rõ).
import { computed, nextTick, onBeforeUnmount, onMounted, ref } from 'vue';
import { Lightbulb } from 'lucide-vue-next';

import type { Step } from '@/engines/core/types';
import Button from '@/components/ui/Button.vue';

const props = defineProps<{
  steps: Step[];
  currentIndex: number;
}>();

const emit = defineEmits<{
  skip: [];
  done: [result: { correct: number; wrong: number }];
}>();

const selected = ref<string>('');
const lastResult = ref<'correct' | 'wrong' | null>(null);
const correctCount = ref(0);
const wrongCount = ref(0);
const finished = ref(false);

/** Chưa chạy mô phỏng (chưa có bước nào) → hiện hướng dẫn thay cho vùng trống. */
const hasRun = computed(() => props.steps.length > 0);

const panelEl = ref<HTMLElement | null>(null);

/** Auto-scroll: khi panel xuất hiện (bật chế độ Tự thực hành) → cuộn mượt vào view. */
onMounted(async () => {
  await nextTick();
  panelEl.value?.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
});

/** Timer tự động sang bước kế sau khi trả lời (feedback hiện rõ ~400ms). */
let advanceTimer: ReturnType<typeof setTimeout> | null = null;

function clearAdvanceTimer(): void {
  if (advanceTimer !== null) {
    clearTimeout(advanceTimer);
    advanceTimer = null;
  }
}

onBeforeUnmount(clearAdvanceTimer);

/** Các thao tác gợi ý (≤ 6) cho bước kế */
const OPTIONS = [
  { key: 'compare', label: 'So sánh hai phần tử' },
  { key: 'swap', label: 'Hoán đổi hai phần tử' },
  { key: 'assign', label: 'Gán giá trị' },
  { key: 'move', label: 'Di chuyển con trỏ' },
  { key: 'insert', label: 'Chèn phần tử' },
  { key: 'delete', label: 'Xóa phần tử' },
];

const nextStep = computed<Step | null>(() => props.steps[props.currentIndex + 1] ?? null);

/** Suy thao tác đúng từ bước kế (dựa vào explanation/structure trạng thái) */
function inferExpected(): string {
  const step = nextStep.value;
  if (!step) return 'finished';
  const text = step.explanation.toLowerCase();
  if (text.includes('hoán') || text.includes('swap')) return 'swap';
  if (text.includes('so sánh') || text.includes('compare')) return 'compare';
  if (text.includes('gán') || text.includes('chèn') || text.includes('đẩy') || text.includes('enqueue')) return 'insert';
  if (text.includes('xóa') || text.includes('pop') || text.includes('dequeue')) return 'delete';
  if (text.includes('con trỏ') || text.includes('di chuyển')) return 'move';
  return 'assign';
}

function submit(): void {
  if (!selected.value || advanceTimer !== null) return;
  const expected = inferExpected();
  if (selected.value === expected) {
    lastResult.value = 'correct';
    correctCount.value += 1;
  } else {
    lastResult.value = 'wrong';
    wrongCount.value += 1;
  }
  selected.value = '';

  // Tự sang bước kế sau ~400ms — cùng hàm stepForward mà nút "Bước tới" dùng (parent @skip="stepForward").
  advanceTimer = setTimeout(() => {
    advanceTimer = null;
    lastResult.value = null;
    emit('skip');
  }, 400);
}

function skip(): void {
  clearAdvanceTimer();
  lastResult.value = null;
  emit('skip');
}

function finish(): void {
  clearAdvanceTimer();
  finished.value = true;
  emit('done', { correct: correctCount.value, wrong: wrongCount.value });
}
</script>

<template>
  <section ref="panelEl" class="practice card" aria-label="Tự thực hành">
    <header class="practice__header">
      <h3 class="practice__title">Tự thực hành</h3>
      <span class="practice__score">
        Đúng {{ correctCount }} · Sai {{ wrongCount }}
      </span>
    </header>

    <div v-if="!hasRun" class="practice__guidance" role="status">
      <span class="practice__guidance-icon" aria-hidden="true">
        <Lightbulb :size="18" />
      </span>
      <p class="practice__guidance-text">
        Chọn nút ▶ trên thanh điều khiển để chạy mô phỏng — sau đó trả lời thao tác kế tiếp của từng bước.
      </p>
    </div>

    <p v-else-if="finished" class="practice__result" role="status">
      Kết thúc luyện tập: {{ correctCount }} đúng / {{ wrongCount }} sai
    </p>

    <template v-else>
      <p class="practice__prompt">
        Bước kế tiếp của thuật toán là gì?
        <span v-if="lastResult === 'correct'" class="practice__ok">✓ Chính xác!</span>
        <span v-else-if="lastResult === 'wrong'" class="practice__bad">✗ Chưa đúng — xem giải thích ở panel bên.</span>
      </p>

      <div class="practice__options" role="radiogroup">
        <label
          v-for="opt in OPTIONS"
          :key="opt.key"
          class="practice__option"
          :class="{ 'practice__option--selected': selected === opt.key }"
        >
          <input v-model="selected" type="radio" :value="opt.key" class="visually-hidden" />
          {{ opt.label }}
        </label>
      </div>

      <div class="practice__actions">
        <Button variant="ghost" size="sm" @click="skip">Bỏ qua bước</Button>
        <Button size="sm" :disabled="!selected" @click="submit">Kiểm tra</Button>
        <Button variant="secondary" size="sm" @click="finish">Kết thúc</Button>
      </div>
    </template>
  </section>
</template>

<style scoped>
.practice { display: flex; flex-direction: column; gap: var(--space-md); }

.practice__header { display: flex; justify-content: space-between; align-items: center; }

.practice__title { font-size: var(--text-md); }

.practice__score { font-size: var(--text-sm); color: var(--color-text-muted); }

/* Hướng dẫn khi chưa chạy mô phỏng (thay vùng trống) */
.practice__guidance {
  display: flex;
  align-items: flex-start;
  gap: var(--space-sm);
  padding: var(--space-md);
  border: 1px dashed var(--color-border-strong);
  border-radius: var(--radius-md);
  background: var(--color-surface-hover);
  color: var(--color-text-secondary);
}

.practice__guidance-icon {
  color: var(--color-primary);
  flex-shrink: 0;
  margin-top: 2px;
}

.practice__guidance-text {
  margin: 0;
  font-size: var(--text-sm);
  line-height: 1.5;
}

.practice__prompt { font-size: var(--text-sm); }

.practice__ok { color: var(--color-success); font-weight: 700; }
.practice__bad { color: var(--color-destructive); font-weight: 700; }

.practice__options { display: grid; grid-template-columns: repeat(auto-fill, minmax(160px, 1fr)); gap: var(--space-sm); }

.practice__option {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-sm);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  cursor: pointer;
  font-size: var(--text-sm);
  transition: var(--transition-fast);
}

.practice__option--selected { border-color: var(--color-primary); background: var(--color-surface-hover); }

.practice__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; }
</style>
