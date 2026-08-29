using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence.Entities;
using DsaVisual.Application.Services;
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

    /// <summary>B0 helper: xây RegisterRequest + xin otpToken (send→verify) để đăng ký qua OTP — TestServices.DevOtpCode = "123456".</summary>
    private static async Task<RegisterRequest> ValidRegisterWithOtp(AuthService service, string email = "minh@university.edu.vn")
    {
        var request = ValidRegister(email);
        request.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, email);
        return request;
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsEmailExists()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_DuplicateEmail_ReturnsEmailExists));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_DuplicateEmail_ReturnsEmailExists));

        var first = await service.RegisterAsync(await ValidRegisterWithOtp(service), null, CancellationToken.None);
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
        request.Department = "Khoa Công nghệ thông tin";
        request.StaffCode = "GV001";
        request.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, request.Email);

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("TEACHER_PENDING", result.Value!.User.Role);
    }

    [Fact]
    public async Task Register_Teacher_MissingDepartment_Succeeds_DepartmentOptional()
    {
        // A2: Khoa/Bộ môn là TÙY CHỌN — không nhập Department vẫn đăng ký GV được
        var db = TestServices.CreateInMemoryDb(nameof(Register_Teacher_MissingDepartment_Succeeds_DepartmentOptional));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Teacher_MissingDepartment_Succeeds_DepartmentOptional));

        var request = ValidRegister();
        request.IsTeacher = true;
        request.StaffCode = "GV001";
        request.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, request.Email);

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        var saved = await db.Users.SingleAsync(u => u.Email == "minh@university.edu.vn");
        Assert.Null(saved.Department);
        Assert.Equal("GV001", saved.StaffCode);
    }

    [Fact]
    public async Task Register_Teacher_MissingStaffCode_ReturnsValidationFailed()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_Teacher_MissingStaffCode_ReturnsValidationFailed));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Teacher_MissingStaffCode_ReturnsValidationFailed));

        var request = ValidRegister();
        request.IsTeacher = true;
        request.Department = "Khoa Công nghệ thông tin";

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
        Assert.NotNull(result.FieldErrors);
        Assert.True(result.FieldErrors!.ContainsKey("staffCode"));
    }

    [Fact]
    public async Task Register_Teacher_BioTooLong_ReturnsValidationFailed()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_Teacher_BioTooLong_ReturnsValidationFailed));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Teacher_BioTooLong_ReturnsValidationFailed));

        var request = ValidRegister();
        request.IsTeacher = true;
        request.Department = "Khoa Công nghệ thông tin";
        request.StaffCode = "GV001";
        request.TeacherBio = new string('a', 501);

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
        Assert.NotNull(result.FieldErrors);
        Assert.True(result.FieldErrors!.ContainsKey("teacherBio"));
    }

    [Fact]
    public async Task Register_Teacher_WhitespaceOnlyFields_ReturnsValidationFailed()
    {
        // Boundary (task L): trim trước khi validate → chuỗi toàn khoảng trắng coi như thiếu
        var db = TestServices.CreateInMemoryDb(nameof(Register_Teacher_WhitespaceOnlyFields_ReturnsValidationFailed));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Teacher_WhitespaceOnlyFields_ReturnsValidationFailed));

        var request = ValidRegister();
        request.IsTeacher = true;
        request.Department = "   ";
        request.StaffCode = "   ";
        request.TeacherBio = "   ";

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
        Assert.NotNull(result.FieldErrors);
        // A2: Department toàn khoảng trắng → coi như bỏ trống → TÙY CHỌN, không còn lỗi
        Assert.False(result.FieldErrors!.ContainsKey("department"));
        Assert.True(result.FieldErrors.ContainsKey("staffCode"));
        // Bio sau trim = rỗng → không lỗi độ dài
        Assert.False(result.FieldErrors.ContainsKey("teacherBio"));
    }

    [Fact]
    public async Task Register_Teacher_BioExactly500_IsAccepted()
    {
        // Boundary (task L): 500 ký tự hợp lệ (lỗi chỉ khi > 500)
        var db = TestServices.CreateInMemoryDb(nameof(Register_Teacher_BioExactly500_IsAccepted));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Teacher_BioExactly500_IsAccepted));

        var request = ValidRegister();
        request.IsTeacher = true;
        request.Department = "Khoa Công nghệ thông tin";
        request.StaffCode = "GV001";
        request.TeacherBio = new string('b', 500);
        request.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, request.Email);

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.Users.SingleAsync(u => u.Email == "minh@university.edu.vn");
        Assert.Equal(500, saved.TeacherBio!.Length);
    }

    [Fact]
    public async Task Register_Teacher_DepartmentTooLong_ReturnsValidationFailed()
    {
        // Review minor 1: Department > 100 → lỗi field, không ném DB truncation
        var db = TestServices.CreateInMemoryDb(nameof(Register_Teacher_DepartmentTooLong_ReturnsValidationFailed));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Teacher_DepartmentTooLong_ReturnsValidationFailed));

        var request = ValidRegister();
        request.IsTeacher = true;
        request.Department = new string('d', 101);
        request.StaffCode = "GV001";

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
        Assert.NotNull(result.FieldErrors);
        Assert.True(result.FieldErrors!.ContainsKey("department"));
        Assert.False(result.FieldErrors.ContainsKey("staffCode"));
    }

    [Fact]
    public async Task Register_Teacher_StaffCodeTooLong_ReturnsValidationFailed()
    {
        // Review minor 1: StaffCode > 50 → lỗi field, không ném DB truncation
        var db = TestServices.CreateInMemoryDb(nameof(Register_Teacher_StaffCodeTooLong_ReturnsValidationFailed));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Teacher_StaffCodeTooLong_ReturnsValidationFailed));

        var request = ValidRegister();
        request.IsTeacher = true;
        request.Department = "Khoa Công nghệ thông tin";
        request.StaffCode = new string('s', 51);

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.VALIDATION_FAILED, result.ErrorCode);
        Assert.NotNull(result.FieldErrors);
        Assert.True(result.FieldErrors!.ContainsKey("staffCode"));
        Assert.False(result.FieldErrors.ContainsKey("department"));
    }

    [Fact]
    public async Task Register_Teacher_DepartmentExactly100_IsAccepted()
    {
        // Boundary: 100 ký tự hợp lệ (lỗi chỉ khi > 100)
        var db = TestServices.CreateInMemoryDb(nameof(Register_Teacher_DepartmentExactly100_IsAccepted));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Teacher_DepartmentExactly100_IsAccepted));

        var request = ValidRegister();
        request.IsTeacher = true;
        request.Department = new string('d', 100);
        request.StaffCode = "GV001";
        request.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, request.Email);

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.Users.SingleAsync(u => u.Email == "minh@university.edu.vn");
        Assert.Equal(100, saved.Department!.Length);
    }

    [Fact]
    public async Task Register_Teacher_StaffCodeExactly50_IsAccepted()
    {
        // Boundary: 50 ký tự hợp lệ (lỗi chỉ khi > 50)
        var db = TestServices.CreateInMemoryDb(nameof(Register_Teacher_StaffCodeExactly50_IsAccepted));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Teacher_StaffCodeExactly50_IsAccepted));

        var request = ValidRegister();
        request.IsTeacher = true;
        request.Department = "Khoa Công nghệ thông tin";
        request.StaffCode = new string('s', 50);
        request.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, request.Email);

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.Users.SingleAsync(u => u.Email == "minh@university.edu.vn");
        Assert.Equal(50, saved.StaffCode!.Length);
    }

    [Fact]
    public async Task Register_Teacher_EmptyBio_StoredAsNull()
    {
        // Review minor 3: bio rỗng sau trim → lưu null, không lưu ""
        var db = TestServices.CreateInMemoryDb(nameof(Register_Teacher_EmptyBio_StoredAsNull));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Teacher_EmptyBio_StoredAsNull));

        var request = ValidRegister();
        request.IsTeacher = true;
        request.Department = "Khoa Công nghệ thông tin";
        request.StaffCode = "GV001";
        request.TeacherBio = "   ";
        request.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, request.Email);

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.Users.SingleAsync(u => u.Email == "minh@university.edu.vn");
        Assert.Null(saved.TeacherBio);
    }

    [Fact]
    public async Task Register_Student_LongTeacherFields_Ignored()
    {
        // Student không bị check độ dài field giảng viên — vẫn lưu null
        var db = TestServices.CreateInMemoryDb(nameof(Register_Student_LongTeacherFields_Ignored));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Student_LongTeacherFields_Ignored));

        var request = ValidRegister();
        request.IsTeacher = false;
        request.Department = new string('d', 101);
        request.StaffCode = new string('s', 51);
        request.TeacherBio = new string('b', 501);
        request.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, request.Email);

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.Users.SingleAsync(u => u.Email == "minh@university.edu.vn");
        Assert.Null(saved.Department);
        Assert.Null(saved.StaffCode);
        Assert.Null(saved.TeacherBio);
    }

    [Fact]
    public async Task Register_Teacher_StoresTrimmedProfileFields()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_Teacher_StoresTrimmedProfileFields));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Teacher_StoresTrimmedProfileFields));

        var request = ValidRegister();
        request.IsTeacher = true;
        request.Department = "  Khoa Công nghệ thông tin  ";
        request.StaffCode = "  GV001  ";
        request.TeacherBio = "  10 năm giảng dạy  ";
        request.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, request.Email);

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.Users.SingleAsync(u => u.Email == "minh@university.edu.vn");
        Assert.Equal("Khoa Công nghệ thông tin", saved.Department);
        Assert.Equal("GV001", saved.StaffCode);
        Assert.Equal("10 năm giảng dạy", saved.TeacherBio);
    }

    [Fact]
    public async Task Register_Student_IgnoresTeacherProfileFields()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_Student_IgnoresTeacherProfileFields));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_Student_IgnoresTeacherProfileFields));

        var request = ValidRegister();
        request.IsTeacher = false;
        request.Department = "Khoa Công nghệ thông tin";
        request.StaffCode = "GV001";
        request.TeacherBio = "Giới thiệu";
        request.OtpToken = await TestServices.IssueRegisterOtpTokenAsync(service, request.Email);

        var result = await service.RegisterAsync(request, null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        var saved = await db.Users.SingleAsync(u => u.Email == "minh@university.edu.vn");
        Assert.Null(saved.Department);
        Assert.Null(saved.StaffCode);
        Assert.Null(saved.TeacherBio);
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

        var result = await service.RegisterAsync(await ValidRegisterWithOtp(service, "student@gmail.com"), null, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotEqual(ErrorCodes.DOMAIN_NOT_ALLOWED, result.ErrorCode);
    }

    [Fact]
    public async Task Register_UniversityEduVn_WithoutDomainSetting_Succeeds()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Register_UniversityEduVn_WithoutDomainSetting_Succeeds));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Register_UniversityEduVn_WithoutDomainSetting_Succeeds));

        var result = await service.RegisterAsync(await ValidRegisterWithOtp(service, "student@university.edu.vn"), null, CancellationToken.None);

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

        var result = await service.RegisterAsync(await ValidRegisterWithOtp(service, "student@gmail.com"), null, CancellationToken.None);

        Assert.True(result.IsSuccess, result.ErrorMessage);
        Assert.NotEqual(ErrorCodes.DOMAIN_NOT_ALLOWED, result.ErrorCode);
    }

    [Fact]
    public async Task Login_Success_ReturnsTokenAndUser()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Login_Success_ReturnsTokenAndUser));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Login_Success_ReturnsTokenAndUser));

        await service.RegisterAsync(await ValidRegisterWithOtp(service), null, CancellationToken.None);
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

        await service.RegisterAsync(await ValidRegisterWithOtp(service), null, CancellationToken.None);
        var result = await service.LoginAsync(new LoginRequest { Email = "minh@university.edu.vn", Password = "SaiMatKhau@1" }, null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.INVALID_CREDENTIALS, result.ErrorCode);
    }

    [Fact]
    public async Task Login_FiveWrongAttempts_LocksAccount()
    {
        var db = TestServices.CreateInMemoryDb(nameof(Login_FiveWrongAttempts_LocksAccount));
        var service = TestServices.CreateAuthService(db, _clock, nameof(Login_FiveWrongAttempts_LocksAccount));

        await service.RegisterAsync(await ValidRegisterWithOtp(service), null, CancellationToken.None);

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

        await service.RegisterAsync(await ValidRegisterWithOtp(service), null, CancellationToken.None);
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

        await service.RegisterAsync(await ValidRegisterWithOtp(service), null, CancellationToken.None);
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
