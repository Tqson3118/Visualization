# 👤 VIEW 15: HỒ SƠ CÁ NHÂN & THÀNH TÍCH (PROFILEVIEW)

* **Tên file Vue**: [`ProfileView.vue`](file:///d:/FPT/metqua/frontend/src/views/ProfileView.vue)
* **Đường dẫn URL**: `/profile` (Alias: `/dashboard`)
* **Route Name**: `profile`
* **Quyền truy cập**: Đã đăng nhập (`requiresAuth: true`).

---

## 1. CẤU TRÚC GIAO DIỆN & 6 SUB-TABS

```
┌────────────────────────────────────────────────────────────────────────┐
│ [ Avatar + Khung viền ]   NGUYỄN VĂN A   [ Cấp 4 • 420 XP ]  [🔥 7d]   │
├────────────────────────────────────────────────────────────────────────┤
│ 6 TABS QUẢN LÝ THÔNG TIN:                                              │
│ [ 📊 Tổng quan ] [ 📈 Tiến độ học ] [ 🏆 Huy hiệu ]                    │
│ [ 🎒 Túi đồ ]    [ 💬 Phản hồi ]    [ ⚙️ Cài đặt ]                     │
├────────────────────────────────────────────────────────────────────────┤
│ NỘI DUNG TAB TỔNG QUAN:                                                │
│ 1. Thống kê Hero: 12 Bài đã học | 5 Bài tập giải | 20 Lượt chạy Lab    │
│ 2. Lưới đóng góp hoạt động học tập hàng ngày (Activity Heatmap)        │
│ 3. Huy hiệu mới nhận gần đây (Recent Badges)                           │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 2. CHI TIẾT CÁC LUỒNG HOẠT ĐỘNG (FLOWS)

### 🔹 Flow 1: Nạp hồ sơ và trang bị cá nhân
1. Khi Component mounted, gọi đồng thời:
   * `auth.fetchMe()`: Lấy thông tin tài khoản (Họ tên, Email, Role).
   * `gamification.fetchAll()`: Lấy Level, XP, số Tim, số Ngọc, Túi đồ (`UserInventory`).
   * `progressStore.fetchOverview()`: Lấy số liệu % tiến độ các khóa học.
2. Tự động gắn Avatar và Khung viền (`equippedAvatar`, `equippedFrame`) đang được kích hoạt từ `UserInventory`.

### 🔹 Flow 2: Trang bị vật phẩm từ Túi đồ (Inventory Equip Flow)
* Trong Tab **Túi đồ (Inventory)**: Người học xem danh sách Khung viền Avatar đã mua từ Shop $\rightarrow$ Bấm *"Trang bị"* $\rightarrow$ Gọi `POST /api/v1/gamification/inventory/{id}/equip` $\rightarrow$ Avatar đổi khung viền phát sáng ngay lập tức.

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend View**: [`ProfileView.vue`](file:///d:/FPT/metqua/frontend/src/views/ProfileView.vue)
* **Frontend Tabs**:
  * `src/components/profile/ProfileOverviewTab.vue`
  * `src/components/profile/ProfileProgressTab.vue`
  * `src/components/profile/ProfileInventoryTab.vue`
  * `src/components/profile/ProfileAchievementsTab.vue`
  * `src/components/profile/ProfileFeedbackTab.vue`
  * `src/components/profile/ProfileSettingsTab.vue`
* **Backend Controller**: [`MeController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/MeController.cs), [`GamificationController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/GamificationController.cs)
* **Backend Service**: [`UserService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/UserService.cs), [`GamificationService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/GamificationService.cs)
