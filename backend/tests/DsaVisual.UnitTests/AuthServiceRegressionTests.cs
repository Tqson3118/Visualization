using System.Text.RegularExpressions;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.UnitTests;

/// <summary>
/// TEST TÁI HIỆN (đỏ/fail) cho các lỗi AuthService KHÔNG cần DB thật
/// (nguồn: docs/work/backend-audit/findings-biz-services.md #10; findings-security.md #5, #16).
/// Mỗi test assert HÀNH VI ĐÚNG dự kiến sau fix — hiện tại PHẢI FAIL vì bug chưa sửa.
/// KHÔNG sửa code production — chỉ test.
/// Race thật (Task.WhenAll, SQL Server) nằm ở DsaVisual.IntegrationTests/AuthRegressionTests.cs.
/// </summary>
public class AuthServiceRegressionTests
{
    private readonly TestServices.FixedClock _clock = new();

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Messages.Add(formatter(state, exception));
    }

    private const string Email = "repro-auth@university.edu.vn";

    private TestServices.RecordingTokenService _tokens = null!;
    private TestServices.RecordingOtpGenerator _otp = null!;

    private async Task<(AuthService Service, CapturingLogger<AuthService> Logger, AppDbContext Db)> CreateAsync(string testName)
    {
        var db = TestServices.CreateInMemoryDb(testName);
        var logger = new CapturingLogger<AuthService>();
        // Finding security#5: production KHÔNG log token/OTP → test lấy token/OTP từ recorder seam
        // (bọc ITokenService thật — chỉ record raw token; OTP qua otpGenerator recorder).
        _tokens = new TestServices.RecordingTokenService(new TokenService(TestServices.CreateConfig()));
        _otp = new TestServices.RecordingOtpGenerator();
        var service = TestServices.CreateAuthService(db, _clock, testName, logger, _tokens, _otp.Generate);
        return (service, logger, db);
    }

    private async Task<int> RegisterUserAsync(AuthService service)
    {
        var result = await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Nguyễn Minh",
            Email = Email,
            Password = "MatKhau@123",
            IsTeacher = false
        }, null, CancellationToken.None);
        Assert.True(result.IsSuccess, result.ErrorMessage);
        return result.Value!.User.Id;
    }

    /// <summary>Raw reset token mới nhất (register tạo 1 refresh token đầu — các token sau là reset token).</summary>
    private string ExtractResetToken() => _tokens.CreatedRefreshTokens[^1];

    /// <summary>Mã OTP mới nhất recorder đã sinh cho Send2FaCodeAsync.</summary>
    private string ExtractOtpCode() => _otp.Last;

    // ── #10 (TRUNG, biz): ForgotPassword tạo token mới PHẢI vô hiệu hóa token cũ chưa dùng ──

    /// <summary>
    /// Bug: ForgotPasswordAsync (AuthService.cs:345-354) tạo PasswordResetToken mới nhưng KHÔNG đánh
    /// dấu Used các token cũ chưa dùng của cùng user (khác pattern Send2FaCodeAsync:440-446) →
    /// nhiều token sống song song 30 phút; token cũ lộ ra vẫn đổi được mật khẩu.
    /// Đúng sau fix: token cũ → RESET_TOKEN_INVALID.
    /// </summary>
    [Fact]
    public async Task ForgotPassword_Twice_OldTokenCannotResetPassword()
    {
        var (service, logger, _) = await CreateAsync(nameof(ForgotPassword_Twice_OldTokenCannotResetPassword));
        await RegisterUserAsync(service);

        await service.ForgotPasswordAsync(new ForgotPasswordRequest { Email = Email }, CancellationToken.None);
        var oldToken = ExtractResetToken();
        await service.ForgotPasswordAsync(new ForgotPasswordRequest { Email = Email }, CancellationToken.None);
        var newToken = ExtractResetToken();
        Assert.NotEqual(oldToken, newToken);

        // BUG: token cũ vẫn còn hiệu lực → reset thành công (phải fail RESET_TOKEN_INVALID)
        var oldReset = await service.ResetPasswordAsync(
            new ResetPasswordRequest { Token = oldToken, NewPassword = "MatKhau@12345" }, CancellationToken.None);
        Assert.False(oldReset.IsSuccess,
            "Token cũ còn dùng được — bug: ForgotPasswordAsync không vô hiệu hóa token cũ (finding #10)");
        Assert.Equal(ErrorCodes.RESET_TOKEN_INVALID, oldReset.ErrorCode);

        // Control: token mới vẫn dùng được bình thường
        var newReset = await service.ResetPasswordAsync(
            new ResetPasswordRequest { Token = newToken, NewPassword = "MatKhau@12345" }, CancellationToken.None);
        Assert.True(newReset.IsSuccess, newReset.ErrorMessage);
    }

    // ── #5 (TRUNG, security): KHÔNG được log reset link/token/OTP ──

    /// <summary>
    /// Bug: AuthService.cs:613 (SMTP thiếu) + :629 (SMTP lỗi) log resetLink chứa token hợp lệ 30 phút
    /// → ai đọc log chiếm được phiên đặt lại mật khẩu. Đúng sau fix: không log link/token, chỉ {UserId}.
    /// </summary>
    [Fact]
    public async Task ForgotPassword_NoResetLinkOrTokenInLogs()
    {
        var (service, logger, _) = await CreateAsync(nameof(ForgotPassword_NoResetLinkOrTokenInLogs));
        await RegisterUserAsync(service);

        await service.ForgotPasswordAsync(new ForgotPasswordRequest { Email = Email }, CancellationToken.None);

        Assert.DoesNotContain(logger.Messages,
            m => m.Contains("reset-password?token=", StringComparison.OrdinalIgnoreCase));
        Assert.All(logger.Messages, m => Assert.False(
            Regex.IsMatch(m, @"reset-password\?token=[A-Za-z0-9_-]{20,}"),
            $"Log chứa reset token — bug security#5: {m}"));
    }

    /// <summary>
    /// Bug: AuthService.cs:640 (SMTP thiếu) + :661 (SMTP lỗi) log mã OTP 6 số → ai đọc log bật được
    /// 2FA trên tài khoản khác. Đúng sau fix: không log mã OTP, chỉ {UserId}.
    /// </summary>
    [Fact]
    public async Task Send2FaCode_NoOtpCodeInLogs()
    {
        var (service, logger, _) = await CreateAsync(nameof(Send2FaCode_NoOtpCodeInLogs));
        var userId = await RegisterUserAsync(service);

        await service.Send2FaCodeAsync(userId, CancellationToken.None);

        Assert.DoesNotContain(logger.Messages,
            m => Regex.IsMatch(m, @"mã 2FA \(dev\) cho user \d+: \d{6}"));
    }

    // ── #16 (THAP, security): OTP verify giới hạn số lần thử sai ──

    /// <summary>
    /// Bug: Verify2FaCodeAsync (AuthService.cs:500-561) không có attempt counter/lockout — brute-force
    /// 6 chữ số về lý thuyết khả thi (finding security#16). Đúng sau fix: > 5 lần sai → khóa
    /// (ACCOUNT_LOCKED/RATE_LIMITED), KHÔNG còn trả OTP_INVALID mãi mãi.
    /// </summary>
    [Fact]
    public async Task Verify2Fa_WrongCodeMoreThanFiveTimes_LocksOut()
    {
        var (service, logger, db) = await CreateAsync(nameof(Verify2Fa_WrongCodeMoreThanFiveTimes_LocksOut));
        var userId = await RegisterUserAsync(service);

        await service.Send2FaCodeAsync(userId, CancellationToken.None);
        var correct = ExtractOtpCode();
        var wrong = correct == "000000" ? "111111" : "000000";

        Result<Toggle2FaResponse>? sixth = null;
        for (var i = 0; i < 6; i++)
        {
            sixth = await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = wrong }, CancellationToken.None);
        }

        // BUG: lần thứ 6 vẫn trả OTP_INVALID — không có khóa (phải ACCOUNT_LOCKED/RATE_LIMITED)
        Assert.False(sixth!.IsSuccess);
        Assert.True(sixth.ErrorCode is ErrorCodes.ACCOUNT_LOCKED or ErrorCodes.RATE_LIMITED,
            $"Lần thử thứ 6 sai OTP phải bị khóa (ACCOUNT_LOCKED/RATE_LIMITED) — hiện {sixth.ErrorCode} (finding security#16)");
        Assert.False(await db.Users.Where(u => u.Id == userId).Select(u => u.TwoFactorEnabled).FirstAsync());
    }
}
