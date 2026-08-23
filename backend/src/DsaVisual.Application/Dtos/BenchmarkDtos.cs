namespace DsaVisual.Application.Dtos;

/// <summary>Chạy benchmark — POST /benchmarks/run (API_REFERENCE.md §4.14 example, FR-3.20b).
/// Đo thời gian chạy phía client sandbox (runMeasure — SDD §4.0.3); server lưu + fit lý thuyết.
/// Mở rộng so với tài liệu: client gửi kèm <c>results</c> (server không tự đo được).</summary>
public sealed class BenchmarkRequest
{
    public List<string> Keys { get; set; } = [];
    public List<int> Sizes { get; set; } = [];
    public string? Language { get; set; }          // "ts" | "js" ...
    public List<BenchmarkResultDto>? Results { get; set; }
}

/// <summary>Kết quả đo 1 thuật toán — API_REFERENCE.md §4.14 example.</summary>
public sealed class BenchmarkResultDto
{
    public string Key { get; set; } = string.Empty;
    public List<BenchmarkMeasurementDto> Measurements { get; set; } = [];
}

/// <summary>1 điểm đo tại n — API_REFERENCE.md §4.14 example.</summary>
public sealed class BenchmarkMeasurementDto
{
    public int N { get; set; }
    public double DurationMs { get; set; }
    public long Comparisons { get; set; }
    public long Swaps { get; set; }
}

/// <summary>Response benchmark — API_REFERENCE.md §4.14 example.</summary>
public sealed class BenchmarkRunResponse
{
    public List<BenchmarkResultDto> Results { get; set; } = [];
    public Dictionary<string, string> Fitted { get; set; } = [];
    public string Conclusion { get; set; } = string.Empty;
}
