# 09. Class assignment và deadline

## Flow
Teacher tạo class → sinh invite code → student tham gia → teacher chọn exercise → tạo assignment và hạn nộp → student submit → backend đối chiếu thời gian → teacher xem tiến độ.

## Quan hệ chính
- `ClassMember`: ai thuộc lớp.
- `ClassAssignment`: bài nào được giao cho lớp nào, hạn nào.
- `ExerciseSubmission`: kết quả nộp của student, có thể gắn assignment.

## Code cần tra
- `backend/src/DsaVisual.Application/Services/ClassService.cs`
- `backend/src/DsaVisual.Api/Controllers/ClassesController.cs`
- `backend/src/DsaVisual.Application/Persistence/Entities/Class.cs`
- `ClassMember.cs`, `ClassAssignment.cs`
- `backend/src/DsaVisual.Application/Persistence/Configurations/ClassConfiguration.cs`

## Câu hỏi sâu
Submission trễ có thể bị chặn hoặc vẫn lưu với trạng thái late, tùy business rule. Quan trọng là server dùng UTC và không tin thời gian từ client.

## Checklist phải học thuộc
Phân biệt exercise gốc với assignment của một lớp; biết membership check, archived check, DueAt, AllowLateSubmission và UTC conversion.

## Cách tra code
Đọc ClassService cho create/list/report, đọc ExerciseService cho submit, rồi đối chiếu ClassAssignmentConfiguration và FK.

## Câu hỏi khó
Student thuộc lớp A có nộp assignment lớp B được không? Không nếu service kiểm tra membership. Teacher xem được dữ liệu nào? Tùy owner/scope, phải tìm authorization.

## 7. Flow assignment/deadline có UI

1. Student UI mở bài từ class; tra `frontend/src/views/ClassDetailView.vue` và `ClassAssignmentSubmissionsModal.vue`.
2. Teacher chỉnh deadline tại `frontend/src/views/ClassDetailView.vue:409-450`; dòng 409 là `handleSaveDeadline`, dòng 450 gọi `classesApi.updateLessonDeadline`.
3. API deadline nằm `frontend/src/api/classes.ts:138`; lần tiếp tới ClassesController/ClassService.
4. Khi student submit, đi theo flow CodeLab ở `LessonStepCodeLab.vue:475` hoặc codeRunner store; request có thể mang classAssignmentId.
5. Backend ClassService/ExerciseService kiểm tra membership, assignment, DueAt và UTC.
6. Response quay về student UI để hiện submitted/late/error; teacher UI gọi lại danh sách và hiển thị report/submission modal.

**Cần đọc thêm:** `ClassAssignmentSubmissionsModal.vue` để biết chính xác teacher xem những field nào và trạng thái nào được hiển thị.

## Flow diễn giải bằng lời
Teacher lưu deadline tại `ClassDetailView.vue:409-450`, gọi `classes.ts:138`; backend kiểm tra quyền rồi lưu. Student mở assignment và nộp code; backend lấy user từ token, kiểm tra membership, assignment active và so sánh giờ server UTC với DueAt. Nếu trễ, áp dụng AllowLateSubmission hoặc từ chối. Response quay về student UI để hiện submitted/late/error. Teacher mở `ClassAssignmentSubmissionsModal.vue`, gọi API report và UI render điểm, trạng thái, thời gian sau khi backend authorize.
