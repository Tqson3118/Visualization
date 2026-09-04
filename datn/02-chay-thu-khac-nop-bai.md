# 02. Chạy thử khác nộp bài

## Chạy thử
- Mục đích: giúp học viên debug.
- Có thể chỉ chạy test case công khai.
- Thường không tạo thành tích chính thức.
- Có thể dùng Web Worker/compiler runner ở frontend.

## Nộp bài
- Gửi source code lên backend.
- Backend chấm lại từ đầu, gồm test case ẩn.
- Lưu vào `CodeSubmissions`.
- Cập nhật điểm, progress, quest và unlock.

## Điểm cần nhớ
Không được lấy kết quả của nút **Chạy thử** làm kết quả chính thức. Frontend chỉ hiển thị; backend mới quyết định submission có Accepted hay không.

## Code cần tra
- `frontend/src/views/CodeRunnerView.vue`
- `frontend/src/core/CompilerStepExecutor.ts`
- `frontend/src/engines/worker/compileWorker.ts`
- `backend/src/DsaVisual.Application/Services/CodelabJudgeService.cs`
- `backend/src/DsaVisual.Application/Services/ExerciseService.cs`

## Checklist phải học thuộc
So sánh Run và Submit theo 5 tiêu chí: nơi chạy, loại test, độ tin cậy, có lưu DB không, có tạo reward không. Đừng chỉ nói hai nút khác tên.

## Cách tra code
Tìm handler của nút Run trong View, lần tới worker/compiler; sau đó tìm handler Submit và lần tới API. Đánh dấu ranh giới client-trusted và server-trusted.

## Câu hỏi khó
Nếu Run pass nhưng Submit fail thì có mâu thuẫn không? Không; Run có thể thiếu hidden tests hoặc môi trường khác.

## 8. Flow Run và Submit theo file/dòng

**Run trong Code Runner:**
1. Bắt đầu tại `frontend/src/stores/codeRunner.ts:130-206`: đọc hàm chạy code, worker/compiler và đoạn lưu kết quả.
2. Worker được gọi qua các import/handler trong `frontend/src/core/CompilerStepExecutor.ts` và `frontend/src/engines/worker/compileWorker.ts`; tra các từ `run`, `execute`, `postMessage`.
3. Kết quả được gán vào `lastRun` tại dòng 196, cập nhật danh sách ở dòng 197 và lưu localStorage ở dòng 198/208.
4. Nếu có lưu server, dòng 159 gọi `saveCodeRun`; phân biệt đây là history/run chứ không phải code submission.

**Submit chính thức:**
1. `frontend/src/stores/codeRunner.ts:226` gọi `codeRunnerApi.submitCode(...)`.
2. `frontend/src/api/codeRunner.ts:64` tạo request; endpoint được khai báo ở dòng 8.
3. Backend tiếp tục `ExercisesController.cs:135-146` → `ExerciseService.cs:818` → `CodelabJudgeService.cs`.
4. Response quay về store, store cập nhật state; View đọc state và hiển thị status/score.

**Cần chốt khi học code:** Run lưu local/server ở đâu, Submit lưu CodeSubmission ở đâu, nút nào cập nhật điểm và nút nào chỉ hiển thị output.

## Flow diễn giải bằng lời
Bấm Chạy thử thì code đi vào runner/worker ở `frontend/src/stores/codeRunner.ts:130-206`, kết quả được gán vào `lastRun` tại dòng 196, cập nhật history tại 197-208 rồi UI hiển thị console. Bấm Nộp bài thì store gọi API tại `codeRunner.ts:226`, đi qua `api/codeRunner.ts:64` và backend judge. Response chính thức quay về store/component để hiển thị score/status. Vì Run có thể chỉ chạy public/sample test và có thể lưu localStorage, còn Submit chạy lại test chính thức và ghi nhận progress, Run pass nhưng Submit fail vẫn là bình thường.
