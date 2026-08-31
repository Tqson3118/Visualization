using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// SEED-V2 (PROMPT_K_SEED_PROD_V2):
/// Task 1: tạo 69 user V2 mới — 68 student @university.edu.vn theo 4 persona (Hardworking 13 /
/// Average 32 / Slacker 13 / New 10) + 1 showcase@demo.local (tổng 69). Pattern giống hệt V1
/// SeedDemoActivity.Students.cs: guard Email → Add → SaveChanges → log. Khác biệt V1: SaveChanges TỪNG
/// user ngay trong vòng lặp (để Task 2-6 có Id thật khi truy vấn theo email), CreatedAt rải 30 ngày theo
/// Index deterministic của V2Users (showcase = đúng 30 ngày trước — streak 30 ngày hợp lý).
/// Idempotent: email đã tồn tại (26 email cũ hoặc lần chạy trước) → bỏ qua, KHÔNG đụng user cũ.
///
/// Task 2: +7 Achievement mới (SortOrder 11-17, guard Code UNIQUE — KHÔNG sửa 10 cái cũ/SeedData.cs) +
/// kế hoạch UserAchievements V2 theo persona (guard (UserId, AchievementId), EarnedAt deterministic theo
/// Index — pattern V1): Showcase 17/17, Hardworking 8-12, Average 4-7, Slacker 0-2, New 0-1 (tổng 337).
///
/// CHƯA wire vào SeedDemoActivity.cs (Task 6 sẽ thêm call).
/// </summary>
public static partial class SeedDemoActivity
{
    /// <summary>
    /// Tạo 69 user V2 (guard Email UNIQUE lowercase). Role=Student, IsActive=true, IsPrimaryAdmin=false,
    /// Hearts=HeartsMax=10, LastHeartAt=CreatedAt, Xp=Gems=StreakDays=0 (hoạt động tính lại ở Task 4),
    /// PasswordHash = PasswordHasher.Hash("Student@123") — PBKDF2, pattern V1.
    /// </summary>
    public static async Task SeedNewStudentsV2Async(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = NowUtc7(clock);          // hôm nay UTC+7 (quy ước SeedDemoActivity)
        var added = 0;
        var skipped = 0;
        foreach (var seed in V2Users.All)
        {
            var email = seed.Email.ToLowerInvariant();
            var exists = await db.Users.AnyAsync(u => u.Email == email, ct);
            if (exists)
            {
                logger.LogInformation("Seed: V2 bỏ qua student {Email} (đã tồn tại)", email);
                skipped++;
                continue;
            }

            // CreatedAt rải trong 30 ngày qua (deterministic theo Index — chạy lại vẫn ổn định);
            // showcase = đúng 30 ngày trước để streak 30 ngày hợp lý
            var createdAt = seed == V2Users.Showcase
                ? now.AddDays(-30)
                : now.AddDays(-(seed.Index * 29 / 68)).AddHours(-(seed.Index % 3) * 2).AddMinutes(-(seed.Index * 5) % 60);

            db.Users.Add(new User
            {
                Email = email,
                PasswordHash = PasswordHasher.Hash(V2Users.StudentPassword),
                DisplayName = seed.FullName,
                Role = UserRole.Student,
                IsActive = true,
                IsPrimaryAdmin = false,
                Hearts = 10,
                HeartsMax = 10,
                LastHeartAt = createdAt,
                Xp = 0,
                Gems = 0,
                StreakDays = 0,
                CreatedAt = createdAt
            });
            await db.SaveChangesAsync(ct);              // từng user — Task 2-6 cần Id sau khi chèn
            added++;
            logger.LogInformation("Seed: V2 Students thêm {Email} ({Persona})", email, seed.Persona);
        }

        logger.LogInformation(
            "Seed: V2 Students thêm xong {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }

    // ── Task 2: 7 Achievement mới (SortOrder 11-17, guard Code UNIQUE) ──────────────

    private sealed record V2AchievementSeed(string Code, string Name, string Description, string ConditionJson, int SortOrder);

    /// <summary>7 achievement mới V2 (SortOrder 11-17) — ConditionJson chỉ metadata count (SDD §7.3.19),
    /// Name/Description tiếng Việt theo phong cách 10 cái cũ. KHÔNG sửa 10 achievement V1 (SortOrder 1-10).</summary>
    private static readonly IReadOnlyList<V2AchievementSeed> V2AchievementSeeds =
    [
        new("tree-master", "Bậc thầy cây", "Hoàn thành 10 mô phỏng chủ đề cây (BST, AVL, Heap).", """{"type":"count","key":"trees","min":10}""", 11),
        new("graph-expert", "Chuyên gia đồ thị", "Hoàn thành 10 mô phỏng chủ đề đồ thị (BFS, DFS).", """{"type":"count","key":"graphs","min":10}""", 12),
        new("code-wizard", "Phù thủy code", "Chạy code 50 lần trong thử thách lập trình.", """{"type":"count","key":"code-runs","min":50}""", 13),
        new("speed-demon", "Tốc độ nhanh như chớp", "Hoàn thành 5 lượt mô phỏng ở tốc độ nhanh.", """{"type":"count","key":"speed-runs","min":5}""", 14),
        new("lab-master", "Vua phòng lab", "Hoàn thành 25 lab mô phỏng.", """{"type":"count","key":"labs","min":25}""", 15),
        new("social-butterfly", "Cộng đồng nhiệt huyết", "Lưu 5 mô phỏng vào danh sách yêu thích.", """{"type":"count","key":"favorites","min":5}""", 16),
        new("quiz-ace", "Át chủ bài trắc nghiệm", "Hoàn thành 100 bài quiz.", """{"type":"count","key":"quizzes","min":100}""", 17),
    ];

    /// <summary>10 achievement V1 theo SortOrder 1-10 (đã tồn tại — chỉ dùng cho kế hoạch UserAchievements V2).</summary>
    private static readonly string[] V2LegacyAchievementCodes =
        ["first-lesson", "first-simulation", "streak-7", "quiz-50", "lab-10", "code-100", "path-1", "sort-master", "perfect-score", "streak-30"];

    /// <summary>7 achievement mới V2 theo SortOrder 11-17 (dùng cho kế hoạch UserAchievements V2).</summary>
    private static readonly string[] V2NewAchievementCodes =
        ["tree-master", "graph-expert", "code-wizard", "speed-demon", "lab-master", "social-butterfly", "quiz-ace"];

    /// <summary>
    /// Seed 7 Achievement mới (guard Code UNIQUE, idempotent — chạy lại lần 2 → bỏ qua hết).
    /// KHÔNG sửa 10 achievement V1 (đã tồn tại, guard bỏ qua).
    /// </summary>
    public static async Task SeedAchievementsV2Async(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        var added = 0;
        var skipped = 0;
        foreach (var seed in V2AchievementSeeds)
        {
            var exists = await db.Achievements.AnyAsync(a => a.Code == seed.Code, ct);
            if (exists)
            {
                logger.LogInformation("Seed: V2 Achievements bỏ qua (đã tồn tại) {Code}", seed.Code);
                skipped++;
                continue;
            }

            db.Achievements.Add(new Achievement
            {
                Code = seed.Code,
                Name = seed.Name,
                Description = seed.Description,
                IconUrl = null,
                ConditionJson = seed.ConditionJson,
                SortOrder = seed.SortOrder
            });
            added++;
            logger.LogInformation("Seed: V2 Achievements thêm {Code}", seed.Code);
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: V2 Achievements thêm xong {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }

    // ── Task 2: kế hoạch UserAchievements V2 theo persona (guard (UserId, AchievementId)) ──

    /// <summary>Xoay vòng danh sách codes bắt đầu từ offset theo index (deterministic — chạy lại vẫn ổn định).</summary>
    private static IEnumerable<string> RotateV2Codes(string[] codes, int index)
    {
        var offset = index % codes.Length;
        for (var k = 0; k < codes.Length; k++)
        {
            yield return codes[(offset + k) % codes.Length];
        }
    }

    /// <summary>
    /// Kế hoạch huy hiệu theo persona (deterministic theo Index — KHÔNG random):
    /// Showcase 17/17 (10 cũ + 7 mới), Hardworking 8 + Index%5 (8..12 — ưu tiên 10 cũ + 2-3 mới),
    /// Average 4 + Index%4 (4..7 — chỉ từ 10 cũ), Slacker Index%3 (0..2 — first-lesson/first-simulation),
    /// New Index%2 (0..1 — first-lesson).
    /// </summary>
    private static string[] BuildV2AchievementPlan(string persona, int index)
    {
        switch (persona)
        {
            case V2Users.ShowcasePersona:
                return [.. V2LegacyAchievementCodes, .. V2NewAchievementCodes];
            case V2Users.HardworkingPersona:
            {
                var count = 8 + index % 5;
                var legacy = count <= V2LegacyAchievementCodes.Length
                    ? V2LegacyAchievementCodes[..count]
                    : V2LegacyAchievementCodes;
                var extras = count - legacy.Length;
                return extras <= 0
                    ? legacy
                    : [.. legacy, .. RotateV2Codes(V2NewAchievementCodes, index).Take(extras)];
            }
            case V2Users.AveragePersona:
                return RotateV2Codes(V2LegacyAchievementCodes, index).Take(4 + index % 4).ToArray();
            case V2Users.SlackerPersona:
                return RotateV2Codes(["first-lesson", "first-simulation"], index).Take(index % 3).ToArray();
            case V2Users.NewPersona:
                return index % 2 == 0 ? [] : ["first-lesson"];
            default:
                return [];
        }
    }

    /// <summary>
    /// Gán UserAchievements cho 69 user V2 theo persona (guard (UserId, AchievementId), idempotent),
    /// EarnedAt rải trong 30 ngày qua theo pattern V1 (deterministic theo Index/j), clamp ≥ CreatedAt.
    /// </summary>
    public static async Task SeedUserAchievementsV2Async(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = DateTime.UtcNow.AddHours(7);          // hôm nay UTC+7 (quy ước SeedDemoActivity)
        now = now.AddTicks(-(now.Ticks % TimeSpan.TicksPerMinute));
        var emails = V2Users.All.Select(u => u.Email).ToList();
        var users = await db.Users.AsNoTracking()
            .Where(u => u.Role == UserRole.Student && u.DeletedAt == null && emails.Contains(u.Email))
            .ToListAsync(ct);
        if (users.Count == 0)
        {
            logger.LogInformation("Seed: V2 UserAchievements bỏ qua (chưa có user V2 nào)");
            return;
        }

        var achievements = await db.Achievements.AsNoTracking()
            .ToDictionaryAsync(a => a.Code, StringComparer.Ordinal);
        var usersByEmail = users.ToDictionary(u => u.Email, StringComparer.OrdinalIgnoreCase);

        var added = 0;
        var skipped = 0;
        foreach (var seed in V2Users.All)
        {
            if (!usersByEmail.TryGetValue(seed.Email, out var user))
            {
                continue;
            }

            var codes = BuildV2AchievementPlan(seed.Persona, seed.Index);
            for (var j = 0; j < codes.Length; j++)
            {
                if (!achievements.TryGetValue(codes[j], out var achievement))
                {
                    logger.LogWarning("Seed: V2 UserAchievements bỏ qua {Code} (achievement chưa tồn tại)", codes[j]);
                    skipped++;
                    continue;
                }

                var exists = await db.UserAchievements.AnyAsync(
                    ua => ua.UserId == user.Id && ua.AchievementId == achievement.Id, ct);
                if (exists)
                {
                    skipped++;
                    continue;
                }

                // EarnedAt rải trong 30 ngày qua (deterministic) — không sớm hơn CreatedAt
                var earnedAt = now.AddDays(-(1 + (seed.Index * 5 + j * 7) % 30)).AddHours(-(j * 2));
                if (earnedAt < user.CreatedAt)
                {
                    earnedAt = user.CreatedAt;
                }

                db.UserAchievements.Add(new UserAchievement
                {
                    UserId = user.Id,
                    AchievementId = achievement.Id,
                    EarnedAt = earnedAt
                });
                added++;
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation(
            "Seed: V2 UserAchievements thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }
}
