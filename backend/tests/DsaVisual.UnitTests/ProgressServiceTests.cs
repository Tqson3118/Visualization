using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test ProgressService.GetMyOverviewAsync — tái hiện bug 500 khi ≥2 user học cùng 1 lesson:
/// LoadCountsAsync query UserProgress KHÔNG filter theo userId → ToDictionary(p => p.LessonId)
/// ném ArgumentException (duplicate key). Fix: query thêm điều kiện p.UserId == userId.
/// </summary>
public class ProgressServiceTests
{
    private readonly TestServices.FixedClock _clock = new();

    private async Task<(ProgressService Service, AppDbContext Db)> SetupAsync(string dbName)
    {
        var db = TestServices.CreateInMemoryDb(dbName);
        db.Topics.Add(new Topic { Id = 1, Name = "Sắp xếp", CreatedBy = 1, CreatedAt = _clock.UtcNow });
        db.Lessons.AddRange(
            new Lesson
            {
                Id = 1,
                TopicId = 1,
                Title = "Bubble Sort",
                ContentHtml = "<p>nội dung</p>",
                Status = LessonStatus.Active,
                CreatedBy = 1,
                CreatedAt = _clock.UtcNow
            },
            new Lesson
            {
                Id = 2,
                TopicId = 1,
                Title = "Quick Sort",
                ContentHtml = "<p>nội dung</p>",
                Status = LessonStatus.Active,
                CreatedBy = 1,
                CreatedAt = _clock.UtcNow
            });
        db.Users.AddRange(
            new User
            {
                Id = 1,
                Email = "student1@university.edu.vn",
                PasswordHash = "x",
                DisplayName = "Student 1",
                CreatedAt = _clock.UtcNow
            },
            new User
            {
                Id = 2,
                Email = "student2@university.edu.vn",
                PasswordHash = "x",
                DisplayName = "Student 2",
                CreatedAt = _clock.UtcNow
            });
        await db.SaveChangesAsync();

        return (new ProgressService(db, _clock), db);
    }

    [Fact]
    public async Task GetMyOverview_TwoUsersSameLesson_NoDuplicateKey()
    {
        // Tái hiện bug: 2 user có UserProgress CÙNG LessonId → trước fix ToDictionary ném ArgumentException → 500
        var (service, db) = await SetupAsync(nameof(GetMyOverview_TwoUsersSameLesson_NoDuplicateKey));
        db.UserProgress.AddRange(
            new UserProgress { Id = 1, UserId = 1, LessonId = 1, Viewed = true, UpdatedAt = _clock.UtcNow },
            new UserProgress { Id = 2, UserId = 2, LessonId = 1, Viewed = true, UpdatedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var result1 = await service.GetMyOverviewAsync(1, CancellationToken.None);
        Assert.True(result1.IsSuccess, result1.ErrorMessage);
        Assert.Equal(1, result1.Value!.LessonsViewed);

        var result2 = await service.GetMyOverviewAsync(2, CancellationToken.None);
        Assert.True(result2.IsSuccess, result2.ErrorMessage);
        Assert.Equal(1, result2.Value!.LessonsViewed);
    }

    [Fact]
    public async Task GetMyOverview_UserWithoutProgress_ReturnsSuccessWithZeroViewed()
    {
        var (service, db) = await SetupAsync(nameof(GetMyOverview_UserWithoutProgress_ReturnsSuccessWithZeroViewed));
        db.UserProgress.Add(new UserProgress { Id = 1, UserId = 2, LessonId = 1, Viewed = true, UpdatedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var result = await service.GetMyOverviewAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(0, result.Value!.LessonsViewed);
        Assert.Equal(2, result.Value.LessonsTotal);
        Assert.All(result.Value.Topics, t => Assert.All(t.Lessons, l => Assert.False(l.Viewed)));
    }

    [Fact]
    public async Task GetMyOverview_DoesNotIncludeOtherUsersProgress()
    {
        // Progress của user2 (cùng lesson + lesson khác) KHÔNG được tính vào overview của user1
        var (service, db) = await SetupAsync(nameof(GetMyOverview_DoesNotIncludeOtherUsersProgress));
        db.UserProgress.AddRange(
            new UserProgress { Id = 1, UserId = 1, LessonId = 1, Viewed = true, BestScore = 8, CompletedAt = _clock.UtcNow, UpdatedAt = _clock.UtcNow },
            new UserProgress { Id = 2, UserId = 2, LessonId = 1, Viewed = true, BestScore = 10, CompletedAt = _clock.UtcNow, UpdatedAt = _clock.UtcNow },
            new UserProgress { Id = 3, UserId = 2, LessonId = 2, Viewed = true, BestScore = 9, UpdatedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var result = await service.GetMyOverviewAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Value!.LessonsViewed);       // chỉ lesson 1 của user1, không bị inflate bởi user2
        Assert.Equal(8, result.Value.AvgScore);             // BestScore của user2 không lẫn vào
        var lesson1 = result.Value.Topics.Single().Lessons.Single(l => l.Id == 1);
        Assert.True(lesson1.Viewed);
        Assert.Equal(8, lesson1.BestScore);                 // progressByLesson chỉ chứa progress của user1
        var lesson2 = result.Value.Topics.Single().Lessons.Single(l => l.Id == 2);
        Assert.False(lesson2.Viewed);                       // user2 xem lesson 2 — user1 phải thấy chưa xem
    }
}
