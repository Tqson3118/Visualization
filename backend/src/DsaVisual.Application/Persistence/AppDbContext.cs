using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.Application.Persistence;

/// <summary>
/// DbContext chính — DbSet đủ 33 bảng theo SDD §7 (lõi học tập 25 + gamification/code 8).
/// KHÔNG có Repository pattern (SDD §5.1 — quyết định A-1): Service truy vấn DbSet trực tiếp.
/// Cấu hình Fluent API đặt trong Configurations/ (SDD §5.3.6 — không attribute trên entity).
/// </summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // ── Lõi học tập (25 bảng) ───────────────────────────────
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();   // 2FA email (GP-T2)
    public DbSet<RegisterOtpCode> RegisterOtpCodes => Set<RegisterOtpCode>();   // OTP đăng ký (B0)
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<LessonSimulation> LessonSimulations => Set<LessonSimulation>();
    public DbSet<LessonNote> LessonNotes => Set<LessonNote>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<ExerciseSubmission> ExerciseSubmissions => Set<ExerciseSubmission>();
    public DbSet<UserProgress> UserProgress => Set<UserProgress>();
    public DbSet<UserNodeProgress> UserNodeProgress => Set<UserNodeProgress>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<ClassMember> ClassMembers => Set<ClassMember>();
    public DbSet<ClassAssignment> ClassAssignments => Set<ClassAssignment>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();
    public DbSet<ContentFeedback> ContentFeedback => Set<ContentFeedback>();
    public DbSet<CourseFeedback> CourseFeedback => Set<CourseFeedback>();
    public DbSet<BugReport> BugReports => Set<BugReport>();
    public DbSet<LearningPath> LearningPaths => Set<LearningPath>();
    public DbSet<LearningPathNode> LearningPathNodes => Set<LearningPathNode>();
    public DbSet<NodeSession> NodeSessions => Set<NodeSession>();

    // ── Gamification + Code (8 bảng) ────────────────────────
    public DbSet<DailyQuest> DailyQuests => Set<DailyQuest>();
    public DbSet<UserQuest> UserQuests => Set<UserQuest>();
    public DbSet<ShopItem> ShopItems => Set<ShopItem>();
    public DbSet<UserInventory> UserInventory => Set<UserInventory>();
    public DbSet<GemTransaction> GemTransactions => Set<GemTransaction>();
    public DbSet<PremiumSubscription> PremiumSubscriptions => Set<PremiumSubscription>();
    public DbSet<CodeRun> CodeRuns => Set<CodeRun>();
    public DbSet<CodeSubmission> CodeSubmissions => Set<CodeSubmission>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
