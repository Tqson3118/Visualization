# Đề 01 — Auth & Session

**Thời gian:** 25 phút | **Tổng điểm:** 10 điểm
**Bao phủ:** `LoginView.vue` · `RegisterView.vue` · `ForgotPasswordView.vue` · `ResetPasswordView.vue` · `auth.ts` (Pinia) · Router Guard

---

## PHẦN I — TRẮC NGHIỆM (5 câu × 1 điểm = 5 điểm)

**Câu 1:** Theo ADR-004 của dự án DSA Visual, `accessToken` sau khi đăng nhập thành công sẽ được lưu ở đâu?

A. `localStorage` dưới key `access_token`
B. `sessionStorage` dưới key `auth_token`
C. Chỉ trong bộ nhớ Pinia (in-memory), **không** lưu Web Storage
D. Cookie HttpOnly do server set

---

**Câu 2:** Người dùng đã đăng nhập, sau đó nhấn **F5** (reload trang). Chuỗi sự kiện nào xảy ra ĐÚNG với thiết kế của `auth.ts`?

A. `accessToken` bị mất → user bị logout → redirect login
B. `accessToken` đọc lại từ `localStorage` → session khôi phục
C. Browser gửi cookie HttpOnly lên server → `authApi.refresh()` được gọi → `accessToken` mới được lưu vào memory Pinia
D. `authApi.refresh()` bị gọi nhiều lần song song bằng cách dùng `Promise.all`

---

**Câu 3:** Khi **nhiều request 401** xảy ra đồng thời (ví dụ 5 tab cùng hết hạn token), cơ chế nào trong `auth.ts` đảm bảo `authApi.refresh()` chỉ được gọi **đúng 1 lần**?

A. `debounce` với `setTimeout(500ms)`
B. Singleton promise — biến `refreshPromise` được chia sẻ; nếu đang chạy thì trả về promise cũ thay vì tạo mới
C. `mutex` từ thư viện `async-mutex`
D. `AbortController` hủy các request dư thừa

---

**Câu 4:** Route `/profile` yêu cầu `roles: ['ADMIN']`. Một user có role `TEACHER` đã đăng nhập truy cập route này. Router guard sẽ làm gì?

A. Cho phép vào vì user đã xác thực
B. Redirect đến `login` với `query.redirect = '/profile'`
C. Redirect đến route `profile` (trang profile của chính user)
D. Trả về HTTP 403 từ server

---

**Câu 5:** Trong `auth.ts`, hàm `logout()` reset bao nhiêu Pinia store và đó là những store nào?

A. 5 store: gamification, progress, lesson, classStore, leaderboard
B. 6 store: gamification, progress, lesson, classStore, leaderboard, codeRunner
C. 7 store: gamification, progress, lesson, classStore, leaderboard, codeRunner, simulation
D. 3 store: auth, gamification, progress

---

## PHẦN II — TỰ LUẬN TRACE LUỒNG (2 câu × 2.5 điểm = 5 điểm)

> Yêu cầu viết rõ **4 chặng** cho mỗi câu

---

**Câu 6:** Trace đầy đủ luồng khi người dùng **đăng nhập thành công** (email hợp lệ, password đúng) trong `LoginView.vue`.

Yêu cầu viết rõ **4 chặng**:

1. **Thao tác UI:** Component nào xử lý? Directive/event nào bắt submit? Validate phía FE kiểm tra gì? State `loading` thay đổi ra sao?
2. **Frontend Data Layer:** Store nào được gọi? Hàm nào trong store? Axios gọi URL gì, Method gì, Payload gì?
3. **Backend Layer:** Controller nào? Service nào? Bảng DB nào được truy vấn? DTO nào được trả về?
4. **UI Render:** Pinia state cập nhật những field nào? Router điều hướng đến đâu? Giao diện (Navbar, nút) thay đổi gì?

---

**Câu 7:** Trace đầy đủ luồng **Quên mật khẩu → Reset mật khẩu**, bao gồm cả bước gửi email và bước đặt lại mật khẩu bằng token.

Yêu cầu viết rõ **4 chặng** cho **CẢ HAI bước**:

**Bước A — Gửi yêu cầu quên mật khẩu (`ForgotPasswordView.vue`):**
1. **UI:** Component nào? Validate gì (email format)? Loading state?
2. **FE Data Layer:** API call — URL, Method, Payload?
3. **Backend:** Controller, Service, logic tạo reset-token và gửi email?
4. **UI Render:** Thông báo gì hiển thị? Form ẩn hay vẫn hiện?

**Bước B — Đặt lại mật khẩu (`ResetPasswordView.vue`):**
1. **UI:** Tham số `token` và `email` lấy từ URL query string thế nào? Validate password mới (min length, confirm match)?
2. **FE Data Layer:** API call — URL, Method, Payload bao gồm token?
3. **Backend:** Verify token còn hạn → hash password mới (bcrypt) → lưu vào bảng Users → xóa token?
4. **UI Render:** Điều hướng về đâu sau thành công? Thông báo lỗi khi token hết hạn?
