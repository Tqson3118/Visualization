<script setup lang="ts">
// Tabs — component UI chung: danh sách tab + slot content (design tokens — SDD §8.1)
import { computed } from 'vue';

export interface TabItem {
  key: string;
  label: string;
  badge?: number | string;
  disabled?: boolean;
}

const props = withDefaults(
  defineProps<{
    tabs: TabItem[];
    modelValue?: string;
  }>(),
  {
    modelValue: '',
  },
);

const emit = defineEmits<{
  'update:modelValue': [key: string];
  change: [key: string];
}>();

const activeKey = computed(() => props.modelValue || props.tabs[0]?.key || '');

function select(tab: TabItem): void {
  if (tab.disabled || tab.key === activeKey.value) return;
  emit('update:modelValue', tab.key);
  emit('change', tab.key);
}
</script>

<template>
  <div class="ui-tabs">
    <div class="ui-tabs__bar" role="tablist">
      <button
        v-for="tab in tabs"
        :key="tab.key"
        type="button"
        role="tab"
        class="ui-tabs__tab"
        :class="{ 'ui-tabs__tab--active': tab.key === activeKey, 'ui-tabs__tab--disabled': tab.disabled }"
        :aria-selected="tab.key === activeKey"
        :disabled="tab.disabled"
        @click="select(tab)"
      >
        {{ tab.label }}
        <span v-if="tab.badge !== undefined" class="ui-tabs__badge">{{ tab.badge }}</span>
      </button>
    </div>
    <div class="ui-tabs__content">
      <slot />
    </div>
  </div>
</template>

<style scoped>
.ui-tabs__bar {
  display: flex;
  gap: var(--space-xs);
  border-bottom: 2px solid var(--color-border);
  overflow-x: auto;
}

.ui-tabs__tab {
  background: none;
  border: none;
  border-bottom: 2px solid transparent;
  padding: var(--space-sm) var(--space-md);
  font-weight: 600;
  font-size: var(--text-sm);
  color: var(--color-text-muted);
  cursor: pointer;
  white-space: nowrap;
  margin-bottom: -2px;
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
}

.ui-tabs__tab:hover:not(:disabled) { color: var(--color-foreground); }

.ui-tabs__tab--active {
  color: var(--color-primary);
  border-bottom-color: var(--color-primary);
}

.ui-tabs__tab--disabled { opacity: 0.5; cursor: not-allowed; }

.ui-tabs__badge {
  font-size: var(--text-xs);
  background: color-mix(in srgb, var(--color-primary) 14%, transparent);
  color: var(--color-primary);
  border-radius: var(--radius-full);
  padding: 1px 8px;
}

.ui-tabs__content { padding-top: var(--space-md); }
</style>
