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
          {{ editingTemplate ? 'Chỉnh sửa Starter Template' : 'Thêm Starter Template mới' }}
        </h2>
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <div class="form-field">
            <label class="form-label">Ngôn ngữ <span class="text-accent-red">*</span></label>
            <select v-model="form.language" class="form-select" required>
              <option value="">Chọn ngôn ngữ</option>
              <option value="csharp">C#</option>
              <option value="python">Python</option>
              <option value="java">Java</option>
              <option value="javascript">JavaScript</option>
              <option value="cpp">C++</option>
              <option value="go">Go</option>
            </select>
          </div>
          <div class="form-field">
            <label class="form-label">Starter Code <span class="text-accent-red">*</span></label>
            <textarea v-model="form.starterCode" class="form-input form-textarea" rows="8" required placeholder="Mã starter cho học viên..."></textarea>
          </div>
          <div class="flex justify-end gap-2 pt-2">
            <button type="button" class="btn-secondary" @click="close">Hủy</button>
            <button type="submit" class="btn-primary" :disabled="saving">
              <span v-if="saving" class="flex items-center gap-2">
                <span class="spinner-sm"></span> Đang lưu...
              </span>
              <span v-else>{{ editingTemplate ? 'Cập nhật' : 'Thêm' }}</span>
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
  editingTemplate: any | null;
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
  language: '',
  starterCode: '',
});

watch(() => props.show, (newShow) => {
  if (newShow && props.editingTemplate) {
    form.language = props.editingTemplate.language ?? '';
    form.starterCode = props.editingTemplate.starterCode ?? '';
  } else if (newShow) {
    form.language = '';
    form.starterCode = '';
  }
});

async function handleSubmit() {
  if (!form.language.trim() || !form.starterCode.trim()) {
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