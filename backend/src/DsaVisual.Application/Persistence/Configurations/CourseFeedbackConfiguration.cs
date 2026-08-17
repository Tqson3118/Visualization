using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DsaVisual.Application.Persistence.Configurations;

/// <summary>Ý kiến học viên theo khóa — pattern ContentFeedback/BugReport (SDD §7.3.21/§7.3.22).</summary>
public sealed class CourseFeedbackConfiguration : IEntityTypeConfiguration<CourseFeedback>
{
    public void Configure(EntityTypeBuilder<CourseFeedback> builder)
    {
        builder.ToTable("CourseFeedback");

        builder.Property(f => f.Content).HasMaxLength(1000).IsRequired();
        builder.Property(f => f.ReplyText).HasMaxLength(2000);
        builder.Property(f => f.Type).HasConversion<int>().HasDefaultValue(CourseFeedbackType.Suggestion);
        builder.Property(f => f.Status).HasConversion<int>().HasDefaultValue(CourseFeedbackStatus.New);

        builder.HasIndex(f => new { f.CourseId, f.CreatedAt });
        builder.HasIndex(f => new { f.Status, f.CreatedAt });

        builder.HasOne<LearningPath>()
            .WithMany()
            .HasForeignKey(f => f.CourseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.RepliedById)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
