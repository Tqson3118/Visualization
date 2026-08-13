using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DsaVisual.Application.Persistence.Configurations;

/// <summary>
/// LessonSimulations — SDD §7.3.8. UNIQUE (LessonId, SimulationKey) chốt khoá seed (audit bề mặt #1/#2).
/// Quan hệ FK Lesson đã khai trong LessonConfiguration (HasMany LessonSimulations) — không khai lại ở đây.
/// </summary>
public sealed class LessonSimulationConfiguration : IEntityTypeConfiguration<LessonSimulation>
{
    public void Configure(EntityTypeBuilder<LessonSimulation> builder)
    {
        builder.ToTable("LessonSimulations");

        // SimulationKey phải có MaxLength cố định — nvarchar(max) không nằm được trong unique index
        // (SQL Server giới hạn key 900 bytes); dùng 100 như Favorite.SimulationKey (nvarchar(100)).
        builder.Property(s => s.SimulationKey).HasMaxLength(100).IsRequired();

        // UNIQUE (LessonId, SimulationKey) — chống trùng seed khi chạy song song / retry sau crash.
        builder.HasIndex(s => new { s.LessonId, s.SimulationKey }).IsUnique();
    }
}
