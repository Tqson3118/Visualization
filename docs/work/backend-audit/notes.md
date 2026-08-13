# Notes — Fix exception handling (findings-exception.md #1–#5)

Ngày: 2026-08-13 — Nhánh: feature/backend-audit
Phạm vi: sửa theo findings-exception.md #1 (CAO), #3, #4 (TRUNG), #5 (THAP); test tái hiện đã có (đỏ → xanh).


## Quyết định: KHÔNG migrate sang IExceptionHandler / IProblemDetailsService (finding #2 — TRUNG)

- Middleware `ErrorHandlingMiddleware` hiện hoạt động đúng: log exception đúng 1 lần tại boundary,
  xử lý OperationCanceledException, ẩn chi tiết production, trả envelope `{ error }` theo API_REFERENCE §2.1.
- Test liên quan đang xanh; migrate sang `AddExceptionHandler` + `AddProblemDetails` (RFC 9457) sẽ chạm
  toàn pipeline (thứ tự middleware, format body, xử lý cancellation, log context) — rủi ro cao, lợi ích
  thấp khi contract §2.1 vẫn là envelope custom.
- Hành động trong phiên này: CHỈ chuẩn hóa trên nền middleware hiện tại:
  - #1: unique violation (SqlException 2627/2601) → 409 CONFLICT/EMAIL_EXISTS thay vì 503.
  - #3: GamificationService.EnterNodeAsync chỉ bắt unique violation của NodeSessions; lỗi DB khác rethrow.
  - #4: mọi lỗi HTTP trả envelope §2.1 — 400 model binding (InvalidModelStateResponseFactory),
    401/403 JWT (JwtBearerEvents OnChallenge/OnForbidden), 404 route không khớp (MapFallback).
  - #5: ApiControllerBase claim thiếu/malformed → UnauthorizedAccessException → middleware map 401.

## Bỏ qua (ghi lại, KHÔNG xử lý trong phiên này)

- #6: comment "Development/Staging" vs code chỉ `IsDevelopment()` — an toàn hơn, chỉ lệch comment.
- #7 (ExerciseService CSV), #8 (correlationId trong body), #9 (MapResultExtensions field đầu tiên) — ngoài
  phạm vi 5 findings được yêu cầu; đề xuất xử lý ở phiên sau (mỗi mục đều cần test riêng).

---

# Notes — Fix SettingService cache (findings-biz-services.md #17, #18)

Ngày: 2026-08-13 — Nhánh: feature/backend-audit
Phạm vi: sửa theo findings-biz-services.md #17 (TRUNG), #18 (THAP); test tái hiện
`tests/DsaVisual.UnitTests/SettingServiceRegressionTests.cs` (đỏ → xanh).

## Đã fix

- **#17a (race nạp cache)**: `SettingsCache` (Singleton) thêm `LoadOnceAsync` — `SemaphoreSlim` + double-checked
  locking. KHÓA PHẢI nằm trong cache singleton vì `SettingService` là **Scoped** (Program.cs:190): mỗi request
  một service → lock trong service không chặn được 2 request cùng load. `EnsureLoadedAsync` của service ủy quyền
  cho `cache.LoadOnceAsync(...)`.
- **#18 (cache ≠ DB khi save fail)**: `UpdateAsync` chuyển `cache.Upsert` xuống SAU `db.SaveChangesAsync` thành
  công — cache chỉ phản ánh DB đã commit.

## Quyết định: CHẤP NHẬN giới hạn single-instance cho #17b (multi-instance stale)

- Cache Settings là in-process per-instance: PUT ở instance A không invalidate instance B → instance B stale mãi
  (AuthService đọc policy từ cache này). Đồ án **chạy 1 instance** (như SubmissionLockRegistry) → chấp nhận,
  ghi comment giới hạn ở `SettingsCache.cs` + `SettingService.cs`, test pin hành vi hiện tại:
  `UpdateAsync_OtherInstanceWithWarmCache_StaysStale_DocumentedSingleInstanceLimit`.
- Nếu scale-out sau này: invalidate qua signal (Redis pub/sub / DB version stamp) — lúc đó phải sửa test pin ở trên.

## Lưu ý còn lại (KHÔNG fix trong phiên này)

- Cửa sổ hẹp còn lại (single-instance): PUT (`cache.Upsert`) xen giữa lúc load lần đầu đang chạy (A đọc DB →
  A SetAll) có thể bị SetAll ghi đè bằng dữ liệu cũ — chỉ xảy ra ở lần nạp đầu tiên sau khởi động, rủi ro rất
  thấp; nếu muốn đóng: cho `cache.Upsert` đi qua cùng lock (cần đổi thành async).


---

# Notes — Tổng hợp cuối phiên audit (đầy đủ)

Ngày: 2026-08-13 · Nhánh: feature/backend-audit (làm việc LOCAL — session frontend chiếm working tree, KHÔNG commit, KHÔNG merge dev)

## Trạng thái tổng (93 findings ban đầu → 8 đợt fix)

| Đợt | Phạm vi | Findings | Kết quả |
|-----|---------|----------|---------|
| A | GamificationService (7 lỗi) + RowVersion | BUG-1, #2-#7 | DONE — 92 unit + 36 integration |
| B | AuthService (7 lỗi) | biz#2,#3,#8,#9,#10,#19 + sec#5,#15,#16 | DONE — 98 + 45 |
| C | Exception handling (5 lỗi) | exc#1,#3,#4,#5 (+#2 quyết định) | DONE — 98 + 45 |
| D | Exercise/Class/Progress (11 lỗi) + 2 Major thiết kế | biz#1,#5,#6,#7,#15 + sec#1,#11 + perf#1,#2,#3,#8 | DONE — 105 + 56 → 121 + 77 cuối phiên |
| E | Security (10 lỗi) + E-Major XFF/mock-pay | sec#2,#6,#7,#8,#9,#10,#12,#13,#17,#18 | DONE — 117 + 67 |
| F | Performance (8 lỗi) | perf#4,#5,#6,#7,#9,#10,#11 + #19 index | DONE — 117 + 77 |
| G | Bề mặt (5 lỗi) | surface#1-#5 | DONE — 117 + 77 |
| H | SettingService (2 lỗi) | biz#17,#18 | DONE — 121 + 77 |

## Thay đổi kiến trúc/quyết định quan trọng (chi tiết: pm-decision-log-backend.md)

1. BUG-1: lazy fix premium downgrade — EnsureHeartsMaxSyncAsync clamp atomic khi đọc hearts (KHÔNG hosted service).
2. RowVersion [Timestamp] cho 7 entity (User, UserQuest, UserProgress, UserNodeProgress, UserInventory, PremiumSubscription, NodeSession) — migration AddRowVersionConcurrency; DbUpdateConcurrencyException → 409 CONFLICT.
3. Double-submit: bỏ unique vĩnh viễn (vi phạm FR-4.4/FR-9.5) → ClientRequestId optional + filtered unique WHERE ClientRequestId IS NOT NULL (migration AddSubmissionUniqueConstraints + RemovePermanentSubmissionUnique).
4. security#1 (tin điểm client): CLAMP score [0,maxScore] + IsClientDeclared=true (giữ client-grading ADR-012, không server re-grade).
5. Rate limiter: partition user claim+IP, /health ngoại lệ, 429+Retry-After+envelope; keys DSA:RateLimit:*.
6. Mock-pay fail-closed: EnableMockPay default FALSE (Dev=true, Prod=false).
7. ForwardedHeaders: KnownProxies config + loopback trust (chống XFF spoof).
8. KHÔNG migrate IExceptionHandler (giữ middleware + chuẩn hóa envelope §2.1).
9. Quest Progress: QuestProgressWriter (atomic UPDATE) + hooks Submit/SubmitCode/MarkViewed + sync-on-read; KHÔNG map quest "streak" (ghi chú code).
10. Sanitizer whitelist 13 tag + 0 attribute (teacher content mất link/ảnh/table — trade-off chống phishing, ghi USER_GUIDE nếu cần).
11. Keyset pagination additive (lastXp/lastId/lastSubmittedAt — không breaking, fallback offset); index Xp/LastActivityDate (AddLeaderboardIndexes).
12. SettingService: cache.Upsert sau SaveChanges + LoadOnceAsync (SemaphoreSlim singleton); chấp nhận single-instance (test pin).

## Còn lại (KHÔNG xử lý phiên này — đề xuất phiên sau, mỗi mục cần test riêng)

- exc#6 comment Dev/Staging lệch; exc#7 CSV catch rộng; exc#8 correlationId trong body; exc#9 MapResultExtensions field đầu.
- biz-gami #8 orderRef trùng (DSV{userId}T{months} không phân biệt 2 order); #9 streak atomic (race freeze); #11 clamp hearts < 0 defense; #12 premium gating thiếu (benchmark/cheatsheet/hint không gate) + 2 nguồn sự thật status; #13 leaderboard week xếp theo tổng XP (cần xác nhận SRS).
- biz-services #20 invite code retry khi conflict; #21 chặn owner tự xóa khỏi lớp; #22 ladder TOCTOU (chấp nhận single-instance); #23 CodeRunner trace không nén (lệch comment) + Enum.TryParse default Success; #24 tham chiếu.
- security #3 LoginAttemptTracker in-memory (multi-instance cần Redis/DB — NFR-12); #4 refresh TOCTOU đã fix, replay-revoke ngoài tx best-effort; #14 student thấy exercise Draft (lọc Status Active); #19 vòng đời token (chấp nhận JWT ≤60p) + cookie Expires hardcode 7 ngày nên đọc config.
- perf #14/#15/#16 offset lessons/topics/users (bảng nhỏ — chấp nhận); #20 index Lessons; #23 SiblingNameExists collation (chấp nhận).
- TEST_PLAN/API_REFERENCE: cập nhật số test mới (121 unit + 77 integration = 198; trước 81/31) + params mới keyset (lastXp/lastId/lastSubmittedAt) + ClientRequestId + EnableMockPay config — giao dev-docs.
- FE: chuyển leaderboard/submissions sang cursor khi deep-page; gửi ClientRequestId khi submit (chống double-submit multi-instance).

## Flake đã theo dõi
- AuthRegressionTests.Verify2Fa_TwoConcurrentSameOtp từng fail 1/3 lần (race test — chạy riêng PASS); OTP lock đã thêm — nếu tái diễn, tăng iteration hoặc dùng DB-backed counter.
