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
  { color: '#5EEAD4', label: 'Mặc định (Chưa xét)' },
  { color: '#FBBF24', label: 'Đang so sánh / Duyệt' },
  { color: '#F87171', label: 'Hoán đổi vị trí' },
  { color: '#34D399', label: 'Đã hoàn thành / Đúng vị trí' },
  { color: '#EF4444', label: 'Lỗi / Không khớp' },
  { color: '#CBD5E1', label: 'Ngoài phạm vi xét' },
];
</script>

<template>
  <div v-if="!collapsed" class="legend" aria-label="Chú giải màu">
    <div
      v-for="item in LEGEND"
      :key="item.label"
      class="legend__item"
      :title="item.label"
    >
      <span class="legend__dot" :style="{ background: item.color }" />
      <span class="legend__label">{{ item.label }}</span>
    </div>
  </div>
</template>

<style scoped>
.legend {
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 11px;
}

.legend__item {
  display: flex;
  align-items: center;
  gap: 8px;
  color: #cbd5e1;
}

.legend__label {
  line-height: 1.3;
}

.legend__dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  flex-shrink: 0;
  box-shadow: 0 0 6px rgba(0, 0, 0, 0.4);
}
</style>
