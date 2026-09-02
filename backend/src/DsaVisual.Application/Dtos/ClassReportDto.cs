namespace DsaVisual.Application.Dtos;

/// <summary>Báo cáo lớp — GET /classes/{id}/report (API_REFERENCE.md §4.11, FR-8.4).</summary>
public sealed class ClassReportDto
{
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;
    public int TotalMembers { get; set; }
    public List<ClassReportAssignmentDto> Assignments { get; set; } = [];
    public List<LaggingLearnerDto> LaggingLearners { get; set; } = [];
}

/// <summary>Thống kê 1 bài gán — API_REFERENCE.md §4.11 example.</summary>
public sealed class ClassReportAssignmentDto
{
    public int AssignmentId { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime? DueAt { get; set; }
    public int OnTime { get; set; }
    public int Late { get; set; }
    public int NotSubmitted { get; set; }
    public double AvgScore { get; set; }
    /// <summary>Loại nội dung bài gán: "theory" (lý thuyết), "quiz" (trắc nghiệm), "code" (thực hành code/lab).</summary>
    public string ItemType { get; set; } = "theory";
}

/// <summary>Học viên chậm tiến độ — API_REFERENCE.md §4.11 example.</summary>
public sealed class LaggingLearnerDto
{
    public int UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public int MissingCount { get; set; }
}
