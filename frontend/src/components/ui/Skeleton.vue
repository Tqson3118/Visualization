<script setup lang="ts">
// Skeleton — wrapper giữ API cũ (G-F1b): height/width/lines/circle.
// Render bằng shadcn-vue Skeleton (animate-pulse bg-muted).
import { cn } from '@/lib/utils';
import Skeleton from './skeleton/Skeleton.vue';

withDefaults(
  defineProps<{
    /** Chiều cao (px hoặc chuỗi CSS) */
    height?: string;
    width?: string;
    /** Số dòng skeleton (mặc định 1) */
    lines?: number;
    circle?: boolean;
  }>(),
  {
    height: '16px',
    width: '100%',
    lines: 1,
    circle: false,
  },
);
</script>

<template>
  <div class="flex flex-col gap-2" :style="{ width }" aria-hidden="true">
    <template v-if="!circle && lines > 1">
      <Skeleton
        v-for="i in lines"
        :key="i"
        :style="{ height, width: i === lines ? '60%' : '100%' }"
      />
    </template>
    <Skeleton v-else :class="cn(circle && 'rounded-full')" :style="{ height, width }" />
  </div>
</template>
