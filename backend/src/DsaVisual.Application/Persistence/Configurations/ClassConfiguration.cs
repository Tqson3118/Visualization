using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DsaVisual.Application.Persistence.Configurations;

/// <summary>Classes + ClassMembers + ClassAssignments — SDD §7.3.16/7.3.17/7.3.18 (Module H).</summary>
public sealed class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.ToTable("Classes");

        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.InviteCode).HasMaxLength(6).IsRequired();
        builder.Property(c => c.Semester).HasMaxLength(50);
        builder.Property(c => c.Description).HasMaxLength(500);
        builder.Property(c => c.CurriculumTitle).HasMaxLength(200);
        builder.Property(c => c.CurriculumDescription).HasMaxLength(500);
        builder.Property(c => c.LearningPathId).IsRequired(false);
        // C6: lớp mới mặc định CurriculumPublished=false (Nháp) — GV phải bấm Xuất bản mới sang published
        builder.Property(c => c.CurriculumPublished).HasDefaultValue(false);
        builder.Property(c => c.Status).HasConversion<int>().HasDefaultValue(ClassStatus.Open);
        builder.Property(c => c.CreatedAt).HasColumnType("datetime2");
        builder.Property(c => c.DeletedAt).HasColumnType("datetime2");

        builder.HasIndex(c => c.InviteCode).IsUnique();
        builder.HasIndex(c => c.OwnerId);
        builder.HasIndex(c => c.LearningPathId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.OwnerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<LearningPath>()
            .WithMany()
            .HasForeignKey(c => c.LearningPathId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public sealed class ClassMemberConfiguration : IEntityTypeConfiguration<ClassMember>
{
    public void Configure(EntityTypeBuilder<ClassMember> builder)
    {
        builder.ToTable("ClassMembers");

        builder.Property(m => m.JoinedAt).HasColumnType("datetime2");

        builder.HasIndex(m => new { m.ClassId, m.UserId }).IsUnique();
        builder.HasIndex(m => m.UserId);

        builder.HasOne<Class>()
            .WithMany()
            .HasForeignKey(m => m.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ClassAssignmentConfiguration : IEntityTypeConfiguration<ClassAssignment>
{
    public void Configure(EntityTypeBuilder<ClassAssignment> builder)
    {
        builder.ToTable("ClassAssignments");

        builder.Property(a => a.DueAt).HasColumnType("datetime2");
        builder.Property(a => a.CreatedAt).HasColumnType("datetime2");
        builder.Property(a => a.SortOrder).HasDefaultValue(0);
        builder.Property(a => a.Archived).HasDefaultValue(false);

        builder.HasIndex(a => new { a.ClassId, a.DueAt });
        builder.HasIndex(a => new { a.ClassId, a.SortOrder });
        builder.HasIndex(a => a.LessonId);
        builder.HasIndex(a => a.ExerciseId);

        // UNIQUE (ClassId, PathItemId) khi PathItemId != NULL — chặn gán trùng node PAGE vào 1 lớp
        // (double-submit race). Overlay assignment cho cùng node phải update chứ không insert thêm.
        builder.HasIndex(a => new { a.ClassId, a.PathItemId })
            .IsUnique()
            .HasFilter("[PathItemId] IS NOT NULL");

        builder.HasOne<Class>()
            .WithMany()
            .HasForeignKey(a => a.ClassId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<LearningPathNode>()
            .WithMany()
            .HasForeignKey(a => a.PathItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Lesson>()
            .WithMany()
            .HasForeignKey(a => a.LessonId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Exercise>()
            .WithMany()
            .HasForeignKey(a => a.ExerciseId)
            .OnDelete(DeleteBehavior.Restrict);

        // CHECK (LessonId IS NOT NULL OR ExerciseId IS NOT NULL OR PathItemId IS NOT NULL)
        builder.ToTable(t => t.HasCheckConstraint("CK_ClassAssignments_Content",
            "([LessonId] IS NOT NULL OR [ExerciseId] IS NOT NULL OR [PathItemId] IS NOT NULL)"));
    }
}
