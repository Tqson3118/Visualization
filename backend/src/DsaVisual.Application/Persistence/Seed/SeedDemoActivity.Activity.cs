using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// SEED-2c: quests/gems/inventory/favorites/feedback (SDD §7.5) — phần activity của
/// <see cref="SeedDemoActivity"/> (partial riêng, idempotent theo pattern SeedRunner).
///
/// Câu chuyện "app phát hành 1 tháng" (hôm nay = clock.UtcNow + 7h, UTC+7):
/// - 9 user (8 student @university.edu.vn + student@demo.local), mỗi user có 1-13 ngày quest;
///   quest hôm nay (DaysAgo=0) đủ 3-5 dòng, quest các ngày trước bổ sung để XP đạt level mong muốn
///   (level = 1 + floor(sqrt(Xp/100)) — không lưu level trong DB):
///   L1: S1 65, S3 40, S6 85 · L2: S2 360, S5 365 · L3: S4 445, S7 610, S9 795 · L4: S8 2160.
/// - User.Xp = tổng reward quest Claimed=1 (tính lại từ DB khi thêm quest mới — không cộng đúp);
///   User.Gems = tổng earn (quest claim) − tổng spend (mua shop) — tính lại từ DB rows.
/// - GemTransactions append-only (không có unique key) → guard: user chưa có giao dịch
///   RefType="quest"/"shop" nào thì mới thêm; chạy lần 2 = 0 dòng thêm.
/// </summary>
public static partial class SeedDemoActivity
{
    // ── Kế hoạch hoạt động (tĩnh, deterministic — đảm bảo idempotent) ──────────────

    private sealed record QuestDef(string Key, int Progress, bool Claimed);

    private sealed record DayDef(int DaysAgo, IReadOnlyList<QuestDef> Quests);

    private sealed record ItemDef(string ItemKey, bool Equip, int DaysAgo);

    private sealed record FavoriteDef(string SimulationKey, string InputJson, int DaysAgo);

    private sealed record FeedbackDef(string LessonTitle, int Rating, int CommentIndex, int DaysAgo);

    private sealed record StudentPlan(
        int StreakDays,
        IReadOnlyList<DayDef> Days,
        IReadOnlyList<ItemDef> Items,
        IReadOnlyList<FavoriteDef> Favorites,
        FeedbackDef Feedback);

    private static readonly IReadOnlyDictionary<string, StudentPlan> StudentActivityPlans = new Dictionary<string, StudentPlan>
    {
        // ── L1: 20-90 XP, ít claim, không đủ gems mua shop (item rẻ nhất 50 gems) ──
        ["nguyenminhanh@university.edu.vn"] = new(
            StreakDays: 3,
            Days:
            [
                new(0, [new("learn-1-node", 1, true), new("pass-1-quiz", 1, true), new("code-run-1", 1, true), new("lesson-viewed-2", 1, false), new("learn-3-node", 1, false)]),
            ],
            Items: [],
            Favorites:
            [
                new("sort.bubble", """{"array":[5,3,8,4,2]}""", 3),
                new("tree.bst-insert", """{"values":[10,5,15,3,7]}""", 9),
            ],
            Feedback: new("Bubble Sort", 5, 0, 2)),

        ["lethikimngan@university.edu.vn"] = new(
            StreakDays: 3,
            Days:
            [
                new(0, [new("pass-1-quiz", 1, true), new("learn-1-node", 1, true), new("code-run-1", 0, false), new("lesson-viewed-2", 1, false)]),
            ],
            Items: [],
            Favorites:
            [
                new("sort.bubble", """{"array":[4,2,9,1]}""", 7),
                new("tree.bst-search", """{"values":[10,5,15],"target":5}""", 15),
            ],
            Feedback: new("Stack", 3, 3, 8)),

        ["nguyentrang@university.edu.vn"] = new(
            StreakDays: 3,
            Days:
            [
                new(0, [new("learn-1-node", 1, true), new("pass-1-quiz", 1, true), new("code-run-1", 1, true), new("lesson-viewed-2", 2, true), new("pass-1-lab", 0, false)]),
            ],
            Items: [],
            Favorites:
            [
                new("search.binary", """{"array":[1,3,5,7,9,11],"target":7}""", 3),
                new("stack.push", """{"values":[1,2,3]}""", 11),
            ],
            Feedback: new("Hash Table", 5, 4, 18)),

        // ── L2: 120-350 XP, đủ gems mua avatar-ai-bot (50) ──
        ["tranquocbao@university.edu.vn"] = new(
            StreakDays: 5,
            Days:
            [
                new(0, [new("learn-3-node", 3, true), new("code-run-5", 6, true), new("streak-3", 4, true), new("lesson-viewed-2", 1, false), new("pass-1-quiz", 0, false)]),
                new(1, [new("learn-1-node", 1, true), new("pass-1-quiz", 1, true), new("code-run-1", 1, true), new("streak-3", 2, false)]),
                new(2, [new("learn-1-node", 1, true), new("pass-1-lab", 1, true), new("code-run-1", 0, false), new("lesson-viewed-2", 1, false)]),
                new(3, [new("streak-3", 3, true), new("code-run-5", 5, true), new("learn-3-node", 1, false)]),
            ],
            Items: [new("avatar-ai-bot", Equip: false, DaysAgo: 1)],
            Favorites:
            [
                new("sort.bubble", """{"array":[5,3,8,4,2]}""", 2),
                new("search.binary", """{"array":[1,3,5,7,9,11],"target":9}""", 5),
                new("graph.bfs", """{"vertices":["A","B","C","D"],"edges":[["A","B"],["A","C"],["B","D"]],"start":"A"}""", 12),
            ],
            Feedback: new("Binary Search", 4, 1, 5)),

        ["vuthanhtung@university.edu.vn"] = new(
            StreakDays: 7,
            Days:
            [
                new(0, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("pass-1-quiz", 0, false), new("lesson-viewed-2", 1, false), new("learn-1-node", 0, false)]),
                new(1, [new("streak-3", 3, true), new("pass-1-lab", 1, true), new("code-run-1", 0, false), new("learn-1-node", 0, false)]),
                new(2, [new("learn-1-node", 1, true), new("pass-1-quiz", 1, true), new("code-run-1", 1, true), new("lesson-viewed-2", 1, false)]),
                new(3, [new("streak-3", 3, true), new("code-run-5", 5, true), new("pass-1-lab", 1, true)]),
            ],
            Items: [new("avatar-ai-bot", Equip: false, DaysAgo: 3)],
            Favorites:
            [
                new("sort.selection", """{"array":[64,25,12,22,11]}""", 4),
                new("list.insert", """{"values":[1,2,3],"position":1,"value":9}""", 8),
                new("graph.dfs", """{"vertices":["0","1","2"],"edges":[["0","1"],["0","2"]],"start":"0"}""", 14),
            ],
            Feedback: new("Linked List", 4, 5, 15)),

        // ── L3: 400-899 XP ──
        ["phamhoanglong@university.edu.vn"] = new(
            StreakDays: 9,
            Days:
            [
                new(0, [new("learn-3-node", 3, true), new("pass-1-lab", 1, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("lesson-viewed-2", 1, false)]),
                new(1, [new("learn-1-node", 1, true), new("pass-1-quiz", 1, true), new("code-run-1", 1, true)]),
                new(2, [new("learn-1-node", 1, true), new("lesson-viewed-2", 2, true), new("pass-1-lab", 1, true)]),
                new(3, [new("pass-1-quiz", 1, true), new("learn-3-node", 3, true), new("streak-3", 3, true)]),
                new(4, [new("learn-1-node", 1, true), new("pass-1-quiz", 1, true), new("code-run-1", 0, false), new("lesson-viewed-2", 1, false)]),
            ],
            Items: [new("avatar-ai-bot", Equip: false, DaysAgo: 1)],
            Favorites:
            [
                new("tree.avl-insert", """{"values":[10,20,30,40]}""", 1),
                new("graph.bfs", """{"vertices":["0","1","2","3"],"edges":[["0","1"],["0","2"],["1","3"]],"start":"0"}""", 6),
                new("hash.insert", """{"capacity":8,"keys":[5,13,21]}""", 10),
                new("stack.push", """{"values":[7,1,4]}""", 18),
            ],
            Feedback: new("AVL", 5, 2, 12)),

        ["doanminhduc@university.edu.vn"] = new(
            StreakDays: 12,
            Days:
            [
                new(0, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("pass-1-lab", 1, true), new("lesson-viewed-2", 1, false)]),
                new(1, [new("learn-1-node", 1, true), new("code-run-1", 1, true), new("pass-1-quiz", 1, true), new("streak-3", 3, true)]),
                new(2, [new("code-run-5", 5, true), new("pass-1-lab", 1, true), new("learn-3-node", 3, true), new("pass-1-quiz", 0, false)]),
                new(3, [new("streak-3", 3, true), new("code-run-5", 5, true), new("lesson-viewed-2", 2, true), new("learn-1-node", 0, false)]),
                new(4, [new("learn-1-node", 1, true), new("pass-1-quiz", 1, true), new("code-run-1", 1, true), new("learn-3-node", 2, false), new("pass-1-lab", 0, false)]),
                new(5, [new("learn-1-node", 1, true), new("pass-1-quiz", 0, false), new("code-run-1", 0, false)]),
            ],
            Items: [new("avatar-ai-bot", Equip: false, DaysAgo: 2)],
            Favorites:
            [
                new("tree.bst-inorder", """{"values":[20,10,30,12,15]}""", 2),
                new("graph.bfs", """{"vertices":["0","1","2"],"edges":[["0","1"],["0","2"]],"start":"0"}""", 5),
                new("sort.quick", """{"array":[10,7,8,9,1,5]}""", 9),
                new("heap.insert", """{"values":[4,10,3,5,1]}""", 16),
            ],
            Feedback: new("BFS", 5, 6, 21)),

        ["student@demo.local"] = new(
            StreakDays: 10,
            Days:
            [
                new(0, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("lesson-viewed-2", 1, false), new("pass-1-quiz", 0, false)]),
                new(1, [new("learn-1-node", 1, true), new("pass-1-lab", 1, true), new("code-run-1", 1, true), new("streak-3", 3, true)]),
                new(2, [new("code-run-5", 5, true), new("streak-3", 3, true), new("pass-1-quiz", 1, true), new("learn-1-node", 0, false)]),
                new(3, [new("learn-3-node", 3, true), new("code-run-1", 1, true), new("learn-1-node", 1, true), new("pass-1-quiz", 0, false)]),
                new(4, [new("pass-1-quiz", 1, true), new("learn-1-node", 1, true), new("code-run-5", 3, false), new("streak-3", 1, false), new("lesson-viewed-2", 1, false)]),
                new(5, [new("streak-3", 3, true), new("code-run-5", 5, true), new("learn-3-node", 3, true), new("pass-1-lab", 0, false)]),
                new(6, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("learn-1-node", 0, false), new("pass-1-quiz", 0, false)]),
            ],
            Items: [new("avatar-ai-bot", Equip: false, DaysAgo: 2)],
            Favorites:
            [
                new("graph.bfs", """{"vertices":["A","B","C"],"edges":[["A","B"],["B","C"]],"start":"A"}""", 6),
                new("tree.bst-insert", """{"values":[20,10,30,25]}""", 10),
                new("sort.bubble", """{"array":[9,2,7,4,1]}""", 20),
            ],
            Feedback: new("Stack", 4, 9, 27)),

        // ── L4: 900+ XP — user chăm nhất, có frame trang bị ──
        ["huynhthuy@university.edu.vn"] = new(
            StreakDays: 15,
            Days:
            [
                new(0, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("pass-1-lab", 1, true), new("code-run-1", 1, true)]),
                new(1, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("pass-1-lab", 1, true)]),
                new(2, [new("code-run-5", 5, true), new("streak-3", 3, true), new("learn-3-node", 3, true), new("pass-1-quiz", 1, true)]),
                new(3, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("pass-1-lab", 1, true)]),
                new(4, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("pass-1-lab", 1, true)]),
                new(5, [new("code-run-5", 5, true), new("streak-3", 3, true), new("learn-3-node", 3, true), new("pass-1-quiz", 1, true)]),
                new(6, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("pass-1-lab", 1, true)]),
                new(7, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("pass-1-lab", 1, true)]),
                new(8, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("pass-1-lab", 1, true)]),
                new(9, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("pass-1-lab", 1, true)]),
                new(10, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("pass-1-lab", 1, true)]),
                new(11, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("pass-1-lab", 1, true)]),
                new(12, [new("learn-3-node", 3, true), new("code-run-5", 5, true), new("streak-3", 3, true), new("pass-1-lab", 1, true)]),
            ],
            Items: [new("avatar-ai-bot", Equip: false, DaysAgo: 1), new("frame-neon", Equip: true, DaysAgo: 10)],
            Favorites:
            [
                new("tree.avl-insert", """{"values":[30,20,10]}""", 1),
                new("graph.dijkstra", """{"vertices":["A","B","C","D"],"edges":[["A","B",4],["A","C",1],["C","B",2],["B","D",1]],"start":"A"}""", 4),
                new("hash.delete", """{"capacity":8,"keys":[5,13,21],"delete":[13]}""", 7),
                new("sort.merge", """{"array":[38,27,43,3,9,82,10]}""", 13),
            ],
            Feedback: new("BST", 2, 7, 24)),
    };

    // ── 1. UserQuests ─────────────────────────────────────────

    private static partial Task SeedUserQuestsAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
        => SeedUserQuestsCoreAsync(db, clock, logger, ct);

    private static async Task SeedUserQuestsCoreAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var todayLocal = clock.UtcNow.AddHours(7).Date;      // hôm nay UTC+7
        var quests = await db.DailyQuests.AsNoTracking().ToDictionaryAsync(q => q.QuestKey, ct);
        var items = await db.ShopItems.AsNoTracking().ToDictionaryAsync(i => i.ItemKey, ct);
        var users = await LoadActivityUsersAsync(db, ct);

        var totalAdded = 0;
        var totalSkipped = 0;
        foreach (var (email, plan) in StudentActivityPlans)
        {
            if (!users.TryGetValue(email, out var user))
            {
                logger.LogWarning("Seed: UserQuests bỏ qua {Email} (user chưa tồn tại)", email);
                continue;
            }

            var addedForUser = 0;
            var skippedForUser = 0;
            var addedClaimed = 0;
            foreach (var day in plan.Days)
            {
                var questDate = todayLocal.AddDays(-day.DaysAgo);
                foreach (var def in day.Quests)
                {
                    if (!quests.TryGetValue(def.Key, out var quest))
                    {
                        logger.LogWarning("Seed: UserQuests bỏ qua {Email}/{Key} (quest template chưa tồn tại)", email, def.Key);
                        continue;
                    }

                    var exists = await db.UserQuests.AnyAsync(
                        uq => uq.UserId == user.Id && uq.QuestDate == questDate && uq.QuestId == quest.Id, ct);
                    if (exists)
                    {
                        skippedForUser++;
                        continue;
                    }

                    db.UserQuests.Add(new UserQuest
                    {
                        UserId = user.Id,
                        QuestId = quest.Id,
                        QuestDate = questDate,
                        Progress = def.Progress,
                        Claimed = def.Claimed
                    });
                    addedForUser++;
                    if (def.Claimed)
                    {
                        addedClaimed++;
                    }
                }
            }

            // Chỉ cập nhật Xp/Gems/Streak khi có quest claim MỚI được thêm lần chạy này
            // (quest đã tồn tại → không đụng — không cộng đúp). Giá trị tính LẠI từ DB rows
            // nên luôn bằng tổng reward quest Claimed=1 / gems = earn − spend.
            if (addedClaimed > 0)
            {
                // Flush quest rows vừa thêm trước khi tính tổng — query đọc DB không thấy pending changes.
                await db.SaveChangesAsync(ct);
                var (xp, gemsEarned) = await SumClaimedRewardsAsync(db, user.Id, quests.Values, ct);
                var gemsSpent = await SumInventorySpendAsync(db, user.Id, items.Values, ct);
                user.Xp = xp;
                user.Gems = gemsEarned - gemsSpent;
                user.StreakDays = plan.StreakDays;
                user.LastActivityDate = todayLocal;
                user.StreakLastProcessed = todayLocal;
                user.UpdatedAt = clock.UtcNow;
                logger.LogInformation(
                    "Seed: UserQuests {Email}: {Added} thêm ({Claimed} đã claim) → Xp={Xp}, Gems={Gems}, Streak={Streak}",
                    email, addedForUser, addedClaimed, user.Xp, user.Gems, user.StreakDays);
            }
            else
            {
                logger.LogInformation("Seed: UserQuests {Email}: {Added} thêm, {Skipped} bỏ qua (đã tồn tại)", email, addedForUser, skippedForUser);
            }

            totalAdded += addedForUser;
            totalSkipped += skippedForUser;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: UserQuests thêm {Added}, bỏ qua {Skipped} (đã tồn tại)", totalAdded, totalSkipped);
    }

    // ── 2. GemTransactions (append-only) ──────────────────────

    private static partial Task SeedGemTransactionsAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
        => SeedGemTransactionsCoreAsync(db, clock, logger, ct);

    private static async Task SeedGemTransactionsCoreAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var todayLocal = clock.UtcNow.AddHours(7).Date;
        var quests = await db.DailyQuests.AsNoTracking().ToDictionaryAsync(q => q.QuestKey, ct);
        var questsById = await db.DailyQuests.AsNoTracking().ToDictionaryAsync(q => q.Id, ct);
        var items = await db.ShopItems.AsNoTracking().ToDictionaryAsync(i => i.ItemKey, ct);
        var users = await LoadActivityUsersAsync(db, ct);

        var totalAdded = 0;
        var totalSkipped = 0;
        foreach (var (email, plan) in StudentActivityPlans)
        {
            if (!users.TryGetValue(email, out var user))
            {
                continue;
            }

            var addedForUser = 0;

            // Quest earn — 1 dòng cho mỗi quest Claimed=1, RefId = UserQuest.Id (khớp ClaimQuestAsync).
            // GemTransactions không có unique key → guard: user chưa có giao dịch quest nào.
            var hasQuestTx = await db.GemTransactions.AsNoTracking()
                .AnyAsync(t => t.UserId == user.Id && t.RefType == "quest", ct);
            if (hasQuestTx)
            {
                totalSkipped++;
            }
            else
            {
                var claimedRows = await db.UserQuests.AsNoTracking()
                    .Where(uq => uq.UserId == user.Id && uq.Claimed)
                    .ToListAsync(ct);
                foreach (var row in claimedRows)
                {
                    if (!questsById.TryGetValue(row.QuestId, out var quest))
                    {
                        continue;
                    }

                    var reward = ParseReward(quest);
                    db.GemTransactions.Add(new GemTransaction
                    {
                        UserId = user.Id,
                        Type = 0,                                    // earn
                        Amount = reward.Gems,
                        RefType = "quest",
                        RefId = row.Id.ToString(),
                        CreatedAt = AtLocal(row.QuestDate, 14)        // 14:00 UTC+7 ngày claim
                    });
                    addedForUser++;
                }
            }

            // Shop spend — Type=1, Amount âm (−giá), khớp từng item UserInventory bước 3.
            var hasShopTx = await db.GemTransactions.AsNoTracking()
                .AnyAsync(t => t.UserId == user.Id && t.RefType == "shop", ct);
            if (hasShopTx)
            {
                totalSkipped++;
            }
            else
            {
                foreach (var itemDef in plan.Items)
                {
                    if (!items.TryGetValue(itemDef.ItemKey, out var item))
                    {
                        logger.LogWarning("Seed: GemTransactions bỏ qua {Email}/{ItemKey} (shop item chưa tồn tại)", email, itemDef.ItemKey);
                        continue;
                    }

                    db.GemTransactions.Add(new GemTransaction
                    {
                        UserId = user.Id,
                        Type = 1,                                    // spend
                        Amount = -item.PriceGems,
                        RefType = "shop",
                        RefId = item.Id.ToString(),
                        CreatedAt = AtLocal(todayLocal.AddDays(-itemDef.DaysAgo), 15)   // 15:00 UTC+7 ngày mua
                    });
                    addedForUser++;
                }
            }

            if (addedForUser > 0)
            {
                logger.LogInformation("Seed: GemTransactions {Email}: {Added} thêm", email, addedForUser);
            }

            totalAdded += addedForUser;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: GemTransactions thêm {Added}, bỏ qua {Skipped} (đã tồn tại)", totalAdded, totalSkipped);
    }

    // ── 3. UserInventory ──────────────────────────────────────

    private static partial Task SeedUserInventoryAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
        => SeedUserInventoryCoreAsync(db, clock, logger, ct);

    private static async Task SeedUserInventoryCoreAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var todayLocal = clock.UtcNow.AddHours(7).Date;
        var quests = await db.DailyQuests.AsNoTracking().ToDictionaryAsync(q => q.QuestKey, ct);
        var items = await db.ShopItems.AsNoTracking().ToDictionaryAsync(i => i.ItemKey, ct);
        var users = await LoadActivityUsersAsync(db, ct);

        var totalAdded = 0;
        var totalSkipped = 0;
        foreach (var (email, plan) in StudentActivityPlans)
        {
            if (!users.TryGetValue(email, out var user))
            {
                continue;
            }

            var addedForUser = 0;
            foreach (var itemDef in plan.Items)
            {
                if (!items.TryGetValue(itemDef.ItemKey, out var item))
                {
                    logger.LogWarning("Seed: UserInventory bỏ qua {Email}/{ItemKey} (shop item chưa tồn tại)", email, itemDef.ItemKey);
                    continue;
                }

                var exists = await db.UserInventory.AnyAsync(i => i.UserId == user.Id && i.ItemId == item.Id, ct);
                if (exists)
                {
                    totalSkipped++;
                    continue;
                }

                var purchasedAt = AtLocal(todayLocal.AddDays(-itemDef.DaysAgo), 15);
                db.UserInventory.Add(new UserInventory
                {
                    UserId = user.Id,
                    ItemId = item.Id,
                    Quantity = 1,
                    IsEquipped = itemDef.Equip && item.Type == 1,    // chỉ frame (Type 1) trang bị được
                    PurchasedAt = purchasedAt,
                    ExpiresAt = item.DurationHours is { } hours ? purchasedAt.AddHours(hours) : null
                });
                addedForUser++;
            }

            // Item MỚI được thêm → tính lại Gems = earn (quest đã claim) − spend (toàn bộ kho)
            // từ DB rows — đảm bảo khớp GemTransactions + UserInventory dù chạy nhiều lần.
            if (addedForUser > 0)
            {
                // Flush inventory rows vừa thêm trước khi tính spend — query đọc DB không thấy pending changes.
                await db.SaveChangesAsync(ct);
                var (_, gemsEarned) = await SumClaimedRewardsAsync(db, user.Id, quests.Values, ct);
                var gemsSpent = await SumInventorySpendAsync(db, user.Id, items.Values, ct);
                user.Gems = gemsEarned - gemsSpent;
                user.UpdatedAt = clock.UtcNow;
                logger.LogInformation(
                    "Seed: UserInventory {Email}: {Added} thêm → Gems={Gems} (earn={Earned}, spend={Spent})",
                    email, addedForUser, user.Gems, gemsEarned, gemsSpent);
            }

            totalAdded += addedForUser;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: UserInventory thêm {Added}, bỏ qua {Skipped} (đã tồn tại)", totalAdded, totalSkipped);
    }

    // ── 4. Favorites ──────────────────────────────────────────

    private static partial Task SeedFavoritesAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
        => SeedFavoritesCoreAsync(db, clock, logger, ct);

    private static async Task SeedFavoritesCoreAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var todayLocal = clock.UtcNow.AddHours(7).Date;
        var users = await LoadActivityUsersAsync(db, ct);

        var totalAdded = 0;
        var totalSkipped = 0;
        foreach (var (email, plan) in StudentActivityPlans)
        {
            if (!users.TryGetValue(email, out var user))
            {
                continue;
            }

            var addedForUser = 0;
            foreach (var fav in plan.Favorites)
            {
                var exists = await db.Favorites.AnyAsync(f => f.UserId == user.Id && f.SimulationKey == fav.SimulationKey, ct);
                if (exists)
                {
                    totalSkipped++;
                    continue;
                }

                db.Favorites.Add(new Favorite
                {
                    UserId = user.Id,
                    SimulationKey = fav.SimulationKey,
                    InputJson = fav.InputJson,
                    CreatedAt = AtLocal(todayLocal.AddDays(-fav.DaysAgo), 20)
                });
                addedForUser++;
            }

            if (addedForUser > 0)
            {
                logger.LogInformation("Seed: Favorites {Email}: {Added} thêm", email, addedForUser);
            }

            totalAdded += addedForUser;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: Favorites thêm {Added}, bỏ qua {Skipped} (đã tồn tại)", totalAdded, totalSkipped);
    }

    // ── 5. ContentFeedback ────────────────────────────────────

    private static partial Task SeedContentFeedbackAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
        => SeedContentFeedbackCoreAsync(db, clock, logger, ct);

    private static async Task SeedContentFeedbackCoreAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var todayLocal = clock.UtcNow.AddHours(7).Date;
        var lessons = await db.Lessons.AsNoTracking()
            .Where(l => l.DeletedAt == null)
            .ToDictionaryAsync(l => l.Title, ct);
        var users = await LoadActivityUsersAsync(db, ct);

        var totalAdded = 0;
        var totalSkipped = 0;
        foreach (var (email, plan) in StudentActivityPlans)
        {
            if (!users.TryGetValue(email, out var user))
            {
                continue;
            }

            var fb = plan.Feedback;
            if (!lessons.TryGetValue(fb.LessonTitle, out var lesson))
            {
                logger.LogWarning("Seed: ContentFeedback bỏ qua {Email}/{Lesson} (lesson chưa tồn tại)", email, fb.LessonTitle);
                continue;
            }

            if (fb.CommentIndex < 0 || fb.CommentIndex >= SeedData.Feedbacks.Count)
            {
                logger.LogWarning("Seed: ContentFeedback bỏ qua {Email} (CommentIndex {Index} ngoài phạm vi)", email, fb.CommentIndex);
                continue;
            }

            var exists = await db.ContentFeedback.AnyAsync(f => f.UserId == user.Id && f.LessonId == lesson.Id, ct);
            if (exists)
            {
                totalSkipped++;
                continue;
            }

            db.ContentFeedback.Add(new ContentFeedback
            {
                UserId = user.Id,
                LessonId = lesson.Id,
                Rating = fb.Rating,
                Comment = SeedData.Feedbacks[fb.CommentIndex].Comment,
                CreatedAt = AtLocal(todayLocal.AddDays(-fb.DaysAgo), 21),
                UpdatedAt = null
            });
            totalAdded++;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: ContentFeedback thêm {Added}, bỏ qua {Skipped} (đã tồn tại)", totalAdded, totalSkipped);
    }

    // ── Helpers ───────────────────────────────────────────────

    /// <summary>9 user activity: 8 student @university.edu.vn (SeedData.Students) + student@demo.local. KHÔNG đụng user khác.</summary>
    private static async Task<Dictionary<string, User>> LoadActivityUsersAsync(AppDbContext db, CancellationToken ct)
    {
        var emails = SeedData.Students
            .Select(s => s.Email.ToLowerInvariant())
            .Append("student@demo.local")
            .ToHashSet(StringComparer.Ordinal);

        return await db.Users
            .Where(u => u.DeletedAt == null && emails.Contains(u.Email))
            .ToDictionaryAsync(u => u.Email, ct);
    }

    /// <summary>Reward từ RewardJson ({"gems":N,"xp":M}) — khớp GetReward của GamificationService.</summary>
    private static (int Xp, int Gems) ParseReward(DailyQuest quest)
    {
        try
        {
            using var doc = JsonDocument.Parse(quest.RewardJson);
            var root = doc.RootElement;
            var xp = root.TryGetProperty("xp", out var x) && x.TryGetInt32(out var xv) ? xv : 0;
            var gems = root.TryGetProperty("gems", out var g) && g.TryGetInt32(out var gv) ? gv : 0;
            return (xp, gems);
        }
        catch (JsonException)
        {
            return (0, 0);
        }
    }

    /// <summary>Tổng reward của MỌI quest Claimed=1 của user (đọc lại từ DB — không cộng đúp).</summary>
    private static async Task<(int Xp, int Gems)> SumClaimedRewardsAsync(AppDbContext db, int userId, IEnumerable<DailyQuest> questTemplates, CancellationToken ct)
    {
        var questsById = questTemplates.ToDictionary(q => q.Id);
        var claimedRows = await db.UserQuests.AsNoTracking()
            .Where(uq => uq.UserId == userId && uq.Claimed)
            .ToListAsync(ct);

        var xp = 0;
        var gems = 0;
        foreach (var row in claimedRows)
        {
            if (!questsById.TryGetValue(row.QuestId, out var quest))
            {
                continue;
            }

            var reward = ParseReward(quest);
            xp += reward.Xp;
            gems += reward.Gems;
        }

        return (xp, gems);
    }

    /// <summary>Tổng tiền đã mua (UserInventory × ShopItem.PriceGems) của user — đọc lại từ DB.</summary>
    private static async Task<int> SumInventorySpendAsync(AppDbContext db, int userId, IEnumerable<ShopItem> shopItems, CancellationToken ct)
    {
        var itemsById = shopItems.ToDictionary(i => i.Id);
        var ownedItemIds = await db.UserInventory.AsNoTracking()
            .Where(i => i.UserId == userId)
            .Select(i => i.ItemId)
            .ToListAsync(ct);

        var spend = 0;
        foreach (var itemId in ownedItemIds)
        {
            if (itemsById.TryGetValue(itemId, out var item))
            {
                spend += item.PriceGems;
            }
        }

        return spend;
    }

    /// <summary>Mốc giờ "địa phương" (giờ UTC+7) → UTC instant lưu DB (CreatedAt/PurchasedAt dùng clock.UtcNow trong app).</summary>
    private static DateTime AtLocal(DateTime localDay, int hour)
        => localDay.Date.AddHours(hour).AddHours(-7);
}
