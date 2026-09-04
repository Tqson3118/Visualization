#:package Microsoft.Data.SqlClient@6.0.2
#:property PublishAot=false
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

var cs = "Server=db65198.public.databaseasp.net; Database=db65198; User Id=db65198; Password=4Rn+#6EoB!a8; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True; Connect Timeout=60;";
using var conn = new SqlConnection(cs);
conn.Open();

Console.WriteLine("==================================================================");
Console.WriteLine("       BẮT ĐẦU QUY TRÌNH DỌN DẸP RÁC VÀ SEEDER DEMO CHUẨN CHỈ       ");
Console.WriteLine("==================================================================");

using var tx = conn.BeginTransaction();

try
{
    var now = DateTime.UtcNow;

    // ──────────────────────────────────────────────────────────────────
    // PHẦN 1: DỌN DẸP SẠCH SẼ DỮ LIỆU RÁC & CŨ
    // ──────────────────────────────────────────────────────────────────
    Console.WriteLine("\n--- [1/2] ĐANG THỰC HIỆN DỌN DẸP RÁC ---");

    var junkPathIds = "(23, 32, 36, 37, 38, 41, 45, 47, 49, 56, 58, 59, 66)";

    // 1.1 Tokens hết hạn
    using (var cmd = new SqlCommand("DELETE FROM [RefreshTokens] WHERE RevokedAt IS NOT NULL OR ExpiresAt < SYSUTCDATETIME()", conn, tx))
        Console.WriteLine("1.1 RefreshTokens hết hạn đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [OtpCodes] WHERE Used = 1 OR ExpiresAt < SYSUTCDATETIME()", conn, tx))
        Console.WriteLine("1.2 OtpCodes đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [RegisterOtpCodes] WHERE Used = 1 OR ExpiresAt < SYSUTCDATETIME()", conn, tx))
        Console.WriteLine("1.3 RegisterOtpCodes đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [PasswordResetTokens] WHERE Used = 1 OR ExpiresAt < SYSUTCDATETIME()", conn, tx))
        Console.WriteLine("1.4 PasswordResetTokens đã xóa: " + cmd.ExecuteNonQuery());

    // 1.2 Sessions hết hạn
    using (var cmd = new SqlCommand("DELETE FROM [NodeSessions] WHERE ExpiresAt < SYSUTCDATETIME() OR NodeId IN (SELECT Id FROM [LearningPathNodes] WHERE DeletedAt IS NOT NULL OR PathId IN " + junkPathIds + ")", conn, tx))
        Console.WriteLine("1.5 NodeSessions hết hạn đã xóa: " + cmd.ExecuteNonQuery());

    // 1.3 Shop items rác (11, 13, 15, 16)
    using (var cmd = new SqlCommand("DELETE FROM [UserInventory] WHERE ItemId IN (11, 13, 15, 16)", conn, tx))
        Console.WriteLine("1.6 UserInventory (shop rác) đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [GemTransactions] WHERE RefType = 'shop' AND RefId IN ('11', '13', '15', '16')", conn, tx))
        Console.WriteLine("1.7 GemTransactions (shop rác) đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [ShopItems] WHERE Id IN (11, 13, 15, 16)", conn, tx))
        Console.WriteLine("1.8 ShopItems rác đã xóa: " + cmd.ExecuteNonQuery());

    // 1.4 BugReports & Feedbacks test QA
    using (var cmd = new SqlCommand("DELETE FROM [BugReports] WHERE Id IN (11, 12, 13, 14, 15, 17)", conn, tx))
        Console.WriteLine("1.9 BugReports test QA đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [CourseFeedback] WHERE Id IN (5, 6, 7) OR CourseId IN " + junkPathIds, conn, tx))
        Console.WriteLine("1.10 CourseFeedback spam đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [ContentFeedback] WHERE Comment = '' OR Comment IS NULL", conn, tx))
        Console.WriteLine("1.11 ContentFeedback rỗng đã xóa: " + cmd.ExecuteNonQuery());

    // 1.5 UserQuests cũ
    using (var cmd = new SqlCommand("DELETE FROM [UserQuests] WHERE QuestDate < CAST(SYSUTCDATETIME() AS DATE)", conn, tx))
        Console.WriteLine("1.12 UserQuests ngày cũ đã xóa: " + cmd.ExecuteNonQuery());

    // 1.6 Toàn bộ 15 Classes cũ & bài nộp liên quan
    using (var cmd = new SqlCommand("DELETE FROM [ExerciseSubmissions] WHERE ClassAssignmentId IS NOT NULL", conn, tx))
        Console.WriteLine("1.13 ExerciseSubmissions lớp học cũ đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [ClassAssignments]", conn, tx))
        Console.WriteLine("1.14 ClassAssignments cũ đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [ClassMembers]", conn, tx))
        Console.WriteLine("1.15 ClassMembers cũ đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [Classes]", conn, tx))
        Console.WriteLine("1.16 Classes cũ đã xóa: " + cmd.ExecuteNonQuery());

    // 1.7 Lộ trình rác & Node đã xóa mềm
    using (var cmd = new SqlCommand(@"
        DROP TABLE IF EXISTS #ValidNodes;
        DROP TABLE IF EXISTS #ValidExercises;
        DROP TABLE IF EXISTS #ValidLessons;

        SELECT Id, LessonId, FinalTestId, LabExerciseId 
        INTO #ValidNodes
        FROM [LearningPathNodes] 
        WHERE DeletedAt IS NULL AND PathId NOT IN " + junkPathIds + @";

        SELECT Id INTO #ValidExercises
        FROM [Exercises] 
        WHERE Id IN (SELECT FinalTestId FROM #ValidNodes WHERE FinalTestId IS NOT NULL)
           OR Id IN (SELECT LabExerciseId FROM #ValidNodes WHERE LabExerciseId IS NOT NULL);

        SELECT LessonId INTO #ValidLessons
        FROM #ValidNodes WHERE LessonId IS NOT NULL
        UNION
        SELECT LessonId FROM [Exercises] WHERE Id IN (SELECT Id FROM #ValidExercises) AND LessonId IS NOT NULL;
    ", conn, tx))
        cmd.ExecuteNonQuery();

    using (var cmd = new SqlCommand("UPDATE [Exercises] SET NodeId = NULL WHERE NodeId IN (SELECT Id FROM [LearningPathNodes] WHERE DeletedAt IS NOT NULL OR PathId IN " + junkPathIds + ")", conn, tx))
        cmd.ExecuteNonQuery();

    using (var cmd = new SqlCommand("UPDATE [LearningPathNodes] SET ParentId = NULL, FinalTestId = NULL, LabExerciseId = NULL, LessonId = NULL WHERE DeletedAt IS NOT NULL OR PathId IN " + junkPathIds, conn, tx))
        cmd.ExecuteNonQuery();

    using (var cmd = new SqlCommand("DELETE FROM [UserNodeProgress] WHERE NodeId IN (SELECT Id FROM [LearningPathNodes] WHERE DeletedAt IS NOT NULL OR PathId IN " + junkPathIds + ")", conn, tx))
        Console.WriteLine("1.17 UserNodeProgress rác đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [LearningPathNodes] WHERE DeletedAt IS NOT NULL OR PathId IN " + junkPathIds, conn, tx))
        Console.WriteLine("1.18 LearningPathNodes rác đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [LearningPaths] WHERE Id IN " + junkPathIds, conn, tx))
        Console.WriteLine("1.19 LearningPaths rác đã xóa: " + cmd.ExecuteNonQuery());

    // 1.8 Xóa Bài tập và Bài học mồ côi
    using (var cmd = new SqlCommand("DELETE FROM [Questions] WHERE ExerciseId NOT IN (SELECT Id FROM #ValidExercises) AND ExerciseId IN (SELECT Id FROM [Exercises] WHERE LessonId NOT IN (SELECT LessonId FROM #ValidLessons))", conn, tx))
        Console.WriteLine("1.20 Questions mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [ExerciseSubmissions] WHERE ExerciseId NOT IN (SELECT Id FROM #ValidExercises) AND ExerciseId IN (SELECT Id FROM [Exercises] WHERE LessonId NOT IN (SELECT LessonId FROM #ValidLessons))", conn, tx))
        Console.WriteLine("1.21 ExerciseSubmissions mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [CodeRuns] WHERE ExerciseId NOT IN (SELECT Id FROM #ValidExercises) AND ExerciseId IN (SELECT Id FROM [Exercises] WHERE LessonId NOT IN (SELECT LessonId FROM #ValidLessons))", conn, tx))
        Console.WriteLine("1.22 CodeRuns mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [CodeSubmissions] WHERE ExerciseId NOT IN (SELECT Id FROM #ValidExercises) AND ExerciseId IN (SELECT Id FROM [Exercises] WHERE LessonId NOT IN (SELECT LessonId FROM #ValidLessons))", conn, tx))
        Console.WriteLine("1.23 CodeSubmissions mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [Exercises] WHERE Id NOT IN (SELECT Id FROM #ValidExercises) AND LessonId NOT IN (SELECT LessonId FROM #ValidLessons)", conn, tx))
        Console.WriteLine("1.24 Exercises mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [LessonSimulations] WHERE LessonId NOT IN (SELECT LessonId FROM #ValidLessons)", conn, tx))
        Console.WriteLine("1.25 LessonSimulations mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [UserProgress] WHERE LessonId NOT IN (SELECT LessonId FROM #ValidLessons)", conn, tx))
        Console.WriteLine("1.26 UserProgress mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [ContentFeedback] WHERE LessonId NOT IN (SELECT LessonId FROM #ValidLessons)", conn, tx))
        Console.WriteLine("1.27 ContentFeedback mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [LessonNotes] WHERE LessonId NOT IN (SELECT LessonId FROM #ValidLessons)", conn, tx))
        Console.WriteLine("1.28 LessonNotes mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [Lessons] WHERE Id NOT IN (SELECT LessonId FROM #ValidLessons)", conn, tx))
        Console.WriteLine("1.29 Lessons mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("UPDATE [Topics] SET ParentId = NULL WHERE DeletedAt IS NOT NULL OR Id = 35", conn, tx))
        cmd.ExecuteNonQuery();

    using (var cmd = new SqlCommand("DELETE FROM [Topics] WHERE DeletedAt IS NOT NULL OR Id = 35", conn, tx))
        Console.WriteLine("1.30 Topics rác đã xóa: " + cmd.ExecuteNonQuery());

    // ──────────────────────────────────────────────────────────────────
    // PHẦN 2: SEEDER DỮ LIỆU CHUẨN CHỈ
    // ──────────────────────────────────────────────────────────────────
    Console.WriteLine("\n--- [2/2] ĐANG THỰC HIỆN SEEDER CHUẨN HÓA ---");

    // 2.1 Chuẩn hóa 12 Lộ trình (gán teacherId = 2, chuẩn hóa tên tiếng Việt kèm phụ đề tiếng Anh)
    var pathTitles = new Dictionary<int, (string Title, int Status, int Visibility, int IsActive)>
    {
        { 1, ("Lộ trình Thuật toán Sắp xếp & Tìm kiếm Cơ bản (Sorting & Searching)", 2, 2, 1) },
        { 2, ("Lộ trình Cấu trúc Dữ liệu Tuyến tính (Stack & Queue)", 2, 2, 1) },
        { 3, ("Lộ trình Cây & Cấu trúc Dữ liệu Phân cấp (Tree & Hierarchy)", 2, 2, 1) },
        { 4, ("Lộ trình Bảng băm & Tra cứu Tốc độ cao (Hash Table & Hashing)", 2, 2, 1) },
        { 5, ("Lộ trình Thuật toán Đồ thị Cơ bản (Graph Traversal)", 2, 2, 1) },
        { 6, ("Cấu trúc Dữ liệu Cốt lõi & Trực quan (Grokking Data Structures)", 2, 2, 1) },
        { 7, ("Thuật toán Toàn thư & Tối ưu hóa (Grokking Algorithms)", 2, 2, 1) },
        { 20, ("Cấu trúc Dữ liệu Tuyến tính & Ứng dụng Thực tế (Linear Data Structures in Practice)", 2, 2, 1) },
        { 21, ("Cây Nhị phân & Cây Tìm kiếm Cân bằng (BST / AVL)", 2, 2, 1) },
        { 22, ("Thiết kế Thuật toán & Tối ưu hóa Nâng cao (Dynamic Programming)", 0, 0, 0) }, // Draft cho Studio
        { 33, ("Lộ trình Quy hoạch Động Thực chiến 2026 (Dynamic Programming 2026)", 2, 2, 1) },
        { 34, ("Lộ trình Cây Nhị Phân Tìm Kiếm Chuẩn 2026 (Binary Search Tree)", 2, 2, 1) }
    };

    foreach (var kvp in pathTitles)
    {
        using var cmd = new SqlCommand(@"
            UPDATE [LearningPaths] 
            SET Title = @title, Status = @status, Visibility = @vis, IsActive = @active, 
                AuthorId = 2, CreatedBy = 2
            WHERE Id = @id", conn, tx);
        cmd.Parameters.AddWithValue("@title", kvp.Value.Title);
        cmd.Parameters.AddWithValue("@status", kvp.Value.Status);
        cmd.Parameters.AddWithValue("@vis", kvp.Value.Visibility);
        cmd.Parameters.AddWithValue("@active", kvp.Value.IsActive);
        cmd.Parameters.AddWithValue("@id", kvp.Key);
        cmd.ExecuteNonQuery();
    }
    Console.WriteLine("2.1 Đã cập nhật chuẩn hóa 12 lộ trình chính thức");

    // Tạo 1 lộ trình Chờ duyệt (PendingReview) để Admin demo
    int pendingCourseId;
    using (var cmd = new SqlCommand(@"
        SELECT Id FROM [LearningPaths] WHERE Title LIKE N'%Thuật toán Đồ thị Chuyên sâu%'", conn, tx))
    {
        var existing = cmd.ExecuteScalar();
        if (existing == null)
        {
            using var insertCmd = new SqlCommand(@"
                INSERT INTO [LearningPaths] (Title, Description, TopicId, SortOrder, IsActive, CreatedBy, AuthorId, Status, Visibility)
                OUTPUT INSERTED.Id
                VALUES (N'Thuật toán Đồ thị Chuyên sâu: Cây khung & Luồng cực đại (Advanced Graph Algorithms)',
                        N'Chuyên đề đồ thị nâng cao: Thuật toán Kruskal, Prim tìm cây khung nhỏ nhất (MST) và thuật toán Edmonds-Karp tìm luồng cực đại.',
                        5, 12, 1, 2, 2, 1, 2)", conn, tx);
            pendingCourseId = (int)insertCmd.ExecuteScalar();
            Console.WriteLine("2.2 Đã tạo lộ trình Chờ duyệt (PendingReview) cho Admin, Id = " + pendingCourseId);
        }
        else
        {
            pendingCourseId = (int)existing;
            using var updCmd = new SqlCommand("UPDATE [LearningPaths] SET Status = 1, Visibility = 2, IsActive = 1, AuthorId = 2 WHERE Id = @id", conn, tx);
            updCmd.Parameters.AddWithValue("@id", pendingCourseId);
            updCmd.ExecuteNonQuery();
            Console.WriteLine("2.2 Đã cập nhật lộ trình Chờ duyệt (PendingReview) Id = " + pendingCourseId);
        }
    }

    // 2.2 Tạo 2 Giảng viên Chờ duyệt (TeacherPending) cho Admin
    string teacherPassHash;
    using (var cmd = new SqlCommand("SELECT PasswordHash FROM [Users] WHERE Id = 2", conn, tx))
        teacherPassHash = (string)cmd.ExecuteScalar();

    // Giảng viên chờ duyệt 1: pending.gv@dsavisual.com
    using (var cmd = new SqlCommand(@"
        IF EXISTS (SELECT 1 FROM [Users] WHERE Email = 'pending.gv@dsavisual.com')
        BEGIN
            UPDATE [Users] 
            SET Role = 2, DisplayName = N'TS. Nguyễn Thanh Tùng', 
                StaffCode = 'GV2026-089', Department = N'Khoa Công nghệ Thông tin - ĐH Bách Khoa',
                AcademicDegree = N'Tiến sĩ Khoa học Máy tính', 
                TeacherBio = N'12 năm giảng dạy Cấu trúc Dữ liệu, Giải thuật nâng cao và Lý thuyết Đồ thị.',
                IsActive = 1, PasswordHash = @pass
            WHERE Email = 'pending.gv@dsavisual.com'
        END
        ELSE
        BEGIN
            INSERT INTO [Users] (Email, PasswordHash, DisplayName, Role, IsActive, IsPrimaryAdmin, TwoFactorEnabled, Hearts, HeartsMax, LastHeartAt, Gems, Xp, StreakDays, StreakFreeze, CreatedAt, StaffCode, Department, AcademicDegree, TeacherBio)
            VALUES ('pending.gv@dsavisual.com', @pass, N'TS. Nguyễn Thanh Tùng', 2, 1, 0, 0, 10, 10, SYSUTCDATETIME(), 500, 0, 0, 0, SYSUTCDATETIME(), 'GV2026-089', N'Khoa Công nghệ Thông tin - ĐH Bách Khoa', N'Tiến sĩ Khoa học Máy tính', N'12 năm giảng dạy Cấu trúc Dữ liệu, Giải thuật nâng cao và Lý thuyết Đồ thị.')
        END", conn, tx))
    {
        cmd.Parameters.AddWithValue("@pass", teacherPassHash);
        cmd.ExecuteNonQuery();
        Console.WriteLine("2.3 Đã tạo/cập nhật Giảng viên chờ duyệt 1 (TS. Nguyễn Thanh Tùng - GV2026-089)");
    }

    // Giảng viên chờ duyệt 2: tranvanhung.pending@university.edu.vn
    using (var cmd = new SqlCommand(@"
        IF EXISTS (SELECT 1 FROM [Users] WHERE Email = 'tranvanhung.pending@university.edu.vn')
        BEGIN
            UPDATE [Users] 
            SET Role = 2, DisplayName = N'ThS. Trần Văn Hùng', 
                StaffCode = 'GV2026-104', Department = N'Khoa Khoa học Máy tính - ĐH Khoa học Tự nhiên',
                AcademicDegree = N'Thạc sĩ Hệ thống Thông tin', 
                TeacherBio = N'Nghiên cứu ứng dụng Cấu trúc Dữ liệu trong Cơ sở Dữ liệu phân tán.',
                IsActive = 1, PasswordHash = @pass
            WHERE Email = 'tranvanhung.pending@university.edu.vn'
        END
        ELSE
        BEGIN
            INSERT INTO [Users] (Email, PasswordHash, DisplayName, Role, IsActive, IsPrimaryAdmin, TwoFactorEnabled, Hearts, HeartsMax, LastHeartAt, Gems, Xp, StreakDays, StreakFreeze, CreatedAt, StaffCode, Department, AcademicDegree, TeacherBio)
            VALUES ('tranvanhung.pending@university.edu.vn', @pass, N'ThS. Trần Văn Hùng', 2, 1, 0, 0, 10, 10, SYSUTCDATETIME(), 500, 0, 0, 0, SYSUTCDATETIME(), 'GV2026-104', N'Khoa Khoa học Máy tính - ĐH Khoa học Tự nhiên', N'Thạc sĩ Hệ thống Thông tin', N'Nghiên cứu ứng dụng Cấu trúc Dữ liệu trong Cơ sở Dữ liệu phân tán.')
        END", conn, tx))
    {
        cmd.Parameters.AddWithValue("@pass", teacherPassHash);
        cmd.ExecuteNonQuery();
        Console.WriteLine("2.4 Đã tạo/cập nhật Giảng viên chờ duyệt 2 (ThS. Trần Văn Hùng - GV2026-104)");
    }

    // 2.3 Tạo 4 Lớp học chuẩn hóa
    int class1Id, class2Id, class3Id, class4Id;

    // Lớp 1: PRO192 (Đã đóng / Đã hoàn thành 100%)
    using (var cmd = new SqlCommand(@"
        INSERT INTO [Classes] (Name, InviteCode, Semester, Description, OwnerId, Status, CreatedAt, LearningPathId)
        OUTPUT INSERTED.Id
        VALUES (N'PRO192 — Cấu trúc Dữ liệu & Giải thuật (HK2-2025)', 'PRO192', 'HK2-2025', N'Lớp Cấu trúc Dữ liệu & Giải thuật nâng cao — Đã hoàn thành trọn vẹn chương trình.', 2, 1, @created, 1)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@created", now.AddDays(-120));
        class1Id = (int)cmd.ExecuteScalar();
    }

    // Lớp 2: CSD201 (Đang mở / Chính khóa đang học tốt)
    using (var cmd = new SqlCommand(@"
        INSERT INTO [Classes] (Name, InviteCode, Semester, Description, OwnerId, Status, CreatedAt, LearningPathId)
        OUTPUT INSERTED.Id
        VALUES (N'CSD201 — Cấu trúc Dữ liệu & Giải thuật (HK1-2026)', 'DSA201', 'HK1-2026', N'Lớp chính khóa môn Cấu trúc Dữ liệu và Giải thuật — Học kỳ 1 năm 2026.', 2, 0, @created, 1)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@created", now.AddDays(-30));
        class2Id = (int)cmd.ExecuteScalar();
    }

    // Lớp 3: ALGO301 (Lớp giải thuật nâng cao / ICPC)
    using (var cmd = new SqlCommand(@"
        INSERT INTO [Classes] (Name, InviteCode, Semester, Description, OwnerId, Status, CreatedAt, LearningPathId)
        OUTPUT INSERTED.Id
        VALUES (N'ALGO301 — Giải thuật Nâng cao & Luyện thi ICPC (HK1-2026)', 'ALGO30', 'HK1-2026', N'Lớp chuyên đề Thuật toán nâng cao và Đồ thị — Đào tạo nòng cốt lập trình thi đấu.', 2, 0, @created, 21)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@created", now.AddDays(-14));
        class3Id = (int)cmd.ExecuteScalar();
    }

    // Lớp 4: DSA202 (Lớp cảnh báo deadline / Lagging Learners)
    using (var cmd = new SqlCommand(@"
        INSERT INTO [Classes] (Name, InviteCode, Semester, Description, OwnerId, Status, CreatedAt, LearningPathId)
        OUTPUT INSERTED.Id
        VALUES (N'DSA202 — Thực hành Cấu trúc Dữ liệu (HK1-2026)', 'DSA202', 'HK1-2026', N'Lớp thực hành chuyên sâu và kiểm tra tiến độ định kỳ.', 2, 0, @created, 2)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@created", now.AddDays(-20));
        class4Id = (int)cmd.ExecuteScalar();
    }

    Console.WriteLine($"2.5 Đã tạo 4 Lớp học chuẩn: PRO192(Id={class1Id}), CSD201(Id={class2Id}), ALGO301(Id={class3Id}), DSA202(Id={class4Id})");

    // 2.4 Thêm thành viên vào các lớp
    void AddMembers(int classId, int[] userIds, int joinedDaysAgo)
    {
        foreach (var uId in userIds)
        {
            using var cmd = new SqlCommand(@"
                INSERT INTO [ClassMembers] (ClassId, UserId, JoinedAt)
                VALUES (@cId, @uId, @joined)", conn, tx);
            cmd.Parameters.AddWithValue("@cId", classId);
            cmd.Parameters.AddWithValue("@uId", uId);
            cmd.Parameters.AddWithValue("@joined", now.AddDays(-joinedDaysAgo).AddMinutes(uId * 7 % 60));
            cmd.ExecuteNonQuery();
        }
    }

    // Lớp 1: 20 sinh viên (User 3, 4..22)
    var class1Members = new List<int> { 3 };
    for (int i = 4; i <= 22; i++) class1Members.Add(i);
    AddMembers(class1Id, class1Members.ToArray(), 115);

    // Lớp 2: 25 sinh viên (User 3, 4..27)
    var class2Members = new List<int> { 3 };
    for (int i = 4; i <= 27; i++) class2Members.Add(i);
    AddMembers(class2Id, class2Members.ToArray(), 28);

    // Lớp 3: 12 sinh viên (User 3, 4..14)
    var class3Members = new List<int> { 3 };
    for (int i = 4; i <= 14; i++) class3Members.Add(i);
    AddMembers(class3Id, class3Members.ToArray(), 13);

    // Lớp 4: 22 sinh viên (User 4..25 - không cho student 3 bị lagging)
    var class4Members = new List<int>();
    for (int i = 4; i <= 25; i++) class4Members.Add(i);
    AddMembers(class4Id, class4Members.ToArray(), 19);

    Console.WriteLine("2.6 Đã thêm sinh viên vào cả 4 lớp học");

    // 2.5 Giao bài tập (ClassAssignments) và nộp bài (ExerciseSubmissions)
    // Lớp 1 (PRO192): 3 bài tập trong quá khứ, 100% nộp đúng hạn
    int a1_1, a1_2, a1_3;
    using (var cmd = new SqlCommand("INSERT INTO [ClassAssignments] (ClassId, PathItemId, LessonId, ExerciseId, DueAt, AllowLateSubmission, SortOrder, Archived, CreatedAt) OUTPUT INSERTED.Id VALUES (@cId, 1, 1, 82, @due, 1, 1, 0, @created)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@cId", class1Id);
        cmd.Parameters.AddWithValue("@due", now.AddDays(-90));
        cmd.Parameters.AddWithValue("@created", now.AddDays(-115));
        a1_1 = (int)cmd.ExecuteScalar();
    }
    using (var cmd = new SqlCommand("INSERT INTO [ClassAssignments] (ClassId, PathItemId, LessonId, ExerciseId, DueAt, AllowLateSubmission, SortOrder, Archived, CreatedAt) OUTPUT INSERTED.Id VALUES (@cId, 2, 2, 1, @due, 1, 2, 0, @created)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@cId", class1Id);
        cmd.Parameters.AddWithValue("@due", now.AddDays(-75));
        cmd.Parameters.AddWithValue("@created", now.AddDays(-90));
        a1_2 = (int)cmd.ExecuteScalar();
    }
    using (var cmd = new SqlCommand("INSERT INTO [ClassAssignments] (ClassId, PathItemId, LessonId, ExerciseId, DueAt, AllowLateSubmission, SortOrder, Archived, CreatedAt) OUTPUT INSERTED.Id VALUES (@cId, 6, 4, 93, @due, 1, 3, 0, @created)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@cId", class1Id);
        cmd.Parameters.AddWithValue("@due", now.AddDays(-60));
        cmd.Parameters.AddWithValue("@created", now.AddDays(-75));
        a1_3 = (int)cmd.ExecuteScalar();
    }

    void Submit(int uId, int exId, int assignId, int score, DateTime subAt)
    {
        using var cmd = new SqlCommand(@"
            INSERT INTO [ExerciseSubmissions] (UserId, ExerciseId, ClassAssignmentId, Score, AnswersJson, ResultJson, DurationSeconds, SubmittedAt)
            VALUES (@uId, @exId, @aId, @score, '{}', '[]', 120, @subAt)", conn, tx);
        cmd.Parameters.AddWithValue("@uId", uId);
        cmd.Parameters.AddWithValue("@exId", exId);
        cmd.Parameters.AddWithValue("@aId", assignId);
        cmd.Parameters.AddWithValue("@score", score);
        cmd.Parameters.AddWithValue("@subAt", subAt);
        cmd.ExecuteNonQuery();
    }

    // 100% nộp đúng hạn lớp 1
    foreach (var uId in class1Members)
    {
        Submit(uId, 82, a1_1, 90 + (uId % 11), now.AddDays(-92));
        Submit(uId, 1, a1_2, 85 + (uId % 16), now.AddDays(-77));
        Submit(uId, 93, a1_3, 88 + (uId % 13), now.AddDays(-62));
    }

    // Lớp 2 (CSD201): 3 bài tập: bài 1 (hết hạn 10 ngày trước), bài 2 (hết hạn 3 ngày trước), bài 3 (còn 5 ngày)
    int a2_1, a2_2, a2_3;
    using (var cmd = new SqlCommand("INSERT INTO [ClassAssignments] (ClassId, PathItemId, LessonId, ExerciseId, DueAt, AllowLateSubmission, SortOrder, Archived, CreatedAt) OUTPUT INSERTED.Id VALUES (@cId, 1, 1, 82, @due, 1, 1, 0, @created)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@cId", class2Id);
        cmd.Parameters.AddWithValue("@due", now.AddDays(-10));
        cmd.Parameters.AddWithValue("@created", now.AddDays(-25));
        a2_1 = (int)cmd.ExecuteScalar();
    }
    using (var cmd = new SqlCommand("INSERT INTO [ClassAssignments] (ClassId, PathItemId, LessonId, ExerciseId, DueAt, AllowLateSubmission, SortOrder, Archived, CreatedAt) OUTPUT INSERTED.Id VALUES (@cId, 2, 2, 1, @due, 1, 2, 0, @created)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@cId", class2Id);
        cmd.Parameters.AddWithValue("@due", now.AddDays(-3));
        cmd.Parameters.AddWithValue("@created", now.AddDays(-10));
        a2_2 = (int)cmd.ExecuteScalar();
    }
    using (var cmd = new SqlCommand("INSERT INTO [ClassAssignments] (ClassId, PathItemId, LessonId, ExerciseId, DueAt, AllowLateSubmission, SortOrder, Archived, CreatedAt) OUTPUT INSERTED.Id VALUES (@cId, 5, 3, 88, @due, 1, 3, 0, @created)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@cId", class2Id);
        cmd.Parameters.AddWithValue("@due", now.AddDays(5));
        cmd.Parameters.AddWithValue("@created", now.AddDays(-3));
        a2_3 = (int)cmd.ExecuteScalar();
    }

    foreach (var uId in class2Members)
    {
        // Bài 1 nộp đủ
        Submit(uId, 82, a2_1, 80 + (uId * 3 % 21), now.AddDays(-12));
        // Bài 2: trừ user 26, 27 chưa nộp; student 3 đạt 100
        if (uId != 26 && uId != 27)
        {
            int score = uId == 3 ? 100 : (75 + (uId * 5 % 26));
            Submit(uId, 1, a2_2, score, now.AddDays(-4));
        }
    }

    // Lớp 3 (ALGO301): 2 bài tập tương lai
    using (var cmd = new SqlCommand("INSERT INTO [ClassAssignments] (ClassId, PathItemId, LessonId, ExerciseId, DueAt, AllowLateSubmission, SortOrder, Archived, CreatedAt) VALUES (@cId, 142, 138, 129, @due, 1, 1, 0, @created)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@cId", class3Id);
        cmd.Parameters.AddWithValue("@due", now.AddDays(7));
        cmd.Parameters.AddWithValue("@created", now.AddDays(-10));
        cmd.ExecuteNonQuery();
    }
    using (var cmd = new SqlCommand("INSERT INTO [ClassAssignments] (ClassId, PathItemId, LessonId, ExerciseId, DueAt, AllowLateSubmission, SortOrder, Archived, CreatedAt) VALUES (@cId, 16, 8, 105, @due, 1, 2, 0, @created)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@cId", class3Id);
        cmd.Parameters.AddWithValue("@due", now.AddDays(14));
        cmd.Parameters.AddWithValue("@created", now.AddDays(-5));
        cmd.ExecuteNonQuery();
    }

    // Lớp 4 (DSA202): LỚP DEMO TRỄ DEADLINE / LAGGING LEARNERS
    // Bài 1 (hết hạn 2 ngày trước), Bài 2 (hết hạn hôm qua), Bài 3 (còn 12 giờ)
    int a4_1, a4_2, a4_3;
    using (var cmd = new SqlCommand("INSERT INTO [ClassAssignments] (ClassId, PathItemId, LessonId, ExerciseId, DueAt, AllowLateSubmission, SortOrder, Archived, CreatedAt) OUTPUT INSERTED.Id VALUES (@cId, 5, 3, 88, @due, 1, 1, 0, @created)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@cId", class4Id);
        cmd.Parameters.AddWithValue("@due", now.AddDays(-2));
        cmd.Parameters.AddWithValue("@created", now.AddDays(-15));
        a4_1 = (int)cmd.ExecuteScalar();
    }
    using (var cmd = new SqlCommand("INSERT INTO [ClassAssignments] (ClassId, PathItemId, LessonId, ExerciseId, DueAt, AllowLateSubmission, SortOrder, Archived, CreatedAt) OUTPUT INSERTED.Id VALUES (@cId, 6, 4, 93, @due, 1, 2, 0, @created)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@cId", class4Id);
        cmd.Parameters.AddWithValue("@due", now.AddDays(-1));
        cmd.Parameters.AddWithValue("@created", now.AddDays(-10));
        a4_2 = (int)cmd.ExecuteScalar();
    }
    using (var cmd = new SqlCommand("INSERT INTO [ClassAssignments] (ClassId, PathItemId, LessonId, ExerciseId, DueAt, AllowLateSubmission, SortOrder, Archived, CreatedAt) OUTPUT INSERTED.Id VALUES (@cId, 13, 7, 102, @due, 1, 3, 0, @created)", conn, tx))
    {
        cmd.Parameters.AddWithValue("@cId", class4Id);
        cmd.Parameters.AddWithValue("@due", now.AddHours(12));
        cmd.Parameters.AddWithValue("@created", now.AddDays(-3));
        a4_3 = (int)cmd.ExecuteScalar();
    }

    // Phân bổ nộp bài cho lớp 4:
    // - 15 học viên nộp đúng hạn (User 4..18)
    for (int u = 4; u <= 18; u++)
    {
        Submit(u, 88, a4_1, 80 + (u % 21), now.AddDays(-2).AddHours(-4));
        Submit(u, 93, a4_2, 85 + (u % 16), now.AddDays(-1).AddHours(-6));
    }
    // - 4 học viên nộp TRỄ hạn (User 19, 20, 21, 22) (SubmittedAt > DueAt)
    for (int u = 19; u <= 22; u++)
    {
        Submit(u, 88, a4_1, 70 + (u % 21), now.AddDays(-2).AddHours(5)); // Nộp trễ 5 tiếng
        Submit(u, 93, a4_2, 75 + (u % 16), now.AddDays(-1).AddHours(8)); // Nộp trễ 8 tiếng
    }
    // - 3 học viên CHƯA NỘP cả 2 bài đã quá hạn (User 23, 24, 25) -> Lagging Learners cờ đỏ!
    // (Không gọi Submit cho 3 học viên này)

    Console.WriteLine("2.7 Đã phân bổ bài tập, nộp đúng hạn, nộp trễ và Lagging Learners cho các lớp");

    // 2.6 Chuẩn hóa Feedback & BugReports
    // Bổ sung 4 CourseFeedback cho teacher
    using (var cmd = new SqlCommand(@"
        DELETE FROM [CourseFeedback];
        INSERT INTO [CourseFeedback] (CourseId, UserId, Type, Content, Status, ReplyText, RepliedById, RepliedAt, CreatedAt, UpdatedAt)
        VALUES 
        (1, 4, 0, N'Bài giảng Sắp xếp nổi bọt rất trực quan, phần mô phỏng từng bước giúp em hiểu rõ hơn slide trên lớp ạ!', 2, N'Cảm ơn em! Thầy đã bổ sung thêm phần phân tích tối ưu cờ hiệu (flag swapped) ở bài tập kế tiếp nhé.', 2, @repAt, @cr1, @cr1),
        (1, 5, 2, N'Thầy có thể ra thêm bài tập code về tìm kiếm nhị phân dạng biến thể (Lower bound / Upper bound) không ạ?', 2, N'Ý kiến rất hay! Thầy vừa thêm bài tập tìm kiếm biên vào phần Codelab thực hành rồi nhé.', 2, @repAt, @cr2, @cr2),
        (2, 6, 0, N'Phần trực quan ngăn xếp và hàng đợi rất mượt mà. Mong thầy bổ sung thêm bài tập về hàng đợi ưu tiên ạ.', 1, NULL, NULL, NULL, @cr3, @cr3),
        (3, 7, 0, N'Thầy ơi cho em hỏi phần duyệt cây theo thứ tự sau (Postorder) có ứng dụng gì trong thực tế nhiều nhất vậy ạ?', 0, NULL, NULL, NULL, @cr4, @cr4);", conn, tx))
    {
        cmd.Parameters.AddWithValue("@repAt", now.AddDays(-1));
        cmd.Parameters.AddWithValue("@cr1", now.AddDays(-3));
        cmd.Parameters.AddWithValue("@cr2", now.AddDays(-2));
        cmd.Parameters.AddWithValue("@cr3", now.AddDays(-1));
        cmd.Parameters.AddWithValue("@cr4", now.AddHours(-5));
        cmd.ExecuteNonQuery();
        Console.WriteLine("2.8 Đã khởi tạo 4 CourseFeedback (2 đã trả lời, 1 đã đọc, 1 mới chờ duyệt)");
    }

    // Bổ sung BugReports chuẩn cho Admin
    using (var cmd = new SqlCommand(@"
        DELETE FROM [BugReports];
        INSERT INTO [BugReports] (UserId, Description, ContextJson, Status, CreatedAt, ResolvedAt, AdminNote)
        VALUES 
        (4, N'Mô phỏng thuật toán trên màn hình điện thoại đôi khi bị tràn ngang khung nhìn.', @ctx1, 2, @b1, @r1, N'Đã cập nhật CSS responsive flex-wrap cho toàn bộ khung Canvas mô phỏng.'),
        (5, N'Tốc độ mô phỏng QuickSort ở mức x2 hơi nhanh, mong hệ thống thêm mức x1.5.', @ctx2, 2, @b2, @r2, N'Đã bổ sung các mốc tốc độ 0.5x, 1x, 1.5x, 2x trong thanh công cụ điều khiển.'),
        (6, N'Khi ấn nộp bài Codelab đôi lúc mất 3-4 giây mới hiện kết quả test case.', @ctx3, 1, @b3, NULL, N'Đang cấu hình tăng bộ nhớ đệm cho worker container chấm code.'),
        (7, N'Hệ thống xem xét bổ sung chế độ Dark Mode cho trang Studio giảng viên.', @ctx4, 0, @b4, NULL, NULL),
        (8, N'Em không nhận được thông báo khi bài tập lớp sắp đến hạn chót (trước 24h).', @ctx5, 0, @b5, NULL, NULL);", conn, tx))
    {
        cmd.Parameters.AddWithValue("@ctx1", "{\"device\": \"mobile\"}");
        cmd.Parameters.AddWithValue("@ctx2", "{\"feature\": \"speed\"}");
        cmd.Parameters.AddWithValue("@ctx3", "{\"module\": \"runner\"}");
        cmd.Parameters.AddWithValue("@ctx4", "{\"ui\": \"studio\"}");
        cmd.Parameters.AddWithValue("@ctx5", "{\"module\": \"notification\"}");
        cmd.Parameters.AddWithValue("@b1", now.AddDays(-5));
        cmd.Parameters.AddWithValue("@r1", now.AddDays(-4));
        cmd.Parameters.AddWithValue("@b2", now.AddDays(-3));
        cmd.Parameters.AddWithValue("@r2", now.AddDays(-2));
        cmd.Parameters.AddWithValue("@b3", now.AddDays(-1));
        cmd.Parameters.AddWithValue("@b4", now.AddHours(-8));
        cmd.Parameters.AddWithValue("@b5", now.AddHours(-2));
        cmd.ExecuteNonQuery();
        Console.WriteLine("2.9 Đã khởi tạo 5 BugReports (2 Resolved, 1 Processing, 2 New)");
    }

    // 2.7 Cấu hình tài khoản Học viên Demo (student@demo.local, Id = 3)
    using (var cmd = new SqlCommand(@"
        UPDATE [Users] 
        SET XP = 2500, StreakDays = 5, Hearts = 8, HeartsMax = 10, Gems = 450, UpdatedAt = SYSUTCDATETIME()
        WHERE Id = 3", conn, tx))
    {
        cmd.ExecuteNonQuery();
        Console.WriteLine("2.10 Đã thiết lập chỉ số cho student@demo.local: XP=2500 (Top 4), Streak=5 ngày, Tim=8/10, Gems=450");
    }

    // Trang bị vật phẩm trong kho đồ cho student (Item 2 Golden Knight, Item 6 Neon Border)
    using (var cmd = new SqlCommand(@"
        DELETE FROM [UserInventory] WHERE UserId = 3;
        INSERT INTO [UserInventory] (UserId, ItemId, Quantity, IsEquipped, PurchasedAt)
        VALUES 
        (3, 2, 1, 1, @pAt),
        (3, 6, 1, 1, @pAt);", conn, tx))
    {
        cmd.Parameters.AddWithValue("@pAt", now.AddDays(-5));
        cmd.ExecuteNonQuery();
        Console.WriteLine("2.11 Đã trang bị Avatar Golden Knight & Khung viền Neon cho student");
    }

    // Tiến độ học tập của student: Lộ trình 1 (100%), Lộ trình 2 (66%), Lộ trình 3 (33%)
    using (var cmd = new SqlCommand(@"
        DELETE FROM [UserNodeProgress] WHERE UserId = 3;
        -- Lộ trình 1 (Nodes 1, 2, 3, 4) - Hoàn thành 100%
        INSERT INTO [UserNodeProgress] (UserId, NodeId, Status, Stars, NodeScore, UnlockedAt, PassedAt, UpdatedAt)
        VALUES 
        (3, 1, 2, 3, 100, @t1, @t1, @t1),
        (3, 2, 2, 3, 100, @t2, @t2, @t2),
        (3, 3, 2, 3, 95, @t3, @t3, @t3),
        (3, 4, 2, 3, 100, @t4, @t4, @t4);

        -- Lộ trình 2 (Nodes 5, 6, 7, 8) - Đang học 66%
        INSERT INTO [UserNodeProgress] (UserId, NodeId, Status, Stars, NodeScore, UnlockedAt, PassedAt, UpdatedAt)
        VALUES 
        (3, 5, 2, 3, 90, @t4, @t5, @t5),
        (3, 6, 1, 0, 0, @t5, NULL, @t5),
        (3, 7, 0, 0, 0, @t5, NULL, @t5),
        (3, 8, 0, 0, 0, @t5, NULL, @t5);

        -- Lộ trình 3 (Nodes 9, 10, 11, 12) - Bắt đầu 33%
        INSERT INTO [UserNodeProgress] (UserId, NodeId, Status, Stars, NodeScore, UnlockedAt, PassedAt, UpdatedAt)
        VALUES 
        (3, 9, 2, 3, 85, @t5, @t6, @t6),
        (3, 10, 1, 0, 0, @t6, NULL, @t6),
        (3, 11, 0, 0, 0, @t6, NULL, @t6),
        (3, 12, 0, 0, 0, @t6, NULL, @t6);", conn, tx))
    {
        cmd.Parameters.AddWithValue("@t1", now.AddDays(-10));
        cmd.Parameters.AddWithValue("@t2", now.AddDays(-7));
        cmd.Parameters.AddWithValue("@t3", now.AddDays(-5));
        cmd.Parameters.AddWithValue("@t4", now.AddDays(-3));
        cmd.Parameters.AddWithValue("@t5", now.AddDays(-1));
        cmd.Parameters.AddWithValue("@t6", now.AddHours(-4));
        cmd.ExecuteNonQuery();
        Console.WriteLine("2.12 Đã thiết lập tiến độ học tập cân đối cho student (Lộ trình 1: 100%, Lộ trình 2: 66%, Lộ trình 3: 33%)");
    }

    // Nhiệm vụ hôm nay cho student
    using (var cmd = new SqlCommand(@"
        DELETE FROM [UserQuests] WHERE UserId = 3 AND QuestDate = CAST(SYSUTCDATETIME() AS DATE);
        INSERT INTO [UserQuests] (UserId, QuestId, QuestDate, Progress, Claimed)
        VALUES 
        (3, 1, CAST(SYSUTCDATETIME() AS DATE), 1, 1),
        (3, 2, CAST(SYSUTCDATETIME() AS DATE), 1, 0),
        (3, 3, CAST(SYSUTCDATETIME() AS DATE), 0, 0);", conn, tx))
    {
        cmd.ExecuteNonQuery();
        Console.WriteLine("2.13 Đã thiết lập 3 Nhiệm vụ hàng ngày hôm nay cho student (1 Claimed, 1 Đang làm 50%, 1 Chưa làm)");
    }

    // Đánh giá ContentFeedback chuẩn cho các bài học
    using (var cmd = new SqlCommand(@"
        IF NOT EXISTS (SELECT 1 FROM [ContentFeedback] WHERE LessonId = 1 AND UserId = 4)
        BEGIN
            INSERT INTO [ContentFeedback] (UserId, LessonId, Rating, Comment, CreatedAt, UpdatedAt)
            VALUES 
            (4, 1, 5, N'Bài học Bubble Sort có mô phỏng từng bước trực quan, rất dễ hiểu cho người mới bắt đầu.', @cAt, @cAt),
            (5, 2, 5, N'Giải thích thuật toán Binary Search rõ ràng, có phân tích cận trên cận dưới rất hay.', @cAt, @cAt),
            (6, 3, 5, N'Cơ chế LIFO của Stack được vẽ đồ họa rất sinh động, dễ hình dung ứng dụng kiểm tra ngoặc.', @cAt, @cAt),
            (7, 4, 4, N'Bài học Danh sách liên kết giải thích tốt, mong bổ sung thêm bài tập về con trỏ kép.', @cAt, @cAt);
        END", conn, tx))
    {
        cmd.Parameters.AddWithValue("@cAt", now.AddDays(-2));
        cmd.ExecuteNonQuery();
        Console.WriteLine("2.14 Đã bổ sung đánh giá chất lượng cho các bài học chính");
    }

    tx.Commit();
    Console.WriteLine("\n==================================================================");
    Console.WriteLine("    🎉 TOÀN BỘ QUY TRÌNH DỌN DẸP & SEEDER ĐÃ HOÀN TẤT THÀNH CÔNG!   ");
    Console.WriteLine("==================================================================");
}
catch (Exception ex)
{
    tx.Rollback();
    Console.WriteLine("\n❌ LỖI TRONG QUÁ TRÌNH THỰC THI - ĐÃ ROLLBACK TOÀN BỘ: " + ex.Message);
    throw;
}
