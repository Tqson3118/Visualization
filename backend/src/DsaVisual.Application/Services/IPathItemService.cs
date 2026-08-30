using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

public interface IPathItemService
{
    /// <summary>Chỉ TEACHER sở hữu lộ trình (CreatedBy/AuthorId) hoặc ADMIN mới được xem cây nội dung.</summary>
    Task<Result<List<PathItemDto>>> GetTreeAsync(int userId, string role, int pathId, CancellationToken ct);
    Task<Result<PathItemDto>> GetItemDetailAsync(int userId, string role, int itemId, CancellationToken ct);
    /// <summary>Predicate phân quyền quản lý lộ trình: ADMIN toàn quyền; TEACHER chỉ khi là chủ sở hữu path.</summary>
    Task<bool> CanManagePathAsync(int userId, string role, int pathId, CancellationToken ct);
    Task<Result<PathItemDto>> CreateItemAsync(int userId, string role, int pathId, PathItemCreateRequest request, CancellationToken ct);
    Task<Result<PathItemDto>> UpdateItemAsync(int userId, string role, int itemId, PathItemUpdateRequest request, CancellationToken ct);
    Task<Result<PathItemDto>> MoveItemAsync(int userId, string role, int itemId, PathItemMoveRequest request, CancellationToken ct);
    Task<Result> DeleteItemAsync(int userId, string role, int itemId, CancellationToken ct);
}
