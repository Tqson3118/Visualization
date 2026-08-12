namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Lớp học — SDD §7.3.16 (Module H). InviteCode UNIQUE (mã mời 6 ký tự).</summary>
public sealed class Class
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;           // UNIQUE
    public string? Semester { get; set; }
    public string? Description { get; set; }
    public int OwnerId { get; set; }                                 // FK → Users.Id (Teacher)
    public ClassStatus Status { get; set; } = ClassStatus.Open;
    public DateTime CreatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
