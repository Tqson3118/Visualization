namespace DsaVisual.Application.Dtos;

/// <summary>Đăng ký tài khoản — API_REFERENCE.md §3.1.</summary>
public sealed class RegisterRequest
{
    public string DisplayName { get; set; } = string.Empty;   // 2-100
    public string Email { get; set; } = string.Empty;         // ≤ 256, lowercase
    public string Password { get; set; } = string.Empty;      // 8-64, chữ hoa + số + ký tự đặc biệt
    public bool IsTeacher { get; set; }                       // mặc định false
    public string? Department { get; set; }                   // optional khi IsTeacher=true (A2 — Khoa/Bộ môn)
    public string? StaffCode { get; set; }                    // bắt buộc + duy nhất khi IsTeacher=true (A3 — Mã giảng viên)
    public string? TeacherBio { get; set; }                   // ≤ 500 ký tự (Kinh nghiệm/giới thiệu)
    public string? AcademicDegree { get; set; }               // v2.15: Học vị (Thạc sĩ, Tiến sĩ...)
    public string? ProfileLink { get; set; }                  // v2.15: Link hồ sơ nghiên cứu / LinkedIn
    public string? OtpToken { get; set; }                     // B0: token nhận từ POST /auth/register/otp/verify — bắt buộc
}
