<script setup lang="ts">
// SimulatorView — Màn 05: 3 vùng (mã giả 3/12 · canvas 6/12 · giải thích 3/12)
// + ControlBar + InputModal + Legend + Stats + CallStack + Tự thực hành + Mini quiz + phím tắt.
// Dùng generator THẬT từ engines/registry (task 3). Demo công khai không token (FR-7.6).
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
import { messages } from '@/i18n/vi';
import Button from '@/components/ui/Button.vue';
import BaseIcon from '@/components/ui/BaseIcon.vue';

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
} = useSimulation(key.value);

const configOpen = ref(false);
const showLegend = ref(true);
const practiceMode = ref(false);
const favorite = ref(false);
const showCallStack = ref(false);
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
</script>

<template>
  <main class="simulator container">
    <header class="simulator__header">
      <div class="simulator__title-block">
        <nav class="simulator__breadcrumb" aria-label="Breadcrumb">
          <RouterLink :to="{ name: 'simulations' }">Khám phá</RouterLink>
          <span aria-hidden="true">/</span>
          <span>{{ currentSim?.title ?? key }}</span>
        </nav>
        <h1 class="simulator__title">{{ currentSim?.title ?? key }}</h1>
      </div>
      <div class="simulator__actions">
        <button
          type="button"
          class="simulator__icon-btn"
          :class="{ 'simulator__icon-btn--on': favorite }"
          :aria-label="favorite ? 'Bỏ yêu thích' : 'Yêu thích'"
          @click="toggleFavorite"
        >
          <BaseIcon name="star" :size="18" />
        </button>
        <button type="button" class="simulator__icon-btn" aria-label="Chia sẻ" @click="shareLink">
          <BaseIcon name="share" :size="18" />
        </button>
        <Button size="sm" variant="secondary" @click="configOpen = true">
          ⚙ {{ messages.simulator.inputConfig }}
        </Button>
        <Button size="sm" variant="ghost" @click="practiceMode = !practiceMode">
          {{ practiceMode ? 'Thoát tự thực hành' : 'Tự thực hành' }}
        </Button>
      </div>
    </header>

    <DemoBanner v-if="isDemo" :sim-key="key" />

    <div v-if="loading" class="simulator__loading" role="status">
      <p>{{ messages.common.loading }} Đang dựng mô phỏng...</p>
    </div>

    <div v-else-if="loadError" class="simulator__error card" role="alert">
      <p>{{ loadError }}</p>
      <Button size="sm" variant="secondary" @click="router.push({ name: 'simulations' })">
        Về danh mục
      </Button>
    </div>

    <div v-else-if="notFound" class="simulator__empty card" role="status">
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
          @update:collapsed="pseudocodeCollapsed = $event"
        />

        <!-- Giữa: canvas + điều khiển -->
        <div class="simulator__center">
          <CanvasArea
            :structure="currentStep?.structure ?? null"
            v-model:show-index="renderOptions.showIndex"
            v-model:show-values="renderOptions.showValues"
            v-model:zoom="renderOptions.zoom"
            :empty-text="messages.simulator.canvasPlaceholder"
          />
          <StatsBar
            :comparisons="currentStep?.stats.comparisons ?? 0"
            :swaps="currentStep?.stats.swaps ?? 0"
            :writes="currentStep?.stats.writes ?? 0"
            :step="currentIndex"
            :total-steps="steps.length"
          />
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
          <div v-if="currentStep && currentStep.annotations.length > 0" class="simulator__annotations card">
            <p v-for="(note, idx) in currentStep.annotations" :key="idx" class="simulator__annotation">
              · {{ note }}
            </p>
          </div>
          <div class="simulator__panel-actions">
            <button type="button" class="simulator__toggle" @click="showCallStack = !showCallStack">
              Call stack {{ showCallStack ? '▾' : '▸' }}
            </button>
            <button type="button" class="simulator__toggle" @click="showLegend = !showLegend">
              Legend {{ showLegend ? '▾' : '▸' }}
            </button>
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

    <footer class="simulator__footer text-muted">
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
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  margin-bottom: 4px;
}

.simulator__title { font-size: var(--text-xl); color: var(--color-foreground); }

.simulator__actions { display: flex; gap: var(--space-sm); flex-wrap: wrap; align-items: center; }

.simulator__icon-btn {
  width: 36px;
  height: 36px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-md);
  background: var(--color-surface);
  color: var(--color-text-muted);
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.simulator__icon-btn--on { color: var(--color-warning); border-color: var(--color-warning); }

.simulator__grid {
  display: grid;
  grid-template-columns: 3fr 6fr 3fr;
  gap: var(--space-md);
  align-items: start;
}

.simulator__center { display: flex; flex-direction: column; gap: var(--space-sm); }

.simulator__right { display: flex; flex-direction: column; gap: var(--space-sm); }

.simulator__annotations { padding: var(--space-sm) var(--space-md); }

.simulator__annotation { font-size: var(--text-xs); color: var(--color-text-muted); }

.simulator__panel-actions { display: flex; gap: var(--space-sm); }

.simulator__toggle {
  background: none;
  border: none;
  color: var(--color-primary);
  font-size: var(--text-xs);
  font-weight: 700;
  cursor: pointer;
  padding: 4px 8px;
  border-radius: var(--radius-sm);
}

.simulator__toggle:hover { background: var(--color-surface-hover); }

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

.simulator__footer { font-size: var(--text-xs); text-align: center; }

@media (max-width: 1024px) {
  .simulator__grid { grid-template-columns: 1fr; }
  .simulator__right { order: 3; }
}
</style>
