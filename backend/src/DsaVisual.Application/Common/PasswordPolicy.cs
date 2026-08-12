namespace DsaVisual.Application.Common;

/// <summary>
/// Policy mật khẩu (API_REFERENCE.md §3.1 / SDD §7.5): ≥ 8 ký tự (minLength có thể cấu hình qua
/// setting <c>password.policy.minLength</c>), chữ hoa, số, ký tự đặc biệt.
/// Trả danh sách lỗi chi tiết cho mã lỗi WEAK_PASSWORD (details/fieldErrors).
/// </summary>
public static class PasswordPolicy
{
    public const int MaxLength = 64;

    public static IReadOnlyList<string> Validate(string? password, int minLength = 8)
    {
        var errors = new List<string>();

        if (string.IsNullOrEmpty(password))
        {
            errors.Add("Mật khẩu không được để trống");
            return errors;
        }

        if (password.Length < minLength)
        {
            errors.Add($"Mật khẩu phải có ít nhất {minLength} ký tự");
        }

        if (password.Length > MaxLength)
        {
            errors.Add($"Mật khẩu không được vượt quá {MaxLength} ký tự");
        }

        if (!password.Any(char.IsUpper))
        {
            errors.Add("Mật khẩu phải chứa ít nhất 1 chữ hoa");
        }

        if (!password.Any(char.IsDigit))
        {
            errors.Add("Mật khẩu phải chứa ít nhất 1 chữ số");
        }

        if (!password.Any(c => !char.IsLetterOrDigit(c)))
        {
            errors.Add("Mật khẩu phải chứa ít nhất 1 ký tự đặc biệt");
        }

        return errors;
    }
}
