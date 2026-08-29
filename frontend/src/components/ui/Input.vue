<script setup lang="ts">
// Input — wrapper giữ API cũ (G-F1b): label + error + hint + icon, control là shadcn Input.
// View-quality: icon nhận thêm Component (lucide-vue-next) — string legacy vẫn qua BaseIcon.
import { useId } from 'vue';
import type { Component } from 'vue';

import { cn } from '@/lib/utils';
import BaseIcon from './BaseIcon.vue';
import Input from './input/Input.vue';

const props = withDefaults(
  defineProps<{
    id?: string;
    label?: string;
    error?: string;
    hint?: string;
    icon?: string | Component;
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

function onUpdate(value: string | number | null): void {
  emit('update:modelValue', value === null ? '' : String(value));
}

function onNativeInput(event: Event): void {
  onUpdate((event.target as HTMLInputElement).value);
}
</script>

<template>
  <div class="ui-input w-full">
    <label
      v-if="label"
      class="mb-1.5 block text-sm font-semibold"
      :for="inputId"
    >
      {{ label }}<span v-if="required" class="text-destructive" aria-hidden="true"> *</span>
    </label>
    <div class="relative">
      <BaseIcon
        v-if="typeof icon === 'string' && icon"
        :name="icon"
        :size="16"
        class="ui-input__icon pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
      />
      <component
        :is="icon"
        v-else-if="icon"
        :size="16"
        aria-hidden="true"
        class="ui-input__icon pointer-events-none absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground"
      />
      <Input
        :id="inputId"
        :type="type"
        :model-value="modelValue ?? ''"
        :placeholder="placeholder"
        :disabled="disabled"
        :autocomplete="autocomplete"
        :maxlength="maxlength"
        :class="cn(
          'w-full',
          icon && 'pl-9',
          error ? 'border-destructive focus-visible:ring-destructive/50' : '',
        )"
        :aria-invalid="!!error"
        @update:model-value="onUpdate"
        @input="onNativeInput"
        @blur="emit('blur')"
      />
    </div>
    <p v-if="error" class="mt-1 text-sm text-destructive" role="alert">{{ error }}</p>
    <p v-else-if="hint" class="mt-1 text-xs text-muted-foreground">{{ hint }}</p>
  </div>
</template>
