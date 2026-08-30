<template>
  <div
    class="h-full w-full flex items-end justify-center px-4 pb-6"
    :style="containerStyle"
  >
    <div class="relative flex h-full">
      <transition-group
        name="sort-list"
        tag="div"
        class="flex items-end h-full justify-center"
        :style="{ gap: itemGap }"
      >
        <div
          v-for="(item, idx) in frame?.arrayStateWithIds || []"
          :key="item.id"
          class="flex flex-col items-center justify-end shrink-0 transition-all duration-300 h-full"
          :style="{ width: barWidth }"
        >
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

      <!-- KEY đang cầm: chip gọn, neo theo vị trí gap (absolute — không phá layout) -->
      <div
        v-if="gapIdx !== null"
        class="vis-key-chip"
        :style="chipStyle"
        :data-testid="'key-chip'"
      >
        <span class="vis-key-label">KEY</span>{{ keyValue }}
      </div>
    </div>
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

// Slot trống đang chờ key hạ xuống — null = không ở pha cầm key
const gapIdx = computed<number | null>(() => {
  const v = props.frame?.insertionGapIndex;
  return typeof v === 'number' ? v : null;
});

const keyValue = computed<string | number>(() => {
  const k = props.frame?.variables?.key;
  return typeof k === 'string' || typeof k === 'number' ? k : '?';
});

// Chip key nằm trên đúng cột gap: left = vị trí cột + nửa bar
const chipStyle = computed(() => {
  const barW = parseInt(barWidth.value);
  const gapW = parseInt(itemGap.value);
  const idx = gapIdx.value ?? 0;
  const left = idx * (barW + gapW) + barW / 2;
  return {
    left: `${left}px`,
    fontSize: itemFontSize.value,
  };
});

function isGapSlot(idx: number): boolean {
  return gapIdx.value === idx;
}

const maxVal = computed(() => {
  if (!props.frame?.arrayState?.length) return 1;
  return Math.max(...props.frame.arrayState, 1);
});

function barHeightPct(value: number): number {
  const span = Math.max(maxVal.value, 1);
  const ratio = Math.max(0, value / span);
  return Math.round(8 + ratio * 80);
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

  // Slot gap luôn hiển thị "ô trống chờ key" (ưu tiên hơn swapped để không bị nhấp nháy)
  if (isGapSlot(idx))
    return 'vis-bar-gap';
  if (sortedIndices.includes(idx))
    return 'vis-bar-sorted';
  if (swappedIndices?.includes(idx))
    return 'vis-bar-swapped';
  if (comparingIndices?.includes(idx))
    return 'vis-bar-comparing';
  return 'vis-bar-default';
}

function getIndexClass(idx: number) {
  if (!props.frame) return 'vis-index-default';
  const { comparingIndices, swappedIndices, sortedIndices } = props.frame;
  if (isGapSlot(idx))                  return 'vis-index-gap';
  if (sortedIndices.includes(idx))     return 'vis-index-sorted';
  if (swappedIndices?.includes(idx))   return 'vis-index-swapped';
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
/* Chip KEY — gọn, neo trên slot gap, trượt ngang mượt khi gap di chuyển */
.vis-key-chip {
  position: absolute;
  top: 4px;
  transform: translateX(-50%);
  z-index: 5;
  display: inline-flex;
  align-items: center;
  gap: 4px;
  padding: 2px 10px;
  border-radius: 999px;
  font-weight: 900;
  color: #fff;
  background: linear-gradient(135deg, #f472b6, #ec4899, #be185d);
  border: 1px solid #f9a8d4;
  box-shadow: 0 2px 10px rgba(236, 72, 153, 0.6), 0 2px 6px rgba(0, 0, 0, 0.3);
  transition: left 0.3s cubic-bezier(0.25, 0.8, 0.25, 1);
  white-space: nowrap;
  line-height: 1.2;
}
.vis-key-label {
  font-size: 8px;
  letter-spacing: 0.08em;
  opacity: 0.85;
  font-weight: 800;
}

/* Slot gap: bar mờ + viền nét đứt hồng — "vị trí key sắp hạ xuống" */
.vis-bar-gap {
  border-color: rgba(236, 72, 153, 0.5);
  border-style: dashed;
  background: repeating-linear-gradient(
    -45deg,
    rgba(236, 72, 153, 0.12) 0 4px,
    transparent 4px 8px
  );
  color: rgba(244, 114, 182, 0.75);
  box-shadow: inset 0 0 16px rgba(236, 72, 153, 0.15);
  opacity: 0.9;
}
.vis-index-gap { color: #f472b6; }
</style>