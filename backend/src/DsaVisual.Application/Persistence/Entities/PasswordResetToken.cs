namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Token đặt lại mật khẩu — SDD §7.3.6.</summary>
public sealed class PasswordResetToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;            // UNIQUE
    public DateTime ExpiresAt { get; set; }                          // 30 phút
    public bool Used { get; set; }
    public DateTime CreatedAt { get; set; }
}
