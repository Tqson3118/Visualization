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
}
