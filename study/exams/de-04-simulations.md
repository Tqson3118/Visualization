# Đề 04 — Simulations, Benchmark & CheatSheet
**Thời gian:** 25 phút | **Tổng điểm:** 10 điểm
**Bao phủ:** SimulationsView.vue · BenchmarkView.vue · CheatSheetTable.vue

---

## PHẦN I — TRẮC NGHIỆM (5 câu × 1 điểm = 5 điểm)

**Câu 1:** Trong `SimulationsView.vue` (route `/simulations`), màn "Khám phá" (Màn 33) có mấy tab chính?

A. 1 tab (Danh mục)
B. 2 tab (Danh mục và CheatSheet)
C. 3 tab (Danh mục, CheatSheet và Benchmark)
D. 4 tab (Danh mục, CheatSheet, Benchmark và Sandbox)

---

**Câu 2:** Các card thuật toán trong tab **Danh mục** được gom nhóm dựa theo tiêu chí nào?

A. Theo độ phức tạp Big-O (O(1), O(n), O(n log n)...)
B. Theo prefix của **key** thuật toán (sort./ search./ stack./ queue./ ...)
C. Theo ngôn ngữ lập trình (JavaScript, Python, C++)
D. Theo thứ tự alphabet của tên thuật toán

---

**Câu 3:** Chip hiển thị Big-O trên card thuật toán có màu sắc tương ứng như thế nào?

A. `O(n log n)` → danger (đỏ); `O(n²)` → warning (vàng); `O(n³+)` → success (xanh)
B. `O(n log n)` → success (xanh); `O(n²)` → warning (vàng); `O(n³+)` → danger (đỏ)
C. Tất cả chip đều cùng màu primary (xanh dương)
D. Màu chip được lấy ngẫu nhiên mỗi lần render

---

**Câu 4:** Component `CheatSheetTable` được import vào `SimulationsView.vue` bằng phương thức nào?

A. Import tĩnh thông thường: `import CheatSheetTable from '@/components/lesson/CheatSheetTable.vue'`
B. Lazy-load qua `defineAsyncComponent(() => import('@/components/lesson/CheatSheetTable.vue'))`
C. Lazy-load qua `React.lazy()` kết hợp `Suspense`
D. Import từ CDN bên ngoài qua `<script src="...">`

---

**Câu 5:** Route của `BenchmarkView` nhận tham số gì và yêu cầu điều kiện gì để truy cập?

A. `/benchmark/:id` — không cần đăng nhập
B. `/benchmark/:k1/:k2` — `requiresAuth: true`
C. `/benchmark?algo1=&algo2=` — không cần đăng nhập
D. `/benchmark/:id` — `requiresAuth: true`

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG (2 câu × 2.5 điểm = 5 điểm)

> **Yêu cầu chung:** Với mỗi câu, viết đủ **4 chặng**: `1. UI` → `2. FE Data` → `3. Backend` → `4. UI Render`
> Sử dụng tên hàm, component, store, API endpoint cụ thể (không viết chung chung).

---

**Câu 6:** Trace luồng khi người dùng **nhấn vào tab "CheatSheet"** trong `SimulationsView.vue` lần đầu tiên.

Gợi ý: Đề cập đến cơ chế lazy-load, trạng thái loading, và cách dữ liệu cheat sheet được hiển thị.

Viết 4 chặng:
1. **UI** — Hành động của người dùng và component nào phản hồi
2. **FE Data** — Quá trình tải component/dữ liệu phía frontend
3. **Backend** — API nào (nếu có) được gọi để lấy dữ liệu
4. **UI Render** — Component nào render kết quả cuối cùng

---

**Câu 7:** Trace luồng khi người dùng **truy cập `/benchmark/sort.bubble/sort.merge`** để so sánh hai thuật toán.

Gợi ý: Đề cập đến router guard, cách hai key được parse, quá trình chạy benchmark và hiển thị kết quả.

Viết 4 chặng:
1. **UI** — Route guard kiểm tra gì; component nào được mount
2. **FE Data** — Hai key `k1`, `k2` được đọc ra sao; dữ liệu thuật toán lấy từ đâu
3. **Backend** — API hoặc worker nào thực thi benchmark
4. **UI Render** — Kết quả so sánh được render thế nào (chart, bảng, highlight)
