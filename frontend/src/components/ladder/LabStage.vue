<script setup lang="ts">
// LabStage — Bậc 2: Interactive Lab (Màn 15 — FR-4.3, chấm trạng thái cuối + bước ≤ chuẩn × 1.5)
// Kịch bản "Sắp xếp": người học hoán đổi các ô liền kề tới khi mảng sắp xếp.
// Hoàn tác/Làm lại không tính bộ đếm. Nộp → so trạng thái cuối + bước.
import { computed, ref } from 'vue';

import { useUiStore } from '@/stores/ui';
import { fireConfetti } from '@/composables/useConfetti';
import Button from '@/components/ui/Button.vue';
import Badge from '@/components/ui/Badge.vue';

const props = withDefaults(
  defineProps<{
    title?: string;
    initialArray?: number[];
    /** Giới hạn bước = chuẩn × 1.5 (làm tròn lên) */
    maxSteps?: number;
    /** Số bước chuẩn để giải (bubble sort worst case n²) */
    standardSteps?: number;
    prompt?: string;
  }>(),
  {
    title: 'Interactive Lab — Sắp xếp',
    initialArray: () => [5, 3, 8, 1, 9, 2],
    maxSteps: 0,
    standardSteps: 0,
    prompt: 'Sắp xếp dãy tăng dần bằng cách hoán đổi 2 ô liền kề (chọn ô 1 rồi ô 2).',
  },
);

const emit = defineEmits<{
  passed: [];
  'view-theory': [];
}>();

const ui = useUiStore();

const array = ref<number[]>([...props.initialArray]);
const history = ref<number[][]>([]);
const stepsUsed = ref(0);
const selectedIndex = ref<number | null>(null);
const feedback = ref('');
const submitted = ref(false);
const won = ref(false);

const sorted = computed(() => [...array.value].sort((a, b) => a - b));
const isSorted = computed(() => array.value.every((v, i) => v === sorted.value[i]));
const limit = computed(() => props.maxSteps || Math.ceil(props.standardSteps * 1.5) || 12);

function clickCell(index: number): void {
  if (submitted.value || won.value) return;
  if (selectedIndex.value === null) {
    selectedIndex.value = index;
    feedback.value = '';
    return;
  }
  const first = selectedIndex.value;
  if (first === index) {
    selectedIndex.value = null;
    return;
  }
  if (Math.abs(first - index) !== 1) {
    feedback.value = 'Chỉ được hoán đổi 2 ô LIỀN KỀ.';
    selectedIndex.value = null;
    return;
  }
  const next = [...array.value];
  [next[first], next[index]] = [next[index], next[first]];
  history.value.push([...array.value]);
  array.value = next;
  stepsUsed.value += 1;
  selectedIndex.value = null;

  if (isSorted.value) {
    feedback.value = 'Mảng đã sắp xếp! Bạn có thể nộp bài.';
  } else if (stepsUsed.value >= limit.value) {
    feedback.value = `Đã dùng hết ${limit.value} bước cho phép — hãy làm lại.`;
  }
}

function undo(): void {
  const previous = history.value.pop();
  if (previous) {
    array.value = previous;
    if (stepsUsed.value > 0) stepsUsed.value -= 1;
  }
}

function resetLab(): void {
  array.value = [...props.initialArray];
  history.value = [];
  stepsUsed.value = 0;
  selectedIndex.value = null;
  feedback.value = '';
  submitted.value = false;
  won.value = false;
}

function submit(): void {
  submitted.value = true;
  if (isSorted.value && stepsUsed.value <= limit.value) {
    won.value = true;
    ui.showToast('🎉 Chúc mừng qua Bậc 2!', 'success');
    fireConfetti('node-pass'); // hoàn thành stage → pháo mint-teal (G-F2c)
    emit('passed');
  } else {
    feedback.value = isSorted.value
      ? `Sai giới hạn bước: dùng ${stepsUsed.value}/${limit.value}.`
      : 'Trạng thái cuối chưa đúng — mảng chưa được sắp xếp.';
  }
}
</script>

<template>
  <section class="lab-stage">
    <header class="lab-stage__header">
      <div>
        <h2 class="lab-stage__title">{{ title }}</h2>
        <p class="lab-stage__prompt">{{ prompt }}</p>
      </div>
      <Badge :variant="stepsUsed <= limit ? 'primary' : 'danger'">
        Đã dùng {{ stepsUsed }}/{{ limit }} bước
      </Badge>
    </header>

    <div class="lab-stage__canvas">
      <button
        v-for="(value, idx) in array"
        :key="idx"
        type="button"
        class="lab-stage__cell"
        :class="{
          'lab-stage__cell--selected': selectedIndex === idx,
          'lab-stage__cell--done': isSorted,
        }"
        :disabled="submitted || won"
        @click="clickCell(idx)"
      >
        {{ value }}
        <small class="lab-stage__cell-idx">{{ idx }}</small>
      </button>
    </div>

    <p v-if="feedback" class="lab-stage__feedback" role="status">{{ feedback }}</p>

    <div class="lab-stage__controls">
      <Button variant="ghost" size="sm" :disabled="history.length === 0" @click="undo">
        Hoàn tác (Ctrl+Z)
      </Button>
      <Button variant="ghost" size="sm" @click="resetLab">Làm lại</Button>
      <Button variant="secondary" size="sm" @click="emit('view-theory')">Xem lại lý thuyết</Button>
      <Button size="sm" :disabled="submitted || won || history.length === 0" @click="submit">
        Nộp bài
      </Button>
    </div>

    <p v-if="won" class="lab-stage__win" role="status">
      🎉 Đã hoàn thành đúng trạng thái cuối trong {{ stepsUsed }} bước — qua Bậc 2!
    </p>
    <p v-else-if="submitted" class="lab-stage__fail" role="status">
      Chưa đạt — làm lại trong phiên (miễn phí, 19.2).
    </p>
  </section>
</template>

<style scoped>
.lab-stage { display: flex; flex-direction: column; gap: var(--space-lg); }

.lab-stage__header {
  display: flex;
  justify-content: space-between;
  gap: var(--space-md);
  flex-wrap: wrap;
  align-items: flex-start;
}

.lab-stage__title {
  font-size: var(--text-lg);
  background-image: var(--gradient-mint);
  -webkit-background-clip: text;
  background-clip: text;
  color: transparent;
}

.lab-stage__prompt { color: var(--color-text-muted); font-size: var(--text-sm); margin-top: 4px; }

.lab-stage__canvas {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
  padding: var(--space-lg);
  border: 2px dashed color-mix(in srgb, var(--color-primary) 35%, var(--color-border));
  border-radius: var(--radius-lg);
  background: color-mix(in srgb, var(--color-background) 55%, var(--color-muted));
  min-height: 140px;
  align-items: center;
  justify-content: center;
  transition: border-color 200ms ease, background 200ms ease;
}

.lab-stage__canvas:has(.lab-stage__cell--done) {
  border-color: color-mix(in srgb, var(--color-success) 55%, var(--color-border));
}

.lab-stage__cell {
  width: 64px;
  height: 64px;
  border: 2px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  font-size: var(--text-lg);
  font-weight: 800;
  color: var(--color-foreground);
  cursor: pointer;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 2px;
  transition: var(--transition-fast);
  box-shadow: var(--shadow-sm);
}

.lab-stage__cell:hover:not(:disabled) {
  border-color: var(--color-primary);
  transform: translateY(-2px);
  box-shadow: var(--shadow-md);
}

.lab-stage__cell--selected {
  border-color: var(--color-warning);
  background: color-mix(in srgb, var(--color-warning) 14%, transparent);
  transform: translateY(-2px);
}

.lab-stage__cell--done {
  border-color: transparent;
  background-image: var(--gradient-mint);
  color: var(--color-on-primary);
  box-shadow: var(--shadow-md);
}

.lab-stage__cell--done .lab-stage__cell-idx { color: color-mix(in srgb, var(--color-on-primary) 80%, transparent); }

.lab-stage__cell-idx { font-size: 10px; color: var(--color-text-muted); }

.lab-stage__feedback {
  font-size: var(--text-sm);
  color: var(--color-warning);
  background: color-mix(in srgb, var(--color-warning) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-warning) 30%, transparent);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
}

.lab-stage__controls { display: flex; gap: var(--space-sm); flex-wrap: wrap; }

.lab-stage__win {
  color: var(--color-success);
  font-weight: 700;
  background: color-mix(in srgb, var(--color-success) 10%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-success) 30%, transparent);
  border-radius: var(--radius-md);
  padding: var(--space-sm) var(--space-md);
}

.lab-stage__fail { color: var(--color-warning); font-weight: 600; }
</style>
