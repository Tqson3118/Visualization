import { onMounted, onUnmounted } from 'vue';
import { storeToRefs } from 'pinia';

import { useSimulationStore } from '@/stores/simulation';
import type { InputConfig } from '@/engines/core/types';

/**
 * useSimulation(key) — SDD §3.6: nạp sim, play/pause/step/jump/setSpeed, dọn timer khi unmount.
 * Skeleton: ủy quyền cho simulationStore; hoàn thiện ở task engine (EDV §4).
 */
export function useSimulation(key: string) {
  const store = useSimulationStore();
  const state = storeToRefs(store);

  function loadSim(input?: InputConfig): Promise<void> {
    return store.loadSim(key, input);
  }

  onMounted(() => {
    // TODO (task engine): dọn interval/timer playback của store.
    // Bắt rejection của skeleton loadSim (store chưa implement) — tránh unhandled rejection.
    void loadSim().catch(() => {
      /* skeleton: simulationStore.loadSim chưa triển khai (task engine) */
    });
  });

  onUnmounted(() => {
    // TODO (task engine): dọn interval/timer playback của store
  });

  return {
    ...state,
    loadSim,
    play: store.play,
    pause: store.pause,
    stepForward: store.stepForward,
    stepBack: store.stepBack,
    jumpTo: store.jumpTo,
    setSpeed: store.setSpeed,
    reset: store.reset,
    setBreakpoint: store.setBreakpoint,
  };
}
