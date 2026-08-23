# GP-T5 — VERIFY ĐỘC LẬP toàn bộ đợt G-PHỤ (dev-test + dev-review)

- **Ngày:** 13/08/2026
- **HEAD kiểm tra:** `c1dc735` (merge `feature/ux-review2` — GP-T9b) — đã merge đủ: feedback-endpoint (`981cf5b`), 2fa-email (`a25faa7`), breakpoints (`5e75900`), premium-qr (`0543411`), docs-sync-gp (`45960af`), ux-review2 (`c1dc735`); lesson-sim-keys (`676afd8`) nằm trong dev từ trước `981cf5b` (đã xác minh là ancestor của HEAD).
- **Người thực hiện:** celscin-coder (vai dev-test chạy lệnh + dev-review đọc diff)
- **Phạm vi:** KHÔNG sửa code production. Chỉ chạy lệnh + đọc diff + ghi report này.

---

## A. PHẦN A — Verify (dev-test, chạy lệnh thật)

| # | Mục | Yêu cầu | Kết quả | Bằng chứng |
|---|-----|---------|---------|------------|
| A1 | `npm run build` (frontend/) | 0 lỗi | **PASS** | `✓ built in 1.35s` — 0 lỗi (vue-tsc + vite). Bundle khớp gp-t8: engine 476.03 kB, echarts 323.63 kB, PremiumView 32.40 kB (lazy, chứa qrcode) |
| A2 | `npm test` (frontend/) | 89/89 PASS | **PASS** | `Test Files 11 passed (11) — Tests 89 passed (89)` — gồm +4 simulation.spec.ts (GP-T4), +7 vietqr.spec.ts (GP-T7) |
| A3 | `npx playwright test` (frontend/) | 13/13 PASS | **PASS** | `13 passed (30.9s)` — auth 3 / code-runner 3 / ladder 2 / leaderboard 2 / simulator 3 |
| A4 | `dotnet build DsaVisual.sln` (backend/) | 0 warning / 0 error | **PASS** | `Build succeeded. 0 Warning(s) 0 Error(s)` (2.48s) |
| A5 | `dotnet test` UnitTests | 81/81 PASS | **PASS** | `Passed! - Failed: 0, Passed: 81, Skipped: 0, Total: 81` (gồm 12 TwoFactorAuthTests + 4 GamificationServiceTests mới) |
| A6 | `dotnet test` IntegrationTests | 31/31 PASS | **PASS** | `Passed! - Failed: 0, Passed: 31, Skipped: 0, Total: 31` — Testcontainers chạy được: đã khởi động Docker Desktop (daemon lúc đầu tắt) → Testcontainers MsSql thật |
| A7 | Grep cấm backend/src + frontend/src: PostgreSQL/Npgsql/MediatR/Repository/Judge0/secret thật | 0 match production | **PASS** | backend/src: 3 match — TẤT CẢ là comment giải thích quyết định "KHÔNG Repository (SDD §5.1)" (`Program.cs:95`, `AppDbContext.cs:8`, `LessonService.cs:15`) — được phép. frontend/src: 0 match. Secret: `appsettings.json` Jwt Secret `""` + Password `CHANGE_ME`; `appsettings.Development.json` chỉ placeholder `dev-only-secret-...` + `DsaVisual@Dev123` (dev) |
| A8 | Smoke tính năng mới (backend docker :5000 + frontend dev :5174, seed `student@demo.local` Id=3) | xem chi tiết dưới | **PASS** | feedback 200/401 ✓ · 2FA send 200 + MailHog + OTP_INVALID ✓ · premium QR + countdown 60s + HeartsMax=30 ✓ · breakpoint dòng 5 ✓ · 12 màn × 2 theme 24/24 ✓ |
| A9 | Secret scan git grep | không JWT secret/connection string/API key thật | **PASS** | git grep toàn repo `eyJ...`/`sk-`/`AIza`/`AKIA`/`ghp_` → **0 match**. .env chỉ placeholder (`change-me-32-characters-minimum-secret`, `DsaVisual@Dev123`, `ChangeMe@123`) |

### A8 chi tiết smoke (bằng chứng số liệu)

**1. Feedback — `POST /lessons/1/feedback`:**
- Có token → **200** `{"lessonId":1,"rating":5}` (sau `mark-viewed` 204)
- Không token → **401**
- `{rating:9}` + token → **400** `VALIDATION_FAILED` (field Rating, "Đánh giá phải từ 1 đến 5 sao")
- Upsert: gửi lần 2 → 200 (không nhân đôi — DB chỉ 1 bản ghi UserId=3/LessonId=1, đã xác minh sqlcmd)

**2. 2FA email:**
- `POST /auth/2fa/send` (token) → **200** `{"message":"...hiệu lực 5 phút","expiresInSeconds":300}`
- MailHog `GET /api/v2/messages` → **1 email** đúng user, subject "Mã xác thực 2FA — DSA Visual", body chứa OTP `409222` (6 số)
- `POST /auth/2fa/verify {code:"000000"}` → **400 OTP_INVALID**
- `PUT /auth/2fa {enabled:true}` (chưa verify) → **400 OTP_REQUIRED**
- `POST /auth/2fa/verify {code:"409222"}` (mã thật từ MailHog) → **200 `{enabled:true}`**
- `PUT /auth/2fa {enabled:false}` → **200** (cleanup, trả DB về trạng thái cũ)
- Không token → 401

**3. Premium QR (Playwright thật, 1366×768):**
- `/premium` sau reset DB (HeartsMax=5, PremiumUntil=NULL, PremiumSubscriptions xóa) → 3 gói hiện, guard không chặn
- Chọn gói 1 tháng → bước 1 → "Tiếp tục →" → bước 2: **QR canvas 208×208 non-blank (40.1% pixel tối)**; thông tin TK MB Bank + nội dung CK **`DSV3T1`** (userId từ store — user 3)
- Countdown **thật**: `Nút khả dụng sau 00:60` → `00:59` (tick 1s); nút "Tôi đã chuyển khoản" disabled trong 60s
- Chờ **62s thật** → nút enabled, countdown biến mất → bấm → màn **"Nâng cấp thành công!"** + confetti + toast
- DB sau smoke: `Users Id=3: HeartsMax=30, PremiumUntil=2026-09-13` ✓; `PremiumSubscriptions: OrderRef=DSV3T1, PlanId=1m, Status=0 (active)` ✓ (Status=2 lúc tạo đơn → 0 sau mock-pay, đúng thiết kế)

**4. Breakpoint `/simulator/sort.bubble`:**
- Toggle chấm breakpoint dòng 5 → `data-bp=1` (bật)
- Bấm "Chạy" → tự dừng: badge `Đã dừng tại breakpoint dòng 5` (data-testid=breakpoint-badge), nút về "Chạy" (paused), active line = 5 (`data-line=5`)
- Bấm "Chạy" lại (resume) → chạy tiếp, KHÔNG kẹt tại cùng dòng ✓

**5. Audit 12 màn light + dark (24 màn):** `/`, `/login`, `/learn/1`, `/simulator/sort.bubble`, `/exercise/1`, `/path`, `/ladder/1`, `/ladder/1/lab`, `/code/sort.bubble`, `/benchmark/sort.bubble/sort.quick`, `/leaderboard`, `/profile` — mỗi màn: **0 console error** (đã lọc 401 guest refresh — thực tế 0 lỗi 401 vì login thật), **0px horizontal overflow**, 0 response 401.

**Ghi chú môi trường:** Docker Desktop lúc đầu tắt → đã khởi động lại + `docker compose up -d`; container `neww-backend-1` đang chạy **image cũ** (trước 2FA) → đã `docker compose build backend` + up lại để smoke đúng HEAD. Script smoke tạm (smoke-gp-t5*.mjs) đã xóa; frontend dev :5174 đã tắt sau khi xong. Docker stack để lại đang chạy (khôi phục môi trường smoke).

---

## B. PHẦN B — Review (dev-review, git diff 981cf5b..HEAD + code tại HEAD)

`git diff 981cf5b..HEAD --stat` → **82 files, +6469/−145** (gồm migration Designer + 24 ảnh r2-fixed + docs). Đánh giá từng vùng:

| Vùng | File | Đánh giá | Kết luận |
|------|------|----------|----------|
| B1. AuthController 2FA | `AuthController.cs` | Controller mỏng, `[Authorize]` cả 3 endpoint, `CurrentUserId()` từ token. Xóa đúng TODO 501. Không lộ secret | ✅ ĐẠT |
| B2. AuthService 2FA | `AuthService.cs` | OTP 6 số **crypto-random** (`RandomNumberGenerator.GetInt32` + D6). DB chỉ lưu **SHA256 hash** hex64 (không mã gốc). Expiry 5 phút (check `ExpiresAt <= UtcNow`). Dùng 1 lần (mark Used trong transaction cùng bước bật 2FA). Gửi mã mới → vô hiệu mã cũ cùng purpose. **SmtpClient Timeout=10s**, lỗi email KHÔNG block luồng (pattern SDD §5.6 như forgot-password). Bật bắt buộc qua OTP (chống attacker khóa tài khoản chủ) | ✅ ĐẠT |
| B3. OtpCodes migration | `20260812182232_AddOtpCodes.cs` + `OtpCode.cs` + `UserConfiguration.cs` | Bảng OtpCodes: CodeHash nvarchar(64)=SHA256 hex ✓, Purpose nvarchar(32), FK→Users **Restrict**, index (UserId) + (UserId, Purpose, Used) khớp query. Migration `Up/Down` đối xứng | ✅ ĐẠT |
| B4. LessonsController feedback (trong base 981cf5b, review tại HEAD) | `LessonService.AddFeedbackAsync` + `LessonFeedbackRequestValidator` + `20260812180545_WidenContentFeedbackComment` | Upsert đúng: query (UserId, LessonId) → insert mới (CreatedAt) / update Rating+Comment+UpdatedAt (không nhân đôi — UNIQUE IX đã có). Validation: Rating 1-5 + Comment ≤1000 (FluentValidation). Rule "phải Đánh dấu đã học" → 403 (khớp API_REFERENCE §4.15 v2.9). Migration widen nvarchar(200)→(1000) an toàn | ✅ ĐẠT |
| B5. GamificationService OrderRef DSV | `GamificationService.cs` + `PremiumDtos.cs` | `OrderRef = $"DSV{userId}T{months}"` — format đúng (VD DSV1002T3), **userId từ tham số** (controller truyền `CurrentUserId()` — không hardcode). `ContentRef` trả trong DTO, FE tự tính nguồn thứ 2 — khớp nhau. Status: tạo đơn = 2 → mock-pay = 0 (active) + HeartsMax=30 (verify bằng smoke + sqlcmd). `ParsePlanMonths` nhận 1m/3m/12m + 1/3/12 (deep link). `MockPayAsync` check `s.UserId == userId` (không IDOR) | ✅ ĐẠT |
| B6. vietqr.ts | `lib/vietqr.ts` + `vietqr.spec.ts` | **CRC16-CCITT (FALSE)**: poly 0x1021, init 0xFFFF, không reflect, không xorout — đúng EMVCo tag 63; test vector `'123456789'→'29B1'` PASS. CRC phủ **toàn bộ payload + "6304"** (đúng chuẩn). TLV length = số BYTE UTF-8, pad 2 (getLength) — đúng; hand-rolled UTF-8 encode xử lý surrogate pair. **Đơn giản hóa có tài liệu hóa rõ** trong header file: tag 62 bỏ GUID 26/51 (A000000727), chỉ sub-tag 01=QRIBFTTA + 08=nội dung CK; static QR (01=11) — lý do ghi đầy đủ. Payload mẫu GP-T7 tự phân tích khớp (00/01/52/53/54/58/59/60/62/63). 7 unit tests phủ CRC vector + cấu trúc + CRC nhúng khớp + amount không dấu phẩy + deterministic + UTF-8 | ✅ ĐẠT |
| B7. PremiumView | `PremiumView.vue` + `gamification.ts` | Countdown **60s thật** (COUNTDOWN_SECONDS=60, interval 1s, cleanup onUnmounted/watch step — không leak timer; smoke xác minh tick + enabled sau 61s). Copy có fallback toast lỗi. **userId từ auth store** (computed, không hardcode). PLANS id **1m/3m/12m khớp contract backend** (fix 3a382f0). Deep link `?plan=1`/`?plan=1m`. guard đăng nhập + guard premium (confirm thay gói). `qrcode` 1.5.4 + `@types/qrcode` 1.5.6 khớp package.json/THIRD_PARTY | ✅ ĐẠT |
| B8. Simulation breakpoint | `stores/simulation.ts` + `PseudocodePanel.vue` + `SimulatorView.vue` + `useSimulation.ts` | Auto-pause đúng chỗ: play loop + stepForward gọi `hitBreakpointAtCurrentStep()` SAU `currentIndex += 1` → dừng tại bước đầu tiên chạm dòng; resume (`play()`) xóa hit trước khi advance → **không kẹt** (smoke xác minh). Clear hit đủ chỗ: stepBack/jumpTo/reset/loadSim/configureInput. `breakpoints` giữ khi reset (theo test spec). UI a11y: aria-pressed + aria-label + data-* + badge role=status. **KHÔNG đụng engines/*** | ✅ ĐẠT |
| B9. UI review v2 | `tokens.css`/`tailwind.css`/`palettes.css` + 12 view/component | Đúng 3 cụm đã chốt: (a) light gradient tối đi (aurora 0.72→0.52, sunset 0.83→0.55, mint 0.87→0.54) → chữ trắng ≥4.5:1; (b) primary `#0D9488→#007E72` + ring (white-on-primary 4.97:1), text-muted `#5E7A77→#56706D`, text-disabled `#9CB5B2→#56706D` (4.5:1+); (c) dark scrim `rgba(4,47,46,0.62)` cho hero light-text (Home/Lesson/Login dark). **shadcn token chỉ đổi giá trị, không đổi cấu trúc**; dark border 0.32→0.4. Không phá layout: 89 unit + 13 e2e + audit 24 màn 0 overflow/0 console error | ✅ ĐẠT |
| B10. Docs đồng bộ | SRS/SDD/USER_GUIDE/API_REFERENCE/THIRD_PARTY/TEST_PLAN | **Khớp code thật** (xem mục 11): SRS §3.10B FR-10.7 + §5.33 UC-32 + §1.3.2 (QR MB Bank NGUYEN THI NHU HOA 83863112088386, DSV{userId}T{months}, 60s, không gọi API ngân hàng); SDD Màn 25/26 + §7.3.28 + §3.9 bundle thật; USER_GUIDE §3.10 4 bước + lưu ý mô phỏng; API_REFERENCE §2.2 (5 mã OTP, v2.13) + §4.12 (3 endpoint) + §4.14 (planId 1m\|3m\|12m, OrderRef DSV, contentRef + ví dụ JSON) + changelog; THIRD_PARTY `qrcode` 1.5.4 + `@types/qrcode` 1.5.6 (MIT, đối chiếu node_modules); TEST_PLAN §10: 81/89/31/214 đúng số đo lại | ✅ ĐẠT |

### 11. Lệch docs ↔ code (rà riêng theo yêu cầu)

| Câu hỏi | Kết quả |
|---------|---------|
| 2FA endpoint contract (PUT + POST send/verify) đã ghi API_REFERENCE? | ✅ CÓ — §4.12: `PUT /auth/2fa` (`enabled:false` tắt trực tiếp; `enabled:true` → 400 OTP_REQUIRED qua /send + /verify), `POST /auth/2fa/send` (expiresInSeconds), `POST /auth/2fa/verify` ({code} → {enabled:true}) — khớp 100% code (AuthController.cs:128/138/147) |
| Premium planId 1m/3m/12m đồng bộ docs? | ✅ CÓ — API_REFERENCE §4.14 `{planId} (1m/3m/12m)` = `ParsePlanMonths` (1m/1, 3m/3, 12m/12) = PLANS.id PremiumView; smoke dùng `1m` thành công |
| THIRD_PARTY có qrcode? | ✅ CÓ — mục qrcode 1.5.4 + @types/qrcode 1.5.6 (MIT), trùng package.json `^1.5.4`/`^1.5.6`; license xác minh từ node_modules (gp-t8) |

### Ghi chú phụ (ngoài phạm vi, không chặn)

- **P3 — AuthService.cs (gần dòng 596/607):** khi SMTP thiếu/lỗi, mã OTP được log ra (pattern dev SDD §5.6, giống forgot-password). Nếu deploy production thiếu SMTP → mã lộ trong log. Đề xuất: gate log theo `IHostEnvironment` (chỉ Development). KHÔNG block vì dev intent rõ ràng + đã ghi SETUP_TODO §10.
- **P3 — AuthService Verify2FaCodeAsync:** chưa có rate-limit brute-force /verify (6 chữ số); endpoint đã [Authorize] nên attacker phải có token; luồng chặn đăng nhập 2FA (sai 3 lần khóa 10 phút) đã ghi ngoài phạm vi tại SETUP_TODO §10.2.
- **P3 — ErrorCodes OTP_INVALID vs OTP_USED/OTP_EXPIRED:** phân biệt mã "từng đúng" — rò rỉ thông tin mức thấp, chấp nhận cho demo.
- **P3 — GamificationService.cs:664:** comment `Status = 2 // chờ thanh toán mô phỏng` hơi khó hiểu (thực tế: 2 = đơn tạo, 0 = active sau mock-pay) — hành vi đúng, chỉ wording.
- **P3 — vietqr.ts getLength:** giả định giá trị <100 byte (không mã hóa độ dài nhiều byte 02xx) — an toàn với format cố định DSV + tên ≤30 ký tự; đã tài liệu hóa đơn giản hóa.
- **P3 — PremiumView.vue:187:** `setTimeout(router.replace, 2500)` không clear khi unmount — vô hại.
- **Note (không phải thay đổi đợt này):** theme store (`ui.ts`) toggle nhưng không có UI áp `.dark` class lên `<html>` — dark mode chỉ kích hoạt qua class (đúng cách T9a/T9b audit đã làm). Ngoài phạm vi GP.

---

## C. KẾT LUẬN

**Phần A: 9/9 PASS** (build 0 lỗi, FE 89 unit + 13 e2e, BE 81 unit + 31 integration — tổng 214, grep cấm 0 match production, smoke 4 tính năng PASS, audit 24/24 màn sạch, secret scan 0 match).

**Phần B: 10/10 vùng ĐẠT** — không tìm thấy lỗi chặn; chỉ 5 ghi chú P3 (hardening) + 1 note ngoài phạm vi.

### VERDICT: ✅ **APPROVE**

- **P1:** không có
- **P2:** không có
- **P3 (không chặn, ghi để sau):** log OTP khi thiếu SMTP nên gate theo environment (`AuthService.cs` ~596/607) · rate-limit /verify (`AuthService.cs` ~470) · wording comment Status (`GamificationService.cs:664`) · TLV multi-byte length (`vietqr.ts:62`) · clear timeout PremiumView (`PremiumView.vue:187`)
