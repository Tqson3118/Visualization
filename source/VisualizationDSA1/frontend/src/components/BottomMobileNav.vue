<template>
  <nav class="bottom-mobile-nav lg:hidden" role="navigation" aria-label="Mobile bottom navigation">
    <div class="bottom-mobile-nav__inner glass-panel">
      <!-- We only show up to 5 items to fit on mobile -->
      <router-link
        v-for="item in primaryTabs"
        :key="item.id"
        :to="item.path"
        class="nav-item"
        active-class="nav-item--active"
      >
        <span class="nav-icon"><BaseIcon :name="getIcon(item.id)" class="w-6 h-6" /></span>
        <span class="nav-label">{{ item.name }}</span>
      </router-link>
      
      <!-- More menu trigger if needed, or link to a generic dashboard -->
      <button class="nav-item" @click="$emit('open-menu')" aria-label="Mở menu chính">
        <span class="nav-icon"><BaseIcon name="menu" class="w-6 h-6" /></span>
        <span class="nav-label">Menu</span>
      </button>
    </div>
  </nav>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { APP_TABS } from '../appTabs';
import type { TabGroup, TabItem } from '../appTabs';

// Define a simplified list of tabs for mobile
const primaryTabs = computed(() => {
  const all: TabItem[] = [];
  APP_TABS.forEach(tabOrGroup => {
    if ('groupName' in tabOrGroup) {
      all.push(...(tabOrGroup as TabGroup).items);
    } else {
      all.push(tabOrGroup as TabItem);
    }
  });
  // Ưu tiên các màn chính: Lộ trình → AI → Lớp học → Bảng điều khiển
  const preferred = ['roadmap', 'ai-assistant', 'classrooms', 'dashboard'];
  const ordered = [...preferred.map(id => all.find(t => t.id === id)).filter((t): t is TabItem => !!t)];
  const rest = all.filter(t => !preferred.includes(t.id));
  return [...ordered, ...rest].slice(0, 4);
});

function getIcon(id: string): string {
  switch (id) {
    case 'dashboard': return 'dashboard';
    case 'courses': return 'book';
    case 'roadmap': return 'learning-path';
    case 'gamification': return 'trophy';
    case 'ai-assistant': return 'ai-assistant';
    case 'classrooms': return 'classrooms';
    case 'sorting': return 'sorting';
    case 'graph': return 'graph';
    default: return 'sparkles';
  }
}
</script>

<style scoped>
.bottom-mobile-nav {
  position: fixed;
  bottom: 0;
  left: 0;
  right: 0;
  z-index: var(--z-raised, 9999);
  padding: 0 16px env(safe-area-inset-bottom, 16px) 16px;
  pointer-events: none; /* Let clicks pass through padding */
}

.bottom-mobile-nav__inner {
  pointer-events: auto;
  display: flex;
  justify-content: space-around;
  align-items: center;
  padding: 12px 8px;
  border-radius: 24px 24px 0 0;
  box-shadow: 0 -10px 40px rgba(0, 0, 0, 0.5), 0 0 20px rgba(79, 70, 229, 0.2);
  border: 1px solid var(--color-border-default);
  border-bottom: none;
  background: var(--glass-bg);
  backdrop-filter: var(--glass-blur);
  -webkit-backdrop-filter: var(--glass-blur);
}

.nav-item {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 4px;
  padding: 12px 16px;
  min-width: 60px;
  min-height: 60px;
  color: var(--color-text-secondary);
  border-radius: 16px;
  text-decoration: none;
  transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
  background: transparent;
  border: none;
  cursor: pointer;
  -webkit-tap-highlight-color: transparent;
}

.nav-item:hover {
  color: var(--color-text-primary);
  background: rgba(255, 255, 255, 0.05);
}

.nav-item:active {
  transform: scale(0.95);
  transition-duration: 0.1s;
}

.nav-item--active {
  color: var(--color-accent-primary);
  background: rgba(79, 70, 229, 0.15);
  box-shadow: inset 0 0 10px rgba(79, 70, 229, 0.1);
}

.nav-item--active .nav-icon {
  transform: scale(1.15) translateY(-2px);
  filter: drop-shadow(0 0 8px rgba(79, 70, 229, 0.6));
}

.nav-icon {
  font-size: 22px;
  transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.nav-label {
  font-size: 10px;
  font-weight: 600;
  font-family: var(--font-sans);
  line-height: 1;
}

/* Safe area support for notched devices */
@supports (padding-bottom: env(safe-area-inset-bottom)) {
  .bottom-mobile-nav {
    padding-bottom: calc(16px + env(safe-area-inset-bottom));
  }
}

/* Reduced motion support */
@media (prefers-reduced-motion: reduce) {
  .nav-item,
  .nav-icon {
    transition: none;
  }
}
</style>
