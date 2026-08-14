<script setup lang="ts">
// SimulatorView — Màn 05: 3 vùng (mã giả 3/12 · canvas 6/12 · giải thích 3/12)
// + ControlBar + InputModal + Legend + Stats + CallStack + Tự thực hành + Mini quiz + phím tắt.
// Dùng generator THẬT từ engines/registry (task 3). Demo công khai không token (FR-7.6).
// Phase 1 view-quality: chrome = surface band level-2 (bỏ gradient-mint + blob + text-gradient),
// nút icon/toggle qua Button.vue (lucide), khung canvas = nền canvas-ink (motif tối lan tỏa §6).
// KHÔNG đụng CanvasArea/engine — vùng dữ liệu giữ NGUYÊN.
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue';
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
import { messages } from '@/i18n/vi';
import { ChevronDown, ChevronRight, Share2, Star } from 'lucide-vue-next';
import Button from '@/components/ui/Button.vue';
import ProseContent from '@/components/ui/ProseContent.vue';

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

const notFound = computed(() => !currentSim.value && steps.value.length === 0 && status.value === 'idle' && !loading.value && !loadError.value);

onBeforeUnmount(() => {
  window.removeEventListener('keydown', onKeydown);
});

watch(key, () => {
  reset();
  void checkFavorite();
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
          <nav class="simulator__breadcrumb" aria-label="Breadcrumb">
            <RouterLink :to="{ name: 'simulations' }">Khám phá</RouterLink>
            <span aria-hidden="true">/</span>
            <span>{{ currentSim?.title ?? key }}</span>
          </nav>
          <h1 class="simulator__title">{{ currentSim?.title ?? key }}</h1>
          <p class="simulator__subtitle">
            <template v-if="generator">
              {{ generator.dataStructure }} · Độ phức tạp TB
              <span class="simulator__complexity">{{ generator.complexity.average }}</span>
            </template>
            <template v-else>{{ messages.simulator.subtitle }}</template>
          </p>
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
          <Button size="sm" variant="ghost" @click="practiceMode = !practiceMode">
            {{ practiceMode ? 'Thoát tự thực hành' : 'Tự thực hành' }}
          </Button>
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
        <!-- Trái: mã giả -->
        <PseudocodePanel
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
  gap: var(--space-md);
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

.simulator__breadcrumb {
  display: flex;
  gap: var(--space-sm);
  font-family: var(--font-mono);
  font-size: var(--text-xs);
  letter-spacing: 0.04em;
  color: var(--color-text-muted);
  margin-bottom: var(--space-xs);
}

.simulator__breadcrumb a { color: var(--color-primary); font-weight: 600; text-decoration: none; }

.simulator__title {
  font-size: var(--text-3xl);
  font-weight: 600;
  letter-spacing: -0.02em;
  color: var(--color-foreground);
  margin: 0;
}

.simulator__subtitle {
  font-size: var(--text-sm);
  color: var(--color-text-muted);
  margin-top: var(--space-xs);
  max-width: 56ch;
}

.simulator__complexity {
  font-family: var(--font-mono);
  font-weight: 600;
  color: var(--color-foreground);
}

.simulator__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; align-items: center; }

.simulator__fav-on { color: var(--color-warning); }

.simulator__grid {
  display: grid;
  grid-template-columns: 3fr 6fr 3fr;
  gap: var(--space-md);
  align-items: start;
}

.simulator__center { display: flex; flex-direction: column; gap: var(--space-sm); }

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

.simulator__right { display: flex; flex-direction: column; gap: var(--space-sm); }

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
  .simulator__grid { grid-template-columns: 1fr; }
  .simulator__right { order: 3; }
}
</style>
