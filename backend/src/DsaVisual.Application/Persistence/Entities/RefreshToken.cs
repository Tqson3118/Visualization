namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Refresh token (rotate-invalidate) — SDD §7.3.5 (NFR-9).</summary>
public sealed class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;            // UNIQUE — SHA256 token thô
    public string? PreviousTokenHash { get; set; }                   // token bị thay (rotate-invalidate)
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? CreatedByIp { get; set; }
    public DateTime CreatedAt { get; set; }
}
