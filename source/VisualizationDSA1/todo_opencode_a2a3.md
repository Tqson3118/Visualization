# TODO LIST CHO OPENCODE (Trận chiến cuối cùng: A2 & A3)

**Bối cảnh:** Bạn đã xuất sắc vượt qua đợt tái cấu trúc Stateless Controllers (A1/B5). Bây giờ, chúng ta sẽ dọn dẹp nốt 2 mock cuối cùng của dự án: A2 (Admin Dashboard) và A3 (Realtime Quiz Room). Hoàn thành lô task này, dự án sẽ sạch bóng mock 100%! Quy trình **REVIEW TRƯỚC KHI COMMIT** vẫn giữ nguyên.

**Nhiệm vụ của bạn (opencode):**

0. [ ] **Commit task cũ:** Chạy `git commit -am "refactor: migrate stateless controllers to real database (A1, B5)"` cho toàn bộ các file đã sửa ở bước trước.

1. [ ] **Tháo gỡ Mock ở Admin Dashboard (A2):**
   - Mở `backend/src/WebApi/Controllers/AdminController.cs`.
   - Các logic thống kê (Dashboard) và liệt kê đang gọi `_authStrategy.GetAllUsers()` và `_quizBank.GetAllQuizzes()` (in-memory).
   - Sửa lại bằng cách query trực tiếp từ `ApplicationDbContext` hoặc thông qua `IMediator` query thật của hệ thống.
   - Sau khi sửa xong A2, **XÓA BỎ HOÀN TOÀN** `StatelessAuthStrategy.cs` và `QuizBankStrategy.cs` khỏi dự án, đừng quên gỡ đăng ký DI của chúng trong file DI config.

2. [ ] **Tháo gỡ MockQuestion trong Realtime Quiz (A3):**
   - Mở file `backend/src/WebApi/Hubs/QuizRoomHub.cs` (hoặc file chứa SignalR logic tương ứng).
   - Tìm đoạn code sinh `MockQuestion` (hiện tượng sinh câu hỏi giả cứng).
   - Thay thế bằng việc load danh sách câu hỏi thật từ DB của Quiz hiện tại đang được thi (ví dụ: query qua `ApplicationDbContext`).

3. [ ] **Kiểm thử Xác minh:**
   - Chạy `dotnet test`. Các test liên quan đến `StatelessAuthStrategy` hoặc `QuizBankStrategy` (nếu có) sẽ bị lỗi do ta đã xóa class. Hãy xóa/sửa luôn các unit test này.
   - Đảm bảo Backend build 0 error, pass mọi test.

4. [ ] **Cập nhật Tracking:**
   - Mở `plan/review/mock-remaining.md`, đánh dấu `[x]` cho **A2** và **A3**.
   - Cập nhật `plan/tracking/progress.md`: Ghi chú dự án đã hoàn thành CLEAN MOCK 100%.

5. [ ] **DỪNG LẠI VÀ BÁO CÁO:**
   - Chờ lệnh duyệt commit từ PM.
