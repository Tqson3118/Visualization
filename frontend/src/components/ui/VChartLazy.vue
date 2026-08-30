<script setup lang="ts">
// VChartLazy — wrapper ECharts lazy-load (G-F2d).
//
// Leaderboard (bar chart) + Profile (skill radar) dùng chung. Khác BenchmarkPanel:
// vue-echarts được dynamic-import (defineAsyncComponent) → tách chunk riêng,
// KHÔNG tăng bundle chính (NFR-5). Màu/theme do parent đưa qua `option`
// (đọc CSS var lúc computed → đổi sáng/tối không cần reload).
import { computed, defineAsyncComponent, onMounted, ref } from 'vue';

import { use } from 'echarts/core';
import { CanvasRenderer } from 'echarts/renderers';
import { BarChart, PieChart, RadarChart } from 'echarts/charts';
import { GridComponent, LegendComponent, TooltipComponent } from 'echarts/components';
import type { EChartsCoreOption } from 'echarts/core';

// Đăng ký module dùng riêng (tree-shaking): Bar + Radar + Pie cho các màn G-F2d / AdminStats.
use([CanvasRenderer, BarChart, PieChart, RadarChart, GridComponent, LegendComponent, TooltipComponent]);

// Lazy VChart → chunk "vue-echarts" chỉ tải khi có màn dùng chart.
const VChart = defineAsyncComponent(() => import('vue-echarts'));

const props = withDefaults(
  defineProps<{
    option: EChartsCoreOption;
    height?: string;
  }>(),
  {
    height: '260px',
  },
);

const mounted = ref(false);

function prefersReducedMotion(): boolean {
  return (
    typeof window !== 'undefined' &&
    typeof window.matchMedia === 'function' &&
    window.matchMedia('(prefers-reduced-motion: reduce)').matches
  );
}

// Giữ nguyên option parent + ép animation theo prefers-reduced-motion (G-F2a global).
const chartOption = computed<EChartsCoreOption>(() => ({
  ...(props.option as Record<string, unknown>),
  animation: !prefersReducedMotion(),
  animationDuration: 500,
} as unknown as EChartsCoreOption));

onMounted(() => {
  mounted.value = true;
});
</script>

<template>
  <div class="vchart-lazy" :style="{ height }" aria-hidden="true">
    <VChart v-if="mounted" :option="chartOption" autoresize style="height: 100%; width: 100%" />
  </div>
</template>

<style scoped>
.vchart-lazy {
  width: 100%;
  min-width: 0;
}
</style>
