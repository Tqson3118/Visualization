namespace DsaVisual.Application.Dtos;

/// <summary>Chi tiết lớp — GET /classes/{id} (API_REFERENCE.md §4.11).</summary>
public sealed class ClassDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public string? Semester { get; set; }
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string Status { get; set; } = "open";
    public DateTime CreatedAt { get; set; }
    public List<ClassMemberDto> Members { get; set; } = [];
    public List<ClassAssignmentDto> Assignments { get; set; } = [];
}

/// <summary>Thành viên lớp — API_REFERENCE.md §4.11.</summary>
public sealed class ClassMemberDto
{
    public int UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
}

/// <summary>Bài gán cho lớp — API_REFERENCE.md §4.11.</summary>
public sealed class ClassAssignmentDto
{
    public int Id { get; set; }
    public int? LessonId { get; set; }
    public int? ExerciseId { get; set; }
    public string? Title { get; set; }
    public DateTime? DueAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
