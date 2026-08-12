# G-BF1 — Backend bugfix Phase 0 (feature/ux-bugfix-backend)

> Ngày: 12/08/2026 · Nhánh: `feature/ux-bugfix-backend` (từ `dev` @ fb7c1be) · Commit: `1fa8c22` (bao) + commit log này
> Nguồn: docs/SETUP_TODO.md §6.2 + §8.2-8.6 · Quyết định: docs/pm-decision-log-g.md §"Chốt phạm vi Phase 0"

## Kết quả 5 mục

| # | Bug | Trạng thái | File đã sửa |
|---|---|---|---|
| 1 | **POST /lessons/{id}/mark-viewed → 404** (P1, SETUP_TODO §6.2) | ✅ DONE | `backend/src/DsaVisual.Api/Controllers/LessonsController.cs` · `backend/src/DsaVisual.Application/Services/LessonService.cs` · `backend/src/DsaVisual.Application/Services/ILessonService.cs` |
| 2 | **Heart regen ảo không persist** (P1, F5-Major §8.2) | ✅ DONE | `backend/src/DsaVisual.Application/Services/GamificationService.cs` |
| 3 | **Duplicate QuestionId → 500** (P2, F5-Minor §8.4) | ✅ DONE | `backend/src/DsaVisual.Application/Services/ExerciseService.cs` |
| 4 | **SubmitCodeAsync thiếu SubmissionLockRegistry + Status Active** (P2, F5-Minor §8.3) | ✅ DONE | `backend/src/DsaVisual.Application/Services/ExerciseService.cs` |
| 5 | **Cookie refresh Secure=true khi dev chạy HTTP** (P3, F5-Minor §8.6) | ✅ DONE | `backend/src/DsaVisual.Api/Controllers/AuthController.cs` |

## Chi tiết từng mục

### 1. mark-viewed (DONE)
- `ILessonService.MarkViewedAsync(userId, role, lessonId, ct)` mới.
- `LessonService.MarkViewedAsync`: validate lesson tồn tại (student chỉ bài Active) → **upsert UserProgress** (1 bản ghi/(User,Lesson); lần 2 chỉ update `Viewed`/`UpdatedAt` — TEST-B-033/034) → `Result.Ok()` (204).
- Controller: `POST /lessons/{id}/mark-viewed` (`[Authorize]`, mỏng — theo mẫu controller/service SDD §5.7).
- Test: `LessonServiceTests.cs` (4 unit) + `LessonsIntegrationTests.cs` (3: TEST-B-033, TEST-B-034, 404).

### 2. Heart regen persist (DONE)
- Thêm `PersistHeartRegenAsync(userId, ct)`: nếu elapsed ≥ 1 chu kỳ regen → `UPDATE Users SET Hearts=…, LastHeartAt=…` (raw SQL, khớp chuỗi UPDATE atomic trừ tim; bỏ qua khi tim đã đầy để không dời LastHeartAt).
- `GetHeartsAsync` + `GetCurrentHeartsAsync`: ghi regen trước khi trả (hết "tim ảo").
- `EnterNodeAsync`: ghi regen **trước** lệnh trừ `Hearts = Hearts - 1 WHERE Hearts > 0` → DB=0 nhưng đã qua chu kỳ regen thì vẫn trừ được (hết HEARTS_EMPTY oan).
- `HeartConfig(user, now)`: Premium 10p/tim (max 30) / Free 30p/tim (max HeartsMax) — tách từ `ComputeHearts`.
- Test: `GamificationServiceTests.cs` +4 (GetHearts persist, at-max không dời LastHeartAt, premium 10p, EnterNode spend sau regen).

### 3. Duplicate QuestionId → 400 (DONE)
- `SubmitAsync`: trước `ToDictionary(a => a.QuestionId)` (trước fix ném ArgumentException → 500) kiểm tra trùng → `Result.Fail(VALIDATION_FAILED)` với field error `questionId`.
- Test: `ExerciseServiceTests.cs` `Submit_DuplicateQuestionId_ReturnsValidationFailed`.

### 4. SubmitCodeAsync lock + Status (DONE)
- Thêm `exercise.Status != Active → EXERCISE_CLOSED` và `SubmissionLockRegistry.TryAcquire(user,id) → SUBMISSION_IN_PROGRESS` (đồng bộ với `SubmitAsync`).
- Test: `ExerciseServiceTests.cs` +3 (valid submit + upsert progress, draft → EXERCISE_CLOSED, concurrent → SUBMISSION_IN_PROGRESS).

### 5. Cookie Secure chỉ khi HTTPS (DONE)
- `AuthController`: bỏ static `RefreshCookieOptions` (Secure=true cứng); thay `BuildRefreshCookieOptions(expires)` với `Secure = Request.IsHttps`; dùng cho Append (login/register/refresh) + Delete (logout).
- Test: `AuthIntegrationTests.cs` `Login_OverHttp_CookieIsNotSecure` (TestServer HTTP → Set-Cookie không chứa `Secure`).

## Verification

| Hạng mục | Kết quả |
|---|---|
| `dotnet build DsaVisual.sln` | ✅ 0 Warning / 0 Error |
| `dotnet test DsaVisual.sln` | ✅ Unit **56/56** (44 cũ + 12 mới) · Integration **31/31** (27 cũ + 4 mới) |
| Grep cấm `postgresql\|mediatr\|repository` (backend/src) | ✅ không có — chỉ 3 comment cũ khẳng định "KHÔNG Repository" (ADR-011/A-1) |

## File thay đổi
- Source (6): LessonsController.cs, LessonService.cs, ILessonService.cs, GamificationService.cs, ExerciseService.cs, AuthController.cs
- Test (6): LessonServiceTests.cs (mới), GamificationServiceTests.cs, ExerciseServiceTests.cs, TestServices.cs, LessonsIntegrationTests.cs, AuthIntegrationTests.cs

## Ghi chú / follow-up
- Không đụng `docs/API_REFERENCE.md` (docs → phuc theo quyết định G): cần cập nhật §4.4 — gỡ ghi chú "CHƯA TRIỂN KHAI" cho `POST /lessons/{id}/mark-viewed` (đã triển khai); giữ ghi chú cho progress/simulations còn thiếu.
- Branch hiện tại: `feature/ux-bugfix-backend` @ `1fa8c22`; dev đã merge G-BF2 (frontend) — branch này được tạo từ dev sau merge.
