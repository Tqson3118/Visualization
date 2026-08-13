using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// SEED-V2-1 (PROMPT_K_SEED_PROD_V2 — Task 1): tạo 69 user V2 mới — 68 student @university.edu.vn theo
/// 4 persona (Hardworking 13 / Average 32 / Slacker 13 / New 10) + 1 showcase@demo.local (tổng 69).
/// Pattern giống hệt V1 SeedDemoActivity.Students.cs: guard Email → Add → SaveChanges → log.
/// Khác biệt V1: SaveChanges TỪNG user ngay trong vòng lặp (để Task 2-6 có Id thật khi truy vấn theo email),
/// CreatedAt rải 30 ngày theo Index deterministic của V2Users (showcase = đúng 30 ngày trước — streak 30 ngày hợp lý).
/// Idempotent: email đã tồn tại (26 email cũ hoặc lần chạy trước) → bỏ qua, KHÔNG đụng user cũ.
/// CHƯA wire vào SeedDemoActivity.cs (Task 6 sẽ thêm call).
/// </summary>
public static partial class SeedDemoActivity
{
    /// <summary>
    /// Tạo 69 user V2 (guard Email UNIQUE lowercase). Role=Student, IsActive=true, IsPrimaryAdmin=false,
    /// Hearts=HeartsMax=10, LastHeartAt=CreatedAt, Xp=Gems=StreakDays=0 (hoạt động tính lại ở Task 4),
    /// PasswordHash = PasswordHasher.Hash("Student@123") — PBKDF2, pattern V1.
    /// </summary>
    public static async Task SeedNewStudentsV2Async(AppDbContext db, ILogger logger, CancellationToken ct)
    {
        var now = DateTime.UtcNow.AddHours(7);          // hôm nay UTC+7 (quy ước SeedDemoActivity)
        now = now.AddTicks(-(now.Ticks % TimeSpan.TicksPerMinute));
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
}
