<script setup lang="ts">
// PathGraph — node-edge graph cho lộ trình học (Data Bench, DESIGN-IDENTITY §1.5).
// VueFlow lazy-load qua PathView (defineAsyncComponent) + CSS '@vue-flow/core/dist/style.css'
// import trong chunk này → entry bundle KHÔNG đổi (ghi số trong decision log).
// Node = block-token PathFlowNode; edge màu resolved khi node nguồn đã qua (ngôn ngữ trạng thái).
import { computed, markRaw } from 'vue';
import { useWindowSize } from '@vueuse/core';
import { VueFlow, type Edge, type Node, type NodeTypesObject } from '@vue-flow/core';
import '@vue-flow/core/dist/style.css';

import PathFlowNode, { type PathNodeData } from './PathFlowNode.vue';
import type { LearningPathNodeDto } from '@/api/gamification';

const props = withDefaults(
  defineProps<{
    nodes: LearningPathNodeDto[];
    enteringId?: number | null;
    finalTestUnlocked?: boolean;
  }>(),
  {
    enteringId: null,
    finalTestUnlocked: false,
  },
);

const emit = defineEmits<{
  'select-node': [node: LearningPathNodeDto];
  'select-final': [];
}>();

// Hằng số layout (px) — snake nhẹ 2 cột trên desktop, cột đơn dưới 640px (không tràn 390).
const ROW_H = 148;
const STEP_X = 140;
const NODE_PAD = 64;

const { width } = useWindowSize();
const narrow = computed(() => width.value < 640);

const graphNodes = computed<Node<PathNodeData>[]>(() => {
  const list: Node<PathNodeData>[] = props.nodes.map((node, idx) => ({
    id: `node-${node.id}`,
    type: 'pathNode',
    position: { x: narrow.value ? 0 : (idx % 2) * STEP_X, y: idx * ROW_H },
    data: {
      ...node,
      index: idx + 1,
      total: props.nodes.length,
      entering: props.enteringId === node.id,
    },
  }));
  list.push({
    id: 'final',
    type: 'pathNode',
    position: {
      x: narrow.value ? 0 : (props.nodes.length % 2) * STEP_X,
      y: props.nodes.length * ROW_H,
    },
    data: {
      id: -1,
      title: 'Kiểm tra cuối lộ trình',
      description: '',
      sortOrder: props.nodes.length + 1,
      status: props.finalTestUnlocked ? 'passed' : 'locked',
      stars: 0,
      bestScore: null,
      lessonId: null,
      simulationKey: null,
      exerciseId: null,
      requiredStages: { quiz: false, lab: false, code: false },
      index: props.nodes.length + 1,
      total: props.nodes.length + 1,
      final: true,
    },
  });
  return list;
});

const graphEdges = computed<Edge[]>(() => {
  const list: Edge[] = [];
  for (let i = 0; i < props.nodes.length; i++) {
    const done = props.nodes[i].status === 'passed';
    list.push({
      id: `edge-${i}`,
      source: `node-${props.nodes[i].id}`,
      target: i + 1 < props.nodes.length ? `node-${props.nodes[i + 1].id}` : 'final',
      type: 'smoothstep',
      // Edge animated dash (CSS .vue-flow__edge.animated — tắt khi prefers-reduced-motion, xem bên dưới)
      animated: true,
      style: {
        stroke: done ? 'var(--color-resolved)' : 'var(--color-index-muted)',
        strokeWidth: 2,
        // Dash dài hơn khi node nguồn đã qua (resolved — ngôn ngữ trạng thái)
        strokeDasharray: done ? '7 5' : '4 6',
      },
    });
  }
  return list;
});

// NodeTypesObject ép kiểu lỏng: SFC script-setup không khai báo đủ NodeProps của vue-flow
// (id/selected/...) — node thật nhận prop qua context, runtime không cần.
// markRaw: tránh Vue wrap component thành reactive object (warn + overhead).
const nodeTypes = { pathNode: markRaw(PathFlowNode) } as unknown as NodeTypesObject;

const flowHeight = computed(() => (props.nodes.length + 1) * ROW_H + NODE_PAD);

function onNodeClick({ node }: { node: Node }): void {
  if (node.id === 'final') {
    emit('select-final');
    return;
  }
  const found = props.nodes.find((n) => n.id === Number(node.id.replace('node-', '')));
  if (found) emit('select-node', found);
}
</script>

<template>
  <div class="path-graph" :style="{ height: `${flowHeight}px` }">
    <VueFlow
      :nodes="graphNodes"
      :edges="graphEdges"
      :node-types="nodeTypes"
      :nodes-draggable="false"
      :nodes-connectable="false"
      :elements-selectable="false"
      :zoom-on-scroll="false"
      :zoom-on-pin="false"
      :pan-on-scroll="false"
      :pan-on-drag="false"
      :prevent-scrolling="false"
      :min-zoom="0.6"
      :max-zoom="1.25"
      class="path-graph__flow"
      @node-click="onNodeClick"
    />
  </div>
</template>

<style scoped>
.path-graph {
  width: 100%;
  min-width: 0;
  border-radius: var(--radius-lg);
}

.path-graph__flow {
  background: transparent;
  width: 100%;
  height: 100%;
}

.path-graph :deep(.vue-flow__node) {
  cursor: default;
}

.path-graph :deep(.vue-flow__edge-path) {
  stroke-linecap: round;
}

.path-graph :deep(.vue-flow__handle) {
  pointer-events: none;
}

@media (prefers-reduced-motion: reduce) {
  .path-graph :deep(.vue-flow__edge.animated path) {
    animation: none;
  }
}
</style>
