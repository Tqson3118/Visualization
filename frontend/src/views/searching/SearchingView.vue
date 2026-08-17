<template>
  <div class="sv-root">

    <!-- ─── TOP BAR ─── -->
    <div class="sv-topbar">
      <div class="sv-topbar-left">
        <div class="sv-badge">
          <span class="sv-badge-dot"></span>
          Searching Sandbox
        </div>
        <div class="sv-tag">INTERACTIVE</div>
      </div>

      <div class="sv-topbar-right">
        <span class="sv-fps">● VISUALGO-MODE 60FPS</span>
      </div>
    </div>

    <!-- ─── BODY ─── -->
    <div class="sv-body">

      <!-- LEFT PANEL -->
      <aside class="sv-aside">
        <section class="sv-aside-section">
          <p class="sv-aside-title">Khởi tạo</p>
          <button class="sv-btn-ghost" @click="initRandom">Ngẫu nhiên</button>
          <button class="sv-btn-ghost sv-btn-sorted" @click="initSorted">Đã sắp xếp</button>
          <button class="sv-btn-ghost sv-btn-rev" @click="initReverse">Đảo ngược</button>
        </section>

        <section class="sv-aside-section">
          <p class="sv-aside-title">Kích thước N</p>
          <input type="range" min="3" max="24" v-model="n" class="sv-slider" />
          <span class="sv-slider-val">{{ n }}</span>
        </section>

        <section v-if="activeAlgo !== 'sliding'" class="sv-aside-section">
          <p class="sv-aside-title">
            {{ activeAlgo === 'two-ptr' ? 'Tổng cần tìm (Sum)' : 'Mục tiêu (Target)' }}
          </p>
          <div v-if="activeAlgo === 'two-ptr'" class="sv-hint">
            Tìm 2 số trong mảng có tổng = target
          </div>
          <div class="sv-target-wrap">
            <input type="number" v-model="target" placeholder="Nhập số…" class="sv-target-input" />
            <button class="sv-btn-search" @click="startSearch">🔍 Tìm</button>
          </div>
        </section>

        <section v-if="activeAlgo === 'sliding'" class="sv-aside-section">
          <p class="sv-aside-title">Window size K</p>
          <input type="range" min="2" :max="Math.min(6, n)" v-model="windowK" class="sv-slider" />
          <span class="sv-slider-val">{{ windowK }}</span>
          <div class="sv-hint">Tính tổng từng cửa sổ K phần tử liên tiếp</div>
          <button class="sv-btn-search" style="margin-top:4px" @click="startSearch">▶ Chạy</button>
        </section>

        <!-- Log -->
        <section class="sv-aside-section sv-log-box">
          <p class="sv-aside-title">Nhật ký</p>
          <div class="sv-log-entry" v-for="(msg, i) in logs" :key="i">
            <span class="sv-log-bullet"></span>{{ msg }}
          </div>
          <div v-if="!logs.length" class="sv-log-empty">Nhấn Tìm để bắt đầu…</div>
        </section>
      </aside>

      <!-- CANVAS -->
      <div class="sv-canvas">

        <!-- Result banner -->
        <div v-if="resultMsg" class="sv-result-banner" :class="found ? 'sv-result-found' : 'sv-result-notfound'">
          {{ resultMsg }}
        </div>

        <!-- Array cells -->
        <div class="sv-cells-wrap">
          <div v-for="(cell, idx) in cells" :key="idx" class="sv-cell-col">

            <!-- Pointer labels -->
            <div class="sv-pointer-row">
              <span v-if="getPointers(idx).length" class="sv-pointer" :class="getPointerClass(idx)">
                {{ getPointerLabel(idx) }} ↓
              </span>
            </div>

            <!-- Window highlight (Sliding Window) -->
            <div class="sv-cell-outer" :class="getWindowClass(idx)">
              <div
                class="sv-cell"
                :class="getCellClass(cell, idx)"
              >
                {{ cell.val }}
              </div>
            </div>

            <div class="sv-cell-index">[{{ idx }}]</div>
          </div>
        </div>

        <!-- VCR bar -->
        <div class="sv-vcr">
          <select class="sv-vcr-speed">
            <option>0.5x</option>
            <option selected>1x</option>
            <option>2x</option>
          </select>
          <button class="sv-vcr-btn" @click="stepSearch(-1)">◀</button>
          <button class="sv-vcr-play" @click="togglePlay">{{ playing ? '⏸' : '▶' }}</button>
          <button class="sv-vcr-btn" @click="stepSearch(1)">▶|</button>
          <div class="sv-vcr-bar">
            <div class="sv-vcr-fill" :style="{ width: progressPct + '%' }"></div>
          </div>
          <span class="sv-vcr-count">{{ stepIdx + 1 }} / {{ steps.length || 1 }}</span>
        </div>

        <!-- Algo pills bottom -->
        <div class="sv-algo-pills-bottom">
          <button
            v-for="a in ALGOS" :key="a.id"
            class="sv-pill" :class="{ 'sv-pill--active': activeAlgo === a.id }"
            @click="activeAlgo = a.id"
          >{{ a.label }}</button>
        </div>

      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onUnmounted } from 'vue';

// ── Types ──
type AlgoId = 'linear' | 'binary' | 'two-ptr' | 'sliding';
interface Cell { val: number; state: 'idle' | 'active' | 'found' | 'eliminated' | 'window' }
interface Step { cells: Cell[]; pointers: Record<number, string[]>; log: string }

// ── State ──
const ALGOS: { id: AlgoId; label: string }[] = [
  { id: 'linear',  label: 'Linear'       },
  { id: 'binary',  label: 'Binary'       },
  { id: 'two-ptr', label: 'Two Pointers' },
  { id: 'sliding', label: 'Sliding Window'},
];

const activeAlgo = ref<AlgoId>('binary');
const n          = ref(10);
const target     = ref<number | null>(33);
const windowK    = ref(3);
const steps      = ref<Step[]>([]);
const stepIdx    = ref(0);
const playing    = ref(false);
const found      = ref(false);
const resultMsg  = ref('');
const logs       = ref<string[]>([]);

let timer: ReturnType<typeof setInterval> | null = null;

// ── Array helpers ──
function makeArray(sorted = false, reverse = false) {
  const arr: number[] = Array.from({ length: n.value }, () => Math.floor(Math.random() * 90) + 10);
  if (sorted) arr.sort((a, b) => a - b);
  if (reverse) arr.sort((a, b) => b - a);
  return arr;
}

const rawArr = ref<number[]>(makeArray(true));
const cells  = computed<Cell[]>(() =>
  steps.value.length ? steps.value[stepIdx.value].cells
    : rawArr.value.map(v => ({ val: v, state: 'idle' }))
);
const currentPointers = computed(() =>
  steps.value.length ? steps.value[stepIdx.value].pointers : {}
);
const progressPct = computed(() => steps.value.length > 1 ? (stepIdx.value / (steps.value.length - 1)) * 100 : 0);

function initRandom()  { rawArr.value = makeArray(false); reset(); }
function initSorted()  { rawArr.value = makeArray(true);  reset(); }
function initReverse() { rawArr.value = makeArray(false, true); reset(); }

function reset() {
  stopPlay(); steps.value = []; stepIdx.value = 0;
  found.value = false; resultMsg.value = ''; logs.value = [];
}

// ── Pointer helpers ──
function getPointers(idx: number) { return currentPointers.value[idx] ?? []; }
function getPointerLabel(idx: number): string {
  const pts = getPointers(idx);
  if (pts.includes('PTR-A')) return 'A';
  if (pts.includes('PTR-B')) return 'B';
  return pts.join(' / ');
}
function getPointerClass(idx: number) {
  const pts = getPointers(idx);
  if (pts.includes('MID'))   return 'sv-ptr-mid';
  if (pts.includes('LEFT'))  return 'sv-ptr-left';
  if (pts.includes('RIGHT')) return 'sv-ptr-right';
  if (pts.includes('PTR-A')) return 'sv-ptr-i';
  if (pts.includes('PTR-B')) return 'sv-ptr-j';
  if (pts.includes('i'))     return 'sv-ptr-i';
  if (pts.includes('j'))     return 'sv-ptr-j';
  return 'sv-ptr-left';
}
function getWindowClass(idx: number) {
  if (activeAlgo.value !== 'sliding' || !steps.value.length) return '';
  return steps.value[stepIdx.value].cells[idx]?.state === 'window' ? 'sv-window-active' : '';
}
function getCellClass(cell: Cell, _idx: number) {
  return {
    'sv-cell--idle':       cell.state === 'idle',
    'sv-cell--active':     cell.state === 'active',
    'sv-cell--found':      cell.state === 'found',
    'sv-cell--eliminated': cell.state === 'eliminated',
    'sv-cell--window':     cell.state === 'window',
  };
}

// ── Algorithm generators ──
function buildStep(arr: number[], pointers: Record<number, string[]>, states: Map<number, Cell['state']>, log: string): Step {
  const cellArr: Cell[] = arr.map((v, i) => ({ val: v, state: states.get(i) ?? 'idle' }));
  return { cells: cellArr, pointers, log };
}

function genLinear(arr: number[], tgt: number): Step[] {
  const out: Step[] = [];
  for (let i = 0; i < arr.length; i++) {
    const m = new Map<number, Cell['state']>();
    for (let k = 0; k < i; k++) m.set(k, 'eliminated');
    m.set(i, arr[i] === tgt ? 'found' : 'active');
    const log = arr[i] === tgt ? `✅ Tìm thấy ${tgt} tại [${i}]` : `So sánh [${i}]=${arr[i]} với target=${tgt}`;
    out.push(buildStep(arr, { [i]: ['i'] }, m, log));
    if (arr[i] === tgt) break;
  }
  return out;
}

function genBinary(arr: number[], tgt: number): Step[] {
  const out: Step[] = [];
  let lo = 0, hi = arr.length - 1;
  while (lo <= hi) {
    const mid = Math.floor((lo + hi) / 2);
    const m = new Map<number, Cell['state']>();
    for (let k = 0; k < lo; k++) m.set(k, 'eliminated');
    for (let k = hi + 1; k < arr.length; k++) m.set(k, 'eliminated');
    m.set(mid, arr[mid] === tgt ? 'found' : 'active');
    const ptrs: Record<number, string[]> = { [lo]: ['LEFT'], [hi]: ['RIGHT'], [mid]: ['MID'] };
    if (lo === mid) { ptrs[lo] = [...(ptrs[lo] || []), 'LEFT']; }
    const log = arr[mid] === tgt
      ? `✅ Tìm thấy ${tgt} tại [${mid}]`
      : `MID=${arr[mid]} ${arr[mid] < tgt ? '<' : '>'} ${tgt} → ${arr[mid] < tgt ? 'sang phải' : 'sang trái'}`;
    out.push(buildStep(arr, ptrs, m, log));
    if (arr[mid] === tgt) break;
    if (arr[mid] < tgt) lo = mid + 1; else hi = mid - 1;
  }
  return out;
}

function genTwoPointers(arr: number[], tgt: number): Step[] {
  const sorted = [...arr].sort((a, b) => a - b);
  const out: Step[] = [];
  let i = 0, j = sorted.length - 1;
  while (i < j) {
    const sum = sorted[i] + sorted[j];
    const m = new Map<number, Cell['state']>();
    const isFound = sum === tgt;
    m.set(i, isFound ? 'found' : 'active');
    m.set(j, isFound ? 'found' : 'active');
    const log = isFound
      ? `✅ Tìm thấy cặp [${i}]=${sorted[i]} + [${j}]=${sorted[j]} = ${tgt}`
      : `[${i}]=${sorted[i]} + [${j}]=${sorted[j]} = ${sum} ${sum < tgt ? '<' : '>'} ${tgt} → ${sum < tgt ? 'A tien' : 'B lui'}`;
    // Dùng key phân biệt rõ: 'PTR-A' cho i, 'PTR-B' cho j
    out.push(buildStep(sorted, { [i]: ['PTR-A'], [j]: ['PTR-B'] }, m, log));
    if (isFound) break;
    if (sum < tgt) i++; else j--;
  }
  return out;
}

function genSliding(arr: number[], k: number): Step[] {
  const out: Step[] = [];
  let winSum = arr.slice(0, k).reduce((a, b) => a + b, 0);
  for (let i = 0; i <= arr.length - k; i++) {
    if (i > 0) winSum = winSum - arr[i - 1] + arr[i + k - 1];
    const m = new Map<number, Cell['state']>();
    for (let w = i; w < i + k; w++) m.set(w, 'window');
    out.push(buildStep(arr, { [i]: ['L'], [i + k - 1]: ['R'] }, m, `Cửa sổ [${i}..${i + k - 1}] — Tổng = ${winSum}`));
  }
  return out;
}

function startSearch() {
  reset();
  const arr = rawArr.value;
  const tgt = Number(target.value);

  let st: Step[] = [];
  if (activeAlgo.value === 'linear')  st = genLinear(arr, tgt);
  if (activeAlgo.value === 'binary')  st = genBinary([...arr].sort((a, b) => a - b), tgt);
  if (activeAlgo.value === 'two-ptr') st = genTwoPointers(arr, tgt);
  if (activeAlgo.value === 'sliding') st = genSliding(arr, windowK.value);

  steps.value = st;
  stepIdx.value = 0;
  logs.value = st.map(s => s.log);

  const last = st[st.length - 1];
  const didFind = last ? last.cells.some(c => c.state === 'found') : false;
  found.value = didFind;

  // Trường hợp kết thúc NGAY ở bước 0 (vd Linear tìm phần tử đầu, Two Pointers
  // trúng cặp đầu+cuối): steps.length = 1 → watch(stepIdx) không bao giờ trigger
  // (stepIdx không đổi) → banner không hiện. Hiện banner luôn ở đây.
  if (st.length === 1) {
    showResult();
  } else {
    // Chỉ hiện banner khi animation kết thúc (sau khi play xong), không hiện ngay
    resultMsg.value = '';
  }
}

function showResult() {
  if (steps.value.length === 0) return;
  const tgt = Number(target.value);
  const last = steps.value[stepIdx.value];
  found.value = last ? last.cells.some(c => c.state === 'found') : false;

  if (activeAlgo.value === 'sliding') {
    const arr = rawArr.value;
    const k = windowK.value;
    let max = -Infinity, winSum = 0, bestIdx = 0;
    for (let i = 0; i < k; i++) winSum += arr[i];
    max = winSum;
    for (let i = 1; i <= arr.length - k; i++) {
      winSum = winSum - arr[i - 1] + arr[i + k - 1];
      if (winSum > max) { max = winSum; bestIdx = i; }
    }
    found.value = true; // Dùng màu xanh found
    resultMsg.value = `🏁 HOÀN THÀNH! TỔNG LỚN NHẤT LÀ ${max} (Tại cửa sổ [${bestIdx}..${bestIdx + k - 1}])`;
  } else {
    resultMsg.value = found.value ? `🎉 TÌM THẤY ${tgt}!` : `❌ KHÔNG TÌM THẤY ${tgt}`;
  }
}

function stepSearch(dir: 1 | -1) {
  stepIdx.value = Math.max(0, Math.min(steps.value.length - 1, stepIdx.value + dir));
}

function togglePlay() {
  if (playing.value) { stopPlay(); return; }
  if (!steps.value.length) startSearch();
  playing.value = true;
  timer = setInterval(() => {
    if (stepIdx.value >= steps.value.length - 1) {
      stopPlay();
      return;
    }
    stepIdx.value++;
  }, 700);
}
function stopPlay() { playing.value = false; if (timer) { clearInterval(timer); timer = null; } }
onUnmounted(stopPlay);

// Theo dõi tiến trình để hiện banner khi đến bước cuối (dành cho cả Play auto và Step thủ công)
watch(stepIdx, (idx) => {
  if (steps.value.length > 0 && idx === steps.value.length - 1) {
    showResult();
  } else {
    resultMsg.value = '';
  }
});

// Khi đổi thuật toán → chỉ reset state (giữ nguyên mảng)
watch(activeAlgo, reset);

// Khi kéo slider N → tạo lại mảng mới với kích thước mới, reset state
watch(n, (newN) => {
  // Giữ nguyên kiểu mảng (sorted nếu đang dùng Binary/Two Pointers)
  const needSorted = activeAlgo.value === 'binary' || activeAlgo.value === 'two-ptr';
  rawArr.value = makeArray(needSorted);
  reset();
});
</script>

<style scoped>
/* ── Root ── */
.sv-root {
  display: flex; flex-direction: column; height: 100%; width: 100%;
  padding: 6px; gap: 6px;
  background: var(--color-bg-primary);
  font-family: var(--font-sans);
}

/* ── Top bar ── */
.sv-topbar {
  display: flex; align-items: center; justify-content: space-between;
  padding: 6px 12px;
  background: var(--color-bg-surface);
  border: 1px solid var(--color-border-default);
  border-radius: 10px;
  backdrop-filter: blur(12px);
}
.sv-topbar-left { display: flex; align-items: center; gap: 10px; }
.sv-badge {
  display: flex; align-items: center; gap: 6px;
  font-size: 11px; font-weight: 800; color: #fcd34d;
  background: rgba(245,158,11,.12); padding: 4px 12px;
  border-radius: 999px; border: 1px solid rgba(245,158,11,.35);
}
.sv-badge-dot { width:7px; height:7px; border-radius:50%; background:#f59e0b; animation: pulse 1.5s infinite; }
.sv-tag { font-size:9px; font-weight:700; color:#64748b; border:1px solid #334155; border-radius:4px; padding:2px 6px; letter-spacing:.08em; }
.sv-fps  { font-size:10px; font-weight:700; color:#f59e0b; font-family:monospace; letter-spacing:.05em; }

/* ── Algo pills (Bottom Right) ── */
.sv-algo-pills-bottom {
  position: absolute; bottom: 12px; right: 12px;
  display: flex; gap: 4px;
  background: rgba(12,10,9,.88); backdrop-filter: blur(14px);
  padding: 4px; border-radius: 999px; border: 1px solid rgba(245,158,11,.15);
  box-shadow: 0 4px 24px rgba(0,0,0,.5);
}
.sv-pill {
  padding: 4px 14px; border-radius: 999px; font-size:11px; font-weight:700;
  border: 1px solid transparent; color: #a8a29e;
  background: transparent; cursor: pointer; transition: all .2s;
}
.sv-pill:hover { color: #d6d3d1; background:rgba(255,255,255,.05); }
.sv-pill--active {
  background: rgba(245,158,11,.15); color: #fbbf24;
  border-color: rgba(245,158,11,.55);
  box-shadow: 0 0 14px rgba(245,158,11,.35);
}

/* ── Body ── */
.sv-body { display: flex; flex: 1; min-height: 0; gap: 6px; }

/* ── Aside ── */
.sv-aside {
  width: 180px; flex-shrink: 0; display: flex; flex-direction: column; gap: 4px;
  background: var(--color-bg-surface);
  border: 1px solid rgba(245,158,11,.15);
  border-radius: 10px; padding: 10px; overflow-y: auto;
}
.sv-aside-section { display: flex; flex-direction: column; gap: 4px; padding-bottom: 8px; border-bottom: 1px solid rgba(255,255,255,.05); }
.sv-aside-section:last-child { border-bottom: none; flex:1; }
.sv-aside-title { font-size:9px; font-weight:700; text-transform:uppercase; letter-spacing:.08em; color:#78716c; margin-bottom:2px; }

.sv-btn-ghost {
  width:100%; padding:5px 8px; border-radius:6px; font-size:11px; font-weight:600;
  cursor:pointer; border:1px solid rgba(100,116,139,.3);
  background: rgba(255,255,255,.03); color:#94a3b8;
  transition: all .15s; text-align:left;
}
.sv-btn-ghost:hover { background:rgba(251,146,60,.1); color:#fb923c; border-color:rgba(251,146,60,.35); }

.sv-slider { width:100%; accent-color:#f59e0b; cursor:pointer; }
.sv-slider-val { font-size:11px; font-weight:700; color:#f59e0b; text-align:right; }

.sv-target-wrap { display:flex; gap:4px; }
.sv-target-input {
  flex:1; min-width:0; background:rgba(255,255,255,.05); border:1px solid rgba(245,158,11,.35);
  border-radius:6px; padding:5px 8px; font-size:12px; font-weight:700; color:#fff;
  outline:none;
}
.sv-target-input:focus { border-color:#f59e0b; box-shadow:0 0 10px rgba(245,158,11,.3); }
.sv-btn-search {
  padding:5px 8px; border-radius:6px; font-size:11px; font-weight:700;
  background:linear-gradient(135deg,#d97706,#b45309); color:#fff;
  border:none; cursor:pointer; transition:all .15s; white-space:nowrap;
}
.sv-btn-search:hover { filter:brightness(1.2); transform:scale(1.02); box-shadow:0 0 10px rgba(245,158,11,.4); }
.sv-hint { font-size:9px; color:#92400e; background:rgba(245,158,11,.08); border:1px solid rgba(245,158,11,.2); border-radius:4px; padding:4px 6px; line-height:1.4; }

/* Log */
.sv-log-box { overflow-y:auto; }
.sv-log-entry { display:flex; align-items:flex-start; gap:4px; font-size:10px; color:#94a3b8; line-height:1.5; padding: 1px 0; }
.sv-log-bullet { width:4px; height:4px; border-radius:50%; background:#f59e0b; margin-top:5px; flex-shrink:0; }
.sv-log-empty { font-size:10px; color:#475569; font-style:italic; }

/* ── Canvas ── */
.sv-canvas {
  flex:1; position:relative; display:flex; align-items:center; justify-content:center;
  background:
    linear-gradient(rgba(255,255,255,.03) 1px, transparent 1px),
    linear-gradient(90deg, rgba(255,255,255,.03) 1px, transparent 1px),
    radial-gradient(ellipse at 50% 35%, rgba(245,158,11,.07) 0%, transparent 60%),
    var(--color-bg-primary);
  background-size: 24px 24px, 24px 24px, 100% 100%, 100% 100%;
  background-position: center center;
  border: 1px solid rgba(245,158,11,.12);
  border-radius: 10px; overflow:hidden;
}

/* Result banner */
.sv-result-banner {
  position:absolute; top:16px; left:50%; transform:translateX(-50%);
  padding:8px 24px; border-radius:999px; font-size:13px; font-weight:800;
  letter-spacing:.04em; z-index:10; animation: fadeDown .4s ease;
}
.sv-result-found    { background:rgba(245,158,11,.15); color:#fbbf24; border:1px solid rgba(245,158,11,.4); box-shadow:0 0 24px rgba(245,158,11,.25); }
.sv-result-notfound { background:rgba(239,68,68,.12);  color:#f87171; border:1px solid rgba(239,68,68,.3);  box-shadow:0 0 20px rgba(239,68,68,.2); }

/* Array cells row */
.sv-cells-wrap {
  display:flex; align-items:flex-end; justify-content:center;
  gap:8px; padding: 60px 20px 90px; flex-wrap:wrap;
}
.sv-cell-col  { display:flex; flex-direction:column; align-items:center; gap:6px; }
.sv-pointer-row { height:28px; display:flex; align-items:flex-end; justify-content:center; }
.sv-pointer {
  font-size:9px; font-weight:800; letter-spacing:.05em;
  padding:2px 6px; border-radius:4px; white-space:nowrap;
  animation: bounce 1s infinite;
}
.sv-ptr-mid   { color:#f43f5e; background:rgba(244,63,94,.15); border:1px solid rgba(244,63,94,.3); }
.sv-ptr-left  { color:#fbbf24; background:rgba(251,191,36,.12); border:1px solid rgba(251,191,36,.3); }
.sv-ptr-right { color:#fb923c; background:rgba(251,146,60,.12); border:1px solid rgba(251,146,60,.3); }
.sv-ptr-i     { color:#c084fc; background:rgba(192,132,252,.1); border:1px solid rgba(192,132,252,.3); }
.sv-ptr-j     { color:#4ade80; background:rgba(74,222,128,.1);  border:1px solid rgba(74,222,128,.3); }

/* Window outer highlight */
.sv-window-active { outline:2px solid rgba(245,158,11,.6); outline-offset:3px; border-radius:10px; background:rgba(245,158,11,.05); }

/* Cell */
.sv-cell {
  width:52px; height:52px; border-radius:10px; display:flex; align-items:center; justify-content:center;
  font-size:16px; font-weight:800; font-family:monospace;
  border:1px solid rgba(100,116,139,.2); transition:all .35s cubic-bezier(.4,0,.2,1);
}
.sv-cell--idle       { background:rgba(87,83,78,.8); color:#d6d3d1; border-color:rgba(120,113,108,.5); }
.sv-cell--active     { background:rgba(245,158,11,.18); color:#fbbf24; border-color:rgba(245,158,11,.7); box-shadow:0 0 20px rgba(245,158,11,.4); transform:scale(1.1); }
.sv-cell--found      { background:rgba(245,158,11,.25); color:#fcd34d; border-color:#f59e0b; box-shadow:0 0 28px rgba(245,158,11,.6), 0 0 8px rgba(245,158,11,.4) inset; transform:scale(1.14); }
.sv-cell--eliminated { background:rgba(41,37,36,.7); color:#a8a29e; border-color:rgba(87,83,78,.4); transform:scale(.9); opacity:.7; }
.sv-cell--window     { background:rgba(245,158,11,.12); color:#fcd34d; border-color:rgba(245,158,11,.45); }
.sv-cell-index       { font-size:10px; color:#a8a29e; font-family:monospace; }

/* VCR bar */
.sv-vcr {
  position:absolute; bottom:12px; left:50%; transform:translateX(-50%);
  display:flex; align-items:center; gap:10px;
  background:rgba(12,10,9,.88); backdrop-filter:blur(14px);
  border:1px solid rgba(245,158,11,.2); border-radius:999px;
  padding:8px 20px; box-shadow:0 4px 28px rgba(0,0,0,.6), 0 0 0 1px rgba(245,158,11,.05);
}
.sv-vcr-speed {
  background:rgba(255,255,255,.06); border:1px solid rgba(100,116,139,.3);
  border-radius:6px; color:#94a3b8; font-size:11px; font-weight:700; padding:2px 6px; cursor:pointer;
}
.sv-vcr-btn {
  width:28px; height:28px; border-radius:50%; display:flex; align-items:center; justify-content:center;
  background:rgba(255,255,255,.06); border:1px solid rgba(100,116,139,.3);
  color:#94a3b8; cursor:pointer; font-size:11px; transition:all .15s;
}
.sv-vcr-btn:hover { background:rgba(245,158,11,.15); color:#fbbf24; border-color:rgba(245,158,11,.4); }
.sv-vcr-play {
  width:36px; height:36px; border-radius:50%; display:flex; align-items:center; justify-content:center;
  background:linear-gradient(135deg,#d97706,#92400e); color:#fff; font-size:15px;
  border:none; cursor:pointer; box-shadow:0 0 16px rgba(245,158,11,.5);
  transition:all .15s;
}
.sv-vcr-play:hover { filter:brightness(1.2); transform:scale(1.06); }
.sv-vcr-bar {
  width:160px; height:4px; background:rgba(255,255,255,.08);
  border-radius:999px; overflow:hidden; cursor:pointer;
}
.sv-vcr-fill { height:100%; background:linear-gradient(90deg,#d97706,#fbbf24); border-radius:999px; transition:width .35s; }
.sv-vcr-count { font-size:10px; font-weight:700; color:#57534e; font-family:monospace; white-space:nowrap; }

/* Animations */
@keyframes pulse  { 0%,100%{opacity:1}50%{opacity:.4} }
@keyframes bounce { 0%,100%{transform:translateY(0)}50%{transform:translateY(-4px)} }
@keyframes fadeDown { from{opacity:0;transform:translateX(-50%) translateY(-8px)} to{opacity:1;transform:translateX(-50%) translateY(0)} }

</style>
