# Đề 06 — Lộ trình học & Bài học
**Thời gian:** 25 phút | **Tổng điểm:** 10 điểm
**Bao phủ:** CoursesListView.vue · CourseDetailView.vue · LessonStudyView.vue

---

## PHẦN I — TRẮC NGHIỆM (5 câu × 1 điểm = 5 điểm)

**Câu 1:** Route `/path` (component `CoursesListView.vue`) được cấu hình với meta nào, cho phép điều gì?

A. `meta: { requiresAuth: true }` — chỉ user đã đăng nhập mới xem được danh sách lộ trình
B. `meta: { public: true }` — khách (guest) có thể xem danh sách lộ trình mà không cần đăng nhập
C. `meta: { requiresRole: 'student' }` — chỉ student mới xem được
D. Không có meta — route không được phân loại

---

**Câu 2:** Trong `CourseDetailView.vue` (route `/path/:id`, 655 dòng), nút **"Tham gia lộ trình"** xuất hiện trong điều kiện nào?

A. `v-if="courseStore.isEnrolled(course.id)"` — khi đã tham gia
B. `v-if="!courseStore.isEnrolled(course.id)"` — khi chưa tham gia
C. `v-if="authStore.isAuthenticated"` — khi đã đăng nhập
D. Luôn hiển thị bất kể trạng thái đăng ký

---

**Câu 3:** Khi người dùng đã đăng ký lộ trình và nhấn nút **"Bắt đầu học"** trong `CourseDetailView.vue`, hành động nào xảy ra?

A. `showRegisterModal = true` → hiện modal đăng ký
B. `router.push('/path')` → quay về danh sách lộ trình
C. `startLesson(course.lessons[0])` → `router.push('/lessons/:id')` đến bài học đầu tiên
D. `router.push('/simulations')` → chuyển sang màn khám phá

---

**Câu 4:** Phần **header stats** trong `CourseDetailView.vue` hiển thị 4 chỉ số nào?

A. Tên khóa / Tác giả / Ngày tạo / Lượt xem
B. Bài học / Quiz / Lab / XP
C. Tổng giờ học / Số học viên / Rating / Giá
D. Completed / In Progress / Not Started / Total

---

**Câu 5:** Trong hệ thống redirect của Router, route `/learn` được redirect về đâu?

A. `/simulations`
B. `/dashboard`
C. `/path`
D. `/lessons`

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG (2 câu × 2.5 điểm = 5 điểm)

> **Yêu cầu chung:** Viết đủ **4 chặng**: `1. UI` → `2. FE Data` → `3. Backend` → `4. UI Render`
> Dùng tên hàm, component, store, API endpoint cụ thể từ source code.

---

**Câu 6:** Trace luồng khi **khách (guest chưa đăng nhập)** vào `/path`, xem danh sách lộ trình, rồi click vào một lộ trình cụ thể để xem chi tiết.

Gợi ý: Đề cập đến sự khác biệt meta giữa `/path` và `/path/:id`, khi nào guard chặn và redirect.

Viết 4 chặng:
1. **UI** — Guest vào `/path` có bị chặn không; component nào render danh sách
2. **FE Data** — Dữ liệu danh sách lộ trình load từ đâu; store nào quản lý
3. **Backend** — API endpoint nào trả về danh sách; response format ra sao
4. **UI Render** — Sau khi click vào `/path/:id`, điều gì xảy ra với guest

---

**Câu 7:** Trace luồng khi học sinh **hoàn thành một bài học** trong `LessonStudyView.vue`.

Gợi ý: Đề cập đến `LessonCompletionModal`, `progressStore`, `gamificationStore`, và API cập nhật progress.

Viết 4 chặng:
1. **UI** — Hành động nào đánh dấu hoàn thành; modal nào xuất hiện
2. **FE Data** — `progressStore` và `gamificationStore` được cập nhật thế nào
3. **Backend** — API nào được gọi để lưu progress; `LessonsController` xử lý gì
4. **UI Render** — Sau khi đóng modal, UI thay đổi gì (XP bar, progress, nút Next)
