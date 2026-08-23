using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

/// <summary>
/// Quản lý người dùng (Admin): danh sách, khóa/mở, đổi vai trò, phê duyệt Teacher, ẩn danh hóa (SDD §5.4).
/// </summary>
public interface IUserService
{
    Task<Result<PagedResponse<AdminUserDto>>> GetListAsync(string? role, string? status, string? q, int page, int pageSize, CancellationToken ct);
    Task<Result<AdminUserDto>> GetByIdAsync(int id, CancellationToken ct);
    Task<Result<AdminUserDto>> CreateUserAsync(int actorId, bool actorIsPrimaryAdmin, AdminCreateUserRequest request, CancellationToken ct);
    Task<Result<AdminUserDto>> UpdateUserAsync(int actorId, bool actorIsPrimaryAdmin, int id, AdminUpdateUserRequest request, CancellationToken ct);
    Task<Result> SetStatusAsync(int actorId, bool actorIsPrimaryAdmin, int id, bool isActive, CancellationToken ct);
    Task<Result> SetRoleAsync(int actorId, bool actorIsPrimaryAdmin, int id, string role, CancellationToken ct);
    Task<Result> ApproveTeacherAsync(int actorId, bool actorIsPrimaryAdmin, int id, ApproveTeacherRequest request, CancellationToken ct);
    Task<Result> ResetPasswordAsync(int actorId, bool actorIsPrimaryAdmin, int id, AdminResetPasswordRequest request, CancellationToken ct);
    Task<Result> DeleteAsync(int actorId, bool actorIsPrimaryAdmin, int id, CancellationToken ct);
}
