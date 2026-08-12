using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DsaVisual.Application.Persistence.Configurations;

/// <summary>UserProgress + UserNodeProgress + LessonNotes + ContentFeedback + BugReports — SDD §7.3.4/7.3.30/7.3.15/7.3.21/7.3.22.</summary>
public sealed class UserProgressConfiguration : IEntityTypeConfiguration<UserProgress>
{
    public void Configure(EntityTypeBuilder<UserProgress> builder)
    {
        builder.ToTable("UserProgress");

        builder.Property(p => p.UpdatedAt).HasColumnType("datetime2");
        builder.Property(p => p.CompletedAt).HasColumnType("datetime2");

        builder.HasIndex(p => new { p.UserId, p.LessonId }).IsUnique();
        builder.HasIndex(p => p.LessonId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Lesson>()
            .WithMany()
            .HasForeignKey(p => p.LessonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class UserNodeProgressConfiguration : IEntityTypeConfiguration<UserNodeProgress>
{
    public void Configure(EntityTypeBuilder<UserNodeProgress> builder)
    {
        builder.ToTable("UserNodeProgress");

        builder.Property(p => p.UpdatedAt).HasColumnType("datetime2");
        builder.Property(p => p.UnlockedAt).HasColumnType("datetime2");
        builder.Property(p => p.PassedAt).HasColumnType("datetime2");

        builder.HasIndex(p => new { p.UserId, p.NodeId }).IsUnique();
        builder.HasIndex(p => p.NodeId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<LearningPathNode>()
            .WithMany()
            .HasForeignKey(p => p.NodeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class LessonNoteConfiguration : IEntityTypeConfiguration<LessonNote>
{
    public void Configure(EntityTypeBuilder<LessonNote> builder)
    {
        builder.ToTable("LessonNotes");

        builder.Property(n => n.ContentHtml).IsRequired();
        builder.Property(n => n.UpdatedAt).HasColumnType("datetime2");

        builder.HasIndex(n => new { n.UserId, n.LessonId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Lesson>()
            .WithMany()
            .HasForeignKey(n => n.LessonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ContentFeedbackConfiguration : IEntityTypeConfiguration<ContentFeedback>
{
    public void Configure(EntityTypeBuilder<ContentFeedback> builder)
    {
        builder.ToTable("ContentFeedback");

        builder.Property(f => f.Comment).HasMaxLength(200);
        builder.Property(f => f.CreatedAt).HasColumnType("datetime2");
        builder.Property(f => f.UpdatedAt).HasColumnType("datetime2");

        builder.HasIndex(f => f.LessonId);
        builder.HasIndex(f => new { f.UserId, f.LessonId }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Lesson>()
            .WithMany()
            .HasForeignKey(f => f.LessonId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class BugReportConfiguration : IEntityTypeConfiguration<BugReport>
{
    public void Configure(EntityTypeBuilder<BugReport> builder)
    {
        builder.ToTable("BugReports");

        builder.Property(b => b.Description).HasMaxLength(2000).IsRequired();
        builder.Property(b => b.ContextJson);
        builder.Property(b => b.Status).HasConversion<int>().HasDefaultValue(BugReportStatus.New);
        builder.Property(b => b.CreatedAt).HasColumnType("datetime2");
        builder.Property(b => b.ResolvedAt).HasColumnType("datetime2");

        builder.HasIndex(b => new { b.Status, b.CreatedAt });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(b => b.AssigneeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
