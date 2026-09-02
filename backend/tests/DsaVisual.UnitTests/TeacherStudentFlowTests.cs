using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DsaVisual.UnitTests;

/// <summary>
/// Kiểm thử luồng toàn vẹn: Giáo viên tạo bài (Lý thuyết, Quiz 1/nhiều đáp án, Lab)
/// -> Admin duyệt xuất bản -> Học viên vào học từng bài -> Tính điểm, mở khóa tuần tự và cộng XP.
/// </summary>
public sealed class TeacherStudentFlowTests
{
    private readonly TestServices.FixedClock _clock = new();

    private async Task<(PathItemService PathItems, ExerciseService Exercises, LessonService Lessons, GamificationService Gamification, AppDbContext Db, int TeacherId, int StudentId, int AdminId)> SetupAsync()
    {
        var (db, _) = TestServices.CreateSqliteDb();

        var teacherId = 10;
        var studentId = 1;
        var adminId = 99;

        db.Users.Add(new User
        {
            Id = teacherId,
            Email = "teacher@dsa.local",
            PasswordHash = "hash",
            DisplayName = "GV Nguyễn Văn A",
            Role = UserRole.Teacher,
            CreatedAt = _clock.UtcNow
        });

        db.Users.Add(new User
        {
            Id = studentId,
            Email = "student@dsa.local",
            PasswordHash = "hash",
            DisplayName = "HV Trần Văn B",
            Role = UserRole.Student,
            Hearts = 10,
            HeartsMax = 10,
            LastHeartAt = _clock.UtcNow,
            Xp = 0,
            CreatedAt = _clock.UtcNow
        });

        db.Users.Add(new User
        {
            Id = adminId,
            Email = "admin@dsa.local",
            PasswordHash = "hash",
            DisplayName = "Admin",
            Role = UserRole.Admin,
            CreatedAt = _clock.UtcNow
        });

        db.Topics.Add(new Topic
        {
            Id = 1,
            Name = "Data Structures",
            CreatedBy = teacherId,
            CreatedAt = _clock.UtcNow
        });

        await db.SaveChangesAsync();

        var pathItems = new PathItemService(db, _clock, NullLogger<PathItemService>.Instance);
        var exercises = TestServices.CreateExerciseService(db, _clock);
        var lessons = TestServices.CreateLessonService(db, _clock);
        var gamification = TestServices.CreateGamificationService(db, _clock);

        return (pathItems, exercises, lessons, gamification, db, teacherId, studentId, adminId);
    }

    [Fact]
    public async Task EndToEnd_TeacherCreatesCurriculum_AndStudentCompletesFullCourse()
    {
        var (pathItems, exercises, lessons, gamification, db, teacherId, studentId, adminId) = await SetupAsync();

        // ═══════════════════════════════════════════════════════════════
        // BƯỚC 1: GIÁO VIÊN TẠO LỘ TRÌNH VÀ CÁC MỤC BÀI HỌC TRONG STUDIO
        // ═══════════════════════════════════════════════════════════════

        // 1.1 Tạo Lộ trình (Learning Path)
        var path = new LearningPath
        {
            Id = 1,
            Title = "Khóa học Cấu trúc dữ liệu nâng cao",
            Description = "Cây nhị phân, Heap và Đồ thị",
            TopicId = 1,
            CreatedBy = teacherId,
            AuthorId = teacherId,
            SortOrder = 1,
            Status = LearningPathStatus.Draft,
            IsActive = false
        };
        db.LearningPaths.Add(path);
        await db.SaveChangesAsync();

        // 1.2 Giáo viên tạo Node 1: Lý thuyết (Theory)
        var createTheoryRes = await pathItems.CreateItemAsync(teacherId, "TEACHER", path.Id, new PathItemCreateRequest
        {
            ItemType = PathItemType.Theory,
            Title = "Bài 1: Tổng quan Cây nhị phân tìm kiếm (BST)",
            Description = "Khái niệm và tính chất của BST",
            SortOrder = 1
        }, CancellationToken.None);
        Assert.True(createTheoryRes.IsSuccess, createTheoryRes.ErrorMessage);
        var theoryNodeId = createTheoryRes.Value!.Id;
        var theoryLessonId = createTheoryRes.Value.LessonId!.Value;

        // Cập nhật nội dung lý thuyết chi tiết cho bài 1
        var updateLessonRes = await lessons.UpdateAsync(teacherId, "TEACHER", theoryLessonId, new LessonUpsertRequest
        {
            TopicId = 1,
            Title = "Bài 1: Tổng quan Cây nhị phân tìm kiếm (BST)",
            Description = "Khái niệm và tính chất của BST",
            ContentHtml = "<h1>BST Overview</h1><p>Mỗi nút con trái nhỏ hơn nút cha, nút con phải lớn hơn nút cha.</p>",
            SortOrder = 1,
            Status = LessonStatus.Active
        }, CancellationToken.None);
        Assert.True(updateLessonRes.IsSuccess, updateLessonRes.ErrorMessage);

        // 1.3 Giáo viên tạo Node 2: Quiz trắc nghiệm (Single + Multiple choice)
        var createQuizRes = await pathItems.CreateItemAsync(teacherId, "TEACHER", path.Id, new PathItemCreateRequest
        {
            ItemType = PathItemType.Quiz,
            Title = "Bài 2: Quiz kiểm tra hiểu biết về BST",
            Description = "2 câu trắc nghiệm kiểm tra lý thuyết",
            SortOrder = 2
        }, CancellationToken.None);
        Assert.True(createQuizRes.IsSuccess, createQuizRes.ErrorMessage);
        var quizNodeId = createQuizRes.Value!.Id;
        var quizExerciseId = createQuizRes.Value.FinalTestId!.Value;

        // Cập nhật câu hỏi cho Quiz (Câu 1: Single Choice, Câu 2: Multiple Choice)
        var updateQuizRes = await exercises.UpdateAsync(teacherId, "TEACHER", quizExerciseId, new ExerciseUpsertRequest
        {
            LessonId = theoryLessonId,
            NodeId = quizNodeId,
            Title = "Bài 2: Quiz kiểm tra hiểu biết về BST",
            Description = "2 câu hỏi",
            Type = ExerciseType.Mcq,
            MaxScore = 100,
            Status = ExerciseStatus.Active,
            Questions =
            [
                new QuestionUpsertDto
                {
                    Content = "Nút con bên trái trong BST có giá trị như thế nào so với nút cha?",
                    Type = QuestionType.Single,
                    Options = ["Nhỏ hơn", "Lớn hơn", "Bằng nhau", "Ngẫu nhiên"],
                    AnswerJson = "[0]", // Đáp án A: Nhỏ hơn
                    Explanation = "Theo định nghĩa BST, cây con trái chứa các khóa nhỏ hơn khóa của nút cha.",
                    Points = 50,
                    SortOrder = 1
                },
                new QuestionUpsertDto
                {
                    Content = "Các phép duyệt cây nào sau đây là chuẩn trong Binary Tree? (Chọn 2 đáp án)",
                    Type = QuestionType.Multi,
                    Options = ["In-order (L-N-R)", "Pre-order (N-L-R)", "Random-order", "Circular-order"],
                    AnswerJson = "[0, 1]", // Đáp án A & B
                    Explanation = "In-order, Pre-order và Post-order là các phép duyệt cây nhị phân kinh điển.",
                    Points = 50,
                    SortOrder = 2
                }
            ]
        }, CancellationToken.None);
        Assert.True(updateQuizRes.IsSuccess, updateQuizRes.ErrorMessage);

        // 1.4 Giáo viên tạo Node 3: Codelab thực hành
        var createLabRes = await pathItems.CreateItemAsync(teacherId, "TEACHER", path.Id, new PathItemCreateRequest
        {
            ItemType = PathItemType.Lab,
            Title = "Bài 3: Cài đặt hàm tìm kiếm trên BST",
            Description = "Viết hàm tìm kiếm một giá trị trên cây BST",
            SortOrder = 3
        }, CancellationToken.None);
        Assert.True(createLabRes.IsSuccess, createLabRes.ErrorMessage);
        var labNodeId = createLabRes.Value!.Id;
        var labExerciseId = createLabRes.Value.LabExerciseId!.Value;

        var labConfig = new
        {
            entryFunction = "searchBST",
            starterCode = "function searchBST(arr, val) {\n  return arr.includes(val);\n}",
            testCases = new[]
            {
                new { input = "[[4, 2, 7, 1, 3], 2]", expected = "true", hidden = false },
                new { input = "[[4, 2, 7, 1, 3], 5]", expected = "false", hidden = true }
            }
        };

        var updateLabRes = await exercises.UpdateAsync(teacherId, "TEACHER", labExerciseId, new ExerciseUpsertRequest
        {
            LessonId = theoryLessonId,
            NodeId = labNodeId,
            Title = "Bài 3: Cài đặt hàm tìm kiếm trên BST",
            Description = "Cài đặt thuật toán tìm kiếm BST",
            Type = ExerciseType.Code,
            ConfigJson = JsonSerializer.Serialize(labConfig),
            MaxScore = 100,
            Status = ExerciseStatus.Active
        }, CancellationToken.None);
        Assert.True(updateLabRes.IsSuccess, updateLabRes.ErrorMessage);

        // ═══════════════════════════════════════════════════════════════
        // BƯỚC 2: ADMIN DUYỆT XUẤT BẢN KHÓA HỌC
        // ═══════════════════════════════════════════════════════════════
        path.Status = LearningPathStatus.Active;
        path.IsActive = true;
        path.Visibility = PathVisibility.Public;
        await db.SaveChangesAsync();

        // ═══════════════════════════════════════════════════════════════
        // BƯỚC 3: HỌC VIÊN TRUY CẬP VÀ HỌC TỪNG BÀI
        // ═══════════════════════════════════════════════════════════════

        // 3.1 Học viên lấy bản đồ lộ trình: Tiến độ 0%, Node 1 mở ("active"), Node 2 & 3 khóa ("locked")
        var mapRes1 = await gamification.GetLearningPathAsync(studentId, path.Id, CancellationToken.None);
        Assert.True(mapRes1.IsSuccess);
        Assert.Equal(0, mapRes1.Value!.ProgressPct);
        Assert.Equal("active", mapRes1.Value.Nodes.First(n => n.Id == theoryNodeId).Status);
        Assert.Equal("locked", mapRes1.Value.Nodes.First(n => n.Id == quizNodeId).Status);
        Assert.Equal("locked", mapRes1.Value.Nodes.First(n => n.Id == labNodeId).Status);

        // 3.2 Học viên vào học Node 1 (Theory)
        var enterTheoryRes = await gamification.EnterNodeAsync(studentId, path.Id, theoryNodeId, null, CancellationToken.None);
        Assert.True(enterTheoryRes.IsSuccess);
        Assert.Equal(9, enterTheoryRes.Value!.HeartsLeft); // Trừ 1 tim lần đầu

        // Học viên đọc xong lý thuyết -> Cập nhật UserNodeProgress sang Status = 2 (Passed) + cộng 50 XP
        var node1Progress = await db.UserNodeProgress.FirstAsync(p => p.UserId == studentId && p.NodeId == theoryNodeId);
        node1Progress.Status = 2;
        node1Progress.Stars = 3;
        node1Progress.NodeScore = 100;
        node1Progress.PassedAt = _clock.UtcNow;
        node1Progress.UpdatedAt = _clock.UtcNow;

        var studentUser = await db.Users.FirstAsync(u => u.Id == studentId);
        studentUser.Xp += 50; // +50 XP lý thuyết lần đầu
        await db.SaveChangesAsync();

        // 3.3 Kiểm tra lộ trình sau khi hoàn thành Node 1:
        // Tiến độ = 33% (1/3), Node 2 (Quiz) TỰ ĐỘNG CHUYỂN SANG "active"!
        var mapRes2 = await gamification.GetLearningPathAsync(studentId, path.Id, CancellationToken.None);
        Assert.Equal(33, mapRes2.Value!.ProgressPct);
        Assert.Equal("passed", mapRes2.Value.Nodes.First(n => n.Id == theoryNodeId).Status);
        Assert.Equal("active", mapRes2.Value.Nodes.First(n => n.Id == quizNodeId).Status);
        Assert.Equal("locked", mapRes2.Value.Nodes.First(n => n.Id == labNodeId).Status);

        // 3.4 Học viên vào làm Node 2 (Quiz)
        var enterQuizRes = await gamification.EnterNodeAsync(studentId, path.Id, quizNodeId, null, CancellationToken.None);
        Assert.True(enterQuizRes.IsSuccess);

        // Lấy danh sách câu hỏi Quiz từ ExerciseService
        var quizDetail = await exercises.GetByIdAsync(studentId, quizExerciseId, CancellationToken.None);
        Assert.True(quizDetail.IsSuccess);
        Assert.Equal(2, quizDetail.Value!.Questions.Count);

        // Học viên nộp bài Quiz với đáp án đúng: Câu 1 chọn [0], Câu 2 chọn [0, 1]
        var q1 = quizDetail.Value.Questions[0];
        var q2 = quizDetail.Value.Questions[1];

        var submitQuizRes = await exercises.SubmitAsync(studentId, quizExerciseId, new SubmitRequest
        {
            Answers =
            [
                new AnswerDto { QuestionId = q1.Id, Selected = [0] },
                new AnswerDto { QuestionId = q2.Id, Selected = [0, 1] }
            ]
        }, CancellationToken.None);

        Assert.True(submitQuizRes.IsSuccess, submitQuizRes.ErrorMessage);
        Assert.Equal(quizDetail.Value.Questions.Sum(q => q.Points), submitQuizRes.Value!.Score);
        Assert.Equal(20, submitQuizRes.Value.Score); // 20/20 raw points (100% tỷ lệ)
        Assert.True(submitQuizRes.Value.Passed);

        // 3.5 Kiểm tra lộ trình sau khi hoàn thành Node 2:
        // Tiến độ = 67% (2/3), Node 3 (Lab) TỰ ĐỘNG CHUYỂN SANG "active"!
        var mapRes3 = await gamification.GetLearningPathAsync(studentId, path.Id, CancellationToken.None);
        Assert.Equal(67, mapRes3.Value!.ProgressPct);
        Assert.Equal("passed", mapRes3.Value.Nodes.First(n => n.Id == theoryNodeId).Status);
        Assert.Equal("passed", mapRes3.Value.Nodes.First(n => n.Id == quizNodeId).Status);
        Assert.Equal("active", mapRes3.Value.Nodes.First(n => n.Id == labNodeId).Status);

        // 3.6 Học viên vào làm Node 3 (Lab)
        var enterLabRes = await gamification.EnterNodeAsync(studentId, path.Id, labNodeId, null, CancellationToken.None);
        Assert.True(enterLabRes.IsSuccess);

        // Nộp code giải thuật Lab (server judge Jint tự chấm máy chủ)
        var studentCode = "function searchBST(arr, val) { return arr.includes(val); }";
        var submitCodeRes = await exercises.SubmitCodeAsync(studentId, labExerciseId, new CodeSubmitRequest
        {
            ExerciseId = labExerciseId,
            Code = studentCode
        }, CancellationToken.None);

        Assert.True(submitCodeRes.IsSuccess, submitCodeRes.ErrorMessage);
        Assert.Equal(2, submitCodeRes.Value!.Passed);
        Assert.Equal(2, submitCodeRes.Value.Total);

        // 3.7 Kiểm tra kết thúc khóa học:
        // Tiến độ = 100% (3/3), tất cả node đều "passed", tổng XP tăng đầy đủ (50 + 100 + 100 = 250 XP)
        var mapResFinal = await gamification.GetLearningPathAsync(studentId, path.Id, CancellationToken.None);
        Assert.Equal(100, mapResFinal.Value!.ProgressPct);
        Assert.All(mapResFinal.Value.Nodes, n => Assert.Equal("passed", n.Status));

        var finalUser = await db.Users.AsNoTracking().FirstAsync(u => u.Id == studentId);
        Assert.Equal(250, finalUser.Xp);
    }
}
