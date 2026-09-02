<script setup lang="ts">
// CanvasArea — vùng vẽ mô phỏng (SDD §8.5 — vùng giữa 6/12)
// - Vẽ Structure (mảng/stack/queue/linkedlist/tree/heap/hashtable/graph) lên canvas
// - Thanh công cụ RenderOptions: hiện chỉ số / hiện giá trị / zoom (50-200%)
// - Resize theo container (ResizeObserver)
import { computed, onMounted, onUnmounted, ref, watch } from 'vue';

import type { Element, ElementStatus, Structure } from '@/engines/core/types';
import type { Renderer } from '@/engines/renderers/interface';
import { getRendererForKind } from '@/engines/renderers/rendererRegistry';
import { useStructureTransition } from '@/composables/useStructureTransition';
import { useSoundEffects } from '@/composables/useSoundEffects';
import { Volume2, VolumeX } from 'lucide-vue-next';
import BaseIcon from '@/components/ui/BaseIcon.vue';

const props = withDefaults(
  defineProps<{
    structure: Structure | null;
    showIndex?: boolean;
    showValues?: boolean;
    zoom?: number;
    emptyText?: string;
    /** Chế độ editable (Lab Bậc 2) — phát sự kiện select */
    interactive?: boolean;
  }>(),
  {
    structure: null,
    showIndex: true,
    showValues: true,
    zoom: 1,
    emptyText: 'Khu vực vẽ cấu trúc dữ liệu',
    interactive: false,
  },
);

const emit = defineEmits<{
  'update:show-index': [value: boolean];
  'update:show-values': [value: boolean];
  'update:zoom': [value: number];
  select: [element: Element];
}>();

const canvasRef = ref<HTMLCanvasElement | null>(null);
let ctx: CanvasRenderingContext2D | null = null;
let observer: ResizeObserver | null = null;

// Renderer thật (SDD §8.3): instance theo kind hiện tại; đổi kind → dispose + mount renderer mới.
let activeRenderer: Renderer | null = null;
let activeKind = '';
let lastView = { w: 0, h: 0 };

// Transition push/pop stack/queue (Task 3): viewport LOGIC (đã chia zoom) để layout trong
// composable khớp layout renderer (renderer vẽ theo logicalWidth/Height = viewport / zoom).
const transition = useStructureTransition({
  viewport: () => ({ width: lastView.w / props.zoom, height: lastView.h / props.zoom }),
});

const ZOOM_OPTIONS = [0.5, 0.75, 1, 1.5, 2];

const STATUS_COLORS: Record<ElementStatus, string> = {
  default: '#5EEAD4',
  active: '#FBBF24',
  highlight: '#F59E0B',
  swap: '#F87171',
  done: '#34D399',
  error: '#EF4444',
  muted: '#CBD5E1',
};

function statusColor(status: ElementStatus): string {
  return STATUS_COLORS[status] ?? STATUS_COLORS.default;
}

/** Bố cục theo kind — trả về vị trí (x, y) của từng element */
function layoutStructure(structure: Structure, width: number, height: number): Map<string, { x: number; y: number; radius: number }> {
  const positions = new Map<string, { x: number; y: number; radius: number }>();
  const elements = structure.elements;
  const n = elements.length;
  const cellW = Math.min(64, Math.max(28, (width - 32) / Math.max(1, n)));
  const cellH = 56;
  const cy = height / 2;

  if (structure.kind === 'tree' || structure.kind === 'heap') {
    // Cây: bố trí theo cấu trúc links cha → con (đơn giản: xếp tầng)
    const levelOf = new Map<string, number>();
    const childrenOf = new Map<string, string[]>();
    const parents = new Set<string>();
    for (const link of structure.links) {
      if (!childrenOf.has(link.from)) childrenOf.set(link.from, []);
      childrenOf.get(link.from)?.push(link.to);
      parents.add(link.to);
    }
    const roots = elements.filter((el) => !parents.has(el.id));
    if (roots.length === 0 && elements.length > 0) levelOf.set(elements[0].id, 0);
    else roots.forEach((r, i) => levelOf.set(r.id, i === 0 ? 0 : 0));

    // BFS gán tầng
    const queue = roots.map((r) => r.id);
    const maxDepth: Record<number, number> = { 0: queue.length };
    const seen = new Set<string>(queue);
    while (queue.length > 0) {
      const id = queue.shift();
      if (!id) continue;
      const lvl = levelOf.get(id) ?? 0;
      for (const child of childrenOf.get(id) ?? []) {
        if (!seen.has(child)) {
          seen.add(child);
          levelOf.set(child, lvl + 1);
          maxDepth[lvl + 1] = (maxDepth[lvl + 1] ?? 0) + 1;
          queue.push(child);
        }
      }
    }
    // Vẽ theo cột (level) — mỗi level 1 hàng dọc
    const depth = Math.max(1, ...Array.from(levelOf.values()).map((l) => l + 1));
    const levelH = (height - 24) / depth;
    const colWidth = Math.min(72, Math.max(36, (width - 24) / Math.max(1, depth)));
    for (const el of elements) {
      const lvl = levelOf.get(el.id) ?? 0;
      const countInLevel = Array.from(levelOf.values()).filter((l) => l === lvl).length;
      const idxInLevel = Array.from(levelOf.entries())
        .filter(([, l]) => l === lvl)
        .map(([id]) => id)
        .indexOf(el.id);
      const x = 12 + colWidth / 2 + lvl * colWidth;
      const y = 12 + levelH / 2 + lvl * levelH * 0.9;
      positions.set(el.id, { x, y, radius: Math.max(14, Math.min(26, colWidth / 2.4)) });
      void idxInLevel;
      void countInLevel;
    }
    return positions;
  }

  if (structure.kind === 'graph' || structure.kind === 'hashtable') {
    // Đồ thị: bố trí vòng tròn quanh tâm; bảng băm: cột theo bucket (group)
    if (structure.kind === 'hashtable') {
      const groups = new Map<string, Element[]>();
      for (const el of elements) {
        const g = el.group ?? 'default';
        if (!groups.has(g)) groups.set(g, []);
        groups.get(g)?.push(el);
      }
      const cols = Math.max(1, groups.size);
      const colW = width / cols;
      groups.forEach((els, group) => {
        const col = Array.from(groups.keys()).indexOf(group);
        const cx = colW * col + colW / 2;
        els.forEach((el, i) => {
          positions.set(el.id, { x: cx, y: 24 + i * (cellH + 8), radius: 18 });
        });
      });
      return positions;
    }
    const radius = Math.min(width, height) / 2 - 30;
    elements.forEach((el, i) => {
      const angle = (i / Math.max(1, elements.length)) * Math.PI * 2 - Math.PI / 2;
      positions.set(el.id, {
        x: width / 2 + radius * Math.cos(angle),
        y: height / 2 + radius * Math.sin(angle),
        radius: 20,
      });
    });
    return positions;
  }

  // Mảng / stack / queue / linkedlist: hàng ngang
  elements.forEach((el, i) => {
    positions.set(el.id, {
      x: 16 + cellW / 2 + i * cellW,
      y: cy,
      radius: Math.min(cellW, cellH) / 2 - 6,
    });
  });
  return positions;
}

/** Lấy (hoặc tạo) renderer cho kind hiện tại; null nếu kind chưa có renderer → dùng fallback cũ. */
function ensureRenderer(): Renderer | null {
  if (!canvasRef.value || !props.structure) return null;
  const renderer = getRendererForKind(props.structure.kind);
  if (!renderer) {
    if (activeRenderer) {
      activeRenderer.dispose();
      activeRenderer = null;
      activeKind = '';
    }
    return null;
  }
  if (activeKind !== props.structure.kind || !activeRenderer) {
    activeRenderer?.dispose();
    activeRenderer = renderer;
    activeKind = props.structure.kind;
    renderer.mount(canvasRef.value);
    renderer.resize(lastView.w, lastView.h);
  }
  return activeRenderer;
}

/** Vẽ 1 structure (thường hoặc frame transition) lên canvas — dùng chung render() và transition. */
function doRender(structure: Structure | null): void {
  if (!canvasRef.value || !ctx) return;
  const canvas = canvasRef.value;
  ctx.clearRect(0, 0, canvas.width, canvas.height);
  if (!structure) return;

  // Renderer thật (SDD §8.3) — ưu tiên; kind chưa có renderer → fallback vẽ inline cũ.
  const renderer = ensureRenderer();
  if (renderer) {
    renderer.resize(lastView.w, lastView.h);
    renderer.render(structure, {
      showIndex: props.showIndex,
      showValues: props.showValues,
      zoom: props.zoom,
      showLegend: false,
    });
    return;
  }

  const dpr = window.devicePixelRatio || 1;
  const w = canvas.width / dpr;
  const h = canvas.height / dpr;

  ctx.save();
  ctx.scale(props.zoom, props.zoom);
  const viewW = w / props.zoom;
  const viewH = h / props.zoom;

  const positions = layoutStructure(structure, viewW, viewH);

  // Vẽ link (cạnh) trước
  for (const link of structure.links) {
    const from = positions.get(link.from);
    const to = positions.get(link.to);
    if (!from || !to) continue;
    ctx.strokeStyle = link.status ? statusColor(link.status) : '#94A3B8';
    ctx.lineWidth = 2;
    ctx.beginPath();
    ctx.moveTo(from.x, from.y);
    ctx.lineTo(to.x, to.y);
    ctx.stroke();
    if (link.label) {
      ctx.fillStyle = '#5E7A77';
      ctx.font = '12px sans-serif';
      ctx.fillText(link.label, (from.x + to.x) / 2, (from.y + to.y) / 2 - 6);
    }
  }

  // Vẽ phần tử
  const isArrayLike = structure.kind === 'array' || structure.kind === 'stack' || structure.kind === 'queue';
  for (const el of structure.elements) {
    const pos = positions.get(el.id);
    if (!pos) continue;
    const r = pos.radius;
    const color = statusColor(el.status);
    ctx.fillStyle = color;
    ctx.strokeStyle = '#134E4A';
    ctx.lineWidth = 1.5;
    ctx.beginPath();
    if (isArrayLike) {
      ctx.roundRect(pos.x - r, pos.y - 18, r * 2, 36, 6);
      ctx.fill();
      ctx.stroke();
    } else {
      ctx.arc(pos.x, pos.y, r, 0, Math.PI * 2);
      ctx.fill();
      ctx.stroke();
    }
    if (props.showValues) {
      ctx.fillStyle = el.status === 'muted' ? '#64748B' : '#134E4A';
      ctx.font = 'bold 13px sans-serif';
      ctx.textAlign = 'center';
      ctx.textBaseline = 'middle';
      ctx.fillText(el.label, pos.x, pos.y);
    }
    if (props.showIndex && isArrayLike) {
      const idx = el.id.replace(/^cell:/, '');
      if (/^\d+$/.test(idx)) {
        ctx.fillStyle = '#5E7A77';
        ctx.font = '10px sans-serif';
        ctx.fillText(idx, pos.x, pos.y + 28);
      }
    }
  }
  ctx.restore();
}

function render(): void {
  if (!canvasRef.value || !ctx) return;
  if (transition.isAnimating()) return; // transition đang vẽ từng frame — không vẽ đè
  doRender(props.structure);
}

function resize(): void {
  const canvas = canvasRef.value;
  if (!canvas) return;
  transition.cancel(); // resize → vẽ thẳng, không chạy transition
  const parent = canvas.parentElement;
  if (!parent) return;
  const dpr = window.devicePixelRatio || 1;
  const w = Math.max(200, parent.clientWidth);
  const h = Math.max(240, parent.clientHeight || 320);
  // Guard chống vòng lặp ResizeObserver: viewport kích thước không đổi
  // (|delta| < 2px) thì không set lại canvas → không phình thêm, hết feedback loop.
  if (Math.abs(w - lastView.w) < 2 && Math.abs(h - lastView.h) < 2) return;
  lastView = { w, h };
  canvas.width = w * dpr;
  canvas.height = h * dpr;
  canvas.style.width = '100%';
  canvas.style.height = parent.clientHeight ? `${parent.clientHeight}px` : '320px';
  activeRenderer?.resize(w, h);
  render();
}

function onCanvasClick(event: MouseEvent): void {
  if (!props.interactive || !canvasRef.value || !props.structure || !ctx) return;
  const rect = canvasRef.value.getBoundingClientRect();
  const x = (event.clientX - rect.left) / props.zoom;
  const y = (event.clientY - rect.top) / props.zoom;
  const positions = layoutStructure(props.structure, rect.width / props.zoom, rect.height / props.zoom);
  for (const el of props.structure.elements) {
    const pos = positions.get(el.id);
    if (!pos) continue;
    const dx = x - pos.x;
    const dy = y - pos.y;
    if (dx * dx + dy * dy <= (pos.radius + 6) * (pos.radius + 6)) {
      emit('select', el);
      return;
    }
  }
}

// Sound effects integration
const { isMuted, toggleMute, playCompare, playSwap, playVictory } = useSoundEffects();
let prevAllDone = false;

function triggerAudioFeedback(next: Structure | null): void {
  if (!next || isMuted.value) return;
  const elements = next.elements;
  if (elements.length === 0) return;
  const allDone = elements.every((e) => e.status === 'done');
  if (allDone) {
    if (!prevAllDone) {
      playVictory();
    }
    prevAllDone = true;
    return;
  }
  prevAllDone = false;

  const hasSwap = elements.some((e) => e.status === 'swap');
  if (hasSwap) {
    playSwap();
    return;
  }
  const activeEl = elements.find((e) => e.status === 'active' || e.status === 'highlight');
  if (activeEl) {
    const val = Number(activeEl.label);
    if (Number.isFinite(val)) {
      playCompare(val);
    } else {
      playCompare(50);
    }
  }
}

// Structure đổi → transition (push/pop stack/queue); options đổi → vẽ thẳng.
let prevStructure: Structure | null = null;

watch(
  () => props.structure,
  (next) => {
    triggerAudioFeedback(next);
    if (next === null) {
      transition.cancel();
      doRender(null);
      prevStructure = null;
      return;
    }
    transition.update(prevStructure, next, (s) => {
      doRender(s);
      prevStructure = next; // cập nhật sau khi đã vẽ (trong callback renderFrame)
    });
  },
  { deep: true },
);

watch(
  () => [props.showIndex, props.showValues, props.zoom] as const,
  () => {
    transition.cancel(); // đổi option/zoom → vẽ thẳng fallback
    render();
  },
);

onMounted(() => {
  if (canvasRef.value) {
    ctx = canvasRef.value.getContext('2d');
  }
  resize();
  if (typeof ResizeObserver !== 'undefined') {
    observer = new ResizeObserver(() => resize());
    if (canvasRef.value?.parentElement) {
      observer.observe(canvasRef.value.parentElement);
    }
  }
  window.addEventListener('resize', resize);
});

onUnmounted(() => {
  transition.cancel();
  observer?.disconnect();
  window.removeEventListener('resize', resize);
  activeRenderer?.dispose();
  activeRenderer = null;
  activeKind = '';
});

const structureLabel = computed(() => props.structure?.kind ?? '');
</script>

<template>
  <div class="canvas-area">
    <div class="canvas-area__toolbar">
      <label class="canvas-area__opt">
        <input
          type="checkbox"
          name="render-option"
          :checked="showIndex"
          @change="emit('update:show-index', ($event.target as HTMLInputElement).checked)"
        />
        <span>Chỉ số</span>
      </label>
      <label class="canvas-area__opt">
        <input
          type="checkbox"
          name="render-option"
          :checked="showValues"
          @change="emit('update:show-values', ($event.target as HTMLInputElement).checked)"
        />
        <span>Giá trị</span>
      </label>
      <button
        type="button"
        class="canvas-area__sound-btn"
        :aria-label="isMuted ? 'Bật âm thanh' : 'Tắt âm thanh'"
        :title="isMuted ? 'Bật hiệu ứng âm thanh' : 'Tắt hiệu ứng âm thanh'"
        @click="toggleMute"
      >
        <component :is="isMuted ? VolumeX : Volume2" :size="14" />
        <span class="text-[11px]">{{ isMuted ? 'Tắt âm' : 'Âm thanh' }}</span>
      </button>
      <label class="canvas-area__zoom">
        <BaseIcon name="search" :size="13" />
        <select :value="zoom" aria-label="Zoom" @change="emit('update:zoom', Number(($event.target as HTMLSelectElement).value))">
          <option v-for="z in ZOOM_OPTIONS" :key="z" :value="z">{{ Math.round(z * 100) }}%</option>
        </select>
      </label>
      <span v-if="structureLabel" class="canvas-area__kind">{{ structureLabel }}</span>
    </div>
    <div class="canvas-area__viewport">
      <canvas
        ref="canvasRef"
        class="canvas-area__canvas"
        :aria-label="emptyText"
        @click="onCanvasClick"
      />
      <p v-if="!structure" class="canvas-area__hint">{{ emptyText }}</p>

      <!-- Mini Legend Bar ghim dưới Canvas -->
      <div v-if="structure" class="canvas-area__mini-legend" aria-label="Chú giải trạng thái">
        <template v-if="structure.kind === 'queue'">
          <span class="legend-badge"><span class="legend-dot" style="background:#34D399" /> Phần tử trong hàng đợi</span>
          <span class="legend-badge"><span class="legend-dot" style="background:#F87171" /> Đang thêm (Enqueue)</span>
          <span class="legend-badge"><span class="legend-dot" style="background:#CBD5E1" /> Ô trống (chưa dùng)</span>
          <span class="legend-badge"><span class="legend-dot" style="background:#38bdf8" /> ↑ front (Đầu lấy ra)</span>
          <span class="legend-badge"><span class="legend-dot" style="background:#fb923c" /> ↑ rear (Cuối thêm vào)</span>
        </template>
        <template v-else-if="structure.kind === 'stack'">
          <span class="legend-badge"><span class="legend-dot" style="background:#34D399" /> Phần tử trong Stack</span>
          <span class="legend-badge"><span class="legend-dot" style="background:#F87171" /> Đang Push / Pop</span>
          <span class="legend-badge"><span class="legend-dot" style="background:#CBD5E1" /> Ô trống</span>
          <span class="legend-badge"><span class="legend-dot" style="background:#FBBF24" /> ← top (Đỉnh ngăn xếp)</span>
        </template>
        <template v-else>
          <span class="legend-badge"><span class="legend-dot" style="background:#5EEAD4" /> Chưa xét</span>
          <span class="legend-badge"><span class="legend-dot" style="background:#FBBF24" /> So sánh</span>
          <span class="legend-badge"><span class="legend-dot" style="background:#F59E0B" /> Pivot / Khóa</span>
          <span class="legend-badge"><span class="legend-dot" style="background:#F87171" /> Hoán đổi</span>
          <span class="legend-badge"><span class="legend-dot" style="background:#34D399" /> Đã chốt</span>
        </template>
      </div>
    </div>
  </div>
</template>

<style scoped>
.canvas-area {
  display: flex;
  flex-direction: column;
  gap: 6px;
  height: 100%;
  width: 100%;
  flex: 1 1 0%;
  min-height: 0;
  overflow: hidden;
}

.canvas-area__toolbar {
  display: flex;
  align-items: center;
  gap: var(--space-md);
  flex-wrap: wrap;
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  flex-shrink: 0;
}

.canvas-area__opt {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  cursor: pointer;
}

.canvas-area__zoom {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  margin-left: auto;
}

.canvas-area__zoom select {
  padding: 2px 6px;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  background: var(--color-surface);
  color: var(--color-foreground);
}

.canvas-area__kind {
  text-transform: uppercase;
  letter-spacing: 0.5px;
  background: var(--color-muted);
  padding: 2px 8px;
  border-radius: var(--radius-sm);
}

/* Chiều cao linh hoạt theo container, tự co giãn trong flex column */
.canvas-area__viewport {
  position: relative;
  flex: 1 1 0%;
  min-height: 140px;
  width: 100%;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-lg);
  background: var(--color-muted);
  overflow: hidden;
}

.canvas-area__hint {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--color-text-muted);
  font-size: var(--text-sm);
  text-align: center;
  padding: var(--space-md);
}

.canvas-area__sound-btn {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  background: transparent;
  border: 1px solid var(--color-border);
  border-radius: var(--radius-sm);
  padding: 2px 8px;
  font-size: var(--text-xs);
  color: var(--color-text-muted);
  cursor: pointer;
  transition: all 150ms ease;
}

.canvas-area__sound-btn:hover {
  color: var(--color-foreground);
  border-color: var(--color-accent-primary, #6b7bff);
  background: rgba(107, 123, 255, 0.1);
}

.canvas-area__mini-legend {
  position: absolute;
  bottom: 8px;
  left: 50%;
  transform: translateX(-50%);
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 4px 12px;
  background: rgba(13, 16, 32, 0.85);
  backdrop-filter: blur(8px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 9999px;
  font-size: 10px;
  color: #94a3b8;
  pointer-events: none;
  z-index: 10;
  flex-wrap: wrap;
  justify-content: center;
  max-width: 95%;
}

.legend-badge {
  display: inline-flex;
  align-items: center;
  gap: 4px;
  white-space: nowrap;
}

.legend-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  display: inline-block;
  box-shadow: 0 0 4px currentColor;
}
</style>
