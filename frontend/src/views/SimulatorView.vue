<script setup lang="ts">
// SimulatorView — Màn 05: 3 vùng (mã giả 3/12 · canvas 6/12 · giải thích 3/12)
// + ControlBar + InputModal + Legend + Stats + CallStack + Tự thực hành + Mini quiz + phím tắt.
// Dùng generator THẬT từ engines/registry (task 3). Demo công khai không token (FR-7.6).
// Phase 1 view-quality: chrome = surface band level-2 (bỏ gradient-mint + blob + text-gradient),
// nút icon/toggle qua Button.vue (lucide), khung canvas = nền canvas-ink (motif tối lan tỏa §6).
// Redesign header: breadcrumb + title + chip complexity 1 hàng, description clamp-2 + "Xem thêm";
// mobile <768px: stack 1 cột (canvas → mã giả → explain), pseudocode auto-collapse.
// KHÔNG đụng CanvasArea/engine — vùng dữ liệu giữ NGUYÊN.
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
import ManualPracticePanel from '@/components/simulator/ManualPracticePanel.vue';
import MiniQuizBanner from '@/components/simulator/MiniQuizBanner.vue';
import DemoBanner from '@/components/simulator/DemoBanner.vue';
import { useSimulation } from '@/composables/useSimulation';
import { useAuthStore } from '@/stores/auth';
import { useUiStore } from '@/stores/ui';
import * as favoritesApi from '@/api/favorites';
import { getCatalogMeta } from '@/engines/catalog';
import { buildSimOverviewHtml } from '@/utils/simOverview';
import { getReference } from '@/data/referenceLinks';
import { messages } from '@/i18n/vi';
import { ChevronDown, ChevronRight, ChevronUp, ExternalLink, Share2, Star } from 'lucide-vue-next';
import Button from '@/components/ui/Button.vue';
import ProseContent from '@/components/ui/ProseContent.vue';
import Tooltip from '@/components/ui/Tooltip.vue';

const route = useRoute();
const router = useRouter();
const auth = useAuthStore();
const ui = useUiStore();

const key = computed(() => String(route.params.key ?? ''));
const isDemoKey = computed(() => getCatalogMeta(key.value)?.demoAllowed === true);
const isDemo = computed(() => !auth.isAuthenticated);

// Màn 05 — điều kiện truy cập: đã đăng nhập HOẶC key thuộc 3 demo công khai (FR-7.6)
onMounted(() => {
  if (!auth.isAuthenticated && !isDemoKey.value) {
    void router.replace({ name: 'login', query: { redirect: route.fullPath } });
    return;
  }
  window.addEventListener('keydown', onKeydown);
  checkFavorite();
  initResponsive();
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
  setSpeed,
  configureInput,
  generator,
  inputConfig,
  breakpoints,
  breakpointHit,
  toggleBreakpoint,
} = useSimulation(key.value);

const configOpen = ref(false);
const showLegend = ref(true);
const practiceMode = ref(false);
const favorite = ref(false);
const showCallStack = ref(false);
const theoryOpen = ref(false);
const pseudocodeCollapsed = ref(false);
const renderOptions = ref({ showIndex: true, showValues: true, zoom: 1 });

const practiceRef = ref<InstanceType<typeof ManualPracticePanel> | null>(null);

// ── "📖 Tài liệu" — dropdown link tham khảo (REFERENCE_LINKS theo key hiện tại) ──
const docOpen = ref(false);
const docMenuRef = ref<HTMLElement | null>(null);

/** Link tài liệu cho key hiện tại — rỗng → ẩn nút (không placeholder). */
const docLinks = computed(() => {
  const ref = getReference(key.value);
  if (!ref) return [];
  const links: { label: string; url: string }[] = [];
  if (ref.wikipedia) links.push({ label: 'Wikipedia', url: ref.wikipedia });
  if (ref.geeksforgeeks) links.push({ label: 'GeeksforGeeks', url: ref.geeksforgeeks });
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

// Mở dropdown → đăng ký đóng khi click ngoài / Escape; đóng → gỡ listener.
watch(docOpen, (open) => {
  if (open) {
    document.addEventListener('pointerdown', onDocPointerDown);
    document.addEventListener('keydown', onDocKeydown);
  } else {
    document.removeEventListener('pointerdown', onDocPointerDown);
    document.removeEventListener('keydown', onDocKeydown);
  }
});

/** Nút "Tự thực hành" chỉ disabled khi CHƯA chạy sim và CHƯA bật practice — đang practice luôn thoát được */
const practiceDisabled = computed(() => steps.value.length === 0 && !practiceMode.value);

// UX fix: bật practice → scroll panel vào tầm nhìn (sau khi v-if render xong)
watch(practiceMode, async (on) => {
  if (!on) return;
  await nextTick();
  practiceRef.value?.rootEl?.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
});

const notFound = computed(() => !currentSim.value && steps.value.length === 0 && status.value === 'idle' && !loading.value && !loadError.value);

// ── Redesign header: breadcrumb theo category + description clamp-2 ──
const categoryLabel = computed(() =>
  getCatalogMeta(key.value)?.category === 'structure' ? 'Cấu trúc dữ liệu' : 'Thuật toán',
);

/** Tooltip đầy đủ 4 mức Big-O — mono data (DESIGN.md §3). */
const complexityFull = computed(() => {
  const meta = getCatalogMeta(key.value);
  if (!meta) return '';
  const { best, average, worst, space } = meta.complexity;
  return `Tốt nhất ${best} · Trung bình ${average} · Tệ nhất ${worst} · Không gian ${space}`;
});

const descEl = ref<HTMLElement | null>(null);
const descExpanded = ref(false);
const descOverflows = ref(false);

/** Đo xem description có tràn quá 2 dòng (clamp) hay không — nếu có mới hiện nút "Xem thêm". */
function measureDesc(): void {
  const el = descEl.value;
  if (!el) return;
  descOverflows.value = el.scrollHeight > el.clientHeight + 1;
}

function toggleDesc(): void {
  descExpanded.value = !descExpanded.value;
  if (!descExpanded.value) void nextTick(measureDesc);
}

// ── Mobile <768px: pseudocode auto-collapse (chỉ 1 lần khi vào mobile, không cướp thao tác user) ──
let mobilePseudoApplied = false;
const mobileQuery = typeof window !== 'undefined' ? window.matchMedia('(max-width: 767px)') : null;

function onMobileQueryChange(event: MediaQueryListEvent | MediaQueryList): void {
  if (event.matches) {
    if (!mobilePseudoApplied) {
      pseudocodeCollapsed.value = true;
      mobilePseudoApplied = true;
    }
  } else {
    mobilePseudoApplied = false;
  }
}

function onResize(): void {
  measureDesc();
}

function initResponsive(): void {
  if (mobileQuery) {
    mobileQuery.addEventListener('change', onMobileQueryChange);
    onMobileQueryChange(mobileQuery);
  }
  window.addEventListener('resize', onResize);
  void nextTick(measureDesc);
}

onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKeydown);
  document.removeEventListener('pointerdown', onDocPointerDown);
  document.removeEventListener('keydown', onDocKeydown);
  if (mobileQuery) mobileQuery.removeEventListener('change', onMobileQueryChange);
  window.removeEventListener('resize', onResize);
});

watch(key, () => {
  reset();
  void checkFavorite();
  void nextTick(measureDesc);
});

/** Phím tắt: Space, →/←, Home/End, [ ] (FR-3.5) */
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
        reset();
        stepForward();
        while (!isLast.value) stepForward();
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

const isLast = computed(() => currentIndex.value >= steps.value.length - 1);

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
    ui.showToast('Đăng nhập để lưu yêu thích.', 'info');
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
      ui.showToast('Đã thêm vào yêu thích!', 'success');
    }
  } catch {
    ui.showToast('Không thể cập nhật yêu thích.', 'error');
  }
}

function shareLink(): void {
  const url = new URL(window.location.href);
  url.searchParams.set('sim', key.value);
  if (inputConfig.value) url.searchParams.set('input', encodeURIComponent(JSON.stringify(inputConfig.value.data)));
  void navigator.clipboard?.writeText(url.toString()).then(() => {
    ui.showToast('Đã sao chép link chia sẻ!', 'success');
  });
}

function onManualDone(result: { correct: number; wrong: number }): void {
  ui.showToast(`Kết thúc: ${result.correct} đúng / ${result.wrong} sai`, 'info');
  practiceMode.value = false;
}

const currentVariables = computed(() => currentStep.value?.variables ?? {});

/** HTML giới thiệu thuật toán/CTDL từ catalog meta — render qua ProseContent (typography chuẩn). */
const theoryHtml = computed(() => buildSimOverviewHtml(getCatalogMeta(key.value)));
</script>

<template>
  <main class="simulator container">
    <!-- Chrome header — surface band level-2 (DESIGN.md §1): không gradient, không blob -->
    <header class="simulator__chrome">
      <div class="simulator__header">
        <div class="simulator__title-block">
          <!-- 1 hàng: breadcrumb gọn (category) + title + chip complexity (DESIGN.md §4.3) -->
          <div class="simulator__title-row">
            <nav class="simulator__breadcrumb" aria-label="Breadcrumb">
              <RouterLink :to="{ name: 'simulations' }">Khám phá</RouterLink>
              <span aria-hidden="true">/</span>
              <span>{{ categoryLabel }}</span>
            </nav>
            <h1 class="simulator__title">{{ currentSim?.title ?? key }}</h1>
            <span v-if="generator" class="simulator__chip">{{ generator.dataStructure }}</span>
            <span v-if="generator" class="simulator__chip simulator__chip--complexity" :title="complexityFull">
              <span class="simulator__chip-label">Độ phức tạp TB</span>
              <span class="simulator__chip-value">{{ generator.complexity.average }}</span>
            </span>
          </div>
          <!-- Description clamp 2 dòng + "Xem thêm"/"Thu gọn" khi tràn -->
          <p
            ref="descEl"
            class="simulator__desc"
            :class="{ 'simulator__desc--clamped': !descExpanded }"
          >
            {{ messages.simulator.subtitle }}
          </p>
          <Button
            v-if="descOverflows"
            variant="ghost"
            size="sm"
            class="simulator__desc-toggle"
            :aria-expanded="descExpanded"
            @click="toggleDesc"
          >
            {{ descExpanded ? 'Thu gọn' : 'Xem thêm' }}
            <component :is="descExpanded ? ChevronUp : ChevronDown" :size="14" aria-hidden="true" />
          </Button>
        </div>
        <div class="simulator__actions">
          <Button
            variant="ghost"
            size="icon"
            :class="{ 'simulator__fav-on': favorite }"
            :aria-label="favorite ? 'Bỏ yêu thích' : 'Yêu thích'"
            :aria-pressed="favorite"
            @click="toggleFavorite"
          >
            <Star :size="16" aria-hidden="true" :fill="favorite ? 'currentColor' : 'none'" />
          </Button>
          <Button variant="ghost" size="icon" aria-label="Chia sẻ" @click="shareLink">
            <Share2 :size="16" aria-hidden="true" />
          </Button>
          <Button size="sm" variant="secondary" @click="configOpen = true">
            {{ messages.simulator.inputConfig }}
          </Button>
          <Tooltip :text="practiceDisabled ? 'Chạy mô phỏng trước để bật Tự thực hành' : ''">
            <Button
              size="sm"
              variant="ghost"
              :disabled="practiceDisabled"
              @click="practiceMode = !practiceMode"
            >
              {{ practiceMode ? 'Thoát tự thực hành' : 'Tự thực hành' }}
            </Button>
          </Tooltip>
          <div v-if="docLinks.length > 0" ref="docMenuRef" class="simulator__doc">
            <Button
              size="sm"
              variant="ghost"
              :aria-expanded="docOpen"
              aria-controls="simulator-doc-menu"
              @click="docOpen = !docOpen"
            >
              📖 Tài liệu
            </Button>
            <div
              v-if="docOpen"
              id="simulator-doc-menu"
              class="simulator__doc-menu"
              :aria-label="`Tài liệu tham khảo — ${currentSim?.title ?? key}`"
            >
              <p class="simulator__doc-title">📖 Đọc thêm</p>
              <a
                v-for="link in docLinks"
                :key="link.url"
                :href="link.url"
                target="_blank"
                rel="noopener noreferrer"
                class="simulator__doc-link"
                @click="docOpen = false"
              >
                <span>{{ link.label }}</span>
                <ExternalLink :size="14" aria-hidden="true" />
              </a>
            </div>
          </div>
        </div>
      </div>
    </header>

    <DemoBanner v-if="isDemo" :sim-key="key" />

    <div v-if="loading" class="simulator__loading simulator__panel" role="status">
      <p>{{ messages.common.loading }} Đang dựng mô phỏng...</p>
    </div>

    <div v-else-if="loadError" class="simulator__error simulator__panel" role="alert">
      <p>{{ loadError }}</p>
      <Button size="sm" variant="secondary" @click="router.push({ name: 'simulations' })">
        Về danh mục
      </Button>
    </div>

    <div v-else-if="notFound" class="simulator__empty simulator__panel" role="status">
      <p>{{ messages.simulator.notFound }} ({{ key }})</p>
      <Button size="sm" variant="secondary" @click="router.push({ name: 'simulations' })">
        Về danh mục
      </Button>
    </div>

    <template v-else>
      <div class="simulator__grid">
        <!-- Trái: mã giả (mobile: auto-collapse + xếp sau canvas) -->
        <PseudocodePanel
          class="simulator__pseudo"
          :pseudocode="generator?.pseudocode ?? []"
          :active-line="currentStep?.pseudocodeLine ?? 0"
          :variables="currentVariables"
          :collapsed="pseudocodeCollapsed"
          :breakpoints="breakpoints"
          @update:collapsed="pseudocodeCollapsed = $event"
          @toggle-breakpoint="toggleBreakpoint"
        />

        <!-- Giữa: canvas + điều khiển -->
        <div class="simulator__center">
          <div class="simulator__canvas-wrap">
            <CanvasArea
              :structure="currentStep?.structure ?? null"
              v-model:show-index="renderOptions.showIndex"
              v-model:show-values="renderOptions.showValues"
              v-model:zoom="renderOptions.zoom"
              :empty-text="messages.simulator.canvasPlaceholder"
            />
            <div class="simulator__canvas-meta">
              <span class="simulator__canvas-dot" aria-hidden="true" />
              <span class="simulator__canvas-label">Khu vực vẽ — renderer cấu trúc dữ liệu</span>
            </div>
          </div>
          <StatsBar
            :comparisons="currentStep?.stats.comparisons ?? 0"
            :swaps="currentStep?.stats.swaps ?? 0"
            :writes="currentStep?.stats.writes ?? 0"
            :step="currentIndex"
            :total-steps="steps.length"
          />
          <div
            v-if="breakpointHit !== null && status === 'paused'"
            class="simulator__bp-badge"
            role="status"
            data-testid="breakpoint-badge"
          >
            <span class="simulator__bp-dot" aria-hidden="true" />
            Đã dừng tại breakpoint dòng {{ breakpointHit }}
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
          />
          <LegendPanel :collapsed="!showLegend" />
          <ManualPracticePanel
            v-if="practiceMode"
            ref="practiceRef"
            :steps="steps"
            :current-index="currentIndex"
            @skip="stepForward"
            @done="onManualDone"
          />
          <MiniQuizBanner
            v-if="status === 'finished'"
            :steps="steps"
            :sim-key="key"
            @restart="reset"
            @view-theory="router.push({ name: 'path' })"
          />
        </div>

        <!-- Phải: giải thích -->
        <div class="simulator__right">
          <ExplainPanel
            :explanation="currentStep?.explanation ?? ''"
            :kind="undefined"
            :frame-key="currentIndex"
          />
          <div v-if="currentStep && currentStep.annotations.length > 0" class="simulator__annotations simulator__panel">
            <p v-for="(note, idx) in currentStep.annotations" :key="idx" class="simulator__annotation">
              · {{ note }}
            </p>
          </div>
          <div class="simulator__panel-actions">
            <Button variant="ghost" size="sm" @click="theoryOpen = !theoryOpen">
              <ChevronDown v-if="theoryOpen" :size="16" aria-hidden="true" />
              <ChevronRight v-else :size="16" aria-hidden="true" />
              Giới thiệu
            </Button>
            <Button variant="ghost" size="sm" @click="showCallStack = !showCallStack">
              <ChevronDown v-if="showCallStack" :size="16" aria-hidden="true" />
              <ChevronRight v-else :size="16" aria-hidden="true" />
              Call stack
            </Button>
            <Button variant="ghost" size="sm" @click="showLegend = !showLegend">
              <ChevronDown v-if="showLegend" :size="16" aria-hidden="true" />
              <ChevronRight v-else :size="16" aria-hidden="true" />
              Legend
            </Button>
          </div>
          <div v-if="theoryOpen" class="simulator__theory simulator__panel">
            <ProseContent :content-html="theoryHtml" />
          </div>
          <CallStackPanel v-if="showCallStack" :variables="currentVariables" />
        </div>
      </div>
    </template>

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

    <footer class="simulator__footer">
      Phím tắt: Space = Phát/Dừng · ←/→ = Bước · Home/End = Về đầu/cuối · [ / ] = Tốc độ
    </footer>
  </main>
</template>

<style scoped>
.simulator {
  display: flex;
  flex-direction: column;
  gap: var(--space-lg);
  padding-block: var(--space-md) var(--space-2xl);
}

/* ── Chrome header — surface band level-2 (DESIGN.md §1 + §6) ── */
.simulator__chrome {
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  background: var(--color-card-raised);
  padding: var(--space-lg) var(--space-xl);
}

.simulator__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: var(--space-md);
  flex-wrap: wrap;
}

.simulator__title-block {
  flex: 1 1 auto;
  min-width: 0;
}

/* 1 hàng: breadcrumb + title + chips — baseline căn chỉnh (DESIGN.md §3) */
.simulator__title-row {
  display: flex;
  align-items: baseline;
  flex-wrap: wrap;
  row-gap: var(--space-xs);
  column-gap: var(--space-md);
}

.simulator__breadcrumb {
  display: inline-flex;
  align-items: baseline;
  gap: var(--space-sm);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.04em;
  color: var(--color-text-tertiary);
}

.simulator__breadcrumb a { color: var(--color-primary); font-weight: 600; text-decoration: none; }

.simulator__title {
  font-size: var(--text-3xl);
  font-weight: 600;
  letter-spacing: -0.025em;
  line-height: 1.15;
  color: var(--color-foreground);
  margin: 0;
}

/* Chip meta — badge chuẩn DESIGN.md §4.3 (text-xs, min-h 24px, radius-md) */
.simulator__chip {
  display: inline-flex;
  align-items: center;
  gap: var(--space-xs);
  min-height: 24px;
  padding: 2px 10px;
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  font-size: var(--text-xs);
  color: var(--color-text-secondary);
  white-space: nowrap;
}

.simulator__chip--complexity { border-color: var(--color-border-strong); }

.simulator__chip-label { color: var(--color-text-tertiary); }

.simulator__chip-value {
  font-family: var(--font-mono);
  font-weight: 600;
  color: var(--color-foreground);
}

/* Description — clamp 2 dòng, nút "Xem thêm" chỉ hiện khi tràn */
.simulator__desc {
  margin: var(--space-xs) 0 0;
  font-size: var(--text-sm);
  line-height: 1.55;
  color: var(--color-text-secondary);
  max-width: 64ch;
}

.simulator__desc--clamped {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.simulator__desc-toggle { margin-top: var(--space-xs); }

.simulator__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; align-items: center; }

.simulator__fav-on { color: var(--color-warning); }

/* "📖 Tài liệu" — dropdown link tham khảo (popover level-3 §6: shadow hợp lệ) */
.simulator__doc { position: relative; }

.simulator__doc-menu {
  position: absolute;
  top: calc(100% + var(--space-sm));
  right: 0;
  z-index: var(--z-raised);
  min-width: 200px;
  max-width: min(280px, calc(100vw - var(--space-xl)));
  display: flex;
  flex-direction: column;
  gap: var(--space-xs);
  padding: var(--space-sm);
  background: var(--color-card-raised);
  border: 1px solid var(--color-border-subtle);
  border-radius: var(--radius-lg);
  box-shadow: var(--shadow-md);
}

.simulator__doc-title {
  margin: 0;
  padding: var(--space-xs) var(--space-sm);
  font-size: var(--text-xs);
  color: var(--color-text-tertiary);
}

.simulator__doc-link {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm);
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-md);
  font-size: var(--text-sm);
  font-weight: 500;
  color: var(--color-primary);
  text-decoration: none;
  transition: background-color 150ms cubic-bezier(0.16, 1, 0.3, 1);
}

.simulator__doc-link:hover { background: var(--color-muted); }

.simulator__doc-link:focus-visible {
  outline: 2px solid var(--color-ring);
  outline-offset: -2px;
}

/* Grid — minmax(0, …) chống tràn ngang (grid blowout) */
.simulator__grid {
  display: grid;
  grid-template-columns: minmax(0, 3fr) minmax(0, 6fr) minmax(0, 3fr);
  gap: var(--space-md);
  align-items: start;
}

.simulator__pseudo { min-width: 0; }

.simulator__center { display: flex; flex-direction: column; gap: var(--space-sm); min-width: 0; }

/* Badge breakpoint hit — trạng thái dừng tại breakpoint (mono, không shadow) */
.simulator__bp-badge {
  display: inline-flex;
  align-items: center;
  gap: var(--space-sm);
  align-self: flex-start;
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  font-weight: 500;
  color: var(--color-on-primary);
  background: var(--color-destructive);
  padding: var(--space-xs) var(--space-sm);
  border-radius: var(--radius-full);
}

.simulator__bp-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--color-destructive-foreground);
}

/* Khung vẽ — NGUYÊN CanvasArea bên trong; khung ngoài = nền canvas-ink (motif tối §6) */
.simulator__canvas-wrap {
  position: relative;
  min-width: 0;
  border: 1px solid color-mix(in srgb, var(--color-index-muted) 45%, transparent);
  border-radius: var(--radius-lg);
  background: var(--color-canvas-ink);
  padding: var(--space-sm);
}

.simulator__canvas-meta {
  display: flex;
  align-items: center;
  gap: var(--space-sm);
  padding: var(--space-sm);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-index-muted);
}

.simulator__canvas-dot {
  width: 8px;
  height: 8px;
  border-radius: 50%;
  background: var(--color-data-core);
  box-shadow: 0 0 0 3px color-mix(in srgb, var(--color-data-core) 22%, transparent);
}

.simulator__right { display: flex; flex-direction: column; gap: var(--space-sm); min-width: 0; }

.simulator__panel {
  background: var(--color-card);
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  padding: var(--space-md);
}

.simulator__annotations { padding: var(--space-sm) var(--space-md); }

.simulator__annotation { font-size: var(--text-xs); color: var(--color-text-muted); margin: 0; }

.simulator__panel-actions { display: flex; gap: var(--space-sm); }

.simulator__loading,
.simulator__empty,
.simulator__error {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: var(--space-md);
  min-height: 240px;
  color: var(--color-text-muted);
  text-align: center;
}

.simulator__footer {
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  text-align: center;
}

@media (max-width: 1024px) {
  .simulator__grid { grid-template-columns: minmax(0, 1fr); }
  .simulator__right { order: 3; }
}

/* Mobile <768px (DESIGN.md §8): stack 1 cột — canvas trước, mã giả auto-collapse, explain cuối */
@media (max-width: 767px) {
  .simulator { gap: var(--space-md); padding-block: var(--space-sm) var(--space-xl); }
  .simulator__chrome { padding: var(--space-md); }
  .simulator__title { font-size: var(--text-2xl); }
  .simulator__title-row { column-gap: var(--space-sm); }
  .simulator__grid { gap: var(--space-sm); }
  .simulator__center { order: 1; }
  .simulator__pseudo { order: 2; }
  .simulator__right { order: 3; }
  .simulator__actions { width: 100%; }
}
</style>
