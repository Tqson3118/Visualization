# ⚙️ VIEW 26: CÀI ĐẶT NỀN TẢNG HỆ THỐNG (ADMINSETTINGSVIEW)

* **Tên file Vue**: [`AdminSettingsView.vue`](file:///d:/FPT/metqua/frontend/src/views/AdminSettingsView.vue)
* **Đường dẫn URL**: `/admin/settings`
* **Route Name**: `admin-settings`
* **Quyền truy cập**: Chỉ Quản trị viên (`roles: ['ADMIN']`).

---

## 1. CẤU TRÚC GIAO DIỆN & 3 TABS THIẾT LẬP

```
┌────────────────────────────────────────────────────────────────────────┐
│  ⚙️ CÀI ĐẶT NỀN TẢNG (PLATFORM SETTINGS)                               │
│  [ Tab 1: Cấu hình hệ thống ]  [ Tab 2: Gamification ]  [ Tab 3: Báo cáo ]│
├────────────────────────────────────────────────────────────────────────┤
│ TAB 2: THIẾT LẬP THAM SỐ GAMIFICATION:                                 │
│                                                                        │
│ • Số Tim tối đa mặc định:        [ 5 ] Tim                             │
│ • Thời gian phục hồi 1 Tim:      [ 30 ] Phút                           │
│ • Điểm kinh nghiệm (XP) Bài học: [ 20 ] XP                             │
│ • Điểm kinh nghiệm (XP) Bài tập: [ 10 ] XP                             │
│ • Điểm thưởng Streak mỗi ngày:   [ 5 ] Gems                            │
│                                                                        │
│ [ 💾 LƯU CẤU HÌNH GAMIFICATION ]                                       │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 2. CHI TIẾT CÁC LUỒNG HOẠT ĐỘNG (FLOWS)

### 🔹 Flow 1: Cập nhật tham số Gamification
1. Admin điều chỉnh các tham số số Tim, thời gian hồi Tim, hệ số nhân XP.
2. Bấm nút **"Lưu cấu hình"**.
3. Gửi `PUT /api/v1/admin/gamification/settings { maxHearts, heartRegenMinutes, lessonXp... }`.
4. Backend [`GamificationConfigService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/GamificationConfigService.cs) lưu vào bảng `Settings` và xóa Cache để áp dụng cho toàn bộ người dùng ngay lập tức.

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend View**: [`AdminSettingsView.vue`](file:///d:/FPT/metqua/frontend/src/views/AdminSettingsView.vue)
* **Backend Controller**: [`SettingsController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/SettingsController.cs), [`AdminGamificationController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/AdminGamificationController.cs)
* **Backend Service**: [`SettingService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/SettingService.cs)
* **Database Entity**: [`Setting.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Persistence/Entities/Setting.cs)
