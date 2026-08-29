<template>
  <div v-if="!frame" class="empty-state flex flex-col items-center justify-center w-full h-full gap-3 p-6 text-center">
    <div class="w-12 h-12 rounded-2xl flex items-center justify-center text-xl font-black text-accent bg-bg-surface border border-border-default shadow-lg animate-pulse">
      <BaseIcon name="zap" class="w-6 h-6" />
    </div>
    <p class="text-sm font-bold text-text-primary">Chưa có dữ liệu hoạt ảnh</p>
    <p class="text-xs text-text-secondary max-w-sm leading-relaxed">
      Hãy tạo mảng mới hoặc chọn một thuật toán để sinh khung hình. Nếu bạn nhập dữ liệu không hợp lệ, hãy kiểm tra lại định dạng.
    </p>
  </div>
  <BubbleSortVisualizer v-else-if="frame.algorithm === 'bubble'" :frame="frame" />
  <SelectionSortVisualizer v-else-if="frame.algorithm === 'selection'" :frame="frame" />
  <InsertionSortVisualizer v-else-if="frame.algorithm === 'insertion'" :frame="frame" />
  <QuickSortVisualizer v-else-if="frame.algorithm === 'quick'" :frame="frame" />
  <MergeSortVisualizer v-else-if="frame.algorithm === 'merge'" :frame="frame" />
  <BubbleSortVisualizer v-else-if="frame.algorithm === 'heap' || frame.algorithm === 'radix' || frame.algorithm === 'counting' || frame.algorithm === 'bucket'" :frame="frame" />
  <div v-else class="empty-state flex flex-col items-center justify-center w-full h-full gap-3 p-6 text-center">
    <p class="algo-error-title text-sm font-bold">Không nhận diện được thuật toán "{{ frame.algorithm }}"</p>
    <p class="text-xs text-text-secondary">Dữ liệu khung hình không hợp lệ. Hãy tạo lại mảng dữ liệu.</p>
  </div>
</template>

<script setup lang="ts">
import type { SortFrame } from '../types/sorting.types';
import BubbleSortVisualizer from './BubbleSortVisualizer.vue';
import SelectionSortVisualizer from './SelectionSortVisualizer.vue';
import InsertionSortVisualizer from './InsertionSortVisualizer.vue';
import QuickSortVisualizer from './QuickSortVisualizer.vue';
import MergeSortVisualizer from './MergeSortVisualizer.vue';
import BaseIcon from '@/shared/components/BaseIcon.vue';

defineProps<{
  frame: SortFrame | null;
}>();
</script>

<style scoped>
.empty-state {
  background-color: color-mix(in srgb, var(--color-bg-secondary) 60%, transparent);
}

.empty-state .algo-error-title {
  color: var(--color-accent-red);
}
</style>
