using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DsaVisual.Application.Persistence.Configurations;

/// <summary>Topics — SDD §7.3.7 (cây 2 cấp, unique (ParentId, Name) + filtered unique cho gốc).</summary>
public sealed class TopicConfiguration : IEntityTypeConfiguration<Topic>
{
    public void Configure(EntityTypeBuilder<Topic> builder)
    {
        builder.ToTable("Topics");

        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        builder.Property(t => t.Description).HasMaxLength(500);
        builder.Property(t => t.SortOrder).HasDefaultValue(0);
        builder.Property(t => t.CreatedAt).HasColumnType("datetime2");
        builder.Property(t => t.UpdatedAt).HasColumnType("datetime2");
        builder.Property(t => t.DeletedAt).HasColumnType("datetime2");

        builder.HasIndex(t => new { t.ParentId, t.Name }).IsUnique();
        builder.HasIndex(t => t.Name)
            .IsUnique()
            .HasFilter("[ParentId] IS NULL");

        builder.HasOne<Topic>()
            .WithMany()
            .HasForeignKey(t => t.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
