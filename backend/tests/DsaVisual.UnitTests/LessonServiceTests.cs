using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test LessonService.MarkViewedAsync — POST /lessons/{id}/mark-viewed (bug P1 SETUP_TODO §6.2).
/// TEST_PLAN TEST-B-033/034: upsert UserProgress không trùng bản ghi; lần 2 chỉ update Viewed/UpdatedAt.
/// </summary>
public class LessonServiceTests
{
    private readonly TestServices.FixedClock _clock = new();

    private async Task<(LessonService Service, AppDbContext Db)> SetupAsync(string dbName)
    {
        var db = TestServices.CreateInMemoryDb(dbName);

        db.Topics.Add(new Topic { Id = 1, Name = "Sắp xếp", CreatedBy = 1, CreatedAt = _clock.UtcNow });
        db.Lessons.Add(new Lesson
        {
            Id = 1,
            TopicId = 1,
            Title = "Bubble Sort",
            ContentHtml = "<p>nội dung</p>",
            Status = LessonStatus.Active,
            CreatedBy = 1,
            CreatedAt = _clock.UtcNow
        });
        db.Lessons.Add(new Lesson
        {
            Id = 2,
            TopicId = 1,
            Title = "Bài nháp",
            ContentHtml = "<p>nội dung nháp</p>",
            Status = LessonStatus.Draft,
            CreatedBy = 1,
            CreatedAt = _clock.UtcNow
        });
        db.Users.Add(new User
        {
            Id = 1,
            Email = "student@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Student",
            CreatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();

        return (TestServices.CreateLessonService(db, _clock), db);
    }

    [Fact]
    public async Task MarkViewed_FirstCall_CreatesUserProgress()
    {
        var (service, db) = await SetupAsync(nameof(MarkViewed_FirstCall_CreatesUserProgress));

        var result = await service.MarkViewedAsync(1, "STUDENT", 1, CancellationToken.None);

        // TEST-B-033: mark-viewed → upsert UserProgress đúng
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var progress = await db.UserProgress.AsNoTracking()
            .FirstAsync(p => p.UserId == 1 && p.LessonId == 1);
        Assert.True(progress.Viewed);
        Assert.Equal(1, await db.UserProgress.CountAsync(p => p.UserId == 1 && p.LessonId == 1));
    }

    [Fact]
    public async Task MarkViewed_SecondCall_DoesNotDuplicate()
    {
        var (service, db) = await SetupAsync(nameof(MarkViewed_SecondCall_DoesNotDuplicate));

        var first = await service.MarkViewedAsync(1, "STUDENT", 1, CancellationToken.None);
        Assert.True(first.IsSuccess);

        _clock.UtcNow = _clock.UtcNow.AddMinutes(5);
        var second = await service.MarkViewedAsync(1, "STUDENT", 1, CancellationToken.None);

        // TEST-B-034: mark-viewed lần 2 → KHÔNG nhân đôi bản ghi, vẫn 1 dòng
        Assert.True(second.IsSuccess);
        Assert.Equal(1, await db.UserProgress.CountAsync(p => p.UserId == 1 && p.LessonId == 1));

        var progress = await db.UserProgress.AsNoTracking()
            .FirstAsync(p => p.UserId == 1 && p.LessonId == 1);
        Assert.True(progress.Viewed);
        Assert.Equal(_clock.UtcNow, progress.UpdatedAt);
    }

    [Fact]
    public async Task MarkViewed_MissingLesson_ReturnsNotFound()
    {
        var (service, _) = await SetupAsync(nameof(MarkViewed_MissingLesson_ReturnsNotFound));

        var result = await service.MarkViewedAsync(1, "STUDENT", 9999, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NOT_FOUND, result.ErrorCode);
    }

    [Fact]
    public async Task MarkViewed_DraftLesson_AsStudent_ReturnsNotFound()
    {
        var (service, db) = await SetupAsync(nameof(MarkViewed_DraftLesson_AsStudent_ReturnsNotFound));

        var result = await service.MarkViewedAsync(1, "STUDENT", 2, CancellationToken.None);

        // Student không được đánh dấu bản nháp — ẩn như không tồn tại
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NOT_FOUND, result.ErrorCode);
        Assert.Equal(0, await db.UserProgress.CountAsync());
    }
}
