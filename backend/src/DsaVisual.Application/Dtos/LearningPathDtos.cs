namespace DsaVisual.Application.Dtos;

/// <summary>Bản đồ learning path — API_REFERENCE.md §4.14 example (FR-2.10).</summary>
public sealed class LearningPathMapDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int ProgressPct { get; set; }
    public List<LearningPathNodeDto> Nodes { get; set; } = [];
}

/// <summary>Node trong bản đồ — status: locked/active/passed (API_REFERENCE.md §4.14).</summary>
public sealed class LearningPathNodeDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string Status { get; set; } = "locked";
    public int Stars { get; set; }
    public int NodeScore { get; set; }
}
