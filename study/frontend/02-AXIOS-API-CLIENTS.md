# 🌐 TÀI LIỆU AXIOS & API CLIENTS — NETWORK COMMUNICATION LAYER

Tầng API Client trong `frontend/src/api/` quản lý toàn bộ giao tiếp mạng giữa Single Page App (Vue 3) và Backend Web API (.NET 10).

---

## 🏛️ 1. CẤU HÌNH AXIOS CLIENT CHUẨN ([`client.ts`](file:///d:/FPT/metqua/frontend/src/api/client.ts))

Instance Axios được cấu hình tập trung với các quy chuẩn kỹ thuật:
* `baseURL`: Đọc từ biến môi trường `VITE_API_BASE_URL` (mặc định: `/api/v1`).
* `timeout`: 15,000ms (15 giây).
* `withCredentials: true`: Cho phép gửi kèm Cookie an toàn (HttpOnly Cookie cho Refresh Token).

---

## 🔄 2. CƠ CHẾ INTERCEPTORS THÔNG MINH

```mermaid
sequenceDiagram
    autonumber
    participant UI as Vue Component
    participant Interceptor as Axios Interceptor (client.ts)
    participant Backend as ASP.NET Core API

    UI->>Interceptor: Gọi API (ví dụ: GET /api/v1/lessons/5)
    Interceptor->>Interceptor: Request Interceptor: Tự động gắn header `Authorization: Bearer <token>`
    Interceptor->>Backend: Gửi HTTP Request kèm Token
    alt Token HỢP LỆ (200 OK)
        Backend-->>Interceptor: 200 OK + JSON Dữ liệu
        Interceptor-->>UI: Trả về dữ liệu cho Component
    else Token HẾT HẠN (401 Unauthorized)
        Backend-->>Interceptor: 401 Unauthorized
        Interceptor->>Interceptor: Response Interceptor bắt mã 401
        alt Chưa thử Refresh (_retry == false)
            Interceptor->>Interceptor: Đánh dấu config._retry = true
            Interceptor->>Backend: POST /api/v1/auth/refresh-token
            alt Refresh THÀNH CÔNG
                Backend-->>Interceptor: Trả về Access Token MỚI
                Interceptor->>Interceptor: Cập nhật authStore.token mới
                Interceptor->>Backend: Tự động GỬI LẠI Request ban đầu (Re-try)
                Backend-->>UI: 200 OK (Người dùng không hề bị gián đoạn trải nghiệm!)
            else Refresh THẤT BẠI
                Interceptor->>Interceptor: authStore.logout()
                Interceptor-->>UI: Chuyển hướng về /login?redirect=/lessons/5
            end
        end
    end
```

---

## 🛡️ 3. LỚP LỖI CHUẨN HÓA `ApiError`

Mọi phản hồi lỗi từ Backend đều được interceptor tự động đóng gói thành class `ApiError`:
```typescript
export class ApiError extends Error {
  readonly code: string;                  // Ví dụ: 'Gamification.InsufficientGems'
  readonly field: string | null;          // Ví dụ: 'email'
  readonly status: number;                // Mã HTTP Status (400, 401, 403, 404, 429, 500)
  readonly retryAfterSeconds?: number;    // Thời gian chờ nếu bị Rate Limit 429
}
```

---

## 📋 4. DANH SÁCH CÁC MODULE API CLIENT

* [`auth.ts`](file:///d:/FPT/metqua/frontend/src/api/auth.ts): `login`, `registerRequestOtp`, `registerVerifyOtp`, `refreshToken`, `forgotPassword`, `resetPassword`.
* [`lessons.ts`](file:///d:/FPT/metqua/frontend/src/api/lessons.ts): `fetchLesson`, `completeLesson`, `saveLessonNote`, `fetchTopics`.
* [`exercises.ts`](file:///d:/FPT/metqua/frontend/src/api/exercises.ts): `fetchExercises`, `fetchExercise`, `submitQuiz`, `submitCode`, `fetchMySubmissions`.
* [`gamification.ts`](file:///d:/FPT/metqua/frontend/src/api/gamification.ts): `fetchProfileStats`, `fetchQuests`, `claimQuest`, `fetchShopItems`, `buyShopItem`, `fetchLeaderboard`.
* [`classes.ts`](file:///d:/FPT/metqua/frontend/src/api/classes.ts): `fetchMyClasses`, `createClass`, `joinClass`, `fetchClassReport`, `assignExercise`.
* [`admin.ts`](file:///d:/FPT/metqua/frontend/src/api/admin.ts): `fetchUsers`, `approveTeacher`, `resetUserPassword`, `fetchAdminStats`.
