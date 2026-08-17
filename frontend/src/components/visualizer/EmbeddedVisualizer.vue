<script setup lang="ts">
// EmbeddedVisualizer — visual tương tác inline trong Lesson theory (Workstream B/B3).
// Không thêm tab Visualizer riêng: bấm "Chạy thử thuật toán" mở shell này ngay trong theory.
import { onMounted, ref } from 'vue';
import { Loader2 } from 'lucide-vue-next';

import { getSimulation } from '@/engines/registry';
import type { InputConfig, SimulationGenerator, Step } from '@/engines/core/types';
import { defaultInputFromSchema } from '@/visualizer/helpers';
import { legacyStepsToFrames } from '@/visualizer/stepToFrames';
import type { SharedVisualFrame } from '@/visualizer/types';

import SharedVisualizerShell from './SharedVisualizerShell.vue';

const props = withDefaults(
  defineProps<{
    simulationKey: string;
    defaultInput?: InputConfig;
  }>(),
  {},
);

const emit = defineEmits<{ close: [] }>();

const loading = ref(true);
const error = ref('');
const generator = ref<SimulationGenerator | null>(null);
const frames = ref<SharedVisualFrame[]>([]);
const title = ref('');
const subtitle = ref('');

onMounted(() => {
  const gen = getSimulation(props.simulationKey);
  if (!gen) {
    error.value = 'Không tìm thấy mô phỏng cho thuật toán này.';
    loading.value = false;
    return;
  }
  generator.value = gen;
  title.value = gen.title;
  subtitle.value = `${gen.dataStructure} · Độ phức tạp TB ${gen.complexity.average}`;
  const input = props.defaultInput ?? defaultInputFromSchema(gen);
  const steps: Step[] = gen.generate(input);
  frames.value = legacyStepsToFrames(steps, gen.key);
  loading.value = false;
});
</script>

<template>
  <div class="embedded-visualizer" data-testid="embedded-visualizer">
    <div v-if="loading" class="embedded-visualizer__state" data-testid="embedded-loading">
      <Loader2 :size="18" class="embedded-visualizer__spin" aria-hidden="true" />
      Đang chuẩn bị thuật toán…
    </div>
    <p v-else-if="error" class="embedded-visualizer__state embedded-visualizer__error" data-testid="embedded-error">
      {{ error }}
    </p>
    <SharedVisualizerShell
      v-else
      :frames="frames"
      :pseudocode="generator?.pseudocode ?? []"
      :title="title"
      :subtitle="subtitle"
      @close="emit('close')"
    />
  </div>
</template>

<style scoped>
.embedded-visualizer {
  width: 100%;
}
.embedded-visualizer__state {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-sm);
  min-height: 120px;
  color: var(--color-text-secondary);
  font-size: var(--text-sm);
}
.embedded-visualizer__error {
  color: var(--color-destructive);
}
.embedded-visualizer__spin {
  animation: embedded-spin 0.9s linear infinite;
}
@keyframes embedded-spin {
  to { transform: rotate(360deg); }
}
@media (prefers-reduced-motion: reduce) {
  .embedded-visualizer__spin { animation: none; }
}
</style>
