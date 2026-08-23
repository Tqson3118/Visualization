namespace DsaVisual.Application.Dtos;

/// <summary>Dòng Cheatsheet — GET /cheatsheet (FR-3.20, SDD §8.4A): bảng Big-O + deep-link mô phỏng.</summary>
public sealed class CheatsheetItemDto
{
    public string Key { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string DataStructure { get; set; } = string.Empty;
    public ComplexityDto Complexity { get; set; } = new();
    public string DeepLink { get; set; } = string.Empty;   // /simulations/{key}
}
