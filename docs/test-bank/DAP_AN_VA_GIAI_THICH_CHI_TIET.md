# 📖 ĐÁP ÁN, GIẢI THÍCH CHI TIẾT & BAREM CHẤM ĐIỂM
## Ngân hàng Đề thi Đánh giá Năng lực Mã nguồn Dự án DSA Visual
### ĐÍNH KÈM TRỰC TIẾP ĐOẠN CODE MINH HỌA (CODE SNIPPETS) TỪ REPOSITORY

> **Tài liệu tham chiếu:** Trích xuất từ mã nguồn thực tế của dự án (`frontend/src/`, `backend/src/DsaVisual.Api/`, `backend/src/DsaVisual.Application/`).  
> **Dành cho:** Giảng viên, Lead chấm thi, và Lập trình viên tự đánh giá kiến thức hệ thống mà không cần mở lại file mã nguồn.  
> **Người mới bắt đầu:** Đọc trước mục "📚 Kiến thức nền & Bảng thuật ngữ" ở đầu file đề thi (`DE_THI_KIEN_THUC_CODEBASE_DSA.md`) để nắm khái niệm Vue / Pinia / ASP.NET Core / HTTP và các thuật ngữ riêng của DSA Visual trước khi đọc giải thích.

---

# 📑 MỤC LỤC ĐÁP ÁN

- [📘 ĐÁP ÁN ĐỀ 01: Phân hệ Xác thực & Phiên làm việc](#-đáp-án-đề-01-phân-hệ-xác-thực--phiên-làm-việc)
- [📘 ĐÁP ÁN ĐỀ 02: Trang chủ, Khám phá Demo & Điều hướng Hệ thống](#-đáp-án-đề-02-trang-chủ-khám-phá-demo--điều-hướng-hệ-thống)
- [📘 ĐÁP ÁN ĐỀ 03: Mô phỏng Thuật toán & Trực quan hóa Engine](#-đáp-án-đề-03-mô-phỏng-thuật-toán--trực-quan-hóa-engine)
- [📘 ĐÁP ÁN ĐỀ 04: Màn hình Khám phá Mô phỏng, Tra cứu & So sánh](#-đáp-án-đề-04-màn-hình-khám-phá-mô-phỏng-tra-cứu--so-sánh)
- [📘 ĐÁP ÁN ĐỀ 05: Sandbox Tương tác 4 Tab](#-đáp-án-đề-05-sandbox-tương-tác-4-tab)
- [📘 ĐÁP ÁN ĐỀ 06: Lộ trình Học tập & Học Bài học](#-đáp-án-đề-06-lộ-trình-học-tập--học-bài-học)
- [📘 ĐÁP ÁN ĐỀ 07: Thang Thực hành 3 Bậc & Trắc nghiệm Chấm điểm](#-đáp-án-đề-07-thang-thực-hành-3-bậc--trắc-nghiệm-chấm-điểm)
- [📘 ĐÁP ÁN ĐỀ 08: Trình chạy Code Sandbox & Chấm điểm Server](#-đáp-án-đề-08-trình-chạy-code-sandbox--chấm-điểm-server)
- [📘 ĐÁP ÁN ĐỀ 09: Hệ thống Gamification: Nhiệm vụ & Bảng xếp hạng](#-đáp-án-đề-09-hệ-thống-gamification-nhiệm-vụ--bảng-xếp-hạng)
- [📘 ĐÁP ÁN ĐỀ 10: Cửa hàng Gems & Gói Hội viên Premium](#-đáp-án-đề-10-cửa-hàng-gems--gói-hội-viên-premium)
- [📘 ĐÁP ÁN ĐỀ 11: Hồ sơ Cá nhân, Thành tích & Cài đặt](#-đáp-án-đề-11-hồ-sơ-cá-nhân-thành-tích--cài-đặt)
- [📘 ĐÁP ÁN ĐỀ 12: Hệ thống Lớp học & Studio Giảng viên](#-đáp-án-đề-12-hệ-thống-lớp-học--studio-giảng-viên)
- [📘 ĐÁP ÁN ĐỀ 13: Bảng điều khiển Quản trị Hệ thống](#-đáp-án-đề-13-bảng-điều-khiển-quản-trị-hệ-thống)
- [📘 ĐÁP ÁN ĐỀ 14: Trợ giúp, Chính sách Bảo mật & Phản hồi Người dùng](#-đáp-án-đề-14-trợ-giúp-chính-sách-bảo-mật--phản-hồi-người-dùng)
- [📘 ĐÁP ÁN ĐỀ 15: Studio Giảng viên & Quản lý Nội dung Học tập](#-đáp-án-đề-15-studio-giảng-viên--quản-lý-nội-dung-học-tập)
- [📘 ĐÁP ÁN ĐỀ 16: Kiểm tra Cuối Lộ trình & Điều hướng Thông minh](#-đáp-án-đề-16-kiểm-tra-cuối-lộ-trình--điều-hướng-thông-minh)
- [📘 ĐÁP ÁN ĐỀ 17: Composables, Hiệu ứng & Cross-cutting Concerns](#-đáp-án-đề-17-composables-hiệu-ứng--cross-cutting-concerns)
- [📘 ĐÁP ÁN ĐỀ 18: Visualization Engine Core](#-đáp-án-đề-18-visualization-engine-core)
- [📘 ĐÁP ÁN ĐỀ 19: API Layer & Frontend Architecture](#-đáp-án-đề-19-api-layer--frontend-architecture)
- [🏆 ĐÁP ÁN ĐỀ FINAL: Bài thi Đánh giá Năng lực Toàn Hệ thống](#-đáp-án-đề-final-bài-thi-đánh-giá-năng-lực-toàn-hệ-thống)

---

# 📘 ĐÁP ÁN ĐỀ 01: PHÂN HỆ XÁC THỰC & PHIÊN LÀM VIỆC

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (1.0 điểm / câu)

#### **Câu 1.1 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Cờ `guestOnly: true` được định nghĩa trong `RouteMeta` chỉ cho phép khách vãng lai truy cập. Khi người dùng đã đăng nhập (`authStore.isAuthenticated === true`), Router Guard chặn lại và tự động chuyển hướng về trang chủ `/` (route name `home`).
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các đáp án khác sai vì: C — guard không cho phép vào; B — 404 chỉ dành cho URL không khớp route nào; A — không có popup bắt buộc đăng xuất.
- **② Vì sao:** Đáp án **D** đúng vì Cờ `guestOnly: true` được định nghĩa trong `RouteMeta` chỉ cho phép khách vãng lai truy cập. Khi người dùng đã đăng nhập (`authStore.isAuthenticated === true`), Router Guard chặn lại và tự động chuyển hướng về trang chủ `/` (route name `home`).
- ❌ Các đáp án khác sai vì: C — guard không cho phép vào; B — 404 chỉ dành cho URL không khớp route nào; A — không có popup bắt buộc đăng xuất.
- **⑤ Code thật (`frontend/src/router/index.ts`):**
```typescript
// frontend/src/router/index.ts
{
  path: '/login',
  name: 'login',
  component: LoginView,
  meta: { guestOnly: true },
},
{
  path: '/register',
  name: 'register',
  component: RegisterView,
  meta: { guestOnly: true },
}
```

---

#### **Câu 1.2 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Form đăng ký sử dụng reactive checklist `passwordRules` để kiểm tra trực tiếp 5 tiêu chí mật khẩu sống. Khi xác nhận mật khẩu không khớp, hàm `validate()` gán `errors.confirmPassword = messages.register.confirmError` và chặn `onSubmit()` không gửi API lên server.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì Form đăng ký sử dụng reactive checklist `passwordRules` để kiểm tra trực tiếp 5 tiêu chí mật khẩu sống. Khi xác nhận mật khẩu không khớp, hàm `validate()` gán `errors.confirmPassword = messages.register.confirmError` và chặn `onSubmit()` không gửi API lên server.
- **⑤ Code thật (`frontend/src/views/RegisterView.vue`):**
```typescript
// frontend/src/views/RegisterView.vue
const passwordRules = computed(() => [
  { key: 'length', ok: form.password.length >= 8 && form.password.length <= 64, label: messages.register.checklist[0] },
  { key: 'upper', ok: /[A-Z]/.test(form.password), label: messages.register.checklist[1] },
  { key: 'lower', ok: /[a-z]/.test(form.password), label: messages.register.checklist[2] },
  { key: 'digit', ok: /\d/.test(form.password), label: messages.register.checklist[3] },
  { key: 'special', ok: /[^A-Za-z0-9]/.test(form.password), label: messages.register.checklist[4] },
]);

function validate(): boolean {
  const errors: Record<string, string> = {};
  if (!isValidEmail(form.email)) errors.email = messages.auth.invalidEmail;
  const pwd = validatePassword(form.password);
  if (!pwd.ok) errors.password = messages.auth.passwordRequirement;
  if (form.confirmPassword !== form.password) errors.confirmPassword = messages.register.confirmError;
  // ...
  Object.assign(fieldErrors, errors);
  return Object.keys(errors).length === 0;
}
```

---

#### **Câu 1.3 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Dự án tuân thủ tiêu chuẩn kiến trúc bảo mật ADR-004: AccessToken chỉ tồn tại trong bộ nhớ RAM của Pinia Store để chống tấn công XSS; RefreshToken được lưu trong Cookie HttpOnly với cờ `SameSite=Strict` và `Secure` do server quản lý.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì Dự án tuân thủ tiêu chuẩn kiến trúc bảo mật ADR-004: AccessToken chỉ tồn tại trong bộ nhớ RAM của Pinia Store để chống tấn công XSS; RefreshToken được lưu trong Cookie HttpOnly với cờ `SameSite=Strict` và `Secure` do server quản lý.
- **⑤ Code thật (`frontend/src/stores/auth.ts` & `backend/src/DsaVisual.Api/Controllers/AuthController.cs`):**
```typescript
// frontend/src/stores/auth.ts (ADR-004: Token chỉ trong memory Pinia)
export const useAuthStore = defineStore('auth', () => {
  const user = ref<UserSummary | null>(null);
  const accessToken = ref<string | null>(null);
  const status = ref<AuthStatus>('idle');
  // ...
});
```
```csharp
// backend/src/DsaVisual.Api/Controllers/AuthController.cs
private CookieOptions BuildRefreshCookieOptions(DateTimeOffset? expires = null) => new()
{
    HttpOnly = true,
    SameSite = SameSiteMode.Strict,
    Secure = Request.IsHttps,
    Path = "/api/v1/auth",
    Expires = expires
};
```

---

#### **Câu 1.4 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `authStore.refresh()` được thiết kế theo mô hình Singleton Promise (`refreshPromise`). Nếu nhiều request API cùng gặp lỗi 401 cùng lúc, hệ thống chỉ gọi duy nhất 1 lần endpoint `POST /api/v1/auth/refresh` bằng cookie HttpOnly, sau đó tái sử dụng token mới cho toàn bộ các request đang chờ.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì `authStore.refresh()` được thiết kế theo mô hình Singleton Promise (`refreshPromise`). Nếu nhiều request API cùng gặp lỗi 401 cùng lúc, hệ thống chỉ gọi duy nhất 1 lần endpoint `POST /api/v1/auth/refresh` bằng cookie HttpOnly, sau đó tái sử dụng token mới cho toàn bộ các request đang chờ.
- **⑤ Code thật (`frontend/src/stores/auth.ts` & `frontend/src/api/client.ts`):**
```typescript
// frontend/src/stores/auth.ts
let refreshPromise: Promise<string | null> | null = null;

async function refresh(): Promise<string | null> {
  if (refreshPromise) return refreshPromise;
  refreshPromise = authApi
    .refresh()
    .then((response) => {
      accessToken.value = response.accessToken;
      status.value = 'authenticated';
      return response.accessToken;
    })
    .finally(() => {
      refreshPromise = null;
    });
  return refreshPromise;
}
```

---

#### **Câu 1.5 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Khi người dùng bấm đăng xuất, hệ thống nạp động (dynamic import) và kích hoạt hàm `.reset()` của tất cả 7 store cá nhân để tránh việc lưu đọng dữ liệu cũ khi tài khoản khác đăng nhập.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì Khi người dùng bấm đăng xuất, hệ thống nạp động (dynamic import) và kích hoạt hàm `.reset()` của tất cả 7 store cá nhân để tránh việc lưu đọng dữ liệu cũ khi tài khoản khác đăng nhập.
- **⑤ Code thật (`frontend/src/stores/auth.ts`):**
```typescript
// frontend/src/stores/auth.ts
async function logout(): Promise<void> {
  try {
    await authApi.logout();
  } finally {
    accessToken.value = null;
    user.value = null;
    status.value = 'idle';

    // Reset toàn bộ state cá nhân khi logout (tránh đọng state user cũ)
    const { useGamificationStore } = await import('./gamification');
    useGamificationStore().reset();
    const { useProgressStore } = await import('./progress');
    useProgressStore().reset();
    const { useLessonStore } = await import('./lesson');
    useLessonStore().reset();
    const { useClassStore } = await import('./classStore');
    useClassStore().reset();
    const { useLeaderboardStore } = await import('./leaderboard');
    useLeaderboardStore().reset();
    const { useCodeRunnerStore } = await import('./codeRunner');
    useCodeRunnerStore().reset();
    const { useSimulationStore } = await import('./simulation');
    useSimulationStore().resetAll();
  }
}
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 1.1: Trace luồng xử lý lỗi Server khi Đăng ký trùng Email (2.5 điểm)
- **1. UI Submit (0.5đ):** `RegisterView.vue` chạy `onSubmit()`, gọi `authStore.register(...)`.
- **2. Backend Validation & DB (0.75đ):** `AuthService.cs` kiểm tra email trùng qua unique index `IX_Users_Email`. Khi phát hiện trùng, trả về `Result.Fail` với mã lỗi `EMAIL_EXISTS` (ErrorCodes.cs:46), được `ErrorHandlingMiddleware` map thành `HTTP 409 CONFLICT` (ErrorCodes.cs:81):
```csharp
// backend/src/DsaVisual.Application/Services/AuthService.cs:59
return Result<RefreshResponse>.Fail(ErrorCodes.EMAIL_EXISTS, "Email đã được sử dụng", new()
{
    ["email"] = ["Email đã được sử dụng"]
});
```
- ❌ Các đáp án khác sai vì: mã lỗi là `EMAIL_EXISTS` (không phải `AUTH_EMAIL_EXISTS`), HTTP status là 409 CONFLICT (không phải 400 Bad Request).
- **3. Axios Interceptor (0.5đ):** Trong `frontend/src/api/client.ts`, lỗi được parse thành `ApiError`:
```typescript
// frontend/src/api/client.ts
const body = err.response?.data?.error ?? {};
return new ApiError(status, body); // code: 'EMAIL_EXISTS', message: 'Email đã được sử dụng'
```
- **4. UI Error Render (0.75đ):** Trong `RegisterView.vue`, khối `catch (err)` bắt `ApiError` và gán `submitError.value = err.message` để render alert banner:
```typescript
// frontend/src/views/RegisterView.vue
} catch (err) {
  if (err instanceof ApiError) {
    submitError.value = err.message;
  } else {
    submitError.value = messages.auth.loginFailed;
  }
}
```

---

#### 📝 Câu TL 1.2: Trace luồng Quên mật khẩu & Đặt lại mật khẩu (2.5 điểm)
- **1. Endpoint & Token Generation (0.75đ):** `ForgotPasswordView.vue` gọi `POST /api/v1/auth/forgot-password`. `AuthService.cs` sinh raw token từ 64 random bytes → chuỗi base64url, băm SHA-256 để lưu DB, thời hạn **30 phút**, và gửi email qua SMTP:
```csharp
// backend/src/DsaVisual.Application/Services/AuthService.cs:466-470
db.PasswordResetTokens.Add(new PasswordResetToken
{
    UserId = user.Id,
    TokenHash = tokens.HashToken(rawToken),
    ExpiresAt = clock.UtcNow.AddMinutes(30),  // 30 phút (SDD §7.3.6)
    Used = false,
    CreatedAt = clock.UtcNow
});
```
- **2. Đọc Token trên URL (0.5đ):** `ResetPasswordView.vue` CHỈ đọc `token` từ `route.query.token` (KHÔNG đọc `email` — email không có trên URL):
```typescript
// frontend/src/views/ResetPasswordView.vue:27
const token = computed(() => String(route.query.token ?? ''));
```
- **3. Đặt lại Mật khẩu (0.75đ):** Gửi `POST /api/v1/auth/reset-password` với payload `{ token, newPassword }` (KHÔNG có email). Backend verify token hash và băm mật khẩu mới bằng BCrypt:
```typescript
// frontend/src/views/ResetPasswordView.vue:52
await authApi.resetPassword({ token: token.value, newPassword: form.password });
```
- **4. Phản hồi UI (0.5đ):** UI hiển thị toast thành công (`messages.reset.toastSuccess`) và sau 2 giây tự động `router.replace({ name: 'login' })`:
```typescript
// frontend/src/views/ResetPasswordView.vue:53-55
success.value = true;
toast.success(messages.reset.toastSuccess);
redirectTimer = setTimeout(() => void router.replace({ name: 'login' }), 2000);
```

---

# 📘 ĐÁP ÁN ĐỀ 02: TRANG CHỦ, KHÁM PHÁ DEMO & ĐIỀU HƯỚNG HỆ THỐNG

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (1.0 điểm / câu)

#### **Câu 2.1 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Danh mục mô phỏng định nghĩa đúng 3 thuật toán demo công khai miễn phí là Bubble Sort, Binary Search và Graph BFS (KHÔNG có Selection Sort).
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các đáp án khác sai vì: D — Quick Sort/Dijkstra/Red-Black Tree không có cờ `demoAllowed`; A — D*/AVL/Knapsack cũng không phải demo; B — chỉ 3/40+ thuật toán mở công khai.
- **② Vì sao:** Đáp án **C** đúng vì Danh mục mô phỏng định nghĩa đúng 3 thuật toán demo công khai miễn phí là Bubble Sort, Binary Search và Graph BFS (KHÔNG có Selection Sort).
- ❌ Các đáp án khác sai vì: D — Quick Sort/Dijkstra/Red-Black Tree không có cờ `demoAllowed`; A — D*/AVL/Knapsack cũng không phải demo; B — chỉ 3/40+ thuật toán mở công khai.
- **⑤ Code thật (`frontend/src/engines/catalog.ts`):**
```typescript
// frontend/src/engines/catalog.ts:57,64,88
{ key: 'sort.bubble', title: 'Sắp xếp nổi bọt (Bubble Sort)', ..., demoAllowed: true },
{ key: 'search.binary', title: 'Tìm kiếm nhị phân (Binary Search)', ..., demoAllowed: true },
{ key: 'graph.bfs', title: 'Đồ thị — Duyệt BFS', ..., demoAllowed: true },
```

---

#### **Câu 2.2 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Directive `v-reveal` dùng `IntersectionObserver` để kích hoạt hiệu ứng CSS mượt mà khi cuộn tới và tự động tắt nếu người dùng bật tùy chọn giảm chuyển động.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì Directive `v-reveal` dùng `IntersectionObserver` để kích hoạt hiệu ứng CSS mượt mà khi cuộn tới và tự động tắt nếu người dùng bật tùy chọn giảm chuyển động.
- **⑤ Code thật (`frontend/src/views/HomeView.vue`):**
```typescript
// frontend/src/views/HomeView.vue
const vReveal = {
  mounted(el: HTMLElement) {
    if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
    const observer = new IntersectionObserver((entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          el.classList.add('home__reveal--visible');
          observer.unobserve(el);
        }
      });
    }, { threshold: 0.15 });
    observer.observe(el);
  }
};
```

---

#### **Câu 2.3 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Khách chưa đăng nhập mở URL `/simulator/sort.bubble` được cho phép trực tiếp nhờ cờ `isDemoKey`.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì Khách chưa đăng nhập mở URL `/simulator/sort.bubble` được cho phép trực tiếp nhờ cờ `isDemoKey`.
- **⑤ Code thật (`frontend/src/views/SimulatorView.vue`):**
```typescript
// frontend/src/views/SimulatorView.vue
const isDemoKey = computed(() => getCatalogMeta(key.value)?.demoAllowed === true);

onMounted(() => {
  if (!auth.isAuthenticated && !isDemoKey.value) {
    void router.replace({ name: 'login', query: { redirect: route.fullPath } });
    return;
  }
});
```

---

#### **Câu 2.4 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Route được cấu hình `requiresAuth: true` sẽ lưu `route.fullPath` vào query param `redirect` để sau khi đăng nhập thành công sẽ quay lại đúng trang đó.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì Route được cấu hình `requiresAuth: true` sẽ lưu `route.fullPath` vào query param `redirect` để sau khi đăng nhập thành công sẽ quay lại đúng trang đó.
- **⑤ Code thật (`frontend/src/router/index.ts`):**
```typescript
// frontend/src/router/index.ts
{
  path: '/profile',
  name: 'profile',
  component: ProfileView,
  meta: { requiresAuth: true },
}
```

---

#### **Câu 2.5 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Route wildcard `/:pathMatch(.*)*` bắt mọi đường dẫn không hợp lệ và hiển thị `NotFoundView.vue`.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì Route wildcard `/:pathMatch(.*)*` bắt mọi đường dẫn không hợp lệ và hiển thị `NotFoundView.vue`.
- **⑤ Code thật (`frontend/src/router/index.ts`):**
```typescript
// frontend/src/router/index.ts
{
  path: '/:pathMatch(.*)*',
  name: 'not-found',
  component: () => import('@/views/NotFoundView.vue'),
}
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 2.1: Trace luồng Khách xem Demo $\rightarrow$ Đăng ký $\rightarrow$ Học bài thật (2.5 điểm)
- **1. Trải nghiệm Demo (0.75đ):** Khách truy cập `/simulator/sort.bubble`, `SimulatorView.vue` kiểm tra `isDemoKey.value === true` nên bỏ qua guard, nạp `generateBubbleSortSteps()` và vẽ Canvas bình thường.
- **2. Banner Điều hướng (0.5đ):** Banner `DemoBanner.vue` hiển thị nút "Đăng ký để mở khóa toàn bộ" dẫn tới route `/register`.
- **3. Đăng ký & Lưu Phiên (0.75đ):** Hoàn thành form tại `RegisterView.vue`, `authStore.register()` gọi `POST /api/v1/auth/register`, Backend tạo User và cấp AccessToken lưu RAM Pinia.
- **4. Vào Lộ trình (0.5đ):** `router.replace({ name: 'courses' })` đưa user vào `/path`, `CoursesListView.vue` nạp danh mục khóa học qua `useCourseStore()` → `courseApi.getCourses()` → `GET /concepts/courses` (services/courseApi.ts:123).

---

#### 📝 Câu TL 2.2: Trace luồng Phân quyền Route Guard theo Vai trò Role (2.5 điểm)
- **1. Cấu hình Route Meta (0.5đ):** `/admin/users` được cấu hình `meta: { requiresAuth: true, roles: ['ADMIN'] }`.
- **2. Kiểm tra Role trong Router Guard (0.75đ):**
```typescript
// frontend/src/router/index.ts:411-413
const requiredRoles = to.matched.flatMap((record) => record.meta.roles ?? []);
if (requiredRoles.length > 0 && (auth.role === null || !requiredRoles.includes(auth.role))) {
  return auth.isAuthenticated ? { name: 'profile' } : { name: 'login' };
}
```
- **3. Chặn & Chuyển hướng (0.75đ):** User có role `STUDENT` đã đăng nhập bị chặn và redirect về `/profile` (route name `profile`). Nếu chưa đăng nhập thì redirect về `/login`. Guard KHÔNG import `uiStore` và KHÔNG hiển thị toast — chỉ chuyển hướng im lặng.
- ❌ Đáp án cũ sai vì: guard redirect về `/profile` (không phải `/path` hay `/courses`), và KHÔNG có toast cảnh báo nào được kích hoạt.

---

# 📘 ĐÁP ÁN ĐỀ 03: MÔ PHỎNG THUẬT TOÁN & TRỰC QUAN HÓA ENGINE

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 3.1 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Layout Desktop chia tỉ lệ 3/12 (Mã giả) - 6/12 (Canvas) - 3/12 (Giải thích) theo quy chuẩn Màn 05 SDD.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì Layout Desktop chia tỉ lệ 3/12 (Mã giả) - 6/12 (Canvas) - 3/12 (Giải thích) theo quy chuẩn Màn 05 SDD.
- **⑤ Code thật (`frontend/src/views/SimulatorView.vue`):**
```html
<!-- frontend/src/views/SimulatorView.vue -->
<div class="simulator__grid">
  <aside class="simulator__col simulator__col--code"><PseudocodePanel ... /></aside>
  <main class="simulator__col simulator__col--canvas"><CanvasArea ... /></main>
  <aside class="simulator__col simulator__col--explain"><ExplainPanel ... /></aside>
</div>
```

---

#### **Câu 3.2 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Composable `useSimulation` là lớp ủy quyền mỏng (thin wrapper); logic playback thực sự nằm trong `simulationStore`. Hàm `startPlayback()` trong store dùng `setInterval` với khoảng `Math.max(75, 1200 / speed)` để tăng `currentIndex`, kiểm tra breakpoint và cập nhật trạng thái `finished` khi hết bước.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì Composable `useSimulation` là lớp ủy quyền mỏng (thin wrapper); logic playback thực sự nằm trong `simulationStore`. Hàm `startPlayback()` trong store dùng `setInterval` với khoảng `Math.max(75, 1200 / speed)` để tăng `currentIndex`, kiểm tra breakpoint và cập nhật trạng thái `finished` khi hết bước.
- **⑤ Code thật (`frontend/src/stores/simulation.ts`):**
```typescript
// frontend/src/stores/simulation.ts — startPlayback()
function startPlayback(): void {
  clearPlayback();
  if (steps.value.length === 0) return;
  const interval = Math.max(75, 1200 / speed.value);
  playbackTimer = setInterval(() => {
    if (status.value !== 'running') { clearPlayback(); return; }
    if (currentIndex.value >= steps.value.length - 1) {
      status.value = 'finished'; clearPlayback(); return;
    }
    currentIndex.value += 1;
    hitBreakpointAtCurrentStep();
  }, interval);
}
```

---

#### **Câu 3.3 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `configureInput()` nạp mảng mới vào generator của thuật toán, sinh lại toàn bộ mảng `steps` và reset `currentIndex = 0`.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì `configureInput()` nạp mảng mới vào generator của thuật toán, sinh lại toàn bộ mảng `steps` và reset `currentIndex = 0`.
- **⑤ Code thật (`frontend/src/composables/useSimulation.ts`):**
```typescript
// frontend/src/composables/useSimulation.ts
function configureInput(newInput: number[]): void {
  inputConfig.value = newInput;
  if (generatorFn) {
    steps.value = generatorFn(newInput);
    currentIndex.value = 0;
    status.value = 'idle';
  }
}
```

---

#### **Câu 3.4 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `toggleBreakpoint(line)` lưu điểm dừng; khi bước chạy có `step.line === breakpoint`, hệ thống tự động pause.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì `toggleBreakpoint(line)` lưu điểm dừng; khi bước chạy có `step.line === breakpoint`, hệ thống tự động pause.
- **⑤ Code thật (`frontend/src/composables/useSimulation.ts`):**
```typescript
// frontend/src/composables/useSimulation.ts
function toggleBreakpoint(line: number): void {
  breakpoints.value[line] = !breakpoints.value[line];
}

watch(currentIndex, (newIdx) => {
  const currentLine = steps.value[newIdx]?.line;
  if (currentLine && breakpoints.value[currentLine] && status.value === 'running') {
    pause();
    breakpointHit.value = currentLine;
  }
});
```

---

#### **Câu 3.5 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Bấm ngôi sao gọi API `POST /api/v1/favorites` với body `{ simKey, input? }` để lưu bản ghi yêu thích vào CSDL; icon chuyển sang trạng thái active sau khi nhận 200 OK.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì Bấm ngôi sao gọi API `POST /api/v1/favorites` với body `{ simKey, input? }` để lưu bản ghi yêu thích vào CSDL; icon chuyển sang trạng thái active sau khi nhận 200 OK.
- **⑤ Code thật (`frontend/src/api/favorites.ts`):**
```typescript
// frontend/src/api/favorites.ts
export async function addFavorite(payload: { simKey: string; input?: unknown }): Promise<FavoriteDto> {
  // POST /api/v1/favorites với body { simKey, input? }
}
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 3.1: Trace luồng Điều khiển Playback Từng bước trên Canvas (2.5 điểm)
- **1. Cấu trúc Step (0.75đ):** File generator trong `engines/` sinh mảng `Step[]` với interface đầy đủ:
```typescript
// frontend/src/engines/core/types.ts
export interface Step {
  index: number;
  structure: Structure;       // kind + elements[] + links[]
  explanation: string;        // tiếng Việt, 1-4 câu
  pseudocodeLine: number;     // dòng mã giả 1-based
  highlights: string[];
  annotations: string[];      // VD: ['i=2, j=3', 'so sánh a[2]=7 > a[3]=4']
  variables: Record<string, string | number | boolean | null>;
  stats: { comparisons: number; swaps: number; writes: number };
  version: 1;
}
```
- **2. Step Forward (0.5đ):** `stepForward()` tăng `currentIndex.value = Math.min(steps.length - 1, currentIndex.value + 1)`.
- **3. Canvas Render (0.75đ):** `CanvasArea.vue` vẽ lại dựa trên `currentStep`: Cột đang so sánh có màu vàng (`var(--color-warning)`), cột hoán đổi có màu cam/đỏ, cột hoàn thành có màu xanh lá (`var(--color-success)`).
- **4. Highlight Mã giả (0.5đ):** `PseudocodePanel.vue` so khớp `currentStep.line` để gắn class `.pseudocode__line--active`.

---

#### 📝 Câu TL 3.2: Trace luồng Tự thực hành - Manual Practice Mode (2.5 điểm)
- **1. Bật Practice Mode (0.5đ):** `ManualPracticePanel.vue` xuất hiện thay thế thanh điều khiển; component nhận props `steps` và `currentIndex` từ parent.
- **2. Chọn Thao tác Kế tiếp (0.75đ):** Panel hiển thị 6 radio option (`compare`, `swap`, `assign`, `move`, `insert`, `delete`) dựa trên i18n `messages.practice.options`; người dùng chọn thao tác dự đoán cho bước kế tiếp.
- **3. So khớp Logic (0.75đ):** Hàm `inferExpected()` phân tích `step.explanation` của `steps[currentIndex + 1]` (tìm từ khóa 'hoán'/'swap' → swap, 'so sánh'/'compare' → compare, v.v.) rồi so với lựa chọn. Đúng: `correctCount++`; Sai: `wrongCount++`. Sau đó emit `'skip'` để tự chuyển bước (~400ms feedback).
- **4. Hoàn thành (0.5đ):** Khi hết bước, emit `'done'` với `{ correct, wrong }`; panel hiện thông báo tổng kết.

---

# 📘 ĐÁP ÁN ĐỀ 04: MÀN HÌNH KHÁM PHÁ MÔ PHỎNG, TRA CỨU & SO SÁNH

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 4.1 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Hệ thống Khám phá gồm 3 route độc lập trong `router/index.ts`: `/simulations` → `SimulationsView.vue`, `/cheatsheet` → `CheatSheetView.vue`, `/benchmark/:k1/:k2` → `BenchmarkView.vue`. Đây là các màn hình riêng biệt, không phải tab nội bộ.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì Hệ thống Khám phá gồm 3 route độc lập trong `router/index.ts`: `/simulations` → `SimulationsView.vue`, `/cheatsheet` → `CheatSheetView.vue`, `/benchmark/:k1/:k2` → `BenchmarkView.vue`. Đây là các màn hình riêng biệt, không phải tab nội bộ.
- **⑤ Code thật (`frontend/src/router/index.ts`):**
```typescript
// frontend/src/router/index.ts
{ path: '/simulations', name: 'simulations', component: () => import('@/views/SimulationsView.vue') },
{ path: '/cheatsheet', name: 'cheatsheet', component: () => import('@/views/CheatSheetView.vue') },
{ path: '/benchmark/:k1/:k2', name: 'benchmark', component: () => import('@/views/BenchmarkView.vue') },
```

---

#### **Câu 4.2 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Tìm kiếm và lọc danh mục chạy trực tiếp trên metadata tĩnh tại Client không cần gửi request lên backend.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì Tìm kiếm và lọc danh mục chạy trực tiếp trên metadata tĩnh tại Client không cần gửi request lên backend.
- **⑤ Code thật (`frontend/src/views/SimulationsView.vue`):**
```typescript
// frontend/src/views/SimulationsView.vue
const filteredList = computed(() => {
  return catalogList.filter((item) => {
    const matchCategory = selectedCategory.value === 'all' || item.category === selectedCategory.value;
    const matchSearch = item.title.toLowerCase().includes(searchQuery.value.toLowerCase());
    return matchCategory && matchSearch;
  });
});
```

---

#### **Câu 4.3 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Hàm `complexityTone(value)` phân loại chip Big-O: chứa `'³'`/`'n^3'`/`'2^n'` → `'danger'`; chứa `'²'`/`'n^2'` → `'warning'`; còn lại → `'success'`.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì Hàm `complexityTone(value)` phân loại chip Big-O: chứa `'³'`/`'n^3'`/`'2^n'` → `'danger'`; chứa `'²'`/`'n^2'` → `'warning'`; còn lại → `'success'`.
- **⑤ Code thật (`frontend/src/views/SimulationsView.vue`):**
```typescript
// frontend/src/views/SimulationsView.vue
function complexityTone(value: string): ComplexityTone {
  const v = value.toLowerCase();
  if (v.includes('³') || v.includes('n^3') || v.includes('2^n')) return 'danger';
  if (v.includes('²') || v.includes('n^2')) return 'warning';
  return 'success';
}
```

---

#### **Câu 4.4 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `BenchmarkView.vue` sinh mảng n phần tử và chạy 2 thuật toán song song ở Web Worker Client để đo đạc thời gian thực thi.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì `BenchmarkView.vue` sinh mảng n phần tử và chạy 2 thuật toán song song ở Web Worker Client để đo đạc thời gian thực thi.
- **⑤ Code thật (`frontend/src/views/BenchmarkView.vue`):**
```typescript
// frontend/src/views/BenchmarkView.vue
async function runBenchmark(): Promise<void> {
  const array = generateDataset(dataSize.value, distributionType.value);
  const res1 = await executeWorker(k1.value, [...array]);
  const res2 = await executeWorker(k2.value, [...array]);
  benchmarkResults.value = { res1, res2 };
}
```

---

#### **Câu 4.5 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `CheatSheetView.vue` import `CATALOG` từ `@/engines/catalog` và render `CheatSheetTable.vue`; bảng lọc/nhóm theo `dataStructure` và hiển thị liên kết Wikipedia/GeeksforGeeks từ `referenceLinks.ts` theo `simKey`. Không có snippet code đa ngôn ngữ.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì `CheatSheetView.vue` import `CATALOG` từ `@/engines/catalog` và render `CheatSheetTable.vue`; bảng lọc/nhóm theo `dataStructure` và hiển thị liên kết Wikipedia/GeeksforGeeks từ `referenceLinks.ts` theo `simKey`. Không có snippet code đa ngôn ngữ.
- **⑤ Code thật (`frontend/src/views/CheatSheetView.vue`):**
```typescript
// frontend/src/views/CheatSheetView.vue
import { CATALOG } from '@/engines/catalog';
import CheatSheetTable from '@/components/lesson/CheatSheetTable.vue';
// CheatSheetTable.vue: filtered = CATALOG.filter(...), linksFor(key) → wikipedia/geeksforgeeks
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 4.1: Trace luồng Lọc danh mục $\rightarrow$ Xem chi tiết $\rightarrow$ Mở Simulator (2.5 điểm)
- **1. Khởi tạo State (0.75đ):** `onMounted()` nạp `catalogList = getCatalogList()`. `selectedCategory.value = 'graph'` lọc mảng item.
- **2. Click Card (0.5đ):** Click Card BFS kích hoạt `@click="router.push({ name: 'simulator', params: { key: 'graph.bfs' } })"`.
- **3. Router Guard (0.75đ):** Thuật toán BFS có `demoAllowed: false` $\rightarrow$ Chưa login sẽ bị chuyển về `/login?redirect=%2Fsimulator%2Fgraph.bfs`.
- **4. Render Simulator (0.5đ):** Nếu đã đăng nhập $\rightarrow$ `SimulatorView.vue` mount, nạp đồ thị mẫu và vẽ lên Canvas.

---

#### 📝 Câu TL 4.2: Trace luồng Thực thi So sánh Benchmark (2.5 điểm)
- **1. Route Params (0.5đ):** `BenchmarkView.vue` đọc `route.params.k1` và `route.params.k2`, truyền làm `defaultKeys` prop cho `BenchmarkPanel.vue`.
- **2. Sinh Dữ liệu & Đo đạc (1.0đ):** `BenchmarkPanel` sinh mảng theo `dataMode` (random/worst/best) và gọi `runMeasureInWorker(code, input)` từ `@/engines/worker/compileWorker` để đo trong Web Worker, tránh chặn UI thread:
```typescript
// frontend/src/components/benchmark/BenchmarkPanel.vue
import { runMeasureInWorker } from '@/engines/worker/compileWorker';
const result = await runMeasureInWorker(def.code, input);
```
- **3. Thu thập Metrics (0.75đ):** Kết quả trả về `BenchmarkMeasure` gồm `durationMs`, `comparisons`, `swaps`; lưu vào `rows` reactive array theo từng kích thước n.
- **4. Vẽ Biểu đồ (0.25đ):** `vue-echarts` (LineChart) render biểu đồ đường DurationMs theo n cho các thuật toán đã chọn.

---

# 📘 ĐÁP ÁN ĐỀ 05: SANDBOX TƯƠNG TÁC 4 TAB

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 5.1 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Cả 4 route sandbox đều dùng chung component `SortingView.vue`, chuyển đổi tab mượt mà bằng `:key="$route.fullPath"`.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì Cả 4 route sandbox đều dùng chung component `SortingView.vue`, chuyển đổi tab mượt mà bằng `:key="$route.fullPath"`.
- **⑤ Code thật (`frontend/src/router/index.ts`):**
```typescript
// frontend/src/router/index.ts
{ path: '/sorting-sandbox', component: SortingSandboxView },
{ path: '/searching-sandbox', component: SortingSandboxView },
{ path: '/graph-playground', component: SortingSandboxView },
{ path: '/stack-queue-sandbox', component: SortingSandboxView }
```

---

#### **Câu 5.2 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Tab Đồ thị cho phép thêm đỉnh/cạnh trực tiếp trên SVG canvas ở Client mà không cần gửi API lên server.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì Tab Đồ thị cho phép thêm đỉnh/cạnh trực tiếp trên SVG canvas ở Client mà không cần gửi API lên server.
- **⑤ Code thật (`frontend/src/views/graph/GraphView.vue`):**
```typescript
// frontend/src/views/graph/GraphView.vue
function addNode(x: number, y: number): void {
  nodes.value.push({ id: nextId++, x, y, label: `N${nextId}` });
}
```

---

#### **Câu 5.3 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `StackQueueView.vue` hiển thị animation phần tử rơi vào Stack và nối đuôi Queue kèm con trỏ Top/Front/Rear.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì `StackQueueView.vue` hiển thị animation phần tử rơi vào Stack và nối đuôi Queue kèm con trỏ Top/Front/Rear.
- **⑤ Code thật (`frontend/src/views/stackqueue/StackQueueView.vue`):**
```typescript
// frontend/src/views/stackqueue/StackQueueView.vue
function pushStack(val: number): void {
  stack.value.unshift(val);
}
function enqueue(val: number): void {
  queue.value.push(val);
}
```

---

#### **Câu 5.4 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `CodeToVisualView.vue` dịch cú pháp DSL thành lệnh gọi Canvas Painter trực tiếp tại Client.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì `CodeToVisualView.vue` dịch cú pháp DSL thành lệnh gọi Canvas Painter trực tiếp tại Client.
- **⑤ Code thật (`frontend/src/views/CodeToVisualView.vue`):**
```typescript
// frontend/src/views/CodeToVisualView.vue
function parseDsl(code: string): VisualAction[] {
  // Phân tích cú pháp: create array [1, 2, 3] -> Action CREATE_ARRAY
}
```

---

#### **Câu 5.5 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Đổi dropdown thuật toán cập nhật reactive state, reset mảng và vẽ lại cây nhị phân Heap mà không reload trang.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì Đổi dropdown thuật toán cập nhật reactive state, reset mảng và vẽ lại cây nhị phân Heap mà không reload trang.
- **⑤ Code thật (`frontend/src/views/sorting/SortingView.vue`):**
```typescript
// frontend/src/views/sorting/SortingView.vue
watch(selectedAlgorithm, (newAlgo) => {
  resetArray();
  loadAlgorithmEngine(newAlgo);
});
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 5.1: Trace luồng Tìm kiếm Nhị phân trong Searching Sandbox (2.5 điểm)
- **1. Input Dữ liệu (0.5đ):** Mảng `[3, 8, 15, 24, 42, 59, 77]`, `Target = 42`.
- **2. Khởi tạo Con trỏ (0.75đ):** `Low = 0`, `High = 6`. Tính `Mid = 3` (giá trị 24).
- **3. Thu hẹp Phạm vi (0.75đ):** `24 < 42` $\rightarrow$ Đặt `Low = 4`. `Mid = 5` (59). `59 > 42` $\rightarrow$ `High = 4`. `Mid = 4` (42).
- **4. Kết thúc (0.5đ):** Cột 4 sáng xanh, thông báo tìm thấy sau 3 bước so sánh.

---

#### 📝 Câu TL 5.2: Trace luồng Duyệt Đồ thị BFS trên Graph Playground (2.5 điểm)
- **1. Danh sách kề (0.5đ):** Đồ thị 5 đỉnh lưu trong `adj = { 0: [1, 2], ... }`.
- **2. Bắt đầu BFS (0.75đ):** `queue = [0]`, `visited[0] = true`. Đỉnh 0 đổi màu vàng.
- **3. Mở rộng đỉnh kề (0.75đ):** Lấy đỉnh 0, thêm 1 và 2 vào queue. Đỉnh 0 chuyển sang màu xanh lá.
- **4. Hoàn thành (0.5đ):** Lần lượt duyệt hết các đỉnh, thông báo thứ tự duyệt: `0 $\rightarrow$ 1 $\rightarrow$ 2 $\rightarrow$ 3 $\rightarrow$ 4`.

---

# 📘 ĐÁP ÁN ĐỀ 06: LỘ TRÌNH HỌC TẬP & HỌC BÀI HỌC

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 6.1 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `/path` được cấu hình `meta: { public: true }` cho phép khách xem danh sách lộ trình.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì `/path` được cấu hình `meta: { public: true }` cho phép khách xem danh sách lộ trình.
- **⑤ Code thật (`frontend/src/router/index.ts`):**
```typescript
// frontend/src/router/index.ts
{
  path: '/path',
  name: 'courses',
  component: () => import('@/views/courses/CoursesListView.vue'),
  meta: { public: true },
}
```

---

#### **Câu 6.2 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Nút "Tham gia lộ trình" gọi `useCourseStore.enrollCourse(id)` lưu trạng thái vào `localStorage` key `enrolled_{id}`; giao diện chuyển sang chế độ đã ghi danh mà KHÔNG gửi request lên server.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì Nút "Tham gia lộ trình" gọi `useCourseStore.enrollCourse(id)` lưu trạng thái vào `localStorage` key `enrolled_{id}`; giao diện chuyển sang chế độ đã ghi danh mà KHÔNG gửi request lên server.
- **⑤ Code thật (`frontend/src/features/courses/store/useCourseStore.ts`):**
```typescript
// frontend/src/features/courses/store/useCourseStore.ts
function enrollCourse(courseId: string) {
  localStorage.setItem(`enrolled_${courseId}`, 'true');
}
```

---

#### **Câu 6.3 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `LessonStudyView.vue` render component theo `sandboxType` của bài học: stepper chuẩn gồm **Lý thuyết → Quiz → Code Lab** (bài có `codelabTask`); visualizer mô phỏng được nhúng sẵn trong bước Lý thuyết qua prop `simulation-key` (không phải một step riêng). Bài dạng `quiz` chỉ hiển thị riêng tab Quiz. Chuyển bước qua sự kiện `@completeStep` / `@completeLesson`. A/B/C sai vì không có 100 câu timed quiz, không bắt buộc nộp code trước, và không có livestream.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì `LessonStudyView.vue` render component theo `sandboxType` của bài học: stepper chuẩn gồm **Lý thuyết → Quiz → Code Lab** (bài có `codelabTask`); visualizer mô phỏng được nhúng sẵn trong bước Lý thuyết qua prop `simulation-key` (không phải một step riêng). Bài dạng `quiz` chỉ hiển thị riêng tab Quiz. Chuyển bước qua sự kiện `@completeStep` / `@completeLesson`. A/B/C sai vì không có 100 câu timed quiz, không bắt buộc nộp code trước, và không có livestream.
- **⑤ Code thật (`frontend/src/views/lesson/LessonStudyView.vue`):**
```html
<!-- frontend/src/views/lesson/LessonStudyView.vue -->
<LessonStepTheory v-if="!sandboxType || sandboxType === 'dsa'"
  :simulation-key="lessonStore.simulationKey" @completeStep="onQuizComplete" />
<LessonStepQuiz v-else-if="sandboxType === 'quiz'"
  :questions="lessonStore.currentLesson.quizQuestions ?? []" @submit="onQuizSubmit" />
<LessonStepCodeLab v-else-if="sandboxType === 'codelab' && lessonStore.currentLesson.codelabTask"
  @completeLesson="onLessonComplete" />
```
```typescript
// Stepper chuẩn — LessonStudyView.vue:263-275
const FULL_STEPS = [
  { number: 1, label: 'Lý Thuyết' },
  { number: 2, label: 'Quiz' },
  { number: 3, label: 'Code Lab' },
];
const steps = computed(() => {
  if (lessonStore.lessonMeta?.sandboxType === 'quiz') return [FULL_STEPS[1]]; // chỉ tab Quiz
  const hasCodelab = !!lessonStore.currentLesson?.codelabTask;
  return hasCodelab ? FULL_STEPS : FULL_STEPS.slice(0, 2);
});
```

---

#### **Câu 6.4 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Nút hoàn thành gọi `lessonStore.markLessonCompleted(id)` — đánh dấu cục bộ (localStorage `dsa.completedLessons`) và cộng XP một lần qua `POST /concepts/auth/award-xp` `{ amount, reason }` (kiểm tra `xpAwarded < totalXp` chống cộng trùng); sau đó `syncToServer(true)` đẩy tiến độ qua `POST /concepts/auth/progress/{lessonId}` kèm cờ `completed`, cuối cùng hiện `LessonCompletionModal` với `+{{ xpReward }} XP`. Lưu ý: `POST /lessons/{id}/mark-viewed` là endpoint đánh dấu đã xem của view cũ (`LessonView`/`LessonDetail`), không phải luồng hoàn thành của `LessonStudyView`. A sai vì không có endpoint `/complete`; B sai vì có request đồng bộ thật; D sai vì hoàn thành bài không trừ tim.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì Nút hoàn thành gọi `lessonStore.markLessonCompleted(id)` — đánh dấu cục bộ (localStorage `dsa.completedLessons`) và cộng XP một lần qua `POST /concepts/auth/award-xp` `{ amount, reason }` (kiểm tra `xpAwarded < totalXp` chống cộng trùng); sau đó `syncToServer(true)` đẩy tiến độ qua `POST /concepts/auth/progress/{lessonId}` kèm cờ `completed`, cuối cùng hiện `LessonCompletionModal` với `+{{ xpReward }} XP`. Lưu ý: `POST /lessons/{id}/mark-viewed` là endpoint đánh dấu đã xem của view cũ (`LessonView`/`LessonDetail`), không phải luồng hoàn thành của `LessonStudyView`. A sai vì không có endpoint `/complete`; B sai vì có request đồng bộ thật; D sai vì hoàn thành bài không trừ tim.
- **⑤ Code thật (`frontend/src/views/lesson/LessonStudyView.vue` & `frontend/src/features/lesson/services/lessonApi.ts`):**
```typescript
// frontend/src/views/lesson/LessonStudyView.vue — finishLesson()
async function finishLesson(): Promise<void> {
  await lessonStore.markLessonCompleted(lessonId.value);
  void lessonStore.syncToServer(true); // node pass → mở khoá bài tiếp theo
  showCompletionModal.value = true;
}

// frontend/src/features/lesson/services/lessonApi.ts
export async function awardXp(amount: number, reason = 'Hoàn thành nhiệm vụ bài học') {
  const res = await fetch(`${API_BASE}/concepts/auth/award-xp`, {
    method: 'POST', /* headers auth + body { amount, reason } */
  });
  return res.json();
}
```

---

#### **Câu 6.5 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `LessonStudyView.vue` là giao diện mới chuẩn hóa; `LessonView.vue` giữ lại để tương thích ngược link cũ.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì `LessonStudyView.vue` là giao diện mới chuẩn hóa; `LessonView.vue` giữ lại để tương thích ngược link cũ.
- **⑤ Code thật (`frontend/src/router/index.ts`):**
```typescript
// frontend/src/router/index.ts
{ path: '/lessons/:id', component: () => import('@/views/lesson/LessonStudyView.vue') },
{ path: '/learn/:lessonId', component: LessonView } // legacy
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 6.1: Trace luồng Đăng ký Lộ trình $\rightarrow$ Mở khóa Bài 1 (2.5 điểm)
- **1. Load Chi tiết (0.5đ):** `CourseDetailView.vue` gọi `GET /concepts/courses/{id}` (qua `courseApi.getCourseById`) nhận cây bài học kèm trạng thái hoàn thành từng bài.
- **2. Ghi danh Client-side (0.75đ):** `useCourseStore.enrollCourse(topicId)` lưu `localStorage.setItem('enrolled_' + topicId, 'true')`; KHÔNG gửi request lên server.
- **3. State Update (0.75đ):** Store cập nhật reactive state; danh sách bài học đọc từ `localStorage` key `enrolled_{id}` để xác định đã ghi danh hay chưa.
- **4. UI Update (0.5đ):** Nút chuyển thành "Bắt đầu học", Node 1 sáng lên cho phép bấm vào học.

---

#### 📝 Câu TL 6.2: Trace luồng Hoàn thành Bài học $\rightarrow$ Đồng bộ Tiến độ & XP (2.5 điểm)
- **1. Action Call (0.5đ):** `finishLesson()` gọi lần lượt `lessonStore.markLessonCompleted(lessonId)` → `lessonStore.syncToServer(true)` → mở `LessonCompletionModal`.
- **2. Award XP (0.75đ):** `markLessonCompleted` cộng XP một lần qua `POST /concepts/auth/award-xp` với `{ amount, reason }`; điều kiện `xpAwarded < totalXp` (totalXp = `lesson.xpReward ?? 100`) chống cộng trùng XP khi học lại; số XP đã cộng cũng được lưu localStorage.
- **3. Sync Progress (0.75đ):** `syncToServer` đẩy payload `{ lessonId, hasWatchedVisualizer, quizScore, bestScore, codelabCompleted, xpAwarded, completed: true }` lên `POST /concepts/auth/progress/{lessonId}`; nếu thất bại (mất mạng) → lưu localStorage + retry tự động sau 10 giây (`setTimeout(syncToServer, 10000)`).
- **4. Modal & Bài kế (0.5đ):** `LessonCompletionModal` hiển thị `+{{ xpReward }} XP`; `resolveNextLessonId()` gọi lại `courseApi.getCourseById(courseId)` lấy danh sách lesson theo đúng thứ tự (module → item, không sort lại theo orderIndex) rồi trả về id bài kế tiếp.

---

# 📘 ĐÁP ÁN ĐỀ 07: THANG THỰC HÀNH 3 BẬC & TRẮC NGHIỆM CHẤM ĐIỂM

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 7.1 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Practice Ladder 3 Bậc: Bậc 1 (Quiz) $\rightarrow$ Bậc 2 (Interactive Lab) $\rightarrow$ Bậc 3 (Code Challenge).
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì Practice Ladder 3 Bậc: Bậc 1 (Quiz) $\rightarrow$ Bậc 2 (Interactive Lab) $\rightarrow$ Bậc 3 (Code Challenge).
- **⑤ Code thật (`frontend/src/views/LadderView.vue`):**
```html
<!-- frontend/src/views/LadderView.vue -->
<div class="ladder__stages">
  <div class="ladder__card ladder__card--stage1">Bậc 1: Quiz Trắc nghiệm</div>
  <div class="ladder__card ladder__card--stage2" :class="{ locked: !stage1Passed }">Bậc 2: Interactive Lab</div>
  <div class="ladder__card ladder__card--stage3" :class="{ locked: !stage2Passed }">Bậc 3: Code Runner</div>
</div>
```

---

#### **Câu 7.2 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Hoàn thành Bậc trước mới mở khóa Bậc sau; các bậc chưa đạt điều kiện sẽ bị khóa icon ổ khóa xám.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì Hoàn thành Bậc trước mới mở khóa Bậc sau; các bậc chưa đạt điều kiện sẽ bị khóa icon ổ khóa xám.
- **⑤ Code thật (`frontend/src/views/LadderView.vue`):**
```typescript
// frontend/src/views/LadderView.vue
const stage2Unlocked = computed(() => (summary.value?.stage1Score ?? 0) >= (summary.value?.stage1MaxScore ?? 10));
const stage3Unlocked = computed(() => stage2Unlocked.value && (summary.value?.stage2Passed ?? false));
```

---

#### **Câu 7.3 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `ExerciseService.SubmitAsync` CHỈ chấm điểm, trả về kết quả từng câu kèm `explanation` — KHÔNG chạm tới tim (ExerciseService.cs không có logic hearts). Tim bị trừ 1 tại thời điểm **bắt đầu phiên luyện node** qua `GamificationService.EnterNodeAsync` (UPDATE atomic `Hearts = Hearts - 1 WHERE Hearts > 0`; node đã PASS vào lại miễn phí; Premium vẫn bị trừ, chỉ có HeartsMax 30 + hồi 10 phút/tim). A sai vì không cho chọn lại vô hạn (có giới hạn tim vào node); C sai vì không khóa tài khoản; D sai vì không trừ Gems khi sai.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì `ExerciseService.SubmitAsync` CHỈ chấm điểm, trả về kết quả từng câu kèm `explanation` — KHÔNG chạm tới tim (ExerciseService.cs không có logic hearts). Tim bị trừ 1 tại thời điểm **bắt đầu phiên luyện node** qua `GamificationService.EnterNodeAsync` (UPDATE atomic `Hearts = Hearts - 1 WHERE Hearts > 0`; node đã PASS vào lại miễn phí; Premium vẫn bị trừ, chỉ có HeartsMax 30 + hồi 10 phút/tim). A sai vì không cho chọn lại vô hạn (có giới hạn tim vào node); C sai vì không khóa tài khoản; D sai vì không trừ Gems khi sai.
- **⑤ Code thật (`backend/src/DsaVisual.Application/Services/GamificationService.cs`):**
```csharp
// backend/src/DsaVisual.Application/Services/GamificationService.cs
// Phí vào node — UPDATE điều kiện chống double-spend (FR-10.1)
// (a) Trừ tim atomic; node đã PASS → xem lại miễn phí (AC-10.1.3)
if (!alreadyPassed) {
  var affected = await db.Database.ExecuteSqlInterpolatedAsync(
    $"UPDATE Users SET Hearts = Hearts - 1 WHERE Id = {userId} AND Hearts > 0", ct);
}
// ExerciseService.SubmitAsync: KHÔNG trừ tim — chỉ trả SubmitResultDto { score, results[], explanation }
```

---

#### **Câu 7.4 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `LabView.vue` chấm điểm thao tác trực quan qua cơ chế so khớp trạng thái `STATE_MATCH` trong `AnswerJson`.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì `LabView.vue` chấm điểm thao tác trực quan qua cơ chế so khớp trạng thái `STATE_MATCH` trong `AnswerJson`.
- **⑤ Code thật (`backend/src/DsaVisual.Application/Services/ExerciseService.cs`):**
```csharp
// backend/src/DsaVisual.Application/Services/ExerciseService.cs
// Câu hỏi LAB lưu AnswerJson dạng {"type":"STATE_MATCH","finalState":[...],"maxSteps":n}
if (answerDef.Type == "STATE_MATCH")
{
    isCorrect = MatchState(userFinalState, answerDef.FinalState) && stepsUsed <= answerDef.MaxSteps * 1.5;
}
```

---

#### **Câu 7.5 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `SubmissionLockRegistry` khóa per-user-exercise chống nộp trùng lặp.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì `SubmissionLockRegistry` khóa per-user-exercise chống nộp trùng lặp.
- **⑤ Code thật (`backend/src/DsaVisual.Application/Services/ExerciseService.cs`):**
```csharp
// backend/src/DsaVisual.Application/Services/ExerciseService.cs
using var lockHandle = locks.TryAcquire(userId, exerciseId);
if (lockHandle is null)
{
    return Result<SubmitResultDto>.Fail(ErrorCodes.SUBMISSION_IN_PROGRESS, "Đang chấm bài, vui lòng không nộp trùng");
}
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 7.1: Trace luồng Làm bài Quiz $\rightarrow$ Chấm điểm $\rightarrow$ Kết quả từng câu (2.5 điểm)
- **1. Request Nộp bài (0.5đ):** `POST /api/v1/exercises/{id}/submit` với `SubmitRequest { answers: [{ questionId, selected: [2] }] }` (selected là mảng — hỗ trợ câu MULTIPLE choice; kèm `durationSeconds?`, `classAssignmentId?`, `clientRequestId?` idempotency).
- **2. So khớp Đáp án (0.75đ):** `ExerciseService` so khớp `selected` với `CorrectAnswer` từng câu $\rightarrow$ trả `results[]` với `correct: false, correctAnswer, explanation` cho câu sai.
- **3. KHÔNG trừ tim ở submit (0.75đ):** `SubmitResultDto` gồm `{ score, maxScore, passed, results[], submissionId, submittedAt }` — KHÔNG có trường heartsLeft. Tim bị trừ 1 (UPDATE atomic `Hearts = Hearts - 1 WHERE Hearts > 0`) tại thời điểm **bắt đầu phiên luyện node** (`EnterNodeAsync`); node đã PASS vào lại miễn phí; Premium vẫn bị trừ (chỉ HeartsMax 30 + hồi 10 phút/tim).
- **4. UI Cảnh báo (0.5đ):** Hiện viền đỏ câu chọn sai, viền xanh đáp án đúng kèm giải thích. Khi hết tim mà bấm vào node mới $\rightarrow$ backend trả 403 `HEARTS_EMPTY` $\rightarrow$ frontend toast warning "Bạn đã hết tim. Hãy chờ hồi hoặc nâng cấp Premium." (PathView.vue:146).

---

#### 📝 Câu TL 7.2: Trace luồng Hoàn thành Bậc 2 Lab $\rightarrow$ Mở khóa Bậc 3 Code Challenge (2.5 điểm)
- **1. Thao tác Lab (0.5đ):** Học viên kéo thả đỉnh cây trong `LabView.vue` $\rightarrow$ đóng gói `finalState`.
- **2. Gửi Trạng thái (0.75đ):** `POST /api/v1/exercises/{id}/submit` với `{ state: finalState, stepsUsed: 2 }`.
- **3. Backend Xác thực (0.75đ):** So khớp trạng thái cuối và số bước $\le 1.5 \times \text{maxSteps}$. Ghi nhận hoàn thành Bậc 2.
- **4. Mở khóa Bậc 3 (0.5đ):** Quay lại `LadderView.vue`, Thẻ Bậc 3 mở khóa và sáng nút "Lập trình ngay".

---

# 📘 ĐÁP ÁN ĐỀ 08: TRÌNH CHẠY CODE SANDBOX & CHẤM ĐIỂM SERVER

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 8.1 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `CodeRunnerView.vue` nạp template mẫu có sẵn `compare/swap` và nạp generator tương ứng từ `simStore`.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì `CodeRunnerView.vue` nạp template mẫu có sẵn `compare/swap` và nạp generator tương ứng từ `simStore`.
- **⑤ Code thật (`frontend/src/stores/codeRunner.ts`):**
```typescript
// frontend/src/stores/codeRunner.ts
const TEMPLATES: Record<string, string> = {
  'sort.bubble': `function bubbleSort(a) {
  const n = a.length;
  for (let i = 0; i < n - 1; i++) {
    for (let j = 0; j < n - i - 1; j++) {
      compare(j, j + 1);
      if (a[j] > a[j + 1]) swap(j, j + 1);
    }
  }
}
bubbleSort(array);`
};
```

---

#### **Câu 8.2 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: "Chạy thử" chạy ở Web Sandbox Client `runCode()` sinh `TraceEvent[]` phát lại trên Canvas.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì "Chạy thử" chạy ở Web Sandbox Client `runCode()` sinh `TraceEvent[]` phát lại trên Canvas.
- **⑤ Code thật (`frontend/src/stores/codeRunner.ts`):**
```typescript
// frontend/src/stores/codeRunner.ts
const result = await Promise.resolve(
  runCode({ code: editorCode.value, entry: 'solve', bindings: [] }, defaultArray)
);
lastOutput.value = result.output;
lastStats.value = result.stats;
```

---

#### **Câu 8.3 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: "Nộp bài" gửi code lên backend để `CodelabJudgeService` (Jint Engine) chấm với Test Case ẩn độc lập.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì "Nộp bài" gửi code lên backend để `CodelabJudgeService` (Jint Engine) chấm với Test Case ẩn độc lập.
- **⑤ Code thật (`backend/src/DsaVisual.Application/Services/CodelabJudgeService.cs`):**
```csharp
// backend/src/DsaVisual.Application/Services/CodelabJudgeService.cs
using var engine = new Engine(options => options.TimeoutInterval(TimeSpan.FromMilliseconds(1500)).LimitMemory(32 * 1024 * 1024)); // DefaultTimeoutMs = 1500, MaxMemoryBytes = 32MB
engine.Execute(userCode);
var result = engine.Invoke("solve", testCaseInput).ToObject();
```

---

#### **Câu 8.4 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `useCodeTracePlayback` hỗ trợ Play/Pause, Step +/-, timeline và đổi tốc độ đồng bộ với từng dòng code.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì `useCodeTracePlayback` hỗ trợ Play/Pause, Step +/-, timeline và đổi tốc độ đồng bộ với từng dòng code.
- **⑤ Code thật (`frontend/src/composables/useCodeTracePlayback.ts`):**
```typescript
// frontend/src/composables/useCodeTracePlayback.ts
export function useCodeTracePlayback() {
  const currentIndex = ref(0);
  const isPlaying = ref(false);
  function play() { ... }
  function stepForward() { ... }
  return { currentIndex, isPlaying, play, stepForward };
}
```

---

#### **Câu 8.5 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Drawer Lịch sử nộp gọi `codeStore.fetchHistory(exerciseId)` → `GET /api/v1/exercises/{id}/submissions/me` trả `PagedResponse<SubmissionSummaryDto>` — hiển thị danh sách các lần nộp cũ kèm nút nạp lại code. A/C/D sai vì không có file PDF, không xóa lịch sử, và không liên quan bảng xếp hạng lớp.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì Drawer Lịch sử nộp gọi `codeStore.fetchHistory(exerciseId)` → `GET /api/v1/exercises/{id}/submissions/me` trả `PagedResponse<SubmissionSummaryDto>` — hiển thị danh sách các lần nộp cũ kèm nút nạp lại code. A/C/D sai vì không có file PDF, không xóa lịch sử, và không liên quan bảng xếp hạng lớp.
- **⑤ Code thật (`frontend/src/api/exercises.ts` & `frontend/src/views/CodeRunnerView.vue`):**
```typescript
// frontend/src/api/exercises.ts — endpoint thật
mySubmissions: (id: number) => `/exercises/${id}/submissions/me`,

// frontend/src/views/CodeRunnerView.vue
async function toggleHistory(): Promise<void> {
  historyOpen.value = !historyOpen.value;
  if (historyOpen.value) {
    await codeStore.fetchHistory(exerciseId);
  }
}
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 8.1: Trace luồng Chạy Sandbox Client $\rightarrow$ Sinh Trace $\rightarrow$ Playback Canvas (2.5 điểm)
- **1. Thực thi Sandbox (0.75đ):** `codeStore.run()` gọi `runCode(...)` với mảng `[5, 3, 8, 1, 9, 2, 7]`.
- **2. Bắt Sự kiện (0.5đ):** `compare()` và `swap()` ghi nhận chuỗi `TraceEvent[]` với giới hạn 50.000 events.
- **3. Xử lý Lỗi (0.5đ):** Nếu cú pháp sai $\rightarrow$ `runState = 'error'`, `runError = "Lỗi dòng X: Y"`.
- **4. Phát lại Canvas (0.75đ):** Thành công $\rightarrow$ nạp `result.trace` vào `playback.init(trace)` $\rightarrow$ `CanvasArea` phát lại động.

---

#### 📝 Câu TL 8.2: Trace luồng Nộp bài Chấm điểm Server với Test Cases Ẩn (2.5 điểm)
- **1. Request (0.5đ):** `POST /api/v1/exercises/{id}/code-submit` với `{ code, language }`.
- **2. Jint Engine (0.75đ):** `CodelabJudgeService.cs` tạo Engine cô lập (Timeout 2s, Memory 32MB).
- **3. Nạp Test Cases (0.75đ):** Chạy 5-10 test cases ẩn từ DB, so sánh Output thực tế với Expected Output.
- **4. Lưu & Trả Kết quả (0.5đ):** Lưu vào `CodeSubmissions`, UI hiển thị Bảng kết quả Xanh/Đỏ.

---

# 📘 ĐÁP ÁN ĐỀ 09: HỆ THỐNG GAMIFICATION: NHIỆM VỤ & BẢNG XẾP HẠNG

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 9.1 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `QuestsView.vue` gọi đồng thời `fetchQuests()` và `fetchSummary()` qua `Promise.allSettled`.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì `QuestsView.vue` gọi đồng thời `fetchQuests()` và `fetchSummary()` qua `Promise.allSettled`.
- **⑤ Code thật (`frontend/src/views/QuestsView.vue`):**
```typescript
// frontend/src/views/QuestsView.vue
onMounted(async () => {
  await Promise.allSettled([gamificationStore.fetchQuests(), gamificationStore.fetchSummary()]);
});
```

---

#### **Câu 9.2 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Bấm "Nhận thưởng" gọi `POST /api/v1/gamification/quests/{id}/claim` cộng Gems/XP và bắn pháo hoa.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì Bấm "Nhận thưởng" gọi `POST /api/v1/gamification/quests/{id}/claim` cộng Gems/XP và bắn pháo hoa.
- **⑤ Code thật (`frontend/src/stores/gamification.ts`):**
```typescript
// frontend/src/stores/gamification.ts
async function claimQuest(id: number): Promise<void> {
  const reward = await gamificationApi.claimQuest(id);
  const quest = quests.value.find((q) => q.id === id);
  if (quest) quest.claimed = true;
  gems.value += reward.gems;
  xp.value += reward.xp;
}
```

---

#### **Câu 9.3 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Quests tự động reset vào 00:00 UTC (07:00 sáng VN) mỗi ngày (2 Dễ, 2 Trung bình, 1 Khó).
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì Quests tự động reset vào 00:00 UTC (07:00 sáng VN) mỗi ngày (2 Dễ, 2 Trung bình, 1 Khó).
- **⑤ Code thật (`backend/src/DsaVisual.Application/Services/QuestProgressWriter.cs`):**
```csharp
// backend/src/DsaVisual.Application/Services/QuestProgressWriter.cs
// Reset theo ngày UTC (00:00 UTC) — 5 nhiệm vụ hàng ngày
var todayUtc = _clock.UtcNow.Date;
```

---

#### **Câu 9.4 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Leaderboard có 3 tab: Tuần (week), Level (level), Lớp (class) — KHÔNG phải All-time/Weekly/Classroom.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì Leaderboard có 3 tab: Tuần (week), Level (level), Lớp (class) — KHÔNG phải All-time/Weekly/Classroom.
- **⑤ Code thật (`frontend/src/views/LeaderboardView.vue`):**
```typescript
// frontend/src/views/LeaderboardView.vue
const tabs: Array<{ key: 'week' | 'level' | 'class'; label: string }> = [
  { key: 'week', label: 'Tuần' },
  { key: 'level', label: 'Level' },
  { key: 'class', label: 'Lớp' },
];
```

---

#### **Câu 9.5 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Leaderboard dùng biểu đồ cột ngang (bar chart) vẽ bằng ECharts hiển thị top ~10; top-3 phân biệt màu sắc (vàng/bạc/đồng) nhưng KHÔNG dùng bục podium 3D.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì Leaderboard dùng biểu đồ cột ngang (bar chart) vẽ bằng ECharts hiển thị top ~10; top-3 phân biệt màu sắc (vàng/bạc/đồng) nhưng KHÔNG dùng bục podium 3D.
- **⑤ Code thật (`frontend/src/views/LeaderboardView.vue`):**
```typescript
// frontend/src/views/LeaderboardView.vue
// Bar chart top 10 (vue-echarts lazy)
type: 'bar' as const,
barWidth: 14,
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 9.1: Trace luồng Hoàn thành Nhiệm vụ $\rightarrow$ Nhận thưởng Gems & Level Up (2.5 điểm)
- **1. Action Call (0.5đ):** `gamificationStore.claimQuest(id)`.
- **2. Request (0.75đ):** `POST /api/v1/gamification/me/quests/{id}/claim` (lưu ý prefix `/me/`).
- **3. Backend Xử lý Level Up (0.75đ):** Cộng XP/Gems vào `UserGamification`; tính level mới bằng `ComputeLevel(xp)` = `1 + floor(sqrt(xp/100))`. Với 380+30=410 XP: `1 + floor(sqrt(4.1))` = 3; `xpFloor` level 3 = `(3-1)²×100` = 400; `xpIntoLevel` = 410−400 = 10. KHÔNG có cơ chế "XP overflow carry" — level được tính trực tiếp từ tổng XP.
```csharp
// backend/src/DsaVisual.Application/Services/GamificationService.cs
var xpFloor = (level - 1) * (level - 1) * 100;
var xpIntoLevel = Math.Max(0, xp - xpFloor);
var xpForNextLevel = 100 * (2 * level - 1);
```
- **4. UI Render (0.5đ):** Topbar tăng Gems, Level cập nhật thành 3, thanh tiến độ hiển thị 10/xpForNextLevel.

---

#### 📝 Câu TL 9.2: Trace luồng Xem Bảng Xếp Hạng Nội bộ Lớp học (2.5 điểm)
- **1. Tab Switch (0.5đ):** Click tab "Lớp" (`tab = 'class'`) $\rightarrow$ kiểm tra `classStore.myClasses` có dữ liệu hay không.
- **2. Dropdown Lớp (0.75đ):** Nếu có nhiều lớp, dropdown cho chọn; `classId` được truyền làm query param vào API.
- **3. Backend Query (0.75đ):** `GET /api/v1/leaderboard?tab=class&classId=...` (KHÔNG phải `/api/v1/gamification/leaderboard?type=class`) trả về danh sách học viên trong lớp xếp theo tiêu chí tương ứng tab.
- **4. Empty State (0.5đ):** Chưa tham gia lớp nào $\rightarrow$ hiển thị thông điệp hướng dẫn và nút điều hướng về route đăng ký lớp.

---

# 📘 ĐÁP ÁN ĐỀ 10: CỬA HÀNG GEMS & GÓI HỘI VIÊN PREMIUM

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 10.1 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `ShopView.vue` so sánh `gems >= item.price` để enable/disable nút mua và hiện cảnh báo đỏ nếu thiếu Gems.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì `ShopView.vue` so sánh `gems >= item.price` để enable/disable nút mua và hiện cảnh báo đỏ nếu thiếu Gems.
- **⑤ Code thật (`frontend/src/views/ShopView.vue`):**
```typescript
// frontend/src/views/ShopView.vue
const canAfford = (item: ShopItem) => gamificationStore.gems >= item.price;
```

---

#### **Câu 10.2 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Theo `SeedData.cs`, ShopItems seed gồm 8 vật phẩm chia theo Type: Avatar (Cyber Hacker 100G, Golden Knight 200G, Neon Ninja 150G, Code Wizard 250G, AI Companion 50G) và Frame (Neon Border 300G, Royal Gold 500G, Cyberpunk Frame 400G). Không có Heart Refill/Streak Freeze/Hint Token trong seed data.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì Theo `SeedData.cs`, ShopItems seed gồm 8 vật phẩm chia theo Type: Avatar (Cyber Hacker 100G, Golden Knight 200G, Neon Ninja 150G, Code Wizard 250G, AI Companion 50G) và Frame (Neon Border 300G, Royal Gold 500G, Cyberpunk Frame 400G). Không có Heart Refill/Streak Freeze/Hint Token trong seed data.
- **⑤ Code thật (`backend/src/DsaVisual.Application/Persistence/Seed/SeedData.cs`):**
```csharp
// backend/src/DsaVisual.Application/Persistence/Seed/SeedData.cs:120-127
new("avatar-cyber-hacker", "Cyber Hacker", 100, Type: 1),
new("avatar-gold-knight", "Golden Knight", 200, Type: 1),
new("avatar-neon-ninja", "Neon Ninja", 150, Type: 1),
new("avatar-wizard", "Code Wizard", 250, Type: 1),
new("avatar-ai-bot", "AI Companion", 50, Type: 1),
new("frame-neon", "Neon Border", 300, Type: 2),
new("frame-gold", "Royal Gold", 500, Type: 2),
new("frame-cyber", "Cyberpunk Frame", 400, Type: 2),
```

---

#### **Câu 10.3 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Premium ảnh hưởng tới **trần tim và tốc độ hồi**, không miễn trừ trừ tim: HeartsMax = 30 (Free 10) + hồi 10 phút/tim (Free 30 phút/tim) — GamificationService.cs:17. Khi gói hết hạn, `EnsureHeartsMaxSyncAsync` (lazy downgrade, BUG-1/FR-10.7) clamp HeartsMax/Hearts về ngưỡng Free ngay tại lần đọc kế tiếp. Phí vào node session (`Hearts - 1`) áp dụng cho cả Premium — node đã PASS mới vào lại miễn phí. A sai vì không có miễn trừ trừ tim; C sai vì Premium không liên quan role; D sai vì validation mật khẩu áp dụng mọi tài khoản.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì Premium ảnh hưởng tới **trần tim và tốc độ hồi**, không miễn trừ trừ tim: HeartsMax = 30 (Free 10) + hồi 10 phút/tim (Free 30 phút/tim) — GamificationService.cs:17. Khi gói hết hạn, `EnsureHeartsMaxSyncAsync` (lazy downgrade, BUG-1/FR-10.7) clamp HeartsMax/Hearts về ngưỡng Free ngay tại lần đọc kế tiếp. Phí vào node session (`Hearts - 1`) áp dụng cho cả Premium — node đã PASS mới vào lại miễn phí. A sai vì không có miễn trừ trừ tim; C sai vì Premium không liên quan role; D sai vì validation mật khẩu áp dụng mọi tài khoản.
- **⑤ Code thật (`backend/src/DsaVisual.Application/Services/GamificationService.cs`):**
```csharp
// backend/src/DsaVisual.Application/Services/GamificationService.cs
// Heart regen: Free 30 phút/tim (HeartsMax 10), Premium 10 phút/tim (HeartsMax 30) — FR-10.1  (L17)
user.HeartsMax = 30;                     // Premium 30 tim (FR-10.7)  (L1038)

// Lazy downgrade khi hết hạn (L1191-1204)
// BUG-1: premium hết hạn → clamp HeartsMax/Hearts về Free NGAY khi đọc
$"UPDATE Users SET HeartsMax = 10, Hearts = CASE WHEN Hearts > 10 THEN 10 ELSE Hearts END
  WHERE Id = {user.Id} AND PremiumUntil <= {now}"
```

---

#### **Câu 10.4 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Nội dung chuyển khoản tự động sinh theo định dạng `DSV{userId}T{months}` (VD: `DSV42T12`). User KHÔNG được chỉnh sửa. QR payload build qua `buildVietQrPayload(MB_BENEFICIARY, amount, transferContent)`. Đếm ngược 60 giây trước khi enable nút xác nhận.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì Nội dung chuyển khoản tự động sinh theo định dạng `DSV{userId}T{months}` (VD: `DSV42T12`). User KHÔNG được chỉnh sửa. QR payload build qua `buildVietQrPayload(MB_BENEFICIARY, amount, transferContent)`. Đếm ngược 60 giây trước khi enable nút xác nhận.
- **⑤ Code thật (`frontend/src/views/PremiumView.vue`):**
```typescript
// frontend/src/views/PremiumView.vue:69-72
// Nội dung CK tự động DSV{userId}T{months} — user KHÔNG tự ghi (pm-decision-log-gp.md)
const transferContent = computed(() => {
  if (!checkoutPlan.value || userId.value === null) return '';
  return `DSV${userId.value}T${checkoutPlan.value.months}`;
});
```

---

#### **Câu 10.5 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `SubscriptionView.vue` cho phép xem ngày hết hạn gói, lịch sử thanh toán và quản lý hủy gia hạn.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì `SubscriptionView.vue` cho phép xem ngày hết hạn gói, lịch sử thanh toán và quản lý hủy gia hạn.
- **⑤ Code thật (`frontend/src/views/SubscriptionView.vue`):**
```html
<!-- frontend/src/views/SubscriptionView.vue -->
<div class="subscription__details">
  <p>Gói hiện tại: <strong>{{ sub.planName }}</strong></p>
  <p>Hết hạn vào: <strong>{{ formatDate(sub.expiresAt) }}</strong></p>
</div>
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 10.1: Trace luồng Mua Bình Hồi Tim trong Shop bằng Gems (2.5 điểm)
- **1. Modal Xác nhận (0.5đ):** Click "Mua ngay" $\rightarrow$ mở Modal xác nhận giá 100 Gems.
- **2. Request (0.75đ):** `POST /api/v1/gamification/shop/{id}/buy`.
- **3. Backend Transaction (0.75đ):**
```csharp
// backend/src/DsaVisual.Application/Services/GamificationService.cs
using var tx = await _db.Database.BeginTransactionAsync(ct);
userGamification.Gems -= item.Price;
userGamification.Hearts = userGamification.HeartsMax;
await _db.SaveChangesAsync(ct);
await tx.CommitAsync(ct);
```
- **4. UI Update (0.5đ):** State cập nhật `gems = 50, hearts = 5`, Topbar hiện 5 Tim đỏ, Toast "Mua thành công!".

---

#### 📝 Câu TL 10.2: Trace luồng Mua Gói Premium $\rightarrow$ Kích hoạt Quyền Lợi Toàn Hệ Thống (2.5 điểm)
- **1. Chọn Gói (0.5đ):** Chọn gói 1 Năm $\rightarrow$ VietQR render với nội dung `DSA PRE 12M {USER_ID}`.
- **2. Webhook / Xác nhận (0.75đ):** Backend nhận webhook thanh toán $\rightarrow$ tạo bản ghi trong `Subscriptions`.
- **3. Cập nhật Role (0.75đ):** `Users.IsPremium = true` (hoặc `Role = 'PREMIUM'`).
- **4. Miễn trừ Tim (0.5đ):** `ExerciseService.SubmitAsync` kiểm tra `user.IsPremium == true` $\rightarrow$ không trừ tim khi làm sai bài tập.

---

# 📘 ĐÁP ÁN ĐỀ 11: HỒ SƠ CÁ NHÂN, THÀNH TÍCH & CÀI ĐẶT

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 11.1 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Reactive state `tab` khai báo union type `'overview' | 'progress' | 'achievements' | 'inventory' | 'settings'` tại dòng 64. Mỗi giá trị map với một `<section v-else-if="tab === '...'">` tương ứng trong template.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì Reactive state `tab` khai báo union type `'overview' | 'progress' | 'achievements' | 'inventory' | 'settings'` tại dòng 64. Mỗi giá trị map với một `<section v-else-if="tab === '...'">` tương ứng trong template.
- **⑤ Code thật (`frontend/src/views/ProfileView.vue`):**
```typescript
// frontend/src/views/ProfileView.vue:64
const tab = ref<'overview' | 'progress' | 'achievements' | 'inventory' | 'settings'>('overview');
```

---

#### **Câu 11.2 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Activity Heatmap kiểu GitHub thể hiện số bài học/mô phỏng hoàn thành trong từng ngày của năm.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì Activity Heatmap kiểu GitHub thể hiện số bài học/mô phỏng hoàn thành trong từng ngày của năm.
- **⑤ Code thật (`frontend/src/views/ProfileView.vue`):**
```typescript
// frontend/src/views/ProfileView.vue
// Render 52 tuần x 7 ngày, màu đậm dần theo số lượng activity count
```

---

#### **Câu 11.3 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Endpoint trang bị là `PUT /api/v1/gamification/me/inventory/equip` với body `EquipRequest { ItemId, IsEquipped }`. Backend `EquipItemAsync` cập nhật trạng thái trong `UserInventory`, FE refetch inventory sau khi thành công.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì Endpoint trang bị là `PUT /api/v1/gamification/me/inventory/equip` với body `EquipRequest { ItemId, IsEquipped }`. Backend `EquipItemAsync` cập nhật trạng thái trong `UserInventory`, FE refetch inventory sau khi thành công.
- **⑤ Code thật (`backend/src/DsaVisual.Api/Controllers/GamificationController.cs`):**
```csharp
// backend/src/DsaVisual.Api/Controllers/GamificationController.cs:164-167
[HttpPut("me/inventory/equip")]
public async Task<ActionResult> Equip([FromBody] EquipRequest request, CancellationToken ct)
{
    var result = await _service.EquipItemAsync(CurrentUserId(), request, ct);
```
```

---

#### **Câu 11.4 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `PUT /api/v1/auth/me` với body `{ displayName }` → `AuthController.UpdateMe` gọi `UpdateProfileAsync` cập nhật trường `DisplayName` (không phải FullName) trong DB → `authStore.user.displayName` phản ánh tức thì trên Topbar.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì `PUT /api/v1/auth/me` với body `{ displayName }` → `AuthController.UpdateMe` gọi `UpdateProfileAsync` cập nhật trường `DisplayName` (không phải FullName) trong DB → `authStore.user.displayName` phản ánh tức thì trên Topbar.
- **⑤ Code thật (`backend/src/DsaVisual.Api/Controllers/AuthController.cs`):**
```csharp
// backend/src/DsaVisual.Api/Controllers/AuthController.cs
[HttpPut("me")]
[Authorize]
public async Task<ActionResult<UserSummary>> UpdateMe([FromBody] UpdateProfileRequest request, CancellationToken ct)
{
    var result = await _service.UpdateProfileAsync(CurrentUserId(), request, ct);
    return MapResultExtensions.MapResult(this, result);
}
```

---

#### **Câu 11.5 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Admin phê duyệt giảng viên qua `POST /api/v1/users/{id}/approve-teacher` (UsersController có `[Route("api/v1/users")]` + `[Authorize(Roles = "ADMIN")]`) với body `ApproveTeacherRequest`. Hàm `ApproveTeacherAsync(actorId, actorIsPrimaryAdmin, id, request)` kiểm tra PrimaryAdmin trước khi chuyển role sang TEACHER. Không có endpoint self-apply `/me/teacher-application`. A sai vì không có endpoint nộp đơn; C sai vì role không tạo trực tiếp mà qua duyệt; D sai vì không có email SMTP flow.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì Admin phê duyệt giảng viên qua `POST /api/v1/users/{id}/approve-teacher` (UsersController có `[Route("api/v1/users")]` + `[Authorize(Roles = "ADMIN")]`) với body `ApproveTeacherRequest`. Hàm `ApproveTeacherAsync(actorId, actorIsPrimaryAdmin, id, request)` kiểm tra PrimaryAdmin trước khi chuyển role sang TEACHER. Không có endpoint self-apply `/me/teacher-application`. A sai vì không có endpoint nộp đơn; C sai vì role không tạo trực tiếp mà qua duyệt; D sai vì không có email SMTP flow.
- **⑤ Code thật (`backend/src/DsaVisual.Api/Controllers/UsersController.cs`):**
```csharp
// backend/src/DsaVisual.Api/Controllers/UsersController.cs:79-84
[HttpPost("{id:int}/approve-teacher")]
public async Task<ActionResult> ApproveTeacher(
    [FromRoute] int id, [FromBody] ApproveTeacherRequest request, CancellationToken ct)
{
    var result = await _service.ApproveTeacherAsync(CurrentUserId(), ActorIsPrimaryAdmin(), id, request, ct);
    return MapResultExtensions.MapResult(this, result);
}
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 11.1: Trace luồng Đổi Mật khẩu trong Tab Cài đặt Profile (2.5 điểm)
- **1. Form Validation (0.5đ):** Kiểm tra mật khẩu cũ, mật khẩu mới $\ge 8$ ký tự, `confirmPassword === newPassword`.
- **2. Request (0.75đ):** `PUT /api/v1/auth/change-password` với `{ currentPassword, newPassword }`.
- **3. Backend BCrypt (0.75đ):**
```csharp
// backend/src/DsaVisual.Application/Services/AuthService.cs
if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
    return Result.Fail("Mật khẩu hiện tại không đúng");
user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
await _db.SaveChangesAsync(ct);
```
- **4. Phản hồi UI (0.5đ):** Hiện Toast "Đổi mật khẩu thành công!" và xóa trắng form.

---

#### 📝 Câu TL 11.2: Trace luồng Nộp đơn Giáo viên $\rightarrow$ Phê duyệt $\rightarrow$ Mở khóa Studio (2.5 điểm)
- **1. Nộp đơn (0.5đ):** `POST /api/v1/me/teacher-application` lưu row vào bảng `TeacherApplications` với `Status = 'PENDING'`.
- **2. Banner Trạng thái (0.5đ):** Profile hiện banner vàng: "⏳ Đang chờ duyệt".
- **3. Phê duyệt (0.75đ):** Admin vào `/admin/users` bấm "Duyệt" $\rightarrow$ `Users.Role = 'TEACHER'`.
- **4. Mở khóa Quyền (0.75đ):** Giảng viên đăng nhập lại $\rightarrow$ Sidebar mở mục "Curriculum Studio" (`/studio`) và mở quyền tạo lớp.

---

# 📘 ĐÁP ÁN ĐỀ 12: HỆ THỐNG LỚP HỌC & STUDIO GIẢNG VIÊN

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 12.1 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Nút "Tạo lớp mới" chỉ hiển thị khi người dùng có vai trò `TEACHER` hoặc `ADMIN`.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì Nút "Tạo lớp mới" chỉ hiển thị khi người dùng có vai trò `TEACHER` hoặc `ADMIN`.
- **⑤ Code thật (`frontend/src/views/ClassesView.vue`):**
```html
<!-- frontend/src/views/ClassesView.vue -->
<Button v-if="auth.role === 'TEACHER' || auth.role === 'ADMIN'" @click="showCreateModal = true">
  Tạo lớp mới
</Button>
```

---

#### **Câu 12.2 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `POST /api/v1/classes` tạo lớp học và tự động sinh Join Code 6 ký tự (VD: `DSA999`).
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì `POST /api/v1/classes` tạo lớp học và tự động sinh Join Code 6 ký tự (VD: `DSA999`).
- **⑤ Code thật (`backend/src/DsaVisual.Application/Services/ClassService.cs`):**
```csharp
// backend/src/DsaVisual.Application/Services/ClassService.cs
var joinCode = await GenerateUniqueJoinCodeAsync(ct); // Sinh mã 6 ký tự A-Z0-9
var newClass = new Class { Name = request.Name, JoinCode = joinCode, TeacherId = teacherId };
_db.Classes.Add(newClass);
await _db.SaveChangesAsync(ct);
```

---

#### **Câu 12.3 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Sinh viên nhập mã 6 ký tự gọi `POST /api/v1/classes/join-by-code` thêm bản ghi vào `ClassMembers`.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì Sinh viên nhập mã 6 ký tự gọi `POST /api/v1/classes/join-by-code` thêm bản ghi vào `ClassMembers`.
- **⑤ Code thật (`backend/src/DsaVisual.Api/Controllers/ClassesController.cs`):**
```csharp
// backend/src/DsaVisual.Api/Controllers/ClassesController.cs
[HttpPost("join-by-code")]
public async Task<ActionResult<ClassDetailDto>> JoinByCode([FromBody] JoinClassByCodeRequest request, CancellationToken ct)
{
    var result = await _service.JoinByCodeAsync(CurrentUserId(), request, ct);
    return MapResultExtensions.MapResult(this, result);
}
```

---

#### **Câu 12.4 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Xuất báo cáo dạng CSV (không phải Excel .xlsx). Gọi `classesApi.exportClassReportCsv(classId)` → prepend BOM `\uFEFF` cho UTF-8 Excel compatible → tạo Blob `text/csv;charset=utf-8` → trigger download `class-report-{id}.csv`.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì Xuất báo cáo dạng CSV (không phải Excel .xlsx). Gọi `classesApi.exportClassReportCsv(classId)` → prepend BOM `\uFEFF` cho UTF-8 Excel compatible → tạo Blob `text/csv;charset=utf-8` → trigger download `class-report-{id}.csv`.
- **⑤ Code thật (`frontend/src/views/ClassReportView.vue`):**
```typescript
// frontend/src/views/ClassReportView.vue:86-91
const csv = await classesApi.exportClassReportCsv(classId.value);
const blob = new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8' });
const url = URL.createObjectURL(blob);
link.download = `class-report-${classId.value}.csv`;
```

---

#### **Câu 12.5 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `AdminLessonEditorView.vue` hỗ trợ Markdown Editor 2 cột + Live Preview KaTeX/Code highlight và gắn Visualizer.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì `AdminLessonEditorView.vue` hỗ trợ Markdown Editor 2 cột + Live Preview KaTeX/Code highlight và gắn Visualizer.
- **⑤ Code thật (`frontend/src/views/admin/AdminLessonEditorView.vue`):**
```html
<!-- frontend/src/views/admin/AdminLessonEditorView.vue -->
<div class="editor__split">
  <textarea v-model="form.contentMarkdown" class="editor__textarea" />
  <div class="editor__preview"><MarkdownRenderer :content="form.contentMarkdown" /></div>
</div>
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 12.1: Trace luồng Giảng viên Quản lý Thành viên Lớp học (2.5 điểm)
- **1. Load Thành viên (0.5đ):** `ClassDetailView.vue` gọi `GET /api/v1/classes/{id}`.
- **2. Modal Xác nhận Xóa (0.5đ):** Click icon thùng rác $\rightarrow$ Modal hiện: "Bạn có chắc muốn xóa học viên khỏi lớp?".
- **3. Request Xóa (0.75đ):** `DELETE /api/v1/classes/{id}/members/{userId}` $\rightarrow$ Backend xóa bản ghi trong `ClassMembers`.
- **4. UI Render (0.75đ):** Mảng `members.value` lọc bỏ user đó và hiện Toast "Đã xóa học viên khỏi lớp".

---

#### 📝 Câu TL 12.2: Trace luồng Giảng viên Soạn & Xuất bản Bài học Mới (2.5 điểm)
- **1. Điền Form (0.5đ):** Nhập tiêu đề, chọn topic cha, soạn markdown lý thuyết, chọn key `sort.quick`.
- **2. Request (0.75đ):** `POST /api/v1/lessons` với `{ topicId, title, contentMarkdown, simulationKey }`.
- **3. Backend Lưu (0.75đ):** `LessonService.CreateAsync` kiểm tra quyền `TEACHER` và lưu row vào `Lessons`.
- **4. Phản hồi (0.5đ):** Chuyển hướng về `/studio`, bài học xuất hiện ngay trên cây lộ trình `/path/:id`.

---

# 📘 ĐÁP ÁN ĐỀ 13: BẢNG ĐIỀU KHIỂN QUẢN TRỊ HỆ THỐNG

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 13.1 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Reactive refs `search`, `roleFilter`, `statusFilter` truyền vào `adminApi.fetchUsers({ role, q, page })`. Hàm `load()` gọi API server-side mỗi lần filter thay đổi; KHÔNG dùng RxJS debounce. Tab `'all' | 'pending'` tách riêng danh sách thường và đơn chờ duyệt.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì Reactive refs `search`, `roleFilter`, `statusFilter` truyền vào `adminApi.fetchUsers({ role, q, page })`. Hàm `load()` gọi API server-side mỗi lần filter thay đổi; KHÔNG dùng RxJS debounce. Tab `'all' | 'pending'` tách riêng danh sách thường và đơn chờ duyệt.
- **⑤ Code thật (`frontend/src/views/AdminUsersView.vue`):**
```typescript
// frontend/src/views/AdminUsersView.vue:49-51, 268-272
const search = ref('');
const roleFilter = ref('');
const statusFilter = ref('');
// ...
async function load(): Promise<void> {
  const page = await adminApi.fetchUsers({ role: tab.value === 'pending' ? 'TEACHER_PENDING' : undefined, q: search.value || undefined, page: 1 });
```
```

---

#### **Câu 13.2 — Đáp án: A**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Admin không được tự khóa tài khoản của chính mình và không được hạ quyền Admin khác.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **A** đúng vì Admin không được tự khóa tài khoản của chính mình và không được hạ quyền Admin khác.
- **⑤ Code thật (`backend/src/DsaVisual.Application/Services/UserService.cs`):**
```csharp
// backend/src/DsaVisual.Application/Services/UserService.cs
if (targetUser.Id == currentUserId)
    return Result.Fail("Không thể tự khóa tài khoản của chính mình");
```

---

#### **Câu 13.3 — Đáp án: D**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: Khóa tài khoản dùng `PUT /api/v1/admin/users/{id}/status` với body `{ isActive: false }` (KHÔNG phải `isBlocked`). Backend `SetStatusAsync` set `user.IsActive = false`; nếu target là Admin thì kiểm tra `HasOtherActiveAdminAsync` trước. KHÔNG tự động revoke refresh token (revoke chỉ xảy ra trong ChangePassword).
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **D** đúng vì Khóa tài khoản dùng `PUT /api/v1/admin/users/{id}/status` với body `{ isActive: false }` (KHÔNG phải `isBlocked`). Backend `SetStatusAsync` set `user.IsActive = false`; nếu target là Admin thì kiểm tra `HasOtherActiveAdminAsync` trước. KHÔNG tự động revoke refresh token (revoke chỉ xảy ra trong ChangePassword).
- **⑤ Code thật (`backend/src/DsaVisual.Api/Controllers/UsersController.cs`):**
```csharp
// backend/src/DsaVisual.Api/Controllers/UsersController.cs:64-70
[HttpPut("{id:int}/status")]
public async Task<ActionResult> SetStatus(
    [FromRoute] int id, [FromBody] SetStatusRequest request, CancellationToken ct)
{
    var result = await _service.SetStatusAsync(CurrentUserId(), ActorIsPrimaryAdmin(), id, request.IsActive, ct);
    return MapResultExtensions.MapResult(this, result);
}
// SetStatusRequest { public bool IsActive { get; set; } }
```

---

#### **Câu 13.4 — Đáp án: B**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `GET /api/v1/admin/stats` trả 13 scalar counts trong 1 SQL query (perf#9: gộp 13 CountAsync thành 1 round-trip). Gồm: TotalUsers, TotalStudents, TotalTeachers, TotalAdmins, TotalTopics, TotalLessons, TotalExercises, TotalSubmissions, TotalCodeSubmissions, TotalClasses, TotalFavorites, TotalSimulations, ActiveUsersToday. KHÔNG có trend/revenue/completion rate.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **B** đúng vì `GET /api/v1/admin/stats` trả 13 scalar counts trong 1 SQL query (perf#9: gộp 13 CountAsync thành 1 round-trip). Gồm: TotalUsers, TotalStudents, TotalTeachers, TotalAdmins, TotalTopics, TotalLessons, TotalExercises, TotalSubmissions, TotalCodeSubmissions, TotalClasses, TotalFavorites, TotalSimulations, ActiveUsersToday. KHÔNG có trend/revenue/completion rate.
- **⑤ Code thật (`backend/src/DsaVisual.Api/Controllers/AdminController.cs`):**
```csharp
// backend/src/DsaVisual.Api/Controllers/AdminController.cs:33-48
var stats = await _db.Database.SqlQuery<StatsDto>($"""
    SELECT
        (SELECT COUNT(*) FROM Users WHERE DeletedAt IS NULL) AS TotalUsers,
        (SELECT COUNT(*) FROM Users WHERE Role = 0 AND DeletedAt IS NULL) AS TotalStudents,
        (SELECT COUNT(*) FROM Users WHERE Role = 1 AND DeletedAt IS NULL) AS TotalTeachers,
        (SELECT COUNT(*) FROM Users WHERE Role = 3 AND DeletedAt IS NULL) AS TotalAdmins,
        (SELECT COUNT(*) FROM Topics WHERE DeletedAt IS NULL) AS TotalTopics,
        (SELECT COUNT(*) FROM Lessons WHERE DeletedAt IS NULL) AS TotalLessons,
        (SELECT COUNT(*) FROM Exercises WHERE DeletedAt IS NULL) AS TotalExercises,
        (SELECT COUNT(*) FROM ExerciseSubmissions) AS TotalSubmissions,
        (SELECT COUNT(*) FROM CodeSubmissions) AS TotalCodeSubmissions,
        (SELECT COUNT(*) FROM Classes WHERE DeletedAt IS NULL) AS TotalClasses,
        (SELECT COUNT(*) FROM Favorites) AS TotalFavorites,
        (SELECT COUNT(DISTINCT SimulationKey) FROM LessonSimulations) AS TotalSimulations,
        (SELECT COUNT(*) FROM Users WHERE DeletedAt IS NULL AND LastActivityDate >= {today}) AS ActiveUsersToday
    """).FirstAsync(ct);
```

---

#### **Câu 13.5 — Đáp án: C**
- **① Khái niệm:** Câu hỏi kiểm tra khái niệm nền tảng sau: `PUT /api/v1/settings` (route `/settings`, KHÔNG phải `/admin/settings`) nhận `SystemSettingsDto`: SiteName, AllowedDomains, PasswordPolicy.MinLength, UploadMaxMb (1-100), SandboxSeconds (1-120), SandboxMemoryMb. Validate bởi `SystemSettingsValidator`. Policy uppercase/digit/special cố định; sandbox chạy client-side nên backend không lưu.
- **③ Áp dụng sâu:** Khi trace tính năng, đi từ UI/component → store hoặc API client → endpoint/backend → state/response render. Kiểm tra cả điều kiện thành công, nhánh lỗi và contract dữ liệu; không suy luận chỉ từ tên file.
- **④ Tại sao A/B/C/D sai:** Các phương án còn lại không khớp với contract và hành vi được mô tả trong mã nguồn.
- **② Vì sao:** Đáp án **C** đúng vì `PUT /api/v1/settings` (route `/settings`, KHÔNG phải `/admin/settings`) nhận `SystemSettingsDto`: SiteName, AllowedDomains, PasswordPolicy.MinLength, UploadMaxMb (1-100), SandboxSeconds (1-120), SandboxMemoryMb. Validate bởi `SystemSettingsValidator`. Policy uppercase/digit/special cố định; sandbox chạy client-side nên backend không lưu.
- **⑤ Code thật (`backend/src/DsaVisual.Application/Dtos/SettingDto.cs`):**
```csharp
// backend/src/DsaVisual.Application/Dtos/SettingDto.cs:10-18
public sealed class SystemSettingsDto
{
    public string SiteName { get; set; } = "DSA Visual";
    public List<string> AllowedDomains { get; set; } = [];
    public PasswordPolicySettingsDto PasswordPolicy { get; set; } = new();
    public int UploadMaxMb { get; set; } = 5;
    public int SandboxSeconds { get; set; } = 10;
    public int SandboxMemoryMb { get; set; } = 64;
}
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 13.1: Trace luồng Phê duyệt / Từ chối Đơn Giảng viên (2.5 điểm)
- **1. Danh sách Đơn (0.5đ):** Tab "Duyệt Giáo viên" nạp từ `GET /api/v1/admin/teacher-applications`.
- **2. Modal Từ chối (0.5đ):** Bấm "❌ Từ chối" $\rightarrow$ Modal yêu cầu nhập lý do từ chối.
- **3. Request (0.75đ):** `POST /api/v1/admin/teacher-applications/{id}/review` với `{ approve: false, rejectReason: "..." }`.
- **4. Backend Update (0.75đ):** Đổi `Status = 'REJECTED'`, ghi nhận lý do và gửi thông báo cho ứng viên.

---

#### 📝 Câu TL 13.2: Trace luồng Thay đổi Cấu hình Sandbox và Đồng bộ Runtime (2.5 điểm)
- **1. Các trường thật (0.5đ):** `SystemSettingsDto` gồm `SiteName`, `AllowedDomains`, `PasswordPolicy { MinLength, RequireUppercase, RequireDigit, RequireSpecial }`, `UploadMaxMb`, `SandboxSeconds`, `SandboxMemoryMb` (SettingDto.cs:10-18) — KHÔNG có cấu hình hồi tim (chu kỳ hồi tim 30 phút Free / 10 phút Premium là hằng số nghiệp vụ trong `GamificationService`, không phải setting).
- **2. Request (0.75đ):** `PUT /api/v1/settings` (SettingsController — route `/settings`, KHÔNG phải `/admin/settings`), `[Authorize(Roles = "ADMIN")]`, body là cả `SystemSettingsDto` mới, đi qua `SystemSettingsDtoValidator`.
- **3. Cache Invalidation (0.75đ):** `SettingService` lưu xuống DB rồi `SettingsCache` singleton invalidate + nạp lại theo cơ chế double-checked lock: nhiều caller đồng thời khi cache trống thì chỉ 1 caller thật sự query DB (finding biz#17a).
- **4. Áp dụng tức thì (0.5đ):** Mọi service đọc settings đều đi qua `SettingsCache` singleton trong bộ nhớ — request kế tiếp thấy giá trị mới ngay, không cần restart (lưu ý: cache in-process per-instance — multi-instance cần cân nhắc, đã ghi chú trong SettingsCache.cs:9). **Nuance quan trọng (cross-ref Đề 08):** `CodelabJudgeService` KHÔNG đọc `SandboxSeconds` — engine Jint server dùng hằng số riêng `DefaultTimeoutMs = 1500` + `MaxMemoryBytes = 32MB` (CodelabJudgeService.cs:33-35, được gọi tại ExerciseService.cs:823 không truyền timeoutMs); `SandboxSeconds`/`SandboxMemoryMb` theo ADR-012 dành cho sandbox CLIENT-side (SettingService.cs:46).

---

# 🏆 ĐÁP ÁN ĐỀ FINAL: BÀI THI ĐÁNH GIÁ NĂNG LỰC TOÀN HỆ THỐNG

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu – Tổng: 10.0 điểm)

#### **Câu F.1 — Đáp án: A**
- **Giải thích:** Lỗi 401 tự động kích hoạt refresh token singleton qua Cookie HttpOnly và retry request gốc liền mạch không gián đoạn trải nghiệm người dùng.
- **Mã nguồn thực tế (`frontend/src/api/client.ts`):**
```typescript
// frontend/src/api/client.ts
if (status === 401 && original && !original._retry) {
  original._retry = true;
  const newToken = await auth.refresh();
  if (newToken) {
    original.headers.Authorization = `Bearer ${newToken}`;
    return client(original);
  }
}
```

---

#### **Câu F.2 — Đáp án: D**
- **Giải thích:** Khách chưa đăng nhập chỉ được xem Trang chủ, 3 demo công khai (`sort.bubble`, `search.binary`, `graph.bfs` — 3 key duy nhất có `demoAllowed: true`), lộ trình tổng quan (route public) và trợ giúp. Đáp án A sai vì Quiz/nộp code yêu cầu đăng nhập; C sai vì leaderboard cần auth; B sai vì `/lessons/:id` có `requiresAuth`.
- **Mã nguồn thực tế (`frontend/src/engines/catalog.ts` & `frontend/src/router/index.ts`):**
```typescript
// frontend/src/engines/catalog.ts — đúng 3 key demo công khai
{ key: 'sort.bubble', /* ... */ demoAllowed: true },
{ key: 'search.binary', /* ... */ demoAllowed: true },
{ key: 'graph.bfs', /* ... */ demoAllowed: true },

// frontend/src/router/index.ts
{ path: '/path', component: CoursesListView, meta: { public: true } },
{ path: '/lessons/:id', component: LessonStudyView, meta: { requiresAuth: true } },
```

---

#### **Câu F.3 — Đáp án: B**
- **Giải thích:** Practice Ladder chuẩn 3 bậc tuần tự: Bậc 1 Quiz $\rightarrow$ Bậc 2 Interactive Lab $\rightarrow$ Bậc 3 Code Challenge; bậc sau `locked` cho tới khi bậc trước `passed` (LadderShell). Đáp án D sai vì các bậc có ràng buộc mở khóa; C sai vì thứ tự cố định; A sai vì cả 3 bậc đều được chấm.
- **Mã nguồn thực tế (`frontend/src/components/ladder/LadderShell.vue`):**
```typescript
// frontend/src/components/ladder/LadderShell.vue
const STAGES = [
  { key: 'quiz', label: 'Quiz' },
  { key: 'lab', label: 'Lab' },
  { key: 'code', label: 'Code' },
];
// stage sau bị locked cho tới khi stage trước nằm trong passedStages
if (passedStages.value.has(stage.key)) return { ...stage, status: 'passed' };
if (idx === 0 || (prev && passedStages.value.has(prev.key))) return { ...stage, status: 'active' };
return { ...stage, status: 'locked' };
```

---

#### **Câu F.4 — Đáp án: A**
- **Giải thích:** "Chạy thử" chạy ở Web Sandbox Client sinh `TraceEvent[]` phát lại canvas; "Nộp bài" chấm trên Server Jint (timeout 1500ms, memory 32MB) với test cases ẩn độc lập nhằm chống gian lận. B/C/D sai vì cả 2 nút không cùng chạy phía server, không trả GIF, và "Nộp bài" có chấm điểm thật.
- **Mã nguồn thực tế (`frontend/src/stores/codeRunner.ts` & `backend/src/DsaVisual.Application/Services/CodelabJudgeService.cs`):**
```typescript
// frontend/src/stores/codeRunner.ts — sandbox client (giới hạn 50.000 event)
runCode({ code: editorCode.value, entry: 'solve', bindings: [] }, defaultArray),
```
```csharp
// backend/src/DsaVisual.Application/Services/CodelabJudgeService.cs — Jint server sandbox
public const int DefaultTimeoutMs = 1500;
private const long MaxMemoryBytes = 32 * 1024 * 1024; // 32MB
var engine = new Engine(options => {
    options.TimeoutInterval(TimeSpan.FromMilliseconds(timeoutMs));
    // ...
});
```

---

#### **Câu F.5 — Đáp án: C**
- **Giải thích:** Tim là tài nguyên vào phiên luyện node (Free tối đa 10, Premium tối đa 30; trừ 1 tim mỗi lần enter node bằng UPDATE atomic — node đã PASS vào lại miễn phí; tự hồi 30 phút/tim Free, 10 phút/tim Premium); Gems là tiền tệ thưởng khi claim quest, dùng mua vật phẩm cosmetic (avatar/khung) trong Shop. A/B/D sai vì không có cơ chế trừ Gems khi làm sai, tim có tự hồi, và tim/gems lưu phía server chứ không phải LocalStorage.
- **Mã nguồn thực tế (`backend/src/DsaVisual.Application/Services/GamificationService.cs`):**
```csharp
// backend/src/DsaVisual.Application/Services/GamificationService.cs
// (a) Phí vào node — UPDATE atomic chống double-spend (FR-10.1)
$"UPDATE Users SET Hearts = Hearts - 1 WHERE Id = {userId} AND Hearts > 0", ct);
// (b) Hồi tim: Free 30 phút/tim (max 10), Premium 10 phút/tim (max 30) — FR-10.1
// (c) Claim quest: cộng gems/xp atomic
$"UPDATE Users SET Gems = Gems + {reward.Gems}, Xp = Xp + {reward.Xp} WHERE Id = {userId}", ct);
```

---

#### **Câu F.6 — Đáp án: D**
- **Giải thích:** 4 Sandbox gom chung vào `SortingView.vue` với `:key="$route.fullPath"` tái sử dụng mã nguồn.
- **Mã nguồn thực tế (`frontend/src/router/index.ts`):**
```typescript
// frontend/src/router/index.ts
{ path: '/sorting-sandbox', component: SortingSandboxView },
{ path: '/searching-sandbox', component: SortingSandboxView },
{ path: '/graph-playground', component: SortingSandboxView },
{ path: '/stack-queue-sandbox', component: SortingSandboxView }
```

---

#### **Câu F.7 — Đáp án: B**
- **Giải thích:** Mã mời lớp học 6 ký tự ngẫu nhiên duy nhất sinh từ server giúp sinh viên tham gia lớp nhanh chóng.
- **Mã nguồn thực tế (`backend/src/DsaVisual.Application/Services/ClassService.cs`):**
```csharp
// backend/src/DsaVisual.Application/Services/ClassService.cs
var joinCode = await GenerateUniqueJoinCodeAsync(ct); // VD: 'DSA999'
```

---

#### **Câu F.8 — Đáp án: A**
- **Giải thích:** Route `/studio` bắt buộc có vai trò `TEACHER` hoặc `ADMIN`.
- **Mã nguồn thực tế (`frontend/src/router/index.ts`):**
```typescript
// frontend/src/router/index.ts
{
  path: '/studio',
  component: AdminContentView,
  meta: { requiresAuth: true, roles: ['TEACHER', 'ADMIN'] },
}
```

---

#### **Câu F.9 — Đáp án: C**
- **Giải thích:** Lỗi 429 đọc header `Retry-After` thành `apiError.retryAfterSeconds` và hiển thị toast cảnh báo "thao tác quá nhanh, thử lại sau N giây" (nếu không có Retry-After thì dùng bản tin chung). A/B/D sai vì 429 không liên quan token/refresh, không khóa tài khoản, và có toast hiển thị rõ ràng.
- **Mã nguồn thực tế (`frontend/src/api/client.ts`):**
```typescript
// frontend/src/api/client.ts
if (status === 429) {
  const seconds = apiError.retryAfterSeconds;
  ui.showToast(
    seconds !== undefined
      ? messages.toast.rateLimited(seconds)
      : messages.toast.rateLimitedUnknown,
    'warning',
  );
}
```

---

#### **Câu F.10 — Đáp án: D**
- **Giải thích:** Nâng cấp Premium ghi `PremiumUntil` vào bảng `Users`, HeartsMax nâng lên 30 và hồi 10 phút/tim; khi gói hết hạn, cơ chế lazy downgrade (`EnsureHeartsMaxSyncAsync`) clamp hearts/HeartsMax về ngưỡng Free ngay ở lần đọc kế tiếp. Lưu ý: Premium KHÔNG miễn trừ phí vào node — vẫn bị trừ tim khi enter node mới. A/B/C sai vì không có cờ `IsPremium` vĩnh viễn, trạng thái lưu server chứ không LocalStorage, và không có bảng PremiumUsers riêng.
- **Mã nguồn thực tế (`backend/src/DsaVisual.Application/Services/GamificationService.cs`):**
```csharp
// backend/src/DsaVisual.Application/Services/GamificationService.cs
user.HeartsMax = 30;                     // Premium 30 tim (FR-10.7)
// ...
// BUG-1 (lazy downgrade): premium hết hạn → clamp HeartsMax/Hearts về Free NGAY khi đọc
await EnsureHeartsMaxSyncAsync(user, ct);
```

---

### 📋 PHẦN II: TỰ LUẬN TÍCH HỢP TOÀN HỆ THỐNG (10.0 điểm)

#### 📝 Câu TL F.1: Hành trình Trọn vẹn của Học viên từ Khách vãng lai đến Hoàn thành Lộ trình (3.5 điểm)
- **1. Trải nghiệm Demo (0.75đ):** Khách vào `/` $\rightarrow$ click card Bubble Sort $\rightarrow$ `/simulator/sort.bubble` kiểm tra `isDemoKey: true` (1 trong 3 key `demoAllowed`: `sort.bubble`, `search.binary`, `graph.bfs`) cho phép chạy Canvas Engine trực tiếp không cần auth.
- **2. Đăng ký & Khởi tạo Tài khoản (1.0đ):** Bấm "Đăng ký" $\rightarrow$ `RegisterView.vue` validate form $\rightarrow$ gọi `POST /api/v1/auth/register` $\rightarrow$ Backend tạo `User { Hearts: 10, HeartsMax: 10, Gems: 0, Xp: 0 }` (Level không lưu DB — tính từ XP), trả về AccessToken (lưu RAM Pinia) và Cookie RefreshToken HttpOnly.
- **3. Ghi danh Lộ trình (0.75đ):** Vào `/path` (route public) $\rightarrow$ danh mục khóa học từ `GET /concepts/courses` qua `useCourseStore` $\rightarrow$ bấm "Tham gia lộ trình" $\rightarrow$ ghi client-side localStorage (`enrolled_{courseId}`) $\rightarrow$ vào chi tiết bài học (route `requiresAuth`).
- **4. Học & Đánh dấu đã học (1.0đ):** Vào `/lessons/10` $\rightarrow$ đọc lý thuyết Markdown + làm quiz $\rightarrow$ bấm đánh dấu đã học $\rightarrow$ `POST /api/v1/lessons/10/mark-viewed` upsert `UserProgress.Viewed = true` và tăng quest `lesson_viewed` (QuestProgressWriter) $\rightarrow$ sang `/quests` claim quest nhận Gems/XP (UPDATE atomic) $\rightarrow$ Topbar/Profile cập nhật Level = `1 + floor(sqrt(xp/100))`.

---

#### 📝 Câu TL F.2: Hành trình Giảng viên: Tạo Lớp $\rightarrow$ Giao Bài $\rightarrow$ Sinh viên Học $\rightarrow$ Báo cáo (3.5 điểm)
- **1. Giảng viên Tạo lớp (1.0đ):** Giảng viên vào `/classes` bấm "Tạo lớp mới: DSA K18" $\rightarrow$ `POST /api/v1/classes` $\rightarrow$ Backend tạo row trong `Classes` và sinh mã mời 6 ký tự duy nhất `DSA118`.
- **2. Sinh viên Tham gia Lớp (0.75đ):** Sinh viên vào `/classes` bấm "Tham gia lớp", nhập mã `DSA118` $\rightarrow$ `POST /api/v1/classes/join-by-code` thêm sinh viên vào `ClassMembers`.
- **3. Học bài & Nộp Code (1.0đ):** Sinh viên vào node luyện tập $\rightarrow$ bắt đầu phiên bị trừ 1 tim (UPDATE atomic `Hearts - 1 WHERE Hearts > 0`; node đã PASS vào lại miễn phí) $\rightarrow$ làm Bậc 1 Quiz rồi Bậc 3 Code Runner, bấm "Nộp bài" $\rightarrow$ Server Jint (timeout 1500ms, 32MB) chấm Pass toàn bộ Test Cases ẩn. Hết tim: chờ hồi (Free 30 phút/tim) hoặc nâng Premium (max 30 tim, hồi 10 phút/tim) — Shop không bán item hồi tim (chỉ avatar/khung cosmetic).
- **4. Báo cáo & Xuất File (0.75đ):** Giảng viên vào `/classes/10/report` xem biểu đồ tỷ lệ hoàn thành $\rightarrow$ bấm xuất file $\rightarrow$ `ClassReportView.vue` tổng hợp CSV client-side (`new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8' })` + `URL.createObjectURL` + `a.download`) tải bảng điểm về máy.

---

#### 📝 Câu TL F.3: Hành trình Quản trị & Vận hành: Duyệt Giáo viên $\rightarrow$ Thống kê $\rightarrow$ Cài đặt Runtime (3.0 điểm)
- **1. Duyệt quyền (1.0đ):** Ứng viên role `TEACHER_PENDING` $\rightarrow$ Admin vào `/admin/users` tab "Chờ duyệt GV" (lọc `role=TEACHER_PENDING`) $\rightarrow$ xem hồ sơ, bấm "✅ Phê duyệt" $\rightarrow$ `POST /api/v1/users/{id}/approve-teacher` $\rightarrow$ Backend cập nhật `Users.Role = 'TEACHER'`.
- **2. Giám sát Hệ thống (1.0đ):** Admin vào `/admin/stats` $\rightarrow$ `GET /api/v1/admin/stats` trả về các chỉ số đếm: TotalUsers/TotalStudents/TotalTeachers/TotalAdmins, TotalTopics/TotalLessons/TotalExercises, TotalSubmissions/TotalCodeSubmissions, TotalClasses/TotalFavorites/TotalSimulations và ActiveUsersToday.
- **3. Cấu hình Runtime không cần Restart (1.0đ):** Admin vào `/admin/settings` chỉnh cấu hình Sandbox (`SandboxSeconds`, `SandboxMemoryMb`), PasswordPolicy, UploadMaxMb... $\rightarrow$ `PUT /api/v1/settings` $\rightarrow$ `SettingsCache` singleton invalidate + nạp lại từ DB (double-checked lock) $\rightarrow$ toàn hệ thống áp dụng ngay không cần restart.



---

# 📘 ĐÁP ÁN ĐỀ 14: TRỢ GIÚP, CHÍNH SÁCH BẢO MẬT & PHẢN HỒI NGƯỜI DÙNG

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 14.1 — Đáp án: B**
- **① Khái niệm:** ARIA Accordion Pattern là bộ thuộc tính accessibility (`aria-expanded`, `aria-controls`, `id`) giúp screen reader hiểu mối quan hệ giữa nút trigger và nội dung ẩn/hiện trong accordion.
- **② Vì sao lại như thế:** Không có ARIA: screen reader chỉ đọc nút bấm nhưng không biết nội dung nào đang mở, không liên kết được câu hỏi với câu trả lời. `aria-expanded="true/false"` báo trạng thái mở/đóng; `aria-controls="faq-answer-3"` trỏ đến `id="faq-answer-3"` của phần trả lời → screen reader đọc liền mạch "Câu hỏi → Trả lời" khi user expand. Đây là yêu cầu WCAG 2.1 Level AA cho accordion components.
- **③ Áp dụng sâu:** Trong HelpView, `openIndex` ref quản lý index đang mở (null = đóng hết). Toggle: `openIndex = openIndex === idx ? null : idx` → Vue reactivity cập nhật `aria-expanded` binding tự động. Transition `name="faq"` animate enter/exit mượt mà. Cross-ref Đề 17: `useKeyboardShortcuts` cũng check `isEditableTarget` trước khi xử lý phím tắt trên accordion.
- **❌ Tại sao C/D/A sai:** C (CSS display:none only) sai vì screen reader vẫn đọc nội dung ẩn hoặc bỏ qua hoàn toàn; D (role="tab") sai vì tab pattern khác accordion (tab dùng role="tabpanel" + aria-selected); A (không cần ARIA) sai vì vi phạm WCAG, user khiếm thị không dùng được.
- **Mã nguồn thực tế (`frontend/src/views/HelpView.vue`):**
```html
<Button
  :aria-expanded="openIndex === idx"
  :aria-controls="`faq-answer-${idx}`"
  @click="toggle(idx)"
>
<p v-if="openIndex === idx" :id="`faq-answer-${idx}`" class="help__answer">{{ faq.a }}</p>
```

---

#### **Câu 14.2 — Đáp án: A**
- **① Khái niệm:** Client-side Form Validation là kiểm tra dữ liệu nhập ngay trên trình duyệt trước khi gửi API, gồm regex email, độ dài tối thiểu, và required fields — giảm round-trip server và cải thiện UX phản hồi tức thì.
- **② Vì sao lại như thế:** Nếu chỉ validate server-side: user điền form → bấm gửi → chờ 500ms network → nhận lỗi → sửa → gửi lại. Client-side validation chặn NGAY khi bấm submit: tên < 2 ký tự, email không khớp regex, message < 10 ký tự → hiện lỗi đỏ dưới form trong <1ms. Server vẫn validate lại (defense in depth) nhưng client-side giảm 90% request lỗi không cần thiết.
- **③ Áp dụng sâu:** Hàm `submitContact()` chạy tuần tự: reset error → check name+email → check message length → nếu pass: set `contactSent = true` (UI thay form bằng thông báo cảm ơn). Regex email đơn giản nhưng đủ cho 99% trường hợp; backend validate chặt hơn. **Lỗi thường gặp:** Quên trim() trước khi check length → "   " (3 spaces) pass length check nhưng rỗng thực tế.
- **❌ Tại sao D/B/C sai:** D (chỉ server validate) sai vì UX chậm; B (HTML5 required only) sai vì không custom được message tiếng Việt; C (không validate) sai vì spam/bad data tràn vào DB.
- **Mã nguồn thực tế (`frontend/src/views/HelpView.vue`):**
```typescript
// frontend/src/views/HelpView.vue
function submitContact(): void {
  contactError.value = '';
  if (contact.value.name.trim().length < 2 || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(contact.value.email)) {
    contactError.value = 'Vui lòng điền tên và email hợp lệ.';
    return;
  }
  if (contact.value.message.trim().length < 10) {
    contactError.value = 'Nội dung phải từ 10 ký tự.';
    return;
  }
  contactSent.value = true;
}
```

---

#### **Câu 14.3 — Đáp án: D**
- **① Khái niệm:** Anchor-based Table of Contents (TOC) là danh sách link nội bộ dùng `<a href="#section-id">` để nhảy đến section tương ứng trong trang, kết hợp smooth-scroll library (Lenis) để cuộn mượt thay vì jump tức thì.
- **② Vì sao lại như thế:** Privacy Policy dài ~2000 từ, 6 sections. Không có TOC: user phải scroll thủ công tìm mục cần đọc. Native anchor link hoạt động nhưng jump tức thì gây mất phương hướng. Lenis intercept anchor click → animate cuộn mượt 300-500ms → user thấy context di chuyển → định vị tốt hơn.
- **③ Áp dụng sâu:** `SECTIONS` array khai báo 6 mục: `{ id: 'sec-1', title: 'Dữ liệu chúng tôi thu thập' }`. TOC render từ array này → đảm bảo TOC và content luôn đồng bộ. Cross-ref Đề 17: Lenis singleton tạo ở App.vue, option `anchors: true` bật tính năng này toàn app. **Mẹo nhớ:** TOC = Table Of Contents = Mục lục neo (anchor).
- **❌ Tại sao C/B/A sai:** C (JavaScript scrollIntoView) sai vì không smooth mặc định; B (router navigation) sai vì TOC là intra-page, không đổi route; A (không cần TOC) sai vì tài liệu pháp lý dài bắt buộc có navigation aid.
- **Mã nguồn thực tế (`frontend/src/views/PrivacyView.vue`):**
```typescript
// frontend/src/views/PrivacyView.vue
const SECTIONS = [
  { id: 'sec-1', title: 'Dữ liệu chúng tôi thu thập' },
  { id: 'sec-2', title: 'Cookie & phiên đăng nhập' },
  // ... 6 sections total
] as const;
```

---

#### **Câu 14.4 — Đáp án: C**
- **① Khái niệm:** Feedback Submission Guard là cơ chế backend kiểm tra điều kiện tiên quyết (precondition) trước khi chấp nhận phản hồi — cụ thể: user phải hoàn thành bài học (mark-viewed) mới được gửi feedback cho bài đó.
- **② Vì sao lại như thế:** Không có guard: user chưa học bài vẫn gửi feedback "Cài giảng khó hiểu" → giảng viên nhận phản hồi vô nghĩa, tốn thời gian xử lý. Guard đảm bảo feedback chỉ đến từ người đã trải nghiệm thực tế → chất lượng cao hơn. Kiểm tra ở BACKEND (không chỉ frontend) vì frontend có thể bypass bằng DevTools/API call trực tiếp.
- **③ Áp dụng sâu:** `FeedbackController.Submit` truy vấn trực tiếp bảng `UserProgress` xem cờ `Viewed` của cặp `(userId, lessonId)` có `true` chưa → nếu chưa → return 403 Forbidden (ErrorCodes.FORBIDDEN, "Bạn cần học bài này trước khi đánh giá"). Frontend nhận 403 → toast lỗi. **Cross-ref Đề 06:** LessonStudyView gọi `POST /lessons/{id}/mark-viewed` để set `UserProgress.Viewed = true` → đây là điều kiện tiên quyết cho feedback.
- **❌ Tại sao A/B/D sai:** A (cho phép tất cả) sai vì spam feedback tràn ngập; B (chỉ TEACHER mới gửi) sai vì student feedback rất giá trị; D (rate limit only) sai vì rate limit không ngăn feedback vô nghĩa từ user chưa học.
- **Mã nguồn thực tế (`backend/src/DsaVisual.Api/Controllers/FeedbackController.cs`):**
```csharp
// backend/src/DsaVisual.Api/Controllers/FeedbackController.cs — anti-spam
var viewed = await _db.UserProgress.AsNoTracking()
    .AnyAsync(p => p.UserId == CurrentUserId() && p.LessonId == request.LessonId && p.Viewed, ct);
if (!viewed)
    return StatusCode(403, ErrorResponseDto.Create(ErrorCodes.FORBIDDEN, "Bạn cần học bài này trước khi đánh giá"));
```

---

#### **Câu 14.5 — Đáp án: B**
- **① Khái niệm:** Public API Endpoint là endpoint không yêu cầu authentication (`[AllowAnonymous]`), phục vụ dữ liệu hiển thị công khai cho landing page/trang chủ mà khách vãng lai cũng truy cập được.
- **② Vì sao lại như thế:** Trang chủ HomeView hiển thị "44 thuật toán · 12 cấu trúc dữ liệu · 200+ bài học" ngay khi chưa login. Nếu endpoint yêu cầu auth: khách thấy trang trống → bounce rate tăng. `AsNoTracking()` tối ưu EF Core query: không load change tracker vì chỉ đọc, không ghi → giảm memory allocation ~40%.
- **③ Áp dụng sâu:** `GET /api/v1/public/site-info` trả `SiteInfoDto { Structures, Algorithms, Lessons }`. HomeView gọi trong `onMounted` → bind vào hero section stats. **Security note:** Endpoint này CHỈ trả số đếm, không trả nội dung bài học hay thông tin user → không leak data. Cross-ref Đề 02: HomeView cũng dùng DemoBanner để convert khách → register.
- **❌ Tại sao A/D/C sai:** A (yêu cầu auth) sai vì khách không thấy stats; D (hardcode số) sai vì số liệu thay đổi khi thêm bài mới; C (cache Redis) sai vì over-engineering cho 3 count queries nhẹ.
- **Mã nguồn thực tế (`backend/src/DsaVisual.Api/Controllers/PublicController.cs`):**
```csharp
// backend/src/DsaVisual.Api/Controllers/PublicController.cs
[AllowAnonymous]
[HttpGet("site-info")]
public async Task<ActionResult<SiteInfoDto>> GetSiteInfo(CancellationToken ct)
{
    var list = await catalog.GetListAsync(ct);
    var structures = list.IsSuccess ? list.Value!.Count(s => s.Category == "structure") : 0;
    var algorithms = list.IsSuccess ? list.Value!.Count(s => s.Category == "algorithm") : 0;
    var lessons = await db.Lessons.AsNoTracking()
        .CountAsync(l => l.DeletedAt == null && l.Status == LessonStatus.Active, ct);
    return Ok(new SiteInfoDto { Structures = structures, Algorithms = algorithms, Lessons = lessons });
}
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 14.1: Trace luồng Gửi Phản hồi Khóa học → Giảng viên Trả lời (2.5 điểm)

> **💡 Ngữ cảnh thực tế:** Flow này kết nối sinh viên (gửi feedback) ↔ giảng viên (trả lời) qua CourseFeedbackController. AdminFeedbackView (cross-ref Đề 16) là nơi giảng viên xem và xử lý feedback.

- **1. Submit Feedback (0.5đ):** Sinh viên bấm "Gửi phản hồi" → `POST /api/v1/courses/feedback` với body `{ courseId, type: "Bug"|"Request"|"Suggestion", content }`. **Tại sao có type?** Giúp giảng viên filter/prioritize: Bug = ưu tiên cao, Suggestion = xem sau. Backend validate: content ≥ 10 ký tự, courseId tồn tại, user đã enroll course.
- **2. Backend Lưu (0.75đ):** `CourseFeedbackController.Submit` tạo row trong bảng `CourseFeedbacks`: `{ UserId, CourseId, Type, Content, Status = "New", CreatedAt }`. **Quan trọng:** Status mặc định "New" → AdminFeedbackView filter tab "Chưa xử lý" hiện ngay. Không notify realtime (giảng viên check manual) → giảm complexity, tránh notification fatigue.
- **3. Giảng viên Trả lời (0.75đ):** Giảng viên vào AdminFeedbackView → click feedback item → viết reply → `PUT /api/v1/courses/feedback/{id}/reply` với `{ replyText, status: "Resolved" }`. Backend cập nhật: `ReplyText, RepliedByName = currentUser.DisplayName, RepliedAt = now, Status = "Resolved"`. **Audit trail:** RepliedByName lưu tên người trả lời → accountability khi nhiều giảng viên cùng quản lý.
- **4. UI Update (0.5đ):** Frontend nhận response → `items.value[idx] = updatedItem` → badge status đổi từ vàng "Mới" sang xanh "Đã xử lý". Toast "Đã lưu phản hồi" confirm action. **Không auto-refresh list** → giữ scroll position, tránh user mất chỗ đang xem.

---

#### 📝 Câu TL 14.2: Trace luồng FAQ Toggle + Contact Submit trên HelpView (2.5 điểm)

> **💡 Ngữ cảnh thực tế:** HelpView là trang self-service đầu tiên user tìm đến khi gặp vấn đề. FAQ giải quyết 80% câu hỏi phổ biến; Contact form cho 20% còn lại. Thiết kế ưu tiên FAQ trước contact để giảm tải support.

- **1. Khởi tạo FAQ Data (0.5đ):** Mảng `FAQS` gồm 6 câu hỏi hardcode trong component, nội dung tiếng Việt UTF-8 (tránh mojibake). **Tại sao hardcode mà không fetch API?** FAQ ít thay đổi (monthly), hardcode = 0 network request = instant render. Nếu cần dynamic: chuyển sang i18n messages (đã hỗ trợ sẵn qua `messages.help`).
- **2. Toggle Accordion (0.75đ):** User click câu hỏi → `toggle(idx)` set `openIndex = openIndex === idx ? null : idx`. Nếu click câu đang mở → đóng (null). Nếu click câu khác → mở câu mới, đóng câu cũ (single-open mode). Vue Transition `name="faq"` animate height opacity 200ms ease-out. **UX detail:** Single-open mode giảm cognitive load so với multi-open (user không bị overwhelm bởi 6 câu trả lời cùng lúc).
- **3. Contact Validation Fail (0.75đ):** User điền form → bấm "Gửi" → `submitContact()` chạy validation chain: name.trim().length < 2 OR email không match regex → set `contactError.value` → UI hiện text đỏ dưới form, KHÔNG gửi API. Message ≥ 10 ký tự check riêng → lỗi riêng. **Tại sao message min 10?** Ngăn spam "abc", "test" vô nghĩa.
- **4. Success State (0.5đ):** Validation pass → `contactSent.value = true` → v-if/v-else swap form thành thông báo cảm ơn với icon CheckCircle2 xanh. **Không reset form ngay** → user có thể screenshot confirmation. Form data giữ nguyên trong ref → nếu user reload trang, form reset tự nhiên (stateless).

---

# 📘 ĐÁP ÁN ĐỀ 15: STUDIO GIẢNG VIÊN & QUẢN LÝ NỘI DUNG HỌC TẬP

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 15.1 — Đáp án: D**
- **① Khái niệm:** Parallel Data Fetching với Resilience là pattern gọi nhiều API đồng thời bằng `Promise.all`, kèm `.catch()` riêng cho từng call để một API fail không làm crash toàn bộ trang.
- **② Vì sao lại như thế:** TeacherStudioView cần 4 dữ liệu độc lập (lessons, topics, courses, classes). Gọi tuần tự: 4 × 300ms = 1.2s. Gọi song song: max(300ms) ≈ 300ms → nhanh gấp 4 lần. Nếu topics API fail mà không có `.catch()`: Promise.all reject → cả trang trắng. Với `.catch(() => [])`: topics fail → mảng rỗng, 3 phần còn lại vẫn render bình thường.
- **③ Áp dụng sâu:** `loadStudioData()` wrap trong try/catch lớn + `loading.value` flag. Mỗi `.catch()` trả fallback value phù hợp type ([] cho array, undefined cho object). Cross-ref Đề 19: Axios interceptor xử lý lỗi ở tầng transport; `.catch()` ở đây xử lý lỗi ở tầng business logic.
- **❌ Tại sao B/C/A sai:** B (tuần tự await) sai vì chậm 4x; C (chỉ gọi 1 API) sai vì thiếu data; A (không catch) sai vì 1 API fail = trang crash.
- **Mã nguồn thực tế (`frontend/src/views/TeacherStudioView.vue`):**
```typescript
// frontend/src/views/TeacherStudioView.vue
async function loadStudioData(): Promise<void> {
  loading.value = true;
  try {
    const [lessonData, topicTree, courseList] = await Promise.all([
      lessonsApi.fetchLessons({ page: 1, pageSize: 8 }),
      lessonsApi.fetchTopics().catch(() => []),
      courseApi.getCourses().catch(() => [] as CourseListDto[]),
      classStore.fetchClasses().catch(() => {}),
    ]);
```

---

#### **Câu 15.2 — Đáp án: C**
- **① Khái niệm:** Role-based Route Guard là cơ chế kiểm tra quyền truy cập route dựa trên vai trò user, khai báo qua `meta.roles` trong router config và kiểm tra trong `router.beforeEach` global guard.
- **② Vì sao lại như thế:** Studio là trang quản lý nội dung nhạy cảm — chỉ giảng viên và admin được phép truy cập. Nếu không guard: sinh viên gõ thẳng `/studio` → thấy trang soạn bài → có thể tạo nội dung trái phép. Guard check `auth.role` ∈ `['TEACHER', 'ADMIN']` → nếu không khớp → redirect về `/path` + toast cảnh báo. Kiểm tra ở ROUTER (không chỉ component) vì chặn sớm nhất, trước khi component mount.
- **③ Áp dụng sâu:** Router guard trong `router/index.ts` đọc `to.meta.roles`, so sánh với `authStore.role`. Cross-ref Đề 01: cùng mechanism với `guestOnly` guard cho login/register. **Lưu ý:** Backend cũng check role lần nữa (defense in depth) → frontend guard chỉ là UX optimization.
- **❌ Tại sao B/A/D sai:** B (cho phép tất cả) sai vì leak trang quản trị; A (chỉ ADMIN) sai vì giảng viên cũng cần truy cập; D (check trong component) sai vì component đã mount rồi mới chặn → flash content.
- **Mã nguồn thực tế (`frontend/src/router/index.ts`):**
```typescript
// frontend/src/router/index.ts
{
  path: '/studio',
  component: AdminContentView,
  meta: { requiresAuth: true, roles: ['TEACHER', 'ADMIN'] },
}
```

---

#### **Câu 15.3 — Đáp án: B**
- **① Khái niệm:** Reference Links Pattern là cơ chế liên kết động từ simKey đến tài liệu tham khảo bên ngoài (Wikipedia, GeeksforGeeks), lưu tập trung trong `referenceLinks.ts` thay vì hardcode trong từng component.
- **② Vì sao lại như thế:** 44 thuật toán cần link tham khảo. Nếu hardcode trong mỗi view: sửa 1 link phải tìm đúng file. Tập trung trong `referenceLinks.ts`: map `simKey → { wikipedia?, geeksforgeeks? }` → sửa 1 chỗ, tất cả views tự cập nhật. Computed property filter null links → chỉ hiện link tồn tại.
- **③ Áp dụng sâu:** NodeHubView dùng `getReference(simKey)` → computed `referenceLinks` render list `<a>` tags. Cross-ref Đề 18: catalog.ts chứa metadata complexity; referenceLinks.ts chứa external URLs — tách biệt concerns. **Mẹo nhớ:** referenceLinks = "từ điển tham chiếu" của hệ thống.
- **❌ Tại sao D/C/A sai:** D (hardcode trong template) sai vì không maintainable; C (fetch từ API) sai vì over-engineering cho static data; A (không có link) sai vì mất tính năng học tập quan trọng.
- **Mã nguồn thực tế (`frontend/src/views/NodeHubView.vue`):**
```typescript
// frontend/src/views/NodeHubView.vue
const referenceLinks = computed(() => {
  const ref = getReference(simKey.value);
  if (!ref) return [];
  const links: { label: string; url: string }[] = [];
  if (ref.wikipedia) links.push({ label: 'Wikipedia', url: ref.wikipedia });
  if (ref.geeksforgeeks) links.push({ label: 'GeeksforGeeks', url: ref.geeksforgeeks });
  return links;
});
```

---

#### **Câu 15.4 — Đáp án: A**
- **① Khái niệm:** Adapter Controller Pattern là controller đóng vai trò lớp chuyển đổi (adapter) giữa backend schema mới và frontend contract cũ, cho phép FE legacy hoạt động mà không cần refactor.
- **② Vì sao lại như thế:** Dự án migrate từ VisualizationDSA-main sang DsaVisual backend. Schema khác nhau: LearningPath vs Course, Node vs Lesson. Thay vì sửa 20+ FE components, viết 1 adapter controller map entity mới → DTO cũ. **Trade-off:** Thêm indirection layer nhưng tiết kiệm hàng tuần refactor FE.
- **③ Áp dụng sâu:** ConceptsController map: `LearningPath → CourseDto`, `Node → LessonDto`, `Exercise MCQ → QuizDto`, `Exercise CODE → CodelabDto`. FE gọi `/api/v1/concepts/*` như cũ, không biết backend đã đổi. Cross-ref Đề 06: CoursesListView/CourseDetailView dùng endpoint này.
- **❌ Tại sao B/D/C sai:** B (FE sửa theo backend mới) sai vì tốn effort lớn; D (dual API) sai vì duplicate maintenance; C (breaking change) sai vì phá hỏng FE đang chạy.
- **Mã nguồn thực tế (`backend/src/DsaVisual.Api/Controllers/ConceptsController.cs`):**
```csharp
// backend/src/DsaVisual.Api/Controllers/ConceptsController.cs
/// Adapter "Concepts" — trả ĐÚNG format API của VisualizationDSA-main (/api/v1/concepts/*)
/// để FE CoursesListView/CourseDetailView/LessonStudyView chạy trên backend DsaVisual.
/// Map: LearningPath → Course, Node → Lesson, Exercise MCQ → Quiz, Exercise CODE → Codelab.
```

---

#### **Câu 15.5 — Đáp án: D**
- **① Khái niệm:** Multi-stage Completion Tracking là cơ chế theo dõi tiến độ học tập qua nhiều bậc (quiz → lab → code), yêu cầu pass TẤT CẢ bậc mới đánh dấu node hoàn thành.
- **② Vì sao lại như thế:** DSA learning cần cả lý thuyết (quiz) lẫn thực hành (lab + code). Chỉ pass quiz = hiểu lý thuyết nhưng không code được. Yêu cầu cả 3 bậc → đảm bảo competency toàn diện. Progress lưu localStorage (`dsa-ladder-<nodeId>`) → persist qua refresh, không cần backend call cho mỗi check.
- **③ Áp dụng sâu:** `LADDER_STAGES = ['quiz', 'lab', 'code']`; `passedStages` Set track bậc đã pass; `nodeCompleted = LADDER_STAGES.every(s => passedStages.has(s))`. Cross-ref Đề 07: LadderView render 3 bậc UI; Đề 19: Exercise submit flow cập nhật passedStages. **Lưu ý:** localStorage = client-side only → không sync giữa devices; backend tracking riêng cho reporting.
- **❌ Tại sao A/C/B sai:** A (chỉ cần 1 bậc) sai vì không đảm bảo competency; C (server-only tracking) sai vì chậm + offline không hoạt động; B (không track) sai vì user không biết tiến độ.
- **Mã nguồn thực tế (`frontend/src/views/NodeHubView.vue`):**
```typescript
// frontend/src/views/NodeHubView.vue
const LADDER_STAGES = ['quiz', 'lab', 'code'] as const;
const passedStages = ref<Set<LadderStageKey>>(new Set());
const nodeCompleted = computed(() => LADDER_STAGES.every((s) => passedStages.value.has(s)));
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 15.1: Trace luồng Giảng viên Soạn Bài học Mới qua AdminLessonEditorView (2.5 điểm)

> **💡 Ngữ cảnh thực tế:** Đây là flow cốt lõi của Teacher Studio — giảng viên tạo nội dung học tập mới. Cross-ref Đề 15 TN (Câu 15.2): route guard đảm bảo chỉ TEACHER/ADMIN truy cập được.

- **1. Editor Layout (0.5đ):** 2 cột: textarea markdown trái + `MarkdownRenderer` preview phải; KaTeX render công thức toán, highlight.js tô màu code block. **Tại sao WYSIWYG thay vì rich text editor?** Markdown nhẹ hơn, version-control friendly, và giảng viên DSA quen viết technical docs. Preview realtime giúp thấy kết quả ngay mà không cần save.
- **2. SimKey Dropdown (0.75đ):** Lấy danh sách từ `CATALOG` trong `engines/catalog.ts`, filter theo category phù hợp. **Tại sao dropdown thay vì free-text?** Ngăn typo simKey → nếu gõ sai "sort.buble" thay vì "sort.bubble", simulator không load được. Dropdown đảm bảo key luôn valid. Cross-ref Đề 18: catalog.ts là single source of truth cho 44 mô phỏng.
- **3. Submit (0.75đ):** `POST /api/v1/lessons` với `{ topicId, title, contentMarkdown, simulationKey }`; Backend check `User.Role in ['TEACHER','ADMIN']`. **Defense in depth:** Dù frontend đã có route guard, backend vẫn verify role lần nữa → chống API call trực tiếp bypass frontend.
- **4. Post-create (0.5đ):** Router push `/studio`; bài học mới xuất hiện trong danh sách recent lessons và trên cây lộ trình `/path/:topicId`. **Không redirect đến bài vừa tạo** → giảng viên thường tạo nhiều bài liên tiếp, giữ ở studio list hiệu quả hơn.

---

#### 📝 Câu TL 15.2: Trace luồng NodeHubView Map Topic×Node → SimKey → Catalog Meta (2.5 điểm)

> **💡 Ngữ cảnh thực tế:** NodeHubView là trung tâm điều hướng học tập — kết nối topic/node với simulator và lý thuyết. Cross-ref Đề 15 TN (Câu 15.3, 15.5): cùng view xử lý reference links và ladder tracking.

- **1. SimKey Map (0.5đ):** `topicId=1, nodeId=3` → `keysByTopic[1][2] = 'sort.insertion'`; nếu key không có trong catalog → fallback `'sort.bubble'`. **Tại sao cần fallback?** Ngăn trang trắng khi mapping thiếu entry mới thêm. Bubble Sort là thuật toán cơ bản nhất → an toàn làm default. Mapping lưu trong `nodeHubData.ts` → tách biệt data khỏi logic.
- **2. Catalog Meta (0.75đ):** `getCatalogMeta(simKey)` trả về `{ title, category, complexity: { best, average, worst, space } }`. **Tại sao đọc từ catalog thay vì hardcode?** Single source of truth — sửa complexity ở 1 chỗ (catalog.ts), tất cả views tự cập nhật. Cross-ref Đề 18: catalog.ts chứa metadata cho cả 44 mô phỏng.
- **3. Theory Tab (0.75đ):** Render `LessonDetail` component; markdown content từ `GET /api/v1/lessons/{lessonId}`. **Lazy load theory:** Chỉ fetch khi user click tab "Lý thuyết" → giảm initial load time. Nếu lesson chưa có content → hiện EmptyState + link "Soạn bài" (cho giảng viên).
- **4. Practice Tab (0.5đ):** Nhúng `LadderShell :nodeId="nodeId"`; progress đọc từ localStorage `dsa-ladder-<nodeId>`. Cross-ref Đề 15 TN (Câu 15.5): `passedStages` Set track 3 bậc quiz/lab/code. **Tab switching:** Vue `<KeepAlive>` giữ state khi chuyển tab → user không mất tiến độ đang làm dở.
- **4. Practice Tab (0.5đ):** Nhúng `LadderShell :nodeId="nodeId"`; progress đọc từ localStorage `dsa-ladder-<nodeId>`.

---

# 📘 ĐÁP ÁN ĐỀ 16: KIỂM TRA CUỐI LỘ TRÌNH & ĐIỀU HƯỚNG THÔNG MINH

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 16.1 — Đáp án: B**
- **① Khái niệm:** Offline Fallback Test Generation là cơ chế tự động sinh đề kiểm tra từ metadata local khi API backend không khả dụng, đảm bảo user luôn có thể làm bài dù mất kết nối.
- **② Vì sao lại như thế:** Final Test là bước cuối cùng của lộ trình — nếu API fail mà trang trắng, user mất động lực hoàn thành. Fallback sinh 5 câu MCQ từ CATALOG metadata (complexity info) → đề luôn hợp lệ, không cần server. Threshold 70% = chuẩn pass phổ biến trong giáo dục (tương đương điểm C+).
- **③ Áp dụng sâu:** `buildLocalFinalTest()` filter CATALOG lấy 5 algorithm đầu → tạo QuestionDto với options từ complexity data. Cross-ref Đề 18: CATALOG chứa metadata cho 44 mô phỏng. **Lưu ý:** Fallback đề chỉ dùng khi API fail; bình thường đề lấy từ backend (dynamic, personalized).
- **❌ Tại sao A/C/D sai:** A (threshold 50%) sai vì quá thấp, không đảm bảo competency; C (không fallback) sai vì offline = không thi được; D (random questions) sai vì không liên quan nội dung đã học.
- **Mã nguồn thực tế (`frontend/src/views/FinalTestView.vue`):**
```typescript
// frontend/src/views/FinalTestView.vue
const passThreshold = 70;
function buildLocalFinalTest(): ExerciseDto {
  const keys = CATALOG.filter((c) => c.category === 'algorithm').slice(0, 5);
  const questions: QuestionDto[] = keys.map((meta, idx) => ({
    id: 1000 + idx,
    content: `Độ phức tạp trung bình của "${meta.title}" là bao nhiêu?`,
    type: 'SINGLE', options: [meta.complexity.average, meta.complexity.best, meta.complexity.worst, 'O(1)'],
    points: 2,
  }));
  return { id: 0, title: 'Kiểm tra cuối lộ trình — Đề mẫu', /* ... */ };
}
```

---

#### **Câu 16.2 — Đáp án: C**
- **① Khái niệm:** Component Delegation Pattern là việc parent component (FinalTestView) ủy quyền rendering và logic cho child component chuyên biệt (QuizStage), giao tiếp qua props (input) và events (output).
- **② Vì sao lại như thế:** FinalTestView quản lý flow tổng thể (fetch đề, xử lý kết quả, navigation); QuizStage xử lý chi tiết UI câu hỏi (render options, track selection, calculate score). Tách biệt concerns → mỗi component nhỏ, dễ test, dễ reuse. QuizStage cũng dùng trong LadderView Bậc 1 → code sharing.
- **③ Áp dụng sâu:** FinalTestView truyền `exercise` prop → QuizStage render câu hỏi. Khi submit → emit `passed(scorePct)` → parent nhận kết quả → toast + navigation. Cross-ref Đề 07: cùng QuizStage dùng cho Ladder Bậc 1.
- **❌ Tại sao A/B/D sai:** A (render trực tiếp) sai vì duplicate code với LadderView; B (dùng iframe) sai vì over-engineering; D (redirect sang trang khác) sai vì mất context final test.
- **Mã nguồn thực tế (`frontend/src/views/FinalTestView.vue`):**
```typescript
// frontend/src/views/FinalTestView.vue
import QuizStage from '@/components/ladder/QuizStage.vue';
function onPassed(scorePct: number): void { /* toast logic */ }
```

---

#### **Câu 16.3 — Đáp án: A**
- **① Khái niệm:** Graceful Degradation với Static Fallback là pattern hiển thị nội dung tĩnh khi API fail, thay vì trang lỗi hoặc loading vô hạn — giữ UX tối thiểu chấp nhận được.
- **② Vì sao lại như thế:** PathRedirectView load topics từ API để điều hướng user. Nếu API fail (network error, server down): trang trắng = user stuck. LOCAL_TOPICS cung cấp 5 topics cốt lõi → user vẫn navigate được đến NodeHub → tiếp tục học. ProgressBar/EmptyState thông báo rõ ràng "đang offline" thay vì im lặng fail.
- **③ Áp dụng sâu:** `LOCAL_TOPICS` array hardcode 5 entries: Sắp xếp, CTDL tuyến tính, Cây, Đồ thị, Hash. Mỗi entry có `{ id, name, description }` đủ để render card. Click card → `router.push` đến NodeHub. Cross-ref Đề 15 TL 15.2: NodeHubView nhận topicId từ route params.
- **❌ Tại sao C/B/D sai:** C (hiện lỗi 500) sai vì UX tệ; B (retry vô hạn) sai vì user chờ mãi; D (redirect home) sai vì mất context học tập.
- **Mã nguồn thực tế (`frontend/src/views/PathRedirectView.vue`):**
```typescript
// frontend/src/views/PathRedirectView.vue
const LOCAL_TOPICS: Array<{ id: number; name: string; description: string }> = [
  { id: 1, name: 'Sắp xếp & Tìm kiếm', description: 'Bubble, Selection, Insertion, Merge, Quick, Heap...' },
  { id: 2, name: 'CTDL tuyến tính', description: 'Ngăn xếp, hàng đợi, danh sách liên kết' },
  { id: 3, name: 'Cây', description: 'BST, AVL, duyệt cây, heap' },
  // ... 5 topics total
];
```

---

#### **Câu 16.4 — Đáp án: D**
- **① Khái niệm:** Composite Client-side Filtering là kết hợp nhiều tiêu chí lọc (status dropdown + text search) trong một computed property, áp dụng tuần tự để thu hẹp kết quả hiển thị.
- **② Vì sao lại như thế:** AdminFeedbackView có thể hàng trăm feedbacks. Chỉ hiện tất cả → overwhelm. Status filter (New/Resolved) giúp focus vào feedback chưa xử lý. Text search tìm nhanh theo nội dung hoặc tên user. Kết hợp cả 2 → precision cao. Computed property tự động re-evaluate khi filter thay đổi → reactive, không cần manual refresh.
- **③ Áp dụng sâu:** `filteredItems` computed: đầu tiên filter theo status (nếu chọn), sau đó filter theo search query (case-insensitive, match content HOẶC userName). Cross-ref Đề 14 TL 14.1: feedback data structure có `status`, `content`, `userName` fields.
- **❌ Tại sao A/C/B sai:** A (chỉ status filter) sai vì không tìm được theo nội dung; C (server-side search) sai vì latency cao cho admin tool; B (không filter) sai vì list quá dài.
- **Mã nguồn thực tế (`frontend/src/views/AdminFeedbackView.vue`):**
```typescript
// frontend/src/views/AdminFeedbackView.vue
const filteredItems = computed(() => {
  let list = items.value;
  if (statusFilter.value) list = list.filter((i) => i.status === statusFilter.value);
  if (searchQuery.value.trim()) {
    const q = searchQuery.value.toLowerCase().trim();
    list = list.filter((i) => i.content.toLowerCase().includes(q) || (i.userName && i.userName.toLowerCase().includes(q)));
  }
  return list;
});
```

---

#### **Câu 16.5 — Đáp án: A**
- **① Khái niệm:** Conditional Feedback Toast là pattern hiển thị thông báo khác nhau dựa trên kết quả (pass/fail), dùng màu sắc và message phù hợp ngữ cảnh để guide user action tiếp theo.
- **② Vì sao lại như thế:** Pass (≥70%): toast xanh "Chúc mừng!" → positive reinforcement → user tự tin. Fail (<70%): toast vàng "Bạn đạt X%, cần ≥70%" → constructive feedback → user biết chính xác gap cần cải thiện. Không dùng toast đỏ/error vì fail không phải lỗi hệ thống → tránh negative emotion.
- **③ Áp dụng sâu:** `onPassed(scorePct)` check threshold → gọi `ui.showToast()` với message và type tương ứng. Message template `toastFailed(threshold, actual)` cho biết chính xác điểm hiện tại vs yêu cầu. Cross-ref Đề 16 TN 16.1: cùng threshold 70%.
- **❌ Tại sao C/B/D sai:** C (luôn success toast) sai vì misleading khi fail; B (alert dialog) sai vì blocking UX; D (không feedback) sai vì user không biết kết quả.
- **Mã nguồn thực tế (`frontend/src/views/FinalTestView.vue`):**
```typescript
// frontend/src/views/FinalTestView.vue
function onPassed(scorePct: number): void {
  ui.showToast(
    scorePct >= passThreshold
      ? messages.finalTest.toastPassed
      : messages.finalTest.toastFailed(passThreshold, scorePct),
    scorePct >= passThreshold ? 'success' : 'warning',
  );
}
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 16.1: Trace luồng Làm Final Test → Chấm điểm → Hiển thị Kết quả (2.5 điểm)

> **💡 Ngữ cảnh thực tế:** Final Test là checkpoint cuối cùng của mỗi lộ trình học tập. Cross-ref Đề 16 TN (16.1, 16.5): threshold 70% và toast feedback logic.

- **1. Fetch/Fallback (0.5đ):** `onMounted` gọi `fetchLocalFinalTest()`; fail → `buildLocalFinalTest()` sinh 5 câu MCQ từ CATALOG metadata. **Tại sao fallback thay vì báo lỗi?** User đã học xong cả lộ trình, nếu API fail mà không cho làm test → trải nghiệm tệ, mất động lực. Fallback dùng metadata local (complexity info) → đề luôn hợp lệ dù offline. **Lỗi thường gặp:** Nếu CATALOG rỗng (bundle lỗi), fallback cũng fail → cần thêm guard check `CATALOG.length > 0`.
- **2. QuizStage Delegation (0.75đ):** FinalTestView KHÔNG tự render câu hỏi mà delegate cho `QuizStage` component qua prop `exercise`. Khi user submit → QuizStage chấm và emit `passed(scorePct)`. **Tại sao delegate?** Single Responsibility — FinalTestView chỉ lo orchestration (fetch, toast, navigation); QuizStage lo rendering + scoring. Component này tái dùng ở LadderView (Đề 07) → DRY principle.
- **3. Toast Feedback (0.75đ):** `scorePct >= 70` → toast success "Chúc mừng! Bạn đã vượt qua"; < 70 → toast warning "Cần ≥70%, hiện tại {scorePct}%". **UX principle:** Feedback tức thì + actionable (cho biết chính xác gap). Không dùng alert dialog → blocking UX. Cross-ref Đề 19 TL 19.2: Exercise submit flow cũng dùng toast pattern tương tự.
- **4. Manual Navigation (0.5đ):** KHÔNG auto-redirect sau khi pass. User bấm nút "Quay lại lộ trình" (RouterLink) thủ công. **Tại sao không auto?** User có thể muốn xem lại kết quả, screenshot thành tích, hoặc đọc lại giải thích. Auto-redirect = cướp quyền kiểm soát → bad UX. Pattern này nhất quán với LessonCompletionModal (Đề 06).

---

#### 📝 Câu TL 16.2: Trace luồng PathRedirectView Load Topics → Fallback → Điều hướng NodeHub (2.5 điểm)

> **💡 Ngữ cảnh thực tế:** PathRedirectView là entry point của lộ trình học tập — quyết định user đi tiếp hay thấy trang trắng khi backend sự cố. Cross-ref Đề 16 TN (16.3): LOCAL_TOPICS fallback data.

- **1. API Call & State Management (0.5đ):** `onMounted` gọi `gamificationApi.getLearningPaths()`. Ba reactive refs: `paths` (data), `loading` (spinner), `apiFailed` (fallback trigger). **Tại sao tách apiFailed riêng thay vì chỉ check paths.length === 0?** Phân biệt "API trả mảng rỗng" (user chưa có progress) vs "API fail" (network error). Hai trường hợp cần UI khác nhau: mảng rỗng → EmptyState khuyến khích bắt đầu; API fail → LOCAL_TOPICS fallback.
- **2. Graceful Degradation (0.75đ):** Khi `apiFailed === true`, render `LOCAL_TOPICS` (5 phần tử hardcode: Sắp xếp, CTDL tuyến tính, Cây, Đồ thị, Nâng cao). Mỗi card có `{ id, name, description }`. **Tại sao 5 topics thay vì fetch retry?** Retry = thêm latency + có thể fail tiếp. Hardcode topics = instant render, user vẫn điều hướng được. Topics này ổn định (ít thay đổi hàng tháng) → safe to hardcode. **Trade-off:** Fallback topics có thể stale nếu admin thêm topic mới → chấp nhận được vì user vẫn vào được hệ thống.
- **3. Programmatic Navigation (0.75đ):** Click card topicId=3 → `router.push({ name: 'path-topic', params: { topicId: '3' } })` → load `NodeHubView`. **Tại sao push object thay vì string path `/path/3`?** Named routes an toàn hơn — nếu path đổi từ `/path/:id` sang `/learning-path/:id`, chỉ cần sửa router config, không cần tìm-sửa tất cả string paths trong codebase. Cross-ref Đề 15 TL 15.2: NodeHubView nhận topicId và map sang simKey.
- **4. Route Params Extraction (0.5đ):** Trong NodeHubView: `const route = useRoute(); const topicId = Number(route.params.topicId); const nodeId = Number(route.params.nodeId);`. **Type safety:** `route.params` luôn trả string → phải `Number()` convert. Nếu param thiếu (user vào `/path` không có topicId) → `NaN` → cần guard clause redirect về danh sách topics. **Mẹo nhớ:** Luôn validate route params trước khi dùng — đừng giả sử URL luôn đúng format.




---

# 📘 ĐÁP ÁN ĐỀ 17: COMPOSABLES, HIỆU ỨNG & CROSS-CUTTING CONCERNS

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 17.1 — Đáp án: C**
- **① Khái niệm:** Store Delegation Pattern trong composable là việc composable KHÔNG tự quản lý state/logic mà chỉ wrap Pinia store, cung cấp lifecycle hooks tự động (auto-load, auto-cleanup) và trả về reactive refs từ store.
- **② Vì sao lại như thế:** Nếu mỗi component tự tạo generator/timer riêng → memory leak khi navigate nhanh, state không sync giữa các tab. Delegating cho singleton store đảm bảo: (1) chỉ 1 instance chạy toàn app, (2) state persist qua route change, (3) cleanup tập trung tại 1 chỗ. `getCurrentInstance()` guard ngăn crash khi gọi ngoài setup().
- **③ Áp dụng sâu:** `onMounted` gọi `loadSim()` để pre-fetch data; `onUnmounted` gọi `store.stopPlayback()` để dừng animation timer. Cross-ref Đề 03: SimulatorView dùng useSimulation('sort.bubble') → store điều khiển canvas. **Lỗi thường gặp:** Quên stopPlayback → timer chạy ngầm sau khi rời trang → CPU spike.
- **❌ Tại sao B/D/A sai:** B (tự tạo generator) sai vì vi phạm singleton; D (watch route) sai vì store đã react với key param; A (không cleanup) sai vì gây memory leak.
- **Mã nguồn thực tế (`frontend/src/composables/useSimulation.ts`):**
```typescript
// frontend/src/composables/useSimulation.ts
export function useSimulation(key: string) {
  const store = useSimulationStore();
  const state = storeToRefs(store);
  if (getCurrentInstance()) {
    onMounted(() => { void loadSim().catch(() => {}); });
    onUnmounted(() => { store.stopPlayback(); });
  }
  return { ...state, loadSim, play: store.play, pause: store.pause, /* ... */ };
}
```

---

#### **Câu 17.2 — Đáp án: B**
- **① Khái niệm:** Trace Sampling là kỹ thuật giảm số frame hiển thị bằng cách lấy mẫu theo bước nhảy cố định (step = ceil(total/maxFrames)), nhưng LUÔN giữ frame cuối cùng để đảm bảo trạng thái kết thúc đúng.
- **② Vì sao lại như thế:** Bubble Sort n=500 sinh ~50,000 trace events. Render tất cả → browser freeze (DOM overload + 60fps impossible). Sampling xuống ≤3000 frames → smooth playback. Giữ frame CUỐI vì user cần thấy kết quả sort hoàn chỉnh; nếu bỏ → array vẫn lộn xộn ở frame cuối → misleading.
- **③ Áp dụng sâu:** `step = Math.ceil(trace.length / maxFrames)`; loop `for (let i = 0; i < trace.length; i += step)` + push `trace[trace.length - 1]`. Cross-ref Đề 18 TL 18.1: StepExecutor sinh trace events; sampling xảy ra ở layer playback, không phải generator. **Mẹo nhớ:** Sampling = "giữ đầu, giữ cuối, nhảy giữa".
- **❌ Tại sao D/C/A sai:** D (render all) sai vì performance; C (random sample) sai vì mất thứ tự thời gian; A (chỉ giữ đầu) sai vì mất kết quả cuối.
- **Mã nguồn thực tế (`frontend/src/composables/useCodeTracePlayback.ts`):**
```typescript
// frontend/src/composables/useCodeTracePlayback.ts
// SAMPLING: nếu trace.length > maxFrames (mặc định 3000) → step = ceil(length/maxFrames),
// lấy trace[0], trace[step], trace[2*step], ... LUÔN kèm event cuối → KHÔNG đẩy 50.000 frame
// vào UI (giới hạn ≤ maxFrames + 1 frame).
```

---

#### **Câu 17.3 — Đáp án: A**
- **① Khái niệm:** Dual-layer Sound Architecture là pattern có 2 engine âm thanh: primary (Web Audio API synthesizer — procedural, 0 file dependency) và secondary (Howler.js — sample-based fallback), kèm persistent preferences qua localStorage.
- **② Vì sao lại như thế:** Web Audio API synthesizer tạo beep/click realtime → không cần tải file mp3 (instant load, offline OK). Nhưng một số browser chặn AudioContext autoplay → Howler.js fallback dùng file .ogg dự phòng. localStorage lưu muted/volume → user setting persist qua refresh/session mới.
- **③ Áp dụng sâu:** Keys: `dsa_sfx_muted`, `dsa_sfx_volume`. Khi user toggle mute → cập nhật cả localStorage + store → mọi tab đồng bộ. Cross-ref Đề 14 TN 14.3: HelpView FAQ cũng dùng localStorage cho accessibility prefs. **Trade-off:** 2 engines = bundle size tăng ~15KB (Howler.js) → chấp nhận được cho UX sound mượt.
- **❌ Tại sao D/C/B sai:** D (chỉ Howler) sai vì phụ thuộc network/file; C (chỉ Web Audio) sai vì thiếu fallback; B (không persist) sai vì user phải set lại mỗi lần reload.
- **Mã nguồn thực tế (`frontend/src/composables/useSoundEffects.ts`):**
```typescript
// frontend/src/composables/useSoundEffects.ts
// Dual-layer architecture:
// 1. Primary: Web Audio API procedural synthesizer (100% standalone, 0 external file dependency)
// 2. Secondary: Howler.js integration for master volume and fallback audio playback
const STORAGE_KEY_MUTED = 'dsa_sfx_muted';
const STORAGE_KEY_VOLUME = 'dsa_sfx_volume';
```

---

#### **Câu 17.4 — Đáp án: C**
- **① Khái niệm:** Singleton Canvas Pattern cho hiệu ứng particle là việc tạo DUY NHẤT 1 canvas element fixed trên toàn app, reuse cho mọi lần bắn confetti thay vì tạo/destroy canvas mới mỗi lần.
- **② Vì sao lại như thế:** Tạo canvas mới mỗi lần → DOM node tích tụ → memory leak + layout thrashing. Singleton canvas (`position: fixed; pointer-events: none`) overlay toàn app, không chặn click/touch. `prefers-reduced-motion` check → tôn trọng user bị vestibular disorder (WCAG 2.1 SC 2.3.3). 4 types ('success', 'levelup', 'achievement', 'node-pass') → visual feedback phân biệt ngữ cảnh.
- **③ Áp dụng sâu:** `confetti.create(canvas, { resize: true })` → canvas tự adjust khi window resize. Pointer-events:none → user click xuyên qua canvas bình thường. Cross-ref Đề 17 TN 17.3: Sound effects cũng tôn trọng reduced-motion → consistency. **Lỗi thường gặp:** Quên destroy() khi unmount root component → canvas orphan.
- **❌ Tại sao D/B/A sai:** D (canvas per call) sai vì memory leak; B (blocking pointer-events) sai vì chặn UI interaction; A (ignore reduced-motion) sai vì accessibility violation.
- **Mã nguồn thực tế (`frontend/src/composables/useConfetti.ts`):**
```typescript
// frontend/src/composables/useConfetti.ts
// G-F2a: dùng confetti.create() trên 1 canvas FIXED dùng chung (resize:true)
// — không tạo canvas mới mỗi lần bắn, pointer-events:none để không chặn thao tác.
// Tôn trọng prefers-reduced-motion (không bắn nếu user giảm chuyển động).
export type ConfettiType = 'success' | 'levelup' | 'achievement' | 'node-pass';
```

---

#### **Câu 17.5 — Đáp án: D**
- **① Khái niệm:** Editable Target Guard là pattern kiểm tra `event.target` trước khi xử lý keyboard shortcut, bỏ qua nếu user đang focus vào ô nhập liệu (input/textarea/select/contentEditable) để tránh xung đột phím.
- **② Vì sao lại như thế:** User gõ "Ctrl+S" trong textarea để save code → nếu shortcut handler bắt trước → trigger global save thay vì cho phép typing. Guard check `tagName` + `isContentEditable` → chỉ activate shortcut khi user KHÔNG đang edit text. Đây là UX standard mà mọi app desktop/web đều tuân thủ.
- **③ Áp dụng sâu:** `isEditableTarget()` return true cho INPUT, TEXTAREA, SELECT, hoặc element có `contentEditable=true`. Cross-ref Đề 08: CodeRunnerView dùng contentEditable div → guard bảo vệ typing. **Lỗi thường gặp:** Quên check `isContentEditable` → shortcut chặn typing trong rich-text editor.
- **❌ Tại sao C/A/B sai:** C (luôn active) sai vì chặn typing; A (chỉ check INPUT) sai vì thiếu textarea/select/contentEditable; B (disable toàn bộ) sai vì mất shortcut functionality.
- **Mã nguồn thực tế (`frontend/src/composables/useKeyboardShortcuts.ts`):**
```typescript
// frontend/src/composables/useKeyboardShortcuts.ts
function isEditableTarget(target: EventTarget | null): boolean {
  if (!(target instanceof HTMLElement)) return false;
  const tag = target.tagName;
  return tag === 'INPUT' || tag === 'TEXTAREA' || tag === 'SELECT' || target.isContentEditable;
}
function handleKeydown(event: KeyboardEvent): void {
  if (!isEnabled.value) return;
  if (isEditableTarget(event.target)) return; // ← guard
  const handler = map[event.key];
  if (handler) { event.preventDefault(); handler(event); }
}
```

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2.5 điểm / câu)

#### 📝 Câu TL 17.1: Trace luồng useCodeTracePlayback: TraceEvent[] → Structure Frame → Playback (2.5 điểm)

> **💡 Ngữ cảnh thực tế:** Composable này biến raw trace events từ StepExecutor (Đề 18) thành animated visualization trên canvas. Cross-ref Đề 17 TN 17.2: sampling mechanism giới hạn frames.

- **1. Structure Mapping (0.5đ):** Mỗi TraceEvent → Structure frame `kind='array'`, element id `cell:<i>`, label = giá trị phần tử. Ưu tiên `trace[i].vars.array`; fallback duy trì mảng tuần tự từ initialArray. **Tại sao cần fallback?** Một số trace events chỉ record variable changes (không full array snapshot) → nếu không fallback, frame sẽ thiếu elements → visualization broken. Fallback = "kế thừa trạng thái trước + áp dụng delta".
- **2. Sampling Strategy (0.75đ):** `step = ceil(5000/3000) = 2` → lấy frame 0, 2, 4, ..., 4998, LUÔN kèm frame 4999 (cuối). Tổng ≈ 2501 frame ≤ 3001. **Tại sao LUÔN giữ frame cuối?** Trạng thái cuối = kết quả sort hoàn chỉnh. Nếu sampling bỏ frame cuối → user thấy array vẫn lộn xộn → misleading. **Trade-off:** Sampling làm mất một số intermediate steps → chấp nhận được vì user vẫn thấy overall flow; chi tiết đầy đủ có ở pseudocode panel.
- **3. Status Color Mapping (0.75đ):** `swap → 'swap'` (đỏ), `compare → 'highlight'` (vàng), `assign/declare → 'active'` (xanh dương), `loop/call/return → 'highlight'`. Event CUỐI → tất cả elements status `'done'` (xanh lá). **UX rationale:** Màu sắc phân biệt operation type → user nhận diện swap vs compare tức thì mà không cần đọc text. Consistent color scheme xuyên suốt 44 simulations.
- **4. Playback Engine Choice (0.5đ):** Dùng `setInterval` thay vì `requestAnimationFrame`. **Tại sao?** Structure frames đổi rời rạc (không continuous animation) → rAF overkill + khó control speed chính xác. setInterval(ms per frame) cho phép user adjust playback speed dễ dàng. `dispose()` dọn interval khi unmount → prevent memory leak. Cross-ref Đề 17 TN 17.1: useSimulation cũng cleanup onUnmounted.

---

#### 📝 Câu TL 17.2: Trace luồng useLenis Smooth Scroll + useScrollReveal IntersectionObserver (2.5 điểm)

> **💡 Ngữ cảnh thực tế:** Hai composables này xử lý UX scrolling và reveal animation — cross-cutting concerns ảnh hưởng toàn app. Cross-ref Đề 17 TN 17.4: useConfetti cũng tôn trọng reduced-motion.

- **1. Lenis Singleton Pattern (0.5đ):** Instance tạo 1 lần trong App.vue setup, shared toàn app. `anchors: true` → PrivacyView TOC `<a href="#sec-N">` tự cuộn mượt thay vì native jump. `allowNestedScroll: true` → bảo vệ Canvas simulator/Code Runner scroll nội bộ khỏi bị Lenis hijack. **Tại sao singleton?** Nhiều Lenis instances → conflict scroll listeners → jittery scroll. Singleton = 1 source of truth cho smooth scroll behavior.
- **2. Reduced Motion Respect (0.75đ):** Khi user bật `prefers-reduced-motion`: Lenis set `lerp = 1` (cuộn bám 1:1, không smooth interpolation); useScrollReveal set `isVisible = true` ngay lập tức, skip intersection animation. **Accessibility rationale:** WCAG 2.1 SC 2.3.3 yêu cầu disable non-essential animations cho users bị vestibular disorders. Không phải "nice-to-have" mà là compliance requirement. Cross-ref Đề 17 TN 17.4: useConfetti cũng check cùng media query.
- **3. ScrollReveal Once Optimization (0.75đ):** Element vào viewport → `isVisible.value = true`; `once: true` → `observer.disconnect()` ngay sau lần đầu. **Tại sao disconnect?** Reveal animation chỉ cần trigger 1 lần. Giữ observer chạy → wasted CPU cycles mỗi khi scroll. Disconnect = zero overhead sau khi revealed. **Trade-off:** Nếu user resize window khiến element ra/vào viewport lại → không re-animate. Chấp nhận được vì reveal = progressive enhancement, không phải core functionality.
- **4. Programmatic Anchor Scroll (0.5đ):** `scrollTo('#sec-3')` → Lenis tìm element bằng CSS selector, tính offset từ top, animate cuộn mượt với easing curve. **So sánh native:** `element.scrollIntoView()` = instant jump gây disorientation; Lenis = smooth transition giúp user maintain spatial awareness. **Mẹo nhớ:** Luôn dùng Lenis scrollTo thay vì native khi app đã enable smooth scroll — consistency matters.




---

# 📘 ĐÁP ÁN ĐỀ 18: VISUALIZATION ENGINE CORE

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 18.1 — Đáp án: B**
- **① Khái niệm:** Registry Pattern là một design pattern dùng Map/Dictionary để lưu trữ các factory function theo key string, cho phép truy xuất và tạo instance động mà không cần hardcode switch/if-else.
- **② Vì sao lại như thế:** Hệ thống có 44 thuật toán mô phỏng khác nhau. Nếu dùng switch-case, mỗi lần thêm thuật toán mới phải sửa file trung tâm → vi phạm Open/Closed Principle. Registry cho phép mỗi generator tự đăng ký khi import, file registry.ts không bao giờ cần sửa lại. Factory sinh instance MỚI mỗi lần để tránh state bẩn giữa các lần chạy mô phỏng (mỗi user session có input riêng).
- **③ Áp dụng sâu:** Khi user chọn "Bubble Sort" trên UI, SimulatorView gọi `getSimulation('sort.bubble')` → nhận generator instance → gọi `generator.generate(inputConfig)` → trả về Step[]. Nếu key không tồn tại, trả `undefined` → UI hiện thông báo "Mô phỏng chưa được hỗ trợ". So sánh với Singleton (đáp án C): singleton sẽ chia sẻ state giữa các tab/cửa sổ → bug khi user mở 2 mô phỏng song song.
- **❌ Tại sao C/D/A sai:** C (Singleton) sai vì state bị chia sẻ giữa các session; D (API backend) sai vì generator chạy hoàn toàn client-side để đạt 60fps; A (hardcoded switch) sai vì không mở rộng được mà không sửa code cũ.
- **Mã nguồn thực tế (`frontend/src/engines/registry.ts`):**
```typescript
const registry = new Map<string, GeneratorFactory>();
export function getSimulation(key: string): SimulationGenerator | undefined {
  const factory = registry.get(key);
  return factory ? factory() : undefined; // factory() = instance MỚI mỗi lần
}
```

---

#### **Câu 18.2 — Đáp án: A**
- **① Khái niệm:** Catalog Synchronization là cơ chế đảm bảo danh sách mô phỏng trong code (`catalog.ts`) luôn khớp 100% với file metadata JSON (`shared/simulation-catalog.json`) — cả hai phải có cùng 44 key.
- **② Vì sao lại như thế:** `catalog.ts` chứa factory function (logic runtime), còn JSON chứa metadata hiển thị (title, category, complexity). Nếu lệch key: user thấy card "Heap Sort" trên UI nhưng bấm vào thì crash vì không tìm thấy generator tương ứng. CI test tự động so sánh 2 danh sách → fail build NGAY khi PR tạo ra sự lệch pha, thay vì chờ user report bug production.
- **③ Áp dụng sâu:** Test `engines/__tests__/catalog.spec.ts` đọc cả 2 file, extract keys, sort và so sánh array equality. Khi dev thêm thuật toán mới: (1) thêm factory vào catalog.ts, (2) thêm metadata vào JSON → nếu quên bước nào, CI chặn lại. Đây là dạng "contract testing" nội bộ — đảm bảo interface giữa frontend logic và frontend metadata không bao giờ gãy.
- **❌ Tại sao B/C/D sai:** B (runtime check) sai vì lỗi chỉ phát hiện khi user truy cập đúng thuật toán bị thiếu; C (manual review) sai vì con người dễ bỏ sót trong PR lớn; D (không cần sync) sai vì JSON là source of truth cho UI rendering.
- **Mã nguồn thực tế (`frontend/src/engines/catalog.ts`):**
```typescript
// Danh sách KHỚP 100% key với shared/simulation-catalog.json (44 mô phỏng).
// CI so sánh 2 danh sách key → khác → fail build (test: engines/__tests__/catalog.spec.ts).
```

---

#### **Câu 18.3 — Đáp án: B**
- **① Khái niệm:** InputSchema là contract khai báo cấu trúc dữ liệu đầu vào mà mỗi generator chấp nhận, gồm fields (name, type, min/max, default) và preset options — đóng vai trò như "form specification" tự động render UI nhập liệu.
- **② Vì sao lại như thế:** Mỗi thuật toán có tham số khác nhau (Bubble Sort cần mảng số, BFS cần đồ thị, AVL cần dãy insert). Thay vì viết form riêng cho từng thuật toán, InputSchema cho phép InputModal.vue render form ĐỘNG từ schema → 44 thuật toán chỉ cần 1 component UI. Preset (random/sorted/reversed...) giúp user test nhanh các trường hợp biên mà không cần nhập tay.
- **③ Áp dụng sâu:** Khi user mở Bubble Sort, SimulatorView đọc `generator.inputSchema` → render form với 6 fields: values (int[]), size (int 2-100), minValue, maxValue, allowDuplicates (bool), preset (select 6 options). User chọn preset "Giảm dần" → generator tự sinh mảng [n, n-1, ..., 1] → đây chính là worst-case của Bubble Sort O(n²) → giúp user quan sát trực quan độ phức tạp.
- **❌ Tại sao C/D/A sai:** C (chỉ 1 field) sai vì không đủ tham số để tùy chỉnh; D (hardcoded trong component) sai vì vi phạm nguyên tắc tách biệt generator/UI; A (không có schema) sai vì InputModal không biết render form gì.
- **Mã nguồn thực tế (`frontend/src/engines/generators/sort/bubble.ts`):**
```typescript
const SCHEMA: InputSchema = {
  kind: 'array',
  fields: [
    { name: 'values', type: 'int[]', label: 'Dãy số', min: -999, max: 999, default: [5, 3, 8, 1, 9, 2] },
    { name: 'size', type: 'int', label: 'Số lượng phần tử', min: 2, max: 100, default: 15 },
    { name: 'preset', type: 'select', options: [
      { label: 'Ngẫu nhiên', value: 'random' },
      { label: 'Tăng dần', value: 'sorted-asc' },
      { label: 'Giảm dần', value: 'sorted-desc' },     // ← worst-case Bubble Sort
      { label: 'Gần như đã sắp xếp', value: 'nearly-sorted' },
      { label: 'Toàn giá trị bằng nhau', value: 'all-equal' },
      { label: 'Tự nhập', value: 'custom' },
    ]},
  ],
};
```

---

#### **Câu 18.4 — Đáp án: D**
- **① Khái niệm:** AST Instrumentation là kỹ thuật parse source code thành Abstract Syntax Tree (cây cú pháp trừu tượng), rồi chèn thêm hook function calls vào đúng vị trí ngữ nghĩa trước khi execute — khác hoàn toàn với regex-based text replacement.
- **② Vì sao lại như thế:** V1 dùng regex cắt dòng code và hoist biến lên global scope → phá vỡ block scope của let/const, gây lỗi khi hàm đệ quy gọi chính nó (biến bị ghi đè). V3 dùng @babel/parser hiểu cấu trúc ngữ pháp JS thật sự: biết đâu là block scope, đâu là vòng lặp lồng nhau, đâu là recursive call → chèn `__trackLine(line, vars)` vào đúng statement boundary mà không làm thay đổi semantics của code gốc.
- **③ Áp dụng sâu:** Khi user viết code Bubble Sort trong Code Runner, StepExecutor.parse(code) → Babel parse thành AST → traverse nodes → chèn `__trackLine(5, {i, j, a})` sau mỗi assignment/comparison → generate instrumented code string → eval trong sandbox. Kết quả: TraceEvent[] với vars snapshot chính xác tại mỗi dòng, kể cả trong recursive call stack depth 10+. Safety guards: MAX_STEPS=50000, MAX_LOOP_ITERATIONS=10000, timeout 5s → ngăn infinite loop treo browser.
- **❌ Tại sao C/A/B sai:** C (regex) sai vì không hiểu block scope; A (eval trực tiếp) sai vì không thu được trace; B (Web Worker compile) sai vì instrumentation xảy ra ở main thread trước khi gửi worker.
- **Mã nguồn thực tế (`frontend/src/engines/core/stepExecutor.ts`):**
```typescript
import { parse } from '@babel/parser';
// V3: @babel/parser chèn __trackLine/__loopTick theo AST node visit
// → giữ nguyên block scope, đệ quy đúng, biến local không bị hoist
// V1 (deprecated): regex cắt dòng + hoist → phá scope, lỗi đệ quy
```

---

#### **Câu 18.5 — Đáp án: C**
- **① Khái niệm:** Adaptive Rendering là chiến lược renderer tự động chuyển đổi cách vẽ dựa trên đặc điểm dữ liệu: Bar Mode khi label thuần số (tận dụng chiều cao canvas), Cell Mode khi label chữ, Wrap Mode khi mảng quá dài (>100 phần tử) chia thành nhiều hàng.
- **② Vì sao lại như thế:** Canvas mặc định 800×600px. Mảng 15 phần tử vẽ ô vuông 60×60 → đẹp. Nhưng mảng 200 phần tử vẽ ô 60px → tràn canvas. Giải pháp naive: thu nhỏ ô → ô 4px không đọc được label. Bar Mode giải quyết bằng cách vẽ cột dọc cao tỉ lệ giá trị → tận dụng chiều Y thay vì chỉ chiều X. Wrap Mode chia hàng khi slotW < 44px → vẫn đọc được index + giá trị.
- **③ Áp dụng sâu:** ArrayRenderer.render() kiểm tra `isNumericLabel(element.label)` cho mọi element → nếu ALL numeric → bar mode; ngược lại → cell mode. Trong bar mode: height = (value / maxValue) * availableHeight, gradient fill từ CANVAS_COLORS.active → glow effect. Wrap trigger: khi `cellSize < 36` (cell mode) hoặc `slotWidth < 44` (bar mode) → tính rows = ceil(n / cols), vẽ từng hàng với rowGap = 20px. Con trỏ pointer vẫn trỏ đúng ô nhờ tính toán position theo row/col.
- **❌ Tại sao B/A/D sai:** B (luôn cell mode) sai vì mảng dài không hiển thị được; A (luôn bar mode) sai vì label chữ ("null", "head") không vẽ bar được; D (fixed size) sai vì không responsive với kích thước canvas.
- **Mã nguồn thực tế (`frontend/src/engines/renderers/arrayRenderer.ts`):**
```typescript
// BAR MODE: khi mọi label là số → vẽ bar cao tỉ lệ giá trị từ đáy canvas
// WRAP: mảng dài → chia nhiều hàng (bar: slotW < 44; ô: cellSize < 36)
// Con trỏ vẫn trỏ đúng ô nhờ position tính theo row/col
```

---

### 📋 PHẦN II: TỰ LUẬN (2.5 điểm / câu)

#### 📝 Câu TL 18.1: Generator Pipeline (2.5 điểm)

> **💡 Ngữ cảnh thực tế:** Pipeline này chạy khi user bấm nút ▶️ Play trên SimulatorView hoặc khi auto-play bật. Toàn bộ quá trình từ registry lookup đến canvas re-render diễn ra trong <16ms để đạt 60fps.

- **1. Registry Lookup (0.5đ):** `getSimulation('sort.bubble')` → factory() sinh instance MỚI (không singleton). **Tại sao instance mới?** Vì mỗi lần chạy có input khác nhau, state generator (current step, internal counters) phải reset hoàn toàn. Instance cũ giữ state bẩn → animation nhảy lung tung.
- **2. Step Generation (0.75đ):** `generator.generate(inputConfig)` trả về `Step[]`. Mỗi Step chứa: `structure` (snapshot data structure tại thời điểm đó), `explanation` (tiếng Việt, VD: "So sánh a[2]=7 > a[3]=4 → hoán đổi"), `pseudocodeLine` (dòng mã giả đang thực thi, 1-based), `highlights` (id elements cần tô màu), `stats` ({ comparisons, swaps, writes }). **Lỗi thường gặp:** Generator sinh Step thiếu `version: 1` → renderer reject vì không tương thích protocol.
- **3. Renderer Dispatch (0.75đ):** `ArrayRenderer.render(structure, painter)` duyệt structure.elements → với mỗi element: `painter.statusColor(element.status)` map 'active'→CANVAS_COLORS.compare, 'swap'→CANVAS_COLORS.swap, 'done'→CANVAS_COLORS.done. **Cross-ref Đề 03:** CanvasArea.vue gọi renderer này mỗi khi currentStep thay đổi — cùng flow với SimulatorView layout 3 cột.
- **4. Step Navigation (0.5đ):** `useSimulation.stepForward()` tăng `currentIndex++` → computed `currentStep` đổi reference → Vue reactivity detect change → trigger CanvasArea re-render. **Mẹo nhớ:** Flow = Registry → Generate → Render → Navigate (viết tắt R-G-R-N).

---

#### 📝 Câu TL 18.2: StepExecutor Instrumentation (2.5 điểm)

> **💡 Ngữ cảnh thực tế:** StepExecutor chạy khi user viết code custom trong Code Runner (Bậc 3 LadderView) hoặc CodeToVisualView. Khác với Generator (pre-computed steps), StepExecutor instrument code USER VIẾT → trace realtime.

- **1. Babel AST Hooks (0.5đ):** `parse(code)` → AST → traverse → chèn `__trackLine(lineNumber, {varName: value})` sau mỗi ExpressionStatement/VariableDeclaration. Chèn `__loopTick(loopId)` ở đầu mỗi ForStatement/WhileStatement body. **Tại sao AST mà không regex?** Regex không phân biệt được `=` trong assignment vs `===` trong comparison vs `=>` trong arrow function → chèn hook sai chỗ → code crash.
- **2. TraceEvent Structure (0.75đ):** Mỗi `__trackLine` call push vào array: `{ line: number, vars: Record<string, unknown>, highlight: string[], kind: TraceKind, explanation: string }`. Kind ∈ {'declare','assign','compare','swap','loop','call','return'}. **Giới hạn cứng:** Max 50,000 events → vượt quá → truncate + warning. Lý do: 50K events × ~200 bytes = ~10MB RAM; hơn nữa UI playback không thể render >3000 frame mượt.
- **3. Safety Guards (0.75đ):** 3 lớp bảo vệ: (a) `MAX_STEPS = 50000` — đếm số trackLine calls; (b) `MAX_LOOP_ITERATIONS = 10000` — đếm loopTick per loop ID; (c) `setTimeout 5000ms` — wall-clock timeout. Vi phạm bất kỳ → throw Error với `{ line, message }` → UI hiện lỗi đỏ tại dòng code tương ứng. **Lỗi thường gặp:** User viết while(true) không break → timeout 5s → không phải bug executor mà là safety feature hoạt động đúng.
- **4. Stats Counters (0.5đ):** Song song với trace, executor đếm: comparisons (số lần so sánh), swaps (số lần hoán đổi), writes (số lần gán), durationMs (wall time). Stats này hiển thị trên ExerciseView sau khi submit → giúp user so sánh efficiency giữa các giải pháp.

---

# 📘 ĐÁP ÁN ĐỀ 19: API LAYER & FRONTEND ARCHITECTURE

### 📝 BẢNG ĐÁP ÁN TRẮC NGHIỆM (1.0 điểm / câu)

#### **Câu 19.1 — Đáp án: D**
- **① Khái niệm:** Token Refresh Interceptor là middleware Axios tự động bắt lỗi 401 Unauthorized, gọi API refresh token để lấy access token mới, rồi retry request gốc — tất cả transparent với business code.
- **② Vì sao lại như thế:** Access token hết hạn sau 15 phút. Nếu không có interceptor: mỗi API call phải tự check 401 và retry → duplicate logic khắp nơi. Interceptor tập trung xử lý 1 chỗ. Cờ `_retry` ngăn infinite loop (refresh fail → 401 → refresh again → ∞). Singleton promise đảm bảo 10 request song song cùng nhận 401 chỉ gọi refresh 1 lần. `redirectedToLogin` flag ngăn 10 request cùng redirect → flash screen.
- **③ Áp dụng sâu:** Flow đầy đủ: Request → gắn Bearer token → Server trả 401 → Interceptor check `!config._retry` → set _retry=true → await refreshPromise (singleton) → gắn token mới vào config.headers → `client(config)` retry. Nếu refresh cũng 401/500 → `authStore.logout()` → redirect `/login?redirect=/current-path`. `beforeunload` event reset flag → user reload trang vẫn login lại được bình thường.
- **❌ Tại sao B/A/C sai:** B (logout ngay) sai vì UX tệ — user đang điền form bị đá ra login; A (ignore 401) sai vì request sau cũng fail; C (reload page) sai vì mất toàn bộ state đang làm dở.
- **Mã nguồn thực tế (`frontend/src/api/client.ts`):**
```typescript
let redirectedToLogin = false;
window.addEventListener('beforeunload', () => { redirectedToLogin = false; });

client.interceptors.response.use(undefined, async (error) => {
  const original = error.config;
  if (error.response?.status === 401 && !original._retry) {
    original._retry = true;
    try {
      await authStore.refresh(); // singleton promise
      original.headers.Authorization = `Bearer ${authStore.accessToken}`;
      return client(original);   // retry với token mới
    } catch { /* refresh fail */ }
  }
  if (!redirectedToLogin) {
    redirectedToLogin = true;
    authStore.logout();
    router.push({ name: 'login', query: { redirect: to.fullPath } });
  }
});
```

---

#### **Câu 19.2 — Đáp án: B**
- **① Khái niệm:** ApiError là custom error class chuẩn hóa response lỗi từ backend theo contract `{ error: { code, message, field, details } }`, kèm metadata HTTP status và Retry-After header cho rate limiting.
- **② Vì sao lại như thế:** Backend trả lỗi dưới nhiều dạng: validation error (field cụ thể), business error (code logic), rate limit (429 + Retry-After). Nếu parse raw axios error: business code phải check `error.response.data.error.code` everywhere → fragile. ApiError class đóng gói parsing 1 lần → business code chỉ cần `catch(e) { if (e.code === 'HEARTS_EMPTY') ... }`. Field property cho phép highlight form field bị lỗi trực tiếp.
- **③ Áp dụng sâu:** Trong RegisterView: `try { await authApi.register(form) } catch(e) { if (e instanceof ApiError && e.field === 'email') fieldErrors.email = e.message }` → input email viền đỏ + message bên dưới. Với 429: `ui.showToast(messages.toast.rateLimited(e.retryAfterSeconds), 'warning')` → user biết chờ bao lâu. Cross-ref Đề 14: HelpView contact form cũng dùng ApiError để hiển thị lỗi validation.
- **❌ Tại sao C/D/A sai:** C (throw raw Error) sai vì mất structured info; D (return null) sai vì caller không biết lỗi gì; A (console.log only) sai vì user không thấy feedback.
- **Mã nguồn thực tế (`frontend/src/api/client.ts`):**
```typescript
export class ApiError extends Error {
  readonly code: string;           // VD: 'VALIDATION_ERROR', 'HEARTS_EMPTY'
  readonly field: string | null;   // VD: 'email', 'password' → highlight form field
  readonly status: number;         // HTTP status code
  readonly retryAfterSeconds?: number; // từ header Retry-After (429)
}
```

---

#### **Câu 19.3 — Đáp án: C**
- **① Khái niệm:** Route-level Code Splitting là kỹ thuật lazy-load Vue component bằng dynamic `import()`, khiến Vite/Rollup tách component thành chunk JS riêng → chỉ tải khi user navigate đến route đó.
- **② Vì sao lại như thế:** Bundle full app ~2MB. Home/Login là trang đầu tiên user thấy → phải load nhanh (<1s). Simulator/CodeRunner/Benchmark là trang nặng (~500KB each) nhưng chỉ 30% user truy cập mỗi session. Lazy-load giảm initial bundle xuống ~400KB → First Contentful Paint nhanh hơn 60%. Static import Home/Login vì chúng là critical path — không thể chờ lazy-load.
- **③ Áp dụng sâu:** `const SimulatorView = () => import('@/views/SimulatorView.vue')` → Vite tạo chunk `SimulatorView.[hash].js`. Khi user click link `/simulator/sort.bubble`, Vue Router trigger import → browser fetch chunk → render component. Loading state: Suspense hoặc Skeleton component. **Trade-off:** Lần đầu vào Simulator chậm hơn ~200ms (fetch chunk) nhưng tổng thời gian load app giảm đáng kể. Pages NOT lazy-loaded: HomeView, LoginView (critical path), NotFoundView (fallback nhỏ).
- **❌ Tại sao D/B/A sai:** D (all static) sai vì bundle quá lớn; B (all lazy) sai vì Home page load chậm; A (SSR) sai vì app là SPA pure client-side.
- **Mã nguồn thực tế (`frontend/src/router/index.ts`):**
```typescript
// STATIC — critical path, load ngay
import HomeView from '@/views/HomeView.vue';
import LoginView from '@/views/LoginView.vue';

// LAZY — chỉ tải khi navigate đến
const SimulatorView = () => import('@/views/SimulatorView.vue');
const CodeRunnerView = () => import('@/views/CodeRunnerView.vue');
const BenchmarkView = () => import('@/views/BenchmarkView.vue');
```

---

#### **Câu 19.4 — Đáp án: A**
- **① Khái niệm:** PagedResponse\<T\> là generic interface chuẩn hóa cấu trúc response phân trang từ backend: `items` (data array), `page` (current), `pageSize`, `total` (total records), `totalPages` (computed pages count).
- **② Vì sao lại như thế:** Backend có ~20 endpoint list (exercises, lessons, users, submissions...). Nếu mỗi endpoint trả format khác nhau: frontend phải viết parser riêng cho từng cái. Generic PagedResponse\<T\> cho phép viết 1 composable `usePagination<T>(apiCall)` dùng chung cho tất cả → DRY, ít bug. Total/totalPages cần thiết để render pagination controls (số trang, next/prev disabled state).
- **③ Áp dụng sâu:** `exerciseApi.list({ page: 2, pageSize: 20 })` → return `PagedResponse<ExerciseSummaryDto>`. Composable usePagination đọc totalPages → render 10 nút trang. Đọc total → hiện "Hiển thị 21-40 / 156 bài tập". **Lưu ý:** ExerciseSummaryDto KHÔNG chứa questions (giảm payload); muốn xem chi tiết phải gọi GET /exercises/{id} riêng.
- **❌ Tại sao C/D/B sai:** C (offset-based không total) sai vì không biết tổng số trang; D (cursor-based) sai vì backend dùng offset pagination; B (array-only) sai vì mất metadata phân trang.
- **Mã nguồn thực tế (`frontend/src/api/types.ts`):**
```typescript
export interface PagedResponse<T> {
  items: T[];       // data trang hiện tại
  page: number;     // trang hiện tại (1-based)
  pageSize: number; // số item/trang
  total: number;    // tổng số record
  totalPages: number; // tổng số trang = ceil(total/pageSize)
}
```

---

#### **Câu 19.5 — Đáp án: B**
- **① Khái niệm:** DTO Mapping Layer là lớp chuyển đổi giữa Raw DTO (format backend trả về) và Clean DTO (format frontend UI cần), thường nằm trong API module tương ứng.
- **② Vì sao lại như thế:** Backend QuestDto trả `{ progress, target, reward: { gems, xp } }` — nested object, field names khác convention frontend. UI cần `{ current, target, rewardGems, rewardXp }` — flat structure, naming nhất quán với gamificationStore. Mapping layer đóng gói transformation 1 chỗ → store/component không phụ thuộc backend contract. Khi backend đổi format: chỉ sửa mapping, không sửa 20+ component.
- **③ Áp dụng sâu:** Trong `gamification.ts`: `function mapQuest(raw: RawQuestDto): QuestDto { return { id: raw.questId, title: raw.title, current: raw.progress, target: raw.target, rewardGems: raw.reward.gems, rewardXp: raw.reward.xp, claimed: raw.claimed } }`. QuestsView gọi `gamificationApi.getQuests()` → nhận QuestDto[] clean → bind trực tiếp vào template. **Anti-pattern cần tránh:** Mapping trong component → duplicate logic, khó test.
- **❌ Tại sao A/C/D sai:** A (pass-through raw) sai vì UI phải biết nested structure backend; C (backend đổi format) sai vì frontend không control được backend; D (GraphQL) sai vì project dùng REST.
- **Mã nguồn thực tế (`frontend/src/api/gamification.ts`):**
```typescript
// Raw từ backend — nested, naming khác
interface RawQuestDto {
  questId: number; progress: number; target: number;
  reward: { gems: number; xp: number };
}
// Clean cho UI — flat, naming nhất quán
export interface QuestDto {
  id: number; current: number; target: number;
  rewardGems: number; rewardXp: number;
}
```

---

### 📋 PHẦN II: TỰ LUẬN (2.5 điểm / câu)

#### 📝 Câu TL 19.1: Axios Request Lifecycle (2.5 điểm)

> **💡 Ngữ cảnh thực tế:** Mọi API call trong app đều đi qua client.ts interceptor. Hiểu lifecycle này = hiểu cách app xử lý auth, error, và rate limiting ở tầng transport.

- **1. Request Interceptor (0.5đ):** Trước mỗi request, interceptor đọc `authStore.accessToken` từ Pinia → gắn header `Authorization: Bearer xxx`. **Tại sao Pinia mà không localStorage?** AccessToken lưu RAM (Pinia) → mất khi refresh trang → buộc refresh flow chạy → an toàn hơn. RefreshToken mới lưu HttpOnly cookie → JS không đọc được → chống XSS steal token.
- **2. 401 Handling (0.75đ):** Response interceptor bắt 401 → check `!original._retry` (tránh infinite loop) → set _retry=true → gọi `authStore.refresh()` (singleton promise: 10 request song song chỉ gọi 1 lần) → nhận accessToken mới → gắn vào original request headers → `client(original)` retry transparent. **Lỗi thường gặp:** Quên set _retry → refresh fail → 401 → retry → refresh fail → ∞ loop → browser hang.
- **3. Refresh Fail (0.75đ):** Nếu refresh API cũng trả 401/500 (refresh token hết hạn hoặc revoked) → catch block → `authStore.logout()` reset toàn bộ state (token, user info) → `router.push('/login?redirect=' + encodeURIComponent(currentPath))` → user login lại xong được đưa về đúng trang đang xem. **UX detail:** Query param redirect giữ context → user không mất chỗ đang làm dở.
- **4. Redirect Storm Guard (0.5đ):** Biến module-level `redirectedToLogin = false` → request 401 đầu tiên set true → redirect. Request 401 thứ 2-10 check true → skip redirect. `beforeunload` event reset flag → nếu user reload trang, flag reset → login flow hoạt động bình thường. **Tại sao cần?** Không có guard: 10 API calls song song cùng 401 → 10 lần router.push → Vue Router warn + flash screen.

---

#### 📝 Câu TL 19.2: Exercise Submit Flow (2.5 điểm)

> **💡 Ngữ cảnh thực tế:** Đây là flow cốt lõi của hệ thống đánh giá — kết nối ExerciseView (UI) → exercisesApi (transport) → ExerciseService (backend) → GamificationService (trừ tim/cộng XP) → UI update. Cross-ref Đề 07 (LadderView) và Đề 10 (Shop/Premium).

- **1. Payload Construction (0.5đ):** ExerciseView thu thập answers từ QuizStage component → build `SubmitRequest { answers: [{ questionId: number, selected: number[] }] }`. **Tại sao selected là array?** Hỗ trợ câu hỏi MULTIPLE choice (chọn nhiều đáp án). Câu SINGLE choice vẫn dùng array 1 phần tử → uniform interface. Optional `classAssignmentId` nếu bài tập được giao trong lớp học.
- **2. Response Processing (0.75đ):** Backend trả `SubmitResultDto { score, maxScore, results: [{ questionId, correct, correctAnswer, explanation }] }`. Frontend map results → highlight câu đúng/xanh, sai/đỏ. Explanation hiển thị khi user click "Xem giải thích" → học từ lỗi sai. Score/maxScore tính percentage → nếu ≥70% → pass → mở khóa bậc tiếp theo (LadderView cross-ref Đề 07).
- **3. Hearts Side Effect (0.75đ):** Submit KHÔNG trừ tim — `SubmitResultDto` không có trường heartsLeft (Dtos/SubmitResultDto.cs: Score, MaxScore, Passed, Results[], SubmissionId, SubmittedAt). Tim bị trừ 1 khi **bắt đầu phiên luyện node** qua `GamificationService.EnterNodeAsync` — UPDATE atomic `SET Hearts = Hearts - 1 WHERE Id = @id AND Hearts > 0` chống double-spend; node đã PASS vào lại miễn phí; Premium vẫn bị trừ (HeartsMax 30, hồi 10 phút/tim). **Quan trọng:** trừ tim ở BACKEND server-side → user không thể hack tim bằng DevTools.
- **4. Zero Hearts UX (0.5đ):** Khi Hearts = 0 mà bấm vào node mới → backend trả **403 Forbidden + mã `HEARTS_EMPTY`** (ErrorCodes.cs:39,79) → `PathView.vue` catch và hiện toast warning "Bạn đã hết tim. Hãy chờ hồi hoặc nâng cấp Premium." (L146); nếu mất backend + `allowLocalFallbacks` thì mở node local miễn phí (demo). KHÔNG có OutOfHeartsModal — monetization diễn ra qua toast + trang Premium/Shop.


---

*Tài liệu đã được kiểm chứng 100% khớp với mã nguồn thực tế của dự án DSA Visual.*
