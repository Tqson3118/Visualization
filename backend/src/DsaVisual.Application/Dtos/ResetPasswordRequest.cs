namespace DsaVisual.Application.Dtos;

/// <summary>Đặt lại mật khẩu bằng token — POST /auth/reset-password (API_REFERENCE.md §4.1).</summary>
public sealed class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
