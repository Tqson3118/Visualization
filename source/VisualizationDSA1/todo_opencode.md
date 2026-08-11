# TODO LIST CHO OPENCODE (Xử lý Option 1)

**Bối cảnh:** Nhánh hiện tại đã có các sửa đổi (uncommitted) xóa bỏ `MockCodeJudgeService.cs` và tháo fallback `leetCodeId = "two-sum"` trong `LessonController.cs`.

**Nhiệm vụ của bạn (opencode):**

1. [ ] **Xác minh thay đổi:** Review lại `git status` và `git diff` để đảm bảo code thừa A4 và A6 đã được gỡ đúng chuẩn, không gây lỗi cú pháp.
2. [ ] **Kiểm thử tự động:** Chạy lệnh `dotnet test` tại thư mục `backend/` để đảm bảo toàn bộ (45/45) test vẫn xanh.
3. [ ] **Cập nhật File Kế Hoạch:**
   - Mở file `plan/review/mock-remaining.md` và đánh dấu `[x]` cho mục **A4** và **A6**.
4. [ ] **Cập nhật Tracking (BẮT BUỘC theo AGENTS.md):**
   - Cập nhật file `plan/tracking/progress.md`: Ghi rõ đã hoàn thành gỡ bỏ dữ liệu giả (A4, A6).
   - (Tùy chọn) Cập nhật `plan/tracking/errors.md` nếu có fix lỗi nào phát sinh lúc test.
5. [ ] **Commit Code:** Thực hiện commit các thay đổi này với format chuẩn: `chore: remove mock code judge service and fallback leetcode id`.

Hãy thực hiện từng bước và báo cáo lại kết quả sau khi hoàn thành!
