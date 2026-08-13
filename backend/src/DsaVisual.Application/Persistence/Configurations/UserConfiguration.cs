using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DsaVisual.Application.Persistence.Configurations;

/// <summary>Users + phiên (RefreshTokens, PasswordResetTokens) + Settings + Favorites — SDD §7.3.1/7.3.5/7.3.6/7.3.11/7.3.12.</summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.Property(u => u.Email).HasMaxLength(256).IsRequired();
        builder.Property(u => u.PasswordHash).HasMaxLength(256).IsRequired();
        builder.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Role).HasConversion<int>().HasDefaultValue(UserRole.Student);
        builder.Property(u => u.IsActive).HasDefaultValue(true);
        builder.Property(u => u.Hearts).HasDefaultValue(10);
        builder.Property(u => u.HeartsMax).HasDefaultValue(10);
        builder.Property(u => u.AvatarUrl).HasMaxLength(500);
        builder.Property(u => u.Department).HasMaxLength(100);       // form đăng ký GV (nullable)
        builder.Property(u => u.StaffCode).HasMaxLength(50);         // form đăng ký GV (nullable)
        builder.Property(u => u.TeacherBio).HasMaxLength(500);       // form đăng ký GV (nullable)
        builder.Property(u => u.CreatedAt).HasColumnType("datetime2");
        builder.Property(u => u.UpdatedAt).HasColumnType("datetime2");
        builder.Property(u => u.DeletedAt).HasColumnType("datetime2");   // xóa mềm (D-5)

        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => new { u.Role, u.IsActive });
        builder.HasIndex(u => u.PremiumUntil);
    }
}

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(t => t.PreviousTokenHash).HasMaxLength(64);
        builder.Property(t => t.CreatedByIp).HasMaxLength(45);
        builder.Property(t => t.ExpiresAt).HasColumnType("datetime2");
        builder.Property(t => t.RevokedAt).HasColumnType("datetime2");
        builder.Property(t => t.CreatedAt).HasColumnType("datetime2");

        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => new { t.UserId, t.ExpiresAt });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("PasswordResetTokens");

        builder.Property(t => t.TokenHash).HasMaxLength(64).IsRequired();
        builder.Property(t => t.ExpiresAt).HasColumnType("datetime2");
        builder.Property(t => t.CreatedAt).HasColumnType("datetime2");

        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => t.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class OtpCodeConfiguration : IEntityTypeConfiguration<OtpCode>
{
    public void Configure(EntityTypeBuilder<OtpCode> builder)
    {
        builder.ToTable("OtpCodes");

        builder.Property(t => t.CodeHash).HasMaxLength(64).IsRequired();
        builder.Property(t => t.Purpose).HasMaxLength(32).IsRequired();
        builder.Property(t => t.ExpiresAt).HasColumnType("datetime2");
        builder.Property(t => t.CreatedAt).HasColumnType("datetime2");

        builder.HasIndex(t => t.UserId);
        builder.HasIndex(t => new { t.UserId, t.Purpose, t.Used });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class SettingConfiguration : IEntityTypeConfiguration<Setting>
{
    public void Configure(EntityTypeBuilder<Setting> builder)
    {
        builder.ToTable("Settings");

        builder.Property(s => s.Key).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Value).HasMaxLength(500).IsRequired();
        builder.Property(s => s.Description).HasMaxLength(500);
        builder.Property(s => s.UpdatedAt).HasColumnType("datetime2");

        builder.HasIndex(s => s.Key).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.UpdatedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
{
    public void Configure(EntityTypeBuilder<Favorite> builder)
    {
        builder.ToTable("Favorites");

        builder.Property(f => f.SimulationKey).HasMaxLength(100).IsRequired();
        builder.Property(f => f.CreatedAt).HasColumnType("datetime2");

        builder.HasIndex(f => new { f.UserId, f.SimulationKey }).IsUnique();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
