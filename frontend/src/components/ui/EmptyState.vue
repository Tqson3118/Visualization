<script setup lang="ts">
// EmptyState — component UI chung: trạng thái rỗng + icon + CTA (design tokens — SDD §8.1)
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
  <div class="ui-empty" role="status">
    <div class="ui-empty__icon">
      <BaseIcon :name="icon" :size="36" />
    </div>
    <h3 class="ui-empty__title">{{ title }}</h3>
    <p v-if="description" class="ui-empty__desc">{{ description }}</p>
    <button v-if="actionLabel" type="button" class="ui-empty__action" @click="emit('action')">
      {{ actionLabel }}
    </button>
  </div>
</template>

<style scoped>
.ui-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--space-sm);
  padding: var(--space-2xl) var(--space-md);
  text-align: center;
  color: var(--color-text-muted);
}

.ui-empty__icon {
  color: var(--color-text-disabled);
  background: var(--color-muted);
  width: 72px;
  height: 72px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
}

.ui-empty__title { font-size: var(--text-md); color: var(--color-foreground); }

.ui-empty__desc { max-width: 36rem; font-size: var(--text-sm); }

.ui-empty__action {
  margin-top: var(--space-sm);
  background: none;
  border: 1px solid var(--color-primary);
  color: var(--color-primary);
  padding: 0.5rem 1.25rem;
  border-radius: var(--radius-md);
  font-weight: 600;
  cursor: pointer;
  transition: var(--transition-fast);
}
.ui-empty__action:hover { background: var(--color-surface-hover); }
</style>
