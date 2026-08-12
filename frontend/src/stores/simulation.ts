import { computed, ref } from 'vue';
import { defineStore } from 'pinia';

import type { InputConfig, Step } from '@/engines/core/types';

/** Trạng thái máy trạng thái mô phỏng — SDD §3.5 */
export type SimulationStatus = 'idle' | 'running' | 'paused' | 'finished';

export interface SimulationStats {
  comparisons: number;
  swaps: number;
  writes: number;
}

/** Store simulation theo SDD §3.2 — body hoàn thiện ở task engine (EDV §4) */
export const useSimulationStore = defineStore('simulation', () => {
  const currentSim = ref<{ key: string; title: string } | null>(null);
  const steps = ref<Step[]>([]);
  const currentIndex = ref(0);
  const speed = ref(1); // 0.25x .. 4x — SDD §3.5: interval = 1200 / speed ms
  const status = ref<SimulationStatus>('idle');
  const stats = ref<SimulationStats>({ comparisons: 0, swaps: 0, writes: 0 });
  const inputConfig = ref<InputConfig | null>(null);

  const currentStep = computed<Step | null>(() => steps.value[currentIndex.value] ?? null);
  const isFirst = computed(() => currentIndex.value <= 0);
  const isLast = computed(() => currentIndex.value >= steps.value.length - 1);

  async function loadSim(key: string, input?: InputConfig): Promise<void> {
    // TODO (task engine): nạp generator từ engines/registry → generate() → steps
    void key;
    void input;
    return Promise.reject(new Error('TODO: simulationStore.loadSim chưa triển khai'));
  }

  async function configureInput(input: InputConfig): Promise<void> {
    // TODO (task engine): validate + reset + loadSim lại
    void input;
    return Promise.reject(new Error('TODO: simulationStore.configureInput chưa triển khai'));
  }

  function play(): void {
    // TODO (task engine): idle/running theo SDD §3.5
    status.value = 'running';
  }

  function pause(): void {
    // TODO (task engine)
    status.value = 'paused';
  }

  function stepForward(): void {
    // TODO (task engine): bước tới + chuyển finished ở bước cuối
  }

  function stepBack(): void {
    // TODO (task engine): bước lùi miễn phí (ADR-001)
  }

  function jumpTo(index: number): void {
    // TODO (task engine): clamp [0, steps.length-1]
    void index;
  }

  function reset(): void {
    // TODO (task engine): về bước 0, status idle
    currentIndex.value = 0;
    status.value = 'idle';
  }

  function setSpeed(value: number): void {
    // TODO (task engine): clamp 0.25..4
    speed.value = value;
  }

  function setBreakpoint(_line: number, _enabled: boolean): void {
    // TODO (task engine): breakpoint theo dòng template
  }

  return {
    currentSim,
    steps,
    currentIndex,
    speed,
    status,
    stats,
    inputConfig,
    currentStep,
    isFirst,
    isLast,
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
  };
});
