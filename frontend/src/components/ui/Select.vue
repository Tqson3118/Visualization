<script setup lang="ts">
// Select — wrapper giữ API cũ (G-F1b): label + options + modelValue + placeholder.
// Render bằng shadcn-vue Select (reka-ui).
import { useId } from 'vue';

import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from './select';

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

function onChange(value: unknown): void {
  emit('update:modelValue', value === null || value === undefined ? '' : String(value));
}
</script>

<template>
  <div class="w-full">
    <label v-if="label" class="mb-1.5 block text-sm font-semibold" :for="selectId">
      {{ label }}
    </label>
    <Select
      :model-value="modelValue === null || modelValue === '' ? undefined : String(modelValue)"
      :disabled="disabled"
      @update:model-value="onChange"
    >
      <SelectTrigger :id="selectId" class="w-full">
        <SelectValue :placeholder="placeholder" />
      </SelectTrigger>
      <SelectContent>
        <SelectGroup>
          <SelectItem v-for="opt in options" :key="String(opt.value)" :value="String(opt.value)">
            {{ opt.label }}
          </SelectItem>
        </SelectGroup>
      </SelectContent>
    </Select>
  </div>
</template>
