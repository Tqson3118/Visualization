using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
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

    // ── AddFeedbackAsync — POST /lessons/{id}/feedback (TEST-B-076/077, FR-7.4) ──

    private async Task SeedViewedProgressAsync(AppDbContext db)
    {
        db.UserProgress.Add(new UserProgress { UserId = 1, LessonId = 1, Viewed = true, UpdatedAt = _clock.UtcNow });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task AddFeedback_FirstCall_CreatesFeedback()
    {
        var (service, db) = await SetupAsync(nameof(AddFeedback_FirstCall_CreatesFeedback));
        await SeedViewedProgressAsync(db);

        var result = await service.AddFeedbackAsync(
            1, "STUDENT", 1, new LessonFeedbackRequest { Rating = 5, Comment = "Bài giảng hay" }, CancellationToken.None);

        // TEST-B-076: gửi feedback → upsert ContentFeedback đúng
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(1, result.Value!.LessonId);
        Assert.Equal(5, result.Value.Rating);

        var feedback = await db.ContentFeedback.AsNoTracking()
            .FirstAsync(f => f.UserId == 1 && f.LessonId == 1);
        Assert.Equal(5, feedback.Rating);
        Assert.Equal("Bài giảng hay", feedback.Comment);
        Assert.Equal(_clock.UtcNow, feedback.CreatedAt);
        Assert.Null(feedback.UpdatedAt);
    }

    [Fact]
    public async Task AddFeedback_SecondCall_Updates_NoDuplicate()
    {
        var (service, db) = await SetupAsync(nameof(AddFeedback_SecondCall_Updates_NoDuplicate));
        await SeedViewedProgressAsync(db);

        var first = await service.AddFeedbackAsync(
            1, "STUDENT", 1, new LessonFeedbackRequest { Rating = 4 }, CancellationToken.None);
        Assert.True(first.IsSuccess, first.ErrorMessage);

        _clock.UtcNow = _clock.UtcNow.AddMinutes(5);
        var second = await service.AddFeedbackAsync(
            1, "STUDENT", 1, new LessonFeedbackRequest { Rating = 5, Comment = "Cập nhật sau khi học lại" }, CancellationToken.None);

        // TEST-B-077: feedback lần 2 → KHÔNG nhân đôi bản ghi, chỉ update Rating/Comment/UpdatedAt
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(1, await db.ContentFeedback.CountAsync(f => f.UserId == 1 && f.LessonId == 1));

        var feedback = await db.ContentFeedback.AsNoTracking()
            .FirstAsync(f => f.UserId == 1 && f.LessonId == 1);
        Assert.Equal(5, feedback.Rating);
        Assert.Equal("Cập nhật sau khi học lại", feedback.Comment);
        Assert.Equal(_clock.UtcNow, feedback.UpdatedAt);
    }

    [Fact]
    public async Task AddFeedback_InvalidRating_ReturnsValidationFailed()
    {
        var (service, db) = await SetupAsync(nameof(AddFeedback_InvalidRating_ReturnsValidationFailed));
        await SeedViewedProgressAsync(db);

        var result = await service.AddFeedbackAsync(
            1, "STUDENT", 1, new LessonFeedbackRequest { Rating = 6 }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
        Assert.Equal(0, await db.ContentFeedback.CountAsync());
    }

    [Fact]
    public async Task AddFeedback_NotViewed_ReturnsForbidden()
    {
        var (service, db) = await SetupAsync(nameof(AddFeedback_NotViewed_ReturnsForbidden));

        // Chưa "Đánh dấu đã học" → 403 (FR-7.4 / v2.9 — API_REFERENCE §4.15)
        var result = await service.AddFeedbackAsync(
            1, "STUDENT", 1, new LessonFeedbackRequest { Rating = 5 }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.FORBIDDEN, result.ErrorCode);
        Assert.Equal(0, await db.ContentFeedback.CountAsync());
    }

    [Fact]
    public async Task AddFeedback_MissingLesson_ReturnsNotFound()
    {
        var (service, db) = await SetupAsync(nameof(AddFeedback_MissingLesson_ReturnsNotFound));

        var result = await service.AddFeedbackAsync(
            1, "STUDENT", 9999, new LessonFeedbackRequest { Rating = 5 }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NOT_FOUND, result.ErrorCode);
    }

    // ── Kiểm duyệt nội dung (v2.15) ──────────────────────────

    private static LessonUpsertRequest BuildUpsertRequest(int topicId, LessonStatus status, bool isClassOnly = false) => new()
    {
        TopicId = topicId,
        Title = "Bài học kiểm duyệt",
        Description = "Mô tả",
        ContentHtml = "<p>Nội dung bài học kiểm duyệt</p>",
        Status = status,
        IsClassOnly = isClassOnly,
        SortOrder = 1
    };

    [Fact]
    public async Task Create_TeacherPublic_BecomesPendingReview()
    {
        var (service, db) = await SetupAsync(nameof(Create_TeacherPublic_BecomesPendingReview));

        // Teacher đăng public (Active) → KHÔNG active trực tiếp, phải chờ Admin duyệt
        var result = await service.CreateAsync(
            1, "TEACHER", BuildUpsertRequest(1, LessonStatus.Active), CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("pendingreview", result.Value!.Status);
        Assert.Null(result.Value.PublishedAt);
        var saved = await db.Lessons.AsNoTracking().SingleAsync(l => l.Id == result.Value.Id);
        Assert.Equal(LessonStatus.PendingReview, saved.Status);
        Assert.Null(saved.PublishedAt);
    }

    [Fact]
    public async Task Create_TeacherClassOnly_BecomesActive()
    {
        var (service, db) = await SetupAsync(nameof(Create_TeacherClassOnly_BecomesActive));

        // Teacher bài IsClassOnly → Active ngay (chỉ lớp học, không cần duyệt)
        var result = await service.CreateAsync(
            1, "TEACHER", BuildUpsertRequest(1, LessonStatus.Active, isClassOnly: true), CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("active", result.Value!.Status);
        Assert.True(result.Value.IsClassOnly);
        var saved = await db.Lessons.AsNoTracking().SingleAsync(l => l.Id == result.Value.Id);
        Assert.Equal(LessonStatus.Active, saved.Status);
    }

    [Fact]
    public async Task Review_Approve_BecomesActiveWithPublishedAt()
    {
        var (service, db) = await SetupAsync(nameof(Review_Approve_BecomesActiveWithPublishedAt));
        var pending = await db.Lessons.SingleAsync(l => l.Id == 2);
        pending.Status = LessonStatus.PendingReview;
        await db.SaveChangesAsync();

        var result = await service.ReviewAsync(
            99, 2, new LessonReviewRequest { Approve = true }, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("active", result.Value!.Status);
        Assert.Equal(_clock.UtcNow, result.Value.PublishedAt);
        var saved = await db.Lessons.AsNoTracking().SingleAsync(l => l.Id == 2);
        Assert.Equal(LessonStatus.Active, saved.Status);
        Assert.Equal(_clock.UtcNow, saved.PublishedAt);
    }

    [Fact]
    public async Task Report_ShortReason_ReturnsValidationFailed()
    {
        var (service, db) = await SetupAsync(nameof(Report_ShortReason_ReturnsValidationFailed));

        // Lý do < 5 ký tự → VALIDATION_FAILED, KHÔNG tạo BugReport
        var result = await service.ReportAsync(1, 1, new LessonReportRequest { Reason = "abc" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
        Assert.Equal(0, await db.BugReports.CountAsync());
    }

    // ── RejectionReason mapping — GET /lessons list shows Admin rejection reason (v2.15) ──

    [Fact]
    public async Task GetList_AsAdmin_MapsRejectionReason()
    {
        var (service, db) = await SetupAsync(nameof(GetList_AsAdmin_MapsRejectionReason));

        // Bài bị từ chối duyệt — có RejectionReason (như hồ sơ Draft thật)
        db.Lessons.Add(new Lesson
        {
            Id = 99,
            TopicId = 1,
            Title = "Bài bị từ chối",
            ContentHtml = "<p>x</p>",
            Status = LessonStatus.Draft,
            RejectionReason = "Thiếu hình minh họa sắp xếp",
            CreatedBy = 1,
            CreatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await service.GetListAsync(
            userId: 1, role: "ADMIN", topicId: null, status: "draft",
            q: "Bài bị từ chối", page: 1, pageSize: 20, ct: CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        var item = Assert.Single(result.Value.Items);
        Assert.Equal("Thiếu hình minh họa sắp xếp", item.RejectionReason);
    }

    [Fact]
    public async Task GetPendingList_MapsRejectionReason()
    {
        var (service, db) = await SetupAsync(nameof(GetPendingList_MapsRejectionReason));

        // Bài PendingReview có RejectionReason legacy (từ lần từ chối trước)
        db.Lessons.Add(new Lesson
        {
            Id = 98,
            TopicId = 1,
            Title = "Bài chờ duyệt lại",
            ContentHtml = "<p>x</p>",
            Status = LessonStatus.PendingReview,
            RejectionReason = "Bổ sung sau khi từ chối",
            CreatedBy = 1,
            CreatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await service.GetPendingListAsync(CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotNull(result.Value);
        var item = Assert.Single(result.Value);
        Assert.Equal("Bổ sung sau khi từ chối", item.RejectionReason);
    }
}
