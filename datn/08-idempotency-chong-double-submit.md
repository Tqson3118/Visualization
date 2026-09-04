# 08. Idempotency và chống double submit

## Vấn đề
User double-click, mạng retry hoặc hai request song song có thể tạo hai submission và cộng thưởng hai lần.

## Cách xử lý trong project
- Client gửi `clientRequestId`.
- Service tìm request đã xử lý trước đó.
- `SubmissionLockRegistry` hạn chế race trong một instance.
- Unique constraint/database bảo vệ tầng cuối.
- Nếu request trùng, trả lại submission cũ thay vì insert bản ghi mới.

## Code cần tra
- `backend/src/DsaVisual.Application/Services/ExerciseService.cs`
- `backend/src/DsaVisual.Application/Services/SubmissionLockRegistry.cs`
- `backend/src/DsaVisual.Application/Persistence/Configurations/ExerciseConfiguration.cs`
- Migration có tên liên quan `SubmissionUniqueConstraints`.

## Câu hỏi sâu
Lock trong memory chỉ bảo vệ một server instance. Khi chạy nhiều instance, unique constraint và idempotency lưu trong database mới là lớp bảo vệ đáng tin cậy.

## Checklist phải học thuộc
Biết idempotency key nằm trong request hay header, unique index gồm các column nào, lock scope là gì và duplicate request trả response nào.

## Cách tra code
Đọc đoạn kiểm tra existing record trước insert, đoạn catch unique violation và migration tạo unique constraint. So sánh quiz submit với code submit.

## Câu hỏi khó
Hai request cùng key nhưng source khác nhau phải bị reject hoặc xử lý conflict; không được trả kết quả của request khác một cách mù quáng.

## 7. Flow double submit từ nút UI đến response

1. Bắt đầu tại nút submit trong `LessonStepCodeLab.vue:475` hoặc `codeRunner.ts:226`. Kiểm tra có cờ loading/disable nút trước request không.
2. Request qua `lessonApi.ts:93-100` hoặc `api/codeRunner.ts:64`; kiểm tra clientRequestId có được gửi trong body/header không.
3. Backend vào `ExercisesController.cs:135-146`, rồi `ExerciseService.cs:818`.
4. Đọc các đoạn idempotency quanh `ExerciseService.cs:964-1021`: tìm record cũ, insert winner, bắt unique violation và đọc lại winner.
5. Đọc `SubmissionLockRegistry.cs` để hiểu lock một process; migration unique constraint là lớp database.
6. Response của request đầu tiên và request lặp quay về cùng UI; UI không được cộng điểm hai lần hoặc tạo hai lịch sử hiển thị.

**Câu hỏi cần tự trả lời:** nếu request timeout ở browser nhưng server đã lưu, retry có trả record cũ không? Nếu cùng key nhưng code khác, code có reject conflict không?

## Flow diễn giải bằng lời
User double-click hoặc browser retry; UI disable nút chỉ là UX. Request đi từ submit handler → API → `ExercisesController.cs:135-146` → `ExerciseService.cs:818`. Service dùng client request id để tìm operation cũ; nếu chưa có thì judge và insert. Request song song được lock trong `SubmissionLockRegistry`, còn unique constraint database bảo đảm chỉ một bản ghi thắng. Request trùng đọc kết quả cũ và trả về UI, nên không nhân đôi submission, XP hay history.
