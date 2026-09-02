<script setup lang="ts">
import { computed, onBeforeUnmount, ref, watch } from 'vue';
import { Play, Pause, RotateCcw, Swords, Trophy, Gauge } from 'lucide-vue-next';
import { getSimulation } from '@/engines/registry';
import type { InputConfig, Step } from '@/engines/core/types';
import Modal from '@/components/ui/Modal.vue';
import Button from '@/components/ui/Button.vue';
import CanvasArea from '@/components/simulator/CanvasArea.vue';

const props = withDefaults(
  defineProps<{
    open: boolean;
    initialKey?: string;
  }>(),
  {
    open: false,
    initialKey: 'sort.quick',
  },
);

const emit = defineEmits<{
  close: [];
}>();

const AVAILABLE_ALGORITHMS = [
  { key: 'sort.quick', title: 'Quick Sort' },
  { key: 'sort.merge', title: 'Merge Sort' },
  { key: 'sort.heap', title: 'Heap Sort' },
  { key: 'sort.insertion', title: 'Insertion Sort' },
  { key: 'sort.selection', title: 'Selection Sort' },
  { key: 'sort.bubble', title: 'Bubble Sort' },
];

const algo1Key = ref(props.initialKey || 'sort.quick');
const algo2Key = ref('sort.bubble');
const sampleArray = ref<number[]>([45, 12, 85, 32, 9, 60, 23, 71, 18, 54]);

const steps1 = ref<Step[]>([]);
const steps2 = ref<Step[]>([]);
const index1 = ref(0);
const index2 = ref(0);
const isRunning = ref(false);
const speed = ref(2); // 1x, 2x, 4x
let timer: ReturnType<typeof setInterval> | null = null;

const step1 = computed<Step | null>(() => steps1.value[index1.value] ?? null);
const step2 = computed<Step | null>(() => steps2.value[index2.value] ?? null);

const is1Finished = computed(() => steps1.value.length > 0 && index1.value >= steps1.value.length - 1);
const is2Finished = computed(() => steps2.value.length > 0 && index2.value >= steps2.value.length - 1);

const winner = computed(() => {
  if (is1Finished.value && !is2Finished.value) return 1;
  if (is2Finished.value && !is1Finished.value) return 2;
  if (is1Finished.value && is2Finished.value) {
    return steps1.value.length <= steps2.value.length ? 1 : 2;
  }
  return 0;
});

function loadSimulations(): void {
  stopTimer();
  index1.value = 0;
  index2.value = 0;
  isRunning.value = false;

  const input: InputConfig = {
    kind: 'array',
    data: { values: [...sampleArray.value] },
  };

  const gen1 = getSimulation(algo1Key.value);
  const gen2 = getSimulation(algo2Key.value);

  steps1.value = gen1 ? gen1.generate(input) : [];
  steps2.value = gen2 ? gen2.generate(input) : [];
}

function randomizeArray(): void {
  const arr: number[] = [];
  for (let i = 0; i < 12; i++) {
    arr.push(Math.floor(Math.random() * 85) + 10);
  }
  sampleArray.value = arr;
  loadSimulations();
}

function startTimer(): void {
  stopTimer();
  isRunning.value = true;
  const interval = Math.max(40, 1000 / speed.value);
  timer = setInterval(() => {
    let advanced = false;
    if (index1.value < steps1.value.length - 1) {
      index1.value++;
      advanced = true;
    }
    if (index2.value < steps2.value.length - 1) {
      index2.value++;
      advanced = true;
    }
    if (!advanced) {
      stopTimer();
      isRunning.value = false;
    }
  }, interval);
}

function stopTimer(): void {
  if (timer) {
    clearInterval(timer);
    timer = null;
  }
}

function togglePlay(): void {
  if (isRunning.value) {
    stopTimer();
    isRunning.value = false;
  } else {
    if (is1Finished.value && is2Finished.value) {
      index1.value = 0;
      index2.value = 0;
    }
    startTimer();
  }
}

function reset(): void {
  stopTimer();
  isRunning.value = false;
  index1.value = 0;
  index2.value = 0;
}

watch(
  () => [props.open, algo1Key.value, algo2Key.value] as const,
  ([isOpen]) => {
    if (isOpen) {
      loadSimulations();
    } else {
      stopTimer();
    }
  },
  { immediate: true },
);

onBeforeUnmount(() => {
  stopTimer();
});
</script>

<template>
  <Modal
    :open="open"
    title="Chạy đua Song Song (Dual Algorithm Race)"
    width="960px"
    @close="emit('close')"
  >
    <div class="dual-race">
      <!-- Toolbar cấu hình -->
      <div class="dual-race__header">
        <div class="flex items-center gap-3 flex-wrap">
          <div class="flex items-center gap-1.5">
            <span class="text-xs font-bold text-teal-400">Thuật toán A:</span>
            <select v-model="algo1Key" class="dual-race__select" :disabled="isRunning">
              <option v-for="a in AVAILABLE_ALGORITHMS" :key="a.key" :value="a.key">{{ a.title }}</option>
            </select>
          </div>

          <div class="flex items-center gap-1 px-2 py-1 rounded bg-purple-500/20 text-purple-300 font-bold text-xs">
            <Swords :size="14" />
            <span>VS</span>
          </div>

          <div class="flex items-center gap-1.5">
            <span class="text-xs font-bold text-amber-400">Thuật toán B:</span>
            <select v-model="algo2Key" class="dual-race__select" :disabled="isRunning">
              <option v-for="a in AVAILABLE_ALGORITHMS" :key="a.key" :value="a.key">{{ a.title }}</option>
            </select>
          </div>
        </div>

        <div class="flex items-center gap-2">
          <Button size="sm" variant="ghost" :disabled="isRunning" @click="randomizeArray">
            Đổi mảng ngẫu nhiên
          </Button>
          <div class="flex items-center gap-1 text-xs text-slate-400">
            <Gauge :size="13" />
            <select v-model="speed" class="dual-race__select" @change="isRunning && startTimer()">
              <option :value="1">1x</option>
              <option :value="2">2x</option>
              <option :value="4">4x</option>
            </select>
          </div>
        </div>
      </div>

      <!-- 2 Cửa sổ Canvas so sánh song song -->
      <div class="dual-race__stage-grid">
        <!-- Canvas Thuật toán 1 -->
        <div class="dual-race__stage" :class="{ 'dual-race__stage--winner': winner === 1 }">
          <div class="dual-race__stage-header">
            <div class="flex items-center gap-1.5">
              <span class="w-2.5 h-2.5 rounded-full bg-teal-400"></span>
              <span class="font-bold text-xs text-white">{{ AVAILABLE_ALGORITHMS.find(a => a.key === algo1Key)?.title }}</span>
            </div>
            <div class="flex items-center gap-2 text-[11px] font-mono text-slate-400">
              <span>So sánh: <b class="text-white">{{ step1?.stats.comparisons ?? 0 }}</b></span>
              <span>Đổi chỗ: <b class="text-white">{{ step1?.stats.swaps ?? 0 }}</b></span>
              <span>Bước: <b class="text-teal-400">{{ index1 + 1 }}/{{ steps1.length }}</b></span>
            </div>
          </div>
          <div class="dual-race__canvas-wrapper">
            <CanvasArea
              :structure="step1?.structure ?? null"
              :show-index="true"
              :show-values="true"
              :zoom="1"
            />
            <div v-if="is1Finished" class="dual-race__badge-finished">
              <Trophy v-if="winner === 1" :size="14" class="text-amber-400 animate-bounce" />
              <span>{{ winner === 1 ? 'CHIẾN THẮNG 🏆' : 'ĐÃ HOÀN THÀNH' }}</span>
            </div>
          </div>
        </div>

        <!-- Canvas Thuật toán 2 -->
        <div class="dual-race__stage" :class="{ 'dual-race__stage--winner': winner === 2 }">
          <div class="dual-race__stage-header">
            <div class="flex items-center gap-1.5">
              <span class="w-2.5 h-2.5 rounded-full bg-amber-400"></span>
              <span class="font-bold text-xs text-white">{{ AVAILABLE_ALGORITHMS.find(a => a.key === algo2Key)?.title }}</span>
            </div>
            <div class="flex items-center gap-2 text-[11px] font-mono text-slate-400">
              <span>So sánh: <b class="text-white">{{ step2?.stats.comparisons ?? 0 }}</b></span>
              <span>Đổi chỗ: <b class="text-white">{{ step2?.stats.swaps ?? 0 }}</b></span>
              <span>Bước: <b class="text-amber-400">{{ index2 + 1 }}/{{ steps2.length }}</b></span>
            </div>
          </div>
          <div class="dual-race__canvas-wrapper">
            <CanvasArea
              :structure="step2?.structure ?? null"
              :show-index="true"
              :show-values="true"
              :zoom="1"
            />
            <div v-if="is2Finished" class="dual-race__badge-finished">
              <Trophy v-if="winner === 2" :size="14" class="text-amber-400 animate-bounce" />
              <span>{{ winner === 2 ? 'CHIẾN THẮNG 🏆' : 'ĐÃ HOÀN THÀNH' }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Controls điều khiển chung -->
      <div class="dual-race__controls">
        <Button variant="ghost" size="sm" @click="reset">
          <RotateCcw :size="14" />
          <span>Đặt lại</span>
        </Button>
        <Button variant="primary" size="sm" class="px-6" @click="togglePlay">
          <component :is="isRunning ? Pause : Play" :size="14" />
          <span>{{ isRunning ? 'Tạm dừng' : 'Bắt đầu đua' }}</span>
        </Button>
      </div>
    </div>
  </Modal>
</template>

<style scoped>
.dual-race {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.dual-race__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  background: #13111C;
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 10px;
  padding: 8px 12px;
  flex-wrap: wrap;
  gap: 8px;
}

.dual-race__select {
  background: #1C1929;
  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: 6px;
  color: #FFFFFF;
  font-size: 12px;
  padding: 3px 8px;
}

.dual-race__stage-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 12px;
}

@media (max-width: 768px) {
  .dual-race__stage-grid {
    grid-template-columns: 1fr;
  }
}

.dual-race__stage {
  background: #0D1020;
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 12px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  transition: all 250ms ease;
}

.dual-race__stage--winner {
  border-color: #F59E0B;
  box-shadow: 0 0 16px rgba(245, 158, 11, 0.25);
}

.dual-race__stage-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 8px 12px;
  background: #13111C;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.dual-race__canvas-wrapper {
  height: 220px;
  position: relative;
  display: flex;
  flex-direction: column;
  padding: 6px;
}

.dual-race__badge-finished {
  position: absolute;
  top: 12px;
  right: 12px;
  display: flex;
  align-items: center;
  gap: 4px;
  background: rgba(16, 185, 129, 0.2);
  border: 1px solid rgba(16, 185, 129, 0.4);
  color: #34D399;
  font-size: 10px;
  font-weight: 800;
  padding: 2px 8px;
  border-radius: 9999px;
  backdrop-filter: blur(4px);
}

.dual-race__controls {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 12px;
  padding-top: 8px;
  border-top: 1px solid rgba(255, 255, 255, 0.08);
}
</style>
