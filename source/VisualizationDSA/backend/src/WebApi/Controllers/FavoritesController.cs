using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VisualizationDSA.Application.Interfaces;
using VisualizationDSA.Domain.Entities;
using VisualizationDSA.WebApi.Filters;

namespace VisualizationDSA.WebApi.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    [Route("api/v{version:apiVersion}/favorites")]
    [RequireJwtRole]
    public class FavoritesController : ControllerBase
    {
        private readonly IApplicationDbContext _ctx;

        public FavoritesController(IApplicationDbContext ctx)
        {
            _ctx = ctx;
        }

        private Guid Uid() =>
            Guid.Parse(JwtHelper.ExtractSubFromToken(Request) ?? Guid.Empty.ToString());

        [HttpGet]
        public async Task<IActionResult> GetFavorites()
        {
            var uid = Uid();
            var list = await _ctx.Set<Favorite>()
                .AsNoTracking()
                .Where(f => f.UserId == uid)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();

            var dtos = list.Select(f => new
            {
                id = Math.Abs(f.Id.GetHashCode()),
                favoriteId = f.Id.ToString(),
                simKey = f.SimulationKey,
                title = FormatTitle(f.SimulationKey),
                input = string.IsNullOrWhiteSpace(f.InputJson) ? null : TryDeserialize(f.InputJson),
                createdAt = f.CreatedAt.ToString("o")
            });

            return Ok(dtos);
        }

        [HttpPost]
        public async Task<IActionResult> AddFavorite([FromBody] AddFavoriteRequest req)
        {
            var uid = Uid();
            if (string.IsNullOrWhiteSpace(req?.SimKey))
                return BadRequest("Simulation key is required.");

            var existing = await _ctx.Set<Favorite>()
                .FirstOrDefaultAsync(f => f.UserId == uid && f.SimulationKey == req.SimKey);

            if (existing != null)
            {
                return Ok(new
                {
                    id = Math.Abs(existing.Id.GetHashCode()),
                    favoriteId = existing.Id.ToString(),
                    simKey = existing.SimulationKey,
                    title = FormatTitle(existing.SimulationKey),
                    input = string.IsNullOrWhiteSpace(existing.InputJson) ? null : TryDeserialize(existing.InputJson),
                    createdAt = existing.CreatedAt.ToString("o")
                });
            }

            var inputJson = req.Input != null ? JsonSerializer.Serialize(req.Input) : null;
            var fav = new Favorite(uid, req.SimKey, inputJson);
            await _ctx.Set<Favorite>().AddAsync(fav);
            await _ctx.SaveChangesAsync(default);

            return Ok(new
            {
                id = Math.Abs(fav.Id.GetHashCode()),
                favoriteId = fav.Id.ToString(),
                simKey = fav.SimulationKey,
                title = FormatTitle(fav.SimulationKey),
                input = req.Input,
                createdAt = fav.CreatedAt.ToString("o")
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveFavorite(string id)
        {
            var uid = Uid();
            Favorite? fav = null;
            if (Guid.TryParse(id, out var gid))
            {
                fav = await _ctx.Set<Favorite>().FirstOrDefaultAsync(f => f.Id == gid && f.UserId == uid);
            }
            else if (int.TryParse(id, out var numericId))
            {
                var userFavs = await _ctx.Set<Favorite>().Where(f => f.UserId == uid).ToListAsync();
                fav = userFavs.FirstOrDefault(f => Math.Abs(f.Id.GetHashCode()) == numericId);
            }

            if (fav != null)
            {
                _ctx.Set<Favorite>().Remove(fav);
                await _ctx.SaveChangesAsync(default);
            }

            return NoContent();
        }

        private static string FormatTitle(string simKey)
        {
            if (string.IsNullOrWhiteSpace(simKey)) return string.Empty;
            var parts = simKey.Split('.', StringSplitOptions.RemoveEmptyEntries);
            return string.Join(" ", parts.Select(p => char.ToUpperInvariant(p[0]) + p[1..]));
        }

        private static object? TryDeserialize(string json)
        {
            try { return JsonSerializer.Deserialize<object>(json); }
            catch { return json; }
        }
    }

    public class AddFavoriteRequest
    {
        public string SimKey { get; set; } = string.Empty;
        public object? Input { get; set; }
    }
}
