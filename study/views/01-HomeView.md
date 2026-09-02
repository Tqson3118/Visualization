# 🏠 VIEW 01: TRANG CHỦ CÔNG KHAI (HOMEVIEW)

* **Tên file Vue**: [`HomeView.vue`](file:///d:/FPT/metqua/frontend/src/views/HomeView.vue)
* **Đường dẫn URL**: `/`
* **Route Name**: `home`
* **Quyền truy cập**: Công khai cho tất cả người dùng (`Public`).

---

## 1. CẤU TRÚC COMPONENT & GIAO DIỆN (UI BREAKDOWN)

`HomeView.vue` được chia thành 5 module component chuyên biệt:
```
HomeView.vue
├── <HeroSection />    -> Tiêu đề chính, nút CTA bắt đầu học, Canvas mô phỏng mini tự động
├── <StatsBar />       -> Thanh số liệu: 44+ Thuật toán, 100% Trực quan, 10 Lộ trình
├── <DemoGrid />       -> Lưới 3 thuật toán demo công khai (Bubble Sort, Binary Search, BFS)
├── <FeatureGrid />    -> 4 trụ cột tính năng: Interactive Visualizer, CodeLab, Gamification, Class
└── <HomeCtaBand />    -> Dải banner kêu gọi hành động cuối trang ("Sẵn sàng làm chủ DSA?")
```

---

## 2. CHI TIẾT CÁC LUỒNG HOẠT ĐỘNG (FLOWS)

### 🔹 Flow 1: Khởi tạo khi vào trang (Mounting & Mini Simulation)
1. Người dùng truy cập URL `/`.
2. Component `<HeroSection />` khởi chạy:
   * Đọc danh mục demo từ `src/engines/catalog.ts` (lấy 3 key: `sort.bubble`, `search.binary`, `graph.bfs`).
   * Tự động khởi chạy một vòng lặp Canvas mini (chạy 5 bước rồi reset nhẹ nhàng) để người dùng thấy ngay hiệu ứng trực quan mà không cần đăng nhập hay bấm nút.
3. Không yêu cầu gọi API Backend bắt buộc (tải trang cực nhanh, tối ưu SEO và trải nghiệm đầu tiên).

### 🔹 Flow 2: Nhấp nút "Bắt đầu học ngay" (Primary CTA Click)
* **Người dùng bấm**: Nút *"Bắt đầu học ngay"* trên HeroSection.
* **Xử lý Vue**: Gọi `router.push('/path')`.
* **Kết quả**: Điều hướng người dùng sang Màn hình Danh sách Lộ trình ([`CoursesListView.vue`](file:///d:/FPT/metqua/frontend/src/views/courses/CoursesListView.vue)).

### 🔹 Flow 3: Thử nghiệm Demo Card trên DemoGrid
* **Người dùng bấm**: Thẻ card *"Binary Search"* trên DemoGrid.
* **Xử lý Vue**: Điều hướng sang `/simulator/search.binary`.
* **Phân quyền**: Vì `search.binary` có cờ `demoAllowed: true` trong `catalog.ts`, khách chưa đăng nhập vẫn được trải nghiệm toàn bộ tính năng của Simulator mà không bị chặn.

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend View**: [`HomeView.vue`](file:///d:/FPT/metqua/frontend/src/views/HomeView.vue)
* **Frontend Components**:
  * `src/components/home/HeroSection.vue`
  * `src/components/home/StatsBar.vue`
  * `src/components/home/DemoGrid.vue`
  * `src/components/home/FeatureGrid.vue`
  * `src/components/home/HomeCtaBand.vue`
* **Catalog Data**: [`frontend/src/engines/catalog.ts`](file:///d:/FPT/metqua/frontend/src/engines/catalog.ts)
