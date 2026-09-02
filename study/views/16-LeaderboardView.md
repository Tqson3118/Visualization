# 🏆 VIEW 16: BẢNG XẾP HẠNG (LEADERBOARDVIEW)

* **Tên file Vue**: [`LeaderboardView.vue`](file:///d:/FPT/metqua/frontend/src/views/LeaderboardView.vue)
* **Đường dẫn URL**: `/leaderboard`
* **Route Name**: `leaderboard`
* **Quyền truy cập**: Đã đăng nhập (`requiresAuth: true`).

---

## 1. CẤU TRÚC GIAO DIỆN & TOP 3 PODIUM

```
┌────────────────────────────────────────────────────────────────────────┐
│  🏆 BẢNG XẾP HẠNG THÀNH TÍCH (LEADERBOARD)                             │
│  [ Tab 1: Tuần này ]        [ Tab 2: Level ]        [ Tab 3: Theo Lớp ]│
├────────────────────────────────────────────────────────────────────────┤
│ KHU VỰC BỤC VINH QUANG TOP 3 (PODIUM):                                 │
│                                                                        │
│                🥇 TOP 1 (Huy hoàng)                                    │
│                 [ Avatar + Khung ]                                     │
│                 Nguyễn Văn A • 1,250 XP                                │
│                                                                        │
│     🥈 TOP 2 (Bạc)                   🥉 TOP 3 (Đồng)                   │
│     Trần Thị B • 980 XP              Lê Văn C • 870 XP                 │
├────────────────────────────────────────────────────────────────────────┤
│ DANH SÁCH XẾP HẠNG TOP 4 - TOP 50:                                     │
│ 4. Phạm D   [ Avatar ]   Level 5   🔥 12d Streak   750 XP              │
│ 5. Hoàng E  [ Avatar ]   Level 4   🔥 5d Streak    620 XP              │
├────────────────────────────────────────────────────────────────────────┤
│ [ GHIM VỊ TRÍ CỦA BẠN: #14 Nguyễn Văn F • 320 XP • 🔥 3d ]             │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 2. CHI TIẾT CÁC LUỒNG HOẠT ĐỘNG (FLOWS)

### 🔹 Flow 1: Tải Bảng xếp hạng theo các chế độ (Switching Tabs)
1. Mặc định tải theo Tuần: Gọi `GET /api/v1/gamification/leaderboard?type=week`.
2. Khi chuyển sang tab **"Level"**: Gọi `GET /api/v1/gamification/leaderboard?type=level`.
3. Khi chuyển sang tab **"Theo Lớp"**:
   * Kiểm tra người dùng có tham gia lớp học nào không (`classStore.fetchClasses()`).
   * Nếu có lớp: Gọi `GET /api/v1/gamification/leaderboard?type=class&classId={selectedClassId}`.

### 🔹 Flow 2: Ghim vị trí của người dùng hiện tại (Sticky Rank)
* Hệ thống tự động tính toán thứ hạng của `currentUserId` trong danh sách.
* Nếu người dùng không nằm trong Top đầu nhìn thấy trên màn hình, một thanh cố định ở đáy trang sẽ hiện: `Thứ hạng của bạn: #14 | 320 XP`.

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend View**: [`LeaderboardView.vue`](file:///d:/FPT/metqua/frontend/src/views/LeaderboardView.vue)
* **Frontend Store**: [`leaderboard.ts`](file:///d:/FPT/metqua/frontend/src/stores/leaderboard.ts)
* **Backend Controller**: [`GamificationController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/GamificationController.cs)
* **Backend Service**: [`GamificationService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/GamificationService.cs)
