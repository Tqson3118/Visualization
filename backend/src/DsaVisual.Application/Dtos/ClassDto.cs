namespace DsaVisual.Application.Dtos;

/// <summary>Lớp học trong danh sách — API_REFERENCE.md §4.11.</summary>
public sealed class ClassDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    public string? Semester { get; set; }
    public string? Description { get; set; }
    public int OwnerId { get; set; }
    public string Status { get; set; } = "open";
    public int MemberCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Tạo/sửa lớp — POST/PUT /classes (API_REFERENCE.md §4.11).</summary>
public sealed class ClassUpsertRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Semester { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }          // open/closed
    public int? OwnerId { get; set; }            // Admin chuyển quyền sở hữu (v2.8)
}
