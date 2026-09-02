# 🏆 PHÂN HỆ 5: GAMIFICATION & KINH TẾ ẢO (GAMIFICATION & SHOP FLOW)

Phân hệ Gamification tạo động lực học tập liên tục cho sinh viên thông qua hệ thống **Tim (Mạng sống)**, **Ngọc (Gems)**, **Chuỗi ngày học (Streaks)**, **Nhiệm vụ hàng ngày (Daily Quests)** và **Cửa hàng vật phẩm (Shop)**.

---

## 1. MÀN HÌNH 32: HỒ SƠ CÁ NHÂN & TIẾN ĐỘ (PROFILE VIEW)

* **URL**: `/profile` (alias: `/dashboard`)
* **File Vue**: [`ProfileView.vue`](file:///d:/FPT/metqua/frontend/src/views/ProfileView.vue)
* **Quyền truy cập**: Đã đăng nhập (`requiresAuth: true`).

### Mắt thấy gì trên giao diện?
1. **Avatar & Khung viền (Avatar Frame)**: Người dùng có thể đổi avatar và gắn khung viền đã mua từ Shop.
2. **Chỉ số tổng thể (Hero Stats)**:
   * Level hiện tại (Cấp độ) & Thanh điểm kinh nghiệm $XP / XP_{\text{next}}$.
   * Số ngày học liên tục (Streak: ví dụ `🔥 7 ngày`).
   * Số bài học đã hoàn thành, số bài tập đã giải.
3. **Lưới đóng góp (Contribution Activity Heatmap)**: Tương tự GitHub, hiển thị mật độ học tập qua từng ngày trong năm.
4. **Huy hiệu đạt được (Achievements / Badges)**: "Bậc thầy Sắp xếp", "Chiến binh Đồ thị", "Học tập không ngừng nghỉ".

---

## 2. MÀN HÌNH 24: BẢNG XẾP HẠNG (LEADERBOARD VIEW)

* **URL**: `/leaderboard`
* **File Vue**: [`LeaderboardView.vue`](file:///d:/FPT/metqua/frontend/src/views/LeaderboardView.vue)
* **Quyền truy cập**: Đã đăng nhập (`requiresAuth: true`).

### Mắt thấy gì trên giao diện?
* **Top 3 Danh vọng (Podium)**: Hiển thị 3 học viên có điểm XP cao nhất với Cúp Vàng 🥇, Bạc 🥈, Đồng 🥉.
* **Bảng danh sách Top 50**: Cột Thứ hạng, Avatar + Khung, Tên người dùng, Chuỗi Streak và Tổng số XP.
* **Thanh ghim vị trí cá nhân (Sticky Current User Rank)**: Luôn hiển thị thứ hạng của chính bạn ở cuối màn hình để dễ theo dõi.
* **Bộ lọc thời gian**: Bảng xếp hạng Tuần này / Tháng này / Toàn thời gian (All-time).

---

## 3. MÀN HÌNH 23: NHIỆM VỤ HÀNG NGÀY (QUESTS VIEW)

* **URL**: `/quests`
* **File Vue**: [`QuestsView.vue`](file:///d:/FPT/metqua/frontend/src/views/QuestsView.vue)
* **Quyền truy cập**: Đã đăng nhập (`requiresAuth: true`).

### Các nhiệm vụ điển hình:
1. *"Hoàn thành 1 bài học bất kỳ"* $\rightarrow$ Thưởng `+10 XP`, `+5 Gems`.
2. *"Chạy thử 2 thuật toán trên Simulator"* $\rightarrow$ Thưởng `+15 XP`, `+10 Gems`.
3. *"Đạt điểm tối đa 1 bài tập trắc nghiệm"* $\rightarrow$ Thưởng `+20 XP`, `+15 Gems`.
* Khi thanh tiến trình đạt $100\%$ $\rightarrow$ Nút **"Nhận thưởng" (Claim)** phát sáng $\rightarrow$ Người dùng bấm nhận $\rightarrow$ Cộng Ngọc và nổ hiệu ứng confetti.

---

## 4. MÀN HÌNH 22: CỬA HÀNG VẬT PHẨM (SHOP VIEW)

* **URL**: `/shop`
* **File Vue**: [`ShopView.vue`](file:///d:/FPT/metqua/frontend/src/views/ShopView.vue)
* **Quyền truy cập**: Đã đăng nhập (`requiresAuth: true`).

```
┌────────────────────────────────────────────────────────────────────────┐
│ 🛒 CỬA HÀNG VẬT PHẨM                [ Số dư: 💎 450 Ngọc | ❤️ 3/5 Tim ] │
├────────────────────────────────────────────────────────────────────────┤
│ [ Tab: Tất cả ]  [ Tab: Hồi Tim ]  [ Tab: Avatar ]  [ Tab: Khung viền ]│
│                                                                        │
│ ┌──────────────────────┐  ┌──────────────────────┐  ┌────────────────┐ │
│ │ ❤️ HỒI ĐẦY 5 TIM     │  │ 🛡️ ĐÔNG BĂNG STREAK  │  │ 👑 KHUNG HOÀNG GIA│
│ │ Không cần chờ hồi giờ│  │ Giữ chuỗi khi quên học│  │ Hiệu ứng phát sáng│
│ │ Giá: 💎 100 Ngọc     │  │ Giá: 💎 200 Ngọc     │  │ Giá: 💎 500 Ngọc │
│ │ [ Mua ngay ]         │  │ [ Mua ngay ]         │  │ [ Mua ngay ]   │
│ └──────────────────────┘  └──────────────────────┘  └────────────────┘ │
└────────────────────────────────────────────────────────────────────────┘
```

### Luồng mua vật phẩm (Shop Purchase Flow):
1. Người dùng chọn vật phẩm $\rightarrow$ Bấm **Mua ngay**.
2. Frontend gọi `POST /api/v1/shop/buy { itemId }`.
3. Backend [`GamificationService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/GamificationService.cs):
   * Kiểm tra số dư Ngọc trong `UserProgress.Gems >= item.Price`.
   * Trừ Ngọc: `UserProgress.Gems -= item.Price`.
   * Thêm vật phẩm vào bảng `UserInventory`.
   * Nếu là gói nạp tim $\rightarrow$ Đặt lại `UserProgress.Hearts = 5`.
   * Ghi log giao dịch vào bảng `GemTransactions`.
4. Trả về thông báo thành công $\rightarrow$ Giao diện cập nhật số dư Ngọc tức thì.

---

## 5. MÀN HÌNH 25 & 27: GÓI PREMIUM & QUẢN LÝ ĐĂNG KÝ (PREMIUM & SUBSCRIPTION)

* **URL**: `/premium` và `/account/subscription`
* **File Vue**: [`PremiumView.vue`](file:///d:/FPT/metqua/frontend/src/views/PremiumView.vue) & [`SubscriptionView.vue`](file:///d:/FPT/metqua/frontend/src/views/SubscriptionView.vue)

### Đặc quyền tài khoản Premium:
* Vô hạn Tim (Không bao giờ bị gián đoạn khi làm sai bài tập).
* Mở khóa toàn bộ 44 thuật toán nâng cao và toàn bộ lời giải chi tiết.
* Sử dụng không giới hạn tính năng **AI Step Explainer** (Giải thích thuật toán bằng AI).
* Huy hiệu Premium độc quyền trên Bảng xếp hạng.
