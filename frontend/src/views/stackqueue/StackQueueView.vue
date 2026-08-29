<template>
  <div class="sq-root">
    <!-- ─── TOP BAR: 2 cấu trúc GỐC ─── -->
    <div class="sq-topbar">
      <div class="sq-topbar-left">
        <div class="sq-badge">
          <span class="sq-badge-dot"></span>
          Stack &amp; Queue Sandbox
        </div>
        <div class="sq-tag">INTERACTIVE</div>
      </div>

      <div class="sq-mode-toggle">
        <button
          v-for="r in ROOTS"
          :key="r.id"
          class="sq-mode-btn"
          :class="{ 'sq-mode-btn--active': root === r.id }"
          @click="setRoot(r.id)"
          :title="r.title"
        >
          <BaseIcon :name="r.icon" class="w-3.5 h-3.5" /> {{ r.label }} <span class="sq-mode-hint">{{ r.hint }}</span>
        </button>
      </div>

      <div class="sq-topbar-right">
        <span class="sq-fps">● VISUALGO-MODE 60FPS</span>
      </div>
    </div>

    <!-- ─── BODY ─── -->
    <div class="sq-body">
      <!-- LEFT: điều khiển -->
      <aside class="sq-panel">
        <!-- Thao tác chính -->
        <section class="sq-section">
          <h4 class="sq-section-title">{{ opsTitle }}</h4>
          <div class="sq-add-row">
            <input type="number" v-model.number="valueInput" class="sq-value-input" placeholder="Giá trị…" />
            <button class="sq-btn-add" @click="addOp({ kind: 'push', value: valueInput })">
              <BaseIcon name="plus" class="w-3 h-3" /> {{ pushBtnLabel }}
            </button>
          </div>
          <button v-if="effectiveMode === 'deque'" class="sq-btn" @click="addOp({ kind: 'pushFront', value: valueInput })">
            <BaseIcon name="chevron-left" class="w-3 h-3" /> Thêm trước
          </button>
          <div class="sq-btn-grid">
            <button class="sq-btn" @click="addOp({ kind: 'pop' })">
              <BaseIcon name="minus" class="w-3 h-3" /> {{ popBtnLabel }}
            </button>
            <button v-if="effectiveMode === 'stack'" class="sq-btn" @click="addOp({ kind: 'peek' })">
              <BaseIcon name="eye" class="w-3 h-3" /> Peek
            </button>
            <button v-if="effectiveMode === 'deque'" class="sq-btn" @click="addOp({ kind: 'popFront' })">
              <BaseIcon name="chevron-right" class="w-3 h-3" /> Bỏ trước
            </button>
          </div>
        </section>

        <!-- Dung lượng -->
        <section class="sq-section">
          <div class="sq-cap-row">
            <h4 class="sq-section-title">Dung lượng</h4>
            <span class="sq-cap-val">{{ capacity }} ô</span>
          </div>
          <input type="range" min="3" max="9" v-model.number="capacity" class="sq-slider" />
        </section>

        <!-- Chuỗi mẫu -->
        <section class="sq-section">
          <h4 class="sq-section-title">Chuỗi mẫu</h4>
          <div class="sq-pills">
            <button
              v-for="p in presets"
              :key="p.key"
              class="sq-pill"
              :class="{ 'sq-pill--active': currentPreset === p.key }"
              @click="loadPreset(p)"
            >{{ p.label }}</button>
          </div>
          <div class="sq-btn-grid">
            <button class="sq-btn" @click="runRandom">
              <BaseIcon name="dice" class="w-3 h-3" /> Ngẫu nhiên
            </button>
            <button class="sq-btn" @click="clearOps">
              <BaseIcon name="trash" class="w-3 h-3" /> Xóa
            </button>
          </div>
        </section>

        <!-- Chuỗi hiện tại -->
        <section class="sq-section">
          <h4 class="sq-section-title">Chuỗi hiện tại ({{ ops.length }})</h4>
          <div class="sq-ops-list">
            <span v-for="(op, i) in ops" :key="i" class="sq-op-chip" :class="{ 'sq-op-chip--pop': op.kind !== 'push' }">
              {{ opLabel(op) }}
            </span>
            <span v-if="!ops.length" class="sq-empty">Chưa có thao tác.</span>
          </div>
        </section>

        <!-- Nhật ký (gọn, cuộn được) -->
        <section class="sq-section sq-log-section">
          <button class="sq-log-toggle" @click="logOpen = !logOpen">
            <span>Nhật ký</span>
            <span class="sq-caret">{{ logOpen ? '▾' : '▸' }}</span>
          </button>
          <div v-if="logOpen" class="sq-log-box">
            <div class="sq-log-entry" v-for="(msg, i) in logs" :key="i" :class="{ 'sq-log-entry--error': !msg.ok }">
              <span class="sq-log-bullet" :class="{ 'sq-log-bullet--error': !msg.ok }"></span>
              <span class="sq-log-text">{{ msg.text }}</span>
            </div>
            <div v-if="!logs.length" class="sq-empty">Bấm một thao tác để bắt đầu…</div>
          </div>
        </section>
      </aside>

      <!-- RIGHT: canvas -->
      <div class="sq-canvas">
        <!-- Thanh biến thể (con) — khung bo tròn giống thanh pills thuật toán của Sorting Sandbox -->
        <div class="sq-variant-box">
          <span class="sq-variant-title">Biến thể:</span>
          <button
            v-for="v in (root === 'stack' ? STACK_VARIANTS : QUEUE_VARIANTS)"
            :key="v.id"
            class="sq-variant-pill"
            :class="{ 'sq-variant-pill--active': variant === v.id }"
            :title="v.note"
            @click="setVariant(v.id)"
          >{{ v.label }}</button>
          <span class="sq-variant-note">{{ variantNote }}</span>
        </div>

        <!-- Vùng vẽ: banner + ô cells -->
        <div class="sq-canvas-body">
          <!-- Banner kết quả / lỗi -->
          <transition name="sq-banner">
            <div v-if="resultMsg" class="sq-result-banner" :class="resultOk ? 'sq-result-ok' : 'sq-result-error'">
              {{ resultMsg }}
            </div>
          </transition>

          <!-- Ô cells — trung tâm màn hình -->
          <div class="sq-cells-wrap">
            <div class="sq-rows">
              <div class="sq-row-main">
                <div v-for="(cell, idx) in cells" :key="idx" class="sq-cell-col">
                  <div class="sq-pointer-row">
                    <span
                      v-for="(lbl, li) in currentPointers[idx] ?? []"
                      :key="li"
                      class="sq-pointer"
                      :class="pointerClass(lbl)"
                    >{{ lbl }} ↓</span>
                  </div>
                  <div class="sq-cell" :class="getCellClass(cell)" :title="cell.val != null ? `giá trị ${cell.val}` : 'ô trống'">
                    {{ cell.val ?? '—' }}
                  </div>
                  <div class="sq-cell-index">{{ idx === 0 ? 'đầu' : idx === capacity - 1 ? 'cuối' : `[${idx}]` }}</div>
                </div>
              </div>
              <!-- Hàng phụ song song (MIN/MAX của Min/Max Stack) -->
              <div v-for="row in currentExtraRows" :key="row.label" class="sq-row-extra">
                <span class="sq-row-label">{{ row.label }}</span>
                <div class="sq-cell sq-cell--mini" v-for="(cell, idx) in row.cells" :key="idx" :class="getCellClass(cell)">
                  {{ cell.val ?? '—' }}
                </div>
              </div>
              <div v-if="!cells.length" class="sq-empty sq-canvas-empty">Chưa có dữ liệu — chọn chuỗi mẫu hoặc bấm thao tác.</div>
            </div>
          </div>
        </div>

        <!-- Mô tả bước hiện tại -->
        <div class="sq-step-strip">
          <div class="sq-step-label-row">
            <span class="sq-step-label">{{ currentLabel }}</span>
            <span v-for="(v, k) in currentIndices" :key="k" class="sq-idx-chip" :class="{ 'sq-idx-chip--warn': k === 'wrap' }">{{ k }}={{ v }}</span>
          </div>
          <span class="sq-step-desc">{{ currentDesc }}</span>
        </div>

        <!-- VCR -->
        <div class="sq-vcr">
          <select class="sq-vcr-speed" v-model.number="playbackSpeed" title="Tốc độ">
            <option :value="0.5">0.5x</option>
            <option :value="1">1x</option>
            <option :value="2">2x</option>
          </select>
          <button class="sq-vcr-btn" title="Lùi 1 bước" @click="stepSearch(-1)">◀</button>
          <button class="sq-vcr-play" :title="playing ? 'Tạm dừng' : 'Phát'" @click="togglePlay">{{ playing ? '⏸' : '▶' }}</button>
          <button class="sq-vcr-btn" title="Tiến 1 bước" @click="stepSearch(1)">▶|</button>
          <div class="sq-vcr-bar" @click="seekFromClick">
            <div class="sq-vcr-fill" :style="{ width: progressPct + '%' }"></div>
          </div>
          <span class="sq-vcr-count">{{ stepIdx + 1 }} / {{ steps.length || 1 }}</span>
          <button class="sq-vcr-reset" title="Làm lại từ đầu" @click="resetPlay">↺</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onUnmounted } from 'vue';
import BaseIcon from '../../shared/components/BaseIcon.vue';
import {
  type DsMode,
  type DsOp,
  type DsStep,
  generateDsSteps,
  generateRandomOps,
} from './stackQueueEngine';

// ── State: 2 cấu trúc GỐC + biến thể con (giống Sorting Sandbox: gốc → pills) ──
const ROOTS: Array<{ id: 'stack' | 'queue'; label: string; hint: string; icon: string; title: string }> = [
  { id: 'stack', label: 'Stack', hint: 'LIFO', icon: 'stack', title: 'Cấu trúc gốc — ngăn xếp, vào sau ra trước' },
  { id: 'queue', label: 'Queue', hint: 'FIFO', icon: 'queue', title: 'Cấu trúc gốc — hàng đợi, vào trước ra trước' },
];

const QUEUE_VARIANTS: Array<{ id: 'fifo' | 'circular' | 'deque'; label: string; note: string }> = [
  { id: 'fifo', label: 'FIFO', note: 'Cơ bản — dequeue dồn các phần tử lên đầu (tốn O(n)).' },
  { id: 'circular', label: 'Tròn', note: 'Front/rear quay vòng, dequeue O(1) — tái sử dụng ô trống.' },
  { id: 'deque', label: 'Deque', note: 'Hàng đợi 2 đầu — thêm/bớt được ở cả front lẫn rear.' },
];

const STACK_VARIANTS: Array<{ id: 'basic' | 'minmax'; label: string; note: string }> = [
  { id: 'basic', label: 'Cơ bản', note: 'Stack LIFO — Push / Pop / Peek trực tiếp.' },
  { id: 'minmax', label: 'Min/Max', note: '3 ngăn xếp song song (s, min, max) — getMin()/getMax() O(1).' },
];

type RootId = 'stack' | 'queue';
type VariantId = 'basic' | 'minmax' | 'fifo' | 'circular' | 'deque';

const root = ref<RootId>('stack');
const variant = ref<VariantId>('basic');
const capacity = ref(6);
const valueInput = ref<number>(42);
// Khởi tạo với chuỗi mẫu để canvas không trống khi vào tab
const ops = ref<DsOp[]>([
  { kind: 'push', value: 5 },
  { kind: 'push', value: 3 },
  { kind: 'push', value: 7 },
  { kind: 'pop' },
  { kind: 'peek' },
  { kind: 'pop' },
]);
const steps = ref<DsStep[]>([]);
const stepIdx = ref(0);
const playing = ref(false);
const playbackSpeed = ref(1);
const currentPreset = ref<string | null>('basic');
const logOpen = ref(true);

let timer: ReturnType<typeof setInterval> | null = null;

// Chế độ thực tế: root=stack → 'stack' (cơ bản) hoặc 'minmax'; root=queue → biến thể đang chọn
const effectiveMode = computed<DsMode>(() => {
  if (root.value === 'stack') return variant.value === 'minmax' ? 'minmax' : 'stack';
  if (variant.value === 'circular') return 'circular';
  if (variant.value === 'deque') return 'deque';
  return 'queue';
});

const variantNote = computed(() => {
  if (root.value === 'stack') {
    return STACK_VARIANTS.find((v) => v.id === variant.value)?.note ?? '';
  }
  return QUEUE_VARIANTS.find((v) => v.id === variant.value)?.note ?? '';
});

// ── Nhãn nút thao tác theo chế độ ──
const opsTitle = computed(() => {
  switch (effectiveMode.value) {
    case 'stack': return 'Thao tác (LIFO)';
    case 'minmax': return 'Thao tác (LIFO + Min/Max)';
    case 'queue': return 'Thao tác (FIFO)';
    case 'circular': return 'Thao tác (quay vòng)';
    case 'deque': return 'Thao tác (2 đầu)';
  }
});
const pushBtnLabel = computed(() => (effectiveMode.value === 'stack' || effectiveMode.value === 'minmax' ? 'Push' : 'Enqueue'));
const popBtnLabel = computed(() => (effectiveMode.value === 'stack' || effectiveMode.value === 'minmax' ? 'Pop' : 'Dequeue'));

// ── Chạy lại toàn bộ steps khi ops/mode/capacity đổi ──
function rebuild(): void {
  steps.value = generateDsSteps(effectiveMode.value, ops.value, capacity.value);
  stopPlay();
  stepIdx.value = 0;
}

rebuild();

// Kéo slider dung lượng → sinh lại steps với capacity mới
watch(capacity, () => {
  currentPreset.value = null;
  rebuild();
});

function setRoot(r: RootId): void {
  if (root.value === r) return;
  root.value = r;
  variant.value = r === 'stack' ? 'basic' : 'fifo';
  currentPreset.value = null;
  ops.value = [];
  rebuild();
}

function setVariant(v: VariantId): void {
  if (variant.value === v) return;
  variant.value = v;
  currentPreset.value = null;
  ops.value = [];
  rebuild();
}

function addOp(op: DsOp): void {
  if (op.kind === 'push' || op.kind === 'pushFront') op = { ...op, value: valueInput.value };
  ops.value = [...ops.value, op];
  currentPreset.value = null;
  rebuild();
}

function clearOps(): void {
  ops.value = [];
  currentPreset.value = null;
  rebuild();
}

function runRandom(): void {
  ops.value = generateRandomOps(effectiveMode.value, capacity.value);
  currentPreset.value = 'random';
  rebuild();
}

// ── Presets theo chế độ ──
const presets = computed(() => {
  const cap = capacity.value;
  const fill = (): DsOp[] => Array.from({ length: cap }, (_, i) => ({ kind: 'push' as const, value: i + 1 }));
  switch (effectiveMode.value) {
    case 'stack':
      return [
        { key: 'basic', label: 'Cơ bản', ops: () => [
          { kind: 'push', value: 5 },
          { kind: 'push', value: 3 },
          { kind: 'push', value: 7 },
          { kind: 'pop' },
          { kind: 'peek' },
          { kind: 'pop' },
        ] as DsOp[] },
        { key: 'overflow', label: 'Tràn', ops: () => [...fill(), { kind: 'push', value: 99 }] as DsOp[] },
        { key: 'empty', label: 'Rỗng', ops: () => [
          { kind: 'pop' },
          { kind: 'peek' },
          { kind: 'pop' },
        ] as DsOp[] },
      ];
    case 'minmax':
      return [
        { key: 'basic', label: 'Cơ bản', ops: () => [
          { kind: 'push', value: 5 },
          { kind: 'push', value: 3 },
          { kind: 'push', value: 8 },
          { kind: 'push', value: 2 },
          { kind: 'peek' },
          { kind: 'pop' },
          { kind: 'peek' },
        ] as DsOp[] },
        { key: 'wave', label: 'Nhấp nhô', ops: () => [
          { kind: 'push', value: 9 },
          { kind: 'push', value: 1 },
          { kind: 'push', value: 5 },
          { kind: 'push', value: 3 },
          { kind: 'pop' },
          { kind: 'push', value: 7 },
          { kind: 'peek' },
        ] as DsOp[] },
        { key: 'overflow', label: 'Tràn', ops: () => [...fill(), { kind: 'push', value: 99 }] as DsOp[] },
        { key: 'empty', label: 'Rỗng', ops: () => [{ kind: 'pop' }, { kind: 'peek' }] as DsOp[] },
      ];
    case 'queue':
      return [
        { key: 'fifo', label: 'FIFO', ops: () => [
          { kind: 'push', value: 10 },
          { kind: 'push', value: 20 },
          { kind: 'push', value: 30 },
          { kind: 'pop' },
          { kind: 'push', value: 40 },
          { kind: 'pop' },
        ] as DsOp[] },
        { key: 'overflow', label: 'Tràn', ops: () => [...fill(), { kind: 'push', value: 99 }] as DsOp[] },
        { key: 'empty', label: 'Rỗng', ops: () => [{ kind: 'pop' }, { kind: 'pop' }] as DsOp[] },
      ];
    case 'circular':
      return [
        { key: 'basic', label: 'Cơ bản', ops: () => [
          { kind: 'push', value: 10 },
          { kind: 'push', value: 20 },
          { kind: 'push', value: 30 },
          { kind: 'pop' },
          { kind: 'pop' },
          { kind: 'push', value: 40 },
        ] as DsOp[] },
        { key: 'wrap', label: 'Quay vòng', ops: () => [
          ...fill(),
          { kind: 'pop' },
          { kind: 'pop' },
          { kind: 'push', value: 11 },
          { kind: 'push', value: 12 },
        ] as DsOp[] },
        { key: 'overflow', label: 'Tràn', ops: () => [...fill(), { kind: 'push', value: 99 }] as DsOp[] },
        { key: 'empty', label: 'Rỗng', ops: () => [{ kind: 'pop' }, { kind: 'pop' }] as DsOp[] },
      ];
    case 'deque':
      return [
        { key: 'basic', label: 'Cơ bản', ops: () => [
          { kind: 'push', value: 5 },
          { kind: 'push', value: 9 },
          { kind: 'pushFront', value: 1 },
          { kind: 'pop' },
          { kind: 'popFront' },
          { kind: 'push', value: 7 },
        ] as DsOp[] },
        { key: 'full', label: 'Đầy', ops: () => [...fill(), { kind: 'pushFront', value: 99 }] as DsOp[] },
        { key: 'empty', label: 'Rỗng', ops: () => [{ kind: 'pop' }, { kind: 'popFront' }] as DsOp[] },
      ];
  }
});

function loadPreset(p: { key: string; label: string; ops: () => DsOp[] }): void {
  ops.value = p.ops();
  currentPreset.value = p.key;
  rebuild();
}

function opLabel(op: DsOp): string {
  if (op.kind === 'pushFront') return `T+${op.value}`;
  if (op.kind === 'popFront') return 'T-';
  if (op.kind === 'push') return (effectiveMode.value === 'stack' || effectiveMode.value === 'minmax') ? `P ${op.value}` : `E ${op.value}`;
  if (op.kind === 'pop') return (effectiveMode.value === 'stack' || effectiveMode.value === 'minmax') ? 'POP' : 'DEQ';
  return 'PEEK';
}

// ── Display ──
const cells = computed(() => steps.value.length ? steps.value[stepIdx.value].cells : []);
const currentExtraRows = computed(() => steps.value.length ? steps.value[stepIdx.value].extraRows ?? [] : []);
const currentPointers = computed(() => steps.value.length ? steps.value[stepIdx.value].pointers : {});
const currentIndices = computed(() => steps.value.length ? steps.value[stepIdx.value].indices : {});
const currentLabel = computed(() => steps.value.length ? steps.value[stepIdx.value].label : '—');
const currentDesc = computed(() => steps.value.length ? steps.value[stepIdx.value].log : 'Chọn chuỗi mẫu hoặc bấm thao tác để bắt đầu.');
const progressPct = computed(() => steps.value.length > 1 ? (stepIdx.value / (steps.value.length - 1)) * 100 : 0);

const logs = computed(() => {
  if (!steps.value.length) return [];
  return steps.value.slice(0, stepIdx.value + 1).map((s) => ({ text: `[${s.stepIndex + 1}] ${s.label}: ${s.log}`, ok: s.ok }));
});

const resultMsg = computed(() => {
  const s = steps.value[stepIdx.value];
  if (!s) return '';
  if (!s.ok) return `⚠ ${s.log}`;
  if (s.isFinal) return `🏁 ${s.log}`;
  return '';
});
const resultOk = computed(() => steps.value[stepIdx.value]?.ok !== false);

function getCellClass(cell: { val: number | null; state: string }) {
  return {
    'sq-cell--idle':      cell.state === 'idle',
    'sq-cell--filled':    cell.state === 'filled',
    'sq-cell--active':    cell.state === 'active',
    'sq-cell--inserted':  cell.state === 'inserted',
    'sq-cell--removed':   cell.state === 'removed',
    'sq-cell--highlight': cell.state === 'highlight',
  };
}

function pointerClass(lbl: string) {
  if (lbl === 'TOP') return 'sq-ptr-top';
  if (lbl === 'FRONT') return 'sq-ptr-front';
  if (lbl === 'REAR') return 'sq-ptr-rear';
  return 'sq-ptr-top';
}

// ── VCR ──
function stepSearch(dir: 1 | -1): void {
  stepIdx.value = Math.max(0, Math.min(steps.value.length - 1, stepIdx.value + dir));
}
function togglePlay(): void {
  if (playing.value) { stopPlay(); return; }
  if (!steps.value.length) { addOp({ kind: 'push', value: valueInput.value }); return; }
  playing.value = true;
  timer = setInterval(() => {
    if (stepIdx.value >= steps.value.length - 1) { stopPlay(); return; }
    stepIdx.value++;
  }, 650 / playbackSpeed.value);
}
function stopPlay(): void { playing.value = false; if (timer) { clearInterval(timer); timer = null; } }
function resetPlay(): void { stopPlay(); stepIdx.value = 0; }
function seekFromClick(e: MouseEvent): void {
  if (steps.value.length < 2) return;
  const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
  const ratio = Math.min(1, Math.max(0, (e.clientX - rect.left) / rect.width));
  stepIdx.value = Math.round(ratio * (steps.value.length - 1));
}

onUnmounted(stopPlay);
</script>

<style scoped>
/* Sky/Ocean blue palette — intentionally different from brand purple.
   Each sandbox has its own color identity for visual context switching.
   Sorting → Purple | Graph → Cyan/Emerald | Stack/Queue → Sky blue */

/* ── Root ── */
.sq-root {
  display: flex; flex-direction: column; height: 100%; width: 100%;
  padding: 6px; gap: 6px;
  background: var(--color-bg-primary);
  font-family: var(--font-sans);
}

/* ── Top bar ── */
.sq-topbar {
  display: flex; align-items: center; justify-content: space-between; gap: 12px;
  padding: 6px 12px;
  background: var(--color-bg-surface);
  border: 1px solid var(--color-border-default);
  border-radius: 10px;
  backdrop-filter: blur(12px);
}
.sq-topbar-left { display: flex; align-items: center; gap: 10px; }
.sq-badge {
  display: flex; align-items: center; gap: 6px;
  font-size: 11px; font-weight: 800; color: #7dd3fc;
  background: rgba(14,165,233,.12); padding: 4px 12px;
  border-radius: 999px; border: 1px solid rgba(14,165,233,.35);
}
.sq-badge-dot { width:7px; height:7px; border-radius:50%; background:#0ea5e9; animation: pulse 1.5s infinite; }
.sq-tag { font-size:9px; font-weight:700; color:#64748b; border:1px solid #334155; border-radius:4px; padding:2px 6px; letter-spacing:.08em; }
.sq-fps  { font-size:10px; font-weight:700; color:#0ea5e9; font-family: var(--font-mono); letter-spacing:.05em; }

/* Mode toggle (top bar) */
.sq-mode-toggle { display: flex; gap: 4px; background: rgba(0,0,0,.2); border-radius: 8px; padding: 3px; }
.sq-mode-btn {
  display: flex; align-items: center; gap: 6px;
  padding: 4px 14px; border-radius: 6px; font-size: 11px; font-weight: 800;
  cursor: pointer; border: 1px solid transparent;
  background: transparent; color:#94a3b8; transition: all .15s;
}
.sq-mode-btn:hover { color:#7dd3fc; }
.sq-mode-btn--active {
  background: linear-gradient(135deg, rgba(14,165,233,.25), rgba(2,132,199,.2));
  color:#bae6fd; border-color: rgba(14,165,233,.55);
  box-shadow: 0 0 12px rgba(14,165,233,.3);
}
.sq-mode-hint { font-size:8px; font-weight:700; color:#64748b; letter-spacing:.05em; }
.sq-mode-btn--active .sq-mode-hint { color:#7dd3fc; }

/* ── Body ── */
.sq-body { display: flex; flex: 1; min-height: 0; gap: 6px; }

/* ── Left panel ── */
.sq-panel {
  width: 230px; flex-shrink: 0; display: flex; flex-direction: column; gap: 10px;
  background: var(--color-bg-surface);
  border: 1px solid rgba(14,165,233,.15);
  border-radius: 10px; padding: 12px; overflow-y: auto;
}
.sq-section { display: flex; flex-direction: column; gap: 6px; }
.sq-section-title { font-size:9px; font-weight:800; text-transform:uppercase; letter-spacing:.08em; color:#64748b; margin:0; }
.sq-cap-row { display:flex; align-items:center; justify-content:space-between; }
.sq-cap-val { font-size:11px; font-weight:800; color:#0ea5e9; }

.sq-add-row { display: flex; gap: 6px; }
.sq-value-input {
  flex: 1; min-width: 0; background:rgba(255,255,255,.05); border:1px solid rgba(14,165,233,.35);
  border-radius:8px; padding:7px 10px; font-size:13px; font-weight:700; color:#fff; outline:none;
}
.sq-value-input:focus { border-color:#0ea5e9; box-shadow:0 0 10px rgba(14,165,233,.3); }
.sq-btn-add {
  padding:7px 14px; border-radius:8px; font-size:12px; font-weight:800;
  display:flex; align-items:center; gap:5px; white-space:nowrap;
  background: linear-gradient(135deg,#0284c7,#0369a1); color:#fff;
  border:none; cursor:pointer; transition:all .15s;
}
.sq-btn-add:hover { filter:brightness(1.2); transform:translateY(-1px); box-shadow:0 4px 14px rgba(14,165,233,.4); }
.sq-btn-grid { display: flex; gap: 6px; }
.sq-btn {
  flex: 1; display:flex; align-items:center; justify-content:center; gap:5px;
  padding:7px 8px; border-radius:8px; font-size:11px; font-weight:700;
  cursor:pointer; border:1px solid rgba(100,116,139,.3);
  background: rgba(255,255,255,.03); color:#94a3b8; transition: all .15s;
}
.sq-btn:hover { background:rgba(14,165,233,.12); color:#38bdf8; border-color:rgba(14,165,233,.4); transform:translateY(-1px); }

.sq-mode-note {
  font-size: 9px; color:#0e7490; background: rgba(14,165,233,.08);
  border: 1px solid rgba(14,165,233,.2); border-radius: 6px; padding: 5px 8px; line-height: 1.45; margin: 0;
}

.sq-slider { width:100%; accent-color:#0ea5e9; cursor:pointer; }

.sq-pills { display: flex; flex-wrap: wrap; gap: 5px; }
.sq-pill {
  padding: 5px 12px; border-radius: 999px; font-size: 10px; font-weight: 800;
  border: 1px solid rgba(14,165,233,.25); color:#7dd3fc; background: transparent; cursor: pointer; transition: all .15s;
}
.sq-pill:hover { background:rgba(14,165,233,.12); }
.sq-pill--active { background: rgba(14,165,233,.2); border-color: rgba(14,165,233,.6); box-shadow: 0 0 10px rgba(14,165,233,.3); }

.sq-ops-list { display:flex; flex-wrap:wrap; gap:4px; max-height: 64px; overflow-y:auto; }
.sq-op-chip {
  padding: 3px 8px; border-radius: 5px; font-size: 9px; font-weight: 800;
  font-family: var(--font-mono); color: #67e8f9; background: rgba(14,165,233,.12);
  border: 1px solid rgba(14,165,233,.3);
}
.sq-op-chip--pop { color:#fca5a5; background:rgba(239,68,68,.12); border-color:rgba(239,68,68,.3); }

/* Nhật ký */
.sq-log-section { border-top: 1px solid rgba(255,255,255,.06); padding-top: 8px; flex: 1; min-height: 0; }
.sq-log-toggle {
  display:flex; align-items:center; justify-content:space-between; width:100%;
  background:none; border:none; cursor:pointer; color:#94a3b8;
  font-size:9px; font-weight:800; text-transform:uppercase; letter-spacing:.08em; padding:0;
}
.sq-log-toggle:hover { color:#38bdf8; }
.sq-caret { font-size:10px; }
.sq-log-box { display:flex; flex-direction:column; gap:2px; overflow-y:auto; flex:1; min-height: 0; padding-right: 2px; }
.sq-log-entry { display:flex; align-items:flex-start; gap:5px; font-size:10px; color:#94a3b8; line-height:1.5; }
.sq-log-entry--error { color:#fca5a5; }
.sq-log-bullet { width:4px; height:4px; border-radius:50%; background:#0ea5e9; margin-top:6px; flex-shrink:0; }
.sq-log-bullet--error { background:#ef4444; }
.sq-log-text { word-break: break-word; }
.sq-empty { font-size:10px; color:#475569; font-style:italic; }

/* ── Canvas ── */
.sq-canvas {
  flex:1; position:relative; display:flex; flex-direction:column;
  background:
    linear-gradient(rgba(255,255,255,.03) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255,255,255,.03) 1px, transparent 1px),
    radial-gradient(ellipse at 50% 35%, rgba(14,165,233,.07) 0%, transparent 60%),
    var(--color-bg-primary);
  background-size: 24px 24px, 24px 24px, 100% 100%, 100% 100%;
  background-position: center center;
  border: 1px solid rgba(14,165,233,.12);
  border-radius: 10px; overflow:hidden;
}

/* Thanh biến thể — KHUNG bo tròn giống `.algorithm-controls` của Sorting Sandbox:
   nền mờ + viền + backdrop-blur + rounded */
.sq-variant-box {
  display: flex; align-items: center; gap: 6px; flex-wrap: wrap;
  margin: 8px 10px 0;
  padding: 6px 10px;
  border-radius: 8px;
  background-color: color-mix(in srgb, var(--color-bg-secondary) 80%, transparent);
  border: 1px solid color-mix(in srgb, var(--color-border-default) 70%, transparent);
  backdrop-filter: blur(12px);
  flex-shrink: 0;
}
.sq-variant-title {
  font-size: 9px; font-weight: 800; text-transform: uppercase; letter-spacing: .08em;
  color: #64748b; margin-right: 2px;
}
.sq-variant-pill {
  padding: 4px 14px; border-radius: 6px; font-size: 11px; font-weight: 800;
  border: 1px solid color-mix(in srgb, var(--color-border-default) 60%, transparent);
  background-color: var(--color-bg-primary); color: var(--color-text-muted);
  cursor: pointer; transition: all .2s;
}
.sq-variant-pill:hover {
  color: var(--color-text-secondary);
  border-color: color-mix(in srgb, var(--color-border-default) 90%, transparent);
  background-color: rgba(14,165,233,.08);
}
.sq-variant-pill--active {
  background: linear-gradient(135deg, rgba(14,165,233,.28), rgba(2,132,199,.22));
  color: #bae6fd; border-color: rgba(14,165,233,.6);
  box-shadow: 0 0 10px rgba(14,165,233,.3);
}
.sq-variant-note {
  margin-left: auto; font-size: 9px; color: #0e7490;
  max-width: 45%; text-align: right; line-height: 1.4;
}

/* Vùng vẽ (banner + cells) */
.sq-canvas-body { flex: 1; min-height: 0; position: relative; display: flex; flex-direction: column; }

/* Banner */
.sq-result-banner {
  position:absolute; top:14px; left:50%; transform:translateX(-50%);
  padding:6px 20px; border-radius:999px; font-size:12px; font-weight:800;
  letter-spacing:.03em; z-index:10; max-width: 90%; text-align:center;
}
.sq-result-ok    { background:rgba(14,165,233,.15); color:#7dd3fc; border:1px solid rgba(14,165,233,.4); box-shadow:0 0 24px rgba(14,165,233,.25); }
.sq-result-error { background:rgba(239,68,68,.12); color:#f87171; border:1px solid rgba(239,68,68,.3); box-shadow:0 0 20px rgba(239,68,68,.2); }
.sq-banner-enter-active, .sq-banner-leave-active { transition: opacity .2s, transform .2s; }
.sq-banner-enter-from, .sq-banner-leave-to { opacity:0; transform:translateX(-50%) translateY(-6px); }

/* Cells — trung tâm */
.sq-cells-wrap {
  flex: 1; display:flex; align-items:center; justify-content:center;
  gap:12px; padding: 40px 20px 10px; flex-wrap:wrap; position:relative;
  overflow-y: auto;
}
.sq-rows { display:flex; flex-direction:column; align-items:center; gap:10px; }
.sq-row-main { display:flex; align-items:flex-end; justify-content:center; gap:12px; flex-wrap:wrap; }
.sq-row-extra {
  display:flex; align-items:center; justify-content:center; gap:6px; flex-wrap:wrap;
  background: rgba(8,20,33,.55); border: 1px solid rgba(14,165,233,.12);
  border-radius: 10px; padding: 5px 10px;
}
.sq-row-label {
  font-size: 8px; font-weight: 900; letter-spacing: .08em; color:#7dd3fc;
  background: rgba(14,165,233,.15); border: 1px solid rgba(14,165,233,.35);
  padding: 2px 6px; border-radius: 4px; margin-right: 4px;
}
.sq-cell--mini {
  width: 36px; height: 30px; border-radius: 6px; font-size: 11px; min-width: 0;
}
.sq-cell-col  { display:flex; flex-direction:column; align-items:center; gap:7px; }
.sq-pointer-row { height:20px; display:flex; align-items:flex-end; justify-content:center; gap:3px; }
.sq-pointer {
  font-size:8px; font-weight:800; letter-spacing:.05em;
  padding:1px 5px; border-radius:4px; white-space:nowrap; animation: bounce 1s infinite;
}
.sq-ptr-top   { color:#7dd3fc; background:rgba(14,165,233,.15); border:1px solid rgba(14,165,233,.4); }
.sq-ptr-front { color:#4ade80; background:rgba(74,222,128,.12); border:1px solid rgba(74,222,128,.35); }
.sq-ptr-rear  { color:#fbbf24; background:rgba(251,191,36,.12); border:1px solid rgba(251,191,36,.35); }

.sq-cell {
  width:72px; height:72px; border-radius:12px; display:flex; align-items:center; justify-content:center;
  font-size:20px; font-weight:800; font-family: var(--font-mono);
  border:1px solid rgba(100,116,139,.2); transition:all .35s cubic-bezier(.4,0,.2,1);
}
.sq-cell--idle       { background:rgba(15,23,42,.5); color:#475569; border-style:dashed; border-color:rgba(51,65,85,.5); }
.sq-cell--filled     { background:rgba(14,165,233,.15); color:#7dd3fc; border-color:rgba(14,165,233,.5); box-shadow:0 0 14px rgba(14,165,233,.25); }
.sq-cell--active     { background:rgba(56,189,248,.2); color:#e0f2fe; border-color:rgba(56,189,248,.7); box-shadow:0 0 20px rgba(14,165,233,.4); transform:scale(1.05); }
.sq-cell--inserted   { background:rgba(14,165,233,.28); color:#bae6fd; border-color:#0ea5e9; box-shadow:0 0 26px rgba(14,165,233,.6), 0 0 8px rgba(14,165,233,.4) inset; transform:scale(1.12); }
.sq-cell--removed    { background:rgba(239,68,68,.18); color:#fca5a5; border-color:rgba(239,68,68,.7); box-shadow:0 0 20px rgba(239,68,68,.4); transform:scale(.92); opacity:.8; }
.sq-cell--highlight  { background:rgba(251,191,36,.18); color:#fcd34d; border-color:rgba(251,191,36,.6); box-shadow:0 0 20px rgba(251,191,36,.35); transform:scale(1.06); }
.sq-cell-index       { font-size:9px; color:#64748b; font-family: var(--font-mono); }
.sq-canvas-empty { position:absolute; inset:0; display:flex; align-items:center; justify-content:center; }

/* Step strip — mô tả bước hiện tại (dưới cells, trên VCR) */
.sq-step-strip {
  display:flex; flex-direction: column; align-items:center; gap:3px;
  margin: 0 12px 8px; padding: 8px 16px;
  background: rgba(8,20,33,.75); backdrop-filter: blur(8px);
  border: 1px solid rgba(14,165,233,.2); border-radius: 10px;
}
.sq-step-label-row { display:flex; align-items:center; gap:6px; flex-wrap:wrap; justify-content:center; }
.sq-step-label { font-size: 11px; font-weight: 900; color:#7dd3fc; letter-spacing:.04em; }
.sq-idx-chip {
  font-size: 9px; font-weight: 800; font-family: var(--font-mono);
  color: #67e8f9; background: rgba(14,165,233,.12);
  border: 1px solid rgba(14,165,233,.3); border-radius: 4px; padding: 1px 6px;
}
.sq-idx-chip--warn { color: #fcd34d; background: rgba(251,191,36,.12); border-color: rgba(251,191,36,.4); }
.sq-step-desc { font-size: 10px; color:#94a3b8; text-align:center; line-height:1.4; }

/* VCR */
.sq-vcr {
  display:flex; align-items:center; gap:10px;
  margin: 0 12px 10px; padding: 8px 16px;
  background:rgba(8,20,33,.88); backdrop-filter:blur(14px);
  border:1px solid rgba(14,165,233,.2); border-radius:999px;
  box-shadow:0 4px 28px rgba(0,0,0,.6);
}
.sq-vcr-speed {
  background:rgba(255,255,255,.06); border:1px solid rgba(100,116,139,.3);
  border-radius:6px; color:#94a3b8; font-size:11px; font-weight:700; padding:2px 6px; cursor:pointer;
}
.sq-vcr-btn {
  width:28px; height:28px; border-radius:50%; display:flex; align-items:center; justify-content:center;
  background:rgba(255,255,255,.06); border:1px solid rgba(100,116,139,.3);
  color:#94a3b8; cursor:pointer; font-size:11px; transition:all .15s;
}
.sq-vcr-btn:hover { background:rgba(14,165,233,.15); color:#7dd3fc; border-color:rgba(14,165,233,.4); }
.sq-vcr-play {
  width:36px; height:36px; border-radius:50%; display:flex; align-items:center; justify-content:center;
  background:linear-gradient(135deg,#0284c7,#075985); color:#fff; font-size:15px;
  border:none; cursor:pointer; box-shadow:0 0 16px rgba(14,165,233,.5);
  transition:all .15s;
}
.sq-vcr-play:hover { filter:brightness(1.2); transform:scale(1.06); }
.sq-vcr-bar {
  flex:1; min-width: 80px; height:5px; background:rgba(255,255,255,.08);
  border-radius:999px; overflow:hidden; cursor:pointer;
}
.sq-vcr-fill { height:100%; background:linear-gradient(90deg,#0284c7,#38bdf8); border-radius:999px; transition:width .35s; }
.sq-vcr-count { font-size:10px; font-weight:700; color:#64748b; font-family: var(--font-mono); white-space:nowrap; }
.sq-vcr-reset {
  width:26px; height:26px; border-radius:50%; display:flex; align-items:center; justify-content:center;
  background:rgba(255,255,255,.06); border:1px solid rgba(100,116,139,.3);
  color:#94a3b8; cursor:pointer; font-size:12px; transition:all .15s;
}
.sq-vcr-reset:hover { color:#7dd3fc; border-color:rgba(14,165,233,.4); }

/* Animations */
@keyframes pulse  { 0%,100%{opacity:1}50%{opacity:.4} }
@keyframes bounce { 0%,100%{transform:translateY(0)}50%{transform:translateY(-4px)} }
</style>