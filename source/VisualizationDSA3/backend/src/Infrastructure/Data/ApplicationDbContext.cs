using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using VisualizationDSA.Domain.Entities;

namespace VisualizationDSA.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext, VisualizationDSA.Application.Interfaces.IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // ── 1. PHÂN HỆ NGƯỜI DÙNG & XÁC THỰC (5 bảng) ──
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<SystemAuditEventStream> SystemAuditEventStreams { get; set; } = null!;
        public DbSet<SystemSetting> SystemSettings { get; set; } = null!;

        // ── 2. PHÂN HỆ KHÓA HỌC & BÀI HỌC (8 bảng) ──
        public DbSet<Course> Courses { get; set; } = null!;
        public DbSet<CourseModule> CourseModules { get; set; } = null!;
        public DbSet<ModuleItem> ModuleItems { get; set; } = null!;
        public DbSet<Lesson> Lessons { get; set; } = null!;
        public DbSet<CourseReview> CourseReviews { get; set; } = null!;
        public DbSet<LessonComment> LessonComments { get; set; } = null!;
        public DbSet<LessonNote> LessonNotes { get; set; } = null!;
        public DbSet<LessonReview> LessonReviews { get; set; } = null!;

        // ── 3. PHÂN HỆ TRẮC NGHIỆM & ĐÁNH GIÁ (4 bảng) ──
        public DbSet<Quiz> Quizzes { get; set; } = null!;
        public DbSet<QuizQuestion> QuizQuestions { get; set; } = null!;
        public DbSet<QuizAttempt> QuizAttempts { get; set; } = null!;
        public DbSet<QuizXpGrant> QuizXpGrants { get; set; } = null!;

        // ── 4. PHÂN HỆ BÀI TẬP LẬP TRÌNH (CODELAB) (6 bảng) ──
        public DbSet<Codelab> Codelabs { get; set; } = null!;
        public DbSet<CodelabTestCase> CodelabTestCases { get; set; } = null!;
        public DbSet<CodelabHint> CodelabHints { get; set; } = null!;
        public DbSet<CodelabTemplate> CodelabTemplates { get; set; } = null!;
        public DbSet<CodelabSubmission> CodelabSubmissions { get; set; } = null!;
        public DbSet<CodelabHintReveal> CodelabHintReveals { get; set; } = null!;

        // ── 5. PHÂN HỆ LỚP HỌC (CLASSROOM) (9 bảng) ──
        public DbSet<Classroom> Classrooms { get; set; } = null!;
        public DbSet<ClassroomEnrollment> ClassroomEnrollments { get; set; } = null!;
        public DbSet<ClassroomLesson> ClassroomLessons { get; set; } = null!;
        public DbSet<ClassroomModule> ClassroomModules { get; set; } = null!;
        public DbSet<ClassroomModuleItem> ClassroomModuleItems { get; set; } = null!;
        public DbSet<ClassroomModuleItemOverride> ClassroomModuleItemOverrides { get; set; } = null!;
        public DbSet<ClassroomQuiz> ClassroomQuizzes { get; set; } = null!;
        public DbSet<ClassroomQuizAttempt> ClassroomQuizAttempts { get; set; } = null!;
        public DbSet<ClassroomAnnouncement> ClassroomAnnouncements { get; set; } = null!;

        // ── 6. PHÂN HỆ TIẾN ĐỘ & LỘ TRÌNH (LEARNING PATH) (8 bảng) ──
        public DbSet<LearningPath> LearningPaths { get; set; } = null!;
        public DbSet<LearningPathNode> LearningPathNodes { get; set; } = null!;
        public DbSet<NodeSession> NodeSessions { get; set; } = null!;
        public DbSet<UserNodeProgress> UserNodeProgresses { get; set; } = null!;
        public DbSet<StageProgress> StageProgresses { get; set; } = null!;
        public DbSet<UserLessonProgress> UserLessonProgresses { get; set; } = null!;
        public DbSet<UserModuleItemProgress> UserModuleItemProgresses { get; set; } = null!;
        public DbSet<LearningProgress> LearningProgresses { get; set; } = null!;

        // ── 7. GAMIFICATION & HUY HIỆU (4 bảng) ──
        public DbSet<Badge> Badges { get; set; } = null!;
        public DbSet<UserBadge> UserBadges { get; set; } = null!;
        public DbSet<Quest> Quests { get; set; } = null!;
        public DbSet<UserQuest> UserQuests { get; set; } = null!;

        // ── 8. CỬA HÀNG, VẬT PHẨM & GIAO DỊCH (4 bảng) ──
        public DbSet<ShopItem> ShopItems { get; set; } = null!;
        public DbSet<UserInventory> UserInventory { get; set; } = null!;
        public DbSet<GemTransaction> GemTransactions { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;

        // ── 9. MỞ RỘNG: LÝ THUYẾT, ĐỒ THỊ KHÁI NIỆM & KHÁC (7 bảng) ──
        public DbSet<TheoryArticle> TheoryArticles { get; set; } = null!;
        public DbSet<TheoryArticleVersion> TheoryArticleVersions { get; set; } = null!;
        public DbSet<LessonTheoryArticle> LessonTheoryArticles { get; set; } = null!;
        public DbSet<SemanticConceptNode> SemanticConceptNodes { get; set; } = null!;
        public DbSet<KnowledgeEdge> KnowledgeEdges { get; set; } = null!;
        public DbSet<Favorite> Favorites { get; set; } = null!;
        public DbSet<Notification> Notifications { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CourseModule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.CourseId, e.OrderIndex }).IsUnique();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.HasOne(e => e.Course)
                      .WithMany(c => c.Modules)
                      .HasForeignKey(e => e.CourseId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<ModuleItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ModuleId, e.OrderIndex }).IsUnique();
                entity.HasOne(e => e.Module)
                      .WithMany(m => m.Items)
                      .HasForeignKey(e => e.ModuleId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<ClassroomModuleItemOverride>(entity => {
                entity.HasKey(o => o.Id);
                entity.HasIndex(o => new { o.ClassroomId, o.ModuleItemId }).IsUnique();
                entity.HasOne(o => o.Classroom)
                    .WithMany(c => c.ModuleItemOverrides)
                    .HasForeignKey(o => o.ClassroomId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ClassroomModule>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.ClassroomId, e.OrderIndex }).IsUnique();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<TheoryArticle>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Slug).IsUnique();
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Slug).IsRequired().HasMaxLength(250);
                entity.Property(e => e.ContentMd).IsRequired();
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<TheoryArticleVersion>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<LessonTheoryArticle>(entity =>
            {
                entity.HasKey(e => new { e.LessonId, e.TheoryArticleId });
            });

            modelBuilder.Entity<ClassroomAnnouncement>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ContentMd).IsRequired();
            });

            modelBuilder.Entity<Lesson>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Quiz>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<Codelab>(entity => {
                entity.HasKey(e => e.Id);
                entity.HasQueryFilter(e => !e.IsDeleted);
            });

            modelBuilder.Entity<CodelabSubmission>(entity => {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
            });

            modelBuilder.Entity<ClassroomEnrollment>(entity =>
            {
                entity.HasIndex(e => new { e.ClassroomId, e.StudentId }).IsUnique();
            });

            modelBuilder.Entity<CodelabHintReveal>(entity =>
            {
                entity.HasIndex(e => new { e.UserId, e.CodelabHintId }).IsUnique();
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20).HasDefaultValue("Student");
            });

            modelBuilder.Entity<Badge>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
            });

            modelBuilder.Entity<UserBadge>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.BadgeId }).IsUnique();
            });

            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token).IsUnique();
                entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.PaymentCode).IsUnique();
            });

            modelBuilder.Entity<SemanticConceptNode>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ConceptKey).IsUnique();
            });

            modelBuilder.Entity<KnowledgeEdge>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<SystemAuditEventStream>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<UserLessonProgress>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.LessonId });
            });

            modelBuilder.Entity<LessonComment>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<AuditLog>(entity =>
            {
                entity.HasKey(e => e.Id);
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.Id);
            });
        }
    }
}
