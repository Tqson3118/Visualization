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
/// Kiểm thử chuyên sâu toàn bộ các trường hợp biên của Lộ trình học:
/// - Case 1: Đã học 100% rồi Giáo viên thêm / xóa / sửa / đổi thứ tự node.
/// - Case 2: Học dở dang thoát ra vào lại (trong 36h vs sau 36h).
/// - Case 3: Giáo viên chuyển khóa học về Draft / Ẩn.
/// - Case 4: Mở khóa xong bỏ dở (Idle session expiration & Heart regen).
/// </summary>
public sealed class LearningPathLifecycleEdgeCaseTests
{
    private readonly TestServices.FixedClock _clock = new();

    private async Task<(GamificationService Gamification, PathItemService PathItems, AppDbContext Db, int StudentId, int TeacherId, int PathId)> SetupAsync(string dbName)
    {
        var (db, _) = TestServices.CreateSqliteDb();

        var teacherId = 10;
        var studentId = 1;

        db.Users.Add(new User
        {
            Id = teacherId,
            Email = "teacher@dsa.local",
            PasswordHash = "hash",
            DisplayName = "Teacher",
            Role = UserRole.Teacher,
            CreatedAt = _clock.UtcNow
        });

        db.Users.Add(new User
        {
            Id = studentId,
            Email = "student@dsa.local",
            PasswordHash = "hash",
            DisplayName = "Student",
            Role = UserRole.Student,
            Hearts = 10,
            HeartsMax = 10,
            LastHeartAt = _clock.UtcNow,
            Xp = 0,
            CreatedAt = _clock.UtcNow
        });

        db.Topics.Add(new Topic
        {
            Id = 1,
            Name = "Data Structures",
            CreatedBy = teacherId,
            CreatedAt = _clock.UtcNow
        });

        var path = new LearningPath
        {
            Id = 1,
            Title = "Thuật toán cơ bản",
            Description = "Lộ trình nhập môn",
            TopicId = 1,
            CreatedBy = teacherId,
            AuthorId = teacherId,
            IsActive = true,
            Status = LearningPathStatus.Active,
            Visibility = PathVisibility.Public,
            SortOrder = 1
        };
        db.LearningPaths.Add(path);

        db.LearningPathNodes.Add(new LearningPathNode
        {
            Id = 101,
            PathId = 1,
            ItemType = PathItemType.Theory,
            Title = "Node 1: Lý thuyết mảng",
            SortOrder = 1
        });

        db.LearningPathNodes.Add(new LearningPathNode
        {
            Id = 102,
            PathId = 1,
            ItemType = PathItemType.Quiz,
            Title = "Node 2: Quiz mảng",
            SortOrder = 2
        });

        await db.SaveChangesAsync();

        var gamification = TestServices.CreateGamificationService(db, _clock);
        var pathItems = new PathItemService(db, _clock, NullLogger<PathItemService>.Instance);

        return (gamification, pathItems, db, studentId, teacherId, path.Id);
    }

    [Fact]
    public async Task Case1A_CompletedPath_TeacherAddsNewNode_ProgressDrops_AndNewNodeIsUnlocked()
    {
        var (gamification, pathItems, db, studentId, teacherId, pathId) = await SetupAsync(nameof(Case1A_CompletedPath_TeacherAddsNewNode_ProgressDrops_AndNewNodeIsUnlocked));

        // 1. Học viên pass cả 2 node ban đầu (101 và 102)
        db.UserNodeProgress.Add(new UserNodeProgress { UserId = studentId, NodeId = 101, Status = 2, Stars = 3, NodeScore = 100, PassedAt = _clock.UtcNow, UpdatedAt = _clock.UtcNow });
        db.UserNodeProgress.Add(new UserNodeProgress { UserId = studentId, NodeId = 102, Status = 2, Stars = 3, NodeScore = 100, PassedAt = _clock.UtcNow, UpdatedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        // Kiểm tra tiến độ ban đầu = 100%
        var mapBefore = await gamification.GetLearningPathAsync(studentId, pathId, CancellationToken.None);
        Assert.True(mapBefore.IsSuccess);
        Assert.Equal(100, mapBefore.Value!.ProgressPct);
        Assert.Equal(2, mapBefore.Value.Nodes.Count);

        // 2. Giáo viên thêm Node 3 (SortOrder 3) vào lộ trình
        var createReq = new PathItemCreateRequest
        {
            ItemType = PathItemType.Lab,
            Title = "Node 3: Codelab đảo ngược mảng",
            SortOrder = 3
        };
        var createRes = await pathItems.CreateItemAsync(teacherId, "TEACHER", pathId, createReq, CancellationToken.None);
        Assert.True(createRes.IsSuccess);
        var node3Id = createRes.Value!.Id;

        // 3. Học viên xem lại lộ trình: Tiến độ tự động tính lại 2/3 = 67%
        var mapAfter = await gamification.GetLearningPathAsync(studentId, pathId, CancellationToken.None);
        Assert.True(mapAfter.IsSuccess);
        Assert.Equal(67, mapAfter.Value!.ProgressPct);
        Assert.Equal(3, mapAfter.Value.Nodes.Count);

        // Node 3 phải ở trạng thái "active" (được mở khóa) vì Node 2 liền trước đã pass
        var node3Dto = mapAfter.Value.Nodes.First(n => n.Id == node3Id);
        Assert.Equal("active", node3Dto.Status);

        // 4. Học viên hoàn thành Node 3 -> Tiến độ trở lại 100%
        db.UserNodeProgress.Add(new UserNodeProgress { UserId = studentId, NodeId = node3Id, Status = 2, Stars = 3, NodeScore = 100, PassedAt = _clock.UtcNow, UpdatedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var mapFinal = await gamification.GetLearningPathAsync(studentId, pathId, CancellationToken.None);
        Assert.Equal(100, mapFinal.Value!.ProgressPct);
    }

    [Fact]
    public async Task Case1B_CompletedPath_TeacherSoftDeletesNode_ProgressStays100_AndNoOrphanError()
    {
        var (gamification, pathItems, db, studentId, teacherId, pathId) = await SetupAsync(nameof(Case1B_CompletedPath_TeacherSoftDeletesNode_ProgressStays100_AndNoOrphanError));

        // Thêm node 103 và pass cả 3 node (100%)
        db.LearningPathNodes.Add(new LearningPathNode { Id = 103, PathId = pathId, ItemType = PathItemType.Theory, Title = "Node 3", SortOrder = 3 });
        db.UserNodeProgress.Add(new UserNodeProgress { UserId = studentId, NodeId = 101, Status = 2, Stars = 3, NodeScore = 100, PassedAt = _clock.UtcNow, UpdatedAt = _clock.UtcNow });
        db.UserNodeProgress.Add(new UserNodeProgress { UserId = studentId, NodeId = 102, Status = 2, Stars = 3, NodeScore = 100, PassedAt = _clock.UtcNow, UpdatedAt = _clock.UtcNow });
        db.UserNodeProgress.Add(new UserNodeProgress { UserId = studentId, NodeId = 103, Status = 2, Stars = 3, NodeScore = 100, PassedAt = _clock.UtcNow, UpdatedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        var before = await gamification.GetLearningPathAsync(studentId, pathId, CancellationToken.None);
        Assert.Equal(100, before.Value!.ProgressPct);
        Assert.Equal(3, before.Value.Nodes.Count);

        // Giáo viên xóa Node 102 (Soft delete)
        var deleteRes = await pathItems.DeleteItemAsync(teacherId, "TEACHER", 102, CancellationToken.None);
        Assert.True(deleteRes.IsSuccess);

        // Học viên xem lại lộ trình: chỉ còn 2 node (101 và 103), cả 2 đều pass -> Tiến độ vẫn 100%
        var after = await gamification.GetLearningPathAsync(studentId, pathId, CancellationToken.None);
        Assert.Equal(100, after.Value!.ProgressPct);
        Assert.Equal(2, after.Value.Nodes.Count);
        Assert.DoesNotContain(after.Value.Nodes, n => n.Id == 102);

        // Dữ liệu UserNodeProgress cho 102 vẫn còn nguyên trong DB (không bị lỗi FK hay cascade làm crash)
        var oldProgress = await db.UserNodeProgress.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.UserId == studentId && p.NodeId == 102);
        Assert.NotNull(oldProgress);
        Assert.Equal(2, oldProgress.Status);
    }

    [Fact]
    public async Task Case1C_PassedNode_ReplayIsFree_DoesNotDeductHearts()
    {
        var (gamification, _, db, studentId, _, pathId) = await SetupAsync(nameof(Case1C_PassedNode_ReplayIsFree_DoesNotDeductHearts));

        // Đánh dấu Node 101 đã pass
        db.UserNodeProgress.Add(new UserNodeProgress { UserId = studentId, NodeId = 101, Status = 2, Stars = 3, NodeScore = 100, PassedAt = _clock.UtcNow, UpdatedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        // Học viên vào lại Node 101 để ôn bài -> Không trừ tim
        var res = await gamification.EnterNodeAsync(studentId, pathId, 101, null, CancellationToken.None);
        Assert.True(res.IsSuccess);
        Assert.Equal(10, res.Value!.HeartsLeft);

        var user = await db.Users.FirstAsync(u => u.Id == studentId);
        Assert.Equal(10, user.Hearts);
    }

    [Fact]
    public async Task Case2_ResumeWithin36Hours_DoesNotDeductHeart()
    {
        var (gamification, _, db, studentId, _, pathId) = await SetupAsync(nameof(Case2_ResumeWithin36Hours_DoesNotDeductHeart));

        // Lần 1: Học viên vào Node 101 chưa pass -> Trừ 1 tim (10 -> 9), tạo session 36h
        var first = await gamification.EnterNodeAsync(studentId, pathId, 101, null, CancellationToken.None);
        Assert.True(first.IsSuccess);
        Assert.Equal(9, first.Value!.HeartsLeft);

        // Giả lập học viên thoát ra và quay lại sau 10 phút (chưa đủ 30 phút để hồi thêm tim)
        _clock.UtcNow = _clock.UtcNow.AddMinutes(10);

        var second = await gamification.EnterNodeAsync(studentId, pathId, 101, null, CancellationToken.None);
        Assert.True(second.IsSuccess);
        Assert.Equal(9, second.Value!.HeartsLeft); // Vẫn còn 9 tim, không bị trừ thêm

        var sessionsCount = await db.NodeSessions.CountAsync(s => s.UserId == studentId && s.NodeId == 101);
        Assert.Equal(1, sessionsCount);
    }

    [Fact]
    public async Task Case4_IdleSessionExpiresAfter36Hours_NextEnterDeductsHeartIfUnpassed()
    {
        var (gamification, _, db, studentId, _, pathId) = await SetupAsync(nameof(Case4_IdleSessionExpiresAfter36Hours_NextEnterDeductsHeartIfUnpassed));

        // 1. Vào học lần đầu -> Trừ 1 tim (10 -> 9), session 36h
        var first = await gamification.EnterNodeAsync(studentId, pathId, 101, null, CancellationToken.None);
        Assert.Equal(9, first.Value!.HeartsLeft);

        // 2. Học viên bỏ dở không học, 37 giờ sau quay lại
        _clock.UtcNow = _clock.UtcNow.AddHours(37);
        db.ChangeTracker.Clear();

        // Hồi tim tự nhiên: sau 37 giờ (Free user hồi 30p/tim -> full 10 tim)
        var user = await db.Users.FirstAsync(u => u.Id == studentId);
        user.Hearts = 10;
        user.LastHeartAt = _clock.UtcNow;
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();

        // 3. Học viên vào lại Node 101 (chưa pass) -> Session 36h cũ đã hết hạn -> Trừ 1 tim để gia hạn session mới
        var second = await gamification.EnterNodeAsync(studentId, pathId, 101, null, CancellationToken.None);
        Assert.True(second.IsSuccess);
        Assert.Equal(9, second.Value!.HeartsLeft);

        // Session được gia hạn mới trong tương lai
        var session = await db.NodeSessions.AsNoTracking().FirstAsync(s => s.UserId == studentId && s.NodeId == 101);
        Assert.True(session.ExpiresAt > _clock.UtcNow);
    }

    [Fact]
    public async Task Case3_PathSetToInactive_GetLearningPathReturnsNotFound()
    {
        var (gamification, _, db, studentId, _, pathId) = await SetupAsync(nameof(Case3_PathSetToInactive_GetLearningPathReturnsNotFound));

        // Giáo viên tạm ẩn lộ trình (IsActive = false)
        var path = await db.LearningPaths.FirstAsync(p => p.Id == pathId);
        path.IsActive = false;
        path.Status = LearningPathStatus.Draft;
        await db.SaveChangesAsync();

        // Học viên truy cập lộ trình -> Báo không tồn tại
        var res = await gamification.GetLearningPathAsync(studentId, pathId, CancellationToken.None);
        Assert.False(res.IsSuccess);
        Assert.Equal(ErrorCodes.NOT_FOUND, res.ErrorCode);
    }
}
