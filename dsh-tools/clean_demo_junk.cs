#:package Microsoft.Data.SqlClient@6.0.2
#:property PublishAot=false
using System;
using Microsoft.Data.SqlClient;

var cs = "Server=db65198.public.databaseasp.net; Database=db65198; User Id=db65198; Password=4Rn+#6EoB!a8; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True; Connect Timeout=60;";
using var conn = new SqlConnection(cs);
conn.Open();

Console.WriteLine("=== BẮT ĐẦU DỌN DẸP DỮ LIỆU RÁC (CLEANUP PHASE) ===");
using var tx = conn.BeginTransaction();

try
{
    var junkPathIds = "(23, 32, 36, 37, 38, 41, 45, 47, 49, 56, 58, 59, 66)";

    // 1. Dọn dẹp Token hết hạn & thu hồi
    using (var cmd = new SqlCommand("DELETE FROM [RefreshTokens] WHERE RevokedAt IS NOT NULL OR ExpiresAt < SYSUTCDATETIME()", conn, tx))
        Console.WriteLine("1. RefreshTokens đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [OtpCodes] WHERE Used = 1 OR ExpiresAt < SYSUTCDATETIME()", conn, tx))
        Console.WriteLine("2. OtpCodes đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [RegisterOtpCodes] WHERE Used = 1 OR ExpiresAt < SYSUTCDATETIME()", conn, tx))
        Console.WriteLine("3. RegisterOtpCodes đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [PasswordResetTokens] WHERE Used = 1 OR ExpiresAt < SYSUTCDATETIME()", conn, tx))
        Console.WriteLine("4. PasswordResetTokens đã xóa: " + cmd.ExecuteNonQuery());

    // 2. Dọn dẹp NodeSessions hết hạn hoặc trỏ node rác
    using (var cmd = new SqlCommand("DELETE FROM [NodeSessions] WHERE ExpiresAt < SYSUTCDATETIME() OR NodeId IN (SELECT Id FROM [LearningPathNodes] WHERE DeletedAt IS NOT NULL OR PathId IN " + junkPathIds + ")", conn, tx))
        Console.WriteLine("5. NodeSessions đã xóa: " + cmd.ExecuteNonQuery());

    // 3. Dọn dẹp 4 vật phẩm shop rác (11, 13, 15, 16)
    using (var cmd = new SqlCommand("DELETE FROM [UserInventory] WHERE ItemId IN (11, 13, 15, 16)", conn, tx))
        Console.WriteLine("6. UserInventory (shop rác) đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [GemTransactions] WHERE RefType = 'shop' AND RefId IN ('11', '13', '15', '16')", conn, tx))
        Console.WriteLine("7. GemTransactions (shop rác) đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [ShopItems] WHERE Id IN (11, 13, 15, 16)", conn, tx))
        Console.WriteLine("8. ShopItems rác đã xóa: " + cmd.ExecuteNonQuery());

    // 4. Dọn dẹp BugReports & Feedbacks test rác
    using (var cmd = new SqlCommand("DELETE FROM [BugReports] WHERE Id IN (11, 12, 13, 14, 15, 17)", conn, tx))
        Console.WriteLine("9. BugReports rác QA đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [CourseFeedback] WHERE Id IN (5, 6, 7) OR CourseId IN " + junkPathIds, conn, tx))
        Console.WriteLine("10. CourseFeedback spam đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [ContentFeedback] WHERE Comment = '' OR Comment IS NULL", conn, tx))
        Console.WriteLine("11. ContentFeedback rỗng đã xóa: " + cmd.ExecuteNonQuery());

    // 5. Dọn dẹp UserQuests quá khứ
    using (var cmd = new SqlCommand("DELETE FROM [UserQuests] WHERE QuestDate < CAST(SYSUTCDATETIME() AS DATE)", conn, tx))
        Console.WriteLine("12. UserQuests ngày cũ đã xóa: " + cmd.ExecuteNonQuery());

    // 6. Xóa sạch TOÀN BỘ 15 lớp học cũ & các bài nộp, phân công, thành viên
    using (var cmd = new SqlCommand("DELETE FROM [ExerciseSubmissions] WHERE ClassAssignmentId IS NOT NULL", conn, tx))
        Console.WriteLine("13. ExerciseSubmissions gắn Assignment đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [ClassAssignments]", conn, tx))
        Console.WriteLine("14. ClassAssignments đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [ClassMembers]", conn, tx))
        Console.WriteLine("15. ClassMembers đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [Classes]", conn, tx))
        Console.WriteLine("16. Classes đã xóa: " + cmd.ExecuteNonQuery());

    // 7. Gỡ bỏ tham chiếu FK từ Exercises.NodeId và LearningPathNodes.ParentId TRƯỚC KHI XÓA
    using (var cmd = new SqlCommand("UPDATE [Exercises] SET NodeId = NULL WHERE NodeId IN (SELECT Id FROM [LearningPathNodes] WHERE DeletedAt IS NOT NULL OR PathId IN " + junkPathIds + ")", conn, tx))
        Console.WriteLine("17. Exercises.NodeId gỡ liên kết: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("UPDATE [LearningPathNodes] SET ParentId = NULL, FinalTestId = NULL, LabExerciseId = NULL, LessonId = NULL WHERE DeletedAt IS NOT NULL OR PathId IN " + junkPathIds, conn, tx))
        Console.WriteLine("18. LearningPathNodes gỡ liên kết tự thân/exercises/lessons: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [UserNodeProgress] WHERE NodeId IN (SELECT Id FROM [LearningPathNodes] WHERE DeletedAt IS NOT NULL OR PathId IN " + junkPathIds + ")", conn, tx))
        Console.WriteLine("19. UserNodeProgress (node rác) đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [LearningPathNodes] WHERE DeletedAt IS NOT NULL OR PathId IN " + junkPathIds, conn, tx))
        Console.WriteLine("20. LearningPathNodes (node rác) đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [LearningPaths] WHERE Id IN " + junkPathIds, conn, tx))
        Console.WriteLine("21. LearningPaths (lộ trình rác) đã xóa: " + cmd.ExecuteNonQuery());

    // 8. Dọn dẹp các Bài tập và Bài học mồ côi (không thuộc bất kỳ node nào còn lại)
    var orphanLessonQuery = "SELECT Id FROM [Lessons] WHERE Id NOT IN (SELECT LessonId FROM [LearningPathNodes] WHERE LessonId IS NOT NULL)";

    // 8a. Gỡ LessonId ở các Exercises nếu Lesson của nó là mồ côi (nhưng Exercise vẫn thuộc Quiz/Lab node hợp lệ)
    using (var cmd = new SqlCommand("UPDATE [Exercises] SET LessonId = NULL WHERE LessonId IN (" + orphanLessonQuery + ")", conn, tx))
        Console.WriteLine("22. Exercises gỡ LessonId mồ côi: " + cmd.ExecuteNonQuery());

    // 8b. Định nghĩa Bài tập mồ côi THỰC SỰ (không thuộc LabExerciseId, không thuộc FinalTestId, và không thuộc Lesson nào còn lại)
    var trueOrphanExQuery = @"SELECT e.Id FROM [Exercises] e 
        WHERE NOT EXISTS (SELECT 1 FROM [LearningPathNodes] n WHERE n.LabExerciseId = e.Id OR n.FinalTestId = e.Id)
          AND (e.LessonId IS NULL OR NOT EXISTS (SELECT 1 FROM [LearningPathNodes] n WHERE n.LessonId = e.LessonId))";

    using (var cmd = new SqlCommand("DELETE FROM [Questions] WHERE ExerciseId IN (" + trueOrphanExQuery + ")", conn, tx))
        Console.WriteLine("23. Questions mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [ExerciseSubmissions] WHERE ExerciseId IN (" + trueOrphanExQuery + ")", conn, tx))
        Console.WriteLine("24. ExerciseSubmissions mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [CodeRuns] WHERE ExerciseId IN (" + trueOrphanExQuery + ")", conn, tx))
        Console.WriteLine("25. CodeRuns mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [CodeSubmissions] WHERE ExerciseId IN (" + trueOrphanExQuery + ")", conn, tx))
        Console.WriteLine("26. CodeSubmissions mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [Exercises] WHERE Id IN (" + trueOrphanExQuery + ")", conn, tx))
        Console.WriteLine("27. Exercises mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    // 8c. Dọn dẹp phụ thuộc của Lessons mồ côi
    using (var cmd = new SqlCommand("DELETE FROM [LessonSimulations] WHERE LessonId IN (" + orphanLessonQuery + ")", conn, tx))
        Console.WriteLine("28. LessonSimulations mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [UserProgress] WHERE LessonId IN (" + orphanLessonQuery + ")", conn, tx))
        Console.WriteLine("29. UserProgress mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [ContentFeedback] WHERE LessonId IN (" + orphanLessonQuery + ")", conn, tx))
        Console.WriteLine("30. ContentFeedback mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [LessonNotes] WHERE LessonId IN (" + orphanLessonQuery + ")", conn, tx))
        Console.WriteLine("31. LessonNotes mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [Lessons] WHERE Id IN (" + orphanLessonQuery + ")", conn, tx))
        Console.WriteLine("32. Lessons mồ côi đã xóa: " + cmd.ExecuteNonQuery());

    // 9. Bây giờ xóa Topics rác/soft-deleted an toàn (vì lessons của nó đã bị xóa)
    using (var cmd = new SqlCommand("UPDATE [Topics] SET ParentId = NULL WHERE DeletedAt IS NOT NULL OR Id = 35", conn, tx))
        Console.WriteLine("33. Topics gỡ ParentId: " + cmd.ExecuteNonQuery());

    using (var cmd = new SqlCommand("DELETE FROM [Topics] WHERE DeletedAt IS NOT NULL OR Id = 35", conn, tx))
        Console.WriteLine("34. Topics rác/soft-deleted đã xóa: " + cmd.ExecuteNonQuery());

    tx.Commit();
    Console.WriteLine("=== HOÀN TẤT DỌN DẸP RÁC THÀNH CÔNG (TRANSACTION COMMITTED) ===");
}
catch (Exception ex)
{
    tx.Rollback();
    Console.WriteLine("LỖI XẢY RA - ĐÃ ROLLBACK TOÀN BỘ: " + ex.Message);
    throw;
}
