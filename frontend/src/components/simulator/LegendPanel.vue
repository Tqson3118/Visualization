<script setup lang="ts">
// LegendPanel — chú giải màu trạng thái phần tử (FR-3.6, SDD §8.5)
withDefaults(
  defineProps<{
    collapsed?: boolean;
  }>(),
  {
    collapsed: false,
  },
);

const LEGEND = [
  { color: '#5EEAD4', label: 'Mặc định' },
  { color: '#FBBF24', label: 'Đang xử lý' },
  { color: '#F87171', label: 'Hoán đổi' },
  { color: '#34D399', label: 'Đã hoàn thành' },
  { color: '#EF4444', label: 'Lỗi' },
  { color: '#CBD5E1', label: 'Mờ / ngoài vùng' },
];
</script>

<template>
  <div v-if="!collapsed" class="legend" aria-label="Chú giải màu">
    <span
      v-for="item in LEGEND"
      :key="item.label"
      class="legend__item"
      :title="item.label"
    >
      <span class="legend__dot" :style="{ background: item.color }" />
      {{ item.label }}
    </span>
  </div>
</template>

<style scoped>
.legend {
  display: flex;
  flex-wrap: wrap;
  gap: var(--space-sm);
  font-size: var(--text-xs);
  color: var(--color-text-muted);
}

.legend__item {
  display: inline-flex;
  align-items: center;
  gap: 4px;
}

.legend__dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  border: 1px solid rgba(0, 0, 0, 0.15);
}
</style>
