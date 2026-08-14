<script setup lang="ts">
// ManualPracticePanel — chế độ Tự thực hành (FR-3.12)
// Hộp chọn thao tác kế tiếp; so sánh với trace thật (bước kế) → đúng/sai
// UX: hướng dẫn khi chưa chạy mô phỏng · tự cuộn vào view khi panel xuất hiện ·
// tự sang bước kế sau khi trả lời (~400ms để feedback hiện rõ).
import { computed, nextTick, onBeforeUnmount, onMounted, ref } from 'vue';
import { Lightbulb } from 'lucide-vue-next';

import type { Step } from '@/engines/core/types';
import Button from '@/components/ui/Button.vue';
import { messages } from '@/i18n/vi';

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

/** Root element - cho SimulatorView scroll tới khi bật practice mode */
const rootEl = ref<HTMLElement | null>(null);
defineExpose({ rootEl });

/** Chưa chạy mô phỏng → không có trace để đoán bước kế → hiện hint thay vì radio vô nghĩa */
const isEmpty = computed(() => props.steps.length === 0);

/** Chưa chạy mô phỏng (chưa có bước nào) → hiện hướng dẫn thay cho vòng trống. */
const hasRun = computed(() => props.steps.length > 0);

/** Auto-scroll: khi panel xuất hiện (bật chế độ Tự thực hành) → cuộn mượt vào view. */
onMounted(async () => {
  await nextTick();
  // jsdom/test không có scrollIntoView → optional-call an toàn.
  rootEl.value?.scrollIntoView?.({ behavior: 'smooth', block: 'nearest' });
});

/** Các thao tác gợi ý (≈ 6) cho bước kế — label i18n theo key (messages.practice.options) */
type PracticeOptionKey = keyof typeof messages.practice.options;

const OPTIONS: PracticeOptionKey[] = ['compare', 'swap', 'assign', 'move', 'insert', 'delete'];

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
  if (!selected.value) return;
  const expected = inferExpected();
  if (selected.value === expected) {
    lastResult.value = 'correct';
    correctCount.value += 1;
  } else {
    lastResult.value = 'wrong';
    wrongCount.value += 1;
  }
  selected.value = '';
  // UX fix: sau khi kiểm tra (đúng/sai) → tự chuyển bước kế tiếp, không bắt user bấm "Bỏ qua".
  // Feedback ✓/✗ còn hiển thị (lastResult) + panel giải thích bên phải.
  emit('skip');
}

function skip(): void {
  lastResult.value = null;
  emit('skip');
}

function finish(): void {
  finished.value = true;
  emit('done', { correct: correctCount.value, wrong: wrongCount.value });
}
</script>

<template>
  <section
    ref="rootEl"
    class="practice card"
    :class="{ 'practice--active': !isEmpty }"
    :aria-label="messages.practice.ariaLabel"
  >
    <header class="practice__header">
      <h3 class="practice__title">{{ messages.practice.title }}</h3>
      <span class="practice__score">
        {{ messages.practice.score(correctCount, wrongCount) }}
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
      {{ messages.practice.finished(correctCount, wrongCount) }}
    </p>

    <p v-else-if="isEmpty" class="practice__empty" role="status">
      {{ messages.practice.emptyHint }}
    </p>

    <template v-else>
      <p class="practice__prompt">
        {{ messages.practice.prompt }}
        <span v-if="lastResult === 'correct'" class="practice__ok">{{ messages.practice.correct }}</span>
        <span v-else-if="lastResult === 'wrong'" class="practice__bad">{{ messages.practice.wrong }}</span>
      </p>

      <div class="practice__options" role="radiogroup">
        <label
          v-for="opt in OPTIONS"
          :key="opt"
          class="practice__option"
          :class="{ 'practice__option--selected': selected === opt }"
        >
          <input v-model="selected" type="radio" :value="opt" class="visually-hidden" />
          {{ messages.practice.options[opt] }}
        </label>
      </div>

      <div class="practice__actions">
        <Button variant="ghost" size="sm" @click="skip">{{ messages.practice.skip }}</Button>
        <Button size="sm" :disabled="!selected" @click="submit">{{ messages.practice.check }}</Button>
        <Button variant="secondary" size="sm" @click="finish">{{ messages.practice.finish }}</Button>
      </div>
    </template>
  </section>
</template>

<style scoped>
.practice { display: flex; flex-direction: column; gap: var(--space-md); }

/* Accent khi panel đang hoạt động (có trace thật) — border primary + bg nổi bật, token sẵn có */
.practice.practice--active {
  border: 2px solid var(--color-primary);
  background: var(--color-surface-hover);
}

/* Empty state — lời mời hành động (DESIGN §7.7): chưa chạy sim thì hướng dẫn chứ không hiện radio vô nghĩa */
.practice__empty {
  margin: 0;
  padding: var(--space-md);
  border: 1px dashed var(--color-border-strong);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  font-size: var(--text-sm);
  line-height: 1.55;
  color: var(--color-text-tertiary);
}

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
