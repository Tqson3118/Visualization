namespace DsaVisual.Application.Persistence.Entities;

/// <summary>
/// Mã OTP xác thực email khi đăng ký (B0) — người dùng chưa tồn tại nên tách bảng riêng với OtpCodes
/// (bảng đó FK UserId bắt buộc). Chỉ lưu SHA256 hash của mã/token — không lưu mã gốc.
/// Vòng đời 1 dòng: tạo (mã 6 số, 5 phút) → verify đúng → cấp otpToken (10 phút) → register tiêu token.
/// </summary>
public sealed class RegisterOtpCode
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;          // đã chuẩn hóa lowercase (≤ 256)
    public string CodeHash { get; set; } = string.Empty;        // SHA256 hex của mã 6 số (64 ký tự)
    public string? VerifyTokenHash { get; set; }                // SHA256 hex của otpToken — có sau khi verify mã
    public DateTime ExpiresAt { get; set; }                     // hạn của MÃ (5 phút từ lúc gửi)
    public DateTime? TokenExpiresAt { get; set; }               // hạn của otpToken (10 phút từ lúc verify)
    public int FailedAttempts { get; set; }                     // số lần verify sai mã
    public bool Used { get; set; }                              // true = đã bị vô hiệu (resend / thử sai quá 5 lần)
    public bool TokenUsed { get; set; }                         // true = otpToken đã tiêu cho 1 lần register
    public DateTime CreatedAt { get; set; }
}
