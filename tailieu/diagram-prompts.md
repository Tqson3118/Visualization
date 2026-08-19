# DIAGRAM PROMPTS — 6 ẢNH NHÓM B (ChatGPT) + DANH SÁCH NHÓM A

> Mục đích: chuẩn bị ảnh sơ đồ cho báo cáo Word. Phân loại theo chất lượng Mermaid:
> - **NHÓM A — Mermaid làm TỐT → giữ Mermaid trong docs** (render thành ảnh bằng mmdc khi cần).
> - **NHÓM B — Mermaid làm KHÔNG tốt (ERD, Use Case) → sinh ảnh bằng ChatGPT** với prompt mô tả chi tiết.
> Mọi tên (actor/use case/bảng/cột/ID FR-UC) lấy NGUYÊN VĂN từ docs/SRS.md + docs/SDD.md — ảnh phải trùng docs 100%. Nguồn: `docs/DIAGRAM_PROMPTS.md`.

---

# NHÓM B — 6 ảnh bắt buộc (dán vào ChatGPT)

## Prompt 1 — Use case tổng thể → `01-usecase-tong-quan.png`

Vẽ sơ đồ Use case tổng thể cho báo cáo đồ án tốt nghiệp, theo đúng dữ liệu sau.

DỮ LIỆU (nguồn docs/SRS.md §5.1 — KHÔNG được đổi tên):
- Tác nhân: Người học (Student), Giảng viên (Teacher), Quản trị viên (Admin)
- Use cases (TOÀN BỘ 32 UC của hệ thống, gộp các nhóm):
  - UC-01 Chạy mô phỏng giải thuật
  - UC-02 Tạo tài khoản
  - UC-03 Đăng nhập và duy trì phiên
  - UC-04 Xem bài học
  - UC-05 Tìm kiếm bài học
  - UC-06 Làm bài tập trắc nghiệm
  - UC-07 Làm bài tập dự đoán bước (Bậc 2 Lab)
  - UC-08 Xem tiến độ cá nhân
  - UC-09 Biên soạn bài học
  - UC-10 Biên soạn bài tập
  - UC-11 Xem báo cáo giảng dạy
  - UC-12 Quản lý người dùng
  - UC-13 Quản trị cấu hình
  - UC-14 Xem demo công khai
  - UC-15 Khôi phục mật khẩu
  - UC-16 Xem chi tiết bài học và mở module riêng
  - UC-17 Viết và chạy code trong sandbox
  - UC-18 Nộp bài tập lập trình
  - UC-19 Xem lịch sử nộp bài code
  - UC-20 Quản lý lớp học phần
  - UC-21 Tham gia lớp bằng mã mời
  - UC-22 Ghi chú cá nhân
  - UC-23 Xem thành tích và huy hiệu
  - UC-24 Gửi phản hồi và báo lỗi
  - UC-25 Học theo Learning Path
  - UC-26 Làm Practice Ladder
  - UC-27 Làm bài kiểm tra cuối lộ trình
  - UC-29 Làm Daily Quest và giữ Streak
  - UC-30 Mua vật phẩm Gems Shop
  - UC-31 Xem Leaderboard
  - UC-32 Nâng cấp Premium
- Phân bổ tác nhân (theo SRS §5.1): Người học → UC-01, 04, 05, 06, 07, 08, 16, 17, 18, 19, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32; Giảng viên → UC-04, 05, 09, 10, 11, 20, 24; Quản trị viên → UC-12, 13, 24. Chú thích nhỏ: UC-02, 03, 14, 15 mở cho Khách chưa đăng nhập (theo SRS §5.1).

PHONG CÁCH VẼ (bắt buộc):
Tác nhân = hình NGƯỜI QUE (stick figure) xếp bên trái, nối đường liền nét tới use case hình ELIP đặt bên trong khung hệ thống (ghi tên "Hệ thống DSA-Visual"). Mỗi elip ghi tên use case (kèm mã UC-xx).

QUY TẮC:
- Giữ NGUYÊN mọi tên và mã ID (UC-xx) — không thêm/bớt.
- Tiếng Việt cho tên hiển thị.
- Chữ ≥ 10pt, nền trắng, tỷ lệ cân đối, không watermark.
- Nếu ảnh quá nhiều nội dung: tách thành 2 ảnh cùng style (báo lại để gộp).

---

## Prompt 2 — Use case người học → `02-usecase-hoc-vien.png`

Vẽ sơ đồ Use case người học cho báo cáo đồ án tốt nghiệp, theo đúng dữ liệu sau.

DỮ LIỆU (nguồn docs/SRS.md §5 — KHÔNG được đổi tên):
- Tác nhân: Người học (Student)
- Use cases:
  - UC-01 Chạy mô phỏng giải thuật
  - UC-02 Tạo tài khoản
  - UC-03 Đăng nhập và duy trì phiên
  - UC-04 Xem bài học
  - UC-05 Tìm kiếm bài học
  - UC-06 Làm bài tập trắc nghiệm
  - UC-07 Làm bài tập dự đoán bước (Bậc 2 Lab)
  - UC-08 Xem tiến độ cá nhân
  - UC-14 Xem demo công khai
  - UC-17 Viết và chạy code trong sandbox
  - UC-18 Nộp bài tập lập trình
  - UC-19 Xem lịch sử nộp bài code
  - UC-21 Tham gia lớp bằng mã mời
  - UC-22 Ghi chú cá nhân
  - UC-23 Xem thành tích và huy hiệu
  - UC-24 Gửi phản hồi và báo lỗi
  - UC-25 Học theo Learning Path
  - UC-26 Làm Practice Ladder
  - UC-27 Làm bài kiểm tra cuối lộ trình
  - UC-29 Làm Daily Quest và giữ Streak
  - UC-30 Mua vật phẩm Gems Shop
  - UC-31 Xem Leaderboard
  - UC-32 Nâng cấp Premium

PHONG CÁCH VẼ (bắt buộc):
Tác nhân = hình NGƯỜI QUE (stick figure) xếp bên trái, nối đường liền nét tới use case hình ELIP đặt bên trong khung hệ thống. Mỗi elip ghi tên use case (kèm mã UC-xx).

QUY TẮC:
- Giữ NGUYÊN mọi tên và mã ID (UC-xx) — không thêm/bớt.
- Tiếng Việt cho tên hiển thị.
- Chữ ≥ 10pt, nền trắng, tỷ lệ cân đối, không watermark.
- Nếu ảnh quá nhiều nội dung: tách thành 2 ảnh cùng style (báo lại để gộp).

---

## Prompt 3 — Use case giảng viên → `03-usecase-giang-vien.png`

Vẽ sơ đồ Use case giảng viên cho báo cáo đồ án tốt nghiệp, theo đúng dữ liệu sau.

DỮ LIỆU (nguồn docs/SRS.md §5 — KHÔNG được đổi tên):
- Tác nhân: Giảng viên (Teacher)
- Use cases:
  - UC-09 Biên soạn bài học
  - UC-10 Biên soạn bài tập
  - UC-11 Xem báo cáo giảng dạy
  - UC-20 Quản lý lớp học phần

PHONG CÁCH VẼ (bắt buộc):
Tác nhân = hình NGƯỜI QUE (stick figure) xếp bên trái, nối đường liền nét tới use case hình ELIP đặt bên trong khung hệ thống. Mỗi elip ghi tên use case (kèm mã UC-xx).

QUY TẮC:
- Giữ NGUYÊN mọi tên và mã ID (UC-xx) — không thêm/bớt.
- Tiếng Việt cho tên hiển thị.
- Chữ ≥ 10pt, nền trắng, tỷ lệ cân đối, không watermark.
- Nếu ảnh quá nhiều nội dung: tách thành 2 ảnh cùng style (báo lại để gộp).

---

## Prompt 4 — Use case quản trị viên → `04-usecase-admin.png`

Vẽ sơ đồ Use case quản trị viên cho báo cáo đồ án tốt nghiệp, theo đúng dữ liệu sau.

DỮ LIỆU (nguồn docs/SRS.md §5 — KHÔNG được đổi tên):
- Tác nhân: Quản trị viên (Admin)
- Use cases:
  - UC-12 Quản lý người dùng
  - UC-13 Quản trị cấu hình

PHONG CÁCH VẼ (bắt buộc):
Tác nhân = hình NGƯỜI QUE (stick figure) xếp bên trái, nối đường liền nét tới use case hình ELIP đặt bên trong khung hệ thống. Mỗi elip ghi tên use case (kèm mã UC-xx).

QUY TẮC:
- Giữ NGUYÊN mọi tên và mã ID (UC-xx) — không thêm/bớt.
- Tiếng Việt cho tên hiển thị.
- Chữ ≥ 10pt, nền trắng, tỷ lệ cân đối, không watermark.
- Nếu ảnh quá nhiều nội dung: tách thành 2 ảnh cùng style (báo lại để gộp).

---

## Prompt 5 — ERD tổng quan → `05-erd-tong-quan.png`

Vẽ sơ đồ ERD tổng quan cho báo cáo đồ án tốt nghiệp, theo đúng dữ liệu sau.

DỮ LIỆU (nguồn docs/SDD.md §2.1 + §7 — KHÔNG được đổi tên):
- 6 cụm nghiệp vụ và bảng đại diện (tên bảng giữ nguyên tiếng Anh):
  - **Auth**: Users (trung tâm, PK Id — mọi cụm tham chiếu qua UserId), RefreshTokens (UserId FK), PasswordResetTokens (UserId FK)
  - **Học tập** (lõi học tập — Module B/C/D/E): Topics, Lessons, LessonSimulations, LessonNotes, Exercises, Questions, ExerciseSubmissions, UserProgress, UserNodeProgress, Favorites, LearningPaths, LearningPathNodes, NodeSessions, Achievements, UserAchievements, ContentFeedback, BugReports
  - **Engine** (Simulation Engine EDV — chạy ở frontend, không có bảng riêng): dữ liệu cấu hình mô phỏng nằm tại LessonSimulations (SimulationKey); mô phỏng yêu thích của người dùng nằm tại Favorites (SimulationKey)
  - **Lớp học** (Module H): Classes, ClassMembers, ClassAssignments
  - **Gamification** (Module J — GamificationService): DailyQuests, UserQuests, ShopItems, UserInventory, GemTransactions, PremiumSubscriptions (các cột gamification Hearts/Gems/Xp/StreakDays/PremiumUntil nằm trên Users)
  - **Code Runner** (Module I): CodeRuns, CodeSubmissions
- Quan hệ giữa cụm (từ ERD docs/SDD.md §7.1–7.2):
  - Auth → Học tập: Users 1-n UserProgress, LessonNotes, Favorites, ExerciseSubmissions, UserAchievements, ContentFeedback, BugReports, NodeSessions, UserNodeProgress; Topics/Lessons/Exercises có CreatedBy FK → Users
  - Auth → Gamification: Users 1-n UserQuests, UserInventory, GemTransactions, PremiumSubscriptions
  - Auth → Code Runner: Users 1-n CodeRuns, CodeSubmissions
  - Auth → Lớp học: Users n-n Classes (quản lý qua OwnerId; tham gia qua ClassMembers)
  - Học tập → Engine: Lessons 1-n LessonSimulations (khóa SimulationKey); Users 1-n Favorites (SimulationKey)
  - Học tập → Lớp học: Classes 1-n ClassAssignments (LessonId/ExerciseId FK); ExerciseSubmissions.ClassAssignmentId FK → ClassAssignments
  - Học tập → Code Runner: Exercises 1-n CodeRuns/CodeSubmissions (ExerciseId FK)
  - Gamification nội bộ: DailyQuests 1-n UserQuests; ShopItems 1-n UserInventory

PHONG CÁCH VẼ (bắt buộc):
Mỗi cụm nghiệp vụ = 1 hình VUÔNG/HÌNH THOI lớn ghi tên cụm + vài bảng đại diện nhỏ bên trong; đường nối giữa cụm có nhãn quan hệ (1-n, n-n).

QUY TẮC:
- Giữ NGUYÊN mọi tên và mã ID (tên cụm, tên bảng, tên cột) — không thêm/bớt.
- Tiếng Việt cho tên hiển thị của cụm; tên bảng/cột kỹ thuật giữ tiếng Anh.
- Chữ ≥ 10pt, nền trắng, tỷ lệ cân đối, không watermark.
- Nếu ảnh quá nhiều nội dung: tách thành 2 ảnh cùng style (báo lại để gộp).

---

## Prompt 6 — ERD chi tiết → `06-erd-chi-tiet.png`

Vẽ sơ đồ ERD chi tiết (32 bảng) cho báo cáo đồ án tốt nghiệp, theo đúng dữ liệu sau.

DỮ LIỆU (nguồn docs/SDD.md §7 — KHÔNG được đổi tên; 32 bảng: 24 lõi học tập + 8 gamification/code):
- **Users** PK Id | UK Email | cột: PasswordHash, DisplayName, Role, IsActive, IsPrimaryAdmin, TwoFactorEnabled, AvatarUrl, Hearts, HeartsMax, LastHeartAt, Gems, Xp, StreakDays, StreakFreeze, PremiumUntil, LastActivityDate, CreatedAt, UpdatedAt, DeletedAt
- **RefreshTokens** PK Id | FK UserId → Users | UK TokenHash | cột: ExpiresAt, RevokedAt, CreatedByIp, CreatedAt
- **PasswordResetTokens** PK Id | FK UserId → Users | UK TokenHash | cột: ExpiresAt, Used, CreatedAt
- **Topics** PK Id | FK ParentId → Topics.Id (tự tham chiếu), CreatedBy → Users | cột: Name, Description, SortOrder, CreatedAt, UpdatedAt, DeletedAt
- **Lessons** PK Id | FK TopicId → Topics, CreatedBy/UpdatedBy → Users | cột: Title, Description, ContentHtml, SortOrder, Status, CreatedAt, UpdatedAt, DeletedAt
- **LessonSimulations** PK Id | FK LessonId → Lessons | cột: SimulationKey, Title, DefaultInputJson, SortOrder (UK LessonId+SimulationKey)
- **LessonNotes** PK Id | FK UserId → Users, LessonId → Lessons | cột: ContentHtml, UpdatedAt (UK UserId+LessonId)
- **Exercises** PK Id | FK LessonId → Lessons, NodeId → LearningPathNodes (tùy chọn), CreatedBy → Users | cột: Stage, ConfigJson, Title, Description, Type, DurationMinutes, MaxScore, Status, DeletedAt
- **Questions** PK Id | FK ExerciseId → Exercises | cột: Content, OptionsJson, AnswerJson, Explanation, Hint1, Hint2, Hint3, WrongExplanationsJson, KeepOrder, Points, SortOrder
- **ExerciseSubmissions** PK Id | FK UserId → Users, ExerciseId → Exercises, ClassAssignmentId → ClassAssignments (tùy chọn) | cột: Score, AnswersJson, ResultJson, SubmittedAt, DurationSeconds
- **UserProgress** PK Id | FK UserId → Users, LessonId → Lessons | cột: Viewed, SimulationCount, BestScore, CompletedAt, UpdatedAt (UK UserId+LessonId)
- **UserNodeProgress** PK Id | FK UserId → Users, NodeId → LearningPathNodes | cột: Status, Stars, NodeScore, UnlockedAt, PassedAt, UpdatedAt
- **Favorites** PK Id | FK UserId → Users | cột: SimulationKey, InputJson, CreatedAt (UK UserId+SimulationKey)
- **Settings** PK Id | UK Key | FK UpdatedBy → Users | cột: Value, Description, UpdatedAt
- **Classes** PK Id | UK InviteCode | FK OwnerId → Users | cột: Name, Semester, Description, Status, CreatedAt, DeletedAt
- **ClassMembers** PK Id | FK ClassId → Classes, UserId → Users | cột: JoinedAt (UK ClassId+UserId)
- **ClassAssignments** PK Id | FK ClassId → Classes, LessonId → Lessons (tùy chọn), ExerciseId → Exercises (tùy chọn) | cột: DueAt, CreatedAt (CHECK: LessonId hoặc ExerciseId có giá trị)
- **Achievements** PK Id | UK Code | cột: Name, Description, IconUrl, ConditionJson, SortOrder
- **UserAchievements** PK Id | FK UserId → Users, AchievementId → Achievements | cột: EarnedAt
- **ContentFeedback** PK Id | FK UserId → Users, LessonId → Lessons | cột: Rating, Comment, CreatedAt, UpdatedAt
- **BugReports** PK Id | FK UserId → Users (tùy chọn), AssigneeId → Users (tùy chọn) | cột: Description, ContextJson, Status, CreatedAt, ResolvedAt
- **LearningPaths** PK Id | FK TopicId → Topics (tùy chọn), CreatedBy → Users | cột: Title, Description, SortOrder, IsActive
- **LearningPathNodes** PK Id | FK PathId → LearningPaths, LessonId → Lessons (tùy chọn), FinalTestId → Exercises (tùy chọn) | cột: Title, SortOrder
- **NodeSessions** PK Id | FK UserId → Users, NodeId → LearningPathNodes | cột: StartedAt, ExpiresAt, Stage, StepIndex
- **DailyQuests** PK Id | UK QuestKey | cột: Title, Type, ConditionJson, RewardJson, PoolEnabled
- **UserQuests** PK Id | FK UserId → Users, QuestId → DailyQuests | cột: QuestDate, Progress, Claimed
- **ShopItems** PK Id | UK ItemKey | cột: Name, PriceGems, MaxStack, Type, DurationHours
- **UserInventory** PK Id | FK UserId → Users, ItemId → ShopItems | cột: Quantity, PurchasedAt, ExpiresAt
- **GemTransactions** PK Id | FK UserId → Users | cột: Type, Amount, RefType, RefId, CreatedAt
- **PremiumSubscriptions** PK Id | FK UserId → Users | cột: PlanId, StartedAt, ExpiresAt, Status, OrderRef, CreatedAt
- **CodeRuns** PK Id | FK UserId → Users, ExerciseId → Exercises (tùy chọn) | cột: Code, InputJson, Status, OutputJson, ErrorJson, TraceJson, DurationMs, CreatedAt
- **CodeSubmissions** PK Id | FK UserId → Users, ExerciseId → Exercises | cột: Code, Score, PassedTests, TotalTests, ResultJson, SubmittedAt
- Quan hệ khóa ngoại chính giữa bảng (từ ERD docs/SDD.md §7.1–7.2): Users 1-n RefreshTokens, PasswordResetTokens, UserProgress, Favorites, ExerciseSubmissions, LessonNotes, UserAchievements, ContentFeedback, BugReports, NodeSessions, UserNodeProgress, UserQuests, UserInventory, GemTransactions, PremiumSubscriptions, CodeRuns, CodeSubmissions; Users n-n Classes (qua ClassMembers, OwnerId); Topics 1-n Topics (parent) và Lessons; Lessons 1-n LessonSimulations, Exercises, UserProgress, LessonNotes, ContentFeedback; Exercises 1-n Questions, ExerciseSubmissions, CodeRuns, CodeSubmissions; Classes 1-n ClassMembers, ClassAssignments; Achievements 1-n UserAchievements; LearningPaths 1-n LearningPathNodes (TopicId tùy chọn); LearningPathNodes 1-n NodeSessions, UserNodeProgress và n-1 Lessons/Exercises (FinalTestId); DailyQuests 1-n UserQuests; ShopItems 1-n UserInventory.

PHONG CÁCH VẼ (bắt buộc):
Mỗi bảng = khung bảng (tiêu đề bảng, dưới là danh sách cột, cột khóa chính đánh dấu PK, khóa ngoại đánh dấu FK); đường nối khóa ngoại giữa các bảng.

QUY TẮC:
- Giữ NGUYÊN mọi tên và mã ID (tên bảng, tên cột, PK/FK) — không thêm/bớt.
- Tiếng Việt cho tên hiển thị; tên bảng/cột kỹ thuật giữ tiếng Anh.
- Chữ ≥ 10pt, nền trắng, tỷ lệ cân đối, không watermark.
- Nếu ảnh quá nhiều nội dung: tách thành 2 ảnh cùng style (báo lại để gộp).

---

# NHÓM A — giữ Mermaid (không gen ảnh ChatGPT)

| # | Sơ đồ | Nguồn Mermaid | Ghi chú |
|---|---|---|---|
| 1 | Sequence UC-01 chạy mô phỏng | SRS §5.2 (5.2) | Mermaid sequence đẹp, giữ |
| 2 | Sequence trừ tim atomic (UC-25 / FR-10.1) | SDD §5 | giữ |
| 3 | Sequence nộp bài + chấm điểm (UC-06) | SRS §5.7 | giữ |
| 4 | Sitemap / sơ đồ luồng màn hình | SDD §8 (20.2.1) | giữ |
| 5 | Activity / state machine mô phỏng | SDD §3 (12.8) | giữ |
| 6 | Class Simulation Engine EDV | SDD §4 | giữ |
| 7 | Class kiến trúc backend | SDD §5 | giữ |
| 8 | Deployment | DEPLOY §1 | giữ |

> Ghi chú: muốn thành ảnh → chạy `npx -y @mermaid-js/mermaid-cli` (mmdc) render từng sơ đồ nhóm A ra `tailieu/diagrams/`. Nếu mmdc lỗi (thiếu Chrome) → để nguyên code block, không chặn tiến độ.

---

# Quy trình sử dụng

1. Dán từng prompt NHÓM B ở trên vào ChatGPT (6 lần, mỗi prompt 1 ảnh) → tải ảnh về với đúng tên: `01-usecase-tong-quan.png`, `02-usecase-hoc-vien.png`, `03-usecase-giang-vien.png`, `04-usecase-admin.png`, `05-erd-tong-quan.png`, `06-erd-chi-tiet.png`.
2. Đặt ảnh vào `tailieu/diagrams/` (thư mục ảnh sơ đồ) HOẶC ghi đè các file cùng tên trong `tailieu/placeholders/` nếu báo cáo đang dùng ảnh giữ chỗ.
3. Chạy lại pandoc build docx để gắn ảnh NHÓM B vào báo cáo.
4. (Tùy chọn) Render NHÓM A bằng mmdc như mục NHÓM A ở trên; lỗi thì giữ code block Mermaid.
