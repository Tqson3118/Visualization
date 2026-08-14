namespace DsaVisual.Application.Dtos;

/// <summary>Người dùng trong quản trị — GET /users (API_REFERENCE.md §4.8, kèm isActive).</summary>
public sealed class AdminUserDto
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Department { get; set; }
    public string? StaffCode { get; set; }
    public string? TeacherBio { get; set; }
    public string? AcademicDegree { get; set; }
    public string? ProfileLink { get; set; }
    public DateTime CreatedAt { get; set; }

    // ── Thống kê học tập (v2.15 — drawer chi tiết user) ──
    public int Xp { get; set; }
    public int Level { get; set; }
    public int StreakDays { get; set; }
    public int Gems { get; set; }
    public int Hearts { get; set; }
    public int LessonsCompletedCount { get; set; }
    public int ExercisesPassedCount { get; set; }
    public int JoinedClassesCount { get; set; }
}
