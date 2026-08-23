namespace DsaVisual.Application.Dtos;

/// <summary>Số liệu trang chủ — GET /public/site-info (API_REFERENCE.md §4.2).</summary>
public sealed class SiteInfoDto
{
    public int Structures { get; set; }
    public int Algorithms { get; set; }
    public int Lessons { get; set; }
}

/// <summary>FAQ — GET /public/faqs (API_REFERENCE.md §4.2).</summary>
public sealed class FaqDto
{
    public string Question { get; set; } = string.Empty;
    public string Answer { get; set; } = string.Empty;
}
