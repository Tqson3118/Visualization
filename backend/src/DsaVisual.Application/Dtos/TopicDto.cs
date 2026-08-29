namespace DsaVisual.Application.Dtos;

/// <summary>Chủ đề trong cây 2 cấp — API_REFERENCE.md §4.3.</summary>
public sealed class TopicDto
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public int CreatedBy { get; set; }
    public List<TopicDto> Children { get; set; } = [];
}
