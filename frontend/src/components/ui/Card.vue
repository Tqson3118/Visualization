<script setup lang="ts">
// Card — wrapper giữ API cũ (G-F1b): interactive/padded.
// Render bằng shadcn-vue Card (border bg-card shadow-sm).
import { cn } from '@/lib/utils';
import Card from './card/Card.vue';

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
  <Card
    :class="cn(
      'w-full',
      interactive &&
        'cursor-pointer transition-all duration-200 hover:-translate-y-0.5 hover:shadow-lg',
      padded ? 'p-6' : 'p-0',
    )"
  >
    <slot />
  </Card>
</template>
