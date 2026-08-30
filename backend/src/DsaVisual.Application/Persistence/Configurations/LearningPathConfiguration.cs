using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DsaVisual.Application.Persistence.Configurations;

/// <summary>LearningPaths + LearningPathNodes + NodeSessions — SDD §7.3.25/§7.3.29.</summary>
public sealed class LearningPathConfiguration : IEntityTypeConfiguration<LearningPath>
{
    public void Configure(EntityTypeBuilder<LearningPath> builder)
    {
        builder.ToTable("LearningPaths");

        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.SortOrder).HasDefaultValue(0);
        builder.Property(p => p.IsActive).HasDefaultValue(true);
        // KHÔNG dùng HasDefaultValue cho Status: Draft = 0 trùng CLR default — EF sentinel sẽ bỏ
        // giá trị 0 khi INSERT và DB default (Active) ghi đè → lộ trình Nháp tự thành Công khai.
        builder.Property(p => p.Status).HasConversion<int>();
        builder.Property(p => p.Visibility).HasConversion<int>().HasDefaultValue(PathVisibility.Private);
        builder.Property(p => p.RejectionReason).HasMaxLength(500);
        builder.Property(p => p.ReviewedAt).HasColumnType("datetime2");
        builder.Property(p => p.SubmittedAt).HasColumnType("datetime2");

        // Index Title cho tìm kiếm và sắp xếp lộ trình
        builder.HasIndex(p => p.Title);

        builder.HasOne<Topic>()
            .WithMany()
            .HasForeignKey(p => p.TopicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.ReviewedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(p => p.HighlightsJson).HasMaxLength(8000);
        builder.Property(p => p.TestimonialsJson).HasMaxLength(8000);
    }
}

public sealed class LearningPathNodeConfiguration : IEntityTypeConfiguration<LearningPathNode>
{
    public void Configure(EntityTypeBuilder<LearningPathNode> builder)
    {
        builder.ToTable("LearningPathNodes");

        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();
        builder.Property(n => n.Description).HasMaxLength(1000);
        // KHÔNG dùng HasDefaultValue cho ItemType: Folder = 0 trùng CLR default của enum,
        // EF sentinel sẽ bỏ giá trị 0 khi INSERT và DB default (Theory) ghi đè → Folder luôn biến thành Theory.
        builder.Property(n => n.ItemType).HasConversion<int>();
        builder.Property(n => n.DeletedAt).HasColumnType("datetime2");

        builder.HasIndex(n => new { n.PathId, n.ParentId, n.SortOrder });

        builder.HasOne<LearningPath>()
            .WithMany()
            .HasForeignKey(n => n.PathId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<LearningPathNode>()
            .WithMany()
            .HasForeignKey(n => n.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Lesson>()
            .WithMany()
            .HasForeignKey(n => n.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(n => n.FinalTestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(n => n.LabExerciseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class NodeSessionConfiguration : IEntityTypeConfiguration<NodeSession>
{
    public void Configure(EntityTypeBuilder<NodeSession> builder)
    {
        builder.ToTable("NodeSessions");

        builder.Property(s => s.StartedAt).HasColumnType("datetime2");
        builder.Property(s => s.ExpiresAt).HasColumnType("datetime2");
        builder.Property(s => s.RowVersion).IsRowVersion();   // concurrency token (finding #3)

        // UNIQUE (UserId, NodeId) — chống double-spend trừ tim (SDD §7.3.29, FR-10.1)
        builder.HasIndex(s => new { s.UserId, s.NodeId }).IsUnique();
        builder.HasIndex(s => s.ExpiresAt);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<LearningPathNode>()
            .WithMany()
            .HasForeignKey(s => s.NodeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
