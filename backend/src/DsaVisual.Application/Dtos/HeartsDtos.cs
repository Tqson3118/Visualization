namespace DsaVisual.Application.Dtos;

/// <summary>Trạng thái tim — API_REFERENCE.md §3.12/§4.14 (FR-10.1).</summary>
public sealed class HeartsStatusDto
{
    public int Hearts { get; set; }
    public int HeartsMax { get; set; }
    public DateTime LastHeartAt { get; set; }
    public int NextHeartInSeconds { get; set; }
    public int Gems { get; set; }
}

/// <summary>Phiên làm bài trong node — API_REFERENCE.md §3.12/§4.14.</summary>
public sealed class NodeSessionDto
{
    public int Id { get; set; }
    public int NodeId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public int? Stage { get; set; }
    public int? StepIndex { get; set; }
}

/// <summary>Body tùy chọn khi enter node — lưu điểm dừng để resume (SDD §7.3.29).</summary>
public sealed class NodeEnterRequest
{
    public int? Stage { get; set; }
    public int? StepIndex { get; set; }
}

/// <summary>Kết quả enter node — API_REFERENCE.md §4.14 example (v2.5).</summary>
public sealed class NodeEnterResultDto
{
    public NodeSessionDto Session { get; set; } = new();
    public int HeartsLeft { get; set; }
}
