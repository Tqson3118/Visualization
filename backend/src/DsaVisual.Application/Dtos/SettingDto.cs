namespace DsaVisual.Application.Dtos;

/// <summary>
/// Cấu hình hệ thống theo shape FE SystemSettingsDto — GET/PUT /settings (API_REFERENCE.md §4.10).
/// Map từ bảng Settings key-value: site.name, allowed.email.domains (phân tách phẩy),
/// password.policy.minLength, upload.maxSizeMb. Các field còn lại là mặc định FE
/// (policy chữ hoa/số/ký tự đặc biệt được PasswordPolicy áp dụng cố định; sandbox chạy client-side
/// nên backend không lưu) — giữ default khớp FE (AdminSettingsView) để form không hiển thị trống.
/// </summary>
public sealed class SystemSettingsDto
{
    public string SiteName { get; set; } = "DSA Visual";
    public List<string> AllowedDomains { get; set; } = [];
    public PasswordPolicySettingsDto PasswordPolicy { get; set; } = new();
    public int UploadMaxMb { get; set; } = 5;
    public int SandboxSeconds { get; set; } = 10;
    public int SandboxMemoryMb { get; set; } = 64;
}

/// <summary>Chính sách mật khẩu — khớp SystemSettingsDto.passwordPolicy (FE).</summary>
public sealed class PasswordPolicySettingsDto
{
    public int MinLength { get; set; } = 8;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSpecial { get; set; } = true;
}
