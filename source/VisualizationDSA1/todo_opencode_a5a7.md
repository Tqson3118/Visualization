# TODO LIST CHO OPENCODE (Xử lý A5 & A7)

**Bối cảnh:** Frontend đã dọn dẹp xong dữ liệu giả (fallback). Bây giờ chúng ta chuyển sang xử lý các điểm fallback ở mức Trung Bình tại Backend. Quy trình **REVIEW TRƯỚC KHI COMMIT** vẫn được áp dụng nghiêm ngặt.

**Nhiệm vụ của bạn (opencode):**

0. [ ] **Commit task cũ:** Chạy lệnh `git commit -am "refactor: remove quiz and sorting mock data from frontend (B1, B4)"` cho các file đã sửa ở bước trước. (Quyết định của PM: Giữ lại hàm sinh mock làm test fixture là hợp lý, không cần xóa hẳn).

1. [ ] **Tháo gỡ Gamification Fallback (A5):**
   - Mở `backend/src/WebApi/Controllers/GamificationController.cs`.
   - Sửa logic sinh quest/XP fake (vấn đề "If empty, generate fake ones for first time"). Nếu user chưa có quest, hãy trả về mảng rỗng `[]` thay vì tạo quest ảo, hoặc xử lý lấy quest thật từ DB.

2. [ ] **Tháo gỡ AI Assistant Fallback (A7):**
   - Mở `backend/src/Infrastructure/Services/AiAssistantService.cs`.
   - Sửa các chuỗi fallback (vd: trả về string "Lỗi..."). Hiện tại nó khiến Frontend tưởng là HTTP Success. Cần ném ra ngoại lệ (`HttpRequestException` hoặc `InvalidOperationException`) hoặc chỉnh Controller trả về `500 Internal Server Error` / `501 Not Implemented` khi gọi LLM thất bại/thiếu key.

3. [ ] **Xác minh qua Kiểm thử:**
   - Chạy `dotnet test` ở backend để đảm bảo mọi thứ vẫn pass.

4. [ ] **Cập nhật File Tracking:**
   - Mở `plan/review/mock-remaining.md`, đánh dấu `[x]` cho **A5** và **A7**.
   - Cập nhật `plan/tracking/progress.md`: Ghi chú gỡ fallback Gamification và AI Assistant ở Backend.

5. [ ] **DỪNG LẠI VÀ BÁO CÁO:**
   - Báo cáo danh sách file sửa và kết quả test. **Không được tự ý commit.**
