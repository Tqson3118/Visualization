using System.Globalization;
using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Services;

/// <summary>
/// GamificationService thật (Module J — SDD §5.4, FR-10.1..10.7, API_REFERENCE.md §4.14).
/// Trừ tim/trừ gems/claim quest dùng UPDATE điều kiện + @@ROWCOUNT (atomic, chống double-spend).
/// Heart regen: Free 30 phút/tim (HeartsMax 10), Premium 10 phút/tim (HeartsMax 30) — FR-10.1.
/// Quests: 5/ngày (2E+2M+1H) chọn theo seed (UserId + ngày) — FR-10.3.
/// Benchmark: client đo (runMeasure), server lưu vào CodeRuns + fit lý thuyết từ catalog.
/// </summary>
public sealed class GamificationService(
    AppDbContext db,
    IDateTimeProvider clock,
    ISimulationCatalogService catalog,
    ILogger<GamificationService> logger) : IGamificationService
{
    private const int SessionMinutes = 30;

    // ── Hearts ────────────────────────────────────────────────

    public async Task<Result<HeartsStatusDto>> GetHeartsAsync(int userId, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        if (user is null)
        {
            return Result<HeartsStatusDto>.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        // F5-Major: truy vấn quá hạn → ghi regen tim xuống DB trước khi trả (elapsed ≥ 1 chu kỳ)
        await PersistHeartRegenAsync(userId, ct);

        return Result<HeartsStatusDto>.Ok(ComputeHearts(user));
    }

    // ── Learning path ─────────────────────────────────────────

    public async Task<Result<LearningPathMapDto>> GetLearningPathAsync(int userId, int pathId, CancellationToken ct)
    {
        var path = await db.LearningPaths.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pathId && p.IsActive, ct);
        if (path is null)
        {
            return Result<LearningPathMapDto>.Fail(ErrorCodes.NOT_FOUND, "Lộ trình học không tồn tại");
        }

        var nodes = await db.LearningPathNodes.AsNoTracking()
            .Where(n => n.PathId == pathId)
            .OrderBy(n => n.SortOrder)
            .ToListAsync(ct);

        var nodeIds = nodes.Select(n => n.Id).ToList();
        var progress = await db.UserNodeProgress.AsNoTracking()
            .Where(p => p.UserId == userId && nodeIds.Contains(p.NodeId))
            .ToDictionaryAsync(p => p.NodeId, ct);

        var passed = nodes.Count(n => progress.TryGetValue(n.Id, out var p) && p.Status == 2);
        var nodeDtos = nodes.Select(n =>
        {
            progress.TryGetValue(n.Id, out var p);
            return new LearningPathNodeDto
            {
                Id = n.Id,
                Title = n.Title,
                SortOrder = n.SortOrder,
                Status = p switch
                {
                    null => "locked",
                    { Status: 0 } => "locked",
                    { Status: 2 } => "passed",
                    _ => "active"
                },
                Stars = p?.Stars ?? 0,
                NodeScore = p?.NodeScore ?? 0
            };
        }).ToList();

        // Node đầu tiên luôn mở (mở khóa tuần tự 1→5 — SDD §7.3.25)
        if (nodeDtos.Count > 0 && nodeDtos[0].Status == "locked")
        {
            nodeDtos[0].Status = "active";
        }

        return Result<LearningPathMapDto>.Ok(new LearningPathMapDto
        {
            Id = path.Id,
            Title = path.Title,
            ProgressPct = nodes.Count == 0 ? 0 : (int)Math.Round(passed * 100.0 / nodes.Count),
            Nodes = nodeDtos
        });
    }

    public async Task<Result<NodeEnterResultDto>> EnterNodeAsync(
        int userId, int pathId, int nodeId, NodeEnterRequest? request, CancellationToken ct)
    {
        var node = await db.LearningPathNodes.AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == nodeId && n.PathId == pathId, ct);
        if (node is null)
        {
            return Result<NodeEnterResultDto>.Fail(ErrorCodes.NOT_FOUND, "Node không tồn tại trong lộ trình");
        }

        var path = await db.LearningPaths.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pathId && p.IsActive, ct);
        if (path is null)
        {
            return Result<NodeEnterResultDto>.Fail(ErrorCodes.NOT_FOUND, "Lộ trình học không tồn tại");
        }

        // Ladder (FR-4.11): phải pass node trước
        if (!await IsPreviousNodePassedAsync(userId, node, ct))
        {
            return Result<NodeEnterResultDto>.Fail(ErrorCodes.LADDER_LOCKED, "Chưa pass bậc trước — không mở bậc sau");
        }

        var nodeProgress = await db.UserNodeProgress.AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == userId && p.NodeId == nodeId, ct);
        var alreadyPassed = nodeProgress?.Status == 2;

        var now = clock.UtcNow;
        var expiresAt = now.AddMinutes(SessionMinutes);

        // 1 transaction: gia hạn session hết hạn / tạo session mới (UNIQUE) → trừ tim (SDD §7.3.29, v2.5)
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // (b) Gia hạn session hết hạn: ROWCOUNT=1 → session mới, KHÔNG trừ tim
        var renewed = await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE NodeSessions SET StartedAt = {now}, ExpiresAt = {expiresAt}, Stage = {request?.Stage}, StepIndex = {request?.StepIndex} WHERE UserId = {userId} AND NodeId = {nodeId} AND ExpiresAt < {now}", ct);

        if (renewed == 0)
        {
            // (c) INSERT — UNIQUE (UserId, NodeId) tuần tự hóa double-spend
            var insertFailed = false;
            try
            {
                db.NodeSessions.Add(new NodeSession
                {
                    UserId = userId,
                    NodeId = nodeId,
                    StartedAt = now,
                    ExpiresAt = expiresAt,
                    Stage = request?.Stage,
                    StepIndex = request?.StepIndex
                });
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateException)
            {
                insertFailed = true;
            }

            if (insertFailed)
            {
                // Unique violation → session còn hiệu lực đã tồn tại → resume, KHÔNG trừ tim (v2.5)
                await tx.RollbackAsync(ct);
                var existing = await db.NodeSessions.AsNoTracking()
                    .FirstAsync(s => s.UserId == userId && s.NodeId == nodeId, ct);
                var heartsLeft = await GetCurrentHeartsAsync(userId, ct);
                return Result<NodeEnterResultDto>.Ok(new NodeEnterResultDto
                {
                    Session = ToSessionDto(existing),
                    HeartsLeft = heartsLeft
                });
            }

            // (d) Trừ tim atomic — UPDATE điều kiện chống double-spend (FR-10.1)
            // Node đã PASS → xem lại miễn phí (AC-10.1.3), KHÔNG trừ
            if (!alreadyPassed)
            {
                // F5-Major: ghi regen tim xuống DB TRƯỚC khi trừ — tránh UI hiện đầy nhưng
                // DB vẫn 0 → HEARTS_EMPTY (UPDATE điều kiện Hearts > 0 sẽ fail dù đã qua chu kỳ regen)
                await PersistHeartRegenAsync(userId, ct);

                var affected = await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE Users SET Hearts = Hearts - 1 WHERE Id = {userId} AND Hearts > 0", ct);
                if (affected == 0)
                {
                    await tx.RollbackAsync(ct);
                    return Result<NodeEnterResultDto>.Fail(ErrorCodes.HEARTS_EMPTY,
                        "Bạn đã hết tim. Hãy chờ hồi hoặc nâng cấp Premium.");
                }
            }

            // Mở khóa node (UserNodeProgress upsert) + eager streak (FR-10.4)
            await EnsureNodeUnlockedAsync(userId, nodeId, now, ct);
            await UpdateStreakEagerAsync(userId, ct);
        }

        await tx.CommitAsync(ct);

        var session = await db.NodeSessions.AsNoTracking()
            .FirstAsync(s => s.UserId == userId && s.NodeId == nodeId, ct);
        var hearts = await GetCurrentHeartsAsync(userId, ct);

        logger.LogInformation("User {UserId} entered node {NodeId} (path {PathId})", userId, nodeId, pathId);
        return Result<NodeEnterResultDto>.Ok(new NodeEnterResultDto
        {
            Session = ToSessionDto(session),
            HeartsLeft = hearts
        });
    }

    public async Task<Result<List<QuestionDto>>> GetFinalTestAsync(int userId, int pathId, CancellationToken ct)
    {
        var path = await db.LearningPaths.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == pathId && p.IsActive, ct);
        if (path is null)
        {
            return Result<List<QuestionDto>>.Fail(ErrorCodes.NOT_FOUND, "Lộ trình học không tồn tại");
        }

        var nodeIds = await db.LearningPathNodes.AsNoTracking()
            .Where(n => n.PathId == pathId)
            .Select(n => n.Id)
            .ToListAsync(ct);

        // Trộn quiz từ exercises của các node (D-3: runtime theo seed, không lưu đề riêng)
        var exerciseIds = await db.Exercises.AsNoTracking()
            .Where(e => e.NodeId != null && nodeIds.Contains(e.NodeId.Value)
                        && e.Type == ExerciseType.Mcq && e.DeletedAt == null)
            .Select(e => e.Id)
            .ToListAsync(ct);

        var questions = await db.Questions.AsNoTracking()
            .Where(q => exerciseIds.Contains(q.ExerciseId))
            .OrderBy(q => q.Id)
            .ToListAsync(ct);

        var seed = HashSeed(pathId, userId, clock.UtcNow.Date);
        var shuffled = questions
            .OrderBy(q => (q.Id * 31 + seed) % 1_000_003)
            .Take(10)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                Content = q.Content,
                Type = q.Type.ToString().ToUpperInvariant(),
                Options = DeserializeOptions(q.OptionsJson),
                Points = q.Points
            })
            .ToList();

        return Result<List<QuestionDto>>.Ok(shuffled);
    }

    // ── Quests ────────────────────────────────────────────────

    public async Task<Result<List<QuestDto>>> GetQuestsAsync(int userId, CancellationToken ct)
    {
        var today = TodayUtc7();

        var pool = await db.DailyQuests.AsNoTracking()
            .Where(q => q.PoolEnabled)
            .OrderBy(q => q.Id)
            .ToListAsync(ct);

        var selected = SelectDailyQuests(pool, userId, today);

        // Đảm bảo 5 quest hôm nay tồn tại trong UserQuests (UNIQUE (UserId, QuestDate, QuestId))
        var existing = await db.UserQuests.AsNoTracking()
            .Where(q => q.UserId == userId && q.QuestDate == today)
            .ToDictionaryAsync(q => q.QuestId, ct);

        foreach (var quest in selected)
        {
            if (!existing.ContainsKey(quest.Id))
            {
                db.UserQuests.Add(new UserQuest
                {
                    UserId = userId,
                    QuestId = quest.Id,
                    QuestDate = today,
                    Progress = 0,
                    Claimed = false
                });
            }
        }

        await db.SaveChangesAsync(ct);

        var all = await db.UserQuests.AsNoTracking()
            .Where(q => q.UserId == userId && q.QuestDate == today)
            .Join(db.DailyQuests.AsNoTracking(), uq => uq.QuestId, dq => dq.Id,
                (uq, dq) => new { uq, dq })
            .ToListAsync(ct);

        var questDtos = all.Select(x => new QuestDto
        {
            Id = x.uq.Id,
            QuestId = x.dq.Id,
            Title = x.dq.Title,
            Type = x.dq.Type,
            Progress = x.uq.Progress,
            Target = GetTarget(x.dq.ConditionJson),
            Claimed = x.uq.Claimed,
            Reward = GetReward(x.dq.RewardJson)
        }).ToList();

        return Result<List<QuestDto>>.Ok(questDtos);
    }

    public async Task<Result<QuestClaimResultDto>> ClaimQuestAsync(int userId, int questId, CancellationToken ct)
    {
        var today = TodayUtc7();

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Claim atomic: UPDATE điều kiện → QUEST_ALREADY_CLAIMED nếu đã claim (FR-10.3, chống claim trùng)
        var claimed = await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE UserQuests SET Claimed = 1 WHERE Id = {questId} AND UserId = {userId} AND QuestDate = {today} AND Claimed = 0", ct);
        if (claimed == 0)
        {
            await tx.RollbackAsync(ct);
            var row = await db.UserQuests.AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == questId && q.UserId == userId, ct);
            return row?.Claimed == true
                ? Result<QuestClaimResultDto>.Fail(ErrorCodes.QUEST_ALREADY_CLAIMED, "Quest này đã được nhận thưởng")
                : Result<QuestClaimResultDto>.Fail(ErrorCodes.NOT_FOUND, "Quest không tồn tại");
        }

        var quest = await db.UserQuests.AsNoTracking()
            .Where(q => q.Id == questId)
            .Join(db.DailyQuests.AsNoTracking(), uq => uq.QuestId, dq => dq.Id,
                (uq, dq) => new { uq, dq })
            .Select(x => new { UserQuestId = x.uq.Id, x.dq.ConditionJson, x.dq.RewardJson })
            .FirstAsync(ct);

        var target = GetTarget(quest.ConditionJson);
        var progress = await db.UserQuests.AsNoTracking()
            .Where(q => q.Id == questId).Select(q => q.Progress).FirstAsync(ct);
        if (progress < target)
        {
            await tx.RollbackAsync(ct);
            return Result<QuestClaimResultDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Quest chưa hoàn thành", new() { ["questId"] = ["Quest chưa hoàn thành"] });
        }

        var reward = GetReward(quest.RewardJson);

        // Cộng gems/xp + log giao dịch CÙNG transaction (SDD §7.3.27)
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Users SET Gems = Gems + {reward.Gems}, Xp = Xp + {reward.Xp} WHERE Id = {userId}", ct);
        db.GemTransactions.Add(new GemTransaction
        {
            UserId = userId,
            Type = 0,                            // earn
            Amount = reward.Gems,
            RefType = "quest",
            RefId = questId.ToString(),
            CreatedAt = clock.UtcNow
        });

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var gemsTotal = await db.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => u.Gems).FirstAsync(ct);
        logger.LogInformation("User {UserId} claimed quest {QuestId}", userId, questId);

        return Result<QuestClaimResultDto>.Ok(new QuestClaimResultDto
        {
            Claimed = true,
            Reward = reward,
            GemsTotal = gemsTotal
        });
    }

    // ── Streak ────────────────────────────────────────────────

    public async Task<Result<StreakDto>> GetStreakAsync(int userId, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        if (user is null)
        {
            return Result<StreakDto>.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        return Result<StreakDto>.Ok(new StreakDto
        {
            StreakDays = user.StreakDays,
            StreakFreeze = user.StreakFreeze
        });
    }

    // ── Leaderboard ───────────────────────────────────────────

    public async Task<Result<PagedResponse<LeaderboardEntryDto>>> GetLeaderboardAsync(
        string tab, int? classId, int page, int pageSize, CancellationToken ct)
    {
        var (safePage, safeSize) = Pagination.Normalize(page, pageSize);

        var query = db.Users.AsNoTracking().Where(u => u.DeletedAt == null && u.Role != UserRole.Admin);

        switch (tab.ToLowerInvariant())
        {
            case "week":
                {
                    // Tuần reset thứ Hai 00:00 UTC+7 (FR-10.6)
                    var nowUtc7 = clock.UtcNow.AddHours(7);
                    var daysSinceMonday = ((int)nowUtc7.DayOfWeek + 6) % 7;
                    var weekStart = nowUtc7.Date.AddDays(-daysSinceMonday).AddHours(-7);
                    query = query.Where(u => u.LastActivityDate != null && u.LastActivityDate >= weekStart);
                    break;
                }
            case "class":
                {
                    if (classId is null)
                    {
                        return Result<PagedResponse<LeaderboardEntryDto>>.Fail(ErrorCodes.VALIDATION_FAILED,
                            "Thiếu classId cho tab lớp", new() { ["classId"] = ["Thiếu classId cho tab lớp"] });
                    }

                    var memberIds = await db.ClassMembers.AsNoTracking()
                        .Where(m => m.ClassId == classId)
                        .Select(m => m.UserId)
                        .ToListAsync(ct);
                    query = query.Where(u => memberIds.Contains(u.Id));
                    break;
                }
            default:   // level
                break;
        }

        var total = await query.CountAsync(ct);
        var rows = await query
            .OrderByDescending(u => u.Xp).ThenBy(u => u.Id)
            .Skip((safePage - 1) * safeSize).Take(safeSize)
            .Select(u => new { u.Id, u.DisplayName, u.Xp })
            .ToListAsync(ct);

        var items = rows.Select((row, index) => new LeaderboardEntryDto
        {
            Rank = (safePage - 1) * safeSize + index + 1,
            UserId = row.Id,
            DisplayName = row.DisplayName,
            Xp = row.Xp,
            Level = ComputeLevel(row.Xp),
            // G-F3E-NEW-1: FE đọc row.value — map theo tab. Cả 3 tab đều xếp hạng theo
            // tổng Xp (week/class chỉ khác bộ lọc user) → Value = Xp là đúng với cách tính hiện có.
            Value = row.Xp
        }).ToList();

        return Result<PagedResponse<LeaderboardEntryDto>>.Ok(
            PagedResponse<LeaderboardEntryDto>.Create(items, safePage, safeSize, total, Pagination.TotalPages(total, safeSize)));
    }

    // ── Shop / Gems ───────────────────────────────────────────

    public async Task<Result<List<ShopItemDto>>> GetShopItemsAsync(int userId, CancellationToken ct)
    {
        var items = await db.ShopItems.AsNoTracking()
            .OrderBy(i => i.PriceGems)
            .ToListAsync(ct);

        var owned = await db.UserInventory.AsNoTracking()
            .Where(i => i.UserId == userId)
            .GroupBy(i => i.ItemId)
            .Select(g => new { ItemId = g.Key, Count = g.Sum(i => i.Quantity) })
            .ToDictionaryAsync(x => x.ItemId, x => x.Count, ct);

        return Result<List<ShopItemDto>>.Ok(items.Select(i => new ShopItemDto
        {
            Id = i.Id,
            ItemKey = i.ItemKey,
            Name = i.Name,
            PriceGems = i.PriceGems,
            MaxStack = i.MaxStack,
            Type = i.Type,
            Owned = owned.GetValueOrDefault(i.Id)
        }).ToList());
    }

    public async Task<Result<ShopBuyResultDto>> BuyItemAsync(int userId, int itemId, CancellationToken ct)
    {
        var item = await db.ShopItems.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == itemId, ct);
        if (item is null)
        {
            return Result<ShopBuyResultDto>.Fail(ErrorCodes.NOT_FOUND, "Vật phẩm không tồn tại");
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Kiểm tra maxStack (FR-10.2)
        var inventory = await db.UserInventory
            .FirstOrDefaultAsync(i => i.UserId == userId && i.ItemId == itemId, ct);
        var currentQty = inventory?.Quantity ?? 0;
        if (currentQty >= item.MaxStack)
        {
            await tx.RollbackAsync(ct);
            return Result<ShopBuyResultDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Đã đạt giới hạn số lượng vật phẩm", new() { ["itemId"] = ["Đã đạt giới hạn số lượng vật phẩm"] });
        }

        // Trừ gems atomic — UPDATE điều kiện chống double-spend (FR-10.2)
        var affected = await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Users SET Gems = Gems - {item.PriceGems} WHERE Id = {userId} AND Gems >= {item.PriceGems}", ct);
        if (affected == 0)
        {
            await tx.RollbackAsync(ct);
            return Result<ShopBuyResultDto>.Fail(ErrorCodes.INSUFFICIENT_GEMS, "Không đủ gems để mua vật phẩm này");
        }

        if (inventory is null)
        {
            db.UserInventory.Add(new UserInventory
            {
                UserId = userId,
                ItemId = itemId,
                Quantity = 1,
                IsEquipped = false,
                PurchasedAt = clock.UtcNow,
                ExpiresAt = item.DurationHours is { } hours ? clock.UtcNow.AddHours(hours) : null
            });
        }
        else
        {
            inventory.Quantity += 1;
        }

        // Log giao dịch CÙNG transaction (append-only, SDD §7.3.27)
        db.GemTransactions.Add(new GemTransaction
        {
            UserId = userId,
            Type = 1,                            // spend
            Amount = item.PriceGems,
            RefType = "shop",
            RefId = itemId.ToString(),
            CreatedAt = clock.UtcNow
        });

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var gemsLeft = await db.Users.AsNoTracking().Where(u => u.Id == userId).Select(u => u.Gems).FirstAsync(ct);
        var newQty = currentQty + 1;
        logger.LogInformation("User {UserId} bought item {ItemId} ({ItemKey})", userId, itemId, item.ItemKey);

        return Result<ShopBuyResultDto>.Ok(new ShopBuyResultDto
        {
            Item = new ShopItemDto
            {
                Id = item.Id,
                ItemKey = item.ItemKey,
                Name = item.Name,
                PriceGems = item.PriceGems,
                MaxStack = item.MaxStack,
                Type = item.Type,
                Owned = newQty
            },
            GemsLeft = gemsLeft,
            Owned = newQty
        });
    }

    // ── Inventory / Equip ─────────────────────────────────────

    public async Task<Result<List<InventoryItemDto>>> GetInventoryAsync(int userId, CancellationToken ct)
    {
        var rows = await db.UserInventory.AsNoTracking()
            .Where(i => i.UserId == userId)
            .Join(db.ShopItems.AsNoTracking(), inv => inv.ItemId, item => item.Id,
                (inv, item) => new InventoryItemDto
                {
                    Id = inv.Id,
                    ItemId = item.Id,
                    ItemKey = item.ItemKey,
                    Name = item.Name,
                    Quantity = inv.Quantity,
                    Type = item.Type,
                    IsEquipped = inv.IsEquipped,
                    ExpiresAt = inv.ExpiresAt
                })
            .OrderBy(i => i.Type).ThenBy(i => i.Name)
            .ToListAsync(ct);

        return Result<List<InventoryItemDto>>.Ok(rows);
    }

    public async Task<Result> EquipItemAsync(int userId, EquipRequest request, CancellationToken ct)
    {
        var row = await db.UserInventory
            .FirstOrDefaultAsync(i => i.UserId == userId && i.ItemId == request.ItemId, ct);
        if (row is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Vật phẩm không có trong kho");
        }

        var item = await db.ShopItems.AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == request.ItemId, ct);
        if (item is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Vật phẩm không tồn tại");
        }

        if (item.Type == 0)
        {
            return Result.Fail(ErrorCodes.VALIDATION_FAILED,
                "Vật phẩm tiêu hao không thể trang bị", new() { ["itemId"] = ["Vật phẩm tiêu hao không thể trang bị"] });
        }

        // v2.9: equip cùng loại → set IsEquipped=false các dòng khác (SDD §7.3.27)
        var sameTypeItemIds = await db.ShopItems.AsNoTracking()
            .Where(i => i.Type == item.Type)
            .Select(i => i.Id)
            .ToListAsync(ct);

        var owned = await db.UserInventory
            .Where(i => i.UserId == userId && sameTypeItemIds.Contains(i.ItemId))
            .ToListAsync(ct);
        foreach (var ownedRow in owned)
        {
            ownedRow.IsEquipped = ownedRow.ItemId == request.ItemId;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("User {UserId} equipped item {ItemId}", userId, request.ItemId);
        return Result.Ok();
    }

    // ── Premium ───────────────────────────────────────────────

    public async Task<Result<PremiumStatusDto>> GetPremiumStatusAsync(int userId, CancellationToken ct)
    {
        var subscription = await db.PremiumSubscriptions.AsNoTracking()
            .Where(s => s.UserId == userId && s.Status != 1)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (subscription is null || subscription.ExpiresAt is null || subscription.ExpiresAt <= clock.UtcNow)
        {
            return Result<PremiumStatusDto>.Ok(new PremiumStatusDto { Status = "none" });
        }

        return Result<PremiumStatusDto>.Ok(new PremiumStatusDto
        {
            PlanId = subscription.PlanId,
            StartedAt = subscription.StartedAt,
            ExpiresAt = subscription.ExpiresAt,
            Status = "active"
        });
    }

    public async Task<Result<PremiumUpgradeResultDto>> UpgradePremiumAsync(int userId, PremiumUpgradeRequest request, CancellationToken ct)
    {
        var months = ParsePlanMonths(request.PlanId);
        if (months is null)
        {
            return Result<PremiumUpgradeResultDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Gói Premium không hợp lệ (1m/3m/12m)", new() { ["planId"] = ["Gói Premium không hợp lệ (1m/3m/12m)"] });
        }

        var now = clock.UtcNow;
        // GP-T7: mã CK tự động DSV{userId}T{months} (VD DSV1002T3) — QR MB Bank + log giao dịch map được.
        // Không còn MOCK-{guid}: OrderRef phải trùng nội dung CK để đối soát.
        var orderRef = $"DSV{userId}T{months.Value}";
        var subscription = new PremiumSubscription
        {
            UserId = userId,
            PlanId = request.PlanId,
            StartedAt = now,
            Status = 2,                          // chờ thanh toán mô phỏng (GP-T7: xác nhận đã chuyển khoản)
            OrderRef = orderRef,
            CreatedAt = now
        };
        db.PremiumSubscriptions.Add(subscription);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Premium order {OrderId} created for user {UserId} ({PlanId}, orderRef={OrderRef})", subscription.Id, userId, request.PlanId, orderRef);
        return Result<PremiumUpgradeResultDto>.Ok(new PremiumUpgradeResultDto
        {
            OrderId = subscription.Id,
            PlanId = request.PlanId,
            ExpiresAt = now.AddMonths(months.Value),
            ContentRef = orderRef
        });
    }

    public async Task<Result<PremiumStatusDto>> MockPayAsync(int userId, PremiumMockPayRequest request, CancellationToken ct)
    {
        var subscription = await db.PremiumSubscriptions
            .FirstOrDefaultAsync(s => s.Id == request.OrderId && s.UserId == userId, ct);
        if (subscription is null)
        {
            return Result<PremiumStatusDto>.Fail(ErrorCodes.NOT_FOUND, "Đơn hàng không tồn tại");
        }

        var months = ParsePlanMonths(subscription.PlanId ?? string.Empty);
        if (months is null)
        {
            return Result<PremiumStatusDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Gói Premium không hợp lệ", new() { ["planId"] = ["Gói Premium không hợp lệ"] });
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var user = await db.Users.FirstAsync(u => u.Id == userId, ct);
        var baseTime = user.PremiumUntil > clock.UtcNow ? user.PremiumUntil.Value : clock.UtcNow;
        var expiresAt = baseTime.AddMonths(months.Value);

        subscription.Status = 0;                 // active
        subscription.ExpiresAt = expiresAt;
        user.PremiumUntil = expiresAt;
        user.HeartsMax = 30;                     // Premium 30 tim (FR-10.7)

        db.GemTransactions.Add(new GemTransaction
        {
            UserId = userId,
            Type = 0,
            Amount = 0,
            RefType = "premium",
            RefId = request.OrderId.ToString(),
            CreatedAt = clock.UtcNow
        });

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        logger.LogInformation("Premium activated for user {UserId} until {ExpiresAt}", userId, expiresAt);
        return Result<PremiumStatusDto>.Ok(new PremiumStatusDto
        {
            PlanId = subscription.PlanId,
            StartedAt = subscription.StartedAt,
            ExpiresAt = expiresAt,
            Status = "active"
        });
    }

    // ── Cheatsheet ────────────────────────────────────────────

    public async Task<Result<List<CheatsheetItemDto>>> GetCheatsheetAsync(string? structure, CancellationToken ct)
    {
        var items = await catalog.GetListAsync(ct);
        if (!items.IsSuccess)
        {
            return Result<List<CheatsheetItemDto>>.Fail(items.ErrorCode!, items.ErrorMessage!);
        }

        var list = items.Value!
            .Where(s => string.IsNullOrWhiteSpace(structure) || s.DataStructure.Contains(structure, StringComparison.OrdinalIgnoreCase))
            .Select(s => new CheatsheetItemDto
            {
                Key = s.Key,
                Name = s.Title,
                DataStructure = s.DataStructure,
                Complexity = s.Complexity,
                DeepLink = $"/simulations/{s.Key}"
            })
            .OrderBy(s => s.Key)
            .ToList();

        return Result<List<CheatsheetItemDto>>.Ok(list);
    }

    // ── Benchmark ─────────────────────────────────────────────

    public async Task<Result<BenchmarkRunResponse>> RunBenchmarkAsync(int userId, BenchmarkRequest request, CancellationToken ct)
    {
        if (request.Keys.Count == 0 || request.Sizes.Count == 0)
        {
            return Result<BenchmarkRunResponse>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Thiếu keys/sizes cho benchmark", new() { ["keys"] = ["Thiếu keys/sizes cho benchmark"] });
        }

        if (request.Results is not { Count: > 0 })
        {
            // Server KHÔNG đo được (chạy client sandbox — SDD §4.0.3); cần kết quả đo client gửi lên
            return Result<BenchmarkRunResponse>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Thiếu kết quả đo (chạy đo phía client, gửi kèm results)", new() { ["results"] = ["Thiếu kết quả đo (chạy đo phía client)"] });
        }

        // Lưu lần benchmark vào CodeRuns (bảng lưu lần chạy sẵn có — FR-9.2)
        db.CodeRuns.Add(new CodeRun
        {
            UserId = userId,
            Code = string.Join(',', request.Keys),
            InputJson = JsonSerializer.Serialize(new { sizes = request.Sizes, language = request.Language }),
            Status = CodeRunStatus.Success,
            OutputJson = JsonSerializer.Serialize(request.Results),
            DurationMs = 0,
            CreatedAt = clock.UtcNow
        });
        await db.SaveChangesAsync(ct);

        var fitted = new Dictionary<string, string>();
        foreach (var key in request.Keys)
        {
            var meta = await catalog.GetByKeyAsync(key, ct);
            fitted[key] = meta.IsSuccess ? meta.Value!.Complexity.Average : "O(n log n)";
        }

        var conclusion = BuildConclusion(request.Results);
        logger.LogInformation("Benchmark run saved for user {UserId}: {Keys}", userId, string.Join(',', request.Keys));

        return Result<BenchmarkRunResponse>.Ok(new BenchmarkRunResponse
        {
            Results = request.Results,
            Fitted = fitted,
            Conclusion = conclusion
        });
    }

    // ── Private ───────────────────────────────────────────────

    private HeartsStatusDto ComputeHearts(User user)
    {
        var now = clock.UtcNow;
        var (maxHearts, regenSeconds) = HeartConfig(user, now);

        var lastHeartAt = user.LastHeartAt == default ? user.CreatedAt : user.LastHeartAt;
        var elapsedSeconds = (long)(now - lastHeartAt).TotalSeconds;
        var regenCount = elapsedSeconds / regenSeconds;
        var hearts = Math.Min(maxHearts, user.Hearts + (int)regenCount);

        var nextHeartInSeconds = hearts >= maxHearts
            ? 0
            : Math.Max(0, (int)(regenSeconds - (elapsedSeconds % regenSeconds)));

        return new HeartsStatusDto
        {
            Hearts = hearts,
            HeartsMax = maxHearts,
            LastHeartAt = lastHeartAt,
            NextHeartInSeconds = nextHeartInSeconds
        };
    }

    /// <summary>
    /// Cấu hình tim: Premium 10 phút/tim (max 30), Free 30 phút/tim (max HeartsMax) — FR-10.1, SDD §8.4A.
    /// </summary>
    private static (int MaxHearts, int RegenSeconds) HeartConfig(User user, DateTime now)
    {
        var isPremium = user.PremiumUntil > now;
        return (isPremium ? 30 : user.HeartsMax, isPremium ? 600 : 1800);
    }

    /// <summary>
    /// Ghi regen tim xuống DB (F5-Major): elapsed ≥ 1 chu kỳ regen → UPDATE Hearts + LastHeartAt.
    /// Dùng raw SQL (không qua EF tracking) để khớp chuỗi UPDATE atomic trừ tim.
    /// Tim đã đầy → không cập nhật (tránh dời LastHeartAt làm sai NextHeartInSeconds).
    /// </summary>
    private async Task PersistHeartRegenAsync(int userId, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == userId, ct);
        var now = clock.UtcNow;
        var (maxHearts, regenSeconds) = HeartConfig(user, now);

        var lastHeartAt = user.LastHeartAt == default ? user.CreatedAt : user.LastHeartAt;
        var elapsedSeconds = (long)(now - lastHeartAt).TotalSeconds;
        var regenCount = (int)(elapsedSeconds / regenSeconds);

        if (regenCount <= 0 || user.Hearts >= maxHearts)
        {
            return;
        }

        var hearts = Math.Min(maxHearts, user.Hearts + regenCount);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Users SET Hearts = {hearts}, LastHeartAt = {now} WHERE Id = {userId}", ct);
    }

    private async Task<int> GetCurrentHeartsAsync(int userId, CancellationToken ct)
    {
        // F5-Major: ghi regen xuống DB trước khi đọc — số tim trả về luôn khớp DB (không còn "tim ảo")
        await PersistHeartRegenAsync(userId, ct);
        var user = await db.Users.AsNoTracking()
            .FirstAsync(u => u.Id == userId, ct);
        return ComputeHearts(user).Hearts;
    }

    private async Task<bool> IsPreviousNodePassedAsync(int userId, LearningPathNode node, CancellationToken ct)
    {
        var isFirst = !await db.LearningPathNodes.AsNoTracking()
            .AnyAsync(n => n.PathId == node.PathId && n.SortOrder < node.SortOrder, ct);
        if (isFirst)
        {
            return true;
        }

        var previous = await db.LearningPathNodes.AsNoTracking()
            .Where(n => n.PathId == node.PathId && n.SortOrder < node.SortOrder)
            .OrderByDescending(n => n.SortOrder)
            .FirstOrDefaultAsync(ct);
        if (previous is null)
        {
            return true;
        }

        return await db.UserNodeProgress.AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.NodeId == previous.Id && p.Status == 2, ct);
    }

    private async Task EnsureNodeUnlockedAsync(int userId, int nodeId, DateTime now, CancellationToken ct)
    {
        var exists = await db.UserNodeProgress.AsNoTracking()
            .AnyAsync(p => p.UserId == userId && p.NodeId == nodeId, ct);
        if (!exists)
        {
            db.UserNodeProgress.Add(new UserNodeProgress
            {
                UserId = userId,
                NodeId = nodeId,
                Status = 1,                      // Unlocked
                Stars = 0,
                NodeScore = 0,
                UnlockedAt = now,
                UpdatedAt = now
            });
        }
    }

    /// <summary>Streak EAGER (FR-10.4 v2.8): LastActivityDate hôm qua → +1; hôm nay → giữ; cũ hơn → dùng freeze hoặc reset.</summary>
    private async Task UpdateStreakEagerAsync(int userId, CancellationToken ct)
    {
        var user = await db.Users.FirstAsync(u => u.Id == userId, ct);
        var today = TodayUtc7();
        var yesterday = today.AddDays(-1);

        if (user.LastActivityDate == today)
        {
            return;
        }

        if (user.LastActivityDate == yesterday)
        {
            user.StreakDays += 1;
        }
        else if (user.LastActivityDate is null)
        {
            user.StreakDays = 1;
        }
        else
        {
            // Nghỉ nhiều ngày: dùng freeze (tối đa 1 freeze/ngày) hoặc reset về 1
            if (user.StreakFreeze > 0)
            {
                user.StreakFreeze -= 1;
                user.StreakDays += 1;
            }
            else
            {
                user.StreakDays = 1;
            }
        }

        user.LastActivityDate = today;
        user.StreakLastProcessed ??= today;
    }

    private static List<DailyQuest> SelectDailyQuests(List<DailyQuest> pool, int userId, DateTime date)
    {
        if (pool.Count == 0)
        {
            return [];
        }

        var seed = HashSeed(userId, 0, date);
        var easy = pool.Where(q => q.Type == 0).OrderBy(q => (q.Id * 31 + seed) % 1_000_003).Take(2).ToList();
        var medium = pool.Where(q => q.Type == 1).OrderBy(q => (q.Id * 31 + seed) % 1_000_003).Take(2).ToList();
        var hard = pool.Where(q => q.Type == 2).OrderBy(q => (q.Id * 31 + seed) % 1_000_003).Take(1).ToList();

        // Pool thiếu template → lấp bằng quest bất kỳ
        var selected = easy.Concat(medium).Concat(hard).ToList();
        if (selected.Count < 5)
        {
            var used = selected.Select(q => q.Id).ToHashSet();
            selected.AddRange(pool.Where(q => !used.Contains(q.Id))
                .OrderBy(q => (q.Id * 17 + seed) % 1_000_003)
                .Take(5 - selected.Count));
        }

        return selected;
    }

    private static int HashSeed(int a, int b, DateTime date)
    {
        unchecked
        {
            var hash = date.Year * 10000 + date.Month * 100 + date.Day;
            hash = (hash * 397) ^ a;
            hash = (hash * 397) ^ b;
            return Math.Abs(hash);
        }
    }

    private static DateTime TodayUtc7() => DateTime.UtcNow.AddHours(7).Date;

    private static int GetTarget(string conditionJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(conditionJson);
            return doc.RootElement.TryGetProperty("count", out var count) && count.TryGetInt32(out var value)
                ? value
                : 1;
        }
        catch (JsonException)
        {
            return 1;
        }
    }

    private static QuestRewardDto GetReward(string rewardJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(rewardJson);
            var root = doc.RootElement;
            return new QuestRewardDto
            {
                Gems = root.TryGetProperty("gems", out var gems) && gems.TryGetInt32(out var g) ? g : 0,
                Xp = root.TryGetProperty("xp", out var xp) && xp.TryGetInt32(out var x) ? x : 0
            };
        }
        catch (JsonException)
        {
            return new QuestRewardDto();
        }
    }

    private static int ComputeLevel(int xp) => 1 + (int)Math.Floor(Math.Sqrt(xp / 100.0));

    private static int? ParsePlanMonths(string planId) => planId.Trim().ToLowerInvariant() switch
    {
        "1m" or "1" => 1,
        "3m" or "3" => 3,
        "12m" or "12" => 12,
        _ => null
    };

    private static NodeSessionDto ToSessionDto(NodeSession session) => new()
    {
        Id = session.Id,
        NodeId = session.NodeId,
        StartedAt = session.StartedAt,
        ExpiresAt = session.ExpiresAt,
        Stage = session.Stage,
        StepIndex = session.StepIndex
    };

    private static List<string> DeserializeOptions(string optionsJson)
    {
        try
        {
            return JsonSerializer.Deserialize<List<string>>(optionsJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string BuildConclusion(List<BenchmarkResultDto> results)
    {
        if (results.Count == 0)
        {
            return string.Empty;
        }

        var withMeasurements = results
            .Where(r => r.Measurements.Count > 0)
            .Select(r => new
            {
                r.Key,
                Max = r.Measurements.Max(m => m.N),
                DurationAtMax = r.Measurements.Where(m => m.N == r.Measurements.Max(x => x.N)).Min(m => m.DurationMs)
            })
            .ToList();

        if (withMeasurements.Count == 0)
        {
            return "Không có dữ liệu đo.";
        }

        var fastest = withMeasurements.OrderBy(x => x.DurationAtMax).First();
        var parts = new List<string>();

        if (withMeasurements.Count > 1)
        {
            foreach (var other in withMeasurements.Where(x => x.Key != fastest.Key))
            {
                var ratio = other.DurationAtMax > 0 ? fastest.DurationAtMax / other.DurationAtMax : 0;
                var verdict = ratio > 0.6 ? "tương đương" : "nhanh hơn rõ rệt";
                parts.Add($"{fastest.Key} {verdict} so với {other.Key} ở n ≥ {other.Max}");
            }
        }

        parts.Add($"thời gian đo tại n={fastest.Max}: {fastest.DurationAtMax.ToString("0.##", CultureInfo.InvariantCulture)} ms");
        return string.Join("; ", parts) + ".";
    }
}
