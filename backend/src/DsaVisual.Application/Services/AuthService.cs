using System.Net.Mail;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Services;

/// <summary>
/// AuthService thật theo SDD §5.4 / API_REFERENCE.md §4.1.
/// - Hash mật khẩu: PBKDF2-SHA256 100.000 vòng + salt 16 byte (SDD §5.6/§7.3.1 — tự triển khai, không phụ thuộc ASP.NET Identity).
/// - Khóa tạm: LoginAttemptTracker (5 lần/15 phút — single-instance).
/// - Refresh: rotate-invalidate (SDD §7.3.5), replay token đã rotate → thu hồi cả chuỗi phiên.
/// - Email: SMTP thiếu → KHÔNG block luồng, ghi log dev link đặt lại mật khẩu (SDD §5.6).
/// </summary>
public sealed class AuthService(
    AppDbContext db,
    ITokenService tokens,
    IDateTimeProvider clock,
    IConfiguration config,
    ISettingService settings,
    LoginAttemptTracker loginAttempts,
    ILogger<AuthService> logger) : IAuthService
{
    private const string SettingAllowedDomains = "allowed.email.domains";
    private const string SettingMinPasswordLength = "password.policy.minLength";

    // ── Đăng ký ──────────────────────────────────────────────

    public async Task<Result<RefreshResponse>> RegisterAsync(RegisterRequest request, string? ipAddress, CancellationToken ct)
    {
        var email = NormalizeEmail(request.Email);
        if (!IsValidEmail(email))
        {
            return Result<RefreshResponse>.Fail(ErrorCodes.INVALID_EMAIL, "Định dạng email sai", new()
            {
                ["email"] = ["Email không hợp lệ"]
            });
        }

        if (await db.Users.AsNoTracking().AnyAsync(u => u.Email == email && u.DeletedAt == null, ct))
        {
            return Result<RefreshResponse>.Fail(ErrorCodes.EMAIL_EXISTS, "Email đã được sử dụng", new()
            {
                ["email"] = ["Email đã được sử dụng"]
            });
        }

        // Domain check nếu setting được bật (SDD §7.5: allowed.email.domains)
        var allowedDomains = await settings.GetValueAsync(SettingAllowedDomains, ct);
        if (!string.IsNullOrWhiteSpace(allowedDomains))
        {
            var domains = allowedDomains.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var emailDomain = email[(email.IndexOf('@') + 1)..];
            if (domains.Length > 0 && !domains.Contains(emailDomain, StringComparer.OrdinalIgnoreCase))
            {
                return Result<RefreshResponse>.Fail(ErrorCodes.DOMAIN_NOT_ALLOWED,
                    "Email không thuộc domain được phép đăng ký", new() { ["email"] = ["Email không thuộc domain được phép đăng ký"] });
            }
        }

        var minLength = await GetMinPasswordLengthAsync(ct);
        var policyErrors = PasswordPolicy.Validate(request.Password, minLength);
        if (policyErrors.Count > 0)
        {
            return Result<RefreshResponse>.Fail(ErrorCodes.WEAK_PASSWORD,
                "Mật khẩu yếu", new() { ["password"] = policyErrors.ToArray() });
        }

        var now = clock.UtcNow;
        var user = new User
        {
            Email = email,
            PasswordHash = PasswordHasher.Hash(request.Password),
            DisplayName = request.DisplayName.Trim(),
            Role = request.IsTeacher ? UserRole.TeacherPending : UserRole.Student,
            IsActive = true,
            Hearts = 10,
            HeartsMax = 10,
            LastHeartAt = now,
            CreatedAt = now
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("User {UserId} registered with role {Role}", user.Id, user.Role);
        var result = await IssueTokensAsync(user, ipAddress, ct);
        return result.IsSuccess
            ? Result<RefreshResponse>.Ok(result.Value!)
            : result;
    }

    // ── Đăng nhập ─────────────────────────────────────────────

    public async Task<Result<RefreshResponse>> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct)
    {
        var email = NormalizeEmail(request.Email);
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null, ct);

        if (user is null)
        {
            return Result<RefreshResponse>.Fail(ErrorCodes.INVALID_CREDENTIALS, "Email hoặc mật khẩu không đúng");
        }

        if (loginAttempts.IsLocked(user.Id))
        {
            return Result<RefreshResponse>.Fail(ErrorCodes.ACCOUNT_LOCKED,
                "Tài khoản đã bị khóa tạm thời do đăng nhập sai nhiều lần. Vui lòng thử lại sau 15 phút");
        }

        if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            loginAttempts.RecordFailure(user.Id);
            logger.LogWarning("Failed login for user {UserId} from {Ip}", user.Id, ipAddress);
            return loginAttempts.IsLocked(user.Id)
                ? Result<RefreshResponse>.Fail(ErrorCodes.ACCOUNT_LOCKED,
                    "Tài khoản đã bị khóa tạm thời do đăng nhập sai nhiều lần. Vui lòng thử lại sau 15 phút")
                : Result<RefreshResponse>.Fail(ErrorCodes.INVALID_CREDENTIALS, "Email hoặc mật khẩu không đúng");
        }

        loginAttempts.Reset(user.Id);

        if (!user.IsActive)
        {
            return Result<RefreshResponse>.Fail(ErrorCodes.ACCOUNT_DISABLED, "Tài khoản chưa được kích hoạt");
        }

        logger.LogInformation("User {UserId} logged in from {Ip}", user.Id, ipAddress);
        return await IssueTokensAsync(user, ipAddress, ct);
    }

    // ── Refresh (rotate-invalidate) ───────────────────────────

    public async Task<Result<RefreshResponse>> RefreshAsync(string? refreshToken, string? ipAddress, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Result<RefreshResponse>.Fail(ErrorCodes.REFRESH_INVALID, "Refresh token không hợp lệ");
        }

        var tokenHash = tokens.HashToken(refreshToken);
        var token = await db.RefreshTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (token is null)
        {
            return Result<RefreshResponse>.Fail(ErrorCodes.REFRESH_INVALID, "Refresh token không hợp lệ");
        }

        // Replay: token đã bị rotate (nằm ở PreviousTokenHash của token mới) → thu hồi cả chuỗi phiên (SDD §7.3.5)
        var isReplay = await db.RefreshTokens.AsNoTracking()
            .AnyAsync(t => t.PreviousTokenHash == tokenHash, ct);
        if (isReplay)
        {
            logger.LogWarning("Refresh token replay detected for user {UserId} from {Ip}", token.UserId, ipAddress);
            await RevokeAllTokensAsync(token.UserId, ct);
            await db.SaveChangesAsync(ct);
            return Result<RefreshResponse>.Fail(ErrorCodes.REFRESH_INVALID, "Phiên đăng nhập không hợp lệ");
        }

        if (token.RevokedAt != null || token.ExpiresAt <= clock.UtcNow)
        {
            return Result<RefreshResponse>.Fail(ErrorCodes.REFRESH_INVALID, "Refresh token đã hết hạn hoặc bị thu hồi");
        }

        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == token.UserId && u.DeletedAt == null, ct);
        if (user is null || !user.IsActive)
        {
            return Result<RefreshResponse>.Fail(ErrorCodes.REFRESH_INVALID, "Tài khoản không tồn tại hoặc bị khóa");
        }

        // Rotate: thu hồi token cũ, tạo token mới có PreviousTokenHash = token cũ
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var oldToken = await db.RefreshTokens.FirstAsync(t => t.Id == token.Id, ct);
        oldToken.RevokedAt = clock.UtcNow;

        var newRefresh = tokens.CreateRefreshToken();
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokens.HashToken(newRefresh),
            PreviousTokenHash = tokenHash,
            ExpiresAt = clock.UtcNow.AddDays(config.GetValue("DSA:Jwt:RefreshTokenDays", 7)),
            CreatedByIp = ipAddress,
            CreatedAt = clock.UtcNow
        });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var access = tokens.CreateAccessToken(user.Id, RoleNames.ToApi(user.Role));
        return Result<RefreshResponse>.Ok(new RefreshResponse
        {
            AccessToken = access.Token,
            ExpiresIn = config.GetValue("DSA:Jwt:AccessTokenMinutes", 60) * 60,
            User = ToUserSummary(user, maskEmail: false),
            RefreshToken = newRefresh
        });
    }

    // ── Logout ────────────────────────────────────────────────

    public async Task<Result> LogoutAsync(int userId, CancellationToken ct)
    {
        await RevokeAllTokensAsync(userId, ct);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("User {UserId} logged out", userId);
        return Result.Ok();
    }

    // ── Me ────────────────────────────────────────────────────

    public async Task<Result<UserSummary>> GetMeAsync(int userId, CancellationToken ct)
    {
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        return user is null
            ? Result<UserSummary>.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại")
            : Result<UserSummary>.Ok(ToUserSummary(user, maskEmail: false));
    }

    public async Task<Result<UserSummary>> UpdateMeAsync(int userId, UpdateProfileRequest request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        if (user is null)
        {
            return Result<UserSummary>.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        if (request.DisplayName is not null)
        {
            var name = request.DisplayName.Trim();
            if (name.Length is < 2 or > 100)
            {
                return Result<UserSummary>.Fail(ErrorCodes.VALIDATION_FAILED,
                    "Tên hiển thị phải từ 2 đến 100 ký tự", new() { ["displayName"] = ["Tên hiển thị phải từ 2 đến 100 ký tự"] });
            }

            user.DisplayName = name;
        }

        if (request.AvatarUrl is not null)
        {
            if (request.AvatarUrl.Length > 500)
            {
                return Result<UserSummary>.Fail(ErrorCodes.VALIDATION_FAILED,
                    "AvatarUrl không được vượt quá 500 ký tự", new() { ["avatarUrl"] = ["AvatarUrl không được vượt quá 500 ký tự"] });
            }

            user.AvatarUrl = request.AvatarUrl;
        }

        user.UpdatedAt = clock.UtcNow;
        await db.SaveChangesAsync(ct);
        return Result<UserSummary>.Ok(ToUserSummary(user, maskEmail: false));
    }

    // ── Đổi mật khẩu ──────────────────────────────────────────

    public async Task<Result> ChangePasswordAsync(int userId, ChangePasswordRequest request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        if (user is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        if (!PasswordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return Result.Fail(ErrorCodes.OLD_PASSWORD_WRONG, "Mật khẩu hiện tại không đúng", new()
            {
                ["currentPassword"] = ["Mật khẩu hiện tại không đúng"]
            });
        }

        if (PasswordHasher.Verify(request.NewPassword, user.PasswordHash))
        {
            return Result.Fail(ErrorCodes.PASSWORD_SAME, "Mật khẩu mới trùng với mật khẩu cũ", new()
            {
                ["newPassword"] = ["Mật khẩu mới trùng với mật khẩu cũ"]
            });
        }

        var minLength = await GetMinPasswordLengthAsync(ct);
        var policyErrors = PasswordPolicy.Validate(request.NewPassword, minLength);
        if (policyErrors.Count > 0)
        {
            return Result.Fail(ErrorCodes.WEAK_PASSWORD, "Mật khẩu yếu", new() { ["password"] = policyErrors.ToArray() });
        }

        user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        user.UpdatedAt = clock.UtcNow;

        // Thu hồi toàn bộ refresh token (phiên khác) — API_REFERENCE.md §4.1
        await RevokeAllTokensAsync(userId, ct);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("User {UserId} changed password", userId);
        return Result.Ok();
    }

    // ── Quên mật khẩu ─────────────────────────────────────────

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct)
    {
        var email = NormalizeEmail(request.Email);
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null, ct);

        // Trả thông báo chung — không lộ email tồn tại hay không (API_REFERENCE.md §4.1)
        if (user is null)
        {
            logger.LogInformation("Forgot-password requested for unknown email {Email}", email);
            return Result.Ok();
        }

        var rawToken = tokens.CreateRefreshToken();   // 64 byte base64url — đủ mạnh cho token 1 lần
        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokens.HashToken(rawToken),
            ExpiresAt = clock.UtcNow.AddMinutes(30),  // 30 phút (SDD §7.3.6)
            Used = false,
            CreatedAt = clock.UtcNow
        });
        await db.SaveChangesAsync(ct);

        // SMTP thiếu → KHÔNG block luồng; ghi log dev link (SDD §5.6)
        await SendResetPasswordEmailAsync(user, rawToken, ct);
        return Result.Ok();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct)
    {
        var tokenHash = tokens.HashToken(request.Token);
        var token = await db.PasswordResetTokens.AsNoTracking()
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

        if (token is null || token.Used || token.ExpiresAt <= clock.UtcNow)
        {
            return Result.Fail(ErrorCodes.RESET_TOKEN_INVALID, "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn", new()
            {
                ["token"] = ["Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn"]
            });
        }

        var minLength = await GetMinPasswordLengthAsync(ct);
        var policyErrors = PasswordPolicy.Validate(request.NewPassword, minLength);
        if (policyErrors.Count > 0)
        {
            return Result.Fail(ErrorCodes.WEAK_PASSWORD, "Mật khẩu yếu", new() { ["password"] = policyErrors.ToArray() });
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var reset = await db.PasswordResetTokens.FirstAsync(t => t.Id == token.Id, ct);
        reset.Used = true;

        var user = await db.Users.FirstAsync(u => u.Id == reset.UserId, ct);
        user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        user.UpdatedAt = clock.UtcNow;

        await RevokeAllTokensAsync(reset.UserId, ct);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        logger.LogInformation("Password reset for user {UserId}", reset.UserId);
        return Result.Ok();
    }

    // ── Private helpers ───────────────────────────────────────

    private async Task<Result<RefreshResponse>> IssueTokensAsync(User user, string? ipAddress, CancellationToken ct)
    {
        var refresh = tokens.CreateRefreshToken();
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokens.HashToken(refresh),
            ExpiresAt = clock.UtcNow.AddDays(config.GetValue("DSA:Jwt:RefreshTokenDays", 7)),
            CreatedByIp = ipAddress,
            CreatedAt = clock.UtcNow
        });
        await db.SaveChangesAsync(ct);

        var access = tokens.CreateAccessToken(user.Id, RoleNames.ToApi(user.Role));
        return Result<RefreshResponse>.Ok(new RefreshResponse
        {
            AccessToken = access.Token,
            ExpiresIn = config.GetValue("DSA:Jwt:AccessTokenMinutes", 60) * 60,
            User = ToUserSummary(user, maskEmail: false),
            RefreshToken = refresh
        });
    }

    private async Task RevokeAllTokensAsync(int userId, CancellationToken ct)
    {
        var active = await db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(ct);
        foreach (var token in active)
        {
            token.RevokedAt = clock.UtcNow;
        }
    }

    private async Task<int> GetMinPasswordLengthAsync(CancellationToken ct)
    {
        var value = await settings.GetValueAsync(SettingMinPasswordLength, ct);
        return int.TryParse(value, out var min) && min > 0 ? min : 8;
    }

    private async Task SendResetPasswordEmailAsync(User user, string rawToken, CancellationToken ct)
    {
        var smtpHost = config["DSA:Email:SmtpHost"];
        var resetLink = $"{config["DSA:App:BaseUrl"] ?? "http://localhost:5173"}/reset-password?token={rawToken}";

        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            // SMTP chưa cấu hình → ghi log dev, KHÔNG block luồng (SDD §5.6)
            logger.LogWarning("SMTP chưa cấu hình — link đặt lại mật khẩu (dev): {ResetLink}", resetLink);
            return;
        }

        try
        {
            using var smtp = new SmtpClient(smtpHost, config.GetValue("DSA:Email:SmtpPort", 1025));
            await smtp.SendMailAsync(
                config["DSA:Email:From"] ?? "no-reply@dsa-visual.local",
                user.Email,
                "Đặt lại mật khẩu — DSA Visual",
                $"Nhấn link sau để đặt lại mật khẩu (hiệu lực 30 phút): {resetLink}", ct);
        }
        catch (Exception ex)
        {
            // Email lỗi KHÔNG block luồng quên mật khẩu (quyết định chốt SDD §5.6)
            logger.LogError(ex, "Gửi email đặt lại mật khẩu thất bại cho user {UserId}; link dev: {ResetLink}", user.Id, resetLink);
        }
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    private static bool IsValidEmail(string email)
    {
        if (email.Length > 256)
        {
            return false;
        }

        try
        {
            var address = new MailAddress(email);
            return address.Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    internal static UserSummary ToUserSummary(User user, bool maskEmail) => new()
    {
        Id = user.Id,
        DisplayName = user.DisplayName,
        Email = maskEmail ? EmailMasker.Mask(user.Email) : user.Email,
        Role = RoleNames.ToApi(user.Role),
        AvatarUrl = user.AvatarUrl,
        CreatedAt = user.CreatedAt
    };
}
