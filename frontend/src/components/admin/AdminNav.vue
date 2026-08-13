<script setup lang="ts">
// AdminNav — thanh điều hướng admin (SDD §8.7)
// View-quality 14/08 (Nhóm D): token shadcn (--card/--border/--primary), link
// min-height 36px + gap ≥8px (trục 5), font-weight 500 (label chuẩn §3).
// UI-PREMIUM Phase 1D: thêm lucide icon (lucide-vue-next — DESIGN §4); <1024px
// sidebar/bar FOLD về icon-only (ẩn label, nowrap + overflow-x auto) — không vỡ
// ở 768/640/360; active indicator = transition glow + accent (easing chuẩn),
// entry stagger nhẹ bằng --i; tôn trọng prefers-reduced-motion.
import { RouterLink } from 'vue-router';
import { BarChart3, BookOpen, ListOrdered, Settings, Users } from 'lucide-vue-next';

defineProps<{
  active: 'users' | 'stats' | 'settings' | 'content' | 'ladder';
}>();

const LINKS = [
  { key: 'users', label: 'Người dùng', to: 'admin-users', icon: Users },
  { key: 'stats', label: 'Thống kê', to: 'admin-stats', icon: BarChart3 },
  { key: 'content', label: 'Nội dung', to: 'admin-content', icon: BookOpen },
  { key: 'ladder', label: 'Soạn Ladder', to: 'admin-ladder', icon: ListOrdered },
  { key: 'settings', label: 'Cấu hình', to: 'admin-settings', icon: Settings },
] as const;
</script>

<template>
  <nav class="admin-nav" aria-label="Quản trị">
    <RouterLink
      v-for="(link, i) in LINKS"
      :key="link.key"
      :to="{ name: link.to }"
      class="admin-nav__link"
      :class="{ 'admin-nav__link--active': active === link.key }"
      :style="{ '--i': i }"
    >
      <component :is="link.icon" :size="16" aria-hidden="true" class="admin-nav__icon" />
      <span class="admin-nav__label">{{ link.label }}</span>
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
  gap: var(--space-sm);
  min-height: 40px;
  padding: var(--space-xs) var(--space-md);
  border-radius: var(--radius-md);
  font-size: var(--text-sm);
  font-weight: 500;
  color: var(--foreground-secondary);
  text-decoration: none;
  opacity: 0;
  transform: translateY(4px);
  animation: admin-nav-enter 280ms cubic-bezier(0.16, 1, 0.3, 1) forwards;
  animation-delay: calc(var(--i) * 40ms + 40ms);
  transition:
    background-color 150ms var(--ease-out-expo),
    color 150ms var(--ease-out-expo),
    box-shadow 150ms var(--ease-out-expo);
}

.admin-nav__link:hover {
  background: var(--muted);
  color: var(--foreground);
  text-decoration: none;
}

.admin-nav__link--active {
  background: var(--primary);
  color: var(--primary-foreground);
  box-shadow: var(--glow-primary);
}

.admin-nav__link--active:hover {
  background: var(--primary);
  color: var(--primary-foreground);
}

.admin-nav__icon { flex-shrink: 0; }

/* ── <1024px: bar FOLD về icon-only (DESIGN §8 — giữ hit target 40px, scroll ngang nếu tràn) ── */
@media (max-width: 1023px) {
  .admin-nav {
    flex-wrap: nowrap;
    overflow-x: auto;
    scrollbar-width: none;
  }

  .admin-nav::-webkit-scrollbar { display: none; }

  .admin-nav__link {
    min-width: 44px;
    justify-content: center;
    padding: var(--space-xs);
  }

  .admin-nav__label {
    position: absolute;
    width: 1px;
    height: 1px;
    padding: 0;
    margin: -1px;
    overflow: hidden;
    clip: rect(0, 0, 0, 0);
    white-space: nowrap;
    border: 0;
  }
}

@keyframes admin-nav-enter {
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@media (prefers-reduced-motion: reduce) {
  .admin-nav__link {
    animation: none;
    opacity: 1;
    transform: none;
    transition: none;
  }
}
</style>
