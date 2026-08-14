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

## Docs sync v2.15 (14/08/2026 — bắt buộc trước khi merge PR #23)

Đồng bộ tài liệu với code đã merge vào `feature/system-logic-overhaul` (HEAD 061f1b2 — build + test xong). Chỉ sửa docs, KHÔNG đụng code / THIRD_PARTY.

File đã sửa:
1. `docs/API_REFERENCE.md` (1.5 → 1.7): §2.2 +`ASSIGNMENT_OVERDUE` (422); §3.1 +`academicDegree`/`profileLink`; §3.4 LessonDto +`isClassOnly`/`publishedAt`/`rejectionReason` + ghi chú LessonSummaryDto; §3.5 +`isClassOnly`/`simulationKeys` + trạng thái theo vai trò; §4.1 body register; §4.4 +3 endpoint (pending/review/report) + vá ghi chú trạng thái triển khai (mark-viewed/feedback đã có) + ví dụ mới; §4.6/§4.13 nộp bài +`ASSIGNMENT_OVERDUE`; §4.8 reject reason bắt buộc + AdminUserDto stats (chỉ GET /users/{id}) + ví dụ; §4.11 +`join-by-code` +`allowLateSubmission`; §4.15 +`adminNote` (sanitize); §5 RBAC 36 → 38 hành động; §6 +mục 6 kiểm duyệt nội dung; §8 +6 dòng thay đổi phiên bản.
2. `docs/SDD.md` (1.7 → 1.8): §5.4 trách nhiệm 3 service; §5.7.2 ghi chú hành vi thật LessonService; §7.3.1 +2 cột; §7.3.2 enum mở rộng +3 cột; §7.3.16-18 +`AllowLateSubmission` + join-by-code + nộp trễ; §7.3.22 +`AdminNote` + `CONTENT_VIOLATION`; Màn 09 (ProseContent markdown, tab chờ duyệt, báo cáo vi phạm); Màn 29 (server enforce reason 400).
3. `docs/SRS.md` (1.6 → 1.7): FR-1.1 (+2 trường GV), FR-1.8 (reject bắt buộc reason), FR-2.2 (kiểm duyệt + isClassOnly + simulationKeys), FR-8.3 (allowLateSubmission), UC-21 (join-by-code).

Nguồn số liệu: đọc code thật trên branch (`LessonsController.cs`, `LessonService.cs`, `ClassService.cs`, `UserService.cs`, `AuthService.cs`, `ExerciseService.cs`, `FeedbackController.cs`, `ErrorCodes.cs`, migration `20260814003835_FullBusinessLogicAndClassOverhaul.cs`, các DTO) — không đoán.
Commit: `docs(api): dong bo API_REFERENCE/SDD v2.15 (kiem duyet, join-by-code, allow-late, inventory, AdminNote)` — push `origin feature/system-logic-overhaul`.
