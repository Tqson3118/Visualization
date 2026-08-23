namespace DsaVisual.Application.Dtos;

/// <summary>Gửi link khôi phục mật khẩu — POST /auth/forgot-password (API_REFERENCE.md §4.1).</summary>
public sealed class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}
