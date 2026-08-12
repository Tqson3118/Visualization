<script setup lang="ts">
// InputModal — modal cấu hình đầu vào theo inputSchema của generator (FR-3.4)
// Render field động theo type: int / int[] / string[] / select / bool + validate + nút "Áp dụng"
import { computed, reactive, ref, watch } from 'vue';

import type { InputConfig, InputSchema } from '@/engines/core/types';
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import Modal from '@/components/ui/Modal.vue';

const props = withDefaults(
  defineProps<{
    open: boolean;
    schema: InputSchema | null;
    /** Input hiện tại (để khôi phục) */
    current?: InputConfig | null;
    validate?: (input: InputConfig) => { ok: boolean; errors: string[] };
    loading?: boolean;
  }>(),
  {
    schema: null,
    current: null,
    validate: undefined,
    loading: false,
  },
);

const emit = defineEmits<{
  close: [];
  apply: [input: InputConfig];
}>();

const form = reactive<Record<string, unknown>>({});
const errors = ref<Record<string, string>>({});

watch(
  () => [props.open, props.schema, props.current] as const,
  () => {
    if (!props.open || !props.schema) return;
    // Khôi phục giá trị: dùng current nếu có (khớp kind), ngược lại default schema
    const source =
      props.current && props.current.kind === props.schema.kind ? props.current.data : {};
    for (const field of props.schema.fields) {
      const existing = (source as Record<string, unknown>)[field.name];
      form[field.name] = existing !== undefined ? existing : field.default;
    }
    errors.value = {};
  },
  { immediate: true },
);

const boolFields = computed(() => props.schema?.fields.filter((f) => f.type === 'bool') ?? []);

function parseArrayInput(raw: string, type: 'int[]' | 'string[]'): unknown {
  if (type === 'int[]') {
    return raw
      .split(/[,\s]+/)
      .filter((part) => part.trim() !== '')
      .map((part) => Number(part.trim()));
  }
  return raw.split(',').map((part) => part.trim());
}

function arrayPlaceholder(type: 'int[]' | 'string[]'): string {
  return type === 'int[]' ? '5, 3, 8, 1, 9 (phân cách dấu phẩy)' : 'a, b, c (phân cách dấu phẩy)';
}

function onSubmit(): void {
  if (!props.schema) return;
  const data: Record<string, unknown> = {};
  const nextErrors: Record<string, string> = {};
  for (const field of props.schema.fields) {
    let value = form[field.name];
    if (typeof value === 'string') {
      value = value.trim();
      if (field.type === 'int') {
        const num = Number(value);
        if (value === '' || !Number.isFinite(num)) {
          nextErrors[field.name] = `Vui lòng nhập số nguyên cho "${field.label}"`;
          continue;
        }
        value = Math.round(num);
      } else if (field.type === 'int[]' || field.type === 'string[]') {
        value = parseArrayInput(String(value), field.type);
      }
    }
    data[field.name] = value;
  }
  if (Object.keys(nextErrors).length > 0) {
    errors.value = nextErrors;
    return;
  }
  const input: InputConfig = { kind: props.schema.kind, data };
  if (props.validate) {
    const result = props.validate(input);
    if (!result.ok) {
      errors.value = { _form: result.errors.join('; ') };
      return;
    }
  }
  emit('apply', input);
}
</script>

<template>
  <Modal
    :open="open"
    :title="messages.simulator.inputConfig"
    :closable="!loading"
    width="520px"
    @close="emit('close')"
  >
    <form v-if="schema" class="input-modal" novalidate @submit.prevent="onSubmit">
      <p v-if="errors._form" class="input-modal__form-error" role="alert">{{ errors._form }}</p>

      <template v-for="field in schema.fields" :key="field.name">
        <div v-if="field.type === 'select'" class="input-modal__field">
          <label class="label" :for="`inp-${field.name}`">{{ field.label }}</label>
          <select
            :id="`inp-${field.name}`"
            v-model="form[field.name]"
            class="input"
          >
            <option
              v-for="opt in field.options ?? []"
              :key="String(opt.value)"
              :value="String(opt.value)"
            >
              {{ opt.label }}
            </option>
          </select>
          <p v-if="field.description" class="input-modal__desc">{{ field.description }}</p>
        </div>

        <div v-else-if="field.type === 'bool'" class="input-modal__field">
          <label class="input-modal__checkbox" :for="`inp-${field.name}`">
            <input
              :id="`inp-${field.name}`"
              v-model="form[field.name]"
              type="checkbox"
              :checked="form[field.name] === true"
            />
            {{ field.label }}
          </label>
          <p v-if="field.description" class="input-modal__desc">{{ field.description }}</p>
        </div>

        <div v-else-if="field.type === 'int[]' || field.type === 'string[]'" class="input-modal__field">
          <label class="label" :for="`inp-${field.name}`">{{ field.label }}</label>
          <input
            :id="`inp-${field.name}`"
            class="input"
            :value="Array.isArray(form[field.name]) ? (form[field.name] as unknown[]).join(', ') : ''"
            :placeholder="arrayPlaceholder(field.type)"
            @input="form[field.name] = ($event.target as HTMLInputElement).value"
          />
          <p v-if="field.description" class="input-modal__desc">{{ field.description }}</p>
          <p v-if="errors[field.name]" class="input-modal__error" role="alert">{{ errors[field.name] }}</p>
        </div>

        <div v-else class="input-modal__field">
          <label class="label" :for="`inp-${field.name}`">{{ field.label }}</label>
          <input
            :id="`inp-${field.name}`"
            class="input"
            type="number"
            :value="form[field.name] === undefined ? '' : String(form[field.name])"
            :min="field.min"
            :max="field.max"
            @input="form[field.name] = ($event.target as HTMLInputElement).value"
          />
          <p v-if="field.description" class="input-modal__desc">{{ field.description }}</p>
          <p v-if="errors[field.name]" class="input-modal__error" role="alert">{{ errors[field.name] }}</p>
        </div>
      </template>

      <div class="input-modal__actions">
        <Button variant="ghost" :disabled="loading" @click="emit('close')">
          {{ messages.common.cancel }}
        </Button>
        <Button type="submit" :loading="loading">
          {{ messages.simulator.play }}
        </Button>
      </div>
    </form>
  </Modal>
</template>

<style scoped>
.input-modal { display: flex; flex-direction: column; gap: var(--space-md); }

.input-modal__field { display: flex; flex-direction: column; gap: var(--space-xs); }

.input-modal__desc { font-size: var(--text-xs); color: var(--color-text-muted); }

.input-modal__error { color: var(--color-destructive); font-size: var(--text-sm); }

.input-modal__form-error {
  color: var(--color-destructive);
  background: color-mix(in srgb, var(--color-destructive) 8%, transparent);
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
  font-size: var(--text-sm);
}

.input-modal__checkbox {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  font-weight: 600;
  cursor: pointer;
}

.input-modal__actions {
  display: flex;
  justify-content: flex-end;
  gap: var(--space-sm);
  padding-top: var(--space-sm);
  border-top: 1px solid var(--color-border);
}
</style>
