<script setup lang="ts">
// AdminHeroStrip — mono strip block-token (DESIGN.md §2.1): panel LUÔN tối canvas-ink
// + 5 block trạng thái dữ liệu + index mono — "dữ liệu luôn được đánh số".
// Trích xuất từ khối strip lặp ở AdminStats/AdminUsers/AdminContent/Classes.
// Nhúng vào slot #side của PageHero. Filled mặc định = i < count; truyền
// activeIndices để filled theo danh sách cụ thể.
import { computed } from 'vue';

const props = withDefaults(
  defineProps<{
    /** Số block FILLED (clamp 1..5) */
    count?: number;
    /** Caption mono dưới panel (VD "5 CHỈ SỐ") */
    label?: string;
    /** Danh sách index filled — nếu truyền thì thắng count */
    activeIndices?: number[];
  }>(),
  {
    count: 5,
    label: '',
    activeIndices: undefined,
  },
);

// size reactive theo props.count (count thay đổi sau khi data load — VD AdminUsers pendingCount).
const size = computed(() => Math.min(Math.max(props.count, 1), 5));

const blocks = computed(() =>
  Array.from({ length: size.value }, (_, i) =>
    props.activeIndices ? props.activeIndices.includes(i) : i < props.count,
  ),
);
</script>

<template>
  <div class="admin-strip" aria-hidden="true">
    <div class="admin-strip__panel">
      <div class="admin-strip__blocks">
        <span
          v-for="(filled, i) in blocks"
          :key="i"
          class="admin-strip__block"
          :class="{ 'admin-strip__block--empty': !filled }"
          :style="{ '--i': i }"
        />
      </div>
      <div class="admin-strip__index">
        <span v-for="(_, i) in blocks" :key="i">{{ String(i).padStart(2, '0') }}</span>
      </div>
    </div>
    <p v-if="label" class="admin-strip__caption">{{ label }}</p>
  </div>
</template>

<style scoped>
/* ── Mono strip: block-token (khoảnh khắc đầu tư duy nhất — enter) ── */
.admin-strip {
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.admin-strip__panel {
  background: var(--canvas-ink);
  border: 1px solid rgba(66, 85, 255, 0.25);
  border-radius: var(--radius-md);
  padding: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: var(--space-sm);
}

.admin-strip__blocks {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: var(--space-sm);
}

.admin-strip__block {
  height: 28px;
  border-radius: var(--radius-sm);
  background: var(--data-core);
  opacity: 0;
  transform: translateY(6px);
  animation: admin-strip-enter 280ms cubic-bezier(0.16, 1, 0.3, 1) forwards;
  animation-delay: calc(var(--i) * 45ms + 60ms);
}

.admin-strip__block--empty {
  background: transparent;
  border: 1px dashed var(--data-core);
  opacity: 1;
  transform: none;
  animation: none;
}

.admin-strip__index {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: var(--space-sm);
}

.admin-strip__index span {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--index-muted);
  text-align: center;
}

.admin-strip__caption {
  margin: 0;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--foreground-tertiary);
  letter-spacing: 0.08em;
  text-align: right;
}

@keyframes admin-strip-enter {
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .admin-strip__block {
    animation: none;
    opacity: 1;
    transform: none;
  }
}

@media (max-width: 640px) {
  .admin-strip__caption {
    text-align: left;
  }
}
</style>
