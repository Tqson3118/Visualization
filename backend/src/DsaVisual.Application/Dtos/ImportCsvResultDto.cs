namespace DsaVisual.Application.Dtos;

/// <summary>Kết quả import CSV — POST /exercises/import-csv (API_REFERENCE.md §4.6).</summary>
public sealed class ImportCsvResultDto
{
    public int Created { get; set; }
    public int Skipped { get; set; }
    public List<string> Errors { get; set; } = [];   // lỗi theo dòng: "Dòng 5: ..."
}
