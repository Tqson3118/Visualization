<script setup lang="ts">
// MiniQuizBanner — câu hỏi kiểm tra nhanh sau mô phỏng (FR-3.16)
// 1-2 câu hỏi sinh từ dữ liệu vừa chạy; chấm ngay tại chỗ, không ảnh hưởng điểm chính thức.
import { computed, ref } from 'vue';

import type { Step } from '@/engines/core/types';
import Button from '@/components/ui/Button.vue';

const props = defineProps<{
  steps: Step[];
  simKey?: string;
}>();

const emit = defineEmits<{
  'view-theory': [];
  restart: [];
}>();

const answer = ref<number | null>(null);
const checked = ref(false);

/** Sinh câu hỏi ngẫu nhiên từ mảng đầu vào (mảng → hỏi kết quả cuối) */
const question = computed(() => {
  const first = props.steps[0];
  const last = props.steps[props.steps.length - 1];
  if (!first || !last) return null;
  const initial = first.structure.elements.map((el) => el.label).join(', ');
  const final = last.structure.elements.map((el) => el.label).join(', ');
  return {
    prompt: `Dãy ban đầu là [${initial}]. Theo mô phỏng vừa xem, dãy cuối cùng là gì?`,
    choices: [final, initial, `${final}, ${initial}`.slice(0, 40), 'Không đổi'],
    correctIndex: 0,
  };
});

const isCorrect = computed(() => checked.value && answer.value === question.value?.correctIndex);

function check(): void {
  checked.value = true;
}
</script>

<template>
  <section v-if="question && !checked" class="mini-quiz card" aria-label="Kiểm tra nhanh">
    <h3 class="mini-quiz__title">💡 Kiểm tra nhanh</h3>
    <p class="mini-quiz__prompt">{{ question.prompt }}</p>
    <div class="mini-quiz__options">
      <label
        v-for="(choice, idx) in question.choices"
        :key="idx"
        class="mini-quiz__option"
        :class="{ 'mini-quiz__option--selected': answer === idx }"
      >
        <input v-model="answer" type="radio" :value="idx" class="visually-hidden" />
        {{ choice }}
      </label>
    </div>
    <div class="mini-quiz__actions">
      <Button size="sm" :disabled="answer === null" @click="check">Trả lời</Button>
      <Button variant="ghost" size="sm" @click="emit('view-theory')">Xem lý thuyết</Button>
    </div>
  </section>

  <section v-else-if="question && checked" class="mini-quiz card" role="status">
    <p class="mini-quiz__prompt" :class="isCorrect ? 'mini-quiz__correct' : 'mini-quiz__wrong'">
      {{ isCorrect ? '✓ Đúng rồi! Bạn đã nắm được ý tưởng thuật toán.' : `✗ Chưa đúng — đáp án: ${question.choices[question.correctIndex]}. Xem lại lý thuyết rồi thử lại nhé.` }}
    </p>
    <div class="mini-quiz__actions">
      <Button size="sm" @click="emit('restart')">Làm lại mô phỏng</Button>
      <Button variant="ghost" size="sm" @click="emit('view-theory')">Xem lý thuyết</Button>
    </div>
  </section>
</template>

<style scoped>
.mini-quiz { display: flex; flex-direction: column; gap: var(--space-md); border-color: var(--color-secondary); }

.mini-quiz__title { font-size: var(--text-md); }

.mini-quiz__prompt { font-size: var(--text-sm); }

.mini-quiz__correct { color: var(--color-success); font-weight: 600; }
.mini-quiz__wrong { color: var(--color-destructive); font-weight: 600; }

.mini-quiz__options { display: grid; grid-template-columns: 1fr 1fr; gap: var(--space-sm); }

.mini-quiz__option {
  padding: var(--space-sm);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  cursor: pointer;
  font-size: var(--text-sm);
}

.mini-quiz__option--selected { border-color: var(--color-primary); background: var(--color-surface-hover); }

.mini-quiz__actions { display: flex; gap: var(--space-sm); }
</style>
