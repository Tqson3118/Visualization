using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test ClassService.JoinByCodeAsync (v2.15 — Vấn đề 14/4.1): tham gia lớp bằng mã mời,
/// không cần biết classId trước. Mã sai → NOT_FOUND; lớp đóng / đã tham gia → fail.
/// </summary>
public class ClassServiceTests
{
    private readonly TestServices.FixedClock _clock = new();

    private async Task<(ClassService Service, AppDbContext Db)> SetupAsync(string dbName)
    {
        var db = TestServices.CreateInMemoryDb(dbName);

        db.Users.Add(new User
        {
            Id = 1,
            Email = "student@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Student",
            CreatedAt = _clock.UtcNow
        });
        db.Users.Add(new User
        {
            Id = 2,
            Email = "teacher@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Teacher",
            CreatedAt = _clock.UtcNow
        });
        db.Classes.Add(new Class
        {
            Id = 1,
            Name = "CTDL-GT K17",
            InviteCode = "ABC123",
            OwnerId = 2,
            Status = ClassStatus.Open,
            CreatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();

        return (TestServices.CreateClassService(db, _clock), db);
    }

    [Fact]
    public async Task JoinByCode_Success_AddsMember()
    {
        var (service, db) = await SetupAsync(nameof(JoinByCode_Success_AddsMember));

        var result = await service.JoinByCodeAsync(1, "STUDENT", new JoinClassByCodeRequest { InviteCode = "abc123" }, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Value!.Id);
        Assert.Equal("ABC123", result.Value.InviteCode);
        Assert.Contains(result.Value.Members, m => m.UserId == 1);
        Assert.Equal(1, await db.ClassMembers.CountAsync(m => m.ClassId == 1 && m.UserId == 1));
    }

    [Fact]
    public async Task JoinByCode_UnknownCode_ReturnsNotFound()
    {
        var (service, db) = await SetupAsync(nameof(JoinByCode_UnknownCode_ReturnsNotFound));

        var result = await service.JoinByCodeAsync(1, "STUDENT", new JoinClassByCodeRequest { InviteCode = "ZZZ999" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NOT_FOUND, result.ErrorCode);
        Assert.Equal(0, await db.ClassMembers.CountAsync());
    }

    [Fact]
    public async Task JoinByCode_ClosedClass_ReturnsValidationFailed()
    {
        var (service, db) = await SetupAsync(nameof(JoinByCode_ClosedClass_ReturnsValidationFailed));
        var classRoom = await db.Classes.SingleAsync(c => c.Id == 1);
        classRoom.Status = ClassStatus.Closed;
        await db.SaveChangesAsync();

        var result = await service.JoinByCodeAsync(1, "STUDENT", new JoinClassByCodeRequest { InviteCode = "ABC123" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
        Assert.Equal(0, await db.ClassMembers.CountAsync());
    }

    // ── FIX D5: gán lộ trình theo Visibility (Status Active/ClassOnly không còn miễn trừ) ──

    [Fact]
    public async Task SetLearningPath_PrivatePathByOwner_AutoUpgradesToClassOnly()
    {
        var (service, db) = await SetupAsync(nameof(SetLearningPath_PrivatePathByOwner_AutoUpgradesToClassOnly));
        db.LearningPaths.Add(new LearningPath
        {
            Id = 10,
            Title = "Lộ trình riêng của tôi",
            CreatedBy = 2,
            AuthorId = 2,
            Status = LearningPathStatus.Draft,
            Visibility = PathVisibility.Private
        });
        await db.SaveChangesAsync();

        var result = await service.SetLearningPathAsync(2, "TEACHER", 1, 10, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var path = await db.LearningPaths.AsNoTracking().SingleAsync(p => p.Id == 10);
        Assert.Equal(PathVisibility.ClassOnly, path.Visibility);   // tự nâng Private → ClassOnly
        var classRoom = await db.Classes.AsNoTracking().SingleAsync(c => c.Id == 1);
        Assert.Equal(10, classRoom.LearningPathId);
    }

    [Fact]
    public async Task SetLearningPath_PrivatePathOfOtherTeacher_ActiveStatusNoLongerBypasses()
    {
        var (service, db) = await SetupAsync(nameof(SetLearningPath_PrivatePathOfOtherTeacher_ActiveStatusNoLongerBypasses));
        db.LearningPaths.Add(new LearningPath
        {
            Id = 11,
            Title = "Lộ trình riêng GV khác",
            CreatedBy = 999,
            AuthorId = 999,
            Status = LearningPathStatus.Active,   // D5: Status Active không còn là "free pass"
            Visibility = PathVisibility.Private
        });
        await db.SaveChangesAsync();

        var result = await service.SetLearningPathAsync(2, "TEACHER", 1, 11, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.FORBIDDEN, result.ErrorCode);
        var path = await db.LearningPaths.AsNoTracking().SingleAsync(p => p.Id == 11);
        Assert.Equal(PathVisibility.Private, path.Visibility);
        var classRoom = await db.Classes.AsNoTracking().SingleAsync(c => c.Id == 1);
        Assert.Null(classRoom.LearningPathId);
    }

    [Fact]
    public async Task SetLearningPath_PrivatePathByAdmin_AllowedAndUpgraded()
    {
        var (service, db) = await SetupAsync(nameof(SetLearningPath_PrivatePathByAdmin_AllowedAndUpgraded));
        db.LearningPaths.Add(new LearningPath
        {
            Id = 12,
            Title = "Lộ trình riêng do Admin gán",
            CreatedBy = 999,
            AuthorId = 999,
            Status = LearningPathStatus.Draft,
            Visibility = PathVisibility.Private
        });
        await db.SaveChangesAsync();

        var result = await service.SetLearningPathAsync(1, "ADMIN", 1, 12, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var path = await db.LearningPaths.AsNoTracking().SingleAsync(p => p.Id == 12);
        Assert.Equal(PathVisibility.ClassOnly, path.Visibility);
    }

    [Fact]
    public async Task SetLearningPath_PublicPathByAnyTeacher_Allowed()
    {
        var (service, db) = await SetupAsync(nameof(SetLearningPath_PublicPathByAnyTeacher_Allowed));
        db.LearningPaths.Add(new LearningPath
        {
            Id = 13,
            Title = "Lộ trình công khai",
            CreatedBy = 999,
            AuthorId = 999,
            Status = LearningPathStatus.Draft,
            Visibility = PathVisibility.Public
        });
        await db.SaveChangesAsync();

        var result = await service.SetLearningPathAsync(2, "TEACHER", 1, 13, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var path = await db.LearningPaths.AsNoTracking().SingleAsync(p => p.Id == 13);
        Assert.Equal(PathVisibility.Public, path.Visibility);   // không bị đổi visibility
    }

    // ── FIX D6/index: UNIQUE (ClassId, PathItemId) khi PathItemId != NULL (SQLite enforce thật) ──

    [Fact]
    public async Task ClassAssignment_DuplicateNodeInSameClass_UniqueIndexRejects()
    {
        var (db, connection) = TestServices.CreateSqliteDb();
        try
        {
            db.Users.Add(new User { Id = 1, Email = "owner@dsa.local", PasswordHash = "x", DisplayName = "Owner", CreatedAt = _clock.UtcNow });
            db.Classes.Add(new Class { Id = 1, Name = "Lớp A", InviteCode = "UNQA1", OwnerId = 1, Status = ClassStatus.Open, CreatedAt = _clock.UtcNow });
            db.Classes.Add(new Class { Id = 2, Name = "Lớp B", InviteCode = "UNQB2", OwnerId = 1, Status = ClassStatus.Open, CreatedAt = _clock.UtcNow });
            db.Topics.Add(new Topic { Id = 1, Name = "Topic", CreatedBy = 1, CreatedAt = _clock.UtcNow });
            db.LearningPaths.Add(new LearningPath { Id = 1, Title = "Path", CreatedBy = 1, AuthorId = 1 });
            db.LearningPathNodes.Add(new LearningPathNode { Id = 5, PathId = 1, ItemType = PathItemType.Theory, Title = "Node 5", SortOrder = 1 });
            db.Lessons.Add(new Lesson { Id = 1, TopicId = 1, Title = "Lesson 1", ContentHtml = "", Status = LessonStatus.Active, CreatedBy = 1, CreatedAt = _clock.UtcNow });
            db.Lessons.Add(new Lesson { Id = 2, TopicId = 1, Title = "Lesson 2", ContentHtml = "", Status = LessonStatus.Active, CreatedBy = 1, CreatedAt = _clock.UtcNow });
            await db.SaveChangesAsync();

            // Gán cùng node cho 2 lớp khác nhau + gán lesson thuần (PathItemId = NULL) → hợp lệ
            db.ClassAssignments.Add(new ClassAssignment { ClassId = 1, PathItemId = 5, SortOrder = 1, CreatedAt = _clock.UtcNow });
            db.ClassAssignments.Add(new ClassAssignment { ClassId = 2, PathItemId = 5, SortOrder = 1, CreatedAt = _clock.UtcNow });
            db.ClassAssignments.Add(new ClassAssignment { ClassId = 1, PathItemId = null, LessonId = 1, SortOrder = 2, CreatedAt = _clock.UtcNow });
            db.ClassAssignments.Add(new ClassAssignment { ClassId = 1, PathItemId = null, LessonId = 2, SortOrder = 3, CreatedAt = _clock.UtcNow });
            await db.SaveChangesAsync();

            // Trùng (ClassId, PathItemId) trong cùng lớp → vi phạm unique filtered index
            db.ClassAssignments.Add(new ClassAssignment { ClassId = 1, PathItemId = 5, SortOrder = 9, CreatedAt = _clock.UtcNow });
            var ex = await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
            var message = ex.InnerException?.Message ?? ex.Message;
            Assert.Contains("UNIQUE constraint failed", message, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("ClassAssignments", message, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            await db.DisposeAsync();
            connection.Dispose();
        }
    }

    [Fact]
    public async Task JoinByCode_AlreadyMember_ReturnsValidationFailed()
    {
        var (service, db) = await SetupAsync(nameof(JoinByCode_AlreadyMember_ReturnsValidationFailed));
        db.ClassMembers.Add(new ClassMember { ClassId = 1, UserId = 1, JoinedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var result = await service.JoinByCodeAsync(1, "STUDENT", new JoinClassByCodeRequest { InviteCode = "ABC123" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
        Assert.Equal(1, await db.ClassMembers.CountAsync(m => m.UserId == 1));
    }
}
