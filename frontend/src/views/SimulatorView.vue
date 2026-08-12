<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import { useRoute } from 'vue-router';

import ControlBar from '@/components/simulator/ControlBar.vue';
import ExplainPanel from '@/components/simulator/ExplainPanel.vue';
import { useSimulation } from '@/composables/useSimulation';
import { messages } from '@/i18n/vi';

// SimulatorView — skeleton EDV (SDD §4): useSimulation + ControlBar + ExplainPanel
// + canvas placeholder. Renderer (SDD §4.4) được gắn ở task renderer — TODO bên dưới.
const route = useRoute();
const key = computed(() => String(route.params.key ?? ''));

const {
  currentSim,
  currentStep,
  currentIndex,
  steps,
  speed,
  status,
  play,
  pause,
  stepForward,
  stepBack,
  reset,
  setSpeed,
} = useSimulation(key.value);

const canvasRef = ref<HTMLCanvasElement | null>(null);
const renderError = ref<string | null>(null);

const notFound = computed(() => !currentSim.value && steps.value.length === 0 && status.value === 'idle');

onMounted(() => {
  // TODO (task renderer — SDD §4.4): gắn Renderer theo `currentStep.structure.kind`:
  //   const renderer = getRenderer(structure.kind);  // registry renderer
  //   renderer.mount(canvasRef.value);
  //   watch(currentStep, (step) => step && renderer.render(step.structure, options));
  //   watch([currentIndex, steps], render lại + resize theo container.
  void canvasRef;
});
</script>

<template>
  <main class="simulator container">
    <header class="simulator__header">
      <h1 class="simulator__title">{{ currentSim?.title ?? key }}</h1>
    </header>

    <div v-if="notFound" class="simulator__empty card" role="status">
      {{ messages.simulator.notFound }} ({{ key }})
    </div>

    <div v-else-if="renderError" class="simulator__empty card" role="alert">
      {{ messages.simulator.simError }}: {{ renderError }}
    </div>

    <template v-else>
      <section class="simulator__canvas card" aria-label="Khu vực vẽ mô phỏng">
        <canvas ref="canvasRef" class="simulator__canvas-element" />
        <p class="simulator__canvas-hint text-muted">
          {{ messages.simulator.canvasPlaceholder }}
        </p>
      </section>

      <ExplainPanel
        :explanation="currentStep?.explanation ?? ''"
        :kind="undefined"
        :frame-key="currentIndex"
      />

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
        @set-speed="setSpeed"
      />
    </template>
  </main>
</template>

<style scoped>
.simulator {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  padding-block: var(--space-lg) var(--space-2xl);
}

.simulator__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-md);
}

.simulator__title {
  font-size: var(--text-xl);
  color: var(--color-foreground);
}

.simulator__canvas {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-md);
  min-height: 360px;
}

.simulator__canvas-element {
  width: 100%;
  height: 320px;
  border: 1px dashed var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-muted);
}

.simulator__canvas-hint {
  font-size: var(--text-sm);
  text-align: center;
}

.simulator__empty {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 240px;
  color: var(--color-text-muted);
  text-align: center;
}
</style>
import '@/engines/worker/compileWorker';
