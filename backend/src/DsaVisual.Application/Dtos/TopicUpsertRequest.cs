namespace DsaVisual.Application.Dtos;

/// <summary>Tạo/sửa chủ đề — POST/PUT /topics (API_REFERENCE.md §4.3).</summary>
public sealed class TopicUpsertRequest
{
    public int? ParentId { get; set; }           // null = chủ đề gốc
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}
