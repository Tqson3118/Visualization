<template>
  <div class="inline-sim-player my-6 rounded-2xl bg-[#0f111a] border border-purple-500/30 overflow-hidden shadow-2xl transition-all duration-300 hover:border-purple-500/50 not-prose">
    <!-- Header -->
    <div class="px-4 py-3 bg-[#16192b] border-b border-purple-500/20 flex flex-wrap items-center justify-between gap-3">
      <div class="flex items-center gap-2.5 min-w-0">
        <span class="p-2 rounded-xl bg-purple-600/20 text-purple-300 flex items-center justify-center shrink-0">
          <Gamepad2 class="w-4 h-4 text-purple-400" />
        </span>
        <div class="min-w-0">
          <div class="flex items-center gap-2 flex-wrap">
            <h3 class="font-bold text-white text-sm tracking-tight truncate">{{ title }}</h3>
            <span class="px-2 py-0.5 rounded-md bg-purple-950/80 border border-purple-500/30 text-purple-300 font-mono text-[11px]">
              ✨ {{ simKey }}
            </span>
          </div>
          <p class="text-[11px] text-slate-400 truncate">Mô phỏng thuật toán trực quan tương tác từng bước</p>
        </div>
      </div>

      <div class="flex items-center gap-2 shrink-0">
        <button
          v-if="pseudocode.length > 0"
          type="button"
          class="px-2.5 py-1.5 rounded-lg text-xs font-medium border transition-colors flex items-center gap-1.5 cursor-pointer"
          :class="showCode ? 'bg-purple-600/20 border-purple-500/50 text-purple-200' : 'bg-[#0f111a] border-slate-700/60 text-slate-300 hover:text-white hover:border-slate-600'"
          @click="showCode = !showCode"
          title="Xem mã giả (Pseudocode)"
        >
          <Code2 class="w-3.5 h-3.5" />
          <span>{{ showCode ? 'Ẩn mã giả' : 'Mã giả' }}</span>
        </button>

        <router-link
          :to="`/simulator/${simKey}`"
          target="_blank"
          class="px-2.5 py-1.5 rounded-lg bg-purple-600/10 hover:bg-purple-600/25 border border-purple-500/30 text-purple-300 hover:text-white text-xs font-semibold flex items-center gap-1 transition-all"
          title="Mở toàn màn hình trong Visual Studio"
        >
          <span>Mở rộng</span>
          <ExternalLink class="w-3 h-3" />
        </router-link>
      </div>
    </div>

    <!-- Main Visualizer Area -->
    <div class="relative p-3 sm:p-4 bg-[#0a0c14] flex flex-col gap-3 min-h-[260px]">
      <!-- Load error fallback -->
      <div v-if="loadError" class="p-6 text-center text-slate-400 text-sm flex flex-col items-center justify-center min-h-[220px]">
        <AlertCircle class="w-8 h-8 text-amber-400 mb-2" />
        <p>{{ loadError }}</p>
        <router-link :to="`/simulator/${simKey}`" target="_blank" class="mt-3 text-xs text-purple-400 underline hover:text-purple-300">
          Chạy trên trang Simulator riêng
        </router-link>
      </div>

      <!-- Live Interactive Canvas -->
      <div v-else class="grid grid-cols-1 gap-3" :class="showCode && pseudocode.length > 0 ? 'lg:grid-cols-12' : ''">
        <!-- Canvas area -->
        <div :class="showCode && pseudocode.length > 0 ? 'lg:col-span-8' : 'w-full'">
          <div class="h-60 sm:h-68 w-full rounded-xl overflow-hidden bg-[#0d101d] border border-slate-800/80 shadow-inner relative flex items-center justify-center">
            <CanvasArea
              :structure="currentStep?.structure ?? null"
              :zoom="1"
              class="w-full h-full"
            />
          </div>
        </div>

        <!-- Collapsible Pseudocode panel -->
        <div
          v-if="showCode && pseudocode.length > 0"
          class="lg:col-span-4 h-60 sm:h-68 rounded-xl bg-[#0d101d] border border-slate-800/80 p-3 overflow-y-auto font-mono text-xs text-slate-300 flex flex-col"
        >
          <div class="text-[10px] font-bold text-slate-400 uppercase tracking-wider pb-1.5 mb-1.5 border-b border-slate-800 flex items-center justify-between">
            <span>Mã giả</span>
            <span class="text-purple-400">Dòng {{ currentStep?.pseudocodeLine || '-' }}</span>
          </div>
          <div class="space-y-1 flex-1">
            <div
              v-for="(line, idx) in pseudocode"
              :key="idx"
              class="px-2 py-0.5 rounded transition-colors text-[11px] leading-relaxed whitespace-pre-wrap"
              :class="currentStep?.pseudocodeLine === idx + 1
                ? 'bg-purple-600/30 text-purple-200 font-bold border-l-2 border-purple-400 pl-1.5 shadow-sm'
                : 'text-slate-400 hover:text-slate-200'"
            >
              {{ line }}
            </div>
          </div>
        </div>
      </div>

      <!-- Step Explanation Banner -->
      <div v-if="currentStep" class="p-3 rounded-xl bg-purple-950/30 border border-purple-500/20 flex items-start gap-2.5 text-xs text-slate-200">
        <span class="p-1 rounded-md bg-purple-500/20 text-purple-300 shrink-0 mt-0.5">
          <Info class="w-3.5 h-3.5" />
        </span>
        <div class="min-w-0 flex-1 leading-relaxed">
          <p class="text-white font-medium">{{ currentStep.explanation || 'Đang thực hiện bước thuật toán...' }}</p>
          <div v-if="currentStep.annotations && currentStep.annotations.length > 0" class="mt-1 flex flex-wrap gap-1.5">
            <span
              v-for="(ann, idx) in currentStep.annotations"
              :key="idx"
              class="px-2 py-0.5 rounded bg-[#16192b] border border-purple-500/30 text-purple-300 font-mono text-[10px]"
            >
              {{ ann }}
            </span>
          </div>
        </div>
      </div>
    </div>

    <!-- Control Bar -->
    <div class="px-4 py-2.5 bg-[#121524] border-t border-purple-500/20">
      <ControlBar
        :current-index="currentIndex"
        :total-frames="steps.length"
        :status="status"
        :speed="speed"
        @play="play"
        @pause="pause"
        @step-back="stepBack"
        @step-forward="stepForward"
        @reset="reset"
        @jump-to="jumpTo"
        @set-speed="setSpeed"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { Gamepad2, Code2, ExternalLink, AlertCircle, Info } from 'lucide-vue-next';
import CanvasArea from '@/components/simulator/CanvasArea.vue';
import ControlBar from '@/components/simulator/ControlBar.vue';
import { getSimulation } from '@/engines/registry';
import { getCatalogMeta } from '@/engines/catalog';
import { defaultInput } from '@/engines/generators/helpers';
import type { InputConfig, Step } from '@/engines/core/types';
import type { SimulationStatus } from '@/stores/simulation';

const props = defineProps<{
  simKey: string;
  initialInput?: InputConfig;
}>();

const steps = ref<Step[]>([]);
const currentIndex = ref(0);
const status = ref<SimulationStatus>('idle');
const speed = ref(1);
const loadError = ref<string | null>(null);
const showCode = ref(false);
const pseudocode = ref<string[]>([]);
const title = ref('');

let playbackTimer: ReturnType<typeof setInterval> | null = null;

const currentStep = computed<Step | null>(() => steps.value[currentIndex.value] ?? null);

function clearPlayback(): void {
  if (playbackTimer !== null) {
    clearInterval(playbackTimer);
    playbackTimer = null;
  }
}

function loadSimulation(): void {
  clearPlayback();
  currentIndex.value = 0;
  status.value = 'idle';
  loadError.value = null;

  try {
    const meta = getCatalogMeta(props.simKey);
    title.value = meta ? meta.title : props.simKey;

    const gen = getSimulation(props.simKey);
    if (!gen) {
      loadError.value = `Chưa tìm thấy bộ mô phỏng cho '${props.simKey}'.`;
      steps.value = [];
      return;
    }

    pseudocode.value = gen.pseudocode || [];
    const input = props.initialInput ?? defaultInput(gen);
    steps.value = gen.generate(input);
    if (steps.value.length === 0) {
      loadError.value = 'Mô phỏng không sinh được bước nào.';
    }
  } catch (err: any) {
    loadError.value = err?.message || 'Lỗi khi khởi tạo mô phỏng.';
  }
}

function play(): void {
  clearPlayback();
  if (steps.value.length === 0) return;
  if (currentIndex.value >= steps.value.length - 1) {
    currentIndex.value = 0;
  }
  status.value = 'running';
  const interval = Math.max(75, 1200 / speed.value);
  playbackTimer = setInterval(() => {
    if (status.value !== 'running') {
      clearPlayback();
      return;
    }
    if (currentIndex.value >= steps.value.length - 1) {
      status.value = 'finished';
      clearPlayback();
      return;
    }
    currentIndex.value += 1;
  }, interval);
}

function pause(): void {
  status.value = 'paused';
  clearPlayback();
}

function stepForward(): void {
  if (currentIndex.value < steps.value.length - 1) {
    currentIndex.value += 1;
  } else {
    status.value = 'finished';
  }
}

function stepBack(): void {
  if (currentIndex.value > 0) {
    currentIndex.value -= 1;
    if (status.value === 'finished') {
      status.value = 'paused';
    }
  }
}

function reset(): void {
  clearPlayback();
  currentIndex.value = 0;
  status.value = 'idle';
}

function jumpTo(idx: number): void {
  currentIndex.value = Math.max(0, Math.min(idx, steps.value.length - 1));
}

function setSpeed(s: number): void {
  speed.value = s;
  if (status.value === 'running') {
    play();
  }
}

watch(() => props.simKey, () => {
  loadSimulation();
});

onMounted(() => {
  loadSimulation();
});

onBeforeUnmount(() => {
  clearPlayback();
});
</script>
