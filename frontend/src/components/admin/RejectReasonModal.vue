<script setup lang="ts">
import { computed, ref, watch } from 'vue';
import { AlertCircle } from 'lucide-vue-next';
import Modal from '@/components/ui/Modal.vue';
import Button from '@/components/ui/Button.vue';

const props = withDefaults(
  defineProps<{
    open: boolean;
    title?: string;
    targetTitle?: string;
    modelValue?: string;
    loading?: boolean;
    placeholder?: string;
  }>(),
  {
    title: 'Xác nhận từ chối nội dung',
    targetTitle: '',
    modelValue: '',
    loading: false,
    placeholder: 'Nhập lý do chi tiết để tác giả có thể chỉnh sửa và nộp lại...',
  },
);

const emit = defineEmits<{
  (e: 'update:modelValue', value: string): void;
  (e: 'confirm', reason: string): void;
  (e: 'close'): void;
}>();

const localReason = ref(props.modelValue);

watch(
  () => props.modelValue,
  (val) => {
    localReason.value = val || '';
  },
);

const textareaRef = ref<HTMLTextAreaElement | null>(null);

watch(
  () => props.open,
  (isOpen) => {
    if (isOpen) {
      localReason.value = props.modelValue || '';
      setTimeout(() => {
        textareaRef.value?.focus();
      }, 50);
    }
  },
);

function onInput(e: Event): void {
  const target = e.target as HTMLTextAreaElement;
  localReason.value = target.value;
  emit('update:modelValue', target.value);
}

function handleConfirm(): void {
  const reason = localReason.value.trim();
  if (!reason) return;
  emit('confirm', reason);
}

const isValid = computed(() => localReason.value.trim().length > 0);
</script>

<template>
  <Modal
    :open="open"
    :title="title"
    width="540px"
    @close="emit('close')"
  >
    <div class="space-y-4 py-2 text-sm text-foreground" data-testid="reject-modal">
      <div v-if="targetTitle" class="flex items-start gap-3 p-3 rounded-xl bg-amber-500/10 border border-amber-500/20 text-amber-300 text-xs">
        <AlertCircle :size="16" class="shrink-0 mt-0.5" />
        <div>
          <span class="font-bold text-amber-200">Đối tượng: </span>
          <span class="font-medium text-white">{{ targetTitle }}</span>
        </div>
      </div>

      <div class="space-y-1.5">
        <label class="block text-xs font-bold text-slate-300 uppercase tracking-wider">
          Lý do từ chối <span class="text-rose-400">*</span>
        </label>
        <textarea
          ref="textareaRef"
          :value="localReason"
          rows="4"
          data-testid="reject-reason-input"
          class="w-full bg-slate-900 border border-slate-700 rounded-xl p-3 text-xs text-white placeholder:text-slate-500 focus:outline-none focus:ring-2 focus:ring-rose-500/50 focus:border-rose-500 transition-all resize-y"
          :placeholder="placeholder"
          @input="onInput"
          @keydown.enter.ctrl="handleConfirm"
        />
        <p class="text-[11px] text-slate-400">
          Gợi ý: Chỉ ra cụ thể phần cần sửa (mô phỏng, quiz, nội dung lý thuyết...).
        </p>
      </div>
    </div>

    <template #footer>
      <div class="flex items-center justify-end gap-2 w-full">
        <Button variant="ghost" size="sm" :disabled="loading" data-testid="btn-cancel-reject" @click="emit('close')">
          Hủy
        </Button>
        <Button
          variant="danger"
          size="sm"
          data-testid="btn-confirm-reject"
          :disabled="!isValid || loading"
          :loading="loading"
          @click="handleConfirm"
        >
          Xác nhận từ chối
        </Button>
      </div>
    </template>
  </Modal>
</template>
