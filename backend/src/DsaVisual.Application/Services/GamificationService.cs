using System.Globalization;
using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.Data.SqlClient;
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
    ILogger<GamificationService> logger,
    IGamificationConfigService? configService = null) : IGamificationService
{
    private readonly IGamificationConfigService _configService = configService ?? new GamificationConfigService();

 
    // ── Hearts ────────────────────────────────────────────────
 
    public async Task<Result<HeartsStatusDto>> GetHeartsAsync(int userId, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        if (user is null)
        {
            return Result<HeartsStatusDto>.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        // BUG-1 (lazy downgrade): premium hết hạn → clamp HeartsMax/Hearts về Free NGAY khi đọc (FR-10.7 AC-10.7.2/3)
        await EnsureHeartsMaxSyncAsync(user, ct);

        // F5-Major: truy vấn quá hạn → ghi regen tim xuống DB trước khi trả (elapsed ≥ 1 chu kỳ)
        await PersistHeartRegenAsync(userId, ct);

        return Result<HeartsStatusDto>.Ok(ComputeHearts(user));
    }

    public async Task<Result<HeartsStatusDto>> SpendHeartAsync(int userId, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        if (user is null)
        {
            return Result<HeartsStatusDto>.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        await EnsureHeartsMaxSyncAsync(user, ct);
        await PersistHeartRegenAsync(user, ct);

        var now = clock.UtcNow;
        var affected = await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Users SET Hearts = Hearts - 1, LastHeartAt = {now} WHERE Id = {userId} AND Hearts > 0", ct);
        if (affected == 0)
        {
            return Result<HeartsStatusDto>.Fail(ErrorCodes.HEARTS_EMPTY,
                "Bạn cần ít nhất 1 tim để đăng ký lộ trình. Hãy chờ hồi hoặc nâng cấp Premium.");
        }

        var updatedUser = await db.Users.AsNoTracking().FirstAsync(u => u.Id == userId, ct);
        return Result<HeartsStatusDto>.Ok(ComputeHearts(updatedUser));
    }

    // ── Gamification summary (level + XP — hồi đáp UI) ──────

    public async Task<Result<GamificationSummaryDto>> GetGamificationSummaryAsync(int userId, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking()
            .Where(u => u.Id == userId && u.DeletedAt == null)
            .Select(u => new { u.Xp, u.Gems })
            .FirstOrDefaultAsync(ct);

        if (user is null)
        {
            return Result<GamificationSummaryDto>.Ok(new GamificationSummaryDto
            {
                Xp = 0,
                Level = 1,
                XpIntoLevel = 0,
                XpForNextLevel = 100,
                LevelProgressPct = 0,
                Gems = 0
            });
        }

        var xp = user.Xp;
        var level = ComputeLevel(xp);
        var xpFloor = (level - 1) * (level - 1) * 100;                  // XP tối thiểu của level hiện tại
        var xpIntoLevel = Math.Max(0, xp - xpFloor);
        var xpForNextLevel = 100 * (2 * level - 1);                    // (level^2 - (level-1)^2)*100
        var levelProgressPct = xpForNextLevel <= 0
            ? 0
            : (int)Math.Round(Math.Clamp((double)xpIntoLevel / xpForNextLevel, 0, 1) * 100);

        return Result<GamificationSummaryDto>.Ok(new GamificationSummaryDto
        {
            Xp = xp,
            Level = level,
            XpIntoLevel = xpIntoLevel,
            XpForNextLevel = xpForNextLevel,
            LevelProgressPct = levelProgressPct,
            Gems = user.Gems
        });
    }

    // ── Learning path ─────────────────────────────────────────

    /// <summary>Danh sách path ACTIVE (selector /path) kèm tiến độ user + số node (FR-2.10).</summary>
    public async Task<Result<List<LearningPathSummaryDto>>> GetLearningPathsAsync(int userId, CancellationToken ct)
    {
        var paths = await db.LearningPaths.AsNoTracking()
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .Select(p => new { p.Id, p.Title, p.Description, p.TopicId, p.SortOrder, p.IsActive })
            .ToListAsync(ct);

        if (paths.Count == 0)
        {
            return Result<List<LearningPathSummaryDto>>.Ok([]);
        }

        var pathIds = paths.Select(p => p.Id).ToList();
        var allNodes = await db.LearningPathNodes.AsNoTracking()
            .Where(n => pathIds.Contains(n.PathId) && n.ItemType != PathItemType.Folder)
            .Select(n => new { n.Id, n.PathId })
            .ToListAsync(ct);

        var nodeIds = allNodes.Select(n => n.Id).ToList();
        var passedNodeIds = nodeIds.Count == 0
            ? new HashSet<int>()
            : (await db.UserNodeProgress.AsNoTracking()
                .Where(up => up.UserId == userId && up.Status == 2 && nodeIds.Contains(up.NodeId))
                .Select(up => up.NodeId)
                .ToListAsync(ct))
                .ToHashSet();

        var nodesByPath = allNodes.GroupBy(n => n.PathId)
            .ToDictionary(g => g.Key, g => g.Select(n => n.Id).ToList());

        var summaries = paths.Select(p =>
        {
            var nodes = nodesByPath.GetValueOrDefault(p.Id, []);
            var passed = nodes.Count(id => passedNodeIds.Contains(id));
            return new LearningPathSummaryDto
            {
                Id = p.Id,
                Title = p.Title,
                Description = p.Description ?? string.Empty,
                TopicId = p.TopicId,
                SortOrder = p.SortOrder,
                ProgressPct = nodes.Count == 0 ? 0 : (int)Math.Round(passed * 100.0 / nodes.Count),
                NodeCount = nodes.Count
            };
        }).ToList();

        return Result<List<LearningPathSummaryDto>>.Ok(summaries);
    }

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

        var studyNodes = nodes.Where(n => n.ItemType != PathItemType.Folder).ToList();
        var studyPassed = studyNodes.Count(n => progress.TryGetValue(n.Id, out var p) && p.Status == 2);
        var progressPct = studyNodes.Count == 0 ? 0 : (int)Math.Round(studyPassed * 100.0 / studyNodes.Count);

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

        // Mở khóa tuần tự theo Ladder: Node 1 luôn mở. Các node sau mở nếu node liền trước đã pass.
        for (var i = 0; i < nodeDtos.Count; i++)
        {
            if (i == 0)
            {
                if (nodeDtos[0].Status == "locked")
                {
                    nodeDtos[0].Status = "active";
                }
            }
            else if (nodeDtos[i].Status == "locked" && nodeDtos[i - 1].Status == "passed")
            {
                nodeDtos[i].Status = "active";
            }
        }

        return Result<LearningPathMapDto>.Ok(new LearningPathMapDto
        {
            Id = path.Id,
            Title = path.Title,
            ProgressPct = progressPct,
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
            .FirstOrDefaultAsync(p => p.Id == pathId, ct);
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
        var expiresAt = now.AddHours(_configService.GetSessionHours());

        // 1 transaction: gia hạn session hết hạn / tạo session mới (UNIQUE) → trừ tim (SDD §7.3.29, v2.5)
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // (b) Gia hạn session hết hạn: ROWCOUNT=1 → session mới
        var renewed = await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE NodeSessions SET StartedAt = {now}, ExpiresAt = {expiresAt}, Stage = {request?.Stage}, StepIndex = {request?.StepIndex} WHERE UserId = {userId} AND NodeId = {nodeId} AND ExpiresAt < {now}", ct);

        if (renewed > 0)
        {
            // Hết hạn + chưa pass → TRỪ 1 tim; Hết hạn + đã pass → xem lại FREE (FEAT-02)
            if (!alreadyPassed)
            {
                var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == userId, ct);
                await PersistHeartRegenAsync(user, ct);

                var affected = await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE Users SET Hearts = Hearts - 1 WHERE Id = {userId} AND Hearts > 0", ct);
                if (affected == 0)
                {
                    await tx.RollbackAsync(ct);
                    return Result<NodeEnterResultDto>.Fail(ErrorCodes.HEARTS_EMPTY,
                        "Bạn đã hết tim. Hãy chờ hồi hoặc nâng cấp Premium.");
                }
            }
        }
        else
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
            catch (DbUpdateException ex) when (IsUniqueViolationForNodeSession(ex))
            {
                // exc#3: CHỈ nuốt unique violation của NodeSessions (session còn hiệu lực → resume);
                // lỗi DB khác (mất kết nối, schema thay đổi, trigger/constraint khác) rethrow →
                // ErrorHandlingMiddleware log đúng 1 lần tại boundary + trả 503/409 đúng ngữ nghĩa.
                insertFailed = true;
            }

            if (insertFailed)
            {
                // Unique violation → session còn hiệu lực đã tồn tại → resume, KHÔNG trừ tim (v2.5)
                logger.LogDebug("EnterNode unique violation on NodeSessions (UserId={UserId}, NodeId={NodeId}) — resume existing session", userId, nodeId);
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
                // perf#22: đọc Users MỘT lần rồi truyền entity qua (trước đây PersistHeartRegenAsync
                // tự đọc lại → 2 lần đọc Users + có thể 2 UPDATE regen trong cùng 1 request).
                var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == userId, ct);
                await PersistHeartRegenAsync(user, ct);

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

            // Persist thay đổi tracked (mở khóa node + streak) CÙNG transaction. Latent bug: trước đây
            // không có SaveChanges sau EnsureNodeUnlockedAsync/UpdateStreakEagerAsync nên unlock node +
            // streak không bao giờ được lưu (lộ ra qua finding #6 — sync quest đọc UserNodeProgress từ DB).
            try
            {
                await db.SaveChangesAsync(ct);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Finding #3: dòng Users/UserNodeProgress đổi (RowVersion) giữa lúc đọc và ghi → 409
                await tx.RollbackAsync(ct);
                return Result<NodeEnterResultDto>.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
            }
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

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Finding #5: 2 GET /me/quests song song đầu ngày → INSERT trùng UNIQUE(UserId,QuestDate,QuestId)
            // của request kia → quest đã tồn tại → bỏ qua, tiếp tục đọc (GET KHÔNG trả 500).
        }

        var all = (await db.UserQuests.AsNoTracking()
            .Where(q => q.UserId == userId && q.QuestDate == today)
            .Join(db.DailyQuests.AsNoTracking(), uq => uq.QuestId, dq => dq.Id,
                (uq, dq) => new { uq, dq })
            .ToListAsync(ct))
            .Select(x => new QuestRow(x.uq, x.dq))
            .ToList();

        // Finding #6 (FR-10.3): Progress quest TỰ cập nhật theo sự kiện học tập — lưới an toàn khi đọc
        // (chỉ tăng, không giảm) cho các activity map được từ trạng thái thật trong DB.
        await SyncQuestProgressAsync(userId, today, all, ct);

        var questDtos = all.Select(x => new QuestDto
        {
            Id = x.Uq.Id,
            QuestId = x.Dq.Id,
            Title = x.Dq.Title,
            Type = x.Dq.Type,
            Progress = x.Uq.Progress,
            Target = GetTarget(x.Dq.ConditionJson),
            Claimed = x.Uq.Claimed,
            Reward = GetReward(x.Dq.RewardJson)
        }).ToList();

        return Result<List<QuestDto>>.Ok(questDtos);
    }

    /// <summary>Bản ghi quest hôm nay + template (dùng cho sync Progress — finding #6).</summary>
    private sealed record QuestRow(UserQuest Uq, DailyQuest Dq);

    /// <summary>
    /// Finding #6 (FR-10.3 — "tiến độ TỰ cập nhật theo sự kiện học tập"): sync Progress từ trạng thái THẬT
    /// khi đọc quest — chỉ TĂNG (UPDATE ... WHERE Progress &lt; actual), không bao giờ giảm. Event hooks
    /// (QuestProgressWriter.IncrementAsync từ SubmitAsync/SubmitCodeAsync/MarkViewedAsync) tăng +1 ngay khi
    /// xảy ra; bước này đảm bảo kể cả khi event bị bỏ sót, Progress vẫn đúng khi user mở /me/quests.
    /// Map ConditionJson key "activity" (xem SeedData.Quests — 8 template FR-10.3A):
    ///   pass_node      → UserNodeProgress Status=2 (PassedAt trong ngày)
    ///   pass_quiz      → ExerciseSubmissions hôm nay của exercise type MCQ
    ///   pass_lab       → ExerciseSubmissions hôm nay của exercise type SimulationLab
    ///   code_run       → CodeSubmissions hôm nay
    ///   lesson_viewed  → UserProgress.Viewed (UpdatedAt trong ngày — cũng được set khi nộp bài)
    ///   streak         → KHÔNG sync: StreakDays là giá trị tích lũy, không phải sự kiện trong ngày
    /// </summary>
    private async Task SyncQuestProgressAsync(int userId, DateTime today, List<QuestRow> quests, CancellationToken ct)
    {
        var byActivity = new Dictionary<string, List<QuestRow>>(StringComparer.Ordinal);
        foreach (var row in quests)
        {
            var activity = GetQuestActivity(row.Dq.ConditionJson);
            if (activity is null)
            {
                continue;
            }

            if (!byActivity.TryGetValue(activity, out var list))
            {
                byActivity[activity] = list = [];
            }

            list.Add(row);
        }

        if (byActivity.Count == 0)
        {
            return;
        }

        var dayStartUtc = today.AddHours(-7);      // 00:00 UTC+7 = 17:00 UTC hôm trước (PassedAt/SubmittedAt/UpdatedAt lưu UTC)
        var dayEndUtc = dayStartUtc.AddDays(1);
        var counts = new Dictionary<string, int>(StringComparer.Ordinal);

        if (byActivity.ContainsKey("pass_node"))
        {
            counts["pass_node"] = await db.UserNodeProgress.AsNoTracking()
                .CountAsync(p => p.UserId == userId && p.Status == 2 && p.PassedAt >= dayStartUtc && p.PassedAt < dayEndUtc, ct);
        }

        if (byActivity.ContainsKey("pass_quiz") || byActivity.ContainsKey("pass_lab"))
        {
            var submissions = db.ExerciseSubmissions.AsNoTracking()
                .Where(s => s.UserId == userId && s.SubmittedAt >= dayStartUtc && s.SubmittedAt < dayEndUtc)
                .Join(db.Exercises.AsNoTracking(), s => s.ExerciseId, e => e.Id, (s, e) => e.Type);
            if (byActivity.ContainsKey("pass_quiz"))
            {
                counts["pass_quiz"] = await submissions.CountAsync(t => t == ExerciseType.Mcq, ct);
            }

            if (byActivity.ContainsKey("pass_lab"))
            {
                counts["pass_lab"] = await submissions.CountAsync(t => t == ExerciseType.SimulationLab, ct);
            }
        }

        if (byActivity.ContainsKey("code_run"))
        {
            counts["code_run"] = await db.CodeSubmissions.AsNoTracking()
                .CountAsync(s => s.UserId == userId && s.SubmittedAt >= dayStartUtc && s.SubmittedAt < dayEndUtc, ct);
        }

        if (byActivity.ContainsKey("lesson_viewed"))
        {
            counts["lesson_viewed"] = await db.UserProgress.AsNoTracking()
                .CountAsync(p => p.UserId == userId && p.Viewed && p.UpdatedAt >= dayStartUtc && p.UpdatedAt < dayEndUtc, ct);
        }

        foreach (var (activity, rows) in byActivity)
        {
            if (!counts.TryGetValue(activity, out var actual) || actual <= 0)
            {
                continue;
            }

            foreach (var row in rows)
            {
                if (row.Uq.Progress >= actual)
                {
                    continue;
                }

                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE UserQuests SET Progress = {actual} WHERE Id = {row.Uq.Id} AND Progress < {actual}", ct);
                row.Uq.Progress = actual;   // DTO đọc đúng giá trị vừa sync
            }
        }
    }

    private static string? GetQuestActivity(string conditionJson)
    {
        try
        {
            using var doc = JsonDocument.Parse(conditionJson);
            return doc.RootElement.TryGetProperty("activity", out var activity) && activity.ValueKind == JsonValueKind.String
                ? activity.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
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
        string tab, int? classId, int page, int pageSize, CancellationToken ct = default,
        int? lastXp = null, int? lastId = null)
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

                    // perf#11: EXISTS subquery thay vì tải toàn bộ memberIds về app rồi IN-list
                    // (1 round-trip; lớp đông không tạo IN-list lớn) — index (ClassId, UserId) có sẵn.
                    query = query.Where(u => db.ClassMembers.AsNoTracking()
                        .Any(m => m.ClassId == classId && m.UserId == u.Id));
                    break;
                }
            default:   // level
                break;
        }

        var total = await query.CountAsync(ct);

        // perf#4: keyset/cursor — (Xp < @lastXp) OR (Xp = @lastXp AND Id < @lastId) ORDER BY Xp DESC, Id DESC.
        // lastXp/lastId = phần tử CUỐI trang trước (client truyền kèm page). Không có cursor → fallback
        // offset (contract page/size cũ giữ nguyên — test bảo vệ không đổi). ThenByDescending(Id) làm
        // tie-break Xp trùng DETERMINISTIC (khớp keyset; trước đây ThenBy(Id) — thứ tự tie không chốt).
        var ordered = query.OrderByDescending(u => u.Xp).ThenByDescending(u => u.Id);
        IQueryable<User> paged;
        if (lastXp is not null && lastId is not null)
        {
            paged = ordered.Where(u => u.Xp < lastXp.Value || (u.Xp == lastXp.Value && u.Id < lastId.Value));
        }
        else
        {
            paged = ordered.Skip((safePage - 1) * safeSize);
        }

        var rows = await paged
            .Take(safeSize)
            .Select(u => new { u.Id, u.DisplayName, u.Xp })
            .ToListAsync(ct);

        Dictionary<int, int> weeklyScores = new();
        if (tab.Equals("week", StringComparison.OrdinalIgnoreCase) && rows.Count > 0)
        {
            var nowUtc7 = clock.UtcNow.AddHours(7);
            var daysSinceMonday = ((int)nowUtc7.DayOfWeek + 6) % 7;
            var weekStart = nowUtc7.Date.AddDays(-daysSinceMonday).AddHours(-7);
            var userIds = rows.Select(r => r.Id).ToList();

            var exScores = await db.ExerciseSubmissions.AsNoTracking()
                .Where(s => userIds.Contains(s.UserId) && s.SubmittedAt >= weekStart)
                .GroupBy(s => s.UserId)
                .Select(g => new { UserId = g.Key, Score = g.Sum(s => s.Score) })
                .ToDictionaryAsync(x => x.UserId, x => x.Score, ct);

            var codeScores = await db.CodeSubmissions.AsNoTracking()
                .Where(s => userIds.Contains(s.UserId) && s.SubmittedAt >= weekStart)
                .GroupBy(s => s.UserId)
                .Select(g => new { UserId = g.Key, Score = g.Sum(s => s.Score) })
                .ToDictionaryAsync(x => x.UserId, x => x.Score, ct);

            foreach (var uid in userIds)
            {
                var totalWeek = exScores.GetValueOrDefault(uid) + codeScores.GetValueOrDefault(uid);
                weeklyScores[uid] = totalWeek > 0 ? totalWeek : rows.First(r => r.Id == uid).Xp;
            }
        }

        var items = rows.Select((row, index) => new LeaderboardEntryDto
        {
            Rank = (safePage - 1) * safeSize + index + 1,
            UserId = row.Id,
            DisplayName = row.DisplayName,
            Xp = row.Xp,
            Level = ComputeLevel(row.Xp),
            Value = tab.Equals("week", StringComparison.OrdinalIgnoreCase)
                ? weeklyScores.GetValueOrDefault(row.Id, row.Xp)
                : row.Xp
        }).ToList();

        return Result<PagedResponse<LeaderboardEntryDto>>.Ok(
            PagedResponse<LeaderboardEntryDto>.Create(items, safePage, safeSize, total, Pagination.TotalPages(total, safeSize)));
    }

    // ── Shop / Gems ───────────────────────────────────────────

    public async Task RecordActivityAsync(int userId, CancellationToken ct = default)
    {
        await UpdateStreakEagerAsync(userId, ct);
        await db.SaveChangesAsync(ct);
    }

    private static readonly Dictionary<string, string> DefaultAvatarUrls = new(StringComparer.OrdinalIgnoreCase)
    {
        ["avatar-cyber-hacker"] = "/assets/avatars/cyber-hacker.svg",
        ["avatar-gold-knight"] = "/assets/avatars/gold-knight.svg",
        ["avatar-neon-ninja"] = "/assets/avatars/neon-ninja.svg",
        ["avatar-wizard"] = "/assets/avatars/wizard.svg",
        ["avatar-ai-bot"] = "/assets/avatars/ai-bot.svg",
        ["avatar-dragon"] = "/assets/avatars/dragon.svg",
    };

    private async Task<Dictionary<string, string>> GetCustomAssetsMapInternalAsync(CancellationToken ct)
    {
        var setting = await db.Settings.AsNoTracking().FirstOrDefaultAsync(s => s.Key == "shop.custom_assets", ct);
        if (setting == null || string.IsNullOrWhiteSpace(setting.Value))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        try
        {
            var map = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(setting.Value);
            return map != null ? new Dictionary<string, string>(map, StringComparer.OrdinalIgnoreCase) : new(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }
    }

    public async Task<Result<List<ShopItemDto>>> GetShopItemsAsync(int userId, CancellationToken ct)
    {
        var items = await db.ShopItems.AsNoTracking()
            .Where(i => !i.ItemKey.Contains("streak_freeze"))
            .OrderBy(i => i.PriceGems)
            .ToListAsync(ct);

        var inventory = await db.UserInventory.AsNoTracking()
            .Where(iv => iv.UserId == userId)
            .ToDictionaryAsync(iv => iv.ItemId, iv => iv.Quantity, ct);

        var customAssets = await GetCustomAssetsMapInternalAsync(ct);

        return Result<List<ShopItemDto>>.Ok(items.Select(i => {
            string? img = null;
            if (customAssets.TryGetValue(i.ItemKey, out var cUrl)) img = cUrl;
            else if (DefaultAvatarUrls.TryGetValue(i.ItemKey, out var dUrl)) img = dUrl;
            else if (i.ItemKey.Contains("dragon", StringComparison.OrdinalIgnoreCase)) img = "/assets/avatars/dragon.svg";

            var isConsumable = i.Type == 0 || i.ItemKey.Contains("heart", StringComparison.OrdinalIgnoreCase) || i.ItemKey.Contains("refill", StringComparison.OrdinalIgnoreCase);
            var isFrame = i.Type == 2 || i.ItemKey.StartsWith("frame", StringComparison.OrdinalIgnoreCase);

            var slot = isConsumable ? "item" : (isFrame ? "frame" : "avatar");
            var desc = isConsumable
                ? (i.ItemKey.Contains("10") ? "Hồi ngay 10 Tim để tiếp tục học tập và làm bài tập." : "Hồi ngay 5 Tim để tiếp tục học tập và làm bài tập.")
                : (isFrame ? "Khung viền nổi bật hiển thị quanh avatar và bảng xếp hạng." : "Avatar trang trí đại diện tài khoản học viên.");

            return new ShopItemDto
            {
                Id = i.Id,
                ItemKey = i.ItemKey,
                Name = i.Name,
                Description = desc,
                Slot = slot,
                PriceGems = i.PriceGems,
                MaxStack = i.MaxStack,
                Type = i.Type,
                Owned = inventory.TryGetValue(i.Id, out var q) ? q : 0,
                ImageUrl = img
            };
        }).ToList());
    }

    public async Task<Result<ShopBuyResultDto>> BuyItemAsync(int userId, int itemId, CancellationToken ct)
    {
        var item = await db.ShopItems.AsNoTracking().FirstOrDefaultAsync(i => i.Id == itemId, ct);
        if (item is null)
        {
            return Result<ShopBuyResultDto>.Fail(ErrorCodes.NOT_FOUND, "Vật phẩm không tồn tại");
        }

        // Bỏ streak_freeze theo yêu cầu hệ thống
        if (item.ItemKey.Contains("streak_freeze", StringComparison.OrdinalIgnoreCase))
        {
            return Result<ShopBuyResultDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Vật phẩm không còn được mở bán");
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Trừ gems bằng 1 UPDATE atomic duy nhất (Gems = Gems - price WHERE Gems >= price)
        var affected = await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Users SET Gems = Gems - {item.PriceGems} WHERE Id = {userId} AND Gems >= {item.PriceGems}", ct);
        if (affected == 0)
        {
            await tx.RollbackAsync(ct);
            return Result<ShopBuyResultDto>.Fail(ErrorCodes.INSUFFICIENT_GEMS, "Không đủ gems để mua vật phẩm này");
        }

        // Nếu là vật phẩm hồi tim tiêu hao -> cộng tim hardcode (+5 hoặc +10) theo vật phẩm và trần theo HeartsMax của user
        if (item.ItemKey.Contains("heart", StringComparison.OrdinalIgnoreCase) || item.ItemKey.Contains("refill", StringComparison.OrdinalIgnoreCase))
        {
            var refillAmount = item.ItemKey.Contains("10") ? 10 : 5;
            if (db.Database.IsRelational())
            {
                await db.Database.ExecuteSqlInterpolatedAsync(
                    $"UPDATE Users SET Hearts = CASE WHEN Hearts + {refillAmount} > HeartsMax THEN HeartsMax ELSE Hearts + {refillAmount} END WHERE Id = {userId}", ct);
            }
            else
            {
                var user = await db.Users.FindAsync([userId], ct);
                if (user is not null)
                {
                    user.Hearts = Math.Min(user.HeartsMax, user.Hearts + refillAmount);
                    await db.SaveChangesAsync(ct);
                }
            }
        }

        // Finding #4: inventory tăng bằng 1 UPDATE atomic DUY NHẤT (Quantity + 1 WHERE Quantity < MaxStack)
        var invAffected = db.Database.IsRelational()
            ? await db.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE UserInventory SET Quantity = Quantity + 1 WHERE UserId = {userId} AND ItemId = {itemId} AND Quantity < {item.MaxStack}", ct)
            : 0;

            if (invAffected == 0)
            {
                var existingInv = await db.UserInventory.FirstOrDefaultAsync(i => i.UserId == userId && i.ItemId == itemId, ct);
                if (existingInv is not null)
                {
                    if (existingInv.Quantity >= item.MaxStack)
                    {
                        await tx.RollbackAsync(ct);
                        return Result<ShopBuyResultDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                            "Đã đạt giới hạn số lượng vật phẩm", new() { ["itemId"] = ["Đã đạt giới hạn số lượng vật phẩm"] });
                    }
                    existingInv.Quantity += 1;
                    await db.SaveChangesAsync(ct);
                }
                else
                {
                    var inventory = new UserInventory
                    {
                        UserId = userId,
                        ItemId = itemId,
                        Quantity = 1,
                        IsEquipped = false,
                        PurchasedAt = clock.UtcNow,
                        ExpiresAt = item.DurationHours is { } hours ? clock.UtcNow.AddHours(hours) : null
                    };
                    db.UserInventory.Add(inventory);
                    try
                    {
                        await db.SaveChangesAsync(ct);
                    }
                    catch (DbUpdateException)
                    {
                        db.Entry(inventory).State = EntityState.Detached;
                        var existing = await db.UserInventory.FirstOrDefaultAsync(i => i.UserId == userId && i.ItemId == itemId, ct);
                        if (existing is not null && existing.Quantity < item.MaxStack)
                        {
                            existing.Quantity += 1;
                            await db.SaveChangesAsync(ct);
                        }
                        else
                        {
                            await tx.RollbackAsync(ct);
                            return Result<ShopBuyResultDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                                "Đã đạt giới hạn số lượng vật phẩm", new() { ["itemId"] = ["Đã đạt giới hạn số lượng vật phẩm"] });
                        }
                    }
                }
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
        var newQty = await db.UserInventory.AsNoTracking()
            .Where(i => i.UserId == userId && i.ItemId == itemId)
            .Select(i => i.Quantity)
            .FirstOrDefaultAsync(ct);
        logger.LogInformation("User {UserId} bought item {ItemId} ({ItemKey})", userId, itemId, item.ItemKey);

        return Result<ShopBuyResultDto>.Ok(new ShopBuyResultDto
        {
            Item = new ShopItemDto
            {
                Id = item.Id,
                ItemKey = item.ItemKey,
                Name = item.Name,
                Description = item.Type == 0 || item.ItemKey.Contains("heart", StringComparison.OrdinalIgnoreCase)
                    ? (item.ItemKey.Contains("10") ? "Hồi ngay 10 Tim để tiếp tục học tập và làm bài tập." : "Hồi ngay 5 Tim để tiếp tục học tập và làm bài tập.")
                    : (item.ItemKey.StartsWith("frame", StringComparison.OrdinalIgnoreCase) || item.Type == 2
                        ? "Khung viền nổi bật hiển thị quanh avatar và bảng xếp hạng."
                        : "Avatar trang trí đại diện tài khoản học viên."),
                Slot = item.Type == 0 || item.ItemKey.Contains("heart", StringComparison.OrdinalIgnoreCase)
                    ? "item"
                    : (item.ItemKey.StartsWith("frame", StringComparison.OrdinalIgnoreCase) || item.Type == 2 ? "frame" : "avatar"),
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
        var customAssets = await GetCustomAssetsMapInternalAsync(ct);

        var rows = await db.UserInventory.AsNoTracking()
            .Where(i => i.UserId == userId)
            .Join(db.ShopItems.AsNoTracking(), inv => inv.ItemId, item => item.Id,
                (inv, item) => new
                {
                    inv.Id,
                    inv.ItemId,
                    item.ItemKey,
                    item.Name,
                    inv.Quantity,
                    item.Type,
                    inv.IsEquipped,
                    inv.ExpiresAt
                })
            .OrderBy(i => i.Type).ThenBy(i => i.Name)
            .ToListAsync(ct);

        var result = rows.Select(i => {
            string? img = null;
            if (customAssets.TryGetValue(i.ItemKey, out var cUrl)) img = cUrl;
            else if (DefaultAvatarUrls.TryGetValue(i.ItemKey, out var dUrl)) img = dUrl;
            else if (i.ItemKey.Contains("dragon", StringComparison.OrdinalIgnoreCase)) img = "/assets/avatars/dragon.svg";

            return new InventoryItemDto
            {
                Id = i.Id,
                ItemId = i.ItemId,
                ItemKey = i.ItemKey,
                Name = i.Name,
                Quantity = i.Quantity,
                Type = i.Type,
                IsEquipped = i.IsEquipped,
                ExpiresAt = i.ExpiresAt,
                ImageUrl = img
            };
        }).ToList();

        return Result<List<InventoryItemDto>>.Ok(result);
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

        // Nhận diện nhóm trang bị: Avatar (1) hoặc Frame (2)
        int targetType = item.Type;
        if (targetType == 0)
        {
            if (item.ItemKey.StartsWith("avatar")) targetType = 1;
            else if (item.ItemKey.StartsWith("frame")) targetType = 2;
        }

        if (targetType == 0)
        {
            return Result.Fail(ErrorCodes.VALIDATION_FAILED,
                "Vật phẩm tiêu hao không thể trang bị", new() { ["itemId"] = ["Vật phẩm tiêu hao không thể trang bị"] });
        }

        // v2.9: equip cùng loại → set IsEquipped=false các dòng khác (SDD §7.3.27)
        var sameTypeItemIds = await db.ShopItems.AsNoTracking()
            .Where(i => i.Type == targetType || (targetType == 1 && i.ItemKey.StartsWith("avatar")) || (targetType == 2 && i.ItemKey.StartsWith("frame")))
            .Select(i => i.Id)
            .ToListAsync(ct);

        var owned = await db.UserInventory
            .Where(i => i.UserId == userId && sameTypeItemIds.Contains(i.ItemId))
            .ToListAsync(ct);

        bool willEquip = !row.IsEquipped;
        foreach (var ownedRow in owned)
        {
            ownedRow.IsEquipped = willEquip && (ownedRow.ItemId == request.ItemId);
        }

        // Cập nhật AvatarUrl trong bảng Users khi trang bị/gỡ trang bị Avatar
        if (targetType == 1)
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
            if (user != null)
            {
                if (willEquip)
                {
                    var customAssets = await GetCustomAssetsMapInternalAsync(ct);
                    string? avatarUrl = null;
                    if (customAssets.TryGetValue(item.ItemKey, out var cUrl)) avatarUrl = cUrl;
                    else if (DefaultAvatarUrls.TryGetValue(item.ItemKey, out var dUrl)) avatarUrl = dUrl;
                    else if (item.ItemKey.Contains("dragon", StringComparison.OrdinalIgnoreCase)) avatarUrl = "/assets/avatars/dragon.svg";

                    if (!string.IsNullOrEmpty(avatarUrl))
                    {
                        user.AvatarUrl = avatarUrl;
                    }
                }
                else
                {
                    // Khi gỡ trang bị avatar, xóa avatarUrl để quay lại avatar mặc định/chữ cái
                    user.AvatarUrl = null;
                }
            }
        }

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Finding #3: equip song song trên cùng dòng UserInventory → RowVersion đổi → 409 CONFLICT
            // (thay vì last-write-wins mất cập nhật IsEquipped).
            return Result.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }

        logger.LogInformation("User {UserId} toggled equip for item {ItemId} -> {IsEquipped}", userId, request.ItemId, willEquip);
        return Result.Ok();
    }

    // ── Premium ───────────────────────────────────────────────

    public async Task<Result<PremiumStatusDto>> GetPremiumStatusAsync(int userId, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, ct);
        if (user is not null && user.PremiumUntil > clock.UtcNow)
        {
            var activeSub = await db.PremiumSubscriptions.AsNoTracking()
                .Where(s => s.UserId == userId && s.Status == 0 && s.ExpiresAt > clock.UtcNow)
                .OrderByDescending(s => s.ExpiresAt)
                .FirstOrDefaultAsync(ct);

            return Result<PremiumStatusDto>.Ok(new PremiumStatusDto
            {
                PlanId = activeSub?.PlanId ?? "1m",
                StartedAt = activeSub?.StartedAt ?? user.CreatedAt,
                ExpiresAt = user.PremiumUntil.Value,
                Status = "active"
            });
        }

        var subscription = await db.PremiumSubscriptions.AsNoTracking()
            .Where(s => s.UserId == userId && s.Status == 0 && s.ExpiresAt > clock.UtcNow)
            .OrderByDescending(s => s.ExpiresAt)
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
        var subscription = await db.PremiumSubscriptions.AsNoTracking()
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

        // Finding #7: kích hoạt bằng UPDATE ĐIỀU KIỆN (WHERE Status = 2) — mock-pay 2 lần cùng order:
        // lần 2 ROWCOUNT=0 → KHÔNG gia hạn chồng (idempotent), không ghi đè subscription/user.
        var activated = await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE PremiumSubscriptions SET Status = 0, ExpiresAt = {expiresAt} WHERE Id = {request.OrderId} AND UserId = {userId} AND Status = 2", ct);
        if (activated == 0)
        {
            await tx.RollbackAsync(ct);
            var current = await db.PremiumSubscriptions.AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == request.OrderId && s.UserId == userId, ct);
            // Idempotent: order đã active → trả trạng thái hiện tại (200), KHÔNG gia hạn thêm
            if (current?.Status == 0 && current.ExpiresAt is { } currentExpiresAt && currentExpiresAt > clock.UtcNow)
            {
                return Result<PremiumStatusDto>.Ok(new PremiumStatusDto
                {
                    PlanId = current.PlanId,
                    StartedAt = current.StartedAt,
                    ExpiresAt = currentExpiresAt,
                    Status = "active"
                });
            }

            return Result<PremiumStatusDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Đơn hàng đã được kích hoạt");
        }

        user.PremiumUntil = expiresAt;
        user.HeartsMax = _configService.GetHeartsMaxPremium();                     // Premium tim theo cấu hình (mặc định 30)

        db.GemTransactions.Add(new GemTransaction
        {
            UserId = userId,
            Type = 0,
            Amount = 0,
            RefType = "premium",
            RefId = request.OrderId.ToString(),
            CreatedAt = clock.UtcNow
        });

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Finding #3: dòng Users đổi (RowVersion) giữa lúc đọc và ghi → rollback, 409 CONFLICT
            await tx.RollbackAsync(ct);
            return Result<PremiumStatusDto>.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }

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

    public async Task<Result<PaymentWebhookResultDto>> ProcessPaymentWebhookAsync(PaymentWebhookRequest request, CancellationToken ct)
    {
        var content = request.Content ?? request.Description ?? request.OrderRef ?? string.Empty;
        if (string.IsNullOrWhiteSpace(content))
        {
            return Result<PaymentWebhookResultDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Nội dung giao dịch trống");
        }

        // Tìm mẫu DSV{userId}T{months} (ví dụ DSV1002T1, DSV1002T3, DSV1002T12)
        var match = System.Text.RegularExpressions.Regex.Match(content, @"DSV(\d+)T(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (!match.Success)
        {
            logger.LogWarning("Payment webhook content does not match DSV pattern: {Content}", content);
            return Result<PaymentWebhookResultDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Nội dung không chứa mã định danh DSV{userId}T{months}");
        }

        var userId = int.Parse(match.Groups[1].Value);
        var months = int.Parse(match.Groups[2].Value);
        if (months != 1 && months != 3 && months != 12)
        {
            return Result<PaymentWebhookResultDto>.Fail(ErrorCodes.VALIDATION_FAILED, "Gói thời hạn Premium không hợp lệ (chỉ hỗ trợ 1, 3, hoặc 12 tháng)");
        }
        var planId = months == 1 ? "1m" : months == 3 ? "3m" : "12m";

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        if (user is null)
        {
            return Result<PaymentWebhookResultDto>.Fail(ErrorCodes.NOT_FOUND, $"Không tìm thấy người dùng ID {userId}");
        }

        // Chống replay attack: kiểm tra nếu mã giao dịch ngân hàng đã được ghi nhận trước đó
        if (!string.IsNullOrWhiteSpace(request.ReferenceCode))
        {
            var alreadyProcessed = await db.GemTransactions.AsNoTracking()
                .AnyAsync(g => g.RefType == "premium_webhook" && g.RefId == request.ReferenceCode, ct);
            if (alreadyProcessed)
            {
                return Result<PaymentWebhookResultDto>.Fail(ErrorCodes.CONFLICT, "Giao dịch thanh toán này đã được xử lý trước đó");
            }
        }

        var orderRef = $"DSV{userId}T{months}";
        var subscription = await db.PremiumSubscriptions
            .OrderByDescending(s => s.Id)
            .FirstOrDefaultAsync(s => s.UserId == userId && (s.OrderRef == orderRef || s.Status == 2), ct);

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var baseTime = user.PremiumUntil > clock.UtcNow ? user.PremiumUntil.Value : clock.UtcNow;
        var expiresAt = baseTime.AddMonths(months);

        if (subscription is not null)
        {
            subscription.Status = 0; // Active
            subscription.ExpiresAt = expiresAt;
            subscription.PlanId = planId;
        }
        else
        {
            subscription = new PremiumSubscription
            {
                UserId = userId,
                PlanId = planId,
                StartedAt = clock.UtcNow,
                ExpiresAt = expiresAt,
                Status = 0,
                OrderRef = orderRef,
                CreatedAt = clock.UtcNow
            };
            db.PremiumSubscriptions.Add(subscription);
        }

        user.PremiumUntil = expiresAt;
        user.HeartsMax = _configService.GetHeartsMaxPremium();

        db.GemTransactions.Add(new GemTransaction
        {
            UserId = userId,
            Type = 0,
            Amount = 0,
            RefType = "premium_webhook",
            RefId = request.ReferenceCode ?? orderRef,
            CreatedAt = clock.UtcNow
        });

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        logger.LogInformation("Payment webhook processed successfully: User {UserId} upgraded to Premium ({PlanId}) until {ExpiresAt}", userId, planId, expiresAt);

        return Result<PaymentWebhookResultDto>.Ok(new PaymentWebhookResultDto
        {
            Success = true,
            Message = "Kích hoạt gói Premium thành công",
            UserId = userId,
            PlanId = planId
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

        // perf#18: catalog là dictionary in-memory — gọi GetListAsync 1 lần rồi lookup thay vì
        // GetByKeyAsync từng lượt trong foreach (không phải N+1 DB nhưng bỏ overhead vòng lặp).
        var catalogResult = await catalog.GetListAsync(ct);
        var catalogByKey = catalogResult.IsSuccess
            ? catalogResult.Value!.ToDictionary(s => s.Key, StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, SimulationMetaDto>(StringComparer.OrdinalIgnoreCase);

        var fitted = new Dictionary<string, string>();
        foreach (var key in request.Keys)
        {
            fitted[key] = catalogByKey.TryGetValue(key, out var meta) ? meta.Complexity.Average : "O(n log n)";
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
            NextHeartInSeconds = nextHeartInSeconds,
            Gems = user.Gems
        };
    }

    /// <summary>
    /// Cấu hình tim: Lấy từ IGamificationConfigService (mặc định Free 30p/tim max 10, Premium 10p/tim max 30).
    /// BUG-1: khi hết hạn premium, Math.Min(user.HeartsMax, maxFree) chặn quyền lợi tim Premium "đọng" trong DB.
    /// </summary>
    private (int MaxHearts, int RegenSeconds) HeartConfig(User user, DateTime now)
    {
        var isPremium = user.PremiumUntil > now;
        var maxFree = _configService.GetHeartsMaxFree();
        var maxPremium = _configService.GetHeartsMaxPremium();
        var regenMinutes = _configService.GetHeartRegenMinutes();
        var freeRegenSec = regenMinutes * 60;
        var premRegenSec = Math.Min(600, freeRegenSec / 3 > 0 ? freeRegenSec / 3 : 600);
        return (isPremium ? maxPremium : Math.Min(user.HeartsMax, maxFree), isPremium ? premRegenSec : freeRegenSec);
    }

    /// <summary>
    /// BUG-1 (lazy downgrade — quyết định ghi decision log): premium đã hết hạn mà HeartsMax vẫn > maxFree trong DB
    /// (không có job downgrade) → đồng bộ xuống DB bằng 1 UPDATE atomic (chỉ chạy khi cần), rồi clamp giá trị
    /// cached để mọi logic tiếp theo dùng đúng. Idempotent: WHERE PremiumUntil <= now AND HeartsMax > maxFree.
    /// </summary>
    private async Task EnsureHeartsMaxSyncAsync(User user, CancellationToken ct)
    {
        var now = clock.UtcNow;
        var maxFree = _configService.GetHeartsMaxFree();
        if (user.PremiumUntil > now || user.HeartsMax <= maxFree)
        {
            return;
        }

        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Users SET HeartsMax = {maxFree}, Hearts = CASE WHEN Hearts > {maxFree} THEN {maxFree} ELSE Hearts END WHERE Id = {user.Id} AND PremiumUntil <= {now} AND HeartsMax > {maxFree}", ct);
        user.HeartsMax = maxFree;
        user.Hearts = Math.Min(user.Hearts, maxFree);
    }

    /// <summary>
    /// Ghi regen tim xuống DB (F5-Major): elapsed ≥ 1 chu kỳ regen → UPDATE Hearts + LastHeartAt.
    /// Dùng raw SQL (không qua EF tracking) để khớp chuỗi UPDATE atomic trừ tim.
    /// Tim đã đầy → không cập nhật (tránh dời LastHeartAt làm sai NextHeartInSeconds).
    /// Finding #2: UPDATE có điều kiện (delta + CASE clamp max + WHERE LastHeartAt <= giá trị ĐÃ ĐỌC) —
    /// không ghi đè Hearts từ read stale khi có request khác vừa trừ/regen (lost-update → "trả lại tim").
    /// </summary>
    private async Task PersistHeartRegenAsync(int userId, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking().FirstAsync(u => u.Id == userId, ct);
        await PersistHeartRegenAsync(user, ct);
    }

    /// <summary>perf#22: bản entity-reuse — caller đã đọc user rồi (VD EnterNodeAsync) → khỏi đọc lại.</summary>
    private async Task PersistHeartRegenAsync(User user, CancellationToken ct)
    {
        // BUG-1: đồng bộ downgrade trước khi tính regen (HeartsMax 30 → 10 + clamp Hearts)
        await EnsureHeartsMaxSyncAsync(user, ct);

        var now = clock.UtcNow;
        var (maxHearts, regenSeconds) = HeartConfig(user, now);

        var lastHeartAt = user.LastHeartAt == default ? user.CreatedAt : user.LastHeartAt;
        var elapsedSeconds = (long)(now - lastHeartAt).TotalSeconds;
        var regenCount = (int)(elapsedSeconds / regenSeconds);

        if (regenCount <= 0 || user.Hearts >= maxHearts)
        {
            return;
        }

        // Delta tính từ giá trị đã đọc, clamp để không bao giờ vượt max (Hearts + regenCount ≤ maxHearts)
        regenCount = Math.Min(regenCount, maxHearts - user.Hearts);
        await db.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE Users SET Hearts = CASE WHEN Hearts < {maxHearts} THEN Hearts + {regenCount} ELSE Hearts END, LastHeartAt = {now} WHERE Id = {user.Id} AND Hearts < {maxHearts} AND LastHeartAt <= {lastHeartAt}", ct);
    }

    private async Task<int> GetCurrentHeartsAsync(int userId, CancellationToken ct)
    {
        // F5-Major: ghi regen xuống DB trước khi đọc — số tim trả về luôn khớp DB (không còn "tim ảo")
        await PersistHeartRegenAsync(userId, ct);
        var user = await db.Users.AsNoTracking()
            .FirstAsync(u => u.Id == userId, ct);
        return ComputeHearts(user).Hearts;
    }

    /// <summary>
    /// exc#3: true khi DbUpdateException là unique violation ĐÚNG của NodeSessions (resume hợp lệ):
    /// SQL Server — SqlException 2601/2627 có message chứa "NodeSessions" (constraint IX_NodeSessions_UserId_NodeId);
    /// SQLite (unit test) — message "UNIQUE constraint failed: NodeSessions...". Vi phạm constraint khác
    /// (trigger RAISE(ABORT), FK, check...) → false → rethrow, KHÔNG nuốt lỗi DB thật.
    /// </summary>
    private static bool IsUniqueViolationForNodeSession(DbUpdateException ex)
    {
        for (var inner = ex.InnerException; inner is not null; inner = inner.InnerException)
        {
            if (inner is SqlException { Number: 2601 or 2627 }
                && inner.Message.Contains("NodeSessions", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (inner.Message.Contains("UNIQUE constraint failed", StringComparison.OrdinalIgnoreCase)
                && inner.Message.Contains("NodeSessions", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
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

    /// <summary>Streak EAGER (FR-10.4 v2.8): LastActivityDate hôm qua → +1; hôm nay → giữ; nghỉ 1 ngày (diffDays==2) dùng freeze → +1; nghỉ >= 2 ngày → reset về 1.</summary>
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
            var diffDays = (today - user.LastActivityDate.Value).TotalDays;
            // 1 lượt Freeze chỉ cứu được tối đa 1 ngày vắng mặt liên tiếp (hôm kia là lần cuối: diffDays == 2)
            if (diffDays == 2 && user.StreakFreeze > 0)
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

    private DateTime TodayUtc7() => clock.UtcNow.AddHours(7).Date;

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
