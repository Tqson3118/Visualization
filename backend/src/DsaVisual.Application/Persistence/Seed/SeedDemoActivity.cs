using DsaVisual.Application.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Persistence.Seed;

/// <summary>
/// Seed dữ liệu hoạt động người dùng demo (SDD §7.5) — kể câu chuyện "app phát hành 1 tháng"
/// (hôm nay = clock.UtcNow + 7h, giờ UTC+7). Được SeedRunner gọi SAU SeedSettingsAsync (task SEED-4).
///
/// File này là SKELETON + WIRING: <see cref="SeedAsync"/> gọi lần lượt các bước bên dưới — mỗi bước được
/// implement ở các file partial riêng: SeedDemoActivity.Students.cs, SeedDemoActivity.Achievements.cs,
/// SeedDemoActivity.Progress.cs (5-7), SeedDemoActivity.Activity.cs (8-12),
/// SeedDemoActivity.Misc.cs (13-15), SeedDemoActivity.Class.cs (16). Mọi bước đều idempotent theo pattern
/// SeedRunner: guard AnyAsync/FirstOrDefault → Add → SaveChanges → LogInformation("Seed: ... thêm / bỏ qua (đã tồn tại)").
/// Trình tự gọi theo phụ thuộc dữ liệu: 1-4 (users/achievements) → 5-7 (progress/submissions, dựa users+lessons)
/// → 8-12 (quest/gems/inventory/favorites/feedback, dựa users+quests+shop) → 13-15 (misc) → 16 (classes, gắn ClassAssignment).
/// </summary>
public static partial class SeedDemoActivity
{
    /// <summary>Chạy toàn bộ seed hoạt động demo. SeedRunner gọi SAU SeedSettingsAsync (task SEED-4).</summary>
    public static async Task SeedAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct)
    {
        // 1-4. Cài đặt + người dùng + huy hiệu
        await SeedCleanupSettingsAsync(db, logger, ct);
        await SeedNewStudentsAsync(db, clock, logger, ct);
        await SeedAchievementsAsync(db, logger, ct);
        await SeedUserAchievementsAsync(db, clock, logger, ct);

        // 5-7. Học tập: tiến độ lesson → tiến độ node → bài nộp (CompletedAt/PassedAt khớp SubmittedAt)
        await SeedUserProgressAsync(db, clock, logger, ct);
        await SeedUserNodeProgressAsync(db, clock, logger, ct);
        await SeedExerciseSubmissionsAsync(db, clock, logger, ct);

        // 8-12. Hoạt động: quest → giao dịch gem → kho đồ → yêu thích → phản hồi
        await SeedUserQuestsAsync(db, clock, logger, ct);
        await SeedGemTransactionsAsync(db, clock, logger, ct);
        await SeedUserInventoryAsync(db, clock, logger, ct);
        await SeedFavoritesAsync(db, clock, logger, ct);
        await SeedContentFeedbackAsync(db, clock, logger, ct);

        // 13-16. Còn lại: nộp code → báo lỗi → ghi chú bài học → lớp học
        await SeedCodeSubmissionsAsync(db, clock, logger, ct);
        await SeedBugReportsAsync(db, clock, logger, ct);
        await SeedLessonNotesAsync(db, clock, logger, ct);
        await SeedClassesAsync(db, clock, logger, ct);

        // === Seed V2 (PROD-K) — Task 1-6: 69 user V2 + hoạt động mở rộng (SEED-V2) ===
        await SeedNewStudentsV2Async(db, logger, ct);
        await SeedAchievementsV2Async(db, logger, ct);
        await SeedUserAchievementsV2Async(db, clock, logger, ct);
        await SeedUserProgressV2Async(db, clock, logger, ct);
        await SeedUserNodeProgressV2Async(db, clock, logger, ct);
        await SeedExerciseSubmissionsV2Async(db, clock, logger, ct);
        await SeedUserQuestsV2Async(db, clock, logger, ct);
        await SeedGemTransactionsV2Async(db, clock, logger, ct);
        await SeedUserInventoryV2Async(db, clock, logger, ct);
        await SeedFavoritesV2Async(db, clock, logger, ct);
        await SeedContentFeedbackV2Async(db, clock, logger, ct);
        await SeedClassesV2Async(db, clock, logger, ct);
        await SeedBugReportsV2Async(db, clock, logger, ct);
        await SeedCodeSubmissionsV2Async(db, clock, logger, ct);
        await SeedPremiumShowcaseAsync(db, clock, logger, ct);

        logger.LogInformation("Seed: V2 hoàn tất — Users={Users}, BugReports={BugReports}, CodeSubmissions={CodeSubmissions}, PremiumSubscriptions={PremiumSubscriptions}",
            await db.Users.CountAsync(ct), await db.BugReports.CountAsync(ct),
            await db.CodeSubmissions.CountAsync(ct), await db.PremiumSubscriptions.CountAsync(ct));

        logger.LogInformation(
            "SeedDemoActivity hoàn tất: Users={Users}, Achievements={Achievements}, UserAchievements={UserAchievements}, Settings={Settings}, UserProgress={UserProgress}, UserNodeProgress={UserNodeProgress}, ExerciseSubmissions={ExerciseSubmissions}, UserQuests={UserQuests}, GemTransactions={GemTransactions}, UserInventory={UserInventory}, Favorites={Favorites}, ContentFeedback={ContentFeedback}, CodeSubmissions={CodeSubmissions}, BugReports={BugReports}, LessonNotes={LessonNotes}, Classes={Classes}",
            await db.Users.CountAsync(ct), await db.Achievements.CountAsync(ct),
            await db.UserAchievements.CountAsync(ct), await db.Settings.CountAsync(ct),
            await db.UserProgress.CountAsync(ct), await db.UserNodeProgress.CountAsync(ct),
            await db.ExerciseSubmissions.CountAsync(ct), await db.UserQuests.CountAsync(ct),
            await db.GemTransactions.CountAsync(ct), await db.UserInventory.CountAsync(ct),
            await db.Favorites.CountAsync(ct), await db.ContentFeedback.CountAsync(ct),
            await db.CodeSubmissions.CountAsync(ct), await db.BugReports.CountAsync(ct),
            await db.LessonNotes.CountAsync(ct), await db.Classes.CountAsync(ct));
    }

    // ── Các bước seed — khai báo partial (signature CỐ ĐỊNH), implement ở file partial riêng ──

    /// <summary>
    /// Xóa setting Key = "allowed.email.domains" nếu tồn tại (Remove 1 dòng + log) — fix domain đăng ký:
    /// mọi email đăng ký được; AuthService tự bỏ qua check khi setting không tồn tại (KHÔNG sửa AuthService).
    /// </summary>
    private static partial Task SeedCleanupSettingsAsync(AppDbContext db, ILogger logger, CancellationToken ct);

    /// <summary>
    /// Tạo 8 student @university.edu.vn từ SeedData.Students (Email chưa tồn tại → tạo; đã tồn tại → bỏ qua).
    /// Role=Student, IsActive=1, Hearts=HeartsMax=10, LastHeartAt=CreatedAt, Xp=Gems=StreakDays=0,
    /// mật khẩu hash bằng PasswordHasher, CreatedAt rải trong 30 ngày qua (clock UTC+7). KHÔNG đụng user rác smoke.
    /// </summary>
    private static partial Task SeedNewStudentsAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct);

    /// <summary>Seed 10 Achievement từ SeedData.Achievements — guard theo Code (UNIQUE).</summary>
    private static partial Task SeedAchievementsAsync(AppDbContext db, ILogger logger, CancellationToken ct);

    /// <summary>
    /// Gán UserAchievements: mỗi student 2-5 huy hiệu (student chăm nhiều hơn), guard (UserId, AchievementId),
    /// EarnedAt rải trong 30 ngày qua.
    /// </summary>
    private static partial Task SeedUserAchievementsAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct);

    // ── 5-7. Học tập (implement: SeedDemoActivity.Progress.cs, task SEED-2b) ──

    /// <summary>
    /// ~31 dòng UserProgress: mỗi student 2-8 lesson; Viewed=1, SimulationCount ≥ 1,
    /// BestScore = max Score bài nộp trong lesson, CompletedAt = thời điểm bài nộp full-score đầu tiên
    /// (khớp SubmittedAt bài nộp tương ứng — dùng chung kế hoạch deterministic).
    /// </summary>
    private static partial Task SeedUserProgressAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct);

    /// <summary>~33 dòng UserNodeProgress: student chăm pass lộ trình 1-2 (Status=2 kèm bài nộp full-score),
    /// student mới 1-3 node Status=1/2; Stars = ceil(score×3/maxScore).</summary>
    private static partial Task SeedUserNodeProgressAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct);

    /// <summary>50 ExerciseSubmissions: mỗi student 3-10 bài (MCQ quiz + final test / LAB / CODE),
    /// Score đa dạng, SubmittedAt rải 1-27 ngày; ClassAssignmentId = null (task SEED-3 gắn sau).</summary>
    private static partial Task SeedExerciseSubmissionsAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct);

    // ── 8-12. Hoạt động (implement: SeedDemoActivity.Activity.cs, task SEED-2c) ──

    /// <summary>UserQuests: mỗi user 1-13 ngày quest, quest hôm nay đủ 3-5 dòng; claim → cập nhật
    /// Xp/Gems/StreakDays tính lại từ DB (không cộng đúp).</summary>
    private static partial Task SeedUserQuestsAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct);

    /// <summary>GemTransactions append-only: earn từng quest Claimed=1 (RefType="quest") + spend mua shop
    /// (RefType="shop"); guard: user chưa có giao dịch RefType nào thì mới thêm.</summary>
    private static partial Task SeedGemTransactionsAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct);

    /// <summary>UserInventory: item mua từ plan (avatar-ai-bot / frame-neon), IsEquipped chỉ frame (Type 1);
    /// item mới thêm → tính lại Gems = earn − spend.</summary>
    private static partial Task SeedUserInventoryAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct);

    /// <summary>Favorites: 2-4 mô phỏng mỗi user (SimulationKey + InputJson), CreatedAt rải trong 27 ngày.</summary>
    private static partial Task SeedFavoritesAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct);

    /// <summary>ContentFeedback: 1 đánh giá mỗi user (Rating + comment SeedData.Feedbacks), guard (UserId, LessonId).</summary>
    private static partial Task SeedContentFeedbackAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct);

    // ── 13-15. Còn lại (implement: SeedDemoActivity.Misc.cs, task sau) ──

    /// <summary>CodeSubmissions: bài nộp code demo (khớp CodeSubmission entity), guard (user, exercise).</summary>
    private static partial Task SeedCodeSubmissionsAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct);

    /// <summary>BugReports: báo lỗi demo từ user (trạng thái/ưu tiên đa dạng), guard (user, title).</summary>
    private static partial Task SeedBugReportsAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct);

    /// <summary>LessonNotes: ghi chú bài học demo (NoteText + note JSON), guard (user, lesson).</summary>
    private static partial Task SeedLessonNotesAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct);

    // ── 16. Lớp học (implement: SeedDemoActivity.Class.cs, task sau) ──

    /// <summary>Classes: 1-2 lớp demo + ClassMember + ClassAssignment gắn ExerciseSubmissions
    /// (ClassAssignmentId hiện null ở bước 7 sẽ được gắn tại đây).</summary>
    private static partial Task SeedClassesAsync(AppDbContext db, IDateTimeProvider clock, ILogger logger, CancellationToken ct);
}
