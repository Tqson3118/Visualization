<script setup lang="ts">
// VisualizerTraceDrawer — trace drawer: danh sách các step/frame để nhảy tới.
import type { SharedVisualFrame } from '@/visualizer/types';

defineProps<{
  frames: SharedVisualFrame[];
  currentIndex: number;
}>();

const emit = defineEmits<{ select: [index: number] }>();
</script>

<template>
  <div class="trace-drawer" data-testid="trace-drawer">
    <ol class="trace-drawer__list">
      <li v-for="(f, i) in frames" :key="f.stepIndex + '-' + i">
        <button
          type="button"
          class="trace-row"
          :class="{ 'trace-row--active': i === currentIndex }"
          :aria-current="i === currentIndex ? 'step' : undefined"
          @click="emit('select', i)"
        >
          <span class="trace-row__num">{{ i + 1 }}</span>
          <span class="trace-row__desc">{{ f.description }}</span>
          <span v-if="f.status" class="trace-row__status" :class="`status--${f.status}`" aria-hidden="true">
            {{ f.status }}
          </span>
        </button>
      </li>
    </ol>
  </div>
</template>

<style scoped>
.trace-drawer {
  max-height: 240px;
  overflow-y: auto;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
  background: var(--color-muted);
}

.trace-drawer__list {
  list-style: none;
  margin: 0;
  padding: var(--space-xs);
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.trace-row {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  width: 100%;
  text-align: left;
  padding: 6px 8px;
  border: 1px solid transparent;
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--color-text-primary);
  font-size: var(--text-xs);
  cursor: pointer;
}

.trace-row:hover { background: var(--color-surface-hover); }
.trace-row--active { background: var(--color-surface); border-color: var(--color-border); }

.trace-row__num {
  font-family: var(--font-mono);
  font-size: 10px;
  color: var(--color-text-tertiary);
  min-width: 22px;
  text-align: right;
}

.trace-row__desc { flex: 1; min-width: 0; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

.trace-row__status {
  font-family: var(--font-mono);
  font-size: 9px;
  text-transform: uppercase;
  color: var(--color-text-tertiary);
}
.status--comparing { color: var(--color-primary); }
.status--swapping { color: var(--color-conflict); }
.status--visited { color: var(--color-accent); }
.status--done { color: var(--color-resolved); }
.status--error { color: var(--color-destructive); }
</style>
