<script setup lang="ts">
// Input — component UI chung: label + error + icon (design tokens — SDD §8.1)
import { useId } from 'vue';

import BaseIcon from './BaseIcon.vue';

const props = withDefaults(
  defineProps<{
    id?: string;
    label?: string;
    error?: string;
    hint?: string;
    icon?: string;
    type?: string;
    modelValue?: string | number | null;
    placeholder?: string;
    disabled?: boolean;
    autocomplete?: string;
    required?: boolean;
    maxlength?: number;
  }>(),
  {
    type: 'text',
    modelValue: '',
    placeholder: '',
    disabled: false,
    required: false,
  },
);

const emit = defineEmits<{
  'update:modelValue': [value: string];
  blur: [];
}>();

const autoId = useId();
const inputId = props.id ?? `ui-input-${autoId}`;

function onInput(event: Event): void {
  emit('update:modelValue', (event.target as HTMLInputElement).value);
}
</script>

<template>
  <div class="ui-input" :class="{ 'ui-input--error': !!error }">
    <label v-if="label" class="ui-input__label" :for="inputId">
      {{ label }}<span v-if="required" class="ui-input__required" aria-hidden="true"> *</span>
    </label>
    <div class="ui-input__wrap">
      <BaseIcon v-if="icon" :name="icon" :size="16" class="ui-input__icon" />
      <input
        :id="inputId"
        class="ui-input__control"
        :type="type"
        :value="modelValue ?? ''"
        :placeholder="placeholder"
        :disabled="disabled"
        :autocomplete="autocomplete"
        :maxlength="maxlength"
        :aria-invalid="!!error"
        @input="onInput"
        @blur="emit('blur')"
      />
    </div>
    <p v-if="error" class="ui-input__error" role="alert">{{ error }}</p>
    <p v-else-if="hint" class="ui-input__hint">{{ hint }}</p>
  </div>
</template>

<style scoped>
.ui-input__label {
  display: block;
  font-size: var(--text-sm);
  font-weight: 600;
  margin-bottom: var(--space-xs);
}

.ui-input__required { color: var(--color-destructive); }

.ui-input__wrap { position: relative; }

.ui-input__icon {
  position: absolute;
  left: var(--space-sm);
  top: 50%;
  transform: translateY(-50%);
  color: var(--color-text-muted);
  pointer-events: none;
}

.ui-input__control {
  width: 100%;
  padding: 0.6rem var(--space-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  font-size: var(--text-base);
  background: var(--color-surface);
  color: var(--color-foreground);
  transition: border-color 200ms ease, box-shadow 200ms ease;
}

.ui-input__wrap:has(.ui-input__icon) .ui-input__control { padding-left: 2.25rem; }

.ui-input__control:focus {
  border-color: var(--color-primary);
  outline: none;
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-primary) 20%, transparent);
}

.ui-input__control:disabled { opacity: 0.6; cursor: not-allowed; }

.ui-input--error .ui-input__control { border-color: var(--color-destructive); }

.ui-input__error { color: var(--color-destructive); font-size: var(--text-sm); margin-top: var(--space-xs); }
.ui-input__hint { color: var(--color-text-muted); font-size: var(--text-xs); margin-top: var(--space-xs); }
</style>
