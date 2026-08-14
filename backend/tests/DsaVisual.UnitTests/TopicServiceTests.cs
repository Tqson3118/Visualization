using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Services;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.UnitTests;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test TopicService: cây 2 cấp, chặn xóa chủ đề có bài học (TOPIC_HAS_LESSONS), tên trùng (VALIDATION_FAILED).
/// </summary>
public class TopicServiceTests
{
    private readonly TestServices.FixedClock _clock = new();

    private async Task<(TopicService Service, AppDbContext Db)> SetupAsync(string dbName)
    {
        var db = TestServices.CreateInMemoryDb(dbName);
        db.Users.Add(new User
        {
            Id = 1,
            Email = "teacher@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Teacher",
            CreatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();

        var service = TestServices.CreateTopicService(db, _clock);
        return (service, db);
    }

    [Fact]
    public async Task CreateAndGetTree_NestedTwoLevels()
    {
        var (service, _) = await SetupAsync(nameof(CreateAndGetTree_NestedTwoLevels));

        var root = await service.CreateAsync(1, new TopicUpsertRequest { Name = "Sắp xếp", SortOrder = 1 }, CancellationToken.None);
        Assert.True(root.IsSuccess);

        var child = await service.CreateAsync(1, new TopicUpsertRequest { ParentId = root.Value!.Id, Name = "Cơ bản", SortOrder = 1 }, CancellationToken.None);
        Assert.True(child.IsSuccess);

        // Chặn cấp 3 (chỉ 2 cấp — SDD §7.3.7)
        var level3 = await service.CreateAsync(1, new TopicUpsertRequest { ParentId = child.Value!.Id, Name = "Quá sâu" }, CancellationToken.None);
        Assert.False(level3.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, level3.ErrorCode);

        var tree = await service.GetTreeAsync(CancellationToken.None);
        Assert.True(tree.IsSuccess);
        Assert.Single(tree.Value!);
        Assert.Single(tree.Value![0].Children);
    }

    [Fact]
    public async Task Create_DuplicateNameSameLevel_ReturnsValidationFailed()
    {
        var (service, _) = await SetupAsync(nameof(Create_DuplicateNameSameLevel_ReturnsValidationFailed));

        var first = await service.CreateAsync(1, new TopicUpsertRequest { Name = "Cây" }, CancellationToken.None);
        Assert.True(first.IsSuccess);

        var duplicate = await service.CreateAsync(1, new TopicUpsertRequest { Name = "cây" }, CancellationToken.None);

        Assert.False(duplicate.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, duplicate.ErrorCode);
        Assert.NotNull(duplicate.FieldErrors);
        Assert.Contains("name", duplicate.FieldErrors!.Keys);
    }

    [Fact]
    public async Task Delete_TopicWithLessons_ReturnsTopicHasLessons()
    {
        var (service, db) = await SetupAsync(nameof(Delete_TopicWithLessons_ReturnsTopicHasLessons));

        var topic = await service.CreateAsync(1, new TopicUpsertRequest { Name = "Sắp xếp" }, CancellationToken.None);
        db.Lessons.Add(new Lesson
        {
            TopicId = topic.Value!.Id,
            Title = "Bubble Sort",
            ContentHtml = "<p>nội dung</p>",
            Status = LessonStatus.Active,
            CreatedBy = 1,
            CreatedAt = _clock.UtcNow
        });
        await db.SaveChangesAsync();

        var result = await service.DeleteAsync(1, "TEACHER", topic.Value.Id, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.TOPIC_HAS_LESSONS, result.ErrorCode);
    }

    [Fact]
    public async Task Delete_EmptyTopic_Succeeds()
    {
        var (service, db) = await SetupAsync(nameof(Delete_EmptyTopic_Succeeds));

        var topic = await service.CreateAsync(1, new TopicUpsertRequest { Name = "Sắp xếp" }, CancellationToken.None);

        var result = await service.DeleteAsync(1, "TEACHER", topic.Value!.Id, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, await db.Topics.CountAsync(t => t.DeletedAt == null));
    }

    [Fact]
    public async Task Reorder_SetsSortOrderByIndex()
    {
        var (service, db) = await SetupAsync(nameof(Reorder_SetsSortOrderByIndex));

        var a = await service.CreateAsync(1, new TopicUpsertRequest { Name = "A" }, CancellationToken.None);
        var b = await service.CreateAsync(1, new TopicUpsertRequest { Name = "B" }, CancellationToken.None);

        var result = await service.ReorderAsync(new TopicReorderRequest { Ids = [b.Value!.Id, a.Value!.Id] }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var bAfter = await db.Topics.AsNoTracking().FirstAsync(t => t.Id == b.Value.Id);
        var aAfter = await db.Topics.AsNoTracking().FirstAsync(t => t.Id == a.Value.Id);
        Assert.Equal(0, bAfter.SortOrder);
        Assert.Equal(1, aAfter.SortOrder);
    }
}
