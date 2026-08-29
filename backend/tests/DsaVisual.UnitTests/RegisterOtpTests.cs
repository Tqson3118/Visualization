using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
using DsaVisual.UnitTests;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.UnitTests;

/// <summary>
/// B0 — OTP xác thực email khi đăng ký: send → verify (cấp otpToken) → register tiêu token.
/// Mã OTP determinstic qua DevOtpCode "123456" (TestServices.CreateConfig).
/// </summary>
public class RegisterOtpTests
{
    private readonly TestServices.FixedClock _clock = new();

    private const string Email = "minh@university.edu.vn";
    private const string DevCode = "123456";

    private static RegisterRequest ValidRegister(string email = Email) => new()
    {
        DisplayName = "Nguyễn Minh",
        Email = email,
        Password = "MatKhau@123",
        IsTeacher = false
    };

    [Fact]
    public async Task SendRegisterOtp_CreatesHashedCodeRow()
    {
        var db = TestServices.CreateInMemoryDb(nameof(SendRegisterOtp_CreatesHashedCodeRow));
        var service = TestServices.CreateAuthService(db, _clock, nameof(SendRegisterOtp_CreatesHashedCodeRow));

        var result = await service.SendRegisterOtpAsync(new SendRegisterOtpRequest { Email = Email }, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal(300, result.Value!.ExpiresInSeconds);
        var row = await db.RegisterOtpCodes.SingleAsync();
        Assert.NotEqual(DevCode, row.CodeHash);                       // KHÔNG lưu mã gốc
        Assert.Equal(64, row.CodeHash.Length);                          // SHA256 hex
        Assert.Equal(Email, row.Email);
        Assert.Equal(_clock.UtcNow.AddMinutes(5), row.ExpiresAt);
    }

    [Fact]
    public async Task SendRegisterOtp_EmailAlreadyRegistered_ReturnsEmailExists()
    {
        var db = TestServices.CreateInMemoryDb(nameof(SendRegisterOtp_EmailAlreadyRegistered_ReturnsEmailExists));
        var service = TestServices.CreateAuthService(db, _clock, nameof(SendRegisterOtp_EmailAlreadyRegistered_ReturnsEmailExists));
        var request = ValidRegister();
        request.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, Email);
        Assert.True((await service.RegisterAsync(request, null, CancellationToken.None)).IsSuccess);

        var result = await service.SendRegisterOtpAsync(new SendRegisterOtpRequest { Email = Email }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.EMAIL_EXISTS, result.ErrorCode);
    }

    [Fact]
    public async Task VerifyRegisterOtp_WrongCode_ReturnsOtpInvalid()
    {
        var db = TestServices.CreateInMemoryDb(nameof(VerifyRegisterOtp_WrongCode_ReturnsOtpInvalid));
        var service = TestServices.CreateAuthService(db, _clock, nameof(VerifyRegisterOtp_WrongCode_ReturnsOtpInvalid));
        await service.SendRegisterOtpAsync(new SendRegisterOtpRequest { Email = Email }, CancellationToken.None);

        var result = await service.VerifyRegisterOtpAsync(
            new VerifyRegisterOtpRequest { Email = Email, Code = "999999" }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.OTP_INVALID, result.ErrorCode);
    }

    [Fact]
    public async Task VerifyRegisterOtp_ExpiredCode_ReturnsOtpExpired()
    {
        var db = TestServices.CreateInMemoryDb(nameof(VerifyRegisterOtp_ExpiredCode_ReturnsOtpExpired));
        var service = TestServices.CreateAuthService(db, _clock, nameof(VerifyRegisterOtp_ExpiredCode_ReturnsOtpExpired));
        await service.SendRegisterOtpAsync(new SendRegisterOtpRequest { Email = Email }, CancellationToken.None);

        _clock.UtcNow = _clock.UtcNow.AddMinutes(6);   // mã hết hạn sau 5 phút
        var result = await service.VerifyRegisterOtpAsync(
            new VerifyRegisterOtpRequest { Email = Email, Code = DevCode }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.OTP_EXPIRED, result.ErrorCode);
    }

    [Fact]
    public async Task Register_WithoutOtpToken_ReturnsOtpRequired()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_WithoutOtpToken_ReturnsOtpRequired));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_WithoutOtpToken_ReturnsOtpRequired));

        var result = await service.RegisterAsync(ValidRegister(), null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.OTP_REQUIRED, result.ErrorCode);
        Assert.NotNull(result.FieldErrors);
        Assert.True(result.FieldErrors!.ContainsKey("otpToken"));
    }

    [Fact]
    public async Task Register_WithValidOtpToken_Succeeds()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_WithValidOtpToken_Succeeds));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_WithValidOtpToken_Succeeds));

        var request = ValidRegister();
        request.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, Email);

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.Equal("STUDENT", result.Value!.User.Role);
        // Token đã tiêu: dòng OTP đánh dấu TokenUsed
        Assert.True(await db.RegisterOtpCodes.AnyAsync(r => r.TokenUsed));
    }

    [Fact]
    public async Task Register_TokenExpired_ReturnsOtpInvalid()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_TokenExpired_ReturnsOtpInvalid));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_TokenExpired_ReturnsOtpInvalid));

        var request = ValidRegister();
        request.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, Email);

        _clock.UtcNow = _clock.UtcNow.AddMinutes(11);   // otpToken hết hạn sau 10 phút
        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.OTP_INVALID, result.ErrorCode);
    }

    [Fact]
    public async Task Register_TokenReuse_AfterUserSoftDeleted_ReturnsOtpInvalid()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_TokenReuse_AfterUserSoftDeleted_ReturnsOtpInvalid));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_TokenReuse_AfterUserSoftDeleted_ReturnsOtpInvalid));

        var request = ValidRegister();
        request.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, Email);
        Assert.True((await service.RegisterAsync(request, null, CancellationToken.None)).IsSuccess);

        // Soft-delete user → email free lại, nhưng token đã tiêu (TokenUsed) → không đăng ký lại được
        var user = await db.Users.SingleAsync();
        user.DeletedAt = _clock.UtcNow;
        await db.SaveChangesAsync();

        // Đăng ký lại CÙNG token đã tiêu → email free nhưng token đã Used → OTP_INVALID
        var retry = ValidRegister();
        retry.OtpToken = request.OtpToken;
        var result = await service.RegisterAsync(retry, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.OTP_INVALID, result.ErrorCode);
    }

    [Fact]
    public async Task SendRegisterOtp_ResendInvalidatesPreviousCodeAndToken()
    {
        var db = TestServices.CreateInMemoryDb(nameof(SendRegisterOtp_ResendInvalidatesPreviousCodeAndToken));
        var service = TestServices.CreateAuthService(db, _clock, nameof(SendRegisterOtp_ResendInvalidatesPreviousCodeAndToken));

        var firstToken = await TestServices.IssueRegisterOtpTokenAsync(service, Email);

        // Gửi lại mã mới → mã cũ + token cũ bị vô hiệu
        var resend = await service.SendRegisterOtpAsync(new SendRegisterOtpRequest { Email = Email }, CancellationToken.None);
        Assert.True(resend.IsSuccess, resend.ErrorMessage);
        Assert.Equal(2, await db.RegisterOtpCodes.CountAsync(r => r.Email == Email));
        Assert.False(await db.RegisterOtpCodes.AnyAsync(r => r.VerifyTokenHash != null && !r.Used));

        var request = ValidRegister();
        request.OtpToken = firstToken;
        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.OTP_INVALID, result.ErrorCode);
    }

    [Fact]
    public async Task VerifyRegisterOtp_FiveWrongAttempts_InvalidatesCode()
    {
        var db = TestServices.CreateInMemoryDb(nameof(VerifyRegisterOtp_FiveWrongAttempts_InvalidatesCode));
        var service = TestServices.CreateAuthService(db, _clock, nameof(VerifyRegisterOtp_FiveWrongAttempts_InvalidatesCode));
        await service.SendRegisterOtpAsync(new SendRegisterOtpRequest { Email = Email }, CancellationToken.None);

        for (var i = 0; i < 5; i++)
        {
            var wrong = await service.VerifyRegisterOtpAsync(
                new VerifyRegisterOtpRequest { Email = Email, Code = "999999" }, CancellationToken.None);
            Assert.False(wrong.IsSuccess);
        }

        // Lần thứ 6 (kể cả mã ĐÚNG) → khóa tạm 15 phút do sai 5 lần (LoginAttemptTracker)
        var result = await service.VerifyRegisterOtpAsync(
            new VerifyRegisterOtpRequest { Email = Email, Code = DevCode }, CancellationToken.None);
        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ACCOUNT_LOCKED, result.ErrorCode);
    }

    [Fact]
    public async Task Register_TeacherDuplicateStaffCode_ReturnsConflict()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_TeacherDuplicateStaffCode_ReturnsConflict));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_TeacherDuplicateStaffCode_ReturnsConflict));

        var first = ValidRegister(Email);
        first.IsTeacher = true;
        first.StaffCode = "GV777";
        first.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, Email);
        Assert.True((await service.RegisterAsync(first, null, CancellationToken.None)).IsSuccess);

        var second = ValidRegister("nguoi.khac@university.edu.vn");
        second.IsTeacher = true;
        second.StaffCode = "GV777";
        second.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, second.Email);

        var result = await service.RegisterAsync(second, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.CONFLICT, result.ErrorCode);
        Assert.NotNull(result.FieldErrors);
        Assert.True(result.FieldErrors!.ContainsKey("staffCode"));
    }
}
