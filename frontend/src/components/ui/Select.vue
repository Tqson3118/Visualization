<script setup lang="ts">
// Select — component UI chung: label + options (design tokens — SDD §8.1)
import { useId } from 'vue';

export interface SelectOption {
  label: string;
  value: string | number;
}

const props = withDefaults(
  defineProps<{
    id?: string;
    label?: string;
    options: SelectOption[];
    modelValue?: string | number | null;
    placeholder?: string;
    disabled?: boolean;
  }>(),
  {
    modelValue: '',
    placeholder: '',
    disabled: false,
  },
);

const emit = defineEmits<{
  'update:modelValue': [value: string];
}>();

const autoId = useId();
const selectId = props.id ?? `ui-select-${autoId}`;

function onChange(event: Event): void {
  emit('update:modelValue', (event.target as HTMLSelectElement).value);
}
</script>

<template>
  <div class="ui-select">
    <label v-if="label" class="ui-select__label" :for="selectId">{{ label }}</label>
    <select
      :id="selectId"
      class="ui-select__control"
      :value="modelValue ?? ''"
      :disabled="disabled"
      @change="onChange"
    >
      <option v-if="placeholder" value="" disabled>{{ placeholder }}</option>
      <option v-for="opt in options" :key="String(opt.value)" :value="String(opt.value)">
        {{ opt.label }}
      </option>
    </select>
  </div>
</template>

<style scoped>
.ui-select__label {
  display: block;
  font-size: var(--text-sm);
  font-weight: 600;
  margin-bottom: var(--space-xs);
}

.ui-select__control {
  width: 100%;
  padding: 0.6rem var(--space-md);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  font-size: var(--text-base);
  background: var(--color-surface);
  color: var(--color-foreground);
  cursor: pointer;
  transition: border-color 200ms ease, box-shadow 200ms ease;
}

.ui-select__control:focus {
  border-color: var(--color-primary);
  outline: none;
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-primary) 20%, transparent);
}

.ui-select__control:disabled { opacity: 0.6; cursor: not-allowed; }
</style>
