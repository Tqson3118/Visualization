<script setup lang="ts">
// AdminNav — thanh điều hướng admin (SDD §8.7)
import { RouterLink } from 'vue-router';

defineProps<{
  active: 'users' | 'stats' | 'settings' | 'content' | 'ladder';
}>();

const LINKS = [
  { key: 'users', label: 'Người dùng', to: 'admin-users' },
  { key: 'stats', label: 'Thống kê', to: 'admin-stats' },
  { key: 'content', label: 'Nội dung', to: 'admin-content' },
  { key: 'ladder', label: 'Soạn Ladder', to: 'admin-ladder' },
  { key: 'settings', label: 'Cấu hình', to: 'admin-settings' },
] as const;
</script>

<template>
  <nav class="admin-nav" aria-label="Quản trị">
    <RouterLink
      v-for="link in LINKS"
      :key="link.key"
      :to="{ name: link.to }"
      class="admin-nav__link"
      :class="{ 'admin-nav__link--active': active === link.key }"
    >
      {{ link.label }}
    </RouterLink>
  </nav>
</template>

<style scoped>
.admin-nav {
  display: flex;
  gap: var(--space-xs);
  flex-wrap: wrap;
  padding: var(--space-sm);
  background: var(--color-surface);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
}

.admin-nav__link {
  padding: 0.4rem 1rem;
  border-radius: var(--radius-md);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-text-muted);
  text-decoration: none;
}

.admin-nav__link:hover { background: var(--color-surface-hover); }

.admin-nav__link--active {
  background: var(--color-primary);
  color: var(--color-on-primary);
}
</style>
