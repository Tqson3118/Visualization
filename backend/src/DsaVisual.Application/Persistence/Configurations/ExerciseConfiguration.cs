using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DsaVisual.Application.Persistence.Configurations;

/// <summary>Exercises + Questions + ExerciseSubmissions + CodeRuns + CodeSubmissions — SDD §7.3.9/7.3.3/7.3.10/7.3.23/7.3.24.</summary>
public sealed class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> builder)
    {
        builder.ToTable("Exercises");

        builder.Property(e => e.Title).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.ConfigJson);
        builder.Property(e => e.Type).HasConversion<int>().HasDefaultValue(ExerciseType.Mcq);
        builder.Property(e => e.Status).HasConversion<int>().HasDefaultValue(ExerciseStatus.Draft);
        builder.Property(e => e.CreatedAt).HasColumnType("datetime2");
        builder.Property(e => e.UpdatedAt).HasColumnType("datetime2");
        builder.Property(e => e.DeletedAt).HasColumnType("datetime2");

        builder.HasIndex(e => e.LessonId);
        builder.HasIndex(e => new { e.NodeId, e.Stage });

        // UNIQUE (LessonId, Title) — chốt khoá seed Exercises (SDD §7.3.9, audit bề mặt #2).
        // Filter DeletedAt IS NULL: xóa mềm không chặn tái sử dụng Title.
        builder.HasIndex(e => new { e.LessonId, e.Title })
            .IsUnique()
            .HasFilter("[DeletedAt] IS NULL");

        builder.HasOne<Lesson>()
            .WithMany(l => l.Exercises)
            .HasForeignKey(e => e.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<LearningPathNode>()
            .WithMany()
            .HasForeignKey(e => e.NodeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(e => e.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.ToTable("Questions");

        builder.Property(q => q.Content).IsRequired();
        builder.Property(q => q.OptionsJson).IsRequired();
        builder.Property(q => q.AnswerJson).IsRequired();
        builder.Property(q => q.Explanation);
        builder.Property(q => q.Hint1).HasMaxLength(500);
        builder.Property(q => q.Hint2).HasMaxLength(500);
        builder.Property(q => q.Hint3).HasMaxLength(500);
        builder.Property(q => q.WrongExplanationsJson);
        builder.Property(q => q.Type).HasConversion<int>().HasDefaultValue(QuestionType.Single);
        builder.Property(q => q.Points).HasDefaultValue(1);

        builder.HasIndex(q => q.ExerciseId);

        builder.HasOne<Exercise>()
            .WithMany(e => e.Questions)
            .HasForeignKey(q => q.ExerciseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ExerciseSubmissionConfiguration : IEntityTypeConfiguration<ExerciseSubmission>
{
    public void Configure(EntityTypeBuilder<ExerciseSubmission> builder)
    {
        builder.ToTable("ExerciseSubmissions");

        builder.Property(s => s.AnswersJson).IsRequired();
        builder.Property(s => s.ResultJson).IsRequired();
        builder.Property(s => s.SubmittedAt).HasColumnType("datetime2");
        // Idempotency key optional — phải có MaxLength (nvarchar(max) không làm key column index).
        builder.Property(s => s.ClientRequestId).HasMaxLength(128);

        // Chống nộp trùng multi-instance CHỈ khi client gửi idempotency key (fix Đợt D — review Major #1):
        // cùng (User, Exercise, ClassAssignment, ClientRequestId) → 1 bản nộp duy nhất (retry/double-click an toàn).
        // ClientRequestId NULL (FE hiện không gửi) → KHÔNG có ràng buộc → re-attempt cải thiện điểm hợp lệ (FR-4.4/FR-9.5).
        // Double-submit single-instance: SubmissionLockRegistry (đã có); multi-instance không key: ghi chú NFR-12.
        builder.HasIndex(s => new { s.UserId, s.ExerciseId, s.ClassAssignmentId, s.ClientRequestId })
            .IsUnique()
            .HasFilter("[ClientRequestId] IS NOT NULL")
            .HasDatabaseName("IX_ExerciseSubmissions_User_Exercise_Assignment_ClientRequestId");
        builder.HasIndex(s => new { s.UserId, s.ExerciseId, s.SubmittedAt });
        builder.HasIndex(s => s.ExerciseId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(s => s.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ClassAssignment>()
            .WithMany()
            .HasForeignKey(s => s.ClassAssignmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class CodeRunConfiguration : IEntityTypeConfiguration<CodeRun>
{
    public void Configure(EntityTypeBuilder<CodeRun> builder)
    {
        builder.ToTable("CodeRuns");

        builder.Property(r => r.Code).IsRequired();
        builder.Property(r => r.InputJson).IsRequired();
        builder.Property(r => r.OutputJson);
        builder.Property(r => r.ErrorJson);
        builder.Property(r => r.TraceJson);
        builder.Property(r => r.Status).HasConversion<int>().HasDefaultValue(CodeRunStatus.Pending);
        builder.Property(r => r.CreatedAt).HasColumnType("datetime2");

        builder.HasIndex(r => new { r.UserId, r.CreatedAt });
        builder.HasIndex(r => new { r.ExerciseId, r.Status });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(r => r.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class CodeSubmissionConfiguration : IEntityTypeConfiguration<CodeSubmission>
{
    public void Configure(EntityTypeBuilder<CodeSubmission> builder)
    {
        builder.ToTable("CodeSubmissions");

        builder.Property(s => s.Code).IsRequired();
        builder.Property(s => s.ResultJson).IsRequired();
        builder.Property(s => s.SubmittedAt).HasColumnType("datetime2");
        builder.Property(s => s.IsClientDeclared).HasDefaultValue(true);
        // Idempotency key optional — phải có MaxLength (nvarchar(max) không làm key column index).
        builder.Property(s => s.ClientRequestId).HasMaxLength(128);

        // Fix Đợt D (review Major #1): BỎ unique vĩnh viễn (UserId, ExerciseId) — FR-9.5 yêu cầu
        // lịch sử nhiều lần nộp + so sánh 2 lần. Chỉ unique khi client gửi idempotency key:
        builder.HasIndex(s => new { s.UserId, s.ExerciseId, s.ClientRequestId })
            .IsUnique()
            .HasFilter("[ClientRequestId] IS NOT NULL")
            .HasDatabaseName("IX_CodeSubmissions_User_Exercise_ClientRequestId");
        builder.HasIndex(s => new { s.UserId, s.ExerciseId, s.SubmittedAt });
        builder.HasIndex(s => s.ExerciseId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(s => s.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
