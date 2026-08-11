<template>
  <span class="inline-flex items-center gap-0.5" :aria-label="ariaLabel">
    <BaseIcon
      v-for="i in 5"
      :key="i"
      name="star"
      class="w-4 h-4"
      :class="i <= filledStars ? 'text-accent-yellow fill-current' : 'text-text-disabled'"
    />
    <span v-if="showValue && value != null" class="ml-1 text-xs font-semibold text-text-secondary">
      {{ value.toFixed(1) }}
    </span>
    <span v-if="showCount && count > 0" class="ml-1 text-xs text-text-muted">({{ count }})</span>
  </span>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import BaseIcon from '@/shared/components/BaseIcon.vue';

const props = withDefaults(
  defineProps<{
    value?: number | null;
    count?: number;
    max?: number;
    showValue?: boolean;
    showCount?: boolean;
  }>(),
  {
    value: null,
    count: 0,
    max: 5,
    showValue: false,
    showCount: false,
  },
);

const filledStars = computed(() => {
  if (props.value == null) return 0;
  return Math.round(Math.max(0, Math.min(props.max, props.value)));
});

const ariaLabel = computed(() => {
  if (props.value == null) return 'Chưa có đánh giá';
  return `${props.value} trên ${props.max} sao`;
});
</script>
