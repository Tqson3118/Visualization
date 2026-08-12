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

        builder.HasOne<Topic>()
            .WithMany()
            .HasForeignKey(p => p.TopicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class LearningPathNodeConfiguration : IEntityTypeConfiguration<LearningPathNode>
{
    public void Configure(EntityTypeBuilder<LearningPathNode> builder)
    {
        builder.ToTable("LearningPathNodes");

        builder.Property(n => n.Title).HasMaxLength(200).IsRequired();

        builder.HasIndex(n => new { n.PathId, n.SortOrder }).IsUnique();

        builder.HasOne<LearningPath>()
            .WithMany()
            .HasForeignKey(n => n.PathId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Lesson>()
            .WithMany()
            .HasForeignKey(n => n.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(n => n.FinalTestId)
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
