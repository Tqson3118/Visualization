namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Người dùng — SDD §7.3.1 (+ cột gamification).</summary>
public sealed class User
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;                 // UNIQUE, lowercase
    public string PasswordHash { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Student;
    public bool IsActive { get; set; } = true;
    public bool IsPrimaryAdmin { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public string? AvatarUrl { get; set; }
    public int Hearts { get; set; } = 10;
    public int HeartsMax { get; set; } = 10;
    public DateTime LastHeartAt { get; set; }
    public int Gems { get; set; }
    public int Xp { get; set; }
    public int StreakDays { get; set; }
    public int StreakFreeze { get; set; }
    public DateTime? PremiumUntil { get; set; }
    public DateTime? LastActivityDate { get; set; }
    public DateTime? StreakLastProcessed { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }                         // xóa mềm (D-5)
    public string? Department { get; set; }                          // Khoa/Bộ môn (form đăng ký GV)
    public string? StaffCode { get; set; }                           // Mã giảng viên (form đăng ký GV)
    public string? TeacherBio { get; set; }                          // Kinh nghiệm/giới thiệu (form đăng ký GV)
}
