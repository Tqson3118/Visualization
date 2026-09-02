# 🛒 VIEW 18: CỬA HÀNG VẬT PHẨM (SHOPVIEW)

* **Tên file Vue**: [`ShopView.vue`](file:///d:/FPT/metqua/frontend/src/views/ShopView.vue)
* **Đường dẫn URL**: `/shop`
* **Route Name**: `shop`
* **Quyền truy cập**: Đã đăng nhập (`requiresAuth: true`).

---

## 1. CẤU TRÚC GIAO DIỆN & 4 DANH MỤC VẬT PHẨM

```
┌────────────────────────────────────────────────────────────────────────┐
│  🛒 CỬA HÀNG VẬT PHẨM (GEMS SHOP)          [ Số dư: 💎 520 | ❤️ 3/5 ]  │
├────────────────────────────────────────────────────────────────────────┤
│ [ Tab: Tất cả ]  [ Tab: Hồi Tim ]  [ Tab: Avatar ]  [ Tab: Khung viền ]│
├────────────────────────────────────────────────────────────────────────┤
│ ┌──────────────────────┐  ┌──────────────────────┐  ┌────────────────┐ │
│ │ ❤️ HỒI ĐẦY 5 TIM     │  │ 🛡️ BẢO VỆ STREAK     │  │ 👑 KHUNG CYBER │ │
│ │ Hồi mạng học tập ngay│  │ Đóng băng streak 1 ng│  │ Viền Neon tím  │ │
│ │ Giá: 💎 100 Ngọc     │  │ Giá: 💎 150 Ngọc     │  │ Giá: 💎 300 Ngọc│ │
│ │ [ Mua ngay ]         │  │ [ Mua ngay ]         │  │ [ Mua ngay ]   │ │
│ └──────────────────────┘  └──────────────────────┘  └────────────────┘ │
└────────────────────────────────────────────────────────────────────────┘
```

---

## 2. CHI TIẾT CÁC LUỒNG HOẠT ĐỘNG (FLOWS)

### 🔹 Flow 1: Mua vật phẩm và Trừ Ngọc (Purchase Flow)

```mermaid
sequenceDiagram
    autonumber
    actor Student as Học viên
    participant UI as ShopView.vue
    participant API as GamificationController.cs
    participant Service as GamificationService.cs
    participant DB as SQL Server

    Student->>UI: Bấm "Mua ngay" trên Gói Hồi Đầy Tim (100 Ngọc)
    UI->>API: POST /api/v1/gamification/shop/buy { itemId }
    API->>Service: BuyItemAsync(userId, itemId)
    Service->>DB: Kiểm tra số dư Ngọc (UserProgress.Gems >= 100)
    alt Không đủ Ngọc
        Service-->>API: Result.Failure(ErrorCodes.Gamification.InsufficientGems)
        API-->>UI: 400 Bad Request -> Hiện thông báo "Không đủ Ngọc, hãy làm thêm nhiệm vụ!"
    else Đủ Ngọc
        Service->>DB: UserProgress.Gems -= 100
        Service->>DB: UserProgress.Hearts = 5 (Hồi đầy tim)
        Service->>DB: Thêm vào UserInventory và ghi log GemTransactions
        Service-->>API: Result.Success({ gemsLeft: 420, hearts: 5 })
        API-->>UI: 200 OK -> Hiện toast thành công, số Tim trên Header đầy 5/5
    end
```

---

## 3. BẢN ĐỒ MÃ NGUỒN LIÊN QUAN

* **Frontend View**: [`ShopView.vue`](file:///d:/FPT/metqua/frontend/src/views/ShopView.vue)
* **Frontend Store**: [`gamification.ts`](file:///d:/FPT/metqua/frontend/src/stores/gamification.ts)
* **Backend Controller**: [`GamificationController.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Api/Controllers/GamificationController.cs)
* **Database Entities**: [`ShopItem.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Persistence/Entities/ShopItem.cs), [`UserInventory.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Persistence/Entities/UserInventory.cs), [`GemTransaction.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Persistence/Entities/GemTransaction.cs)
