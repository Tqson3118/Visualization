<script setup lang="ts">
// AdminNav — thanh điều hướng admin (SDD §8.7)
// View-quality 14/08 (Nhóm D): token shadcn (--card/--border/--primary), link
// min-height 36px + gap ≥8px (trục 5), font-weight 500 (label chuẩn §3).
import { computed } from 'vue';
import { RouterLink } from 'vue-router';
import { useAuthStore } from '@/stores/auth';

defineProps<{
  active?: 'users' | 'stats' | 'settings' | 'content' | 'classes' | 'ladder' | 'feedback' | 'teacher';
}>();

const LINKS = [
  { key: 'content', label: 'Studio Lộ trình & Bài giảng', to: 'curriculum-studio', adminOnly: false },
  { key: 'classes', label: 'Quản lý lớp học', to: 'classes', adminOnly: false },
  { key: 'users', label: 'Quản lý Người dùng', to: 'admin-users', adminOnly: true },
  { key: 'stats', label: 'Thống kê Hệ thống', to: 'admin-stats', adminOnly: true },
  { key: 'settings', label: 'Cấu hình & Báo cáo', to: 'admin-settings', adminOnly: true },
] as const;

const auth = useAuthStore();
const visibleLinks = computed(() => {
  if (auth.role === 'TEACHER') {
    return [
      { key: 'teacher', label: 'Tổng quan Teacher Studio', to: 'teacher-studio' },
      { key: 'content', label: 'Studio Lộ trình & Bài tập', to: 'curriculum-studio' },
      { key: 'classes', label: 'Quản lý lớp học', to: 'classes' },
    ];
  }
  return [
    { key: 'teacher', label: 'Tổng quan Studio', to: 'teacher-studio' },
    ...LINKS,
  ];
});
</script>

<template>
  <nav class="admin-nav" aria-label="Quản trị">
    <RouterLink
      v-for="link in visibleLinks"
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