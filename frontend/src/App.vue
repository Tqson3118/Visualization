<script setup lang="ts">
import { onBeforeUnmount } from 'vue';
import { RouterView } from 'vue-router';
import { Toaster } from 'vue-sonner';

import AppHeader from '@/components/layout/AppHeader.vue';
import { useLenis } from '@/composables/useLenis';
import { messages } from '@/i18n/vi';
import { useUiStore } from '@/stores/ui';

const ui = useUiStore();

// G-F2a: smooth scroll toàn cục (singleton — App đời là tạo 1 lần duy nhất).
const { scrollToTop } = useLenis();

// Đánh dấu điều hướng back/forward (popstate) → không reset scroll về đầu,
// giữ vị trí cũ như savedPosition.
let isHistoryNavigation = false;
function onPopState(): void {
  isHistoryNavigation = true;
}
window.addEventListener('popstate', onPopState);
onBeforeUnmount(() => window.removeEventListener('popstate', onPopState));

/** Sau khi trang mới enter xong → về đầu trang (bỏ qua nếu là back/forward). */
function onPageEnter(): void {
  if (isHistoryNavigation) {
    isHistoryNavigation = false;
    return;
  }
  scrollToTop(true);
}
</script>

<template>
  <div class="app-shell">
    <AppHeader />

    <main class="app-shell__main">
      <!-- G-F2a: page transition theo route.fullPath — fade + slide nhẹ,
           mode out-in tránh nhảy layout, tôn trọng prefers-reduced-motion
           (global.css đã cắt transition khi reduce). -->
      <RouterView v-slot="{ Component, route }">
        <Transition name="page" mode="out-in" @after-enter="onPageEnter">
          <component :is="Component" :key="route.fullPath" />
        </Transition>
      </RouterView>
    </main>

    <footer class="app-shell__footer">
      <div class="container app-shell__footer-inner">
        <span>{{ messages.app.name }} — {{ messages.app.tagline }}</span>
        <nav aria-label="Footer">
          <RouterLink :to="{ name: 'help' }">Trợ giúp</RouterLink>
          <RouterLink :to="{ name: 'help' }">Liên hệ</RouterLink>
          <RouterLink :to="{ name: 'privacy' }">Chính sách bảo mật</RouterLink>
        </nav>
      </div>
    </footer>

    <!-- Toast G-F1b: vue-sonner (thay ToastContainer tự xây) — G-F2a giữ mount hoàn chỉnh -->
    <Toaster position="top-right" :theme="ui.theme" rich-colors close-button />
  </div>
</template>

<style scoped>
.app-shell {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
}

.app-shell__main {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.app-shell__footer {
  margin-top: var(--space-2xl);
  border-top: 1px solid var(--color-border);
  background: var(--color-surface);
  padding-block: var(--space-md);
}

.app-shell__footer-inner {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-md);
  flex-wrap: wrap;
  font-size: var(--text-xs);
  color: var(--color-text-muted);
}

.app-shell__footer nav { display: flex; gap: var(--space-md); }
</style>
