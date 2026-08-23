namespace DsaVisual.Application.Dtos;

/// <summary>File CSV để Controller trả về qua File() — UTF-8 BOM, tên file chuẩn (API_REFERENCE.md §6.3).</summary>
public sealed class CsvFileDto
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = [];
    public string ContentType { get; set; } = "text/csv; charset=utf-8";
}
