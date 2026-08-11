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
          {{ editingHint ? 'Chỉnh sửa Hint' : 'Thêm Hint mới' }}
        </h2>
        <form @submit.prevent="handleSubmit" class="space-y-4">
          <div class="form-field">
            <label class="form-label">Nội dung gợi ý <span class="text-accent-red">*</span></label>
            <textarea v-model="form.content" class="form-input form-textarea" rows="4" required placeholder="Gợi ý cho học viên..."></textarea>
          </div>
          <div class="form-field">
            <label class="flex items-center gap-2 cursor-pointer">
              <input type="checkbox" v-model="form.isTiered" class="form-checkbox" />
              <span class="text-text-secondary">Gợi ý tiered (tốn XP)</span>
            </label>
          </div>
          <div v-if="form.isTiered" class="form-field">
            <label class="form-label">XP cost <span class="text-accent-red">*</span></label>
            <input v-model.number="form.xpCost" type="number" class="form-input" min="1" max="50" required />
          </div>
          <div class="flex justify-end gap-2 pt-2">
            <button type="button" class="btn-secondary" @click="close">Hủy</button>
            <button type="submit" class="btn-primary" :disabled="saving">
              <span v-if="saving" class="flex items-center gap-2">
                <span class="spinner-sm"></span> Đang lưu...
              </span>
              <span v-else>{{ editingHint ? 'Cập nhật' : 'Thêm' }}</span>
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
  editingHint: any | null;
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
  content: '',
  isTiered: false,
  xpCost: 5,
});

watch(() => props.show, (newShow) => {
  if (newShow && props.editingHint) {
    form.content = props.editingHint.content ?? '';
    form.isTiered = props.editingHint.isTiered ?? false;
    form.xpCost = props.editingHint.xpCost ?? 5;
  } else if (newShow) {
    form.content = '';
    form.isTiered = false;
    form.xpCost = 5;
  }
});

async function handleSubmit() {
  if (!form.content.trim()) {
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