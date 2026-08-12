<script setup lang="ts">
// Card — component UI chung: khối nội dung (design tokens — SDD §8.1)
withDefaults(
  defineProps<{
    interactive?: boolean;
    padded?: boolean;
  }>(),
  {
    interactive: false,
    padded: true,
  },
);
</script>

<template>
  <div class="ui-card card" :class="{ 'card--interactive': interactive, 'ui-card--flush': !padded }">
    <slot />
  </div>
</template>

<style scoped>
.ui-card--flush { padding: 0; }
</style>
