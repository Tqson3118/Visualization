<script setup lang="ts">
// DemoBanner — banner chế độ demo công khai (FR-7.6): không token, không trừ tim, không lưu tiến độ
import { RouterLink } from 'vue-router';

defineProps<{
  simKey?: string;
}>();
</script>

<template>
  <div class="demo-banner" role="note">
    <p class="demo-banner__text">
      <strong>Chế độ demo</strong> — bạn đang xem mô phỏng <code>{{ simKey }}</code> công khai
      (không trừ tim, không lưu tiến độ).
    </p>
    <RouterLink class="demo-banner__cta" to="/register">
      Đăng ký để học tiếp →
    </RouterLink>
  </div>
</template>

<style scoped>
.demo-banner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-md);
  flex-wrap: wrap;
  background: color-mix(in srgb, var(--color-secondary) 16%, transparent);
  border: 1px solid var(--color-secondary);
  border-radius: var(--radius-lg);
  padding: var(--space-sm) var(--space-md);
}

.demo-banner__text { font-size: var(--text-sm); }

.demo-banner__text code {
  font-family: var(--font-mono);
  background: var(--color-surface);
  padding: 1px 6px;
  border-radius: var(--radius-sm);
}

.demo-banner__cta { font-weight: 700; font-size: var(--text-sm); white-space: nowrap; }
</style>
