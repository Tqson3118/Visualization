<template>
  <div
    class="h-full w-full flex items-end justify-center px-4 pb-6"
    :style="containerStyle"
  >
    <!-- Algorithm Info Panel -->
    <div v-if="showInfoPanel" class="algo-info-panel animate-slide-in" data-tour-id="algo-info-panel">
      <div class="algo-info-header">
        <h3 class="algo-info-title">{{ algoInfo.name }}</h3>
        <button 
          class="info-close-btn" 
          @click="showInfoPanel = false"
          aria-label="Đóng panel thông tin"
        >
          <BaseIcon name="x" class="w-4 h-4" />
        </button>
      </div>
      <div class="algo-info-content">
        <div class="algo-meta-row">
          <span class="algo-meta-label">Độ phức tạp:</span>
          <span class="algo-meta-value">{{ algoInfo.timeComplexity }}</span>
        </div>
        <div class="algo-meta-row">
          <span class="algo-meta-label">Không gian:</span>
          <span class="algo-meta-value">{{ algoInfo.spaceComplexity }}</span>
        </div>
        <div class="algo-meta-row">
          <span class="algo-meta-label">Ổn định:</span>
          <span class="algo-meta-value" :class="algoInfo.stable ? 'text-accent-green' : 'text-accent-red'">
            {{ algoInfo.stable ? 'Có' : 'Không' }}
          </span>
        </div>
        <p class="algo-description">{{ algoInfo.description }}</p>
      </div>
    </div>

    <!-- Step Narration Panel -->
    <div v-if="showNarrationPanel && stepDescription" class="narration-panel animate-fade-in" data-tour-id="algo-narration-panel">
      <div class="narration-header">
        <span class="narration-label">Bước {{ frame?.stepIndex || 0 }}</span>
        <button 
          class="narration-close-btn" 
          @click="showNarrationPanel = false"
          aria-label="Đóng panel giải thích"
        >
          <BaseIcon name="x" class="w-4 h-4" />
        </button>
      </div>
      <div class="narration-content">
        <p class="narration-text">{{ stepDescription }}</p>
        <div class="narration-progress">
          <div class="narration-progress-bar">
            <div class="narration-progress-fill" :style="{ width: progressPercent + '%' }"></div>
          </div>
          <span class="narration-progress-text">{{ progressPercent }}% hoàn thành</span>
        </div>
      </div>
    </div>

    <!-- Stats Counter Panel -->
    <div class="stats-counter-panel animate-slide-up" data-tour-id="algo-stats-panel">
      <div class="stat-item">
        <span class="stat-label">So sánh</span>
        <span class="stat-value" :class="{ 'stat-animating': comparingAnimating }">
          {{ comparisonCount }}
        </span>
      </div>
      <div class="stat-divider"></div>
      <div class="stat-item">
        <span class="stat-label">Hoán vị</span>
        <span class="stat-value" :class="{ 'stat-animating': swappingAnimating }">
          {{ swapCount }}
        </span>
      </div>
      <div class="stat-divider"></div>
      <div class="stat-item">
        <span class="stat-label">Bước</span>
        <span class="stat-value">{{ frame?.stepIndex || 0 }}</span>
      </div>
    </div>

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
    </div>

    <!-- Floating Action Buttons -->
    <div class="floating-actions">
      <button 
        class="fab-btn" 
        @click="showInfoPanel = !showInfoPanel"
        :aria-label="showInfoPanel ? 'Ẩn thông tin thuật toán' : 'Hiện thông tin thuật toán'"
        :class="{ active: showInfoPanel }"
        :title="showInfoPanel ? 'Ẩn thông tin thuật toán' : 'Hiện thông tin thuật toán'"
      >
        <BaseIcon name="info" class="w-5 h-5" />
      </button>
      <button 
        class="fab-btn" 
        @click="showNarrationPanel = !showNarrationPanel"
        :aria-label="showNarrationPanel ? 'Ẩn giải thích bước' : 'Hiện giải thích bước'"
        :class="{ active: showNarrationPanel }"
        :title="showNarrationPanel ? 'Ẩn giải thích bước' : 'Hiện giải thích bước'"
      >
        <BaseIcon name="message-square" class="w-5 h-5" />
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import type { SortFrame } from '../types/sorting.types';
import BaseIcon from '@/shared/components/BaseIcon.vue';

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

const maxVal = computed(() => {
  if (!props.frame?.arrayState?.length) return 1;
  return Math.max(...props.frame.arrayState, 1);
});

function barHeightPct(value: number): number {
  const ratio = value / maxVal.value;
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
  if (sortedIndices.includes(idx))     return 'vis-index-sorted';
  if (swappedIndices?.includes(idx))   return 'vis-index-swapped';
  if (comparingIndices?.includes(idx)) return 'vis-index-comparing';
  return 'vis-index-default';
}

// Premium features
const showInfoPanel = ref(true);
const showNarrationPanel = ref(true);

const comparingAnimating = ref(false);
const swappingAnimating = ref(false);

const comparisonCount = ref(0);
const swapCount = ref(0);

// Watch for frame changes to animate counters
watch(() => props.frame?.comparingIndices?.length, (newVal, oldVal) => {
  if (newVal && newVal > (oldVal || 0)) {
    comparisonCount.value += newVal - (oldVal || 0);
    comparingAnimating.value = true;
    setTimeout(() => { comparingAnimating.value = false; }, 300);
  }
});

watch(() => props.frame?.swappedIndices?.length, (newVal, oldVal) => {
  if (newVal && newVal > (oldVal || 0)) {
    swapCount.value += newVal - (oldVal || 0);
    swappingAnimating.value = true;
    setTimeout(() => { swappingAnimating.value = false; }, 300);
  }
});

watch(() => props.frame?.stepIndex, () => {
  // Reset counters when step resets
  if (!props.frame || props.frame.stepIndex === 0) {
    comparisonCount.value = 0;
    swapCount.value = 0;
  }
});

const stepDescription = computed(() => {
  if (!props.frame) return '';
  return props.frame.description || `Bước ${props.frame.stepIndex}: Thực hiện so sánh`;
});

const progressPercent = computed(() => {
  // Estimate progress based on step index (Bubble Sort has roughly n*(n-1)/2 steps)
  if (!props.frame) return 0;
  const n = itemCount.value;
  const maxSteps = n * (n - 1) / 2;
  return Math.min(100, Math.round((props.frame.stepIndex / Math.max(maxSteps, 1)) * 100));
})

const algoInfo = {
  name: 'Bubble Sort',
  timeComplexity: 'O(n²) (trung bình/xấu nhất), O(n) (tốt nhất)',
  spaceComplexity: 'O(1)',
  stable: true,
  description: 'Bubble Sort lặp qua mảng nhiều lần, so sánh các cặp phần tử liền kề và hoán vị nếu chúng sai thứ tự. Quá trình lặp lại cho đến khi không còn hoán vị nào. Tối ưu: dừng sớm nếu không có hoán vị nào trong một lần lặp.'
};
</script>

<style scoped>
.sort-list-move {
  transition: transform 0.4s cubic-bezier(0.25, 0.8, 0.25, 1);
}
</style>

<style>

.vis-bar-default {
  border-color: var(--vis-color-default);
  background: linear-gradient(to top, color-mix(in srgb, var(--vis-color-default) 15%, transparent), color-mix(in srgb, var(--vis-color-default) 5%, transparent));
  color: var(--color-text-secondary);
  box-shadow: 0 4px 12px color-mix(in srgb, var(--vis-color-default) 10%, transparent);
}
.vis-bar-sorted {
  border-color: var(--vis-color-sorted);
  background: linear-gradient(to top, color-mix(in srgb, var(--vis-color-sorted) 20%, transparent), color-mix(in srgb, var(--vis-color-sorted) 5%, transparent));
  color: var(--vis-color-sorted);
  box-shadow: 0 4px 16px color-mix(in srgb, var(--vis-color-sorted) 20%, transparent);
}
.vis-bar-swapped {
  border-color: var(--vis-color-swap);
  background: linear-gradient(to top, color-mix(in srgb, var(--vis-color-swap) 25%, transparent), color-mix(in srgb, var(--vis-color-swap) 8%, transparent));
  color: var(--vis-color-swap);
  box-shadow: 0 4px 20px color-mix(in srgb, var(--vis-color-swap) 30%, transparent);
  animation: bar-pulse 0.6s ease-in-out;
}
.vis-bar-comparing {
  border-color: var(--vis-color-compare);
  background: linear-gradient(to top, color-mix(in srgb, var(--vis-color-compare) 20%, transparent), color-mix(in srgb, var(--vis-color-compare) 5%, transparent));
  color: var(--vis-color-compare);
  box-shadow: 0 4px 16px color-mix(in srgb, var(--vis-color-compare) 20%, transparent);
}

.vis-index-default { color: var(--color-text-muted); }
.vis-index-sorted   { color: var(--vis-color-sorted); }
.vis-index-swapped  { color: var(--vis-color-swap); }
.vis-index-comparing { color: var(--vis-color-compare); }

@keyframes bar-pulse {
  0%, 100% { transform: scale(1); }
  50% { transform: scale(1.04); }
}

/* ── PREMIUM STYLES ── */

@keyframes slide-in {
  from { opacity: 0; transform: translateX(-20px); }
  to { opacity: 1; transform: translateX(0); }
}

@keyframes fade-in {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes slide-up {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.animate-slide-in { animation: slide-in 0.4s cubic-bezier(0.4, 0, 0.2, 1); }
.animate-fade-in { animation: fade-in 0.3s ease-out; }
.animate-slide-up { animation: slide-up 0.5s cubic-bezier(0.4, 0, 0.2, 1); }

.stat-animating {
  animation: stat-bump 0.3s ease-out;
}

@keyframes stat-bump {
  0% { transform: scale(1); }
  50% { transform: scale(1.2); color: var(--color-accent-primary); }
  100% { transform: scale(1); }
}

/* Algorithm Info Panel */
.algo-info-panel {
  position: absolute;
  top: 12px;
  right: 12px;
  width: 280px;
  max-height: 40vh;
  background: var(--color-bg-surface);
  border: 1px solid var(--color-border-default);
  border-radius: var(--radius-xl);
  box-shadow: 0 8px 32px rgba(0,0,0,0.3), 0 0 0 1px var(--color-border-subtle);
  backdrop-filter: blur(16px);
  z-index: 10;
  overflow: hidden;
}

.algo-info-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border-subtle);
  background: var(--color-bg-primary);
}

.algo-info-title {
  font-size: 0.875rem;
  font-weight: 700;
  color: var(--color-text-primary);
}

.info-close-btn {
  width: 28px;
  height: 28px;
  border-radius: var(--radius-md);
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.info-close-btn:hover {
  background: var(--color-bg-hover);
  color: var(--color-text-primary);
}

.algo-info-content {
  padding: 16px;
  max-height: 35vh;
  overflow-y: auto;
}

.algo-meta-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 0;
  border-bottom: 1px solid var(--color-border-subtle);
}

.algo-meta-label {
  font-size: 0.75rem;
  color: var(--color-text-muted);
}

.algo-meta-value {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--color-text-primary);
  font-family: var(--font-mono);
}

.algo-description {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid var(--color-border-subtle);
  font-size: 0.75rem;
  line-height: 1.6;
  color: var(--color-text-secondary);
}

/* Narration Panel */
.narration-panel {
  position: absolute;
  bottom: 80px;
  left: 12px;
  right: 12px;
  max-width: 600px;
  margin: 0 auto;
  background: var(--color-bg-surface);
  border: 1px solid var(--color-border-default);
  border-radius: var(--radius-xl);
  box-shadow: 0 8px 32px rgba(0,0,0,0.3);
  backdrop-filter: blur(16px);
  z-index: 10;
  overflow: hidden;
}

.narration-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 16px;
  border-bottom: 1px solid var(--color-border-subtle);
  background: var(--color-bg-primary);
}

.narration-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--color-accent-primary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.narration-close-btn {
  width: 24px;
  height: 24px;
  border-radius: var(--radius-md);
  background: transparent;
  border: none;
  color: var(--color-text-muted);
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: all 0.2s ease;
}

.narration-close-btn:hover {
  background: var(--color-bg-hover);
  color: var(--color-text-primary);
}

.narration-content {
  padding: 16px;
}

.narration-text {
  font-size: 0.875rem;
  line-height: 1.6;
  color: var(--color-text-primary);
  margin-bottom: 12px;
}

.narration-progress {
  display: flex;
  align-items: center;
  gap: 12px;
}

.narration-progress-bar {
  flex: 1;
  height: 6px;
  background: var(--color-bg-active);
  border-radius: 3px;
  overflow: hidden;
}

.narration-progress-fill {
  height: 100%;
  background: linear-gradient(90deg, var(--color-accent-primary), var(--color-accent-cyan));
  border-radius: 3px;
  transition: width 0.5s cubic-bezier(0.4, 0, 0.2, 1);
}

.narration-progress-text {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--color-accent-primary);
  font-family: var(--font-mono);
  min-width: 80px;
  text-align: right;
}

/* Stats Counter Panel */
.stats-counter-panel {
  position: absolute;
  top: 12px;
  left: 12px;
  display: flex;
  align-items: center;
  gap: 0;
  background: var(--color-bg-surface);
  border: 1px solid var(--color-border-default);
  border-radius: var(--radius-xl);
  padding: 8px 16px;
  box-shadow: 0 4px 16px rgba(0,0,0,0.2);
  z-index: 10;
}

.stat-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 8px 16px;
  min-width: 70px;
}

.stat-label {
  font-size: 0.625rem;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-muted);
  margin-bottom: 2px;
}

.stat-value {
  font-family: var(--font-mono);
  font-size: 1.125rem;
  font-weight: 700;
  color: var(--color-text-primary);
  transition: all 0.2s ease;
}

.stat-divider {
  width: 1px;
  height: 40px;
  background: var(--color-border-subtle);
  margin: 0 4px;
}

/* Floating Action Buttons */
.floating-actions {
  position: absolute;
  bottom: 20px;
  right: 12px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  z-index: 10;
}

.fab-btn {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  background: var(--color-bg-surface);
  border: 1px solid var(--color-border-default);
  color: var(--color-text-secondary);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
  box-shadow: 0 4px 16px rgba(0,0,0,0.2);
}

.fab-btn:hover {
  background: var(--color-accent-primary);
  border-color: var(--color-accent-primary);
  color: white;
  transform: scale(1.1) translateY(-2px);
  box-shadow: 0 8px 24px var(--color-accent-primary-glow);
}

.fab-btn.active {
  background: var(--color-accent-primary);
  border-color: var(--color-accent-primary);
  color: white;
  box-shadow: 0 4px 16px var(--color-accent-primary-glow);
}

.fab-btn:active {
  transform: scale(0.95);
}

/* Panel Animations */
@keyframes panel-slide-in {
  from { opacity: 0; transform: translateX(-20px); }
  to { opacity: 1; transform: translateX(0); }
}

@keyframes panel-fade-in {
  from { opacity: 0; }
  to { opacity: 1; }
}

@keyframes panel-slide-up {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}

.animate-slide-in { animation: panel-slide-in 0.4s cubic-bezier(0.4, 0, 0.2, 1); }
.animate-fade-in { animation: fade-in 0.3s ease-out; }
.animate-slide-up { animation: slide-up 0.5s cubic-bezier(0.4, 0, 0.2, 1); }
</style>
