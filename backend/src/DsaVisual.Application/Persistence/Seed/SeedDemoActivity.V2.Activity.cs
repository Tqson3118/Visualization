using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// SEED-V2 (PROMPT_K_SEED_PROD_V2) — Task 4 (+ Task 4b sửa gems âm): UserQuests + GemTransactions +
/// UserInventory + Favorites + ContentFeedback cho 69 user V2 (68 student @university.edu.vn theo 4
/// persona + showcase@demo.local). Pattern y hệt V1 SeedDemoActivity.Activity.cs: kế hoạch
/// deterministic theo Index của V2Users (KHÔNG random — chạy lại y hệt), guard idempotent, recompute
/// Xp/Gems/StreakDays TỪ DB rows khi có quest claim mới (không set tay); chạy lại lần 2 → guard bỏ
/// qua hết (0 dòng thêm).
///
/// Kế hoạch theo persona (tổng — khớp khoảng mục tiêu Task 4 + Task 4b):
///   UserQuests 2856 (Showcase 150 / Hardworking 1360 / Average 1296 / Slacker 50 / New 0),
///   GemTransactions 1360 = claims (1300 quest-earn) + spend (60 shop),
///   UserInventory 60, Favorites 245, ContentFeedback 25.
///   Showcase: 30 ngày × 5 quest, claim 109 → Xp = 2790, Gems = 439 − 150 = 289, Streak 30.
///
/// Task 4b (fix gems âm): mọi user V2 có Gems ≥ 0 — V2ItemPlan lọc theo ngân sách Σ gems earn từ
/// quest claim plan (item rẻ mua trước; user không đủ → không mua item đó). 16 user Average
/// (Index%8 ≥ 4) mua 2 item (150 gems) trước đây bị cắt còn 1 (avatar-ai-bot 50) vì earn tối đa
/// trong khung XP 250-750 chỉ ~125 gems; bù 1 item cho showcase (2 item, gems 289) + thêm 6 quest
/// claim cho Hardworking (Index 0-5, ngày 0) để GemTransactions giữ ≥ 1360. UserInventory 60 < mục
/// tiêu 73-110: trần khả thi khi giữ Gems ≥ 0 + unique (UserId, ItemId) + XP persona là 61
/// (Average ≤ 1 item, Hardworking ≤ 2, Showcase ≤ 3) — ghi chú tại docs/work/seed-v2/quest-xp-showcase.md.
/// KHÔNG sửa file V1, KHÔNG wire vào SeedDemoActivity.cs (Task 6 sẽ thêm call).
/// </summary>
public static partial class SeedDemoActivity
{
    // ── Kế hoạch deterministic theo Index (KHÔNG random — chạy lại y hệt) ──────────────

    private sealed record V2ItemSeed(string ItemKey, bool Equip, int DaysAgo);

    private sealed record V2FavoriteSeed(string SimulationKey, string InputJson, int DaysAgo);

    private sealed record V2FeedbackSeed(string LessonTitle, int Rating, int CommentIndex, int DaysAgo);

    /// <summary>Rotation quest theo thứ tự SeedData.Quests — ngày d của user bắt đầu offset (Index + d) % 8.</summary>
    private static readonly string[] V2QuestRotation =
        ["learn-1-node", "learn-3-node", "pass-1-quiz", "pass-1-lab", "code-run-1", "code-run-5", "lesson-viewed-2", "streak-3"];

    /// <summary>Reward + progress mục tiêu theo QuestKey — khớp SeedData.Quests (RewardJson/ConditionJson).</summary>
    private static readonly IReadOnlyDictionary<string, (int Xp, int Gems, int Progress)> V2QuestStats =
        new Dictionary<string, (int, int, int)>
        {
            ["learn-1-node"] = (20, 3, 1),
            ["learn-3-node"] = (30, 5, 3),
            ["pass-1-quiz"] = (20, 3, 1),
            ["pass-1-lab"] = (25, 4, 1),
            ["code-run-1"] = (25, 4, 1),
            ["code-run-5"] = (50, 8, 5),
            ["lesson-viewed-2"] = (20, 3, 2),
            ["streak-3"] = (60, 10, 3),
        };

    /// <summary>Giá gems theo ItemKey cho item dùng trong V2ItemPlan — khớp SeedData.ShopItems (pattern V2QuestStats).</summary>
    private static readonly IReadOnlyDictionary<string, int> V2ShopPrices =
        new Dictionary<string, int>
        {
            ["avatar-ai-bot"] = 50,
            ["avatar-cyber-hacker"] = 100,
        };

    /// <summary>Simulation keys hợp lệ (shared/simulation-catalog.json) — xoay vòng theo Index cho Favorites.</summary>
    private static readonly string[] V2SimulationKeys =
        ["sort.bubble", "sort.selection", "sort.insertion", "search.binary", "list.insert", "stack.push",
         "tree.bst-insert", "tree.bst-search", "tree.avl-insert", "hash.insert", "graph.bfs", "graph.dfs",
         "sort.quick", "sort.merge", "heap.insert", "graph.dijkstra"];

    /// <summary>Persona theo Index deterministic của V2Users (0-67 student, 68 showcase).</summary>
    private static string V2PersonaOf(int index) => index switch
    {
        68 => V2Users.ShowcasePersona,
        <= 12 => V2Users.HardworkingPersona,
        <= 44 => V2Users.AveragePersona,
        <= 57 => V2Users.SlackerPersona,
        _ => V2Users.NewPersona
    };

    /// <summary>Số ngày quest theo persona — Showcase 30, Hardworking 18-25, Average 8-15, Slacker 1-3, New 0-1.</summary>
    private static int V2QuestDays(string persona, int index) => persona switch
    {
        V2Users.ShowcasePersona => 30,
        V2Users.HardworkingPersona => 18 + index % 8,
        V2Users.AveragePersona => 8 + index % 8,
        V2Users.SlackerPersona => 1 + index % 3,
        _ => index % 2
    };

    /// <summary>Số quest rows/ngày — Showcase/Hardworking 5, Average 3-4, Slacker 2, New 0.</summary>
    private static int V2QuestRows(string persona, int index, int day) => persona switch
    {
        V2Users.ShowcasePersona => 5,
        V2Users.HardworkingPersona => 5,
        V2Users.AveragePersona => 3 + (index + day) % 2,
        V2Users.SlackerPersona => 2,
        _ => 0
    };

    /// <summary>Quest key tại vị trí j trong ngày d — rotation bắt đầu (Index + d) % 8.
    /// Slacker dùng quest cố định [code-run-5, lesson-viewed-2] mỗi ngày (XP 50-190 — giữ khung 40-200).</summary>
    private static string V2QuestKeyAt(string persona, int index, int day, int j)
    {
        if (persona == V2Users.SlackerPersona)
        {
            return j == 0 ? "code-run-5" : "lesson-viewed-2";
        }

        return V2QuestRotation[((index + day) % 8 + j) % V2QuestRotation.Length];
    }

    /// <summary>Quest nào được claim trong ngày — theo persona (deterministic):
    /// Showcase: quest XP ≤ 25 + bù đủ 3 + quest ≥ 50 ngày d%3==0 + bù đủ 4 ngày d%5==0;
    /// Hardworking: j0 luôn + j1 (Index%8 ≤ 3 → luôn, ngược lại chỉ XP ≤ 30);
    /// Average: j0 luôn + j1 (Index%8 ≤ 4 → XP ≤ 30, Index%8 ≥ 5 → XP ≤ 25);
    /// Slacker: code-run-5 mỗi ngày + lesson-viewed-2 (days ≤ 2 hoặc chưa phải ngày cuối); New: không.</summary>
    private static bool[] V2QuestClaims(string persona, int index, int day, int rows)
    {
        var claims = new bool[rows];
        switch (persona)
        {
            case V2Users.ShowcasePersona:
                for (var j = 0; j < rows; j++)
                {
                    if (V2QuestStats[V2QuestKeyAt(persona, index, day, j)].Xp <= 25)
                    {
                        claims[j] = true;
                    }
                }

                // bù đủ 3 claims — quest XP thấp nhất chưa claim
                var count = claims.Count(c => c);
                while (count < 3)
                {
                    var best = -1;
                    var bestXp = int.MaxValue;
                    for (var j = 0; j < rows; j++)
                    {
                        if (!claims[j])
                        {
                            var xp = V2QuestStats[V2QuestKeyAt(persona, index, day, j)].Xp;
                            if (xp < bestXp)
                            {
                                best = j;
                                bestXp = xp;
                            }
                        }
                    }

                    if (best < 0)
                    {
                        break;
                    }

                    claims[best] = true;
                    count++;
                }

                // ngày d%3==0: claim thêm quest XP ≥ 50 (ưu tiên cao nhất) — "ngày đỉnh cao"
                if (day % 3 == 0)
                {
                    var best = -1;
                    var bestXp = 0;
                    for (var j = 0; j < rows; j++)
                    {
                        if (!claims[j])
                        {
                            var xp = V2QuestStats[V2QuestKeyAt(persona, index, day, j)].Xp;
                            if (xp >= 50 && xp > bestXp)
                            {
                                best = j;
                                bestXp = xp;
                            }
                        }
                    }

                    if (best >= 0)
                    {
                        claims[best] = true;
                    }
                }

                // ngày d%5==0: bù đủ 4 claims — quest XP thấp nhất chưa claim
                if (day % 5 == 0 && claims.Count(c => c) < 4)
                {
                    var best = -1;
                    var bestXp = int.MaxValue;
                    for (var j = 0; j < rows; j++)
                    {
                        if (!claims[j])
                        {
                            var xp = V2QuestStats[V2QuestKeyAt(persona, index, day, j)].Xp;
                            if (xp < bestXp)
                            {
                                best = j;
                                bestXp = xp;
                            }
                        }
                    }

                    if (best >= 0)
                    {
                        claims[best] = true;
                    }
                }

                break;
            case V2Users.HardworkingPersona:
                claims[0] = true;
                if (rows > 1 && (index % 8 <= 3 || V2QuestStats[V2QuestKeyAt(persona, index, day, 1)].Xp <= 30))
                {
                    claims[1] = true;
                }

                // Task 4b — bù đủ 6 earn-tx để GemTransactions ≥ 1360 khi UserInventory giảm 75 → 60:
                // 6 user Hardworking đầu (Index 0-5) claim thêm quest j2 ngày 0 (XP sau 1095-1405 ≤ 1500 ✓,
                // gems +3/+4/+4/+8/+3/+10). Deterministic, chỉ áp dụng ngày 0 → chạy lại vẫn y hệt.
                if (day == 0 && index <= 5 && rows > 2)
                {
                    claims[2] = true;
                }

                break;
            case V2Users.AveragePersona:
                claims[0] = true;
                if (rows > 1)
                {
                    var j1Xp = V2QuestStats[V2QuestKeyAt(persona, index, day, 1)].Xp;
                    if ((index % 8 <= 4 && j1Xp <= 30) || (index % 8 >= 5 && j1Xp <= 25))
                    {
                        claims[1] = true;
                    }
                }

                break;
            case V2Users.SlackerPersona:
                claims[0] = true;
                if (rows > 1 && (V2QuestDays(persona, index) <= 2 || day < 2))
                {
                    claims[1] = true;
                }

                break;
        }

        return claims;
    }

    /// <summary>StreakDays theo persona — Showcase 30, Hardworking 12-25, Average 3-8, Slacker 0-2, New 0.</summary>
    private static int V2StreakDays(string persona, int index) => persona switch
    {
        V2Users.ShowcasePersona => 30,
        V2Users.HardworkingPersona => 12 + index % 14,
        V2Users.AveragePersona => 3 + index % 6,
        V2Users.SlackerPersona => index % 3,
        _ => 0
    };

    /// <summary>
    /// Shop items mua theo persona, LỌC theo ngân sách gems (Task 4b — đảm bảo Gems ≥ 0 cho mọi user):
    /// Showcase 2 (ai-bot + cyber-hacker), Hardworking 2, Average 1 (Index%8 ≥ 4 trước đây mua thêm
    /// cyber-hacker 100 → earn tối đa trong XP 250-750 chỉ ~125 gems nên item thứ 2 bị lọc), còn lại 0.
    /// Rule: chỉ giữ item khi Σ gems earn (từ quest claim plan, xem <see cref="V2GemsEarned"/>) ≥ tổng chi
    /// đến item đó — item rẻ đứng trước trong plan nên lọc ra danh sách khả mua hợp lý nhất; user không
    /// đủ gems → 0 item (giữ nguyên quest claim, không đổi XP).
    /// </summary>
    private static IReadOnlyList<V2ItemSeed> V2ItemPlan(string persona, int index)
    {
        IReadOnlyList<V2ItemSeed> desired = persona switch
        {
            V2Users.ShowcasePersona =>
            [
                new("avatar-ai-bot", Equip: false, DaysAgo: 1),
                new("avatar-cyber-hacker", Equip: false, DaysAgo: 5),
            ],
            V2Users.HardworkingPersona =>
            [
                new("avatar-ai-bot", Equip: false, DaysAgo: 2),
                new("avatar-cyber-hacker", Equip: false, DaysAgo: 5 + index % 3),
            ],
            V2Users.AveragePersona => [new("avatar-ai-bot", Equip: false, DaysAgo: 1 + index % 4)],
            _ => []
        };

        var earn = V2GemsEarned(persona, index);
        var spend = 0;
        var affordable = new List<V2ItemSeed>(desired.Count);
        foreach (var item in desired)
        {
            var price = V2ShopPrices[item.ItemKey];
            if (spend + price > earn)       // không đủ gems tích lũy → bỏ item (Gems ≥ 0 tuyệt đối)
            {
                continue;
            }

            affordable.Add(item);
            spend += price;
        }

        return affordable;
    }

    /// <summary>Σ gems earn từ quest claim plan (V2QuestDays × V2QuestRows × V2QuestClaims — đồng bộ 100%
    /// với vòng lặp seed UserQuests, gồm cả extra claim Hardworking Task 4b). Dùng để lọc item khả mua.</summary>
    private static int V2GemsEarned(string persona, int index)
    {
        var gems = 0;
        var days = V2QuestDays(persona, index);
        for (var d = 0; d < days; d++)
        {
            var rows = V2QuestRows(persona, index, d);
            var claims = V2QuestClaims(persona, index, d, rows);
            for (var j = 0; j < rows; j++)
            {
                if (claims[j])
                {
                    gems += V2QuestStats[V2QuestKeyAt(persona, index, d, j)].Gems;
                }
            }
        }

        return gems;
    }

    /// <summary>Favorites theo persona — Showcase 5, Hardworking 8-10, Average 3-4, Slacker 0-2, New 0.</summary>
    private static IReadOnlyList<V2FavoriteSeed> V2FavoritePlan(string persona, int index)
    {
        var count = persona switch
        {
            V2Users.ShowcasePersona => 5,
            V2Users.HardworkingPersona => 8 + index % 3,
            V2Users.AveragePersona => 3 + index % 2,
            V2Users.SlackerPersona => index % 3,
            _ => 0
        };

        var list = new List<V2FavoriteSeed>(count);
        for (var k = 0; k < count; k++)
        {
            var key = V2SimulationKeys[(index * 3 + k * 5) % V2SimulationKeys.Length];
            list.Add(new(key, V2FavoriteInputJson(key), 1 + (index + k * 2) % 29));
        }

        return list;
    }

    /// <summary>InputJson mẫu theo loại simulation (khớp format V1).</summary>
    private static string V2FavoriteInputJson(string key) => key switch
    {
        "search.binary" => """{"array":[1,3,5,7,9,11],"target":7}""",
        "tree.bst-insert" => """{"values":[10,5,15,3,7]}""",
        "tree.bst-search" => """{"values":[10,5,15],"target":5}""",
        "tree.avl-insert" => """{"values":[10,20,30]}""",
        "hash.insert" => """{"capacity":8,"keys":[5,13,21]}""",
        "graph.bfs" => """{"vertices":["A","B","C","D"],"edges":[["A","B"],["B","C"],["C","D"]],"start":"A"}""",
        "graph.dfs" => """{"vertices":["0","1","2"],"edges":[["0","1"],["0","2"]],"start":"0"}""",
        "graph.dijkstra" => """{"vertices":["A","B","C"],"edges":[["A","B",2],["B","C",3]],"start":"A"}""",
        "list.insert" => """{"values":[1,2,3],"position":1,"value":9}""",
        "stack.push" => """{"values":[1,2,3]}""",
        "sort.quick" => """{"array":[10,7,8,9,1,5]}""",
        "sort.merge" => """{"array":[38,27,43,3,9,82,10]}""",
        "heap.insert" => """{"values":[4,10,3,5,1]}""",
        _ => """{"array":[5,3,8,4,2]}"""      // sort.bubble / sort.selection / sort.insertion
    };

    /// <summary>ContentFeedback theo persona — Showcase 2, Hardworking 1, Average 0-1 (Index%3==0), còn lại 0.
    /// Rating 1-5, CommentIndex vào SeedData.Feedbacks (comment tiếng Việt ≤ 200 ký tự).</summary>
    private static IReadOnlyList<V2FeedbackSeed> V2FeedbackPlan(string persona, int index)
    {
        var lessons = SeedData.Lessons;
        return persona switch
        {
            V2Users.ShowcasePersona =>
            [
                new(lessons[0].Title, Rating: 5, CommentIndex: 0, DaysAgo: 2),
                new(lessons[5].Title, Rating: 5, CommentIndex: 2, DaysAgo: 5),
            ],
            V2Users.HardworkingPersona =>
            [
                new(lessons[index % lessons.Count].Title, 4 + index % 2, (1 + index % 9) % SeedData.Feedbacks.Count, 1 + index % 10),
            ],
            V2Users.AveragePersona when index % 3 == 0 =>
            [
                new(lessons[index % lessons.Count].Title, 3 + index % 3, index % SeedData.Feedbacks.Count, 1 + index % 8),
            ],
            _ => []
        };
    }

    /// <summary>69 user V2 theo V2Users.All (dict theo Email) — KHÔNG đụng user V1/khác.</summary>
    private static async Task<Dictionary<string, User>> LoadV2ActivityUsersAsync(AppDbContext db, CancellationToken ct)
    {
        var emails = V2Users.All.Select(u => u.Email).ToList();
        var users = await db.Users.AsNoTracking()
            .Where(u => u.Role == UserRole.Student && u.DeletedAt == null && emails.Contains(u.Email))
            .ToListAsync(ct);
        return users.ToDictionary(u => u.Email, StringComparer.OrdinalIgnoreCase);
    }

    // ── 1. UserQuests V2 ──────────────────────────────────────

    /// <summary>
    /// 2856 dòng UserQuests V2 theo persona (Showcase 30 ngày × 5 quest, Hardworking 18-25 ngày × 5,
    /// Average 8-15 ngày × 3-4, Slacker 1-3 ngày × 2 quest cố định, New 0). Claim → recompute
    /// Xp/Gems/StreakDays từ DB rows (pattern V1 Activity.cs:279-297) — không set tay. Guard (UserId, QuestDate, QuestId).
    /// Task 4b: thêm 6 claim (Hardworking Index 0-5, ngày 0) → tổng claim 1300, XP Hardworking vẫn ≤ 1500.
    /// </summary>
    public static async Task SeedUserQuestsV2Async(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var todayLocal = NowUtc7(clock).Date;                       // hôm nay UTC+7
        var quests = await db.DailyQuests.AsNoTracking().ToDictionaryAsync(q => q.QuestKey, ct);
        var items = await db.ShopItems.AsNoTracking().ToDictionaryAsync(i => i.ItemKey, ct);
        var users = await LoadV2ActivityUsersAsync(db, ct);

        var totalAdded = 0;
        var totalSkipped = 0;
        foreach (var seed in V2Users.All)
        {
            if (!users.TryGetValue(seed.Email, out var user))
            {
                logger.LogWarning("Seed: V2 UserQuests bỏ qua {Email} (user chưa tồn tại)", seed.Email);
                continue;
            }

            var addedForUser = 0;
            var skippedForUser = 0;
            var addedClaimed = 0;
            var days = V2QuestDays(seed.Persona, seed.Index);
            for (var d = 0; d < days; d++)
            {
                var questDate = todayLocal.AddDays(-d);
                var rows = V2QuestRows(seed.Persona, seed.Index, d);
                var claims = V2QuestClaims(seed.Persona, seed.Index, d, rows);
                for (var j = 0; j < rows; j++)
                {
                    var key = V2QuestKeyAt(seed.Persona, seed.Index, d, j);
                    if (!quests.TryGetValue(key, out var quest))
                    {
                        logger.LogWarning("Seed: V2 UserQuests bỏ qua {Email}/{Key} (quest template chưa tồn tại)", seed.Email, key);
                        continue;
                    }

                    var exists = await db.UserQuests.AnyAsync(
                        uq => uq.UserId == user.Id && uq.QuestDate == questDate && uq.QuestId == quest.Id, ct);
                    if (exists)
                    {
                        skippedForUser++;
                        continue;
                    }

                    var stats = V2QuestStats[key];
                    db.UserQuests.Add(new UserQuest
                    {
                        UserId = user.Id,
                        QuestId = quest.Id,
                        QuestDate = questDate,
                        Progress = claims[j] ? stats.Progress : Math.Max(0, stats.Progress - 1),
                        Claimed = claims[j]
                    });
                    addedForUser++;
                    if (claims[j])
                    {
                        addedClaimed++;
                    }
                }
            }

            // Chỉ cập nhật Xp/Gems/Streak khi có quest claim MỚI lần chạy này (pattern V1) —
            // giá trị tính LẠI từ DB rows nên luôn bằng tổng reward quest Claimed=1 / gems = earn − spend.
            if (addedClaimed > 0)
            {
                await db.SaveChangesAsync(ct);                       // flush quest rows trước khi tính tổng
                var (xp, gemsEarned) = await SumClaimedRewardsAsync(db, user.Id, quests.Values, ct);
                var gemsSpent = await SumInventorySpendAsync(db, user.Id, items.Values, ct);
                user.Xp = xp;
                user.Gems = gemsEarned - gemsSpent;
                user.StreakDays = V2StreakDays(seed.Persona, seed.Index);
                user.LastActivityDate = todayLocal;
                user.StreakLastProcessed = todayLocal;
                user.UpdatedAt = clock.UtcNow;
                logger.LogInformation(
                    "Seed: V2 UserQuests {Email} ({Persona}): {Added} thêm ({Claimed} claim) → Xp={Xp}, Gems={Gems}, Streak={Streak}",
                    seed.Email, seed.Persona, addedForUser, addedClaimed, user.Xp, user.Gems, user.StreakDays);
            }
            else
            {
                logger.LogInformation("Seed: V2 UserQuests {Email}: {Added} thêm, {Skipped} bỏ qua (đã tồn tại)", seed.Email, addedForUser, skippedForUser);
            }

            totalAdded += addedForUser;
            totalSkipped += skippedForUser;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: V2 UserQuests thêm {Added}, bỏ qua {Skipped} (đã tồn tại)", totalAdded, totalSkipped);
    }

    // ── 2. GemTransactions V2 (append-only) ───────────────────

    /// <summary>
    /// 1360 giao dịch gems V2: quest-earn cho TỪNG quest Claimed=1 (RefId = UserQuest.Id — khớp
    /// ClaimQuestAsync) + shop-spend mua item (RefId = ShopItem.Id, Amount âm). GemTransactions không
    /// có unique key → guard: user chưa có giao dịch RefType="quest"/"shop" nào thì mới thêm (pattern V1).
    /// Task 4b: số spend = số item V2ItemPlan sau lọc ngân sách (60) → 1300 earn + 60 spend = 1360.
    /// </summary>
    public static async Task SeedGemTransactionsV2Async(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var todayLocal = NowUtc7(clock).Date;
        var questsById = await db.DailyQuests.AsNoTracking().ToDictionaryAsync(q => q.Id, ct);
        var items = await db.ShopItems.AsNoTracking().ToDictionaryAsync(i => i.ItemKey, ct);
        var users = await LoadV2ActivityUsersAsync(db, ct);

        var totalAdded = 0;
        var totalSkipped = 0;
        foreach (var seed in V2Users.All)
        {
            if (!users.TryGetValue(seed.Email, out var user))
            {
                continue;
            }

            var addedForUser = 0;

            // Quest earn — 1 dòng cho mỗi quest Claimed=1, RefId = UserQuest.Id
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

            // Shop spend — Type=1, Amount âm (−giá), khớp từng item UserInventory bước 3
            var hasShopTx = await db.GemTransactions.AsNoTracking()
                .AnyAsync(t => t.UserId == user.Id && t.RefType == "shop", ct);
            if (hasShopTx)
            {
                totalSkipped++;
            }
            else
            {
                foreach (var itemDef in V2ItemPlan(seed.Persona, seed.Index))
                {
                    if (!items.TryGetValue(itemDef.ItemKey, out var item))
                    {
                        logger.LogWarning("Seed: V2 GemTransactions bỏ qua {Email}/{ItemKey} (shop item chưa tồn tại)", seed.Email, itemDef.ItemKey);
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
                logger.LogInformation("Seed: V2 GemTransactions {Email} ({Persona}): {Added} thêm", seed.Email, seed.Persona, addedForUser);
            }

            totalAdded += addedForUser;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: V2 GemTransactions thêm {Added}, bỏ qua {Skipped} (đã tồn tại)", totalAdded, totalSkipped);
    }

    // ── 3. UserInventory V2 ────────────────────────────────────

    /// <summary>
    /// 60 dòng UserInventory V2 theo persona SAU lọc ngân sách gems (Showcase 2, Hardworking 2,
    /// Average 1 — Index%8 ≥ 4 trước đây mua thêm avatar-cyber-hacker nhưng earn ≤ 125 gems trong XP
    /// 250-750 không đủ 150 → tự bị lọc bởi <see cref="V2ItemPlan"/>, Slacker/New 0). Item MỚI thêm →
    /// tính lại Gems = earn − spend từ DB rows (pattern V1 Activity.cs:463-474). Guard (UserId, ItemId).
    /// </summary>
    public static async Task SeedUserInventoryV2Async(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var todayLocal = NowUtc7(clock).Date;
        var quests = await db.DailyQuests.AsNoTracking().ToDictionaryAsync(q => q.QuestKey, ct);
        var items = await db.ShopItems.AsNoTracking().ToDictionaryAsync(i => i.ItemKey, ct);
        var users = await LoadV2ActivityUsersAsync(db, ct);

        var totalAdded = 0;
        var totalSkipped = 0;
        foreach (var seed in V2Users.All)
        {
            if (!users.TryGetValue(seed.Email, out var user))
            {
                continue;
            }

            var addedForUser = 0;
            foreach (var itemDef in V2ItemPlan(seed.Persona, seed.Index))
            {
                if (!items.TryGetValue(itemDef.ItemKey, out var item))
                {
                    logger.LogWarning("Seed: V2 UserInventory bỏ qua {Email}/{ItemKey} (shop item chưa tồn tại)", seed.Email, itemDef.ItemKey);
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

            // Item MỚI thêm → tính lại Gems = earn (quest đã claim) − spend (toàn bộ kho) từ DB rows
            if (addedForUser > 0)
            {
                await db.SaveChangesAsync(ct);                       // flush inventory rows trước khi tính spend
                var (_, gemsEarned) = await SumClaimedRewardsAsync(db, user.Id, quests.Values, ct);
                var gemsSpent = await SumInventorySpendAsync(db, user.Id, items.Values, ct);
                user.Gems = gemsEarned - gemsSpent;
                user.UpdatedAt = clock.UtcNow;
                logger.LogInformation(
                    "Seed: V2 UserInventory {Email} ({Persona}): {Added} thêm → Gems={Gems} (earn={Earned}, spend={Spent})",
                    seed.Email, seed.Persona, addedForUser, user.Gems, gemsEarned, gemsSpent);
            }

            totalAdded += addedForUser;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: V2 UserInventory thêm {Added}, bỏ qua {Skipped} (đã tồn tại)", totalAdded, totalSkipped);
    }

    // ── 4. Favorites V2 ────────────────────────────────────────

    /// <summary>~245 Favorites V2 theo persona (Showcase 5, Hardworking 8-10, Average 3-4, Slacker 0-2, New 0)
    /// — SimulationKey hợp lệ shared/simulation-catalog.json + InputJson mẫu; CreatedAt rải 29 ngày. Guard (UserId, SimulationKey).</summary>
    public static async Task SeedFavoritesV2Async(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var todayLocal = NowUtc7(clock).Date;
        var users = await LoadV2ActivityUsersAsync(db, ct);

        var totalAdded = 0;
        var totalSkipped = 0;
        foreach (var seed in V2Users.All)
        {
            if (!users.TryGetValue(seed.Email, out var user))
            {
                continue;
            }

            var addedForUser = 0;
            foreach (var fav in V2FavoritePlan(seed.Persona, seed.Index))
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
                logger.LogInformation("Seed: V2 Favorites {Email} ({Persona}): {Added} thêm", seed.Email, seed.Persona, addedForUser);
            }

            totalAdded += addedForUser;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: V2 Favorites thêm {Added}, bỏ qua {Skipped} (đã tồn tại)", totalAdded, totalSkipped);
    }

    // ── 5. ContentFeedback V2 ──────────────────────────────────

    /// <summary>25 ContentFeedback V2 theo persona (Showcase 2, Hardworking 1, Average Index%3==0 → 1, còn lại 0)
    /// — Rating 1-5, Comment = SeedData.Feedbacks (tiếng Việt ≤ 200 ký tự). Guard (UserId, LessonId).</summary>
    public static async Task SeedContentFeedbackV2Async(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var todayLocal = NowUtc7(clock).Date;
        var lessons = await db.Lessons.AsNoTracking()
            .Where(l => l.DeletedAt == null)
            .ToDictionaryAsync(l => l.Title, ct);
        var users = await LoadV2ActivityUsersAsync(db, ct);

        var totalAdded = 0;
        var totalSkipped = 0;
        foreach (var seed in V2Users.All)
        {
            if (!users.TryGetValue(seed.Email, out var user))
            {
                continue;
            }

            foreach (var fb in V2FeedbackPlan(seed.Persona, seed.Index))
            {
                if (!lessons.TryGetValue(fb.LessonTitle, out var lesson))
                {
                    logger.LogWarning("Seed: V2 ContentFeedback bỏ qua {Email}/{Lesson} (lesson chưa tồn tại)", seed.Email, fb.LessonTitle);
                    continue;
                }

                if (fb.CommentIndex < 0 || fb.CommentIndex >= SeedData.Feedbacks.Count)
                {
                    logger.LogWarning("Seed: V2 ContentFeedback bỏ qua {Email} (CommentIndex {Index} ngoài phạm vi)", seed.Email, fb.CommentIndex);
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
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: V2 ContentFeedback thêm {Added}, bỏ qua {Skipped} (đã tồn tại)", totalAdded, totalSkipped);
    }
}
