<script setup lang="ts">
// PathFlowNode — node block-token cho PathGraph (signature "Block thở theo bước", DESIGN-IDENTITY §1.5):
// nền canvas-ink LUÔN tối bất kể theme, block trạng thái (data-core/resolved/conflict/index-muted),
// index mono bên dưới — cùng ngôn ngữ với canvas engine. Không dùng Badge/Button UI (node = vùng canvas).
import { computed } from 'vue';
import { Handle, Position, useNodeId, useVueFlow, type GraphNode, type MouseTouchEvent } from '@vue-flow/core';
import { CheckCircle2, Flag, Lock, Play } from 'lucide-vue-next';

import type { LearningPathNodeDto } from '@/api/gamification';

export interface PathNodeData extends LearningPathNodeDto {
  index: number;
  total: number;
  final?: boolean;
  entering?: boolean;
}

const props = defineProps<{
  data: PathNodeData;
}>();

// Phát nodeClick qua store của vue-flow (pattern chính thức cho custom node —
// hook trigger nodeClick = listener của @node-click trên <VueFlow>).
const { emits, findNode } = useVueFlow();
const nodeId = useNodeId();

function activate(event: MouseEvent | KeyboardEvent): void {
  const node = findNode(nodeId);
  if (!node) return;
  emits.nodeClick({ event: event as MouseTouchEvent, node: node as GraphNode });
}

const isFinal = computed(() => props.data.final === true);
const isPassed = computed(() => props.data.status === 'passed' || (props.data.status as string) === 'Completed' || Boolean((props.data as any).isCompleted));
const isLocked = computed(() => props.data.status === 'locked' && !isPassed.value);

const chipLabel = computed(() => {
  if (isFinal.value) return isPassed.value ? 'MỞ ĐƯỢC' : 'KHÓA';
  if (isPassed.value) return 'ĐÃ QUA';
  if (isLocked.value) return 'KHÓA';
  return 'ĐANG HỌC';
});

const chipClass = computed(() => {
  if (isLocked.value) return 'path-node__chip--muted';
  if (isPassed.value) return 'path-node__chip--resolved';
  return 'path-node__chip--active';
});

const iconClass = computed(() => {
  if (isLocked.value) return 'path-node__icon--muted';
  if (isPassed.value) return 'path-node__icon--resolved';
  return 'path-node__icon--active';
});

const label = computed(() => props.data.title);
</script>

<template>
  <div
    role="button"
    tabindex="0"
    class="path-node"
    :class="{
      'path-node--locked': isLocked,
      'path-node--passed': isPassed,
      'path-node--active': !isLocked && !isPassed,
      'path-node--final': isFinal,
    }"
    :aria-label="isFinal ? `Kiểm tra cuối lộ trình — ${chipLabel}` : `${label} — ${chipLabel}`"
    :aria-disabled="isLocked"
    @click.stop="activate"
    @keydown.enter.prevent="activate"
    @keydown.space.prevent="activate"
  >
    <span v-if="data.entering" class="path-node__spinner" aria-hidden="true" />
    <span v-else class="path-node__icon" :class="iconClass" aria-hidden="true">
      <Flag v-if="isFinal" :size="16" />
      <Lock v-else-if="isLocked" :size="16" />
      <CheckCircle2 v-else-if="isPassed" :size="16" />
      <Play v-else :size="16" />
    </span>

    <span class="path-node__title">{{ label }}</span>

    <span class="path-node__meta">
      <span class="path-node__index">{{ isFinal ? 'FINAL' : `NODE ${String(data.index).padStart(2, '0')}/${String(data.total).padStart(2, '0')}` }}</span>
      <span class="path-node__chip" :class="chipClass">{{ chipLabel }}</span>
    </span>

    <Handle type="target" :position="Position.Top" class="path-node__handle" />
    <Handle type="source" :position="Position.Bottom" class="path-node__handle" />
  </div>
</template>

<style scoped>
.path-node {
  position: relative;
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
  width: min(232px, 72vw);
  padding: var(--space-md);
  border-radius: var(--radius-lg);
  background: var(--color-canvas-ink);
  border: 1px solid var(--color-border-strong);
  color: #fff;
  text-align: left;
  cursor: pointer;
  transition:
    border-color 150ms cubic-bezier(0.16, 1, 0.3, 1),
    opacity 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.path-node:focus-visible {
  outline: 2px solid var(--color-ring);
  outline-offset: 2px;
}

.path-node--locked {
  opacity: 0.55;
  cursor: not-allowed;
}

.path-node--passed {
  border-color: color-mix(in srgb, var(--color-resolved) 55%, transparent);
}

.path-node--active {
  border-color: var(--color-data-core);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-data-core) 22%, transparent);
}

.path-node--final {
  border-style: dashed;
}

.path-node__spinner {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  border: 2px solid currentColor;
  border-top-color: transparent;
  animation: path-node-spin 700ms linear infinite;
}

.path-node__icon {
  display: inline-flex;
}

.path-node__icon--active { color: var(--color-data-core); }
.path-node__icon--resolved { color: var(--color-resolved); }
.path-node__icon--muted { color: var(--color-index-muted); }

.path-node__title {
  font-size: var(--text-sm);
  font-weight: 500;
  line-height: 1.4;
}

.path-node__meta {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
}

.path-node__index {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.06em;
  color: var(--color-index-muted);
}

.path-node__chip {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  letter-spacing: 0.04em;
  padding: 2px 8px;
  border-radius: var(--radius-sm);
  border: 1px solid transparent;
  white-space: nowrap;
}

.path-node__chip--resolved {
  color: var(--color-resolved);
  border-color: color-mix(in srgb, var(--color-resolved) 45%, transparent);
}

.path-node__chip--active {
  color: var(--color-data-core);
  border-color: color-mix(in srgb, var(--color-data-core) 45%, transparent);
}

.path-node__chip--muted {
  color: var(--color-index-muted);
  border-color: var(--color-border-strong);
}

.path-node__handle {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--color-index-muted);
  border: 2px solid var(--color-canvas-ink);
}

@keyframes path-node-spin {
  to { transform: rotate(360deg); }
}

@media (prefers-reduced-motion: reduce) {
  .path-node { transition: none; }
  .path-node__spinner { animation-duration: 1.4s; }
}
</style>
