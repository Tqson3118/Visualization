<template>
  <div
    class="h-full w-full flex items-end justify-center px-4 pb-6"
    :style="containerStyle"
  >
    <transition-group
      name="sort-list"
      tag="div"
      class="flex items-end h-full w-full justify-center"
      :style="{ gap: itemGap }"
    >
      <div
        v-for="(item, idx) in frame?.arrayStateWithIds || []"
        :key="item.id"
        class="relative flex flex-col items-center justify-end shrink-0 transition-all duration-300 h-full"
        :style="{ width: barWidth }"
      >
        <!-- Badge MIN: neo tuyệt đối ngay trên bar — không chiếm layout -->
        <span
          v-if="minIdx === idx"
          class="vis-min-badge"
          :style="{ bottom: badgeBottom(item.value) }"
        >▼ MIN</span>
        <div
          class="w-full flex items-center justify-center rounded-xl border font-bold select-none transition-all duration-300"
          :class="getItemClass(idx)"
          :style="{
            height:   barHeightPct(item.value) + '%',
            minHeight: '32px',
            fontSize:  itemFontSize,
          }"
        >
          {{ item.value }}
        </div>
        <div
          v-if="itemCount <= 12"
          class="mt-1 font-mono font-bold shrink-0"
          :style="{ fontSize: indexFontSize }"
          :class="getIndexClass(idx)"
        >
          [{{ idx }}]
        </div>
      </div>
    </transition-group>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { SortFrame } from '../types/sorting.types';

const props = defineProps<{
  frame: SortFrame | null;
}>();

const itemCount = computed(() => props.frame?.arrayStateWithIds?.length ?? 6);

const barWidth = computed(() => {
  const n = itemCount.value;
  if (n <= 8)  return '88px';
  if (n <= 12) return '72px';
  if (n <= 18) return '52px';
  return '36px';
});

const itemGap = computed(() => {
  const n = itemCount.value;
  if (n <= 8)  return '18px';
  if (n <= 12) return '12px';
  if (n <= 18) return '8px';
  return '6px';
});

const itemFontSize  = computed(() => itemCount.value <= 10 ? '14px' : itemCount.value <= 16 ? '11px' : '10px');
const indexFontSize = computed(() => itemCount.value <= 12 ? '10px' : '9px');

// Vị trí "min" đang theo dõi — highlight riêng khỏi bar đang quét
const minIdx = computed<number | null>(() => {
  const v = props.frame?.selectionMinIndex;
  return typeof v === 'number' ? v : null;
});

const maxVal = computed(() => {
  if (!props.frame?.arrayState?.length) return 1;
  return Math.max(...props.frame.arrayState, 1);
});

function barHeightPct(value: number): number {
  const span = Math.max(maxVal.value, 1);
  const ratio = Math.max(0, value / span);
  return Math.round(8 + ratio * 80);
}

// Badge nằm ngay trên đỉnh bar (cộng chỗ chỉ số phía dưới); chặn không vượt quá 72% để
// không tràn khỏi khung khi bar cao.
function badgeBottom(value: number): string {
  return `calc(min(${barHeightPct(value)}%, 72%) + 22px)`;
}

const containerStyle = computed(() => {
  const barW = parseInt(barWidth.value);
  const gapW = parseInt(itemGap.value);
  const minW = itemCount.value * barW + (itemCount.value - 1) * gapW + 32;
  return { minWidth: `${minW}px` };
});

function getItemClass(idx: number) {
  if (!props.frame) return 'vis-bar-default';
  const { comparingIndices, swappedIndices, sortedIndices } = props.frame;

  if (sortedIndices.includes(idx))
    return 'vis-bar-sorted';
  if (swappedIndices?.includes(idx))
    return 'vis-bar-swapped';
  if (minIdx.value === idx)
    return 'vis-bar-selection-min';
  if (comparingIndices?.includes(idx))
    return 'vis-bar-comparing';
  return 'vis-bar-default';
}

function getIndexClass(idx: number) {
  if (!props.frame) return 'vis-index-default';
  const { comparingIndices, swappedIndices, sortedIndices } = props.frame;
  if (sortedIndices.includes(idx))     return 'vis-index-sorted';
  if (swappedIndices?.includes(idx))   return 'vis-index-swapped';
  if (minIdx.value === idx)            return 'vis-index-selection-min';
  if (comparingIndices?.includes(idx)) return 'vis-index-comparing';
  return 'vis-index-default';
}
</script>

<style scoped>
.sort-list-move {
  transition: transform 0.4s cubic-bezier(0.25, 0.8, 0.25, 1);
}
</style>

<style>
/* Badge "MIN" — neo ngay trên bar min, nhịp đập nhẹ để dễ nhận diện */
.vis-min-badge {
  position: absolute;
  left: 0;
  right: 0;
  margin: 0 auto;
  width: max-content;
  z-index: 5;
  display: inline-block;
  padding: 1px 7px;
  border-radius: 4px;
  font-size: 8px;
  font-weight: 900;
  letter-spacing: 0.06em;
  color: #f5d0fe;
  background: linear-gradient(135deg, #a21caf, #7e22ce);
  border: 1px solid #d946ef;
  box-shadow: 0 0 10px rgba(217, 70, 239, 0.5);
  animation: min-bob 0.9s ease-in-out infinite;
  white-space: nowrap;
  transition: bottom 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
}
.vis-index-selection-min { color: #e879f9; }

/* Bar "min" — hồng/tím, phân biệt với bar đang quét (cam) */
.vis-bar-selection-min {
  border-color: #e879f9;
  background: linear-gradient(to top, rgba(217, 70, 239, 0.35), rgba(147, 51, 234, 0.12));
  color: #f5d0fe;
  box-shadow: 0 0 22px rgba(217, 70, 239, 0.55), inset 0 0 0 1px rgba(217, 70, 239, 0.35);
}

@keyframes min-bob {
  0%, 100% { transform: translateY(0); }
  50%      { transform: translateY(-3px); }
}
</style>