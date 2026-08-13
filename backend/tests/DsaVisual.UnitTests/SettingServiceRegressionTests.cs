using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;

namespace DsaVisual.UnitTests;

/// <summary>
/// TEST TÁI HIỆN (đỏ/fail) cho các lỗi SettingService
/// (nguồn: docs/work/backend-audit/findings-biz-services.md #17 TRUNG, #18 THẤP).
/// Mỗi test assert HÀNH VI ĐÚNG dự kiến sau fix — trước fix PHẢI FAIL (trừ 2 test bảo vệ #17b).
/// KHÔNG sửa code production — chỉ test.
/// </summary>
public class SettingServiceRegressionTests
{
    private static SettingService CreateService(AppDbContext db, SettingsCache? cache = null) =>
        new(db, cache ?? new SettingsCache(), new TestServices.FixedClock(), NullLogger<SettingService>.Instance);

    // ── #18 (THẤP): UpdateAsync upsert cache TRƯỚC SaveChanges → cache ≠ DB khi save fail ──

    /// <summary>
    /// Bug: UpdateAsync (SettingService.cs:73-102) gọi cache.Upsert (dòng 92) TRƯỚC
    /// db.SaveChangesAsync (dòng 102). Nếu save thất bại (khóa/network/DB) → cache đã có giá trị mới
    /// nhưng DB giữ giá trị cũ → cache ≠ DB cho tới khi restart.
    /// Tái hiện deterministic: SaveChangesInterceptor ném exception ngay tại điểm save — không cần DB thật,
    /// không phụ thuộc timing.
    /// Đúng sau fix: cache.Upsert chỉ chạy SAU SaveChangesAsync thành công → cache giữ giá trị cũ.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_SaveChangesFails_CacheKeepsOldValue()
    {
        const string dbName = nameof(UpdateAsync_SaveChangesFails_CacheKeepsOldValue);
        var seedDb = TestServices.CreateInMemoryDb(dbName);
        seedDb.Settings.Add(new Setting { Key = "site.name", Value = "Cũ" });
        await seedDb.SaveChangesAsync();

        var cache = new SettingsCache();
        var serviceDb = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .AddInterceptors(new ThrowingSaveChangesInterceptor())
            .Options);
        var service = CreateService(serviceDb, cache);

        // Cache đã nạp giá trị CŨ từ DB
        Assert.Equal("Cũ", await service.GetValueAsync("site.name", CancellationToken.None));

        // SaveChanges thất bại → UpdateAsync phải throw; cache KHÔNG được upsert giá trị mới
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.UpdateAsync(1, new SystemSettingsDto
        {
            SiteName = "Mới"
        }, CancellationToken.None));

        // FAIL trước fix: cache đã bị upsert "Mới" dù DB không ghi được
        Assert.Equal("Cũ", cache.Get("site.name"));

        // DB vẫn giữ giá trị cũ (context mới, không có interceptor ném lỗi)
        var verifyDb = TestServices.CreateInMemoryDb(dbName);
        Assert.Equal("Cũ", (await verifyDb.Settings.SingleAsync(s => s.Key == "site.name")).Value);
    }

    // ── #17a (TRUNG): EnsureLoadedAsync race — 2 request miss đồng thời cùng load DB + SetAll ──

    /// <summary>
    /// Bug: EnsureLoadedAsync (SettingService.cs:126-135) double-check KHÔNG khóa — 2 request miss đồng thời
    /// cùng đọc DB + cùng SetAll (và nếu PUT xen giữa lúc A đọc DB và A SetAll → A ghi đè cache bằng dữ liệu cũ).
    /// Tái hiện deterministic: 2 instance (context + SQLite DB riêng — giống 2 request thật) dùng CHUNG 1
    /// SettingsCache (singleton); materialization interceptor chặn query giữa chừng (gate) để ép cả 2 task cùng
    /// vào pha load, đồng thời đếm số lần entity được materialize từ DB.
    /// Đúng sau fix (async lock TRONG SettingsCache — singleton dùng chung mọi request): đúng 1 lần load DB.
    /// </summary>
    [Fact]
    public async Task GetValueAsync_ConcurrentMisses_LoadsDatabaseExactlyOnce()
    {
        var gate = new QueryGate();
        var interceptor = new GatedMaterializationInterceptor(gate);
        var db1 = await CreateSeededSqliteDbAsync(interceptor);
        var db2 = await CreateSeededSqliteDbAsync(interceptor);
        // Cùng cache singleton (như DI: AddSingleton<SettingsCache>) nhưng service/context riêng (như AddScoped)
        var cache = new SettingsCache();
        var service1 = CreateService(db1, cache);
        var service2 = CreateService(db2, cache);

        gate.Arm(); // từ giờ: query nào materialize sẽ chặn tới khi Release — ép cả 2 task cùng vào pha load
        var task1 = Task.Run(() => service1.GetValueAsync("site.name", CancellationToken.None));
        var task2 = Task.Run(() => service2.GetValueAsync("site.name", CancellationToken.None));

        try
        {
            // Chờ task đầu kịp vào gate (đang load DB, chưa SetAll) — task sau lúc này cũng đã vượt double-check
            await WaitUntilAsync(() => gate.MaterializedCount >= 1, TimeSpan.FromSeconds(5));
            await Task.Delay(100);
        }
        finally
        {
            gate.Release(); // luôn mở cổng — tránh task treo nếu assert giữa chừng fail
        }

        var values = await Task.WhenAll(task1, task2); // cả 2 KHÔNG được lỗi (bug: task 2 lỗi "second operation")
        Assert.Equal("DSA Visual", values[0]);
        Assert.Equal("DSA Visual", values[1]);

        // Chỉ 1 lần load DB — FAIL trước fix: cả 2 task cùng vượt double-check → 2 lần load
        Assert.Equal(1, gate.MaterializedCount);
    }

    private static async Task<AppDbContext> CreateSeededSqliteDbAsync(IMaterializationInterceptor interceptor)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .AddInterceptors(interceptor)
            .Options);
        await db.Database.EnsureCreatedAsync();
        // Setting.UpdatedBy là FK → Users.Id (SettingConfiguration) — SQLite enforce FK nên seed user trước
        var admin = new User { Email = $"{Guid.NewGuid():N}@test.local", PasswordHash = "x", DisplayName = "Admin" };
        db.Users.Add(admin);
        await db.SaveChangesAsync();
        db.Settings.Add(new Setting { Key = "site.name", Value = "DSA Visual", UpdatedBy = admin.Id });
        await db.SaveChangesAsync();
        return db;
    }

    // ── #17b (TRUNG): cache in-process per-instance → multi-instance stale ──

    /// <summary>
    /// BẢO VỆ (xanh cả trước và sau fix): UpdateAsync PHẢI ghi DB thật — một "instance mới"
    /// (SettingsCache mới, chưa từng nạp) đọc lại từ DB phải thấy giá trị mới.
    /// Nếu fix tương lai chỉ ghi cache mà quên DB thì test này đỏ.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_PersistsToDb_FreshInstanceReadsNewValue()
    {
        const string dbName = nameof(UpdateAsync_PersistsToDb_FreshInstanceReadsNewValue);
        var dbA = TestServices.CreateInMemoryDb(dbName);
        dbA.Settings.Add(new Setting { Key = "site.name", Value = "Cũ" });
        await dbA.SaveChangesAsync();

        var cacheA = new SettingsCache();
        var serviceA = CreateService(dbA, cacheA);

        var result = await serviceA.UpdateAsync(1, new SystemSettingsDto { SiteName = "Mới" }, CancellationToken.None);
        Assert.True(result.IsSuccess);

        // DB ghi thật
        var verifyDb = TestServices.CreateInMemoryDb(dbName);
        Assert.Equal("Mới", (await verifyDb.Settings.SingleAsync(s => s.Key == "site.name")).Value);

        // "Instance B" — cache mới hoàn toàn — đọc từ DB thấy giá trị mới
        var serviceB = CreateService(TestServices.CreateInMemoryDb(dbName), new SettingsCache());
        Assert.Equal("Mới", await serviceB.GetValueAsync("site.name", CancellationToken.None));
    }

    /// <summary>
    /// GIỚI HẠN ĐÃ CHẤP NHẬN (ghi chú single-instance — như SubmissionLockRegistry): instance B đã nạp cache
    /// từ TRƯỚC khi A update thì B trả giá trị CŨ (stale) vô thời hạn — cache in-process per-instance,
    /// UpdateAsync chỉ upsert cache của instance gọi nó. Đồ án chạy 1 instance nên chấp nhận (biz#17b).
    /// Test này PIN hành vi hiện tại — khi triển khai invalidation (Redis pub/sub / DB version stamp)
    /// phải sửa test này + xóa giới hạn.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_OtherInstanceWithWarmCache_StaysStale_DocumentedSingleInstanceLimit()
    {
        const string dbName = nameof(UpdateAsync_OtherInstanceWithWarmCache_StaysStale_DocumentedSingleInstanceLimit);
        var dbA = TestServices.CreateInMemoryDb(dbName);
        dbA.Settings.Add(new Setting { Key = "site.name", Value = "Cũ" });
        await dbA.SaveChangesAsync();

        var serviceA = CreateService(dbA, new SettingsCache());
        var cacheB = new SettingsCache();
        var serviceB = CreateService(TestServices.CreateInMemoryDb(dbName), cacheB);

        // B nạp cache TRƯỚC (thấy giá trị cũ)
        Assert.Equal("Cũ", await serviceB.GetValueAsync("site.name", CancellationToken.None));
        // A update (DB + cache A)
        await serviceA.UpdateAsync(1, new SystemSettingsDto { SiteName = "Mới" }, CancellationToken.None);
        // B vẫn stale — giới hạn single-instance đã ghi nhận (comment SettingsCache/SettingService + notes.md)
        Assert.Equal("Cũ", await serviceB.GetValueAsync("site.name", CancellationToken.None));
    }

    // ── Helpers ──

    private static async Task WaitUntilAsync(Func<bool> condition, TimeSpan timeout)
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        while (!condition() && sw.Elapsed < timeout)
        {
            await Task.Delay(10);
        }

        Assert.True(condition(), $"Điều kiện không thỏa trong {timeout}");
    }

    /// <summary>Ném lỗi ngay tại điểm SaveChanges — mô phỏng DB fail (biz#18).</summary>
    private sealed class ThrowingSaveChangesInterceptor : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result) =>
            throw new InvalidOperationException("Simulated SaveChanges failure (biz#18)");

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Simulated SaveChanges failure (biz#18)");
    }

    /// <summary>Cổng chặn + đếm số entity materialize từ DB — ép race deterministic (biz#17a).</summary>
    private sealed class QueryGate
    {
        private TaskCompletionSource _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public int MaterializedCount;

        public void Arm() => _tcs = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        public void BlockUntilReleased() => _tcs.Task.GetAwaiter().GetResult();

        public void Release() => _tcs.TrySetResult();
    }

    private sealed class GatedMaterializationInterceptor(QueryGate gate) : IMaterializationInterceptor
    {
        public object InitializedInstance(MaterializationInterceptionData materializationData, object entity)
        {
            Interlocked.Increment(ref gate.MaterializedCount);
            gate.BlockUntilReleased();
            return entity;
        }
    }
}
