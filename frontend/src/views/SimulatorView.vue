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
import { buildSimOverviewHtml } from '@/utils/simOverview';
import { getReference } from '@/data/referenceLinks';
import { messages } from '@/i18n/vi';
import { ArrowLeft, ChevronDown, ChevronRight, ExternalLink, Settings2, Share2, Star } from 'lucide-vue-next';
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
  if (!auth.isAuthenticated && !isDemoKey.value) {
    void router.replace({ name: 'login', query: { redirect: route.fullPath } });
    return;
  }
  window.addEventListener('keydown', onKeydown);
  checkFavorite();

  if (route.query.input) {
    try {
      const rawInput = route.query.input as string;
      const parsed = JSON.parse(decodeURIComponent(rawInput));
      if (parsed !== null && parsed !== undefined) {
        if (parsed && typeof parsed === 'object' && 'data' in parsed) {
          void configureInput(parsed as any);
        } else {
          void configureInput({ data: parsed } as any);
        }
      }
    } catch (e) {
      console.warn('Failed to parse input from query string', e);
    }
  }
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
} = useSimulation(key.value);

const configOpen = ref(false);
const showLegend = ref(false);
const favorite = ref(false);
const showCallStack = ref(false);
const theoryOpen = ref(false);
const pseudocodeCollapsed = ref(false);
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
  return messages.simulator.complexityFull(best, average, worst, space);
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
  if (target.tagName === 'INPUT' || target.tagName === 'TEXTAREA' || target.tagName === 'SELECT') return;
  if (configOpen.value) return;
  switch (event.key) {
    case ' ':
      event.preventDefault();
      status.value === 'running' ? pause() : play();
      break;
    case 'ArrowRight':
      stepForward();
      break;
    case 'ArrowLeft':
      stepBack();
      break;
    case 'Home':
      reset();
      break;
    case 'End':
      if (steps.value.length > 0) {
        jumpTo(steps.value.length - 1);
      }
      break;
    case '[':
      setSpeed(Math.max(0.25, speed.value / 2));
      break;
    case ']':
      setSpeed(Math.min(4, speed.value * 2));
      break;
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
  const url = new URL(window.location.href);
  url.searchParams.set('sim', key.value);
  if (inputConfig.value) url.searchParams.set('input', encodeURIComponent(JSON.stringify(inputConfig.value.data)));
  void navigator.clipboard?.writeText(url.toString()).then(() => {
    ui.showToast(messages.simulator.toastCopied, 'success');
  });
}

const currentVariables = computed(() => currentStep.value?.variables ?? {});
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

        <div v-if="docLinks.length > 0" ref="docMenuRef" class="relative">
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

    <!-- 3 Cột Workspace: Mã giả · Canvas + Điều khiển · Giải thích -->
    <div v-else class="simulator-workspace">
      <!-- CỘT 1 (Trái): Mã giả (Pseudocode) & Biến -->
      <aside class="simulator-col simulator-col--left">
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
      <section class="simulator-col simulator-col--center">
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
      <aside class="simulator-col simulator-col--right">
        <ExplainPanel
          :explanation="currentStep?.explanation ?? ''"
          :kind="undefined"
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
              class="w-full px-4 py-2.5 flex items-center justify-between text-xs font-semibold text-vdsa-secondary hover:text-white transition"
              @click="theoryOpen = !theoryOpen"
            >
              <span>Giới thiệu thuật toán</span>
              <component :is="theoryOpen ? ChevronDown : ChevronRight" :size="14" />
            </button>
            <div v-if="theoryOpen" class="p-4 border-t border-vdsa-border-subtle max-h-60 overflow-y-auto">
              <ProseContent :content-html="theoryHtml" />
            </div>
          </div>

          <div class="border border-vdsa-border rounded-xl bg-vdsa-surface overflow-hidden">
            <button
              type="button"
              class="w-full px-4 py-2.5 flex items-center justify-between text-xs font-semibold text-vdsa-secondary hover:text-white transition"
              @click="showCallStack = !showCallStack"
            >
              <span>{{ messages.simulator.callStack }}</span>
              <component :is="showCallStack ? ChevronDown : ChevronRight" :size="14" />
            </button>
            <div v-if="showCallStack" class="p-3 border-t border-vdsa-border-subtle">
              <CallStackPanel :variables="currentVariables" />
            </div>
          </div>

          <div class="border border-vdsa-border rounded-xl bg-vdsa-surface overflow-hidden">
            <button
              type="button"
              class="w-full px-4 py-2.5 flex items-center justify-between text-xs font-semibold text-vdsa-secondary hover:text-white transition"
              @click="showLegend = !showLegend"
            >
              <span>{{ messages.simulator.legend }}</span>
              <component :is="showLegend ? ChevronDown : ChevronRight" :size="14" />
            </button>
            <div v-if="showLegend" class="p-3 border-t border-vdsa-border-subtle">
              <LegendPanel :collapsed="false" />
            </div>
          </div>
        </div>

        <div class="text-[11px] text-vdsa-muted text-center pt-2 font-mono">
          Phím tắt: Space (Phát/Dừng) · ←/→ (Tua bước) · Home/End · [ / ] (Tốc độ)
        </div>
      </aside>
    </div>

    <!-- Modal Cấu hình đầu vào -->
    <InputModal
      :open="configOpen"
      :schema="generator?.inputSchema ?? null"
      :current="inputConfig"
      :validate="generator?.validate"
      :loading="loading"
      @close="configOpen = false"
      @apply="
        (input) => {
          configOpen = false;
          void configureInput(input);
        }
      "
    />
  </div>
</template>

<style scoped>
.simulator-app {
  display: flex;
  flex-direction: column;
  height: calc(100vh - var(--app-header-h, 112px));
  max-height: calc(100vh - var(--app-header-h, 112px));
  overflow: hidden;
  background: var(--color-bg, #0B0A12);
  padding: 8px 16px 12px;
  gap: 8px;
}

/* Header thanh gọn */
.simulator-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 8px 16px;
  background: var(--color-card-raised, rgba(22, 20, 36, 0.7));
  border: 1px solid var(--color-border-subtle, rgba(255, 255, 255, 0.08));
  border-radius: var(--radius-lg, 12px);
  backdrop-filter: blur(12px);
  flex-shrink: 0;
}

.simulator-header__left {
  display: flex;
  align-items: center;
  gap: 10px;
  flex-wrap: wrap;
  min-width: 0;
}

.simulator-header__back {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  font-size: 13px;
  font-weight: 600;
  color: var(--color-primary, #A855F7);
  text-decoration: none;
  transition: opacity 150ms ease;
}
.simulator-header__back:hover {
  opacity: 0.85;
}

.simulator-header__divider {
  color: var(--color-text-tertiary, #6B7280);
  font-size: 12px;
}

.simulator-header__title {
  font-size: 18px;
  font-weight: 700;
  color: #FFFFFF;
  margin: 0;
  white-space: nowrap;
}

.simulator-header__badge {
  font-size: 11px;
  font-weight: 500;
  padding: 2px 8px;
  border-radius: 6px;
  background: rgba(255, 255, 255, 0.06);
  border: 1px solid rgba(255, 255, 255, 0.1);
  color: #D1D5DB;
  white-space: nowrap;
}

.simulator-header__badge--accent {
  background: rgba(168, 85, 247, 0.15);
  border-color: rgba(168, 85, 247, 0.3);
  color: #D8B4FE;
  font-family: var(--font-mono, monospace);
}

.simulator-header__actions {
  display: flex;
  align-items: center;
  gap: 6px;
  flex-shrink: 0;
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
  grid-template-columns: 3fr 6fr 3fr;
  gap: 12px;
  overflow: hidden;
}

.simulator-col {
  min-height: 0;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.simulator-col--left {
  overflow: hidden;
}

.simulator-col--center {
  overflow: hidden;
}

.simulator-col--right {
  overflow-y: auto;
  padding-right: 4px;
}

/* Khung Canvas */
.simulator-canvas-card {
  flex: 1 1 0%;
  min-height: 0;
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
}

/* Mobile & Tablet */
@media (max-width: 1024px) {
  .simulator-app {
    height: auto;
    max-height: none;
    overflow: visible;
  }
  .simulator-workspace {
    grid-template-columns: 1fr;
    overflow: visible;
  }
  .simulator-col--right {
    overflow-y: visible;
  }
  .simulator-canvas-card {
    min-height: 380px;
  }
}
</style>
