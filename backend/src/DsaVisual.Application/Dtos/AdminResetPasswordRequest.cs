namespace DsaVisual.Application.Dtos;

/// <summary>Admin đặt lại mật khẩu cho người dùng — POST /users/{id}/reset-password (API_REFERENCE.md §4.8).</summary>
public sealed class AdminResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
