# GP-T2 — 2FA email (thay 501 TODO tại AuthController.cs:123-127)

**Ngày:** 13/08/2026 · **Nhánh:** `feature/2fa-email` (từ `dev` HEAD `981cf5b`) · **Trạng thái:** DONE

## Contract endpoint cuối (đã đồng bộ API_REFERENCE.md §4.12 — v1.3)

| Method | Endpoint | Body | Response | Ghi chú |
|---|---|---|---|---|
| PUT | `/auth/2fa` | `{ enabled: bool }` | `200 { enabled, message }` / `400` | `enabled:false` → tắt trực tiếp (idempotent). `enabled:true` → `400 OTP_REQUIRED` (bắt buộc qua send + verify — chống attacker chiếm quyền khóa tài khoản chủ) |
| POST | `/auth/2fa/send` | — | `200 { message, expiresInSeconds: 300 }` | Sinh mã OTP 6 số (crypto-random), lưu SHA256 hash vào bảng `OtpCodes`, gửi email qua SMTP (dev: MailHog). Đã bật 2FA → `400 TWO_FA_ALREADY_ENABLED`. Gửi lại → mã cũ bị vô hiệu (dùng 1 lần) |
| POST | `/auth/2fa/verify` | `{ code: "123456" }` | `200 { enabled: true }` | Đúng + chưa dùng + chưa hết hạn (5 phút) → đánh dấu Used + bật 2FA. Sai → `400 OTP_INVALID`; hết hạn → `400 OTP_EXPIRED`; đã dùng → `400 OTP_USED`; format không phải 6 chữ số → `400 VALIDATION_FAILED` |

Cả 3 endpoint đều `[Authorize]` — không token → `401`.

**Error codes mới (5 mã, catalog API_REFERENCE.md §2.2 — v2.13):** `OTP_REQUIRED`, `OTP_INVALID`, `OTP_EXPIRED`, `OTP_USED`, `TWO_FA_ALREADY_ENABLED` (đều 400).

## File sửa

**Backend (`backend/`):**
- `src/DsaVisual.Application/Persistence/Entities/OtpCode.cs` — entity mới (UserId FK, CodeHash SHA256 hex 64, Purpose, ExpiresAt, Used, CreatedAt)
- `src/DsaVisual.Application/Persistence/Configurations/UserConfiguration.cs` — thêm `OtpCodeConfiguration` (bảng OtpCodes, FK Restrict, index UserId + (UserId, Purpose, Used))
- `src/DsaVisual.Application/Persistence/AppDbContext.cs` — `DbSet<OtpCode>` (bảng 33)
- `src/DsaVisual.Application/Persistence/Migrations/20260812182232_AddOtpCodes.cs` (+ Designer) — migration mới; `dotnet ef database update` đã chạy thành công lên SQL Server docker
- `src/DsaVisual.Application/Common/ErrorCodes.cs` — 5 mã 2FA + map HTTP 400
- `src/DsaVisual.Application/Dtos/TwoFactorDtos.cs` — `Toggle2FaRequest`, `Verify2FaRequest`, `Send2FaResponse`, `Toggle2FaResponse`
- `src/DsaVisual.Application/Services/IAuthService.cs` + `AuthService.cs` — `Toggle2FaAsync`, `Send2FaCodeAsync`, `Verify2FaCodeAsync` + helpers `GenerateOtpCode` (RandomNumberGenerator 6 số), `HashOtpCode` (SHA256), `Send2FaCodeEmailAsync` (SmtpClient built-in, Timeout 10s)
- `src/DsaVisual.Api/Controllers/AuthController.cs` — xóa 501, thêm 3 endpoint thật
- `tests/DsaVisual.UnitTests/TwoFactorAuthTests.cs` — 12 test mới (OTP hash/expiry/used/invalid/format, bật/tắt, idempotent, mã cũ vô hiệu)
- `tests/DsaVisual.UnitTests/TestServices.cs` — overload `CreateAuthService` nhận `ILogger<AuthService>` (capture mã từ log dev)

**Docs:**
- `docs/API_REFERENCE.md` — §2.2 (5 mã, v2.13) + §4.12 (2 endpoint mới, làm rõ PUT) + changelog v1.3
- `docs/SETUP_TODO.md` — §10: SMTP thật khi deploy (mục 1), luồng 2FA bước 2 khi đăng nhập (mục 2, task riêng), FE Màn N-1 (mục 3)

## Quyết định triển khai (ghi rõ theo yêu cầu)

1. **SMTP**: dev dùng **MailHog** (docker-compose có sẵn — SMTP `localhost:1025`, UI `http://localhost:8025`), cấu hình sẵn trong `appsettings.Development.json` (`DSA:Email:SmtpHost=localhost`, Port 1025). SMTP thật khi deploy: đặt `DSA__Email__SmtpHost/Port/From` (đã ghi SETUP_TODO §1.3/§10). SMTP thiếu → KHÔNG block luồng, ghi mã trong **log dev** (pattern SDD §5.6 như forgot-password).
2. **Thư viện**: dùng `System.Net.Mail.SmtpClient` (built-in) — KHÔNG thêm gói mới → THIRD_PARTY.md không cần sửa (MailHog đã liệt kê mục 3).
3. **Bảng OtpCodes**: SDD §10.2 KHÔNG có bảng OTP → tự thêm migration `AddOtpCodes` (Id, UserId FK→Users Restrict, CodeHash nvarchar(64), Purpose nvarchar(32), ExpiresAt datetime2, Used bit, CreatedAt datetime2) — 33 bảng.
4. **Bảo mật OTP**: mã 6 số crypto-random (`RandomNumberGenerator`); DB chỉ lưu SHA256 hash; hết hạn 5 phút; dùng 1 lần; gửi mã mới → vô hiệu mã cũ cùng purpose.
5. **FE**: kiểm tra `frontend/src` — KHÔNG có code gọi `/2fa` → không phá FE. PUT cũ giữ nguyên route, giờ trả kết quả thật.
6. **Ngoài phạm vi (ghi SETUP_TODO §10.2)**: chặn đăng nhập khi thiếu mã (SDD Màn Login bước 2: sai 3 lần khóa 10 phút, ghi nhớ thiết bị 30 ngày) — task backend riêng (mở rộng LoginAsync + purpose "login").

## Verification (thật)

- `dotnet build DsaVisual.sln` → **0 Warning / 0 Error**
- `dotnet test DsaVisual.UnitTests` → **77/77 PASS** (65 cũ + 12 mới 2FA)
- `dotnet ef database update` → **thành công** lên SQL Server docker (bảng OtpCodes)
- **Smoke thật (docker MailHog + API local port 5099):**
  - `POST /auth/2fa/send` (có token) → `200 {"message":"Mã xác thực đã được gửi qua email (hiệu lực 5 phút)","expiresInSeconds":300}`
  - MailHog `GET /api/v2/messages` → có 1 email cho user (subject "Mã xác thực 2FA — DSA Visual"), body decode: `Mã xác thực hai lớp (2FA) của bạn là: 806491` + dòng hiệu lực 5 phút/dùng 1 lần
  - `POST /auth/2fa/verify {code: 806491}` → `200 {"enabled":true,...}`
  - `POST /auth/2fa/verify {code: 000000}` → `400 OTP_INVALID`
  - `PUT /auth/2fa {enabled:true}` (chưa verify) → `400 OTP_REQUIRED`
  - `PUT /auth/2fa {enabled:false}` → `200 {"enabled":false,...}`
  - Không token → `401`
- **Grep cấm**: `TODO|XXX|NotImplementedException|placeholder` trong AuthController — 0 match (501 đã xóa)

## Commit

- `.\commit-as.ps1 bao "feat: GP-T2 - 2FA email (OtpCodes + PUT /auth/2fa + POST /auth/2fa/send + /verify)"` → **`3a895d5`** trên `feature/2fa-email` (parent `981cf5b` = dev HEAD)
- ⚠️ Sự cố GP-T1 tái diễn: lúc commit, agent GP-T4 (chạy song song) đã checkout `feature/breakpoints` trong cùng workspace → commit đầu tiên (`6ec9200`) rơi vào nhánh của họ. Đã khắc phục an toàn: cherry-pick sang `feature/2fa-email` (`3a895d5`) + `git branch -f feature/breakpoints 973f0bb` (trả đúng commit GP-T4, không đụng working tree). Kiểm tra lại sau fix: build 0/0 + test 77/77 PASS. KHÔNG merge, KHÔNG commit lên dev.
