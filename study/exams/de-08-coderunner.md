# Đề 08 — Code Runner
**Thời gian:** 25 phút | **Tổng điểm:** 10 điểm
**Bao phủ:** CodeRunnerView (`/code/:key`)

---

## PHẦN I — TRẮC NGHIỆM (5 câu × 1 điểm = 5 điểm)

**Câu 1.** Editor trong `CodeRunnerView.vue` hiện tại dùng element HTML nào, và `aria-label` được gắn như thế nào?

A. `<div contenteditable>`, `aria-label="Editor"`
B. `<textarea>`, `aria-label` tĩnh = `"Code editor"`
C. `<textarea>`, `aria-label` động = `` `Trình soạn mã ${key}` ``
D. `<monaco-editor>`, `aria-label="Monaco Editor"`

---

**Câu 2.** Composable `useCodeTracePlayback` trong `CodeRunnerView.vue` được destructure ra những refs nào để dùng trực tiếp trong template?

A. `playbackIndex`, `playbackFrames`, `playbackStructure`, `playbackLine`
B. `currentIndex`, `totalFrames`, `currentStructure`, `currentLine`
C. `traceIndex`, `traceFrames`, `traceStructure`, `traceLine`
D. `index`, `frames`, `structure`, `line`

---

**Câu 3.** Sau khi hàm `onRun()` được gọi và `codeStore.runState === 'passed'` đồng thời `result.trace` có dữ liệu, bộ ba hành động nào xảy ra theo đúng thứ tự?

A. `playback.reset()` → `traceRef.value = result.trace` → `playback.play()`
B. `traceRef.value = result.trace` → `playback.init(result.trace)` → `playback.play()`
C. `playback.init(result.trace)` → `traceRef.value = result.trace` → `playback.pause()`
D. `traceRef.value = null` → `playback.reset()` → `playback.init(result.trace)`

---

**Câu 4.** Trong `CodeRunnerView.vue`, khi `traceMode` là `false` (không có trace), canvas hiển thị structure từ đâu?

A. `playbackStructure` từ composable `useCodeTracePlayback`
B. `simStore.currentStep?.structure ?? null` — generator preview mẫu
C. `codeStore.lastOutput` được parse thành cấu trúc
D. Luôn hiển thị blank canvas trắng

---

**Câu 5.** Hàm `showSamplePreview()` trong `CodeRunnerView.vue` thực hiện gì?

A. Gọi API lấy bài mẫu từ server rồi hiển thị
B. Đặt `traceRef.value = null` và gọi `playback.reset()` để quay về preview generator mẫu
C. Reload toàn bộ trang để reset trạng thái
D. Gọi `simStore.loadSim(key)` để tải lại animation mẫu

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG (2 câu × 2.5 điểm = 5 điểm)

**Câu 6.** Trace luồng khởi tạo `CodeRunnerView.vue` khi user vào `/code/sort.bubble`.
Ghi đủ **4 chặng** tên hàm/hook/store-action thật từ source, mô tả ngắn mỗi chặng.

```
Chặng 1 → _______________: _______________
Chặng 2 → _______________: _______________
Chặng 3 → _______________: _______________
Chặng 4 → _______________: _______________
```

---

**Câu 7.** Trace luồng khi user bấm nút **Chạy (Ctrl+Enter)** và code chạy thành công với trace hợp lệ.
Ghi đủ **4 chặng** tên hàm/computed/state thật từ source, mô tả ngắn mỗi chặng.

```
Chặng 1 → _______________: _______________
Chặng 2 → _______________: _______________
Chặng 3 → _______________: _______________
Chặng 4 → _______________: _______________
```
