<script setup lang="ts">
// AppHeader — thanh điều hướng chung (SDD §8.7 — navigation theo vai trò)
// Tân trang 15/08: trong suốt (transparent) khi ở đầu trang chủ → glass blur + viền mờ
// khi cuộn hoặc ở trang khác; palette "terminal dark" bê từ VisualizationDSA-main
// (nền #131614, accent #a855f7/#c084fc, mono kicker).
import { computed, onMounted, onUnmounted, ref } from 'vue';
import { RouterLink, useRoute, useRouter } from 'vue-router';

import { useAuthStore } from '@/stores/auth';
import { useGamificationStore } from '@/stores/gamification';
import { useUiStore } from '@/stores/ui';
import { avatarImageUrl, avatarVariant, equippedItem, frameVariant } from '@/utils/equipment';
import { messages } from '@/i18n/vi';
import HeartsGemsWidget from '@/components/simulator/HeartsGemsWidget.vue';
import brandLogo from '@/assets/brand-logo.png';

const auth = useAuthStore();
const gamification = useGamificationStore();
const ui = useUiStore();
const router = useRouter();
const route = useRoute();
const menuOpen = ref(false);
const mobileNavOpen = ref(false);

// Phase 1: Sticky header scroll tracking — khi cuộn qua 50px → glass blur + border
const isScrolled = ref(false);
function onScroll(): void {
  isScrolled.value = window.scrollY > 50;
}
onMounted(() => {
  window.addEventListener('scroll', onScroll, { passive: true });
  onScroll(); // khởi tạo ngay
  if (auth.isAuthenticated) {
    void gamification.fetchInventory();
  }
});
onUnmounted(() => {
  window.removeEventListener('scroll', onScroll);
});

const isTeacherOrAdmin = computed(() => auth.role === 'TEACHER' || auth.role === 'ADMIN');

// Quản trị & Soạn bài: TEACHER → /teacher (teacher-studio), ADMIN → /studio (curriculum-studio)
const studioTarget = computed(() => {
  if (auth.role === 'TEACHER') {
    return { name: 'teacher-studio' };
  }
  return { name: 'curriculum-studio' };
});

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

async function onLogout(): Promise<void> {
  menuOpen.value = false;
  await auth.logout();
  await router.replace({ name: 'login' });
}
</script>

<template>
  <header class="app-header" :class="{ 'app-header--scrolled': isScrolled }">
    <div class="container app-header__inner">
      <RouterLink class="app-header__brand" :to="{ name: 'home' }" aria-label="DSA Visual — Trang chủ">
        <img class="app-header__brand-img" :src="brandLogo" alt="DSA Visual" />
      </RouterLink>

      <!-- Luôn hiện cho mọi người (kể cả khách chưa đăng nhập) — bấm vào mục cần
           đăng nhập thì router guard tự chuyển sang /login kèm redirect -->
      <nav class="app-header__nav" aria-label="Điều hướng chính">
        <RouterLink :to="{ name: 'courses' }" class="app-header__link">{{ messages.nav.path }}</RouterLink>
        <RouterLink :to="{ name: 'simulations' }" class="app-header__link">{{ messages.nav.simulations }}</RouterLink>
        <RouterLink :to="{ name: 'classes' }" class="app-header__link">Lớp học</RouterLink>
        <RouterLink :to="{ name: 'quests' }" class="app-header__link">Thử thách</RouterLink>
        <RouterLink :to="{ name: 'shop' }" class="app-header__link">Cửa hàng</RouterLink>
        <RouterLink v-if="isTeacherOrAdmin" :to="studioTarget" class="app-header__link">
          {{ auth.role === 'TEACHER' ? 'Studio' : 'Quản trị' }}
        </RouterLink>
      </nav>

      <div class="app-header__actions">
        <!-- Web chỉ dùng Dark Mode — đã bỏ nút toggle theme -->
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
              <img
                v-if="equippedAvatar && avatarImageUrl(equippedAvatar.itemKey)"
                :src="avatarImageUrl(equippedAvatar.itemKey)"
                :alt="equippedAvatar.name"
                class="app-header__user-avatar-image"
              />
              <img
                v-else-if="auth.user?.avatarUrl"
                :src="auth.user.avatarUrl"
                :alt="auth.user.displayName ?? 'Avatar'"
                class="app-header__user-avatar-image"
                @error="($event.target as HTMLImageElement).style.display = 'none'"
              />
              <span v-else>{{ auth.user?.displayName?.charAt(0)?.toUpperCase() ?? 'U' }}</span>
            </button>
          </span>
          <Transition name="app-menu">
            <div v-if="menuOpen" class="app-header__menu">
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

      <!-- Hamburger mobile (< 900px) — menu dọc tàng hình kèm nền glass đọc được -->
      <button
        type="button"
        class="app-header__burger"
        :aria-label="mobileNavOpen ? 'Đóng menu' : 'Mở menu'"
        :aria-expanded="mobileNavOpen"
        @click="mobileNavOpen = !mobileNavOpen"
      >
        <span class="app-header__burger-bar" :class="{ 'app-header__burger-bar--open': mobileNavOpen }" />
        <span class="app-header__burger-bar" :class="{ 'app-header__burger-bar--open': mobileNavOpen }" />
        <span class="app-header__burger-bar" :class="{ 'app-header__burger-bar--open': mobileNavOpen }" />
      </button>

      <Transition name="app-menu">
        <nav v-if="mobileNavOpen" class="app-header__mobile-nav" aria-label="Menu di động">
          <RouterLink :to="{ name: 'courses' }" class="app-header__mobile-link" @click="mobileNavOpen = false">
            {{ messages.nav.path }}
          </RouterLink>
          <RouterLink :to="{ name: 'simulations' }" class="app-header__mobile-link" @click="mobileNavOpen = false">
            {{ messages.nav.simulations }}
          </RouterLink>
          <RouterLink :to="{ name: 'classes' }" class="app-header__mobile-link" @click="mobileNavOpen = false">
            Lớp học
          </RouterLink>
          <RouterLink :to="{ name: 'quests' }" class="app-header__mobile-link" @click="mobileNavOpen = false">
            Thử thách
          </RouterLink>
          <RouterLink :to="{ name: 'shop' }" class="app-header__mobile-link" @click="mobileNavOpen = false">
            Cửa hàng
          </RouterLink>
          <RouterLink v-if="isTeacherOrAdmin" :to="studioTarget" class="app-header__mobile-link" @click="mobileNavOpen = false">
            {{ auth.role === 'TEACHER' ? 'Studio' : 'Quản trị' }}
          </RouterLink>
        </nav>
      </Transition>
    </div>
  </header>
</template>

<style scoped>
/* ── Palette "terminal dark" (VisualizationDSA-main theme.css) ── */
.app-header {
  --hdr-bg: rgba(13, 12, 20, 0.0);
  --hdr-bg-scrolled: rgba(13, 12, 20, 0.88);
  --hdr-border: rgba(255, 255, 255, 0.06);
  --hdr-purple: #a855f7;
  --hdr-purple-light: #c084fc;
  --hdr-purple-dark: #7c3aed;
  --hdr-text: rgba(255, 255, 255, 0.88);
  --hdr-muted: rgba(255, 255, 255, 0.55);
  --hdr-faint: rgba(255, 255, 255, 0.35);
}

/* Phase 1 — Light Mode: ghi đè --hdr-* vars để text đọc được trên nền sáng */
html.light .app-header {
  --hdr-bg: rgba(250, 248, 255, 0.0);
  --hdr-bg-scrolled: rgba(250, 248, 255, 0.92);
  --hdr-border: rgba(139, 92, 246, 0.15);
  --hdr-text: #1e1b4b;
  --hdr-muted: #4c1d95;
  --hdr-faint: #6d28d9;
}

/* Phase 1 — Sticky Fixed Header (Phương án A):
   position: fixed thay vì absolute → header luôn hiện dù scroll bao nhiêu.
   App.vue đã có padding-top: var(--app-header-h) cho .app-shell → không bị che nội dung.
   Khi chưa scroll (isScrolled=false): hoàn toàn trong suốt (transparent bg, no border).
   Khi scroll qua 50px (isScrolled=true): glass blur + nền mờ + viền mỏng. */
.app-header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: var(--z-overlay);
  background: var(--hdr-bg);
  border-bottom: 1px solid transparent;
  -webkit-backdrop-filter: none;
  backdrop-filter: none;
  transition: background 300ms ease, border-color 300ms ease, backdrop-filter 300ms ease, box-shadow 300ms ease;
}

.app-header--scrolled {
  background: var(--hdr-bg-scrolled);
  border-color: var(--hdr-border);
  -webkit-backdrop-filter: blur(16px) saturate(1.6);
  backdrop-filter: blur(16px) saturate(1.6);
  box-shadow: 0 1px 24px rgba(0, 0, 0, 0.25);
}

.app-header__inner {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: var(--space-lg);
  padding: 30px 0 10px;
}

.app-header__brand {
  display: inline-flex;
  align-items: center;
  text-decoration: none;
  font-weight: 800;
  color: var(--hdr-text);
}

.app-header__brand-img {
  height: 72px;
  width: auto;
  display: block;
  filter: drop-shadow(0 0 18px rgba(168, 85, 247, 0.35));
  transition: filter 200ms ease, height 300ms ease;
}

/* Phase 1 — Khi scroll: logo thu nhỏ để header compact hơn */
.app-header--scrolled .app-header__brand-img {
  height: 48px;
}

.app-header--scrolled .app-header__inner {
  padding: 12px 0;
}

.app-header__brand:hover .app-header__brand-img {
  filter: drop-shadow(0 0 26px rgba(168, 85, 247, 0.55));
}

.app-header__nav {
  display: flex;
  gap: 2rem;
  overflow: visible;
}

.app-header__link {
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 500;
  letter-spacing: 0.06em;
  color: var(--hdr-muted);
  text-decoration: none;
  white-space: nowrap;
  padding-block: 4px;
  position: relative;
  transition: color 150ms ease;
}

.app-header__link::after {
  content: '';
  position: absolute;
  left: 0;
  right: 100%;
  bottom: -2px;
  height: 2px;
  border-radius: 2px;
  background: linear-gradient(90deg, var(--hdr-purple-light), var(--hdr-purple));
  transition: right 200ms cubic-bezier(0.16, 1, 0.3, 1);
}

.app-header__link:hover { color: var(--hdr-text); }
.app-header__link:hover::after { right: 0; }
.app-header__link.router-link-exact-active {
  color: var(--hdr-purple-light);
  text-shadow: 0 0 12px rgba(168, 85, 247, 0.45);
}
.app-header__link.router-link-exact-active::after { right: 0; }

.app-header__actions {
  position: relative;
  display: flex;
  align-items: center;
  gap: var(--space-sm);
}

.app-header__theme {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 40px;
  height: 40px;
  border: none;
  border-radius: var(--radius-full);
  background: transparent;
  color: var(--hdr-muted);
  cursor: pointer;
  transition: background-color 150ms ease, color 150ms ease;
}

.app-header__theme:hover {
  background: rgba(168, 85, 247, 0.12);
  color: var(--hdr-purple-light);
}

.app-header__theme:focus-visible {
  outline: 2px solid var(--color-ring);
  outline-offset: 2px;
}

.app-header__theme-icon {
  width: 20px;
  height: 20px;
  animation: theme-icon-pop 200ms ease;
}

@keyframes theme-icon-pop {
  from { opacity: 0; transform: rotate(-90deg) scale(0.6); }
  to   { opacity: 1; transform: rotate(0deg) scale(1); }
}

.app-header__login {
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--hdr-text);
  text-decoration: none;
  padding: 0.4rem 0.9rem;
  border: 1px solid var(--hdr-border);
  border-radius: var(--radius-md);
  transition: border-color 150ms ease, color 150ms ease;
}

.app-header__login:hover {
  border-color: var(--hdr-purple);
  color: var(--hdr-purple-light);
}

.app-header__register {
  font-size: var(--text-sm);
  font-weight: 700;
  background: linear-gradient(135deg, var(--hdr-purple), var(--hdr-purple-dark));
  color: #fff;
  padding: 0.4rem 1rem;
  border-radius: var(--radius-md);
  text-decoration: none;
  box-shadow: 0 0 16px rgba(168, 85, 247, 0.25);
  transition: box-shadow 200ms ease, transform 200ms ease;
}

.app-header__register:hover {
  box-shadow: 0 0 26px rgba(168, 85, 247, 0.4);
  transform: translateY(-1px);
}

.app-header__user-frame {
  border-radius: var(--radius-full);
  padding: 2px;
  display: inline-flex;
  line-height: 0;
}

.app-header__user-frame--neon  { background: linear-gradient(135deg, #ec4899, #22d3ee); box-shadow: 0 0 12px rgba(236, 72, 153, 0.45); }
.app-header__user-frame--gold  { background: linear-gradient(135deg, #f59e0b, #fde68a, #f59e0b); box-shadow: 0 0 14px rgba(250, 204, 21, 0.5); }
.app-header__user-frame--cyber { background: linear-gradient(135deg, #22d3ee, #6366f1); box-shadow: 0 0 12px rgba(34, 211, 238, 0.45); }
.app-header__user-frame--fire  { background: linear-gradient(135deg, #ef4444, #f97316); box-shadow: 0 0 12px rgba(239, 68, 68, 0.5); }
.app-header__user-frame--ice   { background: linear-gradient(135deg, #7dd3fc, #93c5fd); box-shadow: 0 0 10px rgba(125, 211, 252, 0.5); }
.app-header__user-frame--default { background: linear-gradient(135deg, var(--hdr-purple-light), var(--hdr-purple-dark)); box-shadow: 0 0 10px rgba(168, 85, 247, 0.4); }

.app-header__user {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background: linear-gradient(135deg, var(--hdr-purple), var(--hdr-purple-dark));
  color: #fff;
  border: none;
  font-weight: 800;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
}

.app-header__user-avatar-image { width:100%; height:100%; object-fit:cover; border-radius:inherit; display:block; }
.app-header__user-avatar--cyber  { background: linear-gradient(135deg, #0e7490, #155e75); color: #a5f3fc; }
.app-header__user-avatar--gold   { background: linear-gradient(135deg, #b45309, #92400e); color: #fef3c7; }
.app-header__user-avatar--neon   { background: linear-gradient(135deg, #be185d, #6b21a8); color: #fbcfe8; }
.app-header__user-avatar--wizard { background: linear-gradient(135deg, #6d28d9, #4c1d95); color: #ddd6fe; }
.app-header__user-avatar--bot    { background: linear-gradient(135deg, #0f766e, #134e4a); color: #99f6e4; }

.app-header__menu {
  position: absolute;
  top: calc(100% + 8px);
  right: 0;
  min-width: 200px;
  display: flex;
  flex-direction: column;
  padding: var(--space-sm);
  background: rgba(13, 12, 20, 0.92);
  -webkit-backdrop-filter: blur(20px) saturate(1.4);
  backdrop-filter: blur(20px) saturate(1.4);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: var(--radius-lg);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.5);
}

.app-header__menu-item {
  background: none;
  border: none;
  text-align: left;
  padding: 0.5rem var(--space-sm);
  border-radius: var(--radius-md);
  font-size: var(--text-sm);
  font-weight: 600;
  color: var(--hdr-text);
  text-decoration: none;
  cursor: pointer;
}

.app-header__menu-item:hover {
  background: rgba(168, 85, 247, 0.12);
  color: var(--hdr-purple-light);
}

html.light .app-header__menu {
  background: rgba(250, 248, 255, 0.97);
  border-color: rgba(139, 92, 246, 0.12);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.15);
}

html.light .app-header__menu-item:hover {
  background: rgba(139, 92, 246, 0.08);
  color: #4c1d95;
}

.app-header__menu-item--danger { color: #d07070; }
.app-header__menu-item--danger:hover { color: #e08c8c; background: rgba(184, 92, 92, 0.12); }

.app-menu-enter-active,
.app-menu-leave-active { transition: opacity 150ms ease, transform 150ms ease; }
.app-menu-enter-from,
.app-menu-leave-to { opacity: 0; transform: translateY(-4px); }

/* ── Hamburger (mobile) ── */
.app-header__burger {
  display: none;
  flex-direction: column;
  justify-content: center;
  gap: 4px;
  width: 38px;
  height: 38px;
  padding: 8px;
  background: transparent;
  border: 1px solid var(--hdr-border);
  border-radius: var(--radius-md);
  cursor: pointer;
}

.app-header__burger-bar {
  display: block;
  height: 2px;
  border-radius: 2px;
  background: var(--hdr-text);
  transition: transform 200ms cubic-bezier(0.16, 1, 0.3, 1), opacity 150ms ease;
}

.app-header__burger-bar--open:nth-child(1) { transform: translateY(6px) rotate(45deg); }
.app-header__burger-bar--open:nth-child(2) { opacity: 0; }
.app-header__burger-bar--open:nth-child(3) { transform: translateY(-6px) rotate(-45deg); }

.app-header__mobile-nav {
  position: absolute;
  top: calc(100% + 8px);
  left: var(--space-md);
  right: var(--space-md);
  display: flex;
  flex-direction: column;
  gap: 2px;
  padding: var(--space-sm);
  background: rgba(13, 12, 20, 0.95);
  -webkit-backdrop-filter: blur(20px) saturate(1.4);
  backdrop-filter: blur(20px) saturate(1.4);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: var(--radius-lg);
  box-shadow: 0 12px 40px rgba(0, 0, 0, 0.5);
}

/* Phase 1 Light mode: mobile nav cũng cần nền mờ sáng */
html.light .app-header__mobile-nav {
  background: rgba(250, 248, 255, 0.97);
  border-color: rgba(139, 92, 246, 0.12);
}

.app-header__mobile-link {
  padding: 0.65rem var(--space-sm);
  border-radius: var(--radius-md);
  font-family: var(--font-mono);
  font-size: var(--text-sm);
  font-weight: 500;
  letter-spacing: 0.05em;
  color: var(--hdr-text);
  text-decoration: none;
  transition: background 150ms ease, color 150ms ease;
}

.app-header__mobile-link:hover {
  background: rgba(168, 85, 247, 0.12);
  color: var(--hdr-purple-light);
}

.app-header__mobile-link.router-link-exact-active {
  color: var(--hdr-purple-light);
}

@media (max-width: 900px) {
  .app-header__nav { display: none; }
  .app-header__burger { display: inline-flex; }
  /* Mobile: brand trái — action + burger phải */
  .app-header__inner { justify-content: space-between; }

  /* Phase 1 — Mobile: logo nhỏ hơn để header không chiếm quá nhiều không gian */
  .app-header__brand-img { height: 40px; }
  .app-header--scrolled .app-header__brand-img { height: 32px; }

  /* Phase 1 — Mobile: ẩn nút Đăng nhập + Đăng ký (đã có trong mobile nav drawer) */
  .app-header__login,
  .app-header__register { display: none; }
}
</style>

