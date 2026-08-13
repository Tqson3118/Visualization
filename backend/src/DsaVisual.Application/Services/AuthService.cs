using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Services;

/// <summary>
/// AuthService thật theo SDD §5.4 / API_REFERENCE.md §4.1.
/// - Hash mật khẩu: PBKDF2-SHA256 100.000 vòng + salt 16 byte (SDD §5.6/§7.3.1 — tự triển khai, không phụ thuộc ASP.NET Identity).
/// - Khóa tạm: LoginAttemptTracker (5 lần/15 phút — single-instance; OTP verify dùng key riêng "otp:{userId}").
/// - Refresh: rotate-invalidate (SDD §7.3.5), replay token đã rotate → thu hồi cả chuỗi phiên.
/// - Email: SMTP thiếu → KHÔNG block luồng; KHÔNG log reset token / mã OTP (finding security#5).
/// </summary>
public sealed class AuthService(
    AppDbContext db,
    ITokenService tokens,
    IDateTimeProvider clock,
    IConfiguration config,
    ISettingService settings,
    LoginAttemptTracker loginAttempts,
    ILogger<AuthService> logger,
    Func<string>? otpGenerator = null) : IAuthService
{
    private const string SettingAllowedDomains = "allowed.email.domains";
    private const string SettingMinPasswordLength = "password.policy.minLength";

    // ── 2FA email (GP-T2 — FR-1.11) ──
    private const string OtpPurposeEnable2Fa = "enable_2fa";
    private const int OtpLifetimeMinutes = 5;
    private const int OtpExpiresInSeconds = OtpLifetimeMinutes * 60;

    /// <summary>Hash giả cho PBKDF2 dummy khi user không tồn tại — chống timing oracle (finding security#15).</summary>
    private static readonly string DummyPasswordHash =
        PasswordHasher.Hash("timing-oracle-" + Guid.NewGuid().ToString("N"));

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

        // Finding #19 (THAP): user + refresh token trong 1 transaction — tránh "user tồn tại nhưng không có phiên"
        // khi SaveChanges của IssueTokensAsync fail sau khi user đã commit.
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        db.Users.Add(user);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueEmailViolation(ex))
        {
            // Finding #2 (CAO): race 2 register cùng email song song → cả 2 pass check AnyAsync ở trên,
            // 1 request vấp unique index IX_Users_Email → trả EMAIL_EXISTS (409) thay vì để DbUpdateException lan ra 500.
            return Result<RefreshResponse>.Fail(ErrorCodes.EMAIL_EXISTS, "Email đã được sử dụng", new()
            {
                ["email"] = ["Email đã được sử dụng"]
            });
        }

        logger.LogInformation("User {UserId} registered with role {Role}", user.Id, user.Role);
        var result = await IssueTokensAsync(user, ipAddress, ct);
        if (!result.IsSuccess)
        {
            await tx.RollbackAsync(ct);
            return result;
        }

        await tx.CommitAsync(ct);
        return result;
    }

    // ── Đăng nhập ─────────────────────────────────────────────

    public async Task<Result<RefreshResponse>> LoginAsync(LoginRequest request, string? ipAddress, CancellationToken ct)
    {
        var email = NormalizeEmail(request.Email);
        var user = await db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email && u.DeletedAt == null, ct);

        if (user is null)
        {
            // Finding #15 (THAP): user không tồn tại → vẫn chạy PBKDF2 với hash giả để thời gian
            // phản hồi ~constant với user tồn tại (không lộ email qua timing oracle).
            PasswordHasher.Verify(request.Password, DummyPasswordHash);
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

        // Rotate: thu hồi token cũ bằng UPDATE ĐIỀU KIỆN atomic (finding #3 — TOCTOU):
        // 2 refresh song song cùng token → chỉ 1 request thắng (rows=1); request kia rows=0 → replay → thu hồi chuỗi.
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var revoked = await TryRevokeRefreshTokenAsync(token.Id, clock.UtcNow, ct);
        if (revoked == 0)
        {
            // Token đã bị rotate/revoke bởi request khác chạy đúng lúc (hoặc replay) → thu hồi cả chuỗi phiên (SDD §7.3.5)
            logger.LogWarning("Refresh token race/replay detected for user {UserId} from {Ip}", token.UserId, ipAddress);
            await RevokeAllTokensAsync(token.UserId, ct);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Result<RefreshResponse>.Fail(ErrorCodes.REFRESH_INVALID, "Phiên đăng nhập không hợp lệ");
        }

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
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Finding #3: dòng Users đổi (RowVersion) giữa lúc đọc và ghi → 409 CONFLICT
            return Result<UserSummary>.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }
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
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Finding #3: dòng Users đổi (RowVersion) giữa lúc đọc và ghi → 409 CONFLICT
            return Result.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }

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
            // Finding #15: không log email unknown (tránh email enumeration qua log)
            logger.LogDebug("Forgot-password requested for unknown account");
            return Result.Ok();
        }

        var rawToken = tokens.CreateRefreshToken();   // 64 byte base64url — đủ mạnh cho token 1 lần

        // Finding #10 (TRUNG): token mới PHẢI vô hiệu hóa token cũ chưa dùng của cùng user
        // (pattern giống Send2FaCodeAsync) — trong cùng transaction với insert.
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        var stale = await db.PasswordResetTokens
            .Where(t => t.UserId == user.Id && !t.Used)
            .ToListAsync(ct);
        foreach (var old in stale)
        {
            old.Used = true;
        }

        db.PasswordResetTokens.Add(new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokens.HashToken(rawToken),
            ExpiresAt = clock.UtcNow.AddMinutes(30),  // 30 phút (SDD §7.3.6)
            Used = false,
            CreatedAt = clock.UtcNow
        });
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        // SMTP thiếu → KHÔNG block luồng; KHÔNG log reset token (finding security#5)
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

        // Finding #8 (TRUNG): token dùng ĐÚNG 1 lần — consume bằng UPDATE điều kiện atomic:
        // 2 reset song song cùng token → 1 request rows=1 (thắng), request kia rows=0 → RESET_TOKEN_INVALID
        // (KHÔNG rơi vào nhánh 409 CONFLICT của DbUpdateConcurrencyException như trước).
        var consumed = await TryConsumeResetTokenAsync(token.Id, clock.UtcNow, ct);
        if (consumed == 0)
        {
            await tx.RollbackAsync(ct);
            return Result.Fail(ErrorCodes.RESET_TOKEN_INVALID, "Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn", new()
            {
                ["token"] = ["Link đặt lại mật khẩu không hợp lệ hoặc đã hết hạn"]
            });
        }

        var user = await db.Users.FirstAsync(u => u.Id == token.UserId, ct);
        user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
        user.UpdatedAt = clock.UtcNow;

        await RevokeAllTokensAsync(token.UserId, ct);
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Finding #3: dòng Users đổi (RowVersion) giữa lúc đọc và ghi → rollback, 409 CONFLICT
            await tx.RollbackAsync(ct);
            return Result.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }
        await tx.CommitAsync(ct);

        logger.LogInformation("Password reset for user {UserId}", token.UserId);
        return Result.Ok();
    }

    // ── 2FA email (GP-T2 — FR-1.11) ──────────────────────────

    /// <summary>
    /// Bật/tắt 2FA (API_REFERENCE §4.12). Bật PHẢI qua mã OTP (POST /auth/2fa/send + /verify)
    /// — không cho bật trực tiếp để tránh attacker chiếm quyền khóa tài khoản chủ sở hữu.
    /// Tắt: cho phép trực tiếp vì người dùng đã đăng nhập (mật khẩu + token).
    /// </summary>
    public async Task<Result<Toggle2FaResponse>> Toggle2FaAsync(int userId, Toggle2FaRequest request, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        if (user is null)
        {
            return Result<Toggle2FaResponse>.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        if (!request.Enabled)
        {
            if (user.TwoFactorEnabled)
            {
                user.TwoFactorEnabled = false;
                user.UpdatedAt = clock.UtcNow;
                try
                {
                    await db.SaveChangesAsync(ct);
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Finding #3: dòng Users đổi (RowVersion) giữa lúc đọc và ghi → 409 CONFLICT
                    return Result<Toggle2FaResponse>.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
                }
                logger.LogInformation("User {UserId} disabled 2FA", user.Id);
            }

            return Result<Toggle2FaResponse>.Ok(new Toggle2FaResponse(false, "Đã tắt xác thực hai lớp"));
        }

        if (user.TwoFactorEnabled)
        {
            return Result<Toggle2FaResponse>.Fail(ErrorCodes.TWO_FA_ALREADY_ENABLED, "Xác thực hai lớp đã được bật");
        }

        return Result<Toggle2FaResponse>.Fail(ErrorCodes.OTP_REQUIRED,
            "Bật 2FA cần xác nhận mã: gọi POST /auth/2fa/send để nhận mã OTP, rồi POST /auth/2fa/verify");
    }

    /// <summary>
    /// Sinh mã OTP 6 số (hiệu lực 5 phút, dùng 1 lần), lưu SHA256 hash vào OtpCodes, gửi qua email.
    /// SMTP chưa cấu hình → KHÔNG block luồng; KHÔNG log mã OTP (finding security#5).
    /// </summary>
    public async Task<Result<Send2FaResponse>> Send2FaCodeAsync(int userId, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        if (user is null)
        {
            return Result<Send2FaResponse>.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        if (user.TwoFactorEnabled)
        {
            return Result<Send2FaResponse>.Fail(ErrorCodes.TWO_FA_ALREADY_ENABLED, "Xác thực hai lớp đã được bật");
        }

        // otpGenerator là seam cho test (ghi nhận mã) — production dùng GenerateOtpCode crypto-random
        var code = otpGenerator?.Invoke() ?? GenerateOtpCode();
        var now = clock.UtcNow;

        // Chỉ mã mới nhất có hiệu lực — vô hiệu hóa mã cũ chưa dùng cùng purpose
        var active = await db.OtpCodes
            .Where(o => o.UserId == user.Id && o.Purpose == OtpPurposeEnable2Fa && !o.Used)
            .ToListAsync(ct);
        foreach (var otp in active)
        {
            otp.Used = true;
        }

        db.OtpCodes.Add(new OtpCode
        {
            UserId = user.Id,
            CodeHash = HashOtpCode(code),
            Purpose = OtpPurposeEnable2Fa,
            ExpiresAt = now.AddMinutes(OtpLifetimeMinutes),
            Used = false,
            CreatedAt = now
        });
        await db.SaveChangesAsync(ct);

        // Finding #16: gửi OTP mới thành công → reset counter thử sai "otp:{userId}" để user
        // không bị ACCOUNT_LOCKED 15 phút vì đã sai 5 lần với mã cũ (không có đường hồi phục).
        loginAttempts.Reset("otp:" + user.Id);

        await Send2FaCodeEmailAsync(user, code, ct);
        return Result<Send2FaResponse>.Ok(new Send2FaResponse(
            "Mã xác thực đã được gửi qua email (hiệu lực 5 phút)", OtpExpiresInSeconds));
    }

    /// <summary>
    /// Xác nhận mã OTP: đúng + chưa dùng + chưa hết hạn → đánh dấu Used + bật 2FA cho tài khoản.
    /// </summary>
    public async Task<Result<Toggle2FaResponse>> Verify2FaCodeAsync(int userId, Verify2FaRequest request, CancellationToken ct)
    {
        var code = request.Code?.Trim() ?? string.Empty;
        if (code.Length != 6 || !code.All(char.IsAsciiDigit))
        {
            return Result<Toggle2FaResponse>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Mã xác thực phải là 6 chữ số", new() { ["code"] = ["Mã xác thực phải là 6 chữ số"] });
        }

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId && u.DeletedAt == null, ct);
        if (user is null)
        {
            return Result<Toggle2FaResponse>.Fail(ErrorCodes.NOT_FOUND, "Người dùng không tồn tại");
        }

        if (user.TwoFactorEnabled)
        {
            return Result<Toggle2FaResponse>.Fail(ErrorCodes.TWO_FA_ALREADY_ENABLED, "Xác thực hai lớp đã được bật");
        }

        // Finding #16 (THAP): giới hạn số lần thử sai OTP — 5 lần / 15 phút (LoginAttemptTracker,
        // key riêng "otp:{userId}" — không trộn counter với login). Khóa trước khi verify như LoginAsync.
        var otpKey = "otp:" + user.Id;
        if (loginAttempts.IsLocked(otpKey))
        {
            return Result<Toggle2FaResponse>.Fail(ErrorCodes.ACCOUNT_LOCKED,
                "Tài khoản đã bị khóa tạm thời do nhập mã xác thực sai nhiều lần. Vui lòng thử lại sau 15 phút");
        }

        var hash = HashOtpCode(code);
        var otp = await db.OtpCodes.AsNoTracking()
            .Where(o => o.UserId == user.Id && o.Purpose == OtpPurposeEnable2Fa && o.CodeHash == hash)
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync(ct);

        if (otp is null)
        {
            loginAttempts.RecordFailure(otpKey);
            logger.LogWarning("Failed 2FA verify (wrong code) for user {UserId}", user.Id);
            return Result<Toggle2FaResponse>.Fail(ErrorCodes.OTP_INVALID, "Mã xác thực không đúng");
        }

        if (otp.Used)
        {
            return Result<Toggle2FaResponse>.Fail(ErrorCodes.OTP_USED, "Mã xác thực đã được sử dụng");
        }

        if (otp.ExpiresAt <= clock.UtcNow)
        {
            return Result<Toggle2FaResponse>.Fail(ErrorCodes.OTP_EXPIRED,
                "Mã xác thực đã hết hạn — hãy gọi lại POST /auth/2fa/send để nhận mã mới");
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        // Finding #9 (TRUNG): OTP dùng ĐÚNG 1 lần — consume bằng UPDATE điều kiện atomic:
        // 2 verify song song cùng OTP → 1 request rows=1 (thắng), request kia rows=0 → OTP_USED (KHÔNG 409).
        var consumed = await TryConsumeOtpAsync(otp.Id, clock.UtcNow, ct);
        if (consumed == 0)
        {
            await tx.RollbackAsync(ct);
            return Result<Toggle2FaResponse>.Fail(ErrorCodes.OTP_USED, "Mã xác thực đã được sử dụng");
        }

        user.TwoFactorEnabled = true;
        user.UpdatedAt = clock.UtcNow;
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Finding #3: dòng Users đổi (RowVersion) giữa lúc đọc và ghi → rollback, 409 CONFLICT
            await tx.RollbackAsync(ct);
            return Result<Toggle2FaResponse>.Fail(ErrorCodes.CONFLICT, "Dữ liệu vừa được cập nhật, hãy thử lại");
        }
        await tx.CommitAsync(ct);

        loginAttempts.Reset(otpKey);
        logger.LogInformation("User {UserId} enabled 2FA (email {Email})", user.Id, user.Email);
        return Result<Toggle2FaResponse>.Ok(new Toggle2FaResponse(true, "Đã bật xác thực hai lớp"));
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
            // SMTP chưa cấu hình → KHÔNG block luồng; KHÔNG log reset token/link (finding security#5)
            logger.LogWarning("SMTP chưa cấu hình — không gửi được email đặt lại mật khẩu cho user {UserId}", user.Id);
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
            // Email lỗi KHÔNG block luồng quên mật khẩu (quyết định chốt SDD §5.6); KHÔNG log reset token (finding security#5)
            logger.LogError(ex, "Gửi email đặt lại mật khẩu thất bại cho user {UserId}", user.Id);
        }
    }

    private async Task Send2FaCodeEmailAsync(User user, string code, CancellationToken ct)
    {
        var smtpHost = config["DSA:Email:SmtpHost"];

        if (string.IsNullOrWhiteSpace(smtpHost))
        {
            // SMTP chưa cấu hình → KHÔNG block luồng; KHÔNG log mã OTP (finding security#5)
            logger.LogWarning("SMTP chưa cấu hình — không gửi được mã 2FA cho user {UserId}", user.Id);
            return;
        }

        try
        {
            using var smtp = new SmtpClient(smtpHost, config.GetValue("DSA:Email:SmtpPort", 1025))
            {
                Timeout = 10_000   // timeout ngắn — không giữ request (GP-T2)
            };
            await smtp.SendMailAsync(
                config["DSA:Email:From"] ?? "no-reply@dsa-visual.local",
                user.Email,
                "Mã xác thực 2FA — DSA Visual",
                $"Mã xác thực hai lớp (2FA) của bạn là: {code}\n\n" +
                $"Mã có hiệu lực {OtpLifetimeMinutes} phút và chỉ dùng được 1 lần.\n" +
                "Nếu bạn không yêu cầu, hãy bỏ qua email này.", ct);
        }
        catch (Exception ex)
        {
            // Email lỗi KHÔNG block luồng (SDD §5.6); KHÔNG log mã OTP (finding security#5)
            logger.LogError(ex, "Gửi email mã 2FA thất bại cho user {UserId}", user.Id);
        }
    }

    private static string GenerateOtpCode() =>
        RandomNumberGenerator.GetInt32(0, 1_000_000).ToString("D6");   // 6 chữ số, crypto-random

    private static string HashOtpCode(string code) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code))).ToLowerInvariant();

    /// <summary>
    /// True khi DbUpdateException là unique violation trên IX_Users_Email (SQL Server 2601/2627) —
    /// finding #2: race register cùng email → trả EMAIL_EXISTS thay vì 500.
    /// </summary>
    private static bool IsUniqueEmailViolation(DbUpdateException ex)
    {
        for (var current = (Exception?)ex; current is not null; current = current.InnerException)
        {
            if (current is SqlException { Number: 2601 or 2627 }
                || current.Message.Contains("IX_Users_Email", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Thu hồi refresh token bằng UPDATE ĐIỀU KIỆN atomic (finding #3 — TOCTOU):
    /// <c>UPDATE RefreshTokens SET RevokedAt = @now WHERE Id = @id AND RevokedAt IS NULL</c>.
    /// Trả rows affected — 0 nghĩa là token đã bị request khác rotate/revoke → replay.
    /// InMemory (unit test) không hỗ trợ ExecuteUpdate → fallback đọc tracked với cùng điều kiện.
    /// </summary>
    private async Task<int> TryRevokeRefreshTokenAsync(int id, DateTime now, CancellationToken ct)
    {
        if (db.Database.IsRelational())
        {
            return await db.RefreshTokens
                .Where(t => t.Id == id && t.RevokedAt == null)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.RevokedAt, now), ct);
        }

        var tracked = await db.RefreshTokens.FirstOrDefaultAsync(t => t.Id == id && t.RevokedAt == null, ct);
        if (tracked is null)
        {
            return 0;
        }

        tracked.RevokedAt = now;
        return 1;
    }

    /// <summary>
    /// Consume PasswordResetToken bằng UPDATE ĐIỀU KIỆN atomic (finding #8 — token dùng đúng 1 lần):
    /// <c>UPDATE PasswordResetTokens SET Used = 1 WHERE Id = @id AND Used = 0 AND ExpiresAt &gt; @now</c>.
    /// Trả rows affected — 0 → RESET_TOKEN_INVALID (loser của race, KHÔNG phải 409).
    /// </summary>
    private async Task<int> TryConsumeResetTokenAsync(int id, DateTime now, CancellationToken ct)
    {
        if (db.Database.IsRelational())
        {
            return await db.PasswordResetTokens
                .Where(t => t.Id == id && !t.Used && t.ExpiresAt > now)
                .ExecuteUpdateAsync(s => s.SetProperty(t => t.Used, true), ct);
        }

        var tracked = await db.PasswordResetTokens.FirstOrDefaultAsync(t => t.Id == id && !t.Used && t.ExpiresAt > now, ct);
        if (tracked is null)
        {
            return 0;
        }

        tracked.Used = true;
        return 1;
    }

    /// <summary>
    /// Consume OtpCode bằng UPDATE ĐIỀU KIỆN atomic (finding #9 — OTP dùng đúng 1 lần):
    /// <c>UPDATE OtpCodes SET Used = 1 WHERE Id = @id AND Used = 0 AND ExpiresAt &gt; @now</c>.
    /// Trả rows affected — 0 → OTP_USED (loser của race, KHÔNG phải 409).
    /// </summary>
    private async Task<int> TryConsumeOtpAsync(int id, DateTime now, CancellationToken ct)
    {
        if (db.Database.IsRelational())
        {
            return await db.OtpCodes
                .Where(o => o.Id == id && !o.Used && o.ExpiresAt > now)
                .ExecuteUpdateAsync(s => s.SetProperty(o => o.Used, true), ct);
        }

        var tracked = await db.OtpCodes.FirstOrDefaultAsync(o => o.Id == id && !o.Used && o.ExpiresAt > now, ct);
        if (tracked is null)
        {
            return 0;
        }

        tracked.Used = true;
        return 1;
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
