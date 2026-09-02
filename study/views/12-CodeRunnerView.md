# 💻 VIEW 12: TRÌNH BIÊN DỊCH & CHẠY CODE (CODERUNNERVIEW)

* **Tên file Vue**: [`CodeRunnerView.vue`](file:///d:/FPT/metqua/frontend/src/views/CodeRunnerView.vue)
* **Đường dẫn URL**: `/code/:key`
* **Route Name**: `code`
* **Quyền truy cập**: Đã đăng nhập (`requiresAuth: true`).

---

## 1. CẤU TRÚC GIAO DIỆN

```
┌────────────────────────────────────────────────────────────────────────┐
│ [← Thoát]  TRÌNH CHẠY CODE: BUBBLE SORT          [ ⚙️ JS / C++ / Java ]│
├───────────────────────────────────┬────────────────────────────────────┤
│ CỘT TRÁI (6/12): TRÌNH SOẠN THẢO  │ CỘT PHẢI (6/12): CANVAS & TERMINAL │
│ (Monaco Editor / Canvas-Ink)      │                                    │
│                                   │ ┌── CANVAS TRỰC QUAN HÓA TRACE ──┐ │
│ 1 function bubbleSort(arr) {      │ │  █  █  █  █  █  █  █  █        │ │
│ 2   let n = arr.length;           │ └────────────────────────────────┘ │
│ 3   for (let i = 0; i < n; i++) { │ [⏮] [◀] [▶ Playback] [▶|]          │
│ 4     for (let j = 0; j < n-i-1) {│ ────────────────────────────────── │
│ 5       if (arr[j] > arr[j+1]) {  │ 📟 TERMINAL OUTPUT:                │
│ 6         // swap...              │ [Running sandbox...]               │
│                                   │ > Output: [1, 2, 3, 5, 7, 8, 9]    │
│ [ ▶ CHẠY CODE TRỰC TIẾP ]         │ > Execution Time: 12ms • Memory: 1MB│
└───────────────────────────────────┴────────────────────────────────────┘
```

---

## 2. CHI TIẾT CÁC LUỒNG HOẠT ĐỘNG (FLOWS)

### 🔹 Flow 1: Biên dịch & Chạy mã nguồn Sandbox
1. Người dùng viết hoặc sửa đổi thuật toán trong Monaco Editor.
2. Bấm nút **"Chạy code" (Run)**.
3. Code được chuyển đến [`useCodeTracePlayback.ts`](file:///d:/FPT/metqua/frontend/src/composables/useCodeTracePlayback.ts) và Web Worker:
   * Chèn Probe AST vào code JavaScript.
   * Chạy thực thi trong môi trường sandbox an toàn.
   * Bắt các sự kiện `compare`, `swap`, `write` để sinh ra dòng Trace động.
4. Terminal xuất kết quả `stdout` và thời gian thực thi.
5. Canvas bên phải đồng bộ phát lại chuyển động của chính đoạn code người dùng vừa viết.

### 🔹 Flow 2: Lưu và hiển thị lịch sử nộp bài (Local Run History)
* Mỗi lần bấm Chạy $\rightarrow$ Hệ thống ghi một bản ghi vào `localRuns` (ID, Trạng thái Passed/Failed, Thời gian thực thi, Lỗi cú pháp nếu có).

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend View**: [`CodeRunnerView.vue`](file:///d:/FPT/metqua/frontend/src/views/CodeRunnerView.vue)
* **Frontend Composable**: `src/composables/useCodeTracePlayback.ts`
* **Frontend Store**: [`codeRunner.ts`](file:///d:/FPT/metqua/frontend/src/stores/codeRunner.ts)
* **Web Worker**: `src/engines/worker/compileWorker.ts`
