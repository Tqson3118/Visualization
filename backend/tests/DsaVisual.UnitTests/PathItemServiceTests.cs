using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DsaVisual.UnitTests;

public sealed class PathItemServiceTests
{
    private readonly TestServices.FixedClock _clock = new();

    private async Task<(PathItemService Service, AppDbContext Db, int TeacherId, int PathId)> SetupAsync(string dbName)
    {
        var db = TestServices.CreateInMemoryDb(dbName);
        var teacherId = 10;
        db.Users.Add(new User { Id = teacherId, Email = "teacher@dsa.local", PasswordHash = "hash", DisplayName = "Teacher", CreatedAt = _clock.UtcNow });
        db.Topics.Add(new Topic { Id = 1, Name = "Topic 1", CreatedBy = teacherId, CreatedAt = _clock.UtcNow });

        var path = new LearningPath
        {
            Id = 1,
            Title = "Lộ trình Cấu trúc dữ liệu",
            Description = "Mô tả lộ trình",
            TopicId = 1,
            CreatedBy = teacherId,
            AuthorId = teacherId,
            Status = LearningPathStatus.Draft,
            Visibility = PathVisibility.Private
        };
        db.LearningPaths.Add(path);
        await db.SaveChangesAsync();

        var service = new PathItemService(db, _clock, NullLogger<PathItemService>.Instance);
        return (service, db, teacherId, path.Id);
    }

    [Fact]
    public async Task CreateItem_Folder_CreatesSuccessfully()
    {
        var (service, db, teacherId, pathId) = await SetupAsync(nameof(CreateItem_Folder_CreatesSuccessfully));

        var req = new PathItemCreateRequest
        {
            ItemType = PathItemType.Folder,
            Title = "Chương 1: Mảng & Chuỗi",
            Description = "Kiến thức cơ bản"
        };

        var result = await service.CreateItemAsync(teacherId, "TEACHER", pathId, req, CancellationToken.None);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("Chương 1: Mảng & Chuỗi", result.Value!.Title);
        Assert.Equal(PathItemType.Folder, result.Value.ItemType);
        Assert.Null(result.Value.LessonId);
        Assert.Null(result.Value.FinalTestId);
        Assert.Null(result.Value.LabExerciseId);

        var node = await db.LearningPathNodes.FirstOrDefaultAsync(n => n.Id == result.Value.Id);
        Assert.NotNull(node);
        Assert.Equal(PathItemType.Folder, node.ItemType);
    }

    [Fact]
    public async Task CreateItem_Theory_CreatesLessonWhenNotProvided()
    {
        var (service, db, teacherId, pathId) = await SetupAsync(nameof(CreateItem_Theory_CreatesLessonWhenNotProvided));

        var req = new PathItemCreateRequest
        {
            ItemType = PathItemType.Theory,
            Title = "Bài 1: Giới thiệu mảng",
            Description = "Lý thuyết mảng động"
        };

        var result = await service.CreateItemAsync(teacherId, "TEACHER", pathId, req, CancellationToken.None);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(PathItemType.Theory, result.Value!.ItemType);
        Assert.NotNull(result.Value.LessonId);

        var createdLesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == result.Value.LessonId);
        Assert.NotNull(createdLesson);
        Assert.Equal("Bài 1: Giới thiệu mảng", createdLesson.Title);
        Assert.Equal(LessonStatus.Active, createdLesson.Status);
    }

    [Fact]
    public async Task CreateItem_Quiz_CreatesContainerLessonAndExercise()
    {
        var (service, db, teacherId, pathId) = await SetupAsync(nameof(CreateItem_Quiz_CreatesContainerLessonAndExercise));

        var req = new PathItemCreateRequest
        {
            ItemType = PathItemType.Quiz,
            Title = "Quiz 1: Trắc nghiệm mảng",
            Description = "5 câu trắc nghiệm"
        };

        var result = await service.CreateItemAsync(teacherId, "TEACHER", pathId, req, CancellationToken.None);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(PathItemType.Quiz, result.Value!.ItemType);
        Assert.NotNull(result.Value.FinalTestId);
        Assert.NotNull(result.Value.ExerciseId);

        var createdExercise = await db.Exercises.FirstOrDefaultAsync(e => e.Id == result.Value.FinalTestId);
        Assert.NotNull(createdExercise);
        Assert.Equal(ExerciseType.Mcq, createdExercise.Type);
        Assert.Equal(result.Value.Id, createdExercise.NodeId);

        var containerLesson = await db.Lessons.FirstOrDefaultAsync(l => l.Id == createdExercise.LessonId);
        Assert.NotNull(containerLesson);
        Assert.Equal(LessonStatus.Hidden, containerLesson.Status);
    }

    [Fact]
    public async Task CreateItem_Lab_CreatesContainerLessonAndCodeExercise()
    {
        var (service, db, teacherId, pathId) = await SetupAsync(nameof(CreateItem_Lab_CreatesContainerLessonAndCodeExercise));

        var req = new PathItemCreateRequest
        {
            ItemType = PathItemType.Lab,
            Title = "Lab 1: Đảo ngược mảng",
            Description = "Codelab thuật toán"
        };

        var result = await service.CreateItemAsync(teacherId, "TEACHER", pathId, req, CancellationToken.None);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(PathItemType.Lab, result.Value!.ItemType);
        Assert.NotNull(result.Value.LabExerciseId);
        Assert.NotNull(result.Value.ExerciseId);

        var createdExercise = await db.Exercises.FirstOrDefaultAsync(e => e.Id == result.Value.LabExerciseId);
        Assert.NotNull(createdExercise);
        Assert.Equal(ExerciseType.Code, createdExercise.Type);
        Assert.Equal(result.Value.Id, createdExercise.NodeId);
    }

    [Fact]
    public async Task CreateItem_UnderNonFolder_FailsValidation()
    {
        var (service, _, teacherId, pathId) = await SetupAsync(nameof(CreateItem_UnderNonFolder_FailsValidation));

        var theoryRes = await service.CreateItemAsync(teacherId, "TEACHER", pathId, new PathItemCreateRequest
        {
            ItemType = PathItemType.Theory,
            Title = "Lý thuyết gốc"
        }, CancellationToken.None);
        Assert.True(theoryRes.IsSuccess);

        var childRes = await service.CreateItemAsync(teacherId, "TEACHER", pathId, new PathItemCreateRequest
        {
            ItemType = PathItemType.Theory,
            Title = "Mục con không hợp lệ",
            ParentId = theoryRes.Value!.Id
        }, CancellationToken.None);

        Assert.False(childRes.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, childRes.ErrorCode);
    }

    [Fact]
    public async Task GetTree_ReturnsNestedHierarchy()
    {
        var (service, _, teacherId, pathId) = await SetupAsync(nameof(GetTree_ReturnsNestedHierarchy));

        var f1 = await service.CreateItemAsync(teacherId, "TEACHER", pathId, new PathItemCreateRequest
        {
            ItemType = PathItemType.Folder,
            Title = "Chương 1"
        }, CancellationToken.None);

        var t1 = await service.CreateItemAsync(teacherId, "TEACHER", pathId, new PathItemCreateRequest
        {
            ItemType = PathItemType.Theory,
            Title = "Bài 1.1",
            ParentId = f1.Value!.Id
        }, CancellationToken.None);

        var q1 = await service.CreateItemAsync(teacherId, "TEACHER", pathId, new PathItemCreateRequest
        {
            ItemType = PathItemType.Quiz,
            Title = "Quiz 1.1",
            ParentId = f1.Value.Id
        }, CancellationToken.None);

        var treeRes = await service.GetTreeAsync(teacherId, "TEACHER", pathId, CancellationToken.None);
        Assert.True(treeRes.IsSuccess);
        Assert.Single(treeRes.Value!);
        Assert.Equal("Chương 1", treeRes.Value![0].Title);
        Assert.Equal(2, treeRes.Value[0].Children.Count);
        Assert.Equal("Bài 1.1", treeRes.Value[0].Children[0].Title);
        Assert.Equal("Quiz 1.1", treeRes.Value[0].Children[1].Title);
    }

    [Fact]
    public async Task MoveItem_UnderSelfOrDescendant_FailsCyclePrevention()
    {
        var (service, _, teacherId, pathId) = await SetupAsync(nameof(MoveItem_UnderSelfOrDescendant_FailsCyclePrevention));

        var f1 = await service.CreateItemAsync(teacherId, "TEACHER", pathId, new PathItemCreateRequest
        {
            ItemType = PathItemType.Folder,
            Title = "Folder 1"
        }, CancellationToken.None);

        var f2 = await service.CreateItemAsync(teacherId, "TEACHER", pathId, new PathItemCreateRequest
        {
            ItemType = PathItemType.Folder,
            Title = "Folder 2 (con của Folder 1)",
            ParentId = f1.Value!.Id
        }, CancellationToken.None);

        // Di chuyển f1 vào chính nó -> lỗi
        var moveSelf = await service.MoveItemAsync(teacherId, "TEACHER", f1.Value.Id, new PathItemMoveRequest
        {
            ParentId = f1.Value.Id,
            SortOrder = 1
        }, CancellationToken.None);
        Assert.False(moveSelf.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, moveSelf.ErrorCode);

        // Di chuyển f1 vào f2 (con của f1) -> lỗi cycle
        var moveCycle = await service.MoveItemAsync(teacherId, "TEACHER", f1.Value.Id, new PathItemMoveRequest
        {
            ParentId = f2.Value!.Id,
            SortOrder = 1
        }, CancellationToken.None);
        Assert.False(moveCycle.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, moveCycle.ErrorCode);
    }

    [Fact]
    public async Task DeleteItem_CascadesSubtree()
    {
        var (service, db, teacherId, pathId) = await SetupAsync(nameof(DeleteItem_CascadesSubtree));

        var f1 = await service.CreateItemAsync(teacherId, "TEACHER", pathId, new PathItemCreateRequest
        {
            ItemType = PathItemType.Folder,
            Title = "Folder To Delete"
        }, CancellationToken.None);

        var t1 = await service.CreateItemAsync(teacherId, "TEACHER", pathId, new PathItemCreateRequest
        {
            ItemType = PathItemType.Theory,
            Title = "Theory Inside",
            ParentId = f1.Value!.Id
        }, CancellationToken.None);

        var q1 = await service.CreateItemAsync(teacherId, "TEACHER", pathId, new PathItemCreateRequest
        {
            ItemType = PathItemType.Quiz,
            Title = "Quiz Inside",
            ParentId = f1.Value.Id
        }, CancellationToken.None);

        var delRes = await service.DeleteItemAsync(teacherId, "TEACHER", f1.Value.Id, CancellationToken.None);
        Assert.True(delRes.IsSuccess);

        var remainingNodes = await db.LearningPathNodes.Where(n => n.PathId == pathId).ToListAsync();
        Assert.Empty(remainingNodes);
    }

    // ── FIX security#1: GET tree/detail yêu cầu Teacher sở hữu lộ trình / Admin ──

    [Fact]
    public async Task GetTree_NonOwnerTeacher_ReturnsForbidden()
    {
        var (service, _, teacherId, pathId) = await SetupAsync(nameof(GetTree_NonOwnerTeacher_ReturnsForbidden));

        var res = await service.GetTreeAsync(999, "TEACHER", pathId, CancellationToken.None);
        Assert.False(res.IsSuccess);
        Assert.Equal(ErrorCodes.FORBIDDEN, res.ErrorCode);

        var ownerRes = await service.GetTreeAsync(teacherId, "TEACHER", pathId, CancellationToken.None);
        Assert.True(ownerRes.IsSuccess, ownerRes.ErrorMessage);

        var adminRes = await service.GetTreeAsync(1, "ADMIN", pathId, CancellationToken.None);
        Assert.True(adminRes.IsSuccess, adminRes.ErrorMessage);
    }

    [Fact]
    public async Task GetTree_PathNotFound_ReturnsNotFound()
    {
        var (service, _, teacherId, _) = await SetupAsync(nameof(GetTree_PathNotFound_ReturnsNotFound));

        var res = await service.GetTreeAsync(teacherId, "TEACHER", 99999, CancellationToken.None);
        Assert.False(res.IsSuccess);
        Assert.Equal(ErrorCodes.NOT_FOUND, res.ErrorCode);
    }

    [Fact]
    public async Task GetItemDetail_NonOwnerTeacher_ReturnsForbidden()
    {
        var (service, _, teacherId, pathId) = await SetupAsync(nameof(GetItemDetail_NonOwnerTeacher_ReturnsForbidden));

        var created = await service.CreateItemAsync(teacherId, "TEACHER", pathId, new PathItemCreateRequest
        {
            ItemType = PathItemType.Folder,
            Title = "Folder đọc thử"
        }, CancellationToken.None);
        Assert.True(created.IsSuccess);

        var res = await service.GetItemDetailAsync(999, "TEACHER", created.Value!.Id, CancellationToken.None);
        Assert.False(res.IsSuccess);
        Assert.Equal(ErrorCodes.FORBIDDEN, res.ErrorCode);
    }

    [Fact]
    public async Task CanManagePath_OwnerTrueAdminTrueOtherFalse()
    {
        var (service, _, teacherId, pathId) = await SetupAsync(nameof(CanManagePath_OwnerTrueAdminTrueOtherFalse));

        Assert.True(await service.CanManagePathAsync(teacherId, "TEACHER", pathId, CancellationToken.None));
        Assert.True(await service.CanManagePathAsync(1, "ADMIN", pathId, CancellationToken.None));
        Assert.False(await service.CanManagePathAsync(999, "TEACHER", pathId, CancellationToken.None));
        Assert.False(await service.CanManagePathAsync(teacherId, "TEACHER", 99999, CancellationToken.None));
    }

    // ── FIX D6: xóa mềm node + toàn bộ cây con ──

    [Fact]
    public async Task DeleteItem_SoftDeletesSubtree_RowsRemainButHidden()
    {
        var (service, db, teacherId, pathId) = await SetupAsync(nameof(DeleteItem_SoftDeletesSubtree_RowsRemainButHidden));

        var f1 = await service.CreateItemAsync(teacherId, "TEACHER", pathId, new PathItemCreateRequest
        {
            ItemType = PathItemType.Folder,
            Title = "Folder Xóa mềm"
        }, CancellationToken.None);

        var t1 = await service.CreateItemAsync(teacherId, "TEACHER", pathId, new PathItemCreateRequest
        {
            ItemType = PathItemType.Theory,
            Title = "Theory Xóa mềm",
            ParentId = f1.Value!.Id
        }, CancellationToken.None);

        var delRes = await service.DeleteItemAsync(teacherId, "TEACHER", f1.Value.Id, CancellationToken.None);
        Assert.True(delRes.IsSuccess);

        // Query thường: global query filter ẩn node đã xóa mềm
        var visible = await db.LearningPathNodes.ToListAsync();
        Assert.Empty(visible);

        // IgnoreQueryFilters: dòng vẫn còn trong DB, DeletedAt = thời điểm xóa (không RemoveRange)
        var all = await db.LearningPathNodes.IgnoreQueryFilters().ToListAsync();
        Assert.Equal(2, all.Count);
        Assert.All(all, n => Assert.Equal(_clock.UtcNow, n.DeletedAt));
    }
}
