using DsaVisual.Application.Common;
using DsaVisual.Application.Dtos;
using DsaVisual.Application.Persistence;
using DsaVisual.Application.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DsaVisual.Application.Services;

/// <summary>
/// FavoriteService thật theo API_REFERENCE.md §4.9 / SDD §7.3.11 (UNIQUE (UserId, SimulationKey)).
/// </summary>
public sealed class FavoriteService(
    AppDbContext db,
    IDateTimeProvider clock,
    ISimulationCatalogService catalog,
    ILogger<FavoriteService> logger) : IFavoriteService
{
    public async Task<Result<List<FavoriteDto>>> GetListAsync(int userId, CancellationToken ct)
    {
        var favorites = await db.Favorites.AsNoTracking()
            .Where(f => f.UserId == userId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(ct);

        var items = new List<FavoriteDto>();
        foreach (var favorite in favorites)
        {
            var meta = await catalog.GetByKeyAsync(favorite.SimulationKey, ct);
            items.Add(new FavoriteDto
            {
                Id = favorite.Id,
                SimulationKey = favorite.SimulationKey,
                Title = meta.IsSuccess ? meta.Value!.Title : null,
                DataStructure = meta.IsSuccess ? meta.Value!.DataStructure : null,
                Input = favorite.InputJson,
                CreatedAt = favorite.CreatedAt
            });
        }

        return Result<List<FavoriteDto>>.Ok(items);
    }

    public async Task<Result<FavoriteDto>> AddAsync(int userId, FavoriteUpsertRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.SimKey))
        {
            return Result<FavoriteDto>.Fail(ErrorCodes.SIMULATION_KEY_INVALID,
                "Khóa mô phỏng không tồn tại trong danh mục", new() { ["simKey"] = ["Khóa mô phỏng không được để trống"] });
        }

        var exists = await catalog.ExistsAsync(request.SimKey, ct);
        if (!exists)
        {
            return Result<FavoriteDto>.Fail(ErrorCodes.SIMULATION_KEY_INVALID,
                "Khóa mô phỏng không tồn tại trong danh mục", new() { ["simKey"] = ["Khóa mô phỏng không tồn tại trong danh mục"] });
        }

        var duplicate = await db.Favorites.AsNoTracking()
            .AnyAsync(f => f.UserId == userId && f.SimulationKey == request.SimKey, ct);
        if (duplicate)
        {
            return Result<FavoriteDto>.Fail(ErrorCodes.VALIDATION_FAILED,
                "Mô phỏng đã có trong danh sách yêu thích", new() { ["simKey"] = ["Mô phỏng đã có trong danh sách yêu thích"] });
        }

        var favorite = new Favorite
        {
            UserId = userId,
            SimulationKey = request.SimKey,
            InputJson = request.Input,
            CreatedAt = clock.UtcNow
        };
        db.Favorites.Add(favorite);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Favorite {FavoriteId} ({SimKey}) added by user {UserId}", favorite.Id, request.SimKey, userId);
        var meta = await catalog.GetByKeyAsync(request.SimKey, ct);
        return Result<FavoriteDto>.Ok(new FavoriteDto
        {
            Id = favorite.Id,
            SimulationKey = favorite.SimulationKey,
            Title = meta.IsSuccess ? meta.Value!.Title : null,
            DataStructure = meta.IsSuccess ? meta.Value!.DataStructure : null,
            Input = favorite.InputJson,
            CreatedAt = favorite.CreatedAt
        });
    }

    public async Task<Result> DeleteAsync(int userId, int id, CancellationToken ct)
    {
        var favorite = await db.Favorites.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (favorite is null)
        {
            return Result.Fail(ErrorCodes.NOT_FOUND, "Yêu thích không tồn tại");
        }

        if (favorite.UserId != userId)
        {
            return Result.Fail(ErrorCodes.FORBIDDEN, "Bạn không có quyền xóa yêu thích này");
        }

        db.Favorites.Remove(favorite);
        await db.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
