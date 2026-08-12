namespace DsaVisual.Application.Dtos;

/// <summary>Đăng ký tài khoản — API_REFERENCE.md §3.1.</summary>
public sealed class RegisterRequest
{
    public string DisplayName { get; set; } = string.Empty;   // 2-100
    public string Email { get; set; } = string.Empty;         // ≤ 256, lowercase
    public string Password { get; set; } = string.Empty;      // 8-64, chữ hoa + số + ký tự đặc biệt
    public bool IsTeacher { get; set; }                       // mặc định false
}
