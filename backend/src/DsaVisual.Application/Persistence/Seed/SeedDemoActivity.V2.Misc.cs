using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// SEED-V2 (PROMPT_K_SEED_PROD_V2) — Task 6: BugReports V2 (7 báo cáo — 4 trạng thái), CodeSubmissions V2
/// (7 bài nộp code — user Hardworking/Average) + Premium showcase (gói 12 tháng cho showcase@demo.local).
/// Khác V1 SeedDemoActivity.Misc.cs: V1 guard count==0 (bảng rỗng mới seed) — V2 ĐỔI guard theo
/// (UserId, Description) cho BugReports và (UserId, ExerciseId) cho CodeSubmissions
/// (idempotent tuyệt đối — Task 6c: bỏ SubmittedAt khỏi guard vì now thay đổi mỗi lần chạy,
/// SubmittedAt giờ deterministic từ mốc cố định; chạy lại lần 2 → 0 thêm, KHÔNG đụng dữ liệu runtime/V1).
/// Dùng chung user <see cref="V2Users.All"/> + helpers <see cref="LoadV2ActivityUsersAsync"/>
/// (V2.Activity.cs) và <see cref="NowUtc7"/> (V1 Progress.cs) — không tự tạo user mới.
/// </summary>
public static partial class SeedDemoActivity
{
    // ── 1. BugReports V2 (SDD §7.3.22) — guard (UserId, Description) ──

    /// <summary>
    /// 7 báo cáo tiếng Việt của 4 user V2 (Hardworking/Average/Slacker) — Status đủ 4 trạng thái:
    /// 2 New, 2 Processing, 2 Resolved, 1 Closed; ResolvedAt = CreatedAt + 3-5 ngày cho Resolved/Closed,
    /// null cho New/Processing. Entity không có Title → tiêu đề + nội dung gộp trong Description (≤ 2000).
    /// CreatedAt rải 1-15 ngày qua (deterministic — chạy lại vẫn ổn định).
    /// </summary>
    public static async Task SeedBugReportsV2Async(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = NowUtc7(clock);                        // hôm nay UTC+7 (quy ước SeedDemoActivity)
        var users = await LoadV2ActivityUsersAsync(db, ct);

        var rows = new (string Email, BugReportStatus Status, string Description, string Url, string Browser, string Platform, int DaysAgo, int ResolveDaysAfter)[]
        {
            ("nguyenthanhhai@university.edu.vn", BugReportStatus.New, "Gửi phản hồi nội dung không được — bấm nút Gửi ở trang phản hồi thì báo lỗi 500, thử lại 2 lần vẫn vậy; phản hồi không lưu vào danh sách của tôi.", "/api/feedback/submit", "Chrome 126.0", "Windows 11", 2, 0),
            ("tranminhduc@university.edu.vn", BugReportStatus.New, "Video bài học Bubble Sort không phát — tải trang /learn/bubble-sort thì khung video chỉ hiện màu đen, bấm Play không có phản hồi; các bài khác xem được bình thường.", "/learn/bubble-sort", "Chrome 126.0", "Windows 11", 1, 0),
            ("phanvanhung@university.edu.vn", BugReportStatus.Processing, "Mô phỏng Binary Search bị treo ở lượt chạy thứ 2 — chọn mảng 20 phần tử, bấm Chạy thì thanh tiến trình dừng ở 40% và không tiếp tục; phải tải lại trang.", "/simulation/binary-search", "Chrome 126.0", "Windows 11", 4, 0),
            ("tranquoctuan@university.edu.vn", BugReportStatus.Processing, "Không claim được nhiệm vụ hằng ngày — hoàn thành quest nhưng bấm Nhận thưởng không cộng gem, chỉ báo đã xử lý khi tải lại trang.", "/api/quests/daily", "Edge 126.0", "Windows 11", 6, 0),
            ("tranminhduc@university.edu.vn", BugReportStatus.Resolved, "Tiến độ bài học Hash Table không lưu — xem xong 3 node nhưng quay lại vẫn báo 0%; sau bản cập nhật đã vào được, nay thấy thông báo khắc phục xong.", "/learn/hash-table", "Chrome 126.0", "Windows 11", 9, 3),
            ("phanvanhung@university.edu.vn", BugReportStatus.Resolved, "Hiển thị sai đáp án khi nộp quiz Stack — câu trắc nghiệm chọn đúng nhưng báo sai điểm; đội ngũ đã xác nhận lỗi và sửa, điểm được cập nhật lại.", "/practice/quiz/stack", "Chrome 126.0", "Windows 11", 12, 4),
            ("nguyenthanhhai@university.edu.vn", BugReportStatus.Closed, "Giao diện bài AVL bị lệch trên điện thoại — node bị chồng lên nhau khi xoay cây ở khung nhỏ; đã xác nhận khắc phục ở phiên bản mới và đóng báo cáo.", "/learn/avl-tree", "Chrome 126.0", "Android 14", 15, 5),
        };

        var added = 0;
        var skipped = 0;
        foreach (var row in rows)
        {
            if (!users.TryGetValue(row.Email, out var user))
            {
                logger.LogWarning("Seed: V2 BugReports bỏ qua {Email} (user chưa tồn tại)", row.Email);
                skipped++;
                continue;
            }

            var exists = await db.BugReports.AnyAsync(
                b => b.UserId == user.Id && b.Description == row.Description, ct);
            if (exists)
            {
                skipped++;
                continue;
            }

            var createdAt = now.AddDays(-row.DaysAgo).AddHours(-(row.DaysAgo % 9)).AddMinutes(-(row.DaysAgo * 11) % 60);
            db.BugReports.Add(new BugReport
            {
                UserId = user.Id,
                Description = row.Description,
                ContextJson = JsonSerializer.Serialize(new { url = row.Url, browser = row.Browser, platform = row.Platform }),
                Status = row.Status,
                AssigneeId = null,
                CreatedAt = createdAt,
                ResolvedAt = row.ResolveDaysAfter > 0 ? createdAt.AddDays(row.ResolveDaysAfter) : null
            });
            added++;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: V2 BugReports thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }

    // ── 2. CodeSubmissions V2 (SDD §7.3.24) — guard (UserId, ExerciseId) ──

    /// <summary>
    /// 7 bài nộp code của 5 user V2 (Hardworking/Average) — Score 60-95 (2 bài 85-100: Bubble Sort 95,
    /// Binary Search 88), PassedTests = round(TotalTests × Score / MaxScore) ≤ TotalTests
    /// (TotalTests từ ConfigJson — <see cref="TotalTestsFromConfig"/> pattern V1), SubmittedAt rải 2-27 ngày
    /// tính từ MỐC CỐ ĐỊNH 2026-07-10 (Task 6c: KHÔNG dùng now — now đổi mỗi lần chạy → SubmittedAt khác
    /// → guard cũ không khớp → seed lần 2 thêm 7 dòng trùng; mọi giá trị ≤ 2026-08-06 vẫn "trong quá khứ").
    /// Bảng không UNIQUE → guard (UserId, ExerciseId) — mỗi (user, exercise) trong plan chỉ 1 bài nộp
    /// nên guard đủ idempotent tuyệt đối, chạy lại lần 2 → 0 thêm (KHÔNG dùng guard count==0 như V1).
    /// </summary>
    public static async Task SeedCodeSubmissionsV2Async(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var baseUtc = new DateTime(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc);   // mốc cố định (deterministic)
        var users = await LoadV2ActivityUsersAsync(db, ct);
        var exercises = (await db.Exercises.AsNoTracking()
            .Where(e => e.Type == ExerciseType.Code && e.DeletedAt == null && e.Status == ExerciseStatus.Active)
            .ToListAsync(ct))
            .GroupBy(e => e.Title, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var rows = new (string Email, string ExerciseTitle, int Score, int DaysAgo)[]
        {
            ("nguyenthanhhai@university.edu.vn", "Code: Bubble Sort", 95, 6),
            ("nguyenthanhhai@university.edu.vn", "Code: Binary Search", 88, 2),
            ("phanthanhson@university.edu.vn", "Code: Stack", 82, 9),
            ("tranminhduc@university.edu.vn", "Code: Bubble Sort", 76, 12),
            ("tranminhduc@university.edu.vn", "Code: Linked List", 68, 18),
            ("phanvanhung@university.edu.vn", "Code: Binary Search", 64, 22),
            ("vuhoangnam@university.edu.vn", "Code: Stack", 60, 27),
        };

        var added = 0;
        var skipped = 0;
        foreach (var row in rows)
        {
            if (!users.TryGetValue(row.Email, out var user))
            {
                logger.LogWarning("Seed: V2 CodeSubmissions bỏ qua {Email} (user chưa tồn tại)", row.Email);
                skipped++;
                continue;
            }

            if (!exercises.TryGetValue(row.ExerciseTitle, out var exercise))
            {
                logger.LogWarning("Seed: V2 CodeSubmissions bỏ qua {Title} (exercise code chưa tồn tại)", row.ExerciseTitle);
                skipped++;
                continue;
            }

            var submittedAt = baseUtc.AddDays(row.DaysAgo).AddHours(row.Score % 12).AddMinutes((row.DaysAgo * 7) % 60);
            var exists = await db.CodeSubmissions.AnyAsync(
                s => s.UserId == user.Id && s.ExerciseId == exercise.Id, ct);
            if (exists)
            {
                skipped++;
                continue;
            }

            var total = TotalTestsFromConfig(exercise.ConfigJson);
            var passed = Math.Min(
                (int)Math.Round(total * row.Score / (double)Math.Max(exercise.MaxScore, 1)),
                total);
            var results = Enumerable.Range(1, total)
                .Select(i => new { testId = $"t{i}", passed = i <= passed })
                .ToList();

            db.CodeSubmissions.Add(new CodeSubmission
            {
                UserId = user.Id,
                ExerciseId = exercise.Id,
                Code = $"function solve() {{\n  // bài nộp demo (seed SEED-V2) — {row.ExerciseTitle}\n}}",
                Score = row.Score,
                PassedTests = passed,
                TotalTests = total,
                ResultJson = JsonSerializer.Serialize(results),
                SubmittedAt = submittedAt
            });
            added++;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: V2 CodeSubmissions thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }

    // ── 3. Premium showcase (SDD §7.3.30) — gói 12 tháng cho showcase@demo.local ──

    /// <summary>
    /// showcase@demo.local mua gói Premium 12 tháng: PlanId "12m", StartedAt = CreatedAt user
    /// (~30 ngày trước — Task 1), ExpiresAt = StartedAt + 365 ngày, Status = 0 (active),
    /// OrderRef = $"DSV{{userId}}T12" (Id THẬT sau khi query user — pattern row thật DSV3T1).
    /// Đồng thời user.PremiumUntil = ExpiresAt và HeartsMax = 30 (KHÔNG giảm nếu đang &gt; 30).
    /// Guard: user chưa có subscription nào (FirstOrDefault theo UserId) — chạy lại → bỏ qua.
    /// </summary>
    public static async Task SeedPremiumShowcaseAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == V2Users.ShowcaseEmail && u.DeletedAt == null, ct);
        if (user is null)
        {
            logger.LogWarning("Seed: V2 Premium bỏ qua ({Email} chưa tồn tại — chạy SeedNewStudentsV2 trước)", V2Users.ShowcaseEmail);
            return;
        }

        var existing = await db.PremiumSubscriptions.FirstOrDefaultAsync(p => p.UserId == user.Id, ct);
        if (existing is not null)
        {
            logger.LogInformation("Seed: V2 Premium bỏ qua (showcase đã có subscription {PlanId} / {OrderRef})", existing.PlanId, existing.OrderRef);
            return;
        }

        var startedAt = user.CreatedAt;                       // ~30 ngày trước (Task 1, showcase = đúng 30 ngày)
        var expiresAt = startedAt.AddDays(365);               // gói 12 tháng
        db.PremiumSubscriptions.Add(new PremiumSubscription
        {
            UserId = user.Id,
            PlanId = "12m",
            StartedAt = startedAt,
            ExpiresAt = expiresAt,
            Status = 0,
            OrderRef = $"DSV{user.Id}T12",
            CreatedAt = startedAt
        });

        user.PremiumUntil = expiresAt;
        if (user.HeartsMax < 30)
        {
            user.HeartsMax = 30;                              // KHÔNG giảm nếu đang > 30
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation(
            "Seed: V2 Premium thêm showcase ({Email}): PlanId {PlanId}, OrderRef {OrderRef}, ExpiresAt {ExpiresAt:yyyy-MM-dd}, HeartsMax {HeartsMax}",
            V2Users.ShowcaseEmail, "12m", $"DSV{user.Id}T12", expiresAt, user.HeartsMax);
    }
}
