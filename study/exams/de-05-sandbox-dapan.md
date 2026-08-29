# Đáp án Đề 05 — Sandbox Views

---

## PHẦN I — TRẮC NGHIỆM

### Câu 1 — Đáp án: B
**Giải thích:** Cả 4 route sandbox đều được cấu hình trong `router/index.ts` với cùng một lazy import:
```js
{ path: '/sorting-sandbox',   name: 'sorting-sandbox',   component: () => import('@/views/SortingView.vue') },
{ path: '/searching-sandbox', name: 'searching-sandbox', component: () => import('@/views/SortingView.vue') },
{ path: '/graph-playground',  name: 'graph-playground',  component: () => import('@/views/SortingView.vue') },
{ path: '/stack-queue-sandbox', name: 'stack-queue-sandbox', component: () => import('@/views/SortingView.vue') },
```
`SortingView.vue` là component đa năng, dùng `route.name` để switch nội dung giữa 4 tab/mode.

---

### Câu 2 — Đáp án: B
**Giải thích:** Trong `App.vue`, `<RouterView>` được gắn prop `:key`:
```vue
<RouterView :key="$route.fullPath" />
```
Khi `fullPath` thay đổi (từ `/sorting-sandbox` → `/graph-playground`), Vue xem đây là component khác nhau và thực hiện:
1. **Unmount** component cũ → gọi `onUnmounted()` → cleanup engine, animation, event listeners.
2. **Mount** component mới từ đầu → gọi `onMounted()` → khởi tạo engine mới sạch.

Đảm bảo không có state rò rỉ giữa các sandbox.

---

### Câu 3 — Đáp án: C
**Giải thích:** `StackQueueView.vue` nằm trong thư mục `stackqueue/` và import engine riêng:
```js
import { stackQueueEngine } from './stackQueueEngine'
```
File `stackQueueEngine.ts` có kích thước **25766 bytes** — chứa toàn bộ logic: push, pop, enqueue, dequeue, animation frames, undo/redo stack.

---

### Câu 4 — Đáp án: B
**Giải thích:** Route `CodeToVisualView` được khai báo với:
```js
{
  path: '/playground/code-to-visual',
  component: () => import('@/views/CodeToVisualView.vue'),
  meta: { requiresAuth: true }
}
```
`requiresAuth: true` → Navigation guard `router.beforeEach` kiểm tra authentication token. Nếu chưa đăng nhập, redirect về `/login?redirect=/playground/code-to-visual`.

---

### Câu 5 — Đáp án: B
**Giải thích:** Thiết kế **1 component — 4 route** có mục đích chính là **tái sử dụng codebase**:
- `SortingView.vue` có layout chung: panel cấu hình + canvas animation + code panel.
- Nội dung cụ thể được quyết định bởi `route.name`:
  ```js
  const mode = computed(() => route.name) // 'sorting-sandbox' | 'searching-sandbox' | ...
  ```
- Giảm code duplication: không cần viết 4 component riêng với layout giống nhau.

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG

### Câu 6 — Trace: Chuyển từ `/sorting-sandbox` → `/graph-playground`

**1. UI — Sự kiện click & Router xử lý**
- Người dùng click link `<RouterLink to="/graph-playground">` trong Navbar hoặc Sidebar.
- Vue Router bắt sự kiện, gọi `router.push('/graph-playground')`.
- **Navigation guards** chạy theo thứ tự:
  - `beforeEach`: kiểm tra auth nếu có `requiresAuth` (graph-playground không có → pass).
  - `beforeRouteLeave` của component cũ (SortingView): dọn dẹp nếu cần.

**2. FE Data — Key remount mechanism**
- `App.vue` reactive: `$route.fullPath` thay đổi từ `'/sorting-sandbox'` → `'/graph-playground'`.
- `:key="$route.fullPath"` trên `<RouterView>` thay đổi → Vue destroy component cũ:
  - `SortingView.vue` (mode sorting): `onUnmounted()` chạy → clear `sortingEngine` timers, cancel animation frames, remove event listeners.
- Vue mount **component mới**: `SortingView.vue` được import lại → `onMounted()` chạy.
- Trong `onMounted`, component đọc `route.name === 'graph-playground'` → init Graph mode.

**3. Backend — Dữ liệu khởi tạo**
- Với sandbox mode: **không có API call** cho dữ liệu ban đầu — graph được khởi tạo với đồ thị mẫu hard-coded hoặc random.
- Nếu có lưu trạng thái: `GET /api/sandbox/graph/default` để lấy graph template.
- Danh sách thuật toán graph (BFS, DFS, Dijkstra...) được load từ registry tĩnh (JS object/JSON).

**4. UI Render — Graph tab render**
- `SortingView.vue` computed `mode === 'graph-playground'` → render `<GraphPanel>` thay vì `<SortingPanel>`.
- `graphEngine.ts` được khởi tạo với canvas element từ `ref`.
- Hiển thị: canvas đồ thị với nodes và edges; dropdown chọn thuật toán (BFS/DFS/Dijkstra); nút Play/Step/Reset.
- Animation ready, chờ user nhấn Play.

---

### Câu 7 — Trace: Nhập code vào `/playground/code-to-visual` và nhấn "Visualize"

**1. UI — Auth guard & Giao diện**
- Browser navigate đến `/playground/code-to-visual`.
- `router.beforeEach` phát hiện `meta.requiresAuth: true` → kiểm tra `authStore.isAuthenticated`:
  - Nếu false → redirect `/login?redirect=/playground/code-to-visual`.
  - Nếu true → `CodeToVisualView.vue` được mount.
- Giao diện hiển thị:
  - **Editor panel** (Monaco Editor hoặc textarea): người dùng nhập code.
  - **Visualization panel**: canvas trống chờ kết quả.
- Người dùng nhập đoạn code → nhấn nút **"Visualize"**.

**2. FE Data — Chuẩn bị request**
- Click "Visualize" trigger handler `handleVisualize()`:
  ```js
  const codeInput = ref('')
  const handleVisualize = async () => {
    isLoading.value = true
    const result = await visualizerApi.parse({ code: codeInput.value, language: selectedLang.value })
    visualizationData.value = result
  }
  ```
- Code text + ngôn ngữ được đóng gói thành request body.
- Token JWT được đính kèm vào header `Authorization: Bearer <token>`.

**3. Backend — Parse & Generate visualization data**
- `POST /api/code-visualizer/parse` nhận body `{ code: string, language: string }`.
- **`CodeVisualizerController`** (ASP.NET Core):
  - Validate input (độ dài, ký tự hợp lệ).
  - Gọi service **`CodeParserService`** để parse code thành AST.
  - `VisualizationGeneratorService` duyệt AST → sinh danh sách **step frames**:
    ```json
    [
      { "step": 1, "action": "assign", "variable": "x", "value": 5, "highlight": [1] },
      { "step": 2, "action": "loop_start", "condition": "i < 5", "highlight": [2] }
    ]
    ```
  - Trả về `200 OK` với array `steps[]`.

**4. UI Render — Animation step-by-step**
- `CodeToVisualView.vue` nhận `visualizationData` (array of steps):
  - **Code panel**: highlight dòng code tương ứng với `step.highlight` khi animation chạy.
  - **Memory panel**: hiển thị biến và giá trị thay đổi theo từng step.
  - **Stack/Heap panel**: nếu có function call, hiển thị call stack frame.
- Animation engine (internal timer / `requestAnimationFrame`) duyệt qua `steps[]` theo tốc độ slider.
- Nút Step Forward/Step Backward cho phép di chuyển thủ công từng bước.
- Nút Reset đưa về `step 0`, xóa highlight.
