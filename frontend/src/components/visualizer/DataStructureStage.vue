<script setup lang="ts">
// DataStructureStage.vue — High-End WebGL Visualization Stage & macOS Terminal Surface
// Features:
// - Single Seamless Surface (Card Liền Khối duy nhất: border-radius 12px, dark ink #090d16, zero nested borders)
// - Modern macOS / IDE Header: 3 macOS dots, monospace filename badge (e.g. bubble_sort.ts • ARRAY_STAGE)
// - Custom Pill Switch controls for "Chỉ số" and "Giá trị" (replaces default HTML checkboxes)
// - Backdrop-blur Glassmorphism Audio SFX Toggle & Monospace Zoom Dropdown
// - Auto-resizing Viewport with 60fps WebGL rendering and dynamic vertical centering

import { computed, nextTick, onMounted, onUnmounted, ref, watch } from 'vue';
import { Volume2, VolumeX, ZoomIn, Check } from 'lucide-vue-next';
import type { Element, ElementStatus, Structure } from '@/engines/core/types';
import { usePixiStage } from '@/composables/usePixiStage';
import { useSoundEffects } from '@/composables/useSoundEffects';
import {
  ParticleManager,
  PixiArrayPainter,
  PixiGraphPainter,
  PixiLinearPainter,
  PixiTreePainter,
} from '@/engines/renderers/pixi';

const props = withDefaults(
  defineProps<{
    structure: Structure | null;
    showIndex?: boolean;
    showValues?: boolean;
    zoom?: number;
    emptyText?: string;
    interactive?: boolean;
    enableSound?: boolean;
    height?: number | string;
    simKey?: string;
    complexity?: string;
  }>(),
  {
    structure: null,
    showIndex: true,
    showValues: true,
    zoom: 1,
    emptyText: 'Khu vực vẽ cấu trúc dữ liệu',
    interactive: false,
    enableSound: true,
    height: '100%',
    simKey: '',
    complexity: '',
  },
);

const emit = defineEmits<{
  'update:show-index': [value: boolean];
  'update:show-values': [value: boolean];
  'update:zoom': [value: number];
  select: [element: Element];
}>();

const rootRef = ref<HTMLElement | null>(null);
const containerRef = ref<HTMLElement | null>(null);
const canvasRef = ref<HTMLCanvasElement | null>(null);

// Composables
const { isMuted, toggleMute, playCompare, playSwap, playLaser, playVictory, playPush, playPop } = useSoundEffects();
const { app, stage, isReady, mount, resize, dispose } = usePixiStage();

// Pixi Painters & Particle Manager
let particles: ParticleManager | null = null;
let arrayPainter: PixiArrayPainter | null = null;
let treePainter: PixiTreePainter | null = null;
let graphPainter: PixiGraphPainter | null = null;
let linearPainter: PixiLinearPainter | null = null;
let isDisposed = false;
// P8 — chỉ render lại cấu trúc khi cần (tránh renderActiveStructure() mỗi frame trong onTick)
let needsRender = false;

const ZOOM_OPTIONS = [0.5, 0.75, 1, 1.5, 2];

// Detect sound triggers between steps
let prevStatusMap = new Map<string, ElementStatus>();
let prevLinksCount = 0;
let prevElementsCount = 0;
// P9 — chỉ chơi Victory một lần khi mới chuyển sang all-done (tránh lặp mỗi frame)
let prevAllDone = false;

function triggerMicroSfx(newStructure: Structure | null): void {
  if (!props.enableSound || !newStructure) return;

  const elements = newStructure.elements;
  const isAllDone = elements.length > 0 && elements.every((e) => e.status === 'done');

  if (isAllDone) {
    // P9 — chỉ chơi Victory một lần khi vừa chuyển sang all-done
    if (!prevAllDone) {
      playVictory();
    }
    prevAllDone = true;
    return;
  }
  prevAllDone = false;

  // Push / Pop count change
  if (elements.length > prevElementsCount && prevElementsCount > 0) {
    playPush();
  } else if (elements.length < prevElementsCount) {
    playPop();
  }

  let hasSwap = false;
  let hasActive = false;

  elements.forEach((el) => {
    const prevStatus = prevStatusMap.get(el.id);
    if (el.status === 'swap' && prevStatus !== 'swap') {
      hasSwap = true;
    } else if ((el.status === 'active' || el.status === 'highlight') && prevStatus !== el.status) {
      hasActive = true;
    }
  });

  // Graph laser beam trigger
  if (newStructure.kind === 'graph' && newStructure.links.some((l) => l.status === 'active' || l.status === 'swap')) {
    playLaser();
  } else if (hasSwap) {
    playSwap();
  } else if (hasActive) {
    playCompare();
  }

  // Update tracking map
  prevStatusMap = new Map(elements.map((e) => [e.id, e.status]));
  prevLinksCount = newStructure.links.length;
  prevElementsCount = elements.length;
}

function initPixi(): void {
  if (!containerRef.value) return;

  // Initialize Particle Manager & Painters
  particles = new ParticleManager();
  arrayPainter = new PixiArrayPainter();
  treePainter = new PixiTreePainter();
  graphPainter = new PixiGraphPainter();
  linearPainter = new PixiLinearPainter();

  mount(containerRef.value, canvasRef.value, {
    backgroundColor: 0x090d16,
    antialias: true,
    onResize: () => {
      needsRender = true;
    },
    onTick: (deltaMs) => {
      // 60fps tick update — P8: KHÔNG render cấu trúc mỗi frame
      particles?.update(deltaMs);
      particles?.render();

      arrayPainter?.update(deltaMs, particles ?? undefined);
      treePainter?.update(deltaMs);
      graphPainter?.update(deltaMs);
      linearPainter?.update(deltaMs);

      // Chỉ render lại cấu trúc khi có thay đổi (needsRender) — set false sau render
      if (needsRender) {
        needsRender = false;
        renderActiveStructure();
      }
    },
  }).then((pixiApp) => {
    if (!isDisposed && pixiApp && stage.value && arrayPainter && treePainter && graphPainter && linearPainter && particles) {
      stage.value.addChild(arrayPainter.container);
      stage.value.addChild(treePainter.container);
      stage.value.addChild(graphPainter.container);
      stage.value.addChild(linearPainter.container);
      stage.value.addChild(particles.container);
      renderActiveStructure();
    }
  });
}

function renderActiveStructure(): void {
  if (isDisposed || !props.structure || !stage.value || !app.value || (app.value.renderer as any)?.destroyed) return;

  const clientW = containerRef.value?.clientWidth;
  const clientH = containerRef.value?.clientHeight;
  const w = clientW && clientW > 100 ? clientW : (app.value.screen?.width || 800);
  const h = clientH && clientH > 100 ? clientH : (app.value.screen?.height || 420);
  const kind = props.structure.kind;

  // Hide all painter containers first
  if (arrayPainter?.container && !(arrayPainter.container as any).destroyed) arrayPainter.container.visible = kind === 'array';
  if (treePainter?.container && !(treePainter.container as any).destroyed) treePainter.container.visible = kind === 'tree' || kind === 'heap';
  // B6 — tạm chấp nhận: hashtable vẽ bằng graph painter — backlog
  if (graphPainter?.container && !(graphPainter.container as any).destroyed) graphPainter.container.visible = kind === 'graph' || kind === 'hashtable';
  if (linearPainter?.container && !(linearPainter.container as any).destroyed) linearPainter.container.visible = kind === 'stack' || kind === 'queue' || kind === 'linkedlist';

  const opts = {
    showIndex: props.showIndex,
    showValues: props.showValues,
    zoom: props.zoom,
    showLegend: false,
  };

  // Render to corresponding painter
  if (kind === 'array' && arrayPainter) {
    arrayPainter.render(props.structure, opts, w, h, particles ?? undefined);
  } else if ((kind === 'tree' || kind === 'heap') && treePainter) {
    // B6 — tạm chấp nhận: heap không vẽ dải heap-array riêng — backlog
    treePainter.render(props.structure, opts, w, h, particles ?? undefined);
  } else if ((kind === 'graph' || kind === 'hashtable') && graphPainter) {
    graphPainter.render(props.structure, opts, w, h, particles ?? undefined);
  } else if ((kind === 'stack' || kind === 'queue' || kind === 'linkedlist') && linearPainter) {
    linearPainter.render(props.structure, opts, w, h, particles ?? undefined);
  }
}

watch(
  () => props.structure,
  (next) => {
    if (isDisposed) return;
    // P8 — giữ logic watch hiện có, nhưng thay render trực tiếp bằng cờ needsRender
    triggerMicroSfx(next);
    void nextTick(() => {
      if (!isDisposed) needsRender = true;
    });
  },
  { deep: true },
);

watch(
  () => [props.showIndex, props.showValues, props.zoom] as const,
  () => {
    if (!isDisposed) needsRender = true;
  },
);

onMounted(() => {
  isDisposed = false;
  initPixi();
});

onUnmounted(() => {
  isDisposed = true;
  // B2 — destroy painters + particles TRƯỚC (chúng tự destroy container/texture),
  // RỒI dispose() app (stop ticker, gỡ canvas + destroy stage children).
  particles?.destroy();
  arrayPainter?.destroy();
  treePainter?.destroy();
  graphPainter?.destroy();
  linearPainter?.destroy();
  particles = null;
  arrayPainter = null;
  treePainter = null;
  graphPainter = null;
  linearPainter = null;
  dispose();
});

const structureLabel = computed(() => (props.structure?.kind ?? 'stage').toUpperCase());

const terminalTitle = computed(() => {
  if (props.simKey) {
    const clean = props.simKey.replace(/[^a-zA-Z0-9_]/g, '_');
    return `${clean}.ts • ${structureLabel.value}_STAGE`;
  }
  return `${structureLabel.value.toLowerCase()}_visualizer.ts • ${structureLabel.value}_STAGE`;
});

const rootStyle = computed(() => {
  const h = typeof props.height === 'number' ? `${props.height}px` : props.height;
  return {
    height: h,
    minHeight: typeof props.height === 'number' ? `${props.height}px` : '420px',
  };
});
</script>

<template>
  <div ref="rootRef" class="data-stage" :style="rootStyle">
    <!-- Modern macOS / IDE Terminal Header -->
    <header class="data-stage__header">
      <!-- Left: macOS 3 Window Dots & Monospace Filename/Stage Label -->
      <div class="data-stage__header-left">
        <div class="data-stage__mac-dots" aria-hidden="true">
          <span class="data-stage__dot data-stage__dot--red" />
          <span class="data-stage__dot data-stage__dot--yellow" />
          <span class="data-stage__dot data-stage__dot--green" />
        </div>
        <span class="data-stage__file-badge" :title="terminalTitle">
          {{ terminalTitle }}
        </span>
      </div>

      <!-- Right: Modern Pill Controls, Sound Toggle, Zoom, Kind Badge -->
      <div class="data-stage__header-right">
        <!-- Toggle: Show Index -->
        <button
          type="button"
          class="data-stage__pill"
          :class="{ 'data-stage__pill--active': showIndex }"
          :aria-pressed="showIndex"
          @click="emit('update:show-index', !showIndex)"
        >
          <span class="data-stage__pill-indicator" aria-hidden="true">
            <Check v-if="showIndex" :size="10" />
          </span>
          <span class="data-stage__pill-text">Chỉ số</span>
        </button>

        <!-- Toggle: Show Values -->
        <button
          type="button"
          class="data-stage__pill"
          :class="{ 'data-stage__pill--active': showValues }"
          :aria-pressed="showValues"
          @click="emit('update:show-values', !showValues)"
        >
          <span class="data-stage__pill-indicator" aria-hidden="true">
            <Check v-if="showValues" :size="10" />
          </span>
          <span class="data-stage__pill-text">Giá trị</span>
        </button>

        <!-- Audio SFX Mute Toggle (Glassmorphism Badge) -->
        <button
          v-if="enableSound"
          type="button"
          class="data-stage__sound-btn"
          :class="{ 'data-stage__sound-btn--muted': isMuted }"
          :aria-label="isMuted ? 'Bật âm thanh micro-SFX' : 'Tắt âm thanh micro-SFX'"
          :aria-pressed="!isMuted"
          @click="toggleMute"
        >
          <component :is="isMuted ? VolumeX : Volume2" :size="13" aria-hidden="true" />
          <span class="data-stage__sound-text">{{ isMuted ? 'Tắt âm' : 'Âm thanh' }}</span>
        </button>

        <!-- Zoom Dropdown Selector -->
        <div class="data-stage__zoom-wrap">
          <ZoomIn :size="12" class="data-stage__zoom-icon" aria-hidden="true" />
          <select
            :value="zoom"
            class="data-stage__zoom-select"
            aria-label="Thu phóng vùng nhìn"
            @change="emit('update:zoom', Number(($event.target as HTMLSelectElement).value))"
          >
            <option v-for="z in ZOOM_OPTIONS" :key="z" :value="z">{{ Math.round(z * 100) }}%</option>
          </select>
        </div>

        <!-- Complexity / Kind Badge -->
        <span v-if="complexity" class="data-stage__complexity-chip" :title="'Độ phức tạp: ' + complexity">
          {{ complexity }}
        </span>
      </div>
    </header>

    <!-- WebGL Canvas Viewport (Seamless full-bleed surface) -->
    <div ref="containerRef" class="data-stage__viewport">
      <canvas ref="canvasRef" class="data-stage__canvas" :aria-label="emptyText" />
      <p v-if="!structure" class="data-stage__hint">{{ emptyText }}</p>
    </div>
  </div>
</template>

<style scoped>
/* ── 1. Khung Card Liền Khối duy nhất (Single Seamless Terminal Surface) ── */
.data-stage {
  display: flex;
  flex-direction: column;
  width: 100%;
  border-radius: var(--radius-lg, 12px);
  border: 1px solid rgba(255, 255, 255, 0.08);
  background: #090d16;
  overflow: hidden;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.45);
  position: relative;
  box-sizing: border-box;
}

/* ── 2. Header Terminal chuẩn macOS / IDE (Modern Studio Bar) ── */
.data-stage__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: var(--space-sm, 8px);
  padding: 8px 14px;
  background: #0c1220;
  border-bottom: 1px solid rgba(255, 255, 255, 0.06);
  flex-wrap: wrap;
  user-select: none;
  min-height: 40px;
}

.data-stage__header-left {
  display: flex;
  align-items: center;
  gap: 10px;
  min-width: 0;
}

/* 3 Nút macOS tròn tinh tế (10px) */
.data-stage__mac-dots {
  display: inline-flex;
  align-items: center;
  gap: 6px;
}

.data-stage__dot {
  width: 10px;
  height: 10px;
  border-radius: 50%;
  display: block;
  transition: opacity 0.15s ease;
}

.data-stage__dot--red {
  background: #ff5f56;
  box-shadow: 0 0 4px rgba(255, 95, 86, 0.4);
}

.data-stage__dot--yellow {
  background: #ffbd2e;
  box-shadow: 0 0 4px rgba(255, 189, 46, 0.4);
}

.data-stage__dot--green {
  background: #27c93f;
  box-shadow: 0 0 4px rgba(39, 201, 63, 0.4);
}

/* Monospace Filename Badge */
.data-stage__file-badge {
  font-family: 'JetBrains Mono', 'Fira Code', ui-monospace, monospace;
  font-size: 11.5px;
  font-weight: 500;
  color: #94a3b8;
  letter-spacing: -0.01em;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  max-width: 280px;
}

/* Right Controls Container */
.data-stage__header-right {
  display: flex;
  align-items: center;
  gap: 8px;
  flex-wrap: wrap;
}

/* ── 3. Pill Chips / Custom Switches (Chỉ số, Giá trị) ── */
.data-stage__pill {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 3px 9px;
  font-family: 'JetBrains Mono', 'Fira Code', ui-monospace, monospace;
  font-size: 11px;
  font-weight: 500;
  font-variant-numeric: tabular-nums;
  color: #64748b;
  background: rgba(255, 255, 255, 0.02);
  border: 1px solid rgba(255, 255, 255, 0.07);
  border-radius: var(--radius-full, 9999px);
  cursor: pointer;
  transition: all 0.15s ease;
}

.data-stage__pill:hover {
  background: rgba(255, 255, 255, 0.06);
  color: #cbd5e1;
  border-color: rgba(255, 255, 255, 0.12);
}

.data-stage__pill--active {
  background: rgba(94, 234, 212, 0.12);
  border-color: rgba(94, 234, 212, 0.35);
  color: #5eead4;
}

.data-stage__pill--active:hover {
  background: rgba(94, 234, 212, 0.18);
  border-color: rgba(94, 234, 212, 0.5);
  color: #5eead4;
}

.data-stage__pill-indicator {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 12px;
  height: 12px;
  border-radius: 50%;
  background: rgba(255, 255, 255, 0.08);
  color: #090d16;
}

.data-stage__pill--active .data-stage__pill-indicator {
  background: #5eead4;
}

.data-stage__pill-text {
  line-height: 1;
}

/* ── 4. Sound Button (Glassmorphism Badge) ── */
.data-stage__sound-btn {
  display: inline-flex;
  align-items: center;
  gap: 5px;
  padding: 3px 9px;
  font-family: 'JetBrains Mono', 'Fira Code', ui-monospace, monospace;
  font-size: 11px;
  font-weight: 500;
  color: #5eead4;
  background: rgba(94, 234, 212, 0.08);
  border: 1px solid rgba(94, 234, 212, 0.25);
  border-radius: 6px;
  backdrop-filter: blur(8px);
  cursor: pointer;
  transition: all 0.15s ease;
}

.data-stage__sound-btn:hover {
  background: rgba(94, 234, 212, 0.15);
  border-color: rgba(94, 234, 212, 0.4);
}

.data-stage__sound-btn--muted {
  color: #64748b;
  background: rgba(255, 255, 255, 0.03);
  border-color: rgba(255, 255, 255, 0.08);
}

.data-stage__sound-btn--muted:hover {
  color: #94a3b8;
  background: rgba(255, 255, 255, 0.07);
}

.data-stage__sound-text {
  line-height: 1;
}

/* ── 5. Zoom Dropdown Selector ── */
.data-stage__zoom-wrap {
  display: inline-flex;
  align-items: center;
  position: relative;
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 6px;
  padding: 2px 6px;
  gap: 4px;
}

.data-stage__zoom-icon {
  color: #64748b;
  pointer-events: none;
}

.data-stage__zoom-select {
  font-family: 'JetBrains Mono', 'Fira Code', ui-monospace, monospace;
  font-size: 11px;
  font-weight: 500;
  background: transparent;
  color: #cbd5e1;
  border: none;
  padding: 1px 2px;
  cursor: pointer;
  outline: none;
}

.data-stage__zoom-select:focus-visible {
  color: #5eead4;
}

.data-stage__zoom-select option {
  background: #0c1220;
  color: #f8fafc;
}

/* ── 6. Complexity Chip ── */
.data-stage__complexity-chip {
  font-family: 'JetBrains Mono', 'Fira Code', ui-monospace, monospace;
  font-size: 10.5px;
  font-weight: 600;
  color: #38bdf8;
  background: rgba(56, 189, 248, 0.1);
  border: 1px solid rgba(56, 189, 248, 0.25);
  padding: 2px 8px;
  border-radius: 4px;
  letter-spacing: 0.02em;
}

/* ── 7. Viewport & Canvas (Seamless, zero internal nested borders) ── */
.data-stage__viewport {
  position: relative;
  width: 100%;
  flex: 1 1 0%;
  min-height: 0;
  background: #090d16;
  overflow: hidden;
  box-sizing: border-box;
}

.data-stage__canvas {
  width: 100%;
  height: 100%;
  display: block;
}

.data-stage__hint {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  font-family: 'JetBrains Mono', 'Fira Code', ui-monospace, monospace;
  color: #64748b;
  font-size: var(--text-sm, 13px);
  text-align: center;
  padding: var(--space-md, 16px);
  pointer-events: none;
}

/* Responsive adjustments */
@media (max-width: 768px) {
  .data-stage__header {
    padding: 6px 10px;
    gap: 6px;
  }
  .data-stage__file-badge {
    max-width: 160px;
    font-size: 10.5px;
  }
  .data-stage__header-right {
    gap: 6px;
  }
  .data-stage__pill {
    padding: 2px 7px;
    font-size: 10px;
  }
  .data-stage__sound-btn {
    padding: 2px 7px;
    font-size: 10px;
  }
}
</style>
