<template>
  <div v-if="show" class="fixed inset-0 bg-black/60 backdrop-blur-sm flex items-center justify-center p-4 z-50 animate-fade-in" @click.self="$emit('close')">
    <div class="glass-panel rounded-3xl p-8 max-w-md w-full text-center relative overflow-hidden border border-border-default">
      <button
        class="absolute top-4 right-4 w-8 h-8 flex items-center justify-center rounded-full bg-bg-hover text-text-secondary hover:text-text-primary transition-colors cursor-pointer"
        @click="$emit('close')"
        aria-label="Đóng"
      >
        <BaseIcon name="close" class="w-4 h-4" />
      </button>

      <div class="w-16 h-16 rounded-full bg-accent-warm/20 border border-accent-warm/30 flex items-center justify-center text-accent-warm mx-auto mb-4">
        <BaseIcon name="star" class="w-8 h-8 fill-current" />
      </div>

      <h3 class="text-2xl font-black text-text-heading">Đánh giá roadmap</h3>
      <p class="text-text-secondary mt-2 text-sm">Bạn đã hoàn thành roadmap này. Hãy chia sẻ trải nghiệm của bạn với cộng đồng!</p>

      <div class="my-6 flex items-center justify-center gap-2">
        <button
          v-for="i in 5"
          :key="i"
          type="button"
          class="transition-all duration-200 cursor-pointer p-1"
          :aria-label="`Đánh giá ${i} sao`"
          @mouseenter="hoverRating = i"
          @mouseleave="hoverRating = 0"
          @click="selectedRating = i"
        >
          <BaseIcon
            name="star"
            class="w-10 h-10 transition-all duration-200"
            :class="(hoverRating || selectedRating) >= i ? 'text-accent-yellow fill-current scale-110 drop-shadow-[0_0_8px_rgba(255,205,60,0.5)]' : 'text-text-disabled'"
          />
        </button>
      </div>

      <p class="text-xs font-semibold text-text-muted mb-6 h-5" aria-live="polite">
        {{ selectedRating ? `Bạn chọn ${selectedRating} sao` : 'Chọn số sao bạn muốn đánh giá' }}
      </p>

      <div class="flex flex-col gap-3">
        <button
          :disabled="selectedRating === 0 || submitting"
          class="w-full py-3 bg-gradient-to-r from-accent to-accent-purple text-white font-bold rounded-2xl transition-all duration-300 cursor-pointer disabled:opacity-40 disabled:cursor-not-allowed flex items-center justify-center gap-2"
          @click="submit"
        >
          <BaseIcon v-if="submitting" name="spinner" class="w-4 h-4 animate-spin" />
          <span>{{ submitting ? 'Đang gửi...' : 'Gửi đánh giá' }}</span>
        </button>
        <button
          @click="$emit('close')"
          class="w-full py-3 bg-bg-hover text-text-secondary font-bold rounded-2xl transition-all border border-border-default cursor-pointer hover:text-text-primary"
        >
          Để sau
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import BaseIcon from '@/shared/components/BaseIcon.vue';
import { roadmapApi } from '@/services/roadmapApi';
import { useToastStore } from '@/composables/useToast';

const props = defineProps<{
  show: boolean;
  roadmapId: string;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
  (e: 'submitted', rating: number): void;
}>();

const toastStore = useToastStore();

const selectedRating = ref(0);
const hoverRating = ref(0);
const submitting = ref(false);

watch(
  () => props.show,
  (val) => {
    if (val) {
      selectedRating.value = 0;
      hoverRating.value = 0;
    }
  },
);

async function submit(): Promise<void> {
  if (selectedRating.value < 1 || selectedRating.value > 5 || submitting.value) return;

  submitting.value = true;
  try {
    await roadmapApi.submitReview(props.roadmapId, selectedRating.value);
    toastStore.success('Cảm ơn bạn đã đánh giá roadmap!');
    emit('submitted', selectedRating.value);
    emit('close');
  } catch (err) {
    if (err instanceof Error) {
      toastStore.error(err.message);
    } else {
      const e = err as { error?: string; message?: string };
      if (e?.error === 'ALREADY_REVIEWED') {
        toastStore.error('Bạn đã đánh giá roadmap này rồi.', 'Đã đánh giá');
      } else if (e?.error === 'ROADMAP_NOT_COMPLETED') {
        toastStore.error('Bạn cần hoàn thành roadmap trước khi đánh giá.');
      } else {
        toastStore.error(e?.message ?? 'Không thể gửi đánh giá. Vui lòng thử lại.');
      }
    }
  } finally {
    submitting.value = false;
  }
}
</script>

<style scoped>
.animate-fade-in {
  animation: fadeIn 0.3s cubic-bezier(0.16, 1, 0.3, 1) forwards;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(12px) scale(0.97);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}
</style>
