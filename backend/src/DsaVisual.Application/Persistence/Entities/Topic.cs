namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Chủ đề (tối đa 2 cấp) — SDD §7.3.7.</summary>
public sealed class Topic
{
    public int Id { get; set; }
    public int? ParentId { get; set; }                               // FK → Topics.Id
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public int CreatedBy { get; set; }                               // FK → Users.Id
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
