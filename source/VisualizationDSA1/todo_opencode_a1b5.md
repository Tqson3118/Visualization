# TODO LIST CHO OPENCODE (ĐẠI CHIẾN A1 & B5)

**Bối cảnh:** Bạn muốn một task "lớn" và thử thách hơn? Đây là **A1 + B5** — mũi nhọn phức tạp nhất còn sót lại. Mục tiêu là loại bỏ hoàn toàn kiến trúc dữ liệu in-memory (các class như `_authStrategy`, `EnsureUserInMemory`) ẩn bên dưới các `Stateless*Controller` (Auth, Quiz, Payment, Gamification) và chuyển chúng sang dùng DB thật qua `ApplicationDbContext` hoặc các Service chính thức, đồng thời đồng bộ contract ở Frontend (B5). Quy trình **REVIEW TRƯỚC KHI COMMIT** vẫn giữ nguyên.

**Nhiệm vụ của bạn (opencode):**

0. [ ] **Dọn dẹp working tree:**
   - Commit A5 & A7 (chỉ add các file liên quan): `git commit -m "refactor: remove gamification and ai mock fallbacks (A5, A7)"`.
   - Commit dứt điểm file test tồn đọng: `git add backend/tests/VisualizationDSA.UnitTests/Services/AuthServiceTests.cs` rồi `git commit -m "chore: sync auth service tests with postgres setup"`.

1. [ ] **Đại tu Stateless*Controller (A1 - Backend):**
   - Kiểm tra `StatelessAuthController.cs` và các `Stateless[...].cs` khác.
   - Loại bỏ mọi logic cứng liên quan đến `demo-user-001`, `demo@2024`.
   - Bỏ tiêm (DI) các interface in-memory. Thay vào đó, inject `ApplicationDbContext` (hoặc Repository/Service chuẩn của hệ thống).
   - Sửa các hàm Login/Register/GetUsers trong đó để đọc và ghi dữ liệu thẳng xuống cơ sở dữ liệu thật. (Giữ nguyên cấu trúc route để FE không bị gãy).

2. [ ] **Xóa Dead Code In-Memory:**
   - Xóa các file class Mock In-Memory Authentication nếu chúng không còn được dùng. Rà soát lại `Program.cs` / cấu hình DI để xóa đăng ký interface mock.

3. [ ] **Đồng bộ Frontend Stateless Auth (B5 - Frontend):**
   - Mở `frontend/src/features/auth/services/statelessAuthApi.ts`.
   - Tháo gỡ các dữ liệu gửi hardcode (demo-account) tương thích với logic giả cũ. Khớp nối lại với luồng xác thực thật vừa xây ở bước 1.

4. [ ] **Kiểm thử Hồi quy Toàn diện:**
   - Chạy `dotnet test`. Chắc chắn sẽ có test bị vỡ do xóa cấu trúc mock. **Hãy sửa toàn bộ unit test bị vỡ.**
   - Chạy `npm run type-check` (hoặc build-only) ở Frontend.

5. [ ] **Cập nhật File Tracking:**
   - Đánh dấu `[x]` cho **A1** và **B5** trong `plan/review/mock-remaining.md`.
   - Cập nhật `plan/tracking/progress.md`.
   - Ghi nhận thay đổi kiến trúc cực lớn này vào `plan/tracking/decisions.md` (ADR: Migrate StatelessControllers to Real Database).

6. [ ] **DỪNG LẠI VÀ BÁO CÁO:**
   - Task này ảnh hưởng sâu rộng. Ghi rõ các file Controller/Service đã bị sửa, cũng như test nào đã phải đập đi xây lại. Chờ tôi duyệt!
