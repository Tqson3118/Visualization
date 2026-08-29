namespace DsaVisual.Application.Dtos;

/// <summary>Thông tin người dùng tóm tắt — API_REFERENCE.md §3.3.
/// Email bị mask nửa đầu khi trả cho Teacher (`m***h@...`), xem §3.14.2.</summary>
public sealed class UserSummary
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;    // STUDENT/TEACHER/TEACHER_PENDING/ADMIN
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Xp { get; set; }                         // gamification — hiển thị ở /courses (khớp VDSA-main)
    public int Level { get; set; }                      // 1 + floor(sqrt(xp/100)) — khớp GamificationService.ComputeLevel
    public bool TwoFactorEnabled { get; set; }          // B1: trạng thái 2FA cho toggle ở ProfileView
}
