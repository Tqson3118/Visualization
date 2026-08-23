namespace DsaVisual.Application.Dtos;

/// <summary>Metadata mô phỏng — API_REFERENCE.md §3.6.</summary>
public sealed class SimulationMetaDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string DataStructure { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;   // structure/algorithm
    public string Level { get; set; } = string.Empty;      // basic/advanced
    public ComplexityDto Complexity { get; set; } = new();
    public List<string> Tags { get; set; } = [];
    public bool DemoAllowed { get; set; }
}

/// <summary>Độ phức tạp Big-O — API_REFERENCE.md §3.6.</summary>
public sealed class ComplexityDto
{
    public string Best { get; set; } = string.Empty;
    public string Average { get; set; } = string.Empty;
    public string Worst { get; set; } = string.Empty;
    public string Space { get; set; } = string.Empty;
}
