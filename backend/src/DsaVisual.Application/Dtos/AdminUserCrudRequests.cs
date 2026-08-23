namespace DsaVisual.Application.Dtos;

/// <summary>
/// Yêu cầu Admin tạo tài khoản người dùng mới (POST /api/v1/users).
/// </summary>
public sealed class AdminCreateUserRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = "STUDENT";
    public string? Department { get; set; }
    public string? StaffCode { get; set; }
}

/// <summary>
/// Yêu cầu Admin cập nhật thông tin người dùng (PUT /api/v1/users/{id}).
/// </summary>
public sealed class AdminUpdateUserRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public string? Department { get; set; }
    public string? StaffCode { get; set; }
    public string? AcademicDegree { get; set; }
    public string? ProfileLink { get; set; }
    public string? TeacherBio { get; set; }
}
