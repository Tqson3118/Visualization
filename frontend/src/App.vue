<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref } from 'vue';
import { RouterView, useRoute } from 'vue-router';
import { Toaster } from 'vue-sonner';

import AppHeader from '@/components/layout/AppHeader.vue';
import AppFooter from '@/components/layout/AppFooter.vue';
import ConfirmModal from '@/components/ui/ConfirmModal.vue';
import { startCosmicField } from '@/composables/useCosmicField';
import { useLenis } from '@/composables/useLenis';
import { useUiStore } from '@/stores/ui';

const ui = useUiStore();
const route = useRoute();

// Simulator dùng full-height (full viewport) → main cần chiều cao cố định + ẩn footer.
const SANDBOX_ROUTES = ['simulator'];
const isSandboxRoute = computed(() => route.name !== undefined && SANDBOX_ROUTES.includes(String(route.name)));

// G-F2a: smooth scroll toàn cục (singleton — App đời là tạo 1 lần duy nhất).
const { scrollToTop } = useLenis();

// Vũ trụ tương tác (chòm sao theo chuột) — canvas fixed nền, pointer-events: none
const cosmicCanvas = ref<HTMLCanvasElement | null>(null);
let stopCosmic: (() => void) | null = null;
onMounted(() => {
  if (cosmicCanvas.value) stopCosmic = startCosmicField(cosmicCanvas.value);
});
onBeforeUnmount(() => {
  stopCosmic?.();
});

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
    <!-- Chòm sao tương tác — các điểm nối nhau, rê chuột → chòm theo chuột -->
    <canvas ref="cosmicCanvas" class="cosmic-field" aria-hidden="true" />

    <AppHeader />

    <main class="app-shell__main" :class="{ 'app-shell__main--sandbox': isSandboxRoute, 'sandbox-theme': isSandboxRoute }">
      <!-- G-F2a: page transition theo route.fullPath — fade + slide nhẹ,
           mode out-in tránh nhảy layout, tôn trọng prefers-reduced-motion
           (global.css đã cắt transition khi reduce). -->
      <RouterView v-slot="{ Component, route }">
        <component :is="Component" :key="route.name === 'lesson-study' ? 'lesson-study' : route.path" />
      </RouterView>
    </main>

    <!-- Footer bê từ VisualizationDSA3 (ẩn trên màn hình học bài và sandbox full-height) -->
    <AppFooter v-if="!['lesson-study'].includes(String(route.name)) && !isSandboxRoute" />

    <!-- Global Confirm Modal -->
    <ConfirmModal
      v-if="ui.modalState.kind === 'confirm'"
      :show="ui.modalState.open"
      :title="(ui.modalState.payload as any)?.title || 'Xác nhận'"
      :message="(ui.modalState.payload as any)?.message || 'Bạn có chắc chắn muốn thực hiện hành động này?'"
      :confirm-text="(ui.modalState.payload as any)?.confirmLabel || 'Xác nhận'"
      :cancel-text="(ui.modalState.payload as any)?.cancelLabel || 'Hủy'"
      :variant="(ui.modalState.payload as any)?.variant || 'primary'"
      @update:show="(open) => { if (!open) ui.closeModal(false); }"
      @confirm="ui.closeModal(true)"
    />

    <!-- Toast G-F1b: vue-sonner (thay ToastContainer tự xây) — G-F2a giữ mount hoàn chỉnh -->
    <Toaster position="top-right" :theme="ui.theme" rich-colors close-button />
  </div>
</template>

<style scoped>
.app-shell {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  padding-top: var(--app-header-h, 68px);
}

/* Chòm sao tương tác — cố định theo viewport, nằm dưới mọi nội dung */
.cosmic-field {
  position: fixed;
  inset: 0;
  z-index: 0;
  pointer-events: none;
}

/* Đưa toàn bộ nội dung lên trên canvas (vẽ phía trên chòm sao) */
.app-shell__main,
.app-shell__footer {
  position: relative;
  z-index: 1;
}

.app-shell__main {
  flex: 1;
  display: flex;
  flex-direction: column;
}

.app-shell__main--sandbox {
  flex: 1 1 auto;
  min-height: calc(100vh - var(--app-header-h, 68px));
  overflow: visible;
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

/* C9: Print styles toàn cục chuẩn văn bản trắng đen */
@media print {
  @page {
    size: A4 portrait;
    margin: 12mm;
  }
  *,
  *::before,
  *::after {
    background: transparent !important;
    color: #111827 !important;
    box-shadow: none !important;
    text-shadow: none !important;
  }
  body,
  .app-shell,
  .app-shell__main {
    background: #ffffff !important;
    color: #111827 !important;
    padding: 0 !important;
    margin: 0 !important;
    min-height: auto !important;
  }
  .app-header,
  .app-footer,
  .cosmic-field,
  .no-print,
  button,
  nav,
  [role="dialog"],
  [data-sonner-toaster] {
    display: none !important;
  }
  .app-shell {
    padding-top: 0 !important;
  }
  table,
  th,
  td {
    border-color: #d1d5db !important;
    color: #111827 !important;
  }
}
</style>
