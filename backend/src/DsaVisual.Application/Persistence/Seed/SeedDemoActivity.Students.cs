using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// SEED-2a — nhóm users/achievements của <see cref="SeedDemoActivity"/> (SDD §7.5):
/// (1) xóa setting chặn domain đăng ký "allowed.email.domains" (fix domain — mọi email đăng ký được,
/// AuthService tự bỏ qua check khi setting không tồn tại, KHÔNG sửa AuthService),
/// (2) tạo 8 student @university.edu.vn từ <see cref="SeedData.Students"/>,
/// (3) seed 10 Achievement từ <see cref="SeedData.Achievements"/> (guard theo Code UNIQUE),
/// (4) gán UserAchievements cho 9 student (8 mới + student@demo.local) — mỗi student 2-5 huy hiệu.
///
/// Idempotent theo pattern SeedRunner: guard AnyAsync/FirstOrDefault → Add/Remove → SaveChanges → log.
/// KHÔNG tạo/sửa user khác, KHÔNG đụng user rác smoke, KHÔNG RemoveRange (chỉ Remove 1 setting ở bước 1).
/// </summary>
public static partial class SeedDemoActivity
{
    // ── 1. Dọn setting chặn domain đăng ký ─────────────────────

    private static partial Task SeedCleanupSettingsAsync(AppDbContext db, ILogger logger, CancellationToken ct)
        => SeedCleanupSettingsCoreAsync(db, logger, ct);

    private static async Task SeedCleanupSettingsCoreAsync(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        const string key = "allowed.email.domains";
        var setting = await db.Settings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (setting is null)
        {
            logger.LogInformation("Seed: Xóa setting allowed.email.domains bỏ qua (không có)");
            return;
        }

        db.Settings.Remove(setting);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: Xóa setting allowed.email.domains (fix domain đăng ký)");
    }

    // ── 2. 8 student mới @university.edu.vn ─────────────────────

    private static partial Task SeedNewStudentsAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
        => SeedNewStudentsCoreAsync(db, clock, logger, ct);

    private static async Task SeedNewStudentsCoreAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = NowUtc7(clock);                       // hôm nay UTC+7 (quy ước SeedDemoActivity)
        var added = 0;
        var skipped = 0;
        for (var i = 0; i < SeedData.Students.Count; i++)
        {
            var seed = SeedData.Students[i];
            var email = seed.Email.ToLowerInvariant();
            var exists = await db.Users.AnyAsync(u => u.Email == email, ct);
            if (exists)
            {
                logger.LogInformation("Seed: Bỏ qua student {Email} (đã tồn tại)", email);
                skipped++;
                continue;
            }

            // CreatedAt rải trong 30 ngày qua (deterministic theo index — chạy lại vẫn ổn định)
            var createdAt = now.AddDays(-(1 + i * 4)).AddHours(-(i % 3) * 2).AddMinutes(-(i * 5) % 60);
            db.Users.Add(new User
            {
                Email = email,
                PasswordHash = PasswordHasher.Hash(seed.DevPassword),
                DisplayName = seed.DisplayName,
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
            added++;
            logger.LogInformation("Seed: Students thêm {Email}", email);
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: Students thêm xong {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }

    // ── 3. 10 Achievement (guard theo Code UNIQUE) ──────────────

    private static partial Task SeedAchievementsAsync(AppDbContext db, ILogger logger, CancellationToken ct)
        => SeedAchievementsCoreAsync(db, logger, ct);

    private static async Task SeedAchievementsCoreAsync(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        var added = 0;
        var skipped = 0;
        foreach (var seed in SeedData.Achievements)
        {
            var exists = await db.Achievements.AnyAsync(a => a.Code == seed.Code, ct);
            if (exists)
            {
                logger.LogInformation("Seed: Achievements bỏ qua (đã tồn tại) {Code}", seed.Code);
                skipped++;
                continue;
            }

            db.Achievements.Add(new Achievement
            {
                Code = seed.Code,
                Name = seed.Name,
                Description = seed.Description,
                IconUrl = seed.IconUrl,
                ConditionJson = seed.ConditionJson,
                SortOrder = seed.SortOrder
            });
            added++;
            logger.LogInformation("Seed: Achievements thêm {Code}", seed.Code);
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: Achievements thêm xong {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }

    // ── 4. Gán UserAchievements cho 9 student (8 mới + student@demo.local) ──

    /// <summary>Kế hoạch huy hiệu theo email (2-5 huy hiệu/student, student chăm nhiều hơn) — deterministic,
    /// chạy lại lần 2 → guard (UserId, AchievementId) chặn hết (0 thêm).</summary>
    private static readonly IReadOnlyDictionary<string, string[]> StudentAchievementPlan = new Dictionary<string, string[]>
    {
        ["nguyenminhanh@university.edu.vn"] = ["first-lesson", "first-simulation", "path-1", "perfect-score", "quiz-50"],
        ["tranquocbao@university.edu.vn"] = ["first-lesson", "first-simulation", "path-1", "perfect-score", "code-100"],
        ["lethikimngan@university.edu.vn"] = ["first-lesson", "first-simulation", "path-1", "perfect-score"],
        ["phamhoanglong@university.edu.vn"] = ["first-lesson", "first-simulation", "perfect-score", "streak-7"],
        ["vuthanhtung@university.edu.vn"] = ["first-lesson", "first-simulation", "streak-7"],
        ["nguyentrang@university.edu.vn"] = ["first-lesson", "first-simulation"],
        ["doanminhduc@university.edu.vn"] = ["first-lesson", "first-simulation", "streak-7"],
        ["huynhthuy@university.edu.vn"] = ["first-lesson", "first-simulation", "streak-7"],
        ["student@demo.local"] = ["first-lesson", "first-simulation", "perfect-score", "streak-7"],
    };

    private static partial Task SeedUserAchievementsAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
        => SeedUserAchievementsCoreAsync(db, clock, logger, ct);

    private static async Task SeedUserAchievementsCoreAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = NowUtc7(clock);
        var users = await LoadDemoStudentsAsync(db, ct);   // 8 student SeedData.Students + student@demo.local
        if (users.Count == 0)
        {
            logger.LogInformation("Seed: UserAchievements bỏ qua (chưa có student nào)");
            return;
        }

        var achievements = await db.Achievements.AsNoTracking()
            .ToDictionaryAsync(a => a.Code, StringComparer.Ordinal);
        var usersByEmail = users.ToDictionary(u => u.Email, StringComparer.OrdinalIgnoreCase);
        var emails = SeedData.Students.Select(s => s.Email).Append("student@demo.local").ToList();

        var added = 0;
        var skipped = 0;
        for (var i = 0; i < emails.Count; i++)
        {
            if (!usersByEmail.TryGetValue(emails[i], out var user) ||
                !StudentAchievementPlan.TryGetValue(emails[i], out var codes))
            {
                continue;
            }

            for (var j = 0; j < codes.Length; j++)
            {
                if (!achievements.TryGetValue(codes[j], out var achievement))
                {
                    logger.LogWarning("Seed: UserAchievements bỏ qua {Code} (achievement chưa tồn tại)", codes[j]);
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
                var earnedAt = now.AddDays(-(1 + (i * 5 + j * 7) % 30)).AddHours(-(j * 2));
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
        logger.LogInformation("Seed: UserAchievements thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }
}
