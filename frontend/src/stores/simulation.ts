import { computed, ref } from 'vue';
import { defineStore } from 'pinia';

import type { InputConfig, SimulationGenerator, Step } from '@/engines/core/types';
import { getSimulation } from '@/engines/registry';
import { useUiStore } from './ui';

/** Trạng thái máy trạng thái mô phỏng — SDD §3.5 */
export type SimulationStatus = 'idle' | 'running' | 'paused' | 'finished';

export interface SimulationStats {
  comparisons: number;
  swaps: number;
  writes: number;
}

export interface LoadedSimulation {
  key: string;
  title: string;
  generator: SimulationGenerator;
}

/** Store simulation theo SDD §3.2 — triển khai thật với generator từ engines/registry (task 3). */
export const useSimulationStore = defineStore('simulation', () => {
  const currentSim = ref<LoadedSimulation | null>(null);
  const steps = ref<Step[]>([]);
  const currentIndex = ref(0);
  const speed = ref(1); // 0.25x .. 4x — SDD §3.5: interval = 1200 / speed ms
  const status = ref<SimulationStatus>('idle');
  const stats = ref<SimulationStats>({ comparisons: 0, swaps: 0, writes: 0 });
  const inputConfig = ref<InputConfig | null>(null);
  const loading = ref(false);
  const loadError = ref<string | null>(null);

  let playbackTimer: ReturnType<typeof setInterval> | null = null;

  const currentStep = computed<Step | null>(() => steps.value[currentIndex.value] ?? null);
  const isFirst = computed(() => currentIndex.value <= 0);
  const isLast = computed(() => currentIndex.value >= steps.value.length - 1);
  const totalSteps = computed(() => steps.value.length);
  const generator = computed(() => currentSim.value?.generator ?? null);

  function clearPlayback(): void {
    if (playbackTimer !== null) {
      clearInterval(playbackTimer);
      playbackTimer = null;
    }
  }

  function startPlayback(): void {
    clearPlayback();
    if (steps.value.length === 0) return;
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

  /** Nạp generator từ registry → generate() → steps (SDD §4.5, ADR-003) */
  async function loadSim(key: string, input?: InputConfig): Promise<void> {
    loading.value = true;
    loadError.value = null;
    clearPlayback();
    try {
      const gen = getSimulation(key);
      if (!gen) {
        loadError.value = `Không tìm thấy mô phỏng '${key}'`;
        currentSim.value = null;
        steps.value = [];
        status.value = 'idle';
        return;
      }
      const config: InputConfig = input ?? defaultInputFromSchema(gen);
      const validation = gen.validate(config);
      if (!validation.ok) {
        loadError.value = validation.errors.join('; ');
        return;
      }
      const generated = gen.generate(config);
      if (generated.length === 0) {
        loadError.value = 'Mô phỏng không có bước nào — hãy kiểm tra cấu hình đầu vào.';
        return;
      }
      currentSim.value = { key, title: gen.title, generator: gen };
      inputConfig.value = config;
      steps.value = generated;
      currentIndex.value = 0;
      status.value = 'idle';
      const last = generated[generated.length - 1];
      stats.value = { ...last.stats };
      if (generated.length >= 90) {
        useUiStore().showToast('Dữ liệu lớn, mô phỏng có thể chậm', 'warning');
      }
    } finally {
      loading.value = false;
    }
  }

  /** Cấu hình lại input → validate → sinh lại steps */
  async function configureInput(input: InputConfig): Promise<void> {
    if (!currentSim.value) return;
    loading.value = true;
    loadError.value = null;
    try {
      const validation = currentSim.value.generator.validate(input);
      if (!validation.ok) {
        loadError.value = validation.errors.join('; ');
        return;
      }
      const generated = currentSim.value.generator.generate(input);
      if (generated.length === 0) {
        loadError.value = 'Mô phỏng không có bước nào — hãy kiểm tra cấu hình đầu vào.';
        return;
      }
      inputConfig.value = input;
      steps.value = generated;
      currentIndex.value = 0;
      status.value = 'idle';
      const last = generated[generated.length - 1];
      stats.value = { ...last.stats };
    } finally {
      loading.value = false;
    }
  }

  function play(): void {
    if (steps.value.length === 0) return;
    if (status.value === 'finished') {
      currentIndex.value = 0;
      status.value = 'running';
    } else {
      status.value = 'running';
    }
    startPlayback();
  }

  function pause(): void {
    status.value = 'paused';
    clearPlayback();
  }

  function stepForward(): void {
    if (currentIndex.value >= steps.value.length - 1) {
      status.value = 'finished';
      clearPlayback();
      return;
    }
    currentIndex.value += 1;
  }

  function stepBack(): void {
    if (currentIndex.value > 0) currentIndex.value -= 1;
  }

  function jumpTo(index: number): void {
    const clamped = Math.max(0, Math.min(steps.value.length - 1, index));
    currentIndex.value = clamped;
    if (clamped >= steps.value.length - 1 && steps.value.length > 0) {
      status.value = 'finished';
      clearPlayback();
    }
  }

  function reset(): void {
    clearPlayback();
    currentIndex.value = 0;
    status.value = 'idle';
  }

  function setSpeed(value: number): void {
    const clamped = Math.max(0.25, Math.min(4, value));
    speed.value = clamped;
    if (status.value === 'running') startPlayback(); // áp dụng nhịp mới ngay (SDD Màn 05)
  }

  function setBreakpoint(_line: number, _enabled: boolean): void {
    // TODO (task nâng cao): breakpoint theo dòng template — UI dừng tại bước có pseudocodeLine
    void _line;
    void _enabled;
  }

  /** Dọn timer khi unmount (gọi từ composable useSimulation). */
  function stopPlayback(): void {
    clearPlayback();
  }

  return {
    currentSim,
    steps,
    currentIndex,
    speed,
    status,
    stats,
    inputConfig,
    loading,
    loadError,
    currentStep,
    isFirst,
    isLast,
    totalSteps,
    generator,
    loadSim,
    configureInput,
    play,
    pause,
    stepForward,
    stepBack,
    jumpTo,
    reset,
    setSpeed,
    setBreakpoint,
    stopPlayback,
  };
});

/** Input mặc định từ inputSchema của generator (SDD §4.14). */
function defaultInputFromSchema(gen: SimulationGenerator): InputConfig {
  const data: Record<string, unknown> = {};
  for (const field of gen.inputSchema.fields) {
    data[field.name] = field.default;
  }
  return { kind: gen.inputSchema.kind, data };
}
