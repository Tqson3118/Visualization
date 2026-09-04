# 04. Database: Entity → Configuration → Migration → SQL Server

## Bốn lớp
1. **Entity**: class C# mô tả dữ liệu và quan hệ.
2. **Configuration**: EF Core mapping tên bảng, column, type, nullable, index, FK, delete behavior.
3. **Migration**: lịch sử thay đổi schema do EF Core sinh ra.
4. **SQL Server**: database thực thi table, constraint, index và transaction.

## Flow khi thêm một column
Sửa Entity → sửa Configuration nếu cần → tạo Migration → review migration → apply database → kiểm tra SQL Server.

## Code cần tra
- `backend/src/DsaVisual.Application/Persistence/Entities/`
- `backend/src/DsaVisual.Application/Persistence/Configurations/`
- `backend/src/DsaVisual.Application/Persistence/AppDbContext.cs`
- `backend/src/DsaVisual.Application/Persistence/Migrations/`

## Câu hỏi bảo vệ
- Vì sao không trả Entity trực tiếp? Để tách schema nội bộ khỏi API và tránh lộ dữ liệu.
- Vì sao cần Configuration? Entity không thể hiện đầy đủ quy tắc database.
- Migration có phải database không? Không; migration chỉ là script/lịch sử để thay đổi database.

## Checklist phải học thuộc
Một column cần biết: kiểu dữ liệu, nullable, default, length, index, ai ghi, ai đọc, có chứa dữ liệu nhạy cảm không.

## Cách tra code
Bắt đầu từ property entity; tìm Configuration; tìm DbSet trong AppDbContext; tìm migration; tìm service query/insert; cuối cùng xem endpoint DTO.

## Câu hỏi khó
Nếu đổi string length thì dữ liệu cũ ra sao? Nếu migration chạy một phần thì rollback thế nào? Index nào phục vụ query best score hoặc pagination?

## Giải thích từng bảng chính
- **User**: tài khoản, role/status và hồ sơ; nhiều bảng token, progress, submission tham chiếu User.
- **LearningPath**: lộ trình học; **LearningPathNode**: từng nút lesson/exercise và quan hệ parent/điều kiện unlock.
- **Lesson**: nội dung bài học, title, publish status, metadata.
- **LessonSimulation**: liên kết lesson với visual simulation.
- **Exercise**: đề bài và config CodeLab/quiz; judge đọc config này.
- **Question**: câu hỏi, lựa chọn, đáp án và giải thích.
- **CodeSubmission**: source code và kết quả nộp chính thức.
- **CodeRun**: lịch sử chạy/benchmark, cần phân biệt với submission.
- **ExerciseSubmission**: kết quả exercise trong tiến trình/lớp học.
- **UserProgress/UserNodeProgress**: trạng thái viewed/completed, score và thời điểm của user.
- **Class/ClassMember/ClassAssignment**: lớp, thành viên và bài được giao/deadline.
- **ContentFeedback/CourseFeedback**: phản hồi, trả lời, status và actor xử lý.
- **DailyQuest/UserQuest**: định nghĩa nhiệm vụ và tiến độ từng user.
- **Achievement/UserAchievement**: định nghĩa huy hiệu và user đã đạt.
- **RefreshToken/OtpCode/RegisterOtpCode/PasswordResetToken**: token tạm, expiry và revoke.
- **PremiumSubscription**: gói premium và thời hạn. **ShopItem/UserInventory/GemTransaction**: shop, vật phẩm và lịch sử gem.
- **Favorite/LessonNote/BugReport**: yêu thích, ghi chú và báo lỗi của user.

Cách học mỗi bảng: mở Entity → Configuration → migration → service/API sử dụng; ghi mục đích, column, FK, ai ghi và ai đọc.
