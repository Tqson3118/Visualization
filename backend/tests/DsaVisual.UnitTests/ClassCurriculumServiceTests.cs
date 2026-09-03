using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test Lộ trình học (Curriculum) per-class: sortOrder, draft/publish gating, student status
/// từ dữ liệu progress THẬT (UserProgress/ExerciseSubmission), permission owner/member.
/// </summary>
public sealed class ClassCurriculumServiceTests
{
    private readonly TestServices.FixedClock _clock = new();

    private async Task<(ClassService Service, AppDbContext Db, int TeacherId, int StudentId)> SetupAsync(
        string dbName, int teacherId = 2, int studentId = 1)
    {
        var db = TestServices.CreateInMemoryDb(dbName);
        db.Users.Add(new User { Id = teacherId, Email = $"t{teacherId}@university.edu.vn", PasswordHash = "x", DisplayName = "Teacher", CreatedAt = _clock.UtcNow });
        db.Users.Add(new User { Id = studentId, Email = $"s{studentId}@university.edu.vn", PasswordHash = "x", DisplayName = "Student", CreatedAt = _clock.UtcNow });
        db.Topics.Add(new Topic { Id = 1, Name = "Chủ đề", CreatedBy = teacherId, CreatedAt = _clock.UtcNow });
        db.Lessons.Add(new Lesson { Id = 1, TopicId = 1, Title = "Bài học A", ContentHtml = "<p>a</p>", Status = LessonStatus.Active, CreatedBy = teacherId, CreatedAt = _clock.UtcNow });
        db.Lessons.Add(new Lesson { Id = 2, TopicId = 1, Title = "Bài học B", ContentHtml = "<p>b</p>", Status = LessonStatus.Active, CreatedBy = teacherId, CreatedAt = _clock.UtcNow });
        db.Exercises.Add(new Exercise { Id = 1, LessonId = 1, Title = "Quiz 1", Type = ExerciseType.Mcq, MaxScore = 10, Status = ExerciseStatus.Active, CreatedBy = teacherId, CreatedAt = _clock.UtcNow });
        db.Classes.Add(new Class { Id = 1, Name = "Lớp DSA", InviteCode = "ABC123", OwnerId = teacherId, Status = ClassStatus.Open, CreatedAt = _clock.UtcNow });
        await db.SaveChangesAsync();
        return (TestServices.CreateClassService(db, _clock), db, teacherId, studentId);
    }

    private async Task<int> AddAssignmentAsync(ClassService service, int teacherId, int classId, int? lessonId, int? exerciseId)
    {
        var result = await service.AddAssignmentAsync(teacherId, RoleNames.Teacher, classId,
            new ClassAssignmentUpsertRequest { LessonId = lessonId, ExerciseId = exerciseId }, CancellationToken.None);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!.Assignments.Single(a => (a.LessonId ?? 0) == (lessonId ?? 0) && (a.ExerciseId ?? 0) == (exerciseId ?? 0)).Id;
    }

    [Fact]
    public async Task AddAssignment_AppendsSortOrder()
    {
        var (service, db, teacher, _) = await SetupAsync(nameof(AddAssignment_AppendsSortOrder));
        await AddAssignmentAsync(service, teacher, 1, 1, null);
        await AddAssignmentAsync(service, teacher, 1, null, 1);

        var rows = await db.ClassAssignments.OrderBy(a => a.SortOrder).ToListAsync();
        Assert.Equal(2, rows.Count);
        Assert.Equal(0, rows[0].SortOrder);
        Assert.Equal(1, rows[1].SortOrder);
        Assert.Equal(1, rows[0].LessonId);
        Assert.Equal(1, rows[1].ExerciseId);
    }

    [Fact]
    public async Task ReorderCurriculum_PersistsOrderAfterRefresh()
    {
        var (service, db, teacher, _) = await SetupAsync(nameof(ReorderCurriculum_PersistsOrderAfterRefresh));
        var a1 = await AddAssignmentAsync(service, teacher, 1, 1, null);
        var a2 = await AddAssignmentAsync(service, teacher, 1, 2, null);
        var a3 = await AddAssignmentAsync(service, teacher, 1, null, 1);

        // Đảo ngược thứ tự
        var reorder = await service.ReorderCurriculumAsync(teacher, RoleNames.Teacher, 1,
            new ClassCurriculumReorderRequest
            {
                Items = [new() { AssignmentId = a3, SortOrder = 0 }, new() { AssignmentId = a2, SortOrder = 1 }, new() { AssignmentId = a1, SortOrder = 2 }]
            },
            CancellationToken.None);
        Assert.True(reorder.IsSuccess, reorder.ErrorMessage);

        // Refresh → thứ tự vẫn đúng (bài gán mới phải đứng sau theo sortOrder mới)
        var detail = await service.GetByIdAsync(teacher, RoleNames.Teacher, 1, CancellationToken.None);
        Assert.True(detail.IsSuccess, detail.ErrorMessage);
        var ids = detail.Value!.Assignments.Select(a => a.Id).ToList();
        Assert.Equal([a3, a2, a1], ids);
    }

    [Fact]
    public async Task UpdateCurriculum_MetaAndPublish_GatesStudentView()
    {
        var (service, db, teacher, student) = await SetupAsync(nameof(UpdateCurriculum_MetaAndPublish_GatesStudentView));
        await AddAssignmentAsync(service, teacher, 1, 1, null);
        await AddAssignmentAsync(service, teacher, 1, null, 1);
        db.ClassMembers.Add(new ClassMember { ClassId = 1, UserId = student, JoinedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        // Save draft (unpublish)
        var draft = await service.UpdateCurriculumAsync(teacher, RoleNames.Teacher, 1,
            new ClassCurriculumUpsertRequest { Title = "Graph cơ bản", Description = "Des", Published = false }, CancellationToken.None);
        Assert.True(draft.IsSuccess, draft.ErrorMessage);
        Assert.False(draft.Value!.CurriculumPublished);
        Assert.Equal("Graph cơ bản", draft.Value.CurriculumTitle);

        // Student: không thấy items khi draft
        var studentView = await service.GetCurriculumAsync(student, RoleNames.Student, 1, CancellationToken.None);
        Assert.True(studentView.IsSuccess, studentView.ErrorMessage);
        Assert.False(studentView.Value!.Published);
        Assert.Empty(studentView.Value.Items);

        // Save draft không làm mất item (teacher vẫn thấy)
        var teacherView = await service.GetCurriculumAsync(teacher, RoleNames.Teacher, 1, CancellationToken.None);
        Assert.True(teacherView.IsSuccess, teacherView.ErrorMessage);
        Assert.Equal(2, teacherView.Value!.Items.Count);

        // Publish → student thấy items
        var pub = await service.UpdateCurriculumAsync(teacher, RoleNames.Teacher, 1,
            new ClassCurriculumUpsertRequest { Published = true }, CancellationToken.None);
        Assert.True(pub.IsSuccess, pub.ErrorMessage);
        Assert.True(pub.Value!.CurriculumPublished);

        var publishedView = await service.GetCurriculumAsync(student, RoleNames.Student, 1, CancellationToken.None);
        Assert.True(publishedView.IsSuccess, publishedView.ErrorMessage);
        Assert.True(publishedView.Value!.Published);
        Assert.Equal(2, publishedView.Value.Items.Count);
    }

    [Fact]
    public async Task GetCurriculum_StudentStatuses_FromRealProgress()
    {
        var (service, db, teacher, student) = await SetupAsync(nameof(GetCurriculum_StudentStatuses_FromRealProgress));
        await AddAssignmentAsync(service, teacher, 1, 1, null);   // lesson A
        var quizAssignmentId = await AddAssignmentAsync(service, teacher, 1, null, 1);   // quiz 1
        await AddAssignmentAsync(service, teacher, 1, 2, null);   // lesson B
        db.ClassMembers.Add(new ClassMember { ClassId = 1, UserId = student, JoinedAt = _clock.UtcNow });

        // Progress THẬT: lesson A completed, quiz 1 passed, lesson B only viewed
        var cls1 = await db.Classes.FindAsync(1);
        cls1!.CurriculumPublished = true;
        db.UserProgress.Add(new UserProgress { UserId = student, LessonId = 1, Viewed = true, BestScore = 90, CompletedAt = _clock.UtcNow, UpdatedAt = _clock.UtcNow });
        db.UserProgress.Add(new UserProgress { UserId = student, LessonId = 2, Viewed = true, BestScore = null, UpdatedAt = _clock.UtcNow });
        db.ExerciseSubmissions.Add(new ExerciseSubmission { UserId = student, ExerciseId = 1, ClassAssignmentId = quizAssignmentId, Score = 9, SubmittedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var view = await service.GetCurriculumAsync(student, RoleNames.Student, 1, CancellationToken.None);
        Assert.True(view.IsSuccess, view.ErrorMessage);

        var items = view.Value!.Items.OrderBy(i => i.SortOrder).ToList();
        Assert.Equal(3, items.Count);
        Assert.Equal("completed", items[0].Status);   // lesson A
        Assert.Equal("completed", items[1].Status);   // quiz 1
        Assert.Equal("in_progress", items[2].Status); // lesson B viewed nhưng chưa xong → item đang dở đầu tiên
        Assert.Equal(67, view.Value.ProgressPct);
        Assert.Equal(90, items[0].BestScore);
    }

    [Fact]
    public async Task GetCurriculum_NotStarted_WhenNoProgress()
    {
        var (service, db, teacher, student) = await SetupAsync(nameof(GetCurriculum_NotStarted_WhenNoProgress));
        await AddAssignmentAsync(service, teacher, 1, 1, null);
        await AddAssignmentAsync(service, teacher, 1, null, 1);
        var cls2 = await db.Classes.FindAsync(1);
        cls2!.CurriculumPublished = true;
        db.ClassMembers.Add(new ClassMember { ClassId = 1, UserId = student, JoinedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var view = await service.GetCurriculumAsync(student, RoleNames.Student, 1, CancellationToken.None);
        var items = view.Value!.Items.OrderBy(i => i.SortOrder).ToList();
        Assert.Equal("in_progress", items[0].Status);  // item đầu chưa làm = đang dở
        Assert.Equal("not_started", items[1].Status);
        Assert.Equal(0, view.Value.ProgressPct);
    }

    [Fact]
    public async Task UpdateCurriculum_NonOwnerTeacher_Forbidden()
    {
        var (service, db, teacher, _) = await SetupAsync(nameof(UpdateCurriculum_NonOwnerTeacher_Forbidden));
        // Teacher khác (id 3) không phải owner
        db.Users.Add(new User { Id = 3, Email = "other@university.edu.vn", PasswordHash = "x", DisplayName = "Other", CreatedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var result = await service.UpdateCurriculumAsync(3, RoleNames.Teacher, 1,
            new ClassCurriculumUpsertRequest { Title = "X", Published = true }, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.FORBIDDEN, result.ErrorCode);
    }

    [Fact]
    public async Task ReorderCurriculum_NonOwner_Forbidden()
    {
        var (service, db, teacher, _) = await SetupAsync(nameof(ReorderCurriculum_NonOwner_Forbidden));
        await AddAssignmentAsync(service, teacher, 1, 1, null);
        db.Users.Add(new User { Id = 3, Email = "other@university.edu.vn", PasswordHash = "x", DisplayName = "Other", CreatedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var result = await service.ReorderCurriculumAsync(3, RoleNames.Teacher, 1,
            new ClassCurriculumReorderRequest { Items = [new() { AssignmentId = 1, SortOrder = 0 }] }, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.FORBIDDEN, result.ErrorCode);
    }

    [Fact]
    public async Task GetCurriculum_NonMember_Forbidden()
    {
        var (service, db, teacher, _) = await SetupAsync(nameof(GetCurriculum_NonMember_Forbidden));
        await AddAssignmentAsync(service, teacher, 1, 1, null);

        // Student id 9 chưa tham gia lớp
        var result = await service.GetCurriculumAsync(9, RoleNames.Student, 1, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.FORBIDDEN, result.ErrorCode);
    }

    [Fact]
    public async Task GetCurriculum_LearningPath_ReturnsConfiguredXpReward()
    {
        var (service, db, teacher, student) = await SetupAsync(nameof(GetCurriculum_LearningPath_ReturnsConfiguredXpReward));

        var path = new LearningPath
        {
            Id = 100,
            Title = "Path DSA",
            CreatedBy = teacher,
            TopicId = 1,
            Status = LearningPathStatus.Active
        };
        db.LearningPaths.Add(path);

        var nodeFolder = new LearningPathNode { Id = 101, PathId = 100, ItemType = PathItemType.Folder, Title = "Chương 1", SortOrder = 1 };
        var nodeTheory = new LearningPathNode { Id = 102, PathId = 100, ParentId = 101, ItemType = PathItemType.Theory, Title = "Bài 1", SortOrder = 2, LessonId = 1 };
        var nodeQuiz = new LearningPathNode { Id = 103, PathId = 100, ParentId = 101, ItemType = PathItemType.Quiz, Title = "Quiz 1", SortOrder = 3, FinalTestId = 1 };
        var nodeLab = new LearningPathNode { Id = 104, PathId = 100, ParentId = 101, ItemType = PathItemType.Lab, Title = "Lab 1", SortOrder = 4, LabExerciseId = 2 };
        db.LearningPathNodes.AddRange(nodeFolder, nodeTheory, nodeQuiz, nodeLab);

        var classRoom = await db.Classes.FirstAsync(c => c.Id == 1);
        classRoom.LearningPathId = 100;
        classRoom.CurriculumPublished = true;
        db.ClassMembers.Add(new ClassMember { ClassId = 1, UserId = student, JoinedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var result = await service.GetCurriculumAsync(student, RoleNames.Student, 1, CancellationToken.None);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Single(result.Value!.Items);
        var folder = result.Value.Items[0];
        Assert.Equal(0, folder.XpReward);
        Assert.Equal(3, folder.Children.Count);

        var theoryItem = folder.Children.First(c => c.PathItemId == 102);
        Assert.Equal(50, theoryItem.XpReward);

        var quizItem = folder.Children.First(c => c.PathItemId == 103);
        Assert.Equal(50, quizItem.XpReward);

        var labItem = folder.Children.First(c => c.PathItemId == 104);
        Assert.Equal(100, labItem.XpReward);
    }

    [Fact]
    public async Task GetReport_HierarchicalSort_And_LaggingLearnerSingleMissing()
    {
        var (service, db, teacher, student) = await SetupAsync(nameof(GetReport_HierarchicalSort_And_LaggingLearnerSingleMissing));

        var path = new LearningPath
        {
            Id = 200,
            Title = "Path Multi-Chapter",
            CreatedBy = teacher,
            TopicId = 1,
            Status = LearningPathStatus.Active
        };
        db.LearningPaths.Add(path);

        // Chương 1 (SortOrder 1) có Bài 1.1 (SortOrder 1)
        var chap1 = new LearningPathNode { Id = 201, PathId = 200, ItemType = PathItemType.Folder, Title = "Chương 1", SortOrder = 1 };
        var les1 = new LearningPathNode { Id = 202, PathId = 200, ParentId = 201, ItemType = PathItemType.Theory, Title = "Bài 1.1", SortOrder = 1, LessonId = 1 };

        // Chương 2 (SortOrder 2) có Bài 2.1 (SortOrder 1)
        var chap2 = new LearningPathNode { Id = 203, PathId = 200, ItemType = PathItemType.Folder, Title = "Chương 2", SortOrder = 2 };
        var les2 = new LearningPathNode { Id = 204, PathId = 200, ParentId = 203, ItemType = PathItemType.Theory, Title = "Bài 2.1", SortOrder = 1 };

        db.LearningPathNodes.AddRange(chap1, les1, chap2, les2);

        var classRoom = await db.Classes.FirstAsync(c => c.Id == 1);
        classRoom.LearningPathId = 200;
        classRoom.CurriculumPublished = true;
        db.ClassMembers.Add(new ClassMember { ClassId = 1, UserId = student, JoinedAt = _clock.UtcNow });

        // Bài 1.1 có deadline trong quá khứ (đã quá hạn)
        db.ClassAssignments.Add(new ClassAssignment
        {
            ClassId = 1,
            PathItemId = 202,
            DueAt = _clock.UtcNow.AddHours(-2),
            AllowLateSubmission = true
        });

        await db.SaveChangesAsync();

        var result = await service.GetReportAsync(teacher, RoleNames.Teacher, 1, CancellationToken.None);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var report = result.Value!;

        // 1. Kiểm tra sắp xếp theo thứ tự phân cấp cây (Chương 1 trước rồi tới Chương 2)
        Assert.Equal(2, report.Assignments.Count);
        Assert.Equal("Bài 1.1", report.Assignments[0].Title);
        Assert.Equal("Chương 1", report.Assignments[0].ModuleName);
        Assert.Equal("Bài 2.1", report.Assignments[1].Title);
        Assert.Equal("Chương 2", report.Assignments[1].ModuleName);

        // 2. Học viên chưa học Bài 1.1 (quá hạn) -> Phải hiển thị trong LaggingLearners dù chỉ thiếu 1 bài
        Assert.Single(report.LaggingLearners);
        Assert.Equal(student, report.LaggingLearners[0].UserId);
        Assert.Equal(1, report.LaggingLearners[0].MissingCount);
    }
}
