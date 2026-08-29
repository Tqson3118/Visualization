# Đáp Án — Đề 08: Code Runner

---

## PHẦN I — TRẮC NGHIỆM

### Câu 1 — Đáp án: **C**
**Lý do:** `CodeRunnerView.vue` dòng 220–226:
```html
<textarea
  v-model="codeStore.editorCode"
  class="code-runner__textarea"
  spellcheck="false"
  :aria-label="`Trình soạn mã ${key}`"
  @scroll="onEditorScroll"
/>
```
`aria-label` là **binding động** tính theo `key` (route param). Comment dòng 8: "GIỮ textarea aria-label + text 'Thành công · Xms' (e2e)." Chú thích cũng ghi Monaco sẽ bật khi cài gói `monaco-editor`, hiện tại vẫn là `textarea`.

---

### Câu 2 — Đáp án: **A**
**Lý do:** Dòng 38–43:
```ts
const {
  currentIndex: playbackIndex,
  totalFrames:  playbackFrames,
  currentStructure: playbackStructure,
  currentLine:  playbackLine,
} = playback;
```
Các tên **bên phải** là tên gốc trong composable, tên **bên trái** (`playbackIndex`, ...) là tên alias dùng trong template. Đáp án A liệt kê đúng tên alias được dùng trong template.

---

### Câu 3 — Đáp án: **B**
**Lý do:** `onRun()` dòng 108–111:
```ts
if (codeStore.runState === 'passed' && result && Array.isArray(result.trace) && result.trace.length > 0) {
  traceRef.value = result.trace;
  playback.init(result.trace);
  playback.play();
}
```
Thứ tự: **traceRef.value = result.trace → playback.init(result.trace) → playback.play()**.

---

### Câu 4 — Đáp án: **B**
**Lý do:** Template dòng 282:
```html
<CanvasArea
  :structure="traceMode ? playbackStructure : simStore.currentStep?.structure ?? null"
  ...
/>
```
Khi `traceMode = false`, canvas dùng `simStore.currentStep?.structure ?? null` — tức **preview generator mẫu** đã được load ở `onMounted` qua `simStore.loadSim(key)`.

---

### Câu 5 — Đáp án: **B**
**Lý do:** Dòng 122–125:
```ts
function showSamplePreview(): void {
  traceRef.value = null;
  playback.reset();
}
```
Đặt `traceRef = null` → `traceMode` computed = `false` → canvas quay về generator preview; `playback.reset()` đặt lại index về 0.

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG

### Câu 6 — Trace khởi tạo CodeRunnerView khi vào `/code/sort.bubble`

| Chặng | Tên thật (source) | Mô tả |
|-------|-------------------|-------|
| **1** | `onMounted` (hook) | Hook mount kích hoạt; `key` computed = `'sort.bubble'` từ `route.params.key`; `loading.value = true` |
| **2** | `codeStore.loadTemplate(key.value)` | `useCodeRunnerStore.loadTemplate('sort.bubble')` — tải template code mẫu vào `codeStore.editorCode` |
| **3** | `simStore.loadSim(key.value)` | `useSimulationStore.loadSim('sort.bubble')` — nạp steps generator thật cho canvas preview; wrapped trong `.catch()` vì không bắt buộc |
| **4** | `loading.value = false` (finally) | Kết thúc loading → template render: editor textarea, CanvasArea (preview generator), StatsBar, ControlBar |

---

### Câu 7 — Trace khi click "Chạy" và code thành công có trace

| Chặng | Tên thật (source) | Mô tả |
|-------|-------------------|-------|
| **1** | `onRun()` (hàm) | Click nút Chạy → `onRun()` gọi `codeStore.run()`; trong lúc đó `codeStore.isRunning = true` (nút hiện spinner) |
| **2** | `codeStore.run()` → `result` | Sandbox thực thi code; trả về object gồm `{ trace: TraceEvent[], ... }`; `codeStore.runState = 'passed'`; `codeStore.lastStats` được set |
| **3** | `traceRef.value = result.trace` + `playback.init(result.trace)` | Gán `TraceEvent[]` vào `traceRef`; `traceMode` computed chuyển thành `true`; `useCodeTracePlayback.init()` reset `playbackIndex = 0`, `playbackFrames = trace.length` |
| **4** | `playback.play()` + `ui.showToast('Chạy thành công!', 'success')` | Playback bắt đầu tự động phát; canvas hiển thị `playbackStructure`; toast thành công xuất hiện; StatsBar dùng `playbackIndex`/`playbackFrames` |
