# Đề 02 — HomeView & Public Demo

**Thời gian:** 25 phút | **Tổng điểm:** 10 điểm
**Bao phủ:** `HomeView.vue` · `v-reveal directive` · `CATALOG` · Demo routing · Router guard (`demoAllowed`)

---

## PHẦN I — TRẮC NGHIỆM (5 câu × 1 điểm = 5 điểm)

**Câu 1:** Directive `v-reveal` trong `HomeView.vue` sử dụng API trình duyệt nào để phát hiện phần tử vào viewport?

A. `MutationObserver`
B. `ResizeObserver`
C. `IntersectionObserver` với `threshold: 0.12`
D. `requestAnimationFrame` polling mỗi 16ms

---

**Câu 2:** Directive `v-reveal` KHÔNG chạy hiệu ứng fade+slide trong trường hợp nào? (Chọn câu mô tả ĐÚNG nhất bao gồm cả hai điều kiện)

A. Screen width < 768px hoặc user chưa đăng nhập
B. `prefers-reduced-motion` được bật trên OS hoặc môi trường test jsdom không có `IntersectionObserver`
C. Kết nối mạng chậm hoặc CPU throttling được bật
D. Trình duyệt không hỗ trợ CSS `transform`

---

**Câu 3:** Theo source code `HomeView.vue`, số lượng mô phỏng hiển thị trong phần **stats** là bao nhiêu và lấy từ đâu?

A. 44 mô phỏng — gọi API `GET /api/simulations/count` từ backend mỗi lần render
B. 44 mô phỏng — số liệu tĩnh (`static`) từ `CATALOG` theo SDD §19.6A, không gọi API
C. Số động — đếm realtime từ database qua WebSocket
D. 100 mô phỏng — hardcoded string trong template HTML

---

**Câu 4:** Ba demo công khai trong `HomeView.vue` được lọc và render bằng logic nào?

A. Đọc từ file `.env` với `VITE_PUBLIC_DEMOS=sort.bubble,search.binary,graph.bfs`
B. `CATALOG.filter(c => c.demoAllowed === true)` — field `demoAllowed` trong từng entry của CATALOG
C. Gọi API `GET /api/demos/public` từ backend trả về danh sách
D. Hardcode mảng `['sort.bubble', 'search.binary', 'graph.bfs']` trực tiếp trong template

---

**Câu 5:** Một **guest** (chưa đăng nhập) click vào demo **"Bubble Sort"** trên trang Home. Điều gì xảy ra tiếp theo?

A. Redirect đến `/login?redirect=/simulator/sort.bubble` — bắt buộc đăng nhập trước
B. Điều hướng đến `/simulator/sort.bubble` — không cần xác thực vì `sort.bubble` có `demoAllowed: true` trong CATALOG
C. Mở `/demo/bubble-sort` — route riêng dành cho trang demo công khai
D. Hiển thị modal preview inline ngay tại HomeView, không điều hướng sang trang khác

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG (2 câu × 2.5 điểm = 5 điểm)

> Yêu cầu viết rõ **4 chặng** cho mỗi câu

---

**Câu 6:** Trace luồng khi trang `HomeView.vue` **lần đầu được render** cho một **guest** (chưa đăng nhập). Mô tả cụ thể:

1. **Thao tác UI:** Lifecycle hook nào chạy đầu tiên (`onMounted`/`onBeforeMount`)? `v-reveal` directive khởi tạo `IntersectionObserver` thế nào? Section nào hiển thị với guest (CTA đăng ký), section nào ẩn (dashboard, history)?
2. **Frontend Data Layer:** Có gọi API nào để lấy `stats` hay `demoList` không? Data này lấy từ đâu? CATALOG được import như thế nào (static import hay dynamic)?
3. **Backend Layer:** Backend có bị gọi trong quá trình render HomeView của guest không? Giải thích rõ tại sao có hoặc không.
4. **UI Render:** `demoList` render 3 card với icon từ đâu (`DEMO_ICONS` record)? Nút CTA hiện "Bắt đầu miễn phí" hay "Vào Dashboard"? Stats "44 mô phỏng" hiện ở section nào?

---

**Câu 7:** Trace luồng khi **guest click demo "Binary Search"** từ HomeView đến khi mô phỏng **thực sự chạy được** trong SimulatorView.

Yêu cầu trace đầy đủ:

1. **UI (HomeView → Router):** Event handler nào bắt click? `router.push()` với object params gì? Route name/path nào được kích hoạt?
2. **Router Guard:** Khi gặp route `/simulator/search.binary`, guard kiểm tra `requiresAuth` hay không? Điều kiện `demoAllowed` được kiểm tra ở đâu trong `index.ts`? Guard quyết định như thế nào (cho qua hay redirect)?
3. **SimulatorView khởi tạo:** Component mount, `useSimulation(key)` nhận `key = 'search.binary'`. API nào được gọi để fetch algorithm data? URL, Method, Response chứa gì (steps array)?
4. **UI Render (SimulatorView):** 3 panel (PseudocodePanel, CanvasArea, ExplainPanel) hiển thị nội dung gì ban đầu? ControlBar ở trạng thái `status = 'idle'` hay `'paused'`? Nút **Star** có hiển thị với guest không và tại sao?
