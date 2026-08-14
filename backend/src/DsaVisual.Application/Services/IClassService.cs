using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

/// <summary>
/// CRUD lớp, mã mời 6 ký tự, thêm/xóa sinh viên, gán nội dung + hạn nộp, báo cáo lớp (SDD §5.4 — Module H).
/// </summary>
public interface IClassService
{
    Task<Result<List<ClassDto>>> GetMyClassesAsync(int userId, string role, CancellationToken ct);
    Task<Result<ClassDto>> CreateAsync(int userId, ClassUpsertRequest request, CancellationToken ct);
    Task<Result<ClassDetailDto>> GetByIdAsync(int userId, string role, int id, CancellationToken ct);
    Task<Result<ClassDto>> UpdateAsync(int userId, string role, int id, ClassUpsertRequest request, CancellationToken ct);
    Task<Result> DeleteAsync(int userId, string role, int id, CancellationToken ct);
    Task<Result<ClassDetailDto>> JoinAsync(int userId, int id, JoinClassRequest request, CancellationToken ct);

    /// <summary>v2.15: tham gia lớp bằng mã mời (không cần biết classId trước) — POST /classes/join-by-code.</summary>
    Task<Result<ClassDetailDto>> JoinByCodeAsync(int userId, JoinClassByCodeRequest request, CancellationToken ct);
    Task<Result<ClassDetailDto>> AddMemberAsync(int userId, string role, int id, AddMemberRequest request, CancellationToken ct);
    Task<Result> RemoveMemberAsync(int userId, string role, int id, int memberUserId, CancellationToken ct);
    Task<Result<ClassDetailDto>> AddAssignmentAsync(int userId, string role, int id, ClassAssignmentUpsertRequest request, CancellationToken ct);
    Task<Result> UpdateAssignmentAsync(int userId, string role, int id, int assignId, ClassAssignmentUpdateRequest request, CancellationToken ct);
    Task<Result> RemoveAssignmentAsync(int userId, string role, int id, int assignId, CancellationToken ct);
    Task<Result<ClassReportDto>> GetReportAsync(int userId, string role, int id, CancellationToken ct);
    Task<Result<CsvFileDto>> ExportReportCsvAsync(int userId, string role, int id, CancellationToken ct);
}
