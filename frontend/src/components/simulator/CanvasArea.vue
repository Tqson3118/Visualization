<script setup lang="ts">
// CanvasArea — WebGL Accelerated Game-Style Visualization Area (SDD §8.5)
// Powered by DataStructureStage (PixiJS v8 + GSAP + Howler.js Synthesizer)
// Provides 60fps WebGL rendering, micro-SFX audio feedback, particle effects,
// parabolic arc swaps, dynamic tree rotations, and laser pulse beams.

import type { Element, Structure } from '@/engines/core/types';
import DataStructureStage from '@/components/visualizer/DataStructureStage.vue';

const props = withDefaults(
  defineProps<{
    structure: Structure | null;
    showIndex?: boolean;
    showValues?: boolean;
    zoom?: number;
    emptyText?: string;
    /** Chế độ interactive (Lab Bậc 2) */
    interactive?: boolean;
    simKey?: string;
    complexity?: string;
  }>(),
  {
    structure: null,
    showIndex: true,
    showValues: true,
    zoom: 1,
    emptyText: 'Khu vực vẽ cấu trúc dữ liệu',
    interactive: false,
    simKey: '',
    complexity: '',
  },
);

const emit = defineEmits<{
  'update:show-index': [value: boolean];
  'update:show-values': [value: boolean];
  'update:zoom': [value: number];
  select: [element: Element];
}>();
</script>

<template>
  <div class="canvas-area">
    <DataStructureStage
      :structure="props.structure"
      :show-index="props.showIndex"
      :show-values="props.showValues"
      :zoom="props.zoom"
      :empty-text="props.emptyText"
      :interactive="props.interactive"
      :enable-sound="true"
      :sim-key="props.simKey"
      :complexity="props.complexity"
      height="100%"
      @update:show-index="emit('update:show-index', $event)"
      @update:show-values="emit('update:show-values', $event)"
      @update:zoom="emit('update:zoom', $event)"
      @select="emit('select', $event)"
    />
  </div>
</template>

<style scoped>
.canvas-area {
  display: flex;
  flex-direction: column;
  height: 100%;
  min-height: 420px;
  width: 100%;
}
</style>
