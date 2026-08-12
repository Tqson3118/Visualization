namespace DsaVisual.Application.Dtos;

/// <summary>Phê duyệt/từ chối Teacher — POST /users/{id}/approve-teacher (API_REFERENCE.md §4.8, v2.8).</summary>
public sealed class ApproveTeacherRequest
{
    public bool Approve { get; set; }
    public string? Reason { get; set; }
}
