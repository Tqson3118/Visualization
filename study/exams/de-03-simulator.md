# Đề 03 — SimulatorView & useSimulation

**Thời gian:** 25 phút | **Tổng điểm:** 10 điểm
**Bao phủ:** `SimulatorView.vue` · `useSimulation` composable · `ControlBar` · `InputModal` · `FavoritesController` · Panel layout

---

## PHẦN I — TRẮC NGHIỆM (5 câu × 1 điểm = 5 điểm)

**Câu 1:** Layout 3-panel của `SimulatorView.vue` phân chia cột theo tỷ lệ nào (tính trên grid 12 cột)?

A. PseudocodePanel (4/12) + CanvasArea (4/12) + ExplainPanel (4/12)
B. PseudocodePanel (3/12) + CanvasArea (6/12) + ExplainPanel (3/12)
C. PseudocodePanel (2/12) + CanvasArea (8/12) + ExplainPanel (2/12)
D. Chỉ có 2 panel: PseudocodePanel (4/12) + CanvasArea (8/12)

---

**Câu 2:** `useSimulation` composable expose những reactive state nào? (Chọn đáp án liệt kê ĐÚNG và ĐẦY ĐỦ nhất)

A. `currentSim`, `currentStep`, `currentIndex`, `steps`, `speed`, `status`, `loading`, `loadError`
B. `currentSim`, `frames`, `fps`, `isPlaying`, `error`
C. `algorithm`, `dataset`, `result`, `elapsed`, `running`
D. `sim`, `step`, `index`, `history`, `velocity`, `ready`

---

**Câu 3:** User click nút **Star** để thêm thuật toán vào danh sách yêu thích. Luồng gọi hàm/API diễn ra theo thứ tự nào?

A. Component → `favoritesApi.addFavorite()` → `FavoritesController.AddFavorite()` trên backend
B. Component → Vuex `dispatch('addFavorite')` → Axios POST trực tiếp từ store
C. Component → `favoritesApi.toggleFavorite()` → một endpoint duy nhất tự xử lý add/remove
D. Component → `simulatorStore.addFavorite()` → lưu vào `localStorage` (không gọi API)

---

**Câu 4:** `InputModal` trong `SimulatorView.vue` hoạt động thế nào khi user submit input mới (ví dụ: đổi mảng cần sort)?

A. Gọi `POST /api/simulator/input` gửi dữ liệu lên server, server tính lại và trả về steps mới
B. Gọi `configureInput(newInput)` trong `useSimulation` composable → engine phía client tính lại toàn bộ steps mô phỏng
C. Reload toàn bộ trang với URL params mới (query string `?input=...`)
D. Dispatch Vuex action `SET_INPUT` để cập nhật global state, component tự re-render

---

**Câu 5:** Điều kiện nào đúng để `SimulatorView.vue` **cho phép xem** mà **không redirect về login**?

A. User phải có role `STUDENT` hoặc `TEACHER` (role `TEACHER_PENDING` bị chặn)
B. `isAuthenticated === true` HOẶC `getCatalogMeta(key)?.demoAllowed === true` — một trong hai thỏa mãn là được
C. Bắt buộc `isAuthenticated === true` — không có ngoại lệ nào dù là demo
D. Route phải có query param `?public=true` trong URL

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG (2 câu × 2.5 điểm = 5 điểm)

> Yêu cầu viết rõ **4 chặng** cho mỗi câu

---

**Câu 6:** Trace luồng khi **user đã đăng nhập** mở `SimulatorView.vue` với key `sort.bubble`, sau đó nhấn nút **Play** để bắt đầu mô phỏng.

1. **Thao tác UI (khởi tạo component):** `route.params.key` được đọc thế nào? `useSimulation('sort.bubble')` trả về những reactive state gì? State ban đầu của `status` và `loading` là gì?

2. **Frontend Data Layer (load data):** `useSimulation` gọi API nào để lấy dữ liệu mô phỏng? Viết URL, Method, và mô tả ít nhất 3 fields trong Response. Sau khi load xong, `steps` là mảng gì và `currentIndex` có giá trị ban đầu nào?

3. **ControlBar → Play:** Hàm `play()` trong composable làm gì? `status` chuyển từ `'idle'` sang giá trị nào? Cơ chế nào (interval/requestAnimationFrame) tự động tăng `currentIndex` theo `speed`?

4. **UI Render (3 panel):** `PseudocodePanel` highlight dòng code số mấy dựa vào gì? `CanvasArea` render frame tương ứng với `steps[currentIndex]` thế nào? `ExplainPanel` hiển thị text gì? Nút Play đổi thành nút gì sau khi nhấn?

---

**Câu 7:** Trace luồng khi **user đã đăng nhập** nhấn nút **Star** lần đầu (thêm vào yêu thích) rồi **nhấn lại Star** (xóa khỏi yêu thích) trên simulator đang chạy `graph.bfs`.

**Lần 1 — Thêm yêu thích:**

1. **UI (Component):** Biến reactive nào (`isFavorited`?) theo dõi trạng thái? Condition check trước khi gọi API là gì? `loading` của nút Star thay đổi thế nào để tránh double-click?

2. **FE Data Layer:** Hàm nào được gọi? Viết URL, HTTP Method, và Payload (bao gồm identifier của simulator). Header xác thực gửi thế nào?

3. **Backend (`FavoritesController`):** Action method nào? Bảng DB nào được INSERT? Trường nào được lưu (userId, simulatorKey/Id, createdAt)?  Response HTTP status và body?

4. **UI Render:** Icon Star thay đổi từ dạng nào sang dạng nào (outline/filled)? `isFavorited` được set thế nào? Toast message hiển thị gì?

**Lần 2 — Xóa yêu thích:**

1. **UI:** Điều kiện gì xác định click này là "xóa" (không phải "thêm")? Handler phân nhánh thế nào?

2. **FE Data Layer:** Hàm API nào? HTTP Method khác gì so với lần 1? URL có mang identifier gì?

3. **Backend:** Logic xóa — hard delete hay soft delete? Kiểm tra ownership (chỉ user của mình mới xóa được)?

4. **UI Render:** Icon Star, `isFavorited`, và Toast message thay đổi thế nào?
