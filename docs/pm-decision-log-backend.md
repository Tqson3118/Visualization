## [2026-08-13 10:00] Khởi tạo phiên audit backend (PROMPT_J_BACKEND_AUDIT, --auto)
- Quyết định: Chạy audit 5 trục theo PROMPT_J_BACKEND_AUDIT.md trên nhánh feature/backend-audit (tạo từ feature/ux-h-b @ 61b3351), KHÔNG merge dev — session frontend (feature/ux-h-b) đang chạy song song. Log riêng: docs/pm-report-backend.md + docs/pm-decision-log-backend.md. File prompt gốc tồn tại và đã đọc (lần đầu read báo không tìm thấy do lỗi tool — glob xác nhận có file, read lại thành công).
- Ảnh hưởng: toàn bộ backend/src + backend/tests; findings ghi docs/work/backend-audit/findings-*.md; BUG-1 nguồn docs/work/audit-notes-backend.md.
- Quyết định: BUG-1 fix theo hướng LAZY FIX ưu tiên (đồng bộ clamp HeartsMax=10 + Hearts=MIN(Hearts,10) tại mọi điểm đọc/ghi hearts khi PremiumUntil hết hạn) thay vì hosted service — giao dev-backend đánh giá, nếu bất khả thi mới dùng hosted service; quyết định cuối ghi lại khi task fix.
- Ảnh hưởng: GamificationService.cs, có thể Program.cs (DI) nếu cần hosted service.

## [2026-08-13 10:30] Phát hiện nhánh bị session frontend chiếm — chuyển sang làm việc LOCAL
- Quyết định: Nhánh eature/ux-h-c đang được session frontend giữ working tree (modified frontend/src/i18n/vi.ts + nhiều .vue) — git switch feature/backend-audit FAIL. Theo điều kiện đặc biệt #1 của user (13/08): chuyển sang phương án "làm việc LOCAL" — KHÔNG commit, KHÔNG tạo nhánh mới, KHÔNG merge dev. Mọi thay đổi backend nằm trong working tree, giao sau khi session frontend xong. Backend/frontend tách thư mục nên không xung đột.
- Ảnh hưởng: toàn bộ task fix sẽ để LOCAL (không commit); findings/log nằm docs/work/backend-audit/ + docs/pm-report-backend.md + docs/pm-decision-log-backend.md.

## [2026-08-13 11:00] Đợt A fix (Gamification + RowVersion) — kết quả verify/review
- Quyết định: Nhận kết quả Đợt A: 92/92 unit + 36/36 integration PASS; dev-test verify PASS. dev-review verdict CHANGES REQUESTED — 1 Major R1: RowVersion regression — AuthService (UpdateMeAsync:266, ChangePasswordAsync:308, ResetPasswordAsync:375, Toggle2FaAsync:403, Verify2FaCodeAsync:514) + UserService (SetStatusAsync:93, SetRoleAsync:120, ApproveTeacherAsync:143, ResetPasswordAsync:178, DeleteAsync:222) ghi tracked Users KHÔNG bắt DbUpdateConcurrencyException → 503 sai khi race với raw SQL. Yêu cầu dev-backend sửa R1: catch → 409 CONFLICT.
- Ảnh hưởng: AuthService.cs + UserService.cs.
- Quyết định (R2-R4 minor — KHÔNG chặn, ghi nhận): R2 MarkViewedAsync hook quest ngoài transaction — chấp nhận (có sync-on-read bù); R3 pass_quiz/pass_lab hook đếm mọi submission — chấp nhận (quest count theo SRS là "hoàn thành hoạt động", không yêu cầu pass; ghi chú); R4 GET /me/quests side-effect ghi — chấp nhận (per-user nhỏ); Nit lesson_viewed count lặp — chấp nhận.
- Ảnh hưởng: ghi notes.md.

## [2026-08-13 11:45] Đợt B (AuthService 7 lỗi) + Đợt C (Exception 5 lỗi) — verify + review xong
- Quyết định: dev-test verify PASS 3/3 lần (98 unit + 45 integration). dev-review: CHANGES REQUESTED 1 điểm chặn — Send2FaCodeAsync thiếu reset OTP counter khi gửi mã mới (AuthService.cs:519-559) → đã fix (thêm loginAttempts.Reset("otp:"+userId) sau SaveChanges, dòng 556-558), build+test xanh. Minor ghi nhận: (1) Refresh READ COMMITTED cửa sổ revoke trống — an toàn (≤1 chuỗi sống, replay bắt ở lần dùng kế tiếp); (2) unique violation return qua await-using dispose rollback ngầm — OK; (3) nit comment Dev/Staging lệch code.
- Quyết định: #2 IExceptionHandler KHÔNG migrate (giữ middleware, chuẩn hóa envelope) — ghi notes.md; #6/#7/#8/#9 THAP exception ghi notes.md.
- Ảnh hưởng: AuthService.cs, LoginAttemptTracker.cs, ErrorHandlingMiddleware.cs, Program.cs, ApiControllerBase.cs, GamificationService.cs (EnterNode catch hẹp), LessonsController.cs, test files mới.

## [2026-08-13 12:30] Đợt D fix (Exercise/Class/Progress) — review chặn 2 Major, chốt thiết kế mới
- Quyết định: dev-test verify PASS (104 unit + 55 integration ×2). dev-review CHANGES REQUESTED 2 Major:
  (1) Unique vĩnh viễn (UserId, ExerciseId, ClassAssignmentId) chặn re-attempt — vi phạm FR-4.4 (xem từng lần nộp), FR-9.5 (so sánh 2 lần nộp), USER_GUIDE (làm lại cải thiện điểm), FE fetchMySubmissions trả mảng (nộp nhiều lần hợp lệ).
  (2) Merge path trả response sai: request thua race nhận "không đạt" dù node đã pass.
- Quyết định thiết kế mới: (1) BỎ filtered unique vĩnh viễn → thay bằng unique (UserId, ExerciseId, ClassAssignmentId, ClientRequestId) WHERE ClientRequestId IS NOT NULL (idempotency key optional — FE chưa gửi, backward compatible); double-submit single-instance chống bằng SubmissionLockRegistry (đã có) + ghi chú multi-instance assumption (NFR-12); (2) merge path reload sau merge → response trả kết quả THỰC CUỐI (request thua race nhận đúng kết quả merged).
- Ảnh hưởng: migration AddSubmissionUniqueConstraints (sửa index), ExerciseService, test files (dev-test cập nhật cho khớp thiết kế mới: cùng key → 1 submission; khác key → nhiều submission OK).

## [2026-08-13 13:00] Đợt D fix 2 Major xong — verify cuối PASS
- Quyết định: Đổi thiết kế double-submit: bỏ unique vĩnh viễn → ClientRequestId optional + filtered unique WHERE ClientRequestId IS NOT NULL (cả ExerciseSubmission + CodeSubmission — bỏ unique vĩnh viễn CodeSubmission vì FR-9.5 cho phép nộp code nhiều lần); merge path trả response từ dữ liệu merged (mergedScore=max, Passed=finalScore==maxScore); quest pass_node chống double-count. Migration mới 20260813070119_RemovePermanentSubmissionUnique (không sửa migration cũ).
- Quyết định: dev-test cập nhật 3 test cũ + thêm 2 test mới (idempotent + merge response race qua 2 WebApplicationFactory). Kết quả cuối: 105/105 unit + 56/56 integration (1 flake ngoài scope: AuthRegressionTests.Verify2Fa_TwoConcurrentSameOtp chạy riêng PASS — theo dõi).
- Ảnh hưởng: ExerciseService.cs, SubmitRequest.cs, CodeSubmitRequest.cs, ExerciseSubmission.cs, CodeSubmission.cs, ExerciseConfiguration.cs, migration mới, test files.

## [2026-08-13 13:30] Đợt E fix (Security 10 findings) xong
- Quyết định: Rate limiter partition user+IP (keys DSA:RateLimit:General:PermitLimit=300, Sensitive=60, WindowSeconds=60; /health ngoại lệ; 429+Retry-After+envelope RATE_LIMITED); UseForwardedHeaders đầu pipeline (trust all — comment); sanitize LessonNote/Feedback/BugReport + giới hạn 50KB; leaderboard class membership 403; mock-pay gate DSA:Premium:EnableMockPay (production=false); 17 validator mới + ValidateRequestAsync helper; JWT secret ≥32 fail-fast; Swagger chỉ Development; sanitizer whitelist 13 tag (AllowedTags.Clear+Add, AllowedSchemes http/https/mailto). Kết quả: 117/117 unit + 67/67 integration.
- Quyết định: sửa 3 call site seed trong SecurityRegressionTests (createdBy thiếu) — giữ nguyên ý nghĩa test. KHÔNG làm findings #1/#3/#4/#5/#11/#14/#15/#16/#19 (ngoài scope Đợt E) — ghi notes.md để xử lý sau.
- Ảnh hưởng: Program.cs, AuthController, MeController, FeedbackController, BugReportsController, GamificationController, GamificationService, 17 Validators mới, appsettings.

## [2026-08-13 14:00] Review Đợt E — CHANGES REQUESTED 1 Major + quyết định trade-off
- Quyết định (Major #1): XFF spoof — KnownNetworks/KnownProxies trống → client tự set X-Forwarded-For giả → rate limiter IP partition vô hiệu. FIX: wire KnownProxies từ config (DSA:Proxy:KnownProxies) ở Production + comment; nếu config trống → không trust XFF (ForwardedHeadersOptions.KnownProxies rỗng mặc định không tin header).
- Quyết định (Minor #2): mock-pay default GetValue(..., true) → đổi default FALSE + guard IsProduction() (defense-in-depth).
- Quyết định (trade-off sanitizer): chấp nhận whitelist 13 tag + 0 attribute (chống phishing/tracking); teacher content mất link/ảnh/table — ghi USER_GUIDE/notes; nếu cần sau này bổ sung a[href]/img[src] scheme-restricted.
- Quyết định (validator chết): chấp nhận 5 validator đăng ký không wire (Register/Login/ExerciseUpsert/ClassUpsert/Submit — service check tay) — ghi notes.md.
- Ảnh hưởng: Program.cs (KnownProxies), GamificationController.cs (mock-pay default), notes.md.

## [2026-08-13 14:30] Fix E-Major (XFF + mock-pay) + Đợt F (Perf 8 lỗi) xong
- Quyết định: XFF — ForwardedHeadersOptions tường minh: loopback v4/v6 luôn tin + DSA:Proxy:KnownProxies từ config; internet không tin (chống spoof rate limiter). Mock-pay fail-closed: default false, Dev=true, Prod=false. Keyset pagination additive (lastXp/lastId/lastSubmittedAt params — không breaking, fallback offset); index Xp + LastActivityDate (migration AddLeaderboardIndexes); admin stats 1 query; trace streaming page; class EXISTS; favorites/benchmark catalog 1 lần; feedback GroupBy SQL; EnterNode đọc Users 1 lần. Kết quả: 117/117 unit + 77/77 integration ×2.
- Ảnh hưởng: Program.cs, GamificationController/Service, ExerciseService, AdminController, CodeRunnerService, FavoriteService, FeedbackController, UserConfiguration, migration mới, DEPLOY.md.

## [2026-08-13 15:00] Đợt F verify PASS — chuyển Đợt G (Surface)
- Quyết định: Đợt F (perf 8 lỗi + E-Major 2 điểm) verify độc lập PASS 2/2 (117 unit + 77 integration). Rủi ro ghi nhận: keyset trên sort biến động (cursor cố hữu, fallback offset giữ), JsonDocument vẫn parse O(total) (giảm materialize), thiếu unit test ParseTracePage invalid (chấp nhận).
- Ảnh hưởng: ghi notes.md.

## [2026-08-13 15:40] Kết thúc phiên audit backend — tổng kết
- Quyết định: Tất cả 8 đợt fix hoàn tất, verify độc lập + review từng đợt. Cuối phiên: Unit 121 PASS (baseline 85 → +36 test mới), Integration 77 PASS (baseline 31 → +46 test mới). Mọi test CAO/TRUNG có test tái hiện trước + fix + review. 14/14 CAO đã fix; 30/37 TRUNG đã fix (7 còn lại ghi notes.md — chủ yếu multi-instance/thiết kế chấp nhận); THAP còn lại ghi notes.md.
- Ảnh hưởng: toàn bộ backend/src (8 migrations mới, ~20 services/controllers, 17 validators) + 10+ file test. KHÔNG commit (local), KHÔNG merge dev. Việc tồn đọng: dev-docs cập nhật TEST_PLAN/API_REFERENCE (số test + params keyset + ClientRequestId + EnableMockPay), FE chuyển cursor + gửi ClientRequestId.
