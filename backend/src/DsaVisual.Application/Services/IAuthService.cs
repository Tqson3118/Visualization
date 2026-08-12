using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

/// <summary>
/// Đăng ký, đăng nhập, refresh (rotate-invalidate), logout, khôi phục mật khẩu, khóa tạm (SDD §5.4).
/// </summary>
public interface IAuthService
{
    Task<Result<RefreshResponse>> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken ct);
    Task<Result<RefreshResponse>> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct);
    Task<Result<RefreshResponse>> RefreshAsync(string? refreshToken, string? ipAddress, CancellationToken ct);
    Task<Result> LogoutAsync(int userId, CancellationToken ct);
    Task<Result<UserSummary>> GetMeAsync(int userId, CancellationToken ct);
    Task<Result<UserSummary>> UpdateMeAsync(int userId, UpdateProfileRequest request, CancellationToken ct);
    Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct);
    Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct);

    // ── 2FA email (GP-T2, FR-1.11) ──
    Task<Result<Toggle2FaResponse>> Toggle2FaAsync(int userId, Toggle2FaRequest request, CancellationToken ct);
    Task<Result<Send2FaResponse>> Send2FaCodeAsync(int userId, CancellationToken ct);
    Task<Result<Toggle2FaResponse>> Verify2FaCodeAsync(int userId, Verify2FaRequest request, CancellationToken ct);
}

/// <summary>
/// Hỗ trợ mask email theo API_REFERENCE.md §3.14.2: Teacher thấy dạng `m***h@university.edu.vn`
/// (giữ ký tự đầu + ký tự cuối của local-part; local-part quá ngắn thì chỉ giữ ký tự đầu).
/// </summary>
public static class EmailMasker
{
    public static string Mask(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 0)
        {
            return "***";
        }

        var local = email[..at];
        var domain = email[at..];
        if (local.Length <= 1)
        {
            return $"***{domain}";
        }

        return local.Length == 2
            ? $"{local[0]}***{domain}"
            : $"{local[0]}***{local[^1]}{domain}";
    }
}
