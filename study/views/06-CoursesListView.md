# 🗺️ VIEW 06: DANH SÁCH LỘ TRÌNH (COURSESLISTVIEW)

* **Tên file Vue**: [`CoursesListView.vue`](file:///d:/FPT/metqua/frontend/src/views/courses/CoursesListView.vue)
* **Đường dẫn URL**: `/path` (Alias: `/learn`, `/courses`)
* **Route Name**: `path-list`
* **Quyền truy cập**: Công khai (`meta: { public: true }`). Học viên đăng nhập sẽ thấy thêm tiến độ cá nhân.

---

## 1. CẤU TRÚC GIAO DIỆN & BỘ LỌC ĐA NĂNG

```
┌────────────────────────────────────────────────────────────────────────┐
│  LỘ TRÌNH HỌC DSA          [ Cấp độ của bạn: Cấp 4 | Tích lũy: 350 XP ]│
├────────────────────────────────────────────────────────────────────────┤
│ <CourseFilter />:                                                      │
│ 1. Thanh tìm kiếm: [ 🔍 Tìm bài học, giải thuật... ]                    │
│ 2. Tab nhóm chủ đề: [ Tất cả (15) ] [ Sắp xếp ] [ Cây ] [ Đồ thị ]...   │
│ 3. Bộ lọc cấp độ: [ Tất cả trình độ ▾ | Cơ bản | Trung cấp | Nâng cao ]│
├────────────────────────────────────────────────────────────────────────┤
│ KHU VỰC HIỂN THỊ CÁC THẺ LỘ TRÌNH (COURSE CARDS):                      │
│                                                                        │
│ ┌──────────────────────┐  ┌──────────────────────┐                     │
│ │ 🔀 SẮP XẾP & TÌM KIẾM│  │ 🌲 CÂY NHỊ PHÂN & BST│                     │
│ │ Chủ đề: Cơ bản       │  │ Chủ đề: Trung cấp    │                     │
│ │ 12 Bài học • 340 XP  │  │ 8 Bài học • 220 XP   │                     │
│ │ Tiến độ: [████░░] 60%│  │ Tiến độ: [░░░░░░] 0% │                     │
│ │ [ Tiếp tục học → ]   │  │ [ Mở khóa (-1❤️) ]   │                     │
│ └──────────────────────┘  └──────────────────────┘                     │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 2. CHI TIẾT CÁC LUỒNG HOẠT ĐỘNG (FLOWS)

### 🔹 Flow 1: Khởi tạo và Tải danh sách lộ trình
1. Khi Component mounted, Pinia Store gọi song song:
   * `GET /api/v1/path-items`: Lấy danh sách toàn bộ các lộ trình học.
   * `GET /api/v1/topics`: Lấy danh mục 10 nhóm chủ đề lớn.
   * `GET /api/v1/progress/summary` (nếu đã đăng nhập): Lấy % hoàn thành của từng lộ trình.
2. Dữ liệu được cache trong `courseStore` để chuyển tab không bị load lại.

### 🔹 Flow 2: Lọc và Tìm kiếm theo thời gian thực (Client-side Reactive Filter)
* **Lọc theo chủ đề**: Nhấp vào tab "Cây" $\rightarrow$ `courseStore.setTopic('Cây')` $\rightarrow$ Computed `filteredCourses` tự động lọc các lộ trình thuộc chủ đề Cây.
* **Lọc theo độ khó**: Chọn "Cơ bản" / "Trung cấp" / "Nâng cao".
* **Tìm kiếm**: Gõ từ khóa $\rightarrow$ Tự động tìm kiếm không phân biệt hoa thường theo Tên lộ trình, Mô tả và Tags.

### 🔹 Flow 3: Điều hướng vào chi tiết lộ trình
* Nhấp vào bất kỳ thẻ lộ trình nào $\rightarrow$ Chuyển sang `/path/:id` ([`CourseDetailView.vue`](file:///d:/FPT/metqua/frontend/src/views/courses/CourseDetailView.vue)).

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend View**: [`CoursesListView.vue`](file:///d:/FPT/metqua/frontend/src/views/courses/CoursesListView.vue)
* **Frontend Component**: `src/components/courses/CourseFilter.vue`, `src/components/courses/CourseCard.vue`
* **Frontend Store**: `src/stores/courses.ts`
* **Backend Controller**: [`PathItemsController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/PathItemsController.cs), [`TopicsController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/TopicsController.cs)
* **Backend Service**: [`PathItemService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/PathItemService.cs), [`TopicService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/TopicService.cs)
