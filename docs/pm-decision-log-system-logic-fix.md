# PM Decision Log — Fix blocker PR #23 (feature/system-logic-overhaul)

Ngày: 2026-08-14 · Worktree: `trees/system-logic` · Base: 9d5c551

## Lý do fix 2 type error FE (build gãy)

1. `frontend/src/views/AdminContentView.vue:218` — `formRejectionReason.value = detail.status === 'draft' ? detail.rejectionReason : ''` gán `string | null` (LessonDetailRow.rejectionReason) vào `ref<string>` → thêm `?? ''`. Không đổi logic hiển thị (vẫn chỉ hiện reason khi bài ở trạng thái draft).
2. `frontend/src/api/lessons.ts:74` — `fetchLessons` params thiếu `pageSize` nhưng `ClassDetailView.vue:156` gọi `{ page: 1, pageSize: 100 }`; backend `LessonsController.GetLessons` ĐÃ hỗ trợ `[FromQuery] int pageSize = 20` → thêm `pageSize?: number` vào type (đúng cách, không sửa call site).

## Danh sách test mới (11 unit + 1 integration)

- `LessonServiceTests.cs`: `Create_TeacherPublic_BecomesPendingReview`, `Create_TeacherClassOnly_BecomesActive`, `Review_Approve_BecomesActiveWithPublishedAt`, `Report_ShortReason_ReturnsValidationFailed`.
- `ClassServiceTests.cs` (file mới): `JoinByCode_Success_AddsMember`, `JoinByCode_UnknownCode_ReturnsNotFound`, `JoinByCode_ClosedClass_ReturnsValidationFailed`, `JoinByCode_AlreadyMember_ReturnsValidationFailed`.
- `ExerciseServiceTests.cs`: `Submit_AfterDue_AllowLateFalse_ReturnsAssignmentOverdue`, `Submit_AfterDue_AllowLateTrue_Succeeds`.
- `UserServiceTests.cs`: `ApproveTeacher_RejectWithoutReason_ReturnsValidationFailed`.
- `LessonsIntegrationTests.cs`: `Review_RejectWithoutReason_Returns400` — quy tắc "từ chối bắt buộc lý do" nằm ở CONTROLLER (`LessonsController.ReviewLesson`), service `ReviewAsync` không chặn → test qua HTTP để đúng hành vi thật.

## Ghi chú khác

- Thêm helper `TestServices.CreateClassService` (bám pattern `CreateUserService`).
- Không sửa logic khác; docs sync (API_REFERENCE…) là task riêng — chưa làm.
