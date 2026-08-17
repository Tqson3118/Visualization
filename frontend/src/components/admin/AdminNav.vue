<script setup lang="ts">
// AdminNav — thanh điều hướng admin (SDD §8.7)
// View-quality 14/08 (Nhóm D): token shadcn (--card/--border/--primary), link
// min-height 36px + gap ≥8px (trục 5), font-weight 500 (label chuẩn §3).
import { RouterLink } from 'vue-router';

defineProps<{
  active: 'users' | 'stats' | 'settings' | 'content' | 'ladder' | 'feedback';
}>();

const LINKS = [
  { key: 'users', label: 'Người dùng', to: 'admin-users' },
  { key: 'stats', label: 'Thống kê', to: 'admin-stats' },
  { key: 'content', label: 'Nội dung', to: 'admin-content' },
  { key: 'ladder', label: 'Soạn Ladder', to: 'admin-ladder' },
  { key: 'feedback', label: 'Ý kiến học viên', to: 'admin-feedback' },
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
  align-items: center;
  gap: var(--space-sm);
  flex-wrap: wrap;
  padding: var(--space-sm);
  background: var(--card);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
}

.admin-nav__link {
  display: inline-flex;
  align-items: center;
  min-height: 36px;
  padding: var(--space-xs) var(--space-sm);
  border-radius: var(--radius-md);
  font-size: var(--text-sm);
  font-weight: 500;
  color: var(--foreground-secondary);
  text-decoration: none;
}

.admin-nav__link:hover {
  background: var(--muted);
  color: var(--foreground);
  text-decoration: none;
}

.admin-nav__link--active {
  background: var(--primary);
  color: var(--primary-foreground);
}
</style>
