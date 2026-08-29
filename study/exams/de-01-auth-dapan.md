# Đáp Án — Đề 01: Auth & Session

---

## PHẦN I — TRẮC NGHIỆM

**Câu 1: C**
`accessToken` chỉ lưu trong bộ nhớ Pinia (in-memory) theo ADR-004. Không dùng localStorage/sessionStorage để tránh XSS có thể đọc token. Cookie HttpOnly là nơi lưu **refresh token** (phía server set), không phải access token.

**Câu 2: C**
Khi F5, access token trong memory Pinia bị xóa. Browser tự động gửi cookie HttpOnly lên server → backend xác thực cookie → `authApi.refresh()` trả về access token mới → lưu vào memory Pinia → session khôi phục hoàn toàn mà user không nhận ra.

**Câu 3: B**
`auth.ts` dùng pattern **singleton promise**: biến `refreshPromise` (kiểu `Promise<void> | null`) được kiểm tra trước khi gọi. Nếu `refreshPromise !== null` → các 401 sau dùng chung promise đó. Sau khi resolve/reject → reset `refreshPromise = null`. Đây là cách chuẩn mà không cần thư viện ngoài.

**Câu 4: C**
Guard kiểm tra: user đã login ✓ nhưng role `TEACHER` không nằm trong `['ADMIN']` → **redirect đến route `profile`** (không phải login vì đã xác thực). Chỉ redirect login khi chưa đăng nhập.

**Câu 5: C**
`logout()` reset **7 store**: gamification, progress, lesson, classStore, leaderboard, codeRunner, simulation. Mỗi store có `$reset()` hoặc action reset riêng được gọi tuần tự. Lý do: tránh data leakage giữa các phiên người dùng khác nhau.

---

## PHẦN II — TỰ LUẬN

### Câu 6 — Đăng nhập thành công

**1. Thao tác UI:**
- Component: `LoginView.vue`
- Event: `<form @submit.prevent="handleLogin">` — `@submit.prevent` ngăn reload, gọi `handleLogin()`
- Validate FE: email format (regex RFC 5322), password required + min 6 chars (hoặc theo schema zod/vuelidate)
- `loading = true` ngay khi bắt đầu submit → disable nút, hiện spinner

**2. Frontend Data Layer:**
- Gọi: `authStore.login(email, password)` (trong `auth.ts`)
- Bên trong store: `authApi.login(email, password)` → Axios `POST /api/auth/login`
- Payload: `{ email: "user@example.com", password: "secret123" }`
- Header: `Content-Type: application/json`

**3. Backend Layer:**
- `AuthController.Login([FromBody] LoginRequestDto dto)` → `AuthService.LoginAsync(dto)`
- Service: truy vấn bảng `Users` WHERE `email = dto.Email`
- Verify: `BCrypt.Verify(dto.Password, user.PasswordHash)`
- Tạo JWT access token (HS256, exp: 15 phút) + refresh token (lưu bảng `RefreshTokens`)
- Set cookie HttpOnly `refresh_token` trên response
- Trả về: `LoginResponseDto { accessToken: string, user: { id, email, fullName, role, avatarUrl } }`

**4. UI Render:**
- `authStore`: set `token = accessToken`, `user = dto.user`, `status = 'authenticated'`
- `loading = false`
- Router: `router.push(route.query.redirect || { name: 'home' })`
- Navbar: ẩn nút "Đăng nhập / Đăng ký" → hiện Avatar + tên user + dropdown menu
- Toast: "Chào mừng trở lại, [fullName]!"

---

### Câu 7 — Quên mật khẩu → Reset mật khẩu

#### Bước A — ForgotPasswordView.vue

**1. UI:**
- Component: `ForgotPasswordView.vue`
- Form: `<form @submit.prevent="handleForgot">`
- Validate: email required + format hợp lệ
- `loading = true` khi submit

**2. FE Data Layer:**
- `authApi.forgotPassword(email)` → Axios `POST /api/auth/forgot-password`
- Payload: `{ email: "user@example.com" }`

**3. Backend:**
- `AuthController.ForgotPassword([FromBody] ForgotPasswordDto dto)`
- `AuthService.ForgotPasswordAsync(dto.Email)`:
  - Tìm user theo email trong bảng `Users`
  - Tạo `resetToken` = `Guid.NewGuid().ToString("N")` (hoặc crypto random)
  - Lưu token + `ExpiresAt = DateTime.UtcNow.AddHours(1)` vào bảng `PasswordResetTokens`
  - Gọi `EmailService.SendResetPasswordEmail(email, resetToken)` → gửi link dạng `https://dsavisual.com/reset-password?token=xxx&email=user@example.com`
- Response: `200 OK { message: "Email đã được gửi nếu tài khoản tồn tại" }` (không reveal user tồn tại hay không)

**4. UI Render:**
- `loading = false`
- Hiện thông báo thành công: "Kiểm tra hộp thư của bạn để nhận link đặt lại mật khẩu"
- Form có thể ẩn đi hoặc disable nút gửi lại trong 60 giây (cooldown timer)

---

#### Bước B — ResetPasswordView.vue

**1. UI:**
- Route params: `token = route.query.token`, `email = route.query.email` (lấy từ URL query string)
- Form: 2 trường `newPassword` + `confirmPassword`
- Validate: min 8 ký tự, có chữ hoa + số (tùy policy), `confirmPassword === newPassword`
- `loading = true` khi submit

**2. FE Data Layer:**
- `authApi.resetPassword(token, email, newPassword)` → Axios `POST /api/auth/reset-password`
- Payload: `{ token: "abc123...", email: "user@example.com", newPassword: "NewPass@123" }`

**3. Backend:**
- `AuthController.ResetPassword([FromBody] ResetPasswordDto dto)`
- `AuthService.ResetPasswordAsync(dto)`:
  1. Tìm record trong bảng `PasswordResetTokens` WHERE `Token = dto.Token AND Email = dto.Email`
  2. Kiểm tra `ExpiresAt > DateTime.UtcNow` (token chưa hết hạn)
  3. Hash password mới: `BCrypt.HashPassword(dto.NewPassword, workFactor: 12)`
  4. Update bảng `Users` SET `PasswordHash = newHash`
  5. Delete record trong `PasswordResetTokens` (token dùng 1 lần)
- Response: `200 OK { message: "Mật khẩu đã được đặt lại thành công" }`

**4. UI Render:**
- Thành công: Toast "Mật khẩu đã được cập nhật!" → `router.push({ name: 'login' })` sau 2 giây
- Token hết hạn: Hiện thông báo lỗi "Link đặt lại mật khẩu đã hết hạn. Vui lòng gửi lại yêu cầu." + nút redirect về ForgotPasswordView
