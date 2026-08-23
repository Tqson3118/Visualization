# THƯ VIỆN MÃ NGUỒN MỞ (THIRD_PARTY)

**Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật (DSA-Visual)**

| | |
|---|---|
| Loại tài liệu | Danh sách thư viện mã nguồn mở + license |
| Phiên bản | 1.3 |
| Ngày cập nhật | 13/08/2026 |
| Trạng thái | Dự thảo |
| Người soạn | Trần Viết Tâm Phúc |
| Người duyệt | Phạm Ngọc Ái Liên |
| Tài liệu liên quan | README.md (root), frontend/package.json, backend/src/DsaVisual.Api/*.csproj, backend/src/DsaVisual.Application/*.csproj, backend/tests/*/*.csproj |
| Nguồn yêu cầu | NFR-36 (chỉ dùng thư viện mã nguồn mở — liệt kê license), PRODUCTION_PROMPT.md §17.1 file 11 |
| Giả định chính | 1) Danh sách cập nhật theo `package.json` / `.csproj` thực tế khi thêm dependency. 2) Không sử dụng thư viện license thương mại trả phí. 3) Phiên bản xác nhận theo lệnh thật ngày 12/08/2026 (`npm ls --depth=0` cho frontend, `dotnet list package` cho 4 project backend). |

## Lịch sử thay đổi

| Phiên bản | Ngày | Người sửa | Mô tả thay đổi |
|---|---|---|---|
| 1.4 | 15/08/2026 | Trần Viết Tâm Phúc | Đợt chấm code server (nghiệp vụ ASM): bổ sung `Jint` 4.16.0 (BSD-2-Clause — JS interpreter cho máy chủ chấm bài code Assignment/Kiểm tra cuối, `CodelabJudgeService`) |
| 1.3 | 13/08/2026 | Trần Viết Tâm Phúc | GP-T8 (đồng bộ GP-T7 — Premium QR MB Bank): bổ sung `qrcode` 1.5.4 + `@types/qrcode` 1.5.6 (sinh QR VietQR EMVCo cho checkout Premium — license MIT xác nhận từ `node_modules/<gói>/package.json` đã cài + `npm ls`); tổng 41 gói top-level |
| 1.1 | 12/08/2026 | Trần Viết Tâm Phúc | Cập nhật phiên bản thật theo `npm ls` + `dotnet list package` (đợt F): thay phiên bản ước lượng bằng số chính xác, xóa thư viện không cài, bổ sung thư viện mới (Testcontainers.MsSql, HtmlSanitizer, Swashbuckle, coverlet...), xác nhận license từ nuspec/package.json đã cài |
| 1.0 | 12/08/2026 | Trần Viết Tâm Phúc | Tạo mới (danh sách thư viện + license); bổ sung front matter đầy đủ theo khuôn §17.11 + Lịch sử thay đổi (vá review) |

> Quy tắc: danh sách này cập nhật theo `package.json` (frontend) và `.csproj` (backend) — bổ sung khi thêm dependency; cấm dùng thư viện license thương mại trả phí.

---

# 1. FRONTEND (NPM)

Nguồn: `npm ls --depth=0` chạy tại `frontend/` ngày 13/08/2026 (41 gói top-level — đợt G bổ sung 19 gói stack UI/UX + GP-T8 bổ sung `qrcode`/`@types/qrcode`). License trích từ `node_modules/<gói>/package.json` bản đã cài; vaul-vue không có license field trong package.json → xác nhận từ GitHub LICENSE (MIT).

| Thư viện | Phiên bản (thật) | Mục đích | License |
|---|---|---|---|
| vue | 3.5.41 | Framework SPA | MIT |
| pinia | 3.0.4 | Quản lý trạng thái | MIT |
| vue-router | 4.6.4 | Định tuyến | MIT |
| axios | 1.19.0 | HTTP client | MIT |
| canvas-confetti | 1.9.4 | Hiệu ứng confetti (hoàn thành bài/challenge) | ISC |
| @monaco-editor/loader | 1.7.0 | Loader Monaco Editor (trình soạn mã Module I, tải runtime) | MIT |
| @babel/parser | 7.29.8 | Parse mã nguồn (hỗ trợ phân tích mã trong bài giảng) | MIT |
| @babel/types | 7.29.8 | AST types cho Babel | MIT |
| @types/canvas-confetti | 1.9.0 | Type definitions cho canvas-confetti (dev) | MIT |
| @types/node | 24.13.3 | Type definitions cho Node.js (dev) | MIT |
| qrcode | 1.5.4 | Sinh mã QR VietQR EMVCo cho Premium checkout (GP-T7) | MIT |
| @types/qrcode | 1.5.6 | Type definitions cho qrcode (dev, GP-T7) | MIT |
| vite | 8.2.1 | Build tool (dev) | MIT |
| @vitejs/plugin-vue | 6.0.8 | Plugin Vue cho Vite (dev) | MIT |
| typescript | 6.0.3 | Ngôn ngữ + typecheck (dev) | Apache-2.0 |
| vue-tsc | 3.3.9 | Typecheck SFC (dev) | MIT |
| @vue/tsconfig | 0.9.1 | Base tsconfig cho Vue (dev) | MIT |
| vitest | 4.1.10 | Unit test (dev) | MIT |
| jsdom | 29.1.1 | DOM environment cho test (dev) | MIT |
| @vue/test-utils | 2.4.11 | Test component Vue (dev) | MIT |
| @playwright/test | 1.62.1 | E2E test (dev) | Apache-2.0 |
| @webgpu/types | 0.1.71 | Type definitions WebGPU (dev) | BSD-3-Clause |
| tailwindcss | 4.3.3 | Framework CSS (Tailwind 4 — CSS-first, đợt G) | MIT |
| @tailwindcss/vite | 4.3.3 | Plugin Tailwind cho Vite (đợt G) | MIT |
| tw-animate-css | 1.4.0 | CSS animation tiện ích cho Tailwind (đợt G) | MIT |
| shadcn-vue | 2.8.2 | Component library (wrapper UI, đợt G) | MIT |
| reka-ui | 2.10.3 | Primitives headless UI cho shadcn-vue (đợt G) | MIT |
| class-variance-authority | 0.7.1 | Variant API cho component (đợt G) | Apache-2.0 |
| clsx | 2.1.1 | Ghép class điều kiện (đợt G) | MIT |
| tailwind-merge | 3.6.0 | Merge class xung đột Tailwind (đợt G) | MIT |
| @lucide/vue | 1.31.0 | Icon set Lucide cho Vue (đợt G) | ISC |
| lucide-vue-next | 1.0.0 | Icon set Lucide cho Vue (đợt G) | ISC |
| @phosphor-icons/vue | 2.2.1 | Icon set Phosphor cho Vue (đợt G) | MIT |
| motion-v | 2.3.0 | Animation (Framer Motion port cho Vue — page transition/hover, đợt G) | MIT |
| gsap | 3.15.0 | Animation engine (canvas/simulator, đợt G) | Standard "no charge" license (gsap.com/standard-license) |
| vue-echarts | 8.1.0 | Component biểu đồ ECharts cho Vue (đợt G) | MIT |
| echarts | 6.1.0 | Thư viện biểu đồ (lazy-load chunk riêng, đợt G) | Apache-2.0 |
| lenis | 1.3.26 | Smooth scroll (đợt G) | MIT |
| vue-sonner | 2.0.9 | Toast notification (thay ToastContainer tự xây, đợt G) | MIT |
| @vueuse/core | 14.4.0 | Composables utility Vue (đợt G) | MIT |
| vaul-vue | 0.4.1 | Drawer primitive (đợt G, license từ GitHub LICENSE) | MIT |

# 2. BACKEND (NUGET)

Nguồn: `dotnet list package` chạy trên 4 project (Api, Application, UnitTests, IntegrationTests) ngày 12/08/2026 (22 gói top-level duy nhất). License trích từ `.nuspec` bản đã cài trong NuGet cache. Dự án: Api = DsaVisual.Api, App = DsaVisual.Application, UT = DsaVisual.UnitTests, IT = DsaVisual.IntegrationTests.

| Thư viện | Phiên bản (thật) | Dự án | Mục đích | License |
|---|---|---|---|---|
| Asp.Versioning.Http | 10.2.1 | Api | API versioning (HTTP/URL) | MIT |
| Asp.Versioning.Mvc | 10.2.1 | Api | API versioning (controller) | MIT |
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.11 | Api | Xác thực JWT | MIT |
| Microsoft.AspNetCore.OpenApi | 10.0.11 | Api | OpenAPI metadata | MIT |
| Microsoft.EntityFrameworkCore.Design | 10.0.11 | Api | Công cụ design-time EF Core (migration) | MIT |
| Serilog.AspNetCore | 10.0.0 | Api | Ghi log cấu trúc (HTTP + console sink transitive) | Apache-2.0 |
| Serilog.Sinks.File | 7.0.0 | Api | Log ra file | Apache-2.0 |
| Swashbuckle.AspNetCore.SwaggerUI | 10.2.3 | Api | Giao diện Swagger UI | MIT |
| FluentValidation | 12.1.1 | App | Validation DTO | Apache-2.0 |
| HtmlSanitizer | 9.2.995 | App | Sanitize HTML (kế thừa dự án Ganss.Xss) | MIT |
| Microsoft.EntityFrameworkCore.SqlServer | 10.0.11 | App | Provider SQL Server (ORM EF Core) | MIT |
| Jint | 4.16.0 | App | JS interpreter thuần .NET — chấm code bài ASM phía máy chủ (Jint sandbox: timeout/max statements/memory/stack guard) | BSD-2-Clause |
| Microsoft.Extensions.Configuration.Binder | 10.0.11 | App | Bind cấu hình | MIT |
| Microsoft.Extensions.Hosting.Abstractions | 10.0.11 | App | Hosting abstractions | MIT |
| Microsoft.Extensions.Logging.Abstractions | 10.0.11 | App | Logging abstractions | MIT |
| Microsoft.EntityFrameworkCore.InMemory | 10.0.11 | UT | In-memory provider (unit test) | MIT |
| Microsoft.EntityFrameworkCore.Sqlite | 10.0.11 | UT | Provider SQLite (unit test) | MIT |
| coverlet.collector | 6.0.4 | UT, IT | Thu thập code coverage | MIT |
| Microsoft.NET.Test.Sdk | 17.14.1 | UT, IT | Test SDK | MIT |
| xunit | 2.9.3 | UT, IT | Unit/integration test framework | Apache-2.0 |
| xunit.runner.visualstudio | 3.1.4 | UT, IT | Test runner (VSTest) | Apache-2.0 |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.11 | IT | Integration test (WebApplicationFactory) | MIT |
| Testcontainers.MsSql | 4.13.0 | IT | SQL Server container cho integration test | MIT |

# 3. HẠ TẦNG & CÔNG CỤ

| Công cụ | Mục đích | License |
|---|---|---|
| SQL Server Express 2019+ | CSDL (production đề xuất) | Free (bản quyền Microsoft — miễn phí cho giáo dục) |
| Nginx | Reverse proxy + static files | BSD-2-Clause |
| Docker / docker-compose | Môi trường dev chuẩn (SQL Server + MailHog) | Apache-2.0 |
| MailHog | SMTP mock cho dev/staging | MIT |
| k6 | Load test | AGPL-3.0 (dùng CLI miễn phí) |
| GitHub Actions | CI/CD | — |
| Pandoc | Sinh báo cáo Word | GPL-2.0 |

> Ghi chú license đáng chú ý: k6 (AGPL-3.0) và Pandoc (GPL-2.0) là copyleft — chỉ dùng ở dạng CLI công cụ, không nhúng vào sản phẩm, không ảnh hưởng license mã nguồn của dự án. **GSAP 3.15.0 (đợt G)** dùng *Standard "no charge" license* của GreenSock (miễn phí, không phải OSI license nhưng không tính phí — thỏa NFR-36 "không thư viện thương mại trả phí"); ghi chú trên package.json bản đã cài. Các gói còn lại dùng MIT / Apache-2.0 / BSD / ISC — thân thiện thương mại.

# 4. KẾT LUẬN

- Mọi thư viện liệt kê ở trên là mã nguồn mở (hoặc miễn phí sử dụng), không phát sinh chi phí bản quyền → **NFR-36 đạt** (không có thư viện thương mại trả phí nào được cài).
- **Ghi chú nguồn số liệu (đợt G, ngày chạy 12/08/2026):**
  - Frontend: `npm ls --depth=0` (workdir `frontend/`) — **41 gói top-level** (đợt F: 20 → đợt G: +19 gói stack UI/UX: tailwindcss 4 + @tailwindcss/vite + tw-animate-css + shadcn-vue + reka-ui + class-variance-authority + clsx + tailwind-merge + @lucide/vue + lucide-vue-next + @phosphor-icons/vue + motion-v + gsap + vue-echarts + echarts + lenis + vue-sonner + @vueuse/core + vaul-vue → GP-T8: +2 gói `qrcode` + `@types/qrcode` — GP-T7, QR VietQR EMVCo).
  - Backend: `dotnet list backend/src/DsaVisual.Api/DsaVisual.Api.csproj package` (8 gói), `dotnet list backend/src/DsaVisual.Application/DsaVisual.Application.csproj package` (6 gói), `dotnet list backend/tests/DsaVisual.UnitTests/DsaVisual.UnitTests.csproj package` (6 gói), `dotnet list backend/tests/DsaVisual.IntegrationTests/DsaVisual.IntegrationTests.csproj package` (6 gói) — tổng 22 gói duy nhất.
  - License: trích từ `frontend/node_modules/<gói>/package.json` và `.nuspec` trong NuGet cache (`C:\Users\Administrator\.nuget\packages\<gói>\<phiên bản>\`) của bản đã cài; vaul-vue xác nhận từ GitHub LICENSE (`Elliot-Alexander/vaul-vue`, MIT) vì package.json không có license field; **GP-T8 (13/08/2026)**: `qrcode@1.5.4` + `@types/qrcode@1.5.6` xác nhận license MIT từ `node_modules/<gói>/package.json` bản đã cài.
  - Cập nhật danh sách theo phiên bản thực tế khi cài đặt (`npm ls` / `dotnet list package`).
