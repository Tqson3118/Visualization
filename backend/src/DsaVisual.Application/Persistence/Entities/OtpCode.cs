namespace DsaVisual.Application.Persistence.Entities;

/// <summary>
/// Mã OTP 1 lần cho 2FA email (FR-1.11) — bảng mới GP-T2 (SDD §7.3 không có sẵn; bổ sung migration AddOtpCodes).
/// Chỉ lưu SHA256 hash của mã — không lưu mã gốc; dùng 1 lần + hết hạn 5 phút.
/// </summary>
public sealed class OtpCode
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string CodeHash { get; set; } = string.Empty;            // SHA256 hex (64 ký tự)
    public string Purpose { get; set; } = string.Empty;              // "enable_2fa" | "login"
    public DateTime ExpiresAt { get; set; }                          // 5 phút
    public bool Used { get; set; }
    public DateTime CreatedAt { get; set; }
}
