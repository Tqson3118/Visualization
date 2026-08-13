# PROMPT_J_BACKEND_AUDIT — Audit bảo mật + business logic + hiệu suất backend

Dán vào `/pm "..." --auto`.

📌 **BÀI HỌC ĐÃ ĐÚC KẾT (bắt buộc đọc trước khi dispatch):**
- **LỖI "TASK TRẢ RỖNG" (13/08 — 4/4 lần)**: KHÔNG nhúng file >5KB vào prompt task (style-guide 21KB làm subagent vượt context → trả rỗng). Prompt task CHỈ trỏ đường dẫn file, agent tự đọc. Quy trình khi subagent trả rỗng: (1) bỏ nhúng file dài → thay đường dẫn; (2) task test siêu nhỏ xác nhận tool hoạt động; (3) tách nhỏ task; (4) resume session cũ — không tạo task trùng; (5) 2 lần fail → ghi FAIL + lý do.
- **ĐÃ BIẾT 1 LỖ HỔNG THẬT (ghi trước khi audit — BUG-1 trong `docs/work/audit-notes-backend.md`)**: **job downgrade Premium KHÔNG tồn tại** — `GamificationService.cs:840-844` `HeartConfig()` trả `user.HeartsMax` (cột DB vẫn 30) khi hết hạn; SRS FR-10.7 yêu cầu clamp về 10. Sửa: hoặc (a) hosted service định kỳ query `Users WHERE PremiumUntil < now AND HeartsMax = 30` → set 10 + `UPDATE Hearts = MIN(Hearts,10)` atomic; hoặc (b) lazy fix trong HeartConfig + migration — ghi rõ quyết định. Kèm test tái hiện (bắt buộc theo BƯỚC 2).

⚠ **RÀNG BUỘC CHẠY** (điều chỉnh nếu tình trạng nhánh khác lúc chạy): nếu có agent khác đang chạy song song trên `dev` → làm việc LOCAL, không tạo nhánh mới, không merge `dev`; commit (nếu có) vào nhánh `feature/backend-audit` (tạo nếu chưa có) hoặc để local. Đầu ra đặt trong `docs/work/backend-audit/`. Ghi log vào `docs/pm-report-backend.md` + quyết định vào `docs/pm-decision-log-backend.md`.

## BỐI CẢNH DỰ ÁN

ASP.NET Core 10, 2 project (Api + Application), Service gọi thẳng `DbContext` (**cấm Repository pattern** — giữ nguyên kiến trúc hiện tại, không đề xuất thêm Repository). Đã có Result/ErrorCodes pattern cho lỗi kỳ vọng. 14 controller + 13 service, EF Core + SQL Server, 32 bảng (gồm module gamification: XP/gems/hearts, NodeSessions, PremiumSubscriptions).

Mục tiêu: **backend chạy mượt, không để lộ ngoại lệ ra client, và hạn chế tối đa lỗi business logic** — đặc biệt các chỗ có thể mất tiền/mất dữ liệu người dùng (XP/gems/hearts âm, double-submit, downgrade premium sai). Vì người yêu cầu không rành sâu backend, agent audit phải tự quyết định kỹ thuật cụ thể theo bảng chuẩn dưới đây, không hỏi lại người dùng — chỉ ghi quyết định vào decision log.

## QUY TRÌNH

### BƯỚC 1 — Audit theo 5 trục (dev-backend), ghi từng vấn đề vào `docs/work/backend-audit/findings.md` (cột: service/controller | trục | mức độ nghiêm trọng cao/trung/thấp | mô tả | dòng code)

**1. EXCEPTION HANDLING**
- Dùng `IExceptionHandler` (.NET 8+, chuẩn cho .NET 10) — KHÔNG dùng `IExceptionFilter`. Đăng ký chuỗi handler theo thứ tự: handler riêng cho từng loại lỗi kỳ vọng đã có Result (NotFound, Validation, Forbidden...) chạy trước và trả `false` nếu không khớp loại của nó; `GlobalExceptionHandler` đứng cuối, luôn xử lý (không được trả `false`), dùng `IProblemDetailsService` để trả response chuẩn RFC 9457.
- Không leak stack trace / message nội bộ / tên bảng-cột DB ra response production — log chi tiết ở server (kèm correlation/trace ID), trả client message đã làm sạch.
- Log exception 1 lần duy nhất tại boundary (không log lại nhiều lần dọc theo call stack) kèm trace ID để đối chiếu.
- Lỗi kỳ vọng (not found, validation, 403, insufficient balance...) dùng Result pattern — KHÔNG dùng exception cho luồng nghiệp vụ bình thường.

**2. BUSINESS LOGIC — trọng tâm nhất, audit kỹ**
- Rà từng service theo quy tắc nghiệp vụ: điểm/XP/gems/hearts không được âm sau mọi thao tác (kể cả khi 2 request chạy đồng thời); nộp bài không double-submit; downgrade premium clamp đúng giới hạn mới; NodeSessions không bị 2 tiến trình cùng sửa đè lên nhau.
- **Cơ chế bắt buộc cho các bảng có giá trị cộng/trừ đồng thời (XP, gems, hearts, NodeSessions)**: dùng **optimistic concurrency** — thêm cột `RowVersion` (`[Timestamp]` / `IsRowVersion()`) làm concurrency token. EF Core sẽ đưa token vào `WHERE` của câu `UPDATE`; nếu có request khác đã đổi dữ liệu trước đó, `SaveChangesAsync` ném `DbUpdateConcurrencyException` thay vì ghi đè âm thầm — bắt exception này, trả 409 Conflict, có thể kèm retry 1 lần với dữ liệu mới. Transaction (`BeginTransactionAsync`) dùng cho multi-write cần atomic (VD: trừ hearts + ghi log cùng lúc), nhưng **transaction không tự chống được lost-update giữa 2 request song song** — vẫn cần RowVersion đi kèm.
- Double-submit: dùng idempotency key (VD: hash của request + user + thời điểm, hoặc unique constraint ở DB cho hành động nộp bài 1 lần/exercise/attempt) — không chỉ dựa vào check-rồi-ghi ở tầng service (race condition).
- **Với mỗi lỗi business logic tìm thấy, bắt buộc phải viết 1 test tái hiện được lỗi đó TRƯỚC khi sửa** (giao cho dev-test) — audit tìm ra lỗi 1 lần không đảm bảo nó không tái phát nếu không có test khóa lại.

**3. PERFORMANCE (EF Core)**
- N+1: dùng `Include`/`ThenInclude` hoặc Projection (`Select` sang DTO) thay vì load rồi loop query riêng. Cảnh báo: `Include` nhiều navigation collection cùng lúc (VD: Order kèm cả Items và Payments) sinh JOIN kiểu cartesian product làm phình số dòng trả về — nếu 1 entity có ≥2 collection cần load, ưu tiên tách 2 query hoặc dùng Projection thay vì Include chồng.
- `AsNoTracking()` cho mọi query chỉ đọc (list, report, leaderboard).
- Pagination: danh sách lớn (leaderboard, lịch sử bài làm, log) chuyển từ offset (`Skip/Take`) sang keyset/cursor pagination (`WHERE Id > lastId ORDER BY Id LIMIT n`) — cần index đúng cột sort. Có thể dùng thư viện `MR.EntityFrameworkCore.KeysetPagination` để đỡ tự viết tay.
- Kiểm tra index cho cột filter/join thường dùng (userId, exerciseId, classId...).

**4. SECURITY**
- JWT: secret ≥ 32 ký tự, có expiry hợp lý, refresh token hợp lệ (không tái sử dụng token cũ vô hạn), không lưu token ở localStorage phía FE (khuyến nghị httpOnly cookie hoặc memory).
- Authorization: từng endpoint kiểm tra đúng role (admin/teacher/student) — không chỉ check đăng nhập mà quên check quyền cụ thể (VD: student gọi được API sửa điểm của người khác).
- Rate limiting: dùng middleware built-in .NET (`AddRateLimiter`/`UseRateLimiter`), phân vùng theo user/API key (không chỉ theo IP — dễ chặn nhầm nhiều người dùng chung mạng công ty/trường), endpoint đăng nhập/đăng ký cần policy chặt hơn endpoint thường, loại trừ health-check khỏi rate limit, cấu hình `OnRejected` trả 429 + `Retry-After` + body ProblemDetails (không trả 429 rỗng).
- CORS: không `AllowAnyOrigin` kết hợp `AllowCredentials`; whitelist domain cụ thể.
- Input validation: mọi DTO có validator (FluentValidation hoặc DataAnnotations) đầy đủ, không tin dữ liệu từ client.
- SQL injection: EF Core tham số hóa mặc định — chỉ audit chỗ nào dùng `FromSqlRaw`/raw SQL có nối chuỗi trực tiếp.
- XSS: sanitize input hiển thị lại (đặc biệt phần bình luận/phản hồi nếu có).
- Không log secret/token/mật khẩu ra log file.

**5. BỀ MẶT**
- Migration khớp với entity hiện tại (chạy `dotnet ef migrations has-pending-model-changes` hoặc tương đương để xác nhận).
- Seed data idempotent (chạy lại nhiều lần không tạo trùng).
- `DbContext` dùng đúng lifetime (Scoped mặc định, không giữ DbContext sống qua nhiều request/background task).

### BƯỚC 2 — Sửa (dev-backend) + viết test (dev-test) + review (dev-review)

Sửa theo mức độ nghiêm trọng: **Cao** (có thể mất dữ liệu/tiền, lộ thông tin, bypass auth) → sửa trước, **Trung bình** (hiệu suất, lỗi logic không critical) → sau, **Thấp** (code style, log) → cuối. Mỗi fix mức Cao/Trung phải có test đi kèm (dev-test), review bởi dev-review trước khi coi là xong.

## VERIFY (dev-test + dev-review)

- Đếm: số endpoint có thể trả 500 kèm stack trace khi ép lỗi (test bằng cách gửi input sai kiểu, DB tạm ngắt...) = 0.
- Đếm: số bảng có cộng/trừ giá trị đồng thời (XP/gems/hearts/NodeSessions) có RowVersion/concurrency token = đủ 100% danh sách đã audit.
- Đếm: số lỗi business-logic mức Cao/Trung tìm thấy ở BƯỚC 1 có test tái hiện + test pass sau khi fix = 100%.
- Chạy log SQL (`LogTo`) trên 5 endpoint list nặng nhất (leaderboard, lịch sử bài làm...) — số N+1 pattern phát hiện = 0.
- Test JWT hết hạn, role sai, rate limit vượt ngưỡng → đều trả đúng status code (401/403/429) kèm ProblemDetails, không lộ chi tiết nội bộ.
- dev-review tổng hợp: bảng vấn đề đã sửa (mức độ | mô tả | test kèm theo | trạng thái) + xác nhận không còn vấn đề mức Cao nào mở.

Việc còn chờ / không chắc → ghi vào `docs/work/backend-audit/notes.md`. --auto
