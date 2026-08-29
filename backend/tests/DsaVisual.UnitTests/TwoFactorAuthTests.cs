using System.Security.Cryptography;
using System.Text;
using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Services;
using Microsoft.Extensions.Logging;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test 2FA email (GP-T2 — FR-1.11): gửi mã OTP (hash SHA256, hết hạn 5 phút, dùng 1 lần),
/// verify đúng/sai/hết hạn/đã dùng, bật/tắt 2FA.
/// Mã OTP lấy từ RecordingOtpGenerator (test seam) — finding security#5 cấm log mã OTP trong production.
/// </summary>
public class TwoFactorAuthTests
{
    private readonly TestServices.FixedClock _clock = new();

    private TestServices.RecordingOtpGenerator _otp = null!;

    private sealed class CapturingLogger<T> : ILogger<T>
    {
        public List<string> Messages { get; } = [];

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
            => Messages.Add(formatter(state, exception));
    }

    private static string HashOtp(string code) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(code))).ToLowerInvariant();

    private async Task<int> RegisterUserAsync(AuthService service, string dbName)
    {
        // B0: đăng ký cần otpToken — xin qua send + verify (mã từ recorder _otp.Last)
        await service.SendRegisterOtpAsync(
            new SendRegisterOtpRequest { Email = "minh@university.edu.vn" }, CancellationToken.None);
        var verify = await service.VerifyRegisterOtpAsync(
            new VerifyRegisterOtpRequest { Email = "minh@university.edu.vn", Code = _otp.Last }, CancellationToken.None);
        Assert.True(verify.IsSuccess, verify.ErrorMessage);

        var result = await service.RegisterAsync(new RegisterRequest
        {
            DisplayName = "Nguyễn Minh",
            Email = "minh@university.edu.vn",
            Password = "MatKhau@123",
            IsTeacher = false,
            OtpToken = verify.Value!.OtpToken
        }, null, CancellationToken.None);
        Assert.True(result.IsSuccess);
        return result.Value!.User.Id;
    }

    private async Task<(AuthService Service, CapturingLogger<AuthService> Logger, AppDbContext Db)> CreateAsync(string testName)
    {
        var db = TestServices.CreateInMemoryDb(testName);
        var logger = new CapturingLogger<AuthService>();
        _otp = new TestServices.RecordingOtpGenerator();
        var service = TestServices.CreateAuthService(db, _clock, testName, logger, otpGenerator: _otp.Generate);
        return (service, logger, db);
    }

    private string ExtractOtpCode() => _otp.Last;

    [Fact]
    public async Task Send2FaCode_StoresHashedCodeAndReturnsExpiry()
    {
        var (service, _, db) = await CreateAsync(nameof(Send2FaCode_StoresHashedCodeAndReturnsExpiry));
        var userId = await RegisterUserAsync(service, nameof(Send2FaCode_StoresHashedCodeAndReturnsExpiry));

        var result = await service.Send2FaCodeAsync(userId, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(300, result.Value!.ExpiresInSeconds);

        var otp = db.OtpCodes.Single();
        Assert.Equal(userId, otp.UserId);
        Assert.Equal("enable_2fa", otp.Purpose);
        Assert.Equal(64, otp.CodeHash.Length);
        Assert.Equal(_clock.UtcNow.AddMinutes(5), otp.ExpiresAt);
        Assert.False(otp.Used);
        Assert.False(db.Users.Single(u => u.Id == userId).TwoFactorEnabled);
    }

    [Fact]
    public async Task Send2FaCode_WhenAlreadyEnabled_ReturnsAlreadyEnabled()
    {
        var (service, logger, _) = await CreateAsync(nameof(Send2FaCode_WhenAlreadyEnabled_ReturnsAlreadyEnabled));
        var userId = await RegisterUserAsync(service, nameof(Send2FaCode_WhenAlreadyEnabled_ReturnsAlreadyEnabled));

        await service.Send2FaCodeAsync(userId, CancellationToken.None);
        var code = ExtractOtpCode();
        var verified = await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = code }, CancellationToken.None);
        Assert.True(verified.IsSuccess);

        var result = await service.Send2FaCodeAsync(userId, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.TWO_FA_ALREADY_ENABLED, result.ErrorCode);
    }

    [Fact]
    public async Task Send2FaCode_Twice_OnlyLatestCodeWorks()
    {
        var (service, logger, _) = await CreateAsync(nameof(Send2FaCode_Twice_OnlyLatestCodeWorks));
        var userId = await RegisterUserAsync(service, nameof(Send2FaCode_Twice_OnlyLatestCodeWorks));

        await service.Send2FaCodeAsync(userId, CancellationToken.None);
        var firstCode = ExtractOtpCode();
        await service.Send2FaCodeAsync(userId, CancellationToken.None);
        var secondCode = ExtractOtpCode();
        Assert.NotEqual(firstCode, secondCode);

        // Mã cũ đã bị vô hiệu hóa (Used) khi gửi mã mới → không dùng lại được
        var old = await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = firstCode }, CancellationToken.None);
        Assert.False(old.IsSuccess);
        Assert.Equal(ErrorCodes.OTP_USED, old.ErrorCode);

        var latest = await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = secondCode }, CancellationToken.None);
        Assert.True(latest.IsSuccess);
    }

    [Fact]
    public async Task Verify2Fa_CorrectCode_EnablesTwoFactorAndMarksUsed()
    {
        var (service, logger, db) = await CreateAsync(nameof(Verify2Fa_CorrectCode_EnablesTwoFactorAndMarksUsed));
        var userId = await RegisterUserAsync(service, nameof(Verify2Fa_CorrectCode_EnablesTwoFactorAndMarksUsed));

        await service.Send2FaCodeAsync(userId, CancellationToken.None);
        var code = ExtractOtpCode();

        var result = await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = code }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value!.Enabled);
        Assert.True(db.Users.Single(u => u.Id == userId).TwoFactorEnabled);
        Assert.True(db.OtpCodes.Single().Used);
        // DB chỉ lưu hash, không lưu mã gốc
        Assert.Equal(HashOtp(code), db.OtpCodes.Single().CodeHash);
        Assert.NotEqual(code, db.OtpCodes.Single().CodeHash);
    }

    [Fact]
    public async Task Verify2Fa_WrongCode_ReturnsOtpInvalid()
    {
        var (service, logger, db) = await CreateAsync(nameof(Verify2Fa_WrongCode_ReturnsOtpInvalid));
        var userId = await RegisterUserAsync(service, nameof(Verify2Fa_WrongCode_ReturnsOtpInvalid));

        await service.Send2FaCodeAsync(userId, CancellationToken.None);
        _ = ExtractOtpCode();

        var result = await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = "000000" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.OTP_INVALID, result.ErrorCode);
        Assert.False(db.Users.Single(u => u.Id == userId).TwoFactorEnabled);
    }

    [Fact]
    public async Task Verify2Fa_ExpiredCode_ReturnsOtpExpired()
    {
        var (service, logger, db) = await CreateAsync(nameof(Verify2Fa_ExpiredCode_ReturnsOtpExpired));
        var userId = await RegisterUserAsync(service, nameof(Verify2Fa_ExpiredCode_ReturnsOtpExpired));

        await service.Send2FaCodeAsync(userId, CancellationToken.None);
        var code = ExtractOtpCode();

        _clock.UtcNow = _clock.UtcNow.AddMinutes(6);   // quá 5 phút hiệu lực

        var result = await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = code }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.OTP_EXPIRED, result.ErrorCode);
        Assert.False(db.Users.Single(u => u.Id == userId).TwoFactorEnabled);
    }

    [Fact]
    public async Task Verify2Fa_UsedCode_ReturnsOtpUsed()
    {
        var (service, logger, db) = await CreateAsync(nameof(Verify2Fa_UsedCode_ReturnsOtpUsed));
        var userId = await RegisterUserAsync(service, nameof(Verify2Fa_UsedCode_ReturnsOtpUsed));

        await service.Send2FaCodeAsync(userId, CancellationToken.None);
        var code = ExtractOtpCode();

        var first = await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = code }, CancellationToken.None);
        Assert.True(first.IsSuccess);

        // Tắt rồi thử lại với mã đã dùng → OTP_USED (không phải TWO_FA_ALREADY_ENABLED)
        var disable = await service.Toggle2FaAsync(userId, new Toggle2FaRequest { Enabled = false }, CancellationToken.None);
        Assert.True(disable.IsSuccess);

        var second = await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = code }, CancellationToken.None);

        Assert.False(second.IsSuccess);
        Assert.Equal(ErrorCodes.OTP_USED, second.ErrorCode);
        Assert.False(db.Users.Single(u => u.Id == userId).TwoFactorEnabled);
    }

    [Fact]
    public async Task Verify2Fa_NonDigitCode_ReturnsValidationFailed()
    {
        var (service, _, _) = await CreateAsync(nameof(Verify2Fa_NonDigitCode_ReturnsValidationFailed));
        var userId = await RegisterUserAsync(service, nameof(Verify2Fa_NonDigitCode_ReturnsValidationFailed));

        var result = await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = "12ab56" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
    }

    [Fact]
    public async Task Toggle2Fa_Disable_DisablesTwoFactor()
    {
        var (service, logger, db) = await CreateAsync(nameof(Toggle2Fa_Disable_DisablesTwoFactor));
        var userId = await RegisterUserAsync(service, nameof(Toggle2Fa_Disable_DisablesTwoFactor));

        await service.Send2FaCodeAsync(userId, CancellationToken.None);
        var code = ExtractOtpCode();
        await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = code }, CancellationToken.None);
        Assert.True(db.Users.Single(u => u.Id == userId).TwoFactorEnabled);

        var result = await service.Toggle2FaAsync(userId, new Toggle2FaRequest { Enabled = false }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.Enabled);
        Assert.False(db.Users.Single(u => u.Id == userId).TwoFactorEnabled);
    }

    [Fact]
    public async Task Toggle2Fa_EnableWithoutVerify_ReturnsOtpRequired()
    {
        var (service, _, _) = await CreateAsync(nameof(Toggle2Fa_EnableWithoutVerify_ReturnsOtpRequired));
        var userId = await RegisterUserAsync(service, nameof(Toggle2Fa_EnableWithoutVerify_ReturnsOtpRequired));

        var result = await service.Toggle2FaAsync(userId, new Toggle2FaRequest { Enabled = true }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.OTP_REQUIRED, result.ErrorCode);
    }

    [Fact]
    public async Task Toggle2Fa_DisableWhenAlreadyDisabled_ReturnsSuccessIdempotent()
    {
        var (service, _, _) = await CreateAsync(nameof(Toggle2Fa_DisableWhenAlreadyDisabled_ReturnsSuccessIdempotent));
        var userId = await RegisterUserAsync(service, nameof(Toggle2Fa_DisableWhenAlreadyDisabled_ReturnsSuccessIdempotent));

        var result = await service.Toggle2FaAsync(userId, new Toggle2FaRequest { Enabled = false }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value!.Enabled);
    }

    [Fact]
    public async Task Toggle2Fa_EnableWhenAlreadyEnabled_ReturnsAlreadyEnabled()
    {
        var (service, logger, _) = await CreateAsync(nameof(Toggle2Fa_EnableWhenAlreadyEnabled_ReturnsAlreadyEnabled));
        var userId = await RegisterUserAsync(service, nameof(Toggle2Fa_EnableWhenAlreadyEnabled_ReturnsAlreadyEnabled));

        await service.Send2FaCodeAsync(userId, CancellationToken.None);
        var code = ExtractOtpCode();
        await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = code }, CancellationToken.None);

        var result = await service.Toggle2FaAsync(userId, new Toggle2FaRequest { Enabled = true }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.TWO_FA_ALREADY_ENABLED, result.ErrorCode);
    }

    [Fact]
    public async Task Login_WhenTwoFactorEnabled_RequiresTwoFactorAndSendsOtp()
    {
        var (service, _, db) = await CreateAsync(nameof(Login_WhenTwoFactorEnabled_RequiresTwoFactorAndSendsOtp));
        var userId = await RegisterUserAsync(service, nameof(Login_WhenTwoFactorEnabled_RequiresTwoFactorAndSendsOtp));

        // Enable 2FA
        await service.Send2FaCodeAsync(userId, CancellationToken.None);
        var enableCode = ExtractOtpCode();
        await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = enableCode }, CancellationToken.None);
        Assert.True(db.Users.Single(u => u.Id == userId).TwoFactorEnabled);

        // Login
        var loginResult = await service.LoginAsync(new LoginRequest
        {
            Email = "minh@university.edu.vn",
            Password = "MatKhau@123"
        }, "127.0.0.1", CancellationToken.None);

        Assert.True(loginResult.IsSuccess);
        Assert.True(loginResult.Value!.RequiresTwoFactor);
        Assert.False(string.IsNullOrEmpty(loginResult.Value.TwoFactorToken));
        Assert.Equal(string.Empty, loginResult.Value.AccessToken); // Token not issued yet

        var loginOtp = ExtractOtpCode();
        Assert.NotNull(loginOtp);
        Assert.Equal(6, loginOtp.Length);

        // Verify 2FA login with correct code
        var verifyResult = await service.VerifyLogin2FaAsync(new Login2FaRequest
        {
            TwoFactorToken = loginResult.Value.TwoFactorToken!,
            Code = loginOtp
        }, "127.0.0.1", CancellationToken.None);

        Assert.True(verifyResult.IsSuccess);
        Assert.False(string.IsNullOrEmpty(verifyResult.Value!.AccessToken));
        Assert.Equal(userId, verifyResult.Value.User.Id);
    }

    [Fact]
    public async Task VerifyLogin2Fa_WrongCode_Fails()
    {
        var (service, _, _) = await CreateAsync(nameof(VerifyLogin2Fa_WrongCode_Fails));
        var userId = await RegisterUserAsync(service, nameof(VerifyLogin2Fa_WrongCode_Fails));

        await service.Send2FaCodeAsync(userId, CancellationToken.None);
        var enableCode = ExtractOtpCode();
        await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = enableCode }, CancellationToken.None);

        var loginResult = await service.LoginAsync(new LoginRequest
        {
            Email = "minh@university.edu.vn",
            Password = "MatKhau@123"
        }, "127.0.0.1", CancellationToken.None);

        var verifyResult = await service.VerifyLogin2FaAsync(new Login2FaRequest
        {
            TwoFactorToken = loginResult.Value!.TwoFactorToken!,
            Code = "999999"
        }, "127.0.0.1", CancellationToken.None);

        Assert.False(verifyResult.IsSuccess);
        Assert.Equal(ErrorCodes.OTP_INVALID, verifyResult.ErrorCode);
    }

    [Fact]
    public async Task ResendLogin2Fa_GeneratesNewOtp()
    {
        var (service, _, _) = await CreateAsync(nameof(ResendLogin2Fa_GeneratesNewOtp));
        var userId = await RegisterUserAsync(service, nameof(ResendLogin2Fa_GeneratesNewOtp));

        await service.Send2FaCodeAsync(userId, CancellationToken.None);
        var enableCode = ExtractOtpCode();
        await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = enableCode }, CancellationToken.None);

        var loginResult = await service.LoginAsync(new LoginRequest
        {
            Email = "minh@university.edu.vn",
            Password = "MatKhau@123"
        }, "127.0.0.1", CancellationToken.None);

        var firstOtp = ExtractOtpCode();

        var resend = await service.ResendLogin2FaAsync(new ResendLogin2FaRequest
        {
            TwoFactorToken = loginResult.Value!.TwoFactorToken!
        }, CancellationToken.None);

        Assert.True(resend.IsSuccess);
        var secondOtp = ExtractOtpCode();
        Assert.NotEqual(firstOtp, secondOtp);

        // Old code is invalid
        var oldVerify = await service.VerifyLogin2FaAsync(new Login2FaRequest
        {
            TwoFactorToken = loginResult.Value!.TwoFactorToken!,
            Code = firstOtp
        }, "127.0.0.1", CancellationToken.None);
        Assert.False(oldVerify.IsSuccess);

        // New code works
        var newVerify = await service.VerifyLogin2FaAsync(new Login2FaRequest
        {
            TwoFactorToken = loginResult.Value!.TwoFactorToken!,
            Code = secondOtp
        }, "127.0.0.1", CancellationToken.None);
        Assert.True(newVerify.IsSuccess);
    }

    [Fact]
    public async Task ResendLogin2Fa_RateLimited_After5Attempts()
    {
        var (service, _, _) = await CreateAsync(nameof(ResendLogin2Fa_RateLimited_After5Attempts));
        var userId = await RegisterUserAsync(service, nameof(ResendLogin2Fa_RateLimited_After5Attempts));

        await service.Send2FaCodeAsync(userId, CancellationToken.None);
        var enableCode = ExtractOtpCode();
        await service.Verify2FaCodeAsync(userId, new Verify2FaRequest { Code = enableCode }, CancellationToken.None);

        var loginResult = await service.LoginAsync(new LoginRequest
        {
            Email = "minh@university.edu.vn",
            Password = "MatKhau@123"
        }, "127.0.0.1", CancellationToken.None);

        for (int i = 0; i < 5; i++)
        {
            var res = await service.ResendLogin2FaAsync(new ResendLogin2FaRequest
            {
                TwoFactorToken = loginResult.Value!.TwoFactorToken!
            }, CancellationToken.None);
            Assert.True(res.IsSuccess);
        }

        // 6th attempt is locked
        var sixth = await service.ResendLogin2FaAsync(new ResendLogin2FaRequest
        {
            TwoFactorToken = loginResult.Value!.TwoFactorToken!
        }, CancellationToken.None);
        Assert.False(sixth.IsSuccess);
        Assert.Equal(ErrorCodes.ACCOUNT_LOCKED, sixth.ErrorCode);
    }
}
