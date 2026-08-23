# PM REPORT — BACKEND AUDIT (PROMPT_J_BACKEND_AUDIT) — --auto

> Ngày: 13/08/2026 · PM: orchestrator · Nhánh: feature/backend-audit (thực tế chạy LOCAL — session frontend chiếm working tree trên feature/ux-h-c, KHÔNG commit, KHÔNG merge dev)
> Log quyết định: docs/pm-decision-log-backend.md · Findings: docs/work/backend-audit/ (findings-*.md + notes.md + audit-notes-backend.md)

## Mục tiêu
Audit + sửa backend ASP.NET Core 10 theo 5 trục (exception, business logic, performance EF, security, bề mặt) — đặc biệt các điểm mất tiền/mất dữ liệu (XP/gems/hearts âm, double-submit, downgrade premium sai). Khởi điểm BUG-1 đã biết: job downgrade Premium không tồn tại (GamificationService.cs:840-844).

## Trạng thái từng task

| Đợt | Task | Trạng thái | Kết quả verify |
|-----|------|-----------|----------------|
| Audit | 6 task audit 5 trục (exception/biz-gami/security/perf/surface/biz-services) | **DONE** | 93 findings (14 CAO / 37 TRUNG / 42 THAP) ghi docs/work/backend-audit/findings-*.md |
| A | GamificationService 7 lỗi (BUG-1, regen lost-update, shop race, quest race, quest XP chết, mock-pay, RowVersion) | **DONE** (review 1 lần CHANGES REQUESTED R1 → đã fix) | 92+36 PASS → verify độc lập PASS · review APPROVE sau R1 |
| B | AuthService 7 lỗi (register race, refresh TOCTOU, reset/2FA race, forgot token cũ, log token/OTP, OTP limit, register 1 tx) | **DONE** (review 1 lần CHANGES REQUESTED → fix reset OTP counter) | 98+45 PASS ×3 · review APPROVE |
| C | Exception 5 lỗi (unique→409, EnterNode catch hẹp, envelope 400/401/403/404, ApiControllerBase) | **DONE** | 98+45 PASS ×3 · review APPROVE |
| D | Exercise/Class/Progress (double-submit, score clamp, node sync, ladder, retry merge, N+1, overview 500, report lớp) | **DONE** (review 2 lần CHANGES REQUESTED → fix 2 Major thiết kế: idempotency key thay unique vĩnh viễn + merge response) | 105+56 → 121+77 PASS · verify PASS |
| E | Security 10 lỗi (rate limit, forwarded headers, sanitize XSS ×3, leaderboard membership, mock-pay gate, 17 validators, JWT ≥32, swagger, sanitizer whitelist) | **DONE** (review 1 lần CHANGES REQUESTED → fix XFF KnownProxies + mock-pay fail-closed) | 117+67 PASS ×2 · verify PASS |
| F | Perf 8 lỗi (keyset leaderboard/submissions, index Xp, stats 1 query, trace streaming, EXISTS, catalog 1 lần, feedback SQL, EnterNode 1 đọc) | **DONE** | 117+77 PASS ×2 · verify PASS |
| G | Bề mặt 5 lỗi (5 unique index + LessonSimulation config, seed transaction, comments) | **DONE** | 117+77 PASS · verify PASS (seed idempotent thực tế 2 lần) |
| H | SettingService (cache race #17, upsert trước save #18) | **DONE** | 121+77 PASS ×2 · verify PASS |

## File thay đổi (KHÔNG commit — local)
- **Migrations (8 mới)**: AddRowVersionConcurrency, AddSubmissionUniqueConstraints, RemovePermanentSubmissionUnique, AddLeaderboardIndexes, AddContentUniqueIndexes (+3 migration nhỏ từ các đợt — xem git status untracked)
- **Services**: GamificationService (premium clamp, regen atomic, shop atomic, quest progress hooks, mock-pay idempotent, keyset leaderboard), AuthService (race consume, OTP lock, dummy PBKDF2), ExerciseService (clamp, idempotency, retry merge, keyset, ownership), ClassService (batch, report), ProgressService (LoadCounts), SettingService (cache), SettingsCache, CodeRunnerService (trace streaming), QuestProgressWriter (mới), LoginAttemptTracker (key API), SubmissionLockRegistry (timeout)
- **Api**: Program.cs (rate limiter, forwarded headers, sanitizer, envelope, jwt check, swagger), ErrorHandlingMiddleware (unique→409), ApiControllerBase (ValidateRequestAsync + TryParse), MeController/FeedbackController/BugReportsController (sanitize), GamificationController (membership, mock-pay gate), AuthController (cookie), AdminController (stats 1 query), ExercisesController (keyset params)
- **Entities/Config**: User + 6 entity (RowVersion), ExerciseSubmission/CodeSubmission (ClientRequestId, IsClientDeclared), LessonSimulationConfiguration (mới), UserConfiguration (index Xp), validators ×17 (mới), DTOs (SubmitRequest/CodeSubmitRequest + ClientRequestId), appsettings (RateLimit, EnableMockPay, Proxy)
- **Test (10+ file mới/sửa)**: GamificationServiceRegressionTests, GamificationRegressionTests, AuthServiceRegressionTests, AuthRegressionTests, GamificationEnterNodeExceptionTests, ErrorHandlingMiddlewareTests, ExerciseServiceRegressionTests, ExerciseRegressionTests, ClassProgressRegressionTests, SecurityRegressionTests, JwtSecretStartupTests, PerformanceGuardRegressionTests, SettingServiceRegressionTests, TestServices, TwoFactorAuthTests

## Kết quả verify cuối
- **Unit: 121/121 PASS** (baseline 85 → +36) · **Integration: 77/77 PASS** (baseline 31 → +46) · build 0 warning/0 error
- Đếm theo PROMPT_J VERIFY:
  - Endpoint trả 500 kèm stack trace khi ép lỗi: **0** (envelope thống nhất + unique→409 + ApiControllerBase 401)
  - Bảng cộng/trừ có concurrency token: **7/7** (User, UserQuest, UserProgress, UserNodeProgress, UserInventory, PremiumSubscription, NodeSession)
  - Lỗi business CAO/TRUNG có test tái hiện + pass sau fix: **100%** (mọi fix Đợt A-H đều có test đỏ→xanh)
  - N+1 trên 5 endpoint list nặng: **0 phát hiện mới** (ClassService batch, leaderboard keyset+EXISTS, stats 1 query, trace streaming)
  - JWT hết hạn/role sai/rate limit → 401/403/429 kèm envelope ProblemDetails-style: **đạt** (test 401/403 envelope + 429 Retry-After)
- dev-review tổng: không còn vấn đề CAO mở; mỗi đợt có verdict APPROVE sau khi xử lý CHANGES REQUESTED

## Quyết định đã ghi (chi tiết: docs/pm-decision-log-backend.md — 10 mục)
Lazy fix premium (không hosted service) · RowVersion 7 bảng + 409 · Idempotency key thay unique vĩnh viễn (FR-4.4/9.5) · Clamp điểm client + IsClientDeclared · Rate limiter partition user+IP · Mock-pay fail-closed · KnownProxies chống XFF spoof · Không migrate IExceptionHandler · Sanitizer 13 tag trade-off · Keyset additive + index · SettingService single-instance chấp nhận · Chạy LOCAL (session frontend chiếm nhánh).

## Việc còn tồn đọng (chi tiết: docs/work/backend-audit/notes.md)
1. dev-docs: cập nhật TEST_PLAN (số test thật 121+77=198), API_REFERENCE (params keyset lastXp/lastId/lastSubmittedAt, ClientRequestId, EnableMockPay, mã lỗi CONFLICT/RATE_LIMITED), USER_GUIDE (sanitizer 13 tag — teacher mất link/ảnh/table), DEPLOY (DSA__Proxy__KnownProxies, EnableMockPay).
2. FE: chuyển leaderboard/submissions sang cursor khi deep-page; gửi ClientRequestId khi submit.
3. Findings còn lại (7 TRUNG + ~20 THAP — hầu hết chấp nhận thiết kế/multi-instance): xem notes.md phần "Còn lại".
4. Flake tiềm năng: AuthRegressionTests.Verify2Fa_TwoConcurrentSameOtp (race — theo dõi).

Người dùng xem báo cáo: OK → kết thúc. Chưa OK → yêu cầu 'làm lại <task/mục>' kèm ghi chú, PM chạy lại phần đó.
