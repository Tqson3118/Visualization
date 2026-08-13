using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test VERIFY ĐỘC LẬP cho seeder hoạt động người dùng demo (SEED-5) — chỉ đọc DB sau khi chạy
/// <see cref="SeedRunner.SeedAsync"/> (seed content + demo activity). KHÔNG sửa production code.
///
/// Cơ chế: EF InMemory (giống AuthServiceTests) — SeedRunner/SeedDemoActivity không dùng raw SQL nên
/// InMemory đủ; InMemory không enforce unique index → test "không trùng unique" tự đếm/group để xác minh.
/// Clock cố định 2026-08-12 08:00 UTC → "hôm nay UTC+7" = 2026-08-12, deterministic.
/// </summary>
public class SeedDemoActivityTests
{
    private static async Task<AppDbContext> SeedOnceAsync(string dbName)
    {
        var db = TestServices.CreateInMemoryDb(dbName);
        await SeedRunner.SeedAsync(db, new TestServices.FixedClock(), NullLogger.Instance, CancellationToken.None);
        return db;
    }

    private static async Task<Dictionary<string, int>> SnapshotCountsAsync(AppDbContext db) => new()
    {
        ["Users"] = await db.Users.CountAsync(),
        ["Achievements"] = await db.Achievements.CountAsync(),
        ["UserAchievements"] = await db.UserAchievements.CountAsync(),
        ["UserProgress"] = await db.UserProgress.CountAsync(),
        ["UserNodeProgress"] = await db.UserNodeProgress.CountAsync(),
        ["ExerciseSubmissions"] = await db.ExerciseSubmissions.CountAsync(),
        ["UserQuests"] = await db.UserQuests.CountAsync(),
        ["GemTransactions"] = await db.GemTransactions.CountAsync(),
        ["UserInventory"] = await db.UserInventory.CountAsync(),
        ["Favorites"] = await db.Favorites.CountAsync(),
        ["ContentFeedback"] = await db.ContentFeedback.CountAsync(),
        ["Classes"] = await db.Classes.CountAsync(),
        ["ClassMembers"] = await db.ClassMembers.CountAsync(),
        ["ClassAssignments"] = await db.ClassAssignments.CountAsync(),
        ["CodeSubmissions"] = await db.CodeSubmissions.CountAsync(),
        ["BugReports"] = await db.BugReports.CountAsync(),
        ["LessonNotes"] = await db.LessonNotes.CountAsync(),
    };

    /// <summary>
    /// 9 user activity V1 (8 student SeedData.Students + student@demo.local) — invariant Xp/Gems chỉ áp
    /// dụng cho V1 (user load TRACKED, recompute persist — Activity.cs). V2 (Task 1-6) có lỗi đã biết:
    /// SeedDemoActivity.V2.Activity.cs cập nhật Xp/Gems trên entity AsNoTracking (detached) → giá trị
    /// KHÔNG persist (xem report Task 6) → các test balance/XP giới hạn phạm vi V1.
    /// </summary>
    private static HashSet<string> V1ActivityEmails() => SeedData.Students
        .Select(s => s.Email.ToLowerInvariant())
        .Append("student@demo.local")
        .ToHashSet(StringComparer.Ordinal);

    private static void AssertNoDuplicateKey<T>(IEnumerable<T> rows, Func<T, object> keySelector, string label)
    {
        var duplicates = rows.GroupBy(keySelector).Where(g => g.Count() > 1).ToList();
        Assert.True(
            duplicates.Count == 0,
            $"{label} trùng {duplicates.Count} key: {string.Join(" | ", duplicates.Take(3).Select(g => g.Key))}");
    }

    // ── 1. Counts ──────────────────────────────────────────────

    [Fact]
    public async Task Seed_Counts_MeetExpectations()
    {
        var db = await SeedOnceAsync(nameof(Seed_Counts_MeetExpectations));

        Assert.True(await db.Users.CountAsync() >= 9, "Users >= 9");
        Assert.Equal(17, await db.Achievements.CountAsync());   // 10 V1 + 7 V2 (SortOrder 11-17)
        Assert.True(await db.UserAchievements.CountAsync() >= 10, "UserAchievements >= 10");
        Assert.True(await db.UserProgress.CountAsync() >= 15, "UserProgress >= 15");
        Assert.True(await db.UserNodeProgress.CountAsync() >= 20, "UserNodeProgress >= 20");
        Assert.True(await db.ExerciseSubmissions.CountAsync() >= 30, "ExerciseSubmissions >= 30");
        Assert.True(await db.UserQuests.CountAsync() >= 20, "UserQuests >= 20");
        Assert.True(await db.GemTransactions.CountAsync() >= 30, "GemTransactions >= 30");
        Assert.True(await db.UserInventory.CountAsync() >= 7, "UserInventory >= 7");
        Assert.True(await db.Favorites.CountAsync() >= 10, "Favorites >= 10");
        Assert.True(await db.ContentFeedback.CountAsync() >= 5, "ContentFeedback >= 5");
        Assert.Equal(3, await db.Classes.CountAsync());         // DSA213 + ADVNCE (V1) + AI1702 (V2)
        Assert.True(await db.ClassMembers.CountAsync() >= 10, "ClassMembers >= 10");
        Assert.True(await db.ClassAssignments.CountAsync() >= 6, "ClassAssignments >= 6");
        Assert.True(await db.CodeSubmissions.CountAsync() >= 3, "CodeSubmissions >= 3");
        Assert.True(await db.BugReports.CountAsync() >= 2, "BugReports >= 2");
        Assert.True(await db.LessonNotes.CountAsync() >= 2, "LessonNotes >= 2");
    }

    // ── 2. Idempotent ──────────────────────────────────────────

    [Fact]
    public async Task Seed_SecondRun_DoesNotChangeAnyCount()
    {
        var db = await SeedOnceAsync(nameof(Seed_SecondRun_DoesNotChangeAnyCount));
        var before = await SnapshotCountsAsync(db);

        await SeedRunner.SeedAsync(db, new TestServices.FixedClock(), NullLogger.Instance, CancellationToken.None);

        var after = await SnapshotCountsAsync(db);
        foreach (var (table, count) in before)
        {
            Assert.Equal(count, after[table]);
        }
    }

    // ── 3. Xp / Gems nhất quán ─────────────────────────────────

    [Fact]
    public async Task Seed_GemsBalance_EqualsEarnMinusSpend()
    {
        var db = await SeedOnceAsync(nameof(Seed_GemsBalance_EqualsEarnMinusSpend));
        var users = await db.Users.AsNoTracking().ToDictionaryAsync(u => u.Id);
        var activityEmails = V1ActivityEmails();
        var transactions = (await db.GemTransactions.AsNoTracking().ToListAsync())
            .Where(t => users.TryGetValue(t.UserId, out var u) && activityEmails.Contains(u.Email))
            .ToList();

        Assert.NotEmpty(transactions);
        foreach (var group in transactions.GroupBy(t => t.UserId))
        {
            var user = users[group.Key];
            var earn = group.Where(t => t.Type == 0).Sum(t => t.Amount);
            var spend = group.Where(t => t.Type == 1).Sum(t => Math.Abs(t.Amount));
            Assert.True(
                user.Gems == earn - spend,
                $"{user.Email}: User.Gems={user.Gems} nhưng tổng earn={earn} − spend={spend} = {earn - spend}");
        }
    }

    [Fact]
    public async Task Seed_UsersWithClaimedQuests_HaveXpAndLevel()
    {
        var db = await SeedOnceAsync(nameof(Seed_UsersWithClaimedQuests_HaveXpAndLevel));
        var users = await db.Users.AsNoTracking().ToDictionaryAsync(u => u.Id);
        var activityEmails = V1ActivityEmails();
        var claimedUserIds = (await db.UserQuests.AsNoTracking()
            .Where(q => q.Claimed)
            .Select(q => q.UserId)
            .Distinct()
            .ToListAsync())
            .Where(id => users.TryGetValue(id, out var u) && activityEmails.Contains(u.Email))
            .ToList();

        Assert.NotEmpty(claimedUserIds);
        foreach (var userId in claimedUserIds)
        {
            var user = users[userId];
            Assert.True(user.Xp > 0, $"{user.Email} có quest đã claim nhưng Xp={user.Xp} (phải > 0)");
            var level = 1 + (int)Math.Floor(Math.Sqrt(user.Xp / 100.0));
            Assert.True(level >= 1, $"{user.Email}: level phải >= 1 (Xp={user.Xp})");
        }
    }

    // ── 4. Node pass ⇔ bài nộp full-score ──────────────────────

    [Fact]
    public async Task Seed_PassedNode_HasFullScoreSubmissionForNodeExercise()
    {
        var db = await SeedOnceAsync(nameof(Seed_PassedNode_HasFullScoreSubmissionForNodeExercise));
        var nodes = await db.LearningPathNodes.AsNoTracking().ToDictionaryAsync(n => n.Id);
        var exercises = await db.Exercises.AsNoTracking().ToListAsync();
        var submissions = await db.ExerciseSubmissions.AsNoTracking().ToListAsync();
        var passed = await db.UserNodeProgress.AsNoTracking().Where(p => p.Status == 2).ToListAsync();

        Assert.NotEmpty(passed);
        foreach (var row in passed)
        {
            var node = nodes[row.NodeId];

            // Node bài học ("Học: X") → exercise MCQ gắn NodeId node (H-FINAL1 — seed gán NodeId/Stage
            // cho exercise lesson); node kiểm tra cuối → exercise qua FinalTestId. Node luyện tập
            // không có exercise → chỉ Status=1.
            Exercise? exercise = null;
            if (node.FinalTestId is { } finalTestId)
            {
                exercise = exercises.FirstOrDefault(e => e.Id == finalTestId);
            }
            else if (node.LessonId is { })
            {
                exercise = exercises.FirstOrDefault(e =>
                    e.NodeId == node.Id && e.Type == ExerciseType.Mcq);
            }

            Assert.True(exercise is not null,
                $"User {row.UserId} pass node '{node.Title}' (Id={node.Id}) nhưng không xác định được exercise gắn node");
            Assert.True(
                submissions.Any(s =>
                    s.UserId == row.UserId && s.ExerciseId == exercise!.Id && s.Score == exercise.MaxScore),
                $"User {row.UserId} pass node '{node.Title}' nhưng không có ExerciseSubmission full-score " +
                $"cho exercise '{exercise!.Title}' (Id={exercise.Id}, MaxScore={exercise.MaxScore})");
        }
    }

    // ── 5. Submission score hợp lệ + đủ loại bài tập ───────────

    [Fact]
    public async Task Seed_SubmissionScores_WithinBounds_AndAllTypesPresent()
    {
        var db = await SeedOnceAsync(nameof(Seed_SubmissionScores_WithinBounds_AndAllTypesPresent));
        var exercises = await db.Exercises.AsNoTracking().ToDictionaryAsync(e => e.Id);
        var submissions = await db.ExerciseSubmissions.AsNoTracking().ToListAsync();

        Assert.NotEmpty(submissions);
        foreach (var s in submissions)
        {
            Assert.True(exercises.ContainsKey(s.ExerciseId), $"Submission {s.Id} trỏ ExerciseId không tồn tại {s.ExerciseId}");
            Assert.InRange(s.Score, 0, exercises[s.ExerciseId].MaxScore);
        }

        var types = submissions.Select(s => exercises[s.ExerciseId].Type).Distinct().ToList();
        Assert.Contains(ExerciseType.Mcq, types);
        Assert.Contains(ExerciseType.SimulationLab, types);
        Assert.Contains(ExerciseType.Code, types);
    }

    // ── 6. Cleanup setting allowed.email.domains ───────────────

    [Fact]
    public async Task Seed_RemovesAllowedEmailDomainsSetting()
    {
        var db = await SeedOnceAsync(nameof(Seed_RemovesAllowedEmailDomainsSetting));

        Assert.False(await db.Settings.AnyAsync(s => s.Key == "allowed.email.domains"));
    }

    [Fact]
    public async Task Seed_RemovesPreExistingAllowedEmailDomainsSetting()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Seed_RemovesPreExistingAllowedEmailDomainsSetting));
        db.Settings.Add(new Setting
        {
            Key = "allowed.email.domains",
            Value = "university.edu.vn",
            UpdatedAt = new TestServices.FixedClock().UtcNow,
            UpdatedBy = 1
        });
        await db.SaveChangesAsync();

        await SeedRunner.SeedAsync(db, new TestServices.FixedClock(), NullLogger.Instance, CancellationToken.None);

        Assert.False(await db.Settings.AnyAsync(s => s.Key == "allowed.email.domains"));
    }

    // ── 7. Không trùng unique key ──────────────────────────────

    [Fact]
    public async Task Seed_NoDuplicateUniqueKeys()
    {
        var db = await SeedOnceAsync(nameof(Seed_NoDuplicateUniqueKeys));

        AssertNoDuplicateKey(
            await db.UserProgress.AsNoTracking().Select(p => new { p.UserId, p.LessonId }).ToListAsync(),
            x => x, "UserProgress (UserId, LessonId)");
        AssertNoDuplicateKey(
            await db.UserNodeProgress.AsNoTracking().Select(p => new { p.UserId, p.NodeId }).ToListAsync(),
            x => x, "UserNodeProgress (UserId, NodeId)");
        AssertNoDuplicateKey(
            await db.UserQuests.AsNoTracking().Select(q => new { q.UserId, q.QuestDate, q.QuestId }).ToListAsync(),
            x => x, "UserQuests (UserId, QuestDate, QuestId)");
        AssertNoDuplicateKey(
            await db.Favorites.AsNoTracking().Select(f => new { f.UserId, f.SimulationKey }).ToListAsync(),
            x => x, "Favorites (UserId, SimulationKey)");
        AssertNoDuplicateKey(
            await db.ContentFeedback.AsNoTracking().Select(f => new { f.UserId, f.LessonId }).ToListAsync(),
            x => x, "ContentFeedback (UserId, LessonId)");
        AssertNoDuplicateKey(
            await db.UserInventory.AsNoTracking().Select(i => new { i.UserId, i.ItemId }).ToListAsync(),
            x => x, "UserInventory (UserId, ItemId)");
        AssertNoDuplicateKey(
            await db.ClassMembers.AsNoTracking().Select(m => new { m.ClassId, m.UserId }).ToListAsync(),
            x => x, "ClassMembers (ClassId, UserId)");
        AssertNoDuplicateKey(
            await db.ClassAssignments.AsNoTracking().Where(a => a.LessonId != null)
                .Select(a => new { a.ClassId, a.LessonId }).ToListAsync(),
            x => x, "ClassAssignments (ClassId, LessonId)");
        AssertNoDuplicateKey(
            await db.ClassAssignments.AsNoTracking().Where(a => a.ExerciseId != null)
                .Select(a => new { a.ClassId, a.ExerciseId }).ToListAsync(),
            x => x, "ClassAssignments (ClassId, ExerciseId)");
    }
}
