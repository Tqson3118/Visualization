# TODO LIST CHO OPENCODE (Xử lý B2 & B3)

**Bối cảnh:** Xảy ra tình trạng lệch chuẩn (mismatch contract) giữa API Codelab Frontend và Backend. Cụ thể: Backend tồn tại 2 file Controller trùng lặp, còn Frontend đang gọi sai prefix (không có `/v1/`).

**Nhiệm vụ của bạn (opencode):**

1. [ ] **Hợp nhất Controller Backend (B3):**
   - Xóa file `backend/src/WebApi/Controllers/CodelabsController.cs` (bản legacy).
   - Di chuyển 3 endpoints: `SubmitCodelab`, `RunCodelab`, và `RevealHint` sang `backend/src/WebApi/Controllers/CodelabController.cs`.
   - **Đặc biệt lưu ý RevealHint:** Đổi route endpoint này thành `[HttpPost("{id}/hints/{hintIndex}/reveal")]` và lấy biến `hintIndex` trực tiếp từ URL thay vì từ Body, để khớp 100% với hàm gọi của Frontend hiện tại. (Nhớ xóa class `RevealHintRequestDto` thừa).

2. [ ] **Đồng bộ Frontend Contract (B2):**
   - Mở file `frontend/src/features/codelabs/api/codelabApi.ts`.
   - Tìm cách import `apiClient` chuẩn của dự án thay vì import `axios` thuần (nếu dự án có sẵn file `apiClient` cấu hình prefix). Hoặc ít nhất, sửa tất cả đường dẫn `/api/codelabs/...` thành `/api/v1/codelabs/...`.

3. [ ] **Xác minh qua Kiểm thử:**
   - Chạy `dotnet test` trong thư mục `backend` (việc xóa `CodelabsController` có thể tác động đến một số unit test legacy, hãy sửa các test đó chuyển sang dùng `CodelabController`).
   - Chạy `npm run build-only` hoặc kiểm tra type bằng lệnh `npm run type-check` ở frontend (tùy script có sẵn) để đảm bảo không vỡ code.

4. [ ] **Cập nhật File Tracking (BẮT BUỘC):**
   - Mở `plan/review/mock-remaining.md` và đánh dấu `[x]` vào **B2** và **B3**.
   - Cập nhật `plan/tracking/progress.md`: Ghi nhận hoàn thành hợp nhất API Codelabs.
   - Thêm một mục (ADR nhỏ) vào `plan/tracking/decisions.md`: "Refactor: Xóa CodelabsController, gom toàn bộ CRUD và Submit/Run về chung CodelabController (/api/v1/codelabs) để tránh phân mảnh."

5. [ ] **Commit Code:**
   - Message: `refactor: merge codelabs controllers and sync frontend api contract (B2, B3)`
