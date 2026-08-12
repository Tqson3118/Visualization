using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DsaVisual.Application.Persistence.Configurations;

/// <summary>
/// Cấu hình mẫu cho bảng Lessons (SDD §7.3.2) — Fluent API, không dùng attribute (SDD §5.3.6).
/// Các bảng khác dùng convention EF Core mặc định (PascalCase, Id PK identity);
/// khi triển khai từng service, bổ sung Configuration riêng cho từng bảng.
/// </summary>
public sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("Lessons");

        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedOnAdd();

        builder.Property(l => l.Title).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Description).HasMaxLength(500);
        builder.Property(l => l.ContentHtml).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(l => l.SortOrder).HasDefaultValue(0);
        builder.Property(l => l.Status).HasConversion<int>().HasDefaultValue(LessonStatus.Draft);
        builder.Property(l => l.CreatedAt).HasColumnType("datetime2");
        builder.Property(l => l.UpdatedAt).HasColumnType("datetime2");
        builder.Property(l => l.DeletedAt).HasColumnType("datetime2");   // xóa mềm (D-5)

        builder.HasOne<Topic>()
            .WithMany()
            .HasForeignKey(l => l.TopicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(l => l.CreatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(l => l.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(l => l.LessonSimulations)
            .WithOne()
            .HasForeignKey(s => s.LessonId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.Exercises)
            .WithOne()
            .HasForeignKey(e => e.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.TopicId);
        builder.HasIndex(l => new { l.CreatedBy, l.Status });
    }
}
