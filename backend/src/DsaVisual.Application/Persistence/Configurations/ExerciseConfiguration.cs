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

        builder.HasIndex(s => new { s.UserId, s.ExerciseId, s.SubmittedAt });
        builder.HasIndex(s => s.ExerciseId);
        builder.HasIndex(s => s.ClassAssignmentId);

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
