using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Xunit;

namespace DsaVisual.UnitTests;

public class LessonAccessPolicyTests
{
    private readonly TestServices.FixedClock _clock = new();

    private async Task<(LessonService Service, AppDbContext Db)> SetupAsync(string dbName)
    {
        var db = TestServices.CreateInMemoryDb(dbName);

        db.Topics.Add(new Topic { Id = 1, Name = "Topic 1", CreatedBy = 1, CreatedAt = _clock.UtcNow });

        db.Lessons.Add(new Lesson
        {
            Id = 1,
            TopicId = 1,
            Title = "Active Public Lesson",
            ContentHtml = "<p>Nội dung công khai</p>",
            Status = LessonStatus.Active,
            IsClassOnly = false,
            CreatedBy = 10,
            CreatedAt = _clock.UtcNow
        });

        db.Lessons.Add(new Lesson
        {
            Id = 2,
            TopicId = 1,
            Title = "Draft Lesson",
            ContentHtml = "<p>Nội dung nháp</p>",
            Status = LessonStatus.Draft,
            IsClassOnly = false,
            CreatedBy = 10,
            CreatedAt = _clock.UtcNow
        });

        db.Lessons.Add(new Lesson
        {
            Id = 3,
            TopicId = 1,
            Title = "Pending Review Lesson",
            ContentHtml = "<p>Nội dung chờ duyệt</p>",
            Status = LessonStatus.PendingReview,
            IsClassOnly = false,
            CreatedBy = 10,
            CreatedAt = _clock.UtcNow
        });

        db.Lessons.Add(new Lesson
        {
            Id = 4,
            TopicId = 1,
            Title = "Class Only Lesson",
            ContentHtml = "<p>Nội dung cho lớp</p>",
            Status = LessonStatus.Active,
            IsClassOnly = true,
            CreatedBy = 10,
            CreatedAt = _clock.UtcNow
        });

        await db.SaveChangesAsync();

        var service = TestServices.CreateLessonService(db, _clock);
        return (service, db);
    }

    [Fact]
    public async Task GetLesson_StudentAccessActive_ReturnsSuccess()
    {
        var (service, _) = await SetupAsync(nameof(GetLesson_StudentAccessActive_ReturnsSuccess));

        var result = await service.GetByIdAsync(1, "STUDENT", 1, includeContent: true, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Active Public Lesson", result.Value!.Title);
    }

    [Theory]
    [InlineData(2)] // Draft
    [InlineData(3)] // PendingReview
    public async Task GetLesson_StudentAccessNonActive_Returns404(int lessonId)
    {
        var (service, _) = await SetupAsync($"{nameof(GetLesson_StudentAccessNonActive_Returns404)}_{lessonId}");

        var result = await service.GetByIdAsync(1, "STUDENT", lessonId, includeContent: true, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NOT_FOUND, result.ErrorCode);
    }

    [Fact]
    public async Task GetLesson_StudentAccessClassOnly_Returns404()
    {
        var (service, _) = await SetupAsync(nameof(GetLesson_StudentAccessClassOnly_Returns404));

        var result = await service.GetByIdAsync(1, "STUDENT", 4, includeContent: true, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NOT_FOUND, result.ErrorCode);
    }

    [Theory]
    [InlineData("ADMIN")]
    [InlineData("TEACHER")]
    public async Task GetLesson_TeacherOrAdminAccessDraftAndClassOnly_ReturnsSuccess(string role)
    {
        var (service, _) = await SetupAsync($"{nameof(GetLesson_TeacherOrAdminAccessDraftAndClassOnly_ReturnsSuccess)}_{role}");

        var draftRes = await service.GetByIdAsync(10, role, 2, includeContent: true, CancellationToken.None);
        Assert.True(draftRes.IsSuccess);

        var classOnlyRes = await service.GetByIdAsync(10, role, 4, includeContent: true, CancellationToken.None);
        Assert.True(classOnlyRes.IsSuccess);
    }
}
