using System.Text.Json;
using DsaVisual.Application.Common;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// SEED-2d — nhóm misc của <see cref="SeedDemoActivity"/> (SDD §7.5): CodeSubmissions (SDD §7.3.24),
/// BugReports (SDD §7.3.22) và LessonNotes (SDD §7.3.15) cho 2-3 student đầu (SeedData.Students).
/// Idempotent theo pattern SeedRunner: guard → Add → SaveChanges → log.
/// CodeSubmissions/BugReports CHỈ seed khi bảng đang RỖNG (có dữ liệu runtime → log bỏ qua, return);
/// LessonNotes guard theo UNIQUE (UserId, LessonId). KHÔNG tạo user mới, KHÔNG đụng user rác smoke.
/// </summary>
public static partial class SeedDemoActivity
{
    // ── 1. CodeSubmissions (SDD §7.3.24) — append-only, không có UNIQUE ──

    private static partial Task SeedCodeSubmissionsAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
        => SeedCodeSubmissionsCoreAsync(db, clock, logger, ct);

    /// <summary>
    /// 5 bài nộp code demo: Score 60-100, PassedTests ≤ TotalTests (TotalTests từ ConfigJson testCases),
    /// SubmittedAt rải 1-30 ngày, UserId = 3 student đầu, ExerciseId = exercise Code theo title.
    /// Bảng KHÔNG unique → chỉ seed khi count == 0 (dữ liệu runtime → bỏ qua).
    /// </summary>
    private static async Task SeedCodeSubmissionsCoreAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = clock.UtcNow.AddHours(7);      // hôm nay UTC+7 (quy ước SeedDemoActivity)
        var existing = await db.CodeSubmissions.CountAsync(ct);
        if (existing > 0)
        {
            logger.LogInformation("Seed: CodeSubmissions bỏ qua (có dữ liệu runtime, {Count} dòng)", existing);
            return;
        }

        var students = await LoadFirstDemoStudentsAsync(db, take: 3, ct);
        var exercises = (await db.Exercises.AsNoTracking()
            .Where(e => e.Type == ExerciseType.Code && e.DeletedAt == null && e.Status == ExerciseStatus.Active)
            .ToListAsync(ct))
            .GroupBy(e => e.Title, StringComparer.Ordinal)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var rows = new (int StudentIndex, string ExerciseTitle, int Score, int Passed, int DaysAgo)[]
        {
            (0, "Code: Bubble Sort", 100, 11, 21),
            (1, "Code: Bubble Sort", 80, 9, 18),
            (0, "Code: Binary Search", 90, 10, 12),
            (2, "Code: Linked List", 60, 7, 8),
            (1, "Code: Stack", 70, 8, 3),
        };

        var added = 0;
        var skipped = 0;
        foreach (var row in rows)
        {
            if (row.StudentIndex >= students.Count)
            {
                skipped++;
                continue;
            }

            if (!exercises.TryGetValue(row.ExerciseTitle, out var exercise))
            {
                logger.LogWarning("Seed: CodeSubmissions bỏ qua {Title} (exercise code chưa tồn tại)", row.ExerciseTitle);
                skipped++;
                continue;
            }

            var total = TotalTestsFromConfig(exercise.ConfigJson);
            var passed = Math.Min(row.Passed, total);
            var results = Enumerable.Range(1, total)
                .Select(i => new { testId = $"t{i}", passed = i <= passed })
                .ToList();

            db.CodeSubmissions.Add(new CodeSubmission
            {
                UserId = students[row.StudentIndex].Id,
                ExerciseId = exercise.Id,
                Code = $"function solve() {{\n  // bài nộp demo (seed SEED-2d) — {row.ExerciseTitle}\n}}",
                Score = row.Score,
                PassedTests = passed,
                TotalTests = total,
                ResultJson = JsonSerializer.Serialize(results),
                SubmittedAt = now.AddDays(-row.DaysAgo).AddHours(-(row.Score % 12)).AddMinutes(-(row.DaysAgo * 7) % 60)
            });
            added++;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: CodeSubmissions thêm {Added} / bỏ qua {Skipped} (đã tồn tại / có dữ liệu runtime)", added, skipped);
    }

    // ── 2. BugReports (SDD §7.3.22) ─────────────────────────────

    private static partial Task SeedBugReportsAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
        => SeedBugReportsCoreAsync(db, clock, logger, ct);

    /// <summary>
    /// 3 báo cáo mẫu tiếng Việt của 3 student đầu — Status = New (mở), CreatedAt rải 30 ngày.
    /// Entity chỉ có Description (không có Title) → tiêu đề + nội dung gộp trong Description.
    /// Bảng không unique → chỉ seed khi count == 0 (dữ liệu runtime → bỏ qua).
    /// </summary>
    private static async Task SeedBugReportsCoreAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = clock.UtcNow.AddHours(7);      // hôm nay UTC+7
        var existing = await db.BugReports.CountAsync(ct);
        if (existing > 0)
        {
            logger.LogInformation("Seed: BugReports bỏ qua (có dữ liệu runtime, {Count} dòng)", existing);
            return;
        }

        var students = await LoadFirstDemoStudentsAsync(db, take: 3, ct);

        var rows = new (int StudentIndex, string Description, string Url, int DaysAgo)[]
        {
            (0, "Không hiện điểm khi nộp bài quiz — sau khi nộp bài trắc nghiệm Bubble Sort, màn hình chỉ báo đã nộp mà không hiện điểm số; tải lại trang vẫn không thấy.", "/practice/quiz/bubble-sort", 5),
            (1, "Nút chạy mô phỏng bị chậm khi chọn sort lớn — chọn mảng 50 phần tử ở mô phỏng Bubble Sort thì nút Chạy phản hồi sau vài giây, đôi khi bị treo.", "/simulation/bubble-sort", 12),
            (2, "Không vào được bài Lab: Hash Table trên điện thoại — bấm nút Bắt đầu lab thì chỉ thấy màn hình trắng (Chrome Android).", "/lesson/hash-table", 20),
        };

        var added = 0;
        var skipped = 0;
        foreach (var row in rows)
        {
            if (row.StudentIndex >= students.Count)
            {
                skipped++;
                continue;
            }

            db.BugReports.Add(new BugReport
            {
                UserId = students[row.StudentIndex].Id,
                Description = row.Description,
                ContextJson = JsonSerializer.Serialize(new { url = row.Url, browser = "Chrome 126.0", platform = "Windows 11" }),
                Status = BugReportStatus.New,
                AssigneeId = null,
                CreatedAt = now.AddDays(-row.DaysAgo).AddHours(-row.DaysAgo % 9).AddMinutes(-(row.DaysAgo * 11) % 60),
                ResolvedAt = null
            });
            added++;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: BugReports thêm {Added} / bỏ qua {Skipped} (đã tồn tại / có dữ liệu runtime)", added, skipped);
    }

    // ── 3. LessonNotes (SDD §7.3.15) — UNIQUE (UserId, LessonId) ──

    private static partial Task SeedLessonNotesAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
        => SeedLessonNotesCoreAsync(db, clock, logger, ct);

    /// <summary>
    /// 3 ghi chú tiếng Việt ngắn của 3 student đầu trên 3 bài học — guard theo UNIQUE (UserId, LessonId),
    /// nội dung tĩnh (không cần sanitize), UpdatedAt rải 30 ngày (entity không có CreatedAt).
    /// </summary>
    private static async Task SeedLessonNotesCoreAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        var now = clock.UtcNow.AddHours(7);      // hôm nay UTC+7
        var students = await LoadFirstDemoStudentsAsync(db, take: 3, ct);
        var lessons = await db.Lessons.AsNoTracking()
            .Where(l => l.DeletedAt == null && l.Status == LessonStatus.Active)
            .ToDictionaryAsync(l => l.Title, ct);

        var rows = new (int StudentIndex, string LessonTitle, string ContentHtml, int DaysAgo)[]
        {
            (0, "Bubble Sort", "<p>Ghi nhớ: Bubble Sort lặp n-1 lượt, mỗi lượt đẩy phần tử lớn nhất về cuối — độ phức tạp O(n²).</p>", 9),
            (1, "Binary Search", "<p>Ghi nhớ: Binary Search cần mảng đã sort — mỗi bước thu hẹp nửa phạm vi nên độ phức tạp O(log n).</p>", 4),
            (2, "Stack", "<p>Ghi nhớ: Stack là LIFO — push/pop ở đỉnh; dùng khi cần undo hoặc kiểm tra ngoặc.</p>", 15),
        };

        var added = 0;
        var skipped = 0;
        foreach (var row in rows)
        {
            if (row.StudentIndex >= students.Count)
            {
                skipped++;
                continue;
            }

            if (!lessons.TryGetValue(row.LessonTitle, out var lesson))
            {
                logger.LogWarning("Seed: LessonNotes bỏ qua {Title} (lesson chưa tồn tại)", row.LessonTitle);
                skipped++;
                continue;
            }

            var userId = students[row.StudentIndex].Id;
            var exists = await db.LessonNotes.AnyAsync(n => n.UserId == userId && n.LessonId == lesson.Id, ct);
            if (exists)
            {
                skipped++;
                continue;
            }

            db.LessonNotes.Add(new LessonNote
            {
                UserId = userId,
                LessonId = lesson.Id,
                ContentHtml = row.ContentHtml,
                UpdatedAt = now.AddDays(-row.DaysAgo).AddHours(-row.DaysAgo % 7).AddMinutes(-(row.DaysAgo * 13) % 60)
            });
            added++;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seed: LessonNotes thêm {Added} / bỏ qua {Skipped} (đã tồn tại)", added, skipped);
    }

    // ── Helpers chung (tự chứa, không phụ thuộc file partial khác) ──

    /// <summary>2-3 student đầu theo thứ tự SeedData.Students (student chăm ở đầu). KHÔNG tạo user mới.</summary>
    private static async Task<List<User>> LoadFirstDemoStudentsAsync(AppDbContext db, int take, CancellationToken ct)
    {
        var emails = SeedData.Students.Select(s => s.Email).Take(take).ToList();
        var users = await db.Users.AsNoTracking()
            .Where(u => u.Role == UserRole.Student && u.DeletedAt == null && emails.Contains(u.Email))
            .ToListAsync(ct);
        return emails
            .Select(e => users.FirstOrDefault(u => u.Email == e))
            .Where(u => u is not null)
            .Cast<User>()
            .ToList();
    }

    /// <summary>Số test ẩn từ ConfigJson exercise Code (testCases array) — fallback 11 (khớp SeedRunner).</summary>
    private static int TotalTestsFromConfig(string? configJson)
    {
        if (string.IsNullOrWhiteSpace(configJson))
        {
            return 11;
        }

        try
        {
            using var doc = JsonDocument.Parse(configJson);
            return doc.RootElement.TryGetProperty("testCases", out var tests) && tests.ValueKind == JsonValueKind.Array
                ? tests.GetArrayLength()
                : 11;
        }
        catch (JsonException)
        {
            return 11;
        }
    }
}
