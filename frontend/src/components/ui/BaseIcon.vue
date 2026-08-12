<template>
  <svg
    xmlns="http://www.w3.org/2000/svg"
    :width="size"
    :height="size"
    viewBox="0 0 24 24"
    fill="none"
    :stroke="color"
    stroke-width="2"
    stroke-linecap="round"
    stroke-linejoin="round"
    class="base-icon"
    :class="iconClass"
    v-html="pathData"
  />
</template>

<script setup lang="ts">
import { computed } from 'vue';

import { SVG_PATHS } from '@/utils/emojiParser';

// Bê từ source/VisualizationDSA1/frontend/src/components/icons/SvgIcon.vue (V1),
// đổi import SVG_PATHS sang utils/emojiParser (đã có sẵn ở task 1).
const props = withDefaults(defineProps<{
  name: string;
  size?: number | string;
  color?: string;
  filled?: boolean;
}>(), {
  size: 16,
  color: 'currentColor',
  filled: false,
});

const iconClass = computed(() => `icon-${props.name}`);

const pathData = computed(() => SVG_PATHS[props.name] ?? SVG_PATHS['x-circle']);
</script>

<style scoped>
.base-icon {
  display: inline-block;
  vertical-align: middle;
  flex-shrink: 0;
}
</style>
