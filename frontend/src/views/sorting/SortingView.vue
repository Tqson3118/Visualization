<template>
  <div class="sorting-view-root flex flex-col h-full w-full p-1.5 max-w-[1920px] mx-auto overflow-hidden relative font-sans">
    <div class="top-control-bar flex items-center justify-between px-3 py-1 bg-bg-surface border border-border-default rounded-lg backdrop-blur-xl shrink-0 shadow-md z-20 mb-1">
      <div class="flex items-center gap-1.5 relative" ref="tabBarRef">
        <div
          class="tab-indicator"
          :class="`tab-indicator--${activeTab}`"
          :style="indicatorStyle"
        ></div>
        <button
          v-for="tab in tabs"
          :key="tab.id"
          :ref="(el) => setTabEl(tab.id, el)"
          class="sub-tab-pill flex items-center gap-1.5 px-2.5 py-1 rounded-md text-xs font-bold transition-all duration-200 cursor-pointer"
          :class="activeTab === tab.id
            ? 'text-white'
            : 'text-text-secondary hover:text-text-primary hover:bg-bg-hover'"
          @click="activeTab = tab.id"
        >
          <BaseIcon :name="tab.icon" class="w-3.5 h-3.5" />
          <span>{{ tab.name }}</span>
        </button>
      </div>

      <div class="flex items-center gap-3 font-mono text-[10px] uppercase tracking-wider text-text-secondary select-none">
        <span class="flex items-center gap-1.5 text-accent font-bold">
          <span class="w-2 h-2 rounded-full bg-accent animate-pulse"></span>
          VISUALGO-MODE 60FPS
        </span>
        <span class="text-text-muted text-[9px]">Space: Play/Pause | <BaseIcon name="arrow-left" class="w-2.5 h-2.5 inline align-middle" /> <BaseIcon name="arrow-right" class="w-2.5 h-2.5 inline align-middle" />: Step</span>
        <button
          class="w-5 h-5 flex items-center justify-center rounded text-text-secondary hover:text-accent hover:bg-bg-hover transition-all cursor-pointer"
          title="Xem lại hướng dẫn"
          @click="tourStore.startPageTour('/sorting', true)"
        >?</button>
      </div>
    </div>

    <div class="flex-1 min-h-0 relative w-full h-full overflow-hidden rounded-lg border border-border-default bg-bg-primary">
      <Transition name="sandbox-tab" mode="out-in">
        <KeepAlive>
          <component
            :is="activeComponent"
            :key="activeTab"
            class="absolute inset-0 w-full h-full"
            v-bind="activeProps"
            :data-tour-id="activeTab === 'sorting' ? 'algo-theory-pane' : undefined"
          />
        </KeepAlive>
      </Transition>

      <template v-if="activeTab === 'sorting'">
        <div class="absolute bottom-3 left-0 right-0 z-30 px-4 flex items-center justify-center pointer-events-none gap-2">
          <div class="pointer-events-auto flex-1 min-w-0 flex justify-center max-w-2xl">
            <VcrDockBar />
          </div>

          <div class="pointer-events-auto shrink-0">
            <SortingDrawerTrace />
          </div>
        </div>
      </template>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, defineComponent, h, watch, onMounted, onUnmounted, nextTick } from 'vue';
import { useRoute } from 'vue-router';
import { ArrayBarVisualizer } from '../../features/algorithm-sandbox';
import SortingDrawerTrace from '../../features/algorithm-sandbox/components/SortingDrawerTrace.vue';
import { VcrDockBar } from '../../features/vcr-player';
import { useVcrStore } from '../../features/vcr-player/store/useVcrStore';
import BaseIcon from '../../shared/components/BaseIcon.vue';
import { useGuidedTourStore } from '../../features/guided-tour/store/useGuidedTourStore';
import SearchingView from '../searching/SearchingView.vue';
import GraphView from '../graph/GraphView.vue';
import StackQueueView from '../stackqueue/StackQueueView.vue';

// Tab mặc định theo route đang vào: /sorting-sandbox → sorting, /searching-sandbox → searching,
// /graph-playground → graph, /stack-queue-sandbox → stack-queue. Đổi route (dropdown nav)
// sẽ remount (App key theo fullPath) → mở đúng tab.
function initialTabFromRoute(): string {
  const name = useRoute().name;
  if (name === 'searching-sandbox') return 'searching';
  if (name === 'graph-playground') return 'graph';
  if (name === 'stack-queue-sandbox') return 'stack-queue';
  return 'sorting';
}

const activeTab = ref(initialTabFromRoute());
const tourStore = useGuidedTourStore();
const vcrStore = useVcrStore();

// ── Sliding tab indicator: khối nền trượt ngang theo tab active + đổi màu ──
const tabBarRef = ref<HTMLElement | null>(null);
const tabEls = new Map<string, HTMLElement>();
const indicatorX = ref(0);
const indicatorW = ref(0);

function setTabEl(id: string, el: unknown): void {
  if (el instanceof HTMLElement) tabEls.set(id, el);
}

function updateIndicator(): void {
  const el = tabEls.get(activeTab.value);
  const bar = tabBarRef.value;
  if (!el || !bar) return;
  indicatorX.value = el.offsetLeft;
  indicatorW.value = el.offsetWidth;
}

const indicatorStyle = computed(() => ({
  transform: `translateX(${indicatorX.value}px)`,
  width: `${indicatorW.value}px`,
}));

// Đo lại khi tab đổi (nextTick để DOM render nút mới/đủ) + khi resize
watch(activeTab, () => {
  void nextTick(updateIndicator);
});

onMounted(() => {
  window.addEventListener('keydown', handleKeydown);
  tourStore.startPageTour('/sorting');
  window.addEventListener('resize', updateIndicator);
  void nextTick(updateIndicator);
});

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeydown);
  window.removeEventListener('resize', updateIndicator);
  // Dọn dẹp state của store dùng chung khi rời route:
  // dừng timer VCR + xóa customCompileFn để không "hijack" compile của feature khác
  vcrStore.pause();
  vcrStore.customCompileFn = null;
});

function handleKeydown(e: KeyboardEvent) {
  const tag = (e.target as HTMLElement)?.tagName;
  if (tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT' || tag === 'BUTTON') return;

  if (activeTab.value !== 'sorting') return;

  switch (e.key.toLowerCase()) {
    case ' ':
      e.preventDefault();
      vcrStore.togglePlay();
      break;
    case 'arrowright':
      e.preventDefault();
      vcrStore.stepNext();
      break;
    case 'arrowleft':
      e.preventDefault();
      vcrStore.stepPrev();
      break;
    case 'r':
      e.preventDefault();
      vcrStore.reset();
      break;
  }
}

onMounted(() => {
  window.addEventListener('keydown', handleKeydown);
  tourStore.startPageTour('/sorting');
});

onUnmounted(() => {
  window.removeEventListener('keydown', handleKeydown);
  // Dọn dẹp state của store dùng chung khi rời route:
  // dừng timer VCR + xóa customCompileFn để không "hijack" compile của feature khác
  vcrStore.pause();
  vcrStore.customCompileFn = null;
});

// Dừng phát khi rời tab sorting (KeepAlive giữ component sống, timer VCR vẫn chạy nền)
watch(activeTab, (tab) => {
  if (tab !== 'sorting') vcrStore.pause();
});

const tabs = [
  { id: 'sorting', name: 'Sorting Sandbox', icon: 'sorting' },
  { id: 'searching', name: 'Searching Sandbox', icon: 'search' },
  { id: 'graph', name: 'Graph Playground', icon: 'graph' },
  { id: 'stack-queue', name: 'Stack & Queue', icon: 'stack' }
];

const SortingSandbox = defineComponent({
  name: 'SortingSandbox',
  setup() {
    return () => h('div', { class: 'relative w-full h-full flex flex-col overflow-hidden' }, [
      h(ArrayBarVisualizer, { class: 'flex-1 w-full h-full min-h-0' })
    ]);
  }
});

const activeComponent = computed(() => {
  if (activeTab.value === 'searching') return SearchingView;
  if (activeTab.value === 'graph') return GraphView;
  if (activeTab.value === 'stack-queue') return StackQueueView;
  return SortingSandbox;
});

const activeProps = computed(() => ({}));
</script>

<style scoped>
.sorting-view-root {
  background-color: var(--color-bg-primary);
}

/* ── Sliding tab indicator: khối nền trượt ngang + đổi màu theo tab ── */
.tab-indicator {
  position: absolute;
  top: 2px;
  bottom: 2px;
  left: 0;
  border-radius: 6px;
  z-index: 0;
  transition:
    transform 0.3s cubic-bezier(0.16, 1, 0.3, 1),
    width 0.3s cubic-bezier(0.16, 1, 0.3, 1),
    background-color 0.3s ease,
    box-shadow 0.3s ease;
  pointer-events: none;
}

/* Màu nền + glow riêng từng tab */
.tab-indicator--sorting {
  background: linear-gradient(135deg, #c4b5fd, #8b5cf6, #7c3aed);
  box-shadow: 0 1px 8px rgba(139, 92, 246, 0.35);
}
.tab-indicator--searching {
  background: linear-gradient(135deg, #fdba74, #fb923c, #ea580c);
  box-shadow: 0 1px 8px rgba(249, 115, 22, 0.35);
}
.tab-indicator--graph {
  background: linear-gradient(135deg, #6ee7b7, #34d399, #059669);
  box-shadow: 0 1px 8px rgba(16, 185, 129, 0.35);
}
.tab-indicator--stack-queue {
  background: linear-gradient(135deg, #93c5fd, #3b82f6, #1d4ed8);
  box-shadow: 0 1px 8px rgba(59, 130, 246, 0.35);
}

/* Nút tab nằm TRÊN indicator (z-index 1), chữ active trắng, không nền riêng */
:deep(.sub-tab-pill) {
  position: relative;
  z-index: 1;
}
:deep(.sub-tab-pill) { background: transparent; }

/* ── Animation chuyển tab sandbox (Sorting · Searching · Graph) ── */
.sandbox-tab-enter-active {
  transition: opacity 0.28s ease, transform 0.28s cubic-bezier(0.16, 1, 0.3, 1);
}
.sandbox-tab-leave-active {
  transition: opacity 0.18s ease, transform 0.18s cubic-bezier(0.4, 0, 1, 1);
}
.sandbox-tab-enter-from {
  opacity: 0;
  transform: translateY(14px) scale(0.985);
}
.sandbox-tab-leave-to {
  opacity: 0;
  transform: translateY(-10px) scale(0.99);
}
</style>
