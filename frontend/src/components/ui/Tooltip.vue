<script setup lang="ts">
// Tooltip — wrapper giữ API cũ (G-F1b): text + position + slot trigger.
// Render bằng shadcn-vue Tooltip (reka-ui).
import { Tooltip as TooltipRoot, TooltipContent, TooltipProvider, TooltipTrigger } from './tooltip';

withDefaults(
  defineProps<{
    text?: string;
    position?: 'top' | 'bottom' | 'left' | 'right';
  }>(),
  {
    text: '',
    position: 'top',
  },
);
</script>

<template>
  <TooltipProvider :delay-duration="100">
    <TooltipRoot>
      <TooltipTrigger as-child>
        <span class="inline-flex">
          <slot />
        </span>
      </TooltipTrigger>
      <TooltipContent v-if="text" :side="position">
        {{ text }}
      </TooltipContent>
    </TooltipRoot>
  </TooltipProvider>
</template>
