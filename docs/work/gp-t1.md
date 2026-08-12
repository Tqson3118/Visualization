# GP-T1 — POST /lessons/{id}/feedback (đánh giá nội dung bài học)

- **Trạng thái**: DONE ✅
- **Nhánh**: `feature/feedback-endpoint` (tạo từ dev `b4618aa`)
- **Ngày**: 2026-08-13
- **Nguồn**: PROMPT_G_PHU task 1 (FR-7.4 / API_REFERENCE §4.15 / SDD §7.3.21)

## Vấn đề
- `LessonDetail.vue` nút "Gửi đánh giá" chỉ toast giả — không lưu được (TODO dòng 94).
- Entity `ContentFeedback` đã có sẵn (DbSet + migration InitialCreate + UNIQUE index `IX_ContentFeedback_UserId_LessonId`) nhưng chưa có endpoint RESTful theo lesson.

## Giải pháp
Backend (chuẩn đợt D: controller mỏng + Result/ErrorCodes + service chứa logic, CẤM Repository):

| File | Thay đổi |
|---|---|
| `src/DsaVisual.Application/Dtos/FeedbackDtos.cs` | + `LessonFeedbackRequest {Rating 1-5, Comment? ≤1000}` + `FeedbackSavedDto {LessonId, Rating}` |
| `src/DsaVisual.Application/Validators/LessonFeedbackRequestValidator.cs` | **MỚI** — FluentValidation: Rating `InclusiveBetween(1,5)`, Comment `MaximumLength(1000)` |
| `src/DsaVisual.Application/Services/ILessonService.cs` | + `AddFeedbackAsync(userId, role, lessonId, request, ct)` |
| `src/DsaVisual.Application/Services/LessonService.cs` | + `AddFeedbackAsync`: validate → lesson tồn tại/active (student) → đã "Đánh dấu đã học" (403, giữ rule v2.9 API_REFERENCE §4.15) → **upsert ContentFeedback** (tạo mới với CreatedAt / lần 2 chỉ update Rating+Comment+UpdatedAt) → trả `FeedbackSavedDto` |
| `src/DsaVisual.Api/Controllers/LessonsController.cs` | + `POST /lessons/{id}/feedback` `[Authorize]` — controller mỏng, MapResult |
| `src/DsaVisual.Api/Program.cs` | DI đăng ký `IValidator<LessonFeedbackRequest>` |
| `Persistence/Configurations/ProgressConfiguration.cs` | Comment `HasMaxLength(200)` → `1000` (khớp DTO) |
| `Persistence/Migrations/20260812180545_WidenContentFeedbackComment` | **MỚI** — chỉ alter 1 cột Comment nvarchar(200)→nvarchar(1000) |
| `tests/DsaVisual.UnitTests/LessonServiceTests.cs` | + 5 test AddFeedback (tạo mới / lần 2 update không nhân đôi / rating 6 → VALIDATION_FAILED / chưa học → FORBIDDEN / lesson 9999 → NOT_FOUND) |
| `tests/DsaVisual.UnitTests/TestServices.cs` | CreateLessonService truyền validator mới |

Frontend:

| File | Thay đổi |
|---|---|
| `src/api/lessons.ts` | + endpoint `feedback(id)`, + `submitLessonFeedback(id, {rating, comment?})` |
| `src/components/lesson/LessonDetail.vue` | `submitRating` gọi API thật + toast `lib/toast.ts` (success/error), gỡ TODO |

## Verify
- `dotnet build DsaVisual.sln` → **0 warning / 0 error** ✅
- `dotnet test tests/DsaVisual.UnitTests` → **65 PASS / 0 FAIL** (60 cũ + 5 mới) ✅
- `npm run build` (vue-tsc + vite) → **0 lỗi** ✅
- `npm test` (vitest) → **78 PASS / 9 files** ✅
- Smoke thật (docker: sqlserver + backend localhost:5000, sau `--seed` idempotent áp migration):
  - `POST /api/v1/lessons/1/mark-viewed` + token → **204**
  - `POST /api/v1/lessons/1/feedback {rating:5, comment}` + token → **200** `{"lessonId":1,"rating":5}`
  - Gửi lại `{rating:4, comment:"Updated"}` → **200** `{"lessonId":1,"rating":4}`
  - `{rating:9}` + token → **400 VALIDATION_FAILED** (details Rating)
  - Không token → **401**
  - DB (sqlcmd): đúng **1 bản ghi** UserId=3/LessonId=1, rating=4, comment="Updated", CreatedAt+UpdatedAt đủ → upsert không nhân đôi ✅

## Ghi chú
- Giữ nguyên `FeedbackController` cũ (`POST /feedback`, lessonId trong body) — ngoài phạm vi, không đụng.
- Migration là widen an toàn (nvarchar 200→1000), áp được lên DB đang chạy.
- Commit: backend (bao) + frontend (son) + docs (phuc) trên nhánh `feature/feedback-endpoint`, **chưa merge** (chờ verify/review theo quy trình).
