<template>
  <Transition name="modal">
    <div
      v-if="show"
      class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-sm"
      @click.self="close"
    >
      <div class="bg-bg-secondary rounded-xl border border-border-subtle shadow-2xl max-w-md w-full p-6">
        <h2 class="text-xl font-bold text-text-primary mb-2">
          <BaseIcon name="tool" class="w-4 h-4 inline-block mr-1 align-text-bottom" />
          {{ editingTestcase ? 'Chỉnh sửa Testcase' : 'Thêm Testcase mới' }}
        </h2>
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <div class="form-field">
            <label class="form-label">Input <span class="text-accent-red">*</span></label>
            <textarea v-model="form.input" class="form-input form-textarea" rows="3" required placeholder="VD: [5,2,9,1,5,6]"></textarea>
          </div>
          <div class="form-field">
            <label class="form-label">Expected Output <span class="text-accent-red">*</span></label>
            <textarea v-model="form.expectedOutput" class="form-input form-textarea" rows="3" required placeholder="VD: [1,2,5,5,6,9]"></textarea>
          </div>
          <div class="form-field">
            <label class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" v-model="form.isHidden" class="form-checkbox" />
              <span class="text-text-secondary">Ẩn testcase (ẩn với học viên)</span>
            </label>
          </div>
          <div class="flex justify-end gap-2 pt-2">
            <button type="button" class="btn-secondary" @click="close">Hủy</button>
            <button type="submit" class="btn-primary" :disabled="saving">
              <span v-if="saving" class="flex items-center gap-2">
                <span class="spinner-sm"></span> Đang lưu...
              </span>
              <span v-else>{{ editingTestcase ? 'Cập nhật' : 'Thêm' }}</span>
            </button>
          </div>
        </form>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { ref, reactive, watch } from 'vue';
import BaseIcon from '@/shared/components/BaseIcon.vue';

interface Props {
  show: boolean;
  editingTestcase: any | null;
  parentCodelab: any | null;
}

interface Emits {
  (e: 'update:show', value: boolean): void;
  (e: 'save', data: any): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const saving = ref(false);

const form = reactive({
  input: '',
  expectedOutput: '',
  isHidden: false,
});

watch(() => props.show, (newShow) => {
  if (newShow && props.editingTestcase) {
    form.input = props.editingTestcase.input ?? '';
    form.expectedOutput = props.editingTestcase.expectedOutput ?? '';
    form.isHidden = props.editingTestcase.isHidden ?? false;
  } else if (newShow) {
    form.input = '';
    form.expectedOutput = '';
    form.isHidden = false;
  }
});

async function handleSubmit() {
  if (!form.input.trim() || !form.expectedOutput.trim()) {
    return;
  }
  saving.value = true;
  try {
    await emit('save', { ...form });
    close();
  } finally {
    saving.value = false;
  }
}

function close() {
  emit('update:show', false);
}
</script>