using System.Net;
using System.Net.Http.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DsaVisual.IntegrationTests;

/// <summary>
/// TEST TÁI HIỆN (đỏ/fail) qua HTTP thật (Testcontainers SQL Server) — 7 lỗi business-logic
/// GamificationService (docs/work/backend-audit/findings-biz-gamification.md).
/// Test race thật đánh dấu [Trait("Category","Race")] — test deterministic (đỏ chắc chắn) nằm ở
/// DsaVisual.UnitTests/GamificationServiceRegressionTests.cs.
/// </summary>
public sealed class GamificationRegressionTests : IntegrationTestBase, IClassFixture<ApiTestFixture>
{
    public GamificationRegressionTests(ApiTestFixture fixture) : base(fixture) { }

    private async Task SeedQuestPoolAsync()
    {
        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.DailyQuests.AddRange(
            new DailyQuest { QuestKey = $"reg-learn-1-node-{Guid.NewGuid():N}", Title = "Học 1 node", Type = 0, ConditionJson = """{"activity":"pass_node","count":1}""", RewardJson = """{"gems":3,"xp":20}""", PoolEnabled = true },
            new DailyQuest { QuestKey = $"reg-pass-1-quiz-{Guid.NewGuid():N}", Title = "Quiz 1", Type = 0, ConditionJson = """{"activity":"pass_quiz","count":1}""", RewardJson = """{"gems":3,"xp":20}""", PoolEnabled = true },
            new DailyQuest { QuestKey = $"reg-learn-3-node-{Guid.NewGuid():N}", Title = "3 node", Type = 1, ConditionJson = """{"activity":"pass_node","count":3}""", RewardJson = """{"gems":5,"xp":30}""", PoolEnabled = true },
            new DailyQuest { QuestKey = $"reg-code-run-1-{Guid.NewGuid():N}", Title = "Code 1", Type = 1, ConditionJson = """{"activity":"code_run","count":1}""", RewardJson = """{"gems":4,"xp":25}""", PoolEnabled = true },
            new DailyQuest { QuestKey = $"reg-code-run-5-{Guid.NewGuid():N}", Title = "Code 5", Type = 2, ConditionJson = """{"activity":"code_run","count":5}""", RewardJson = """{"gems":8,"xp":50}""", PoolEnabled = true });
        await db.SaveChangesAsync();
    }

    // ── BUG-1 (CAO): Premium hết hạn → GET /me/hearts phải trả HeartsMax=10, Hearts clamp ≤ 10 ──

    [Fact(DisplayName = "REPRO BUG-1: premium hết hạn (HeartsMax=30, Hearts=25) → GET /me/hearts trả 10/10")]
    public async Task GetHearts_ExpiredPremium_ClampsToFreeLimits()
    {
        var user = await CreateUserAsync();
        await using (var scope = Factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var u = await db.Users.FirstAsync(x => x.Id == user.Id);
            u.Hearts = 25;
            u.HeartsMax = 30;                       // quyền lợi premium còn "đọng" trong DB
            u.PremiumUntil = DateTime.UtcNow.AddDays(-1);   // đã hết hạn
            u.LastHeartAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }

        using var client = CreateClientWithToken(user.Id, RoleNames.Student);
        var response = await client.GetAsync("/api/v1/me/hearts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var hearts = await ReadJsonAsync<HeartsStatusDto>(response);
        Assert.Equal(10, hearts.HeartsMax);   // bug: 30 (HeartConfig trả HeartsMax DB khi hết hạn — BUG-1)
        Assert.Equal(10, hearts.Hearts);      // bug: 25 (không clamp Hearts về 10 — FR-10.7)
    }

    // ── #7 (TRUNG): mock-pay 2 lần cùng orderId → PremiumUntil KHÔNG gia hạn chồng ─────────

    [Fact(DisplayName = "REPRO #7: mock-pay 2 lần cùng orderId → PremiumUntil sau 2 lần = sau 1 lần")]
    public async Task MockPay_SameOrderTwice_DoesNotStackRenewal()
    {
        var user = await CreateUserAsync();
        using var client = CreateClientWithToken(user.Id, RoleNames.Student);

        var upgrade = await client.PostAsJsonAsync("/api/v1/premium/upgrade", new PremiumUpgradeRequest { PlanId = "1m" });
        Assert.Equal(HttpStatusCode.OK, upgrade.StatusCode);
        var order = await ReadJsonAsync<PremiumUpgradeResultDto>(upgrade);

        var pay1 = await client.PostAsJsonAsync("/api/v1/premium/mock-pay", new PremiumMockPayRequest { OrderId = order.OrderId });
        Assert.Equal(HttpStatusCode.OK, pay1.StatusCode);
        var status1 = await ReadJsonAsync<PremiumStatusDto>(pay1);

        var pay2 = await client.PostAsJsonAsync("/api/v1/premium/mock-pay", new PremiumMockPayRequest { OrderId = order.OrderId });
        Assert.Equal(HttpStatusCode.OK, pay2.StatusCode);
        var status2 = await ReadJsonAsync<PremiumStatusDto>(pay2);

        // Bug: lần 2 baseTime = PremiumUntil hiện tại + AddMonths → gia hạn chồng 2 lần
        Assert.Equal(status1.ExpiresAt, status2.ExpiresAt);

        await using var scope = Factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userDb = await db.Users.AsNoTracking().FirstAsync(x => x.Id == user.Id);
        Assert.Equal(status1.ExpiresAt, userDb.PremiumUntil);   // bug: PremiumUntil tăng gấp đôi
        var sub = await db.PremiumSubscriptions.AsNoTracking().FirstAsync(s => s.Id == order.OrderId);
        Assert.Equal(status1.ExpiresAt, sub.ExpiresAt);         // bug: ExpiresAt bị ghi đè +1 tháng
    }

    // ── #5 (TRUNG): GET /me/quests 2 request song song (chưa có quest hôm nay) → cả 2 phải 200 ──

    /// <summary>
    /// Bug: GetQuestsAsync check-then-insert (GamificationService.cs:273-288) không bắt DbUpdateException —
    /// 2 request song song cùng thấy chưa có quest → cả 2 INSERT 5 dòng → vi phạm UNIQUE(UserId,QuestDate,QuestId)
    /// → ErrorHandlingMiddleware trả 503 SERVICE_UNAVAILABLE (DbUpdateException).
    /// Đúng sau fix: cả 2 phải 200 (upsert/idempotent).
    /// </summary>
    [Fact(DisplayName = "REPRO #5: 2 GET /me/quests song song đầu ngày → cả 2 phải 200 (hiện 1 request 503)")]
    public async Task GetQuests_TwoConcurrentRequests_BothReturn200()
    {
        await SeedQuestPoolAsync();

        for (var i = 0; i < 3; i++)
        {
            var user = await CreateUserAsync();

            using var client = CreateClientWithToken(user.Id, RoleNames.Student);
            var responses = await Task.WhenAll(
                client.GetAsync("/api/v1/me/quests"),
                client.GetAsync("/api/v1/me/quests"));

            // Bug: 1 trong 2 request INSERT trùng → DbUpdateException → 503 SERVICE_UNAVAILABLE
            Assert.Equal(HttpStatusCode.OK, responses[0].StatusCode);
            Assert.Equal(HttpStatusCode.OK, responses[1].StatusCode);
        }
    }

    // ── #2 (CAO, race): GET hearts song song EnterNode → tim không được "trả lại" ─────────

    /// <summary>
    /// Probe race thật (finding #2): PersistHeartRegenAsync ghi đè tim vừa trừ.
    /// Nếu trúng race: GET ghi 3 (regen từ read stale) sau khi EnterNode trừ → hearts=3 (sai, tim bị trả lại).
    /// Serial-equivalent: 2 (regen 2→3 rồi trừ → 2).
    /// Test deterministic (luôn đỏ hiện tại) ở unit test HeartRegen_StaleWriteAfterConcurrentDeduct_DoesNotRefundHeart.
    /// </summary>
    [Trait("Category", "Race")]
    [Fact(DisplayName = "REPRO #2 (race): GET hearts song song EnterNode → hearts không tăng sai")]
    public async Task EnterNode_ConcurrentWithGetHearts_HeartsNotInflated()
    {
        for (var i = 0; i < 6; i++)
        {
            var user = await CreateUserAsync();
            var pathId = 0;
            var nodeId = 0;
            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var u = await db.Users.FirstAsync(x => x.Id == user.Id);
                u.Hearts = 2;
                u.HeartsMax = 10;
                u.LastHeartAt = DateTime.UtcNow.AddMinutes(-31);   // đã qua 1 chu kỳ regen free 30p
                // SỬA TEST (giữ nguyên ý nghĩa probe): không set explicit Id trên identity column
                // (SQL Server reject IDENTITY_INSERT OFF) — để identity tự sinh, dùng Id sau khi lưu.
                var path = new LearningPath { Title = $"Race path {i}", IsActive = true, CreatedBy = user.Id };
                db.LearningPaths.Add(path);
                await db.SaveChangesAsync();
                var node = new LearningPathNode { PathId = path.Id, Title = $"Race node {i}", SortOrder = 1 };
                db.LearningPathNodes.Add(node);
                await db.SaveChangesAsync();
                pathId = path.Id;
                nodeId = node.Id;
            }

            using var client = CreateClientWithToken(user.Id, RoleNames.Student);
            var get = client.GetAsync("/api/v1/me/hearts");
            var enter = client.PostAsJsonAsync($"/api/v1/learning-path/{pathId}/nodes/{nodeId}/enter", new NodeEnterRequest());
            await Task.WhenAll(get, enter);

            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var hearts = await db.Users.AsNoTracking().Where(x => x.Id == user.Id).Select(x => x.Hearts).FirstAsync();
                // Serial: regen 2→3 rồi trừ 1 → 2; bug (lost-update): GET ghi đè 3 sau khi enter trừ
                Assert.True(hearts <= 2, $"Iteration {i}: hearts={hearts} — tim tăng sai (finding #2, lost-update)");
            }
        }
    }

    // ── #4 (CAO, race): 2 mua song song sát MaxStack → gems trừ đúng 1 lần ───────────────

    /// <summary>
    /// Probe race thật (finding #4): Quantity=MaxStack−1, 2 mua song song cùng đọc 4 → đều trừ gems,
    /// cả 2 ghi 5 (lost-update) → trừ gems 2 lần cho 1 lần +1.
    /// Serial-equivalent: 1 lần mua (4→5), gems 200−50=150.
    /// Test deterministic (luôn đỏ hiện tại) ở unit test BuyItem_StaleQuantityRead_DoesNotDoubleDeductGems.
    /// </summary>
    [Trait("Category", "Race")]
    [Fact(DisplayName = "REPRO #4 (race): 2 mua song song sát MaxStack → gems KHÔNG trừ 2 lần")]
    public async Task BuyItem_TwoConcurrentBuysAtMaxStackEdge_GemsDeductedOnce()
    {
        for (var i = 0; i < 6; i++)
        {
            var user = await CreateUserAsync();
            var itemKey = $"race-item-{Guid.NewGuid():N}";
            var itemId = 0;
            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var u = await db.Users.FirstAsync(x => x.Id == user.Id);
                u.Gems = 200;
                var item = new ShopItem { ItemKey = itemKey, Name = $"Item {i}", PriceGems = 50, MaxStack = 5, Type = 0 };
                db.ShopItems.Add(item);
                await db.SaveChangesAsync();
                db.UserInventory.Add(new UserInventory { UserId = user.Id, ItemId = item.Id, Quantity = 4, IsEquipped = false, PurchasedAt = DateTime.UtcNow });
                await db.SaveChangesAsync();
                itemId = item.Id;
            }

            using var client = CreateClientWithToken(user.Id, RoleNames.Student);
            var buy1 = client.PostAsJsonAsync("/api/v1/shop/buy", new ShopBuyRequest { ItemId = itemId });
            var buy2 = client.PostAsJsonAsync("/api/v1/shop/buy", new ShopBuyRequest { ItemId = itemId });
            await Task.WhenAll(buy1, buy2);

            await using (var scope = Factory.Services.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var gems = await db.Users.AsNoTracking().Where(x => x.Id == user.Id).Select(x => x.Gems).FirstAsync();
                var qty = await db.UserInventory.AsNoTracking().Where(x => x.UserId == user.Id).Select(x => x.Quantity).FirstOrDefaultAsync();
                Assert.True(qty <= 5, $"Iteration {i}: qty={qty} vượt MaxStack");
                // Serial: chỉ 1 lần mua hợp lệ → gems 150; bug (stale-read): trừ 2 lần → 100
                Assert.True(gems >= 150, $"Iteration {i}: gems={gems} — trừ gems 2 lần cho 1 lần +1 (finding #4)");
            }
        }
    }
}
