<script setup lang="ts">
// AppHeader — thanh điều hướng chung (SDD §8.7 — navigation theo vai trò)
// Hiển thị cho mọi route; ẩn menu học tập khi chưa đăng nhập.
import { computed, ref } from 'vue';
import { RouterLink, useRouter } from 'vue-router';

import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import { messages } from '@/i18n/vi';
import HeartsGemsWidget from '@/components/simulator/HeartsGemsWidget.vue';

const auth = useAuthStore();
const ui = useUiStore();
const router = useRouter();
const menuOpen = ref(false);

const isTeacherOrAdmin = computed(() => auth.role === 'TEACHER' || auth.role === 'ADMIN');

async function onLogout(): Promise<void> {
  menuOpen.value = false;
  await auth.logout();
  await router.replace({ name: 'login' });
}
</script>

<template>
  <header class="app-header">
    <div class="container app-header__inner">
      <RouterLink class="app-header__brand" :to="{ name: 'home' }">
        <span class="app-header__logo" aria-hidden="true">DSA</span>
        <span class="app-header__name">{{ messages.app.name }}</span>
      </RouterLink>

      <nav v-if="auth.isAuthenticated" class="app-header__nav" aria-label="Điều hướng chính">
        <RouterLink :to="{ name: 'path' }" class="app-header__link">{{ messages.nav.path }}</RouterLink>
        <RouterLink :to="{ name: 'simulations' }" class="app-header__link">{{ messages.nav.simulations }}</RouterLink>
        <RouterLink :to="{ name: 'classes' }" class="app-header__link">Lớp học</RouterLink>
        <RouterLink :to="{ name: 'quests' }" class="app-header__link">Thử thách</RouterLink>
        <RouterLink :to="{ name: 'shop' }" class="app-header__link">Cửa hàng</RouterLink>
        <RouterLink v-if="isTeacherOrAdmin" :to="{ name: 'admin-users' }" class="app-header__link">
          {{ messages.nav.admin }}
        </RouterLink>
      </nav>

      <div class="app-header__actions">
        <HeartsGemsWidget v-if="auth.isAuthenticated" />
        <template v-if="!auth.isAuthenticated">
          <RouterLink class="app-header__login" :to="{ name: 'login' }">{{ messages.nav.login }}</RouterLink>
          <RouterLink class="app-header__register" :to="{ name: 'register' }">{{ messages.nav.register }}</RouterLink>
        </template>
        <template v-else>
          <button type="button" class="app-header__user" :aria-label="auth.user?.displayName ?? 'Hồ sơ'" @click="menuOpen = !menuOpen">
            {{ auth.user?.displayName?.charAt(0)?.toUpperCase() ?? 'U' }}
          </button>
          <Transition name="app-menu">
            <div v-if="menuOpen" class="app-header__menu card">
              <RouterLink :to="{ name: 'profile' }" class="app-header__menu-item" @click="menuOpen = false">
                {{ messages.nav.profile }}
              </RouterLink>
              <RouterLink :to="{ name: 'leaderboard' }" class="app-header__menu-item" @click="menuOpen = false">
                Bảng xếp hạng
              </RouterLink>
              <RouterLink :to="{ name: 'premium' }" class="app-header__menu-item" @click="menuOpen = false">
                Premium
              </RouterLink>
              <RouterLink :to="{ name: 'help' }" class="app-header__menu-item" @click="menuOpen = false">
                Trợ giúp
              </RouterLink>
              <button type="button" class="app-header__menu-item app-header__menu-item--danger" @click="onLogout">
                {{ messages.nav.logout }}
              </button>
            </div>
          </Transition>
        </template>
      </div>
    </div>
  </header>
</template>

<style scoped>
.app-header {
  position: sticky;
  top: 0;
  z-index: var(--z-raised);
  background: var(--color-surface);
  border-bottom: 1px solid var(--color-border);
  box-shadow: var(--shadow-sm);
}

.app-header__inner {
  display: flex;
  align-items: center;
  gap: var(--space-lg);
  padding-block: var(--space-sm);
}

.app-header__brand {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  text-decoration: none;
  font-weight: 800;
  color: var(--color-foreground);
}

.app-header__logo {
  background: var(--color-primary);
  color: var(--color-on-primary);
  border-radius: var(--radius-md);
  padding: 3px 8px;
  font-size: var(--text-sm);
}

.app-header__name { font-size: var(--text-md); }

.app-header__nav {
  display: flex;
  gap: var(--space-md);
  overflow-x: auto;
  flex: 1;
}

.app-header__link {
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-text-muted);
  text-decoration: none;
  white-space: nowrap;
  padding-block: 4px;
}

.app-header__link:hover { color: var(--color-primary); }
.app-header__link.router-link-exact-active { color: var(--color-primary); }

.app-header__actions {
  position: relative;
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  margin-left: auto;
}

.app-header__login {
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-foreground);
  text-decoration: none;
}

.app-header__register {
  font-size: var(--text-sm);
  font-weight: 700;
  background: var(--color-primary);
  color: var(--color-on-primary);
  padding: 0.4rem 1rem;
  border-radius: var(--radius-md);
  text-decoration: none;
}

.app-header__user {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--color-primary);
  color: var(--color-on-primary);
  border: none;
  font-weight: 800;
  cursor: pointer;
}

.app-header__menu {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  min-width: 200px;
  display: flex;
  flex-direction: column;
  padding: var(--space-sm);
  box-shadow: var(--shadow-lg);
}

.app-header__menu-item {
  background: none;
  border: none;
  text-align: left;
  padding: 0.5rem var(--space-sm);
  border-radius: var(--radius-md);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--color-foreground);
  text-decoration: none;
  cursor: pointer;
}

.app-header__menu-item:hover { background: var(--color-surface-hover); }

.app-header__menu-item--danger { color: var(--color-destructive); }

.app-menu-enter-active,
.app-menu-leave-active { transition: opacity 150ms ease, transform 150ms ease; }
.app-menu-enter-from,
.app-menu-leave-to { opacity: 0; transform: translateY(-4px); }

@media (max-width: 900px) {
  .app-header__nav { display: none; }
}
</style>
