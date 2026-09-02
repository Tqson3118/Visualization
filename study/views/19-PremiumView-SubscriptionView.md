# 👑 VIEW 19: NÂNG CẤP PREMIUM & QUẢN LÝ GÓI (PREMIUMVIEW & SUBSCRIPTIONVIEW)

* **Tên file Vue**:
  1. [`PremiumView.vue`](file:///d:/FPT/metqua/frontend/src/views/PremiumView.vue) (URL: `/premium`)
  2. [`SubscriptionView.vue`](file:///d:/FPT/metqua/frontend/src/views/SubscriptionView.vue) (URL: `/account/subscription`, alias: `/subscription`)
* **Quyền truy cập**: Đã đăng nhập (`requiresAuth: true`).

---

## 1. MÀN HÌNH NÂNG CẤP PREMIUM (PREMIUM VIEW)

### Mắt thấy gì trên giao diện?
1. **3 Gói đăng ký**:
   * Gói 1 Tháng: `49.000₫`
   * Gói 3 Tháng: `129.000₫`
   * Gói 12 Tháng: `399.000₫` (Gắn huy hiệu *"Tiết kiệm nhất"*).
2. **Bảng so sánh Quyền lợi Free vs Premium**:
   * Số Tim: 5 Tim giới hạn vs Vô hạn Tim ❤️.
   * Số thuật toán: Cơ bản vs Trọn bộ 44 thuật toán.
   * AI Step Explainer: Giới hạn vs Không giới hạn.
3. **Modal Thanh toán QR VietQR (MB Bank)**:
   * Hiển thị mã QR chuẩn EMVCo động (tự điền số tiền và nội dung chuyển khoản `DSV{userId}T{months}`).
   * Nút *"Tôi đã chuyển khoản"* kích hoạt sau 60s đếm ngược.

---

## 2. MÀN HÌNH QUẢN LÝ GÓI THUÊ BAO (SUBSCRIPTION VIEW)

### Mắt thấy gì trên giao diện?
* Thông tin gói hiện tại (Standard Free / Premium Pro).
* Ngày hết hạn của gói Premium.
* Lịch sử các lần gia hạn và hóa đơn.
* Nút *"Gia hạn gói"* hoặc *"Hủy tự động gia hạn"*.

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend Views**: [`PremiumView.vue`](file:///d:/FPT/metqua/frontend/src/views/PremiumView.vue), [`SubscriptionView.vue`](file:///d:/FPT/metqua/frontend/src/views/SubscriptionView.vue)
* **VietQR Generator**: `src/lib/vietqr.ts`
* **Database Entity**: [`PremiumSubscription.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Persistence/Entities/PremiumSubscription.cs)
