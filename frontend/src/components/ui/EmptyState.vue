<script setup lang="ts">
// EmptyState — giữ API cũ, restyle bằng tailwind/shadcn tokens (G-F1b).
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
    title: 'Không có dữ liệu',
    description: '',
    actionLabel: '',
  },
);

const emit = defineEmits<{
  action: [];
}>();
</script>

<template>
  <div
    class="flex flex-col items-center justify-center gap-3 px-4 py-14 text-center text-muted-foreground"
    role="status"
  >
    <div class="flex h-[72px] w-[72px] items-center justify-center rounded-full bg-muted text-foreground/80">
      <BaseIcon :name="icon" :size="36" />
    </div>
    <h3 class="text-base font-semibold text-foreground">{{ title }}</h3>
    <p v-if="description" class="max-w-2xl text-sm text-muted-foreground">{{ description }}</p>
    <button
      v-if="actionLabel"
      type="button"
      class="mt-2 inline-flex items-center justify-center rounded-md border border-primary px-5 py-2 text-sm font-semibold text-primary transition-colors hover:bg-accent"
      @click="emit('action')"
    >
      {{ actionLabel }}
    </button>
  </div>
</template>
