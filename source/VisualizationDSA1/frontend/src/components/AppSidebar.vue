<template>
  <aside 
    class="app-sidebar" 
    :class="{ 
      'is-collapsed': isCollapsed, 
      'is-mobile-open': isMobileOpen,
      'is-hovered': isHovered 
    }"
    @mouseenter="handleMouseEnter"
    @mouseleave="handleMouseLeave"
  >
    <!-- Sidebar Header -->
    <div class="sidebar-header">
      <div class="sidebar-brand" :class="{ 'collapsed': isCollapsed }">
        <BaseIcon name="code" class="w-6 h-6 text-accent-primary" />
        <span class="brand-text" v-if="!isCollapsed">VizDSA</span>
        <span class="brand-text-short" v-else>VDSA</span>
      </div>
      
      <!-- Desktop collapse toggle -->
      <button 
        v-if="!isMobile" 
        class="sidebar-toggle" 
        @click="toggleCollapse"
        :aria-label="isCollapsed ? 'Mở rộng sidebar' : 'Thu gọn sidebar'"
        :title="isCollapsed ? 'Mở rộng sidebar' : 'Thu gọn sidebar'"
      >
        <BaseIcon :name="isCollapsed ? 'chevron-right' : 'chevron-left'" class="w-4 h-4" />
      </button>
    </div>

    <!-- Navigation -->
    <nav class="sidebar-nav" role="navigation" aria-label="Main navigation">
      <template v-for="group in filteredTabs" :key="isTabGroup(group) ? group.groupName : group.id">
        <!-- Group Label -->
        <div v-if="isTabGroup(group) && group.groupName && !isCollapsed" class="nav-group-label">
          {{ group.groupName }}
        </div>
        
        <ul class="nav-list" role="list">
          <li v-for="item in isTabGroup(group) ? group.items : [group]" :key="item.id">
            <component 
              v-if="item.path" 
              :is="item.requiresAuth && !authStore.isAuthenticated ? 'span' : 'router-link'"
              :to="item.path"
              class="nav-item"
              :class="{
                'nav-item--active': isActive(item.path),
                'nav-item--disabled': item.requiresAuth && !authStore.isAuthenticated
              }"
              :title="isCollapsed ? item.name : ''"
            >
              <span class="nav-icon" aria-hidden="true">
                <BaseIcon :name="getIcon(item.id)" class="w-5 h-5" />
              </span>
              <span class="nav-label" v-if="!isCollapsed">{{ item.name }}</span>
              <span class="nav-indicator" v-if="isActive(item.path)" aria-hidden="true"></span>
            </component>
            
            <div v-else-if="isTabGroup(group) && isCollapsed" class="nav-group-collapsed">
              <BaseIcon :name="getIcon(group.items[0]?.id)" class="w-5 h-5" />
              <span class="group-badge">{{ group.items.length }}</span>
            </div>
          </li>
        </ul>
      </template>
    </nav>

    <!-- Sidebar Footer (User Profile) -->
    <div v-if="!isCollapsed && !isMobile" class="sidebar-footer">
      <div class="user-info">
        <div class="user-avatar">
          <span class="sidebar-avatar-placeholder">{{ getInitials() }}</span>
        </div>
        <div class="user-details">
          <span class="user-name">{{ authStore.userName }}</span>
          <span class="user-role" v-if="authStore.isTeacher">Giảng viên</span>
          <span class="user-role" v-else>Sinh viên</span>
        </div>
      </div>
      <div class="user-stats">
        <div class="stat">
          <span class="stat-value">{{ authStore.userLevel }}</span>
          <span class="stat-label">Level</span>
        </div>
        <div class="stat">
          <span class="stat-value">{{ authStore.userXP }}</span>
          <span class="stat-label">XP</span>
        </div>
        <div class="stat">
          <span class="stat-value">{{ authStore.currentUser?.streakDays || 0 }}</span>
          <span class="stat-label">Streak</span>
        </div>
      </div>
    </div>

      <!-- Theme Toggle -->
      <button 
        class="theme-toggle" 
        @click="themeStore.toggleTheme"
        :aria-label="themeStore.currentTheme === 'light' ? 'Chuyển sang giao diện tối' : 'Chuyển sang giao diện sáng'"
        :title="themeStore.currentTheme === 'light' ? 'Chuyển sang giao diện tối' : 'Chuyển sang giao diện sáng'"
      >
        <svg v-if="themeStore.currentTheme === 'light'" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M21 12.79A9 9 0 1 1 11.21 3 7 7 0 0 0 21 12.79z"></path>
        </svg>
        <svg v-else width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="12" cy="12" r="5"></circle>
          <line x1="12" y1="1" x2="12" y2="3"></line>
          <line x1="12" y1="21" x2="12" y2="23"></line>
          <line x1="4.22" y1="4.22" x2="5.64" y2="5.64"></line>
          <line x1="18.36" y1="18.36" x2="19.78" y2="19.78"></line>
          <line x1="1" y1="12" x2="3" y2="12"></line>
          <line x1="21" y1="12" x2="23" y2="23"></line>
          <line x1="4.22" y1="19.78" x2="5.64" y2="18.36"></line>
          <line x1="18.36" y1="5.64" x2="19.78" y2="4.22"></line>
        </svg>
      </button>
    <button 
      v-if="isMobile" 
      class="mobile-close-btn" 
      @click="closeMobile"
      aria-label="Đóng sidebar"
    >
      <BaseIcon name="x" class="w-5 h-5" />
    </button>

    <!-- Mobile Overlay -->
    <div 
      v-if="isMobileOpen" 
      class="sidebar-overlay" 
      @click="closeMobile"
      @touchstart.passive="handleTouchStart"
      @touchmove.passive="handleTouchMove"
      @touchend="handleTouchEnd"
      aria-hidden="true"
    ></div>
  </aside>
</template>

<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import { useAuthStore } from '@/features/auth/store/useAuthStore';
import BaseIcon from '@/shared/components/BaseIcon.vue';
import type { TabGroup, TabItem } from '@/appTabs';
import { APP_TABS } from '@/appTabs';
import { useThemeStore } from '@/shared/store/useThemeStore';

const authStore = useAuthStore();
const themeStore = useThemeStore();
const route = useRoute();
const router = useRouter();

// C6 — chữ đầu an toàn với unicode + fallback
function getInitials(): string {
  const name = authStore.userName?.trim();
  if (!name) return '?';
  return [...name].at(0)?.toUpperCase() ?? '?';
}

const isCollapsed = ref(false);
const isMobileOpen = ref(false);
const isHovered = ref(false);
const isMobile = ref(false);

// Touch/swipe handling for mobile
const touchStartX = ref(0);
const touchStartY = ref(0);
const isSwiping = ref(false);

function updateIsMobile() {
  isMobile.value = window.innerWidth < 1024;
  if (isMobile.value) {
    isMobileOpen.value = false;
  }
}

function handleTouchStart(e: TouchEvent) {
  if (!isMobile.value) return;
  touchStartX.value = e.touches[0].clientX;
  touchStartY.value = e.touches[0].clientY;
  isSwiping.value = true;
}

function handleTouchMove(e: TouchEvent) {
  if (!isSwiping.value || !isMobile.value) return;
  
  const touchX = e.touches[0].clientX;
  const touchY = e.touches[0].clientY;
  const deltaX = touchX - touchStartX.value;
  const deltaY = touchY - touchStartY.value;
  
  // Only handle horizontal swipes
  if (Math.abs(deltaX) > Math.abs(deltaY) && Math.abs(deltaX) > 10) {
    e.preventDefault();
    
    if (isMobileOpen.value) {
      // Swipe left to close
      if (deltaX < -50) {
        closeMobile();
        isSwiping.value = false;
      }
    } else {
      // Swipe right from left edge to open
      if (touchStartX.value < 30 && deltaX > 50) {
        openMobile();
        isSwiping.value = false;
      }
    }
  }
}

function handleTouchEnd() {
  isSwiping.value = false;
}

onMounted(() => {
  updateIsMobile();
  window.addEventListener('resize', updateIsMobile);
  
  // Add touch listeners to document for edge swipe detection
  document.addEventListener('touchstart', handleGlobalTouchStart, { passive: true });
  document.addEventListener('touchmove', handleGlobalTouchMove, { passive: false });
  document.addEventListener('touchend', handleGlobalTouchEnd, { passive: true });
});

onUnmounted(() => {
  window.removeEventListener('resize', updateIsMobile);
  document.removeEventListener('touchstart', handleGlobalTouchStart);
  document.removeEventListener('touchmove', handleGlobalTouchMove);
  document.removeEventListener('touchend', handleGlobalTouchEnd);
});

function handleGlobalTouchStart(e: TouchEvent) {
  if (!isMobile.value) return;
  touchStartX.value = e.touches[0].clientX;
  touchStartY.value = e.touches[0].clientY;
}

function handleGlobalTouchMove(e: TouchEvent) {
  if (!isMobile.value || isMobileOpen.value) return;
  
  const touchX = e.touches[0].clientX;
  const deltaX = touchX - touchStartX.value;
  
  // Swipe from left edge (within 30px) to open sidebar
  if (touchStartX.value < 30 && deltaX > 80) {
    openMobile();
  }
}

function handleGlobalTouchEnd() {
  // Reset
}

function handleMouseEnter() {
  isHovered.value = true;
  if (isCollapsed.value && !isMobile.value) {
    isCollapsed.value = false;
  }
}

function handleMouseLeave() {
  isHovered.value = false;
}

function toggleCollapse() {
  if (!isMobile.value) {
    isCollapsed.value = !isCollapsed.value;
  }
}

function closeMobile() {
  isMobileOpen.value = false;
}

function openMobile() {
  isMobileOpen.value = true;
}

const filteredTabs = computed(() => {
  return APP_TABS.filter((tabOrGroup) => {
    if ('groupName' in tabOrGroup) {
      const group = tabOrGroup as TabGroup;
      const visibleItems = group.items.filter((item: TabItem) => isTabVisible(item));
      return visibleItems.length > 0;
    }
    return isTabVisible(tabOrGroup as TabItem);
  }).map((tabOrGroup) => {
    if ('groupName' in tabOrGroup) {
      const group = tabOrGroup as TabGroup;
      return {
        ...group,
        items: group.items.filter((item: TabItem) => isTabVisible(item)),
      };
    }
    return tabOrGroup;
  });
});

function isTabGroup(group: TabGroup | TabItem): group is TabGroup {
  return 'groupName' in group;
}

function isTabVisible(tab: TabItem): boolean {
  if (tab.requiresAuth && !authStore.isAuthenticated) return false;
  if (tab.requiresRole) {
    const role = authStore.userRole;
    if (role === 'Admin') return true;
    if (role !== tab.requiresRole) return false;
  }
  return true;
}

function isActive(path: string): boolean {
  return route.path === path || route.path.startsWith(path + '/');
}

function getIcon(id: string): string {
  switch (id) {
    case 'dashboard': return 'dashboard';
    case 'courses': return 'book';
    case 'roadmap': return 'learning-path';
    case 'algorithms': return 'dsa';
    case 'docs': return 'book';
    case 'classrooms': return 'classrooms';
    case 'ai-assistant': return 'ai-assistant';
    case 'gems-shop': return 'gem';
    case 'export-share': return 'export-share';
    case 'gamification': return 'trophy';
    case 'sorting': return 'sorting';
    case 'graph': return 'graph';
    case 'playground': return 'playground';
    case 'teacher': return 'academic';
    case 'teacher-studio': return 'academic';
    case 'admin': return 'admin';
    case 'profile': return 'profile';
    case 'settings': return 'cog';
    default: return 'sparkles';
  }
}

// Expose for mobile menu button
defineExpose({
  openMobile,
  closeMobile,
  isMobileOpen
});
</script>

<style scoped>
.app-sidebar {
  position: fixed;
  top: 0;
  left: 0;
  height: 100vh;
  width: var(--sidebar-width, 260px);
  background: var(--color-bg-base, #0f172a);
  border-right: 1px solid var(--color-border-default, #1e293b);
  background: linear-gradient(180deg, var(--color-bg-base) 0%, rgba(15, 23, 42, 0.98) 100%);
  display: flex;
  flex-direction: column;
  z-index: var(--z-sidebar, 1000);
  transition: width 0.3s cubic-bezier(0.4, 0, 0.2, 1), transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  backdrop-filter: blur(20px);
  overflow: hidden;
}

.app-sidebar.is-collapsed {
  width: var(--sidebar-collapsed-width, 72px);
}

.app-sidebar.is-mobile-open {
  transform: translateX(0);
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
}

@media (max-width: 1023px) {
  .app-sidebar {
    transform: translateX(-100%);
    width: var(--sidebar-width, 280px);
  }
  
  .app-sidebar.is-mobile-open {
    transform: translateX(0);
  }
  
  .app-sidebar.is-collapsed {
    width: var(--sidebar-width, 280px);
  }
}

.sidebar-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 1rem 1rem 0.75rem;
  border-bottom: 1px solid var(--color-border-subtle);
  min-height: 64px;
}

.sidebar-brand {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  font-weight: 700;
  font-size: 1.125rem;
  color: var(--color-text-primary);
  white-space: nowrap;
  overflow: hidden;
}

.brand-text-short {
  display: none;
}

.app-sidebar.is-collapsed .brand-text {
  display: none;
}

.app-sidebar.is-collapsed .brand-text-short {
  display: block;
}

.sidebar-toggle {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  background: var(--color-bg-surface);
  border: 1px solid var(--color-border-subtle);
  color: var(--color-text-secondary);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s ease;
  flex-shrink: 0;
}

.sidebar-toggle:hover {
  background: var(--color-accent-primary);
  border-color: var(--color-accent-primary);
  color: white;
  transform: rotate(180deg);
}

.sidebar-nav {
  flex: 1;
  overflow-y: auto;
  padding: 0.75rem 0.5rem;
}

.nav-group-label {
  font-size: 0.625rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--color-text-disabled);
  padding: 0.5rem 1rem 0.25rem;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.nav-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  padding: 0.625rem 1rem;
  border-radius: 10px;
  color: var(--color-text-secondary);
  text-decoration: none;
  font-size: 0.875rem;
  font-weight: 500;
  transition: all 0.2s ease;
  position: relative;
  overflow: hidden;
}

.nav-item:hover {
  background: rgba(255, 255, 255, 0.05);
  color: var(--color-text-primary);
}

.nav-item--active {
  background: linear-gradient(90deg, rgba(79, 70, 229, 0.15), transparent);
  color: var(--color-accent-primary);
  border-right: 3px solid var(--color-accent-primary);
}

.nav-item--active .nav-indicator {
  position: absolute;
  right: 0;
  top: 50%;
  transform: translateY(-50%);
  width: 3px;
  height: 60%;
  background: var(--color-accent-primary);
  border-radius: 0 4px 4px 0;
  box-shadow: 0 0 10px var(--color-accent-primary);
}

.nav-item--disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

.nav-item--disabled:hover {
  background: transparent;
  color: var(--color-text-secondary);
}

.nav-icon {
  flex-shrink: 0;
  transition: transform 0.2s ease;
}

.nav-label {
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  transition: opacity 0.2s ease, transform 0.2s ease;
}

.app-sidebar.is-collapsed .nav-label {
  opacity: 0;
  transform: translateX(-10px);
  pointer-events: none;
}

.nav-indicator {
  position: absolute;
  right: 0;
  top: 50%;
  transform: translateY(-50%);
  width: 3px;
  height: 60%;
  background: var(--color-accent-primary);
  border-radius: 0 4px 4px 0;
  box-shadow: 0 0 10px var(--color-accent-primary);
}

.nav-group-collapsed {
  display: none;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
  padding: 0.5rem;
  color: var(--color-text-secondary);
}

.app-sidebar.is-collapsed .nav-group-collapsed {
  display: flex;
}

.group-badge {
  font-size: 0.625rem;
  font-weight: 700;
  background: var(--color-accent-primary);
  color: white;
  border-radius: 999px;
  padding: 0.125rem 0.5rem;
}

.sidebar-footer {
  padding: 1rem;
  border-top: 1px solid var(--color-border-subtle);
  background: var(--color-bg-surface);
}

.user-info {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--color-accent-primary), var(--color-accent-cyan));
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  color: white;
  flex-shrink: 0;
}

/* C6 — class riêng (scoped) tránh bị .avatar-placeholder global 100px đè */
.sidebar-avatar-placeholder {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 0.875rem;
  font-weight: 700;
  line-height: 1;
  color: white;
}

.user-details {
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.user-name {
  font-weight: 600;
  font-size: 0.875rem;
  color: var(--color-text-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.user-role {
  font-size: 0.625rem;
  color: var(--color-accent-warm);
  font-weight: 500;
  text-transform: uppercase;
}

.user-stats {
  display: flex;
  justify-content: space-around;
  padding-top: 0.75rem;
  border-top: 1px solid var(--color-border-subtle);
}

.stat {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2px;
  min-width: 0;
}

.stat-value {
  font-weight: 700;
  font-size: 0.875rem;
  color: var(--color-text-primary);
}

.stat-label {
  font-size: 0.625rem;
  color: var(--color-text-disabled);
  text-transform: uppercase;
}

.mobile-close-btn {
  display: none;
  position: absolute;
  top: 1rem;
  right: 1rem;
  width: 40px;
  height: 40px;
  border-radius: 50%;
  background: var(--color-bg-surface);
  border: 1px solid var(--color-border-default);
  color: var(--color-text-primary);
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  z-index: 10;
}

@media (max-width: 1023px) {
  .mobile-close-btn {
    display: flex;
  }
}

.sidebar-overlay {
  display: none;
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.5);
  z-index: 999;
}

@media (max-width: 1023px) {
  .sidebar-overlay {
    display: block;
  }
}

/* Breadcrumb for mobile */
.breadcrumb-mobile {
  display: none;
  padding: 0.75rem 1rem;
  background: var(--color-bg-surface);
  border-bottom: 1px solid var(--color-border-subtle);
  overflow-x: auto;
  white-space: nowrap;
}

@media (max-width: 1023px) {
  .breadcrumb-mobile {
    display: flex;
    gap: 0.5rem;
    align-items: center;
    padding: 0.5rem 1rem;
  }
}

.breadcrumb-item {
  font-size: 0.75rem;
  color: var(--color-text-secondary);
  cursor: pointer;
  transition: color 0.2s ease;
}

.breadcrumb-item:hover {
  color: var(--color-accent-primary);
}

.breadcrumb-item.active {
  color: var(--color-text-primary);
  font-weight: 500;
  pointer-events: none;
}

.breadcrumb-separator {
  color: var(--color-text-disabled);
  margin: 0 0.25rem;
}
/* Theme Toggle Button */
.theme-toggle {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border-radius: var(--radius-md);
  background: var(--color-bg-surface);
  border: 1px solid var(--color-border-subtle);
  color: var(--color-text-secondary);
  cursor: pointer;
  transition: all 0.2s ease;
  margin-top: 0.75rem;
}

.theme-toggle:hover {
  background: var(--color-accent-primary);
  border-color: var(--color-accent-primary);
  color: white;
  transform: scale(1.05);
}

.theme-toggle svg {
  transition: transform 0.3s ease;
}

.theme-toggle:hover svg {
  transform: rotate(15deg);
}
</style>