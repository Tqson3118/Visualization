namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Lớp học — SDD §7.3.16 (Module H). InviteCode UNIQUE (mã mời 6 ký tự).</summary>
public sealed class Class
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;           // UNIQUE
    public string? Semester { get; set; }
    public string? Description { get; set; }
    public int OwnerId { get; set; }                                 // FK → Users.Id (Teacher)
    public ClassStatus Status { get; set; } = ClassStatus.Open;

    // ── Learning Path / Curriculum (per-class) ──
    /// <summary>Tiêu đề lộ trình học được gán cho lớp (teacher-chosen, optional).</summary>
    public string? CurriculumTitle { get; set; }
    /// <summary>Mô tả lộ trình học (optional).</summary>
    public string? CurriculumDescription { get; set; }
    /// <summary>false = draft (học viên KHÔNG thấy items); true = published (mặc định — giữ hành vi cũ).</summary>
    public bool CurriculumPublished { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
