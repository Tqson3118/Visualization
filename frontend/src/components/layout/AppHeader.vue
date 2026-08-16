<script setup lang="ts">
// AppHeader — thanh điều hướng chung (SDD §8.7 — navigation theo vai trò)
// Hiển thị cho mọi route; ẩn menu học tập khi chưa đăng nhập.
import { computed, onMounted, ref } from 'vue';
import { RouterLink, useRouter } from 'vue-router';
import { Moon, Sun } from 'lucide-vue-next';

import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import { avatarVariant, equippedItem, frameVariant } from '@/utils/equipment';
import { messages } from '@/i18n/vi';
import HeartsGemsWidget from '@/components/simulator/HeartsGemsWidget.vue';

const auth = useAuthStore();
const gamification = useGamificationStore();
const ui = useUiStore();
const router = useRouter();
const menuOpen = ref(false);

const isTeacherOrAdmin = computed(() => auth.role === 'TEACHER' || auth.role === 'ADMIN');

// Task 15C — nhãn nút đổi màu nền: mô tả hành động SẮP xảy ra khi bấm
// (light → "sang tối", dark → "sang sáng") — icon hiển thị trạng thái đích.
const themeToggleLabel = computed(() =>
  ui.theme === 'light' ? messages.common.toDarkTheme : messages.common.toLightTheme,
);

// Quản trị: TEACHER → /admin/content (roles TEACHER|ADMIN); ADMIN → /admin/users
const adminTarget = computed(() => ({ name: auth.role === 'TEACHER' ? 'admin-content' : 'admin-users' }));

const equippedFrame = computed(() => equippedItem(gamification.inventory, 'frame'));
const equippedAvatar = computed(() => equippedItem(gamification.inventory, 'avatar'));

const userFrameClass = computed(() => {
  const key = equippedFrame.value?.itemKey;
  return key ? `app-header__user-frame--${frameVariant(key)}` : '';
});
const userAvatarClass = computed(() => {
  const key = equippedAvatar.value?.itemKey;
  return key ? `app-header__user-avatar--${avatarVariant(key)}` : '';
});

onMounted(() => {
  void gamification.fetchInventory();
});

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
        <RouterLink v-if="isTeacherOrAdmin" :to="adminTarget" class="app-header__link">
          {{ messages.nav.admin }}
        </RouterLink>
      </nav>

      <div class="app-header__actions">
        <button
          type="button"
          class="app-header__theme"
          :title="themeToggleLabel"
          :aria-label="themeToggleLabel"
          @click="ui.toggleTheme()"
        >
          <Sun v-if="ui.theme === 'dark'" class="app-header__theme-icon" aria-hidden="true" />
          <Moon v-else class="app-header__theme-icon" aria-hidden="true" />
        </button>
        <HeartsGemsWidget v-if="auth.isAuthenticated" />
        <template v-if="!auth.isAuthenticated">
          <RouterLink class="app-header__login" :to="{ name: 'login' }">{{ messages.nav.login }}</RouterLink>
          <RouterLink class="app-header__register" :to="{ name: 'register' }">{{ messages.nav.register }}</RouterLink>
        </template>
        <template v-else>
          <span class="app-header__user-frame" :class="userFrameClass">
            <button
              type="button"
              class="app-header__user"
              :class="userAvatarClass"
              :aria-label="auth.user?.displayName ?? 'Hồ sơ'"
              @click="menuOpen = !menuOpen"
            >
              {{ auth.user?.displayName?.charAt(0)?.toUpperCase() ?? 'U' }}
            </button>
          </span>
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
  border-bottom: 1px solid var(--border);
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
  color: var(--foreground);
}

.app-header__logo {
  background: var(--primary);
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

.app-header__link:hover { color: var(--primary); }
.app-header__link.router-link-exact-active { color: var(--primary); }

.app-header__actions {
  position: relative;
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  margin-left: auto;
}

/* Task 15C — nút đổi màu nền: icon 20px trong button 40px (interactive
   sizing), pill, hover surface + primary, focus ring chuẩn. */
.app-header__theme {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border: none;
  border-radius: var(--radius-full);
  background: transparent;
  color: var(--color-text-muted);
  cursor: pointer;
  transition: background-color 150ms ease, color 150ms ease;
}

.app-header__theme:hover {
  background: var(--color-surface-hover);
  color: var(--primary);
}

.app-header__theme:focus-visible {
  outline: 2px solid var(--ring);
  outline-offset: 2px;
}

.app-header__theme-icon {
  width: 20px;
  height: 20px;
  animation: theme-icon-pop 200ms ease;
}

/* Rotate + scale nhẹ khi đổi icon (reduced-motion đã được global.css cắt) */
@keyframes theme-icon-pop {
  from {
    opacity: 0;
    transform: rotate(-90deg) scale(0.6);
  }
  to {
    opacity: 1;
    transform: rotate(0deg) scale(1);
  }
}

.app-header__login {
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--foreground);
  text-decoration: none;
}

.app-header__register {
  font-size: var(--text-sm);
  font-weight: 700;
  background: var(--primary);
  color: var(--color-on-primary);
  padding: 0.4rem 1rem;
  border-radius: var(--radius-md);
  text-decoration: none;
}

.app-header__user-frame {
  border-radius: var(--radius-full);
  padding: 2px;
  display: inline-flex;
  line-height: 0;
}

/* Gradient theo frame itemKey đang trang bị (khớp ProfileView — utils/equipment.ts) */
.app-header__user-frame--neon {
  background: linear-gradient(135deg, #ec4899, #22d3ee);
  box-shadow: 0 0 12px rgba(236, 72, 153, 0.45);
}

.app-header__user-frame--gold {
  background: linear-gradient(135deg, #f59e0b, #fde68a, #f59e0b);
  box-shadow: 0 0 14px rgba(250, 204, 21, 0.5);
}

.app-header__user-frame--cyber {
  background: linear-gradient(135deg, #22d3ee, #6366f1);
  box-shadow: 0 0 12px rgba(34, 211, 238, 0.45);
}

.app-header__user-frame--fire {
  background: linear-gradient(135deg, #ef4444, #f97316);
  box-shadow: 0 0 12px rgba(239, 68, 68, 0.5);
}

.app-header__user-frame--ice {
  background: linear-gradient(135deg, #7dd3fc, #93c5fd);
  box-shadow: 0 0 10px rgba(125, 211, 252, 0.5);
}

.app-header__user-frame--default {
  background: linear-gradient(135deg, var(--primary), var(--color-data-core));
  box-shadow: 0 0 10px color-mix(in srgb, var(--primary) 45%, transparent);
}

.app-header__user {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: var(--primary);
  color: var(--color-on-primary);
  border: none;
  font-weight: 800;
  cursor: pointer;
}

/* Avatar theme theo itemKey đang trang bị — gradient tối + chữ sáng */
.app-header__user-avatar--cyber { background: linear-gradient(135deg, #0e7490, #155e75); color: #a5f3fc; }
.app-header__user-avatar--gold { background: linear-gradient(135deg, #b45309, #92400e); color: #fef3c7; }
.app-header__user-avatar--neon { background: linear-gradient(135deg, #be185d, #6b21a8); color: #fbcfe8; }
.app-header__user-avatar--wizard { background: linear-gradient(135deg, #6d28d9, #4c1d95); color: #ddd6fe; }
.app-header__user-avatar--bot { background: linear-gradient(135deg, #0f766e, #134e4a); color: #99f6e4; }

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
  color: var(--foreground);
  text-decoration: none;
  cursor: pointer;
}

.app-header__menu-item:hover { background: var(--color-surface-hover); }

.app-header__menu-item--danger { color: var(--destructive); }

.app-menu-enter-active,
.app-menu-leave-active { transition: opacity 150ms ease, transform 150ms ease; }
.app-menu-enter-from,
.app-menu-leave-to { opacity: 0; transform: translateY(-4px); }

@media (max-width: 900px) {
  .app-header__nav { display: none; }
}
</style>
