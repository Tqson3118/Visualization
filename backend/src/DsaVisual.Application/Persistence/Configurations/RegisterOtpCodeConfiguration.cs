using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DsaVisual.Application.Persistence.Configurations;

/// <summary>RegisterOtpCodes (B0) — index tra cứu theo (Email, Used) khi verify + theo VerifyTokenHash khi register.</summary>
public sealed class RegisterOtpCodeConfiguration : IEntityTypeConfiguration<RegisterOtpCode>
{
    public void Configure(EntityTypeBuilder<RegisterOtpCode> builder)
    {
        builder.ToTable("RegisterOtpCodes");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.CodeHash)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(r => r.VerifyTokenHash)
            .HasMaxLength(64);

        // Lookup verify: mã chưa dùng mới nhất theo email
        builder.HasIndex(r => new { r.Email, r.Used });

        // Lookup register: tiêu token theo hash
        builder.HasIndex(r => r.VerifyTokenHash);
    }
}
