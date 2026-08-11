# THƯ VIỆN MÃ NGUỒN MỞ (THIRD_PARTY)

**Hệ thống hỗ trợ học tập và trực quan hóa cấu trúc dữ liệu và giải thuật (DSA-Visual)**

| | |
|---|---|
| Loại tài liệu | Danh sách thư viện mã nguồn mở + license |
| Phiên bản | 1.0 |
| Ngày cập nhật | 12/08/2026 |
| Trạng thái | Dự thảo |
| Người soạn | Trần Viết Tâm Phúc |
| Người duyệt | Phạm Ngọc Ái Liên |
| Tài liệu liên quan | README.md (root), package.json, backend/src/DsaVisual.Api/*.csproj |
| Nguồn yêu cầu | NFR-36 (chỉ dùng thư viện mã nguồn mở — liệt kê license), PRODUCTION_PROMPT.md §17.1 file 11 |
| Giả định chính | 1) Danh sách cập nhật theo `package.json` / `.csproj` thực tế khi thêm dependency. 2) Không sử dụng thư viện license thương mại trả phí. 3) Phiên bản trong bảng là gợi ý — xác nhận theo bản cài đặt thật. |

## Lịch sử thay đổi

| Phiên bản | Ngày | Người sửa | Mô tả thay đổi |
|---|---|---|---|
| 1.0 | 12/08/2026 | Trần Viết Tâm Phúc | Tạo mới (danh sách thư viện + license); bổ sung front matter đầy đủ theo khuôn §17.11 + Lịch sử thay đổi (vá review) |

> Quy tắc: danh sách này cập nhật theo `package.json` (frontend) và `.csproj` (backend) — bổ sung khi thêm dependency; cấm dùng thư viện license thương mại trả phí.

---

# 1. FRONTEND (NPM)

| Thư viện | Phiên bản (gợi ý) | Mục đích | License |
|---|---|---|---|
| vue | 3.4+ | Framework SPA | MIT |
| pinia | 2.1+ | Quản lý trạng thái | MIT |
| vue-router | 4.3+ | Định tuyến | MIT |
| vite | 5.x | Build tool | MIT |
| typescript | 5.x | Ngôn ngữ + typecheck | Apache-2.0 |
| axios | 1.6+ | HTTP client | MIT |
| vue-i18n | 9+ | i18n (chuẩn bị đa ngôn ngữ) | MIT |
| monaco-editor | 0.4x | Trình soạn mã (Module I) | MIT |
| lucide-vue-next | 0.3xx | Icon SVG | ISC |
| chart.js | 4.x | Biểu đồ (Màn 08, 17, báo cáo) | MIT |
| quill | 2.x | Rich text editor (soạn bài giảng viên) | BSD-3-Clause |
| dompurify | 3.x | Sanitize HTML phía client | Apache-2.0 / MPL-2.0 (dual) |
| vitest | 1.x | Unit test | MIT |
| @vue/test-utils | 2.x | Test component Vue | MIT |
| @playwright/test | 1.x | E2E test | Apache-2.0 |
| eslint / prettier | 8.x / 3.x | Lint + format | MIT |
| vue-tsc | 2.x | Typecheck SFC | MIT |
| katex (tùy chọn) | 0.16 | Công thức toán trong bài học | MIT |

# 2. BACKEND (NUGET)

| Thư viện | Phiên bản (gợi ý) | Mục đích | License |
|---|---|---|---|
| Microsoft.EntityFrameworkCore | 8.x | ORM | MIT |
| Microsoft.EntityFrameworkCore.SqlServer | 8.x | Provider SQL Server | MIT |
| Microsoft.EntityFrameworkCore.Sqlite | 8.x | Provider SQLite (dev) | MIT |
| Microsoft.AspNetCore.Authentication.JwtBearer | 8.x | Xác thực JWT | MIT |
| Asp.Versioning.Http | 8.x | API versioning | MIT |
| FluentValidation | 11.x | Validation DTO | Apache-2.0 |
| Ganss.Xss | 1.5+ | Sanitize HTML | MIT |
| Serilog.AspNetCore | 8.x | Ghi log cấu trúc | Apache-2.0 |
| Serilog.Sinks.File | 5.x | Log ra file | Apache-2.0 |
| Serilog.Sinks.Console | 5.x | Log ra console | Apache-2.0 |
| BCrypt.Net-Next | 4.x | Hash mật khẩu (bcrypt cost 12) | MIT |
| MailKit (tùy chọn) | 4.x | Gửi email SMTP | MIT |
| xunit | 2.x | Unit test | Apache-2.0 |
| FluentAssertions | 6.x | Assertion test | Apache-2.0 |
| Microsoft.AspNetCore.Mvc.Testing | 8.x | Integration test (WebApplicationFactory) | MIT |
| Testcontainers | 3.x | SQL Server container cho test | MIT |

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

> Ghi chú: mọi thư viện trên là mã nguồn mở, không phát sinh chi phí bản quyền (NFR-36, ràng buộc ngân sách §2.4). Cập nhật danh sách theo phiên bản thực tế khi cài đặt (`npm ls` / `dotnet list package`).
