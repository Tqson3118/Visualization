namespace DsaVisual.Application.Dtos;

/// <summary>Đổi mật khẩu — PUT /auth/me/password (API_REFERENCE.md §4.1).</summary>
public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
