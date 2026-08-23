using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using DsaVisual.UnitTests;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.UnitTests;

/// <summary>
/// TEST TÁI HIỆN (đỏ/fail) cho 7 lỗi business-logic GamificationService
/// (nguồn: docs/work/backend-audit/findings-biz-gamification.md — BUG-1, #2, #3, #4, #6, #7).
/// Mỗi test assert HÀNH VI ĐÚNG dự kiến sau fix — hiện tại PHẢI FAIL vì bug chưa sửa.
/// KHÔNG sửa code production — chỉ test.
/// Dùng SQLite in-memory (shared-cache cho 2 test race mô phỏng interleave qua write-lock).
/// </summary>
public class GamificationServiceRegressionTests
{
    private readonly TestServices.FixedClock _clock = new();

    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>Setup chuẩn: user 1 + path 1 + node 10 (mở, SortOrder=1) + node 11 (khóa).</summary>
    private async Task<(GamificationService Service, AppDbContext Db)> SetupAsync(string dbName)
    {
        var (db, _) = TestServices.CreateSqliteDb();

        db.Users.Add(new User
        {
            Id = 1,
            Email = "repro@university.edu.vn",
            PasswordHash = "x",
            DisplayName = "Repro",
            Hearts = 10,
            HeartsMax = 10,
            LastHeartAt = _clock.UtcNow,
            CreatedAt = _clock.UtcNow
        });
        db.LearningPaths.Add(new LearningPath
        {
            Id = 1,
            Title = "Sắp xếp & Tìm kiếm",
            IsActive = true,
            CreatedBy = 1
        });
        db.LearningPathNodes.Add(new LearningPathNode { Id = 10, PathId = 1, Title = "Bubble Sort", SortOrder = 1 });
        db.LearningPathNodes.Add(new LearningPathNode { Id = 11, PathId = 1, Title = "Binary Search", SortOrder = 2 });
        await db.SaveChangesAsync();

        var service = TestServices.CreateGamificationService(db, _clock);
        return (service, db);
    }

    /// <summary>
    /// Tái hiện race (lost-update) bằng MANUAL INTERLEAVE deterministic trên CÙNG db:
    /// đọc stale → request thật (service) hoàn tất → request còn lại ghi đè bằng giá trị stale
    /// (đúng câu SQL service đang chạy). SQLite shared-cache không dùng được write-lock (deadlock
    /// giữa 2 connection: writer chờ RESERVED trong khi peer commit cần EXCLUSIVE) nên interleave
    /// được mô phỏng theo đúng chuỗi lệnh service thực thi khi race.
    /// Race thật (Task.WhenAll trên SQL Server) nằm ở integration GamificationRegressionTests [Trait Race].
    /// </summary>

    private static void SeedQuestPool(AppDbContext db)
    {
        db.DailyQuests.AddRange(
            new DailyQuest { Id = 1, QuestKey = "t-learn-1-node", Title = "Học 1 node", Type = 0, ConditionJson = """{"activity":"pass_node","count":1}""", RewardJson = """{"gems":3,"xp":20}""", PoolEnabled = true },
            new DailyQuest { Id = 2, QuestKey = "t-pass-1-quiz", Title = "Quiz 1", Type = 0, ConditionJson = """{"activity":"pass_quiz","count":1}""", RewardJson = """{"gems":3,"xp":20}""", PoolEnabled = true },
            new DailyQuest { Id = 3, QuestKey = "t-learn-3-node", Title = "3 node", Type = 1, ConditionJson = """{"activity":"pass_node","count":3}""", RewardJson = """{"gems":5,"xp":30}""", PoolEnabled = true },
            new DailyQuest { Id = 4, QuestKey = "t-code-run-1", Title = "Code 1", Type = 1, ConditionJson = """{"activity":"code_run","count":1}""", RewardJson = """{"gems":4,"xp":25}""", PoolEnabled = true },
            new DailyQuest { Id = 5, QuestKey = "t-code-run-5", Title = "Code 5", Type = 2, ConditionJson = """{"activity":"code_run","count":5}""", RewardJson = """{"gems":8,"xp":50}""", PoolEnabled = true });
    }

    // ── BUG-1 (CAO): Premium hết hạn → HeartsMax/Hearts phải clamp về free (10) ──────────

    /// <summary>
    /// SRS FR-10.7: hết hạn premium → HeartsMax 30→10, Hearts=MIN(Hearts,10), regen 30p/tim.
    /// Bug: HeartConfig() (GamificationService.cs:840-844) trả user.HeartsMax (DB vẫn 30) khi hết hạn —
    /// không có job downgrade (audit-notes-backend.md BUG-1).
    /// </summary>
    [Fact]
    public async Task GetHearts_ExpiredPremium_ClampsHeartsMaxAndHeartsToFree()
    {
        var (service, db) = await SetupAsync(nameof(GetHearts_ExpiredPremium_ClampsHeartsMaxAndHeartsToFree));

        // Premium hết hạn (PremiumUntil quá khứ), DB vẫn HeartsMax=30, Hearts=25
        var user = await db.Users.FirstAsync(u => u.Id == 1);
        user.Hearts = 25;
        user.HeartsMax = 30;
        user.PremiumUntil = _clock.UtcNow.AddDays(-1);
        user.LastHeartAt = _clock.UtcNow;
        await db.SaveChangesAsync();

        var result = await service.GetHeartsAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(10, result.Value!.HeartsMax);   // bug: trả 30 (HeartsMax DB chưa downgrade)
        Assert.Equal(10, result.Value!.Hearts);      // bug: trả 25 (không clamp Hearts về 10)

        var persisted = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        Assert.Equal(10, persisted.HeartsMax);       // bug: DB vẫn 30
        Assert.Equal(10, persisted.Hearts);          // bug: DB vẫn 25
    }

    // ── #2 (CAO): PersistHeartRegenAsync ghi đè tim vừa trừ (lost-update) ─────────────────

    /// <summary>
    /// Bug: PersistHeartRegenAsync (GamificationService.cs:867-868) ghi `Hearts = {computed}` KHÔNG điều kiện
    /// từ read AsNoTracking stale → race với EnterNodeAsync (trừ tim) ghi đè kết quả Hearts = Hearts - 1
    /// vừa commit → user được "trả lại" tim đã trừ.
    /// Interleave mô phỏng: GET đọc stale (Hearts=2 → sẽ ghi 3) → EnterNode thật hoàn tất (2) → GET ghi đè 3.
    /// Đúng sau fix: Hearts phải là 2 (regen không ghi đè tim vừa trừ; atomic/điều kiện).
    /// CẬP NHẬT SAU FIX: bước 3 mô phỏng câu UPDATE MỚI của service (finding #2) — delta + CASE clamp max
    /// + WHERE LastHeartAt &lt;= giá trị ĐÃ ĐỌC: EnterNode vừa đổi LastHeartAt=now → WHERE không khớp → no-op.
    /// </summary>
    [Fact]
    public async Task HeartRegen_StaleWriteAfterConcurrentDeduct_DoesNotRefundHeart()
    {
        var (service, db) = await SetupAsync(nameof(HeartRegen_StaleWriteAfterConcurrentDeduct_DoesNotRefundHeart));

        var user = await db.Users.FirstAsync(u => u.Id == 1);
        user.Hearts = 2;
        user.HeartsMax = 10;
        user.LastHeartAt = _clock.UtcNow.AddMinutes(-30);   // đủ 1 chu kỳ regen free 30p
        await db.SaveChangesAsync();

        // Bước 1 — request "GET hearts" đọc STALE (trước khi EnterNode trừ):
        // giá trị GET sẽ ghi = min(10, 2 + 30p/30p) = 3 — đúng công thức ComputeHearts/PersistHeartRegenAsync
        var staleRead = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        var staleComputedHearts = Math.Min(10, staleRead.Hearts
            + (int)((_clock.UtcNow - staleRead.LastHeartAt).TotalSeconds / 1800));
        Assert.Equal(3, staleComputedHearts);   // tiền đề: GET (stale) sẽ ghi 3

        // Bước 2 — request "EnterNode" hoàn tất (service thật): regen 2→3 rồi trừ 1 → Hearts=2
        var entered = await service.EnterNodeAsync(1, 1, 10, null, CancellationToken.None);
        Assert.True(entered.IsSuccess, entered.ErrorMessage);
        Assert.Equal(2, entered.Value!.HeartsLeft);

        // Bước 3 — request "GET" ghi regen từ STALE bằng câu UPDATE MỚI của service (finding #2 fix):
        // delta + CASE clamp max + WHERE LastHeartAt <= giá trị ĐÃ ĐỌC → EnterNode vừa đổi LastHeartAt=now
        // (regen 2→3 rồi trừ → 2) → WHERE không khớp → no-op, KHÔNG ghi đè Hearts=3 (không "trả lại tim")
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Users SET Hearts = CASE WHEN Hearts < 10 THEN Hearts + 1 ELSE Hearts END, LastHeartAt = {_clock.UtcNow} WHERE Id = 1 AND Hearts < 10 AND LastHeartAt <= {staleRead.LastHeartAt}");

        var final = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        // Serial-equivalent: GET regen (2→3) + EnterNode trừ (→2) = 2; bug: GET ghi đè 3 (trả lại tim đã trừ)
        Assert.Equal(2, final.Hearts);
        Assert.Equal(_clock.UtcNow, final.LastHeartAt);
    }

    /// <summary>Baseline invariant #2: công thức regen không bao giờ LỚN HƠN HeartsMax (clamp tại max).</summary>
    [Fact]
    public async Task HeartRegen_Formula_NeverExceedsMax()
    {
        var (service, db) = await SetupAsync(nameof(HeartRegen_Formula_NeverExceedsMax));

        // 300 phút trước = 10 chu kỳ free 30p → 2 + 10 = 12 nhưng phải clamp tại max 10
        var user = await db.Users.FirstAsync(u => u.Id == 1);
        user.Hearts = 2;
        user.LastHeartAt = _clock.UtcNow.AddMinutes(-300);
        await db.SaveChangesAsync();

        var result = await service.GetHeartsAsync(1, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.True(result.Value!.Hearts <= result.Value.HeartsMax, "regen không bao giờ vượt max");
        Assert.Equal(10, result.Value.Hearts);
        Assert.Equal(10, result.Value.HeartsMax);
    }

    // ── #4 (CAO): Shop mua — lost-update Quantity + trừ gems 2 lần cho 1 lần +1 ──────────

    /// <summary>
    /// Bug: BuyItemAsync (GamificationService.cs:494-528) maxStack check read-then-act không atomic +
    /// `inventory.Quantity += 1` (527) trên entity tracked → 2 mua song song cùng đọc Quantity=4 &lt; MaxStack 5
    /// → đều trừ gems nhưng cả 2 ghi 5 (lost-update) → trừ gems 2 lần cho 1 lần +1.
    /// Interleave mô phỏng: 2 request cùng đọc stale Quantity=4 → request A thật hoàn tất (4→5, gems −50)
    /// → request B (stale) trừ gems + ghi đè Quantity=5 (lost-update) → gems −100 nhưng chỉ +1 quantity.
    /// Đúng sau fix: request B bị CHẶN (stack đầy) → gems chỉ trừ 1 lần (150), Quantity=5 ≤ MaxStack.
    /// CẬP NHẬT SAU FIX: bước 3 mô phỏng câu SQL MỚI của service (finding #4) — 1 UPDATE atomic
    /// (Quantity + 1 WHERE Quantity &lt; MaxStack) → ROWCOUNT=0 → tồn tại → VALIDATION_FAILED → rollback hoàn gems.
    /// </summary>
    [Fact]
    public async Task BuyItem_StaleQuantityRead_DoesNotDoubleDeductGems()
    {
        var (service, db) = await SetupAsync(nameof(BuyItem_StaleQuantityRead_DoesNotDoubleDeductGems));

        var user = await db.Users.FirstAsync(u => u.Id == 1);
        user.Gems = 200;
        db.ShopItems.Add(new ShopItem { Id = 1, ItemKey = "repro-item", Name = "Item", PriceGems = 50, MaxStack = 5, Type = 0 });
        db.UserInventory.Add(new UserInventory { Id = 1, UserId = 1, ItemId = 1, Quantity = 4, IsEquipped = false, PurchasedAt = _clock.UtcNow });
        await db.SaveChangesAsync();

        // Bước 1 — 2 request mua đọc STALE cùng lúc: cả 2 thấy Quantity=4 < MaxStack 5
        var staleQty = (await db.UserInventory.AsNoTracking().FirstAsync(i => i.UserId == 1 && i.ItemId == 1)).Quantity;
        Assert.Equal(4, staleQty);   // tiền đề: cả 2 request đều vượt qua maxStack check

        // Bước 2 — request mua A hoàn tất (service thật): trừ gems 50 + Quantity 4→5
        var first = await service.BuyItemAsync(1, 1, CancellationToken.None);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(5, first.Value!.Owned);
        Assert.Equal(150, first.Value.GemsLeft);

        // Bước 3 — request mua B (read stale Quantity=4) chạy câu SQL MỚI của service (finding #4 fix):
        // trừ gems atomic + inventory 1 UPDATE atomic (Quantity + 1 WHERE Quantity < MaxStack);
        // ROWCOUNT=0 (qty đã = 5 ≥ MaxStack) → B bị chặn (VALIDATION_FAILED) → rollback → hoàn gems
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Users SET Gems = Gems - 50 WHERE Id = 1 AND Gems >= 50");
        var affectedB = await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE UserInventory SET Quantity = Quantity + 1 WHERE UserId = 1 AND ItemId = 1 AND Quantity < 5");
        Assert.Equal(0, affectedB);   // stack đầy → B không tăng quantity
        if (affectedB == 0)
        {
            await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE Users SET Gems = Gems + 50 WHERE Id = 1");   // rollback hoàn gems
        }

        var userAfter = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        var inv = await db.UserInventory.AsNoTracking().FirstAsync(i => i.UserId == 1 && i.ItemId == 1);

        // Serial-equivalent: chỉ 1 lần mua hợp lệ (4→5), gems 200−50=150; bug: trừ 2 lần → 100
        Assert.Equal(150, userAfter.Gems);
        Assert.Equal(5, inv.Quantity);                       // không vượt MaxStack
        Assert.True(inv.Quantity <= 5, "Quantity không được vượt MaxStack");
    }

    // ── #6 (CAO): Quest không bao giờ hoàn thành (không nơi nào tăng Progress) ───────────

    /// <summary>
    /// Bug: grep toàn backend KHÔNG có chỗ nào tăng UserQuests.Progress (chỉ INSERT Progress=0 tại
    /// GamificationService.cs:282; ClaimQuestAsync:338-345 đọc progress=0 &lt; target → luôn VALIDATION_FAILED).
    /// Hệ quả: XP/gems quest chết, leaderboard đứng yên (finding #6).
    /// Ghi chú thiết kế: quest "pass_node" cần cơ chế tăng Progress khi pass node (server-side atomic UPDATE
    /// trong cùng tx với hành động học, hoặc API riêng cho FE tăng — hiện KHÔNG tồn tại cơ chế nào).
    /// </summary>
    [Fact]
    public async Task QuestProgress_AfterPassingNode_IsIncremented()
    {
        var (service, db) = await SetupAsync(nameof(QuestProgress_AfterPassingNode_IsIncremented));
        SeedQuestPool(db);
        await db.SaveChangesAsync();

        var before = await service.GetQuestsAsync(1, CancellationToken.None);
        Assert.True(before.IsSuccess, before.ErrorMessage);
        Assert.Equal(5, before.Value!.Count);
        var passNodeQuest = before.Value.First(q => q.QuestId == 1);   // learn-1-node: pass_node count 1
        Assert.Equal(0, passNodeQuest.Progress);
        Assert.Equal(1, passNodeQuest.Target);

        // Hành động học tương ứng: enter node (mở khóa node — UserNodeProgress Status=1, được Add vào
        // ChangeTracker) + pass node (mô phỏng đúng flow ExerciseService.SubmitAsync → UpsertNodeProgressAsync
        // set Status=2 + SaveChanges persist — như production khi nộp bài).
        var entered = await service.EnterNodeAsync(1, 1, 10, null, CancellationToken.None);
        Assert.True(entered.IsSuccess, entered.ErrorMessage);
        var nodeProgress = db.ChangeTracker.Entries<UserNodeProgress>()
            .Select(e => e.Entity)
            .SingleOrDefault(p => p.UserId == 1 && p.NodeId == 10);
        Assert.NotNull(nodeProgress);   // EnterNode phải mở khóa node
        nodeProgress!.Status = 2;       // mô phỏng ExerciseService.UpsertNodeProgressAsync khi pass node
        // PassedAt dùng REAL UTC now (không phải FixedClock): quest hôm nay được tạo theo TodayUtc7()
        // = real clock (service dùng DateTime.UtcNow), FixedClock lệch ngày so với real clock → pass
        // không nằm trong cửa sổ "hôm nay" của quest. Giữ nguyên ý nghĩa: pass node → Progress +1.
        nodeProgress.PassedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();    // mô phỏng SaveChanges trong SubmitAsync (production persist pass node)

        var after = await service.GetQuestsAsync(1, CancellationToken.None);
        Assert.True(after.IsSuccess, after.ErrorMessage);
        var passNodeQuestAfter = after.Value!.First(q => q.QuestId == 1);

        // BUG: Progress luôn 0 — không có cơ chế tăng → assert này FAIL
        Assert.True(passNodeQuestAfter.Progress > 0,
            "Progress quest pass_node phải tăng sau khi pass node — hiện luôn 0 (finding #6: không nơi nào tăng Progress)");

        // Hệ quả trực tiếp: quest hoàn thành phải claim được (hiện VALIDATION_FAILED "Quest chưa hoàn thành")
        var claim = await service.ClaimQuestAsync(1, passNodeQuestAfter.Id, CancellationToken.None);
        Assert.True(claim.IsSuccess, $"Claim quest hoàn thành phải OK — hiện {claim.ErrorCode}: {claim.ErrorMessage}");
    }

    // ── #7 (TRUNG): mock-pay 2 lần cùng orderId → gia hạn chồng 2 lần ────────────────────

    /// <summary>
    /// Bug: MockPayAsync (GamificationService.cs:688-736) UPDATE không có `WHERE Status = 2` → gọi mock-pay
    /// 2 lần trên cùng order → lần 2 lấy baseTime = PremiumUntil hiện tại + AddMonths → gia hạn CHỒNG.
    /// Đúng sau fix: lần 2 phải no-op (order đã active) — PremiumUntil/subscription KHÔNG đổi.
    /// </summary>
    [Fact]
    public async Task MockPay_SameOrderTwice_DoesNotStackPremiumUntil()
    {
        var (service, db) = await SetupAsync(nameof(MockPay_SameOrderTwice_DoesNotStackPremiumUntil));

        var upgrade = await service.UpgradePremiumAsync(1, new PremiumUpgradeRequest { PlanId = "1m" }, CancellationToken.None);
        Assert.True(upgrade.IsSuccess, upgrade.ErrorMessage);

        var first = await service.MockPayAsync(1, new PremiumMockPayRequest { OrderId = upgrade.Value!.OrderId }, CancellationToken.None);
        Assert.True(first.IsSuccess, first.ErrorMessage);
        Assert.Equal(_clock.UtcNow.AddMonths(1), first.Value!.ExpiresAt);

        // Lần 2 cùng orderId — phải KHÔNG gia hạn thêm
        var second = await service.MockPayAsync(1, new PremiumMockPayRequest { OrderId = upgrade.Value!.OrderId }, CancellationToken.None);
        Assert.True(second.IsSuccess, second.ErrorMessage);

        // BUG: lần 2 baseTime = PremiumUntil (đã +1 tháng) → ExpiresAt = +2 tháng (chồng 2 lần)
        Assert.Equal(first.Value!.ExpiresAt, second.Value!.ExpiresAt);

        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == 1);
        Assert.Equal(first.Value!.ExpiresAt, user.PremiumUntil);   // bug: PremiumUntil tăng gấp đôi

        var sub = await db.PremiumSubscriptions.AsNoTracking().FirstAsync(s => s.Id == upgrade.Value!.OrderId);
        Assert.Equal(first.Value!.ExpiresAt, sub.ExpiresAt);       // bug: ExpiresAt bị ghi đè +1 tháng
    }

    // ── #3 (CAO): User phải có concurrency token (RowVersion) — compile-level ─────────────

    /// <summary>
    /// Bug: toàn repo 0 entity dùng RowVersion/IsConcurrencyToken/[Timestamp] (finding #3) → mọi write qua
    /// EF tracking là last-write-wins, không có DbUpdateConcurrencyException.
    /// Test compile-level trên model: sau fix entity User phải có property IsConcurrencyToken=true.
    /// </summary>
    [Fact]
    public void UserEntity_HasRowVersionConcurrencyToken()
    {
        var (db, connection) = TestServices.CreateSqliteDb();
        try
        {
            var entityType = db.Model.FindEntityType(typeof(User));
            Assert.NotNull(entityType);

            var hasConcurrencyToken = entityType!.GetProperties().Any(p => p.IsConcurrencyToken);
            Assert.True(hasConcurrencyToken,
                "Users phải có concurrency token (RowVersion/[Timestamp]) — finding #3: hiện không có property IsConcurrencyToken");
        }
        finally
        {
            db.Dispose();
            connection.Dispose();
        }
    }
}
