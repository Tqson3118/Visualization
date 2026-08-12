namespace DsaVisual.Application.Persistence.Entities;

/// <summary>Thành viên lớp — SDD §7.3.17. UNIQUE (ClassId, UserId).</summary>
public sealed class ClassMember
{
    public int Id { get; set; }
    public int ClassId { get; set; }                                 // FK cascade
    public int UserId { get; set; }
    public DateTime JoinedAt { get; set; }
}
