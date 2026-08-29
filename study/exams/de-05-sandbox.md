# Đề 05 — Sandbox Views
**Thời gian:** 25 phút | **Tổng điểm:** 10 điểm
**Bao phủ:** SortingView.vue · SearchingView · GraphView · StackQueueView · CodeToVisualView.vue

---

## PHẦN I — TRẮC NGHIỆM (5 câu × 1 điểm = 5 điểm)

**Câu 1:** Các route sandbox sau đây cùng sử dụng **một component duy nhất** nào?

```
/sorting-sandbox     (name: 'sorting-sandbox')
/searching-sandbox   (name: 'searching-sandbox')
/graph-playground    (name: 'graph-playground')
/stack-queue-sandbox (name: 'stack-queue-sandbox')
```

A. `SandboxLayout.vue`
B. `SortingView.vue`
C. `AlgorithmView.vue`
D. `PlaygroundView.vue`

---

**Câu 2:** Trong `App.vue`, cơ chế nào đảm bảo component được **remount hoàn toàn** khi người dùng chuyển giữa các route sandbox khác nhau (ví dụ từ `/sorting-sandbox` sang `/graph-playground`)?

A. `watch(() => route.name, ...)` tự reset state thủ công
B. `<RouterView :key="route.fullPath" />` — key theo `fullPath` force remount
C. `v-if` / `v-else` trên từng component riêng biệt
D. Vuex action `RESET_SANDBOX` được dispatch mỗi khi route thay đổi

---

**Câu 3:** `StackQueueView.vue` (thư mục `stackqueue/`) sử dụng engine nào để xử lý logic hoạt ảnh?

A. `sortingEngine.ts`
B. `graphEngine.ts`
C. `stackQueueEngine.ts` (25766 bytes)
D. `animationCore.ts`

---

**Câu 4:** Route `/playground/code-to-visual` (component `CodeToVisualView.vue`) yêu cầu điều kiện gì để truy cập?

A. `meta: { public: true }` — bất kỳ ai cũng vào được
B. `meta: { requiresAuth: true }` — phải đăng nhập mới vào được
C. `meta: { requiresRole: 'teacher' }` — chỉ giáo viên được vào
D. Không có meta guard nào — route không được bảo vệ

---

**Câu 5:** Trong thiết kế 4 route sandbox dùng chung `SortingView.vue`, mục đích chính của cách thiết kế này là gì?

A. Giảm số lượng file trong project để dễ quản lý
B. Tái sử dụng **1 component đa tab** cho 4 loại thuật toán, phân biệt qua `route.name` hoặc prop
C. Ép Vue Router tái sử dụng DOM node để tăng performance
D. Cho phép 4 route cùng share một Vuex store mà không cần namespace

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG (2 câu × 2.5 điểm = 5 điểm)

> **Yêu cầu chung:** Viết đủ **4 chặng**: `1. UI` → `2. FE Data` → `3. Backend` → `4. UI Render`
> Dùng tên hàm, component, store, engine, API thật từ source code.

---

**Câu 6:** Trace luồng khi người dùng đang ở `/sorting-sandbox` rồi **click navigation để chuyển sang `/graph-playground`**.

Gợi ý: Đề cập đến cơ chế key remount của App.vue, lifecycle hooks bị trigger, và trạng thái engine bị reset.

Viết 4 chặng:
1. **UI** — Sự kiện nào xảy ra khi user click; Vue Router làm gì
2. **FE Data** — `App.vue` key thay đổi thế nào; component cũ bị destroy, component mới mount ra sao
3. **Backend** — Có API call nào không khi mount sandbox mới; dữ liệu khởi tạo lấy từ đâu
4. **UI Render** — `SortingView.vue` (mode Graph) render tab/panel nào; engine nào được khởi tạo

---

**Câu 7:** Trace luồng khi người dùng vào `/playground/code-to-visual`, **nhập đoạn code và nhấn "Visualize"**.

Gợi ý: Đề cập đến guard auth, cách code text được gửi lên backend, và cách animation được render.

Viết 4 chặng:
1. **UI** — Guard `requiresAuth` kiểm tra gì; `CodeToVisualView.vue` hiển thị giao diện nào
2. **FE Data** — Code input được lưu vào state nào; request được chuẩn bị ra sao
3. **Backend** — Endpoint nào nhận code; xử lý parse và trả về dữ liệu visualization
4. **UI Render** — Dữ liệu trả về được bind vào component nào; animation step-by-step diễn ra thế nào
