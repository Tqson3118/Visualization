<script setup lang="ts">
// Tabs — wrapper giữ API cũ (G-F1b): tabs[] + modelValue + @change + slot content.
// Bar tab render bằng shadcn-vue Tabs (TabsList/TabsTrigger); content slot luôn hiển thị phía dưới.
import { computed } from 'vue';

import Badge from './Badge.vue';
import { Tabs as TabsRoot, TabsList, TabsTrigger } from './tabs';

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

const activeKey = computed({
  get: () => props.modelValue || props.tabs[0]?.key || '',
  set: (key: string) => {
    emit('update:modelValue', key);
    emit('change', key);
  },
});
</script>

<template>
  <div class="w-full">
    <TabsRoot :model-value="activeKey" @update:model-value="(val: any) => { emit('update:modelValue', String(val)); emit('change', String(val)); }">
      <TabsList class="h-auto w-full justify-start overflow-x-auto rounded-none border-b bg-transparent p-0">
        <TabsTrigger
          v-for="tab in tabs"
          :key="tab.key"
          :value="tab.key"
          :disabled="tab.disabled"
          class="min-h-9 rounded-none border-b-2 border-transparent px-4 py-2 data-[state=active]:border-primary data-[state=active]:bg-transparent data-[state=active]:text-primary data-[state=active]:shadow-none cursor-pointer"
          @click="emit('update:modelValue', tab.key); emit('change', tab.key)"
        >
          {{ tab.label }}
          <Badge v-if="tab.badge !== undefined" variant="primary" class="ml-1">
            {{ tab.badge }}
          </Badge>
        </TabsTrigger>
      </TabsList>
    </TabsRoot>
    <div class="pt-4">
      <slot />
    </div>
  </div>
</template>
