namespace DsaVisual.Application.Dtos;

/// <summary>Tham gia lớp bằng mã mời — POST /classes/{id}/join {inviteCode} (API_REFERENCE.md §4.11).</summary>
public sealed class JoinClassRequest
{
    public string InviteCode { get; set; } = string.Empty;
}

/// <summary>Thêm sinh viên theo email — POST /classes/{id}/members (API_REFERENCE.md §4.11).</summary>
public sealed class AddMemberRequest
{
    public string Email { get; set; } = string.Empty;
}
