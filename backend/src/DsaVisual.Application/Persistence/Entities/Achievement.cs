namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Thành tích định nghĩa — SDD §7.3.19 (FR-5.5). Code UNIQUE (first-lesson, streak-7, sort-master...).</summary>
public sealed class Achievement
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;                 // UNIQUE
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string ConditionJson { get; set; } = string.Empty;        // {type:"count", key:"simulations", min:100}...
    public int SortOrder { get; set; }
}
