<script setup lang="ts">
import { RouterView } from 'vue-router';
import { Toaster } from 'vue-sonner';

import AppHeader from '@/components/layout/AppHeader.vue';
import { messages } from '@/i18n/vi';
import { useUiStore } from '@/stores/ui';

const ui = useUiStore();
</script>

<template>
  <div class="app-shell">
    <AppHeader />

    <main class="app-shell__main">
      <RouterView />
    </main>

    <footer class="app-shell__footer">
      <div class="container app-shell__footer-inner">
        <span>{{ messages.app.name }} — {{ messages.app.tagline }}</span>
        <nav aria-label="Footer">
          <RouterLink :to="{ name: 'help' }">Trợ giúp</RouterLink>
          <RouterLink :to="{ name: 'privacy' }">Chính sách bảo mật</RouterLink>
        </nav>
      </div>
    </footer>

    <!-- Toast G-F1b: vue-sonner (thay ToastContainer tự xây) -->
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
