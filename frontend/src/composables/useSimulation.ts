import { onMounted, onUnmounted } from 'vue';
import { storeToRefs } from 'pinia';

import { useSimulationStore } from '@/stores/simulation';
import type { InputConfig } from '@/engines/core/types';

/**
 * useSimulation(key) — SDD §3.6: nạp sim, play/pause/step/jump/setSpeed, dọn timer khi unmount.
 * Triển khai thật: ủy quyền toàn bộ cho simulationStore (generator từ engines/registry).
 */
export function useSimulation(key: string) {
  const store = useSimulationStore();
  const state = storeToRefs(store);

  function loadSim(input?: InputConfig): Promise<void> {
    return store.loadSim(key, input);
  }

  onMounted(() => {
    void loadSim().catch(() => {
      /* loadSim không reject — lỗi nằm trong loadError */
    });
  });

  onUnmounted(() => {
    // Dọn timer playback của store (SDD §3.5)
    store.stopPlayback();
  });

  return {
    ...state,
    loadSim,
    configureInput: store.configureInput,
    play: store.play,
    pause: store.pause,
    stepForward: store.stepForward,
    stepBack: store.stepBack,
    jumpTo: store.jumpTo,
    setSpeed: store.setSpeed,
    reset: store.reset,
    toggleBreakpoint: store.toggleBreakpoint,
  };
}
