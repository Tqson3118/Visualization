<template>
  
  
  
  
  <div class="app-shell" :class="{ 'is-embed': isMinimalMode, 'has-sidebar': !isLandingPage && !isMinimalMode && !isAuthPage }">
    
    <AppHeader
      v-if="!isMinimalMode && !isAuthPage"
      @logout="handleLogout"
      @openLogin="handleOpenLogin"
      @toggleSidebar="sidebar?.openMobile?.()"
    />
    
    <!-- Sidebar: ẩn trên landing + auth pages -->
    <AppSidebar 
      v-if="!isMinimalMode && !isLandingPage && !isAuthPage"
      ref="sidebar"
      @toggleSidebar="sidebar?.openMobile?.()"
    />

    <!-- ═══════════════════════════════════════════════════════════ -->
    <!-- BODY — Sidebar + Main Content Layout -->
    <div class="app-body">
      <!-- ── BREADCRUMB (Mobile) ── -->
      <div v-if="!isMinimalMode && !isLandingPage && !isAuthPage" class="breadcrumb-mobile">
        <router-link to="/" class="breadcrumb-item">
          <BaseIcon name="home" class="w-3.5 h-3.5 inline-block mr-1 align-text-bottom" />
          Trang chủ
        </router-link>
        <span class="breadcrumb-separator" aria-hidden="true">/</span>
        <span class="breadcrumb-item active">{{ currentPageTitle }}</span>
      </div>

      <!-- ── MAIN CONTENT AREA ── -->
      
      <main class="app-main" :class="{ 'app-main--full': isLandingPage || isAuthPage }">
        <RouterView v-slot="{ Component }">
          <Transition name="page-fade">
            <component
              v-if="Component"
              :is="Component"
              :key="$route.fullPath"
              class="app-view"
              @openLogin="handleOpenLogin"
            />
          </Transition>
        </RouterView>
      </main>
    </div>
  </div>

  <!-- Impersonation Banner (Phase C) -->
  <div v-if="authStore.isImpersonating" class="impersonate-banner">
    <div class="impersonate-banner__pulse"></div>
    <div class="impersonate-banner__text">
      <span class="impersonate-banner__icon" aria-hidden="true"><BaseIcon name="impersonate" class="w-4 h-4" /></span>
      <span>Đang đóng vai: <strong>{{ authStore.userName }}</strong></span>
    </div>
    <button class="impersonate-banner__btn" @click="handleStopImpersonating">
      Thoát đóng vai
    </button>
  </div>

  <!-- Sync Error Banner -->
  <div v-if="progressStore.isSyncError" class="sync-error-banner">
    <div class="sync-error-banner__pulse"></div>
    <div class="sync-error-banner__text">
      <span class="sync-error-banner__icon" aria-hidden="true"><BaseIcon name="warning" class="w-4 h-4" /></span>
      <span>Đồng bộ tiến trình thất bại.</span>
    </div>
    <button class="sync-error-banner__btn" :disabled="isSyncingProgress" @click="handleRetrySync">
      {{ isSyncingProgress ? 'Đang thử...' : 'Thử lại' }}
    </button>
  </div>

  <BottomMobileNav v-if="!isMinimalMode" />
  
  <ToastContainer />
  
  <ConfirmDialogHost />
  
  
  <GuidedTourOverlay />
  
  <!-- Epic 2 Gamification Modals -->
  <OutOfHeartsModal 
    :show="sessionStore.showOutOfHeartsModal"
    :recoveryInfo="sessionStore.outOfHeartsRecoveryInfo"
    @close="sessionStore.closeOutOfHearts()"
    @watch-ad="handleWatchAd"
  />
  
  <SessionResumePrompt 
    :show="sessionStore.showResumePromptModal"
    :currentStep="sessionStore.pendingSessionInfo?.currentStep || 'Theory'"
    @resume="sessionStore.handleResumePromptDecision(true)"
    @restart="sessionStore.handleResumePromptDecision(false)"
  />
</template>

<script setup lang="ts">
import { useToastStore } from '@/composables/useToast';
import { ref, computed, onMounted } from 'vue';
import { RouterView, useRoute, useRouter } from 'vue-router';
import { useAuthStore } from './features/auth/store/useAuthStore';
import { useGuidedTourStore } from './features/guided-tour/store/useGuidedTourStore';
import { API_BASE_URL } from './services/apiConfig';
import AppHeader from './components/AppHeader.vue';
import AppSidebar from './components/AppSidebar.vue';
import BottomMobileNav from './components/BottomMobileNav.vue';
import ToastContainer from './components/ToastContainer.vue';
import ConfirmDialogHost from './features/ui/components/ConfirmDialogHost.vue';
import { useUserProgressStore } from './features/user-progress/store/useUserProgressStore';
// NotificationBell removed
import HeartDisplay from './components/common/HeartDisplay.vue';
import { APP_TABS } from './appTabs';
import type { TabGroup, TabItem } from './appTabs';
import BaseIcon from './shared/components/BaseIcon.vue';

// Epic 2 Session
import { useSessionStore } from './features/gamification-engine/store/useSessionStore';
import OutOfHeartsModal from './features/gamification/components/OutOfHeartsModal.vue';
import SessionResumePrompt from './features/gamification/components/SessionResumePrompt.vue';
import GuidedTourOverlay from './features/guided-tour/components/GuidedTourOverlay.vue';
import { useThemeStore } from './shared/store/useThemeStore';

const authStore      = useAuthStore();
const progressStore  = useUserProgressStore();
const sessionStore   = useSessionStore();
const themeStore     = useThemeStore();
const tourStore      = useGuidedTourStore();
const toastStore     = useToastStore();
const route          = useRoute();
const router         = useRouter();
const isSyncingProgress = ref(false);

const sidebar = ref<InstanceType<typeof AppSidebar> | null>(null);


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

function isTabVisible(tab: TabItem): boolean {
  if (tab.requiresAuth && !authStore.isAuthenticated) return false;
  if (tab.requiresRole) {
    const role = authStore.userRole;
    if (role === 'Admin') return true;
    if (role !== tab.requiresRole) return false;
  }
  return true;
}

const isLandingPage = computed(() => route.name === 'landing');
const isAuthPage    = computed(() => route.name === 'login' || route.name === 'register');
const isMinimalMode = computed(() => route.path === '/embed' && route.query.algo !== undefined);

const currentPageTitle = computed(() => {
  const matched = route.matched[route.matched.length - 1];
  return matched?.meta?.title || route.meta?.title || 'Dashboard';
});

async function handleRetrySync(): Promise<void> {
  isSyncingProgress.value = true;
  try {
    await progressStore.loadProgress();
  } finally {
    isSyncingProgress.value = false;
  }
}

async function handleLogout(): Promise<void> {
  if (authStore.isStatelessMode) {
    await authStore.statelessLogout();
  } else {
    await authStore.logOut();
  }
  router.push('/');
}

function handleOpenLogin(): void {
  router.push('/login');
}

function handleStopImpersonating(): void {
  authStore.stopImpersonating();
  toastStore.info('Đã thoát chế độ đóng vai. Khôi phục tài khoản Admin.');
  router.push('/admin');
}

onMounted(() => {
  authStore.init();
  // tourStore.initTour();
  themeStore.initTheme();
  authStore.statelessInit();
  tourStore.initTour();
});

async function handleWatchAd() {
  try {
    const token = authStore.getAccessToken();
    const res = await fetch(`${API_BASE_URL}/api/v1/hearts/watch-ad`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        ...(token ? { Authorization: `Bearer ${token}` } : {}),
      },
    });

    if (!res.ok) {
      if (res.status === 429) {
        toastStore.warning('Bạn đã dùng hết lượt xem quảng cáo trong 24h. Hãy chờ tim hồi phục.');
      } else {
        toastStore.error('Không thể xem quảng cáo. Vui lòng thử lại sau.');
      }
      return;
    }

    const data = await res.json();
    if (authStore.currentUser) {
      authStore.currentUser.hearts = data.hearts ?? authStore.currentUser.hearts;
      authStore.currentUser.maxHearts = data.maxHearts ?? authStore.currentUser.maxHearts;
    }
    toastStore.success(`Bạn đã nhận trái tim. Hiện có ${data.hearts ?? '?'} ❤️`);
    sessionStore.closeOutOfHearts();
  } catch (error) {
    console.error(error);
    toastStore.error('Lỗi kết nối khi xem quảng cáo. Vui lòng thử lại.');
  }
}
</script>

<style>
@import "./App.css";
</style>