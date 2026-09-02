# ⚙️ TÀI LIỆU SERVICES — BUSINESS LOGIC & DATA ACCESS LAYER

Tầng Services trong `backend/src/DsaVisual.Application/Services/` là nơi chứa **toàn bộ logic nghiệp vụ (Business Rules)**, các phép tính toán, trừ Tim, cộng Ngọc, mở khóa bài học và thao tác trực tiếp với cơ sở dữ liệu qua EF Core `AppDbContext`.

---

## 🏛️ ĐẶC ĐIỂM KIẾN TRÚC SERVICES

1. **Dependency Injection**: Mỗi Service đều có Interface tương ứng (`ILessonService`, `IAuthService`, `IGamificationService`...) được đăng ký dạng `Scoped` trong `Program.cs`.
2. **EF Core Direct Query**:
   ```csharp
   // Truy vấn đọc tối ưu với AsNoTracking()
   var lesson = await _dbContext.Lessons
       .AsNoTracking()
       .Include(l => l.LearningPathNode)
       .FirstOrDefaultAsync(l => l.Id == lessonId, cancellationToken);
   ```
3. **Quản lý Transaction an toàn**: Các thao tác nhạy cảm (như Mua vật phẩm trừ Ngọc, Ghi danh trừ Tim) đều được bọc trong giao dịch `using var transaction = await _dbContext.Database.BeginTransactionAsync()`.

---

## 📋 CHI TIẾT 16 BUSINESS SERVICES

### 1. [`AuthService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/AuthService.cs)
* **Trách nhiệm**: Xử lý toàn bộ vòng đời xác thực của tài khoản.
* **Các phương thức chính**:
  * `LoginAsync(usernameOrEmail, password)`: Kiểm tra tài khoản, xác thực mật khẩu BCrypt, kiểm tra số lần đăng nhập sai (`LoginAttemptTracker`), sinh Access & Refresh Token.
  * `RequestRegisterOtpAsync(email, displayName, role)`: Kiểm tra email trùng, sinh OTP 6 số, lưu vào `RegisterOtpCodes`, gọi `EmailTemplateService` gửi qua MailHog.
  * `VerifyRegisterOtpAndCreateUserAsync(...)`: Xác thực OTP và tạo bản ghi User mới trong CSDL.
  * `RefreshTokenAsync(refreshToken)`: Kiểm tra hạn và tính hợp lệ của Refresh Token $\rightarrow$ Thu hồi token cũ và phát hành cặp token mới (Token Rotation).
  * `ResetPasswordAsync(email, otp, newPassword)`: Xác thực OTP và đặt lại mật khẩu mới.

### 2. [`LessonService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/LessonService.cs)
* **Trách nhiệm**: Quản lý CRUD bài học, lưu nội dung Markdown, cấu hình mô phỏng nhúng và đánh dấu hoàn thành bài.
* **Logic Hoàn thành bài học (`CompleteLessonAsync`)**:
  1. Kiểm tra bài học tồn tại và quyền của học viên.
  2. Cập nhật bản ghi `UserNodeProgress`: `Status = Completed`, `CompletedAt = DateTime.UtcNow`.
  3. Tìm kiếm Node bài học kế tiếp (Next Lesson Node) $\rightarrow$ Cập nhật `IsLocked = false`.
  4. Kích hoạt [`QuestProgressWriter.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/QuestProgressWriter.cs) ghi nhận hoạt động học tập (`ActivityType.CompleteLesson`) $\rightarrow$ Tự động tăng tiến độ Daily Quest và cộng điểm kinh nghiệm (+20 XP).

### 3. [`GamificationService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/GamificationService.cs)
* **Trách nhiệm**: Điều phối toàn bộ nền kinh tế ảo và động lực học tập.
* **Các logic nghiệp vụ quan trọng**:
  * `DeductHeartAsync(userId)`: Trừ 1 Tim khi học viên làm sai câu hỏi hoặc mở khóa lộ trình mới.
  * `RegenerateHeartsAsync(userId)`: Tính toán thời gian trôi qua từ lần cuối mất tim $\rightarrow$ Tự động hồi phục Tim nếu đủ thời gian quy định (ví dụ: mỗi 30 phút hồi 1 Tim cho tài khoản Free).
  * `BuyShopItemAsync(userId, itemId)`: Kiểm tra số dư Ngọc $\rightarrow$ Trừ Ngọc $\rightarrow$ Thêm vào `UserInventory` $\rightarrow$ Ghi log vào `GemTransactions`.
  * `EquipItemAsync(userId, inventoryItemId)`: Gỡ trang bị cũ và kích hoạt Khung viền/Avatar mới.
  * `GetLeaderboardAsync(type, classId)`: Tính toán bảng xếp hạng theo tuần hoặc theo lớp học.

### 4. [`QuestProgressWriter.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/QuestProgressWriter.cs)
* **Trách nhiệm**: Ghi nhận hoạt động và tự động tăng tiến độ nhiệm vụ hàng ngày (Event-driven Progress Recorder).
* Khi người dùng hoàn thành 1 bài học, chạy 1 mô phỏng, hoặc giải 1 bài tập $\rightarrow$ Service này tự động quét bảng `DailyQuests` của ngày hôm nay và tăng trường `Current += 1`. Khi `Current >= Target`, mở khóa nút nhận thưởng.

### 5. [`ClassService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/ClassService.cs)
* **Trách nhiệm**: Quản lý các lớp học, sĩ số, bài tập giao và tổng hợp báo cáo kết quả.
* **Các phương thức chính**:
  * `CreateClassAsync(ownerId, dto)`: Tạo lớp học, tự động sinh mã `InviteCode` duy nhất không trùng lặp.
  * `JoinClassByCodeAsync(userId, inviteCode)`: Sinh viên tham gia lớp học qua mã Code.
  * `AssignExerciseAsync(classId, exerciseId, deadline)`: Giao bài tập cho toàn bộ học sinh trong lớp.
  * `GetClassReportAsync(classId)`: Tổng hợp số lượng bài nộp đúng hạn, trễ hạn, tính % hoàn thành và liệt kê danh sách học sinh có nguy cơ trễ hạn (`LaggingLearners`).

### 6. [`ExerciseService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/ExerciseService.cs) & [`CodelabJudgeService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/CodelabJudgeService.cs)
* **Trách nhiệm**:
  * `ExerciseService`: Tiếp nhận bài thi trắc nghiệm $\rightarrow$ Đối chiếu đáp án đúng trong bảng `Questions` $\rightarrow$ Chấm điểm và kích hoạt thưởng phạt Tim/XP.
  * `CodelabJudgeService`: Chạy mã nguồn người dùng gửi lên trong môi trường cách ly (Sandbox Runner), nạp từng Test Case ẩn, so sánh `stdout` thực tế với `expectedOutput` $\rightarrow$ Trả về kết quả `Accepted`, `WrongAnswer` hoặc `TimeLimitExceeded`.

### 7. [`PathItemService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/PathItemService.cs) & [`TopicService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/TopicService.cs)
* **Trách nhiệm**: Quản trị cây phân cấp giáo trình (Topic $\rightarrow$ LearningPath $\rightarrow$ LearningPathNode).
* Hỗ trợ chức năng kéo thả sắp xếp lại thứ tự bài học trong Studio (`MovePathItemAsync`).

### 8. [`UserService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/UserService.cs)
* **Trách nhiệm**: Phục vụ các tác vụ quản trị người dùng của Admin:
  * `ApproveTeacherAsync(userId)`: Phê duyệt hồ sơ Giảng viên (`TEACHER_PENDING` $\rightarrow$ `TEACHER`).
  * `AdminResetPasswordAsync(userId, newPassword)`: Đổi mật khẩu trực tiếp cho tài khoản.
  * `ToggleLockUserAsync(userId)`: Khóa hoặc mở khóa tài khoản người dùng vi phạm.

### 9. Các Services Hạ tầng & Tiện ích:
* [`TokenService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/TokenService.cs): Sinh chuỗi JWT Claims, ký số bằng Symmetric Key và sinh chuỗi Refresh Token ngẫu nhiên.
* [`EmailTemplateService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/EmailTemplateService.cs) & [`SmtpClientFactory.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/SmtpClientFactory.cs): Soạn thảo template HTML email và gửi qua MailHog SMTP.
* [`SettingService.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/SettingService.cs) & [`SettingsCache.cs`](file:///d:/FPT/metqua/backend/src/DsaVisual.Application/Services/SettingsCache.cs): Quản lý cấu hình nền tảng lưu trong Database kèm bộ nhớ đệm Memory Cache.
