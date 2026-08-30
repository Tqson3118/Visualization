<script setup lang="ts">
// SimulatorView — Trực quan hóa CTDL & Giải thuật (Màn 05: Visual Studio)
// 3 vùng chuẩn: Mã giả (trái 3/12) · Canvas + Điều khiển (giữa 6/12) · Giải thích (phải 3/12)
// Full viewport, không cuộn chuột thừa, không nhồi Quiz hay Codelab vào Visualizer.
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue';
import { useRoute, useRouter } from 'vue-router';

import ControlBar from '@/components/simulator/ControlBar.vue';
import ExplainPanel from '@/components/simulator/ExplainPanel.vue';
import CanvasArea from '@/components/simulator/CanvasArea.vue';
import PseudocodePanel from '@/components/simulator/PseudocodePanel.vue';
import LegendPanel from '@/components/simulator/LegendPanel.vue';
import StatsBar from '@/components/simulator/StatsBar.vue';
import InputModal from '@/components/simulator/InputModal.vue';
import CallStackPanel from '@/components/simulator/CallStackPanel.vue';
import DemoBanner from '@/components/simulator/DemoBanner.vue';
import { useSimulation } from '@/composables/useSimulation';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import * as favoritesApi from '@/api/favorites';
import { getCatalogMeta } from '@/engines/catalog';
import type { InputConfig } from '@/engines/core/types';
import { buildSimOverviewHtml } from '@/utils/simOverview';
import { getReference } from '@/data/referenceLinks';
import { messages } from '@/i18n/vi';
import { ArrowLeft, ChevronDown, ChevronRight, ExternalLink, Maximize2, Minimize2, Settings2, Share2, Star } from 'lucide-vue-next';
import Button from '@/components/ui/Button.vue';
import ProseContent from '@/components/ui/ProseContent.vue';

const route = useRoute();
const router = useRouter();
const auth = useAuthStore();
const ui = useUiStore();

const key = computed(() => String(route.params.key ?? ''));

const isDemoKey = computed(() => getCatalogMeta(key.value)?.demoAllowed === true);
const isDemo = computed(() => !auth.isAuthenticated);

onMounted(() => {
  window.addEventListener('keydown', onKeydown);
  checkFavorite();
});

const initialInput = computed<InputConfig | undefined>(() => {
  if (!route.query.input) return undefined;
  try {
    const rawInput = route.query.input as string;
    const parsed = JSON.parse(decodeURIComponent(rawInput));
    if (parsed !== null && parsed !== undefined) {
      if (parsed && typeof parsed === 'object' && 'data' in parsed) {
        return parsed as InputConfig;
      }
      return { data: parsed } as InputConfig;
    }
  } catch (e) {
    console.warn('Failed to parse input from query string', e);
  }
  return undefined;
});

const {
  currentSim,
  currentStep,
  currentIndex,
  steps,
  speed,
  status,
  loading,
  loadError,
  play,
  pause,
  stepForward,
  stepBack,
  reset,
  jumpTo,
  setSpeed,
  configureInput,
  generator,
  inputConfig,
  breakpoints,
  breakpointHit,
  toggleBreakpoint,
} = useSimulation(key, initialInput);

watch(steps, (newSteps) => {
  if (newSteps.length > 0 && route.query.step) {
    const targetStep = parseInt(String(route.query.step), 10);
    if (!Number.isNaN(targetStep) && targetStep >= 0) {
      jumpTo(targetStep);
    }
  }
}, { immediate: true });

const configOpen = ref(false);
const showLegend = ref(false);
const favorite = ref(false);
const showCallStack = ref(false);
const theoryOpen = ref(false);
const pseudocodeCollapsed = ref(false);
const focusMode = ref(false);
const mobileActiveTab = ref<'canvas' | 'code' | 'explain'>('canvas');
const renderOptions = ref({ showIndex: true, showValues: true, zoom: 1 });

// ── Dropdown link tham khảo ──
const docOpen = ref(false);
const docMenuRef = ref<HTMLElement | null>(null);

const docLinks = computed(() => {
  const refData = getReference(key.value);
  if (!refData) return [];
  const links: { label: string; url: string }[] = [];
  if (refData.wikipedia) links.push({ label: 'Wikipedia', url: refData.wikipedia });
  if (refData.geeksforgeeks) links.push({ label: 'GeeksforGeeks', url: refData.geeksforgeeks });
  return links;
});

function onDocPointerDown(event: PointerEvent): void {
  if (docMenuRef.value && !docMenuRef.value.contains(event.target as Node)) {
    docOpen.value = false;
  }
}

function onDocKeydown(event: KeyboardEvent): void {
  if (event.key === 'Escape') docOpen.value = false;
}

watch(docOpen, (open) => {
  if (open) {
    document.addEventListener('pointerdown', onDocPointerDown);
    document.addEventListener('keydown', onDocKeydown);
  } else {
    document.removeEventListener('pointerdown', onDocPointerDown);
    document.removeEventListener('keydown', onDocKeydown);
  }
});

const notFound = computed(() => !currentSim.value && steps.value.length === 0 && status.value === 'idle' && !loading.value && !loadError.value);

const categoryLabel = computed(() =>
  getCatalogMeta(key.value)?.category === 'structure'
    ? messages.simulator.categoryStructure
    : messages.simulator.categoryAlgorithm,
);

const complexityFull = computed(() => {
  const meta = getCatalogMeta(key.value);
  if (!meta) return '';
  const { best, average, worst, space } = meta.complexity;
  if (!generator.value) return '';
  const c = generator.value.complexity;
  return `Tệ nhất: ${c.worst} | Trung bình: ${c.average} | Tốt nhất: ${c.best} | Bộ nhớ: ${c.space}`;
});

onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKeydown);
  document.removeEventListener('pointerdown', onDocPointerDown);
  document.removeEventListener('keydown', onDocKeydown);
});

watch(key, () => {
  reset();
  void checkFavorite();
});

/** Phím tắt: Space, ->/<- , Home/End, [ ] */
function onKeydown(event: KeyboardEvent): void {
  const target = event.target as HTMLElement;
  if (
    (target.tagName === 'INPUT' && (target as HTMLInputElement).type !== 'range') ||
    target.tagName === 'TEXTAREA' ||
    target.tagName === 'SELECT'
  ) {
    return;
  }
  if (configOpen.value) return;
  if (event.key === ' ' || event.code === 'Space') {
    event.preventDefault();
    status.value === 'running' ? pause() : play();
    return;
  }
  if (event.key === 'ArrowRight' || event.code === 'ArrowRight') {
    event.preventDefault();
    stepForward();
    return;
  }
  if (event.key === 'ArrowLeft' || event.code === 'ArrowLeft') {
    event.preventDefault();
    stepBack();
    return;
  }
  if (event.key === 'Home' || event.code === 'Home') {
    event.preventDefault();
    reset();
    return;
  }
  if (event.key === 'End' || event.code === 'End') {
    event.preventDefault();
    if (steps.value.length > 0) {
      jumpTo(steps.value.length - 1);
    }
    return;
  }
  if (event.key === '[' || event.code === 'BracketLeft') {
    setSpeed(Math.max(0.25, speed.value / 2));
    return;
  }
  if (event.key === ']' || event.code === 'BracketRight') {
    setSpeed(Math.min(4, speed.value * 2));
    return;
  }
}

async function checkFavorite(): Promise<void> {
  if (!auth.isAuthenticated) return;
  try {
    const list = await favoritesApi.fetchFavorites();
    favorite.value = list.some((f) => f.simKey === key.value);
  } catch {
    favorite.value = false;
  }
}

async function toggleFavorite(): Promise<void> {
  if (!auth.isAuthenticated) {
    ui.showToast(messages.simulator.toastFavoriteLogin, 'info');
    return;
  }
  try {
    if (favorite.value) {
      const list = await favoritesApi.fetchFavorites();
      const item = list.find((f) => f.simKey === key.value);
      if (item) await favoritesApi.removeFavorite(item.id);
      favorite.value = false;
    } else {
      await favoritesApi.addFavorite({ simKey: key.value, input: inputConfig.value?.data });
      favorite.value = true;
      ui.showToast(messages.simulator.toastFavoriteAdded, 'success');
    }
  } catch {
    ui.showToast(messages.simulator.toastFavoriteError, 'error');
  }
}

function shareLink(): void {
  const resolved = router.resolve({ name: 'simulator', params: { key: key.value } });
  const url = new URL(resolved.href, window.location.origin);
  if (inputConfig.value?.data) {
    url.searchParams.set('input', encodeURIComponent(JSON.stringify(inputConfig.value.data)));
  }
  void navigator.clipboard?.writeText(url.toString()).then(() => {
    ui.showToast(messages.simulator.toastCopied, 'success');
  });
}

const currentVariables = computed(() => currentStep.value?.variables ?? {});
const hasCallStack = computed(() => {
  const raw = currentVariables.value?.callStack;
  return Array.isArray(raw) && raw.length > 0;
});
const theoryHtml = computed(() => buildSimOverviewHtml(getCatalogMeta(key.value)));
</script>

<template>
  <div class="simulator-app">
    <!-- Header thanh gọn: Breadcrumb + Tiêu đề + Độ phức tạp + Nút thao tác -->
    <header class="simulator-header">
      <div class="simulator-header__left">
        <RouterLink :to="{ name: 'simulations' }" class="simulator-header__back" title="Quay lại danh mục">
          <ArrowLeft :size="16" />
          <span>{{ messages.simulator.breadcrumbExplore }}</span>
        </RouterLink>
        <span class="simulator-header__divider">/</span>
        <h1 class="simulator-header__title">{{ currentSim?.title ?? key }}</h1>
        <span v-if="generator" class="simulator-header__badge">{{ generator.dataStructure }}</span>
        <span v-if="generator" class="simulator-header__badge simulator-header__badge--accent" :title="complexityFull">
          {{ messages.simulator.complexityChip }}: {{ generator.complexity.average }}
        </span>
      </div>

      <div class="simulator-header__actions">
        <Button
          variant="ghost"
          size="sm"
          :class="{ 'text-purple-300 bg-purple-500/20 border border-purple-500/40': focusMode }"
          :aria-label="focusMode ? 'Tắt chế độ tập trung' : 'Bật chế độ tập trung'"
          :title="focusMode ? 'Thoát chế độ tập trung' : 'Chế độ tập trung (ẩn thanh bên)'"
          class="hidden sm:inline-flex items-center gap-1 text-xs"
          @click="focusMode = !focusMode"
        >
          <component :is="focusMode ? Minimize2 : Maximize2" :size="14" />
          <span>{{ focusMode ? 'Thu nhỏ' : 'Tập trung' }}</span>
        </Button>

        <Button
          variant="ghost"
          size="sm"
          :class="{ 'text-amber-400': favorite }"
          :aria-label="favorite ? messages.simulator.unfavoriteAria : messages.simulator.favoriteAria"
          @click="toggleFavorite"
        >
          <Star :size="15" :fill="favorite ? 'currentColor' : 'none'" />
        </Button>

        <Button variant="ghost" size="sm" :aria-label="messages.simulator.shareAria" @click="shareLink">
          <Share2 :size="15" />
        </Button>

        <Button size="sm" variant="secondary" class="flex items-center gap-1.5" @click="configOpen = true">
          <Settings2 :size="14" />
          <span>{{ messages.simulator.inputConfig }}</span>
        </Button>

        <div v-if="docLinks && docLinks.length > 0" ref="docMenuRef" class="relative">
          <Button
            size="sm"
            variant="ghost"
            @click="docOpen = !docOpen"
          >
            {{ messages.simulator.docButton }}
          </Button>
          <div v-if="docOpen" class="doc-menu">
            <p class="text-xs text-vdsa-muted px-2 py-1">{{ messages.simulator.docTitle }}</p>
            <a
              v-for="link in docLinks"
              :key="link.url"
              :href="link.url"
              target="_blank"
              rel="noopener noreferrer"
              class="doc-menu__item"
              @click="docOpen = false"
            >
              <span>{{ link.label }}</span>
              <ExternalLink :size="13" />
            </a>
          </div>
        </div>
      </div>
    </header>

    <DemoBanner v-if="isDemo" :sim-key="key" class="shrink-0 mx-4 mt-2" />

    <!-- Khu vực nạp hoặc lỗi -->
    <div v-if="loading" class="flex-1 flex items-center justify-center text-vdsa-muted" role="status">
      <div class="inline-block w-8 h-8 border-3 border-vdsa-accent/20 border-t-vdsa-accent rounded-full animate-spin mr-3"></div>
      <span>{{ messages.common.loading }} {{ messages.simulator.loadingSim }}</span>
    </div>

    <div v-else-if="loadError || notFound" class="flex-1 flex flex-col items-center justify-center gap-3 text-center" role="alert">
      <p class="text-rose-400 font-semibold">{{ loadError || `${messages.simulator.notFound} (${key})` }}</p>
      <Button size="sm" variant="secondary" @click="router.push({ name: 'simulations' })">
        {{ messages.simulator.backToCatalog }}
      </Button>
    </div>

    <!-- Mobile Segmented Tabs (màn hình <= 1024px) -->
    <nav
      v-if="!loading && !loadError && !notFound"
      class="simulator-mobile-tabs lg:hidden flex items-center bg-[#13111C] p-1 rounded-xl border border-white/10 gap-1 shrink-0"
      aria-label="Chuyển đổi khung nhìn"
    >
      <button
        type="button"
        class="flex-1 py-1.5 px-2 rounded-lg text-xs font-bold transition-all text-center cursor-pointer"
        :class="mobileActiveTab === 'canvas' ? 'bg-purple-600 text-white shadow-md' : 'text-slate-400 hover:text-white'"
        @click="mobileActiveTab = 'canvas'"
      >
        Mô phỏng
      </button>
      <button
        type="button"
        class="flex-1 py-1.5 px-2 rounded-lg text-xs font-bold transition-all text-center cursor-pointer"
        :class="mobileActiveTab === 'code' ? 'bg-purple-600 text-white shadow-md' : 'text-slate-400 hover:text-white'"
        @click="mobileActiveTab = 'code'"
      >
        Mã giả
      </button>
      <button
        type="button"
        class="flex-1 py-1.5 px-2 rounded-lg text-xs font-bold transition-all text-center cursor-pointer"
        :class="mobileActiveTab === 'explain' ? 'bg-purple-600 text-white shadow-md' : 'text-slate-400 hover:text-white'"
        @click="mobileActiveTab = 'explain'"
      >
        Giải thích
      </button>
    </nav>

    <!-- 3 Cột Workspace: Mã giả · Canvas + Điều khiển · Giải thích -->
    <!-- Lưu ý: KHÔNG dùng v-else ở đây — nav mobile tabs ở trên có v-if riêng,
         làm đứt chuỗi v-if/v-else (v-else sẽ ghép nhầm với nav → workspace chỉ render
         khi loading/lỗi). Dùng điều kiện tường minh để giữ cả hai hiển thị đúng. -->
    <div
      v-if="!loading && !loadError && !notFound"
      class="simulator-workspace"
      :class="{
        'simulator-workspace--collapsed-left': pseudocodeCollapsed && !focusMode,
        'simulator-workspace--focus': focusMode,
      }"
    >
      <!-- CỘT 1 (Trái): Mã giả (Pseudocode) & Biến -->
      <aside
        v-show="!focusMode"
        class="simulator-col simulator-col--left"
        :class="{ 'hidden lg:flex': mobileActiveTab !== 'code', 'flex': mobileActiveTab === 'code' }"
      >
        <PseudocodePanel
          class="h-full"
          :pseudocode="generator?.pseudocode ?? []"
          :active-line="currentStep?.pseudocodeLine ?? 0"
          :variables="currentVariables"
          :collapsed="pseudocodeCollapsed"
          :breakpoints="breakpoints"
          @update:collapsed="pseudocodeCollapsed = $event"
          @toggle-breakpoint="toggleBreakpoint"
        />
      </aside>

      <!-- CỘT 2 (Giữa): Canvas Trực quan + Điều khiển + Thống kê -->
      <section
        class="simulator-col simulator-col--center"
        :class="{ 'hidden lg:flex': mobileActiveTab !== 'canvas' && !focusMode, 'flex': mobileActiveTab === 'canvas' || focusMode }"
      >
        <div class="simulator-canvas-card">
          <CanvasArea
            :structure="currentStep?.structure ?? null"
            v-model:show-index="renderOptions.showIndex"
            v-model:show-values="renderOptions.showValues"
            v-model:zoom="renderOptions.zoom"
            :empty-text="messages.simulator.canvasPlaceholder"
          />
        </div>

        <div class="simulator-controls-card">
          <StatsBar
            :comparisons="currentStep?.stats.comparisons ?? 0"
            :swaps="currentStep?.stats.swaps ?? 0"
            :writes="currentStep?.stats.writes ?? 0"
            :step="currentIndex"
            :total-steps="steps.length"
          />

          <div
            v-if="breakpointHit !== null && status === 'paused'"
            class="px-3 py-1 bg-rose-500/20 text-rose-300 text-xs font-mono rounded-full self-start flex items-center gap-1.5"
            role="status"
          >
            <span class="w-2 h-2 rounded-full bg-rose-400 animate-pulse"></span>
            {{ messages.simulator.breakpointHit(breakpointHit) }}
          </div>

          <ControlBar
            :current-index="currentIndex"
            :total-frames="steps.length"
            :status="status"
            :speed="speed"
            @play="play"
            @pause="pause"
            @step-back="stepBack"
            @step-forward="stepForward"
            @reset="reset"
            @set-speed="setSpeed"
            @jump-to="jumpTo"
          />
        </div>
      </section>

      <!-- CỘT 3 (Phải): Giải thích bước chạy + Chú giải -->
      <aside
        v-show="!focusMode"
        class="simulator-col simulator-col--right"
        :class="{ 'hidden lg:flex': mobileActiveTab !== 'explain', 'flex': mobileActiveTab === 'explain' }"
      >
        <ExplainPanel
          :explanation="currentStep?.explanation ?? ''"
          :kind="currentStep?.structure?.kind"
          :frame-key="currentIndex"
        />

        <div v-if="currentStep && currentStep.annotations.length > 0" class="p-3 bg-vdsa-surface rounded-xl border border-vdsa-border text-xs text-vdsa-muted">
          <p v-for="(note, idx) in currentStep.annotations" :key="idx" class="m-0 leading-relaxed">
            · {{ note }}
          </p>
        </div>

        <!-- Accordions phụ -->
        <div class="flex flex-col gap-2">
          <div class="border border-vdsa-border rounded-xl bg-vdsa-surface overflow-hidden">
            <button
              type="button"
              class="w-full px-4 py-2.5 flex items-center justify-between text-xs font-semibold text-vdsa-secondary hover:text-white transition cursor-pointer"
              @click="theoryOpen = !theoryOpen"
            >
              <span>Giới thiệu thuật toán</span>
              <component :is="theoryOpen ? ChevronDown : ChevronRight" :size="14" />
            </button>
            <div v-if="theoryOpen" class="p-4 border-t border-vdsa-border-subtle max-h-60 overflow-y-auto">
              <ProseContent :content-html="theoryHtml" />
            </div>
          </div>

          <!-- Chỉ hiển thị Call Stack khi thuật toán thật sự có frame đệ quy -->
          <div v-if="hasCallStack" class="border border-vdsa-border rounded-xl bg-vdsa-surface overflow-hidden">
            <button
              type="button"
              class="w-full px-4 py-2.5 flex items-center justify-between text-xs font-semibold text-vdsa-secondary hover:text-white transition cursor-pointer"
              @click="showCallStack = !showCallStack"
            >
              <div class="flex items-center gap-1.5">
                <span>{{ messages.simulator.callStack }}</span>
                <span class="px-1.5 py-0.5 rounded text-[10px] bg-purple-500/20 text-purple-300 font-mono">Đệ quy</span>
              </div>
              <component :is="showCallStack ? ChevronDown : ChevronRight" :size="14" />
            </button>
            <div v-if="showCallStack" class="p-3 border-t border-vdsa-border-subtle">
              <CallStackPanel :variables="currentVariables" />
            </div>
          </div>

          <!-- Chú giải màu sắc -->
          <div class="border border-vdsa-border rounded-xl bg-vdsa-surface overflow-hidden">
            <button
              type="button"
              class="w-full px-4 py-2.5 flex items-center justify-between text-xs font-semibold text-vdsa-secondary hover:text-white transition cursor-pointer"
              @click="showLegend = !showLegend"
            >
              <span>{{ messages.simulator.legend }}</span>
              <component :is="showLegend ? ChevronDown : ChevronRight" :size="14" />
            </button>
            <div v-if="showLegend" class="p-3 border-t border-vdsa-border-subtle">
              <LegendPanel />
            </div>
          </div>
        </div>
      </aside>
    </div>

    <!-- Modal Cấu hình dữ liệu đầu vào -->
    <InputModal
      v-if="generator"
      :open="configOpen"
      :schema="generator.inputSchema"
      :current="inputConfig"
      :validate="generator?.validate"
      @close="configOpen = false"
      @apply="(input) => { configureInput(input); configOpen = false; }"
    />
  </div>
</template>

<style scoped>
.simulator-app {
  height: calc(100vh - var(--app-header-h, 68px));
  max-height: calc(100vh - var(--app-header-h, 68px));
  /* Viewport thấp (laptop + zoom 125–150%): 100vh−header không đủ chứa controls.
     min-height thắng height/max-height → trang được CUỘN thay vì cắt cụt,
     nút Chạy luôn với tới được. */
  min-height: 560px;
  background: #0B0914;
  color: #FFFFFF;
  display: flex;
  flex-direction: column;
  padding: 8px 12px;
  gap: 8px;
  overflow: hidden;
  box-sizing: border-box;
}

/* Header */
.simulator-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 4px 12px;
  background: #13111C;
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: var(--radius-lg, 12px);
  min-height: 44px;
  flex-shrink: 0;
  gap: 12px;
}

.simulator-header__left {
  display: flex;
  align-items: center;
  gap: 8px;
  min-width: 0;
  flex: 1 1 auto;
}

.simulator-header__back {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  font-size: 13px;
  color: #9CA3AF;
  text-decoration: none;
  padding: 4px 6px;
  border-radius: 6px;
  transition: color 150ms, background 150ms;
  flex-shrink: 0;
}
.simulator-header__back:hover {
  color: #FFFFFF;
  background: rgba(255, 255, 255, 0.06);
}

.simulator-header__divider {
  color: rgba(255, 255, 255, 0.2);
  font-size: 12px;
}

.simulator-header__title {
  font-size: 15px;
  font-weight: 700;
  color: #FFFFFF;
  margin: 0;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.simulator-header__badge {
  font-size: 11px;
  font-weight: 600;
  padding: 2px 8px;
  border-radius: 9999px;
  background: rgba(255, 255, 255, 0.08);
  color: #D1D5DB;
  white-space: nowrap;
  flex-shrink: 0;
}

.simulator-header__badge--accent {
  background: rgba(168, 85, 247, 0.15);
  color: #C084FC;
  border: 1px solid rgba(168, 85, 247, 0.3);
}

.simulator-header__actions {
  display: flex;
  align-items: center;
  gap: 4px;
  flex-shrink: 0;
}

/* Mobile (≤640px): hàng hành động xuống dòng riêng — 5 nút ~453px không vừa 375px,
   trước đây gây tràn ngang toàn trang. */
@media (max-width: 640px) {
  .simulator-header {
    flex-wrap: wrap;
    padding: 8px 12px;
  }
  .simulator-header__actions {
    width: 100%;
    flex-wrap: wrap;
    flex-shrink: 1;
  }
}

/* Dropdown link tài liệu */
.doc-menu {
  position: absolute;
  top: calc(100% + 6px);
  right: 0;
  z-index: 50;
  min-width: 180px;
  background: #181628;
  border: 1px solid rgba(255, 255, 255, 0.12);
  border-radius: 10px;
  padding: 6px;
  box-shadow: 0 10px 25px rgba(0, 0, 0, 0.5);
}

.doc-menu__item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 6px 10px;
  font-size: 13px;
  color: #E5E7EB;
  text-decoration: none;
  border-radius: 6px;
  transition: background 150ms;
}
.doc-menu__item:hover {
  background: rgba(168, 85, 247, 0.15);
  color: #FFFFFF;
}

/* 3 Cột Workspace */
.simulator-workspace {
  flex: 1 1 0%;
  min-height: 0;
  display: grid;
  grid-template-columns: minmax(260px, 3fr) minmax(440px, 6fr) minmax(260px, 3fr);
  gap: 12px;
  transition: grid-template-columns 200ms ease;
}

.simulator-workspace--collapsed-left {
  grid-template-columns: 56px minmax(500px, 8.5fr) minmax(260px, 3.5fr);
  gap: 12px;
  overflow: hidden;
}

/* Chế độ tập trung (Zen Mode) */
.simulator-workspace--focus {
  grid-template-columns: 1fr;
  gap: 0;
}

.simulator-col {
  min-height: 0;
  min-width: 0;
  flex-direction: column;
  gap: 8px;
}

/* display:flex chỉ áp dụng desktop (≥1024px): dưới ngưỡng này việc hiện/ẩn cột
   do Tailwind utilities ('hidden'/'flex' theo mobileActiveTab) quyết định.
   Lưu ý: scoped style KHÔNG đóng layer sẽ override @layer utilities của
   Tailwind v4, nên KHÔNG được khai báo display tường minh ở đây. */
@media (min-width: 1024px) {
  .simulator-col {
    display: flex;
  }
}

.simulator-col--left {
  overflow: hidden;
}

.simulator-col--center {
  overflow: visible;
  flex-direction: column;
  gap: 8px;
  min-height: 0;
}

.simulator-col--right {
  overflow-y: auto;
  padding-right: 4px;
}

/* Khung Canvas */
.simulator-canvas-card {
  flex: 1 1 0%;
  min-height: 140px;
  position: relative;
  background: #0D0B18;
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: var(--radius-lg, 12px);
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.simulator-controls-card {
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
  position: sticky;
  bottom: 0;
  z-index: 20;
}

/* Mobile & Tablet */
@media (max-width: 1024px) {
  .simulator-app {
    height: auto;
    min-height: calc(100vh - var(--app-header-h, 68px));
    max-height: none;
    overflow: visible;
  }
  .simulator-workspace {
    display: flex;
    flex-direction: column;
    overflow: visible;
  }
  .simulator-col--right {
    overflow-y: visible;
  }
  .simulator-canvas-card {
    min-height: 360px;
  }
}
</style>
