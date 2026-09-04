# 01. Nộp CodeLab và chấm test case

## Mục tiêu
Hiểu từ lúc học viên bấm **Nộp bài** đến lúc backend quyết định Accepted/Failed.

## Flow
`ExerciseView.vue` gửi `sourceCode`, `language`, `clientRequestId` đến:

`POST /api/v1/exercises/{id}/code-submit`

Backend xử lý theo chuỗi:

1. `ExercisesController.SubmitCode` nhận request.
2. `ExerciseService.SubmitCodeAsync` kiểm tra bài tập, quyền và request.
3. Lấy cấu hình CodeLab và parse test case.
4. `CodelabJudgeService` chạy code với từng input.
5. So sánh output thực tế với `expectedOutput`.
6. Tính số test pass, score và status.
7. Lưu lịch sử vào `CodeSubmissions`.
8. Cập nhật progress, XP/quest nếu đạt điều kiện.

## Code cần tra
- `frontend/src/views/ExerciseView.vue`
- `frontend/src/features/lesson/utils/codelabTaskRegistry.ts`
- `backend/src/DsaVisual.Api/Controllers/ExercisesController.cs`
- `backend/src/DsaVisual.Application/Services/ExerciseService.cs`
- `backend/src/DsaVisual.Application/Services/CodelabJudgeService.cs`
- `backend/src/DsaVisual.Application/Dtos/CodeSubmitRequest.cs`
- `backend/src/DsaVisual.Application/Persistence/Entities/CodeSubmission.cs`

## Ví dụ
`input = [[1,3,5,7], 5]`, `expectedOutput = 2`. Judge gọi entry function của bài, nhận output thực tế rồi chuẩn hóa và so sánh.

## Câu hỏi bảo vệ
- Vì sao chấm ở backend? Để không tin kết quả do frontend gửi và tránh sửa bằng DevTools.
- Test ẩn để làm gì? Để chống hard-code theo test công khai.
- Code chạy vô hạn? Phải có timeout và trả trạng thái Timeout.
- Score tính ở đâu? Backend là nguồn kết quả chính thức.

## Checklist phải học thuộc
- Request DTO gồm field nào và field nào server tự lấy từ token.
- Config bài tập lưu ở đâu, parse test case bằng method nào.
- Judge gọi entry function ra sao.
- Cách phân biệt Accepted, Wrong Answer, Runtime Error và Timeout.
- Bản ghi CodeSubmission được tạo trước hay sau khi cập nhật progress.

## Cách tra code
Đọc Controller trước để biết endpoint; sang ExerciseService để biết business rule; cuối cùng đọc CodelabJudgeService để hiểu execution và compare. Mỗi method hãy ghi input, output, side effect và exception.

## Câu hỏi khó
Nếu client gửi score hoặc passedTests thì server có dùng không? Không, server phải tự tính. Nếu một hidden test lỗi thì kết quả tổng thể là gì? Đọc rule thật trong judge; thường submission không Accepted.

## 9. Flow theo file và dòng code

**Bắt đầu từ UI:**
1. Mở `frontend/src/views/lesson/components/LessonStepCodeLab.vue:475`. Hàm submit lấy `props.exerciseId`, code hiện tại qua `currentCode()`, rồi gọi `submitCodelab(...)`.
2. Sang `frontend/src/features/lesson/services/lessonApi.ts:93-100`. Hàm `submitCodelab` tạo HTTP POST tới `/exercises/{exerciseId}/code-submit`; đọc body thực tế ở các dòng 94 trở đi.
3. Backend vào `backend/src/DsaVisual.Api/Controllers/ExercisesController.cs:135-146`. Attribute dòng 135 là route, method dòng 137 nhận request, dòng 146 gọi `_service.SubmitCodeAsync(...)`.
4. Sang `backend/src/DsaVisual.Application/Services/ExerciseService.cs:818`. Đọc từ method `SubmitCodeAsync` đến đoạn tạo `CodeSubmission`, xử lý idempotency và cập nhật progress.
5. Tra `backend/src/DsaVisual.Application/Services/CodelabJudgeService.cs`: dùng Ctrl+F các từ `TryParseTasks`, `Compare`, `Execute`, `Timeout`, `expectedOutput` để tìm chính xác đoạn parse/chạy/so sánh.
6. Kết quả quay về DTO ở Controller; quay lại Promise trong `lessonApi.ts`, rồi về `LessonStepCodeLab.vue` để cập nhật loading, console, score/status và thông báo UI quanh đoạn submit.

**Điểm kết thúc ở UI:** response từ `submitCodelab` được gán vào state kết quả; component render Accepted/Failed, số test pass và lỗi. Hãy mở tiếp các dòng sau 475 để ghi đúng tên biến UI thực tế.

## Flow diễn giải bằng lời
Khi học viên mở bài, UI hiển thị editor. Bấm Nộp tại `frontend/src/views/lesson/components/LessonStepCodeLab.vue:475` sẽ lấy code và gọi `submitCodelab`. Hàm HTTP ở `frontend/src/features/lesson/services/lessonApi.ts:93-100` gửi POST tới backend. `ExercisesController.cs:135-146` nhận request và gọi `ExerciseService.SubmitCodeAsync` tại `ExerciseService.cs:818`. Service lấy config/test case, gọi các hàm parse/execute/compare trong `CodelabJudgeService.cs`. Sau khi chấm, service lưu kết quả và trả DTO. Promise quay về component; component tắt loading, cập nhật score/status và UI hiện Accepted, Failed, Error hoặc Timeout.
