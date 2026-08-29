# 🎯 NGÂN HÀNG ĐỀ THI KIỂM TRA HIỂU BIẾT MÃ NGUỒN DỰ ÁN DSA VISUAL
## Full-flow Assessment: UI $\rightarrow$ Pinia Store / API Client $\rightarrow$ ASP.NET Core $\rightarrow$ Database $\rightarrow$ UI Render

> **Dành cho:** Kiểm tra năng lực thành viên dự án, onboarding lập trình viên mới, review kiến trúc hệ thống.  
> **Quy chuẩn đề thi:** 19 Bộ đề chuyên sâu (Đề 01–13 theo màn hình nghiệp vụ, Đề 14–17 các view còn lại, Đề 18–19 lớp Engine/API) + 01 Bộ đề Tổng hợp Final Test.  
> **Cấu trúc mỗi đề chuyên sâu:** 5 câu Trắc nghiệm Thực chiến (5.0 điểm) + 2 câu Tự luận Trace Luồng (5.0 điểm). Thời gian làm bài: 25 – 30 phút / đề.  
> **Thang điểm:** 10.0 điểm / đề. Điểm đạt: $\ge 7.0$ điểm.

---

# 📚 KIẾN THỨC NỀN & BẢNG THUẬT NGỮ (cho người chưa biết gì về web stack)

> Mục này giúp người **chưa từng code web** vẫn làm được đề thi. Nếu bạn đã quen Vue / ASP.NET, có thể bỏ qua và đi thẳng [Mục lục](#-mục-lục-các-bộ-đề-thi).

## 🧭 A. Hệ thống 3 lớp của DSA Visual (hiểu trước khi làm đề)

DSA Visual là một hệ thống **full-stack** gồm 3 lớp chạy song song:

1. **Lớp Giao diện (Frontend / UI)** — cái người dùng thấy trên trình duyệt web. Viết bằng **Vue 3**, chạy 100% trong trình duyệt. Gọi API qua HTTP để lấy / lưu dữ liệu.
2. **Lớp Máy chủ (Backend / API)** — máy chủ viết bằng **ASP.NET Core (.NET 10)**. Nhận HTTP request từ frontend, xử lý logic, đọc / ghi cơ sở dữ liệu, trả JSON về.
3. **Cơ sở dữ liệu (Database)** — nơi lưu trường tồn (người dùng, bài học, điểm XP, số tim...). Backend thao tác với DB qua **Entity Framework Core**.

**Luồng điển hình:** Người dùng bấm nút trên UI → Vue component chạy hàm → Pinia store gọi `axios` → HTTP request tới backend → Controller nhận → Service xử lý + ghi DB → trả JSON → axios interceptor nhận → store cập nhật state → Vue render lại UI.

> Khi đề yêu cầu "trace luồng" (Câu Tự luận) — bạn phải kể chính xác chuỗi trên cho một tính năng cụ thể, có thể chỉ ra **file code thật** ở mỗi bước.

## 🌐 B. Thuật ngữ Web & HTTP

| Thuật ngữ | Giải thích ngắn (cho người mới) |
|---|---|
| **Client / Server** | Client = bên gởi yêu cầu (trình duyệt). Server = bên nhận và xử lý (máy chủ backend). |
| **HTTP / HTTPS** | Giao thức truyền tin web. HTTPS = có mã hoá. Mọi request của frontend tới backend đều là HTTP. |
| **HTTP Method** | Động từ chỉ mục đích: `GET` (đọc), `POST` (tạo mới / thực thi), `PUT` (cập nhật), `DELETE` (xoá). |
| **Endpoint / Route** | Một URL cụ thể trên API, VD `POST /api/v1/me/quests/5/claim`. |
| **Request body / JSON** | Dữ liệu kèm request, dạng JSON (text có cấu trúc key–value). VD `{"simKey":"sort.bubble"}`. |
| **Query string** | Tham số sau dấu `?` trong URL, VD `/leaderboard?tab=week`. |
| **Status code** | Mã kết quả HTTP: `2xx` OK · `400` lỗi input · `401` chưa login · `403` bị cấm (VD hết tim) · `404` không thấy · `409` xung đột (VD email tồn tại) · `500` lỗi server. |
| **API** | Tập hợp các endpoint mà backend công bố để frontend gọi. |
| **JWT (JSON Web Token)** | Một chuỗi chữ ký mã hoá, đóng vai trò "vé vào cổng" sau khi login. Frontend gửi kèm mỗi request qua header `Authorization: Bearer <token>`. |
| **Interceptor** | Hàm tự chạy trước / sau mỗi request của axios — dùng để đính token tự động và xử lý lỗi 401 (token hết hạn → gọi refresh). |
| **SSE / WebSocket** | Kênh server "đẩy" dữ liệu về trình duyệt (chỉ frontend phải hỏi lại). Dùng cho tiến trình chạy code chạy dài. |

## 🖼️ C. Thuật ngữ Frontend (Vue 3 + Pinia + Router + Axios)

| Thuật ngữ | Giải thích ngắn |
|---|---|
| **Vue 3 + Composition API** | Framework UI hiện đại. Composition API = cách viết component dùng `<script setup>`. |
| **Component / `.vue`** | Đơn vị UI dùng lại được. File `.vue` có 3 phần: `<template>` (HTML), `<script>` (TS), `<style>` (CSS). |
| **Props / Emit** | Props = dữ liệu cha truyền xuống con. Emit = con bắn sự kiện lên cha. |
| **Reactivity (`ref`, `reactive`, `computed`, `watch`)** | `ref(x)` biến giá trị thành "đáp ứng" — Vue tự render lại khi nó đổi. `computed` = giá trị dẫn xuất. `watch` = chạy hàm khi giá trị đổi. |
| **Pinia store** | Kho state toàn cục dùng cho cả app. Component nào cũng lấy / sửa được qua `useXStore()`. VD `useAuthStore()`, `useSimulationStore()`. |
| **Composable (`useX`)** | Hàm dùng lại chứa logic Vue (state + hàm), đặt tên `useX`. VD `useSimulation`, `useToast`. |
| **Vue Router / Route / Router Guard** | Bộ định tuyến: ánh xạ URL → component. Guard = hàm chặn kiểm tra trước khi vào route (VD: chặn người đã login vào `/login`). |
| **Route meta** | Trường nhãn dán tùy chọn trên route (VD `meta: { guestOnly: true }`) để guard đọc. |
| **Axios / API client** | Thư viện gọi HTTP thay cho `fetch`. Có interceptor, base URL, timeout. |
| **localStorage** | Kho lưu trong trình duyệt, tồn tại sau reload (chỉ client, backend không thấy). |
| **PixiJS / GSAP / ECharts** | Pixi = vẽ canvas 2D tốc độ cao (chạy visualization); GSAP = animation; ECharts = biểu đồ (bảng xếp hạng). |
| **Monaco Editor** | Trình soạn code như VS Code, dùng cho Sandbox / Code Runner. |
| **TailwindCSS / shadcn-vue** | Hệ thống CSS + bộ component UI sẵn. |

## ⚙️ D. Thuật ngữ Backend (.NET 10 / ASP.NET Core / EF Core)

| Thuật ngữ | Giải thích ngắn |
|---|---|
| **.NET 10 / ASP.NET Core** | Nền tảng chạy mã C#; ASP.NET Core = framework web của .NET. |
| **Controller** | Lớp nhận HTTP request, gọi Service, trả response. VD `AuthController`, `AdminController`. |
| **Action / Endpoint method** | Mỗi hàm public trong controller, gắn attribute `[HttpGet]`, `[HttpPost]`... là một endpoint. |
| **Service** | Lớp logic nghiệp vụ, được Controller gọi. VD `AuthService`, `GamificationService`, `ExerciseService`. |
| **DTO (Data Transfer Object)** | Lớp chỉ mang dữ liệu vào / ra API (không phải bảng DB). VD `SubmitResultDto`, `SubmitRequest`. |
| **EF Core / DbContext** | ORM: ánh xạ bảng DB ↔ class C#. DbContext là "cổng" thao tác DB. |
| **Migration** | Tập lệnh sinh ra để tạo / sửa bảng DB khi model C# đổi. |
| **Middleware / Pipeline** | Chuỗi bước xử lý mỗi request (log, auth, routing, response). |
| **JWT Bearer auth** | Cơ chế ASP.NET đọc header `Authorization: Bearer...`, kiểm chữ ký, tạo `ClaimsPrincipal` (biết user là ai). |
| **ApiVersioning (`api/v1`)** | Version hoá API trong URL để đổi API sau này mà không hỏng client cũ. |
| **DI (Dependency Injection)** | Cách .NET cấp phát service vào constructor — thay vì tự `new`. |
| **Jint** | Trình thông dịch JS viết bằng C#; backend dùng để chạy code người dùng nộp trong Sandbox ở chế độ cô lập (timeout 1500ms, giới hạn 32MB). |

## 🎮 E. Thuật ngữ riêng của DSA Visual (domain terms)

Đây là những từ **chỉ DSA Visual mới có** — hiểu chúng là chìa khoá giải đề (đã đối chiếu mã nguồn):

| Thuật ngữ | Giải thích ngắn (đã verify trong code) |
|---|---|
| **DSA** | Data Structures & Algorithms — Cấu trúc dữ liệu & Giải thuật (cây, đồ thị, sắp xếp, tìm kiếm...). Toàn bộ web xoay quanh việc mô phỏng / trực quan hoá chúng. |
| **Visualization Engine** | Trái tim frontend: chạy từng bước thuật toán, phát ra mảng `Step[]` để render animation. Gồm Generator (sinh steps) + Renderer (vẽ) + Step Executor (chạy step). |
| **Step** | Một "khung hình" của animation: `{ index, structure, explanation, pseudocodeLine, highlights, annotations, variables, stats, version: 1 }`. |
| **Sandbox** | Trò chơi tương tác thử thuật toán tay (4 khu: Sorting / Searching / Graph / Stack–Queue). |
| **Code Runner / Code Lab** | Nơi người dùng viết code thật (Monaco editor) → submit → backend Jint chấm → trả kết quả. |
| **Hearts (Tim)** | Mạng chơi. **Free:** tối đa 10 tim, hồi 1 tim / 30 phút. **Premium:** tối đa 30 tim, hồi 1 tim / 10 phút. **Trừ 1 tim khi ENTER một node (bài học) mới**; node đã PASS vào lại miễn phí. Submit quiz **không trừ tim**. Hết tim vào node mới → `403 HEARTS_EMPTY`. |
| **XP / Level** | Điểm kinh nghiệm. `Level = 1 + floor(sqrt(xp / 100))`: 100 XP → L2, 400 XP → L3. |
| **Quest** | Nhiệm vụ hằng ngày (2 Easy + 2 Medium + 1 Hard). Claim qua `POST /api/v1/me/quests/{id}/claim`. Reset 00:00 giờ Việt Nam (UTC+7). |
| **Ladder (Thang thực hành)** | Chuỗi bài tập tăng dần; mỗi bài có 3 bậc theo `sandboxType`: Lý thuyết → Quiz → Code Lab. |
| **Gems / Shop / Premium** | Gems = tiền ảo mua avatar / frame. Premium = gói trả phí (tim nhiều hơn, hồi nhanh hơn — nhưng **vẫn bị trừ tim như thường**). Thanh toán qua VietQR mock, mã `DSV{userId}T{months}`. |
| **Node / Path** | Bài học trong lộ trình là "node"; người dùng đi theo "path". Vào node mới tốn tim. |
| **Course / Lesson** | Khoá học / bài học. Hoàn thành bài học → award-xp + sync progress về server. |

> **Quy ước đọc đề:** câu trắc nghiệm có 4 đáp án A / B / C / D (chỉ một đúng). Ký hiệu `→` trong câu tự luận nghĩa là "kết quả / bước kế tiếp". Tên file kèm path (VD `stores/auth.ts`) là file thật trong repo — hãy mở ra đối chiếu nếu cần. Khi đề ghi "trace luồng", hãy đi theo luồng 3 lớp ở mục A.

---

# 📑 MỤC LỤC CÁC BỘ ĐỀ THI

- [📚 Kiến thức nền & Bảng thuật ngữ — dành cho người mới bắt đầu](#-kiến-thức-nền--bảng-thuật-ngữ-cho-người-chưa-biết-gì-về-web-stack)
- [📘 ĐỀ 01: Phân hệ Xác thực & Phiên làm việc (Auth, Session & Axios Interceptor)](#-đề-01-phân-hệ-xác-thực--phiên-làm-việc)
- [📘 ĐỀ 02: Trang chủ, Khám phá Demo & Điều hướng Hệ thống (HomeView & Router)](#-đề-02-trang-chủ-khám-phá-demo--điều-hướng-hệ-thống)
- [📘 ĐỀ 03: Mô phỏng Thuật toán & Trực quan hóa Engine (SimulatorView & Controls)](#-đề-03-mô-phỏng-thuật-toán--trực-quan-hóa-engine)
- [📘 ĐỀ 04: Màn hình Khám phá Mô phỏng, Tra cứu & So sánh (SimulationsView, CheatSheet & Benchmark)](#-đề-04-màn-hình-khám-phá-mô-phỏng-tra-cứu--so-sánh)
- [📘 ĐỀ 05: Sandbox Tương tác 4 Tab (Sorting, Searching, Graph, Stack & Queue)](#-đề-05-sandbox-tương-tác-4-tab)
- [📘 ĐỀ 06: Lộ trình Học tập & Học Bài học (CoursesList, CourseDetail & LessonStudy)](#-đề-06-lộ-trình-học-tập--học-bài-học)
- [📘 ĐỀ 07: Thang Thực hành 3 Bậc & Trắc nghiệm Chấm điểm (LadderView, LabView & ExerciseView)](#-đề-07-thang-thực-hành-3-bậc--trắc-nghiệm-chấm-điểm)
- [📘 ĐỀ 08: Trình chạy Code Sandbox & Chấm điểm Server (CodeRunnerView & Judge Service)](#-đề-08-trình-chạy-code-sandbox--chấm-điểm-server)
- [📘 ĐỀ 09: Hệ thống Gamification: Nhiệm vụ & Bảng xếp hạng (QuestsView & LeaderboardView)](#-đề-09-hệ-thống-gamification-nhiệm-vụ--bảng-xếp-hạng)
- [📘 ĐỀ 10: Cửa hàng Gems & Gói Hội viên Premium (ShopView, PremiumView & SubscriptionView)](#-đề-10-cửa-hàng-gems--gói-hội-viên-premium)
- [📘 ĐỀ 11: Hồ sơ Cá nhân, Thành tích & Cài đặt (ProfileView 5 Tabs)](#-đề-11-hồ-sơ-cá-nhân-thành-tích--cài-đặt)
- [📘 ĐỀ 12: Hệ thống Lớp học & Studio Giảng viên (ClassesView, ClassReport & Curriculum Studio)](#-đề-12-hệ-thống-lớp-học--studio-giảng-viên)
- [📘 ĐỀ 13: Bảng điều khiển Quản trị Hệ thống (AdminUsers, AdminStats & AdminSettings)](#-đề-13-bảng-điều-khiển-quản-trị-hệ-thống)
- [📘 ĐỀ 14: Trợ giúp, Chính sách Bảo mật & Phản hồi Người dùng (HelpView & PrivacyView)](#-đề-14-trợ-giúp-chính-sách-bảo-mật--phản-hồi-người-dùng)
- [📘 ĐỀ 15: Studio Giảng viên & Quản lý Nội dung Học tập (TeacherStudio & NodeHub)](#-đề-15-studio-giảng-viên--quản-lý-nội-dung-học-tập)
- [📘 ĐỀ 16: Kiểm tra Cuối Lộ trình & Điều hướng Thông minh (FinalTestView & PathRedirectView)](#-đề-16-kiểm-tra-cuối-lộ-trình--điều-hướng-thông-minh)
- [📘 ĐỀ 17: Composables, Hiệu ứng & Cross-cutting Concerns](#-đề-17-composables-hiệu-ứng--cross-cutting-concerns)
- [📘 ĐỀ 18: Visualization Engine Core (Generator, Renderer & Step Executor)](#-đề-18-visualization-engine-core-generator-renderer--step-executor)
- [📘 ĐỀ 19: API Layer, Router & Frontend Architecture](#-đề-19-api-layer-router--frontend-architecture)
- [🏆 ĐỀ FINAL: Bài thi Đánh giá Năng lực Toàn Hệ thống (Integration End-to-End Test)](#-đề-final-bài-thi-đánh-giá-năng-lực-toàn-hệ-thống)

---

# 📘 ĐỀ 01: PHÂN HỆ XÁC THỰC & PHIÊN LÀM VIỆC
**Màn hình & Files trọng tâm:** `LoginView.vue` · `RegisterView.vue` · `ForgotPasswordView.vue` · `ResetPasswordView.vue` · `stores/auth.ts` · `api/client.ts` · `AuthController.cs` · `AuthService.cs`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 1.1 (Vào trang & Khởi tạo):**  
Khi một người dùng ĐÃ ĐĂNG NHẬP (`authStore.isAuthenticated === true`) gõ URL `/login` hoặc `/register`, Router Guard trong `router/index.ts` sẽ xử lý như thế nào?
- **A.** Hiển thị popup bắt buộc người dùng bấm "Đăng xuất" trước khi tải trang.
- **B.** Chuyển hướng người dùng về trang 404 `NotFoundView`.
- **C.** Cho phép vào trang bình thường để đăng nhập tài khoản khác.
- **D.** Route có cờ `meta: { guestOnly: true }` nên Router Guard tự động chuyển hướng người dùng về trang chủ `/` (route name `home`).

**Câu 1.2 (Validation Client-side Form Đăng ký):**  
Trên màn hình `RegisterView.vue`, khi người dùng nhập mật khẩu `abc123` và ô xác nhận mật khẩu `abc1234`, sự kiện validation diễn ra như thế nào?
- **A.** Checklist `passwordRules` đánh dấu đỏ các tiêu chí chưa đạt (thiếu ký tự hoa, ký tự đặc biệt, độ dài < 8), `fieldErrors.confirmPassword` nhận thông báo lỗi từ `messages.register.confirmError` và chặn hàm `onSubmit()` không gửi API.
- **B.** Form gửi ngay `POST /api/v1/auth/register` lên Backend và chờ Backend báo lỗi mật khẩu không khớp.
- **C.** Ô xác nhận mật khẩu tự động xóa trắng ký tự thừa để khớp với ô mật khẩu.
- **D.** Trình duyệt tự sinh mật khẩu mới và thay thế vào form.

**Câu 1.3 (Lưu trữ Token & Bảo mật Session):**  
Theo quy chuẩn kiến trúc ADR-004 của dự án, khi API `POST /api/v1/auth/login` trả về thành công:
- **A.** `accessToken` và `refreshToken` đều được lưu vào `localStorage` của trình duyệt.
- **B.** `accessToken` được nhúng vào URL query string của mọi trang web.
- **C.** `accessToken` chỉ được lưu trong bộ nhớ RAM của Pinia Store (`auth.ts`), còn `refreshToken` được Backend gán tự động vào Cookie `HttpOnly; SameSite=Strict; Secure; Path=/api/v1/auth`.
- **D.** Token được mã hóa và lưu vào `sessionStorage`.

**Câu 1.4 (Khôi phục Phiên khi F5 & Chống bão Refresh):**  
Khi người dùng nhấn F5 trên trình duyệt hoặc có 3 request API đồng thời trả về mã lỗi `HTTP 401 Unauthorized`:
- **A.** Cả 3 request đều gửi đồng thời 3 request `/auth/refresh` lên máy chủ.
- **B.** `authStore.refresh()` sử dụng cơ chế **Singleton Promise** (`refreshPromise`), chỉ gửi duy nhất 1 request `POST /api/v1/auth/refresh` đọc Cookie HttpOnly; khi có token mới thì cả 3 request gốc được gắn header mới và retry thành công.
- **C.** Trình duyệt hiện hộp thoại yêu cầu người dùng nhập lại mật khẩu.
- **D.** Hệ thống đăng xuất ngay lập tức và đưa người dùng về trang `/login`.

**Câu 1.5 (Đăng xuất sạch sẽ - State Cleansing):**  
Khi người dùng bấm nút "Đăng xuất" (`authStore.logout()`), những store nào sau đây trong hệ thống sẽ được nạp động và kích hoạt hàm `.reset()`?
- **A.** Chỉ xóa state của `useAuthStore`.
- **B.** Đồng loạt 7 stores cá nhân: `gamification`, `progress`, `lesson`, `classStore`, `leaderboard`, `codeRunner`, `simulation`.
- **C.** Chỉ reset `gamificationStore` và `progressStore`.
- **D.** Không reset store nào mà chỉ chuyển hướng về `/login`.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 1.1 (Trace luồng xử lý lỗi Server khi Đăng ký trùng Email – 2.5 điểm):**  
Giả sử người dùng điền đầy đủ form đăng ký tại `RegisterView.vue` hợp lệ ở Client, nhưng Email này đã tồn tại trong CSDL. Hãy trace chi tiết 4 chặng:
1. **[UI Submit]**: Component thu thập dữ liệu gì, gọi action nào của `authStore`?
2. **[Backend Validation & DB]**: `AuthController.Register` $\rightarrow$ `AuthService.RegisterAsync` kiểm tra bảng `Users` ra sao? Trả về mã lỗi HTTP mấy và định dạng JSON lỗi `ApiErrorBody` như thế nào?
3. **[Axios Interceptor]**: File `client.ts` bắt mã lỗi này và ném ra `ApiError` ra sao?
4. **[UI Error Render]**: `RegisterView.vue` bắt `catch (err)` gán vào biến reactive nào và hiển thị banner cảnh báo đỏ trên form ra sao?

*Khu vực làm bài TL 1.1:*
```text
...
```

**Câu TL 1.2 (Trace luồng Quên mật khẩu & Đặt lại mật khẩu – 2.5 điểm):**  
Người dùng vào `/forgot-password`, nhập email và bấm "Gửi liên kết khôi phục", sau đó bấm vào link đính kèm token trong email để vào `/reset-password`. Hãy phân tích:
1. Endpoint và Method HTTP mà `ForgotPasswordView.vue` gọi xuống Backend? Backend `AuthService` sinh reset token, thời hạn bao lâu và gửi qua cổng SMTP nào?
2. Tại `ResetPasswordView.vue`, component đọc `token` và `email` từ đâu trên URL?
3. Khi bấm "Cập nhật mật khẩu mới", payload gửi đến `POST /api/v1/auth/reset-password` gồm những gì?
4. Khi thành công, UI hiển thị thông báo gì và tự động điều hướng người dùng về màn hình nào?

*Khu vực làm bài TL 1.2:*
```text
...
```

---

# 📘 ĐỀ 02: TRANG CHỦ, KHÁM PHÁ DEMO & ĐIỀU HƯỚNG HỆ THỐNG
**Màn hình & Files trọng tâm:** `HomeView.vue` · `NotFoundView.vue` · `router/index.ts` · `engines/catalog.ts`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 2.1 (Khởi tạo Trang chủ & Demo công khai):**  
Tại màn hình `HomeView.vue` (dành cho khách vãng lai chưa đăng nhập), hệ thống cung cấp sẵn các thẻ mô phỏng Demo công khai (`demoAllowed: true`). Đó là các thuật toán nào?
- **A.** A* Pathfinding, AVL Tree, Dynamic Programming Knapsack.
- **B.** Toàn bộ 40+ thuật toán trong hệ thống đều mở công khai.
- **C.** Bubble Sort (`sort.bubble`), Binary Search (`search.binary`), Graph BFS (`graph.bfs`).
- **D.** Quick Sort, Dijkstra, Red-Black Tree.

**Câu 2.2 (Hiệu ứng Cuộn mượt & Directive trên HomeView):**  
Trên `HomeView.vue`, các section như Hero, Tính năng, Bảng so sánh được gắn directive tùy biến `v-reveal`. Directive này hoạt động theo cơ chế nào?
- **A.** Sử dụng `IntersectionObserver` để kích hoạt class animation CSS khi phần tử lọt vào khung nhìn; tự động bỏ qua animation nếu người dùng bật chế độ `prefers-reduced-motion`.
- **B.** Luôn luôn tải lại toàn bộ DOM sau mỗi 2 giây.
- **C.** Bắt buộc dùng thư viện Flash Player để render hiệu ứng.
- **D.** Gọi API đo tọa độ scroll từ server.

**Câu 2.3 (Tương tác Click Demo trên HomeView):**  
Khi khách chưa đăng nhập click vào nút "Thử nghiệm ngay" trên Card Bubble Sort ở Trang chủ:
- **A.** Chuyển hướng sang kênh Youtube hướng dẫn thuật toán.
- **B.** Modal bắt buộc đăng nhập hiện lên chặn lại.
- **C.** Tải file mã nguồn `.zip` về máy tính của khách.
- **D.** Router điều hướng trực tiếp sang `/simulator/sort.bubble` và mở trình mô phỏng chạy bình thường mà không bị chặn auth guard.

**Câu 2.4 (Router Guard & Deep Link Bảo vệ):**  
Khi một người dùng CHƯA ĐĂNG NHẬP click vào đường link `/profile` hoặc `/classes/10`:
- **A.** Tự động đăng nhập với tư cách Guest có quyền xem toàn bộ dữ liệu.
- **B.** Hiển thị trang lỗi 500 Server Error.
- **C.** Router Guard chặn lại, lưu URL gốc vào query param và chuyển hướng về `/login?redirect=%2Fprofile` hoặc `/login?redirect=%2Fclasses%2F10`.
- **D.** Router hiển thị màn hình trắng xóa.

**Câu 2.5 (Xử lý Trang không tồn tại - NotFoundView):**  
Khi người dùng truy cập một URL không tồn tại trong hệ thống (ví dụ: `/duong-dan-khong-hop-le`):
- **A.** Tự động chuyển hướng về trang `/login`.
- **B.** Router bắt bằng route wildcard `{ path: '/:pathMatch(.*)*', component: NotFoundView }` hiển thị giao diện 404 kèm nút "Về trang chủ" (`/`).
- **C.** Server trả về trang trắng của Nginx.
- **D.** Trình duyệt tự crash và đóng ứng dụng.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 2.1 (Trace luồng Khách trải nghiệm Demo $\rightarrow$ Đăng ký $\rightarrow$ Học bài thật – 2.5 điểm):**  
Hãy phân tích hành trình trải nghiệm người dùng từ Trang chủ:
1. Khách vào `/` $\rightarrow$ bấm "Xem Demo Bubble Sort" $\rightarrow$ `/simulator/sort.bubble` hoạt động ra sao (kiểm tra `isDemoKey`)?
2. Sau khi xem demo, khách bấm nút "Tạo tài khoản để mở khóa toàn bộ" trên banner `DemoBanner.vue` $\rightarrow$ Điều hướng về `/register`.
3. Khách hoàn thành đăng ký $\rightarrow$ Hệ thống tự cấp phiên $\rightarrow$ Điều hướng vào `/path` (Lộ trình).
4. Tại `/path`, dữ liệu khóa học được tải về như thế nào?

*Khu vực làm bài TL 2.1:*
```text
...
```

**Câu TL 2.2 (Trace luồng Phân quyền Route Guard theo Vai trò Role – 2.5 điểm):**  
Giả sử tài khoản sinh viên có role `STUDENT` cố tình nhập URL quản trị `/admin/users` hoặc `/admin/stats` trên thanh địa chỉ trình duyệt:
1. Route Meta của `/admin/users` được cấu hình những trường gì trong `router/index.ts`?
2. Hàm `router.beforeEach` kiểm tra những điều kiện gì (`requiresAuth`, `roles`)?
3. Khi phát hiện role `STUDENT` không nằm trong danh sách `['ADMIN']`, Router xử lý chặn và chuyển hướng người dùng về đâu? Có thông báo lỗi Toast gì không?

*Khu vực làm bài TL 2.2:*
```text
...
```

---

# 📘 ĐỀ 03: MÔ PHỎNG THUẬT TOÁN & TRỰC QUAN HÓA ENGINE
**Màn hình & Files trọng tâm:** `SimulatorView.vue` · `ControlBar.vue` · `CanvasArea.vue` · `PseudocodePanel.vue` · `ExplainPanel.vue` · `InputModal.vue` · `composables/useSimulation.ts` · `FavoritesController.cs`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 3.1 (Bố cục Giao diện Desktop của SimulatorView):**  
Giao diện màn hình mô phỏng thuật toán trên Desktop (màn hình rộng $\ge 1024px$) được bố trí theo hệ thống lưới (Grid) 12 cột chuẩn nào?
- **A.** Cột trái: Video bài giảng (6/12) · Cột phải: Khung vẽ Canvas (6/12).
- **B.** Toàn màn hình là Canvas, không có mã giả và giải thích.
- **C.** 4 cột đều nhau (3 - 3 - 3 - 3).
- **D.** Cột trái: Mã giả `PseudocodePanel` (3/12) · Cột giữa: Khung vẽ `CanvasArea` (6/12) · Cột phải: Giải thích `ExplainPanel` (3/12) cùng thanh điều khiển `ControlBar` bên dưới.

**Câu 3.2 (Thao tác Nút Play & Tốc độ trên ControlBar):**  
Khi người dùng bấm nút **Play** ($\blacktriangleright$) trên thanh điều khiển `ControlBar.vue`:
- **A.** Composable `useSimulation` chuyển `status = 'running'`, kích hoạt bộ đếm thời gian (timer theo `speed`) $\rightarrow$ tăng `currentIndex` $\rightarrow$ `CanvasArea` vẽ lại state $\rightarrow$ `PseudocodePanel` highlight dòng tương ứng $\rightarrow$ `ExplainPanel` cập nhật câu giải thích.
- **B.** Phát một video MP4 đã quay sẵn từ máy chủ.
- **C.** Trình duyệt gửi request HTTP liên tục lên server để nhận từng ảnh PNG về vẽ.
- **D.** Trình duyệt tự tải lại trang sau mỗi bước chạy.

**Câu 3.3 (Đổi dữ liệu đầu vào qua InputModal):**  
Khi người dùng bấm nút "Cấu hình dữ liệu", nhập mảng `[12, 3, 7, 1]` vào `InputModal.vue` và bấm "Áp dụng":
- **A.** Hàm `configureInput([12, 3, 7, 1])` được gọi $\rightarrow$ Engine nạp mảng mới vào hàm generator của thuật toán $\rightarrow$ Sinh lại toàn bộ mảng `steps` mới $\rightarrow$ Đặt `currentIndex = 0` và vẽ lại Canvas ở trạng thái bắt đầu.
- **B.** Mảng mới được lưu vào CSDL trên máy chủ qua API.
- **C.** Thuật toán tự động đổi sang Quick Sort.
- **D.** Trừ của người dùng 1 Tim vì thay đổi dữ liệu mặc định.

**Câu 3.4 (Thao tác Breakpoint trên Panel Mã giả):**  
Khi người dùng click chuột vào số dòng (line gutter) trên `PseudocodePanel.vue`:
- **A.** Dòng code đó bị xóa khỏi thuật toán.
- **B.** Điểm dừng (Breakpoint) được bật/tắt trên dòng đó; khi bấm Play, thuật toán sẽ tự động tạm dừng (Pause) ngay khi bước chạy chạm đến dòng code có breakpoint.
- **C.** Chuyển ngôn ngữ mã giả từ Tiếng Việt sang Tiếng Anh.
- **D.** Mở cửa sổ soạn thảo code mới.

**Câu 3.5 (Lưu Thuật toán Yêu thích):**  
Khi người dùng bấm vào biểu tượng ngôi sao "⭐ Yêu thích" trên Header của Simulator:
- **A.** Mở modal bắt buộc đánh giá 5 sao cho ứng dụng.
- **B.** Icon đổi màu cục bộ nhưng mất đi khi F5.
- **C.** Gọi API `POST /api/v1/favorites` với body `{ key: "sort.bubble" }` $\rightarrow$ Backend `FavoritesController` lưu vào bảng `Favorites` $\rightarrow$ Trả về 200 OK $\rightarrow$ Icon chuyển sang màu vàng sáng.
- **D.** Tự động chia sẻ bài học lên trang Facebook cá nhân.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 3.1 (Trace luồng Điều khiển Playback Từng bước trên Canvas – 2.5 điểm):**  
Người dùng đang mở bài mô phỏng Selection Sort:
1. Mảng khởi tạo ban đầu được sinh từ file nào trong `engines/`? Cấu trúc của một phần tử `Step` gồm những thuộc tính gì?
2. Khi người dùng bấm nút **Step Forward** ($\blacktriangleright\mid$): Hàm nào trong `useSimulation.ts` được gọi? Thuộc tính nào thay đổi?
3. Canvas nhận props gì từ `currentStep` để tô màu các cột đang so sánh (Comparing), hoán đổi (Swapping) hay đã sắp xếp xong (Sorted)?
4. `PseudocodePanel` căn cứ vào trường nào của `currentStep` để dịch chuyển dòng sáng highlight?

*Khu vực làm bài TL 3.1:*
```text
...
```

**Câu TL 3.2 (Trace luồng Tự thực hành - Manual Practice Mode – 2.5 điểm):**  
Trên màn hình Simulator có tính năng "Tự thực hành" (`ManualPracticePanel.vue`):
1. Khi người dùng bật toggle "Chế độ tự thực hành", giao diện thay đổi như thế nào?
2. Khi hệ thống yêu cầu "Hãy chọn 2 phần tử cần hoán đổi tiếp theo", người dùng click chọn 2 cột trên Canvas $\rightarrow$ Sự kiện click trên Canvas truyền dữ liệu gì về component cha?
3. Nếu người dùng chọn đúng vs chọn sai $\rightarrow$ Hiệu ứng giao diện, âm thanh/thông báo và điểm số thực hành phản hồi ra sao?

*Khu vực làm bài TL 3.2:*
```text
...
```

---

# 📘 ĐỀ 04: MÀN HÌNH KHÁM PHÁ MÔ PHỎNG, TRA CỨU & SO SÁNH
**Màn hình & Files trọng tâm:** `SimulationsView.vue` · `CheatSheetView.vue` · `BenchmarkView.vue` · `engines/catalog.ts` · `data/referenceLinks.ts`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 4.1 (Kiến trúc Điều hướng Khám phá Mô phỏng):**  
Hệ thống Khám phá Mô phỏng gồm các màn hình/route riêng biệt nào được cấu hình trong `router/index.ts`?
- **A.** Tất cả tính năng khám phá được nhúng trực tiếp vào trang chủ `/` dưới dạng widget cuộn dọc.
- **B.** Một trang duy nhất `/simulations` chứa toàn bộ danh mục, cheatsheet và benchmark dưới dạng tab chuyển đổi nội bộ.
- **C.** Hai route: `/explore` (danh sách thuật toán) và `/lab` (chạy benchmark), không có trang cheatsheet riêng.
- **D.** Ba route độc lập: `/simulations` (Danh mục mô phỏng — `SimulationsView.vue`) · `/cheatsheet` (Bảng tra cứu Big-O — `CheatSheetView.vue`) · `/benchmark/:k1/:k2` (So sánh hiệu năng — `BenchmarkView.vue`).

**Câu 4.2 (Phân nhóm & Tìm kiếm trên Danh mục Catalog):**  
Khi người dùng gõ từ khóa `"cây"` hoặc chọn chip lọc danh mục `"Cây & Đồ thị"` trên `SimulationsView.vue`:
- **A.** Lọc trực tiếp trên mảng catalog metadata tại Client (`engines/catalog.ts`), hiển thị các thuật toán có category hoặc title khớp điều kiện (ví dụ: BST, AVL, Segment Tree, Trie).
- **B.** Gửi request API `GET /api/v1/simulations/search?q=cay` lên server.
- **C.** Tự động mở bài học đầu tiên trong danh mục.
- **D.** Xóa toàn bộ các thuật toán khác khỏi bộ nhớ máy tính.

**Câu 4.3 (Hàm `complexityTone` Phân loại Màu Chip Big-O):**  
Trong `SimulationsView.vue`, hàm `complexityTone(value)` xác định variant màu cho chip độ phức tạp theo quy tắc nào?
- **A.** Mọi giá trị Big-O đều trả về `'success'` bất kể nội dung chuỗi.
- **B.** Dựa trên số lượng ký tự trong chuỗi Big-O: ngắn hơn 6 ký tự là xanh, dài hơn là đỏ.
- **C.** Đọc từ biến CSS `--complexity-color` do người dùng tùy chỉnh trong trang Cài đặt.
- **D.** Chuỗi chứa `'³'`, `'n^3'` hoặc `'2^n'` → `'danger'`; chứa `'²'` hoặc `'n^2'` → `'warning'`; còn lại → `'success'`.

**Câu 4.4 (Phòng thí nghiệm So sánh Hiệu năng - BenchmarkView):**  
Tại màn hình `/benchmark/:k1/:k2` (ví dụ: so sánh `sort.bubble` và `sort.quick`):
- **A.** Hệ thống chạy giả lập video so sánh 2 thuật toán.
- **B.** Chạy benchmark trên máy chủ backend làm nghẽn CPU server.
- **C.** Hệ thống sinh cùng 1 mảng dữ liệu ngẫu nhiên với kích thước n (10 đến 1000 phần tử), thực thi 2 thuật toán song song ở Client Web Worker, đo thời gian thực thi (ms), số phép so sánh, số phép hoán đổi và vẽ biểu đồ cột đối chiếu trực quan.
- **D.** Bắt buộc trừ 10 Gems cho mỗi lần chạy so sánh.

**Câu 4.5 (Nguồn dữ liệu & Liên kết Tham khảo trong CheatSheetView):**  
Bảng tra cứu độ phức tạp tại `/cheatsheet` (`CheatSheetView.vue` + `CheatSheetTable.vue`) sử dụng nguồn dữ liệu và liên kết tham khảo nào?
- **A.** Gọi API `GET /api/v1/cheatsheet` từ backend mỗi lần mở trang để lấy danh sách thuật toán mới nhất.
- **B.** Import trực tiếp mảng `CATALOG` từ `@/engines/catalog` và hàm `getReference(key)` từ `@/data/referenceLinks` để hiển thị bảng Big-O kèm liên kết Wikipedia/GeeksforGeeks theo từng `simKey`, hoàn toàn tĩnh phía Client.
- **C.** Crawl tự động từ Wikipedia và GeeksforGeeks khi build rồi cache vào localStorage.
- **D.** Người dùng tự thêm thuật toán vào bảng thông qua form nhập liệu trên giao diện.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 4.1 (Trace luồng Lọc danh mục $\rightarrow$ Xem chi tiết $\rightarrow$ Mở Simulator – 2.5 điểm):**  
Người dùng truy cập `/simulations`:
1. Khi component `SimulationsView.vue` được mount, dữ liệu danh mục được khởi tạo ra sao? Biến reactive nào quản lý từ khóa tìm kiếm (`searchQuery`) và danh mục đang chọn (`selectedCategory`)?
2. Khi người dùng click vào Card "Quick Sort", router chuyển hướng sang đâu?
3. Thao tác này có yêu cầu đăng nhập không nếu Quick Sort không phải là Demo Key? Router Guard xử lý thế nào?

*Khu vực làm bài TL 4.1:*
```text
...
```

**Câu TL 4.2 (Trace luồng Thực thi So sánh Benchmark giữa 2 Thuật toán – 2.5 điểm):**  
Tại `BenchmarkView.vue` khi so sánh Bubble Sort vs Merge Sort:
1. Hai tham số `:k1` và `:k2` được đọc từ đâu trong Vue Router?
2. Khi người dùng chọn kích thước `n = 500`, kiểu phân bố dữ liệu "Ngược hoàn toàn" (Reversed) và bấm nút **"Bắt đầu Benchmark"**: Quá trình sinh dữ liệu và chạy thuật toán diễn ra ở đâu?
3. Các chỉ số thống kê (`durationMs`, `comparisons`, `swaps`) được tổng hợp và render lên biểu đồ như thế nào?

*Khu vực làm bài TL 4.2:*
```text
...
```

---

# 📘 ĐỀ 05: SANDBOX TƯƠNG TÁC 4 TAB
**Màn hình & Files trọng tâm:** `views/sorting/SortingView.vue` · `SearchingView.vue` · `GraphView.vue` · `StackQueueView.vue` · `CodeToVisualView.vue` · `router/index.ts`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 5.1 (Kiến trúc Dùng chung Component cho 4 Route Sandbox):**  
Trong `router/index.ts`, 4 route `/sorting-sandbox`, `/searching-sandbox`, `/graph-playground` và `/stack-queue-sandbox` được cấu hình như thế nào?
- **A.** Cả 4 route đều trỏ chung vào component `SortingView.vue` (trang Sandbox 4 tab tích hợp); `App.vue` sử dụng `:key="$route.fullPath"` để tự động mở đúng tab tương ứng khi người dùng chuyển đổi route.
- **B.** Các route này đều redirect về trang chủ.
- **C.** Mỗi route trỏ đến một file component hoàn toàn độc lập và không liên quan gì nhau.
- **D.** Chạy bằng iframe nhúng từ một website bên thứ 3.

**Câu 5.2 (Thao tác Thêm Node/Edge trên Graph Playground):**  
Trong tab Đồ thị (`GraphView.vue` / `/graph-playground`), khi người dùng click chuột lên vùng trống để tạo đỉnh mới hoặc kéo nối 2 đỉnh để tạo cạnh có trọng số:
- **A.** Dữ liệu đồ thị được gửi lên server qua `POST /api/v1/graph/save`.
- **B.** Màn hình reload lại toàn bộ trang web.
- **C.** Bắt buộc phải viết code C++ mới tạo được đỉnh đồ thị.
- **D.** State ma trận kề / danh sách kề được cập nhật trực tiếp trong reactive state cục bộ ở Client và vẽ lại ngay lập tức trên canvas SVG tương tác.

**Câu 5.3 (Trực quan hóa Ngăn xếp & Hàng đợi Stack & Queue):**  
Trong tab `StackQueueView.vue`, khi người dùng bấm nút **Push (15)** vào Stack và sau đó bấm **Enqueue (20)** vào Queue:
- **A.** Dữ liệu bị xóa sau 3 giây.
- **B.** Canvas hiển thị animation phần tử 15 rơi từ trên xuống đáy Stack, và phần tử 20 xếp vào cuối hàng đợi Queue kèm con trỏ Front/Rear dịch chuyển.
- **C.** Chỉ hiển thị thông báo text dạng console log mà không có hình ảnh.
- **D.** Báo lỗi vì không thể chạy Stack và Queue cùng lúc.

**Câu 5.4 (DSL Code-to-Visual Playground):**  
Tại màn hình `/playground/code-to-visual` (`CodeToVisualView.vue`), người dùng nhập các lệnh dạng DSL: `create array [5, 2, 9]`, `swap(0, 1)`. Hệ thống xử lý như thế nào?
- **A.** Báo lỗi không nhận diện được cú pháp C#.
- **B.** Gửi code lên server biên dịch bằng GCC.
- **C.** Bộ parser ở Client phân tích cú pháp DSL $\rightarrow$ Chuyển thành các lệnh gọi Canvas Painter tương ứng $\rightarrow$ Hiển thị tức thì khung trực quan động bên cạnh.
- **D.** Trả về file video động dạng GIF để tải về máy.

**Câu 5.5 (Điều khiển Thuật toán trong Sorting Sandbox):**  
Khi đang ở `/sorting-sandbox`, người dùng chuyển đổi dropdown từ "Merge Sort" sang "Heap Sort":
- **A.** Component reset mảng về trạng thái ban đầu, nạp bộ mã giả và hàm generator của Heap Sort, vẽ lại cây nhị phân heap trên canvas mà không cần tải lại trang.
- **B.** Màn hình bị khóa trong 5 giây.
- **C.** Trình duyệt tải về file cài đặt thuật toán mới.
- **D.** Toàn bộ trang web bị reload và đăng xuất tài khoản.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 5.1 (Trace luồng Tương tác Thuật toán Tìm kiếm Nhị phân trong Searching Sandbox – 2.5 điểm):**  
Tại `/searching-sandbox`:
1. Người dùng nhập giá trị cần tìm `Target = 42` và bấm nút "Tìm kiếm".
2. Thuật toán Binary Search khởi tạo hai con trỏ `Low = 0`, `High = n - 1` như thế nào?
3. Tại mỗi bước lặp, vị trí `Mid` được tính toán và hiển thị trên Canvas ra sao?
4. Khi tìm thấy phần tử (hoặc không tìm thấy sau khi `Low > High`), thông báo kết quả và màu sắc cột kết thúc hiển thị như thế nào?

*Khu vực làm bài TL 5.1:*
```text
...
```

**Câu TL 5.2 (Trace luồng Duyệt Đồ thị BFS/DFS trên Graph Playground – 2.5 điểm):**  
Tại `/graph-playground`:
1. Người dùng vẽ một đồ thị gồm 5 đỉnh (0, 1, 2, 3, 4) và các cạnh nối.
2. Chọn đỉnh xuất phát là đỉnh `0` và bấm nút "Chạy thuật toán BFS (Breadth-First Search)".
3. Hàng đợi Queue hiển thị các đỉnh chờ duyệt ra sao?
4. Màu sắc của các đỉnh (Chưa thăm $\rightarrow$ Đang xét trong hàng đợi $\rightarrow$ Đã duyệt xong) và các cạnh được tô màu động trên canvas như thế nào?

*Khu vực làm bài TL 5.2:*
```text
...
```

---

# 📘 ĐỀ 06: LỘ TRÌNH HỌC TẬP & HỌC BÀI HỌC
**Màn hình & Files trọng tâm:** `CoursesListView.vue` · `CourseDetailView.vue` · `LessonStudyView.vue` · `LessonView.vue` · `stores/lesson.ts` · `LessonsController.cs` · `LessonService.cs`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 6.1 (Quyền Truy cập Trang Lộ trình CoursesListView):**  
Đường dẫn `/path` (`CoursesListView.vue`) được cấu hình quyền truy cập như thế nào trong `router/index.ts`?
- **A.** Bắt buộc phải đăng nhập mới xem được danh sách lộ trình (`requiresAuth: true`).
- **B.** Chỉ tài khoản Admin mới có quyền truy cập.
- **C.** Khách vãng lai được phép vào xem danh sách các khóa học công khai (`meta: { public: true }`); tuy nhiên khi click vào chi tiết khóa học hoặc bài học cụ thể thì mới yêu cầu đăng nhập.
- **D.** Trang này chỉ mở vào các ngày cuối tuần.

**Câu 6.2 (Nút "Tham gia Lộ trình" trên CourseDetailView):**  
Trên màn hình `/path/:id` (`CourseDetailView.vue`), khi học viên chưa đăng ký khóa học này:
- **A.** Khóa học bị ẩn toàn bộ danh sách bài học.
- **B.** Hiển thị nút nổi bật "Tham gia lộ trình"; khi click $\rightarrow$ `useCourseStore.enrollCourse(id)` lưu trạng thái vào `localStorage` key `enrolled_{id}` $\rightarrow$ Giao diện chuyển sang chế độ đã ghi danh và mở khóa danh sách bài học mà KHÔNG gửi request lên server.
- **C.** Bắt buộc phải thanh toán 500 Gems mới được tham gia.
- **D.** Gửi email cho giảng viên xin phép vào học.

**Câu 6.3 (Cấu trúc Màn học Bài LessonStudyView):**  
Giao diện màn hình học tập `/lessons/:id` (`LessonStudyView.vue`) được thiết kế với cấu trúc đa bước tuần tự gồm:
- **A.** 100 câu trắc nghiệm tính giờ không có lý thuyết.
- **B.** Trình chấm code tự động bắt buộc nộp bài mới được đọc lý thuyết.
- **C.** Xem video Livestream trực tiếp của giáo viên.
- **D.** Stepper 3 bước: (1) **Lý thuyết** — đọc Markdown, có nhúng sẵn visualizer mô phỏng theo `simulation-key` $\rightarrow$ (2) **Quiz** — mini quiz trắc nghiệm $\rightarrow$ (3) **Code Lab** — thực hành code nếu bài có `codelabTask` (bài dạng quiz chỉ hiện riêng tab Quiz); component render theo `sandboxType` và chuyển bước qua sự kiện `@completeStep` / `@completeLesson`.

**Câu 6.4 (Hoàn thành Bài học & Nhận thưởng):**  
Khi học viên hoàn thành đủ các step và bấm nút **"Hoàn thành bài học"** trên `LessonStudyView.vue`:
- **A.** Gọi API `POST /api/v1/lessons/{id}/complete` $\rightarrow$ Backend cộng ngay XP và Gems vào bảng `UserGamification`, mở popup chúc mừng kèm hiệu ứng pháo hoa Confetti.
- **B.** Chỉ chuyển sang bài tiếp theo mà không gửi bất kỳ request nào lên máy chủ; toàn bộ trạng thái chỉ nằm trong RAM.
- **C.** `lessonStore.markLessonCompleted(id)` đánh dấu hoàn thành cục bộ (localStorage `dsa.completedLessons`) và cộng XP qua `POST /concepts/auth/award-xp` với payload `{ amount, reason }`; sau đó `syncToServer` đẩy tiến độ qua `POST /concepts/auth/progress/{lessonId}` (kèm cờ `completed`) $\rightarrow$ hiện `LessonCompletionModal` với số XP nhận được.
- **D.** Hệ thống tự động trừ 1 Tim của học viên cho mỗi bài học hoàn thành.

**Câu 6.5 (Sự khác biệt giữa LessonStudyView và LessonView cũ):**  
Trong codebase, `LessonStudyView.vue` (tại `/lessons/:id`) và `LessonView.vue` (tại `/learn/:lessonId`):
- **A.** Cả hai đều không còn được sử dụng trong hệ thống.
- **B.** `LessonStudyView` dành cho giáo viên, `LessonView` dành cho học sinh.
- **C.** Là hai file hoàn toàn trùng nhau không có gì khác.
- **D.** `LessonStudyView.vue` là giao diện học tập thế hệ mới (Rich Theory Layout v4.0 có nhúng visualizer và mini quiz); còn `LessonView.vue` là view cũ được giữ lại để tương thích ngược với các deep link bài học trước đây.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 6.1 (Trace luồng Đăng ký Lộ trình $\rightarrow$ Mở khóa Bài học đầu tiên – 2.5 điểm):**  
Học viên vào xem chi tiết Lộ trình Cấu trúc Dữ liệu Cây tại `/path/tree`:
1. `CourseDetailView.vue` gọi API nào để lấy cây bài học và trạng thái từng bài (đã hoàn thành / mở khóa)?
2. Khi học viên bấm nút "Tham gia lộ trình" (Enroll): dữ liệu được ghi ở đâu và tại sao luồng này không cần gọi API backend?
3. Store `useCourseStore` cập nhật reactive state ra sao sau khi enroll?
4. Trạng thái nút bấm và danh sách bài học trên giao diện thay đổi thế nào cho tới khi vào được bài đầu tiên?

*Khu vực làm bài TL 6.1:*
```text
...
```

**Câu TL 6.2 (Trace luồng Hoàn thành Bài học $\rightarrow$ Cập nhật Tiến độ & XP – 2.5 điểm):**  
Tại màn hình học bài `/lessons/15`, học viên đọc xong lý thuyết, làm quiz xong và bấm hoàn thành:
1. `finishLesson()` trong `LessonStudyView.vue` gọi những action nào của `lessonStore` theo thứ tự thế nào?
2. Việc cộng XP đi qua API nào với payload ra sao, và tại sao cần kiểm tra `xpAwarded < totalXp` trước khi cộng (chống cộng trùng XP)?
3. `syncToServer` đẩy payload gì lên endpoint nào? Nếu đồng bộ thất bại (mất mạng), cơ chế fallback là gì?
4. `LessonCompletionModal` hiển thị thông tin gì và dữ liệu bài kế tiếp (`resolveNextLessonId`) được xác định ra sao?

*Khu vực làm bài TL 6.2:*
```text
...
```

---

# 📘 ĐỀ 07: THANG THỰC HÀNH 3 BẬC & TRẮC NGHIỆM CHẤM ĐIỂM
**Màn hình & Files trọng tâm:** `LadderView.vue` · `LabView.vue` · `ExerciseView.vue` · `ExercisesController.cs` · `ExerciseService.cs`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 7.1 (Mô hình 3 Bậc thang Thử thách Practice Ladder):**  
Màn hình Practice Ladder `/ladder/:nodeId` (`LadderView.vue`) được thiết kế với 3 bậc thử thách theo thứ tự và trọng số điểm nào?
- **A.** **Bậc 1: Quiz Trắc nghiệm** (Hiểu khái niệm & Big-O) $\rightarrow$ **Bậc 2: Interactive Lab** (Điền/kéo thả trạng thái trên Canvas) $\rightarrow$ **Bậc 3: Code Challenge** (Lập trình giải thuật thực tế với test cases).
- **B.** Dễ (30%) $\rightarrow$ Vừa (30%) $\rightarrow$ Khó (40%) đều là câu hỏi trắc nghiệm chữ.
- **C.** 3 bài thi viết trên giấy nộp cho giáo viên.
- **D.** Chỉ có 1 bậc duy nhất là làm bài thi trắc nghiệm 50 câu.

**Câu 7.2 (Điều kiện Mở khóa các Bậc tiếp theo):**  
Khi học viên mới vào màn hình `LadderView.vue` của một chủ đề bài học:
- **A.** Cả 3 bậc đều mở sẵn cho học viên chọn tùy ý.
- **B.** Bậc 1 (Quiz) mở sẵn; Bậc 2 (Lab) bị khóa (hiển thị ổ khóa xám) và chỉ mở ra khi học viên đạt điểm tối thiểu ở Bậc 1; Bậc 3 (Code) chỉ mở ra khi hoàn thành Bậc 2.
- **C.** Học viên phải trả 100 Gems để mở khóa Bậc 2 và Bậc 3.
- **D.** Giáo viên phải vào phê duyệt thủ công thì học viên mới được làm Bậc 2.

**Câu 7.3 (Nộp bài Trắc nghiệm sai & Cơ chế Trừ Tim):**  
Tại màn hình làm bài trắc nghiệm `/exercise/:id` (`ExerciseView.vue`), khi học viên chọn phương án sai và bấm "Nộp bài":
- **A.** Hệ thống bỏ qua và cho phép chọn lại không giới hạn.
- **B.** Backend `ExerciseService.SubmitAsync` chỉ chấm điểm và trả về `SubmitResultDto { score, maxScore, results[]: [{ questionId, correct, correctAnswer, explanation }] }`; việc trừ tim KHÔNG diễn ra ở endpoint này — tim đã bị trừ 1 (UPDATE atomic) khi học viên **bắt đầu phiên luyện node** (`EnterNodeAsync`), và vào lại node đã PASS thì miễn phí. Frontend highlight câu đúng/xanh sai/đỏ kèm giải thích.
- **C.** Tài khoản bị khóa 24 giờ.
- **D.** Trừ 50 Gems trong tài khoản của học viên.

**Câu 7.4 (Bậc 2 - Màn hình Interactive Lab LabView):**  
Màn hình Bậc 2 Interactive Lab `/ladder/:nodeId/lab` (`LabView.vue`) yêu cầu học viên làm gì?
- **A.** Chụp ảnh bài làm nộp lên hệ thống.
- **B.** Viết một bài luận 500 từ về cấu trúc dữ liệu.
- **C.** Xem một đoạn hoạt hình và bấm nút thích.
- **D.** Thao tác trực tiếp trên Canvas (kéo thả node cây, điền giá trị mảng tiếp theo sau 1 bước chạy) $\rightarrow$ Backend chấm điểm bằng cách so khớp trạng thái cuối (`STATE_MATCH`) trong `AnswerJson`.

**Câu 7.5 (Cơ chế Chống Nộp bài Trùng lặp - Submission Lock):**  
Trong `ExerciseService.cs`, hệ thống sử dụng cơ chế gì để ngăn chặn học viên spam click nút "Nộp bài" nhiều lần liên tục?
- **A.** Sử dụng `SubmissionLockRegistry` để khóa luồng xử lý theo từng cặp `(userId, exerciseId)`; nếu có request thứ 2 gửi đến khi request 1 đang xử lý sẽ bị từ chối ngay với mã lỗi `SUBMISSION_IN_PROGRESS`.
- **B.** Tắt kết nối internet của người dùng.
- **C.** Không có cơ chế bảo vệ nào.
- **D.** Tự động hủy bài nộp của học viên.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 7.1 (Trace luồng Làm bài Quiz $\rightarrow$ Server Chấm điểm $\rightarrow$ Trừ Tim khi làm sai – 2.5 điểm):**  
Học viên đang làm một câu trắc nghiệm tại `ExerciseView.vue`:
1. Khi học viên chọn phương án C và bấm "Nộp bài": Payload gửi lên `POST /api/v1/exercises/{id}/submit` gồm những gì?
2. Backend `ExerciseService` giải mã trường `AnswerJson` trong DB và so sánh với câu trả lời của học viên ra sao?
3. Nếu trả lời SAI: Quá trình trừ tim diễn ra trong bảng CSDL nào? Nếu số tim còn lại $= 0$, response trả về thông báo gì và UI hiển thị Modal cảnh báo hết tim ra sao?
4. Nếu trả lời ĐÚNG: Điểm số, XP được cộng thế nào và Bậc 2 (Lab) được mở khóa ra sao?

*Khu vực làm bài TL 7.1:*
```text
...
```

**Câu TL 7.2 (Trace luồng Hoàn thành Bậc 2 Lab $\rightarrow$ Mở khóa Bậc 3 Code Challenge – 2.5 điểm):**  
Tại `LabView.vue`:
1. Bài tập yêu cầu: "Hãy click chọn đỉnh tiếp theo trong cây AVL sau khi quay trái". Học viên thực hiện thao tác trên Canvas.
2. Dữ liệu trạng thái thao tác được đóng gói thành JSON gửi lên API nào?
3. Backend kiểm tra số bước thực hiện (`stepsUsed <= maxSteps * 1.5`) và trạng thái cuối ra sao?
4. Khi đạt yêu cầu, học viên quay lại `LadderView.vue` $\rightarrow$ Giao diện mở khóa thẻ Bậc 3 (Code Runner) như thế nào?

*Khu vực làm bài TL 7.2:*
```text
...
```

---

# 📘 ĐỀ 08: TRÌNH CHẠY CODE SANDBOX & CHẤM ĐIỂM SERVER
**Màn hình & Files trọng tâm:** `CodeRunnerView.vue` · `stores/codeRunner.ts` · `useCodeTracePlayback.ts` · `CodeRunsController.cs` · `CodelabJudgeService.cs`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 8.1 (Giao diện và Code Mẫu Khởi tạo trên CodeRunnerView):**  
Khi người dùng mở màn hình `/code/:key` (ví dụ: `/code/sort.bubble`), component `CodeRunnerView.vue` khởi tạo những gì trong `onMounted`?
- **A.** Mở một trình soạn thảo trống không có hướng dẫn.
- **B.** Bắt buộc người dùng tải file code từ máy tính lên.
- **C.** Gọi `codeStore.loadTemplate('sort.bubble')` nạp đoạn code mẫu có sẵn hàm `compare(i, j)` và `swap(i, j)`, đồng thời nạp generator tương ứng từ `simStore` để sẵn sàng hiển thị canvas 2 chiều.
- **D.** Tự động nộp bài luôn mà không cần viết code.

**Câu 8.2 (Tính năng "Chạy thử" Client Web Sandbox):**  
Khi người dùng bấm nút **"Chạy thử"** (Run Code) trên `CodeRunnerView.vue`:
- **A.** Lưu code vào file text trên Desktop.
- **B.** Code được gửi lên máy chủ để chạy lệnh `gcc` / `node`.
- **C.** In kết quả ra máy in văn phòng.
- **D.** Code được thực thi an toàn ngay tại Client qua Web Worker / Sandbox `runCode()` $\rightarrow$ Các hàm `compare()` và `swap()` ghi lại chuỗi sự kiện `TraceEvent[]` $\rightarrow$ Canvas bên cạnh tự động phát lại (Playback) trực quan hóa từng bước chạy của code vừa viết.

**Câu 8.3 (Tính năng "Nộp bài" Chấm điểm Server Jint):**  
Khi người dùng bấm nút **"Nộp bài"** (Submit) để hoàn thành bài tập Code Challenge Bậc 3:
- **A.** Frontend gửi mã nguồn lên `POST /api/v1/exercises/{id}/code-submit` $\rightarrow$ Backend sử dụng engine `CodelabJudgeService` (Jint JavaScript Sandbox cô lập) chạy mã nguồn với các bộ Test Case ẩn $\rightarrow$ Kiểm tra Output đúng, đo thời gian chạy (ms), bộ nhớ sử dụng $\rightarrow$ Ghi lịch sử vào bảng `CodeSubmissions` $\rightarrow$ Trả về danh sách Test Case Xanh/Đỏ.
- **B.** Client tự chấm và gửi điểm số lên server.
- **C.** Chờ giáo viên vào chấm thủ công sau 3 ngày.
- **D.** Bài nộp được gửi lên trang LeetCode để chấm điểm.

**Câu 8.4 (Điều khiển Playback Trace từ Code Người dùng):**  
Composable `useCodeTracePlayback` trong `CodeRunnerView.vue` cung cấp những khả năng điều khiển nào khi xem lại kết quả chạy code?
- **A.** Chỉ xem được kết quả cuối cùng, không tua lại được.
- **B.** Tự động chuyển đổi code từ C++ sang Java.
- **C.** Cho phép Play/Pause, Tua từng bước (Step Forward / Step Back), Kéo thanh trượt timeline và Đổi tốc độ phát lại (1x, 2x, 4x) tương ứng với từng dòng lệnh mã nguồn.
- **D.** Tự động sửa lỗi sai trong code của người dùng.

**Câu 8.5 (Xem Lịch sử Nộp bài Drawer History):**  
Khi người dùng bấm nút "Lịch sử nộp" trên Header của Code Runner:
- **A.** Tải về file PDF lịch sử học tập.
- **B.** Mở Drawer bên phải gọi API `GET /api/v1/exercises/{id}/submissions/me` (phân trang `PagedResponse<SubmissionSummaryDto>`) $\rightarrow$ Hiển thị danh sách các lần nộp trước đó kèm Trạng thái (Passed / Failed), Thời gian nộp, Số test case đạt và Nút bấm "Xem lại code cũ".
- **C.** Xóa toàn bộ các lần nộp bài trước đó.
- **D.** Hiển thị bảng xếp hạng của lớp.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 8.1 (Trace luồng Chạy Sandbox Client $\rightarrow$ Sinh Trace $\rightarrow$ Playback Canvas – 2.5 điểm):**  
Người dùng viết thuật toán Bubble Sort trong trình soạn thảo Code Runner:
1. Khi bấm nút "Chạy thử": Hàm `codeStore.run()` trong `stores/codeRunner.ts` thực thi đoạn mã người dùng như thế nào?
2. Mảng dữ liệu mặc định nào được truyền vào hàm giải thuật? Giới hạn số lượng sự kiện an toàn (`maxEvents`) là bao nhiêu?
3. Nếu code bị vòng lặp vô tận hoặc lỗi cú pháp (Syntax Error dòng 5): Trạng thái `runState` và biến `runError` hiển thị thông báo lỗi lên giao diện như thế nào?
4. Nếu code chạy thành công: Mảng sự kiện `TraceEvent[]` được truyền vào `playback.init()` và điều khiển Canvas phát lại ra sao?

*Khu vực làm bài TL 8.1:*
```text
...
```

**Câu TL 8.2 (Trace luồng Nộp bài Chấm điểm Server với Test Cases Ẩn – 2.5 điểm):**  
Tại bài tập Code Challenge Bậc 3:
1. Khi người dùng bấm nút "Nộp bài", request `POST /api/v1/exercises/{id}/code-submit` gửi lên những trường dữ liệu nào?
2. Phía máy chủ, `CodelabJudgeService.cs` khởi tạo môi trường Jint Sandbox an toàn ra sao để chống mã độc (ngăn chặn truy cập file system, network)?
3. Bộ test case (gồm Input và Expected Output) được nạp và so khớp kết quả như thế nào?
4. Kết quả chấm bài được lưu vào bảng `CodeSubmissions` và trả về `CodeSubmitResult` để UI hiển thị Bảng kết quả (Xanh: Accepted, Đỏ: Wrong Answer / Time Limit Exceeded) ra sao?

*Khu vực làm bài TL 8.2:*
```text
...
```

---

# 📘 ĐỀ 09: HỆ THỐNG GAMIFICATION: NHIỆM VỤ & BẢNG XẾP HẠNG
**Màn hình & Files trọng tâm:** `QuestsView.vue` · `LeaderboardView.vue` · `stores/gamification.ts` · `stores/leaderboard.ts` · `GamificationController.cs` · `GamificationService.cs`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 9.1 (Khởi tạo Dữ liệu trên QuestsView):**  
Khi người dùng mở màn hình `/quests` (`QuestsView.vue`), component gọi các API nào trong `onMounted`?
- **A.** Không gọi API nào mà lấy dữ liệu ngẫu nhiên ở client.
- **B.** Gọi đồng thời qua `Promise.allSettled`: `gamificationStore.fetchQuests()` (lấy danh sách 5 nhiệm vụ hàng ngày) và `gamificationStore.fetchSummary()` (lấy level, XP, Gems hiện tại).
- **C.** Chỉ gọi API xóa nhiệm vụ cũ.
- **D.** Tự động chuyển sang trang nạp tiền.

**Câu 9.2 (Cơ chế Nhận thưởng Nhiệm vụ Daily Quests):**  
Khi một nhiệm vụ (ví dụ: "Xem 1 mô phỏng thuật toán") đạt tiến độ `1/1 (100%)`, nút "Nhận thưởng" sáng lên. Khi người dùng click:
- **A.** Gửi `POST /api/v1/gamification/quests/{id}/claim` $\rightarrow$ Backend đánh dấu `Claimed = true`, cộng Gems & XP vào DB $\rightarrow$ Trả về `{ gems, xp }` $\rightarrow$ Topbar nhảy số Gems, hiệu ứng confetti chúc mừng và nút chuyển sang trạng thái "Đã nhận" (Disabled).
- **B.** Nhiệm vụ bị xóa vĩnh viễn khỏi hệ thống.
- **C.** Pinia tự cộng điểm mà không gửi request lên máy chủ.
- **D.** Gửi mã giảm giá qua tin nhắn SMS.

**Câu 9.3 (Cơ chế Reset Nhiệm vụ Hàng ngày):**  
Danh sách 5 nhiệm vụ hàng ngày (Daily Quests) được hệ thống làm mới (Reset) vào thời điểm nào?
- **A.** Không bao giờ reset.
- **B.** Sau mỗi 1 tiếng đồng hồ.
- **C.** Tự động vào lúc 00:00 UTC (07:00 sáng giờ Việt Nam) mỗi ngày; hệ thống sinh ngẫu nhiên 2 nhiệm vụ Dễ, 2 nhiệm vụ Trung bình và 1 nhiệm vụ Khó.
- **D.** Chỉ reset khi người dùng bấm nút xóa nhiệm vụ.

**Câu 9.4 (Các Tab Bảng Xếp Hạng LeaderboardView):**  
Màn hình Bảng xếp hạng `/leaderboard` (`LeaderboardView.vue`) cung cấp 3 tab xem thứ hạng nào?
- **A.** Xếp hạng theo số tiền đã nạp vào ứng dụng.
- **B.** Toàn quốc · Quốc tế · Trường học.
- **C.** Xếp hạng theo độ tuổi người học.
- **D.** **Tab Tuần** (XP tuần hiện tại) · **Tab Level** (xếp theo cấp độ người chơi) · **Tab Lớp** (bảng xếp hạng nội bộ lớp học đang chọn).

**Câu 9.5 (Trực quan hóa Bảng Xếp Hạng Leaderboard):**  
Giao diện Bảng xếp hạng trên `LeaderboardView.vue` hiển thị top người chơi bằng thành phần đồ họa nào?
- **A.** Biểu đồ cột ngang (bar chart) vẽ bằng ECharts hiển thị top ~10 người; 3 vị trí đầu được phân biệt bằng màu sắc riêng (vàng/bạc/đồng) nhưng KHÔNG dùng bục podium 3D.
- **B.** Bục vinh danh 3 bậc dạng podium với cúp vàng/bạc/đồng và avatar VIP.
- **C.** Danh sách text thuần túy không có biểu đồ.
- **D.** Bản đồ nhiệt (heatmap) thể hiện mật độ điểm số theo vùng miền.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 9.1 (Trace luồng Hoàn thành Nhiệm vụ $\rightarrow$ Nhận thưởng Gems & Level Up – 2.5 điểm):**  
Người dùng đang có Level 2 với 380/400 XP (công thức: `level = 1 + floor(sqrt(xp/100))`; xpFloor level 2 = 100, xp cần cho level 3 = 400), số dư 50 Gems:
1. Người dùng vào `/quests` bấm nút "Nhận thưởng" cho nhiệm vụ khó (phần thưởng: +30 XP, +30 Gems).
2. API `POST /api/v1/gamification/me/quests/{id}/claim` xử lý cộng dồn XP và Gems trong bảng `UserGamification` như thế nào?
3. Khi tổng XP đạt 410 $\rightarrow$ Backend tính `ComputeLevel(410)` = 3 (vì `1 + floor(sqrt(410/100))` = 3); `xpIntoLevel` mới = 410 − 400 = 10 XP. Giải thích tại sao KHÔNG có cơ chế "XP overflow carry" sang level tiếp theo.
4. Pinia store cập nhật state `summary`, Topbar và Profile phản ứng thay đổi số dư và thanh tiến độ Level như thế nào?

*Khu vực làm bài TL 9.1:*
```text
...
```

**Câu TL 9.2 (Trace luồng Xem Bảng Xếp Hạng Nội bộ Lớp học – 2.5 điểm):**  
Tại `LeaderboardView.vue`:
1. Khi học viên bấm chuyển sang tab "Lớp" (`tab = 'class'`): Component kiểm tra điều kiện gì từ `classStore` để xác định có lớp nào đang tham gia hay không?
2. Nếu học viên tham gia nhiều lớp học $\rightarrow$ Dropdown chọn lớp hoạt động thế nào? Giá trị `classId` được truyền vào API ra sao?
3. API `GET /api/v1/leaderboard?tab=class&classId=...` trả về danh sách học viên xếp theo tiêu chí gì (XP tuần, XP toàn thời gian, hay level)?
4. Nếu học viên chưa tham gia lớp học nào $\rightarrow$ Giao diện hiển thị Empty State với thông điệp gì và nút điều hướng về route nào?

*Khu vực làm bài TL 9.2:*
```text
...
```

---

# 📘 ĐỀ 10: CỬA HÀNG GEMS & GÓI HỘI VIÊN PREMIUM
**Màn hình & Files trọng tâm:** `ShopView.vue` · `PremiumView.vue` · `SubscriptionView.vue` · `stores/gamification.ts` · `GamificationController.cs` · `MeController.cs`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 10.1 (Khởi tạo Cửa hàng & Kiểm tra Khả năng Mua):**  
Khi người dùng mở `/shop` (`ShopView.vue`), hệ thống nạp dữ liệu gì và tính toán cờ `canAfford` của từng vật phẩm ra sao?
- **A.** Lấy dữ liệu tĩnh; mọi vật phẩm đều bấm mua được dù không có Gems.
- **B.** Bắt buộc phải kết nối thẻ Visa mới xem được cửa hàng.
- **C.** Cửa hàng chỉ mở vào khung giờ vàng 12h trưa.
- **D.** Gọi song song `gamificationApi.fetchShopItems()`, `gamificationStore.fetchInventory()`, `gamificationStore.fetchSummary()` $\rightarrow$ So sánh `userGems >= item.price`; nếu không đủ Gems, nút "Mua" bị disable và hiện số Gems còn thiếu màu đỏ.

**Câu 10.2 (Danh mục Vật phẩm trong Shop Gems):**  
Theo seed data `SeedData.cs`, cửa hàng Gems cung cấp những vật phẩm nào sau đây?
- **A.** Gói nạp tiền thật qua ví điện tử MoMo/ZaloPay và voucher giảm giá khóa học.
- **B.** Quyền truy cập sớm vào bài học chưa xuất bản và đề thi thử độc quyền.
- **C.** **Avatar Cyber Hacker** (100 Gems) · **Avatar Golden Knight** (200 Gems) · **Frame Neon Border** (300 Gems) · **Frame Royal Gold** (500 Gems) — chia theo Type 1 (avatar) và Type 2 (frame).
- **D.** Chứng chỉ hoàn thành lộ trình dạng PDF có chữ ký số của giảng viên.

**Câu 10.3 (Cơ chế Premium & Trái tim):**  
Theo `GamificationService.cs` (FR-10.1/FR-10.7), tài khoản Premium khác tài khoản Free ở điểm cốt lõi nào?
- **A.** Premium được miễn trừ hoàn toàn việc trừ tim khi vào node luyện tập (tim vô hạn), còn lại mọi cơ chế giống Free.
- **B.** HeartsMax nâng lên 30 (Free là 10) và chu kỳ hồi tim rút xuống 10 phút/tim (Free là 30 phút/tim); khi gói hết hạn, lần đọc hearts kế tiếp tự clamp HeartsMax/Hearts về ngưỡng Free (lazy downgrade); Premium vẫn bị trừ 1 tim khi bắt đầu node session mới như thường.
- **C.** Premium tự động cấp quyền Admin và mở khóa toàn bộ endpoint quản trị `/admin/*`.
- **D.** Premium cho phép bỏ qua validation mật khẩu khi đăng ký tài khoản mới.

**Câu 10.4 (Nội dung Chuyển khoản VietQR Chuẩn hóa):**  
Theo `PremiumView.vue` dòng 70-72, nội dung chuyển khoản (addInfo) trong mã VietQR được sinh tự động theo định dạng nào?
- **A.** Người dùng tự nhập tay nội dung chuyển khoản tùy ý, hệ thống không kiểm soát cú pháp.
- **B.** Định dạng cố định `DSV{userId}T{months}` (ví dụ: `DSV42T12` cho user ID 42 mua gói 12 tháng) — user KHÔNG được chỉnh sửa; QR payload build qua hàm `buildVietQrPayload(MB_BENEFICIARY, amount, transferContent)`.
- **C.** Định dạng `PREMIUM-{planId}-{randomUUID}` do backend sinh và trả về qua API checkout.
- **D.** Chuỗi rỗng — VietQR không yêu cầu nội dung chuyển khoản, chỉ cần đúng số tiền.

**Câu 10.5 (Quản lý Gói Thuê bao SubscriptionView):**  
Màn hình Quản lý gói thuê bao `/account/subscription` (`SubscriptionView.vue`) cho phép người dùng thực hiện những thao tác gì?
- **A.** Rút tiền từ tài khoản về ngân hàng.
- **B.** Chuyển nhượng tài khoản cho người khác.
- **C.** Đổi mật khẩu ngân hàng.
- **D.** Xem ngày hết hạn gói Premium hiện tại, Lịch sử các giao dịch đã thanh toán, và Nút bấm "Hủy gia hạn tự động" hoặc "Nâng cấp gói trọn đời".

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 10.1 (Trace luồng Mua Bình Hồi Tim trong Shop bằng Gems – 2.5 điểm):**  
Người dùng đang có 0 Tim, số dư 150 Gems:
1. Người dùng vào `/shop`, click nút "Mua ngay" trên vật phẩm "Bình hồi 5 Tim" (giá 100 Gems). Modal xác nhận hiển thị những thông tin gì?
2. Khi bấm "Xác nhận mua", API `POST /api/v1/gamification/shop/{id}/buy` được gọi với payload gì?
3. Phía Backend, Transaction trong Database kiểm tra số dư và cập nhật `Gems = 50`, `Hearts = 5` ra sao?
4. Khi nhận Response thành công, `gamificationStore` cập nhật những state nào và Topbar thay đổi ra sao?

*Khu vực làm bài TL 10.1:*
```text
...
```

**Câu TL 10.2 (Trace luồng Mua Gói Premium $\rightarrow$ Kích hoạt Quyền Lợi Toàn Hệ Thống – 2.5 điểm):**  
Tại `PremiumView.vue`:
1. Người dùng chọn gói 1 Năm (giá 499.000 VNĐ) và bấm "Thanh toán VietQR".
2. Thư viện nào được sử dụng để sinh mã QR? Nội dung chuyển khoản được cấu hình định dạng chuẩn gì để tránh nhầm lẫn?
3. Khi Backend xác nhận thanh toán thành công: Bảng `Subscriptions` và trường `Role` / `IsPremium` của User trong CSDL được cập nhật như thế nào?
4. Khi User quay lại học bài, hệ thống kiểm tra cờ `isPremium` để bỏ qua việc trừ tim như thế nào?

*Khu vực làm bài TL 10.2:*
```text
...
```

---

# 📘 ĐỀ 11: HỒ SƠ CÁ NHÂN, THÀNH TÍCH & CÀI ĐẶT
**Màn hình & Files trọng tâm:** `ProfileView.vue` (1595 dòng) · `stores/auth.ts` · `stores/gamification.ts` · `MeController.cs` · `UserService.cs`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 11.1 (5 Tab Chức năng trên ProfileView):**  
Theo `ProfileView.vue` dòng 64, reactive state `tab` khai báo đúng 5 giá trị nào?
- **A.** `'overview' | 'progress' | 'achievements' | 'inventory' | 'settings'` — tương ứng Tổng quan, Tiến độ học (Heatmap), Thành tích, Kho đồ và Cài đặt.
- **B.** `'attendance' | 'schedule' | 'tuition' | 'grades' | 'awards'` — mô hình trường học truyền thống.
- **C.** `'friends' | 'messages' | 'groups' | 'posts' | 'photos'` — mô hình mạng xã hội.
- **D.** `'dashboard' | 'revenue' | 'customers' | 'invoices' | 'reports'` — mô hình quản trị doanh nghiệp.

**Câu 11.2 (Biểu đồ Hoạt động Đóng góp Heatmap):**  
Tab "Tiến độ học" trên Profile hiển thị biểu đồ nhiệt (Activity Heatmap kiểu GitHub). Biểu đồ này thể hiện điều gì?
- **A.** Thời gian ngủ của người dùng mỗi ngày.
- **B.** Dự báo thời tiết các ngày trong tuần.
- **C.** Số lượng bài học, bài tập mô phỏng và bài code mà người dùng đã hoàn thành trong từng ngày của năm qua các ô màu xanh đậm/nhạt theo tần suất hoạt động.
- **D.** Tốc độ mạng internet của người dùng.

**Câu 11.3 (Endpoint Trang bị Vật phẩm Kho đồ):**  
Theo `GamificationController.cs` dòng 164-167, khi người dùng bấm "Trang bị" trên một vật phẩm trong Kho đồ, frontend gọi endpoint nào?
- **A.** `PUT /api/v1/gamification/me/inventory/equip` với body `EquipRequest { ItemId, IsEquipped }` — Backend `EquipItemAsync` cập nhật trạng thái trang bị trong `UserInventory` và refetch inventory phía FE.
- **B.** `POST /api/v1/gamification/inventory/{itemId}/equip` — truyền itemId trên URL path.
- **C.** `PATCH /api/v1/users/me/avatar-frame` — endpoint riêng chỉ dành cho khung avatar.
- **D.** `GET /api/v1/gamification/inventory/{itemId}/toggle-equip` — toggle bằng GET request.

**Câu 11.4 (Cập nhật Hồ sơ Cá nhân):**  
Khi người dùng thay đổi tên hiển thị trong tab Cài đặt và bấm "Lưu thay đổi", hệ thống xử lý thế nào?
- **A.** Gửi `POST /api/v1/users/update-profile` với form-data multipart kèm ảnh CCCD bắt buộc.
- **B.** Endpoint đã bị deprecated; người dùng phải liên hệ admin để đổi tên thủ công.
- **C.** Gửi `PUT /api/v1/auth/me` với `{ displayName: "..." }` → `AuthController.UpdateMe` gọi `UpdateProfileAsync` cập nhật trường `DisplayName` trong DB → `authStore.user.displayName` phản ánh tức thì trên Topbar.
- **D.** Thay đổi được lưu vào localStorage nhưng không gửi lên server, mất khi xóa cache.

**Câu 11.5 (Phê duyệt Giảng viên từ Admin):**  
Theo `UsersController.cs` dòng 79-84, endpoint phê duyệt giảng viên có route và cơ chế nào?
- **A.** Sinh viên tự gọi `POST /api/v1/me/teacher-application` để nộp đơn; hệ thống tự động duyệt nếu email thuộc miền `.edu.vn`.
- **B.** Admin gọi `POST /api/v1/users/{id}/approve-teacher` (UsersController, [Authorize(Roles = "ADMIN")]) với body `ApproveTeacherRequest` → `ApproveTeacherAsync(actorId, actorIsPrimaryAdmin, id, request)` kiểm tra PrimaryAdmin trước khi chuyển role sang TEACHER.
- **C.** Giảng viên được tạo trực tiếp bởi Super Admin qua `POST /api/v1/admin/users` với `role: 'TEACHER'`, không có quy trình phê duyệt.
- **D.** Hệ thống gửi email mời giảng viên qua SMTP; link xác nhận trong email tự động kích hoạt tài khoản TEACHER.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 11.1 (Trace luồng Đổi Mật khẩu trong Tab Cài đặt Profile – 2.5 điểm):**  
Người dùng vào tab Cài đặt trên Profile để đổi mật khẩu:
1. Form yêu cầu nhập những trường nào? Quy tắc validate ở Client kiểm tra những gì?
2. Khi bấm "Đổi mật khẩu", API `PUT /api/v1/auth/change-password` gửi đi payload gồm những gì?
3. Phía Backend, `AuthService.ChangePasswordAsync` kiểm tra mật khẩu cũ qua BCrypt và lưu mật khẩu mới ra sao?
4. Khi thành công, form phản hồi gì và các thiết bị đăng nhập khác bị xử lý refresh token ra sao?

*Khu vực làm bài TL 11.1:*
```text
...
```

**Câu TL 11.2 (Trace luồng Nộp đơn Giáo viên $\rightarrow$ Admin Phê duyệt $\rightarrow$ Mở khóa Studio – 2.5 điểm):**  
Học viên nộp hồ sơ trở thành Giảng viên trên nền tảng:
1. Hồ sơ gửi lên `POST /api/v1/me/teacher-application` gồm những thông tin gì? Được lưu vào bảng CSDL nào?
2. Tại màn hình Profile của học viên, thẻ trạng thái hiển thị những thông tin gì trong thời gian chờ Admin duyệt?
3. Khi Admin vào `/admin/users` bấm "✅ Phê duyệt đơn", trường `Role` trong bảng `Users` đổi thành gì?
4. Khi Giảng viên đăng nhập lại, thanh Sidebar và Router Guard mở khóa những màn hình/tính năng mới nào?

*Khu vực làm bài TL 11.2:*
```text
...
```

---

# 📘 ĐỀ 12: HỆ THỐNG LỚP HỌC & STUDIO GIẢNG VIÊN
**Màn hình & Files trọng tâm:** `ClassesView.vue` · `ClassDetailView.vue` · `ClassReportView.vue` · `AdminContentView.vue` · `AdminLessonEditorView.vue` · `ClassesController.cs` · `ClassService.cs`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 12.1 (Điều kiện Nhìn thấy Nút "Tạo lớp mới"):**  
Trên màn hình Lớp học `/classes` (`ClassesView.vue`), nút "Tạo lớp mới" chỉ hiển thị khi nào?
- **A.** Hiển thị khi người dùng có trên 1000 Gems.
- **B.** Hiển thị cho tất cả mọi người kể cả khách chưa đăng nhập.
- **C.** Chỉ hiển thị khi người dùng đạt Level 50.
- **D.** Chỉ hiển thị khi người dùng đã đăng nhập và có vai trò là Giảng viên hoặc Quản trị viên (`authStore.role === 'TEACHER' || 'ADMIN'`).

**Câu 12.2 (Sinh Mã Mời Lớp học 6 Ký tự):**  
Khi Giảng viên điền tên lớp, mô tả, chọn lộ trình gán cho lớp và bấm "Tạo lớp":
- **A.** Hệ thống gửi mã mời qua bưu điện.
- **B.** Hệ thống tạo link Google Meet.
- **C.** Gửi `POST /api/v1/classes` $\rightarrow$ Backend `ClassService` tạo bản ghi lớp học và tự động sinh một **Mã mời (Join Code) ngẫu nhiên duy nhất gồm 6 ký tự** chữ và số (ví dụ: `DSA999`) $\rightarrow$ Trả về `ClassDto` (201 Created) $\rightarrow$ Chuyển hướng vào trang chi tiết lớp.
- **D.** Giáo viên phải tự gõ mã mời bằng tay theo ý mình.

**Câu 12.3 (Sinh viên Tham gia Lớp bằng Mã 6 Ký tự):**  
Sinh viên vào `/classes`, bấm nút "Tham gia lớp học", nhập mã `DSA999` và bấm "Vào lớp":
- **A.** Gửi `POST /api/v1/classes/join-by-code` với `{ code: "DSA999" }` $\rightarrow$ Backend kiểm tra mã hợp lệ, kiểm tra chưa tham gia $\rightarrow$ Thêm học viên vào bảng `ClassMembers` $\rightarrow$ Trả về `ClassDetailDto` $\rightarrow$ UI thêm lớp mới vào danh sách lớp của học viên.
- **B.** Mã được lưu vào Cookie và không gửi lên máy chủ.
- **C.** Sinh viên phải nộp 100 Gems để vào lớp.
- **D.** Hệ thống bắt buộc phải phỏng vấn sinh viên trước khi cho vào lớp.

**Câu 12.4 (Xuất Báo cáo Tiến độ Lớp học):**  
Theo `ClassReportView.vue` dòng 86-91, khi Giảng viên bấm "Xuất báo cáo", hệ thống xử lý thế nào?
- **A.** Chụp ảnh màn hình bảng danh sách bằng html2canvas rồi tải xuống dạng PNG.
- **B.** Gọi `classesApi.exportClassReportCsv(classId)` → nhận chuỗi CSV text → prepend BOM `\uFEFF` (UTF-8 Excel compatible) → tạo Blob `text/csv;charset=utf-8` → trigger download file `class-report-{id}.csv`.
- **C.** Gọi `GET /api/v1/classes/{id}/export-pdf` → Backend render PDF server-side bằng Puppeteer → trả binary stream.
- **D.** Mở modal hiển thị bảng điểm inline, không hỗ trợ tải xuống file.

**Câu 12.5 (Trình soạn thảo Bài học Studio AdminLessonEditorView):**  
Tại `/studio/lessons/new` (`AdminLessonEditorView.vue`), Giảng viên soạn bài học được hỗ trợ những công cụ gì?
- **A.** Không cho phép chỉnh sửa nội dung bài học.
- **B.** Chỉ là một ô nhập text đơn giản không có định dạng.
- **C.** Bắt buộc phải quay video clip mp4 tải lên.
- **D.** Trình soạn thảo 2 cột (Markdown Editor bên trái + Live Preview HTML bên phải), hỗ trợ định dạng Callout ghi chú, Công thức toán học KaTeX, Khung chèn mã nguồn có highlight cú pháp, và Dropdown chọn mô phỏng thuật toán gắn kèm vào bài học.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 12.1 (Trace luồng Giảng viên Quản lý Thành viên Lớp học – 2.5 điểm):**  
Tại màn hình chi tiết lớp `/classes/10`:
1. `ClassDetailView.vue` gọi API nào để lấy danh sách học viên, bảng xếp hạng nội bộ và tiến độ học của cả lớp?
2. Giảng viên muốn gỡ một học sinh vi phạm ra khỏi lớp: Khi click nút "Xóa thành viên" $\rightarrow$ Modal xác nhận hiển thị gì?
3. API `DELETE /api/v1/classes/10/members/{userId}` được gửi đi với quyền hạn nào? Backend xóa bản ghi trong bảng nào?
4. Danh sách học viên trên giao diện cập nhật xóa dòng của học sinh đó ra sao?

*Khu vực làm bài TL 12.1:*
```text
...
```

**Câu TL 12.2 (Trace luồng Giảng viên Soạn & Xuất bản Bài học Mới – 2.5 điểm):**  
Giảng viên truy cập `/studio/lessons/new`:
1. Giảng viên điền Tiêu đề, chọn Lộ trình cha, nhập nội dung Markdown lý thuyết, chọn thuật toán đính kèm `sort.quick`.
2. Khi bấm nút "Lưu & Xuất bản" (Publish): Payload gửi lên `POST /api/v1/lessons` gồm những trường dữ liệu nào?
3. Backend `LessonService` lưu bài học vào bảng `Lessons`, kiểm tra quyền của Giảng viên ra sao?
4. Sau khi tạo thành công, bài học mới xuất hiện trên cây lộ trình `/path/:id` của học sinh như thế nào?

*Khu vực làm bài TL 12.2:*
```text
...
```

---

# 📘 ĐỀ 13: BẢNG ĐIỀU KHIỂN QUẢN TRỊ HỆ THỐNG
**Màn hình & Files trọng tâm:** `AdminUsersView.vue` · `AdminStatsView.vue` · `AdminSettingsView.vue` · `AdminController.cs` · `SettingService.cs`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 13.1 (Tìm kiếm & Lọc trên AdminUsersView):**  
Theo `AdminUsersView.vue` dòng 49-51 và 268-284, cơ chế tìm kiếm/lọc người dùng hoạt động thế nào?
- **A.** Ô tìm kiếm dùng RxJS debounceTime(300ms) + distinctUntilChanged trước khi gọi API.
- **B.** Tìm kiếm client-side bằng Array.filter trên mảng users đã fetch sẵn; không gọi lại API khi gõ.
- **C.** Reactive refs `search`, `roleFilter`, `statusFilter` truyền vào `adminApi.fetchUsers({ role, q, page })`; hàm `load()` gọi API server-side mỗi lần filter thay đổi hoặc submit; tab `'all' | 'pending'` tách riêng danh sách thường và đơn chờ duyệt.
- **D.** Chỉ lọc được theo vai trò, không hỗ trợ tìm kiếm text theo email/họ tên.

**Câu 13.2 (Business Rule Bảo vệ An toàn Tài khoản Admin):**  
Trong `AdminUsersView.vue` và `AdminController.cs`, quy tắc nghiệp vụ nào được cài đặt để bảo vệ tài khoản quản trị viên?
- **A.** Admin KHÔNG ĐƯỢC PHÉP khóa tài khoản của chính mình, KHÔNG ĐƯỢC hạ quyền của Admin khác, và luôn phải có ít nhất 1 tài khoản Super Admin hoạt động trong hệ thống.
- **B.** Admin có thể tự xóa tài khoản của chính mình bất kỳ lúc nào.
- **C.** Admin bị tự động hạ quyền thành Student sau 30 ngày.
- **D.** Ai cũng có quyền tự nâng mình lên làm Admin.

**Câu 13.3 (Khóa/Mở khóa Tài khoản Người dùng):**  
Theo `UsersController.cs` dòng 64-70 và `UserService.SetStatusAsync`, thao tác khóa tài khoản hoạt động thế nào?
- **A.** Gọi `DELETE /api/v1/admin/users/{id}` soft-delete user; khôi phục phải tạo lại tài khoản mới.
- **B.** Gửi email reset password cưỡng bức + SMS OTP xác nhận đến số điện thoại đã đăng ký.
- **C.** Gửi `PUT /api/v1/admin/users/{id}/block` với `{ isBlocked: true }` → Backend set cờ IsBlocked và xóa toàn bộ session.
- **D.** Gửi `PUT /api/v1/admin/users/{id}/status` với body `{ isActive: false }` → Backend set `user.IsActive = false` và lưu DB; nếu target là Admin thì kiểm tra `HasOtherActiveAdminAsync` trước — KHÔNG tự động revoke refresh token (revoke chỉ xảy ra trong ChangePassword).

**Câu 13.4 (Chỉ số Thống kê trên AdminStatsView):**  
Theo `StatsDto.cs` và `AdminController.cs` dòng 33-48, `GET /api/v1/admin/stats` trả về những chỉ số nào?
- **A.** Biểu đồ tăng trưởng DAU/MAU, doanh thu Premium theo tháng, tỷ lệ hoàn thành khóa học và heatmap hoạt động theo khung giờ.
- **B.** 13 scalar counts trong 1 SQL query: TotalUsers, TotalStudents, TotalTeachers, TotalAdmins, TotalTopics, TotalLessons, TotalExercises, TotalSubmissions, TotalCodeSubmissions, TotalClasses, TotalFavorites, TotalSimulations, ActiveUsersToday — KHÔNG có trend/revenue/completion rate.
- **C.** Danh sách top 10 user hoạt động nhiều nhất kèm thời gian online trung bình.
- **D.** Log audit trail 30 ngày gần nhất gồm IP, user-agent và endpoint truy cập.

**Câu 13.5 (Cấu hình Hệ thống SettingsController):**  
Theo `SettingDto.cs` và `SettingsController.cs` (route `api/v1/settings`), Admin có thể tinh chỉnh những tham số nào qua `PUT /api/v1/settings`?
- **A.** ThemeColor, LogoUrl, FaviconPath — tùy chỉnh giao diện frontend.
- **B.** HeartMax, HeartRefillMinutes, XpPerQuiz, SmtpHost, AiApiKey — các tham số gamification và tích hợp bên ngoài.
- **C.** SiteName, AllowedDomains (email), PasswordPolicy.MinLength, UploadMaxMb (1-100), SandboxSeconds (1-120), SandboxMemoryMb — validate bởi `SystemSettingsValidator`; policy uppercase/digit/special cố định, sandbox chạy client-side.
- **D.** DatabaseConnectionString, JwtSecretKey, RedisCacheUrl — cấu hình hạ tầng nhạy cảm.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 13.1 (Trace luồng Phê duyệt / Từ chối Đơn Giảng viên – 2.5 điểm):**  
Admin đang ở tab "Duyệt Giáo viên" trên `AdminUsersView.vue`:
1. Danh sách đơn chờ duyệt (`TEACHER_PENDING`) hiển thị những thông tin gì (Trường công tác, CV link, Lý do)?
2. Khi Admin bấm nút "❌ Từ chối", Modal yêu cầu nhập lý do từ chối hoạt động thế nào?
3. API `POST /api/v1/admin/teacher-applications/{id}/review` gửi lên payload gồm những gì?
4. Backend cập nhật trạng thái đơn trong CSDL, gửi thông báo hệ thống và hoàn trả role `STUDENT` ra sao?

*Khu vực làm bài TL 13.1:*
```text
...
```

**Câu TL 13.2 (Trace luồng Thay đổi Cấu hình Sandbox và Đồng bộ Toàn Hệ Thống – 2.5 điểm):**  
Tại `AdminSettingsView.vue`, admin thay đổi giới hạn thời gian chạy code sandbox (`SandboxSeconds`) từ 1500ms quy đổi sang mức mới và bấm "Lưu cấu hình":
1. Form `AdminSettingsView.vue` thu thập và validate những trường cấu hình nào (liệt kê các trường thật trong `SystemSettingsDto`)?
2. API `PUT /api/v1/settings` gửi payload xuống Backend như thế nào? Ai được phép gọi endpoint này?
3. Backend `SettingService` lưu vào đâu và `SettingsCache` singleton invalidate/nạp lại ra sao (cơ chế double-checked lock)?
4. Vì sao toàn hệ thống áp dụng cấu hình mới ngay mà không cần restart máy chủ? (Gợi ý cross-ref Đề 08: `CodelabJudgeService` đọc giới hạn runtime từ đâu?)

*Khu vực làm bài TL 13.2:*
```text
...
```

---

# 🏆 ĐỀ FINAL: BÀI THI ĐÁNH GIÁ NĂNG LỰC TOÀN HỆ THỐNG
## Integration End-to-End System Test (Thời gian: 60 phút – Thang điểm: 20.0 điểm)

> **Mục tiêu:** Kiểm tra năng lực hiểu biết toàn diện toàn bộ luồng nghiệp vụ của hệ thống DSA Visual từ khi là Khách vãng lai $\rightarrow$ Học viên $\rightarrow$ Thành viên Lớp học $\rightarrow$ Giảng viên $\rightarrow$ Quản trị viên.

### 📝 PHẦN I: TRẮC NGHIỆM TỔNG HỢP TOÀN HỆ THỐNG (10 câu – 10.0 điểm)

**Câu F.1 (Auth & Token Lifecycle):**  
Một AccessToken trong hệ thống có thời hạn hiệu lực là 15 phút. Khi một request API bị 401 trong lúc học viên đang làm bài thi:
- **A.** Axios Interceptor âm thầm gửi `POST /auth/refresh` bằng Cookie HttpOnly, lấy AccessToken mới, gán lại vào header của request gốc và retry — học viên không hề bị gián đoạn trải nghiệm.
- **B.** Interceptor hủy request, hiện modal "Phiên đăng nhập hết hạn" và bắt học viên đăng nhập lại từ đầu trước khi nộp bài.
- **C.** Interceptor xóa token khỏi Pinia và chuyển hướng về `/login?redirect=...`, mất toàn bộ tiến trình bài đang làm.
- **D.** Frontend bỏ qua lỗi 401, tự gửi lại request cũ thêm 3 lần với token đã hết hạn rồi mới báo lỗi.

**Câu F.2 (Demo Allowed vs Auth Requirement):**  
Khách vãng lai chưa đăng nhập có thể truy cập tối đa những tính năng nào sau đây?
- **A.** Xem trọn bộ danh mục mô phỏng và làm trắc nghiệm Bậc 1 Quiz, nhưng không nộp được bài code Bậc 3.
- **B.** Vào chi tiết từng bài học `/lessons/:id` và tương tác các bài tập trong bài.
- **C.** Xem bảng xếp hạng toàn hệ thống kèm điểm XP chi tiết của mọi học viên.
- **D.** Trang chủ (`/`), 3 bài mô phỏng demo công khai (`/simulator/sort.bubble`, `/simulator/search.binary`, `/simulator/graph.bfs`), danh mục lộ trình tổng quan (`/path`), trang trợ giúp (`/help`) và trang đăng nhập/đăng ký.

**Câu F.3 (Thang Thực hành Practice Ladder 3 Bậc):**  
Tỷ trọng đóng góp điểm số và mở khóa của 3 Bậc trong Practice Ladder là:
- **A.** Chỉ Bậc 1 Quiz được chấm điểm; Bậc 2 và Bậc 3 chỉ hiển thị lời giải tham khảo không tính kết quả.
- **B.** Bậc 1 (Quiz) $\rightarrow$ Bậc 2 (Interactive Lab) $\rightarrow$ Bậc 3 (Code Challenge); mỗi bậc đánh giá một tầng nhận thức từ Nhận biết/Thông hiểu $\rightarrow$ Vận dụng trực quan $\rightarrow$ Lập trình thực tế.
- **C.** Thứ tự ngược — Code Challenge làm trước để mở khóa Interactive Lab, Quiz tổng kết làm cuối cùng.
- **D.** Ba bậc độc lập: học viên được chọn làm ngay Bậc 3 Code Challenge mà không cần hoàn thành Quiz và Lab trước.

**Câu F.4 (Client Sandbox vs Server Judge):**  
Sự khác biệt cốt lõi giữa nút "Chạy thử" (Run) và nút "Nộp bài" (Submit) trên màn hình Code Runner là gì?
- **A.** Nút "Chạy thử" thực thi ngay tại Client Web Sandbox với mảng mẫu để sinh `TraceEvent[]` phát lại trên Canvas; còn nút "Nộp bài" gửi mã nguồn lên máy chủ để `CodelabJudgeService` (Jint Engine) chấm với các Test Case ẩn độc lập nhằm chống gian lận.
- **B.** Cả hai nút đều gửi code lên máy chủ; chỉ khác "Chạy thử" không chấm điểm còn "Nộp bài" có chấm.
- **C.** "Chạy thử" gửi code lên server chạy rồi trả về ảnh GIF mô phỏng; "Nộp bài" chỉ lưu code vào bảng `CodeSubmissions` mà không chấm điểm.
- **D.** "Chạy thử" chạy bằng Web Worker ngay trong trình duyệt; "Nộp bài" cũng chạy phía client nhưng trong iframe cách ly.

**Câu F.5 (Cơ chế Quản lý Tài nguyên Tim & Gems):**  
Trong hệ thống Gamification của nền tảng:
- **A.** Tim dùng để mua vật phẩm trong Shop; Gems bị trừ mỗi khi trả lời sai một câu trắc nghiệm.
- **B.** Tim không tự hồi — phải chờ Admin cấp lại thủ công; Gems chỉ dùng để thanh toán gói Premium.
- **C.** Tim (Free tối đa 10, Premium tối đa 30) là tài nguyên bắt buộc khi vào phiên luyện node — trừ 1 tim mỗi lần enter node bằng UPDATE atomic (node đã PASS thì vào lại miễn phí), tự hồi 30 phút/tim (Premium 10 phút/tim); Gems là tiền tệ thưởng khi nhận thưởng nhiệm vụ, dùng mua vật phẩm cosmetic trong Shop.
- **D.** Tim và Gems lưu trong LocalStorage của trình duyệt nên người dùng có thể tự chỉnh bằng DevTools.

**Câu F.6 (Kiến trúc Sandbox 4 Tab):**  
Bốn phân khu `/sorting-sandbox`, `/searching-sandbox`, `/graph-playground` và `/stack-queue-sandbox`:
- **A.** Là 4 component view hoàn toàn độc lập, mỗi route tải một bundle JavaScript riêng không dùng chung mã nguồn.
- **B.** Mỗi tab gọi API backend riêng để sinh dữ liệu mô phỏng phía server trước khi vẽ.
- **C.** 4 route trỏ về cùng 1 component nhưng chuyển tab bằng reload toàn bộ trang (F5).
- **D.** Được gom chung vào 1 View duy nhất `SortingView.vue` với cơ chế remount theo `:key="$route.fullPath"` giúp tái sử dụng tối đa mã nguồn và thống nhất trải nghiệm người dùng.

**Câu F.7 (Hệ thống Lớp học & Mã mời Join Code):**  
Mã mời lớp học (ví dụ: `DSA999`):
- **A.** Hết hạn sau 5 phút nếu không có ai dùng để join, giảng viên phải tạo mã mới.
- **B.** Gồm 6 ký tự ngẫu nhiên duy nhất sinh từ máy chủ khi Giảng viên tạo lớp, giúp Sinh viên chỉ cần nhập 6 ký tự là tham gia được lớp học mà không cần gửi link phức tạp.
- **C.** Là chuỗi 32 ký tự hex mã hóa UUID của lớp, sinh viên phải copy toàn bộ vào ô tham gia.
- **D.** Do sinh viên tự sinh khi bấm "Tham gia lớp" và gửi kèm CV cho giảng viên duyệt.

**Câu F.8 (Curriculum Studio & Quyền Soạn bài):**  
Để có quyền truy cập vào đường dẫn `/studio` (`AdminContentView.vue`) và soạn thảo bài học mới:
- **A.** Người dùng bắt buộc phải có vai trò là `TEACHER` (đã được Admin phê duyệt) hoặc `ADMIN`.
- **B.** Bất kỳ tài khoản đã đăng nhập nào cũng vào được; quyền chỉ được kiểm tra ở backend khi lưu bài.
- **C.** Mọi role đều được vào xem, nhưng chỉ ADMIN được bấm lưu; TEACHER chỉ xem trước.
- **D.** Route mở cho khách vãng lai để giới thiệu tính năng soạn bài cho người mới.

**Câu F.9 (Quy chuẩn Xử lý Lỗi Toàn cục Global Error Handling):**  
Khi một API trả về mã lỗi `HTTP 429 Too Many Requests` (do người dùng spam thao tác quá nhanh):
- **A.** Interceptor hiểu 429 là lỗi token, tự động gọi refresh rồi retry request như lỗi 401.
- **B.** Backend chặn vĩnh viễn tài khoản spam và gửi email cảnh báo cho người dùng.
- **C.** Axios Interceptor trong `client.ts` đọc header `Retry-After`, kích hoạt `uiStore.showToast` hiển thị thông báo "Bạn thao tác quá nhanh, vui lòng thử lại sau N giây" với kiểu cảnh báo `warning`.
- **D.** Interceptor nuốt lỗi im lặng, người dùng bấm lại không thấy phản hồi gì cho tới khi reload trang.

**Câu F.10 (Trạng thái Premium trong CSDL & Hạ tầng Token):**  
Khi người dùng nâng cấp gói Hội viên Premium thành công, hệ thống cập nhật dữ liệu và quyền lợi ra sao?
- **A.** Backend set cờ `IsPremium = true` vĩnh viễn; mọi thao tác trừ tim bị vô hiệu hóa toàn bộ hệ thống kể từ đó.
- **B.** Trạng thái premium chỉ lưu ở LocalStorage trình duyệt; server vẫn coi là tài khoản Free khi chấm bài.
- **C.** Backend tạo bảng `PremiumUsers` riêng và heartbeat mỗi 60 giây để giữ trạng thái còn hiệu lực.
- **D.** Backend ghi `PremiumUntil` vào bảng `Users`; HeartsMax nâng lên 30 và chu kỳ hồi tim rút xuống 10 phút/tim; khi gói hết hạn, lần đọc hearts kế tiếp tự động clamp về ngưỡng Free — và người dùng Premium vẫn bị trừ tim khi vào node session mới như thường.

---

### 📋 PHẦN II: TỰ LUẬN TÍCH HỢP TOÀN HỆ THỐNG (3 câu – 10.0 điểm)

**Câu TL F.1 (Hành trình Trọn vẹn của Học viên từ Khách vãng lai đến Hoàn thành Lộ trình – 3.5 điểm):**  
Hãy phân tích và vẽ sơ đồ luồng dữ liệu (Flow Diagram / 4 chặng chi tiết) cho kịch bản sau:
1. Khách truy cập Trang chủ `/` $\rightarrow$ Trải nghiệm demo Bubble Sort trên Canvas (1 trong 3 key `demoAllowed: true`).
2. Khách bấm đăng ký tài khoản mới $\rightarrow$ Điền form $\rightarrow$ Backend tạo User với 10 Tim (HeartsMax 10), 0 Gems, 0 XP và trả về phiên đăng nhập.
3. Học viên vào Lộ trình `/path` (public) $\rightarrow$ Xem danh mục khóa học từ `GET /concepts/courses` $\rightarrow$ Bấm "Tham gia lộ trình" (lưu client-side) $\rightarrow$ Vào chi tiết bài học (yêu cầu đăng nhập).
4. Học viên học bài 1 tại `/lessons/10` $\rightarrow$ Đọc lý thuyết Markdown $\rightarrow$ Làm quiz cuối bài $\rightarrow$ Bấm đánh dấu đã học (`POST /lessons/10/mark-viewed` ghi `UserProgress.Viewed` và tăng tiến độ quest `lesson_viewed`) $\rightarrow$ Sang `/quests` nhận thưởng quest để nhận Gems/XP $\rightarrow$ Topbar và Profile cập nhật Level theo công thức `1 + floor(sqrt(xp/100))`.

*Khu vực làm bài TL F.1:*
```text
...
```

**Câu TL F.2 (Hành trình Giảng viên: Tạo Lớp $\rightarrow$ Giao Bài $\rightarrow$ Sinh viên Học $\rightarrow$ Báo cáo Tiến độ – 3.5 điểm):**  
Hãy phân tích sự phối hợp giữa 2 tài khoản (Giảng viên & Sinh viên):
1. Giảng viên vào `/classes` bấm "Tạo lớp mới: DSA K18" $\rightarrow$ Backend sinh mã `DSA118`.
2. Sinh viên đăng nhập tài khoản của mình, vào `/classes` bấm "Tham gia lớp" $\rightarrow$ Nhập mã `DSA118` $\rightarrow$ Thêm vào lớp thành công.
3. Sinh viên vào node luyện tập trong bài $\rightarrow$ Bắt đầu phiên luyện bị trừ 1 tim (UPDATE atomic; node đã PASS thì vào lại miễn phí) $\rightarrow$ Làm Bậc 1 Quiz rồi tới Bậc 3 nộp code lên server Jint chấm với Test Case ẩn; khi hết tim, sinh viên chờ hồi 30 phút/tim hoặc nâng Premium (tối đa 30 tim, hồi 10 phút/tim).
4. Giảng viên mở `/classes/:id/report` $\rightarrow$ Xem bảng phân tích tỷ lệ hoàn thành của sinh viên và bấm xuất file tải bảng điểm về máy (CSV sinh phía client).

*Khu vực làm bài TL F.2:*
```text
...
```

**Câu TL F.3 (Hành trình Quản trị & Vận hành: Duyệt Giáo viên $\rightarrow$ Giám sát Thống kê $\rightarrow$ Cài đặt Runtime – 3.0 điểm):**  
Hãy phân tích luồng quản trị hệ thống của Admin:
1. Ứng viên có role `TEACHER_PENDING` xuất hiện trong hàng đợi duyệt $\rightarrow$ Admin vào `/admin/users` tab "Chờ duyệt GV" (lọc role `TEACHER_PENDING`).
2. Admin xem hồ sơ và bấm "✅ Phê duyệt" $\rightarrow$ `POST /api/v1/users/{id}/approve-teacher` $\rightarrow$ Backend cập nhật role thành `TEACHER`.
3. Admin vào `/admin/stats` $\rightarrow$ `GET /api/v1/admin/stats` trả các chỉ số đếm (TotalUsers, TotalLessons, TotalExercises, TotalSubmissions, ActiveUsersToday...).
4. Admin vào `/admin/settings` $\rightarrow$ Đổi cấu hình Sandbox (giới hạn thời gian `SandboxSeconds`, bộ nhớ `SandboxMemoryMb`) $\rightarrow$ `PUT /api/v1/settings` $\rightarrow$ `SettingsCache` singleton invalidate và nạp lại $\rightarrow$ toàn hệ thống áp dụng ngay không cần restart.

*Khu vực làm bài TL F.3:*
```text
...
```


---

# 📘 ĐỀ 14: TRỢ GIÚP, CHÍNH SÁCH BẢO MẬT & PHẢN HỒI NGƯỜI DÙNG
**Màn hình & Files trọng tâm:** `HelpView.vue` · `PrivacyView.vue` · `FeedbackController.cs` · `CourseFeedbackController.cs` · `PublicController.cs`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 14.1 (FAQ Accordion & Accessibility trên HelpView):**  
Trên màn hình `/help` (`HelpView.vue`), danh sách FAQ được render dưới dạng accordion. Khi người dùng click vào một câu hỏi để mở/trả lời, cơ chế accessibility nào được áp dụng?
- **A.** Chuyển hướng sang trang riêng biệt cho từng câu hỏi.
- **B.** Nút trigger gắn `aria-expanded="true/false"` và `aria-controls="faq-answer-{idx}"`; phần trả lời có `id` tương ứng để screen reader liên kết đúng vùng nội dung.
- **C.** Không có thuộc tính ARIA nào; chỉ dùng CSS `display: none` để ẩn/hiện.
- **D.** Sử dụng `role="dialog"` và bắt buộc focus trap bên trong câu trả lời.

**Câu 14.2 (Validation Form Liên hệ trên HelpView):**  
Khi người dùng điền form "Liên hệ hỗ trợ" trên `HelpView.vue` và bấm gửi, validation phía Client kiểm tra những điều kiện gì trước khi gọi API?
- **A.** Tên ≥ 2 ký tự, email khớp regex `/^[^\s@]+@[^\s@]+\.[^\s@]+$/`, nội dung tin nhắn ≥ 10 ký tự; nếu sai thì gán `contactError` và chặn submit.
- **B.** Gửi thẳng lên server để backend validate toàn bộ.
- **C.** Tự động điền tên và email từ tài khoản đang đăng nhập, không cần validate.
- **D.** Chỉ kiểm tra email không được để trống.

**Câu 14.3 (Chính sách Bảo mật PrivacyView — Mục lục Neo):**  
Trên màn hình `/privacy` (`PrivacyView.vue`), mục lục (Table of Contents) ở đầu trang sử dụng cơ chế nào để cuộn mượt đến từng section?
- **A.** Sử dụng iframe nhúng nội dung PDF.
- **B.** Reload lại trang với query parameter `?section=3`.
- **C.** JavaScript `window.scrollTo()` thủ công với `setTimeout`.
- **D.** Thẻ `<a href="#sec-N">` native kết hợp với Lenis smooth-scroll (`anchors: true`); mỗi section có `id="sec-N"` tương ứng.

**Câu 14.4 (Phản hồi Bài học — FeedbackController Anti-Spam):**  
Theo `FeedbackController.cs`, khi người dùng gửi phản hồi về một bài học qua `POST /api/v1/feedback`, hệ thống ngăn chặn spam bằng cách nào?
- **A.** Giới hạn rate-limit 1 request/phút per IP.
- **B.** Bắt buộc nhập CAPTCHA trước khi gửi.
- **C.** Kiểm tra xem người dùng đã "Đánh dấu đã học" bài đó chưa (truy vấn `UserProgress.Viewed` của cặp `(userId, lessonId)`); nếu chưa thì trả về HTTP 403 Forbidden.
- **D.** Không có cơ chế chống spam nào.

**Câu 14.5 (API Công khai PublicController — Site Info):**  
Endpoint `GET /api/v1/public/site-info` trong `PublicController.cs` trả về những số liệu thống kê nào mà KHÔNG yêu cầu đăng nhập?
- **A.** Danh sách email toàn bộ người dùng.
- **B.** Số lượng Cấu trúc dữ liệu (`structures`), số lượng Giải thuật (`algorithms`), và số lượng Bài học đang active (`lessons`) — tất cả đếm từ DB qua `AsNoTracking()`.
- **C.** Mã nguồn frontend của ứng dụng.
- **D.** Token JWT của admin hệ thống.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 14.1 (Trace luồng Gửi Phản hồi Khóa học → Giảng viên Trả lời – 2.5 điểm):**  
Học viên gửi góp ý về lộ trình "Cấu trúc Cây" qua `AdminFeedbackView.vue`:
1. Học viên chọn loại phản hồi (Bug / Request / Suggestion), nhập nội dung và bấm gửi. Request `POST /api/v1/courses/feedback` gồm những trường gì?
2. Backend `CourseFeedbackController.Submit` lưu bản ghi vào bảng `CourseFeedbacks` với trạng thái mặc định là gì?
3. Giảng viên vào `AdminFeedbackView.vue`, lọc theo courseId, mở draft reply, nhập câu trả lời và đổi status thành "Resolved". Request `PUT /api/v1/courses/feedback/{id}/reply` cập nhật những cột nào?
4. UI phản hồi tức thì như thế nào sau khi save thành công?

*Khu vực làm bài TL 14.1:*
```text
...
```

**Câu TL 14.2 (Trace luồng FAQ Toggle + Contact Submit trên HelpView – 2.5 điểm):**  
Người dùng truy cập `/help`:
1. Component khởi tạo mảng `FAQS` gồm bao nhiêu câu hỏi? Nội dung lấy từ đâu (hardcode i18n hay API)?
2. Khi click vào câu hỏi thứ 3: Hàm `toggle(2)` thay đổi reactive state nào? Transition CSS nào được áp dụng?
3. Khi điền form liên hệ với email không hợp lệ và bấm gửi: Validation chặn ở đâu, biến reactive nào nhận thông báo lỗi?
4. Khi gửi thành công: Biến `contactSent` đổi giá trị ra sao và UI hiển thị gì?

*Khu vực làm bài TL 14.2:*
```text
...
```

---

# 📘 ĐỀ 15: STUDIO GIẢNG VIÊN & QUẢN LÝ NỘI DUNG HỌC TẬP
**Màn hình & Files trọng tâm:** `TeacherStudioView.vue` · `NodeHubView.vue` · `AdminLessonEditorView.vue` · `ConceptsController.cs` · `LessonsController.cs`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 15.1 (Dashboard TeacherStudioView — Tải dữ liệu tổng hợp):**  
Khi giảng viên mở `/studio` (`TeacherStudioView.vue`), component gọi đồng thời những API nào trong `loadStudioData()`?
- **A.** Lấy dữ liệu từ localStorage cache, không gọi API.
- **B.** Chỉ gọi 1 API duy nhất `GET /api/v1/studio/dashboard`.
- **C.** Gọi tuần tự từng API, chờ cái trước xong mới gọi cái sau.
- **D.** `Promise.all` gồm: `lessonsApi.fetchLessons({ page: 1, pageSize: 8 })`, `lessonsApi.fetchTopics()`, `courseApi.getCourses()`, `classStore.fetchClasses()` — mỗi call có `.catch(() => ...)` để không chặn lẫn nhau nếu 1 API lỗi.

**Câu 15.2 (Phân quyền Truy cập Studio):**  
Route `/studio` trong `router/index.ts` được cấu hình meta guard như thế nào?
- **A.** Không có guard, nhưng component tự check role và hiện nút "Mua Premium".
- **B.** `meta: { public: true }` — ai cũng vào được.
- **C.** `meta: { requiresAuth: true, roles: ['TEACHER', 'ADMIN'] }` — chỉ giảng viên và admin mới truy cập; sinh viên bị redirect về `/login`.
- **D.** Chỉ mở vào giờ hành chính (8h–17h).

**Câu 15.3 (NodeHubView — 3 Tab Lý thuyết / Luyện tập / Cheatsheet):**  
Trên màn hình `/path/:topicId/node/:nodeId` (`NodeHubView.vue`), tab "Cheatsheet" hiển thị những gì?
- **A.** Chatbot AI trả lời câu hỏi.
- **B.** Bảng độ phức tạp từ `catalogMeta.complexity` (Best/Avg/Worst/Space) + Link tham khảo Wikipedia & GeeksforGeeks lấy từ `referenceLinks.ts` theo `simKey` của node.
- **C.** Form nộp bài tập trắc nghiệm.
- **D.** Video bài giảng YouTube nhúng.

**Câu 15.4 (ConceptsController — Adapter Pattern cho FE Legacy):**  
`ConceptsController.cs` tồn tại trong codebase với mục đích gì?
- **A.** Adapter map entity nội bộ (`LearningPath`, `Lesson`, `Exercise`) sang format DTO của project VisualizationDSA gốc để FE courses/list/detail/study chạy mà không cần sửa schema; map `LearningPath → Course`, `Node → Lesson`, `Exercise MCQ → Quiz`, `Exercise CODE → Codelab`.
- **B.** Là controller chính thay thế hoàn toàn `LessonsController`.
- **C.** Endpoint công khai cho khách vãng lai xem danh sách bài học.
- **D.** Controller test dùng cho unit test.

**Câu 15.5 (Tiến độ Node trên NodeHubView — Ladder Stages):**  
Trên `NodeHubView.vue`, tiến độ hoàn thành một node bài học được xác định bởi điều kiện gì?
- **A.** Chỉ cần đọc hết lý thuyết markdown.
- **B.** Giảng viên phê duyệt thủ công.
- **C.** Xem video mô phỏng 3 lần trở lên.
- **D.** Pass đủ 3 bậc `['quiz', 'lab', 'code']` trong `LadderShell`; trạng thái pass lưu trong localStorage key `dsa-ladder-<nodeId>` và đồng bộ với submissions/progress.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 15.1 (Trace luồng Giảng viên Soạn Bài học Mới qua AdminLessonEditorView – 2.5 điểm):**  
Giảng viên vào `/admin/lessons/create`:
1. Giao diện editor chia 2 cột: Bên trái là textarea soạn Markdown, bên phải là Live Preview. Component nào render preview? KaTeX và code highlight xử lý ra sao?
2. Khi chọn `simulationKey = "sort.quick"`: Dropdown lấy danh sách key từ đâu (`engines/catalog.ts`)?
3. Bấm "Xuất bản": Request `POST /api/v1/lessons` gửi payload gồm những trường gì? Backend `LessonService.CreateAsync` kiểm tra quyền gì?
4. Sau khi tạo thành công: Router điều hướng về đâu và bài học mới xuất hiện trên cây lộ trình như thế nào?

*Khu vực làm bài TL 15.1:*
```text
...
```

**Câu TL 15.2 (Trace luồng NodeHubView Map Topic×Node → SimKey → Catalog Meta – 2.5 điểm):**  
Học viên vào `/path/1/node/3` (Topic Sắp xếp, Node 3):
1. `simKey` computed property map `topicId=1, nodeId=3` thành key nào trong `keysByTopic`? Nếu key không tồn tại trong catalog thì fallback về gì?
2. `catalogMeta` lấy metadata từ hàm nào trong `engines/catalog.ts`? Trả về những field gì (title, complexity, category)?
3. Tab "Lý thuyết" render component con nào? Dữ liệu markdown lấy từ API nào?
4. Tab "Luyện tập" nhúng `LadderShell` với props gì? Progress stages đọc từ đâu?

*Khu vực làm bài TL 15.2:*
```text
...
```

---

# 📘 ĐỀ 16: KIỂM TRA CUỐI LỘ TRÌNH & ĐIỀU HƯỚNG THÔNG MINH
**Màn hình & Files trọng tâm:** `FinalTestView.vue` · `PathRedirectView.vue` · `AdminFeedbackView.vue` · `ExercisesController.cs`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 16.1 (FinalTestView — Ngưỡng Đậu & Fallback Đề):**  
Trên màn hình `/path/:topicId/final-test` (`FinalTestView.vue`), ngưỡng đậu (`passThreshold`) là bao nhiêu % và khi backend chưa có đề final-test, hệ thống xử lý thế nào?
- **A.** Ngưỡng 50%; nếu không có đề thì hiện trang trắng.
- **B.** Ngưỡng 70%; nếu API lỗi và `allowLocalFallbacks === true`, hàm `buildLocalFinalTest()` sinh đề mẫu từ 5 algorithm đầu tiên trong `CATALOG` (category === 'algorithm'), mỗi câu 2 điểm, tổng 10 điểm.
- **C.** Ngưỡng 90%; bắt buộc phải có đề từ backend, không có fallback.
- **D.** Ngưỡng 70%; chuyển hướng về trang chủ nếu không có đề.

**Câu 16.2 (FinalTestView — Tái sử dụng QuizStage Component):**  
`FinalTestView.vue` không tự render câu hỏi mà delegate cho component con nào?
- **A.** `ExerciseView.vue` — component làm bài tập chung.
- **B.** `MiniQuiz.vue` — component quiz nhỏ trong bài học.
- **C.** `QuizStage.vue` từ `@/components/ladder/QuizStage.vue` — nhận props `exercise` và emit sự kiện `passed(scorePct)` khi hoàn thành.
- **D.** Tự render inline bằng vòng lặp `v-for`.

**Câu 16.3 (PathRedirectView — Fallback Cục bộ khi API Lỗi):**  
Trên `/path` (`PathRedirectView.vue`), khi API `GET /api/v1/gamification/learning-paths` thất bại, hệ thống hiển thị gì?
- **A.** Mảng `LOCAL_TOPICS` hardcode gồm 5 topic (Sắp xếp & Tìm kiếm, CTDL tuyến tính, Cây, Bảng băm, Đồ thị) kèm ProgressBar 0% và EmptyState nếu không có dữ liệu.
- **B.** Redirect về `/login`.
- **C.** Trang lỗi 500 Server Error.
- **D.** Hiện popup yêu cầu refresh trang.

**Câu 16.4 (AdminFeedbackView — Lọc & Tìm kiếm Phản hồi):**  
Trên `AdminFeedbackView.vue`, giảng viên/admin có thể lọc danh sách phản hồi theo những tiêu chí nào?
- **A.** Chỉ lọc theo ngày tháng.
- **B.** Không có chức năng lọc.
- **C.** Chỉ tìm kiếm theo email.
- **D.** Dropdown lọc theo Status (Tất cả / Mới / Đã đọc / Đã xử lý) + ô tìm kiếm text search theo nội dung hoặc tên người gửi; computed `filteredItems` áp dụng cả 2 filter cùng lúc.

**Câu 16.5 (FinalTestView — Toast Thông báo Kết quả):**  
Khi học viên hoàn thành Final Test, hàm `onPassed(scorePct)` hiển thị toast như thế nào?
- **A.** Nếu `scorePct >= 70`: toast success `messages.finalTest.toastPassed`; nếu < 70: toast warning `messages.finalTest.toastFailed(70, scorePct)` thông báo điểm tối thiểu cần đạt.
- **B.** Mở modal popup chặn toàn màn hình.
- **C.** Luôn hiện toast xanh "Chúc mừng!" bất kể điểm số.
- **D.** Gửi email thông báo kết quả.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 16.1 (Trace luồng Làm Final Test → Chấm điểm → Hiển thị Kết quả – 2.5 điểm):**  
Học viên vào `/path/1/final-test`:
1. `onMounted` gọi `fetchLocalFinalTest()` → nếu thành công, gán `exercise.value`; nếu fail và `allowLocalFallbacks`, gọi `buildLocalFinalTest()`. Describe cấu trúc `ExerciseDto` trả về.
2. `QuizStage` nhận exercise và render câu hỏi. Khi học viên chọn đáp án và bấm submit, component con emit sự kiện gì?
3. Hàm `onPassed(scorePct)` so sánh với `passThreshold = 70`. Nếu đạt: toast gì, màu gì? Nếu trượt: toast gì, thông báo gì?
4. Có tự động điều hướng sau khi pass không? Hay học viên phải bấm nút thủ công?

*Khu vực làm bài TL 16.1:*
```text
...
```

**Câu TL 16.2 (Trace luồng PathRedirectView Load Topics → Fallback → Điều hướng NodeHub – 2.5 điểm):**  
Người dùng vào `/path`:
1. `onMounted` gọi API nào để lấy danh sách lộ trình? Biến reactive nào quản lý loading/error state?
2. Nếu API fail: `LOCAL_TOPICS` gồm mấy phần tử? Mỗi phần tử có những field gì?
3. Khi click vào card "Cây" (topicId=3): Router điều hướng sang route nào? Component đích là gì?
4. Tại `NodeHubView`, `topicId` và `nodeId` được đọc từ đâu trong Vue Router?

*Khu vực làm bài TL 16.2:*
```text
...
```


---

# 📘 ĐỀ 17: COMPOSABLES, HIỆU ỨNG & CROSS-CUTTING CONCERNS
**Files trọng tâm:** `useSimulation.ts` · `useCodeTracePlayback.ts` · `useStructureTransition.ts` · `useSoundEffects.ts` · `useConfetti.ts` · `useLenis.ts` · `useScrollReveal.ts` · `useKeyboardShortcuts.ts` · `useDebounce.ts` · `usePagination.ts` · `useCountdown.ts` · `useConfirm.ts` · `usePixiStage.ts` · `useCosmicField.ts`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 17.1 (useSimulation — Lifecycle & Store Delegation):**  
Composable `useSimulation(key)` trong `useSimulation.ts` quản lý lifecycle của mô phỏng thuật toán như thế nào?
- **A.** Tạo Web Worker riêng để chạy thuật toán độc lập.
- **B.** Tự tạo generator và timer nội bộ, không liên quan đến Pinia store.
- **C.** Ủy quyền toàn bộ cho `simulationStore`: gọi `store.loadSim(key)` trong `onMounted`, `store.stopPlayback()` trong `onUnmounted`; trả về refs từ `storeToRefs(store)` và các action play/pause/step/jump/setSpeed/reset/toggleBreakpoint.
- **D.** Chỉ load dữ liệu, không quản lý playback.

**Câu 17.2 (useCodeTracePlayback — Sampling Frame):**  
Khi mảng `TraceEvent[]` có hơn 3000 phần tử, `useCodeTracePlayback.ts` xử lý thế nào để tránh đẩy quá nhiều frame vào UI?
- **A.** Gửi lên server để render video rồi stream về.
- **B.** Cơ chế SAMPLING: tính `step = ceil(trace.length / maxFrames)`, lấy mẫu đều `trace[0], trace[step], trace[2*step]...` và LUÔN kèm event cuối cùng → giới hạn ≤ `maxFrames + 1` frame hiển thị.
- **C.** Nén toàn bộ trace thành 1 frame duy nhất.
- **D.** Hiển thị lỗi "Quá nhiều bước" và dừng playback.

**Câu 17.3 (useSoundEffects — Dual-layer Audio Architecture):**  
Hệ thống âm thanh `useSoundEffects.ts` sử dụng kiến trúc dual-layer nào?
- **A.** Layer 1: Web Audio API procedural synthesizer (tạo âm thanh tức thì, 0 file ngoài); Layer 2: Howler.js cho master volume và fallback; preferences (muted/volume) lưu localStorage key `dsa_sfx_muted` / `dsa_sfx_volume`.
- **B.** Phát âm thanh qua Bluetooth speaker.
- **C.** Chỉ dùng Howler.js, không có Web Audio API.
- **D.** Chỉ dùng file MP3 tải từ CDN.

**Câu 17.4 (useConfetti — Canvas Singleton & Reduced Motion):**  
`useConfetti.ts` tuân thủ accessibility và performance bằng cách nào?
- **A.** Dùng CSS animation thay vì canvas.
- **B.** Luôn bắn confetti bất kể setting người dùng.
- **C.** Singleton canvas FIXED dùng chung (`pointer-events: none`), lazy init; tôn trọng `prefers-reduced-motion`: nếu user bật giảm chuyển động thì KHÔNG bắn confetti; hỗ trợ 4 loại: success/levelup/achievement/node-pass với bảng màu riêng.
- **D.** Tạo canvas mới mỗi lần bắn pháo hoa.

**Câu 17.5 (useKeyboardShortcuts — Editable Target Guard):**  
`useKeyboardShortcuts.ts` ngăn chặn phím tắt xung đột với ô nhập liệu bằng cơ chế gì?
- **A.** Tắt toàn bộ phím tắt khi form mở.
- **B.** Chuyển focus ra khỏi ô nhập trước khi xử lý phím.
- **C.** Không có cơ chế bảo vệ, phím tắt luôn hoạt động.
- **D.** Hàm `isEditableTarget(target)` kiểm tra `tagName === 'INPUT' | 'TEXTAREA' | 'SELECT' || isContentEditable`; nếu true thì bỏ qua sự kiện, KHÔNG gọi handler và KHÔNG `preventDefault()`.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 17.1 (Trace luồng useCodeTracePlayback: TraceEvent[] → Structure Frame → Playback – 2.5 điểm):**  
Người dùng viết code Bubble Sort, bấm "Chạy thử", sinh ra 5000 TraceEvent:
1. Hàm `buildStructureFromTrace()` chuyển mỗi TraceEvent thành Structure frame kind gì? Element id format nào (`cell:<i>`)?
2. Khi `trace.length > maxFrames (3000)`: Sampling step tính ra sao? Frame cuối cùng có luôn được giữ không? Tại sao?
3. Status mapping: event kind='swap' → element status gì? kind='compare' → status gì? Event CUỐI cùng → tất cả elements nhận status gì?
4. Playback dùng `setInterval` hay `requestAnimationFrame`? Tại sao chọn cơ chế đó? `dispose()` dọn gì khi component unmount?

*Khu vực làm bài TL 17.1:*
```text
...
```

**Câu TL 17.2 (Trace luồng useLenis Smooth Scroll + useScrollReveal IntersectionObserver – 2.5 điểm):**  
Trên trang PrivacyView và HomeView:
1. `useLenis` tạo instance Lenis singleton ở đâu? Option `anchors: true` giúp gì cho PrivacyView TOC? `allowNestedScroll: true` bảo vệ component nào khỏi bị phá scroll?
2. Khi user bật `prefers-reduced-motion`: Lenis thay đổi hành vi cuộn ra sao (`lerp` = ?)? `useScrollReveal` xử lý `isVisible` thế nào?
3. `useScrollReveal(target, { once: true })`: Khi element vào viewport, `isVisible` đổi giá trị gì? Observer có disconnect sau lần đầu không?
4. `scrollTo('#sec-3')` trong PrivacyView: Lenis xử lý anchor link như thế nào so với native browser scroll?

*Khu vực làm bài TL 17.2:*
```text
...
```


---

# 📘 ĐỀ 18: VISUALIZATION ENGINE CORE — GENERATOR, RENDERER & STEP EXECUTOR
**Files trọng tâm:** `engines/core/types.ts` · `engines/registry.ts` · `engines/catalog.ts` · `engines/generators/sort/bubble.ts` · `engines/core/stepExecutor.ts` · `engines/renderers/arrayRenderer.ts` · `engines/renderers/painter/canvasPainter.ts` · `engines/renderers/canvasTheme.ts`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 18.1 (Registry Pattern — Đăng ký & Truy xuất Generator):**  
Trong `engines/registry.ts`, hàm `getSimulation(key)` hoạt động theo cơ chế nào?
- **A.** Import tĩnh module tương ứng bằng dynamic import.
- **B.** Lấy `GeneratorFactory` từ Map, gọi factory để sinh instance MỚI mỗi lần; nếu key không tồn tại thì trả `undefined`.
- **C.** Trả về singleton instance dùng chung cho toàn bộ ứng dụng.
- **D.** Gọi API backend để tải generator từ server.

**Câu 18.2 (Catalog — Đồng bộ Key với CI):**  
File `engines/catalog.ts` đảm bảo tính nhất quán giữa danh mục mô phỏng và shared catalog JSON bằng cách nào?
- **A.** CI chạy test `engines/__tests__/catalog.spec.ts` so sánh 44 key trong `catalog.ts` với `shared/simulation-catalog.json`; khác → fail build. Tất cả 44 generator đăng ký tập trung tại đây, KHÔNG rải rác.
- **B.** Không có kiểm tra, developer tự đảm bảo thủ công.
- **C.** Backend validate key khi frontend gọi API.
- **D.** Dùng TypeScript enum để ép kiểu compile-time.

**Câu 18.3 (Bubble Sort Generator — InputSchema & Presets):**  
Generator Bubble Sort trong `engines/generators/sort/bubble.ts` hỗ trợ những preset dữ liệu đầu vào nào qua `InputSchema`?
- **A.** Tải dataset từ server.
- **B.** 6 preset: random, sorted-asc, sorted-desc, nearly-sorted, all-equal, custom; kèm fields values, size, minValue, maxValue, allowDuplicates.
- **C.** Chỉ có 1 chế độ ngẫu nhiên duy nhất.
- **D.** Người dùng phải viết code JavaScript để tạo dữ liệu.

**Câu 18.4 (StepExecutor — Babel AST Instrumentation):**  
`engines/core/stepExecutor.ts` sử dụng thư viện nào để chèn trace hook vào code người dùng, và tại sao chọn nó thay vì regex-based approach?
- **A.** `esprima` — tương thích ES5 tốt hơn.
- **B.** Không dùng parser, chỉ replace string trực tiếp.
- **C.** `acorn` — nhẹ hơn, parse nhanh hơn.
- **D.** `@babel/parser` — chèn `__trackLine`/`__loopTick` theo AST, giữ đúng block scope và đệ quy; V1 regex phá block scope và lỗi khi đệ quy.

**Câu 18.5 (ArrayRenderer — Bar Mode & Wrap):**  
`engines/renderers/arrayRenderer.ts` chuyển sang chế độ Bar Mode khi nào và xử lý mảng dài ra sao?
- **A.** Chuyển sang chế độ text-only khi mảng > 50 phần tử.
- **B.** Luôn vẽ ô vuông 60×60 bất kể kích thước mảng.
- **C.** Bar Mode khi mọi label là số → vẽ bar cao tỉ lệ giá trị từ đáy canvas (gradient + glow); WRAP khi mảng dài (slotW < 44 hoặc cellSize < 36) → chia nhiều hàng, index toàn cục 0..n-1.
- **D.** Gửi lên server render rồi nhận ảnh PNG về.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 18.1 (Trace luồng Generator Pipeline: InputConfig → Step[] → Canvas Render – 2.5 điểm):**  
Người dùng mở SimulatorView, chọn Bubble Sort, nhập mảng [5, 3, 8, 1]:
1. `simulationStore.loadSim('sort.bubble', input)` gọi hàm nào trong `registry.ts` để lấy generator? Generator được tạo mới hay dùng lại?
2. `generator.generate(input)` trả về mảng `Step[]`. Mỗi Step gồm những field gì (`structure`, `explanation`, `pseudocodeLine`, `highlights`, `stats`)?
3. `CanvasArea` nhận `currentStep.structure` và delegate cho renderer nào? `ArrayRenderer.render()` đọc `Element.status` để quyết định màu sắc qua hàm nào trong `canvasPainter.ts`?
4. Khi bấm Step Forward: composable `useSimulation` cập nhật `currentIndex`, trigger re-render như thế nào?

*Khu vực làm bài TL 18.1:*
```text
...
```

**Câu TL 18.2 (Trace luồng StepExecutor: User Code → Babel Instrument → TraceEvent[] – 2.5 điểm):**  
Học viên viết code Bubble Sort trong CodeRunnerView và bấm "Chạy thử":
1. `codeStore.run()` gọi `runCode(codeSim, defaultArray)` trong `stepExecutor.ts`. Hàm `compileJavaScript()` dùng `@babel/parser` chèn những hook nào vào AST?
2. Khi code thực thi, mỗi hook ghi nhận `TraceEvent` gồm những field gì (`line`, `vars`, `highlight`, `kind`, `explanation`)? Giới hạn tối đa bao nhiêu event?
3. Nếu code vòng lặp vô hạn: cơ chế nào ngăn chặn (`MAX_STEPS`, `MAX_LOOP_ITERATIONS`, timeout)? Response trả về `error` object có cấu trúc gì?
4. `RunResult.stats` tổng hợp những counter nào (`comparisons`, `swaps`, `writes`, `durationMs`)?

*Khu vực làm bài TL 18.2:*
```text
...
```

---

# 📘 ĐỀ 19: API LAYER, ROUTER & FRONTEND ARCHITECTURE
**Files trọng tâm:** `api/client.ts` · `api/auth.ts` · `api/lessons.ts` · `api/exercises.ts` · `api/gamification.ts` · `api/types.ts` · `router/index.ts`

### 📝 PHẦN I: TRẮC NGHIỆM THỰC CHIẾN (5 câu – 5.0 điểm)

**Câu 19.1 (Axios Client — Error Handling & Interceptors):**  
Trong `api/client.ts`, khi response trả về HTTP 401, interceptor xử lý theo trình tự nào?
- **A.** Hiển thị popup yêu cầu người dùng nhập lại mật khẩu.
- **B.** Logout ngay lập tức và redirect về /login.
- **C.** Tự động tạo tài khoản guest và tiếp tục.
- **D.** Kiểm tra cờ `_retry`: nếu chưa retry → gọi `auth.refresh()` (singleton promise), gắn token mới vào header, retry request gốc; nếu refresh fail hoặc đã retry → logout → redirect `/login?redirect=` (chống redirect storm bằng cờ `redirectedToLogin`).

**Câu 19.2 (ApiError Class — Structured Error Body):**  
Class `ApiError` trong `api/client.ts` parse error body từ backend theo format chuẩn nào?
- **A.** XML response.
- **B.** `{ error: { code, message, field?, details? } }` — map vào các property `code`, `message`, `field`, `status`, `retryAfterSeconds` (từ header Retry-After khi 429).
- **C.** Plain text message.
- **D.** HTML error page.

**Câu 19.3 (Router — Lazy Loading Strategy):**  
Trong `router/index.ts`, những route nào được lazy-load bằng dynamic import và tại sao?
- **A.** Lazy-load dựa trên vai trò người dùng.
- **B.** Chỉ admin pages được lazy-load.
- **C.** Các trang lớn (SimulatorView, ExerciseView, Admin/*, CodeRunnerView, BenchmarkView, LessonView...) dùng `() => import(...)` để code-splitting; chỉ HomeView và LoginView import tĩnh vì cần load nhanh nhất.
- **D.** Tất cả route đều import tĩnh.

**Câu 19.4 (PagedResponse — Chuẩn Phân Trang):**  
Interface `PagedResponse<T>` trong `api/types.ts` định nghĩa response phân trang chuẩn gồm những field gì?
- **A.** `{ items: T[], page: number, pageSize: number, total: number, totalPages: number }` — dùng thống nhất cho lessons, exercises, leaderboard, admin users.
- **B.** Mỗi API module tự định nghĩa format riêng.
- **C.** `{ data: T[], count: number }`.
- **D.** `{ results: T[], next: string | null }` (cursor-based).

**Câu 19.5 (Gamification API — Quest Progress Mapping):**  
Trong `api/gamification.ts`, DTO `RawQuestDto` từ backend khác `QuestDto` hiển thị ở frontend như thế nào?
- **A.** Giống hệt nhau, không cần mapping.
- **B.** Backend trả `progress`/`target`/`reward:{gems,xp}`; frontend map thành `current`/`target`/`rewardGems`/`rewardXp` để UI dễ binding.
- **C.** Frontend tự tính progress từ localStorage.
- **D.** Backend trả HTML rendered sẵn.

---

### 📋 PHẦN II: TỰ LUẬN TÌNH HUỐNG TRACE LUỒNG (2 câu – 5.0 điểm)

**Câu TL 19.1 (Trace luồng Axios Request Lifecycle: Auth Token → API Call → Error Recovery – 2.5 điểm):**  
Người dùng đã đăng nhập, mở trang LessonsList:
1. `lessonsApi.fetchLessons({ page: 1, pageSize: 20 })` gọi `getData()` trong `client.ts`. Request interceptor gắn header gì từ `authStore.accessToken`?
2. Nếu accessToken hết hạn, backend trả 401. Response interceptor phát hiện và xử lý thế nào? Mô tả singleton refresh promise pattern.
3. Sau khi refresh thành công: request gốc được retry với header mới. Nếu refresh fail: `authStore.logout()` làm gì và redirect về đâu?
4. Cờ `redirectedToLogin` ngăn chặn vấn đề gì khi có nhiều request 401 song song?

*Khu vực làm bài TL 19.1:*
```text
...
```

**Câu TL 19.2 (Trace luồng Exercise Submit Flow: Frontend → API → Backend Response – 2.5 điểm):**  
Học viên làm bài quiz MCQ và bấm nộp:
1. `ExerciseView.vue` thu thập answers, gọi `exercisesApi.submit(exerciseId, { answers })`. Payload `SubmitRequest` có cấu trúc gì?
2. Backend chấm điểm, trả `SubmitResultDto` gồm những field (`score`, `maxScore`, `results[]` với `correct`, `correctAnswer`, `explanation`)?
3. Tim bị trừ ở thời điểm nào trong luồng học (nếu không phải ở bước nộp bài)? Cơ chế UPDATE atomic chống double-spend hoạt động ra sao?
4. Khi hết tim (Hearts = 0), học viên bấm vào node mới: backend trả lỗi gì (mã lỗi + HTTP status) và frontend hiển thị cảnh báo thế nào?

*Khu vực làm bài TL 19.2:*
```text
...
```

