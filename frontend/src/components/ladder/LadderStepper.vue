<script setup lang="ts">
// LadderStepper — thanh 3 bậc Quiz → Lab → Code (Màn 14, SDD §8.6)
// G-F2c: badge trạng thái + đường nối giữa các bậc + số tròn gradient.
// GIỮ nguyên: <ol aria-label="Các bậc luyện tập"> + nút Quiz/Lab/Code + disabled khi locked (e2e).
import { Lock, Check, Play } from 'lucide-vue-next';
import Badge from '@/components/ui/Badge.vue';

export type StageStatus = 'locked' | 'active' | 'passed';

const props = defineProps<{
  stages: Array<{ key: string; label: string; status: StageStatus }>;
  activeKey?: string;
}>();

const emit = defineEmits<{
  select: [key: string];
}>();

const STATUS_BADGE: Record<StageStatus, { text: string; variant: 'primary' | 'success' | 'muted' }> = {
  active: { text: 'Đang học', variant: 'primary' },
  passed: { text: 'Đã qua', variant: 'success' },
  locked: { text: 'Khóa', variant: 'muted' },
};

function badgeFor(status: StageStatus): { text: string; variant: 'primary' | 'success' | 'muted' } {
  return STATUS_BADGE[status];
}
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
        <span class="ladder-stepper__num" aria-hidden="true">
          <Lock v-if="stage.status === 'locked'" :size="13" />
          <Check v-else-if="stage.status === 'passed'" :size="13" />
          <Play v-else :size="13" />
        </span>
        <span class="ladder-stepper__label">
          <span class="ladder-stepper__label-text">{{ stage.label }}</span>
          <Badge :variant="badgeFor(stage.status).variant" class="ladder-stepper__status">
            {{ badgeFor(stage.status).text }}
          </Badge>
        </span>
      </button>
    </li>
  </ol>
</template>

<style scoped>
.ladder-stepper {
  list-style: none;
  display: flex;
  gap: var(--space-md);
  padding: var(--space-md);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  overflow-x: auto;
  box-shadow: var(--shadow-sm);
}

.ladder-stepper__step {
  flex: 1;
  min-width: 140px;
  position: relative;
}

/* Đường nối giữa các bậc (trừ bậc cuối) */
.ladder-stepper__step:not(:last-child)::after {
  content: '';
  position: absolute;
  top: 50%;
  right: -14px;
  width: var(--space-md);
  height: 2px;
  border-radius: 2px;
  background: var(--color-border);
  transform: translateY(-50%);
}

.ladder-stepper__step--passed:not(:last-child)::after {
  background: var(--color-success);
}

.ladder-stepper__step--active:not(:last-child)::after {
  background: color-mix(in srgb, var(--color-primary) 45%, var(--color-border));
}

.ladder-stepper__btn {
  width: 100%;
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-surface);
  color: var(--color-text-muted);
  cursor: pointer;
  font-weight: 700;
  font-size: var(--text-sm);
  transition: var(--transition-fast);
}

.ladder-stepper__btn:hover:not(:disabled) {
  border-color: var(--color-primary);
  transform: translateY(-1px);
  box-shadow: var(--shadow-sm);
}

.ladder-stepper__step--active .ladder-stepper__btn {
  border-color: var(--color-primary);
  background: color-mix(in srgb, var(--color-primary) 10%, transparent);
  color: var(--color-primary);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-primary) 14%, transparent);
}

.ladder-stepper__step--passed .ladder-stepper__btn {
  border-color: color-mix(in srgb, var(--color-success) 40%, var(--color-border));
  color: var(--color-success);
}

.ladder-stepper__step--locked .ladder-stepper__btn {
  opacity: 0.55;
  cursor: not-allowed;
}

.ladder-stepper__num {
  width: 30px;
  height: 30px;
  border-radius: 50%;
  background: var(--color-muted);
  display: inline-flex;
  align-items: center;
  justify-content: center;
  font-size: var(--text-xs);
  flex-shrink: 0;
  color: var(--color-text-muted);
}

.ladder-stepper__step--active .ladder-stepper__num {
  background-image: var(--gradient-mint);
  color: var(--color-on-primary);
  box-shadow: var(--shadow-sm);
}

.ladder-stepper__step--passed .ladder-stepper__num {
  background: color-mix(in srgb, var(--color-success) 18%, transparent);
  color: var(--color-success);
}

.ladder-stepper__step--locked .ladder-stepper__num {
  background: var(--color-muted);
  color: var(--color-text-disabled);
}

.ladder-stepper__label {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
}

.ladder-stepper__label-text { white-space: nowrap; }

.ladder-stepper__status { font-size: 10px; padding: 1px 8px; }
</style>
