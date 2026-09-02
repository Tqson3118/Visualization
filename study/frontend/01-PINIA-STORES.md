# 🍍 TÀI LIỆU PINIA STORES — STATE MANAGEMENT LAYER

Hệ thống sử dụng **Pinia** (chuẩn mới nhất của Vue 3) để quản lý trạng thái tập trung. Các store được đặt tại `frontend/src/stores/`, thiết kế theo phong cách Setup Stores (`defineStore` với arrow function).

---

## 📋 DANH SÁCH 9 PINIA STORES CHI TIẾT

### 1. [`auth.ts`](file:///d:/FPT/metqua/frontend/src/stores/auth.ts) — Quản lý Phiên Đăng Nhập & Phân Quyền
* **State**:
  * `token`: Chuỗi JWT Access Token (được lưu trong memory và đồng bộ localStorage).
  * `user`: Thông tin người dùng hiện tại (`id`, `username`, `email`, `role`, `displayName`, `avatarUrl`, `level`, `xp`).
* **Getters**:
  * `isAuthenticated`: `computed(() => !!token.value && !!user.value)`
  * `role`: `computed(() => user.value?.role ?? null)`
* **Actions**:
  * `login(email, password)`: Gọi API login, lưu token và profile vào state.
  * `logout()`: Xóa token, reset state và điều hướng về `/login`.
  * `fetchMe()`: Cập nhật thông tin profile mới nhất từ `GET /api/v1/me`.

### 2. [`simulation.ts`](file:///d:/FPT/metqua/frontend/src/stores/simulation.ts) — Quản lý VCR Player Mô Phỏng
* **State**:
  * `currentSim`: Thông tin metadata của thuật toán đang chọn.
  * `steps`: Mảng toàn bộ các bước thực thi `TraceStep[]` sinh ra từ AST Engine.
  * `currentIndex`: Chỉ số bước hiện tại (`0 <= currentIndex < steps.length`).
  * `status`: Trạng thái máy phát (`'idle' | 'running' | 'paused' | 'finished'`).
  * `speed`: Tốc độ chạy tự động (mili-giây giữa 2 bước, ví dụ: 500ms).
* **Actions**:
  * `loadSimulation(key, customInput)`: Nạp thuật toán và chạy AST Executor để sinh mảng `steps`.
  * `play()` / `pause()`: Chạy tự động hoặc tạm dừng timer.
  * `stepForward()`: Tăng `currentIndex++` và cập nhật highlight code.
  * `stepBack()`: Lùi `currentIndex--`.
  * `jumpTo(index)`: Nhảy thẳng tới một bước bất kỳ trên thanh Scrubber.

### 3. [`lesson.ts`](file:///d:/FPT/metqua/frontend/src/stores/lesson.ts) & [`progress.ts`](file:///d:/FPT/metqua/frontend/src/stores/progress.ts)
* **State**:
  * `currentLesson`: Chi tiết bài học đang xem (Markdown, SandboxConfig, Note cá nhân).
  * `courseModules`: Danh sách các chương và bài học trong lộ trình.
  * `overview`: Tổng số bài đã học, số bài tập đã giải, % hoàn thành khóa học.
* **Actions**:
  * `fetchLesson(lessonId)`: Nạp nội dung bài học.
  * `completeLesson(lessonId)`: Gửi request hoàn thành bài $\rightarrow$ Cập nhật `isCompleted = true` và gỡ khóa (`isLocked = false`) bài tiếp theo.

### 4. [`gamification.ts`](file:///d:/FPT/metqua/frontend/src/stores/gamification.ts) — Quản lý Tim, Ngọc & Túi Đồ
* **State**:
  * `hearts`: Số Tim hiện có (mặc định 5).
  * `gems`: Số dư Ngọc.
  * `xp`: Điểm kinh nghiệm & `level`: Cấp độ hiện tại.
  * `streak`: Chuỗi ngày học liên tiếp (`🔥 X ngày`).
  * `quests`: Danh sách 3-5 nhiệm vụ hàng ngày.
  * `inventory`: Kho đồ cá nhân chứa Khung viền và Avatar đã mua.
* **Actions**:
  * `fetchAll()`: Nạp đồng thời toàn bộ chỉ số game từ server.
  * `claimQuest(questId)`: Nhận thưởng nhiệm vụ $\rightarrow$ Tăng Ngọc và XP.
  * `buyShopItem(itemId)`: Mua vật phẩm $\rightarrow$ Trừ Ngọc và thêm vào Túi đồ.
  * `equipItem(inventoryId)`: Kích hoạt Khung viền cho Avatar.

### 5. [`classStore.ts`](file:///d:/FPT/metqua/frontend/src/stores/classStore.ts) — Quản lý Lớp Học
* **State**:
  * `classes`: Danh sách các lớp học người dùng tham gia hoặc quản lý.
  * `currentClass`: Chi tiết lớp học đang chọn (Thành viên, Bài tập được giao).
* **Actions**:
  * `fetchClasses()`: Lấy danh sách lớp học.
  * `joinClass(code)`: Tham gia lớp học bằng mã mời.
  * `createClass(name, desc)`: Giảng viên tạo lớp học mới.

### 6. [`codeRunner.ts`](file:///d:/FPT/metqua/frontend/src/stores/codeRunner.ts) & [`leaderboard.ts`](file:///d:/FPT/metqua/frontend/src/stores/leaderboard.ts)
* **`codeRunner`**: Quản lý code trong Monaco Editor, ngôn ngữ chọn (`js`, `cpp`, `java`), kết quả Terminal và thời gian thực thi.
* **`leaderboard`**: Quản lý bảng xếp hạng theo Tuần, Level, Lớp và thứ hạng cá nhân `myRank`.

### 7. [`ui.ts`](file:///d:/FPT/metqua/frontend/src/stores/ui.ts) — Quản lý Trạng Thái Giao Diện
* **Actions**:
  * `showToast(message, type: 'success' | 'error' | 'warning' | 'info')`: Bắn thông báo Toast nổi trên góc màn hình.
  * `toggleSidebar()`: Ẩn/Hiện thanh menu bên trái.
