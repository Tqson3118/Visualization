<script setup lang="ts">
// EmptyState — motif "khung mảng [ ]" (Data Bench, DESIGN-IDENTITY §1.1):
// panel tối canvas-ink chứa 3 block — ô giữa rỗng (dashed) chờ dữ liệu, index mono
// bên dưới từng block (signature "index mono dưới block") + icon chủ đích.
// Giữ NGUYÊN API cũ (icon/title/description/actionLabel + emit action) — 57 chỗ gọi.
import Button from '@/components/ui/Button.vue';
import BaseIcon from './BaseIcon.vue';

withDefaults(
  defineProps<{
    icon?: string;
    title?: string;
    description?: string;
    actionLabel?: string;
  }>(),
  {
    icon: 'package',
    title: 'Chưa có dữ liệu',
    description: 'Chưa có nội dung ở đây — thực hiện thao tác đầu tiên để bắt đầu.',
    actionLabel: '',
  },
);

const emit = defineEmits<{
  action: [];
}>();
</script>

<template>
  <div
    class="empty-state flex flex-col items-center gap-4 px-4 py-14 text-center"
    role="status"
  >
    <!-- Khung mảng [ ] — block rỗng chờ dữ liệu (decorative) -->
    <div
      aria-hidden="true"
      class="flex flex-col items-center gap-2 rounded-lg border border-data-core/20 bg-canvas-ink px-6 py-4"
    >
      <div class="flex items-center gap-2">
        <span class="block h-8 w-8 rounded-sm border border-data-core/40 bg-data-core/10" />
        <span
          class="flex h-8 w-8 items-center justify-center rounded-sm border border-dashed border-data-core/60 bg-data-core/15 text-data-core"
        >
          <BaseIcon :name="icon" :size="18" />
        </span>
        <span class="block h-8 w-8 rounded-sm border border-data-core/40 bg-data-core/10" />
      </div>
      <div class="flex items-center gap-2 font-mono text-xs text-index-muted">
        <span class="w-8">0</span>
        <span class="w-8">1</span>
        <span class="w-8">2</span>
      </div>
    </div>

    <h3 class="text-base font-medium text-foreground">{{ title }}</h3>
    <p v-if="description" class="max-w-md text-sm text-foreground-secondary">{{ description }}</p>
    <Button v-if="actionLabel" size="md" @click="emit('action')">
      {{ actionLabel }}
    </Button>
  </div>
</template>

<style scoped>
/* Enter nhẹ: transform + opacity, easing chuẩn DESIGN.md §7 (200–300ms) */
.empty-state {
  animation: empty-state-enter 250ms cubic-bezier(0.16, 1, 0.3, 1) both;
}

@keyframes empty-state-enter {
  from {
    opacity: 0;
    transform: translateY(8px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .empty-state {
    animation: none;
  }
}
</style>
