using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.UnitTests;
using Microsoft.EntityFrameworkCore;

namespace DsaVisual.UnitTests;

/// <summary>
/// Test AuthService: đăng ký (email trùng, weak password, teacher pending, domain), đăng nhập
/// (sai mật khẩu 5 lần → khóa), refresh rotate-invalidate + replay.
/// </summary>
public class AuthServiceTests
{
    private readonly TestServices.FixedClock _clock = new();

    private static RegisterRequest ValidRegister(string email = "minh@university.edu.vn") => new()
    {
        DisplayName = "Nguyễn Minh",
        Email = email,
        Password = "MatKhau@123",
        IsTeacher = false
    };

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsEmailExists()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_DuplicateEmail_ReturnsEmailExists));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_DuplicateEmail_ReturnsEmailExists));

        var first = await service.RegisterAsync(ValidRegister(), null, CancellationToken.None);
        var second = await service.RegisterAsync(ValidRegister(), null, CancellationToken.None);

        Assert.True(first.IsSuccess);
        Assert.False(second.IsSuccess);
        Assert.Equal(ErrorCodes.EMAIL_EXISTS, second.ErrorCode);
    }

    [Fact]
    public async Task Register_WeakPassword_ReturnsWeakPassword()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_WeakPassword_ReturnsWeakPassword));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_WeakPassword_ReturnsWeakPassword));

        var request = ValidRegister();
        request.Password = "matkhau";   // không có chữ hoa/số/ký tự đặc biệt

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.WEAK_PASSWORD, result.ErrorCode);
        Assert.NotNull(result.FieldErrors);
        Assert.True(result.FieldErrors!.ContainsKey("password"));
    }

    [Fact]
    public async Task Register_Teacher_HasTeacherPendingRole()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_Teacher_HasTeacherPendingRole));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Teacher_HasTeacherPendingRole));

        var request = ValidRegister();
        request.IsTeacher = true;

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("TEACHER_PENDING", result.Value!.User.Role);
    }

    [Fact]
    public async Task Register_DisallowedDomain_ReturnsDomainNotAllowed()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_DisallowedDomain_ReturnsDomainNotAllowed));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_DisallowedDomain_ReturnsDomainNotAllowed));

        // Bật setting allowed.email.domains = university.edu.vn
        db.Settings.Add(new Setting
        {
            Key = "allowed.email.domains",
            Value = "university.edu.vn",
            UpdatedAt = _clock.UtcNow,
            UpdatedBy = 1
        });
        await db.SaveChangesAsync();

        var result = await service.RegisterAsync(ValidRegister("nguoi.khac@gmail.com"), null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.DOMAIN_NOT_ALLOWED, result.ErrorCode);
    }

    // ── SEED-5: sau khi seed (xóa setting allowed.email.domains) mọi email đăng ký được ──

    [Fact]
    public async Task Register_Gmail_WithoutDomainSetting_Succeeds()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_Gmail_WithoutDomainSetting_Succeeds));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Gmail_WithoutDomainSetting_Succeeds));

        var result = await service.RegisterAsync(ValidRegister("student@gmail.com"), null, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotEqual(ErrorCodes.DOMAIN_NOT_ALLOWED, result.ErrorCode);
    }

    [Fact]
    public async Task Register_UniversityEduVn_WithoutDomainSetting_Succeeds()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_UniversityEduVn_WithoutDomainSetting_Succeeds));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_UniversityEduVn_WithoutDomainSetting_Succeeds));

        var result = await service.RegisterAsync(ValidRegister("student@university.edu.vn"), null, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
    }

    [Fact]
    public async Task Register_Gmail_WithEmptyDomainSetting_Succeeds()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_Gmail_WithEmptyDomainSetting_Succeeds));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Gmail_WithEmptyDomainSetting_Succeeds));

        // Setting tồn tại nhưng Value rỗng → AuthService bỏ qua check (IsNullOrWhiteSpace)
        db.Settings.Add(new Setting
        {
            Key = "allowed.email.domains",
            Value = "",
            UpdatedAt = _clock.UtcNow,
            UpdatedBy = 1
        });
        await db.SaveChangesAsync();

        var result = await service.RegisterAsync(ValidRegister("student@gmail.com"), null, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotEqual(ErrorCodes.DOMAIN_NOT_ALLOWED, result.ErrorCode);
    }

    [Fact]
    public async Task Login_Success_ReturnsTokenAndUser()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Login_Success_ReturnsTokenAndUser));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Login_Success_ReturnsTokenAndUser));

        await service.RegisterAsync(ValidRegister(), null, CancellationToken.None);
        var result = await service.LoginAsync(new LoginRequest { Email = "minh@university.edu.vn", Password = "MatKhau@123" }, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(string.IsNullOrEmpty(result.Value!.AccessToken));
        Assert.False(string.IsNullOrEmpty(result.Value.RefreshToken));
        Assert.Equal("minh@university.edu.vn", result.Value.User.Email);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsInvalidCredentials()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Login_WrongPassword_ReturnsInvalidCredentials));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Login_WrongPassword_ReturnsInvalidCredentials));

        await service.RegisterAsync(ValidRegister(), null, CancellationToken.None);
        var result = await service.LoginAsync(new LoginRequest { Email = "minh@university.edu.vn", Password = "SaiMatKhau@1" }, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.INVALID_CREDENTIALS, result.ErrorCode);
    }

    [Fact]
    public async Task Login_FiveWrongAttempts_LocksAccount()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Login_FiveWrongAttempts_LocksAccount));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Login_FiveWrongAttempts_LocksAccount));

        await service.RegisterAsync(ValidRegister(), null, CancellationToken.None);

        var last = new Result<RefreshResponse>();
        for (var i = 0; i < 5; i++)
        {
            last = await service.LoginAsync(new LoginRequest { Email = "minh@university.edu.vn", Password = "SaiMatKhau@1" }, null, CancellationToken.None);
        }

        Assert.False(last.IsSuccess);
        Assert.Equal(ErrorCodes.ACCOUNT_LOCKED, last.ErrorCode);
    }

    [Fact]
    public async Task Refresh_RotatesAndInvalidatesOldToken()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Refresh_RotatesAndInvalidatesOldToken));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Refresh_RotatesAndInvalidatesOldToken));

        await service.RegisterAsync(ValidRegister(), null, CancellationToken.None);
        var login = await service.LoginAsync(
            new LoginRequest { Email = "minh@university.edu.vn", Password = "MatKhau@123" }, null, CancellationToken.None);
        Assert.True(login.IsSuccess);

        var refreshed = await service.RefreshAsync(login.Value!.RefreshToken, null, CancellationToken.None);
        Assert.True(refreshed.IsSuccess);
        Assert.NotNull(refreshed.Value!.RefreshToken);

        // Token cũ đã bị rotate → REFRESH_INVALID
        var replay = await service.RefreshAsync(login.Value.RefreshToken, null, CancellationToken.None);
        Assert.False(replay.IsSuccess);
        Assert.Equal(ErrorCodes.REFRESH_INVALID, replay.ErrorCode);
    }

    [Fact]
    public async Task Refresh_ReplayToken_RevokesWholeChain()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Refresh_ReplayToken_RevokesWholeChain));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Refresh_ReplayToken_RevokesWholeChain));

        await service.RegisterAsync(ValidRegister(), null, CancellationToken.None);
        var login = await service.LoginAsync(
            new LoginRequest { Email = "minh@university.edu.vn", Password = "MatKhau@123" }, null, CancellationToken.None);

        // Vòng 1: refresh token1 → token2
        var first = await service.RefreshAsync(login.Value!.RefreshToken, null, CancellationToken.None);
        Assert.True(first.IsSuccess);

        // Replay token1 (đã rotate) → thu hồi cả chuỗi
        var replay = await service.RefreshAsync(login.Value.RefreshToken, null, CancellationToken.None);
        Assert.False(replay.IsSuccess);
        Assert.Equal(ErrorCodes.REFRESH_INVALID, replay.ErrorCode);

        // Token2 giờ cũng bị thu hồi
        var after = await service.RefreshAsync(first.Value!.RefreshToken, null, CancellationToken.None);
        Assert.False(after.IsSuccess);
        Assert.Equal(ErrorCodes.REFRESH_INVALID, after.ErrorCode);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsInvalidCredentials()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Login_UnknownEmail_ReturnsInvalidCredentials));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Login_UnknownEmail_ReturnsInvalidCredentials));

        var result = await service.LoginAsync(
            new LoginRequest { Email = "khongton tai@university.edu.vn", Password = "MatKhau@123" }, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.INVALID_CREDENTIALS, result.ErrorCode);
    }
}
