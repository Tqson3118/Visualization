<template>
  <Transition name="modal-fade">
    <div v-if="show" class="modal-overlay" @click.self="emit('update:show', false)">
      <div class="modal-container" :class="variantClass">
        <div class="modal-header" :class="variantClass">
          <h3 class="modal-title">
            <BaseIcon :name="icon" :size="20" />
            {{ title }}
          </h3>
          <button type="button" class="modal-close" :aria-label="messages.common.close" @click="emit('update:show', false)">
            <BaseIcon name="x" :size="18" />
          </button>
        </div>

        <div class="modal-body">
          <p class="modal-message">{{ message }}</p>

          <div v-if="details" class="modal-details">
            {{ details }}
          </div>
        </div>

        <div class="modal-footer">
          <button
            type="button"
            class="btn-secondary"
            :disabled="loading"
            @click="emit('update:show', false)"
          >
            {{ cancelText }}
          </button>
          <button
            type="button"
            :class="['btn-primary', variantClass]"
            :disabled="loading"
            @click="handleConfirm"
          >
            <span v-if="loading" class="modal-loading">
              <span class="spinner-sm" />
              {{ messages.common.processing }}
            </span>
            <span v-else>{{ confirmText }}</span>
          </button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';

import { messages } from '@/i18n/vi';
import BaseIcon from './BaseIcon.vue';

// Bê từ source/VisualizationDSA1/frontend/src/components/ui/ConfirmModal.vue (V1).
// ĐIỀU CHỈNH: import BaseIcon sang components/ui, chuỗi cứng (Xác nhận/Hủy/Đang xử lý...)
// → i18n, màu → design tokens. Chưa nối vào uiStore.modalState — task tích hợp App.vue sau.

interface Props {
  show: boolean;
  title: string;
  message: string;
  details?: string;
  confirmText?: string;
  cancelText?: string;
  variant?: 'danger' | 'primary' | 'warning';
  icon?: string;
}

interface Emits {
  (e: 'update:show', value: boolean): void;
  (e: 'confirm'): void;
}

const props = withDefaults(defineProps<Props>(), {
  confirmText: messages.common.confirm,
  cancelText: messages.common.cancel,
  variant: 'primary',
  icon: 'alert-circle',
});

const emit = defineEmits<Emits>();
const loading = ref(false);

const variantClass = computed(() => {
  switch (props.variant) {
    case 'danger': return 'btn-danger';
    case 'warning': return 'btn-warning';
    default: return '';
  }
});

async function handleConfirm() {
  loading.value = true;
  try {
    emit('confirm');
  } finally {
    loading.value = false;
  }
}
</script>

<style scoped>
@import "./ConfirmModal.css";
</style>
