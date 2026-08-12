namespace DsaVisual.Application.Dtos;

/// <summary>Đăng nhập — API_REFERENCE.md §3.2.</summary>
public sealed class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
