<script setup lang="ts">
// LadderStepper — thanh 3 bậc Quiz → Lab → Code (Màn 14, SDD §8.6)
export type StageStatus = 'locked' | 'active' | 'passed';

const props = defineProps<{
  stages: Array<{ key: string; label: string; status: StageStatus }>;
  activeKey?: string;
}>();

const emit = defineEmits<{
  select: [key: string];
}>();

const ICON: Record<StageStatus, string> = {
  locked: '🔒',
  active: '▶',
  passed: '✔',
};
</script>

<template>
  <ol class="ladder-stepper" aria-label="Các bậc luyện tập">
    <li
      v-for="(stage, idx) in stages"
      :key="stage.key"
      class="ladder-stepper__step"
      :class="{
        'ladder-stepper__step--active': stage.key === activeKey,
        'ladder-stepper__step--passed': stage.status === 'passed',
        'ladder-stepper__step--locked': stage.status === 'locked',
      }"
    >
      <button
        type="button"
        class="ladder-stepper__btn"
        :disabled="stage.status === 'locked'"
        @click="emit('select', stage.key)"
      >
        <span class="ladder-stepper__num">{{ idx + 1 }}</span>
        <span class="ladder-stepper__label">{{ stage.label }}</span>
        <span class="ladder-stepper__icon" aria-hidden="true">{{ ICON[stage.status] }}</span>
      </button>
    </li>
  </ol>
</template>

<style scoped>
.ladder-stepper {
  list-style: none;
  display: flex;
  gap: var(--space-sm);
  padding: var(--space-sm);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  overflow-x: auto;
}

.ladder-stepper__step { flex: 1; min-width: 120px; }

.ladder-stepper__btn {
  width: 100%;
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-sm);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text-muted);
  cursor: pointer;
  font-weight: 700;
  font-size: var(--text-sm);
  transition: var(--transition-fast);
}

.ladder-stepper__step--active .ladder-stepper__btn {
  border-color: var(--color-primary);
  background: color-mix(in srgb, var(--color-primary) 10%, transparent);
  color: var(--color-primary);
}

.ladder-stepper__step--passed .ladder-stepper__btn { color: var(--color-success); }

.ladder-stepper__step--locked .ladder-stepper__btn { opacity: 0.5; cursor: not-allowed; }

.ladder-stepper__num {
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: var(--color-muted);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: var(--text-xs);
  flex-shrink: 0;
}

.ladder-stepper__label { flex: 1; text-align: left; }

.ladder-stepper__icon { font-size: var(--text-sm); }
</style>
