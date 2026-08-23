using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.UnitTests;

/// <summary>
/// TEST TÁI HIỆN cho finding exc#3 (docs/work/backend-audit/findings-exception.md #3):
/// GamificationService.EnterNodeAsync (GamificationService.cs:156-176) `catch (DbUpdateException)` nuốt
/// TẤT CẢ lỗi DB — chỉ nên bắt unique violation (SqlException 2627/2601), lỗi khác phải lan ra.
/// - Green (giữ nguyên): duplicate session → resume 200 OK — hành vi đúng hiện tại, phải giữ sau fix.
/// - Đỏ: lỗi DB KHÔNG phải unique → KHÔNG được nuốt — hiện bị nuốt thành resume OK.
/// Dùng SQLite in-memory (service cần ExecuteSqlInterpolatedAsync — InMemory không hỗ trợ raw SQL).
/// </summary>
public class GamificationEnterNodeExceptionTests
{
    private readonly TestServices.FixedClock _clock = new();

    private async Task<(GamificationService Service, AppDbContext Db)> SetupAsync(string dbName)
    {
        var (db, _) = TestServices.CreateSqliteDb();

        db.Users.Add(new User
        {
            Id = 1,
            Email = "repro-enter@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Repro",
            Hearts = 10,
            HeartsMax = 10,
            LastHeartAt = _clock.UtcNow,
            CreatedAt = _clock.UtcNow
        });
        db.LearningPaths.Add(new LearningPath { Id = 1, Title = "Sắp xếp & Tìm kiếm", IsActive = true, CreatedBy = 1 });
        db.LearningPathNodes.Add(new LearningPathNode { Id = 10, PathId = 1, Title = "Bubble Sort", SortOrder = 1 });
        await db.SaveChangesAsync();

        var service = TestServices.CreateGamificationService(db, _clock);
        return (service, db);
    }

    /// <summary>
    /// Green — hành vi ĐÚNG phải giữ nguyên: session còn hiệu lực → INSERT trùng (UNIQUE UserId,NodeId)
    /// → resume session cũ 200 OK, KHÔNG trừ tim lần 2.
    /// </summary>
    [Fact]
    public async Task EnterNode_Twice_SecondCallResumesExistingSession()
    {
        var (service, _) = await SetupAsync(nameof(EnterNode_Twice_SecondCallResumesExistingSession));

        var first = await service.EnterNodeAsync(1, 1, 10, null, CancellationToken.None);
        Assert.True(first.IsSuccess, first.ErrorMessage);

        var second = await service.EnterNodeAsync(1, 1, 10, null, CancellationToken.None);
        Assert.True(second.IsSuccess, second.ErrorMessage);
        Assert.Equal(first.Value!.Session.Id, second.Value!.Session.Id);   // resume session cũ
    }

    /// <summary>
    /// Đỏ (finding exc#3): ép INSERT NodeSessions fail bằng lỗi KHÔNG phải unique violation —
    /// bỏ unique index + trigger RAISE(ABORT) → SQLite báo SQLITE_CONSTRAINT qua SqliteException →
    /// EF bọc thành DbUpdateException. Đúng sau fix: chỉ bắt unique violation → lỗi này LAN RA
    /// (không bị nuốt thành "session đã tồn tại" như hiện tại).
    /// </summary>
    [Fact]
    public async Task EnterNode_NonUniqueDbUpdateException_NotSwallowed()
    {
        var (service, db) = await SetupAsync(nameof(EnterNode_NonUniqueDbUpdateException_NotSwallowed));

        var first = await service.EnterNodeAsync(1, 1, 10, null, CancellationToken.None);
        Assert.True(first.IsSuccess, first.ErrorMessage);

        // Lỗi DB không phải unique violation
        await db.Database.ExecuteSqlRawAsync("DROP INDEX IF EXISTS IX_NodeSessions_UserId_NodeId");
        await db.Database.ExecuteSqlRawAsync(
            "CREATE TRIGGER trg_fail_node_insert BEFORE INSERT ON NodeSessions BEGIN SELECT RAISE(ABORT, 'boom'); END");

        // Lần 2: UPDATE renew = 0 (session chưa hết hạn) → INSERT → trigger abort → DbUpdateException.
        // BUG hiện tại: catch (DbUpdateException) nuốt → resume OK; phải lan ra (finding exc#3).
        await Assert.ThrowsAsync<DbUpdateException>(() =>
            service.EnterNodeAsync(1, 1, 10, null, CancellationToken.None));
    }
}
