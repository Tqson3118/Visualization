using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;

namespace DsaVisual.Application.Services;

/// <summary>CRUD yêu thích mô phỏng — API_REFERENCE.md §4.9 (SDD §5.4).</summary>
public interface IFavoriteService
{
    Task<Result<List<FavoriteDto>>> GetListAsync(int userId, CancellationToken ct);
    Task<Result<FavoriteDto>> AddAsync(int userId, FavoriteUpsertRequest request, CancellationToken ct);
    Task<Result> DeleteAsync(int userId, int id, CancellationToken ct);
}
